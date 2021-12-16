/*
   Copyright 2009 Sören Rinne, Ruhr-Universität Bochum, Germany

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
// for [MethodImpl(MethodImplOptions.Synchronized)]
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace CrypTool.BerlekampMassey
{
    [Author("Soeren Rinne", "soeren.rinne@CrypTool.org", "Ruhr-Universitaet Bochum, Chair for System Security", "http://www.trust.rub.de/")]
    [PluginInfo("CrypTool.BerlekampMassey.Properties.Resources", "PluginCaption", "PluginTooltip", "BerlekampMassey/DetailedDescription/doc.xml", "BerlekampMassey/Images/icon2.png", "BerlekampMassey/Images/icon2.png", "BerlekampMassey/Images/icon2.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class BerlekampMassey : ICrypComponent
    {

        #region Private variables

        private string input;
        private string polynomialOutput;
        private int output;

        private readonly BerlekampMasseyPresentation berlekampMasseyPresentation;

        #endregion

        #region Public interface

        /// <summary>
        /// Contructor
        /// </summary>
        public BerlekampMassey()
        {
            berlekampMasseyPresentation = new BerlekampMasseyPresentation();
            Presentation = berlekampMasseyPresentation;
        }

        /// <summary>
        /// Get or set all settings for this algorithm
        /// </summary>
        public ISettings Settings => null;

        [PropertyInfo(Direction.InputData, "InputCaption", "InputTooltip", true)]
        public string Input
        {
            get => input;
            set
            {
                input = value;
                OnPropertyChanged("Input");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip", false)]
        public int Output => output;

        [PropertyInfo(Direction.OutputData, "PolynomialOutputCaption", "PolynomialOutputTooltip", false)]
        public string PolynomialOutput
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => polynomialOutput;
        }

        #endregion

        #region IPlugin members

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public void Stop()
        {
        }

        public void PostExecution()
        {
        }

        public void PreExecution()
        {
        }

        public UserControl Presentation { get; private set; }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        #region Private methods

        /* This function is used under The Code Project Open License (CPOL)
         * http://www.codeproject.com/info/cpol10.aspx
        */
        private int BerlekampMasseyAlgorithm(byte[] s)
        {
            int L, N, m, d;
            int n = s.Length;
            byte[] c = new byte[n];
            byte[] b = new byte[n];
            byte[] t = new byte[n];

            //Initialization
            b[0] = c[0] = 1;
            N = L = 0;
            m = -1;

            //Algorithm core
            while (N < n)
            {
                d = s[N];
                for (int i = 1; i <= L; i++)
                {
                    d ^= c[i] & s[N - i];       //(d+=c[i]*s[N-i] mod 2)
                }

                if (d == 1)
                {
                    Array.Copy(c, t, n);        //T(D)<-C(D)
                    for (int i = 0; (i + N - m) < n; i++)
                    {
                        c[i + N - m] ^= b[i];
                    }

                    if (L <= (N >> 1))
                    {
                        L = N + 1 - L;
                        m = N;
                        Array.Copy(t, b, n);    //B(D)<-T(D)
                    }
                }
                N++;
            }

            string myC = null;

            foreach (byte myc in c)
            {
                myC += myc.ToString();
            }

            string polynomial = BuildPolynomialFromBinary(myC.ToCharArray());
            polynomialOutput = polynomial;
            berlekampMasseyPresentation.setPolynomial(polynomial);
            OnPropertyChanged("PolynomialOutput");

            //GuiLogMessage("C(D): " + myC, NotificationLevel.Info);
            //GuiLogMessage("polynomial: " + polynomial, NotificationLevel.Info);

            return L;
        }

        private static byte[] StrToByteArray(string StringToConvert)
        {
            char[] CharArray = StringToConvert.ToCharArray();
            byte[] ByteArray = new byte[CharArray.Length];

            for (int i = 0; i < CharArray.Length; i++)
            {
                ByteArray[i] = Convert.ToByte(int.Parse(CharArray[i].ToString()));
            }

            return ByteArray;
        }

        // Function to turn around tapSequence (01101 -> 10110)
        private char[] ReverseOrder(char[] tapSequence)
        {
            //String tempString = new String(tapSequence);
            //GuiLogMessage("tapSequence before = " + tempString, NotificationLevel.Info);
            char[] tempCharArray = new char[tapSequence.Length];

            int temp;
            for (int j = tapSequence.Length - 1; j >= 0; j--)
            {
                temp = (j - tapSequence.Length + 1) % (tapSequence.Length);
                if (temp < 0)
                {
                    temp *= -1;
                }
                //GuiLogMessage("temp = " + temp, NotificationLevel.Info);
                tempCharArray[j] = tapSequence[temp];
            }
            //tempString = new String(tempCharArray);
            //GuiLogMessage("tapSequence after = " + tempString, NotificationLevel.Info);
            return tempCharArray;
        }

        private string BuildPolynomialFromBinary(char[] tapSequence)
        {
            string polynomial = "";
            char[] tempTapSequence = ReverseOrder(tapSequence);
            int power;

            //build polynomial
            for (int i = 0; i < tapSequence.Length; i++)
            {
                power = (i - tapSequence.Length + 1) * -1 % tapSequence.Length;
                if (tempTapSequence[i] == '1')
                {
                    if (power == 1)
                    {
                        polynomial += "x + ";
                    }
                    else if (power != 0)
                    {
                        polynomial += "x^" + power + " + ";
                    }
                    else
                    {
                        polynomial += "1";
                    }
                }
            }

            return polynomial;
        }

        #endregion

        #region IPlugin Members

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
#pragma warning restore

        public void Execute()
        {
            try
            {
                string myInputString = input.Replace(",", "").Replace(" ", "");
                //GuiLogMessage("myInputString: " + myInputString, NotificationLevel.Info);

                byte[] inputByte = StrToByteArray(myInputString);

                // start counter
                DateTime startTime = DateTime.Now;
                output = BerlekampMasseyAlgorithm(inputByte);
                // stop counter
                DateTime stopTime = DateTime.Now;
                // compute overall time
                TimeSpan duration = stopTime - startTime;
                berlekampMasseyPresentation.setLength(output.ToString());
                OnPropertyChanged("Output");

                //GuiLogMessage("Complete!", NotificationLevel.Debug);
                GuiLogMessage("Time used: " + duration, NotificationLevel.Debug);

            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
            finally
            {
                ProgressChanged(1.0, 1.0);
            }
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        #endregion
    }
}
