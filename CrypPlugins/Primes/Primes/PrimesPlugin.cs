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
using CrypTool.PluginBase;
using Primes.WpfVisualization;

namespace Primes
{
    [Author("Timo Eckhardt", "T-Eckhardt@gmx.de", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("Primes.Properties.Resources", "PluginCaption", "PluginTooltip", "Primes/DetailedDescription/doc.xml", "Primes/PrimesPlugin.png")]
    [FunctionList("FL_F_factorization", "FL_P_bruteforce")]
    [FunctionList("FL_F_factorization", "FL_P_qs")]
    [FunctionList("FL_F_primalitytest", "FL_P_eratosthenes")]
    [FunctionList("FL_F_primalitytest", "FL_P_millerrabin")]
    [FunctionList("FL_F_primalitytest", "FL_P_atkin")]
    [FunctionList("FL_F_primegeneration", "FL_P_primegeneration")]
    [FunctionList("FL_F_primedistribution", "FL_P_numberline")]
    [FunctionList("FL_F_primedistribution", "FL_P_numbergrid")]
    [FunctionList("FL_F_primedistribution", "FL_P_numberofprimes")]
    [FunctionList("FL_F_primedistribution", "FL_P_ulam")]
    [FunctionList("FL_F_numbertheory", "FL_P_powering_iteratingexponent")]
    [FunctionList("FL_F_numbertheory", "FL_P_powering_iteratingbase")]
    [FunctionList("FL_F_numbertheory", "FL_P_numbertheoryfunctions")]
    [FunctionList("FL_F_numbertheory", "FL_P_primitiveroots")]
    [FunctionList("FL_F_numbertheory", "FL_P_goldbach")]
    public class PrimesPlugin : ICrypTutorial
    {
        #region IPlugin Members

        private PrimesControl m_PrimesPlugin = null;
        public event CrypTool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;

        public event CrypTool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event CrypTool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;

        public CrypTool.PluginBase.ISettings Settings => null;

        public System.Windows.Controls.UserControl Presentation
        {
            get
            {
                if (m_PrimesPlugin == null)
                {
                    m_PrimesPlugin = new PrimesControl();
                }

                return m_PrimesPlugin;
            }
        }

        public void Execute()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
            if (m_PrimesPlugin != null)
            {
                m_PrimesPlugin.Dispose();
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}