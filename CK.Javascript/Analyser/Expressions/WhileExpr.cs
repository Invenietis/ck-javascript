using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using CK.Core;
using System.Diagnostics;

namespace CK.Javascript
{

    public class WhileExpr : Expr
    {
        public WhileExpr( SourceLocation location, Expr condition, Expr code )
            : base( location, false )
        {
            Condition = condition;
            Code = code;
        }

        public Expr Condition { get; private set; }

        public Expr Code { get; private set; }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( IExprVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

        public override string ToString()
        {
            return "while(" + Condition.ToString() + ") {" + Code.ToString() + "}";
        }
    }


}
