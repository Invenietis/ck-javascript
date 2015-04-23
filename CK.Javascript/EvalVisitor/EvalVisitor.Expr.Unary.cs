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
        class UnaryExprFrame : Frame<UnaryExpr>
        {
            PExpr _expression;

            public UnaryExprFrame( EvalVisitor evaluator, UnaryExpr e )
                : base( evaluator, e )
            {
            }

            protected override PExpr DoVisit()
            {
                if( (_expression = Resolve( _expression, Expr.Expression )).IsPendingOrError ) return PendingOrError( _expression );

                RuntimeObj result = _expression.Result;
                // Minus and Plus are classified as a binary operator.
                // Handle those special cases here.
                if( Expr.TokenType == JSTokeniserToken.Minus )
                {
                    result = Global.CreateNumber( -result.ToDouble() );
                }
                else if( Expr.TokenType == JSTokeniserToken.Plus )
                {
                    result = Global.CreateNumber( result.ToDouble() );
                }
                else
                {
                    switch( (int)Expr.TokenType & 15 )
                    {
                        case (int)JSTokeniserToken.Not & 15:
                            {
                                result = Global.CreateBoolean( !result.ToBoolean() );
                                break;
                            }
                        case (int)JSTokeniserToken.BitwiseNot & 15:
                            {
                                result = Global.CreateNumber( ~JSSupport.ToInt64( result.ToDouble() ) );
                                break;
                            }
                        case (int)JSTokeniserToken.TypeOf & 15:
                            {
                                // Well known Javascript bug: typeof null === "object".
                                if( result == RuntimeObj.Null ) result = Global.CreateString( RuntimeObj.TypeObject );
                                else result = Global.CreateString( result.Type );
                                break;
                            }
                        case (int)JSTokeniserToken.Void & 15:
                            {
                                result = RuntimeObj.Undefined;
                                break;
                            }
                        default:
                            {
                                result = new RuntimeError( Expr, "Unsupported unary operator: " + ((int)Expr.TokenType & 15) );
                                break;
                            }
                    }
                }
                return SetResult( result );
            }
        }

        public PExpr Visit( UnaryExpr e )
        {
            using( var f = new UnaryExprFrame( this, e ) ) return f.Visit();
        }
    }
}
