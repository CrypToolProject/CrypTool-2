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
using CrypTool.PluginBase.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CrypTool.PluginBase
{
    /// <summary>
    /// This class is used to maintain global informations about all loaded components.
    /// It is necessary, so that components get to know each other if necessary.
    /// </summary>
    public static class ComponentInformations
    {
        /// <summary>
        /// This dictionary maps types IControlCost, IControlEnryption, IP2PControl and IControlCubeAttack
        /// to a list of plugin, which uses them in slave mode.
        /// </summary>
        public static readonly Dictionary<Type, List<Type>> PluginsWithSpecificController = new Dictionary<Type, List<Type>>();

        /// <summary>
        /// This dictionary maps types names of all loaded plugins to their types.
        /// </summary>
        public static readonly Dictionary<string, Type> AllLoadedPlugins = new Dictionary<string, Type>();

        /// <summary>
        /// This dictionary maps editor extensions to the corresponding editor type.
        /// </summary>
        public static Dictionary<string, Type> EditorExtension = new Dictionary<string, Type>();

        public static void AddPlugin(Type pluginType)
        {
            try
            {
                AllLoadedPlugins.Add(pluginType.FullName, pluginType);

                foreach (PropertyInfo pInfo in pluginType.GetProperties())
                {
                    Type propertyType = pInfo.PropertyType;
                    PropertyInfoAttribute[] attributes = (PropertyInfoAttribute[])pInfo.GetCustomAttributes(typeof(PropertyInfoAttribute), false);
                    if (attributes.Length == 1 && attributes[0].Direction == Direction.ControlSlave)
                    {
                        if (propertyType.IsInterface && propertyType.GetInterfaces().Contains(typeof(IControl)))
                        {
                            if (PluginsWithSpecificController.ContainsKey(propertyType))
                            {
                                PluginsWithSpecificController[propertyType].Add(pluginType);
                            }
                            else
                            {
                                PluginsWithSpecificController.Add(propertyType, new List<Type>() { pluginType });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }
    }
}
