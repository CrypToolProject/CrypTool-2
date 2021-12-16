/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using CrypTool.Plugins.CramerShoup.lib;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Security;
using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace CrypTool.Plugins.CramerShoup
{
    [Author("Jan Jansen", "jan.jansen-n22@rub.de", "Ruhr Uni-Bochum", "http://cits.rub.de/")]
    [PluginInfo("CramerShoup.Properties.Resources", "PluginCaption", "PluginTooltip", "CramerShoup/DetailedDescription/doc.xml", new[] { "CramerShoup/Images/csencaps.png", "CramerShoup/Images/csdecaps.png" })]
    [ComponentCategory(ComponentCategory.CiphersModernAsymmetric)]
    public class CramerShoup : ICrypComponent
    {
        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly CramerShoupSettings settings = new CramerShoupSettings();

        #endregion

        public CramerShoup()
        {

            settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
        }

        private void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(this, args);
            }
        }

        #region Data Properties

        /// <summary>
        /// HOWTO: Input interface to read the input data.
        /// You can add more input properties of other type if needed.
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputPublCaption", "InputPublTooltip")]
        public ECCramerShoupParameter Parameter
        {
            get;
            set;
        }

        /// <summary>
        /// HOWTO: Output interface to write the output data.
        /// You can add more output properties ot other type if needed.
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputCiphertextCaption", "InputCiphertextTooltip")]
        public ECCramerShoupCipherText InputCiphertext
        {
            get;
            set;
        }

        /// <summary>
        /// HOWTO: Output interface to write the output data.
        /// You can add more output properties ot other type if needed.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputCiphertextCaption", "OutputCiphertextTooltip")]
        public ECCramerShoupCipherText OutputCiphertext
        {
            get;
            set;
        }

        /// <summary>
        /// HOWTO: Output interface to write the output data.
        /// You can add more output properties ot other type if needed.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputKeyCaption", "OutputKeyTooltip")]
        public byte[] Key
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
        public UserControl Presentation => null;

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
            ProgressChanged(0, 1);
            SecureRandom random = new SecureRandom();
            ECCramerShoupEngine engine = new ECCramerShoupEngine();
            IDigest digest = null;
            switch (settings.KeySize)
            {
                case 0:
                    digest = new RipeMD128Digest();
                    break;
                case 1:
                    digest = new Sha256Digest();
                    break;
                case 2:
                    digest = new Sha512Digest();
                    break;

            }
            if (settings.Action == 0)
            {
                ECCramerShoupPublicParameter parameter = Parameter as ECCramerShoupPublicParameter;
                if (parameter != null)
                {
                    ProgressChanged(0.33, 1);
                    Tuple<ECCramerShoupCipherText, byte[]> output = engine.Encaps(parameter, random, digest);

                    ProgressChanged(0.66, 1);
                    OutputCiphertext = output.Item1;

                    Key = output.Item2;

                    OnPropertyChanged("OutputCiphertext");
                    OnPropertyChanged("Key");
                    // HOWTO: Make sure the progress bar is at maximum when your Execute() finished successfully.
                    ProgressChanged(1, 1);
                }
                else
                {
                    throw new Exception("Empty or Wrong Parameter!");
                }
            }
            else
            {
                ECCramerShoupPrivateParameter parameter = Parameter as ECCramerShoupPrivateParameter;
                if (parameter != null && InputCiphertext != null)
                {
                    ProgressChanged(0.33, 1);
                    Key = engine.Decaps(parameter, InputCiphertext, digest);

                    ProgressChanged(0.66, 1);
                    OnPropertyChanged("Key");
                    // HOWTO: Make sure the progress bar is at maximum when your Execute() finished successfully.
                    ProgressChanged(1, 1);
                }
                else
                {
                    throw new Exception("Empty or Wrong Parameter!");
                }
            }
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

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
