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
/// This file encapsulates fundamental enumerations and custom metadata attributes utilized
/// by the LLM integration layer. These structures govern provider selection and enforce
/// state mutation permissions plus minimum context-window requirements on dynamically
/// registered Semantic Kernel tools.
/// </summary>

using System;

namespace CrypTool.CrypLLM.Helper
{
    /// <summary>
    /// Defines the supported integrations for Large Language Model backends.
    /// Used by the configuration modules to route API calls either to cloud-based services 
    /// or localized model inference engines.
    /// </summary>
    public enum SupportedProviders
    {
        /// <summary>
        /// Represents a localized LLM instance (e.g., LM Studio or Ollama) operated securely within the user's local infrastructure.
        /// </summary>
        Local,

        /// <summary>
        /// Represents an integration with the official cloud-based OpenAI API platform.
        /// </summary>
        OpenAI,

        /// <summary>
        /// Represents a disconnected or unconfigured system state where no provider is active.
        /// </summary>
        None
    }

    /// <summary>
    /// A custom metadata attribute designating whether a specific tool possesses the capability 
    /// to actively modify the application state, workspace, or host file system.
    /// This mechanism allows the orchestrator to enforce safety boundaries, isolate read-only tools, 
    /// or require explicit user consent prior to executing mutation operations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    internal sealed class AiToolMutationAttribute : Attribute
    {
        /// <summary>
        /// Gets a value indicating whether the decorated tool alters the program state.
        /// </summary>
        public bool CanModifyProgram { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AiToolMutationAttribute"/> class.
        /// </summary>
        /// <param name="canModifyProgram">If set to true, authorizes the tool to perform systemic mutations.</param>
        public AiToolMutationAttribute(bool canModifyProgram)
        {
            CanModifyProgram = canModifyProgram;
        }
    }

    /// <summary>
    /// A custom metadata attribute establishing a minimum context window requirement for an AI tool.
    /// Tools decorated with this attribute are only exposed to the agent if the active model meets
    /// the declared minimum token capacity.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    internal sealed class ContextWindowTokenAttribute : Attribute
    {
        /// <summary>
        /// The minimum token capacity required to execute the tool.
        /// </summary>
        public int MinTokens { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextWindowTokenAttribute"/> class.
        /// </summary>
        /// <param name="min">The minimum required context window size in tokens.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the provided value is outside the valid token range.
        /// </exception>
        public ContextWindowTokenAttribute(int min = 0)
        {
            if (min < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(min), "ContextWindowToken min must be >= 0.");
            }

            MinTokens = min;
        }
    }
}
