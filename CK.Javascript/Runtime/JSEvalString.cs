using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.Javascript
{
    public class JSEvalString : RuntimeObj, IComparable
    {
        string _value;

        public JSEvalString( string value )
        {
            if( value == null ) throw new ArgumentNullException( "value" );
            _value = value;
        }

        public override string Type
        {
            get { return RuntimeObj.TypeString; }
        }

        public override bool ToBoolean()
        {
            return JSSupport.ToBoolean( _value );
        }

        public override double ToDouble()
        {
            return JSSupport.ToNumber( _value );
        }

        public override string ToString()
        {
            return _value;
        }

        public override bool Equals( object obj )
        {
            if( obj == this ) return true;
            JSEvalString s = obj as JSEvalString;
            return s != null ? s._value == _value : false;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public int CompareTo( object obj )
        {
            JSEvalString s = obj as JSEvalString;
            if( s != null ) return String.Compare( _value, s._value, StringComparison.InvariantCulture );
            if( obj is String ) return String.Compare( _value, (String)obj, StringComparison.InvariantCulture );
            throw new ArgumentException( "Must be a string.", "obj" );
        }

        public override void Visit( IEvalVisitor v, IAccessorFrame frame )
        {
            CallFunctionDescriptor f = frame.MatchCall( "charAt", 1 );
            if( f.IsValid )
            {
                int idx = f.Arguments.Count > 0 ? JSSupport.ToInt32( f.Arguments[0].ToDouble() ) : 0;
                if( idx < 0 || idx >= _value.Length ) f.Frame.SetResult( v.Global.EmptyString );
                else f.Frame.SetResult( v.Global.CreateString( new String( _value[idx], 1 ) ) );
            }
        }

    }

}
