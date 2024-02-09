/*
   Copyright CrypTool 2 Team josef.matwich@gmail.com

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using M209AnalyzerLib.Common;
using M209AnalyzerLib.Enums;
using M209AnalyzerLib.M209;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

        public SimulatedAnnealing SimulatedAnnealing { get; set; }

        public SimulatedAnnealingPins SimulatedAnnealingPins { get; set; }
        public HillClimbLugs HillClimbLugs { get; set; }
        public HillClimbPins HillClimbPins { get; set; }
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

        public long EvaluationCount
        {
            get
            {
                long count = 0;
                if (EvaluationCounter.Length != 0)
                {
                    for (int i = 0; i < EvaluationCounter.Length; i++)
                    {
                        count += EvaluationCounter[i];
                    }
                }
                return count;
            }
        }

        private readonly object LOCK = new object();

        private int _taskCounter = 0;

        private int _taskTracker = -1;

        public CtBestList[] BestLists;
        public long[] EvaluationCounter;

        public Stats Stats;

        #endregion

        public M209AttackManager()
        {
            Stats = new Stats();
            Stats.Load(Language, true);

            BestList.SetThrottle(true);

            Scoring = new M209Scoring(Stats);
            SimulatedAnnealing = new SimulatedAnnealing(this);
        }

        public M209AttackManager(IScoring scoring)
        {
            Scoring = scoring;
            Stats = new Stats();
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

        public void AddNewBestListEntry(double score, Key key, int[] decryption, int taskId)
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
            if (DateTime.Now > _lastProgressUpdate.AddSeconds(UPDATE_INTERVAL_SECONDS))
            {
                OnProgressStatusChanged?.Invoke(this, new OnProgressStatusChangedEventArgs(attackType, phase, counter, targetValue, EvaluationCount, ElapsedTime));
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

        private void CipherTextOnlyTask()
        {
            try
            {

                Key key = new Key(this);
                key.SetCipherText(CipherText);
                LocalState localState = new LocalState(_taskCounter++);

                if (SimulationKey != null)
                {
                    key.setOriginalKey(SimulationKey);
                    key.setOriginalScore(Evaluate(EvalType.MONO, SimulationKey.CribArray, SimulationKey.CribArray, localState.TaskId));
                }

                CiphertextOnlyAttack cipherTextOnlyAttack = new CiphertextOnlyAttack(key, this, localState);

                cipherTextOnlyAttack.Solve();
                if (ShouldStop)
                {
                    return;
                }
            }
            catch (Exception e)
            {
                LogMessage(e.Message, "Error");
                throw;
            }
        }

        public async void CipherTextOnlyAttack()
        {
            StartTimeMeasurement();

            if (CipherText != String.Empty)
            {
                // Create a scheduler that uses two threads.
                LimitedConcurrencyLevelTaskScheduler lcts = new LimitedConcurrencyLevelTaskScheduler(Threads);
                List<Task> tasks = new List<Task>();

                // Create a TaskFactory and pass it our custom scheduler.
                TaskFactory factory = new TaskFactory(lcts);
                CancellationTokenSource cts = new CancellationTokenSource();

                //List<Action> tasks = new List<Action>();
                BestLists = new CtBestList[Threads];
                EvaluationCounter = new long[Threads];

                for (int i = 0; i < Threads; i++)
                {
                    //tasks.Add(CipherTextOnlyTask);
                    Task t = factory.StartNew(() =>
                    {
                        CipherTextOnlyTask();
                    }, cts.Token);
                }

                //foreach (var task in tasks)
                //{
                //    await Task.Run(task);
                //}

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

        private void KnownPlainTextTask()
        {
            try
            {

                Key key = new Key(this);
                key.SetCipherTextAndCrib(CipherText, Crib);
                if (SimulationKey != null)
                {
                    key.setOriginalKey(SimulationKey);
                }
                key.setOriginalScore(130000);

                LocalState localState = new LocalState(_taskCounter++);
                KnownPlaintextAttack knownPlaintextAttack = new KnownPlaintextAttack(key, this, localState);
                knownPlaintextAttack.Solve();
                if (ShouldStop)
                {
                    return;
                }
            }
            catch (Exception e)
            {
                LogMessage(e.Message, "Error");
                throw;
            }
        }

        public void KnownPlainTextAttack()
        {
            StartTimeMeasurement();
            if (CipherText != String.Empty && Crib != String.Empty)
            {
                List<Action> tasks = new List<Action>();

                EvaluationCounter = new long[Threads];

                for (int i = 0; i < Threads; i++)
                {
                    tasks.Add(KnownPlainTextTask);
                }

                foreach (var task in tasks)
                {
                    Task.Run(task);
                }
            }
        }

        public double Evaluate(EvalType evalType, int[] decryptedText, int[] crib, int taskId)
        {
            EvaluationCounter[taskId]++;
            return Scoring.Evaluate(evalType, decryptedText, crib);
        }
    }


}
