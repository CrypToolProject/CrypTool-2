/*                              
   Copyright 2026 Marc Philipp Kray, CrypTool Project

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

/// <summary>
/// This file implements the CrypWinAdapter class, which serves as a cross-boundary bridge 
/// between the main WPF graphical user interface (CrypWin) and the CrypLLM chat runtime. 
/// It encapsulates interactions with UI components to ensure thread-safety and decoupling.
/// </summary>

using CrypTool.CrypLLM.Ports;
using CrypTool.CrypLLM.Helper;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace CrypTool.CrypWin.Adapter
{
    /// <summary>
    /// Adapter implementation of ICrypWinAdapter to expose main window functionalities to the chat runtime.
    /// By heavily utilizing the UI Dispatcher, this class protects against invalid cross-thread operations 
    /// that are common when an asynchronous or background framework (like an LLM module) queries GUI states.
    /// </summary>
    internal sealed class CrypWinAdapter : ICrypWinAdapter
    {
        private readonly MainWindow _mainWindow;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrypWinAdapter"/> class.
        /// </summary>
        /// <param name="mainWindow">The active main application window to interface with.</param>
        /// <exception cref="ArgumentNullException">Thrown to prevent unpredictable behavior if no valid window is supplied.</exception>
        public CrypWinAdapter(MainWindow mainWindow)
        {
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        }

        /// <summary>
        /// Gets the currently active plugin editor. 
        /// </summary>
        public IEditor ActiveEditor => _mainWindow.ActiveEditor;

        /// <summary>
        /// Retrieves a collection of open tabs from the application workspace.
        /// Due to the collection being tied to UI controls, iteration is marshaled to the UI thread.
        /// </summary>
        /// <returns>An enumerable collection encapsulating abstract tab data.</returns>
        public IEnumerable<OpenTabsAbstraction> GetOpenTabs()
        {
            List<OpenTabsAbstraction> result = null;

            // Ensure we read UI data on the UI thread to avoid InvalidOperationExceptions.
            _mainWindow.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                result = _mainWindow.GetOpenTabs();
            }, null);

            return result ?? new List<OpenTabsAbstraction>();
        }

        /// <summary>
        /// Attempts to locate and retrieve a workspace editor based on its registered tab identifier.
        /// </summary>
        /// <param name="tabId">The string identifier for the target tab.</param>
        /// <param name="editor">Extracted editor instance, or null if unsuccessful.</param>
        /// <returns>True if the editor could be successfully matched to a tab; otherwise, false.</returns>
        public bool TryGetWorkspaceEditorByTabId(string tabId, out IEditor editor)
        {
            IEditor local = null;

            // The underlying lookup reads window layouts; therefore, dispatch matching to the UI thread.
            _mainWindow.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                local = _mainWindow.TryGetWorkspaceModelByTabId(tabId);
            }, null);

            editor = local;
            return editor != null;
        }

        /// <summary>
        /// Attempts to retrieve plain-text documentation from the designated or currently selected Online Help tab.
        /// </summary>
        public bool TryGetDocumentationContextByTabId(string tabId, out DocumentationContextAbstraction documentationContext)
        {
            DocumentationContextAbstraction local = null;

            _mainWindow.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _mainWindow.TryGetDocumentationContextByTabId(tabId, out local);
            }, null);

            documentationContext = local;
            return documentationContext != null;
        }

        /// <summary>
        /// Queries recent log messages generated across the application, filtered by severity.
        /// Limits bounds mathematically to restrict memory pressure and overflow from excessive queries.
        /// </summary>
        /// <param name="maxLines">Requested number of historical log entries.</param>
        /// <returns>An ordered list of recent log line strings.</returns>
        public IList<string> GetRecentLogMessages(int maxLines)
        {
            // Clamp the request volume to ensure safe boundaries (1 min, 2000 max lines).
            int safeMaxLines = Math.Max(1, Math.Min(2000, maxLines));

            // Retrieves comprehensive application logs directly using the predefined notification severity levels.
            IList<string> all = _mainWindow.GetAllMessagesFromGuiThread(
                NotificationLevel.Debug,
                NotificationLevel.Info,
                NotificationLevel.Warning,
                NotificationLevel.Error,
                NotificationLevel.Balloon);

            if (all == null || all.Count == 0)
            {
                return new List<string>();
            }

            // Exceeding subset limitation: Skip early elements to yield only the desired tail count.
            int skip = Math.Max(0, all.Count - safeMaxLines);
            return all.Skip(skip).ToList();
        }

        /// <summary>
        /// Triggers the UI command to reveal or highlight the AI Chat pane.
        /// </summary>
        public void ShowAIChatPane()
        {
            _mainWindow.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _mainWindow.ShowAIChatPane();
            }, null);
            
        }

        /// <summary>
        /// Triggers the UI command to open and display the AI Chat settings configuration dialog.
        /// </summary>
        public void ShowAIChatSettings()
        {
            _mainWindow.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _mainWindow.ShowAIChatSettings();
            }, null);
        }

        /// <summary>
        /// Creates a new visible tab containing an empty workspace and returns its abstract tab metadata.
        /// </summary>
        public OpenTabsAbstraction CreateEmptyWorkspaceTab()
        {
            OpenTabsAbstraction result = null;

            _mainWindow.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                result = _mainWindow.CreateEmptyWorkspaceTab();
            }, null);

            return result;
        }

        /// <summary>
        /// Opens an existing CT2 template in a new visible workspace tab and returns its abstract tab metadata.
        /// </summary>
        public OpenTabsAbstraction OpenTemplateWorkspaceTab(string templateFilePath)
        {
            OpenTabsAbstraction result = null;

            _mainWindow.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                result = _mainWindow.OpenTemplateWorkspaceTab(templateFilePath);
            }, null);

            return result;
        }
    }
}
