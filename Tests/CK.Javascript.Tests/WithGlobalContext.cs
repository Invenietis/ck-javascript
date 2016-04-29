#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Javascript.Tests\WithGlobalContext.cs) is part of CiviKey. 
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
using System.Threading.Tasks;
using NUnit.Framework;

namespace CK.Javascript.Tests
{
    public class WithGlobalContext
    {
        class Context : GlobalContext
        {
            public double [] Array = new double[0];

            public override void Visit( IEvalVisitor v, IAccessorFrame frame )
            {
                IAccessorFrame frameArray = frame.MatchMember( "Array" );
                if( frameArray != null )
                {
                    AccessorIndexerExpr indexer = frameArray.Expr as AccessorIndexerExpr;
                    if( indexer != null )
                    {
                        v.VisitExpr( indexer.Index );
                        if( !v.HasError() )
                        {
                            if( v.Current.Type != "number" )
                            {
                                frameArray.SetRuntimeError( "Number expected." );
                            }
                            else
                            {
                                int i = JSSupport.ToInt32( v.Current.ToDouble() );
                                if( i < 0 || i >= Array.Length ) frameArray.SetRuntimeError( "Index out of range." );
                                else frameArray.SetResult( CreateNumber( Array[i] ) );
                            }
                        }
                    }
                }
                base.Visit( v, frame );
            }
        }

        [Test]
        public void AccessToIntrinsicArray()
        {
            RuntimeObj o = EvalVisitor.Evaluate( "Array[0]" );
            Assert.That( o is RuntimeError );
            var ctx = new Context();
            o = EvalVisitor.Evaluate( "Array[0]", ctx );
            Assert.That( ((RuntimeError)o).Message, Is.EqualTo( "Index out of range." ) );
            ctx.Array = new[] { 1.2 };
            o = EvalVisitor.Evaluate( "Array[-1]", ctx );
            Assert.That( ((RuntimeError)o).Message, Is.EqualTo( "Index out of range." ) );
            o = EvalVisitor.Evaluate( "Array[2]", ctx );
            Assert.That( ((RuntimeError)o).Message, Is.EqualTo( "Index out of range." ) );
            o = EvalVisitor.Evaluate( "Array[0]", ctx );
            Assert.That( o is JSEvalNumber );
            Assert.That( o.ToDouble(), Is.EqualTo( 1.2 ) );
            ctx.Array = new[] { 3.4, 5.6 };
            o = EvalVisitor.Evaluate( "Array[0] + Array[1] ", ctx );
            Assert.That( o is JSEvalNumber );
            Assert.That( o.ToDouble(), Is.EqualTo( 3.4 + 5.6 ) );
        }

    }
}
