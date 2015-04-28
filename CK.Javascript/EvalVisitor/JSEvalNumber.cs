#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Javascript\EvalVisitor\JSEvalNumber.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.Javascript
{
    public class JSEvalNumber : RuntimeObj
    {
        static public readonly JSEvalNumber NaN = new JSEvalNumber( Double.NaN );
        static public readonly JSEvalNumber Zero = new JSEvalNumber( 0 );
        static public readonly JSEvalNumber Infinity = new JSEvalNumber( Double.PositiveInfinity );
        static public readonly JSEvalNumber NegativeInfinity = new JSEvalNumber( Double.NegativeInfinity );

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

        public override PExpr Visit( IAccessorFrame frame )
        {
            var s = frame.GetState( c => 
                c.On("toString").OnCall( (f,args) => 
                {
                    int radix = 10;
                    if( args.Count == 1 ) radix = JSSupport.ToInt32( args[0].ToDouble() );
                    if( radix < 2 || radix > 36 ) return f.SetError( "Radix must be between 2 and 36." );
                    return f.SetResult( f.Global.CreateString( JSSupport.ToString( _value, radix ) ) );
                }
                ) );
            return s != null ? s.Visit() : frame.SetError();
        }

    }

}
