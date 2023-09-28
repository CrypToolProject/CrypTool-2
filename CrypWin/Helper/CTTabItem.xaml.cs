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
using CrypTool.PluginBase;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace CrypTool.CrypWin.Helper
{
    //<summary>
    //Interaction logic for CTTabItem.xaml
    //</summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.CrypWin.Properties.Resources")]
    public partial class CTTabItem : TabItem
    {
        public event EventHandler RequestBigViewFrame;
        public event EventHandler RequestHideMenuOnOffEvent;
        public event EventHandler RequestDistractionFreeOnOffEvent;

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
            "Icon",
            typeof(ImageSource),
            typeof(CTTabItem),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, null));

        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty HasChangesProperty =
            DependencyProperty.Register(
            "HasChanges",
            typeof(bool),
            typeof(CTTabItem),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool HasChanges
        {
            get => (bool)GetValue(HasChangesProperty);
            set => SetValue(HasChangesProperty, value);
        }

        public static readonly DependencyProperty HeaderTooltipProperty =
            DependencyProperty.Register(
            "HeaderTooltip",
            typeof(FrameworkElement),
            typeof(CTTabItem),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public FrameworkElement HeaderTooltip
        {
            get => (FrameworkElement)GetValue(HeaderTooltipProperty);
            set => SetValue(HeaderTooltipProperty, value);
        }

        public static readonly DependencyProperty IsExecutingProperty =
            DependencyProperty.Register(
            "IsExecuting",
            typeof(bool?),
            typeof(CTTabItem),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool? IsExecuting
        {
            get => (bool?)GetValue(IsExecutingProperty);
            set => SetValue(IsExecutingProperty, value);
        }

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(
            "Progress",
            typeof(double),
            typeof(CTTabItem),
            new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        public delegate void OnCloseHandler();
        public event OnCloseHandler OnClose;

        public CTTabItem()
        {
            InitializeComponent();
        }

        public CTTabItem(PluginBase.Editor.IEditor parent)
        {
            Editor = parent;
            InitializeComponent();

            Editor.HasBeenClosed = false;
        }

        public CTTabItem(TabInfo info)
        {
            Info = info;
            InitializeComponent();
        }

        public void Close()
        {
            if (OnClose != null)
            {
                OnClose();
            }

            if (Editor != null)
            {
                Editor.HasBeenClosed = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            TabControl tabctrl = (TabControl)Parent;
            System.Collections.Generic.List<CTTabItem> list = tabctrl.Items.Cast<CTTabItem>().ToList();
            foreach (CTTabItem o in list)
            {
                o.Close();
            }
        }

        private void DockPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                Close();
            }

            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2)
            {
                RequestBigViewFrame.Invoke(this, new EventArgs());
            }
        }

        private void Button_Click3(object sender, RoutedEventArgs e)
        {
            TabControl tabctrl = (TabControl)Parent;
            System.Collections.Generic.IEnumerable<CTTabItem> list = tabctrl.Items.Cast<CTTabItem>().ToList().Where(a => a != this);

            foreach (CTTabItem o in list)
            {
                o.Close();
            }
        }

        private PluginBase.Editor.IEditor editor;
        public PluginBase.Editor.IEditor Editor
        {
            get => editor;
            set
            {
                editor = value;
                if (editor is WorkspaceManager.WorkspaceManagerClass)
                {
                    WorkspaceManager.WorkspaceManagerClass workspaceManagerClass = editor as WorkspaceManager.WorkspaceManagerClass;
                    IsExecuting = false;
                    WorkspaceManager.View.Visuals.EditorVisual y = (WorkspaceManager.View.Visuals.EditorVisual)workspaceManagerClass.Presentation;

                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        Binding bind = new Binding
                        {
                            Path = new PropertyPath(WorkspaceManager.View.Visuals.EditorVisual.ProgressProperty),
                            Source = y
                        };
                        SetBinding(CTTabItem.ProgressProperty, bind);
                    }, null);

                    workspaceManagerClass.executeEvent += new EventHandler(workspaceManager_executeEvent);
                }
            }
        }

        private void workspaceManager_executeEvent(object sender, EventArgs e)
        {
            WorkspaceManager.WorkspaceManagerClass workspaceManagerClass = editor as WorkspaceManager.WorkspaceManagerClass;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                IsExecuting = workspaceManagerClass.isExecuting();
            }, null);
        }

        private void CopyToClipboard(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Header.ToString());
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (RequestHideMenuOnOffEvent != null)
            {
                RequestHideMenuOnOffEvent.Invoke(this, null);
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (RequestDistractionFreeOnOffEvent != null)
            {
                RequestDistractionFreeOnOffEvent.Invoke(this, null);
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            if (info != null && info.CopyText != null)
            {
                Clipboard.SetText(info.CopyText);
            }
        }

        private TabInfo info;
        public TabInfo Info
        {
            get => info;
            set
            {
                if (value == null)
                {
                    return;
                }

                if (info == null)
                {
                    info = new TabInfo();
                }

                if (value.Icon != null)
                {
                    Icon = value.Icon;
                    info.Icon = value.Icon;
                }

                if (value.Title != null)
                {
                    Header = value.Title.Replace("_", " ");
                    info.Title = value.Title;
                }

                if (value.Tooltip != null)
                {
                    HeaderTooltip = new TextBlock(value.Tooltip) { MaxWidth = 400, TextWrapping = TextWrapping.Wrap };
                    info.Tooltip = value.Tooltip;
                }

                if (value.Filename != null)
                {
                    info.Filename = value.Filename;
                }

            }
        }

        internal void SetTabInfo(TabInfo info)
        {
            Info = info;
        }
    }
}
