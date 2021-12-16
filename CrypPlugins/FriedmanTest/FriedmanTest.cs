using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Linq;

namespace FriedmanTest
{
    [Author("Georgi Angelov, Danail Vazov", "vazov@CrypTool.org", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("FriedmanTest.Properties.Resources", "PluginCaption", "PluginTooltip", "FriedmanTest/DetailedDescription/doc.xml", "FriedmanTest/friedman.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class FriedmanTest : ICrypComponent
    {
        public FriedmanTest()
        {
            settings = new FriedmanTestSettings();
        }

        #region Private Variables

        private FriedmanTestSettings settings;
        private double keyLength = 0;
        private double kappaCiphertext = 0;
        private string stringOutput = "";
        private int[] arrayInput;

        #endregion

        #region Private methods

        private void ShowStatusBarMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void ShowProgress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion

        #region Properties (Inputs/Outputs)

        [PropertyInfo(Direction.OutputData, "StringOutputCaption", "StringOutputTooltip", false)]
        public string StringOutput
        {
            get => stringOutput;
            set { }
        }

        [PropertyInfo(Direction.OutputData, "KeyLengthCaption", "KeyLengthTooltip", false)]
        public double KeyLength
        {
            get => keyLength;
            set { }
        }

        [PropertyInfo(Direction.OutputData, "KappaCiphertextCaption", "KappaCiphertextTooltip", false)]
        public double KappaCiphertext
        {
            get => kappaCiphertext;
            set { }
        }

        [PropertyInfo(Direction.InputData, "ArrayInputCaption", "ArrayInputTooltip", true)]
        public int[] ArrayInput
        {
            get => arrayInput;
            set
            {
                arrayInput = value;
                OnPropertyChanged("ArrayInput");
            }
        }
        #endregion

        #region IPlugin Members

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;


        public ISettings Settings
        {
            get => settings;
            set => settings = (FriedmanTestSettings)value;
        }

        public System.Windows.Controls.UserControl Presentation => null;

        public void PreExecution()
        {
            //nothing to do
        }

        public void Execute()
        {
            if (arrayInput == null)
            {
                return;
            }

            double Kp; //Kappa "language"

            //Now we set the Kappa plain-text coefficient. Default is English.
            switch (settings.Kappa)
            {
                case 1: Kp = 0.0762; break;     // german
                case 2: Kp = 0.0778; break;     // french
                case 3: Kp = 0.0770; break;     // spanish
                case 4: Kp = 0.0738; break;     // italian
                case 5: Kp = 0.0745; break;     // portuguese
                default: Kp = 0.0667; break;    // english
            }

            ShowStatusBarMessage("Using IC = " + Kp.ToString() + " for analysis...", NotificationLevel.Debug);

            if (arrayInput.Length < 2)
            {
                // error, only one letter?
                ShowStatusBarMessage("Error - cannot analyze an array of a single letter.", NotificationLevel.Error);
                return;
            }

            double cipherTextLength = arrayInput.Sum();
            double countDoubleCharacters = arrayInput.Sum(i => i * ((double)i - 1));

            ShowStatusBarMessage(string.Format("Input analyzed: Got {0} different letters in a text of total length {1}.", arrayInput.Length, (int)cipherTextLength), NotificationLevel.Debug);

            double Keqdist = 1.0 / 26;
            kappaCiphertext = countDoubleCharacters / (cipherTextLength * (cipherTextLength - 1));
            keyLength = ((Kp - Keqdist) * cipherTextLength) / ((cipherTextLength - 1) * kappaCiphertext - (Keqdist * cipherTextLength) + Kp);

            string ciphermode = (Math.Abs(Kp - kappaCiphertext) > 0.01) ? "polyalphabetic" : "monoalphabetic/cleartext";

            stringOutput = string.Format("KeyLen = {0}\r\nIC_analyzed = {1}\r\nIC_provided = {2}\r\nMode = {3}", keyLength.ToString("0.00000"), kappaCiphertext.ToString("0.00000"), Kp, ciphermode);

            OnPropertyChanged("StringOutput");
            OnPropertyChanged("KeyLength");
            OnPropertyChanged("KappaCiphertext");

            // final step of progress
            ShowProgress(100, 100);
        }

        public void PostExecution()
        {
            //throw new NotImplementedException();
        }

        public void Stop()
        {
            //throw new NotImplementedException();
        }

        public void Initialize()
        {
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}