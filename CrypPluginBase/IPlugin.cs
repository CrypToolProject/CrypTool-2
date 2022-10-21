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
using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace CrypTool.PluginBase
{
    /// <summary>
    /// See Wiki for more information: https://www.CrypTool.org/trac/CrypTool2/wiki/IPluginHints
    /// </summary>
    public interface IPlugin : INotifyPropertyChanged, IDisposable
    {
        event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        ISettings Settings { get; }

        /// <summary>
        /// May return custom UI presentation or null.
        /// </summary>
        /// <value>The presentation.</value>
        UserControl Presentation { get; }

        /// <summary>
        /// Will be called each time the plugin is run during workflow and after the inputs have been set.
        /// </summary>
        void Execute();

        /// <summary>
        /// Triggered when user clicked Stop button. Plugin must shut down long running tasks.
        /// </summary>
        void Stop();

        /// <summary>
        /// Called once for each plugin instance after creation.
        /// </summary>
        void Initialize();
    }
}
