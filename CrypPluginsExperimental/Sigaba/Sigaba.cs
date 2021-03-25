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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Control;
using CrypTool.PluginBase.Miscellaneous;


namespace Sigaba
{
    [Author("Julian Weyers", "weyers@CrypTool.org", "CrypTool 2 Team", "http://CrypTool2.vs.uni-due.de")]
    [PluginInfo("Sigaba.Properties.Resources", "PluginCaption", "PluginToolTip", "Enigma/DetailedDescription/doc.xml", "Sigaba/Images/Icon.png", "Enigma/Images/encrypt.png", "Enigma/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Sigaba : ICrypComponent
    {
        public readonly SigabaSettings _settings = new SigabaSettings();
        public readonly SigabaCore _core;
        public readonly SigabaCoreFast _fastCore;

        public static bool verbose = false;

        #region Constructor 
        public Sigaba()
        {
            SigabaPresentation sigpa = new SigabaPresentation(this,_settings);
            Presentation = sigpa;
            this._settings.PropertyChanged += sigpa.settings_OnPropertyChange;
            
            _core = new SigabaCore(this, sigpa);
            _fastCore = new SigabaCoreFast();
            _keys = new string[5];

            this._settings.PropertyChanged += _core.settings_OnPropertyChange;
        }

        #endregion

        #region Private Variables

        private string[] _keys;

        #endregion

        #region Data Properties

        /// <summary>
        /// </summary>
        [PropertyInfo(Direction.InputData, "(De)Cipher", "Sets the (De)Cipher as input")]
        public string InputString
        {
            get;
            set;
        }

        /// <summary>
        /// </summary>
        [PropertyInfo(Direction.OutputData, "(De)Cipher", "Sets the (De)Cipher as output")]
        public string OutputString
        {
            get;
            set;
        }

      /*  [PropertyInfo(Direction.OutputData, "Survivor Array", "Sets the (De)Cipher as output")]
        public object[] SurvivorArray
        {
            get;
            set;
        }*/

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return _settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get;
            private set;
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            if (controlSlave == null)
            {
                _core.SetInternalConfig();
                _keys[0] = _settings.CipherKey;
                _keys[1] = _settings.IndexKey;
                _keys[2] = _settings.ControlKey;
            }
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        /// 
        public void Execute()
        {
            ProgressChanged(0, 1);

            _settings.CipherKey = _keys[0].ToUpper();
            _settings.IndexKey = _keys[1].ToUpper();
            _settings.ControlKey = _keys[2].ToUpper();

            _core.SetKeys();

            if(!Presentation.IsVisible)
            {
                object[] repeat = _core.Encrypt(preFormatInput(InputString));
                OutputString = postFormatOutput((string) repeat[0]);
                //SurvivorArray = new object[] { _keys[0].ToUpper(), new int[] { _settings.CipherRotor1, _settings.CipherRotor2, _settings.CipherRotor3, _settings.CipherRotor4, _settings.CipherRotor5, }, repeat[1] as int[][], new[] { _settings.CipherRotor1Reverse, _settings.CipherRotor2Reverse, _settings.CipherRotor3Reverse, _settings.CipherRotor4Reverse, _settings.CipherRotor5Reverse} };
                OnPropertyChanged("SurvivorArray");
            }
            else
            {
                OutputString = postFormatOutput(_core.EncryptPresentation(preFormatInput(InputString)));
            }

            OnPropertyChanged("OutputString");

            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            if (controlSlave == null)
            {
                _settings.CipherKey = _keys[0].ToUpper();
                _settings.IndexKey = _keys[1].ToUpper();
                _settings.ControlKey = _keys[2].ToUpper();
            }
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            _core.stop();
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        public void changeSettings(string setting, object value)
        {
            if (setting.Equals("CipherKey")) _settings.CipherKey = (string)value;
            else if (setting.Equals("ControlKey")) _settings.ControlKey = (string)value;
            else if (setting.Equals("IndexKey")) _settings.IndexKey = (string)value;

            else if (setting.Equals("IndexKey")) _settings.Action = (int) value;

            else if (setting.Equals("CipherRotor1")) _settings.CipherRotor1 = (int)value;
            else if (setting.Equals("CipherRotor2")) _settings.CipherRotor2 = (int)value;
            else if (setting.Equals("CipherRotor3")) _settings.CipherRotor3 = (int)value;
            else if (setting.Equals("CipherRotor4")) _settings.CipherRotor4 = (int)value;
            else if (setting.Equals("CipherRotor5")) _settings.CipherRotor5 = (int)value;
            
            else if (setting.Equals("ControlRotor1")) _settings.ControlRotor1 = (int)value;
            else if (setting.Equals("ControlRotor2")) _settings.ControlRotor2 = (int)value;
            else if (setting.Equals("ControlRotor3")) _settings.ControlRotor3 = (int)value;
            else if (setting.Equals("ControlRotor4")) _settings.ControlRotor4 = (int)value;
            else if (setting.Equals("ControlRotor5")) _settings.ControlRotor5 = (int)value;

            else if (setting.Equals("IndexRotor1")) _settings.IndexRotor1 = (int)value;
            else if (setting.Equals("IndexRotor2")) _settings.IndexRotor2 = (int)value;
            else if (setting.Equals("IndexRotor3")) _settings.IndexRotor3 = (int)value;
            else if (setting.Equals("IndexRotor4")) _settings.IndexRotor4 = (int)value;
            else if (setting.Equals("IndexRotor5")) _settings.IndexRotor5 = (int)value;

            else if (setting.Equals("CipherRotor1Reverse")) _settings.CipherRotor1Reverse = (bool)value;
            else if (setting.Equals("CipherRotor2Reverse")) _settings.CipherRotor2Reverse = (bool)value;
            else if (setting.Equals("CipherRotor3Reverse")) _settings.CipherRotor3Reverse = (bool)value;
            else if (setting.Equals("CipherRotor4Reverse")) _settings.CipherRotor4Reverse = (bool)value;
            else if (setting.Equals("CipherRotor5Reverse")) _settings.CipherRotor5Reverse = (bool)value;

            else if (setting.Equals("ControlRotor1Reverse")) _settings.ControlRotor1Reverse = (bool)value;
            else if (setting.Equals("ControlRotor2Reverse")) _settings.ControlRotor2Reverse = (bool)value;
            else if (setting.Equals("ControlRotor3Reverse")) _settings.ControlRotor3Reverse = (bool)value;
            else if (setting.Equals("ControlRotor4Reverse")) _settings.ControlRotor4Reverse = (bool)value;
            else if (setting.Equals("ControlRotor5Reverse")) _settings.ControlRotor5Reverse = (bool)value;

            else if (setting.Equals("IndexRotor1Reverse")) _settings.IndexRotor1Reverse = (bool)value;
            else if (setting.Equals("IndexRotor2Reverse")) _settings.IndexRotor2Reverse = (bool)value;
            else if (setting.Equals("IndexRotor3Reverse")) _settings.IndexRotor3Reverse = (bool)value;
            else if (setting.Equals("IndexRotor4Reverse")) _settings.IndexRotor4Reverse = (bool)value;
            else if (setting.Equals("IndexRotor5Reverse")) _settings.IndexRotor5Reverse = (bool)value;
        }

        #endregion

        #region ConnectorProperties
        
        #endregion

        #region Tools 
        
        internal class UnknownToken
        {
            internal string text;
            internal int position;

            internal UnknownToken(char c, int position)
            {
                this.text = char.ToString(c);
                this.position = position;
            }

            public override string ToString()
            {
                return "[" + text + "," + position + "]";
            }
        }

        IList<UnknownToken> unknownList = new List<UnknownToken>();
        IList<UnknownToken> lowerList = new List<UnknownToken>();

        public string preFormatInput(string text)
        {
            StringBuilder result = new StringBuilder();
            bool newToken = true;
            unknownList.Clear();
            lowerList.Clear();

            for (int i = 0; i < text.Length; i++)
            {
                if (_settings.Alphabet.Contains(char.ToUpper(text[i])))
                {
                    newToken = true;
                    if (text[i] == char.ToLower(text[i])) //Solution for preserve FIXME underconstruction
                    {
                        if (_settings.UnknownSymbolHandling == 1)
                        {
                            lowerList.Add(new UnknownToken(text[i], result.Length));
                        }
                        else
                        {
                            lowerList.Add(new UnknownToken(text[i], i));
                        }

                    }                                      //underconstruction end
                    result.Append(char.ToUpper(text[i])); // FIXME: shall save positions of lowercase letters
                }
                else if (_settings.UnknownSymbolHandling != 1) // 1 := remove
                {
                    // 0 := preserve, 2 := replace by X
                    char symbol = _settings.UnknownSymbolHandling == 0 ? text[i] : 'X';

                    if (newToken)
                    {
                        unknownList.Add(new UnknownToken(symbol, i));
                        newToken = false;
                    }
                    else
                    {
                        unknownList.Last().text += symbol;
                    }
                }
            }

            return result.ToString().ToUpper();
        }

        public string postFormatOutput(string text)
        {
            StringBuilder workstring = new StringBuilder(text);
            foreach (UnknownToken token in unknownList)
            {
                workstring.Insert(token.position, token.text);
            }

            foreach (UnknownToken token in lowerList)   //Solution for preserve FIXME underconstruction
            {
                char help = workstring[token.position];
                workstring.Remove(token.position, 1);
                workstring.Insert(token.position, char.ToLower(help));
            }                                           //underconstruction end

            switch (_settings.CaseHandling)
            {
                default:
                case 0: // preserve
                    // FIXME: shall restore lowercase letters
                    return workstring.ToString();
                case 1: // upper
                    return workstring.ToString().ToUpper();
                case 2: // lower
                    return workstring.ToString().ToLower();
            }
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

        private IControlSigabaEncryption controlSlave;
        [PropertyInfo(Direction.ControlSlave, "ControlSlaveCaption", "ControlSlaveTooltip")]
        public IControlSigabaEncryption ControlSlave
        {
            get
            {
                if (controlSlave == null)
                    controlSlave = new SigabaControl(this);
                return controlSlave;
            }
        }
    }

    public class SigabaControl : IControlSigabaEncryption
    {
        private Sigaba plugin;

        public SigabaControl(Sigaba plugin)
        {
            this.plugin = plugin;
        }

        public string postFormatOutput(string text)
        {
            return plugin.postFormatOutput(text);
        }

        public string preFormatInput(string text)
        {
            return plugin.preFormatInput(text);
        }

        public string Decrypt(string ciphertext)
        {
            return (string)plugin._core.Encrypt(ciphertext)[0];
        }

        public byte[] DecryptFast(byte[] ciphertext, int[] a, byte[] positions)
        {
            return plugin._fastCore.Encrypt(ciphertext, a, positions);
        }

        public void setInternalConfig()
        {
            plugin._core.SetInternalConfig();
        }

        public void setCipherRotors(int i, byte a)
        {
            plugin._fastCore.setCipherRotors(i,a);
        }

        public void setControlRotors(byte i, byte b)
        {
            plugin._fastCore.setControlRotors(i,b);
        }

        public void setIndexRotors(byte i, byte c)
        {
            plugin._fastCore.setIndexRotors(i, c);
        }

        public void setBool(byte ix, byte i, bool rev)
        {
            plugin._fastCore.setBool(ix,i,rev);
        }

        public void setIndexMaze()
        {
            plugin._fastCore.setIndexMaze();
        }

        public void setIndexMaze(int[] indexmaze)
        {
            plugin._fastCore.setIndexMaze(indexmaze);
        }

        public void setIndexMaze2(int[] indexmaze)
        {
            plugin._fastCore.setIndexMaze2(indexmaze);
        }

        public void setPositionsControl(byte ix, byte i, byte position)
        {
            plugin._fastCore.setPositionsControl(ix,i,position);
        }

        public void setPositionsIndex(byte ix, byte i, byte position)
        {
            plugin._fastCore.setPositionsIndex(ix, i, position);
        }

        public void onStatusChanged()
        {
            if (OnStatusChanged != null)
                OnStatusChanged(this, true);
        }

        public RotorByte[] CipherRotors()
        {
            return plugin._fastCore.CipherRotors;
        }

        public void changeSettings(string setting, object value)
        {
            plugin.changeSettings(setting, value);
        }

        public event IControlStatusChangedEventHandler OnStatusChanged;

        public void Dispose()
        {
        }
    }
}