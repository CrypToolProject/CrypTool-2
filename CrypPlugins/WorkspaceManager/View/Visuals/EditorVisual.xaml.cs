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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using CrypTool.Core;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Editor;
using CrypTool.PluginBase.Properties;
using WorkspaceManager.Base.Sort;
using WorkspaceManager.Model;
using WorkspaceManager.View.Base;
using WorkspaceManager.View.VisualComponents;
using WorkspaceManager.View.VisualComponents.CryptoLineView;
using WorkspaceManagerModel.Model.Interfaces;
using WorkspaceManagerModel.Model.Operations;
using WorkspaceManagerModel.Model.Tools;

namespace WorkspaceManager.View.Visuals
{
    public class VisualsHelper : INotifyPropertyChanged
    {
        private System.Drawing.RectangleF rect = new System.Drawing.RectangleF(-2000, -2000, 6000, 6000);
        private Point? startDragPoint;
        private Point? draggedFrom, draggedTo;
        private FromTo draggedFT;
        private readonly Popup pop;

        internal Line part = new Line();
        public ObservableCollection<UIElement> Visuals { get; set; }
        public QuadTreeLib.QuadTree<FakeNode> PluginTree { get; set; }
        public QuadTreeLib.QuadTree<FakeNode> FromToTree { get; set; }
        public CryptoLineView CurrentLine { get; set; }
        public CryptoLineView LastLine { get; set; }
        public int LineCount { get; set; }

        private FromTo selectedPart;
        public FromTo SelectedPart
        {
            get => selectedPart;
            set
            {
                selectedPart = value;
                OnPropertyChanged("SelectedPart");
            }
        }

        private WorkspaceModel model;
        public WorkspaceModel Model
        {
            get => model;
            set => model = value;
        }

        private EditorVisual editor;
        public EditorVisual Editor
        {
            get => editor;
            private set => editor = value;
        }

        private bool callback = true;
        private readonly DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(1000) };
        public VisualsHelper(WorkspaceModel model, EditorVisual editor)
        {
            Model = model;
            Editor = editor;
            PluginTree = new QuadTreeLib.QuadTree<FakeNode>();
            FromToTree = new QuadTreeLib.QuadTree<FakeNode>();
            timer.Tick += delegate (object o, EventArgs args)
            {
                callback = false;

                DispatcherOperation op = editor.Dispatcher.BeginInvoke(DispatcherPriority.Background, (SendOrPostCallback)delegate
                    {
                        IEnumerable<CryptoLineView> filter = Visuals.OfType<CryptoLineView>();

                        foreach (CryptoLineView element in filter)
                        {
                            element.Line.ClearIntersect();
                        }

                        foreach (CryptoLineView element in filter)
                        {
                            element.Line.DrawDecoration();
                        }

                        foreach (CryptoLineView element in filter)
                        {
                            element.Line.InvalidateVisual();
                        }
                    }, null);

                op.Completed += delegate
                    {
                        callback = true;
                    };

                timer.Stop();
            };

            Visuals = Editor.VisualCollection;
            Visuals.CollectionChanged += new NotifyCollectionChangedEventHandler(VisualsCollectionChanged);
            Visuals.Add(part);
        }


        internal void DrawDecoration()
        {
            if (!timer.IsEnabled && callback)
            {
                timer.Start();
            }
        }

        internal void panelPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            reset();
        }

        internal void panelMouseLeave(object sender, MouseEventArgs e)
        {
            reset();
        }

        internal void panelPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedPart != null)
            {
                draggedFT = SelectedPart;
            }

            startDragPoint = Util.MouseUtilities.CorrectGetPosition((UIElement)sender);
        }

        private void reset()
        {
            if (CurrentLine != null)
            {
                CurrentLine.Line.IsEditingPoint = false;
                LastLine = CurrentLine;
                DrawDecoration();
            }

            CurrentLine = null;
            SelectedPart = null;
            startDragPoint = null;
            draggedFrom = null;
            draggedTo = null;
            draggedFT = null;
        }

        internal void panelPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.Source is CryptoLineView && draggedFT == null)
            {
                CryptoLineView l = (CryptoLineView)e.Source;
                if (l.IsSelected)
                {
                    Point p = Mouse.GetPosition(sender as FrameworkElement);
                    List<FakeNode> list = FromToTree.Query(new System.Drawing.RectangleF(
                        (float)p.X - (float)1.5, (float)p.Y - (float)1.5, 3, 3));

                    //filter out all line segments with end and start points:
                    list = list.Where(element => element.FromTo.MetaData != FromToMeta.HasEndpoint && element.FromTo.MetaData != FromToMeta.HasStartPoint).ToList();

                    if (list.Count != 0)
                    {
                        if (CurrentLine == null)
                        {
                            CurrentLine = l;
                        }

                        foreach (FakeNode x in list)
                        {
                            if (x.LogicalParent == CurrentLine.Line)
                            {
                                SelectedPart = x.FromTo;
                            }
                        }
                    }
                }
            }
            else
            {
                if (draggedFT == null && !(e.Source is Line))
                {
                    SelectedPart = null;
                }
            }

            if (startDragPoint == null)
            {
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed && draggedFT != null && CurrentLine != null)
            {
                if (e.Source is CryptoLineView)
                {
                    if (e.Source as CryptoLineView != CurrentLine)
                    {
                        return;
                    }
                }

                Point currentPoint = Util.MouseUtilities.CorrectGetPosition(Editor.panel);
                Vector delta = Point.Subtract((Point)startDragPoint, currentPoint);
                delta.Negate();

                LinkedList<FromTo> linkedPointList = new LinkedList<FromTo>(CurrentLine.Line.PointList);
                FromTo data = draggedFT;
                LinkedListNode<FromTo> curData = linkedPointList.Find(draggedFT);
                if (curData == null)
                {
                    return;
                }

                LinkedListNode<FromTo> prevData = curData.Previous;
                if (prevData == null)
                {
                    return;
                }

                LinkedListNode<FromTo> nextData = curData.Next;
                if (nextData == null)
                {
                    return;
                }

                CurrentLine.Line.IsEditingPoint = true;
                CurrentLine.Line.HasManualModification = true;

                if (draggedFrom == null || draggedTo == null)
                {
                    draggedFrom = new Point(data.From.X, data.From.Y);
                    draggedTo = new Point(data.To.X, data.To.Y);
                }

                CurrentLine.Line.ClearIntersect();

                switch (data.DirSort)
                {
                    case DirSort.X_ASC:
                        data.From = prevData.Value.To = new Point(((Point)draggedFrom).X, ((Point)draggedFrom).Y + delta.Y);
                        data.To = nextData.Value.From = new Point(((Point)draggedTo).X, ((Point)draggedTo).Y + delta.Y);
                        break;
                    case DirSort.X_DESC:
                        data.From = prevData.Value.To = new Point(((Point)draggedFrom).X, ((Point)draggedFrom).Y + delta.Y);
                        data.To = nextData.Value.From = new Point(((Point)draggedTo).X, ((Point)draggedTo).Y + delta.Y);
                        break;
                    case DirSort.Y_ASC:
                        data.From = prevData.Value.To = new Point(((Point)draggedFrom).X + delta.X, ((Point)draggedFrom).Y);
                        data.To = nextData.Value.From = new Point(((Point)draggedTo).X + delta.X, ((Point)draggedTo).Y);
                        break;
                    case DirSort.Y_DESC:
                        data.From = prevData.Value.To = new Point(((Point)draggedFrom).X + delta.X, ((Point)draggedFrom).Y);
                        data.To = nextData.Value.From = new Point(((Point)draggedTo).X + delta.X, ((Point)draggedTo).Y);
                        break;
                }
                CurrentLine.Line.InvalidateVisual();

                foreach (FromTo fromTo in CurrentLine.Line.PointList)
                {
                    fromTo.Update();
                }

                CurrentLine.Line.Save();
            }

        }

        #region Protected
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        private void VisualsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null)
                    {
                        return;
                    }

                    if (e.NewItems[0] is ComponentVisual)
                    {
                        (e.NewItems[0] as ComponentVisual).IsDraggingChanged += new EventHandler<IsDraggingChangedArgs>(VisualsHelperIsDraggingChanged);
                        (e.NewItems[0] as ComponentVisual).Loaded += new RoutedEventHandler(VisualsHelper_Loaded);
                    }

                    if (e.NewItems[0] is CryptoLineView)
                    {
                        (e.NewItems[0] as CryptoLineView).Line.ComputationDone += new EventHandler<ComputationDoneEventArgs>(Line_ComputationDone);
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null)
                    {
                        return;
                    }
                    if (e.OldItems[0] is ComponentVisual)
                    {
                        (e.OldItems[0] as ComponentVisual).IsDraggingChanged -= new EventHandler<IsDraggingChangedArgs>(VisualsHelperIsDraggingChanged);
                        (e.OldItems[0] as ComponentVisual).Loaded -= new RoutedEventHandler(VisualsHelper_Loaded);
                    }
                    if (e.OldItems[0] is CryptoLineView)
                    {
                        (e.OldItems[0] as CryptoLineView).Line.ComputationDone -= new EventHandler<ComputationDoneEventArgs>(Line_ComputationDone);
                    }

                    break;
            }
        }

        private void Line_ComputationDone(object sender, ComputationDoneEventArgs e)
        {
            if (e.IsPathComputationDone)
            {
                renewFromToTree();
            }
        }

        private void VisualsHelper_Loaded(object sender, RoutedEventArgs e)
        {
            renewPluginTree();
        }

        private void VisualsHelperIsDraggingChanged(object sender, IsDraggingChangedArgs e)
        {
            if (!e.IsDragging)
            {
                renewPluginTree();
            }
        }

        private void renewPluginTree()
        {
            PluginTree = new QuadTreeLib.QuadTree<FakeNode>();
            foreach (ComponentVisual element in Visuals.OfType<ComponentVisual>())
            {
                PluginTree.Insert(new FakeNode()
                {
                    Rectangle = new System.Drawing.RectangleF((float)element.Position.X,
                                                               (float)element.Position.Y,
                                                               (float)element.ObjectSize.X,
                                                               (float)element.ObjectSize.Y)
                });
            }
        }

        private void renewFromToTree()
        {
            const uint stroke = 8;
            FromToTree = new QuadTreeLib.QuadTree<FakeNode>();
            foreach (CryptoLineView element in Visuals.OfType<CryptoLineView>())
            {
                foreach (FromTo fromTo in element.Line.PointList)
                {
                    FromToTree.Insert(new FakeNode()
                    {
                        Rectangle = fromTo.GetRectangle(stroke),
                        FromTo = fromTo,
                        LogicalParent = element.Line
                    });
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }

    /// <summary>
    /// Interaction logic for BinEditorVisual.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("WorkspaceManager.Properties.Resources")]
    public partial class EditorVisual : UserControl, IUpdateableView, INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler SelectedTextChanged;
        public event EventHandler SelectedConnectorChanged;
        public event EventHandler<SelectedItemsEventArgs> ItemsSelected;
        #endregion

        #region Fields
        internal ModifiedCanvas panel;
        private Window window;
        private ArevaloRectanglePacker packer;
        private ConnectorVisual from, to;
        private readonly RectangleGeometry selectRectGeometry = new RectangleGeometry();
        private bool startedSelection;
        private readonly CryptoLineView draggedLink;
        private readonly Path selectionPath = new Path()
        {
            Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3399ff")),
            Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffff")),
            StrokeThickness = 1,
            Opacity = 0.5
        };
        private Point? startDragPoint;
        #endregion

        #region Properties

        private VisualsHelper visualsHelper;
        public VisualsHelper VisualsHelper
        {
            get => visualsHelper;
            set
            {
                visualsHelper = value;
                OnPropertyChanged("VisualsHelper");
            }
        }

        private WorkspaceModel model;
        public WorkspaceModel Model
        {
            get => model;
            set
            {
                model = value;
                model.DeletedChildElement += DeleteChild;
                model.NewChildElement += NewChild;
                model.ChildPositionChanged += ChildPositionChanged;
                model.ChildSizeChanged += ChildSizeChanged;
                model.ChildNameChanged += ChildNameChanged;

                if (model.Zoom != 0)
                {
                    ZoomLevel = model.Zoom;
                }
            }
        }

        public WorkspaceManagerClass MyEditor { get; private set; }

        public FullscreenVisual FullscreenVisual => (FullscreenVisual)FullScreen.Content;

        private ObservableCollection<UIElement> selectedItemsObservable = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> SelectedItemsObservable
        {
            get => selectedItemsObservable;
            private set => selectedItemsObservable = value;
        }

        private ObservableCollection<UIElement> visualCollection = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> VisualCollection
        {
            get => visualCollection;
            private set => visualCollection = value;
        }

        private ObservableCollection<ComponentVisual> componentCollection = new ObservableCollection<ComponentVisual>();
        public ObservableCollection<ComponentVisual> ComponentCollection
        {
            get => componentCollection;
            private set => componentCollection = value;
        }

        private ObservableCollection<CryptoLineView> pathCollection = new ObservableCollection<CryptoLineView>();
        public ObservableCollection<CryptoLineView> PathCollection
        {
            get => pathCollection;
            private set => pathCollection = value;
        }

        #endregion

        #region DependencyProperties

        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register("Progress",
            typeof(double), typeof(EditorVisual), new FrameworkPropertyMetadata((double)0));

        public double Progress
        {
            get => (double)base.GetValue(ProgressProperty);
            set => base.SetValue(ProgressProperty, value);
        }

        public static readonly DependencyProperty IsSettingsOpenProperty = DependencyProperty.Register("IsSettingsOpen",
            typeof(bool), typeof(EditorVisual), new FrameworkPropertyMetadata(true));



        public string ProgressDuration
        {
            get => (string)base.GetValue(ProgressDurationProperty);
            set => base.SetValue(ProgressDurationProperty, value);
        }

        public static readonly DependencyProperty ProgressDurationProperty = DependencyProperty.Register("ProgressDuration",
          typeof(string), typeof(EditorVisual), new FrameworkPropertyMetadata(string.Empty));


        public bool IsSettingsOpen
        {
            get => (bool)base.GetValue(IsSettingsOpenProperty);
            set => base.SetValue(IsSettingsOpenProperty, value);
        }

        public static readonly DependencyProperty IsLinkingProperty = DependencyProperty.Register("IsLinking",
            typeof(bool), typeof(EditorVisual), new FrameworkPropertyMetadata(false, OnIsLinkingChanged));

        public bool IsLinking
        {
            get => (bool)base.GetValue(IsLinkingProperty);
            set => base.SetValue(IsLinkingProperty, value);
        }

        public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register("ZoomLevel",
            typeof(double), typeof(EditorVisual), new FrameworkPropertyMetadata((double)1, OnZoomLevelChanged));

        public double ZoomLevel
        {
            get => (double)base.GetValue(ZoomLevelProperty);
            set => base.SetValue(ZoomLevelProperty, value);
        }

        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State",
            typeof(BinEditorState), typeof(EditorVisual), new FrameworkPropertyMetadata(BinEditorState.READY, null));

        public BinEditorState State
        {
            get => (BinEditorState)base.GetValue(StateProperty);
            set => base.SetValue(StateProperty, value);
        }

        public static readonly DependencyProperty SelectedConnectorProperty = DependencyProperty.Register("SelectedConnector",
            typeof(ConnectorVisual), typeof(EditorVisual), new FrameworkPropertyMetadata(null, OnSelectedConnectorChanged));

        public ConnectorVisual SelectedConnector
        {
            get => (ConnectorVisual)base.GetValue(SelectedConnectorProperty);
            private set => base.SetValue(SelectedConnectorProperty, value);
        }

        public static readonly DependencyProperty SelectedTextProperty = DependencyProperty.Register("SelectedText",
            typeof(TextVisual), typeof(EditorVisual), new FrameworkPropertyMetadata(null, OnSelectedTextChanged));

        public TextVisual SelectedText
        {
            get => (TextVisual)base.GetValue(SelectedTextProperty);
            set => base.SetValue(SelectedTextProperty, value);
        }

        public static readonly DependencyProperty SelectedImageProperty = DependencyProperty.Register("SelectedImage",
            typeof(ImageVisual), typeof(EditorVisual), new FrameworkPropertyMetadata(null, OnSelectedImageChanged));

        public ImageVisual SelectedImage
        {
            get => (ImageVisual)base.GetValue(SelectedImageProperty);
            set => base.SetValue(SelectedImageProperty, value);
        }

        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register("SelectedItems",
            typeof(UIElement[]), typeof(EditorVisual), new FrameworkPropertyMetadata(null, OnSelectedItemChanged));

        public UIElement[] SelectedItems
        {
            get => (UIElement[])base.GetValue(SelectedItemsProperty);
            set => base.SetValue(SelectedItemsProperty, value);
        }

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading",
            typeof(bool), typeof(EditorVisual), new FrameworkPropertyMetadata(false, OnIsLoadingChanged));

        public bool IsLoading
        {
            get => (bool)base.GetValue(IsLoadingProperty);
            set => base.SetValue(IsLoadingProperty, value);
        }

        public static readonly DependencyProperty IsFullscreenOpenProperty = DependencyProperty.Register("IsFullscreenOpen",
            typeof(bool), typeof(EditorVisual), new FrameworkPropertyMetadata(false, null));


        public bool IsFullscreenOpen
        {
            get => (bool)base.GetValue(IsFullscreenOpenProperty);
            set => base.SetValue(IsFullscreenOpenProperty, value);
        }

        public static readonly DependencyProperty IsExecutingProperty = DependencyProperty.Register("IsExecuting",
            typeof(bool), typeof(EditorVisual), new FrameworkPropertyMetadata(false, null));


        public bool IsExecuting
        {
            get => (bool)base.GetValue(IsExecutingProperty);
            set => base.SetValue(IsExecutingProperty, value);
        }

        public static readonly DependencyProperty HasLoadingErrorProperty = DependencyProperty.Register("HasLoadingError",
            typeof(bool), typeof(EditorVisual), new FrameworkPropertyMetadata(false, null));

        public bool HasLoadingError
        {
            get => (bool)base.GetValue(HasLoadingErrorProperty);
            set => base.SetValue(HasLoadingErrorProperty, value);
        }

        public static readonly DependencyProperty LoadingErrorTextProperty = DependencyProperty.Register("LoadingErrorText",
            typeof(string), typeof(EditorVisual), new FrameworkPropertyMetadata(string.Empty, null));

        public string LoadingErrorText
        {
            get => (string)base.GetValue(LoadingErrorTextProperty);
            set => base.SetValue(LoadingErrorTextProperty, value);
        }

        #endregion

        #region Constructors
        public EditorVisual(WorkspaceModel model)
        {
            Model = model;
            VisualsHelper = new VisualsHelper(Model, this);
            MyEditor = (WorkspaceManagerClass)Model.MyEditor;
            MyEditor.executeEvent += new EventHandler(ExecuteEvent);
            VisualCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(CollectionChangedHandler);
            VisualCollection.Add(selectionPath);
            draggedLink = new CryptoLineView(VisualCollection);
            MyEditor.LoadingErrorOccurred += new EventHandler<LoadingErrorEventArgs>(LoadingErrorOccurred);
            MyEditor.PasteOccured += new EventHandler(PasteOccured);
            InitializeComponent();
            _usagePopup = new UsageStatisticPopup(this);
            IsSettingsOpen = false; //Settings (sidebar) start in state closed
        }

        #endregion

        #region Public

        public int NumberOfSelectedItems
        {
            get
            {
                if (SelectedItems == null)
                {
                    return 0;
                }

                return SelectedItems.OfType<ComponentVisual>().Count();
            }
        }

        public bool HasSelectedItems
        {
            get
            {
                if (MyEditor.isExecuting())
                {
                    return false;
                }

                return NumberOfSelectedItems > 0;
            }
        }

        public bool HasSeveralSelectedItems
        {
            get
            {
                if (MyEditor.isExecuting())
                {
                    return false;
                }

                return NumberOfSelectedItems > 1;
            }
        }

        private void PasteOccured(object sender, EventArgs e)
        {
            UIElement[] concat = new UIElement[0];

            foreach (ComponentVisual bin in ComponentCollection)
            {
                if (MyEditor.CurrentCopies.Contains(bin.Model))
                {
                    concat = concat.Concat(new UIElement[] { bin }).ToArray();
                }
            }
            SelectedItems = concat;
        }

        public void AddComponentVisual(PluginModel pluginModel)
        {
            if (State != BinEditorState.READY)
            {
                return;
            }

            ComponentVisual bin = new ComponentVisual(pluginModel);
            Binding bind = new Binding
            {
                Path = new PropertyPath(EditorVisual.SelectedItemsProperty),
                Source = this,
                ConverterParameter = bin,
                Converter = new SelectionChangedConverter()
            };
            bin.SetBinding(ComponentVisual.IsSelectedProperty, bind);
            bin.PositionDeltaChanged += new EventHandler<PositionDeltaChangedArgs>(ComponentPositionDeltaChanged);

            VisualCollection.Add(bin);

            //We initialize the settings after all other procedures have been finsihed, thus, settings, that are set
            //to invisible, are not shown in the settings bar
            if (pluginModel.Plugin.Settings != null)
            {
                pluginModel.Plugin.Settings.Initialize();
            }
        }

        public DispatcherOperation Load(WorkspaceModel model, bool isPaste = false)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            if (!isPaste)
            {
                Model = model;
                DispatcherOperation op = internalLoad(model);
                op.Completed += delegate
                {
                    IsLoading = false;
                };
                return op;
            }
            else
            {
                internalPasteLoad(model);
            }
            return null;
        }

        private void internalPasteLoad(WorkspaceModel model)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (SendOrPostCallback)delegate
            {
                WorkspaceModel m = model;

                foreach (PluginModel pluginModel in m.GetAllPluginModels())
                {
                    this.model.addPluginModel(pluginModel);
                    bool skip = false;
                    foreach (ConnectorModel connModel in pluginModel.GetInputConnectors())
                    {
                        if (connModel.IControl && connModel.GetInputConnections().Count > 0)
                        {
                            skip = true;
                            break;
                        }
                    }
                    if (!skip)
                    {
                        AddComponentVisual(pluginModel);
                    }
                }

                foreach (ConnectionModel connModel in m.GetAllConnectionModels())
                {
                    if (connModel.To.IControl)
                    {
                        continue;
                    }

                    foreach (UIElement element in VisualCollection)
                    {
                        ComponentVisual bin = element as ComponentVisual;
                        if (bin != null)
                        {
                            foreach (ConnectorVisual connector in bin.ConnectorCollection)
                            {
                                if (connModel.From == connector.Model)
                                {
                                    from = connector;
                                }
                                else if (connModel.To == connector.Model)
                                {
                                    to = connector;
                                }
                            }
                        }
                    }

                    AddConnectionVisual(from, to, connModel);
                }
                PartialCopyHelper.CurrentSelection = null;
            }
            , null);
        }

        public void ResetConnections()
        {
            foreach (CryptoLineView view in PathCollection)
            {
                view.Line.reset();
            }
        }

        public static Rect BoundsRelativeTo(FrameworkElement element, Visual relativeTo)
        {
            return
              element.TransformToVisual(relativeTo)
                     .TransformBounds(LayoutInformation.GetLayoutSlot(element));
        }

        /// <summary>
        /// TODO: Optimise this algorithm.
        /// </summary>
        public void FitToScreen()
        {
            if (ComponentCollection.Count == 0)
            {
                return;
            }

            if (ScrollViewer.ScrollableWidth > 0 || ScrollViewer.ScrollableHeight > 0)
            {
                while (ZoomLevel > Settings.Default.WorkspaceManager_MinScale
                    && (ScrollViewer.ScrollableHeight > 0
                    || ScrollViewer.ScrollableWidth > 0))
                {
                    ZoomLevel -= 0.02;
                    ScrollViewer.UpdateLayout();
                }
            }
            else
            {
                while (ZoomLevel < Settings.Default.WorkspaceManager_MaxScale
                    && ScrollViewer.ScrollableHeight == 0
                    && ScrollViewer.ScrollableWidth == 0)
                {
                    ZoomLevel += 0.02;
                    ScrollViewer.UpdateLayout();
                }
                if (ScrollViewer.ScrollableHeight > 0
                    || ScrollViewer.ScrollableWidth > 0)
                {
                    ZoomLevel -= 0.02;
                }
            }
        }

        public void ResetPlugins(int value)
        {
            if (value == 0)
            {
                foreach (ComponentVisual b in ComponentCollection)
                {
                    b.Progress = 0;
                }
            }

            if (value == 1)
            {
                foreach (ComponentVisual b in ComponentCollection)
                {
                    b.LogMessages.Clear();
                }
            }
        }

        public void AddText()
        {
            try
            {
                Model.ModifyModel(new NewTextModelOperation());
            }
            catch (Exception e)
            {
                MyEditor.GuiLogMessage(string.Format("Could not load Text to Workspace: {0}", e.Message), NotificationLevel.Error);
            }
        }

        public void AddImage(Uri uri)
        {
            try
            {
                ImageVisual bin = new ImageVisual((ImageModel)Model.ModifyModel(new NewImageModelOperation(uri)));
                bin.PositionDeltaChanged += new EventHandler<PositionDeltaChangedArgs>(ComponentPositionDeltaChanged);
                VisualCollection.Add(bin);
            }
            catch (Exception e)
            {
                MyEditor.GuiLogMessage(string.Format("Could not add image to workspace: {0}", e.Message), NotificationLevel.Error);
            }
        }
        #endregion

        #region Private

        private DispatcherOperation internalLoad(object model)
        {
            IsLoading = true;
            return Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (SendOrPostCallback)delegate
            {
                WorkspaceModel m = (WorkspaceModel)model;
                foreach (PluginModel pluginModel in m.GetAllPluginModels())
                {
                    bool skip = false;
                    foreach (ConnectorModel connModel in pluginModel.GetInputConnectors())
                    {
                        if (connModel.IControl && connModel.GetInputConnections().Count > 0)
                        {
                            skip = true;
                            break;
                        }
                    }
                    if (!skip)
                    {
                        AddComponentVisual(pluginModel);
                    }
                }

                foreach (ConnectionModel connModel in m.GetAllConnectionModels())
                {
                    if (connModel.To.IControl)
                    {
                        continue;
                    }

                    foreach (UIElement element in VisualCollection)
                    {
                        ComponentVisual bin = element as ComponentVisual;
                        if (bin != null)
                        {
                            foreach (ConnectorVisual connector in bin.ConnectorCollection)
                            {
                                if (connModel.From == connector.Model)
                                {
                                    from = connector;
                                }
                                else if (connModel.To == connector.Model)
                                {
                                    to = connector;
                                }
                            }
                        }
                    }

                    AddConnectionVisual(from, to, connModel);
                }

                foreach (ImageModel img in m.GetAllImageModels())
                {
                    ImageVisual visual = new ImageVisual(img);
                    visual.PositionDeltaChanged += new EventHandler<PositionDeltaChangedArgs>(ComponentPositionDeltaChanged);
                    VisualCollection.Add(visual);
                }

                foreach (TextModel txt in m.GetAllTextModels())
                {
                    try
                    {
                        TextVisual visual = new TextVisual(txt);
                        visual.PositionDeltaChanged += new EventHandler<PositionDeltaChangedArgs>(ComponentPositionDeltaChanged);
                        VisualCollection.Add(visual);
                    }
                    catch (Exception e)
                    {
                        MyEditor.GuiLogMessage(string.Format("Could not load Text to Workspace: {0}", e.Message), NotificationLevel.Error);
                    }
                }
            }
            , null);
        }

        public void AddConnectionVisual(ConnectorVisual source, ConnectorVisual target, ConnectionModel model)
        {
            if (State != BinEditorState.READY || source == null || target == null)
            {
                return;
            }

            CryptoLineView link = new CryptoLineView(model, source, target);
            Binding bind = new Binding
            {
                Path = new PropertyPath(EditorVisual.SelectedItemsProperty),
                Source = this,
                ConverterParameter = link,
                Converter = new SelectionChangedConverter()
            };
            link.SetBinding(CryptoLineView.IsSelectedProperty, bind);
            VisualCollection.Add(link);
        }

        private void reset()
        {
            VisualCollection.Remove(draggedLink);
            SelectedConnector = null;
            IsLinking = false;
            Mouse.OverrideCursor = null;
        }

        private readonly UsageStatisticPopup _usagePopup;

        internal void SetFullscreen(ComponentVisual bin, BinComponentState state)
        {
            FullscreenVisual.ActiveComponent = bin;
            bin.FullScreenState = state;
            IsFullscreenOpen = true;
        }

        private void dragReset()
        {
            selectionPath.Data = null;
            startDragPoint = null;

            if (!startedSelection)
            {
                SelectedItems = null;
            }

            startedSelection = false;
        }

        private void removeDragWindowHandle()
        {
            if (window != null)
            {
                window.PreviewMouseMove -= new MouseEventHandler(WindowPreviewMouseMove);
                window.PreviewMouseLeftButtonUp -= new MouseButtonEventHandler(WindowPreviewMouseLeftButtonUp);
                window.MouseLeave -= new MouseEventHandler(WindowMouseLeave);
            }
        }

        private void setDragWindowHandle()
        {
            if (window != null)
            {
                window.PreviewMouseMove += new MouseEventHandler(WindowPreviewMouseMove);
                window.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(WindowPreviewMouseLeftButtonUp);
                window.MouseLeave += new MouseEventHandler(WindowMouseLeave);
            }
        }

        #endregion

        #region Protected
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        #region Model Handler

        /// <summary>
        /// A child is deleted on model side
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void DeleteChild(object sender, ModelArgs args)
        {
            if (State == BinEditorState.READY)
            {
                if (args.EffectedModelElement is ConnectionModel)
                {
                    if (((ConnectionModel)args.EffectedModelElement).UpdateableView != null)
                    {
                        UIElement uielement = (UIElement)((ConnectionModel)args.EffectedModelElement).UpdateableView;
                        if (VisualCollection.Contains(uielement))
                        {
                            VisualCollection.Remove(uielement);
                        }
                    }
                }
                else if (args.EffectedModelElement is PluginModel)
                {
                    if (((PluginModel)args.EffectedModelElement).UpdateableView != null)
                    {
                        UIElement uielement = (UIElement)((PluginModel)args.EffectedModelElement).UpdateableView;
                        if (VisualCollection.Contains(uielement))
                        {
                            VisualCollection.Remove(uielement);
                        }
                    }
                }
                else if (args.EffectedModelElement is ImageModel)
                {
                    if (((ImageModel)args.EffectedModelElement).UpdateableView != null)
                    {
                        UIElement uielement = (UIElement)((ImageModel)args.EffectedModelElement).UpdateableView;
                        if (VisualCollection.Contains(uielement))
                        {
                            VisualCollection.Remove(uielement);
                        }
                    }
                }
                else if (args.EffectedModelElement is TextModel)
                {
                    if (((TextModel)args.EffectedModelElement).UpdateableView != null)
                    {
                        UIElement uielement = (UIElement)((TextModel)args.EffectedModelElement).UpdateableView;
                        if (VisualCollection.Contains(uielement))
                        {
                            VisualCollection.Remove(uielement);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// A child is created on model side
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void NewChild(object sender, ModelArgs args)
        {
            if (args.EffectedModelElement is ConnectionModel)
            {
                if (((ConnectionModel)args.EffectedModelElement).UpdateableView != null)
                {
                    CryptoLineView conn = (CryptoLineView)((ConnectionModel)args.EffectedModelElement).UpdateableView;
                    if (!VisualCollection.Contains(conn))
                    {
                        VisualCollection.Add(conn);
                    }
                }
                else
                {
                    ConnectionModel model = (ConnectionModel)args.EffectedModelElement;
                    if (!model.To.IControl)
                    {
                        AddConnectionVisual((ConnectorVisual)model.From.UpdateableView, (ConnectorVisual)model.To.UpdateableView, model);
                    }
                }
            }
            else if (args.EffectedModelElement is PluginModel)
            {
                if (((PluginModel)args.EffectedModelElement).UpdateableView != null)
                {
                    ComponentVisual plugin = (ComponentVisual)((PluginModel)args.EffectedModelElement).UpdateableView;
                    if (!VisualCollection.Contains(plugin))
                    {
                        VisualCollection.Add(plugin);
                    }
                }
                else
                {
                    PluginModel pluginModel = (PluginModel)args.EffectedModelElement;
                    bool skip = false;
                    foreach (ConnectorModel connModel in pluginModel.GetInputConnectors())
                    {
                        if (connModel.IControl && connModel.GetInputConnections().Count > 0)
                        {
                            skip = true;
                            break;
                        }
                    }
                    if (!skip)
                    {
                        AddComponentVisual(pluginModel);
                    }
                }
            }
            else if (args.EffectedModelElement is ImageModel)
            {
                if (((ImageModel)args.EffectedModelElement).UpdateableView != null)
                {

                    ImageVisual img = (ImageVisual)((ImageModel)args.EffectedModelElement).UpdateableView;
                    if (!VisualCollection.Contains(img))
                    {
                        VisualCollection.Add(img);
                    }
                }
                else
                {
                    ImageVisual bin = new ImageVisual(((ImageModel)args.EffectedModelElement));
                    bin.PositionDeltaChanged += new EventHandler<PositionDeltaChangedArgs>(ComponentPositionDeltaChanged);
                    SelectedImage = bin;
                    VisualCollection.Add(bin);
                }
            }
            else if (args.EffectedModelElement is TextModel)
            {
                if (((TextModel)args.EffectedModelElement).UpdateableView != null)
                {
                    TextVisual txt = (TextVisual)((TextModel)args.EffectedModelElement).UpdateableView;
                    if (!VisualCollection.Contains(txt))
                    {
                        VisualCollection.Add(txt);
                    }
                }
                else
                {
                    TextVisual bin = new TextVisual(((TextModel)args.EffectedModelElement));
                    bin.PositionDeltaChanged += new EventHandler<PositionDeltaChangedArgs>(ComponentPositionDeltaChanged);
                    Paragraph p = new Paragraph();
                    bin.mainRTB.Document.Blocks.Add(p);
                    SelectedText = bin;
                    VisualCollection.Add(bin);
                }
            }
        }

        /// <summary>
        /// The position of a child has changed on model side
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void ChildPositionChanged(object sender, PositionArgs args)
        {
            if (args.OldPosition.Equals(args.NewPosition))
            {
                return;
            }
            else if (args.EffectedModelElement is PluginModel)
            {
                if (((PluginModel)args.EffectedModelElement).UpdateableView != null)
                {
                    ComponentVisual bin = (ComponentVisual)((PluginModel)args.EffectedModelElement).UpdateableView;
                    bin.Position = args.NewPosition;
                }
            }
            else if (args.EffectedModelElement is ImageModel)
            {
                if (((ImageModel)args.EffectedModelElement).UpdateableView != null)
                {
                    ImageVisual img = (ImageVisual)((ImageModel)args.EffectedModelElement).UpdateableView;
                    img.Position = args.NewPosition;
                }
            }
            else if (args.EffectedModelElement is TextModel)
            {
                if (((TextModel)args.EffectedModelElement).UpdateableView != null)
                {
                    TextVisual txt = (TextVisual)((TextModel)args.EffectedModelElement).UpdateableView;
                    txt.Position = args.NewPosition;
                }
            }
        }

        /// <summary>
        /// The size of a child changed on model side
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void ChildSizeChanged(object sender, SizeArgs args)
        {
            if (args.NewHeight.Equals(args.OldHeight) &&
               args.NewWidth.Equals(args.OldWidth))
            {
                return;
            }
            else if (args.EffectedModelElement is PluginModel)
            {
                if (((PluginModel)args.EffectedModelElement).UpdateableView != null)
                {
                    ComponentVisual pluginContainerView = (ComponentVisual)((PluginModel)args.EffectedModelElement).UpdateableView;
                    pluginContainerView.WindowWidth = args.NewWidth;
                    pluginContainerView.WindowHeight = args.NewHeight;
                }
            }
            else if (args.EffectedModelElement is ImageModel)
            {
                if (((ImageModel)args.EffectedModelElement).UpdateableView != null)
                {
                    ImageVisual imgWrapper = (ImageVisual)((ImageModel)args.EffectedModelElement).UpdateableView;
                    imgWrapper.WindowWidth = args.NewWidth;
                    imgWrapper.WindowHeight = args.NewHeight;
                }
            }
            else if (args.EffectedModelElement is TextModel)
            {
                if (((TextModel)args.EffectedModelElement).UpdateableView != null)
                {
                    TextVisual txtWrapper = (TextVisual)((TextModel)args.EffectedModelElement).UpdateableView;
                    txtWrapper.WindowWidth = args.NewWidth;
                    txtWrapper.WindowHeight = args.NewHeight;
                }
            }
        }

        /// <summary>
        /// The size of a child changed on model side
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ChildNameChanged(object sender, NameArgs args)
        {
            if (args.NewName == null || args.NewName.Equals(args.Oldname))
            {
                return;
            }

            if (args.EffectedModelElement is PluginModel)
            {
                ComponentVisual bin = (ComponentVisual)((PluginModel)args.EffectedModelElement).UpdateableView;
                bin.CustomName = args.NewName;
            }
        }
        #endregion

        #region Event Handler

        public static RoutedCommand AlignLeft = new RoutedCommand("AlignLeft", typeof(RoutedCommand), new InputGestureCollection() { new KeyGesture(Key.Left, ModifierKeys.Control) });
        public static RoutedCommand AlignRight = new RoutedCommand("AlignRight", typeof(RoutedCommand), new InputGestureCollection() { new KeyGesture(Key.Right, ModifierKeys.Control) });
        public static RoutedCommand AlignTop = new RoutedCommand("AlignTop", typeof(RoutedCommand), new InputGestureCollection() { new KeyGesture(Key.Up, ModifierKeys.Control) });
        public static RoutedCommand AlignBottom = new RoutedCommand("AlignBottom", typeof(RoutedCommand), new InputGestureCollection() { new KeyGesture(Key.Down, ModifierKeys.Control) });
        public static RoutedCommand MoveLeft = new RoutedCommand("MoveLeft", typeof(RoutedCommand), new InputGestureCollection() { new KeyGesture(Key.Left, ModifierKeys.Control | ModifierKeys.Shift) });
        public static RoutedCommand MoveRight = new RoutedCommand("MoveRight", typeof(RoutedCommand), new InputGestureCollection() { new KeyGesture(Key.Right, ModifierKeys.Control | ModifierKeys.Shift) });
        public static RoutedCommand MoveUp = new RoutedCommand("MoveUp", typeof(RoutedCommand), new InputGestureCollection() { new KeyGesture(Key.Up, ModifierKeys.Control | ModifierKeys.Shift) });
        public static RoutedCommand MoveDown = new RoutedCommand("MoveDown", typeof(RoutedCommand), new InputGestureCollection() { new KeyGesture(Key.Down, ModifierKeys.Control | ModifierKeys.Shift) });
        public static RoutedCommand SpreadHorizontal = new RoutedCommand("SpreadHorizontal", typeof(RoutedCommand), new InputGestureCollection() { new KeyGesture(Key.X, ModifierKeys.Control | ModifierKeys.Shift) });
        public static RoutedCommand SpreadVertical = new RoutedCommand("SpreadVertical", typeof(RoutedCommand), new InputGestureCollection() { new KeyGesture(Key.Y, ModifierKeys.Control | ModifierKeys.Shift) });
        public static RoutedCommand BiggestWidth = new RoutedCommand("BiggestWidth", typeof(RoutedCommand), new InputGestureCollection() { new KeyGesture(Key.W, ModifierKeys.Control | ModifierKeys.Shift) });
        public static RoutedCommand SmallestWidth = new RoutedCommand("SmallestWidth", typeof(RoutedCommand), new InputGestureCollection() { new KeyGesture(Key.W, ModifierKeys.Control) });
        public static RoutedCommand BiggestHeight = new RoutedCommand("BiggestHeight", typeof(RoutedCommand), new InputGestureCollection() { new KeyGesture(Key.H, ModifierKeys.Control | ModifierKeys.Shift) });
        public static RoutedCommand SmallestHeight = new RoutedCommand("SmallestHeight", typeof(RoutedCommand), new InputGestureCollection() { new KeyGesture(Key.H, ModifierKeys.Control) });
        public static RoutedCommand RearrangeAllLines = new RoutedCommand("RearrangeAllLines", typeof(RoutedCommand), new InputGestureCollection() { new KeyGesture(Key.R, ModifierKeys.Control) });

        private void RearrangeAllLinesCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !MyEditor.isExecuting();
        }

        private void RearrangeAllLinesCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (CryptoLineView line in VisualCollection.OfType<CryptoLineView>())
            {
                line.Line.Rearrange();
            }
        }

        private void LayoutCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!MyEditor.isExecuting() && SelectedItems != null)
            {
                int n = SelectedItems.OfType<ComponentVisual>().Count();
                string name = ((RoutedCommand)e.Command).Name;
                if (name.StartsWith("Align"))
                {
                    e.CanExecute = n > 1;
                }
                else if (name.StartsWith("Spread"))
                {
                    e.CanExecute = n > 2;
                }
                else if (name.StartsWith("Move"))
                {
                    e.CanExecute = n > 0;
                }
                else
                {
                    e.CanExecute = n > 1;
                }
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void LayoutCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            string name = ((RoutedCommand)e.Command).Name;
            List<Operation> list = DoSelectionOperation(name);

            if (list != null && list.Count > 0)
            {
                Model.ModifyModel(new MultiOperation(list));
            }
        }

        private List<Operation> DoSelectionOperation(string operation)
        {
            if (SelectedItems == null)
            {
                return null;
            }
            List<ComponentVisual> components = SelectedItems.OfType<ComponentVisual>().ToList();

            switch (operation)
            {
                case "AlignLeft":
                    return Selection_Align(components, Direction.Left);
                case "AlignRight":
                    return Selection_Align(components, Direction.Right);
                case "AlignTop":
                    return Selection_Align(components, Direction.Top);
                case "AlignBottom":
                    return Selection_Align(components, Direction.Bottom);
                case "MoveLeft":
                    return Selection_Move(components, Direction.Left);
                case "MoveRight":
                    return Selection_Move(components, Direction.Right);
                case "MoveUp":
                    return Selection_Move(components, Direction.Top);
                case "MoveDown":
                    return Selection_Move(components, Direction.Bottom);
                case "SpreadHorizontal":
                    return Selection_Spread(components, Orientation.Horizontal);
                case "SpreadVertical":
                    return Selection_Spread(components, Orientation.Vertical);
                case "BiggestWidth":
                    return Selection_UniformSize(components, Orientation.Horizontal, true);
                case "SmallestWidth":
                    return Selection_UniformSize(components, Orientation.Horizontal, false);
                case "BiggestHeight":
                    return Selection_UniformSize(components, Orientation.Vertical, true);
                case "SmallestHeight":
                    return Selection_UniformSize(components, Orientation.Vertical, false);
            }

            return null;
        }

        private void ComponentPositionDeltaChanged(object sender, PositionDeltaChangedArgs e)
        {
            try
            {
                //moving is not allowed, when workspace is being executed
                //also, moving while conntrol pressed is not allowed
                if (MyEditor.isExecuting() || Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    return;
                }

                List<Operation> list = new List<Operation>();

                if (sender is ComponentVisual)
                {
                    if (SelectedItems != null && SelectedItems.Length > 0)
                    {
                        IEnumerable<ComponentVisual> components = SelectedItems.OfType<ComponentVisual>();
                        Point min = new Point(components.Select(p => p.Position.X).Min(), components.Select(p => p.Position.Y).Min());  // upper left corner of component bounding box
                        Vector delta = new Vector(Math.Max(e.PosDelta.X, -min.X), Math.Max(e.PosDelta.Y, -min.Y));

                        if (delta.LengthSquared > 0.00001)
                        {  // don't add non-movements to undo list
                            foreach (ComponentVisual element in components)
                            {
                                list.Add(new MoveModelElementOperation(element.Model, element.Position + delta));
                            }
                        }
                    }
                }
                else
                {
                    Point pos = ((Base.Interfaces.IRouting)sender).Position;
                    Vector delta = new Vector(Math.Max(e.PosDelta.X, -pos.X), Math.Max(e.PosDelta.Y, -pos.Y));
                    if (delta.LengthSquared > 0.00001)
                    {
                        list.Add(new MoveModelElementOperation(e.Model, pos + delta));
                    }
                }

                if (list.Count > 0)
                {
                    Model.ModifyModel(new MultiOperation(list));
                }
            }
            catch (Exception)
            {
                //do nothing here
            }
        }

        private void ExecuteEvent(object sender, EventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                IsExecuting = MyEditor.isExecuting();
            }, null);
        }

        private void CopyToClipboardClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(LoadingErrorText);
            }
            catch (Exception)
            {
                //1ms
                DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(10000) };
                timer.Start();
                timer.Tick += new EventHandler(delegate (object timerSender, EventArgs ee)
                {
                    DispatcherTimer t = (DispatcherTimer)timerSender;
                    t.Stop();
                    Clipboard.SetText(LoadingErrorText);
                });
            }
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EditorVisual b = (EditorVisual)d;
            UIElement[] newItem = e.NewValue as UIElement[];
            UIElement[] oldItem = e.OldValue as UIElement[];

            if (b.ItemsSelected != null)
            {
                b.ItemsSelected.Invoke(b, new SelectedItemsEventArgs() { Items = b.SelectedItems });
            }
        }

        private static void OnIsLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EditorVisual b = (EditorVisual)d;
            bool newItem = (bool)e.NewValue;
            bool oldItem = (bool)e.OldValue;
            if (newItem)
            {
                Mouse.OverrideCursor = Cursors.Wait;
            }
            else
            {
                Mouse.OverrideCursor = null;
            }
        }

        private static void OnSelectedImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EditorVisual b = (EditorVisual)d;
            ImageVisual newItem = e.NewValue as ImageVisual;
            ImageVisual oldItem = e.OldValue as ImageVisual;

            if (newItem != null)
            {
                newItem.IsSelected = true;
            }

            if (oldItem != null)
            {
                oldItem.IsSelected = false;
            }
        }

        private static void OnIsLinkingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        private static void OnZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EditorVisual b = (EditorVisual)d;
            b.model.Zoom = b.ZoomLevel;
        }

        private static void OnSelectedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EditorVisual b = (EditorVisual)d;
            TextVisual newItem = e.NewValue as TextVisual;
            TextVisual oldItem = e.OldValue as TextVisual;

            if (newItem != null)
            {
                newItem.IsSelected = true;
            }

            if (oldItem != null)
            {
                oldItem.IsSelected = false;
            }

            b.SelectedTextChanged.Invoke(b, null);
        }

        private static void OnSelectedConnectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EditorVisual b = (EditorVisual)d;
            if (b.SelectedConnectorChanged != null)
            {
                b.SelectedConnectorChanged.Invoke(b, null);
            }
        }

        public void update()
        {

        }

        public void updateStatus()
        {

        }

        private void AddTextHandler(object sender, AddTextEventArgs e)
        {
            if (State == BinEditorState.READY)
            {
                AddText();
            }
        }

        private void AddImageHandler(object sender, ImageSelectedEventArgs e)
        {
            if (State == BinEditorState.READY)
            {
                AddImage(e.uri);
            }
        }

        private void FitToScreenHandler(object sender, FitToScreenEventArgs e)
        {
            FitToScreen();
        }

        private void OverviewHandler(object sender, EventArgs e)
        {
            IsFullscreenOpen = !IsFullscreenOpen;
        }

        private void SortHandler(object sender, EventArgs e)
        {
            if (State == BinEditorState.READY)
            {
                packer = new ArevaloRectanglePacker(Settings.Default.WorkspaceManager_SortWidth, Settings.Default.WorkspaceManager_SortHeight);
                foreach (ComponentVisual element in ComponentCollection)
                {
                    if (packer.TryPack(element.ActualWidth + Settings.Default.WorkspaceManager_SortPadding, element.ActualHeight + Settings.Default.WorkspaceManager_SortPadding, out Point point))
                    {
                        point.X += Settings.Default.WorkspaceManager_SortPadding;
                        point.Y += Settings.Default.WorkspaceManager_SortPadding;
                        element.Position = point;
                    }
                }
            }
        }

        private void CollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null)
                    {
                        return;
                    }
                    if (e.NewItems[0] is ComponentVisual && ComponentCollection != null)
                    {
                        ComponentCollection.Add(e.NewItems[0] as ComponentVisual);
                    }
                    if (e.NewItems[0] is CryptoLineView && PathCollection != null)
                    {
                        PathCollection.Add(e.NewItems[0] as CryptoLineView);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null)
                    {
                        return;
                    }
                    if (e.OldItems[0] is ComponentVisual && ComponentCollection != null)
                    {
                        ComponentCollection.Remove(e.OldItems[0] as ComponentVisual);
                    }

                    if (e.OldItems[0] is TextVisual)
                    {
                        SelectedText = null;
                    }

                    if (e.OldItems[0] is CryptoLineView && PathCollection != null)
                    {
                        PathCollection.Remove(e.OldItems[0] as CryptoLineView);
                    }

                    if (SelectedItems != null && SelectedItems.Length > 0)
                    {
                        List<UIElement> x = SelectedItems.ToList();
                        foreach (object uiElement in e.OldItems)
                        {
                            x.Remove(uiElement as UIElement);
                        }
                        SelectedItems = x.ToArray();
                    }
                    break;
            }
        }

        private void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            reset();
        }

        private void MouseUpButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            reset();
        }

        private void MouseWheelHandler(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                ZoomLevel = (e.Delta > 0)
                    ? Math.Min(ZoomLevel + 0.05, Settings.Default.WorkspaceManager_MaxScale)
                    : Math.Max(ZoomLevel - 0.05, Settings.Default.WorkspaceManager_MinScale);

                Point pos = e.GetPosition(sender as FrameworkElement);
                Point scrollpos = e.GetPosition(ScrollViewer);
                ScrollViewer.ScrollToHorizontalOffset(pos.X * ZoomLevel - scrollpos.X);
                ScrollViewer.ScrollToVerticalOffset(pos.Y * ZoomLevel - scrollpos.Y);

                e.Handled = true;
            }
        }

        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            Point pp = e.GetPosition(sender as FrameworkElement);
            Point ps = e.GetPosition(ScrollViewer);

            if (IsLinking)
            {
                draggedLink.Line.EndPoint = e.GetPosition(sender as FrameworkElement);
                e.Handled = true;
                return;
            }

            if (startDragPoint != null && e.RightButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(sender as FrameworkElement);
                Vector delta = Point.Subtract((Point)startDragPoint, currentPoint);
                ScrollViewer.ScrollToHorizontalOffset(ScrollViewer.HorizontalOffset + delta.X);
                ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset + delta.Y);
                return;
            }
        }

        private void ContextMenuClick(object sender, RoutedEventArgs e)
        {
            if (VisualsHelper.LastLine != null)
            {
                VisualsHelper.LastLine.Model.WorkspaceModel.ModifyModel(new DeleteConnectionModelOperation(VisualsHelper.LastLine.Model));
            }
        }

        private void RearrangeLineContextMenuClick(object sender, RoutedEventArgs e)
        {
            VisualsHelper.LastLine?.Line.Rearrange();
        }

        private void MouseRightButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (!(e.Source is ComponentVisual) && !(e.Source is ImageVisual) &&
                !(e.Source is TextVisual))
            {
                startDragPoint = Mouse.GetPosition(sender as FrameworkElement);
                Mouse.OverrideCursor = Cursors.ScrollAll;
                e.Handled = true;
            }

            if (e.Source is ComponentVisual && e.OriginalSource is FrameworkElement)
            {
                ComponentVisual c = (ComponentVisual)e.Source;
                FrameworkElement f = (FrameworkElement)e.OriginalSource,
                    element = f.TryFindParent<ConnectorVisual>();
                if (element is ConnectorVisual)
                {
                    ConnectorVisual con = (ConnectorVisual)element;
                    DataObject data = new DataObject("BinConnector", element);
                    DragDrop.AddQueryContinueDragHandler(this, QueryContinueDragHandler);
                    con.IsDragged = true;
                    DragDrop.DoDragDrop(c, data, DragDropEffects.Move);
                    con.IsDragged = false;
                    e.Handled = true;
                }
            }
        }

        private void returnWindowFocus()
        {
            Window win = Util.TryFindParent<Window>(this);
            if (win != null)
            {
                Keyboard.Focus(win);
            }
        }

        private void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (!(e.Source is ComponentVisual) && !(e.Source is ImageVisual) && !(e.Source is TextVisual)
                && !(e.Source is CryptoLineView) && !(e.Source is Line))
            {
                window = Window.GetWindow(this);
                setDragWindowHandle();
                startDragPoint = Mouse.GetPosition(sender as FrameworkElement);
                Mouse.OverrideCursor = Cursors.Arrow;
                Focus();
                Keyboard.Focus(this);
                e.Handled = true;
            }


            switch (e.ClickCount)
            {
                case 1:
                    IControlVisual result = Util.TryFindParent<IControlVisual>(e.OriginalSource as UIElement);
                    if (result != null)
                    {
                        return;
                    }

                    if (e.Source is ImageVisual || e.Source is TextVisual)
                    {
                        if (e.Source is ImageVisual)
                        {
                            ImageVisual c = (ImageVisual)e.Source;
                            if (SelectedImage != c)
                            {
                                SelectedImage = c;
                            }
                        }
                        else
                        {
                            SelectedImage = null;
                        }

                        if (e.Source is TextVisual)
                        {
                            TextVisual c = (TextVisual)e.Source;
                            if (SelectedText != c)
                            {
                                SelectedText = c;
                            }
                        }
                        else
                        {
                            SelectedText = null;
                        }
                        SelectedItems = null;
                        return;
                    }
                    else
                    {
                        SelectedText = null;
                        SelectedImage = null;
                    }

                    if (e.Source is ComponentVisual && e.OriginalSource is FrameworkElement)
                    {
                        ComponentVisual c = (ComponentVisual)e.Source;
                        FrameworkElement f = (FrameworkElement)e.OriginalSource, element = f.TryFindParent<ConnectorVisual>();
                        if ((element is ConnectorVisual && !IsLinking && State == BinEditorState.READY))
                        {
                            ConnectorVisual b = element as ConnectorVisual;
                            SelectedConnector = b;
                            draggedLink.Line.SetBinding(InternalCryptoLineView.StartPointProperty, Util.CreateConnectorBinding(b, draggedLink));
                            draggedLink.Line.EndPoint = e.GetPosition(sender as FrameworkElement);
                            VisualCollection.Add(draggedLink);
                            Mouse.OverrideCursor = Cursors.Cross;
                            e.Handled = IsLinking = true;
                        }
                        PluginChangedEventArgs componentArgs = new PluginChangedEventArgs(c.Model.Plugin, c.FunctionName, DisplayPluginMode.Normal);
                        MyEditor.onSelectedPluginChanged(componentArgs);

                        if (SelectedItems == null)
                        {
                            SelectedItems = new UIElement[] { };
                        }

                        if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None)
                        {
                            // Toggle selected item
                            List<UIElement> items = SelectedItems.ToList();
                            if (SelectedItems.Contains(c))
                            {
                                items.Remove(c);
                            }
                            else
                            {
                                items.Add(c);
                            }

                            SelectedItems = items.ToArray();
                        }
                        else if (!SelectedItems.Contains(c))
                        {
                            SelectedItems = new UIElement[] { c };
                        }

                        startedSelection = true;
                        return;
                    }

                    if (e.Source is CryptoLineView)
                    {
                        CryptoLineView line = e.Source as CryptoLineView;
                        if (SelectedItems == null || !SelectedItems.Contains(line))
                        {
                            SelectedItems = new UIElement[] { line };
                        }

                        startedSelection = true;
                        return;
                    }
                    break;

                case 2:
                    if (e.Source is ComponentVisual)
                    {
                        ComponentVisual c = (ComponentVisual)e.Source;
                        if (c.IsICPopUpOpen || Util.TryFindParent<TextBox>(e.OriginalSource as UIElement) != null ||
                            Util.TryFindParent<Thumb>(e.OriginalSource as UIElement) == null)
                        {
                            startedSelection = true;
                            break;
                        }

                        SetFullscreen(c, c.State != BinComponentState.Min ? c.State : c.FullScreenState);
                        e.Handled = true;
                        startedSelection = true;
                    }
                    break;
            }
        }

        private void WindowMouseLeave(object sender, MouseEventArgs e)
        {
            removeDragWindowHandle();
            dragReset();
        }

        private void WindowPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            removeDragWindowHandle();
            dragReset();
        }

        private void LoadingErrorOccurred(object sender, LoadingErrorEventArgs e)
        {
            HasLoadingError = true;
            LoadingErrorText = e.Message;
        }

        private void PanelLoaded(object sender, RoutedEventArgs e)
        {
            panel = (ModifiedCanvas)sender;
            panel.PreviewMouseMove += new MouseEventHandler(VisualsHelper.panelPreviewMouseMove);
            panel.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(VisualsHelper.panelPreviewMouseLeftButtonUp);
            panel.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(VisualsHelper.panelPreviewMouseLeftButtonDown);
            panel.MouseLeave += new MouseEventHandler(VisualsHelper.panelMouseLeave);
            VisualsHelper.part.Style = (Style)FindResource("FromToLine");
        }

        private void WindowPreviewMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (!_usagePopup.IsOpen)
                {
                    if (startDragPoint != null && e.LeftButton == MouseButtonState.Pressed)
                    {
                        startedSelection = true;
                        Point currentPoint = Util.MouseUtilities.CorrectGetPosition(panel);
                        Vector delta = Point.Subtract((Point)startDragPoint, currentPoint);
                        delta.Negate();
                        selectRectGeometry.Rect = new Rect((Point)startDragPoint, delta);
                        selectionPath.Data = selectRectGeometry;

                        List<UIElement> items = new List<UIElement>();

                        foreach (ComponentVisual element in ComponentCollection)
                        {
                            Rect elementRect = new Rect(element.Position, new Size(element.ActualWidth, element.ActualHeight));
                            if (selectRectGeometry.Rect.IntersectsWith(elementRect))
                            {
                                items.Add(element);
                            }
                            else
                            {
                                items.Remove(element);
                            }
                        }

                        foreach (CryptoLineView line in PathCollection)
                        {
                            foreach (FromTo ft in line.Line.PointList)
                            {
                                Rect elementRect = new Rect(ft.From, ft.To);
                                if (selectRectGeometry.Rect.IntersectsWith(elementRect))
                                {
                                    items.Add(line);
                                    break;
                                }
                                else
                                {
                                    items.Remove(line);
                                }
                            }
                        }

                        // if Control is pressed, add new items to selection, otherwise replace selection with new items
                        if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None)
                        {
                            foreach (UIElement x in SelectedItems)
                            {
                                if (!items.Contains(x))
                                {
                                    items.Add(x);
                                }
                            }
                        }
                        SelectedItems = items.ToArray();
                        return;
                    }
                }
            }
            catch (Exception)
            {
                //do nothing, in case there is nothing selected
            }
        }

        private void MouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is ComponentVisual && e.OriginalSource is FrameworkElement)
            {
                FrameworkElement f = (FrameworkElement)e.OriginalSource, element = f.TryFindParent<ConnectorVisual>();
                if (element is ConnectorVisual)
                {
                    ConnectorVisual b = (ConnectorVisual)element;
                    if (IsLinking && SelectedConnector != null)
                    {
                        if (SelectedConnector.Model != null || b.Model != null)
                        {
                            if (SelectedConnector.Model.ConnectorType != null || b.Model.ConnectorType != null)
                            {
                                ConnectorVisual input, output;
                                input = SelectedConnector.Model.Outgoing == true ? b : SelectedConnector;
                                output = SelectedConnector.Model.Outgoing == false ? b : SelectedConnector;
                                ConversionLevel lvl = WorkspaceModel.compatibleConnectors(output.Model, input.Model);
                                if (lvl != ConversionLevel.Red && lvl != ConversionLevel.NA)
                                {
                                    ConnectionModel connectionModel = (ConnectionModel)Model.ModifyModel(new NewConnectionModelOperation(
                                        output.Model,
                                        input.Model,
                                        output.Model.ConnectorType));
                                    AddConnectionVisual(output, input, connectionModel);
                                    e.Handled = true;
                                }
                                reset();
                                startedSelection = false;
                                return;
                            }
                        }
                    }
                }
            }

            _usagePopup.Open();

            reset();
            startedSelection = false;
        }

        #region DragDropHandler

        private void QueryContinueDragHandler(object source, QueryContinueDragEventArgs e)
        {
            e.Handled = true;

            if (e.EscapePressed)
            {
                e.Action = DragAction.Cancel;
                return;
            }

            e.Action = DragAction.Drop;
            if ((e.KeyStates & DragDropKeyStates.LeftMouseButton) != DragDropKeyStates.None)
            {
                e.Action = DragAction.Continue;
            }
            else if ((e.KeyStates & DragDropKeyStates.RightMouseButton) != DragDropKeyStates.None)
            {
                e.Action = DragAction.Continue;
            }
        }

        public enum Direction { Top, Bottom, Left, Right };
        public enum Orientation { Horizontal, Vertical };

        private List<Operation> Selection_Move(List<ComponentVisual> components, Direction direction)
        {
            List<Operation> list = new List<Operation>();

            if (components.Count >= 1)
            {
                switch (direction)
                {
                    case Direction.Top:
                        double ymin = components.Select(p => p.Position.Y).Min();
                        if (ymin > 0)
                        {
                            foreach (ComponentVisual element in components)
                            {
                                list.Add(new MoveModelElementOperation(element.Model, new Point(element.Position.X, element.Position.Y - 1)));
                            }
                        }

                        break;
                    case Direction.Bottom:
                        foreach (ComponentVisual element in components)
                        {
                            list.Add(new MoveModelElementOperation(element.Model, new Point(element.Position.X, element.Position.Y + 1)));
                        }

                        break;
                    case Direction.Left:
                        double xmin = components.Select(p => p.Position.X).Min();
                        if (xmin > 0)
                        {
                            foreach (ComponentVisual element in components)
                            {
                                list.Add(new MoveModelElementOperation(element.Model, new Point(element.Position.X - 1, element.Position.Y)));
                            }
                        }

                        break;
                    case Direction.Right:
                        foreach (ComponentVisual element in components)
                        {
                            list.Add(new MoveModelElementOperation(element.Model, new Point(element.Position.X + 1, element.Position.Y)));
                        }

                        break;
                }
            }

            return list;
        }

        private List<Operation> Selection_Align(List<ComponentVisual> components, Direction align)
        {
            List<Operation> list = new List<Operation>();

            if (components.Count >= 2)
            {
                switch (align)
                {
                    case Direction.Top:
                        double ymin = components.Select(p => p.Position.Y).Min();
                        foreach (ComponentVisual element in components)
                        {
                            list.Add(new MoveModelElementOperation(element.Model, new Point(element.Position.X, ymin)));
                        }

                        break;
                    case Direction.Bottom:
                        double ymax = components.Select(p => p.Position.Y + p.ActualHeight).Max();
                        foreach (ComponentVisual element in components)
                        {
                            list.Add(new MoveModelElementOperation(element.Model, new Point(element.Position.X, ymax - element.ActualHeight)));
                        }

                        break;
                    case Direction.Left:
                        double xmin = components.Select(p => p.Position.X).Min();
                        foreach (ComponentVisual element in components)
                        {
                            list.Add(new MoveModelElementOperation(element.Model, new Point(xmin, element.Position.Y)));
                        }

                        break;
                    case Direction.Right:
                        double xmax = components.Select(p => p.Position.X + p.ActualWidth).Max();
                        foreach (ComponentVisual element in components)
                        {
                            list.Add(new MoveModelElementOperation(element.Model, new Point(xmax - element.ActualWidth, element.Position.Y)));
                        }

                        break;
                }
            }

            return list;
        }

        private List<Operation> Selection_Spread(List<ComponentVisual> components, Orientation orientation)
        {
            List<Operation> list = new List<Operation>();

            if (components.Count >= 3)
            {
                switch (orientation)
                {
                    case Orientation.Horizontal:
                        if (components.Count > 1)
                        {
                            double width = components.Select(p => p.ActualWidth).Sum();
                            double xmin = components.Select(p => p.Position.X).Min();
                            double xmax = components.Select(p => p.Position.X + p.ActualWidth).Max();
                            double delta = (xmax - xmin - width) / (components.Count - 1);
                            components.Sort((p1, p2) => p1.Position.X.CompareTo(p2.Position.X));
                            double x = xmin;
                            foreach (ComponentVisual element in components)
                            {
                                list.Add(new MoveModelElementOperation(element.Model, new Point(x, element.Position.Y)));
                                x += element.ActualWidth + delta;
                            }
                        }
                        break;
                    case Orientation.Vertical:
                        if (components.Count > 1)
                        {
                            double height = components.Select(p => p.ActualHeight).Sum();
                            double ymin = components.Select(p => p.Position.Y).Min();
                            double ymax = components.Select(p => p.Position.Y + p.ActualHeight).Max();
                            double delta = (ymax - ymin - height) / (components.Count - 1);
                            components.Sort((p1, p2) => p1.Position.Y.CompareTo(p2.Position.Y));
                            double y = ymin;
                            foreach (ComponentVisual element in components)
                            {
                                list.Add(new MoveModelElementOperation(element.Model, new Point(element.Position.X, y)));
                                y += element.ActualHeight + delta;
                            }
                        }
                        break;
                }
            }

            return list;
        }

        private List<Operation> Selection_UniformSize(List<ComponentVisual> components, Orientation orientation, bool maximize)
        {
            List<Operation> list = new List<Operation>();

            if (components.Count >= 2)
            {
                switch (orientation)
                {
                    case Orientation.Horizontal:
                        double width = maximize ? components.Select(p => p.WindowWidth).Max() : components.Select(p => p.WindowWidth).Min();
                        foreach (ComponentVisual element in components)
                        {
                            list.Add(new ResizeModelElementOperation(element.Model, width, element.WindowHeight));
                        }

                        break;
                    case Orientation.Vertical:
                        double height = maximize ? components.Select(p => p.WindowHeight).Max() : components.Select(p => p.WindowHeight).Min();
                        foreach (ComponentVisual element in components)
                        {
                            list.Add(new ResizeModelElementOperation(element.Model, element.WindowWidth, height));
                        }

                        break;
                }
            }

            return list;
        }

        private void PreviewKeyDownHandler(object sender, KeyEventArgs e)
        {
            bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None;
            bool shift = (Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.None;
            bool alt = (Keyboard.Modifiers & ModifierKeys.Alt) != ModifierKeys.None;

            if (ctrl && !shift && !alt)
            {
                if (e.Key == Key.I)
                {
                    //with Ctrl + I, the user can toggle the settings bar
                    IsSettingsOpen = !IsSettingsOpen;
                }

                // select all components
                if (e.Key == Key.A)
                {
                    if (e.OriginalSource is EditorVisual)
                    {
                        SelectedItems = VisualsHelper.Visuals.OfType<ComponentVisual>().ToArray();
                        e.Handled = true;
                        return;
                    }
                }
                // fit to screen
                if (e.Key == Key.F)
                {
                    if (e.OriginalSource is EditorVisual)
                    {
                        FitToScreen();
                        e.Handled = true;
                        return;
                    }
                }
            }

            if (MyEditor.isExecuting())
            {
                return;
            }

            if (SelectedItems == null)
            {
                return;
            }

            List<ComponentVisual> components = SelectedItems.OfType<ComponentVisual>().ToList();

            List<Operation> list = null;

            // move component 1px up, down, left or right
            if (ctrl && shift && !alt)
            {
                if (e.Key == Key.Up)
                {
                    list = Selection_Move(components, Direction.Top);
                }
                else if (e.Key == Key.Down)
                {
                    list = Selection_Move(components, Direction.Bottom);
                }
                else if (e.Key == Key.Left)
                {
                    list = Selection_Move(components, Direction.Left);
                }
                else if (e.Key == Key.Right)
                {
                    list = Selection_Move(components, Direction.Right);
                }
            }

            // align components on outermost upper, lower, left or right edge
            if (ctrl && !shift && !alt)
            {
                if (e.Key == Key.Up)
                {
                    list = Selection_Align(components, Direction.Top);
                }
                else if (e.Key == Key.Down)
                {
                    list = Selection_Align(components, Direction.Bottom);
                }
                else if (e.Key == Key.Left)
                {
                    list = Selection_Align(components, Direction.Left);
                }
                else if (e.Key == Key.Right)
                {
                    list = Selection_Align(components, Direction.Right);
                }
            }

            // uniformly spread selected components horizontal or vertical
            if (ctrl && shift && !alt)
            {
                if (e.Key == Key.X)
                {
                    list = Selection_Spread(components, Orientation.Horizontal);
                }
                else if (e.Key == Key.Y)
                {
                    list = Selection_Spread(components, Orientation.Vertical);
                }
            }

            // unify widths or heights of selected components, set to biggest value
            if (ctrl && shift && !alt)
            {
                if (e.Key == Key.W)
                {
                    list = Selection_UniformSize(components, Orientation.Horizontal, true);
                }
                else if (e.Key == Key.H)
                {
                    list = Selection_UniformSize(components, Orientation.Vertical, true);
                }
            }

            // unify widths or heights of selected components, set to smallest value
            if (ctrl && !shift && !alt)
            {
                if (e.Key == Key.W)
                {
                    list = Selection_UniformSize(components, Orientation.Horizontal, false);
                }
                else if (e.Key == Key.H)
                {
                    list = Selection_UniformSize(components, Orientation.Vertical, false);
                }
            }

            if (list != null && list.Count > 0)
            {
                Model.ModifyModel(new MultiOperation(list));
                e.Handled = true;
            }
        }

        private void PreviewDragEnterHandler(object sender, DragEventArgs e)
        {

        }

        private void PreviewDragLeaveHandler(object sender, DragEventArgs e)
        {

        }

        private void PreviewDropHandler(object sender, DragEventArgs e)
        {
            if (State != BinEditorState.READY)
            {
                return;
            }

            if (e.Data.GetDataPresent("CrypTool.PluginBase.Editor.DragDropDataObject") && !(e.Source is ComponentVisual))
            {
                try
                {
                    DragDropDataObject obj = e.Data.GetData("CrypTool.PluginBase.Editor.DragDropDataObject") as DragDropDataObject;
                    PluginModel pluginModel = (PluginModel)Model.ModifyModel(new NewPluginModelOperation(Util.MouseUtilities.CorrectGetPosition(sender as FrameworkElement), 0, 0, DragDropDataObjectToPluginConverter.CreatePluginInstance(obj.AssemblyFullName, obj.TypeFullName)));
                    AddComponentVisual(pluginModel);
                    e.Handled = true;
                }
                catch (Exception ex)
                {
                    MyEditor.GuiLogMessage(string.Format("Could not add Plugin to Workspace: {0}", ex.Message), NotificationLevel.Error);
                    MyEditor.GuiLogMessage(ex.StackTrace, NotificationLevel.Error);
                }
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filePaths = (string[])(e.Data.GetData(DataFormats.FileDrop));
                if (filePaths != null && filePaths.Count() == 1)
                {
                    // we only open existing files that names end with cwm
                    if (System.IO.File.Exists(filePaths[0]) && filePaths[0].ToLower().EndsWith("cwm"))
                    {
                        MyEditor.Open(filePaths[0]);
                    }
                }
                return;
            }
        }
        #endregion

        #endregion
    }

    #region HelperClass

    public class SelectedItemsEventArgs : EventArgs
    {
        public UIElement[] Items { get; set; }
    }

    internal class SelectionChangedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }

            UIElement[] elements = (UIElement[])value;
            if (elements.Contains(parameter))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class DragDropDataObjectToPluginConverter
    {
        public static PluginManager PluginManager { get; set; }
        private static Type type;
        public static Type CreatePluginInstance(string assemblyQualifiedName, string typeVar)
        {
            if (PluginManager != null && assemblyQualifiedName != null && typeVar != null)
            {
                AssemblyName assName = new AssemblyName(assemblyQualifiedName);
                type = PluginManager.LoadType(assName.Name, typeVar);

                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }
    }
    #endregion
}