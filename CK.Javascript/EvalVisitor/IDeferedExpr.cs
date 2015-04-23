using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Javascript
{
    public interface IDeferedExpr
    {
        /// <summary>
        /// Gets the expression.
        /// </summary>
        Expr Expr { get; }

        /// <summary>
        /// Gets the resolved result. Null until this defered is resolved.
        /// </summary>
        RuntimeObj Result { get; }

        /// <summary>
        /// Gets whether a result (or an error) has been resolved.
        /// </summary>
        bool IsResolved { get; }

        /// <summary>
        /// Executes the required code until this expression is resolved.
        /// </summary>
        /// <returns>A promise that may not be resolved if a breakpoint is met.</returns>
        PExpr StepOut();

        /// <summary>
        /// Executes only one step.
        /// </summary>
        /// <returns>A promise that may be resolved if all the required code hase been executed.</returns>
        PExpr StepIn();

    }
}
