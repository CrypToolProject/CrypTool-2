using System;
using System.Collections.Generic;
using System.Reflection;
using CrypTool.PluginBase.Control;
using System.Linq;

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
                    var propertyType = pInfo.PropertyType;
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
