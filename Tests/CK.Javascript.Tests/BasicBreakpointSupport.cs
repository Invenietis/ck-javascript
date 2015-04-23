using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CK.Javascript.Tests
{
    [TestFixture]
    public class BasicBreakpointSupport
    {

        [TestCase( "3" )]
        [TestCase( "3+7" )]
        [TestCase( "5 < 8" )]
        [TestCase( "(5&2) <= (7-(4<<2)*5+69)" )]
        public void breaking_and_restarting_an_evaluation( string s )
        {
            ScriptEngine engine = new ScriptEngine();
            Expr e = ExprAnalyser.AnalyseString( s );
            RuntimeObj syncResult;
            using( var r1 = engine.Execute( e ) )
            {
                Assert.That( r1.Status, Is.EqualTo( ScriptEngineStatus.IsFinished ) );
                syncResult = r1.Result;
            }
            engine.Breakpoints.BreakAlways = true;
            using( var r2 = engine.Execute( e ) )
            {
                int nbStep = 0;
                while( r2.Status == ScriptEngineStatus.IsPending )
                {
                    ++nbStep;
                    r2.Continue();
                }
                Assert.That( r2.Status, Is.EqualTo( ScriptEngineStatus.IsFinished ) );
                Assert.That( new RuntimeObjComparer( r2.Result, syncResult ).AreEqualStrict( engine.Context ) );
                Console.WriteLine( "String '{0}' = {1} evaluated in {2} steps.", s, syncResult.ToString(), nbStep );
            }
        }
    }
}
