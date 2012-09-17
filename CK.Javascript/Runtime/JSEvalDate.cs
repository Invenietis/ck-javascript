using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.Javascript
{
    public class JSEvalDate : RuntimeObj, IComparable
    {
        public static readonly string Format = "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'zzz";
        public static readonly string FormatTUTC = "ddd, dd MMM yyyy HH':'mm':'ss 'UTC'";
        public static readonly string DateFormat = "ddd, dd MMM yyyy";
        public static readonly string TimeFormat = "HH':'mm':'ss 'GMT'zzz";
        
        DateTime _value;

        public JSEvalDate( DateTime value )
        {
            _value = value;
        }

        public override string Type
        {
            get { return RuntimeObj.TypeObject; }
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
            return JSSupport.ToString( _value ); ;
        }

        public override bool Equals( object obj )
        {
            if( obj == this ) return true;
            JSEvalDate d = obj as JSEvalDate;
            return d != null ? d._value == _value : false;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public int CompareTo( object obj )
        {
            JSEvalDate d = obj as JSEvalDate;
            if( d != null ) return _value.CompareTo( d._value );
            if( obj is DateTime ) return _value.CompareTo( (DateTime)obj );
            throw new ArgumentException( "Must be a Date.", "obj" );
        }
    }



}
