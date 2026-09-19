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
/// This file implements the LLMPluginService. It functions as the central orchestration facility 
/// linking asynchronous LLM workflows to the graphical dispatcher. Through the employment of 
/// execution contexts (`AsyncLocal`) and priority-based Thread-Marshaling, it stabilizes conversational interactions 
/// against asynchronous tab-switching behavior executed by users during model generation.
/// </summary>

using CrypTool.CrypLLM.Ports;
using CrypTool.PluginBase.Editor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using WorkspaceManager;
using WorkspaceManager.Model;
using WorkspaceManager.View.Visuals;

namespace CrypTool.CrypLLM.Services
{
    /// <summary>
    /// Central helper for resolving workspace-related context from the host application.
    /// Manages temporal correlation, allowing generated interactions to lock onto a target workspace ('pinned')
    /// to safely execute multi-step tool logic corresponding to the moment a user requested assistance.
    /// </summary>
    internal static class LLMPluginService
    {
        // Enforces linguistic normalization during formatting operations executed within background kernels.
        private static readonly CultureInfo ToolCulture = CultureInfo.GetCultureInfo("en");

        // Compensates for natural language variability when models hallucinatively refer to the "current" interface.
        private static readonly HashSet<string> ActiveTabAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "active",
            "current",
            "focused",
            "default"
        };

        // AsyncLocal keeps the preferred tab id scoped to the current asynchronous task execution flow.
        // This structural strategy segregates contextual states, preventing cross-request pollution during simultaneous inferences.
        private static readonly AsyncLocal<string> _preferredWorkspaceTabId = new AsyncLocal<string>();

        /// <summary>
        /// Executes a function synchronously on the UI dispatcher thread.
        /// Resolves unhandled access exceptions that typically arise when background AI threads probe graphical model states.
        /// </summary>
        /// <param name="func">The targeted anonymous execution routine.</param>
        public static T InvokeOnUi<T>(Func<T> func)
        {
            var session = Threads.AgentEditSession.Current;
            T Execute()
            {
                using (Threads.AgentEditSession.EnterUi(session))
                using (WorkspaceManager.Model.Tools.UndoRedoManager.PushOperationOwner(session?.Owner))
                {
                    session?.BeforeTool();
                    T result = ExecuteWithToolCulture(func);
                    return session == null ? result : session.AfterTool(result);
                }
            }
            Dispatcher dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                return Execute();
            }

            // Invokes on the background priority queue preventing main GUI frame-drops during iterative schema generations.
            return dispatcher.Invoke(Execute, DispatcherPriority.Background);
        }

        /// <summary>
        /// Assures localized thread execution complies with a standardized diagnostic 'en' culture. 
        /// Crucial for guaranteeing consistent decimal serialization in JSON structures required by Semantic Kernel.
        /// </summary>
        private static T ExecuteWithToolCulture<T>(Func<T> func)
        {
            CultureInfo previousCulture = Thread.CurrentThread.CurrentCulture;
            CultureInfo previousUiCulture = Thread.CurrentThread.CurrentUICulture;

            try
            {
                Thread.CurrentThread.CurrentCulture = ToolCulture;
                Thread.CurrentThread.CurrentUICulture = ToolCulture;
                return func();
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = previousCulture;
                Thread.CurrentThread.CurrentUICulture = previousUiCulture;
            }
        }

        /// <summary>
        /// Injects a targeted UI tab ID into the executing asynchronous context. 
        /// Implements <see cref="IDisposable"/> ensuring state is safely rolled back via an encapsulating `using` statement after the conversational request concludes.
        /// </summary>
        /// <param name="tabId">The target instance identifier to bind onto.</param>
        public static IDisposable PushPreferredWorkspaceTabId(string tabId)
        {
            string previous = _preferredWorkspaceTabId.Value;
            _preferredWorkspaceTabId.Value = NormalizeTabId(tabId);
            return new Scope(() => _preferredWorkspaceTabId.Value = previous);
        }

        /// <summary>
        /// Resolves the contextually appropriate WorkspaceModel by evaluating cascading fallbacks.
        /// A request-local pin is the safety anchor. A different explicit tab may
        /// be used only when both the pin and that tab still resolve. If the model
        /// invents an invalid tab ID, resolution stays on the pinned workspace.
        /// Without a request pin, an explicit tab and finally the active editor are used.
        /// </summary>
        public static bool TryGetWorkspaceModel(out WorkspaceModel model, string tabId = "")
        {
            model = null;

            if (!TryGetWorkspaceEditor(out WorkspaceManagerClass workspaceManager, out string effectiveTabId, tabId))
            {
                return false;
            }

            if (workspaceManager.Presentation is not EditorVisual visual)
            {
                Log.Debug($"WorkspaceManager presentation is not an EditorVisual (tabId: {effectiveTabId}).");
                return false;
            }

            model = visual.Model;
            if (model != null) Threads.AgentEditSession.Current?.Register(model, effectiveTabId);
            return model != null;
        }

        /// <summary>
        /// Tries to resolve a <see cref="WorkspaceManagerClass"/> using the same priority
        /// strategy as <see cref="TryGetWorkspaceModel(out WorkspaceModel, string)"/>.
        /// </summary>
        public static bool TryGetWorkspaceEditor(out WorkspaceManagerClass workspaceManager, string tabId = "")
        {
            return TryGetWorkspaceEditor(out workspaceManager, out _, tabId);
        }

        private static bool TryGetWorkspaceEditor(out WorkspaceManagerClass workspaceManager, out string effectiveTabId, string tabId = "")
        {
            workspaceManager = null;
            string explicitTabId = NormalizeTabId(tabId);
            string pinnedTabId = _preferredWorkspaceTabId.Value;
            effectiveTabId = pinnedTabId ?? explicitTabId;

            ICrypWinAdapter bridge = CrypWinPort.Instance;
            if (bridge is null)
            {
                Log.Debug("CrypWinPort instance is null.");
                return false;
            }

            IEditor editor = null;
            if (!string.IsNullOrWhiteSpace(pinnedTabId))
            {
                if (!bridge.TryGetWorkspaceEditorByTabId(pinnedTabId, out IEditor pinnedEditor))
                {
                    // Never redirect a request after its original workspace closes.
                    Log.Warning($"The request-pinned workspace tab is no longer available: {pinnedTabId}");
                    return false;
                }

                editor = pinnedEditor;
                effectiveTabId = pinnedTabId;
                if (!string.IsNullOrWhiteSpace(explicitTabId) &&
                    !string.Equals(explicitTabId, pinnedTabId, StringComparison.Ordinal))
                {
                    if (bridge.TryGetWorkspaceEditorByTabId(explicitTabId, out IEditor explicitEditor))
                    {
                        editor = explicitEditor;
                        effectiveTabId = explicitTabId;
                    }
                    else
                    {
                        Log.Warning($"Ignoring unavailable model-supplied workspace tab '{explicitTabId}' and retaining request pin '{pinnedTabId}'.");
                    }
                }
            }
            else if (string.IsNullOrWhiteSpace(effectiveTabId))
            {
                editor = bridge.ActiveEditor;
                if (editor == null)
                {
                    Log.Debug("No active editor available.");
                    return false;
                }
            }
            else if (!bridge.TryGetWorkspaceEditorByTabId(effectiveTabId, out editor))
            {
                Log.Warning($"The requested workspace tab is no longer available: {effectiveTabId}");
                return false;
            }

            if (editor is not WorkspaceManagerClass resolvedWorkspaceManager)
            {
                Log.Debug($"Editor (tabId: {effectiveTabId}) is not a WorkspaceManager.");
                return false;
            }

            workspaceManager = resolvedWorkspaceManager;
            return true;
        }

        private static string ResolveEffectiveTabId(string tabId)
        {
            return _preferredWorkspaceTabId.Value ?? NormalizeTabId(tabId);
        }

        /// <summary>
        /// Sanitizes conversational inputs derived from large language models.
        /// Subroutines routinely yield semantic approximations (e.g., 'active', 'default') when asked for technical IDs. 
        /// This method neutralizes those strings, prompting the upstream resolver to safely cascade to programmatic defaults.
        /// </summary>
        private static string NormalizeTabId(string tabId)
        {
            if (string.IsNullOrWhiteSpace(tabId))
            {
                return null;
            }

            string normalized = tabId.Trim();
            normalized = normalized.Trim('"', '\'');

            // Models frequently send semantic placeholders like "active".
            // We normalize them to null so resolution falls back to pinned/active editor.
            if (ActiveTabAliases.Contains(normalized))
            {
                return null;
            }

            return normalized;
        }

        private sealed class Scope : IDisposable
        {
            private Action _onDispose;

            public Scope(Action onDispose)
            {
                _onDispose = onDispose;
            }

            public void Dispose()
            {
                Action action = Interlocked.Exchange(ref _onDispose, null);
                action?.Invoke();
            }
        }
    }
}
