/*
   Copyright 2019 Christian Bender christian1.bender@student.uni-siegen.de

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

using CrypTool.PluginBase;
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using DCAKeyRecovery;
using DCAKeyRecovery.Logic;
using DCAKeyRecovery.Logic.Cipher1;
using DCAKeyRecovery.Logic.Cipher2;
using DCAKeyRecovery.Logic.Cipher3;
using DCAKeyRecovery.Properties;
using DCAKeyRecovery.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CrypTool.Plugins.DCAKeyRecovery
{
    [Author("Christian Bender", "christian1.bender@student.uni-siegen.de", null, "http://www.uni-siegen.de")]
    [PluginInfo("DCAKeyRecovery.Properties.Resources", "PluginCaption", "PluginTooltip", "DCAKeyRecovery/userdoc.xml", new[] { "DCAKeyRecovery/Images/IC_KeyRecovery.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    [AutoAssumeFullEndProgress(false)]
    public class DCAKeyRecovery : ICrypComponent
    {
        #region Private Variables

        private readonly DCAKeyRecoverySettings _settings = new DCAKeyRecoverySettings();
        private readonly KeyRecoveryPres _pres = new KeyRecoveryPres();
        private string _differential;
        private int _messageDifference;
        private Algorithms _currentAlgorithm;
        private bool _finished;
        private ICrypToolStream _unencryptedMessagePairs;
        private ICrypToolStream _encryptedMessagePairs;
        private int _neededMessageCount;
        private Thread _workerThread;
        private readonly AutoResetEvent _nextStep;
        private bool _attackingLastRound;
        private int neededMessageCounterLastRound;

        private double _currentProgressValue = 0.0;
        private readonly double _progressMaximum = 1;

        private volatile bool _stop = false;

        private DifferentialKeyRecoveryAttack attack;
        private IKeyRecovery keyRecovery;
        private DifferentialAttackRoundConfiguration roundConfiguration;

        private bool _hasNewDifferential = false;
        private bool _hasNewPlaintextPairs = false;
        private bool _hasNewCipherTextPairs = false;

        private byte[] _roundKeys;

        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        #endregion

        public DCAKeyRecovery()
        {
            _settings.PropertyChanged += new PropertyChangedEventHandler(SettingChangedListener);
            _settings.SettingsErrorOccured += HandleSettingsError;

            _nextStep = new AutoResetEvent(false);

            //Check specific algorithm and invoke the selection into the UI class
            if (_settings.CurrentAlgorithm == Algorithms.Cipher1)
            {
                _currentAlgorithm = Algorithms.Cipher1;
                _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                    (SendOrPostCallback)delegate { _pres.TutorialNumber = Algorithms.Cipher1; }, null);
            }
            else if (_settings.CurrentAlgorithm == Algorithms.Cipher2)
            {
                _currentAlgorithm = Algorithms.Cipher2;
                _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                    (SendOrPostCallback)delegate { _pres.TutorialNumber = Algorithms.Cipher2; }, null);
            }
            else if (_settings.CurrentAlgorithm == Algorithms.Cipher3)
            {
                _currentAlgorithm = Algorithms.Cipher3;
                _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                    (SendOrPostCallback)delegate { _pres.TutorialNumber = Algorithms.Cipher3; }, null);
            }


        }

        #region Data Properties

        /// <summary>
        /// Input for the differential
        /// </summary>
        [PropertyInfo(Direction.InputData, "DifferentialInput", "DifferentialInputToolTip", true)]
        public string Differential
        {
            get => _differential;
            set
            {
                _differential = value;
                _hasNewDifferential = true;
                OnPropertyChanged("Differential");
            }
        }

        /// <summary>
        /// input of the plaintext message pairs
        /// </summary>
        [PropertyInfo(Direction.InputData, "UnencryptedMessagePairsInput", "UnencryptedMessagePairsInputToolTip", true)]
        public ICrypToolStream UnencryptedMessagePairs
        {
            get => _unencryptedMessagePairs;
            set
            {
                _unencryptedMessagePairs = value;
                _hasNewPlaintextPairs = true;
                OnPropertyChanged("UnencryptedMessagePairs");
            }
        }

        /// <summary>
        /// Input if the encrypted message pairs
        /// </summary>
        [PropertyInfo(Direction.InputData, "EncryptedMessagePairsInput", "EncryptedMessagePairsInputToolTip", true)]
        public ICrypToolStream EncryptedMessagePairs
        {
            get => _encryptedMessagePairs;
            set
            {
                _encryptedMessagePairs = value;
                _hasNewCipherTextPairs = true;
                OnPropertyChanged("EncryptedMessagePairs");
            }
        }

        /// <summary>
        /// Output for the round keys
        /// </summary>
        [PropertyInfo(Direction.OutputData, "RoundKeysOutput", "RoundKeysOutputToolTip")]
        public byte[] RoundKeys
        {
            get => _roundKeys;
            set
            {
                _roundKeys = value;
                OnPropertyChanged("RoundKeys");
            }
        }

        /// <summary>
        /// Output for the needed message count
        /// </summary>
        [PropertyInfo(Direction.OutputData, "NeededMessageCountOutput", "NeededMessageCountOutputToolTip")]
        public int NeededMessageCount
        {
            get => _neededMessageCount;
            set
            {
                _neededMessageCount = value;
                OnPropertyChanged("NeededMessageCount");
            }
        }

        /// <summary>
        /// Output for the needed message difference
        /// </summary>
        [PropertyInfo(Direction.OutputData, "MessageDifferenceOutput", "MessageDifferenceOutputToolTip")]
        public int MessageDifference
        {
            get => _messageDifference;
            set
            {
                _messageDifference = value;
                OnPropertyChanged("MessageDifference");
            }
        }


        /// <summary>
        /// Output for the finished flag
        /// </summary>
        [PropertyInfo(Direction.OutputData, "FinishedOutput", "FinishedOutputToolTip")]
        public bool Finished
        {
            get => _finished;
            set
            {
                _finished = value;
                OnPropertyChanged("Finished");
            }
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => _settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => _pres;

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            _nextStep.Reset();
            _attackingLastRound = false;
            _hasNewPlaintextPairs = false;
            _hasNewCipherTextPairs = false;
            roundConfiguration = null;
            _stop = false;
            neededMessageCounterLastRound = 0;

            //prepare UI to show round results
            _pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
           {
               _pres.TutorialNumber = _settings.CurrentAlgorithm;
               _pres.StartClickEvent.Reset();
               _pres.ResetUI();
               _pres.IsNextStepPanelVisible = Visibility.Hidden;
           }, null);

            //IKeyRecovery: Methods for differential attack
            //DifferentialKeyRecoveryAttack: Attributes to manage the state of the attack

            switch (_currentAlgorithm)
            {
                case Algorithms.Cipher1:
                    keyRecovery = new Cipher1KeyRecovery();
                    attack = new Cipher1DifferentialKeyRecoveryAttack();

                    //subscribe events
                    keyRecovery.NeedMessagePairOccured += GenerateNewMessagePair;
                    keyRecovery.LastRoundResultViewRefreshOccured += RefreshLastRoundResultViewData;
                    keyRecovery.ResultViewRefreshRoundFinishedOccured += AddResultViewRefreshToResultViewRefreshView;
                    keyRecovery.ProgressChangedOccured += RefreshProgress;
                    break;
                case Algorithms.Cipher2:
                    keyRecovery = new Cipher2KeyRecovery();
                    attack = new Cipher2DifferentialKeyRecoveryAttack();

                    //subscribe events
                    keyRecovery.NeedMessagePairOccured += GenerateNewMessagePair;
                    keyRecovery.LastRoundResultViewRefreshOccured += RefreshLastRoundResultViewData;
                    keyRecovery.ResultViewRefreshRoundFinishedOccured += AddResultViewRefreshToResultViewRefreshView;
                    keyRecovery.AnyRoundResultViewRefreshOccured += RefreshAnyRoundResultViewData;
                    keyRecovery.AnyRoundResultViewKeyResultsRefreshOccured += AddKeyResultToResultView;
                    keyRecovery.ProgressChangedOccured += RefreshProgress;
                    break;
                case Algorithms.Cipher3:
                    keyRecovery = new Cipher3KeyRecovery();
                    attack = new Cipher3DifferentialKeyRecoveryAttack();

                    //subscribe events
                    keyRecovery.NeedMessagePairOccured += GenerateNewMessagePair;
                    keyRecovery.LastRoundResultViewRefreshOccured += RefreshLastRoundResultViewData;
                    keyRecovery.ResultViewRefreshRoundFinishedOccured += AddResultViewRefreshToResultViewRefreshView;
                    keyRecovery.AnyRoundResultViewRefreshOccured += RefreshAnyRoundResultViewData;
                    keyRecovery.AnyRoundResultViewKeyResultsRefreshOccured += AddKeyResultToResultView;
                    keyRecovery.ProgressChangedOccured += RefreshProgress;
                    break;
            }

            //show presentation
            _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                (SendOrPostCallback)delegate { _pres.WorkspaceRunning = true; }, null);

            //prepare thread to run
            ThreadStart tStart = new ThreadStart(ExecuteDifferentialAttack);
            _workerThread = new Thread(tStart)
            {
                Name = "DCA-KeyRecovery-WorkerThread",
                IsBackground = true
            };
        }

        /// <summary>
        /// Add key results in the UI for any round
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddKeyResultToResultView(object sender, ResultViewAnyRoundKeyResultEventArgs e)
        {
            //Invoke the call to refresh the UI
            _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                (SendOrPostCallback)delegate { _pres.AddAnyRoundKeyResult(e); }, null);
        }

        /// <summary>
        /// Refreshs the data in the UI for any round
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshAnyRoundResultViewData(object sender, ResultViewAnyRoundEventArgs e)
        {
            //Invoke the call to refresh the UI
            _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                (SendOrPostCallback)delegate { _pres.RefreshAnyRoundResultViewData(e); }, null);
        }

        /// <summary>
        /// Adds a new round result to the UI for the last round
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddResultViewRefreshToResultViewRefreshView(object sender,
            ResultViewLastRoundRoundResultEventArgs e)
        {
            //Invoke the call to add the result to the UI
            _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                (SendOrPostCallback)delegate { _pres.AddLastRoundRoundResult(e); }, null);
        }

        /// <summary>
        /// Refreshs the data in the UI for the last round
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshLastRoundResultViewData(object sender, ResultViewLastRoundEventArgs e)
        {
            //Invoke the call to add the data to the UI
            _pres.Dispatcher.Invoke(DispatcherPriority.Background,
                (SendOrPostCallback)delegate { _pres.RefreshLastRoundResultViewData(e); }, null);
        }


        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, _progressMaximum);

            if ((_workerThread.ThreadState & ThreadState.Unstarted) == ThreadState.Unstarted)
            {
                _workerThread.Start();
            }

            if (_stop)
            {
                return;
            }

            if (!_hasNewPlaintextPairs || !_hasNewCipherTextPairs)
            {
                return;
            }
            else
            {
                _hasNewDifferential = false;
                _hasNewPlaintextPairs = false;
                _hasNewCipherTextPairs = false;

                List<Pair> plainTextPairs = readPlaintextPairs();
                List<Pair> cipherTextPairs = readCipherTextPairs();

                switch (_currentAlgorithm)
                {
                    case Algorithms.Cipher1:
                        {
                            //wait for the user to click start for the first time
                            if (!_attackingLastRound)
                            {
                                if (!_settings.AutomaticMode)
                                {
                                    //set the start button to active
                                    _pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                                   {
                                       _pres.StartEnabled = true;
                                       _pres.HighlightDispatcher.Start();
                                       _pres.IsNextStepPanelVisible = Visibility.Hidden;
                                   }, null);

                                    _pres.StartClickEvent.WaitOne();
                                }

                                _nextStep.Set();
                            }

                            _attackingLastRound = true;
                            keyRecovery.AddNewPairs(plainTextPairs[0], cipherTextPairs[0]);
                        }
                        break;
                    case Algorithms.Cipher2:
                        {
                            Cipher2DifferentialKeyRecoveryAttack c2Attack = attack as Cipher2DifferentialKeyRecoveryAttack;
                            if (c2Attack.recoveredSubkey3 && c2Attack.recoveredSubkey2)
                            {
                                //wait for the user to click start for the first time
                                if (!_attackingLastRound)
                                {
                                    if (!_settings.AutomaticMode)
                                    {
                                        //set the start button to active
                                        _pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                                       {
                                           _pres.HighlightDispatcher.Start();
                                           _pres.StartEnabled = true;
                                           _pres.IsNextStepPanelVisible = Visibility.Hidden;
                                       }, null);

                                        _pres.StartClickEvent.WaitOne();
                                    }

                                    //set the start button to active
                                    /*
                                    _pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                                    {
                                        _pres.NextMessageEnabled = true;
                                        _pres.NextKeyEnabled = true;
                                        _pres.BtnStart.Content = Resources.BtnSkip;
                                    }, null);
                                    */
                                }

                                _attackingLastRound = true;
                                keyRecovery.AddNewPairs(plainTextPairs[0], cipherTextPairs[0]);
                            }
                            else
                            {
                                if (!_settings.AutomaticMode)
                                {
                                    //set the start button to active
                                    _pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                                   {
                                       _pres.HighlightDispatcher.Start();
                                       _pres.StartEnabled = true;
                                       _pres.IsNextStepPanelVisible = Visibility.Hidden;
                                   }, null);

                                    _pres.StartClickEvent.WaitOne();
                                }

                                //set the start button to active
                                /*
                                _pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                                {
                                    _pres.NextMessageEnabled = true;
                                    _pres.NextKeyEnabled = true;
                                    _pres.BtnStart.Content = Resources.BtnSkip;
                                }, null);
                                */

                                roundConfiguration = ReadConfiguration(Differential);
                                roundConfiguration.UnfilteredPairList = plainTextPairs;
                                roundConfiguration.FilteredPairList = plainTextPairs;
                                roundConfiguration.EncrypedPairList = cipherTextPairs;

                                //clear last round results
                                _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                                    (SendOrPostCallback)delegate
                                   {
                                        try
                                        {
                                            _pres.clearLastKeyResults();
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex.Message);
                                        }
                                    }, null);
                            }

                            _nextStep.Set();
                        }
                        break;
                    case Algorithms.Cipher3:
                        {
                            Cipher3DifferentialKeyRecoveryAttack c3Attack = attack as Cipher3DifferentialKeyRecoveryAttack;

                            if (c3Attack.recoveredSubkey5 && c3Attack.recoveredSubkey4 && c3Attack.recoveredSubkey3 &&
                                c3Attack.recoveredSubkey2)
                            {
                                //wait for the user to click start for the first time
                                if (!_attackingLastRound)
                                {
                                    if (!_settings.AutomaticMode)
                                    {
                                        //set the start button to active
                                        _pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                                       {
                                           _pres.HighlightDispatcher.Start();
                                           _pres.StartEnabled = true;
                                           _pres.IsNextStepPanelVisible = Visibility.Hidden;
                                       }, null);

                                        _pres.StartClickEvent.WaitOne();
                                    }

                                    //set the start button to active
                                    /*
                                    _pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                                    {
                                        _pres.NextMessageEnabled = true;
                                        _pres.NextKeyEnabled = true;
                                        _pres.BtnStart.Content = Resources.BtnSkip;
                                    }, null);
                                    */
                                }

                                _attackingLastRound = true;
                                keyRecovery.AddNewPairs(plainTextPairs[0], cipherTextPairs[0]);
                            }
                            else
                            {
                                if (!_settings.AutomaticMode)
                                {
                                    //set the start button to active
                                    _pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                                   {
                                       _pres.HighlightDispatcher.Start();
                                       _pres.StartEnabled = true;
                                       _pres.IsNextStepPanelVisible = Visibility.Hidden;
                                   }, null);

                                    _pres.StartClickEvent.WaitOne();
                                }

                                roundConfiguration = ReadConfiguration(Differential);
                                roundConfiguration.UnfilteredPairList = plainTextPairs;
                                roundConfiguration.FilteredPairList = plainTextPairs;
                                roundConfiguration.EncrypedPairList = cipherTextPairs;

                                //clear last round results
                                _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                                    (SendOrPostCallback)delegate
                                   {
                                        try
                                        {
                                            _pres.clearLastKeyResults();
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex.Message);
                                        }
                                    }, null);
                            }

                            _nextStep.Set();
                        }
                        break;
                }
            }

            //set the start button to active
            _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                (SendOrPostCallback)delegate { _pres.StartEnabled = false; }, null);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            //show presentation
            _pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
           {
               _pres.WorkspaceRunning = false;
               _pres.StartClickEvent.Reset();
               _pres.StartEnabled = false;
               _pres.HighlightDispatcher.Stop();
               _pres.BtnStart.Background = Brushes.LightGray;
               _pres.IsNextStepPanelVisible = Visibility.Hidden;
           }, null);

            _hasNewDifferential = false;
            _hasNewPlaintextPairs = false;
            _hasNewCipherTextPairs = false;
            _nextStep.Reset();
            neededMessageCounterLastRound = 0;
            _currentProgressValue = 0.0;

            switch (_currentAlgorithm)
            {
                case Algorithms.Cipher1:

                    //unsubscribe events
                    keyRecovery.NeedMessagePairOccured -= GenerateNewMessagePair;
                    keyRecovery.LastRoundResultViewRefreshOccured -= RefreshLastRoundResultViewData;
                    keyRecovery.ResultViewRefreshRoundFinishedOccured -= AddResultViewRefreshToResultViewRefreshView;
                    keyRecovery.ProgressChangedOccured -= RefreshProgress;
                    break;
                case Algorithms.Cipher2:

                    //unsubscribe events
                    keyRecovery.NeedMessagePairOccured -= GenerateNewMessagePair;
                    keyRecovery.LastRoundResultViewRefreshOccured -= RefreshLastRoundResultViewData;
                    keyRecovery.ResultViewRefreshRoundFinishedOccured -= AddResultViewRefreshToResultViewRefreshView;
                    keyRecovery.AnyRoundResultViewRefreshOccured -= RefreshAnyRoundResultViewData;
                    keyRecovery.AnyRoundResultViewKeyResultsRefreshOccured -= AddKeyResultToResultView;
                    keyRecovery.ProgressChangedOccured -= RefreshProgress;
                    break;
                case Algorithms.Cipher3:

                    //unsubscribe events
                    keyRecovery.NeedMessagePairOccured -= GenerateNewMessagePair;
                    keyRecovery.LastRoundResultViewRefreshOccured -= RefreshLastRoundResultViewData;
                    keyRecovery.ResultViewRefreshRoundFinishedOccured -= AddResultViewRefreshToResultViewRefreshView;
                    keyRecovery.AnyRoundResultViewRefreshOccured -= RefreshAnyRoundResultViewData;
                    keyRecovery.AnyRoundResultViewKeyResultsRefreshOccured -= AddKeyResultToResultView;
                    keyRecovery.ProgressChangedOccured -= RefreshProgress;
                    break;
            }
        }

        /// <summary>
        /// Refreshs the progressbar 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshProgress(object sender, ProgressEventArgs e)
        {
            _currentProgressValue += e.Increment;
            if (_currentProgressValue > 1.0)
            {
                _currentProgressValue = 1.0;
            }

            ProgressChanged(_currentProgressValue, _progressMaximum);
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            _stop = true;

            switch (_currentAlgorithm)
            {
                case Algorithms.Cipher1:
                    if (keyRecovery is Cipher1KeyRecovery)
                    {
                        Cipher1KeyRecovery c1Recovery = keyRecovery as Cipher1KeyRecovery;
                        c1Recovery.stop = true;
                        c1Recovery.DataReceivedEvent.Set();
                    }

                    break;
                case Algorithms.Cipher2:
                    if (keyRecovery is Cipher2KeyRecovery)
                    {
                        Cipher2KeyRecovery c2Recovery = keyRecovery as Cipher2KeyRecovery;
                        c2Recovery.stop = true;
                        c2Recovery.DataReceivedEvent.Set();
                        _semaphoreSlim.Wait();
                        try
                        {
                            if (c2Recovery.Cts != null)
                            {
                                c2Recovery.Cts.Cancel();
                                c2Recovery.Cts.Dispose();
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        finally
                        {
                            _semaphoreSlim.Release();
                        }
                    }

                    break;
                case Algorithms.Cipher3:
                    if (keyRecovery is Cipher3KeyRecovery)
                    {
                        Cipher3KeyRecovery c3Recovery = keyRecovery as Cipher3KeyRecovery;
                        c3Recovery.stop = true;
                        c3Recovery.DataReceivedEvent.Set();
                        _semaphoreSlim.Wait();
                        try
                        {
                            if (c3Recovery.Cts != null)
                            {
                                c3Recovery.Cts.Cancel();
                                c3Recovery.Cts.Dispose();
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        finally
                        {
                            _semaphoreSlim.Release();
                        }
                    }

                    break;
            }

            _pres.StartClickEvent.Set();
            _nextStep.Set();

            if ((_workerThread.ThreadState & ThreadState.Unstarted) != ThreadState.Unstarted)
            {
                _workerThread.Join();
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

        #region methods

        /// <summary>
        /// Handles changes within the settings class
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingChangedListener(object sender, PropertyChangedEventArgs e)
        {
            //Listen for changes of the current chosen algorithm
            if (e.PropertyName == "CurrentAlgorithm")
            {
                //Check specific algorithm and invoke the selection into the UI class
                if (_settings.CurrentAlgorithm == Algorithms.Cipher1)
                {
                    _currentAlgorithm = Algorithms.Cipher1;
                    _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                        (SendOrPostCallback)delegate { _pres.TutorialNumber = Algorithms.Cipher1; }, null);
                }
                else if (_settings.CurrentAlgorithm == Algorithms.Cipher2)
                {
                    _currentAlgorithm = Algorithms.Cipher2;
                    _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                        (SendOrPostCallback)delegate { _pres.TutorialNumber = Algorithms.Cipher2; }, null);
                }
                else if (_settings.CurrentAlgorithm == Algorithms.Cipher3)
                {
                    _currentAlgorithm = Algorithms.Cipher3;
                    _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                        (SendOrPostCallback)delegate { _pres.TutorialNumber = Algorithms.Cipher3; }, null);
                }
            }
        }

        /// <summary>
        /// Executes the main work
        /// </summary>
        private void ExecuteDifferentialAttack()
        {
            switch (_currentAlgorithm)
            {
                case Algorithms.Cipher1:
                    {
                        SummaryViewRefreshArgs summaryViewRefreshArgs;
                        SummaryLastRound lastRoundSummary;

                        lastRoundSummary = new SummaryLastRound()
                        {
                            decryptionCount = 0,
                            startTime = DateTime.Now,
                            endTime = DateTime.Now,
                            messageCount = 0,
                            recoveredSubKey0 = "",
                            recoveredSubKey1 = "",
                            testedKeys = 0
                        };

                        summaryViewRefreshArgs = new SummaryViewRefreshArgs()
                        {
                            currentRound = 1,
                            firstEvent = true,
                            lastEvent = false,
                            currentAlgorithm = Algorithms.Cipher1,
                            lastRoundSummary = lastRoundSummary,
                            anyRoundSummary = null
                        };

                        //prepare UI to show last round results
                        _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                            (SendOrPostCallback)delegate { _pres.RefreshSummaryView(summaryViewRefreshArgs); }, null);

                        Cipher1DifferentialKeyRecoveryAttack c1Attack = attack as Cipher1DifferentialKeyRecoveryAttack;

                        _nextStep.WaitOne();

                        //exit thread
                        if (_stop)
                        {
                            return;
                        }

                        //set refresh setting for UI
                        Cipher1KeyRecovery c1KeyRecovery = keyRecovery as Cipher1KeyRecovery;
                        if (c1KeyRecovery != null)
                        {
                            c1KeyRecovery.refreshUi = _settings.UIUpdateWhileExecution;
                        }

                        DifferentialAttackLastRoundResult lastRoundResult = keyRecovery.AttackFirstRound(attack);
                        if (lastRoundResult != null)
                        {
                            c1Attack.subkey0 = lastRoundResult.SubKey0;
                            c1Attack.subkey1 = lastRoundResult.SubKey1;

                            //exit thread
                            if (_stop)
                            {
                                return;
                            }

                            lastRoundSummary = new SummaryLastRound()
                            {
                                decryptionCount = lastRoundResult.DecryptionCounter,
                                startTime = DateTime.Now,
                                endTime = DateTime.Now,
                                messageCount = neededMessageCounterLastRound * 2,
                                recoveredSubKey0 = Convert.ToString(c1Attack.subkey0, 2).PadLeft(16, '0'),
                                recoveredSubKey1 = Convert.ToString(c1Attack.subkey1, 2).PadLeft(16, '0'),
                                testedKeys = lastRoundResult.KeyCounter
                            };

                            summaryViewRefreshArgs = new SummaryViewRefreshArgs()
                            {
                                currentRound = 1,
                                firstEvent = false,
                                lastEvent = true,
                                currentAlgorithm = Algorithms.Cipher2,
                                lastRoundSummary = lastRoundSummary,
                                anyRoundSummary = null
                            };

                            //prepare UI to show last round results
                            _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                                (SendOrPostCallback)delegate { _pres.RefreshSummaryView(summaryViewRefreshArgs); }, null);

                            c1Attack.LastRoundResult = lastRoundResult;

                            byte[] result = new byte[4];
                            byte[] keybytes = BitConverter.GetBytes(c1Attack.subkey0);
                            result[0] = keybytes[1];
                            result[1] = keybytes[0];

                            ushort test = BitConverter.ToUInt16(keybytes, 0);

                            keybytes = BitConverter.GetBytes(c1Attack.subkey1);
                            result[2] = keybytes[1];
                            result[3] = keybytes[0];

                            //exit thread
                            if (_stop)
                            {
                                return;
                            }

                            RoundKeys = result;
                        }
                        else
                        {
                            GuiLogMessage(Resources.MessageNoResult, NotificationLevel.Warning);
                        }
                    }
                    break;
                case Algorithms.Cipher2:
                    {
                        Cipher2DifferentialKeyRecoveryAttack c2Attack = attack as Cipher2DifferentialKeyRecoveryAttack;

                        //set refresh setting for UI
                        Cipher2KeyRecovery c2KeyRecovery = keyRecovery as Cipher2KeyRecovery;
                        if (c2KeyRecovery != null)
                        {
                            c2KeyRecovery.refreshUi = _settings.UIUpdateWhileExecution;
                            c2KeyRecovery.threadCount = _settings.ThreadCount;
                        }

                        SummaryAnyRound anyRoundSummary;
                        SummaryViewRefreshArgs summaryViewRefreshArgs;
                        SummaryLastRound lastRoundSummary;

                        bool firstIteration = true;
                        bool lastIteration = false;

                        //attack k4
                        while (!c2Attack.recoveredSubkey3)
                        {
                            _nextStep.WaitOne();

                            _currentProgressValue = 0.0;
                            ProgressChanged(_currentProgressValue, _progressMaximum);

                            //exit thread
                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                //try to recover key bits and save results
                                DifferentialAttackRoundResult round3Result =
                                    keyRecovery.RecoverKeyInformation(c2Attack, roundConfiguration);

                                if (round3Result == null)
                                {
                                    return;
                                }

                                c2Attack.RoundConfigurations.Add(roundConfiguration);
                                c2Attack.RoundResults.Add(round3Result);

                                //save key bits
                                c2Attack.subkey3 = (ushort)(c2Attack.subkey3 ^ round3Result.PossibleKey);

                                //save attacked SBoxes
                                if (!c2Attack.attackedSBoxesRound3[0] && roundConfiguration.ActiveSBoxes[0])
                                {
                                    c2Attack.attackedSBoxesRound3[0] = true;
                                }

                                //save attacked SBoxes
                                if (!c2Attack.attackedSBoxesRound3[1] && roundConfiguration.ActiveSBoxes[1])
                                {
                                    c2Attack.attackedSBoxesRound3[1] = true;
                                }

                                //save attacked SBoxes
                                if (!c2Attack.attackedSBoxesRound3[2] && roundConfiguration.ActiveSBoxes[2])
                                {
                                    c2Attack.attackedSBoxesRound3[2] = true;
                                }

                                //save attacked SBoxes
                                if (!c2Attack.attackedSBoxesRound3[3] && roundConfiguration.ActiveSBoxes[3])
                                {
                                    c2Attack.attackedSBoxesRound3[3] = true;
                                }

                                anyRoundSummary = new SummaryAnyRound()
                                {
                                    decryptionCount = roundConfiguration.FilteredPairList.Count * 2 *
                                                      roundConfiguration.ActiveSBoxes.Count(b => b),
                                    startTime = DateTime.Now,
                                    endTime = DateTime.MinValue,
                                    messageCount = roundConfiguration.UnfilteredPairList.Count * 2,
                                    recoveredSubKey = Convert.ToString(c2Attack.subkey3, 2).PadLeft(16, '0'),
                                    testedKeys = round3Result.KeyCandidateProbabilities.Count
                                };

                                //check if we attacked all SBoxes
                                if (c2Attack.attackedSBoxesRound3[0] && c2Attack.attackedSBoxesRound3[1] &&
                                    c2Attack.attackedSBoxesRound3[2] && c2Attack.attackedSBoxesRound3[3])
                                {
                                    c2Attack.recoveredSubkey3 = true;
                                    lastIteration = true;
                                    anyRoundSummary.endTime = DateTime.Now;
                                }

                                summaryViewRefreshArgs = new SummaryViewRefreshArgs()
                                {
                                    currentRound = 3,
                                    firstEvent = firstIteration,
                                    lastEvent = lastIteration,
                                    currentAlgorithm = Algorithms.Cipher2,
                                    anyRoundSummary = anyRoundSummary,
                                    lastRoundSummary = null
                                };

                                //prepare UI to show last round results
                                _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                                    (SendOrPostCallback)delegate { _pres.RefreshSummaryView(summaryViewRefreshArgs); },
                                    null);
                            }
                            catch (OperationCanceledException)
                            {
                                if (_stop)
                                {
                                    return;
                                }
                            }
                            catch (Exception e)
                            {
                                GuiLogMessage(e.Message, NotificationLevel.Error);
                                return;
                            }
                            finally
                            {
                                _semaphoreSlim.Wait();
                                try
                                {
                                    ((Cipher2KeyRecovery)keyRecovery).Cts.Dispose();
                                    ((Cipher2KeyRecovery)keyRecovery).Cts = new CancellationTokenSource();
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            //set finished flag to indicate that we can go further
                            Finished = true;
                            firstIteration = false;
                            _currentProgressValue = 1.0;
                            ProgressChanged(_currentProgressValue, _progressMaximum);

                            if (!_settings.AutomaticMode)
                            {
                                //prepare UI
                                _pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                                {
                                    _pres.IsNextStepPanelVisible = Visibility.Visible;
                                }, null);
                            }
                        }

                        //reset iteration indicators for events
                        firstIteration = true;
                        lastIteration = false;

                        _currentProgressValue = 0.0;

                        //attack k3
                        while (!c2Attack.recoveredSubkey2)
                        {
                            _nextStep.WaitOne();
                            _currentProgressValue = 0.0;
                            ProgressChanged(_currentProgressValue, _progressMaximum);

                            //exit thread
                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                //try to recover key bits and save results
                                DifferentialAttackRoundResult round2Result =
                                    keyRecovery.RecoverKeyInformation(c2Attack, roundConfiguration);

                                if (round2Result == null)
                                {
                                    return;
                                }

                                c2Attack.RoundConfigurations.Add(roundConfiguration);
                                c2Attack.RoundResults.Add(round2Result);

                                //save key bits
                                c2Attack.subkey2 = (ushort)(c2Attack.subkey2 ^ round2Result.PossibleKey);

                                //save attacked SBoxes
                                if (!c2Attack.attackedSBoxesRound2[0] && roundConfiguration.ActiveSBoxes[0])
                                {
                                    c2Attack.attackedSBoxesRound2[0] = true;
                                }

                                //save attacked SBoxes
                                if (!c2Attack.attackedSBoxesRound2[1] && roundConfiguration.ActiveSBoxes[1])
                                {
                                    c2Attack.attackedSBoxesRound2[1] = true;
                                }

                                //save attacked SBoxes
                                if (!c2Attack.attackedSBoxesRound2[2] && roundConfiguration.ActiveSBoxes[2])
                                {
                                    c2Attack.attackedSBoxesRound2[2] = true;
                                }

                                //save attacked SBoxes
                                if (!c2Attack.attackedSBoxesRound2[3] && roundConfiguration.ActiveSBoxes[3])
                                {
                                    c2Attack.attackedSBoxesRound2[3] = true;
                                }

                                anyRoundSummary = new SummaryAnyRound()
                                {
                                    decryptionCount = roundConfiguration.FilteredPairList.Count * 2 *
                                                      roundConfiguration.ActiveSBoxes.Count(b => b),
                                    startTime = DateTime.Now,
                                    endTime = DateTime.MinValue,
                                    messageCount = roundConfiguration.UnfilteredPairList.Count * 2,
                                    recoveredSubKey = Convert.ToString(c2Attack.subkey2, 2).PadLeft(16, '0'),
                                    testedKeys = round2Result.KeyCandidateProbabilities.Count
                                };

                                //check if we attacked all SBoxes
                                if (c2Attack.attackedSBoxesRound2[0] && c2Attack.attackedSBoxesRound2[1] &&
                                    c2Attack.attackedSBoxesRound2[2] && c2Attack.attackedSBoxesRound2[3])
                                {
                                    c2Attack.recoveredSubkey2 = true;
                                    lastIteration = true;
                                    anyRoundSummary.endTime = DateTime.Now;
                                }

                                summaryViewRefreshArgs = new SummaryViewRefreshArgs()
                                {
                                    currentRound = 2,
                                    firstEvent = firstIteration,
                                    lastEvent = lastIteration,
                                    currentAlgorithm = Algorithms.Cipher2,
                                    anyRoundSummary = anyRoundSummary,
                                    lastRoundSummary = null
                                };

                                //prepare UI to show last round results
                                _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                                    (SendOrPostCallback)delegate { _pres.RefreshSummaryView(summaryViewRefreshArgs); },
                                    null);
                            }
                            catch (OperationCanceledException)
                            {
                                if (_stop)
                                {
                                    return;
                                }
                            }
                            catch (Exception e)
                            {
                                GuiLogMessage(e.Message, NotificationLevel.Error);
                                return;
                            }
                            finally
                            {
                                _semaphoreSlim.Wait();
                                try
                                {
                                    ((Cipher2KeyRecovery)keyRecovery).Cts.Dispose();
                                    ((Cipher2KeyRecovery)keyRecovery).Cts = new CancellationTokenSource();
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            //set finished flag to indicate that we can go further
                            Finished = true;
                            firstIteration = false;
                            _currentProgressValue = 1.0;
                            ProgressChanged(_currentProgressValue, _progressMaximum);

                            if (!_settings.AutomaticMode)
                            {
                                //prepare UI
                                _pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                                {
                                    _pres.IsNextStepPanelVisible = Visibility.Visible;
                                }, null);
                            }
                        }

                        //exit thread
                        if (_stop)
                        {
                            return;
                        }

                        //reset iteration indicators for events
                        firstIteration = true;
                        lastIteration = false;

                        _nextStep.WaitOne();
                        _currentProgressValue = 0.0;
                        ProgressChanged(_currentProgressValue, _progressMaximum);

                        //exit thread
                        if (_stop)
                        {
                            return;
                        }

                        //prepare UI to show last round results
                        _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                            (SendOrPostCallback)delegate { _pres.PrepareUIForLastRound(); }, null);

                        lastRoundSummary = new SummaryLastRound()
                        {
                            decryptionCount = 0,
                            startTime = DateTime.Now,
                            endTime = DateTime.Now,
                            messageCount = 0,
                            recoveredSubKey0 = "",
                            recoveredSubKey1 = "",
                            testedKeys = 0
                        };

                        summaryViewRefreshArgs = new SummaryViewRefreshArgs()
                        {
                            currentRound = 1,
                            firstEvent = true,
                            lastEvent = false,
                            currentAlgorithm = Algorithms.Cipher2,
                            lastRoundSummary = lastRoundSummary,
                            anyRoundSummary = null
                        };

                        //prepare UI to show last round results
                        _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                            (SendOrPostCallback)delegate { _pres.RefreshSummaryView(summaryViewRefreshArgs); }, null);

                        //exit thread
                        if (_stop)
                        {
                            return;
                        }

                        //Attack k2 and k1
                        DifferentialAttackLastRoundResult lastRoundResult = keyRecovery.AttackFirstRound(attack);
                        _currentProgressValue = 1.0;
                        ProgressChanged(_currentProgressValue, _progressMaximum);

                        //exit thread
                        if (_stop)
                        {
                            return;
                        }

                        //check if there is a result
                        if (lastRoundResult != null)
                        {
                            c2Attack.recoveredSubkey1 = true;
                            c2Attack.subkey1 = lastRoundResult.SubKey1;
                            c2Attack.recoveredSubkey0 = true;
                            c2Attack.subkey0 = lastRoundResult.SubKey0;
                            c2Attack.LastRoundResult = lastRoundResult;

                            lastRoundSummary = new SummaryLastRound()
                            {
                                decryptionCount = lastRoundResult.DecryptionCounter,
                                startTime = DateTime.Now,
                                endTime = DateTime.Now,
                                messageCount = neededMessageCounterLastRound * 2,
                                recoveredSubKey0 = Convert.ToString(c2Attack.subkey0, 2).PadLeft(16, '0'),
                                recoveredSubKey1 = Convert.ToString(c2Attack.subkey1, 2).PadLeft(16, '0'),
                                testedKeys = lastRoundResult.KeyCounter
                            };

                            summaryViewRefreshArgs = new SummaryViewRefreshArgs()
                            {
                                currentRound = 1,
                                firstEvent = false,
                                lastEvent = true,
                                currentAlgorithm = Algorithms.Cipher2,
                                lastRoundSummary = lastRoundSummary,
                                anyRoundSummary = null
                            };

                            //prepare UI to show last round results
                            _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                                (SendOrPostCallback)delegate { _pres.RefreshSummaryView(summaryViewRefreshArgs); }, null);

                            //add k0
                            byte[] keyBytes = BitConverter.GetBytes(c2Attack.subkey0);
                            byte[] result = new byte[8];

                            //add k0
                            result[0] = keyBytes[1];
                            result[1] = keyBytes[0];

                            //add k1
                            keyBytes = BitConverter.GetBytes(c2Attack.subkey1);
                            result[2] = keyBytes[1];
                            result[3] = keyBytes[0];

                            //add k2
                            keyBytes = BitConverter.GetBytes(c2Attack.subkey2);
                            result[4] = keyBytes[1];
                            result[5] = keyBytes[0];

                            //add k3
                            keyBytes = BitConverter.GetBytes(c2Attack.subkey3);
                            result[6] = keyBytes[1];
                            result[7] = keyBytes[0];

                            RoundKeys = result;
                        }
                        else
                        {
                            GuiLogMessage(Resources.MessageNoResult, NotificationLevel.Warning);
                        }
                    }
                    break;
                case Algorithms.Cipher3:
                    {
                        Cipher3DifferentialKeyRecoveryAttack c3Attack = attack as Cipher3DifferentialKeyRecoveryAttack;

                        //set refresh setting for UI
                        Cipher3KeyRecovery c3KeyRecovery = keyRecovery as Cipher3KeyRecovery;
                        if (c3KeyRecovery != null)
                        {
                            c3KeyRecovery.refreshUi = _settings.UIUpdateWhileExecution;
                            c3KeyRecovery.threadCount = _settings.ThreadCount;
                        }

                        SummaryAnyRound anyRoundSummary;
                        SummaryViewRefreshArgs summaryViewRefreshArgs;
                        SummaryLastRound lastRoundSummary;

                        bool firstIteration = true;
                        bool lastIteration = false;

                        //attack k5
                        while (!c3Attack.recoveredSubkey5)
                        {
                            _nextStep.WaitOne();
                            _currentProgressValue = 0.0;
                            ProgressChanged(_currentProgressValue, _progressMaximum);

                            if (_stop == true)
                            {
                                return;
                            }

                            try
                            {
                                //try to recover key bits and save results
                                DifferentialAttackRoundResult round5Result = keyRecovery.RecoverKeyInformation(c3Attack, roundConfiguration);

                                if (round5Result == null)
                                {
                                    return;
                                }

                                c3Attack.RoundConfigurations.Add(roundConfiguration);
                                c3Attack.RoundResults.Add(round5Result);

                                //save key bits
                                c3Attack.subkey5 = (ushort)(c3Attack.subkey5 ^ round5Result.PossibleKey);

                                //save attacked SBoxes
                                if (!c3Attack.attackedSBoxesRound5[0] && roundConfiguration.ActiveSBoxes[0])
                                {
                                    c3Attack.attackedSBoxesRound5[0] = true;
                                }

                                //save attacked SBoxes
                                if (!c3Attack.attackedSBoxesRound5[1] && roundConfiguration.ActiveSBoxes[1])
                                {
                                    c3Attack.attackedSBoxesRound5[1] = true;
                                }

                                //save attacked SBoxes
                                if (!c3Attack.attackedSBoxesRound5[2] && roundConfiguration.ActiveSBoxes[2])
                                {
                                    c3Attack.attackedSBoxesRound5[2] = true;
                                }

                                //save attacked SBoxes
                                if (!c3Attack.attackedSBoxesRound5[3] && roundConfiguration.ActiveSBoxes[3])
                                {
                                    c3Attack.attackedSBoxesRound5[3] = true;
                                }

                                anyRoundSummary = new SummaryAnyRound()
                                {
                                    decryptionCount = roundConfiguration.FilteredPairList.Count * 2 *
                                                      roundConfiguration.ActiveSBoxes.Count(b => b),
                                    startTime = DateTime.Now,
                                    endTime = DateTime.MinValue,
                                    messageCount = roundConfiguration.UnfilteredPairList.Count * 2,
                                    recoveredSubKey = Convert.ToString(c3Attack.subkey5, 2).PadLeft(16, '0'),
                                    testedKeys = round5Result.KeyCandidateProbabilities.Count
                                };

                                //check if we attacked all SBoxes
                                if (c3Attack.attackedSBoxesRound5[0] && c3Attack.attackedSBoxesRound5[1] &&
                                    c3Attack.attackedSBoxesRound5[2] && c3Attack.attackedSBoxesRound5[3])
                                {
                                    c3Attack.recoveredSubkey5 = true;
                                    lastIteration = true;
                                    anyRoundSummary.endTime = DateTime.Now;
                                }

                                summaryViewRefreshArgs = new SummaryViewRefreshArgs()
                                {
                                    currentRound = 5,
                                    firstEvent = firstIteration,
                                    lastEvent = lastIteration,
                                    currentAlgorithm = Algorithms.Cipher3,
                                    anyRoundSummary = anyRoundSummary,
                                    lastRoundSummary = null
                                };

                                //prepare UI to show last round results
                                _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                                    (SendOrPostCallback)delegate { _pres.RefreshSummaryView(summaryViewRefreshArgs); },
                                    null);
                            }
                            catch (OperationCanceledException)
                            {
                                if (_stop)
                                {
                                    return;
                                }
                            }
                            catch (Exception e)
                            {
                                GuiLogMessage(e.Message, NotificationLevel.Error);
                                return;
                            }
                            finally
                            {
                                _semaphoreSlim.Wait();
                                try
                                {
                                    ((Cipher3KeyRecovery)keyRecovery).Cts.Dispose();
                                    ((Cipher3KeyRecovery)keyRecovery).Cts = new CancellationTokenSource();
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            //set finished flag to indicate that we can go further
                            Finished = true;
                            firstIteration = false;

                            _currentProgressValue = 1.0;
                            ProgressChanged(_currentProgressValue, _progressMaximum);

                            if (!_settings.AutomaticMode)
                            {
                                //prepare UI
                                _pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                                {
                                    _pres.IsNextStepPanelVisible = Visibility.Visible;
                                }, null);
                            }
                        }

                        //reset iteration indicators for events
                        firstIteration = true;
                        lastIteration = false;

                        //attack k4
                        while (!c3Attack.recoveredSubkey4)
                        {
                            _nextStep.WaitOne();
                            _currentProgressValue = 0.0;
                            ProgressChanged(_currentProgressValue, _progressMaximum);

                            //exit thread
                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                //try to recover key bits and save results
                                DifferentialAttackRoundResult round4Result =
                                    keyRecovery.RecoverKeyInformation(c3Attack, roundConfiguration);

                                if (_stop == true)
                                {
                                    return;
                                }

                                c3Attack.RoundConfigurations.Add(roundConfiguration);
                                c3Attack.RoundResults.Add(round4Result);

                                //save key bits
                                c3Attack.subkey4 = (ushort)(c3Attack.subkey4 ^ round4Result.PossibleKey);

                                //save attacked SBoxes
                                if (!c3Attack.attackedSBoxesRound4[0] && roundConfiguration.ActiveSBoxes[0])
                                {
                                    c3Attack.attackedSBoxesRound4[0] = true;
                                }

                                //save attacked SBoxes
                                if (!c3Attack.attackedSBoxesRound4[1] && roundConfiguration.ActiveSBoxes[1])
                                {
                                    c3Attack.attackedSBoxesRound4[1] = true;
                                }

                                //save attacked SBoxes
                                if (!c3Attack.attackedSBoxesRound4[2] && roundConfiguration.ActiveSBoxes[2])
                                {
                                    c3Attack.attackedSBoxesRound4[2] = true;
                                }

                                //save attacked SBoxes
                                if (!c3Attack.attackedSBoxesRound4[3] && roundConfiguration.ActiveSBoxes[3])
                                {
                                    c3Attack.attackedSBoxesRound4[3] = true;
                                }

                                anyRoundSummary = new SummaryAnyRound()
                                {
                                    decryptionCount = roundConfiguration.FilteredPairList.Count * 2 *
                                                      roundConfiguration.ActiveSBoxes.Count(b => b),
                                    startTime = DateTime.Now,
                                    endTime = DateTime.MinValue,
                                    messageCount = roundConfiguration.UnfilteredPairList.Count * 2,
                                    recoveredSubKey = Convert.ToString(c3Attack.subkey4, 2).PadLeft(16, '0'),
                                    testedKeys = round4Result.KeyCandidateProbabilities.Count
                                };

                                //check if we attacked all SBoxes
                                if (c3Attack.attackedSBoxesRound4[0] && c3Attack.attackedSBoxesRound4[1] &&
                                    c3Attack.attackedSBoxesRound4[2] && c3Attack.attackedSBoxesRound4[3])
                                {
                                    c3Attack.recoveredSubkey4 = true;
                                    lastIteration = true;
                                    anyRoundSummary.endTime = DateTime.Now;
                                }

                                summaryViewRefreshArgs = new SummaryViewRefreshArgs()
                                {
                                    currentRound = 4,
                                    firstEvent = firstIteration,
                                    lastEvent = lastIteration,
                                    currentAlgorithm = Algorithms.Cipher2,
                                    anyRoundSummary = anyRoundSummary,
                                    lastRoundSummary = null
                                };

                                //prepare UI to show last round results
                                _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                                    (SendOrPostCallback)delegate { _pres.RefreshSummaryView(summaryViewRefreshArgs); },
                                    null);
                            }
                            catch (OperationCanceledException)
                            {
                                if (_stop)
                                {
                                    return;
                                }
                            }
                            catch (Exception e)
                            {
                                GuiLogMessage(e.Message, NotificationLevel.Error);
                                return;
                            }
                            finally
                            {
                                _semaphoreSlim.Wait();
                                try
                                {
                                    ((Cipher3KeyRecovery)keyRecovery).Cts.Dispose();
                                    ((Cipher3KeyRecovery)keyRecovery).Cts = new CancellationTokenSource();
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }


                            //set finished flag to indicate that we can go further
                            Finished = true;
                            firstIteration = false;

                            _currentProgressValue = 1.0;
                            ProgressChanged(_currentProgressValue, _progressMaximum);
                            if (!_settings.AutomaticMode)
                            {
                                //prepare UI
                                _pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                                {
                                    _pres.IsNextStepPanelVisible = Visibility.Visible;
                                }, null);
                            }
                        }

                        //reset iteration indicators for events
                        firstIteration = true;
                        lastIteration = false;

                        //attack k3
                        while (!c3Attack.recoveredSubkey3)
                        {
                            _nextStep.WaitOne();
                            _currentProgressValue = 0.0;
                            ProgressChanged(_currentProgressValue, _progressMaximum);

                            //exit thread
                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                //try to recover key bits and save results
                                DifferentialAttackRoundResult round3Result =
                                    keyRecovery.RecoverKeyInformation(c3Attack, roundConfiguration);

                                if (_stop == true)
                                {
                                    return;
                                }

                                c3Attack.RoundConfigurations.Add(roundConfiguration);
                                c3Attack.RoundResults.Add(round3Result);

                                //save key bits
                                c3Attack.subkey3 = (ushort)(c3Attack.subkey3 ^ round3Result.PossibleKey);

                                //save attacked SBoxes
                                if (!c3Attack.attackedSBoxesRound3[0] && roundConfiguration.ActiveSBoxes[0])
                                {
                                    c3Attack.attackedSBoxesRound3[0] = true;
                                }

                                //save attacked SBoxes
                                if (!c3Attack.attackedSBoxesRound3[1] && roundConfiguration.ActiveSBoxes[1])
                                {
                                    c3Attack.attackedSBoxesRound3[1] = true;
                                }

                                //save attacked SBoxes
                                if (!c3Attack.attackedSBoxesRound3[2] && roundConfiguration.ActiveSBoxes[2])
                                {
                                    c3Attack.attackedSBoxesRound3[2] = true;
                                }

                                //save attacked SBoxes
                                if (!c3Attack.attackedSBoxesRound3[3] && roundConfiguration.ActiveSBoxes[3])
                                {
                                    c3Attack.attackedSBoxesRound3[3] = true;
                                }

                                anyRoundSummary = new SummaryAnyRound()
                                {
                                    decryptionCount = roundConfiguration.FilteredPairList.Count * 2 *
                                                      roundConfiguration.ActiveSBoxes.Count(b => b),
                                    startTime = DateTime.Now,
                                    endTime = DateTime.MinValue,
                                    messageCount = roundConfiguration.UnfilteredPairList.Count * 2,
                                    recoveredSubKey = Convert.ToString(c3Attack.subkey3, 2).PadLeft(16, '0'),
                                    testedKeys = round3Result.KeyCandidateProbabilities.Count
                                };

                                //check if we attacked all SBoxes
                                if (c3Attack.attackedSBoxesRound3[0] && c3Attack.attackedSBoxesRound3[1] &&
                                    c3Attack.attackedSBoxesRound3[2] && c3Attack.attackedSBoxesRound3[3])
                                {
                                    c3Attack.recoveredSubkey3 = true;
                                    lastIteration = true;
                                    anyRoundSummary.endTime = DateTime.Now;
                                }

                                summaryViewRefreshArgs = new SummaryViewRefreshArgs()
                                {
                                    currentRound = 3,
                                    firstEvent = firstIteration,
                                    lastEvent = lastIteration,
                                    currentAlgorithm = Algorithms.Cipher2,
                                    anyRoundSummary = anyRoundSummary,
                                    lastRoundSummary = null
                                };

                                //prepare UI to show last round results
                                _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                                    (SendOrPostCallback)delegate { _pres.RefreshSummaryView(summaryViewRefreshArgs); },
                                    null);
                            }
                            catch (OperationCanceledException)
                            {
                                if (_stop)
                                {
                                    return;
                                }
                            }
                            catch (Exception e)
                            {
                                GuiLogMessage(e.Message, NotificationLevel.Error);
                                return;
                            }
                            finally
                            {
                                _semaphoreSlim.Wait();
                                try
                                {
                                    ((Cipher3KeyRecovery)keyRecovery).Cts.Dispose();
                                    ((Cipher3KeyRecovery)keyRecovery).Cts = new CancellationTokenSource();
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            //set finished flag to indicate that we can go further
                            Finished = true;
                            firstIteration = false;

                            _currentProgressValue = 1.0;
                            ProgressChanged(_currentProgressValue, _progressMaximum);
                            if (!_settings.AutomaticMode)
                            {
                                //prepare UI
                                _pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                                {
                                    _pres.IsNextStepPanelVisible = Visibility.Visible;
                                }, null);
                            }
                        }

                        //reset iteration indicators for events
                        firstIteration = true;
                        lastIteration = false;

                        //attack k2
                        while (!c3Attack.recoveredSubkey2)
                        {
                            _nextStep.WaitOne();
                            _currentProgressValue = 0.0;
                            ProgressChanged(_currentProgressValue, _progressMaximum);

                            //exit thread
                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                //try to recover key bits and save results
                                DifferentialAttackRoundResult round2Result =
                                    keyRecovery.RecoverKeyInformation(c3Attack, roundConfiguration);

                                if (_stop == true)
                                {
                                    return;
                                }

                                c3Attack.RoundConfigurations.Add(roundConfiguration);
                                c3Attack.RoundResults.Add(round2Result);

                                //save key bits
                                c3Attack.subkey2 = (ushort)(c3Attack.subkey2 ^ round2Result.PossibleKey);

                                //save attacked SBoxes
                                if (!c3Attack.attackedSBoxesRound2[0] && roundConfiguration.ActiveSBoxes[0])
                                {
                                    c3Attack.attackedSBoxesRound2[0] = true;
                                }

                                //save attacked SBoxes
                                if (!c3Attack.attackedSBoxesRound2[1] && roundConfiguration.ActiveSBoxes[1])
                                {
                                    c3Attack.attackedSBoxesRound2[1] = true;
                                }

                                //save attacked SBoxes
                                if (!c3Attack.attackedSBoxesRound2[2] && roundConfiguration.ActiveSBoxes[2])
                                {
                                    c3Attack.attackedSBoxesRound2[2] = true;
                                }

                                //save attacked SBoxes
                                if (!c3Attack.attackedSBoxesRound2[3] && roundConfiguration.ActiveSBoxes[3])
                                {
                                    c3Attack.attackedSBoxesRound2[3] = true;
                                }

                                anyRoundSummary = new SummaryAnyRound()
                                {
                                    decryptionCount = roundConfiguration.FilteredPairList.Count * 2 *
                                                      roundConfiguration.ActiveSBoxes.Count(b => b),
                                    startTime = DateTime.Now,
                                    endTime = DateTime.MinValue,
                                    messageCount = roundConfiguration.UnfilteredPairList.Count * 2,
                                    recoveredSubKey = Convert.ToString(c3Attack.subkey2, 2).PadLeft(16, '0'),
                                    testedKeys = round2Result.KeyCandidateProbabilities.Count
                                };

                                //check if we attacked all SBoxes
                                if (c3Attack.attackedSBoxesRound2[0] && c3Attack.attackedSBoxesRound2[1] &&
                                    c3Attack.attackedSBoxesRound2[2] && c3Attack.attackedSBoxesRound2[3])
                                {
                                    c3Attack.recoveredSubkey2 = true;
                                    lastIteration = true;
                                    anyRoundSummary.endTime = DateTime.Now;
                                }

                                summaryViewRefreshArgs = new SummaryViewRefreshArgs()
                                {
                                    currentRound = 2,
                                    firstEvent = firstIteration,
                                    lastEvent = lastIteration,
                                    currentAlgorithm = Algorithms.Cipher2,
                                    anyRoundSummary = anyRoundSummary,
                                    lastRoundSummary = null
                                };

                                //prepare UI to show last round results
                                _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                                    (SendOrPostCallback)delegate { _pres.RefreshSummaryView(summaryViewRefreshArgs); },
                                    null);
                            }
                            catch (OperationCanceledException)
                            {
                                if (_stop)
                                {
                                    return;
                                }
                            }
                            catch (Exception e)
                            {
                                GuiLogMessage(e.Message, NotificationLevel.Error);
                                return;
                            }
                            finally
                            {
                                _semaphoreSlim.Wait();
                                try
                                {
                                    ((Cipher3KeyRecovery)keyRecovery).Cts.Dispose();
                                    ((Cipher3KeyRecovery)keyRecovery).Cts = new CancellationTokenSource();
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            //set finished flag to indicate that we can go further
                            Finished = true;
                            firstIteration = false;

                            _currentProgressValue = 1.0;
                            ProgressChanged(_currentProgressValue, _progressMaximum);
                            if (!_settings.AutomaticMode)
                            {
                                //prepare UI
                                _pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                                {
                                    _pres.IsNextStepPanelVisible = Visibility.Visible;
                                }, null);
                            }
                        }

                        //exit thread
                        if (_stop)
                        {
                            return;
                        }

                        //reset iteration indicators for events
                        firstIteration = true;
                        lastIteration = false;

                        _nextStep.WaitOne();

                        _currentProgressValue = 0.0;
                        ProgressChanged(_currentProgressValue, _progressMaximum);

                        //exit thread
                        if (_stop)
                        {
                            return;
                        }

                        //prepare UI to show last round results
                        _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                            (SendOrPostCallback)delegate { _pres.PrepareUIForLastRound(); }, null);

                        lastRoundSummary = new SummaryLastRound()
                        {
                            decryptionCount = 0,
                            startTime = DateTime.Now,
                            endTime = DateTime.Now,
                            messageCount = 0,
                            recoveredSubKey0 = "",
                            recoveredSubKey1 = "",
                            testedKeys = 0
                        };

                        summaryViewRefreshArgs = new SummaryViewRefreshArgs()
                        {
                            currentRound = 1,
                            firstEvent = true,
                            lastEvent = false,
                            currentAlgorithm = Algorithms.Cipher3,
                            lastRoundSummary = lastRoundSummary,
                            anyRoundSummary = null
                        };

                        //prepare UI to show last round results
                        _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                            (SendOrPostCallback)delegate { _pres.RefreshSummaryView(summaryViewRefreshArgs); }, null);

                        //exit thread
                        if (_stop)
                        {
                            return;
                        }

                        //Attack k2 and k1
                        DifferentialAttackLastRoundResult lastRoundResult = keyRecovery.AttackFirstRound(attack);

                        //exit thread
                        if (_stop)
                        {
                            return;
                        }

                        //check if there is a result
                        if (lastRoundResult != null)
                        {
                            c3Attack.recoveredSubkey1 = true;
                            c3Attack.subkey1 = lastRoundResult.SubKey1;
                            c3Attack.recoveredSubkey0 = true;
                            c3Attack.subkey0 = lastRoundResult.SubKey0;
                            c3Attack.LastRoundResult = lastRoundResult;

                            lastRoundSummary = new SummaryLastRound()
                            {
                                decryptionCount = lastRoundResult.DecryptionCounter,
                                startTime = DateTime.Now,
                                endTime = DateTime.Now,
                                messageCount = neededMessageCounterLastRound * 2,
                                recoveredSubKey0 = Convert.ToString(c3Attack.subkey0, 2).PadLeft(16, '0'),
                                recoveredSubKey1 = Convert.ToString(c3Attack.subkey1, 2).PadLeft(16, '0'),
                                testedKeys = lastRoundResult.KeyCounter
                            };

                            summaryViewRefreshArgs = new SummaryViewRefreshArgs()
                            {
                                currentRound = 1,
                                firstEvent = false,
                                lastEvent = true,
                                currentAlgorithm = Algorithms.Cipher2,
                                lastRoundSummary = lastRoundSummary,
                                anyRoundSummary = null
                            };

                            //prepare UI to show last round results
                            _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                                (SendOrPostCallback)delegate { _pres.RefreshSummaryView(summaryViewRefreshArgs); }, null);

                            //add k0
                            byte[] keyBytes = BitConverter.GetBytes(c3Attack.subkey0);
                            byte[] result = new byte[12];

                            //add k0
                            result[0] = keyBytes[1];
                            result[1] = keyBytes[0];

                            //add k1
                            keyBytes = BitConverter.GetBytes(c3Attack.subkey1);
                            result[2] = keyBytes[1];
                            result[3] = keyBytes[0];

                            //add k2
                            keyBytes = BitConverter.GetBytes(c3Attack.subkey2);
                            result[4] = keyBytes[1];
                            result[5] = keyBytes[0];

                            //add k3
                            keyBytes = BitConverter.GetBytes(c3Attack.subkey3);
                            result[6] = keyBytes[1];
                            result[7] = keyBytes[0];

                            //add k4
                            keyBytes = BitConverter.GetBytes(c3Attack.subkey4);
                            result[8] = keyBytes[1];
                            result[9] = keyBytes[0];

                            //add k5
                            keyBytes = BitConverter.GetBytes(c3Attack.subkey5);
                            result[10] = keyBytes[1];
                            result[11] = keyBytes[0];

                            RoundKeys = result;
                        }
                        else
                        {
                            GuiLogMessage(Resources.MessageNoResult, NotificationLevel.Warning);
                        }
                    }
                    break;
            }

            ProgressChanged(1, _progressMaximum);
        }

        /// <summary>
        /// Executed if new message pairs are needed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateNewMessagePair(object sender, EventArgs e)
        {
            neededMessageCounterLastRound++;
            NeededMessageCount = 1;
            MessageDifference = (new Random()).Next(0, ((int)Math.Pow(2, 16) - 1));
        }

        /// <summary>
        /// reads input plaintext pairs from the input
        /// </summary>
        /// <returns></returns>
        private List<Pair> readPlaintextPairs()
        {
            List<Pair> result = new List<Pair>();

            //Read all messages
            using (CStreamReader reader = UnencryptedMessagePairs.CreateReader())
            {
                if ((reader.Length % 4) != 0)
                {
                    //GuiLogMessage(Resources.MessageError, NotificationLevel.Error);
                    //ProgressChanged(1, 1);
                    return null;
                }

                byte[] inputBlocks = new byte[reader.Length];
                int readCount = 0;
                while ((readCount += reader.Read(inputBlocks, readCount, 2)) < reader.Length &&
                       reader.Position < reader.Length && !_stop)
                {
                }

                for (int i = 0; i < inputBlocks.Length; i += 4)
                {
                    byte[] leftMemberBytes = new byte[2];
                    leftMemberBytes[1] = inputBlocks[i];
                    leftMemberBytes[0] = inputBlocks[i + 1];

                    byte[] rightMemberBytes = new byte[2];
                    rightMemberBytes[1] = inputBlocks[i + 2];
                    rightMemberBytes[0] = inputBlocks[i + 3];

                    ushort leftMemberInt = BitConverter.ToUInt16(leftMemberBytes, 0);
                    ushort rightMemberInt = BitConverter.ToUInt16(rightMemberBytes, 0);
                    result.Add(new Pair() { LeftMember = leftMemberInt, RightMember = rightMemberInt });
                }
            }

            return result;
        }

        /// <summary>
        /// reads input ciphertext pairs from the input
        /// </summary>
        /// <returns></returns>
        private List<Pair> readCipherTextPairs()
        {
            List<Pair> result = new List<Pair>();

            //Read all messages
            using (CStreamReader reader = EncryptedMessagePairs.CreateReader())
            {
                if ((reader.Length % 4) != 0)
                {
                    //GuiLogMessage(Resources.MessageError, NotificationLevel.Error);
                    //ProgressChanged(1, 1);
                    return null;
                }

                byte[] inputBlocks = new byte[reader.Length];
                int readCount = 0;
                while ((readCount += reader.Read(inputBlocks, readCount, 2)) < reader.Length &&
                       reader.Position < reader.Length && !_stop)
                {
                }

                for (int i = 0; i < inputBlocks.Length; i += 4)
                {
                    byte[] leftMemberBytes = new byte[2];
                    leftMemberBytes[1] = inputBlocks[i];
                    leftMemberBytes[0] = inputBlocks[i + 1];

                    byte[] rightMemberBytes = new byte[2];
                    rightMemberBytes[1] = inputBlocks[i + 2];
                    rightMemberBytes[0] = inputBlocks[i + 3];

                    ushort leftMemberInt = BitConverter.ToUInt16(leftMemberBytes, 0);
                    ushort rightMemberInt = BitConverter.ToUInt16(rightMemberBytes, 0);
                    result.Add(new Pair() { LeftMember = leftMemberInt, RightMember = rightMemberInt });
                }
            }

            return result;
        }

        /// <summary>
        /// Reads json string and returns it as object
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private DifferentialAttackRoundConfiguration ReadConfiguration(string json)
        {
            DifferentialAttackRoundConfiguration config = null;
            json = json.Replace("DCAPathFinder", "DCAKeyRecovery");

            try
            {
                config = JsonConvert.DeserializeObject<DifferentialAttackRoundConfiguration>(json,
                    new Newtonsoft.Json.JsonSerializerSettings
                    {
                        TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                        NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                    });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return config;
        }

        /// <summary>
        /// Handles an occured error during changing a setting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleSettingsError(object sender, SettingsErrorMessagsEventArgs e)
        {
            GuiLogMessage(e.message, NotificationLevel.Warning);
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}