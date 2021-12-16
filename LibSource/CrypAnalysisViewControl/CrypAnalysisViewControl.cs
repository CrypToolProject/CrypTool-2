/*
   Copyright 2021 CrypTool 2 Team <ct2contact@CrypTool.org>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
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
            get => (string)GetValue(ResultHeaderCaptionProperty);
            set => SetValue(ResultHeaderCaptionProperty, value);
        }

        public static readonly DependencyProperty ResultHeaderCaptionProperty = DependencyProperty.Register(
          "ResultHeaderCaption", typeof(string), typeof(CrypAnalysisViewControl), new PropertyMetadata(null));

        public string ResultListCaption
        {
            get => (string)GetValue(ResultListCaptionProperty);
            set => SetValue(ResultListCaptionProperty, value);
        }

        public static readonly DependencyProperty ResultListCaptionProperty = DependencyProperty.Register(
          "ResultListCaption", typeof(string), typeof(CrypAnalysisViewControl), new PropertyMetadata(null));

        public string ResultProgressCaption
        {
            get => (string)GetValue(ResultProgressCaptionProperty);
            set => SetValue(ResultProgressCaptionProperty, value);
        }

        public static readonly DependencyProperty ResultProgressCaptionProperty = DependencyProperty.Register(
          "ResultProgressCaption", typeof(string), typeof(CrypAnalysisViewControl), new PropertyMetadata(null));

        public List<ViewLabel> ResultHeaderLabels { get; } = new List<ViewLabel>();

        public List<SectionControl> AdditionalHeaders { get; } = new List<SectionControl>();

        public List<ViewLabel> ResultProgressLabels { get; } = new List<ViewLabel>();

        public List<SectionControl> AdditionalSections { get; } = new List<SectionControl>();

        public CrypAnalysisViewControl()
        {
            string packUri = @"CrypAnalysisViewControl;component/Themes/Generic.xaml";
            ResourceDictionary resourceDictionary = Application.LoadComponent(new Uri(packUri, UriKind.Relative)) as ResourceDictionary;
            Resources.MergedDictionaries.Add(resourceDictionary);
            Loaded += CrypAnalysisViewControl_Loaded;
        }

        private void CrypAnalysisViewControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Add non-component children as logical children to enable binding:
            foreach (object element in ResultHeaderLabels.Union<object>(AdditionalHeaders).Union(ResultProgressLabels).Union(AdditionalSections))
            {
                AddLogicalChild(element);
            }
        }
    }
}
