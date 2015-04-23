using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Javascript
{
    public class ConstantExpr : Expr
    {
        public static readonly ConstantExpr UndefinedExpr = new ConstantExpr( SourceLocation.Empty, JSSupport.Undefined );

        public ConstantExpr( SourceLocation location, object value )
            : base( location )
        {
            Value = value;
        }

        public object Value { get; private set; }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( IExprVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

        public override string ToString()
        {
            return Value != null ? Value.ToString() : "(null)";
        }
    }
}
