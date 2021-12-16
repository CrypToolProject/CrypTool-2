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
