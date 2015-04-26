using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using CK.Core;
using System.Diagnostics;

namespace CK.Javascript
{

    public class IfExpr : Expr
    {
        public IfExpr( SourceLocation location, bool isTernary, Expr condition, Expr whenTrue, Expr whenFalse )
            : base( location, true )
        {
            IsTernaryOperator = isTernary;
            Condition = condition;
            WhenTrue = whenTrue;
            WhenFalse = whenFalse;
        }

        /// <summary>
        /// Gets whether this is a ternary ?: expression (<see cref="WhenFalse"/> necessarily exists). 
        /// Otherwise, it is an if statement: <see cref="WhenTrue"/> and WhenFalse are
        /// Blocks (and WhenFalse may be null).
        /// </summary>
        public bool IsTernaryOperator { get; private set; }

        public Expr Condition { get; private set; }

        public Expr WhenTrue { get; private set; }

        public Expr WhenFalse { get; private set; }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( IExprVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

        public override string ToString()
        {
            string s = "if(" + Condition.ToString() + ") then {" + WhenTrue.ToString() + "}";
            if( WhenFalse != null ) s += " else {" + WhenFalse.ToString() + "}";
            return s;
        }
    }


}