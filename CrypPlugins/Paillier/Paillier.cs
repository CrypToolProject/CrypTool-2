/* HOWTO: Set year, author name and organization.
   Copyright 2011 CrypTool 2 Team

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
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Numerics;
using System.Text;
using System.Windows.Controls;

namespace CrypTool.Plugins.Paillier
{
    [Author("Armin Krauss, Martin Franz", "", "", "")]
    [PluginInfo("Paillier.Properties.Resources",
        "PluginCaption", "PluginTooltip", "Paillier/DetailedDescription/doc.xml",
        "Paillier/Image/PaillierEnc.png", "Paillier/Image/PaillierDec.png", "Paillier/Image/PaillierAdd.png", "Paillier/Image/PaillierMul.png")]
    [ComponentCategory(ComponentCategory.CiphersModernAsymmetric)]
    public class Paillier : ICrypComponent
    {
        #region Private Variables

        private readonly PaillierSettings settings = new PaillierSettings();

        private BigInteger inputn;          // public key
        private BigInteger inputg;          // public key
        private BigInteger inputlambda;     // private key
        private object inputm;              // plaintext
        private BigInteger inputoperand;    // summand or multiplicand
        private BigInteger outputc1;        // encrypted output (as BigInteger)
        private BigInteger _r;              // random value
        private byte[] outputc2;            // encrypted output (as byte[])

        // Encryption/decryption can be sped up by using the chinese remainder theorem.
        // TODO: As the private key lambda is an input variable we would need a way to reconstruct
        // the factors p and q of n from lambda to use the faster CRT method.

        private BigInteger p, q, n, lambda;

        // Variables for CRT
        private readonly int keyBitLength = 1024;
        private BigInteger twoPowKeyBitLength;
        private BigInteger n_square, p_square, q_square;
        private BigInteger n_plus1, p_minus1, q_minus1;
        private BigInteger n_inv, decDiv;
        private BigInteger hp, hq, ep, eq, ep2, eq2;
        private BigInteger mp, mq;

        #endregion

        #region Initialisation

        public Paillier()
        {
            settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
        }

        private void settings_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Execute();
        }

        private void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(this, args);
            }
        }

        #endregion

        #region Algorithm

        /// <summary>
        /// Decryption ( ((c^lambda) % n^2 - 1) div n ) * lambda^(-1) ) % n
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        private BigInteger decrypt(BigInteger c)
        {
            if (c >= n_square)
            {
                GuiLogMessage("Cipher is bigger than N^2 - this will produce a wrong result!", NotificationLevel.Warning);
            }

            BigInteger lambdainv = BigIntegerHelper.ModInverse(InputLambda, n);
            return (((BigInteger.ModPow(c, InputLambda, n_square) - 1) / n) * lambdainv) % n;
        }

        /// <summary>
        /// Encryption ( g^m * r^n = (1 + m*n) * r^n mod n^2 )
        /// Hint: g^m = (n+1)^m = sum(k= 0, m)((m over k) * n^k) = 1+m* n(mod n^2)
        /// </summary>        
        /// <param name="m"></param>
        private BigInteger encrypt(BigInteger m)
        {
            if (m >= n)
            {
                GuiLogMessage("Message is bigger than N - this will produce a wrong result!", NotificationLevel.Warning);
            }

            BigInteger r;
            bool useRandom = true;

            if (useRandom)
            {
                while (true)
                {
                    r = BigIntegerHelper.RandomIntLimit(n) % n;
                    if (BigInteger.GreatestCommonDivisor(r, n) == 1)
                    {
                        break;
                    }

                    GuiLogMessage("GCD <> 1, retrying...", NotificationLevel.Warning);
                }
                this.r = r; // output r with property
                r = BigInteger.ModPow(r, n, n_square);
            }
            else
            {
                r = 1;
            }

            return (((n * m + 1) % n_square) * r) % n_square;
        }

        /// <summary>
        /// Using the homomorphic property of the Paillier cryptosystem:
        /// This function multiplies two ciphertexts c1 = E(m1) and c2 = E(m2)
        /// in order to add the encrypted plaintexts: res = E(m1 + m2)
        /// </summary>                    
        private BigInteger cipherAdd(BigInteger c1, BigInteger c2)
        {
            return (c1 * c2) % n_square;
        }

        /// <summary>
        /// Computing under the hom. encryption: res = E(m1 * exp)
        /// Raises ciphertext E(m1) = c to the power of exp.
        /// </summary>
        private BigInteger cipherMul(BigInteger c, BigInteger exp)
        {
            return BigInteger.ModPow(c, exp, n_square);
        }

        /// <summary>
        /// This function first checks if exp is negative and then computes the result.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="exp"></param>
        /// <returns></returns>
        private BigInteger cipherMulSigned(BigInteger c, BigInteger exp)
        {
            return (exp < 0)
                ? cipherNeg(cipherMul(c, -exp))
                : cipherMul(c, exp);
        }

        /// <summary>
        ///  Compute: res = E(-m)
        /// Computes the multiplicative inverse of some ciphertext c = E(m).
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private BigInteger cipherNeg(BigInteger c)
        {
            return BigIntegerHelper.ModInverse(c, n_square);
        }

        #endregion

        #region Data Properties

        /// <summary>
        /// Gets/Sets public key n
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputNCaption", "InputNTooltip", true)]
        public BigInteger InputN
        {
            get => inputn;
            set => inputn = value;//OnPropertyChanged("InputN");
        }

        /// <summary>
        /// Gets/Sets public key g
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputGCaption", "InputGTooltip", true)]
        public BigInteger InputG
        {
            get => inputg;
            set => inputg = value;//OnPropertyChanged("InputG");
        }

        /// <summary>
        /// Gets/Sets private key lambda
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputLambdaCaption", "InputLambdaTooltip")]
        public BigInteger InputLambda
        {
            get => inputlambda;
            set => inputlambda = value;//OnPropertyChanged("InputLambda");
        }

        /// <summary>
        /// Gets/Sets a input message as BigInteger called M
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputMCaption", "InputMTooltip", true)]
        public object InputM
        {
            get => inputm;
            set
            {
                if (value is BigInteger)
                {
                    inputm = (BigInteger)value;
                }
                else if (value is byte[])
                {
                    inputm = value as byte[];
                }
                else if (value is string)
                {
                    inputm = Encoding.UTF8.GetBytes((string)value);
                }
                else if (value is CStreamWriter)
                {
                    CStreamReader reader = ((ICrypToolStream)value).CreateReader();
                    reader.WaitEof();
                    inputm = new byte[reader.Length];
                    reader.Seek(0, System.IO.SeekOrigin.Begin);
                    reader.ReadFully((byte[])inputm, 0, (int)reader.Length);
                }
                else
                {
                    if (value != null)
                    {
                        GuiLogMessage("Input type " + value.GetType() + " is not allowed", NotificationLevel.Error);
                    }
                    //throw new Exception("Input type " + value.GetType() + " is not allowed");
                    inputm = (BigInteger)0;
                }

                //OnPropertyChanged("InputM");
            }
        }

        /// <summary>
        /// Gets/Sets operand with which to change the encrypted input m
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputOperandCaption", "InputOperandTooltip")]
        public BigInteger InputOperand
        {
            get => inputoperand;
            set => inputoperand = value;//OnPropertyChanged("InputOperand");
        }

        /// <summary>
        /// Gets/Sets the result of the encryption as a BigInteger
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputC1Caption", "OutputC1Tooltip")]
        public BigInteger OutputC1
        {
            get => outputc1;
            set
            {
                if (inputm is BigInteger)
                {
                    outputc1 = value;
                    OnPropertyChanged("OutputC1");
                }
            }
        }

        /// <summary>
        /// Gets/Sets the result of the encryption as byte[]
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputC2Caption", "OutputC2Tooltip")]
        public byte[] OutputC2
        {
            get => outputc2;
            set
            {
                if (inputm is byte[])
                {
                    outputc2 = value;
                    OnPropertyChanged("OutputC2");
                }
            }
        }

        /// <summary>
        /// Gets/Sets the random value of the encryption as a BigInteger
        /// </summary>
        [PropertyInfo(Direction.OutputData, "Output_rCaption", "Output_rTooltip")]
        public BigInteger r
        {
            get => _r;
            set
            {
                if (_r is BigInteger)
                {
                    _r = value;
                    OnPropertyChanged("r");
                }
            }
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings => settings;

        /// <summary>
        /// HOWTO: You can provide a custom (tabbed) presentation to visualize your algorithm.
        /// Return null if you don't provide one.
        /// </summary>
        public UserControl Presentation => null;

        public void PreExecution()
        {

        }

        private byte[] removeZeros(byte[] input)
        {
            int i;
            for (i = input.Length; i > 0 && input[i - 1] == 0; i--)
            {
                ;
            }

            byte[] output = new byte[i];
            Buffer.BlockCopy(input, 0, output, 0, i);

            return output;
        }

        private BigInteger BigIntegerFromBuffer(byte[] buffer, int ofs, int len)
        {
            byte[] tmp = new byte[len + 1];  // extra byte makes sure that BigInteger is positive
            Buffer.BlockCopy(buffer, ofs, tmp, 0, len);
            return new BigInteger(tmp);
        }

        private void BigIntegerIntoBuffer(BigInteger b, byte[] buffer, int ofs, int len)
        {
            byte[] bytes = b.ToByteArray();
            Buffer.BlockCopy(bytes, 0, buffer, ofs, Math.Min(len, bytes.Length));
        }

        ///<summary>
        /// Takes a BigInteger as input, performs some computations on it, and returns another BigInteger.
        ///</summary>
        private delegate BigInteger blockconvertDelegate(BigInteger m);

        ///<summary>
        /// BlockConvert interprets the bytearray 'input' as a sequence of BigIntegers with 'blocksize_input' bytes per BigInteger. 
        /// The funtion 'cFunc' is applied on each of these BigIntegers. The results of 'cFunc' are BigIntegers
        /// that are then transformed back into a bytearray with 'blocksize_output' bytes per BigInteger.
        ///</summary>
        private byte[] BlockConvert(byte[] input, int blocksize_input, int blocksize_output, blockconvertDelegate cFunc)
        {
            if (blocksize_input <= 0)
            {
                throw new Exception("Illegal Input blocksize " + blocksize_input);
            }

            if (blocksize_output <= 0)
            {
                throw new Exception("Output blocksize " + blocksize_output);
            }

            int blockcount = (input.Length + blocksize_input - 1) / blocksize_input;
            byte[] output = new byte[blocksize_output * blockcount];

            for (int ofs_in = 0, ofs_out = 0; ofs_in < input.Length; ofs_in += blocksize_input, ofs_out += blocksize_output)
            {
                BigInteger m = BigIntegerFromBuffer(input, ofs_in, Math.Min(input.Length - ofs_in, blocksize_input));
                m = cFunc(m);
                BigIntegerIntoBuffer(m, output, ofs_out, blocksize_output);
            }

            return output;
        }

        private byte[] BlockConvert(byte[] input, BigInteger n_input, BigInteger n_output, blockconvertDelegate cFunc, bool encrypt)
        {
            int blocksize_input, blocksize_output;

            if (encrypt)
            {
                blocksize_input = (int)Math.Floor(BigInteger.Log(n_input, 256));
                blocksize_output = (int)Math.Ceiling(BigInteger.Log(n_output, 256));
            }
            else
            {
                blocksize_input = (int)Math.Ceiling(BigInteger.Log(n_input, 256));
                blocksize_output = (int)Math.Floor(BigInteger.Log(n_output, 256));
            }

            return BlockConvert(input, blocksize_input, blocksize_output, cFunc);
        }

        public void Execute()
        {
            ProgressChanged(0, 1);

            n = InputN;
            n_square = n * n;

            if (n < 2 * 3)
            {
                GuiLogMessage("Illegal Input N - Paillier cannot work", NotificationLevel.Error);
                return;
            }

            if (settings.Action == 0)   // Encryption
            {
                if (InputM is BigInteger)
                {
                    OutputC1 = encrypt((BigInteger)InputM);
                }
                else if (InputM is byte[])
                {
                    OutputC2 = BlockConvert((byte[])InputM, n, n_square, encrypt, true);
                }
            }
            else if (settings.Action == 1)  // Decryption
            {
                if (InputLambda < 1)
                {
                    GuiLogMessage("Illegal private key Lambda - Paillier cannot decrypt", NotificationLevel.Error);
                    return;
                }

                if (InputM is BigInteger)
                {
                    OutputC1 = decrypt((BigInteger)InputM);
                }
                else if (InputM is byte[])
                {
                    OutputC2 = removeZeros(BlockConvert((byte[])InputM, n_square, n, decrypt, false));
                }
            }
            else if (settings.Action == 2)  // Addition
            {
                if (!(InputM is BigInteger))
                {
                    GuiLogMessage("Message must be a BigInteger for Addition Mode", NotificationLevel.Error);
                    return;
                }

                if (n_square <= (BigInteger)InputM)
                {
                    GuiLogMessage("Message is bigger than N^2 - this will produce a wrong result!", NotificationLevel.Warning);
                }

                if (n_square <= InputOperand)
                {
                    GuiLogMessage("Operand is bigger than N^2 - this will produce a wrong result!", NotificationLevel.Warning);
                }

                OutputC1 = cipherAdd((BigInteger)InputM, InputOperand);
            }
            else if (settings.Action == 3)  // Multiplication
            {
                if (!(InputM is BigInteger))
                {
                    GuiLogMessage("Message must be a BigInteger for Multiplication Mode", NotificationLevel.Error);
                    return;
                }

                if (n_square <= (BigInteger)InputM)
                {
                    GuiLogMessage("Message is bigger than N^2 - this will produce a wrong result!", NotificationLevel.Warning);
                }

                if (n_square <= InputOperand)
                {
                    GuiLogMessage("Operand is bigger than N^2 - this will produce a wrong result!", NotificationLevel.Warning);
                }

                OutputC1 = cipherMul((BigInteger)InputM, InputOperand);
            }

            // Make sure the progress bar is at maximum when your Execute() finished successfully.
            ProgressChanged(1, 1);
        }

        public void PostExecution()
        {

        }

        public void Stop()
        {

        }

        public void Initialize()
        {
            settings.ChangePluginIcon(settings.Action);
        }

        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
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