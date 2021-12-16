/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

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

namespace CrypTool.Plugins.DECRYPTTools
{
    public class DECRYPTParserTesterSettings : ISettings
    {
        private int _maximumNumberOfNulls = 2;
        public event PropertyChangedEventHandler PropertyChanged;

        public void Initialize()
        {

        }

        [TaskPane("MaximumNumberOfNullsCaption", "MaximumNumberOfNullsTooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int MaximumNumberOfNulls
        {
            get => _maximumNumberOfNulls;
            set
            {
                if ((value) != _maximumNumberOfNulls)
                {
                    _maximumNumberOfNulls = value;
                }
            }
        }
    }
}
