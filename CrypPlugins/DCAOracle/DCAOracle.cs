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
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using DCAOracle;
using DCAOracle.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;


namespace CrypTool.Plugins.DCAOracle
{
    [Author("Christian Bender", "christian1.bender@student.uni-siegen.de", null, "http://www.uni-siegen.de")]
    [PluginInfo("DCAOracle.Properties.Resources", "PluginCaption", "PluginTooltip", "DCAOracle/userdoc.xml", new[] { "DCAOracle/Images/IC_Oracle.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class DCAOracle : ICrypComponent
    {
        #region Private Variables

        private readonly DCAOracleSettings _settings = new DCAOracleSettings();
        private readonly Random _random = new Random();
        private int _messageDifference;
        private bool _newMessageDiff;
        private int _messagePairsCount;
        private bool _newMessagePairsCount;
        private ICrypToolStream _messagePairsOutput;

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public DCAOracle()
        {
            _newMessageDiff = false;
            _newMessagePairsCount = false;
        }

        #region Data Properties

        /// <summary>
        /// Property for the count of message pairs
        /// </summary>
        [PropertyInfo(Direction.InputData, "MessagePairsCountInput", "MessagePairsCountInputToolTip", true)]
        public int MessagePairsCount
        {
            get => _messagePairsCount;
            set
            {
                _messagePairsCount = value;
                _newMessagePairsCount = true;
                OnPropertyChanged("MessagePairsCount");
            }
        }

        /// <summary>
        /// Property for the difference of the messages of a pair
        /// </summary>
        [PropertyInfo(Direction.InputData, "MessageDifferenceInput", "MessageDifferenceInputToolTip", true)]
        public int MessageDifference
        {
            get => _messageDifference;
            set
            {
                _messageDifference = value;
                _newMessageDiff = true;
                OnPropertyChanged("MessageDifference");
            }
        }

        /// <summary>
        /// Property for the generated message pairs
        /// </summary>
        [PropertyInfo(Direction.OutputData, "MessagePairsOutput", "MessagePairsOutputToolTip")]
        public ICrypToolStream MessagePairsOutput
        {
            get => _messagePairsOutput;
            set
            {
                _messagePairsOutput = value;
                OnPropertyChanged("MessagePairsOutput");
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
        public UserControl Presentation => null;

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            //check if both inputs are new
            if (!_newMessageDiff || !_newMessagePairsCount)
            {
                return;
            }

            //reset the inputs
            _newMessageDiff = false;
            _newMessagePairsCount = false;

            if (MessagePairsCount == 0)
            {
                GuiLogMessage(Resources.WarningMessageCountMustBeSpecified, NotificationLevel.Warning);
                return;
            }

            double curProgress = 0;
            double stepCount = 1.0 / (MessagePairsCount * 2);
            ProgressChanged(curProgress, 1);

            List<Pair> pairList = new List<Pair>();

            int i;
            for (i = 0; i < MessagePairsCount; i++)
            {
                int xtemp = _random.Next(0, ((int)Math.Pow(2, _settings.WordSize) - 1));
                int ytemp = xtemp ^ MessageDifference;

                ushort x = (ushort)xtemp;
                ushort y = (ushort)ytemp;

                Pair inputPair = new Pair()
                {
                    LeftMember = x,
                    RightMember = y
                };

                pairList.Add(inputPair);

                curProgress += stepCount;
                ProgressChanged(curProgress, 1);
            }

            _newMessageDiff = false;

            //each pair consists of 2 uint16 and each uint16 consists of 2 byte
            byte[] outputTemp = new byte[MessagePairsCount * 2 * 2];

            //convert pairs
            i = 0;
            foreach (Pair curPair in pairList)
            {
                byte[] leftMember = BitConverter.GetBytes(curPair.LeftMember);
                outputTemp[i] = leftMember[1];
                outputTemp[i + 1] = leftMember[0];

                byte[] rightMember = BitConverter.GetBytes(curPair.RightMember);
                outputTemp[i + 2] = rightMember[1];
                outputTemp[i + 3] = rightMember[0];

                i += 4;
                curProgress += stepCount;
                ProgressChanged(curProgress, 1);
            }

            //write all messages to the output
            using (CStreamWriter writer = new CStreamWriter())
            {
                writer.Write(outputTemp, 0, outputTemp.Length);
                writer.Flush();
                MessagePairsOutput = writer;
            }

            //finished: inform output
            //OnPropertyChanged("MessagePairsOutput");
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            _newMessageDiff = false;
            _newMessagePairsCount = false;
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
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
