using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Javascript
{
    public interface IAccessorVisitor
    {
        /// <summary>
        /// Handles the given <see cref="IAccessorFrame"/>.
        /// Through <see cref="IAccessorFrame.NextAccessor"/>, subsequent members, calls or indexers can be evaluated: 
        /// the <see cref="IAccessorFrame.SetRuntimeError"/> or <see cref="IAccessorFrame.SetResult"/> methods on the deepest handled frame must then be called
        /// to store the result and shortcut the evaluation process.
        /// </summary>
        /// <param name="frame">The frame to handle.</param>
        void Visit( IEvalVisitor v, IAccessorFrame frame );
    }
}
