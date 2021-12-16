using System.Windows;

namespace CrypTool.Plugins.ChaCha.Helper.Validation
{
    public class RangeChecker : DependencyObject
    {
        public int Minimum
        {
            get => (int)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(int), typeof(RangeChecker), new UIPropertyMetadata(int.MinValue));

        public int Maximum
        {
            get => (int)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(int), typeof(RangeChecker), new UIPropertyMetadata(int.MaxValue));
    }
}