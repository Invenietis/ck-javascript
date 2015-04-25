using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Javascript
{
    public class StaticScope
    {
        class Scope
        {
            public Scope NextScope;
            NameEntry _firstNamed;
            int _count;

            public Scope( Scope next )
            {
                NextScope = next;
            }

            internal void Add( NameEntry newOne, NameEntry first )
            {
                Debug.Assert( first == newOne || first.Next == newOne );
                newOne.Scope = this;
                newOne.NextInScope = _firstNamed;
                _firstNamed = first;
                ++_count;
            }

            internal IReadOnlyList<AccessorDeclVarExpr> RetrieveValues( StaticScope container, bool close )
            {
                if( _count == 0 ) return Util.EmptyArray<AccessorDeclVarExpr>.Empty;
                int i = _count;
                var all = new AccessorDeclVarExpr[i];
                NameEntry first = _firstNamed;
                for( ; ; )
                {
                    NameEntry e = first.Next ?? first;
                    Debug.Assert( e.E != null );
                    all[--i] = e.E;
                    if( close ) container.Unregister( first );
                    if( i == 0 ) break;
                    first = e.NextInScope;
                    Debug.Assert( first != null );
                }
                return all;
            }
        }

        class NameEntry
        {
            /// <summary>
            /// Next entry for the same name. 
            /// </summary>
            public NameEntry Next;
            
            /// <summary>
            /// Next entry in the same scope.
            /// </summary>
            public NameEntry NextInScope;
            
            /// <summary>
            /// The declared expression. Null if first declaration has been scoped out.
            /// </summary>
            public AccessorDeclVarExpr E;

            /// <summary>
            /// This is unfortunately required to support AllowLocalRedefinition = false in O(1).
            /// </summary>
            public Scope Scope;

            public NameEntry( NameEntry next, AccessorDeclVarExpr e )
            {
                Next = next;
                E = e;
            }
        }

        Scope _firstScope;
        readonly Dictionary<string,NameEntry> _vars;
        readonly bool _globalScope;
        bool _allowMasking;
        bool _disallowRegistration;
        bool _allowLocalRedefinition;

        /// <summary>
        /// Initializes a new <see cref="StaticScope"/>.
        /// </summary>
        /// <param name="globalScope">
        /// True to share declarations in a global default scope. By default, <see cref="OpenScope"/> must be called before calling <see cref="Declare"/>.
        /// </param>
        /// <param name="allowMasking">
        /// False to forbid masking (to work like in C#). By default declaration in a subordinated scope masks any declaration from upper levels (Javascript).
        /// </param>
        /// <param name="allowLocalRedefinition">
        /// True to allow redifinition of a name in the same scope (masking but in the currenly opened scope).
        /// This is allowed in javascript even with "use strict" but here it defaults to false.
        /// </param>
        public StaticScope( bool globalScope = false, bool allowMasking = true, bool allowLocalRedefinition = false )
        {
            _vars = new Dictionary<string, NameEntry>();
            _allowMasking = allowMasking;
            _allowLocalRedefinition = allowLocalRedefinition;
            _globalScope = globalScope;
            if( _globalScope ) _firstScope = new Scope( null );
        }

        /// <summary>
        /// Gets or sets whether masking is allowed (like in Javascript). 
        /// When masking is disallowed (like in C#), registering new entries returns a <see cref="SyntaxErrorExpr"/>
        /// instead of the registered expression.
        /// </summary>
        public bool AllowMasking
        {
            get { return _allowMasking; }
            set { _allowMasking = true; }
        }

        /// <summary>
        /// True to allow redifinition of a name in the same scope. 
        /// This is allowed in javascript even with "use strict" but here it defaults to false since I consider this a dangerous and useless feature.
        /// </summary>
        public bool AllowLocalRedefinition
        {
            get { return _allowLocalRedefinition; }
            set { _allowLocalRedefinition = value; }
        }

        /// <summary>
        /// Disallow any new registration.
        /// Defaults to false (typically sets to true to evaluate pure functions).
        /// </summary>
        public bool DisallowRegistration
        {
            get { return _disallowRegistration; }
            set { _disallowRegistration = true; }
        }

        /// <summary>
        /// Gets whether a global scope is opened.
        /// </summary>
        public bool GlobalScope
        {
            get { return _globalScope; }
        }

        /// <summary>
        /// Declares an expression in the current scope.
        /// </summary>
        /// <param name="name">Name of the expression.</param>
        /// <param name="e">The expression to register.</param>
        /// <returns>The expression to register or a syntax error if it can not be registered.</returns>
        public Expr Declare( string name, AccessorDeclVarExpr e )
        {
            if( _firstScope == null ) return new SyntaxErrorExpr( e.Location, "Invalid declaration (a scope must be opened first)." );
            if( _disallowRegistration ) return new SyntaxErrorExpr( e.Location, "Invalid declaration." );
            NameEntry first, newOne;
            if( _vars.TryGetValue( name, out first ) )
            {
                if( first.E == null )
                {
                    first.E = e;
                    newOne = first;
                }
                else if( _allowMasking )
                {
                    if( first.Next == null )
                    {
                        if( _allowLocalRedefinition || first.Scope != (_firstScope.NextScope ?? _firstScope) )
                        {
                            first.Next = newOne = new NameEntry( null, e );
                        }
                        else
                        {
                            return new SyntaxErrorExpr( e.Location, "Declaration conflicts with declaration at {0}.", first.E.Location );
                        }
                    }
                    else
                    {
                        if( _allowLocalRedefinition || first.Next.Scope != (_firstScope.NextScope ?? _firstScope) )
                        {
                            first.Next = newOne = new NameEntry( first.Next, e );
                        }
                        else return new SyntaxErrorExpr( e.Location, "Declaration conflicts with declaration at {0}.", first.Next.E.Location );
                    }
                }
                else
                {
                    return new SyntaxErrorExpr( e.Location, "Masking is not allowed: declaration conflicts with declaration at {0}.", first.E.Location );
                }
            }
            else _vars.Add( name, (first = newOne = new NameEntry( null, e )) );
            (_firstScope.NextScope ?? _firstScope).Add( newOne, first );
            return e;
        }

        void Unregister( NameEntry first )
        {
            if( first.Next != null )
            {
                first.Next = first.Next.Next;
            }
            else if( first.E != null )
            {
                first.E = null;
            }
        }

        /// <summary>
        /// Opens a new scope: any <see cref="Declare"/> will be done in this new scope.
        /// </summary>
        public void OpenScope()
        {
            if( _firstScope == null ) _firstScope = new Scope( null );
            else _firstScope.NextScope = new Scope( _firstScope.NextScope );
        }

        /// <summary>
        /// Closes the current scope and returns all the declared expressions in the order of their declarations.
        /// </summary>
        /// <returns>The declared expressions (an empty list if nothing has been declared).</returns>
        public IReadOnlyList<AccessorDeclVarExpr> CloseScope()
        {
            if( _firstScope == null ) throw new InvalidOperationException( "No Scope opened." );
            if( _firstScope != null && _firstScope.NextScope == null && _globalScope ) throw new InvalidOperationException( "The GlobalScope can not be closed." );
            Scope closing;
            if( _firstScope.NextScope == null )
            {
                closing = _firstScope;
                _firstScope = null;
            }
            else
            {
                closing = _firstScope.NextScope;
                _firstScope.NextScope = closing.NextScope;
            }
            return closing.RetrieveValues( this, true );
        }

        /// <summary>
        /// Gets the global scope content.
        /// </summary>
        /// <returns>The global scope. Empty if GlobalScope is false (or if nothing has been declared at the global scope).</returns>
        public IReadOnlyList<Expr> Globals
        {
            get { return _firstScope == null ? CKReadOnlyListEmpty<AccessorDeclVarExpr>.Empty : _firstScope.RetrieveValues( this, false ); }
        }

        /// <summary>
        /// Obtains a named <see cref="Expr"/> if it exists. Null otherwise.
        /// </summary>
        /// <param name="name">Name in the scope.</param>
        /// <returns>Null if not found.</returns>
        public Expr Find( string name )
        {
            NameEntry t;
            if( _vars.TryGetValue( name, out t ) ) return (t.Next ?? t).E;
            return null;
        }
    }

}
