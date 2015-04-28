using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using CK.Core;
using System.Diagnostics;

namespace CK.Javascript
{

    public class FlowBreakingExpr : Expr
    {
        public enum BreakingType
        {
            None,
            Break,
            Continue,
            Return
        }

        public FlowBreakingExpr( SourceLocation location, bool isContinue )
            : base( location, true )
        {
            Type = isContinue ? BreakingType.Continue : BreakingType.Break;
        }

        public FlowBreakingExpr( SourceLocation location, Expr returnValue )
            : base( location, true )
        {
            if( returnValue == null ) throw new ArgumentNullException( "returnValue" );
            Type = BreakingType.Return;
            ReturnedValue = returnValue;
        }

        public FlowBreakingExpr( SourceLocation location, BreakingType type, Expr returnValue )
            : base( location, true )
        {
            if( type == BreakingType.None ) throw new ArgumentNullException( "type" );
            if( type == BreakingType.Return && returnValue == null ) throw new ArgumentNullException( "returnValue" );
            Type = type;
            ReturnedValue = returnValue;
        }

        /// <summary>
        /// Gets whether this is a return, a break or a continue statement.
        /// </summary>
        public BreakingType Type { get; private set; }

        /// <summary>
        /// Gets the parameter exprssion. Currently makes senses only for <see cref="BreakingType.Return"/>.
        /// </summary>
        public Expr ReturnedValue { get; private set; }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( IExprVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

        public override string ToString()
        {
            string p;
            switch( Type )
            {
                case BreakingType.Break: p = "break"; break;
                case BreakingType.Continue: return p = "continue"; break;
                default: p = "return"; break;
            }
            if( ReturnedValue != null ) p += ' ' + ReturnedValue.ToString();
            return p + ';';
        }
    }


}
