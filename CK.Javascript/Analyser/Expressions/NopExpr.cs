using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using CK.Core;
using System.Diagnostics;

namespace CK.Javascript
{

    public class NopExpr : Expr
    {
        public static readonly NopExpr Default = new NopExpr();

        NopExpr()
            : base( SourceLocation.Empty, false )
        {
        }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( IExprVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

        public override string ToString()
        {
            return String.Empty;
        }
    }


}
