using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using System.Diagnostics;

namespace CK.Javascript
{
    public abstract class RuntimeObj : IAccessorVisitor
    {
        public readonly static string TypeBoolean = "boolean";
        public readonly static string TypeNull = "null";
        public readonly static string TypeNumber = "number";
        public readonly static string TypeObject = "object";
        public readonly static string TypeString = "string";
        public readonly static string TypeUndefined = "undefined";

        class JSUndefined : RuntimeObj
        {            
            public override string Type
            {
                get { return TypeUndefined; }
            }

            public override string ToString()
            {
                return "undefined";
            }

            public override bool ToBoolean()
            {
                return false;
            }

            public override double ToDouble()
            {
                return double.NaN;
            }
        }
        
        class JSNull : RuntimeObj
        {
            public override string Type
            {
                get { return RuntimeObj.TypeNull; }
            }

            public override bool ToBoolean()
            {
                return false;
            }

            public override double ToDouble()
            {
                return 0;
            }

            public override string ToString()
            {
                return String.Empty;
            }

            public override int GetHashCode()
            {
                return 0;
            }

            public override bool Equals( object obj )
            {
                return obj == null || obj == DBNull.Value || obj is JSNull;
            }
        }

        public static readonly RuntimeObj Undefined = new JSUndefined();
        public static readonly RuntimeObj Null = new JSNull();

        public abstract string Type { get; }

        public abstract bool ToBoolean();

        public abstract double ToDouble();

        public virtual RuntimeObj ToPrimitive( GlobalContext c )
        {
            return this;
        }

        public virtual void Visit( IEvalVisitor v, IAccessorFrame frame )
        {
        }

    }

}
