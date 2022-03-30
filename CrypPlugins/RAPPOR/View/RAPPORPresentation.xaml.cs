using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CrypTool.Plugins.RAPPOR.ViewModel;

namespace CrypTool.Plugins.RAPPOR.View
{
    /// <summary>
    /// Interaction logic for RAPPORPresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.Plugins.RAPPOR.Properties.Resources")]
    public partial class RAPPORPresentation : UserControl
    {
        /// <summary>
        /// The used instance of the rappor presentation view model.
        /// </summary>
        public RAPPORPresentationViewModel rAPPORPresentationViewModel;
        private readonly LinearGradientBrush lG;
        private int selectedView;
        /// <summary>
        /// Initializes the RAPPOR presentation view and obtains an instance of the RAPPOR class.
        /// With this instance the rappor presentation view model is initalized.
        /// </summary>
        /// <param name="RAPPORVisualization"></param>
        public RAPPORPresentation(RAPPOR RAPPORVisualization)
        {
            InitializeComponent();
            rAPPORPresentationViewModel = new RAPPORPresentationViewModel(RAPPORVisualization);
            lG = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            lG.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#d3e7d8"), 0.0));
            lG.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#bcebbc"), 0.5));
            lG.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#d3e7d8"), 1.0));
            DataContext = rAPPORPresentationViewModel;
            DataContext = rAPPORPresentationViewModel.GetViewArray()[0];
            selectedView = 0;
            ChangeButtonBackground(0);
        }
        /// <summary>
        /// Switches the content control to the start view. Also chages the color to indicate that 
        /// this view is being used.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text composition event arg.</param>
        private void StartButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DataContext = rAPPORPresentationViewModel.GetViewArray()[0];
            rAPPORPresentationViewModel.GetViewArray()[0].DrawCanvas();
            rAPPORPresentationViewModel.SetSelectedViewInteger(0);
            selectedView = 0;
            ChangeButtonBackground(0);
        }
        /// <summary>
        /// Switches the content control to the overview view. Also chages the color to indicate that 
        /// this view is being used.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text composition event arg.</param>
        private void OvewviewButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (selectedView != 1)
            {
                DataContext = rAPPORPresentationViewModel.GetViewArray()[1];
                rAPPORPresentationViewModel.GetViewArray()[1].DrawCanvas();
                rAPPORPresentationViewModel.SetSelectedViewInteger(1);
                selectedView = 1;
                ChangeButtonBackground(1);
            }

        }
        /// <summary>
        /// Switches the content control to the bloom filter view. Also chages the color to indicate that 
        /// this view is being used.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text composition event arg.</param>
        private void BloomFilterButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DataContext = rAPPORPresentationViewModel.GetViewArray()[2];
            rAPPORPresentationViewModel.GetViewArray()[2].DrawCanvas();
            rAPPORPresentationViewModel.SetSelectedViewInteger(2);
            selectedView = 2;
            ChangeButtonBackground(2);
        }
        /// <summary>
        /// Switches the content control to the randomized response view. Also chages the color to indicate that 
        /// this view is being used.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text composition event arg.</param>
        private void RandomizedResponseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DataContext = rAPPORPresentationViewModel.GetViewArray()[3];
            rAPPORPresentationViewModel.GetViewArray()[3].DrawCanvas();
            rAPPORPresentationViewModel.SetSelectedViewInteger(3);
            selectedView = 3;
            ChangeButtonBackground(3);
        }
        /// <summary>
        /// Switches the content control to the heatmap view. Also chages the color to indicate that 
        /// this view is being used.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text composition event arg.</param>
        private void HeatMapsButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DataContext = rAPPORPresentationViewModel.GetViewArray()[4];
            rAPPORPresentationViewModel.GetViewArray()[4].DrawCanvas();
            rAPPORPresentationViewModel.SetSelectedViewInteger(4);
            selectedView = 4;
            ChangeButtonBackground(4);
        }
        public RAPPORPresentationViewModel GetRapporPresentationViewModel()
        {
            return rAPPORPresentationViewModel;
        }

        public void ChangeButtonBackground(int x)
        {
            switch (x)
            {
                //Changed button thickness back to 1
                case 0:
                    Button0.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#8ABC94");
                    Button1.Background = lG;
                    Button2.Background = lG;
                    Button3.Background = lG;
                    Button4.Background = lG;
                    Button0.BorderThickness = new Thickness(1);
                    Button1.BorderThickness = new Thickness(1);
                    Button2.BorderThickness = new Thickness(1);
                    Button3.BorderThickness = new Thickness(1);
                    Button4.BorderThickness = new Thickness(1);
                    Textblock0.FontWeight = FontWeights.Bold;
                    Textblock1.FontWeight = FontWeights.Normal;
                    Textblock2.FontWeight = FontWeights.Normal;
                    Textblock3.FontWeight = FontWeights.Normal;
                    Textblock4.FontWeight = FontWeights.Normal;
                    break;
                case 1:
                    Button0.Background = lG;
                    Button1.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#8ABC94");
                    Button2.Background = lG;
                    Button3.Background = lG;
                    Button4.Background = lG;
                    Button0.BorderThickness = new Thickness(1);
                    Button1.BorderThickness = new Thickness(1);
                    Button2.BorderThickness = new Thickness(1);
                    Button3.BorderThickness = new Thickness(1);
                    Button4.BorderThickness = new Thickness(1);
                    Textblock0.FontWeight = FontWeights.Normal;
                    Textblock1.FontWeight = FontWeights.Bold;
                    Textblock2.FontWeight = FontWeights.Normal;
                    Textblock3.FontWeight = FontWeights.Normal;
                    Textblock4.FontWeight = FontWeights.Normal;
                    break;
                case 2:

                    Button0.Background = lG;
                    Button1.Background = lG;
                    Button2.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#8ABC94");
                    Button3.Background = lG;
                    Button4.Background = lG;
                    Button0.BorderThickness = new Thickness(1);
                    Button1.BorderThickness = new Thickness(1);
                    Button2.BorderThickness = new Thickness(1);
                    Button3.BorderThickness = new Thickness(1);
                    Button4.BorderThickness = new Thickness(1);
                    Textblock0.FontWeight = FontWeights.Normal;
                    Textblock1.FontWeight = FontWeights.Normal;
                    Textblock2.FontWeight = FontWeights.Bold;
                    Textblock3.FontWeight = FontWeights.Normal;
                    Textblock4.FontWeight = FontWeights.Normal;
                    break;
                case 3:

                    Button0.Background = lG;
                    Button1.Background = lG;
                    Button2.Background = lG;
                    Button3.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#8ABC94");
                    Button4.Background = lG;
                    Button0.BorderThickness = new Thickness(1);
                    Button1.BorderThickness = new Thickness(1);
                    Button2.BorderThickness = new Thickness(1);
                    Button3.BorderThickness = new Thickness(1);
                    Button4.BorderThickness = new Thickness(1);
                    Textblock0.FontWeight = FontWeights.Normal;
                    Textblock1.FontWeight = FontWeights.Normal;
                    Textblock2.FontWeight = FontWeights.Normal;
                    Textblock3.FontWeight = FontWeights.Bold;
                    Textblock4.FontWeight = FontWeights.Normal;
                    break;
                case 4:
                    Button0.Background = lG;
                    Button1.Background = lG;
                    Button2.Background = lG;
                    Button3.Background = lG;
                    Button4.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#8ABC94");//#bcebbc before
                    Button0.BorderThickness = new Thickness(1);
                    Button1.BorderThickness = new Thickness(1);
                    Button2.BorderThickness = new Thickness(1);
                    Button3.BorderThickness = new Thickness(1);
                    Button4.BorderThickness = new Thickness(1);
                    Textblock0.FontWeight = FontWeights.Normal;
                    Textblock1.FontWeight = FontWeights.Normal;
                    Textblock2.FontWeight = FontWeights.Normal;
                    Textblock3.FontWeight = FontWeights.Normal;
                    Textblock4.FontWeight = FontWeights.Bold;
                    break;
                default:
                    break;
            }
        }
    }
}