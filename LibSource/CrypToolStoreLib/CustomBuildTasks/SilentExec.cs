/*
   Copyright 2018 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

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
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Diagnostics;

namespace CustomBuildTasks
{
    public class SilentExec : Task
    {
        [Required]
        public string Command { get; set; }

        public string Arguments { get; set; }

        /// <summary>
        /// Executes a task without showing a window and without logging the output to console
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo(Command);
                info.Arguments = Arguments;
                info.CreateNoWindow = false;
                info.UseShellExecute = false;
                Process p = Process.Start(info);
                p.WaitForExit();
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
            }
            return !Log.HasLoggedErrors;

        }
    }
}