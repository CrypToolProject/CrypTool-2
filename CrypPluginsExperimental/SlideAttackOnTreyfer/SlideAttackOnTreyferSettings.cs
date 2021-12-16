/*                              
  Aditya Deshpande, University of Mannheim

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
using System.ComponentModel;

namespace CrypTool.SlideAttackOnTreyfer
{
    public class SlideAttackOnTreyferSettings : ISettings
    {
        #region Public TREYFER specific interface

        /// <summary>
        /// We use this delegate to send log messages from the settings class to the TREYFER plugin
        /// </summary>
        public delegate void TREYFERLogMessage(string msg, NotificationLevel loglevel);

        public enum SlideAttackOnTreyferMode { Encrypt = 0, Decrypt = 1 };

        /// <summary>
        /// An enumaration for the different modes of dealing with unknown characters
        /// </summary>
      //  public enum UnknownSymbolHandlingMode { Ignore = 0, Remove = 1, Replace = 2 };


        /// <summary>
        /// Feuern, wenn ein neuer Text im Statusbar angezeigt werden soll.
        /// </summary>
        public event TREYFERLogMessage LogMessage;

        #endregion

        #region Private variables and public constructor

        private SlideAttackOnTreyferMode selectedAction = SlideAttackOnTreyferMode.Encrypt;


        public SlideAttackOnTreyferSettings()
        {

        }

        #endregion

        #region Private methods

        private void OnLogMessage(string msg, NotificationLevel level)
        {
            if (LogMessage != null)
            {
                LogMessage(msg, level);
            }
        }

        #endregion

        #region Algorithm settings properties (visible in the Settings pane)


        [TaskPane("ActionTPCaption", "ActionTPTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public SlideAttackOnTreyferMode Action
        {
            get => selectedAction;
            set
            {
                if (value != selectedAction)
                {
                    selectedAction = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
