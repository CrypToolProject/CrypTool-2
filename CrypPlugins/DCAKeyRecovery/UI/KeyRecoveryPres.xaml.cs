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

using DCAKeyRecovery.Logic;
using DCAKeyRecovery.UI.Cipher1;
using DCAKeyRecovery.UI.Cipher2;
using DCAKeyRecovery.UI.Cipher3;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace DCAKeyRecovery.UI
{
    /// <summary>
    /// Interaktionslogik für KeyRecoveryPres.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("DCAKeyRecovery.Properties.Resources")]
    public partial class KeyRecoveryPres : UserControl, INotifyPropertyChanged
    {
        private Algorithms _tutorialNumber;
        private UserControl _tableView;
        private UserControl _summaryView;
        private bool _workspaceRunning;
        private bool _startEnabled;
        private bool _nextMessageEnabled;
        private bool _nextKeyEnabled;
        private DispatcherTimer _dispatcher;
        private Visibility _isNextStepPanelVisible = Visibility.Hidden;

        public AutoResetEvent StartClickEvent;

        /// <summary>
        /// Constructor
        /// </summary>
        public KeyRecoveryPres()
        {
            _dispatcher = new System.Windows.Threading.DispatcherTimer();
            _dispatcher.Tick += new EventHandler(highlightComponent);
            _dispatcher.Interval = new TimeSpan(0, 0, 0, 0, 500);


            StartClickEvent = new AutoResetEvent(false);
            StartEnabled = false;
            NextMessageEnabled = false;
            NextKeyEnabled = false;
            DataContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// handler to highlight the button start
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void highlightComponent(object sender, EventArgs e)
        {
            if (BtnStart.Background == Brushes.LightGray)
            {
                BtnStart.Background = Brushes.LimeGreen;
            }
            else
            {
                BtnStart.Background = Brushes.LightGray;
            }
        }

        #region properties

        /// <summary>
        /// Property for _dispatcher
        /// </summary>
        public DispatcherTimer HighlightDispatcher
        {
            get => _dispatcher;
            set
            {
                _dispatcher = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for running workspace
        /// </summary>
        public bool WorkspaceRunning
        {
            get => _workspaceRunning;
            set
            {
                _workspaceRunning = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set the state of the button
        /// </summary>
        public bool StartEnabled
        {
            get => _startEnabled;
            set
            {
                _startEnabled = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set the state of the button
        /// </summary>
        public bool NextMessageEnabled
        {
            get => _nextMessageEnabled;
            set
            {
                _nextMessageEnabled = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set the state of the button
        /// </summary>
        public bool NextKeyEnabled
        {
            get => _nextKeyEnabled;
            set
            {
                _nextKeyEnabled = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for the tutorial number
        /// </summary>
        public Algorithms TutorialNumber
        {
            get => _tutorialNumber;
            set
            {
                _tutorialNumber = value;

                switch (_tutorialNumber)
                {
                    case Algorithms.Cipher1:
                        {
                            _tableView = new Cipher1ResultView();
                            _summaryView = new Cipher1SummaryResultView();

                            TabItemTable.Child = _tableView;
                            TabItemSummary.Child = _summaryView;
                        }
                        break;
                    case Algorithms.Cipher2:
                        {
                            _tableView = new Cipher2AnyRoundResultView();
                            _summaryView = new Cipher2SummaryResultView();

                            TabItemTable.Child = _tableView;
                            TabItemSummary.Child = _summaryView;
                        }
                        break;
                    case Algorithms.Cipher3:
                        {
                            _tableView = new Cipher3AnyRoundResultView();
                            _summaryView = new Cipher3SummaryResultView();

                            TabItemTable.Child = _tableView;
                            TabItemSummary.Child = _summaryView;
                        }
                        break;
                }

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Visibility of NextStepPanel
        /// </summary>
        public Visibility IsNextStepPanelVisible
        {
            get => _isNextStepPanelVisible;
            set
            {
                _isNextStepPanelVisible = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region methods

        /// <summary>
        /// Prepares the UI to show results of the last attack round
        /// </summary>
        public void PrepareUIForLastRound()
        {
            //select tutorial
            switch (TutorialNumber)
            {
                case Algorithms.Cipher2:
                    {
                        _tableView = new Cipher2LastRoundResultView();
                        TabItemTable.Child = _tableView;
                    }
                    break;
                case Algorithms.Cipher3:
                    {
                        _tableView = new Cipher3LastRoundResultView();
                        TabItemTable.Child = _tableView;
                    }
                    break;
            }
        }

        /// <summary>
        /// Resets the UI to the initial state
        /// </summary>
        public void ResetUI()
        {
            switch (_tutorialNumber)
            {
                case Algorithms.Cipher1:
                    {
                        _tableView = new Cipher1ResultView();

                        TabItemTable.Child = _tableView;
                    }
                    break;
                case Algorithms.Cipher2:
                    {
                        _tableView = new Cipher2AnyRoundResultView();
                        TabItemTable.Child = _tableView;
                    }
                    break;
                case Algorithms.Cipher3:
                    {
                        _tableView = new Cipher3AnyRoundResultView();
                        TabItemTable.Child = _tableView;
                    }
                    break;
            }
        }

        /// <summary>
        /// Clears the last key results
        /// </summary>
        public void clearLastKeyResults()
        {
            switch (TutorialNumber)
            {
                case Algorithms.Cipher2:
                    try
                    {
                        Cipher2AnyRoundResultView v2 = _tableView as Cipher2AnyRoundResultView;
                        if (v2 != null)
                        {
                            if (v2.KeyResults != null)
                            {
                                v2.Cipher2AnyRoundResultDatagrid.SelectedItem = null;
                                v2.KeyResults.Clear();
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }

                    break;

                case Algorithms.Cipher3:
                    try
                    {
                        Cipher3AnyRoundResultView v3 = _tableView as Cipher3AnyRoundResultView;
                        if (v3 != null)
                        {
                            if (v3.KeyResults != null)
                            {
                                v3.Cipher3AnyRoundResultDatagrid.SelectedItem = null;
                                v3.KeyResults.Clear();
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }

                    break;
            }
        }

        /// <summary>
        /// Adds the key results to the table
        /// </summary>
        /// <param name="e"></param>
        public void AddAnyRoundKeyResult(ResultViewAnyRoundKeyResultEventArgs e)
        {
            //check if object is present
            if (_tableView == null)
            {
                return;
            }

            //select tutorial
            switch (TutorialNumber)
            {
                //cipher 2
                case Algorithms.Cipher2:
                    {
                        Cipher2AnyRoundResultView v = _tableView as Cipher2AnyRoundResultView;
                        if (v.KeyResults != null)
                        {
                            v.KeyResults.Clear();

                            //insert the items with descending order
                            foreach (Models.KeyResult curKeyResult in e.keyResults.OrderByDescending(curItem => curItem.HitCount)
                                .ToList())
                            {
                                //add the data
                                v.KeyResults.Add(curKeyResult);
                            }
                        }
                    }
                    break;
                case Algorithms.Cipher3:
                    {
                        Cipher3AnyRoundResultView v = _tableView as Cipher3AnyRoundResultView;
                        if (v.KeyResults != null)
                        {
                            v.KeyResults.Clear();

                            //insert the items with descending order
                            foreach (Models.KeyResult curKeyResult in e.keyResults.OrderByDescending(curItem => curItem.HitCount)
                                .ToList())
                            {
                                //add the data
                                v.KeyResults.Add(curKeyResult);
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Refresh the UI of result view for the last round
        /// </summary>
        /// <param name="lastRoundEventArgs"></param>
        public void RefreshLastRoundResultViewData(ResultViewLastRoundEventArgs lastRoundEventArgs)
        {
            //check if object is present
            if (_tableView == null)
            {
                return;
            }

            //select tutorial
            switch (TutorialNumber)
            {
                //cipher 0
                case Algorithms.Cipher1:
                    //set the data
                    if (_tableView is Cipher1ResultView)
                    {
                        ((Cipher1ResultView)_tableView).CurrentCipherText = lastRoundEventArgs.currentCipherText;
                        ((Cipher1ResultView)_tableView).CurrentKeyCandidate =
                            lastRoundEventArgs.currentKeyCandidate;
                        ((Cipher1ResultView)_tableView).CurrentKeysToTestThisRound =
                            lastRoundEventArgs.currentKeysToTestThisRound;
                        ((Cipher1ResultView)_tableView).CurrentPlainText = lastRoundEventArgs.currentPlainText;
                        ((Cipher1ResultView)_tableView).ExaminedPairCount = lastRoundEventArgs.examinedPairCount;
                        ((Cipher1ResultView)_tableView).ExpectedDifference = lastRoundEventArgs.expectedDifference;
                        ((Cipher1ResultView)_tableView).Round = lastRoundEventArgs.round;
                        ((Cipher1ResultView)_tableView).RemainingKeyCandidates =
                            lastRoundEventArgs.remainingKeyCandidates;

                        ((Cipher1ResultView)_tableView).EndTime = lastRoundEventArgs.endTime;
                        ((Cipher1ResultView)_tableView).StartTime = lastRoundEventArgs.startTime;
                    }

                    break;
                case Algorithms.Cipher2:
                    //set the data
                    if (_tableView is Cipher2LastRoundResultView)
                    {
                        ((Cipher2LastRoundResultView)_tableView).CurrentCipherText =
                            lastRoundEventArgs.currentCipherText;
                        ((Cipher2LastRoundResultView)_tableView).CurrentKeyCandidate =
                            lastRoundEventArgs.currentKeyCandidate;
                        ((Cipher2LastRoundResultView)_tableView).CurrentKeysToTestThisRound =
                            lastRoundEventArgs.currentKeysToTestThisRound;
                        ((Cipher2LastRoundResultView)_tableView).CurrentPlainText =
                            lastRoundEventArgs.currentPlainText;
                        ((Cipher2LastRoundResultView)_tableView).ExaminedPairCount =
                            lastRoundEventArgs.examinedPairCount;
                        ((Cipher2LastRoundResultView)_tableView).ExpectedDifference =
                            lastRoundEventArgs.expectedDifference;
                        ((Cipher2LastRoundResultView)_tableView).Round = lastRoundEventArgs.round;
                        ((Cipher2LastRoundResultView)_tableView).RemainingKeyCandidates =
                            lastRoundEventArgs.remainingKeyCandidates;

                        ((Cipher2LastRoundResultView)_tableView).EndTime = lastRoundEventArgs.endTime;
                        ((Cipher2LastRoundResultView)_tableView).StartTime = lastRoundEventArgs.startTime;
                    }

                    break;
                case Algorithms.Cipher3:
                    //set the data
                    if (_tableView is Cipher3LastRoundResultView)
                    {
                        ((Cipher3LastRoundResultView)_tableView).CurrentCipherText =
                            lastRoundEventArgs.currentCipherText;
                        ((Cipher3LastRoundResultView)_tableView).CurrentKeyCandidate =
                            lastRoundEventArgs.currentKeyCandidate;
                        ((Cipher3LastRoundResultView)_tableView).CurrentKeysToTestThisRound =
                            lastRoundEventArgs.currentKeysToTestThisRound;
                        ((Cipher3LastRoundResultView)_tableView).CurrentPlainText =
                            lastRoundEventArgs.currentPlainText;
                        ((Cipher3LastRoundResultView)_tableView).ExaminedPairCount =
                            lastRoundEventArgs.examinedPairCount;
                        ((Cipher3LastRoundResultView)_tableView).ExpectedDifference =
                            lastRoundEventArgs.expectedDifference;
                        ((Cipher3LastRoundResultView)_tableView).Round = lastRoundEventArgs.round;
                        ((Cipher3LastRoundResultView)_tableView).RemainingKeyCandidates =
                            lastRoundEventArgs.remainingKeyCandidates;

                        ((Cipher3LastRoundResultView)_tableView).EndTime = lastRoundEventArgs.endTime;
                        ((Cipher3LastRoundResultView)_tableView).StartTime = lastRoundEventArgs.startTime;
                    }

                    break;
            }
        }

        /// <summary>
        /// Refresh the UI of the result view for any round
        /// </summary>
        /// <param name="anyRoundEventArgs"></param>
        public void RefreshAnyRoundResultViewData(ResultViewAnyRoundEventArgs anyRoundEventArgs)
        {
            //check if object is present
            if (_tableView == null)
            {
                return;
            }

            switch (TutorialNumber)
            {
                case Algorithms.Cipher2:
                    //set the data
                    ((Cipher2AnyRoundResultView)_tableView).StartTime = anyRoundEventArgs.startTime;
                    ((Cipher2AnyRoundResultView)_tableView).EndTime = anyRoundEventArgs.endTime;
                    ((Cipher2AnyRoundResultView)_tableView).Round = anyRoundEventArgs.round;
                    ((Cipher2AnyRoundResultView)_tableView).CurrentExpectedProbability = anyRoundEventArgs.currentExpectedProbability;
                    ((Cipher2AnyRoundResultView)_tableView).ExpectedDifference = anyRoundEventArgs.expectedDifference;
                    ((Cipher2AnyRoundResultView)_tableView).ExpectedHitCount = anyRoundEventArgs.expectedHitCount;
                    ((Cipher2AnyRoundResultView)_tableView).CurrentKeyCandidate = anyRoundEventArgs.currentKeyCandidate;
                    ((Cipher2AnyRoundResultView)_tableView).MessagePairCountToExamine = anyRoundEventArgs.messagePairCountToExamine.ToString() + "/" + anyRoundEventArgs.messagePairCountFilteredToExamine.ToString();
                    ((Cipher2AnyRoundResultView)_tableView).CurrentRecoveredRoundKey = anyRoundEventArgs.currentRecoveredRoundKey;
                    ((Cipher2AnyRoundResultView)_tableView).CurrentKeysToTestThisRound = anyRoundEventArgs.currentKeysToTestThisRound;

                    break;
                case Algorithms.Cipher3:

                    //set the data
                    ((Cipher3AnyRoundResultView)_tableView).StartTime = anyRoundEventArgs.startTime;
                    ((Cipher3AnyRoundResultView)_tableView).EndTime = anyRoundEventArgs.endTime;
                    ((Cipher3AnyRoundResultView)_tableView).Round = anyRoundEventArgs.round;
                    ((Cipher3AnyRoundResultView)_tableView).CurrentExpectedProbability = anyRoundEventArgs.currentExpectedProbability;
                    ((Cipher3AnyRoundResultView)_tableView).ExpectedDifference = anyRoundEventArgs.expectedDifference;
                    ((Cipher3AnyRoundResultView)_tableView).ExpectedHitCount = anyRoundEventArgs.expectedHitCount;
                    ((Cipher3AnyRoundResultView)_tableView).CurrentKeyCandidate = anyRoundEventArgs.currentKeyCandidate;
                    ((Cipher3AnyRoundResultView)_tableView).MessagePairCountToExamine = anyRoundEventArgs.messagePairCountToExamine.ToString() + "/" + anyRoundEventArgs.messagePairCountFilteredToExamine.ToString();
                    ((Cipher3AnyRoundResultView)_tableView).CurrentRecoveredRoundKey = anyRoundEventArgs.currentRecoveredRoundKey;
                    ((Cipher3AnyRoundResultView)_tableView).CurrentKeysToTestThisRound = anyRoundEventArgs.currentKeysToTestThisRound;

                    break;
            }
        }

        /// <summary>
        /// Refresh the UI of last round
        /// </summary>
        /// <param name="eventArgs"></param>
        public void AddLastRoundRoundResult(ResultViewLastRoundRoundResultEventArgs eventArgs)
        {
            //select tutorial
            switch (TutorialNumber)
            {
                //cipher 0
                case Algorithms.Cipher1:
                    //add the data
                    Cipher1ResultView v1 = (Cipher1ResultView)_tableView;
                    if (v1 != null)
                    {
                        v1.RoundResults.Add(eventArgs.RoundResult);
                    }

                    break;

                case Algorithms.Cipher2:
                    //add the data
                    Cipher2LastRoundResultView v2 = (Cipher2LastRoundResultView)_tableView;
                    if (v2 != null)
                    {
                        v2.RoundResults.Add(eventArgs.RoundResult);
                    }

                    break;
                case Algorithms.Cipher3:
                    //add the data
                    Cipher3LastRoundResultView v3 = (Cipher3LastRoundResultView)_tableView;
                    if (v3 != null)
                    {
                        v3.RoundResults.Add(eventArgs.RoundResult);
                    }

                    break;
            }
        }

        /// <summary>
        /// Refresh the summary view
        /// </summary>
        /// <param name="args"></param>
        public void RefreshSummaryView(SummaryViewRefreshArgs args)
        {
            switch (TutorialNumber)
            {
                case Algorithms.Cipher1:
                    {
                        Cipher1SummaryResultView view = _summaryView as Cipher1SummaryResultView;

                        if (view == null)
                        {
                            return;
                        }

                        //set startimes if it is the first event for that round
                        if (args.firstEvent)
                        {
                            view.StartTimeRound1 = args.lastRoundSummary.startTime;
                        }

                        //set endtime if it is the last event for that round
                        if (args.lastEvent)
                        {
                            view.EndTimeRound1 = args.lastRoundSummary.endTime;
                            view.EndTime = args.lastRoundSummary.endTime;
                        }

                        //add results to the round
                        view.DecryptionCountRound1 += args.lastRoundSummary.decryptionCount;
                        view.MessageCountRound1 += args.lastRoundSummary.messageCount;
                        view.RecoveredSubKey1 = args.lastRoundSummary.recoveredSubKey1;
                        view.RecoveredSubKey0 = args.lastRoundSummary.recoveredSubKey0;
                        view.TestedKeysRound1 += args.lastRoundSummary.testedKeys;

                        //add results to the summary view
                        view.DecryptionCount += args.lastRoundSummary.decryptionCount;
                        view.MessageCount += args.lastRoundSummary.messageCount;
                        view.TestedKeys += args.lastRoundSummary.testedKeys;
                        view.CurrentRound = args.currentRound;
                    }
                    break;
                case Algorithms.Cipher2:
                    {
                        Cipher2SummaryResultView view = _summaryView as Cipher2SummaryResultView;

                        if (view == null)
                        {
                            return;
                        }

                        switch (args.currentRound)
                        {
                            case 1:
                                {
                                    //set startimes if it is the first event for that round
                                    if (args.firstEvent)
                                    {
                                        view.StartTimeRound1 = args.lastRoundSummary.startTime;
                                    }

                                    //set endtime if it is the last event for that round
                                    if (args.lastEvent)
                                    {
                                        view.EndTimeRound1 = args.lastRoundSummary.endTime;
                                        view.EndTime = args.lastRoundSummary.endTime;
                                    }

                                    //add results to the round
                                    view.DecryptionCountRound1 += args.lastRoundSummary.decryptionCount;
                                    view.MessageCountRound1 += args.lastRoundSummary.messageCount;
                                    view.RecoveredSubKey1 = args.lastRoundSummary.recoveredSubKey1;
                                    view.RecoveredSubKey0 = args.lastRoundSummary.recoveredSubKey0;
                                    view.TestedKeysRound1 += args.lastRoundSummary.testedKeys;

                                    //add results to the summary view
                                    view.DecryptionCount += args.lastRoundSummary.decryptionCount;
                                    view.MessageCount += args.lastRoundSummary.messageCount;
                                    view.TestedKeys += args.lastRoundSummary.testedKeys;
                                    view.CurrentRound = args.currentRound;
                                }
                                break;
                            case 2:
                                {
                                    //set startimes if it is the first event for that round
                                    if (args.firstEvent)
                                    {
                                        view.StartTimeRound2 = args.anyRoundSummary.startTime;
                                    }

                                    //set endtime if it is the last event for that round
                                    if (args.lastEvent)
                                    {
                                        view.EndTimeRound2 = args.anyRoundSummary.endTime;
                                    }

                                    //add results to the round
                                    view.DecryptionCountRound2 += args.anyRoundSummary.decryptionCount;
                                    view.MessageCountRound2 += args.anyRoundSummary.messageCount;
                                    view.RecoveredSubKey2 = args.anyRoundSummary.recoveredSubKey;
                                    view.TestedKeysRound2 += args.anyRoundSummary.testedKeys;

                                    //add results to the summary view
                                    view.DecryptionCount += args.anyRoundSummary.decryptionCount;
                                    view.MessageCount += args.anyRoundSummary.messageCount;
                                    view.TestedKeys += args.anyRoundSummary.testedKeys;
                                    view.CurrentRound = args.currentRound;
                                }
                                break;
                            case 3:
                                {
                                    //set startimes if it is the first event for that round
                                    if (args.firstEvent)
                                    {
                                        view.StartTime = args.anyRoundSummary.startTime;
                                        view.StartTimeRound3 = args.anyRoundSummary.startTime;
                                    }

                                    //set endtime if it is the last event for that round
                                    if (args.lastEvent)
                                    {
                                        view.EndTimeRound3 = args.anyRoundSummary.endTime;
                                    }

                                    //add results to the round
                                    view.DecryptionCountRound3 += args.anyRoundSummary.decryptionCount;
                                    view.MessageCountRound3 += args.anyRoundSummary.messageCount;
                                    view.RecoveredSubKey3 = args.anyRoundSummary.recoveredSubKey;
                                    view.TestedKeysRound3 += args.anyRoundSummary.testedKeys;

                                    //add results to the summary view
                                    view.DecryptionCount += args.anyRoundSummary.decryptionCount;
                                    view.MessageCount += args.anyRoundSummary.messageCount;
                                    view.TestedKeys += args.anyRoundSummary.testedKeys;
                                    view.CurrentRound = args.currentRound;
                                }
                                break;
                        }
                    }
                    break;
                case Algorithms.Cipher3:
                    {
                        Cipher3SummaryResultView view = _summaryView as Cipher3SummaryResultView;

                        if (view == null)
                        {
                            return;
                        }

                        switch (args.currentRound)
                        {
                            case 1:
                                {
                                    //set startimes if it is the first event for that round
                                    if (args.firstEvent)
                                    {
                                        view.StartTimeRound1 = args.lastRoundSummary.startTime;
                                    }

                                    //set endtime if it is the last event for that round
                                    if (args.lastEvent)
                                    {
                                        view.EndTimeRound1 = args.lastRoundSummary.endTime;
                                        view.EndTime = args.lastRoundSummary.endTime;
                                    }

                                    //add results to the round
                                    view.DecryptionCountRound1 += args.lastRoundSummary.decryptionCount;
                                    view.MessageCountRound1 += args.lastRoundSummary.messageCount;
                                    view.RecoveredSubKey1 = args.lastRoundSummary.recoveredSubKey1;
                                    view.RecoveredSubKey0 = args.lastRoundSummary.recoveredSubKey0;
                                    view.TestedKeysRound1 += args.lastRoundSummary.testedKeys;

                                    //add results to the summary view
                                    view.DecryptionCount += args.lastRoundSummary.decryptionCount;
                                    view.MessageCount += args.lastRoundSummary.messageCount;
                                    view.TestedKeys += args.lastRoundSummary.testedKeys;
                                    view.CurrentRound = args.currentRound;
                                }
                                break;
                            case 2:
                                {
                                    //set startimes if it is the first event for that round
                                    if (args.firstEvent)
                                    {
                                        view.StartTimeRound2 = args.anyRoundSummary.startTime;
                                    }

                                    //set endtime if it is the last event for that round
                                    if (args.lastEvent)
                                    {
                                        view.EndTimeRound2 = args.anyRoundSummary.endTime;
                                    }

                                    //add results to the round
                                    view.DecryptionCountRound2 += args.anyRoundSummary.decryptionCount;
                                    view.MessageCountRound2 += args.anyRoundSummary.messageCount;
                                    view.RecoveredSubKey2 = args.anyRoundSummary.recoveredSubKey;
                                    view.TestedKeysRound2 += args.anyRoundSummary.testedKeys;

                                    //add results to the summary view
                                    view.DecryptionCount += args.anyRoundSummary.decryptionCount;
                                    view.MessageCount += args.anyRoundSummary.messageCount;
                                    view.TestedKeys += args.anyRoundSummary.testedKeys;
                                    view.CurrentRound = args.currentRound;
                                }
                                break;
                            case 3:
                                {
                                    //set startimes if it is the first event for that round
                                    if (args.firstEvent)
                                    {
                                        view.StartTimeRound3 = args.anyRoundSummary.startTime;
                                    }

                                    //set endtime if it is the last event for that round
                                    if (args.lastEvent)
                                    {
                                        view.EndTimeRound3 = args.anyRoundSummary.endTime;
                                    }

                                    //add results to the round
                                    view.DecryptionCountRound3 += args.anyRoundSummary.decryptionCount;
                                    view.MessageCountRound3 += args.anyRoundSummary.messageCount;
                                    view.RecoveredSubKey3 = args.anyRoundSummary.recoveredSubKey;
                                    view.TestedKeysRound3 += args.anyRoundSummary.testedKeys;

                                    //add results to the summary view
                                    view.DecryptionCount += args.anyRoundSummary.decryptionCount;
                                    view.MessageCount += args.anyRoundSummary.messageCount;
                                    view.TestedKeys += args.anyRoundSummary.testedKeys;
                                    view.CurrentRound = args.currentRound;
                                }
                                break;
                            case 4:
                                {
                                    //set startimes if it is the first event for that round
                                    if (args.firstEvent)
                                    {
                                        view.StartTimeRound4 = args.anyRoundSummary.startTime;
                                    }

                                    //set endtime if it is the last event for that round
                                    if (args.lastEvent)
                                    {
                                        view.EndTimeRound4 = args.anyRoundSummary.endTime;
                                    }

                                    //add results to the round
                                    view.DecryptionCountRound4 += args.anyRoundSummary.decryptionCount;
                                    view.MessageCountRound4 += args.anyRoundSummary.messageCount;
                                    view.RecoveredSubKey4 = args.anyRoundSummary.recoveredSubKey;
                                    view.TestedKeysRound4 += args.anyRoundSummary.testedKeys;

                                    //add results to the summary view
                                    view.DecryptionCount += args.anyRoundSummary.decryptionCount;
                                    view.MessageCount += args.anyRoundSummary.messageCount;
                                    view.TestedKeys += args.anyRoundSummary.testedKeys;
                                    view.CurrentRound = args.currentRound;
                                }
                                break;
                            case 5:
                                {
                                    //set startimes if it is the first event for that round
                                    if (args.firstEvent)
                                    {
                                        view.StartTime = args.anyRoundSummary.startTime;
                                        view.StartTimeRound5 = args.anyRoundSummary.startTime;
                                    }

                                    //set endtime if it is the last event for that round
                                    if (args.lastEvent)
                                    {
                                        view.EndTimeRound5 = args.anyRoundSummary.endTime;
                                    }

                                    //add results to the round
                                    view.DecryptionCountRound5 += args.anyRoundSummary.decryptionCount;
                                    view.MessageCountRound5 += args.anyRoundSummary.messageCount;
                                    view.RecoveredSubKey5 = args.anyRoundSummary.recoveredSubKey;
                                    view.TestedKeysRound5 += args.anyRoundSummary.testedKeys;

                                    //add results to the summary view
                                    view.DecryptionCount += args.anyRoundSummary.decryptionCount;
                                    view.MessageCount += args.anyRoundSummary.messageCount;
                                    view.TestedKeys += args.anyRoundSummary.testedKeys;
                                    view.CurrentRound = args.currentRound;
                                }
                                break;
                        }
                    }
                    break;
            }
        }

        #endregion

        /// <summary>
        /// Action listener for the start button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartButton_Clicked(object sender, RoutedEventArgs e)
        {
            StartClickEvent.Set();
            _dispatcher.Stop();
        }

        /// <summary>
        /// Action listener for next message button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextMessageButton_Clicked(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Action listener for next key button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextKeyButton_Clicked(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Change listener for tab control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControl_ItemChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null)
            {
                if (e.AddedItems.Count != 0)
                {
                    TabItem selectedTab = e.AddedItems[0] as TabItem;
                }
            }
            e.Handled = true;
        }

        /// <summary>
        /// Event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Method to call if data changes
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}