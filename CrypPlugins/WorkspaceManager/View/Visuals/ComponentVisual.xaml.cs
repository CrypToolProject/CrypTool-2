using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;
using WorkspaceManager.View.Base;
using WorkspaceManager.Model;
using WorkspaceManager.View.Base.Interfaces;
using WorkspaceManagerModel.Model.Operations;
using System.ComponentModel;
using WorkspaceManager.View.VisualComponents;
using System.Collections;
using WorkspaceManagerModel.Model.Interfaces;
using System.Windows.Controls.Primitives;
using CrypTool.PluginBase;
using System.Windows.Threading;
using System.Threading;

namespace WorkspaceManager.View.Visuals
{
    /// <summary>
    /// Interaction logic for BinFunctionVisual.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("WorkspaceManager.Properties.Resources")]
    public partial class ComponentVisual : UserControl, IRouting, IZOrdering, INotifyPropertyChanged, IUpdateableView
    {

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<VisualStateChangedArgs> StateChanged;
        public event EventHandler<PositionDeltaChangedArgs> PositionDeltaChanged;
        public event EventHandler<ZIndexChangedArgs> ZIndexChanged;
        public event EventHandler<IsDraggingChangedArgs> IsDraggingChanged;
        public event EventHandler Redraw;
        #endregion

        #region IZOrdering
        public int ZIndex
        {
            get
            {
                return Model.ZIndex;
            }
            set
            {
                Model.ZIndex = value;
            }
        }
        #endregion

        #region IRouting
        public ObjectSize ObjectSize
        {
            get
            {
                if(this.ActualWidth == 0 || this.ActualHeight == 0)
                    return new ObjectSize(120, 140);
                else
                    return new ObjectSize(this.ActualWidth, this.ActualHeight - NameTextBox.ActualHeight);
            }
        }

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

        #endregion

        #region Model

        private PluginSettingsContainer modelSettingsContainer;

        private PluginModel model;
        public PluginModel Model
        {
            get { return model; }
            private set 
            { 
                model = value;
                modelSettingsContainer = new PluginSettingsContainer(value.Plugin);
                AddPresentationElement(BinComponentState.Presentation, model.PluginPresentation);
                var image = Model.getImage();
                AddPresentationElement(BinComponentState.Min, image);
                AddPresentationElement(BinComponentState.Default, image);
                AddPresentationElement(BinComponentState.Data, () => new DataVisual(ConnectorCollection));
                AddPresentationElement(BinComponentState.Log, () => new LogVisual(this));
                AddPresentationElement(BinComponentState.Setting, () => new SettingsVisual(modelSettingsContainer, this, true, false));

                FullScreenState = HasComponentPresentation ? BinComponentState.Presentation : BinComponentState.Log;

                var s = State = (BinComponentState)Enum.Parse(typeof(BinComponentState), Model.ViewState.ToString());
                if (s == BinComponentState.Default)
                {
                    if (CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_ComponentAppearance == 1)
                    {
                        State = HasComponentPresentation ? BinComponentState.Presentation : BinComponentState.Min;
                        return;
                    }
                    if (CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_ComponentAppearance == 2)
                    {
                        State = BinComponentState.Min;
                        return;
                    }
                    if (CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_ComponentAppearance == 0)
                    {
                        var x = Model.PluginType.GetCustomAttributes(typeof(CrypTool.PluginBase.Attributes.ComponentVisualAppearance), false);

                        if (x != null)
                        {
                            if (x.Count() != 0)
                            {
                                var y = (CrypTool.PluginBase.Attributes.ComponentVisualAppearance)x[0];
                                if (y.DefaultVisualAppearance == CrypTool.PluginBase.Attributes.ComponentVisualAppearance.VisualAppearanceEnum.Opened)
                                {
                                    State = HasComponentPresentation ? BinComponentState.Presentation : BinComponentState.Min;
                                    return;
                                }
                                if (y.DefaultVisualAppearance == CrypTool.PluginBase.Attributes.ComponentVisualAppearance.VisualAppearanceEnum.Closed)
                                {
                                    State = BinComponentState.Min;
                                    return;
                                }

                            }
                        }

                    }
                    State = BinComponentState.Min;
                    return;
                }
                State = s;
            }
        }
        #endregion

        #region Fields

        #endregion

        #region Presentation Elements

        /// <summary>
        /// Represents a presentation element on the plugin component.
        /// </summary>
        private class PresentationElement
        {
            private UIElement element;
            private readonly Func<UIElement> createElementAction;

            public UIElement Element => element ?? (element = createElementAction?.Invoke());

            /// <summary>
            /// Lazy loading constructor. Presentation element will only be created as soon as it is accessed.
            /// </summary>
            /// <param name="createElementAction">Creation function which returns the instance of the UI element.</param>
            public PresentationElement(Func<UIElement> createElementAction)
            {
                this.createElementAction = createElementAction;
            }

            /// <summary>
            /// Immediate constructor for an already instatiated presentation element.
            /// </summary>
            /// <param name="element">The instance of the UI element.</param>
            public PresentationElement(UIElement element)
            {
                this.element = element;
            }
        }

        private readonly Dictionary<BinComponentState, PresentationElement> presentations = new Dictionary<BinComponentState, PresentationElement>();

        /// <summary>
        /// Adds an already instantiated presentation element to the plugin component.
        /// </summary>
        internal void AddPresentationElement(BinComponentState state, UIElement presentation)
        {
            if (presentation != null)   //do not add empty elements in the first place, in order to make availability check easier.
            {
                presentations[state] = new PresentationElement(presentation);
            }
        }

        /// <summary>
        /// Adds a presentation element to the plugin component, which will be instantiated as soon as it is needed.
        /// </summary>
        internal void AddPresentationElement(BinComponentState state, Func<UIElement> createElementAction)
        {
            presentations[state] = new PresentationElement(createElementAction);
        }

        internal UIElement GetPresentationElement(BinComponentState state)
        {
            if (presentations.TryGetValue(state, out var element))
            {
                return element.Element;
            }
            return null;
        }

        internal bool IsPresentationElementAvailable(BinComponentState state) => presentations.ContainsKey(state);

        #endregion

        #region Properties
        public Queue<Log> ErrorsTillReset { private set; get; }
        public ThumHack HackThumb = new ThumHack();
        public EditorVisual EditorVisual { private set; get; }

        public Vector Delta { private set; get; }
        public EditorVisual Editor { private set; get; }

        public bool HasComponentPresentation => IsPresentationElementAvailable(BinComponentState.Presentation);

        public bool HasComponentSetting => IsPresentationElementAvailable(BinComponentState.Setting);

        public UIElement ActivePresentation => GetPresentationElement(State);

        private BinComponentState lastState;
        public BinComponentState LastState
        {
            set
            {
                lastState = value;
            }

            get
            {
                return lastState;
            }
        }

        private BinComponentState fullScreenState;
        public BinComponentState FullScreenState
        {
            set
            {
                fullScreenState = value;
            }

            get
            {
                return fullScreenState;
            }
        }

        public Image Icon => GetPresentationElement(BinComponentState.Min) as Image;

        private ObservableCollection<IControlMasterElement> iControlCollection = new ObservableCollection<IControlMasterElement>();
        public ObservableCollection<IControlMasterElement> IControlCollection { get { return iControlCollection; } }

        private ObservableCollection<ConnectorVisual> connectorCollection = new ObservableCollection<ConnectorVisual>();
        public ObservableCollection<ConnectorVisual> ConnectorCollection { get { return connectorCollection; } }

        private ObservableCollection<ConnectorVisual> southConnectorCollection = new ObservableCollection<ConnectorVisual>();
        public ObservableCollection<ConnectorVisual> SouthConnectorCollection { get { return southConnectorCollection; } }

        private ObservableCollection<ConnectorVisual> northConnectorCollection = new ObservableCollection<ConnectorVisual>();
        public ObservableCollection<ConnectorVisual> NorthConnectorCollection { get { return northConnectorCollection; } }

        private ObservableCollection<ConnectorVisual> eastConnectorCollection = new ObservableCollection<ConnectorVisual>();
        public ObservableCollection<ConnectorVisual> EastConnectorCollection { get { return eastConnectorCollection; } }

        private ObservableCollection<ConnectorVisual> westConnectorCollection = new ObservableCollection<ConnectorVisual>();
        public ObservableCollection<ConnectorVisual> WestConnectorCollection { get { return westConnectorCollection; } }

        private ObservableCollection<Log> logMessages = new ObservableCollection<Log>();
        public ObservableCollection<Log> LogMessages { get { return logMessages; } }

        #endregion

        #region DependencyProperties

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected",
            typeof(bool), typeof(ComponentVisual), new FrameworkPropertyMetadata(false, OnIsSelectedChanged));

        public bool IsSelected
        {
            get { return (bool)base.GetValue(IsSelectedProperty); }
            set
            {
                base.SetValue(IsSelectedProperty, value);
            }
        }

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive",
            typeof(bool), typeof(ComponentVisual), new FrameworkPropertyMetadata(false, OnIsActiveChanged));

        public bool IsActive
        {
            get { return (bool)base.GetValue(IsActiveProperty); }
            set
            {
                base.SetValue(IsActiveProperty, value);
            }
        }

        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ComponentVisual bin = (ComponentVisual)d;
        }

        public static readonly DependencyProperty LogNotifierProperty = DependencyProperty.Register("LogNotifier", typeof(LogNotifierVisual),
            typeof(ComponentVisual), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public LogNotifierVisual LogNotifier
        {
            get { return (LogNotifierVisual)base.GetValue(LogNotifierProperty); }
            set
            {
                base.SetValue(LogNotifierProperty, value);
            }
        }

        public static readonly DependencyProperty IsConnectorDragStartedProperty = DependencyProperty.Register("IsConnectorDragStarted", typeof(bool),
            typeof(ComponentVisual), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public bool IsConnectorDragStarted
        {
            get { return (bool)base.GetValue(IsConnectorDragStartedProperty); }
            set
            {
                base.SetValue(IsConnectorDragStartedProperty, value);
            }
        }

        public static readonly DependencyProperty IsDraggingProperty = DependencyProperty.Register("IsDragging", typeof(bool),
            typeof(ComponentVisual), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                new PropertyChangedCallback(OnIsDraggingChanged)));

        public bool IsDragging
        {
            get { return (bool)base.GetValue(IsDraggingProperty); }
            set
            {
                base.SetValue(IsDraggingProperty, value);
            }
        }

        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State",
            typeof(BinComponentState), typeof(ComponentVisual), new FrameworkPropertyMetadata(BinComponentState.Min, new PropertyChangedCallback(OnStateValueChanged)));

        public BinComponentState State
        {
            get
            {
                return (BinComponentState)base.GetValue(StateProperty);
            }
            set
            {
                base.SetValue(StateProperty, value);
            }
        }

        public static readonly DependencyProperty InternalStateProperty = DependencyProperty.Register("InternalState",
            typeof(PluginModelState), typeof(ComponentVisual), new FrameworkPropertyMetadata(PluginModelState.Normal));

        public PluginModelState InternalState
        {
            get
            {
                return (PluginModelState)base.GetValue(InternalStateProperty);
            }
            set
            {
                base.SetValue(InternalStateProperty, value);
            }
        }

        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position",
            typeof(Point), typeof(ComponentVisual), new FrameworkPropertyMetadata(new Point(0, 0)));

        public static readonly DependencyProperty IsFullscreenProperty = DependencyProperty.Register("IsFullscreen",
                typeof(bool), typeof(ComponentVisual), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsFullscreenChanged)));

        public bool IsFullscreen
        {
            get
            {
                return (bool)base.GetValue(IsFullscreenProperty);
            }
            set
            {
                base.SetValue(IsFullscreenProperty, value);
            }
        }

        public static readonly DependencyProperty IsICPopUpOpenProperty = DependencyProperty.Register("IsICPopUpOpen",
        typeof(bool), typeof(ComponentVisual), new FrameworkPropertyMetadata(false));

        public bool IsICPopUpOpen
        {
            get
            {
                return (bool)base.GetValue(IsICPopUpOpenProperty);
            }
            set
            {
                base.SetValue(IsICPopUpOpenProperty, value);
            }
        }

        public static readonly DependencyProperty IsErrorDisplayVisibleProperty = DependencyProperty.Register("IsErrorDisplayVisible",
            typeof(bool), typeof(ComponentVisual), new FrameworkPropertyMetadata(false));

        public bool IsErrorDisplayVisible
        {
            get
            {
                return (bool)base.GetValue(IsErrorDisplayVisibleProperty);
            }
            set
            {
                base.SetValue(IsErrorDisplayVisibleProperty, value);
            }
        }

        public static readonly DependencyProperty IsRepeatableProperty = DependencyProperty.Register("IsRepeatable",
            typeof(bool), typeof(ComponentVisual), new FrameworkPropertyMetadata(false));

        public bool IsRepeatable
        {
            get
            {
                return (bool)base.GetValue(IsRepeatableProperty);
            }
            private set
            {
                base.SetValue(IsRepeatableProperty, value);
            }
        }

        public static readonly DependencyProperty RepeatProperty = DependencyProperty.Register("Repeat",
            typeof(bool), typeof(ComponentVisual), new FrameworkPropertyMetadata(false));

        public bool Repeat
        {
            get
            {
                return (bool)base.GetValue(RepeatProperty);
            }
            private set
            {
                base.SetValue(RepeatProperty, value);
                Model.RepeatStart = value;
            }
        }

        public static readonly DependencyProperty CustomNameProperty = DependencyProperty.Register("CustomName",
            typeof(string), typeof(ComponentVisual), new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnCustomNameChanged)));

        public string CustomName
        {
            get
            {
                return (string)base.GetValue(CustomNameProperty);
            }
            set
            {
                base.SetValue(CustomNameProperty, value);
            }
        }

        public static readonly DependencyProperty IsICMasterProperty = DependencyProperty.Register("IsICMaster",
            typeof(bool), typeof(ComponentVisual), new FrameworkPropertyMetadata(false));

        public bool IsICMaster
        {
            get
            {
                return (bool)base.GetValue(IsICMasterProperty);
            }
            set
            {
                base.SetValue(IsICMasterProperty, value);
            }
        }

        public static readonly DependencyProperty WindowHeightProperty = DependencyProperty.Register("WindowHeight",
            typeof(double), typeof(ComponentVisual), new FrameworkPropertyMetadata(double.Epsilon));

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
            typeof(double), typeof(ComponentVisual), new FrameworkPropertyMetadata(double.Epsilon));

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

        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register("Progress",
            typeof(double), typeof(ComponentVisual), new FrameworkPropertyMetadata((double)0));

        public double Progress
        {
            get
            {
                return (double)base.GetValue(ProgressProperty);
            }
            set
            {
                base.SetValue(ProgressProperty, value);
            }
        }

        public static readonly DependencyProperty FunctionNameProperty = DependencyProperty.Register("FunctionName",
            typeof(string), typeof(ComponentVisual), new FrameworkPropertyMetadata(string.Empty));

        public string FunctionName
        {
            get
            {
                return (string)base.GetValue(FunctionNameProperty);
            }
            set
            {
                base.SetValue(FunctionNameProperty, value);
            }
        }

        public static readonly DependencyProperty BackgroundBrushColorProperty = DependencyProperty.Register("BackgroundBrushColor",
typeof(SolidColorBrush), typeof(ComponentVisual), new FrameworkPropertyMetadata(null));

        public SolidColorBrush BackgroundBrushColor
        {
            get
            {
                return (SolidColorBrush)base.GetValue(BackgroundBrushColorProperty);
            }
            set
            {
                base.SetValue(BackgroundBrushColorProperty, value);
            }
        }

        public static readonly DependencyProperty BorderBrushColorProperty = DependencyProperty.Register("BorderBrushColor",
    typeof(SolidColorBrush), typeof(ComponentVisual), new FrameworkPropertyMetadata(null));

        public SolidColorBrush BorderBrushColor
        {
            get
            {
                return (SolidColorBrush)base.GetValue(BorderBrushColorProperty);
            }
            set
            {
                base.SetValue(BorderBrushColorProperty, value);
            }
        }

        private SettingsVisual sideBarSetting;
        public SettingsVisual SideBarSetting => sideBarSetting ?? (sideBarSetting = new SettingsVisual(modelSettingsContainer, this, true, true));

        #endregion

        #region Constructors
        public ComponentVisual(PluginModel model)
        {
            Model = model;
            Model.UpdateableView = this;
            LastState = HasComponentPresentation ? BinComponentState.Presentation : BinComponentState.Setting;
            Editor = (EditorVisual)((WorkspaceManagerClass)Model.WorkspaceModel.MyEditor).Presentation;
            Editor.FullscreenVisual.Open += new EventHandler(FullscreenVisual_Close);
            ErrorsTillReset = new Queue<Log>();
            EditorVisual = (EditorVisual)((WorkspaceManagerClass)Model.WorkspaceModel.MyEditor).Presentation;

            InitializeComponent();
        }

        void FullscreenVisual_Close(object sender, EventArgs e)
        {

            OnPropertyChanged("ActivePresentation");
        }
        #endregion

        #region public

        /*
         * 1________3
         * |        |   
         * |        |
         * |________|
         * 2        4
         * 
         * */
        public Point GetRoutingPoint(int routPoint)
        {
            switch (routPoint)
            {
                case 0:
                    return new Point(Position.X - 1, Position.Y - 1);
                case 1:
                    return new Point(Position.X - 1, Position.Y + ObjectSize.Y + 1);
                case 2:
                    return new Point(Position.X + 1 + ObjectSize.X, Position.Y - 1);
                case 3:
                    return new Point(Position.X + ObjectSize.X + 1, Position.Y + ObjectSize.Y + 1);
            }
            return default(Point);
        }

        public void update()
        {
            Progress = Model.PercentageFinished;
            AddPresentationElement(BinComponentState.Min, Model.getImage());
            OnPropertyChanged("ActivePresentation");
        }
        #endregion

        #region private

        private void initConnectorVisuals(PluginModel model)
        {
            IEnumerable<ConnectorModel> list = model.GetOutputConnectors().Concat<ConnectorModel>(model.GetInputConnectors());
            foreach (ConnectorModel m in list)
            {
                if (m.IControl && m.Outgoing)
                {
                    PluginModel pm = null;
                    if (m.GetOutputConnections().Count > 0)
                    {
                        pm = m.GetOutputConnections()[0].To.PluginModel;
                    }

                    IControlCollection.Add(new IControlMasterElement(m, pm));
                    continue;
                }

                if (m.IControl)
                    continue;

                addConnectorView(m);
            }

            var SouthConnectorCollectionView = CollectionViewSource.GetDefaultView(SouthConnectorCollection) as ListCollectionView;
            SouthConnectorCollectionView.CustomSort = new ConnectorSorter(SouthConnectorCollection, SouthConnectorCollectionView, this);

            var EastConnectorCollectionView = CollectionViewSource.GetDefaultView(EastConnectorCollection) as ListCollectionView;
            EastConnectorCollectionView.CustomSort = new ConnectorSorter(EastConnectorCollection, EastConnectorCollectionView, this);

            var WestConnectorCollectionView = CollectionViewSource.GetDefaultView(WestConnectorCollection) as ListCollectionView;
            WestConnectorCollectionView.CustomSort = new ConnectorSorter(WestConnectorCollection, WestConnectorCollectionView, this);

            var NorthConnectorCollectionView = CollectionViewSource.GetDefaultView(NorthConnectorCollection) as ListCollectionView;
            NorthConnectorCollectionView.CustomSort = new ConnectorSorter(NorthConnectorCollection, NorthConnectorCollectionView, this);

            SouthConnectorCollection.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(SouthConnectorCollectionCollectionChanged);
        }

        internal class ConnectorSorter : IComparer
        {
            private ObservableCollection<ConnectorVisual> collection;
            private ListCollectionView view;

            public ConnectorSorter(ObservableCollection<ConnectorVisual> collection, ListCollectionView view, ComponentVisual Parent)
            {
                this.collection = collection;
                this.view = view;
                if (collection.Any(x => x.Model.Index == int.MinValue))
                {
                    foreach (var item in collection)
                        item.Model.Index = collection.IndexOf(item);
                }
                else
                    this.collection.OrderBy(x => x.Model.Index);

                this.collection.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ConnectorCollectionItemChanged);
            }

            public int Compare(object x, object y)
            {
                var connA = x as ConnectorVisual;
                var connB = y as ConnectorVisual;
                var val = connA.Model.Index.CompareTo(connB.Model.Index);
                return val;
            }

            private void assignIndex()
            {
                foreach (var connector in collection)
                {
                    int index = collection.IndexOf(connector);
                    connector.Model.Index = index;
                }
                view.Refresh();
            }

            void ConnectorCollectionItemChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                assignIndex();
            }
        }

        private void initializeVisual(PluginModel model)
        {
            initConnectorVisuals(model);
            LogNotifier = new LogNotifierVisual(LogMessages, this);
            LogNotifier.ErrorMessagesOccured += new EventHandler<ErrorMessagesOccuredArgs>(LogNotifierErrorMessagesOccuredHandler);
            //LogMessages.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(LogMessagesCollectionChanged);
            Model.Plugin.OnGuiLogNotificationOccured += new GuiLogNotificationEventHandler(OnGuiLogNotificationOccuredHandler);
            WindowWidth = Model.GetWidth();
            WindowHeight = Model.GetHeight();
            //IsRepeatable = Model.Startable;
            Repeat = Model.RepeatStart;
            Position = model.GetPosition();
            FunctionName = Model.Plugin.GetPluginInfoAttribute().Caption;
            CustomName = Model.GetName();
            IsICMaster = Model.HasIControlInputs();
            SetBinding(ComponentVisual.IsDraggingProperty,
                Util.CreateIsDraggingBinding(new Thumb[] { ContentThumb, TitleThumb, ScaleThumb, HackThumb, TopLeftDrag, TopRightDrag, BottomLeftDrag }));
            setWindowColors(ColorHelper.GetColor(Model.PluginType), ColorHelper.GetColorLight(Model.PluginType));
        }

        void SouthConnectorCollectionCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs("SouthConnectorCollection.Count"));
        }

        void LogNotifierErrorMessagesOccuredHandler(object sender, ErrorMessagesOccuredArgs e)
        {
            if (e.HasErrors)
                IsErrorDisplayVisible = true;
            else
                IsErrorDisplayVisible = false;
        }

        //void LogMessagesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        //    {
        //        Log log = (Log)e.NewItems[0];
        //        if (log.Level == NotificationLevel.Error)
        //        {
        //            IsErrorDisplayVisible = true;
        //        }
        //    }

        //    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
        //    {
        //        IsErrorDisplayVisible = false;
        //    }
        //}

        private void OnGuiLogNotificationOccuredHandler(IPlugin sender, GuiLogEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, (SendOrPostCallback)delegate
            {
                LogMessages.Add(new Log(args));
            }
            , null);
        }

        private void setWindowColors(Color Border, Color Background)
        {
            BorderBrushColor = new SolidColorBrush(Border);
            BackgroundBrushColor = new SolidColorBrush(Background);
        }

        private void addConnectorView(ConnectorModel model)
        {
            ConnectorVisual bin = new ConnectorVisual(model, this);

            Binding bind = new Binding();
            bind.Path = new PropertyPath(EditorVisual.IsLinkingProperty);
            bind.Source = EditorVisual;
            bin.SetBinding(ConnectorVisual.IsLinkingProperty, bind);

            switch (model.Orientation)
            {
                case ConnectorOrientation.Unset:
                    if (model.Outgoing)
                        EastConnectorCollection.Add(bin);
                    else
                        WestConnectorCollection.Add(bin);
                    break;
                case ConnectorOrientation.West:
                    WestConnectorCollection.Add(bin);
                    break;
                case ConnectorOrientation.East:
                    EastConnectorCollection.Add(bin);
                    break;
                case ConnectorOrientation.North:
                    NorthConnectorCollection.Add(bin);
                    break;
                case ConnectorOrientation.South:
                    SouthConnectorCollection.Add(bin);
                    break;
            }
            ConnectorCollection.Add(bin);
            bin.Dragged += new EventHandler<IsDraggedEventArgs>(ConnectorDragged);
        }
        #endregion

        #region protected
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            initializeVisual(Model);
        }
        #endregion

        #region Event Handler
        #region DragDropHandler

        private void PreviewDropHandler(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("BinConnector"))
            {
                try
                {
                    ItemsControl items = (ItemsControl)sender;
                    ConnectorVisual connector = (ConnectorVisual)e.Data.GetData("BinConnector");

                    if (connector.WindowParent != this)
                        return;

                    switch (connector.Orientation)
                    {
                        case ConnectorOrientation.North:
                            NorthConnectorCollection.Remove(connector);
                            break;
                        case ConnectorOrientation.South:
                            SouthConnectorCollection.Remove(connector);
                            break;
                        case ConnectorOrientation.East:
                            EastConnectorCollection.Remove(connector);
                            break;
                        case ConnectorOrientation.West:
                            WestConnectorCollection.Remove(connector);
                            break;
                    }

                    IList itemsSource = (IList)items.ItemsSource;
                    itemsSource.Add(connector);
                }
                catch (Exception ex)
                {
                    
                }
            }
        }
        #endregion

        private void ContextMenuClick(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            BinComponentState localState = BinComponentState.Log;
            switch ((string)item.Tag)
            {
                case "presentation":
                    localState = BinComponentState.Presentation;
                    break;

                case "data":
                    localState = BinComponentState.Data;
                    break;

                case "log":
                    localState = BinComponentState.Log;
                    break;

                case "setting":
                    localState = BinComponentState.Setting;
                    break;

                case "help":
                    OnlineHelp.InvokeShowDocPage(model.PluginType);
                    return;

                case "up":
                    ModifiedCanvas.RequestZIndexModification(VisualParent as ModifiedCanvas, this, ModifiedCanvas.ZPaneRequest.up);
                    return;

                case "down":
                    ModifiedCanvas.RequestZIndexModification(VisualParent as ModifiedCanvas, this, ModifiedCanvas.ZPaneRequest.down);
                    return;

                case "top":
                    ModifiedCanvas.RequestZIndexModification(VisualParent as ModifiedCanvas, this, ModifiedCanvas.ZPaneRequest.top);
                    return;

                case "bottom":
                    ModifiedCanvas.RequestZIndexModification(VisualParent as ModifiedCanvas, this, ModifiedCanvas.ZPaneRequest.bot);
                    return;
            }
            Editor.SetFullscreen(this, localState);
        }

        private void ActionHandler(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;

            if (b.Content is BinComponentState)
            {
                State = (BinComponentState)b.Content;
                return;
            }

            if (b.Content is BinComponentAction && ((BinComponentAction)b.Content) == BinComponentAction.LastState)
            {
                State = (BinComponentState)LastState;
                return;
            }

            if (b.Content is string)
            {
                string s = (string)b.Content;

                if (s == "Info")
                    OnlineHelp.InvokeShowDocPage(model.PluginType);
            }

            e.Handled = true;
        }

        private void ScaleDragDeltaHandler(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (State == BinComponentState.Min)
            {
                if ((Window.ActualHeight + e.VerticalChange >= Window.MinHeight + 15) && (Window.ActualWidth + e.HorizontalChange >= Window.MinHeight + 15))
                {
                    Model.WorkspaceModel.ModifyModel(new ResizeModelElementOperation(Model, 300, 200));
                    State = LastState;
                }
                else
                { return; }
            }

            if ((Window.ActualHeight + e.VerticalChange <= 80 - 15) && (Window.ActualWidth + e.HorizontalChange <= 80 - 15))
            {
                State = BinComponentState.Min;
            }
            else
            {
                WindowHeight += e.VerticalChange;
                WindowWidth += e.HorizontalChange;
                
            }

            Model.WorkspaceModel.ModifyModel(new ResizeModelElementOperation(Model, WindowWidth, WindowHeight));
            e.Handled = true;
        }

        void ConnectorDragged(object sender, IsDraggedEventArgs e)
        {
            IsConnectorDragStarted = e.IsDragged;
        }

        private void PositionDragDeltaHandler(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Delta = new Vector(e.HorizontalChange, e.VerticalChange);
            if (PositionDeltaChanged != null)
                PositionDeltaChanged.Invoke(this, new PositionDeltaChangedArgs() { PosDelta = Delta });

        }

        private void DragCompletedHandler(object sender, DragCompletedEventArgs e)
        {
            Delta = new Vector(0, 0);
        }

        private static void OnStateValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ComponentVisual bin = (ComponentVisual)d;
            if ((BinComponentState)e.OldValue != BinComponentState.Default)
                bin.LastState = (BinComponentState)e.OldValue;
            bin.OnPropertyChanged("LastState");
            bin.OnPropertyChanged("ActivePresentation");
            bin.OnPropertyChanged("HasComponentSetting");
            bin.OnPropertyChanged("HasComponentPresentation");
            if (bin.StateChanged != null)
                bin.StateChanged.Invoke(bin, new VisualStateChangedArgs() { State = bin.State });
            bin.Model.ViewState = (PluginViewState)Enum.Parse(typeof(PluginViewState), e.NewValue.ToString());
            if (bin.State == BinComponentState.Log)
                bin.IsErrorDisplayVisible = false;
        }


        private static void OnIsDraggingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ComponentVisual bin = (ComponentVisual)d;
            if (bin.IsDraggingChanged != null)
                bin.IsDraggingChanged.Invoke(bin, new IsDraggingChangedArgs() { IsDragging = bin.IsDragging });

        }

        private static void OnIsFullscreenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ComponentVisual bin = (ComponentVisual)d;
        }

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ComponentVisual bin = (ComponentVisual)d;
        }

        private static void OnCustomNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ComponentVisual bin = (ComponentVisual)d;
            bin.Model.WorkspaceModel.ModifyModel(new RenameModelElementOperation(bin.Model, (string)e.NewValue));
            /*if (bin.Model.WorkspaceModel.MyEditor != null)
            {
                ((WorkspaceManagerClass)bin.Model.WorkspaceModel.MyEditor).HasChanges = true;
            }*/
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            // process only if workspace is not running
            if (Model != null && !((WorkspaceManagerClass)Model.WorkspaceModel.MyEditor).isExecuting())
            {
                var list = new List<Operation>();
                //first delete all connections of this PluginModel
                foreach (var connector in Model.GetOutputConnectors())
                {
                    foreach (var connectionModel in connector.GetOutputConnections())
                    {
                        list.Add(new DeleteConnectionModelOperation(connectionModel));
                    }
                }
                foreach (var connector in Model.GetInputConnectors())
                {
                    foreach (var connectionModel in connector.GetInputConnections())
                    {
                        list.Add(new DeleteConnectionModelOperation(connectionModel));
                    }
                }
                //then delete the PluginModel
                var deletePluginModelOperation = new DeletePluginModelOperation(Model);
                list.Add(deletePluginModelOperation);
                Model.WorkspaceModel.ModifyModel(new MultiOperation(list));
            }
        }

        private void RepeatHandler(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b == null)
                return;

            Repeat = (bool)b.Content;
        }
        #endregion

        private void PreviewDragEnterHandler(object sender, DragEventArgs e)
        {
            //if (e.Data.GetDataPresent("BinConnector"))
            //{
            //    return;
            //}
            //else
            //{
            //    Mouse.OverrideCursor = Cursors.No;
            //}
        }

        private void PreviewDragLeaveHandler(object sender, DragEventArgs e)
        {
            //Mouse.OverrideCursor = null;
        }

        private void OpenClickHandler(object sender, RoutedEventArgs e)
        {
            Editor.SetFullscreen(this, State != BinComponentState.Min ? State : FullScreenState);
        }

        private void TitleThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {

        }
    }

    #region Events
    public class VisualStateChangedArgs : EventArgs
    {
        public BinComponentState State { get; set; }
    }

    public class PositionDeltaChangedArgs : EventArgs
    {
        public VisualElementModel Model { get; set; }
        public Vector PosDelta { get; set; }
    }

    public class ZIndexChangedArgs : EventArgs
    {
        public int ZIndex { get; set; }
    }

    public class IsDraggingChangedArgs : EventArgs
    {
        public bool IsDragging { get; set; }
    }
    #endregion

    #region Converter

    public class StateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is BinComponentState))
                return double.Epsilon;

            BinComponentState state = (BinComponentState)value;
            if (state != BinComponentState.Min)
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EnsureMinHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return double.Epsilon;

            var d = (double)value;
            var d2 = double.Parse(parameter.ToString());
            if (double.IsNaN(d) || d == 0)
                return d2;
            else
                return d + 60;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StateFullscreenConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null || !(values[0] is BinComponentState) || !(values[1] is bool))
                return double.Epsilon;

            BinComponentState state = (BinComponentState)values[0];
            bool b = (bool)values[1];
            if (state != BinComponentState.Min && !b)
                return true;
            else
                return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsDraggingConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return false;

            var x = value.OfType<bool>();

            return x.Any(y => y == true);
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class testconverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class WidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var baseElement = values[0] as FrameworkElement;
            var element = (double)values[1];

            return Math.Abs(element - baseElement.ActualWidth);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Custom class

    public class ThumHack : Thumb
    {
        public bool HackDrag
        {
            get
            {
                return IsDragging;
            }
            set
            {
                IsDragging = value;
            }
        }
    }

    public class Log
    {
        public NotificationLevel Level { get; set; }
        public String Message { get; set; }
        public String Date { get; set; }
        public String ID { get; set; }

        public Log(GuiLogEventArgs element)
        {
            Message = element.Message;
            Level = element.NotificationLevel;
            Date = element.DateTime.ToString("dd.MM.yyyy, H:mm:ss");
        }

        public override string ToString()
        {
            return Message;
        }
    }

    public class CustomTextBox : TextBox, INotifyPropertyChanged
    {
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected",
            typeof(bool), typeof(CustomTextBox), new FrameworkPropertyMetadata(false, OnIsSelectedChanged));

        public bool IsSelected
        {
            get { return (bool)base.GetValue(IsSelectedProperty); }
            set
            {
                base.SetValue(IsSelectedProperty, value);
            }
        }

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomTextBox bin = (CustomTextBox)d;
            if (bin.IsSelected)
                return;
            else
                bin.Focusable = false;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            Focusable = true;
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Focusable"));
            Focus();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                Focusable = false;
                if (PropertyChanged != null)
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Focusable"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class NSWEStackPanel : StackPanel
    {
        public PanelOrientation PanelOrientation { get; set; }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            ConnectorVisual cNew = visualAdded as ConnectorVisual;
            ConnectorVisual cOld = visualRemoved as ConnectorVisual;

            if (cNew != null)
            {
                cNew.Dragged += new EventHandler<IsDraggedEventArgs>(ConnectorDragged);
            }

            if (cOld != null)
            {
                cOld.Dragged -= new EventHandler<IsDraggedEventArgs>(ConnectorDragged);
                update();
            }
        }

        void ConnectorDragged(object sender, IsDraggedEventArgs e)
        {
            if (!e.IsDragged)
            {
                update();
            }
        }

        private void update()
        {
            IEnumerable<ConnectorVisual> filter = Children.OfType<ConnectorVisual>();

            foreach (ConnectorVisual child in filter)
            {
                child.RaiseUpdate();
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size resultSize = new Size(0, 0);

            IEnumerable<ConnectorVisual> filter = Children.OfType<ConnectorVisual>();

            foreach (ConnectorVisual child in filter)
            {
                switch (PanelOrientation)
                {
                    case Base.PanelOrientation.East:
                        if (child.Orientation == ConnectorOrientation.East)
                            continue;

                        child.Orientation = ConnectorOrientation.East;
                        if (child.IsOutgoing)
                            child.RotationAngle = (double)-90;
                        else
                            child.RotationAngle = (double)90;
                        break;
                    case Base.PanelOrientation.South:
                        if (child.Orientation == ConnectorOrientation.South)
                            continue;

                        child.Orientation = ConnectorOrientation.South;
                        if (child.IsOutgoing)
                            child.RotationAngle = (double)0;
                        else
                            child.RotationAngle = (double)180;
                        break;
                    case Base.PanelOrientation.West:
                        if (child.Orientation == ConnectorOrientation.West)
                            continue;

                        child.Orientation = ConnectorOrientation.West;
                        if (child.IsOutgoing)
                            child.RotationAngle = (double)90;
                        else
                            child.RotationAngle = (double)-90;
                        break;
                    case Base.PanelOrientation.North:
                        if (child.Orientation == ConnectorOrientation.North)
                            continue;

                        child.Orientation = ConnectorOrientation.North;
                        if (child.IsOutgoing)
                            child.RotationAngle = (double)180;
                        else
                            child.RotationAngle = (double)0;
                        break;
                }
                child.Measure(availableSize);
                resultSize.Width = Math.Max(resultSize.Width, child.DesiredSize.Width);
                resultSize.Height = Math.Max(resultSize.Height, child.DesiredSize.Height);
            }

            resultSize.Width = double.IsPositiveInfinity(availableSize.Width) ?
                resultSize.Width : availableSize.Width;

            resultSize.Height = double.IsPositiveInfinity(availableSize.Height) ?
                resultSize.Height : availableSize.Height;

            return resultSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children.Count == 0)
                return finalSize;

            double currentX = 0, currentY = 0, currentHeight = 0, currentWidth = 0;

            IEnumerable<ConnectorVisual> filter = Children.OfType<ConnectorVisual>();
            foreach (ConnectorVisual child in filter)
            {
                if (Orientation == System.Windows.Controls.Orientation.Vertical)
                {
                    currentY += currentHeight;
                    Point p = new Point(currentX, currentY);
                    child.Arrange(new Rect(currentX, currentY, child.DesiredSize.Width,
                        child.DesiredSize.Height));

                    currentHeight = child.DesiredSize.Height;
                    child.Position = null;
                    child.Position = p;
                }

                if (Orientation == System.Windows.Controls.Orientation.Horizontal)
                {
                    currentX += currentWidth;
                    Point p = new Point(currentX, currentY);
                    child.Arrange(new Rect(currentX, currentY, child.DesiredSize.Width,
                        child.DesiredSize.Height));

                    currentWidth = child.DesiredSize.Width;
                    child.Position = null;
                    child.Position = p;
                }

                child.IsDragged = false;
            }

            return finalSize;
        }
    }


    public class IControlMasterElement
    {
        public event EventHandler PluginModelChanged;

        private PluginModel pluginModel;
        public PluginModel PluginModel
        {
            get
            {
                return pluginModel;
            }
            set
            {
                pluginModel = value;

                if (value?.Plugin != null)
                {
                    PluginSettingsContainer = new PluginSettingsContainer(value.Plugin);
                }

                if (PluginModelChanged != null)
                    PluginModelChanged.Invoke(this, null);
            }
        }
        public ConnectorModel ConnectorModel { get; private set; }
        
        public PluginSettingsContainer PluginSettingsContainer { get; private set; }

        public IControlMasterElement(ConnectorModel connectorModel, PluginModel pluginModel)
        {
            this.ConnectorModel = connectorModel;
            this.PluginModel = pluginModel;
        }
    }

    #endregion
}
