using System;
using System.Windows;

namespace CrypTool.CrypAnalysisViewControl
{
    public class ViewLabel : FrameworkContentElement
    {
        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register(
          "Caption", typeof(string), typeof(ViewLabel), new PropertyMetadata(""));

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
          "Value", typeof(string), typeof(ViewLabel), new PropertyMetadata(""));
    }
}
