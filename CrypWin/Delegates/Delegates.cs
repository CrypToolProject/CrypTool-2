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
using System;
using System.Collections.Generic;

namespace CrypTool.CrypWin
{
    /// <summary>
    /// Fired by TaskPane if ShowPluginDescription button was pressed
    /// </summary>
    public delegate void ShowPluginDescription(object sender);

    /// <summary>
    /// Used to load plugins async and show gui while init of plugins is still in progress.
    /// </summary>
    public delegate void LoadPluginsDelegate();

    /// <summary>
    /// Used for a convenient app startup
    /// </summary>
    public delegate void InitTypesDelegate(Dictionary<string, List<Type>> dicTypeLists);

    /// <summary>
    /// Used to display LogMessages in gui thread
    /// </summary>
    public delegate void GuiLogNotificationDelegate(object sender, GuiLogEventArgs arg);

    /// <summary>
    /// Used to select last item in log list of CrypTool
    /// </summary>
    public delegate void SetSelectedItemDelegate();

    /// <summary>
    /// Used to set init messages on startup
    /// </summary>
    public delegate void SetMessageDelegate(string message);

    public delegate bool? ShowInitWindow();

    public delegate void ExecuteDelegate();

    public delegate void OpenDelegate(string filename);
}
