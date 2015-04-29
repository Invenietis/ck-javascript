#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Javascript.Tests\EvalTests.cs) is part of CiviKey. 
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
using NUnit.Framework;
using CK.Javascript;

namespace CK.Javascript.Tests
{
    [TestFixture]
    public class FunctionTests
    {
        [Test]
        public void functions_are_runtime_objects()
        {
            string s = @"function yo(a) { return 'yo' + a; }";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<JSEvalFunction>( o );
            var f = (JSEvalFunction)o;
            CollectionAssert.AreEqual( new[]{ "a" }, f.Expr.Parameters.Select( p => p.Name ).ToArray() );
        }

        [Test]
        public void functions_are_callable()
        {
            string s = @"function yo(a) { return 'yo' + a; }
                         yo('b');";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<JSEvalString>( o );
            Assert.That( o.ToString(), Is.EqualTo( "yob" ) );
        }

        [Test]
        public void functions_have_multiple_parameters_and_superfluous_actual_parameters_are_ignored()
        {
            string s = @"function F(a,b,c,d,e,f,g) { return a+b+c+d+e+f+g; }
                         F(1,2,3,4,5,6,7,8,9,10,11,12);";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<JSEvalNumber>( o );
            Assert.That( o.ToDouble(), Is.EqualTo( 1 + 2 + 3 + 4 + 5 + 6 + 7 ) );
        }

        [Test]
        public void functions_are_first_class_objects()
        {
            string s = @"
                            function gen() 
                            { 
                              return function(a,b) { return a+b; };
                            }
                            var f = gen();
                            f( 'x', 'y' );
                        ";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<JSEvalString>( o );
            Assert.That( o.ToString(), Is.EqualTo( "xy" ) );
        }

        [Test]
        public void closure_is_supported()
        {
            string s = @"
                        function next( s ) 
                        { 
                          var _seed = s; 
                          return function() { return ++_seed; };
                        }
                        var f = next(0);
                        f(0) + f(0) + f(0);
                        ";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<JSEvalNumber>( o );
            Assert.That( o.ToDouble(), Is.EqualTo( 1 + 2 + 3 ) );
        }

        [Test]
        public void closure_with_two_levels()
        {
            string s = @"
                        function next( s ) 
                        { 
                            var _seed = s; 
                            function oneMore() {
                                return function() { return ++_seed; };
                            }
                            return oneMore();
                        }
                        var f = next(0);
                        f(0) + f(0) + f(0);
                        f = next(0);
                        f(0) + f(0) + f(0) + f(0);
                        ";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<JSEvalNumber>( o );
            Assert.That( o.ToDouble(), Is.EqualTo( 1 + 2 + 3 + 4 ) );
        }

        [Test]
        public void closure_and_immediately_invoked_function_expression_IIFE()
        {
            string s = @"
                        var i = 10, j = 10; 
                        (function() { 
                          i = j + i; 
                        })();
                        i.toString();
                        ";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<JSEvalString>( o );
            Assert.That( o.ToString(), Is.EqualTo( "20" ) );
        }

    }
}
