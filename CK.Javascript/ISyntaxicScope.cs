#region LGPL License
/* ----------------------------------------------------------------------------
*  This file (ISyntaxicScope.cs) is part of CK-Javascript. 
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
    public interface ISyntaxicScope
    {
        /// <summary>
        /// Obtains a named <see cref="Expx"/> if it exists. Null otherwise.
        /// </summary>
        /// <param name="name">Name in the scope.</param>
        /// <returns>Null if not found.</returns>
        Expr Find( string name );

        /// <summary>
        /// Reserves a name in the scope by creating a <see cref="SyntaxErrorExpr.ReservedErrorExpr"/>.
        /// </summary>
        /// <param name="name">Name to reserve.</param>
        void Reserve( string name );

        /// <summary>
        /// Registers a named <see cref="Expr"/> in the scope.
        /// </summary>
        /// <param name="name">Name to define.</param>
        /// <param name="e">Associated expression.</param>
        /// <returns>
        /// The given <paramref name="e"/> or a <see cref="SyntaxErrorExpr"/> that can be <see cref="SyntaxErrorExpr.ReservedErrorExpr"/>
        /// if the name is reserved.
        /// </returns>
        Expr Define( string name, Expr e );

        /// <summary>
        /// Opens a new subordinated scope.
        /// </summary>
        /// <param name="location">The location in the source.</param>
        /// <returns>The opened scope.</returns>
        ISyntaxicScope OpenScope( SourceLocation location );

        /// <summary>
        /// Closes the current scope.
        /// </summary>
        void CloseScope();
    }
}
