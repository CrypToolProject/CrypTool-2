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
/// This file implements the WorkspaceEditingPlugin, which exposes mutation-capable Semantic Kernel tools.
/// It empowers the LLM agent to programmatically assemble, modify, and orchestrate cryptographic workspaces 
/// by translating generative AI plans into standardized topological operations within the CrypTool 2 environment.
/// </summary>

using CrypTool.CrypLLM.Helper;
using CrypTool.CrypLLM.Services;
using CrypTool.PluginBase;
using CrypTool.Plugins.Numbers;
using CrypTool.TextInput;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using TextOutput;
using WorkspaceManager;
using WorkspaceManager.Model;
using WorkspaceManagerModel.Model.Operations;

namespace CrypTool.CrypLLM
{
    /// <summary>
    /// Encapsulates computational endpoints for active workspace manipulation. 
    /// Methods dynamically interact with the graphical canvas model to add, connect, config, or delete components.
    /// To ensure stability, operations systematically enforce execution-state safety (e.g., halting running models before editing).
    /// </summary>
    internal sealed class WorkspaceEditingPlugin
    {
        #region KernelFunctions - Active Workspace

        [KernelFunction("ws_add_component")]
        [AiToolMutation(true)]
        [ContextWindowToken(min: 10000)]
        [Description("Adds a component to a workspace. Optional name sets the visible short user-facing label/description text of the component. Optional tabId (active if omitted) and optional x/y position.")]
        public string AddComponentToWorkspace(string typeFullName, string name = null, double? x = null, double? y = null, string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() => AddComponentToWorkspaceInternal(typeFullName, name, x, y, tabId));
        }

        [KernelFunction("ws_add_connection")]
        [AiToolMutation(true)]
        [ContextWindowToken(min: 10000)]
        [Description("Creates a connection in a workspace. Optional tabId (active if omitted). Supports implicit conversions.")]
        public string CreateConnectionInWorkspace(string fromComponentId, string fromConnector, string toComponentId, string toConnector, string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() =>
                CreateConnectionInWorkspaceInternal(fromComponentId, fromConnector, toComponentId, toConnector, tabId));
        }

        [KernelFunction("ws_set_component_text")]
        [AiToolMutation(true)]
        [ContextWindowToken(min: 10000)]
        [Description("Sets a component text/number value by componentId. Optional tabId (active if omitted).")]
        public string SetComponentTextInWorkspace(string componentId, string text, string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() =>
                SetComponentTextInWorkspaceInternal(componentId, text, tabId));
        }

        [KernelFunction("ws_set_component_name")]
        [AiToolMutation(true)]
        [ContextWindowToken(min: 10000)]
        [Description("Sets the visible short user-facing label/description text of an existing component by componentId. Optional tabId (active if omitted).")]
        public string SetComponentNameInWorkspace(string componentId, string name, string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() =>
                SetComponentNameInWorkspaceInternal(componentId, name, tabId));
        }

        [KernelFunction("ws_set_component_setting")]
        [AiToolMutation(true)]
        [ContextWindowToken(min: 20000)]
        [Description("Sets one visible component setting by componentId and propertyName. Value is passed as text and converted to the target setting type. Optional tabId (active if omitted).")]
        public string SetComponentSettingInWorkspace(string componentId, string propertyName, string value, string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() =>
                SetComponentSettingInWorkspaceInternal(componentId, propertyName, value, tabId));
        }

        [KernelFunction("ws_add_memo")]
        [AiToolMutation(true)]
        [ContextWindowToken(min: 10000)]
        [Description("Adds a memo field with optional initial text and optional x/y position. Optional tabId (active if omitted).")]
        public string AddMemoToWorkspace(string text = null, double? x = null, double? y = null, string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() =>
                AddMemoToWorkspaceInternal(text, x, y, tabId));
        }

        [KernelFunction("ws_set_memo_text")]
        [AiToolMutation(true)]
        [ContextWindowToken(min: 10000)]
        [Description("Sets the plain text of a memo field by memoId. Optional tabId (active if omitted).")]
        public string SetMemoTextInWorkspace(string memoId, string text, string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() =>
                SetMemoTextInWorkspaceInternal(memoId, text, tabId));
        }

        [KernelFunction("ws_move_component")]
        [AiToolMutation(true)]
        [ContextWindowToken(min: 10000)]
        [Description("Moves a component to absolute x/y position. Optional tabId (active if omitted).")]
        public string MoveComponentInWorkspace(string componentId, double x, double y, string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() =>
                MoveComponentInWorkspaceInternal(componentId, x, y, tabId));
        }

        /// <summary>Moves an existing connector to a named component side without rewiring it.</summary>
        [KernelFunction("ws_set_connector_orientation")]
        [AiToolMutation(true)]
        [ContextWindowToken(min: 10000)]
        [Description("Moves an existing input or output connector to North (top), South (bottom), East (right), or West (left). Identify the component by componentId and connector by its technical Name/property from ws_model, not its translated caption. Orientation is case-insensitive. Preserves connector identity, data direction and existing connections; reroutes attached wires and supports Undo/Redo. Read current Orientation from ws_model and verify with ws_screenshot. Optional tabId; defaults to the request-pinned workspace.")]
        public string SetConnectorOrientationInWorkspace(string componentId, string connectorName, string orientation, string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() => SetConnectorOrientationInternal(componentId, connectorName, orientation, tabId));
        }

        /// <summary>Resizes an existing component without replacing its content or connections.</summary>
        [KernelFunction("ws_resize_component")]
        [AiToolMutation(true)]
        [ContextWindowToken(min: 10000)]
        [Description("Resizes an existing component by componentId to width/height in device-independent canvas units (not screen pixels). Expands icon mode to its presentation or settings if available. Sizes are clamped to the visual minimum/maximum; the result reports actual dimensions. Preserves content, position and connections, with Undo/Redo. Use ws_bounds and ws_screenshot to verify. Optional tabId; defaults to the request-pinned workspace.")]
        public string ResizeComponentInWorkspace(string componentId, double width, double height, string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() => ResizeWorkspaceElementInternal(componentId, width, height, false, tabId));
        }

        /// <summary>Resizes a memo's text box; changing its text is a separate operation.</summary>
        [KernelFunction("ws_resize_memo")]
        [AiToolMutation(true)]
        [ContextWindowToken(min: 10000)]
        [Description("Resizes an existing memo text box by memoId to width/height in device-independent canvas units. Preserves memo text and position, with Undo/Redo. Sizes are clamped to the visual minimum/maximum and reported in the result. Use ws_bounds and ws_screenshot to verify. Optional tabId; defaults to the request-pinned workspace.")]
        public string ResizeMemoInWorkspace(string memoId, double width, double height, string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() => ResizeWorkspaceElementInternal(memoId, width, height, true, tabId));
        }

        /// <summary>Use WPF text formatting at the chosen width, then verify the live memo viewport.</summary>
        [KernelFunction("ws_fit_memo")]
        [AiToolMutation(true)]
        [ContextWindowToken(min: 10000)]
        [Description("Fits an existing memo to its complete formatted content using actual text measurement and padding. Optional width in canvas units (default at least 600); preserves text, position and focus, with Undo/Redo. Returns fit verification and constraint limitations. Re-check spacing after growth. Optional tabId defaults to the request-pinned workspace.")]
        public string FitMemoToContent(string memoId, double? width = null, string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() =>
            {
                if (width.HasValue && (!WorkspaceElementGeometry.IsPositiveFinite(width.Value) || width.Value > 10000))
                    return JsonConvert.SerializeObject(new { success = false, error = "Width must be finite, positive and at most 10000." });
                if (!LLMPluginService.TryGetWorkspaceModel(out WorkspaceModel model, tabId))
                    return JsonConvert.SerializeObject(new { success = false, error = "Workspace not available." });
                TextModel memo = model.GetAllTextModels().FirstOrDefault(item => GetRuntimeId(item) == memoId);
                if (memo == null) return JsonConvert.SerializeObject(new { success = false, error = "Memo not found." });
                string stopError = EnsureWorkspaceStopped(tabId);
                if (stopError != null) return stopError;
                ElementSize size = WorkspaceElementGeometry.Measure(memo);
                double targetWidth = Math.Max(size.MinWidth, Math.Min(width ?? Math.Max(600, size.Width), size.MaxWidth ?? 10000));
                RichTextBox box = WorkspaceLayoutService.MemoBox(memo);
                double horizontalChrome = box != null && box.ActualWidth > 0 ? Math.Max(0, size.Width - box.ActualWidth) : 16;
                double verticalChrome = box != null && box.ActualHeight > 0 ? Math.Max(0, size.Height - box.ActualHeight) : 48;
                double required = WorkspaceLayoutService.MeasureMemoHeight(memo, Math.Max(1, targetWidth - horizontalChrome));
                double desired = Math.Max(size.MinHeight, required + verticalChrome + 16);
                var resize = Newtonsoft.Json.Linq.JObject.Parse(ResizeWorkspaceElementInternal(memoId, targetWidth, Math.Min(10000, desired), true, tabId));
                if (resize.Value<bool?>("success") != true) return resize.ToString();
                box = WorkspaceLayoutService.MemoBox(memo);
                box?.UpdateLayout();
                bool checkedLive = box != null && box.ActualWidth > 0 && box.ActualHeight > 0;
                bool fits = checkedLive && WorkspaceLayoutService.MeasureMemoHeight(memo, box.ActualWidth) <= box.ActualHeight + 1 && box.ExtentWidth <= box.ViewportWidth + 1;
                resize["contentFits"] = fits;
                resize["verifiedLive"] = checkedLive;
                resize["requiredHeight"] = desired;
                if (!fits) resize["limitation"] = checkedLive ? "Content still exceeds the available box; increase width or inspect size constraints." : "Live text viewport is unavailable; fit is measured but not visually verified.";
                return resize.ToString();
            });
        }

        [KernelFunction("ws_move_memo")]
        [AiToolMutation(true)]
        [ContextWindowToken(min: 10000)]
        [Description("Moves a memo field to absolute x/y position. Optional tabId (active if omitted).")]
        public string MoveMemoInWorkspace(string memoId, double x, double y, string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() =>
                MoveMemoInWorkspaceInternal(memoId, x, y, tabId));
        }

        [KernelFunction("ws_remove_component")]
        [AiToolMutation(true)]
        [ContextWindowToken(min: 10000)]
        [Description("Removes a component by componentId. Optional tabId (active if omitted).")]
        public string RemoveComponentFromWorkspace(string componentId, string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() =>
                RemoveComponentFromWorkspaceInternal(componentId, tabId));
        }

        [KernelFunction("ws_remove_memo")]
        [AiToolMutation(true)]
        [ContextWindowToken(min: 10000)]
        [Description("Removes a memo field by memoId. Optional tabId (active if omitted).")]
        public string RemoveMemoFromWorkspace(string memoId, string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() =>
                RemoveMemoFromWorkspaceInternal(memoId, tabId));
        }

        [KernelFunction("ws_remove_connection")]
        [AiToolMutation(true)]
        [ContextWindowToken(min: 10000)]
        [Description("Removes a connection between connectors. Optional tabId (active if omitted).")]
        public string RemoveConnectionInWorkspace(string fromComponentId, string fromConnector, string toComponentId, string toConnector, string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() =>
                RemoveConnectionInWorkspaceInternal(fromComponentId, fromConnector, toComponentId, toConnector, tabId));
        }

        [KernelFunction("ws_run")]
        [AiToolMutation(true)]
        [ContextWindowToken(min: 10000)]
        [Description("Starts workspace execution. Optional tabId (active if omitted).")]
        public string RunWorkspace(string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() => ControlWorkspaceExecutionInternal(start: true, tabId: tabId));
        }

        [KernelFunction("ws_stop")]
        [AiToolMutation(true)]
        [ContextWindowToken(min: 10000)]
        [Description("Stops workspace execution. Optional tabId (active if omitted).")]
        public string StopWorkspace(string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() => ControlWorkspaceExecutionInternal(start: false, tabId: tabId));
        }

        [KernelFunction("wait_seconds")]
        [AiToolMutation(true)]
        [ContextWindowToken(min: 10000)]
        [Description("Waits for N seconds before continuing with next tool calls.")]
        public string WaitSeconds(double seconds = 1)
        {
            double safeSeconds = Math.Max(0, Math.Min(60, seconds));
            int delayMs = (int)Math.Round(safeSeconds * 1000.0);

            Thread.Sleep(delayMs);

            return JsonConvert.SerializeObject(new
            {
                success = true,
                requestedSeconds = seconds,
                waitedSeconds = safeSeconds
            }, Formatting.Indented);
        }

        #endregion

        #region Internals - Active Workspace

        /// <summary>
        /// Governs the execution cycle (Start/Stop) of the designated workspace editor. 
        /// Prevents invalid state transitions by evaluating programmatic readiness flags prior to invocation.
        /// </summary>
        /// <param name="start">Boolean flag determining the requested execution transition (true for run, false for stop).</param>
        /// <param name="tabId">Target workspace identifier; defaults to the globally active interface.</param>
        /// <returns>A serialized JSON structured payload enumerating the outcome and subsequent execution capacities.</returns>
        private static string ControlWorkspaceExecutionInternal(bool start, string tabId = "")
        {
            if (!LLMPluginService.TryGetWorkspaceEditor(out WorkspaceManagerClass workspaceManager, tabId))
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    action = start ? "run" : "stop",
                    error = "Workspace editor not available."
                }, Formatting.Indented);
            }

            try
            {
                if (start)
                {
                    if (!workspaceManager.CanExecute)
                    {
                        return JsonConvert.SerializeObject(new
                        {
                            success = false,
                            action = "run",
                            error = "Workspace cannot be started in the current state.",
                            canStop = workspaceManager.CanStop
                        }, Formatting.Indented);
                    }

                    workspaceManager.Execute();
                    return JsonConvert.SerializeObject(new
                    {
                        success = true,
                        action = "run",
                        canExecute = workspaceManager.CanExecute,
                        canStop = workspaceManager.CanStop
                    }, Formatting.Indented);
                }

                if (!workspaceManager.CanStop)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        action = "stop",
                        error = "Workspace is not running.",
                        canExecute = workspaceManager.CanExecute
                    }, Formatting.Indented);
                }

                workspaceManager.Stop();
                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    action = "stop",
                    canExecute = workspaceManager.CanExecute,
                    canStop = workspaceManager.CanStop
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    action = start ? "run" : "stop",
                    error = ex.Message
                }, Formatting.Indented);
            }
        }

        /// <summary>
        /// Instantiates a new component on the active workspace layout.
        /// Identifies the appended instance by capturing pre- and post-mutation structural footprints (hash codes), 
        /// which facilitates immediate programmatic repositioning via the Editor's Operation history stack.
        /// </summary>
        private static string AddComponentToWorkspaceInternal(string typeFullName, string componentName, double? posX, double? posY, string tabId = "")
        {
            try
            {
                string stopError = EnsureWorkspaceStopped(tabId);
                if (stopError != null)
                {
                    return stopError;
                }

                if (string.IsNullOrWhiteSpace(typeFullName))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "typeFullName is required."
                    }, Formatting.Indented);
                }

                if (!LLMPluginService.TryGetWorkspaceEditor(out WorkspaceManagerClass workspaceManager, tabId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "Workspace editor not available."
                    }, Formatting.Indented);
                }

                if (!LLMPluginService.TryGetWorkspaceModel(out WorkspaceModel model, tabId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "Workspace model not available."
                    }, Formatting.Indented);
                }

                if (!ComponentInformations.AllLoadedPlugins.TryGetValue(typeFullName, out Type componentType) || componentType == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = $"Component type not found: {typeFullName}"
                    }, Formatting.Indented);
                }

                if (!typeof(ICrypComponent).IsAssignableFrom(componentType))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = $"Type is not an ICrypComponent: {typeFullName}"
                    }, Formatting.Indented);
                }

                List<PluginModel> existingPlugins = model.GetAllPluginModels().ToList();
                HashSet<int> existingHashes = new HashSet<int>(existingPlugins.Select(p => p.GetHashCode()));

                bool hasCustomPosition = posX.HasValue && posY.HasValue;
                Point targetPosition = hasCustomPosition
                    ? new Point(posX.Value, posY.Value)
                    : new Point(0, 0);

                workspaceManager.Add(componentType);

                PluginModel addedPlugin = model.GetAllPluginModels()
                    .FirstOrDefault(p => !existingHashes.Contains(p.GetHashCode()));

                if (addedPlugin != null)
                {
                    model.ModifyModel(new MoveModelElementOperation(addedPlugin, targetPosition), true);

                    string trimmedComponentName = string.IsNullOrWhiteSpace(componentName)
                        ? null
                        : componentName.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedComponentName) &&
                        !string.Equals(addedPlugin.GetName(), trimmedComponentName, StringComparison.Ordinal))
                    {
                        model.ModifyModel(new RenameModelElementOperation(addedPlugin, trimmedComponentName), true);
                    }
                }

                PluginInfoAttribute info = PluginExtension.GetPluginInfoAttribute(componentType);
                string caption = info != null ? info.Caption : componentType.Name;
                string effectiveComponentName = addedPlugin?.GetName() ?? caption;

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    type = componentType.FullName,
                    caption,
                    name = effectiveComponentName,
                    usedCustomPosition = hasCustomPosition,
                    position = new
                    {
                        x = targetPosition.X,
                        y = targetPosition.Y
                    }
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Log.Error($"add_component_to_workspace failed: {ex.Message}");
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    error = ex.Message
                }, Formatting.Indented);
            }
        }

        /// <summary>
        /// Eradicates a spatial component and safely severs its contiguous data transmission connections.
        /// Formulates deletion routines into a singular <see cref="MultiOperation"/> to maintain schema integrity 
        /// and ensure cohesive compatibility with the environment's telemetry rollback (Undo/Redo) systems.
        /// </summary>
        private static string RemoveComponentFromWorkspaceInternal(string componentId, string tabId = "")
        {
            try
            {
                string stopError = EnsureWorkspaceStopped(tabId);
                if (stopError != null)
                {
                    return stopError;
                }

                if (string.IsNullOrWhiteSpace(componentId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "componentId is required."
                    }, Formatting.Indented);
                }

                if (!LLMPluginService.TryGetWorkspaceModel(out WorkspaceModel model, tabId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "Workspace model not available."
                    }, Formatting.Indented);
                }

                PluginModel pluginModel = model.GetAllPluginModels()
                    .FirstOrDefault(p => GetRuntimeId(p) == componentId);

                if (pluginModel == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = $"component not found: {componentId}"
                    }, Formatting.Indented);
                }

                List<ConnectionModel> connections = pluginModel.GetInputConnectors()
                    .SelectMany(c => c.GetInputConnections())
                    .Concat(pluginModel.GetOutputConnectors().SelectMany(c => c.GetOutputConnections()))
                    .Distinct()
                    .ToList();

                var deleteOperations = new List<Operation>(connections.Count + 1);
                foreach (ConnectionModel connection in connections)
                {
                    deleteOperations.Add(new DeleteConnectionModelOperation(connection));
                }

                deleteOperations.Add(new DeletePluginModelOperation(pluginModel));

                model.ModifyModel(new MultiOperation(deleteOperations), true);

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    componentId,
                    removedConnections = connections.Count
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Log.Error($"remove_component_from_workspace failed: {ex.Message}");
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    error = ex.Message
                }, Formatting.Indented);
            }
        }

        private static string RemoveConnectionInWorkspaceInternal(string fromComponentId, string fromConnector, string toComponentId, string toConnector, string tabId = "")
        {
            try
            {
                string stopError = EnsureWorkspaceStopped(tabId);
                if (stopError != null)
                {
                    return stopError;
                }

                if (string.IsNullOrWhiteSpace(fromComponentId) || string.IsNullOrWhiteSpace(fromConnector) ||
                    string.IsNullOrWhiteSpace(toComponentId) || string.IsNullOrWhiteSpace(toConnector))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "fromComponentId, fromConnector, toComponentId and toConnector are required."
                    }, Formatting.Indented);
                }

                if (!LLMPluginService.TryGetWorkspaceModel(out WorkspaceModel model, tabId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "Workspace model not available."
                    }, Formatting.Indented);
                }

                List<PluginModel> plugins = model.GetAllPluginModels().ToList();

                PluginModel fromPlugin = plugins.FirstOrDefault(p => GetRuntimeId(p) == fromComponentId);
                if (fromPlugin == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = $"from component not found: {fromComponentId}"
                    }, Formatting.Indented);
                }

                PluginModel toPlugin = plugins.FirstOrDefault(p => GetRuntimeId(p) == toComponentId);
                if (toPlugin == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = $"to component not found: {toComponentId}"
                    }, Formatting.Indented);
                }

                ConnectorModel from = fromPlugin.GetOutputConnectors()
                    .FirstOrDefault(c => string.Equals(c.PropertyName, fromConnector, StringComparison.OrdinalIgnoreCase));
                if (from == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = $"output connector not found: {fromConnector}"
                    }, Formatting.Indented);
                }

                ConnectorModel to = toPlugin.GetInputConnectors()
                    .FirstOrDefault(c => string.Equals(c.PropertyName, toConnector, StringComparison.OrdinalIgnoreCase));
                if (to == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = $"input connector not found: {toConnector}"
                    }, Formatting.Indented);
                }

                ConnectionModel connection = from.GetOutputConnections().FirstOrDefault(c => c.To == to);
                if (connection == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "connection not found."
                    }, Formatting.Indented);
                }

                model.ModifyModel(new DeleteConnectionModelOperation(connection), true);

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    from = new
                    {
                        componentId = fromComponentId,
                        connector = from.PropertyName
                    },
                    to = new
                    {
                        componentId = toComponentId,
                        connector = to.PropertyName
                    }
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Log.Error($"remove_connection_in_workspace failed: {ex.Message}");
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    error = ex.Message
                }, Formatting.Indented);
            }
        }

        private static string SetComponentTextInWorkspaceInternal(string componentId, string text, string tabId = "")
        {
            try
            {
                string stopError = EnsureWorkspaceStopped(tabId);
                if (stopError != null)
                {
                    return stopError;
                }

                if (string.IsNullOrWhiteSpace(componentId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "componentId is required."
                    }, Formatting.Indented);
                }

                if (!LLMPluginService.TryGetWorkspaceModel(out WorkspaceModel model, tabId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "Workspace model not available."
                    }, Formatting.Indented);
                }

                PluginModel pluginModel = model.GetAllPluginModels()
                    .FirstOrDefault(p => GetRuntimeId(p) == componentId);

                if (pluginModel == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = $"component not found: {componentId}"
                    }, Formatting.Indented);
                }

                IPlugin plugin = pluginModel.Plugin as IPlugin;
                ISettings settings = plugin?.Settings;
                if (settings == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "component has no settings."
                    }, Formatting.Indented);
                }

                TaskPaneAttribute settingAttribute;
                PropertyInfo propertyInfo;

                if (TryResolveWritableVisibleSetting(plugin, settings, "Text", out settingAttribute, out propertyInfo, out string textSettingError))
                {
                    return ApplyVisibleComponentSetting(pluginModel, plugin, settings, settingAttribute, propertyInfo, text);
                }

                if (TryResolveWritableSettingProperty(settings, "Text", out propertyInfo, out string directTextSettingError))
                {
                    return ApplyVisibleComponentSetting(pluginModel, plugin, settings, null, propertyInfo, text);
                }

                if (TryResolveWritableVisibleSetting(plugin, settings, "Number", out settingAttribute, out propertyInfo, out string numberSettingError))
                {
                    return ApplyVisibleComponentSetting(pluginModel, plugin, settings, settingAttribute, propertyInfo, text);
                }

                if (TryResolveWritableSettingProperty(settings, "Number", out propertyInfo, out string directNumberSettingError))
                {
                    return ApplyVisibleComponentSetting(pluginModel, plugin, settings, null, propertyInfo, text);
                }

                return JsonConvert.SerializeObject(new
                {
                    componentId,
                    success = false,
                    error = "supported text setting not found (Text/Number, visible or direct).",
                    componentType = pluginModel.PluginType?.FullName,
                    visibleWritableProperties = GetVisibleWritableSettingPropertyNames(plugin, settings),
                    writablePublicProperties = GetWritableSettingPropertyNames(settings),
                    textSettingError,
                    directTextSettingError,
                    numberSettingError,
                    directNumberSettingError
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Log.Error($"set_component_text_in_workspace failed: {ex.Message}");
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    error = ex.Message
                }, Formatting.Indented);
            }
        }

        private static string SetComponentNameInWorkspaceInternal(string componentId, string name, string tabId = "")
        {
            try
            {
                string stopError = EnsureWorkspaceStopped(tabId);
                if (stopError != null)
                {
                    return stopError;
                }

                if (string.IsNullOrWhiteSpace(componentId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "componentId is required."
                    }, Formatting.Indented);
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "name is required."
                    }, Formatting.Indented);
                }

                if (!LLMPluginService.TryGetWorkspaceModel(out WorkspaceModel model, tabId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "Workspace model not available."
                    }, Formatting.Indented);
                }

                PluginModel pluginModel = model.GetAllPluginModels()
                    .FirstOrDefault(p => GetRuntimeId(p) == componentId);

                if (pluginModel == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = $"component not found: {componentId}"
                    }, Formatting.Indented);
                }

                string trimmedName = name.Trim();
                string previousName = pluginModel.GetName();

                if (string.Equals(previousName, trimmedName, StringComparison.Ordinal))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = true,
                        componentId,
                        previousName,
                        name = trimmedName,
                        changed = false
                    }, Formatting.Indented);
                }

                model.ModifyModel(new RenameModelElementOperation(pluginModel, trimmedName), true);

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    componentId,
                    previousName,
                    name = pluginModel.GetName(),
                    changed = true
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Log.Error($"set_component_name_in_workspace failed: {ex.Message}");
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    error = ex.Message
                }, Formatting.Indented);
            }
        }

        private static string SetComponentSettingInWorkspaceInternal(string componentId, string propertyName, string value, string tabId = "")
        {
            try
            {
                string stopError = EnsureWorkspaceStopped(tabId);
                if (stopError != null)
                {
                    return stopError;
                }

                if (string.IsNullOrWhiteSpace(componentId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "componentId is required."
                    }, Formatting.Indented);
                }

                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "propertyName is required.",
                        componentId
                    }, Formatting.Indented);
                }

                if (!TryResolveWorkspaceComponentSettings(componentId, tabId, out PluginModel pluginModel, out IPlugin plugin, out ISettings settings, out string resolveComponentError))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        componentId,
                        propertyName,
                        error = resolveComponentError
                    }, Formatting.Indented);
                }

                if (!TryResolveWritableVisibleSetting(plugin, settings, propertyName, out TaskPaneAttribute settingAttribute, out PropertyInfo propertyInfo, out string resolveSettingError))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        componentId,
                        propertyName,
                        error = resolveSettingError,
                        visibleWritableProperties = GetVisibleWritableSettingPropertyNames(plugin, settings)
                    }, Formatting.Indented);
                }

                return ApplyVisibleComponentSetting(pluginModel, plugin, settings, settingAttribute, propertyInfo, value);
            }
            catch (Exception ex)
            {
                Log.Error($"set_component_setting_in_workspace failed: {ex.Message}");
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    componentId,
                    propertyName,
                    error = ex.Message
                }, Formatting.Indented);
            }
        }

        private static string AddMemoToWorkspaceInternal(string text, double? x, double? y, string tabId = "")
        {
            try
            {
                string stopError = EnsureWorkspaceStopped(tabId);
                if (stopError != null)
                {
                    return stopError;
                }

                if (!LLMPluginService.TryGetWorkspaceModel(out WorkspaceModel model, tabId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "Workspace model not available."
                    }, Formatting.Indented);
                }

                // Selecting a new memo queues a delayed RichTextBox focus change on Loaded.
                // Suppress it at creation so the chat or the user's current control keeps focus.
                TextModel memoModel = model.ModifyModel(new NewTextModelOperation(false, text ?? string.Empty), true) as TextModel;
                if (memoModel == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "Failed to create memo field."
                    }, Formatting.Indented);
                }

                Point originalPosition = memoModel.GetPosition();
                Point targetPosition = new Point(x ?? originalPosition.X, y ?? originalPosition.Y);
                bool moved = x.HasValue || y.HasValue;

                if (moved)
                {
                    model.ModifyModel(new MoveModelElementOperation(memoModel, targetPosition), true);
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    memoId = GetRuntimeId(memoModel),
                    text = ReadPlainTextFromMemoModel(memoModel),
                    position = new { x = memoModel.GetPosition().X, y = memoModel.GetPosition().Y },
                    usedCustomPosition = moved
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Log.Error($"add_memo_to_workspace failed: {ex.Message}");
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    error = ex.Message
                }, Formatting.Indented);
            }
        }

        private static string SetMemoTextInWorkspaceInternal(string memoId, string text, string tabId = "")
        {
            try
            {
                string stopError = EnsureWorkspaceStopped(tabId);
                if (stopError != null)
                {
                    return stopError;
                }

                if (string.IsNullOrWhiteSpace(memoId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "memoId is required."
                    }, Formatting.Indented);
                }

                if (!LLMPluginService.TryGetWorkspaceModel(out WorkspaceModel model, tabId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "Workspace model not available."
                    }, Formatting.Indented);
                }

                TextModel memoModel = model.GetAllTextModels()
                    .FirstOrDefault(memo => GetRuntimeId(memo) == memoId);

                if (memoModel == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        memoId,
                        error = $"memo not found: {memoId}"
                    }, Formatting.Indented);
                }

                string previousText = ReadPlainTextFromMemoModel(memoModel);
                SetPlainTextInMemoModel(memoModel, text ?? string.Empty);

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    memoId,
                    previousText,
                    text = ReadPlainTextFromMemoModel(memoModel)
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Log.Error($"set_memo_text_in_workspace failed: {ex.Message}");
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    memoId,
                    error = ex.Message
                }, Formatting.Indented);
            }
        }

        private static string MoveComponentInWorkspaceInternal(string componentId, double x, double y, string tabId = "")
        {
            try
            {
                string stopError = EnsureWorkspaceStopped(tabId);
                if (stopError != null)
                {
                    return stopError;
                }

                if (string.IsNullOrWhiteSpace(componentId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "componentId is required."
                    }, Formatting.Indented);
                }

                if (!LLMPluginService.TryGetWorkspaceModel(out WorkspaceModel model, tabId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "Workspace model not available."
                    }, Formatting.Indented);
                }

                PluginModel pluginModel = model.GetAllPluginModels()
                    .FirstOrDefault(p => GetRuntimeId(p) == componentId);

                if (pluginModel == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = $"component not found: {componentId}"
                    }, Formatting.Indented);
                }

                Point oldPosition = pluginModel.GetPosition();
                Point newPosition = new Point(x, y);

                List<ConnectionModel> affectedConnections = pluginModel.GetInputConnectors()
                    .SelectMany(c => c.GetInputConnections())
                    .Concat(pluginModel.GetOutputConnectors().SelectMany(c => c.GetOutputConnections()))
                    .Distinct()
                    .ToList();

                foreach (ConnectionModel connection in affectedConnections)
                {
                    connection.IsCopy = true;
                    connection.PointList = null;
                }

                model.ModifyModel(new MoveModelElementOperation(pluginModel, newPosition), true);

                int rearrangedConnections = RearrangeConnectionVisuals(affectedConnections);

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    componentId,
                    from = new { x = oldPosition.X, y = oldPosition.Y },
                    to = new { x = newPosition.X, y = newPosition.Y },
                    rearrangedConnections
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Log.Error($"move_component_in_workspace failed: {ex.Message}");
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    error = ex.Message
                }, Formatting.Indented);
            }
        }

        /// <summary>
        /// Validates a named side and applies the native, undoable connector operation.
        /// </summary>
        private static string SetConnectorOrientationInternal(string componentId, string connectorName, string orientation, string tabId)
        {
            try
            {
                string side = orientation?.Trim();
                if (string.IsNullOrWhiteSpace(componentId) || string.IsNullOrWhiteSpace(connectorName))
                    return JsonConvert.SerializeObject(new { success = false, error = "componentId and connectorName are required." });
                // Accept only named sides; Enum.TryParse alone also accepts numeric values.
                if (!new[] { "North", "South", "East", "West" }.Contains(side, StringComparer.OrdinalIgnoreCase))
                    return JsonConvert.SerializeObject(new { success = false, error = "orientation must be North, South, East or West." });
                ConnectorOrientation requested = (ConnectorOrientation)Enum.Parse(typeof(ConnectorOrientation), side, true);
                if (!LLMPluginService.TryGetWorkspaceModel(out WorkspaceModel model, tabId))
                    return JsonConvert.SerializeObject(new { success = false, error = "Workspace model not available." });
                PluginModel component = model.GetAllPluginModels().FirstOrDefault(item => GetRuntimeId(item) == componentId);
                if (component == null)
                    return JsonConvert.SerializeObject(new { success = false, error = "Component not found." });
                List<ConnectorModel> matches = component.GetInputConnectors().Concat(component.GetOutputConnectors())
                    .Where(item => string.Equals(item.PropertyName, connectorName.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
                if (matches.Count != 1)
                    return JsonConvert.SerializeObject(new { success = false, error = matches.Count == 0 ? "Connector not found; use its technical Name from ws_model." : "Connector name is ambiguous." });
                string stopError = EnsureWorkspaceStopped(tabId);
                if (stopError != null) return stopError;
                ConnectorModel connector = matches[0];
                ConnectorOrientation previous = connector.Orientation == ConnectorOrientation.Unset
                    ? (connector.Outgoing ? ConnectorOrientation.East : ConnectorOrientation.West) : connector.Orientation;
                bool changed = previous != requested && (bool)model.ModifyModel(new SetConnectorOrientationOperation(connector, requested), true);
                if (previous != requested && !changed)
                    return JsonConvert.SerializeObject(new { success = false, error = "The workspace could not apply the connector orientation change." });
                return JsonConvert.SerializeObject(new
                {
                    success = true, componentId, connectorName = connector.PropertyName, changed,
                    from = previous.ToString(), orientation = requested.ToString(),
                    direction = connector.Outgoing ? "output" : "input",
                    attachedConnections = connector.GetInputConnections().Count + connector.GetOutputConnections().Count
                });
            }
            catch (Exception ex)
            {
                Log.Error($"set_connector_orientation failed: {ex.Message}");
                return JsonConvert.SerializeObject(new { success = false, error = ex.Message });
            }
        }

        /// <summary>Applies the native resize operation with visual notifications, persistence and Undo/Redo.</summary>
        private static string ResizeWorkspaceElementInternal(string elementId, double width, double height, bool memo, string tabId)
        {
            WorkspaceManager.View.Visuals.ComponentVisual expandedVisual = null;
            BinComponentState previousState = default;
            bool completed = false;
            try
            {
                if (string.IsNullOrWhiteSpace(elementId))
                    return JsonConvert.SerializeObject(new { success = false, error = memo ? "memoId is required." : "componentId is required." });
                if (!WorkspaceElementGeometry.IsPositiveFinite(width) || !WorkspaceElementGeometry.IsPositiveFinite(height) ||
                    width > 10000 || height > 10000)
                    return JsonConvert.SerializeObject(new { success = false, error = "Width and height must be finite, positive canvas dimensions of at most 10000 units." });
                if (!LLMPluginService.TryGetWorkspaceModel(out WorkspaceModel model, tabId))
                    return JsonConvert.SerializeObject(new { success = false, error = "Workspace model not available." });

                VisualElementModel element = memo
                    ? model.GetAllTextModels().FirstOrDefault(item => GetRuntimeId(item) == elementId)
                    : model.GetAllPluginModels().FirstOrDefault(item => GetRuntimeId(item) == elementId);
                if (element == null)
                    return JsonConvert.SerializeObject(new { success = false, elementId, error = memo ? "Memo not found." : "Component not found." });

                string stopError = EnsureWorkspaceStopped(tabId);
                if (stopError != null) return stopError;
                ElementSize previous = WorkspaceElementGeometry.Measure(element);
                bool viewExpanded = false;
                if (element.UpdateableView is WorkspaceManager.View.Visuals.ComponentVisual component)
                {
                    if (component.IsFullscreen)
                        return JsonConvert.SerializeObject(new { success = false, error = "Exit the component's fullscreen view before resizing its workspace box." });
                    if (component.State == BinComponentState.Min)
                    {
                        if (!component.HasComponentPresentation && !component.HasComponentSetting)
                            return JsonConvert.SerializeObject(new { success = false, error = "This component has only a fixed icon view and no resizable presentation or settings view." });
                        expandedVisual = component;
                        previousState = component.State;
                        component.State = component.HasComponentPresentation
                            ? BinComponentState.Presentation
                            : BinComponentState.Setting;
                        component.UpdateLayout();
                        viewExpanded = true;
                    }
                }

                ElementSize limits = WorkspaceElementGeometry.Measure(element);
                double targetWidth = Math.Min(Math.Max(width, limits.MinWidth), limits.MaxWidth ?? 10000);
                double targetHeight = Math.Min(Math.Max(height, limits.MinHeight), limits.MaxHeight ?? 10000);
                bool changed = (bool)model.ModifyModel(new ResizeModelElementOperation(element, targetWidth, targetHeight), true);
                WorkspaceElementGeometry.GetWindow(element)?.UpdateLayout();
                int rearrangedConnections = 0;
                if (element is PluginModel plugin)
                {
                    List<ConnectionModel> connections = plugin.GetInputConnectors().SelectMany(item => item.GetInputConnections())
                        .Concat(plugin.GetOutputConnectors().SelectMany(item => item.GetOutputConnections())).Distinct().ToList();
                    if (changed || viewExpanded)
                    {
                        foreach (ConnectionModel connection in connections)
                        {
                            connection.IsCopy = true;
                            connection.PointList = null;
                        }
                        rearrangedConnections = RearrangeConnectionVisuals(connections);
                    }
                }
                ElementSize actual = WorkspaceElementGeometry.Measure(element);
                completed = true;
                return JsonConvert.SerializeObject(new
                {
                    success = true, elementId, changed, viewExpanded,
                    from = new { width = previous.Width, height = previous.Height },
                    requested = new { width, height },
                    size = new { width = actual.Width, height = actual.Height },
                    storedSize = new { width = element.GetWidth(), height = element.GetHeight() },
                    adjustedToConstraints = !width.Equals(targetWidth) || !height.Equals(targetHeight),
                    position = new { x = element.GetPosition().X, y = element.GetPosition().Y },
                    rearrangedConnections
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Log.Error($"resize_workspace_element failed: {ex.Message}");
                return JsonConvert.SerializeObject(new { success = false, elementId, error = ex.Message });
            }
            finally
            {
                if (!completed && expandedVisual != null) expandedVisual.State = previousState;
            }
        }

        private static string MoveMemoInWorkspaceInternal(string memoId, double x, double y, string tabId = "")
        {
            try
            {
                string stopError = EnsureWorkspaceStopped(tabId);
                if (stopError != null)
                {
                    return stopError;
                }

                if (string.IsNullOrWhiteSpace(memoId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "memoId is required."
                    }, Formatting.Indented);
                }

                if (!LLMPluginService.TryGetWorkspaceModel(out WorkspaceModel model, tabId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "Workspace model not available."
                    }, Formatting.Indented);
                }

                TextModel memoModel = model.GetAllTextModels()
                    .FirstOrDefault(memo => GetRuntimeId(memo) == memoId);

                if (memoModel == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        memoId,
                        error = $"memo not found: {memoId}"
                    }, Formatting.Indented);
                }

                Point oldPosition = memoModel.GetPosition();
                Point newPosition = new Point(x, y);
                model.ModifyModel(new MoveModelElementOperation(memoModel, newPosition), true);

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    memoId,
                    from = new { x = oldPosition.X, y = oldPosition.Y },
                    to = new { x = newPosition.X, y = newPosition.Y }
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Log.Error($"move_memo_in_workspace failed: {ex.Message}");
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    memoId,
                    error = ex.Message
                }, Formatting.Indented);
            }
        }

        private static string RemoveMemoFromWorkspaceInternal(string memoId, string tabId = "")
        {
            try
            {
                string stopError = EnsureWorkspaceStopped(tabId);
                if (stopError != null)
                {
                    return stopError;
                }

                if (string.IsNullOrWhiteSpace(memoId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "memoId is required."
                    }, Formatting.Indented);
                }

                if (!LLMPluginService.TryGetWorkspaceModel(out WorkspaceModel model, tabId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "Workspace model not available."
                    }, Formatting.Indented);
                }

                TextModel memoModel = model.GetAllTextModels()
                    .FirstOrDefault(memo => GetRuntimeId(memo) == memoId);

                if (memoModel == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        memoId,
                        error = $"memo not found: {memoId}"
                    }, Formatting.Indented);
                }

                string currentText = ReadPlainTextFromMemoModel(memoModel);
                Point currentPosition = memoModel.GetPosition();

                model.ModifyModel(new DeleteTextModelOperation(memoModel), true);

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    memoId,
                    removedText = currentText,
                    position = new { x = currentPosition.X, y = currentPosition.Y }
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Log.Error($"remove_memo_from_workspace failed: {ex.Message}");
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    memoId,
                    error = ex.Message
                }, Formatting.Indented);
            }
        }

        /// <summary>
        /// Establishes cryptographic dataflow connections linking distinct components topologies.
        /// Resolves compatibility mappings and leverages bidirectional fallback validation in instances 
        /// where the generative agent inadvertently reverses the source and destination logical flow sequence.
        /// </summary>
        private static string CreateConnectionInWorkspaceInternal(string fromComponentId, string fromConnector, string toComponentId, string toConnector, string tabId = "")
        {
            try
            {
                string stopError = EnsureWorkspaceStopped(tabId);
                if (stopError != null)
                {
                    return stopError;
                }

                if (string.IsNullOrWhiteSpace(fromComponentId) || string.IsNullOrWhiteSpace(fromConnector) ||
                    string.IsNullOrWhiteSpace(toComponentId) || string.IsNullOrWhiteSpace(toConnector))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "fromComponentId, fromConnector, toComponentId and toConnector are required."
                    }, Formatting.Indented);
                }

                if (!LLMPluginService.TryGetWorkspaceModel(out WorkspaceModel model, tabId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "Workspace model not available."
                    }, Formatting.Indented);
                }

                List<PluginModel> plugins = model.GetAllPluginModels().ToList();

                PluginModel fromPlugin = plugins.FirstOrDefault(p => GetRuntimeId(p) == fromComponentId);
                if (fromPlugin == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = $"from component not found: {fromComponentId}"
                    }, Formatting.Indented);
                }

                PluginModel toPlugin = plugins.FirstOrDefault(p => GetRuntimeId(p) == toComponentId);
                if (toPlugin == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = $"to component not found: {toComponentId}"
                    }, Formatting.Indented);
                }

                if (!TryResolveConnectionEndpoints(
                        fromPlugin,
                        fromConnector,
                        toPlugin,
                        toConnector,
                        out ConnectorModel from,
                        out ConnectorModel to,
                        out bool autoSwapped,
                        out string resolveError))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = resolveError,
                        fromComponentId,
                        toComponentId,
                        fromConnectorCandidates = GetConnectorNames(fromPlugin.GetOutputConnectors(), fromPlugin.GetInputConnectors()),
                        toConnectorCandidates = GetConnectorNames(toPlugin.GetInputConnectors(), toPlugin.GetOutputConnectors())
                    }, Formatting.Indented);
                }

                if (from.GetOutputConnections().Any(c => c.To == to))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "connection already exists."
                    }, Formatting.Indented);
                }

                ConversionLevel conversionLevel = WorkspaceModel.compatibleConnectors(from, to);
                if (conversionLevel == ConversionLevel.Red || conversionLevel == ConversionLevel.NA)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = $"incompatible connector types: from={from.ConnectorType?.FullName ?? "null"}, to={to.ConnectorType?.FullName ?? "null"}"
                    }, Formatting.Indented);
                }

                Type fromType = from.ConnectorType ?? typeof(object);

                ConnectionModel connection = model.ModifyModel(
                    new NewConnectionModelOperation(from, to, fromType), true) as ConnectionModel;

                return JsonConvert.SerializeObject(new
                {
                    success = connection != null,
                    conversionLevel = conversionLevel.ToString(),
                    autoSwappedDirection = autoSwapped,
                    from = new
                    {
                        componentId = GetRuntimeId(from.PluginModel),
                        connector = from.PropertyName
                    },
                    to = new
                    {
                        componentId = GetRuntimeId(to.PluginModel),
                        connector = to.PropertyName
                    }
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Log.Error($"create_connection_in_workspace failed: {ex.Message}");
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    error = ex.Message
                }, Formatting.Indented);
            }
        }

        private static bool TryResolveWorkspaceComponentSettings(
            string componentId,
            string tabId,
            out PluginModel pluginModel,
            out IPlugin plugin,
            out ISettings settings,
            out string error)
        {
            pluginModel = null;
            plugin = null;
            settings = null;
            error = null;

            if (!LLMPluginService.TryGetWorkspaceModel(out WorkspaceModel model, tabId))
            {
                error = "Workspace model not available.";
                return false;
            }

            pluginModel = model.GetAllPluginModels()
                .FirstOrDefault(p => GetRuntimeId(p) == componentId);

            if (pluginModel == null)
            {
                error = $"component not found: {componentId}";
                return false;
            }

            plugin = pluginModel.Plugin as IPlugin;
            settings = plugin?.Settings;
            if (settings == null)
            {
                error = "component has no settings.";
                return false;
            }

            return true;
        }

        private static bool TryResolveWritableVisibleSetting(
            IPlugin plugin,
            ISettings settings,
            string propertyName,
            out TaskPaneAttribute settingAttribute,
            out PropertyInfo propertyInfo,
            out string error)
        {
            settingAttribute = null;
            propertyInfo = null;
            error = null;

            if (plugin == null || settings == null)
            {
                error = "component settings are not available.";
                return false;
            }

            TaskPaneAttribute[] taskPaneAttributes = settings.GetSettingsProperties(plugin) ?? Array.Empty<TaskPaneAttribute>();
            settingAttribute = taskPaneAttributes.FirstOrDefault(tpa =>
                tpa != null &&
                !string.IsNullOrWhiteSpace(tpa.PropertyName) &&
                string.Equals(tpa.PropertyName, propertyName, StringComparison.OrdinalIgnoreCase));

            if (settingAttribute == null)
            {
                error = $"visible setting not found: {propertyName}";
                return false;
            }

            propertyInfo = settingAttribute.PropertyInfo ??
                settings.GetType().GetProperty(settingAttribute.PropertyName, BindingFlags.Instance | BindingFlags.Public);

            if (propertyInfo == null)
            {
                if (settingAttribute.MethodInfo != null)
                {
                    error = $"setting '{settingAttribute.PropertyName}' is action-based and cannot be assigned with the current tool.";
                    return false;
                }

                error = $"setting '{settingAttribute.PropertyName}' is not backed by a public property.";
                return false;
            }

            if (!propertyInfo.CanWrite)
            {
                error = $"setting '{propertyInfo.Name}' is read-only.";
                return false;
            }

            return true;
        }

        private static bool TryResolveWritableSettingProperty(
            ISettings settings,
            string propertyName,
            out PropertyInfo propertyInfo,
            out string error)
        {
            propertyInfo = null;
            error = null;

            if (settings == null)
            {
                error = "component settings are not available.";
                return false;
            }

            propertyInfo = settings.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(pi =>
                    pi != null &&
                    string.Equals(pi.Name, propertyName, StringComparison.OrdinalIgnoreCase));

            if (propertyInfo == null)
            {
                error = $"public property not found: {propertyName}";
                return false;
            }

            if (!propertyInfo.CanWrite)
            {
                error = $"property '{propertyInfo.Name}' is read-only.";
                return false;
            }

            return true;
        }

        private static string ApplyVisibleComponentSetting(
            PluginModel pluginModel,
            IPlugin plugin,
            ISettings settings,
            TaskPaneAttribute settingAttribute,
            PropertyInfo propertyInfo,
            string inputValue)
        {
            string componentId = GetRuntimeId(pluginModel);
            object oldRawValue = TryReadSettingRawValue(settings, settingAttribute, propertyInfo);
            object oldDisplayValue = ReadSettingDisplayValue(settings, settingAttribute, propertyInfo, oldRawValue);

            if (!TryConvertSettingInput(
                    inputValue,
                    settings,
                    settingAttribute,
                    propertyInfo,
                    out object convertedValue,
                    out string acceptedInputMode,
                    out string conversionError))
            {
                List<string> options = GetComboBoxOptions(settings, settingAttribute, oldRawValue);
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    componentId,
                    property = propertyInfo.Name,
                    controlType = settingAttribute?.ControlType.ToString() ?? "DirectProperty",
                    error = conversionError,
                    currentValue = oldDisplayValue,
                    options = options.Count > 0 ? options : null
                }, Formatting.Indented);
            }

            propertyInfo.SetValue(settings, convertedValue, null);

            object newRawValue = TryReadSettingRawValue(settings, settingAttribute, propertyInfo);
            object newDisplayValue = ReadSettingDisplayValue(settings, settingAttribute, propertyInfo, newRawValue);

            return JsonConvert.SerializeObject(new
            {
                success = true,
                componentId,
                componentType = pluginModel?.PluginType?.FullName,
                componentCaption = plugin?.GetPluginInfoAttribute()?.Caption,
                property = propertyInfo.Name,
                caption = settingAttribute?.Caption,
                controlType = settingAttribute?.ControlType.ToString() ?? "DirectProperty",
                acceptedInputMode,
                previousValue = oldDisplayValue,
                value = newDisplayValue
            }, Formatting.Indented);
        }

        private static string[] GetVisibleWritableSettingPropertyNames(IPlugin plugin, ISettings settings)
        {
            if (plugin == null || settings == null)
            {
                return Array.Empty<string>();
            }

            return (settings.GetSettingsProperties(plugin) ?? Array.Empty<TaskPaneAttribute>())
                .Where(tpa => tpa != null && !string.IsNullOrWhiteSpace(tpa.PropertyName))
                .Select(tpa => tpa.PropertyInfo ??
                    settings.GetType().GetProperty(tpa.PropertyName, BindingFlags.Instance | BindingFlags.Public))
                .Where(pi => pi != null && pi.CanWrite)
                .Select(pi => pi.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static string[] GetWritableSettingPropertyNames(ISettings settings)
        {
            if (settings == null)
            {
                return Array.Empty<string>();
            }

            return settings.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(pi => pi != null && pi.CanWrite)
                .Select(pi => pi.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static object TryReadSettingRawValue(ISettings settings, TaskPaneAttribute settingAttribute, PropertyInfo propertyInfo)
        {
            try
            {
                if (propertyInfo != null)
                {
                    return propertyInfo.GetValue(settings, null);
                }

                if (settingAttribute?.MethodInfo != null)
                {
                    return null;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static object ReadSettingDisplayValue(ISettings settings, TaskPaneAttribute settingAttribute, PropertyInfo propertyInfo, object rawValue)
        {
            if (settingAttribute == null)
            {
                return NormalizeSettingValue(rawValue);
            }

            if (IsFontSetting(settingAttribute))
            {
                object rawFontValue = TryReadFontValue(settings, settingAttribute);
                return SerializeSettingValueWithOptionsIfNeeded(settings, settingAttribute, rawFontValue);
            }

            return SerializeSettingValueWithOptionsIfNeeded(settings, settingAttribute, rawValue);
        }

        private static bool TryConvertSettingInput(
            string inputValue,
            ISettings settings,
            TaskPaneAttribute settingAttribute,
            PropertyInfo propertyInfo,
            out object convertedValue,
            out string acceptedInputMode,
            out string error)
        {
            convertedValue = null;
            acceptedInputMode = null;
            error = null;

            if (propertyInfo == null)
            {
                error = "target setting property is not available.";
                return false;
            }

            Type targetType = propertyInfo.PropertyType;
            Type effectiveType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            string rawInput = inputValue ?? string.Empty;
            string trimmedInput = rawInput.Trim();

            if (Nullable.GetUnderlyingType(targetType) != null &&
                string.Equals(trimmedInput, "null", StringComparison.OrdinalIgnoreCase))
            {
                convertedValue = null;
                acceptedInputMode = "null";
                return true;
            }

            object currentRawValue = TryReadSettingRawValue(settings, settingAttribute, propertyInfo);
            List<string> options = GetComboBoxOptions(settings, settingAttribute, currentRawValue);
            bool restrictToKnownOptions = settingAttribute != null &&
                                          (IsFontSetting(settingAttribute) ||
                                           settingAttribute.ControlType == ControlType.ComboBox ||
                                           settingAttribute.ControlType == ControlType.LanguageSelector);

            if (options.Count > 0 &&
                TryConvertOptionInput(trimmedInput, effectiveType, options, out convertedValue, out acceptedInputMode))
            {
                return true;
            }

            if (restrictToKnownOptions && options.Count > 0)
            {
                error = $"value does not match a supported option for setting '{propertyInfo.Name}'.";
                return false;
            }

            string decodedString = TryDecodeQuotedJsonString(rawInput);

            if (effectiveType == typeof(string))
            {
                convertedValue = decodedString;
                acceptedInputMode = string.Equals(decodedString, rawInput, StringComparison.Ordinal)
                    ? "string"
                    : "json-string";
                return true;
            }

            if (effectiveType == typeof(bool))
            {
                if (TryParseBoolean(trimmedInput, out bool boolValue))
                {
                    convertedValue = boolValue;
                    acceptedInputMode = "bool";
                    return true;
                }

                error = $"value could not be converted to Boolean for setting '{propertyInfo.Name}'.";
                return false;
            }

            if (effectiveType.IsEnum)
            {
                if (TryParseEnumValue(trimmedInput, effectiveType, out object enumValue))
                {
                    convertedValue = enumValue;
                    acceptedInputMode = "enum";
                    return true;
                }

                error = $"value could not be converted to enum '{effectiveType.Name}' for setting '{propertyInfo.Name}'.";
                return false;
            }

            if (TryConvertUsingTypeConverter(trimmedInput, effectiveType, out object typedValue))
            {
                convertedValue = typedValue;
                acceptedInputMode = "typed";
                return true;
            }

            error = $"value could not be converted to {effectiveType.Name} for setting '{propertyInfo.Name}'.";
            return false;
        }

        private static bool TryConvertOptionInput(
            string inputValue,
            Type targetType,
            List<string> options,
            out object convertedValue,
            out string acceptedInputMode)
        {
            convertedValue = null;
            acceptedInputMode = null;

            if (options == null || options.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < options.Count; i++)
            {
                if (!string.Equals(options[i], inputValue, StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                if (TryCreateOptionBasedValue(targetType, options, i, out convertedValue))
                {
                    acceptedInputMode = "option-text";
                    return true;
                }

                return false;
            }

            if (int.TryParse(inputValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int optionIndex) ||
                int.TryParse(inputValue, NumberStyles.Integer, CultureInfo.CurrentCulture, out optionIndex))
            {
                if (optionIndex >= 0 && optionIndex < options.Count &&
                    TryCreateOptionBasedValue(targetType, options, optionIndex, out convertedValue))
                {
                    acceptedInputMode = "option-index";
                    return true;
                }
            }

            return false;
        }

        private static bool TryCreateOptionBasedValue(Type targetType, List<string> options, int optionIndex, out object convertedValue)
        {
            convertedValue = null;

            if (targetType == null || options == null || optionIndex < 0 || optionIndex >= options.Count)
            {
                return false;
            }

            string optionText = options[optionIndex];

            if (targetType == typeof(string))
            {
                convertedValue = optionText;
                return true;
            }

            if (targetType == typeof(int))
            {
                convertedValue = optionIndex;
                return true;
            }

            if (targetType.IsEnum && TryParseEnumValue(optionText, targetType, out object enumValue))
            {
                convertedValue = enumValue;
                return true;
            }

            return TryConvertUsingTypeConverter(optionText, targetType, out convertedValue);
        }

        private static bool TryParseBoolean(string inputValue, out bool value)
        {
            value = false;

            if (bool.TryParse(inputValue, out value))
            {
                return true;
            }

            if (string.Equals(inputValue, "1", StringComparison.Ordinal))
            {
                value = true;
                return true;
            }

            if (string.Equals(inputValue, "0", StringComparison.Ordinal))
            {
                value = false;
                return true;
            }

            return false;
        }

        private static bool TryParseEnumValue(string inputValue, Type enumType, out object value)
        {
            value = null;

            if (enumType == null || !enumType.IsEnum)
            {
                return false;
            }

            try
            {
                value = Enum.Parse(enumType, inputValue, true);
                return true;
            }
            catch
            {
                // ignore and try numeric fallback
            }

            try
            {
                Type underlyingType = Enum.GetUnderlyingType(enumType);
                if (TryConvertUsingTypeConverter(inputValue, underlyingType, out object numericValue))
                {
                    value = Enum.ToObject(enumType, numericValue);
                    return true;
                }
            }
            catch
            {
                // ignore
            }

            return false;
        }

        private static bool TryConvertUsingTypeConverter(string inputValue, Type targetType, out object value)
        {
            value = null;

            if (targetType == null)
            {
                return false;
            }

            try
            {
                TypeConverter converter = TypeDescriptor.GetConverter(targetType);
                if (converter != null && converter.CanConvertFrom(typeof(string)))
                {
                    value = converter.ConvertFromInvariantString(inputValue);
                    return true;
                }
            }
            catch
            {
                // ignore and try current culture below
            }

            try
            {
                TypeConverter converter = TypeDescriptor.GetConverter(targetType);
                if (converter != null && converter.CanConvertFrom(typeof(string)))
                {
                    value = converter.ConvertFromString(null, CultureInfo.CurrentCulture, inputValue);
                    return true;
                }
            }
            catch
            {
                // ignore
            }

            return false;
        }

        private static string TryDecodeQuotedJsonString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value ?? string.Empty;
            }

            string trimmed = value.Trim();
            if (trimmed.Length >= 2 &&
                trimmed[0] == '"' &&
                trimmed[trimmed.Length - 1] == '"')
            {
                try
                {
                    string decoded = JsonConvert.DeserializeObject<string>(trimmed);
                    if (decoded != null)
                    {
                        return decoded;
                    }
                }
                catch
                {
                    // ignore and return original value below
                }
            }

            return value;
        }

        private static bool IsFontSetting(TaskPaneAttribute settingAttribute)
        {
            return settingAttribute != null &&
                   string.Equals(settingAttribute.PropertyName, "Font", StringComparison.Ordinal);
        }

        private static object TryReadFontValue(ISettings settings, TaskPaneAttribute settingAttribute)
        {
            try
            {
                if (settings == null || settingAttribute?.PropertyInfo == null)
                {
                    return null;
                }

                int identifier = (int)settingAttribute.PropertyInfo.GetValue(settings, null);

                if (settings is TextInputSettings textInputSettings)
                {
                    return identifier >= 0 && identifier < textInputSettings.Fonts.Count
                        ? textInputSettings.Fonts[identifier]
                        : null;
                }

                if (settings is TextOutputSettings textOutputSettings)
                {
                    return identifier >= 0 && identifier < textOutputSettings.Fonts.Count
                        ? textOutputSettings.Fonts[identifier]
                        : null;
                }

                if (settings is NumberInputSettings numberInputSettings)
                {
                    return identifier >= 0 && identifier < numberInputSettings.Fonts.Count
                        ? numberInputSettings.Fonts[identifier]
                        : null;
                }
            }
            catch
            {
                // ignore
            }

            return null;
        }

        private static object SerializeSettingValueWithOptionsIfNeeded(ISettings settings, TaskPaneAttribute settingAttribute, object rawValue)
        {
            if (settingAttribute == null)
            {
                return NormalizeSettingValue(rawValue);
            }

            if (settingAttribute.ControlType != ControlType.ComboBox &&
                settingAttribute.ControlType != ControlType.DynamicComboBox &&
                settingAttribute.ControlType != ControlType.LanguageSelector)
            {
                return NormalizeSettingValue(rawValue);
            }

            List<string> options = GetComboBoxOptions(settings, settingAttribute, rawValue);
            int? selectedIndex = TryGetSelectedIndex(rawValue, options);
            string selectedText = TryGetSelectedText(rawValue, options, selectedIndex);

            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["selectedIndex"] = selectedIndex,
                ["selectedText"] = selectedText,
                ["selectedRaw"] = NormalizeSettingValue(rawValue),
                ["options"] = options.Count > 0 ? options : null
            };
        }

        private static List<string> GetComboBoxOptions(ISettings settings, TaskPaneAttribute settingAttribute, object rawValue)
        {
            var options = new List<string>();

            try
            {
                if (settingAttribute == null)
                {
                    return options;
                }

                if (settingAttribute.ControlType == ControlType.LanguageSelector)
                {
                    foreach (LanguageStatisticsLib.Languages item in Enum.GetValues(typeof(LanguageStatisticsLib.Languages)))
                    {
                        options.Add(item.ToString());
                    }
                }

                if (IsFontSetting(settingAttribute))
                {
                    if (settings is TextInputSettings textInputSettings)
                    {
                        options.AddRange(textInputSettings.Fonts.Where(font => !string.IsNullOrEmpty(font)));
                        return options;
                    }

                    if (settings is TextOutputSettings textOutputSettings)
                    {
                        options.AddRange(textOutputSettings.Fonts.Where(font => !string.IsNullOrEmpty(font)));
                        return options;
                    }

                    if (settings is NumberInputSettings numberInputSettings)
                    {
                        options.AddRange(numberInputSettings.Fonts.Where(font => !string.IsNullOrEmpty(font)));
                        return options;
                    }
                }

                if (settingAttribute.ControlValues != null && settingAttribute.ControlValues.Length > 0)
                {
                    options.AddRange(settingAttribute.ControlValues.Where(option => !string.IsNullOrEmpty(option)));
                    return options;
                }

                if (rawValue is Enum enumValue)
                {
                    options.AddRange(Enum.GetValues(enumValue.GetType()).Cast<object>().Select(v => v.ToString()));
                    return options;
                }

                Type propertyType = settingAttribute.PropertyInfo?.PropertyType ??
                    settings?.GetType().GetProperty(settingAttribute.PropertyName)?.PropertyType;
                if (propertyType != null && propertyType.IsEnum)
                {
                    options.AddRange(Enum.GetValues(propertyType).Cast<object>().Select(v => v.ToString()));
                }
            }
            catch
            {
                // ignore
            }

            return options;
        }

        private static int? TryGetSelectedIndex(object rawValue, List<string> options)
        {
            if (options == null || options.Count == 0 || rawValue == null)
            {
                return null;
            }

            if (rawValue is int intValue)
            {
                return intValue >= 0 && intValue < options.Count ? intValue : (int?)null;
            }

            string text = rawValue.ToString();
            for (int i = 0; i < options.Count; i++)
            {
                if (string.Equals(options[i], text, StringComparison.CurrentCultureIgnoreCase))
                {
                    return i;
                }
            }

            return null;
        }

        private static string TryGetSelectedText(object rawValue, List<string> options, int? selectedIndex)
        {
            if (selectedIndex.HasValue &&
                options != null &&
                selectedIndex.Value >= 0 &&
                selectedIndex.Value < options.Count)
            {
                return options[selectedIndex.Value];
            }

            return rawValue?.ToString();
        }

        private static object NormalizeSettingValue(object value)
        {
            if (value == null)
            {
                return null;
            }

            Type valueType = value.GetType();
            if (valueType.IsEnum)
            {
                return value.ToString();
            }

            return value;
        }

        private static string GetRuntimeId(object instance)
        {
            if (instance == null)
            {
                return string.Empty;
            }

            return RuntimeHelpers.GetHashCode(instance).ToString();
        }

        private static string ReadPlainTextFromMemoModel(TextModel memoModel)
        {
            if (memoModel == null)
            {
                return string.Empty;
            }

            RichTextBox richTextBox = null;
            bool usesLiveView = TryGetMemoRichTextBox(memoModel, out richTextBox);
            richTextBox = richTextBox ?? new RichTextBox();

            if (!usesLiveView)
            {
                memoModel.loadRTB(richTextBox);
            }

            if (richTextBox.Document == null)
            {
                richTextBox.Document = new FlowDocument();
            }

            string plainText = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).Text;
            return plainText?.TrimEnd('\r', '\n') ?? string.Empty;
        }

        private static void SetPlainTextInMemoModel(TextModel memoModel, string text)
        {
            if (memoModel == null)
            {
                return;
            }

            if (memoModel.WorkspaceModel != null)
            {
                memoModel.WorkspaceModel.ModifyModel(new ChangeMemoTextOperation(memoModel, text ?? string.Empty), true);
                return;
            }

            RichTextBox richTextBox = null;
            bool usesLiveView = TryGetMemoRichTextBox(memoModel, out richTextBox);
            richTextBox = richTextBox ?? new RichTextBox();

            if (!usesLiveView)
            {
                memoModel.loadRTB(richTextBox);
            }

            if (richTextBox.Document == null)
            {
                richTextBox.Document = new FlowDocument();
            }

            TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            textRange.Text = text ?? string.Empty;
            memoModel.saveRTB(richTextBox);
        }

        private static bool TryGetMemoRichTextBox(TextModel memoModel, out RichTextBox richTextBox)
        {
            richTextBox = null;

            try
            {
                object view = memoModel?.UpdateableView;
                if (view == null)
                {
                    return false;
                }

                FieldInfo richTextBoxField = view.GetType()
                    .GetField("mainRTB", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                richTextBox = richTextBoxField?.GetValue(view) as RichTextBox;
                return richTextBox != null;
            }
            catch (Exception ex)
            {
                Log.Debug($"resolve memo richtextbox failed: {ex.Message}");
                richTextBox = null;
                return false;
            }
        }

        private static bool TryResolveConnectionEndpoints(
            PluginModel fromPlugin,
            string fromConnector,
            PluginModel toPlugin,
            string toConnector,
            out ConnectorModel source,
            out ConnectorModel target,
            out bool autoSwapped,
            out string error)
        {
            source = fromPlugin.GetOutputConnectors()
                .FirstOrDefault(c => string.Equals(c.PropertyName, fromConnector, StringComparison.OrdinalIgnoreCase));
            target = toPlugin.GetInputConnectors()
                .FirstOrDefault(c => string.Equals(c.PropertyName, toConnector, StringComparison.OrdinalIgnoreCase));

            if (source != null && target != null)
            {
                autoSwapped = false;
                error = null;
                return true;
            }

            ConnectorModel swappedSource = toPlugin.GetOutputConnectors()
                .FirstOrDefault(c => string.Equals(c.PropertyName, toConnector, StringComparison.OrdinalIgnoreCase));
            ConnectorModel swappedTarget = fromPlugin.GetInputConnectors()
                .FirstOrDefault(c => string.Equals(c.PropertyName, fromConnector, StringComparison.OrdinalIgnoreCase));

            if (swappedSource != null && swappedTarget != null)
            {
                source = swappedSource;
                target = swappedTarget;
                autoSwapped = true;
                error = null;
                return true;
            }

            autoSwapped = false;
            error = "could not resolve connector direction. Use output connector for source and input connector for target.";
            return false;
        }

        private static string[] GetConnectorNames(IEnumerable<ConnectorModel> preferred, IEnumerable<ConnectorModel> secondary)
        {
            return preferred
                .Concat(secondary)
                .Select(c => c?.PropertyName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n)
                .ToArray();
        }

        /// <summary>
        /// Forces immediate topology rendering updates for connection lines succeeding spatial coordinate mutations (movements).
        /// Achieved dynamically via Reflection parameters because visual pathing methods reside within isolated, non-public presentation classes.
        /// </summary>
        private static int RearrangeConnectionVisuals(IEnumerable<ConnectionModel> connections)
        {
            int rearranged = 0;

            foreach (ConnectionModel connection in connections ?? Enumerable.Empty<ConnectionModel>())
            {
                try
                {
                    object view = connection?.UpdateableView;
                    if (view == null)
                    {
                        continue;
                    }

                    object line = view.GetType()
                        .GetProperty("Line", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        ?.GetValue(view, null);

                    object target = line ?? view;

                    MethodInfo rearrangeMethod = target.GetType()
                        .GetMethod("Rearrange", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    if (rearrangeMethod != null)
                    {
                        rearrangeMethod.Invoke(target, null);
                        rearranged++;
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug($"rearrange connection failed: {ex.Message}");
                }
            }

            return rearranged;
        }

        private static string EnsureWorkspaceStopped(string tabId = "")
        {
            if (!LLMPluginService.TryGetWorkspaceEditor(out WorkspaceManagerClass workspaceManager, tabId))
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    error = "Workspace editor not available."
                }, Formatting.Indented);
            }

            try
            {
                if (workspaceManager.CanStop)
                {
                    workspaceManager.Stop();
                }

                return null;
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    error = "Failed to stop workspace execution.",
                    message = ex.Message
                }, Formatting.Indented);
            }
        }

        #endregion
    }
}
