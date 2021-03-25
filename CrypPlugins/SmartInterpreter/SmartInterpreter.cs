using System;
using CrypTool.PluginBase;
using System.ComponentModel;
using CrypTool.PluginBase.Miscellaneous;
using System.Windows.Controls;

namespace SmartInterpreter
{
  [Author("Malte Gronau", "malte.gronau@web.de", "", "")]
  [PluginInfo("SmartInterpreter.Properties.Resources", "PluginCaption", "PluginTooltip", "SmartInterpreter/DetailedDescription/doc.xml", "SmartInterpreter/Images/SmartInterpreter.png")]
  [ComponentCategory(ComponentCategory.Protocols)]
  public class SmartInterpreter : ICrypComponent
  {
    # region private variables
    // Input data string
    private String dataInput;
    // Command string - array
    private String[] Commands = null;
    // last statusword received
    private byte[] statusWord = null;
    // last response data received
    private byte[] response = null;
    // current string command output
    private String apdustring;
    // internal command sequence counter
    // resetting to 0 after getting new dataInput
    // increasing by 1 calling Execute()
    private int CommandCounter = 0;
    # endregion

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
    # endregion events

    #region IO

    [PropertyInfo(Direction.InputData, "DataInputCaption", "DataInputTooltip", true)]
    public String DataInput
    {
        get
        {
            return dataInput;
        }
        set
        {
            if(value == null)
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
                Commands = new String[1];
                Commands[0] = dataInput;
            }
            
            // command sequence not validated !!!
            OnPropertyChanged("DataInput");
        }
    }

    [PropertyInfo(Direction.InputData, "ResponseCaption", "ResponseTooltip", false)]
    public byte[] Response
    {
        get {
            if (this.statusWord != null)
            {
                if (this.response == null)
                {
                    // return only statusword
                    return this.statusWord;
                }
                else
                {
                    // return response data + status word
                    byte[] aReturn = new byte[this.response.Length + this.statusWord.Length];

                    this.response.CopyTo(aReturn, 0);
                    this.statusWord.CopyTo(aReturn, this.response.Length);

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
                this.statusWord = null;
                this.response = null;
                OnPropertyChanged("Response");
                return;
            }

            // structure response data
            this.statusWord = new byte[2];
            this.statusWord[0] = value[value.Length - 2];
            this.statusWord[1] = value[value.Length - 1];
            this.response = new byte[value.Length - 2];
            for (int i = 0; i < value.Length - 2; i++)
            {
                this.response[i] = value[i];
            }

            OnPropertyChanged("Response");
        }
    }

    [PropertyInfo(Direction.OutputData, "APDUStringCaption", "APDUStringTooltip", true)]
    public String APDUString
    {
        get
        {
            return apdustring;
        }
        set
        {
            apdustring = value;
            OnPropertyChanged("APDUString");
        }
    }

    #endregion
      
    # region IPlugin-Methods
    public ISettings Settings
    {
        get { return null; }
    }

    public UserControl Presentation
    {
      get { return null; }
    }

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
                this.statusWord = null;
                // logging and sending command
                GuiLogMessage("Executing Send command: " + Commands[CommandCounter-1], NotificationLevel.Debug);
                APDUString = Commands[CommandCounter-1].Substring(4);
            } else
                if (Commands[CommandCounter - 1].ToUpper().IndexOf("//") == 0)
                {
                    GuiLogMessage("Comment: " + Commands[CommandCounter - 1], NotificationLevel.Debug);
                    // execute myself without sending command to smartcard
                    this.Response = null;
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
        this.CommandCounter = 0;
        // last statusword received
        this.statusWord = null;
        // last response data received
        this.response = null;
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
