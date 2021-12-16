/*
   Copyright 2019 Axel Wehage

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
using BlindSignatureGenerator;
using BlindSignatureGenerator.Properties;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.BlindSignatureGenerator
{
    [Author("Axel Wehage", "axel.wehage@unibw.de", "Universität der Bundeswehr München", "https://www.unibw.de")]
    [PluginInfo("BlindSignatureGenerator.Properties.Resources", "BlindSignatureGeneratorCaption", "BlindSignatureGeneratorTooltip", "BlindSignatureGenerator/userdoc.xml", new[] { "BlindSignatureGenerator/Images/Icon.png" })]
    [ComponentCategory(ComponentCategory.CiphersModernAsymmetric)]
    public class BlindSignatureGenerator : ICrypComponent
    {
        #region Private Variables
        private readonly BlindSignatureGeneratorSettings settings = new BlindSignatureGeneratorSettings();
        private readonly BlindSignatureGeneratorPresentation presentation = new BlindSignatureGeneratorPresentation();
        private static readonly Random random = new Random();
        private readonly RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        private BigInteger modulo;
        private BigInteger publickey;
        private BigInteger privatekey;
        private BigInteger blindingfactor;
        private object message;
        private object publicmessage;
        private BigInteger[] PaillierSignature;
        private BigInteger blindedmessage;
        private byte[] hash;
        private byte[] signature;
        private BigInteger securitylevel;
        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "MessageM", "MessageMTooltip")]
        public object Message
        {
            get => message;
            set
            {
                if (value is BigInteger)
                {
                    message = (BigInteger)value;
                }
                else if (value is byte[])
                {
                    message = value as byte[];
                }
                else if (value is string)
                {
                    message = Encoding.UTF8.GetBytes((string)value);
                }
                else
                {
                    if (value != null)
                    {
                        GuiLogMessage("Input type " + value.GetType() + " is not allowed", NotificationLevel.Error);
                    }

                    message = (BigInteger)0;
                }
            }
        }

        [PropertyInfo(Direction.InputData, "PublicMessage", "PublicMTooltip")]
        public object PublicMessage
        {
            get => publicmessage;
            set
            {
                if (value is BigInteger)
                {
                    publicmessage = (BigInteger)value;
                }
                else if (value is byte[])
                {
                    publicmessage = value as byte[];
                }
                else if (value is string)
                {
                    publicmessage = Encoding.UTF8.GetBytes((string)value);
                }
                else
                {
                    if (value != null)
                    {
                        GuiLogMessage("Input type " + value.GetType() + " is not allowed", NotificationLevel.Error);
                    }

                    publicmessage = (BigInteger)0;
                }
            }
        }

        [PropertyInfo(Direction.InputData, "ModuloN", "ModuloNTooltip")]
        public BigInteger Modulo
        {
            get => modulo;
            set => modulo = value;
        }

        [PropertyInfo(Direction.InputData, "PublicKey", "PublicKeyTooltip")]
        public BigInteger PublicKey
        {
            get => publickey;
            set => publickey = value;
        }

        [PropertyInfo(Direction.InputData, "PrivateKey", "PrivateKeyTooltip")]
        public BigInteger PrivateKey
        {
            get => privatekey;
            set => privatekey = value;
        }

        [PropertyInfo(Direction.OutputData, "BlindSignature", "BlindSignatureTooltip")]
        public byte[] BlindSignature
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "BlindSignatureAsNumber", "BlindSignatureAsNumberTooltip")]
        public BigInteger BlindSignatureNumber
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "BlindSignaturePaillier", "BlindSignaturePaillierTooltip")]
        public BigInteger[] BlindSignaturePaillier
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "Debug", "DebugTooltip")]
        public string Debug
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => presentation;

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            Debug = "";
            PaillierSignature = null;
            signature = null;
            publicmessage = null;
        }

        public static string ByteArrayToHexString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 100);
            StringBuilder DebugBuilder = new StringBuilder();
            byte[] m;
            BigInteger temp;
            DebugBuilder.AppendLine(Resources.Message_M_is_);
            if (Message is BigInteger)
            {
                m = ((BigInteger)Message).ToByteArray();
                DebugBuilder.AppendLine(((BigInteger)Message).ToString());
            }
            else
            {
                m = (byte[])Message;
                DebugBuilder.AppendLine(Encoding.UTF8.GetString(m, 0, m.Length));
            }
            // Hash is generated depending on the hash algorithm chosen in the settings
            hash = DoHashes(m);
            // Hash is being encrypted (signed) depending on the signature algorithm chosen in the settings and afterwards unblinded.
            switch (settings.SigningAlgorithm)
            {
                case BlindSignatureGeneratorSettings.SigningMode.RSA:
                    //A creates its array of blinded messages and C checks for a cheating attempt. The procedure stops if A has been found to be cheating.
                    BlindSigningAttackDefenderRSA();
                    //If A has not been found to be cheating the program resumes while using the one unchecked and still blinded message for signing.
                    DebugBuilder.AppendLine(Resources.Hash_h_M__is__);
                    DebugBuilder.AppendLine(ByteArrayToHexString(hash).Replace("-", " "));
                    DebugBuilder.AppendLine(Resources.Random_Number_k_is__);
                    DebugBuilder.AppendLine(blindingfactor.ToString());
                    //signing-process with RSA is being carried out
                    temp = BigInteger.ModPow(blindedmessage, PrivateKey, Modulo);
                    temp = UnBlindingRSA(temp);
                    signature = temp.ToByteArray();
                    break;

                case BlindSignatureGeneratorSettings.SigningMode.Paillier:
                    temp = new BigInteger(hash);
                    //A creates its array of blinded messages and C checks for a cheating attempt. The procedure stops if A has been found to be cheating.
                    BlindSigningAttackDefenderPaillier();
                    //calculate s1 
                    if (temp > Modulo) { GuiLogMessage("Message is bigger than N - this will produce a wrong result!", NotificationLevel.Warning); }
                    BigInteger Modulo2 = Modulo * Modulo;
                    BigInteger lambdainv = BigIntegerHelper.ModInverse(PrivateKey, Modulo);
                    BigInteger s1 = (((BigInteger.ModPow(temp, PrivateKey, Modulo2) - 1) / Modulo) * lambdainv) % Modulo;
                    //calculate s2
                    BigInteger InversePublicKey = BigIntegerHelper.ModInverse(PublicKey, Modulo);
                    BigInteger s2 = BigInteger.ModPow(InversePublicKey, s1, Modulo);
                    s2 = blindedmessage * s2;
                    BigInteger R3 = BigIntegerHelper.ModInverse(Modulo, PrivateKey);
                    s2 = BigInteger.ModPow(s2, R3, Modulo);
                    // s2 is now being un-blinded with s2 = s2 * (k^-1 mod n) mod n
                    s2 = UnBlindingPaillier(s2);
                    //signature is (s1,s2)
                    PaillierSignature = new BigInteger[2];
                    PaillierSignature[0] = s1;
                    PaillierSignature[1] = s2;
                    break;
            }
            DebugBuilder.AppendLine(Resources.Signature_s_is__);
            if (signature != null)
            {
                DebugBuilder.AppendLine(ByteArrayToHexString(signature).Replace("-", " "));
            }
            else
            {
                DebugBuilder.Append("s1: ");
                DebugBuilder.AppendLine(ByteArrayToHexString(PaillierSignature[0].ToByteArray()).Replace("-", " "));
                DebugBuilder.Append("s2: ");
                DebugBuilder.AppendLine(ByteArrayToHexString(PaillierSignature[1].ToByteArray()).Replace("-", " "));
            }
            Debug = DebugBuilder.ToString();
            OnPropertyChanged("Debug");

            BlindSignature = signature;
            OnPropertyChanged("BlindSignature");
            if (signature != null)
            {
                BlindSignatureNumber = new BigInteger(signature);
            }

            OnPropertyChanged("BlindSignatureNumber");

            BlindSignaturePaillier = PaillierSignature;
            OnPropertyChanged("BlindSignaturePaillier");

            presentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
            {
                presentation.signature = signature;
                presentation.message = Encoding.UTF8.GetString(m, 0, m.Length);
                presentation.modulo = Modulo;
                presentation.hash = hash;
                presentation.privateKey = PrivateKey;
                presentation.publicKey = PublicKey;
                presentation.current_step = 0;
                presentation.signaturepaillier = PaillierSignature;
                presentation.signatureNumber = BlindSignatureNumber;
                presentation.blindingfactor = blindingfactor;
                presentation.presentationEnabled = true;
                presentation.blindedmessage = blindedmessage;
            }, null);


            // HOWTO: You can pass error, warning, info or debug messages to the CT2 main window.
            GuiLogMessage("Blind Signature Generator finished.", NotificationLevel.Debug);
            // HOWTO: Make sure the progress bar is at maximum when your Execute() finished successfully.
            ProgressChanged(100, 100);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        public byte[] DoHashes(byte[] m)
        {
            byte[] calculatedhash = null;
            switch (settings.HashAlgorithm)
            {
                case BlindSignatureGeneratorSettings.HashMode.SHA1:
                    using (SHA1 sha1Hash = SHA1.Create())
                    {
                        calculatedhash = sha1Hash.ComputeHash(m);
                    }

                    break;
                case BlindSignatureGeneratorSettings.HashMode.SHA256:
                    using (SHA256 sha256Hash = SHA256.Create())
                    {
                        calculatedhash = sha256Hash.ComputeHash(m);
                    }

                    break;
                case BlindSignatureGeneratorSettings.HashMode.SHA384:
                    using (SHA384 sha384Hash = SHA384.Create())
                    {
                        calculatedhash = sha384Hash.ComputeHash(m);
                    }

                    break;
                case BlindSignatureGeneratorSettings.HashMode.SHA512:
                    using (SHA512 sha512Hash = SHA512.Create())
                    {
                        calculatedhash = sha512Hash.ComputeHash(m);
                    }

                    break;
                case BlindSignatureGeneratorSettings.HashMode.None:
                    calculatedhash = m;
                    break;
            }
            return calculatedhash;
        }

        public BigInteger BlindingRSA(BigInteger toblind)
        {
            //blindedresult is now being blinded with m * (k^e mod n) mod n
            BigInteger blindedresult = (toblind * BigInteger.ModPow(blindingfactor, PublicKey, Modulo)) % Modulo;
            return blindedresult;
        }
        public BigInteger BlindingPaillier(BigInteger toblind)
        {
            // blindedresult is being created with m * (k^n)
            BigInteger blindedresult = toblind * BigInteger.ModPow(blindingfactor, Modulo, Modulo);
            return blindedresult;
        }

        public BigInteger UnBlindingRSA(BigInteger blinded)
        {
            //blinded is now being un-blinded with s = s* * (k^-1 mod n) mod n
            BigInteger inverse = BigIntegerHelper.ModInverse(blindingfactor, Modulo);
            BigInteger unblinded = (blinded * inverse) % Modulo;
            return unblinded;
        }
        public BigInteger UnBlindingPaillier(BigInteger blinded)
        {
            // blinded is now being un-blinded with s2 = s2 * (k^-1 mod n) mod n
            BigInteger unblinded = (blinded * BigIntegerHelper.ModInverse(blindingfactor, Modulo)) % Modulo;
            return unblinded;
        }

        public void BlindSigningAttackDefenderRSA()
        {
            byte[] pm;
            securitylevel = BigInteger.Parse(settings.BlindSigningSecurity);
            if (PublicMessage == null)
            {
                GuiLogMessage("No public message given, cannot check for cheating attempt!", NotificationLevel.Warning);
                blindingfactor = Randomnumber();
                blindedmessage = BlindingRSA(new BigInteger(hash));
            }
            else
            {
                if (PublicMessage is BigInteger)
                {
                    pm = ((BigInteger)PublicMessage).ToByteArray();
                }
                else
                {
                    pm = (byte[])PublicMessage;
                }
                pm = DoHashes(pm);
                bool cheats = false;
                BigInteger checkpublicmessage = new BigInteger(pm);
                BigInteger[] factors = new BigInteger[(int)securitylevel];
                BigInteger[] messages = new BigInteger[(int)securitylevel];
                BigInteger checkmessage;
                //A creates a set of blind messages
                for (int i = 0; i < securitylevel; i++)
                {
                    blindingfactor = Randomnumber();
                    factors[i] = blindingfactor;
                    blindedmessage = BlindingRSA(new BigInteger(hash));
                    messages[i] = blindedmessage;
                }
                //C picks one at random.
                int chosenOne = random.Next((int)securitylevel);
                //C demands the unblinding of all other blind messages and checks if those are equal to what C has been told the message is.
                for (int i = 0; i < chosenOne; i++)
                {
                    blindingfactor = factors[i];
                    blindedmessage = messages[i];
                    //signing-process with RSA is being carried out
                    BigInteger temp = BigInteger.ModPow(blindedmessage, PrivateKey, Modulo);
                    checkmessage = UnBlindingRSA(temp);

                    checkmessage = BigInteger.ModPow(checkmessage, PublicKey, Modulo);

                    if (checkmessage != checkpublicmessage)
                    {
                        GuiLogMessage("Cheating detected! Generator will immediately stop!", NotificationLevel.Warning);
                        cheats = true;
                        break;
                    }
                }
                for (int i = (chosenOne + 1); i < securitylevel; i++)
                {
                    blindingfactor = factors[i];
                    blindedmessage = messages[i];
                    //signing-process with RSA is being carried out
                    BigInteger temp = BigInteger.ModPow(blindedmessage, PrivateKey, Modulo);
                    checkmessage = UnBlindingRSA(temp);

                    checkmessage = BigInteger.ModPow(checkmessage, PublicKey, Modulo);

                    if (checkmessage != checkpublicmessage)
                    {
                        GuiLogMessage("Cheating detected! Generator will immediately stop!", NotificationLevel.Warning);
                        cheats = true;
                        break;
                    }
                }

                if (cheats)
                {
                    Stop();
                    throw new Exception();
                }
                else
                {
                    GuiLogMessage("The public message has been verified, no cheating detected.", NotificationLevel.Warning);
                    //if C has found that A is not cheating it will resume with the blind signing process.
                    blindingfactor = factors[chosenOne];
                    blindedmessage = messages[chosenOne];
                }

                //note that the chosenOne message has not been unblinded and C has not read it. Therefore it remaines a blind signature.
            }
        }

        public void BlindSigningAttackDefenderPaillier()
        {
            byte[] pm;
            securitylevel = BigInteger.Parse(settings.BlindSigningSecurity);
            BigInteger temp0 = new BigInteger(hash);

            if (PublicMessage == null)
            {
                GuiLogMessage("No public message given, cannot check for cheating attempt!", NotificationLevel.Warning);
                blindingfactor = Randomnumber();
                blindedmessage = BlindingPaillier(new BigInteger(hash));
            }
            else
            {
                if (PublicMessage is BigInteger)
                {
                    pm = ((BigInteger)PublicMessage).ToByteArray();
                }
                else
                {
                    pm = (byte[])PublicMessage;
                }
                pm = DoHashes(pm);

                bool cheats = false;
                BigInteger checkpublicmessage = new BigInteger(pm);

                BigInteger[] factors = new BigInteger[(int)securitylevel];
                BigInteger[] messages = new BigInteger[(int)securitylevel];
                BigInteger signatures1;
                BigInteger signatures2;
                BigInteger Modulo2 = Modulo * Modulo;
                BigInteger InversePublicKey = BigIntegerHelper.ModInverse(PublicKey, Modulo);
                BigInteger lambdainv = BigIntegerHelper.ModInverse(PrivateKey, Modulo);
                BigInteger R3 = BigIntegerHelper.ModInverse(Modulo, PrivateKey);

                BigInteger checkpublicmessage1 = (((BigInteger.ModPow(checkpublicmessage, PrivateKey, Modulo2) - 1) / Modulo) * lambdainv) % Modulo;
                BigInteger checkpublicmessage2 = BigInteger.ModPow(InversePublicKey, checkpublicmessage1, Modulo);
                checkpublicmessage2 = checkpublicmessage * checkpublicmessage2;
                checkpublicmessage2 = BigInteger.ModPow(checkpublicmessage2, R3, Modulo);

                //A creates a set of blind messages
                for (int i = 0; i < securitylevel; i++)
                {
                    blindingfactor = Randomnumber();
                    factors[i] = blindingfactor;
                    blindedmessage = BlindingPaillier(new BigInteger(hash));
                    messages[i] = blindedmessage;
                }

                //C picks one at random.
                int chosenOne = random.Next((int)securitylevel);
                //C demands the unblinding of all other blind messages and checks if those are equal to what C has been told the message is.
                for (int i = 0; i < chosenOne; i++)
                {
                    blindingfactor = factors[i];
                    blindedmessage = messages[i];

                    signatures1 = (((BigInteger.ModPow(temp0, PrivateKey, Modulo2) - 1) / Modulo) * lambdainv) % Modulo;

                    signatures2 = BigInteger.ModPow(InversePublicKey, signatures1, Modulo);
                    signatures2 = blindedmessage * signatures2;
                    signatures2 = BigInteger.ModPow(signatures2, R3, Modulo);
                    signatures2 = UnBlindingPaillier(signatures2);

                    if (signatures1 != checkpublicmessage1 || signatures2 != checkpublicmessage2)
                    {
                        GuiLogMessage("Cheating detected! Generator will immediately stop!", NotificationLevel.Warning);
                        cheats = true;
                        break;
                    }
                }
                for (int i = (chosenOne + 1); i < securitylevel; i++)
                {
                    blindingfactor = factors[i];
                    blindedmessage = messages[i];

                    signatures1 = (((BigInteger.ModPow(temp0, PrivateKey, Modulo2) - 1) / Modulo) * lambdainv) % Modulo;

                    signatures2 = BigInteger.ModPow(InversePublicKey, signatures1, Modulo);
                    signatures2 = blindedmessage * signatures2;
                    signatures2 = BigInteger.ModPow(signatures2, R3, Modulo);
                    signatures2 = UnBlindingPaillier(signatures2);

                    if (signatures1 != checkpublicmessage1 || signatures2 != checkpublicmessage2)
                    {
                        GuiLogMessage("Cheating detected! Generator will immediately stop!", NotificationLevel.Warning);
                        cheats = true;
                        break;
                    }
                }

                if (cheats)
                {
                    Stop();
                    throw new Exception();
                }
                else
                {
                    GuiLogMessage("The public message has been verified, no cheating detected.", NotificationLevel.Warning);
                    //if C has found that A is not cheating it will resume with the blind signing process.
                    blindingfactor = factors[chosenOne];
                    blindedmessage = messages[chosenOne];
                }
                //note that the chosenOne message has not been unblinded and C has not read it. Therefore it remaines a blind signature.
            }
        }

        public BigInteger Randomnumber(int size = 16)
        {
            byte[] randomnumberBytes = new byte[size];
            BigInteger randomnumber;

            while (true)
            {
                try
                {
                    rng.GetBytes(randomnumberBytes);
                    randomnumber = new BigInteger(randomnumberBytes);
                    if (randomnumber < 0)
                    {
                        randomnumber *= (-1);
                    }
                    BigIntegerHelper.ModInverse(randomnumber, Modulo);
                    if (randomnumber != 0)
                    {
                        return randomnumber;
                    }
                }
                catch (Exception)
                {
                    //do nothing
                }
            }
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
