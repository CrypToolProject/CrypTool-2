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
using CrypTool.PluginBase.Miscellaneous;
using DCAPathFinder;
using DCAPathFinder.Logic;
using DCAPathFinder.Logic.Cipher1;
using DCAPathFinder.Logic.Cipher2;
using DCAPathFinder.Logic.Cipher3;
using DCAPathFinder.Properties;
using DCAPathFinder.UI;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CrypTool.Plugins.DCAPathFinder
{
    [Author("Christian Bender", "christian1.bender@student.uni-siegen.de", null, "http://www.uni-siegen.de")]
    [PluginInfo("DCAPathFinder.Properties.Resources", "PluginCaption", "PluginTooltip", "DCAPathFinder/userdoc.xml",
        new[] { "DCAPathFinder/Images/IC_DCAPathFinder.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    [AutoAssumeFullEndProgress(false)]
    public class DCAPathFinder : ICrypComponent
    {
        #region Private Variables

        private readonly DCAPathFinderSettings settings = new DCAPathFinderSettings();
        private readonly DCAPathFinderPres _activePresentation = new DCAPathFinderPres();
        private int _expectedDifferential;
        private int _messageCount;
        private bool _ready;
        private string _path;
        private bool _stop;
        private Thread _workerThread;
        private readonly AutoResetEvent _nextRound = new AutoResetEvent(false);
        private DifferentialKeyRecoveryAttack _differentialKeyRecoveryAttack = null;
        private IPathFinder pathFinder = null;
        private bool firstIteration = true;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private double _currentProgress;
        private readonly double _maxProgress = 1.0;

        private readonly bool _serializeData = false;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public DCAPathFinder()
        {
            _stop = false;
            settings.PropertyChanged += SettingChangedListener;
            settings.SettingsErrorOccured += HandleSettingsError;

            //Check specific algorithm and invoke the selection into the UI class
            if (settings.CurrentAlgorithm == Algorithms.Cipher1)
            {
                //dispatch action: set active tutorial number
                _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                    (SendOrPostCallback)delegate { _activePresentation.TutorialNumber = 1; }, null);
            }
            else if (settings.CurrentAlgorithm == Algorithms.Cipher2)
            {
                //dispatch action: set active tutorial number
                _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                    (SendOrPostCallback)delegate { _activePresentation.TutorialNumber = 2; }, null);
            }
            else if (settings.CurrentAlgorithm == Algorithms.Cipher3)
            {
                //dispatch action: set active tutorial number
                _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                    (SendOrPostCallback)delegate { _activePresentation.TutorialNumber = 3; }, null);
            }
            else if (settings.CurrentAlgorithm == Algorithms.Cipher4)
            {
                //dispatch action: set active tutorial number
                _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                    (SendOrPostCallback)delegate { _activePresentation.TutorialNumber = 4; }, null);
            }

            //dispatch action: setup view
            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                (SendOrPostCallback)delegate { _activePresentation.SetupView(); }, null);
        }

        #region Data Properties

        /// <summary>
        /// This output describes the characteristic path
        /// </summary>
        [PropertyInfo(Direction.OutputData, "Path", "PathToolTip")]
        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                OnPropertyChanged("Path");
            }
        }

        /// <summary>
        /// This output describes the expected value
        /// </summary>
        [PropertyInfo(Direction.OutputData, "ExpectedDifferential", "ExpectedDifferentialToolTip")]
        public int ExpectedDifferential
        {
            get => _expectedDifferential;
            set
            {
                _expectedDifferential = value;
                OnPropertyChanged("ExpectedDifferential");
            }
        }

        /// <summary>
        /// This output describes the count of messages to generate
        /// </summary>
        [PropertyInfo(Direction.OutputData, "MessageCount", "MessageCountToolTip")]
        public int MessageCount
        {
            get => _messageCount;
            set
            {
                _messageCount = value;
                OnPropertyChanged("MessageCount");
            }
        }

        /// <summary>
        /// This input signals to the component that the KeyRecovery component has finished its work 
        /// </summary>
        [PropertyInfo(Direction.InputData, "ReadyInput", "ReadyInputToolTip")]
        public bool Ready
        {
            get => _ready;
            set
            {
                _ready = value;
                OnPropertyChanged("Ready");
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
        public UserControl Presentation => _activePresentation;

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            _currentProgress = 0;

            //dispatch action: inform ui that workspace is running
            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
           {
               _activePresentation.SBoxesCurrentAttack = new bool[4];
               _activePresentation.SBoxesAlreadyAttacked = new bool[] { false, false, false, false };
               _activePresentation.PresentationMode = settings.PresentationMode;
               _activePresentation.AutomaticMode = settings.AutomaticMode;
               _activePresentation.SlideCounterVisibility = Visibility.Visible;
               _activePresentation.WorkspaceRunning = true;
               _activePresentation.SearchPolicy = settings.CurrentSearchPolicy;
               _activePresentation.MessageCount = settings.ChosenMessagePairsCount;
               _activePresentation.UIProgressRefresh = true;
               _activePresentation.UseOfflinePaths = settings.UseOfflinePaths;
           }, null);

            _activePresentation.MessageToDisplayOccured += displayMessage;
            _activePresentation.ProgressChangedOccured += UIRefreshProgress;

            firstIteration = true;
            _stop = false;

            switch (settings.CurrentAlgorithm)
            {
                case Algorithms.Cipher1:
                    {
                        pathFinder = new Cipher1PathFinder();
                        _differentialKeyRecoveryAttack = new Cipher1DifferentialKeyRecoveryAttack();
                        pathFinder.ProgressChangedOccured += RefreshProgress;
                    }
                    break;
                case Algorithms.Cipher2:
                    {
                        pathFinder = new Cipher2PathFinder();
                        _differentialKeyRecoveryAttack = new Cipher2DifferentialKeyRecoveryAttack();
                        pathFinder.AttackSearchResultOccured += handleSearchresult;
                        pathFinder.ProgressChangedOccured += RefreshProgress;
                        Cipher2Configuration.PROBABILITYBOUNDBESTCHARACTERISTICSEARCH = settings.AbortingThresholdCharacteristicSearch;
                        Cipher2Configuration.PROBABILITYBOUNDDIFFERENTIALSEARCH = settings.AbortingThresholdDifferentialSearch;
                    }
                    break;
                case Algorithms.Cipher3:
                    {
                        pathFinder = new Cipher3PathFinder();
                        _differentialKeyRecoveryAttack = new Cipher3DifferentialKeyRecoveryAttack();
                        pathFinder.AttackSearchResultOccured += handleSearchresult;
                        pathFinder.ProgressChangedOccured += RefreshProgress;
                        Cipher3Configuration.PROBABILITYBOUNDBESTCHARACTERISTICSEARCH = settings.AbortingThresholdCharacteristicSearch;
                        Cipher3Configuration.PROBABILITYBOUNDDIFFERENTIALSEARCH = settings.AbortingThresholdDifferentialSearch;
                    }
                    break;
            }

            _nextRound.Reset();
            _activePresentation.sendDataEvent.Reset();
            _activePresentation.workDataEvent.Reset();

            //prepare thread to run
            ThreadStart tStart = new ThreadStart(ExecuteDifferentialAttack);
            _workerThread = new Thread(tStart)
            {
                Name = "DCA-PathFinder Workerthread",
                IsBackground = true
            };
        }

        private void UIRefreshProgress(object sender, ProgressEventArgs e)
        {
            _currentProgress = e.Increment;
            ProgressChanged(_currentProgress, _maxProgress);
        }

        private void RefreshProgress(object sender, ProgressEventArgs e)
        {
            _currentProgress += e.Increment;
            if (_currentProgress > 1.0)
            {
                _currentProgress = 1.0;
            }

            ProgressChanged(_currentProgress, _maxProgress);
        }

        /// <summary>
        /// handles messages to display in the protocol of the plugin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void displayMessage(object sender, MessageEventArgs e)
        {
            GuiLogMessage(e.message, NotificationLevel.Warning);
        }

        /// <summary>
        /// Executes the main work
        /// </summary>
        private void ExecuteDifferentialAttack()
        {
            switch (settings.CurrentAlgorithm)
            {
                case Algorithms.Cipher1:
                    {
                        Cipher1DifferentialKeyRecoveryAttack c1Attack =
                            _differentialKeyRecoveryAttack as Cipher1DifferentialKeyRecoveryAttack;

                        if (!settings.AutomaticMode)
                        {
                            //wait until pres has finished
                            _nextRound.WaitOne();

                            if (_stop)
                            {
                                return;
                            }

                            DifferentialAttackRoundConfiguration conf = new DifferentialAttackRoundConfiguration
                            {
                                SelectedAlgorithm = Algorithms.Cipher1,
                                ActiveSBoxes = new bool[] { true, true, true, true },
                                Round = 1,
                                AbortingPolicy = AbortingPolicy.GlobalMaximum,
                                SearchPolicy = SearchPolicy.FirstBestCharacteristicDepthSearch,
                                IsFirst = true,
                                IsLast = false,
                                IsBeforeLast = false
                            };

                            if (_stop)
                            {
                                return;
                            }

                            //write data to outputs
                            MessageCount = 1;
                            ExpectedDifferential = (new Random()).Next(0, ((int)Math.Pow(2, 16) - 1));
                            Path = SerializeConfiguration(conf);

                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                        }
                        else
                        {
                            //wait until pres has finished
                            _nextRound.WaitOne();

                            DifferentialAttackRoundConfiguration conf = new DifferentialAttackRoundConfiguration
                            {
                                SelectedAlgorithm = Algorithms.Cipher1,
                                ActiveSBoxes = new bool[] { true, true, true, true },
                                Round = 1,
                                AbortingPolicy = AbortingPolicy.GlobalMaximum,
                                SearchPolicy = SearchPolicy.FirstBestCharacteristicDepthSearch,
                                IsFirst = true,
                                IsLast = false,
                                IsBeforeLast = false
                            };

                            //write data to outputs
                            MessageCount = 1;
                            ExpectedDifferential = (new Random()).Next(0, ((int)Math.Pow(2, 16) - 1));
                            Path = SerializeConfiguration(conf);

                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                        }
                    }
                    break;
                case Algorithms.Cipher2:
                    {
                        List<Differential> diffList = pathFinder.CountDifferentialsSingleSBox();
                        DifferentialAttackRoundConfiguration conf;
                        Cipher2DifferentialKeyRecoveryAttack c2Attack = _differentialKeyRecoveryAttack as Cipher2DifferentialKeyRecoveryAttack;

                        //serialize data for offline search?
                        if (_serializeData)
                        {
                            Cipher2PathFinder c2PathFinder = pathFinder as Cipher2PathFinder;
                            c2PathFinder.threadCount = settings.ThreadCount;

                            for (int round = 3; round > 1; round--)
                            {
                                for (int i = 1; i < 16; i++)
                                {
                                    string binaryString = Convert.ToString(i, 2).PadLeft(4, '0');
                                    BitArray bitArray = new BitArray(binaryString.Length);
                                    for (int j = 0; j < bitArray.Length; j++)
                                    {
                                        bitArray[j] = (binaryString[j] == '1');
                                    }

                                    bool[] activeSBoxes = new bool[] { bitArray[3], bitArray[2], bitArray[1], bitArray[0] };

                                    /*
                                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                                    sw.Start();
                                    DifferentialAttackRoundConfiguration data = c2PathFinder.GenerateOfflineConfiguration(round, activeSBoxes, diffList, AbortingPolicy.Threshold);
                                    sw.Stop();
                                    data.SelectedAlgorithm = Algorithms.Cipher2;


                                        SaveConfigurationToDisk(data, "Cipher2_BestCharacteristicDepthSearch_globalThreshold_R" + round + "_SBoxes" + binaryString + "_Reduced.json");

                                        //to benchmark the results
                                        using (StreamWriter writer = File.AppendText("Results_BestCharacteristicDepthSearch_globalThreshold.txt"))
                                        {
                                            writer.WriteLine("Cipher2_BestCharacteristicDepthSearch_globalThreshold_R" + round + "_SBoxes" + binaryString + ". Time: " + sw.ElapsedMilliseconds / 1000 + " seconds");
                                            writer.WriteLine("----------------");
                                        }

                                        sw = new System.Diagnostics.Stopwatch();
                                        sw.Start();
                                        data = c2PathFinder.GenerateOfflineConfiguration(round, activeSBoxes, diffList, AbortingPolicy.GlobalMaximum);
                                        sw.Stop();
                                        data.SelectedAlgorithm = Algorithms.Cipher2;

                                        SaveConfigurationToDisk(data, "Cipher2_BestCharacteristicDepthSearch_globalMaximum" + round + "_SBoxes" + binaryString + "_Reduced.json");

                                        using (StreamWriter writer = File.AppendText("Results_BestCharacteristicDepthSearch_globalMaximum.txt"))
                                        {
                                            writer.WriteLine("Cipher2_BestCharacteristicDepthSearch_globalMaximum_R" + round + "_SBoxes" + binaryString + ". Time: " + sw.ElapsedMilliseconds / 1000 + " seconds");
                                            writer.WriteLine("----------------");
                                        }
                                        */

                                    /* */
                                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                                    sw.Start();
                                    DifferentialAttackRoundConfiguration data = c2PathFinder.GenerateOfflineConfiguration(round, activeSBoxes, diffList, AbortingPolicy.Threshold);
                                    sw.Stop();
                                    data.SelectedAlgorithm = Algorithms.Cipher2;

                                    SaveConfigurationToDisk(data, "Cipher2_BestCharacteristicHeuristic_R" + round + "_SBoxes" + binaryString + "_Reduced.json");

                                    using (StreamWriter writer = File.AppendText("Results_BestCharacteristicHeuristic.txt"))
                                    {
                                        writer.WriteLine("Cipher2_BestCharacteristicHeuristic_R" + round + "_SBoxes" + binaryString + ". Time: " + sw.ElapsedMilliseconds / 1000 + " seconds");
                                        writer.WriteLine("----------------");
                                    }

                                }
                            }
                        }

                        if (!settings.AutomaticMode)
                        {
                            Cipher2PathFinder c2PathFinder = pathFinder as Cipher2PathFinder;

                            if (!settings.PresentationMode)
                            {
                                c2PathFinder._maxProgress = 1.0;
                            }
                            else
                            {
                                c2PathFinder._maxProgress = 0.5;
                            }

                            c2PathFinder.threadCount = settings.ThreadCount;
                            bool firstIteration = true;

                            //break round 3
                            while (!c2Attack.recoveredSubkey3)
                            {
                                if (!firstIteration)
                                {
                                    c2PathFinder._maxProgress = 1.0;
                                    _currentProgress = 0;
                                }

                                _activePresentation.workDataEvent.WaitOne();

                                _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                                    (SendOrPostCallback)delegate { _activePresentation.UIProgressRefresh = false; }, null);

                                if (_stop)
                                {
                                    return;
                                }

                                //set data to state object and adjust data in the UI for next round
                                _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                               {
                                   if (_activePresentation.SBoxesCurrentAttack[0] &&
                                       !_activePresentation.SBoxesAlreadyAttacked[0])
                                   {
                                       _activePresentation.SBoxesAlreadyAttacked[0] = true;
                                   }

                                   if (_activePresentation.SBoxesCurrentAttack[1] &&
                                       !_activePresentation.SBoxesAlreadyAttacked[1])
                                   {
                                       _activePresentation.SBoxesAlreadyAttacked[1] = true;
                                   }

                                   if (_activePresentation.SBoxesCurrentAttack[2] &&
                                       !_activePresentation.SBoxesAlreadyAttacked[2])
                                   {
                                       _activePresentation.SBoxesAlreadyAttacked[2] = true;
                                   }

                                   if (_activePresentation.SBoxesCurrentAttack[3] &&
                                       !_activePresentation.SBoxesAlreadyAttacked[3])
                                   {
                                       _activePresentation.SBoxesAlreadyAttacked[3] = true;
                                   }
                               }, null);

                                //set the attack state
                                if (!c2Attack.attackedSBoxesRound3[0] && _activePresentation.SBoxesCurrentAttack[0])
                                {
                                    c2Attack.attackedSBoxesRound3[0] = true;
                                }

                                if (!c2Attack.attackedSBoxesRound3[1] && _activePresentation.SBoxesCurrentAttack[1])
                                {
                                    c2Attack.attackedSBoxesRound3[1] = true;
                                }

                                if (!c2Attack.attackedSBoxesRound3[2] && _activePresentation.SBoxesCurrentAttack[2])
                                {
                                    c2Attack.attackedSBoxesRound3[2] = true;
                                }

                                if (!c2Attack.attackedSBoxesRound3[3] && _activePresentation.SBoxesCurrentAttack[3])
                                {
                                    c2Attack.attackedSBoxesRound3[3] = true;
                                }

                                //check if all SBoxes are attacked
                                if (c2Attack.attackedSBoxesRound3[0] && c2Attack.attackedSBoxesRound3[1] &&
                                    c2Attack.attackedSBoxesRound3[2] && c2Attack.attackedSBoxesRound3[3])
                                {
                                    c2Attack.recoveredSubkey3 = true;
                                }

                                try
                                {
                                    conf = pathFinder.GenerateConfigurationAttack(3,
                                        _activePresentation.SBoxesCurrentAttack, settings.UseOfflinePaths,
                                        settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                    //check if there is a result
                                    if (conf != null && conf.Characteristics.Count == 0)
                                    {
                                        GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                        _currentProgress = 1.0;
                                        ProgressChanged(_currentProgress, _maxProgress);
                                        return;
                                    }

                                    if (_stop)
                                    {
                                        return;
                                    }

                                    conf.SelectedAlgorithm = Algorithms.Cipher2;
                                    conf.IsFirst = false;
                                    conf.IsBeforeLast = false;
                                    conf.IsLast = true;

                                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                                        (SendOrPostCallback)delegate
                                       {
                                            _activePresentation.IsNextPossible = true;
                                            _activePresentation.ArrowNextOpacity = 1.0;
                                            _activePresentation.HighlightDispatcher.Start();
                                            _activePresentation.probability = string.Format("{0:0.0000}", conf.Probability);
                                            _activePresentation.inputDifference = Convert.ToString(conf.InputDifference, 2)
                                                .PadLeft(16, '0').Insert(8, " ");
                                            _activePresentation.expectedDifference = Convert
                                                .ToString(conf.ExpectedDifference, 2).PadLeft(16, '0').Insert(8, " ");
                                        }, null);

                                    if (_stop)
                                    {
                                        return;
                                    }

                                    _nextRound.WaitOne();

                                    if (_stop)
                                    {
                                        return;
                                    }

                                    //write data to outputs
                                    MessageCount = settings.ChosenMessagePairsCount;
                                    ExpectedDifferential = conf.InputDifference;
                                    Path = SerializeConfiguration(conf);
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
                                        if (c2PathFinder.Cts != null)
                                        {
                                            c2PathFinder.Cts.Dispose();
                                            c2PathFinder.Cts = new CancellationTokenSource();
                                        }
                                    }
                                    finally
                                    {
                                        _semaphoreSlim.Release();
                                    }
                                }

                                firstIteration = false;

                                _currentProgress = 1.0;
                                ProgressChanged(_currentProgress, _maxProgress);
                            }

                            //reset SBoxes which are already attacked
                            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                                (SendOrPostCallback)delegate { _activePresentation.SBoxesAlreadyAttacked = new bool[4]; },
                                null);

                            //break round 2
                            while (!c2Attack.recoveredSubkey2)
                            {
                                _activePresentation.workDataEvent.WaitOne();
                                _currentProgress = 0;

                                if (_stop)
                                {
                                    return;
                                }

                                //set data to state object and adjust data in the UI for next round
                                _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                               {
                                   if (_activePresentation.SBoxesCurrentAttack[0] &&
                                       !_activePresentation.SBoxesAlreadyAttacked[0])
                                   {
                                       _activePresentation.SBoxesAlreadyAttacked[0] = true;
                                   }

                                   if (_activePresentation.SBoxesCurrentAttack[1] &&
                                       !_activePresentation.SBoxesAlreadyAttacked[1])
                                   {
                                       _activePresentation.SBoxesAlreadyAttacked[1] = true;
                                   }

                                   if (_activePresentation.SBoxesCurrentAttack[2] &&
                                       !_activePresentation.SBoxesAlreadyAttacked[2])
                                   {
                                       _activePresentation.SBoxesAlreadyAttacked[2] = true;
                                   }

                                   if (_activePresentation.SBoxesCurrentAttack[3] &&
                                       !_activePresentation.SBoxesAlreadyAttacked[3])
                                   {
                                       _activePresentation.SBoxesAlreadyAttacked[3] = true;
                                   }
                               }, null);

                                //set the attack state
                                if (!c2Attack.attackedSBoxesRound2[0] && _activePresentation.SBoxesCurrentAttack[0])
                                {
                                    c2Attack.attackedSBoxesRound2[0] = true;
                                }

                                if (!c2Attack.attackedSBoxesRound2[1] && _activePresentation.SBoxesCurrentAttack[1])
                                {
                                    c2Attack.attackedSBoxesRound2[1] = true;
                                }

                                if (!c2Attack.attackedSBoxesRound2[2] && _activePresentation.SBoxesCurrentAttack[2])
                                {
                                    c2Attack.attackedSBoxesRound2[2] = true;
                                }

                                if (!c2Attack.attackedSBoxesRound2[3] && _activePresentation.SBoxesCurrentAttack[3])
                                {
                                    c2Attack.attackedSBoxesRound2[3] = true;
                                }

                                //check if all SBoxes are attacked
                                if (c2Attack.attackedSBoxesRound2[0] && c2Attack.attackedSBoxesRound2[1] &&
                                    c2Attack.attackedSBoxesRound2[2] && c2Attack.attackedSBoxesRound2[3])
                                {
                                    c2Attack.recoveredSubkey2 = true;
                                }


                                try
                                {
                                    conf = pathFinder.GenerateConfigurationAttack(2,
                                        _activePresentation.SBoxesCurrentAttack, settings.UseOfflinePaths,
                                        settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                    //check if there is a result
                                    if (conf != null && conf.Characteristics.Count == 0)
                                    {
                                        GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                        _currentProgress = 1.0;
                                        ProgressChanged(_currentProgress, _maxProgress);
                                        return;
                                    }

                                    if (_stop)
                                    {
                                        return;
                                    }

                                    conf.SelectedAlgorithm = Algorithms.Cipher2;
                                    conf.IsFirst = false;
                                    conf.IsBeforeLast = true;
                                    conf.IsLast = false;

                                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                                        (SendOrPostCallback)delegate
                                       {
                                            _activePresentation.IsNextPossible = true;
                                            _activePresentation.ArrowNextOpacity = 1.0;
                                            _activePresentation.HighlightDispatcher.Start();
                                            _activePresentation.probability = string.Format("{0:0.0000}", conf.Probability);
                                            _activePresentation.inputDifference = Convert.ToString(conf.InputDifference, 2)
                                                .PadLeft(16, '0').Insert(8, " ");
                                            _activePresentation.expectedDifference = Convert
                                                .ToString(conf.ExpectedDifference, 2).PadLeft(16, '0').Insert(8, " ");
                                        }, null);

                                    _nextRound.WaitOne();

                                    //write data to outputs
                                    MessageCount = settings.ChosenMessagePairsCount;
                                    ExpectedDifferential = conf.InputDifference;
                                    Path = SerializeConfiguration(conf);
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
                                        if (c2PathFinder.Cts != null)
                                        {
                                            c2PathFinder.Cts.Dispose();
                                            c2PathFinder.Cts = new CancellationTokenSource();
                                        }
                                    }
                                    finally
                                    {
                                        _semaphoreSlim.Release();
                                    }
                                }


                                _currentProgress = 1.0;
                                ProgressChanged(_currentProgress, _maxProgress);
                            }

                            if (_stop)
                            {
                                return;
                            }

                            _nextRound.WaitOne();

                            _currentProgress = 0;

                            if (_stop)
                            {
                                return;
                            }

                            conf = new DifferentialAttackRoundConfiguration
                            {
                                Round = 1,
                                SelectedAlgorithm = Algorithms.Cipher2,
                                IsFirst = true,
                                IsBeforeLast = false,
                                IsLast = false,

                                //set all SBoxes active as convention
                                ActiveSBoxes = new bool[] { true, true, true, true }
                            };

                            //write data to outputs
                            MessageCount = 1;
                            ExpectedDifferential = (new Random()).Next(0, ((int)Math.Pow(2, 16) - 1));
                            Path = SerializeConfiguration(conf);

                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                        }
                        else
                        {
                            Cipher2PathFinder c2PathFinder = pathFinder as Cipher2PathFinder;
                            c2PathFinder._maxProgress = 1.0;
                            c2PathFinder.threadCount = settings.ThreadCount;
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                //round 3 run 1
                                bool[] sboxesToAttack = new bool[] { false, false, false, true };

                                conf = pathFinder.GenerateConfigurationAttack(3, sboxesToAttack, settings.UseOfflinePaths, settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                //check if there is a result
                                if (conf != null && conf.Characteristics.Count == 0)
                                {
                                    GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                    _currentProgress = 1.0;
                                    ProgressChanged(_currentProgress, _maxProgress);
                                    return;
                                }

                                if (_stop)
                                {
                                    return;
                                }

                                conf.SelectedAlgorithm = Algorithms.Cipher2;
                                conf.IsFirst = false;
                                conf.IsBeforeLast = false;
                                conf.IsLast = true;

                                //write data to outputs
                                MessageCount = settings.ChosenMessagePairsCount;
                                ExpectedDifferential = conf.InputDifference;
                                Path = SerializeConfiguration(conf);
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
                                    if (c2PathFinder.Cts != null)
                                    {
                                        c2PathFinder.Cts.Dispose();
                                        c2PathFinder.Cts = new CancellationTokenSource();
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            //round 3 run 2
                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                bool[] sboxesToAttack = new bool[] { false, false, true, false };

                                conf = pathFinder.GenerateConfigurationAttack(3, sboxesToAttack, settings.UseOfflinePaths, settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                //check if there is a result
                                if (conf != null && conf.Characteristics.Count == 0)
                                {
                                    GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                    _currentProgress = 1.0;
                                    ProgressChanged(_currentProgress, _maxProgress);
                                    return;
                                }

                                if (_stop)
                                {
                                    return;
                                }

                                conf.SelectedAlgorithm = Algorithms.Cipher2;
                                conf.IsFirst = false;
                                conf.IsBeforeLast = false;
                                conf.IsLast = true;

                                //write data to outputs
                                MessageCount = settings.ChosenMessagePairsCount;
                                ExpectedDifferential = conf.InputDifference;
                                Path = SerializeConfiguration(conf);
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
                                    if (c2PathFinder.Cts != null)
                                    {
                                        c2PathFinder.Cts.Dispose();
                                        c2PathFinder.Cts = new CancellationTokenSource();
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            //round 3 run 3
                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                bool[] sboxesToAttack = new bool[] { false, true, false, false };

                                conf = pathFinder.GenerateConfigurationAttack(3, sboxesToAttack, settings.UseOfflinePaths, settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                //check if there is a result
                                if (conf != null && conf.Characteristics.Count == 0)
                                {
                                    GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                    _currentProgress = 1.0;
                                    ProgressChanged(_currentProgress, _maxProgress);
                                    return;
                                }

                                if (_stop)
                                {
                                    return;
                                }

                                conf.SelectedAlgorithm = Algorithms.Cipher2;
                                conf.IsFirst = false;
                                conf.IsBeforeLast = false;
                                conf.IsLast = true;

                                //write data to outputs
                                MessageCount = settings.ChosenMessagePairsCount;
                                ExpectedDifferential = conf.InputDifference;
                                Path = SerializeConfiguration(conf);
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
                                    if (c2PathFinder.Cts != null)
                                    {
                                        c2PathFinder.Cts.Dispose();
                                        c2PathFinder.Cts = new CancellationTokenSource();
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            //round 3 run 4
                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                bool[] sboxesToAttack = new bool[] { true, false, false, false };

                                conf = pathFinder.GenerateConfigurationAttack(3, sboxesToAttack, settings.UseOfflinePaths, settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                //check if there is a result
                                if (conf != null && conf.Characteristics.Count == 0)
                                {
                                    GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                    _currentProgress = 1.0;
                                    ProgressChanged(_currentProgress, _maxProgress);
                                    return;
                                }

                                if (_stop)
                                {
                                    return;
                                }

                                conf.SelectedAlgorithm = Algorithms.Cipher2;
                                conf.IsFirst = false;
                                conf.IsBeforeLast = false;
                                conf.IsLast = true;

                                //write data to outputs
                                MessageCount = settings.ChosenMessagePairsCount;
                                ExpectedDifferential = conf.InputDifference;
                                Path = SerializeConfiguration(conf);
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
                                    if (c2PathFinder.Cts != null)
                                    {
                                        c2PathFinder.Cts.Dispose();
                                        c2PathFinder.Cts = new CancellationTokenSource();
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            //round 2 run 1
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                bool[] sboxesToAttack = new bool[] { false, false, false, true };

                                conf = pathFinder.GenerateConfigurationAttack(2, sboxesToAttack, settings.UseOfflinePaths, settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                //check if there is a result
                                if (conf != null && conf.Characteristics.Count == 0)
                                {
                                    GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                    _currentProgress = 1.0;
                                    ProgressChanged(_currentProgress, _maxProgress);
                                    return;
                                }

                                if (_stop)
                                {
                                    return;
                                }

                                conf.SelectedAlgorithm = Algorithms.Cipher2;
                                conf.IsFirst = false;
                                conf.IsBeforeLast = true;
                                conf.IsLast = false;

                                //write data to outputs
                                MessageCount = settings.ChosenMessagePairsCount;
                                ExpectedDifferential = conf.InputDifference;
                                Path = SerializeConfiguration(conf);
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
                                    if (c2PathFinder.Cts != null)
                                    {
                                        c2PathFinder.Cts.Dispose();
                                        c2PathFinder.Cts = new CancellationTokenSource();
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            //round 2 run 2
                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                bool[] sboxesToAttack = new bool[] { false, false, true, false };

                                conf = pathFinder.GenerateConfigurationAttack(2, sboxesToAttack, settings.UseOfflinePaths, settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                //check if there is a result
                                if (conf != null && conf.Characteristics.Count == 0)
                                {
                                    GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                    _currentProgress = 1.0;
                                    ProgressChanged(_currentProgress, _maxProgress);
                                    return;
                                }

                                if (_stop)
                                {
                                    return;
                                }

                                conf.SelectedAlgorithm = Algorithms.Cipher2;
                                conf.IsFirst = false;
                                conf.IsBeforeLast = true;
                                conf.IsLast = false;

                                //write data to outputs
                                MessageCount = settings.ChosenMessagePairsCount;
                                ExpectedDifferential = conf.InputDifference;
                                Path = SerializeConfiguration(conf);
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
                                    if (c2PathFinder.Cts != null)
                                    {
                                        c2PathFinder.Cts.Dispose();
                                        c2PathFinder.Cts = new CancellationTokenSource();
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            //round 2 run 3
                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                bool[] sboxesToAttack = new bool[] { false, true, false, false };

                                conf = pathFinder.GenerateConfigurationAttack(2, sboxesToAttack, settings.UseOfflinePaths, settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                //check if there is a result
                                if (conf != null && conf.Characteristics.Count == 0)
                                {
                                    GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                    _currentProgress = 1.0;
                                    ProgressChanged(_currentProgress, _maxProgress);
                                    return;
                                }

                                if (_stop)
                                {
                                    return;
                                }

                                conf.SelectedAlgorithm = Algorithms.Cipher2;
                                conf.IsFirst = false;
                                conf.IsBeforeLast = true;
                                conf.IsLast = false;

                                //write data to outputs
                                MessageCount = settings.ChosenMessagePairsCount;
                                ExpectedDifferential = conf.InputDifference;
                                Path = SerializeConfiguration(conf);
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
                                    if (c2PathFinder.Cts != null)
                                    {
                                        c2PathFinder.Cts.Dispose();
                                        c2PathFinder.Cts = new CancellationTokenSource();
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            //round 2 run 4
                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                bool[] sboxesToAttack = new bool[] { true, false, false, false };

                                conf = pathFinder.GenerateConfigurationAttack(2, sboxesToAttack, settings.UseOfflinePaths, settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                //check if there is a result
                                if (conf != null && conf.Characteristics.Count == 0)
                                {
                                    GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                    _currentProgress = 1.0;
                                    ProgressChanged(_currentProgress, _maxProgress);
                                    return;
                                }

                                if (_stop)
                                {
                                    return;
                                }

                                conf.SelectedAlgorithm = Algorithms.Cipher2;
                                conf.IsFirst = false;
                                conf.IsBeforeLast = true;
                                conf.IsLast = false;

                                //write data to outputs
                                MessageCount = settings.ChosenMessagePairsCount;
                                ExpectedDifferential = conf.InputDifference;
                                Path = SerializeConfiguration(conf);
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
                                    if (c2PathFinder.Cts != null)
                                    {
                                        c2PathFinder.Cts.Dispose();
                                        c2PathFinder.Cts = new CancellationTokenSource();
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            //attack last round
                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            conf = new DifferentialAttackRoundConfiguration
                            {
                                Round = 1,
                                SelectedAlgorithm = Algorithms.Cipher2,
                                IsFirst = true,
                                IsBeforeLast = false,
                                IsLast = false,

                                //set all SBoxes active as convention
                                ActiveSBoxes = new bool[] { true, true, true, true }
                            };

                            if (_stop)
                            {
                                return;
                            }

                            //write data to outputs
                            MessageCount = 1;
                            ExpectedDifferential = (new Random()).Next(0, ((int)Math.Pow(2, 16) - 1));
                            Path = SerializeConfiguration(conf);
                        }
                    }
                    break;
                case Algorithms.Cipher3:
                    {
                        List<Differential> diffList = pathFinder.CountDifferentialsSingleSBox();
                        DifferentialAttackRoundConfiguration conf;
                        Cipher3DifferentialKeyRecoveryAttack c3Attack =
                            _differentialKeyRecoveryAttack as Cipher3DifferentialKeyRecoveryAttack;

                        Cipher3PathFinder c3PathFinder = pathFinder as Cipher3PathFinder;

                        if (_serializeData)
                        {
                            c3PathFinder.threadCount = settings.ThreadCount;

                            for (int round = 5; round > 1; round--)
                            {
                                for (int i = 1; i < 16; i++)
                                {
                                    string binaryString = Convert.ToString(i, 2).PadLeft(4, '0');
                                    BitArray bitArray = new BitArray(binaryString.Length);
                                    for (int j = 0; j < bitArray.Length; j++)
                                    {
                                        bitArray[j] = (binaryString[j] == '1');
                                    }

                                    bool[] activeSBoxes = new bool[] { bitArray[3], bitArray[2], bitArray[1], bitArray[0] };
                                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                                    sw.Start();
                                    DifferentialAttackRoundConfiguration data = c3PathFinder.GenerateOfflineConfiguration(round, activeSBoxes, diffList, AbortingPolicy.Threshold);
                                    sw.Stop();
                                    data.SelectedAlgorithm = Algorithms.Cipher3;
                                    SaveConfigurationToDisk(data, "Cipher3_BestCharacteristicDepthSearch_globalThreshold_R" + round + "_SBoxes" + binaryString + "_Reduced.json");

                                    using (StreamWriter writer = File.AppendText("Results_BestCharacteristicDepthSearch_globalThreshold.txt"))
                                    {
                                        writer.WriteLine("Cipher3_BestCharacteristicDepthSearch_globalThreshold_R" + round + "_SBoxes" + binaryString + ". Time: " + sw.ElapsedMilliseconds / 1000 + " seconds");
                                        writer.WriteLine("----------------");
                                    }

                                    /*
                                    sw = new System.Diagnostics.Stopwatch();
                                    sw.Start();
                                    data = c3PathFinder.GenerateOfflineConfiguration(round, activeSBoxes, diffList, AbortingPolicy.GlobalMaximum);
                                    sw.Stop();
                                    data.SelectedAlgorithm = Algorithms.Cipher3;
                                    SaveConfigurationToDisk(data, "Cipher3_BestCharacteristicDepthSearch_globalMaximum_R" + round + "_SBoxes" + binaryString + "_Reduced.json");

                                    using (StreamWriter writer = File.AppendText("Results_BestCharacteristicDepthSearch_globalMaximum.txt"))
                                    {
                                        writer.WriteLine("Cipher3_BestCharacteristicDepthSearch_globalMaximum_R" + round + "_SBoxes" + binaryString + ". Time: " + sw.ElapsedMilliseconds / 1000 + " seconds");
                                        writer.WriteLine("----------------");
                                    }

                                    sw = new System.Diagnostics.Stopwatch();
                                    sw.Start();
                                    data = c3PathFinder.GenerateOfflineConfiguration(round, activeSBoxes, diffList, AbortingPolicy.Threshold);
                                    sw.Stop();
                                    data.SelectedAlgorithm = Algorithms.Cipher3;

                                    SaveConfigurationToDisk(data, "Cipher3_BestCharacteristicHeuristic_R" + round + "_SBoxes" + binaryString + "_Reduced.json");

                                    using (StreamWriter writer = File.AppendText("Results_BestCharacteristicHeuristic.txt"))
                                    {
                                        writer.WriteLine("Cipher3_BestCharacteristicHeuristic_R" + round + "_SBoxes" +binaryString + ". Time: " + sw.ElapsedMilliseconds / 1000 +" seconds");
                                        writer.WriteLine("----------------");
                                    }
                                    */
                                }
                            }
                        }

                        if (!settings.AutomaticMode)
                        {
                            if (!settings.PresentationMode)
                            {
                                c3PathFinder._maxProgress = 1.0;
                            }
                            else
                            {
                                c3PathFinder._maxProgress = 0.5;
                            }

                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            c3PathFinder.threadCount = settings.ThreadCount;
                            bool firstIteration = true;

                            //break round 5
                            while (!c3Attack.recoveredSubkey5)
                            {
                                if (!firstIteration)
                                {
                                    c3PathFinder._maxProgress = 1.0;
                                    _currentProgress = 0;
                                }

                                _activePresentation.workDataEvent.WaitOne();

                                if (_stop)
                                {
                                    return;
                                }

                                //set data to state object and adjust data in the UI for next round
                                _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                               {
                                   if (_activePresentation.SBoxesCurrentAttack[0] &&
                                       !_activePresentation.SBoxesAlreadyAttacked[0])
                                   {
                                       _activePresentation.SBoxesAlreadyAttacked[0] = true;
                                   }

                                   if (_activePresentation.SBoxesCurrentAttack[1] &&
                                       !_activePresentation.SBoxesAlreadyAttacked[1])
                                   {
                                       _activePresentation.SBoxesAlreadyAttacked[1] = true;
                                   }

                                   if (_activePresentation.SBoxesCurrentAttack[2] &&
                                       !_activePresentation.SBoxesAlreadyAttacked[2])
                                   {
                                       _activePresentation.SBoxesAlreadyAttacked[2] = true;
                                   }

                                   if (_activePresentation.SBoxesCurrentAttack[3] &&
                                       !_activePresentation.SBoxesAlreadyAttacked[3])
                                   {
                                       _activePresentation.SBoxesAlreadyAttacked[3] = true;
                                   }
                               }, null);

                                //set the attack state
                                if (!c3Attack.attackedSBoxesRound5[0] && _activePresentation.SBoxesCurrentAttack[0])
                                {
                                    c3Attack.attackedSBoxesRound5[0] = true;
                                }

                                if (!c3Attack.attackedSBoxesRound5[1] && _activePresentation.SBoxesCurrentAttack[1])
                                {
                                    c3Attack.attackedSBoxesRound5[1] = true;
                                }

                                if (!c3Attack.attackedSBoxesRound5[2] && _activePresentation.SBoxesCurrentAttack[2])
                                {
                                    c3Attack.attackedSBoxesRound5[2] = true;
                                }

                                if (!c3Attack.attackedSBoxesRound5[3] && _activePresentation.SBoxesCurrentAttack[3])
                                {
                                    c3Attack.attackedSBoxesRound5[3] = true;
                                }


                                //check if all SBoxes are attacked
                                if (c3Attack.attackedSBoxesRound5[0] && c3Attack.attackedSBoxesRound5[1] &&
                                    c3Attack.attackedSBoxesRound5[2] && c3Attack.attackedSBoxesRound5[3])
                                {
                                    c3Attack.recoveredSubkey5 = true;
                                }

                                try
                                {
                                    conf = pathFinder.GenerateConfigurationAttack(5,
                                        _activePresentation.SBoxesCurrentAttack, settings.UseOfflinePaths,
                                        settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                    //check if there is a result
                                    if (conf != null && conf.Characteristics.Count == 0)
                                    {
                                        GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                        _currentProgress = 1.0;
                                        ProgressChanged(_currentProgress, _maxProgress);
                                        return;
                                    }

                                    if (_stop)
                                    {
                                        return;
                                    }

                                    conf.SelectedAlgorithm = Algorithms.Cipher3;

                                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                                        (SendOrPostCallback)delegate
                                       {
                                            _activePresentation.IsNextPossible = true;
                                            _activePresentation.ArrowNextOpacity = 1.0;
                                            _activePresentation.HighlightDispatcher.Start();
                                            _activePresentation.probability = string.Format("{0:0.0000}", conf.Probability);
                                            _activePresentation.inputDifference = Convert.ToString(conf.InputDifference, 2)
                                                .PadLeft(16, '0').Insert(8, " ");
                                            _activePresentation.expectedDifference = Convert
                                                .ToString(conf.ExpectedDifference, 2).PadLeft(16, '0').Insert(8, " ");
                                        }, null);

                                    _nextRound.WaitOne();

                                    if (_stop)
                                    {
                                        return;
                                    }

                                    //write data to outputs
                                    MessageCount = settings.ChosenMessagePairsCount;
                                    ExpectedDifferential = conf.InputDifference;
                                    Path = SerializeConfiguration(conf);
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
                                        if (c3PathFinder.Cts != null)
                                        {
                                            c3PathFinder.Cts.Dispose();
                                            c3PathFinder.Cts = new CancellationTokenSource();
                                        }
                                    }
                                    finally
                                    {
                                        _semaphoreSlim.Release();
                                    }
                                }

                                firstIteration = false;
                                _currentProgress = 1.0;
                                ProgressChanged(_currentProgress, _maxProgress);

                                if (_stop)
                                {
                                    return;
                                }
                            }

                            //reset SBoxes which are already attacked
                            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                                (SendOrPostCallback)delegate { _activePresentation.SBoxesAlreadyAttacked = new bool[4]; },
                                null);

                            //break round 4
                            while (!c3Attack.recoveredSubkey4)
                            {
                                _activePresentation.workDataEvent.WaitOne();
                                _currentProgress = 0.0;
                                ProgressChanged(_currentProgress, _maxProgress);

                                if (_stop)
                                {
                                    return;
                                }

                                //set data to state object and adjust data in the UI for next round
                                _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                               {
                                   if (_activePresentation.SBoxesCurrentAttack[0] &&
                                       !_activePresentation.SBoxesAlreadyAttacked[0])
                                   {
                                       _activePresentation.SBoxesAlreadyAttacked[0] = true;
                                   }

                                   if (_activePresentation.SBoxesCurrentAttack[1] &&
                                       !_activePresentation.SBoxesAlreadyAttacked[1])
                                   {
                                       _activePresentation.SBoxesAlreadyAttacked[1] = true;
                                   }

                                   if (_activePresentation.SBoxesCurrentAttack[2] &&
                                       !_activePresentation.SBoxesAlreadyAttacked[2])
                                   {
                                       _activePresentation.SBoxesAlreadyAttacked[2] = true;
                                   }

                                   if (_activePresentation.SBoxesCurrentAttack[3] &&
                                       !_activePresentation.SBoxesAlreadyAttacked[3])
                                   {
                                       _activePresentation.SBoxesAlreadyAttacked[3] = true;
                                   }
                               }, null);

                                //set the attack state
                                if (!c3Attack.attackedSBoxesRound4[0] && _activePresentation.SBoxesCurrentAttack[0])
                                {
                                    c3Attack.attackedSBoxesRound4[0] = true;
                                }

                                if (!c3Attack.attackedSBoxesRound4[1] && _activePresentation.SBoxesCurrentAttack[1])
                                {
                                    c3Attack.attackedSBoxesRound4[1] = true;
                                }

                                if (!c3Attack.attackedSBoxesRound4[2] && _activePresentation.SBoxesCurrentAttack[2])
                                {
                                    c3Attack.attackedSBoxesRound4[2] = true;
                                }

                                if (!c3Attack.attackedSBoxesRound4[3] && _activePresentation.SBoxesCurrentAttack[3])
                                {
                                    c3Attack.attackedSBoxesRound4[3] = true;
                                }

                                //check if all SBoxes are attacked
                                if (c3Attack.attackedSBoxesRound4[0] && c3Attack.attackedSBoxesRound4[1] &&
                                    c3Attack.attackedSBoxesRound4[2] && c3Attack.attackedSBoxesRound4[3])
                                {
                                    c3Attack.recoveredSubkey4 = true;
                                }

                                try
                                {
                                    conf = pathFinder.GenerateConfigurationAttack(4,
                                        _activePresentation.SBoxesCurrentAttack, settings.UseOfflinePaths,
                                        settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                    //check if there is a result
                                    if (conf != null && conf.Characteristics.Count == 0)
                                    {
                                        GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                        _currentProgress = 1.0;
                                        ProgressChanged(_currentProgress, _maxProgress);
                                        return;
                                    }

                                    if (_stop)
                                    {
                                        return;
                                    }

                                    conf.SelectedAlgorithm = Algorithms.Cipher3;

                                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                                        (SendOrPostCallback)delegate
                                       {
                                            _activePresentation.IsNextPossible = true;
                                            _activePresentation.ArrowNextOpacity = 1.0;
                                            _activePresentation.HighlightDispatcher.Start();
                                            _activePresentation.probability = string.Format("{0:0.0000}", conf.Probability);
                                            _activePresentation.inputDifference = Convert.ToString(conf.InputDifference, 2)
                                                .PadLeft(16, '0').Insert(8, " ");
                                            _activePresentation.expectedDifference = Convert
                                                .ToString(conf.ExpectedDifference, 2).PadLeft(16, '0').Insert(8, " ");
                                        }, null);

                                    _nextRound.WaitOne();

                                    if (_stop)
                                    {
                                        return;
                                    }

                                    //write data to outputs
                                    MessageCount = settings.ChosenMessagePairsCount;
                                    ExpectedDifferential = conf.InputDifference;
                                    Path = SerializeConfiguration(conf);
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
                                        if (c3PathFinder.Cts != null)
                                        {
                                            c3PathFinder.Cts.Dispose();
                                            c3PathFinder.Cts = new CancellationTokenSource();
                                        }
                                    }
                                    finally
                                    {
                                        _semaphoreSlim.Release();
                                    }
                                }

                                _currentProgress = 1.0;
                                ProgressChanged(_currentProgress, _maxProgress);

                                if (_stop)
                                {
                                    return;
                                }
                            }

                            //reset SBoxes which are already attacked
                            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                                (SendOrPostCallback)delegate { _activePresentation.SBoxesAlreadyAttacked = new bool[4]; },
                                null);

                            //break round 3
                            while (!c3Attack.recoveredSubkey3)
                            {
                                _activePresentation.workDataEvent.WaitOne();
                                _currentProgress = 0.0;
                                ProgressChanged(_currentProgress, _maxProgress);

                                if (_stop)
                                {
                                    return;
                                }

                                //set data to state object and adjust data in the UI for next round
                                _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                               {
                                   if (_activePresentation.SBoxesCurrentAttack[0] &&
                                       !_activePresentation.SBoxesAlreadyAttacked[0])
                                   {
                                       _activePresentation.SBoxesAlreadyAttacked[0] = true;
                                   }

                                   if (_activePresentation.SBoxesCurrentAttack[1] &&
                                       !_activePresentation.SBoxesAlreadyAttacked[1])
                                   {
                                       _activePresentation.SBoxesAlreadyAttacked[1] = true;
                                   }

                                   if (_activePresentation.SBoxesCurrentAttack[2] &&
                                       !_activePresentation.SBoxesAlreadyAttacked[2])
                                   {
                                       _activePresentation.SBoxesAlreadyAttacked[2] = true;
                                   }

                                   if (_activePresentation.SBoxesCurrentAttack[3] &&
                                       !_activePresentation.SBoxesAlreadyAttacked[3])
                                   {
                                       _activePresentation.SBoxesAlreadyAttacked[3] = true;
                                   }
                               }, null);

                                //set the attack state
                                if (!c3Attack.attackedSBoxesRound3[0] && _activePresentation.SBoxesCurrentAttack[0])
                                {
                                    c3Attack.attackedSBoxesRound3[0] = true;
                                }

                                if (!c3Attack.attackedSBoxesRound3[1] && _activePresentation.SBoxesCurrentAttack[1])
                                {
                                    c3Attack.attackedSBoxesRound3[1] = true;
                                }

                                if (!c3Attack.attackedSBoxesRound3[2] && _activePresentation.SBoxesCurrentAttack[2])
                                {
                                    c3Attack.attackedSBoxesRound3[2] = true;
                                }

                                if (!c3Attack.attackedSBoxesRound3[3] && _activePresentation.SBoxesCurrentAttack[3])
                                {
                                    c3Attack.attackedSBoxesRound3[3] = true;
                                }

                                //check if all SBoxes are attacked
                                if (c3Attack.attackedSBoxesRound3[0] && c3Attack.attackedSBoxesRound3[1] &&
                                    c3Attack.attackedSBoxesRound3[2] && c3Attack.attackedSBoxesRound3[3])
                                {
                                    c3Attack.recoveredSubkey3 = true;
                                }

                                try
                                {
                                    conf = pathFinder.GenerateConfigurationAttack(3,
                                        _activePresentation.SBoxesCurrentAttack, settings.UseOfflinePaths,
                                        settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                    //check if there is a result
                                    if (conf != null && conf.Characteristics.Count == 0)
                                    {
                                        GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                        _currentProgress = 1.0;
                                        ProgressChanged(_currentProgress, _maxProgress);
                                        return;
                                    }

                                    if (_stop)
                                    {
                                        return;
                                    }

                                    conf.SelectedAlgorithm = Algorithms.Cipher3;

                                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                                        (SendOrPostCallback)delegate
                                       {
                                            _activePresentation.IsNextPossible = true;
                                            _activePresentation.ArrowNextOpacity = 1.0;
                                            _activePresentation.HighlightDispatcher.Start();
                                            _activePresentation.probability = string.Format("{0:0.0000}", conf.Probability);
                                            _activePresentation.inputDifference = Convert.ToString(conf.InputDifference, 2)
                                                .PadLeft(16, '0').Insert(8, " ");
                                            _activePresentation.expectedDifference = Convert
                                                .ToString(conf.ExpectedDifference, 2).PadLeft(16, '0').Insert(8, " ");
                                        }, null);

                                    _nextRound.WaitOne();

                                    if (_stop)
                                    {
                                        return;
                                    }

                                    //write data to outputs
                                    MessageCount = settings.ChosenMessagePairsCount;
                                    ExpectedDifferential = conf.InputDifference;
                                    Path = SerializeConfiguration(conf);
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
                                        if (c3PathFinder.Cts != null)
                                        {
                                            c3PathFinder.Cts.Dispose();
                                            c3PathFinder.Cts = new CancellationTokenSource();
                                        }
                                    }
                                    finally
                                    {
                                        _semaphoreSlim.Release();
                                    }
                                }

                                _currentProgress = 1.0;
                                ProgressChanged(_currentProgress, _maxProgress);

                                if (_stop)
                                {
                                    return;
                                }
                            }

                            //reset SBoxes which are already attacked
                            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                                (SendOrPostCallback)delegate { _activePresentation.SBoxesAlreadyAttacked = new bool[4]; },
                                null);

                            while (!c3Attack.recoveredSubkey2)
                            {
                                _activePresentation.workDataEvent.WaitOne();
                                _currentProgress = 0.0;
                                ProgressChanged(_currentProgress, _maxProgress);

                                if (_stop)
                                {
                                    return;
                                }

                                //set data to state object and adjust data in the UI for next round
                                _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                               {
                                   if (_activePresentation.SBoxesCurrentAttack[0] &&
                                       !_activePresentation.SBoxesAlreadyAttacked[0])
                                   {
                                       _activePresentation.SBoxesAlreadyAttacked[0] = true;
                                   }

                                   if (_activePresentation.SBoxesCurrentAttack[1] &&
                                       !_activePresentation.SBoxesAlreadyAttacked[1])
                                   {
                                       _activePresentation.SBoxesAlreadyAttacked[1] = true;
                                   }

                                   if (_activePresentation.SBoxesCurrentAttack[2] &&
                                       !_activePresentation.SBoxesAlreadyAttacked[2])
                                   {
                                       _activePresentation.SBoxesAlreadyAttacked[2] = true;
                                   }

                                   if (_activePresentation.SBoxesCurrentAttack[3] &&
                                       !_activePresentation.SBoxesAlreadyAttacked[3])
                                   {
                                       _activePresentation.SBoxesAlreadyAttacked[3] = true;
                                   }
                               }, null);

                                //set the attack state
                                if (!c3Attack.attackedSBoxesRound2[0] && _activePresentation.SBoxesCurrentAttack[0])
                                {
                                    c3Attack.attackedSBoxesRound2[0] = true;
                                }

                                if (!c3Attack.attackedSBoxesRound2[1] && _activePresentation.SBoxesCurrentAttack[1])
                                {
                                    c3Attack.attackedSBoxesRound2[1] = true;
                                }

                                if (!c3Attack.attackedSBoxesRound2[2] && _activePresentation.SBoxesCurrentAttack[2])
                                {
                                    c3Attack.attackedSBoxesRound2[2] = true;
                                }

                                if (!c3Attack.attackedSBoxesRound2[3] && _activePresentation.SBoxesCurrentAttack[3])
                                {
                                    c3Attack.attackedSBoxesRound2[3] = true;
                                }

                                //check if all SBoxes are attacked
                                if (c3Attack.attackedSBoxesRound2[0] && c3Attack.attackedSBoxesRound2[1] &&
                                    c3Attack.attackedSBoxesRound2[2] && c3Attack.attackedSBoxesRound2[3])
                                {
                                    c3Attack.recoveredSubkey2 = true;
                                }

                                try
                                {
                                    conf = pathFinder.GenerateConfigurationAttack(2,
                                        _activePresentation.SBoxesCurrentAttack, settings.UseOfflinePaths,
                                        settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                    //check if there is a result
                                    if (conf != null && conf.Characteristics.Count == 0)
                                    {
                                        GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                        _currentProgress = 1.0;
                                        ProgressChanged(_currentProgress, _maxProgress);
                                        return;
                                    }

                                    if (_stop)
                                    {
                                        return;
                                    }

                                    conf.SelectedAlgorithm = Algorithms.Cipher3;

                                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                                        (SendOrPostCallback)delegate
                                       {
                                            _activePresentation.IsNextPossible = true;
                                            _activePresentation.ArrowNextOpacity = 1.0;
                                            _activePresentation.HighlightDispatcher.Start();
                                            _activePresentation.probability = string.Format("{0:0.0000}", conf.Probability);
                                            _activePresentation.inputDifference = Convert.ToString(conf.InputDifference, 2)
                                                .PadLeft(16, '0').Insert(8, " ");
                                            _activePresentation.expectedDifference = Convert
                                                .ToString(conf.ExpectedDifference, 2).PadLeft(16, '0').Insert(8, " ");
                                        }, null);

                                    _nextRound.WaitOne();

                                    if (_stop)
                                    {
                                        return;
                                    }

                                    //write data to outputs
                                    MessageCount = settings.ChosenMessagePairsCount;
                                    ExpectedDifferential = conf.InputDifference;
                                    Path = SerializeConfiguration(conf);
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
                                        if (c3PathFinder.Cts != null)
                                        {
                                            c3PathFinder.Cts.Dispose();
                                            c3PathFinder.Cts = new CancellationTokenSource();
                                        }
                                    }
                                    finally
                                    {
                                        _semaphoreSlim.Release();
                                    }
                                }

                                _currentProgress = 1.0;
                                ProgressChanged(_currentProgress, _maxProgress);

                                if (_stop)
                                {
                                    return;
                                }
                            }

                            if (_stop)
                            {
                                return;
                            }

                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            _nextRound.WaitOne();


                            if (_stop)
                            {
                                return;
                            }

                            conf = new DifferentialAttackRoundConfiguration
                            {
                                Round = 1,
                                SelectedAlgorithm = Algorithms.Cipher3,
                                IsFirst = true,
                                IsBeforeLast = false,
                                IsLast = false,

                                //set all SBoxes active as convention
                                ActiveSBoxes = new bool[] { true, true, true, true }
                            };

                            //write data to outputs
                            MessageCount = 1;
                            ExpectedDifferential = (new Random()).Next(0, ((int)Math.Pow(2, 16) - 1));
                            Path = SerializeConfiguration(conf);
                        }
                        else
                        {
                            c3PathFinder._maxProgress = 1.0;
                            c3PathFinder.threadCount = settings.ThreadCount;
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                //round 5 run 1
                                bool[] sboxesToAttack = new bool[] { false, false, false, true };

                                conf = pathFinder.GenerateConfigurationAttack(5, sboxesToAttack, settings.UseOfflinePaths, settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                //check if there is a result
                                if (conf != null && conf.Characteristics.Count == 0)
                                {
                                    GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                    _currentProgress = 1.0;
                                    ProgressChanged(_currentProgress, _maxProgress);
                                    return;
                                }

                                if (_stop)
                                {
                                    return;
                                }

                                conf.SelectedAlgorithm = Algorithms.Cipher3;
                                conf.IsFirst = false;
                                conf.IsBeforeLast = false;
                                conf.IsLast = true;

                                //write data to outputs
                                MessageCount = settings.ChosenMessagePairsCount;
                                ExpectedDifferential = conf.InputDifference;
                                Path = SerializeConfiguration(conf);
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
                                    if (c3PathFinder.Cts != null)
                                    {
                                        c3PathFinder.Cts.Dispose();
                                        c3PathFinder.Cts = new CancellationTokenSource();
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                //round 5 run 2
                                bool[] sboxesToAttack = new bool[] { false, false, true, false };

                                conf = pathFinder.GenerateConfigurationAttack(5, sboxesToAttack, settings.UseOfflinePaths, settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                //check if there is a result
                                if (conf != null && conf.Characteristics.Count == 0)
                                {
                                    GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                    _currentProgress = 1.0;
                                    ProgressChanged(_currentProgress, _maxProgress);
                                    return;
                                }

                                if (_stop)
                                {
                                    return;
                                }

                                conf.SelectedAlgorithm = Algorithms.Cipher3;
                                conf.IsFirst = false;
                                conf.IsBeforeLast = false;
                                conf.IsLast = true;

                                //write data to outputs
                                MessageCount = settings.ChosenMessagePairsCount;
                                ExpectedDifferential = conf.InputDifference;
                                Path = SerializeConfiguration(conf);
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
                                    if (c3PathFinder.Cts != null)
                                    {
                                        c3PathFinder.Cts.Dispose();
                                        c3PathFinder.Cts = new CancellationTokenSource();
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                //round 5 run 3
                                bool[] sboxesToAttack = new bool[] { false, true, false, false };

                                conf = pathFinder.GenerateConfigurationAttack(5, sboxesToAttack, settings.UseOfflinePaths, settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                //check if there is a result
                                if (conf != null && conf.Characteristics.Count == 0)
                                {
                                    GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                    _currentProgress = 1.0;
                                    ProgressChanged(_currentProgress, _maxProgress);
                                    return;
                                }

                                if (_stop)
                                {
                                    return;
                                }

                                conf.SelectedAlgorithm = Algorithms.Cipher3;
                                conf.IsFirst = false;
                                conf.IsBeforeLast = false;
                                conf.IsLast = true;

                                //write data to outputs
                                MessageCount = settings.ChosenMessagePairsCount;
                                ExpectedDifferential = conf.InputDifference;
                                Path = SerializeConfiguration(conf);
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
                                    if (c3PathFinder.Cts != null)
                                    {
                                        c3PathFinder.Cts.Dispose();
                                        c3PathFinder.Cts = new CancellationTokenSource();
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                //round 5 run 4
                                bool[] sboxesToAttack = new bool[] { true, false, false, false };

                                conf = pathFinder.GenerateConfigurationAttack(5, sboxesToAttack, settings.UseOfflinePaths, settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                //check if there is a result
                                if (conf != null && conf.Characteristics.Count == 0)
                                {
                                    GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                    _currentProgress = 1.0;
                                    ProgressChanged(_currentProgress, _maxProgress);
                                    return;
                                }

                                if (_stop)
                                {
                                    return;
                                }

                                conf.SelectedAlgorithm = Algorithms.Cipher3;
                                conf.IsFirst = false;
                                conf.IsBeforeLast = false;
                                conf.IsLast = true;

                                //write data to outputs
                                MessageCount = settings.ChosenMessagePairsCount;
                                ExpectedDifferential = conf.InputDifference;
                                Path = SerializeConfiguration(conf);
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
                                    if (c3PathFinder.Cts != null)
                                    {
                                        c3PathFinder.Cts.Dispose();
                                        c3PathFinder.Cts = new CancellationTokenSource();
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                //round 4 run 1
                                bool[] sboxesToAttack = new bool[] { false, false, false, true };

                                conf = pathFinder.GenerateConfigurationAttack(4, sboxesToAttack, settings.UseOfflinePaths, settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                //check if there is a result
                                if (conf != null && conf.Characteristics.Count == 0)
                                {
                                    GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                    _currentProgress = 1.0;
                                    ProgressChanged(_currentProgress, _maxProgress);
                                    return;
                                }

                                if (_stop)
                                {
                                    return;
                                }

                                conf.SelectedAlgorithm = Algorithms.Cipher3;
                                conf.IsFirst = false;
                                conf.IsBeforeLast = true;
                                conf.IsLast = false;

                                //write data to outputs
                                MessageCount = settings.ChosenMessagePairsCount;
                                ExpectedDifferential = conf.InputDifference;
                                Path = SerializeConfiguration(conf);
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
                                    if (c3PathFinder.Cts != null)
                                    {
                                        c3PathFinder.Cts.Dispose();
                                        c3PathFinder.Cts = new CancellationTokenSource();
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                //round 4 run 2
                                bool[] sboxesToAttack = new bool[] { false, false, true, false };

                                conf = pathFinder.GenerateConfigurationAttack(4, sboxesToAttack, settings.UseOfflinePaths, settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                //check if there is a result
                                if (conf != null && conf.Characteristics.Count == 0)
                                {
                                    GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                    _currentProgress = 1.0;
                                    ProgressChanged(_currentProgress, _maxProgress);
                                    return;
                                }

                                if (_stop)
                                {
                                    return;
                                }

                                conf.SelectedAlgorithm = Algorithms.Cipher3;
                                conf.IsFirst = false;
                                conf.IsBeforeLast = true;
                                conf.IsLast = false;

                                //write data to outputs
                                MessageCount = settings.ChosenMessagePairsCount;
                                ExpectedDifferential = conf.InputDifference;
                                Path = SerializeConfiguration(conf);
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
                                    if (c3PathFinder.Cts != null)
                                    {
                                        c3PathFinder.Cts.Dispose();
                                        c3PathFinder.Cts = new CancellationTokenSource();
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                //round 4 run 3
                                bool[] sboxesToAttack = new bool[] { false, true, false, false };

                                conf = pathFinder.GenerateConfigurationAttack(4, sboxesToAttack, settings.UseOfflinePaths, settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                //check if there is a result
                                if (conf != null && conf.Characteristics.Count == 0)
                                {
                                    GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                    _currentProgress = 1.0;
                                    ProgressChanged(_currentProgress, _maxProgress);
                                    return;
                                }

                                if (_stop)
                                {
                                    return;
                                }

                                conf.SelectedAlgorithm = Algorithms.Cipher3;
                                conf.IsFirst = false;
                                conf.IsBeforeLast = true;
                                conf.IsLast = false;

                                //write data to outputs
                                MessageCount = settings.ChosenMessagePairsCount;
                                ExpectedDifferential = conf.InputDifference;
                                Path = SerializeConfiguration(conf);
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
                                    if (c3PathFinder.Cts != null)
                                    {
                                        c3PathFinder.Cts.Dispose();
                                        c3PathFinder.Cts = new CancellationTokenSource();
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                //round 4 run 4
                                bool[] sboxesToAttack = new bool[] { true, false, false, false };

                                conf = pathFinder.GenerateConfigurationAttack(4, sboxesToAttack, settings.UseOfflinePaths, settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                //check if there is a result
                                if (conf != null && conf.Characteristics.Count == 0)
                                {
                                    GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                    _currentProgress = 1.0;
                                    ProgressChanged(_currentProgress, _maxProgress);
                                    return;
                                }

                                if (_stop)
                                {
                                    return;
                                }

                                conf.SelectedAlgorithm = Algorithms.Cipher3;
                                conf.IsFirst = false;
                                conf.IsBeforeLast = true;
                                conf.IsLast = false;

                                //write data to outputs
                                MessageCount = settings.ChosenMessagePairsCount;
                                ExpectedDifferential = conf.InputDifference;
                                Path = SerializeConfiguration(conf);
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
                                    if (c3PathFinder.Cts != null)
                                    {
                                        c3PathFinder.Cts.Dispose();
                                        c3PathFinder.Cts = new CancellationTokenSource();
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                //round 3 run 1
                                bool[] sboxesToAttack = new bool[] { false, false, false, true };

                                conf = pathFinder.GenerateConfigurationAttack(3, sboxesToAttack, settings.UseOfflinePaths, settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                //check if there is a result
                                if (conf != null && conf.Characteristics.Count == 0)
                                {
                                    GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                    _currentProgress = 1.0;
                                    ProgressChanged(_currentProgress, _maxProgress);
                                    return;
                                }

                                if (_stop)
                                {
                                    return;
                                }

                                conf.SelectedAlgorithm = Algorithms.Cipher3;
                                conf.IsFirst = false;
                                conf.IsBeforeLast = false;
                                conf.IsLast = false;

                                //write data to outputs
                                MessageCount = settings.ChosenMessagePairsCount;
                                ExpectedDifferential = conf.InputDifference;
                                Path = SerializeConfiguration(conf);
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
                                    if (c3PathFinder.Cts != null)
                                    {
                                        c3PathFinder.Cts.Dispose();
                                        c3PathFinder.Cts = new CancellationTokenSource();
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                //round 3 run 2
                                bool[] sboxesToAttack = new bool[] { false, false, true, false };

                                conf = pathFinder.GenerateConfigurationAttack(3, sboxesToAttack, settings.UseOfflinePaths, settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                //check if there is a result
                                if (conf != null && conf.Characteristics.Count == 0)
                                {
                                    GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                    _currentProgress = 1.0;
                                    ProgressChanged(_currentProgress, _maxProgress);
                                    return;
                                }

                                if (_stop)
                                {
                                    return;
                                }

                                conf.SelectedAlgorithm = Algorithms.Cipher3;
                                conf.IsFirst = false;
                                conf.IsBeforeLast = false;
                                conf.IsLast = false;

                                //write data to outputs
                                MessageCount = settings.ChosenMessagePairsCount;
                                ExpectedDifferential = conf.InputDifference;
                                Path = SerializeConfiguration(conf);
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
                                    if (c3PathFinder.Cts != null)
                                    {
                                        c3PathFinder.Cts.Dispose();
                                        c3PathFinder.Cts = new CancellationTokenSource();
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                //round 3 run 3
                                bool[] sboxesToAttack = new bool[] { false, true, false, false };

                                conf = pathFinder.GenerateConfigurationAttack(3, sboxesToAttack, settings.UseOfflinePaths, settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                //check if there is a result
                                if (conf != null && conf.Characteristics.Count == 0)
                                {
                                    GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                    _currentProgress = 1.0;
                                    ProgressChanged(_currentProgress, _maxProgress);
                                    return;
                                }

                                if (_stop)
                                {
                                    return;
                                }

                                conf.SelectedAlgorithm = Algorithms.Cipher3;
                                conf.IsFirst = false;
                                conf.IsBeforeLast = false;
                                conf.IsLast = false;

                                //write data to outputs
                                MessageCount = settings.ChosenMessagePairsCount;
                                ExpectedDifferential = conf.InputDifference;
                                Path = SerializeConfiguration(conf);
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
                                    if (c3PathFinder.Cts != null)
                                    {
                                        c3PathFinder.Cts.Dispose();
                                        c3PathFinder.Cts = new CancellationTokenSource();
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                //round 3 run 4
                                bool[] sboxesToAttack = new bool[] { true, false, false, false };

                                conf = pathFinder.GenerateConfigurationAttack(3, sboxesToAttack, settings.UseOfflinePaths, settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                //check if there is a result
                                if (conf != null && conf.Characteristics.Count == 0)
                                {
                                    GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                    _currentProgress = 1.0;
                                    ProgressChanged(_currentProgress, _maxProgress);
                                    return;
                                }

                                if (_stop)
                                {
                                    return;
                                }

                                conf.SelectedAlgorithm = Algorithms.Cipher3;
                                conf.IsFirst = false;
                                conf.IsBeforeLast = false;
                                conf.IsLast = false;

                                //write data to outputs
                                MessageCount = settings.ChosenMessagePairsCount;
                                ExpectedDifferential = conf.InputDifference;
                                Path = SerializeConfiguration(conf);
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
                                    if (c3PathFinder.Cts != null)
                                    {
                                        c3PathFinder.Cts.Dispose();
                                        c3PathFinder.Cts = new CancellationTokenSource();
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                //round 2 run 1
                                bool[] sboxesToAttack = new bool[] { false, false, false, true };

                                conf = pathFinder.GenerateConfigurationAttack(2, sboxesToAttack, settings.UseOfflinePaths, settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                //check if there is a result
                                if (conf != null && conf.Characteristics.Count == 0)
                                {
                                    GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                    _currentProgress = 1.0;
                                    ProgressChanged(_currentProgress, _maxProgress);
                                    return;
                                }

                                if (_stop)
                                {
                                    return;
                                }

                                conf.SelectedAlgorithm = Algorithms.Cipher3;
                                conf.IsFirst = false;
                                conf.IsBeforeLast = false;
                                conf.IsLast = false;

                                //write data to outputs
                                MessageCount = settings.ChosenMessagePairsCount;
                                ExpectedDifferential = conf.InputDifference;
                                Path = SerializeConfiguration(conf);
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
                                    if (c3PathFinder.Cts != null)
                                    {
                                        c3PathFinder.Cts.Dispose();
                                        c3PathFinder.Cts = new CancellationTokenSource();
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                //round 2 run 2
                                bool[] sboxesToAttack = new bool[] { false, false, true, false };

                                conf = pathFinder.GenerateConfigurationAttack(2, sboxesToAttack, settings.UseOfflinePaths, settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                //check if there is a result
                                if (conf != null && conf.Characteristics.Count == 0)
                                {
                                    GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                    _currentProgress = 1.0;
                                    ProgressChanged(_currentProgress, _maxProgress);
                                    return;
                                }

                                if (_stop)
                                {
                                    return;
                                }

                                conf.SelectedAlgorithm = Algorithms.Cipher3;
                                conf.IsFirst = false;
                                conf.IsBeforeLast = false;
                                conf.IsLast = false;

                                //write data to outputs
                                MessageCount = settings.ChosenMessagePairsCount;
                                ExpectedDifferential = conf.InputDifference;
                                Path = SerializeConfiguration(conf);
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
                                    if (c3PathFinder.Cts != null)
                                    {
                                        c3PathFinder.Cts.Dispose();
                                        c3PathFinder.Cts = new CancellationTokenSource();
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                //round 2 run 3
                                bool[] sboxesToAttack = new bool[] { false, true, false, false };

                                conf = pathFinder.GenerateConfigurationAttack(2, sboxesToAttack, settings.UseOfflinePaths, settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                //check if there is a result
                                if (conf != null && conf.Characteristics.Count == 0)
                                {
                                    GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                    _currentProgress = 1.0;
                                    ProgressChanged(_currentProgress, _maxProgress);
                                    return;
                                }

                                if (_stop)
                                {
                                    return;
                                }

                                conf.SelectedAlgorithm = Algorithms.Cipher3;
                                conf.IsFirst = false;
                                conf.IsBeforeLast = false;
                                conf.IsLast = false;

                                //write data to outputs
                                MessageCount = settings.ChosenMessagePairsCount;
                                ExpectedDifferential = conf.InputDifference;
                                Path = SerializeConfiguration(conf);
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
                                    if (c3PathFinder.Cts != null)
                                    {
                                        c3PathFinder.Cts.Dispose();
                                        c3PathFinder.Cts = new CancellationTokenSource();
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            try
                            {
                                //round 2 run 4
                                bool[] sboxesToAttack = new bool[] { true, false, false, false };

                                conf = pathFinder.GenerateConfigurationAttack(2, sboxesToAttack, settings.UseOfflinePaths, settings.CurrentAbortingPolicy, settings.CurrentSearchPolicy, diffList);

                                //check if there is a result
                                if (conf != null && conf.Characteristics.Count == 0)
                                {
                                    GuiLogMessage(Resources.NoCharacteristicFoundError, NotificationLevel.Warning);
                                    _currentProgress = 1.0;
                                    ProgressChanged(_currentProgress, _maxProgress);
                                    return;
                                }

                                if (_stop)
                                {
                                    return;
                                }

                                conf.SelectedAlgorithm = Algorithms.Cipher3;
                                conf.IsFirst = false;
                                conf.IsBeforeLast = false;
                                conf.IsLast = false;

                                //write data to outputs
                                MessageCount = settings.ChosenMessagePairsCount;
                                ExpectedDifferential = conf.InputDifference;
                                Path = SerializeConfiguration(conf);
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
                                    if (c3PathFinder.Cts != null)
                                    {
                                        c3PathFinder.Cts.Dispose();
                                        c3PathFinder.Cts = new CancellationTokenSource();
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }

                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                            _nextRound.WaitOne();
                            _currentProgress = 0.0;
                            ProgressChanged(_currentProgress, _maxProgress);

                            if (_stop)
                            {
                                return;
                            }

                            conf = new DifferentialAttackRoundConfiguration
                            {
                                Round = 1,
                                SelectedAlgorithm = Algorithms.Cipher3,
                                IsFirst = true,
                                IsBeforeLast = false,
                                IsLast = false,

                                //set all SBoxes active as convention
                                ActiveSBoxes = new bool[] { true, true, true, true }
                            };

                            if (_stop)
                            {
                                return;
                            }

                            //write data to outputs
                            MessageCount = 1;
                            ExpectedDifferential = (new Random()).Next(0, ((int)Math.Pow(2, 16) - 1));
                            Path = SerializeConfiguration(conf);

                            _currentProgress = 1.0;
                            ProgressChanged(_currentProgress, _maxProgress);
                        }
                    }
                    break;
            }

            ProgressChanged(1, 1);
        }

        /// <summary>
        /// handles the occurence of a new search result for the ui
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void handleSearchresult(object sender, SearchResult e)
        {
            if (!settings.AutomaticMode)
            {
                _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                    (SendOrPostCallback)delegate { _activePresentation.addSearchResultToUI(e); }, null);
            }
        }

        /// <summary>
        /// Serializes a configuration to send it as json string
        /// </summary>
        /// <param name="conf"></param>
        /// <returns></returns>
        private string SerializeConfiguration(DifferentialAttackRoundConfiguration conf)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            JsonSerializer serializer = new JsonSerializer
            {
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                Formatting = Newtonsoft.Json.Formatting.Indented
            };
            serializer.Serialize(sw, conf);

            return sb.ToString();
        }

        private void SaveConfigurationToDisk(DifferentialAttackRoundConfiguration data, string fileName)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            JsonSerializer serializer = new JsonSerializer
            {
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                Formatting = Newtonsoft.Json.Formatting.Indented
            };
            serializer.Serialize(sw, data);

            using (StreamWriter file = File.CreateText(fileName))
            {
                file.Write(sb.ToString());
            }
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            if ((_workerThread.ThreadState & ThreadState.Unstarted) == ThreadState.Unstarted)
            {
                _workerThread.Start();
            }

            //dispatch action {DEBUG}: show slide 17 to save time
            /*
            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
            {
                _activePresentation.StepCounter = 16;
                _activePresentation.SetupView();
            }, null);
            */

            //prepare handles for pausing the state machine
            WaitHandle[] waitHandles = new WaitHandle[]
            {
                _activePresentation.sendDataEvent
            };

            switch (settings.CurrentAlgorithm)
            {
                case Algorithms.Cipher1:
                    {
                        Cipher1DifferentialKeyRecoveryAttack c1Attack =
                            _differentialKeyRecoveryAttack as Cipher1DifferentialKeyRecoveryAttack;

                        //implementation of state machine runs as long as the presentation has not reached the end or workspace is stopping
                        if (!c1Attack.recoveredSubkey1)
                        {
                            if (!settings.AutomaticMode)
                            {
                                WaitHandle.WaitAny(waitHandles);
                            }
                        }

                        _nextRound.Set();
                    }
                    break;


                case Algorithms.Cipher2:
                    {
                        Cipher2DifferentialKeyRecoveryAttack c2Attack =
                            _differentialKeyRecoveryAttack as Cipher2DifferentialKeyRecoveryAttack;

                        //Attack Round 3
                        if (!c2Attack.recoveredSubkey3)
                        {
                            if (!firstIteration)
                            {
                                _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                               {
                                   if (settings.PresentationMode)
                                   {
                                       _activePresentation.StepCounter = 22;
                                   }
                                   else
                                   {
                                       _activePresentation.StepCounter = 1;
                                   }

                                   if (!settings.AutomaticMode)
                                   {
                                       _activePresentation.HighlightDispatcher.Start();
                                       _activePresentation.SetupView();
                                   }
                               }, null);
                            }

                            firstIteration = false;

                            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                                (SendOrPostCallback)delegate { _activePresentation.SBoxesCurrentAttack = new bool[4]; },
                                null);

                            if (!settings.AutomaticMode)
                            {
                                //wait until data is sent or workspace stopped
                                WaitHandle.WaitAny(waitHandles);
                            }

                            _nextRound.Set();
                        }
                        //Attack Round 2
                        else if (!c2Attack.recoveredSubkey2)
                        {
                            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                           {
                               if (settings.PresentationMode)
                               {
                                   _activePresentation.StepCounter = 25;
                               }
                               else
                               {
                                   _activePresentation.StepCounter = 4;
                               }

                               if (!settings.AutomaticMode)
                               {
                                   _activePresentation.HighlightDispatcher.Start();
                                   _activePresentation.SetupView();
                               }
                           }, null);

                            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                                (SendOrPostCallback)delegate { _activePresentation.SBoxesCurrentAttack = new bool[4]; },
                                null);

                            if (!settings.AutomaticMode)
                            {
                                //wait until data is sent or workspace stopped
                                WaitHandle.WaitAny(waitHandles);
                            }

                            _nextRound.Set();
                        }
                        else
                        {
                            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                           {
                               if (settings.PresentationMode)
                               {
                                   _activePresentation.StepCounter = 28;
                               }
                               else
                               {
                                   _activePresentation.StepCounter = 7;
                               }

                               if (!settings.AutomaticMode)
                               {
                                   _activePresentation.HighlightDispatcher.Start();
                                   _activePresentation.SetupView();
                               }
                           }, null);

                            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                                (SendOrPostCallback)delegate { _activePresentation.SBoxesCurrentAttack = new bool[4]; },
                                null);

                            if (!settings.AutomaticMode)
                            {
                                //wait until data is sent or workspace stopped
                                WaitHandle.WaitAny(waitHandles);
                            }

                            _nextRound.Set();
                        }
                    }
                    break;


                case Algorithms.Cipher3:
                    {
                        Cipher3DifferentialKeyRecoveryAttack c3Attack =
                            _differentialKeyRecoveryAttack as Cipher3DifferentialKeyRecoveryAttack;

                        //Attack Round 5
                        if (!c3Attack.recoveredSubkey5)
                        {
                            if (!firstIteration)
                            {
                                //set slide to show
                                _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                               {
                                   if (settings.PresentationMode)
                                   {
                                       _activePresentation.StepCounter = 15;
                                   }
                                   else
                                   {
                                       _activePresentation.StepCounter = 1;
                                   }

                                   if (!settings.AutomaticMode)
                                   {
                                       _activePresentation.HighlightDispatcher.Start();
                                       _activePresentation.SetupView();
                                   }
                               }, null);
                            }


                            firstIteration = false;

                            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                           {
                               _activePresentation.SBoxesCurrentAttack = new bool[4];
                           }, null);

                            //check mode
                            if (!settings.AutomaticMode)
                            {
                                //wait until data is sent or workspace stopped
                                WaitHandle.WaitAny(waitHandles);
                            }

                            _nextRound.Set();
                        }
                        else if (!c3Attack.recoveredSubkey4)
                        {
                            //set slide to show
                            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                           {
                               if (settings.PresentationMode)
                               {
                                   _activePresentation.StepCounter = 18;
                               }
                               else
                               {
                                   _activePresentation.StepCounter = 4;
                               }

                               if (!settings.AutomaticMode)
                               {
                                   _activePresentation.HighlightDispatcher.Start();
                                   _activePresentation.SetupView();
                               }
                           }, null);

                            //reset SBoxes to attack in the UI
                            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                                (SendOrPostCallback)delegate { _activePresentation.SBoxesCurrentAttack = new bool[4]; },
                                null);

                            //check mode
                            if (!settings.AutomaticMode)
                            {
                                //wait until data is sent or workspace stopped
                                WaitHandle.WaitAny(waitHandles);
                            }

                            _nextRound.Set();
                        }
                        else if (!c3Attack.recoveredSubkey3)
                        {
                            //set slide to show
                            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                           {
                               if (settings.PresentationMode)
                               {
                                   _activePresentation.StepCounter = 21;
                               }
                               else
                               {
                                   _activePresentation.StepCounter = 7;
                               }

                               if (!settings.AutomaticMode)
                               {
                                   _activePresentation.HighlightDispatcher.Start();
                                   _activePresentation.SetupView();
                               }
                           }, null);

                            //reset SBoxes to attack in the UI
                            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                                (SendOrPostCallback)delegate { _activePresentation.SBoxesCurrentAttack = new bool[4]; },
                                null);

                            //check mode
                            if (!settings.AutomaticMode)
                            {
                                //wait until data is sent or workspace stopped
                                WaitHandle.WaitAny(waitHandles);
                            }

                            _nextRound.Set();
                        }
                        else if (!c3Attack.recoveredSubkey2)
                        {
                            //set slide to show
                            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                           {
                               if (settings.PresentationMode)
                               {
                                   _activePresentation.StepCounter = 24;
                               }
                               else
                               {
                                   _activePresentation.StepCounter = 10;
                               }

                               if (!settings.AutomaticMode)
                               {
                                   _activePresentation.HighlightDispatcher.Start();
                                   _activePresentation.SetupView();
                               }
                           }, null);

                            //reset SBoxes to attack in the UI
                            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                                (SendOrPostCallback)delegate { _activePresentation.SBoxesCurrentAttack = new bool[4]; },
                                null);

                            //check mode
                            if (!settings.AutomaticMode)
                            {
                                //wait until data is sent or workspace stopped
                                WaitHandle.WaitAny(waitHandles);
                            }

                            _nextRound.Set();
                        }
                        else
                        {
                            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                           {
                               if (settings.PresentationMode)
                               {
                                   _activePresentation.StepCounter = 27;
                               }
                               else
                               {
                                   _activePresentation.StepCounter = 13;
                               }

                               if (!settings.AutomaticMode)
                               {
                                   _activePresentation.HighlightDispatcher.Start();
                                   _activePresentation.SetupView();
                               }
                           }, null);

                            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                                (SendOrPostCallback)delegate { _activePresentation.SBoxesCurrentAttack = new bool[4]; },
                                null);

                            if (!settings.AutomaticMode)
                            {
                                //wait until data is sent or workspace stopped
                                WaitHandle.WaitAny(waitHandles);
                            }

                            _nextRound.Set();
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            _stop = false;

            //dispatch action: inform ui that workspace is stopped
            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
           {
               _activePresentation.SlideCounterVisibility = Visibility.Hidden;
               _activePresentation.WorkspaceRunning = false;
               _activePresentation.ArrowBeforeOpacity = 0.25;
               _activePresentation.ArrowNextOpacity = 0.25;
               _activePresentation.IsNextPossible = false;
               _activePresentation.IsPreviousPossible = false;
               _activePresentation.IsSkipChapterVisible = Visibility.Hidden;
               _activePresentation.HighlightDispatcher.Stop();
               _activePresentation.sendDataEvent.Reset();
               _activePresentation.workDataEvent.Reset();
               _activePresentation.HighlightDispatcher.Stop();
               _activePresentation.BtnNext.Background = Brushes.LightGray;
               _activePresentation.UIProgressRefresh = true;
           }, null);

            _activePresentation.MessageToDisplayOccured -= displayMessage;
            _activePresentation.ProgressChangedOccured -= UIRefreshProgress;

            switch (settings.CurrentAlgorithm)
            {
                case Algorithms.Cipher1:
                    pathFinder.ProgressChangedOccured -= RefreshProgress;
                    break;
                case Algorithms.Cipher2:
                    pathFinder.AttackSearchResultOccured -= handleSearchresult;
                    pathFinder.ProgressChangedOccured -= RefreshProgress;
                    break;
                case Algorithms.Cipher3:
                    pathFinder.AttackSearchResultOccured -= handleSearchresult;
                    pathFinder.ProgressChangedOccured -= RefreshProgress;
                    break;
            }
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            _stop = true;

            switch (settings.CurrentAlgorithm)
            {
                //there is nothing to stop in cipher1
                case Algorithms.Cipher1:
                    {
                    }
                    break;
                case Algorithms.Cipher2:
                    {
                        if (pathFinder is Cipher2PathFinder)
                        {
                            Cipher2PathFinder c2PathFinder = pathFinder as Cipher2PathFinder;
                            c2PathFinder.Stop = true;
                            _semaphoreSlim.Wait();
                            try
                            {
                                if (c2PathFinder.Cts != null)
                                {
                                    c2PathFinder.Cts.Cancel();
                                    c2PathFinder.Cts.Dispose();
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
                    }
                    break;
                case Algorithms.Cipher3:
                    {
                        if (pathFinder is Cipher3PathFinder)
                        {
                            Cipher3PathFinder c3PathFinder = pathFinder as Cipher3PathFinder;
                            c3PathFinder.Stop = true;
                            _semaphoreSlim.Wait();
                            try
                            {
                                if (c3PathFinder.Cts != null)
                                {
                                    c3PathFinder.Cts.Cancel();
                                    c3PathFinder.Cts.Dispose();
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
                    }
                    break;
            }

            _nextRound.Set();
            _activePresentation.sendDataEvent.Set();
            _activePresentation.workDataEvent.Set();

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
        /// Handles an occured error during changing a setting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleSettingsError(object sender, SettingsErrorMessagsEventArgs e)
        {
            GuiLogMessage(e.message, NotificationLevel.Warning);
        }

        /// <summary>
        /// Method to generate the attack configuration for following components
        /// </summary>
        /// <returns></returns>
        private DifferentialAttackRoundConfiguration GenerateAttackConfiguration()
        {
            //result object
            DifferentialAttackRoundConfiguration result = new DifferentialAttackRoundConfiguration();

            //select algorithm
            switch (settings.CurrentAlgorithm)
            {
                //Cipher 1
                case Algorithms.Cipher1:
                    {
                        result.SelectedAlgorithm = settings.CurrentAlgorithm;
                        result.SearchPolicy = settings.CurrentSearchPolicy;
                        result.AbortingPolicy = settings.CurrentAbortingPolicy;
                        result.IsFirst = true;
                    }
                    break;
            }

            //return
            return result;
        }

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
                if (settings.CurrentAlgorithm == Algorithms.Cipher1)
                {
                    //dispatch action: set active tutorial number
                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                        (SendOrPostCallback)delegate { _activePresentation.TutorialNumber = 1; }, null);
                }
                else if (settings.CurrentAlgorithm == Algorithms.Cipher2)
                {
                    //dispatch action: set active tutorial number
                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                        (SendOrPostCallback)delegate { _activePresentation.TutorialNumber = 2; }, null);
                }
                else if (settings.CurrentAlgorithm == Algorithms.Cipher3)
                {
                    //dispatch action: set active tutorial number
                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                        (SendOrPostCallback)delegate { _activePresentation.TutorialNumber = 3; }, null);
                }
                else if (settings.CurrentAlgorithm == Algorithms.Cipher4)
                {
                    //dispatch action: set active tutorial number
                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                        (SendOrPostCallback)delegate { _activePresentation.TutorialNumber = 4; }, null);
                }
            }
            else if (e.PropertyName == "AutomaticMode")
            {
                _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send,
                    (SendOrPostCallback)delegate
                   {
                       _activePresentation.AutomaticMode = settings.AutomaticMode;
                       _activePresentation.PresentationMode = settings.PresentationMode;
                       _activePresentation.SetupView();
                   }, null);
            }
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