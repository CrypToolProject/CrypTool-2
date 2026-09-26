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
using CrypTool.Plugins.ChaCha.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace CrypTool.Plugins.ChaCha.View
{
    /// <summary>
    /// Interaction logic for Xor.xaml
    /// </summary>
    [PluginBase.Attributes.Localization("CrypTool.Plugins.ChaCha.Properties.Resources")]
    public partial class Xor : UserControl
    {
        public Xor()
        {
            InitializeComponent();
            ActionViewBase.LoadLocaleResources(this);
            DataContextChanged += OnDataContextChanged;
        }

        private XorViewModel ViewModel { get; set; }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            XorViewModel ViewModel = (XorViewModel)e.NewValue;
            if (ViewModel != null)
            {
                this.ViewModel = ViewModel;
                if (ViewModel.DiffusionActive)
                {
                    InitDiffusionValues();
                }

                Root.Width = ViewModel.ChaCha.Keystream.ToArray().Length * 25;
            }
        }

        /// <summary>
        /// Init the diffusion values for the keystream and the output text.
        /// </summary>
        private void InitDiffusionValues()
        {
            Plugins.ChaCha.ViewModel.Components.Diffusion.InitDiffusionValue(DiffusionKeystream, ViewModel.ChaCha.KeystreamDiffusion.ToArray(), ViewModel.ChaCha.Keystream.ToArray());
            Plugins.ChaCha.ViewModel.Components.Diffusion.InitXORValue(DiffusionKeystreamXOR, ViewModel.ChaCha.KeystreamDiffusion.ToArray(), ViewModel.ChaCha.Keystream.ToArray());
            Plugins.ChaCha.ViewModel.Components.Diffusion.InitDiffusionValue(DiffusionOutput, ViewModel.ChaCha.OutputDiffusion.ToArray(), ViewModel.ChaCha.Output.ToArray());
            Plugins.ChaCha.ViewModel.Components.Diffusion.InitXORValue(DiffusionOutputXOR, ViewModel.ChaCha.OutputDiffusion.ToArray(), ViewModel.ChaCha.Output.ToArray());
        }
    }
}