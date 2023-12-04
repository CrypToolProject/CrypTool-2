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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using WorkspaceManager.Execution;
using WorkspaceManager.Model;
using Path = System.IO.Path;

namespace CrypTool.CrypConsole
{
    public partial class Main : Window
    {
        private static readonly string[] subfolders = new string[]
        {
            "",
            "CrypPlugins",
            "Lib",
        };

        private bool _verbose = false;
        private int _timeout = int.MaxValue;
        private readonly Dictionary<IPlugin, double> _pluginProgressValues = new Dictionary<IPlugin, double>();
        private readonly Dictionary<IPlugin, string> _pluginNames = new Dictionary<IPlugin, string>();
        private readonly List<string> _outputValues = new List<string>();
        private WorkspaceModel _workspaceModel = null;
        private ExecutionEngine _engine = null;
        private int _globalProgress;
        private DateTime _startTime;
        private readonly object _progressLockObject = new object();        
        private NotificationLevel _loglevel = NotificationLevel.Warning;
        private bool _terminate = false;
        private string _cwm_file = null;
        private List<Setting> _settings = null;
        private List<Parameter> _inputParameters = null;
        private List<Parameter> _outputParameters = null;

        public static bool JsonOutput { get; private set; } = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public Main()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called, after "ui" is initialized. From this point, we should have a running ui thread
        /// Thus, we start the execution of the CrypConsole
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Main_Initialized(object sender, EventArgs e)
        {
            Start(CrypConsole.Args);
        }

        /// <summary>
        /// Starts the execution of the defined workspace
        /// 1) Parses the commandline parameters
        /// 2) Creates CT2 model and execution engine
        /// 3) Starts execution
        /// 4) Gives data as defined by user to the model
        /// 5) Retrieves results for output and outputs these
        /// 6) [terminates]
        /// </summary>
        /// <param name="args"></param>
        public void Start(string[] args)
        {
            _startTime = DateTime.Now;

            //Step 0: Set locale to English
            CultureInfo cultureInfo = new CultureInfo("en-us", false);
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;

            //Step 1: Check, if Help needed
            if (ArgsHelper.GetShowHelp(args))
            {
                Environment.Exit(0);
            }

            string jsonInput = null;
            //check, if not jsoninput is given
            if ((jsonInput = ArgsHelper.GetJsonInput(args)) == null)
            {
                GetArgumentValues(args);
            }
            else
            {
                ParseJsonInputFile(jsonInput);
            }            

            //Step 8: Update application domain. This allows loading additional .net assemblies
            try
            {
                UpdateAppDomain();
            }
            catch (Exception ex)
            {
                JsonHelper.WriteOutput("Exception", string.Format("Exception occured while updating AppDomain: {0}", ex.Message), JsonOutput);
                Environment.Exit(-4);
            }

            //Step 9: Load cwm file and create model            
            try
            {
                ModelPersistance modelPersistance = new ModelPersistance();
                _workspaceModel = modelPersistance.loadModel(_cwm_file, true);

                foreach (PluginModel pluginModel in _workspaceModel.GetAllPluginModels())
                {
                    pluginModel.Plugin.OnGuiLogNotificationOccured += OnGuiLogNotificationOccured;
                }
            }
            catch (Exception ex)
            {                
                JsonHelper.WriteOutput("Exception", string.Format("Exception occured while loading model from cwm file: {0}", ex.Message), JsonOutput);
                Environment.Exit(-5);
            }

            //Step 10: Set input parameters
            foreach (Parameter param in _inputParameters)
            {
                string name = param.Name;
                bool found = false;
                foreach (PluginModel component in _workspaceModel.GetAllPluginModels())
                {
                    //we also memorize here the name of each plugin
                    if (!_pluginNames.ContainsKey(component.Plugin))
                    {
                        _pluginNames.Add(component.Plugin, component.GetName());
                    }

                    if (component.GetName().ToLower().Equals(param.Name.ToLower()))
                    {
                        if (component.PluginType.FullName.Equals("CrypTool.TextInput.TextInput"))
                        {
                            ISettings plugin_settings = component.Plugin.Settings;
                            PropertyInfo textProperty = plugin_settings.GetType().GetProperty("Text");

                            if (param.ParameterType == ParameterType.Text)
                            {
                                textProperty.SetValue(plugin_settings, param.Value);
                            }
                            else if (param.ParameterType == ParameterType.File)
                            {
                                try
                                {
                                    if (!File.Exists(param.Value))
                                    {                                        
                                        JsonHelper.WriteOutput("Exception", string.Format("Input file does not exist: {0}", param.Value), JsonOutput);
                                        Environment.Exit(-7);
                                    }
                                    string value = File.ReadAllText(param.Value);
                                    textProperty.SetValue(plugin_settings, value);
                                }
                                catch (Exception ex)
                                {                                    
                                    JsonHelper.WriteOutput("Exception", string.Format("Exception occured while reading file {0}: {1}", param.Value, ex.Message), JsonOutput);
                                    Environment.Exit(-7);
                                }
                            }
                            //we need to call initialize to get the new text to the ui of the TextInput component
                            //otherwise, it will output the value retrieved by deserialization
                            component.Plugin.Initialize();
                            found = true;
                        }
                        else if (component.PluginType.FullName.Equals("CrypTool.Plugins.Numbers.NumberInput"))
                        {
                            ISettings plugin_settings = component.Plugin.Settings;
                            PropertyInfo textProperty = plugin_settings.GetType().GetProperty("Number");

                            if (param.ParameterType == ParameterType.Number)
                            {
                                textProperty.SetValue(plugin_settings, param.Value);
                            }
                            //we need to call initialize to get the new text to the ui of the TextInput component
                            //otherwise, it will output the value retrieved by deserialization
                            component.Plugin.Initialize();
                            found = true;
                        }
                    }
                }
                if (!found)
                {
                    JsonHelper.WriteOutput("Exception", string.Format("Component for setting input parameter not found: {0}", param), JsonOutput);
                    Environment.Exit(-7);
                }
            }
            
            //Step 11: Set settings
            foreach(Setting setting in _settings)
            {
                ChangeSetting(setting);                
            }

            //Step 12: Set output parameters
            foreach (Parameter param in _outputParameters)
            {
                string name = param.Name;
                bool found = false;
                foreach (PluginModel component in _workspaceModel.GetAllPluginModels())
                {
                    if (component.GetName().ToLower().Equals(param.Name.ToLower()) && component.PluginType.FullName.Equals("TextOutput.TextOutput"))
                    {                     
                        component.Plugin.PropertyChanged += Plugin_PropertyChanged;
                        found = true;                    
                    }
                }
                if (!found)
                {                    
                    JsonHelper.WriteOutput("Exception", string.Format("TextOutput for setting output parameter not found: {0}", param), JsonOutput);
                    Environment.Exit(-8);
                }
            }

            //Step 13: add OnPluginProgressChanged handlers
            foreach (PluginModel plugin in _workspaceModel.GetAllPluginModels())
            {
                plugin.Plugin.OnPluginProgressChanged += OnPluginProgressChanged;
            }

            //Step 14: Create execution engine            
            try
            {
                _engine = new ExecutionEngine(null);
                _engine.OnGuiLogNotificationOccured += OnGuiLogNotificationOccured;
                _engine.Execute(_workspaceModel, false);
            }
            catch (Exception ex)
            {
                JsonHelper.WriteOutput("Exception", string.Format("Exception occured while executing model: {0}", ex.Message), JsonOutput);
                Environment.Exit(-9);
            }        

            //Step 15: Start execution in a dedicated thread
            DateTime endTime = DateTime.Now.AddSeconds(_timeout);
            Thread t = new Thread(() =>
            {
                CultureInfo.CurrentCulture = new CultureInfo("en-Us", false);
                while (!_terminate)
                {
                    Thread.Sleep(100);
                    if (_timeout < int.MaxValue && DateTime.Now >= endTime)
                    {
                        JsonHelper.WriteOutput("Message", string.Format("Timeout ({0} seconds) reached. Kill process hard now", _timeout), JsonOutput);
                        Environment.Exit(-8);
                    }
                }                          

                _engine.Stop();

                //Output all output values
                foreach (string output in _outputValues)
                {
                    if (output != null)
                    {
                        JsonHelper.WriteOutput("Output", output, JsonOutput);
                    }
                }                

                if (_verbose)
                {                    
                    JsonHelper.WriteOutput("Message", "ExecutionEngine stopped.Terminate now", JsonOutput);
                    JsonHelper.WriteOutput("Message", string.Format("Total execution took: {0}", DateTime.Now - _startTime), JsonOutput);
                }
                Environment.Exit(0);
            });
            t.Start();
        }

        /// <summary>
        /// Handles the case we have arguments instead of a json file
        /// </summary>
        /// <param name="args"></param>
        private void GetArgumentValues(string[] args)
        {
            //Step 2: Get cwm_file to open
            _cwm_file = ArgsHelper.GetCWMFileName(args);
            if (_cwm_file == null)
            {                
                JsonHelper.WriteOutput("Exception", "Please specify a cwm file using -cwm=filename", JsonOutput);
                Environment.Exit(-1);
            }
            if (!File.Exists(_cwm_file))
            {                
                JsonHelper.WriteOutput("Exception", string.Format("Specified cwm file \"{0}\" does not exist", _cwm_file), JsonOutput);
                Environment.Exit(-2);
            }

            //Step 3: Get additional parameters
            _verbose = ArgsHelper.CheckVerboseMode(args);
            try
            {
                _timeout = ArgsHelper.GetTimeout(args);
                //check, if timeout <=0
                if (_timeout <= 0)
                {
                    JsonHelper.WriteOutput("Exception", "Timeout must be greater than 0", JsonOutput);
                    Environment.Exit(-2);
                }
            }
            catch (Exception ex)
            {                
                JsonHelper.WriteOutput("Exception", string.Format("Exception occured while parsing timeout: {0}", ex.Message), JsonOutput);
                Environment.Exit(-2);
            }            

            try
            {
                _loglevel = ArgsHelper.GetLoglevel(args);
            }
            catch (Exception ex)
            {
                JsonHelper.WriteOutput("Exception", string.Format("Exception occured while parsing loglevel: {0}", ex.Message), JsonOutput);
                Environment.Exit(-2);
            }
            try
            {
                JsonOutput = ArgsHelper.CheckJsonOutput(args);
            }
            catch (Exception ex)
            {
                JsonHelper.WriteOutput("Exception", string.Format("Exception occured while parsing jsonoutput: {0}", ex.Message), JsonOutput);
                Environment.Exit(-2);
            }

            //Step 4: Check, if discover mode was selected
            if (ArgsHelper.CheckDiscoverMode(args))
            {
                DiscoverCWMFile(_cwm_file);
                Environment.Exit(0);
            }

            //Step 5: Get input parameters
            try
            {
                _inputParameters = ArgsHelper.GetInputParameters(args);
                if (_verbose)
                {
                    foreach (Parameter param in _inputParameters)
                    {                       
                        JsonHelper.WriteOutput("InputParameter", string.Format("Input parameter given: ", param), JsonOutput);
                    }
                }
            }
            catch (InvalidParameterException ipex)
            {
                JsonHelper.WriteOutput("Exception", ipex.Message, JsonOutput);
                Environment.Exit(-3);
            }
            catch (Exception ex)
            {                
                JsonHelper.WriteOutput("Exception", string.Format("Exception occured while parsing parameters: {0}", ex.Message), JsonOutput);
                Environment.Exit(-3);
            }

            //Step 6: Get settings
            try
            {
                _settings = ArgsHelper.GetSettings(args);
                if (_verbose)
                {
                    foreach (Setting setting in _settings)
                    {
                        JsonHelper.WriteOutput("Setting", string.Format("Setting given: {0}", setting), JsonOutput);
                    }
                }
            }
            catch (InvalidParameterException ipex)
            {
                Console.WriteLine(ipex.Message);
                Environment.Exit(-3);
            }
            catch (Exception ex)
            {                
                JsonHelper.WriteOutput("Exception", string.Format("Exception occured while parsing settings: {0}", ex.Message), JsonOutput);
                Environment.Exit(-3);
            }

            //Step 7: Get output parameters
            try
            {
                _outputParameters = ArgsHelper.GetOutputParameters(args);
                if (_verbose)
                {
                    foreach (Parameter param in _outputParameters)
                    {
                        JsonHelper.WriteOutput("OutputParameter", string.Format("Output parameter given: {0}", param), JsonOutput);
                    }
                }
            }
            catch (InvalidParameterException ipex)
            {
                JsonHelper.WriteOutput("Exception", ipex.Message, JsonOutput);
                Environment.Exit(-3);
            }
            catch (Exception ex)
            {
                JsonHelper.WriteOutput("Exception", string.Format("Exception occured while parsing parameters: {0}", ex.Message), JsonOutput);
                Environment.Exit(-3);
            }
        }

        /// <summary>
        /// Parses the json file and sets the values defined in the file similiar to GetArgumentValues does with argument values
        /// </summary>
        /// <param name="jsonfile"></param>
        private void ParseJsonInputFile(string jsonfile)
        {
            if (!File.Exists(jsonfile))
            {
                JsonHelper.WriteOutput("Exception", string.Format("Specified json file \"{0}\" does not exist", jsonfile), JsonOutput);
                Environment.Exit(-2);
            }

            JsonInput jsonInput = null;
            try
            {
                jsonInput = JsonHelper.ParseAndValidateJsonInput(jsonfile);
            }
            catch (Exception ex)
            {
                JsonHelper.WriteOutput("Exception", string.Format("Exception occured while parsing json file: {0}", ex.Message), JsonOutput);   
                Environment.Exit(-3);
            }

            _verbose = jsonInput.Verbose;
            _timeout = jsonInput.Timeout;
            _loglevel = jsonInput.Loglevel;
            JsonOutput = jsonInput.JsonOutput;
            _cwm_file = jsonInput.CwmFile;
            _settings = jsonInput.Settings;
            _inputParameters = jsonInput.InputParameters;
            _outputParameters = jsonInput.OutputParameters;
        }

        /// <summary>
        /// Changes the setting of a CrypTool component to the given value
        /// </summary>
        /// <param name="setting"></param>
        private void ChangeSetting(Setting setting)
        {
            bool found = false;
            foreach (PluginModel component in _workspaceModel.GetAllPluginModels())
            {
                if (component.GetName().ToLower().Equals(setting.ComponentName.ToLower()))
                {
                    found = true;
                    ISettings plugin_settings = component.Plugin.Settings;
                    PropertyInfo property = plugin_settings.GetType().GetProperty(setting.SettingName);
                    if (property == null)
                    {
                        JsonHelper.WriteOutput("Exception", string.Format("Setting \"{0}\" not found in component \"{1}\"", setting.SettingName, setting.ComponentName), JsonOutput);   
                        Environment.Exit(-7);
                    }

                    //handle case setting is text
                    if (property.PropertyType.FullName.Equals("System.String"))
                    {
                        property.SetValue(plugin_settings, setting.Value);
                        return;
                    }

                    //handle case setting is number
                    if (property.PropertyType.FullName.Equals("System.Int32"))
                    {
                        try
                        {
                            int value = int.Parse(setting.Value);
                            property.SetValue(plugin_settings, value);
                            return;
                        }
                        catch (Exception ex)
                        {
                            JsonHelper.WriteOutput("Exception", string.Format("Exception occured while parsing value \"{0}\" to int: {1}", setting.Value, ex.Message), JsonOutput); 
                            Environment.Exit(-7);
                        }
                    }
                   
                    //handle case setting is long
                    if (property.PropertyType.FullName.Equals("System.Int64"))
                    {
                        try
                        {
                            long value = long.Parse(setting.Value);
                            property.SetValue(plugin_settings, value);
                            return;
                        }
                        catch (Exception ex)
                        {
                            JsonHelper.WriteOutput("Exception", string.Format("Exception occured while parsing value \"{0}\" to long: {1}", setting.Value, ex.Message), JsonOutput);
                            Environment.Exit(-7);
                        }
                    }

                    //handle case setting is float
                    if (property.PropertyType.FullName.Equals("System.Single"))
                    {
                        try
                        {
                            float value = float.Parse(setting.Value);
                            property.SetValue(plugin_settings, value);
                            return;
                        }
                        catch (Exception ex)
                        {
                            JsonHelper.WriteOutput("Exception", string.Format("Exception occured while parsing value \"{0}\" to float: {1}", setting.Value, ex.Message), JsonOutput);
                            Environment.Exit(-7);
                        }
                    }

                    //handle case setting is double
                    if (property.PropertyType.FullName.Equals("System.Double"))
                    {
                        try
                        {
                            double value = double.Parse(setting.Value);
                            property.SetValue(plugin_settings, value);
                            return;
                        }
                        catch (Exception ex)
                        {
                            //write using JsonHelper.WriteOutput
                            JsonHelper.WriteOutput("Exception", string.Format("Exception occured while parsing value \"{0}\" to double: {1}", setting.Value, ex.Message), JsonOutput);      
                            Environment.Exit(-7);
                        }
                    }


                    //handle case setting is bool
                    if (property.PropertyType.FullName.Equals("System.Boolean"))
                    {
                        try
                        {
                            bool value = bool.Parse(setting.Value);
                            property.SetValue(plugin_settings, value);
                            return;
                        }
                        catch (Exception ex)
                        {
                            JsonHelper.WriteOutput("Exception", string.Format("Exception occured while parsing value \"{0}\" to bool: {1}", setting.Value, ex.Message), JsonOutput);    
                            Environment.Exit(-7);
                        }
                    }
                    
                    //handle case setting is enum
                    if (property.PropertyType.IsEnum)
                    {
                        try
                        {
                            object value = Enum.Parse(property.PropertyType, setting.Value);
                            property.SetValue(plugin_settings, value);
                            return;
                        }
                        catch (Exception ex)
                        {
                            JsonHelper.WriteOutput("Exception", string.Format("Exception occured while parsing value \"{0}\" to enum: {1}", setting.Value, ex.Message), JsonOutput);    
                            Environment.Exit(-7);
                        }
                    }
                }
            }

            if(!found)
            {
                JsonHelper.WriteOutput("Exception", string.Format("Component \"{0}\" not found", setting.ComponentName), JsonOutput);       
                Environment.Exit(-7);
            }
        }

        /// <summary>
        /// This method analyses a given cwm file and returns all parameters
        /// </summary>
        /// <param name="cwm_file"></param>
        private void DiscoverCWMFile(string cwm_file)
        {
            try
            {
                UpdateAppDomain();
            }
            catch (Exception ex)
            {
                JsonHelper.WriteOutput("Exception", string.Format("Exception occured while updating AppDomain: {0}", ex.Message), JsonOutput);
                Environment.Exit(-4);
            }

            ModelPersistance modelPersistance = new ModelPersistance();
            try
            {
                _workspaceModel = modelPersistance.loadModel(cwm_file, true);
            }
            catch (Exception ex)
            {
                JsonHelper.WriteOutput("Exception", string.Format("Exception occured during loading of cwm file: {0}", ex.Message), JsonOutput);
                Environment.Exit(0);
            }
            DiscoverWorkspaceModel(cwm_file);
        }

        /// <summary>
        /// Discovers the workspace model and outputs the results to console
        /// </summary>
        /// <param name="cwm_file"></param>
        private void DiscoverWorkspaceModel(string cwm_file)
        {
            if (JsonOutput)
            {
                Console.Write("{\"components\":[");
            }
            else
            {
                Console.WriteLine("Discovery of cwm_file \"{0}\"", cwm_file);
                Console.WriteLine();
            }
            int counter = 0;
            System.Collections.ObjectModel.ReadOnlyCollection<PluginModel> allPluginModels = _workspaceModel.GetAllPluginModels();
            foreach (PluginModel pluginModel in allPluginModels)
            {
                counter++;
                if (!JsonOutput)
                {
                    Console.WriteLine("\"{0}\" (\"{1}\")", pluginModel.GetName(), pluginModel.Plugin.GetType().FullName);
                }

                System.Collections.ObjectModel.ReadOnlyCollection<ConnectorModel> inputs = pluginModel.GetInputConnectors();
                System.Collections.ObjectModel.ReadOnlyCollection<ConnectorModel> outputs = pluginModel.GetOutputConnectors();
                ISettings settings = pluginModel.Plugin.Settings;
                TaskPaneAttribute[] taskPaneAttributes = settings.GetSettingsProperties(pluginModel.Plugin);

                if (JsonOutput)
                {
                    Console.Write("{0}", JsonHelper.GetPluginDiscoveryString(pluginModel, inputs, outputs, taskPaneAttributes));
                    if (counter < allPluginModels.Count)
                    {
                        Console.Write(",");
                    }
                    continue;
                }
                if (inputs.Count > 0)
                {
                    Console.WriteLine("- Input connectors:");
                    foreach (ConnectorModel input in inputs)
                    {
                        Console.WriteLine("-- \"{0}\" (\"{1}\")", input.GetName(), input.ConnectorType.FullName);
                    }
                }
                if (outputs.Count > 0)
                {
                    Console.WriteLine("- Output connectors:");
                    foreach (ConnectorModel output in outputs)
                    {
                        Console.WriteLine("-- \"{0}\" (\"{1}\")", output.GetName(), output.ConnectorType.FullName);
                    }
                }
                if (taskPaneAttributes != null && taskPaneAttributes.Length > 0)
                {
                    Console.WriteLine("- Settings:");
                    foreach (TaskPaneAttribute taskPaneAttribute in taskPaneAttributes)
                    {
                        Console.WriteLine("-- \"{0}\" (\"{1}\")", taskPaneAttribute.PropertyName, taskPaneAttribute.PropertyInfo.PropertyType.FullName);
                        if (taskPaneAttribute.PropertyInfo.PropertyType.IsEnum)
                        {
                            Console.WriteLine("--- Possible values:");
                            foreach (object value in Enum.GetValues(taskPaneAttribute.PropertyInfo.PropertyType))
                            {
                                Console.WriteLine("---> \"{0}\"", value.ToString());
                            }
                        }
                    }
                }
                Console.WriteLine();
            }
            if (JsonOutput)
            {
                Console.Write("]}");
            }
        }

        /// <summary>
        /// Called, when progress on a single plugin changed        
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnPluginProgressChanged(IPlugin sender, PluginProgressEventArgs args)
        {           
            HandleGlobalProgress(sender, args);                        
        }

        /// <summary>
        /// Handles the global progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HandleGlobalProgress(IPlugin sender, PluginProgressEventArgs args)
        {
            lock (_progressLockObject)
            {
                if (!_pluginProgressValues.ContainsKey(sender))
                {
                    _pluginProgressValues.Add(sender, args.Value / args.Max);
                }
                else
                {
                    double newValue = args.Value / args.Max;
                    if (newValue > _pluginProgressValues[sender])
                    {
                        _pluginProgressValues[sender] = newValue;
                    }
                }
                double numberOfPlugins = _workspaceModel.GetAllPluginModels().Count;
                double totalProgress = 0;
                foreach (double value in _pluginProgressValues.Values)
                {
                    totalProgress += value;
                }
                if (!_terminate && totalProgress == numberOfPlugins)
                {                   
                    _terminate = true;
                }
                int newProgress = (int)(totalProgress / numberOfPlugins * 100);
                if (newProgress > _globalProgress)
                {
                    _globalProgress = newProgress;
                    if (_verbose)
                    {
                        JsonHelper.WriteOutput("Message", string.Format("Progress:{0}", _globalProgress), JsonOutput);
                    }
                }
            }
        }

        /// <summary>
        /// Property changed on plugin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pceargs"></param>
        private void Plugin_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs pceargs)
        {
            IPlugin plugin = (IPlugin)sender;
            PropertyInfo property = sender.GetType().GetProperty(pceargs.PropertyName);
            if (!property.Name.ToLower().Equals("input"))
            {
                return;
            }
            string output = string.Format("{0}={1}", _pluginNames[plugin], property.GetValue(plugin).ToString());
            
            object value = property.GetValue(plugin);
            if (value != null && _verbose)
            {
                JsonHelper.WriteOutput("Output", output, JsonOutput);
            }
            
            _outputValues.Add(output);
        }

        /// <summary>
        /// Logs guilog to console based on error level and verbosity
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnGuiLogNotificationOccured(IPlugin sender, GuiLogEventArgs args)
        {
            if (args.NotificationLevel < _loglevel)
            {
                return;
            }
            if (_verbose)
            {                
                JsonHelper.WriteOutput("GuiLog", string.Format("{0}:{1}:{2}", args.NotificationLevel, (sender != null ? sender.GetType().Name : "null"), args.Message), JsonOutput);
            }            
        }

        /// <summary>
        /// Updates app domain with user defined assembly resolver routine
        /// </summary>
        private void UpdateAppDomain()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(LoadAssembly);
        }

        /// <summary>
        /// Loads assemblies defined by subfolders definition
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private Assembly LoadAssembly(object sender, ResolveEventArgs args)
        {
            string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (string subfolder in subfolders)
            {
                string assemblyPath = Path.Combine(folderPath, (Path.Combine(subfolder, new AssemblyName(args.Name).Name + ".dll")));
                if (File.Exists(assemblyPath))
                {
                    Assembly assembly = Assembly.LoadFrom(assemblyPath);
                    if (_verbose)
                    {
                        JsonHelper.WriteOutput("Message", string.Format("Loaded assembly:{0}", assemblyPath), JsonOutput);
                    }
                    return assembly;
                }
                assemblyPath = Path.Combine(folderPath, (Path.Combine(subfolder, new AssemblyName(args.Name).Name + ".exe")));
                if (File.Exists(assemblyPath))
                {
                    Assembly assembly = Assembly.LoadFrom(assemblyPath);
                    if (_verbose)
                    {
                        JsonHelper.WriteOutput("Message", string.Format("Loaded assembly:{0}", assemblyPath), JsonOutput);
                    }
                    return assembly;
                }
            }
            return null;
        }
    }
}
