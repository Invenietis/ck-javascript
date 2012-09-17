using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.Javascript
{
    public class JSEvalBoolean : RuntimeObj
    {
        bool _value;

        public JSEvalBoolean( bool v )
        {
            _value = v;
        }

        public override string Type
        {
            get { return RuntimeObj.TypeBoolean; }
        }

        public override bool ToBoolean()
        {
            return _value;
        }

        public override double ToDouble()
        {
            return JSSupport.ToNumber( _value );
        }

        public override string ToString()
        {
            return JSSupport.ToString( _value );
        }

    }
}
