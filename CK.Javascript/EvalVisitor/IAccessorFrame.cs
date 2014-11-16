#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Javascript\EvalVisitor\IAccessorFrame.cs) is part of CiviKey. 
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

    /// <summary>
    /// Encapsulates chain of accessors. The <see cref=""/>
    /// </summary>
    public interface IAccessorFrame
    {
        /// <summary>
        /// Gets the <see cref="Expr"/> of this frame.
        /// </summary>
        AccessorExpr Expr { get; }

        /// <summary>
        /// Gets the next accessor if any.
        /// </summary>
        IAccessorFrame NextAccessor { get; }

        /// <summary>
        /// Sets the result for this frame.
        /// </summary>
        /// <param name="result">The evaluated object.</param>
        void SetResult( RuntimeObj result );

        /// <summary>
        /// Sets any error message if this frame can not be evaluated.
        /// </summary>
        /// <param name="message">Error message.</param>
        void SetRuntimeError( string message );

        /// <summary>
        /// Gets whether this frame has been resolved.
        /// </summary>
        bool HasResultOrError { get; }

        /// <summary>
        /// Returns <see cref="NextAccessor"/> if the <see cref="Expr"/> of this frame is 
        /// an <see cref="AccessorMemberExpr"/> with the provided name, null otherwise.
        /// </summary>
        /// <param name="memberName">Member name.</param>
        /// <returns>The <see cref="NextAccessor"/> or null.</returns>
        IAccessorFrame MatchMember( string memberName );

        /// <summary>
        /// Always returns a <see cref="CallFunctionDescriptor"/>. <see cref="CallFunctionDescriptor.IsValid"/> is true
        /// if the <see cref="Expr"/> of this frame is a function call.
        /// The first <paramref name="maxParameterCount"/> arguments must be succesfully evaluated for <see cref="CallFunctionDescriptor.IsValid"/> to be true.
        /// </summary>
        /// <param name="functionName">The function name.</param>
        /// <param name="maxParameterCount">The maximum numbers of parameters: use a negative value (-1) to evaluate all the arguments.</param>
        /// <returns>A <see cref="CallFunctionDescriptor"/> that may not be valid.</returns>
        CallFunctionDescriptor MatchCall( string functionName, int maxParameterCount = -1 );

        /// <summary>
        /// Evaluates <see cref="AccessorExpr.CallArguments"/> of <see cref="Expr"/> of this frame.
        /// </summary>
        /// <param name="maxParameterCount">The numbers of parameters to evaluate: use a negative value (-1) to evaluate all the arguments.</param>
        /// <returns></returns>
        IReadOnlyList<RuntimeObj> EvalCallArguments( int maxCount = -1 );

    }

}
