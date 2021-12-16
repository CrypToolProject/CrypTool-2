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

namespace CrypTool.Plugins.Numbers
{
    [Author("Sven Rech, Nils Kopal", "sven.rech@CrypTool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("CrypTool.Plugins.Numbers.Properties.Resources", "PluginOperationCaption", "PluginOperationTooltip",
        "Numbers/DetailedDescription/doc.xml",
        "Numbers/icons/plusIcon.png",
        "Numbers/icons/minusIcon.png",
        "Numbers/icons/timesIcon.png",
        "Numbers/icons/divIcon.png",
        "Numbers/icons/powIcon.png",
        "Numbers/icons/gcdicon.png",
        "Numbers/icons/lcmIcon.png",
        "Numbers/icons/rootIcon.png",
        "Numbers/icons/inverseIcon.png",
        "Numbers/icons/phiIcon.png",
        "Numbers/icons/divsumIcon.png",
        "Numbers/icons/divnumIcon.png",
        "Numbers/icons/PiIcon.png",
        "Numbers/icons/PrimeNIcon.png",
        "Numbers/icons/NextPrimeIcon.png",
        "Numbers/icons/PrevOrimeIcon.png",
        "Numbers/icons/IsPrime.png",
        "Numbers/icons/abs.png",
        "Numbers/icons/factorial.png",
        "Numbers/icons/crosssum.png",
        "Numbers/icons/dlog.png",
        "Numbers/icons/nPr.png",
        "Numbers/icons/nCr.png"
        )]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    internal class NumberOperations : ICrypComponent
    {

        #region private variable
        //The variables are defined
        private BigInteger input1 = 0;
        private BigInteger input2 = 0;
        private BigInteger mod = 0;
        private BigInteger output = 0;
        private NumberSettings settings = new NumberSettings();
        private bool _running = false;

        private bool input1Updated = false;
        private bool input2Updated = false;


        #endregion

        #region event

        public event CrypTool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;

        public event CrypTool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public event CrypTool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;

        #endregion

        #region public

        public NumberOperations()
        {
            settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
        }

        /// <summary>
        /// The inputs are defined.
        /// Only BigInteger are accepted.
        /// </summary>
        [PropertyInfo(Direction.InputData, "Input1Caption", "Input1Tooltip", true)]
        public BigInteger Input1
        {
            get => input1;
            set
            {
                input1 = value;
                input1Updated = true;
                OnPropertyChanged("Input1");
            }
        }


        [PropertyInfo(Direction.InputData, "Input2Caption", "Input2Tooltip", false)]
        public BigInteger Input2
        {
            get => input2;
            set
            {
                input2 = value;
                input2Updated = true;
                OnPropertyChanged("Input2");
            }
        }


        [PropertyInfo(Direction.InputData, "ModCaption", "ModTooltip")]
        public BigInteger Mod
        {
            get => mod;
            set
            {
                mod = value;
                OnPropertyChanged("Mod");
            }
        }

        /// <summary>
        /// The output is defined.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip")]
        public BigInteger Output
        {
            get => output;
            set
            {
                output = value;
                OnPropertyChanged("Output");
            }
        }


        /// <summary>
        /// Showing the progress change while plug-in is working
        /// </summary>
        /// <param name="value">Value of current process progress</param>
        /// <param name="max">Max value for the current progress</param>
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }


        public ISettings Settings
        {
            get => settings;
            set => settings = (NumberSettings)value;
        }


        public System.Windows.Controls.UserControl Presentation => null;

        public void PreExecution()
        {
            input1 = 0;
            input2 = 0;
            input1Updated = false;
            input2Updated = false;
            mod = 0;
        }

        /// <summary>
        /// Main method
        /// </summary>
        public void Execute()
        {
            BigInteger result = 0;
            _running = true;

            //First checks if both inputs are set
            if (input1 != null)
            {
                if (settings.UpdateOnlyAtBothInputsChanged && (!input1Updated || !input2Updated))
                {
                    return;
                }
                input1Updated = false;
                input2Updated = false;

                ProgressChanged(0.5, 1.0);

                try
                {
                    //As the user changes the operation different outputs are calculated.
                    switch (settings.Operat)
                    {
                        case NumberOperation.Addition:
                            result = Input1 + Input2;
                            break;
                        case NumberOperation.Subtraction:
                            result = Input1 - Input2;
                            break;
                        case NumberOperation.Multiplication:
                            result = Input1 * Input2;
                            break;
                        case NumberOperation.Division:
                            result = Input1 / Input2;
                            break;
                        case NumberOperation.Power:
                            if (Mod != 0)
                            {
                                if (Input2 >= 0)
                                {
                                    result = BigInteger.ModPow(Input1, Input2, Mod);
                                }
                                else
                                {
                                    result = BigInteger.ModPow(BigIntegerHelper.ModInverse(Input1, Mod), -Input2, Mod);
                                }
                            }
                            else
                            {
                                result = BigIntegerHelper.Pow(Input1, Input2);
                            }
                            break;
                        case NumberOperation.GCD:
                            result = Input1.GCD(Input2);
                            break;
                        case NumberOperation.LCM:
                            result = Input1.LCM(Input2);
                            break;
                        case NumberOperation.SQRT:
                            result = Input1.Sqrt();
                            break;
                        case NumberOperation.MODINV:
                            if (Input2 != 0)
                            {
                                result = BigIntegerHelper.ModInverse(Input1, Input2);
                            }
                            else
                            {
                                result = 1 / Input1;
                            }
                            break;
                        case NumberOperation.Phi:
                            result = Input1.Phi();
                            break;
                        case NumberOperation.Divsum:
                            BigInteger r = 0;
                            foreach (BigInteger n in Input1.Divisors())
                            {
                                r += n;
                            }
                            result = r;
                            break;
                        case NumberOperation.Divnum:
                            result = Input1.Divisors().Count;
                            break;
                        case NumberOperation.Pi:
                            BigInteger pi = 0;
                            for (BigInteger b = 2; b <= Input1; b++)
                            {
                                if (b.IsProbablePrime())
                                {
                                    pi++;
                                }
                                if (!_running)
                                {
                                    return;
                                }
                            }
                            result = pi;
                            break;
                        case NumberOperation.PrimeN:
                            BigInteger p = 0;
                            BigInteger i = 2;
                            while (Input1 <= 0 && _running)
                            {
                                if (i.IsProbablePrime())
                                {
                                    p++;
                                    if (p == Input1)
                                    {
                                        result = i;
                                        break;
                                    }
                                }
                                i++;
                            }
                            break;
                        case NumberOperation.Nextprime:
                            result = Input1.NextProbablePrime();
                            break;
                        case NumberOperation.Prevprime:
                            result = Input1.PreviousProbablePrime();
                            break;
                        case NumberOperation.Isprime:
                            result = (Input1.IsProbablePrime() ? 1 : 0);
                            break;
                        case NumberOperation.ABS:
                            result = (Input1 > 0 ? Input1 : Input1 * -1);
                            break;
                        case NumberOperation.Factorial:
                            result = Input1.Factorial();
                            break;
                        case NumberOperation.Crosssum:
                            result = Input1.CrossSum(Input2);
                            break;
                        case NumberOperation.Dlog:
                            result = Input1.DiscreteLogarithm(Input2, Mod);
                            break;
                        case NumberOperation.NPR:
                            result = Input1.nPr(Input2);
                            break;
                        case NumberOperation.NCR:
                            result = Input1.nCr(Input2);
                            break;
                    }
                    Output = (Mod == 0) ? result : (((result % Mod) + Mod) % Mod);
                }
                catch (Exception e)
                {
                    GuiLogMessage("Big Number fail: " + e.Message, NotificationLevel.Error);
                    return;
                }
                finally
                {
                    _running = false;
                }

                ProgressChanged(1.0, 1.0);
            }
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
            _running = false;
        }

        public void Initialize()
        {
            //change to the correct icon which belongs to actual selected arithmetic function 
            settings.changeToCorrectIcon(settings.Operat);
        }

        public void Dispose()
        {
        }

        #endregion

        #region private        

        private void OnPropertyChanged(string p)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(p));
        }

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        private void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(this, args);
            }
        }

        #endregion
    }
}
