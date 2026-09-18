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
using CrypTool.CrypLLM.Threads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

// Module overview:
// Debug window for inspecting recorded LLM invocations, including
// prompt context, text-reduction traces, and HTTP request/response exchanges.
namespace CrypTool.CrypLLM
{
    /// <summary>
    /// Diagnostic WPF window for browsing invocation history and transport-level debug artifacts.
    /// </summary>
    public partial class LastInvocationDebugWindow : Window
    {
        private readonly List<LastInvocationDebugInfo> _invocations;
        private readonly List<DebugTextReduction> _textReductions = new List<DebugTextReduction>();
        private readonly List<DebugHttpExchange> _httpExchanges = new List<DebugHttpExchange>();
        private readonly List<DebugToolCall> _toolCalls = new List<DebugToolCall>();
        private bool _updatingUi;

        /// <summary>
        /// Initializes the window for a single invocation by wrapping it as a one-element history list.
        /// </summary>
        public LastInvocationDebugWindow(LastInvocationDebugInfo info)
            : this(info == null
                ? new List<LastInvocationDebugInfo>()
                : new List<LastInvocationDebugInfo> { info })
        {
        }

        /// <summary>
        /// Initializes the window from invocation history and selects the newest entry by default.
        /// </summary>
        public LastInvocationDebugWindow(IReadOnlyList<LastInvocationDebugInfo> infos)
        {
            InitializeComponent();

            _invocations = NormalizeInvocations(infos);
            InitializeInvocationSelector();
            ApplyInvocationSelection(_invocations.Count - 1);
        }

        private static string GetResourceText(string key, string fallback)
        {
            return CrypTool.CrypLLM.Properties.Resources.ResourceManager.GetString(key) ?? fallback;
        }

        /// <summary>
        /// Normalizes invocation input by removing null entries and ensuring a non-empty fallback list.
        /// </summary>
        private static List<LastInvocationDebugInfo> NormalizeInvocations(IReadOnlyList<LastInvocationDebugInfo> infos)
        {
            List<LastInvocationDebugInfo> result = new List<LastInvocationDebugInfo>();
            if (infos != null)
            {
                for (int i = 0; i < infos.Count; i++)
                {
                    if (infos[i] != null)
                    {
                        result.Add(infos[i]);
                    }
                }
            }

            if (result.Count == 0)
            {
                result.Add(new LastInvocationDebugInfo());
            }

            return result;
        }

        /// <summary>
        /// Populates the invocation selector and enables it only when multiple entries are available.
        /// </summary>
        private void InitializeInvocationSelector()
        {
            _updatingUi = true;
            try
            {
                InvocationSelectorComboBox.Items.Clear();
                ToolInvocationSelectorComboBox.Items.Clear();
                PreFinalInvocationSelectorComboBox.Items.Clear();
                for (int i = 0; i < _invocations.Count; i++)
                {
                    string label = BuildInvocationLabel(_invocations[i], i);
                    InvocationSelectorComboBox.Items.Add(label);
                    ToolInvocationSelectorComboBox.Items.Add(label);
                    PreFinalInvocationSelectorComboBox.Items.Add(label);
                }

                InvocationSelectorComboBox.IsEnabled = _invocations.Count > 1;
                ToolInvocationSelectorComboBox.IsEnabled = _invocations.Count > 1;
                PreFinalInvocationSelectorComboBox.IsEnabled = _invocations.Count > 1;
            }
            finally
            {
                _updatingUi = false;
            }
        }

        /// <summary>
        /// Builds a compact selector label from timestamp and truncated user question text.
        /// </summary>
        private static string BuildInvocationLabel(LastInvocationDebugInfo info, int index)
        {
            string timestamp = info?.CreatedAtLocal == default
                ? GetResourceText("LastInvocationSelectorNoTime", "(no time)")
                : info.CreatedAtLocal.ToString("yyyy-MM-dd HH:mm:ss");
            string question = (info?.UserInput ?? string.Empty)
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Trim();
            if (question.Length > 90)
            {
                question = question.Substring(0, 90) + "...";
            }

            if (string.IsNullOrWhiteSpace(question))
            {
                question = GetResourceText("LastInvocationSelectorEmptyQuestion", "(empty user question)");
            }

            return $"#{index + 1} {timestamp} | {question}";
        }

        /// <summary>
        /// Applies invocation change requests from the selector while suppressing recursive updates.
        /// </summary>
        private void InvocationSelectorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_updatingUi)
            {
                return;
            }

            ApplyInvocationSelection(InvocationSelectorComboBox?.SelectedIndex ?? -1);
        }

        /// <summary>
        /// Synchronizes summary/context panes and dependent selectors for the chosen invocation.
        /// </summary>
        private void ApplyInvocationSelection(int index)
        {
            int safeIndex = index;
            if (safeIndex < 0 || safeIndex >= _invocations.Count)
            {
                safeIndex = Math.Max(0, _invocations.Count - 1);
            }

            LastInvocationDebugInfo selected = _invocations[safeIndex];

            _updatingUi = true;
            try
            {
                InvocationSelectorComboBox.SelectedIndex = safeIndex;
                ToolInvocationSelectorComboBox.SelectedIndex = safeIndex;
                PreFinalInvocationSelectorComboBox.SelectedIndex = safeIndex;
                SummaryTextBlock.Text = BuildSummaryText(selected);
                ContextTextBox.Text = BuildContextText(selected);
            }
            finally
            {
                _updatingUi = false;
            }

            InitializeReductionsForInvocation(selected);
            InitializeHttpExchangesForInvocation(selected);
            InitializeToolCallsForInvocation(selected);
            InitializePreFinalAnswerView(selected);
        }

        /// <summary>
        /// Builds a one-line technical summary containing model and budget diagnostics.
        /// </summary>
        private static string BuildSummaryText(LastInvocationDebugInfo info)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetResourceText("LastInvocationSummaryTime", "Time"));
            sb.Append(": ");
            sb.Append(info.CreatedAtLocal == default ? GetResourceText("LastInvocationValueUnknown", "(unknown)") : info.CreatedAtLocal.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.Append("   |   ");
            sb.Append(GetResourceText("LastInvocationSummaryModel", "Model"));
            sb.Append(": ");
            sb.Append(string.IsNullOrWhiteSpace(info.ModelId) ? GetResourceText("LastInvocationValueUnknown", "(unknown)") : info.ModelId);
            sb.Append("   |   ");
            sb.Append(GetResourceText("LastInvocationSummaryTabId", "tabId"));
            sb.Append(": ");
            sb.Append(string.IsNullOrWhiteSpace(info.ActiveTabId) ? GetResourceText("LastInvocationValueNone", "(none)") : info.ActiveTabId);
            sb.Append("   |   ");
            sb.Append(GetResourceText("LastInvocationSummaryReducedThread", "reduced-thread"));
            sb.Append(": ");
            sb.Append(info.UsedReducedThread
                ? GetResourceText("LastInvocationValueYes", "yes")
                : GetResourceText("LastInvocationValueNo", "no"));
            sb.AppendLine();
            sb.Append(GetResourceText("LastInvocationSummaryBudget", "Budget"));
            sb.Append(": ");
            sb.Append(GetResourceText("LastInvocationSummaryContextWindow", "contextWindow"));
            sb.Append("=");
            sb.Append(info.Budget?.ContextWindowTokens ?? 0);
            sb.Append(", ");
            sb.Append(GetResourceText("LastInvocationSummaryEffectiveContext", "effectiveContext"));
            sb.Append("=");
            sb.Append(info.Budget?.EffectiveContextWindowTokens ?? 0);
            sb.Append(", ");
            sb.Append(GetResourceText("LastInvocationSummaryUncertaintyReserve", "uncertaintyReserve"));
            sb.Append("=");
            sb.Append(info.Budget?.UncertaintyReserveTokens ?? 0);
            sb.Append(", ");
            sb.Append(GetResourceText("LastInvocationSummaryHistoryBudget", "historyBudget"));
            sb.Append("=");
            sb.Append(info.Budget?.HistoryBudgetTokens ?? 0);
            sb.Append(", ");
            sb.Append(GetResourceText("LastInvocationSummaryHistoryEstimated", "historyEstimated"));
            sb.Append("=");
            sb.Append(info.Budget?.EstimatedHistoryTokens ?? 0);
            sb.Append(", ");
            sb.Append(GetResourceText("LastInvocationSummaryUserInputTokens", "userInputTokens"));
            sb.Append("=");
            sb.Append(info.Budget?.UserInputTokens ?? 0);
            sb.Append(", ");
            sb.Append(GetResourceText("LastInvocationSummaryToolSchema", "toolSchema"));
            sb.Append("=");
            sb.Append(info.Budget?.ToolSchemaBudgetTokens ?? 0);
            sb.Append(", ");
            sb.Append(GetResourceText("LastInvocationSummaryResponseReserve", "responseReserve"));
            sb.Append("=");
            sb.Append(info.Budget?.ResponseReserveTokens ?? 0);
            sb.Append(", ");
            sb.Append(GetResourceText("LastInvocationSummarySafety", "safety"));
            sb.Append("=");
            sb.Append(info.Budget?.SafetyMarginTokens ?? 0);
            sb.Append(", ");
            sb.Append(GetResourceText("LastInvocationSummaryPreflightPlanned", "preflightPlanned"));
            sb.Append("=");
            sb.Append(info.Budget?.PreflightPlannedTokens ?? 0);
            sb.Append(", ");
            sb.Append(GetResourceText("LastInvocationSummaryPreflightPassed", "preflightPassed"));
            sb.Append("=");
            sb.Append(info.Budget?.PreflightPassed == true
                ? GetResourceText("LastInvocationValueYes", "yes")
                : GetResourceText("LastInvocationValueNo", "no"));
            return sb.ToString();
        }

        /// <summary>
        /// Builds the full prompt-context pane, including user input and serialized prompt messages.
        /// </summary>
        private static string BuildContextText(LastInvocationDebugInfo info)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GetResourceText("LastInvocationPreFinalSectionUserInput", "[UserInput]"));
            sb.AppendLine(string.IsNullOrWhiteSpace(info.UserInput)
                ? GetResourceText("LastInvocationValueEmpty", "(empty)")
                : info.UserInput);
            sb.AppendLine(new string('-', 90));
            sb.AppendLine(GetResourceText("LastInvocationContextSectionPromptMessages", "[PromptMessages]"));

            if (info.PromptMessages == null || info.PromptMessages.Count == 0)
            {
                sb.AppendLine(GetResourceText("LastInvocationPreFinalNoPromptMessages", "(no prompt messages stored)"));
                return sb.ToString();
            }

            for (int i = 0; i < info.PromptMessages.Count; i++)
            {
                DebugPromptMessage message = info.PromptMessages[i];
                sb.Append('[');
                sb.Append(message.Index.ToString("000"));
                sb.Append("] ");
                sb.AppendLine(message.Role ?? GetResourceText("LastInvocationValueUnknown", "(unknown)"));
                sb.AppendLine(string.IsNullOrWhiteSpace(message.Content)
                    ? GetResourceText("LastInvocationValueEmpty", "(empty)")
                    : message.Content);

                if (message.ItemSummaries != null && message.ItemSummaries.Count > 0)
                {
                    for (int summaryIndex = 0; summaryIndex < message.ItemSummaries.Count; summaryIndex++)
                    {
                        sb.Append("  ");
                        sb.AppendLine(message.ItemSummaries[summaryIndex] ?? string.Empty);
                    }
                }

                sb.AppendLine(new string('-', 90));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Loads text-reduction diagnostics for the selected invocation.
        /// </summary>
        private void InitializeReductionsForInvocation(LastInvocationDebugInfo info)
        {
            _textReductions.Clear();
            if (info?.TextReductions != null)
            {
                _textReductions.AddRange(info.TextReductions.Where(x => x != null));
            }

            InitializeReductionsSelector();
        }

        /// <summary>
        /// Initializes reduction selector UI and default detail pane values.
        /// </summary>
        private void InitializeReductionsSelector()
        {
            ReductionsComboBox.Items.Clear();

            if (_textReductions.Count == 0)
            {
                ReductionsComboBox.IsEnabled = false;
                ReductionMetaTextBlock.Text = GetResourceText("LastInvocationReductionNoneDetected", "No direct text reductions detected for the last request.");
                ReductionOriginalTextBox.Text = GetResourceText("LastInvocationValueEmpty", "(empty)");
                ReductionShortTextBox.Text = GetResourceText("LastInvocationValueEmpty", "(empty)");
                return;
            }

            for (int i = 0; i < _textReductions.Count; i++)
            {
                DebugTextReduction reduction = _textReductions[i];
                string label = string.Format(
                    GetResourceText("LastInvocationReductionLabelFormat", "Reduction #{0}: {1} [{2}->{3}]"),
                    i + 1,
                    BuildToolName(reduction.PluginName, reduction.FunctionName),
                    reduction.OriginalTokens,
                    reduction.ReducedTokens);
                ReductionsComboBox.Items.Add(label);
            }

            ReductionsComboBox.IsEnabled = true;
            ReductionsComboBox.SelectedIndex = 0;
            UpdateSelectedReduction(0);
        }

        /// <summary>
        /// Handles reduction selector changes and updates the reduction detail view.
        /// </summary>
        private void ReductionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectedReduction(ReductionsComboBox?.SelectedIndex ?? -1);
        }

        /// <summary>
        /// Renders metadata, original text, and compacted text for one selected reduction entry.
        /// </summary>
        private void UpdateSelectedReduction(int index)
        {
            if (_textReductions.Count == 0)
            {
                return;
            }

            int safeIndex = index;
            if (safeIndex < 0 || safeIndex >= _textReductions.Count)
            {
                safeIndex = 0;
            }

            DebugTextReduction reduction = _textReductions[safeIndex];
            ReductionMetaTextBlock.Text = string.Format(
                GetResourceText("LastInvocationReductionMetaFormat", "source={0}, method={1}, tool={2}, tokens={3}->{4}"),
                SafeText(reduction.Source),
                SafeText(reduction.Method),
                BuildToolName(reduction.PluginName, reduction.FunctionName),
                reduction.OriginalTokens,
                reduction.ReducedTokens);
            ReductionOriginalTextBox.Text = string.IsNullOrWhiteSpace(reduction.OriginalText)
                ? GetResourceText("LastInvocationValueEmpty", "(empty)")
                : reduction.OriginalText;
            ReductionShortTextBox.Text = string.IsNullOrWhiteSpace(reduction.ReducedText)
                ? GetResourceText("LastInvocationValueEmpty", "(empty)")
                : reduction.ReducedText;
        }

        /// <summary>
        /// Loads and orders HTTP exchanges for the selected invocation and initializes related UI controls.
        /// </summary>
        private void InitializeHttpExchangesForInvocation(LastInvocationDebugInfo info)
        {
            _httpExchanges.Clear();
            if (info?.HttpExchanges != null)
            {
                _httpExchanges.AddRange(
                    info.HttpExchanges
                        .Where(x => x != null)
                        .OrderBy(x => x.Sequence));
            }

            _updatingUi = true;
            try
            {
                HttpRequestSelectorComboBox.Items.Clear();

                if (_httpExchanges.Count == 0)
                {
                    HttpRequestSelectorComboBox.IsEnabled = false;
                    HttpExchangeMetaTextBlock.Text = GetResourceText("LastInvocationHttpNoRequestsForQuestion", "No HTTP requests are stored for this user question.");
                    RawRequestTextBox.Text = string.IsNullOrWhiteSpace(info?.RawLlmRequest)
                        ? GetResourceText("LastInvocationHttpNoRequestJsonStored", "(no stored request JSON available)")
                        : info.RawLlmRequest;
                    RawResponseTextBox.Text = string.IsNullOrWhiteSpace(info?.RawLlmResponse)
                        ? GetResourceText("LastInvocationHttpNoResponseJsonStored", "(no stored response JSON available)")
                        : info.RawLlmResponse;
                    return;
                }

                for (int i = 0; i < _httpExchanges.Count; i++)
                {
                    HttpRequestSelectorComboBox.Items.Add(BuildHttpExchangeLabel(_httpExchanges[i]));
                }

                HttpRequestSelectorComboBox.IsEnabled = true;
                HttpRequestSelectorComboBox.SelectedIndex = 0;
            }
            finally
            {
                _updatingUi = false;
            }

            UpdateSelectedHttpExchange(0);
        }

        /// <summary>
        /// Builds a concise selector label for one HTTP exchange.
        /// </summary>
        private static string BuildHttpExchangeLabel(DebugHttpExchange exchange)
        {
            if (exchange == null)
            {
                return GetResourceText("LastInvocationHttpUnknownRequest", "(unknown HTTP request)");
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("#");
            sb.Append(Math.Max(0, exchange.Sequence));
            sb.Append(" ");
            sb.Append(string.IsNullOrWhiteSpace(exchange.Method) ? "HTTP" : exchange.Method.ToUpperInvariant());
            sb.Append(" ");
            sb.Append(TrimUriForDisplay(exchange.Uri));
            sb.Append(" -> ");
            sb.Append(exchange.StatusCode?.ToString() ?? GetResourceText("LastInvocationValueNone", "(none)"));
            if (!string.IsNullOrWhiteSpace(exchange.ReasonPhrase))
            {
                sb.Append(" ");
                sb.Append(exchange.ReasonPhrase);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Reduces a full URI to path and query for compact display in selector labels.
        /// </summary>
        private static string TrimUriForDisplay(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
            {
                return GetResourceText("LastInvocationHttpUnknownUri", "(unknown-uri)");
            }

            if (Uri.TryCreate(uri, UriKind.Absolute, out Uri parsed))
            {
                return parsed.PathAndQuery;
            }

            return uri;
        }

        /// <summary>
        /// Handles HTTP exchange selector changes while avoiding re-entrant updates.
        /// </summary>
        private void HttpRequestSelectorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_updatingUi)
            {
                return;
            }

            UpdateSelectedHttpExchange(HttpRequestSelectorComboBox?.SelectedIndex ?? -1);
        }

        /// <summary>
        /// Updates HTTP metadata and raw payload panes for the selected exchange.
        /// </summary>
        private void UpdateSelectedHttpExchange(int index)
        {
            if (_httpExchanges.Count == 0)
            {
                return;
            }

            int safeIndex = index;
            if (safeIndex < 0 || safeIndex >= _httpExchanges.Count)
            {
                safeIndex = 0;
            }

            DebugHttpExchange exchange = _httpExchanges[safeIndex];
            HttpExchangeMetaTextBlock.Text = BuildHttpExchangeMeta(exchange);
            RawRequestTextBox.Text = string.IsNullOrWhiteSpace(exchange.RequestBody)
                ? GetResourceText("LastInvocationValueEmpty", "(empty)")
                : exchange.RequestBody;
            RawResponseTextBox.Text = ResolveResponseBodyDisplay(exchange);
        }

        /// <summary>
        /// Builds detailed metadata text for one HTTP exchange including error details when available.
        /// </summary>
        private static string BuildHttpExchangeMeta(DebugHttpExchange exchange)
        {
            if (exchange == null)
            {
                return GetResourceText("LastInvocationHttpNoMetadata", "(no metadata)");
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(GetResourceText("LastInvocationHttpMetaRequest", "request"));
            sb.Append("=");
            sb.Append(string.IsNullOrWhiteSpace(exchange.Method) ? "HTTP" : exchange.Method.ToUpperInvariant());
            sb.Append(" ");
            sb.Append(string.IsNullOrWhiteSpace(exchange.Uri)
                ? GetResourceText("LastInvocationHttpUnknownUri", "(unknown-uri)")
                : exchange.Uri);
            sb.Append(" | ");
            sb.Append(GetResourceText("LastInvocationHttpMetaResponse", "response"));
            sb.Append("=");
            sb.Append(exchange.StatusCode?.ToString() ?? GetResourceText("LastInvocationValueNone", "(none)"));
            if (!string.IsNullOrWhiteSpace(exchange.ReasonPhrase))
            {
                sb.Append(" ");
                sb.Append(exchange.ReasonPhrase);
            }

            if (!string.IsNullOrWhiteSpace(exchange.Error))
            {
                sb.Append(" | ");
                sb.Append(GetResourceText("LastInvocationHttpMetaError", "error"));
                sb.Append("=");
                sb.Append(exchange.Error);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Resolves response pane content, preferring response body and falling back to structured error JSON.
        /// </summary>
        private static string ResolveResponseBodyDisplay(DebugHttpExchange exchange)
        {
            if (exchange == null)
            {
                return GetResourceText("LastInvocationValueEmpty", "(empty)");
            }

            if (!string.IsNullOrWhiteSpace(exchange.ResponseBody))
            {
                return exchange.ResponseBody;
            }

            if (!string.IsNullOrWhiteSpace(exchange.Error))
            {
                return $"{{\"error\":\"{EscapeJsonString(exchange.Error)}\"}}";
            }

            return GetResourceText("LastInvocationValueEmpty", "(empty)");
        }

        /// <summary>
        /// Applies invocation changes from the final-answer tab while keeping all tabs synchronized.
        /// </summary>
        private void PreFinalInvocationSelectorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_updatingUi)
            {
                return;
            }

            ApplyInvocationSelection(PreFinalInvocationSelectorComboBox?.SelectedIndex ?? -1);
        }

        /// <summary>
        /// Applies invocation changes from the tool-call tab while keeping all tabs synchronized.
        /// </summary>
        private void ToolInvocationSelectorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_updatingUi)
            {
                return;
            }

            ApplyInvocationSelection(ToolInvocationSelectorComboBox?.SelectedIndex ?? -1);
        }

        /// <summary>
        /// Loads tool-call diagnostics for the selected invocation.
        /// </summary>
        private void InitializeToolCallsForInvocation(LastInvocationDebugInfo info)
        {
            _toolCalls.Clear();
            if (info?.ToolCalls != null)
            {
                _toolCalls.AddRange(
                    info.ToolCalls
                        .Where(x => x != null)
                        .OrderBy(x => x.Sequence));
            }

            _updatingUi = true;
            try
            {
                ToolSelectorComboBox.Items.Clear();

                if (_toolCalls.Count == 0)
                {
                    ToolSelectorComboBox.IsEnabled = false;
                    ToolCallMetaTextBlock.Text = GetResourceText("LastInvocationToolNoCallsForQuestion", "Keine Toolaufrufe fuer diese Userfrage gespeichert.");
                    ToolRequestTextBox.Text = GetResourceText("LastInvocationToolNoRequestJsonStored", "(keine gespeicherte Tool-Request-JSON vorhanden)");
                    ToolResponseTextBox.Text = GetResourceText("LastInvocationToolNoResponseJsonStored", "(keine gespeicherte Tool-Response-JSON vorhanden)");
                    return;
                }

                for (int i = 0; i < _toolCalls.Count; i++)
                {
                    ToolSelectorComboBox.Items.Add(BuildToolCallLabel(_toolCalls[i]));
                }

                ToolSelectorComboBox.IsEnabled = true;
                ToolSelectorComboBox.SelectedIndex = 0;
            }
            finally
            {
                _updatingUi = false;
            }

            UpdateSelectedToolCall(0);
        }

        /// <summary>
        /// Builds a compact selector label showing the AI-facing tool name for a recorded call.
        /// </summary>
        private static string BuildToolCallLabel(DebugToolCall call)
        {
            if (call == null)
            {
                return GetResourceText("LastInvocationToolUnknownCall", "(unbekannter Toolaufruf)");
            }

            return $"#{Math.Max(0, call.Sequence)} {BuildAiFacingToolName(call)}";
        }

        /// <summary>
        /// Handles tool selector changes and updates the tool-call detail view.
        /// </summary>
        private void ToolSelectorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_updatingUi)
            {
                return;
            }

            UpdateSelectedToolCall(ToolSelectorComboBox?.SelectedIndex ?? -1);
        }

        /// <summary>
        /// Renders metadata plus request/response payloads for the selected tool call.
        /// </summary>
        private void UpdateSelectedToolCall(int index)
        {
            if (_toolCalls.Count == 0)
            {
                return;
            }

            int safeIndex = index;
            if (safeIndex < 0 || safeIndex >= _toolCalls.Count)
            {
                safeIndex = 0;
            }

            DebugToolCall call = _toolCalls[safeIndex];
            ToolCallMetaTextBlock.Text = BuildToolCallMeta(call);
            ToolRequestTextBox.Text = BuildToolCallRequestPayload(call);
            ToolResponseTextBox.Text = BuildToolCallResponsePayload(call);
        }

        /// <summary>
        /// Builds one-line metadata for the selected tool call.
        /// </summary>
        private static string BuildToolCallMeta(DebugToolCall call)
        {
            if (call == null)
            {
                return GetResourceText("LastInvocationToolNoMetadata", "(keine Metadaten)");
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(GetResourceText("LastInvocationMetaSequence", "sequence"));
            sb.Append("=");
            sb.Append(Math.Max(0, call.Sequence));
            sb.Append(" | ");
            sb.Append(GetResourceText("LastInvocationMetaTool", "tool"));
            sb.Append("=");
            sb.Append(BuildAiFacingToolName(call));
            sb.Append(" | ");
            sb.Append(GetResourceText("LastInvocationMetaPlugin", "plugin"));
            sb.Append("=");
            sb.Append(SafeText(call.PluginName));
            sb.Append(" | ");
            sb.Append(GetResourceText("LastInvocationMetaFunction", "function"));
            sb.Append("=");
            sb.Append(SafeText(call.FunctionName));
            sb.Append(" | ");
            sb.Append(GetResourceText("LastInvocationMetaCallId", "callId"));
            sb.Append("=");
            sb.Append(SafeText(call.CallId));
            sb.Append(" | ");
            sb.Append(GetResourceText("LastInvocationMetaResponse", "response"));
            sb.Append("=");
            sb.Append(call.HasReturn
                ? GetResourceText("LastInvocationValueYes", "yes")
                : GetResourceText("LastInvocationValueNo", "no"));

            if (!string.IsNullOrWhiteSpace(call.ReturnCallId))
            {
                sb.Append(" | ");
                sb.Append(GetResourceText("LastInvocationMetaReturnCallId", "returnCallId"));
                sb.Append("=");
                sb.Append(call.ReturnCallId);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Builds a technical JSON payload representing the tool request issued by the AI.
        /// </summary>
        private static string BuildToolCallRequestPayload(DebugToolCall call)
        {
            if (call == null)
            {
                return GetResourceText("LastInvocationValueEmpty", "(empty)");
            }

            JObject payload = new JObject
            {
                ["type"] = "function_call",
                ["sequence"] = Math.Max(0, call.Sequence),
                ["callId"] = call.CallId ?? string.Empty,
                ["toolName"] = BuildAiFacingToolName(call),
                ["pluginName"] = call.PluginName ?? string.Empty,
                ["functionName"] = call.FunctionName ?? string.Empty,
                ["arguments"] = ParseJsonTokenOrFallback(call.Arguments, useEmptyObjectFallback: true)
            };

            return payload.ToString(Formatting.Indented);
        }

        /// <summary>
        /// Builds a technical JSON payload representing the tool result returned back into the LLM context.
        /// </summary>
        private static string BuildToolCallResponsePayload(DebugToolCall call)
        {
            if (call == null)
            {
                return GetResourceText("LastInvocationValueEmpty", "(empty)");
            }

            JObject payload = new JObject
            {
                ["type"] = "function_result",
                ["sequence"] = Math.Max(0, call.Sequence),
                ["callId"] = !string.IsNullOrWhiteSpace(call.ReturnCallId) ? call.ReturnCallId : call.CallId ?? string.Empty,
                ["toolName"] = BuildAiFacingToolName(call),
                ["pluginName"] = call.PluginName ?? string.Empty,
                ["functionName"] = !string.IsNullOrWhiteSpace(call.ReturnFunctionName) ? call.ReturnFunctionName : call.FunctionName ?? string.Empty,
                ["hasReturn"] = call.HasReturn,
                ["result"] = ParseJsonTokenOrFallback(call.ReturnValue, useEmptyObjectFallback: false)
            };

            return payload.ToString(Formatting.Indented);
        }

        /// <summary>
        /// Initializes the view showing the complete knowledge state before the final assistant answer.
        /// </summary>
        private void InitializePreFinalAnswerView(LastInvocationDebugInfo info)
        {
            PreFinalContextTextBox.Text = BuildPreFinalAnswerContextText(info);
            FinalAssistantResponseTextBox.Text = BuildFinalAssistantResponseText(info);
        }

        /// <summary>
        /// Builds a consolidated text representation of the information available to the model before the final answer.
        /// </summary>
        private static string BuildPreFinalAnswerContextText(LastInvocationDebugInfo info)
        {
            if (info == null)
            {
                return GetResourceText("LastInvocationValueEmpty", "(empty)");
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(GetResourceText("LastInvocationPreFinalSectionUserInput", "[UserInput]"));
            sb.AppendLine(string.IsNullOrWhiteSpace(info.UserInput)
                ? GetResourceText("LastInvocationValueEmpty", "(empty)")
                : info.UserInput);
            sb.AppendLine(new string('-', 90));

            sb.AppendLine(GetResourceText("LastInvocationPreFinalSectionPromptMessages", "[PromptMessagesBeforeFinalAnswer]"));
            if (info.PromptMessages == null || info.PromptMessages.Count == 0)
            {
                sb.AppendLine(GetResourceText("LastInvocationPreFinalNoPromptMessages", "(keine Prompt-Nachrichten gespeichert)"));
            }
            else
            {
                for (int i = 0; i < info.PromptMessages.Count; i++)
                {
                    DebugPromptMessage message = info.PromptMessages[i];
                    sb.Append('[');
                    sb.Append(message.Index.ToString("000"));
                    sb.Append("] ");
                    sb.AppendLine(message.Role ?? GetResourceText("LastInvocationValueUnknown", "(unknown)"));
                    sb.AppendLine(string.IsNullOrWhiteSpace(message.Content)
                        ? GetResourceText("LastInvocationValueEmpty", "(empty)")
                        : message.Content);

                    if (message.ItemSummaries != null && message.ItemSummaries.Count > 0)
                    {
                        for (int summaryIndex = 0; summaryIndex < message.ItemSummaries.Count; summaryIndex++)
                        {
                            sb.Append("  ");
                            sb.AppendLine(message.ItemSummaries[summaryIndex] ?? string.Empty);
                        }
                    }

                    sb.AppendLine(new string('-', 90));
                }
            }

            if (info.ToolCalls != null && info.ToolCalls.Count > 0)
            {
                sb.AppendLine(GetResourceText("LastInvocationPreFinalSectionToolCalls", "[ToolCallsAvailableBeforeFinalAnswer]"));
                for (int i = 0; i < info.ToolCalls.Count; i++)
                {
                    DebugToolCall call = info.ToolCalls[i];
                    sb.AppendLine(BuildToolCallMeta(call));
                    sb.AppendLine(GetResourceText("LastInvocationPreFinalSectionToolRequest", "[ToolRequest]"));
                    sb.AppendLine(BuildToolCallRequestPayload(call));
                    sb.AppendLine(GetResourceText("LastInvocationPreFinalSectionToolResponse", "[ToolResponse]"));
                    sb.AppendLine(BuildToolCallResponsePayload(call));
                    sb.AppendLine(new string('-', 90));
                }
            }
            else
            {
                sb.AppendLine(GetResourceText("LastInvocationPreFinalSectionToolCalls", "[ToolCallsAvailableBeforeFinalAnswer]"));
                sb.AppendLine(GetResourceText("LastInvocationPreFinalNoToolCalls", "(keine Toolaufrufe gespeichert)"));
            }

            if (info.SummaryMessagesAfterReduction != null && info.SummaryMessagesAfterReduction.Count > 0)
            {
                sb.AppendLine(GetResourceText("LastInvocationPreFinalSectionSummaries", "[SummariesAfterReduction]"));
                for (int i = 0; i < info.SummaryMessagesAfterReduction.Count; i++)
                {
                    sb.AppendLine(info.SummaryMessagesAfterReduction[i] ?? string.Empty);
                    sb.AppendLine(new string('-', 90));
                }
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Returns the final assistant answer text for the selected invocation.
        /// </summary>
        private static string BuildFinalAssistantResponseText(LastInvocationDebugInfo info)
        {
            if (info == null)
            {
                return GetResourceText("LastInvocationValueEmpty", "(empty)");
            }

            if (!string.IsNullOrWhiteSpace(info.FinalAssistantResponse))
            {
                return info.FinalAssistantResponse;
            }

            return GetResourceText("LastInvocationPreFinalNoFinalResponse", "(keine finale Antwort gespeichert)");
        }

        /// <summary>
        /// Parses captured JSON payloads when possible and falls back to plain text otherwise.
        /// </summary>
        private static JToken ParseJsonTokenOrFallback(string value, bool useEmptyObjectFallback)
        {
            string normalized = value?.Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return useEmptyObjectFallback ? new JObject() : JValue.CreateNull();
            }

            try
            {
                return JToken.Parse(normalized);
            }
            catch
            {
                return new JValue(value);
            }
        }

        /// <summary>
        /// Escapes plain text for safe inclusion in synthetic JSON output.
        /// </summary>
        private static string EscapeJsonString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
        }

        /// <summary>
        /// Copies the currently visible raw request payload to clipboard.
        /// </summary>
        private void CopyRequestJsonButton_Click(object sender, RoutedEventArgs e)
        {
            CopyJsonToClipboard(RawRequestTextBox?.Text);
        }

        /// <summary>
        /// Copies the currently visible raw response payload to clipboard.
        /// </summary>
        private void CopyResponseJsonButton_Click(object sender, RoutedEventArgs e)
        {
            CopyJsonToClipboard(RawResponseTextBox?.Text);
        }

        /// <summary>
        /// Writes normalized payload text to clipboard with failure-safe logging.
        /// </summary>
        private static void CopyJsonToClipboard(string text)
        {
            try
            {
                string payload = NormalizeClipboardPayload(text);
                Clipboard.SetText(payload ?? string.Empty);
            }
            catch (Exception ex)
            {
                Log.Warning($"Copy JSON to clipboard failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Normalizes placeholder-like text to an empty clipboard payload.
        /// </summary>
        private static string NormalizeClipboardPayload(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            string trimmed = text.Trim();
            if (trimmed.StartsWith("(") && trimmed.EndsWith(")"))
            {
                return string.Empty;
            }

            return text;
        }

        /// <summary>
        /// Returns a non-empty display token for optional metadata fields.
        /// </summary>
        private static string SafeText(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? GetResourceText("LastInvocationValueNone", "(none)")
                : value;
        }

        /// <summary>
        /// Builds the AI-facing tool name shown in the tool selector.
        /// </summary>
        private static string BuildAiFacingToolName(DebugToolCall call)
        {
            if (call == null)
            {
                return GetResourceText("LastInvocationValueUnknown", "(unknown)");
            }

            if (!string.IsNullOrWhiteSpace(call.FunctionName))
            {
                return call.FunctionName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(call.ReturnFunctionName))
            {
                return call.ReturnFunctionName.Trim();
            }

            return BuildToolName(call.PluginName, call.FunctionName);
        }

        /// <summary>
        /// Builds a canonical tool name from plugin and function identifiers.
        /// </summary>
        private static string BuildToolName(string plugin, string function)
        {
            plugin ??= string.Empty;
            function ??= string.Empty;

            if (string.IsNullOrWhiteSpace(plugin) && string.IsNullOrWhiteSpace(function))
            {
                return GetResourceText("LastInvocationValueUnknown", "(unknown)");
            }

            if (string.IsNullOrWhiteSpace(plugin))
            {
                return function;
            }

            if (string.IsNullOrWhiteSpace(function))
            {
                return plugin;
            }

            return plugin + "." + function;
        }

        /// <summary>
        /// Copies the currently visible tool-request payload to clipboard.
        /// </summary>
        private void CopyToolRequestButton_Click(object sender, RoutedEventArgs e)
        {
            CopyJsonToClipboard(ToolRequestTextBox?.Text);
        }

        /// <summary>
        /// Copies the currently visible tool-response payload to clipboard.
        /// </summary>
        private void CopyToolResponseButton_Click(object sender, RoutedEventArgs e)
        {
            CopyJsonToClipboard(ToolResponseTextBox?.Text);
        }

        /// <summary>
        /// Copies the currently visible pre-final context to clipboard.
        /// </summary>
        private void CopyPreFinalContextButton_Click(object sender, RoutedEventArgs e)
        {
            CopyJsonToClipboard(PreFinalContextTextBox?.Text);
        }

        /// <summary>
        /// Copies the currently visible final assistant response to clipboard.
        /// </summary>
        private void CopyFinalAssistantResponseButton_Click(object sender, RoutedEventArgs e)
        {
            CopyJsonToClipboard(FinalAssistantResponseTextBox?.Text);
        }

        /// <summary>
        /// Closes the debug window.
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
