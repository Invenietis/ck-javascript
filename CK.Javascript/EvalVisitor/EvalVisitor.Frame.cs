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
    public partial class EvalVisitor
    {
        /// <summary>
        /// This is a basic frame object that captures an evaluation step. 
        /// The "stack" is implemented with links to a previous and next frames.
        /// </summary>
        protected abstract class Frame : IDeferredExpr, IDisposable
        {          
            internal readonly EvalVisitor _visitor;
            readonly Expr _expr;
            Frame _prev;
            Frame _next;
            RuntimeObj _result;

            protected Frame( EvalVisitor visitor, Expr e )
            {
                _visitor = visitor;
                _prev = visitor._currentFrame;
                if( _prev != null ) _prev._next = this;
                else visitor._firstFrame = this;
                visitor._currentFrame = this;
                _expr = e;
            }

            public Expr Expr
            {
                get { return _expr; }
            }

            public RuntimeObj Result
            {
                get { return _result; }
            }

            public bool IsResolved
            {
                get { return Result != null; }
            }

            public PExpr StepOut()
            {
                return _result == null ? DoVisit() : new PExpr( _result );
            }

            public PExpr StepIn()
            {
                _visitor.BreakOnNext = true;
                return StepOut();
            }

            internal PExpr Visit()
            {
                Debug.Assert( _result == null );
                if( Expr.IsBreakable && (_visitor.BreakOnNext || _visitor._breakpoints( Expr )) )
                {
                    _visitor.BreakOnNext = false;
                    return new PExpr( this );
                }
                return DoVisit();
            }

            protected abstract PExpr DoVisit();

            public PExpr PendingOrError( PExpr sub )
            {
                return sub.IsErrorResult ? SetResult( sub.Result ) : new PExpr( this );
            }

            public bool IsPendingOrError( ref PExpr current, Expr e )
            {
                if( current.IsResolved ) return false;
                if( current.IsUnknown ) current = _visitor.VisitExpr( e );
                else current = current.Deferred.StepOut();
                return current.IsPendingOrError;
            }

            public virtual PExpr SetResult( RuntimeObj result )
            {
                Debug.Assert( _result == null );
                return new PExpr( (_result = result) );
            }

            public Frame NextFrame
            {
                get { return _next; }
            }

            public Frame PrevFrame
            {
                get { return _prev; }
            }

            public IEvalVisitor Visitor
            {
                get { return _visitor; }
            }

            public GlobalContext Global
            {
                get { return _visitor.Global; }
            }

            void IDisposable.Dispose()
            {
                if( _result != null && !(_visitor._keepStackOnError && _result is RuntimeError) )
                {
                    OnDispose();
                    _visitor._currentFrame = _prev;
                    if( _prev != null ) _prev._next = null;
                    else _visitor._firstFrame = null;
                }
            }
            
            protected virtual void OnDispose()
            {
            }
        }

        protected abstract class Frame<T> : Frame where T : Expr
        {
            protected Frame( EvalVisitor evaluator, T e )
                : base( evaluator, e )
            {
            }

            public new T Expr { get { return (T)base.Expr; } }
        }

    }
}
