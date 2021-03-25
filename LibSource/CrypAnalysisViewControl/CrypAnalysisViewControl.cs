using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CrypTool.CrypAnalysisViewControl
{
    public class CrypAnalysisViewControl : ContentControl
    {
        public string ResultHeaderCaption
        {
            get { return (string)GetValue(ResultHeaderCaptionProperty); }
            set { SetValue(ResultHeaderCaptionProperty, value); }
        }

        public static readonly DependencyProperty ResultHeaderCaptionProperty = DependencyProperty.Register(
          "ResultHeaderCaption", typeof(string), typeof(CrypAnalysisViewControl), new PropertyMetadata(null));

        public string ResultListCaption
        {
            get { return (string)GetValue(ResultListCaptionProperty); }
            set { SetValue(ResultListCaptionProperty, value); }
        }

        public static readonly DependencyProperty ResultListCaptionProperty = DependencyProperty.Register(
          "ResultListCaption", typeof(string), typeof(CrypAnalysisViewControl), new PropertyMetadata(null));

        public string ResultProgressCaption
        {
            get { return (string)GetValue(ResultProgressCaptionProperty); }
            set { SetValue(ResultProgressCaptionProperty, value); }
        }

        public static readonly DependencyProperty ResultProgressCaptionProperty = DependencyProperty.Register(
          "ResultProgressCaption", typeof(string), typeof(CrypAnalysisViewControl), new PropertyMetadata(null));

        public List<ViewLabel> ResultHeaderLabels { get; } = new List<ViewLabel>();

        public List<ViewLabel> ResultProgressLabels { get; } = new List<ViewLabel>();

        public List<SectionControl> AdditionalSections { get; } = new List<SectionControl>();

        public CrypAnalysisViewControl()
        {
            string packUri = @"CrypAnalysisViewControl;component/Themes/Generic.xaml";
            var resourceDictionary = Application.LoadComponent(new Uri(packUri, UriKind.Relative)) as ResourceDictionary;
            Resources.MergedDictionaries.Add(resourceDictionary);

            Loaded += CrypAnalysisViewControl_Loaded;
        }

        private void CrypAnalysisViewControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Add non-component children as logical children to enable binding:
            foreach (var element in ResultHeaderLabels.Union<object>(ResultProgressLabels).Union(AdditionalSections))
            {
                AddLogicalChild(element);
            }
        }
    }
}
