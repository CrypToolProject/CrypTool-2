/*
   Copyright 2023 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using System;
using System.Text;

namespace CrypTool.LorenzSZ42.SZ42Machine
{
    /// <summary>
    /// These are limitations of the motors that 
    /// lead to a greater movement of the psi rotors
    /// </summary>
    public enum Limitation
    {
        NO_LIMITATION,
        CHI2_1BACK,
        PSI1_1BACK,
        P5_2BACK,
        PSI1_1BACK_P5_2BACK
    }

    /// <summary>
    /// There is only a difference between encryption
    /// and decryption, when the P5_2BACK or PSI1_1BACK_P5_2BACK
    /// limitations are set
    /// </summary>
    public enum Action
    {
        Encrypt,
        Decrypt,
        GenerateKey
    }

    /// <summary>
    /// Implementation of the Lorenz SZ42 ("Tunny") cipher machine 
    /// </summary>
    public class SZ42Machine
    {
        #region Wheels

        public SZ42Wheel[] ChiWheels { get; private set; } = { new SZ42Wheel(41, "ChiWheel1"), new SZ42Wheel(31, "ChiWheel2"), new SZ42Wheel(29, "ChiWheel3"), new SZ42Wheel(26, "ChiWheel4"), new SZ42Wheel(23, "ChiWheel5") };
        public SZ42Wheel[] PsiWheels { get; private set; } = { new SZ42Wheel(43, "PsiWheel1"), new SZ42Wheel(47, "PsiWheel2"), new SZ42Wheel(51, "PsiWheel3"), new SZ42Wheel(53, "PsiWheel4"), new SZ42Wheel(59, "PsiWheel5") };
        public SZ42Wheel[] MuWheels { get; private set; } = { new SZ42Wheel(61, "MuWheel1"), new SZ42Wheel(37, "MuWheel2") };

        #endregion

        /// <summary>
        /// En-/decrypts the given text using the given notations
        /// </summary>
        /// <param name="text"></param>
        /// <param name="encrypt"></param>
        /// <param name="inputNotation"></param>
        /// <param name="outputNotation"></param>
        /// <returns></returns>
        public string Crypt(string text, Action action, Limitation limitation, BaudotNotation inputNotation = BaudotNotation.Readable, BaudotNotation outputNotation = BaudotNotation.Readable)
        {
            BaudotCode baudotCode = new BaudotCode();
            byte[] baudot;

            //step 1: convert text to binary using a baudot notation
            switch (inputNotation)
            {
                case BaudotNotation.Readable:
                    baudot = baudotCode.FromReadableNotation(text);
                    break;
                case BaudotNotation.British:
                    baudot = baudotCode.FromBritishNotation(text);
                    break;
                case BaudotNotation.Raw:
                default:
                    baudot = baudotCode.FromRawPlaintext(text);
                    break;
            }

            //step 2: encrypt/decrypt

            bool p5_1back = false;
            bool p5_2back = false;

            //en-/decrypt all symbols
            for (int i = 0; i < baudot.Length; i++)
            {
                byte plaintextSymbol = baudot[i];
                byte ciphertextSymbol = CryptSymbol(plaintextSymbol, limitation, p5_2back);

                //memorize plaintext symbol (bit 5) which is "2 back"
                p5_2back = p5_1back;
                if (action == Action.Encrypt)
                {
                    p5_1back = (plaintextSymbol & 0b10000) == 0b10000;
                }
                else
                {
                    p5_1back = (ciphertextSymbol & 0b10000) == 0b10000;
                }

                baudot[i] = ciphertextSymbol;
            }

            //step 3: Convert to a string using a baudot notation

            switch (outputNotation)
            {
                case BaudotNotation.Readable:
                    return baudotCode.ToReadableNotation(baudot);
                case BaudotNotation.British:
                    return baudotCode.ToBritishNotation(baudot);
                case BaudotNotation.Raw:
                default:
                    return baudotCode.ToRawPlaintext(baudot);
            }
        }

        /// <summary>
        /// En-/decrypts a single symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private byte CryptSymbol(byte symbol, Limitation limitation, bool p5_2back)
        {
            //generate key bits
            byte chiBits = (byte)(ChiWheels[4].PinActive() << 4 | ChiWheels[3].PinActive() << 3 | ChiWheels[2].PinActive() << 2 | ChiWheels[1].PinActive() << 1 | ChiWheels[0].PinActive());
            byte psiBits = (byte)(PsiWheels[4].PinActive() << 4 | PsiWheels[3].PinActive() << 3 | PsiWheels[2].PinActive() << 2 | PsiWheels[1].PinActive() << 1 | PsiWheels[0].PinActive());
            byte keyBits = (byte)(chiBits ^ psiBits); //keystream bits          

            //determine, if Psi wheels should move based on wheel mu2
            bool psiWheelsShouldMove = MuWheels[1].PinActive() == 0b1;

            //compute limitations (if enabled)
            switch (limitation)
            {
                case Limitation.CHI2_1BACK:
                    if (ChiWheels[1].OneBack == 0b0)
                    {
                        psiWheelsShouldMove = true;
                    }
                    //memorize pins for later checks
                    ChiWheels[1].OneBack = ChiWheels[1].PinActive();
                    break;
                case Limitation.PSI1_1BACK:
                    if (!((ChiWheels[1].OneBack == 0b1) ^ (PsiWheels[0].OneBack == 0b1)))
                    {
                        psiWheelsShouldMove = true;
                    }
                    //memorize pins for later checks
                    ChiWheels[1].OneBack = ChiWheels[1].PinActive();
                    PsiWheels[0].OneBack = PsiWheels[0].PinActive();
                    break;
                case Limitation.P5_2BACK:
                    if (!((ChiWheels[1].OneBack == 0b1) ^ p5_2back))
                    {
                        psiWheelsShouldMove = true;
                    }
                    ChiWheels[1].OneBack = ChiWheels[1].PinActive();
                    break;
                case Limitation.PSI1_1BACK_P5_2BACK:

                    if (!(ChiWheels[1].OneBack == 0b1) ^ (PsiWheels[0].OneBack == 0b1) ^ p5_2back)
                    {
                        psiWheelsShouldMove = true;
                    }
                    ChiWheels[1].OneBack = ChiWheels[1].PinActive();
                    PsiWheels[0].OneBack = PsiWheels[0].PinActive();
                    break;
                case Limitation.NO_LIMITATION:
                default:
                    break;
            }

            // Chi wheels stepping (always)
            ChiWheels[0].Step();
            ChiWheels[1].Step();
            ChiWheels[2].Step();
            ChiWheels[3].Step();
            ChiWheels[4].Step();

            // Psi weels stepping (based on mu2 and motor limitations)
            if (psiWheelsShouldMove)
            {
                PsiWheels[0].Step();
                PsiWheels[1].Step();
                PsiWheels[2].Step();
                PsiWheels[3].Step();
                PsiWheels[4].Step();
            }

            // Mu wheels stepping
            // mu2 steps based on mu1
            if (MuWheels[0].PinActive() == 0b1)
            {
                MuWheels[1].Step();
            }
            //mu1 always steps
            MuWheels[0].Step();

            //return encrypted/decrypt symbol
            return (byte)(symbol ^ keyBits);
        }

        /// <summary>
        /// Parses a given key string and sets the machine accordingly
        /// </summary>
        /// <param name="keyDefinition"></param>
        /// <exception cref="ArgumentException"></exception>
        public void SetKey(string keyDefinition)
        {
            string[] lines = keyDefinition.ToLower().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length != 13)
            {
                throw new ArgumentException(string.Format(Properties.Resources.InvalidKeyDefinition1, lines.Length));
            }

            int lineNumber = 0;

            //parse Chi wheel pins
            foreach (SZ42Wheel wheel in ChiWheels)
            {
                ParseWheelDefinition(wheel, lines, lineNumber);
                lineNumber++;
            }

            //parse Psi wheel pins
            foreach (SZ42Wheel wheel in PsiWheels)
            {
                ParseWheelDefinition(wheel, lines, lineNumber);
                lineNumber++;
            }

            //parse Mu wheel pins
            foreach (SZ42Wheel wheel in MuWheels)
            {
                ParseWheelDefinition(wheel, lines, lineNumber);
                lineNumber++;
            }

            //parse all start positions
            string[] startPositions = lines[lineNumber].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (startPositions.Length != 3)
            {
                throw new ArgumentException(string.Format(Properties.Resources.InvalidKeyDefinition2, startPositions.Length));
            }

            //parse Chi wheel start positions
            string[] positions = startPositions[0].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            int positionNumber = 0;
            foreach (SZ42Wheel wheel in ChiWheels)
            {
                ParseWheelStartPosition(wheel, positions, positionNumber);
                wheel.OneBack = 0;
                positionNumber++;
            }

            //parse Psi wheel start positions
            positions = startPositions[1].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            positionNumber = 0;
            foreach (SZ42Wheel wheel in PsiWheels)
            {
                ParseWheelStartPosition(wheel, positions, positionNumber);
                wheel.OneBack = 0;
                positionNumber++;
            }

            //parse Mu wheel start positions
            positions = startPositions[2].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            positionNumber = 0;
            foreach (SZ42Wheel wheel in MuWheels)
            {
                ParseWheelStartPosition(wheel, positions, positionNumber);
                wheel.OneBack = 0;
                positionNumber++;
            }
        }

        /// <summary>
        /// Parses a wheel definition for the given wheel using the line of lines defined by lineNumber
        /// </summary>
        /// <param name="wheel"></param>
        /// <param name="lines"></param>
        /// <param name="lineNumber"></param>
        /// <exception cref="ArgumentException"></exception>
        private void ParseWheelDefinition(SZ42Wheel wheel, string[] lines, int lineNumber)
        {
            if (lines[lineNumber].Length != wheel.Pins.Length)
            {
                throw new ArgumentException(string.Format(Properties.Resources.InvalidKeyDefinition3, lineNumber, wheel.Pins.Length, lines[lineNumber].Length));
            }
            int offset = 0;
            foreach (char c in lines[lineNumber])
            {
                switch (c)
                {
                    case SZ42Wheel.ACTIVE_PIN:
                    case SZ42Wheel.INACTIVE_PIN:
                        wheel.Pins[offset] = c;
                        break;
                    default:
                        throw new ArgumentException(string.Format(Properties.Resources.InvalidKeyDefinition4, c, lineNumber));
                }
                offset++;
            }
        }

        /// <summary>
        /// Parses the start position for the given wheel using the position defined by positionNumber
        /// </summary>
        /// <param name="wheel"></param>
        /// <param name="positions"></param>
        /// <param name="positionNumber"></param>
        /// <exception cref="ArgumentException"></exception>
        private void ParseWheelStartPosition(SZ42Wheel wheel, string[] positions, int positionNumber)
        {
            int startPosition;
            try
            {
                startPosition = int.Parse(positions[positionNumber]);
            }
            catch (FormatException)
            {
                throw new ArgumentException(string.Format(Properties.Resources.InvalidKeyStartPosition, wheel.Name, wheel.Pins.Length, positions[positionNumber]));
            }
            if (startPosition < 0 || startPosition >= wheel.Pins.Length)
            {
                throw new ArgumentException(string.Format(Properties.Resources.InvalidKeyStartPosition, wheel.Name, wheel.Pins.Length, startPosition));
            }
            wheel.Position = startPosition;
        }

        /// <summary>
        /// Returns the internal key (current internal state) as a string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (SZ42Wheel wheel in ChiWheels)
            {
                builder.AppendLine(new string(wheel.Pins));
            }
            foreach (SZ42Wheel wheel in PsiWheels)
            {
                builder.AppendLine(new string(wheel.Pins));
            }
            foreach (SZ42Wheel wheel in MuWheels)
            {
                builder.AppendLine(new string(wheel.Pins));
            }

            builder.Append(string.Format("{0}:{1}:{2}:{3}:{4}|", ChiWheels[0].Position, ChiWheels[1].Position, ChiWheels[2].Position, ChiWheels[3].Position, ChiWheels[4].Position));
            builder.Append(string.Format("{0}:{1}:{2}:{3}:{4}|", PsiWheels[0].Position, PsiWheels[1].Position, PsiWheels[2].Position, PsiWheels[3].Position, PsiWheels[4].Position));
            builder.Append(string.Format("{0}:{1}", MuWheels[0].Position, MuWheels[1].Position));

            return builder.ToString();
        }
    }
}