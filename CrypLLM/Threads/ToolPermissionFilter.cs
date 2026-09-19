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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

// Module overview:
// Central policy filter for AI-triggered tool calls.
// Responsibilities: permission gating, tool activity and execution diagnostics.
namespace CrypTool.CrypLLM.Threads
{
    /// <summary>
    /// Enforces tool permissions and logs tool-execution diagnostics.
    /// </summary>
    internal sealed class ToolPermissionFilter : IAutoFunctionInvocationFilter
    {
        private const string PermissionControlledPluginPrefix = "CrypLLM_";
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
        /// Executes permission checks and reports activity for each tool function.
        /// Context compression is performed before model requests, not by denying tools.
        /// </summary>
        /// <remarks>
        /// Input: invocation metadata and arguments.
        /// Output: an allowed invocation result or a denial by user permissions.
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

            object resultObject = null;
            try
            {
                resultObject = context?.Result?.GetValue<object>();
            }
            catch
            {
                resultObject = null;
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

        private sealed class ToolCompletionOutcome
        {
            public ToolActivityStatus Status { get; set; }
            public string DisplayText { get; set; } = string.Empty;
        }

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
