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
                if( args.Count == 0 || maxParameterCount == 0 ) return ReadOnlyListEmpty<RuntimeObj>.Empty;
                if( maxParameterCount < 0 ) maxParameterCount = args.Count;
                else maxParameterCount = Math.Min( maxParameterCount, args.Count );
                RuntimeObj[] results = new RuntimeObj[ maxParameterCount ];
                for( int i = 0; i < maxParameterCount; i++ )
                {
                    _visitor.VisitExpr( args[i] );
                    if( _visitor.HasError ) return null;
                    results[i] = _visitor.Current;
                }
                return results.ToReadOnlyList();
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
            get { return _result != null || _visitor.HasError; }
        }

        protected override void Dispose( RuntimeObj result )
        {
            base.Dispose( _result );
        }

    }

}
