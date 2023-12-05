/*
   Copyright 2023 Nils Kopal <kopal<AT>CrypTool.org>

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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using WorkspaceManager.Model;

namespace CrypTool.CrypConsole
{
    /// <summary>
    /// Container for values defined in the json input file
    /// </summary>
    public class JsonInput
    {
        public bool Verbose { get; internal set; }
        public int Timeout { get; internal set; }
        public NotificationLevel Loglevel { get; internal set; }
        public bool JsonOutput { get; internal set; }
        public string CwmFile { get; internal set; }
        public List<Setting> Settings { get; internal set; }
        public List<Parameter> InputParameters { get; internal set; }
        public List<Parameter> OutputParameters { get; internal set; }
    }

    /// <summary>
    /// Helper class for parsing the json input file and generating json output
    /// </summary>
    public class JsonHelper
    {
        /// <summary>
        /// Returns the output string as json string
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public static string GetOutputJsonString(string output, string name)
        {
            return string.Format("{{\"name\":\"{0}\",\"value\":\"{1}\"}}", name, EscapeString(output));
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
                foreach (ConnectorModel input in inputs)
                {
                    counter++;
                    stringBuilder.AppendFormat("{{\"name\":\"{0}\",\"type\":\"{1}\"}}", input.GetName(), input.ConnectorType.FullName);
                    if (counter < inputs.Count)
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
                foreach (ConnectorModel output in outputs)
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
                foreach (TaskPaneAttribute taskPaneAttribute in taskPaneAttributes)
                {
                    //if enum generate possible values
                    if (taskPaneAttribute.PropertyInfo.PropertyType.IsEnum)
                    {
                        stringBuilder.AppendFormat("{{\"name\":\"{0}\",\"type\":\"{1}\",\"values\":[", taskPaneAttribute.PropertyName, taskPaneAttribute.PropertyInfo.PropertyType.FullName);
                        int enumCounter = 0;
                        foreach (string enumValue in Enum.GetNames(taskPaneAttribute.PropertyInfo.PropertyType))
                        {
                            enumCounter++;
                            stringBuilder.AppendFormat("{{\"name\":\"{0}\",\"value\":\"{1}\"}}", enumValue, enumValue);
                            if (enumCounter < Enum.GetNames(taskPaneAttribute.PropertyInfo.PropertyType).Length)
                            {
                                stringBuilder.Append(",");
                            }
                        }
                        stringBuilder.AppendFormat("]}}");                        
                        counter++;
                        if (counter < taskPaneAttributes.Length)
                        {
                            stringBuilder.Append(",");
                        }
                    }
                    else
                    {
                        counter++;
                        stringBuilder.AppendFormat("{{\"name\":\"{0}\",\"type\":\"{1}\"}}", taskPaneAttribute.PropertyName, taskPaneAttribute.PropertyInfo.PropertyType.FullName);
                        if (counter < taskPaneAttributes.Length)
                        {
                            stringBuilder.Append(",");
                        }
                    }                 
                }
                stringBuilder.AppendFormat("]");
            }
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Parses the json input file and returns a JsonInput object using System.Text.Json
        /// Example json input file:
        /// 
        /// {
        ///    "verbose":false,
        ///    "timeout":10,
        ///    "loglevel":"Error",
        ///    "jsonoutput":true,
        ///    "cwmfile":"C:\\Path\\To\\Caesar.cwm",
        ///    "inputs":[
        ///       {
        ///          "type":"text",
        ///          "name":"plaintext",
        ///          "value":"rovvy gybvn"
        ///       }
        ///    ],
        ///    "outputs":[
        ///       {
        ///          "name":"Ciphertext"
        ///       }
        ///    ],
        ///    "settings":[
        ///       {
        ///          "component":"Caesar",
        ///          "setting":"Action",
        ///          "value":"Decrypt"
        ///       }
        ///    ]
        /// }
        ///  
        /// </summary>
        /// <param name="jsonfile"></param>        
        /// <returns></returns>
        public static JsonInput ParseAndValidateJsonInput(string jsonfile)
        {
            JsonInput jsonInput = new JsonInput();
            try
            {
                using (StreamReader file = File.OpenText(jsonfile))
                {
                    string json = file.ReadToEnd();
                    JObject jobject = (JObject)JsonConvert.DeserializeObject(json);
                    
                    //check, if verbose exists in inputm then assign it to jsonInput.Verbose
                    //otherwise assign false to jsonInput.Verbose
                    if (jobject["verbose"] != null)
                    {
                        jsonInput.Verbose = jobject["verbose"].Value<string>().ToLower() == "true" ? true : false;
                    }
                    else
                    {
                        jsonInput.Verbose = false;
                    }
                    
                    //check, if timeout exists
                    if (jobject["timeout"] != null)
                    {
                        jsonInput.Timeout = int.Parse(jobject["timeout"].Value<string>());
                    }
                    else
                    {
                        jsonInput.Timeout = 10;
                    }

                    //check, if loglevel exists
                    if (jobject["loglevel"] != null)
                    {
                        string loglevel = jobject["loglevel"].Value<string>();
                        switch (loglevel.ToLower())
                        {
                            case "debug":
                                jsonInput.Loglevel = NotificationLevel.Debug;
                                break;
                            case "info":
                                jsonInput.Loglevel = NotificationLevel.Info;
                                break;
                            case "warning":
                                jsonInput.Loglevel = NotificationLevel.Warning;
                                break;
                            case "error":
                                jsonInput.Loglevel = NotificationLevel.Error;
                                break;
                            default:
                                WriteOutput("Error", "Error parsing loglevel from json file. Invalid logtype given: " + loglevel.ToLower(), Main.JsonOutput);
                                Environment.Exit(-3);
                                break;
                        }
                    }
                    else
                    {
                        jsonInput.Loglevel = NotificationLevel.Error;
                    }

                    //check, if jsonoutput exists
                    if (jobject["jsonoutput"] != null)
                    {
                        jsonInput.JsonOutput = jobject["jsonoutput"].Value<string>().ToLower() == "true" ? true : false;
                    }
                    else
                    {
                        jsonInput.JsonOutput = false;
                    }

                    //check, if cwmfile exists
                    if (jobject["cwmfile"] != null)
                    {
                        jsonInput.CwmFile = jobject["cwmfile"].Value<string>();
                    }
                    else
                    {
                        jsonInput.CwmFile = null;
                    }

                    //check, if inputs exist
                    if (jobject["inputs"] == null)
                    {
                        jsonInput.InputParameters = new List<Parameter>();
                    }
                    else
                    {
                        jsonInput.InputParameters = new List<Parameter>();
                        foreach (JToken input in jobject["inputs"].Children())
                        {
                            jsonInput.InputParameters.Add(new Parameter(input["type"].Value<string>(), input["name"].Value<string>(), input["value"].Value<string>()));
                        }
                    }

                    //check, if outputs exist
                    if (jobject["outputs"] == null)
                    {
                        jsonInput.OutputParameters = new List<Parameter>();
                    }
                    else
                    {
                        jsonInput.OutputParameters = new List<Parameter>();
                        foreach (JToken output in jobject["outputs"].Children())
                        {
                            jsonInput.OutputParameters.Add(new Parameter(ParameterType.Output, output["name"].Value<string>(), "none"));
                        }
                    }

                    //check, if settings exist
                    if (jobject["settings"] == null)
                    {
                        jsonInput.Settings = new List<Setting>();
                    }
                    else
                    {
                        jsonInput.Settings = new List<Setting>();
                        foreach (JToken setting in jobject["settings"].Children())
                        {
                            jsonInput.Settings.Add(new Setting(setting["component"].Value<string>(), setting["setting"].Value<string>(), setting["value"].Value<string>()));
                        }
                    }
                }
            }
            catch (Exception ex)
            {                
                WriteOutput("Error", "Error parsing json input file: " + ex.Message, Main.JsonOutput);
                Environment.Exit(-3);
            }

            //show error message, if cwm file is not specified
            if (jsonInput.CwmFile == null)
            {
                WriteOutput("Error", "Please specify a cwm file using \"cwmfile\":\"C:\\\\Path\\\\To\\\\cwm file\" ", Main.JsonOutput);
                Environment.Exit(-1);
            }

            //show error message, if cwm file does not exist
            if (!File.Exists(jsonInput.CwmFile))
            {
                WriteOutput("Error", "Specified cwm file \"" + jsonInput.CwmFile + "\" does not exist", Main.JsonOutput);
                Environment.Exit(-2);
            }

            //check, if timeout <=0
            if (jsonInput.Timeout <= 0)
            {
                WriteOutput("Error", "Timeout must be greater than 0", Main.JsonOutput);
                Environment.Exit(-2);
            }

            return jsonInput;
        }
        
        /// <summary>
        /// Writes the output as standard output, if jsonoutput is false; otherwise as json string
        /// </summary>
        /// <param name="name"></param>
        /// <param name="output"></param>
        /// <param name="jsonoutput"></param>
        public static void WriteOutput(string name, string output, bool jsonoutput)
        {
            if (jsonoutput)
            {
                Console.WriteLine(GetOutputJsonString(output, name));
            }
            else
            {
                Console.WriteLine(output);
            }
        }
    }
}
