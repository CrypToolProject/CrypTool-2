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
using System;
using System.Collections.Generic;
using System.Threading;

// Module overview:
// Provides request-scoped runtime budgeting for tool invocations and
// records text-reduction metadata for diagnostics and reproducibility.
namespace CrypTool.CrypLLM.Threads
{
    /// <summary>
    /// Runtime-scoped tool invocation budget manager that stores per-request
    /// tool-call budget in <see cref="AsyncLocal{T}"/> state.
    /// </summary>
    /// <remarks>
    /// This module allows nested components to coordinate tool usage limits
    /// without passing budget objects through every call.
    /// </remarks>
    internal static class ToolInvocationBudgetRuntime
    {
        // Heuristic used for coarse token estimation when exact tokenizer access is unavailable.
        private const int TokensPerCharacterDivisor = 4;
        // Fraction of the model context window reserved as a minimal per-tool-call execution budget.
        private const double ToolCallReservationShare = 0.005;
        private static readonly AsyncLocal<ToolInvocationBudgetState> CurrentState = new AsyncLocal<ToolInvocationBudgetState>();

        /// <summary>
        /// Opens a new runtime budget scope for the current asynchronous control flow.
        /// </summary>
        /// <param name="contextWindowTokens">Total model context window in tokens.</param>
        /// <param name="reservedToolTokens">Initial token budget available for tool interactions.</param>
        /// <returns>
        /// A disposable scope handle that restores the previous budget state when disposed.
        /// </returns>
        public static IDisposable Enter(int contextWindowTokens, int reservedToolTokens)
        {
            ToolInvocationBudgetState previous = CurrentState.Value;
            CurrentState.Value = new ToolInvocationBudgetState(contextWindowTokens, reservedToolTokens);
            return new Scope(previous);
        }

        /// <summary>
        /// Attempts to retrieve the active budget state for the current asynchronous flow.
        /// </summary>
        /// <param name="state">The active state when available; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if a state is active; otherwise <c>false</c>.</returns>
        public static bool TryGet(out ToolInvocationBudgetState state)
        {
            state = CurrentState.Value;
            return state != null;
        }

        /// <summary>
        /// Scope guard that restores the previous runtime budget state.
        /// </summary>
        private sealed class Scope : IDisposable
        {
            private readonly ToolInvocationBudgetState _previous;
            private bool _disposed;

            public Scope(ToolInvocationBudgetState previous)
            {
                _previous = previous;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                CurrentState.Value = _previous;
                _disposed = true;
            }
        }

        /// <summary>
        /// Mutable per-request budget state shared across thread-manager components.
        /// </summary>
        /// <remarks>
        /// Input assumptions:
        /// - Token values may be approximate and are clamped to non-negative ranges.
        /// Output behavior:
        /// - Remaining budget never drops below zero.
        /// Limitations:
        /// - Token estimation from plain text length is heuristic and tokenizer-agnostic.
        /// </remarks>
        internal sealed class ToolInvocationBudgetState
        {
            // Keep at least one token for final tool result generation after reservation is deducted.
            private const int MinimumResultTokensAfterReservation = 1;
            private readonly object _textReductionSync = new object();
            private readonly List<TextReductionEntry> _textReductions = new List<TextReductionEntry>();

            /// <summary>
            /// Initializes a new budget state for one logical request.
            /// </summary>
            /// <param name="contextWindowTokens">Model context size used to derive per-call reservations.</param>
            /// <param name="reservedToolTokens">Initial budget intended for tool operations.</param>
            public ToolInvocationBudgetState(int contextWindowTokens, int reservedToolTokens)
            {
                ContextWindowTokens = Math.Max(0, contextWindowTokens);
                RemainingToolTokens = Math.Max(0, reservedToolTokens);
            }

            public int ContextWindowTokens { get; }
            public int RemainingToolTokens { get; private set; }
            public int ToolCallCount { get; private set; }
            public int ToolCallReservationTokens => Math.Max(1, (int)Math.Ceiling(ContextWindowTokens * ToolCallReservationShare));
            public int RequiredTokensForToolCall => ToolCallReservationTokens + MinimumResultTokensAfterReservation;

            /// <summary>
            /// Checks whether the remaining budget can safely cover a new tool invocation.
            /// </summary>
            public bool CanInvokeTool()
            {
                return RemainingToolTokens >= RequiredTokensForToolCall;
            }

            /// <summary>
            /// Registers one tool invocation and consumes its reserved per-call budget.
            /// </summary>
            /// <returns>
            /// <c>true</c> if the call was admitted and charged; otherwise <c>false</c>.
            /// </returns>
            public bool TryRegisterToolCall()
            {
                if (!CanInvokeTool())
                {
                    return false;
                }

                // Section: per-call reservation charging
                // Reserve a minimal per-call runtime budget so small-context models can
                // execute at least one tool call, but not an unlimited sequence.
                ConsumeByTokens(ToolCallReservationTokens);
                ToolCallCount++;
                return true;
            }

            /// <summary>
            /// Deducts budget based on an estimated token count derived from textual content.
            /// </summary>
            /// <param name="content">Content attributed to tool-runtime consumption.</param>
            public void ConsumeByContent(string content)
            {
                if (string.IsNullOrEmpty(content))
                {
                    return;
                }

                ConsumeByTokens(EstimateTokens(content));
            }

            /// <summary>
            /// Deducts a concrete token amount from the remaining tool budget.
            /// </summary>
            /// <param name="tokens">Token amount to consume. Non-positive values are ignored.</param>
            public void ConsumeByTokens(int tokens)
            {
                if (tokens <= 0)
                {
                    return;
                }

                RemainingToolTokens = Math.Max(0, RemainingToolTokens - tokens);
            }

            /// <summary>
            /// Records one text-reduction event for later diagnostics or reporting.
            /// </summary>
            /// <remarks>
            /// The method stores normalized, non-null fields and clamps token values to maintain
            /// stable downstream serialization and analysis.
            /// </remarks>
            public void RegisterTextReduction(
                string source,
                string method,
                string pluginName,
                string functionName,
                int originalTokens,
                int reducedTokens,
                string originalText,
                string reducedText)
            {
                lock (_textReductionSync)
                {
                    _textReductions.Add(new TextReductionEntry
                    {
                        Source = source ?? string.Empty,
                        Method = method ?? string.Empty,
                        PluginName = pluginName ?? string.Empty,
                        FunctionName = functionName ?? string.Empty,
                        OriginalTokens = Math.Max(0, originalTokens),
                        ReducedTokens = Math.Max(0, reducedTokens),
                        OriginalText = originalText ?? string.Empty,
                        ReducedText = reducedText ?? string.Empty
                    });
                }
            }

            /// <summary>
            /// Returns a thread-safe copy of all recorded text-reduction entries.
            /// </summary>
            /// <returns>
            /// A deep snapshot list that can be consumed without holding internal locks.
            /// </returns>
            public List<TextReductionEntry> GetTextReductionSnapshot()
            {
                lock (_textReductionSync)
                {
                    // Section: defensive copy to prevent external mutation of internal state.
                    List<TextReductionEntry> snapshot = new List<TextReductionEntry>(_textReductions.Count);
                    for (int i = 0; i < _textReductions.Count; i++)
                    {
                        TextReductionEntry entry = _textReductions[i];
                        snapshot.Add(new TextReductionEntry
                        {
                            Source = entry.Source ?? string.Empty,
                            Method = entry.Method ?? string.Empty,
                            PluginName = entry.PluginName ?? string.Empty,
                            FunctionName = entry.FunctionName ?? string.Empty,
                            OriginalTokens = Math.Max(0, entry.OriginalTokens),
                            ReducedTokens = Math.Max(0, entry.ReducedTokens),
                            OriginalText = entry.OriginalText ?? string.Empty,
                            ReducedText = entry.ReducedText ?? string.Empty
                        });
                    }

                    return snapshot;
                }
            }

            /// <summary>
            /// Estimates token usage from character length using a coarse static heuristic.
            /// </summary>
            /// <param name="content">Input text to estimate.</param>
            /// <returns>Estimated tokens, with a minimum of one token for non-empty content.</returns>
            private static int EstimateTokens(string content)
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    return 0;
                }

                return Math.Max(1, (int)Math.Ceiling(content.Length / (double)TokensPerCharacterDivisor));
            }

            /// <summary>
            /// Data record describing one text reduction operation.
            /// </summary>
            public sealed class TextReductionEntry
            {
                public string Source { get; set; }
                public string Method { get; set; }
                public string PluginName { get; set; }
                public string FunctionName { get; set; }
                public int OriginalTokens { get; set; }
                public int ReducedTokens { get; set; }
                public string OriginalText { get; set; }
                public string ReducedText { get; set; }
            }
        }

    }
}
