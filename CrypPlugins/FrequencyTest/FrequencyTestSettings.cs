/*  
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
using System;
using System.ComponentModel;
using System.Windows;

namespace CrypTool.FrequencyTest
{
    public enum AnalysisMode
    {
        NGrams,
        SymbolSeparated
    }

    public class FrequencyTestSettings : ISettings
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            UpdateSettingsVisibility();
        }

        /// <summary>
        /// Upon changing the analysis mode, the visibility of the settings elements must be updated
        /// </summary>
        private void UpdateSettingsVisibility()
        {
            if (_analysisMode == AnalysisMode.NGrams)
            {
                showSettingsElement("GrammLength");
                showSettingsElement("BoundaryFragments");
                hideSettingsElement("SymbolSeparators");
            }
            else
            {
                hideSettingsElement("GrammLength");
                hideSettingsElement("BoundaryFragments");
                showSettingsElement("SymbolSeparators");
            }
        }


        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        #region Private variables

        private int unknownSymbolHandling = 0;
        private int caseSensitivity = 0;
        private int grammLength = 1;
        private int boundaryFragments = 0;
        private bool autozoom = true;
        private int chartHeight = 160;
        private int scale = 10000; // = 1 , factor of 10000        
        private bool sortFrequencies = false;
        private bool countOverlapping = true;
        private int maxNumberOfShownNGramms = 30;
        private bool showAbsoluteValues = false;
        private bool showTotal = false;
        private AnalysisMode _analysisMode = AnalysisMode.NGrams;
        private string _symbolSeparators = ".,:;";

        #endregion

        #region Private helper methods

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

        #region Public events and methods

        /// <summary>
        /// This event is needed in order to render settings elements visible/invisible
        /// </summary>
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        #endregion

        #region Visible settings

        [PropertySaveOrder(0)]
        [TaskPane("AnalysisModeCaption", "AnalysisModeTooltip", null, 0, false, ControlType.ComboBox, new string[] { "NGrams", "SymbolSeparated" })]
        public AnalysisMode AnalysisMode
        {
            get => _analysisMode;
            set
            {
                if (value != _analysisMode)
                {
                    _analysisMode = value;
                    UpdateSettingsVisibility();
                    OnPropertyChanged("AnalysisMode");
                }
            }
        }

        /// <summary>
        /// Visible setting how to deal with alphabet case. 0 = case insentive, 1 = case sensitive
        /// </summary>
        [PropertySaveOrder(1)]
        [TaskPane("CaseSensitivityCaption", "CaseSensitivityTooltip", null, 7, false, ControlType.ComboBox, new string[] { "CaseSensitivityList1", "CaseSensitivityList2" })]
        public int CaseSensitivity
        {
            get => caseSensitivity;
            set
            {
                if (value != caseSensitivity)
                {
                    caseSensitivity = value;
                    OnPropertyChanged("CaseSensitivity");
                }
            }
        }     

        [PropertySaveOrder(2)]
        [TaskPane("SymbolSeparatorsCaption", "SymbolSeparatorsTooltip", null, 3, false, ControlType.TextBox)]
        public string SymbolSeparators
        {
            get => _symbolSeparators;
            set
            {
                if (value != _symbolSeparators)
                {
                    _symbolSeparators = value;
                    OnPropertyChanged("SymbolSeparators");
                }
            }
        }
       
        [PropertySaveOrder(3)]
        [TaskPane("GrammLengthCaption", "GrammLengthTooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int GrammLength
        {
            get => grammLength;
            set
            {
                if (value != grammLength)
                {
                    grammLength = value;
                    OnPropertyChanged("GrammLength");
                }
            }
        }

        [PropertySaveOrder(8)]
        [TaskPane("SortFrequenciesCaption", "SortFrequenciesTooltip", null, 8, false, ControlType.CheckBox)]
        public bool SortFrequencies
        {
            get => sortFrequencies;
            set
            {
                if (value != sortFrequencies)
                {
                    sortFrequencies = value;
                    OnPropertyChanged("SortFrequencies");
                }
            }
        }

        [PropertySaveOrder(9)]
        [TaskPane("CountOverlappingCaption", "CountOverlappingTooltip", null, 9, false, ControlType.CheckBox)]
        public bool CountOverlapping
        {
            get => countOverlapping;
            set
            {
                if (countOverlapping != value)
                {
                    countOverlapping = value;
                    OnPropertyChanged("CountOverlapping");
                }
            }
        }

        [PropertySaveOrder(9)]
        [TaskPane("ShowAbsoluteValuesCaption", "ShowAbsoluteValuesTooltip", null, 10, false, ControlType.CheckBox)]
        public bool ShowAbsoluteValues
        {
            get => showAbsoluteValues;
            set
            {
                if (showAbsoluteValues != value)
                {
                    showAbsoluteValues = value;
                    OnPropertyChanged("ShowAbsoluteValues");
                }
            }
        }

        [PropertySaveOrder(10)]
        [TaskPane("ShowTotalCaption", "ShowTotalTooltip", null, 11, false, ControlType.CheckBox)]
        public bool ShowTotal
        {
            get => showTotal;
            set
            {
                if (showTotal != value)
                {
                    showTotal = value;
                    OnPropertyChanged("ShowTotal");
                }
            }
        }

        [PropertySaveOrder(11)]
        [TaskPane("MaxNumberOfShownNGrammsCaption", "MaxNumberOfShownNGrammsTooltip", null, 12, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
        public int MaxNumberOfShownNGramms
        {
            get => maxNumberOfShownNGramms;
            set
            {
                if (maxNumberOfShownNGramms != value)
                {
                    maxNumberOfShownNGramms = value;
                    OnPropertyChanged("MaxNumberOfShownNGramms");
                }
            }
        }

        [PropertySaveOrder(3)]
        [ContextMenu("ProcessUnknownSymbolsCaption", "ProcessUnknownSymbolsTooltip", 4, ContextMenuControlType.ComboBox, null, new string[] { "ProcessUnknownSymbolsList1", "ProcessUnknownSymbolsList2" })]
        [TaskPane("ProcessUnknownSymbolsCaption", "ProcessUnknownSymbolsTooltip", null, 4, false, ControlType.ComboBox, new string[] { "ProcessUnknownSymbolsList1", "ProcessUnknownSymbolsList2" })]
        public int ProcessUnknownSymbols
        {
            get => unknownSymbolHandling;
            set
            {
                if (value != unknownSymbolHandling)
                {
                    unknownSymbolHandling = value;
                    OnPropertyChanged("ProcessUnknownSymbols");
                }
            }
        }

        /// <summary>
        /// This option is to choose whether additional n-grams shall be used at word boundary for n-grams with n>=2.
        /// Example trigrams for the word "cherry":
        /// che
        /// her
        /// err
        /// rry
        /// The following fragments at word boundary may be included optionally:
        /// __c
        /// _ch
        /// ry_
        /// y__
        /// The underline char represents a whitespace.
        /// </summary>
        [PropertySaveOrder(4)]
        [TaskPane("BoundaryFragmentsCaption", "BoundaryFragmentsTooltip", null, 12, false, ControlType.ComboBox, new string[] { "BoundaryFragmentsList1", "BoundaryFragmentsList2" })]
        public int BoundaryFragments
        {
            get => boundaryFragments;
            set
            {
                if (value != boundaryFragments)
                {
                    boundaryFragments = value;
                    OnPropertyChanged("BoundaryFragments");
                }
            }
        }

        [PropertySaveOrder(5)]
        [TaskPane("AutozoomCaption", "AutozoomTooltip", "PresentationGroup", 20, true, ControlType.CheckBox)]
        public bool Autozoom
        {
            get => autozoom;
            set
            {
                if (value != autozoom)
                {
                    autozoom = value;

                    if (autozoom)
                    {
                        hideSettingsElement("ChartHeight");
                    }
                    else
                    {
                        showSettingsElement("ChartHeight");
                    }

                    OnPropertyChanged("Autozoom");
                }
            }
        }


        [PropertySaveOrder(6)]
        [TaskPane("ChartHeightCaption", "ChartHeightTooltip", "PresentationGroup", 21, true, ControlType.NumericUpDown, ValidationType.RangeInteger, 10, 1000)]
        public int ChartHeight
        {
            get => chartHeight;
            set
            {
                if (value != chartHeight)
                {
                    chartHeight = value;
                    OnPropertyChanged("ChartHeight");
                }
            }
        }


        [PropertySaveOrder(7)]
        [TaskPane("ScaleCaption", "ScaleTooltip", "PresentationGroup", 22, true, ControlType.Slider, 5, 20000)]
        public int Scale
        {
            get => scale;
            set
            {
                if (scale != value)
                {
                    scale = value;
                    OnPropertyChanged("Scale");
                }
            }
        }

        #endregion

    }
}
