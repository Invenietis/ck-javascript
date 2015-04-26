﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Javascript
{

    /// <summary>
    /// Promise of an <see cref="Expr"/>: either a Expression or a <see cref="IDeferredExpr"/>.
    /// </summary>
    public struct PExpr
    {
        public readonly IDeferredExpr Deferred;
        public readonly RuntimeObj Result;

        public PExpr( IDeferredExpr pending )
            : this( pending, null )
        {
        }

        public PExpr( RuntimeObj resultOrSignal )
            : this( null, resultOrSignal )
        {
        }

        PExpr( IDeferredExpr pending, RuntimeObj resultOrSignal )
        {
            Deferred = pending;
            Result = resultOrSignal;
        }

        public bool IsUnknown { get { return Result == null && Deferred == null; } }

        public bool IsSignal { get { return Result is RuntimeSignal; } }

        public bool IsErrorResult { get { return Result is RuntimeError; } }

        public bool IsPending { get { return Deferred != null; } }
        
        public bool IsResolved { get { return Result != null; } }

        public bool IsPendingOrSignal { get { return Deferred != null || IsSignal; } }

        public bool IsValidResult { get { return Result != null && !IsSignal; } }

        public override string ToString()
        {
            string sP = Deferred != null ? String.Format( "Deferred = {0}", Deferred.Expr ) : null;
            string sR = Result != null ? String.Format( "Result = {0}", Result ) : null;
            if( sP == null ) return sR != null ? sR : "(Unknown)";
            if( sR == null ) return sP;
            return sP + ", " + sR;
        }
    }

}
