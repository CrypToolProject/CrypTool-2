/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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


namespace CrypTool.Plugins.BooleanOperators
{
    [Author("Nils Kopal", "nils.kopal@CrypTool.org", "Uni Duisburg-Essen", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("BooleanOperators.Properties.Resources", "PluginBBO_Caption", "PluginBBO_Tooltip", "BooleanOperators/DetailedDescription/doc.xml",
        "BooleanOperators/icons/and.png",
        "BooleanOperators/icons/or.png",
        "BooleanOperators/icons/nand.png",
        "BooleanOperators/icons/nor.png",
        "BooleanOperators/icons/xor.png"
        )]
    [ComponentCategory(ComponentCategory.ToolsBoolean)]
    public class BooleanBinaryOperators : ICrypComponent
    {
        private bool FlagA = false;
        private bool FlagB = false;

        private bool inputA = false;
        private bool inputB = false;
        private bool output = false;

        private BooleanBinaryOperatorsSettings settings;

        public BooleanBinaryOperators()
        {
            settings = new BooleanBinaryOperatorsSettings();
            settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;

        }

        [PropertyInfo(Direction.InputData, "BBO_InputACaption", "BBO_InputATooltip")]
        public bool InputA
        {
            get => inputA;
            set
            {
                inputA = value;
                FlagA = true;
                OnPropertyChange("InputA");
            }
        }

        [PropertyInfo(Direction.InputData, "BBO_InputBCaption", "BBO_InputBTooltip")]
        public bool InputB
        {
            get => inputB;
            set
            {
                inputB = value;
                FlagB = true;
                OnPropertyChange("InputB");
            }
        }


        [PropertyInfo(Direction.OutputData, "BBO_OutputCaption", "BBO_OutputTooltip")]
        public bool Output
        {
            get => output;
            set
            {
                output = value;
                OnPropertyChange("Output");
            }
        }


        public ISettings Settings
        {
            get => settings;
            set => settings = (BooleanBinaryOperatorsSettings)value;
        }


        #region IPlugin Members

        public void Dispose()
        {
        }

        public void Execute()
        {

            if (settings.UpdateOnlyAtBothInputsChanged &&
                !(FlagA && FlagB))
            {
                //We only update our output if both inputs have changed
                return;
            }

            switch (settings.OperatorType)
            {
                case 0: //AND
                    Output = inputA && InputB;
                    break;

                case 1: //OR
                    Output = inputA || InputB;
                    break;

                case 2: //NAND
                    Output = !(inputA && InputB);
                    break;

                case 3: //NOR
                    Output = !(inputA || InputB);
                    break;

                case 4: //XOR
                    Output = inputA ^ InputB;
                    break;

                default:
                    Output = false;
                    break;

            }//end switch

            FlagA = false;
            FlagB = false;

            ProgressChanged(1, 1);

        }

        public void Initialize()
        {
        }

        public void PostExecution()
        {
        }

        public void PreExecution()
        {

            FlagA = ((BooleanBinaryOperatorsSettings)Settings).DefaultFlagA;
            FlagB = ((BooleanBinaryOperatorsSettings)Settings).DefaultFlagB;
        }

        public System.Windows.Controls.UserControl Presentation => null;

        public void Stop()
        {
            FlagA = false;
            FlagB = false;
        }

        #endregion

        #region event handling

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChange(string propertyname)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(propertyname));
        }

        private void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(this, args);
            }
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        #endregion
    }
}
