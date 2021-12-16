//////////////////////////////////////////////////////////////////////////////////////////////////
// CrypTool V2
// © 2008 - Gerhard Junker
// Apache License see http://www.apache.org/licenses/
//
// $HeadURL: https://svn.cryptool.org/CrypTool2/trunk/CrypPlugins/Whirlpool/WhirlpoolPlugin.cs $
//////////////////////////////////////////////////////////////////////////////////////////////////
// $Revision:: 8983                                                                           $://
// $Author:: kopal                                                                            $://
// $Date:: 2021-03-24 14:51:34 +0100 (Mi, 24 Mrz 2021)                                        $://
//////////////////////////////////////////////////////////////////////////////////////////////////

// The Whirlpool algorithm was developed by
// Paulo S. L. M. Barreto and Vincent Rijmen</a>.
//
// This implementation is based on the reference implementation found at
// http://www.larc.usp.br/~pbarreto/whirlpool.zip
// .. and moved from C to C#

// Read more at
// http://en.wikipedia.org/wiki/Whirlpool_(cryptography)
// http://de.wikipedia.org/wiki/Whirlpool_(Algorithmus) 
// http://www.larc.usp.br/~pbarreto/WhirlpoolPage.html


using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Security.Cryptography;

namespace Whirlpool
{
    [Author("Gerhard Junker", null, "private project member", null)]
    [PluginInfo("Whirlpool.Properties.Resources", "PluginCaption", "PluginTooltip", "Whirlpool/DetailedDescription/doc.xml", "Whirlpool/Whirlpool1.png")]
    [ComponentCategory(ComponentCategory.HashFunctions)]
    public class WPHash : ICrypComponent
    {

        /// <summary>
        /// can only handle one input canal
        /// </summary>
        private enum dataCanal
        {
            /// <summary>
            /// nothing assigned
            /// </summary>
            none,
            /// <summary>
            /// using stream interface
            /// </summary>
            streamCanal,
            /// <summary>
            /// using byte array interface
            /// </summary>
            byteCanal
        };

        private WhirlpoolSettings whirlpoolSetting = new WhirlpoolSettings();

        /// <summary>
        /// Initializes a new instance of the <see cref="WPHash"/> class.
        /// </summary>
        public WPHash()
        {
        }

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        /// <value>The settings.</value>
        public ISettings Settings
        {
            get => whirlpoolSetting;
            set
            {
                whirlpoolSetting = (WhirlpoolSettings)value;
                OnPropertyChanged("Settings");
                GuiLogMessage("Settings changed.", NotificationLevel.Debug);
            }

        }


        #region Input data

        // Input input
        private static readonly byte[] empty = { };
        private byte[] inputData = empty;
        //private dataCanal inputCanal = dataCanal.none;

        /// <summary>
        /// Notifies the update input.
        /// </summary>
        private void NotifyUpdateInput()
        {
            OnPropertyChanged("InputStream");
            OnPropertyChanged("InputData");
        }

        /// <summary>
        /// Gets or sets the input data.
        /// </summary>
        /// <value>The input input.</value>
        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputStreamTooltip", false)]
        public ICrypToolStream InputStream
        {
            get
            {
                if (inputData == null)
                {
                    return null;
                }
                else
                {
                    return new CStreamWriter(inputData);
                }
            }
            set
            {
                if (value != null)
                {
                    using (CStreamReader reader = value.CreateReader())
                    {
                        inputData = reader.ReadFully();
                        GuiLogMessage("InputStream changed.", NotificationLevel.Debug);
                    }

                    NotifyUpdateInput();
                }
            }
        }

        /// <summary>
        /// Gets the input data.
        /// </summary>
        /// <value>The input data.</value>
        [PropertyInfo(Direction.InputData, "InputDataCaption", "InputDataTooltip", false)]
        public byte[] InputData
        {
            get => inputData;
            set
            {
                if (inputData != value)
                {
                    inputData = (value == null) ? empty : value;
                    GuiLogMessage("InputData changed.", NotificationLevel.Debug);
                    NotifyUpdateInput();
                }
            }
        }
        #endregion

        #region Output

        // Output
        private byte[] outputData = empty;

        /// <summary>
        /// Notifies the update output.
        /// </summary>
        private void NotifyUpdateOutput()
        {
            OnPropertyChanged("HashOutputStream");
            OnPropertyChanged("HashOutputData");
        }


        /// <summary>
        /// Gets or sets the output data stream.
        /// </summary>
        /// <value>The output data stream.</value>
        [PropertyInfo(Direction.OutputData, "HashOutputStreamCaption", "HashOutputStreamTooltip", true)]
        public ICrypToolStream HashOutputStream => new CStreamWriter(outputData);

        /// <summary>
        /// Gets the output data.
        /// </summary>
        /// <value>The output data.</value>
        [PropertyInfo(Direction.OutputData, "HashOutputDataCaption", "HashOutputDataTooltip", true)]
        public byte[] HashOutputData => outputData;

        /// <summary>
        /// Hashes this instance.
        /// </summary>
        public void Hash()
        {
            HMACWHIRLPOOL wh = new HMACWHIRLPOOL();
            wh.Initialize();
            outputData = wh.ComputeHash(inputData);
            wh = null;
            NotifyUpdateOutput();
        }
        #endregion

        #region IPlugin Member

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
#pragma warning restore

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }


        /// <summary>
        /// Provide all presentation stuff in this user control, it will be opened in an tab.
        /// Return null if your plugin has no presentation.
        /// </summary>
        /// <value>The presentation.</value>
        public System.Windows.Controls.UserControl Presentation => null;

        /// <summary>
        /// Will be called from editor after restoring settings and before adding to workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Will be called from editor before right before chain-run starts
        /// </summary>
        public void PreExecution()
        {
        }

        /// <summary>
        /// Will be called from editor while chain-run is active and after last necessary input
        /// for plugin has been set.
        /// </summary>
        public void Execute()
        {
            Progress(0.0, 1.0);
            Hash();
            Progress(1.0, 1.0);
        }

        /// <summary>
        /// Will be called from editor after last plugin in chain has finished its work.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Will be called from editor while chain-run is active. Plugin hast to stop work immediately.
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// Will be called from editor when element is deleted from worksapce.
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region INotifyPropertyChanged Member

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="name">The name.</param>
        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// GUIs the log message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="logLevel">The log level.</param>
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        #endregion
    }
}
