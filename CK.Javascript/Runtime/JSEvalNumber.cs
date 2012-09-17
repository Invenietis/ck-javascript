using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.Javascript
{
    public class JSEvalNumber : RuntimeObj
    {
        double _value;

        public JSEvalNumber( double value )
        {
            _value = value;
        }

        public override string Type
        {
            get { return RuntimeObj.TypeNumber; }
        }

        public override bool ToBoolean()
        {
            return JSSupport.ToBoolean( _value );
        }

        public override double ToDouble()
        {
            return _value;
        }

        public bool IsNaN
        {
            get { return Double.IsNaN( _value ); }
        }

        public override string ToString()
        {
            return JSSupport.ToString( _value );
        }

        public override void Visit(  IEvalVisitor v, IAccessorFrame frame )
        {
            CallFunctionDescriptor f = frame.MatchCall( "toString", 1 ); 
            if( f.IsValid )
            {
                int radix = 10;
                if( f.Arguments.Count == 1 ) radix = JSSupport.ToInt32( f.Arguments[0].ToDouble() );
                if( radix < 2 || radix > 36 ) f.Frame.SetRuntimeError( "Radix must be between 2 and 36." );
                else f.Frame.SetResult( v.Global.CreateString( JSSupport.ToString( _value, radix ) ) );
            }
        }

    }

}
