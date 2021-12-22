using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Editor;
using WorkspaceManager.Model;
using WorkspaceManager.View.Base;
using WorkspaceManager.View.Visuals;
using WorkspaceManagerModel.Model.Operations;

namespace WorkspaceManager.View.VisualComponents
{
    /// <summary>
    /// Interaction logic for UsageStatisticPopup.xaml
    /// </summary>
    public partial class UsageStatisticPopup : UserControl
    {
        private ControlAdorner adorner;
        private AdornerLayer currentAdornerlayer;
        private Window window;
        public static readonly DependencyProperty SelectedConnectorProperty = DependencyProperty.Register("SelectedConnector",
            typeof(ConnectorVisual),
            typeof(UsageStatisticPopup),
            new FrameworkPropertyMetadata(null));

        public ConnectorVisual SelectedConnector
        {
            get => (ConnectorVisual)GetValue(SelectedConnectorProperty);
            set => SetValue(SelectedConnectorProperty, value);
        }

        public bool IsOpen = false;

        public static readonly DependencyProperty SuggestionsProperty = DependencyProperty.Register("Suggestions",
            typeof(ObservableCollection<SuggestionContainer>),
            typeof(UsageStatisticPopup),
            new FrameworkPropertyMetadata(null, null));

        private readonly EditorVisual _editor;
        private Point position;

        public ObservableCollection<SuggestionContainer> Suggestions
        {
            get => (ObservableCollection<SuggestionContainer>)GetValue(SuggestionsProperty);
            set => SetValue(SuggestionsProperty, value);
        }

        public UsageStatisticPopup(EditorVisual editor)
        {
            InitializeComponent();
            _editor = editor;
            _editor.SelectedConnectorChanged += new EventHandler(_editorSelectedConnectorChanged);
            Suggestions = new ObservableCollection<SuggestionContainer>();
            PreviewMouseLeftButtonDown += new MouseButtonEventHandler(UsageStatisticPopup_MouseLeftButtonDown);
        }

        private void UsageStatisticPopup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (TopUsages.SelectedItem != null)
            {
                try
                {
                    SuggestionContainer x = (SuggestionContainer)TopUsages.SelectedItem;
                    PluginModel pluginModel = (PluginModel)_editor.Model.ModifyModel(new NewPluginModelOperation(position, 0, 0, x.ComponentType));
                    _editor.AddComponentVisual(pluginModel);
                    ConnectorVisual connector = null;

                    foreach (ConnectorModel con in pluginModel.GetInputConnectors())
                    {
                        if (con.GetName() == x.ConnectorName)
                        {
                            connector = (ConnectorVisual)con.UpdateableView;
                        }
                    }
                    foreach (ConnectorModel con in pluginModel.GetOutputConnectors())
                    {
                        if (con.GetName() == x.ConnectorName)
                        {
                            connector = (ConnectorVisual)con.UpdateableView;
                        }
                    }

                    if (connector == null)
                    {
                        return;
                    }

                    ConnectorVisual input = SelectedConnector.Model.Outgoing == true ? connector : SelectedConnector;
                    ConnectorVisual output = SelectedConnector.Model.Outgoing == false ? connector : SelectedConnector;
                    ConnectionModel connectionModel = (ConnectionModel)_editor.Model.ModifyModel(new NewConnectionModelOperation(
                        output.Model,
                        input.Model,
                        output.Model.ConnectorType));
                    _editor.AddConnectionVisual(output, input, connectionModel);

                    position.X += 50;
                    position.Y += 50;
                }
                catch (Exception)
                {
                    return;
                }
                Close();
            }

        }


        public void Open()
        {
            try
            {
                if (!CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_ShowComponentConnectionProposition)
                {
                    return;
                }

                if (_editor.SelectedConnector != null && currentAdornerlayer == null)
                {
                    window = Window.GetWindow(_editor);
                    currentAdornerlayer = AdornerLayer.GetAdornerLayer((FrameworkElement)window.Content);
                    if (adorner != null)
                    {
                        adorner.RemoveRef();
                    }

                    adorner = new ControlAdorner((FrameworkElement)window.Content, this);
                    window.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(window_MouseLeftButtonUp);
                    position = Util.MouseUtilities.CorrectGetPosition(_editor.panel);

                    Point p = Mouse.GetPosition(window);
                    currentAdornerlayer.Add(adorner);
                    RenderTransform = new TranslateTransform(p.X - 620, p.Y - 350);
                    IsOpen = true;
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                UsageStatisticPopup result = Util.TryFindParent<UsageStatisticPopup>(e.OriginalSource as UIElement);
                if (TopUsages.SelectedItem != null || result == null)
                {
                    Close();
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void _editorSelectedConnectorChanged(object sender, EventArgs e)
        {
            try
            {
                if (!CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_ShowComponentConnectionProposition)
                {
                    return;
                }

                window = Window.GetWindow(_editor);
                if (window == null)
                {
                    return;
                }

                if (_editor.SelectedConnector == null)
                {
                    return;
                }

                SelectedConnector = _editor.SelectedConnector;

                System.Collections.Generic.IEnumerable<ComponentConnectionStatistics.ComponentConnector> list = ComponentConnectionStatistics.GetMostlyUsedComponentsFromConnector(SelectedConnector.Model.PluginModel.PluginType, SelectedConnector.Model.GetName());

                Suggestions = list != null
                    ? new ObservableCollection<SuggestionContainer>(list.Select(c => new SuggestionContainer(c.ConnectorName, c.Component)))
                    : new ObservableCollection<SuggestionContainer>();
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void Close()
        {

            if (currentAdornerlayer == null)
            {
                return;
            }

            try
            {
                currentAdornerlayer.Remove(adorner);
                window.PreviewMouseLeftButtonDown -= new MouseButtonEventHandler(window_MouseLeftButtonUp);
                currentAdornerlayer = null;
                IsOpen = false;
            }
            catch (Exception)
            {
                //do nothing
            }
        }
    }

    public class SuggestionContainer
    {
        public SuggestionContainer(string connectorName, Type componentType)
        {
            ConnectorName = connectorName;
            ComponentType = componentType;
            ComponentCaption = componentType.Name;
            ConnectorCaption = connectorName;

            if (string.IsNullOrEmpty(connectorName) || componentType == null)
            {
                return;
            }

            try
            {
                PluginInfoAttribute pluginfo = PluginExtension.GetPluginInfoAttribute(componentType);
                pluginfo.PluginType = componentType;
                ComponentCaption = pluginfo.Caption;
                System.Reflection.PropertyInfo propinfo = componentType.GetProperty(connectorName);
                if (propinfo != null)
                {
                    PropertyInfoAttribute[] attributes = ((PropertyInfoAttribute[])propinfo.GetCustomAttributes(typeof(PropertyInfoAttribute), false));
                    if (attributes == null || attributes.Length == 0)
                    {
                        return;
                    }
                    attributes[0].PluginType = componentType;
                    ConnectorCaption = attributes[0].Caption;
                    IsInput = attributes[0].Direction == Direction.InputData;
                    //Icon = componentType.CreateComponentInstance().GetImage(0);
                    Icon = componentType.GetImage(0);
                }
            }
            catch (Exception)
            {
                //do nothing 
            }
        }

        public SuggestionContainer()
        {
        }

        public Image Icon { get; private set; }
        public bool IsInput { get; private set; }

        // language dependent names as used in connector statistics popup
        public string ComponentCaption { get; set; }
        public string ConnectorCaption { get; set; }

        // language independent names as used in component statistics file ccs.xml
        public Type ComponentType { get; set; }
        public string ConnectorName { get; set; }

        public string Test { get; set; }
    }

    internal class ControlAdorner : Adorner
    {
        private readonly UserControl _child;
        private readonly UIElement _adornedElement;

        public ControlAdorner(UIElement adornedElement, UserControl ctrl)
            : base(adornedElement)
        {
            _child = ctrl;
            _adornedElement = adornedElement;
            AddLogicalChild(_child);
            AddVisualChild(_child);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            // ... add custom rendering code here ...
        }

        protected override int VisualChildrenCount => 1;

        protected override Size MeasureOverride(Size constraint)
        {
            _child.Measure(constraint);
            if (double.IsInfinity(constraint.Height))
            {
                constraint.Height = _adornedElement.DesiredSize.Height;
            }
            return constraint;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _child.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return _child;
        }

        internal void RemoveRef()
        {
            RemoveLogicalChild(_child);
            RemoveVisualChild(_child);
        }
    }

    public sealed class CricularLineProgress : Shape
    {
        public static readonly DependencyProperty StartPointProperty = DependencyProperty.Register("StartPoint",
   typeof(Point), typeof(CricularLineProgress), new FrameworkPropertyMetadata(new Point(0, 0),
       FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public Point StartPoint
        {
            get => (Point)GetValue(StartPointProperty);
            set => SetValue(StartPointProperty, value);
        }

        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register("Progress",
typeof(double), typeof(CricularLineProgress), new FrameworkPropertyMetadata((double)0, null));

        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register("Radius",
typeof(double), typeof(CricularLineProgress), new FrameworkPropertyMetadata((double)40, OnRadiusChanged));

        public double Radius
        {
            get => (double)GetValue(RadiusProperty);
            set => SetValue(RadiusProperty, value);
        }

        private static void OnRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CricularLineProgress c = (CricularLineProgress)d;
            c.InvalidateVisual();
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                StreamGeometry geometry = new StreamGeometry
                {
                    FillRule = FillRule.EvenOdd
                };

                using (StreamGeometryContext context = geometry.Open())
                {
                    internalGeometryDraw(context);
                }

                geometry.Freeze();
                return geometry;
            }
        }

        public CricularLineProgress()
        {
            Stroke = Brushes.RoyalBlue;
            Opacity = 0.7;
            StrokeThickness = 3;
        }

        private static readonly double kappa = 4 * ((Math.Sqrt(2) - 1) / 3);

        private void ellipse(double xm, double ym, double r, StreamGeometryContext context)
        {
            double ctrl_ell_x1 = Radius * kappa;
            double ctrl_ell_y1 = Radius;

            double ctrl_ell_x2 = Radius;
            double ctrl_ell_y2 = Radius * kappa;

            double ell_x = Radius * Math.Sin(2 * Math.PI * 0.25);
            double ell_y = Radius * Math.Cos(2 * Math.PI * 0.25);

            Point ctrlPoint1 = new Point(xm + ctrl_ell_x1, ym - ctrl_ell_y1);
            Point ctrlPoint2 = new Point(xm + ctrl_ell_x2, ym - ctrl_ell_y2);
            Point point = new Point(xm + ell_x, ym - ell_y);

            context.BezierTo(ctrlPoint1, ctrlPoint2, point, true, true);
            //--------------------------------------------------------------------------

            ctrl_ell_x2 = Radius * kappa;
            ctrl_ell_y2 = -Radius;

            ctrl_ell_x1 = Radius;
            ctrl_ell_y1 = -Radius * kappa;

            ell_x = Radius * Math.Sin(2 * Math.PI * 0.50);
            ell_y = Radius * Math.Cos(2 * Math.PI * 0.50);

            ctrlPoint1 = new Point(xm + ctrl_ell_x1, ym - ctrl_ell_y1);
            ctrlPoint2 = new Point(xm + ctrl_ell_x2, ym - ctrl_ell_y2);
            point = new Point(xm + ell_x, ym - ell_y);

            context.BezierTo(ctrlPoint1, ctrlPoint2, point, true, true);
            //--------------------------------------------------------------------------
            ctrl_ell_x1 = -Radius * kappa;
            ctrl_ell_y1 = -Radius;

            ctrl_ell_x2 = -Radius;
            ctrl_ell_y2 = -Radius * kappa;

            ell_x = Radius * Math.Sin(2 * Math.PI * 0.75);
            ell_y = Radius * Math.Cos(2 * Math.PI * 0.75);

            ctrlPoint1 = new Point(xm + ctrl_ell_x1, ym - ctrl_ell_y1);
            ctrlPoint2 = new Point(xm + ctrl_ell_x2, ym - ctrl_ell_y2);
            point = new Point(xm + ell_x, ym - ell_y);

            context.BezierTo(ctrlPoint1, ctrlPoint2, point, true, true);
            //--------------------------------------------------------------------------
            ctrl_ell_x2 = -Radius * kappa;
            ctrl_ell_y2 = Radius;

            ctrl_ell_x1 = -Radius;
            ctrl_ell_y1 = Radius * kappa;

            ell_x = Radius * Math.Sin(2 * Math.PI * 1);
            ell_y = Radius * Math.Cos(2 * Math.PI * 1);

            ctrlPoint1 = new Point(xm + ctrl_ell_x1, ym - ctrl_ell_y1);
            ctrlPoint2 = new Point(xm + ctrl_ell_x2, ym - ctrl_ell_y2);
            point = new Point(xm + ell_x, ym - ell_y);

            context.BezierTo(ctrlPoint1, ctrlPoint2, point, true, true);
            //--------------------------------------------------------------------------
        }

        private void internalGeometryDraw(StreamGeometryContext context)
        {
            Point realSP = new Point(StartPoint.X, StartPoint.Y - Radius);
            context.BeginFigure(realSP, true, false);
            ellipse(StartPoint.X, StartPoint.Y, Radius, context);
        }
    }

    public class CricularLineProgressAdorner : Adorner
    {
        private readonly UIElement _adornedElement;
        private readonly CricularLineProgress _clp;
        private readonly Window _win;

        public CricularLineProgressAdorner(UIElement adornedElement, CricularLineProgress clp)
            : base(adornedElement)
        {
            _adornedElement = adornedElement;
            _clp = clp;
            _clp.IsHitTestVisible = false;
            AddLogicalChild(clp);
            AddVisualChild(clp);
            _win = Window.GetWindow(adornedElement);
            _win.PreviewMouseMove += new MouseEventHandler(_win_PreviewMouseMove);
            Point p = Mouse.GetPosition(_win);
            _clp.RenderTransform = new TranslateTransform(p.X, p.Y);

            DoubleAnimation anim = new DoubleAnimation(50, 0, new Duration(TimeSpan.FromSeconds(CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_BlingDelay)));
            DoubleAnimation anim2 = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_BlingDelay)));
            Storyboard sb = new Storyboard();
            Storyboard.SetTarget(anim, _clp);
            Storyboard.SetTarget(anim2, _clp);
            Storyboard.SetTargetProperty(anim, new PropertyPath(CricularLineProgress.RadiusProperty));
            Storyboard.SetTargetProperty(anim2, new PropertyPath(CricularLineProgress.OpacityProperty));
            sb.Children.Add(anim);
            sb.Children.Add(anim2);
            sb.Begin();
        }

        private void _win_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            TranslateTransform trans = (TranslateTransform)_clp.RenderTransform;
            trans.X = e.GetPosition(_win).X;
            trans.Y = e.GetPosition(_win).Y;
        }

        protected override int VisualChildrenCount => 1;

        protected override Size MeasureOverride(Size constraint)
        {
            _clp.Measure(constraint);
            if (double.IsInfinity(constraint.Height))
            {
                constraint.Height = _adornedElement.DesiredSize.Height;
            }
            return constraint;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _clp.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return _clp;
        }
    }
}
