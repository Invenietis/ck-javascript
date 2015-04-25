using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using CK.Core;
using System.Diagnostics;

namespace CK.Javascript
{

    public class AssignExpr : Expr
    {
        public AssignExpr( SourceLocation location, AccessorExpr left, Expr right )
            : base( location, true )
        {
            if( left == null ) throw new ArgumentNullException( "left" );
            if( right == null ) throw new ArgumentNullException( "right" );
            Left = left;
            Right = right;
        }

        public AccessorExpr Left { get; private set; }

        public Expr Right { get; private set; }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( IExprVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

        public override string ToString()
        {
            return Left.ToString() + " = " + Right.ToString();
        }
    }


}
