using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.Plugins.CramerShoup.lib;
using Org.BouncyCastle.Security;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace CrypTool.Plugins.CramerShoup
{
    [Author("Jan Jansen", "jan.jansen-n22@rub.de", "Ruhr Uni-Bochum", "http://cits.rub.de/")]
    [PluginInfo("CramerShoup.Properties.Resources", "PluginKeyCaption", "PluginKeyTooltip", "CramerShoup/DetailedDescription/doc.xml", new[] { "CramerShoup/Images/cskey.png" })]
    [ComponentCategory(ComponentCategory.CiphersModernAsymmetric)]
    public class CramerShoupKeyGenerator : ICrypComponent
    {
        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly CramerShoupKeyGeneratorSettings settings = new CramerShoupKeyGeneratorSettings();

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.OutputData, "OutputPrivCaption", "OutputPrivTooltip")]
        public ECCramerShoupPrivateParameter Priv
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputPublCaption", "OutputPublTooltip")]
        public ECCramerShoupPublicParameter Publ
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
        private string GetCurveName()
        {
            List<string> list = new List<string>
            {
                "curve25519",
                "secp128r1",
                "secp160k1",
                "sect409r1"
            };

            return list[settings.Curve];
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            SecureRandom random = new SecureRandom();
            // HOWTO: Use this to show the progress of a plugin algorithm execution in the editor.
            ECCramerShoupGenerator gen = new ECCramerShoupGenerator();
            ECCramerShoupKeyPair key = gen.Generator(random, GetCurveName());
            ProgressChanged(0.5, 1);
            Publ = key.Public;
            Priv = key.Priv;
            OnPropertyChanged("Publ");
            OnPropertyChanged("Priv");
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
