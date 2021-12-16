using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace CrypTool.Plugins.RegularExpressions
{
    [Author("Daniel Kohnen", "kohnen@CrypTool.org", "Universität Duisburg Essen", "http://www.uni-due.de")]
    [PluginInfo("Regular Expression Match", "Matching Regular Expression", "RegularExpressions/Description/RegexMatchDescript.xaml", new[] { "RegularExpressions/icons/regmatchicon.png" })]
    [ComponentCategory(ComponentCategory.ToolsDataflow)]
    public class RegExMatch : ICrypComponent
    {
        #region Private variables

        private RegExMatchSettings settings;
        private string input;
        private string pattern;
        private string outputText;
        private bool outputBool = false;

        # endregion

        # region Public Methods

        public RegExMatch()
        {
            settings = new RegExMatchSettings();
        }

        # endregion

        # region Properties

        [PropertyInfo(Direction.InputData, "Input", "Input a string to be processed by the RegEx Matcher", true)]
        public string Input
        {
            get => input;

            set
            {
                input = value;
                OnPropertyChange("Input");
            }
        }

        [PropertyInfo(Direction.InputData, "Pattern", "Pattern for the RegEx", true)]
        public string Pattern
        {
            get => pattern;

            set
            {
                pattern = value;
                OnPropertyChange("Pattern");
            }
        }

        [PropertyInfo(Direction.OutputData, "Output", "Output")]
        public string OutputText
        {
            get => outputText;
            set
            {
                outputText = value;
                OnPropertyChange("OutputText");
            }
        }

        [PropertyInfo(Direction.OutputData, "Output Bool", "True: pattern matches. False: it does not.")]
        public bool OutputBool
        {
            get => outputBool;
            set
            {
                outputBool = value;
                OnPropertyChange("OutputBool");
            }
        }

        # endregion

        #region IPlugin Member

        public void Dispose()
        {

        }

        public void Execute()
        {
            try
            {
                if (input != null && pattern != null)
                {
                    Match match = Regex.Match(input, pattern);
                    if (match.Success)
                    {
                        OutputBool = true;
                        OutputText = match.Value;

                    }

                    else
                    {
                        OutputBool = false;
                        OutputText = "";

                    }
                }

                ProgressChanged(1, 1);
            }
            catch (Exception)
            {
                //GuiLogMessage("Regular Expression is not valid.", NotificationLevel.Warning);
            }
        }

        public void Initialize()
        {

        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;

        private void OnPropertyChange(string propertyname)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(propertyname));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public void PostExecution()
        {

        }

        public void PreExecution()
        {

        }

        public System.Windows.Controls.UserControl Presentation => null;

        public ISettings Settings
        {
            get => settings;
            set => settings = (RegExMatchSettings)value;
        }

        public void Stop()
        {

        }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
