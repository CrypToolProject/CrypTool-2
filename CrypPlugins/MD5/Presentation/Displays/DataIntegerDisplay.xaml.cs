using System;
using System.Windows;
using System.Windows.Controls;

namespace CrypTool.MD5.Presentation.Displays
{
	/// <summary>
	/// Interaction logic for DataIntegerDisplay.xaml
	/// </summary>
	public partial class DataIntegerDisplay : UserControl
	{
        public int HighlightedValue
        {
            get { return (int)GetValue(HighlightedValueProperty); }
            set { SetValue(HighlightedValueProperty, value); }
        }

        public UInt32[] DisplayedValues
        {
            get { return (UInt32[])GetValue(DisplayedValuesProperty); }
            set { SetValue(DisplayedValuesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayedValues.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayedValuesProperty =
            DependencyProperty.Register("DisplayedValues", typeof(UInt32[]), typeof(DataIntegerDisplay), new UIPropertyMetadata(new UInt32[16]));

	    // Using a DependencyProperty as the backing store for HighlightedValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighlightedValueProperty =
            DependencyProperty.Register("HighlightedValue", typeof(int), typeof(DataIntegerDisplay), new UIPropertyMetadata(-1));

        
		public DataIntegerDisplay()
		{
			this.InitializeComponent();
			
			Width = double.NaN;
			Height = double.NaN;
		}
	}
}