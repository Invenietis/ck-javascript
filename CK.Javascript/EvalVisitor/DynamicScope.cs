using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Javascript
{

    class DynamicScope
    {
        class Entry
        {
            public Entry Next;
            public RefRuntimeObj O;
            public Entry( Entry n, RefRuntimeObj o = null ) 
            { 
                Next = n;
                O = o ?? new RefRuntimeObj();
            }
        }
        readonly Dictionary<AccessorDeclVarExpr,Entry> _vars;

        public DynamicScope()
        {
            _vars = new Dictionary<AccessorDeclVarExpr, Entry>();
        }

        public RefRuntimeObj Register( AccessorDeclVarExpr local )
        {
            Entry e;
            if( _vars.TryGetValue( local, out e ) )
            {
                if( e.O == null ) e.O = new RefRuntimeObj();
                else if( e.Next == null ) e = e.Next = new Entry( null );
                else e = e.Next = new Entry( e.Next );
            }
            else _vars.Add( local, e = new Entry( null ) );
            return e.O;
        }

        public RefRuntimeObj Register( Closure c )
        {
            Entry e;
            if( _vars.TryGetValue( c.Variable, out e ) )
            {
                if( e.O == null ) e.O = c.Ref;
                else if( e.Next == null ) e.Next = new Entry( null, c.Ref );
                else e = e.Next = new Entry( e.Next, c.Ref );
            }
            else _vars.Add( c.Variable, new Entry( null, c.Ref ) );
            return c.Ref;
        }

        public void Unregister( AccessorDeclVarExpr local )
        {
            Entry e;
            if( _vars.TryGetValue( local, out e ) )
            {
                if( e.Next != null )
                {
                    e.Next = e.Next.Next;
                    return;
                }
                if( e.O != null )
                {
                    e.O = null;
                    return;
                }
            }
            throw new InvalidOperationException( String.Format( "Unregistering non registered '{0}'.", local.Name ) );
        }
        
        public RefRuntimeObj FindRegistered( AccessorDeclVarExpr r )
        {
            Entry e;
            if( _vars.TryGetValue( r, out e ) ) return (e.Next ?? e).O;
            throw new CKException( "Unregistered variable '{0}'.", r.Name );
        }
    }
}
