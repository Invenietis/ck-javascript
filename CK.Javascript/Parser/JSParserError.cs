using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Javascript
{
    public enum JSParserError
    {
        None = 0,

        /// <summary>
        /// Sign bit (bit n°31) is 1 to indicate an error 
        /// or the end of the input.
        /// This allows easy and efficient error/end test: any negative token value marks the end.
        /// </summary>
        IsErrorOrEndOfInput = -2147483648,

        /// <summary>
        /// The end of input (the two most sgnificant bit set).
        /// </summary>
        EndOfInput = IsErrorOrEndOfInput | (1 << 30),
        /// <summary>
        /// Error mask.
        /// </summary>
        ErrorMask = IsErrorOrEndOfInput | (1 << 29),

        /// <summary>
        /// Invalid character.
        /// </summary>
        ErrorInvalidChar = ErrorMask | 1,

        /// <summary>
        /// Error string mask.
        /// </summary>
        ErrorStringMask = ErrorMask | (1 << 28),

        /// <summary>
        /// Error number mask.
        /// </summary>
        ErrorNumberMask = ErrorMask | (1 << 27),

        /// <summary>
        /// Error regex mask.
        /// </summary>
        ErrorRegexMask = ErrorMask | (1 << 26),

        /// <summary>
        /// Whenever a non terminated string is encountered.
        /// </summary>
        ErrorStringUnterminated = ErrorStringMask | 1,
        /// <summary>
        /// Bad Unicode value embedded in a string.
        /// </summary>
        ErrorStringEmbeddedUnicodeValue = ErrorStringMask | 2,
        /// <summary>
        /// Bad hexadecimal value embedded in a string.
        /// </summary>
        ErrorStringEmbeddedHexaValue = ErrorStringMask | 4,
        /// <summary>
        /// Line continuation \ followed by a \r without \n after it.
        /// </summary>
        ErrorStringUnexpectedCRInLineContinuation = ErrorStringMask | 8,

        /// <summary>
        /// Unterminated number.
        /// </summary>
        ErrorNumberUnterminatedValue = ErrorNumberMask | 1,
        /// <summary>
        /// Invalid number value.
        /// </summary>
        ErrorNumberValue = ErrorNumberMask | 2,
        /// <summary>
        /// Number value is immediately followed by an identifier: 45D for example.
        /// </summary>
        ErrorNumberIdentifierStartsImmediately = ErrorNumberMask | 4,

        /// <summary>
        /// Whenever a non terminated regular expression.
        /// </summary>
        ErrorRegexUnterminated = ErrorRegexMask | 1,
    }
}
