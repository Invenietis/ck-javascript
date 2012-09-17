using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using CK.Core;
using System.Diagnostics;

namespace CK.Javascript
{
    public abstract class Expr
    {
        protected Expr( SourceLocation location )
        {
            Location = location;
        }

        public readonly SourceLocation Location;

        internal protected abstract T Accept<T>( IExprVisitor<T> visitor );
    }

    public class SyntaxErrorExpr : Expr
    {
        public static readonly SyntaxErrorExpr ReferenceErrorExpr = new SyntaxErrorExpr( SourceLocation.Empty, "Reference error." );
        public static readonly SyntaxErrorExpr ReservedErrorExpr = new SyntaxErrorExpr( SourceLocation.Empty, "Reserved." );

        public SyntaxErrorExpr( SourceLocation location, string errorMessageFormat, params object[] messageParameters )
            : base( location )
        {
            ErrorMessage = String.Format( errorMessageFormat, messageParameters );
        }

        public string ErrorMessage { get; private set; }

        public bool IsReferenceError
        {
            get { return this == ReferenceErrorExpr; }
        }

        public bool IsReserved
        {
            get { return this == ReservedErrorExpr; }
        }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( IExprVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

        public override string ToString()
        {
            return "Syntax: " + ErrorMessage;
        }
    }

    public class UnaryExpr : Expr
    {
        public UnaryExpr( SourceLocation location, JSParserToken type, Expr e )
            : base( location )
        {
            TokenType = type;
            Expression = e;
        }

        public JSParserToken TokenType { get; private set; }

        public Expr Expression { get; private set; }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( IExprVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

        public override string ToString()
        {
            return JSParser.Explain( TokenType ) + Expression.ToString();
        }
    }

    public class ConstantExpr : Expr
    {
        public static readonly ConstantExpr UndefinedExpr = new ConstantExpr( SourceLocation.Empty, JSSupport.Undefined );
        
        public ConstantExpr( SourceLocation location, object value )
            : base( location )
        {
            Value = value;
        }

        public object Value { get; private set; }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( IExprVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

        public override string ToString()
        {
            return Value != null ? Value.ToString() : "(null)";
        }
    }

    public abstract class AccessorExpr : Expr
    {
        protected AccessorExpr( SourceLocation location, Expr left )
            : base( location )
        {
            if( left == null ) throw new ArgumentNullException( "left" );
            Left = left;
        }

        public Expr Left { get; private set; }

        public virtual bool IsMember( string memberName )
        {
            return false;
        }

        public virtual IReadOnlyList<Expr> CallArguments
        {
            get { return null; }
        }
    }

    public class AccessorMemberExpr : AccessorExpr
    {
        /// <summary>
        /// Creates a new <see cref="AccessorMemberExpr"/> for a field or a variable.
        /// </summary>
        /// <param name="left">Left scope. Must not be null.</param>
        /// <param name="fieldOrVariableName">Field, variable or function name.</param>
        public AccessorMemberExpr( SourceLocation location, Expr left, string fieldOrVariableName )
            : base( location, left )
        {
            Name = fieldOrVariableName;
        }

        public string Name { get; private set; }

        public bool IsUnbound { get { return Left == SyntaxErrorExpr.ReferenceErrorExpr; } }

        public override bool IsMember( string memberName )
        {
            return memberName == Name;
        }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( IExprVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

        public override string ToString()
        {
            return Left.ToString() + '.' + Name;
        }

    }

    public class AccessorIndexerExpr : AccessorExpr
    {
        /// <summary>
        /// Creates a new <see cref="AccessorIndexerExpr"/>. 
        /// One [Expr] is enough.
        /// </summary>
        /// <param name="left">Left scope. Must not be null.</param>
        /// <param name="index">Index for the indexer.</param>
        public AccessorIndexerExpr( SourceLocation location, Expr left, Expr index )
            : base( location, left )
        {
            Index = index;
        }

        public Expr Index { get; private set; }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( IExprVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

        public override string ToString()
        {
            return Left.ToString() + '[' + Index.ToString() + ']';
        }

    }

    public class AccessorCallExpr : AccessorExpr
    {
        IReadOnlyList<Expr> _args;

        /// <summary>
        /// Creates a new <see cref="AccessorCallExpr"/>: 0 or n arguments can be provided.
        /// </summary>
        /// <param name="left">Left scope. Must not be null.</param>
        /// <param name="arguments">When null, it is normalized to an empty list.</param>
        public AccessorCallExpr( SourceLocation location, Expr left, IReadOnlyList<Expr> arguments = null )
            : base( location, left )
        {
            _args = arguments ?? ReadOnlyListEmpty<Expr>.Empty;
        }

        public override IReadOnlyList<Expr> CallArguments { get { return _args; } }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( IExprVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder( Left.ToString() );
            b.Append( '(' );
            bool first = true;
            foreach( var e in CallArguments ) 
            {
                if( first ) first = false;
                else b.Append( ',' );
                b.Append( e.ToString() );
            }
            b.Append( ')' );
            return b.ToString();
        }
    }

    public class BinaryExpr : Expr
    {
        public BinaryExpr( SourceLocation location, Expr left, JSParserToken binaryOperatorToken, Expr right )
            : base( location )
        {
            Left = left;
            BinaryOperatorToken = binaryOperatorToken;
            Right = right;
        }

        public Expr Left { get; private set; }

        public JSParserToken BinaryOperatorToken { get; private set; }

        public Expr Right { get; private set; }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( IExprVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

        public override string ToString()
        {
            return Left.ToString() + JSParser.Explain( BinaryOperatorToken ) + Right.ToString();
        }
    }

    public class IfExpr : Expr
    {
        public IfExpr( SourceLocation location, bool isTernary, Expr condition, Expr whenTrue, Expr whenFalse )
            : base( location )
        {
            IsTernaryOperator = isTernary;
            Condition = condition;
            WhenTrue = whenTrue;
            WhenFalse = whenFalse;
        }

        /// <summary>
        /// Gets whether this is a ternary ?: expression (<see cref="WhenFalse"/> necessarily exists). 
        /// Otherwise, it is an if statement: <see cref="WhenTrue"/> and WhenFalse are
        /// Blocks (and WhenFalse may be null).
        /// </summary>
        public bool IsTernaryOperator { get; private set; }

        public Expr Condition { get; private set; }

        public Expr WhenTrue { get; private set; }

        public Expr WhenFalse { get; private set; }

        [DebuggerStepThrough]
        internal protected override T Accept<T>( IExprVisitor<T> visitor )
        {
            return visitor.Visit( this );
        }

        public override string ToString()
        {
            string s = "if(" + Condition.ToString() + ") then {" + WhenTrue.ToString() + "}";
            if( WhenFalse != null ) s += " else {" + WhenFalse.ToString() + "}";
            return s;
        }
    }


}
