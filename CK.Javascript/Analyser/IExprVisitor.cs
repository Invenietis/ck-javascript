#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Javascript\IExprVisitor.cs) is part of CiviKey. 
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
using CK.Core;

namespace CK.Javascript
{
    /// <summary>
    /// Basic visitor contract: it is parametrized with the returns type of the visit methods.
    /// </summary>
    /// <typeparam name="T">Type of the returns value of the visit methods.</typeparam>
    public interface IExprVisitor<out T>
    {
        T VisitExpr( Expr e );
        T Visit( AccessorMemberExpr e );
        T Visit( AccessorIndexerExpr e );
        T Visit( AccessorCallExpr e );
        T Visit( BinaryExpr e );
        T Visit( ConstantExpr e );
        T Visit( IfExpr e );
        T Visit( SyntaxErrorExpr e );
        T Visit( UnaryExpr e );
        T Visit( ListOfExpr e );
        T Visit( BlockExpr e );
        T Visit( AssignExpr e );
        T Visit( AccessorDeclVarExpr e );
        T Visit( NopExpr e );
        T Visit( PrePostIncDecExpr e );
        T Visit( WhileExpr e );
        T Visit( FlowBreakingExpr e );
        T Visit( FunctionExpr e );
    }
}
