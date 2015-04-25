#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Javascript.Tests\JSAnalyserTests.cs) is part of CiviKey. 
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
using CK.Core;

namespace CK.Javascript.Tests
{
    [TestFixture]
    public class JSAnalyserTests
    {
        [Test]
        public void EmptyParsing()
        {
            ExprAnalyser a = new ExprAnalyser();
            JSTokeniser p = new JSTokeniser();
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
            ExprAnalyser a = new ExprAnalyser();
            JSTokeniser p = new JSTokeniser();

            {
                p.Reset( "45DD" );
                Assert.That( p.IsErrorOrEndOfInput, Is.True );
                Assert.That( p.ErrorCode, Is.EqualTo( JSTokeniserError.ErrorNumberIdentifierStartsImmediately ) );
            }
            {
                p.Reset( "45.member" );
                Assert.That( p.IsErrorOrEndOfInput, Is.True );
                Assert.That( p.ErrorCode, Is.EqualTo( JSTokeniserError.ErrorNumberIdentifierStartsImmediately ) );
            }
            {
                p.Reset( ".45.member" );
                Assert.That( p.IsErrorOrEndOfInput, Is.True );
                Assert.That( p.ErrorCode, Is.EqualTo( JSTokeniserError.ErrorNumberIdentifierStartsImmediately ) );
            }
            {
                p.Reset( "45.01member" );
                Assert.That( p.IsErrorOrEndOfInput, Is.True );
                Assert.That( p.ErrorCode, Is.EqualTo( JSTokeniserError.ErrorNumberIdentifierStartsImmediately ) );
            }
            {
                p.Reset( ".45.member" );
                Assert.That( p.IsErrorOrEndOfInput, Is.True );
                Assert.That( p.ErrorCode, Is.EqualTo( JSTokeniserError.ErrorNumberIdentifierStartsImmediately ) );
            }
            {
                p.Reset( ".45.01member" );
                Assert.That( p.IsErrorOrEndOfInput, Is.True );
                Assert.That( p.ErrorCode, Is.EqualTo( JSTokeniserError.ErrorNumberIdentifierStartsImmediately ) );
            }
            {
                p.Reset( "45.01e23member" );
                Assert.That( p.IsErrorOrEndOfInput, Is.True );
                Assert.That( p.ErrorCode, Is.EqualTo( JSTokeniserError.ErrorNumberIdentifierStartsImmediately ) );
            }
        }

        [Test]
        public void RoundtripParsing()
        {
            JSTokeniser p = new JSTokeniser();
            Assert.That( JSTokeniser.Explain( JSTokeniserToken.Integer ), Is.EqualTo( "42" ) );

            string s = " function ( x , z ) ++ -- { if ( x != z || x && z % x - x >>> z >> z << x | z & x ^ z -- = x ++ ) return x + ( z * 42 ) / 42 ; } void == typeof += new -= delete >>= instanceof >>>= x % z %= x === z !== x ! z ~ = x |= z &= x <<= z ^= x /= z *= x %=";
            p.Reset( s );
            string recompose = "";
            while( !p.IsEndOfInput )
            {
                recompose += " " + JSTokeniser.Explain( p.CurrentToken );
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
            ExprAnalyser a = new ExprAnalyser();
            JSTokeniser p = new JSTokeniser();

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
                Assert.That( u.TokenType == JSTokeniserToken.Not );
                Assert.That( u.Expression is SyntaxErrorExpr );
                Assert.That( SyntaxErrorCollector.Collect( e, null ).Count == 1 );
            }
            {
                p.Reset( "!value" );
                Expr e = a.Analyse( p );
                Assert.That( e is UnaryExpr );
                UnaryExpr u = e as UnaryExpr;
                Assert.That( u.TokenType == JSTokeniserToken.Not );
                Assert.That( u.Expression is AccessorExpr );
                Assert.That( SyntaxErrorCollector.Collect( e, Util.ActionVoid ).Count == 0 );
            }
            {
                p.Reset( " 0.12e43 && ~b " );
                Expr e = a.Analyse( p );
                Assert.That( e is BinaryExpr );
                BinaryExpr and = e as BinaryExpr;
                Assert.That( and.BinaryOperatorToken == JSTokeniserToken.And );
                IsConstant( and.Left, 0.12e43 );
                Assert.That( and.Right is UnaryExpr );
                UnaryExpr u = and.Right as UnaryExpr;
                Assert.That( u.TokenType == JSTokeniserToken.BitwiseNot );
                Assert.That( u.Expression is AccessorExpr );

                Assert.That( SyntaxErrorCollector.Collect( e, Util.ActionVoid ).Count == 0 );
            }
            {
                p.Reset( @"!a||~""x""" );
                Expr e = a.Analyse( p );
                Assert.That( e is BinaryExpr );
                BinaryExpr or = e as BinaryExpr;
                Assert.That( or.BinaryOperatorToken == JSTokeniserToken.Or );
                Assert.That( or.Left is UnaryExpr );
                Assert.That( or.Right is UnaryExpr );
                UnaryExpr u = or.Right as UnaryExpr;
                Assert.That( u.TokenType == JSTokeniserToken.BitwiseNot );
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
                Assert.That( u.TokenType == JSTokeniserToken.TypeOf );
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

        [Test]
        public void ArraySupport()
        {
            ExprAnalyser a = new ExprAnalyser();
            JSTokeniser p = new JSTokeniser();
            {
                p.Reset( "a[9]" );
                Assert.That( p.IsErrorOrEndOfInput, Is.False );
                Expr e = a.Analyse( p );
                Assert.That( e is AccessorIndexerExpr );
                AccessorIndexerExpr ac = e as AccessorIndexerExpr;
                IsConstant( ac.Index, 9 );
            }
            {
                p.Reset( "array['Hello World!']" );
                Assert.That( p.IsErrorOrEndOfInput, Is.False );
                Expr e = a.Analyse( p );
                Assert.That( e is AccessorIndexerExpr );
                AccessorIndexerExpr ac = e as AccessorIndexerExpr;
                IsConstant( ac.Index, "Hello World!" );
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
