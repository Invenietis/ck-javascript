using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Javascript
{
    public class BinaryExpr : Expr
    {
        public BinaryExpr( SourceLocation location, Expr left, JSTokeniserToken binaryOperatorToken, Expr right )
            : base( location, true )
        {
            Left = left;
            BinaryOperatorToken = binaryOperatorToken;
            Right = right;
        }

        public Expr Left { get; private set; }

        public JSTokeniserToken BinaryOperatorToken { get; private set; }

        public Expr Right { get; private set; }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( IExprVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

        public override string ToString()
        {
            return Left.ToString() + JSTokeniser.Explain( BinaryOperatorToken ) + Right.ToString();
        }
    }
}
