using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Javascript
{
    public enum ScriptEngineStatus
    {
        None = 0,
        IsFinished = 1,
        IsPending = 2,
        IsError = 4
    }

    public interface IScriptEngineResult : IDisposable
    {
        RuntimeObj Result { get; }

        ScriptEngineStatus Status { get; }

        void Continue();

    }
}
