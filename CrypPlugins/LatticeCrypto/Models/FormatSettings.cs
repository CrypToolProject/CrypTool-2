using System;

namespace LatticeCrypto.Models
{
    [Serializable]
    public static class FormatSettings
    {
        public static string LatticeTagOpen = "[";
        public static string LatticeTagClosed = "]";
        public static string VectorTagOpen = "{";
        public static string VectorTagClosed = "}";
        public static char VectorSeparator = ',';
        public static char CoordinateSeparator = ';';
    }
}
