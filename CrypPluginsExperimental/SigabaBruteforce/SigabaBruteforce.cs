/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using CrypTool.CrypAnalysisViewControl;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Control;
using CrypTool.PluginBase.Miscellaneous;
//using SigabaBruteforce.CrypTool.PluginBase.Control;

namespace SigabaBruteforce
{
    [Author("Julian Weyers", "weyers@CrypTool.org", "CrypTool 2 Team", "http://CrypTool2.vs.uni-due.de")]
    [PluginInfo("SigabaBruteforce.Properties.Resources", "PluginCaption", "PluginToolTip", "Enigma/DetailedDescription/doc.xml", "Sigaba/Images/Icon.png", "Enigma/Images/encrypt.png", "Enigma/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class SigabaBruteforce : ICrypComponent
    {
        private IControlSigabaEncryption controlMaster;

        private IControlCost costMaster;
        private Boolean stop;

        #region Constructor 

        public SigabaBruteforce()
        {
            var sigpa = new SigabaBruteforceQuickWatchPresentation();
            Presentation = sigpa;
            sigpa.SelectedResultEntry += SelectedResultEntry;
        }

        #endregion

        #region Private Variables

        public readonly SigabaBruteforceSettings _settings = new SigabaBruteforceSettings();

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputCaption", "InputTooltip")]
        public string Input { get; set; }

        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip")]
        public string Output { get; set; }

        #endregion

        [PropertyInfo(Direction.ControlMaster, "ControlMasterCaption", "ControlMasterTooltip", false)]
        public IControlSigabaEncryption ControlMaster
        {
            get { return controlMaster; }
            set
            {
                // value.OnStatusChanged += onStatusChanged;
                controlMaster = value;
                OnPropertyChanged("ControlMaster");
            }
        }

        [PropertyInfo(Direction.ControlMaster, "CostMasterCaption", "CostMasterTooltip", false)]
        public IControlCost CostMaster
        {
            get { return costMaster; }
            set { costMaster = value; }
        }

        #region ICrypComponent Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return _settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation { get; private set; }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            stop = false;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            bruteforceAlphabetMaze();

            ProgressChanged(1, 1);
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
            stop = true;
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

        #region Bruteforce 

        private LinkedList<ValueKey> list1;
        private Queue valuequeue;

        private static string GetIntBinaryString(int n)
        {
            var b = new char[10];

            for (int i = 0, pos = 9; i < 10; i++, pos--)
                b[pos] = ((n & (1 << i)) == 0) ? '0' : '1';

            return new string(b);
        }

        private void bruteforceAlphabetMaze()
        {
            DateTime lastUpdate = DateTime.Now;
            DateTime starttime = DateTime.Now;
            BigInteger isummax = _settings.getKeyspaceAsLong();
            BigInteger icount = 0;
            List<double> bestlist = new List<double>();
            
            double best = Double.MinValue;
            bestlist.Add(Double.MinValue);

            int[][] indexarr = _settings.indexRotorSettings();
            int[][] controlarr = _settings.rotorSettings();

            bool[][] whitelist = _settings.getWhiteList();

            int[] arr2 = _settings.setStartingArr(indexarr);

            string input = controlMaster.preFormatInput(Input);
            byte[] ciphertext = Encoding.ASCII.GetBytes(input);

            string reversekey;

            if (costMaster.GetRelationOperator() == RelationOperator.LessThen)
            {
                best = Double.MaxValue;
                bestlist.Add(Double.MaxValue);
            }

            list1 = getDummyLinkedList(best);
            valuequeue = Queue.Synchronized(new Queue());

            UpdatePresentationList(isummax, icount, starttime);
            
            do
            {
                for (byte i = 0; i < arr2.Length; i++)
                    controlMaster.setIndexRotors(i, (byte)arr2[i]);

                for (int i1 = _settings.IndexRotor1From; i1 <= _settings.IndexRotor1To; i1++)
                {
                    ControlMaster.setPositionsIndex((byte)arr2[0], 0, (byte)i1);

                    for (int i2 = _settings.IndexRotor2From; i2 <= _settings.IndexRotor2To; i2++)
                    {
                        ControlMaster.setPositionsIndex((byte)arr2[1], 1, (byte)i2);

                        for (int i3 = _settings.IndexRotor3From; i3 <= _settings.IndexRotor3To; i3++)
                        {
                            ControlMaster.setPositionsIndex((byte)arr2[2], 2, (byte)i3);
                            for (int i4 = _settings.IndexRotor4From; i4 <= _settings.IndexRotor4To; i4++)
                            {
                                ControlMaster.setPositionsIndex((byte)arr2[3], 3, (byte)i4);
                                for (int i5 = _settings.IndexRotor5From; i5 <= _settings.IndexRotor5To; i5++)
                                {
                                    ControlMaster.setPositionsIndex((byte)arr2[4], 4, (byte)i5);

                                    int[] arr = _settings.setStartingArr(controlarr);

                                    do
                                    {
                                        for (byte i = 0; i < arr.Length; i++)
                                            if (i < 5)
                                                controlMaster.setCipherRotors(i, (byte)arr[i]);
                                            else
                                                controlMaster.setControlRotors(i, (byte)arr[i]);

                                        for (int co5 = _settings.ControlRotor5From; co5 <= _settings.ControlRotor5To; co5++)
                                        {
                                            ControlMaster.setPositionsControl((byte)arr[9], 9, (byte)co5);

                                            foreach (var bits in whitelist)
                                            {
                                                //bin = GetIntBinaryString(whitelist[r]);
                                                //reversekey = bin.Replace('1', 'R').Replace('0', ' ');
                                                reversekey = String.Join("", bits.Select(b => b ? 'R' : ' '));

                                                for (int i = 0; i < bits.Length; i++)
                                                {
                                                    //        if (lastkey[i] != bin[i])
                                                    ControlMaster.setBool((byte)arr[i], (byte)i, bits[i]);
                                                }
                                                //  lastkey = bin;

                                                ControlMaster.setIndexMaze();

                                                for (int ci1 = _settings.CipherRotor1From; ci1 <= _settings.CipherRotor1To; ci1++)
                                                {
                                                    for (int ci2 = _settings.CipherRotor2From; ci2 <= _settings.CipherRotor2To; ci2++)
                                                    {
                                                        for (int ci3 = _settings.CipherRotor3From; ci3 <= _settings.CipherRotor3To; ci3++)
                                                        {
                                                            for (int ci4 = _settings.CipherRotor4From; ci4 <= _settings.CipherRotor4To; ci4++)
                                                            {
                                                                for (int ci5 = _settings.CipherRotor5From; ci5 <= _settings.CipherRotor5To; ci5++)
                                                                {
                                                                    for (int co1 = _settings.ControlRotor1From; co1 <= _settings.ControlRotor1To; co1++)
                                                                    {
                                                                        ControlMaster.setPositionsControl((byte)arr[5], 5, (byte)co1);
                                                                        for (int co2 = _settings.ControlRotor2From; co2 <= _settings.ControlRotor2To; co2++)
                                                                        {
                                                                            for (int co3 = _settings.ControlRotor3From; co3 <= _settings.ControlRotor3To; co3++)
                                                                            {
                                                                                for (int co4 = _settings.ControlRotor4From; co4 <= _settings.ControlRotor4To; co4++)
                                                                                {
                                                                                    byte[] rotorPositions = {
																				            (byte) ci1, (byte) ci2, (byte) ci3, (byte) ci4, (byte) ci5,
																				            (byte) co1, (byte) co2, (byte) co3, (byte) co4, (byte) co5
																			            };

                                                                                    byte[] plain = controlMaster.DecryptFast(ciphertext, arr, rotorPositions);

                                                                                    double score = costMaster.CalculateCost(plain);

                                                                                    bool better = false;
                                                                                    if (costMaster.GetRelationOperator() == RelationOperator.LessThen)
                                                                                    {
                                                                                        if (score <= bestlist.Last())
                                                                                        {
                                                                                            bestlist.Add(score);
                                                                                            bestlist.Sort();
                                                                                            better = true;
                                                                                        }
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        if (score >= bestlist.Last())
                                                                                        {
                                                                                            bestlist.Add(score);
                                                                                            bestlist.Sort();
                                                                                            bestlist.Reverse();
                                                                                            better = true;
                                                                                        }
                                                                                    }

                                                                                    if (better)
                                                                                    {
                                                                                        if (bestlist.Count > 10)
                                                                                            bestlist.RemoveAt(10);

                                                                                        int[] indexPositions = { i1, i2, i3, i4, i5 };

                                                                                        var valkey = new ValueKey();
                                                                                        valkey.decryption = plain;
                                                                                        valkey.cipherKey = String.Join("", rotorPositions.Take(5).Select(i => (char)(i + 65))); ;
                                                                                        valkey.indexKey = String.Join("", indexPositions);
                                                                                        valkey.controlKey = String.Join("", rotorPositions.Skip(5).Select(i => (char)(i + 65))); ;
                                                                                        valkey.cipherRotors = String.Join("", Enumerable.Range(0, 5).Select(i => arr[i] + "" + reversekey[i]));
                                                                                        valkey.controlRotors = String.Join("", Enumerable.Range(5, 5).Select(i => arr[i] + "" + reversekey[i]));
                                                                                        valkey.indexRotors = String.Join("", arr2);
                                                                                        valkey.value = score;
                                                                                        valuequeue.Enqueue(valkey);
                                                                                    }

                                                                                    if (lastUpdate.AddMilliseconds(1000) < DateTime.Now)
                                                                                    {
                                                                                        UpdatePresentationList(isummax, icount, starttime);
                                                                                        lastUpdate = DateTime.Now;
                                                                                    }

                                                                                    icount++;

                                                                                    if (stop) return;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    while (_settings.NextPermutation(arr, controlarr));
                                }
                            }
                        }
                    }
                }
            } 
            while (_settings.NextPermutation(arr2, indexarr));

            UpdatePresentationList(isummax, icount, starttime);
        }

        private void SelectedResultEntry(ResultEntry resultEntry)
        {
            Output = resultEntry.Text;
            OnPropertyChanged("Output");
        }

        private LinkedList<ValueKey> getDummyLinkedList(double best)
        {
            var valueKey = new ValueKey();
            valueKey.value = best;
            valueKey.cipherKey = "";
            valueKey.controlKey = "";
            valueKey.indexKey = "";
            valueKey.cipherRotors = "";
            valueKey.controlRotors = "";

            valueKey.decryption = new byte[0];
            var list = new LinkedList<ValueKey>();
            LinkedListNode<ValueKey> node = list.AddFirst(valueKey);
            for (int i = 0; i < 9; i++)
            {
                node = list.AddAfter(node, valueKey);
            }
            return list;
        }

        private DateTime UpdatePresentationList(BigInteger size, BigInteger sum, DateTime starttime)
        {
            updateToplist(list1);
            showProgress(starttime, size, sum);

            double d = (((double) sum/(double) size)*100);
            ProgressChanged(d, 100);

            return DateTime.Now;;
        }

        private void showProgress(DateTime startTime, BigInteger size, BigInteger sum)
        {
            LinkedListNode<ValueKey> linkedListNode;

            if (Presentation.IsVisible)
            {
                DateTime currentTime = DateTime.Now;
                TimeSpan elapsedtime = DateTime.Now.Subtract(startTime);

                var elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);

                TimeSpan span = currentTime.Subtract(startTime);
                int seconds = span.Seconds;
                int minutes = span.Minutes;
                int hours = span.Hours;
                int days = span.Days;

                long allseconds = seconds + 60*minutes + 60*60*hours + 24*60*60*days;
                if (allseconds == 0) allseconds = 1;

                double keysPerSec = Math.Round((double) sum/allseconds, 2);

                BigInteger keystodo = (size - sum);

                if (keysPerSec == 0)
                    keysPerSec = 1;

                double secstodo = ((double)keystodo / keysPerSec);

                //dummy Time 
                var endTime = new DateTime(1970, 1, 1);
                try
                {
                    endTime = DateTime.Now.AddSeconds(secstodo);
                }
                catch
                {
                }

                (Presentation).Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    var presentation = (SigabaBruteforceQuickWatchPresentation)Presentation;
                    presentation.StartTime.Value = "" + startTime;
                    presentation.ElapsedTime.Value = "" + elapsedspan;
                    presentation.KeysPerSecond.Value = "" + keysPerSec;

                    if (endTime != (new DateTime(1970, 1, 1)))
                    {
                        presentation.TimeLeft.Value = "" + endTime.Subtract(DateTime.Now).ToString(@"dd\.hh\:mm\:ss");
                        presentation.EndTime.Value = "" + endTime;
                    }
                    else
                    {
                        presentation.TimeLeft.Value = "incalculable";
                        presentation.EndTime.Value = "in a galaxy far, far away...";
                    }

                    if (list1 != null)
                    {
                        linkedListNode = list1.First;
                        presentation.Entries.Clear();
                        int i = 0;
                        while (linkedListNode != null)
                        {
                            i++;
                            var entry = new ResultEntry();
                            entry.Ranking = i;

                            String dec = Encoding.ASCII.GetString(linkedListNode.Value.decryption);
                            if (dec.Length > 2500) // Short strings need not be cut off
                                dec = dec.Substring(0, 2500);

                            entry.Text = dec;
                            entry.CipherKey = linkedListNode.Value.cipherKey;
                            entry.IndexKey = linkedListNode.Value.indexKey;
                            entry.ControlKey = linkedListNode.Value.controlKey;
                            entry.CipherRotors = linkedListNode.Value.cipherRotors;
                            entry.ControlRotors = linkedListNode.Value.controlRotors;
                            entry.IndexRotors = linkedListNode.Value.indexRotors;
                            entry.Value = Math.Round(linkedListNode.Value.value, 2) + "";

                            presentation.Entries.Add(entry);

                            linkedListNode = linkedListNode.Next;
                        }
                    }
                }, null);
            }
        }

        private void updateToplist(LinkedList<ValueKey> costList)
        {
            LinkedListNode<ValueKey> node;

            var enc = new ASCIIEncoding();

            while (valuequeue.Count != 0)
            {
                var vk = (ValueKey) valuequeue.Dequeue();
                if (costMaster.GetRelationOperator() == RelationOperator.LargerThen)
                {
                    if (vk.value > costList.Last().value)
                    {
                        node = costList.First;
                        int i = 0;
                        while (node != null)
                        {
                            if (vk.value > node.Value.value)
                            {
                                costList.AddBefore(node, vk);
                                costList.RemoveLast();
                                if (i == 0)
                                {
                                    Output = enc.GetString(vk.decryption);
                                }
                                // value_threshold = costList.Last.Value.value;
                                break;
                            }
                            node = node.Next;
                            i++;
                        } //end while
                    } //end if
                }
                else
                {
                    if (vk.value < costList.Last().value)
                    {
                        node = costList.First;
                        int i = 0;
                        while (node != null)
                        {
                            if (vk.value < node.Value.value)
                            {
                                costList.AddBefore(node, vk);
                                costList.RemoveLast();
                                if (i == 0)
                                {
                                    Output = enc.GetString(vk.decryption);
                                }

                                // value_threshold = costList.Last.Value.value;
                                break;
                            }
                            node = node.Next;
                            i++;
                        } //end while
                    } //end if
                }
            }
            OnPropertyChanged("Output");
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

    public struct ValueKey
    {
        public String cipherKey;
        public String cipherRotors;
        public String controlKey;
        public String controlRotors;
        public byte[] decryption;
        public String indexKey;
        public String indexRotors;
        public byte[] keyArray;
        public double value;
    };

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
        public string CipherKey { get; set; }
        public string ControlKey { get; set; }
        public string IndexKey { get; set; }
        public string CipherRotors { get; set; }
        public string ControlRotors { get; set; }
        public string IndexRotors { get; set; }
        public string Text { get; set; }

        public string ClipboardValue => Value;
        public string ClipboardKey => $"Cipher Key: {CipherKey} - Control Key: {ControlKey} - Index Key: {IndexKey}";
        public string ClipboardText => Text;
        public string ClipboardEntry =>
            "Rank: " + Ranking + Environment.NewLine +
            "Value: " + Value + Environment.NewLine +
            ClipboardKey + Environment.NewLine +
            "Text: " + Text;
    }
}