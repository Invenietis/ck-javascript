using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Javascript
{
    public interface ISyntaxicScope
    {
        /// <summary>
        /// Obtains a named <see cref="Expx"/> if it exists. Null otherwise.
        /// </summary>
        /// <param name="name">Name in the scope.</param>
        /// <returns>Null if not found.</returns>
        Expr Find( string name );

        /// <summary>
        /// Reserves a name in the scope by creating a <see cref="SyntaxErrorExpr.ReservedErrorExpr"/>.
        /// </summary>
        /// <param name="name">Name to reserve.</param>
        void Reserve( string name );

        /// <summary>
        /// Registers a named <see cref="Expr"/> in the scope.
        /// </summary>
        /// <param name="name">Name to define.</param>
        /// <param name="e">Associated expression.</param>
        /// <returns>
        /// The given <paramref name="e"/> or a <see cref="SyntaxErrorExpr"/> that can be <see cref="SyntaxErrorExpr.ReservedErrorExpr"/>
        /// if the name is reserved.
        /// </returns>
        Expr Define( string name, Expr e );

        /// <summary>
        /// Opens a new subordinated scope.
        /// </summary>
        /// <param name="location">The location in the source.</param>
        /// <returns>The opened scope.</returns>
        ISyntaxicScope OpenScope( SourceLocation location );

        /// <summary>
        /// Closes the current scope.
        /// </summary>
        void CloseScope();
    }
}
