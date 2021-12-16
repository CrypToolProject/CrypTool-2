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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CrypTool.CrypAnalysisViewControl
{
    public class SectionControl : ContentControl
    {
        public string SectionHeaderCaption
        {
            get => (string)GetValue(SectionHeaderCaptionProperty);
            set => SetValue(SectionHeaderCaptionProperty, value);
        }

        public static readonly DependencyProperty SectionHeaderCaptionProperty = DependencyProperty.Register(
          "SectionHeaderCaption", typeof(string), typeof(SectionControl), new PropertyMetadata(null));

        public Brush SectionHeaderBackground
        {
            get => (Brush)GetValue(SectionHeaderBackgroundProperty);
            set => SetValue(SectionHeaderBackgroundProperty, value);
        }

        public static readonly DependencyProperty SectionHeaderBackgroundProperty = DependencyProperty.Register(
          "SectionHeaderBackground", typeof(Brush), typeof(SectionControl), new PropertyMetadata(Brushes.Transparent));

        public Brush SectionBackground
        {
            get => (Brush)GetValue(SectionBackgroundProperty);
            set => SetValue(SectionBackgroundProperty, value);
        }

        public static readonly DependencyProperty SectionBackgroundProperty = DependencyProperty.Register(
          "SectionBackground", typeof(Brush), typeof(SectionControl), new PropertyMetadata(Brushes.Transparent));

        public bool IsSectionVisible
        {
            get => (bool)GetValue(IsSectionVisibleProperty);
            set => SetValue(IsSectionVisibleProperty, value);
        }

        public static readonly DependencyProperty IsSectionVisibleProperty = DependencyProperty.Register(
          "IsSectionVisible", typeof(bool), typeof(SectionControl), new PropertyMetadata(true));

    }
}
