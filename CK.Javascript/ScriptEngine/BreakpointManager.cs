using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Javascript
{
    public class BreakpointManager
    {
        readonly HashSet<Expr> _breakpoints;
        bool _breakAlways;

        public BreakpointManager()
        {
            _breakpoints = new HashSet<Expr>();
        }

        protected ISet<Expr> Breakpoints
        {
            get { return _breakpoints; }
        }

        public void ClearBreakpoints()
        {
            _breakpoints.Clear();
        }

        public bool BreakAlways
        {
            get { return _breakAlways; }
            set { _breakAlways = value; }
        }

        public bool AddBreakpoint( Expr e )
        {
            if( !e.IsBreakable ) return false;
            return _breakpoints.Add( e );
        }
        
        public bool RemoveBreakpoint( Expr e )
        {
            if( !e.IsBreakable ) return false;
            return _breakpoints.Remove( e );
        }

        public virtual bool MustBreak( Expr e )
        {
            return e.IsBreakable && (_breakAlways || _breakpoints.Contains( e ));
        }

    }
}
