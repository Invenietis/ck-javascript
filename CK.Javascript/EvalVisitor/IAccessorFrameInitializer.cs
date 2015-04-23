using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Javascript
{
    
    public interface IAccessorFrameInitializer : IFluentInterface
    {

        /// <summary>
        /// Register a member name selector.
        /// </summary>
        /// <param name="memberName">Name of the member.</param>
        /// <returns>This initializer (fluent syntax).</returns>
        IAccessorFrameInitializer On( string memberName );

        /// <summary>
        /// Registers an access index.
        /// </summary>
        /// <param name="code">Handler that must actually resolve the index access.</param>
        /// <returns>This initializer (fluent syntax).</returns>
        IAccessorFrameInitializer OnIndex( Func<IAccessorFrame, RuntimeObj, PExpr> code );

        /// <summary>
        /// Registers a call to a function.
        /// </summary>
        /// <param name="maxParameterCount">Maximum parameters count (others will be ignored).</param>
        /// <param name="code">Handler that must actually do the call.</param>
        IAccessorFrameInitializer OnCall( int maxParameterCount, Func<IAccessorFrame, IReadOnlyList<RuntimeObj>, PExpr> code );

    }

}
