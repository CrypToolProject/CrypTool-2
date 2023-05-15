/*                              
   Copyright 2010 Nils Kopal

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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Xml;
using System.Xml.Linq;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using WorkspaceManagerModel.Properties;

namespace WorkspaceManager.Model
{
    /// <summary>
    /// Class with static methods for loading and saving of WorkspaceModels
    /// </summary>
    public class ModelPersistance
    {
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        /// <summary>
        /// Deserializes a model from the given file with the given filename
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public WorkspaceModel loadModel(string filename, bool handleTemplateReplacement = true)
        {
            PersistantModel persistantModel = (PersistantModel)XMLSerialization.XMLSerialization.Deserialize(filename, true);
            WorkspaceModel workspacemodel = persistantModel.WorkspaceModel;
            ValidateAndFixConnectionModels(workspacemodel);

            RestorePersistantSettings(persistantModel, workspacemodel);

            workspacemodel.AllConnectionModels = CheckModelListForCorruption(workspacemodel.AllConnectionModels, filename);
            workspacemodel.AllConnectorModels = CheckModelListForCorruption(workspacemodel.AllConnectorModels, filename);
            workspacemodel.AllImageModels = CheckModelListForCorruption(workspacemodel.AllImageModels, filename);
            workspacemodel.AllPluginModels = CheckModelListForCorruption(workspacemodel.AllPluginModels, filename);
            workspacemodel.AllTextModels = CheckModelListForCorruption(workspacemodel.AllTextModels, filename);

            if (handleTemplateReplacement)
            {
                try
                {
                    HandleTemplateReplacement(filename, workspacemodel);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Exception while template replacement: {0}", ex.Message), NotificationLevel.Warning);
                }
            }
            workspacemodel.UndoRedoManager.ClearStacks();
            return workspacemodel;
        }

        private List<T> CheckModelListForCorruption<T>(List<T> modelList, string filename)
        {
            //There has been a bug in CT2, which led to double entries of the same objects in internal model lists.
            List<T> distinctModelList = modelList.Distinct().ToList();
            if (distinctModelList.Count != modelList.Count)
            {
                GuiLogMessage(string.Format("The workspace model of file {0} is corrupt due to double entries in internal lists. It has been automatically repaired by the load routine.", filename), NotificationLevel.Warning);
                return distinctModelList;
            }
            return modelList;
        }

        /// <summary>
        /// Validates and fixes connection models
        /// </summary>
        /// <param name="workspaceModel"></param>
        public void ValidateAndFixConnectionModels(WorkspaceModel workspaceModel)
        {
            List<(ConnectorModel,ConnectorModel)> fromsTos = new List<(ConnectorModel, ConnectorModel)>();
            List<ConnectionModel> deleteConnections = new List<ConnectionModel>();

            foreach (ConnectionModel connectionModel in workspaceModel.AllConnectionModels)
            {
                if (fromsTos.Contains((connectionModel.From, connectionModel.To)))
                {
                    GuiLogMessage(String.Format("Found duplicate ConnectorModel between {0} and {1}. Remove it.", connectionModel.From.PluginModel.Name, connectionModel.To.PluginModel.Name), NotificationLevel.Warning);
                    deleteConnections.Add(connectionModel);
                }
                else
                {
                    fromsTos.Add((connectionModel.From, connectionModel.To));
                }
            }
            foreach (ConnectionModel connectionModel in deleteConnections)
            {
                workspaceModel.deleteConnectionModel(connectionModel);
            }
        }

        public WorkspaceModel loadModel(StreamWriter writer)
        {
            PersistantModel persistantModel = (PersistantModel)XMLSerialization.XMLSerialization.Deserialize(writer);            
            WorkspaceModel workspacemodel = persistantModel.WorkspaceModel;
            ValidateAndFixConnectionModels(workspacemodel);
            RestorePersistantSettings(persistantModel, workspacemodel);
            return workspacemodel;
        }

        public WorkspaceModel loadModel(XmlDocument filename)
        {
            PersistantModel persistantModel = (PersistantModel)XMLSerialization.XMLSerialization.Deserialize(filename);
            WorkspaceModel workspacemodel = persistantModel.WorkspaceModel;
            ValidateAndFixConnectionModels(workspacemodel);
            RestorePersistantSettings(persistantModel, workspacemodel);
            return workspacemodel;
        }

        public void HandleTemplateReplacement(string filename, WorkspaceModel workspacemodel)
        {
            string xmlFile = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".xml");
            if (!File.Exists(xmlFile))
            {
                return;
            }

            XElement xml = XElement.Load(xmlFile);
            if (XMLHelper.GetGlobalizedElementFromXML(xml, "replacements") == null)
            {
                return;
            }

            Dictionary<string, string> replacements = XMLHelper.GetGlobalizedElementFromXML(xml, "replacements").Elements().ToDictionary(r => r.Attribute("key").Value, r => r.Attribute("value").Value);

            foreach (PluginModel plugin in workspacemodel.AllPluginModels)
            {
                //Replace Names of components
                foreach (string key in replacements.Keys)
                {
                    plugin.Name = plugin.Name.Replace(key, replacements[key]);
                }

                //Replace text in TextInputs
                if (plugin.PluginType.FullName.Equals("CrypTool.TextInput.TextInput"))
                {
                    string value = (string)plugin.Plugin.Settings.GetType().GetProperty("Text").GetValue(plugin.Plugin.Settings, null);
                    if (replacements.ContainsKey(value))
                    {
                        plugin.Plugin.Settings.GetType().GetProperty("Text").SetValue(plugin.Plugin.Settings, replacements[value], null);
                        plugin.Plugin.GetType().GetMethod("Initialize").Invoke(plugin.Plugin, null);
                    }
                }
            }
            //Replace memo fields
            foreach (TextModel textmodel in workspacemodel.AllTextModels)
            {
                //create flowdocument out of data in xaml package format
                FlowDocument flowDocument = new FlowDocument();
                TextRange textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);

                using (MemoryStream memoryStream = new MemoryStream(textmodel.data))
                {
                    textRange.Load(memoryStream, System.Windows.DataFormats.XamlPackage);
                }
                //get content from textRange in RTF format
                string rtf = null;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    textRange.Save(memoryStream, DataFormats.Rtf);
                    memoryStream.Position = 0;
                    rtf = Encoding.UTF8.GetString(memoryStream.ToArray());
                }
                //replace all keys with corresponding values
                foreach (string key in replacements.Keys)
                {
                    rtf = rtf.Replace(key, GetRtfUnicodeEscapedString(replacements[key]));
                }
                //create new textRange with replaced values
                using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(rtf)))
                {
                    textRange.Load(memoryStream, DataFormats.Rtf);
                }
                //convert back to xaml package format
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    textRange.Save(memoryStream, System.Windows.DataFormats.XamlPackage);
                    textmodel.data = memoryStream.ToArray();
                    memoryStream.Close();
                }
            }
        }

        /// <summary>
        /// Converts the unicode chars to rtf escape values
        /// obtained from https://stackoverflow.com/questions/1368020/how-to-output-unicode-string-to-rtf-using-c
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static string GetRtfUnicodeEscapedString(string s)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in s)
            {
                if (c <= 0x7f)
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append("\\u" + Convert.ToUInt32(c) + "?");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Restores the stored settings
        /// </summary>
        /// <param name="persistantModel"></param>
        /// <param name="workspacemodel"></param>
        /// <exception cref="Exception"></exception>
        private void RestorePersistantSettings(PersistantModel persistantModel, WorkspaceModel workspacemodel)
        {
            //restore all settings of each plugin
            foreach (PersistantPlugin persistantPlugin in persistantModel.PersistantPluginList)
            {
                if (persistantPlugin.PluginModel.Plugin.Settings == null)
                {
                    continue; // do not attempt deserialization if plugin type has no settings
                }

                foreach (PersistantSetting persistantSetting in persistantPlugin.PersistantSettingsList)
                {

                    PropertyInfo[] propertyInfos = persistantPlugin.PluginModel.Plugin.Settings.GetType().GetProperties();
                    foreach (PropertyInfo propertyInfo in propertyInfos)
                    {
                        try
                        {
                            DontSaveAttribute[] dontSave =
                                (DontSaveAttribute[])propertyInfo.GetCustomAttributes(typeof(DontSaveAttribute), false);
                            if (dontSave.Length == 0)
                            {
                                if (propertyInfo.Name.Equals(persistantSetting.Name))
                                {
                                    if (persistantSetting.Type.Equals("System.String") && propertyInfo.PropertyType.FullName.Equals("System.String"))
                                    {
                                        propertyInfo.SetValue(persistantPlugin.PluginModel.Plugin.Settings,
                                                       persistantSetting.Value, null);
                                    }
                                    else if (persistantSetting.Type.Equals("System.Int16") && propertyInfo.PropertyType.FullName.Equals("System.Int16"))
                                    {
                                        propertyInfo.SetValue(persistantPlugin.PluginModel.Plugin.Settings,
                                                       short.Parse(persistantSetting.Value), null);
                                    }
                                    else if (persistantSetting.Type.Equals("System.Int32") && propertyInfo.PropertyType.FullName.Equals("System.Int32"))
                                    {
                                        propertyInfo.SetValue(persistantPlugin.PluginModel.Plugin.Settings,
                                                       int.Parse(persistantSetting.Value), null);
                                    }
                                    else if (persistantSetting.Type.Equals("System.Int64") && propertyInfo.PropertyType.FullName.Equals("System.Int64"))
                                    {
                                        propertyInfo.SetValue(persistantPlugin.PluginModel.Plugin.Settings,
                                                       long.Parse(persistantSetting.Value), null);
                                    }
                                    else if (persistantSetting.Type.Equals("System.Single") && propertyInfo.PropertyType.FullName.Equals("System.Single"))
                                    {
                                        float.TryParse(persistantSetting.Value.Replace(',', '.'),
                                                                                NumberStyles.Number,
                                                                                CultureInfo.CreateSpecificCulture("en-Us"),
                                                                                out float result);
                                        propertyInfo.SetValue(persistantPlugin.PluginModel.Plugin.Settings, result, null);
                                    }
                                    else if (persistantSetting.Type.Equals("System.Double") && propertyInfo.PropertyType.FullName.Equals("System.Double"))
                                    {
                                        double.TryParse(persistantSetting.Value.Replace(',', '.'),
                                                                                NumberStyles.Number,
                                                                                CultureInfo.CreateSpecificCulture("en-Us"),
                                                                                out double result);
                                        propertyInfo.SetValue(persistantPlugin.PluginModel.Plugin.Settings, result, null);
                                    }
                                    else if (persistantSetting.Type.Equals("System.Boolean") && propertyInfo.PropertyType.FullName.Equals("System.Boolean"))
                                    {
                                        propertyInfo.SetValue(persistantPlugin.PluginModel.Plugin.Settings,
                                                       bool.Parse(persistantSetting.Value), null);
                                    }
                                    else if (propertyInfo.PropertyType.IsEnum)
                                    {
                                        int.TryParse(persistantSetting.Value, out int result);
                                        object newEnumValue = Enum.ToObject(propertyInfo.PropertyType, result);
                                        propertyInfo.SetValue(persistantPlugin.PluginModel.Plugin.Settings, newEnumValue, null);
                                    }
                                    else
                                    {
                                        if (OnGuiLogNotificationOccured != null)
                                        {
                                            OnGuiLogNotificationOccured.Invoke(persistantPlugin.PluginModel.Plugin,
                                            new GuiLogEventArgs(
                                                string.Format(Resources.ModelPersistance_restoreSettings_Could_not_restore_the_setting___0___of_plugin___1__, persistantSetting.Name, persistantPlugin.PluginModel.Name),
                                                persistantPlugin.PluginModel.Plugin,
                                                NotificationLevel.Warning));
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            if (OnGuiLogNotificationOccured != null)
                            {
                                OnGuiLogNotificationOccured.Invoke(persistantPlugin.PluginModel.Plugin,
                                           new GuiLogEventArgs(
                                               string.Format(Resources.ModelPersistance_restoreSettings_Could_not_restore_the_setting___0___of_plugin___1__, persistantSetting.Name, persistantPlugin.PluginModel.Name),
                                               persistantPlugin.PluginModel.Plugin,
                                               NotificationLevel.Warning));
                            }
                        }
                    }
                }
            }

            //check if all properties belonging to its ConnectorModels really exist and if each property has a ConnectorModel
            //if not generate new ConnectorModels
            foreach (PluginModel pluginModel in workspacemodel.AllPluginModels)
            {
                List<ConnectorModel> connectorModels = new List<ConnectorModel>();
                connectorModels.AddRange(pluginModel.InputConnectors);
                connectorModels.AddRange(pluginModel.OutputConnectors);
                //Check if a property of a ConnectorModel was deleted or its type changed => delete the ConnectorModel););
                //also delete it silently, if we are not in CryptoBenchmark and it is marked for CryptoBenchmark
                foreach (ConnectorModel connectorModel in new List<ConnectorModel>(connectorModels))
                {
                    PropertyInfo propertyInfo = connectorModel.PluginModel.Plugin.GetType().GetProperty(connectorModel.PropertyName);
                    if (propertyInfo == null ||
                        !connectorModel.ConnectorType.Equals(propertyInfo.PropertyType))
                    {
                        //the property belonging to this ConnectorModel was not found
                        //or the type of the saved property differs to the real one
                        //so we delete the connector
                        pluginModel.WorkspaceModel.deleteConnectorModel(connectorModel);
                        connectorModels.Remove(connectorModel);
                        GuiLogMessage(string.Format(Resources.ModelPersistance_restoreSettings_A_property_with_name___0___of_type___1___does_not_exist_in___2___3___but_a_ConnectorModel_exists_in_the_PluginModel__Delete_the_ConnectorModel_now_, connectorModel.PropertyName, connectorModel.ConnectorType.Name, pluginModel.PluginType, pluginModel.Name),
                                      NotificationLevel.Warning);
                    }
                }
                //Check if there are properties which have no own ConnectorModel
                foreach (PropertyInfoAttribute propertyInfoAttribute in pluginModel.Plugin.GetProperties())
                {
                    IEnumerable<ConnectorModel> query = from c in connectorModels
                                                        where c.PropertyName.Equals(propertyInfoAttribute.PropertyName)
                                                        select c;
                    if (query.Count() == 0)
                    {
                        //we found a property which has no ConnectorModel, so we create a new one
                        pluginModel.generateConnector(propertyInfoAttribute);
                        GuiLogMessage(string.Format(Resources.ModelPersistance_restoreSettings_A_ConnectorModel_for_the_plugins_property___0___of_type___1___does_not_exist_in_the_PluginModel_of___2___3____Create_a_ConnectorModel_now_, propertyInfoAttribute.PropertyName, propertyInfoAttribute.PropertyInfo.PropertyType.Name, pluginModel.PluginType, pluginModel.Name),
                                      NotificationLevel.Warning);
                    }
                }
            }

            //initialize the plugins
            //connect all listener for plugins/plugin models            
            foreach (PluginModel pluginModel in workspacemodel.AllPluginModels)
            {
                try
                {
                    pluginModel.Plugin.Initialize();
                    pluginModel.PercentageFinished = 0;
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format(Resources.ModelPersistance_restoreSettings_Error_while_initializing____0__, pluginModel.Name), ex);
                }
                pluginModel.Plugin.OnGuiLogNotificationOccured += workspacemodel.GuiLogMessage;
                pluginModel.Plugin.OnPluginProgressChanged += pluginModel.PluginProgressChanged;
                pluginModel.Plugin.OnPluginStatusChanged += pluginModel.PluginStatusChanged;
                if (pluginModel.Plugin.Settings != null)
                {
                    pluginModel.Plugin.Settings.PropertyChanged += pluginModel.SettingsPropertyChanged;
                }
                pluginModel.StoreAllDefaultInputConnectorValues();
            }

            foreach (ConnectorModel connectorModel in workspacemodel.AllConnectorModels)
            {
                //refresh language stuff
                foreach (PropertyInfoAttribute property in connectorModel.PluginModel.Plugin.GetProperties())
                {
                    if (property.PropertyName.Equals(connectorModel.PropertyName))
                    {
                        connectorModel.ToolTip = property.ToolTip;
                        connectorModel.Caption = property.Caption;
                        break;
                    }
                }
                connectorModel.PluginModel.Plugin.PropertyChanged += connectorModel.PropertyChangedOnPlugin;
            }

            //restore all IControls
            foreach (ConnectionModel connectionModel in workspacemodel.AllConnectionModels)
            {
                ConnectorModel from = connectionModel.From;
                ConnectorModel to = connectionModel.To;
                try
                {
                    if (from.IControl && to.IControl)
                    {
                        object data = null;
                        //Get IControl data from "to"                       
                        data = to.PluginModel.Plugin.GetType().GetProperty(to.PropertyName).GetValue(to.PluginModel.Plugin, null);
                        PropertyInfo propertyInfo = from.PluginModel.Plugin.GetType().GetProperty(from.PropertyName);
                        propertyInfo.SetValue(from.PluginModel.Plugin, data, null);

                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format(Resources.ModelPersistance_restoreSettings_Error_while_restoring_IControl_Connection_between___0___to___1____Workspace_surely_will_not_work_well_, from.PluginModel.Name, to.PluginModel.Name), ex);
                }
            }

            //Check if all TextModels and ImageModelsmodels are valid (byte array != null || byte array is empty)
            //Otherwise delete them from the model and show a warning GuiLogMessage
            foreach (TextModel textModel in new List<TextModel>(workspacemodel.AllTextModels))
            {
                if (!textModel.HasData())
                {
                    GuiLogMessage(
                        string.Format(Resources.ModelPersistance_restoreSettings_TextModel),
                        NotificationLevel.Warning);
                    workspacemodel.AllTextModels.Remove(textModel);
                }
            }
            foreach (ImageModel imageModel in new List<ImageModel>(workspacemodel.AllImageModels))
            {
                if (!imageModel.HasData())
                {
                    GuiLogMessage(string.Format(Resources.ModelPersistance_restoreSettings_ImageModel),
                        NotificationLevel.Warning);
                    workspacemodel.AllImageModels.Remove(imageModel);
                }
            }

            //Store settings in SettingsManager of UndoRedoManager
            workspacemodel.UndoRedoManager.SettingsManager.StoreCurrentSettingValues();
        }

        /// <summary>
        /// Serializes the given model to a file with the given filename
        /// </summary>
        /// <param name="workspaceModel"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public void saveModel(WorkspaceModel workspaceModel, string filename)
        {
            XMLSerialization.XMLSerialization.Serialize(GetPersistantModel(workspaceModel), filename, true);
            workspaceModel.UndoRedoManager.SavedHere = true;
        }

        public PersistantModel GetPersistantModel(WorkspaceModel workspaceModel)
        {
            PersistantModel persistantModel = new PersistantModel
            {
                WorkspaceModel = workspaceModel
            };

            //Save all Settings of each Plugin
            foreach (PluginModel pluginModel in workspaceModel.AllPluginModels)
            {
                if (pluginModel.Plugin.Settings != null)
                {
                    PropertyInfo[] arrpInfo = pluginModel.Plugin.Settings.GetType().GetProperties();

                    PersistantPlugin persistantPlugin = new PersistantPlugin
                    {
                        PluginModel = pluginModel
                    };

                    foreach (PropertyInfo pInfo in arrpInfo)
                    {
                        DontSaveAttribute[] dontSave = (DontSaveAttribute[])pInfo.GetCustomAttributes(typeof(DontSaveAttribute), false);
                        if (pInfo.CanWrite && dontSave.Length == 0)
                        {
                            PersistantSetting persistantSetting = new PersistantSetting();
                            if (pInfo.PropertyType.IsEnum)
                            {
                                persistantSetting.Value = "" + pInfo.GetValue(pluginModel.Plugin.Settings, null).GetHashCode();
                            }
                            else
                            {
                                persistantSetting.Value = "" + pInfo.GetValue(pluginModel.Plugin.Settings, null);
                            }
                            persistantSetting.Name = pInfo.Name;
                            persistantSetting.Type = pInfo.PropertyType.FullName;
                            persistantPlugin.PersistantSettingsList.Add(persistantSetting);
                        }
                    }
                    persistantModel.PersistantPluginList.Add(persistantPlugin);
                }
            }
            return persistantModel;
        }

        /// <summary>
        /// Loggs a gui message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="level"></param>
        internal void GuiLogMessage(string message, NotificationLevel level)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                GuiLogEventArgs args = new GuiLogEventArgs(message, null, level)
                {
                    Title = "-"
                };
                OnGuiLogNotificationOccured(null, args);
            }
        }


    }

    /// <summary>
    /// Class for persisting a workspace model
    /// stores the model and a list of persistant plugin models
    /// </summary>
    [Serializable]
    public class PersistantModel
    {
        public WorkspaceModel WorkspaceModel { get; set; }
        public List<PersistantPlugin> PersistantPluginList = new List<PersistantPlugin>();
    }

    /// <summary>
    /// Class for persisting a plugin model
    /// stores the plugin model and a list of settings
    /// </summary>
    [Serializable]
    public class PersistantPlugin
    {
        public PluginModel PluginModel { get; set; }
        public List<PersistantSetting> PersistantSettingsList = new List<PersistantSetting>();
    }

    /// <summary>
    /// Class for persisting settings
    /// stores the name, the type and the value of the setting
    /// </summary>
    [Serializable]
    public class PersistantSetting
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }

}
