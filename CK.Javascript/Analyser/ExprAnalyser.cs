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
        ISyntaxicScope _scope;

        public ExprAnalyser( ISyntaxicScope scope )
        {
            if( scope == null ) throw new ArgumentNullException( "scope" );
            _scope = scope;
        }

        public Expr Analyse( JSTokeniser p, bool allowBlock = true )
        {
            _parser = p;
            var e = Expression( 0 );
            _parser.Match( JSTokeniserToken.SemiColon );
            if( !allowBlock ) return e;
            return HandleBlock( e );
        }

        public static Expr AnalyseString( string s )
        {
            ExprAnalyser a = new ExprAnalyser( new StaticSyntaxicScope() );
            return a.Analyse( new JSTokeniser( s ) );
        }

        public Expr Expression( int rightBindingPower )
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

        protected virtual Expr HandleNud()
        {
            Debug.Assert( !_parser.IsErrorOrEndOfInput );
            while( _parser.Match( JSTokeniserToken.SemiColon ) ) ;
            if( _parser.IsNumber ) return HandleNumber();
            if( _parser.IsString ) return new ConstantExpr( _parser.Location, _parser.ReadString() );
            if( _parser.IsUnaryOperatorExtended || _parser.CurrentToken == JSTokeniserToken.Minus ) return HandleUnaryExpr();
            if( _parser.IsIdentifier ) return HandleIdentifier();
            if( _parser.Match( JSTokeniserToken.OpenCurly ) ) return HandleBlock();
            if( _parser.MatchIdentifier( "if" ) ) return HandleIf();
            if( _parser.Match( JSTokeniserToken.OpenPar ) )
            {
                SourceLocation location = _parser.PrevNonCommentLocation;
                Expr e = Expression( 0 );
                if( e is SyntaxErrorExpr ) return e;
                return _parser.Match( JSTokeniserToken.ClosePar ) ? e : new SyntaxErrorExpr( _parser.Location, "Expected ')' opened at {0}.", location );
            }
            return new SyntaxErrorExpr( _parser.Location, "Syntax Error." );
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

        Expr HandleStatement()
        {
            if( !_parser.Match( JSTokeniserToken.OpenCurly ) ) return HandleBlock();
            return Expression( 0 );
        }

        Expr HandleBlock( Expr first = null )
        {
            SourceLocation location = _parser.PrevNonCommentLocation;
            List<Expr> statements = new List<Expr>();
            if( first != null ) statements.Add( first );
            while( (first == null && !_parser.Match( JSTokeniserToken.CloseCurly )) || !(first == null || _parser.IsEndOfInput) )
            {
                Expr e = Expression( 0 );
                _parser.Match( JSTokeniserToken.SemiColon );
                statements.Add( e );
                if( e is SyntaxErrorExpr ) break; 
            }
            if( statements.Count == 1 ) return statements[0];
            return new BlockExpr( location, statements != null ? (IReadOnlyList<Expr>)statements.ToArray() : CKReadOnlyListEmpty<Expr>.Empty );
        }

        protected virtual Expr HandleLed( Expr left )
        {
            if( _parser.IsBinaryOperator || _parser.IsCompareOperator ) return HandleBinaryExpr( left );
            if( _parser.IsLogical ) return HandleLogicalExpr( left );
            if( _parser.Match( JSTokeniserToken.Dot ) ) return HandleMember( left );
            if( _parser.Match( JSTokeniserToken.OpenSquare ) ) return HandleIndexer( left );
            if( _parser.Match( JSTokeniserToken.OpenPar ) ) return HandleCall( left );
            if( _parser.Match( JSTokeniserToken.QuestionMark ) ) return HandleTernaryConditional( left );
            return new SyntaxErrorExpr( _parser.Location, "Syntax Error." );
        }

        //Expr HandleStatementTerminator( Expr left )
        //{
        //    if( left is StatementExpr ) return left;
        //    return new StatementExpr( _parser.PrevNonCommentLocation, left );
        //}

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

        Expr HandleNumber()
        {
            Debug.Assert( _parser.IsNumber );
            return new ConstantExpr( _parser.Location, _parser.ReadDouble() );
        }

        Expr HandleIdentifier()
        {
            string id = _parser.ReadIdentifier();
            if( id == "null" ) return new ConstantExpr( _parser.PrevNonCommentLocation, null );
            if( id == "true" ) return new ConstantExpr( _parser.PrevNonCommentLocation, true );
            if( id == "false" ) return new ConstantExpr( _parser.PrevNonCommentLocation, false );
            
            return new AccessorMemberExpr( _parser.PrevNonCommentLocation, _scope.Find( id ), id );
        }

        Expr HandleUnaryExpr()
        {
            _parser.Forward();
            // Unary operators are JSParserToken.OpLevel14, except Minus that is classified as a binary operator and is associated to JSParserToken.OpLevel12.
            return new UnaryExpr( _parser.PrevNonCommentLocation, _parser.PrevNonCommentToken, Expression( JSTokeniser.PrecedenceLevel( JSTokeniserToken.OpLevel14 ) ) );
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
