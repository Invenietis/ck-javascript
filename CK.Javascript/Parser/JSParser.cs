﻿using System;
using System.Diagnostics;
using System.Text;
using System.IO;
using CK.Core;
using System.Globalization;

namespace CK.Javascript
{

    /// <summary>
    ///	Small parser text to handle javascript based language (ECMAScript).
    /// </summary>
    public class JSParser
    {
        #region Private fields

        TextReader		_inner;
        int             _prevCharPosTokenEnd;
        int             _charPosTokenBeg;
        int             _charPos;
        int				_nextC;
        SourceLocation  _prevNonCommentLocation;
        SourceLocation  _location;
        bool			_lineInc;

        bool			_skipComments;

        int             _integerValue;
        string          _identifierValue;

        StringBuilder	_buffer;
        int				_token;
        int             _prevNonCommentToken;

        #endregion

        public JSParser()
        {
            _skipComments = true;
            _buffer = new StringBuilder( 512 );
        }

        public bool Reset( string input, string source = SourceLocation.NoSource, int startLineNumber = 0, int startColumnNumber = 0 )
        {
            return Reset( new StringReader( input ), source, startLineNumber, startColumnNumber );
        }

        public bool Reset( TextReader input, string source, int startLineNumber, int startColumnNumber )
        {
            _inner = input;
            _location.Source = source ?? SourceLocation.NoSource;
            _location.Line = startLineNumber;
            _location.Column = startColumnNumber;

            _charPosTokenBeg = 0;
            _prevCharPosTokenEnd = 0;
            _charPos = 0;
            _nextC = 0;
            _token = 0;
            NextToken2();
            return _token >= 0;
        }

        /// <summary>
        /// Defaults to true.
        /// </summary>
        public bool SkipComments
        {
            get { return _skipComments; }
            set { _skipComments = value; }
        }

        /// <summary>
        /// Gets the current precedence level from <see cref="CurrentToken"/> with a provision of 1 bit
        /// to ease the handling of right associative infix operators (this level is even).
        /// </summary>
        /// <remarks>
        /// This uses <see cref="JSParserToken.OpLevelMask"/> and <see cref="JSParserToken.OpLevelShift"/>.
        /// </remarks>
        public int CurrentPrecedenceLevel
        {
            get { return PrecedenceLevel( CurrentToken ); }
        }

        /// <summary>
        /// Computes the precedence with a provision of 1 bit to ease the handling of right associative infix operators.
        /// </summary>
        /// <returns>An even precedence level between 30 and 2. 0 if the token has <see cref="JSParserError.IsErrorOrEndOfInput"/> bit set.</returns>
        /// <remarks>
        /// This uses <see cref="JSParserToken.OpLevelMask"/> and <see cref="JSParserToken.OpLevelShift"/>.
        /// </remarks>
        public static int PrecedenceLevel( JSParserToken t )
        {
            return t > 0 ? ( ((int)(t & JSParserToken.OpLevelMask)) >> (int)JSParserToken.OpLevelShift ) << 1 : 0;
        }

        /// <summary>
        /// Gets the current <see cref="JSParserToken"/> code.
        /// </summary>
        public JSParserToken CurrentToken
        {
            get { return (JSParserToken)_token; }
        }

        /// <summary>
        /// Gets the <see cref="JSParserError"/> code if the parser is in error
        /// (or the end of the input is reached). <see cref="JSParserError.None"/> if
        /// no error occured.
        /// </summary>
        public JSParserError ErrorCode
        {
            get { return _token < 0 ? (JSParserError)_token : JSParserError.None; }
        }

        #region IsErrorOrEndOfInput, IsEndOfInput, IsAssignOperator, ..., IsUnaryOperator
        /// <summary>
        /// True if an error or the end of the stream is reached.
        /// </summary>
        /// <returns></returns>
        public bool IsErrorOrEndOfInput
        {
            get { return _token < 0; }
        }

        /// <summary>
        /// True if <see cref="ErrorCode"/> is <see cref="JSParserError.EndOfInput"/>.
        /// </summary>
        /// <returns></returns>
        public bool IsEndOfInput
        {
            get { return _token == (int)JSParserError.EndOfInput; }
        }

        public bool IsAssignOperator
        {
            get { return (_token&(int)JSParserToken.IsAssignOperator) != 0; }
        }

        public bool IsBinaryOperator
        {
            get { return (_token & (int)JSParserToken.IsBinaryOperator) != 0; }
        }

        public bool IsBracket
        {
            get { return (_token & (int)JSParserToken.IsBracket) != 0; }
        }

        public bool IsCompareOperator
        {
            get { return (_token & (int)JSParserToken.IsCompareOperator) != 0; }
        }

        public bool IsComment
        {
            get { return (_token & (int)JSParserToken.IsComment) != 0; }
        }

        /// <summary>
        /// True if the current token is an identifier. <see cref="ReadIdentifier"/> can be called to get 
        /// the actual value.
        /// </summary>
        public bool IsIdentifier
        {
            get { return (_token & (int)JSParserToken.IsIdentifier) != 0; }
        }

        public bool IsLogical
        {
            get { return (_token & (int)JSParserToken.IsLogical) != 0; }
        }

        #region IsNumber, IsNumberFloat and IsNumberInteger
        /// <summary>
        /// True if the current token is a number. <see cref="ReadNumber"/> can be called to get 
        /// the actual value.
        /// </summary>
        public bool IsNumber
        {
            get { return (_token & (int)JSParserToken.IsNumber) != 0; }
        }

        /// <summary>
        /// True if the current token is a float number (i.e. with a fractional and/or 
        /// exponent part). <see cref="ReadNumber"/> can be called to get the value.
        /// </summary>
        public bool IsNumberFloat
        {
            get { return _token == (int)JSParserToken.Float; }
        }

        /// <summary>
        /// True if the current token is an integer number (i.e. without any fractional nor 
        /// exponent part). <see cref="ReadNumber"/> can be called to get the value.
        /// </summary>
        public bool IsNumberInteger
        {
            get { return (_token & (int)JSParserToken.Integer) == (int)JSParserToken.Integer; }
        }
        #endregion

        public bool IsPunctuation
        {
            get { return (_token & (int)JSParserToken.IsPunctuation) != 0; }
        }

        public bool IsRegex
        {
            get { return (_token & (int)JSParserToken.IsRegex) != 0; }
        }

        /// <summary>
        /// True if the current token is a string. <see cref="ReadString"/> can be called to get 
        /// the actual value.
        /// </summary>
        public bool IsString
        {
            get { return (_token & (int)JSParserToken.IsString) != 0; }
        }

        /// <summary>
        /// True when <see cref="CurrentToken"/> is "++", "--", "-", "~", "!", "delete", "new", "typeof" or "void".
        /// </summary>
        public bool IsUnaryOperator
        {
            get { return (_token & (int)JSParserToken.IsUnaryOperator) != 0; }
        }
        
        public bool IsUnaryOperatorExtended
        {
            get { return (_token & (int)JSParserToken.OpLevelMask) == (int)JSParserToken.OpLevel14; }
        }
        #endregion

        /// <summary>
        /// Forwards the head to the next token.
        /// </summary>
        /// <returns>True if a token is available. False if the end of the stream is encountered
        /// or an error occured.</returns>
        public bool Forward()
        {
            return NextToken2() >= 0;
        }

        /// <summary>
        /// Gets the character index in the input stream of the current token.
        /// </summary>
        public int CharPosTokenBeg
        {
            get { return _charPosTokenBeg; }
        }

        /// <summary>
        /// Gets the current character index in the input stream: it corresponds to the
        /// end of the current token.
        /// </summary>
        public int CharPosTokenEnd
        {
            get { return _charPos; }
        }

        /// <summary>
        /// Gets the current source location. A <see cref="SourceLocation"/> is a value type.
        /// </summary>
        public SourceLocation Location
        {
            get { return _location; }
        }

        /// <summary>
        /// Gets the previous token (ignoring any comments that may have occured).
        /// </summary>
        public JSParserToken PrevNonCommentToken
        {
            get { return (JSParserToken)_prevNonCommentToken; }
        }

        /// <summary>
        /// Gets the previous token source location. A <see cref="SourceLocation"/> is a value type.
        /// </summary>
        public SourceLocation PrevNonCommentLocation
        {
            get { return _prevNonCommentLocation; }
        }

        /// <summary>
        /// Gets the character index in the input stream before the current token.
        /// Since it is the end of the previous token, separators (white space, comments if <see cref="SkipComments"/> is 
        /// true) before the current token are included.
        /// If SkipComments is false and a comment exists before the current token, this is the index of 
        /// the end of the comment.
        /// </summary>
        public int PrevCharPosTokenEnd
        {
            get { return _prevCharPosTokenEnd; }
        }

        static string[] _assignOperator = { "=", "~=", "|=", "&=", "<<=", "^=", ">>=", ">>>=", "+=", "-=", "/=", "*=", "%=" };
        static string[] _binaryOperator = { "|", "^", "&", ">>", "<<", ">>>", "+", "-", "/", "*", "%", "instanceof" };
        static string[] _compareOperator = { "==", "<", ">", "<=", ">=", "!=", "===", "!==" };
        static string[] _punctuations = { ".", ",", "?", ":", ";" };
        static string[] _specialIdentifiers = { "delete", "new", "typeof", "void" };
        static string[] _unaryOperator = { "!", "~", "--", "++", "delete", "new", "typeof", "void" };

        public static string Explain( JSParserToken t )
        {
            if( t < 0 )
            {
                return ((JSParserError)t).ToString();
            }
            if( (t & JSParserToken.IsAssignOperator) != 0 ) return _assignOperator[((int)t & 15) - 1];
            if( (t & JSParserToken.IsBinaryOperator) != 0 ) return _binaryOperator[((int)t & 15) - 1];
            if( (t & JSParserToken.IsCompareOperator) != 0 ) return _compareOperator[((int)t & 15) - 1];
            if( (t & JSParserToken.IsPunctuation) != 0 ) return _punctuations[((int)t & 15) - 1];
            if( (t & JSParserToken.IsUnaryOperator) != 0 ) return _unaryOperator[((int)t & 15) - 1];
            
            if( t == JSParserToken.Identifier ) return "identifier";
            if( t == JSParserToken.And ) return "&&";
            if( t == JSParserToken.Or ) return "||";
            
            if( t == JSParserToken.String ) return "\"string\"";

            if( t == JSParserToken.Float ) return "6.02214129e+23";
            if( t == JSParserToken.Integer ) return "42";
            if( t == JSParserToken.HexNumber ) return "0x00CF12A4";
            if( t == JSParserToken.NaN ) return "NaN";
            if( t == JSParserToken.Infinity ) return "Infinity";

            if( t == JSParserToken.StarComment ) return "/* ... */";
            if( t == JSParserToken.LineComment ) return "// ..." + Environment.NewLine;
            
            if( t == JSParserToken.Regex ) return "/regex/gi";

            if( t == JSParserToken.OpenPar ) return "(";
            if( t == JSParserToken.ClosePar ) return ")";
            if( t == JSParserToken.OpenBracket ) return "[";
            if( t == JSParserToken.CloseBracket ) return "]";
            if( t == JSParserToken.OpenCurly ) return "{";
            if( t == JSParserToken.CloseCurly ) return "}";


            return JSParserToken.None.ToString();
        }

        /// <summary>
        /// Reads a comment (with its opening and closing tags) and forwards head. Returns null and 
        /// does not forward the head if current token is not a comment. 
        /// To be able to read comments (ie. returning not null here) requires <see cref="SkipComments"/> to be false.
        /// </summary>
        /// <returns></returns>
        public string ReadComment()
        {
            return (_token & (int)JSParserToken.IsComment) != 0 ? ReadBuffer() : null;
        }

        /// <summary>
        /// Reads a string value and forwards head. Returns null and 
        /// does not forward the head if current token is not a string. 
        /// </summary>
        /// <returns></returns>
        public string ReadString()
        {
            return _token == (int)JSParserToken.String ? ReadBuffer() : null;
        }


        /// <summary>
        /// Reads an identifier and forwards head. Returns null and 
        /// does not forward the head if current token is not an identifier. 
        /// </summary>
        /// <returns></returns>
        public string ReadIdentifier()
        {
            string id = null;
            if( IsIdentifier )
            {
                id = _identifierValue;
                Forward();
            }
            return id;
        }

        /// <summary>
        /// Reads a dotted identifier and forwards head (stops on any non identifier nor dot token). 
        /// Returns null and does not forward the head if current token is not an identifier. 
        /// </summary>
        /// <remarks>
        /// If the identifier ends with a dot, this last dot is kept in the result.
        /// </remarks>
        /// <returns>The dotted identifier or null if not found.</returns>
        public string ReadDottedIdentifier()
        {
            string multiId = null;
            string id = ReadIdentifier();
            if( id != null )
            {
                multiId = id;
                while( _token == (int)JSParserToken.Dot )
                {
                    multiId += '.';
                    Forward();
                    id = ReadIdentifier();
                    if( id == null ) break;
                    multiId += id;
                }
            }
            return multiId;
        }

        /// <summary>
        /// Reads an identifier that may be a string or a number (i.e. <c>myId</c> or <c>'My Identifier'</c> or <c>0.112E3</c>) and forwards head. 
        /// Returns null and does not forward the head if current token is not an identifier nor a string nor a number.
        /// Useful for reading javascript objects since a Javascript key can be any of these tokens.
        /// </summary>
        /// <returns></returns>
        public string ReadExtendedIdentifierAsString()
        {
            if( (_token & (int)JSParserToken.IsIdentifier) != 0 ) return _identifierValue;
            if( (_token & (int)(JSParserToken.IsString | JSParserToken.IsNumber)) != 0 ) return ReadBuffer();
            return null;
        }

        /// <summary>
        /// Reads an identifier and forwards head. Returns false and 
        /// does not forward the head if current token is not an identifier. 
        /// </summary>
        /// <returns></returns>
        public bool MatchIdentifier( string identifier )
        {
            if( (_token & (int)JSParserToken.IsIdentifier) != 0
                && String.CompareOrdinal( _identifierValue, identifier ) == 0 )
            {
                Forward();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Match identifier. Forward the head on success and can throw an exception
        /// if not found.
        /// </summary>
        public bool MatchIdentifier( string identifier, bool throwError )
        {
            if( !MatchIdentifier( identifier ) )
            {
                if( throwError )
                    throw new CKException( "Identifier '{0}' expected. {1}.", identifier, _location );
                return false;
            }
            return true;
        }


        /// <summary>
        /// Matches a token. Forwards the head on success.
        /// </summary>
        /// <param name="token">Must be one of <see cref="JSParserToken"/> value (not an Error one).</param>
        /// <returns>True if the given token matches.</returns>
        public bool Match( JSParserToken token )
        {
            if( token < 0 ) throw new ArgumentException( "Token must not be an Error token." );
            if( _token == (int)token )
            {
                Forward();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Matches a token. Forwards the head on success and can throw an error
        /// if token does not match.
        /// </summary>
        /// <param name="token">Token to match (must not be an Error one).</param>
        /// <returns>True if the given token matches.</returns>
        public bool Match( JSParserToken token, bool throwError )
        {
            if( !Match( token ) )
            {
                if( throwError )
                    throw new CKException( "Token {0} expected. {1}.", token.ToString(), _location );
                return false;
            }
            return true;
        }

        /// <summary>
        /// Reads a number and forwards head on success. 
        /// May return <see cref="Double.NaN"/> and does not forward the head if current token is not a number (<see cref="IsNumber"/> is false)
        /// or if the double can not be parsed by <see cref="Double.TryParse"/>. 
        /// </summary>
        /// <returns>The number or <see cref="Double.NaN"/> if <see cref="IsNumber"/> is false.</returns>
        public bool IsDouble( out double d )
        {
            d = 0;
            if( (_token & (int)JSParserToken.IsNumber) == 0 ) return false;
            d = ReadDouble();
            return true;
        }

        /// <summary>
        /// Reads the current number and forwards head. Throws an <see cref="InvalidOperationException"/> if <see cref="IsNumber"/> is false.
        /// </summary>
        /// <returns>The number. It can be <see cref="Double.NaN"/> or <see cref="Double.PositiveInfinity"/>.</returns>
        public double ReadDouble()
        {
            Double d;
            if( _token == (int)JSParserToken.NaN ) d = Double.NaN;
            else if( _token == (int)JSParserToken.Infinity ) d = Double.PositiveInfinity;
            else if( _token == (int)JSParserToken.Float )
            {
                // This is not compliant with Javascript rules: it returns 0 four huge or very small numbers.
                // It should return Infinity for huge numbers.
                Double.TryParse( _buffer.ToString(), NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out d );
            }
            else d = _integerValue;
            Forward();
            return d;
        }

        private string ReadBuffer()
        {
            Debug.Assert( _token > 0 );
            string r = _buffer.ToString();
            Forward();
            return r;
        }

        #region Basic input
        int Peek()
        {
            return _nextC == 0 ? (_nextC = _inner.Read()) : _nextC;
        }

        bool Read( int c )
        {
            if( Peek() == c )
            {
                Read();
                return true;
            }
            return false;
        }

        int Read()
        {
            int ret;
            if( _nextC != 0 )
            {
                ret = _nextC;
                _nextC = 0;
            }
            else ret = _inner.Read();

            _charPos++;

            if( _lineInc )
            {
                _location.Line++;
                _location.Column = 1;
                _lineInc = false;
            }
            if( ret != '\r' )
            {
                // Line Separator \u2028 and Paragraph Separator \u2029
                // are mapped to \n.
                if( ret == '\n' || ret == '\u2028' || ret == '\u2029' )
                {
                    ret = '\n';
                    _lineInc = true;
                }
                _location.Column++;
            }
            return ret;
        }

        int ReadFirstNonWhiteSpace()
        {
            int c;
            for( ; ; )
            {
                switch( (c = Read()) )
                {
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n': continue;
                    default: return c;
                }
            }
        }

        static private bool IsIdentifierStartChar( int c )
        {
            return c == '_' || c == '$' || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        static private bool IsIdentifierChar( int c )
        {
            return IsIdentifierStartChar( c ) || (c >= '0' && c <= '9');
        }

        static private int FromHexDigit( int c )
        {
            Debug.Assert( '0' < 'A' && 'A' < 'a' );
            c -= '0';
            if( c < 0 ) return -1;
            if( c <= 9 ) return c;
            c -= 'A' - '0';
            if( c < 0 ) return -1;
            if( c <= 5 ) return 10 + c;
            c -= 'a' - 'A';
            if( c >= 0 && c <= 5 ) return 10 + c;
            return -1;
        }

        static private int FromDecDigit( int c )
        {
            c -= '0';
            return c >= 0 && c <= 9 ? c : -1;
        }

        private bool HandleStdComment()
        {
            int ic = Read();
            if( _skipComments )
                for( ; ; )
                {
                    do { if( ic == -1 ) return false; ic = Read(); }
                    while( ic != '*' );
                    ic = Read();
                    if( ic == '/' ) return true;
                }
            else
                for( ; ; )
                {
                    do { if( ic == -1 ) return false; ic = Read(); _buffer.Append( (char)ic ); }
                    while( ic != '*' );
                    ic = Read();
                    if( ic == '/' )
                    {
                        --_buffer.Length; // Removes added *.
                        return true;
                    }
                }
        }

        private void HandleEndOfLineComment()
        {
            int ic = Read();
            if( _skipComments )
            {
                do { ic = Peek(); }
                while( ic != '\n' && Read() != -1 );
            }
            else
            {
                do { ic = Peek(); }
                while( ic != '\n' && ic != '\u2028' && ic != '\u2029'
                    && Read() != -1
                    && _buffer.Append( (char)ic ) != null );
            }
        }


        #endregion

        int HandleStarComment()
        {
            int ic;
            while( (ic = Read()) != -1 )
            {
                if( ic == '*' && Read( '/' ) ) return (int)JSParserToken.StarComment;
            }
            return (int)JSParserError.EndOfInput;
        }

        int HandleLineComment()
        {
            int ic;
            while( (ic = Read()) != -1 )
            {
                if( ic == '\n' ) return (int)JSParserToken.LineComment;
            }
            return (int)JSParserError.EndOfInput;
        }

        int HandleRegex()
        {
            int ic;
            while( (ic = Read()) != -1 )
            {
                if( ic == '\\' )
                {
                    ic = Read();
                    if( ic == -1 ) break;
                }
                else if( ic == '/' )
                {
                    while( (ic = Read()) == 'g' || ic == 'i' || ic == 'm' ) ;
                    if( ic == -1 ) break;
                    return (int)JSParserToken.Regex;
                }
            }
            return (int)JSParserError.ErrorRegexUnterminated;
        }

        int NextToken2()
        {
            if( _token >= 0 )
            {
                // Current char position is the end of the previous token.
                _prevCharPosTokenEnd = _charPos;

                if( (_token & (int)JSParserToken.IsComment) == 0 )
                {
                    // Previous token and token location are preserved.
                    _prevNonCommentLocation = _location;
                    _prevNonCommentToken = _token;
                }

                // A cute goto loop :-)
                readToken:

                _token = NextTokenLowLevel();
                if( (_token & (int)JSParserToken.IsComment) != 0 && _skipComments ) goto readToken;
            }
            return _token;
        }

        int NextTokenLowLevel()
        {
            int ic = ReadFirstNonWhiteSpace();
            // Current char position is the beginning of the new current token.
            _charPosTokenBeg = _charPos;

            if( ic == -1 ) return (int)JSParserError.EndOfInput;
            switch( ic )
            {
                case '\'':
                case '\"': return ReadString( ic );
                case '=': return Read( '=' ) ? (Read('=') ? (int)JSParserToken.StrictEqual : (int)JSParserToken.Equal) : (int)JSParserToken.Assign;
                case '*': return Read( '=' ) ? (int)JSParserToken.MultAssign : (int)JSParserToken.Mult;
                case '!': return Read( '=' ) ? (Read( '=' ) ? (int)JSParserToken.StrictDifferent : (int)JSParserToken.Different) : (int)JSParserToken.Not;
                case '^':
                    if( Read( '=' ) ) return (int)JSParserToken.BitwiseXOrAssign;
                    return (int)JSParserToken.BitwiseXOr;
                case '&':
                    if( Read( '&' ) ) return (int)JSParserToken.And;
                    if( Read( '=' ) ) return (int)JSParserToken.BitwiseAndAssign;
                    return (int)JSParserToken.BitwiseAnd;
                case '|':
                    if( Read( '|' ) ) return (int)JSParserToken.Or;
                    if( Read( '=' ) ) return (int)JSParserToken.BitwiseOrAssign;
                    return (int)JSParserToken.BitwiseOr;
                case '>':
                    if( Read( '=' ) ) return (int)JSParserToken.GreaterOrEqual;
                    if( Read( '>' ) )
                    {
                        if( Read( '=' ) ) return (int)JSParserToken.BitwiseShiftRightAssign;
                        if( Read( '>' ) )
                        {
                            if( Read( '=' ) ) return (int)JSParserToken.BitwiseShiftRightNoSignBitAssign;
                            return (int)JSParserToken.BitwiseShiftRightNoSignBit;
                        }
                        return (int)JSParserToken.BitwiseShiftRight;
                    }
                    return (int)JSParserToken.Greater;
                case '<':
                    if( Read( '=' ) ) return (int)JSParserToken.LessOrEqual;
                    if( Read( '<' ) )
                    {
                        if( Read( '=' ) ) return (int)JSParserToken.BitwiseShiftLeftAssign;
                        return (int)JSParserToken.BitwiseShiftLeft;
                    }
                    return (int)JSParserToken.Less;
                case '.':
                    // A number can start with a dot.
                    ic = FromDecDigit( Peek() );
                    if( ic >= 0 )
                    {
                        Read();
                        return ReadNumber( ic, true );
                    }
                    return (int)JSParserToken.Dot;
                case '{': return (int)JSParserToken.OpenCurly;
                case '}': return (int)JSParserToken.CloseCurly;
                case '(': return (int)JSParserToken.OpenPar;
                case ')': return (int)JSParserToken.ClosePar;
                case '[': return (int)JSParserToken.OpenSquare;
                case ']': return (int)JSParserToken.CloseSquare;
                case ':': return (int)JSParserToken.Colon;
                case ';': return (int)JSParserToken.SemiColon;
                case ',': return (int)JSParserToken.Comma;
                case '?': return (int)JSParserToken.QuestionMark;
                case '/':
                    {
                        if( Read( '*' ) ) return HandleStarComment();
                        if( Read( '/' ) ) return HandleLineComment();
                        if( Read( '=' ) ) return (int)JSParserToken.DivideAssign;
                        if( (_prevNonCommentToken & (int)(JSParserToken.IsIdentifier | JSParserToken.IsString | JSParserToken.IsNumber)) != 0
                            || _prevNonCommentToken == (int)JSParserToken.ClosePar
                            || _prevNonCommentToken == (int)JSParserToken.CloseSquare
                            || _prevNonCommentToken == (int)JSParserToken.PlusPlus
                            || _prevNonCommentToken == (int)JSParserToken.MinusMinus ) return (int)JSParserToken.Divide;
                        return HandleRegex();
                    }
                case '-':
                    if( Read( '-' ) ) return (int)JSParserToken.MinusMinus;
                    if( Read( '=' ) ) return (int)JSParserToken.MinusAssign;
                    return (int)JSParserToken.Minus;
                case '+':
                    if( Read( '+' ) ) return (int)JSParserToken.PlusPlus;
                    if( Read( '=' ) ) return (int)JSParserToken.PlusAssign;
                    return (int)JSParserToken.Plus;
                case '%':
                    if( Read( '=' ) ) return (int)JSParserToken.ModuloAssign;
                    return (int)JSParserToken.Modulo;
                case '~':
                    if( Read( '=' ) ) return (int)JSParserToken.BitwiseNotAssign;
                    return (int)JSParserToken.BitwiseNot;
                default:
                    {
                        int digit = FromDecDigit( ic );
                        if( digit >= 0 ) return ReadAllKindOfNumber( digit );
                        if( IsIdentifierStartChar( ic ) ) return ReadIdentifier( ic );
                        return (int)JSParserError.ErrorInvalidChar;
                    }
            }
        }

        private int ReadAllKindOfNumber( int firstDigit )
        {
            Debug.Assert( firstDigit >= 0 && firstDigit <= 9 );
            if( firstDigit == 0 && Read( 'x' ) ) return ReadHexNumber();
            return ReadNumber( firstDigit, false );
        }

        private int ReadHexNumber()
        {
            ulong uValue;
            int nbD = IsPositiveHexNumber( out uValue, -1 );
            if( nbD == 0 ) return (int)JSParserError.ErrorNumberUnterminatedValue;
            _integerValue = (int)uValue;
            return (int)JSParserToken.HexNumber;
        }

        /// <summary>
        /// Returns the number of processed digits.
        /// </summary>
        private int IsPositiveHexNumber( out ulong val, int maxNbDigit )
        {
            unchecked
            {
                int nbDigit = 0;
                val = 0;
                int vHex;
                while( (vHex = FromHexDigit( Peek() )) >= 0 )
                {
                    Debug.Assert( vHex < 16 );
                    if( nbDigit < 16 )
                    {
                        val *= 16;
                        val += (uint)vHex;
                    }
                    Read();
                    if( ++nbDigit == maxNbDigit ) break;
                }
                return nbDigit;
            }
        }

        /// <summary>
        /// May return an error code or a number token.
        /// Whatever the read result is, the buffer contains the token.
        /// </summary>
        private int ReadNumber( int firstDigit, bool hasDot )
        {
            bool hasExp = false;
            int nextRequired = 0;
            _buffer.Length = 0;
            if( hasDot ) _buffer.Append( "0." );
            else _integerValue = firstDigit;
            _buffer.Append( (char)(firstDigit+'0') );
            for( ; ; )
            {
                int ic = Peek();
                if( ic >= '0' && ic <= '9' )
                {
                    Read();
                    _buffer.Append( (char)ic );
                    if( !hasDot ) _integerValue = _integerValue * 10 + (ic - '0');
                    nextRequired = 0;
                    continue;
                }
                if( !hasExp && (ic == 'e' || ic == 'E') )
                {
                    Read();
                    hasExp = hasDot = true;
                    _buffer.Append( 'E' );
                    if( Read( '-' ) ) _buffer.Append( '-' ); 
                    else Read( '+' );
                    // At least a digit is required.
                    nextRequired = 1;
                    continue;
                }
                if( ic == '.' )
                {
                    if( !hasDot )
                    {
                        Read();
                        hasDot = true;
                        _buffer.Append( '.' );
                        // Dot can be the last character. 
                        // Use 2 to remember that dot has been found: we consider it as an integer value.
                        nextRequired = 2;
                        continue;
                    }
                    return (int)JSParserError.ErrorNumberIdentifierStartsImmediately;
                }

                if( nextRequired == 1 ) return (int)JSParserError.ErrorNumberUnterminatedValue;
                // To be valid, the number must be followed by an operator, a punctuation or a statement separator (the ';')
                // or a line ending (recall that awful javascript "feature": lines without ending ';' 
                // are automagically corrected if 'needed').
                // We do not handle all cases here, except the 45DD.
                if( IsIdentifierStartChar( ic ) ) return (int)JSParserError.ErrorNumberIdentifierStartsImmediately;
                break;
            }
            if( hasDot )
            {
                // Consider number terminated by dot as integer.
                if( nextRequired != 2 ) return (int)JSParserToken.Float;
            }
            return (int)JSParserToken.Integer;
        }

        private int ReadString( int quote )
        {
            _buffer.Length = 0;
            ulong icu;
            for( ; ; )
            {
                int ic = Read();
                if( ic == -1 ) return (int)JSParserError.ErrorStringUnterminated;
                if( ic == quote ) break;
                else if( ic == '\\' )
                {
                    ic = Read();
                    switch( ic )
                    {
                        case '"': break;
                        case '\'': break;
                        case '\\': break;
                        case 'r': ic = '\r'; break;
                        case 'n': ic = '\n'; break;
                        case 't': ic = '\t'; break;
                        case 'b': ic = '\b'; break;
                        case 'v': ic = '\v'; break;
                        case 'f': ic = '\f'; break;
                        case 'u':
                            // Reads an Unicode Char like \uXXXX
                            icu = 0;
                            unchecked
                            {
                                int vHex;
                                for( int x = 0; x < 4; ++x )
                                {
                                    vHex = FromHexDigit( Peek() );
                                    if( vHex < 0 ) return (int)JSParserError.ErrorStringEmbeddedUnicodeValue;
                                    Debug.Assert( vHex < 16 );
                                    icu *= 16;
                                    icu += (uint)vHex;
                                    Read();
                                }
                            }
                            ic = (int)icu;
                            break;
                        case 'x':
                            // Allow only \xNN (2 digits): this is the norm.
                            if( IsPositiveHexNumber( out icu, 2 ) != 2 ) return (int)JSParserError.ErrorStringEmbeddedHexaValue;
                            ic = (int)icu;
                            break;
                        case '\r':  // Read transforms Line Separator '\u2028' and Paragraph Separator '\u2029' in '\n' 
                            // New JS (1.5?) supports the \ as a line continuation: we can just continue our loop...
                            // If a \n follows, we eat it. If no '\n' follows, this is an error.
                            if( !Read( '\n' ) ) return (int)JSParserError.ErrorStringUnexpectedCRInLineContinuation;
                            ic = '\n';
                            break;
                        case '\n':
                            // Read transforms Line Separator '\u2028' and Paragraph Separator '\u2029' in '\n' 
                            // New JS (1.5?) supports the \ as a line continuation: we can just continue our loop...
                            break;
                        case -1: return (int)JSParserError.ErrorStringUnterminated;
                        default: break;
                    }
                }
                _buffer.Append( (char)ic );
            }
            return (int)JSParserToken.String;
        }

        private int ReadIdentifier( int ic )
        {
            Debug.Assert( IsIdentifierStartChar( ic ) );
            _buffer.Length = 0;
            for( ; ; )
            {
                _buffer.Append( (char)ic );
                if( (IsIdentifierChar( ic = Peek() )) ) Read();
                else break;
            }
            _identifierValue = _buffer.ToString();
            switch( _identifierValue )
            {
                case "instanceof": return (int)JSParserToken.InstanceOf;
                case "delete": return (int)JSParserToken.Delete;
                case "new": return (int)JSParserToken.New;
                case "typeof": return (int)JSParserToken.TypeOf;
                case "void": return (int)JSParserToken.Void;
                case "NaN": return (int)JSParserToken.NaN;
                case "Infinity": return (int)JSParserToken.Infinity;
            }
            return (int)JSParserToken.Identifier;
        }

    }
}
