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
using CrypTool.PluginBase.Properties;
using System;
using System.Resources;
using System.Windows.Markup;
using System.Xaml;

// Register the extention in the Microsoft's default namespaces
[assembly: System.Windows.Markup.XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "CrypTool.PluginBase.Miscellaneous")]
[assembly: System.Windows.Markup.XmlnsDefinition("http://schemas.microsoft.com/winfx/2007/xaml/presentation", "CrypTool.PluginBase.Miscellaneous")]
[assembly: System.Windows.Markup.XmlnsDefinition("http://schemas.microsoft.com/winfx/2008/xaml/presentation", "CrypTool.PluginBase.Miscellaneous")]

namespace CrypTool.PluginBase.Miscellaneous
{
    [MarkupExtensionReturnType(typeof(object))]
    [ContentProperty("Key")]
    public class LocExtension : MarkupExtension
    {
        public delegate void GuiLogMessageHandler(string message, NotificationLevel logLevel);
        public static event GuiLogMessageHandler OnGuiLogMessageOccured;

        public static void GuiLogMessageOccured(string message, NotificationLevel loglevel)
        {
            if (OnGuiLogMessageOccured != null)
            {
                OnGuiLogMessageOccured(message, loglevel);
            }
        }

        public string Key { get; set; }

        public LocExtension(string key)
        {
            Key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            try
            {
                IRootObjectProvider service = serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;
                LocalizationAttribute locAttribute = (LocalizationAttribute)Attribute.GetCustomAttribute(service.RootObject.GetType(), typeof(LocalizationAttribute));
                ResourceManager resman = new ResourceManager(locAttribute.ResourceClassPath, service.RootObject.GetType().Assembly);

                if (resman.GetString(Key) != null)
                {
                    return resman.GetString(Key);
                }
                else
                {
                    GuiLogMessageOccured(string.Format(Resources.Can_t_find_localization_key, Key, service.RootObject.GetType()), NotificationLevel.Warning);
                    return Key;
                }
            }
            catch (Exception ex)
            {
                GuiLogMessageOccured(string.Format(Resources.Error_trying_to_lookup_localization_key, Key, ex.Message), NotificationLevel.Warning);
                return Key;
            }
        }

    }
}
