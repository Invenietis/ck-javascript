#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Javascript\EvalVisitor\RuntimeError.cs) is part of CiviKey. 
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
