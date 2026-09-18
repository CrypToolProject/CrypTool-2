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
using CrypTool.CrypLLM.Helper;
using CrypTool.CrypLLM.Properties;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

// Module overview:
// Central policy filter for AI-triggered tool calls.
// Responsibilities: permission gating, runtime budget enforcement,
// and token-aware compaction/summarization of tool results.
namespace CrypTool.CrypLLM.Threads
{
    /// <summary>
    /// Enforces tool permissions and logs tool-execution diagnostics.
    /// </summary>
    internal sealed class ToolPermissionFilter : IAutoFunctionInvocationFilter
    {
        private const string ToolCompactionInvariantErrorCode = "CTX-TOOL-001";
        private const string ToolRuntimeBudgetErrorCode = "CTX-TOOL-002";
        private const string PermissionControlledPluginPrefix = "CrypLLM_";
        private const double ToolResultChunkCharsPerTargetTokenFactor = 8.0;
        private const double PerChunkTargetShare = 0.75;
        private const string ToolResultSummaryPrefix = "[tool-result-summary]";
        private static readonly Type[] KnownPluginTypes =
        {
            typeof(WorkspaceStatusPlugin),
            typeof(ComponentCatalogPlugin),
            typeof(TemplateCatalogPlugin),
            typeof(UiTextCatalogPlugin),
            typeof(WorkspaceEditingPlugin)
        };

        private static readonly Lazy<HashSet<string>> MutationFunctionNames =
            new Lazy<HashSet<string>>(BuildMutationFunctionNames);

        /// <summary>
        /// Executes permission checks, runtime budget accounting, and optional
        /// result compaction for each auto-invoked tool function.
        /// </summary>
        /// <remarks>
        /// Input: invocation metadata, arguments, and optional budget state from <see cref="ToolInvocationBudgetRuntime"/>.
        /// Output: either an allowed invocation result, a denied result, or a compacted/summarized replacement result.
        /// Limitation: token estimates are heuristic and intentionally conservative.
        /// </remarks>
        public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
        {
            string pluginName = context?.Function?.Metadata?.PluginName ?? string.Empty;
            string functionName = context?.Function?.Metadata?.Name ?? string.Empty;
            bool permissionControlledInvocation = IsPermissionControlledPlugin(pluginName);
            string callId = ExtractFunctionCallId(context);
            string argumentsForDisplay = ExtractArgumentsForLog(context);

            if (permissionControlledInvocation)
            {
                AIThreadManager.Instance.ReportToolActivityStarted(callId, pluginName, functionName, argumentsForDisplay);
                Log.Info(BuildToolCallLogLine(context, functionName));
            }

            if (context?.Function?.Metadata != null)
            {
                // Apply permission checks only to our controlled plugins.
                if (permissionControlledInvocation)
                {
                    if (!Settings.Default.allowAiToolMutations && IsMutationFunction(functionName))
                    {
                        Log.Warning($"Tool call denied (mutating tools disabled): plugin={pluginName}, function={functionName}, arguments={FormatArgumentsForStructuredLog(argumentsForDisplay)}");
                        AIThreadManager.Instance.ReportToolActivityFinished(
                            callId,
                            pluginName,
                            functionName,
                            ToolActivityStatus.Denied,
                            GetResourceTextOrFallback("AiChatToolDeniedMutationsDisabled", "Denied by user setting: Modifying tools are disabled."));
                        context.Result = new FunctionResult(context.Function, $"Denied by user settings (mutating tools disabled): {functionName}");
                        return;
                    }

                    if (!IsEditorStatusFunctionAllowed(functionName))
                    {
                        Log.Warning($"Tool call denied (function disabled by settings): plugin={pluginName}, function={functionName}, arguments={FormatArgumentsForStructuredLog(argumentsForDisplay)}");
                        AIThreadManager.Instance.ReportToolActivityFinished(
                            callId,
                            pluginName,
                            functionName,
                            ToolActivityStatus.Denied,
                            GetResourceTextOrFallback("AiChatToolDeniedFunctionDisabled", "Denied by user setting: This tool is disabled."));
                        context.Result = new FunctionResult(context.Function, $"Denied by user settings: {functionName}");
                        return;
                    }
                }
            }

            ToolInvocationBudgetRuntime.TryGet(out ToolInvocationBudgetRuntime.ToolInvocationBudgetState budgetBeforeCall);
            if (permissionControlledInvocation && budgetBeforeCall == null)
            {
                Log.Warning($"Tool budget runtime state missing for tool call. Falling back to unbudgeted execution: plugin={pluginName}, function={functionName}");
            }

            if (permissionControlledInvocation && budgetBeforeCall != null)
            {
                // Section: reserve per-call runtime budget before tool execution.
                int remainingBeforeReservation = budgetBeforeCall.RemainingToolTokens;
                if (!budgetBeforeCall.TryRegisterToolCall())
                {
                    Log.Warning(
                        $"Tool call denied (tool budget exhausted): plugin={pluginName}, function={functionName}, " +
                        $"remainingToolTokens={budgetBeforeCall.RemainingToolTokens}, requiredForCall={budgetBeforeCall.RequiredTokensForToolCall}, " +
                        $"arguments={FormatArgumentsForStructuredLog(argumentsForDisplay)}");
                    AIThreadManager.Instance.ReportToolActivityFinished(
                        callId,
                        pluginName,
                        functionName,
                        ToolActivityStatus.Denied,
                        GetResourceTextOrFallback("AiChatToolDeniedBudgetExhausted", "Denied by runtime budget: The tool budget is exhausted."));
                    context.Result = new FunctionResult(context.Function, $"Denied by runtime budget (tool budget exhausted): {functionName}");
                    return;
                }

                Log.Info(
                    $"Tool budget changed: reason=tool-call-reservation, plugin={pluginName}, function={functionName}, " +
                    $"reservationTokens={budgetBeforeCall.ToolCallReservationTokens}, requiredForCall={budgetBeforeCall.RequiredTokensForToolCall}, " +
                    $"remainingBefore={remainingBeforeReservation}, remainingAfter={budgetBeforeCall.RemainingToolTokens}");
            }

            Stopwatch stopwatch = permissionControlledInvocation ? Stopwatch.StartNew() : null;
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                if (permissionControlledInvocation)
                {
                    stopwatch?.Stop();
                    bool likelyContextError = IsLikelyContextWindowExceeded(ex);
                    Log.Error(
                        $"Tool invocation failed: plugin={pluginName}, function={functionName}, durationMs={stopwatch?.ElapsedMilliseconds ?? 0}, " +
                        $"contextError={likelyContextError}, retryPolicy=disabled, arguments={FormatArgumentsForStructuredLog(argumentsForDisplay)}, error={ex.Message}");
                    AIThreadManager.Instance.ReportToolActivityFinished(
                        callId,
                        pluginName,
                        functionName,
                        ToolActivityStatus.Failed,
                        FormatFailureForDisplay(ex));
                }
                throw;
            }

            ToolInvocationBudgetRuntime.TryGet(out ToolInvocationBudgetRuntime.ToolInvocationBudgetState latestBudget);

            object resultObject = null;
            try
            {
                resultObject = context?.Result?.GetValue<object>();
            }
            catch
            {
                resultObject = null;
            }

            int originalResultTokens = EstimateResultTokens(resultObject);
            int estimatedResultTokens = originalResultTokens;
            // Section: compact result when it exceeds remaining tool budget.
            ToolResultCompactionOutcome compactionOutcome = await TryCompactResultForBudgetAsync(
                context,
                latestBudget,
                resultObject).ConfigureAwait(false);
            if (compactionOutcome.IsCompacted)
            {
                context.Result = new FunctionResult(context.Function, compactionOutcome.CompactedResult);
                resultObject = compactionOutcome.CompactedResult;
                estimatedResultTokens = compactionOutcome.CompactedTokens;
                latestBudget?.RegisterTextReduction(
                    source: "ToolResultBudget",
                    method: compactionOutcome.CompactionMethod,
                    pluginName: pluginName,
                    functionName: functionName,
                    originalTokens: compactionOutcome.OriginalTokens,
                    reducedTokens: compactionOutcome.CompactedTokens,
                    originalText: compactionOutcome.OriginalText,
                    reducedText: compactionOutcome.CompactedResult);

                if (compactionOutcome.UsedSemanticSummary)
                {
                    Log.Info($"Tool result summarized for tool budget: plugin={pluginName}, function={functionName}, originalTokens={compactionOutcome.OriginalTokens}, summarizedTokens={compactionOutcome.CompactedTokens}, remainingToolTokens={latestBudget?.RemainingToolTokens.ToString(CultureInfo.InvariantCulture) ?? "n/a"}");
                }
                else
                {
                    Log.Info($"Tool result compacted for tool budget: plugin={pluginName}, function={functionName}, originalTokens={compactionOutcome.OriginalTokens}, compactedTokens={compactionOutcome.CompactedTokens}, remainingToolTokens={latestBudget?.RemainingToolTokens.ToString(CultureInfo.InvariantCulture) ?? "n/a"}");
                }
            }
            else
            {
                Log.Info(
                    $"Tool result kept raw for tool budget: plugin={pluginName}, function={functionName}, resultTokens={originalResultTokens}, " +
                    $"remainingToolTokens={latestBudget?.RemainingToolTokens.ToString(CultureInfo.InvariantCulture) ?? "n/a"}");
            }

            if (latestBudget != null)
            {
                // Section: invariant checks before final budget consumption.
                if (compactionOutcome.IsCompacted && estimatedResultTokens > latestBudget.RemainingToolTokens)
                {
                    Log.Error(
                        $"[{ToolCompactionInvariantErrorCode}] Compacted tool result still exceeds remaining tool budget: plugin={pluginName}, function={functionName}, " +
                        $"compactedTokens={estimatedResultTokens}, remainingToolTokens={latestBudget.RemainingToolTokens}, method={compactionOutcome.CompactionMethod}.");
                }
                else if (!compactionOutcome.IsCompacted && originalResultTokens > latestBudget.RemainingToolTokens)
                {
                    Log.Error(
                        $"[{ToolCompactionInvariantErrorCode}] Raw tool result exceeds remaining tool budget but compaction was not applied: plugin={pluginName}, function={functionName}, " +
                        $"rawTokens={originalResultTokens}, remainingToolTokens={latestBudget.RemainingToolTokens}.");
                }

                if (estimatedResultTokens > latestBudget.RemainingToolTokens)
                {
                    Log.Error(
                        $"[{ToolRuntimeBudgetErrorCode}] Tool result tokens exceed remaining runtime tool budget before consume: plugin={pluginName}, function={functionName}, " +
                        $"resultTokens={estimatedResultTokens}, remainingToolTokens={latestBudget.RemainingToolTokens}, originalResultTokens={originalResultTokens}, compactionMethod={compactionOutcome.CompactionMethod}.");
                }

                int remainingBeforeResultConsume = latestBudget.RemainingToolTokens;
                latestBudget.ConsumeByTokens(estimatedResultTokens);
                Log.Info(
                    $"Tool budget changed: reason=tool-result-consume, plugin={pluginName}, function={functionName}, " +
                    $"consumedTokens={estimatedResultTokens}, remainingBefore={remainingBeforeResultConsume}, remainingAfter={latestBudget.RemainingToolTokens}");
            }

            if (permissionControlledInvocation)
            {
                ToolCompletionOutcome completionOutcome = DetermineToolCompletionOutcome(resultObject);
                if (completionOutcome.Status == ToolActivityStatus.Failed)
                {
                    Log.Error(
                        $"Tool invocation returned logical failure: plugin={pluginName}, function={functionName}, " +
                        $"arguments={FormatArgumentsForStructuredLog(argumentsForDisplay)}, error={completionOutcome.DisplayText}");
                }

                AIThreadManager.Instance.ReportToolActivityFinished(
                    callId,
                    pluginName,
                    functionName,
                    completionOutcome.Status,
                    completionOutcome.DisplayText);
            }
        }

        /// <summary>
        /// Creates a human-readable tool-call log entry with normalized argument rendering.
        /// </summary>
        private static string BuildToolCallLogLine(AutoFunctionInvocationContext context, string functionName)
        {
            string safeFunctionName = string.IsNullOrWhiteSpace(functionName)
                ? "tool"
                : functionName.Trim();

            string arguments = ExtractArgumentsForLog(context);
            return string.IsNullOrWhiteSpace(arguments)
                ? $"[ToolCall] {safeFunctionName}()"
                : $"[ToolCall] {safeFunctionName}({arguments})";
        }

        private static string FormatArgumentsForStructuredLog(string arguments)
        {
            if (string.IsNullOrWhiteSpace(arguments))
            {
                return "<none>";
            }

            string normalized = arguments.Trim();
            normalized = normalized.Replace("\r", "\\r").Replace("\n", "\\n");
            return normalized;
        }

        /// <summary>
        /// Extracts call arguments from multiple Semantic Kernel context shapes for robust logging.
        /// </summary>
        private static string ExtractArgumentsForLog(AutoFunctionInvocationContext context)
        {
            if (context == null)
            {
                return string.Empty;
            }

            FunctionCallContent functionCall = FindMatchingFunctionCallContent(context);
            string fromFunctionCall = FormatArgumentsForLog(functionCall?.Arguments);
            if (!string.IsNullOrWhiteSpace(fromFunctionCall))
            {
                return fromFunctionCall;
            }

            foreach (object argumentsObject in TryGetPropertyValues(
                context,
                "Arguments",
                "KernelArguments",
                "FunctionArguments"))
            {
                string formattedArguments = FormatArgumentsForLog(argumentsObject);
                if (!string.IsNullOrWhiteSpace(formattedArguments))
                {
                    return formattedArguments;
                }
            }

            return string.Empty;
        }

        private static string ExtractFunctionCallId(AutoFunctionInvocationContext context)
        {
            string toolCallId = TryGetTrimmedStringPropertyValue(context, "ToolCallId");
            if (!string.IsNullOrWhiteSpace(toolCallId))
            {
                return toolCallId;
            }

            FunctionCallContent functionCall = FindMatchingFunctionCallContent(context);
            string functionCallId = functionCall?.Id?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(functionCallId))
            {
                return functionCallId;
            }

            object legacyFunctionCall = TryGetPropertyValue(context, "FunctionCallContent");
            object id = TryGetPropertyValue(legacyFunctionCall, "Id") ?? TryGetPropertyValue(legacyFunctionCall, "CallId");
            return id?.ToString()?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Formats arbitrary argument payloads into a compact log-friendly representation.
        /// </summary>
        private static string FormatArgumentsForLog(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is string s)
            {
                return FormatArgumentsText(s);
            }

            if (value is KernelArguments kernelArguments && TryFormatKernelArguments(kernelArguments, out string kernelArgumentsText))
            {
                return kernelArgumentsText;
            }

            if (TryFormatArgumentsEnumerable(value, out string enumerableText))
            {
                return enumerableText;
            }

            string serialized = SerializeResult(value);
            return FormatArgumentsText(serialized);
        }

        /// <summary>
        /// Attempts to format enumerable argument collections as key-value pairs.
        /// </summary>
        private static bool TryFormatArgumentsEnumerable(object value, out string formatted)
        {
            formatted = string.Empty;
            if (value is string || value is not System.Collections.IEnumerable enumerable)
            {
                return false;
            }

            List<string> parts = new List<string>();
            foreach (object entry in enumerable)
            {
                if (entry == null)
                {
                    continue;
                }

                if (entry is System.Collections.DictionaryEntry de)
                {
                    string key = de.Key?.ToString();
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        continue;
                    }

                    string valueText = FormatSingleArgumentValue(de.Value);
                    parts.Add($"{key}={valueText}");
                    continue;
                }

                object keyObj = TryGetPropertyValue(entry, "Key") ?? TryGetPropertyValue(entry, "Name");
                if (keyObj == null)
                {
                    continue;
                }

                string keyText = keyObj.ToString();
                if (string.IsNullOrWhiteSpace(keyText))
                {
                    continue;
                }

                object entryValue = TryGetPropertyValue(entry, "Value");
                string entryValueText = FormatSingleArgumentValue(entryValue);
                parts.Add($"{keyText}={entryValueText}");
            }

            if (parts.Count == 0)
            {
                return false;
            }

            formatted = string.Join(", ", parts);
            return true;
        }

        private static bool TryFormatKernelArguments(KernelArguments kernelArguments, out string formatted)
        {
            formatted = string.Empty;
            if (kernelArguments == null)
            {
                return false;
            }

            List<string> parts = new List<string>();
            foreach (var entry in kernelArguments)
            {
                string key = entry.Key?.Trim();
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                string valueText = FormatSingleArgumentValue(entry.Value);
                parts.Add($"{key}={valueText}");
            }

            if (parts.Count == 0)
            {
                return false;
            }

            formatted = string.Join(", ", parts);
            return true;
        }

        /// <summary>
        /// Safely formats one argument value, preserving readability for strings and objects.
        /// </summary>
        private static string FormatSingleArgumentValue(object value)
        {
            if (value == null)
            {
                return "null";
            }

            if (value is string s)
            {
                string escaped = s.Replace("\\", "\\\\").Replace("\"", "\\\"");
                return $"\"{escaped}\"";
            }

            string serialized = SerializeResult(value);
            return CompactWhitespace(serialized, 120);
        }

        /// <summary>
        /// Converts raw JSON-like text into compact <c>key=value</c> log output when possible.
        /// </summary>
        private static string FormatArgumentsText(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            string trimmed = raw.Trim();
            if (string.Equals(trimmed, "{}", StringComparison.Ordinal))
            {
                return string.Empty;
            }

            try
            {
                var token = JsonConvert.DeserializeObject(trimmed);
                if (token is Newtonsoft.Json.Linq.JObject obj)
                {
                    List<string> parts = new List<string>();
                    foreach (var property in obj.Properties())
                    {
                        string value;
                        if (property.Value == null || property.Value.Type == Newtonsoft.Json.Linq.JTokenType.Null)
                        {
                            value = "null";
                        }
                        else if (property.Value.Type == Newtonsoft.Json.Linq.JTokenType.String)
                        {
                            value = FormatSingleArgumentValue(property.Value.ToString());
                        }
                        else
                        {
                            value = CompactWhitespace(property.Value.ToString(Formatting.None), 120);
                        }

                        parts.Add($"{property.Name}={value}");
                    }

                    return string.Join(", ", parts);
                }

                return CompactWhitespace(trimmed, 140);
            }
            catch
            {
                return CompactWhitespace(trimmed, 140);
            }
        }

        /// <summary>
        /// Reads a public instance property via reflection and suppresses reflection failures.
        /// </summary>
        private static object TryGetPropertyValue(object instance, string propertyName)
        {
            if (instance == null || string.IsNullOrWhiteSpace(propertyName))
            {
                return null;
            }

            try
            {
                PropertyInfo property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
                return property?.GetValue(instance, null);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Reads candidate properties with the same name across the full type hierarchy.
        /// This is required for hidden members such as AutoFunctionInvocationContext.Arguments
        /// vs. FunctionInvocationContext.Arguments.
        /// </summary>
        private static IEnumerable<object> TryGetPropertyValues(object instance, params string[] propertyNames)
        {
            if (instance == null || propertyNames == null || propertyNames.Length == 0)
            {
                yield break;
            }

            for (Type currentType = instance.GetType(); currentType != null; currentType = currentType.BaseType)
            {
                for (int i = 0; i < propertyNames.Length; i++)
                {
                    string propertyName = propertyNames[i];
                    if (string.IsNullOrWhiteSpace(propertyName))
                    {
                        continue;
                    }

                    PropertyInfo property;
                    try
                    {
                        property = currentType.GetProperty(
                            propertyName,
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    }
                    catch
                    {
                        property = null;
                    }

                    if (property == null || property.GetIndexParameters().Length != 0)
                    {
                        continue;
                    }

                    object value;
                    try
                    {
                        value = property.GetValue(instance, null);
                    }
                    catch
                    {
                        continue;
                    }

                    if (value != null)
                    {
                        yield return value;
                    }
                }
            }
        }

        private static string TryGetTrimmedStringPropertyValue(object instance, string propertyName)
        {
            object value = TryGetPropertyValue(instance, propertyName);
            return value?.ToString()?.Trim() ?? string.Empty;
        }

        private static int? TryGetIntPropertyValue(object instance, string propertyName)
        {
            object value = TryGetPropertyValue(instance, propertyName);
            if (value == null)
            {
                return null;
            }

            if (value is int intValue)
            {
                return intValue;
            }

            if (value is long longValue && longValue >= int.MinValue && longValue <= int.MaxValue)
            {
                return (int)longValue;
            }

            if (int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
            {
                return parsed;
            }

            return null;
        }

        private static FunctionCallContent FindMatchingFunctionCallContent(AutoFunctionInvocationContext context)
        {
            if (context == null)
            {
                return null;
            }

            if (TryGetPropertyValue(context, "ChatMessageContent") is not ChatMessageContent message ||
                message.Items == null ||
                message.Items.Count == 0)
            {
                return null;
            }

            List<FunctionCallContent> functionCalls = message.Items
                .OfType<FunctionCallContent>()
                .ToList();
            if (functionCalls.Count == 0)
            {
                return null;
            }

            string toolCallId = TryGetTrimmedStringPropertyValue(context, "ToolCallId");
            if (!string.IsNullOrWhiteSpace(toolCallId))
            {
                FunctionCallContent byId = functionCalls.FirstOrDefault(call =>
                    string.Equals(call?.Id?.Trim() ?? string.Empty, toolCallId, StringComparison.OrdinalIgnoreCase));
                if (byId != null)
                {
                    return byId;
                }
            }

            int? functionSequenceIndex = TryGetIntPropertyValue(context, "FunctionSequenceIndex");
            if (functionSequenceIndex.HasValue &&
                functionSequenceIndex.Value >= 0 &&
                functionSequenceIndex.Value < functionCalls.Count)
            {
                return functionCalls[functionSequenceIndex.Value];
            }

            string functionName = context.Function?.Metadata?.Name ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(functionName))
            {
                FunctionCallContent byName = functionCalls.FirstOrDefault(call =>
                    string.Equals(call?.FunctionName ?? string.Empty, functionName, StringComparison.OrdinalIgnoreCase));
                if (byName != null)
                {
                    return byName;
                }
            }

            return functionCalls[0];
        }

        /// <summary>
        /// Tries to reduce tool-result size to the remaining runtime budget.
        /// </summary>
        /// <remarks>
        /// Strategy order: semantic summarization (if available) -> deterministic fallback preview.
        /// </remarks>
        private static async Task<ToolResultCompactionOutcome> TryCompactResultForBudgetAsync(
            AutoFunctionInvocationContext context,
            ToolInvocationBudgetRuntime.ToolInvocationBudgetState budgetState,
            object resultObject)
        {
            int originalTokens = EstimateResultTokens(resultObject);

            if (budgetState == null || resultObject == null)
            {
                return ToolResultCompactionOutcome.NotCompacted(originalTokens);
            }

            int targetTokens = Math.Max(1, budgetState.RemainingToolTokens);
            if (originalTokens <= targetTokens)
            {
                return ToolResultCompactionOutcome.NotCompacted(originalTokens);
            }

            string serialized = SerializeResult(resultObject);
            if (WorkspaceScreenshotContent.TryParse(serialized, out _))
            {
                // Never summarize or truncate PNG Base64 as if it were prose.
                string denied = HardClampToTokenBudget("Workspace screenshot omitted: insufficient image context budget.", targetTokens);
                return ToolResultCompactionOutcome.Compacted(denied, originalTokens, EstimateResultTokens(denied),
                    usedSemanticSummary: false, compactionMethod: "ImageBudgetDenied", originalText: "[workspace screenshot]");
            }
            if (string.IsNullOrWhiteSpace(serialized))
            {
                return ToolResultCompactionOutcome.NotCompacted(originalTokens);
            }

            IChatCompletionService chatCompletionService = TryGetChatCompletionService(context);
            CancellationToken cancellationToken = TryGetCancellationToken(context);
            string summarizedContent = string.Empty;

            if (chatCompletionService != null)
            {
                // Section: model-assisted summarization for oversized tool payloads.
                try
                {
                    summarizedContent = await SummarizeToolResultWithSemanticKernelAsync(
                        chatCompletionService,
                        serialized,
                        targetTokens,
                        budgetState.ContextWindowTokens,
                        cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log.Warning($"Tool result semantic summarization failed. Falling back to compact preview. {ex.Message}");
                }
            }

            if (!string.IsNullOrWhiteSpace(summarizedContent))
            {
                // Section: enforce strict post-summary budget compliance.
                string normalizedSummary = await TrimToTokenBudgetAsync(
                    summarizedContent,
                    targetTokens,
                    chatCompletionService,
                    cancellationToken).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(normalizedSummary))
                {
                    string summarizedResult = HardClampToTokenBudget(
                        $"{ToolResultSummaryPrefix} {normalizedSummary}",
                        targetTokens);
                    int summarizedTokens = EstimateResultTokens(summarizedResult);
                    if (summarizedTokens > targetTokens)
                    {
                        Log.Error(
                            $"[{ToolCompactionInvariantErrorCode}] Semantic summary still exceeds target after hard clamp. Enforcing emergency trim. " +
                            $"targetTokens={targetTokens}, summarizedTokens={summarizedTokens}");
                        summarizedResult = HardClampToTokenBudget(summarizedResult, targetTokens);
                        summarizedTokens = EstimateResultTokens(summarizedResult);
                    }

                    return ToolResultCompactionOutcome.Compacted(
                        summarizedResult,
                        originalTokens,
                        summarizedTokens,
                        usedSemanticSummary: true,
                        compactionMethod: "SemanticSummary+HardClamp",
                        originalText: serialized);
                }
            }

            int maxChars = Math.Max(1, targetTokens * 4);
            string preview = CompactWhitespace(serialized, maxChars);
            string fallback = HardClampToTokenBudget(
                $"{ToolResultSummaryPrefix} Output compacted for tool return budget. Original size ~{originalTokens} tokens. Preview: {preview}",
                targetTokens);
            int fallbackTokens = EstimateResultTokens(fallback);
            if (fallbackTokens > targetTokens)
            {
                Log.Error(
                    $"[{ToolCompactionInvariantErrorCode}] Fallback summary exceeds target after hard clamp. Enforcing emergency trim. " +
                    $"targetTokens={targetTokens}, fallbackTokens={fallbackTokens}");
                fallback = HardClampToTokenBudget(fallback, targetTokens);
                fallbackTokens = EstimateResultTokens(fallback);
            }

            return ToolResultCompactionOutcome.Compacted(
                fallback,
                originalTokens,
                fallbackTokens,
                usedSemanticSummary: false,
                compactionMethod: "CompactPreviewFallback",
                originalText: serialized);
        }

        /// <summary>
        /// Resolves a chat-completion service from invocation context or active thread kernel.
        /// </summary>
        private static IChatCompletionService TryGetChatCompletionService(AutoFunctionInvocationContext context)
        {
            try
            {
                object kernel =
                    TryGetPropertyValue(context, "Kernel") ??
                    TryGetPropertyValue(context?.Function, "Kernel") ??
                    AIThreadManager.Instance?.Agent?.Kernel;
                object services = TryGetPropertyValue(kernel, "Services");
                if (services is not IServiceProvider provider)
                {
                    return null;
                }

                return provider.GetService(typeof(IChatCompletionService)) as IChatCompletionService;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieves the invocation cancellation token when exposed by the runtime context.
        /// </summary>
        private static CancellationToken TryGetCancellationToken(AutoFunctionInvocationContext context)
        {
            object token = TryGetPropertyValue(context, "CancellationToken");
            return token is CancellationToken cancellationToken ? cancellationToken : CancellationToken.None;
        }

        /// <summary>
        /// Summarizes large tool output by chunking first, then optionally merging chunk summaries.
        /// </summary>
        private static async Task<string> SummarizeToolResultWithSemanticKernelAsync(
            IChatCompletionService chatCompletionService,
            string serializedResult,
            int targetTokens,
            int contextWindowTokens,
            CancellationToken cancellationToken)
        {
            if (chatCompletionService == null || string.IsNullOrWhiteSpace(serializedResult))
            {
                return string.Empty;
            }

            int chunkTokenBudget = Math.Max(
                1,
                (int)Math.Ceiling(Math.Max(targetTokens * 2.0, contextWindowTokens * 0.02)));
            int chunkChars = Math.Max(1, (int)Math.Ceiling(chunkTokenBudget * ToolResultChunkCharsPerTargetTokenFactor));
            List<string> chunks = SplitTextIntoChunks(serializedResult, chunkChars);
            if (chunks.Count == 0)
            {
                return string.Empty;
            }

            int perChunkTargetTokens = Math.Max(1, (int)Math.Ceiling(targetTokens * PerChunkTargetShare));
            List<string> chunkSummaries = new List<string>(chunks.Count);

            // Section: summarize each chunk independently to stabilize long-input behavior.
            for (int i = 0; i < chunks.Count; i++)
            {
                string chunkPrompt =
                    $"Tool output chunk {i + 1}/{chunks.Count}. " +
                    $"Create a concise technical summary (max {perChunkTargetTokens} tokens). " +
                    "Keep only key facts, ids, errors, warnings, numbers, file paths, and execution-relevant state.\n\n" +
                    chunks[i];
                string chunkSummary = await SummarizeTextAsync(
                    chatCompletionService,
                    chunkPrompt,
                    perChunkTargetTokens,
                    cancellationToken).ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(chunkSummary))
                {
                    chunkSummary = CompactWhitespace(chunks[i], Math.Max(1, perChunkTargetTokens * 4));
                }

                if (!string.IsNullOrWhiteSpace(chunkSummary))
                {
                    chunkSummaries.Add(chunkSummary.Trim());
                }
            }

            if (chunkSummaries.Count == 0)
            {
                return string.Empty;
            }

            string merged = string.Join(Environment.NewLine, chunkSummaries);
            if (EstimateResultTokens(merged) <= targetTokens)
            {
                return merged;
            }

            // Section: second-pass merge keeps global coherence under the final token target.
            string finalPrompt =
                $"Merge the following chunk summaries into one final technical summary (max {targetTokens} tokens). " +
                "Preserve essential facts and numeric values only.\n\n" +
                merged;
            string finalSummary = await SummarizeTextAsync(
                chatCompletionService,
                finalPrompt,
                targetTokens,
                cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(finalSummary))
            {
                return finalSummary.Trim();
            }

            return merged;
        }

        /// <summary>
        /// Performs one generic model summarization call for technical text compression.
        /// </summary>
        private static async Task<string> SummarizeTextAsync(
            IChatCompletionService chatCompletionService,
            string userPrompt,
            int targetTokens,
            CancellationToken cancellationToken)
        {
            if (chatCompletionService == null || string.IsNullOrWhiteSpace(userPrompt))
            {
                return string.Empty;
            }

            ChatHistory prompt = new ChatHistory
            {
                new ChatMessageContent(
                    AuthorRole.System,
                    "You summarize tool outputs for constrained LLM context windows. Keep factual technical details and remove repetition. Return plain text only."),
                new ChatMessageContent(
                    AuthorRole.User,
                    $"{userPrompt}\n\nHard limit: approximately {Math.Max(1, targetTokens)} tokens.")
            };

            IReadOnlyList<ChatMessageContent> responses = await chatCompletionService.GetChatMessageContentsAsync(
                prompt,
                null,
                null,
                cancellationToken).ConfigureAwait(false);
            if (responses == null || responses.Count == 0)
            {
                return string.Empty;
            }

            for (int i = 0; i < responses.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(responses[i]?.Content))
                {
                    return responses[i].Content.Trim();
                }
            }

            return responses[0]?.Content?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Splits text into bounded chunks, preferring newline boundaries when feasible.
        /// </summary>
        private static List<string> SplitTextIntoChunks(string text, int maxCharsPerChunk)
        {
            List<string> chunks = new List<string>();
            if (string.IsNullOrWhiteSpace(text) || maxCharsPerChunk <= 0)
            {
                return chunks;
            }

            string normalized = text.Replace("\r\n", "\n");
            int offset = 0;
            while (offset < normalized.Length)
            {
                int remaining = normalized.Length - offset;
                int length = Math.Min(maxCharsPerChunk, remaining);
                int cut = normalized.LastIndexOf('\n', Math.Min(normalized.Length - 1, offset + length - 1), length);

                if (cut < offset + (maxCharsPerChunk / 3))
                {
                    cut = offset + length - 1;
                }

                int finalLength = Math.Max(1, (cut - offset) + 1);
                string chunk = normalized.Substring(offset, finalLength).Trim();
                if (!string.IsNullOrWhiteSpace(chunk))
                {
                    chunks.Add(chunk);
                }

                offset += finalLength;
            }

            return chunks;
        }

        /// <summary>
        /// Reduces content to a target token budget using semantic compression and hard fallback trimming.
        /// </summary>
        private static async Task<string> TrimToTokenBudgetAsync(
            string content,
            int tokenBudget,
            IChatCompletionService chatCompletionService,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return string.Empty;
            }

            int safeBudget = Math.Max(1, tokenBudget);
            if (EstimateResultTokens(content) <= safeBudget)
            {
                return HardClampToTokenBudget(content.Trim(), safeBudget);
            }

            if (chatCompletionService != null)
            {
                string compactPrompt =
                    $"Compress the following technical summary to at most {safeBudget} tokens. " +
                    "Preserve key facts, ids, numbers, warnings, and errors. Return plain text only.\n\n" +
                    content;
                string compactedByModel = await SummarizeTextAsync(
                    chatCompletionService,
                    compactPrompt,
                    safeBudget,
                    cancellationToken).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(compactedByModel))
                {
                    string normalized = compactedByModel.Trim();
                    if (EstimateResultTokens(normalized) <= safeBudget)
                    {
                        return HardClampToTokenBudget(normalized, safeBudget);
                    }

                    // One more semantic compression attempt before falling back to hard trim.
                    string secondPassPrompt =
                        $"The text is still too long. Compress it further to at most {safeBudget} tokens. " +
                        "Keep only essential technical facts and values.\n\n" +
                        normalized;
                    string secondPass = await SummarizeTextAsync(
                        chatCompletionService,
                        secondPassPrompt,
                        safeBudget,
                        cancellationToken).ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(secondPass))
                    {
                        string secondNormalized = secondPass.Trim();
                        if (EstimateResultTokens(secondNormalized) <= safeBudget)
                        {
                            return HardClampToTokenBudget(secondNormalized, safeBudget);
                        }
                    }
                }
            }

            int maxChars = Math.Max(1, safeBudget * 4);
            string compactFallback = CompactWhitespace(content, maxChars);
            return HardClampToTokenBudget(compactFallback, safeBudget);
        }

        /// <summary>
        /// Deterministically enforces a token budget via iterative character-level clamping.
        /// </summary>
        /// <remarks>
        /// This is a last-resort safety path when semantic compression cannot meet constraints.
        /// </remarks>
        private static string HardClampToTokenBudget(string text, int tokenBudget)
        {
            int safeBudget = Math.Max(1, tokenBudget);
            string candidate = string.IsNullOrWhiteSpace(text) ? string.Empty : text.Trim();
            if (string.IsNullOrEmpty(candidate))
            {
                return ".";
            }

            int maxChars = Math.Max(1, safeBudget * 4);
            if (candidate.Length > maxChars)
            {
                candidate = CompactWhitespace(candidate, maxChars);
            }

            int guard = 0;
            while (EstimateResultTokens(candidate) > safeBudget && candidate.Length > 1 && guard < 128)
            {
                int overflowTokens = EstimateResultTokens(candidate) - safeBudget;
                int shrinkChars = Math.Max(1, overflowTokens * 4);
                int nextLength = Math.Max(1, candidate.Length - shrinkChars);
                candidate = candidate.Substring(0, nextLength).TrimEnd();
                guard++;
            }

            while (EstimateResultTokens(candidate) > safeBudget && candidate.Length > 1)
            {
                candidate = candidate.Substring(0, candidate.Length - 1).TrimEnd();
            }

            if (string.IsNullOrWhiteSpace(candidate))
            {
                return ".";
            }

            if (EstimateResultTokens(candidate) > safeBudget)
            {
                Log.Error(
                    $"[{ToolCompactionInvariantErrorCode}] Hard clamp could not satisfy target token budget. Returning smallest possible text. " +
                    $"targetTokens={safeBudget}, finalTokens={EstimateResultTokens(candidate)}, finalChars={candidate.Length}.");
            }

            return candidate;
        }

        /// <summary>
        /// Value object describing whether and how a tool result was compacted.
        /// </summary>
        private sealed class ToolResultCompactionOutcome
        {
            public bool IsCompacted { get; private set; }
            public string CompactedResult { get; private set; }
            public int OriginalTokens { get; private set; }
            public int CompactedTokens { get; private set; }
            public bool UsedSemanticSummary { get; private set; }
            public string CompactionMethod { get; private set; }
            public string OriginalText { get; private set; }

            public static ToolResultCompactionOutcome NotCompacted(int originalTokens)
            {
                return new ToolResultCompactionOutcome
                {
                    IsCompacted = false,
                    CompactedResult = string.Empty,
                    OriginalTokens = Math.Max(0, originalTokens),
                    CompactedTokens = Math.Max(0, originalTokens),
                    UsedSemanticSummary = false,
                    CompactionMethod = string.Empty,
                    OriginalText = string.Empty
                };
            }

            public static ToolResultCompactionOutcome Compacted(
                string compactedResult,
                int originalTokens,
                int compactedTokens,
                bool usedSemanticSummary,
                string compactionMethod,
                string originalText)
            {
                return new ToolResultCompactionOutcome
                {
                    IsCompacted = true,
                    CompactedResult = compactedResult ?? string.Empty,
                    OriginalTokens = Math.Max(0, originalTokens),
                    CompactedTokens = Math.Max(0, compactedTokens),
                    UsedSemanticSummary = usedSemanticSummary,
                    CompactionMethod = compactionMethod ?? string.Empty,
                    OriginalText = originalText ?? string.Empty
                };
            }
        }

        private sealed class ToolCompletionOutcome
        {
            public ToolActivityStatus Status { get; set; }
            public string DisplayText { get; set; } = string.Empty;
        }

        /// <summary>
        /// Indicates whether a plugin name belongs to the permission-controlled tool set.
        /// </summary>
        private static bool IsPermissionControlledPlugin(string pluginName)
        {
            if (string.IsNullOrWhiteSpace(pluginName))
            {
                return false;
            }

            return pluginName.StartsWith(PermissionControlledPluginPrefix, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks per-function allow/deny status derived from runtime settings.
        /// </summary>
        private static bool IsEditorStatusFunctionAllowed(string functionName)
        {
            if (string.IsNullOrWhiteSpace(functionName))
            {
                return true;
            }

            var map = ParseAllowedFunctions(Settings.Default.editorStatusAllowedFunctions);
            return !map.TryGetValue(functionName.Trim(), out bool allowed) || allowed;
        }

        /// <summary>
        /// Parses semicolon-delimited function toggles in the form <c>FunctionName=true|false</c>.
        /// </summary>
        private static Dictionary<string, bool> ParseAllowedFunctions(string raw)
        {
            var map = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(raw))
            {
                return map;
            }

            var parts = raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var kv = part.Split(new[] { '=' }, 2);
                var key = (kv.ElementAtOrDefault(0) ?? string.Empty).Trim();
                var val = (kv.ElementAtOrDefault(1) ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                if (bool.TryParse(val, out bool b))
                {
                    map[key] = b;
                }
            }

            return map;
        }

        /// <summary>
        /// Returns whether a function is classified as mutating according to plugin metadata.
        /// </summary>
        private static bool IsMutationFunction(string functionName)
        {
            if (string.IsNullOrWhiteSpace(functionName))
            {
                return false;
            }

            return MutationFunctionNames.Value.Contains(functionName.Trim());
        }

        /// <summary>
        /// Builds the set of mutating function names by scanning known plugin attributes.
        /// </summary>
        private static HashSet<string> BuildMutationFunctionNames()
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (Type pluginType in KnownPluginTypes)
            {
                if (pluginType == null)
                {
                    continue;
                }

                MethodInfo[] methods = pluginType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                foreach (MethodInfo method in methods)
                {
                    KernelFunctionAttribute kernelFunction = method.GetCustomAttribute<KernelFunctionAttribute>();
                    if (kernelFunction == null || string.IsNullOrWhiteSpace(kernelFunction.Name))
                    {
                        continue;
                    }

                    AiToolMutationAttribute mutationAttribute = method.GetCustomAttribute<AiToolMutationAttribute>();
                    if (mutationAttribute?.CanModifyProgram != true)
                    {
                        continue;
                    }

                    result.Add(kernelFunction.Name.Trim());
                }
            }

            return result;
        }

        /// <summary>
        /// Estimates token count for arbitrary values after stable serialization.
        /// </summary>
        private static int EstimateResultTokens(object value)
        {
            string serialized = SerializeResult(value);
            if (WorkspaceScreenshotContent.TryParse(serialized, out _))
                return WorkspaceScreenshotContent.EstimatedImageTokens;
            if (string.IsNullOrWhiteSpace(serialized))
            {
                return 0;
            }

            return Math.Max(1, (int)Math.Ceiling(serialized.Length / 4.0));
        }

        /// <summary>
        /// Serializes tool results to plain text for logging, compaction, and token estimation.
        /// </summary>
        private static string SerializeResult(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            string serialized;
            if (value is string s)
            {
                serialized = s;
            }
            else
            {
                try
                {
                    serialized = JsonConvert.SerializeObject(value, Formatting.None) ?? string.Empty;
                }
                catch
                {
                    serialized = value.ToString() ?? string.Empty;
                }
            }

            if (string.IsNullOrWhiteSpace(serialized))
            {
                return string.Empty;
            }

            return serialized;
        }

        private static string FormatResultForDisplay(object value)
        {
            string serialized = SerializeResult(value);
            if (string.IsNullOrWhiteSpace(serialized))
            {
                return GetResourceTextOrFallback("AiChatToolEmptyResult", "(empty result)");
            }

            string trimmed = serialized.Trim();
            try
            {
                object token = JsonConvert.DeserializeObject(trimmed);
                if (token != null)
                {
                    string pretty = JsonConvert.SerializeObject(token, Formatting.Indented);
                    return CompactWhitespaceForDisplay(pretty, 4000);
                }
            }
            catch
            {
            }

            return CompactWhitespaceForDisplay(trimmed, 4000);
        }

        private static ToolCompletionOutcome DetermineToolCompletionOutcome(object resultObject)
        {
            if (TryExtractLogicalToolFailure(resultObject, out string failureMessage))
            {
                return new ToolCompletionOutcome
                {
                    Status = ToolActivityStatus.Failed,
                    DisplayText = failureMessage
                };
            }

            return new ToolCompletionOutcome
            {
                Status = ToolActivityStatus.Succeeded,
                DisplayText = FormatResultForDisplay(resultObject)
            };
        }

        private static bool TryExtractLogicalToolFailure(object resultObject, out string failureMessage)
        {
            failureMessage = string.Empty;
            if (resultObject == null)
            {
                return false;
            }

            object successValue = TryGetToolResultFieldValue(resultObject, "success", "Success");
            if (successValue == null || !TryConvertToBoolean(successValue, out bool success) || success)
            {
                return false;
            }

            object errorValue = TryGetToolResultFieldValue(
                resultObject,
                "error",
                "Error",
                "message",
                "Message",
                "reason",
                "Reason");

            string text = ConvertToolResultFieldToText(errorValue);
            failureMessage = string.IsNullOrWhiteSpace(text)
                ? GetResourceTextOrFallback("AiChatToolUnknownError", "Unknown error.")
                : CompactWhitespaceForDisplay(text, 4000);
            return true;
        }

        private static object TryGetToolResultFieldValue(object resultObject, params string[] fieldNames)
        {
            if (resultObject == null || fieldNames == null || fieldNames.Length == 0)
            {
                return null;
            }

            foreach (string fieldName in fieldNames)
            {
                object directValue = TryGetPropertyValue(resultObject, fieldName);
                if (directValue != null)
                {
                    return directValue;
                }
            }

            if (TryGetToolResultFieldValueFromJsonLike(resultObject, fieldNames, out object jsonLikeValue))
            {
                return jsonLikeValue;
            }

            string serialized = SerializeResult(resultObject);
            if (string.IsNullOrWhiteSpace(serialized) ||
                string.Equals(serialized, resultObject as string, StringComparison.Ordinal))
            {
                return null;
            }

            return TryGetToolResultFieldValueFromJsonString(serialized, fieldNames, out object serializedValue)
                ? serializedValue
                : null;
        }

        private static bool TryGetToolResultFieldValueFromJsonLike(object resultObject, string[] fieldNames, out object value)
        {
            value = null;
            if (resultObject == null || fieldNames == null || fieldNames.Length == 0)
            {
                return false;
            }

            if (resultObject is JToken token)
            {
                return TryGetFieldValueFromJToken(token, fieldNames, out value);
            }

            if (resultObject is string text)
            {
                return TryGetToolResultFieldValueFromJsonString(text, fieldNames, out value);
            }

            return false;
        }

        private static bool TryGetToolResultFieldValueFromJsonString(string json, string[] fieldNames, out object value)
        {
            value = null;
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                JToken token = JToken.Parse(json);
                return TryGetFieldValueFromJToken(token, fieldNames, out value);
            }
            catch
            {
                return false;
            }
        }

        private static bool TryGetFieldValueFromJToken(JToken token, string[] fieldNames, out object value)
        {
            value = null;
            if (token == null || fieldNames == null || fieldNames.Length == 0)
            {
                return false;
            }

            if (token is not JObject obj)
            {
                return false;
            }

            foreach (string fieldName in fieldNames)
            {
                JProperty property = obj.Properties()
                    .FirstOrDefault(candidate => string.Equals(candidate.Name, fieldName, StringComparison.OrdinalIgnoreCase));
                if (property == null)
                {
                    continue;
                }

                value = ConvertJTokenToPlainObject(property.Value);
                return true;
            }

            return false;
        }

        private static object ConvertJTokenToPlainObject(JToken token)
        {
            if (token == null)
            {
                return null;
            }

            if (token is JValue scalar)
            {
                return scalar.Value;
            }

            return token;
        }

        private static string ConvertToolResultFieldToText(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is JToken token)
            {
                return token.Type == JTokenType.String
                    ? token.ToString()
                    : token.ToString(Formatting.None);
            }

            return value.ToString()?.Trim() ?? string.Empty;
        }

        private static bool TryConvertToBoolean(object value, out bool result)
        {
            result = false;
            if (value == null)
            {
                return false;
            }

            if (value is bool boolean)
            {
                result = boolean;
                return true;
            }

            if (value is string text && bool.TryParse(text.Trim(), out bool parsed))
            {
                result = parsed;
                return true;
            }

            if (value is JValue jValue)
            {
                return TryConvertToBoolean(jValue.Value, out result);
            }

            try
            {
                result = Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string FormatFailureForDisplay(Exception exception)
        {
            if (exception == null)
            {
                return GetResourceTextOrFallback("AiChatToolUnknownError", "Unknown error.");
            }

            List<string> parts = new List<string>();
            Exception current = exception;
            while (current != null)
            {
                string message = string.IsNullOrWhiteSpace(current.Message)
                    ? current.GetType().Name
                    : current.Message.Trim();
                parts.Add(message);
                current = current.InnerException;
            }

            return CompactWhitespaceForDisplay(string.Join(Environment.NewLine, parts.Distinct(StringComparer.Ordinal)), 4000);
        }

        private static string CompactWhitespaceForDisplay(string text, int maxChars)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            string normalized = text.Replace("\r\n", "\n").Trim();
            if (normalized.Length <= maxChars)
            {
                return normalized;
            }

            return normalized.Substring(0, maxChars).TrimEnd() + Environment.NewLine + "...";
        }

        private static string GetResourceTextOrFallback(string resourceKey, string fallback)
        {
            string value = Resources.ResourceManager.GetString(resourceKey);
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        /// <summary>
        /// Normalizes whitespace and optionally truncates text for compact logging/output previews.
        /// </summary>
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

        /// <summary>
        /// Detects likely context-window overflow errors in an exception chain.
        /// </summary>
        private static bool IsLikelyContextWindowExceeded(Exception exception)
        {
            Exception current = exception;
            while (current != null)
            {
                string message = current.Message ?? string.Empty;
                if (message.IndexOf("exceeds the available context size", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    message.IndexOf("maximum context length", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    message.IndexOf("context window", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    message.IndexOf("context_length_exceeded", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }

                current = current.InnerException;
            }

            return false;
        }
    }
}
