﻿#region LGPL License
/* ----------------------------------------------------------------------------
*  This file (SyntaxicScopeTests.cs) is part of CK-Javascript. 
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
    public class SyntaxicScopeTests
    {
        [Test]
        public void ClosingRootIsAnError()
        {
            StaticSyntaxicScope s = new StaticSyntaxicScope();
            s.OpenScope( new SourceLocation() );
            s.CloseScope();
            Assert.Throws<InvalidOperationException>( () => s.CloseScope() );
        }

        [Test]
        public void DefineAndSubordinatedDefines()
        {
            StaticSyntaxicScope s = new StaticSyntaxicScope();
            
            var toto = s.Define( "toto", new ConstantExpr( SourceLocation.Empty, 0 ) );
            Assert.That( s.Find( "toto" ) == toto );
            Assert.That( s.Define( "toto", new ConstantExpr( SourceLocation.Empty, -1 ) ) is SyntaxErrorExpr );
            Assert.That( s.Find( "t" ) == null );

            s.OpenScope( new SourceLocation() );
            var toto2 = s.Define( "toto", new ConstantExpr( SourceLocation.Empty, 1 ) );
            Assert.That( s.Find( "toto" ) == toto2 );
            Assert.That( s.Define( "toto", new ConstantExpr( SourceLocation.Empty, -1 ) ) is SyntaxErrorExpr );
            Assert.That( s.Find( "t" ) == null );
            
            s.CloseScope();
            Assert.That( s.Find( "toto" ) == toto );
            Assert.That( s.Define( "toto", new ConstantExpr( SourceLocation.Empty, -1 ) ) is SyntaxErrorExpr );
        }

    }
}
