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

namespace CK.Javascript
{

    public partial class EvalVisitor : IExprVisitor<PExpr>, IEvalVisitor
    {
        readonly GlobalContext _global;
        readonly Func<Expr,bool> _breakpoints;
        Frame _firstFrame;
        Frame _currentFrame;
        bool _keepStackOnError;

        public EvalVisitor( GlobalContext context, bool keepStackOnError = false, Func<Expr,bool> breakpoints = null )
        {
            if( context == null ) throw new ArgumentNullException( "context" );
            _global = context;
            _keepStackOnError = keepStackOnError;
            _breakpoints = breakpoints ?? (e => false);
        }

        internal bool BreakOnNext;

        public PExpr VisitExpr( Expr e )
        {
            return e.Accept( this );
        }

        public IDeferedExpr CurrentFrame
        {
            get { return _currentFrame; }
        }

        public IDeferedExpr FirstFrame
        {
            get { return _firstFrame; }
        }

        public IEnumerable<IDeferedExpr> Frames
        {
            get 
            { 
                var f = _currentFrame; 
                while( f != null )
                {
                    yield return f;
                    f = f.NextFrame;
                }
            }
        }

        public void ResetCurrentEvaluation()
        {
            _currentFrame = null;
        }

        public GlobalContext Global 
        {
            get { return _global; }
        }

    }
}
