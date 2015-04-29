using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Javascript
{
    struct Closure
    {
        public readonly AccessorDeclVarExpr Variable;
        public readonly RefRuntimeObj Ref;

        public Closure( AccessorDeclVarExpr v, RefRuntimeObj r )
        {
            Variable = v;
            Ref = r;
        }
    }

}
