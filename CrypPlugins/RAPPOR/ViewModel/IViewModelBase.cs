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
    /// Base class for all view models.
    /// <remarks>
    /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2009/february/patterns-wpf-apps-with-the-model-view-viewmodel-design-pattern#viewmodelbase-class
    /// </remarks>
    /// </summary>
    public interface IViewModelBase
    {
        /// <summary>
        /// Interface method for DrawCanvas which has to be be implemented by every view model class.
        /// </summary>
        void DrawCanvas();
        //void Activate();
        //void Deactivate();
        void ChangeButton(Boolean ru);
        void CreateHeatMapViewText(int it);

    }
}