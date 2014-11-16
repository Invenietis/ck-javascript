#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Javascript\EvalVisitor\AccessorFrame.cs) is part of CiviKey. 
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
    public class AccessorFrame : EvalVisitor.VisitFrame, IAccessorFrame
    {
        RuntimeObj _result;

        internal protected AccessorFrame( EvalVisitor visitor, AccessorExpr e )
            : base( visitor, e )
        {
        }

        public new AccessorExpr Expr
        {
            get { return (AccessorExpr)base.Expr; }
        }

        public IEvalVisitor Visitor
        {
            get { return _visitor; }
        }

        public GlobalContext Global
        {
            get { return _visitor.Global; }
        }

        public CallFunctionDescriptor MatchCall( string functionName, int maxParameterCount )
        {
            IAccessorFrame func = MatchMember( functionName );
            return new CallFunctionDescriptor( func, func != null ? func.EvalCallArguments( maxParameterCount ) : null );
        }

        public IReadOnlyList<RuntimeObj> EvalCallArguments( int maxParameterCount )
        {
            var args = Expr.CallArguments;
            if( args != null )
            {
                if( args.Count == 0 || maxParameterCount == 0 ) return CKReadOnlyListEmpty<RuntimeObj>.Empty;
                if( maxParameterCount < 0 ) maxParameterCount = args.Count;
                else maxParameterCount = Math.Min( maxParameterCount, args.Count );
                RuntimeObj[] results = new RuntimeObj[ maxParameterCount ];
                for( int i = 0; i < maxParameterCount; i++ )
                {
                    _visitor.VisitExpr( args[i] );
                    if( _visitor.HasError() ) return null;
                    results[i] = _visitor.Current;
                }
                return results.AsReadOnlyList();
            }
            return null;
        }

        public IAccessorFrame MatchMember( string memberName )
        {
            return Expr.IsMember( memberName ) ? NextAccessor : null;
        }

        public IAccessorFrame NextAccessor
        {
            get { return PrevFrame as AccessorFrame; }
        }

        public void SetResult( RuntimeObj result )
        {
            _result = result;
            EvalVisitor.VisitFrame n = NextFrame;
            if( n != null )
            {
                if( !(n is AccessorFrame) ) throw new InvalidOperationException( "Invalid use of Frames." );
                ((AccessorFrame)n).SetResult( result );
            }
        }

        internal void SetAccessorError()
        {
            SetResult( _visitor.SetAccessorError( Expr ) );
        }

        public void SetRuntimeError( string message )
        {
            SetResult( _visitor.SetRuntimeError( Expr, message ) );
        }

        public bool HasResultOrError
        {
            get { return _result != null || _visitor.HasError(); }
        }

        protected override void Dispose( RuntimeObj result )
        {
            base.Dispose( _result );
        }

    }

}
