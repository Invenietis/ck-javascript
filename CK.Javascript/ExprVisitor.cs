using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using CK.Core;

namespace CK.Javascript
{
    public class ExprVisitor : IExprVisitor<Expr>
    {
        public virtual Expr VisitExpr( Expr e )
        {
            return e.Accept( this );
        }

        public virtual Expr Visit( AccessorMemberExpr e )
        {
            Expr lV = VisitExpr( e.Left );
            return lV == e.Left ? e : new AccessorMemberExpr( e.Location, lV, e.Name );
        }

        public virtual Expr Visit( AccessorIndexerExpr e )
        {
            Expr lV = VisitExpr( e.Left );
            Expr iV = VisitExpr( e.Index );
            return lV == e.Left && iV == e.Index ? e : new AccessorIndexerExpr( e.Location, lV, iV );
        }

        public virtual Expr Visit( AccessorCallExpr e )
        {
            var lV = VisitExpr( e.Left );
            var aV = Visit( e.CallArguments );
            return lV == e.Left && aV == e.CallArguments ? e : new AccessorCallExpr( e.Location, lV, aV );
        }

        public IReadOnlyList<Expr> Visit( IReadOnlyList<Expr> args )
        {
            Expr[] newArgs = null;
            for( int i = 0; i < args.Count; ++i )
            {
                Expr p = args[i];
                Expr sp = VisitExpr( p );
                if( newArgs != null ) newArgs[i] = sp;
                else if( p != sp )
                {
                    newArgs = new Expr[args.Count];
                    int j = i;
                    while( --j >= 0 ) newArgs[j] = args[j];
                    newArgs[i] = sp;
                }
            }
            if( newArgs != null ) args = newArgs.ToReadOnlyList();
            return args;
        }

        public virtual Expr Visit( BinaryExpr e )
        {
            Expr lV = VisitExpr( e.Left );
            Expr rV = VisitExpr( e.Right );
            return lV == e.Left && rV == e.Right ? e : new BinaryExpr( e.Location, lV, e.BinaryOperatorToken, rV );
        }

        public virtual Expr Visit( ConstantExpr e )
        {
            return e;
        }

        public virtual Expr Visit( IfExpr e )
        {
            Expr cV = VisitExpr( e.Condition );
            Expr tV = VisitExpr( e.WhenTrue );
            Expr fV = e.WhenFalse != null ? VisitExpr( e.WhenFalse ) : null;
            return cV == e.Condition && tV == e.WhenTrue && fV == e.WhenFalse ? e : new IfExpr( e.Location, e.IsTernaryOperator, cV, tV, fV );
        }

        public virtual Expr Visit( UnaryExpr e )
        {
            Expr eV = VisitExpr( e.Expression );
            return eV == e.Expression ? e : new UnaryExpr( e.Location, e.TokenType, eV );
        }

        public virtual Expr Visit( SyntaxErrorExpr e )
        {
            return e;
        }

    }

}
