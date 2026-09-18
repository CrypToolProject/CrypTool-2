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
/// This file defines the core architectural boundary (port) between the underlying LLM module 
/// and the primary WPF host environment (CrypWin). By utilizing the Dependency Inversion Principle, 
/// it guarantees the LLM plugins remain intrinsically decoupled from direct GUI references, ensuring stable, testable telemetry.
/// </summary>

using CrypTool.PluginBase.Editor;
using System.Collections.Generic;

namespace CrypTool.CrypLLM.Ports
{
    /// <summary>
    /// Defines the contractual bridge between the decoupled LLM subsystem and the primary WPF host environment.
    /// Enforces architectural boundaries by ensuring semantic plugins interact exclusively via this interface, 
    /// preventing direct adherence to complex user interface components and preserving application thread safety.
    /// </summary>
    public interface ICrypWinAdapter
    {
        /// <summary>
        /// Retrieves the currently focused cryptographic editor component active in the user environment.
        /// </summary>
        IEditor ActiveEditor { get; }

        /// <summary>
        /// Extracts a volatile snapshot of the workspace topology, providing the AI with localized 
        /// navigational context regarding currently instantiated modules.
        /// </summary>
        /// <returns>A collection of structural metadata DTOs representing active UI tabs.</returns>
        IEnumerable<OpenTabsAbstraction> GetOpenTabs();

        /// <summary>
        /// Resolves a transient internal workspace identifier into its corresponding architectural editor schema.
        /// Essential for targeting localized mutations to specific workflows securely.
        /// </summary>
        /// <param name="tabId">The runtime-assigned hash key representing the target UI tab.</param>
        /// <param name="model">The output parameter resolved to the concrete editor instance, if successful.</param>
        /// <returns>True if the resolution succeeds and the designated context embodies a valid workspace model; otherwise, false.</returns>
        bool TryGetWorkspaceEditorByTabId(string tabId, out IEditor model);

        /// <summary>
        /// Resolves plain-text documentation content from an Online Help tab.
        /// If <paramref name="tabId"/> is null or empty, the currently active tab is inspected.
        /// </summary>
        /// <param name="tabId">Optional runtime-assigned hash key representing the target UI tab.</param>
        /// <param name="documentationContext">The extracted documentation snapshot if successful.</param>
        /// <returns>True if the designated or active tab is an Online Help tab with readable content; otherwise, false.</returns>
        bool TryGetDocumentationContextByTabId(string tabId, out DocumentationContextAbstraction documentationContext);

        /// <summary>
        /// Accesses recent in-memory application telemetry logs, allowing the agent to perform diagnostic 
        /// reasoning on system failures decoupled from disk IO lock latencies.
        /// </summary>
        /// <param name="maxLines">The structural limit determining the maximum volume of log trails to harvest.</param>
        /// <returns>A chronologically ordered subset of diagnostic strings.</returns>
        IList<string> GetRecentLogMessages(int maxLines);

        /// <summary>
        /// Programmatically transitions the host graphical layout to emphasize the primary AI interaction panel.
        /// </summary>
        void ShowAIChatPane();

        /// <summary>
        /// Orchestrates GUI navigation directly to the AI plugin configuration namespace.
        /// </summary>
        void ShowAIChatSettings();

        /// <summary>
        /// Creates a new visible tab containing an empty workspace and returns metadata for the opened tab.
        /// </summary>
        OpenTabsAbstraction CreateEmptyWorkspaceTab();

        /// <summary>
        /// Opens an existing CT2 template in a new visible workspace tab and returns metadata for the opened tab.
        /// </summary>
        OpenTabsAbstraction OpenTemplateWorkspaceTab(string templateFilePath);
    }

    /// <summary>
    /// Data Transfer Object (DTO) facilitating the safe exchange of active session states across the subsystem boundary.
    /// Prevents dependency leakage by isolating the LLM inference agent from heavyweight, thread-affinitized graphical layout structures.
    /// </summary>
    public sealed class OpenTabsAbstraction
    {
        public string Id { get; set; }
        public string ContentType { get; set; }
        public string Title { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO carrying the currently visible documentation page in a format suitable for LLM tool responses.
    /// </summary>
    public sealed class DocumentationContextAbstraction
    {
        public string TabId { get; set; }
        public string TabTitle { get; set; }
        public string PageTitle { get; set; }
        public string Source { get; set; }
        public string Text { get; set; }
    }

    /// <summary>
    /// Global service locator functioning as the fundamental dependency injection terminus for the host's implementation of the adapter.
    /// Must be explicitly initialized by the application bootstrapper preceding explicit Semantic Kernel instantiations.
    /// </summary>
    public static class CrypWinPort
    {
        public static ICrypWinAdapter Instance { get; set; }
    }
}
