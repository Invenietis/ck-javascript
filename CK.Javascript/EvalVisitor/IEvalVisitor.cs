#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Javascript\EvalVisitor\IEvalVisitor.cs) is part of CiviKey. 
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

    /// <summary>
    /// A <see cref="IExprVisitor{T}"/> where T is a <see cref="PExpr"/> that is bound to a <see cref="GlobalContext"/>
    /// and exposes a <see cref="CurrentResult"/> evaluation result object and/or a <see cref="CurrentError"/>.
    /// </summary>
    public interface IEvalVisitor : IExprVisitor<PExpr>
    {
        /// <summary>
        /// Gets the <see cref="GlobalContext"/> that will be used to obtain primitive 
        /// objects (<see cref="RuntimeObj)"/>) and resolve unbound accessors.
        /// </summary>
        GlobalContext Global { get; }

    }

}
