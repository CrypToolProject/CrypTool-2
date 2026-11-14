using CrypTool.JosseCipherAnalyzer.Evaluator;
using System;
using System.Diagnostics;

namespace CrypTool.JosseCipherAnalyzer.AttackTypes
{
    /// <summary>
    /// Base of the key searches. A search knows nothing about the presentation: it reports
    /// candidates, progress and speed through events.
    /// </summary>
    public abstract class AttackType
    {
        private readonly Stopwatch _speedWatch = new Stopwatch();
        private long _keysSinceLastReport;
        private volatile bool _shouldStop;

        protected AttackType(string alphabet, int keyLength, int restarts)
        {
            Alphabet = alphabet;
            KeyLength = Math.Max(1, Math.Min(keyLength, alphabet.Length));
            Restarts = restarts;
            Random = new Random(Guid.NewGuid().GetHashCode());
        }

        protected string Alphabet { get; }
        protected int KeyLength { get; }
        protected int Restarts { get; }
        protected Random Random { get; }

        public IEvaluator Evaluator { get; set; }

        public bool ShouldStop
        {
            get => _shouldStop;
            set => _shouldStop = value;
        }

        /// <summary>Progress of this search between 0 and 1.</summary>
        public event EventHandler<double> ProgressChanged;

        /// <summary>A candidate for the best list.</summary>
        public event EventHandler<ResultEntry> ResultFound;

        /// <summary>Number of keys tested per second, raised about once a second.</summary>
        public event EventHandler<long> KeysPerSecondChanged;

        /// <summary>
        /// Searches the key of the given ciphertext and returns the best result, or null if stopped.
        /// </summary>
        public ResultEntry Start(string ciphertext)
        {
            CipherImplementation cipher = new CipherImplementation(Alphabet);
            int[] ciphertextNumbers = cipher.MapToNumbers(ciphertext.ToUpper());
            _speedWatch.Restart();
            _keysSinceLastReport = 0;
            return Search(cipher, ciphertextNumbers);
        }

        protected abstract ResultEntry Search(CipherImplementation cipher, int[] ciphertext);

        /// <summary>
        /// Decrypts with the given key and rates the plaintext.
        /// </summary>
        protected double Evaluate(CipherImplementation cipher, int[] key, int[] ciphertext, int[] plaintext)
        {
            cipher.SetKey(key);
            cipher.Decipher(ciphertext, plaintext);
            CountKey();
            return Evaluator.Evaluate(plaintext);
        }

        /// <summary>
        /// Draws a key of distinct random letters and marks them in <paramref name="used"/>.
        /// </summary>
        protected void RandomKey(int[] key, bool[] used)
        {
            Array.Clear(used, 0, used.Length);
            for (int i = 0; i < key.Length; i++)
            {
                int letter;
                do
                {
                    letter = Random.Next(Alphabet.Length);
                } while (used[letter]);
                key[i] = letter;
                used[letter] = true;
            }
        }

        protected ResultEntry CreateResult(CipherImplementation cipher, int[] key, double value, int[] ciphertext, int[] plaintext)
        {
            cipher.SetKey(key);
            cipher.Decipher(ciphertext, plaintext);
            return new ResultEntry
            {
                Key = cipher.MapToText(key),
                Text = cipher.MapToText(plaintext),
                Value = value
            };
        }

        protected void OnResultFound(ResultEntry entry)
        {
            ResultFound?.Invoke(this, entry);
        }

        protected void OnProgressChanged(double progress)
        {
            ProgressChanged?.Invoke(this, progress);
        }

        private void CountKey()
        {
            _keysSinceLastReport++;
            // Looking at the clock costs more than testing a key, so only do it now and then
            if ((_keysSinceLastReport & 0x3FF) != 0 || _speedWatch.ElapsedMilliseconds < 1000)
            {
                return;
            }
            long keysPerSecond = _keysSinceLastReport * 1000 / _speedWatch.ElapsedMilliseconds;
            _keysSinceLastReport = 0;
            _speedWatch.Restart();
            KeysPerSecondChanged?.Invoke(this, keysPerSecond);
        }
    }
}
