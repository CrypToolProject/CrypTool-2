/*
   Copyright 2023 Nils Kopal <kopal<AT>cryptool.org>

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
using CrypTool.Typex.TypexMachine;
using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;
using static CrypTool.Typex.TypexMachine.Util;

namespace CrypTool.Typex
{
    /// <summary>
    /// Implementation of the Typex rotor machine. Compatible to (and rotor definitions taken from) https://typex.virtualcolossus.co.uk/Typex/
    /// See also the GCHQ's Typex implementation of "CyberChef": https://gchq.github.io/CyberChef/ and https://github.com/gchq/CyberChef
    /// </summary>
    /// </summary>
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "CrypTool project", "http://www.cryptool.org")]
    [PluginInfo("CrypTool.Typex.Properties.Resources", "PluginCaption", "PluginTooltip", "Typex/DetailedDescription/doc.xml", "Typex/icon.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Typex : ICrypComponent
    {
        public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; //Latin alphabet
        private const string Entry = "AZYXWVUTSRQPONMLKJIHGFEDCB"; //Entry stator ("reversed" rotor compared to Enigma)

        private readonly TypexSettings _settings = new TypexSettings();
        private bool _stop = false;

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextTooltip", true)]
        public string InputText
        {
            get;
            set;
        }      

        [PropertyInfo(Direction.OutputData, "OutputTextCaption", "OutputTextTooltip", true)]
        public string OutputText
        {
            get;
            set;
        }

        public void PreExecution()
        {            
        }

        public void PostExecution()
        {

        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public ISettings Settings => _settings;

        public UserControl Presentation => null;

        public void Execute()
        {
            ProgressChanged(0, 1);

            _stop = false;
            Machine machine = new Machine(CreateRotorArray(), CreateReflector(), CreatePlugboard(), Alphabet);           
            OutputText = CryptText(machine, InputText.ToUpper());
            OnPropertyChanged(nameof(OutputText));

            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Creates rotor array for selected machine and selected rotors and start positions
        /// Also adds the entry stator (reversed rotor) to the beginning of the array
        /// </summary>
        /// <returns></returns>
        private Rotor[] CreateRotorArray()
        {
            //entry stator reverses the alphabet
            Stator entry = new Stator(MapTextIntoNumberSpace(Entry, Alphabet), null, 0, false);

            //Typex has two stators
            Stator stator1;
            Stator stator2;

            //Typex has three rotors
            Rotor rotor1;
            Rotor rotor2;
            Rotor rotor3;

            switch (_settings.TypexMachineType)
            {
                case TypexMachineType.CyberChef:
                    stator1 = new Stator(MapTextIntoNumberSpace(Typex_CyberChef.Rotors[_settings.Stator1], Alphabet), Typex_CyberChef.Notches[_settings.Stator1], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[4]), _settings.Stator1IsReversed);
                    stator2 = new Stator(MapTextIntoNumberSpace(Typex_CyberChef.Rotors[_settings.Stator2], Alphabet), Typex_CyberChef.Notches[_settings.Stator2], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[3]), _settings.Stator2IsReversed);
                    rotor1 = new Rotor(MapTextIntoNumberSpace(Typex_CyberChef.Rotors[_settings.Rotor1], Alphabet), Typex_CyberChef.Notches[_settings.Rotor1], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[2]), false, _settings.Rotor1IsReversed);
                    rotor2 = new Rotor(MapTextIntoNumberSpace(Typex_CyberChef.Rotors[_settings.Rotor2], Alphabet), Typex_CyberChef.Notches[_settings.Rotor2], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[1]), true, _settings.Rotor2IsReversed);
                    rotor3 = new Rotor(MapTextIntoNumberSpace(Typex_CyberChef.Rotors[_settings.Rotor3], Alphabet), Typex_CyberChef.Notches[_settings.Rotor3], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[0]), false, _settings.Rotor3IsReversed);
                    break;
                case TypexMachineType.TestRotors:
                    stator1 = new Stator(MapTextIntoNumberSpace(Typex_TestRotors.Rotors[_settings.Stator1], Alphabet), Typex_TestRotors.Notches[_settings.Stator1], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[4]), _settings.Stator1IsReversed);
                    stator2 = new Stator(MapTextIntoNumberSpace(Typex_TestRotors.Rotors[_settings.Stator2], Alphabet), Typex_TestRotors.Notches[_settings.Stator2], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[3]), _settings.Stator2IsReversed);
                    rotor1 = new Rotor(MapTextIntoNumberSpace(Typex_TestRotors.Rotors[_settings.Rotor1], Alphabet), Typex_TestRotors.Notches[_settings.Rotor1], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[2]), false, _settings.Rotor1IsReversed);
                    rotor2 = new Rotor(MapTextIntoNumberSpace(Typex_TestRotors.Rotors[_settings.Rotor2], Alphabet), Typex_TestRotors.Notches[_settings.Rotor2], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[1]), true, _settings.Rotor2IsReversed);
                    rotor3 = new Rotor(MapTextIntoNumberSpace(Typex_TestRotors.Rotors[_settings.Rotor3], Alphabet), Typex_TestRotors.Notches[_settings.Rotor3], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[0]), false, _settings.Rotor3IsReversed);
                    break;
                case TypexMachineType.EnigmaI:
                    stator1 = new Stator(MapTextIntoNumberSpace(Typex_EnigmaI.Rotors[_settings.Stator1], Alphabet), Typex_EnigmaI.Notches[_settings.Stator1], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[4]), _settings.Stator1IsReversed);
                    stator2 = new Stator(MapTextIntoNumberSpace(Typex_EnigmaI.Rotors[_settings.Stator2], Alphabet), Typex_EnigmaI.Notches[_settings.Stator2], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[3]), _settings.Stator2IsReversed);
                    rotor1 = new Rotor(MapTextIntoNumberSpace(Typex_EnigmaI.Rotors[_settings.Rotor1], Alphabet), Typex_EnigmaI.Notches[_settings.Rotor1], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[2]), false, _settings.Rotor1IsReversed);
                    rotor2 = new Rotor(MapTextIntoNumberSpace(Typex_EnigmaI.Rotors[_settings.Rotor2], Alphabet), Typex_EnigmaI.Notches[_settings.Rotor2], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[1]), true, _settings.Rotor2IsReversed);
                    rotor3 = new Rotor(MapTextIntoNumberSpace(Typex_EnigmaI.Rotors[_settings.Rotor3], Alphabet), Typex_EnigmaI.Notches[_settings.Rotor3], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[0]), false, _settings.Rotor3IsReversed);
                    break;
                case TypexMachineType.Y296:
                    stator1 = new Stator(MapTextIntoNumberSpace(Typex_Y296.Rotors[_settings.Stator1], Alphabet), Typex_Y296.Notches[_settings.Stator1], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[4]), _settings.Stator1IsReversed);
                    stator2 = new Stator(MapTextIntoNumberSpace(Typex_Y296.Rotors[_settings.Stator2], Alphabet), Typex_Y296.Notches[_settings.Stator2], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[3]), _settings.Stator2IsReversed);
                    rotor1 = new Rotor(MapTextIntoNumberSpace(Typex_Y296.Rotors[_settings.Rotor1], Alphabet), Typex_Y296.Notches[_settings.Rotor1], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[2]), false, _settings.Rotor1IsReversed);
                    rotor2 = new Rotor(MapTextIntoNumberSpace(Typex_Y296.Rotors[_settings.Rotor2], Alphabet), Typex_Y296.Notches[_settings.Rotor2], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[1]), true, _settings.Rotor2IsReversed);
                    rotor3 = new Rotor(MapTextIntoNumberSpace(Typex_Y296.Rotors[_settings.Rotor3], Alphabet), Typex_Y296.Notches[_settings.Rotor3], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[0]), false, _settings.Rotor3IsReversed);
                    break;
                case TypexMachineType.SP02390:
                    stator1 = new Stator(MapTextIntoNumberSpace(Typex_SP02390.Rotors[_settings.Stator1], Alphabet), Typex_SP02390.Notches[_settings.Stator1], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[4]), _settings.Stator1IsReversed);
                    stator2 = new Stator(MapTextIntoNumberSpace(Typex_SP02390.Rotors[_settings.Stator2], Alphabet), Typex_SP02390.Notches[_settings.Stator2], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[3]), _settings.Stator2IsReversed);
                    rotor1 = new Rotor(MapTextIntoNumberSpace(Typex_SP02390.Rotors[_settings.Rotor1], Alphabet), Typex_SP02390.Notches[_settings.Rotor1], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[2]), false, _settings.Rotor1IsReversed);
                    rotor2 = new Rotor(MapTextIntoNumberSpace(Typex_SP02390.Rotors[_settings.Rotor2], Alphabet), Typex_SP02390.Notches[_settings.Rotor2], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[1]), true, _settings.Rotor2IsReversed);
                    rotor3 = new Rotor(MapTextIntoNumberSpace(Typex_SP02390.Rotors[_settings.Rotor3], Alphabet), Typex_SP02390.Notches[_settings.Rotor3], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[0]), false, _settings.Rotor3IsReversed);
                    break;
                case TypexMachineType.SP02391:
                    stator1 = new Stator(MapTextIntoNumberSpace(Typex_SP02391.Rotors[_settings.Stator1], Alphabet), Typex_SP02391.Notches[_settings.Stator1], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[4]), _settings.Stator1IsReversed);
                    stator2 = new Stator(MapTextIntoNumberSpace(Typex_SP02391.Rotors[_settings.Stator2], Alphabet), Typex_SP02391.Notches[_settings.Stator2], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[3]), _settings.Stator2IsReversed);
                    rotor1 = new Rotor(MapTextIntoNumberSpace(Typex_SP02391.Rotors[_settings.Rotor1], Alphabet), Typex_SP02391.Notches[_settings.Rotor1], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[2]), false, _settings.Rotor1IsReversed);
                    rotor2 = new Rotor(MapTextIntoNumberSpace(Typex_SP02391.Rotors[_settings.Rotor2], Alphabet), Typex_SP02391.Notches[_settings.Rotor2], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[1]), true, _settings.Rotor2IsReversed);
                    rotor3 = new Rotor(MapTextIntoNumberSpace(Typex_SP02391.Rotors[_settings.Rotor3], Alphabet), Typex_SP02391.Notches[_settings.Rotor3], Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[0]), false, _settings.Rotor3IsReversed);
                    break;
                default:
                    throw new Exception(string.Format("Invalid Typex machine type: {0}", _settings.TypexMachineType));
            }
            return new Rotor[] { entry, stator1, stator2, rotor1, rotor2, rotor3 };
        }        

        /// <summary>
        /// Creates reflector for selected machine and selected reflector
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private Reflector CreateReflector()
        {
            switch (_settings.TypexReflector)
            {
                case TypexReflector.CyberChef:
                    return new Reflector(MapTextIntoNumberSpace(Typex_CyberChef.Reflector, Alphabet), null, 0);
                case TypexReflector.TestReflector:
                    return new Reflector(MapTextIntoNumberSpace(Typex_TestRotors.Reflector, Alphabet), null, 0);
                case TypexReflector.EnigmaI_A:
                    return new Reflector(MapTextIntoNumberSpace(Typex_EnigmaI.UKWA, Alphabet), null, 0);
                case TypexReflector.EnigmaI_B:
                    return new Reflector(MapTextIntoNumberSpace(Typex_EnigmaI.UKWA, Alphabet), null, 0);
                case TypexReflector.EnigmaI_C:
                    return new Reflector(MapTextIntoNumberSpace(Typex_EnigmaI.UKWA, Alphabet), null, 0);
                case TypexReflector.Custom:
                    return new Reflector(MapTextIntoNumberSpace(_settings.CustomReflector, Alphabet), null, 0);
                default:
                    throw new Exception(string.Format("Invalid reflector type: {0}", _settings.TypexMachineType));
            }
        }      

        /// <summary>
        /// Creates a plugboard
        /// </summary>
        /// <returns></returns>
        private Plugboard CreatePlugboard()
        {
            return new Plugboard(_settings.PlugSettings, Alphabet);
        }
      
        public void Stop()
        {
            _stop = true;
        }

        public void Initialize()
        {

        }

        /// <summary>
        /// En-/decrypts the given text using the given machine
        /// Handles unknown symbols as defined in the settings
        /// Also stops, if user stops (_stop = true)
        /// 
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private string CryptText(Machine machine, string text)
        {
            StringBuilder textBuilder = new StringBuilder();

            for (int position = 0; position < text.Length; position++)
            {
                //get next letter from text
                int letter = Alphabet.IndexOf(text[position]);

                //handle unknown symbols
                if (letter == -1)
                {
                    //handle unknown symbols
                    switch (_settings.UnknownSymbolHandling)
                    {
                        case UnknownSymbolHandlingMode.Ignore:
                            textBuilder.Append(text[position]);
                            break;
                        case UnknownSymbolHandlingMode.Remove:
                            //do nothing;
                            break;
                        case UnknownSymbolHandlingMode.Replace:
                            textBuilder.Append("?");
                            break;
                    }
                    continue;
                }

                //encryptd/decrypt using machine
                int cryptletter = machine.CryptLetter(letter);
                textBuilder.Append(Alphabet[cryptletter]);

                if (_stop)
                {
                    return string.Empty;
                }

                ProgressChanged(position, text.Length);
            }

            return textBuilder.ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {

        }

        /// <summary>
        /// Property of plugin has new data
        /// </summary>
        /// <param name="name"></param>
        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Log to CT2
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="logLevel"></param>
        private void GuiLogMessage(string msg, NotificationLevel logLevel)
        {
            OnGuiLogNotificationOccured?.Invoke(this, new GuiLogEventArgs(msg, this, logLevel));
        }

        /// <summary>
        /// Set the progress of this component
        /// </summary>
        /// <param name="value"></param>
        /// <param name="max"></param>
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }
    }
}