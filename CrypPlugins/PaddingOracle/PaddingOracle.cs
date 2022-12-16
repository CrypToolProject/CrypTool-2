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
using CrypTool.PluginBase;
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.PaddingOracle
{
    [Author("Alexander Juettner", "alex@juettner-online.com", "CrypTool 2 Team", "http://CrypTool2.vs.uni-due.de")]
    [PluginInfo("PaddingOracle.Properties.Resources", "PluginCaption", "PluginTooltip", "PaddingOracle/DetailedDescription/doc.xml", new[] { "PaddingOracle/img/icon.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    [ComponentVisualAppearance(ComponentVisualAppearance.VisualAppearanceEnum.Opened)]
    public class PaddingOracle : ICrypComponent
    {
        #region Private Variables

        private readonly PaddingOracleSettings settings = new PaddingOracleSettings();

        private ICrypToolStream inputStream;
        private readonly OraclePresentation pres = new OraclePresentation();

        //private PropertyChangedEventHandler seventHandler;
        private RoutedPropertyChangedEventHandler<double> rviewEventHandler;

        private byte[] plainBlock;
        private int blockSize;
        private int errorCode;
        private int firstViewBytePos;
        private int padRange;
        private int paddingLength;
        private string plainBlockStr;
        private bool isInitiated = false;


        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputDecCaption", "InputDecTooltip")]
        public ICrypToolStream DecryptedInputStream
        {
            get => inputStream;
            set
            {
                inputStream = value;
                OnPropertyChanged("DecryptedInputStream");
            }
        }

        [PropertyInfo(Direction.OutputData, "PaddingResultCaption", "PaddingResultTooltip")]
        public bool PaddingResult
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings => settings;

        public UserControl Presentation => pres;

        public void PreExecution()
        {
            PaddingResult = false;
            plainBlockStr = "";
            firstViewBytePos = 1;
            OnPropertyChanged("PaddingResult");
        }

        public void Execute()
        {
            //GuiLogMessage("PO exec", NotificationLevel.Info);

            errorCode = 0;

            try
            {
                ProgressChanged(0, 1);
                using (CStreamReader reader = DecryptedInputStream.CreateReader())
                {
                    errorCode = 1;

                    blockSize = System.Convert.ToInt16(reader.Length / 2);

                    plainBlock = new byte[blockSize];

                    errorCode = 2;

                    //read input
                    reader.Read(plainBlock); //first block -> ignore
                    reader.Read(plainBlock);
                    reader.Close();
                }

                errorCode = 3;

                bool validPadding = true;
                byte[] result = new byte[1];

                //expected padding length = value of last byte
                paddingLength = System.Convert.ToInt16(plainBlock[blockSize - 1]);
                if (paddingLength == 0 || paddingLength > blockSize) //check if paddingLen is invalid
                {
                    validPadding = false;
                }
                else //padding length is valid
                {
                    //go through all supposed padding bytes and check if they all have the correct value
                    int wordCounter = 1;
                    while (validPadding && wordCounter < paddingLength)
                    {
                        if (plainBlock[blockSize - 1 - wordCounter] != paddingLength)
                        {
                            validPadding = false;
                        }

                        wordCounter++;
                    }
                }

                errorCode = 4;

                //Update Presentation Layer
                plainBlockStr = System.BitConverter.ToString(plainBlock);
                plainBlockStr = plainBlockStr.Replace("-", " ");
                errorCode = 6;
                setPadPointer();
                errorCode = 7;
                setContent();
                errorCode = 85;
                errorCode = 9;

                double minVal = Math.Min(18 - blockSize, 10);
                minVal /= 10;


                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.showPaddingImg(validPadding);
                    pres.viewByteScroller.Minimum = minVal;
                }, null);

                isInitiated = true;

                PaddingResult = validPadding;
                OnPropertyChanged("PaddingResult");

            }
            catch (System.Exception)
            {
                GuiLogMessage("PO error: " + errorCode, NotificationLevel.Error);
            }


            ProgressChanged(1, 1);

        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {

            //add byteview event handler to presentation
            rviewEventHandler = new RoutedPropertyChangedEventHandler<double>(PresViewEventHandler);

            try { pres.viewBytePanel.RemoveHandler(System.Windows.Controls.Primitives.RangeBase.ValueChangedEvent, rviewEventHandler); }
            catch (Exception e) { GuiLogMessage("Error!! " + e.Message, NotificationLevel.Info); }

            pres.viewBytePanel.AddHandler(System.Windows.Controls.Primitives.RangeBase.ValueChangedEvent, rviewEventHandler);
        }

        public void Dispose()
        {
        }

        #endregion

        #region Methods

        private bool IsValidPadding(byte[] message)
        {
            bool result = false;
            return result;
        }

        private bool compareByteArrays(byte[] array1, byte[] array2)
        {
            bool result = true;

            for (int byteCounter = 0; byteCounter < blockSize; byteCounter++)
            {
                if (array1[byteCounter] != array2[byteCounter])
                {
                    result = false;
                }
            }

            return result;
        }

        private void updateViewPres()
        {
            setPadPointer();
            setContent();
        }

        private void setContent()
        {
            if (plainBlockStr.Length > 1)
            {
                string newContent = plainBlockStr.Substring(3 * firstViewBytePos - 3);

                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.plainBlock.Content = newContent;
                }, null);
            }
        }

        private void setPadPointer()
        {
            //set pad range (how many bytes shall be included in the pointer)
            padRange = (paddingLength > blockSize || paddingLength < 1) ? 1 : paddingLength;

            int lastViewBytePos = firstViewBytePos + 7;
            int padStart = blockSize - padRange + 1;
            int padEnd = blockSize;
            bool startVisible = (firstViewBytePos <= padStart) && (padStart <= lastViewBytePos);
            bool endVisible = (firstViewBytePos <= padEnd) && (padEnd <= lastViewBytePos);

            //0 = padding range is full visible
            //1 = ony start of padding range is not visible
            //2 = start and end of padding range are not visible, but range overlaps with viewport
            //3 = only end of padding range is not visible
            //4 = padding range has no overlap with viewport
            int viewMode = endVisible
                         ? (startVisible ? 0 : 1)
                         : ((padStart > lastViewBytePos) ? 4 : startVisible ? 3 : 2);

            padRange = Math.Max(Math.Min(lastViewBytePos - padStart + 1, 8), 0);

            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.setPadPointer(padRange, viewMode);
            }, null);
        }

        #endregion

        #region Event Handling

        private void PresViewEventHandler(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //FrameworkElement feSource = e.Source as FrameworkElement;
            if (isInitiated)
            {
                double viewBytePos = pres.viewByteScroller.Value;

                firstViewBytePos = Convert.ToInt32((1.1 - viewBytePos) * 10);
                int lastViewBytePos = firstViewBytePos + 7;

                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.descShownBytes.Content = firstViewBytePos + ".." + lastViewBytePos;
                }, null);

                updateViewPres();
            }
        }


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