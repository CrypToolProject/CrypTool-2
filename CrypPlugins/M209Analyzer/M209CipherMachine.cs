using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace M209Analyzer
{
    enum Alphabeth
    {
        Default,
        BeaufortCipher
    }
    internal class M209CipherMachine
    {
        public M209CipherMachine(string[] lugSettingString)
        {
            SetLugSettings(lugSettingString);
        }

        public readonly string ALPHABETH = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public readonly string BEAUFORTCIPHER = "ZYXWVUTSRQPONMLKJIHGFEDCBA";

        public LugSettings LugSettings;
        public PinSettings PinSetting;

        /// <summary>
        /// The six wheels and their alphabets
        /// </summary>
        private Wheel[] _wheels = new Wheel[6]
        {
            new Wheel("ABCDEFGHIJKLMNOPQRSTUVWXYZ", 15),
            new Wheel("ABCDEFGHIJKLMNOPQRSTUVXYZ", 14),      // no W
            new Wheel("ABCDEFGHIJKLMNOPQRSTUVX", 13),        // no WYZ
            new Wheel("ABCDEFGHIJKLMNOPQRSTU", 12),          // no V-Z
            new Wheel("ABCDEFGHIJKLMNOPQRS", 11),            // no T-Z
            new Wheel("ABCDEFGHIJKLMNOPQ", 10)               // no R-Z
        };

        private Random _randomizer = new Random();

        /// <summary>
        /// Convert letter (char) into an int of the number this letter have in the alphabeth
        /// </summary>
        /// <param name="letter"></param>
        /// <param name="alphabeth"></param>
        /// <returns></returns>
        public int LetterToInt(char letter, Alphabeth alphabeth = Alphabeth.Default)
        {
            // For Spaces
            if(letter == ' ') 
            {
                letter = 'Z';
            }

            string usedAlphabeth = "";
            if (alphabeth == Alphabeth.Default)
            { 
                usedAlphabeth = this.ALPHABETH; 
            } else { 
                usedAlphabeth = this.BEAUFORTCIPHER;
            }

            for (int i = 0; i < usedAlphabeth.Length; i++)
            {
                if (usedAlphabeth[i] == letter)
                {
                    return i;
                }
            }
            
            return 0;
        }

        public char IntToLetter(int integer, Alphabeth alphabeth)
        {
            string usedAlphabeth = "";
            if (alphabeth == Alphabeth.Default)
            {
                usedAlphabeth = this.ALPHABETH;
            }
            else
            {
                usedAlphabeth = this.BEAUFORTCIPHER;
            }

            
            return usedAlphabeth[integer];            
        }

        /// <summary>
        /// Encrypt a given plainText string using the current settings
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns>ciphertext</returns>
        public string Encrypt(string plainText)
        {
            plainText = plainText.ToUpper();
            string cipherText = "";
            for (int i = 0; i < plainText.Length; i++)
            {
                int displacement = this.GetDisplacement();
                int cipherLetter = (25 - this.LetterToInt(plainText[i], Alphabeth.Default) + displacement) % 26;

                cipherText += this.IntToLetter((cipherLetter) % 26, Alphabeth.Default);
            }
            return cipherText;
        }

        /// <summary>
        /// Calculates displacement using the current settings
        /// </summary>
        /// <returns>Displacement</returns>
        public int GetDisplacement()
        {
            int displacement = 0;

            for (int i = 0; i < this.LugSettings.Bar.Length; i++)
            {
                int lug1Position = this.LugSettings.Bar[i].Value[0];
                int lug2Position = this.LugSettings.Bar[i].Value[1];

                if (lug1Position >= 0 && this.PinSetting.Wheels[lug1Position].EvaluateCurrentPin())
                {
                    displacement++;
                    continue;
                }

                if (lug2Position >= 0 && this.PinSetting.Wheels[lug2Position].EvaluateCurrentPin())
                {
                    displacement++;
                    continue;
                }
            }                       

            for (int i = 0; i < this.PinSetting.Wheels.Length; i++)
            {
                this.PinSetting.Wheels[i].Rotate();
            }
            
            return displacement;
        }

        /// <summary>
        /// Set the pins of the letters on the wheel
        /// </summary>
        /// <param name="wheelNr">The wheel where the settings will be applied</param>
        /// <param name="wheelSetting">The settings of the pins on the wheel</param>
        public void SetWheelSettings(int wheelNr, string wheelSetting)
        {
            this.PinSetting.Wheels[wheelNr].SetAllPinValuesUsingString(wheelSetting);
        }

        public void SetLugSettings(string[] lugSettingString)
        {
            LugSettings = new LugSettings(lugSettingString);
        }

        public void RandomizePinSettings()
        {
            for (int i = 0; i < this.PinSetting.Wheels.Length; i++)
            {
                this.PinSetting.Wheels[i].RandomizeAllPinValues();
            }
        }
    }
}
