#region LGPL License
/* ----------------------------------------------------------------------------
*  This file (GlobalContext.cs) is part of CK-Javascript. 
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
using System.Globalization;

namespace CK.Javascript
{
    public class GlobalContext : IAccessorVisitor
    {
        JSEvalNumber _nan;
        JSEvalNumber _zero;
        JSEvalNumber _infinity;
        JSEvalNumber _negativeInfinity;
        JSEvalString _emptyString;
        JSEvalBoolean _true;
        JSEvalBoolean _false;
        JSEvalDate _epoch;

        public GlobalContext()
        {
            _nan = new JSEvalNumber( Double.NaN );
            _zero = new JSEvalNumber( 0 );
            _infinity = new JSEvalNumber( Double.PositiveInfinity );
            _negativeInfinity = new JSEvalNumber( Double.NegativeInfinity );
            _emptyString = new JSEvalString( String.Empty );
            _true = new JSEvalBoolean( true );
            _false = new JSEvalBoolean( false );
            _epoch = new JSEvalDate( JSSupport.JSEpoch );
        }

        public JSEvalNumber NaN
        {
            get { return _nan; }
        }

        public JSEvalNumber Infinity
        {
            get { return _infinity; }
        }

        public JSEvalNumber NegativeInfinity
        {
            get { return _negativeInfinity; }
        }

        public JSEvalNumber Zero
        {
            get { return _zero; }
        }

        public JSEvalString EmptyString
        {
            get { return _emptyString; }
        }

        public JSEvalBoolean True
        {
            get { return _true; }
        }

        public JSEvalBoolean False
        {
            get { return _false; }
        }

        public JSEvalDate Epoch
        {
            get { return _epoch; }
        }

        public RuntimeObj CreateBoolean( bool value )
        {
            return value ? True : False;
        }

        public RuntimeObj CreateBoolean( RuntimeObj o )
        {
            if( o == null ) return _false;
            if( o is JSEvalBoolean ) return o;
            o = o.ToPrimitive( this );
            if( o is JSEvalBoolean ) return o;
            return CreateBoolean( o.ToBoolean() );
        }

        public RuntimeObj CreateNumber( double value )
        {
            if( value == 0 ) return _zero;
            if( Double.IsNaN( value ) ) return _nan;
            if( Double.IsPositiveInfinity( value ) ) return _infinity;
            if( Double.IsNegativeInfinity( value ) ) return _negativeInfinity;
            return new JSEvalNumber( value );
        }

        public RuntimeObj CreateNumber( RuntimeObj o )
        {
            if( o == null ) return _zero;
            if( o is JSEvalNumber ) return o;
            o = o.ToPrimitive( this );
            if( o is JSEvalNumber ) return o;
            return CreateNumber( o.ToDouble() );
        }

        public RuntimeObj CreateString( string value )
        {
            if( value == null ) return RuntimeObj.Null;
            if( value.Length == 0 ) return _emptyString;
            return new JSEvalString( value );
        }

        public RuntimeObj CreateString( RuntimeObj o )
        {
            if( o == null ) return RuntimeObj.Null;
            if( o is JSEvalString ) return o;
            o = o.ToPrimitive( this );
            if( o is JSEvalString ) return o;
            return CreateString( o.ToString() );
        }

        public RuntimeObj CreateDateTime( DateTime value )
        {
            if( value == JSSupport.JSEpoch ) return _epoch;
            return new JSEvalDate( value );
        }

        public RuntimeError CreateRuntimeError( Expr e, string message, RuntimeError previous = null )
        {
            return new RuntimeError( e, message, previous );
        }

        public RuntimeError CreateAccessorError( AccessorExpr e, RuntimeError previous = null )
        {
            AccessorMemberExpr m = e as AccessorMemberExpr;
            if( m != null )
            {
                if( m.IsUnbound ) return CreateRuntimeError( e, "Undefined in scope: " + m.Name );
                return CreateRuntimeError( e, "Unknown property: " + m.Name, previous );
            }
            if( e is AccessorIndexerExpr ) return CreateRuntimeError( e, "Indexer is not supported.", previous );
            return CreateRuntimeError( e, "Not a function.", previous );
        }

        public virtual void Visit( IEvalVisitor v, IAccessorFrame frame )
        {
            CallFunctionDescriptor f = frame.MatchCall( "Number", 1 );
            if( f.IsValid )
            {
                if( f.Arguments.Count == 0 ) f.Frame.SetResult( Zero );
                else f.Frame.SetResult( CreateNumber( f.Arguments[0] ) );
            }
            else if( (f = frame.MatchCall( "String", 1 )).IsValid )
            {
                if( f.Arguments.Count == 0 ) f.Frame.SetResult( EmptyString );
                else f.Frame.SetResult( CreateString( f.Arguments[0] ) );
            }
            else if( (f = frame.MatchCall( "Boolean", 1 )).IsValid )
            {
                if( f.Arguments.Count == 0 ) f.Frame.SetResult( False );
                else f.Frame.SetResult( CreateBoolean( f.Arguments[0] ) );
            }
            else if( (f = frame.MatchCall( "Date", 7 )).IsValid )
            {
                try
                {
                    int[] p = new int[7];
                    for( int i = 0; i < f.Arguments.Count; ++i )
                    {
                        p[i] = (int)f.Arguments[i].ToDouble();
                        if( p[i] < 0 ) p[i] = 0;
                    }
                    if( p[0] > 9999 ) p[0] = 9999;
                    if( p[1] < 1 ) p[1] = 1;
                    else if( p[1] > 12 ) p[1] = 12;
                    if( p[2] < 1 ) p[2] = 1;
                    else if( p[2] > 31 ) p[2] = 31;
                    DateTime d = new DateTime( p[0], p[1], p[2], p[3], p[4], p[5], p[6], DateTimeKind.Utc );
                    f.Frame.SetResult( CreateDateTime( d ) );
                }
                catch( Exception ex )
                {
                    f.Frame.SetRuntimeError( ex.Message );
                }
            }
        }
    }

}
