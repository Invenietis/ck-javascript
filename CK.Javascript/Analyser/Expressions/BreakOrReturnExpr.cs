using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using CK.Core;
using System.Diagnostics;

namespace CK.Javascript
{

    public class BreakOrReturnExpr : Expr
    {
        public BreakOrReturnExpr( SourceLocation location, bool isReturn, Expr returns )
            : base( location, true )
        {
            IsReturn = isReturn;
            Returns = returns;
        }

        /// <summary>
        /// Initializes a new 'break' expression.
        /// </summary>
        /// <param name="location">Source location.</param>
        public BreakOrReturnExpr( SourceLocation location )
            : this( location, false, null )
        {
        }

        /// <summary>
        /// Gets whether this is a return or a break statement.
        /// </summary>
        public bool IsReturn { get; private set; }

        /// <summary>
        /// Gets the returned exprssion. Null is <see cref="IsReturn"/> is false or there is no returned value.
        /// </summary>
        public Expr Returns { get; private set; }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( IExprVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

        public override string ToString()
        {
            return IsReturn ? "return " + (Returns != null ? Returns.ToString() : String.Empty) + ';' : "break;";
        }
    }


}
