/*
   Copyright CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.Plugins.RAPPOR
{

    /// <summary>
    /// This class contains all releveant settings of the rappor component
    /// </summary>
    public class RAPPORSettings : ISettings
    {
        #region Private Variables
        //User tunable parameters of the bloom filter.
        /// <summary>
        /// Size of the Bloom filter boolean array.
        /// </summary>
        private int sizeOfBloomFilter = 128;
        /// <summary>
        /// Amount of hash functions which are beeing used in the component.
        /// </summary>
        private int amountOfHashFunctions = 2;
        //User tunable parameter of the permanent randomized response.
        /// <summary>
        /// User tunable parameter f which is used for the permanent randomized response.
        /// </summary>
        private int f = 50;
        //User tunable parameters of the instantaneous randomized response.
        /// <summary>
        /// User tunable parameter q which is used for the instantaneous randomized response.
        /// </summary>
        private int q = 75;
        /// <summary>
        /// User tunable parameter p which is used for the instantaneous randomized response.
        /// </summary>
        private int p = 50;
        /// <summary>
        /// Amount of Instantaneous randomized responses which are beeing used.
        /// </summary>
        private int amountOfIRR = 8;
        private int iterations = 100;
        private readonly RAPPOR rappor;

        /// <summary>
        /// We use this delegate to send log messages from the settings class to the Caesar plugin
        /// </summary>
        public delegate void RAPPORLogMessage(string msg, NotificationLevel loglevel);
        /// <summary>
        /// Feuern, wenn ein neuer Text im Statusbar angezeigt werden soll.
        /// </summary>
        public event RAPPORLogMessage LogMessage;
        /// <summary>
        /// This method is used to log messages for rappor
        /// </summary>
        /// <param name="msg">The message which is to be logged</param>
        /// <param name="level">The level at which the message is to be logged</param>
        public void OnLogMessage(string msg, NotificationLevel level)
        {
            if (LogMessage != null)
            {
                LogMessage(msg, level);
            }
        }
        #endregion
        /// <summary>
        /// Initializes the rappor settings with the current rappor instance
        /// </summary>
        /// <param name="rAPPOR"></param>
        public RAPPORSettings(RAPPOR rAPPOR)
        {
            rappor = rAPPOR;
        }

        #region TaskPane Settings

        /// <summary>
        /// Public getter and setter for the size of the bloom filter.
        /// </summary>
        [TaskPane("BloomFilterSizeCaption", "BloomFilterSizeToolTip", "General", 1, true, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
        public int SizeOfBloomFilter
        {
            get => sizeOfBloomFilter;
            set
            {
                if (value < 0)
                {
                    OnLogMessage("Negative Values are not possible for the size of the Bloom filter, changing the value to 0.", NotificationLevel.Info);
                    value = 0;
                }
                else if (value > 256)
                {
                    OnLogMessage("Values higher than 256 are not possible for the size of the Bloom filter, changing the value to 256.", NotificationLevel.Info);
                    value = 256;
                }
                if (sizeOfBloomFilter != value)
                {
                    sizeOfBloomFilter = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    OnPropertyChanged("sizeOfBloomFilter");
                    UpdateCurrentView();
                }
            }
        }
        /// <summary>
        /// Public getter and setter for the amount of hash functions.
        /// </summary>
        [TaskPane("AmountOfHashFunctionCaption", "AmountOfHashFunctionToolTip", "General", 1, true, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
        public int AmountOfHashFunctions
        {
            get => amountOfHashFunctions;
            set
            {
                if (value < 0)
                {
                    OnLogMessage("Negative Values are not possible for the amount of hash functions, changing the value to 0.", NotificationLevel.Info);
                    value = 0;
                }
                else if (value > 64)
                {
                    OnLogMessage("Values higher than 64 are not possible for the amount of hash functions, changing the value to 64.", NotificationLevel.Info);
                    value = 64;
                }
                if (amountOfHashFunctions != value)
                {
                    amountOfHashFunctions = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    OnPropertyChanged("amountOfHashFunctions");
                    UpdateCurrentView();
                }
            }
        }
        /// <summary>
        /// Public getter and setter for the size of the f percentage. The setter ensures that only
        /// Values between 0 nd100 can be entered.
        /// </summary>
        [TaskPane("FPercentageCaption", "FPercentageToolTip", "General", 1, true, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int f_percentage
        {
            get => f;

            set
            {
                if (value < 0)
                {
                    OnLogMessage("Negative Values are not possible for the f percentage, changing the value to 0.", NotificationLevel.Info);
                    value = 0;
                }
                else if (value > 100)
                {
                    OnLogMessage("Values higher than 100 are not possible for the f percentage, changing the value to 100.", NotificationLevel.Info);
                    value = 100;
                }
                if (f != value)
                {
                    f = value;
                    OnPropertyChanged("f");
                    UpdateCurrentView();
                }
            }
        }
        /// <summary>
        /// Public getter and setter for the size of the q percentage. The setter ensures that only
        /// Values between 0 nd100 can be entered.
        /// </summary>
        [TaskPane("QPercentageCaption", "QPercentageToolTip", "General", 1, true, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int q_Percentage
        {
            get => q;
            set
            {
                if (value < 0)
                {
                    OnLogMessage("Negative values are not possible for the q percentage, changing the value to 0.", NotificationLevel.Info);
                    value = 0;
                }
                else if (value > 100)
                {
                    OnLogMessage("Values higher than 100 are not possible for the q percentage, changing the value to 100.", NotificationLevel.Info);
                    value = 100;
                }
                if (q != value)
                {
                    q = value;
                    OnPropertyChanged("q");
                    UpdateCurrentView();
                }
            }
        }
        /// <summary>
        /// Public getter and setter for the size of the p percentage. The setter ensures that only
        /// Values between 0 nd100 can be entered.
        /// </summary>
        [TaskPane("PPercentageCaption", "PPercentageToolTip", "General", 1, true, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int p_Percentage
        {
            get => p;
            set
            {

                if (value < 0)
                {
                    OnLogMessage("Negative Values are not possible for the p percentage, changing the value to 0.", NotificationLevel.Info);
                    value = 0;
                }
                else if (value > 100)
                {
                    OnLogMessage("Values higher than 100 are not possible for the p percentage, changing the value to 100.", NotificationLevel.Info);
                    value = 100;
                }
                if (p != value)
                {
                    p = value;
                    OnPropertyChanged("p");
                    UpdateCurrentView();
                }
            }
        }
        /// <summary>
        /// Public getter and setter for the size of the amount of the instantaenous randomized
        /// responses The setter ensures that only Values over 0 can be entered.
        /// </summary>
        [TaskPane("AmountOfInstantaneousRandomizedResponsesCaption", "AmountOfInstantaneousRandomizedResponsesToolTip", "Overview", 1, true, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int AmountOfIRR
        {
            get => amountOfIRR;

            set
            {
                if (value < 0)
                {
                    OnLogMessage("Negative Values are not possible for the the amount of instantaneous randomized responses, changing the value to 0.", NotificationLevel.Info);
                    value = 0;
                }
                if (value > 128)
                {
                    OnLogMessage("Values higher than 128 are not possible for the amount of instantaneous randomized responses, changing the value to 128.", NotificationLevel.Info);
                    value = 128;
                }
                if (amountOfIRR != value)
                {
                    amountOfIRR = value;
                    OnPropertyChanged("amountOfIRR");
                    UpdateCurrentView();
                }
            }
        }
        /// <summary>
        /// Public getter and setter for the amount of Iterations.
        /// </summary>
        [TaskPane("IterationsCaption", "IterationToolTip", "HeatMaps", 1, true, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
        public int Iterations
        {
            get => iterations;
            set
            {
                if (value < 0)
                {
                    OnLogMessage("Negative values are not possible for the amount of iterations, changing the value to 0.", NotificationLevel.Info);
                    value = 0;
                }
                else if (value > 1024)
                {
                    OnLogMessage("Values higher than 1024 are not possible for the amount of iterations, changing the value to 1024.", NotificationLevel.Info);
                    value = 1024;
                }
                if (iterations != value)
                {
                    iterations = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    OnPropertyChanged("iterations");
                    UpdateCurrentView();
                }
                rappor.GetRAPPORPresentation().GetRapporPresentationViewModel().GetViewArray()[4].CreateHeatMapViewText(value);
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
        /// <summary>
        /// Initializes the rappor setting class
        /// </summary>
        public void Initialize()
        {

        }
        /// <summary>
        /// This method is used to update the current view which is being used. The selection of
        /// the user decides which view is being displayed in the component. 
        /// </summary>
        public void UpdateCurrentView()
        {
            switch (rappor.GetRAPPORPresentation().GetRapporPresentationViewModel().GetSelectedViewInteger())
            {
                case 0:
                    break;
                case 1:
                    if (rappor.GetRAPPORPresentation() != null)
                    {
                        rappor.Presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate//Changed .Invoke to .BeginInvoke in all approaches
                        {
                            rappor.GetRAPPORPresentation().GetRapporPresentationViewModel().GetViewArray()[1].DrawCanvas();
                        }, null);
                    }
                    break;
                case 2:
                    {
                        if (rappor.GetRAPPORPresentation() != null)
                        {
                            rappor.Presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            rappor.GetRAPPORPresentation().GetRapporPresentationViewModel().GetViewArray()[2].DrawCanvas();
                        }, null);
                        }
                    }
                    break;
                case 3:
                    if (rappor.GetRAPPORPresentation() != null)
                    {
                        rappor.Presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        rappor.GetRAPPORPresentation().GetRapporPresentationViewModel().GetViewArray()[3].DrawCanvas();
                    }, null);
                    }
                    break;
                case 4:
                    if (rappor.GetRAPPORPresentation() != null)
                    {
                        rappor.Presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            rappor.GetRAPPORPresentation().GetRapporPresentationViewModel().GetViewArray()[4].DrawCanvas();
                        }, null);
                    }

                    break;
                default:
                    //Throw exception here
                    break;
            }

        }

        #region Getter and Setter
        /// <summary>
        /// This method is used to create a string, containgin all rapoor setting informations.
        /// This is used for debuggin purposes.
        /// </summary>
        /// <returns>All information about the rappor settings as a string.</returns>
        public string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("");
            stringBuilder.Append("\r\n\r\nDEBUG\r\nAmount of Hashfunctions: ");
            stringBuilder.Append(GetAmountOfHashFunctions());
            stringBuilder.Append("\r\nAmount of Instantaneous randomized responses: ");
            stringBuilder.Append(GetAmountOfInstantaneousRandomizedResponses());
            stringBuilder.Append("\r\nF percentage: ");
            stringBuilder.Append(GetFVariable());
            stringBuilder.Append("\r\nQ percentage: ");
            stringBuilder.Append(GetQVariable());
            stringBuilder.Append("\r\nP percentage: ");
            stringBuilder.Append(GetPVariable());
            return stringBuilder.ToString();
        }

        //Getter and setter information for the rappor settings parameters.
        /// <summary>
        /// Sets the amount of instantaneous randomized responses in the component
        /// </summary>
        /// <param name="x">The new amount of instantaneous randomized responses</param>
        public void SetAmountOfInstantaneousRandomizedResponses(string x)
        {
            int y = -1;
            if (int.TryParse(x, out y))
            {
                amountOfIRR = y;
            }
            else
            {
                amountOfIRR = y;
            }
        }
        /// <summary>
        /// Sets the amount of f variables
        /// </summary>
        /// <param name="x">The amount of f variables which it will be set to</param>
        public void SetFVariable(string x)
        {
            int y = -1;
            if (int.TryParse(x, out y))
            {
                f = y;
            }
            else
            {
                f = y;
            }
        }
        /// <summary>
        /// Sets the amount of q variables
        /// </summary>
        /// <param name="x">The amount of q variables which it will be set to</param>
        public void SetQVariable(string x)
        {
            int y = -1;
            if (int.TryParse(x, out y))
            {
                q = y;
            }
            else
            {
                q = y;
            }
        }
        /// <summary>
        /// Sets the amount of p variables
        /// </summary>
        /// <param name="x">The amount of p variables which it will be set to</param>
        public void SetPVariable(string x)
        {
            int y = -1;
            if (int.TryParse(x, out y))
            {
                p = y;
            }
            else
            {
                p = y;
            }
        }
        /// <summary>
        /// Sets the size of the bloom filter
        /// </summary>
        /// <param name="x">The size which it will be set too</param>
        public void SetSizeOfBloomFilter(string x)
        {
            int y = -1;
            if (int.TryParse(x, out y))
            {
                sizeOfBloomFilter = y;
            }
            else
            {
                sizeOfBloomFilter = y;
            }
        }
        /// <summary>
        /// Sets the size of the bloom filter
        /// </summary>
        /// <param name="x">The size which the size of the bloom filter will be set to</param>
        public void SetSizeOfBloomFilter(int x)
        {
            sizeOfBloomFilter = x;
        }


        /// <summary>
        /// Sets the amount fo hash functions
        /// </summary>
        /// <param name="x">The amount of hash functions which it will be set to</param>
        public void SetAmountOfHashFunctions(int x)
        {
            amountOfHashFunctions = x;
        }

        /// <summary>
        /// Sets the amount of iterations
        /// </summary>
        /// <param name="x">The amount of iterations which will be set</param>
        public void SetIterations(int x)
        {
            iterations = x;
        }
        /// <summary>
        /// Gets the current amount of iterations
        /// </summary>
        /// <returns>The current amount of iterations</returns>
        public int GetIterations()
        {
            return iterations;
        }
        /// <summary>
        /// Sets the amount of hash functions
        /// </summary>
        /// <param name="x">Sets the amount of hash functions</param>
        public void SetAmountOfHashFunctions(string x)
        {
            int y = -1;
            if (int.TryParse(x, out y))
            {
                amountOfHashFunctions = y;
            }
            else
            {
                amountOfHashFunctions = y;
            }
        }
        /// <summary>
        /// Gets the amount of hash functions
        /// </summary>
        /// <returns>The amount of hash functions as an int</returns>
        public int GetAmountOfHashFunctions()
        {
            return amountOfHashFunctions;
        }
        /// <summary>
        /// Gets the amount of instantaneous randomized response
        /// </summary>
        /// <returns>The amount of instantaneous randomized responses as an int</returns>
        public int GetAmountOfInstantaneousRandomizedResponses()
        {
            return amountOfIRR;
        }
        /// <summary>
        /// Gets the size of the bloom filter
        /// </summary>
        /// <returns>The size of the bloom filter as an int</returns>
        public int GetSizeOfBloomFilter()
        {
            return SizeOfBloomFilter;
        }
        /// <summary>
        /// Gets the size of the f variable
        /// </summary>
        /// <returns>The f variable as an int</returns>
        public int GetFVariable()
        {
            return f;
        }
        /// <summary>
        /// Gets the size of the q variable
        /// </summary>
        /// <returns>The size of the q varibale as an int</returns>
        public int GetQVariable()
        {
            return q;
        }

        /// <summary>
        /// Gets the size of the p variable
        /// </summary>
        /// <returns>The p variable as an int</returns>
        public int GetPVariable()
        {
            return p;
        }

        #endregion
    }
}
