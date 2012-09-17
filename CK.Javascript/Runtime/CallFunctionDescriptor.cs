using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.Javascript
{
    public struct CallFunctionDescriptor
    {
        public readonly IAccessorFrame Frame;
        public readonly IReadOnlyList<RuntimeObj> Arguments;

        public bool IsValid
        {
            get { return Frame != null; }
        }

        internal CallFunctionDescriptor( IAccessorFrame frame, IReadOnlyList<RuntimeObj> arguments )
        {
            Frame = frame;
            Arguments = arguments;
        }
    }

}
