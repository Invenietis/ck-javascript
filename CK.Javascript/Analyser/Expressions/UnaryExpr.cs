using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Javascript
{
    public class UnaryExpr : Expr
    {
        public UnaryExpr( SourceLocation location, JSTokeniserToken type, Expr e )
            : base( location )
        {
            TokenType = type;
            Expression = e;
        }

        public JSTokeniserToken TokenType { get; private set; }

        public Expr Expression { get; private set; }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( IExprVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

        public override string ToString()
        {
            return JSTokeniser.Explain( TokenType ) + Expression.ToString();
        }
    }
}
