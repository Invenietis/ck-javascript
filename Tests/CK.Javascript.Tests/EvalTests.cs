#region LGPL License
/* ----------------------------------------------------------------------------
*  This file (EvalTests.cs) is part of CK-Javascript. 
*   
*  CK-Javascript is free software: you can redistribute it and/or modify 
*  it under the terms of the GNU Lesser General Public License as published 
*  by the Free Software Foundation, either version 3 of the License, or 
*  (at your option) any later version. 
*   
*  CK-Javascript is distributed in the hope that it will be useful, 
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
*  GNU Lesser General Public License for more details. 
*  You should have received a copy of the GNU Lesser General Public License 
*  along with CK-Javascript.  If not, see <http://www.gnu.org/licenses/>. 
*   
*  Copyright © 2013, 
*      Invenietis <http://www.invenietis.com>
*  All rights reserved. 
* -----------------------------------------------------------------------------*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CK.Javascript;

namespace CK.MultiPlan.Tests.Language
{
    [TestFixture]
    public class EvalTests
    {
        [Test]
        public void BasicNumbers()
        {
            RuntimeObj o;
            {
                o = Eval( "6" );
                Assert.That( o is JSEvalNumber );
                Assert.That( o.ToDouble(), Is.EqualTo( 6 ) );
            }
            {
                o = Eval( "6+++8" );
                Assert.That( o is RuntimeError );
            }
            {
                o = Eval( "(6+6)*3/4*2" );
                Assert.That( o is JSEvalNumber );
                Assert.That( o.ToDouble(), Is.EqualTo( (6.0 + 6.0) * 3.0 / 4.0 * 2.0 ) );
            }
            {
                o = Eval( "8*5/4+1-(100/5/4)" );
                Assert.That( o is JSEvalNumber );
                Assert.That( o.ToDouble(), Is.EqualTo( 8.0 * 5.0 / 4.0 + 1.0 - (100.0 / 5.0 / 4.0) ) );
            }
            {
                o = Eval( "8*5/4+1-(100/5/4) > 1 ? 14+56/7/2-4 : (14+13+12)/2*47/3" );
                Assert.That( o is JSEvalNumber );
                Assert.That( o.ToDouble(), Is.EqualTo( 14.0 + 56.0 / 7.0 / 2.0 - 4.0 ) );
            }
        }

        [Test]
        public void StringAndNumbers()
        {
            RuntimeObj o;
            {
                o = Eval( "7 + '45' / 2 * '10' / '4'" );
                Assert.That( o is JSEvalNumber );
                Assert.That( o.ToDouble(), Is.EqualTo( 7.0 + 45.0 / 2.0 * 10.0 / 4.0 ) );
            }
            {
                o = Eval( "'45' + 4 == '454'" );
                Assert.That( o is JSEvalBoolean );
                Assert.That( o.ToBoolean(), Is.True );
            }
            {
                o = Eval( "'45' <= '454'" );
                Assert.That( o is JSEvalBoolean );
                Assert.That( o.ToBoolean(), Is.True );
            }
            {
                o = Eval( "45 <= '454'" );
                Assert.That( o is JSEvalBoolean );
                Assert.That( o.ToBoolean(), Is.True );
            }
            {
                o = Eval( "'45' > 454" );
                Assert.That( o is JSEvalBoolean );
                Assert.That( o.ToBoolean(), Is.False );
            }
            {
                o = Eval( "'olivier' < 'spi'" );
                Assert.That( o is JSEvalBoolean );
                Assert.That( o.ToBoolean(), Is.True );
            }
        }

        [Test]
        public void BitwiseOnNumbers()
        {
            IsNumber( "7&3 == 3", 1, "=> 7&(3 == 3)" );
            IsBoolean( "(7&3) == 3" );
            IsBoolean( "((7&3)&1)+2 == (1&45)+2*1" );
            IsBoolean( "(1|2|8) == 1+2+8" );
            IsBoolean( "(1|2.56e2) == 2+'57'" );
            IsBoolean( "(1|2.56e2) !== 2+'57'" );

            IsNumber( "~7", -8 );
            IsNumber( "~(7|1)", -8 );
            IsNumber( "~7|1", -7 );

        }

        [Test]
        public void Ternary()
        {
            IsBoolean( "3 ? true : false", true );
            IsBoolean( "0 ? true : false", false );
            IsNumber( "'' ? 1+1 : 3+3", 6 );
            IsNumber( "' ' ? 1+1 : false", 2 );
            IsNumber( "'false' ? ~45*8 : 's'", ~45 * 8.0, "The string 'false' is true." );
        }

        [Test]
        public void Inequality()
        {
            {
                IsBoolean( "45 > 45", false );
                IsBoolean( "45 >= 45", true );
                IsBoolean( "46 > 45", true );
                IsBoolean( "45 > '45'", false );
                IsBoolean( "45 >= '45'", true );
                IsBoolean( "46 > '45'", true );
                IsBoolean( "'45' > 45", false );
                IsBoolean( "'45' >= 45", true );
                IsBoolean( "'45'+3 > 452", true );
                IsBoolean( "'45'+2 > 452", false );
                IsBoolean( "'45'+2 >= 452", true );
                IsBoolean( "'45DD' > 45", false );
                IsBoolean( "'45DD' >= 45", false );
                IsBoolean( "Infinity > 45", true );
                IsBoolean( "Infinity >= 45", true );
                IsBoolean( "Infinity > Infinity", false );
                IsBoolean( "Infinity >= Infinity", true );

                IsBoolean( "Infinity >= NaN", false );
                IsBoolean( "Infinity > NaN", false );
                IsBoolean( "0 >= NaN", false );
                IsBoolean( "0 > NaN", false );

                IsBoolean( "'z' > 'z'", false );
                IsBoolean( "'z' >= 'z'", true );
                IsBoolean( "'z' > 'a'", true );
                IsBoolean( "'z' > 'a'", true );
            }
            {
                IsBoolean( "45 < 45", false );
                IsBoolean( "45 <= 45", true );
                IsBoolean( "44 < 45", true );
                IsBoolean( "45 < '45'", false );
                IsBoolean( "45 <= '45'", true );
                IsBoolean( "'44' < 45", true );
                IsBoolean( "'45' < 45", false );
                IsBoolean( "'45' <= 45", true );
                IsBoolean( "'45'+1 < 452", true );
                IsBoolean( "'45'+2 < 452", false );
                IsBoolean( "'45'+2 <= 452", true );
                IsBoolean( "'45DD' < 45", false );
                IsBoolean( "'45DD' <= 45", false );
                IsBoolean( "-Infinity < 45", true );
                IsBoolean( "-Infinity <= 45", true );
                IsBoolean( "-Infinity < -Infinity", false );
                IsBoolean( "-Infinity <= -Infinity", true );

                IsBoolean( "-Infinity <= NaN", false );
                IsBoolean( "-Infinity < NaN", false );
                IsBoolean( "0 <= NaN", false );
                IsBoolean( "0 < NaN", false );

                IsBoolean( "'z' < 'z'", false );
                IsBoolean( "'z' <= 'z'", true );
                IsBoolean( "'a' < 'z'", true );
                IsBoolean( "'a' < 'z'", true );
            }

        }

        [Test]
        public void Equality()
        {
            IsBoolean( "45 == 45", true );
            IsBoolean( "45 == '45'", true );
            IsBoolean( "'45' == 45", true );
            IsBoolean( "'45'+2 == 452", true );
            IsBoolean( "'45DD' != 45", true );

            IsBoolean( "45 === 45", true );
            IsBoolean( "45 === '45'", false );
            IsBoolean( "'45' === 45", false );
            IsBoolean( "'45'+2 === 452", false );

            IsBoolean( "45 !== 45", false );
            IsBoolean( "45 !== '45'", true );
            IsBoolean( "'45' !== 45", true );
            IsBoolean( "'45'+2 !== 452", true );

            IsBoolean( "Infinity == Infinity", true );
            IsBoolean( "Infinity == 45/0", true );
            IsBoolean( "-Infinity == -45/0", true );
            IsBoolean( "NaN == NaN", false );
            IsBoolean( "NaN != NaN", true );
            IsBoolean( "Infinity != NaN", true );
            
            IsBoolean( "Infinity === Infinity", true );
            IsBoolean( "Infinity === 45/0", true );
            IsBoolean( "-Infinity === -45/0", true );
            IsBoolean( "NaN === NaN", false );
            IsBoolean( "NaN !== NaN", true );
            IsBoolean( "Infinity !== NaN", true );
        }

        [Test]
        public void CallFunc()
        {
            IsBoolean( "(400+50+3).toString() === '453'", true );
            IsBoolean( "(-98979).toString(2) === '-11000001010100011'", true );
            IsBoolean( "(14714).toString(3) === '202011222'", true );
            IsBoolean( "(-1.47e12).toString(9) === '-5175284306313'", true );

            IsBoolean( "(1.4756896725e12).toString(30) === '27e7t31k0'", true );
            IsBoolean( "(1.4756896725e12).toString(31) === '1mjn02pj9'", true );
            IsBoolean( "(1.4756896725e12).toString(32) === '1auavarpk'", true );
            IsBoolean( "(1.4756896725e12).toString(33) === '11kl9kf8l'", true );
            IsBoolean( "(1.4756896725e12).toString(34) === 's38se3kg'", true );
            IsBoolean( "(1.4756896725e12).toString(35) === 'mwqnd0lf'", true );
            IsBoolean( "(1.4756896725e12).toString(36) === 'itx7j2no'", true );
        }

        [Test]
        public void Dates()
        {
            //IsDate( "Date(2012,4,26)", new DateTime( 2012, 4, 26, 0, 0, 0, DateTimeKind.Utc ) );
            //IsDate( "Date(2012,4)", new DateTime( 2012, 4, 1, 0, 0, 0, DateTimeKind.Utc ) );
            //IsDate( "Date(2012)", new DateTime( 2012, 1, 1, 0, 0, 0, DateTimeKind.Utc ) );
            //IsDate( "Date(2012,-4,-26)", new DateTime( 2012, 1, 1, 0, 0, 0, DateTimeKind.Utc ) );

            //IsBoolean( "Date(2012) < Date(2013)", true );
            IsBoolean( "Date(2012) == Date(2012)", true );

            IsBoolean( "Date(2012,4,3) == Date(2012,4,3)", true );
            IsBoolean( "Date(2012,4,3) != Date(2012,4,3,1)", true );
        }

        void IsBoolean( string s, bool v = true, string msg = null )
        {
            RuntimeObj o = Eval( s );
            Assert.That( o is JSEvalBoolean );
            Assert.That( o.ToBoolean(), Is.EqualTo( v ), msg ?? s );
        }

        void IsDate( string s, DateTime v, string msg = null )
        {
            RuntimeObj o = Eval( s );
            Assert.That( o is JSEvalDate );
            Assert.That( ((JSEvalDate)o).CompareTo( v ), Is.EqualTo( 0 ), msg ?? s );
        }

        void IsNumber( string s, double v, string msg = null )
        {
            RuntimeObj o = Eval( s );
            Assert.That( o is JSEvalNumber );
            Assert.That( o.ToDouble(), Is.EqualTo( v ), msg ?? s );
        }

        static RuntimeObj Eval( string s )
        {
            JSTokeniser p = new JSTokeniser();
            p.Reset( s );
            ExprAnalyser a = new ExprAnalyser( new StaticSyntaxicScope() );
            Expr e = a.Analyse( p );
            EvalVisitor v = new EvalVisitor( new GlobalContext() );
            v.VisitExpr( e );
            return v.Current;
        }
    }
}
