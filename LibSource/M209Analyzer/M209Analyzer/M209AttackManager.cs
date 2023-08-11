using M209AnalyzerLib.Common;
using M209AnalyzerLib.Enums;
using M209AnalyzerLib.M209;
using System;
using System.Threading;

namespace M209AnalyzerLib
{
    public class M209AttackManager
    {
        #region Events        
        // Event for Log Message        
        public event EventHandler<OnLogMessageEventArgs> OnLogMessage;
        public class OnLogMessageEventArgs : EventArgs
        {
            public string Message { get; set; }
            public string LogLevel { get; set; }
            public OnLogMessageEventArgs(string message, string logLevel)
            {
                Message = message;
                LogLevel = logLevel;
            }
        }

        public event EventHandler<OnNewBestListEntryEventArgs> OnNewBestListEntry;
        public class OnNewBestListEntryEventArgs : EventArgs
        {
            public Key Key { get; set; }
            public double Score { get; set; }
            public int[] Decryption { get; set; }
            public OnNewBestListEntryEventArgs(double score, Key key, int[] decryption)
            {
                Key = key;
                Score = score;
                Decryption = decryption;
            }
        }

        public event EventHandler<OnProgressStatusChangedEventArgs> OnProgressStatusChanged;
        public class OnProgressStatusChangedEventArgs : EventArgs
        {
            public string AttackType { get; set; }
            public string Phase { get; set; }
            public int Counter { get; set; }
            public int TargetValue { get; set; }
            public TimeSpan ElapsedTime { get; set; }
            public long EvaluationCount { get; set; }

            public OnProgressStatusChangedEventArgs(string attackType, string phase, int counter, int targetValue, long evaluationCount, TimeSpan elapsedTime)
            {
                AttackType = attackType;
                Phase = phase;
                TargetValue = targetValue;
                Counter = counter;
                EvaluationCount = evaluationCount;
                ElapsedTime = elapsedTime;
            }
        }

        #endregion

        #region Properties
        public bool ShouldStop { get; set; }
        /// <summary>
        /// The language of the cipher-text
        /// </summary>
        public Language Language { get; set; } = Language.ENGLISH;

        /// <summary>
        /// Version of the used cipher-machine. This is important, because there were different instruction for creating key.
        /// </summary>
        public MachineVersion Version { get; set; } = MachineVersion.V1947;

        /// <summary>
        /// Set the simulation mode:
        /// 0 (default) - no simulation, 1 - ciphertext only, 2 - with crib.
        /// </summary>
        public int SimulationValue { get; set; } = 1;
        public int SimulationTextLength { get; set; } = 1500;
        public int SimulationOverlaps { get; set; } = 2;

        /// <summary>
        /// Set how often the main loop for the ciphertext only attack is repeated.
        /// </summary>
        public int Cycles { get; set; } = 2_147_483_647;

        /// <summary>
        /// Control how often the random trial phase in cipher text only attack should be repeated.
        /// Default is 200000 / CipherText.Length
        /// </summary>
        public int Phase1Trials { get; set; }

        /// <summary>
        /// Set the amount of threads should be used for the attacks
        /// </summary>
        public int Threads { get; set; } = 1;

        /// <summary>
        /// Path to the gram files.
        /// </summary>
        public string ResourcePath { get; set; }

        public string Crib { get; set; } = String.Empty;

        /// <summary>
        /// The cipher text in string format
        /// </summary>
        public string CipherText
        {
            get { return _cipherText; }
            set
            {
                _cipherText = value;
                Phase1Trials = 200_000 / _cipherText.Length;
            }
        }
        private string _cipherText = String.Empty;

        public bool SearchSlide = false;

        /// <summary>
        /// Parameters for the Simulated Annealing algorithm
        /// </summary>
        public SimulatedAnnealingParameters SAParameters { get; set; } = new SimulatedAnnealingParameters();

        /// <summary>
        /// For scoring used function
        /// </summary>
        public IScoring Scoring { get; set; }

        /// <summary>
        /// Simulation key, having the pin and lugssettings used for the generation of the simulation values.
        /// </summary>
        public Key SimulationKey { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public bool UseOwnBestList { get; set; } = false;
        public CtBestList BestList { get; } = new CtBestList();

        public TimeSpan ElapsedTime
        {
            get
            {
                EndTime = DateTime.Now;
                return EndTime.Subtract(StartTime);
            }
        }

        private DateTime _lastProgressUpdate = DateTime.Now;
        private const int UPDATE_INTERVAL_SECONDS = 1;

        public long EvaluationCount = 0;

        private readonly object LOCK = new object();

        #endregion

        public M209AttackManager(IScoring scoring)
        {
            Scoring = scoring;
            Stats.Load(Language, true);

            BestList.SetThrottle(true);
        }

        /// <summary>
        /// Simulation values gets generated from textfiles in resource path.
        /// </summary>
        public void UseSimulationValues()
        {
            SimulationKey = Simulation.CreateSimulationValues(this);

            CipherText = SimulationKey.CipherText;
            Crib = SimulationKey.Crib;

            LogMessage($"SIMULATION MODE: \n  Plaintext: \n {SimulationKey.Crib} Ciphertext: \n {SimulationKey.CipherText} \n", "info");
        }

        /// <summary>
        /// Log messages
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logLevel"></param>
        public void LogMessage(string message, string logLevel)
        {
            lock (LOCK)
            {
                OnLogMessage?.Invoke(this, new OnLogMessageEventArgs(message, logLevel));
            }
        }

        public void AddNewBestListEntry(double score, Key key, int[] decryption)
        {
            lock (LOCK)
            {
                if (UseOwnBestList)
                {
                    BestList.PushResult(score, key, decryption);
                }
                OnNewBestListEntry?.Invoke(this, new OnNewBestListEntryEventArgs(score, key, decryption));
            }
        }

        public void ProgressChanged(string attackType, string phase, int counter, int targetValue)
        {
            lock (LOCK)
            {
                if (DateTime.Now > _lastProgressUpdate.AddSeconds(UPDATE_INTERVAL_SECONDS))
                {
                    OnProgressStatusChanged?.Invoke(this, new OnProgressStatusChangedEventArgs(attackType, phase, counter, targetValue, EvaluationCount, ElapsedTime));
                }
            }
        }

        public void NewKeyFound()
        {
            lock (LOCK)
            {
                EvaluationCount++;
            }
        }

        private void StartTimeMeasurement()
        {
            StartTime = DateTime.Now;
        }
        public void CipherTextOnlyAttack(string cipherText)
        {
            CipherText = cipherText;
            CipherTextOnlyAttack();
        }

        public void CipherTextOnlyAttack()
        {
            StartTimeMeasurement();

            if (CipherText != String.Empty)
            {
                if (Threads > 1)
                {
                    ThreadPool.SetMinThreads(1, 1);
                    ThreadPool.SetMaxThreads(Threads, Threads);

                    CountdownEvent countdownEvent = new CountdownEvent(Threads);

                    for (int i = 0; i < Threads; i++)
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(o =>
                        {
                            try
                            {

                                Key key = new Key();
                                key.SetCipherText(CipherText);

                                if (SimulationKey != null)
                                {
                                    key.setOriginalKey(SimulationKey);
                                    key.setOriginalScore(Evaluate(EvalType.MONO, SimulationKey.CribArray, SimulationKey.CribArray));
                                }

                                LocalState localState = new LocalState(i);
                                CiphertextOnlyAttack.Solve(key, this, localState);
                            }
                            catch (Exception e)
                            {
                                LogMessage(e.Message, "Error");
                                throw;
                            }

                        }));
                    }

                    while (!ShouldStop)
                    {
                        try
                        {
                            countdownEvent.Wait(1000);
                        }
                        catch (Exception)
                        {
                            //do nothing
                        }
                    }
                }
                else
                {
                    Key key = new Key();
                    key.SetCipherText(CipherText);

                    if (SimulationKey != null)
                    {
                        key.setOriginalKey(SimulationKey);
                        key.setOriginalScore(Evaluate(EvalType.MONO, SimulationKey.CribArray, SimulationKey.CribArray));
                    }

                    LocalState localState = new LocalState(0);
                    CiphertextOnlyAttack.Solve(key, this, localState);
                }
            }
            else
            {
                throw new Exception("No ciphertext given!");
            }
        }


        public void KnownPlainTextAttack(string cipherText, string crib)
        {
            CipherText = cipherText;
            Crib = crib;

            KnownPlainTextAttack();
        }

        public void KnownPlainTextAttack()
        {
            StartTimeMeasurement();
            if (CipherText != String.Empty && Crib != String.Empty)
            {

                if (Threads > 1)
                {
                    ThreadPool.SetMinThreads(1, 1);
                    ThreadPool.SetMaxThreads(Threads, Threads);

                    for (int i = 0; i < Threads; i++)
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(o =>
                        {
                            try
                            {

                                Key key = new Key();
                                key.SetCipherTextAndCrib(CipherText, Crib);
                                if (SimulationKey != null)
                                {
                                    key.setOriginalKey(SimulationKey);
                                }
                                key.setOriginalScore(130000);

                                LocalState localState = new LocalState(i);
                                KnownPlaintextAttack.Solve(key, this, localState);
                            }
                            catch (Exception e)
                            {
                                LogMessage(e.Message, "Error");
                                throw;
                            }

                        }));
                    }
                }
                else
                {
                    Key key = new Key();
                    key.SetCipherText(CipherText);

                    key.SetCipherTextAndCrib(CipherText, Crib);
                    if (SimulationKey != null)
                    {
                        key.setOriginalKey(SimulationKey);
                    }
                    key.setOriginalScore(130000);

                    LocalState localState = new LocalState(0);
                    KnownPlaintextAttack.Solve(key, this, localState);
                }
            }
        }

        public double Evaluate(EvalType evalType, int[] decryptedText, int[] crib)
        {
            NewKeyFound();
            return Scoring.Evaluate(evalType, decryptedText, crib);
        }

        public void Attack()
        {
            BestList.SetDiscardSamePlaintexts(false);
            BestList.SetThrottle(true);

            if (SimulationValue != 0)
            {
                ReportManager.simulation = true;
                Key simulationKey = Simulation.CreateSimulationValues(this);
                if (SimulationValue == 2)
                {
                    KnownPlaintextAttack.SolveMultithreaded(simulationKey, this);
                }
                else
                {
                    CiphertextOnlyAttack.SolveMultithreaded(simulationKey, this);
                }
            }
            else
            {
                if (CipherText == null || CipherText.Length == 0)
                {
                    Console.WriteLine("\"Ciphertext or ciphertext file required when not in simulation mode\"\n");
                    Console.Read();
                    Environment.Exit(-1);
                }
                if (Crib != null && Crib.Length != 0)
                {
                    KnownPlaintextAttack.SolveMultithreaded(null, this);
                }
                else
                {
                    CiphertextOnlyAttack.SolveMultithreaded(null, this);
                }
            }
        }
    }
}
