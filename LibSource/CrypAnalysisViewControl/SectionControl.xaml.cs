using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CrypTool.CrypAnalysisViewControl
{
    public class SectionControl : ContentControl
    {
        public string SectionHeaderCaption
        {
            get { return (string)GetValue(SectionHeaderCaptionProperty); }
            set { SetValue(SectionHeaderCaptionProperty, value); }
        }

        public static readonly DependencyProperty SectionHeaderCaptionProperty = DependencyProperty.Register(
          "SectionHeaderCaption", typeof(string), typeof(SectionControl), new PropertyMetadata(null));

        public Brush SectionHeaderBackground
        {
            get { return (Brush)GetValue(SectionHeaderBackgroundProperty); }
            set { SetValue(SectionHeaderBackgroundProperty, value); }
        }

        public static readonly DependencyProperty SectionHeaderBackgroundProperty = DependencyProperty.Register(
          "SectionHeaderBackground", typeof(Brush), typeof(SectionControl), new PropertyMetadata(Brushes.Transparent));

        public Brush SectionBackground
        {
            get { return (Brush)GetValue(SectionBackgroundProperty); }
            set { SetValue(SectionBackgroundProperty, value); }
        }

        public static readonly DependencyProperty SectionBackgroundProperty = DependencyProperty.Register(
          "SectionBackground", typeof(Brush), typeof(SectionControl), new PropertyMetadata(Brushes.Transparent));

        public bool IsSectionVisible
        {
            get { return (bool)GetValue(IsSectionVisibleProperty); }
            set { SetValue(IsSectionVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsSectionVisibleProperty = DependencyProperty.Register(
          "IsSectionVisible", typeof(bool), typeof(SectionControl), new PropertyMetadata(true));
    }
}
