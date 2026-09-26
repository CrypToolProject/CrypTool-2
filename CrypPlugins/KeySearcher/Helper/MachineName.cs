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
namespace KeySearcher.Helper
{
    public static class MachineName
    {
        public delegate void OnMachineNameToUseChangedHandler(string newMachineNameToUse);
        public static event OnMachineNameToUseChangedHandler OnMachineNameToUseChanged;

        private static readonly string realMachineName = "";
        private static readonly long id = 0;

        public static string MachineNameToUse
        {
            get;
            private set;
        }

        static MachineName()
        {
            MachineNameToUse = GenerateMachineNameToUse();
            CrypTool.PluginBase.Properties.Settings.Default.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Default_PropertyChanged);
        }

        private static void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == "Anonymize") || (e.PropertyName == "MachNameChars"))
            {
                if (OnMachineNameToUseChanged != null)
                {
                    MachineNameToUse = GenerateMachineNameToUse();
                    OnMachineNameToUseChanged(MachineNameToUse);
                }
            }
        }

        private static string GenerateMachineNameToUse()
        {
            if (!CrypTool.PluginBase.Properties.Settings.Default.KeySearcher_Anonymize)
            {
                return realMachineName;
            }
            else
            {
                return string.Format("{0}_{1:X}", realMachineName.Substring(0, CrypTool.PluginBase.Properties.Settings.Default.KeySearcher_MachNameChars), id);
            }
        }
    }
}
