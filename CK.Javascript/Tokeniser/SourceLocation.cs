using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.Javascript
{
    public struct SourceLocation
    {
        public const string NoSource = "(no source)";

        public static readonly SourceLocation Empty = new SourceLocation() { Source = NoSource };

        public string Source;
        public int Line;
        public int Column;

        public override int GetHashCode()
        {
            return Util.Hash.Combine( Util.Hash.StartValue, Source, Line, Column ).GetHashCode();
        }

        public override bool Equals( object obj )
        {
            if( obj is SourceLocation )
            {
                SourceLocation other = (SourceLocation)obj;
                return Line == other.Line && Column == other.Column && Source == other.Source;
            }
            return false;
        }

        public override string ToString()
        {
            return String.Format( "{0} - line {1}, column {2}", Source, Line, Column );
        }
    }
}
