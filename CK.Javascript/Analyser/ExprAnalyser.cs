#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Javascript\ExprAnalyser.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using CK.Core;

namespace CK.Javascript
{
    public class ExprAnalyser
    {
        static readonly int _questionMarkPrecedenceLevel = JSTokeniser.PrecedenceLevel( JSTokeniserToken.QuestionMark );

        JSTokeniser _parser;
        StaticScope _scope;

        /// <summary>
        /// Configuration for an <see cref="ExprAnalyser"/>.
        /// </summary>
        public class Config
        {
            /// <summary>
            /// Initalizes a default configuration.
            /// </summary>
            public Config()
            {
                AllowMasking = true;
            }

            /// <summary>
            /// Gets or sets whether a global scope is opened.
            /// </summary>
            public bool GlobalScope { get; set; }

            /// <summary>
            /// Gets or sets whether masking is allowed (like in Javascript). 
            /// When masking is disallowed (like in C#), registering new entries returns a <see cref="SyntaxErrorExpr"/>
            /// instead of the registered expression.
            /// Defaults to true (javascript mode).
            /// </summary>
            public bool AllowMasking { get; set; }

            /// <summary>
            /// Gets or sets whether redefinition of a name in the same scope is possible. 
            /// This is allowed in javascript even with "use strict" but here it defaults to false since I consider this a dangerous and useless feature.
            /// </summary>
            public bool AllowLocalRedefinition { get; set; }

        }

        static readonly Config _emptyConfig = new Config();

        public ExprAnalyser( Config configuration = null )
        {
            if( configuration == null ) configuration = _emptyConfig;
            _scope = new StaticScope( configuration.GlobalScope, configuration.AllowMasking, configuration.AllowLocalRedefinition );
        }

        /// <summary>
        /// Analyses the tokens to produce an AST of Expr.
        /// When <paramref name="allowGlobalUse"/> is true and <see cref="Config.GlobalScope"/> is true, the top-level declarations
        /// go into the global scope.
        /// </summary>
        /// <param name="p">Tokeinzer to analyse.</param>
        /// <param name="allowGlobalUse">False to scope declarations to this analysis.</param>
        /// <returns>The AST (that may be a <see cref="SyntaxErrorExpr"/> or contains such errors).</returns>
        public Expr Analyse( JSTokeniser p, bool allowGlobalUse = true )
        {
            _parser = p;
            if( !(allowGlobalUse && _scope.GlobalScope) ) _scope.OpenScope();
            var e = Expression( 0 );
            _parser.Match( JSTokeniserToken.SemiColon );
            return HandleBlock( e );
        }

        public static Expr AnalyseString( string s, Config configuration = null)
        {
            ExprAnalyser a = new ExprAnalyser( configuration );
            return a.Analyse( new JSTokeniser( s ) );
        }

        Expr Expression( int rightBindingPower )
        {
            if( _parser.IsErrorOrEndOfInput )
            {
                return new SyntaxErrorExpr( _parser.Location, "Error: " + _parser.ErrorCode.ToString() );
            }
            Expr left = HandleNud();
            while( !(left is SyntaxErrorExpr) && rightBindingPower < _parser.CurrentPrecedenceLevel )
            {
                left = HandleLed( left );
            }
            return left;
        }

        Expr HandleNud()
        {
            Debug.Assert( !_parser.IsErrorOrEndOfInput );
            if( _parser.IsNumber ) return new ConstantExpr( _parser.Location, _parser.ReadDouble() );
            if( _parser.IsString ) return new ConstantExpr( _parser.Location, _parser.ReadString() );
            if( _parser.IsUnaryOperatorExtended || _parser.CurrentToken == JSTokeniserToken.Minus ) return HandleUnaryExpr();
            if( _parser.IsIdentifier )
            {
                if( _parser.MatchIdentifier( "if" ) ) return HandleIf();
                if( _parser.MatchIdentifier( "var" ) ) return HandleVar();
                if( _parser.MatchIdentifier( "while" ) ) return HandleWhile();
                if( _parser.MatchIdentifier( "break" ) ) return new BreakOrReturnExpr( _parser.PrevNonCommentLocation );
                return HandleIdentifier();
            }
            if( _parser.Match( JSTokeniserToken.OpenCurly ) ) return HandleBlock();
            if( _parser.Match( JSTokeniserToken.OpenPar ) )
            {
                SourceLocation location = _parser.PrevNonCommentLocation;
                Expr e = Expression( 0 );
                if( e is SyntaxErrorExpr ) return e;
                return _parser.Match( JSTokeniserToken.ClosePar ) ? e : new SyntaxErrorExpr( _parser.Location, "Expected ')' opened at {0}.", location );
            }
            if( _parser.Match( JSTokeniserToken.SemiColon ) ) return NopExpr.Default;
            return new SyntaxErrorExpr( _parser.Location, "Syntax Error." );
        }

        Expr HandleLed( Expr left )
        {
            if( _parser.IsBinaryOperator || _parser.IsCompareOperator ) return HandleBinaryExpr( left );
            if( _parser.IsLogical ) return HandleLogicalExpr( left );
            if( _parser.Match( JSTokeniserToken.Dot ) ) return HandleMember( left );
            if( _parser.Match( JSTokeniserToken.QuestionMark ) ) return HandleTernaryConditional( left );
            if( _parser.Match( JSTokeniserToken.OpenPar ) ) return HandleCall( left );
            if( _parser.Match( JSTokeniserToken.OpenSquare ) ) return HandleIndexer( left );
            if( _parser.IsAssignOperator ) return HandleAssign( left );
            if( _parser.IsUnaryOperator 
                && ( _parser.CurrentToken == JSTokeniserToken.PlusPlus 
                     || _parser.CurrentToken == JSTokeniserToken.MinusMinus ) ) return HandlePostIncDec( left );
            return new SyntaxErrorExpr( _parser.Location, "Syntax Error." );
        }

        private Expr HandlePostIncDec( Expr left )
        {
            var loc = _parser.Location;
            var t = _parser.CurrentToken;
            _parser.Forward();
            AccessorExpr a = left as AccessorExpr;
            if( a == null ) return new SyntaxErrorExpr( loc, "invalid increment operand." );
            return new PrePostIncDecExpr( loc, a, t == JSTokeniserToken.PlusPlus, false );
        }

        Expr HandleVar()
        {
            SourceLocation location = _parser.PrevNonCommentLocation;
            var multi = new List<Expr>();
            do
            {
                string name = _parser.ReadIdentifier();
                if( name == null ) return new SyntaxErrorExpr( location, "Expected identifier (variable name)." );
                Expr e = _scope.Declare( name, new AccessorDeclVarExpr( location, name ) );
                if( _parser.Match( JSTokeniserToken.Assign ) ) e = HandleAssign( e, true );
                location = _parser.Location;
                multi.Add( e );
                if( e is SyntaxErrorExpr ) break;
            }
            while( _parser.Match( JSTokeniserToken.Comma ) );
            if( multi.Count == 1 ) return multi[0];
            return new ListOfExpr( multi );
        }

        Expr HandleAssign( Expr left, bool pureAssign = false )
        {
            var location = _parser.Location;
            AccessorExpr a = left as AccessorExpr;
            if( a == null ) return new SyntaxErrorExpr( location, "Invalid assignment left-hand side." );
            if( pureAssign || _parser.Match( JSTokeniserToken.Assign ) )
            {
                return new AssignExpr( location, a, Expression( JSTokeniser.PrecedenceLevel( JSTokeniserToken.Comma ) ) );
            }
            JSTokeniserToken binaryTokenType = JSTokeniser.FromAssignOperatorToBinary( _parser.CurrentToken );
            _parser.Forward();
            return new AssignExpr( location, a, new BinaryExpr( location, left, binaryTokenType, Expression( 0 ) ) );
        }

        Expr HandleIf()
        {
            SourceLocation location = _parser.PrevNonCommentLocation;
            if( !_parser.Match( JSTokeniserToken.OpenPar ) ) return new SyntaxErrorExpr( _parser.Location, "Expected '('." );
            Expr c = Expression( 0 );
            if( !_parser.Match( JSTokeniserToken.ClosePar ) ) return new SyntaxErrorExpr( _parser.Location, "Expected ')'." );
            Expr whenTrue = HandleStatement();
            Expr whenFalse = null;
            if( _parser.MatchIdentifier( "else" ) ) whenFalse = HandleStatement();
            return new IfExpr( location, false, c, whenTrue, whenFalse );
        }

        Expr HandleWhile()
        {
            SourceLocation location = _parser.PrevNonCommentLocation;
            if( !_parser.Match( JSTokeniserToken.OpenPar ) ) return new SyntaxErrorExpr( _parser.Location, "Expected '('." );
            Expr c = Expression( 0 );
            if( !_parser.Match( JSTokeniserToken.ClosePar ) ) return new SyntaxErrorExpr( _parser.Location, "Expected ')'." );
            Expr code = HandleStatement();
            return new WhileExpr( location, c, code );
        }

        Expr HandleStatement()
        {
            if( _parser.Match( JSTokeniserToken.OpenCurly ) ) return HandleBlock();
            return Expression( 0 );
        }

        Expr HandleBlock( Expr first = null )
        {
            if( first == null ) _scope.OpenScope();
            List<Expr> statements = new List<Expr>();
            if( first != null && first != NopExpr.Default ) statements.Add( first );
            while( (first == null && !_parser.Match( JSTokeniserToken.CloseCurly )) || !(first == null || _parser.IsEndOfInput) )
            {
                Expr e = Expression( 0 );
                _parser.Match( JSTokeniserToken.SemiColon );
                if( e != NopExpr.Default ) statements.Add( e );
                if( e is SyntaxErrorExpr ) break; 
            }
            if( statements.Count == 0 ) return NopExpr.Default;
            var locals = _scope.CloseScope();
            if( statements.Count == 1 ) return statements[0];
            return new BlockExpr( statements.ToArray(), locals );
        }

        Expr HandleMember( Expr left )
        {
            string id = _parser.ReadIdentifier();
            if( id == null ) return new SyntaxErrorExpr( _parser.Location, "Identifier expected." );
            return new AccessorMemberExpr( _parser.PrevNonCommentLocation, left, id );
        }

        Expr HandleIndexer( Expr left )
        {
            SourceLocation loc = _parser.PrevNonCommentLocation;
            Expr i = Expression( 0 );
            if( i is SyntaxErrorExpr ) return i;
            if( !_parser.Match( JSTokeniserToken.CloseSquare ) )
            {
                return new SyntaxErrorExpr( _parser.Location, "Expected ] opened at {0}.", loc );
            }
            return new AccessorIndexerExpr( loc, left, i );
        }

        Expr HandleCall( Expr left )
        {
            SourceLocation loc = _parser.PrevNonCommentLocation;
            IList<Expr> parameters = null;
            if( !_parser.Match( JSTokeniserToken.ClosePar ) )
            {
                for( ; ; )
                {
                    Debug.Assert( JSTokeniser.PrecedenceLevel( JSTokeniserToken.Comma ) == 2 );
                    Expr e = Expression( 2 );
                    if( e is SyntaxErrorExpr ) return e;

                    if( parameters == null ) parameters = new List<Expr>();
                    parameters.Add( e );

                    if( _parser.Match( JSTokeniserToken.ClosePar ) ) break;
                    if( !_parser.Match( JSTokeniserToken.Comma ) )
                    {
                        return new SyntaxErrorExpr( _parser.Location, "Expected ) opened at {0}.", loc );
                    }
                }
            }
            var arguments = parameters != null ? parameters.ToReadOnlyList() : CKReadOnlyListEmpty<Expr>.Empty;
            return new AccessorCallExpr( loc, left, arguments );
        }

        Expr HandleIdentifier()
        {
            string id = _parser.ReadIdentifier();
            if( id == "null" ) return new ConstantExpr( _parser.PrevNonCommentLocation, null );
            if( id == "true" ) return new ConstantExpr( _parser.PrevNonCommentLocation, true );
            if( id == "false" ) return new ConstantExpr( _parser.PrevNonCommentLocation, false );
            if( id == "undefined" ) return ConstantExpr.UndefinedExpr;
            var bound = _scope.Find( id );
            return bound != null ? bound : new AccessorMemberExpr( _parser.PrevNonCommentLocation, null, id );
        }

        Expr HandleUnaryExpr()
        {
            var loc = _parser.Location;
            var t = _parser.CurrentToken;
            _parser.Forward();
            // Unary operators are JSParserToken.OpLevel14, except Minus that is classified as a binary operator and is associated to JSParserToken.OpLevel12.
            var right = Expression( JSTokeniser.PrecedenceLevel( JSTokeniserToken.OpLevel14 ) );
            if( t == JSTokeniserToken.PlusPlus || t == JSTokeniserToken.MinusMinus )
            {
                AccessorExpr a = right as AccessorExpr;
                if( a == null ) return new SyntaxErrorExpr( loc, "invalid increment operand." );
                return new PrePostIncDecExpr( loc, a, t == JSTokeniserToken.PlusPlus, true );
            }
            return new UnaryExpr( loc, t, right );
        }

        Expr HandleBinaryExpr( Expr left )
        {
            _parser.Forward();
            return new BinaryExpr( _parser.PrevNonCommentLocation, left, _parser.PrevNonCommentToken, Expression( JSTokeniser.PrecedenceLevel( _parser.PrevNonCommentToken ) ) );
        }

        Expr HandleLogicalExpr( Expr left )
        {
            _parser.Forward();
            // Right associative operators to support short-circuit (hence the -1 on the level).
            return new BinaryExpr( _parser.PrevNonCommentLocation, left, _parser.PrevNonCommentToken, Expression( JSTokeniser.PrecedenceLevel( _parser.PrevNonCommentToken ) - 1 ) );
        }

        Expr HandleTernaryConditional( Expr left )
        {
            SourceLocation qLoc = _parser.PrevNonCommentLocation;
            Expr whenTrue = Expression( _questionMarkPrecedenceLevel );
            if( whenTrue is SyntaxErrorExpr ) return whenTrue;
            if( !_parser.Match( JSTokeniserToken.Colon ) ) return new SyntaxErrorExpr( _parser.Location, "Expected colon (:) after ? at {0}.", qLoc );
            return new IfExpr( qLoc, true, left, whenTrue, Expression( _questionMarkPrecedenceLevel ) );
        }

    }

}
