/*  
   Copyright 2011 Selim Arikan, Istanbul University

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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrypTool.PluginBase;
using System.ComponentModel;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using System.Windows.Controls;

namespace CrypTool.Plugins.PasswordStrength
{
    [Author("Selim Arikan", "selimarikan@windowslive.com", "Istanbul University, Electrics-Electronics Engineering", "http://www.selimarikan.com")]    
    [PluginInfo("Password Strength", "Determine the strength of the password given.", null, new[] { "PasswordStrength/Images/password.png" })]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class PasswordStrength : ICrypComponent
    {
        #region Private Variables
        
        private readonly PasswordStrengthSettings settings = new PasswordStrengthSettings();
        #endregion

        
        #region Data Properties

        [PropertyInfo(Direction.InputData, "Password Input", "Enter your password for measuring its strength.", true)]
        public string PasswordInput
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "Strength", "Strength of the password given.", true)]
        public int StrengthOutput 
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return this.settings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        public int MeasureStrength(string pass)
        {
            int iTypeMultiplier = 0, iPoint = 0;
            int iNumberAmount = 0, iLowerCharAmount = 0, iUpperCharAmount = 0, iSpecialAmount = 0;

            char[] chPassArray = pass.ToCharArray();

            //Character ranges.
            for (int i = 0; i < chPassArray.Length; i++)
            {
                if ((chPassArray[i] >= 0x41) && (chPassArray[i] <= 0x5A))
                    iUpperCharAmount++;

                if ((chPassArray[i] >= 0x61) && (chPassArray[i] <= 0x7A))
                    iLowerCharAmount++;

                if ((chPassArray[i] >= 0x30) && (chPassArray[i] <= 0x39))
                    iNumberAmount++;
                 //Special Character range
                if ((chPassArray[i] >= 0x20) && (chPassArray[i] <= 0x2F))
                    iSpecialAmount++;

                if ((chPassArray[i] >= 0x3A) && (chPassArray[i] <= 0x40))
                    iSpecialAmount++;

                if ((chPassArray[i] >= 0x5B) && (chPassArray[i] <= 0x60))
                    iSpecialAmount++;

                if ((chPassArray[i] >= 0x7B) && (chPassArray[i] <= 0x7E))
                    iSpecialAmount++;

                if ((chPassArray[i] >= 0x80) && (chPassArray[i] <= 0xFF))
                    iSpecialAmount++;

            }


            if (iLowerCharAmount != 0)
                iTypeMultiplier++;

            if (iUpperCharAmount != 0)
                iTypeMultiplier++;

            if (iNumberAmount != 0)
                iTypeMultiplier++;

            if (iSpecialAmount != 0)
                iTypeMultiplier++;

            iPoint = ((iTypeMultiplier * 3) + chPassArray.Length) * 3;

            return iPoint;
        }


        public void Execute()
        {
            if (PasswordInput != null)
            {
                ProgressChanged(0, 0);
                StrengthOutput = MeasureStrength(PasswordInput);
                ProgressChanged(1, 1);
                OnPropertyChanged("StrengthOutput");
            }
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
