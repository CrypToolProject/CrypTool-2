using System.Windows;
using System.Windows.Controls;
using CrypTool.Plugins.MD5Collider.Algorithm;

namespace CrypTool.Plugins.MD5Collider.Presentation
{
    /// <summary>
    /// Interaktionslogik für QuickWatchPresentationContainer.xaml
    /// </summary>
    public partial class QuickWatchPresentationContainer : UserControl
    {
        public static DependencyProperty ColliderProperty = DependencyProperty.Register("Collider", typeof(IMD5ColliderAlgorithm), typeof(QuickWatchPresentationContainer));
        public IMD5ColliderAlgorithm Collider
        {
            get { return (IMD5ColliderAlgorithm)GetValue(ColliderProperty); }
            set { SetValue(ColliderProperty, value); }
        }

        public QuickWatchPresentationContainer()
        {
            InitializeComponent();
        }
    }
}
