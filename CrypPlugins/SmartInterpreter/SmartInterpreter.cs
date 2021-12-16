using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Windows.Controls;

namespace SmartInterpreter
{
    [Author("Malte Gronau", "malte.gronau@web.de", "", "")]
    [PluginInfo("SmartInterpreter.Properties.Resources", "PluginCaption", "PluginTooltip", "SmartInterpreter/DetailedDescription/doc.xml", "SmartInterpreter/Images/SmartInterpreter.png")]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class SmartInterpreter : ICrypComponent
    {
        #region private variables
        // Input data string
        private string dataInput;
        // Command string - array
        private string[] Commands = null;
        // last statusword received
        private byte[] statusWord = null;
        // last response data received
        private byte[] response = null;
        // current string command output
        private string apdustring;
        // internal command sequence counter
        // resetting to 0 after getting new dataInput
        // increasing by 1 calling Execute()
        private int CommandCounter = 0;
        #endregion

        #region events
        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void PluginStatusChanged(int imageNumber)
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(this, new StatusEventArgs(StatusChangedMode.ImageUpdate, imageNumber));
            }
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }
        #endregion events

        #region IO

        [PropertyInfo(Direction.InputData, "DataInputCaption", "DataInputTooltip", true)]
        public string DataInput
        {
            get => dataInput;
            set
            {
                if (value == null)
                {
                    return;
                }
                // reset sequence counter after getting new data
                CommandCounter = 0;

                // remove formatting symbols from dataInput 
                dataInput = value;
                dataInput = dataInput.Replace("\r", "");
                dataInput = dataInput.Replace("\n", "");
                dataInput = dataInput.Replace(" ", "");

                // getting command sequence from dataInput
                Commands = dataInput.Split(';');
                if (Commands.Length == 0)
                {
                    Commands = new string[1];
                    Commands[0] = dataInput;
                }

                // command sequence not validated !!!
                OnPropertyChanged("DataInput");
            }
        }

        [PropertyInfo(Direction.InputData, "ResponseCaption", "ResponseTooltip", false)]
        public byte[] Response
        {
            get
            {
                if (statusWord != null)
                {
                    if (response == null)
                    {
                        // return only statusword
                        return statusWord;
                    }
                    else
                    {
                        // return response data + status word
                        byte[] aReturn = new byte[response.Length + statusWord.Length];

                        response.CopyTo(aReturn, 0);
                        statusWord.CopyTo(aReturn, response.Length);

                        return aReturn;
                    }
                }
                else { return null; }
            }
            set
            {
                if (value == null)
                {
                    // call after no smartcard operation
                    // leave variable states
                    OnPropertyChanged("Response");
                    return;
                }

                if (value.Length < 2)
                {
                    // length of response at least 2 bytes statusword
                    GuiLogMessage("Invalid Response data!", NotificationLevel.Error);
                    statusWord = null;
                    response = null;
                    OnPropertyChanged("Response");
                    return;
                }

                // structure response data
                statusWord = new byte[2];
                statusWord[0] = value[value.Length - 2];
                statusWord[1] = value[value.Length - 1];
                response = new byte[value.Length - 2];
                for (int i = 0; i < value.Length - 2; i++)
                {
                    response[i] = value[i];
                }

                OnPropertyChanged("Response");
            }
        }

        [PropertyInfo(Direction.OutputData, "APDUStringCaption", "APDUStringTooltip", true)]
        public string APDUString
        {
            get => apdustring;
            set
            {
                apdustring = value;
                OnPropertyChanged("APDUString");
            }
        }

        #endregion

        #region IPlugin-Methods
        public ISettings Settings => null;

        public UserControl Presentation => null;

        public void PreExecution()
        {
        }

        public void Execute()
        {
            // Execute first called by getting input from TextInput box,
            // then called by getting input from response input
            GuiLogMessage("Executing SmartInterpreter plugin.", NotificationLevel.Debug);

            if ((Commands == null) || (Commands.Length <= CommandCounter) || (Commands[CommandCounter] == null))
            {
                GuiLogMessage("No commands.", NotificationLevel.Debug);
                return;
            }

            GuiLogMessage("Command Counter: " + CommandCounter, NotificationLevel.Debug);

            if (CommandCounter < Commands.Length)
            {
                // first increasing CommandCounter
                CommandCounter++;

                // select command
                if (Commands[CommandCounter - 1].ToUpper().IndexOf("SEND") == 0)
                {
                    statusWord = null;
                    // logging and sending command
                    GuiLogMessage("Executing Send command: " + Commands[CommandCounter - 1], NotificationLevel.Debug);
                    APDUString = Commands[CommandCounter - 1].Substring(4);
                }
                else
                    if (Commands[CommandCounter - 1].ToUpper().IndexOf("//") == 0)
                {
                    GuiLogMessage("Comment: " + Commands[CommandCounter - 1], NotificationLevel.Debug);
                    // execute myself without sending command to smartcard
                    Response = null;
                }
                else
                {
                    GuiLogMessage("Invalid command string: " + Commands[CommandCounter - 1], NotificationLevel.Error);
                }
            }

            ProgressChanged(CommandCounter, Commands.Length);
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
            // resetting command counter
            CommandCounter = 0;
            // last statusword received
            statusWord = null;
            // last response data received
            response = null;
        }

        public void Initialize()
        {

        }

        public void Dispose()
        {
        }

        #endregion IPlugin-Methods

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion

    }
}
