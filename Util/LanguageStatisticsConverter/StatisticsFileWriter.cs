using System;
using System.IO;
using System.IO.Compression;

namespace LanguageStatisticsConverter
{
    /// <summary>
    /// Writes statistics files according to the new language statistics file format.
    /// </summary>
    public static class StatisticsFileWriter
    {
        /// <summary>
        /// Magic number (ASCII string 'CTLS') for this file format.
        /// </summary>
        public const uint FileFormatMagicNumber = 'C' + ('T' << 8) + ('L' << 16) + ('S' << 24);

        public static void WriteFile(Grams inputGrams, string outputFile)
        {
            using (FileStream fs = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
            using (var gz = new GZipStream(fs, CompressionMode.Compress))
            using (var bw = new BinaryWriter(gz))
            {
                bw.Write(FileFormatMagicNumber);
                bw.Write(inputGrams.GramLength);
                bw.Write(inputGrams.Alphabet);
                foreach (var frequencyValue in inputGrams.GetFlattenedFrequencies())
                {
                    bw.Write(frequencyValue);
                }
            }
        }
    }
}
