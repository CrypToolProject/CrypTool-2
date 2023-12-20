/*
   Copyright 2022 Vasily Mikhalev, CrypTool 2 Team

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
using HagelinMachine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using static HagelinMachine.HagelinConstants;
using static HagelinMachine.HagelinEnums;

namespace CrypTool.Plugins.HagelinMachine
{
    [Author("Vasily Mikhalev", "vasily.mikhalev@uni-siegen.de", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("CrypTool.Plugins.HagelinMachine.Properties.Resources", "HagelinMachineCaption", "HagelinMachineTooltip", "HagelinMachine/userdoc.xml", new[] { "HagelinMachine/icon.jpg" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class HagelinMachine : ICrypComponent
    {
        #region Private Variables

        private readonly HagelinMachineSettings _settings = new HagelinMachineSettings();
        private readonly HagelinMachinePresentation _presentation = new HagelinMachinePresentation();
        private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string _inputText;
        private bool _gridsToBeCreated = true;
        private bool _executing = false;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextTip", true)]
        public string InputText
        {
            get
            {
                return _inputText;
            }
            set
            {
                _inputText = value;
                OnPropertyChanged("InputString");
            }
        }

        [PropertyInfo(Direction.InputData, "InputWheelsStateCaption", "InputWheelsStateTip", false)]
        public string InputWheelPositions
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "InputWheelPinsCaption", "InputWheelPinsTip", false)]
        public string InputWheelPins
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "InputBarLugsCaption", "InputBarLugsTip", false)]
        public string InputBarLugs
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputTextCaption", "OutputTextTip", false)]
        public string OutputText
        {
            get;
            set;
        }

        /*[PropertyInfo(Direction.OutputData, "OutputReportCaption", "OutputReportTip", false)]
        public string Report
        {
            get;
            set;
        }*/

        [PropertyInfo(Direction.OutputData, "OutputKeystreamCaption", "OutputKeystreamTip", false)]
        public int[] Keystream
        {
            get;
            set;
        }

        #endregion

        #region constructor

        public HagelinMachine()
        {
            _settings.PropertyChanged += Settings_OnPropertyChanged;
            UpdateHagelinModelString();
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get
            {
                return _settings;
            }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get
            {
                return _presentation;
            }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            //Getting information about selected machine model. ToDo: change from public properties to private in settings class and use Get/Set
            ModelType model = _settings.Model;
            int numberOfWheels = _settings.NumberOfWheels;
            int numberOfBars = _settings.NumberOfBars;

            //types of wheels and their rotation position
            WheelType[] wheelTypes = _settings._wheelsTypes;
            string[] wheelsStates = _settings._wheelsInitialStates;

            //types of bars
            int[] barTypes = _settings._barType;
            bool[] barHasLugs = _settings._barHasLugs;
            string[] barCumsAsString = _settings._barCamsAsString;

            //internal key
            string[] wheelsPins;
            string[] barLugs;

            //Checking if the inputs of the internal/external keys are provided. If so try to use them 
            if (InputWheelPositions != null)
            {
                string inputWheelPositions = Regex.Replace(InputWheelPositions, @"\.| |;|\s", ",");

                if (InputWheelPositions != _settings._wheelsInitialPositions.Substring(0, _settings._wheelsInitialPositions.Length - 1)) //substring is needed to match the formats (that is to remove the last comma)
                {
                    wheelsStates = GetInitialPositionsFromInput(inputWheelPositions, wheelsStates);
                }

                if (wheelsStates == null)
                {
                    GuiLogMessage(string.Format(CrypTool.Plugins.HagelinMachine.Properties.Resources.ErrorInvalidState, inputWheelPositions), NotificationLevel.Error);
                    return;
                }
            }

            if (InputWheelPins != null)
            {
                wheelsPins = GetWheelPinsFromInput(InputWheelPins, wheelsStates);
                if (wheelsPins == null)
                {
                    GuiLogMessage(string.Format(CrypTool.Plugins.HagelinMachine.Properties.Resources.ErrorInvalidPins, InputWheelPins), NotificationLevel.Error);
                    return;
                }
            }
            else
            {
                wheelsPins = _settings._wheelsPins;
            }

            if (InputBarLugs != null)
            {
                barLugs = GetBarLugsInput(InputBarLugs, barHasLugs);
                if (barLugs == null)
                {
                    GuiLogMessage(string.Format(CrypTool.Plugins.HagelinMachine.Properties.Resources.ErrorInvalidLugs, InputBarLugs), NotificationLevel.Error);
                    return;
                }

            }
            else
            {
                barLugs = _settings._barLug;
            }

            ProgressChanged(0, 1);

            //execution:

            //StringBuilder reportBuilder = new StringBuilder();

            try
            {
                _executing = true;
                string input = InputText.ToUpper();

                //reportBuilder.Append(string.Format(CrypTool.Plugins.HagelinMachine.Properties.Resources.PhraseInReportInputText + input + "\r"));

                if (_settings.UseZAsSpace && _settings.Mode == ModeType.Encrypt)
                {
                    input = input.Replace(" ", "Z");
                }

                List<UnknownSymbol> unknownSymbolList = new List<UnknownSymbol>();
                input = RemoveUnknownSymbols(input, ALPHABET, out unknownSymbolList);
                StringBuilder output = new StringBuilder();

                HagelinImplementation hagelinImplementation = new HagelinImplementation(
                    _settings.Model,
                    _settings.NumberOfWheels,
                    _settings.NumberOfBars,
                    _settings._wheelsTypes,
                    wheelsPins,
                    wheelsStates,
                    _settings._barType,
                    _settings._barHasLugs,
                    _settings._barCamsAsString,
                     barLugs,
                    _settings._barTooth,
                    _settings.InitOffset,
                    _settings.Mode,
                    _settings.FVFeatureIsActive,
                    ALPHABET);

                if (_gridsToBeCreated)
                {
                    Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        _presentation.CreateDinamicInfoGrid();
                        _presentation.CreateWheelsInfoGrids();
                        _gridsToBeCreated = false;
                    }, null);
                }

                //reportBuilder.Clear();
                var keystreamList = new List<int>();

                hagelinImplementation.UpdateWheelPositions();
                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    _presentation.ShowDisplayedPositionsInGrid(hagelinImplementation._shownWheelPositions);
                    _presentation.ShowActivePositionsInGrid(hagelinImplementation._activeWheelPositions);
                    _presentation.ShowWheelPinActivityInGrid(hagelinImplementation._wheelsWithActivePin);
                    _presentation.ShowWheelPositionsinGrid(hagelinImplementation._shownWheelPositions);
                    _presentation.labelInput.Content = string.Empty;
                    _presentation.labelOutput.Content = string.Empty;
                }, null);

                for (int offset = 0; offset < input.Length; offset++)
                {
                    if (!_executing)
                    {
                        return;
                    }

                    //reportBuilder.Append(CrypTool.Plugins.HagelinMachine.Properties.Resources.PhraseInReportInputCharacter + input[offset] + " \r");
                    char outputChar = hagelinImplementation.EncryptDecryptOneCharacter(input[offset]);

                    output.Append(outputChar);
                    //reportBuilder.Append(hagelinImplementation._reportStringBuilder);
                    keystreamList.Add(hagelinImplementation._displacement);

                    hagelinImplementation.UpdateWheelPositions();
                    if (offset == input.Length - 1) // only show
                    {
                        Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            _presentation.ShowDisplayedPositionsInGrid(hagelinImplementation._shownWheelPositions);
                            _presentation.ShowActivePositionsInGrid(hagelinImplementation._activeWheelPositions);
                            _presentation.ShowWheelsAdvancementsInGrid(hagelinImplementation._advancementsToShow);
                            _presentation.ShowWheelPinActivityInGrid(hagelinImplementation._wheelsWithActivePin);
                            _presentation.ShowWheelPositionsinGrid(hagelinImplementation._shownWheelPositions);
                            _presentation.labelInput.Content = input[offset];
                            _presentation.labelOutput.Content = outputChar;                            
                        }, null);
                    }
                    if (offset % 10 == 0) //only fire progress changed every 10th letter
                    {
                        ProgressChanged(offset, _inputText.Length);
                    }
                    //reportBuilder.Append("\r\r========================================== \r");
                }
                string outputText = string.Empty;
                switch (_settings.UnknownSymbolHandling)
                {
                    case UnknownSymbolHandling.Ignore:
                        outputText = AddUnknownSymbols(output.ToString(), unknownSymbolList, null);
                        break;
                    case UnknownSymbolHandling.Replace:
                        outputText = AddUnknownSymbols(output.ToString(), unknownSymbolList, "?");
                        break;
                    case UnknownSymbolHandling.Remove:
                        outputText = output.ToString();
                        break;
                }

                if (_settings.UseZAsSpace && _settings.Mode == ModeType.Decrypt)
                {
                    outputText = outputText.Replace("Z", " ");
                }

                OutputText = outputText;
                OnPropertyChanged("OutputText");
                //Report = reportBuilder.ToString();
                //OnPropertyChanged("Report");
                Keystream = keystreamList.ToArray();
                OnPropertyChanged("Keystream");

                ProgressChanged(1, 1);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format(CrypTool.Plugins.HagelinMachine.Properties.Resources.ErrorGeneral, ex.Message), NotificationLevel.Error);
            }
            _executing = false;
            
        }

        /// <summary>
        /// Updates the model string shown in top of the presentatio
        /// </summary>
        private void UpdateHagelinModelString()
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _presentation.LabelModel.Content = string.Format("{0}: {1}", Properties.Resources.HagelinModel, _settings.SelectedModel);
            }, null);
        }


        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            _executing = false;

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

        #endregion

        #region Event Handling


        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public void GuiLogMessage(string message, NotificationLevel logLevel)
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

        /// <summary>
        /// Called, when a setting property changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyChangedEvent"></param>
        private void Settings_OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEvent)
        {
            if (propertyChangedEvent.PropertyName.Equals(nameof(HagelinMachineSettings.Model)))
            {
                UpdateHagelinModelString();
            }
        }

        #endregion

        #region Functions to get settings from input

        private string[] GetInitialPositionsFromInput(string inputWheelPositions, string[] wheelsStates)

        {
            try
            {
                string[] s = InputWheelPositions.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (s.Length != _settings.NumberOfWheels)
                {
                    GuiLogMessage(CrypTool.Plugins.HagelinMachine.Properties.Resources.ErrorInvalidSettingsWheelPositions, NotificationLevel.Error);
                    return null;
                }

                for (int i = 0; i < _settings.NumberOfWheels; i++)
                {
                    string val = wheelsStates[i];
                    if (val.IndexOf(s[i]) < 0)
                    {
                        return null;
                    }
                    string last2characters = val.Substring(val.Length - 2).Trim();
                    if (last2characters[last2characters.Length - 1] != ',')
                    {
                        val += ",";
                    }

                    while (val.IndexOf(s[i]) > 0)
                    {
                        val = val.Substring(1) + val.Substring(0, 1);
                    }

                    wheelsStates[i] = val;
                }
                return wheelsStates;
            }
            catch (Exception)
            {

            }
            return null;
        }

        private string[] GetWheelPinsFromInput(string wheelPinsFromInput, string[] wheelsStates)
        {
            StringBuilder newPinsForWheel = new StringBuilder();
            string[] wheelPins = new string[maxNumberOfWheels];
            string[] s = wheelPinsFromInput.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (s.Length != _settings.NumberOfWheels)
            {
                return null;
            }

            try
            {
                for (int i = 0; i < _settings.NumberOfWheels; i++)
                {
                    char[] separator = { ',', ' ', ';' };
                    string[] allPositionsList = s[i].Split(separator, StringSplitOptions.RemoveEmptyEntries);

                    for (int ii = 0; ii < allPositionsList.Length; ii++)
                    {
                        bool givenCharacterNotFound = !wheelsStates[i].Contains(allPositionsList[ii]);
                        if (givenCharacterNotFound)
                        {
                            return null;
                        }
                    }

                    wheelPins[i] = Regex.Replace(s[i], @"\.| |;|\s", ",");
                }
                return wheelPins;
            }
            catch (Exception)
            {
                // ?
            }
            return null;
        }

        private string[] GetBarLugsInput(string barLugsFromInput, bool[] barHasLugs)
        {
            try
            {
                string[] barLugs = new string[maxNumberOfBars];
                var value = barLugsFromInput;
                if (value == null) return null;
                string[] s = value.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (s.Length != _settings.NumberOfBars)
                {
                    GuiLogMessage(CrypTool.Plugins.HagelinMachine.Properties.Resources.ErrorInvalidSettingsBarLugs, NotificationLevel.Error);
                    return null;
                }

                for (int i = 0; i < _settings.NumberOfBars; i++)
                {
                    s[i] = Regex.Replace(s[i], @"\.| |;|\s", ","); //we accept other separators, such as ; , etc.. and replace them by ,
                    s[i] = Regex.Replace(s[i], @"[^1-6,]", string.Empty); // we filter all characters despite wheel numbers and ","
                    if (barHasLugs[i])
                    {
                        barLugs[i] = s[i];
                    }
                    else
                    {
                        barLugs[i] = string.Empty;
                    }
                }
                return barLugs;
            }
            catch (Exception)
            {
                // ?
            }

            return null;
        }

        #endregion

        #region Other functions
        private string RemoveUnknownSymbols(string text,
                                            string alphabet,
                                            out List<UnknownSymbol> unknownSymbolList)
        {
            unknownSymbolList = new List<UnknownSymbol>();
            for (var position = 0; position < text.Length; position++)
            {
                var symbol = text.Substring(position, 1);
                if (!alphabet.Contains(symbol))
                {
                    unknownSymbolList.Add(new UnknownSymbol() { Symbol = symbol, Position = position });
                }
            }
            foreach (var unkownSymbol in unknownSymbolList)
            {
                text = text.Replace(unkownSymbol.Symbol, string.Empty);
            }
            return text;
        }

        /// <summary>
        /// Adds all unknown symbols from the list back to the text. If a replacement string is given, it replaces these by this string
        /// </summary>
        /// <param name="text"></param>
        /// <param name="unknownSymbolList"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        private string AddUnknownSymbols(string text,
                                         List<UnknownSymbol> unknownSymbolList,
                                         string replacement = null)
        {
            foreach (var unkownSymbol in unknownSymbolList)
            {
                if (replacement == null)
                {
                    text = text.Insert(unkownSymbol.Position, unkownSymbol.Symbol);
                }
                else
                {
                    text = text.Insert(unkownSymbol.Position, replacement);
                }
            }
            return text;
        }
        #endregion
    }
    public class UnknownSymbol
    {
        public string Symbol { get; set; }
        public int Position { get; set; }
    }
}
