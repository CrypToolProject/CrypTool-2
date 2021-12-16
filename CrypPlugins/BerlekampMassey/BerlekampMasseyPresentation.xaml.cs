/*
   Copyright 2009 Sören Rinne, Ruhr-Universität Bochum, Germany

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

using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.BerlekampMassey
{
    /// <summary>
    /// Interaction logic for BerlekampMasseyPresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.BerlekampMassey.Properties.Resources")]
    public partial class BerlekampMasseyPresentation : UserControl
    {
        public BerlekampMasseyPresentation()
        {
            InitializeComponent();
            Height = double.NaN;
            Width = double.NaN;
        }

        public void setLength(string length)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                lText.Text = length;
            }, null);
        }

        public void setPolynomial(string polynommial)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                cdText.Text = polynommial;
            }, null);
        }
    }
}
