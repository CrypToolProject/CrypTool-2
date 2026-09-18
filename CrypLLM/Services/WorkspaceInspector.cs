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
/// This file implements the WorkspaceInspector, functioning as an extraction and translation layer.
/// It projects internal, platform-specific UI configurations (e.g. WPF coordinates, parameter schemas, input/output states)
/// into generic, parseable JSON representations designed exclusively for interpretation by Large Language Models.
/// </summary>

using CrypTool.PluginBase;
using CrypTool.Plugins.Numbers;
using CrypTool.TextInput;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using TextOutput;
using WorkspaceManager.Model;

namespace CrypTool.CrypLLM.Services
{
    /// <summary>
    /// Utility module encapsulating routines to serialize distinct segments of the active WorkspaceModel topology.
    /// By utilizing distinct JSON payloads configured to limit token verbosity, it ensures the generative AI 
    /// is supplied strictly with contextual structures relevant to its immediate task constraints.
    /// </summary>
    internal static class WorkspaceInspector
    {
        private sealed class SettingsSerializationContext
        {
            public bool FontOptionsAlreadySerialized { get; set; }
        }

        #region Constants

        private const string TextInputTypeFullName = "CrypTool.TextInput.TextInput";
        private const string TextOutputTypeFullName = "TextOutput.TextOutput";
        private const string NumberInputTypeFullName = "CrypTool.Plugins.Numbers.NumberInput";
        private const string NumberOutputTypeFullName = "CrypTool.Plugins.Numbers.NumberOutput";

        #endregion

        #region Public API (WorkspaceModel -> JSON)

        /// <summary>
        /// Scans the overarching workspace topology explicitly filtering for recognized input/output (IO) components.
        /// Extracts dynamic textual or numerical payloads to supply the orchestrator with active cipher and plaintext artifacts.
        /// </summary>
        /// <param name="model">The authoritative reference defining the algorithmic graph structure.</param>
        /// <returns>A formatted JSON mapping of transient component IDs to their respective string configurations.</returns>
        public static string CreateIoTextsJson(WorkspaceModel model)
        {
            try
            {
                if (model == null)
                {
                    return "{}";
                }

                IList<PluginModel> plugins = model.GetAllPluginModels();
                var result = new Dictionary<string, object>(StringComparer.Ordinal);

                foreach (PluginModel pm in plugins)
                {
                    if (pm == null)
                    {
                        continue;
                    }

                    string typeFullName = pm.PluginType?.FullName ?? string.Empty;
                    if (!IsSupportedIoComponentType(typeFullName))
                    {
                        continue;
                    }

                    string id = RuntimeHelpers.GetHashCode(pm).ToString();

                    result[id] = new Dictionary<string, object>(StringComparer.Ordinal)
                    {
                        { "type", pm.PluginType?.Name ?? string.Empty },
                        { "id", id },
                        { "text", TryExtractTextFromKnownSettingsTypes(pm) }
                    };
                }

                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Log.Warning($"WorkspaceInspector.CreateIoTextsJson failed: {ex.Message}");
                return "{}";
            }
        }

        public static string CreatePluginCoordinatesJson(WorkspaceModel model)
        {
            try
            {
                if (model == null)
                {
                    return "{}";
                }

                IList<PluginModel> plugins = model.GetAllPluginModels();
                var result = new Dictionary<string, object>(StringComparer.Ordinal);

                foreach (PluginModel pm in plugins)
                {
                    if (pm == null)
                    {
                        continue;
                    }

                    string id = RuntimeHelpers.GetHashCode(pm).ToString();
                    Point pos = pm.GetPosition();

                    result[id] = new Dictionary<string, object>(StringComparer.Ordinal)
                    {
                        ["id"] = id,
                        ["type"] = pm.PluginType?.FullName ?? string.Empty,
                        ["caption"] = pm.Plugin?.GetPluginInfoAttribute()?.Caption,
                        ["x"] = pos.X,
                        ["y"] = pos.Y,
                        ["width"] = pm.GetWidth(),
                        ["height"] = pm.GetHeight(),
                        ["zIndex"] = pm.ZIndex
                    };
                }

                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Log.Warning($"WorkspaceInspector.CreatePluginCoordinatesJson failed: {ex.Message}");
                return "{}";
            }
        }

        /// <summary>
        /// Harvests user-configurable UI parameters associated with embedded cryptographic modules.
        /// This ensures the LLM is exposed to modifiable control flow directives (e.g., toggle flags, dropdowns)
        /// mirroring exactly what the human operator visually perceives in the active application.
        /// </summary>
        /// <param name="model">The corresponding WorkspaceModel topology.</param>
        /// <returns>A JSON collection of component settings hierarchically mapped by identity hashes.</returns>
        public static string CreateComponentSettingsJson(WorkspaceModel model)
        {
            try
            {
                if (model == null)
                {
                    return "{}";
                }

                IList<PluginModel> plugins = model.GetAllPluginModels();
                var result = new Dictionary<string, object>(StringComparer.Ordinal);
                var serializationContext = new SettingsSerializationContext();

                foreach (PluginModel pm in plugins)
                {
                    if (pm?.Plugin == null)
                    {
                        continue;
                    }

                    IPlugin plugin = pm.Plugin as IPlugin;
                    if (plugin?.Settings == null)
                    {
                        continue;
                    }

                    string id = RuntimeHelpers.GetHashCode(pm).ToString();
                    Dictionary<string, object> entry = BuildVisibleSettingsEntry(plugin, pm, id, serializationContext);
                    if (entry != null)
                    {
                        result[id] = entry;
                    }
                }

                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Log.Warning($"WorkspaceInspector.CreateComponentSettingsJson failed: {ex.Message}");
                return "{}";
            }
        }

        /// <summary>
        /// Analyzes the structural correctness of the semantic workspace graph preceding compilation.
        /// Notably audits constraints such as detached mandatory input connectors to empower the LLM
        /// in autonomously recommending architectural diagnoses when workflows fail to execute.
        /// </summary>
        /// <param name="model">The corresponding workspace model to inspect.</param>
        /// <param name="tabId">Optional identifier associating the diagnostic output with a particular UI canvas.</param>
        /// <returns>A generalized JSON report summarizing component count discrepancies and obligatory connector oversights.</returns>
        public static string CreateValidationJson(WorkspaceModel model, string tabId = null)
        {
            try
            {
                if (model == null)
                {
                    return JsonConvert.SerializeObject(new { error = "Workspace model not available." }, Formatting.Indented);
                }

                IList<PluginModel> plugins = model.GetAllPluginModels();
                IList<ConnectionModel> connections = model.GetAllConnectionModels();

                var components = new List<Dictionary<string, object>>(plugins.Count);
                var unconnectedMandatoryInputs = new List<Dictionary<string, object>>();

                int totalInputs = 0;
                int totalMandatoryInputs = 0;
                int totalConnectedMandatoryInputs = 0;

                foreach (PluginModel p in plugins)
                {
                    if (p == null)
                    {
                        continue;
                    }

                    string id = RuntimeHelpers.GetHashCode(p).ToString();
                    string typeFullName = p.PluginType?.FullName ?? string.Empty;
                    string name = p.GetName();

                    components.Add(new Dictionary<string, object>(StringComparer.Ordinal)
                    {
                        ["id"] = id,
                        ["type"] = typeFullName,
                        ["name"] = name
                    });

                    foreach (ConnectorModel input in p.GetInputConnectors())
                    {
                        if (input == null)
                        {
                            continue;
                        }

                        totalInputs++;
                        if (!input.IsMandatory)
                        {
                            continue;
                        }

                        totalMandatoryInputs++;

                        int incomingCount = input.GetInputConnections()?.Count ?? 0;
                        if (incomingCount > 0)
                        {
                            totalConnectedMandatoryInputs++;
                            continue;
                        }

                        unconnectedMandatoryInputs.Add(new Dictionary<string, object>(StringComparer.Ordinal)
                        {
                            ["componentId"] = id,
                            ["componentType"] = typeFullName,
                            ["componentName"] = name,
                            ["inputName"] = input.PropertyName,
                            ["inputTitle"] = input.Caption,
                            ["inputDescription"] = input.ToolTip,
                            ["expectedType"] = input.ConnectorType?.FullName ?? "unknown"
                        });
                    }
                }

                int componentCount = components.Count;
                int connectionCount = connections?.Count ?? 0;

                var summary = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["tabId"] = tabId,
                    ["counts"] = new Dictionary<string, object>(StringComparer.Ordinal)
                    {
                        ["components"] = componentCount,
                        ["connections"] = connectionCount,
                        ["inputsTotal"] = totalInputs,
                        ["inputsMandatory"] = totalMandatoryInputs,
                        ["inputsMandatoryConnected"] = totalConnectedMandatoryInputs,
                        ["unconnectedMandatoryInputs"] = unconnectedMandatoryInputs.Count
                    },
                    ["unconnectedMandatoryInputs"] = unconnectedMandatoryInputs,
                    ["components"] = components
                };

                return JsonConvert.SerializeObject(summary, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Log.Warning($"WorkspaceInspector.CreateValidationJson failed: {ex.Message}");
                return JsonConvert.SerializeObject(new { error = ex.Message }, Formatting.Indented);
            }
        }

        public static string CreateWorkspaceElementBoundsJson(WorkspaceModel model)
        {
            try
            {
                if (model == null)
                {
                    return "{}";
                }

                IList<PluginModel> plugins = model.GetAllPluginModels();
                IList<TextModel> texts = model.GetAllTextModels();
                IList<ImageModel> images = model.GetAllImageModels();
                var result = new Dictionary<string, object>(StringComparer.Ordinal);

                foreach (PluginModel pm in plugins)
                {
                    if (pm == null)
                    {
                        continue;
                    }

                    string id = RuntimeHelpers.GetHashCode(pm).ToString();
                    AddBoundsEntry(result, id, "component", pm.PluginType?.FullName ?? string.Empty, pm.Plugin?.GetPluginInfoAttribute()?.Caption,
                        pm);
                }

                foreach (TextModel tm in texts)
                {
                    if (tm == null)
                    {
                        continue;
                    }

                    string id = RuntimeHelpers.GetHashCode(tm).ToString();
                    AddBoundsEntry(result, id, "text", tm.GetType().FullName, null, tm);
                }

                foreach (ImageModel im in images)
                {
                    if (im == null)
                    {
                        continue;
                    }

                    string id = RuntimeHelpers.GetHashCode(im).ToString();
                    AddBoundsEntry(result, id, "image", im.GetType().FullName, null, im);
                }

                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Log.Warning($"WorkspaceInspector.CreateWorkspaceElementBoundsJson failed: {ex.Message}");
                return "{}";
            }
        }

        #endregion

        #region IO Text Extraction

        private static bool IsSupportedIoComponentType(string typeFullName)
        {
            return string.Equals(typeFullName, TextInputTypeFullName, StringComparison.Ordinal)
                || string.Equals(typeFullName, TextOutputTypeFullName, StringComparison.Ordinal)
                || string.Equals(typeFullName, NumberInputTypeFullName, StringComparison.Ordinal)
                || string.Equals(typeFullName, NumberOutputTypeFullName, StringComparison.Ordinal);
        }

        private static string TryExtractTextFromKnownSettingsTypes(PluginModel pm)
        {
            object pluginInstance = pm.Plugin;

            if (pluginInstance is TextOutput.TextOutput textOutput)
            {
                return textOutput.CurrentValue ?? string.Empty;
            }

            if (pluginInstance is TextInput.TextInput textInput)
            {
                return textInput.TextOutput ?? string.Empty;
            }

            ISettings settings = (pluginInstance as IPlugin)?.Settings;
            if (settings == null)
            {
                return null;
            }

            if (settings is Plugins.Numbers.NumberInputSettings numberInputSettings)
            {
                return numberInputSettings.Number ?? string.Empty;
            }

            try
            {
                if (settings is CrypTool.TextInput.TextInputSettings textInputSettings)
                {
                    return textInputSettings.Text ?? string.Empty;
                }

                if (settings is TextOutput.TextOutputSettings)
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

        #endregion

        #region Settings Serialization (TaskPaneAttributes)

        /// <summary>
        /// Resolves reflection metadata provided by TaskPaneAttributes linked to corresponding plugin configurations.
        /// Intersects discrete algorithmic control logic constraints while systematically ordering UI enumerations dynamically.
        /// </summary>
        private static Dictionary<string, object> BuildVisibleSettingsEntry(
            IPlugin plugin,
            PluginModel pluginModel = null,
            string runtimeId = "",
            SettingsSerializationContext serializationContext = null)
        {
            try
            {
                ISettings settings = plugin.Settings;
                TaskPaneAttribute[] taskPaneAttributes = settings.GetSettingsProperties(plugin) ?? Array.Empty<TaskPaneAttribute>();

                List<VisibleSettingEntry> visibleSettings = new List<VisibleSettingEntry>(taskPaneAttributes.Length);
                foreach (TaskPaneAttribute tpa in taskPaneAttributes)
                {
                    if (tpa == null || string.IsNullOrEmpty(tpa.PropertyName))
                    {
                        continue;
                    }

                    object value = ReadSettingValue(settings, tpa, serializationContext);

                    visibleSettings.Add(new VisibleSettingEntry
                    {
                        PropertyName = tpa.PropertyName,
                        Caption = tpa.Caption,
                        Tooltip = tpa.ToolTip,
                        GroupName = tpa.HasGroupName ? tpa.GroupName : null,
                        Order = tpa.Order,
                        ControlType = tpa.ControlType.ToString(),
                        Value = value
                    });
                }

                visibleSettings = visibleSettings
                    .OrderBy(s => s.GroupName ?? string.Empty, StringComparer.CurrentCultureIgnoreCase)
                    .ThenBy(s => s.Order)
                    .ThenBy(s => s.Caption ?? string.Empty, StringComparer.CurrentCultureIgnoreCase)
                    .ToList();

                List<object> settingsAsDict = visibleSettings
                    .Select(s => (object)new Dictionary<string, object>(StringComparer.Ordinal)
                    {
                        ["propertyName"] = s.PropertyName,
                        ["caption"] = s.Caption,
                        ["tooltip"] = s.Tooltip,
                        ["groupName"] = s.GroupName,
                        ["controlType"] = s.ControlType,
                        ["value"] = s.Value
                    })
                    .ToList();

                return new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["id"] = runtimeId ?? string.Empty,
                    ["type"] = pluginModel?.PluginType?.FullName ?? plugin?.GetType().FullName ?? string.Empty,
                    ["caption"] = plugin?.GetPluginInfoAttribute()?.Caption,
                    ["settings"] = settingsAsDict
                };
            }
            catch
            {
                return null;
            }
        }

        private static object ReadSettingValue(ISettings settings, TaskPaneAttribute tpa, SettingsSerializationContext serializationContext = null)
        {
            try
            {
                if (settings == null || tpa == null)
                {
                    return null;
                }

                if (IsFontSetting(tpa))
                {
                    object rawFont = TryReadFontValue(settings, tpa);
                    return SerializeSettingValueWithOptionsIfNeeded(settings, tpa, rawFont, serializationContext);
                }

                if (tpa.PropertyInfo != null)
                {
                    object raw = tpa.PropertyInfo.GetValue(settings, null);
                    return SerializeSettingValueWithOptionsIfNeeded(settings, tpa, raw, serializationContext);
                }

                if (tpa.MethodInfo != null)
                {
                    return null;
                }

                PropertyInfo pi = settings.GetType().GetProperty(tpa.PropertyName, BindingFlags.Instance | BindingFlags.Public);
                if (pi != null)
                {
                    object raw = pi.GetValue(settings, null);
                    return SerializeSettingValueWithOptionsIfNeeded(settings, tpa, raw, serializationContext);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Font Handling

        /// <summary>
        /// Explicit identification method isolating graphical font enumeration parameters, 
        /// which are structurally decoupled from standard discrete types managed natively via reflection APIs.
        /// </summary>
        private static bool IsFontSetting(TaskPaneAttribute tpa)
        {
            return string.Equals(tpa.PropertyName, "Font", StringComparison.Ordinal);
        }

        private static object TryReadFontValue(ISettings settings, TaskPaneAttribute tpa)
        {
            try
            {
                if (tpa.PropertyInfo == null)
                {
                    return null;
                }

                int identifier = (int)tpa.PropertyInfo.GetValue(settings, null);

                if (settings is TextInputSettings textInputSettings)
                {
                    return (identifier >= 0 && identifier < textInputSettings.Fonts.Count) ? textInputSettings.Fonts[identifier] : null;
                }

                if (settings is TextOutputSettings textOutputSettings)
                {
                    return (identifier >= 0 && identifier < textOutputSettings.Fonts.Count) ? textOutputSettings.Fonts[identifier] : null;
                }

                if (settings is NumberInputSettings numberInputSettings)
                {
                    return (identifier >= 0 && identifier < numberInputSettings.Fonts.Count) ? numberInputSettings.Fonts[identifier] : null;
                }
            }
            catch
            {
                // ignore
            }

            return null;
        }

        #endregion

        #region ComboBox Options / Normalization

        private static object SerializeSettingValueWithOptionsIfNeeded(
            ISettings settings,
            TaskPaneAttribute tpa,
            object rawValue,
            SettingsSerializationContext serializationContext = null)
        {
            if (tpa == null)
            {
                return NormalizeSettingValue(rawValue);
            }

            if (tpa.ControlType != ControlType.ComboBox
                && tpa.ControlType != ControlType.DynamicComboBox
                && tpa.ControlType != ControlType.LanguageSelector)
            {
                return NormalizeSettingValue(rawValue);
            }

            List<string> options = GetComboBoxOptions(settings, tpa, rawValue);

            int? selectedIndex = TryGetSelectedIndex(rawValue, options);
            string selectedText = TryGetSelectedText(rawValue, options, selectedIndex);
            bool suppressOptions = IsFontSetting(tpa) &&
                                   serializationContext != null &&
                                   serializationContext.FontOptionsAlreadySerialized;

            var serialized = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["selectedIndex"] = selectedIndex,
                ["selectedText"] = selectedText,
                ["selectedRaw"] = NormalizeSettingValue(rawValue)
            };

            if (suppressOptions)
            {
                serialized["options"] = new[] { "see first font field" };
            }
            else
            {
                serialized["options"] = options.Count > 0 ? options : null;
            }

            if (IsFontSetting(tpa) &&
                serializationContext != null &&
                options.Count > 0)
            {
                serializationContext.FontOptionsAlreadySerialized = true;
            }

            return serialized;
        }

        private static List<string> GetComboBoxOptions(ISettings settings, TaskPaneAttribute tpa, object rawValue)
        {
            var options = new List<string>();

            try
            {
                if (tpa.ControlType == ControlType.LanguageSelector)
                {
                    foreach (LanguageStatisticsLib.Languages item in Enum.GetValues(typeof(LanguageStatisticsLib.Languages)))
                    {
                        options.Add(item.ToString());
                    }
                }

                if (IsFontSetting(tpa))
                {
                    if (settings is TextInputSettings textInputSettings)
                    {
                        options.AddRange(textInputSettings.Fonts.Where(f => !string.IsNullOrEmpty(f)));
                        return options;
                    }

                    if (settings is TextOutputSettings textOutputSettings)
                    {
                        options.AddRange(textOutputSettings.Fonts.Where(f => !string.IsNullOrEmpty(f)));
                        return options;
                    }

                    if (settings is NumberInputSettings numberInputSettings)
                    {
                        options.AddRange(numberInputSettings.Fonts.Where(f => !string.IsNullOrEmpty(f)));
                        return options;
                    }
                }

                if (tpa.ControlValues != null && tpa.ControlValues.Length > 0)
                {
                    options.AddRange(tpa.ControlValues.Where(s => !string.IsNullOrEmpty(s)));
                    return options;
                }

                if (rawValue is Enum enumValue)
                {
                    options.AddRange(Enum.GetValues(enumValue.GetType()).Cast<object>().Select(v => v.ToString()));
                    return options;
                }

                Type propertyType = tpa.PropertyInfo?.PropertyType ?? settings?.GetType().GetProperty(tpa.PropertyName)?.PropertyType;
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
                return (intValue >= 0 && intValue < options.Count) ? intValue : (int?)null;
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
            if (selectedIndex.HasValue
                && options != null
                && selectedIndex.Value >= 0
                && selectedIndex.Value < options.Count)
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

            Type t = value.GetType();
            if (t.IsEnum)
            {
                return value.ToString();
            }

            return value;
        }

        private static void AddBoundsEntry(
            Dictionary<string, object> result,
            string id,
            string elementKind,
            string type,
            string caption,
            VisualElementModel element)
        {
            ElementSize size = WorkspaceElementGeometry.Measure(element);
            Point position = element.GetPosition();
            result[id] = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["id"] = id,
                ["elementKind"] = elementKind,
                ["type"] = type,
                ["caption"] = caption,
                ["x"] = position.X,
                ["y"] = position.Y,
                ["width"] = size.Width,
                ["height"] = size.Height,
                ["storedWidth"] = element.GetWidth(),
                ["storedHeight"] = element.GetHeight(),
                ["minWidth"] = size.MinWidth,
                ["minHeight"] = size.MinHeight,
                ["maxWidth"] = size.MaxWidth,
                ["maxHeight"] = size.MaxHeight,
                ["sizeSource"] = size.SizeSource,
                ["zIndex"] = element.ZIndex
            };
        }

        #endregion

        #region Private Types

        private sealed class VisibleSettingEntry
        {
            public string PropertyName { get; set; }
            public string Caption { get; set; }
            public string Tooltip { get; set; }
            public string GroupName { get; set; }
            public int Order { get; set; }
            public string ControlType { get; set; }
            public object Value { get; set; }
        }

        #endregion
    }
}
