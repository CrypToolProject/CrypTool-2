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
using System;

namespace CrypTool.Plugins.RAPPOR.ViewModel
{
    /// <summary>
    /// Internal class of the state view model, handling the ui logic of the start view.
    /// </summary>
    public class StartViewModel : IViewModelBase
    {
        /// <summary>
        /// Name of the start view model.
        /// </summary>
        private readonly string name;
        /// <summary>
        /// Constructor for the start view model.
        /// </summary>
        public StartViewModel()
        {
            name = "{Loc Start}";
        }
        /// <summary>
        /// Empty constructer class for drawing on the canvas. The canvas is handled by the start
        /// xaml.
        /// </summary>
        public void DrawCanvas()
        {
        }

        /// <summary>
        /// Getter for the name of the start view model.
        /// </summary>
        /// <returns>Returns the name of the start view model as an string.</returns>
        public string GetName()
        {
            return name;
        }
        public new void ChangeButton(Boolean ru)
        {
        }
        public void CreateHeatMapViewText(int a)
        {
        }
    }
}