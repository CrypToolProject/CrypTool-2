using System;
using System.IO;

namespace LanguageStatisticsConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: {0} <source directory> <output directory>", System.AppDomain.CurrentDomain.FriendlyName);
                Console.WriteLine("\nThe input language statistics files of the old format in the source directory will be converted to the new format in the output directory.");
                Console.WriteLine("\nThe output directory should not be the same as the input directory.");
                return;
            }

            var sourceDirectory = args[0];
            var destinationDirectory = args[1];

            foreach (var inputFilePath in Directory.EnumerateFiles(sourceDirectory, "*gram-nocs*.gz"))
            {
                var fileName = Path.GetFileName(inputFilePath);
                var fileNameParts = fileName.Split('-');
                if (fileNameParts.Length > 2)
                {
                    Console.Out.WriteLine($"Converting file '{fileName}'.");
                    var gramType = fileNameParts[1];
                    var outputFilePath = Path.Combine(destinationDirectory, fileName);
                    StatisticsFileWriter.WriteFile(LoadGrams(inputFilePath, gramType), outputFilePath);
                }
            }
        }

        private static Grams LoadGrams(string filePath, string gramType)
        {
            switch (gramType.ToLower())
            {
                case "1gram":
                    return new UniGrams(filePath);
                case "2gram":
                    return new BiGrams(filePath);
                case "3gram":
                    return new TriGrams(filePath);
                case "4gram":
                    return new QuadGrams(filePath);
                case "5gram":
                    return new PentaGrams(filePath);
                default:
                    throw new Exception($"Gram type '{gramType}' not supported by converter.");
            }
        }
    }
}
