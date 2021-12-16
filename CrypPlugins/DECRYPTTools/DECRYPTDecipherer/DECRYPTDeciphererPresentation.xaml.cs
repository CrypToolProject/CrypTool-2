/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.Plugins.DECRYPTTools.Util;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace CrypTool.Plugins.DECRYPTTools
{
    /// <summary>
    /// Interaktionslogik für DECRYPTTextParserPresentation.xaml
    /// </summary>
    public partial class DECRYPTDeciphererPresentation : UserControl
    {
        public TextDocument CurrentTextDocument
        {
            get => (TextDocument)GetValue(CurrentTextDocumentProperty);
            set => SetValue(CurrentTextDocumentProperty, value);
        }
        public static readonly DependencyProperty CurrentTextDocumentProperty = DependencyProperty.Register(
          nameof(CurrentTextDocument), typeof(TextDocument), typeof(DECRYPTDeciphererPresentation), new PropertyMetadata(null));

        public DECRYPTDeciphererPresentation()
        {
            DataContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// Shows the given document in the user interface
        /// </summary>
        /// <param name="document"></param>
        public void ShowDocument(TextDocument document)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                CurrentTextDocument = document;
            }, null);
        }

        /// <summary>
        /// Forward MouseWheel event to parent ui element (ScrollViewer) 
        /// to enable the user to scroll through the image list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageList_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                MouseWheelEventArgs eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = UIElement.MouseWheelEvent,
                    Source = sender
                };
                UIElement parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        protected void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, null, new GuiLogEventArgs(message, null, logLevel));
        }
    }
}
