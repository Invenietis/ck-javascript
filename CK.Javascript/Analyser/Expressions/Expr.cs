using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Javascript
{
    public abstract class Expr
    {
        protected Expr( SourceLocation location, bool isbreakable = false )
        {
            Location = location;
            IsBreakable = isbreakable;
        }

        public readonly bool IsBreakable;

        public readonly SourceLocation Location;

        internal protected abstract T Accept<T>( IExprVisitor<T> visitor );
    }
}
