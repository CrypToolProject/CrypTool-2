using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using PKCS1.Library;
using PKCS1.WpfVisualization;

namespace PKCS1
{
    [Author("Jens Schomburg", "mail@escobar.de", "Universität Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("PKCS1.Properties.Resources", "PluginCaption", "PluginTooltip", "PKCS1/DetailedDescription/doc.xml", "PKCS1/PKCS1.png")]
    [FunctionList("FL_F_pkcs1", "FL_P_bleichenbacher")]
    [FunctionList("FL_F_pkcs1", "FL_P_kuehn")]

    public class PKCS1Plugin : ICrypTutorial
    {
        private Pkcs1Control m_Pkcs1Plugin = null;

        public PKCS1Plugin()
        {
            GuiLogMsgHandOff.getInstance().OnGuiLogMsgSend += GuiLogMessage; // bei weiterleitung registrieren
        }

        #region EventHandler

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, message, logLevel);
        }

        #region IPlugin Member

        public void Dispose()
        {
            if (m_Pkcs1Plugin != null)
            {
                m_Pkcs1Plugin.Dispose();
            }
        }

        public void Execute()
        {
            //throw new NotImplementedException();
        }

        public void Initialize()
        {
            //throw new NotImplementedException();
        }

        public void Stop()
        {
            //throw new NotImplementedException();
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get
            {
                if (m_Pkcs1Plugin == null)
                {
                    m_Pkcs1Plugin = new Pkcs1Control();
                }

                return m_Pkcs1Plugin;
            }
        }

        public ISettings Settings => null;

        #endregion
    }
}
