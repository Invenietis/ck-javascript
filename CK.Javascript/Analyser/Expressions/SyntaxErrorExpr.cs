using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Javascript
{
    public class SyntaxErrorExpr : Expr
    {
        public static readonly SyntaxErrorExpr ReservedErrorExpr = new SyntaxErrorExpr( SourceLocation.Empty, "Reserved." );

        public SyntaxErrorExpr( SourceLocation location, string errorMessageFormat, params object[] messageParameters )
            : base( location )
        {
            ErrorMessage = String.Format( errorMessageFormat, messageParameters );
        }

        public string ErrorMessage { get; private set; }

        public bool IsReserved
        {
            get { return this == ReservedErrorExpr; }
        }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( IExprVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

        public override string ToString()
        {
            return "Syntax: " + ErrorMessage;
        }
    }

}
