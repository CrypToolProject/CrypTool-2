/*
   Copyright 2024 Nils Kopal <Nils.Kopal<at>CrypTool.org

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

namespace CrypTool.Plugins.CryptoBoxCipher
{
    /// <summary>
    /// The action to perform
    /// </summary>
    public enum Action
    {
        Encrypt,
        Decrypt
    }

    /// <summary>
    /// The padding to use
    /// </summary>
    public enum Padding
    {
        X,
        SPACE,
    }

    /// <summary>
    /// How to handle spaces
    /// </summary>
    public enum SpaceHandling
    {
        ReplaceX,
        Leave,
        Remove
    }

    /// <summary>
    /// The settings for the CryptoBoxCipher plugin
    /// </summary>
    public class CryptoBoxCipherSettings : ISettings
    {
        #region Private Variables

        private Action _action = Action.Encrypt;
        private Padding _padding = Padding.X;
        private SpaceHandling _spaceHandling = SpaceHandling.ReplaceX;

        #endregion

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

        [TaskPane("PaddingCaption", "PaddingTooltip", null, 2, false, ControlType.ComboBox, new string[] { "X", "Space" })]
        public Padding Padding
        {
            get
            {
                return _padding;
            }
            set
            {
                if (_padding != value)
                {
                    _padding = value;
                    OnPropertyChanged(nameof(Padding));
                }
            }
        }

        [TaskPane("SpaceHandlingCaption", "SpaceHandlingTooltip", null, 3, false, ControlType.ComboBox, new string[] { "ReplaceX", "Leave", "Remove" })]
        public SpaceHandling SpaceHandling
        {
            get
            {
                return _spaceHandling;
            }
            set
            {
                if (_spaceHandling != value)
                {
                    _spaceHandling = value;
                    OnPropertyChanged(nameof(SpaceHandling));
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

        }
    }
}