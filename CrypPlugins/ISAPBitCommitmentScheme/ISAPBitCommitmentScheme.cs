using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Controls;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using ISAPCommitmentSchemeWrapper;

namespace ISAPBitCommitmentScheme
{
    [Author("Sven Rech and Martin Schmidt", "rech@CrypTool.org", "Uni Duisburg-Essen", "http://www.vs.uni-duisburg-essen.de")]
    [PluginInfo("ISAPBitCommitmentScheme.Properties.Resources", "PluginCaption", "PluginTooltip", "ISAPBitCommitmentScheme/DetailedDescription/doc.xml", "ISAPBitCommitmentScheme/Images/icon.png")]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class ISAPBitCommitmentScheme : ICrypComponent
    {
        private readonly ISAPBitCommitmentSchemeSettings _settings = new ISAPBitCommitmentSchemeSettings();
        private string _logMessage;
        private Wrapper _ISAPalgorithmWrapper = new Wrapper();

        public event PropertyChangedEventHandler PropertyChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private bool _inputBit;
        [PropertyInfo(Direction.InputData, "InputBitCaption", "InputBitTooltip")]
        public bool InputBit
        {
            get
            {
                return _inputBit;
            }
            set
            {
                _inputBit = value;
                OnPropertyChanged("InputBit");
            }
        }

        private int _dimension = 7;
        [PropertyInfo(Direction.InputData, "DimensionCaption", "DimensionTooltip")]
        public int Dimension
        {
            get
            {
                return _dimension;
            }
            set
            {
                _dimension = value;
                OnPropertyChanged("Dimension");
            }
        }

        private int _s = 128;
        [PropertyInfo(Direction.InputData, "SCaption", "STooltip")]
        public int S
        {
            get
            {
                return _s;
            }
            set
            {
                _s = value;
                OnPropertyChanged("S");
            }
        }

        private BigInteger[] _p;
        [PropertyInfo(Direction.OutputData, "PCaption", "PTooltip")]
        public BigInteger[] P
        {
            get
            {
                return _p;
            }
            set 
            { 
                _p = value;
                OnPropertyChanged("P");
            }
        }

        private BigInteger _q;
        [PropertyInfo(Direction.OutputData, "QCaption", "QTooltip")]
        public BigInteger Q
        {
            get
            {
                return _q;
            }
            set
            {
                _q = value;
                OnPropertyChanged("Q");
            }
        }

        private double[] _alpha;
        [PropertyInfo(Direction.OutputData, "AlphaCaption", "AlphaTooltip")]
        public double[] Alpha
        {
            get
            {
                return _alpha;
            }
            set
            {
                _alpha = value;
                OnPropertyChanged("Alpha");
            }
        }

        private BigInteger[] _a;
        [PropertyInfo(Direction.OutputData, "ACaption", "ATooltip")]
        public BigInteger[] A
        {
            get
            {
                return _a;
            }
            set
            {
                _a = value;
                OnPropertyChanged("A");
            }
        }

        private BigInteger[] _b;
        [PropertyInfo(Direction.OutputData, "BCaption", "BTooltip")]
        public BigInteger[] B
        {
            get
            {
                return _b;
            }
            set
            {
                _b = value;
                OnPropertyChanged("B");
            }
        }

        private double[] _eta;
        [PropertyInfo(Direction.OutputData, "EtaCaption", "EtaTooltip")]
        public double[] Eta
        {
            get
            {
                return _eta;
            }
            set
            {
                _eta = value;
                OnPropertyChanged("Eta");
            }
        }

        [PropertyInfo(Direction.OutputData, "LogMessageCaption", "LogMessageTooltip")]
        public String LogMessage
        {
            get
            {
                return _logMessage;
            }
            set
            {
                _logMessage = value;
                OnPropertyChanged("LogMessage");
            }
        }

        public ISettings Settings
        {
            get { return _settings; }
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
            ProgressChanged(0.0, 100.0);
            try
            {
                var result = _ISAPalgorithmWrapper.Run(_inputBit, _dimension, _s);
                LogMessage = result.log;
                P = result.p;
                Q = result.q;
                Alpha = result.alpha;
                A = result.a;
                B = result.b;
                Eta = result.eta;
                ProgressChanged(100.0, 100.0);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("ISAP algorithm failed: {0}", ex.Message), NotificationLevel.Error);
            }
            OnPropertyChanged("InputBit");
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

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }
    }
}
