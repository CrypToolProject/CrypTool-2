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
using CrypTool.PluginBase.Attributes;
using System;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CrypTool.CrypWin
{
    /// <summary>
    /// Interaction logic for SettingsPresentation.xaml
    /// </summary>
    [TabColor("gray")]
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.CrypWin.Properties.Resources")]
    public partial class SettingsPresentation : UserControl
    {
        private static SettingsPresentation singleton = null;
        private Style settingsStyle;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private SettingsPresentation()
        {
            InitializeComponent();
            CreateSettingsStyle();

            var allSettingsTabs =
                from a in AppDomain.CurrentDomain.GetAssemblies()
                let types = GetTypesSafely(a)
                where types != null
                from t in types
                let attributes = t.GetCustomAttributes(typeof(SettingsTabAttribute), true)
                where attributes != null && attributes.Length > 0
                select new { Type = t, Attributes = attributes.Cast<SettingsTabAttribute>() };

            AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(CurrentDomain_AssemblyLoad);

            foreach (var tab in allSettingsTabs)
            {
                RegisterType(tab.Type);
            }

            Tag = FindResource("ctLogo");
        }

        private Type[] GetTypesSafely(Assembly a)
        {
            try
            {
                return a.GetTypes();
            }
            catch (Exception e)
            {
                GuiLogMessage(e.Message, NotificationLevel.Error);
                return null;
            }
        }

        private void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            Type[] types = GetTypesSafely(args.LoadedAssembly);
            if (types == null)
            {
                return;
            }

            var allSettingsTabsInNewAssembly =
                from t in types
                let attributes = t.GetCustomAttributes(typeof(SettingsTabAttribute), true)
                where attributes != null && attributes.Length > 0
                select new { Type = t, Attributes = attributes.Cast<SettingsTabAttribute>() };

            foreach (var tab in allSettingsTabsInNewAssembly)
            {
                RegisterType(tab.Type);
            }
        }

        private void RegisterType(Type tab)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(delegate
                                                             {
                                                                 try
                                                                 {
                                                                     ConstructorInfo constructor = tab.GetConstructor(new Type[] { typeof(Style) });
                                                                     if (constructor != null)
                                                                     {
                                                                         Control t = (Control)constructor.Invoke(new object[] { settingsStyle });
                                                                         RegisterSettingsTab(t);
                                                                     }
                                                                 }
                                                                 catch (Exception ex)
                                                                 {
                                                                     GuiLogMessage(string.Format("Registering settings tab {0} failed: {1}", tab.Name, ex.Message), NotificationLevel.Error);
                                                                 }
                                                             }), null);

        }

        private void SetStyleToControl(Visual control)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(control); i++)
            {
                Visual childVisual = (Visual)VisualTreeHelper.GetChild(control, i);
                if (childVisual is FrameworkElement)
                {
                    (childVisual as FrameworkElement).Style = settingsStyle;
                }
                SetStyleToControl(childVisual);
            }
        }

        private void CreateSettingsStyle()
        {
            settingsStyle = new Style();
            Thickness margin = new Thickness(30, 5, 0, 10);
            Setter marginSetter = new Setter(FrameworkElement.MarginProperty, margin);
            settingsStyle.Setters.Add(marginSetter);
        }

        public static SettingsPresentation GetSingleton()
        {
            if (singleton == null)
            {
                singleton = new SettingsPresentation();
            }
            return singleton;
        }

        private void RegisterSettingsTab(Control tab)
        {
            SettingsTabAttribute settingsTabAttribute = (SettingsTabAttribute)Attribute.GetCustomAttribute(tab.GetType(), typeof(SettingsTabAttribute));
            LocalizationAttribute localitationAttribute = (LocalizationAttribute)Attribute.GetCustomAttribute(tab.GetType(), typeof(LocalizationAttribute));
            ResourceManager resman = new ResourceManager(localitationAttribute.ResourceClassPath, tab.GetType().Assembly);

            TreeViewItem i = GetTreeViewItemFromAddress(settingsTree.Items, settingsTabAttribute.Address + settingsTabAttribute.Caption + "/", settingsTabAttribute.Priority);
            i.Header = resman.GetString(settingsTabAttribute.Caption);
            i.Selected += new RoutedEventHandler(delegate
                                                     {
                                                         if (settingsTree.SelectedItem == i)
                                                         {
                                                             settingsTab.Content = tab;
                                                         }
                                                     });

            if (settingsTree.Items.IndexOf(i) == 0)
            {
                i.IsSelected = true;
            }
        }

        private TreeViewItem GetTreeViewItemFromAddress(ItemCollection items, string address, double priority)
        {
            string[] split = address.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string name = split[0];
            string remainingAddress = address.Substring(address.IndexOf('/', 1));
            foreach (object item in items)
            {
                if (item is TreeViewItem)
                {
                    if (((TreeViewItem)item).Name == name)
                    {
                        if (split.Count() > 1)
                        {
                            return GetTreeViewItemFromAddress(((TreeViewItem)item).Items, remainingAddress, priority);
                        }
                        else
                        {
                            //adjust this entry to its right position (because it didn't happen already):
                            ((TreeViewItem)item).Tag = priority;
                            items.Remove(item);
                            int i = 0;
                            while (i < items.Count && (double)((TreeViewItem)items[i]).Tag > priority)
                            {
                                i++;
                            }

                            items.Insert(i, item);

                            return (TreeViewItem)item;
                        }
                    }
                }
            }

            TreeViewItem newItem = new TreeViewItem
            {
                Name = name,
                Header = name,  //temporary header
                Tag = priority
            };

            //search the right position (based on the priorities):
            int pos = 0;
            while (pos < items.Count && (double)((TreeViewItem)items[pos]).Tag > priority)
            {
                pos++;
            }
            items.Insert(pos, newItem);

            if (split.Count() > 1)
            {
                return GetTreeViewItemFromAddress(newItem.Items, remainingAddress, priority);
            }
            else
            {
                return newItem;
            }
        }

        public void GuiLogMessage(string message, NotificationLevel loglevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(null, new GuiLogEventArgs(message, null, loglevel));
            }
        }
    }
}
