using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace CrypTool.JosseCipherAnalyzer.AttackTypes
{
    public class SimulatedAnnealing : AttackType
    {
        private char[] Alphabet { get; }
        public string KeyAlphabet { get; }
        private int KeyLength { get; }
        private int Restarts { get; set;  }
        private ResultEntry _finalResultEntry;

        public SimulatedAnnealing(string alphabet, int keyLength, int restarts, JosseCipherAnalyzerPresentation presentation)
        {
            Alphabet = alphabet.ToUpper().ToCharArray();
            KeyAlphabet = alphabet.ToUpper();
            KeyLength = keyLength;
            Restarts = restarts;
            Presentation = presentation;
        }

        public override ResultEntry Start(string ciphertext)
        {
            ciphertext = ciphertext.ToUpper();
            var random = new Random(Guid.NewGuid().GetHashCode());
            var lastTime = DateTime.Now;
            var cipher = new CipherImplementation { Alphabet = new string(Alphabet) };
            var currentKey = KeyAlphabet.OrderBy(_ => random.Next()).Take(KeyLength).ToArray();

            var bestKey = (char[])currentKey.Clone();

            while (Restarts > 0)
            {
                const double alpha = 0.995;
                const double epsilon = 0.005;
                var temperature = 400.0;
                var nextKey = new char[KeyLength];
                cipher.BuildDictionaries(bestKey);
                var initialPlaintext = cipher.Decipher(ciphertext);
                var distance = Evaluator.Evaluate(initialPlaintext);

                if (ShouldStop)
                {
                    return null;
                }

                while (temperature > epsilon)
                {
                    if (ShouldStop)
                    {
                        return null;
                    }

                    // Generate new Key
                    GenerateNewKey(currentKey, nextKey);

                    // Get value for current Key
                    cipher.BuildDictionaries(nextKey);
                    var plaintext = cipher.Decipher(ciphertext);
                    var currentCost = Evaluator.Evaluate(plaintext);

                    // Annealing
                    var delta = currentCost - distance;
                    if (delta >= 0)
                    {
                        bestKey = (char[])currentKey.Clone();
                        Assign(currentKey, nextKey);
                        distance = delta + distance;
                        AddNewBestListEntry(bestKey, currentCost, plaintext);
                    }
                    else
                    {
                        //reset key
                        double probability = (float)random.NextDouble();
                        if (probability < Math.Pow(Math.E, delta / temperature))
                        {
                            bestKey = (char[])currentKey.Clone();
                            Assign(currentKey, nextKey);
                            distance = delta + distance;
                        }
                    }

                    if (DateTime.Now < lastTime.AddMilliseconds(100))
                    {
                        continue;
                    }

                    lastTime = DateTime.Now;

                    //cooling process on every iteration
                    temperature *= alpha;
                    OnProcessChanged(1 - temperature / 400.0);
                }

                currentKey = (char[])bestKey.Clone();
                Restarts--;
            }

            return _finalResultEntry;
        }

        private void GenerateNewKey(char[] c, char[] n)
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            do
            {
                for (var i = 0; i < c.Length; i++)
                    n[i] = c[i];
                var i1 = rnd.Next(n.Length);
                var i2 = rnd.Next(n.Length);
                var aux = n[i1];
                n[i1] = n[i2];
                // Exchange this letter with a probability
                if (rnd.Next(100) < 10)
                {
                    aux = Alphabet.OrderBy(_ => rnd.Next()).Take(1).ToArray().First();
                }

                n[i2] = aux;
            } while (n.Distinct().Count() != n.Length);
        }

        private static void Assign(IList<char> c, IReadOnlyList<char> n)
        {
            for (var i = 0; i < c.Count; i++)
                c[i] = n[i];
        }

        /// <summary>
        /// Adds an entry to the BestList
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="plaintext"></param>
        private void AddNewBestListEntry(char[] key, double value, string plaintext)
        {
            var entry = new ResultEntry
            {
                Key = new string(key),
                Text = plaintext,
                Value = value
            };

            if (Presentation.BestList.Count == 0 || entry.Value > Presentation.BestList.First().Value)
            {
                _finalResultEntry = entry;
            }

            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    if (Presentation.BestList.Count > 0 && entry.Value <= Presentation.BestList.Last().Value)
                    {
                        return;
                    }
                    //Insert new entry at correct place to sustain order of list:
                    var insertIndex = Presentation.BestList.TakeWhile(e => e.Value > entry.Value).Count();
                    Presentation.BestList.Insert(insertIndex, entry);

                    if (Presentation.BestList.Count > MaxBestListEntries)
                    {
                        Presentation.BestList.RemoveAt(MaxBestListEntries);
                    }
                    var ranking = 1;
                    foreach (var e in Presentation.BestList)
                    {
                        e.Ranking = ranking;
                        ranking++;
                    }
                }
                catch (Exception)
                {
                    // ignore
                }
            }, null);
        }
    }
}