#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Javascript\EvalVisitor\RuntimeObj.cs) is part of CiviKey. 
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
using System.Diagnostics;

namespace CK.Javascript
{
    public class RefRuntimeObj : RuntimeObj
    {
        RuntimeObj _value;

        public RefRuntimeObj()
        {
            _value = Undefined;
        }

        public RuntimeObj Value
        {
            get { return _value; }
            set 
            {
                if( value == null ) _value = RuntimeObj.Null;
                else 
                {
                    var r = value as RefRuntimeObj;
                    _value = r != null ? r.Value : value;
                }
            }
        }

        public override string Type 
        { 
            get { return _value.Type; } 
        }

        public override bool ToBoolean()
        {
            return _value.ToBoolean();
        }

        public override double ToDouble()
        {
            return _value.ToDouble();
        }

        /// <summary>
        /// Overridden to return this <see cref="Value"/>.
        /// </summary>
        /// <returns>This Value.</returns>
        public override RuntimeObj ToValue()
        {
            return _value;
        }


        public override RuntimeObj ToPrimitive( GlobalContext c )
        {
            return _value.ToPrimitive( c );
        }

        public override PExpr Visit( IAccessorFrame frame )
        {
            return new PExpr( this );
        }
    }

}
