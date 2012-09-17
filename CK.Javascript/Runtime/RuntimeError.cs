using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Javascript
{
    public class RuntimeError : RuntimeObj
    {
        RuntimeError _next;
        RuntimeError _prev;

        public RuntimeError( Expr culprit, string message, RuntimeError previous = null )
        {
            if( culprit == null ) throw new ArgumentNullException( "culprit" );
            CulpritExpr = culprit;
            Message = message;
            if( previous != null )
            {
                if( previous._next != null ) throw new InvalidOperationException( "Previous error is already linked to a next error." );
                previous._next = this;
                _prev = previous;
            }
        }

        public Expr CulpritExpr { get; private set; }

        public string Message { get; private set; }

        public RuntimeError Previous { get { return _prev; } }

        public RuntimeError Origin
        {
            get
            {
                RuntimeError e = this;
                while( e._prev != null ) e = e._prev;
                return e;
            }
        }

        public override string Type
        {
            get { return RuntimeObj.TypeObject; }
        }

        public override double ToDouble()
        {
            return Double.NaN;
        }

        public override bool ToBoolean()
        {
            return false;
        }

        public override RuntimeObj ToPrimitive( GlobalContext c )
        {
            return RuntimeObj.Undefined;
        }

        public override void Visit( IEvalVisitor v, IAccessorFrame frame )
        {
            if( frame.Expr.IsMember( "message" ) ) frame.SetResult( v.Global.CreateString( Message ) );
        }

        public override string ToString()
        {
            return String.Format( "Error: {0} at {1}.", Message, CulpritExpr.Location.ToString() );
        }
    }

}
