using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CrypTool.MD5.Presentation.Displays
{
    /// <summary>
    /// Interaktionslogik für DataBlockDisplay.xaml
    /// </summary>
    public partial class DataBlockDisplay : UserControl
    {
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(IList<byte>), typeof(DataBlockDisplay), null);
        public IList<byte> Data { get => (IList<byte>)GetValue(DataProperty); set => SetValue(DataProperty, value); }

        public DataBlockDisplay()
        {
            InitializeComponent();
        }
    }
}
