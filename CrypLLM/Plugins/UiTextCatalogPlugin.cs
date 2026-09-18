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
/// This file implements the UiTextCatalogPlugin, a Semantic Kernel plugin that provides the LLM agent 
/// with domain-specific UI texts and structural help content. It acts as an abstraction layer, mapping 
/// standardized, clear-text topic names to internal instruction configuration keys, ensuring that the AI 
/// relies on a strict, single-parameter interface without exposure to cryptic internal identifiers.
/// </summary>

using CrypTool.CrypLLM.AgentInstructions;
using CrypTool.CrypLLM.Helper;
using CrypTool.CrypLLM.Ports;
using CrypTool.CrypLLM.Services;
using CrypTool.PluginBase.IO;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace CrypTool.CrypLLM
{
    /// <summary>
    /// Semantic Kernel plugin providing access to localized UI text elements and conceptual tutorials.
    /// </summary>
    internal sealed class UiTextCatalogPlugin
    {
        private const double TutorialSummaryCharacterShare = 0.10d;
        private const string EnglishLanguageCode = "en";

        private static readonly UiTopicDefinition[] Topics =
        {
            new UiTopicDefinition("Startcenter", "UiText_Startcenter"),
            new UiTopicDefinition("Wizard", "UiText_Wizard"),
            new UiTopicDefinition("Workspace", "UiText_Workspace"),
            new UiTopicDefinition("CrypCloud", "UiText_CrypCloud"),
            new UiTopicDefinition("CrypToolStore", "UiText_Store"),
            new UiTopicDefinition("Homomorphic ciphers", "UiText_Homomorphic"),
            new UiTopicDefinition("Key Derivation Functions Based on Pseudorandom Functions", "UiText_PrfKdf")
        };

        private static readonly Dictionary<string, UiTopicDefinition> TopicsByName =
            Topics.ToDictionary(t => t.TopicName, StringComparer.OrdinalIgnoreCase);

        private static readonly TutorialDocumentationDefinition[] TutorialDocumentations =
        {
            new TutorialDocumentationDefinition(
                "Attacks on PKCS#1",
                Path.Combine("CrypPlugins", "PKCS1"),
                "PKCS1",
                "PKCS#1",
                "Attacks on PKCS1"),
            new TutorialDocumentationDefinition(
                "Lattice-based Cryptography",
                Path.Combine("CrypPlugins", "LatticeCrypto"),
                "LatticeCrypto",
                "Lattice-based cryptography"),
            new TutorialDocumentationDefinition(
                "World of Primes",
                Path.Combine("CrypPlugins", "Primes", "Primes"),
                "Primes",
                "World of primes")
        };

        private static readonly Dictionary<string, TutorialDocumentationDefinition> TutorialDocsByName =
            BuildTutorialDocumentationLookup();

        /// <summary>
        /// Retrieves descriptive documentation or UI text for a specifically requested topic.
        /// Subscribes to architectural rules by exposing exactly one clear-text parameter for the AI, 
        /// mitigating hallucinations associated with complex or programmatic internal identifiers.
        /// </summary>
        /// <param name="topicName">The human-readable, clear-text name of the requested conceptual topic.</param>
        /// <returns>The documented text associated with the topic, or a structured JSON error enumerating valid choices if unrecognized.</returns>
        [KernelFunction("text_ui")]
        [Description("Returns CrypTool 2 UI / help text by topicName.Allowed: Startcenter, Wizard, Workspace, CrypCloud, CrypToolStore, Homomorphic ciphers and Key Derivation Functions Based on Pseudorandom Functions.")]
        public string GetUiText(string topicName)
        {
            string normalizedTopicName = (topicName ?? string.Empty).Trim();

            if (!TopicsByName.TryGetValue(normalizedTopicName, out UiTopicDefinition topic))
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    error = "Unknown topic.",
                    allowedTopics = BuildAllowedTopicsHelpText()
                }, Formatting.Indented);
            }

            string instructionText = GetInstructionText(topic.InstructionKey);
            if (!string.IsNullOrWhiteSpace(instructionText))
            {
                return instructionText;
            }

            return "No instructions found";
        }

        /// <summary>
        /// Reads the documentation text from the currently active Online Help tab (or a specific tab id)
        /// and returns it directly to the AI as contextual grounding material.
        /// </summary>
        /// <param name="tabId">Optional tab id. If omitted, the currently active tab is inspected.</param>
        /// <returns>Structured JSON containing the extracted documentation text or an explanatory error payload.</returns>
        [KernelFunction("open_documentation_text")]
        [Description("Returns the plain text of the currently active Online Help/documentation tab. Optional tabId: if omitted, the active tab is used. Use this when the user asks about the documentation currently visible on screen.")]
        public string SetDocumentationContext(string tabId = null)
        {
            return LLMPluginService.InvokeOnUi(() => GetDocumentationContextInternal(tabId));
        }

        [KernelFunction("tutorial_docs_all")]
        [ContextWindowToken(min: 150000)]
        [Description("Returns the full assembled documentation text for one Crypto Tutorial. Input tutorialName must be one of: Attacks on PKCS#1, Lattice-based Cryptography, World of Primes. The tool collects DetailedDescription/doc.xml, OnlineHelp start pages, and tutorial help pages and returns only the assembled text.")]
        public string GetTutorialDocumentationBundle(string tutorialName)
        {
            return GetTutorialDocumentationBundleInternal(tutorialName, summaryOnly: false);
        }

        [KernelFunction("tutorial_docs_summary")]
        [ContextWindowToken(min: 10000)]
        [Description("Returns a reduced plain-text summary of the documentation for one Crypto Tutorial. Input tutorialName must be one of: Attacks on PKCS#1, Lattice-based Cryptography, World of Primes. The tool collects DetailedDescription/doc.xml, OnlineHelp start pages, and tutorial help pages, selects a compact subset, and returns only the assembled text.")]
        public string GetTutorialDocumentationSummaryBundle(string tutorialName)
        {
            return GetTutorialDocumentationBundleInternal(tutorialName, summaryOnly: true);
        }

        private static string GetInstructionText(string instructionKey)
        {
            string text = InstructionManager.Instance.GetInstructionText(instructionKey);
            return string.IsNullOrWhiteSpace(text) ? string.Empty : text;
        }

        private static string[] BuildAllowedTopicsHelpText()
        {
            return Topics
                .Select(t => t.TopicName)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static string GetDocumentationContextInternal(string tabId)
        {
            string effectiveTabId = NormalizeDocumentationTabId(tabId);
            ICrypWinAdapter bridge = CrypWinPort.Instance;
            if (bridge == null)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    error = "CrypWin bridge not available."
                }, Formatting.Indented);
            }

            if (bridge.TryGetDocumentationContextByTabId(effectiveTabId, out DocumentationContextAbstraction documentationContext))
            {
                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    tabId = documentationContext.TabId,
                    tabTitle = documentationContext.TabTitle,
                    pageTitle = documentationContext.PageTitle,
                    source = documentationContext.Source,
                    text = documentationContext.Text
                }, Formatting.Indented);
            }

            OpenTabsAbstraction activeTab = (bridge.GetOpenTabs() ?? Array.Empty<OpenTabsAbstraction>())
                .FirstOrDefault(t => t != null && t.IsActive);

            return JsonConvert.SerializeObject(new
            {
                success = false,
                error = string.IsNullOrWhiteSpace(effectiveTabId)
                    ? "The active tab is not an Online Help/documentation tab or no readable documentation page is loaded."
                    : "The requested tab is not an Online Help/documentation tab or no readable documentation page is loaded.",
                activeTab = activeTab == null ? null : new
                {
                    id = activeTab.Id,
                    title = activeTab.Title,
                    contentType = activeTab.ContentType
                }
            }, Formatting.Indented);
        }

        private static string NormalizeDocumentationTabId(string tabId)
        {
            if (string.IsNullOrWhiteSpace(tabId))
            {
                return null;
            }

            string normalized = tabId.Trim().Trim('"', '\'');
            if (string.Equals(normalized, "active", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "current", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "focused", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "default", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return normalized;
        }

        private static string GetTutorialDocumentationBundleInternal(string tutorialName, bool summaryOnly)
        {
            string normalizedTutorialName = NormalizeTopicOrTutorialName(tutorialName);
            if (!TutorialDocsByName.TryGetValue(normalizedTutorialName, out TutorialDocumentationDefinition tutorial))
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    error = "Unknown tutorial.",
                    allowedTutorials = BuildAllowedTutorialsHelpText()
                }, Formatting.Indented);
            }

            try
            {
                List<DocumentationRecord> allDocuments = CollectTutorialDocumentation(tutorial);
                if (allDocuments.Count == 0)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        tutorialName = tutorial.TutorialName,
                        error = "No tutorial documentation files found."
                    }, Formatting.Indented);
                }

                int totalRawCharacterCount = allDocuments.Sum(d => d.RawCharacterCount);
                int targetRawCharacterBudget = Math.Max(1, (int)Math.Ceiling(totalRawCharacterCount * TutorialSummaryCharacterShare));
                List<DocumentationRecord> selectedDocuments = summaryOnly
                    ? SelectSummaryDocumentationDocuments(allDocuments, targetRawCharacterBudget)
                    : new List<DocumentationRecord>(allDocuments);

                string bundleText = summaryOnly
                    ? BuildDocumentationSummaryText(selectedDocuments)
                    : BuildDocumentationBundleText(selectedDocuments);

                return bundleText;
            }
            catch (Exception ex)
            {
                Log.Warning($"tutorial documentation bundle failed: tutorial={tutorial?.TutorialName ?? "<unknown>"}, summaryOnly={summaryOnly}, error={ex.Message}");
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    tutorialName = tutorial?.TutorialName,
                    error = ex.Message
                }, Formatting.Indented);
            }
        }

        private static Dictionary<string, TutorialDocumentationDefinition> BuildTutorialDocumentationLookup()
        {
            Dictionary<string, TutorialDocumentationDefinition> lookup =
                new Dictionary<string, TutorialDocumentationDefinition>(StringComparer.OrdinalIgnoreCase);

            foreach (TutorialDocumentationDefinition tutorial in TutorialDocumentations)
            {
                lookup[tutorial.TutorialName] = tutorial;

                foreach (string alias in tutorial.Aliases)
                {
                    if (!string.IsNullOrWhiteSpace(alias))
                    {
                        lookup[alias] = tutorial;
                    }
                }
            }

            return lookup;
        }

        private static string[] BuildAllowedTutorialsHelpText()
        {
            return TutorialDocumentations
                .Select(t => t.TutorialName)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static string NormalizeTopicOrTutorialName(string value)
        {
            return (value ?? string.Empty).Trim().Trim('"', '\'');
        }

        private static List<DocumentationRecord> CollectTutorialDocumentation(TutorialDocumentationDefinition tutorial)
        {
            string tutorialRoot = ResolveTutorialRootDirectory(tutorial);
            if (string.IsNullOrWhiteSpace(tutorialRoot) || !Directory.Exists(tutorialRoot))
            {
                throw new DirectoryNotFoundException(
                    $"Tutorial root directory not found for '{tutorial?.TutorialName}'. Relative root: {tutorial?.RelativeRoot}");
            }

            var candidateFiles = new List<string>();

            string detailedDescriptionFile = Path.Combine(tutorialRoot, "DetailedDescription", "doc.xml");
            AddFileIfExists(candidateFiles, detailedDescriptionFile);

            string onlineHelpRoot = Path.Combine(tutorialRoot, "OnlineHelp");
            if (Directory.Exists(onlineHelpRoot))
            {
                AddEnglishStartPages(candidateFiles, onlineHelpRoot);

                string helpFilesRoot = Path.Combine(onlineHelpRoot, "HelpFiles");
                if (Directory.Exists(helpFilesRoot))
                {
                    string englishHelpDirectory = Path.Combine(helpFilesRoot, EnglishLanguageCode);
                    if (Directory.Exists(englishHelpDirectory))
                    {
                        foreach (string helpFile in Directory.GetFiles(englishHelpDirectory, "*.htm*", SearchOption.TopDirectoryOnly))
                        {
                            AddFileIfExists(candidateFiles, helpFile);
                        }
                    }
                }
            }

            string[] orderedFiles = candidateFiles
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(path => GetDocumentationPriorityScore(GetRelativePathFromBase(path)))
                .ThenBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
                .ThenBy(path => GetRelativePathFromBase(path), StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var records = new List<DocumentationRecord>(orderedFiles.Length);
            foreach (string file in orderedFiles)
            {
                string relativePath = GetRelativePathFromBase(file);
                string content = ReadDocumentationContentForAi(file, relativePath);
                records.Add(new DocumentationRecord(
                    relativePath,
                    DetectDocumentationLanguage(relativePath),
                    DetectDocumentationKind(relativePath),
                    content,
                    GetDocumentationPriorityScore(relativePath)));
            }

            return records;
        }

        private static void AddEnglishStartPages(List<string> candidateFiles, string onlineHelpRoot)
        {
            if (candidateFiles == null || string.IsNullOrWhiteSpace(onlineHelpRoot) || !Directory.Exists(onlineHelpRoot))
            {
                return;
            }

            string[] preferredEnglishStartPageNames =
            {
                "StartControl.html",
                "Start.htm"
            };

            foreach (string fileName in preferredEnglishStartPageNames)
            {
                AddFileIfExists(candidateFiles, Path.Combine(onlineHelpRoot, fileName));
            }
        }

        private static string ReadDocumentationContentForAi(string filePath, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return string.Empty;
            }

            string rawContent = File.ReadAllText(filePath);
            if (!IsDetailedDescriptionXml(relativePath))
            {
                return rawContent;
            }

            try
            {
                XDocument document = XDocument.Parse(rawContent, LoadOptions.PreserveWhitespace);
                XElement filteredRoot = FilterXmlElementToEnglish(document.Root);
                if (filteredRoot == null)
                {
                    return string.Empty;
                }

                XDocument filteredDocument = new XDocument(new XDeclaration("1.0", "utf-8", null), filteredRoot);
                return filteredDocument.ToString(SaveOptions.DisableFormatting);
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to filter tutorial doc.xml to English. Falling back to raw content: path={relativePath}, error={ex.Message}");
                return rawContent;
            }
        }

        private static XElement FilterXmlElementToEnglish(XElement element)
        {
            if (element == null || !ShouldKeepElementForEnglish(element))
            {
                return null;
            }

            XElement clone = new XElement(element.Name);

            foreach (XAttribute attribute in element.Attributes())
            {
                if (ShouldKeepAttributeForEnglish(attribute))
                {
                    clone.Add(new XAttribute(attribute.Name, attribute.Value));
                }
            }

            foreach (XNode node in element.Nodes())
            {
                if (node is XElement childElement)
                {
                    XElement filteredChild = FilterXmlElementToEnglish(childElement);
                    if (filteredChild != null)
                    {
                        clone.Add(filteredChild);
                    }
                }
                else
                {
                    clone.Add(node);
                }
            }

            return clone;
        }

        private static bool ShouldKeepElementForEnglish(XElement element)
        {
            XAttribute langAttribute = element.Attribute("lang");
            if (langAttribute != null && !IsEnglishLanguageValue(langAttribute.Value))
            {
                return false;
            }

            XAttribute cultureAttribute = element.Attribute("culture");
            if (cultureAttribute != null && !IsEnglishCultureValue(cultureAttribute.Value))
            {
                return false;
            }

            return true;
        }

        private static bool ShouldKeepAttributeForEnglish(XAttribute attribute)
        {
            if (attribute == null)
            {
                return false;
            }

            if (string.Equals(attribute.Name.LocalName, "lang", StringComparison.OrdinalIgnoreCase))
            {
                return IsEnglishLanguageValue(attribute.Value);
            }

            if (string.Equals(attribute.Name.LocalName, "culture", StringComparison.OrdinalIgnoreCase))
            {
                return IsEnglishCultureValue(attribute.Value);
            }

            return true;
        }

        private static bool IsEnglishLanguageValue(string value)
        {
            return string.Equals((value ?? string.Empty).Trim(), EnglishLanguageCode, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsEnglishCultureValue(string value)
        {
            string normalized = (value ?? string.Empty).Trim();
            return string.Equals(normalized, EnglishLanguageCode, StringComparison.OrdinalIgnoreCase) ||
                   normalized.StartsWith("en-", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsDetailedDescriptionXml(string relativePath)
        {
            return !string.IsNullOrWhiteSpace(relativePath) &&
                   relativePath.Replace('\\', '/').EndsWith("/DetailedDescription/doc.xml", StringComparison.OrdinalIgnoreCase);
        }

        private static List<DocumentationRecord> SelectSummaryDocumentationDocuments(
            List<DocumentationRecord> allDocuments,
            int targetRawCharacterBudget)
        {
            List<DocumentationRecord> orderedDocuments = allDocuments
                .OrderBy(d => d.PriorityScore)
                .ThenBy(d => d.RawCharacterCount)
                .ThenBy(d => d.RelativePath, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var selectedDocuments = new List<DocumentationRecord>();
            int currentCharacterCount = 0;

            foreach (DocumentationRecord document in orderedDocuments)
            {
                if (selectedDocuments.Count == 0)
                {
                    selectedDocuments.Add(document);
                    currentCharacterCount += document.RawCharacterCount;
                    continue;
                }

                if (currentCharacterCount >= targetRawCharacterBudget)
                {
                    break;
                }

                selectedDocuments.Add(document);
                currentCharacterCount += document.RawCharacterCount;
            }

            return selectedDocuments;
        }

        private static string BuildDocumentationBundleText(IEnumerable<DocumentationRecord> documents)
        {
            return string.Join(
                Environment.NewLine + Environment.NewLine,
                documents.Select(document =>
                    "===== BEGIN DOCUMENT =====" + Environment.NewLine +
                    "Path: " + document.RelativePath + Environment.NewLine +
                    "Language: " + document.Language + Environment.NewLine +
                    "Kind: " + document.Kind + Environment.NewLine +
                    "RawCharacterCount: " + document.RawCharacterCount + Environment.NewLine +
                    document.Content + Environment.NewLine +
                    "===== END DOCUMENT ====="));
        }

        private static string BuildDocumentationSummaryText(IEnumerable<DocumentationRecord> documents)
        {
            if (documents == null)
            {
                return string.Empty;
            }

            return string.Join(
                Environment.NewLine + Environment.NewLine,
                documents
                    .Select(document => document?.Content?.Trim())
                    .Where(content => !string.IsNullOrWhiteSpace(content)));
        }

        private static void AddFileIfExists(List<string> files, string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                files.Add(path);
            }
        }

        private static string ResolveTutorialRootDirectory(TutorialDocumentationDefinition tutorial)
        {
            if (tutorial == null || string.IsNullOrWhiteSpace(tutorial.RelativeRoot))
            {
                return null;
            }

            foreach (string baseDirectoryCandidate in EnumerateTutorialBaseDirectoryCandidates())
            {
                try
                {
                    string candidateRoot = Path.GetFullPath(Path.Combine(baseDirectoryCandidate, tutorial.RelativeRoot));
                    if (Directory.Exists(candidateRoot))
                    {
                        return candidateRoot;
                    }
                }
                catch
                {
                    // ignore invalid candidate
                }
            }

            return null;
        }

        private static IEnumerable<string> EnumerateTutorialBaseDirectoryCandidates()
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string seed in new[]
            {
                DirectoryHelper.BaseDirectory,
                AppDomain.CurrentDomain.BaseDirectory,
                Environment.CurrentDirectory
            })
            {
                foreach (string candidate in EnumerateDirectoryAndAncestors(seed))
                {
                    if (seen.Add(candidate))
                    {
                        yield return candidate;
                    }
                }
            }
        }

        private static IEnumerable<string> EnumerateDirectoryAndAncestors(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                yield break;
            }

            string currentPath;
            try
            {
                currentPath = Path.GetFullPath(path);
            }
            catch
            {
                yield break;
            }

            DirectoryInfo current = new DirectoryInfo(currentPath);
            while (current != null)
            {
                yield return current.FullName;
                current = current.Parent;
            }
        }

        private static string GetRelativePathFromBase(string absolutePath)
        {
            if (string.IsNullOrWhiteSpace(absolutePath))
            {
                return string.Empty;
            }

            string fullPath = Path.GetFullPath(absolutePath);

            foreach (string baseDirectory in EnumerateTutorialBaseDirectoryCandidates())
            {
                string normalizedBaseDirectory = baseDirectory?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(normalizedBaseDirectory) ||
                    !fullPath.StartsWith(normalizedBaseDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string relative = fullPath.Substring(normalizedBaseDirectory.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return relative.Replace(Path.DirectorySeparatorChar, '/');
            }

            return fullPath.Replace(Path.DirectorySeparatorChar, '/');
        }

        private static string DetectDocumentationLanguage(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return "neutral";
            }

            string normalizedPath = relativePath.Replace('\\', '/');
            string fileName = Path.GetFileName(normalizedPath);

            if (normalizedPath.IndexOf("/HelpFiles/en/", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "en";
            }

            if (normalizedPath.IndexOf("/HelpFiles/de/", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "de";
            }

            if (fileName.EndsWith(".de.htm", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".de.html", StringComparison.OrdinalIgnoreCase))
            {
                return "de";
            }

            if (fileName.EndsWith(".htm", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            {
                return "en";
            }

            return "multilingual";
        }

        private static string DetectDocumentationKind(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return "unknown";
            }

            string normalizedPath = relativePath.Replace('\\', '/');
            string fileName = Path.GetFileName(normalizedPath);

            if (normalizedPath.EndsWith("/DetailedDescription/doc.xml", StringComparison.OrdinalIgnoreCase))
            {
                return "detailedDescription";
            }

            if (fileName.StartsWith("Start", StringComparison.OrdinalIgnoreCase))
            {
                return "startPage";
            }

            return "helpPage";
        }

        private static int GetDocumentationPriorityScore(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return int.MaxValue;
            }

            string normalizedPath = relativePath.Replace('\\', '/');
            string fileName = Path.GetFileName(normalizedPath) ?? string.Empty;

            if (normalizedPath.EndsWith("/DetailedDescription/doc.xml", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            if (fileName.Equals("StartControl.html", StringComparison.OrdinalIgnoreCase))
            {
                return 1;
            }

            if (fileName.Equals("StartControl.de.html", StringComparison.OrdinalIgnoreCase))
            {
                return 2;
            }

            if (fileName.Equals("Start.htm", StringComparison.OrdinalIgnoreCase))
            {
                return 3;
            }

            if (fileName.Equals("Start.de.htm", StringComparison.OrdinalIgnoreCase))
            {
                return 4;
            }

            if (fileName.IndexOf("Overview", StringComparison.OrdinalIgnoreCase) >= 0 ||
                fileName.IndexOf("Introduction", StringComparison.OrdinalIgnoreCase) >= 0 ||
                fileName.Equals("Distribution_Distribution.html", StringComparison.OrdinalIgnoreCase))
            {
                return 5;
            }

            if (fileName.StartsWith("Start", StringComparison.OrdinalIgnoreCase))
            {
                return 6;
            }

            return 100 + fileName.Length;
        }

        /// <summary>
        /// Metamodel binding an AI-facing conversational topic string to an internal system configuration key.
        /// </summary>
        private sealed class UiTopicDefinition
        {
            public UiTopicDefinition(string topicName, string instructionKey)
            {
                TopicName = topicName;
                InstructionKey = instructionKey;
            }

            public string TopicName { get; }
            public string InstructionKey { get; }
        }

        private sealed class TutorialDocumentationDefinition
        {
            public TutorialDocumentationDefinition(string tutorialName, string relativeRoot, params string[] aliases)
            {
                TutorialName = tutorialName;
                RelativeRoot = relativeRoot;
                Aliases = aliases ?? Array.Empty<string>();
            }

            public string TutorialName { get; }
            public string RelativeRoot { get; }
            public string[] Aliases { get; }
        }

        private sealed class DocumentationRecord
        {
            public DocumentationRecord(string relativePath, string language, string kind, string content, int priorityScore)
            {
                RelativePath = relativePath ?? string.Empty;
                Language = language ?? "neutral";
                Kind = kind ?? "unknown";
                Content = content ?? string.Empty;
                PriorityScore = priorityScore;
            }

            public string RelativePath { get; }
            public string Language { get; }
            public string Kind { get; }
            public string Content { get; }
            public int PriorityScore { get; }
            public int RawCharacterCount => Content.Length;
        }
    }
}
