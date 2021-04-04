using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;

namespace LanguageStatisticsGenerator
{        
    public class NGrams
    {
        public static Dictionary<string, string> alphabets = new Dictionary<string, string>
        {
                {"en", "ABCDEFGHIJKLMNOPQRSTUVWXYZ" },                      // English
                {"de", "ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÜß" },                  // German
                {"fr", "ABCDEFGHIJKLMNOPQRSTUVWXYZ" },                      // French
                {"es", "ABCDEFGHIJKLMNOPQRSTUVWXYZÑ" },                     // Spanish
                {"it", "ABCDEFGHIJKLMNOPQRSTUVWXYZ" },                      // Italian
                {"hu", "ABCDEFGHIJKLMNOPQRSTUVWXYZ" },                      // Hungarian
                {"ru", "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ" },               // Russian
                {"cs", "ABCDEFGHIJKLMNOPQRSTUVWXYZ" },                      // Slovak
                {"la", "ABCDEFGHIJKLMNOPQRSTUVWXYZ" },                      // Latin
                {"el", "ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ" },                        // Greek
                {"nl", "ABCDEFGHIJKLMNOPQRSTUVWXYZ"},                       // Dutch
                {"sv", "ABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ"},                    // Swedish
                {"pt", "ABCDEFGHIJKLMNOPQRSTUVWXYZ"},                       // Portuguese 
                {"pl", "AĄBCĆDEĘFGHIJKLŁMNŃOÓPQRSŚTUVWXYZŹŻ"},              // Polish
                {"tr", "ABCÇDEFGĞHIİJKLMNOÖPRSŞTUÜVYZ" }                    // Turkish
        };

        private ConcurrentQueue<string> _fileQueue = new ConcurrentQueue<string>();
        private object _lockobject = new object();
        
        private const int NGRAM_SIZE = 5;
        private const int WORKERS = 8;

        public string _alphabet;

        //public uint[,,,,,,] freq7;
        //public uint[,,,,,] _freq6;
        public uint[,,,,] _freq5;
        public uint[,,,] _freq4;
        public uint[,,] _freq3;
        public uint[,] _freq2;
        public uint[] _freq1;

        public uint _max;
        public ulong _sum;

        // create statistics from directory
        public NGrams(string alphabet, string path, bool useSpace)
        {
            if (!useSpace) alphabet.Replace(" ", "");
            if (useSpace && !alphabet.Contains(" ")) alphabet += " ";
            _alphabet = alphabet;

            //freq7 = new uint[alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length];
            //_freq6 = new uint[alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length];
            _freq5 = new uint[alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length];
            _freq4 = new uint[alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length];
            _freq3 = new uint[alphabet.Length, alphabet.Length, alphabet.Length];
            _freq2 = new uint[alphabet.Length, alphabet.Length];
            _freq1 = new uint[alphabet.Length];

            if (path.ToUpper().EndsWith("XML"))
            {
                ReadInWikipediaXML(path);
            }
            else
            {
                ReadInGutenbergZips(path);
            }
        }

        /// <summary>
        /// This method parses and reads a dumped wikipedia in xml format
        /// </summary>
        /// <param name="path"></param>
        private void ReadInWikipediaXML(string path)
        {
            var startTime = DateTime.Now;
            Console.WriteLine("Parsing Wikipedia xml file: {0}", path);
            var letterBuffer = new int[6];
            long totalValidLetters = 0;
            long pagesCount = 0;

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = XmlReader.Create(stream))
            {
                var textTagFound = false;
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name.ToUpper().Equals("TEXT"))
                            {
                                textTagFound = true;

                            }
                            break;
                        case XmlNodeType.Text:
                            if (textTagFound)
                            {
                                var text = reader.Value;
                                var countCurlyBrackets = 0;
                                var countSquareBrackets = 0;

                                foreach (var c in text)
                                {
                                    var addLetter = char.ToUpper(c);
                                    switch (addLetter)
                                    {
                                        case '{':
                                            countCurlyBrackets++;
                                            continue;
                                        case '}':
                                            countCurlyBrackets--;
                                            continue;
                                        case '[':
                                            countSquareBrackets++;
                                            continue;
                                        case ']':
                                            countSquareBrackets--;
                                            continue;
                                    }
                                    //we only count alphabet letters and we only count when we are outside of [] and of {}
                                    if (!_alphabet.Contains(addLetter) || countCurlyBrackets > 0 || countSquareBrackets > 0)
                                    {
                                        continue;
                                    }

                                    totalValidLetters++;
                                    letterBuffer[0] = letterBuffer[1];
                                    letterBuffer[1] = letterBuffer[2];
                                    letterBuffer[2] = letterBuffer[3];
                                    letterBuffer[3] = letterBuffer[4];
                                    letterBuffer[4] = letterBuffer[5];
                                    letterBuffer[5] = _alphabet.IndexOf(addLetter);
                                    /*if (totalValidLetters > 5)
                                    {
                                        _freq6[letterBuffer[0], letterBuffer[1], letterBuffer[2], letterBuffer[3], letterBuffer[4], letterBuffer[5]]++;
                                    }*/
                                    if (totalValidLetters > 4)
                                    {
                                        _freq5[letterBuffer[0], letterBuffer[1], letterBuffer[2], letterBuffer[3], letterBuffer[4]]++;
                                    }
                                    if (totalValidLetters > 3)
                                    {
                                        _freq4[letterBuffer[0], letterBuffer[1], letterBuffer[2], letterBuffer[3]]++;
                                    }
                                    if (totalValidLetters > 2)
                                    {
                                        _freq3[letterBuffer[0], letterBuffer[1], letterBuffer[2]]++;
                                    }
                                    if (totalValidLetters > 1)
                                    {
                                        _freq2[letterBuffer[0], letterBuffer[1]]++;
                                    }
                                    _freq1[_alphabet.IndexOf(addLetter)]++;
                                }
                                pagesCount++;
                                if(pagesCount % 1000 == 0)
                                {
                                    Console.Write("\rProcessed a total of {0} pages...", pagesCount);
                                }
                            }
                            break;
                        default:
                            textTagFound = false;
                            break;

                    }
                }
            }
            Console.WriteLine("\rProcessed a total of {0} pages with a total of {1} valid letters in {2}", pagesCount, totalValidLetters, DateTime.Now - startTime);
        }

        /// <summary>
        /// This method starts workers who walk over a directory, parse all zip files and expect them containing Gutenberg library txt files
        /// </summary>
        /// <param name="path"></param>
        private void ReadInGutenbergZips(string path)
        {
            Console.WriteLine("Creating files list");
            string[] files = Directory.GetFiles(path, "*.zip", SearchOption.AllDirectories);
            Console.WriteLine("Files list created");

            foreach (var file in files)
            {
                _fileQueue.Enqueue(file);
            }

            Console.WriteLine("Starting worker tasks");
            var tasks = new List<Task>();
            var totalFiles = _fileQueue.Count;
            for (var i = 0; i < WORKERS; i++)
            {
                var id = i;
                var task = Task.Run(() => ProcessGutenbergBookZipFiles(id, totalFiles));
                tasks.Add(task);
                while (task.Status != TaskStatus.Running)
                {
                    Thread.Sleep(10);
                }
            }
            Console.WriteLine("Worker tasks started");

            Console.WriteLine("Waiting for worker tasks to finish");
            tasks.WaitAll();
            Console.WriteLine("Worker tasks finished");
            Console.WriteLine("Calculating statistics");
            GetMaxAndSum(NGRAM_SIZE);
            Console.WriteLine("Finished calculating statistics");
            GC.Collect();
            GC.WaitForFullGCComplete();
        }

        public void ProcessGutenbergBookZipFiles(int taskId, int totalFiles)
        {
            Console.WriteLine("Task {0} started", taskId);
            DateTime startTime = DateTime.Now;

            var book = new char[1024 * 1024 * 100];
            //var freq6 = new uint[_alphabet.Length, _alphabet.Length, _alphabet.Length, _alphabet.Length, _alphabet.Length, _alphabet.Length];
            var freq5 = new uint[_alphabet.Length, _alphabet.Length, _alphabet.Length, _alphabet.Length, _alphabet.Length];
            var freq4 = new uint[_alphabet.Length, _alphabet.Length, _alphabet.Length, _alphabet.Length];
            var freq3 = new uint[_alphabet.Length, _alphabet.Length, _alphabet.Length];
            var freq2 = new uint[_alphabet.Length, _alphabet.Length];
            var freq1 = new uint[_alphabet.Length];
            
            long totalValidLetters = 0;
            
            DateTime lastProcessTime = DateTime.Now;

            while (_fileQueue.Count > 0)
            {                
                if (taskId == 0 && DateTime.Now > lastProcessTime.AddSeconds(1))
                {
                    lastProcessTime = DateTime.Now;
                    int finished = totalFiles - _fileQueue.Count;

                    var totalSec = (DateTime.Now - startTime).TotalSeconds;
                    var speed = totalSec / (float)finished;
                    var remainingSeconds = (int)(speed * (totalFiles - finished));
                    TimeSpan remainingTime = new TimeSpan(0, 0, 0, remainingSeconds);

                    Console.Write("\rFinished {0} from {1} files: {2:0.##} % ({3} left)     ", finished, totalFiles, ((double)finished) / totalFiles * 100, remainingTime);
                }

                bool startfound = false;
                int index = 0;
                int offset = 0;
                var letterBuffer = new int[6];
                string filename;
                                
                if (_fileQueue.TryDequeue(out filename))
                {
                    //read unzipped text file(s) into memory
                    try
                    {
                        using (ZipArchive zipfile = ZipFile.Open(filename, ZipArchiveMode.Read))
                        {
                            foreach (ZipArchiveEntry entry in zipfile.Entries)
                            {
                                if (!entry.FullName.EndsWith("txt", StringComparison.OrdinalIgnoreCase))
                                {
                                    continue;
                                }
                                using (var stream = entry.Open())
                                {
                                    using (StreamReader streamReader = new StreamReader(stream))
                                    {
                                        while (!streamReader.EndOfStream && entry.Length - offset != 0)
                                        {
                                            offset += streamReader.Read(book, offset, (int)(entry.Length - offset));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception during reading of {0}: ", ex.Message);
                        continue;
                    }

                    //process text file(s)               
                    while (index < offset - 8)
                    {
                        // search for start line
                        // Gutenberg text files always start with ***START
                        if (!startfound)
                        {
                            if (book[index + 0] == '*' &&
                                book[index + 1] == '*' &&
                                book[index + 2] == '*' &&
                                book[index + 3] == ' ' &&
                               (book[index + 4] == 'S' || book[index + 4] == 's') &&
                               (book[index + 5] == 'T' || book[index + 5] == 't') &&
                               (book[index + 6] == 'A' || book[index + 6] == 'a') &&
                               (book[index + 7] == 'R' || book[index + 7] == 'r') &&
                               (book[index + 8] == 'T' || book[index + 8] == 't'))
                            {
                                while (book[index] != '\r' && book[index] != '\n' && index < offset - 8)
                                {
                                    index++;
                                }
                                startfound = true;
                                continue;
                            }
                            index++;
                            continue;
                        }

                        // search for end line
                        // Gutenberg text files always end with ***END
                        if (book[index + 0] == '*' &&
                             book[index + 1] == '*' &&
                             book[index + 2] == '*' &&
                             book[index + 3] == ' ' &&
                            (book[index + 4] == 'E' || book[index + 4] == 'e') &&
                            (book[index + 5] == 'N' || book[index + 5] == 'n') &&
                            (book[index + 6] == 'D' || book[index + 6] == 'd'))
                        {
                            break;
                        }

                        var addLetter = char.ToUpper(book[index]);

                        //we only count alphabet letters
                        if (_alphabet.Contains(addLetter))
                        {
                            totalValidLetters++;
                            letterBuffer[0] = letterBuffer[1];
                            letterBuffer[1] = letterBuffer[2];
                            letterBuffer[2] = letterBuffer[3];
                            letterBuffer[3] = letterBuffer[4];
                            letterBuffer[4] = letterBuffer[5];
                            letterBuffer[5] = _alphabet.IndexOf(addLetter);
                            /*if (totalValidLetters > 5)
                            {
                                freq6[letterBuffer[0], letterBuffer[1], letterBuffer[2], letterBuffer[3], letterBuffer[4], letterBuffer[5]]++;
                            }*/
                            if (totalValidLetters > 4)
                            {
                                freq5[letterBuffer[0], letterBuffer[1], letterBuffer[2], letterBuffer[3], letterBuffer[4]]++;
                            }
                            if (totalValidLetters > 3)
                            {
                                freq4[letterBuffer[0], letterBuffer[1], letterBuffer[2], letterBuffer[3]]++;
                            }
                            if (totalValidLetters > 2)
                            {
                                freq3[letterBuffer[0], letterBuffer[1], letterBuffer[2]]++;
                            }
                            if (totalValidLetters > 1)
                            {
                                freq2[letterBuffer[0], letterBuffer[1]]++;
                            }
                            freq1[_alphabet.IndexOf(addLetter)]++;
                        }
                        index++;
                    }
                }
            }

            //sum all local thread statistics in the one big statistics
            lock (_lockobject)
            {
                Console.WriteLine("Task {0} -> merging statistics... Counted {1} valid letters", taskId, totalValidLetters);
                for (int a = 0; a < _alphabet.Length; a++)
                {
                    _freq1[a] += freq1[a];
                    for (int b = 0; b < _alphabet.Length; b++)
                    {
                        _freq2[a, b] += freq2[a, b];
                        for (int c = 0; c < _alphabet.Length; c++)
                        {
                            _freq3[a, b, c] += freq3[a, b, c];
                            for (int d = 0; d < _alphabet.Length; d++)
                            {
                                _freq4[a, b, c, d] += freq4[a, b, c, d];
                                for (int e = 0; e < _alphabet.Length; e++)
                                {
                                    _freq5[a, b, c, d, e] += freq5[a, b, c, d, e];
                                    /*for (int f = 0; f < _alphabet.Length; f++)
                                    {
                                        {
                                            _freq6[a, b, c, d, e, f] += freq6[a, b, c, d, e, f];
                                        }
                                    }*/
                                }
                            }
                        }
                    }
                }
                Console.WriteLine("Merging statistics done");
            }
            Console.WriteLine("Task {0} finished", taskId);
        }
     

        public NGrams(string filename)
        {
            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                using (var gz = new GZipStream(fs, CompressionMode.Decompress))
                {
                    _alphabet = (string)bf.Deserialize(gz);
                    _max = (uint)bf.Deserialize(gz);
                    _sum = (ulong)bf.Deserialize(gz);
                    _freq4 = (uint[,,,])bf.Deserialize(gz);
                }
            }
        }

        void GetMaxAndSum(int n)
        {
            _sum = 0;
            _max = 0;

            /*if (n == 7)
            {
                for (int a = 0; a < alphabet.Length; a++)
                    for (int b = 0; b < alphabet.Length; b++)
                        for (int c = 0; c < alphabet.Length; c++)
                            for (int d = 0; d < alphabet.Length; d++)
                                for (int e = 0; e < alphabet.Length; e++)
                                    for (int f = 0; f < alphabet.Length; f++)
                                        for (int g = 0; g < alphabet.Length; g++)
                                        {
                                            uint x = freq7[a, b, c, d, e, f, g];
                                            if (max < x) max = x;
                                            sum += x;
                                        }
            }
             if (n == 6)
            {
                for (int a = 0; a < _alphabet.Length; a++)
                    for (int b = 0; b < _alphabet.Length; b++)
                        for (int c = 0; c < _alphabet.Length; c++)
                            for (int d = 0; d < _alphabet.Length; d++)
                                for (int e = 0; e < _alphabet.Length; e++)                                
                                    for (int f = 0; f < _alphabet.Length; f++)
                                    {
                                        uint x = _freq6[a, b, c, d, e,f];
                                        if (_max < x) _max = x;
                                        _sum += x;
                                    }
            }
            else else*/
            if (n == 5)
            {
                for (int a = 0; a < _alphabet.Length; a++)
                    for (int b = 0; b < _alphabet.Length; b++)
                        for (int c = 0; c < _alphabet.Length; c++)
                            for (int d = 0; d < _alphabet.Length; d++)
                                for (int e = 0; e < _alphabet.Length; e++)
                                {
                                    uint x = _freq5[a, b, c, d, e];
                                    if (_max < x) _max = x;
                                    _sum += x;
                                }
            }
            else if (n == 4)
            {
                for (int a = 0; a < _alphabet.Length; a++)
                    for (int b = 0; b < _alphabet.Length; b++)
                        for (int c = 0; c < _alphabet.Length; c++)
                            for (int d = 0; d < _alphabet.Length; d++)
                            {
                                uint x = _freq4[a, b, c, d];
                                if (_max < x) _max = x;
                                _sum += x;
                            }
            }
            else if (n == 3)
            {
                for (int a = 0; a < _alphabet.Length; a++)
                    for (int b = 0; b < _alphabet.Length; b++)
                        for (int c = 0; c < _alphabet.Length; c++)
                        {
                            uint x = _freq3[a, b, c];
                            if (_max < x) _max = x;
                            _sum += x;
                        }
            }
            else if (n == 2)
            {
                for (int a = 0; a < _alphabet.Length; a++)
                    for (int b = 0; b < _alphabet.Length; b++)
                    {
                        uint x = _freq2[a, b];
                        if (_max < x) _max = x;
                        _sum += x;
                    }
            }
            else if (n == 1)
            {
                for (int a = 0; a < _alphabet.Length; a++)
                {
                    uint x = _freq1[a];
                    if (_max < x) _max = x;
                    _sum += x;
                }
            }
        }


        /// <summary>
        /// Magic number (ASCII string 'CTLS') for statistics file format.
        /// </summary>
        public const uint FileFormatMagicNumber = 'C' + ('T' << 8) + ('L' << 16) + ('S' << 24);

        private void WriteStatisticsFile(int gramLength, IEnumerable<float> frequencies, string outputFile, string langCode)
        {
            using (FileStream fs = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
            using (var gz = new GZipStream(fs, CompressionMode.Compress))
            using (var bw = new BinaryWriter(gz))
            {
                bw.Write(FileFormatMagicNumber);
                bw.Write(langCode);
                bw.Write(gramLength);                
                bw.Write(_alphabet);
                foreach (var frequencyValue in frequencies)
                {
                    bw.Write(frequencyValue);
                }
            }
        }

        private static IEnumerable<float> CalculateLogs(Array freq, ulong sum)
        {            
            return freq.Cast<uint>().Select(value => (float)Math.Log(value == 0 ? 1.0 / sum : value  / (double)sum));
        }

        public void WriteGZ(string filename, string langCode)
        {
            var freqs = new Array[] { _freq1, _freq2, _freq3, _freq4, _freq5/*, _freq6, freq7*/ };
            for (int gramLength = NGRAM_SIZE; gramLength >= 1; gramLength--)
            {
                Console.WriteLine("Writing {0}-grams", gramLength);
                GetMaxAndSum(gramLength);
                WriteStatisticsFile(gramLength, CalculateLogs(freqs[gramLength-1], _sum), string.Format(filename + ".gz", gramLength), langCode);
            }
        }
    }

    public class Program
    {                     
        public static void Main(string[] args)
        {
            if(args.Length!=2)
            {
                Console.WriteLine("Usage: {0} textcorpusdirectory (alphabetfile | language selector)", AppDomain.CurrentDomain.FriendlyName);
                Console.WriteLine("\nSpecify a predefined language selector or a file, that\ncontains the alphabet as a single line of text (UTF-8).");
                Console.WriteLine("The following alphabets are predefined:");
                foreach (var a in NGrams.alphabets.OrderBy(x => x.Key))
                    Console.WriteLine("\t{0}: {1}", a.Key, a.Value);
                return;
            }

            string alphabet;

            string langCode = args[1].ToLower();

            if (NGrams.alphabets.ContainsKey(langCode))
            {
                alphabet = NGrams.alphabets[langCode];
            }
            else
            {
                try
                {
                    alphabet = File.ReadAllText(args[1], Encoding.UTF8);
                    alphabet = Regex.Replace(alphabet, "[\r\n]", "");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Error while reading alphabet file '{0}'", args[1]));
                    return;
                }
                langCode = "lang";
            }

            alphabet = alphabet.ToUpper();

            var duplicates = alphabet.Distinct().Where(c => alphabet.Count(d => c == d) > 1);
            if (duplicates.Count() > 0)
            {
                Console.WriteLine("Error: alphabet contains duplicate characters: '" + string.Join("", duplicates.OrderBy(c => c)) + "'");
                return;
            }

            string outname = langCode + "-{0}gram-nocs";

            string sentencesFilename = args[0];

            DateTime startDateTime = DateTime.Now;

            try
            {
                Console.WriteLine("creating statistics without spaces...");
                new NGrams(alphabet, sentencesFilename, false).WriteGZ(outname, langCode);
                Console.WriteLine("creating statistics with spaces...");
                new NGrams(alphabet, sentencesFilename, true).WriteGZ(outname + "-sp", langCode);
            }
            catch(Exception ex)
            {
                Console.WriteLine(string.Format("Error while reading text corpus file '{0}': {1}", sentencesFilename, ex.Message));
                return;
            }
            Console.WriteLine("Creating language statistics took {0}", DateTime.Now - startDateTime);
        }        
    }
    public static class ThreadExtension
    {
        public static void WaitAll(this IEnumerable<Task> tasks)
        {
            if (tasks != null)
            {
                foreach (var task in tasks)
                {
                    task.Wait();
                }
            }
        }
    }
}