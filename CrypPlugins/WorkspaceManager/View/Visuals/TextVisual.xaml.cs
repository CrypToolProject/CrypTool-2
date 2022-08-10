/*                              
   Copyright 2010-2022 Nils Kopal, Viktor M.

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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WorkspaceManager.Model;
using WorkspaceManager.View.Base.Interfaces;
using WorkspaceManagerModel.Model.Interfaces;
using WorkspaceManagerModel.Model.Operations;

namespace WorkspaceManager.View.Visuals
{
    /// <summary>
    /// Interaction logic for BinTextVisual.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("WorkspaceManager.Properties.Resources")]
    public partial class TextVisual : UserControl, IUpdateableView, IRouting
    {
        #region Properties
        private TextModel model;
        public TextModel Model { get => model; private set { model = value; Model.UpdateableView = this; } }
        #endregion

        #region DependencyProperties

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected",
            typeof(bool), typeof(TextVisual), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsSelectedChanged)));

        public bool IsSelected
        {
            get => (bool)base.GetValue(IsSelectedProperty);
            set => base.SetValue(IsSelectedProperty, value);
        }

        public static readonly DependencyProperty WindowHeightProperty = DependencyProperty.Register("WindowHeight",
            typeof(double), typeof(TextVisual), new FrameworkPropertyMetadata(double.Epsilon));

        public double WindowHeight
        {
            get => (double)base.GetValue(WindowHeightProperty);
            set
            {
                if (value < 0)
                {
                    return;
                }

                base.SetValue(WindowHeightProperty, value);
            }
        }

        public static readonly DependencyProperty WindowWidthProperty = DependencyProperty.Register("WindowWidth",
            typeof(double), typeof(TextVisual), new FrameworkPropertyMetadata(double.Epsilon));

        public double WindowWidth
        {
            get => (double)base.GetValue(WindowWidthProperty);
            set
            {
                if (value < 0)
                {
                    return;
                }

                base.SetValue(WindowWidthProperty, value);
            }
        }

        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position",
            typeof(Point), typeof(TextVisual), new FrameworkPropertyMetadata(new Point(0, 0), new PropertyChangedCallback(OnPositionValueChanged)));

        public Point Position
        {
            get => (Point)base.GetValue(PositionProperty);
            set => base.SetValue(PositionProperty, value);
        }

        public static readonly DependencyProperty IsLockedProperty = DependencyProperty.Register("IsLocked",
            typeof(bool), typeof(TextVisual), new FrameworkPropertyMetadata(false, null));

        public bool IsLocked
        {
            get => (bool)base.GetValue(IsLockedProperty);
            set => base.SetValue(IsLockedProperty, value);
        }

        public static readonly DependencyProperty WindowNameProperty = DependencyProperty.Register("WindowName",
            typeof(string), typeof(TextVisual), new FrameworkPropertyMetadata(Properties.Resources.Enter_Name, null));

        public string WindowName
        {
            get => (string)base.GetValue(WindowNameProperty);
            set => base.SetValue(WindowNameProperty, value);
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color",
            typeof(SolidColorBrush), typeof(TextVisual), new FrameworkPropertyMetadata(Brushes.White, new PropertyChangedCallback(OnColorValueChanged)));

        public SolidColorBrush Color
        {
            get => (SolidColorBrush)base.GetValue(ColorProperty);
            set => base.SetValue(ColorProperty, value);
        }

        #endregion

        #region Constructors

        public TextVisual(TextModel model)
        {
            InitializeComponent();
            WindowWidth = model.GetWidth();
            WindowHeight = model.GetHeight();
            Position = model.GetPosition();
            if (model.BackgroundColor != null)
            {
                Color = new SolidColorBrush(model.BackgroundColor);
            }

            Model = model;
            Model.loadRTB(mainRTB);
            Model.UpdateableView = this;
        }
        #endregion

        #region Event Handler
        private static void OnPositionValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private static void OnColorValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            TextVisual bin = (TextVisual)d;
            if (bin.Model != null)
            {
                bin.Model.BackgroundColor = bin.Color.Color;
            }
        }

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextVisual bin = (TextVisual)d;
            bin.mainRTB.Focusable = bin.IsSelected;
            if (bin.IsSelected)
            {
                bin.mainRTB.Loaded += new RoutedEventHandler(mainRTB_Loaded);
            }
        }

        private static void mainRTB_Loaded(object sender, RoutedEventArgs e)
        {
            FocusHelper.Focus((UIElement)sender);
        }

        protected virtual void CloseClick(object sender, RoutedEventArgs e)
        {
            if (Model != null && !((WorkspaceManagerClass)Model.WorkspaceModel.MyEditor).isExecuting())
            {
                Model.WorkspaceModel.ModifyModel(new DeleteTextModelOperation(Model));
            }
        }


        private void LockHandler(object sender, RoutedEventArgs e)
        {
            IsLocked = !IsLocked;
            e.Handled = true;
        }

        private void ScaleDragDeltaHandler(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            WindowHeight += e.VerticalChange;
            WindowWidth += e.HorizontalChange;
            Model.WorkspaceModel.ModifyModel(new ResizeModelElementOperation(Model, WindowWidth, WindowHeight));
            e.Handled = true;
        }

        private void PositionDragDeltaHandler(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Vector delta = new Vector(e.HorizontalChange, e.VerticalChange);
            if (PositionDeltaChanged != null)
            {
                PositionDeltaChanged.Invoke(this, new PositionDeltaChangedArgs() { PosDelta = delta, Model = Model });
            }
        }

        public void update()
        {

        }

        public void updateStatus()
        {

        }
        private void TextChangedHandler(object sender, TextChangedEventArgs e)
        {
            Model.saveRTB((RichTextBox)sender);
        }
        #endregion

        public ObjectSize ObjectSize => throw new NotImplementedException();

        public Point[] RoutingPoints => throw new NotImplementedException();

        public event EventHandler<PositionDeltaChangedArgs> PositionDeltaChanged;


        public Point GetRoutingPoint(int routPoint)
        {
            throw new NotImplementedException();
        }
    }

    internal static class FocusHelper
    {
        private delegate void MethodInvoker();

        public static void Focus(UIElement element)
        {
            //Focus in a callback to run on another thread, ensuring the main UI thread is initialized by the
            //time focus is set
            ThreadPool.QueueUserWorkItem(delegate (object foo)
            {
                UIElement elem = (UIElement)foo;
                elem.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                    (MethodInvoker)delegate ()
                    {
                        elem.Focus();
                        Keyboard.Focus(elem);
                    });
            }, element);
        }
    }
}
