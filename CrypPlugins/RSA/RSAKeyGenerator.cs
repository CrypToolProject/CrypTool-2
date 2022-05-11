/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CrypTool.Plugins.RSA
{
    [Author("Dennis Nolte, Raoul Falk, Sven Rech, Nils Kopal", "", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("RSA.Properties.Resources", "PluginKeyCaption", "PluginKeyTooltip", "RSA/DetailedDescription/dockeygen.xml", "RSA/iconkey.png")]
    [ComponentCategory(ComponentCategory.CiphersModernAsymmetric)]
    public class RSAKeyGenerator : ICrypComponent
    {
        #region private members

        private BigInteger _n;
        private BigInteger _e;
        private BigInteger _d;
        private BigInteger _p;
        private BigInteger _q;

        private RSAKeyGeneratorSettings settings = new RSAKeyGeneratorSettings();
        private bool _stopped = false;

        #endregion

        #region events

        public event CrypTool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;
        public event CrypTool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event CrypTool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region public

        /// <summary>
        /// Sets the N of the public/private key
        /// </summary>
        [PropertyInfo(Direction.OutputData, "NCaption", "NTooltip")]
        public BigInteger N
        {
            get => _n;
            set
            {
                _n = value;
                OnPropertyChanged("N");
            }
        }

        /// <summary>
        /// Sets the E of the public key
        /// </summary>
        [PropertyInfo(Direction.OutputData, "ECaption", "ETooltip")]
        public BigInteger E
        {
            get => _e;
            set
            {
                _e = value;
                OnPropertyChanged("E");
            }
        }

        /// <summary>
        /// Sets the D of the private key
        /// </summary>
        [PropertyInfo(Direction.OutputData, "DCaption", "DTooltip")]
        public BigInteger D
        {
            get => _d;
            set
            {
                _d = value;
                OnPropertyChanged("D");
            }
        }

        /// <summary>
        /// Sets the P of the private key
        /// </summary>
        [PropertyInfo(Direction.OutputData, "PCaption", "PTooltip")]
        public BigInteger P
        {
            get => _p;
            set
            {
                _p = value;
                OnPropertyChanged("P");
            }
        }

        /// <summary>
        /// Sets the Q of the private key
        /// </summary>
        [PropertyInfo(Direction.OutputData, "QCaption", "QTooltip")]
        public BigInteger Q
        {
            get => _q;
            set
            {
                _q = value;
                OnPropertyChanged("Q");
            }
        }

        /// <summary>
        /// Getter/Setter for the settings of this plugin
        /// </summary>
        public ISettings Settings
        {
            get => settings;
            set => settings = (RSAKeyGeneratorSettings)value;
        }

        /// <summary>
        /// Get the presentation of this plugin
        /// </summary>
        public System.Windows.Controls.UserControl Presentation => null;

        /// <summary>
        /// This method is called by the environment before execution
        /// </summary>
        public void PreExecution()
        {
        }

        /// <summary>
        /// Called by the environment to start generating of public/private keys
        /// </summary>
        public void Execute()
        {
            _stopped = false;
            BigInteger p;
            BigInteger q;
            BigInteger n;
            BigInteger e;
            BigInteger d;

            ProgressChanged(0.0, 1.0);

            switch (settings.Source)
            {
                // manual
                case 0:
                    if (settings.E_or_D == 0) //user knows e
                    {
                        try
                        {

                            p = BigIntegerHelper.ParseExpression(settings.P);
                            q = BigIntegerHelper.ParseExpression(settings.Q);
                            e = BigIntegerHelper.ParseExpression(settings.E);

                            if (!BigIntegerHelper.IsProbablePrime(p))
                            {
                                GuiLogMessage(p.ToString() + " is not prime!", NotificationLevel.Error);
                                return;
                            }
                            if (!BigIntegerHelper.IsProbablePrime(q))
                            {
                                GuiLogMessage(q.ToString() + " is not prime!", NotificationLevel.Error);
                                return;
                            }
                            if (p == q)
                            {
                                GuiLogMessage("The primes P and Q cannot be equal!", NotificationLevel.Error);
                                return;
                            }

                        }
                        catch (Exception ex)
                        {
                            GuiLogMessage("Invalid Big Number input: " + ex.Message, NotificationLevel.Error);
                            return;
                        }

                        try
                        {
                            D = BigIntegerHelper.ModInverse(e, (p - 1) * (q - 1));
                        }
                        catch (Exception)
                        {
                            GuiLogMessage("RSAKeyGenerator Error: E (" + e + ") cannot be inverted.", NotificationLevel.Error);
                            return;
                        }

                        try
                        {
                            N = p * q;
                            E = e;
                            P = p;
                            Q = q;
                        }
                        catch (Exception ex)
                        {
                            GuiLogMessage("Big Number fail: " + ex.Message, NotificationLevel.Error);
                            return;
                        }
                    }
                    else //user knows d
                    {
                        try
                        {

                            p = BigIntegerHelper.ParseExpression(settings.P);
                            q = BigIntegerHelper.ParseExpression(settings.Q);
                            d = BigIntegerHelper.ParseExpression(settings.D);

                            if (!BigIntegerHelper.IsProbablePrime(p))
                            {
                                GuiLogMessage(p.ToString() + " is not prime!", NotificationLevel.Error);
                                return;
                            }
                            if (!BigIntegerHelper.IsProbablePrime(q))
                            {
                                GuiLogMessage(q.ToString() + " is not prime!", NotificationLevel.Error);
                                return;
                            }
                            if (p == q)
                            {
                                GuiLogMessage("The primes P and Q cannot be equal!", NotificationLevel.Error);
                                return;
                            }

                        }
                        catch (Exception ex)
                        {
                            GuiLogMessage("Invalid Big Number input: " + ex.Message, NotificationLevel.Error);
                            return;
                        }

                        try
                        {
                            E = BigIntegerHelper.ModInverse(d, (p - 1) * (q - 1));
                        }
                        catch (Exception)
                        {
                            GuiLogMessage("RSAKeyGenerator Error: D (" + d + ") cannot be inverted.", NotificationLevel.Error);
                            return;
                        }

                        try
                        {
                            N = p * q;
                            D = d;
                            P = p;
                            Q = q;
                        }
                        catch (Exception ex)
                        {
                            GuiLogMessage("Big Number fail: " + ex.Message, NotificationLevel.Error);
                            return;
                        }
                    }
                    break;

                case 1:
                    try
                    {
                        n = BigIntegerHelper.ParseExpression(settings.N);
                        d = BigIntegerHelper.ParseExpression(settings.D);
                        e = BigIntegerHelper.ParseExpression(settings.E);
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage("Invalid Big Number input: " + ex.Message, NotificationLevel.Error);
                        return;
                    }

                    try
                    {
                        N = n;
                        E = e;
                        D = d;
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage("Big Number fail: " + ex.Message, NotificationLevel.Error);
                        return;
                    }
                    break;

                //randomly generated
                case 2:
                    try
                    {
                        n = BigInteger.Parse(settings.Range);

                        switch (settings.RangeType)
                        {
                            case 0: // n = number of bits for primes
                                if ((int)n <= 2)
                                {
                                    GuiLogMessage("Value for n has to be greater than 2.", NotificationLevel.Error);
                                    return;
                                }

                                if (n >= 1024)
                                {
                                    GuiLogMessage("Please note that the generation of prime numbers with " + n + " bits may take some time...", NotificationLevel.Warning);
                                }

                                // calculate the number of expected tries for the indeterministic prime number generation using the density of primes in the given region
                                BigInteger limit = ((BigInteger)1) << (int)n;
                                limittries = (int)(BigInteger.Log(limit) / 6);
                                if (settings.GenerateSafePrimes)
                                {
                                    //simply by "eye balling" estimated value to allow to see a progress
                                    //also when generating safe primes
                                    limittries = limittries * 1000;
                                }
                                expectedtries = 2 * limittries;

                                tries = 0;
                                p = settings.GenerateSafePrimes ? RandomSafePrimeBits((int)n) : RandomPrimeBits((int)n);
                                tries = limittries; 
                                limittries = expectedtries;
                                do
                                {
                                    q = settings.GenerateSafePrimes ? RandomSafePrimeBits((int)n) : RandomPrimeBits((int)n);
                                } while (p == q && !_stopped);
                                break;

                            case 1: // n = upper limit for primes
                            default:
                                if (n <= 4)
                                {
                                    GuiLogMessage("Value for n has to be greater than 4", NotificationLevel.Error);
                                    return;
                                }

                                p = settings.GenerateSafePrimes ? BigIntegerHelper.RandomSafePrimeLimit(n + 1, ref _stopped) : BigIntegerHelper.RandomPrimeLimit(n + 1, ref _stopped);
                                ProgressChanged(0.5, 1.0);
                                do
                                {
                                    q = settings.GenerateSafePrimes ? BigIntegerHelper.RandomSafePrimeLimit(n + 1, ref _stopped) : BigIntegerHelper.RandomPrimeLimit(n + 1, ref _stopped);
                                } while (p == q && !_stopped);
                                break;
                        }
                    }
                    catch
                    {
                        GuiLogMessage("Please enter an integer value for n.", NotificationLevel.Error);
                        return;
                    }

                    BigInteger phi = (p - 1) * (q - 1);

                    // generate E for the given values of p and q
                    bool found = false;
                    foreach (BigInteger ee in new BigInteger[] { 3, 5, 7, 11, 17, 65537 })
                    {
                        if (ee < phi && ee.GCD(phi) == 1) { E = ee; found = true; break; }
                    }
                    if (!found)
                    {
                        for (int i = 0; i < 1000; i++)
                        {
                            if (_stopped)
                            {
                                return;
                            }

                            e = BigIntegerHelper.RandomIntLimit(phi, ref _stopped);
                            if (e >= 2 && e.GCD(phi) == 1) { E = e; found = true; break; }
                        }
                    }
                    if (!found)
                    {
                        GuiLogMessage("Could not generate a valid E for p=" + p + " and q=" + q + ".", NotificationLevel.Error);
                        return;
                    }

                    N = p * q;
                    D = BigIntegerHelper.ModInverse(E, phi);
                    P = p;
                    Q = q;
                    break;

                //using x509 certificate
                case 3:
                    try
                    {

                        X509Certificate2 cert;
                        RSAParameters par;

                        if (!string.IsNullOrEmpty(settings.Password))
                        {
                            GuiLogMessage("Password entered. Try getting public and private key", NotificationLevel.Info);
                            cert = new X509Certificate2(settings.CertificateFile, settings.Password, X509KeyStorageFlags.Exportable);
                            if (cert == null || cert.PrivateKey == null)
                            {
                                throw new Exception("Private Key of X509Certificate could not be fetched");
                            }

                            RSACryptoServiceProvider provider = (RSACryptoServiceProvider)cert.PrivateKey;
                            par = provider.ExportParameters(true);

                        }
                        else
                        {
                            GuiLogMessage("No Password entered. Try loading public key only", NotificationLevel.Info);
                            cert = new X509Certificate2(settings.CertificateFile);
                            if (cert == null || cert.PublicKey == null || cert.PublicKey.Key == null)
                            {
                                throw new Exception("Private Key of X509Certificate could not be fetched");
                            }

                            RSACryptoServiceProvider provider = (RSACryptoServiceProvider)cert.PublicKey.Key;
                            par = provider.ExportParameters(false);
                        }

                        try
                        {
                            N = new BigInteger(par.Modulus);
                        }
                        catch (Exception ex)
                        {
                            GuiLogMessage("Could not get N from certificate: " + ex.Message, NotificationLevel.Warning);
                        }

                        try
                        {
                            E = new BigInteger(par.Exponent);
                        }
                        catch (Exception ex)
                        {
                            GuiLogMessage("Could not get E from certificate: " + ex.Message, NotificationLevel.Warning);
                        }

                        try
                        {
                            if (!string.IsNullOrEmpty(settings.Password))
                            {
                                D = new BigInteger(par.D);
                            }
                        }
                        catch (Exception ex)
                        {
                            GuiLogMessage("Could not get D from certificate: " + ex.Message, NotificationLevel.Warning);
                        }

                        try
                        {
                            if (!string.IsNullOrEmpty(settings.Password))
                            {
                                P = new BigInteger(par.P);
                            }
                        }
                        catch (Exception ex)
                        {
                            GuiLogMessage("Could not get P from certificate: " + ex.Message, NotificationLevel.Warning);
                        }

                        try
                        {
                            if (!string.IsNullOrEmpty(settings.Password))
                            {
                                Q = new BigInteger(par.Q);
                            }
                        }
                        catch (Exception ex)
                        {
                            GuiLogMessage("Could not get Q from certificate: " + ex.Message, NotificationLevel.Warning);
                        }                        
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage("Could not load the selected certificate: " + ex.Message, NotificationLevel.Error);
                    }
                    break;
            }
            ProgressChanged(1.0, 1.0);
            _stopped = true;
        }

        /// <summary>
        /// This method is called by the environment after execution
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// This method is called by the environment to stop execution
        /// </summary>
        public void Stop()
        {
            _stopped = true;
        }

        /// <summary>
        /// This method is called by the environment to initialise the plugin
        /// </summary>
        public void Initialize()
        {
            settings.UpdateTaskPaneVisibility();
        }

        /// <summary>
        /// This method is called by the environment to dispose
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        // variables for progress bar handling when big prime numbers are requested
        private int tries;          // number of actual tries
        private int expectedtries;  // number of expected tries for both primes
        private int limittries;     // number up to which to increase tries

        private BigInteger RandomSafePrimeBits(int bits)
        {
            if (bits < 0)
            {
                throw new ArithmeticException("Enter a positive bitcount");
            }

            BigInteger limit = ((BigInteger)1) << bits;
            if (limit <= 2)
            {
                throw new ArithmeticException("No primes below this limit");
            }

            while (true)
            {
                if (_stopped)
                {
                    return -1;
                }

                BigInteger p = NextProbableSafePrime(limit.RandomIntLimit(ref _stopped));
                if (p < limit)
                {
                    return p;
                }
            }
        }

        private BigInteger RandomPrimeBits(int bits)
        {
            if (bits < 0)
            {
                throw new ArithmeticException("Enter a positive bitcount");
            }

            BigInteger limit = ((BigInteger)1) << bits;
            if (limit <= 2)
            {
                throw new ArithmeticException("No primes below this limit");
            }

            while (true)
            {
                if (_stopped)
                {
                    return -1;
                }

                BigInteger p = NextProbablePrime(limit.RandomIntLimit(ref _stopped));
                if (p < limit)
                {
                    return p;
                }
            }
        }

        private BigInteger NextProbablePrime(BigInteger n)
        {
            if (n < 0)
            {
                throw new ArithmeticException("NextProbablePrime cannot be called on value < 0");
            }

            if (n <= 2)
            {
                return 2;
            }

            if (n.IsEven)
            {
                n++;
            }

            if (n == 3)
            {
                return 3;
            }

            BigInteger r = n % 6;
            if (r == 3)
            {
                n += 2;
            }

            if (r == 1)
            {
                if (n.IsProbablePrime(ref _stopped))
                {
                    return n;
                }
                else
                {
                    n += 4;
                }
            }

            // at this point n mod 6 = 5

            while (true)
            {
                ProgressChanged((int)(tries * 100.0 / expectedtries), 100);
                if (tries + 1 < limittries)
                {
                    tries++;
                }

                if (n.IsProbablePrime(ref _stopped))
                {
                    return n;
                }

                n += 2;
                if (n.IsProbablePrime(ref _stopped))
                {
                    return n;
                }

                n += 4;
                if (_stopped)
                {
                    return -1;
                }
            }
        }

        private BigInteger NextProbableSafePrime(BigInteger n)
        {
            if (n < 0)
            {
                throw new ArithmeticException("NextProbablePrime cannot be called on value < 0");
            }

            if (n <= 2)
            {
                return 2;
            }

            if (n.IsEven)
            {
                n++;
            }

            if (n == 3)
            {
                return 3;
            }

            BigInteger r = n % 6;
            if (r == 3)
            {
                n += 2;
            }

            if (r == 1)
            {
                if (n.IsProbableSafePrime(ref _stopped))
                {
                    return n;
                }
                else
                {
                    n += 4;
                }
            }

            // at this point n mod 6 = 5

            while (true)
            {
                ProgressChanged((int)(tries * 100.0 / expectedtries), 100);
                if (tries + 1 < limittries)
                {
                    tries++;
                }

                if (n.IsProbableSafePrime(ref _stopped))
                {
                    return n;
                }

                n += 2;
                if (n.IsProbableSafePrime(ref _stopped))
                {
                    return n;
                }

                n += 4;
                if (_stopped)
                {
                    return -1;
                }
            }
        }

        #region private

        /// <summary>
        /// Changes the progress of this plugin
        /// </summary>
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        /// <summary>
        /// Logs a message to CrypTool
        /// </summary>
        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        /// <summary>
        /// The property name changed
        /// </summary>
        /// <param name="name">name</param>
        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion

    }//end RSAKeyGenerator

}//end CrypTool.Plugins.RSA
