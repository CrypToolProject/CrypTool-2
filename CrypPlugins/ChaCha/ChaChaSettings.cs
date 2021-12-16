/*
   Copyright CrypTool 2 Team <ct2contact@CrypTool.org>

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

namespace CrypTool.Plugins.ChaCha
{
    public class ChaChaSettings : ISettings
    {
        #region Private Variables

        private int _rounds = 20;
        private Version _version;

        #endregion Private Variables

        #region TaskPane Settings

        [TaskPane("RoundCaption", "RoundTooltip", null, 0, false, ControlType.ComboBox, new string[] { "8", "12", "20" })]
        public int RoundsIndex
        {
            get
            {
                // We need to return the index of the entry we want to display.
                switch (Rounds)
                {
                    case 8: return 0;
                    case 12: return 1;
                    case 20: return 2;
                    default: return 2;
                }
            }
            set
            {
                // The CT2 environment calls this setter with the index thus we map the indices to the actual round value.
                switch (value)
                {
                    case 0:
                        Rounds = 8;
                        break;

                    case 1:
                        Rounds = 12;
                        break;

                    case 2:
                        Rounds = 20;
                        break;
                }
                OnPropertyChanged("Rounds");
            }
        }

        public int Rounds
        {
            get => _rounds;
            set
            {
                _rounds = value;
                OnPropertyChanged("Rounds");
            }
        }

        [TaskPane("VersionCaption", "VersionTooltip", null, 0, false, ControlType.ComboBox, new string[] { "DJB", "IETF" })]
        public int VersionIndex
        {
            get => Version.Name == Version.DJB.Name ? 0 : 1;
            set
            {
                Version selectedVersion = value == 0 ? Version.DJB : Version.IETF;
                if (Version.Name != selectedVersion.Name)
                {
                    Version = selectedVersion;
                    OnPropertyChanged("IntVersion");
                }
            }
        }

        public Version Version
        {
            get
            {
                if (_version == null)
                {
                    _version = Version.DJB;
                }
                return _version;
            }
            private set
            {
                if (_version.Name != value.Name)
                {
                    _version = value;
                    OnPropertyChanged("Version");
                }
            }
        }

        #endregion TaskPane Settings

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion Events

        public void Initialize()
        {
        }

        public override string ToString()
        {
            return string.Format(Properties.Resources.SettingsToString, Rounds, Version.Name);
        }
    }
}