/*                          
   Copyright 2025 Nils Kopal, CrypTool Project

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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace CrypTool.Plugins.EllipticCurveCryptography
{
    public enum ModeEnum
    {
        ReturnGenerator,
        ReturnCustomPoint
    }

    public class ECCInputSettings : ISettings
    {
        #region Private Variables

        private string _selectedCurve = "curve25519";
        private ObservableCollection<string> _predefinedCurves = new ObservableCollection<string>();
        private ModeEnum _mode = ModeEnum.ReturnGenerator;

        private string _a = "2";
        private string _b = "2";
        private string _mod = "17";

        private string _x = "5";
        private string _y = "1";

        #endregion

        public ECCInputSettings()
        {
            PredefinedCurvesNames.Clear();
            PredefinedCurvesNames.Add(Properties.Resources.UserDefinedCurve);
            foreach (string curveName in PredefinedEllipticCurves.Curves.Keys)
            {
                PredefinedCurvesNames.Add(curveName);
            }
        }

        #region TaskPane Settings

        [DontSave]
        public ObservableCollection<string> PredefinedCurvesNames
        {
            get => _predefinedCurves;
            set
            {
                if (value != _predefinedCurves)
                {
                    _predefinedCurves = value;
                    OnPropertyChanged(nameof(PredefinedCurvesNames));
                }
            }
        }

        [TaskPane("SelectedCurveCaption", "SelectedCurveTooltip", "CurveSelectionGroup", 0, false, ControlType.DynamicComboBox, new string[] { nameof(PredefinedCurvesNames) })]
        public int SelectedCurve
        {
            get => PredefinedCurvesNames.IndexOf(_selectedCurve);
            set
            {
                if (PredefinedCurvesNames.IndexOf(_selectedCurve) != value)
                {
                    _selectedCurve = PredefinedCurvesNames[value];
                    OnPropertyChanged(nameof(SelectedCurve));
                    UpdateTaskPaneVisibility();
                }
            }
        }

        [TaskPane("ModeCaption", "ModeTooltip", "CurveSelectionGroup", 1, false, ControlType.ComboBox, new[] { "UseGenerator", "EnterCustomPoint" })]
        public ModeEnum Mode
        {
            get => _mode;
            set
            {
                if (_mode != value)
                {
                    _mode = value;
                    OnPropertyChanged(nameof(Mode));
                    UpdateTaskPaneVisibility();
                }
            }
        }

        [TaskPane("ACaption", "ATooltip", "CurveParametersGroup", 2, false, ControlType.TextBox)]
        public string A
        {
            get => _a;
            set
            {
                if (_a != value)
                {
                    _a = value;
                    OnPropertyChanged(nameof(A));
                }
            }
        }

        [TaskPane("BCaption", "BTooltip", "CurveParametersGroup", 3, false, ControlType.TextBox)]
        public string B
        {
            get => _b;
            set
            {
                if (_b != value)
                {
                    _b = value;
                    OnPropertyChanged(nameof(B));
                }
            }
        }

        [TaskPane("ModCaption", "ModTooltip", "CurveParametersGroup", 4, false, ControlType.TextBox)]
        public string Mod
        {
            get => _mod;
            set
            {
                if (_mod != value)
                {
                    _mod = value;
                    OnPropertyChanged(nameof(Mod));
                }
            }
        }

        [TaskPane("XCaption", "XTooltip", "PointParametersGroup", 5, false, ControlType.TextBox)]
        public string X
        {
            get => _x;
            set
            {
                if (_x != value)
                {
                    _x = value;
                    OnPropertyChanged(nameof(X));
                }
            }
        }

        [TaskPane("YCaption", "YTooltip", "PointParametersGroup", 6, false, ControlType.TextBox)]
        public string Y
        {
            get => _y;
            set
            {
                if (_y != value)
                {
                    _y = value;
                    OnPropertyChanged(nameof(Y));
                }
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);

        #endregion

        #region Interface Implementation

        public void Initialize()
        {
            UpdateTaskPaneVisibility();
        }

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
            {
                return;
            }

            ChangeTaskPaneVisibility(nameof(A), Visibility.Collapsed);
            ChangeTaskPaneVisibility(nameof(B), Visibility.Collapsed);
            ChangeTaskPaneVisibility(nameof(Mod), Visibility.Collapsed);
            ChangeTaskPaneVisibility(nameof(X), Visibility.Collapsed);
            ChangeTaskPaneVisibility(nameof(Y), Visibility.Collapsed);

            if (Mode == ModeEnum.ReturnCustomPoint)
            {
                ChangeTaskPaneVisibility(nameof(X), Visibility.Visible);
                ChangeTaskPaneVisibility(nameof(Y), Visibility.Visible);
            }
            if (SelectedCurve.Equals(_predefinedCurves.IndexOf(Properties.Resources.UserDefinedCurve)))
            {
                ChangeTaskPaneVisibility(nameof(A), Visibility.Visible);
                ChangeTaskPaneVisibility(nameof(B), Visibility.Visible);
                ChangeTaskPaneVisibility(nameof(Mod), Visibility.Visible);
            }
        }

        private void ChangeTaskPaneVisibility(string settingsName, Visibility visibility)
        {
            TaskPaneAttributeChanged.Invoke(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(settingsName, visibility)));
        }

        #endregion
    }
}