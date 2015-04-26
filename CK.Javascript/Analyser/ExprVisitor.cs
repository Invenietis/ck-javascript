#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Javascript\ExprVisitor.cs) is part of CiviKey. 
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
using System.Linq.Expressions;
using CK.Core;

namespace CK.Javascript
{
    public class ExprVisitor : IExprVisitor<Expr>
    {
        public virtual Expr VisitExpr( Expr e )
        {
            return e.Accept( this );
        }

        public virtual Expr Visit( AccessorMemberExpr e )
        {
            Expr lV = VisitExpr( e.Left );
            return lV == e.Left ? e : new AccessorMemberExpr( e.Location, lV, e.Name );
        }

        public virtual Expr Visit( AccessorIndexerExpr e )
        {
            Expr lV = VisitExpr( e.Left );
            Expr iV = VisitExpr( e.Index );
            return lV == e.Left && iV == e.Index ? e : new AccessorIndexerExpr( e.Location, lV, iV );
        }

        public virtual Expr Visit( AccessorCallExpr e )
        {
            var lV = VisitExpr( e.Left );
            var aV = Visit( e.Arguments );
            return lV == e.Left && aV == e.Arguments ? e : new AccessorCallExpr( e.Location, lV, aV );
        }

        public IReadOnlyList<Expr> Visit( IReadOnlyList<Expr> multi )
        {
            Expr[] newMulti = null;
            for( int i = 0; i < multi.Count; ++i )
            {
                Expr p = multi[i];
                Expr sp = VisitExpr( p );
                if( newMulti != null ) newMulti[i] = sp;
                else if( p != sp )
                {
                    newMulti = new Expr[multi.Count];
                    int j = i;
                    while( --j >= 0 ) newMulti[j] = multi[j];
                    newMulti[i] = sp;
                }
            }
            if( newMulti != null ) multi = newMulti.ToReadOnlyList();
            return multi;
        }

        public virtual Expr Visit( BinaryExpr e )
        {
            Expr lV = VisitExpr( e.Left );
            Expr rV = VisitExpr( e.Right );
            return lV == e.Left && rV == e.Right ? e : new BinaryExpr( e.Location, lV, e.BinaryOperatorToken, rV );
        }

        public virtual Expr Visit( ConstantExpr e )
        {
            return e;
        }

        public virtual Expr Visit( IfExpr e )
        {
            Expr cV = VisitExpr( e.Condition );
            Expr tV = VisitExpr( e.WhenTrue );
            Expr fV = e.WhenFalse != null ? VisitExpr( e.WhenFalse ) : null;
            return cV == e.Condition && tV == e.WhenTrue && fV == e.WhenFalse ? e : new IfExpr( e.Location, e.IsTernaryOperator, cV, tV, fV );
        }

        public virtual Expr Visit( UnaryExpr e )
        {
            Expr eV = VisitExpr( e.Expression );
            return eV == e.Expression ? e : new UnaryExpr( e.Location, e.TokenType, eV );
        }

        public virtual Expr Visit( SyntaxErrorExpr e )
        {
            return e;
        }

        public virtual Expr Visit( ListOfExpr e )
        {
            var lV = Visit( e.List );
            return lV == e.List ? e : new ListOfExpr( lV );
        }

        public virtual Expr Visit( BlockExpr e )
        {
            var sV = Visit( e.List );
            var lV = (IReadOnlyList<AccessorDeclVarExpr>)Visit( e.Locals );
            return sV == e.List && lV == e.Locals ? e : new BlockExpr( sV, lV );
        }

        public virtual Expr Visit( AssignExpr e )
        {
            var lV = (AccessorMemberExpr)VisitExpr( e.Left );
            var rV = VisitExpr( e.Right );
            return lV == e.Left && rV == e.Right ? e : new AssignExpr( e.Location, lV, rV );
        }

        public virtual Expr Visit( AccessorDeclVarExpr e )
        {
            return e;
        }

        public virtual Expr Visit( NopExpr e )
        {
            return e;
        }

        public virtual Expr Visit( PrePostIncDecExpr e )
        {
            var oV = VisitExpr( e.Operand );
            return oV == e.Operand ? e : new PrePostIncDecExpr( e.Location, (AccessorExpr)oV, e.Plus, e.Prefix );
        }

        public virtual Expr Visit( WhileExpr e )
        {
            var cV = VisitExpr( e.Condition );
            var oV = VisitExpr( e.Code );
            return cV == e.Condition && oV == e.Code ? e : new WhileExpr( e.Location, cV, oV );
        }

        public virtual Expr Visit( BreakOrReturnExpr e )
        {
            var rV = e.Returns != null ? VisitExpr( e.Returns ) : null;
            return rV == e.Returns ? e : new BreakOrReturnExpr( e.Location, e.IsReturn, rV );
        }

    }

}