using System;

namespace CrypTool.JosseCipherAnalyzer.AttackTypes
{
    /// <summary>
    /// Simulated annealing with random restarts. A step swaps two key letters or replaces one by an unused
    /// letter; worse keys are accepted with a probability that shrinks as the temperature cools down.
    /// The start temperature is taken from the cost function, so the schedule fits n-grams, IoC and entropy alike.
    /// </summary>
    public class SimulatedAnnealing : AttackType
    {
        private const double CoolingRate = 0.97;
        private const double FinalTemperatureRatio = 0.001;
        private const int StepsPerTemperature = 200;
        private const double InitialAcceptance = 0.8;
        private const int TemperatureSamples = 100;

        // The last move, so it can be undone
        private int _movePosition1;
        private int _movePosition2;
        private int _moveOldLetter;

        public SimulatedAnnealing(string alphabet, int keyLength, int restarts)
            : base(alphabet, keyLength, restarts)
        {
        }

        protected override ResultEntry Search(CipherImplementation cipher, int[] ciphertext)
        {
            int[] plaintext = new int[ciphertext.Length];
            int[] key = new int[KeyLength];
            int[] bestKey = new int[KeyLength];
            bool[] used = new bool[Alphabet.Length];
            int temperatureSteps = (int)Math.Ceiling(Math.Log(FinalTemperatureRatio) / Math.Log(CoolingRate));
            ResultEntry best = null;

            for (int restart = 0; restart < Restarts; restart++)
            {
                RandomKey(key, used);
                double cost = Evaluate(cipher, key, ciphertext, plaintext);
                double bestCost = cost;
                Array.Copy(key, bestKey, KeyLength);
                double temperature = InitialTemperature(cipher, key, used, cost, ciphertext, plaintext);

                for (int step = 0; step < temperatureSteps; step++)
                {
                    for (int i = 0; i < StepsPerTemperature; i++)
                    {
                        Move(key, used);
                        double newCost = Evaluate(cipher, key, ciphertext, plaintext);
                        double delta = newCost - cost;
                        if (delta >= 0 || Random.NextDouble() < Math.Exp(delta / temperature))
                        {
                            cost = newCost;
                            if (cost > bestCost)
                            {
                                bestCost = cost;
                                Array.Copy(key, bestKey, KeyLength);
                            }
                        }
                        else
                        {
                            UndoMove(key, used);
                        }
                    }

                    if (ShouldStop)
                    {
                        return null;
                    }
                    temperature *= CoolingRate;
                    OnProgressChanged((restart + (step + 1.0) / temperatureSteps) / Restarts);
                }

                ResultEntry result = CreateResult(cipher, bestKey, bestCost, ciphertext, plaintext);
                OnResultFound(result);
                if (best == null || result.Value > best.Value)
                {
                    best = result;
                }
            }

            return best;
        }

        /// <summary>
        /// Chooses the temperature at which a typical worse neighbour is accepted with <see cref="InitialAcceptance"/>.
        /// </summary>
        private double InitialTemperature(CipherImplementation cipher, int[] key, bool[] used, double cost, int[] ciphertext, int[] plaintext)
        {
            double sum = 0;
            int count = 0;
            for (int i = 0; i < TemperatureSamples; i++)
            {
                Move(key, used);
                double delta = Evaluate(cipher, key, ciphertext, plaintext) - cost;
                UndoMove(key, used);
                if (delta < 0)
                {
                    sum -= delta;
                    count++;
                }
            }
            return count == 0 || sum == 0 ? double.Epsilon : sum / count / -Math.Log(InitialAcceptance);
        }

        private void Move(int[] key, bool[] used)
        {
            bool canReplace = KeyLength < Alphabet.Length;
            if (KeyLength == 1 && !canReplace)
            {
                // A one letter alphabet has no neighbours; "replace" the letter by itself
                _movePosition1 = 0;
                _movePosition2 = -1;
                _moveOldLetter = key[0];
                return;
            }
            if (KeyLength > 1 && (!canReplace || Random.Next(2) == 0))
            {
                // Swap two key letters
                _movePosition1 = Random.Next(KeyLength);
                do
                {
                    _movePosition2 = Random.Next(KeyLength);
                } while (_movePosition2 == _movePosition1);
                Swap(key, _movePosition1, _movePosition2);
                return;
            }

            // Replace one key letter by an unused one
            _movePosition1 = Random.Next(KeyLength);
            _movePosition2 = -1;
            _moveOldLetter = key[_movePosition1];
            int letter;
            do
            {
                letter = Random.Next(Alphabet.Length);
            } while (used[letter]);
            used[_moveOldLetter] = false;
            used[letter] = true;
            key[_movePosition1] = letter;
        }

        private void UndoMove(int[] key, bool[] used)
        {
            if (_movePosition2 >= 0)
            {
                Swap(key, _movePosition1, _movePosition2);
                return;
            }
            used[key[_movePosition1]] = false;
            used[_moveOldLetter] = true;
            key[_movePosition1] = _moveOldLetter;
        }

        private static void Swap(int[] key, int i, int j)
        {
            int letter = key[i];
            key[i] = key[j];
            key[j] = letter;
        }
    }
}
