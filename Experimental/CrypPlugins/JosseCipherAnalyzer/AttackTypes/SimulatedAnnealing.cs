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
        private int Restarts { get; set; }
        private ResultEntry _finalResultEntry;

        public SimulatedAnnealing(string alphabet, int keyLength, int restarts,
            JosseCipherAnalyzerPresentation presentation)
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
            Random random = new Random(Guid.NewGuid().GetHashCode());
            DateTime lastTime = DateTime.Now;
            CipherImplementation cipher = new CipherImplementation { Alphabet = new string(Alphabet) };
            char[] currentKey = KeyAlphabet.OrderBy(_ => random.Next()).Take(KeyLength).ToArray();

            char[] bestKey = (char[])currentKey.Clone();

            while (Restarts > 0)
            {
                const double alpha = 0.995;
                const double epsilon = 0.005;
                const double initTemperature = 400;
                double temperature = initTemperature;
                char[] nextKey = new char[KeyLength];
                cipher.BuildDictionaries(bestKey);
                string initialPlaintext = cipher.Decipher(ciphertext);
                double distance = Evaluator.Evaluate(initialPlaintext);

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
                    string plaintext = cipher.Decipher(ciphertext);
                    double currentCost = Evaluator.Evaluate(plaintext);

                    // Annealing
                    double delta = currentCost - distance;
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
                    OnProcessChanged(GetPercentage(initTemperature, temperature, alpha, epsilon));
                }

                currentKey = (char[])bestKey.Clone();
                Restarts--;
            }

            return _finalResultEntry;
        }

        private static double GetPercentage(double initTemp, double temp, double alpha, double epsilon)
        {
            double steps = Math.Log(initTemp * Math.Log(epsilon, alpha), alpha);
            return -((Math.Log(temp, alpha) + -Math.Log(initTemp, alpha) - 0) /
                (steps - 0) * (1 - 0) + 0);
        }

        private void GenerateNewKey(IReadOnlyList<char> c, IList<char> n)
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            do
            {
                for (int i = 0; i < c.Count; i++)
                {
                    n[i] = c[i];
                }

                int i1 = rnd.Next(n.Count);
                int i2 = rnd.Next(n.Count);
                char aux = n[i1];
                n[i1] = n[i2];
                // Exchange this letter with a probability
                if (rnd.Next(100) < 10)
                {
                    aux = Alphabet.OrderBy(_ => rnd.Next()).Take(1).ToArray().First();
                }

                n[i2] = aux;
            } while (n.Distinct().Count() != n.Count);
        }

        private static void Assign(IList<char> c, IReadOnlyList<char> n)
        {
            for (int i = 0; i < c.Count; i++)
            {
                c[i] = n[i];
            }
        }

        /// <summary>
        /// Adds an entry to the BestList
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="plaintext"></param>
        private void AddNewBestListEntry(char[] key, double value, string plaintext)
        {
            ResultEntry entry = new ResultEntry
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

                    //check for duplicates
                    foreach (ResultEntry e in Presentation.BestList)
                    {
                        if (entry.Key.Equals(e.Key))
                        {
                            return;
                        }
                    }

                    //Insert new entry at correct place to sustain order of list:
                    int insertIndex = Presentation.BestList.TakeWhile(e => e.Value > entry.Value).Count();
                    Presentation.BestList.Insert(insertIndex, entry);

                    if (Presentation.BestList.Count > MaxBestListEntries)
                    {
                        Presentation.BestList.RemoveAt(MaxBestListEntries);
                    }

                    int ranking = 1;
                    foreach (ResultEntry e in Presentation.BestList)
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