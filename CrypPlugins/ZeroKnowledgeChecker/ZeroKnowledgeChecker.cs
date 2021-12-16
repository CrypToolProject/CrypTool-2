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
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Controls;

namespace CrypTool.Plugins.ZeroKnowledgeChecker
{
    [Author("Ondřej Skowronek", "xskowr00@stud.fit.vutbr.cz", "Brno University of Technology", "https://www.vutbr.cz")]
    [PluginInfo("ZeroKnowledgeChecker.Properties.Resources", "PluginCaption", "PluginTooltip", "ZeroKnowledgeChecker/userdoc.xml", new[] { "ZeroKnowledgeChecker/icon.png" })]
    [AutoAssumeFullEndProgress(false)]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class ZeroKnowledgeChecker : ICrypComponent
    {
        #region Private Variables

        private readonly ZeroKnowledgeCheckerSettings settings = new ZeroKnowledgeCheckerSettings();

        private int currentAttempt;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputCaption", "InputTooltip", true)]
        public BigInteger Input
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputRandomCaption", "OutputRandomTooltip")]
        public BigInteger OutputRandom
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "AmountOfOptionsCaption", "AmountOfOptionsTooltip")]
        public BigInteger AmountOfOptions
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "SuccessCaption", "SuccessTooltip")]
        public bool Success
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "RateOfSuccessCaption", "RateOfSuccessTooltip")]
        public double RateOfSuccess
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings => settings;

        public UserControl Presentation => null;

        public void PreExecution()
        {
            currentAttempt = 0;
            Success = true;

            AmountOfOptions = settings._AmountOfOptions;

            ProgressChanged(0, 1);
        }

        public void Execute()
        {
            if (currentAttempt == 0)
            {
                RateOfSuccess = Math.Pow(settings._AmountOfOptions, -settings.AmountOfAttempts);
                OnPropertyChanged("RateOfSuccess");
            }

            if (currentAttempt > 0 && OutputRandom != Input)
            {
                Success = false;
            }

            if (currentAttempt >= settings.AmountOfAttempts || !Success)
            {
                OnPropertyChanged("Success");
                ProgressChanged(1, 1);
                return;
            }

            OutputRandom = BigIntegerHelper.RandomIntLimit(settings._AmountOfOptions);
            OnPropertyChanged("AmountOfOptions");
            OnPropertyChanged("OutputRandom");

            currentAttempt++;
            ProgressChanged(currentAttempt, settings.AmountOfAttempts);
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

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