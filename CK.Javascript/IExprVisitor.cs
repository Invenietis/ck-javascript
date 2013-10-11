using System;
using CK.Core;

namespace CK.Javascript
{
    public interface IExprVisitor<out T>
    {
        T VisitExpr( Expr e );
        T Visit( AccessorMemberExpr e );
        T Visit( AccessorIndexerExpr e );
        T Visit( AccessorCallExpr e );
        T Visit( BinaryExpr e );
        T Visit( ConstantExpr e );
        T Visit( IfExpr e );
        T Visit( SyntaxErrorExpr e );
        T Visit( UnaryExpr e );
    }
}
