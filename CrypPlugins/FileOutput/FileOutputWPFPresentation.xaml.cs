using FileOutput;
using System;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace FileOutputWPF
{
    /// <summary>
    /// Interaction logic for FileOutputWPFPresentation.xaml
    /// </summary>
    public partial class FileOutputWPFPresentation : UserControl
    {
        private readonly FileOutputClass exp;
        public HexBox.HexBox hexBox;

        public FileOutputWPFPresentation(FileOutputClass exp)
        {
            InitializeComponent();
            this.exp = exp;
            SizeChanged += sizeChanged;
            hexBox = new HexBox.HexBox
            {
                InReadOnlyMode = true
            };
            hexBox.OnFileChanged += fileChanged;
            hexBox.ErrorOccured += new HexBox.HexBox.GUIErrorEventHandler(hexBox_ErrorOccured);

            MainMain.Children.Add(hexBox);
            hexBox.collapseControl(false);
        }

        private void hexBox_ErrorOccured(object sender, HexBox.GUIErrorEventArgs ge)
        {
            exp.getMessage(ge.Message);
        }

        public void CloseFileToGetFileStreamForExecution()
        {
            hexBox.closeFile(false);
        }

        public void Clear()
        {
            hexBox.Clear();
        }

        public void ReopenClosedFile()
        {
            if (File.Exists((exp.Settings as FileOutputSettings).TargetFilename))
            {
                hexBox.closeFile(false);
                hexBox.openFile((exp.Settings as FileOutputSettings).TargetFilename, true);
                hexBox.collapseControl(false);
            }
        }


        internal void OpenFile(string fileName)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                hexBox.openFile(fileName, true);
            }, null);
        }

        internal void dispose()
        {
            hexBox.dispose();
        }

        private void fileChanged(object sender, EventArgs eventArgs)
        {
        }

        private void sizeChanged(object sender, EventArgs eventArgs)
        {
            hexBox.Width = ActualWidth;
            hexBox.Height = ActualHeight;
        }

        internal void CloseFile()
        {
        }
    }
}