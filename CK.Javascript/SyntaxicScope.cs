using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CK.Javascript
{
    public class StaticSyntaxicScope : ISyntaxicScope
    {
        public static readonly string RootScopeName = "<<global>>";

        Dictionary<string,List<ScopedExpr>> _locals;
        Stack<ScopeId> _scopes;

        class ScopeId
        {
            public ScopeId( SourceLocation l )
            {
                ScopeLocation = l;
            }
            public SourceLocation ScopeLocation { get; private set; }
            public bool IsClosed { get; set; }
        }

        class ScopedExpr
        {
            public ScopeId Scope { get; set; }
            public Expr Expr { get; set; }
            public bool IsReserved { get { return Expr == SyntaxErrorExpr.ReservedErrorExpr; } }
        }

        public StaticSyntaxicScope()
        {
            _locals = new Dictionary<string, List<ScopedExpr>>();
            _scopes = new Stack<ScopeId>();
            _scopes.Push( new ScopeId( new SourceLocation() { Source = RootScopeName } ) );
        }

        public Expr Find( string name )
        {
            Expr e = null;
            List<ScopedExpr> candidates;
            if( _locals.TryGetValue( name, out candidates ) )
            {
                // Finds the last one in the list that is opened: it is necessarily 
                // the current one.
                ScopedExpr se = candidates.LastOrDefault( s => !s.Scope.IsClosed );
                if( se != null ) e = se.Expr;
            }
            return e;
        }

        public void Reserve( string name )
        {
            Define( name, SyntaxErrorExpr.ReservedErrorExpr );
        }

        public Expr Define( string name, Expr e )
        {
            if( String.IsNullOrWhiteSpace( name ) ) throw new ArgumentException( "name" );
            if( e == null ) throw new ArgumentException( "e" );

            ScopeId currentScope = _scopes.Peek();
            Debug.Assert( !currentScope.IsClosed );
            ScopedExpr se = null;

            List<ScopedExpr> candidates;
            if( !_locals.TryGetValue( name, out candidates ) )
            {
                _locals.Add( name, candidates = new List<ScopedExpr>() );
            }
            else
            {
                // Finds the last one in the list that is opened.
                int idx = candidates.FindLastIndex( s => !s.Scope.IsClosed );
                if( idx >= 0 )
                {
                    se = candidates[idx];
                    if( se.IsReserved ) return SyntaxErrorExpr.ReservedErrorExpr;
                    if( se.Scope == currentScope ) return new SyntaxErrorExpr( e.Location, String.Format( "{0} is already defined at {1}.", name, se.Expr.Location ) );
                    // Tries to reuse a closed entry.
                    idx = candidates.FindIndex( idx, s => s.Scope.IsClosed );
                    se = idx > 0 ? candidates[idx] : null;
                }
            }
            if( se == null )
            {
                se = new ScopedExpr();
                candidates.Add( se );
            }
            se.Scope = currentScope;
            se.Expr = e;
            return e;
        }

        public ISyntaxicScope OpenScope( SourceLocation location )
        {
            _scopes.Push( new ScopeId( location ) );
            return this;
        }

        public void CloseScope()
        {
            if( _scopes.Count == 1 ) throw new InvalidOperationException( "Closing root scope." );
            _scopes.Pop().IsClosed = true;
        }
    }
}
