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
using CrypTool.PluginBase;
using System.Collections.Generic;
using System.Reflection;

namespace UnitTests
{
    internal class PluginTestScenario : TestScenario
    {
        private readonly ICrypComponent _plugin;

        public PluginTestScenario(ICrypComponent plugin, string[] inputProperties, string[] outputProperties)
            : base(GetProperties(plugin, inputProperties), GetObjectArray(plugin, inputProperties),
                   GetProperties(plugin, outputProperties), GetObjectArray(plugin, outputProperties))
        {
            _plugin = plugin;
        }

        private static object[] GetObjectArray(ICrypComponent plugin, string[] properties)
        {
            ISettings settings = plugin.Settings;

            object[] res = new object[properties.Length];
            for (int i = 0; i < res.Length; i++)
            {
                if (properties[i].StartsWith("."))
                {
                    res[i] = settings;
                }
                else
                {
                    res[i] = plugin;
                }
            }
            return res;
        }

        private static PropertyInfo[] GetProperties(ICrypComponent plugin, string[] properties)
        {
            ISettings settings = plugin.Settings;

            List<PropertyInfo> res = new List<PropertyInfo>();
            foreach (string property in properties)
            {
                if (property.StartsWith("."))
                {
                    res.Add(settings.GetType().GetProperty(property.Substring(1)));
                }
                else
                {
                    res.Add(plugin.GetType().GetProperty(property));
                }
            }
            return res.ToArray();
        }

        protected override void Initialize()
        {
            _plugin.Initialize();
        }

        protected override void PreExecution()
        {
            _plugin.PreExecution();
        }

        protected override void Execute()
        {
            _plugin.Execute();
        }
    }
}