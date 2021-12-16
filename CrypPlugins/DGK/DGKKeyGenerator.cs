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
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Numerics;

namespace CrypTool.Plugins.DGK
{
    [Author("Armin Krauss, Martin Franz", "", "", "http://www.uni-due.de")]
    [PluginInfo("DGK.Properties.Resources", "PluginKeyCaption", "PluginKeyTooltip", "DGK/DetailedDescription/dockeygen.xml", "DGK/Image/DGKKey.png")]
    [ComponentCategory(ComponentCategory.CiphersModernAsymmetric)]
    internal
    /**
<summary>
This plugin is a generator plugin which helps the user to generate pairs of private/public keys
for the DGK encryption

there are several modes:

1. manual
in this mode p and q are given by the user

2. random
in this mode the keys will be generated randomly with a given bitlength

</summary>    
**/
    class DGKKeyGenerator : ICrypComponent
    {
        #region private members

        private BigInteger n;          // public key
        private BigInteger g;          // public key
        private BigInteger h;          // public key
        private BigInteger u;          // public key

        private BigInteger vp;         // private key
        private BigInteger vq;         // private key
        private BigInteger p;          // private key
        private BigInteger q;          // private key

        private BigInteger vpvq;

        private DGKKeyGeneratorSettings settings = new DGKKeyGeneratorSettings();

        #endregion

        #region events

        public event CrypTool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;
        public event CrypTool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event CrypTool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region public

        //public DGKKeyGenerator()
        //{
        //    //this.settings = new DGKKeyGeneratorSettings();
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
            set => n = value;//OnPropertyChanged("N");
        }

        /// <summary>
        /// Sets the G of the public key
        /// </summary>
        [PropertyInfo(Direction.OutputData, "GCaption", "GTooltip")]
        public BigInteger G
        {
            get => g;
            set => g = value;//OnPropertyChanged("G");
        }

        /// <summary>
        /// Sets the H of the public key
        /// </summary>
        [PropertyInfo(Direction.OutputData, "HCaption", "HTooltip")]
        public BigInteger H
        {
            get => h;
            set => h = value;//OnPropertyChanged("H");
        }

        /// <summary>
        /// Sets the U of the public key
        /// </summary>
        [PropertyInfo(Direction.OutputData, "UCaption", "UTooltip")]
        public BigInteger U
        {
            get => u;
            set => u = value;//OnPropertyChanged("U");
        }

        /// <summary>
        /// Sets the VP of the secret key
        /// </summary>
        [PropertyInfo(Direction.OutputData, "VPCaption", "VPTooltip")]
        public BigInteger VP
        {
            get => vp;
            set => vp = value;//OnPropertyChanged("VP");
        }
        /// <summary>
        /// Sets the VQ of the secret key
        /// </summary>
        [PropertyInfo(Direction.OutputData, "VQCaption", "VQTooltip")]
        public BigInteger VQ
        {
            get => vq;
            set => vq = value;//OnPropertyChanged("VQ");
        }

        /// <summary>
        /// Sets the P of the secret key
        /// </summary>
        [PropertyInfo(Direction.OutputData, "PCaption", "PTooltip")]
        public BigInteger P
        {
            get => p;
            set => p = value;//OnPropertyChanged("P");
        }

        /// <summary>
        /// Getter/Setter for the settings of this plugin
        /// </summary>
        public ISettings Settings
        {
            get => settings;
            set => settings = (DGKKeyGeneratorSettings)value;
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

        private void generateKeys(int k, int t, int l, BigInteger ownu)
        {
            if (l < 8 || l > 16)
            {
                throw new Exception("Choose parameter l from the interval 8<=l<=16 !");
            }

            if (t <= l)
            {
                throw new Exception("Parameter t must be greater than l.");
            }

            if (k <= t)
            {
                throw new Exception("Parameter k must be greater than t.");
            }

            if (k % 2 != 0)
            {
                throw new Exception("Parameter k has to be an even number!");
            }

            //if ((k / 2 < (l + 5)) || (k / 2 < t + 2))
            //    throw new Exception("Parameter k has to be specified by the rules k/2 > l+4 and k/2 > t+1!");

            if (k / 2 < l + t + 10)
            {
                throw new Exception("Choose parameters k,l,t so that k/2 >= l+t+10 !");
            }

            // generate u the minimal prime number greater than l+2
            //Workaround, TODO:
            //u = (l == 0) ? ownu : BigIntegerHelper.NextProbablePrime(l + l + 2);
            u = BigIntegerHelper.NextProbablePrime((1 << l) + 2);

            // generate vp, vq as a random t bit prime number
            vp = BigIntegerHelper.RandomPrimeBits(t);
            vq = BigIntegerHelper.RandomPrimeBits(t);

            // store the product vp*vq
            vpvq = vp * vq;

            // DGK style to generate p and q from u and v:

            // p is chosen as rp * u * vp + 1 where r_p is randomly chosen such that p has roughly k/2 bits
            BigInteger rp, rq, tmp;

            int needed_bits;

            tmp = u * vp;
            needed_bits = k / 2 - (int)Math.Ceiling(BigInteger.Log(tmp, 2));
            //int needed_bits = k / 2 - mpz_sizeinbase(tmp1, 2);

            do
            {
                rp = BigIntegerHelper.RandomIntMSBSet(needed_bits - 1) * 2;
                p = rp * tmp + 1;
            } while (!BigIntegerHelper.IsProbablePrime(p));

            // q is chosen as rq * u*vq + 1 where rq is randomly chosen such that q has roughly k/2 bits

            tmp = u * vq;
            needed_bits = k / 2 - (int)Math.Ceiling(BigInteger.Log(tmp, 2));
            do
            {
                rq = BigIntegerHelper.RandomIntMSBSet(needed_bits - 1) * 2;
                q = rq * tmp + 1;
            } while (!BigIntegerHelper.IsProbablePrime(q));

            // RSA modulus n
            N = p * q;

            /*
             h must be random in Zn* and have order vp*vq. We
             choose it by setting

             h = h' ^{rp * rq * u}.

             Seeing h as (hp, hq) and h' as (h'p, h'q) in Zp* x Zq*, we
             then have

             (hp^vpvq, hq^vpvq) = (h'p^{rp*u*vp}^(rq*vq), h'q^{rq*u*vq}^(rp*vp))
             = (1^(rq*vq), 1^(rp*vp)) = (1, 1)

             which means that h^(vpvq) = 1 in Zn*.

             So we only need to check that h is not 1 and that it really
             is in Zn*.
             */

            BigInteger r;

            tmp = rp * rq * u;

            while (true)
            {
                r = BigIntegerHelper.RandomIntLimit(n);
                h = BigInteger.ModPow(r, tmp, n);
                if (h == 1)
                {
                    continue;
                }

                if (BigInteger.GreatestCommonDivisor(h, n) == 1)
                {
                    break;
                }
            }

            /*
             g is chosen at random in Zn* such that it has order uv. This
             is done in much the same way as for h, but the order of
             power of the random number might be u, v or uv. We therefore
             also check that g^u and g^v are different from 1.
             */

            BigInteger rprq = rp * rq;

            while (true)
            {
                r = BigIntegerHelper.RandomIntLimit(n);
                g = BigInteger.ModPow(r, rprq, n);

                // test if g is "good":
                if (g == 1)
                {
                    continue;
                }

                if (BigInteger.GreatestCommonDivisor(g, n) != 1)
                {
                    continue;
                }

                if (BigInteger.ModPow(g, u, n) == 1)
                {
                    continue;      // test if ord(g) == u
                }

                if (BigInteger.ModPow(g, vp, n) == 1)
                {
                    continue;     // test if ord(g) == vp
                }

                if (BigInteger.ModPow(g, vq, n) == 1)
                {
                    continue;     // test if ord(g) == vq
                }

                if (BigInteger.ModPow(g, u * vp, n) == 1)
                {
                    continue; // test if ord(g) == u*vp
                }

                if (BigInteger.ModPow(g, u * vq, n) == 1)
                {
                    continue; // test if ord(g) == u*vq
                }

                if (BigInteger.ModPow(g, vpvq, n) == 1)
                {
                    continue;   // test if ord(g) == vp*vq
                }

                break;  // g has passed all tests
            }

        }

        /// <summary>
        /// Called by the environment to start generating of public/private keys
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            try
            {
                int k = Convert.ToInt32(settings.BitSizeK);
                int t = Convert.ToInt32(settings.BitSizeT);
                int l = Convert.ToInt32(settings.LimitL);

                generateKeys(k, t, l, 40);
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
                return;
            }

            OnPropertyChanged("N");
            OnPropertyChanged("G");
            OnPropertyChanged("H");
            OnPropertyChanged("U");
            OnPropertyChanged("VP");
            OnPropertyChanged("VQ");
            OnPropertyChanged("P");

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

    }//end DGKKeyGenerator

}//end CrypTool.Plugins.DGK
