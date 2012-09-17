using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.Javascript
{
    public interface IAccessorFrame
    {
        AccessorExpr Expr { get; }

        IAccessorFrame NextAccessor { get; }

        void SetResult( RuntimeObj result );

        void SetRuntimeError( string message );

        bool HasResultOrError { get; }

        IAccessorFrame MatchMember( string memberName );

        CallFunctionDescriptor MatchCall( string functionName, int maxParameterCount );

        IReadOnlyList<RuntimeObj> EvalCallArguments( int maxCount );

    }

}
