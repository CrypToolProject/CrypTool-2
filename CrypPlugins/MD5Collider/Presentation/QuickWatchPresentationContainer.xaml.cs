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
using CrypTool.Plugins.MD5Collider.Algorithm;
using System.Windows;
using System.Windows.Controls;

namespace CrypTool.Plugins.MD5Collider.Presentation
{
    /// <summary>
    /// Interaktionslogik für QuickWatchPresentationContainer.xaml
    /// </summary>
    public partial class QuickWatchPresentationContainer : UserControl
    {
        public static DependencyProperty ColliderProperty = DependencyProperty.Register("Collider", typeof(IMD5ColliderAlgorithm), typeof(QuickWatchPresentationContainer));
        public IMD5ColliderAlgorithm Collider
        {
            get => (IMD5ColliderAlgorithm)GetValue(ColliderProperty);
            set => SetValue(ColliderProperty, value);
        }

        public QuickWatchPresentationContainer()
        {
            InitializeComponent();
        }
    }
}
