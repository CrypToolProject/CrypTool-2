using System.ComponentModel;
using System.Windows.Controls;

using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

using Microsoft.Research.SEAL;

namespace CrypTool.Plugins.EncryptedVM
{
    [Author("Robert Stark", "robert.stark@rub.de", "", "")]
    [PluginInfo("CrypTool.Plugins.EncryptedVM.Properties.Resources", "EncryptedVM_Keygen_Name", "EncryptedVM_Keygen_Tooltip", "EncryptedVM/doc/keygen.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class EncryptedVMKeyGen : ICrypComponent
    {
        #region Private Variables

        private readonly EncryptedVMKeyGenSettings settings = new EncryptedVMKeyGenSettings();

        private EncryptionParameters parms = new EncryptionParameters();
        private BigPolyArray pubkey;
        private BigPoly seckey;
        private EvaluationKeys evalkeys;

        private KeyGenerator keygen;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.OutputData, "EncryptionParameters_Name", "EncryptionParameters_Tooltip")]
        public EncryptionParameters ParameterOutput
        {
            get
            {
                return parms;
            }
        }

        [PropertyInfo(Direction.OutputData, "PublicKey_Name", "PublicKey_Tooltip")]
        public BigPolyArray PubKeyOutput
        {
            get
            {
                return pubkey;
            }
        }

        [PropertyInfo(Direction.OutputData, "EvaluationKeys_Name", "EvaluationKeys_Tooltip")]
        public EvaluationKeys EvalKeysOutput
        {
            get
            {
                return evalkeys;
            }
        }

        [PropertyInfo(Direction.OutputData, "SecretKey_Name", "SecretKey_Tooltip")]
        public BigPoly SecKeyOutput
        {
            get
            {
                return seckey;
            }
        }
        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get { return null; }
        }

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
  
            switch (settings.PowerOfPolyModulus)
            {
                case 0:
                    parms.PolyModulus.Set("1x^1024 + 1");
                    parms.CoeffModulus.Set(ChooserEvaluator.DefaultParameterOptions[1024]);
                    parms.DecompositionBitCount = 24;
                    break;
                case 1:
                    parms.PolyModulus.Set("1x^2048 + 1");
                    parms.CoeffModulus.Set(ChooserEvaluator.DefaultParameterOptions[2048]);
                    parms.DecompositionBitCount = 48; // maybe not optimal
                    break;
                case 2:
                    parms.PolyModulus.Set("1x^4096 + 1");
                    parms.CoeffModulus.Set(ChooserEvaluator.DefaultParameterOptions[4096]);
                    parms.DecompositionBitCount = 96; // maybe not optimal
                    break;
                case 3:
                    parms.PolyModulus.Set("1x^8192 + 1");
                    parms.CoeffModulus.Set(ChooserEvaluator.DefaultParameterOptions[8192]);
                    parms.DecompositionBitCount = 192; // maybe not optimal
                    break;
                case 4:
                    parms.PolyModulus.Set("1x^16384 + 1");
                    parms.CoeffModulus.Set(ChooserEvaluator.DefaultParameterOptions[16384]);
                    parms.DecompositionBitCount = 284; // maybe not optimal
                    break;
            }
            OnPropertyChanged("ParameterOutput");

            ProgressChanged(0.5, 1);

            keygen = new KeyGenerator(parms);
            keygen.Generate(10);
            pubkey = keygen.PublicKey;
            OnPropertyChanged("PubKeyOutput");
            evalkeys = keygen.EvaluationKeys;
            OnPropertyChanged("EvalKeysOutput");
            seckey = keygen.SecretKey;
            OnPropertyChanged("SecKeyOutput");

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
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
            parms.PlainModulus.Set(2);
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

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
