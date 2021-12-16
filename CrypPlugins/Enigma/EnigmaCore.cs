/*
   Copyright 2008-2013 Arno Wacker, University of Kassel

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
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace CrypTool.Enigma
{

    internal partial class EnigmaCore
    {

        #region Private structs/classes

        private struct internalConfigStruct
        {
            public string PlugBoard;
            public int Rotor1;
            public int Rotor2;
            public int Rotor3;
            public int Rotor4;
            public int Reflector;
            public int Rotor1pos;
            public int Rotor2pos;
            public int Rotor3pos;
            public int Rotor4pos;
            public int Ring1pos;
            public int Ring2pos;
            public int Ring3pos;
            public int Ring4pos;
        }

        #endregion

        #region Private member variables

        private readonly Enigma pluginFacade;
        private readonly EnigmaSettings settings;

        private internalConfigStruct iCfg;

        private string rotor1For, rotor2For, rotor3For, rotor4For; // forward susbstitution strings for the rotors   
        private string rotor1Rev, rotor2Rev, rotor3Rev, rotor4Rev; // reverse susbstitution strings for the rotors
        private string reflector;
        private string statorFor, statorRev; // stator, i.e. "Eintrittswalze" (ETW)

        private string rotor1notches, rotor2notches, rotor3notches; // rotor notches


        #endregion

        #region Properties

        public string Key => currentKeyString();

        public string Plugs => getPlugs();

        public string Plugboard => iCfg.PlugBoard;

        public int Ring1 => iCfg.Ring1pos;
        public int Ring2 => iCfg.Ring2pos;
        public int Ring3 => iCfg.Ring3pos;
        public int Ring4 => iCfg.Ring4pos;

        public int Rotor1 => iCfg.Rotor1;
        public int Rotor2 => iCfg.Rotor2;
        public int Rotor3 => iCfg.Rotor3;
        public int Rotor4 => iCfg.Rotor4;

        public VerboseLevels VerboseLevel { get; set; }

        #endregion

        public EnigmaCore(Enigma facade)
        {
            pluginFacade = facade;
            settings = (EnigmaSettings)pluginFacade.Settings;
            iCfg = new internalConfigStruct();
        }


        /// <summary>
        /// Encrypts or decrypts a string with the given key (rotor positions)
        /// Unknown symbols are ignored (=retuned unchanged) and case is preserved.
        /// </summary>
        /// <param name="rotor1Pos">Position of rotor 1 (fastest)</param>
        /// <param name="rotor2Pos">Position of rotor 2 (middle)</param>
        /// <param name="rotor3Pos">Position of rotor 3 (slowest)</param>
        /// <param name="rotor4Pos">Position of rotor 4 (extra rotor for M4)</param>
        /// <param name="input">The text for en/decryption. All letters which are not in the alphabet are returned unprocessed.</param>
        /// <returns>The encrypted/decrypted string</returns>
        public string Encrypt(int rotor1Pos, int rotor2Pos, int rotor3Pos, int rotor4Pos, string input)
        {

            if (!Stopwatch.IsHighResolution)
            {
                logMessage("No high resolution timer available. Time measurements will be inaccurate!", NotificationLevel.Warning);
            }


            // Start the stopwatch
            Stopwatch sw = Stopwatch.StartNew();

            // set the current key, i.e. the rotor positions
            iCfg.Rotor1pos = rotor1Pos;
            iCfg.Rotor2pos = rotor2Pos;
            iCfg.Rotor3pos = rotor3Pos;
            iCfg.Rotor4pos = rotor4Pos;

            // now perform the enigma operation for each character
            char[] result = new char[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                result[i] = enigmacrypt(input[i]);

                //update the status, if we are not in anylzing mode
                // this must be deactivated during analysis, since it takes a lot of time
                if (!pluginFacade.Presentation.IsVisible)
                {
                    pluginFacade.ShowProgress(i, input.Length);
                }
            }

            // Stop the stopwatch
            sw.Stop();

            // Print some info on the console, if not in analyzing mode.
            logMessage(string.Format("Enigma processing done! Processed {0} characters in {1} ms!", input.Length, sw.ElapsedMilliseconds), NotificationLevel.Info);

            return new string(result);
        }

        /// <summary>
        /// This method set the internal configuration of the enigma, i.e.
        /// The used rotors, their ring position and the current plugboard substitution
        /// </summary>
        /// <param name="rotor1">The rightmost (fastest) rotor</param>
        /// <param name="rotor2">The middle rotor</param>
        /// <param name="rotor3">The leftmost (slowest) rotor</param>
        /// <param name="rotor4">The extra rotor for the M4</param>
        /// <param name="ring1Pos">The ring position for the rightmost (fastest) rotor</param>
        /// <param name="ring2Pos">The ring position for the middle rotor</param>
        /// <param name="ring3Pos">The ring position for the leftmost (slowest) rotor</param>
        /// <param name="ring4Pos">The ring position for the extra rotor for the M4</param>
        /// <param name="plugBoard">An alphabet string providing the substitution given by the Plugboard</param>
        public void setInternalConfig(int rotor1, int rotor2, int rotor3, int rotor4,
            int refl,
            int ring1Pos, int ring2Pos, int ring3Pos, int ring4Pos, string plugBoard)
        {
            //set the internal config elements
            iCfg.Rotor1 = rotor1;
            iCfg.Rotor2 = rotor2;
            iCfg.Rotor3 = rotor3;
            iCfg.Rotor4 = rotor4;

            iCfg.Reflector = refl;

            iCfg.Ring1pos = ring1Pos;
            iCfg.Ring2pos = ring2Pos;
            iCfg.Ring3pos = ring3Pos;
            iCfg.Ring4pos = ring4Pos;

            iCfg.PlugBoard = plugBoard;


            logMessage(string.Format("Setting internal config to Rotors: {0},{1},{2}; Rings: {3},{4},{5}; Reflector: {6}; Plugboard: \"{7}\"",
                (rotorEnum)iCfg.Rotor3, (rotorEnum)iCfg.Rotor2, (rotorEnum)iCfg.Rotor1,
                iCfg.Ring3pos, iCfg.Ring2pos, iCfg.Ring1pos,
                (reflectorEnum)iCfg.Reflector,
                getPlugs()), NotificationLevel.Info);


            ///////////////////////////////////////
            // now prepare everything for operation
            ///////////////////////////////////////

            // prepare the current string for the forward substitution. We use a the string 3-times, 
            // since this saves some alphabet wrap-arround checking
            rotor1For = rotors[settings.Model, iCfg.Rotor1] + rotors[settings.Model, iCfg.Rotor1] + rotors[settings.Model, iCfg.Rotor1];
            rotor2For = rotors[settings.Model, iCfg.Rotor2] + rotors[settings.Model, iCfg.Rotor2] + rotors[settings.Model, iCfg.Rotor2];
            rotor3For = rotors[settings.Model, iCfg.Rotor3] + rotors[settings.Model, iCfg.Rotor3] + rotors[settings.Model, iCfg.Rotor3];
            if (iCfg.Rotor4 < 8)
            {
                rotor4For = rotors[settings.Model, iCfg.Rotor4] + rotors[settings.Model, iCfg.Rotor4] + rotors[settings.Model, iCfg.Rotor4];
            }


            if (settings.Model > 0) // the Enigma A/B did not have a reflector, hence there is no reverse substitution
            {
                // prepare the current string for the reflector
                reflector = reflectors[settings.Model, iCfg.Reflector] + reflectors[settings.Model, iCfg.Reflector] + reflectors[settings.Model, iCfg.Reflector];

                // prepare the current string for the reverse substitution.
                rotor1Rev = generateReverseSubst(rotors[settings.Model, iCfg.Rotor1]);
                rotor1Rev += rotor1Rev + rotor1Rev;
                rotor2Rev = generateReverseSubst(rotors[settings.Model, iCfg.Rotor2]);
                rotor2Rev += rotor2Rev + rotor2Rev;
                rotor3Rev = generateReverseSubst(rotors[settings.Model, iCfg.Rotor3]);
                rotor3Rev += rotor3Rev + rotor3Rev;
                if (iCfg.Rotor4 < 8)
                {
                    rotor4Rev = generateReverseSubst(rotors[settings.Model, iCfg.Rotor4]);
                    rotor4Rev += rotor4Rev + rotor4Rev;
                }
            }

            // configure rotor notches
            rotor1notches = notches[settings.Model, iCfg.Rotor1];
            rotor2notches = notches[settings.Model, iCfg.Rotor2];
            rotor3notches = notches[settings.Model, iCfg.Rotor3];

            // configure the stator
            statorFor = stators[settings.Model];
            statorRev = generateReverseSubst(statorFor);

        }


        /// <summary>
        /// Generates the inverse substitution string for a given substitution
        /// </summary>
        /// <param name="p">The substitution string</param>
        /// <returns>The reversed substitution string</returns>
        private string generateReverseSubst(string p)
        {
            char[] result = new char[alen];

            for (int i = 0; i < alen; i++)
            {
                result[i] = settings.Alphabet[p.IndexOf(settings.Alphabet[i])];
            }

            return new string(result);
        }


        /// <summary>
        /// Encrypt one char at the time. Only letters from the alphabet are allowed.
        /// This mast be taken care by the calling method. Illegal characters will lead 
        /// to exception!
        /// </summary>
        /// <param name="keypressed"></param>
        /// <returns></returns>
        private char enigmacrypt(char keypressed)
        {
            //1. Substitution with plugboard
            char entrySubst = enigmaPlugBoard(keypressed);

            //2. Spindle substitution
            char spindleSubst = enigmaSpindle(entrySubst);

            //3. Substitution with plugboard
            return enigmaPlugBoard(spindleSubst);
        }


        /// <summary>
        /// The method simulates the substition done by the plugboard of the Enigma.
        /// This part provieds the monoalpahbetic substitution for the Enigma.
        /// </summary>
        /// <param name="c">The letter to be substituted.</param>
        /// <returns>The substituted character</returns>
        private char enigmaPlugBoard(char c)
        {
            return iCfg.PlugBoard[settings.AlphabetIndexOf(c)];
        }


        /// <summary>
        /// This method simulated the substitution done by the rotors, i.e.
        /// the spindle. This part provides the polyalphabetic substitution for the Enigma
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private char enigmaSpindle(char entrySubst)
        {
            char ch = entrySubst;

            // we need to consider the stator here:
            ch = statorFor[settings.AlphabetIndexOf(ch)];

            //check notches and update the rotor position. notches of rotor2 were checked first to avoid double-stepping (mechanical impossible)
            foreach (char n in rotor2notches)
            {
                int currentRotor2pos = iCfg.Rotor2pos;

                if (settings.AlphabetIndexOf(n) == currentRotor2pos)
                {
                    iCfg.Rotor3pos = (iCfg.Rotor3pos + 1) % alen;
                    iCfg.Rotor2pos = (iCfg.Rotor2pos + 1) % alen; // step of rotor 3 takes rotor 2 with it 
                }
            }

            foreach (char n in rotor3notches)
            {
                if (settings.AlphabetIndexOf(n) == iCfg.Rotor3pos)
                {
                    iCfg.Rotor4pos = (iCfg.Rotor4pos + 1) % alen;
                }
            }


            foreach (char n in rotor1notches)
            {
                if (settings.AlphabetIndexOf(n) == iCfg.Rotor1pos)
                {
                    iCfg.Rotor2pos = (iCfg.Rotor2pos + 1) % alen;
                }
            }
            // Rotor 1 always steps
            iCfg.Rotor1pos = (iCfg.Rotor1pos + 1) % alen;


            // write back the updated rotor settings (only if not analyzing)
            //if (settings.Action == 0)
            //    settings.Key = currentKeyString();

            //add the ring-offset
            int rotor1Pos = iCfg.Rotor1pos - (iCfg.Ring1pos - 1); if (rotor1Pos < 0)
            {
                rotor1Pos += alen;
            }

            int rotor2Pos = iCfg.Rotor2pos - (iCfg.Ring2pos - 1); if (rotor2Pos < 0)
            {
                rotor2Pos += alen;
            }

            int rotor3Pos = iCfg.Rotor3pos - (iCfg.Ring3pos - 1); if (rotor3Pos < 0)
            {
                rotor3Pos += alen;
            }
            //int rotor3Pos = (alen + iCfg.Rotor3pos - (iCfg.Ring3pos - 1)) % alen; // slower alternative

            // now do the substitution
            ch = A3[alen + settings.AlphabetIndexOf(rotor1For[alen + settings.AlphabetIndexOf(ch) + rotor1Pos]) - rotor1Pos];
            ch = A3[alen + settings.AlphabetIndexOf(rotor2For[alen + settings.AlphabetIndexOf(ch) + rotor2Pos]) - rotor2Pos];
            ch = A3[alen + settings.AlphabetIndexOf(rotor3For[alen + settings.AlphabetIndexOf(ch) + rotor3Pos]) - rotor3Pos];
            ch = reflector[alen + settings.AlphabetIndexOf(ch)];
            ch = A3[alen + settings.AlphabetIndexOf(rotor3Rev[alen + settings.AlphabetIndexOf(ch) + rotor3Pos]) - rotor3Pos];
            ch = A3[alen + settings.AlphabetIndexOf(rotor2Rev[alen + settings.AlphabetIndexOf(ch) + rotor2Pos]) - rotor2Pos];
            ch = A3[alen + settings.AlphabetIndexOf(rotor1Rev[alen + settings.AlphabetIndexOf(ch) + rotor1Pos]) - rotor1Pos];

            //again, we need to consider the stator here
            ch = statorRev[settings.AlphabetIndexOf(ch)];

            return ch;
        }


        /// <summary>
        /// Returns the current key as a string, i.e. the rotor positions in the settings pane
        /// </summary>
        private string currentKeyString()
        {
            char R1 = settings.Alphabet[iCfg.Rotor1pos];
            char R2 = settings.Alphabet[iCfg.Rotor2pos];
            char R3 = settings.Alphabet[iCfg.Rotor3pos];
            char R4 = settings.Alphabet[iCfg.Rotor4pos];

            string key = R3.ToString() + R2.ToString() + R1.ToString();

            if (settings.Model == 4)
            {
                return R4 + key;
            }
            else
            {
                return key;
            }
        }

        private string getPlugs()
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < settings.Alphabet.Length; i++)
            {
                if (settings.Alphabet[i] != iCfg.PlugBoard[i] && !result.ToString().Contains(settings.Alphabet[i]))
                {
                    if (result.Length > 0)
                    {
                        result.Append(' ');
                    }

                    result.Append(settings.Alphabet[i].ToString() + iCfg.PlugBoard[i].ToString());
                }
            }

            if (result.Length == 0)
            {
                result.Append("-- no plugs --");
            }

            return result.ToString();
        }

        private void logMessage(string msg, NotificationLevel lvl)
        {
            if ((int)lvl >= ((int)VerboseLevel - 1))
            {
                pluginFacade.LogMessage(msg, lvl);
            }
        }
    }
}
