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
using CrypTool.CrypLLM.Properties;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

/// <summary>
/// Provides the central state management and telemetry structures for individual AI conversation threads.
/// It bridges Microsoft Semantic Kernel's chat history with CrypTool-specific runtime context (such as active UI tabs)
/// and captures extensive diagnostic snapshots (e.g., token budgets, HTTP exchanges, and tool invocations) for academic and analytical evaluation.
/// </summary>
namespace CrypTool.CrypLLM.Threads
{
    /// <summary>
    /// Represents an isolated conversation environment, maintaining both the semantic chat history 
    /// and CrypTool-specific metadata (like pinned workspaces). It incorporates caching mechanisms 
    /// to manage token limits and preserves a rolling diagnostic history of recent LLM invocations.
    /// </summary>
    public class AIThread
    {
        // Undo handles refer to live native stacks and are intentionally not persisted.
        internal System.Collections.ObjectModel.ObservableCollection<AgentWorkspaceChange> AgentWorkspaceChanges { get; } =
            new System.Collections.ObjectModel.ObservableCollection<AgentWorkspaceChange>();
        private const int MaxInvocationDebugHistoryEntries = 40;
        private readonly List<LastInvocationDebugInfo> _invocationDebugHistory = new List<LastInvocationDebugInfo>();
        private readonly object _toolActivitySync = new object();
        private LastInvocationDebugInfo _lastInvocationDebugInfo;
        private string _activeToolActivitySessionId = string.Empty;
        private List<ToolActivityEntry> _currentToolActivityEntries = new List<ToolActivityEntry>();
        private List<ToolActivityEntry> _lastCompletedToolActivityEntries = new List<ToolActivityEntry>();
        private List<ToolActivityMessageAttachment> _toolActivityMessageAttachments = new List<ToolActivityMessageAttachment>();
        private bool _isToolActivitySessionActive;

        /// <summary>
        /// Id of the thread.
        /// </summary>
        public string Id => AgentThread.Id;

        /// <summary>
        /// Name of the thread.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Tab id that was active when the last user message was sent.
        /// </summary>
        public string LastPinnedWorkspaceTabId { get; set; }

        /// <summary>
        /// Runtime-only debug snapshot for the latest AI invocation.
        /// This is not persisted to disk.
        /// </summary>
        public LastInvocationDebugInfo LastInvocationDebugInfo
        {
            get => _lastInvocationDebugInfo;
            internal set
            {
                _lastInvocationDebugInfo = value;
                if (value == null)
                {
                    return;
                }

                _invocationDebugHistory.Add(value);
                while (_invocationDebugHistory.Count > MaxInvocationDebugHistoryEntries)
                {
                    _invocationDebugHistory.RemoveAt(0);
                }
            }
        }

        public IReadOnlyList<LastInvocationDebugInfo> InvocationDebugHistory => _invocationDebugHistory;

        /// <summary>
        /// Gets the chat history associated with the current agent thread.
        /// </summary>
        public ChatHistory ChatHistory => AgentThread.ChatHistory;

        /// <summary>
        /// Estimates the current prompt history, using the compressed cache plus
        /// subsequent messages rather than the complete archived conversation.
        /// </summary>
        internal int EstimateContextHistoryTokens()
        {
            var fullMessages = ChatHistory?.ToArray() ?? Array.Empty<ChatMessageContent>();
            var cachedMessages = ReducedHistoryCacheMessages;
            int coveredCount = ReducedHistoryCacheSourceMessageCount;
            if (cachedMessages == null || cachedMessages.Count == 0 ||
                coveredCount <= 0 || coveredCount > fullMessages.Length)
            {
                return ChatTokenEstimator.EstimateMessagesTokens(fullMessages);
            }

            int tokens = ChatTokenEstimator.EstimateMessagesTokens(cachedMessages);
            for (int i = coveredCount; i < fullMessages.Length; i++)
            {
                tokens += ChatTokenEstimator.EstimateMessageTokens(fullMessages[i]);
            }
            return tokens;
        }

        // Transport snapshots belong to this conversation, including when the SDK
        // invokes a separate compressed thread or the user selects another chat.
        private readonly object _contextUsageSync = new object();
        private string _contextUsageModel;
        private int? _contextUsageTokens;
        private bool _contextUsageRunning;
        private int _contextUsageHistoryCount;
        private ChatMessageContent _contextUsageLastMessage;

        internal void BeginContextUsage(string modelId)
        {
            lock (_contextUsageSync)
            {
                _contextUsageModel = modelId;
                _contextUsageTokens = null;
                _contextUsageRunning = true;
            }
        }

        internal void UpdateContextUsage(int tokens)
        {
            lock (_contextUsageSync) _contextUsageTokens = tokens;
        }

        internal void CompleteContextUsage()
        {
            lock (_contextUsageSync)
            {
                _contextUsageRunning = false;
                _contextUsageHistoryCount = ChatHistory.Count;
                _contextUsageLastMessage = ChatHistory.LastOrDefault();
            }
        }

        /// <summary>Uses the latest wire prompt while running, with an idle-history fallback.</summary>
        internal int EstimateContextUsageTokens(string modelId, int systemAndToolTokens)
        {
            lock (_contextUsageSync)
            {
                if (_contextUsageTokens.HasValue &&
                    string.Equals(modelId, _contextUsageModel, StringComparison.OrdinalIgnoreCase) &&
                    (_contextUsageRunning || (_contextUsageHistoryCount == ChatHistory.Count &&
                        ReferenceEquals(_contextUsageLastMessage, ChatHistory.LastOrDefault()))))
                    return _contextUsageTokens.Value;
            }
            return EstimateContextHistoryTokens() + systemAndToolTokens;
        }

        /// <summary>
        /// Cached reduced/summarized history used for prompt preparation.
        /// This does not affect the full history shown in the UI.
        /// </summary>
        internal List<ChatMessageContent> ReducedHistoryCacheMessages { get; private set; } = new List<ChatMessageContent>();

        /// <summary>
        /// Number of full-history messages covered by <see cref="ReducedHistoryCacheMessages"/>.
        /// </summary>
        internal int ReducedHistoryCacheSourceMessageCount { get; private set; }

        public ChatHistoryAgentThread AgentThread { get; set; }

        public AIThread(string name = null)
        {
            AgentThread = new ChatHistoryAgentThread();
            Name = name?.Trim();
        }

        internal bool RemoveSyntheticGreetingMessage()
        {
            if (ChatHistory == null || ChatHistory.Count == 0)
            {
                return false;
            }

            if (!IsSyntheticGreetingMessage(ChatHistory[0]))
            {
                return false;
            }

            ChatHistory.RemoveAt(0);
            return true;
        }

        internal static bool IsSyntheticGreetingMessage(ChatMessageContent message)
        {
            if (message?.Role != AuthorRole.Assistant)
            {
                return false;
            }

            string content = (message.Content ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(content))
            {
                return false;
            }

            string englishGreeting = GetLocalizedResourceText("ChatHelloMessage", CultureInfo.InvariantCulture, "Hello! How can I help you today?");
            string germanGreeting = GetLocalizedResourceText("ChatHelloMessage", CultureInfo.GetCultureInfo("de"), "Hallo! Wie kann ich dir heute helfen?");
            return string.Equals(content, englishGreeting, StringComparison.Ordinal) ||
                   string.Equals(content, germanGreeting, StringComparison.Ordinal);
        }

        /// <summary>
        /// Updates the local cache with a compressed or truncated version of the conversation history.
        /// This mechanism allows the AI to stay within model token limits without losing the full, human-readable UI history.
        /// </summary>
        /// <param name="messages">The reduced subset of messages to dispatch to the model.</param>
        /// <param name="sourceMessageCount">The number of original messages that were factored into this reduction.</param>
        internal void SetReducedHistoryCache(IEnumerable<ChatMessageContent> messages, int sourceMessageCount)
        {
            ReducedHistoryCacheMessages = (messages ?? Enumerable.Empty<ChatMessageContent>())
                .Select(m => new ChatMessageContent(m.Role, m.Content ?? string.Empty))
                .ToList();
            ReducedHistoryCacheSourceMessageCount = Math.Max(0, sourceMessageCount);
        }

        /// <summary>
        /// Invalidates the reduced history cache, forcing a strict recalculation of the token budget on the next invocation.
        /// </summary>
        internal void ClearReducedHistoryCache()
        {
            lock (_contextUsageSync) _contextUsageTokens = null;
            ReducedHistoryCacheMessages = new List<ChatMessageContent>();
            ReducedHistoryCacheSourceMessageCount = 0;
        }

        internal void BeginToolActivitySession(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return;
            }

            lock (_toolActivitySync)
            {
                _activeToolActivitySessionId = sessionId.Trim();
                _currentToolActivityEntries = new List<ToolActivityEntry>();
                _lastCompletedToolActivityEntries = new List<ToolActivityEntry>();
                _isToolActivitySessionActive = true;
            }
        }

        internal void RecordToolActivityStarted(
            string sessionId,
            string callId,
            string pluginName,
            string functionName,
            string arguments)
        {
            lock (_toolActivitySync)
            {
                if (!IsMatchingToolActivitySession(sessionId))
                {
                    return;
                }

                ToolActivityEntry entry = FindRunningToolActivityEntry(callId, pluginName, functionName);
                if (entry == null)
                {
                    entry = CreateToolActivityEntry(callId, pluginName, functionName, arguments);
                    _currentToolActivityEntries.Add(entry);
                    return;
                }

                entry.PluginName = pluginName ?? entry.PluginName ?? string.Empty;
                entry.FunctionName = functionName ?? entry.FunctionName ?? string.Empty;
                entry.Arguments = string.IsNullOrWhiteSpace(arguments) ? entry.Arguments ?? string.Empty : arguments;
                entry.Status = ToolActivityStatus.Running;
                if (entry.StartedAtLocal == default)
                {
                    entry.StartedAtLocal = DateTime.Now;
                }
            }
        }

        internal void RecordToolActivityFinished(
            string sessionId,
            string callId,
            string pluginName,
            string functionName,
            ToolActivityStatus status,
            string resultText)
        {
            lock (_toolActivitySync)
            {
                if (!IsMatchingToolActivitySession(sessionId))
                {
                    return;
                }

                ToolActivityEntry entry = FindRunningToolActivityEntry(callId, pluginName, functionName);
                if (entry == null)
                {
                    entry = CreateToolActivityEntry(callId, pluginName, functionName, string.Empty);
                    _currentToolActivityEntries.Add(entry);
                }

                entry.PluginName = pluginName ?? entry.PluginName ?? string.Empty;
                entry.FunctionName = functionName ?? entry.FunctionName ?? string.Empty;
                entry.Status = status;
                entry.ResultText = resultText ?? string.Empty;
                entry.CompletedAtLocal = DateTime.Now;
            }
        }

        internal void ApplyToolActivityArguments(IEnumerable<ToolActivityArgumentInfo> argumentInfos)
        {
            List<ToolActivityArgumentInfo> infos = (argumentInfos ?? Enumerable.Empty<ToolActivityArgumentInfo>())
                .Where(info => info != null && !string.IsNullOrWhiteSpace(info.Arguments))
                .ToList();

            if (infos.Count == 0)
            {
                return;
            }

            lock (_toolActivitySync)
            {
                ApplyToolActivityArgumentsToEntries(_currentToolActivityEntries, infos);
                ApplyToolActivityArgumentsToEntries(_lastCompletedToolActivityEntries, infos);
            }
        }

        internal void FinalizeToolActivitySession(string sessionId, ToolActivitySessionCompletion completion)
        {
            lock (_toolActivitySync)
            {
                if (!IsMatchingToolActivitySession(sessionId))
                {
                    return;
                }

                ToolActivityStatus unfinishedStatus = completion == ToolActivitySessionCompletion.Cancelled
                    ? ToolActivityStatus.Cancelled
                    : ToolActivityStatus.Incomplete;

                foreach (ToolActivityEntry entry in _currentToolActivityEntries.Where(item => item.Status == ToolActivityStatus.Running))
                {
                    entry.Status = unfinishedStatus;
                    entry.ResultText = completion == ToolActivitySessionCompletion.Cancelled
                        ? GetResourceTextOrFallback("AiChatToolActivityCancelledMessage", "Tool execution was cancelled.")
                        : GetResourceTextOrFallback("AiChatToolActivityIncompleteMessage", "Tool execution did not complete.");
                    entry.CompletedAtLocal = DateTime.Now;
                }

                _lastCompletedToolActivityEntries = _currentToolActivityEntries
                    .Select(entry => entry.Clone())
                    .ToList();

                _isToolActivitySessionActive = false;
                _activeToolActivitySessionId = string.Empty;
            }
        }

        internal bool IsToolActivitySessionActive
        {
            get
            {
                lock (_toolActivitySync)
                {
                    return _isToolActivitySessionActive;
                }
            }
        }

        internal List<ToolActivityEntry> GetVisibleToolActivitySnapshot()
        {
            lock (_toolActivitySync)
            {
                List<ToolActivityEntry> source = _isToolActivitySessionActive
                    ? _currentToolActivityEntries
                    : _lastCompletedToolActivityEntries;

                return source
                    .Select(entry => entry.Clone())
                    .ToList();
            }
        }

        internal void ClearVisibleToolActivitySnapshot()
        {
            lock (_toolActivitySync)
            {
                if (_isToolActivitySessionActive)
                {
                    _currentToolActivityEntries = new List<ToolActivityEntry>();
                    return;
                }

                _lastCompletedToolActivityEntries = new List<ToolActivityEntry>();
            }
        }

        internal void AttachLastCompletedToolActivitiesToAssistantMessage(int assistantMessageOrdinal)
        {
            if (assistantMessageOrdinal <= 0)
            {
                return;
            }

            lock (_toolActivitySync)
            {
                if (_lastCompletedToolActivityEntries == null || _lastCompletedToolActivityEntries.Count == 0)
                {
                    return;
                }

                _toolActivityMessageAttachments.RemoveAll(item => item.AssistantMessageOrdinal == assistantMessageOrdinal);
                _toolActivityMessageAttachments.Add(new ToolActivityMessageAttachment
                {
                    AssistantMessageOrdinal = assistantMessageOrdinal,
                    Entries = _lastCompletedToolActivityEntries
                        .Select(entry => entry.Clone())
                        .ToList()
                });
            }
        }

        internal List<ToolActivityMessageAttachment> GetToolActivityMessageAttachmentsSnapshot()
        {
            lock (_toolActivitySync)
            {
                return _toolActivityMessageAttachments
                    .Select(item => item.Clone())
                    .ToList();
            }
        }

        internal void SetToolActivityMessageAttachments(IEnumerable<ToolActivityMessageAttachment> attachments)
        {
            lock (_toolActivitySync)
            {
                _toolActivityMessageAttachments = (attachments ?? Enumerable.Empty<ToolActivityMessageAttachment>())
                    .Where(item => item != null && item.AssistantMessageOrdinal > 0 && item.Entries != null && item.Entries.Count > 0)
                    .Select(item => item.Clone())
                    .ToList();
            }
        }

        private bool IsMatchingToolActivitySession(string sessionId)
        {
            return _isToolActivitySessionActive &&
                   !string.IsNullOrWhiteSpace(sessionId) &&
                   string.Equals(_activeToolActivitySessionId, sessionId.Trim(), StringComparison.Ordinal);
        }

        private static void ApplyToolActivityArgumentsToEntries(
            List<ToolActivityEntry> entries,
            List<ToolActivityArgumentInfo> infos)
        {
            if (entries == null || entries.Count == 0 || infos == null || infos.Count == 0)
            {
                return;
            }

            Dictionary<string, Queue<ToolActivityArgumentInfo>> blankCallIdInfosByFunction =
                infos
                    .Where(info => string.IsNullOrWhiteSpace(info.CallId) && !string.IsNullOrWhiteSpace(info.FunctionName))
                    .GroupBy(info => info.FunctionName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        group => group.Key,
                        group => new Queue<ToolActivityArgumentInfo>(group),
                        StringComparer.OrdinalIgnoreCase);

            foreach (ToolActivityEntry entry in entries)
            {
                if (entry == null)
                {
                    continue;
                }

                string normalizedCallId = (entry.CallId ?? string.Empty).Trim();
                ToolActivityArgumentInfo match = null;
                if (!string.IsNullOrWhiteSpace(normalizedCallId))
                {
                    match = infos.FirstOrDefault(info =>
                        string.Equals((info.CallId ?? string.Empty).Trim(), normalizedCallId, StringComparison.OrdinalIgnoreCase));
                }

                if (match == null)
                {
                    string functionName = entry.FunctionName ?? string.Empty;
                    if (blankCallIdInfosByFunction.TryGetValue(functionName, out Queue<ToolActivityArgumentInfo> queuedInfos) &&
                        queuedInfos.Count > 0)
                    {
                        match = queuedInfos.Dequeue();
                    }
                }

                if (match != null && !string.IsNullOrWhiteSpace(match.Arguments))
                {
                    entry.Arguments = match.Arguments;
                }
            }
        }

        private ToolActivityEntry CreateToolActivityEntry(
            string callId,
            string pluginName,
            string functionName,
            string arguments)
        {
            return new ToolActivityEntry
            {
                Sequence = _currentToolActivityEntries.Count + 1,
                CallId = (callId ?? string.Empty).Trim(),
                PluginName = pluginName ?? string.Empty,
                FunctionName = functionName ?? string.Empty,
                Arguments = arguments ?? string.Empty,
                Status = ToolActivityStatus.Running,
                StartedAtLocal = DateTime.Now
            };
        }

        private ToolActivityEntry FindRunningToolActivityEntry(
            string callId,
            string pluginName,
            string functionName)
        {
            string normalizedCallId = (callId ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(normalizedCallId))
            {
                return _currentToolActivityEntries.LastOrDefault(entry =>
                    string.Equals(entry.CallId ?? string.Empty, normalizedCallId, StringComparison.OrdinalIgnoreCase) &&
                    entry.Status == ToolActivityStatus.Running);
            }

            return _currentToolActivityEntries.LastOrDefault(entry =>
                string.IsNullOrWhiteSpace(entry.CallId) &&
                string.Equals(entry.PluginName ?? string.Empty, pluginName ?? string.Empty, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(entry.FunctionName ?? string.Empty, functionName ?? string.Empty, StringComparison.OrdinalIgnoreCase) &&
                entry.Status == ToolActivityStatus.Running);
        }

        private static string GetResourceTextOrFallback(string resourceKey, string fallback)
        {
            string value = Resources.ResourceManager.GetString(resourceKey);
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static string GetLocalizedResourceText(string resourceKey, CultureInfo culture, string fallback)
        {
            string value = Resources.ResourceManager.GetString(resourceKey, culture);
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

    }

    internal enum ToolActivityStatus
    {
        Running,
        Succeeded,
        Failed,
        Denied,
        Cancelled,
        Incomplete
    }

    internal enum ToolActivitySessionCompletion
    {
        Succeeded,
        Failed,
        Cancelled
    }

    internal sealed class ToolActivityEntry
    {
        public int Sequence { get; set; }
        public string CallId { get; set; } = string.Empty;
        public string PluginName { get; set; } = string.Empty;
        public string FunctionName { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty;
        public ToolActivityStatus Status { get; set; }
        public string ResultText { get; set; } = string.Empty;
        public DateTime StartedAtLocal { get; set; }
        public DateTime CompletedAtLocal { get; set; }

        public ToolActivityEntry Clone()
        {
            return new ToolActivityEntry
            {
                Sequence = Sequence,
                CallId = CallId ?? string.Empty,
                PluginName = PluginName ?? string.Empty,
                FunctionName = FunctionName ?? string.Empty,
                Arguments = Arguments ?? string.Empty,
                Status = Status,
                ResultText = ResultText ?? string.Empty,
                StartedAtLocal = StartedAtLocal,
                CompletedAtLocal = CompletedAtLocal
            };
        }
    }

    internal sealed class ToolActivityMessageAttachment
    {
        public int AssistantMessageOrdinal { get; set; }
        public List<ToolActivityEntry> Entries { get; set; } = new List<ToolActivityEntry>();

        public ToolActivityMessageAttachment Clone()
        {
            return new ToolActivityMessageAttachment
            {
                AssistantMessageOrdinal = AssistantMessageOrdinal,
                Entries = (Entries ?? new List<ToolActivityEntry>())
                    .Select(entry => entry?.Clone())
                    .Where(entry => entry != null)
                    .ToList()
            };
        }
    }

    internal sealed class ToolActivityArgumentInfo
    {
        public string CallId { get; set; } = string.Empty;
        public string FunctionName { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty;
    }

    /// <summary>
    /// A diagnostic data transfer object capturing a comprehensive snapshot of a single AI transaction.
    /// Used heavily for telemetry, UI inspection, and evaluating the agent's behavior and context window utilization.
    /// </summary>
    public sealed class LastInvocationDebugInfo
    {
        public DateTime CreatedAtLocal { get; set; }
        public string ModelId { get; set; }
        public string ActiveTabId { get; set; }
        public bool UsedReducedThread { get; set; }
        public string UserInput { get; set; }
        public string FinalAssistantResponse { get; set; } = string.Empty;
        public string RawLlmRequest { get; set; } = string.Empty;
        public string RawLlmResponse { get; set; } = string.Empty;
        public PromptBudgetSnapshot Budget { get; set; } = new PromptBudgetSnapshot();
        public List<DebugPromptMessage> PromptMessages { get; } = new List<DebugPromptMessage>();
        public List<DebugToolCall> ToolCalls { get; } = new List<DebugToolCall>();
        public List<DebugHttpExchange> HttpExchanges { get; } = new List<DebugHttpExchange>();
        public List<DebugTextReduction> TextReductions { get; } = new List<DebugTextReduction>();
        public List<string> SummaryMessagesBeforeReduction { get; } = new List<string>();
        public List<string> SummaryMessagesAfterReduction { get; } = new List<string>();
    }

    /// <summary>
    /// Details the deterministic token allocation strategy per LLM invocation. 
    /// Ensures that requests do not exceed the model's physical constraints by reserving margins for responses and system instructions.
    /// </summary>
    public sealed class PromptBudgetSnapshot
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
        public int PreflightPlannedTokens { get; set; }
        public bool PreflightPassed { get; set; }
    }

    /// <summary>
    /// Tracks individual message payloads injected into the final prompt stream.
    /// </summary>
    public sealed class DebugPromptMessage
    {
        public int Index { get; set; }
        public string Role { get; set; }
        public string Content { get; set; }
        public List<string> ItemSummaries { get; } = new List<string>();
    }

    /// <summary>
    /// Represents an intercepted tool (function) invocation made by the LLM, including arguments and synchronous callback results.
    /// </summary>
    public sealed class DebugToolCall
    {
        public int Sequence { get; set; }
        public string CallId { get; set; }
        public string PluginName { get; set; }
        public string FunctionName { get; set; }
        public string Arguments { get; set; }
        public bool HasReturn { get; set; }
        public string ReturnCallId { get; set; }
        public string ReturnFunctionName { get; set; }
        public string ReturnValue { get; set; }
    }

    /// <summary>
    /// Captures raw, low-level HTTP interaction traces between the native application and the external LLM provider endpoint.
    /// Useful for latency analysis and error attribution.
    /// </summary>
    public sealed class DebugHttpExchange
    {
        public int Sequence { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Uri { get; set; } = string.Empty;
        public string RequestHeaders { get; set; } = string.Empty;
        public string RequestBody { get; set; } = string.Empty;
        public int? StatusCode { get; set; }
        public string ReasonPhrase { get; set; } = string.Empty;
        public string ResponseHeaders { get; set; } = string.Empty;
        public string ResponseBody { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }

    /// <summary>
    /// Documents instances where text was actively truncated or summarized to comply with context window limits.
    /// Allows analyzing the fidelity loss experienced during token constraint execution.
    /// </summary>
    public sealed class DebugTextReduction
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
