/*
   Copyright 2019 Simon Leischnig, based on the work of Soeren Rinne

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
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using LogLevel = CrypTool.LFSR.Utils.LogLevel;

namespace CrypTool.LFSR.Utils
{
    public static class Compat
    {
        public static dynamic ThrowCompatIncomplete(string msg)
        {
            throw new Exception($"CT2 component compat (CrypTool.Component.StandaloneCompat) has a bug: {msg}");
        }
        public static NotificationLevel ConvertNotificationLevel(Utils.LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug matched:
                    return NotificationLevel.Debug;
                case LogLevel.Info matched:
                    return NotificationLevel.Info;
                case LogLevel.Warning matched:
                    return NotificationLevel.Warning;
                case LogLevel.Error matched:
                    return NotificationLevel.Error;
                case LogLevel.Balloon matched:
                    return NotificationLevel.Balloon;
                default:
                    return ThrowCompatIncomplete("not all LogLevels covered");
            }
        }
    }

    public abstract class AbstractStandaloneComponentCompat<ApiType, ParamsType>
        : AbstractStandaloneComponent<ApiType, ParamsType>, ICrypComponent
        where ApiType : IComponentAPI<ParamsType>
        where ParamsType : IParameters
    {
        public ISettings Settings { get; protected set; }
        public event StatusChangedEventHandler OnPluginStatusChanged = (sender, args) => { };
        public event PluginProgressChangedEventHandler OnPluginProgressChanged = (sender, args) => { };
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured = (sender, args) => { };
        public event PropertyChangedEventHandler PropertyChanged = (sender, args) => { };

        public AbstractStandaloneComponentCompat(ApiType api) : base(api)
        {
            this.api.OnStatusImageChanged += (newStatus) => ChangeStatus(newStatus);
            this.api.OnProgressChanged += (current) => ChangeProgress(current.Ratio, 1.0);
            this.api.OnLogMessage += (msg, level) => LogFromApi(msg, level);

            Settings = CreateSettingsDescriptor(api.Parameters);
        }

        protected abstract ISettings CreateSettingsDescriptor(ParamsType parameters);

        #region event reconciliation with the IComponentAPI
        protected void RaisePropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        protected void Log(string msg, NotificationLevel lvl)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(msg, this, lvl));
        }

        protected void LogDebug(string msg)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(msg, this, NotificationLevel.Debug));
        }

        protected void ChangeProgress(double current, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(current, max));
        }
        protected void ChangeStatus(int newStatus)
        {
            EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, newStatus));
        }
        /// <summary>
        /// Implements redirection of IComponentAPI log messages. By default implementation, messages are passed on directly to the GUI Log.
        /// </summary>
        /// <param name="msg">The log message</param>
        /// <param name="lvl">The Simlei.Util.Logging.LogLevel</param>
        protected void LogFromApi(string msg, LogLevel lvl)
        {
            Log(msg, Compat.ConvertNotificationLevel(lvl));
        }
        #endregion
        #region lifecycle
        public void Dispose()
        {
            api._raiseDispose();
        }

        public void Execute()
        {
            api._raiseExecute();
        }

        public void Initialize()
        {
            api._raiseInitialize();
        }

        public void PostExecution()
        {
            api._raisePostExecution();
        }

        public void PreExecution()
        {
            api._raisePreExecution();
        }

        public void Stop()
        {
            api._raiseStop();
        }
        #endregion
    }
    // this class wraps access to the IParameters instance of the IComponentAPI
    public abstract class AbstractComponentSettingsCompat<ParamsType>
        : ISettings
        where ParamsType : IParameters
    {
        private readonly ParamsType Parameters;

        // Wiring of property Ids to parameters (for CT2 property-name-based event infrastructure)
        protected Dictionary<dynamic, string> Ids;
        protected string GetPropertyId(dynamic parameter)
        {
            if (!Ids.ContainsKey(parameter))
            {
                throw new Exception($"parameter {parameter} was not registered as an attributed property (through AssignPropertyId(...))");
            }

            return Ids[parameter];
        }
        protected void AssignPropertyId<T>(IParameter<T> parameter, string id)
        {
            Ids[parameter] = id;
        }

        #region CT2 settings event infrastructure
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged = (settings, args) => { };
        public event PropertyChangedEventHandler PropertyChanged = (sender, eventArgs) => { };
        public void RaisePropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }
        #endregion

        public event Action OnInitialization = () => { };
        public AbstractComponentSettingsCompat() { }

        // CT2 ISettings interface member re-implemented with an event
        /// <summary>
        /// Initialize initalizes the settings of a component. it is called directly after the call of the Initialize call of the component.
        /// In contrast to the CT2 API, this is implemented as Action event for which the implementation may register (in the constructor e.g.)
        /// </summary>
        public void Initialize()
        {
            OnInitialization();
        }

        // helper functions for common CT2 Workspace UI tasks

        protected void UpdateVisibility(string key, Visibility vis, TaskPaneAttribute tpa = null)
        {
            if (tpa != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(key, vis, tpa)));
            }
            else
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(key, vis)));
            }
        }
    }
}
