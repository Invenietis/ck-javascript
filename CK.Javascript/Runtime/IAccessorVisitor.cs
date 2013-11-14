#region LGPL License
/* ----------------------------------------------------------------------------
*  This file (IAccessorVisitor.cs) is part of CK-Javascript. 
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

namespace CK.Javascript
{
    public interface IAccessorVisitor
    {
        /// <summary>
        /// Handles the given <see cref="IAccessorFrame"/>.
        /// Through <see cref="IAccessorFrame.NextAccessor"/>, subsequent members, calls or indexers can be evaluated: 
        /// the <see cref="IAccessorFrame.SetRuntimeError"/> or <see cref="IAccessorFrame.SetResult"/> methods on the deepest handled frame must then be called
        /// to store the result and shortcut the evaluation process.
        /// </summary>
        /// <param name="frame">The frame to handle.</param>
        void Visit( IEvalVisitor v, IAccessorFrame frame );
    }
}
