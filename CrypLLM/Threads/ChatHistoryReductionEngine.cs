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
/// This module provides the core mechanism for token estimation, prompt budget planning, 
/// and history reduction logic. It ensures that Semantic Kernel interactions strictly 
/// adhere to Large Language Model context window limits by leveraging sequential 
/// compression techniques (summarization, parameter trimming, and tool-output formatting).
/// </summary>
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using CrypTool.CrypLLM.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrypTool.CrypLLM.Threads
{
    /// <summary>
    /// Central token estimator used by budgeting and reduction logic.
    /// Employs a deterministic, model-agnostic heuristic (approximately 1 token per 4 characters) 
    /// coupled with fixed structural overheads to ensure uniform token predictability across different computational providers.
    /// Consistently applying one approximation model across all reduction phases prevents alignment failures.
    /// </summary>
    internal static class ChatTokenEstimator
    {
        private const int BaseMessageOverheadTokens = 8;
        private const int ItemOverheadTokens = 16;

        /// <summary>
        /// Estimates the actual serialized prompt after transport normalization.
        /// Image pixels have a fixed estimate; base64 size is not text token usage.
        /// Request settings and response reserves do not occupy input context.
        /// </summary>
        internal static int? EstimateRequestTokens(string body)
        {
            try
            {
                var request = JObject.Parse(body);
                if (request["messages"] is not JArray messages) return null;
                int tokens = request["tools"]?.Type == JTokenType.Null ? 0 : EstimateTokens(request["tools"]?.ToString(Formatting.None));
                foreach (JToken message in messages)
                    tokens += EstimateWireMessageTokens(message);
                return tokens;
            }
            catch (JsonException) { return null; }
        }

        internal static int EstimateResponseTokens(string body)
        {
            try
            {
                // Only the selected completion is retained, never sum usage across calls.
                JToken message = JObject.Parse(body)["choices"]?.First?["message"];
                return message == null ? 0 : EstimateWireMessageTokens(message);
            }
            catch (JsonException) { return 0; }
        }

        private static int EstimateWireMessageTokens(JToken message)
        {
            int tokens = BaseMessageOverheadTokens;
            JToken content = message["content"];
            if (content is JArray parts)
            {
                foreach (JToken part in parts)
                    tokens += (string)part["type"] == "image_url"
                        ? WorkspaceScreenshotContent.EstimatedImageTokens
                        : EstimateTokens((string)part["text"]);
            }
            else if (content?.Type == JTokenType.String)
                tokens += EstimateTokens((string)content);
            tokens += EstimateTokens(message["tool_calls"]?.ToString(Formatting.None));
            tokens += EstimateTokens((string)message["name"]);
            return tokens;
        }

        public static int EstimateTokens(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return 0;
            }

            if (WorkspaceScreenshotContent.TryParse(content, out _))
                return WorkspaceScreenshotContent.EstimatedImageTokens;

            return Math.Max(1, (int)Math.Ceiling(content.Length / 4.0));
        }

        public static int EstimateMessageTokens(ChatMessageContent message)
        {
            if (message == null)
            {
                return 0;
            }

            int tokens = BaseMessageOverheadTokens + EstimateTokens(message.Content);
            if (message.Items != null)
            {
                tokens += message.Items.Count * ItemOverheadTokens;
                foreach (KernelContent item in message.Items)
                {
                    // Tool messages commonly carry their payload only in Items, not Content.
                    if (item is FunctionResultContent result)
                        tokens += EstimateTokens(result.Result is string text ? text : JsonConvert.SerializeObject(result.Result));
                    else if (item is FunctionCallContent call && call.Arguments != null)
                        tokens += EstimateTokens(JsonConvert.SerializeObject(call.Arguments));
                }
            }

            return tokens;
        }

        public static int EstimateMessagesTokens(IReadOnlyList<ChatMessageContent> messages)
        {
            if (messages == null || messages.Count == 0)
            {
                return 0;
            }

            int total = 0;
            for (int i = 0; i < messages.Count; i++)
            {
                total += EstimateMessageTokens(messages[i]);
            }

            return total;
        }
    }

    /// <summary>
    /// Defines fixed token allocation constraints for a specific model configuration.
    /// Distributes the total effective context window into static upper bounds for system prompts, tools, user input, and responses.
    /// The constraints are evaluated selectively when the agent ecosystem undergoes architectural changes.
    /// </summary>
    internal sealed class PromptBudgetProfile
    {
        public string ModelId { get; set; }
        public int ContextWindowTokens { get; set; }
        public int EffectiveContextWindowTokens { get; set; }
        public int UncertaintyReserveTokens { get; set; }
        public int MaxSystemAndToolTokens { get; set; }
        public int MaxUserInputTokens { get; set; }
        public int SystemPromptTokens { get; set; }
        public int ToolSchemaTokens { get; set; }
        public int SystemPromptBudgetTokens { get; set; }
        public int ToolSchemaBudgetTokens { get; set; }
        public int ResponseReserveTokens { get; set; }
        public int SafetyMarginTokens { get; set; }
    }

    /// <summary>
    /// Represents the dynamic, per-request token budget tracking constraint.
    /// Adapts the static boundaries from the <see cref="PromptBudgetProfile"/> to accommodate the actual algorithmic footprint 
    /// of the active user input, effectively yielding residual margins to background processing (tool parsing and semantic traces).
    /// </summary>
    internal sealed class PromptBudget
    {
        public int ContextWindowTokens { get; set; }
        public int EffectiveContextWindowTokens { get; set; }
        public int UncertaintyReserveTokens { get; set; }
        public int UserInputTokens { get; set; }
        public int HistoryBudgetTokens { get; set; }
        public int EstimatedHistoryTokens { get; set; }
        public int SystemPromptBudgetTokens { get; set; }
        public int ToolSchemaBudgetTokens { get; set; }
        public int ResponseReserveTokens { get; set; }
        public int SafetyMarginTokens { get; set; }
        public int KeepRawBudgetTokens { get; set; }
        public int SummaryBudgetTokens { get; set; }
        public int PreflightPlannedTokens { get; set; }
        public bool PreflightPassed { get; set; }
    }

    /// <summary>
    /// Encodes the finalized historical payload array alongside diagnostic markers detailing which 
    /// semantic contraction strategies were successfully executed to resolve overflow preconditions.
    /// </summary>
    internal sealed class HistoryPreparationResult
    {
        public List<ChatMessageContent> InvocationMessages { get; set; }
        public bool UseReducedThread { get; set; }
        public bool Summarized { get; set; }
        public bool Trimmed { get; set; }
        public bool CompressedToolOutputs { get; set; }
        public int RemovedMessagesCount { get; set; }
        public PromptBudget Budget { get; set; }
        public bool CacheApplied { get; set; }
        public bool CacheUpdated { get; set; }
        public int CacheCoveredSourceMessages { get; set; }
        public int FullHistoryCount { get; set; }
        public int CandidateHistoryCount { get; set; }
        public int ReducedHistoryCount { get; set; }
        public int UsedHistoryTokens { get; set; }
    }

    /// <summary>
    /// Responsible for calculating rigid fractional bounds for token capacities to preempt hardware or network-layer context overflow objections.
    /// Employs proportional heuristics indexing the model's raw context frame to interleave active context states optimally.
    /// </summary>
    internal static class PromptBudgetPlanner
    {
        private const string BudgetInvariantErrorCode = "CTX-BUDGET-ALLOC-001";
        private const string BudgetInvariantRangeErrorCode = "CTX-BUDGET-RANGE-001";
        private const int ReferenceContextWindowTokens = 4096;
        private const double MaxSystemAndToolShare = 0.50;
        private const double MaxUserInputShare = 0.20;
        private const double SafetyShare = 0.03;
        private const double ResponseReserveShare = 0.09;
        private const double KeepRawWithinHistoryShare = 0.35;
        private const double SummaryWithinHistoryShare = 0.25;

        /// <summary>
        /// Bootstraps the foundational budget limits mapping for a targeted model parameter set, reserving space for static invariants 
        /// (e.g. baseline tooling descriptors). Aborts preemptively if static definitions immediately monopolize context bounds.
        /// </summary>
        public static PromptBudgetProfile BuildProfile(string modelId, string systemPrompt, int estimatedToolSchemaTokens)
        {
            if (!ModelContextWindowResolver.TryResolveContextWindowTokens(modelId, out int contextWindowTokens))
            {
                throw new InvalidOperationException(
                    $"No context window is configured for model '{modelId}'. Please set it in AI chat settings.");
            }

            int systemPromptTokens = ChatTokenEstimator.EstimateTokens(systemPrompt);
            int toolSchemaTokens = Math.Max(0, estimatedToolSchemaTokens);
            double uncertaintyShare = ComputeUncertaintyShare(contextWindowTokens);
            int uncertaintyTokens = PercentFloor(contextWindowTokens, uncertaintyShare);
            int effectiveContextTokens = Math.Max(1, contextWindowTokens - uncertaintyTokens);

            int maxSystemAndToolTokens = PercentFloor(effectiveContextTokens, MaxSystemAndToolShare);
            int usedSystemAndToolTokens = systemPromptTokens + toolSchemaTokens;
            if (usedSystemAndToolTokens > maxSystemAndToolTokens)
            {
                string message =
                    $"System prompt + tool schema exceed the allowed budget for model '{modelId}'. " +
                    $"Used={usedSystemAndToolTokens}, Allowed={maxSystemAndToolTokens} " +
                    $"(50% of effective context window {effectiveContextTokens}; raw context={contextWindowTokens}, uncertaintyReserve={uncertaintyTokens}).";
                Log.Error(message);
                throw new InvalidOperationException(message);
            }

            int maxUserInputTokens = PercentFloor(effectiveContextTokens, MaxUserInputShare);
            int safetyTokens = PercentFloor(effectiveContextTokens, SafetyShare);
            int responseReserveTokens = PercentFloor(effectiveContextTokens, ResponseReserveShare);

            PromptBudgetProfile profile = new PromptBudgetProfile
            {
                ModelId = modelId ?? string.Empty,
                ContextWindowTokens = contextWindowTokens,
                EffectiveContextWindowTokens = effectiveContextTokens,
                UncertaintyReserveTokens = uncertaintyTokens,
                MaxSystemAndToolTokens = maxSystemAndToolTokens,
                MaxUserInputTokens = maxUserInputTokens,
                SystemPromptTokens = systemPromptTokens,
                ToolSchemaTokens = toolSchemaTokens,
                SystemPromptBudgetTokens = systemPromptTokens,
                ToolSchemaBudgetTokens = toolSchemaTokens,
                ResponseReserveTokens = responseReserveTokens,
                SafetyMarginTokens = safetyTokens
            };

            Log.Info(
                $"Prompt budget profile calculated: model={profile.ModelId}, rawContext={profile.ContextWindowTokens}, " +
                $"uncertaintyShare={uncertaintyShare:F5}, uncertaintyReserve={profile.UncertaintyReserveTokens}, effectiveContext={profile.EffectiveContextWindowTokens}, " +
                $"systemTokens={profile.SystemPromptTokens}, toolSchemaTokens={profile.ToolSchemaTokens}, " +
                $"systemPlusTool={usedSystemAndToolTokens}/{profile.MaxSystemAndToolTokens}, " +
                $"maxUser={profile.MaxUserInputTokens}, safety={profile.SafetyMarginTokens}, responseReserveBase={profile.ResponseReserveTokens}");
            LogProfileInvariantDiagnostics(profile);

            return profile;
        }

        /// <summary>
        /// Derives the final operational token allocation by overlaying input sizes onto the static <paramref name="profile"/>.
        /// Cascades unused fractional allocations from user requests into context retention (historical buffers and multi-turn tool limits).
        /// </summary>
        public static PromptBudget Calculate(
            PromptBudgetProfile profile,
            string userInput,
            IReadOnlyList<ChatMessageContent> history,
            string reason = null)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            string normalizedReason = string.IsNullOrWhiteSpace(reason) ? "default" : reason.Trim();
            int userInputTokens = ChatTokenEstimator.EstimateTokens(userInput);
            if (userInputTokens > profile.MaxUserInputTokens)
            {
                string message =
                    $"User message exceeds the allowed budget for model '{profile.ModelId}'. " +
                    $"Used={userInputTokens}, Allowed={profile.MaxUserInputTokens} " +
                    $"(20% of effective context window {profile.EffectiveContextWindowTokens}; raw context={profile.ContextWindowTokens}, uncertaintyReserve={profile.UncertaintyReserveTokens}).";
                Log.Error(message);
                throw new InvalidOperationException(message);
            }

            int usedSystemAndToolTokens = profile.SystemPromptTokens + profile.ToolSchemaTokens;
            int safetyMarginTokens = profile.SafetyMarginTokens;
            int responseReserveTokens = profile.ResponseReserveTokens;
            // All remaining input capacity belongs to retained context. There is
            // no separate allocation or cumulative quota for tool interactions.
            int historyBudgetTokens = Math.Max(0, profile.EffectiveContextWindowTokens -
                usedSystemAndToolTokens - userInputTokens - safetyMarginTokens - responseReserveTokens);

            int estimatedHistoryTokens = ChatTokenEstimator.EstimateMessagesTokens(history);
            int keepRawBudgetTokens = Math.Max(1, PercentFloor(historyBudgetTokens, KeepRawWithinHistoryShare));
            int summaryBudgetTokens = Math.Max(1, PercentFloor(historyBudgetTokens, SummaryWithinHistoryShare));

            PromptBudget budget = new PromptBudget
            {
                ContextWindowTokens = profile.ContextWindowTokens,
                EffectiveContextWindowTokens = profile.EffectiveContextWindowTokens,
                UncertaintyReserveTokens = profile.UncertaintyReserveTokens,
                UserInputTokens = userInputTokens,
                HistoryBudgetTokens = historyBudgetTokens,
                EstimatedHistoryTokens = estimatedHistoryTokens,
                SystemPromptBudgetTokens = profile.SystemPromptTokens,
                ToolSchemaBudgetTokens = profile.ToolSchemaTokens,
                ResponseReserveTokens = responseReserveTokens,
                SafetyMarginTokens = safetyMarginTokens,
                KeepRawBudgetTokens = keepRawBudgetTokens,
                SummaryBudgetTokens = summaryBudgetTokens
            };
            UpdatePreflightEstimate(budget);
            LogBudgetInvariantDiagnostics(profile, budget, normalizedReason);

            Log.Info(
                $"Prompt budgets calculated ({normalizedReason}): rawContext={profile.ContextWindowTokens}, effectiveContext={profile.EffectiveContextWindowTokens}, uncertaintyReserve={profile.UncertaintyReserveTokens}, " +
                $"system={budget.SystemPromptBudgetTokens}, toolSchema={budget.ToolSchemaBudgetTokens}, user={budget.UserInputTokens}, " +
                $"history={budget.HistoryBudgetTokens}, responseReserve={budget.ResponseReserveTokens}, safety={budget.SafetyMarginTokens}, " +
                $"preflightPlanned={budget.PreflightPlannedTokens}, preflightAllowed={budget.EffectiveContextWindowTokens}, preflightPassed={budget.PreflightPassed}");

            return budget;
        }

        public static bool ValidatePreflight(PromptBudget budget, out int plannedTokens, out int allowedTokens, out int overflowTokens)
        {
            plannedTokens = 0;
            allowedTokens = 0;
            overflowTokens = 0;

            if (budget == null)
            {
                return false;
            }

            UpdatePreflightEstimate(budget);
            plannedTokens = budget.PreflightPlannedTokens;
            allowedTokens = Math.Max(0, budget.EffectiveContextWindowTokens);
            overflowTokens = Math.Max(0, plannedTokens - allowedTokens);
            return overflowTokens <= 0;
        }

        private static void UpdatePreflightEstimate(PromptBudget budget)
        {
            if (budget == null)
            {
                return;
            }

            int plannedTokens =
                Math.Max(0, budget.SystemPromptBudgetTokens) +
                Math.Max(0, budget.ToolSchemaBudgetTokens) +
                Math.Max(0, budget.UserInputTokens) +
                Math.Max(0, budget.EstimatedHistoryTokens) +
                Math.Max(0, budget.ResponseReserveTokens) +
                Math.Max(0, budget.SafetyMarginTokens);

            budget.PreflightPlannedTokens = plannedTokens;
            budget.PreflightPassed = plannedTokens <= Math.Max(0, budget.EffectiveContextWindowTokens);
        }

        private static void LogProfileInvariantDiagnostics(PromptBudgetProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            if (profile.EffectiveContextWindowTokens <= 0 || profile.EffectiveContextWindowTokens > profile.ContextWindowTokens)
            {
                Log.Error(
                    $"[{BudgetInvariantRangeErrorCode}] Invalid context profile range: model={profile.ModelId}, rawContext={profile.ContextWindowTokens}, " +
                    $"effectiveContext={profile.EffectiveContextWindowTokens}, uncertaintyReserve={profile.UncertaintyReserveTokens}.");
            }

            int capacityWithMaxUser =
                Math.Max(0, profile.SystemPromptTokens) +
                Math.Max(0, profile.ToolSchemaTokens) +
                Math.Max(0, profile.MaxUserInputTokens) +
                PercentFloor(profile.EffectiveContextWindowTokens, ResponseReserveShare) +
                PercentFloor(profile.EffectiveContextWindowTokens, SafetyShare);
            if (capacityWithMaxUser > profile.EffectiveContextWindowTokens)
            {
                Log.Error(
                    $"[{BudgetInvariantErrorCode}] Base profile capacity exceeds effective context: model={profile.ModelId}, capacity={capacityWithMaxUser}, " +
                    $"effectiveContext={profile.EffectiveContextWindowTokens}, rawContext={profile.ContextWindowTokens}, uncertaintyReserve={profile.UncertaintyReserveTokens}.");
            }
        }

        private static void LogBudgetInvariantDiagnostics(PromptBudgetProfile profile, PromptBudget budget, string reason)
        {
            if (budget == null)
            {
                return;
            }

            string normalizedReason = string.IsNullOrWhiteSpace(reason) ? "default" : reason.Trim();
            int effectiveContext = Math.Max(0, budget.EffectiveContextWindowTokens);
            int allocatedCapacity =
                Math.Max(0, budget.SystemPromptBudgetTokens) +
                Math.Max(0, budget.ToolSchemaBudgetTokens) +
                Math.Max(0, budget.UserInputTokens) +
                Math.Max(0, budget.HistoryBudgetTokens) +
                Math.Max(0, budget.ResponseReserveTokens) +
                Math.Max(0, budget.SafetyMarginTokens);

            if (budget.EffectiveContextWindowTokens <= 0 || budget.EffectiveContextWindowTokens > budget.ContextWindowTokens)
            {
                Log.Error(
                    $"[{BudgetInvariantRangeErrorCode}] Invalid budget context range ({normalizedReason}): rawContext={budget.ContextWindowTokens}, " +
                    $"effectiveContext={budget.EffectiveContextWindowTokens}, uncertaintyReserve={budget.UncertaintyReserveTokens}.");
            }

            if (allocatedCapacity > effectiveContext)
            {
                Log.Error(
                    $"[{BudgetInvariantErrorCode}] Budget allocation exceeds effective context ({normalizedReason}): allocated={allocatedCapacity}, " +
                    $"effectiveContext={effectiveContext}, rawContext={budget.ContextWindowTokens}, uncertaintyReserve={budget.UncertaintyReserveTokens}, " +
                    $"system={budget.SystemPromptBudgetTokens}, toolSchema={budget.ToolSchemaBudgetTokens}, user={budget.UserInputTokens}, history={budget.HistoryBudgetTokens}, " +
                    $"responseReserve={budget.ResponseReserveTokens}, safety={budget.SafetyMarginTokens}.");
            }

            if (profile != null)
            {
                int expectedSystemTool = Math.Max(0, budget.SystemPromptBudgetTokens) + Math.Max(0, budget.ToolSchemaBudgetTokens);
                if (expectedSystemTool > Math.Max(0, profile.MaxSystemAndToolTokens))
                {
                    Log.Error(
                        $"[{BudgetInvariantErrorCode}] System+tool budget exceeds maxSystemAndTool ({normalizedReason}): used={expectedSystemTool}, allowed={profile.MaxSystemAndToolTokens}, model={profile.ModelId}.");
                }
            }
        }

        private static double ComputeUncertaintyShare(int contextWindowTokens)
        {
            if (contextWindowTokens <= 0)
            {
                return 0.10;
            }

            double uncertaintyShare = 0.03 + (0.07 * (ReferenceContextWindowTokens / (double)contextWindowTokens));
            return Math.Max(0.03, uncertaintyShare);
        }

        private static int PercentFloor(int totalTokens, double fraction)
        {
            if (totalTokens <= 0 || fraction <= 0)
            {
                return 0;
            }

            return Math.Max(0, (int)Math.Floor(totalTokens * fraction));
        }
    }

    /// <summary>
    /// Orchestrates semantic and selective geometric condensing regimens applied to the conversation trace to satisfy boundary limitations calculated by the <see cref="PromptBudgetPlanner"/>.
    /// Resolves volume constraints sequentially via cached overlaps, trivial threshold truncations, deterministic tool volume compression, LLM abstractive summarization, and eventually structural hard removal routines.
    /// </summary>
    internal sealed class ChatHistoryReductionEngine
    {
        private const string HistoryBudgetViolationErrorCode = "CTX-HIST-001";
        private const string HistoryPreflightWarningCode = "CTX-HIST-002";
        private const string HistorySummaryPrefix = "[history-summary]";
        private const string ToolSummaryPrefix = "[tool-summary]";
        private const string BudgetSkipToolMessagePrefix = "Skipped tool call due to limited context budget";
        private const int RecentTurnWindow = 2;
        private const int SummaryLineMaxChars = 260;
        private const int SummaryPreviewLines = 8;

        /// <summary>
        /// Central translation conduit mapping an unstructured history sequence into an enforced token-constrained framework.
        /// Guarantees structural parity internally by reconstructing partially pruned function call relationships reliably context-locking dependencies.
        /// </summary>
        public async Task<HistoryPreparationResult> PrepareForInvocationAsync(
            AIThread thread,
            string modelId,
            string userInput,
            ChatHistory fullHistory,
            PromptBudgetProfile profile,
            IChatCompletionService chatCompletionService,
            CancellationToken cancellationToken)
        {
            List<ChatMessageContent> fullMessages = CloneMessages(fullHistory?.ToList());
            _ = PromptBudgetPlanner.Calculate(profile, userInput, fullMessages, "initial-full-history");

            List<ChatMessageContent> candidateMessages = BuildCandidateMessagesFromCache(
                thread,
                fullMessages,
                out int cacheCoveredSourceMessages,
                out bool cacheApplied);
            PromptBudget candidateBudget = PromptBudgetPlanner.Calculate(profile, userInput, candidateMessages, "candidate-with-cache");
            int reductionHistoryLimit = GetReductionHistoryLimit(candidateBudget);

            int oversizedToolThreshold = ComputeOversizedToolThreshold(candidateBudget.HistoryBudgetTokens);
            bool hasOversizedToolOutputs = ContainsOversizedToolMessages(candidateMessages, oversizedToolThreshold);
            bool shouldReduce =
                ShouldRunReducer(profile, candidateBudget, hasOversizedToolOutputs) ||
                HasLowValueMessages(candidateMessages);

            List<ChatMessageContent> finalMessages = candidateMessages;
            bool summarized = false;
            bool trimmed = false;
            bool compressedToolOutputs = false;
            int removedMessagesCount = 0;

            if (shouldReduce)
            {
                List<HistoryMessageDescriptor> descriptors = DescribeMessages(candidateMessages);
                List<ChatMessageContent> withoutNoise = RemoveLowValueMessages(descriptors, out removedMessagesCount);

                PromptBudget noiseBudget = PromptBudgetPlanner.Calculate(profile, userInput, withoutNoise, "after-noise-removal");
                List<HistoryMessageDescriptor> compactDescriptors = DescribeMessages(withoutNoise);
                List<ChatMessageContent> compressed = CompressLargeToolMessages(
                    compactDescriptors,
                    noiseBudget.HistoryBudgetTokens,
                    out compressedToolOutputs);

                PromptBudget compressedBudget = PromptBudgetPlanner.Calculate(profile, userInput, compressed, "after-tool-compression");
                // Apply the requested target to both the summary and retained raw messages.
                compressedBudget.HistoryBudgetTokens = reductionHistoryLimit;
                compressedBudget.SummaryBudgetTokens = Math.Max(1, reductionHistoryLimit / 4);
                compressedBudget.KeepRawBudgetTokens = Math.Max(1, reductionHistoryLimit * 35 / 100);
                List<HistoryMessageDescriptor> summaryDescriptors = DescribeMessages(compressed);
                HistoryPartition partition = PartitionForComposition(summaryDescriptors);

                List<ChatMessageContent> summaryMessages = new List<ChatMessageContent>();
                if (partition.SummarizableMessages.Count > 0)
                {
                    summaryMessages = await SummarizeOlderMessagesAsync(
                        partition.SummarizableMessages,
                        compressedBudget,
                        chatCompletionService,
                        cancellationToken);
                    summarized = summaryMessages.Count > 0;
                }

                List<ComposedMessage> composed = BuildComposedMessages(partition, summaryMessages);
                finalMessages = SelectMessagesWithinBudget(composed, compressedBudget.HistoryBudgetTokens, out int _, out trimmed);
            }

            finalMessages = EnsureValidToolMessageChains(finalMessages, out int normalizedToolMessagesCount);
            PromptBudget finalBudget = PromptBudgetPlanner.Calculate(profile, userInput, finalMessages, "final-before-trim");
            int usedHistoryTokens = finalBudget.EstimatedHistoryTokens;

            int finalHistoryLimit = Math.Min(finalBudget.HistoryBudgetTokens, reductionHistoryLimit);
            if (finalBudget.EstimatedHistoryTokens > finalHistoryLimit)
            {
                finalMessages = TrimMessagesToBudgetPreservingSystem(
                    finalMessages,
                    finalHistoryLimit,
                    out usedHistoryTokens);
                finalBudget = PromptBudgetPlanner.Calculate(profile, userInput, finalMessages, "final-after-trim");
                trimmed = true;

                // Trimming can separate tool results from their originating function-call message.
                finalMessages = EnsureValidToolMessageChains(finalMessages, out int normalizedAfterTrimCount);
                if (normalizedAfterTrimCount > 0)
                {
                    normalizedToolMessagesCount += normalizedAfterTrimCount;
                    finalBudget = PromptBudgetPlanner.Calculate(profile, userInput, finalMessages, "final-after-tool-chain-normalization");
                    usedHistoryTokens = finalBudget.EstimatedHistoryTokens;
                }
            }

            if (normalizedToolMessagesCount > 0)
            {
                // Log.Debug($"Normalized {normalizedToolMessagesCount} orphan tool messages to assistant memory for strict provider compatibility.");
            }

            LogFinalReductionDiagnostics(modelId, finalBudget, fullMessages.Count, finalMessages.Count, shouldReduce);

            bool useReducedThread = !AreEquivalentMessages(fullMessages, finalMessages);
            bool cacheUpdated = UpdateReducedHistoryCache(thread, finalMessages, fullMessages.Count, useReducedThread);

            return new HistoryPreparationResult
            {
                InvocationMessages = finalMessages,
                UseReducedThread = useReducedThread,
                Summarized = summarized,
                Trimmed = trimmed,
                CompressedToolOutputs = compressedToolOutputs,
                RemovedMessagesCount = removedMessagesCount,
                Budget = finalBudget,
                CacheApplied = cacheApplied,
                CacheUpdated = cacheUpdated,
                CacheCoveredSourceMessages = useReducedThread ? fullMessages.Count : cacheCoveredSourceMessages,
                FullHistoryCount = fullMessages.Count,
                CandidateHistoryCount = candidateMessages.Count,
                ReducedHistoryCount = finalMessages.Count,
                UsedHistoryTokens = usedHistoryTokens
            };
        }

        private static void LogFinalReductionDiagnostics(
            string modelId,
            PromptBudget budget,
            int fullHistoryCount,
            int reducedHistoryCount,
            bool reducerActivated)
        {
            if (budget == null)
            {
                Log.Error(
                    $"[{HistoryBudgetViolationErrorCode}] Missing final budget after history reduction: model={modelId ?? "<unknown>"}, fullHistoryMessages={fullHistoryCount}, reducedHistoryMessages={reducedHistoryCount}.");
                return;
            }

            if (budget.EstimatedHistoryTokens > budget.HistoryBudgetTokens)
            {
                Log.Error(
                    $"[{HistoryBudgetViolationErrorCode}] Final history exceeds history budget: model={modelId ?? "<unknown>"}, " +
                    $"historyEstimated={budget.EstimatedHistoryTokens}, historyBudget={budget.HistoryBudgetTokens}, reducerActivated={reducerActivated}, " +
                    $"fullHistoryMessages={fullHistoryCount}, reducedHistoryMessages={reducedHistoryCount}, preflightPlanned={budget.PreflightPlannedTokens}, effectiveContext={budget.EffectiveContextWindowTokens}.");
            }
            else
            {
                Log.Info(
                    $"History reduction check: model={modelId ?? "<unknown>"}, historyEstimated={budget.EstimatedHistoryTokens}/{budget.HistoryBudgetTokens}, " +
                    $"reducerActivated={reducerActivated}, fullHistoryMessages={fullHistoryCount}, reducedHistoryMessages={reducedHistoryCount}, preflightPassed={budget.PreflightPassed}.");
            }

            if (!budget.PreflightPassed)
            {
                int overflow = Math.Max(0, budget.PreflightPlannedTokens - budget.EffectiveContextWindowTokens);
                Log.Warning(
                    $"[{HistoryPreflightWarningCode}] Final reduced prompt still exceeds effective context preflight and will be blocked before invoke: " +
                    $"model={modelId ?? "<unknown>"}, planned={budget.PreflightPlannedTokens}, effectiveContext={budget.EffectiveContextWindowTokens}, overflow={overflow}, " +
                    $"rawContext={budget.ContextWindowTokens}, uncertaintyReserve={budget.UncertaintyReserveTokens}.");
            }
        }

        private static bool ShouldRunReducer(PromptBudgetProfile profile, PromptBudget budget, bool hasOversizedToolOutputs)
        {
            if (profile == null || budget == null)
            {
                return false;
            }

            if (budget.EstimatedHistoryTokens > budget.HistoryBudgetTokens)
            {
                return true;
            }

            if (GetReductionHistoryLimit(budget) < budget.HistoryBudgetTokens)
            {
                return true;
            }

            return hasOversizedToolOutputs;
        }

        /// <summary>
        /// Percentages use the available history budget after model/system/tool reserves.
        /// Invalid manually edited settings are clamped; target must remain below trigger.
        /// Disabling proactive compaction never disables mandatory overflow protection.
        /// </summary>
        private static int GetReductionHistoryLimit(PromptBudget budget)
        {
            int trigger = Math.Max(2, Math.Min(100, Settings.Default.contextCompressionTriggerPercent));
            int target = Math.Max(1, Math.Min(trigger - 1, Settings.Default.contextCompressionTargetPercent));
            if (!Settings.Default.autoCompressContext ||
                budget.EstimatedHistoryTokens < budget.HistoryBudgetTokens * (trigger / 100.0))
                return budget.HistoryBudgetTokens;
            return Math.Max(1, (int)Math.Floor(budget.HistoryBudgetTokens * (target / 100.0)));
        }

        /// <summary>
        /// Asynchronously pipelines older traces out to an embedded text-truncation adapter instance.
        /// Flattens aged conversational chains into synthetic assistant blocks retaining density without incurring raw contextual volume scaling penalties.
        /// </summary>
        private async Task<List<ChatMessageContent>> SummarizeOlderMessagesAsync(
            IReadOnlyList<ChatMessageContent> olderMessages,
            PromptBudget budget,
            IChatCompletionService chatCompletionService,
            CancellationToken cancellationToken)
        {
            if (olderMessages == null || olderMessages.Count == 0)
            {
                return new List<ChatMessageContent>();
            }

            ChatHistory source = new ChatHistory();
            for (int i = 0; i < olderMessages.Count; i++)
            {
                source.Add(CloneMessage(olderMessages[i]));
            }

            List<ChatMessageContent> reducedMessages;
            try
            {
                int averageTokens = Math.Max(1, ChatTokenEstimator.EstimateMessagesTokens(olderMessages) / olderMessages.Count);
                int targetMessages = Math.Max(2, Math.Min(24, Math.Max(2, budget.SummaryBudgetTokens / averageTokens)));
                int summarizationThreshold = Math.Max(1, targetMessages / 2);

                IChatHistoryReducer reducer = new ChatHistorySummarizationReducer(
                    chatCompletionService,
                    targetMessages,
                    summarizationThreshold);

                IEnumerable<ChatMessageContent> reduced = await reducer.ReduceAsync(source, cancellationToken);
                reducedMessages = reduced?.ToList() ?? new List<ChatMessageContent>();
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception)
            {
                // Log.Warning("Chat history summarization failed. Falling back to deterministic summary.");
                reducedMessages = olderMessages.ToList();
            }

            ChatMessageContent summaryMemory = BuildSummaryMemoryMessage(
                reducedMessages.Count > 0 ? reducedMessages : olderMessages,
                olderMessages.Count,
                ChatTokenEstimator.EstimateMessagesTokens(olderMessages),
                budget.SummaryBudgetTokens);

            return new List<ChatMessageContent> { summaryMemory };
        }

        private static ChatMessageContent BuildSummaryMemoryMessage(
            IReadOnlyList<ChatMessageContent> summarySource,
            int coveredMessages,
            int coveredTokens,
            int summaryBudgetTokens)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(HistorySummaryPrefix);
            sb.Append(" Task memory block. ");
            sb.Append("Covered ");
            sb.Append(coveredMessages);
            sb.Append(" messages (");
            sb.Append(coveredTokens);
            sb.Append(" estimated tokens).");
            sb.AppendLine();

            int appended = 0;
            for (int i = 0; i < summarySource.Count && appended < SummaryPreviewLines; i++)
            {
                ChatMessageContent message = summarySource[i];
                string content = CompactWhitespace(message?.Content, SummaryLineMaxChars);
                if (string.IsNullOrWhiteSpace(content))
                {
                    continue;
                }

                sb.Append("- ");
                sb.Append(message.Role.ToString().ToLowerInvariant());
                sb.Append(": ");
                sb.AppendLine(content);
                appended++;
            }

            string summaryText = TrimContentToTokenBudget(sb.ToString().Trim(), summaryBudgetTokens);
            return new ChatMessageContent(AuthorRole.Assistant, summaryText);
        }

        private static List<ChatMessageContent> BuildCandidateMessagesFromCache(
            AIThread thread,
            IReadOnlyList<ChatMessageContent> fullMessages,
            out int cacheCoveredSourceMessages,
            out bool cacheApplied)
        {
            cacheCoveredSourceMessages = 0;
            cacheApplied = false;

            if (fullMessages == null || fullMessages.Count == 0 || thread == null)
            {
                return CloneMessages(fullMessages);
            }

            List<ChatMessageContent> cachedMessages = thread.ReducedHistoryCacheMessages;
            int sourceCount = thread.ReducedHistoryCacheSourceMessageCount;
            if (cachedMessages == null || cachedMessages.Count == 0 || sourceCount <= 0 || sourceCount > fullMessages.Count)
            {
                return CloneMessages(fullMessages);
            }

            List<ChatMessageContent> candidate = CloneMessages(cachedMessages);
            for (int i = sourceCount; i < fullMessages.Count; i++)
            {
                candidate.Add(CloneMessage(fullMessages[i]));
            }

            cacheCoveredSourceMessages = sourceCount;
            cacheApplied = true;
            return candidate;
        }

        private static bool UpdateReducedHistoryCache(
            AIThread thread,
            IReadOnlyList<ChatMessageContent> reducedMessages,
            int sourceMessageCount,
            bool useReducedThread)
        {
            if (thread == null)
            {
                return false;
            }

            if (useReducedThread && reducedMessages != null && reducedMessages.Count > 0 && sourceMessageCount > 0)
            {
                thread.SetReducedHistoryCache(reducedMessages, sourceMessageCount);
                return true;
            }

            if ((thread.ReducedHistoryCacheMessages?.Count ?? 0) > 0 || thread.ReducedHistoryCacheSourceMessageCount > 0)
            {
                thread.ClearReducedHistoryCache();
                return true;
            }

            return false;
        }

        private static List<HistoryMessageDescriptor> DescribeMessages(IReadOnlyList<ChatMessageContent> messages)
        {
            List<HistoryMessageDescriptor> descriptors = new List<HistoryMessageDescriptor>();
            if (messages == null || messages.Count == 0)
            {
                return descriptors;
            }

            int[] turnIndices = BuildTurnIndices(messages);
            int newestTurn = turnIndices.Max();
            int recentTurnCutoff = Math.Max(0, newestTurn - (RecentTurnWindow - 1));
            HashSet<int> openToolChainIndices = DetectOpenToolChainIndices(messages);
            int lastUserIndex = FindLastIndex(messages, AuthorRole.User);
            int lastAssistantIndex = FindLastAssistantTextIndex(messages);

            for (int i = 0; i < messages.Count; i++)
            {
                ChatMessageContent message = messages[i];
                int messageTokens = ChatTokenEstimator.EstimateMessageTokens(message);
                bool isSystem = message.Role == AuthorRole.System;
                bool isTool = message.Role == AuthorRole.Tool;
                bool isRecent = turnIndices[i] >= recentTurnCutoff && turnIndices[i] >= 0;
                bool isLocked =
                    isSystem ||
                    i == lastUserIndex ||
                    i == lastAssistantIndex ||
                    openToolChainIndices.Contains(i);

                MessageRetentionClass retentionClass = MessageRetentionClass.Summarize;
                if (string.IsNullOrWhiteSpace(message.Content) && message.Items.Count == 0)
                {
                    retentionClass = MessageRetentionClass.Remove;
                }
                else if (isLocked || isRecent)
                {
                    retentionClass = MessageRetentionClass.KeepRaw;
                }

                descriptors.Add(new HistoryMessageDescriptor
                {
                    Index = i,
                    Message = message,
                    Tokens = messageTokens,
                    IsSystem = isSystem,
                    IsTool = isTool,
                    IsLocked = isLocked,
                    RetentionClass = retentionClass
                });
            }

            return descriptors;
        }

        private static HistoryPartition PartitionForComposition(IReadOnlyList<HistoryMessageDescriptor> descriptors)
        {
            HistoryPartition partition = new HistoryPartition();
            if (descriptors == null || descriptors.Count == 0)
            {
                return partition;
            }

            for (int i = 0; i < descriptors.Count; i++)
            {
                HistoryMessageDescriptor descriptor = descriptors[i];
                if (descriptor.IsSystem)
                {
                    partition.SystemMessages.Add(descriptor);
                    continue;
                }

                switch (descriptor.RetentionClass)
                {
                    case MessageRetentionClass.KeepRaw:
                        partition.KeepRawMessages.Add(descriptor);
                        break;
                    case MessageRetentionClass.Remove:
                        break;
                    default:
                        partition.SummarizableMessages.Add(descriptor.Message);
                        break;
                }
            }

            return partition;
        }

        /// <summary>
        /// Intercepts non-essential internal signaling abstractions (such as runtime skip markers) 
        /// clearing nominal space before more intensive LLM processes enact.
        /// </summary>
        private static List<ChatMessageContent> RemoveLowValueMessages(
            IReadOnlyList<HistoryMessageDescriptor> descriptors,
            out int removedMessagesCount)
        {
            removedMessagesCount = 0;
            if (descriptors == null || descriptors.Count == 0)
            {
                return new List<ChatMessageContent>();
            }

            int newestBudgetSkipIndex = descriptors
                .Where(d => IsToolBudgetSkipMessage(d.Message))
                .Select(d => d.Index)
                .DefaultIfEmpty(-1)
                .Max();

            List<ChatMessageContent> result = new List<ChatMessageContent>(descriptors.Count);
            for (int i = 0; i < descriptors.Count; i++)
            {
                HistoryMessageDescriptor descriptor = descriptors[i];
                bool remove = descriptor.RetentionClass == MessageRetentionClass.Remove;

                // Keep only the newest budget-skip marker to avoid repetitive noise.
                if (!descriptor.IsLocked &&
                    descriptor.IsTool &&
                    descriptor.Index != newestBudgetSkipIndex &&
                    IsToolBudgetSkipMessage(descriptor.Message))
                {
                    remove = true;
                }

                if (remove)
                {
                    removedMessagesCount++;
                    continue;
                }

                result.Add(CloneMessage(descriptor.Message));
            }

            return result;
        }

        /// <summary>
        /// Identifies disproportionate raw tool evaluations embedded in historical segments, structurally transcoding 
        /// dense outputs efficiently into abstract descriptive identifiers acting as stand-ups ensuring validation integrity.
        /// </summary>
        private static List<ChatMessageContent> CompressLargeToolMessages(
            IReadOnlyList<HistoryMessageDescriptor> descriptors,
            int historyBudgetTokens,
            out bool compressedToolOutputs)
        {
            compressedToolOutputs = false;
            if (descriptors == null || descriptors.Count == 0)
            {
                return new List<ChatMessageContent>();
            }

            int oversizedToolThreshold = ComputeOversizedToolThreshold(historyBudgetTokens);
            List<ChatMessageContent> result = new List<ChatMessageContent>(descriptors.Count);

            for (int i = 0; i < descriptors.Count; i++)
            {
                HistoryMessageDescriptor descriptor = descriptors[i];
                if (descriptor.IsTool &&
                    descriptor.RetentionClass == MessageRetentionClass.Summarize &&
                    descriptor.Tokens >= oversizedToolThreshold)
                {
                    string summary = BuildToolOutputSummary(descriptor.Message, descriptor.Tokens);
                    // Synthetic tool summaries must be assistant text to avoid strict
                    // provider validation that requires a function-result payload.
                    result.Add(new ChatMessageContent(AuthorRole.Assistant, summary));
                    compressedToolOutputs = true;
                    continue;
                }

                result.Add(CloneMessage(descriptor.Message));
            }

            return result;
        }

        private static string BuildToolOutputSummary(ChatMessageContent toolMessage, int originalTokens)
        {
            string functionName = TryExtractFunctionName(toolMessage);
            if (toolMessage?.Items.OfType<FunctionResultContent>().Any(result =>
                result.Result is string text && WorkspaceScreenshotContent.TryParse(text, out _)) == true)
                return $"{ToolSummaryPrefix} {functionName}: a workspace screenshot was captured. Request a new screenshot to inspect the current layout.";
            string preview = CompactWhitespace(toolMessage?.Content, 220);
            if (string.IsNullOrWhiteSpace(preview))
            {
                preview = "(empty)";
            }

            return $"{ToolSummaryPrefix} {functionName} output compressed. Original size ~{originalTokens} tokens. Preview: {preview}";
        }

        private static List<ComposedMessage> BuildComposedMessages(
            HistoryPartition partition,
            IReadOnlyList<ChatMessageContent> summaryMessages)
        {
            List<ComposedMessage> composed = new List<ComposedMessage>();
            int order = 0;

            foreach (HistoryMessageDescriptor system in partition.SystemMessages.OrderBy(d => d.Index))
            {
                composed.Add(new ComposedMessage
                {
                    Order = order++,
                    Message = CloneMessage(system.Message),
                    Tokens = system.Tokens,
                    IsMandatory = true,
                    IsSystem = true
                });
            }

            if (summaryMessages != null)
            {
                for (int i = 0; i < summaryMessages.Count; i++)
                {
                    ChatMessageContent summary = summaryMessages[i];
                    composed.Add(new ComposedMessage
                    {
                        Order = order++,
                        Message = CloneMessage(summary),
                        Tokens = ChatTokenEstimator.EstimateMessageTokens(summary),
                        IsMandatory = false,
                        IsSystem = false
                    });
                }
            }

            foreach (HistoryMessageDescriptor keepRaw in partition.KeepRawMessages.OrderBy(d => d.Index))
            {
                composed.Add(new ComposedMessage
                {
                    Order = order++,
                    Message = CloneMessage(keepRaw.Message),
                    Tokens = keepRaw.Tokens,
                    IsMandatory = true,
                    IsSystem = false
                });
            }

            return composed;
        }

        private static List<ChatMessageContent> SelectMessagesWithinBudget(
            IReadOnlyList<ComposedMessage> composedMessages,
            int historyBudgetTokens,
            out int usedHistoryTokens,
            out bool trimmed)
        {
            usedHistoryTokens = 0;
            trimmed = false;

            if (composedMessages == null || composedMessages.Count == 0)
            {
                return new List<ChatMessageContent>();
            }

            HashSet<int> selected = new HashSet<int>();
            int mandatoryTokens = 0;

            foreach (ComposedMessage message in composedMessages.Where(m => m.IsMandatory))
            {
                selected.Add(message.Order);
                mandatoryTokens += message.Tokens;
            }

            // If mandatory content alone is too large, keep only system messages as hard lock.
            if (mandatoryTokens > historyBudgetTokens)
            {
                selected.Clear();
                mandatoryTokens = 0;

                foreach (ComposedMessage systemMessage in composedMessages.Where(m => m.IsSystem))
                {
                    selected.Add(systemMessage.Order);
                    mandatoryTokens += systemMessage.Tokens;
                }
            }

            usedHistoryTokens = mandatoryTokens;

            List<ComposedMessage> optional = composedMessages
                .Where(m => !selected.Contains(m.Order))
                .OrderByDescending(m => m.Order)
                .ToList();

            foreach (ComposedMessage candidate in optional)
            {
                if (usedHistoryTokens + candidate.Tokens > historyBudgetTokens)
                {
                    continue;
                }

                selected.Add(candidate.Order);
                usedHistoryTokens += candidate.Tokens;
            }

            if (selected.Count == 0)
            {
                ComposedMessage newest = composedMessages.OrderByDescending(m => m.Order).First();
                selected.Add(newest.Order);
                usedHistoryTokens = newest.Tokens;
            }

            List<ChatMessageContent> result = composedMessages
                .Where(m => selected.Contains(m.Order))
                .OrderBy(m => m.Order)
                .Select(m => CloneMessage(m.Message))
                .ToList();

            trimmed = result.Count < composedMessages.Count;
            return result;
        }

        private static List<ChatMessageContent> TrimMessagesToBudgetPreservingSystem(
            IReadOnlyList<ChatMessageContent> messages,
            int historyBudgetTokens,
            out int usedHistoryTokens)
        {
            usedHistoryTokens = 0;
            if (messages == null || messages.Count == 0)
            {
                return new List<ChatMessageContent>();
            }

            List<ChatMessageContent> systemMessages = new List<ChatMessageContent>();
            List<ChatMessageContent> nonSystemMessages = new List<ChatMessageContent>();

            for (int i = 0; i < messages.Count; i++)
            {
                ChatMessageContent message = messages[i];
                if (message.Role == AuthorRole.System)
                {
                    systemMessages.Add(message);
                }
                else
                {
                    nonSystemMessages.Add(message);
                }
            }

            List<ChatMessageContent> selected = new List<ChatMessageContent>(messages.Count);
            foreach (ChatMessageContent system in systemMessages)
            {
                int tokens = ChatTokenEstimator.EstimateMessageTokens(system);
                if (usedHistoryTokens + tokens > historyBudgetTokens)
                {
                    continue;
                }

                selected.Add(CloneMessage(system));
                usedHistoryTokens += tokens;
            }

            List<ChatMessageContent> reverseTail = new List<ChatMessageContent>();
            for (int i = nonSystemMessages.Count - 1; i >= 0; i--)
            {
                ChatMessageContent message = nonSystemMessages[i];
                int tokens = ChatTokenEstimator.EstimateMessageTokens(message);

                if (reverseTail.Count == 0 || usedHistoryTokens + tokens <= historyBudgetTokens)
                {
                    reverseTail.Add(CloneMessage(message));
                    usedHistoryTokens += tokens;
                }
            }

            reverseTail.Reverse();
            selected.AddRange(reverseTail);
            return selected;
        }

        private static bool ContainsOversizedToolMessages(IReadOnlyList<ChatMessageContent> messages, int oversizedToolThreshold)
        {
            if (messages == null || messages.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < messages.Count; i++)
            {
                ChatMessageContent message = messages[i];
                if (message.Role != AuthorRole.Tool)
                {
                    continue;
                }

                int tokens = ChatTokenEstimator.EstimateMessageTokens(message);
                if (tokens >= oversizedToolThreshold)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasLowValueMessages(IReadOnlyList<ChatMessageContent> messages)
        {
            if (messages == null || messages.Count == 0)
            {
                return false;
            }

            int budgetSkipCount = 0;
            for (int i = 0; i < messages.Count; i++)
            {
                ChatMessageContent message = messages[i];
                if (string.IsNullOrWhiteSpace(message?.Content) && (message?.Items == null || message.Items.Count == 0))
                {
                    return true;
                }

                if (IsToolBudgetSkipMessage(message))
                {
                    budgetSkipCount++;
                    if (budgetSkipCount > 1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static int ComputeOversizedToolThreshold(int historyBudgetTokens)
        {
            int relativeThreshold = (int)Math.Ceiling(historyBudgetTokens * 0.25);
            return Math.Max(600, Math.Min(5000, relativeThreshold));
        }

        private static int[] BuildTurnIndices(IReadOnlyList<ChatMessageContent> messages)
        {
            int[] turns = new int[messages.Count];
            int currentTurn = -1;

            for (int i = 0; i < messages.Count; i++)
            {
                if (messages[i].Role == AuthorRole.User)
                {
                    currentTurn++;
                }

                turns[i] = currentTurn;
            }

            return turns;
        }

        private static HashSet<int> DetectOpenToolChainIndices(IReadOnlyList<ChatMessageContent> messages)
        {
            HashSet<int> openIndices = new HashSet<int>();
            if (messages == null || messages.Count == 0)
            {
                return openIndices;
            }

            for (int i = messages.Count - 1; i >= 0; i--)
            {
                ChatMessageContent message = messages[i];
                bool isToolOrCall = message.Role == AuthorRole.Tool || HasFunctionCallItems(message);

                if (openIndices.Count == 0)
                {
                    if (isToolOrCall)
                    {
                        openIndices.Add(i);
                        continue;
                    }

                    break;
                }

                if (isToolOrCall)
                {
                    openIndices.Add(i);
                    continue;
                }

                // If an assistant text message already closed the chain, do not lock anything.
                if (message.Role == AuthorRole.Assistant)
                {
                    openIndices.Clear();
                }

                break;
            }

            return openIndices;
        }

        private static bool HasFunctionCallItems(ChatMessageContent message)
        {
            if (message?.Items == null || message.Items.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < message.Items.Count; i++)
            {
                object item = message.Items[i];
                string typeName = item?.GetType().Name ?? string.Empty;
                if (typeName.IndexOf("FunctionCall", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static List<ChatMessageContent> EnsureValidToolMessageChains(
            IReadOnlyList<ChatMessageContent> messages,
            out int normalizedToolMessagesCount)
        {
            normalizedToolMessagesCount = 0;
            if (messages == null || messages.Count == 0)
            {
                return new List<ChatMessageContent>();
            }

            List<ChatMessageContent> normalized = new List<ChatMessageContent>(messages.Count);
            HashSet<string> pendingCallIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int pendingAnonymousCalls = 0;

            for (int i = 0; i < messages.Count; i++)
            {
                ChatMessageContent message = messages[i];
                if (message == null)
                {
                    continue;
                }

                if (message.Role == AuthorRole.Assistant && HasFunctionCallItems(message))
                {
                    // Budget trimming may retain a call but remove its result. Only send
                    // calls whose results survive in the immediately following tool group.
                    var availableResults = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    int anonymousResults = 0;
                    for (int next = i + 1; next < messages.Count && messages[next]?.Role == AuthorRole.Tool; next++)
                    {
                        List<string> ids = ExtractFunctionResultCallIds(messages[next].Items);
                        foreach (string id in ids) availableResults.Add(id);
                        if (ids.Count == 0 && HasFunctionResultItem(messages[next].Items)) anonymousResults++;
                    }
                    ChatMessageContent completeCalls = CloneMessage(message);
                    foreach (FunctionCallContent call in completeCalls.Items.OfType<FunctionCallContent>().ToList())
                    {
                        bool hasResult = string.IsNullOrEmpty(call.Id) ? anonymousResults-- > 0 : availableResults.Contains(call.Id);
                        if (!hasResult)
                        {
                            completeCalls.Items.Remove(call);
                            normalizedToolMessagesCount++;
                        }
                    }
                    if (completeCalls.Items.Count == 0 && string.IsNullOrWhiteSpace(completeCalls.Content)) continue;
                    normalized.Add(completeCalls);

                    pendingCallIds.Clear();
                    CapturePendingFunctionCallIds(completeCalls.Items, pendingCallIds, out pendingAnonymousCalls);
                    continue;
                }

                if (message.Role == AuthorRole.Tool)
                {
                    if (IsToolMessageCompatibleWithPendingCalls(message, pendingCallIds, ref pendingAnonymousCalls))
                    {
                        normalized.Add(CloneMessage(message));
                    }
                    else
                    {
                        normalized.Add(ConvertToolMessageToAssistantMemory(message));
                        normalizedToolMessagesCount++;
                    }

                    continue;
                }

                pendingCallIds.Clear();
                pendingAnonymousCalls = 0;
                normalized.Add(CloneMessage(message));
            }

            return normalized;
        }

        private static bool IsToolMessageCompatibleWithPendingCalls(
            ChatMessageContent message,
            HashSet<string> pendingCallIds,
            ref int pendingAnonymousCalls)
        {
            if (message == null || !HasFunctionResultItem(message.Items))
            {
                return false;
            }

            int pendingNamedCount = pendingCallIds?.Count ?? 0;
            if (pendingNamedCount == 0 && pendingAnonymousCalls <= 0)
            {
                return false;
            }

            List<string> resultCallIds = ExtractFunctionResultCallIds(message.Items);
            if (resultCallIds.Count > 0)
            {
                bool matched = false;
                for (int i = 0; i < resultCallIds.Count; i++)
                {
                    string callId = resultCallIds[i];
                    if (string.IsNullOrWhiteSpace(callId))
                    {
                        continue;
                    }

                    if (pendingCallIds != null && pendingCallIds.Remove(callId))
                    {
                        matched = true;
                    }
                }

                return matched;
            }

            if (pendingAnonymousCalls > 0)
            {
                pendingAnonymousCalls--;
                return true;
            }

            if (pendingCallIds != null && pendingCallIds.Count > 0)
            {
                string firstPendingCallId = pendingCallIds.First();
                pendingCallIds.Remove(firstPendingCallId);
                return true;
            }

            return false;
        }

        private static void CapturePendingFunctionCallIds(
            ChatMessageContentItemCollection items,
            HashSet<string> pendingCallIds,
            out int pendingAnonymousCalls)
        {
            pendingAnonymousCalls = 0;
            if (items == null || items.Count == 0)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] is FunctionCallContent functionCall)
                {
                    string callId = functionCall.Id?.Trim();
                    if (!string.IsNullOrWhiteSpace(callId))
                    {
                        pendingCallIds?.Add(callId);
                    }
                    else
                    {
                        pendingAnonymousCalls++;
                    }
                }
            }
        }

        private static List<string> ExtractFunctionResultCallIds(ChatMessageContentItemCollection items)
        {
            List<string> ids = new List<string>();
            if (items == null || items.Count == 0)
            {
                return ids;
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] is FunctionResultContent functionResult)
                {
                    string callId = functionResult.CallId?.Trim();
                    if (!string.IsNullOrWhiteSpace(callId))
                    {
                        ids.Add(callId);
                    }
                }
            }

            return ids;
        }

        private static ChatMessageContent ConvertToolMessageToAssistantMemory(ChatMessageContent message)
        {
            string functionName = TryExtractFunctionName(message);
            string compactText = CompactWhitespace(message?.Content, 400);
            string prefix = $"{AssistantResponseText.ToolMemoryPrefix} {functionName}";

            return new ChatMessageContent(
                AuthorRole.Assistant,
                string.IsNullOrWhiteSpace(compactText)
                    ? prefix
                    : $"{prefix}: {compactText}");
        }

        private static string TryExtractFunctionName(ChatMessageContent message)
        {
            if (message?.Items == null || message.Items.Count == 0)
            {
                return "tool";
            }

            for (int i = 0; i < message.Items.Count; i++)
            {
                object item = message.Items[i];
                if (item == null)
                {
                    continue;
                }

                var property = item.GetType().GetProperty("FunctionName");
                if (property?.PropertyType == typeof(string))
                {
                    string functionName = property.GetValue(item, null) as string;
                    if (!string.IsNullOrWhiteSpace(functionName))
                    {
                        return functionName.Trim();
                    }
                }
            }

            return "tool";
        }

        private static string CompactWhitespace(string text, int maxChars)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            string compact = text
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\t", " ");
            while (compact.Contains("  "))
            {
                compact = compact.Replace("  ", " ");
            }

            compact = compact.Trim();
            if (compact.Length <= maxChars)
            {
                return compact;
            }

            return compact.Substring(0, maxChars).TrimEnd() + "...";
        }

        private static string TrimContentToTokenBudget(string content, int tokenBudget)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return string.Empty;
            }

            if (tokenBudget <= 0)
            {
                return content;
            }

            int estimated = ChatTokenEstimator.EstimateTokens(content);
            if (estimated <= tokenBudget)
            {
                return content;
            }

            int maxChars = Math.Max(64, tokenBudget * 4);
            if (content.Length <= maxChars)
            {
                return content;
            }

            return content.Substring(0, maxChars).TrimEnd() + "...";
        }

        private static bool IsToolBudgetSkipMessage(ChatMessageContent message)
        {
            if (message?.Role != AuthorRole.Tool)
            {
                return false;
            }

            string content = message.Content ?? string.Empty;
            return content.IndexOf(BudgetSkipToolMessagePrefix, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static int FindLastIndex(IReadOnlyList<ChatMessageContent> messages, AuthorRole role)
        {
            if (messages == null || messages.Count == 0)
            {
                return -1;
            }

            for (int i = messages.Count - 1; i >= 0; i--)
            {
                if (messages[i].Role == role)
                {
                    return i;
                }
            }

            return -1;
        }

        private static int FindLastAssistantTextIndex(IReadOnlyList<ChatMessageContent> messages)
        {
            if (messages == null || messages.Count == 0)
            {
                return -1;
            }

            for (int i = messages.Count - 1; i >= 0; i--)
            {
                ChatMessageContent message = messages[i];
                if (message.Role != AuthorRole.Assistant)
                {
                    continue;
                }

                if (!HasFunctionCallItems(message))
                {
                    return i;
                }
            }

            return -1;
        }

        private static List<ChatMessageContent> CloneMessages(IReadOnlyList<ChatMessageContent> messages)
        {
            if (messages == null || messages.Count == 0)
            {
                return new List<ChatMessageContent>();
            }

            List<ChatMessageContent> clones = new List<ChatMessageContent>(messages.Count);
            for (int i = 0; i < messages.Count; i++)
            {
                clones.Add(CloneMessage(messages[i]));
            }

            return clones;
        }

        private static ChatMessageContent CloneMessage(ChatMessageContent message)
        {
            if (message == null)
            {
                return new ChatMessageContent(AuthorRole.Assistant, string.Empty);
            }

            if (message.Role == AuthorRole.Tool && !HasFunctionResultItem(message.Items))
            {
                // Legacy or synthetic tool messages without result metadata are converted
                // to assistant memory so OpenAI-compatible endpoints accept the history.
                string legacyToolText = CompactWhitespace(message.Content, 400);
                return new ChatMessageContent(
                    AuthorRole.Assistant,
                    string.IsNullOrWhiteSpace(legacyToolText)
                        ? AssistantResponseText.ToolMemoryPrefix
                        : $"{AssistantResponseText.ToolMemoryPrefix} {legacyToolText}");
            }

            ChatMessageContentItemCollection items = CloneItems(message.Items);
            if (items != null && items.Count > 0)
            {
                return new ChatMessageContent(
                    message.Role,
                    items,
                    message.AuthorName,
                    message.InnerContent,
                    message.Encoding,
                    message.Metadata);
            }

            return new ChatMessageContent(
                message.Role,
                message.Content ?? string.Empty,
                message.AuthorName,
                message.InnerContent,
                message.Encoding,
                message.Metadata);
        }

        private static ChatMessageContentItemCollection CloneItems(ChatMessageContentItemCollection items)
        {
            if (items == null || items.Count == 0)
            {
                return null;
            }

            ChatMessageContentItemCollection cloned = new ChatMessageContentItemCollection();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] is KernelContent kernelContent)
                {
                    cloned.Add(CloneKernelContent(kernelContent));
                }
            }

            return cloned;
        }

        private static KernelContent CloneKernelContent(KernelContent item)
        {
            if (item is FunctionResultContent functionResult)
            {
                return new FunctionResultContent(
                    functionResult.CallId,
                    functionResult.PluginName,
                    functionResult.FunctionName,
                    functionResult.Result);
            }

            if (item is FunctionCallContent functionCall)
            {
                return new FunctionCallContent(
                    functionCall.Id,
                    functionCall.PluginName,
                    functionCall.FunctionName,
                    functionCall.Arguments);
            }

            return item;
        }

        private static bool HasFunctionResultItem(ChatMessageContentItemCollection items)
        {
            if (items == null || items.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] is FunctionResultContent)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool AreEquivalentMessages(IReadOnlyList<ChatMessageContent> left, IReadOnlyList<ChatMessageContent> right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null || left.Count != right.Count)
            {
                return false;
            }

            for (int i = 0; i < left.Count; i++)
            {
                ChatMessageContent a = left[i];
                ChatMessageContent b = right[i];

                if (a?.Role != b?.Role)
                {
                    return false;
                }

                string aContent = a?.Content ?? string.Empty;
                string bContent = b?.Content ?? string.Empty;
                if (!string.Equals(aContent, bContent, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        private enum MessageRetentionClass
        {
            KeepRaw,
            Summarize,
            Remove
        }

        private sealed class HistoryMessageDescriptor
        {
            public int Index { get; set; }
            public ChatMessageContent Message { get; set; }
            public int Tokens { get; set; }
            public bool IsSystem { get; set; }
            public bool IsTool { get; set; }
            public bool IsLocked { get; set; }
            public MessageRetentionClass RetentionClass { get; set; }
        }

        private sealed class HistoryPartition
        {
            public List<HistoryMessageDescriptor> SystemMessages { get; } = new List<HistoryMessageDescriptor>();
            public List<HistoryMessageDescriptor> KeepRawMessages { get; } = new List<HistoryMessageDescriptor>();
            public List<ChatMessageContent> SummarizableMessages { get; } = new List<ChatMessageContent>();
        }

        private sealed class ComposedMessage
        {
            public int Order { get; set; }
            public ChatMessageContent Message { get; set; }
            public int Tokens { get; set; }
            public bool IsMandatory { get; set; }
            public bool IsSystem { get; set; }
        }
    }
}
