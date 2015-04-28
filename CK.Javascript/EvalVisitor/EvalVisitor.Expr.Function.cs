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
        internal class FunctionExprFrame : Frame<FunctionExpr>
        {
            readonly FrameStateBase _arguments;
            PExpr _body;

            public FunctionExprFrame( AccessorFrame callFrame, FunctionExpr e )
                : base( callFrame._visitor, e )
            {
                _arguments = new FrameStateBase( this, callFrame.Expr.Arguments );
            }

            protected override PExpr DoVisit()
            {
                PExpr args = _arguments.VisitArguments();
                if( args.IsPendingOrSignal ) return args;

                int iParam = 0;
                foreach( var local in Expr.Parameters )
                {
                    var r = _visitor._dynamicScope.Register( local );
                    if( iParam < _arguments.ResolvedParameters.Count ) r.Value = _arguments.ResolvedParameters[iParam];
                }
                if( IsPendingOrSignal( ref _body, Expr.Body ) )
                {
                    if( _body.IsSignal )
                    {
                        RuntimeFlowBreaking r = _body.Result as RuntimeFlowBreaking;
                        if( r != null && r.Expr.Type == FlowBreakingExpr.BreakingType.Return )
                        {
                            return SetResult( r.Value );
                        }
                    }
                    return PendingOrSignal( _body );
                }
                return new PExpr( RuntimeObj.Undefined );
            }

            protected override void OnDispose()
            {
                foreach( var local in Expr.Parameters )
                {
                    _visitor._dynamicScope.Unregister( local );
                }
            }
        }

        public PExpr Visit( FunctionExpr e )
        {
            return new PExpr( new JSEvalFunction( e ) );
        }

    }
}
