/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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

namespace CrypTool.Plugins.A5
{

    // HOWTO: rename class (click name, press F2)
    public class A5Settings : ISettings
    {
        private int framesCount;
        [TaskPane("Framescount", "Number of frames to generate (Must be greater then 1)", null, 0, false, ControlType.TextBox)]
        public int FramesCount
        {
            get => framesCount;
            set
            {
                if (value >= 1)
                {
                    framesCount = value;
                    OnPropertyChanged("FramesCount");
                }
                else
                {
                }
            }
        }
        public A5Settings()
        { FramesCount = 1; }

        public void Initialize()
        {
        }

        #region TaskPane Settings



        #endregion




        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}