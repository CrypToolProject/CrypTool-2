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

using CrypTool.PluginBase;
using System.ComponentModel;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.BooleanFunctionParser
{
    class BooleanFunctionParserSettings : ISettings
    {
        #region Private variables

        private bool useBFPforCube = false;

        #endregion

        #region for Quickwatch

        private string function;
        public string Function
        {
            get { return function; }
            set
            {
                if (value != function)
                {
                    function = value;
                    OnPropertyChanged("Function");
                }
            }
        }

        private string data;
        public string Data
        {
            get { return data; }
            set
            {
                if (value != data)
                {
                    data = value;
                    OnPropertyChanged("Data");
                }
            }
        }

        private string functionCube;
        public string FunctionCube
        {
            get { return functionCube; }
            set
            {
                if (value != functionCube)
                {
                    functionCube = value;
                    OnPropertyChanged("FunctionCube");
                }
            }
        }

        private string dataCube;
        public string DataCube
        {
            get { return dataCube; }
            set
            {
                if (value != dataCube)
                {
                    dataCube = value;
                    OnPropertyChanged("DataCube");
                }
            }
        }

        #endregion

        #region ISettings Members

        [ContextMenu( "UseBFPforCubeCaption", "UseBFPforCubeTooltip",
            1,
            ContextMenuControlType.CheckBox,
            null,
            "")]
        [TaskPane( "UseBFPforCubeCaption", "UseBFPforCubeTooltip",
            null,
            1,
            false,
            ControlType.CheckBox, "")]
        public bool UseBFPforCube
        {
            get { return this.useBFPforCube; }
            set
            {
                if (value != this.useBFPforCube)
                {
                    this.useBFPforCube = value;
                    OnPropertyChanged("UseBFPforCube");                    
                }
            }
        }

        /*[TaskPane("Evaluate function", "", null, 2, false, ControlType.Button)]
        public void evalFunction()
        {
            OnPropertyChanged("evalFunction");
        }*/

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
