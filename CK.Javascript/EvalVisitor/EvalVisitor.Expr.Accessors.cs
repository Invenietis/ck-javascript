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
        class AccessorFrame : Frame<AccessorExpr>, IAccessorFrame
        {
            class FrameState : IAccessorFrameState, IReadOnlyList<RuntimeObj>
            {
                readonly AccessorFrame _winner;
                readonly Func<IAccessorFrame, RuntimeObj, PExpr> _indexCode;
                readonly Func<IAccessorFrame, IReadOnlyList<RuntimeObj>, PExpr> _callCode;
                readonly PExpr[] _args;
                int _rpCount;

                public FrameState( AccessorFrame winner,
                                   Func<IAccessorFrame, RuntimeObj, PExpr> indexCode,
                                   Func<IAccessorFrame, IReadOnlyList<RuntimeObj>, PExpr> callCode,
                                   PExpr[] args )
                {
                    _winner = winner;
                    _indexCode = indexCode;
                    _callCode = callCode;
                    _args = args;
                }

                public PExpr Visit()
                {
                    _winner._initCount = 0;
                    while( _rpCount < _args.Length )
                    {
                        if( _winner.IsPendingOrSignal( ref _args[_rpCount], _winner.Expr.Arguments[_rpCount] ) ) return _winner.PendingOrSignal( _args[_rpCount] );
                        ++_rpCount;
                    }
                    var r = _indexCode != null ? _indexCode( _winner, _args[0].Result ) : _callCode( _winner, this );
                    if( !r.IsResolved && r.Deferred != _winner ) throw new CKException( "Implementations must call either SetResult, SetError, or PendigOrSignal frame's method." );
                    return r;
                }

                #region Auto implemented access to resolved arguments (avoid an allocation).
                RuntimeObj IReadOnlyList<RuntimeObj>.this[int index]
                {
                    get { return _args[index].Result; }
                }

                int IReadOnlyCollection<RuntimeObj>.Count
                {
                    get { return _args.Length; }
                }

                IEnumerator<RuntimeObj> IEnumerable<RuntimeObj>.GetEnumerator()
                {
                    return _args.Select( e => e.Result ).GetEnumerator();
                }

                System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                {
                    return ((IEnumerable<RuntimeObj>)this).GetEnumerator();
                }
                #endregion
            }

            class FrameInitializer : IAccessorFrameInitializer
            {
                readonly AccessorFrame _frame;
                AccessorFrame _current;
                FrameState _state;

                public FrameInitializer( AccessorFrame f )
                {
                    _current = _frame = f;
                }

                public FrameState State { get { return _state; } }

                public IAccessorFrameInitializer On( string memberName )
                {
                    if( _state == null && _current != null )
                    {
                        _current = (AccessorFrame)(_current.Expr.IsMember( memberName ) ? _current.NextAccessor : null);
                    }
                    return this;
                }

                public IAccessorFrameInitializer OnIndex( Func<IAccessorFrame, RuntimeObj, PExpr> code )
                {
                    if( _state == null )
                    {
                        if( _current != null && _current.Expr is AccessorIndexerExpr )
                        {
                            _state = new FrameState( _current, code, null, new PExpr[1] );
                        }
                        _current = _frame;
                    }
                    return this;
                }

                public IAccessorFrameInitializer OnCall( int maxParameterCount, Func<IAccessorFrame, IReadOnlyList<RuntimeObj>, PExpr> code )
                {
                    if( _state == null )
                    {
                        if( _current != null && _current.Expr is AccessorCallExpr )
                        {
                            int argCount = _current.Expr.Arguments.Count;
                            if( maxParameterCount < 0 || maxParameterCount > argCount ) maxParameterCount = argCount;
                            _state = new FrameState( _current, null, code, new PExpr[maxParameterCount] );
                        }
                        _current = _frame;
                    }
                    return this;
                }
            }

            IAccessorFrameState _state;
            int _initCount;
            int _realInitCount;
            protected PExpr _left;

            internal protected AccessorFrame( EvalVisitor visitor, AccessorExpr e )
                : base( visitor, e )
            {
            }

            /// <summary>
            /// Implementation valid for AccessorIndexerFrame and AccessorCallFrame.
            /// The AccessorMemberFrame substitutes it.
            /// </summary>
            protected override PExpr DoVisit()
            {
                if( IsPendingOrSignal( ref _left, Expr.Left ) ) return ReentrantPendingOrSignal( _left );
                return Result != null ? new PExpr( Result ) : SetError();
            }

            public IAccessorFrameState GetState( Action<IAccessorFrameInitializer> configuration )
            {
                if( ++_initCount < _realInitCount ) return null;
                if( _state == null )
                {
                    var init = new FrameInitializer( this );
                    configuration( init );
                    _state = init.State;
                    ++_realInitCount;
                }
                return _state;
            }
            
            public IAccessorFrame NextAccessor
            {
                get { return PrevFrame as IAccessorFrame; }
            }

            IAccessorFrame PrevAccessor
            {
                get { return NextFrame as IAccessorFrame; }
            }

            public override PExpr SetResult( RuntimeObj result )
            {
                IAccessorFrame p = PrevAccessor;
                if( p != null && !p.IsResolved ) p.SetResult( result );
                return base.SetResult( result );
            }

            public PExpr SetError( string message = null )
            {
                if( message != null ) return SetResult( _visitor._global.CreateRuntimeError( Expr, message ) );
                return SetResult( _visitor._global.CreateAccessorError( Expr ) );
            }

            AccessorExpr IAccessorFrame.Expr 
            { 
                get { return base.Expr; } 
            }

            protected PExpr ReentrantPendingOrSignal( PExpr sub )
            {
                Debug.Assert( sub.IsPendingOrSignal );
                if( Result != null )
                {
                    Debug.Assert( Result == sub.Result );
                    return new PExpr( Result );
                }
                return sub.IsErrorResult ? SetResult( sub.Result ) : new PExpr( this );
            }

            protected PExpr ReentrantSetResult( RuntimeObj result )
            {
                Debug.Assert( result != null );
                if( Result != null )
                {
                    Debug.Assert( Result == result );
                    return new PExpr( result );
                }
                return SetResult( result );
            }

            public PExpr ReentrantSetError( string message = null )
            {
                Debug.Assert( Result == null || Result is RuntimeError );
                return Result == null ? SetError( message ) : new PExpr( Result ); 
            }

        }

        class AccessorMemberFrame : AccessorFrame
        {
            internal protected AccessorMemberFrame( EvalVisitor visitor, AccessorMemberExpr e )
                : base( visitor, e )
            {
            }

            PExpr _result;

            protected override PExpr DoVisit()
            {
                if( !_left.IsResolved )
                {
                    if( ((AccessorMemberExpr)Expr).IsUnbound )
                    {
                        if( (_left = Global.Visit( this )).IsPendingOrSignal ) return ReentrantPendingOrSignal( _left );
                    }
                    else
                    {
                        if( IsPendingOrSignal( ref _left, Expr.Left ) ) return ReentrantPendingOrSignal( _left );
                    }
                }
                if( Result != null ) return new PExpr( Result );

                Debug.Assert( !_result.IsResolved );
                if( (_result = _left.Result.Visit( this )).IsPendingOrSignal ) return ReentrantPendingOrSignal( _result );
                return ReentrantSetResult( _result.Result );
            }

        }

        public PExpr Visit( AccessorMemberExpr e )
        {
            return new AccessorMemberFrame( this, e ).Visit();
        }

        public PExpr Visit( AccessorIndexerExpr e )
        {
            return new AccessorFrame( this, e ).Visit();
        }

        public PExpr Visit( AccessorCallExpr e )
        {
            return new AccessorFrame( this, e ).Visit();
        }

    }
}
