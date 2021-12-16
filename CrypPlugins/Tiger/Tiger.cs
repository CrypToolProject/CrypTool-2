//////////////////////////////////////////////////////////////////////////////////////////////////
// CrypTool V2
// © 2008 - Gerhard Junker
// Apache License see http://www.apache.org/licenses/
//
// $HeadURL: https://svn.cryptool.org/CrypTool2/trunk/CrypPlugins/Tiger/Tiger.cs $
//////////////////////////////////////////////////////////////////////////////////////////////////
// $Revision:: 8983                                                                           $://
// $Author:: kopal                                                                            $://
// $Date:: 2021-03-24 14:51:34 +0100 (Mi, 24 Mrz 2021)                                        $://
//////////////////////////////////////////////////////////////////////////////////////////////////


// read more about Tiger
//
// http://en.wikipedia.org/wiki/Tiger_(cryptography)
// http://de.wikipedia.org/wiki/Tiger_(Hashfunktion)
// http://www.cs.technion.ac.il/~biham/Reports/Tiger/
//
// based first on an VisualBasic implementation of Markus Hahn - Thanks.
// from http://www.hotpixel.net/software.html
// and changed to fit more the published algorithm

using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Security.Cryptography;


namespace Tiger
{

    [Author("Gerhard Junker", null, null, null)]
    [PluginInfo("Tiger.Properties.Resources", "PluginCaption", "PluginTooltip", "Tiger/DetailedDescription/doc.xml", "Tiger/Tiger1.png")]
    [ComponentCategory(ComponentCategory.HashFunctions)]
    public class Tiger : ICrypComponent
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

        #region Settings

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        /// <value>The settings.</value>
        public ISettings Settings => null;

        #endregion

        #region Input inputdata / password

        // Input inputdata
        private byte[] inputdata = { };
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
        /// Gets or sets the input inputdata.
        /// </summary>
        /// <value>The input inputdata.</value>
        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputStreamTooltip", false)]
        public ICrypToolStream InputStream
        {
            get
            {
                if (inputdata == null)
                {
                    return null;
                }
                else
                {
                    return new CStreamWriter(inputdata);
                }
            }
            set
            {
                if (value != null)
                {
                    using (CStreamReader reader = value.CreateReader())
                    {
                        inputdata = reader.ReadFully();
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
            get => inputdata;
            set
            {
                if (inputdata != value)
                {
                    inputdata = (value == null) ? new byte[0] : value;
                    GuiLogMessage("InputData changed.", NotificationLevel.Debug);
                    NotifyUpdateInput();
                }
            }
        }
        #endregion

        #region Output

        // Output
        private byte[] outputData = { };

        /// <summary>
        /// Notifies the update output.
        /// </summary>
        private void NotifyUpdateOutput()
        {
            OnPropertyChanged("HashOutputStream");
            OnPropertyChanged("HashOutputData");
        }


        /// <summary>
        /// Gets or sets the output inputdata stream.
        /// </summary>
        /// <value>The output inputdata stream.</value>
        [PropertyInfo(Direction.OutputData, "HashOutputStreamCaption", "HashOutputStreamTooltip", true)]
        public ICrypToolStream HashOutputStream => new CStreamWriter(outputData);

        /// <summary>
        /// Gets the output inputdata.
        /// </summary>
        /// <value>The output inputdata.</value>
        [PropertyInfo(Direction.OutputData, "HashOutputDataCaption", "HashOutputDataTooltip", true)]
        public byte[] HashOutputData
        {
            get
            {
                GuiLogMessage("Got HashOutputData.", NotificationLevel.Debug);
                return outputData;
            }
        }

        #endregion

        private void Hash()
        {
            HMACTIGER2 tg = new HMACTIGER2();
            outputData = tg.ComputeHash(inputdata);
            NotifyUpdateOutput();
        }

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
        /// Will be called from editor after restoring settings and before adding to workspace.
        /// </summary>
        public void Initialize()
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

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="name">The name.</param>
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                if (name == "Settings")
                {
                    Hash();
                }
                else
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(name));
                }
            }
        }


        /// <summary>
        /// GUIs the log message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="logLevel">The log level.</param>
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this,
              new GuiLogEventArgs(message, this, logLevel));
        }

        #endregion
    }
}
