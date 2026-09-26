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
using System.Collections.Generic;
using CrypTool.PluginBase;

namespace WorkspaceManager.View.Base
{
    /// <summary>
    /// Holds a plugin instance and keeps track of all task pane attribute changes for that plugin.
    /// </summary>
    public class PluginSettingsContainer
    {
        private readonly Dictionary<string, TaskPaneAttribteContainer> currentTaskPaneAttributes;

        public IEnumerable<TaskPaneAttribteContainer> CurrentTaskPaneAttributes => currentTaskPaneAttributes.Values;

        public IPlugin Plugin { get; }

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public PluginSettingsContainer(IPlugin plugin)
        {
            Plugin = plugin;
            currentTaskPaneAttributes = new Dictionary<string, TaskPaneAttribteContainer>();

            System.Reflection.EventInfo taskPaneAttributeChanged = plugin.Settings?.GetTaskPaneAttributeChanged();
            if (taskPaneAttributeChanged != null)
            {
                taskPaneAttributeChanged.AddEventHandler(plugin.Settings,
                    new TaskPaneAttributeChangedHandler(HandleTaskPaneAttributeChange));
            }
        }

        private void HandleTaskPaneAttributeChange(ISettings settings, TaskPaneAttributeChangedEventArgs args)
        {
            foreach (TaskPaneAttribteContainer tpac in args.ListTaskPaneAttributeContainer)
            {
                currentTaskPaneAttributes[tpac.Property] = tpac;
            }

            TaskPaneAttributeChanged?.Invoke(settings, args);
        }
    }
}
