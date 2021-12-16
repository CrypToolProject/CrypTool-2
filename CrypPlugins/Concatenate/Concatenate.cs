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
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Windows.Controls;

namespace Concatenate
{
    [Author("Matthäus Wander", "wander@CrypTool.org", "Uni Duisburg-Essen", "http://www.vs.uni-due.de")]
    [PluginInfo("Concatenate.Properties.Resources", "PluginCaption", "PluginTooltip", "Concatenate/DetailedDescription/doc.xml", "Concatenate/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataflow)]
    public class Concatenate : ICrypComponent
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private CStreamWriter outputStreamWriter;

        [PropertyInfo(Direction.InputData, "InputStream1Caption", "InputStream1Tooltip", true)]
        public ICrypToolStream InputStreamOne { get; set; }

        [PropertyInfo(Direction.InputData, "InputStream2Caption", "InputStream2Tooltip", true)]
        public ICrypToolStream InputStreamTwo { get; set; }

        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", true)]
        public ICrypToolStream OutputStream => outputStreamWriter;

        public ISettings Settings => null;

        public UserControl Presentation => null;

        public void PreExecution()
        {
        }

        public void Execute()
        {
            if (InputStreamOne == null || InputStreamTwo == null)
            {
                return;
            }

            using (CStreamReader reader1 = InputStreamOne.CreateReader(), reader2 = InputStreamTwo.CreateReader())
            {
                outputStreamWriter = new CStreamWriter();
                EventsHelper.PropertyChanged(PropertyChanged, this, "OutputStream");

                int bytesRead;
                byte[] buffer = new byte[1024];

                // Input One
                while ((bytesRead = reader1.Read(buffer)) > 0)
                {
                    outputStreamWriter.Write(buffer, 0, bytesRead);
                }
                // Input Two
                while ((bytesRead = reader2.Read(buffer)) > 0)
                {
                    outputStreamWriter.Write(buffer, 0, bytesRead);
                }

                outputStreamWriter.Close();
            }
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
            if (outputStreamWriter != null)
            {
                outputStreamWriter.Dispose();
                outputStreamWriter = null;
            }
        }
    }
}
