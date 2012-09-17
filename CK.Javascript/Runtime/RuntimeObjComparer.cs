using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CK.Javascript
{
    public struct RuntimeObjComparer
    {
        public readonly RuntimeObj X;
        public readonly RuntimeObj Y;
        public readonly bool Swapped;

        public RuntimeObjComparer( RuntimeObj x, RuntimeObj y )
        {
            if( x == RuntimeObj.Null ) x = RuntimeObj.Undefined;
            if( y == RuntimeObj.Null ) y = RuntimeObj.Undefined;

            if( String.CompareOrdinal( x.Type, y.Type ) > 0 )
            {
                X = y;
                Y = x;
                Swapped = true;
            }
            else
            {
                X = x;
                Y = y;
                Swapped = false;
            }
        }

        public bool AreEqual( GlobalContext c )
        {
            if( ReferenceEquals( X, Y ) )
            {
                return X != c.NaN;
            }
            if( ReferenceEquals( X.Type, Y.Type ) )
            {
                Debug.Assert( X != RuntimeObj.Undefined && X != RuntimeObj.Null, "This has been handled by the normalization and the above reference test." );
                if( ReferenceEquals( X.Type, RuntimeObj.TypeNumber ) )
                {
                    Debug.Assert( !(((JSEvalNumber)X).IsNaN && ((JSEvalNumber)Y).IsNaN) );
                    return X.ToDouble() == Y.ToDouble();
                }
                else if( ReferenceEquals( X.Type, RuntimeObj.TypeString ) )
                {
                    return X.ToString() == Y.ToString();
                }
                else if( ReferenceEquals( X.Type, RuntimeObj.TypeBoolean ) )
                {
                    return X.ToBoolean() == Y.ToBoolean();
                }
                else
                {
                    IComparable cmp;
                    if( X.GetType() == Y.GetType() && (cmp = X as IComparable) != null )
                    {
                        Debug.Assert( (cmp.CompareTo( Y ) == 0) == X.Equals( Y ), "When IComparable is implemented, it must match Equals behavior." );
                        return cmp.Equals( Y );
                    }
                }
                return false;
            }
            if( ReferenceEquals( X.Type, RuntimeObj.TypeNumber ) && ReferenceEquals( Y.Type, RuntimeObj.TypeString ) )
            {
                return X.ToDouble() == Y.ToDouble();
            }
            if( ReferenceEquals( X.Type, RuntimeObj.TypeBoolean ) || ReferenceEquals( Y.Type, RuntimeObj.TypeBoolean ) )
            {
                return X.ToBoolean() == Y.ToBoolean();
            }
            if( ReferenceEquals( Y.Type, RuntimeObj.TypeObject ) && X != RuntimeObj.Undefined )
            {
                return new RuntimeObjComparer( X, Y.ToPrimitive( c ) ).AreEqual( c );
            }
            if( ReferenceEquals( X.Type, RuntimeObj.TypeObject ) && Y != RuntimeObj.Undefined )
            {
                return new RuntimeObjComparer( X.ToPrimitive( c ), Y ).AreEqual( c );
            }
            return false;
        }

        public bool Compare( GlobalContext c, out int result )
        {
            result = 0;
            if( Y == RuntimeObj.Undefined ) return X == RuntimeObj.Undefined;

            Debug.Assert( typeof( IComparable ).IsAssignableFrom( typeof( JSEvalString ) ), "JSEvalString is Comparable." );
            Debug.Assert( typeof( IComparable ).IsAssignableFrom( typeof( JSEvalDate ) ), "JSEvalDate is Comparable." );
            
            IComparable cmp;
            if( X.GetType() == Y.GetType() && (cmp = X as IComparable) != null )
            {
                result = cmp.CompareTo( Y );
            }
            else
            {
                Double xD = X.ToDouble();
                Double yD = Y.ToDouble();
                if( Double.IsNaN( xD ) || Double.IsNaN( yD ) ) return false;
                if( xD < yD ) result = -1;
                else if( xD > yD ) result = 1;
            }
            if( Swapped ) result = -result;
            return true;
        }

        public bool AreEqualStrict( GlobalContext c )
        {
            if( ReferenceEquals( X, Y ) )
            {
                return X != c.NaN;
            }

            if( !ReferenceEquals( X.Type, Y.Type ) ) return false;
            Debug.Assert( X != RuntimeObj.Undefined && X != RuntimeObj.Null );

            if( ReferenceEquals( X.Type, RuntimeObj.TypeNumber ) )
            {
                if( X == c.NaN || Y == c.NaN )
                {
                    return false;
                }
                return X.ToDouble() == Y.ToDouble();
            }
            if( ReferenceEquals( X.Type, RuntimeObj.TypeString ) )
            {
                return X.ToString() == Y.ToString();
            }
            if( ReferenceEquals( X.Type, RuntimeObj.TypeBoolean ) )
            {
                return X.ToBoolean() == Y.ToBoolean();
            }
            return false;
        }
    }

}
