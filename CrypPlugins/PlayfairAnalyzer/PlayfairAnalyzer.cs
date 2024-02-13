/*
   Copyright 2022 George Lasry, Nils Kopal, CrypTool 2 Team

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
using CrypTool.CrypAnalysisViewControl;
using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.PluginBase.Utils;
using LanguageStatisticsLib;
using PlayfairAnalysis;
using PlayfairAnalysis.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.PlayfairAnalyzer
{
    public delegate void PluginProgress(double current, double maximum);
    public delegate void UpdateOutput(string keyString, string plaintextString);

    [Author("George Lasry, Nils Kopal", "George.Lasry@CrypTool.org", "Uni Kassel", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.PlayfairAnalyzer.Properties.Resources", "PluginCaption", "PluginTooltip", "PlayfairAnalyzer/DetailedDescription/doc.xml", "PlayfairAnalyzer/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class PlayfairAnalyzer : ICrypComponent
    {
        public const long fileSizeResource11bin = 617831552; //this is the file size of "Resource-1-1.bin", which is downloaded from CrypToolStore

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _Running = false;

        private string _Ciphertext = null;
        private string _Crib = null;
        private string _Plaintext = null;
        private string _Key = null;

        private const string PLAYFAIR_ALPHABET = "ABCDEFGHIKLMNOPQRSTUVWXYZ";

        private readonly PlayfairAnalyzerSettings _settings = new PlayfairAnalyzerSettings();
        private readonly AssignmentPresentation _presentation;
        private DateTime _startTime;
        private bool _alreadyExecuted = false;
        private AnalysisInstance _analysisInstance;

        [PropertyInfo(Direction.InputData, "CiphertextCaption", "CiphertextTooltip", true)]
        public string Ciphertext
        {
            get => _Ciphertext;
            set
            {
                if (_Ciphertext != value)
                {
                    _Ciphertext = value;
                    if (!string.IsNullOrEmpty(_Ciphertext))
                    {
                        _Ciphertext = RemoveInvalidSymbols(_Ciphertext);
                    }
                }
            }
        }

        /// <summary>
        /// Removes all invalid symbols, which are not part of the PLAYFAIR_ALPHABET
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string RemoveInvalidSymbols(string value)
        {
            value = value.ToUpper();
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                if (PLAYFAIR_ALPHABET.Contains(value.Substring(i, 1)))
                {
                    builder.Append(value.Substring(i, 1));
                }
            }
            return builder.ToString();
        }

        [PropertyInfo(Direction.InputData, "CribCaption", "CribTooltip", false)]
        public string Crib
        {
            get => _Crib;
            set
            {
                if (_Crib != value)
                {
                    _Crib = value;
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "PlaintextCaption", "PlaintextTooltip", false)]
        public string Plaintext
        {
            get => _Plaintext;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _Plaintext = value;
                    OnPropertyChanged("Plaintext");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "KeyCaption", "KeyTooltip", false)]
        public string Key
        {
            get => _Key;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _Key = value;
                    OnPropertyChanged("Key");
                }
            }
        }

        public PlayfairAnalyzer()
        {
            _presentation = new AssignmentPresentation(_settings);
        }

        public void PreExecution()
        {
            //reset inputs, outputs, and sending queue
            _Plaintext = string.Empty;
            _alreadyExecuted = false;
        }

        public void PostExecution()
        {

        }

        public ISettings Settings => _settings;

        public UserControl Presentation => _presentation;

        public void Execute()
        {
            // some checks:

            if (_alreadyExecuted)
            {
                GuiLogMessage(Properties.Resources.AlreadyExecuted, NotificationLevel.Error);
                return;
            }

            if (!string.IsNullOrEmpty(_Crib))
            {
                if(_settings.MaxCribOffset + _Crib.Length > Ciphertext.Length)
                {
                    GuiLogMessage(string.Format("Crib cannot be placed at the given max offset (={0}) since the crib is too long (={1})", _settings.MaxCribOffset, Crib.Length), NotificationLevel.Error);
                    return;
                }
            }

            _alreadyExecuted = true;           

            try
            {
                //Step 0: Set running true :-)
                _Running = true;

                //set start time in presentation; remove elapsedTime and endTime
                _startTime = DateTime.Now;
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        _presentation.StartTime.Value = _startTime.ToString();
                        _presentation.EndTime.Value = string.Empty;
                        _presentation.ElapsedTime.Value = string.Empty;
                        _presentation.EvaluatedKeys.Value = string.Empty;
                        _presentation.MaxKeys.Value = string.Empty;
                        _presentation.KeySpaceSize.Value = string.Empty;
                        _presentation.BestList.Clear();
                    }
                    catch (Exception)
                    {
                        //wtf?
                    }
                }, null);

                _analysisInstance?.Cancel();

                //Step 1: Start analysis                
                RunAnalysis();
            }
            finally
            {
                _Running = false;

                //set end time in presentation
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        _presentation.EndTime.Value = DateTime.Now.ToString();
                        TimeSpan elapsedTime = (DateTime.Now - _startTime);
                        _presentation.ElapsedTime.Value = new TimeSpan(elapsedTime.Hours, elapsedTime.Minutes, elapsedTime.Seconds).ToString();
                    }
                    catch (Exception)
                    {
                        //wtf?
                    }
                }, null);

            }
        }

        private void RunAnalysis()
        {
            Grams grams = LanguageStatistics.CreateGrams(_settings.Language, DirectoryHelper.DirectoryLanguageStatistics, GramsType.Pentragrams, false);
            grams.Normalize(10_000_000); //normalize cost function for simulated annealing
            AnalysisInstance analysisInstance = new AnalysisInstance(_settings.DiscardSamePlaintexts, grams);
            _analysisInstance = analysisInstance;
            analysisInstance.CtAPI.BestListChangedEvent += HandleIncomingBestList;
            analysisInstance.CtAPI.BestResultChangedEvent += HandleBestResultChangedEvent;
            analysisInstance.CtAPI.ProgressChangedEvent += OnProgressChanged;

            try
            {
                CancellationToken ct = analysisInstance.CancellationToken;
                int[] cipherText = Utils.GetText(Ciphertext);
                if (cipherText.Length < 6)
                {
                    throw new Exception($"Ciphertext length must have at least six characters - found {cipherText.Length} characters.");
                }
                if (cipherText.Length % 2 != 0)
                {
                    throw new Exception($"Ciphertext length must be even - found {cipherText.Length} characters.");
                }

                int threads = (_settings.Threads + 1);  //index starts at 0; thus +1
                long maxKeys = SolvePlayfair.SolveMultithreaded(cipherText, Crib ?? string.Empty, _settings.MinCribOffset, _settings.MaxCribOffset, threads, _settings.Cycles, null, analysisInstance); ;
                GuiLogMessage("Starting analysis now.", NotificationLevel.Debug);
                BigInteger keySpace = GetKeyspaceSize();
                GuiLogMessage($"Size of key space is {keySpace:N0}.", NotificationLevel.Debug);

                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    _presentation.MaxKeys.Value = $"{maxKeys:N0}";
                    //Show key space with exponential formatting:
                    string scientificKeySpace = $"{GetKeyspaceSize():0.##E-0}";
                    _presentation.KeySpaceSize.Value = string.Join("*10^", scientificKeySpace.Split('E'));
                }, null);

                DateTime lastUpdateTime = DateTime.Now;

                //Wait for completion/cancellation and update display:
                while (!ct.IsCancellationRequested)
                {
                    Thread.Sleep(100);
                    if (!_Running)
                    {
                        return;
                    }
                    if (DateTime.Now >= lastUpdateTime.AddSeconds(1))
                    {
                        lastUpdateTime = DateTime.Now;
                        //set elapsed time in presentation
                        _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            try
                            {
                                TimeSpan elapsedTime = (DateTime.Now - _startTime);
                                _presentation.ElapsedTime.Value = new TimeSpan(elapsedTime.Hours, elapsedTime.Minutes, elapsedTime.Seconds).ToString();
                            }
                            catch (Exception)
                            {
                                //wtf?
                            }
                        }, null);
                    }
                }
            }
            finally
            {
                analysisInstance.Cancel();
                analysisInstance.CtAPI.BestListChangedEvent -= HandleIncomingBestList;
                analysisInstance.CtAPI.BestResultChangedEvent -= HandleBestResultChangedEvent;
                analysisInstance.CtAPI.ProgressChangedEvent -= OnProgressChanged;
            }
        }

        /// <summary>
        /// Calculates the key space of playfair cipher.
        /// </summary>
        private BigInteger GetKeyspaceSize()
        {
            int dim = Playfair.DIM; //Note: Playfair analysis component only has fixed dimension right now.
            BigInteger squareSize = new BigInteger(dim * dim);
            return squareSize.Factorial();
        }

        /// <summary>
        /// Handles change event for a new best result.
        /// </summary>
        private void HandleBestResultChangedEvent(CtBestList.Result bestResult)
        {
            UpdateOutput(bestResult.keyString, bestResult.plaintextString);
        }

        /// <summary>
        /// Handles incoming best list entries
        /// </summary>
        /// <param name="value"></param>
        private void HandleIncomingBestList(IList<CtBestList.Result> bestList)
        {
            try
            {
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        _presentation.BestList.Clear();
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage(string.Format("Error occured while clearing best list: {0}", ex.Message), NotificationLevel.Error);
                    }

                    int ranking = 1;
                    foreach (CtBestList.Result result in bestList)
                    {
                        ResultEntry resultEntry = new ResultEntry
                        {
                            Ranking = ranking++,
                            Value = $"{result.score:N0}",
                            Key = result.keyString,
                            Text = result.plaintextString,
                            Elapsed = result.elapsed,
                            Evaluations = result.evaluations,
                            Info = result.commentString
                        };

                        try
                        {
                            _presentation.BestList.Add(resultEntry);
                        }
                        catch (Exception ex)
                        {
                            GuiLogMessage(string.Format("Error occured while adding new entry to best list: {0}", ex.Message), NotificationLevel.Error);
                        }
                    }
                }, null);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Error occured while handling new best list: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        public void Stop()
        {
            _Running = false;
            _analysisInstance?.Cancel();
        }

        public void Initialize()
        {
            _presentation.UpdateOutputFromUserChoice += UpdateOutput;
        }

        public void Dispose()
        {

        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void OnProgressChanged(double value, double max, long evaluations)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
            _presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _presentation.EvaluatedKeys.Value = $"{evaluations:N0}";
            }, null);
        }

        private void UpdateOutput(string keyString, string plaintextString)
        {
            Plaintext = plaintextString;
            Key = keyString;
        }
    }

    public class ResultEntry : ICrypAnalysisResultListEntry, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int ranking;
        public int Ranking
        {
            get => ranking;
            set
            {
                ranking = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Ranking)));
            }
        }

        public string Value { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }
        public string Info { get; set; }
        public TimeSpan Elapsed { get; set; }
        public long Evaluations { get; set; }
        public long EvaluationsDiv1000 => Evaluations / 1000;
        public double Performance => Evaluations / Elapsed.TotalMilliseconds;

        public string ClipboardValue => Value.ToString();
        public string ClipboardKey => Key;
        public string ClipboardText => Text;
        public string ClipboardEntry =>
            "Rank: " + Ranking + Environment.NewLine +
            "Value: " + Value + Environment.NewLine +
            "Key: " + Key + Environment.NewLine +
            "Text: " + Text + Environment.NewLine +
            $"Elapsed: {Elapsed.TotalSeconds:N0} sec" + Environment.NewLine +
            $"Evaluations: {EvaluationsDiv1000:N0} K decryptions" + Environment.NewLine +
            $"Performance: {Performance:N0} K/sec";
    }
}
