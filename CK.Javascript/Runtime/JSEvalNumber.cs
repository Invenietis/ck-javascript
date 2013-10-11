#region LGPL License
/* ----------------------------------------------------------------------------
*  This file (JSEvalNumber.cs) is part of CK-Javascript. 
*   
*  CK-Javascript is free software: you can redistribute it and/or modify 
*  it under the terms of the GNU Lesser General Public License as published 
*  by the Free Software Foundation, either version 3 of the License, or 
*  (at your option) any later version. 
*   
*  CK-Javascript is distributed in the hope that it will be useful, 
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
*  GNU Lesser General Public License for more details. 
*  You should have received a copy of the GNU Lesser General Public License 
*  along with CK-Javascript.  If not, see <http://www.gnu.org/licenses/>. 
*   
*  Copyright © 2013, 
*      Invenietis <http://www.invenietis.com>
*  All rights reserved. 
* -----------------------------------------------------------------------------*/
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
