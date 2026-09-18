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
/// This file implements the ComponentCatalogPlugin, a metadata discovery interface accessible to the LLM agent.
/// It provides the foundational knowledge structure facilitating the dynamic exploration of 
/// cryptographic components, their technical specifications, configuration parameters, and detailed algorithmic documentation.
/// </summary>

using CrypTool.CrypLLM.Helper;
using CrypTool.CrypLLM.Services;
using CrypTool.PluginBase;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CrypTool.CrypLLM
{
    /// <summary>
    /// Semantic Kernel plugin exposing global component catalog/documentation (workspace independent).
    /// By acting neutrally relative to the active workspace, it allows the LLM to hypothesize solutions
    /// relying on plugins that might not yet be present on the active canvas.
    /// </summary>
    internal sealed class ComponentCatalogPlugin
    {
        #region Constants / Caches

        /// <summary>
        /// Explicit synchronization primitive ensuring thread-safety during concurrent cache read/write operations
        /// when multiple asynchronous SK requests evaluate metadata simultaneously.
        /// </summary>
        private static readonly object CacheLock = new object();

        /// <summary>
        /// In-memory caches storing serialized JSON data to circumvent repeated computationally expensive reflection iterations.
        /// </summary>
        private static readonly Dictionary<string, string> ComponentsByCategoryCacheJson = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly Dictionary<string, string> ComponentFullNamesCacheJson = new Dictionary<string, string>(StringComparer.Ordinal);

        #endregion

        #region KernelFunctions - Catalog

        /// <summary>
        /// Iterates through registered plugins and compiles an aggregated list categorized by functional domains.
        /// Dispatches the execution request securely to the GUI thread relying on LLMPluginService.
        /// </summary>
        [KernelFunction("comp_list")]
        [ContextWindowToken(min: 10000)]
        [Description("Lists components by category. Default output is compact captions; set includeTooltips=true for caption+tooltip. Optional category filters one key. Categories: CiphersClassic, CiphersModernSymmetric, CiphersModernAsymmetric, Steganography, HashFunctions, CryptanalysisSpecific, CryptanalysisGeneric, Protocols, ToolsBoolean, ToolsDataflow, ToolsDataInputOutput, ToolsRandomNumbers, ToolsCodes, ToolsMisc, Undefined.")]
        public string GetAvailableWorkspaceComponents(bool includeTooltips = false, string category = null)
        {
            return LLMPluginService.InvokeOnUi(() => GetAvailableWorkspaceComponentsInternal(includeTooltips, category));
        }

        /// <summary>
        /// Specialized overload providing a bounded context limitation for smaller token windows.
        /// </summary>
        [KernelFunction("comp_list_by_category")]
        [Description("Lists components by category. Categories: CiphersClassic, CiphersModernSymmetric, CiphersModernAsymmetric, Steganography, HashFunctions, CryptanalysisSpecific, CryptanalysisGeneric, Protocols, ToolsBoolean, ToolsDataflow, ToolsDataInputOutput, ToolsRandomNumbers, ToolsCodes, ToolsMisc, Undefined.")]
        public string GetAvailableWorkspaceComponents(string category)
        {
            return LLMPluginService.InvokeOnUi(() => GetAvailableWorkspaceComponentsInternal(false, category));
        }

        /// <summary>
        /// Retrieves the exact underlying CLR Type full names for available tools.
        /// Allows the agent to form reliable operational addresses when needing to instantiate models internally.
        /// </summary>
        [KernelFunction("comp_fullnames")]
        [ContextWindowToken(min: 10000)]
        [Description("Returns a JSON array of full type names (System.Type.FullName) for all workspace components, ordered by caption.")]
        public string GetComponentFullNames()
        {
            return LLMPluginService.InvokeOnUi(GetComponentFullNamesInternal);
        }

        #endregion

        #region KernelFunctions - Documentation / Defaults

        /// <summary>
        /// Resolves and fetches deep offline textual HTML documentation for a specific component.
        /// Facilitates complex cryptographic understanding and parameter constraint awareness for the AI.
        /// </summary>
        [KernelFunction("comp_docs")]
        [Description("Returns a JSON object containing the generated HTML online help for the given component (Type.FullName). Returns documentation in the current UI culture and English ('en').")]
        public string GetComponentDocumentationHtml(string typeFullName)
        {
            return LLMPluginService.InvokeOnUi(() => GetComponentDocumentationHtmlInternal(typeFullName));
        }

        /// <summary>
        /// Reads the schema metadata of component parameters and acceptable ranges, bypassing existing workspace configurations.
        /// Essential for forming structurally sound parameter modifications or generation plans.
        /// </summary>
        [KernelFunction("comp_defaults")]
        [Description("Returns a JSON object containing the (default) TaskPane settings for a component identified by its technical type name (System.Type.FullName). This is independent from any workspace. The result reflects the preconfiguration right after instantiation (constructor + optional Settings.Initialize()). JSON keys: type, caption, tooltip, settings[]. Each setting includes caption, tooltip, propertyName, controlType, groupName, value and (for combo boxes) options/selectedIndex/selectedText.")]
        public string GetComponentDefaultSettings(string typeFullName)
        {
            return LLMPluginService.InvokeOnUi(() => GetComponentDefaultSettingsInternal(typeFullName));
        }

        #endregion

        #region Internals - Catalog

        private string GetAvailableWorkspaceComponentsInternal(bool includeTooltips, string category)
        {
            try
            {
                string cultureKey = CultureInfo.CurrentUICulture?.Name ?? Properties.Settings.Default.defaultCultureKey;
                string normalizedCategory = string.IsNullOrWhiteSpace(category) ? string.Empty : category.Trim();
                string cacheKey = string.Concat(cultureKey, "|", includeTooltips ? "details" : "compact", "|", normalizedCategory.ToLowerInvariant());
                lock (CacheLock)
                {
                    if (ComponentsByCategoryCacheJson.TryGetValue(cacheKey, out string cached))
                    {
                        return cached;
                    }
                }

                IEnumerable<Type> componentTypes = ComponentInformations
                    .AllLoadedPlugins
                    .Values
                    .Where(t => typeof(ICrypComponent).IsAssignableFrom(t))
                    .Distinct();

                Dictionary<string, List<Dictionary<string, string>>> byCategory = BuildComponentsByCategory(componentTypes);

                var comparer = StringComparer.Create(CultureInfo.CurrentUICulture ?? CultureInfo.CurrentCulture, true);

                foreach (string key in byCategory.Keys.ToList())
                {
                    byCategory[key] = byCategory[key]
                        .OrderBy(d => d.TryGetValue("caption", out string caption) ? caption : string.Empty, comparer)
                        .ToList();
                }

                Dictionary<string, List<Dictionary<string, string>>> ordered = byCategory
                    .OrderBy(kvp => kvp.Key, comparer)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, byCategory.Comparer);

                object resultObject;
                if (!string.IsNullOrWhiteSpace(normalizedCategory))
                {
                    if (!ordered.TryGetValue(normalizedCategory, out List<Dictionary<string, string>> selected))
                    {
                        var hit = ordered.FirstOrDefault(k => string.Equals(k.Key, normalizedCategory, StringComparison.OrdinalIgnoreCase));
                        if (!string.IsNullOrEmpty(hit.Key))
                        {
                            selected = hit.Value;
                            normalizedCategory = hit.Key;
                        }
                    }

                    if (selected == null)
                    {
                        resultObject = includeTooltips
                            ? new Dictionary<string, List<Dictionary<string, string>>>(StringComparer.Ordinal)
                            : new Dictionary<string, List<string>>(StringComparer.Ordinal);
                    }
                    else
                    {
                        resultObject = includeTooltips
                            ? new Dictionary<string, List<Dictionary<string, string>>>(StringComparer.Ordinal) { [normalizedCategory] = selected }
                            : new Dictionary<string, List<string>>(StringComparer.Ordinal)
                            {
                                [normalizedCategory] = selected
                                    .Select(d => d.TryGetValue("caption", out string caption) ? caption : string.Empty)
                                    .ToList()
                            };
                    }
                }
                else
                {
                    resultObject = includeTooltips
                        ? (object)ordered
                        : ordered.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Select(d => d.TryGetValue("caption", out string caption) ? caption : string.Empty).ToList(),
                            StringComparer.Ordinal);
                }

                string json = JsonConvert.SerializeObject(resultObject, Formatting.None);

                lock (CacheLock)
                {
                    ComponentsByCategoryCacheJson[cacheKey] = json;
                }

                return json;
            }
            catch (Exception ex)
            {
                Log.Warning($"get_available_workspace_components failed: {ex.Message}");
                return "{}";
            }
        }

        public string GetComponentFullNamesInternal()
        {
            try
            {
                string cultureKey = CultureInfo.CurrentUICulture?.Name ?? Properties.Settings.Default.defaultCultureKey;
                lock (CacheLock)
                {
                    if (ComponentFullNamesCacheJson.TryGetValue(cultureKey, out string cached))
                    {
                        return cached;
                    }
                }

                IEnumerable<Type> componentTypes = ComponentInformations
                    .AllLoadedPlugins
                    .Values
                    .Where(t => typeof(ICrypComponent).IsAssignableFrom(t))
                    .Distinct();

                var comparer = StringComparer.Create(CultureInfo.CurrentUICulture ?? CultureInfo.CurrentCulture, true);

                List<string> fullNames = componentTypes
                    .Select(t =>
                    {
                        PluginInfoAttribute info = PluginExtension.GetPluginInfoAttribute(t);
                        string caption = info != null ? info.Caption : t.Name;
                        return new { Type = t, Caption = caption };
                    })
                    .OrderBy(x => x.Caption, comparer)
                    .ThenBy(x => x.Type.FullName, StringComparer.InvariantCultureIgnoreCase)
                    .Select(x => x.Type.FullName)
                    .ToList();

                string json = JsonConvert.SerializeObject(fullNames, Formatting.Indented);

                lock (CacheLock)
                {
                    ComponentFullNamesCacheJson[cultureKey] = json;
                }

                return json;
            }
            catch (Exception ex)
            {
                Log.Warning($"get_all_component_fullnames failed: {ex.Message}");
                return "[]";
            }
        }

        #endregion

        #region Internals - Documentation

        public string GetComponentDocumentationHtmlInternal(string typeFullName)
        {
            try
            {
                if (!ComponentInformations.AllLoadedPlugins.TryGetValue(typeFullName, out Type componentType) || componentType == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        error = $"Component type not found: {typeFullName}"
                    }, Formatting.Indented);
                }

                PluginInfoAttribute pluginInfo = PluginExtension.GetPluginInfoAttribute(componentType);
                string caption = pluginInfo != null ? pluginInfo.Caption : componentType.Name;
                string tooltip = pluginInfo != null ? pluginInfo.ToolTip : null;

                string uiCulture = CultureInfo.CurrentUICulture?.TwoLetterISOLanguageName;
                if (string.IsNullOrWhiteSpace(uiCulture))
                {
                    uiCulture = Properties.Settings.Default.defaultCultureKey;
                }

                var culturesToReturn = new List<string> { uiCulture };
                if (!string.Equals(uiCulture, Properties.Settings.Default.defaultCultureKey, StringComparison.OrdinalIgnoreCase))
                {
                    culturesToReturn.Add(Properties.Settings.Default.defaultCultureKey);
                }

                var culturesObject = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (string langTwoLetter in culturesToReturn.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    culturesObject[langTwoLetter] = ReadOrGeneratePluginHelp(componentType, caption, tooltip, langTwoLetter);
                }

                return JsonConvert.SerializeObject(new
                {
                    caption,
                    tooltip,
                    cultures = culturesObject
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message }, Formatting.Indented);
            }
        }

        private static object ReadOrGeneratePluginHelp(Type componentType, string caption, string tooltip, string langTwoLetter)
        {
            try
            {
                string htmlRelative = OnlineDocumentationGenerator.Generators.HtmlGenerator.OnlineHelp.GetPluginDocFilename(componentType, langTwoLetter);

                string baseDir = AppDomain.CurrentDomain.BaseDirectory ?? Environment.CurrentDirectory;
                string helpDir = OnlineDocumentationGenerator.Generators.HtmlGenerator.OnlineHelp.HelpDirectory;
                string htmlFullPath = System.IO.Path.Combine(baseDir, helpDir, htmlRelative);

                if (!System.IO.File.Exists(htmlFullPath))
                {
                    try
                    {
                        var docGenerator = new OnlineDocumentationGenerator.DocGenerator();
                        var htmlGenerator = new OnlineDocumentationGenerator.Generators.HtmlGenerator.HtmlGenerator(componentType);
                        docGenerator.Generate(baseDir, htmlGenerator);
                    }
                    catch (Exception genEx)
                    {
                        return new
                        {
                            caption,
                            tooltip,
                            culture = langTwoLetter,
                            html = (string)null,
                            htmlFile = (string)null,
                            generationError = genEx.Message
                        };
                    }
                }

                if (!System.IO.File.Exists(htmlFullPath))
                {
                    return new
                    {
                        caption,
                        tooltip,
                        culture = langTwoLetter,
                        html = (string)null,
                        htmlFile = (string)null,
                        generationError = (string)null
                    };
                }

                string htmlContent = System.IO.File.ReadAllText(htmlFullPath, Encoding.UTF8);

                return new
                {
                    caption,
                    tooltip,
                    culture = langTwoLetter,
                    html = htmlContent,
                    htmlFile = htmlFullPath,
                    generationError = (string)null
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    caption,
                    tooltip,
                    culture = langTwoLetter,
                    html = (string)null,
                    htmlFile = (string)null,
                    generationError = ex.Message
                };
            }
        }

        #endregion

        #region Internals - Default Settings (TaskPane)

        private string GetComponentDefaultSettingsInternal(string typeFullName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(typeFullName))
                {
                    return JsonConvert.SerializeObject(new { error = "typeFullName is required" }, Formatting.Indented);
                }

                if (!ComponentInformations.AllLoadedPlugins.TryGetValue(typeFullName, out Type componentType) || componentType == null)
                {
                    return JsonConvert.SerializeObject(new { error = $"Component type not found: {typeFullName}" }, Formatting.Indented);
                }

                if (!typeof(ICrypComponent).IsAssignableFrom(componentType))
                {
                    return JsonConvert.SerializeObject(new { error = $"Type is not an ICrypComponent: {typeFullName}" }, Formatting.Indented);
                }

                // Ephemeral allocation of a component instance dedicated specifically to exposing and extracting parameter metadata.
                // Disposed implicitly as instances are unrestrained by the global workspace container.
                ICrypComponent component = null;
                IPlugin plugin = null;
                try
                {
                    component = componentType.CreateComponentInstance();
                    plugin = component as IPlugin;
                }
                catch (Exception ex)
                {
                    return JsonConvert.SerializeObject(new { error = $"Failed to create instance: {ex.Message}" }, Formatting.Indented);
                }

                if (plugin == null)
                {
                    return JsonConvert.SerializeObject(new { error = $"Component does not implement IPlugin: {typeFullName}" }, Formatting.Indented);
                }

                PluginInfoAttribute pluginInfo = PluginExtension.GetPluginInfoAttribute(componentType);
                string caption = pluginInfo != null ? pluginInfo.Caption : componentType.Name;
                string tooltip = pluginInfo != null ? pluginInfo.ToolTip : null;

                ISettings settings = plugin.Settings;
                if (settings == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        type = componentType.FullName,
                        caption,
                        tooltip,
                        settings = new object[0]
                    }, Formatting.Indented);
                }

                try
                {
                    settings.Initialize();
                }
                catch
                {
                    // some settings implementations may assume being hosted; ignore
                }

                Dictionary<string, object> entry = BuildVisibleSettingsEntry(plugin, componentType.FullName, caption);

                if (entry == null)
                {
                    return JsonConvert.SerializeObject(new { error = "Failed to build settings entry" }, Formatting.Indented);
                }

                return JsonConvert.SerializeObject(entry, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message }, Formatting.Indented);
            }
        }

        private static Dictionary<string, object> BuildVisibleSettingsEntry(IPlugin plugin, string typeFullName, string caption)
        {
            try
            {
                ISettings settings = plugin.Settings;
                TaskPaneAttribute[] taskPaneAttributes = settings.GetSettingsProperties(plugin) ?? Array.Empty<TaskPaneAttribute>();

                var visibleSettings = new List<Dictionary<string, object>>(taskPaneAttributes.Length);

                foreach (TaskPaneAttribute tpa in taskPaneAttributes)
                {
                    if (tpa == null || string.IsNullOrEmpty(tpa.PropertyName))
                    {
                        continue;
                    }

                    object value = ReadSettingValue(settings, tpa);

                    visibleSettings.Add(new Dictionary<string, object>(StringComparer.Ordinal)
                    {
                        ["propertyName"] = tpa.PropertyName,
                        ["caption"] = tpa.Caption,
                        ["tooltip"] = tpa.ToolTip,
                        ["groupName"] = tpa.HasGroupName ? tpa.GroupName : null,
                        ["controlType"] = tpa.ControlType.ToString(),
                        ["value"] = value
                    });
                }

                return new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["id"] = string.Empty,
                    ["type"] = typeFullName ?? string.Empty,
                    ["caption"] = caption,
                    ["settings"] = visibleSettings
                };
            }
            catch
            {
                return null;
            }
        }

        private static object ReadSettingValue(ISettings settings, TaskPaneAttribute tpa)
        {
            try
            {
                if (settings == null || tpa == null)
                {
                    return null;
                }

                if (tpa.PropertyInfo != null)
                {
                    object raw = tpa.PropertyInfo.GetValue(settings, null);
                    return SerializeSettingValueWithOptionsIfNeeded(settings, tpa, raw);
                }

                if (tpa.MethodInfo != null)
                {
                    return null;
                }

                PropertyInfo pi = settings.GetType().GetProperty(tpa.PropertyName, BindingFlags.Instance | BindingFlags.Public);
                if (pi != null)
                {
                    object raw = pi.GetValue(settings, null);
                    return SerializeSettingValueWithOptionsIfNeeded(settings, tpa, raw);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Settings Value Helpers (ComboBox/Enums)

        private static object SerializeSettingValueWithOptionsIfNeeded(ISettings settings, TaskPaneAttribute tpa, object rawValue)
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

            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["selectedIndex"] = selectedIndex,
                ["selectedText"] = selectedText,
                ["selectedRaw"] = NormalizeSettingValue(rawValue),
                ["options"] = options.Count > 0 ? options : null
            };
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

                    return options;
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

        #endregion

        #region Catalog Helpers (Category Grouping)

        private static Dictionary<string, List<Dictionary<string, string>>> BuildComponentsByCategory(IEnumerable<Type> componentTypes)
        {
            var byCategory = new Dictionary<string, List<Dictionary<string, string>>>(StringComparer.CurrentCultureIgnoreCase);

            foreach (Type type in componentTypes)
            {
                PluginInfoAttribute pluginInfo = PluginExtension.GetPluginInfoAttribute(type);
                string caption = pluginInfo != null ? pluginInfo.Caption : type.Name;
                string tooltip = pluginInfo != null ? pluginInfo.ToolTip : null;

                ComponentCategoryAttribute[] categoryAttributes = type
                    .GetCustomAttributes(typeof(ComponentCategoryAttribute), true)
                    .OfType<ComponentCategoryAttribute>()
                    .ToArray();

                if (categoryAttributes.Length == 0)
                {
                    AddComponent(byCategory, ComponentCategory.Undefined.ToString(), caption, tooltip);
                    continue;
                }

                foreach (ComponentCategoryAttribute catAttr in categoryAttributes)
                {
                    AddComponent(byCategory, catAttr.Category.ToString(), caption, tooltip);
                }
            }

            return byCategory;
        }

        private static void AddComponent(
            Dictionary<string, List<Dictionary<string, string>>> byCategory,
            string categoryKey,
            string caption,
            string tooltip)
        {
            if (!byCategory.TryGetValue(categoryKey, out List<Dictionary<string, string>> list))
            {
                list = new List<Dictionary<string, string>>();
                byCategory[categoryKey] = list;
            }

            list.Add(new Dictionary<string, string>
            {
                { "caption", caption },
                { "tooltip", tooltip }
            });
        }

        #endregion
    }
}
