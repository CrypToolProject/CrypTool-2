/*
   Copyright 2016, Eugen Antal, FEI STU Bratislava

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

using System.Linq;
using System.Text;

namespace CrypTool.Fialka
{
    /// <summary>
    /// Contains the Fialka cipher implementation. To make this implementation as similar to real Fialka machine as it is possible,
    /// we have to handle a lot of parameters, settings, special cases etc. We also have to specially handle the input and the output charset.
    /// See the documentation references for more details.
    /// </summary>
    public class FialkaCore
    {
        /*
         * In fact, we do not need any other inputs, all necessary data is obtaind from the internal state.
         * Some restrictions are automatically handled in the internal state.
         */
        #region Private member variables
        /// <summary>
        /// Internal state, contains all necessary data/settings.
        /// </summary>
        private readonly FialkaInternalState internalState;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="internalState"> Internal state reference.</param>
        public FialkaCore(FialkaInternalState internalState)
        {
            this.internalState = internalState;
        }
        #endregion

        #region Public member methods

        /// <summary>
        /// Delegate - function reference to handle progress changed.
        /// </summary>
        /// <param name="val"> Already processed letter's count. </param>
        /// <param name="max"> Length of the input</param>
        public delegate void LetterProcesseddelegate(double val, double max);
        /// <summary>
        /// Encryption/decryption is called based on the settings. All necessery parameters are called from the internal state. 
        /// </summary>
        /// <param name="inputStr">Input message.</param>
        /// <param name="del">Function reference, that is called after a letter is processed.</param>
        /// <returns>En(de)crypted message.</returns>
        public string encrypt(string inputStr, LetterProcesseddelegate del)
        {
            // small workaround, the punchCard inverse is calculated only if the punchCard reference is changed, if the reference array was changed
            // we are unable to detect the change - so we re-create the inverse pemutation by this special way:-)
            // I don't know the way how to capture if any value is changed an array
            internalState.punchCard = internalState.punchCard;
            // workaround end

            StringBuilder output = new StringBuilder();
            // encode input string to Z_30
            for (int i = 0; i < inputStr.Length; i++)
            {
                int inputChar = keyboardToZ30(inputStr[i]);
                if (inputChar == -1)
                {
                    if (internalState.inputHandler == FialkaEnums.handleInvalidInput.KeepUnchanged)
                    {
                        output.Append(inputStr[i]);
                    }
                    else if (internalState.inputHandler == FialkaEnums.handleInvalidInput.KeepAndMark)
                    {
                        output.Append(FialkaConstants.InvalidInputPrintSymbol);
                    }  // else  do nothing - char removed / not added :-)
                }
                else
                {
                    // valid charset
                    // encryption ....
                    int encryptedInputChar = encrypt(inputChar);
                    char outputChar = printHeadFromZ30(encryptedInputChar);
                    output.Append(outputChar);
                    // after encryption check the mixed mode, call only after printHead usage !!!
                    if (internalState.txtOpMode == FialkaEnums.textOperationmMode.Mixed)
                    {
                        mixedModeShiftCheck(inputChar, encryptedInputChar);
                    }
                }
                // callback 
                if (del != null)
                {
                    del(i + 1, inputStr.Length);
                }
            }

            return output.ToString();
        }

        /// <summary>
        /// Automatically changes the printHeadShift in case of mixed mode when the corresponding keys are processed.
        /// Based on the operationMode it can be calculated from input or output.
        /// </summary>
        /// <param name="input">Input char's Z_30 mapping.</param>
        /// <param name="output">Output char's Z_30 mapping.</param>
        private void mixedModeShiftCheck(int input, int output)
        {
            // changing the print head layout only after encryption - based on operationMode it's checked from input or output
            switch (internalState.opMode)
            {
                case FialkaEnums.operationMode.Plain:
                case FialkaEnums.operationMode.Encrypt:
                    {
                        if (input == FialkaConstants.LetterShiftContact)
                        {
                            internalState.printHeadShift = FialkaEnums.printHeadShift.LetterShift;
                        }
                        else if (input == FialkaConstants.NumerShiftContact)
                        {
                            internalState.printHeadShift = FialkaEnums.printHeadShift.NumberShift;
                        }
                    }
                    break;
                case FialkaEnums.operationMode.Decrypt:
                    {
                        if (output == FialkaConstants.LetterShiftContact)
                        {
                            internalState.printHeadShift = FialkaEnums.printHeadShift.LetterShift;
                        }
                        else if (output == FialkaConstants.NumerShiftContact)
                        {
                            internalState.printHeadShift = FialkaEnums.printHeadShift.NumberShift;
                        }
                    }
                    break;
            }
        }


        /// <summary>
        /// Wrapper method without delegate. For more details see encrypt(string,delegate)
        /// </summary>
        /// <param name="inputStr">Input message.</param>
        /// <returns>En(de)crypted message.</returns>
        public string encrypt(string inputStr)
        {
            return encrypt(inputStr, null);
        }
        #endregion

        #region Private member methods 
        /// <summary>
        /// Different encryption flow is used for different settings.
        /// M-125 and M-125-3 in Numlock30 has the same flow. 
        /// M-125-3 in NumLock10 mode has a different flow. 
        /// </summary>
        /// <param name="input">Input value (should be Z_{30}). </param>
        /// <returns>The substituted input in Z_{30}. </returns>
        private int encrypt(int input)
        {
            if (internalState.opMode != FialkaEnums.operationMode.Plain)
            {
                switch (internalState.numlockType)
                {
                    case FialkaEnums.numLockType.NumLock10: return encrypt10(input);
                    case FialkaEnums.numLockType.NumLock30: return encrypt30(input);
                }
            }

            return input;
        }

        /// <summary>
        /// The encryption flow: keyboard, punch card, entry disk, (+3 offset), 10 rotors, (-3 offset), reflector, 
        /// (+3 offset), 10 rotors inverse, (-3 offset), entry disk (inverse), punch card (inverse), keyboard (inverse),
        /// and rotor stepping (internal state change).
        /// </summary>
        /// <param name="input">Input value (should be Z_{30}). </param>
        /// <returns>The substituted input in Z_{30}. </returns>
        private int encrypt30(int input)
        {
            // initial permuatation
            input = FialkaConstants.keyboard[input];
            input = internalState.punchCard[input];
            input = FialkaConstants.entryDisk[input];
            // +3 offset between the entry disk and the rightmost rotor
            input = (input + FialkaConstants.rotorsMagicOffset) % FialkaConstants.mod;
            for (int i = 9; i >= 0; i--)
            {
                // rotors from the right (EntryDisk) to the left (Reflector)
                input = handleRotor(FialkaConstants.ED2REF, i, input);
            }
            // -3 offset between the leftmost rotor and the reflector
            input = (input - FialkaConstants.rotorsMagicOffset + FialkaConstants.mod) % FialkaConstants.mod;
            // reflector
            input = handleReflector(input);
            // +3 offset between the leftmost rotor and the reflector
            input = (input + FialkaConstants.rotorsMagicOffset) % FialkaConstants.mod;
            for (int i = 0; i < 10; i++)
            {
                // rotors from the left (Reflector) to the right (EntryDisk)
                input = handleRotor(FialkaConstants.REF2ED, i, input);
            }
            // -3 offset between the entry disk and the rightmost rotor
            input = (input - FialkaConstants.rotorsMagicOffset + FialkaConstants.mod) % FialkaConstants.mod;
            // initial permutation (inverse)
            input = FialkaConstants.entryDiskInverse[input];
            input = internalState.punchCardInverse[input];
            input = FialkaConstants.keyboardInverse[input];
            // internal state change
            rotorStepping();

            return input;
        }

        /// <summary>
        /// The encryption flow:
        /// In case of M-125-3, when the num lock switch is set to NumLock10  -
        /// also the text operation mode should be set to numbers only, and the cyrillic print head should set (in the internal state it is managed automatically)
        /// - different flow is performed. On the reflector only 10 contacts are connected in pairs, 20 contacts are not reflected, but connected
        /// to the inputs of the punch card. In this case the encryption is repeated, while the result is not reflected back directly by the reflector. 
        /// </summary>
        /// <param name="input">Input value (should be Z_{30}). </param>
        /// <returns>The substituted input in Z_{30}. </returns>
        private int encrypt10(int input)
        {
            // initial permuatation
            input = FialkaConstants.keyboard[input];
            bool endFlag = false;
            // looping while the specific wires/contacts are not hit on the reflector
            do
            {
                input = internalState.punchCard[input];
                input = FialkaConstants.entryDisk[input];
                // +3 offset between the entry disk and the rightmost rotor
                input = (input + FialkaConstants.rotorsMagicOffset) % FialkaConstants.mod;
                for (int i = 9; i >= 0; i--)
                {
                    // rotors from the right (EntryDisk) to the left (Reflector)
                    input = handleRotor(FialkaConstants.ED2REF, i, input);
                }
                // -3 offset between the leftmost rotor and the reflector
                input = (input - FialkaConstants.rotorsMagicOffset + FialkaConstants.mod) % FialkaConstants.mod;
                // reflector check
                endFlag = FialkaConstants.reflectorActiveContactsNumLock10.Contains(input);
                // not reflected back - redirected to the punch card + additional substitution is performed
                if (endFlag == false)
                {
                    input = FialkaConstants.reflectorToPunchCardNumLock10[input];
                }

            } while (endFlag == false);

            input = handleReflector(input);
            endFlag = false;
            do
            {
                // +3 offset between the leftmost rotor and the reflector
                input = (input + FialkaConstants.rotorsMagicOffset) % FialkaConstants.mod;
                for (int i = 0; i < 10; i++)
                {
                    // rotors from the left (Reflector) to the right (EntryDisk)
                    input = handleRotor(FialkaConstants.REF2ED, i, input);
                }
                // -3 offset between the entry disk and the rightmost rotor
                input = (input - FialkaConstants.rotorsMagicOffset + FialkaConstants.mod) % FialkaConstants.mod;
                // initial permutation (inverse)
                input = FialkaConstants.entryDiskInverse[input];
                input = internalState.punchCardInverse[input];
                // keyboard check
                endFlag = FialkaConstants.keyboardActiveContactsNumLock10.Contains(input);
                // not connected to the keyboard - redirected back to the reflector from the card + additional substitution is performed
                if (endFlag == false)
                {
                    input = FialkaConstants.reflectorToPunchCardNumLock10Inverse[input];
                }

            } while (endFlag == false);

            input = FialkaConstants.keyboardInverse[input];
            // internal state change
            rotorStepping();

            return input;
        }

        /// <summary>
        /// Performs the substitution on the reflector based on the selected operation mode.
        /// </summary>
        /// <param name="input">Input value (should be Z_{30}). </param>
        /// <returns>The substituted input in Z_{30}.</returns>
        private int handleReflector(int input)
        {
            switch (internalState.opMode)
            {
                case FialkaEnums.operationMode.Encrypt: return FialkaConstants.reflectorEncrypt[input];
                case FialkaEnums.operationMode.Decrypt: return FialkaConstants.reflectorDecrypt[input];
                default: return input;  // without any change, instead we should not call the encryption while the machine is in plain text mode
            };
        }

        /// <summary>
        /// Performs the specific permutation of input based on selected rotor. Firstly the offsets are removed from the rotors,
        /// next the substitution is calculated from base position. At the end the offsets are restored.
        /// </summary>
        /// <param name="inverse">Normal permutation 1, inverse permutation -1.</param>
        /// <param name="index">Index of the rotor (core) how it is inserted into the machine (reflector, rot0, ... rot9, entryDisk).</param>
        /// <param name="input">Input value (should be Z_{30}). </param>
        /// <returns>The substituted input in Z_{30}.</returns>
        private int handleRotor(int inverse, int index, int input)
        {
            // - offsets
            input += internalState.rotorOffsets[index]; // rotor offset
            input += internalState.ringOffsets[index]; // ring
            input -= internalState.coreOffsets[index]; // core offset 
            input = (input + FialkaConstants.mod/*max one part is -*/) % FialkaConstants.mod;
            // lookup table in base position without offsets
            // The corePosition determine the current wiring. In case of PROTON I, this should be the same as rotorPosition.
            input = handleRotorWiringWithCoreOrientation(inverse, internalState.coreOrientation[index], index, input);
            // + offsets
            input -= internalState.rotorOffsets[index]; // rotor offset
            input -= internalState.ringOffsets[index]; // ring
            input += internalState.coreOffsets[index]; // core offset 
            input = (input + FialkaConstants.mod2/*max 2 parts are -*/) % FialkaConstants.mod;
            return input;
        }


        /// <summary>
        /// Handle the direct rotor substitution after the offsets are 'eliminated' and rotor is in a base position.
        /// Inputs helps to determine if we have to use inverse or not. 
        /// Examples: Inverse * core flipped = (-1 * -1) = 1 = normal permutation...
        /// In case of normal permutation when the core is flipped (1 * -1) = -1 = inverse permutation is called.
        /// </summary>
        /// <param name="inverseWiring">Should be -1 if we are dealing with inverse permutation (REFLECTOR to ENTRY DISK) and 1 if we are dealing with normal (ENTRY DISK to REFLECTOR) permutation. </param>
        /// <param name="coreOrientation">Orientation (visible side) of the used core.</param>
        /// <param name="index">Index of the rotor how it is inserted into the machine (reflector, rot0, ... rot9, entryDisk).</param>
        /// <param name="input">Input value (should be Z_{30}). </param>
        /// <returns>Substitution from sub. tables.</returns>
        private int handleRotorWiringWithCoreOrientation(int inverseWiring, int coreOrientation, int index, int input)
        {
            int permType = inverseWiring * coreOrientation;

            if (permType == 1) // normal perm
            {
                if (coreOrientation == FialkaConstants.coreSideReverse)
                {
                    input = internalState.rotorWiringsFlippedCores[internalState.coreOrders[index], input];
                }
                else
                {
                    input = internalState.rotorWirings[internalState.coreOrders[index], input];
                }
            }
            else if (permType == -1) // inverse perm
            {
                if (coreOrientation == FialkaConstants.coreSideReverse)
                {
                    input = internalState.inverseRotorWiringsFlippedCores[internalState.coreOrders[index], input];
                }
                else
                {
                    input = internalState.inverseRotorWirings[internalState.coreOrders[index], input];
                }
            }
            else
            {
                // this should not happend, maybe throw some exception in the future
            }

            return input;
        }

        /// <summary>
        /// This method changes the internal state (rotor offsets) of the machine. 
        /// </summary>
        private void rotorStepping()
        {
            /*
            After a key is pressed (a letter is encrypted/decrypted) this rotor stepping should be performed.
            The rotors form two independent parts based on the order how they are inserted (if they are labeled from the left (reflector) to 
            the right (entryDisk) with increasing numbers (starting with 1) even numbers are independent from odd numbers).
            The rotation is controlled by the blocking pins. The odd rotors are controlled from the right and are rotated anti-clockwise (towards the keyboard).
            The even rotors are controlled from the left to the right and are rotated clockwise (away from the keyboard).
            The presence of the blocking pin (the position is specific for both parts) affects the rotation of all pins of the part.
            In each direction one rotor is rotated all the time (rotor labeled 2 and 9).
             */
            //even rotors indexes
            for (int i = 1; i < 10; i += 2)
            {
                // left to right 2,4,6,8,10 (indexes 1,3,5,7,9) 
                // rot offset + ring offset + even check position - clockwise
                // we do not need to handle coreOffset it does not influence the pin position
                int checkPosition = (internalState.rotorOffsets[i] + internalState.ringOffsets[i] + FialkaConstants.evenRotorsPinCheckPosition) % FialkaConstants.mod;
                // 
                internalState.rotorOffsets[i] = (internalState.rotorOffsets[i] + FialkaConstants.mod - 1) % FialkaConstants.mod;
                if (internalState.pinPositions[internalState.rotorOrders[i], checkPosition] == 1)
                {
                    break; // the next rotors are disconnected, no rotation
                }
            }
            // odd rotor indexes
            for (int i = 8; i >= 0; i -= 2)
            {
                // right to left 9,7,5,3,1 (indexes 8,6,4,2,0)
                // rot offset + ring offset + odd check position - anti clockwise
                // we do not need to handle coreOffset it does not influence the pin position
                int checkPosition = (internalState.rotorOffsets[i] + internalState.ringOffsets[i] + FialkaConstants.oddRotorsPinCheckPosition) % FialkaConstants.mod;
                // 
                internalState.rotorOffsets[i] = (internalState.rotorOffsets[i] + 1) % FialkaConstants.mod;
                if (internalState.pinPositions[internalState.rotorOrders[i], checkPosition] == 1)
                {
                    break; // the next rotors are disconnected, no rotation
                }
            }
            // handling the counter, etc..
            internalState.notifyInternalStatChangede();
        }
        #endregion

        #region Input and output encoding
        /// <summary>
        /// Input char Z_30 encoding. Charset based on various machine settings/modes: text, numbers, special chars.
        /// </summary>
        /// <param name="input">Input char</param>
        /// <returns>Encoded integer in Z_30</returns>
        public int keyboardToZ30(char input)
        {

            int encoded = FialkaConstants.InvalidInput; // init
            char c = char.ToUpper(input);
            // 1. step chceck the numbers only mode - numlock10
            if (internalState.numlockType == FialkaEnums.numLockType.NumLock10)
            {
                // in real fialka the numLock 10 blocks 20 contacts in the keyboard, to encrypt numbers only it was necessary to
                // set the textOperation mode to Numbers and change the print head to cyrillic - we auto set these settings - so
                // it's enough to check if the input is numeric digit, we omit other inputs in this case
                if (c >= '0' && c <= '9') // OK
                {
                    encoded = FialkaConstants.mapCyrillicSpecialInput(c);
                }
                // else it is invalid input (see init. of the encoded variable)
            }
            else
            {   // numLock 30 modes - necessary to check different cases
                // 2.a. step: space should be specially handled only for decrypt mode
                if (c == FialkaConstants.space)
                {
                    if (internalState.opMode == FialkaEnums.operationMode.Decrypt)
                    {
                        encoded = FialkaConstants.InvalidInput; // space is allowed only in plain and encryption, it's transformed as Й 
                    }
                    else
                    { // for encrypt and plain
                        encoded = FialkaConstants.spaceContactInDecryptMode; // faster version, add directly the contact number wthout check
                        //old version: c = FialkaConstants.spaceMapping; // space is mapped to a special input, processed as other valid inputs see below
                    }
                }
                else if (internalState.opMode == FialkaEnums.operationMode.Encrypt &&
                         FialkaConstants.spaceDisabledKey(internalState.printHead, internalState.countryLayout, input))
                {
                    // step 2.b.: in encrypt mode 1 letter is disabled to allow space
                    encoded = FialkaConstants.InvalidInput; // spaceMapping key is blocked (swapped with space key)
                }
                else
                {
                    // 3. step based on machine model - check the allowed charset
                    switch (internalState.model)
                    {
                        // it's streightforward, but we are not setting the input letter/key directly only the country specific layout and the print head
                        // one key may contains 4 different inputs, we have to check and map to a corresponding mapping
                        // M-125 contains only 2 options/key, M-125-3 contains 4 options/key
                        // we use the selected print head to determine the cyrillic / latin part
                        case FialkaEnums.machineModel.M125:
                            {
                                // cyrillic or country spec latin
                                if (internalState.printHead == FialkaEnums.printHeadMapping.Cyrillic)
                                {
                                    encoded = FialkaConstants.mapCyrillicInput(c);
                                }
                                else
                                {
                                    encoded = FialkaConstants.mapLatinInput(internalState.countryLayout, c);
                                }
                            } // end case M-125
                            break;
                        case FialkaEnums.machineModel.M125_3:
                            {
                                if (internalState.txtOpMode == FialkaEnums.textOperationmMode.Mixed)
                                {
                                    // special shorcut to [] - not presented in real fialka - easier to handle the printHeadShift
                                    if (c == FialkaConstants.MixedModeLetterShiftChars[0])
                                    {
                                        return FialkaConstants.LetterShiftContact;
                                    }
                                    else if (c == FialkaConstants.MixedModeNumberShiftChars[0])
                                    {
                                        return FialkaConstants.NumerShiftContact;
                                    }
                                } // end shortcut

                                // cyrillic or country spec latin
                                // additionaly cyrillic special or coutry spec latin special
                                if (internalState.printHead == FialkaEnums.printHeadMapping.Cyrillic)
                                {
                                    if (internalState.printHeadShift == FialkaEnums.printHeadShift.LetterShift)
                                    {
                                        encoded = FialkaConstants.mapCyrillicInput(c);
                                    }
                                    else
                                    {
                                        encoded = FialkaConstants.mapCyrillicSpecialInput(c);
                                    }
                                }
                                else
                                {
                                    if (internalState.printHeadShift == FialkaEnums.printHeadShift.LetterShift)
                                    {
                                        encoded = FialkaConstants.mapLatinInput(internalState.countryLayout, c);
                                    }
                                    else
                                    {
                                        encoded = FialkaConstants.mapSpecialInput(internalState.countryLayout, c);
                                    }
                                }
                            } // end case M-125-3
                            break;
                    } // end switch
                } // end else
            } // end numLock30

            return encoded;
        }

        /// <summary>
        /// Converts the output Z_30 mapping into char based on the specific charset.
        /// </summary>
        /// <param name="input">Output mapping in Z_30.</param>
        /// <returns>Output char based on the allowed charset.</returns>
        public char printHeadFromZ30(int input)
        {

            // 1. step handle space for decryption
            if (input == FialkaConstants.spaceContactInDecryptMode && internalState.opMode == FialkaEnums.operationMode.Decrypt)
            {
                return FialkaConstants.space;
            }
            // 2. step based on the selected print head (and printHeadShift) converts the output based on country specific charset
            // text only mode for M-125-3 == M-125 (text mode is set automatically), numbers and mixed modes are available only for M-125-3
            char output = FialkaConstants.InvalidInputPrintSymbol;
            switch (internalState.printHead)
            {
                case FialkaEnums.printHeadMapping.Cyrillic:
                    {
                        // cyrillic print head
                        output = FialkaConstants.getPrinterCharCyrillic(internalState.printHeadShift, input);
                    }
                    break;
                case FialkaEnums.printHeadMapping.Latin:
                    {
                        // latin print head
                        output = FialkaConstants.getPrinterCharLatin(internalState.countryLayout, internalState.printHeadShift, input);
                    }
                    break;
            };
            return output;
        }
        #endregion

    }
}
