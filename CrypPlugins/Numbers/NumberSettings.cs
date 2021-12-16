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
using System.ComponentModel;

namespace CrypTool.Plugins.Numbers
{
    public enum NumberOperation
    {
        Addition,
        Subtraction,
        Multiplication,
        Division,
        Power,
        GCD,
        LCM,
        SQRT,
        MODINV,
        Phi,
        Divsum,
        Divnum,
        Pi,
        PrimeN,
        Nextprime,
        Prevprime,
        Isprime,
        ABS,
        Factorial,
        Crosssum,
        Dlog,
        NPR,
        NCR
    }

    internal class NumberSettings : ISettings
    {
        #region private variables

        private NumberOperation operat;
        private bool updateOnlyAtBothInputsChanged = false;

        #endregion

        #region taskpane

        [TaskPane("OperatCaption", "OperatTooltip", null, 1, false, ControlType.ComboBox, new string[] {
            "OperatList1",
            "OperatList2",
            "OperatList3",
            "OperatList4",
            "OperatList5",
            "OperatList6",
            "OperatList7",
            "OperatList8",
            "OperatList9",
            "OperatList10",
            "Divsum",
            "Divnum",
            "Pi",
            "Prime",
            "Nextprime",
            "Prevprime",
            "Isprime",
            "ABS",
            "Factorial",
            "Crosssum",
            "Dlog",
            "nPr",
            "nCr"})]
        public NumberOperation Operat
        {
            get => operat;
            set
            {
                if (value != operat)
                {
                    operat = value;
                    OnPropertyChanged("Operat");

                    changeToCorrectIcon(operat);
                }
            }
        }

        [TaskPane("UpdateOnlyAtBothInputsChangedCaption", "UpdateOnlyAtBothInputsChangedTooltip", null, 2, false, ControlType.CheckBox, "", null)]
        public bool UpdateOnlyAtBothInputsChanged
        {
            get => updateOnlyAtBothInputsChanged;
            set
            {
                if (value != updateOnlyAtBothInputsChanged)
                {
                    updateOnlyAtBothInputsChanged = value;
                    OnPropertyChanged("UpdateOnlyAtBothInputsChanged");
                }
            }
        }

        /// <summary>
        /// Changes the plugins icon to the icon fitting to actual selected arithmetic function
        /// </summary>
        /// <param name="operat"></param>
        public void changeToCorrectIcon(NumberOperation operat)
        {
            switch (operat)
            {
                case NumberOperation.Addition:
                    ChangePluginIcon(0);
                    break;
                case NumberOperation.Subtraction:
                    ChangePluginIcon(1);
                    break;
                case NumberOperation.Multiplication:
                    ChangePluginIcon(2);
                    break;
                case NumberOperation.Division:
                    ChangePluginIcon(3);
                    break;
                case NumberOperation.Power:
                    ChangePluginIcon(4);
                    break;
                case NumberOperation.GCD:
                    ChangePluginIcon(5);
                    break;
                // lcm(x,y)
                case NumberOperation.LCM:
                    ChangePluginIcon(6);
                    break;
                case NumberOperation.SQRT:
                    ChangePluginIcon(7);
                    break;
                case NumberOperation.MODINV:
                    ChangePluginIcon(8);
                    break;
                case NumberOperation.Phi:
                    ChangePluginIcon(9);
                    break;
                case NumberOperation.Divsum:
                    ChangePluginIcon(10);
                    break;
                case NumberOperation.Divnum:
                    ChangePluginIcon(11);
                    break;
                case NumberOperation.Pi:
                    ChangePluginIcon(12);
                    break;
                case NumberOperation.PrimeN:
                    ChangePluginIcon(13);
                    break;
                case NumberOperation.Nextprime:
                    ChangePluginIcon(14);
                    break;
                case NumberOperation.Prevprime:
                    ChangePluginIcon(15);
                    break;
                case NumberOperation.Isprime:
                    ChangePluginIcon(16);
                    break;
                case NumberOperation.ABS:
                    ChangePluginIcon(17);
                    break;
                case NumberOperation.Factorial:
                    ChangePluginIcon(18);
                    break;
                case NumberOperation.Crosssum:
                    ChangePluginIcon(19);
                    break;
                case NumberOperation.Dlog:
                    ChangePluginIcon(20);
                    break;
                case NumberOperation.NPR:
                    ChangePluginIcon(21);
                    break;
                case NumberOperation.NCR:
                    ChangePluginIcon(22);
                    break;
            }
        }

        private void ChangePluginIcon(int p)
        {
            OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, p));
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        #endregion
    }
}
