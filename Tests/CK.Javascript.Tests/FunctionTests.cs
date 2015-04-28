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

    }
}
