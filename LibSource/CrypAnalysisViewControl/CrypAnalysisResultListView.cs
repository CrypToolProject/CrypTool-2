/*
   Copyright 2021 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CrypTool.CrypAnalysisViewControl
{
    public partial class CrypAnalysisResultListView : ListView
    {
        public static RoutedCommand ClickContextMenuCopyValue = new RoutedCommand("ClickContextMenuCopyValue", typeof(RoutedCommand));
        public static RoutedCommand ClickContextMenuCopyKey = new RoutedCommand("ClickContextMenuCopyKey", typeof(RoutedCommand));
        public static RoutedCommand ClickContextMenuCopyText = new RoutedCommand("ClickContextMenuCopyText", typeof(RoutedCommand));
        public static RoutedCommand ClickContextMenuCopyLine = new RoutedCommand("ClickContextMenuCopyLine", typeof(RoutedCommand));
        public static RoutedCommand ClickContextMenuCopyAll = new RoutedCommand("ClickContextMenuCopyAll", typeof(RoutedCommand));

        public delegate void ResultItemActionHandler(ICrypAnalysisResultListEntry item);
        public event ResultItemActionHandler ResultItemAction;

        public CrypAnalysisResultListView()
        {
            CommandBindings.Add(new CommandBinding(ClickContextMenuCopyValue, HandleContextMenuCopyValue));
            CommandBindings.Add(new CommandBinding(ClickContextMenuCopyKey, HandleContextMenuCopyKey));
            CommandBindings.Add(new CommandBinding(ClickContextMenuCopyText, HandleContextMenuCopyText));
            CommandBindings.Add(new CommandBinding(ClickContextMenuCopyLine, HandleContextMenuCopyLine));
            CommandBindings.Add(new CommandBinding(ClickContextMenuCopyAll, HandleContextMenuCopyAll));

            Loaded += CrypAnalysisResultListView_Loaded;
        }

        private void CrypAnalysisResultListView_Loaded(object sender, RoutedEventArgs e)
        {
            //Add mouse double click event handler to item style:
            Style itemContainerStyle = new Style(typeof(ListViewItem), ItemContainerStyle);
            itemContainerStyle.Setters.Add(new EventSetter(MouseDoubleClickEvent, new MouseButtonEventHandler(ItemDoubleClickHandler)));
            itemContainerStyle.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
            ItemContainerStyle = itemContainerStyle;
        }

        private void ItemDoubleClickHandler(object sender, MouseButtonEventArgs e)
        {
            ListViewItem viewItem = sender as ListViewItem;
            if (viewItem?.Content is ICrypAnalysisResultListEntry item)
            {
                ResultItemAction?.Invoke(item);
            }
        }

        private ICrypAnalysisResultListEntry GetCurrentEntry(EventArgs eventArgs)
        {
            return (eventArgs as ExecutedRoutedEventArgs)?.Parameter as ICrypAnalysisResultListEntry;
        }

        private void HandleContextMenuCopyValue(object sender, EventArgs eventArgs)
        {
            SetClipboard(GetCurrentEntry(eventArgs)?.ClipboardValue ?? "");
        }

        private void HandleContextMenuCopyKey(object sender, EventArgs eventArgs)
        {
            SetClipboard(GetCurrentEntry(eventArgs)?.ClipboardKey ?? "");
        }

        private void HandleContextMenuCopyText(object sender, EventArgs eventArgs)
        {
            SetClipboard(GetCurrentEntry(eventArgs)?.ClipboardText ?? "");
        }

        private void HandleContextMenuCopyLine(object sender, EventArgs eventArgs)
        {
            SetClipboard(GetCurrentEntry(eventArgs)?.ClipboardEntry ?? "");
        }

        private void HandleContextMenuCopyAll(object sender, EventArgs eventArgs)
        {
            CrypAnalysisResultListView listView = sender as CrypAnalysisResultListView;
            System.Collections.Generic.IEnumerable<string> entryStrings = listView?.Items.OfType<ICrypAnalysisResultListEntry>().Select(item => item.ClipboardEntry);
            if (entryStrings != null)
            {
                SetClipboard(string.Join(Environment.NewLine, entryStrings));
            }
        }

        private void SetClipboard(string text)
        {
            text = text.Replace(Convert.ToChar(0x0).ToString(), "");    //Remove null chars in order to avoid problems in clipboard
            Clipboard.SetText(text);
        }
    }
}
