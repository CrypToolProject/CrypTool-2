using System.Windows;
using System.Windows.Controls;

namespace CrypTool.MD5.Presentation.Displays
{
    /// <summary>
    /// Interaction logic for LabelledIntegerDisplay.xaml
    /// </summary>
    public partial class LabelledIntegerDisplay : UserControl
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(uint), typeof(LabelledIntegerDisplay), null);
        public uint Value { get => (uint)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }


        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption", typeof(string), typeof(LabelledIntegerDisplay), null);
        public string Caption { get => (string)GetValue(CaptionProperty); set => SetValue(CaptionProperty, value); }

        public LabelledIntegerDisplay()
        {
            InitializeComponent();

            Width = double.NaN;
            Height = double.NaN;
        }
    }
}