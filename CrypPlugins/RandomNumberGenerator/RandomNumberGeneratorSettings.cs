/*
   Copyright 2018 CrypTool 2 Team <ct2contact@CrypTool.org>
   Author: Christian Bender, Universität Siegen
           Nils Kopal, CrypTool 2 Team

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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.Plugins.RandomNumberGenerator
{
    /// <summary>
    /// Type of random number generator
    /// </summary>
    public enum AlgorithmType
    {
        RandomRandom = 0,               //.net Random.Random
        RNGCryptoServiceProvider = 1,   //.net RNGCryptoServiceProvider
        X2modN = 2,
        LCG = 3,
        ICG = 4,
        SubtractiveGenerator = 5,
        XORShift = 6
    }

    public enum XORShiftType
    {
        XOR_Shift8 = 0,
        XOR_Shift16 = 1,
        XOR_Shift32 = 2,
        XOR_Shift64 = 3
    }

    public enum OutputType
    {
        ByteArray = 0,
        CrypToolStream = 1,
        Number = 2,
        NumberArray = 3,
        Bool
    }


    public class RandomNumberGeneratorSettings : ISettings
    {
        #region Private Variables

        private readonly Dictionary<AlgorithmType, List<string>> _settingsVisibility = new Dictionary<AlgorithmType, List<string>>();
        private readonly List<string> _settingsList = new List<string>();

        private AlgorithmType _AlgorithmType = 0;
        private XORShiftType _XORShiftType = XORShiftType.XOR_Shift32;
        private OutputType _OutputType = 0;

        private string _OutputLength = string.Empty;
        private string _OutputAmount = string.Empty;
        private string _Seed = "42";
        private string _Modulus = string.Empty;
        private string _a = string.Empty;
        private string _b = string.Empty;

        private bool _nonZeroBytes = false;

        #endregion

        public RandomNumberGeneratorSettings()
        {
            foreach (AlgorithmType type in Enum.GetValues(typeof(AlgorithmType)))
            {
                _settingsVisibility[type] = new List<string>();
            }
        }

        #region TaskPane Settings

        [TaskPane("AlgorithmTypeCaption", "AlgorithmTypeTooltip", "GeneralSettingsGroup", 0, false, ControlType.ComboBox, new string[] { "Random.Random", "RNGCryptoServiceProvider", "X^2 mod N", "LCG", "ICG", "Subtractive Generator", "XORShift" })]
        public AlgorithmType AlgorithmType
        {
            get => _AlgorithmType;
            set
            {
                _AlgorithmType = value;
                UpdateTaskPaneVisibility();
                OnPropertyChanged("AlgorithmType");
            }
        }

        [TaskPane("XORShiftTypeCaption", "XORShiftTypeTooltip", "GeneralSettingsGroup", 1, false, ControlType.ComboBox, new string[] { "XORShift8", "XORShift16", "XORShift32", "XORShift64" })]
        public XORShiftType XORShiftType
        {
            get => _XORShiftType;
            set
            {
                _XORShiftType = value;
                UpdateTaskPaneVisibility();
                OnPropertyChanged("XORShiftType");
            }
        }

        [TaskPane("NonZeroBytesCaption", "NonZeroBytesTooltip", "GeneralSettingsGroup", 1, false, ControlType.CheckBox)]
        public bool NonZeroBytes
        {
            get => _nonZeroBytes;
            set
            {
                _nonZeroBytes = value;
                UpdateTaskPaneVisibility();
                OnPropertyChanged("NonZeroBytes");
            }
        }

        [TaskPane("OutputTypeCaption", "OutputTypeTooltip", "GeneralSettingsGroup", 2, false, ControlType.ComboBox, new string[] { "Byte Array", "CrypToolStream", "Number", "Number Array", "Bool" })]
        public OutputType OutputType
        {
            get => _OutputType;
            set
            {
                _OutputType = value;
                UpdateTaskPaneVisibility();
                OnPropertyChanged("OutputType");

            }
        }

        [TaskPane("OutputLengthCaption", "OutputLengthTooltip", "GeneralSettingsGroup", 3, false, ControlType.TextBox)]
        public string OutputLength
        {
            get => _OutputLength;
            set
            {
                _OutputLength = value;
                OnPropertyChanged("OutputLength");
            }
        }

        [TaskPane("OutputAmountCaption", "OutputAmountTooltip", "GeneralSettingsGroup", 4, false, ControlType.TextBox)]
        public string OutputAmount
        {
            get => _OutputAmount;
            set
            {
                _OutputAmount = value;
                OnPropertyChanged("OutputAmount");
            }
        }


        [TaskPane("SeedCaption", "SeedTooltip", "AlgorithmSettingsGroup", 0, false, ControlType.TextBox)]
        public string Seed
        {
            get => _Seed;
            set
            {
                _Seed = value;
                OnPropertyChanged("Seed");
            }
        }

        [TaskPane("ModulusCaption", "ModulusTooltip", "AlgorithmSettingsGroup", 1, false, ControlType.TextBox)]
        public string Modulus
        {
            get => _Modulus;
            set
            {
                _Modulus = value;
                OnPropertyChanged("Modulus");
            }
        }

        [TaskPane("aCaption", "aTooltip", "AlgorithmSettingsGroup", 2, false, ControlType.TextBox)]
        public string a
        {
            get => _a;
            set
            {
                _a = value;
                OnPropertyChanged("a");
            }
        }

        [TaskPane("bCaption", "aTooltip", "AlgorithmSettingsGroup", 3, false, ControlType.TextBox)]
        public string b
        {
            get => _b;
            set
            {
                _b = value;
                OnPropertyChanged("b");
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion

        public void Initialize()
        {
            _settingsList.Add("Seed");
            _settingsList.Add("Modulus");
            _settingsList.Add("a");
            _settingsList.Add("b");
            _settingsList.Add("XORShiftType");
            _settingsList.Add("NonZeroBytes");
            _settingsList.Add("Seed");
            _settingsVisibility[AlgorithmType.RandomRandom].Add("Seed");
            _settingsVisibility[AlgorithmType.RNGCryptoServiceProvider].Add("NonZeroBytes");
            _settingsVisibility[AlgorithmType.X2modN].Add("Seed");
            _settingsVisibility[AlgorithmType.X2modN].Add("Modulus");
            _settingsVisibility[AlgorithmType.ICG].Add("Seed");
            _settingsVisibility[AlgorithmType.ICG].Add("Modulus");
            _settingsVisibility[AlgorithmType.ICG].Add("a");
            _settingsVisibility[AlgorithmType.ICG].Add("b");
            _settingsVisibility[AlgorithmType.LCG].Add("Seed");
            _settingsVisibility[AlgorithmType.LCG].Add("Modulus");
            _settingsVisibility[AlgorithmType.LCG].Add("a");
            _settingsVisibility[AlgorithmType.LCG].Add("b");
            _settingsVisibility[AlgorithmType.SubtractiveGenerator].Add("Seed");
            _settingsVisibility[AlgorithmType.XORShift].Add("XORShiftType");
            _settingsVisibility[AlgorithmType.XORShift].Add("Seed");
            UpdateTaskPaneVisibility();
        }

        private void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
            {
                return;
            }

            foreach (TaskPaneAttribteContainer tpac in _settingsList.Select(operation => new TaskPaneAttribteContainer(operation, (_settingsVisibility[AlgorithmType].Contains(operation)) ? Visibility.Visible : Visibility.Collapsed)))
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(tpac));
            }

            if (_OutputType == OutputType.NumberArray)
            {
                TaskPaneAttribteContainer tpac = new TaskPaneAttribteContainer("OutputAmount", Visibility.Visible);
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(tpac));
            }
            else
            {
                TaskPaneAttribteContainer tpac = new TaskPaneAttribteContainer("OutputAmount", Visibility.Collapsed);
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(tpac));
            }
        }

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
    }
}
