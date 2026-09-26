/*
   Copyright (C) CrypTool 2 Team

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
using CrypTool.PluginBase.Attributes;
using KeySearcherPresentation.Controls;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace KeySearcherPresentation
{
    /// <summary>
    /// Interaction logic for QuickWatch.xaml
    /// </summary>
    [TabColor("pink")]
    [CrypTool.PluginBase.Attributes.Localization("KeySearcher.Properties.Resources")]
    public partial class QuickWatch : UserControl
    {
        public static readonly DependencyProperty IsP2PEnabledProperty =
            DependencyProperty.Register("IsP2PEnabled",
                                        typeof(
                                            bool),
                                        typeof(
                                            QuickWatch), new PropertyMetadata(false));

        public bool IsP2PEnabled
        {
            get => (bool)GetValue(IsP2PEnabledProperty);
            set => SetValue(IsP2PEnabledProperty, value);
        }      

        public bool ShowStatistics
        {
            get => (bool)GetValue(ShowStatisticsProperty);
            set => SetValue(ShowStatisticsProperty, value);
        }

        public static DependencyProperty ShowStatisticsProperty =
            DependencyProperty.Register("ShowStatistics",
                                        typeof(
                                            bool),
                                        typeof(
                                            QuickWatch), new PropertyMetadata(false));

        public CultureInfo CurrentCulture { get; private set; }

        public QuickWatch()
        {
            InitializeComponent();

            CurrentCulture = CultureInfo.CurrentCulture;
        }
    }
}
