#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Javascript\EvalVisitor\IAccessorVisitor.cs) is part of CiviKey. 
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
    /// Any <see cref="RuntimeObj"/> and the <see cref="GlobalContext"/> support this interface.
    /// This is the "binder to the external world" for any <see cref="IEvalVisitor"/>.  
    /// </summary>
    public interface IAccessorVisitor
    {
        /// <summary>
        /// Handles the given <see cref="IAccessorFrame"/>.
        /// Through <see cref="IAccessorFrame.NextAccessor"/>, subsequent members, calls or indexers can be evaluated: 
        /// the <see cref="IAccessorFrame.SetRuntimeError"/> or <see cref="IAccessorFrame.SetResult"/> methods on the deepest handled frame must then be called
        /// to store the result and shortcut the evaluation process.
        /// </summary>
        /// <param name="v">The visitor.</param>
        /// <param name="frame">The frame to handle.</param>
        void Visit( IEvalVisitor v, IAccessorFrame frame );
    }
}
