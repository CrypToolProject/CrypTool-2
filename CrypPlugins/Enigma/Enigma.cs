/* 
   Copyright 2008-2017, Arno Wacker, University of Kassel

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


//CrypTool 2.0 specific includes
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
// additional needed libs
using System.Windows.Controls;


namespace CrypTool.Enigma
{
    [Author("Arno Wacker, Matthäus Wander", "arno.wacker@CrypTool.org", "Universität Kassel, Universität Duisburg-Essen", "http://www.ais.uni-kassel.de")]
    [PluginInfo("CrypTool.Enigma.Properties.Resources", "PluginCaption", "PluginTooltip", "Enigma/DetailedDescription/doc.xml",
      "Enigma/Images/Enigma.png", "Enigma/Images/encrypt.png", "Enigma/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Enigma : ICrypComponent
    {
        #region Constants

        internal const int ABSOLUTE = 0;
        internal const int PERCENTAGED = 1;
        internal const int LOG2 = 2;
        internal const int SINKOV = 3;

        #endregion

        #region Private variables

        private readonly EnigmaSettings _settings;
        private readonly EnigmaPresentationFrame _enigmaPresentationFrame;
        private readonly EnigmaCore _core;
        private string _textInput;
        private string _keyInput;

        private string _textOutput;

        public bool _isrunning;

        private bool _running = false;
        private bool _stopped = false;

        #endregion

        #region Private methods

        #region Formatting stuff

        /// <summary>
        /// Encrypts or decrypts a string with the given key (rotor positions) and formats
        /// the output according to the settings
        /// </summary>
        /// <param name="rotor1Pos">Position of rotor 1 (fastest)</param>
        /// <param name="rotor2Pos">Position of rotor 2 (middle)</param>
        /// <param name="rotor3Pos">Position of rotor 3 (slowest)</param>
        /// <param name="rotor4Pos">Position of rotor 4 (extra rotor for M4)</param>
        /// <param name="text">The text for en/decryption. This string may contain 
        /// arbitrary characters, which will be dealt with according to the settings given</param>
        /// <returns>The encrypted/decrypted string</returns>
        private string FormattedEncrypt(int rotor1Pos, int rotor2Pos, int rotor3Pos, int rotor4Pos, string text)
        {
            string input = preFormatInput(text);
            _enigmaPresentationFrame.ChangeStatus(_isrunning, _enigmaPresentationFrame.EnigmaPresentation.IsVisible);

            if (Presentation.IsVisible && _enigmaPresentationFrame.EnigmaPresentation.PresentationDisabled.DisabledBoolProperty)
            {
                string output = _core.Encrypt(rotor1Pos, rotor2Pos, rotor3Pos, rotor4Pos, input);
                _enigmaPresentationFrame.EnigmaPresentation.output = output;
                if (_enigmaPresentationFrame.EnigmaPresentation.checkReady())
                {
                    _enigmaPresentationFrame.EnigmaPresentation.setinput(input);
                }
                else
                {
                    LogMessage("Presentation Error!", NotificationLevel.Error);
                }
                return string.Empty;
            }

            return postFormatOutput(_core.Encrypt(rotor1Pos, rotor2Pos, rotor3Pos, rotor4Pos, input));
        }

        internal class UnknownToken
        {
            internal string _text;
            internal int _position;

            internal UnknownToken(char c, int position)
            {
                _text = char.ToString(c);
                _position = position;
            }

            public override string ToString()
            {
                return "[" + _text + "," + _position + "]";
            }
        }

        private readonly IList<UnknownToken> unknownList = new List<UnknownToken>();
        private readonly IList<UnknownToken> lowerList = new List<UnknownToken>();

        /// <summary>
        /// Format the string to contain only alphabet characters in upper case
        /// </summary>
        /// <param name="text">The string to be prepared</param>
        /// <returns>The properly formated string to be processed direct by the encryption function</returns>
        private string preFormatInput(string text)
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
                        unknownList.Last()._text += symbol;
                    }
                }
            }

            return result.ToString().ToUpper();
        }

        /// <summary>
        /// Formats the string processed by the encryption for presentation according
        /// to the settings given
        /// </summary>
        /// <param name="text">The encrypted text</param>
        /// <returns>The formatted text for output</returns>
        private string postFormatOutput(string text)
        {
            StringBuilder workstring = new StringBuilder(text);
            foreach (UnknownToken token in unknownList)
            {
                workstring.Insert(token._position, token._text);
            }

            foreach (UnknownToken token in lowerList)   //Solution for preserve FIXME underconstruction
            {
                char help = workstring[token._position];
                workstring.Remove(token._position, 1);
                workstring.Insert(token._position, char.ToLower(help));
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

        #endregion

        #region Constructor

        public Enigma()
        {
            _settings = new EnigmaSettings();
            _core = new EnigmaCore(this);

            _enigmaPresentationFrame = new EnigmaPresentationFrame(this);
            EnigmaPresentation myPresentation = _enigmaPresentationFrame.EnigmaPresentation;
            Presentation = _enigmaPresentationFrame;
            //Presentation.IsVisibleChanged += presentation_isvisibleChanged;
            _settings.PropertyChanged += _enigmaPresentationFrame.EnigmaPresentation.settings_OnPropertyChange;
            _enigmaPresentationFrame.EnigmaPresentation.fireLetters += fireLetters;
            _enigmaPresentationFrame.EnigmaPresentation.newInput += newInput;
        }

        #endregion

        #region Events

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void newInput(object sender, EventArgs args)
        {
            _running = false;
        }

        private void fireLetters(object sender, EventArgs args)
        {
            object[] carrier = sender as object[];

            _textOutput = (string)carrier[0];
            int x = (int)carrier[1];
            int y = (int)carrier[2];

            ShowProgress(x, y);
        }

        #endregion

        #region IPlugin properties

        public ISettings Settings => _settings;

        public UserControl Presentation
        {
            get;
            private set;
        }

        #endregion

        #region Connector properties

        [PropertyInfo(Direction.InputData, "TextInputCaption", "TextInputTooltip", true)]
        public string TextInput
        {
            get => _textInput;
            set
            {
                _textInput = value;
                OnPropertyChanged("TextInput");
            }
        }

        [PropertyInfo(Direction.InputData, "KeyInputCaption", "KeyInputTooltip", false)]
        public string KeyInput
        {
            get => _keyInput;
            set
            {
                _keyInput = value;
                OnPropertyChanged("KeyInput");
            }
        }

        [PropertyInfo(Direction.OutputData, "TextOutputCaption", "TextOutputTooltip", false)]
        public string TextOutput
        {
            get => _textOutput;
            set
            {
                _textOutput = value;
                OnPropertyChanged("TextOutput");
            }
        }

        #endregion

        #region Public methods

        public void PreExecution()
        {

        }

        public void Execute()
        {
            if (!string.IsNullOrEmpty(KeyInput))
            {
                _settings.SetKeySettings(KeyInput);
            }
            if (_settings.Model != 3 && _settings.Model != 2)
            {
                LogMessage("This simulator is work in progress. As of right now only Enigma I and Enigma Reichsbahn (Rocket) is supported!!", NotificationLevel.Error);
                return;
            }

            _isrunning = true;
            _running = false;
            _stopped = false;

            if (_enigmaPresentationFrame.EnigmaPresentation.checkReady())
            {
                _enigmaPresentationFrame.EnigmaPresentation.stopclick(this, EventArgs.Empty);
            }

            //configure the Enigma
            _core.setInternalConfig(_settings.Rotor1, _settings.Rotor2, _settings.Rotor3, _settings.Rotor4,
                        _settings.Reflector, _settings.Ring1, _settings.Ring2, _settings.Ring3, _settings.Ring4,
                        _settings.PlugBoard);

            while (_running)
            {
                _enigmaPresentationFrame.EnigmaPresentation.stopclick(this, EventArgs.Empty);
                if (_stopped)
                {
                    return;
                }
            }

            _running = true;

            // re-set the key, in case we get executed again during single run
            //_settings.InitialRotorPos = _savedInitialRotorPos.ToUpper();

            // do the encryption
            _textOutput = FormattedEncrypt(_settings.Alphabet.IndexOf(_settings.InitialRotorPos[2]),
                _settings.Alphabet.IndexOf(_settings.InitialRotorPos[1]),
                _settings.Alphabet.IndexOf(_settings.InitialRotorPos[0]),
                0, _textInput);

            // "fire" the output
            OnPropertyChanged("TextOutput");

            LogMessage("Decrypted with key: " + KeyInput, NotificationLevel.Info);
        }

        public void PostExecution()
        {
            _keyInput = string.Empty;
            _textInput = string.Empty;
            _running = false;
            _isrunning = false;
            _enigmaPresentationFrame.ChangeStatus(_isrunning, _enigmaPresentationFrame.EnigmaPresentation.IsVisible);
        }

        public void Stop()
        {
            _stopped = true;
            LogMessage("Enigma stopped", NotificationLevel.Info);
            _enigmaPresentationFrame.EnigmaPresentation.stopclick(this, EventArgs.Empty);
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Logs a message to the CrypTool console
        /// </summary>
        public void LogMessage(string msg, NotificationLevel level)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(msg, this, level));
        }

        /// <summary>
        /// Sets the progress bar for this plugin
        /// </summary>
        /// <param name="val"></param>
        /// <param name="max"></param>
        public void ShowProgress(double val, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(val, max));
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}
