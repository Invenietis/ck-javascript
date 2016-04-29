#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Javascript\SyntaxErrorCollector.cs) is part of CiviKey. 
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
    public class SyntaxErrorCollector : ExprVisitor
    {
        Action<SyntaxErrorExpr> _collector;
        Action<AccessorExpr> _unboundCollector;

        public SyntaxErrorCollector( Action<SyntaxErrorExpr> collector, Action<AccessorExpr> unboundCollector = null )
        {
            if( collector == null ) throw new ArgumentNullException( "collector" );
            _collector = collector;
            _unboundCollector = unboundCollector;
        }

        static public IReadOnlyList<SyntaxErrorExpr> Collect( Expr e, Action<AccessorExpr> unboundCollector = null )
        {
            List<SyntaxErrorExpr> collector = new List<SyntaxErrorExpr>();
            new SyntaxErrorCollector( collector.Add, unboundCollector ).VisitExpr( e );
            return collector.ToArray();
        }

        public override Expr Visit( AccessorMemberExpr e )
        {
            if( _unboundCollector != null )
            {
                if( e.IsUnbound ) _unboundCollector( e );
                else VisitExpr( e.Left );
            }
            return e;
        }

        public override Expr Visit( SyntaxErrorExpr e )
        {
            _collector( e );
            return e;
        }
    }
}
