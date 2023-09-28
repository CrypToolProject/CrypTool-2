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
using System.Linq;
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

        [PropertyInfo(Direction.InputData, "KeyInputCaption", "KeyInputTooltip", false)]
        public string KeyInput
        {
            get;
            set;
        }


        public void PreExecution()
        {
            KeyInput = string.Empty;
            InputText = string.Empty;
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
            Machine machine;

            if (KeyInput != string.Empty)
            {
                machine = CreateMachineUsingProvidedKeyString();                
            }
            else
            {
                machine = new Machine(CreateRotorArray(), CreateReflector(), CreatePlugboard(), Alphabet);
            }

            OutputText = CryptText(machine, InputText.ToUpper());
            OnPropertyChanged(nameof(OutputText));

            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Creates a Typex machine using the provided input key settings
        /// </summary>
        /// <returns></returns>
        private Machine CreateMachineUsingProvidedKeyString()
        {
            string[] keyElements = KeyInput.Split(':');

            //create reflector
            Reflector reflector = null;

            string reflector_definition = keyElements[0].ToLower();
            switch (reflector_definition)
            {
                case "cyberchef":
                    reflector =  new Reflector(MapTextIntoNumberSpace(Typex_CyberChef.Reflector, Alphabet), null, 0, 0);
                    break;
                case "testreflector":
                    reflector = new Reflector(MapTextIntoNumberSpace(Typex_TestRotors.Reflector, Alphabet), null, 0, 0);
                    break;
                case "enigmaa":
                    reflector = new Reflector(MapTextIntoNumberSpace(Typex_EnigmaI.UKWA, Alphabet), null, 0, 0);
                    break;
                case "enigmab":
                    reflector = new Reflector(MapTextIntoNumberSpace(Typex_EnigmaI.UKWB, Alphabet), null, 0, 0);
                    break;
                case "enigmac":
                    reflector = new Reflector(MapTextIntoNumberSpace(Typex_EnigmaI.UKWC, Alphabet), null, 0, 0);
                    break;               
                default:
                    if (!reflector_definition.StartsWith("custom"))
                    {
                        throw new Exception("Unknown reflector definition. Valid are: cyberchef, testreflector, enigmaa, enigmab, enigmac, custom;abc...xyz");
                    }
                    else
                    {
                        //custom reflector is separated using a semicolon
                        string[] reflector_definition_parts = reflector_definition.Split(';');
                        reflector = new Reflector(MapTextIntoNumberSpace(reflector_definition_parts[1].ToUpper(), Alphabet), null, 0, 0);
                    }
                    break;
            }

            //create rotors
            Rotor[] rotors = new Rotor[6];
            string rotor_definitions = keyElements[1].ToUpper();
            
            //rotor selection
            if (rotor_definitions.Length != 15)
            {
                throw new Exception("Invalid rotor selection. A rotor is defined by two digits and a letter (n=normal, r=reversed). An example for a valid selection is : 01n02n03n04n05n");
            }

            //rotor positions
            string rotor_positions = keyElements[2].ToUpper();            
            if (rotor_positions.Length != 5)
            {
                throw new Exception("Invalid rotor-position definition. An example for a valid rotor-position definition is: AAAAA");
            }

            //ring positions
            string ring_positions = keyElements[3].ToUpper();            
            if (ring_positions.Length != 5)
            {
                throw new Exception("Invalid ring-position definition. An example for a valid ring-position definition is: AAAAA");
            }

            //create entry
            Stator entry = new Stator(MapTextIntoNumberSpace(Entry, Alphabet), null, 0, 0, false);

            rotors[0] = entry;
            for (int i = 0; i < 5; i++)
            {
                rotors[i + 1] = CreateRotor(rotor_definitions.Substring(i * 3, 3), rotor_positions[4 - i].ToString(), ring_positions[4 - i].ToString(), i == 3, i < 2); // i == 3 <- only this rotor has the anomaly
            }

            //create plugboard
            string plugboard_definition = string.Concat(keyElements[4].ToUpper().Distinct());
            if(plugboard_definition.Length != 26)
            {
                throw new Exception("Invalid plugboard definition. You need to provide a permuted 26-letter alphabet");
            }

            return new Machine(rotors, reflector, new Plugboard(plugboard_definition, Alphabet));
        }

        /// <summary>
        /// Creates a rotor for the currently selected machine based on the rotor string
        /// </summary>
        /// <param name="rotor"></param>
        /// <param name="rotor_position"></param>
        /// <param name="ring_position"></param>
        /// <param name="stator"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private Rotor CreateRotor(string rotor, string rotor_position, string ring_position, bool hasAnomaly, bool stator)
        {
            try
            {
                int rotor_selection = int.Parse(rotor.Substring(0, 2)) - 1;
                bool isReversed = rotor.ToUpper()[2] == 'R';

                switch (_settings.TypexMachineType)
                {
                    case TypexMachineType.CyberChef:
                        if (stator)
                        {
                            return new Stator(MapTextIntoNumberSpace(Typex_CyberChef.Rotors[rotor_selection], Alphabet), Typex_CyberChef.Notches[rotor_selection], Alphabet.IndexOf(ring_position), Alphabet.IndexOf(rotor_position), isReversed);
                        }
                        else
                        {
                            return new Rotor(MapTextIntoNumberSpace(Typex_CyberChef.Rotors[rotor_selection], Alphabet), Typex_CyberChef.Notches[rotor_selection], Alphabet.IndexOf(ring_position), Alphabet.IndexOf(rotor_position), hasAnomaly, isReversed);
                        }
                    case TypexMachineType.TestRotors:
                        if (stator)
                        {
                            return new Stator(MapTextIntoNumberSpace(Typex_TestRotors.Rotors[rotor_selection], Alphabet), Typex_TestRotors.Notches[rotor_selection], Alphabet.IndexOf(ring_position), Alphabet.IndexOf(rotor_position), isReversed);
                        }
                        else
                        {
                            return new Rotor(MapTextIntoNumberSpace(Typex_TestRotors.Rotors[rotor_selection], Alphabet), Typex_TestRotors.Notches[rotor_selection], Alphabet.IndexOf(ring_position), Alphabet.IndexOf(rotor_position), hasAnomaly, isReversed);
                        }
                    case TypexMachineType.EnigmaI:
                        if (stator)
                        {
                            return new Stator(MapTextIntoNumberSpace(Typex_EnigmaI.Rotors[rotor_selection], Alphabet), Typex_EnigmaI.Notches[rotor_selection], Alphabet.IndexOf(ring_position), Alphabet.IndexOf(rotor_position), isReversed);
                        }
                        else
                        {
                            return new Rotor(MapTextIntoNumberSpace(Typex_EnigmaI.Rotors[rotor_selection], Alphabet), Typex_EnigmaI.Notches[rotor_selection], Alphabet.IndexOf(ring_position), Alphabet.IndexOf(rotor_position), hasAnomaly, isReversed);
                        }
                    case TypexMachineType.Y296:
                        if (stator)
                        {
                            return new Stator(MapTextIntoNumberSpace(Typex_Y296.Rotors[rotor_selection], Alphabet), Typex_Y296.Notches[rotor_selection], Alphabet.IndexOf(ring_position), Alphabet.IndexOf(rotor_position), isReversed);
                        }
                        else
                        {
                            return new Rotor(MapTextIntoNumberSpace(Typex_Y296.Rotors[rotor_selection], Alphabet), Typex_Y296.Notches[rotor_selection], Alphabet.IndexOf(ring_position), Alphabet.IndexOf(rotor_position), hasAnomaly, isReversed);
                        }                       
                    case TypexMachineType.SP02390:
                        if (stator)
                        {
                            return new Stator(MapTextIntoNumberSpace(Typex_SP02390.Rotors[rotor_selection], Alphabet), Typex_SP02390.Notches[rotor_selection], Alphabet.IndexOf(ring_position), Alphabet.IndexOf(rotor_position), isReversed);
                        }
                        else
                        {
                            return new Rotor(MapTextIntoNumberSpace(Typex_SP02390.Rotors[rotor_selection], Alphabet), Typex_SP02390.Notches[rotor_selection], Alphabet.IndexOf(ring_position), Alphabet.IndexOf(rotor_position), hasAnomaly, isReversed);
                        }                    
                    case TypexMachineType.SP02391:
                        if (stator)
                        {
                            return new Stator(MapTextIntoNumberSpace(Typex_SP02391.Rotors[rotor_selection], Alphabet), Typex_SP02391.Notches[rotor_selection], Alphabet.IndexOf(ring_position), Alphabet.IndexOf(rotor_position), isReversed);
                        }
                        else
                        {
                            return new Rotor(MapTextIntoNumberSpace(Typex_SP02391.Rotors[rotor_selection], Alphabet), Typex_SP02391.Notches[rotor_selection], Alphabet.IndexOf(ring_position), Alphabet.IndexOf(rotor_position), hasAnomaly, isReversed);
                        }                       
                    default:
                        throw new Exception(string.Format("Invalid Typex machine type: {0}", _settings.TypexMachineType));
                }
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Invalid rotor definition: {0} {1} {2}", rotor, rotor_position, ring_position));
            }
        }

        /// <summary>
        /// Creates rotor array for selected machine and selected rotors and start positions
        /// Also adds the entry stator (reversed rotor) to the beginning of the array
        /// </summary>
        /// <returns></returns>
        private Rotor[] CreateRotorArray()
        {
            //entry stator reverses the alphabet
            Stator entry = new Stator(MapTextIntoNumberSpace(Entry, Alphabet), null, 0, 0, false);

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
                    stator1 = new Stator(MapTextIntoNumberSpace(Typex_CyberChef.Rotors[_settings.Stator1], Alphabet), Typex_CyberChef.Notches[_settings.Stator1], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[4]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[4]), _settings.Stator1IsReversed);
                    stator2 = new Stator(MapTextIntoNumberSpace(Typex_CyberChef.Rotors[_settings.Stator2], Alphabet), Typex_CyberChef.Notches[_settings.Stator2], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[3]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[3]), _settings.Stator2IsReversed);
                    rotor1 = new Rotor(MapTextIntoNumberSpace(Typex_CyberChef.Rotors[_settings.Rotor1], Alphabet), Typex_CyberChef.Notches[_settings.Rotor1], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[2]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[2]), false, _settings.Rotor1IsReversed);
                    rotor2 = new Rotor(MapTextIntoNumberSpace(Typex_CyberChef.Rotors[_settings.Rotor2], Alphabet), Typex_CyberChef.Notches[_settings.Rotor2], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[1]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[1]), true, _settings.Rotor2IsReversed);
                    rotor3 = new Rotor(MapTextIntoNumberSpace(Typex_CyberChef.Rotors[_settings.Rotor3], Alphabet), Typex_CyberChef.Notches[_settings.Rotor3], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[0]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[0]), false, _settings.Rotor3IsReversed);
                    break;
                case TypexMachineType.TestRotors:
                    stator1 = new Stator(MapTextIntoNumberSpace(Typex_TestRotors.Rotors[_settings.Stator1], Alphabet), Typex_TestRotors.Notches[_settings.Stator1], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[4]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[4]), _settings.Stator1IsReversed);
                    stator2 = new Stator(MapTextIntoNumberSpace(Typex_TestRotors.Rotors[_settings.Stator2], Alphabet), Typex_TestRotors.Notches[_settings.Stator2], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[3]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[3]), _settings.Stator2IsReversed);
                    rotor1 = new Rotor(MapTextIntoNumberSpace(Typex_TestRotors.Rotors[_settings.Rotor1], Alphabet), Typex_TestRotors.Notches[_settings.Rotor1], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[2]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[2]), false, _settings.Rotor1IsReversed);
                    rotor2 = new Rotor(MapTextIntoNumberSpace(Typex_TestRotors.Rotors[_settings.Rotor2], Alphabet), Typex_TestRotors.Notches[_settings.Rotor2], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[1]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[1]), true, _settings.Rotor2IsReversed);
                    rotor3 = new Rotor(MapTextIntoNumberSpace(Typex_TestRotors.Rotors[_settings.Rotor3], Alphabet), Typex_TestRotors.Notches[_settings.Rotor3], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[0]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[0]), false, _settings.Rotor3IsReversed);
                    break;
                case TypexMachineType.EnigmaI:
                    stator1 = new Stator(MapTextIntoNumberSpace(Typex_EnigmaI.Rotors[_settings.Stator1], Alphabet), Typex_EnigmaI.Notches[_settings.Stator1], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[4]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[4]), _settings.Stator1IsReversed);
                    stator2 = new Stator(MapTextIntoNumberSpace(Typex_EnigmaI.Rotors[_settings.Stator2], Alphabet), Typex_EnigmaI.Notches[_settings.Stator2], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[3]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[3]), _settings.Stator2IsReversed);
                    rotor1 = new Rotor(MapTextIntoNumberSpace(Typex_EnigmaI.Rotors[_settings.Rotor1], Alphabet), Typex_EnigmaI.Notches[_settings.Rotor1], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[2]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[2]), false, _settings.Rotor1IsReversed);
                    rotor2 = new Rotor(MapTextIntoNumberSpace(Typex_EnigmaI.Rotors[_settings.Rotor2], Alphabet), Typex_EnigmaI.Notches[_settings.Rotor2], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[1]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[1]), true, _settings.Rotor2IsReversed);
                    rotor3 = new Rotor(MapTextIntoNumberSpace(Typex_EnigmaI.Rotors[_settings.Rotor3], Alphabet), Typex_EnigmaI.Notches[_settings.Rotor3], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[0]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[0]), false, _settings.Rotor3IsReversed);
                    break;
                case TypexMachineType.Y296:
                    stator1 = new Stator(MapTextIntoNumberSpace(Typex_Y296.Rotors[_settings.Stator1], Alphabet), Typex_Y296.Notches[_settings.Stator1], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[4]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[4]), _settings.Stator1IsReversed);
                    stator2 = new Stator(MapTextIntoNumberSpace(Typex_Y296.Rotors[_settings.Stator2], Alphabet), Typex_Y296.Notches[_settings.Stator2], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[3]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[3]), _settings.Stator2IsReversed);
                    rotor1 = new Rotor(MapTextIntoNumberSpace(Typex_Y296.Rotors[_settings.Rotor1], Alphabet), Typex_Y296.Notches[_settings.Rotor1], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[2]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[2]), false, _settings.Rotor1IsReversed);
                    rotor2 = new Rotor(MapTextIntoNumberSpace(Typex_Y296.Rotors[_settings.Rotor2], Alphabet), Typex_Y296.Notches[_settings.Rotor2], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[1]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[1]), true, _settings.Rotor2IsReversed);
                    rotor3 = new Rotor(MapTextIntoNumberSpace(Typex_Y296.Rotors[_settings.Rotor3], Alphabet), Typex_Y296.Notches[_settings.Rotor3], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[0]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[0]), false, _settings.Rotor3IsReversed);
                    break;
                case TypexMachineType.SP02390:
                    stator1 = new Stator(MapTextIntoNumberSpace(Typex_SP02390.Rotors[_settings.Stator1], Alphabet), Typex_SP02390.Notches[_settings.Stator1], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[4]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[4]), _settings.Stator1IsReversed);
                    stator2 = new Stator(MapTextIntoNumberSpace(Typex_SP02390.Rotors[_settings.Stator2], Alphabet), Typex_SP02390.Notches[_settings.Stator2], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[3]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[3]), _settings.Stator2IsReversed);
                    rotor1 = new Rotor(MapTextIntoNumberSpace(Typex_SP02390.Rotors[_settings.Rotor1], Alphabet), Typex_SP02390.Notches[_settings.Rotor1], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[2]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[2]), false, _settings.Rotor1IsReversed);
                    rotor2 = new Rotor(MapTextIntoNumberSpace(Typex_SP02390.Rotors[_settings.Rotor2], Alphabet), Typex_SP02390.Notches[_settings.Rotor2], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[1]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[1]), true, _settings.Rotor2IsReversed);
                    rotor3 = new Rotor(MapTextIntoNumberSpace(Typex_SP02390.Rotors[_settings.Rotor3], Alphabet), Typex_SP02390.Notches[_settings.Rotor3], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[0]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[0]), false, _settings.Rotor3IsReversed);
                    break;
                case TypexMachineType.SP02391:
                    stator1 = new Stator(MapTextIntoNumberSpace(Typex_SP02391.Rotors[_settings.Stator1], Alphabet), Typex_SP02391.Notches[_settings.Stator1], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[4]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[4]), _settings.Stator1IsReversed);
                    stator2 = new Stator(MapTextIntoNumberSpace(Typex_SP02391.Rotors[_settings.Stator2], Alphabet), Typex_SP02391.Notches[_settings.Stator2], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[3]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[3]), _settings.Stator2IsReversed);
                    rotor1 = new Rotor(MapTextIntoNumberSpace(Typex_SP02391.Rotors[_settings.Rotor1], Alphabet), Typex_SP02391.Notches[_settings.Rotor1], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[2]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[2]), false, _settings.Rotor1IsReversed);
                    rotor2 = new Rotor(MapTextIntoNumberSpace(Typex_SP02391.Rotors[_settings.Rotor2], Alphabet), Typex_SP02391.Notches[_settings.Rotor2], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[1]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[1]), true, _settings.Rotor2IsReversed);
                    rotor3 = new Rotor(MapTextIntoNumberSpace(Typex_SP02391.Rotors[_settings.Rotor3], Alphabet), Typex_SP02391.Notches[_settings.Rotor3], Alphabet.IndexOf(_settings.RingPositions.ToUpper()[0]), Alphabet.IndexOf(_settings.RotorPositions.ToUpper()[0]), false, _settings.Rotor3IsReversed);
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
                    return new Reflector(MapTextIntoNumberSpace(Typex_CyberChef.Reflector, Alphabet), null, 0, 0);
                case TypexReflector.TestReflector:
                    return new Reflector(MapTextIntoNumberSpace(Typex_TestRotors.Reflector, Alphabet), null, 0, 0);
                case TypexReflector.EnigmaI_A:
                    return new Reflector(MapTextIntoNumberSpace(Typex_EnigmaI.UKWA, Alphabet), null, 0, 0);
                case TypexReflector.EnigmaI_B:
                    return new Reflector(MapTextIntoNumberSpace(Typex_EnigmaI.UKWB, Alphabet), null, 0, 0);
                case TypexReflector.EnigmaI_C:
                    return new Reflector(MapTextIntoNumberSpace(Typex_EnigmaI.UKWC, Alphabet), null, 0, 0);
                case TypexReflector.Custom:
                    return new Reflector(MapTextIntoNumberSpace(_settings.CustomReflector, Alphabet), null, 0, 0);
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