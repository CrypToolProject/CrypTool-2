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
/// This file implements the WorkspaceStatusPlugin, establishing a read-only Semantic Kernel toolkit.
/// It grants the LLM agent observational access to the active GUI state, component topologies, settings, 
/// and application diagnostics (logs). This enables context-aware generative planning without risking unsolicited runtime mutations.
/// </summary>

using CrypTool.CrypLLM.Helper;
using CrypTool.CrypLLM.Ports;
using CrypTool.CrypLLM.Services;
using CrypTool.CrypLLM.Threads;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using WorkspaceManager.Model;

namespace CrypTool.CrypLLM
{
    /// <summary>
    /// Analyzes read-only representations of workspace environments.
    /// Operates predominantly by invoking structural extraction procedures scheduled synchronously on the GUI dispatcher thread.
    /// </summary>
    internal sealed class WorkspaceStatusPlugin
    {
        #region KernelFunctions - Tabs

        /// <summary>
        /// Retrieves enumerations of currently active UI workspace tabs. Promotes context anchoring 
        /// when resolving ambiguous 'active application state' references by the AI.
        /// </summary>
        [KernelFunction("tabs_list")]
        [Description("Returns all open tabs of Cryptool 2. The user can see the content of the active tab.")]
        public string GetOpenEditors()
        {
            return LLMPluginService.InvokeOnUi(GetOpenEditorsInternal);
        }

        #endregion

        #region KernelFunctions - Workspace Models

        /// <summary>
        /// Captures the visible workspace viewport on the UI thread. Uses the same pinned
        /// workspace resolution as other tools; a closed tab never falls back to another tab.
        /// </summary>
        [KernelFunction("ws_screenshot")]
        [Description("Returns an actual PNG screenshot of the visible workspace viewport at its current zoom and scroll position. Use it to inspect the visual layout. Requires a vision-capable model. Optional tabId; the request-pinned workspace is used if omitted. The workspace tab must be visible.")]
        public string GetWorkspaceScreenshot(string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() =>
            {
                if (!LLMPluginService.TryGetWorkspaceEditor(out WorkspaceManager.WorkspaceManagerClass editor, tabId) ||
                    !(editor.Presentation is WorkspaceManager.View.Visuals.EditorVisual visual))
                {
                    return JsonConvert.SerializeObject(new { error = "Workspace not available for a screenshot." });
                }
                if (!(visual.FindName("ScrollViewer") is System.Windows.FrameworkElement viewport))
                    return JsonConvert.SerializeObject(new { error = "Workspace viewport not available." });
                return WorkspaceScreenshotContent.Capture(viewport);
            });
        }

        /// <summary>
        /// Generates a complete mathematical and relational topology map of the targeted workspace,
        /// including progress status and node interconnectedness, essential for functional analysis.
        /// </summary>
        [KernelFunction("ws_model")]
        [Description("Returns all elements of a workspace with execution progress as JSON string. Optional tabId: if omitted, active tab is used.")]
        public string GetWorkspaceModel(string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() => GetWorkspaceModelInternal(tabId));
        }

        #endregion

        #region KernelFunctions - Validation

        /// <summary>Check measured boxes, memo content and available orthogonal wire routes.</summary>
        [KernelFunction("ws_check_layout")]
        [Description("Checks occupied rectangles including connector rails/captions, memo clipping, and routes for crossings, shared segments and passage through ANY box, including the wire's own source/target. Wire penetration is an error. Returns IDs, connector names and conditional routingAdvice to move receivers or change port sides; recheck every branch of a shared output. Unavailable measurements are reported. Resolve errors before finishing.")]
        public string CheckWorkspaceLayout(double minimumGap = 40, string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() =>
            {
                if (double.IsNaN(minimumGap) || double.IsInfinity(minimumGap) || minimumGap < 0 || minimumGap > 500)
                    return JsonConvert.SerializeObject(new { error = "minimumGap must be finite and between 0 and 500." });
                if (!LLMPluginService.TryGetWorkspaceModel(out WorkspaceModel model, tabId))
                    return JsonConvert.SerializeObject(new { error = "Workspace not available." });
                return JsonConvert.SerializeObject(WorkspaceLayoutService.Check(model, minimumGap));
            });
        }

        /// <summary>
        /// Aggregates systemic compatibility errors between linked algorithms and components.
        /// Assists the agent in troubleshooting non-executable workflows autonomously.
        /// </summary>
        [KernelFunction("ws_validate")]
        [Description("Returns compact workspace validation JSON. Optional tabId: if omitted, active tab is used.")]
        public string GetWorkspaceValidation(string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() => GetWorkspaceValidationInternal(tabId));
        }

        #endregion

        #region KernelFunctions - IO/Bounds/Settings

        [KernelFunction("ws_io")]
        [ContextWindowToken(min: 5000)]
        [Description("Returns workspace text/number IO values by runtime ID. Optional tabId: if omitted, active tab is used.")]
        public string GetWorkspaceIoTexts(string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() => GetWorkspaceIoTextsInternal(tabId));
        }

        [KernelFunction("ws_settings")]
        [Description("Returns visible workspace component settings. Optional tabId: if omitted, active tab is used.")]
        public string GetWorkspaceComponentSettings(string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() => GetWorkspaceComponentSettingsInternal(tabId));
        }

        [KernelFunction("ws_bounds")]
        [ContextWindowToken(min: 5000)]
        [Description("Returns element MOVE anchors (x/y), resizable inner window width/height, stored/minimum/maximum sizes and sizeSource in canvas units. bodyBounds locates the actual box including its offset from the anchor; occupiedBounds also includes connector rails and captions. Use occupiedBounds for spacing/overlaps and bodyBounds for wire clearance. Zero stored size means automatic sizing. Optional tabId defaults to the request-pinned workspace.")]
        public string GetWorkspaceElementBounds(string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() => GetWorkspaceElementBoundsInternal(tabId));
        }

        #endregion

        #region Internals - Tabs

        /// <summary>
        /// Resolves tab data employing a preemptive snapshot cache optimized for rapid successive invocations during AI inference.
        /// Failing a cache hit, explicitly marshals queries out to the architectural bridge interop.
        /// </summary>
        private string GetOpenEditorsInternal()
        {
            try
            {
                // Optimization: AI threading environments periodically freeze states. Rely on these to avoid costly UI locks.
                if (AIThreadManager.Instance.TryGetCachedOpenTabsSnapshot(out string cachedOpenTabs))
                {
                    return cachedOpenTabs;
                }

                ICrypWinAdapter bridge = CrypWinPort.Instance;
                IEnumerable<OpenTabsAbstraction> openTabs = bridge.GetOpenTabs();
                return JsonConvert.SerializeObject(openTabs, Formatting.Indented);
            }
            catch (Exception ex)
            {
                string errormessage = "LLM-get_open_editors: Exception occurred -> " + ex.Message;
                Log.Error(errormessage);
                return errormessage;
            }
        }

        #endregion

        #region Internals - Workspace Models

        private string GetWorkspaceModelInternal(string tabId = "")
        {
            if (LLMPluginService.TryGetWorkspaceModel(out WorkspaceModel model, tabId))
            {
                WorkspaceModelAbstraction abstraction = AISchemaGenerator.CreateWorkspaceModelAbstraction(model);
                return JsonConvert.SerializeObject(abstraction, Formatting.Indented);
            }

            Log.Debug("get_open_workspace_model: No active workspace model found.");
            return JsonConvert.SerializeObject(new { error = "Workspace model not available." }, Formatting.Indented);
        }


        #endregion

        #region Internals - Validation

        private static string GetWorkspaceValidationInternal(string tabId = "")
        {
            if (!LLMPluginService.TryGetWorkspaceModel(out WorkspaceModel model, tabId))
            {
                return JsonConvert.SerializeObject(new { error = "Workspace model not available." }, Formatting.Indented);
            }

            string idOrNull = string.IsNullOrWhiteSpace(tabId) ? null : tabId;
            return WorkspaceInspector.CreateValidationJson(model, idOrNull);
        }

        #endregion

        #region Internals - IO/Bounds/Settings

        private static string GetWorkspaceIoTextsInternal(string tabId = "")
        {
            if (!LLMPluginService.TryGetWorkspaceModel(out WorkspaceModel model, tabId))
            {
                return "{}";
            }

            return WorkspaceInspector.CreateIoTextsJson(model);
        }

        private static string GetWorkspaceComponentSettingsInternal(string tabId = "")
        {
            if (!LLMPluginService.TryGetWorkspaceModel(out WorkspaceModel model, tabId))
            {
                return "{}";
            }

            return WorkspaceInspector.CreateComponentSettingsJson(model);
        }

        private static string GetWorkspaceElementBoundsInternal(string tabId = "")
        {
            if (!LLMPluginService.TryGetWorkspaceModel(out WorkspaceModel model, tabId))
            {
                return "{}";
            }

            return WorkspaceInspector.CreateWorkspaceElementBoundsJson(model);
        }

        #endregion

        #region Internals - Application Log

        /// <summary>
        /// Exposes raw application diagnostics to the model, aiding contextual inference regarding systemic execution exceptions.
        /// </summary>
        [KernelFunction("app_log_recent")]
        [Description("Returns recent in-memory GUI log entries.")]
        public string GetApplicationLog(int maxLines = 200)
        {
            return GetApplicationLogInternal(maxLines);
        }

        /// <summary>
        /// Reads recent GUI log entries from the application's in-memory log collection
        /// via CrypWinAccess and returns the newest <paramref name="maxLines"/> entries as JSON.
        /// Operation avoids disk file-system fallback to prevent asynchronous lock-contention exceptions.
        /// </summary>
        private static string GetApplicationLogInternal(int maxLines = 200)
        {
            try
            {
                int safeMaxLines = Math.Max(1, Math.Min(2000, maxLines));
                ICrypWinAdapter bridge = CrypWinPort.Instance;

                if (bridge == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        error = "CrypWinPort instance is null."
                    }, Formatting.Indented);
                }

                IList<string> lines = bridge.GetRecentLogMessages(safeMaxLines);

                return JsonConvert.SerializeObject(new
                {
                    source = "in_memory_gui_log",
                    returnedLines = lines?.Count ?? 0,
                    lines = lines ?? new List<string>()
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Log.Error("get_application_log failed: " + ex.Message);
                return JsonConvert.SerializeObject(new
                {
                    error = "Failed to read in-memory GUI log.",
                    message = ex.Message
                }, Formatting.Indented);
            }
        }

        #endregion
    }
}
