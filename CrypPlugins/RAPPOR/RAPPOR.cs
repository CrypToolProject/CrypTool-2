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

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Controls;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.Plugins.RAPPOR.View;
using RAPPOR.Helper;
using RAPPOR.Model;
using BloomFilter = RAPPOR.Model.BloomFilter;

namespace CrypTool.Plugins.RAPPOR
{
    //Removed Cryptool.Plugins.
    /// <summary>
    /// The rappor class is the main class of the rappor component. It unites the model, viewmodel 
    /// and view classes of the component.
    /// </summary>
    [Author("Thomas Maria Frey", "thfrey@mail.uni-mannheim.de", "Universität Mannheim", "https://www.uni-mannheim.de/")]
    [PluginInfo("CrypTool.Plugins.RAPPOR.Properties.Resources", "RAPPORCaption", "RAPPORToolTip", "RAPPOR/userdoc.xml", new[] { "RAPPOR/Images/R.png" })] //TODO: Input a user documentation and a icon here.
    [ComponentCategory(ComponentCategory.Protocols)]
    public class RAPPOR : ICrypComponent, INotifyPropertyChanged
    {
        #region Private Variables
        /// <summary>
        /// Boolean value representing if the component is running or not. Running hereby means that the play button is clicked, not running means that the stop button is clicked
        /// or that no button is clicked.
        /// </summary>
        private Boolean running = false;


        /// <summary>
        /// Instance of the Bloom filter.
        /// </summary>
        private BloomFilter bloomFilter;

        /// <summary>
        /// Instance of the permanent randomized response.
        /// </summary>
        private PermanentRandomizedResponse permanentRandomizedResponse;
        /// <summary>
        /// Instance array of the instantaneours randomized response
        /// </summary>
        private InstantaneousRandomizedResponse[] instantaneousRandomizedResponse;
        /// <summary>
        /// Instance of the rappor settings.
        /// </summary>
        private readonly RAPPORSettings settings;
        /// <summary>
        /// Instance of the rappor presentation.
        /// </summary>
        private readonly RAPPORPresentation presentation;
        /// <summary>
        /// Variable for internal debugging.
        /// </summary>
        private readonly bool debug = false;
        /// <summary>
        /// MathFunctions helper class.
        /// </summary>
        private readonly MathFunctions mathFunctions;
        public Random r;

        #endregion
        /// <summary>
        /// Initializes the rappor class. Setting the input, output, settings and presentaiton.
        /// </summary>
        public RAPPOR()
        {
            mathFunctions = new MathFunctions();
            Input = string.Empty;
            Output = string.Empty;
            r = new Random();
            settings = new RAPPORSettings(this);
            presentation = new RAPPORPresentation(this);
            settings.LogMessage += GuiLogMessage;
            IsActionPossible = true;
        }

        #region Data Properties
        /// <summary>
        /// Input string for the component.
        /// </summary>
        private string _Input;
        /// <summary>
        /// Getter and setter method for the Input of the component.
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputCaption", "InputToolTip")]
        public string Input
        {
            get
            {
                if (string.IsNullOrEmpty(_Input))
                {
                    return string.Empty;
                }

                return _Input;
            }
            set
            {
                _Input = value;
                OnPropertyChanged("Input");
                if (GetRAPPORSettings() != null)
                {
                    GetRAPPORSettings().UpdateCurrentView();
                }
            }
        }
        /// <summary>
        /// Getter and setter for the output of the component.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputToolTip")]
        public string Output
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

        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            SetRunning(true);
            ProgressChanged(0, 1);

            RunRappor();

            ProgressChanged(1, 1);
        }
        /// <summary>
        /// This method is executed whenever an internal parameter changes. When it executes all 
        /// cascading dependables are also updated. The order is hereby
        /// Bloom Filter -> Permanent randomized response -> Instantaneous randomized response.
        /// </summary>
        public void RunRappor()
        {
            if (Input.Length > 4096)
            {
                GuiLogMessage("The size of the chosen input is larger than 4096, this can negative impact the performance of the RAPPOR plugin.", NotificationLevel.Warning);
            }

            ProgressChanged(0, 1);
            StringBuilder stringBuilderOutput = new StringBuilder(string.Empty);


            //Creating the Bloom

            bloomFilter = new BloomFilter(Input.Split(','), settings.SizeOfBloomFilter, settings.AmountOfHashFunctions, GetRunning());
            ProgressChanged(1, 3);

            stringBuilderOutput.Append(bloomFilter.ToString());
            stringBuilderOutput.AppendLine(String.Empty);
            stringBuilderOutput.AppendLine(String.Empty);

            //Creating the permanent randomized response
            permanentRandomizedResponse = new PermanentRandomizedResponse(bloomFilter.GetBoolArray(), r, settings.SizeOfBloomFilter, settings.f_percentage);
            ProgressChanged(2, 3);

            stringBuilderOutput.Append(permanentRandomizedResponse.ToString());
            stringBuilderOutput.AppendLine(String.Empty);
            stringBuilderOutput.AppendLine(String.Empty);

            //Creating the instantaneous randomized responses
            instantaneousRandomizedResponse = new InstantaneousRandomizedResponse[settings.AmountOfIRR];

            for (int i = 0; i < settings.AmountOfIRR; i++)
            {
                instantaneousRandomizedResponse[i] = new InstantaneousRandomizedResponse(permanentRandomizedResponse.GetBoolArray(), r, settings.SizeOfBloomFilter, settings.p_Percentage, settings.q_Percentage);
                ProgressChanged(2 + i, 3 + i);
            }

            for (int i = 0; i < settings.AmountOfIRR; i++)
            {
                if (instantaneousRandomizedResponse[i] != null)
                {
                    stringBuilderOutput.Append(instantaneousRandomizedResponse[i].ToString());
                    stringBuilderOutput.AppendLine(String.Empty);
                }

            }
            ProgressChanged(1, 1);


            if (debug)
            {
                string s = Input.Split(',')[0];

                // byte array representation of that string
                byte[] sArray = new UTF8Encoding().GetBytes(s);

                // need MD5 to calculate the hash
                byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(sArray);

                // string representation (similar to UNIX format)
                string x = BitConverter.ToString(hash)
                   // without dashes
                   .Replace("-", string.Empty)
                   // make lowercase
                   .ToLower();
                stringBuilderOutput.Append("Sha1 Convertion: " + s + " -Sha1[0]> " + x + "-ToDec>" + mathFunctions.hexToDec(x) + "-ToBloom>" + (mathFunctions.hexToDec(x) % GetRAPPORSettings().SizeOfBloomFilter));
                //Debug classic
                stringBuilderOutput.Append(GetRAPPORSettings().ToString());
                stringBuilderOutput.AppendLine(String.Empty);
                stringBuilderOutput.AppendLine(String.Format("Input: {0}", Input.ToString()));
                stringBuilderOutput.AppendLine(String.Format("Output: {0}", Output.ToString()));
            }

            Output = stringBuilderOutput.ToString();
            OnPropertyChanged("Output");
            GuiLogMessage("RAPPOR executed successfully.", NotificationLevel.Debug);
        }

        /// <summary>
        /// This method is executed when only the IRR needs to be updated. It changes all relevant
        /// instantaneous randomized response arrays.
        /// </summary>
        public void RunRapporIRR()
        {
            ProgressChanged(0, 1);
            StringBuilder stringBuilderOutput = new StringBuilder(string.Empty);
            Random r = new Random();
            stringBuilderOutput.Append(bloomFilter.ToString());
            stringBuilderOutput.AppendLine(String.Empty);
            stringBuilderOutput.AppendLine(String.Empty);
            stringBuilderOutput.Append(permanentRandomizedResponse.ToString());
            stringBuilderOutput.AppendLine(String.Empty);
            stringBuilderOutput.AppendLine(String.Empty);
            instantaneousRandomizedResponse = new InstantaneousRandomizedResponse[settings.AmountOfIRR];

            for (int i = 0; i < settings.AmountOfIRR; i++)
            {
                instantaneousRandomizedResponse[i] = new InstantaneousRandomizedResponse(permanentRandomizedResponse.GetBoolArray(), r, settings.SizeOfBloomFilter, settings.p_Percentage, settings.q_Percentage);
                ProgressChanged(2 + i, 3 + i);
            }

            for (int i = 0; i < settings.AmountOfIRR; i++)
            {
                stringBuilderOutput.Append(instantaneousRandomizedResponse[i].ToString());
                stringBuilderOutput.AppendLine(String.Empty);
                stringBuilderOutput.AppendLine(String.Empty);
            }

            if (debug)
            {
                stringBuilderOutput.Append(GetRAPPORSettings().ToString());
                stringBuilderOutput.AppendLine(String.Format("Input: {0}", Input.ToString()));
                stringBuilderOutput.AppendLine(String.Format("Output: {0}", Output.ToString()));
            }

            Output = stringBuilderOutput.ToString();
            OnPropertyChanged("Output");
            GuiLogMessage("RAPPOR executed successfully.", NotificationLevel.Debug);
            ProgressChanged(1, 1);
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
            SetRunning(false);
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

        #endregion

        #region Event Handling
        /// <summary>
        /// Internal event variable for when the plugin status is changed.
        /// </summary>
        public event StatusChangedEventHandler OnPluginStatusChanged;

        /// <summary>
        /// Internal event variable for when the plguing progress is changed.
        /// </summary>
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        /// <summary>
        /// Internal evenet variable for when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Iternal event variable for when a gui log notificaton occured.
        /// </summary>
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        /// <summary>
        /// Internal method which is used to log gui messages for the user.
        /// </summary>
        /// <param name="message">The message which is supposed to be logged as a string</param>
        /// <param name="logLevel">The level of the message which is to be logged.</param>
        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        /// <summary>
        /// Internal method which is used to notify the internal logic whenever the property is changed.
        /// </summary>
        /// <param name="name">The name of the property which is changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        /// <summary>
        /// Internal method which is used to alculated the progress change of the component.
        /// </summary>
        /// <param name="value">Current status of the progress of the component.</param>
        /// <param name="max">Maximal status of the progress of the component.</param>
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion

        private bool _isActionPossible;
        public bool IsActionPossible
        {
            get { return _isActionPossible; }
            set
            {
                _isActionPossible = value;
                OnPropertyChanged("IsActionPossible");
            }
        }

        #region Getter and Setter methods
        //Contains getters and setters of all relevant parameters and instances of the rappor class
        //and component.
        /// <summary>
        /// Gets the current running instance.
        /// </summary>
        /// <returns>Gets the current running instance</returns>
        public Boolean GetRunning()
        {
            return running;
        }
        /// <summary>
        /// Sets the current running instance.
        /// </summary>
        /// <param name="r">the new running instance.</param>
        public void SetRunning(Boolean ru)
        {
            running = ru;
            GetBloomFilter().SetIsActionPossible(ru);
            SetIsActionPossible(ru);
            GetRAPPORPresentation().GetRapporPresentationViewModel().GetViewArray()[2].ChangeButton(ru);

        }
        /// <summary>
        /// Gets the current Bloomfilter instance.
        /// </summary>
        /// <returns>Gets the current bloom filter instance</returns>
        public BloomFilter GetBloomFilter()
        {
            return bloomFilter;
        }
        /// <summary>
        /// Sets the current bloom filter instance.
        /// </summary>
        /// <param name="bF">the new bloom filter instance.</param>
        public void SetBloomFilter(BloomFilter bF)
        {
            bloomFilter = bF;
        }
        public void SetIsActionPossible(Boolean ru)
        {
            IsActionPossible = ru;
        }
        /// <summary>
        /// gets the current permantent randomized response
        /// </summary>
        /// <returns>the current permanent randomized response</returns>
        public PermanentRandomizedResponse GetPermanentRandomizedResponse()
        {
            return permanentRandomizedResponse;
        }
        /// <summary>
        /// sets the current permanent randomized response to a new one
        /// </summary>
        /// <param name="pRR">The new permanent randomized response to be used</param>
        public void SetPermanentRandomizedResponse(PermanentRandomizedResponse pRR)
        {
            permanentRandomizedResponse = pRR;
        }
        /// <summary>
        /// Gets the current instantaneous randomized response
        /// </summary>
        /// <returns>Returns the current the instantaneous randomized response.</returns>
        public InstantaneousRandomizedResponse[] GetInstantaneousRandomizedResponse()
        {
            return instantaneousRandomizedResponse;
        }
        /// <summary>
        /// Sets the current instantaneous randomized response
        /// </summary>
        /// <param name="iRR">The new instantaneous randomized response to be set</param>
        public void SetInstantaneousRandomizedResponse(InstantaneousRandomizedResponse[] iRR)
        {
            instantaneousRandomizedResponse = iRR;
        }
        /// <summary>
        /// Gets the current RAPPOR Settings
        /// </summary>
        /// <returns>the current rappor settings.</returns>
        public RAPPORSettings GetRAPPORSettings()
        {
            return settings;
        }
        /// <summary>
        /// Gets the current rappor presentation
        /// </summary>
        /// <returns>The current rappor presentation</returns>
        public RAPPORPresentation GetRAPPORPresentation()
        {
            return presentation;
        }
        /// <summary>
        /// Gets the random object which is currently being used.
        /// </summary>
        /// <returns>The random object r which is currently being used.</returns>
        public Random GetRandom()
        {
            return r;
        }

        #endregion
    }
}
