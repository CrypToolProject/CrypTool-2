/*
   Copyright 2009 Sören Rinne, Ruhr-Universität Bochum, Germany

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

using System;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CrypTool.CLK
{
    /// <summary>
    /// Interaction logic for CLKPresentation.xaml
    /// </summary>
    public partial class CLKPresentation : UserControl
    {
        public CLKPresentation()
        {
            InitializeComponent();
            Height = double.NaN;
            Width = double.NaN;
        }

        public void setImageTrue()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                BitmapImage png = new BitmapImage();
                png.BeginInit();
                png.UriSource = new Uri("Images/true.png", UriKind.Relative);
                png.EndInit();
                CLKButtonImage.Source = png;
            }, null);
        }

        public void setImageFalse()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                BitmapImage png = new BitmapImage();
                png.BeginInit();
                png.UriSource = new Uri("Images/false.png", UriKind.Relative);
                png.EndInit();
                CLKButtonImage.Source = png;
            }, null);
        }
    }
}
