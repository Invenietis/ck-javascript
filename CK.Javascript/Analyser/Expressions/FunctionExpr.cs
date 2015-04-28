using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using CK.Core;
using System.Diagnostics;

namespace CK.Javascript
{

    public class FunctionExpr : Expr
    {
        public FunctionExpr( SourceLocation location, IReadOnlyList<AccessorDeclVarExpr> parameters, Expr body, AccessorDeclVarExpr name = null )
            : base( location, false )
        {
            if( parameters == null ) throw new ArgumentNullException();
            if( body == null ) throw new ArgumentNullException();
            Parameters = parameters;
            Name = name;
            Body = body;
        }

        public Expr Body { get; private set; }

        public AccessorDeclVarExpr Name { get; private set; }

        public IReadOnlyList<AccessorDeclVarExpr> Parameters { get; private set; }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( IExprVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

        public override string ToString()
        {
            string r = "function";
            if( Name != null ) r += ' ' + Name.Name;
            r += '(' + String.Join( ", ", Parameters.Select( e => e.Name ) ) + ')';
            return r + Body.ToString();
        }

    }

}
