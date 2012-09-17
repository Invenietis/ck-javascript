using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CK.Javascript
{
    public class ToStringVisitor : ExprVisitor
    {
        static public readonly string DefaultExprPrefix = "‹";
        static public readonly string DefaultExprSuffix = "›";
        
        StringBuilder _b;
        string _exprPrefix;
        string _exprSuffix;

        public ToStringVisitor( StringBuilder b = null, string exprPrefix = null, string exprSuffix = null )
        {
            _exprPrefix = exprPrefix ?? DefaultExprPrefix;
            _exprSuffix = exprSuffix ?? DefaultExprSuffix;
            _b = b ?? new StringBuilder();
        }

        static public string ToString( Expr e, string exprPrefix = null, string exprSuffix = null )
        {
            var v = new ToStringVisitor( new StringBuilder(), exprPrefix, exprSuffix );
            v.VisitExpr( e );
            return v.ToString();
        }

        public override Expr Visit( AccessorMemberExpr e )
        {
            _b.Append( _exprPrefix );
            if( e.IsUnbound )
            {
                _b.Append( StaticSyntaxicScope.RootScopeName );
            }
            else VisitExpr( e.Left );
            _b.Append( '.' ).Append( e.Name );
            _b.Append( _exprSuffix );
            return e;
        }

        public override Expr Visit( AccessorIndexerExpr e )
        {
            _b.Append( _exprPrefix );
            VisitExpr( e.Left );
            _b.Append( '[' );
            VisitExpr( e.Index );
            _b.Append( ']' );
            _b.Append( _exprSuffix );
            return e;
        }

        public override Expr Visit( AccessorCallExpr e )
        {
            _b.Append( _exprPrefix );
            VisitExpr( e.Left );
            _b.Append( '(' );
            e.CallArguments.Select( ( p, i ) => 
            { 
                if( i > 0 ) _b.Append( ',' ); 
                return VisitExpr( p ); 
            }).LastOrDefault();
            _b.Append( ')' );
            _b.Append( _exprSuffix );
            return e;
        }

        public override Expr Visit( BinaryExpr e )
        {
            _b.Append( _exprPrefix );
            VisitExpr( e.Left );
            _b.Append( JSParser.Explain( e.BinaryOperatorToken ) );
            VisitExpr( e.Right );
            _b.Append( _exprSuffix );
            return e;
        }

        public override Expr Visit( ConstantExpr e )
        {
            _b.Append( _exprPrefix );
            _b.Append( e.Value );
            _b.Append( _exprSuffix );
            return e;
        }

        public override Expr Visit( IfExpr e )
        {
            _b.Append( _exprPrefix );
            if( !e.IsTernaryOperator ) _b.Append( "if" );
            VisitExpr( e.Condition );
            if( e.IsTernaryOperator ) _b.Append( '?' );
            VisitExpr( e.WhenTrue );
            if( e.IsTernaryOperator )
            {
                _b.Append( ':' );
                VisitExpr( e.WhenFalse );
            }
            else if( e.WhenFalse != null )
            {
                _b.Append( "else" );
                VisitExpr( e.WhenFalse );
            }
            _b.Append( _exprSuffix );
            return e;
        }

        public override Expr Visit( UnaryExpr e )
        {
            _b.Append( _exprPrefix );
            _b.Append( JSParser.Explain( e.TokenType ) );
            _b.Append( ' ' );
            VisitExpr( e.Expression );
            _b.Append( _exprSuffix );
            return e;
        }

        public override Expr Visit( SyntaxErrorExpr e )
        {
            _b.Append( _exprPrefix );
            _b.AppendFormat( "Syntax Error: {0}", e.ErrorMessage );
            _b.Append( _exprSuffix );
            return e;
        }

        public override string ToString()
        {
            return _b.ToString();
        }

    }
}
