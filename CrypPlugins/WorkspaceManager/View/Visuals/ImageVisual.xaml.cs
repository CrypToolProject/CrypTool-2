using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WorkspaceManager.Model;
using WorkspaceManagerModel.Model.Operations;
using WorkspaceManagerModel.Model.Interfaces;
using WorkspaceManager.View.Base.Interfaces;

namespace WorkspaceManager.View.Visuals
{
    /// <summary>
    /// Interaction logic for BinWindowVisual.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("WorkspaceManager.Properties.Resources")]
    public partial class ImageVisual : UserControl, IUpdateableView, IRouting
    {
        #region Properties
        private ImageModel model;
        public ImageModel Model { get { return model; } private set { model = value; Model.UpdateableView = this; } }
        #endregion

        #region DependencyProperties

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected",
            typeof(bool), typeof(ImageVisual), new FrameworkPropertyMetadata(false));

        public bool IsSelected
        {
            get { return (bool)base.GetValue(IsSelectedProperty); }
            set
            {
                base.SetValue(IsSelectedProperty, value);
            }
        }

        public static readonly DependencyProperty WindowHeightProperty = DependencyProperty.Register("WindowHeight",
            typeof(double), typeof(ImageVisual), new FrameworkPropertyMetadata(double.Epsilon));

        public double WindowHeight
        {
            get
            {
                return (double)base.GetValue(WindowHeightProperty);
            }
            set
            {
                if (value < 0)
                    return;

                base.SetValue(WindowHeightProperty, value);
            }
        }

        public static readonly DependencyProperty WindowWidthProperty = DependencyProperty.Register("WindowWidth",
            typeof(double), typeof(ImageVisual), new FrameworkPropertyMetadata(double.Epsilon));

        public double WindowWidth
        {
            get
            {
                return (double)base.GetValue(WindowWidthProperty);
            }
            set
            {
                if (value < 0)
                    return;

                base.SetValue(WindowWidthProperty, value);
            }
        }

        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position",
            typeof(Point), typeof(ImageVisual), new FrameworkPropertyMetadata(new Point(0, 0), new PropertyChangedCallback(OnPositionValueChanged)));

        public Point Position
        {
            get
            {
                return (Point)base.GetValue(PositionProperty);
            }
            set
            {
                base.SetValue(PositionProperty, value);
            }
        } 

        public static readonly DependencyProperty IsLockedProperty = DependencyProperty.Register("IsLocked",
            typeof(bool), typeof(ImageVisual), new FrameworkPropertyMetadata(false, null));

        public bool IsLocked
        {
            get
            {
                return (bool)base.GetValue(IsLockedProperty);
            }
            set
            {
                base.SetValue(IsLockedProperty, value);
            }
        }

        public static readonly DependencyProperty WindowNameProperty = DependencyProperty.Register("WindowName",
            typeof(string), typeof(ImageVisual), new FrameworkPropertyMetadata( Properties.Resources.Enter_Name, null));

        public string WindowName
        {
            get
            {
                return (string)base.GetValue(WindowNameProperty);
            }
            set
            {
                base.SetValue(WindowNameProperty, value);
            }
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source",
            typeof(ImageSource), typeof(ImageVisual), new FrameworkPropertyMetadata(null, null));

        public ImageSource Source
        {
            get
            {
                return (ImageSource)base.GetValue(SourceProperty);
            }
            set
            {
                base.SetValue(SourceProperty, value);
            }
        } 


        #endregion

        #region Constructors

        public ImageVisual(ImageModel model)
        {
            Model = model;
            Source = Model.getImage().Source;
            WindowWidth = Model.GetWidth();
            WindowHeight = Model.GetHeight();
            Position = Model.GetPosition();
            Model.UpdateableView = this;
            InitializeComponent();
        } 
        #endregion

        #region Event Handler

        private static void OnPositionValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        virtual protected void CloseClick(object sender, RoutedEventArgs e) 
        {
            Model.WorkspaceModel.ModifyModel(new DeleteImageModelOperation(Model));
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
            var delta = new Vector(e.HorizontalChange, e.VerticalChange);
            if (PositionDeltaChanged != null)
                PositionDeltaChanged.Invoke(this, new PositionDeltaChangedArgs() { PosDelta = delta, Model = this.Model });
        }
        #endregion

        public void update()
        {
            throw new NotImplementedException();
        }

        public ObjectSize ObjectSize
        {
            get { throw new NotImplementedException(); }
        }

        public Point[] RoutingPoints
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler<PositionDeltaChangedArgs> PositionDeltaChanged;


        public Point GetRoutingPoint(int routPoint)
        {
            throw new NotImplementedException();
        }
    }
}
