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
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace CrypTool.Plugins.A5_attack
{
    [Author("Kristina Hita", "khita@mail.uni-mannheim.de", "Universität Mannheim", "https://www.uni-mannheim.de/1/english/university/profile/")]
    [PluginInfo("A5_attack.Properties.Resources", "A5 Attack", "Tries to guess the key and decrypt the ciphertext", "A5_attack/userdoc.xml", new[] { "A5_attack/Images/gsm.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class A5_attack : ICrypComponent
    {
        private byte[] pt;//plaintext frames
        private byte[] ct;//cipher text frames
        private byte[] iv;//iv
        private byte[] tk;//trial key
        private string cs;//case
        private int framesCount;
        private readonly A5AttackSettings settings = new A5AttackSettings();

        public ISettings Settings => settings;

        //Input for plain text
        [PropertyInfo(Direction.InputData, "Plain text", "Must have the same length as Ciphertext", true)]
        public byte[] PlainText
        {
            get => pt;
            set
            {
                pt = value;
                OnPropertyChanged("PlainText");
            }
        }

        //input for number of frames in the ciphertext
        //1 frame is part of ciphertext which was encrypted with own IV
        [PropertyInfo(Direction.InputData, "Frames count", "Number of frames in the plain\\cipher text bytes", true)]
        public int FramesCount
        {
            get => framesCount;
            set
            {
                framesCount = value;
                OnPropertyChanged("FramesCount");
            }
        }
        //input for ciphertext
        [PropertyInfo(Direction.InputData, "Cipher text", "Must have the same length as Plain text", true)]
        public byte[] CipherText
        {
            get => ct;
            set
            {
                ct = value;
                OnPropertyChanged("CipherText");
            }
        }
        //input for initial vector
        //starting vector
        // each frame of the plaintext/ciphertext corresponds to one IV
        //IVs for the upcoming frames would be incremented with 1
        [PropertyInfo(Direction.InputData, "Initial vector", "22 bits (or 3 bytes)", true)]
        public byte[] InitialVector
        {
            get => iv;
            set
            {
                iv = value;
                OnPropertyChanged("InitialVector");
            }
        }

        //the otput for guessed (weak) key
        [PropertyInfo(Direction.OutputData, "Guessed key", "Found weak key", true)]
        public byte[] TrialKey
        {
            get => tk;
            set
            {
                tk = value;
                OnPropertyChanged("TrialKey");
            }
        }

        //the output with the corresponding case of guessed key
        [PropertyInfo(Direction.OutputData, "Case", "Found weak key case", true)]
        public string Case
        {
            get => cs;
            set
            {
                cs = value;
                OnPropertyChanged("Case");
            }
        }

        //size of initial vector (in bits)
        private const int IV_SIZE = 22;
        private const int NUM_OF_CASES = 3;

        //two dimensional array for plaintext
        //first dimension means frame number, second consists of frame bits 
        private int[][] plaintext;

        //same for ciphertext
        private int[][] ciphertext;



        //arrays of initial vectors for each frame
        private int[][] IV;

        //here would be hold the key which would be tested
        public int[] trialKey;

        //number of frames
        private int numberOfFrames;
        //method to initialize the algorithm
        //it divides the plaintext and ciphertext into frames and writes them into two dimensional arrays
        //it also generates IVs for each frame by incrementing the input IV with 1
        public void InitValues(byte[] PlainText, byte[] CipherText, byte[] iv, int frameCount)
        {
            //initialize arrays
            plaintext = new int[frameCount][];
            ciphertext = new int[frameCount][];
            IV = new int[frameCount][];
            //gets the size of single frames (in bits)
            int frameSize = PlainText.Length / frameCount;
            //temporary array to hold currect frame in the cycle
            byte[] temporaryText = new byte[frameSize];
            //variable temporaryIV holds current IV in the cycle
            int[] temporaryIV = ConvertFromByteArr(iv, IV_SIZE);
            numberOfFrames = frameCount;
            for (int i = 0; i < frameCount; i++)

            {
                //Copying one frame to temporaryText
                Array.Copy(PlainText, i * frameSize, temporaryText, 0, frameSize);
                //Converting byte array temporaryText to bit representation
                plaintext[i] = ConvertFromByteArr(temporaryText);
                //doing the same thing with ciphertext
                Array.Copy(CipherText, i * frameSize, temporaryText, 0, frameSize);
                ciphertext[i] = ConvertFromByteArr(temporaryText);
                //initializing the array which would hold IV for current frame
                IV[i] = new int[IV_SIZE];
                //copying the temp iv to the array
                Array.Copy(temporaryIV, IV[i], IV_SIZE);
                //incrementing by 1 the tempIV
                Increment(temporaryIV, 0);
            }

        }
        //method which converts byte array to bit representation
        private int[] ConvertFromByteArr(byte[] arr, int size = -1)
        {
            //if size was not defined as the argument
            if (size == -1)
            {
                //use default size (each byte = 8 bits)
                size = arr.Length * 8;
            }
            //initializing resulting array
            int[] res = new int[size];
            //cycle goes through all bytes in the array
            for (int i = 0; i < arr.Length; i++)
            {
                //cycle goes through all bits in current byte
                for (int j = 7; j >= 0; j--)
                {
                    //shifts the number to get current bit value 
                    res[res.Length - size] = (arr[i] >> j) & 1;
                    size--;
                    //if size was defined and we get all bits
                    if (size < 1)
                    {
                        //stop the cycle and end the method
                        return res;
                    }
                }
            }
            return res;
        }
        //function for checking two bit arrays, if they are identical
        private bool CheckIdent(int[] plain, int[] cipher)
        {
            for (int i = 0; i < plain.Length; i++)
            {
                if (plain[i] != cipher[i])
                {
                    return false;
                }
            }
            return true;
        }
        //function for checking 2 bit arrays, if they are complement
        private bool CheckComplem(int[] plain, int[] cipher)
        {
            for (int i = 0; i < plain.Length; i++)
            {
                if ((plain[i] ^ cipher[i]) != 1)
                {
                    return false;
                }
            }
            return true;
        }
        //method checks current trial key
        //it decrypts ciphertext with trial key
        //if plain text and decrypted text are identical
        //it returns true, otherwise returns false
        private bool CheckKey()
        {
            A5 test;
            int[] trialPlainText;
            for (int i = 1; i < numberOfFrames; i++)
            {
                test = new A5(trialKey, IV[i]);
                trialPlainText = test.Encrypt(ciphertext[i]);
                if (!CheckIdent(plaintext[i], trialPlainText))
                {
                    return false;
                }
            }
            return true;
        }
        // Case when all LFSR are zeros, only the IV determines the secret key
        public int[] Scenario0(int[] v)
        {
            int[] x = new int[64];

            x[0] = (v[0] + v[1] + v[5] + v[6] + v[8] + v[11] + v[18]) % 2;
            x[1] = (v[1] + v[2] + v[6] + v[7] + v[9] + v[12] + v[19]) % 2;
            x[2] = (v[2] + v[3] + v[7] + v[8] + v[10] + v[13] + v[20]) % 2;
            x[3] = (v[3] + v[4] + v[8] + v[9] + v[11] + v[14] + v[21]) % 2;
            x[4] = (v[4] + v[5] + v[9] + v[10] + v[12] + v[15]) % 2;
            x[5] = (v[5] + v[6] + v[10] + v[11] + v[13] + v[16]) % 2;
            x[6] = (v[6] + v[7] + v[11] + v[12] + v[14] + v[17]) % 2;
            x[7] = (v[7] + v[8] + v[12] + v[13] + v[15] + v[18]) % 2;

            x[8] = (v[0] + v[1] + v[5] + v[6] + v[9] + v[11] + v[13] + v[14] + v[16] + v[18] + v[19]) % 2;
            x[9] = (v[1] + v[2] + v[6] + v[7] + v[10] + v[12] + v[14] + v[15] + v[17] + v[19] + v[20]) % 2;
            x[10] = (v[2] + v[3] + v[7] + v[8] + v[11] + v[13] + v[15] + v[16] + v[18] + v[20] + v[21]) % 2;
            x[11] = (v[3] + v[4] + v[8] + v[9] + v[12] + v[14] + v[16] + v[17] + v[19] + v[21]) % 2;
            x[12] = (v[4] + v[5] + v[9] + v[10] + v[13] + v[15] + v[17] + v[18] + v[20]) % 2;
            x[13] = (v[5] + v[6] + v[10] + v[11] + v[14] + v[16] + v[18] + v[19] + v[21]) % 2;

            x[14] = (v[0] + v[1] + v[5] + v[7] + v[8] + v[12] + v[15] + v[17] + v[18] + v[19] + v[20]) % 2;
            x[15] = (v[1] + v[2] + v[6] + v[8] + v[9] + v[13] + v[16] + v[18] + v[19] + v[20] + v[21]) % 2;
            x[16] = (v[2] + v[3] + v[7] + v[9] + v[10] + v[14] + v[17] + v[19] + v[20] + v[21]) % 2;

            x[17] = (v[0] + v[1] + v[3] + v[4] + v[5] + v[6] + v[10] + v[15] + v[20] + v[21]) % 2;
            x[18] = (v[0] + v[2] + v[4] + v[7] + v[8] + v[16] + v[18] + v[21]) % 2;

            x[19] = (v[0] + v[3] + v[6] + v[9] + v[11] + v[17] + v[18] + v[19]) % 2;
            x[20] = (v[1] + v[4] + v[7] + v[10] + v[12] + v[18] + v[19] + v[20]) % 2;
            x[21] = (v[2] + v[5] + v[8] + v[11] + v[13] + v[19] + v[20] + v[21]) % 2;

            x[22] = (v[0] + v[1] + v[3] + v[5] + v[8] + v[9] + v[11] + v[12] + v[14] + v[18] + v[20] + v[21]) % 2;

            x[23] = (v[0] + v[2] + v[4] + v[5] + v[8] + v[9] + v[10] + v[11] + v[12] + v[13] + v[15] + v[18] + v[19] + v[21]) % 2;
            x[24] = (v[1] + v[3] + v[5] + v[6] + v[9] + v[10] + v[11] + v[12] + v[13] + v[14] + v[16] + v[17] + v[20]) % 2;

            x[25] = (v[0] + v[1] + v[2] + v[4] + v[5] + v[7] + v[8] + v[10] + v[12] + v[13] + v[14] + v[15] + v[17] + v[18] + v[20] + v[21]) % 2;
            x[26] = (v[0] + v[2] + v[3] + v[9] + v[13] + v[14] + v[15] + v[16] + v[19] + v[21]) % 2;

            x[27] = (v[0] + v[3] + v[4] + v[5] + v[6] + v[8] + v[10] + v[11] + v[14] + v[15] + v[16] + v[17] + v[18] + v[20]) % 2;
            x[28] = (v[1] + v[4] + v[5] + v[6] + v[7] + v[9] + v[11] + v[12] + v[15] + v[16] + v[17] + v[18] + v[19] + v[21]) % 2;

            x[29] = (v[0] + v[1] + v[2] + v[7] + v[10] + v[11] + v[12] + v[13] + v[16] + v[17] + v[19] + v[20]) % 2;

            x[30] = (v[0] + v[2] + v[3] + v[5] + v[6] + v[12] + v[13] + v[14] + v[17] + v[20] + v[21]) % 2;
            x[31] = (v[1] + v[3] + v[4] + v[6] + v[7] + v[13] + v[14] + v[15] + v[18] + v[21]) % 2;
            x[32] = (v[2] + v[4] + v[5] + v[7] + v[8] + v[14] + v[15] + v[16] + v[19]) % 2;
            x[33] = (v[3] + v[5] + v[6] + v[8] + v[9] + v[15] + v[16] + v[17] + v[20]) % 2;
            x[34] = (v[4] + v[6] + v[7] + v[9] + v[10] + v[16] + v[17] + v[18] + v[21]) % 2;
            x[35] = (v[5] + v[7] + v[8] + v[10] + v[11] + v[17] + v[18] + v[19]) % 2;
            x[36] = (v[6] + v[8] + v[9] + v[11] + v[12] + v[18] + v[19] + v[20]) % 2;

            x[37] = (v[0] + v[1] + v[5] + v[6] + v[7] + v[8] + v[9] + v[10] + v[11] + v[12] + v[13] + v[18] + v[19] + v[20] + v[21]) % 2;
            x[38] = (v[1] + v[2] + v[6] + v[7] + v[8] + v[9] + v[10] + v[11] + v[12] + v[13] + v[14] + v[19] + v[20] + v[21]) % 2;
            x[39] = (v[2] + v[3] + v[7] + v[8] + v[9] + v[10] + v[11] + v[12] + v[13] + v[14] + v[15] + v[20] + v[21]) % 2;

            x[40] = (v[0] + v[1] + v[3] + v[4] + v[5] + v[6] + v[9] + v[10] + v[12] + v[13] + v[14] + v[15] + v[16] + v[18] + v[21]) % 2;
            x[41] = (v[0] + v[2] + v[4] + v[7] + v[8] + v[10] + v[13] + v[14] + v[15] + v[16] + v[17] + v[18] + v[19]) % 2;
            x[42] = (v[1] + v[3] + v[5] + v[8] + v[9] + v[11] + v[14] + v[15] + v[16] + v[17] + v[18] + v[19] + v[20]) % 2;

            x[43] = (v[0] + v[1] + v[2] + v[4] + v[5] + v[8] + v[9] + v[10] + v[11] + v[12] + v[15] + v[16] + v[17] + v[19] + v[20] + v[21]) % 2;
            x[44] = (v[0] + v[2] + v[3] + v[8] + v[9] + v[10] + v[12] + v[13] + v[16] + v[17] + v[20] + v[21]) % 2;
            x[45] = (v[0] + v[3] + v[4] + v[5] + v[6] + v[8] + v[9] + v[10] + v[13] + v[14] + v[17] + v[21]) % 2;

            x[46] = (v[0] + v[4] + v[7] + v[9] + v[10] + v[14] + v[15]) % 2;
            x[47] = (v[1] + v[5] + v[8] + v[10] + v[11] + v[15] + v[16]) % 2;
            x[48] = (v[2] + v[6] + v[9] + v[11] + v[12] + v[16] + v[17]) % 2;

            x[49] = (v[0] + v[1] + v[3] + v[5] + v[6] + v[7] + v[8] + v[10] + v[12] + v[13] + v[17]) % 2;
            x[50] = (v[1] + v[2] + v[4] + v[6] + v[7] + v[8] + v[9] + v[11] + v[13] + v[14] + v[18]) % 2;
            x[51] = (v[2] + v[3] + v[5] + v[7] + v[8] + v[9] + v[10] + v[12] + v[14] + v[15] + v[19]) % 2;
            x[52] = (v[3] + v[4] + v[6] + v[8] + v[9] + v[10] + v[11] + v[13] + v[15] + v[16] + v[20]) % 2;
            x[53] = (v[4] + v[5] + v[7] + v[9] + v[10] + v[11] + v[12] + v[14] + v[16] + v[17] + v[21]) % 2;
            x[54] = (v[5] + v[6] + v[8] + v[10] + v[11] + v[12] + v[13] + v[15] + v[17] + v[18]) % 2;
            x[55] = (v[6] + v[7] + v[9] + v[11] + v[12] + v[13] + v[14] + v[16] + v[18] + v[19]) % 2;

            x[56] = (v[0] + v[1] + v[5] + v[6] + v[7] + v[10] + v[11] + v[12] + v[13] + v[14] + v[15] + v[17] + v[18] + v[19] + v[20]) % 2;
            x[57] = (v[1] + v[2] + v[6] + v[7] + v[8] + v[11] + v[12] + v[13] + v[14] + v[15] + v[16] + v[18] + v[19] + v[20] + v[21]) % 2;
            x[58] = (v[2] + v[3] + v[7] + v[8] + v[9] + v[12] + v[13] + v[14] + v[15] + v[16] + v[17] + v[19] + v[20] + v[21]) % 2;
            x[59] = (v[3] + v[4] + v[8] + v[9] + v[10] + v[13] + v[14] + v[15] + v[16] + v[17] + v[18] + v[20] + v[21]) % 2;

            x[60] = (v[0] + v[1] + v[4] + v[6] + v[8] + v[9] + v[10] + v[14] + v[15] + v[16] + v[17] + v[19] + v[21]) % 2;
            x[61] = (v[0] + v[2] + v[6] + v[7] + v[8] + v[9] + v[10] + v[15] + v[16] + v[17] + v[20]) % 2;
            x[62] = (v[0] + v[3] + v[5] + v[6] + v[7] + v[9] + v[10] + v[16] + v[17] + v[21]) % 2;
            x[63] = (v[0] + v[4] + v[5] + v[7] + v[10] + v[17]) % 2;

            return x;
        }
        // Case 1, Where Register A & B are all zeros and Register C contains non-zero values => k=23,
        // register in non zero values together with the IV determine the secret key
        // the weak keys are ending on 111001 (6 bits), so we keep that part constant,
        // by reducing the number of guessed bits with 6, in order to make the plugin run faster when brute forcing

        public int[] Scenario1(int[] k, int[] v)
        {
            // for case 2a, guessed bits belong to register C which is in non-zero, so there are in total 23(guessed bits of C)-6(constant)=17 guessed bits of C
            // 17 bits are guessed, so these determine the other 41 bits of the key
            // {1, 1, 1, 0, 0, 1} are the 6 constant (guessed) values for all the weak keys
            // 6 bits are kept constant so 58 bits of the key are actually computed (64-6=58)
            int[] x = new int[64];
            Array.Copy(k, 0, x, 41, 17);
            Array.Copy(new int[] { 1, 1, 1, 0, 0, 1 }, 0, x, 58, 6);

            x[0] = (x[41] + x[44] + x[46] + x[50] + x[51] + x[52] + x[53] + x[54] + x[56] + x[57] + x[59] + x[60] + x[62] + v[0] + v[1] + v[2] + v[3] + v[4] + v[8] + v[9] + v[10] + v[11] + v[14] + v[15] + v[18] + v[19] + v[20]) % 2;
            x[1] = (x[42] + x[45] + x[47] + x[51] + x[52] + x[53] + x[54] + x[55] + x[57] + x[58] + x[60] + x[61] + x[63] + v[1] + v[2] + v[3] + v[4] + v[5] + v[9] + v[10] + v[11] + v[12] + v[15] + v[16] + v[19] + v[20] + v[21]) % 2;
            x[2] = (x[43] + x[46] + x[48] + x[52] + x[53] + x[54] + x[55] + x[56] + x[58] + x[59] + x[61] + x[62] + v[0] + v[2] + v[3] + v[4] + v[5] + v[6] + v[10] + v[11] + v[12] + v[13] + v[16] + v[17] + v[20] + v[21]) % 2;
            x[3] = (x[44] + x[47] + x[49] + x[53] + x[54] + x[55] + x[56] + x[57] + x[59] + x[60] + x[62] + x[63] + v[1] + v[3] + v[4] + v[5] + v[6] + v[7] + v[11] + v[12] + v[13] + v[14] + v[17] + v[18] + v[21]) % 2;
            x[4] = (x[45] + x[48] + x[50] + x[54] + x[55] + x[56] + x[57] + x[58] + x[60] + x[61] + x[63] + v[0] + v[2] + v[4] + v[5] + v[6] + v[7] + v[8] + v[12] + v[13] + v[14] + v[15] + v[18] + v[19]) % 2;
            x[5] = (x[46] + x[49] + x[51] + x[55] + x[56] + x[57] + x[58] + x[59] + x[61] + x[62] + v[0] + v[1] + v[3] + v[5] + v[6] + v[7] + v[8] + v[9] + v[13] + v[14] + v[15] + v[16] + v[19] + v[20]) % 2;
            x[6] = (x[47] + x[50] + x[52] + x[56] + x[57] + x[58] + x[59] + x[60] + x[62] + x[63] + v[1] + v[2] + v[4] + v[6] + v[7] + v[8] + v[9] + v[10] + v[14] + v[15] + v[16] + v[17] + v[20] + v[21]) % 2;
            x[7] = (x[48] + x[51] + x[53] + x[57] + x[58] + x[59] + x[60] + x[61] + x[63] + v[0] + v[2] + v[3] + v[5] + v[7] + v[8] + v[9] + v[10] + v[11] + v[15] + v[16] + v[17] + v[18] + v[21]) % 2;
            x[8] = (x[49] + x[52] + x[54] + x[58] + x[59] + x[60] + x[61] + x[62] + v[0] + v[1] + v[3] + v[4] + v[6] + v[8] + v[9] + v[10] + v[11] + v[12] + v[16] + v[17] + v[18] + v[19]) % 2;
            x[9] = (x[50] + x[53] + x[55] + x[59] + x[60] + x[61] + x[62] + x[63] + v[1] + v[2] + v[4] + v[5] + v[7] + v[9] + v[10] + v[11] + v[12] + v[13] + v[17] + v[18] + v[19] + v[20]) % 2;
            x[10] = (x[51] + x[54] + x[56] + x[60] + x[61] + x[62] + x[63] + v[0] + v[2] + v[3] + v[5] + v[6] + v[8] + v[10] + v[11] + v[12] + v[13] + v[14] + v[18] + v[19] + v[20] + v[21]) % 2;
            x[11] = (x[52] + x[55] + x[57] + x[61] + x[62] + x[63] + v[0] + v[1] + v[3] + v[4] + v[6] + v[7] + v[9] + v[11] + v[12] + v[13] + v[14] + v[15] + v[19] + v[20] + v[21]) % 2;
            x[12] = (x[53] + x[56] + x[58] + x[62] + x[63] + v[0] + v[1] + v[2] + v[4] + v[5] + v[7] + v[8] + v[10] + v[12] + v[13] + v[14] + v[15] + v[16] + v[20] + v[21]) % 2;
            x[13] = (x[54] + x[57] + x[59] + x[63] + v[0] + v[1] + v[2] + v[3] + v[5] + v[6] + v[8] + v[9] + v[11] + v[13] + v[14] + v[15] + v[16] + v[17] + v[21]) % 2;

            x[14] = (x[41] + x[44] + x[46] + x[50] + x[51] + x[52] + x[53] + x[54] + x[55] + x[56] + x[57] + x[58] + x[59] + x[62] + v[6] + v[7] + v[8] + v[11] + v[12] + v[16] + v[17] + v[19] + v[20]) % 2;
            x[15] = (x[42] + x[45] + x[47] + x[51] + x[52] + x[53] + x[54] + x[55] + x[56] + x[57] + x[58] + x[59] + x[60] + x[63] + v[7] + v[8] + v[9] + v[12] + v[13] + v[17] + v[18] + v[20] + v[21]) % 2;
            x[16] = (x[43] + x[46] + x[48] + x[52] + x[53] + x[54] + x[55] + x[56] + x[57] + x[58] + x[59] + x[60] + x[61] + v[0] + v[8] + v[9] + v[10] + v[13] + v[14] + v[18] + v[19] + v[21]) % 2;

            x[17] = (x[41] + x[46] + x[47] + x[49] + x[50] + x[51] + x[52] + x[55] + x[58] + x[61] + v[0] + v[2] + v[3] + v[4] + v[8] + v[18]) % 2;
            x[18] = (x[41] + x[42] + x[44] + x[46] + x[47] + x[48] + x[54] + x[57] + x[60] + v[0] + v[2] + v[5] + v[8] + v[10] + v[11] + v[14] + v[15] + v[18] + v[20]) % 2;

            x[19] = (x[41] + x[42] + x[43] + x[44] + x[45] + x[46] + x[47] + x[48] + x[49] + x[50] + x[51] + x[52] + x[53] + x[54] + x[55] + x[56] + x[57] + x[58] + x[59] + x[60] + x[61] + x[62] + v[0] + v[2] + v[4] + v[6] + v[8] + v[10] + v[12] + v[14] + v[16] + v[18] + v[20] + v[21]) % 2;
            x[20] = (x[42] + x[43] + x[44] + x[45] + x[46] + x[47] + x[48] + x[49] + x[50] + x[51] + x[52] + x[53] + x[54] + x[55] + x[56] + x[57] + x[58] + x[59] + x[60] + x[61] + x[62] + x[63] + v[1] + v[3] + v[5] + v[7] + v[9] + v[11] + v[13] + v[15] + v[17] + v[19] + v[21]) % 2;

            x[21] = (x[41] + x[43] + x[45] + x[47] + x[48] + x[49] + x[55] + x[58] + x[61] + x[63] + v[1] + v[3] + v[6] + v[9] + v[11] + v[12] + v[15] + v[16] + v[19]) % 2;

            x[22] = (x[41] + x[42] + x[48] + x[49] + x[51] + x[52] + x[53] + x[54] + x[57] + x[60] + v[1] + v[3] + v[7] + v[8] + v[9] + v[11] + v[12] + v[13] + v[14] + v[15] + v[16] + v[17] + v[18] + v[19]) % 2;
            x[23] = (x[42] + x[43] + x[49] + x[50] + x[52] + x[53] + x[54] + x[55] + x[58] + x[61] + v[2] + v[4] + v[8] + v[9] + v[10] + v[12] + v[13] + v[14] + v[15] + v[16] + v[17] + v[18] + v[19] + v[20]) % 2;
            x[24] = (x[43] + x[44] + x[50] + x[51] + x[53] + x[54] + x[55] + x[56] + x[59] + x[62] + v[3] + v[5] + v[9] + v[10] + v[11] + v[13] + v[14] + v[15] + v[16] + v[17] + v[18] + v[19] + v[20] + v[21]) % 2;
            x[25] = (x[44] + x[45] + x[51] + x[52] + x[54] + x[55] + x[56] + x[57] + x[60] + x[63] + v[4] + v[6] + v[10] + v[11] + v[12] + v[14] + v[15] + v[16] + v[17] + v[18] + v[19] + v[20] + v[21]) % 2;
            x[26] = (x[45] + x[46] + x[52] + x[53] + x[55] + x[56] + x[57] + x[58] + x[61] + v[0] + v[5] + v[7] + v[11] + v[12] + v[13] + v[15] + v[16] + v[17] + v[18] + v[19] + v[20] + v[21]) % 2;
            x[27] = (x[46] + x[47] + x[53] + x[54] + x[56] + x[57] + x[58] + x[59] + x[62] + v[1] + v[6] + v[8] + v[12] + v[13] + v[14] + v[16] + v[17] + v[18] + v[19] + v[20] + v[21]) % 2;
            x[28] = (x[47] + x[48] + x[54] + x[55] + x[57] + x[58] + x[59] + x[60] + x[63] + v[2] + v[7] + v[9] + v[13] + v[14] + v[15] + v[17] + v[18] + v[19] + v[20] + v[21]) % 2;
            x[29] = (x[48] + x[49] + x[55] + x[56] + x[58] + x[59] + x[60] + x[61] + v[0] + v[3] + v[8] + v[10] + v[14] + v[15] + v[16] + v[18] + v[19] + v[20] + v[21]) % 2;
            x[30] = (x[49] + x[50] + x[56] + x[57] + x[59] + x[60] + x[61] + x[62] + v[1] + v[4] + v[9] + v[11] + v[15] + v[16] + v[17] + v[19] + v[20] + v[21]) % 2;
            x[31] = (x[50] + x[51] + x[57] + x[58] + x[60] + x[61] + x[62] + x[63] + v[2] + v[5] + v[10] + v[12] + v[16] + v[17] + v[18] + v[20] + v[21]) % 2;
            x[32] = (x[51] + x[52] + x[58] + x[59] + x[61] + x[62] + x[63] + v[0] + v[3] + v[6] + v[11] + v[13] + v[17] + v[18] + v[19] + v[21]) % 2;
            x[33] = (x[52] + x[53] + x[59] + x[60] + x[62] + x[63] + v[0] + v[1] + v[4] + v[7] + v[12] + v[14] + v[18] + v[19] + v[20]) % 2;
            x[34] = (x[53] + x[54] + x[60] + x[61] + x[63] + v[0] + v[1] + v[2] + v[5] + v[8] + v[13] + v[15] + v[19] + v[20] + v[21]) % 2;

            x[35] = (x[41] + x[44] + x[46] + x[50] + x[51] + x[52] + x[53] + x[55] + x[56] + x[57] + x[59] + x[60] + x[61] + v[4] + v[6] + v[8] + v[10] + v[11] + v[15] + v[16] + v[18] + v[19] + v[21]) % 2;

            x[36] = (x[41] + x[42] + x[44] + x[45] + x[46] + x[47] + x[50] + x[58] + x[59] + x[61] + v[0] + v[1] + v[2] + v[3] + v[4] + v[5] + v[7] + v[8] + v[10] + v[12] + v[14] + v[15] + v[16] + v[17] + v[18]) % 2;
            x[37] = (x[42] + x[43] + x[45] + x[46] + x[47] + x[48] + x[51] + x[59] + x[60] + x[62] + v[1] + v[2] + v[3] + v[4] + v[5] + v[6] + v[8] + v[9] + v[11] + v[13] + v[15] + v[16] + v[17] + v[18] + v[19]) % 2;

            x[38] = (x[41] + x[43] + x[47] + x[48] + x[49] + x[50] + x[51] + x[53] + x[54] + x[56] + x[57] + x[59] + x[61] + x[62] + x[63] + v[0] + v[1] + v[5] + v[6] + v[7] + v[8] + v[11] + v[12] + v[15] + v[16] + v[17]) % 2;
            x[39] = (x[42] + x[44] + x[48] + x[49] + x[50] + x[51] + x[52] + x[54] + x[55] + x[57] + x[58] + x[60] + x[62] + x[63] + v[0] + v[1] + v[2] + v[6] + v[7] + v[8] + v[9] + v[12] + v[13] + v[16] + v[17] + v[18]) % 2;
            x[40] = (x[43] + x[45] + x[49] + x[50] + x[51] + x[52] + x[53] + x[55] + x[56] + x[58] + x[59] + x[61] + x[63] + v[0] + v[1] + v[2] + v[3] + v[7] + v[8] + v[9] + v[10] + v[13] + v[14] + v[17] + v[18] + v[19]) % 2;

            return x;
        }
        // Case 2b, Where Register A & C are all zeros and Register B contains non-zero values => k=22
        // register in non zero values together with the IV determine the secret key
        // for case 2b, guessed bits belong to register B which is in non-zero, so there are in total 22(guessed bits of B)-6(constant)=16 guessed
        // 16 bits are guessed, so these determine the other 42 bits of the key
        // {1, 1, 1, 0, 0, 1} are the 6 constant (guessed) values for all the weak keys so 58 bits of the key are actually computed
        public int[] Scenario2(int[] k, int[] v)
        {
            int[] x = new int[64];
            Array.Copy(k, 0, x, 42, 16);
            Array.Copy(new int[] { 1, 1, 1, 0, 0, 1 }, 0, x, 58, 6);

            x[0] = (x[42] + x[44] + x[47] + x[49] + x[50] + x[51] + x[53] + x[54] + x[60] + x[61] + v[0] + v[4] + v[5] + v[7] + v[12] + v[14] + v[17] + v[19] + v[20] + v[21]) % 2;
            x[1] = (x[43] + x[45] + x[48] + x[50] + x[51] + x[52] + x[54] + x[55] + x[61] + x[62] + v[1] + v[5] + v[6] + v[8] + v[13] + v[15] + v[18] + v[20] + v[21]) % 2;
            x[2] = (x[44] + x[46] + x[49] + x[51] + x[52] + x[53] + x[55] + x[56] + x[62] + x[63] + v[2] + v[6] + v[7] + v[9] + v[14] + v[16] + v[19] + v[21]) % 2;
            x[3] = (x[45] + x[47] + x[50] + x[52] + x[53] + x[54] + x[56] + x[57] + x[63] + v[0] + v[3] + v[7] + v[8] + v[10] + v[15] + v[17] + v[20]) % 2;
            x[4] = (x[46] + x[48] + x[51] + x[53] + x[54] + x[55] + x[57] + x[58] + v[0] + v[1] + v[4] + v[8] + v[9] + v[11] + v[16] + v[18] + v[21]) % 2;
            x[5] = (x[47] + x[49] + x[52] + x[54] + x[55] + x[56] + x[58] + x[59] + v[1] + v[2] + v[5] + v[9] + v[10] + v[12] + v[17] + v[19]) % 2;
            x[6] = (x[48] + x[50] + x[53] + x[55] + x[56] + x[57] + x[59] + x[60] + v[2] + v[3] + v[6] + v[10] + v[11] + v[13] + v[18] + v[20]) % 2;
            x[7] = (x[49] + x[51] + x[54] + x[56] + x[57] + x[58] + x[60] + x[61] + v[3] + v[4] + v[7] + v[11] + v[12] + v[14] + v[19] + v[21]) % 2;

            x[8] = (x[42] + x[44] + x[47] + x[49] + x[51] + x[52] + x[53] + x[54] + x[55] + x[57] + x[58] + x[59] + x[60] + x[62] + v[0] + v[7] + v[8] + v[13] + v[14] + v[15] + v[17] + v[19] + v[21]) % 2;
            x[9] = (x[43] + x[45] + x[48] + x[50] + x[52] + x[53] + x[54] + x[55] + x[56] + x[58] + x[59] + x[60] + x[61] + x[63] + v[1] + v[8] + v[9] + v[14] + v[15] + v[16] + v[18] + v[20]) % 2;
            x[10] = (x[44] + x[46] + x[49] + x[51] + x[53] + x[54] + x[55] + x[56] + x[57] + x[59] + x[60] + x[61] + x[62] + v[0] + v[2] + v[9] + v[10] + v[15] + v[16] + v[17] + v[19] + v[21]) % 2;
            x[11] = (x[45] + x[47] + x[50] + x[52] + x[54] + x[55] + x[56] + x[57] + x[58] + x[60] + x[61] + x[62] + x[63] + v[1] + v[3] + v[10] + v[11] + v[16] + v[17] + v[18] + v[20]) % 2;
            x[12] = (x[46] + x[48] + x[51] + x[53] + x[55] + x[56] + x[57] + x[58] + x[59] + x[61] + x[62] + x[63] + v[0] + v[2] + v[4] + v[11] + v[12] + v[17] + v[18] + v[19] + v[21]) % 2;
            x[13] = (x[47] + x[49] + x[52] + x[54] + x[56] + x[57] + x[58] + x[59] + x[60] + x[62] + x[63] + v[0] + v[1] + v[3] + v[5] + v[12] + v[13] + v[18] + v[19] + v[20]) % 2;

            x[14] = (x[42] + x[44] + x[47] + x[48] + x[49] + x[51] + x[54] + x[55] + x[57] + x[58] + x[59] + x[63] + v[1] + v[2] + v[5] + v[6] + v[7] + v[12] + v[13] + v[17]) % 2;
            x[15] = (x[43] + x[45] + x[48] + x[49] + x[50] + x[52] + x[55] + x[56] + x[58] + x[59] + x[60] + v[0] + v[2] + v[3] + v[6] + v[7] + v[8] + v[13] + v[14] + v[18]) % 2;
            x[16] = (x[44] + x[46] + x[49] + x[50] + x[51] + x[53] + x[56] + x[57] + x[59] + x[60] + x[61] + v[1] + v[3] + v[4] + v[7] + v[8] + v[9] + v[14] + v[15] + v[19]) % 2;

            x[17] = (x[42] + x[44] + x[45] + x[49] + x[52] + x[53] + x[57] + x[58] + x[62] + v[0] + v[2] + v[7] + v[8] + v[9] + v[10] + v[12] + v[14] + v[15] + v[16] + v[17] + v[19] + v[21]) % 2;

            x[18] = (x[42] + x[43] + x[44] + x[45] + x[46] + x[47] + x[49] + x[51] + x[58] + x[59] + x[60] + x[61] + x[63] + v[0] + v[1] + v[3] + v[4] + v[5] + v[7] + v[8] + v[9] + v[10] + v[11] + v[12] + v[13] + v[14] + v[15] + v[16] + v[18] + v[19] + v[21]) % 2;

            x[19] = (x[42] + x[43] + x[45] + x[46] + x[48] + x[49] + x[51] + x[52] + x[53] + x[54] + x[59] + x[62] + v[1] + v[2] + v[6] + v[7] + v[8] + v[9] + v[10] + v[11] + v[13] + v[15] + v[16] + v[21]) % 2;
            x[20] = (x[43] + x[44] + x[46] + x[47] + x[49] + x[50] + x[52] + x[53] + x[54] + x[55] + x[60] + x[63] + v[2] + v[3] + v[7] + v[8] + v[9] + v[10] + v[11] + v[12] + v[14] + v[16] + v[17]) % 2;

            x[21] = (x[42] + x[45] + x[48] + x[49] + x[55] + x[56] + x[60] + v[3] + v[5] + v[7] + v[8] + v[9] + v[10] + v[11] + v[13] + v[14] + v[15] + v[18] + v[19] + v[20] + v[21]) % 2;
            x[22] = (x[43] + x[46] + x[49] + x[50] + x[56] + x[57] + x[61] + v[4] + v[6] + v[8] + v[9] + v[10] + v[11] + v[12] + v[14] + v[15] + v[16] + v[19] + v[20] + v[21]) % 2;

            x[23] = (x[42] + x[49] + x[53] + x[54] + x[57] + x[58] + x[60] + x[61] + x[62] + v[0] + v[4] + v[9] + v[10] + v[11] + v[13] + v[14] + v[15] + v[16] + v[19]) % 2;
            x[24] = (x[43] + x[50] + x[54] + x[55] + x[58] + x[59] + x[61] + x[62] + x[63] + v[1] + v[5] + v[10] + v[11] + v[12] + v[14] + v[15] + v[16] + v[17] + v[20]) % 2;

            x[25] = (x[42] + x[47] + x[49] + x[50] + x[53] + x[54] + x[55] + x[56] + x[59] + x[61] + x[62] + x[63] + v[2] + v[4] + v[5] + v[6] + v[7] + v[11] + v[13] + v[14] + v[15] + v[16] + v[18] + v[19] + v[20]) % 2;
            x[26] = (x[42] + x[43] + x[44] + x[47] + x[48] + x[49] + x[53] + x[55] + x[56] + x[57] + x[61] + x[62] + x[63] + v[3] + v[4] + v[6] + v[8] + v[15] + v[16]) % 2;

            x[27] = (x[42] + x[43] + x[45] + x[47] + x[48] + x[51] + x[53] + x[56] + x[57] + x[58] + x[60] + x[61] + x[62] + x[63] + v[9] + v[12] + v[14] + v[16] + v[19] + v[20] + v[21]) % 2;
            x[28] = (x[43] + x[44] + x[46] + x[48] + x[49] + x[52] + x[54] + x[57] + x[58] + x[59] + x[61] + x[62] + x[63] + v[0] + v[10] + v[13] + v[15] + v[17] + v[20] + v[21]) % 2;
            x[29] = (x[44] + x[45] + x[47] + x[49] + x[50] + x[53] + x[55] + x[58] + x[59] + x[60] + x[62] + x[63] + v[0] + v[1] + v[11] + v[14] + v[16] + v[18] + v[21]) % 2;
            x[30] = (x[45] + x[46] + x[48] + x[50] + x[51] + x[54] + x[56] + x[59] + x[60] + x[61] + x[63] + v[0] + v[1] + v[2] + v[12] + v[15] + v[17] + v[19]) % 2;
            x[31] = (x[46] + x[47] + x[49] + x[51] + x[52] + x[55] + x[57] + x[60] + x[61] + x[62] + v[0] + v[1] + v[2] + v[3] + v[13] + v[16] + v[18] + v[20]) % 2;
            x[32] = (x[47] + x[48] + x[50] + x[52] + x[53] + x[56] + x[58] + x[61] + x[62] + x[63] + v[1] + v[2] + v[3] + v[4] + v[14] + v[17] + v[19] + v[21]) % 2;
            x[33] = (x[48] + x[49] + x[51] + x[53] + x[54] + x[57] + x[59] + x[62] + x[63] + v[0] + v[2] + v[3] + v[4] + v[5] + v[15] + v[18] + v[20]) % 2;
            x[34] = (x[49] + x[50] + x[52] + x[54] + x[55] + x[58] + x[60] + x[63] + v[0] + v[1] + v[3] + v[4] + v[5] + v[6] + v[16] + v[19] + v[21]) % 2;

            x[35] = (x[42] + x[44] + x[47] + x[49] + x[54] + x[55] + x[56] + x[59] + x[60] + v[1] + v[2] + v[6] + v[12] + v[14] + v[19] + v[21]) % 2;
            x[36] = (x[42] + x[43] + x[44] + x[45] + x[47] + x[48] + x[49] + x[51] + x[53] + x[54] + x[55] + x[56] + x[57] + v[0] + v[2] + v[3] + v[4] + v[5] + v[12] + v[13] + v[14] + v[15] + v[17] + v[19] + v[21]) % 2;
            x[37] = (x[42] + x[43] + x[45] + x[46] + x[47] + x[48] + x[51] + x[52] + x[53] + x[55] + x[56] + x[57] + x[58] + x[60] + x[61] + v[0] + v[1] + v[3] + v[6] + v[7] + v[12] + v[13] + v[15] + v[16] + v[17] + v[18] + v[19] + v[21]) % 2;

            x[38] = (x[42] + x[43] + x[46] + x[48] + x[50] + x[51] + x[52] + x[56] + x[57] + x[58] + x[59] + x[60] + x[62] + v[0] + v[1] + v[2] + v[5] + v[8] + v[12] + v[13] + v[16] + v[18] + v[21]) % 2;
            x[39] = (x[43] + x[44] + x[47] + x[49] + x[51] + x[52] + x[53] + x[57] + x[58] + x[59] + x[60] + x[61] + x[63] + v[1] + v[2] + v[3] + v[6] + v[9] + v[13] + v[14] + v[17] + v[19]) % 2;

            x[40] = (x[42] + x[45] + x[47] + x[48] + x[49] + x[51] + x[52] + x[58] + x[59] + x[62] + v[2] + v[3] + v[5] + v[10] + v[12] + v[15] + v[17] + v[18] + v[19] + v[21]) % 2;
            x[41] = (x[43] + x[46] + x[48] + x[49] + x[50] + x[52] + x[53] + x[59] + x[60] + x[63] + v[3] + v[4] + v[6] + v[11] + v[13] + v[16] + v[18] + v[19] + v[20]) % 2;

            return x;
        }
        // Case 2c, Where Register B & C are all zeros and Register A contains non-zero values k=19
        // register A and IV determine the secret key values
        // for case 2c, guessed bits belong to register A which is in non-zero, so there are in total 19(guessed bits of A)-6(constant)=13 guessed
        // 13 bits are guessed, so these determine the other 45 bits of the key
        // {1, 1, 1, 0, 0, 1} are the 6 constant (guessed) values for all the weak keys so 58 bits of the key are actually computed
        public int[] Scenario3(int[] k, int[] v)
        {

            int[] x = new int[64];
            Array.Copy(k, 0, x, 45, 13);
            Array.Copy(new int[] { 1, 1, 1, 0, 0, 1 }, 0, x, 58, 6);

            x[0] = (x[45] + x[48] + x[51] + x[54] + x[57] + x[61] + x[63] + v[5] + v[13] + v[14] + v[16] + v[17] + v[18]) % 2;
            x[1] = (x[46] + x[49] + x[52] + x[55] + x[58] + x[62] + v[0] + v[6] + v[14] + v[15] + v[17] + v[18] + v[19]) % 2;
            x[2] = (x[47] + x[50] + x[53] + x[56] + x[59] + x[63] + v[1] + v[7] + v[15] + v[16] + v[18] + v[19] + v[20]) % 2;
            x[3] = (x[48] + x[51] + x[54] + x[57] + x[60] + v[0] + v[2] + v[8] + v[16] + v[17] + v[19] + v[20] + v[21]) % 2;
            x[4] = (x[49] + x[52] + x[55] + x[58] + x[61] + v[1] + v[3] + v[9] + v[17] + v[18] + v[20] + v[21]) % 2;
            x[5] = (x[50] + x[53] + x[56] + x[59] + x[62] + v[2] + v[4] + v[10] + v[18] + v[19] + v[21]) % 2;
            x[6] = (x[51] + x[54] + x[57] + x[60] + x[63] + v[3] + v[5] + v[11] + v[19] + v[20]) % 2;
            x[7] = (x[52] + x[55] + x[58] + x[61] + v[0] + v[4] + v[6] + v[12] + v[20] + v[21]) % 2;

            x[8] = (x[45] + x[48] + x[51] + x[53] + x[54] + x[56] + x[57] + x[59] + x[61] + x[62] + x[63] + v[1] + v[7] + v[14] + v[16] + v[17] + v[18] + v[21]) % 2;
            x[9] = (x[46] + x[49] + x[52] + x[54] + x[55] + x[57] + x[58] + x[60] + x[62] + x[63] + v[0] + v[2] + v[8] + v[15] + v[17] + v[18] + v[19]) % 2;
            x[10] = (x[47] + x[50] + x[53] + x[55] + x[56] + x[58] + x[59] + x[61] + x[63] + v[0] + v[1] + v[3] + v[9] + v[16] + v[18] + v[19] + v[20]) % 2;
            x[11] = (x[48] + x[51] + x[54] + x[56] + x[57] + x[59] + x[60] + x[62] + v[0] + v[1] + v[2] + v[4] + v[10] + v[17] + v[19] + v[20] + v[21]) % 2;
            x[12] = (x[49] + x[52] + x[55] + x[57] + x[58] + x[60] + x[61] + x[63] + v[1] + v[2] + v[3] + v[5] + v[11] + v[18] + v[20] + v[21]) % 2;
            x[13] = (x[50] + x[53] + x[56] + x[58] + x[59] + x[61] + x[62] + v[0] + v[2] + v[3] + v[4] + v[6] + v[12] + v[19] + v[21]) % 2;
            x[14] = (x[51] + x[54] + x[57] + x[59] + x[60] + x[62] + x[63] + v[1] + v[3] + v[4] + v[5] + v[7] + v[13] + v[20]) % 2;
            x[15] = (x[52] + x[55] + x[58] + x[60] + x[61] + x[63] + v[0] + v[2] + v[4] + v[5] + v[6] + v[8] + v[14] + v[21]) % 2;
            x[16] = (x[53] + x[56] + x[59] + x[61] + x[62] + v[0] + v[1] + v[3] + v[5] + v[6] + v[7] + v[9] + v[15]) % 2;
            x[17] = (x[54] + x[57] + x[60] + x[62] + x[63] + v[1] + v[2] + v[4] + v[6] + v[7] + v[8] + v[10] + v[16]) % 2;
            x[18] = (x[55] + x[58] + x[61] + x[63] + v[0] + v[2] + v[3] + v[5] + v[7] + v[8] + v[9] + v[11] + v[17]) % 2;
            x[19] = (x[56] + x[59] + x[62] + v[0] + v[1] + v[3] + v[4] + v[6] + v[8] + v[9] + v[10] + v[12] + v[18]) % 2;
            x[20] = (x[57] + x[60] + x[63] + v[1] + v[2] + v[4] + v[5] + v[7] + v[9] + v[10] + v[11] + v[13] + v[19]) % 2;
            x[21] = (x[58] + x[61] + v[0] + v[2] + v[3] + v[5] + v[6] + v[8] + v[10] + v[11] + v[12] + v[14] + v[20]) % 2;
            x[22] = (x[59] + x[62] + v[1] + v[3] + v[4] + v[6] + v[7] + v[9] + v[11] + v[12] + v[13] + v[15] + v[21]) % 2;

            x[23] = (x[45] + x[48] + x[51] + x[54] + x[57] + x[60] + x[61] + v[2] + v[4] + v[7] + v[8] + v[10] + v[12] + v[17] + v[18]) % 2;
            x[24] = (x[46] + x[49] + x[52] + x[55] + x[58] + x[61] + x[62] + v[3] + v[5] + v[8] + v[9] + v[11] + v[13] + v[18] + v[19]) % 2;
            x[25] = (x[47] + x[50] + x[53] + x[56] + x[59] + x[62] + x[63] + v[4] + v[6] + v[9] + v[10] + v[12] + v[14] + v[19] + v[20]) % 2;
            x[26] = (x[48] + x[51] + x[54] + x[57] + x[60] + x[63] + v[0] + v[5] + v[7] + v[10] + v[11] + v[13] + v[15] + v[20] + v[21]) % 2;
            x[27] = (x[49] + x[52] + x[55] + x[58] + x[61] + v[0] + v[1] + v[6] + v[8] + v[11] + v[12] + v[14] + v[16] + v[21]) % 2;
            x[28] = (x[50] + x[53] + x[56] + x[59] + x[62] + v[1] + v[2] + v[7] + v[9] + v[12] + v[13] + v[15] + v[17]) % 2;

            x[29] = (x[45] + x[48] + x[60] + x[61] + v[2] + v[3] + v[5] + v[8] + v[10] + v[17]) % 2;

            x[30] = (x[45] + x[46] + x[48] + x[49] + x[51] + x[54] + x[57] + x[62] + x[63] + v[3] + v[4] + v[5] + v[6] + v[9] + v[11] + v[13] + v[14] + v[16] + v[17]) % 2;
            x[31] = (x[46] + x[47] + x[49] + x[50] + x[52] + x[55] + x[58] + x[63] + v[0] + v[4] + v[5] + v[6] + v[7] + v[10] + v[12] + v[14] + v[15] + v[17] + v[18]) % 2;
            x[32] = (x[47] + x[48] + x[50] + x[51] + x[53] + x[56] + x[59] + v[0] + v[1] + v[5] + v[6] + v[7] + v[8] + v[11] + v[13] + v[15] + v[16] + v[18] + v[19]) % 2;
            x[33] = (x[48] + x[49] + x[51] + x[52] + x[54] + x[57] + x[60] + v[1] + v[2] + v[6] + v[7] + v[8] + v[9] + v[12] + v[14] + v[16] + v[17] + v[19] + v[20]) % 2;
            x[34] = (x[49] + x[50] + x[52] + x[53] + x[55] + x[58] + x[61] + v[2] + v[3] + v[7] + v[8] + v[9] + v[10] + v[13] + v[15] + v[17] + v[18] + v[20] + v[21]) % 2;
            x[35] = (x[50] + x[51] + x[53] + x[54] + x[56] + x[59] + x[62] + v[3] + v[4] + v[8] + v[9] + v[10] + v[11] + v[14] + v[16] + v[18] + v[19] + v[21]) % 2;
            x[36] = (x[51] + x[52] + x[54] + x[55] + x[57] + x[60] + x[63] + v[4] + v[5] + v[9] + v[10] + v[11] + v[12] + v[15] + v[17] + v[19] + v[20]) % 2;
            x[37] = (x[52] + x[53] + x[55] + x[56] + x[58] + x[61] + v[0] + v[5] + v[6] + v[10] + v[11] + v[12] + v[13] + v[16] + v[18] + v[20] + v[21]) % 2;
            x[38] = (x[53] + x[54] + x[56] + x[57] + x[59] + x[62] + v[1] + v[6] + v[7] + v[11] + v[12] + v[13] + v[14] + v[17] + v[19] + v[21]) % 2;
            x[39] = (x[54] + x[55] + x[57] + x[58] + x[60] + x[63] + v[2] + v[7] + v[8] + v[12] + v[13] + v[14] + v[15] + v[18] + v[20]) % 2;
            x[40] = (x[55] + x[56] + x[58] + x[59] + x[61] + v[0] + v[3] + v[8] + v[9] + v[13] + v[14] + v[15] + v[16] + v[19] + v[21]) % 2;
            x[41] = (x[56] + x[57] + x[59] + x[60] + x[62] + v[1] + v[4] + v[9] + v[10] + v[14] + v[15] + v[16] + v[17] + v[20]) % 2;

            x[42] = (x[45] + x[48] + x[51] + x[54] + x[58] + x[60] + v[2] + v[10] + v[11] + v[13] + v[14] + v[15] + v[21]) % 2;
            x[43] = (x[46] + x[49] + x[52] + x[55] + x[59] + x[61] + v[3] + v[11] + v[12] + v[14] + v[15] + v[16]) % 2;
            x[44] = (x[47] + x[50] + x[53] + x[56] + x[60] + x[62] + v[4] + v[12] + v[13] + v[15] + v[16] + v[17]) % 2;

            return x;
        }


        public int Start()
        {// the attack only works for cases when the ciphertext is identical to the plaintext or if they are complements of each other
            if (CheckIdent(plaintext[0], ciphertext[0]) || CheckComplem(plaintext[0], ciphertext[0]))
            {
                int[] trialPlainText;
                trialKey = Scenario0(IV[0]);
                A5 test = new A5(trialKey, IV[0]);
                trialPlainText = test.Encrypt(ciphertext[0]);
                if (CheckIdent(plaintext[0], trialPlainText))
                {
                    if (CheckKey())
                    {
                        return 0;
                    }
                }

                //array for guessbits
                //guessbits are generating using bruteforce
                // the largest corresponds to case 2a (where originally the register in non zero is 23 bits => 17 bits will be used in bruteforce,
                // other 6 are constant
                int[] guessbits = new int[17] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                //cycle tries guessbits values for all the cases
                for (int i = 0; i < (1 << guessbits.Length) - 1; i++)
                {
                    if ((i & 0x3ff) == 0)
                    {
                        ProgressChanged(i, 1 << 17);
                    }

                    //   string s = " ";
                    //     s = s + guessbits;
                    //     for (int j = 0; j < 17; j++)
                    //    {
                    //         s += guessbits[j];
                    //    }

                    //  GuiLogMessage(""+guessbits, NotificationLevel.Info);


                    trialKey = Scenario1(guessbits, IV[0]);
                    test = new A5(trialKey, IV[0]);
                    trialPlainText = test.Encrypt(ciphertext[0]);
                    if (CheckIdent(plaintext[0], trialPlainText))
                    {
                        if (CheckKey())
                        {
                            //{
                            return 4;
                        }
                    }
                    // GuiLogMessage(s, NotificationLevel.Info); }


                    trialKey = Scenario2(guessbits, IV[0]);
                    test = new A5(trialKey, IV[0]);
                    trialPlainText = test.Encrypt(ciphertext[0]);
                    if (CheckIdent(plaintext[0], trialPlainText))
                    {
                        if (CheckKey())
                        {
                            return 2;
                        }
                    }

                    trialKey = Scenario3(guessbits, IV[0]);
                    test = new A5(trialKey, IV[0]);
                    trialPlainText = test.Encrypt(ciphertext[0]);
                    if (CheckIdent(plaintext[0], trialPlainText))
                    {
                        if (CheckKey())
                        {
                            return 3;
                        }
                    }

                    Increment(guessbits, 0);

                }

            }
            return -1;
        }
        //method increments IV with 1
        // We use different IV`s for different frames. 
        // if for the 1st frame iv was 1100....00 for second it ill be 0010.....00 for third 1010.....00 for fourth 0110....00 and so on (the most left bit being MSB)
        private void Increment(int[] b, int curbit)
        {
            //to stop reccursion checks if current bit value isn`t greater then array length
            if (curbit >= b.Length - 1)
            {
                return;
            }
            //if current bit is 1, set it to 0 and increment next (by recursive call of this method)
            if (b[curbit] == 1)
            {
                b[curbit] = 0;
                Increment(b, ++curbit);
            }
            else
            {//otherwise set 0 to 1 and end the recursion
                b[curbit]++;
            }
        }



        // Converts bit representation to byte array
        private byte[] FromInt(int[] arr)
        {
            //initialize result array each byte = 8 bits, so we divide
            byte[] result = new byte[arr.Length / 8];
            int temporary = 0;
            //cycle goes through all bytes
            for (int i = 0; i < result.Length; i++)
            {
                //cycle goes through all bits in current byte
                for (int j = 7; j >= 0; j--)
                {
                    //first we shift the bit to its positon in byte
                    //binary OR operation sets the bit to temporary value
                    temporary |= arr[i * 8 + (7 - j)] << j;
                }
                //sets temporary value to array cell
                result[i] = (byte)temporary;
                temporary = 0;
            }
            return result;
        }

        public void Execute()
        {
            ProgressChanged(0, 1);
            // check validity of input values
            if (InitialVector.Length > 3)
            {
                GuiLogMessage("Initial vector size must be 22 bits. All bits after the 22th will be ignored.", NotificationLevel.Warning);
            }
            if (PlainText.Length == 0)
            {
                GuiLogMessage("Please enter the plaintext.", NotificationLevel.Error);
                return;
            }
            if (PlainText.Length != CipherText.Length)
            {
                GuiLogMessage("The plaintext and the ciphertext must have the same length.", NotificationLevel.Error);
                return;
            }
            if (FramesCount < 2)
            {
                GuiLogMessage("Frame count should be more than 1", NotificationLevel.Error);
                return;
            }
            InitValues(PlainText, CipherText, InitialVector, framesCount);


            //Method returns case number of the found key, otherwise returns -1
            int result = Start();

            if (result == -1)
            {
                GuiLogMessage("The key hasn't been found.", NotificationLevel.Warning);
            }
            else
            {
                switch (result)
                {
                    case 0:
                        Case = "Case 1 -- only IV used to determine the key";
                        break;

                    case 4:
                        Case = "Case 2a -- register C (23 bits) and IV used to determine the key";
                        break;

                    case 2:
                        Case = "Case 2b -- register B (22 bits) and IV used to determine the key";
                        break;

                    case 3:
                        Case = "Case 2c -- register A (19 bits) and IV used to determine the key";
                        break;

                }
                //sets output trial key
                TrialKey = FromInt(trialKey);
            }
            ProgressChanged(1, 1);
        }

        public void Dispose()
        {
        }

        public void Initialize()
        {
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public void PostExecution()
        {
            Dispose();
        }

        public void PreExecution()
        {
            Dispose();
        }

        public UserControl Presentation => null;

        public void Stop()
        {
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
            //if (PropertyChanged != null)
            //{
            //  PropertyChanged(this, new PropertyChangedEventArgs(name));
            //}
        }

        #endregion
    }

    // to construct the cipher with its characteristics
    internal class A5
    {
        private readonly int[] Key;
        private readonly int[] IV;
        private readonly LFSR[] registers;
        public A5(int[] key, int[] iv)
        {
            Key = new int[64];
            IV = new int[22];
            Array.Copy(key, Key, 64);
            Array.Copy(iv, IV, 22);
            registers = new LFSR[3];
            // determine the tapped bits and clocking bits for each register
            registers[0] = new LFSR(19, new int[] { 13, 16, 17, 18 }, 8);
            registers[1] = new LFSR(22, new int[] { 20, 21 }, 10);
            registers[2] = new LFSR(23, new int[] { 7, 20, 21, 22 }, 10);
            InitPhase();
        }


        private void InitPhase()
        {// registers are clocked 64 times using the secret key
            for (int i = 0; i < 64; i++)
            {
                registers[0].Shift(Key[i]);
                registers[1].Shift(Key[i]);
                registers[2].Shift(Key[i]);
            } // registers are clocked 22 times using the IV
            for (int i = 0; i < 22; i++)
            {
                registers[0].Shift(IV[i]);
                registers[1].Shift(IV[i]);
                registers[2].Shift(IV[i]);
            }
            //Console.WriteLine(this);
            // registers are clocked 100 times following the majority rule
            for (int i = 0; i < 100; i++)
            {
                Majority();
            }


            //Console.WriteLine(this);
        }
        // returns registers in majority
        // which means it compares the clocking bits of registers and returns the ones who have the clocking bits in majority
        private int GetMajor()
        {
            if (registers[0].ClockingTap() == registers[1].ClockingTap() || registers[0].ClockingTap() == registers[2].ClockingTap())
            {
                return registers[0].ClockingTap();
            }
            else
            {
                return registers[1].ClockingTap();
            }
        }
        // the registers in majority are being shifted
        private void Majority()
        {
            int major = GetMajor();
            System.Collections.Generic.IEnumerable<LFSR> res = registers.Where(x => x.ClockingTap() == major);
            foreach (LFSR i in res)
            {
                i.Shift();
            }
        } // keystream generation
        private int Output()
        {
            return (registers[0].GetLast() + registers[1].GetLast() + registers[2].GetLast()) % 2;
        }
        private int Cipher(int infoBit)
        { // ciphertext as a result of XORed plaintext with keystream
            Majority();
            int res = (infoBit + Output()) % 2;

            return res;
        }
        public int[] Encrypt(int[] frame)
        { // encrypt each of the frames in ciphertext
            int[] encrypted = new int[frame.Length];
            for (int i = 0; i < frame.Length; i++)
            {
                encrypted[i] = Cipher(frame[i]);
            }
            return encrypted;
        }
        public override string ToString()
        {
            return registers[0].ToString() + "\n" + registers[1].ToString() + "\n" + registers[2].ToString() + "\n";
        }
    }

    internal class LFSR
    { // constructs the LFSRs : size, tapped bits, clocking bits
        private readonly int[] cells;
        private readonly int[] tapBits;
        private readonly int clockingTap;
        public LFSR(int size, int[] tapped, int clocking)
        {
            cells = new int[size];
            tapBits = tapped;
            clockingTap = clocking;
        }
        public int Shift(int input = 0)
        { // tapped bits of each register are being XORed with each other and the feedback bits are being put in the first position of 
            // each corresponding register, by shifting all the other values with 1 position
            int result = cells[cells.Length - 1];
            int next = input;
            foreach (int tap in tapBits)
            {
                next += cells[tap];
            }
            next = next % 2;
            for (int i = cells.Length - 1; i > 0; i--)
            {
                cells[i] = cells[i - 1];
            }
            cells[0] = next;
            return result;
        }
        public int this[int indexer] => cells[indexer];
        public int ClockingTap()
        {
            return this[clockingTap];
        }
        public int GetLast()
        {
            return cells[cells.Length - 1];
        }
        public override string ToString()
        {
            string s = "";
            foreach (int i in cells)
            {
                s += i.ToString();
            }
            return s;
        }
    }
}
