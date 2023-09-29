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
using System;

namespace CrypTool.PluginBase.Miscellaneous
{
    public class ApplicationSettingsHelper
    {
        /// <summary>
        /// Saves the Application settings. If save fails it tries to save again. If
        /// 2. try fails too app settings wont be saved
        /// </summary>
        public static void SaveApplicationsSettings()
        {
            try
            {
                Properties.Settings.Default.Save();
            }
            catch (Exception)
            {
                //if saving failed try one more time
                try
                {
                    Properties.Settings.Default.Save();
                }
                catch (Exception)
                {
                    //if saving failed again we do not try it again
                }
            }
        }
    }
}
