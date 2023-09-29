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
using CrypTool.PluginBase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CrypTool.CrypWin
{
    [SerializableAttribute]
    public class PluginInformation
    {
        public string Pluginname { get; set; }
        public string Assemblyname { get; set; }
        public string Plugindescription { get; set; }
        public bool Disabled { get; set; }
    }

    public static class PluginList
    {
        private static readonly ObservableCollection<PluginInformation> allPlugins = new ObservableCollection<PluginInformation>();

        public static ObservableCollection<PluginInformation> AllPlugins => allPlugins;

        private static List<PluginInformation> allTempPlugins = new List<PluginInformation>();

        public static void AddDisabledPluginsToPluginList(ArrayList disabledPlugins)
        {
            if (disabledPlugins != null)
            {
                foreach (PluginInformation plugin in disabledPlugins)
                {
                    allTempPlugins.Add(plugin);
                }
            }
        }

        public static void AddTypeToPluginList(Type pluginType)
        {
            PluginInfoAttribute info = pluginType.GetPluginInfoAttribute();
            if (info != null)
            {
                allTempPlugins.Add(new PluginInformation() { Pluginname = info.Caption, Plugindescription = info.ToolTip, Assemblyname = pluginType.Assembly.ManifestModule.Name, Disabled = false });
            }
        }

        public static void Finished()
        {
            allTempPlugins.Sort((x, y) => x.Pluginname.CompareTo(y.Pluginname));
            foreach (PluginInformation p in allTempPlugins)
            {
                allPlugins.Add(p);
            }
            allTempPlugins = null;
        }
    }
}
