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
using CrypTool.MD5.Algorithm;
using System.Windows;
using System.Windows.Controls;

namespace CrypTool.MD5.Presentation
{
    /// <summary>
    /// Interaktionslogik für PresentationContainer.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.Plugins.MD5.Properties.Resources")]
    public partial class PresentationContainer : UserControl
    {
        private readonly PresentableMD5 md5;

        public PresentationContainer(PresentableMD5 presentableMd5)
        {
            DataContext = md5 = presentableMd5;

            InitializeComponent();

            Width = double.NaN;
            Height = double.NaN;
        }

        private void nextStepButton_Click(object sender, RoutedEventArgs e)
        {
            md5.NextStep();
        }

        private void previousStepButton_Click(object sender, RoutedEventArgs e)
        {
            md5.PreviousStep();
        }

        private void endOfRoundButton_Click(object sender, RoutedEventArgs e)
        {
            md5.NextStepUntilRoundEnd();
        }

        private void endOfCompressionButton_Click(object sender, RoutedEventArgs e)
        {
            md5.NextStepUntilBlockEnd();
        }
    }
}
