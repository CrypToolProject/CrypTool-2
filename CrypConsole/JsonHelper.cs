/*
   Copyright 2020 Nils Kopal <kopal<AT>CrypTool.org>

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
using System.Collections.ObjectModel;
using System.Text;
using CrypTool.PluginBase;
using WorkspaceManager.Model;

namespace CrypTool.CrypConsole
{
    public class JsonHelper
    {
        /// <summary>
        /// Returns the output string as json string
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public static string GetOutputJsonString(string output, string name)
        {
            return string.Format("{{\"output\":{{\"name\":\"{0}\",\"value\":\"{1}\"}}}}", name, EscapeString(output));
        }
       
        /// <summary>
        /// Returns the log as json string
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string GetLogJsonString(IPlugin sender, GuiLogEventArgs args)
        {
            return string.Format("{{\"log\":{{\"logtime\":\"{0}\",\"logtype\":\"{1}\",\"sender\":\"{2}\",\"message\":\"{3}\"}}}}",
                DateTime.Now,
                args.NotificationLevel,
                sender == null ? "null" : sender.GetPluginInfoAttribute().Caption,
                EscapeString(args.Message == null ? "null" : args.Message));
        }

        /// <summary>
        /// Returns the global progress as json string
        /// </summary>
        /// <param name="globalProgress"></param>
        /// <returns></returns>
        public static string GetProgressJson(int globalProgress)
        {
            return string.Format("{{\"progress\":{{\"value\":\"{0}\"}}}}", globalProgress);
        }

        /// <summary>
        /// Escapes the string by replacing \ with \\
        /// and '\r' with \r and '\n' with \n
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        private static string EscapeString(string output)
        {
            string newoutput = output.Replace("\\", "\\\\");
            newoutput = newoutput.Replace("\n", "\\n");
            newoutput = newoutput.Replace("\r", "\\r");
            return newoutput;
        }

        /// <summary>
        /// Returns the inputs, outputs, and settings as a json string
        /// </summary>
        /// <param name="globalProgress"></param>
        /// <returns></returns>
        public static string GetPluginDiscoveryString(PluginModel pluginModel, ReadOnlyCollection<ConnectorModel> inputs, ReadOnlyCollection<ConnectorModel> outputs, TaskPaneAttribute[] taskPaneAttributes)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("{{\"name\":\"{0}\", \"type\":\"{1}\"", pluginModel.GetName(), pluginModel.Plugin.GetType().FullName);

            if (inputs.Count > 0)
            {
                stringBuilder.AppendFormat(",\"inputs\":[");
                int counter = 0;
                foreach (var input in inputs)
                {
                    counter++;
                    stringBuilder.AppendFormat("{{\"name\":\"{0}\",\"type\":\"{1}\"}}", input.GetName(), input.ConnectorType.FullName);
                    if(counter < inputs.Count)
                    {
                        stringBuilder.Append(",");
                    }
                }
                stringBuilder.AppendFormat("]");
            }
            if (outputs.Count > 0)
            {
                stringBuilder.AppendFormat(",\"outputs\":[");
                int counter = 0;
                foreach (var output in outputs)
                {
                    counter++;
                    stringBuilder.AppendFormat("{{\"name\":\"{0}\",\"type\":\"{1}\"}}", output.GetName(), output.ConnectorType.FullName);
                    if (counter < outputs.Count)
                    {
                        stringBuilder.Append(",");
                    }
                }
                stringBuilder.AppendFormat("]");
            }
            if (taskPaneAttributes != null && taskPaneAttributes.Length > 0)
            {
                stringBuilder.AppendFormat(",\"settings\":[");
                int counter = 0;
                foreach (var taskPaneAttribute in taskPaneAttributes)
                {
                    counter++;
                    stringBuilder.AppendFormat("{{\"name\":\"{0}\",\"type\":\"{1}\"}}", taskPaneAttribute.PropertyName, taskPaneAttribute.PropertyInfo.PropertyType.FullName);
                    if (counter < taskPaneAttributes.Length)
                    {
                        stringBuilder.Append(",");
                    }
                }
                stringBuilder.AppendFormat("]");
            }
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }

    }
}
