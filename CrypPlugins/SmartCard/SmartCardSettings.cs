using System;
using System.Text;
using CrypTool.PluginBase;
using System.ComponentModel;
using CrypTool.PluginBase.Miscellaneous;
using System.Collections.ObjectModel;

namespace SmartCard
{
  public class SmartCardSettings : ISettings
  {
    private ObservableCollection<string> collection = new ObservableCollection<string>();

    public static string VirtualReader = "Virtual_Cardreader";

    public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
    private void GuiLogMessage(string message, NotificationLevel logLevel)
    {
      EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, null, new GuiLogEventArgs(message, null, logLevel));
    }


    private int cardReader = 0; // 0=default reader

    public SmartCardSettings()
    {
        this.SearchCardReaders();
    }

    [TaskPane("CardReaderCaption", "CardReaderTooltip", null, 0, false, ControlType.DynamicComboBox, new string[] { "Collection" })]
    public int CardReader
    {
      get { return this.cardReader; }
      set
      {
        if (((int)value) != cardReader)
        {
            this.cardReader = (int)value;
            OnPropertyChanged("CardReader");            
        }
      }
    }

    [TaskPane("SearchCardReadersCaption", "SearchCardReadersTooltip", null, 1, false, ControlType.Button)]
    public void SearchCardReaders()
    {
        int hContext = pcscWrapper.EstablishContext();
        String[] sReaders = pcscWrapper.ListReaders(hContext);

        collection.Clear();
        
        collection.Add( typeof(SmartCard).GetPluginStringResource(VirtualReader) );

        if (sReaders != null)
        {
            foreach (String sReader in sReaders)
            {
                pcscWrapper.READERSTATE rState = pcscWrapper.getReaderState(hContext, sReader);

                GuiLogMessage("Reader found : " + sReader, NotificationLevel.Info);

                // Debug Messages
                GuiLogMessage("Reader Status: 0x" + System.Convert.ToString(rState.dwEventState, 16), NotificationLevel.Debug);
                if ((rState.dwEventState & (UInt32)pcscWrapper.CardState.PRESENT) == 0)
                {
                    GuiLogMessage("Reader is empty.", NotificationLevel.Debug);
                }
                else
                {
                    // ATR aufbereiten
                    StringBuilder ATRHex = new StringBuilder();
                    for (int i = 0; i < rState.cbAtr; i++)
                    {
                        ATRHex.Append(" 0x" + rState.rgbAtr[i].ToString("X2"));
                    }
                    GuiLogMessage("smartcard's ATR:" + ATRHex.ToString(), NotificationLevel.Debug);
                    if ((rState.dwEventState & (UInt32)pcscWrapper.CardState.EXCLUSIVE) != 0)
                    {
                        GuiLogMessage("Reader is exclusively used by another application.", NotificationLevel.Debug);
                    }
                }
                collection.Add(sReader);
            }
        }
        else
        {
            GuiLogMessage("No reader found.", NotificationLevel.Info);
        }

      pcscWrapper.ReleaseContext(hContext);

      Collection = collection;
      CardReader = 0;
    }

    public ObservableCollection<string> Collection
    {
      get { return collection; }
      set 
      {
        if (value != collection)
        {
          collection = value;
        }
        OnPropertyChanged("Collection");
      }
    }


    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;
      public void Initialize()
      {
          
      }

      protected void OnPropertyChanged(string name)
    {
      EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
    }

    #endregion
  }
}
