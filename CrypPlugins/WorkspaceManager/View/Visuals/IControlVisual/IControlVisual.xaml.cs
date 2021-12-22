using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CrypTool.PluginBase;

namespace WorkspaceManager.View.Visuals
{
    /// <summary>
    /// Interaction logic for BinIControlVisual.xaml
    /// </summary>
    public partial class IControlVisual : Popup
    {
        #region Properties

        //public WorkspaceManager MyEditor { get { return (WorkspaceManager)this.Model.WorkspaceModel.MyEditor; } }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty ConnectorsProperty = DependencyProperty.Register("Connectors",
            typeof(ObservableCollection<IControlMasterElement>), typeof(IControlVisual), new FrameworkPropertyMetadata(null, OnListChanged));

        public ObservableCollection<IControlMasterElement> Connectors
        {
            get => (ObservableCollection<IControlMasterElement>)base.GetValue(ConnectorsProperty);
            set => base.SetValue(ConnectorsProperty, value);
        }

        public static readonly DependencyProperty SlotsProperty = DependencyProperty.Register("Slots",
            typeof(ObservableCollection<Slot>), typeof(IControlVisual), new FrameworkPropertyMetadata(null));

        public ObservableCollection<Slot> Slots
        {
            get => (ObservableCollection<Slot>)base.GetValue(SlotsProperty);
            set => base.SetValue(SlotsProperty, value);
        }
        #endregion

        #region Constructors
        public IControlVisual()
        {
            InitializeComponent();
            Slots = new ObservableCollection<Slot>();
        }
        #endregion

        #region public

        #endregion

        #region protected

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            StaysOpen = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnOpened(e);
            StaysOpen = true;
        }

        #endregion

        #region Event Handler

        private static void OnListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IControlVisual pop = (IControlVisual)d;
            if (e.NewValue == null)
            {
                return;
            }

            ObservableCollection<IControlMasterElement> newCollection = e.NewValue as ObservableCollection<IControlMasterElement>;
            ObservableCollection<IControlMasterElement> oldCollection = e.OldValue as ObservableCollection<IControlMasterElement>;

            if (oldCollection != null)
            {
                oldCollection.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(pop.CollectionChangedHandler);
            }

            if (newCollection != null)
            {
                newCollection.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(pop.CollectionChangedHandler);
            }
        }

        private void CollectionChangedHandler(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (object element in e.NewItems)
            {
                Slot s = new Slot((IControlMasterElement)element);
                Slots.Add(s);
            }
        }
        #endregion
    }

    #region Custom Classes
    public class IPluginWrapper
    {
        public IPlugin Plugin { get; private set; }
        public Image Image => Plugin.GetImage(0);
        public string ToolTip => Plugin.GetPluginInfoAttribute().Caption;

        public IPluginWrapper(IPlugin plugin)
        {
            Plugin = plugin;
        }
    }
    #endregion
    #region Converter

    #endregion
}
