/*
   Copyright 2008-2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using CrypTool.PluginBase.Miscellaneous;
using System;

namespace Factorizer
{
    public class FactorizerSettings : ISettings
    {

        private const int BRUTEFORCEMIN = 100;
        private const int BRUTEFORCEMAX = (1 << 30);

        private long m_BruteForceLimit = 100000;
        private bool m_BruteForceLimitEnabled = true;
        private int action = 0; // 0 = factorize, 1 = find smallest factor


        [TaskPane("ActionCaption", "ActionTooltip", "BruteForceLimitGroup", 0, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public int Action
        {
            get => action;
            set
            {
                if (value != action)
                {
                    action = value;
                    FirePropertyChangedEvent("Action");
                }
            }
        }

        [TaskPane("BruteForceLimitEnabledCaption", "BruteForceLimitEnabledTooltip", "BruteForceLimitGroup", 1, false, ControlType.CheckBox)]
        public bool BruteForceLimitEnabled
        {
            get => m_BruteForceLimitEnabled;
            set
            {
                if (value != m_BruteForceLimitEnabled)
                {
                    m_BruteForceLimitEnabled = value;
                    FirePropertyChangedEvent("BruteForceLimitEnabled");
                }
            }
        }

        [TaskPane("BruteForceLimitCaption", "BruteForceLimitTooltip", "BruteForceLimitGroup", 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, BRUTEFORCEMIN, BRUTEFORCEMAX)]
        public long BruteForceLimit
        {
            get => m_BruteForceLimit;
            set
            {
                m_BruteForceLimit = Math.Max(BRUTEFORCEMIN, value);
                m_BruteForceLimit = Math.Min(BRUTEFORCEMAX, value);
                FirePropertyChangedEvent("BruteForceLimit");
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        private void FirePropertyChangedEvent(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }
    }
}
