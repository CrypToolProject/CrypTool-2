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
/// This file implements the AISchemaGenerator, which projects the heavy, UI-coupled CrypTool 2 WorkspaceModel 
/// into a decoupled, lightweight Data Transfer Object (DTO) graph structure. 
/// It enables the LLM to contextually comprehend the cryptographic workflow topology independently of serialization constraints.
/// </summary>

using CrypTool.PluginBase;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Controls;
using System.Windows.Documents;
using WorkspaceManager.Execution;
using WorkspaceManager.Model;

namespace CrypTool.CrypLLM.Services
{
    /// <summary>
    /// Orchestrates the projection of a strictly-typed graph model from the active workspace.
    /// By returning generalized object abstractions rather than raw JSON strings, it decouples the semantic topology 
    /// from the underlying serialization engine, facilitating agnostic consumption by AI plugins.
    /// </summary>
    internal static class AISchemaGenerator
    {
        /// <summary>
        /// Projects an active <see cref="WorkspaceModel"/> into a detached <see cref="WorkspaceModelAbstraction"/>.
        /// It iteratively builds a mathematical directed graph (nodes = components, edges = connections) 
        /// while enforcing transient identity mappings using hash codes for LLM entity referencing.
        /// </summary>
        /// <param name="model">The primary structural workspace model to analyze.</param>
        /// <returns>A structured, serializable abstraction representing the workspace workflow.</returns>
        internal static WorkspaceModelAbstraction CreateWorkspaceModelAbstraction(WorkspaceModel model)
        {
            if (model == null)
            {
                return new WorkspaceModelAbstraction();
            }

            IList<PluginModel> plugins = model.GetAllPluginModels();
            IList<ConnectionModel> connections = model.GetAllConnectionModels();
            IList<TextModel> texts = model.GetAllTextModels();
            IList<ImageModel> images = model.GetAllImageModels();

            // Phase 1: Identity Mapping. Create transient, session-stable reference IDs utilizing memory hash codes
            // to enable reliable edge-to-node referential integrity within the abstracted graph boundaries.
            Dictionary<PluginModel, string> pluginId = new Dictionary<PluginModel, string>(plugins.Count);
            for (int i = 0; i < plugins.Count; i++)
            {
                pluginId[plugins[i]] = GetRuntimeId(plugins[i]);
            }

            Dictionary<ConnectionModel, string> connectionId = new Dictionary<ConnectionModel, string>(connections.Count);
            for (int i = 0; i < connections.Count; i++)
            {
                connectionId[connections[i]] = GetRuntimeId(connections[i]);
            }

            Dictionary<TextModel, string> textId = new Dictionary<TextModel, string>(texts.Count);
            for (int i = 0; i < texts.Count; i++)
            {
                textId[texts[i]] = GetRuntimeId(texts[i]);
            }

            Dictionary<ImageModel, string> imageId = new Dictionary<ImageModel, string>(images.Count);
            for (int i = 0; i < images.Count; i++)
            {
                imageId[images[i]] = GetRuntimeId(images[i]);
            }

            // Phase 2: Node Extraction. Project technical models into graph components containing discrete I/O topological boundaries.
            List<ComponentAbstraction> pluginNodes = new List<ComponentAbstraction>(plugins.Count);
            foreach (PluginModel p in plugins)
            {
                List<InputConnectorAbstraction> inputConnectors = new List<InputConnectorAbstraction>(p.GetInputConnectors().Count);
                foreach (ConnectorModel c in p.GetInputConnectors())
                {
                    inputConnectors.Add(new InputConnectorAbstraction
                    {
                        Name = c.PropertyName,
                        Title = c.Caption,
                        Description = c.ToolTip,
                        Type = c.ConnectorType?.FullName ?? "unknown",
                        Mandatory = c.IsMandatory,
                        Orientation = (c.Orientation == ConnectorOrientation.Unset ? ConnectorOrientation.West : c.Orientation).ToString(),
                        Control = c.IControl,
                        IncomingConnections = c.GetInputConnections().Select(cc => connectionId[cc]).ToList()
                    });
                }

                List<OutputConnectorAbstraction> outputConnectors = new List<OutputConnectorAbstraction>(p.GetOutputConnectors().Count);
                foreach (ConnectorModel c in p.GetOutputConnectors())
                {
                    outputConnectors.Add(new OutputConnectorAbstraction
                    {
                        Name = c.PropertyName,
                        Title = c.Caption,
                        Description = c.ToolTip,
                        Type = c.ConnectorType?.FullName ?? "unknown",
                        Control = c.IControl,
                        OutgoingConnections = c.GetOutputConnections().Select(cc => connectionId[cc]).ToList(),
                        Orientation = (c.Orientation == ConnectorOrientation.Unset ? ConnectorOrientation.East : c.Orientation).ToString()
                    });
                }

                pluginNodes.Add(new ComponentAbstraction
                {
                    Id = pluginId[p],
                    Type = p.PluginType?.Name ?? "unknown",
                    TypeInNaturalLanguage = p.PluginType?.GetPluginStringResource("PluginCaption") ?? "unknown",
                    Name = p.GetName(),
                    Bounds = WorkspaceElementGeometry.CreateBounds(p),
                    State = p.State.ToString(),
                    Progress = NormalizeProgress(p.PercentageFinished),
                    Inputs = inputConnectors,
                    Outputs = outputConnectors
                });
            }

            // Phase 3: Edge Extraction. Define the directed dataflow topology mapping component outputs to subsequent inputs.
            List<ConnectionAbstraction> connectionEdges = new List<ConnectionAbstraction>(connections.Count);
            foreach (ConnectionModel c in connections)
            {
                connectionEdges.Add(new ConnectionAbstraction
                {
                    Id = connectionId[c],
                    From = new EndpointRefAbstraction
                    {
                        ComponentId = pluginId[c.From.PluginModel],
                        Connector = c.From.PropertyName
                    },
                    To = new EndpointRefAbstraction
                    {
                        ComponentId = pluginId[c.To.PluginModel],
                        Connector = c.To.PropertyName
                    },
                    Type = c.ConnectionType?.FullName ?? c.From.ConnectorType?.FullName ?? "unknown",
                    Active = c.Active
                });
            }

            // Phase 4: Ancillary Elements Extraction. Harvest non-executable descriptive entities (memos and imagery) for spatial context.
            List<WorkspaceMemoAbstraction> textNodes = new List<WorkspaceMemoAbstraction>(texts.Count);
            foreach (TextModel t in texts)
            {
                string text = GetTextFromTextModel(t);

                textNodes.Add(new WorkspaceMemoAbstraction
                {
                    Id = textId[t],
                    HasData = t.HasData(),
                    Text = text,
                    Bounds = WorkspaceElementGeometry.CreateBounds(t)
                });
            }

            // Images
            List<WorkspaceImageAbstraction> imageNodes = new List<WorkspaceImageAbstraction>(images.Count);
            foreach (ImageModel im in images)
            {
                imageNodes.Add(new WorkspaceImageAbstraction
                {
                    Id = imageId[im]
                });
            }

            ExecutionEngine engine = null;
            try
            {
                var executionEngineProperty = typeof(WorkspaceModel).GetField("ExecutionEngine", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (executionEngineProperty != null)
                {
                    engine = executionEngineProperty.GetValue(model) as ExecutionEngine;
                }
            }
            catch
            {
                engine = null;
            }

            WorkspaceExecutionAbstraction execution = CreateExecutionAbstraction(model, engine, plugins);

            return new WorkspaceModelAbstraction
            {
                Execution = execution,
                WorkspaceIsBeingExecuted = engine != null && SafeIsRunning(engine),
                Components = pluginNodes,
                Connections = connectionEdges,
                Texts = textNodes,
                Images = imageNodes,
            };
        }

        /// <summary>
        /// Aggregates execution telemetry relative to the entire workspace construct.
        /// By inspecting the active execution engine, it provides the AI insight into dynamic operational states.
        /// </summary>
        private static WorkspaceExecutionAbstraction CreateExecutionAbstraction(WorkspaceModel model, ExecutionEngine engine, IList<PluginModel> plugins)
        {
            int globalProgress = CalculateGlobalProgressPercent(plugins);

            bool isRunning = SafeIsRunning(engine);

            if (engine == null)
            {
                return new WorkspaceExecutionAbstraction
                {
                    HasEngine = false,
                    IsRunning = model != null && isRunning,
                    GlobalProgress = globalProgress
                };
            }

            return new WorkspaceExecutionAbstraction
            {
                HasEngine = true,
                IsRunning = isRunning,
                GlobalProgress = globalProgress,
                BenchmarkPlugins = engine.BenchmarkPlugins,
                GuiUpdateInterval = engine.GuiUpdateInterval,
                SleepTime = engine.SleepTime
            };
        }

        /// <summary>
        /// Safely evaluates the engine's current execution status, preemptively suppressing state-transition anomalies.
        /// </summary>
        private static bool SafeIsRunning(ExecutionEngine engine)
        {
            try
            {
                return engine != null && engine.IsRunning();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Derives a macroscopic percentage gauge indicating overarching workspace processing completion, 
        /// utilizing mathematical averaging across all constituent algorithmic nodes.
        /// </summary>
        private static int CalculateGlobalProgressPercent(IList<PluginModel> plugins)
        {
            if (plugins == null || plugins.Count == 0)
            {
                return 0;
            }

            double total = 0;
            int count = 0;

            foreach (PluginModel p in plugins)
            {
                if (p == null)
                {
                    continue;
                }

                total += NormalizeProgress(p.PercentageFinished);
                count++;
            }

            if (count == 0)
            {
                return 0;
            }

            int percent = (int)Math.Round(total / count * 100.0, MidpointRounding.AwayFromZero);
            if (percent < 0)
            {
                return 0;
            }

            if (percent > 100)
            {
                return 100;
            }

            return percent;
        }

        private static double NormalizeProgress(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return 0;
            }

            if (value < 0)
            {
                return 0;
            }

            if (value > 1)
            {
                return 1;
            }

            return value;
        }

        private static string GetRuntimeId(object instance)
        {
            if (instance == null)
            {
                return string.Empty;
            }

            // Stable only within the current process/session; does NOT survive reload/restart.
            return RuntimeHelpers.GetHashCode(instance).ToString();
        }

        private static string GetTextFromTextModel(TextModel model)
        {
            if (model == null || !model.HasData())
            {
                return string.Empty;
            }

            // TextModel speichert ein XamlPackage -> über loadRTB() in ein FlowDocument laden
            RichTextBox rtb = new RichTextBox();
            model.loadRTB(rtb);

            if (rtb.Document == null)
            {
                return string.Empty;
            }

            TextRange range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            return range.Text ?? string.Empty;
        }
    }

    /// <summary>
    /// Represents the root container for a workspace snapshot, forming a bipartite graph 
    /// of algorithmic components and dataflow connections configured for semantic AI comprehension.
    /// </summary>
    [Description("Workspace graph snapshot containing all components and their relationships. " +
        "Components are connected to other components via connectors and connections. " +
        "Components have input and output connectors, and connections can only be connected to the connectors of the components.")]
    public sealed class WorkspaceModelAbstraction
    {
        [JsonPropertyName("execution")]
        [Description("Execution engine / runtime information for the workspace.")]
        public WorkspaceExecutionAbstraction Execution { get; set; }

        [JsonPropertyName("workspace_is_being_executed")]
        [Description("True if the workspace model indicates it is currently being executed (best-effort).")]
        public bool WorkspaceIsBeingExecuted { get; set; }

        [JsonPropertyName("component")]
        public List<ComponentAbstraction> Components { get; set; }

        [JsonPropertyName("connections")]
        public List<ConnectionAbstraction> Connections { get; set; }

        [JsonPropertyName("texts")]
        public List<WorkspaceMemoAbstraction> Texts { get; set; }

        [JsonPropertyName("images")]
        public List<WorkspaceImageAbstraction> Images { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    [Description("Execution-related info for the workspace.")]
    public sealed class WorkspaceExecutionAbstraction
    {
        [JsonPropertyName("has_engine")]
        public bool HasEngine { get; set; }

        [JsonPropertyName("is_running")]
        public bool IsRunning { get; set; }

        [JsonPropertyName("global_progress")]
        [Description("Global progress in percent (0..100), best-effort (average of PluginModel.PercentageFinished).")]
        public int GlobalProgress { get; set; }

        [JsonPropertyName("benchmark_plugins")]
        public bool BenchmarkPlugins { get; set; }

        [JsonPropertyName("gui_update_interval")]
        public int GuiUpdateInterval { get; set; }

        [JsonPropertyName("sleep_time")]
        public int SleepTime { get; set; }
    }

    [Description("A component with its connectors and metadata")]
    public sealed class ComponentAbstraction
    {
        [JsonPropertyName("bounds")]
        [Description("Canvas position and visible width/height in device-independent units, independent of zoom.")]
        public object Bounds { get; set; }
        [JsonPropertyName("id")]
        [Description("Runtime/session-stable ID derived from the underlying model object identity")]
        public string Id { get; set; }

        [JsonPropertyName("technical type")]
        public string Type { get; set; }

        [JsonPropertyName("technical type in natural language (localized)")]
        public string TypeInNaturalLanguage { get; set; }

        [JsonPropertyName("user given name")]
        public string Name { get; set; }

        [JsonPropertyName("state")]
        [Description("WorkspaceManager plugin model state (Normal/Warning/Error).")]
        public string State { get; set; }

        [JsonPropertyName("progress")]
        [Description("Execution progress in range [0..1] (best-effort).")]
        public double Progress { get; set; }

        [JsonPropertyName("inputs")]
        [Description("All input connectors of this component")]
        public List<InputConnectorAbstraction> Inputs { get; set; }

        [JsonPropertyName("outputs")]
        [Description("All output connectors of this component")]
        public List<OutputConnectorAbstraction> Outputs { get; set; }
    }

    [Description("Connector that accepts incoming data")]
    public sealed class InputConnectorAbstraction
    {
        [JsonPropertyName("orientation")]
        [Description("Component side: North (top), South (bottom), East (right), or West (left).")]
        public string Orientation { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("title")]
        [Description("Localized connector caption as shown in the UI.")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        [Description("Localized connector tooltip/description as shown in the UI.")]
        public string Description { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("mandatory")]
        [Description("Whether this input is required for execution")]
        public bool Mandatory { get; set; }

        [JsonPropertyName("control")]
        [Description("True if this is a control connector")]
        public bool Control { get; set; }

        [JsonPropertyName("incoming_connections")]
        [Description("Runtime/session-stable IDs of connections feeding this connector")]
        public List<string> IncomingConnections { get; set; }
    }

    [Description("Connector that emits outgoing data")]
    public sealed class OutputConnectorAbstraction
    {
        [JsonPropertyName("orientation")]
        [Description("Component side: North (top), South (bottom), East (right), or West (left).")]
        public string Orientation { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("title")]
        [Description("Localized connector caption as shown in the UI.")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        [Description("Localized connector tooltip/description as shown in the UI.")]
        public string Description { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("control")]
        [Description("True if this is a control connector")]
        public bool Control { get; set; }

        [JsonPropertyName("outgoing_connections")]
        [Description("Runtime/session-stable IDs of connections leaving this connector")]
        public List<string> OutgoingConnections { get; set; }
    }

    [Description("A directed edge between two component connectors")]
    public sealed class ConnectionAbstraction
    {
        [JsonPropertyName("id")]
        [Description("Runtime/session-stable ID derived from the underlying model object identity")]
        public string Id { get; set; }

        [JsonPropertyName("from")]
        [Description("Source connector reference")]
        public EndpointRefAbstraction From { get; set; }

        [JsonPropertyName("to")]
        [Description("Target connector reference")]
        public EndpointRefAbstraction To { get; set; }

        [JsonPropertyName("data_type")]
        public string Type { get; set; }

        [JsonPropertyName("active")]
        [Description("Whether the connection is currently active (data flowing)")]
        public bool Active { get; set; }
    }

    [Description("A reference to a connector")]
    public sealed class EndpointRefAbstraction
    {
        [JsonPropertyName("component_id")]
        public string ComponentId { get; set; }

        [JsonPropertyName("connector")]
        [Description("Connector/property name")]
        public string Connector { get; set; }
    }

    [Description("A memo field")]
    public sealed class WorkspaceMemoAbstraction
    {
        [JsonPropertyName("bounds")]
        [Description("Memo position and visible width/height in device-independent canvas units.")]
        public object Bounds { get; set; }
        [JsonPropertyName("id")]
        [Description("Runtime/session-stable ID derived from the underlying model object identity")]
        public string Id { get; set; }

        [JsonPropertyName("has_data")]
        [Description("True if the text model contains data")]
        public bool HasData { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    [Description("An image element")]
    public sealed class WorkspaceImageAbstraction
    {
        [JsonPropertyName("id")]
        [Description("Runtime/session-stable ID derived from the underlying model object identity")]
        public string Id { get; set; }
    }
}
