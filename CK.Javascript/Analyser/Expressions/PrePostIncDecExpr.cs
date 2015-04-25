using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using CK.Core;
using System.Diagnostics;

namespace CK.Javascript
{

    public class PrePostIncDecExpr : Expr
    {
        public PrePostIncDecExpr( SourceLocation location, AccessorExpr operand, bool plus, bool prefix )
            : base( location, true )
        {
            if( operand == null ) throw new ArgumentNullException( "left" );
            Operand = operand;
            Plus = plus;
            Prefix = prefix;
        }

        public AccessorExpr Operand { get; private set; }

        public bool Plus { get; private set; }

        public bool Prefix { get; private set; }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( IExprVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

        public override string ToString()
        {
            string o = Plus ? "++" : "--";
            return Prefix ? o + Operand.ToString() : Operand.ToString() + o;
        }
    }


}
