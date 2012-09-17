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
