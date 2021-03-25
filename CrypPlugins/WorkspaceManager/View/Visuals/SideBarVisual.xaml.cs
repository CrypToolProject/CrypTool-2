using System.Windows.Controls;

namespace WorkspaceManager.View.Visuals
{
    /// <summary>
    /// Interaction logic for BinSideBarVisual.xaml
    /// </summary>
    public partial class SideBarVisual : UserControl
    {
        public SideBarVisual()
        {
            InitializeComponent();
        }

        // wander 2011-12-13: unused code
        /*public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen",
            typeof(bool), typeof(BinSideBarVisual), new FrameworkPropertyMetadata(false));

        public bool IsOpen
        {
            get
            {
                return (bool)base.GetValue(IsOpenProperty);
            }
            set
            {
                base.SetValue(IsOpenProperty, value);
            }
        }

        private void ActionHandler(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b.Content is bool)
            {
                IsOpen = (bool)b.Content;
                return;
            }
        }*/
    }
}
