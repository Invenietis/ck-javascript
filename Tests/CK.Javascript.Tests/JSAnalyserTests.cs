using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CK.Javascript;
using CK.Core;

namespace CK.MultiPlan.Tests.Language
{
    [TestFixture]
    public class JSAnalyserTests
    {
        [Test]
        public void EmptyParsing()
        {
            ExprAnalyser a = new ExprAnalyser( new StaticSyntaxicScope() );
            JSParser p = new JSParser();
            {
                p.Reset( "" );
                Assert.That( p.IsEndOfInput );
                Expr e = a.Analyse( p );
                Assert.That( e is SyntaxErrorExpr );
            }
            {
                p.Reset( " \r\n \n   \r  \n \t  " );
                Assert.That( p.IsEndOfInput );
                Expr e = a.Analyse( p );
                Assert.That( e is SyntaxErrorExpr );
            }
        }

        [Test]
        public void BadNumbers()
        {
            ExprAnalyser a = new ExprAnalyser( new StaticSyntaxicScope() );
            JSParser p = new JSParser();

            {
                p.Reset( "45DD" );
                Assert.That( p.IsErrorOrEndOfInput, Is.True );
                Assert.That( p.ErrorCode, Is.EqualTo( JSParserError.ErrorNumberIdentifierStartsImmediately ) );
            }
            {
                p.Reset( "45.member" );
                Assert.That( p.IsErrorOrEndOfInput, Is.True );
                Assert.That( p.ErrorCode, Is.EqualTo( JSParserError.ErrorNumberIdentifierStartsImmediately ) );
            }
            {
                p.Reset( ".45.member" );
                Assert.That( p.IsErrorOrEndOfInput, Is.True );
                Assert.That( p.ErrorCode, Is.EqualTo( JSParserError.ErrorNumberIdentifierStartsImmediately ) );
            }
            {
                p.Reset( "45.01member" );
                Assert.That( p.IsErrorOrEndOfInput, Is.True );
                Assert.That( p.ErrorCode, Is.EqualTo( JSParserError.ErrorNumberIdentifierStartsImmediately ) );
            }
            {
                p.Reset( ".45.member" );
                Assert.That( p.IsErrorOrEndOfInput, Is.True );
                Assert.That( p.ErrorCode, Is.EqualTo( JSParserError.ErrorNumberIdentifierStartsImmediately ) );
            }
            {
                p.Reset( ".45.01member" );
                Assert.That( p.IsErrorOrEndOfInput, Is.True );
                Assert.That( p.ErrorCode, Is.EqualTo( JSParserError.ErrorNumberIdentifierStartsImmediately ) );
            }
            {
                p.Reset( "45.01e23member" );
                Assert.That( p.IsErrorOrEndOfInput, Is.True );
                Assert.That( p.ErrorCode, Is.EqualTo( JSParserError.ErrorNumberIdentifierStartsImmediately ) );
            }
        }

        [Test]
        public void RoundtripParsing()
        {
            JSParser p = new JSParser();
            Assert.That( JSParser.Explain( JSParserToken.Integer ), Is.EqualTo( "42" ) );

            string s = " function ( x , z ) { if ( x != z || x && z % x - x >>> z >> z << x | z & x ^ z -- = x ++ ) return x + ( z * 42 ) / 42 ; } void == typeof += new -= delete >>= instanceof >>>= x % z %= x === z !== x ! z ~= x |= z &= x <<= z ^= x /= z *= x %=";
            p.Reset( s );
            string recompose = "";
            while( !p.IsEndOfInput )
            {
                recompose += " " + JSParser.Explain( p.CurrentToken );
                p.Forward();
            }
            s = s.Replace( "if", "identifier" )
                .Replace( "function", "identifier" )
                .Replace( "x", "identifier" )
                .Replace( "z", "identifier" )
                .Replace( "return", "identifier" );

            Assert.That( recompose, Is.EqualTo( s ) );
        }

        [Test]
        public void SimpleExpression()
        {
            ExprAnalyser a = new ExprAnalyser( new StaticSyntaxicScope() );
            JSParser p = new JSParser();

            {
                p.Reset( "value" );
                Assert.That( p.IsErrorOrEndOfInput, Is.False );
                Expr e = a.Analyse( p );
                Assert.That( e is AccessorMemberExpr );
                AccessorMemberExpr ac = e as AccessorMemberExpr;
                Assert.That( ac.IsUnbound == true );
            }
            {
                p.Reset( "!" );
                Expr e = a.Analyse( p );
                Assert.That( e is UnaryExpr );
                UnaryExpr u = e as UnaryExpr;
                Assert.That( u.TokenType == JSParserToken.Not );
                Assert.That( u.Expression is SyntaxErrorExpr );
                Assert.That( SyntaxErrorCollector.Collect( e, null ).Count == 1 );
            }
            {
                p.Reset( "!value" );
                Expr e = a.Analyse( p );
                Assert.That( e is UnaryExpr );
                UnaryExpr u = e as UnaryExpr;
                Assert.That( u.TokenType == JSParserToken.Not );
                Assert.That( u.Expression is AccessorExpr );
                Assert.That( SyntaxErrorCollector.Collect( e, Util.ActionVoid ).Count == 0 );
            }
            {
                p.Reset( " 0.12e43 && ~b " );
                Expr e = a.Analyse( p );
                Assert.That( e is BinaryExpr );
                BinaryExpr and = e as BinaryExpr;
                Assert.That( and.BinaryOperatorToken == JSParserToken.And );
                IsConstant( and.Left, 0.12e43 );
                Assert.That( and.Right is UnaryExpr );
                UnaryExpr u = and.Right as UnaryExpr;
                Assert.That( u.TokenType == JSParserToken.BitwiseNot );
                Assert.That( u.Expression is AccessorExpr );

                Assert.That( SyntaxErrorCollector.Collect( e, Util.ActionVoid ).Count == 0 );
            }
            {
                p.Reset( @"!a||~""x""" );
                Expr e = a.Analyse( p );
                Assert.That( e is BinaryExpr );
                BinaryExpr or = e as BinaryExpr;
                Assert.That( or.BinaryOperatorToken == JSParserToken.Or );
                Assert.That( or.Left is UnaryExpr );
                Assert.That( or.Right is UnaryExpr );
                UnaryExpr u = or.Right as UnaryExpr;
                Assert.That( u.TokenType == JSParserToken.BitwiseNot );
                IsConstant( u.Expression, "x" );

                Assert.That( SyntaxErrorCollector.Collect( e, Util.ActionVoid ).Count == 0 );
            }
            {
                p.Reset( "(3)" );
                Expr e = a.Analyse( p );
                IsConstant( e, 3 );
            }
            {
                p.Reset( "(3+typeof 'x')" );
                Expr e = a.Analyse( p );
                Assert.That( e is BinaryExpr );
                BinaryExpr b = e as BinaryExpr;
                IsConstant( b.Left, 3 );
                Assert.That( b.Right is UnaryExpr );
                UnaryExpr u = b.Right as UnaryExpr;
                Assert.That( u.TokenType == JSParserToken.TypeOf );
                IsConstant( u.Expression, "x" );

                Assert.That( SyntaxErrorCollector.Collect( e, Util.ActionVoid ).Count == 0 );
            }
            {
                p.Reset( "1 ? 2 : 3" );
                Expr e = a.Analyse( p );
                Assert.That( e is IfExpr );
                IfExpr i = e as IfExpr;
                Assert.That( i.IsTernaryOperator == true );
                IsConstant( i.Condition, 1 );
                IsConstant( i.WhenTrue, 2 );
                IsConstant( i.WhenFalse, 3 );
            }
        }

        void IsConstant( Expr e, object o )
        {
            Assert.That( e is ConstantExpr );
            ConstantExpr c = e as ConstantExpr;
            Assert.That( c.Value, Is.EqualTo( o ) );
        }
    }
}
