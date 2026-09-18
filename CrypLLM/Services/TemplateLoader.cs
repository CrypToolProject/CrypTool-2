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
/// This file encapsulates the abstraction logic responsible for statically resolving 
/// and initializing predefined CrypTool 2 templates from local disk storage. 
/// It acts as a resilient buffer against anomalous path formulations synthesized by the AI agent.
/// </summary>

using CrypTool.PluginBase.IO;
using System;
using System.IO;
using WorkspaceManager.Model;

namespace CrypTool.CrypLLM.Services
{
    /// <summary>
    /// Utility module explicitly targeted at handling file I/O operations tailored for `.cwm` workspaces.
    /// Defensively guards the legacy deserialization engine from malformed textual references.
    /// </summary>
    internal static class TemplateLoader
    {
        /// <summary>
        /// Orchestrates the strict deserialization of a serialized XML template file into its object-graph model.
        /// Resolves ambiguities in absolute vs. relative paths dynamically before persisting telemetry upon invocation failure.
        /// </summary>
        /// <param name="templatePath">The raw, potentially unformulated structural path string.</param>
        /// <returns>A materialized <see cref="WorkspaceModel"/> instance, or null if schema constraints are unmet.</returns>
        public static WorkspaceModel TryLoadTemplateWorkspaceModel(string templatePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(templatePath))
                {
                    Log.Debug("TemplateLoader.TryLoadTemplateWorkspaceModel called with empty templatePath");
                    return null;
                }

                string templatesRoot = DirectoryHelper.DirectorySamples;
                if (string.IsNullOrWhiteSpace(templatesRoot) || !Directory.Exists(templatesRoot))
                {
                    Log.Debug("TemplateLoader.TryLoadTemplateWorkspaceModel: Templates directory not found.");
                    return null;
                }

                string resolvedPath = ResolveTemplatePath(templatesRoot, templatePath);
                if (string.IsNullOrWhiteSpace(resolvedPath) || !File.Exists(resolvedPath))
                {
                    Log.Debug("TemplateLoader.TryLoadTemplateWorkspaceModel: templatePath could not be resolved to an existing file.");
                    return null;
                }

                if (!resolvedPath.EndsWith(".cwm", StringComparison.OrdinalIgnoreCase))
                {
                    Log.Debug("TemplateLoader.TryLoadTemplateWorkspaceModel: templatePath does not point to a .cwm file.");
                    return null;
                }

                // Delegates the underlying parsing complexity to the core architectural ModelPersistance engine.
                // handleTemplateReplacement guarantees relative template structural integrity is maintained.
                var persistance = new ModelPersistance();
                WorkspaceModel model = persistance.loadModel(resolvedPath, handleTemplateReplacement: true);

                if (model == null)
                {
                    Log.Debug("TemplateLoader.TryLoadTemplateWorkspaceModel: failed to load template workspace model.");
                }

                return model;
            }
            catch (Exception ex)
            {
                Log.Warning($"TemplateLoader.TryLoadTemplateWorkspaceModel failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Heuristically interpolates physical file system routes.
        /// Compensates for missing file extensions or divergent relative configurations commonly assumed by Large Language Models.
        /// </summary>
        /// <param name="templatesRoot">The system-defined absolute anchor for template directories.</param>
        /// <param name="templatePath">The dynamic relative or absolute string to be unified.</param>
        /// <returns>A sanitized uniform execution path, or null if the structure critically faults.</returns>
        public static string ResolveTemplatePath(string templatesRoot, string templatePath)
        {
            // NOTE: Public because it can be useful for diagnostics/tests.
            try
            {
                templatePath = templatePath.Trim();

                // absolute path
                if (Path.IsPathRooted(templatePath))
                {
                    return templatePath;
                }

                // relative to Templates directory
                string combined = Path.Combine(templatesRoot, templatePath);

                // if caller omitted .cwm, try with .cwm
                if (!combined.EndsWith(".cwm", StringComparison.OrdinalIgnoreCase))
                {
                    string withExt = combined + ".cwm";
                    if (File.Exists(withExt))
                    {
                        return withExt;
                    }
                }

                return combined;
            }
            catch
            {
                return null;
            }
        }
    }
}
