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
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WorkspaceManager.Model;
using WorkspaceManagerModel.Model.Interfaces;
using WorkspaceManagerModel.Model.Tools;

namespace WorkspaceManager.View.Visuals
{
    /// <summary>
    /// Interaction logic for ConnectorView.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("WorkspaceManager.Properties.Resources")]
    public partial class ConnectorVisual : UserControl, IUpdateableView, INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<IsDraggedEventArgs> Dragged;
        public event EventHandler Update;
        #endregion

        #region Properties
        public string ConnectorName => Model != null ? Model.GetName() : Properties.Resources.Error;

        public string TypeName => Model.ConnectorType != null ? Model.ConnectorType.Name : Properties.Resources.Class_Not_Found;

        public string Data
        {
            get
            {
                if (Model == null || Model.LastData == null)
                {
                    return Properties.Resources.No_data;
                }

                return ViewHelper.GetDataPresentationString(Model.LastData);
            }
        }
        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position",
            typeof(Point?), typeof(ConnectorVisual), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPositionChanged)));

        public Point? Position
        {
            get => (Point?)base.GetValue(PositionProperty);
            set => base.SetValue(PositionProperty, value);
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description",
            typeof(string), typeof(ConnectorVisual), new FrameworkPropertyMetadata(string.Empty));

        public string Description
        {
            get => (string)base.GetValue(DescriptionProperty);
            set => base.SetValue(DescriptionProperty, value);
        }

        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption",
            typeof(string), typeof(ConnectorVisual), new FrameworkPropertyMetadata(string.Empty));

        public string Caption
        {
            get => (string)base.GetValue(CaptionProperty);
            set => base.SetValue(CaptionProperty, value);
        }

        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model",
            typeof(ConnectorModel), typeof(ConnectorVisual), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnMyValueChanged)));

        public ConnectorModel Model
        {
            get => (ConnectorModel)base.GetValue(ModelProperty);
            set
            {
                base.SetValue(ModelProperty, value);
                Model.UpdateableView = this;
            }
        }

        public static readonly DependencyProperty IsMandatoryProperty = DependencyProperty.Register("IsMandatory",
            typeof(bool), typeof(ConnectorVisual), new FrameworkPropertyMetadata(false));

        public bool IsMandatory
        {
            get => (bool)base.GetValue(IsMandatoryProperty);
            set => base.SetValue(IsMandatoryProperty, value);
        }

        public static readonly DependencyProperty IsDraggedProperty = DependencyProperty.Register("IsDragged",
            typeof(bool), typeof(ConnectorVisual), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsDraggedChanged)));

        public bool IsDragged
        {
            get => (bool)base.GetValue(IsDraggedProperty);
            set => base.SetValue(IsDraggedProperty, value);
        }

        public static readonly DependencyProperty IsOutgoingProperty = DependencyProperty.Register("IsOutgoing",
            typeof(bool), typeof(ConnectorVisual), new FrameworkPropertyMetadata(false));

        public bool IsOutgoing
        {
            get => (bool)base.GetValue(IsOutgoingProperty);
            set => base.SetValue(IsOutgoingProperty, value);
        }

        public static readonly DependencyProperty RotationAngleProperty = DependencyProperty.Register("RotationAngle",
            typeof(double), typeof(ConnectorVisual), new FrameworkPropertyMetadata(double.Epsilon));

        public double RotationAngle
        {
            get => (double)base.GetValue(RotationAngleProperty);
            set => base.SetValue(RotationAngleProperty, value);
        }

        public static readonly DependencyProperty FunctionColorProperty = DependencyProperty.Register("FunctionColor",
            typeof(SolidColorBrush), typeof(ConnectorVisual), new FrameworkPropertyMetadata(Brushes.Black));

        public SolidColorBrush FunctionColor
        {
            get => (SolidColorBrush)base.GetValue(FunctionColorProperty);
            set => base.SetValue(FunctionColorProperty, value);
        }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation",
            typeof(ConnectorOrientation), typeof(ConnectorVisual), new FrameworkPropertyMetadata(ConnectorOrientation.Unset, new PropertyChangedCallback(OnOrientationChanged)));

        public ConnectorOrientation Orientation
        {
            get => (ConnectorOrientation)base.GetValue(OrientationProperty);
            set
            {
                base.SetValue(OrientationProperty, value);
                Model.Orientation = value;
            }
        }

        public static readonly DependencyProperty WindowParentProperty = DependencyProperty.Register("WindowParent",
            typeof(ComponentVisual), typeof(ConnectorVisual), new FrameworkPropertyMetadata(null));

        public ComponentVisual WindowParent
        {
            get => (ComponentVisual)base.GetValue(WindowParentProperty);
            set => base.SetValue(WindowParentProperty, value);
        }

        public static readonly DependencyProperty MarkedProperty = DependencyProperty.Register("Marked",
            typeof(bool), typeof(ConnectorVisual), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnMarkedValueChanged)));

        public bool Marked
        {
            get => (bool)base.GetValue(MarkedProperty);
            set => base.SetValue(MarkedProperty, value);
        }

        public static readonly DependencyProperty CVLevelProperty = DependencyProperty.Register("CVLevel",
            typeof(ConversionLevelInformation), typeof(ConnectorVisual), new FrameworkPropertyMetadata(null));

        public ConversionLevelInformation CVLevel
        {
            get => (ConversionLevelInformation)base.GetValue(CVLevelProperty);
            set => base.SetValue(CVLevelProperty, value);
        }

        public static readonly DependencyProperty IsLinkingProperty = DependencyProperty.Register("IsLinking",
            typeof(bool), typeof(ConnectorVisual), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsLinkingValueChanged)));

        public bool IsLinking
        {
            get => (bool)base.GetValue(IsLinkingProperty);
            set => base.SetValue(IsLinkingProperty, value);
        }
        #endregion

        public ConnectorVisual(ConnectorModel model, ComponentVisual component)
        {
            // TODO: Complete member initialization
            Model = model;
            WindowParent = component;
            InitializeComponent();
            //Loaded += delegate(object sender, RoutedEventArgs args) { RaiseUpdate(); };
        }

        public void RaiseUpdate()
        {
            if (Update != null)
            {
                Update.Invoke(this, null);
            }
        }

        private static void OnMarkedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ConnectorVisual bin = (ConnectorVisual)d;
            ConnectorVisual selected = bin.WindowParent.EditorVisual.SelectedConnector;

            //if (selected == null)
            //    return;

            //if (selected.Equals(bin))
            //    bin.CVLevel = null;
            //else
            //    bin.CVLevel = Util.ConversionCheck(bin.Model, bin.WindowParent.EditorVisual.SelectedConnector.Model);
        }

        private static void OnIsLinkingValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ConnectorVisual bin = (ConnectorVisual)d;

            if (bin.IsLinking == false)
            {
                bin.Marked = false;
                return;
            }

            if (bin.WindowParent == null || bin.WindowParent.EditorVisual == null || bin.WindowParent.EditorVisual.SelectedConnector == null)
            {
                bin.Marked = false;
                return;
            }

            ConnectorVisual selected = bin.WindowParent.EditorVisual.SelectedConnector;
            ConnectorModel input, output;
            input = selected.Model.Outgoing == true ? bin.Model : selected.Model;
            output = selected.Model.Outgoing == false ? bin.Model : selected.Model;

            if (bin == selected)
            {
                bin.CVLevel = new ConversionLevelInformation() { Level = ConversionLevel.NA };
            }
            else
            {
                ConversionLevel lvl = WorkspaceModel.compatibleConnectors(output, input);
                bin.CVLevel = new ConversionLevelInformation() { Level = lvl };
            }

            if (bin.CVLevel.Level != ConversionLevel.Red && bin.CVLevel.Level != ConversionLevel.NA)
            {
                bin.Marked = true;
            }
        }

        private static void OnMyValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ConnectorVisual bin = (ConnectorVisual)d;
            bin.FunctionColor = new SolidColorBrush(ColorHelper.GetLineColor(bin.Model.ConnectorType));
            bin.IsMandatory = bin.Model.IsMandatory;
            bin.IsOutgoing = bin.Model.Outgoing;
            bin.Description = bin.Model.ToolTip;
            bin.Caption = bin.Model.Caption;
            //bin.Model.Orientation = bin.Orientation;
        }

        private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ConnectorVisual bin = (ConnectorVisual)d;
            //bin.Model.Orientation = bin.Orientation;
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ConnectorVisual bin = (ConnectorVisual)d;
            ConnectorOrientation oldO = (ConnectorOrientation)e.OldValue;
            ConnectorOrientation newO = (ConnectorOrientation)e.NewValue;

            //if (oldO != ConnectorOrientation.Unset)
            //    bin.RaiseUpdate();
            //bin.Model.Orientation = bin.Orientation;
        }

        private static void OnIsDraggedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ConnectorVisual bin = (ConnectorVisual)d;
            if (bin.Dragged != null)
            {
                bin.Dragged.Invoke(bin, new IsDraggedEventArgs() { IsDragged = bin.IsDragged });
            }

            bin.RaiseUpdate();
        }

        public Point GetAbsolutePosition()
        {
            Panel ic = VisualParent as Panel;
            if (ic == null || Position == null)
            {
                return new Point(0, 0);
            }

            Point point = ic.TranslatePoint(new Point(0, 0), WindowParent);
            Point relativePoint = (Point)Position;
            return new Point(WindowParent.Position.X + point.X + relativePoint.X, WindowParent.Position.Y + point.Y + relativePoint.Y);
        }

        #region protected
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        private static void OnSelectedConnectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ConnectorVisual bin = (ConnectorVisual)d;
        }

        public bool CanConnect => throw new NotImplementedException();

        public void update()
        {

        }

        public void updateStatus()
        {

        }

        private void MouseEnterHandler(object sender, MouseEventArgs e)
        {
            if (!(sender is FrameworkElement))
            {
                return;
            }

            OnPropertyChanged("Data");
            ToolTip.IsOpen = true;
        }

        private void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            if (!(sender is FrameworkElement))
            {
                return;
            }

            ToolTip.IsOpen = false;
        }
    }

    public class ConversionLevelInformation
    {
        public ConversionLevel Level { get; set; }
        public Type SourceType { get; set; }
        public Type TargetType { get; set; }

        public string SourceTypeString
        {
            get
            {
                if (SourceType == null)
                {
                    return string.Empty;
                }

                return SourceType.Name;
            }
        }

        public string TargetTypeName
        {
            get
            {
                if (TargetType == null)
                {
                    return string.Empty;
                }

                return TargetType.Name;
            }
        }
    }

    public class IsDraggedEventArgs : EventArgs
    {
        public bool IsDragged { get; set; }
    }

    public class BinConnectorVisualBindingConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            ConnectorVisual connector = (ConnectorVisual)parameter;
            Point p = connector.GetAbsolutePosition();
            switch (connector.Orientation)
            {
                case ConnectorOrientation.West:
                    return new Point(p.X, p.Y + connector.ActualHeight / 2);
                case ConnectorOrientation.East:
                    return new Point(p.X + connector.ActualWidth, p.Y + connector.ActualHeight / 2);
                case ConnectorOrientation.North:
                    return new Point(p.X + connector.ActualWidth / 2, p.Y);
                case ConnectorOrientation.South:
                    return new Point(p.X + connector.ActualWidth / 2, p.Y + connector.ActualHeight);
            }

            return new Point(0, 0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
