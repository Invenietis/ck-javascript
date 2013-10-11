#region LGPL License
/* ----------------------------------------------------------------------------
*  This file (IAccessorFrame.cs) is part of CK-Javascript. 
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
    public interface IAccessorFrame
    {
        AccessorExpr Expr { get; }

        IAccessorFrame NextAccessor { get; }

        void SetResult( RuntimeObj result );

        void SetRuntimeError( string message );

        bool HasResultOrError { get; }

        IAccessorFrame MatchMember( string memberName );

        CallFunctionDescriptor MatchCall( string functionName, int maxParameterCount );

        IReadOnlyList<RuntimeObj> EvalCallArguments( int maxCount );

    }

}
