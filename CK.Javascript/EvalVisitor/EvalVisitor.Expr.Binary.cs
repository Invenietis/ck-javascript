#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Javascript\EvalVisitor\EvalVisitor.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using CK.Core;
using System.Collections.ObjectModel;

namespace CK.Javascript
{

    public partial class EvalVisitor
    {
        class BinaryExprFrame : Frame<BinaryExpr>
        {
            PExpr _left;
            PExpr _right;
            
            public BinaryExprFrame( EvalVisitor evaluator, BinaryExpr e )
                : base( evaluator, e )
            {
            }

            protected override PExpr DoVisit()
            {
                if( (_left = Resolve( _left, Expr.Left )).IsPendingOrError ) return PendingOrError( _left );

                // Do not evaluate right expression if it is useless: short-circuit boolean evaluation.
                if( (Expr.BinaryOperatorToken == JSTokeniserToken.And && !_left.Result.ToBoolean())
                    || (Expr.BinaryOperatorToken == JSTokeniserToken.Or && _left.Result.ToBoolean()) )
                {
                    return SetResult( _left.Result );
                }

                if( (_right = Resolve( _right, Expr.Right )).IsPendingOrError ) return PendingOrError( _right );

                RuntimeObj left = _left.Result;
                RuntimeObj right = _right.Result;

                RuntimeObj result = right;

                if( Expr.BinaryOperatorToken != JSTokeniserToken.And && Expr.BinaryOperatorToken != JSTokeniserToken.Or )
                {
                    if( (Expr.BinaryOperatorToken & JSTokeniserToken.IsCompareOperator) != 0 )
                    {
                        #region ==, <, >, <=, >=, !=, === and !==
                        int compareValue;
                        switch( (int)Expr.BinaryOperatorToken & 15 )
                        {
                            case (int)JSTokeniserToken.StrictEqual & 15:
                                {
                                    result = Global.CreateBoolean( new RuntimeObjComparer( left, right ).AreEqualStrict( Global ) );
                                    break;
                                }
                            case (int)JSTokeniserToken.StrictDifferent & 15:
                                {
                                    result = Global.CreateBoolean( !new RuntimeObjComparer( left, right ).AreEqualStrict( Global ) );
                                    break;
                                }
                            case (int)JSTokeniserToken.Greater & 15:
                                {
                                    result = Global.CreateBoolean( new RuntimeObjComparer( left, right ).Compare( Global, out compareValue ) && compareValue > 0 );
                                    break;
                                }
                            case (int)JSTokeniserToken.GreaterOrEqual & 15:
                                {
                                    result = Global.CreateBoolean( new RuntimeObjComparer( left, right ).Compare( Global, out compareValue ) && compareValue >= 0 );
                                    break;
                                }
                            case (int)JSTokeniserToken.Less & 15:
                                {
                                    result = Global.CreateBoolean( new RuntimeObjComparer( left, right ).Compare( Global, out compareValue ) && compareValue < 0 );
                                    break;
                                }
                            case (int)JSTokeniserToken.LessOrEqual & 15:
                                {
                                    result = Global.CreateBoolean( new RuntimeObjComparer( left, right ).Compare( Global, out compareValue ) && compareValue <= 0 );
                                    break;
                                }
                            case (int)JSTokeniserToken.Equal & 15:
                                {
                                    result = Global.CreateBoolean( new RuntimeObjComparer( left, right ).AreEqual( Global ) );
                                    break;
                                }
                            case (int)JSTokeniserToken.Different & 15:
                                {
                                    result = Global.CreateBoolean( !new RuntimeObjComparer( left, right ).AreEqual( Global ) );
                                    break;
                                }
                            default:
                                {
                                    result = new RuntimeError( Expr, "Unsupported operator: " + ((int)Expr.BinaryOperatorToken & 15) );
                                    break;
                                }
                        }
                        #endregion
                    }
                    else if( (Expr.BinaryOperatorToken & JSTokeniserToken.IsBinaryOperator) != 0 )
                    {
                        #region |, ^, &, >>, <<, >>>, +, -, /, * and %.
                        switch( (int)Expr.BinaryOperatorToken & 15 )
                        {
                            case (int)JSTokeniserToken.Plus & 15:
                                {
                                    RuntimeObj l = left.ToPrimitive( Global );
                                    RuntimeObj rO = right.ToPrimitive( Global );

                                    if( ReferenceEquals( l.Type, RuntimeObj.TypeString ) || ReferenceEquals( rO.Type, RuntimeObj.TypeString ) )
                                    {
                                        result = Global.CreateString( String.Concat( l.ToString(), rO.ToString() ) );
                                    }
                                    else
                                    {
                                        result = Global.CreateNumber( l.ToDouble() + rO.ToDouble() );
                                    }
                                    break;
                                }
                            case (int)JSTokeniserToken.Minus & 15:
                                {
                                    result = Global.CreateNumber( left.ToDouble() - right.ToDouble() );
                                    break;
                                }
                            case (int)JSTokeniserToken.Mult & 15:
                                {
                                    result = Global.CreateNumber( left.ToDouble() * right.ToDouble() );
                                    break;
                                }
                            case (int)JSTokeniserToken.Divide & 15:
                                {
                                    result = Global.CreateNumber( left.ToDouble() / right.ToDouble() );
                                    break;
                                }
                            case (int)JSTokeniserToken.Modulo & 15:
                                {
                                    if( right == Global.Zero || left == Global.NegativeInfinity || left == Global.Infinity )
                                    {
                                        result = Global.NaN;
                                    }
                                    else if( left == Global.NegativeInfinity || left == Global.Infinity )
                                    {
                                        result = right;
                                    }
                                    else
                                    {
                                        result = Global.CreateNumber( left.ToDouble() % right.ToDouble() );
                                    }
                                    break;
                                }
                            case (int)JSTokeniserToken.BitwiseAnd & 15:
                                {
                                    Int64 l = JSSupport.ToInt64( left.ToDouble() );
                                    Int64 rO = JSSupport.ToInt64( right.ToDouble() );
                                    result = Global.CreateNumber( l & rO );
                                    break;
                                }
                            case (int)JSTokeniserToken.BitwiseOr & 15:
                                {
                                    Int64 l = JSSupport.ToInt64( left.ToDouble() );
                                    Int64 rO = JSSupport.ToInt64( right.ToDouble() );
                                    result = Global.CreateNumber( l | rO );
                                    break;
                                }
                            case (int)JSTokeniserToken.BitwiseXOr & 15:
                                {
                                    Int64 l = JSSupport.ToInt64( left.ToDouble() );
                                    Int64 rO = JSSupport.ToInt64( right.ToDouble() );
                                    result = Global.CreateNumber( l ^ rO );
                                    break;
                                }
                            case (int)JSTokeniserToken.BitwiseShiftLeft & 15:
                                {
                                    result = BitwiseShift( left, right, ( i, shift ) => i << shift );
                                    break;
                                }
                            case (int)JSTokeniserToken.BitwiseShiftRight & 15:
                                {
                                    result = BitwiseShift( left, right, ( i, shift ) => i >> shift );
                                    break;
                                }
                            case (int)JSTokeniserToken.BitwiseShiftRightNoSignBit & 15:
                                {
                                    result = BitwiseShift( left, right, ( i, shift ) => (long)((ulong)i >> shift) );
                                    break;
                                }
                            default:
                                {
                                    result = new RuntimeError( Expr, "Unsupported operator: " + ((int)Expr.BinaryOperatorToken & 15) );
                                    break;
                                }
                        }
                        #endregion
                    }
                    else
                    {
                        result = new RuntimeError( Expr, "Unsupported binary operator: " + JSTokeniser.Explain( Expr.BinaryOperatorToken ) );
                    }
                }
                return SetResult( result );
            }

            RuntimeObj BitwiseShift( RuntimeObj left, RuntimeObj right, Func<Int64, int, Int64> f )
            {
                Int64 lN = JSSupport.ToInt64( left.ToDouble() );
                if( lN == 0 )
                {
                    return Global.Zero;
                }
                double dR = right.ToDouble();
                if( Double.IsNaN( dR ) || dR > 64 )
                {
                    return Global.CreateNumber( lN );
                }
                if( dR < 0 )
                {
                    return Global.Zero;
                }
                int shift = Convert.ToInt32( dR );
                return Global.CreateNumber( f( lN, shift ) );
            }
        }

        public PExpr Visit( BinaryExpr e )
        {
            using( var f = new BinaryExprFrame( this, e ) ) return f.Visit();
        }

    }
}
