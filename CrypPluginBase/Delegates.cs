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
using CrypTool.PluginBase.Control;
using CrypTool.PluginBase.Editor;
using System;
using System.Numerics;
using System.Windows.Controls;

namespace CrypTool.PluginBase
{
    #region public interface delegates
    /// <summary>
    /// Used by plugins to inform the editor about internal status changes. E.g. a comparator
    /// plugin can send a new icon to show after comparison was done.
    /// </summary>
    public delegate void StatusChangedEventHandler(IPlugin sender, StatusEventArgs args);

    /// <summary>
    /// Used to send messages to the GUI log-window.
    /// </summary>
    public delegate void GuiLogNotificationEventHandler(IPlugin sender, GuiLogEventArgs args);

    /// <summary>
    /// Used to notify the editor about prgress changes in execution method.
    /// </summary>
    public delegate void PluginProgressChangedEventHandler(IPlugin sender, PluginProgressEventArgs args);
    #endregion public interface delegates

    #region editor delegates
    /// <summary>
    /// Used by editor plugins to communicate the selected plugin to CrypWin.
    /// </summary>
    public delegate void SelectedPluginChangedHandler(IEditor editor, PluginChangedEventArgs pce);

    /// <summary>
    /// Used by editor plugins to send current project title to CrypWin.
    /// </summary>
    public delegate void ProjectTitleChangedHandler(IEditor editor, string newProjectTitle);

    /// <summary>
    /// Used by editor plugins after a project file was dropped on workspace, to inform the GUI
    /// to execute project open operations (including questions for unsaved changes etc.).
    /// </summary>
    public delegate void OpenProjectFileHandler(IEditor editor, string fileName);

    /// <summary>
    /// Used to signal crypwin that an editor wants to open a tab.
    /// </summary>  
    public delegate TabItem OpenTabHandler(object content, TabInfo info, IEditor parent);

    /// <summary>
    /// Used to signal crypwin that an editor wants to open an editor.
    /// </summary>  
    public delegate IEditor OpenEditorHandler(Type editorType, TabInfo info);

    /// <summary>
    /// Used to signal crypwin, that the loading of a file (initiated by the IEditor.Open method) is done.
    /// </summary>
    public delegate void FileLoadedHandler(IEditor editor, string filename);
    #endregion editor delegates

    #region optional delegates
    /// <summary>
    /// This optional delegate can be used by plugins to inform editors of an 
    /// change of dynamic properties.
    /// </summary>
    public delegate void DynamicPropertiesChanged(IPlugin plugin);

    /// <summary>
    /// This optional delegate can be used to hide task pane settings.
    /// </summary>
    public delegate void TaskPaneAttributeChangedHandler(ISettings settings, TaskPaneAttributeChangedEventArgs args);
    #endregion optional delegates

    #region master-slave delegates
    public delegate void KeyPatternChanged();
    public delegate void IControlStatusChangedEventHandler(IControl sender, bool readyForExecution);

    #region Delegates for Manager-/Worker-Infrastructure

    /// <summary>
    /// P2PWorker-Control delegate: will be thrown, when the actual job is successfully processed
    /// </summary>
    /// <param name="result"></param>
    public delegate void ProcessingSuccessfullyEnded(BigInteger JobId, byte[] result);
    /// <summary>
    /// P2PWorker-Control delegate: will be thrown, when processing was canceled by the user or due to an error
    /// </summary>
    /// <param name="result"></param>
    public delegate void ProcessingCanceled(byte[] result);
    public delegate void InfoText(string sText, NotificationLevel notLevel);

    #endregion
    #endregion
}
