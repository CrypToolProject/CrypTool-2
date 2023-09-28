/*
   Copyright 2008 - 2022 CrypTool Team

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
namespace CrypTool.PluginBase
{
    public interface ICrypComponent : IPlugin
    {
        /// <summary>
        /// Will be called once before workflow starts. May be used to set up data used for execution.
        /// </summary>
        void PreExecution();

        /// <summary>
        /// Will be called after the workflow has been stopped. May be used for cleanup data used during execution.
        /// </summary>
        void PostExecution();

        event StatusChangedEventHandler OnPluginStatusChanged;
        event PluginProgressChangedEventHandler OnPluginProgressChanged;
    }
}
