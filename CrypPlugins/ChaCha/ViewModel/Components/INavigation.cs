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
namespace CrypTool.Plugins.ChaCha.ViewModel.Components
{
    /// <summary>
    /// Interface for page view models which are navigable to by the page navigation.
    /// </summary>
    internal interface INavigation
    {
        /// <summary>
        /// Page name. Will be used as content in page navigation button.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Function which should be called if user enters this page.
        /// </summary>
        void Setup();

        /// <summary>
        /// Function which should be called if user leaves this page.
        /// </summary>
        void Teardown();
    }
}