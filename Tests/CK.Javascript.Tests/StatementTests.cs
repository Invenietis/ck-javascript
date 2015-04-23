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
    public class StatementTests
    {
        [Test]
        public void evaluating_basic_numbers_expressions()
        {
            RuntimeObj o;
            {
                o = ScriptEngine.Evaluate( "6;7+3" );
                Assert.IsInstanceOf<JSEvalNumber>( o );
                Assert.That( o.ToDouble(), Is.EqualTo( 10 ) );
            }
            {
                o = ScriptEngine.Evaluate( "6;7+3;typeof 6 == 'number' ? 2173 : 3712" );
                Assert.IsInstanceOf<JSEvalNumber>( o );
                Assert.That( o.ToDouble(), Is.EqualTo( 2173 ) );
            }
        }

    }
}
