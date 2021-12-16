using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System.Numerics;

namespace PrimeTest
{
    [PluginInfo("PrimeTest.Properties.Resources", "PluginCaption", "PluginTooltip", "PrimeTest/DetailedDescription/doc.xml", "PrimeTest/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class PrimeTest : ICrypComponent
    {
        #region IPlugin Members

        public event CrypTool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;
        private void FireOnPluginStatusChangedEvent()
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(this, new StatusEventArgs(0));
            }
        }

        public event CrypTool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void FireOnGuiLogNotificationOccuredEvent(string message, NotificationLevel lvl)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, lvl));
            }
        }
        private void FireOnGuiLogNotificationOccuredEventError(string message)
        {
            FireOnGuiLogNotificationOccuredEvent(message, NotificationLevel.Error);
        }

        public event CrypTool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void FireOnPluginProgressChangedEvent(string message, NotificationLevel lvl)
        {
            if (OnPluginProgressChanged != null)
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(0, 0));
            }
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public CrypTool.PluginBase.ISettings Settings => new PrimeTestSettings();

        public System.Windows.Controls.UserControl Presentation => null;

        public void PreExecution()
        {
        }

        public void Execute()
        {
            ProgressChanged(0, 100);

            if (InputNumber != null)
            {
                Output = InputNumber.IsProbablePrime();
            }
            else
            {
                Output = false;
            }

            ProgressChanged(100, 100);
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

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        private void FirePropertyChangeEvent(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Properties
        private BigInteger m_InputNumber;
        [PropertyInfo(Direction.InputData, "InputNumberCaption", "InputNumberTooltip", true)]
        public BigInteger InputNumber
        {
            get => m_InputNumber;
            set
            {
                if (value != m_InputNumber)
                {
                    try
                    {
                        m_InputNumber = value;
                        FirePropertyChangeEvent("InputString");
                    }
                    catch
                    {
                        FireOnGuiLogNotificationOccuredEventError("Damn");
                    }
                }
            }
        }

        private bool m_Output;
        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip", false)]
        public bool Output
        {
            get => m_Output;
            set
            {
                m_Output = value;
                FirePropertyChangeEvent("Output");
            }
        }
        #endregion
    }
}