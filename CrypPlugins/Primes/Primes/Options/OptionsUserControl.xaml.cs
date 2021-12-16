/*
   Copyright 2008 Timo Eckhardt, University of Siegen

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

using Primes.Properties;
using System.Windows.Controls;

namespace Primes.Options
{
    /// <summary>
    /// Interaction logic for OptionsUserControl.xaml
    /// </summary>
    public partial class OptionsUserControl : UserControl
    {
        private readonly Settings m_Settings;
        private readonly IOption m_OptionCountPrimes;

        public OptionsUserControl()
        {
            InitializeComponent();
            m_Settings = new Settings();
            m_OptionCountPrimes = optCountPrimes;
            optCountPrimes.Settings = m_Settings;
        }

        public bool Save()
        {
            bool result = m_OptionCountPrimes.Save();

            if (result)
            {
                m_Settings.Save();
            }

            return result;
        }
    }
}
