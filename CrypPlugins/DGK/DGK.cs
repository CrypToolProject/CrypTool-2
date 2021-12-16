/*                              
   Copyright Armin Krauss, Martin Franz

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
using System.Collections;
using System.ComponentModel;
using System.Numerics;
using System.Text;
using System.Windows.Controls;

namespace CrypTool.Plugins.DGK
{
    [Author("Armin Krauss, Martin Franz", "", "", "")]
    [PluginInfo("DGK.Properties.Resources",
        "PluginCaption", "PluginTooltip", "DGK/DetailedDescription/doc.xml",
        "DGK/Image/DGKEnc.png", "DGK/Image/DGKDec.png", "DGK/Image/DGKAdd.png", "DGK/Image/DGKMul.png")]
    [ComponentCategory(ComponentCategory.CiphersModernAsymmetric)]
    public class DGK : ICrypComponent
    {
        #region Private Variables

        private readonly DGKSettings settings = new DGKSettings();

        private BigInteger n;          // public key
        private BigInteger g;          // public key
        private BigInteger h;          // public key
        private BigInteger u;          // public key

        private BigInteger p;          // private key
        private BigInteger q;          // private key
        private BigInteger vp;         // private key
        private BigInteger vq;         // private key

        private object inputm;         // plaintext
        private BigInteger outputc1;   // encrypted output (as BigInteger)
        private byte[] outputc2;       // encrypted output (as byte[])

        private bool secretKeyValid;        // indicates whether the secret key is provided and therefore CRT can be used
        private BigInteger pp_inv, qq_inv;
        private BigInteger vpvq;
        private BigInteger n_square;

        private BigInteger[] decrypttable;
        private Hashtable dechash;
        private bool decrypttableIsValid = false;

        // Variables for CRT

        //const int DGK_MODULUS_BITS = 1024;
        //const int DGK_T = 160;
        private const int DGK_BLINDING_T = 400;

        #endregion

        #region Initialisation

        public DGK()
        {
            //this.settings = new DGKSettings();
            //twoPowKeyBitLength = 1 << (keyBitLength - 1);
            //generateKeys(200,80,40,0);
            //this.settings.PropertyChanged += settings_OnPropertyChanged;
            //this.PropertyChanged += settings_OnPropertyChange;
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

        /*
         DGK encryption using CRT for the owner of the private key (p,q,vp,vq).
         */
        private BigInteger encryptCRT(BigInteger message)
        {
            if (message >= u)
            {
                GuiLogMessage("Message is bigger than U - this will produce a wrong result!", NotificationLevel.Warning);
            }

            // Use Zv instead of a 2t bit number:
            BigInteger r = BigIntegerHelper.RandomIntLimit(vpvq);

            // Calculate in Zp and Zq instead of Zn:
            BigInteger tmp_cp = BigInteger.ModPow(g, message, p) * BigInteger.ModPow(h, r, p);
            BigInteger tmp_cq = BigInteger.ModPow(g, message, q) * BigInteger.ModPow(h, r, q);

            return (tmp_cp * qq_inv + tmp_cq * pp_inv) % n;
        }

        /*
	        Standard DGK encryption.
        */
        private BigInteger encrypt(BigInteger message)
        {
            if (secretKeyValid)
            {
                return encryptCRT(message);
            }

            if (message >= u)
            {
                GuiLogMessage("Message is bigger than U - this will produce a wrong result!", NotificationLevel.Warning);
            }

            //BigInteger r = BigIntegerHelper.RandomIntBits(DGK_BLINDING_T);
            // Choose random number with 2t bits. But t is not part of the public key.
            // Workaround: use n as upper bound. n has k bits and k>t, so n^2 has 2k bits.
            BigInteger r = BigIntegerHelper.RandomIntLimit(n_square);

            return cipherAdd(cipherMul(g, message), cipherMul(h, r));
            //return ( BigInteger.ModPow(g,message,n) * BigInteger.ModPow(h,r,n) ) % n;
        }

        /*
	        Standard deterministic DGK encryption.
        */
        private BigInteger encryptdet(BigInteger message)
        {
            return cipherMul(g, message);
        }

        /*
             DGK Decryption.

             This method can be used for decryption.

             For better performance...
             ... instead of running the loop each time one can use a HashTable for fast lookup
             ... one can use CRT as in isZeroDecryption
         */

        private BigInteger decrypt(BigInteger cipher)
        {
            BigInteger message = BigInteger.ModPow(cipher, vp, p);

            if (decrypttableIsValid)
            {
                //for (int m = 0; m < (int)u; m++)
                //    if (message == decrypttable[m]) return m;
                message = message % (((BigInteger)1) << 48);
                if (dechash.ContainsKey(message))
                {
                    object x = dechash[message];
                    BigInteger res = (int)x;
                    return res;
                }
                //int i = (int)( message % (((BigInteger)1)<<48) );
                //return decrypttable[i];
            }
            else
            {
                BigInteger gv = BigInteger.ModPow(g, vp, p);

                for (int m = 0; m < (int)u; m++)
                {
                    if (message == BigInteger.ModPow(gv, m, p))
                    {
                        return m;
                    }
                }
            }

            // should not be reached
            GuiLogMessage("This value is no regular ciphertext", NotificationLevel.Error);
            return 0;
        }


        /*
             DGK "zero decryption".

             This method only checks whether or not the ciphertext is an encryption of zero, or not.
         */
        private bool isZeroEncryption2(BigInteger cipher)
        {
            return (BigInteger.ModPow(cipher, vpvq, q) == 1);
        }

        /*
             DGK "zero decryption". Using CRT.

             This method only checks whether or not the ciphertext is an encryption of zero, or not.
         */
        private bool isZeroEncryption(BigInteger cipher)
        {
            BigInteger tmp_cp = BigInteger.ModPow(cipher, vpvq, p);
            BigInteger tmp_cq = BigInteger.ModPow(cipher, vpvq, q);
            BigInteger tmp = (tmp_cp * qq_inv + tmp_cq * pp_inv) % n;
            return (tmp == 1);
        }


        /*
            Using the homomorphic property of the DGK cryptosystem:
            This function multiplies two ciphertexts c1 = E(m1) and c2 = E(m2)
            in order to add the encrypted plaintexts: res = E(m1 + m2)
        */
        private BigInteger cipherAdd(BigInteger c1, BigInteger c2)
        {
            return (c1 * c2) % n;
        }

        /*
            Computing under the hom. encryption: res = E(m1 * exp)
            Raises ciphertext E(m1) = c to the power of exp.
        */
        private BigInteger cipherMul(BigInteger c, BigInteger exp)
        {
            return BigInteger.ModPow(c, exp, n);
        }

        /*
            This function first checks if exp is negative and then computes the result.
        */
        private BigInteger cipherMulSigned(BigInteger c, BigInteger exp)
        {
            return (exp < 0)
                ? cipherNeg(cipherMul(c, -exp))
                : cipherMul(c, exp);
        }

        /*
            Computing under the hom. encryption: res = E(m1 * (-exp))
            Raises ciphertext E(m1) = c to the power of exp.
        */
        private BigInteger cipherMulNeg(BigInteger c, BigInteger negExp)
        {
            return cipherMulSigned(c, -negExp);
        }

        /*
            Compute: res = E(-m)
            Computes the multiplicative inverse of some ciphertext c = E(m).
        */
        private BigInteger cipherNeg(BigInteger c)
        {
            return BigIntegerHelper.ModInverse(c, n);
        }

        /*
            Compute: res = E( c1 - c2 )
            Computes the multiplicative inverse of some c2 and multiplies this with c1.
        */
        private BigInteger cipherSub(BigInteger c1, BigInteger c2)
        {
            return cipherAdd(c1, cipherNeg(c2));
        }

        /*
	        Generates a random nonzero-plaintext and uses it for multiplicative blinding.
        */
        private BigInteger multiplicativeBlind(BigInteger cipher)
        {
            BigInteger r = BigIntegerHelper.RandomIntLimit(u - 1) + 1;
            return cipherMul(cipher, r);
        }

        /*
	        Re-randomization.
        */
        private BigInteger reRandomize(BigInteger cipher)
        {
            BigInteger r = BigIntegerHelper.RandomIntBits(DGK_BLINDING_T);
            return cipherAdd(cipher, cipherMul(h, r));
        }

        /*
	        Produces a random non-zero plaintext and encrypts it.
	        might be implemented as precomputation.
        */
        private BigInteger nonZeroEncryption()
        {
            BigInteger plain = BigIntegerHelper.RandomIntLimit(u - 1) + 1;
            return encrypt(plain);
        }

        #endregion

        #region Data Properties

        /// <summary>
        /// Gets/Sets public key n
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputNCaption", "InputNTooltip", true)]
        public BigInteger InputN
        {
            get => n;
            set
            {
                n = value;
                decrypttableIsValid = false;
                //OnPropertyChanged("InputN");
            }
        }

        /// <summary>
        /// Gets/Sets public key g
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputGCaption", "InputGTooltip", true)]
        public BigInteger InputG
        {
            get => g;
            set
            {
                g = value;
                decrypttableIsValid = false;
                //OnPropertyChanged("InputG");
            }
        }

        /// <summary>
        /// Gets/Sets public key h
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputHCaption", "InputHTooltip", true)]
        public BigInteger InputH
        {
            get => h;
            set
            {
                h = value;
                decrypttableIsValid = false;
                //OnPropertyChanged("InputH");
            }
        }

        /// <summary>
        /// Gets/Sets public key u
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputUCaption", "InputUTooltip", true)]
        public BigInteger InputU
        {
            get => u;
            set
            {
                u = value;
                decrypttableIsValid = false;
                //OnPropertyChanged("InputU");
            }
        }

        /// <summary>
        /// Gets/Sets private key vp
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputVPCaption", "InputVPTooltip")]
        public BigInteger InputVP
        {
            get => vp;
            set
            {
                vp = value;
                decrypttableIsValid = false;
                //OnPropertyChanged("InputVP");
            }
        }

        /// <summary>
        /// Gets/Sets private key vq
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputVQCaption", "InputVQTooltip")]
        public BigInteger InputVQ
        {
            get => vq;
            set
            {
                vq = value;
                decrypttableIsValid = false;
                //OnPropertyChanged("InputVQ");
            }
        }

        /// <summary>
        /// Gets/Sets private key p
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputPCaption", "InputPTooltip")]
        public BigInteger InputP
        {
            get => p;
            set
            {
                p = value;
                decrypttableIsValid = false;
                //OnPropertyChanged("InputP");
            }
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
            // setzt die Eingaben auf 0, damit sie keine ungültigen Werte von vorherigen Durchläufen beibehalten
            n = 0;
            p = 0;
            q = 0;
            u = 0;
            vp = 0;
            vq = 0;
            g = 0;
            h = 0;
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

            if (n < 2 * 3)
            {
                GuiLogMessage("Illegal Input N - DGK cannot work", NotificationLevel.Error);
                return;
            }

            n_square = n * n;

            // check whether the sectret key is provided
            secretKeyValid = false;
            if (p != null && p != 0)
            {
                q = n / p;

                if (vp != null && vp > 0 && vq != null && vq > 0) { vpvq = vp * vq; secretKeyValid = true; }

                // for faster de-/encryption:
                pp_inv = BigIntegerHelper.ModInverse(p, q) * p;
                qq_inv = BigIntegerHelper.ModInverse(q, p) * q;
            }

            if (settings.Action == 0)   // Encryption
            {
                if (InputM is BigInteger)
                {
                    OutputC1 = encrypt((BigInteger)InputM);
                }
                else if (InputM is byte[])
                {
                    OutputC2 = BlockConvert((byte[])InputM, (u < 256) ? 256 : u, n, encrypt, true);
                }
            }
            else if (settings.Action == 1)  // Decryption
            {
                if (!secretKeyValid)
                {
                    GuiLogMessage("Can't decrypt because secret key is not available", NotificationLevel.Error);
                    return;
                }

                if (!decrypttableIsValid)
                {
                    decrypttable = new BigInteger[(int)u];

                    BigInteger gv = BigInteger.ModPow(g, vp, p);
                    //for (int i = 0; i < (int)u; i++) decrypttable[i] = BigInteger.ModPow(gv,i,p);
                    decrypttableIsValid = true;

                    dechash = new Hashtable();
                    for (int i = 0; i < (int)u; i++)
                    {
                        dechash[BigInteger.ModPow(gv, i, p) % (((BigInteger)1) << 48)] = i;
                    }
                }

                if (InputM is BigInteger)
                {
                    OutputC1 = decrypt((BigInteger)InputM);
                }
                else if (InputM is byte[])
                {
                    OutputC2 = removeZeros(BlockConvert((byte[])InputM, n, (u < 256) ? 256 : u, decrypt, false));
                }
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
