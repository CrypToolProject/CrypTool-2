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
using System.Runtime.CompilerServices;

namespace CrypTool.Plugins.Comparators
{
    [Author("Raoul Falk", "falk@CrypTool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Comparators.Properties.Resources", "PluginCaption", "PluginTooltip", "Comparators/DetailedDescription/doc.xml", "Comparators/icons/icon_is.png", "Comparators/icons/icon_isnot.png", "Comparators/icons/icon_smaller.png", "Comparators/icons/icon_bigger.png", "Comparators/icons/icon_smallerIs.png", "Comparators/icons/icon_biggerIs.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    internal class Comparators : ICrypComponent
    {
        #region private variables

        private ComparatorsSettings settings = new ComparatorsSettings();
        private IComparable inputOne;
        private IComparable inputTwo;
        private bool output;

        #endregion

        #region public interfaces

        public Comparators()
        {
            settings = new ComparatorsSettings();
            settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
        }

        public ISettings Settings
        {
            get => settings;
            set => settings = (ComparatorsSettings)value;
        }

        private void Comparators_LogMessage(string msg, NotificationLevel loglevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(msg, this, loglevel));
        }

        public System.Windows.Controls.UserControl Presentation => null;

        [PropertyInfo(Direction.InputData, "InputOneCaption", "InputOneTooltip", true)]
        public IComparable InputOne
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => inputOne;
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (value != inputOne)
                {
                    inputOne = value;
                    OnPropertyChanged("InputOne");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "InputTwoCaption", "InputTwoTooltip", true)]
        public IComparable InputTwo
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => inputTwo;
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (value != inputTwo)
                {
                    inputTwo = value;
                    OnPropertyChanged("InputTwo");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip", true)]
        public bool Output
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => output;

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                output = value;
                OnPropertyChanged("Output");
            }
        }

        #endregion

        #region IPlugin members

        public void Dispose()
        {

        }

        public void Execute()
        {
            if (InputOne != null && InputTwo != null)
            {
                //int and BigInt may be compared, but first the int has to be converted to big int:
                if ((InputOne.GetType().FullName == "System.Int32" ||
                     InputOne.GetType().FullName == "System.Int64") &&
                     InputTwo.GetType().FullName == "System.Numerics.BigInteger")
                {
                    InputOne = new BigInteger((int)InputOne);
                }
                if ((InputTwo.GetType().FullName == "System.Int32" ||
                     InputTwo.GetType().FullName == "System.Int64") &&
                     InputOne.GetType().FullName == "System.Numerics.BigInteger")
                {
                    InputTwo = new BigInteger((int)InputTwo);
                }

                try
                {
                    switch (settings.Comparator)
                    {
                        // if operator is =
                        case 0:
                            {
                                Output = InputOne.CompareTo(InputTwo) == 0;
                                ProgressChanged(100, 100);
                                break;
                            }
                        // if operator is !=
                        case 1:
                            {
                                Output = InputOne.CompareTo(InputTwo) != 0;
                                ProgressChanged(100, 100);
                                break;
                            }
                        //if operator is <
                        case 2:
                            {
                                Output = InputOne.CompareTo(InputTwo) < 0;
                                ProgressChanged(100, 100);
                                break;

                            }
                        //if operator is >
                        case 3:
                            {

                                Output = InputOne.CompareTo(InputTwo) > 0;
                                ProgressChanged(100, 100);
                                break;
                            }
                        //if operator is <=
                        case 4:
                            {

                                Output = InputOne.CompareTo(InputTwo) <= 0;
                                ProgressChanged(100, 100);
                                break;

                            }
                        //if operator is >=
                        case 5:
                            {
                                Output = InputOne.CompareTo(InputTwo) >= 0;
                                ProgressChanged(100, 100);
                                break;
                            }
                    }
                }
                catch (Exception e)
                {
                    GuiLogMessage("The given Inputs are not comparable: " + e.Message, NotificationLevel.Error);
                }

            }
        }

        public void Initialize()
        {
            settings.ChangePluginIcon(settings.Comparator);
        }

        public void PostExecution()
        {

        }

        public void PreExecution()
        {

        }

        public void Stop()
        {

        }

        #endregion

        #region INotifyPropertyChanged Member

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region event handling

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        private void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(this, args);
            }
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        #endregion
    }
}
