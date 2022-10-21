/*
   Copyright 2008 - 2022 CrypTool Team

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
using CrypCloud.Core;
using CrypTool.PluginBase;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace CrypTool.CrypWin
{
    public partial class MainWindow
    {

        public static readonly DependencyProperty P2PButtonVisibilityProperty =
            DependencyProperty.Register("P2PButtonVisibility", typeof(Visibility), typeof(MainWindow),
                new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.AffectsRender, null));

        private void InitCloud()
        {
            try
            {
                CrypCloudCore.Instance.ConnectionStateChanged += UpdateIcons;
                UpdateIcons(connected: false);
                GuiLogMessage("Cloud initialized", NotificationLevel.Debug);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during initialization of cloud: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        [TypeConverter(typeof(Visibility))]
        public Visibility P2PButtonVisibility
        {
            get => (Visibility)GetValue(P2PButtonVisibilityProperty);
            set => SetValue(P2PButtonVisibilityProperty, value);
        }

        private void UpdateIcons(bool connected)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
           {
               string iconSource = connected ? "cloudConnected" : "cloudDisconnected";
               P2PIconImageBig.Source = (ImageSource)FindResource(iconSource);
               P2PIconImageSmall.Source = (ImageSource)FindResource(iconSource);
           }, null);
        }

    }
}
