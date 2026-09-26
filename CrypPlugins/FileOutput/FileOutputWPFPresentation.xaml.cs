/*
   Copyright (C) CrypTool 2 Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
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