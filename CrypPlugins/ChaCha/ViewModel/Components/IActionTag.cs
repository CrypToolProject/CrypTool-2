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
    internal interface IActionTag
    {
        /// <summary>
        /// Saves action indices under a "tag" for later retrieval.
        /// This implements "action tagging". We can mark actions with a string
        /// and then retrieve their action index using that string.
        /// One must use this function during action creation and call
        /// it with the index of the action we want to tag.
        /// </summary>
        void TagAction(string tag, int actionIndex);

        /// <summary>
        /// Return the action index of the given action tag.
        /// </summary>
        int GetTaggedActionIndex(string tag);
    }
}