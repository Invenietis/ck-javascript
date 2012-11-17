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

        public Expr Analyse( JSTokeniser p )
        {
            _parser = p;
            return Expression( 0 );
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
            if( _parser.IsNumber ) return HandleNumber();
            if( _parser.IsString ) return new ConstantExpr( _parser.Location, _parser.ReadString() );
            if( _parser.IsUnaryOperatorExtended || _parser.CurrentToken == JSTokeniserToken.Minus ) return HandleUnaryExpr();
            if( _parser.IsIdentifier ) return HandleIdentifier();
            if( _parser.Match( JSTokeniserToken.OpenPar ) )
            {
                SourceLocation location = _parser.PrevNonCommentLocation;
                Expr e = Expression( 0 );
                return _parser.Match( JSTokeniserToken.ClosePar ) ? e : new SyntaxErrorExpr( _parser.Location, "Expected ) opened at {0}.", location );
            }
            return new SyntaxErrorExpr( _parser.Location, "Syntax Error." );
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

        Expr HandleMember( Expr left )
        {
            string id = _parser.ReadIdentifier();
            return new AccessorMemberExpr( _parser.PrevNonCommentLocation, left, id );
        }

        Expr HandleIndexer( Expr left )
        {
            SourceLocation loc = _parser.PrevNonCommentLocation;
            Expr i = Expression( 0 );
            if( i is SyntaxErrorExpr ) return i;
            if( !_parser.Match( JSTokeniserToken.CloseBracket ) )
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
            var arguments = parameters != null ? parameters.ToReadOnlyList() : ReadOnlyListEmpty<Expr>.Empty;
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
            
            return new AccessorMemberExpr( _parser.PrevNonCommentLocation, _scope.Find( id ) ?? SyntaxErrorExpr.ReferenceErrorExpr, id );
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
