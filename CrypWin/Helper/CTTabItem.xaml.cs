using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading;
using CrypTool.PluginBase;

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
            get { return (ImageSource)GetValue(IconProperty); }
            set
            {
                SetValue(IconProperty, value);
            }
        }

        public static readonly DependencyProperty HasChangesProperty =
            DependencyProperty.Register(
            "HasChanges",
            typeof(bool),
            typeof(CTTabItem),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool HasChanges
        {
            get { return (bool)GetValue(HasChangesProperty); }
            set
            {
                SetValue(HasChangesProperty, value);
            }
        }

        public static readonly DependencyProperty HeaderTooltipProperty =
            DependencyProperty.Register(
            "HeaderTooltip",
            typeof(FrameworkElement),
            typeof(CTTabItem),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public FrameworkElement HeaderTooltip
        {
            get { return (FrameworkElement)GetValue(HeaderTooltipProperty); }
            set
            {
                SetValue(HeaderTooltipProperty, value);
            }
        }

        public static readonly DependencyProperty IsExecutingProperty =
            DependencyProperty.Register(
            "IsExecuting",
            typeof(bool?),
            typeof(CTTabItem),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool? IsExecuting
        {
            get { return (bool?)GetValue(IsExecutingProperty); }
            set
            {
                SetValue(IsExecutingProperty, value);
            }
        }

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(
            "Progress",
            typeof(double),
            typeof(CTTabItem),
            new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double Progress
        {
            get { return (double)GetValue(ProgressProperty); }
            set
            {
                SetValue(ProgressProperty, value);
            }
        }

        public delegate void OnCloseHandler();
        public event OnCloseHandler OnClose;

        public CTTabItem()
        {
            InitializeComponent();
        }

        public CTTabItem(PluginBase.Editor.IEditor parent)
        {
            this.Editor = parent;
            InitializeComponent();

            this.Editor.HasBeenClosed = false;
        }

        public CTTabItem(TabInfo info)
        {
            // TODO: Complete member initialization
            this.Info = null;
            this.Info = info;
            InitializeComponent();
        }

        public void Close()
        {
            if (OnClose != null) 
                OnClose();
            if (this.Editor != null)
            {
                this.Editor.HasBeenClosed = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            TabControl tabctrl = (TabControl)this.Parent;
            var list = tabctrl.Items.Cast<CTTabItem>().ToList();
            foreach (CTTabItem o in list)
                o.Close();
        }

        private void DockPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
                Close();
            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2)
                RequestBigViewFrame.Invoke(this, new EventArgs());
        }

        private void Button_Click3(object sender, RoutedEventArgs e)
        {
            TabControl tabctrl = (TabControl)this.Parent;
            var list = tabctrl.Items.Cast<CTTabItem>().ToList().Where(a => a != this);

            foreach (CTTabItem o in list)
                o.Close();
        }

        private PluginBase.Editor.IEditor editor;
        public PluginBase.Editor.IEditor Editor 
        {
            get 
            { 
                return editor; 
            }
            set 
            { 
                editor = value; 
                if(editor is WorkspaceManager.WorkspaceManagerClass)
                {
                    var x = editor as WorkspaceManager.WorkspaceManagerClass;
                    IsExecuting = false;
                    var y = (WorkspaceManager.View.Visuals.EditorVisual)x.Presentation;

                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        Binding bind = new Binding();
                        bind.Path = new PropertyPath(WorkspaceManager.View.Visuals.EditorVisual.ProgressProperty);
                        bind.Source = y;
                        this.SetBinding(CTTabItem.ProgressProperty, bind);
                    }, null);

                    x.executeEvent +=new EventHandler(x_executeEvent);
                }
            }
        }

        void  x_executeEvent(object sender, EventArgs e)
        {
            var x = editor as WorkspaceManager.WorkspaceManagerClass;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                IsExecuting = x.isExecuting();
            }, null);
        }

        private void CopyToClipboard(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Header.ToString());
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (RequestHideMenuOnOffEvent != null)
                RequestHideMenuOnOffEvent.Invoke(this, null);
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (RequestDistractionFreeOnOffEvent != null)
                RequestDistractionFreeOnOffEvent.Invoke(this, null);
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
            get 
            {
                return info; 
            }
            set 
            {
                if (value == null)
                    return;

                if (info == null)
                    info = new TabInfo();

                if (value.Icon != null) 
                {
                    this.Icon = value.Icon;
                    info.Icon = value.Icon;
                }

                if (value.Title != null)
                {
                    this.Header = value.Title.Replace("_", " ");
                    info.Title = value.Title;
                }

                if (value.Tooltip != null)
                {
                    this.HeaderTooltip = new TextBlock(value.Tooltip) { MaxWidth = 400, TextWrapping = TextWrapping.Wrap };
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
            this.Info = info;
        }
    }
}
