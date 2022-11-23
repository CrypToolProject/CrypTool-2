using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace FileInput
{
    /// <summary>
    /// Interaction logic for FileInputWPFPresentation.xaml
    /// </summary>
    public partial class FileInputWPFPresentation : UserControl
    {
        private readonly FileInputClass exp;
        public HexBox.HexBox hexBox;

        public FileInputWPFPresentation(FileInputClass exp)
        {
            InitializeComponent();
            this.exp = exp;
            SizeChanged += sizeChanged;
            hexBox = new HexBox.HexBox();
            hexBox.OnFileChanged += fileChanged;
            MainMain.Children.Add(hexBox);
            hexBox.ErrorOccured += new HexBox.HexBox.GUIErrorEventHandler(hexBox_ErrorOccured);
        }

        private void hexBox_ErrorOccured(object sender, HexBox.GUIErrorEventArgs ge)
        {
            exp.getMessage(ge.Message);
        }

        public void makeUnaccesAble(bool b)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                hexBox.makeUnAccesable(b);
                hexBox.IsEnabled = b;
                hexBox.IsManipulationEnabled^= b;
            }, null);
        }

        public void CloseFileToGetFileStreamForExecution()
        {
            try
            {
                hexBox.saveData(true, false);
                hexBox.closeFile(false);
                hexBox.openFile((exp.Settings as FileInputSettings).OpenFilename, true);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Error trying to reopen file: {0}", ex), NotificationLevel.Error);
            }
        }

        public void ReopenClosedFile()
        {
            if (File.Exists((exp.Settings as FileInputSettings).OpenFilename))
            {
                hexBox.closeFile(false);
                hexBox.openFile((exp.Settings as FileInputSettings).OpenFilename, false);
            }
        }


        internal void OpenFile(string fileName)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
           {
               try
               {
                   hexBox.openFile(fileName, false);
               }
               catch (Exception ex)
               {
                   GuiLogMessage(string.Format("Error trying to open file: {0}", ex), NotificationLevel.Error);
               }
           }, null);
        }

        public void dispose()
        {
            hexBox.dispose();
        }

        private void fileChanged(object sender, EventArgs eventArgs)
        {
            (exp.Settings as FileInputSettings).OpenFilename = hexBox.Path;
        }

        private void sizeChanged(object sender, EventArgs eventArgs)
        {
            hexBox.Width = ActualWidth;
            hexBox.Height = ActualHeight;
        }

        internal void CloseFile()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                hexBox.closeFile(true);
            }, null);
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, exp, new GuiLogEventArgs(message, exp, logLevel));
        }
    }
}