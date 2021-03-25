using System.Windows;
using System.Windows.Controls;

namespace CrypTool.MD5.Presentation.Displays
{
    /// <summary>
    /// Interaktionslogik für FunctionNameDisplay.xaml
    /// </summary>
    public partial class FunctionNameDisplay : UserControl
    {
        public static readonly DependencyProperty FunctionNameProperty = DependencyProperty.Register("FunctionName", typeof(string), typeof(FunctionNameDisplay));
        public string FunctionName { get { return (string)GetValue(FunctionNameProperty); } set { SetValue(FunctionNameProperty, value); } }

        public FunctionNameDisplay()
        {
            InitializeComponent();
        }
    }
}
