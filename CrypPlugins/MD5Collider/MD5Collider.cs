using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.Plugins.MD5Collider.Algorithm;
using CrypTool.Plugins.MD5Collider.Presentation;
using System.ComponentModel;

namespace CrypTool.Plugins.MD5Collider
{
    [Author("Holger Pretzsch", "CrypTool@holger-pretzsch.de", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("CrypTool.Plugins.MD5Collider.Properties.Resources", "PluginCaption", "PluginTooltip", "MD5Collider/DetailedDescription/doc.xml", "MD5Collider/MD5Collider.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    internal class MD5Collider : ICrypComponent
    {
        private readonly QuickWatchPresentationContainer presentation = new QuickWatchPresentationContainer();

        private IMD5ColliderAlgorithm _collider;
        private IMD5ColliderAlgorithm Collider
        {
            get => _collider;
            set { _collider = value; presentation.Collider = value; }
        }

        public event CrypTool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;
        public event CrypTool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event CrypTool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public CrypTool.PluginBase.ISettings Settings => null;
        public System.Windows.Controls.UserControl Presentation => presentation;

        public void PreExecution() { Dispose(); }
        public void PostExecution() { Dispose(); }
        public void Stop() { Collider.Stop(); }
        public void Initialize() { }

        public MD5Collider()
        {
            Collider = new MultiThreadedMD5Collider<StevensCollider>();
            //Collider.Status = "Waiting";
        }

        private byte[] outputData1;
        [PropertyInfo(Direction.OutputData, "OutputData1Caption", "OutputData1Tooltip", false)]
        public byte[] OutputData1
        {
            get => outputData1;
            set
            {
                outputData1 = value;
                OnPropertyChanged("OutputData1");
                OnPropertyChanged("OutputDataStream1");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputDataStream1Caption", "OutputDataStream1Tooltip", false)]
        public ICrypToolStream OutputDataStream1
        {
            get
            {
                if (outputData1 != null)
                {
                    return new CStreamWriter(outputData1);
                }
                else
                {
                    return null;
                }
            }
        }

        private byte[] outputData2;
        [PropertyInfo(Direction.OutputData, "OutputData2Caption", "OutputData2Tooltip", false)]
        public byte[] OutputData2
        {
            get => outputData2;
            set
            {
                outputData2 = value;
                OnPropertyChanged("OutputData2");
                OnPropertyChanged("OutputDataStream2");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputDataStream2Caption", "OutputDataStream2Tooltip", false)]
        public ICrypToolStream OutputDataStream2
        {
            get
            {
                if (outputData2 != null)
                {
                    return new CStreamWriter(outputData2);
                }
                else
                {
                    return null;
                }
            }
        }

        private byte[] randomSeed;
        [PropertyInfo(Direction.InputData, "RandomSeedCaption", "RandomSeedTooltip", false)]
        public byte[] RandomSeed
        {
            get => randomSeed;
            set { randomSeed = value; OnPropertyChanged("RandomSeed"); }
        }

        private byte[] prefix;
        [PropertyInfo(Direction.InputData, "PrefixCaption", "PrefixTooltip", false)]
        public byte[] Prefix
        {
            get => prefix;
            set { prefix = value; OnPropertyChanged("Prefix"); }
        }

        public void Execute()
        {
            ProgressChanged(0.5, 1.0);

            Collider.RandomSeed = RandomSeed;

            if (Prefix != null)
            {
                if (Prefix.Length % 64 != 0)
                {
                    GuiLogMessage("Prefixed data must be a multiple of 64 bytes long!", NotificationLevel.Error);
                    return;
                }

                Collider.IHV = new IHVCalculator(Prefix).GetIHV();
            }

            Collider.FindCollision();

            OutputData1 = Collider.FirstCollidingData;
            OutputData2 = Collider.SecondCollidingData;

            ProgressChanged(1.0, 1.0);
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public void Dispose()
        {
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }
    }
}
