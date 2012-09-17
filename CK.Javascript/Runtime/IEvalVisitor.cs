using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Javascript
{
    public  interface IEvalVisitor : IExprVisitor<Expr>
    {
        GlobalContext Global { get; }

        RuntimeObj Current { get; }
        
        RuntimeError CurrentError { get; }
    }

}
