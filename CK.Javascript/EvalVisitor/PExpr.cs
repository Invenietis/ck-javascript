using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Javascript
{

    /// <summary>
    /// Promise of an <see cref="Expr"/>: either a Expr or a <see cref=""/>.
    /// </summary>
    public struct PExpr
    {
        public readonly IDeferedExpr Defered;
        public readonly RuntimeObj Result;

        public PExpr( IDeferedExpr pending )
            : this( pending, null )
        {
        }

        public PExpr( RuntimeObj resultOrError )
            : this( null, resultOrError )
        {
        }

        PExpr( IDeferedExpr pending, RuntimeObj resultOrError )
        {
            Defered = pending;
            Result = resultOrError;
        }

        public bool IsUnknown { get { return Result == null && Defered == null; } }

        public bool IsErrorResult { get { return Result is RuntimeError; } }

        public bool IsPending { get { return Defered != null; } }
        
        public bool IsResolved { get { return Result != null; } }

        public bool IsPendingOrError { get { return Defered != null || IsErrorResult; } }

        public bool IsValidResult { get { return Result != null && !IsErrorResult; } }

        public override string ToString()
        {
            string sP = Defered != null ? String.Format( "Defered = {0}", Defered.Expr ) : null;
            string sR = Result != null ? String.Format( "Result = {0}", Result ) : null;
            if( sP == null ) return sR != null ? sR : "(Unknown)";
            if( sR == null ) return sP;
            return sP + ", " + sR;
        }
    }

}
