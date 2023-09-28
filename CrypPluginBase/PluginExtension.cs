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
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.Editor;
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.PluginBase.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CrypTool.PluginBase
{
    public static class PluginExtension
    {
        public static bool IsTestMode { get; set; }

        public static event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private static void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, null, new GuiLogEventArgs(message, null, logLevel));
        }

        /// <summary>
        /// Gets the properties marked with the PropertyInfoAttribute.
        /// </summary>
        /// <param name="pluginType">The plugin type.</param>
        /// <returns></returns>
        public static PropertyInfoAttribute[] GetProperties(Type pluginType)
        {
            List<PropertyInfoAttribute> propertyInfos = new List<PropertyInfoAttribute>();
            foreach (PropertyInfo pInfo in pluginType.GetProperties())
            {
                PropertyInfoAttribute[] attributes = (PropertyInfoAttribute[])pInfo.GetCustomAttributes(typeof(PropertyInfoAttribute), false);
                if (attributes.Length == 1)
                {
                    PropertyInfoAttribute attr = attributes[0];
                    attr.PropertyName = pInfo.Name;
                    attr.PluginType = pluginType;
                    attr.PropertyInfo = pInfo;
                    propertyInfos.Add(attr);
                }
            }
            return propertyInfos.ToArray();
        }

        /// <summary>
        /// Gets the properties marked with the PropertyInfoAttribute.
        /// </summary>
        /// <param name="plugin">The plugin.</param>
        /// <returns></returns>
        public static PropertyInfoAttribute[] GetProperties(this IPlugin plugin)
        {
            return GetProperties(plugin.GetType());
        }

        /// <summary>
        /// Gets the properties marked with the PropertyInfoAttribute that match given direction.
        /// </summary>
        /// <param name="plugin">The plugin.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        public static PropertyInfoAttribute[] GetProperties(this IPlugin plugin, Direction direction)
        {
            List<PropertyInfoAttribute> list = new List<PropertyInfoAttribute>();
            foreach (PropertyInfoAttribute pInfo in plugin.GetProperties())
            {
                if (pInfo.Direction == direction)
                {
                    list.Add(pInfo);
                }
            }
            return list.ToArray();
        }

        public static EventInfo GetTaskPaneAttributeChanged(this ISettings settings)
        {
            foreach (EventInfo eventInfo in settings.GetType().GetEvents())
            {
                if (eventInfo.EventHandlerType == typeof(TaskPaneAttributeChangedHandler))
                {
                    return eventInfo;
                }
            }
            return null;
        }

        public static void SetPropertyValue(this IPlugin plugin, PropertyInfo property, object value)
        {
            property.SetValue(plugin, value, null);
        }

        public static object GetPropertyValue(this IPlugin plugin, PropertyInfo property)
        {
            return property.GetValue(plugin, null);
        }

        public static ComponentCategoryAttribute[] GetComponentCategoryAttributes(this Type type)
        {
            return (ComponentCategoryAttribute[])type.GetCustomAttributes(typeof(ComponentCategoryAttribute), false);
        }

        public static PluginInfoAttribute GetPluginInfoAttribute(this IPlugin plugin)
        {
            return GetPluginInfoAttribute(plugin.GetType());
        }

        public static PluginInfoAttribute GetPluginInfoAttribute(this Type type)
        {
            PluginInfoAttribute[] attributes = (PluginInfoAttribute[])type.GetCustomAttributes(typeof(PluginInfoAttribute), false);
            if (attributes.Length == 1)
            {
                // if resource file is set - keys are used instead of values. resource access necessary 
                if (attributes[0].ResourceFile != null)
                {
                    attributes[0].PluginType = type;
                }
                return attributes[0];
            }
            return null;
        }

        public static AuthorAttribute GetPluginAuthorAttribute(this IPlugin plugin)
        {
            if (plugin != null)
            {
                return GetPluginAuthorAttribute(plugin.GetType());
            }

            return null;
        }

        public static bool GetAutoAssumeZeroBeginProgressAttributeValue(this IPlugin plugin)
        {
            if (plugin != null)
            {
                AutoAssumeZeroBeginProgressAttribute[] attributes = (AutoAssumeZeroBeginProgressAttribute[])
                                 plugin.GetType().GetCustomAttributes(typeof(AutoAssumeZeroBeginProgressAttribute), false);
                if (attributes.Length == 1)
                {
                    return attributes[0].AutoProgressChanged;
                }
            }
            return true;
        }

        public static bool GetAutoAssumeFullEndProgressAttributeValue(this IPlugin plugin)
        {
            if (plugin != null)
            {
                AutoAssumeFullEndProgressAttribute[] attributes = (AutoAssumeFullEndProgressAttribute[])
                                 plugin.GetType().GetCustomAttributes(typeof(AutoAssumeFullEndProgressAttribute), false);
                if (attributes.Length == 1)
                {
                    return attributes[0].AutoProgressChanged;
                }
            }
            return true;
        }

        public static AuthorAttribute GetPluginAuthorAttribute(this Type type)
        {
            if (type == null)
            {
                return null;
            }

            AuthorAttribute[] attributes = (AuthorAttribute[])type.GetCustomAttributes(typeof(AuthorAttribute), false);
            if (attributes.Length == 1)
            {
                return attributes[0];
            }
            else
            {
                return null;
            }
        }

        public static TaskPaneAttribute[] GetSettingsProperties(this ISettings settings, IPlugin plugin)
        {
            if (settings == null || plugin == null)
            {
                return new TaskPaneAttribute[0];
            }

            return GetSettingsProperties(settings.GetType(), plugin);
        }

        public static TaskPaneAttribute[] GetSettingsProperties(this Type type, IPlugin plugin)
        {
            if (type == null || plugin == null)
            {
                return new TaskPaneAttribute[0];
            }

            return type.GetSettingsProperties(plugin.GetType());
        }

        public static TaskPaneAttribute[] GetSettingsProperties(this Type type, Type pluginType)
        {
            if (type == null || pluginType == null)
            {
                return new TaskPaneAttribute[0];
            }

            try
            {
                List<TaskPaneAttribute> taskPaneAttributes = new List<TaskPaneAttribute>();
                foreach (PropertyInfo pInfo in type.GetProperties())
                {
                    TaskPaneAttribute[] attributes = (TaskPaneAttribute[])pInfo.GetCustomAttributes(typeof(TaskPaneAttribute), false);
                    if (attributes != null && attributes.Length == 1)
                    {
                        TaskPaneAttribute attr = attributes[0];
                        attr.PropertyInfo = pInfo;
                        attr.PropertyName = pInfo.Name;
                        // does plugin have a resource file for translation?
                        if (pluginType.GetPluginInfoAttribute().ResourceFile != null)
                        {
                            attr.PluginType = pluginType;
                        }

                        taskPaneAttributes.Add(attr);
                    }
                }

                foreach (MethodInfo mInfo in type.GetMethods())
                {
                    if (mInfo.IsPublic && mInfo.GetParameters().Length == 0)
                    {
                        TaskPaneAttribute[] attributes = (TaskPaneAttribute[])mInfo.GetCustomAttributes(typeof(TaskPaneAttribute), false);
                        if (attributes != null && attributes.Length == 1)
                        {
                            TaskPaneAttribute attr = attributes[0];
                            attr.Method = mInfo;
                            attr.MethodInfo = mInfo;
                            attr.PropertyName = mInfo.Name;
                            // does plugin have a resource file for translation?
                            if (pluginType.GetPluginInfoAttribute().ResourceFile != null)
                            {
                                attr.PluginType = pluginType;
                            }

                            taskPaneAttributes.Add(attr);
                        }
                    }
                }

                return taskPaneAttributes.ToArray();
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
            return null;
        }

        public static RibbonBarAttribute[] GetRibbonBarSettingsProperties(this ISettings settings, IPlugin plugin)
        {
            return GetRibbonBarSettingsProperties(settings.GetType(), plugin);
        }

        public static RibbonBarAttribute[] GetRibbonBarSettingsProperties(this Type type, IPlugin plugin)
        {
            try
            {
                List<RibbonBarAttribute> taskPaneAttributes = new List<RibbonBarAttribute>();
                foreach (PropertyInfo pInfo in type.GetProperties())
                {
                    RibbonBarAttribute[] attributes = (RibbonBarAttribute[])pInfo.GetCustomAttributes(typeof(RibbonBarAttribute), false);
                    if (attributes != null && attributes.Length == 1)
                    {
                        RibbonBarAttribute attr = attributes[0];
                        attr.PropertyName = pInfo.Name;
                        // does plugin have a resource file for translation?
                        if (plugin.GetType().GetPluginInfoAttribute().ResourceFile != null)
                        {
                            attr.PluginType = plugin.GetType();
                        }

                        taskPaneAttributes.Add(attr);
                    }
                }

                foreach (MethodInfo mInfo in type.GetMethods())
                {
                    if (mInfo.IsPublic && mInfo.GetParameters().Length == 0)
                    {
                        RibbonBarAttribute[] attributes = (RibbonBarAttribute[])mInfo.GetCustomAttributes(typeof(RibbonBarAttribute), false);
                        if (attributes != null && attributes.Length == 1)
                        {
                            RibbonBarAttribute attr = attributes[0];
                            attr.Method = mInfo;
                            attr.PropertyName = mInfo.Name;
                            // does plugin have a resource file for translation?
                            if (plugin.GetType().GetPluginInfoAttribute().ResourceFile != null)
                            {
                                attr.PluginType = plugin.GetType();
                            }

                            taskPaneAttributes.Add(attr);
                        }
                    }
                }

                return taskPaneAttributes.ToArray();
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
            return null;
        }

        public static SettingsFormatAttribute GetSettingsFormat(this ISettings settings, string propertyName)
        {
            if (settings == null || string.IsNullOrEmpty(propertyName))
            {
                return null;
            }

            return GetSettingsFormat(settings.GetType(), propertyName);
        }

        public static SettingsFormatAttribute GetSettingsFormat(this Type type, string propertyName)
        {
            if (type == null || string.IsNullOrEmpty(propertyName))
            {
                return null;
            }

            try
            {
                if (type.GetProperty(propertyName) != null)
                {
                    SettingsFormatAttribute[] settingsFormat = (SettingsFormatAttribute[])type.GetProperty(propertyName).GetCustomAttributes(typeof(SettingsFormatAttribute), false);
                    if (settingsFormat != null && settingsFormat.Length == 1)
                    {
                        return settingsFormat[0];
                    }
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
            return null;
        }

        public static Image GetImage(this IPlugin plugin, int index)
        {
            return GetImage(plugin.GetType(), index);
        }

        public static Image GetImage(this Type type, int index, int maxWidth = -1, int maxHeight = -1)
        {
            try
            {
                return GetImageWithoutLogMessage(type, index, maxWidth, maxHeight);
            }
            catch (Exception exception)
            {
                if (type != null)
                {
                    GuiLogMessage(string.Format(Resources.plugin_extension_error_get_image, new object[] { type.Name, exception.Message }), NotificationLevel.Error);
                }
                else
                {
                    GuiLogMessage(exception.Message, NotificationLevel.Error);
                }

                return null;
            }
        }

        public static Image GetImageWithoutLogMessage(this Type type, int index, int maxWidth = -1, int maxHeight = -1)
        {
            string icon = type.GetPluginInfoAttribute().Icons[index];
            int sIndex = icon.IndexOf('/');
            Image img = new Image
            {
                Source = BitmapFrame.Create(new Uri(string.Format("pack://application:,,,/{0};component/{1}", icon.Substring(0, sIndex), icon.Substring(sIndex + 1))))
            };
            if (maxWidth > 0)
            {
                img.Width = Math.Min(img.Source.Width, maxWidth);
            }

            if (maxHeight > 0)
            {
                img.Height = Math.Min(img.Source.Height, maxHeight);
            }

            return img;
        }

        public static string GetPluginStringResource(this IPlugin plugin, string keyword)
        {
            try
            {
                return plugin.GetType().GetPluginStringResource(keyword);
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
                return null;
            }
        }

        public static string GetPluginStringResource(this Type type, string keyword)
        {
            return GetPluginStringResource(type, keyword, null);
        }

        public static string GetPluginStringResource(this Type type, string keyword, CultureInfo culture)
        {
            try
            {
                // Get resource file from plugin assembly -> <Namespace>.<ResourceFileName> without "resx" file extension
                ResourceManager resman = new ResourceManager(type.GetPluginInfoAttribute().ResourceFile, type.Assembly);

                string[] resources = type.Assembly.GetManifestResourceNames();

                // Load the translation for the keyword
                string translation;
                if (culture == null)
                {
                    translation = resman.GetString(keyword);
                }
                else
                {
                    translation = resman.GetString(keyword, culture);
                }
                if (translation != null)
                {
                    return translation;
                }
                else
                {
                    if (IsTestMode)
                    {
                        GuiLogMessage(string.Format(Resources.Can_t_find_localization_key, keyword, type), NotificationLevel.Warning);
                    }
                    return keyword;
                }
            }
            catch (Exception ex)
            {
                if (IsTestMode)
                {
                    GuiLogMessage(string.Format(Resources.Error_trying_to_lookup_localization_key, keyword, ex.Message), NotificationLevel.Warning);
                }
                return keyword;
            }
        }

        public static ICrypComponent CreateComponentInstance(this Type type)
        {
            if (type.GetInterface(typeof(ICrypComponent).Name) != null)
            {
                try
                {
                    return (ICrypComponent)Activator.CreateInstance(type);
                }
                catch (Exception exception)
                {
                    GuiLogMessage(string.Format(Resources.plugin_extension_create_object, new object[] { type.Name, exception.Message }), NotificationLevel.Error);
                    return null;
                }
            }
            return null;
        }

        public static IEditor CreateEditorInstance(this Type type)
        {
            if (type.GetInterface(typeof(IEditor).Name) != null)
            {
                try
                {
                    return (IEditor)Activator.CreateInstance(type);
                }
                catch (Exception exception)
                {
                    GuiLogMessage(string.Format(Resources.plugin_extension_create_object, new object[] { type.Name, exception.Message }), NotificationLevel.Error);
                    return null;
                }
            }
            return null;
        }

        public static ICrypTutorial CreateTutorialInstance(this Type type)
        {
            if (type.GetInterface(typeof(ICrypTutorial).Name) != null)
            {
                try
                {
                    return (ICrypTutorial)Activator.CreateInstance(type);
                }
                catch (Exception exception)
                {
                    GuiLogMessage(string.Format(Resources.plugin_extension_create_object, new object[] { type.Name, exception.Message }), NotificationLevel.Error);
                    return null;
                }
            }
            return null;
        }
    }
}
