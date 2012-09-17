using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.Javascript
{
    public class SyntaxErrorCollector : ExprVisitor
    {
        Action<SyntaxErrorExpr> _collector;
        Action<AccessorExpr> _unboundCollector;

        public SyntaxErrorCollector( Action<SyntaxErrorExpr> collector, Action<AccessorExpr> unboundCollector = null )
        {
            if( collector == null ) throw new ArgumentNullException( "collector" );
            _collector = collector;
            _unboundCollector = unboundCollector;
        }

        static public IReadOnlyList<SyntaxErrorExpr> Collect( Expr e, Action<AccessorExpr> unboundCollector = null )
        {
            List<SyntaxErrorExpr> collector = new List<SyntaxErrorExpr>();
            new SyntaxErrorCollector( collector.Add, unboundCollector ).VisitExpr( e );
            return collector.ToReadOnlyList();
        }

        public override Expr Visit( AccessorMemberExpr e )
        {
            if( _unboundCollector != null )
            {
                if( e.IsUnbound ) _unboundCollector( e );
                else VisitExpr( e.Left );
            }
            return e;
        }

        public override Expr Visit( SyntaxErrorExpr e )
        {
            _collector( e );
            return e;
        }
    }
}
