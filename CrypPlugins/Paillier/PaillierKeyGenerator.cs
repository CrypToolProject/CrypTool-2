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

namespace CrypTool.Plugins.Paillier
{
    [Author("Armin Krauss, Martin Franz", "", "", "http://www.uni-due.de")]
    [PluginInfo("Paillier.Properties.Resources", "PluginKeyCaption", "PluginKeyTooltip", "Paillier/DetailedDescription/dockeygen.xml", "Paillier/Image/PaillierKey.png")]
    [ComponentCategory(ComponentCategory.CiphersModernAsymmetric)]
    internal
    /**
<summary>
This plugin is a generator plugin which helps the user to generate pairs of private/public keys
for the Paillier encryption

there are several modes:

1. manual
in this mode p and q are given by the user

2. random
in this mode the keys will be generated randomly with a given bitlength

</summary>    
**/
    class PaillierKeyGenerator : ICrypComponent
    {
        #region private members

        private BigInteger n;       // public key
        private BigInteger g;       // public key
        private BigInteger lambda;  // private key

        private PaillierKeyGeneratorSettings settings = new PaillierKeyGeneratorSettings();

        #endregion

        #region events

        public event CrypTool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;
        public event CrypTool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event CrypTool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region public

        //public PaillierKeyGenerator()
        //{
        //    //this.settings = new PaillierKeyGeneratorSettings();
        //    //twoPowKeyBitLength = 1 << (keyBitLength - 1);
        //    //generateKeys();
        //}

        /// <summary>
        /// Sets the modulus N (public key)
        /// </summary>
        [PropertyInfo(Direction.OutputData, "NCaption", "NTooltip")]
        public BigInteger N
        {
            get => n;
            set
            {
                n = value;
                OnPropertyChanged("N");
            }
        }

        /// <summary>
        /// Sets the G of the public key
        /// </summary>
        [PropertyInfo(Direction.OutputData, "GCaption", "GTooltip")]
        public BigInteger G
        {
            get => g;
            set
            {
                g = value;
                OnPropertyChanged("G");
            }
        }

        /// <summary>
        /// Sets the Lambda of the private key
        /// </summary>
        [PropertyInfo(Direction.OutputData, "LambdaCaption", "LambdaTooltip")]
        public BigInteger Lambda
        {
            get => lambda;
            set
            {
                lambda = value;
                OnPropertyChanged("Lambda");
            }
        }

        /// <summary>
        /// Getter/Setter for the settings of this plugin
        /// </summary>
        public ISettings Settings
        {
            get => settings;
            set => settings = (PaillierKeyGeneratorSettings)value;
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
            BigInteger p = 1, q = 1;

            ProgressChanged(0, 1);

            switch (settings.Source)
            {
                // manually enter primes
                case 0:
                    try
                    {
                        p = BigIntegerHelper.ParseExpression(settings.P);
                        q = BigIntegerHelper.ParseExpression(settings.Q);

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


                    break;

                //randomly generated primes
                case 1:
                    try
                    {
                        int keyBitLength = Convert.ToInt32(settings.KeyBitLength);
                        if (keyBitLength < 4)
                        {
                            GuiLogMessage("The keylength must be greater than 3.", NotificationLevel.Error);
                            return;
                        }

                        int i, maxtries = 10;
                        for (i = 0; i < maxtries; i++)
                        {
                            p = BigIntegerHelper.RandomPrimeMSBSet(keyBitLength - (keyBitLength / 2));
                            q = BigIntegerHelper.RandomPrimeMSBSet(keyBitLength / 2);
                            //GuiLogMessage("p = " + p.ToString(), NotificationLevel.Info);
                            //GuiLogMessage("q = " + q.ToString(), NotificationLevel.Info);
                            if (p != q)
                            {
                                break;
                            }
                        }
                        if (i == maxtries)
                        {
                            throw new Exception("Could not create two differing primes");
                        }
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage(ex.Message, NotificationLevel.Error);
                        return;
                    }
                    break;

                default:
                    throw new Exception("Illegal Key Generation Mode");
            }

            N = p * q;
            G = N + 1;
            Lambda = (p - 1).LCM((q - 1));

            ProgressChanged(1, 1);
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

        private void ChangePluginIcon(int Icon)
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, Icon));
            }
        }

        #endregion

    }//end PaillierKeyGenerator

}//end CrypTool.Plugins.Paillier
