/*
   Copyright CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using static CrypTool.ACACiphersLib.Cipher;

namespace CrypTool.Plugins.AcaCiphers
{

    public enum Action
    {
        Encrypt,
        Decrypt
    }

    public enum NullType
    {
        FirstLetter,
        MiddleLetter,
        LastLetter
    }


    public class AcaCiphersSettings : ISettings
    {
        #region Private Variables

        private CipherType AlgorithmType = CipherType.PORTA;
        private Action _action = Action.Encrypt;
        private NullType _nullmode = NullType.MiddleLetter;
        private bool _maptext = true;
        public List<string> parameter = new List<string>();

        #endregion

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        #region TaskPane Settings


        [TaskPane("Cipher type", "Select cipher types", null, 1, false, ControlType.ComboBox, new string[] {"AMSCO",
        "AUTOKEY",
        "BACONIAN",
        "BAZERIES",
        "BEAUFORT",
        "BIFID",
        "CADENUS",
        "CHECKERBOARD",
        "COMPLETE_COLUMNAR_TRANSPOSITION",
        "CONDI",
        "CM_BIFID",
        "DIGRAFID",
        "FOURSQUARE",
        "FRANCTIONATED_MORSE",
        "GRANDPRE",
        "GRILLE",
        "GROMARK",
        "GRONSFELD",
        "HEADLINES",
        "HOMOPHONIC",
        "INCOMPLETE_COLUMNAR",
        "INTERRUPTED_KEY",
        "KEY_PHRASE",
        "MONOME_DINOME",
        "MORBIT",
        "MYSZKOWSKI",
        "NICODEMUS",
        "NIHILIST_SUBSTITUTION",
        "NIHILIST_TRANSPOSITION",
        "NULL",
        "NUMBERED_KEY",
        "PERIODIC_GROMARK",
        "PHILLIPS",
        "PHILLIPS_RC",
        "PLAYFAIR",
        "POLLUX",
        "PORTA",
        "PORTAX",
        "PROGRESSIVE_KEY",
        "QUAGMIRE_I",
        "QUAGMIRE_II",
        "QUAGMIRE_III",
        "QUAGMIRE_VI",
        "RAGBABY",
        "RAILFENCE",
        "REDEFENSE",
        "ROUTE_TRANSPOSITION",
        "RUNNING_KEY",
        "SERIATED_PLAYFAIR",
        "SLIDEFAIR",
        "SWAGMAN",
        "SYLLABARY",
        "TRIDIGITAL",
        "TRIFID",
        "TRI_SQUARE",
        "TWIN_BIFID",
        "TWIN_TRIFID",
        "TWO_SQUARE",
        "VARIANT",
        "VIGENERE"})]
        public CipherType AcaCipherType
        {
            get => AlgorithmType;
            set
            {
                if (AlgorithmType != value)
                {
                    AlgorithmType = value;
                    OnPropertyChanged("AcaCipherType");
                }
            }
        }

        [TaskPane("Encrypt or decrypt switch", "Switch encrypt and decrypt", null, 2, false, ControlType.ComboBox,
            new string[] { "Encrypt", "Decrypt" })]
        public Action AcaMode
        {
            get => _action;
            set
            {
                if (_action != value)
                {
                    _action = value;
                    OnPropertyChanged("AcaMode");
                }
            }
        }

        [TaskPane("Encrypt or decrypt switch", "Switch encrypt and decrypt","Null cipher", 2, false, ControlType.ComboBox,
            new string[] { "First letter", "Middle letter", "Last letter" })]
        public NullType NullMode
        {
            get => _nullmode;
            set
            {
                if (_nullmode != value)
                {
                    _nullmode = value;
                    AddParameter(_nullmode.ToString());
                    OnPropertyChanged("NullMode");
                }
            }
        }

        [TaskPane("Map number into textspace", "Switch automatic mapping from number into textspace", null, 2, false, ControlType.CheckBox)]
        public bool MapIntoTextSpace
        {
            get => _maptext;
            set
            {
                if (_maptext != value)
                {
                    _maptext = value;
                    OnPropertyChanged("MapIntoTextSpace");
                }
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
            UpdateSettingsVisibility();
        }

        #endregion

        public void Initialize()
        {
            UpdateSettingsVisibility();
            AddParameter(_nullmode.ToString());
        }

        private void UpdateSettingsVisibility()
        {
            //null cipher settings
            if (AlgorithmType == CipherType.NULL)
            {
                ShowHideSetting("NullMode",true);
            }
            else
            {
                ShowHideSetting("NullMode", false);
            }

            if (AlgorithmType == CipherType.HOMOPHONIC)
            {
                _maptext = false;
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

        public void AddParameter(string single_parameter)
        {
            parameter.Add(single_parameter);
        }
    }
}

