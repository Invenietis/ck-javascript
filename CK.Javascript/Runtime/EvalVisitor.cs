using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CK.Javascript
{
    public class EvalVisitor : ExprVisitor, IEvalVisitor
    {
        GlobalContext _global;
        RuntimeObj _current;
        RuntimeError _currentError;
        VisitFrame _firstFrame;
        VisitFrame _currentFrame;

        public class VisitFrame : IDisposable
        {          
            internal EvalVisitor _visitor;
            Expr _expr;
            VisitFrame _prev;
            VisitFrame _next;

            internal protected VisitFrame( EvalVisitor visitor, Expr e )
            {
                _visitor = visitor;
                _prev = visitor._currentFrame;
                if( _prev != null ) _prev._next = this;
                else visitor._firstFrame = this;
                visitor._currentFrame = this;
                _expr = e;
            }

            public Expr Expr
            {
                get { return _expr; }
            }

            public VisitFrame NextFrame
            {
                get { return _next; }
            }

            public VisitFrame PrevFrame
            {
                get { return _prev; }
            }

            void IDisposable.Dispose()
            {
                Dispose( null );
            }

            protected virtual void Dispose( RuntimeObj result )
            {
                _visitor._currentFrame = _prev;
                if( _prev != null ) _prev._next = null;
                else _visitor._firstFrame = null;
                if( result != null && !(result is RuntimeError) )
                {
                    _visitor._current = result;
                }
            }
        }

        public EvalVisitor( GlobalContext context )
        {
            if( context == null ) throw new ArgumentNullException( "context" );
            _global = context;
        }

        public GlobalContext Global 
        {
            get { return _global; }
        }

        public RuntimeObj Current
        {
            get { return _current; }
        }

        public RuntimeError CurrentError
        {
            get { return _currentError; }
        }

        public bool HasError
        {
            get { return _currentError != null; }
        }

        internal RuntimeError SetRuntimeError( Expr e, string message )
        {
            _currentError = _global.CreateRuntimeError( e, message, _currentError );
            _current = _currentError;
            return _currentError;
        }

        internal RuntimeError SetAccessorError( AccessorExpr e )
        {
            _currentError = _global.CreateAccessorError( e, _currentError );
            _current = _currentError;
            return _currentError;
        }

        public override Expr Visit( AccessorMemberExpr e )
        {
            using( var frame = EnterAccessorFrame( e ) )
            {
                if( e.Left == SyntaxErrorExpr.ReferenceErrorExpr )
                {
                    _global.Visit( this, frame );
                    if( !frame.HasResultOrError ) frame.SetAccessorError();
                }
                else
                {
                    VisitAccessorLeft( frame );
                }
            }
            return e;
        }

        public override Expr Visit( AccessorIndexerExpr e )
        {
            using( var frame = EnterAccessorFrame( e ) )
            {
                VisitAccessorLeft( frame );
            }
            return e;
        }

        public override Expr Visit( AccessorCallExpr e )
        {
            using( var frame = EnterAccessorFrame( e ) )
            {
                VisitAccessorLeft( frame );
            }
            return e;
        }

        void VisitAccessorLeft( AccessorFrame frame )
        {
            VisitExpr( frame.Expr.Left );
            if( !frame.HasResultOrError )
            {
                _current.Visit( this, frame );
                if( !frame.HasResultOrError ) frame.SetAccessorError();
            }
        }

        public override Expr Visit( BinaryExpr e )
        {
            using( EnterVisitFrame( e ) )
            {
                VisitExpr( e.Left );
                if( HasError ) return e;

                // Do not evaluate right expression if it is useless.
                if( (e.BinaryOperatorToken == JSTokeniserToken.And && !_current.ToBoolean())
                    || (e.BinaryOperatorToken == JSTokeniserToken.Or && _current.ToBoolean()) )
                {
                    return e;
                }
                RuntimeObj left = _current;
                VisitExpr( e.Right );
                if( HasError ) return e;
                RuntimeObj right = _current;

                if( e.BinaryOperatorToken == JSTokeniserToken.And || e.BinaryOperatorToken == JSTokeniserToken.Or )
                {
                    _current = right;
                }
                else if( (e.BinaryOperatorToken & JSTokeniserToken.IsCompareOperator) != 0 )
                {
                    #region ==, <, >, <=, >=, !=, === and !==
                    int compareValue;
                    switch( (int)e.BinaryOperatorToken & 15 )
                    {
                        case (int)JSTokeniserToken.StrictEqual & 15:
                            {
                                _current = _global.CreateBoolean( new RuntimeObjComparer( left, right ).AreEqualStrict( _global ) );
                                break;
                            }
                        case (int)JSTokeniserToken.StrictDifferent & 15:
                            {
                                _current = _global.CreateBoolean( !new RuntimeObjComparer( left, right ).AreEqualStrict( _global ) );
                                break;
                            }
                        case (int)JSTokeniserToken.Greater & 15:
                            {
                                _current = _global.CreateBoolean( new RuntimeObjComparer( left, right ).Compare( _global, out compareValue ) && compareValue > 0 );
                                break;
                            }
                        case (int)JSTokeniserToken.GreaterOrEqual & 15:
                            {
                                _current = _global.CreateBoolean( new RuntimeObjComparer( left, right ).Compare( _global, out compareValue ) && compareValue >= 0 );
                                break;
                            }
                        case (int)JSTokeniserToken.Less & 15:
                            {
                                _current = _global.CreateBoolean( new RuntimeObjComparer( left, right ).Compare( _global, out compareValue ) && compareValue < 0 );
                                break;
                            }
                        case (int)JSTokeniserToken.LessOrEqual & 15:
                            {
                                _current = _global.CreateBoolean( new RuntimeObjComparer( left, right ).Compare( _global, out compareValue ) && compareValue <= 0 );
                                break;
                            }
                        case (int)JSTokeniserToken.Equal & 15:
                            {
                                _current = _global.CreateBoolean( new RuntimeObjComparer( left, right ).AreEqual( _global ) );
                                break;
                            }
                        case (int)JSTokeniserToken.Different & 15:
                            {
                                _current = _global.CreateBoolean( !new RuntimeObjComparer( left, right ).AreEqual( _global ) );
                                break;
                            }
                        default: throw new InvalidOperationException( "Unsupported operator: " + ((int)e.BinaryOperatorToken & 15) );
                    }
                    #endregion
                }
                else if( (e.BinaryOperatorToken & JSTokeniserToken.IsBinaryOperator) != 0 )
                {
                    #region |, ^, &, >>, <<, >>>, +, -, /, * and %.
                    switch( (int)e.BinaryOperatorToken & 15 )
                    {
                        case (int)JSTokeniserToken.Plus & 15:
                            {
                                RuntimeObj l = left.ToPrimitive( _global );
                                RuntimeObj r = right.ToPrimitive( _global );

                                if( ReferenceEquals( l.Type, RuntimeObj.TypeString ) || ReferenceEquals( r.Type, RuntimeObj.TypeString ) )
                                {
                                    _current = _global.CreateString( String.Concat( l.ToString(), r.ToString() ) );
                                }
                                else
                                {
                                    _current = _global.CreateNumber( l.ToDouble() + r.ToDouble() );
                                }
                                break;
                            }
                        case (int)JSTokeniserToken.Minus & 15:
                            {
                                _current = _global.CreateNumber( left.ToDouble() - right.ToDouble() );
                                break;
                            }
                        case (int)JSTokeniserToken.Mult & 15:
                            {
                                _current = _global.CreateNumber( left.ToDouble() * right.ToDouble() );
                                break;
                            }
                        case (int)JSTokeniserToken.Divide & 15:
                            {
                                _current = _global.CreateNumber( left.ToDouble() / right.ToDouble() );
                                break;
                            }
                        case (int)JSTokeniserToken.Modulo & 15:
                            {
                                if( right == _global.Zero || left == _global.NegativeInfinity || left == _global.Infinity )
                                {
                                    _current = _global.NaN;
                                }
                                else if( left == _global.NegativeInfinity || left == _global.Infinity )
                                {
                                    _current = right;
                                }
                                else
                                {
                                    _current = _global.CreateNumber( left.ToDouble() % right.ToDouble() );
                                }
                                break;
                            }
                        case (int)JSTokeniserToken.BitwiseAnd & 15:
                            {
                                Int64 l = JSSupport.ToInt64( left.ToDouble() );
                                Int64 r = JSSupport.ToInt64( right.ToDouble() );
                                _current = _global.CreateNumber( l & r );
                                break;
                            }
                        case (int)JSTokeniserToken.BitwiseOr & 15:
                            {
                                Int64 l = JSSupport.ToInt64( left.ToDouble() );
                                Int64 r = JSSupport.ToInt64( right.ToDouble() );
                                _current = _global.CreateNumber( l | r );
                                break;
                            }
                        case (int)JSTokeniserToken.BitwiseXOr & 15:
                            {
                                Int64 l = JSSupport.ToInt64( left.ToDouble() );
                                Int64 r = JSSupport.ToInt64( right.ToDouble() );
                                _current = _global.CreateNumber( l ^ r );
                                break;
                            }
                        case (int)JSTokeniserToken.BitwiseShiftLeft & 15:
                            {
                                BitwiseShift( left, right, ( i, shift ) => i << shift );
                                break;
                            }
                        case (int)JSTokeniserToken.BitwiseShiftRight & 15:
                            {
                                BitwiseShift( left, right, ( i, shift ) => i >> shift );
                                break;
                            }
                        case (int)JSTokeniserToken.BitwiseShiftRightNoSignBit & 15:
                            {
                                BitwiseShift( left, right, ( i, shift ) => (long)((ulong)i >> shift) );
                                break;
                            }
                        default: throw new InvalidOperationException( "Unsupported operator: " + ((int)e.BinaryOperatorToken & 15) );
                    }
                    #endregion
                }
                else throw new NotSupportedException( "Unsupported binary operator: " + JSTokeniser.Explain( e.BinaryOperatorToken ) );
            }
            return e;
        }

        void BitwiseShift( RuntimeObj left, RuntimeObj right, Func<Int64, int, Int64> f )
        {
            Int64 lN = JSSupport.ToInt64( left.ToDouble() );
            if( lN == 0 )
            {
                _current = _global.Zero;
            }
            else
            {
                double dR = right.ToDouble();
                if( Double.IsNaN( dR ) || dR > 64 )
                {
                    _current = _global.CreateNumber( lN );
                }
                else if( dR < 0 )
                {
                    _current = _global.Zero;
                }
                else
                {
                    int shift = Convert.ToInt32( dR );
                    _current = _global.CreateNumber( f( lN, shift ) );
                }
            }
        }

        public override Expr Visit( ConstantExpr e )
        {
            if( e.Value == null || e.Value is string ) _current = _global.CreateString( (string)e.Value );
            else if( e.Value is Double ) _current = _global.CreateNumber( (Double)e.Value );
            else if( e.Value is Boolean ) _current = _global.CreateBoolean( (Boolean)e.Value );
            else throw new NotSupportedException( "Unsupported JS type: " + e.Value.GetType().Name );
            return e;
        }

        public override Expr Visit( IfExpr e )
        {
            using( EnterVisitFrame( e ) )
            {
                VisitExpr( e.Condition );
                if( HasError ) return e;
                if( _current.ToBoolean() )
                {
                    VisitExpr( e.WhenTrue );
                }
                else if( e.WhenFalse != null )
                {
                    VisitExpr( e.WhenFalse );
                }
                else _current = RuntimeObj.Undefined;
            }
            return e;
        }

        public override Expr Visit( UnaryExpr e )
        {
            using( EnterVisitFrame( e ) )
            {
                VisitExpr( e.Expression );
                if( HasError ) return e;

                // Minus and Plus are classified as a binary operator.
                // Handle thoss special cases here.
                if( e.TokenType == JSTokeniserToken.Minus )
                {
                    _current = _global.CreateNumber( -_current.ToDouble() );
                }
                else if( e.TokenType == JSTokeniserToken.Plus )
                {
                    _current = _global.CreateNumber( _current.ToDouble() );
                }
                else
                {
                    switch( (int)e.TokenType & 15 )
                    {
                        case (int)JSTokeniserToken.Not & 15:
                            {
                                _current = _global.CreateBoolean( !_current.ToBoolean() );
                                break;
                            }
                        case (int)JSTokeniserToken.BitwiseNot & 15:
                            {
                                _current = _global.CreateNumber( ~JSSupport.ToInt64( _current.ToDouble() ) );
                                break;
                            }
                        case (int)JSTokeniserToken.TypeOf & 15:
                            {
                                // Well known Javascript bug: typeof null === "object".
                                if( _current == RuntimeObj.Null ) _current = _global.CreateString( RuntimeObj.TypeObject );
                                else _current = _global.CreateString( _current.Type );
                                break;
                            }
                        case (int)JSTokeniserToken.Void & 15:
                            {
                                _current = RuntimeObj.Undefined; 
                                break;
                            }
                        default: throw new InvalidOperationException( "Unsupported unary operator: " + ((int)e.TokenType & 15) );
                    }
                }
            }
            return e;
        }

        public override Expr Visit( SyntaxErrorExpr e )
        {
            SetRuntimeError( e, e.ErrorMessage );
            return e;
        }

        protected virtual VisitFrame EnterVisitFrame( Expr e )
        {
            return new VisitFrame( this, e );
        }

        protected virtual AccessorFrame EnterAccessorFrame( AccessorExpr e )
        {
            return new AccessorFrame( this, e );
        }


    }
}
