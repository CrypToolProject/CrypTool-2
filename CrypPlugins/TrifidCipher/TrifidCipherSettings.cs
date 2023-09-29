/*
   Copyright 2022 Nils Kopal <Nils.Kopal<at>CrypTool.org

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

namespace CrypTool.Plugins.TrifidCipher
{
    public enum Action
    {
        Encrypt,
        Decrypt
    }
  

    public class TrifidCipherSettings : ISettings
    {
        #region Private Variables

        private Action _action = Action.Encrypt;
        private bool _enablePeriod = false;
        private int _period = 5;

        #endregion

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        #region TaskPane Settings

        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
        public Action Action
        {
            get
            {
                return _action;
            }
            set
            {
                if (_action != value)
                {
                    _action = value;
                    OnPropertyChanged(nameof(Action));
                }
            }
        }

        [TaskPane("EnablePeriodCaption", "EnablePeriodTooltip", null, 2, false, ControlType.CheckBox)]
        public bool EnablePeriod
        {
            get
            {
                return _enablePeriod;
            }
            set
            {
                if (_enablePeriod != value)
                {
                    _enablePeriod = value;
                    OnPropertyChanged(nameof(EnablePeriod));
                    UpdateSettingsVisibility();
                }
            }
        }

        [TaskPane("PeriodCaption", "PeriodTooltip", null, 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
        public int Period
        {
            get
            {
                return _period;
            }
            set
            {
                if (_period != value)
                {
                    _period = value;
                    OnPropertyChanged(nameof(Period));
                }
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
            UpdateSettingsVisibility();
        }

        private void UpdateSettingsVisibility()
        {
            if (_enablePeriod == true)
            {
                ShowHideSetting(nameof(Period), true);
            }
            else
            {
                ShowHideSetting(nameof(Period), false);
            }
        }

        private void ShowHideSetting(string propertyName, bool show)
        {
            if (TaskPaneAttributeChanged == null)
            {
                return;
            }

            TaskPaneAttribteContainer container = new TaskPaneAttribteContainer(propertyName, show == true ? Visibility.Visible : Visibility.Collapsed);
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(container));
        }
    }
}
