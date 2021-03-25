using System;
using CrypTool.PluginBase;
using System.ComponentModel;
using CrypTool.PluginBase.Miscellaneous;
using System.Windows.Controls;

namespace SmartCard
{
  [Author("Malte Gronau", null, "", "")]
  [PluginInfo("SmartCard.Properties.Resources", "PluginCaption", "PluginTooltip", "SmartCard/DetailedDescription/doc.xml", "SmartCard/Images/SmartCard.png")]
  [ComponentCategory(ComponentCategory.Protocols)]
  public class SmartCard : ICrypComponent
  {
    # region private variables
    private SmartCardSettings settings = new SmartCardSettings();
    // smartcard command input as hex-text
    private String dataInput;
    // apdu data to send to card
    private byte[] APDUData = null;
    // rapdu data receiving from card
    private byte[] response = null;
    // log string output
    private String logString;
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
    private void ProgressEnd()
    {
        EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(1, 1));
    }

    private void ProgressStart()
    {
        EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(0, 1));
    }

    # endregion events

    # region constructor
    public SmartCard()
    {
      settings.OnGuiLogNotificationOccured += settings_OnGuiLogNotificationOccured;
    }

    void settings_OnGuiLogNotificationOccured(IPlugin sender, GuiLogEventArgs args)
    {
      GuiLogMessage(args.Message, args.NotificationLevel);
    }
    # endregion

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
            dataInput = value;
            APDUData = HexStringToByte(dataInput);
            // HexStringToByte returns null if conversion failed
            if (APDUData == null)
            {
                GuiLogMessage("InputData is not valid hex data.", NotificationLevel.Error);
            }
            OnPropertyChanged("DataInput");
        }
    }

    [PropertyInfo(Direction.OutputData, "LogStringCaption", "LogStringTooltip", true)]
    public String LogString
    {
        get
        {
            return logString;
        }
        set
        {
            logString = value;
            OnPropertyChanged("LogString");
        }
    }

    [PropertyInfo(Direction.OutputData, "ResponseCaption", "ResponseTooltip", true)]
    public byte[] Response
    {
      get { return response; }
        set
        {
          this.response = value;
          OnPropertyChanged("Response"); 
        }
    } 
    #endregion

    # region IPlugin-Methods
    public ISettings Settings
    {
      get { return settings; }
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
        ProgressStart();
        GuiLogMessage("Executing smartcard plugin.", NotificationLevel.Debug);

        // data to send ?
        if (this.APDUData == null)
        {
            GuiLogMessage("No data to send.", NotificationLevel.Error);
            return;
        } 
        
        // APDU >= 4 !!
        if (this.APDUData.Length < 4)
        {
            GuiLogMessage("Invalid APDU.", NotificationLevel.Error);
            return;
        }


      // just virtual reader ??  -> response = 0x9000
        if (settings.Collection[settings.CardReader] == typeof(SmartCard).GetPluginStringResource(SmartCardSettings.VirtualReader))
        {
            byte[] bResponse = new byte[2];
            bResponse[0] = 0x64;
            bResponse[1] = 0xA1;

            // output response data
            this.Response = bResponse;

            // create logging output
            this.LogString = "APDU:\n" +
                             dataInput +
                             "\n" +
                             "RAPDU:\n" +
                             HexValuesToHexString(bResponse) +
                             "\n----------------------------";
        }
        else
        {
            int Context = pcscWrapper.EstablishContext();
            if (Context == pcscWrapper.INVALID_HANDLE)
            {
                GuiLogMessage("Could not establish PC/SC Context.", NotificationLevel.Error);
                return;
            }

            int hCard = pcscWrapper.Connect(Context, settings.Collection[settings.CardReader]);
            if (hCard == pcscWrapper.INVALID_HANDLE)
            {
                GuiLogMessage("Could not establish connection to reader: " + settings.Collection[settings.CardReader], NotificationLevel.Error);
                return;
            }

            byte[] tmpResponse = pcscWrapper.Transmit(hCard, APDUData);
            if (tmpResponse == null)
            {
                GuiLogMessage("Transmission of APDU failed.", NotificationLevel.Error);
                return;
            }

            if (tmpResponse.Length < 2)
            {
                GuiLogMessage("invalid response data: " + HexValuesToHexString(tmpResponse), NotificationLevel.Error);
                return;
            }

            // output response data
            this.Response = tmpResponse;

            // create logging output
            this.LogString = "APDU:\n" +
                             dataInput +
                             "\n" +
                             "RAPDU:\n" +
                             HexValuesToHexString(this.response) +
                             "\n----------------------------";


            // disconnect reader and release PC/SC context
            if (pcscWrapper.Disconnect(hCard, pcscWrapper.SCARD_LEAVE_CARD) != pcscWrapper.SCARD_S_SUCCESS)
            {
                GuiLogMessage("Disconnection failed.", NotificationLevel.Error);
                return;
            }
            if (pcscWrapper.ReleaseContext(Context) != pcscWrapper.SCARD_S_SUCCESS)
            {
                GuiLogMessage("Release context failed.", NotificationLevel.Error);
                return;
            }
        }

      GuiLogMessage("Execution of smartcard command succeeded.", NotificationLevel.Debug);
      ProgressEnd();
    }

    public void PostExecution()
    {
    }

      public void Stop()
    {
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

      #region Hilfsmethoden

    private static String HexValuesToHexString(byte[] bytes)
    {
        String sTemp = "";

        for (int i = 0; i < bytes.Length; i++)
        {
            sTemp += bytes[i].ToString("X2");
        }

        return sTemp;
    }

    private byte[] HexStringToByte(String hexString)
    {
        // String in bytes konvertieren
        if (hexString == null)
            return null;

        if (hexString.Length % 2 == 1)
            hexString = '0' + hexString; // Up to you whether to pad the first or last byte

        byte[] data = new byte[hexString.Length / 2];

        for (int i = 0; i < data.Length; i++)
        {
            try
            {
                data[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            catch (System.OverflowException)
            {
                GuiLogMessage("Conversion from string to byte overflowed.", NotificationLevel.Error);
                return null;
            }
            catch (System.FormatException)
            {
                GuiLogMessage("The string is not formatted as a byte.", NotificationLevel.Error);
                return null;
            }
            catch (System.ArgumentNullException)
            {
                GuiLogMessage("The string is null.", NotificationLevel.Error);
                return null;
            }
        }

        return data;
    }

      #endregion
  }
}
