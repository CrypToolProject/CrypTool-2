/*
   Copyright 2018 Dominik Vogt <ct2contact@CrypTool.org>

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
using common;
using CrypTool.CrypAnalysisViewControl;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ADFGVXAnalyzer
{
    [Author("Dominik Vogt", "dvogt@posteo.de", null, null)]
    [PluginInfo("ADFGVXAnalyzer.Properties.Resources", "ADFGVXAnalyzerCaption", "ADFGVXAnalyzerToolTip", "ADFGVXAnalyzer/userdoc.xml", new[] { "ADFGVXAnalyzer/icon.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class ADFGVXAnalyzer : ICrypComponent
    {
        #region Private Variables

        private readonly ADFGVXANalyzerSettings settings;
        private readonly ADFGVXAnalyzerPresentation myPresentation;
        private readonly Logger log;

        private const int MaxBestListEntries = 10;
        private DateTime startTime;
        private DateTime endTime;
        private string[] messages;
        private string separator;
        private int keylength = 0;
        private int threads = 0;
        private List<Thread> ThreadList;
        private int keysPerSecond;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public ADFGVXAnalyzer()
        {
            settings = new ADFGVXANalyzerSettings();
            myPresentation = new ADFGVXAnalyzerPresentation();
            myPresentation.getTranspositionResult += getTranspositionResult;
            Presentation = myPresentation;
            log = new Logger();

        }

        #region Data Properties

        [PropertyInfo(Direction.InputData, "MessagesCaption", "MessagesToolTip")]
        public string Messages
        {
            get;
            set;
        }

        private string transpositionResult;
        [PropertyInfo(Direction.OutputData, "TranspositionResultCaption", "TranspositionResultToolTip")]
        public string TranspositionResult
        {
            get => transpositionResult;
            set
            {
                transpositionResult = value;
                OnPropertyChanged("TranspositionResult");
            }
        }

        private string logText;
        [PropertyInfo(Direction.OutputData, "LogTextCaption", "LogTextToolTip")]
        public string LogText
        {
            get => logText;
            set
            {
                logText = value;
                OnPropertyChanged("LogText");
            }
        }

        private string transpositionkey;
        [PropertyInfo(Direction.OutputData, "TranspositionkeyCaption", "TranspositionkeyToolTip")]
        public string Transpositionkey
        {
            get => transpositionkey;
            set
            {
                transpositionkey = value;
                OnPropertyChanged("Transpositionkey");
            }
        }

        #endregion


        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get;
            private set;
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            try
            {
                startTime = new DateTime();
                endTime = new DateTime();
                keylength = settings.KeyLength;
                threads = settings.CoresUsed + 1;
                ThreadList = new List<Thread>();
                LogText = "";
            }
            catch (Exception ex)
            {
                GuiLogMessage("PreExecution: " + ex.Message, NotificationLevel.Error);
            }

        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            try
            {
                if (!CheckAlphabetLength()) { return; }
                separator = ChooseSeparator(settings.Separator);
                messages = Messages.Split(new[] { separator }, StringSplitOptions.None);

                if (!CheckMessages()) { return; }
            }
            catch (Exception ex)
            {
                GuiLogMessage("ExecuteChecks: " + ex.Message, NotificationLevel.Error);
            }

            ClearDisplay();

            UpdateDisplayStart();

            try
            {
                ThreadingHelper threadingHelper = new ThreadingHelper(threads, this);
                for (int j = 1; j <= threads; j++)
                {
                    Thread thread = new Thread(AlgorithmThread);
                    ThreadList.Add(thread);
                    thread.IsBackground = true;
                    thread.Start(new object[] { keylength, messages, j, threadingHelper, settings });
                    LogText += Environment.NewLine + "Starting Thread: " + j;
                }
                foreach (Thread t in ThreadList)
                {
                    t.Join();
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage("Execute: " + ex.Message, NotificationLevel.Error);
            }
        }


        private void AlgorithmThread(object parametersObject)
        {
            try
            {
                object[] parameters = (object[])parametersObject;
                int i = (int)parameters[0];
                string[] messages = (string[])parameters[1];
                int j = (int)parameters[2];
                ThreadingHelper threadingHelper = (ThreadingHelper)parameters[3];
                ADFGVXANalyzerSettings settings = (ADFGVXANalyzerSettings)parameters[4];
                Algorithm a = new Algorithm(i, messages, log, j, threadingHelper, settings, this);
                a.SANgramsIC();
            }
            catch (ThreadAbortException)
            {
                //do nothing; analysis aborts threads to stop them :-/
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format(Properties.Resources.ExceptionDuringThread, ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Adds an entry to the BestList
        /// </summary>
        /// <param name="score"></param>
        /// <param name="IoC1"></param>
        /// <param name="IoC2"></param>
        /// <param name="transkey"></param>
        /// <param name="transpositionResult"></param>
        public void AddNewBestListEntry(double score, double IoC1, double IoC2, string transkey, string transpositionResult)
        {
            try
            {
                ResultEntry entry = new ResultEntry
                {
                    Score = score,
                    Ic1 = IoC1,
                    Ic2 = IoC2,
                    TransKey = transkey,
                    TranspositionResult = transpositionResult
                };

                if (((ADFGVXAnalyzerPresentation)Presentation).BestList.Count == 0)
                {

                }
                else if (entry.Score > ((ADFGVXAnalyzerPresentation)Presentation).BestList.First().Score)
                {

                }

                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        if (((ADFGVXAnalyzerPresentation)Presentation).BestList.Count > 0 && entry.Score <= ((ADFGVXAnalyzerPresentation)Presentation).BestList.Last().Score)
                        {
                            return;
                        }

                        //Insert new entry at correct place to sustain order of list:
                        int insertIndex = myPresentation.BestList.TakeWhile(e => e.Score > entry.Score).Count();
                        myPresentation.BestList.Insert(insertIndex, entry);

                        if (((ADFGVXAnalyzerPresentation)Presentation).BestList.Count > MaxBestListEntries)
                        {
                            ((ADFGVXAnalyzerPresentation)Presentation).BestList.RemoveAt(MaxBestListEntries);
                        }
                        int ranking = 1;
                        foreach (ResultEntry e in ((ADFGVXAnalyzerPresentation)Presentation).BestList)
                        {
                            e.Ranking = ranking;
                            ranking++;
                        }
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch (Exception)
                    {
                        //do nothing
                    }
                }, null);
            }
            catch (Exception ex)
            {
                GuiLogMessage("AddNewBestListEntry: " + ex.Message, NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Clear the UI
        /// </summary>
        private void ClearDisplay()
        {
            try
            {
                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        ((ADFGVXAnalyzerPresentation)Presentation).StartTime.Value = "";
                        ((ADFGVXAnalyzerPresentation)Presentation).EndTime.Value = "";
                        ((ADFGVXAnalyzerPresentation)Presentation).ElapsedTime.Value = "";
                        ((ADFGVXAnalyzerPresentation)Presentation).CurrentAnalysedKeylength.Value = "";
                        ((ADFGVXAnalyzerPresentation)Presentation).Keys.Value = "";
                        ((ADFGVXAnalyzerPresentation)Presentation).MessageCount.Value = "";
                        ((ADFGVXAnalyzerPresentation)Presentation).BestList.Clear();
                    }
                    catch (Exception)
                    {
                        //do nothing
                    }
                }, null);
            }
            catch (Exception ex)
            {
                GuiLogMessage("ClearDisplay: " + ex.Message, NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Set start time in UI
        /// </summary>
        private void UpdateDisplayStart()
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    startTime = DateTime.Now;
                    ((ADFGVXAnalyzerPresentation)Presentation).StartTime.Value = "" + startTime;
                    ((ADFGVXAnalyzerPresentation)Presentation).EndTime.Value = "";
                    ((ADFGVXAnalyzerPresentation)Presentation).ElapsedTime.Value = "";
                }
                catch (Exception)
                {
                    //do nothing
                }
            }, null);
        }

        /// <summary>
        /// Set end time in UI
        /// </summary>
        public void UpdateDisplayEnd(int keylength, long decryptions, long alldecyptions)
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    endTime = DateTime.Now;
                    TimeSpan elapsedtime = endTime.Subtract(startTime);
                    double totalSeconds = elapsedtime.TotalSeconds;
                    keysPerSecond = (int)(decryptions / totalSeconds);
                    TimeSpan elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);
                    ((ADFGVXAnalyzerPresentation)Presentation).EndTime.Value = "" + endTime;
                    ((ADFGVXAnalyzerPresentation)Presentation).ElapsedTime.Value = "" + elapsedspan;
                    ((ADFGVXAnalyzerPresentation)Presentation).CurrentAnalysedKeylength.Value = "" + keylength;
                    ((ADFGVXAnalyzerPresentation)Presentation).Keys.Value = "" + keysPerSecond + " (" + decryptions + ")";
                    ((ADFGVXAnalyzerPresentation)Presentation).MessageCount.Value = "" + messages.Length;
                }
                catch (Exception)
                {
                    //do nothing
                }
            }, null);

        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            foreach (Thread t in ThreadList)
            {
                t.Abort();
            }
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion

        #region Helper Methods

        private bool CheckAlphabetLength()
        {
            try
            {
                if (!(settings.Alphabet.Length == Math.Pow(settings.EncryptAlphabet.Length, 2)))
                {
                    GuiLogMessage("Plaintext and ciphertext length do not match", NotificationLevel.Error);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                GuiLogMessage("CheckAlphabetLength: " + ex.Message, NotificationLevel.Error);
                return false;
            }
        }

        private bool CheckMessages()
        {
            try
            {
                foreach (string message in messages)
                {
                    foreach (char c in message)
                    {
                        if (settings.EncryptAlphabet.IndexOf(c) == -1) //if c not even present in the string, this will output value -1
                        {
                            GuiLogMessage(string.Format(Properties.Resources.InvalidCharacters, message), NotificationLevel.Error);
                            return false;
                        }
                    }
                    if (message.Length % 2 != 0)
                    {
                        GuiLogMessage(string.Format(Properties.Resources.NotEvenLength, message), NotificationLevel.Error);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format(Properties.Resources.ExceptionDuringCheckOfMessages, ex.Message), NotificationLevel.Error);
                return false;
            }
        }

        private string ChooseSeparator(int separator)
        {
            switch (separator)
            {
                case 0:
                    return Environment.NewLine;
                case 1:
                    return ",";
                case 2:
                    return ";";
                case 3:
                    return " ";
                default:
                    return null;
            }
        }

        //Method to send a transactionhash by doubleclick
        private void getTranspositionResult(ResultEntry resultEntry)
        {
            try
            {
                TranspositionResult = resultEntry.TranspositionResult;
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        #endregion       

    }

    #region Helper Classes

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

        public double Score { get; set; }
        public double Ic1 { get; set; }
        public double Ic2 { get; set; }
        public string TransKey { get; set; }
        public string TranspositionResult { get; set; }

        public string ClipboardValue => $"Score: {Score}\tIc1: {Ic1}\tIc2: {Ic2}";
        public string ClipboardKey => TransKey;
        public string ClipboardText => TranspositionResult;
        public string ClipboardEntry =>
            "Ranking: " + Ranking + Environment.NewLine +
            "Score: " + Score + Environment.NewLine +
            "Ic1: " + Ic1 + Environment.NewLine +
            "Ic2: " + Ic2 + Environment.NewLine +
            "TransKey: " + TransKey + Environment.NewLine +
            "Plaintext: " + TranspositionResult;
    }

    #endregion
}


