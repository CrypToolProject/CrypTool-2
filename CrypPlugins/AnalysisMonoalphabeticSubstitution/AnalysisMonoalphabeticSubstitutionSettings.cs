/*
   Copyright 2024 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Windows;

namespace CrypTool.AnalysisMonoalphabeticSubstitution
{
    public class AnalysisMonoalphabeticSubstitutionSettings : ISettings
    {
        #region Private Variables

        private readonly bool hasChanges = false;
        private int language = 0;
        private readonly int language2 = 0;
        private bool useSpaces = true;
        private int treatmentInvalidChars = 0;
        private int chooseAlgorithm = 0;
        private int restarts = 100;
        private int _gramsType = 4; // Pentagrams

        #endregion

        #region Initialization / Constructor

        public void Initialize()
        {
        }

        #endregion

        #region TaskPane Settings

        [TaskPane("ChooseAlgorithmCaption", "ChooseAlgorithmTooltip", "SelectAlgorithmGroup", 1, false, ControlType.ComboBox, new string[] { "ChooseAlgorithmList1", "ChooseAlgorithmList2" })]
        public int ChooseAlgorithm
        {
            get => chooseAlgorithm;
            set
            {
                chooseAlgorithm = value;
                OnPropertyChanged("ChooseAlgorithm");
            }
        }

        [TaskPane("RestartsCaption", "RestartsTooltip", "SelectAlgorithmGroup", 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 10000000)]
        public int Restarts
        {
            get => restarts;
            set
            {
                if (value != restarts)
                {
                    restarts = value;
                    OnPropertyChanged("Restarts");
                }
            }
        }

        [TaskPane("ChooseAlphabetCaption", "ChooseAlphabetTooltip", "AlphabetGroup", 1, false, ControlType.LanguageSelector)]
        public int Language
        {
            get => language;
            set => language = value;
        }

        [TaskPane("UseSpacesCaption", "UseSpacesTooltip", "AlphabetGroup", 2, false, ControlType.CheckBox)]
        public bool UseSpaces
        {
            get => useSpaces;
            set => useSpaces = value;
        }

        [TaskPane("GramsTypeCaption", "GramsTypeTooltip", "AlphabetGroup", 3, false, ControlType.ComboBox,
            new string[] { "Unigrams", "Bigrams", "Trigrams", "Tetragrams", "Pentagrams" })]
        public int GramsType
        {
            get => _gramsType;
            set
            {
                if (value != _gramsType)
                {
                    _gramsType = value;
                    OnPropertyChanged("GramsType");
                }
            }
        }


        [TaskPane("TreatmentInvalidCharsCaption", "TreatmentInvalidCharsTooltip", "AdvancedSettingsGroup", 4, false, ControlType.ComboBox, new string[] { "ChooseInvalidCharsList1", "ChooseInvalidCharsList2", "ChooseInvalidCharsList3" })]
        public int TreatmentInvalidChars
        {
            get => treatmentInvalidChars;
            set
            {
                treatmentInvalidChars = value;
                OnPropertyChanged("TreatmentInvalidChars");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        #endregion

        #region Helper Functions

        private void showSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
            }
        }

        private void hideSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
            }
        }

        #endregion
    }
}
