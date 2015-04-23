#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Javascript\EvalVisitor\EvalVisitor.cs) is part of CiviKey. 
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
using System.Diagnostics;
using CK.Core;
using System.Collections.ObjectModel;

namespace CK.Javascript
{

    public partial class EvalVisitor
    {
        public PExpr Visit( ConstantExpr e )
        {
            if( e.Value == null || e.Value is string ) return new PExpr( _global.CreateString( (string)e.Value ) );
            if( e.Value is Double ) return new PExpr( _global.CreateNumber( (Double)e.Value ) );
            if( e.Value is Boolean ) return new PExpr( _global.CreateBoolean( (Boolean)e.Value ) );
            return new PExpr( new RuntimeError( e, "Unsupported JS type: " + e.Value.GetType().Name ) );
        }

        public PExpr Visit( SyntaxErrorExpr e )
        {
            return new PExpr( _global.CreateRuntimeError( e, e.ErrorMessage ) );
        }

    }
}
