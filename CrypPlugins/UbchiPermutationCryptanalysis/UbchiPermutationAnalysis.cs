using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.Plugins.UbchiPermutationCryptanalysis.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.UbchiPermutationCryptanalysis
{
    [Author("Adrián Lutišan", "adrian.lutisan@gmail.com",
        "Institute of Computer Science and Mathematics, FEI STU", "https://uim.fei.stuba.sk")]
    [PluginInfo("CrypTool.Plugins.UbchiPermutationCryptanalysis.Properties.Resources",
        "PluginCaption", "PluginTooltip",
        "UbchiPermutationCryptanalysis/userdoc.xml",
        new[] { "UbchiPermutationCryptanalysis/Images/ubchi_crib.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class UbchiPermutationAnalysis : ICrypComponent
    {
        private readonly UbchiPermutationAnalysisSettings _settings = new UbchiPermutationAnalysisSettings();
        private UbchiPermutationAnalysisPresentation _presentation;
        private bool _stopped;

        [PropertyInfo(Direction.InputData, "InputCiphertext_Name", "InputCiphertext_Description")]
        public string InputCiphertext { get; set; }

        [PropertyInfo(Direction.InputData, "InputCrib_Name", "InputCrib_Description")]
        public string InputCrib { get; set; }

        [PropertyInfo(Direction.OutputData, "OutputBestPlaintext_Name", "OutputBestPlaintext_Description")]
        public string OutputBestPlaintext { get; set; }

        [PropertyInfo(Direction.OutputData, "OutputBestKey_Name", "OutputBestKey_Description")]
        public string OutputBestKey { get; set; }

        public ISettings Settings
        {
            get { return _settings; }
        }

        public UserControl Presentation
        {
            get
            {
                if (_presentation == null)
                {
                    _presentation = new UbchiPermutationAnalysisPresentation();
                    _presentation.SelectedResultEntry += OnSelectedResult;
                }
                return _presentation;
            }
        }

        public void PreExecution() { }
        public void PostExecution() { }

        public void Stop()
        {
            _stopped = true;
        }

        public void Initialize() { }
        public void Dispose() { }

        public void Execute()
        {
            _stopped = false;
            ProgressChanged(0, 100);

            if (string.IsNullOrEmpty(InputCiphertext))
            {
                GuiLogMessage(Resources.Log_NoCiphertext, NotificationLevel.Error);
                return;
            }

            string ciphertext = UbchiCore.PrepareText(InputCiphertext);

            if (string.IsNullOrWhiteSpace(InputCrib))
            {
                GuiLogMessage("Crib is required. Please enter a known plaintext prefix.", NotificationLevel.Error);
                return;
            }

            string crib = UbchiCore.PrepareText(InputCrib);

            if (string.IsNullOrEmpty(crib))
            {
                GuiLogMessage("Crib is required. Please enter a known plaintext prefix (letters A-Z only).", NotificationLevel.Error);
                return;
            }

            if (ciphertext.Length < 4)
            {
                GuiLogMessage(Resources.Log_CiphertextTooShort, NotificationLevel.Error);
                return;
            }

            // Clear presentation
            if (_presentation != null)
            {
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
                {
                    _presentation.Entries.Clear();
                });
            }

            GuiLogMessage(Resources.Log_Starting, NotificationLevel.Info);

            Stopwatch sw = Stopwatch.StartNew();
            List<UbchiPermutationResult> allResults = new List<UbchiPermutationResult>();
            long totalTested = 0;

            int minK = _settings.MinKeyLength;
            int maxK = _settings.MaxKeyLength;
            int minN = _settings.MinNulls;
            int maxN = _settings.MaxNulls;
            int totalCombinations = (maxK - minK + 1) * (maxN - minN + 1);
            int completedCombinations = 0;

            UpdateHeaderStatus(Resources.Status_Running);

            for (int nulls = minN; nulls <= maxN && !_stopped; nulls++)
            {
                for (int keyLen = minK; keyLen <= maxK && !_stopped; keyLen++)
                {
                    if (ciphertext.Length < keyLen * 2)
                    {
                        completedCombinations++;
                        continue;
                    }

                    UpdateProgressLabels(keyLen, nulls, completedCombinations, totalCombinations);

                    DateTime deadline = DateTime.UtcNow.AddSeconds(_settings.TimeoutSeconds);

                    List<ScoredCandidate> candidates = AllLetterAnagramming.Analyze(
                        ciphertext, crib, keyLen, nulls,
                        _settings.MaxCandidates,
                        () => _stopped,
                        (count) =>
                        {
                            totalTested = count;
                            UpdateHeaderKeysTested(totalTested);
                        },
                        deadline);

                    foreach (ScoredCandidate candidate in candidates)
                    {
                        string permStr = string.Join(",", candidate.Permutation);
                        string preview = candidate.Plaintext.Length > 60
                            ? candidate.Plaintext.Substring(0, 60) + "..."
                            : candidate.Plaintext;

                        string info = "";
                        if (candidate.CribMatched)
                        {
                            info = "CRIB MATCH";
                        }
                        info += " Freq:" + candidate.FrequencyScore.ToString("F2");

                        UbchiPermutationResult result = new UbchiPermutationResult
                        {
                            Score = candidate.Score,
                            KeyLength = keyLen,
                            Nulls = nulls,
                            KeyPermutation = permStr,
                            PlaintextPreview = preview,
                            FullPlaintext = candidate.Plaintext,
                            AnalysisInfo = info.Trim()
                        };

                        allResults.Add(result);
                    }

                    completedCombinations++;
                    double progress = (double)completedCombinations / totalCombinations * 100.0;
                    ProgressChanged(progress, 100);
                }
            }

            sw.Stop();

            // Sort all results by score and assign ranks
            allResults.Sort((a, b) => b.Score.CompareTo(a.Score));
            if (allResults.Count > _settings.MaxCandidates)
            {
                allResults.RemoveRange(_settings.MaxCandidates, allResults.Count - _settings.MaxCandidates);
            }

            for (int i = 0; i < allResults.Count; i++)
            {
                allResults[i].Rank = i + 1;
            }

            // Update presentation
            if (_presentation != null)
            {
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
                {
                    _presentation.Entries.Clear();
                    foreach (UbchiPermutationResult r in allResults)
                    {
                        _presentation.Entries.Add(r);
                    }
                });
            }

            // Set output from best result
            if (allResults.Count > 0)
            {
                OutputBestPlaintext = allResults[0].FullPlaintext;
                OutputBestKey = allResults[0].KeyPermutation;
                OnPropertyChanged("OutputBestPlaintext");
                OnPropertyChanged("OutputBestKey");

                UpdateHeaderBestScore(allResults[0].Score);
                GuiLogMessage(string.Format(Resources.Log_Completed, allResults[0].Score), NotificationLevel.Info);
            }

            UpdateHeaderStatus(_stopped ? Resources.Status_Stopped : Resources.Status_Finished);
            UpdateHeaderElapsedTime(sw.Elapsed);
            ProgressChanged(100, 100);
        }

        private void OnSelectedResult(UbchiPermutationResult result)
        {
            OutputBestPlaintext = result.FullPlaintext;
            OutputBestKey = result.KeyPermutation;
            OnPropertyChanged("OutputBestPlaintext");
            OnPropertyChanged("OutputBestKey");
        }

        private void UpdateHeaderStatus(string status)
        {
            if (_presentation == null) return;
            try
            {
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
                {
                    _presentation.HeaderStatus.Value = status;
                });
            }
            catch { }
        }

        private void UpdateHeaderBestScore(double score)
        {
            if (_presentation == null) return;
            try
            {
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
                {
                    _presentation.HeaderBestScore.Value = score.ToString("F2");
                });
            }
            catch { }
        }

        private void UpdateHeaderElapsedTime(TimeSpan elapsed)
        {
            if (_presentation == null) return;
            try
            {
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
                {
                    _presentation.HeaderElapsedTime.Value = elapsed.ToString(@"hh\:mm\:ss\.fff");
                });
            }
            catch { }
        }

        private void UpdateHeaderKeysTested(long count)
        {
            if (_presentation == null) return;
            try
            {
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
                {
                    _presentation.HeaderKeysTested.Value = count.ToString("N0");
                });
            }
            catch { }
        }

        private void UpdateProgressLabels(int keyLen, int nulls, int completed, int total)
        {
            if (_presentation == null) return;
            try
            {
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
                {
                    _presentation.ProgressCurrentKeyLen.Value = keyLen.ToString();
                    _presentation.ProgressCurrentNulls.Value = nulls.ToString();
                    _presentation.ProgressCompleted.Value = completed.ToString();
                    _presentation.ProgressTotal.Value = total.ToString();
                });
            }
            catch { }
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel level)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this,
                new GuiLogEventArgs(message, this, level));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this,
                new PluginProgressEventArgs(value, max));
        }
    }
}

