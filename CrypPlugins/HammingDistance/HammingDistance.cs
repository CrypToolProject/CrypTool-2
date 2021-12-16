/*                              
   Copyright 2014 Nils Kopal, University of Kassel

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
using System.ComponentModel;
using System.Windows.Controls;

namespace HammingDistance
{
    [Author("Nils Kopal", "nils.kopal@uni-kassel.org", "Universität Kassel", "http://www.ais.uni-kassel.de")]
    [PluginInfo("HammingDistance.Properties.Resources", "PluginCaption", "PluginTooltip", "HammingDistance/DetailedDescription/doc.xml", "HammingDistance/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class HammingDistance : ICrypComponent
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public ISettings Settings { get; private set; }
        public UserControl Presentation { get; private set; }

        [PropertyInfo(Direction.InputData, "InputCodeword1", "InputCodeword1Tooltip", true)]
        public byte[] InputData1
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "InputCodeword2", "InputCodeword2Tooltip", true)]
        public byte[] InputData2
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "DistanceValue", "DistanceValueTooltip", true)]
        public int DistanceValue
        {
            get;
            set;
        }

        public void Execute()
        {
            if (InputData1.Length != InputData2.Length)
            {
                GuiLogMessage("Codeword lengths are not equal!", NotificationLevel.Error);
                return;
            }

            int distance = 0;

            for (int i = 0; i < InputData1.Length; i++)
            {
                int x = InputData1[i];
                int y = InputData2[i];
                int z = x ^ y;
                while (z != 0)
                {
                    distance += 1;
                    z &= z - 1;
                }
            }
            DistanceValue = distance;
            OnPropertyChanged("DistanceValue");
        }


        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            }
        }

        private void OnPropertyChanged(string p)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(p));
        }

        public void Dispose()
        {

        }

        public void Stop()
        {

        }

        public void Initialize()
        {

        }

        public void PreExecution()
        {

        }

        public void PostExecution()
        {

        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
    }
}
