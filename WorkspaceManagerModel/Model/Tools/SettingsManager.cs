/*                              
   Copyright 2021 Nils Kopal, University of Siegen

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
using System.Collections.Generic;
using WorkspaceManager.Model;

namespace WorkspaceManagerModel.Model.Tools
{
    /// <summary>
    /// This class stores the current settings values, which are then used in the undo/redo manager to create
    /// for a ChangeSettingsOperation
    /// </summary>
    public class SettingsManager
    {
        private readonly Dictionary<string, object> _settingsValues = new Dictionary<string, object>();
        private readonly WorkspaceModel _workspaceModel;

        public SettingsManager(WorkspaceModel workspaceModel)
        {
            _workspaceModel = workspaceModel;
        }

        /// <summary>
        /// Stores all setting values of all PluginModels of the given WorkspaceModel
        /// </summary>
        /// <param name="workspaceModel"></param>
        public void StoreCurrentSettingValues()
        {
            foreach (PluginModel pluginModel in _workspaceModel.AllPluginModels)
            {
                if (pluginModel.Plugin.Settings == null)
                {
                    continue;
                }
                System.Reflection.PropertyInfo[] properties = pluginModel.Plugin.Settings.GetType().GetProperties();
                foreach (System.Reflection.PropertyInfo property in properties)
                {
                    object value = property.GetValue(pluginModel.Plugin.Settings);
                    string propertyId = pluginModel.Plugin.Settings.GetHashCode() + "_" + property.Name;
                    if (!_settingsValues.ContainsKey(propertyId))
                    {
                        _settingsValues.Add(propertyId, value);
                    }
                    else
                    {
                        _settingsValues[propertyId] = value;
                    }
                }
            }
        }

        /// <summary>
        /// Return the current setting value identified by the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetCurrentSettingValue(string key)
        {
            return _settingsValues[key];
        }
    }
}
