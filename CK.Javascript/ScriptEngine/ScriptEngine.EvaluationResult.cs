using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Javascript
{
    public partial class ScriptEngine
    {
        class EvaluationResult : IScriptEngineResult
        {
            ScriptEngine _engine;
            readonly EvalVisitor _ev;
            RuntimeObj _result;
            RuntimeError _error;
            ScriptEngineStatus _status;

            class FrameStack : ObservableCollection<IDeferedExpr>, IObservableReadOnlyList<IDeferedExpr> {}
            FrameStack _frameStack;


            public EvaluationResult( ScriptEngine e )
            {
                _engine = e;
                _ev = e._evaluator;
            }

            public RuntimeObj Result
            {
                get { return _result; }
            }

            public RuntimeError Error
            {
                get { return _error; }
            }

            public ScriptEngineStatus Status
            {
                get { return _status; }
            }

            public void Continue()
            {
                if( _engine == null ) throw new ObjectDisposedException( "EvaluationResult" );
                if( _status != ScriptEngineStatus.IsPending ) throw new InvalidOperationException();
                if( _ev.CurrentFrame != null )
                {
                    UpdateStatus( _ev.FirstFrame.StepOut() );
                }
            }

            public void UpdateStatus( PExpr r )
            {
                _error = (_result = r.Result) as RuntimeError;
                _status = ScriptEngineStatus.None;
                if( r.IsErrorResult ) _status |= ScriptEngineStatus.IsError;
                if( r.IsPending ) _status |= ScriptEngineStatus.IsPending;
                else _status |= ScriptEngineStatus.IsFinished;
            }

            public IObservableReadOnlyList<IDeferedExpr> EnsureFrameStack()
            {
                if( _frameStack == null )
                {
                    _frameStack = new FrameStack();
                    foreach( var f in _ev.Frames )
                    {
                        _frameStack.Add( f );
                    }
                }
                return _frameStack;
            }

            public void Dispose()
            {
                if( _engine != null )
                {
                    _ev.ResetCurrentEvaluation();
                    _engine._currentResult = null;
                    _engine = null;
                }
            }
        }
    }
}
