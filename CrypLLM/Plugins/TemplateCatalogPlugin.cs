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
/// This file implements the TemplateCatalogPlugin, providing Semantic Kernel tools 
/// for the discovery, abstraction, and computational inspection of predefined CrypTool 2 workspace templates.
/// These endpoints empower the LLM agent to analyze static instructional templates, parse existing 
/// cryptographic configurations, and present structurally sound cryptographic logic flows directly to the user.
/// </summary>

using CrypTool.CrypLLM.Helper;
using CrypTool.CrypLLM.Ports;
using CrypTool.CrypLLM.Services;
using CrypTool.PluginBase.IO;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using WorkspaceManager.Model;

namespace CrypTool.CrypLLM
{
    /// <summary>
    /// Encapsulates template catalog extraction capabilities, exposing AI functions to query 
    /// disk-based .cwm templates alongside their localized XML metadata, component settings, 
    /// and spatial topological layouts.
    /// </summary>
    internal sealed class TemplateCatalogPlugin
    {
        #region KernelFunctions - Template Catalog

        /// <summary>Return a short ranked catalog subset without loading or opening workspaces.</summary>
        [KernelFunction("tpl_search")]
        [Description("Searches available CT2 template titles, paths, summaries, descriptions and keywords using short task/algorithm keywords. Returns up to maxResults actual template paths/titles and matching metadata. Inspect candidates with tpl_info. For a suitable template, explain why and ask whether to open it or build from scratch; wait for the reply before either action, unless already explicitly chosen.")]
        public string SearchTemplates(string query, int maxResults = 8)
        {
            return LLMPluginService.InvokeOnUi(() =>
            {
                if (string.IsNullOrWhiteSpace(query) || maxResults < 1 || maxResults > 20)
                    return JsonConvert.SerializeObject(new { error = "Use nonempty search keywords and maxResults between 1 and 20." });
                string root = DirectoryHelper.DirectorySamples;
                if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
                    return JsonConvert.SerializeObject(new { error = "Templates directory not found." });
                string[] terms = query.Split(new[] { ' ', ',', ';', '-', '/', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var matches = Directory.EnumerateFiles(root, "*.cwm", SearchOption.AllDirectories)
                    .Select(path => new { entry = ReadTemplateEntry(root, path, "en", true), german = ReadTemplateEntry(root, path, "de", true) }).Where(item => item.entry != null)
                    .Select(item => new { entry = item.entry, score = terms.Count(term =>
                        (JsonConvert.SerializeObject(item.entry) + JsonConvert.SerializeObject(item.german)).IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0) })
                    .Where(match => match.score > 0).OrderByDescending(match => match.score)
                    .ThenBy(match => Convert.ToString(match.entry["templatePath"]), StringComparer.OrdinalIgnoreCase).ToList();
                return JsonConvert.SerializeObject(new
                {
                    count = matches.Count,
                    templates = matches.Take(maxResults).Select(match => new
                    {
                        templatePath = match.entry["templatePath"], title = match.entry["title"],
                        summary = Convert.ToString(match.entry["summary"])?.Substring(0, Math.Min(600, Convert.ToString(match.entry["summary"])?.Length ?? 0)),
                        keywords = match.entry["keywords"], matchedTerms = match.score
                    }),
                    truncated = matches.Count > maxResults
                });
            });
        }

        [KernelFunction("tpl_list")]
        [ContextWindowToken(min: 10000)]
        [Description("Returns a compact list of available CT2 templates (*.cwm) found under the Templates directory. Includes only templatePath and title.")]
        public string GetAvailableTemplates()
        {
            return LLMPluginService.InvokeOnUi(GetAvailableTemplatesInternal);
        }

        [KernelFunction("tpl_info")]
        [ContextWindowToken(min: 10000)]
        [Description("Returns detailed metadata for one CT2 template (*.cwm), including summary, description and keywords. Parameter accepts absolute path or Templates-relative path. Resolve valid paths via tool tpl_list.")]
        public string GetTemplateInfo(string templatePath)
        {
            return LLMPluginService.InvokeOnUi(() => GetTemplateInfoInternal(templatePath));
        }

        [KernelFunction("tpl_open")]
        [AiToolMutation(true)]
        [ContextWindowToken(min: 10000)]
        [Description("Opens an existing CT2 template (*.cwm) in a new visible workspace tab and returns at least the new tabId. Parameter accepts absolute path or Templates-relative path. Resolve valid paths via tool tpl_list.")]
        public string OpenTemplateInNewTab(string templatePath)
        {
            return LLMPluginService.InvokeOnUi(() => OpenTemplateInNewTabInternal(templatePath));
        }

        #endregion

        #region KernelFunctions - Template Model

        [KernelFunction("tpl_model")]
        [ContextWindowToken(min: 10000)]
        [Description("Loads a CT2 template (*.cwm) and returns its workspace model abstraction as JSON string. Parameter accepts absolute path or Templates-relative path. Resolve valid paths via tool tpl_list.")]
        public string GetTemplateWorkspaceModel(string templatePath)
        {
            return LLMPluginService.InvokeOnUi(() => GetTemplateWorkspaceModelInternal(templatePath));
        }

        #endregion

        #region KernelFunctions - Template IO/Bounds/Settings

        [KernelFunction("tpl_io")]
        [ContextWindowToken(min: 10000)]
        [Description("Loads a CT2 template (*.cwm) and returns JSON mapping runtime IDs to visible text/number IO values. Resolve valid paths via tool tpl_list.")]
        public string GetTemplateIoTexts(string templatePath)
        {
            return LLMPluginService.InvokeOnUi(() => GetTemplateIoTextsInternal(templatePath));
        }

        [KernelFunction("tpl_bounds")]
        [ContextWindowToken(min: 10000)]
        [Description("Loads a CT2 template (*.cwm) and returns JSON mapping runtime IDs to element coordinates/sizes for components, texts and images. Resolve valid paths via tool tpl_list.")]
        public string GetTemplateElementBounds(string templatePath)
        {
            return LLMPluginService.InvokeOnUi(() => GetTemplateElementBoundsInternal(templatePath));
        }

        [KernelFunction("tpl_settings")]
        [ContextWindowToken(min: 10000)]
        [Description("Loads a CT2 template (*.cwm) and returns JSON mapping runtime IDs to visible component settings. Resolve valid paths via tool tpl_list.")]
        public string GetTemplateComponentSettings(string templatePath)
        {
            return LLMPluginService.InvokeOnUi(() => GetTemplateComponentSettingsInternal(templatePath));
        }

        #endregion

        #region Internals - Template Catalog

        private string GetAvailableTemplatesInternal()
        {
            try
            {
                string templatesRoot = DirectoryHelper.DirectorySamples; // BaseDirectory/Templates
                if (string.IsNullOrWhiteSpace(templatesRoot) || !Directory.Exists(templatesRoot))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        templates = Array.Empty<object>(),
                        error = "Templates directory not found."
                    }, Formatting.None);
                }

                string[] cwmFiles = Directory.GetFiles(templatesRoot, "*.cwm", SearchOption.AllDirectories);

                string uiLang = CultureInfo.CurrentUICulture?.TwoLetterISOLanguageName;
                if (string.IsNullOrWhiteSpace(uiLang))
                {
                    uiLang = Properties.Settings.Default.defaultCultureKey;
                }

                var templates = new List<Dictionary<string, object>>(cwmFiles.Length);

                foreach (string cwmPath in cwmFiles)
                {
                    if (string.IsNullOrWhiteSpace(cwmPath))
                    {
                        continue;
                    }

                    // Enforces graceful degradation during file I/O operations. Malformed or inaccessible XML configs are skipped implicitly.
                    var entry = ReadTemplateEntry(templatesRoot, cwmPath, uiLang, includeDetails: false);
                    if (entry != null)
                    {
                        templates.Add(entry);
                    }
                }

                // Deterministic ordering keeps template discovery stable for the LLM across calls.
                var ordered = templates
                    .OrderBy(t => (t.TryGetValue("title", out object title) ? title as string : string.Empty) ?? string.Empty, StringComparer.CurrentCultureIgnoreCase)
                    .ThenBy(t => (t.TryGetValue("templatePath", out object path) ? path as string : string.Empty) ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return JsonConvert.SerializeObject(new
                {
                    count = ordered.Count,
                    templates = ordered
                }, Formatting.None);
            }
            catch (Exception ex)
            {
                Log.Warning($"get_available_templates failed: {ex.Message}");
                return JsonConvert.SerializeObject(new { error = ex.Message }, Formatting.None);
            }
        }

        private static string GetTemplateInfoInternal(string templatePath)
        {
            try
            {
                string templatesRoot = DirectoryHelper.DirectorySamples;
                if (string.IsNullOrWhiteSpace(templatesRoot) || !Directory.Exists(templatesRoot))
                {
                    return JsonConvert.SerializeObject(new { error = "Templates directory not found." }, Formatting.None);
                }

                string resolvedPath = TemplateLoader.ResolveTemplatePath(templatesRoot, templatePath);
                if (string.IsNullOrWhiteSpace(resolvedPath) || !File.Exists(resolvedPath))
                {
                    return JsonConvert.SerializeObject(new { error = "Template file not found.", templatePath }, Formatting.None);
                }

                string uiLang = CultureInfo.CurrentUICulture?.TwoLetterISOLanguageName;
                if (string.IsNullOrWhiteSpace(uiLang))
                {
                    uiLang = Properties.Settings.Default.defaultCultureKey;
                }

                Dictionary<string, object> entry = ReadTemplateEntry(templatesRoot, resolvedPath, uiLang, includeDetails: true);
                if (entry == null)
                {
                    return JsonConvert.SerializeObject(new { error = "Template metadata could not be read.", templatePath }, Formatting.None);
                }

                return JsonConvert.SerializeObject(entry, Formatting.None);
            }
            catch (Exception ex)
            {
                Log.Warning($"get_template_info failed: {ex.Message}");
                return JsonConvert.SerializeObject(new { error = ex.Message }, Formatting.None);
            }
        }

        #endregion

        #region Internals - Template Model

        private static string GetTemplateWorkspaceModelInternal(string templatePath)
        {
            try
            {
                WorkspaceModel model = TemplateLoader.TryLoadTemplateWorkspaceModel(templatePath);
                WorkspaceModelAbstraction abstraction = model == null
                    ? null
                    : AISchemaGenerator.CreateWorkspaceModelAbstraction(model);

                return JsonConvert.SerializeObject(abstraction, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Log.Warning($"get_template_workspace_model failed: {ex.Message}");
                return JsonConvert.SerializeObject(new { error = ex.Message }, Formatting.Indented);
            }
        }

        #endregion

        #region Internals - Template IO/Bounds/Settings

        private static string GetTemplateIoTextsInternal(string templatePath)
        {
            WorkspaceModel model = TemplateLoader.TryLoadTemplateWorkspaceModel(templatePath);
            return WorkspaceInspector.CreateIoTextsJson(model);
        }

        private static string GetTemplateElementBoundsInternal(string templatePath)
        {
            WorkspaceModel model = TemplateLoader.TryLoadTemplateWorkspaceModel(templatePath);
            return WorkspaceInspector.CreateWorkspaceElementBoundsJson(model);
        }

        private static string GetTemplateComponentSettingsInternal(string templatePath)
        {
            WorkspaceModel model = TemplateLoader.TryLoadTemplateWorkspaceModel(templatePath);
            return WorkspaceInspector.CreateComponentSettingsJson(model);
        }

        #endregion

        #region Internals - Template Metadata Parsing

        /// <summary>
        /// Extracts corresponding XML metadata elements associated with a specific workspace template.
        /// Resolves localized strings (e.g., titles, descriptions) responding dynamically to the active UI culture, 
        /// utilizing robust fallback mechanisms to default technical structures ensuring consistent metadata delivery.
        /// </summary>
        /// <param name="templatesRoot">The static root directory of installed templates.</param>
        /// <param name="cwmPath">The target template file location path.</param>
        /// <param name="uiLang">The targeted two-letter ISO language environment identifier.</param>
        /// <returns>A comprehensive mapping dictionary of templated metadata, or null upon physical data corruption.</returns>
        private static Dictionary<string, object> ReadTemplateEntry(string templatesRoot, string cwmPath, string uiLang, bool includeDetails)
        {
            try
            {
                var cwmFile = new FileInfo(cwmPath);
                if (!cwmFile.Exists)
                {
                    return null;
                }

                string xmlPath = Path.Combine(cwmFile.DirectoryName ?? string.Empty, Path.GetFileNameWithoutExtension(cwmFile.Name) + ".xml");
                string iconPath = Path.Combine(cwmFile.DirectoryName ?? string.Empty, Path.GetFileNameWithoutExtension(cwmFile.Name) + ".png");

                XElement xml = null;
                if (File.Exists(xmlPath))
                {
                    try
                    {
                        xml = XElement.Load(xmlPath);
                    }
                    catch
                    {
                        xml = null;
                    }
                }

                string title = ReadLocalizedElementValue(xml, "title", uiLang) ??
                               ReadLocalizedElementValue(xml, "title", Properties.Settings.Default.defaultCultureKey) ??
                               HumanizeFileName(Path.GetFileNameWithoutExtension(cwmFile.Name));

                string summary = ReadLocalizedElementValue(xml, "summary", uiLang) ??
                                 ReadLocalizedElementValue(xml, "summary", Properties.Settings.Default.defaultCultureKey);

                string description = ReadLocalizedElementValue(xml, "description", uiLang) ??
                                     ReadLocalizedElementValue(xml, "description", Properties.Settings.Default.defaultCultureKey);

                List<string> keywords = ReadKeywords(xml, uiLang);
                if (keywords.Count == 0 && !string.Equals(uiLang, Properties.Settings.Default.defaultCultureKey, StringComparison.OrdinalIgnoreCase))
                {
                    keywords = ReadKeywords(xml, Properties.Settings.Default.defaultCultureKey);
                }

                string templateRelativePath = TryGetRelativeFilePath(templatesRoot, cwmFile.FullName) ?? cwmFile.Name;

                var result = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["templatePath"] = templateRelativePath,
                    ["title"] = title
                };

                if (includeDetails)
                {
                    result["summary"] = summary;
                    result["description"] = description;
                    result["keywords"] = keywords.Count > 0 ? keywords : null;
                    result["hasMetadataXml"] = File.Exists(xmlPath);
                    result["hasIconFile"] = File.Exists(iconPath);
                }

                return result;
            }
            catch
            {
                return null;
            }
        }

        private static string ReadLocalizedElementValue(XElement root, string elementName, string lang)
        {
            try
            {
                if (root == null || string.IsNullOrWhiteSpace(elementName))
                {
                    return null;
                }

                foreach (XElement el in root.Elements(elementName))
                {
                    if (el?.Attribute("lang") == null)
                    {
                        continue;
                    }

                    if (string.Equals(el.Attribute("lang").Value, lang, StringComparison.OrdinalIgnoreCase))
                    {
                        return el.Value;
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private static List<string> ReadKeywords(XElement root, string lang)
        {
            var result = new List<string>();

            try
            {
                if (root == null)
                {
                    return result;
                }

                foreach (XElement kwEl in root.Elements("keywords"))
                {
                    XAttribute langAttr = kwEl.Attribute("lang");
                    string elLang = langAttr?.Value ?? Properties.Settings.Default.defaultCultureKey;

                    if (!string.Equals(elLang, lang, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string raw = kwEl.Value ?? string.Empty;
                    foreach (string kw in raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string trimmed = kw.Trim();
                        if (trimmed.Length > 0)
                        {
                            result.Add(trimmed);
                        }
                    }
                }
            }
            catch
            {
                // ignore
            }

            return result;
        }

        private static string HumanizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            return name.Replace("-", " ").Replace("_", " ").Trim();
        }

        private static string TryGetRelativeFilePath(string templatesRoot, string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(templatesRoot) || string.IsNullOrWhiteSpace(filePath))
                {
                    return null;
                }

                string root = Path.GetFullPath(templatesRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
                string fullPath = Path.GetFullPath(filePath);

                if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                return fullPath.Substring(root.Length).Replace(Path.DirectorySeparatorChar, '/');
            }
            catch
            {
                return null;
            }
        }

        private static string OpenTemplateInNewTabInternal(string templatePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(templatePath))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "templatePath is required."
                    }, Formatting.Indented);
                }

                string templatesRoot = DirectoryHelper.DirectorySamples;
                if (string.IsNullOrWhiteSpace(templatesRoot) || !Directory.Exists(templatesRoot))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "Templates directory not found."
                    }, Formatting.Indented);
                }

                string resolvedPath = TemplateLoader.ResolveTemplatePath(templatesRoot, templatePath);
                if (string.IsNullOrWhiteSpace(resolvedPath) || !File.Exists(resolvedPath))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "Template file not found.",
                        templatePath
                    }, Formatting.Indented);
                }

                if (!resolvedPath.EndsWith(".cwm", StringComparison.OrdinalIgnoreCase))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "templatePath must point to a .cwm file.",
                        templatePath
                    }, Formatting.Indented);
                }

                OpenTabsAbstraction openedTab = CrypWinPort.Instance?.OpenTemplateWorkspaceTab(resolvedPath);
                if (openedTab == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = "Template could not be opened in a new tab.",
                        templatePath
                    }, Formatting.Indented);
                }

                string templateRelativePath = TryGetRelativeFilePath(templatesRoot, resolvedPath) ?? resolvedPath;
                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    templatePath = templateRelativePath,
                    resolvedPath,
                    tabId = openedTab.Id,
                    title = openedTab.Title,
                    contentType = openedTab.ContentType,
                    isActive = openedTab.IsActive
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Log.Warning($"open_template_in_new_tab failed: {ex.Message}");
                return JsonConvert.SerializeObject(new { success = false, error = ex.Message }, Formatting.Indented);
            }
        }

        #endregion
    }
}
