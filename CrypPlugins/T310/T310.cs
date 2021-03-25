/*
   Copyright 1995 - 2011 Jörg Drobick

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
using System.Collections.Generic;
using System.Text;
using CrypTool.PluginBase;
using System.ComponentModel;
using CrypTool.PluginBase.Miscellaneous;
using System.Windows.Controls;


namespace CrypTool.Plugins.T310
{
    [Author("Jörg Drobick, Matthäus Wander", "ct2contact@CrypTool.org", "", "")]
    [PluginInfo("T_310.Properties.Resources", "PluginCaption", "PluginTooltip", "T310/DetailedDescription/doc.xml", "T310/Images/t310.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class T310 : ICrypComponent
    {
        #region Private Variables

        private bool stopPressed = false;

        private readonly T310Settings settings = new T310Settings();
        private static uint syncSequenceLength = 33;


        private const byte figs = 0x1B; // A CCITT-2 control character, indicates a figure is following
        private const byte ltrs = 0x1F; // A CCITT-2 control character, indicates a letter is following

        //The algorithm parts of the T310
        private ControlUnit controlUnit;
        private SynchronizationUnit synchronizationUnit;
        private ComplexUnit complexUnit;
        private EncryptionUnit encryptionUnit;

        /*
         * Lookup and reverse lookup tables for converting between ASCII and CCITT-2
         */
        private static Dictionary<byte, byte> ccitLetters = new Dictionary<byte, byte>
        {
            {0x00, 0x00}, // Null (blank tape)
            {0x01, 0x45}, // E
            {0x02, 0x0A}, // LR (Line Feed)
            {0x03, 0x41}, // A
            {0x04, 0x20}, // Space
            {0x05, 0x53}, // S
            {0x06, 0x49}, // I 
            {0x07, 0x55}, // U
            {0x08, 0x0D}, // CR (Carriage Return)
            {0x09, 0x44}, // D
            {0x0A, 0x52}, // R
            {0x0B, 0x4A}, // J
            {0x0C, 0x4E}, // N
            {0x0D, 0x46}, // F
            {0x0E, 0x43}, // C
            {0x0F, 0x4B}, // K
            {0x10, 0x54}, // T
            {0x11, 0x5A}, // Z
            {0x12, 0x4C}, // L
            {0x13, 0x57}, // W
            {0x14, 0x48}, // H
            {0x15, 0x59}, // Y
            {0x16, 0x50}, // P
            {0x17, 0x51}, // Q
            {0x18, 0x4F}, // O
            {0x19, 0x42}, // B
            {0x1A, 0x47}, // G
            {0x1B, 0x00}, // FIGS (control character - no ASCII representation)
            {0x1C, 0x4D}, // M
            {0x1D, 0x58}, // X
            {0x1E, 0x56}, // V
            {0x1F, 0x00}  // LTRS (control character - no ASCII representation)
        };
        private static Dictionary<byte, byte> ccitFigures = new Dictionary<byte, byte>
        {
            {0x00, 0x00}, // Null (blank tape)
            {0x01, 0x33}, // 3
            {0x02, 0x0A}, // LR (Line Feed)
            {0x03, 0x2D}, // -
            {0x04, 0x20}, // Space
            {0x05, 0x27}, // '
            {0x06, 0x38}, // 8 
            {0x07, 0x37}, // 7
            {0x08, 0x0D}, // CR (Carriage Return)
            {0x09, 0x05}, // ENC (Enquiry, Who are you?, WRU)
            {0x0A, 0x34}, // 4 
            {0x0B, 0x07}, // BEL (Bell, ring at other end)
            {0x0c, 0x2C}, // ,
            {0x0D, 0x21}, // !
            {0x0E, 0x3A}, // :
            {0x0F, 0x28}, // (
            {0x10, 0x35}, // 5
            {0x11, 0x2B}, // +
            {0x12, 0x29}, // )
            {0x13, 0x32}, // 2
            {0x14, 0x24}, // $
            {0x15, 0x36}, // 6
            {0x16, 0x30}, // 0
            {0x17, 0x31}, // 1
            {0x18, 0x39}, // 9
            {0x19, 0x3F}, // ?
            {0x1A, 0x26}, // &
            {0x1B, 0x00}, // FIGS (control character - no ASCII representation)
            {0x1C, 0x2E}, // .
            {0x1D, 0x2F}, // /
            {0x1E, 0x3B}, // ;
            {0x1F, 0x00}  // LTRS (control character - no ASCII representation)
        };
        private static Dictionary<byte, byte> ccitLettersReverse = new Dictionary<byte, byte>
        {
            {0x00, 0x00}, // Null (blank tape)
            {0x45, 0x01}, // E
            {0x0A, 0x02}, // LR (Line Feed)
            {0x41, 0x03}, // A
            {0x20, 0x04}, // Space
            {0x53, 0x05}, // S
            {0x49, 0x06}, // I 
            {0x55, 0x07}, // U
            {0x0D, 0x08}, // CR (Carriage Return)
            {0x44, 0x09}, // D
            {0x52, 0x0A}, // R
            {0x4A, 0x0B}, // J
            {0x4E, 0x0C}, // N
            {0x46, 0x0D}, // F
            {0x43, 0x0E}, // C
            {0x4B, 0x0F}, // K
            {0x54, 0x10}, // T
            {0x5A, 0x11}, // Z
            {0x4C, 0x12}, // L
            {0x57, 0x13}, // W
            {0x48, 0x14}, // H
            {0x59, 0x15}, // Y
            {0x50, 0x16}, // P
            {0x51, 0x17}, // Q
            {0x4F, 0x18}, // O
            {0x42, 0x19}, // B
            {0x47, 0x1A}, // G
            {0xFE, 0x1B}, // FIGS (control character - no ASCII representation)
            {0x4D, 0x1C}, // M
            {0x58, 0x1D}, // X
            {0x56, 0x1E}, // V
            {0xFF, 0x1F}  // LTRS (control character - no ASCII representation)
        };
        private static Dictionary<byte, byte> ccitFiguresReverse = new Dictionary<byte, byte>
        {
                        {0x00, 0x00}, // Null (blank tape)
            {0x33, 0x01}, // 3
            {0x0A, 0x02}, // LR (Line Feed)
            {0x2D, 0x03}, // -
            {0x20, 0x04}, // Space
            {0x27, 0x05}, // '
            {0x38, 0x06}, // 8 
            {0x37, 0x07}, // 7
            {0x0D, 0x08}, // CR (Carriage Return)
            {0x05, 0x09}, // ENC (Enquiry, Who are you?, WRU)
            {0x34, 0x0A}, // 4 
            {0x07, 0x0B}, // BEL (Bell, ring at other end)
            {0x2C, 0x0c}, // ,
            {0x21, 0x0D}, // !
            {0x3A, 0x0E}, // :
            {0x28, 0x0F}, // (
            {0x35, 0x10}, // 5
            {0x2B, 0x11}, // +
            {0x29, 0x12}, // )
            {0x32, 0x13}, // 2
            {0x24, 0x14}, // $
            {0x36, 0x15}, // 6
            {0x30, 0x16}, // 0
            {0x31, 0x17}, // 1
            {0x39, 0x18}, // 9
            {0x3F, 0x19}, // ?
            {0x26, 0x1A}, // &
            {0xFE, 0x1B}, // FIGS (control character - no ASCII representation)
            {0x2E, 0x1C}, // .
            {0x2F, 0x1D}, // /
            {0x3B, 0x1E}, // ;
            {0xFF, 0x1F}  // LTRS (control character - no ASCII representation)
        };

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputDataCaption", "InputDataTooltip")]
        public byte[] InputData
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption1", "InputKeyTooltip1")]
        public byte[] InputKey1
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption2", "InputKeyTooltip2")]
        public byte[] InputKey2
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip")]
        public byte[] OutputData
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return settings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        public void PostExecution()
        {
            Dispose();
        }

        public void Stop()
        {
            stopPressed = true;
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
            InputData = null;
            InputKey1 = null;
            InputKey2 = null;
            OutputData = null;
            stopPressed = false;
        }

        public void Execute()
        {
            // progress to 0
            ProgressChanged(0, 1);

            // Init all algorithm classes and set the chosen long term key
            controlUnit = new ControlUnit(this, settings.Selector);
            synchronizationUnit = new SynchronizationUnit(this, settings.Selector);
            complexUnit = new ComplexUnit(synchronizationUnit, controlUnit);
            complexUnit.SetLongTermKey(settings.Key);
            encryptionUnit = new EncryptionUnit(complexUnit);

            if (!checkKeys())
                return;

            if (InputData == null)
            {
                
                GuiLogMessage(T_310.Properties.Resources.ErrorInputNull, NotificationLevel.Error);
                return;
            }

            if (InputData.Length == 0)
            {
                GuiLogMessage(T_310.Properties.Resources.ErrorInputEmpty, NotificationLevel.Error);
                return;
            }

            if (settings.Mode == ModeEnum.Encrypt)
                PrepareAndEncrypt(InputData);
            else
                PrepareAndDecrypt(InputData);

            ProgressChanged(1, 1);
        }
        #endregion

        #region T310 Functions

        /// <summary>
        /// Converts the data into the correct form and encrypts it
        /// </summary>
        /// <param name="inputData">plaintext bytes</param>
        private void PrepareAndEncrypt(byte[] inputData)
        {
            List<byte> output = new List<byte>();

            byte[] tmpSyncSequence = synchronizationUnit.InitSync();
            ResetMachine();

            // Write the header (synchronization sequence) to the output
            foreach (byte element in tmpSyncSequence)
                output.Add(element);

            // The complex unit takes initial rounds before usable bits are generated.
            // The reason for this is unknown.
            complexUnit.initialRounds();

            //Check if we execute version 50 (telex) or 51 (data)
            if (settings.Version == VersionEnum.Version50)
            {
                // Everything with 2 or more bytes (Unicode beyond ASCII) is bad for us, so we strip it away beforehand
                inputData = RemoveUnicode(inputData);
                byte[] invalidCharacters;
                byte[] convertedChars = AsciiToCcitt2(inputData, out invalidCharacters);

                // if necessary, communicate truncated characters to the user
                if (invalidCharacters != null && invalidCharacters.Length > 0)
                    handleInvalidCharacters(invalidCharacters);

                // Check if a message is left after converting 
                if (convertedChars.Length <= 0)
                {
                    GuiLogMessage(T_310.Properties.Resources.ErrorEmptyConversion, NotificationLevel.Error);
                    return;
                }

                /*
                 * Encryption happens here (T310/50)
                 */
                for (int i = 0; i < convertedChars.Length && !stopPressed; ++i)
                {
                    output.Add(encryptionUnit.EncryptCharacter(convertedChars[i]));
                    ProgressChanged(i, convertedChars.Length - 1);
                }
            }
            else
            {
                byte[] buffer = new byte[5];
                byte[] encodedBuffer = null;

                int paddingLength = inputData.Length % 5;

                for (int i = 0; i < inputData.Length - paddingLength && !stopPressed; i += 5)
                {
                    //Extend the 5 byte to 8 bytes with a max value of 31 (5 bit)
                    Array.Copy(inputData, i, buffer, 0, buffer.Length);
                    encodedBuffer = map5to8(buffer);

                    /*
                    * Encryption happens here (T310/51)
                    */
                    for (int j = 0; j < 8; j++)
                        output.Add(encryptionUnit.EncryptCharacter(encodedBuffer[j]));

                    ProgressChanged(i, inputData.Length - 1);
                }

                //last bytes are treated extra if the are not a simple 5 byte block
                if (paddingLength != 0)
                {
                    for (long i = paddingLength; i < 5; ++i)
                        buffer[i] = 0x20; //Space
                    Array.Copy(inputData, inputData.Length - paddingLength, buffer, 0, paddingLength);
                    encodedBuffer = map5to8(buffer);
                    for (int j = 0; j < 8; j++)
                        output.Add(encryptionUnit.EncryptCharacter(encodedBuffer[j]));
                }
            }

            OutputData = output.ToArray();
            OnPropertyChanged("OutputData");
        }


        /// <summary>
        /// Prepares the ciphertext, decrypts it and converts it back to normal data
        /// </summary>
        /// <param name="inputData">a T310 ciphertext</param>
        private void PrepareAndDecrypt(byte[] inputData)
        {
            List<byte> output = new List<byte>();
            // parse the header and check it
            if (inputData.Length < syncSequenceLength)
            {
                GuiLogMessage(String.Format(T_310.Properties.Resources.ErrorHeaderLength, syncSequenceLength, inputData.Length), NotificationLevel.Error);
                return;
            }

            byte[] tmpSyncSequence = new byte[syncSequenceLength];

            for (int i = 0; i < tmpSyncSequence.Length; ++i)
            {
                tmpSyncSequence[i] = inputData[i];
            }

            if (!synchronizationUnit.ProcessSync(tmpSyncSequence))
                return;
            ResetMachine();

            // The complex unit takes initial rounds before usable bits are generated.
            // The reason for this is unknown.
            complexUnit.initialRounds();

            if (settings.Version == VersionEnum.Version50)
            {
                byte[] conversionBuffer;
                List<byte> outputBuffer = new List<byte>();
                /*
                 * Decryption happens here (T310/50)
                 */
                for (uint i = syncSequenceLength; i < inputData.Length && !stopPressed; ++i)
                {
                    outputBuffer.Add(encryptionUnit.DecryptCharacter(inputData[i]));
                    ProgressChanged(i, inputData.Length - 1);
                }

                conversionBuffer = Ccitt2ToAscii(outputBuffer.ToArray());
                output.AddRange(conversionBuffer);
            }
            else
            {
                byte[] buffer = new byte[8];
                long paddingLength = (inputData.Length - syncSequenceLength) % 8;
                paddingLength += paddingLength < 0 ? 8 : 0;

                /*
                 * Decryption happens here (T310/51)
                 */
                for (uint i = syncSequenceLength; i < inputData.Length - paddingLength && !stopPressed; i += 8)
                {
                    for (int j = 0; j < 8; j++)
                        buffer[j] = encryptionUnit.DecryptCharacter(inputData[i + j]);

                    // Shrink the 8 bytes (5 bit words) back to 5 (normal 8 bit bytes) and append them to the output
                    output.AddRange(map8to5(buffer));

                    ProgressChanged(i, inputData.Length - 1);
                }

                // If for some reason no complete 8 byte "block" was given, we pad with 0s
               if (paddingLength != 0)
                {
                    Array.Copy(inputData, InputData.Length - paddingLength, buffer, 0, paddingLength);

                    for (int j = 0; j < paddingLength; j++)
                        buffer[j] = encryptionUnit.DecryptCharacter(buffer[j]);

                    for (long i = paddingLength; i < 8; ++i)
                        buffer[i] = 0x00; //Space

                    output.AddRange(map8to5(buffer));
                }
            }
            OutputData = output.ToArray();
            OnPropertyChanged("OutputData");


        }

        /// <summary>
        /// Map 8 5-bit characters to 5 bytes
        /// </summary>
        /// <param name="b">a byte[] of length 8 containing 5 bit words</param>
        /// <returns>a byte[] containing the contracted bytes</returns>
        private byte[] map8to5(byte[] b)
        {
            byte[] res = new byte[5];

            res[0] = (byte)((b[0] << 3) | (b[1] >> 2));
            res[1] = (byte)((b[1] << 6) | (b[2] << 1) | (b[3] >> 4));
            res[2] = (byte)((b[3] << 4) | (b[4] >> 1));
            res[3] = (byte)((b[4] << 7) | (b[6] >> 3) | (b[5] << 2));
            res[4] = (byte)((b[6] << 5) | b[7]);

            return res;
        }

        /// <summary>
        /// Map 5 bytes to 8 5-bit characters
        /// </summary>
        /// <param name="b">a byte[] with length 5</param>
        /// <param name="i"></param>
        /// <returns></returns>
        private byte[] map5to8(byte[] b)
        {
            byte[] res = new byte[8];

            res[0] = (byte)((b[0] >> 3) & 0x1f);
            res[1] = (byte)((0x07 & b[0]) << 2);
            res[1] |= (byte)(b[1] >> 6);
            res[2] = (byte)((0x3e & b[1]) >> 1);
            res[3] = (byte)((0x01 & b[1]) << 4);
            res[3] |= (byte)((0xf0 & b[2]) >> 4);
            res[4] = (byte)((0x0f & b[2]) << 1);
            res[4] |= (byte)(b[3] >> 7);
            res[5] = (byte)((0x7c & b[3]) >> 2);
            res[6] = (byte)((0x03 & b[3]) << 3);
            res[6] |= (byte)((0xe0 & b[4]) >> 5);
            res[7] = (byte)(b[4] & 0x1f);

            return res;
        }

        /// <summary>
        /// Maps an array of ASCII characters to available 5 bit CCITT-2 characters
        /// </summary>
        /// CCITT-2 is a character encoding developed for telegraphy technology. It was used by the T310/50
        /// and therefore needs to be mapped accordingly. It knows only upper case letters (LTRS) and some special
        /// figures and numbers (FIGS). To switch between eachother, special control characters are included.
        /// 
        /// See <cref="T310:Ccitt2ToAscii"/> for the conversion in the opposite direction 
        /// 
        /// <param name="message">A 7-bit ASCII character, which has a respective representation in CCITT-2</param>
        /// <param name="invalidCharacter">Includes a list of all non-convertable characters. May return null if none occured</param>
        /// <returns>The byte value of the character in CCITT-2 encoding</returns>
        /// 
        private byte[] AsciiToCcitt2(byte[] message, out byte[] invalidCharacters)
        {
            bool figureShift = false;
            List<byte> encodedBytes = new List<byte>();
            List<byte> invalidBytes = new List<byte>();
            for (int i = 0; i < message.Length; ++i)
            {
                byte character = message[i];
                byte ccittChar = 0x00;

                // convert to upper case if needed
                if (character >= 0x61 && character <= 0x7A)
                    character -= 0x20;

                /*
                * Note on these lookups: We use containsKey and an assignment instead of TryGetValue()
                * because elsewise we can't check if the key really exists and is 0x0 or is a default 0x0
                */

                // Check if we can find it in the letter table
                if (ccitLettersReverse.ContainsKey(character))
                {
                    ccittChar = ccitLettersReverse[character];

                    //check if we are in figure mode, if we are we have to switch back to letters
                    if (figureShift)
                    {
                        encodedBytes.Add(ltrs);
                        figureShift = false;
                        encodedBytes.Add(ccittChar);
                    }
                    else
                        encodedBytes.Add(ccittChar);
                }
                // Check if we can find it in the figures table
                else if (ccitFiguresReverse.ContainsKey(character))
                {
                    ccittChar = ccitFiguresReverse[character];

                    //check if we are NOT in figure mode, if we are we have to switch to figure mode
                    if (!figureShift)
                    {
                        encodedBytes.Add(figs);
                        figureShift = true;
                        encodedBytes.Add(ccittChar);
                    }
                    else
                        encodedBytes.Add(ccittChar);
                }
                // If we couldn't find anything, we add it to the list of invalid characters
                else
                {
                    // We only want a single instance of invalid characters in the list
                    if (!invalidBytes.Contains(character))
                        invalidBytes.Add(character);
                }

            }

            // Pack the converted characters and the invalid ones into arrays
            if (invalidBytes.Count > 0)
                invalidCharacters = invalidBytes.ToArray();
            else
                invalidCharacters = null;
            return encodedBytes.ToArray();
        }

        /// <summary>
        /// Maps an array of CCITT-2 characters to ASCII characters
        /// </summary>
        /// <param name="message">a string as bytearray which will be converted</param>
        /// <returns>an ASCII encoded byte[]</returns>
        private byte[] Ccitt2ToAscii(byte[] message)
        {
            bool figureShift = false;
            byte tmpByte;
            List<byte> encodedBytes = new List<byte>();
            foreach (byte character in message)
            {
                if (character == figs)
                {
                    figureShift = true;
                    continue;
                }
                if (character == ltrs)
                {
                    figureShift = false;
                    continue;
                }

                /*
                * Note on these lookups: We don't check for non-existent characters here,
                * because we don't expect them to be coming out of the machine.
                */
                if (figureShift)
                    ccitFigures.TryGetValue(character, out tmpByte);
                else
                    ccitLetters.TryGetValue(character, out tmpByte);

                encodedBytes.Add(tmpByte);

            }

            return encodedBytes.ToArray();
        }

        /// <summary>
        /// Purge a byte array of all non ASCII characters
        /// </summary>
        /// <param name="stringAsBytes">A byte array which gets interpreted as UTF-8 string</param>
        /// <returns>A byte array string with only ASCII characters</returns>
        private byte[] RemoveUnicode(byte[] stringAsBytes)
        {
            string asAscii = Encoding.ASCII.GetString(
                Encoding.Convert(
                    Encoding.UTF8,
                    Encoding.GetEncoding(
                        Encoding.ASCII.EncodingName,
                        new EncoderReplacementFallback(string.Empty),
                        new DecoderExceptionFallback()
                    ),
                stringAsBytes
               )
            );
            return Encoding.ASCII.GetBytes(asAscii);
        }

        /// <summary>
        /// Communicates truncated characters in the plaintext to the user
        /// </summary>
        /// <param name="invalidCharacters">A byte array containing a single instance of characters</param>
        private void handleInvalidCharacters(byte[] invalidCharacters)
        {
            String truncatedMessage = invalidCharacters.Length == 1 ?
                           String.Format(T_310.Properties.Resources.ErrorUnconvertableBeginningSingular, invalidCharacters.Length) :
                           String.Format(T_310.Properties.Resources.ErrorUnconvertableBeginningPlural, invalidCharacters.Length);

            //we will only print the non-convertable characters if there are less than 10
            if (invalidCharacters.Length <= 10)
            {
                truncatedMessage += ": ";
                for (int i = 0; i < invalidCharacters.Length; ++i)
                    truncatedMessage += "'" + Encoding.ASCII.GetString(invalidCharacters, i, 1) + "', ";
                // Truncate the ", " from the last character
                truncatedMessage = truncatedMessage.Remove(truncatedMessage.Length - 2);
            }
            truncatedMessage += invalidCharacters.Length == 1 ?
                T_310.Properties.Resources.ErrorUnconvertableEndSingular :
                T_310.Properties.Resources.ErrorUnconvertableEndPlural;

            GuiLogMessage(truncatedMessage, NotificationLevel.Warning);
        }


        /// <summary>
        /// Resets the T310 components
        /// </summary>
        public void ResetMachine()
        {
            if (complexUnit != null)
                complexUnit.ResetUnit();
            if (controlUnit != null)
                controlUnit.ResetKeys();
        }

        /// <summary>
        /// Check T310 keys from the connectors
        /// </summary>
        /// Includes a check for null, correct length and parity.
        /// 
        /// <returns>true on valid keys, false otherwise</returns>
        public bool checkKeys()
        {
            int validKeys = 0;
            byte[] keyBinary1, keyBinary2;
            /*
             * Check if connector's keys are null (not connected)
             */
            if (InputKey1 == null && InputKey2 != null)
            {
                GuiLogMessage(String.Format(T_310.Properties.Resources.ErrorKeyNull, 1), NotificationLevel.Error);
                return false;
            } else if (InputKey2 == null && InputKey1 != null)
            {
                GuiLogMessage(String.Format(T_310.Properties.Resources.ErrorKeyNull, 2), NotificationLevel.Error);
                return false;
            }

            if (InputKey1 == null && InputKey2 == null)
            {
                GuiLogMessage(T_310.Properties.Resources.ErrorBothKeysNull, NotificationLevel.Error);
                return false;
            }

            /*
             * Check if key length equals 15 (120 bit)
             */
            if (InputKey1.Length != ControlUnit.keyLength && InputKey2.Length == ControlUnit.keyLength)
            {
                GuiLogMessage(String.Format(T_310.Properties.Resources.ErrorKeyLength, 1, InputKey1.Length), NotificationLevel.Error);
                return false;
            }
            else if (InputKey2.Length != ControlUnit.keyLength && InputKey1.Length == ControlUnit.keyLength)
            {
                GuiLogMessage(String.Format(T_310.Properties.Resources.ErrorKeyLength, 2, InputKey2.Length), NotificationLevel.Error);
                return false;
            }

            if (InputKey1.Length != ControlUnit.keyLength && InputKey2.Length != ControlUnit.keyLength)
            {
                GuiLogMessage(T_310.Properties.Resources.ErrorBothKeysLength, NotificationLevel.Error);
                return false;
            }

            /*
             *  Check if the keys are odd. The machine requires odd keys.
             */
            keyBinary1 = InputKey1;
            keyBinary2 = InputKey2;
            if (!controlUnit.KeyFromBytes(keyBinary1, KeyIndex.S1))   
                validKeys |= 1;           
            if (!controlUnit.KeyFromBytes(keyBinary2, KeyIndex.S2))
               validKeys |= 2;
            

            if (validKeys == 1)
                GuiLogMessage(String.Format(T_310.Properties.Resources.ErrorKeyEvenParity, 1), NotificationLevel.Error);
            else if (validKeys == 2)
                GuiLogMessage(String.Format(T_310.Properties.Resources.ErrorKeyEvenParity, 2), NotificationLevel.Error);
            else if (validKeys == 3)
                GuiLogMessage(T_310.Properties.Resources.ErrorBothKeysEvenParity, NotificationLevel.Error);

            return validKeys == 0 ? true : false;
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

        #endregion
    }

}
