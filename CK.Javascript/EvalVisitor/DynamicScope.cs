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
            public Entry( Entry n ) 
            { 
                Next = n;
                O = new RefRuntimeObj();
            }
        }
        readonly Dictionary<AccessorDeclVarExpr,Entry> _vars;

        public DynamicScope()
        {
            _vars = new Dictionary<AccessorDeclVarExpr, Entry>();
        }

        public void Register( AccessorDeclVarExpr local )
        {
            Entry e;
            if( _vars.TryGetValue( local, out e ) )
            {
                if( e.O == null ) e.O = new RefRuntimeObj();
                else if( e.Next == null ) e.Next = new Entry( null );
                else e.Next = new Entry( e.Next );
            }
            else _vars.Add( local, new Entry( null ) );
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
