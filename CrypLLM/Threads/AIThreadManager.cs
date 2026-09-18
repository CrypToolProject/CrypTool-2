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
/// This module encapsulates the orchestration of Large Language Model (LLM) agents and their associated conversation threads.
/// It manages the lifecycle of Microsoft Semantic Kernel instances, API connections, context window limits,
/// dynamic tool registration, and telemetry capturing, which is essential for systematic evaluation and debugging.
/// </summary>
using CrypTool.CrypLLM.AgentInstructions;
using CrypTool.CrypLLM.Converter;
using CrypTool.CrypLLM.Helper;
using CrypTool.CrypLLM.Ports;
using CrypTool.CrypLLM.Properties;
using CrypTool.CrypLLM.Services;
using CrypTool.PluginBase.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrypTool.CrypLLM.Threads
{
    /// <summary>
    /// A singleton orchestrator responsible for managing active AI agents and coordinating chat threads.
    /// Brokers requests between the user interface and the underlying LLM provider, enforces strict token budgets, 
    /// persists historical conversations securely, and records low-level HTTP diagnostics.
    /// </summary>
    internal sealed class AIThreadManager
    {
        #region private Fields

        private const string ContextReductionErrorCode = "CTX-REDUCE-001";
        private const string ContextInvokeErrorCode = "CTX-INVOKE-001";
        internal const string DefaultLocalOpenAiCompatibleEndpoint = "http://127.0.0.1:1234/v1";

        /// <summary>
        /// Construction preferences appended at runtime so existing user instruction
        /// profiles also receive the current readability and wiring requirements.
        /// Dimensions are starting points; actual content and visual bounds govern the final layout.
        /// </summary>
        internal const string WorkspaceConstructionInstructions =
            "\n\nWORKSPACE CONSTRUCTION AND FINAL QUALITY CHECK:\n" +
            "Apply these rules whenever you build or change a workspace, respecting the user's explicit choices and available tool permissions. " +
            "TEMPLATE CHOICE BEFORE BUILDING: For a new task, search available templates using tpl_search with concise algorithm/task keywords (try synonyms), then inspect promising matches with tpl_info and, if needed, tpl_model. " +
            "If an existing template solves the requested problem, name the actual template and explain why it fits, then ASK the user whether to open that template or build the workspace yourself. " +
            "Stop and wait for the user's reply BEFORE tpl_open or any workspace construction for that task. Do not silently choose either option. " +
            "If the user already explicitly chose a template or explicitly chose building from scratch, honor that choice without asking again. " +
            "If no suitable template is found or discovery is unavailable, build normally and state that briefly. Small edits to an existing workspace do not require a template choice. " +
            "1. Prefer connected, visible input components for data, keys, IVs, alphabets and other parameters whenever a matching input connector exists. " +
            "Inspect the component catalog/schema for real connector names and data types before wiring. " +
            "Use TextInput for text. For numeric input connectors, prefer a visible NumberInput (number input) with a compatible numeric output type. " +
            "For Caesar, the key input is ShiftKey (an integer): connect a visible NumberInput containing e.g. 3 to ShiftKey. " +
            "Use TextInput with supported conversion or an appropriate conversion component for numeric inputs only if a compatible NumberInput is unavailable or the user explicitly requests it. " +
            "Never invent connector names or connect a key to the plaintext/alphabet input. " +
            "Use internal settings only when no appropriate connector exists, a connection is unsupported, or the user explicitly requests internal configuration. " +
            "Verify the connection actually succeeded and, when execution is available, verify that the external value affects the result. " +
            "2. TextInput and TextOutput boxes must be comfortably readable, not tiny icons. " +
            "Start around 400 by 220 canvas units for short text and enlarge for longer/multiline input or output. " +
            "Use ws_resize_component and its reported actual sizes; account for titles, controls and padding. " +
            "3. Every memo must display its complete text without scrolling or clipping. " +
            "After ws_add_memo and after every text change, call ws_fit_memo to size it from its actual formatted contents and check contentFits. " +
            "If fitting is constrained or unverified, inspect and use ws_resize_memo/another width to resolve it. " +
            "Start around 600 units wide and estimate height from ALL wrapped lines and paragraphs at a readable font, allowing at least 24 units per line plus 48 units padding; " +
            "increase the height generously when uncertain. Do not use a fixed short height for a long memo. " +
            "Do not remove requested content or reduce readability to make text fit. Resize first and place the memo outside component and wire corridors. " +
            "4. Components MUST NEVER overlap. Memos must not overlap components or one another. " +
            "Read ws_bounds and plan with occupiedBounds, which include connector rails and captions, and bodyBounds, which locate the actual box. " +
            "The top-level x/y are the MOVE anchor and width/height are the resizable inner window; the actual body is offset from that anchor. " +
            "Do not combine the anchor with the inner window size and assume this describes the whole visible rectangle. " +
            "Leave at least 40 canvas units clear space between boxes, preferably 80-120 for wiring. " +
            "After ANY resize, recalculate downstream positions because enlarged boxes can invalidate earlier spacing. " +
            "Check every relevant pair of rectangles: one box's right edge plus the gap must be left of the other's left edge, " +
            "or its bottom edge plus the gap must be above the other's top edge (or the reverse). " +
            "Resolve every overlap involving created or modified elements using move/resize tools before finishing. " +
            "5. Arrange the main data flow left to right. Put key/parameter inputs in separate rows near their target components. " +
            "Keep wire corridors clear of boxes and memos. Avoid shared/overlaid wire segments and crossings wherever possible. " +
            "Use ws_set_connector_orientation to face connected sides toward one another: East/West for horizontal flow, North/South for vertical parameter feeds. " +
            "Plan each directed connection from its source to its receiver, including output display branches and shared key inputs. " +
            "For East-output to West-input routing, keep the ENTIRE source occupiedBounds left of the receiver occupiedBounds with a clear corridor; matching box centers is insufficient. " +
            "Do not place a wide TextOutput directly beneath its source with a West input if this makes the return wire run through the source or the output box. " +
            "Either move that output sufficiently to the right of its source, or use a South output and North input for a receiver below. " +
            "For a key input above its cipher use South-output/North-input; for an input below use North-output/South-input. " +
            "For right-to-left feeds use West-output/East-input. Never point a port through its own box to reach the other component. " +
            "Changing the side of one output affects ALL its fan-out connections. When it also feeds the next cipher in the main row, " +
            "keep the main East/West flow and move branch receivers into clear side corridors or change the branch input side appropriately, then inspect every branch again. " +
            "A wire may touch its actual connector at the box boundary but MUST NOT run through ANY component or memo interior, including its own source and target. " +
            "Treat wire_through_element as an error. Use its affected IDs, endpoint connector names and routingAdvice to move boxes or change connector sides. " +
            "connector_sides_need_detour is conditional advice: choose positions or connector sides that clear the real route rather than blindly changing a shared port for one branch. " +
            "Align endpoints rather than just box centers, stagger input positions where necessary, and increase spacing to separate routes. " +
            "Preserve existing wiring; do not remove required connections just to simplify the drawing. " +
            "6. You MUST actually inspect the result before reporting completion; planning a check or describing an intended layout does not count. " +
            "Tool edits return automaticLayoutCheck findings: treat these as real defects to fix, not merely informational logs. " +
            "Call ws_check_layout, ws_model, ws_bounds and ws_validate after your last edits; resolve all layout errors involving your changes before finishing. " +
            "Do not say 'no overlaps' while a wire_through_element or element_overlap remains. Re-run the layout check after each correction. " +
            "Minimize reported wire crossings/overlaid segments and spacing warnings; if unavoidable explain the specific reason. " +
            "A passed report with complete=false does not verify unavailable memo or wire measurements. " +
            "Verify actual connections, values, sizes and non-overlapping rectangles. " +
            "For a runnable workflow, unless the user asked not to run it, use ws_run when permitted, wait for the result as needed, " +
            "then call ws_io and inspect the actual output and relevant errors (app_log_recent if needed). " +
            "Compare outputs against the requested task and a known expected result when available; successful tool calls or nonempty output alone do not prove correctness. " +
            "With a vision-capable model and ws_screenshot available, taking and EXAMINING a screenshot of the final layout is REQUIRED, not optional. " +
            "Inspect the returned image itself, not just the screenshot tool's metadata: all memo text and text IO must be visible, boxes separated, " +
            "connector arrows correct, and wire routes as clear as possible. The screenshot shows only the visible viewport; do not claim to have inspected offscreen elements. " +
            "Do not assume you lack vision merely because the tool result is textual; the screenshot tool supplies an image in the next model request. " +
            "When inspection reveals a problem, correct it, re-read the bounds/IO as appropriate and take a NEW screenshot after the correction. " +
            "An earlier screenshot does not verify later edits. Only finish once the final state has been checked, and briefly state what you actually verified. " +
            "Without vision, use conservative sizes and rectangle checks and do not claim that text visibility or wire crossings have been visually verified. " +
            "If a required tool is unavailable or a route cannot be made clean, state the specific limitation rather than claiming the layout is finished.";

        private static readonly Lazy<AIThreadManager> _instance = new(() => new AIThreadManager());
        private static readonly HttpClient InstrumentedOpenAiHttpClient = CreateInstrumentedOpenAiHttpClient();
        private static readonly AsyncLocal<LlmHttpCaptureSession> CurrentLlmHttpCapture = new AsyncLocal<LlmHttpCaptureSession>();
        private static readonly AsyncLocal<ToolActivitySessionState> CurrentToolActivitySession = new AsyncLocal<ToolActivitySessionState>();
        private AIThread _activeThread;
        private readonly string _historyPath;
        private string _cachedOpenTabsSnapshotJson;
        private PromptBudgetProfile _promptBudgetProfile;
        private readonly ChatHistoryReductionEngine _historyReductionEngine = new ChatHistoryReductionEngine();
        private int _activeThreadIndex;
        public bool AgentsPropertysChanged;

        #endregion

        public static AIThreadManager Instance => _instance.Value;

        public ChatCompletionAgent Agent { get; set; }

        public List<AIThread> AIThreads { get; } = new();

        public string LastPinnedWorkspaceTabId { get; private set; }

        public AIThread ActiveThread
        {
            get => _activeThread;
            set
            {
                if (_activeThread != value)
                {
                    _activeThread = value;
                    _activeThreadIndex = Math.Max(0, AIThreads.IndexOf(_activeThread));
                    ActiveThreadChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler ThreadListChanged;
        public event EventHandler ActiveThreadChanged;
        public event EventHandler NewChatMessageReceived;
        public event EventHandler ToolActivityChanged;

        /// <summary>
        /// Applies shipped model-default migrations so blank or outdated profiles pick up the current
        /// default API/local model lists and local endpoint.
        /// </summary>
        internal static void ApplyLocalProviderDefaultsMigration()
        {
            string[] defaultOpenAiModelIds =
            {
                "gpt-5.4-mini",
                "gpt-5.4-nano",
                "gpt-5.5",
                "gpt-5.4"
            };

            string[] defaultLocalModelIds =
            {
                "openai/gpt-oss-20b",
                "openai/gpt-oss-120b",
                "qwen/qwen3.6-27b",
                "qwen/qwen3.6-35b-a3b"
            };

            string[] legacyLocalModelIds =
            {
                "qwen/qwen3.6-35b-a3b",
                "openai/gpt-oss-20b"
            };

            bool changed = false;
            string[] configuredOpenAiModelIds = SplitStoredModelIds(Settings.Default.openAiModelIds);
            if (configuredOpenAiModelIds.Length == 0)
            {
                Settings.Default.openAiModelIds = string.Join(Environment.NewLine, defaultOpenAiModelIds);
                changed = true;
            }

            string[] configuredLocalModelIds = SplitStoredModelIds(Settings.Default.localModelIds);
            if (configuredLocalModelIds.Length == 0)
            {
                Settings.Default.localModelIds = string.Join(Environment.NewLine, defaultLocalModelIds);
                changed = true;
            }
            else if (configuredLocalModelIds.Length == 1 &&
                     string.Equals(configuredLocalModelIds[0], legacyLocalModelIds[1], StringComparison.OrdinalIgnoreCase))
            {
                Settings.Default.localModelIds = string.Join(Environment.NewLine, defaultLocalModelIds);
                changed = true;
            }
            else if (configuredLocalModelIds.Length == legacyLocalModelIds.Length &&
                     configuredLocalModelIds.SequenceEqual(legacyLocalModelIds, StringComparer.OrdinalIgnoreCase))
            {
                Settings.Default.localModelIds = string.Join(Environment.NewLine, defaultLocalModelIds);
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(Settings.Default.localModelEndpoint))
            {
                Settings.Default.localModelEndpoint = DefaultLocalOpenAiCompatibleEndpoint;
                changed = true;
            }

            if (changed)
            {
                Settings.Default.Save();
            }
        }

        /// <summary>
        /// Initializes the AIThreadManager. Establishes the safe local storage directory for historical data
        /// and automatically attempts to recover previously persisted state to ensure continuity across application sessions.
        /// </summary>
        public AIThreadManager()
        {
            string dir = Path.Combine(DirectoryHelper.DirectoryLocal, "LLM");
            Directory.CreateDirectory(dir);
            _historyPath = Path.Combine(dir, "AIThreads.history.json");
            Log.Info($"AIThreadManager initialized. HistoryPath='{_historyPath}'.");

            LoadThreadHistoriesSafe();
            CreateNewThread();
        }

        /// <summary>
        /// Attempts to extract the active model identifier directly from the initialized Semantic Kernel capabilities.
        /// Crucial for validating correct alignment between UI selections and the underlying agent instance.
        /// </summary>
        public string GetAgentModelId()
        {
            try
            {
                var service = Agent?.Kernel?.Services?.GetService(typeof(IChatCompletionService)) as IChatCompletionService;
                if (service is OpenAIChatCompletionService openAi)
                {
                    return openAi.AsChatClient().GetModelId() ?? string.Empty;
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Log.Warning($"GetAgentModelId failed: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Promotes the given thread to the active foreground, triggering UI and context re-evaluations.
        /// </summary>
        public void SetActiveThread(string threadId)
        {
            if (string.IsNullOrWhiteSpace(threadId))
            {
                return;
            }
            AIThread thread = AIThreads.Find(t => t.Id == threadId);
            if (thread != null)
            {
                ActiveThread = thread;
            }
        }

        /// <summary>
        /// Destroys the currently active thread and its history, rolling over to the nearest adjacent thread
        /// or spawning a fresh context if the user's workspace array runs completely empty.
        /// </summary>
        public void DeleteActiveThread()
        {
            if (ActiveThread == null)
            {
                return;
            }

            int index = AIThreads.IndexOf(ActiveThread);
            if (AIThreads.Remove(ActiveThread))
            {
                OnThreadListChanged();
            }

            if (AIThreads.Count > 0)
            {
                ActiveThread = AIThreads[Math.Min(index, AIThreads.Count - 1)];
            }
            else
            {
                ActiveThread = CreateNewThread();
            }

            SaveThreadHistoriesSafe();
        }

        /// <summary>
        /// Wipes the entire local conversation history, both from RAM and disk arrays.
        /// Acts as a hard reset for privacy guarantees and error recovery.
        /// </summary>
        public void ClearAllThreads()
        {
            try
            {
                AIThreads.Clear();
                ActiveThread = null;

                if (File.Exists(_historyPath))
                {
                    File.Delete(_historyPath);
                }

                CreateNewThread();
                SaveThreadHistoriesSafe();
            }
            catch (Exception ex)
            {
                Log.Warning($"ClearAllThreads failed: {ex.Message}");
            }
        }

        public void ReloadAgent(string modelId = null)
        {
            try
            {
                SupportedProviders supportedProvider = ProviderToStringConverter.Convert(Settings.Default.selectedProvider);
                string selectedModel = string.IsNullOrWhiteSpace(modelId) ? Settings.Default.activeModelId : modelId;
                CreateNewAgent(supportedProvider, selectedModel);
                AgentsPropertysChanged = false;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to reload agent: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Dynamically establishes a Semantic Kernel instance tailored to the configured LLM provider (e.g., Local vs. OpenAI API).
        /// Reevaluates plugin registration and filters based on the specific context window parameters of the target model.
        /// </summary>
        /// <param name="provider">The designated service backend supplying inference capabilities.</param>
        /// <param name="modelId">Specific model identifier used to accurately compute bounds and budget reserves.</param>
        private void CreateNewAgent(SupportedProviders provider, string modelId)
        {
            if (string.IsNullOrWhiteSpace(modelId))
            {
                throw new ArgumentException("Model id must be provided.", nameof(modelId));
            }
            if (provider != SupportedProviders.Local && provider != SupportedProviders.OpenAI)
            {
                throw new NotSupportedException($"The provider '{provider}' is not supported.");
            }

            IKernelBuilder builder = Kernel.CreateBuilder();
            ConfigureKernelLogging(builder);

            if (provider == SupportedProviders.Local)
            {
                Uri endpoint = GetConfiguredLocalEndpointUri();
                // Local OpenAI-compatible servers may require their own Bearer token.
                // Keep the existing placeholder for servers that do not require authentication.
                string apiKey = SecretProtector.TryDecryptOrReturn(Settings.Default.localApiKey);
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    apiKey = "no-api-key";
                }
                builder.AddOpenAIChatCompletion(modelId, endpoint, apiKey, null, null, InstrumentedOpenAiHttpClient);
            }
            else if (provider == SupportedProviders.OpenAI)
            {
                string encryptedApiKey = Settings.Default.apiKey;
                string apiKey = encryptedApiKey == null ? "no-api-key" : SecretProtector.TryDecryptOrReturn(encryptedApiKey);
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new InvalidOperationException("OpenAI API key is missing or invalid.");
                }
                string orgId = Settings.Default.orgId;
                builder.AddOpenAIChatCompletion(modelId, apiKey, orgId, null, InstrumentedOpenAiHttpClient);
            }

            Kernel kernel = builder.Build();
            bool hasResolvedContextWindowTokens = ModelContextWindowResolver.TryResolveContextWindowTokens(modelId, out int resolvedContextTokens);
            List<string> registeredPlugins = new List<string>(5);

            RegisterPlugin<WorkspaceStatusPlugin>(
                kernel,
                "CrypLLM_WorkspaceStatus",
                hasResolvedContextWindowTokens,
                resolvedContextTokens,
                registeredPlugins);
            RegisterPlugin<ComponentCatalogPlugin>(
                kernel,
                "CrypLLM_ComponentCatalog",
                hasResolvedContextWindowTokens,
                resolvedContextTokens,
                registeredPlugins);
            RegisterPlugin<TemplateCatalogPlugin>(
                kernel,
                "CrypLLM_TemplateCatalog",
                hasResolvedContextWindowTokens,
                resolvedContextTokens,
                registeredPlugins);
            RegisterPlugin<UiTextCatalogPlugin>(
                kernel,
                "CrypLLM_UiTextCatalog",
                hasResolvedContextWindowTokens,
                resolvedContextTokens,
                registeredPlugins);
            RegisterPlugin<WorkspaceEditingPlugin>(
                kernel,
                "CrypLLM_WorkspaceEditing",
                hasResolvedContextWindowTokens,
                resolvedContextTokens,
                registeredPlugins);

            // Filters are split into dedicated files to keep this manager focused on orchestration.
            kernel.AutoFunctionInvocationFilters.Add(new ToolPermissionFilter());

            string instructions = InstructionManager.Instance.GetInstructionText("Default");
            instructions += AssistantResponseText.ResponseInstructions;
            instructions += WorkspaceConstructionInstructions;
            if (kernel.Plugins.Any(plugin => plugin.Any(function => function.Metadata.Name == "ws_resize_component")))
            {
                instructions += "\n\nWORKSPACE LAYOUT:\n" +
                    "Use ws_resize_component and ws_resize_memo to change physical workspace box sizes. " +
                    "Use ws_set_memo_text only to change memo content. Inspect ws_bounds and, with a vision model, ws_screenshot before and after layout edits. " +
                    "Stored width/height of zero indicates automatic sizing; use visible bounds and sizeSource. " +
                    "Do not delete and recreate components, change their text, or rename them merely to resize their boxes. " +
                    "Respect reported minimum/maximum sizes and actual resize results. If a tool fails or is denied, explain that result rather than claiming all CT2 component sizes are fixed.";
            }

            if (kernel.Plugins.Any(plugin => plugin.Any(function => function.Metadata.Name == "ws_set_connector_orientation")))
            {
                instructions += "\nUse ws_set_connector_orientation(componentId, connectorName, orientation) to move existing connectors between North, South, East and West. " +
                    "Read technical connector Name and current Orientation from ws_model. Moving a connector's side does not change input/output direction; keep existing wiring intact.";
            }

            OpenAIPromptExecutionSettings settings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            bool supportsExplicitReasoningEffort = provider == SupportedProviders.OpenAI;
            if (supportsExplicitReasoningEffort &&
                !string.Equals(Settings.Default.reasoningLevel, "medium", StringComparison.OrdinalIgnoreCase))
            {
                settings.ReasoningEffort = Settings.Default.reasoningLevel;
            }
            else if (!supportsExplicitReasoningEffort &&
                     !string.Equals(Settings.Default.reasoningLevel, "medium", StringComparison.OrdinalIgnoreCase))
            {
                Log.Debug($"Ignoring reasoning effort override for local provider. Model/server defaults remain active: model={modelId}, configuredReasoningLevel={Settings.Default.reasoningLevel}");
            }

            // Build every dependent object before replacing the last working agent.
            int toolSchemaTokens = EstimateToolSchemaTokens(kernel);
            PromptBudgetProfile budgetProfile = PromptBudgetPlanner.BuildProfile(modelId, instructions, toolSchemaTokens);
            Agent = new ChatCompletionAgent
            {
                Instructions = instructions,
                Kernel = kernel,
                Arguments = new KernelArguments(settings)
            };

            _promptBudgetProfile = budgetProfile;
            Log.Info($"Agent initialized: provider={provider}, model={modelId}, contextWindow={_promptBudgetProfile.ContextWindowTokens}, plugins=[{string.Join(", ", registeredPlugins)}], llmLogLevel={(Settings.Default.llmLogLevel ?? "info").Trim()}");
            Log.Debug($"Prompt budget profile: model={modelId}, systemPromptTokens={_promptBudgetProfile.SystemPromptTokens}, toolSchemaTokens={_promptBudgetProfile.ToolSchemaTokens}, maxSystemAndTool={_promptBudgetProfile.MaxSystemAndToolTokens}, maxUserInput={_promptBudgetProfile.MaxUserInputTokens}, responseReserveBase={_promptBudgetProfile.ResponseReserveTokens}, safety={_promptBudgetProfile.SafetyMarginTokens}");
        }

        public AIThread CreateNewThread()
        {
            AIThread newAIThread = new(Properties.Resources.NewThread);
            AIThreads.Add(newAIThread);
            OnThreadListChanged();
            ActiveThread = newAIThread;
            SaveThreadHistoriesSafe();
            return newAIThread;
        }

        /// <summary>
        /// Orchestrates the lifecycle of an AI request, transitioning from user prompt to complete response text stream.
        /// Conducts preflight context validation, truncates history when necessary, provisions telemetry hooks, and routes tool capabilities.
        /// </summary>
        /// <param name="selectedModel">Target model identifier, used to govern the maximum allowable token count.</param>
        /// <param name="userInput">The raw directive text submitted by the user interface.</param>
        /// <param name="cancellationToken">Permits asynchronous interruptions if the user discards the request context.</param>
        /// <returns>The finalized, combined response output returned by the invoked agent instance.</returns>
        public async Task<string> CreateAsync(string selectedModel, string userInput, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userInput))
            {
                return string.Empty;
            }

            CreateNewAgentIfNeccessary(selectedModel);
            SupportedProviders provider = ProviderToStringConverter.Convert(Settings.Default.selectedProvider);

            if (AIThreads.Count == 0)
            {
                CreateNewThread();
            }
            ActiveThread ??= AIThreads[0];
            // UI selection may change while inference awaits the provider.
            AIThread requestThread = ActiveThread;
            ChatCompletionAgent requestAgent = Agent;
            if (requestThread.RemoveSyntheticGreetingMessage())
            {
                requestThread.ClearReducedHistoryCache();
            }

            if (TryAssignThreadNameFromUserInput(requestThread, userInput))
            {
                OnThreadListChanged();
            }

            RequestWorkspaceContext context = CaptureRequestWorkspaceContext(requestThread);
            var message = new ChatMessageContent(AuthorRole.User, userInput?.Trim() ?? string.Empty);
            EnsurePromptBudgetProfile(selectedModel);
            DateTime requestStartedUtc = DateTime.UtcNow;
            Log.Debug($"LLM request started: threadId={requestThread?.Id ?? "<none>"}, model={selectedModel}, userTokens={ChatTokenEstimator.EstimateTokens(message.Content)}, activeTabId={context.ActiveTabId ?? "<none>"}");

            DateTime now = DateTime.Now;
            KernelArguments arguments = new()
            {
                { "now", $"{now.ToShortDateString()} {now.ToShortTimeString()}" }
            };

            var sb = new StringBuilder();
            string rawLlmRequest = string.Empty;
            string rawLlmResponse = string.Empty;
            List<DebugHttpExchange> capturedHttpExchanges = new List<DebugHttpExchange>();
            using IDisposable llmHttpCaptureScope = EnterLlmHttpCapture(out LlmHttpCaptureSession llmHttpCaptureSession);
            using var editSession = new AgentEditSession(requestThread);

            try
            {
                // Phase 1: Context budget estimation and prompt preflight
                // Calculates whether the combined request (history + tool definitions + user input) exceeds the model's architectural context limit.
                ChatHistory chatHistory = requestThread?.AgentThread?.ChatHistory;
                int fullHistoryTokenEstimate = ChatTokenEstimator.EstimateMessagesTokens(chatHistory?.ToList());
                HistoryPreparationResult history = await _historyReductionEngine.PrepareForInvocationAsync(
                    requestThread,
                    selectedModel,
                    message.Content,
                    chatHistory,
                    _promptBudgetProfile,
                    GetChatCompletionServiceOrThrow(),
                    cancellationToken);
                int runtimeToolBudgetTokens = CalculateRuntimeToolBudgetTokens(history.Budget);
                bool preflightPassed = PromptBudgetPlanner.ValidatePreflight(
                    history.Budget,
                    out int preflightPlannedTokens,
                    out int preflightAllowedTokens,
                    out int preflightOverflowTokens);

                Log.Info(
                    $"Prompt preflight: model={selectedModel}, passed={preflightPassed}, rawContext={history.Budget.ContextWindowTokens}, " +
                    $"effectiveContext={history.Budget.EffectiveContextWindowTokens}, uncertaintyReserve={history.Budget.UncertaintyReserveTokens}, " +
                    $"planned={preflightPlannedTokens}, allowed={preflightAllowedTokens}, overflow={preflightOverflowTokens}");

                if (!preflightPassed)
                {
                    CaptureRawLlmHttpPayloads(llmHttpCaptureSession, out rawLlmRequest, out rawLlmResponse);
                    capturedHttpExchanges = GetCapturedHttpExchangesSnapshot(llmHttpCaptureSession);
                    if (string.IsNullOrWhiteSpace(rawLlmRequest))
                    {
                        rawLlmRequest = BuildRawLlmRequestDebugPayload(
                            provider,
                            selectedModel,
                            requestAgent.Instructions,
                            history.InvocationMessages,
                            message,
                            arguments);
                    }

                    string preflightError =
                        $"LLM request blocked before invoke because estimated prompt usage exceeds safe context budget. " +
                        $"Model={selectedModel}, rawContext={history.Budget.ContextWindowTokens}, " +
                        $"uncertaintyReserve={history.Budget.UncertaintyReserveTokens}, effectiveContext={preflightAllowedTokens}, " +
                        $"planned={preflightPlannedTokens}, overflow={preflightOverflowTokens}.";
                    Log.Error(preflightError);

                    requestThread.LastInvocationDebugInfo = BuildLastInvocationDebugInfo(
                        selectedModel,
                        context.ActiveTabId,
                        message.Content,
                        history.UseReducedThread,
                        history.Budget,
                        chatHistory?.ToList(),
                        history.InvocationMessages ?? new List<ChatMessageContent>(),
                        new List<ChatMessageContent>(),
                        new List<ToolInvocationBudgetRuntime.ToolInvocationBudgetState.TextReductionEntry>(),
                        string.Empty,
                        rawLlmRequest,
                        rawLlmResponse,
                        capturedHttpExchanges);

                    throw new InvalidOperationException(preflightError);
                }

                Log.Info(
                    $"Runtime tool budget prepared: model={selectedModel}, rawContext={history.Budget.ContextWindowTokens}, effectiveContext={history.Budget.EffectiveContextWindowTokens}, " +
                    $"toolReturnBudget={history.Budget.ToolReturnBudgetTokens}, responseReserve={history.Budget.ResponseReserveTokens}, " +
                    $"historyUsed={history.UsedHistoryTokens}/{history.Budget.HistoryBudgetTokens}, preflightPlanned={history.Budget.PreflightPlannedTokens}, preflightPassed={history.Budget.PreflightPassed}");
                // Log.Debug($"Prompt budget: model={selectedModel}, estimatedContext={history.Budget.ContextWindowTokens}, userTokens={history.Budget.UserInputTokens}, historyBudget={history.Budget.HistoryBudgetTokens}, historyEstimated={history.Budget.EstimatedHistoryTokens}, historyUsed={history.UsedHistoryTokens}, historyMessages={history.CandidateHistoryCount}, keptMessages={history.ReducedHistoryCount}, toolSchemaBudget={history.Budget.ToolSchemaBudgetTokens}, runtimeToolBudget={runtimeToolBudgetTokens}, systemPromptBudget={history.Budget.SystemPromptBudgetTokens}, responseReserve={history.Budget.ResponseReserveTokens}, safety={history.Budget.SafetyMarginTokens}");

                ChatHistoryAgentThread invocationThread = requestThread.AgentThread;
                int invocationBaseCount = invocationThread.ChatHistory.Count;
                bool usingReducedThread = history.UseReducedThread;
                List<ChatMessageContent> debugPromptMessages = null;
                List<ToolInvocationBudgetRuntime.ToolInvocationBudgetState.TextReductionEntry> textReductions =
                    new List<ToolInvocationBudgetRuntime.ToolInvocationBudgetState.TextReductionEntry>();

                if (history.Summarized || history.Trimmed || history.CompressedToolOutputs || history.RemovedMessagesCount > 0)
                {
                    int promptTokensBefore = fullHistoryTokenEstimate + history.Budget.UserInputTokens;
                    int promptTokensAfter = history.Budget.EstimatedHistoryTokens + history.Budget.UserInputTokens;
                    int totalPlannedBefore = promptTokensBefore
                        + history.Budget.SystemPromptBudgetTokens
                        + history.Budget.ToolSchemaBudgetTokens
                        + history.Budget.ResponseReserveTokens
                        + history.Budget.SafetyMarginTokens;
                    int totalPlannedAfter = promptTokensAfter
                        + history.Budget.SystemPromptBudgetTokens
                        + history.Budget.ToolSchemaBudgetTokens
                        + history.Budget.ResponseReserveTokens
                        + history.Budget.SafetyMarginTokens;
                    int effectiveContext = history.Budget.EffectiveContextWindowTokens > 0
                        ? history.Budget.EffectiveContextWindowTokens
                        : history.Budget.ContextWindowTokens;
                    int overflowAfterReduction = Math.Max(0, totalPlannedAfter - effectiveContext);

                    if (overflowAfterReduction > 0)
                    {
                        Log.Error(
                            $"[{ContextReductionErrorCode}] Reduced context estimate still exceeds effective context before invoke: model={selectedModel}, " +
                            $"totalPlannedBefore={totalPlannedBefore}, totalPlannedAfter={totalPlannedAfter}, effectiveContext={effectiveContext}, overflow={overflowAfterReduction}, " +
                            $"historyTokensBefore={fullHistoryTokenEstimate}, historyTokensAfter={history.Budget.EstimatedHistoryTokens}, " +
                            $"summarized={history.Summarized}, trimmed={history.Trimmed}, compressedTools={history.CompressedToolOutputs}, removed={history.RemovedMessagesCount}.");
                    }
                    else
                    {
                        Log.Info(
                            $"Context reduction diagnostics: model={selectedModel}, totalPlannedBefore={totalPlannedBefore}, totalPlannedAfter={totalPlannedAfter}, " +
                            $"effectiveContext={effectiveContext}, freedTokens={Math.Max(0, totalPlannedBefore - totalPlannedAfter)}, summarized={history.Summarized}, trimmed={history.Trimmed}, " +
                            $"compressedTools={history.CompressedToolOutputs}, removed={history.RemovedMessagesCount}.");
                    }
                }

                if (usingReducedThread)
                {
                    // Invoke on a reduced thread, but merge only new messages back into the full thread history.
                    invocationThread = new ChatHistoryAgentThread();
                    foreach (ChatMessageContent reducedMessage in history.InvocationMessages)
                    {
                        invocationThread.ChatHistory.Add(reducedMessage);
                    }

                    invocationBaseCount = invocationThread.ChatHistory.Count;
                }

                debugPromptMessages = invocationThread.ChatHistory?.ToList() ?? new List<ChatMessageContent>();

                requestThread.BeginContextUsage(selectedModel);
                // Enable after preparation so summary requests do not replace the agent meter.
                llmHttpCaptureSession.ContextThread = requestThread;
                llmHttpCaptureSession.ContextChanged = () => ToolActivityChanged?.Invoke(this, EventArgs.Empty);

                // Log.Debug($"Chat history reduction: full={history.FullHistoryCount}, candidate={history.CandidateHistoryCount}, reduced={history.ReducedHistoryCount}, summarized={history.Summarized}, trimmed={history.Trimmed}, compressedTools={history.CompressedToolOutputs}, removed={history.RemovedMessagesCount}, cacheApplied={history.CacheApplied}, cacheUpdated={history.CacheUpdated}, cacheCovered={history.CacheCoveredSourceMessages}, usingReducedThread={usingReducedThread}");

                IAsyncEnumerable<AgentResponseItem<ChatMessageContent>> responseStream;
                using (LLMPluginService.PushPreferredWorkspaceTabId(context.ActiveTabId))
                using (EnterToolActivitySession(requestThread))
                using (ToolInvocationBudgetRuntime.Enter(
                    history.Budget.EffectiveContextWindowTokens > 0
                        ? history.Budget.EffectiveContextWindowTokens
                        : history.Budget.ContextWindowTokens,
                    runtimeToolBudgetTokens))
                {
                    ToolActivitySessionCompletion toolActivityCompletion = ToolActivitySessionCompletion.Failed;
                    try
                    {
                        responseStream = requestAgent.InvokeAsync(message, invocationThread,
                            options: new() { KernelArguments = arguments }, cancellationToken: cancellationToken);

                        NewChatMessageReceived?.Invoke(this, EventArgs.Empty);

                        await using var enumerator = responseStream.GetAsyncEnumerator(cancellationToken);
                        while (await enumerator.MoveNextAsync())
                        {
                            AgentResponseItem<ChatMessageContent> response = enumerator.Current;
                            sb.Append(response.Message.Content);
                        }

                        if (ToolInvocationBudgetRuntime.TryGet(out ToolInvocationBudgetRuntime.ToolInvocationBudgetState currentBudgetState))
                        {
                            textReductions = currentBudgetState.GetTextReductionSnapshot();
                            Log.Info(
                                $"Tool runtime diagnostics: model={selectedModel}, initialToolBudget={runtimeToolBudgetTokens}, remainingToolTokens={currentBudgetState.RemainingToolTokens}, " +
                                $"toolCalls={currentBudgetState.ToolCallCount}, textReductions={textReductions.Count}");
                        }

                        toolActivityCompletion = ToolActivitySessionCompletion.Succeeded;
                    }
                    catch (Exception ex) when (IsLikelyContextWindowExceeded(ex))
                    {
                        int effectiveContext = history.Budget.EffectiveContextWindowTokens > 0
                            ? history.Budget.EffectiveContextWindowTokens
                            : history.Budget.ContextWindowTokens;
                        int preflightOverflow = Math.Max(0, history.Budget.PreflightPlannedTokens - effectiveContext);
                        int remainingToolTokens = -1;
                        int toolCalls = -1;
                        if (ToolInvocationBudgetRuntime.TryGet(out ToolInvocationBudgetRuntime.ToolInvocationBudgetState runtimeState))
                        {
                            remainingToolTokens = runtimeState.RemainingToolTokens;
                            toolCalls = runtimeState.ToolCallCount;
                        }

                        Log.Error(
                            $"[{ContextInvokeErrorCode}] Provider rejected request due to context window overflow despite preflight. " +
                            $"model={selectedModel}, message={ex.Message}, preflightPassed={history.Budget.PreflightPassed}, preflightPlanned={history.Budget.PreflightPlannedTokens}, " +
                            $"effectiveContext={effectiveContext}, preflightOverflow={preflightOverflow}, runtimeToolBudgetInitial={runtimeToolBudgetTokens}, remainingToolTokens={remainingToolTokens}, toolCalls={toolCalls}.");

                        CaptureRawLlmHttpPayloads(llmHttpCaptureSession, out rawLlmRequest, out rawLlmResponse);
                        capturedHttpExchanges = GetCapturedHttpExchangesSnapshot(llmHttpCaptureSession);
                        if (string.IsNullOrWhiteSpace(rawLlmResponse))
                        {
                            rawLlmResponse = sb.ToString();
                        }
                        requestThread.LastInvocationDebugInfo = BuildLastInvocationDebugInfo(
                            selectedModel,
                            context.ActiveTabId,
                            message.Content,
                            usingReducedThread,
                            history.Budget,
                            chatHistory?.ToList(),
                            debugPromptMessages,
                            invocationThread.ChatHistory.Skip(invocationBaseCount).ToList(),
                            textReductions,
                            sb.ToString(),
                            rawLlmRequest,
                            rawLlmResponse,
                            capturedHttpExchanges);
                        toolActivityCompletion = cancellationToken.IsCancellationRequested
                            ? ToolActivitySessionCompletion.Cancelled
                            : ToolActivitySessionCompletion.Failed;
                        throw;
                    }
                    catch (Exception ex)
                    {
                        string endpointInfo = GetConfiguredEndpointInfo(provider);
                        string exceptionChain = BuildExceptionMessageChain(ex);
                        bool requestWasCancelledByClient = cancellationToken.IsCancellationRequested;
                        string userFacingMessage = BuildUserFacingInvocationErrorMessage(
                            provider,
                            selectedModel,
                            endpointInfo,
                            ex,
                            requestWasCancelledByClient);

                        if (provider == SupportedProviders.Local && IsLikelyConnectionFailure(ex))
                        {
                            Log.Error(
                                $"LLM invocation failed: local endpoint unreachable. provider={provider}, model={selectedModel}, endpoint={endpointInfo}, errors={exceptionChain}");
                        }
                        else if (requestWasCancelledByClient)
                        {
                            Log.Info(
                                $"LLM invocation cancelled by client. provider={provider}, model={selectedModel}, endpoint={endpointInfo}, errors={exceptionChain}");
                        }
                        else
                        {
                            Log.Error(
                                $"LLM invocation failed: provider={provider}, model={selectedModel}, endpoint={endpointInfo}, errors={exceptionChain}");
                        }

                        Log.Error($"LLM invocation exception details: {ex}");
                        CaptureRawLlmHttpPayloads(llmHttpCaptureSession, out rawLlmRequest, out rawLlmResponse);
                        capturedHttpExchanges = GetCapturedHttpExchangesSnapshot(llmHttpCaptureSession);
                        if (string.IsNullOrWhiteSpace(rawLlmResponse))
                        {
                            rawLlmResponse = sb.ToString();
                        }
                        requestThread.LastInvocationDebugInfo = BuildLastInvocationDebugInfo(
                            selectedModel,
                            context.ActiveTabId,
                            message.Content,
                            usingReducedThread,
                            history.Budget,
                            chatHistory?.ToList(),
                            debugPromptMessages,
                            invocationThread.ChatHistory.Skip(invocationBaseCount).ToList(),
                            textReductions,
                            sb.ToString(),
                            rawLlmRequest,
                            rawLlmResponse,
                            capturedHttpExchanges);
                        toolActivityCompletion = cancellationToken.IsCancellationRequested
                            ? ToolActivitySessionCompletion.Cancelled
                            : ToolActivitySessionCompletion.Failed;
                        if (requestWasCancelledByClient)
                        {
                            throw new OperationCanceledException(userFacingMessage, ex, cancellationToken);
                        }

                        throw new InvalidOperationException(userFacingMessage, ex);
                    }
                    finally
                    {
                        FinalizeToolActivitySession(toolActivityCompletion);
                    }
                }

                string displayAnswer = AssistantResponseText.GetVisibleText(sb.ToString());
                ChatMessageContent finalAssistantMessage = invocationThread.ChatHistory.Skip(invocationBaseCount)
                    .LastOrDefault(item => item.Role == AuthorRole.Assistant);
                if (!IsVisibleAssistantChatMessage(finalAssistantMessage) ||
                    finalAssistantMessage.Items.OfType<FunctionCallContent>().Any())
                {
                    displayAnswer = Resources.ResourceManager.GetString("AiChatNoVisibleModelAnswer");
                    invocationThread.ChatHistory.AddAssistantMessage(displayAnswer);
                }

                if (usingReducedThread)
                {
                    for (int i = invocationBaseCount; i < invocationThread.ChatHistory.Count; i++)
                    {
                        requestThread.AgentThread.ChatHistory.Add(invocationThread.ChatHistory[i]);
                    }

                }

                List<ChatMessageContent> debugNewMessages = invocationThread.ChatHistory
                    .Skip(invocationBaseCount)
                    .ToList();
                requestThread.ApplyToolActivityArguments(ExtractToolActivityArgumentInfos(debugNewMessages));
                CaptureRawLlmHttpPayloads(llmHttpCaptureSession, out rawLlmRequest, out rawLlmResponse);
                capturedHttpExchanges = GetCapturedHttpExchangesSnapshot(llmHttpCaptureSession);
                if (string.IsNullOrWhiteSpace(rawLlmResponse))
                {
                    rawLlmResponse = sb.ToString();
                }
                requestThread.LastInvocationDebugInfo = BuildLastInvocationDebugInfo(
                    selectedModel,
                    context.ActiveTabId,
                    message.Content,
                    usingReducedThread,
                    history.Budget,
                    chatHistory?.ToList(),
                    debugPromptMessages,
                    debugNewMessages,
                    textReductions,
                    sb.ToString(),
                    rawLlmRequest,
                    rawLlmResponse,
                    capturedHttpExchanges);

                AttachLastCompletedToolActivityToLatestAssistantMessage(requestThread);

                SaveThreadHistoriesSafe();
                double elapsedMs = (DateTime.UtcNow - requestStartedUtc).TotalMilliseconds;
                Log.Info($"LLM request finished: model={selectedModel}, durationMs={elapsedMs:F0}, responseChars={sb.Length}, threadId={requestThread?.Id ?? "<none>"}");
                return displayAnswer;
            }
            finally
            {
                requestThread.CompleteContextUsage();
                LLMPluginService.InvokeOnUi(() => { editSession.Complete(); return true; });
                ToolActivityChanged?.Invoke(this, EventArgs.Empty);
                ClearRequestWorkspaceContext(requestThread);
            }
        }

        /// <summary>
        /// Synthesizes a comprehensive snapshot of the agent's interaction loop, encompassing text reductions, network footprints,
        /// token limits, and tool utilization. This snapshot serves as the primary artifact for telemetry and thesis evaluation.
        /// </summary>
        private static LastInvocationDebugInfo BuildLastInvocationDebugInfo(
            string modelId,
            string activeTabId,
            string userInput,
            bool usedReducedThread,
            PromptBudget budget,
            IReadOnlyList<ChatMessageContent> fullHistoryBeforeReduction,
            IReadOnlyList<ChatMessageContent> promptMessages,
            IReadOnlyList<ChatMessageContent> newMessages,
            IReadOnlyList<ToolInvocationBudgetRuntime.ToolInvocationBudgetState.TextReductionEntry> textReductions,
            string finalAssistantResponse,
            string rawLlmRequest,
            string rawLlmResponse,
            IReadOnlyList<DebugHttpExchange> httpExchanges)
        {
            LastInvocationDebugInfo info = new LastInvocationDebugInfo
            {
                CreatedAtLocal = DateTime.Now,
                ModelId = modelId ?? string.Empty,
                ActiveTabId = activeTabId,
                UsedReducedThread = usedReducedThread,
                UserInput = userInput ?? string.Empty,
                FinalAssistantResponse = finalAssistantResponse ?? string.Empty,
                RawLlmRequest = rawLlmRequest ?? string.Empty,
                RawLlmResponse = rawLlmResponse ?? string.Empty,
                Budget = new PromptBudgetSnapshot
                {
                    ContextWindowTokens = budget?.ContextWindowTokens ?? 0,
                    EffectiveContextWindowTokens = budget?.EffectiveContextWindowTokens ?? 0,
                    UncertaintyReserveTokens = budget?.UncertaintyReserveTokens ?? 0,
                    UserInputTokens = budget?.UserInputTokens ?? 0,
                    HistoryBudgetTokens = budget?.HistoryBudgetTokens ?? 0,
                    EstimatedHistoryTokens = budget?.EstimatedHistoryTokens ?? 0,
                    SystemPromptBudgetTokens = budget?.SystemPromptBudgetTokens ?? 0,
                    ToolSchemaBudgetTokens = budget?.ToolSchemaBudgetTokens ?? 0,
                    ToolReturnBudgetTokens = budget?.ToolReturnBudgetTokens ?? 0,
                    ResponseReserveTokens = budget?.ResponseReserveTokens ?? 0,
                    SafetyMarginTokens = budget?.SafetyMarginTokens ?? 0,
                    PreflightPlannedTokens = budget?.PreflightPlannedTokens ?? 0,
                    PreflightPassed = budget?.PreflightPassed ?? false
                }
            };

            CapturePromptMessagesForDebug(info, promptMessages);
            CaptureToolCallsForDebug(info, newMessages);
            CaptureHttpExchangesForDebug(info, httpExchanges);
            CaptureTextReductionsForDebug(info, textReductions);
            CaptureSummaryMessagesForDebug(info, fullHistoryBeforeReduction, promptMessages);
            return info;
        }

        private static void CaptureHttpExchangesForDebug(
            LastInvocationDebugInfo info,
            IReadOnlyList<DebugHttpExchange> httpExchanges)
        {
            if (info == null || httpExchanges == null || httpExchanges.Count == 0)
            {
                return;
            }

            for (int i = 0; i < httpExchanges.Count; i++)
            {
                DebugHttpExchange exchange = httpExchanges[i];
                if (exchange == null)
                {
                    continue;
                }

                info.HttpExchanges.Add(new DebugHttpExchange
                {
                    Sequence = Math.Max(0, exchange.Sequence),
                    Method = exchange.Method ?? string.Empty,
                    Uri = exchange.Uri ?? string.Empty,
                    RequestHeaders = exchange.RequestHeaders ?? string.Empty,
                    RequestBody = exchange.RequestBody ?? string.Empty,
                    StatusCode = exchange.StatusCode,
                    ReasonPhrase = exchange.ReasonPhrase ?? string.Empty,
                    ResponseHeaders = exchange.ResponseHeaders ?? string.Empty,
                    ResponseBody = exchange.ResponseBody ?? string.Empty,
                    Error = exchange.Error ?? string.Empty
                });
            }
        }

        private static void CaptureTextReductionsForDebug(
            LastInvocationDebugInfo info,
            IReadOnlyList<ToolInvocationBudgetRuntime.ToolInvocationBudgetState.TextReductionEntry> reductions)
        {
            if (info == null || reductions == null || reductions.Count == 0)
            {
                return;
            }

            for (int i = 0; i < reductions.Count; i++)
            {
                ToolInvocationBudgetRuntime.ToolInvocationBudgetState.TextReductionEntry reduction = reductions[i];
                if (reduction == null)
                {
                    continue;
                }

                info.TextReductions.Add(new DebugTextReduction
                {
                    Source = reduction.Source ?? string.Empty,
                    Method = reduction.Method ?? string.Empty,
                    PluginName = reduction.PluginName ?? string.Empty,
                    FunctionName = reduction.FunctionName ?? string.Empty,
                    OriginalTokens = Math.Max(0, reduction.OriginalTokens),
                    ReducedTokens = Math.Max(0, reduction.ReducedTokens),
                    OriginalText = reduction.OriginalText ?? string.Empty,
                    ReducedText = reduction.ReducedText ?? string.Empty
                });
            }
        }

        private static void CaptureSummaryMessagesForDebug(
            LastInvocationDebugInfo info,
            IReadOnlyList<ChatMessageContent> fullHistoryBeforeReduction,
            IReadOnlyList<ChatMessageContent> promptMessagesAfterReduction)
        {
            if (info == null)
            {
                return;
            }

            AppendSummaryMessages(info.SummaryMessagesBeforeReduction, fullHistoryBeforeReduction);
            AppendSummaryMessages(info.SummaryMessagesAfterReduction, promptMessagesAfterReduction);
        }

        private static void AppendSummaryMessages(List<string> target, IReadOnlyList<ChatMessageContent> messages)
        {
            if (target == null || messages == null || messages.Count == 0)
            {
                return;
            }

            for (int i = 0; i < messages.Count; i++)
            {
                ChatMessageContent message = messages[i];
                if (message == null)
                {
                    continue;
                }

                string content = (message.Content ?? string.Empty).TrimStart();
                bool isSummaryLikeAssistantText =
                    message.Role == AuthorRole.Assistant &&
                    (content.StartsWith("[history-summary]", StringComparison.OrdinalIgnoreCase) ||
                     content.StartsWith("[tool-summary]", StringComparison.OrdinalIgnoreCase) ||
                     content.StartsWith("[tool-result-text]", StringComparison.OrdinalIgnoreCase) ||
                     content.StartsWith(AssistantResponseText.ToolMemoryPrefix, StringComparison.Ordinal));

                bool isBudgetSkipToolMarker =
                    message.Role == AuthorRole.Tool &&
                    content.StartsWith("Skipped tool call due to limited context budget", StringComparison.OrdinalIgnoreCase);

                if (!isSummaryLikeAssistantText && !isBudgetSkipToolMarker)
                {
                    continue;
                }

                target.Add(content);
            }
        }

        private static void CapturePromptMessagesForDebug(
            LastInvocationDebugInfo info,
            IReadOnlyList<ChatMessageContent> promptMessages)
        {
            if (info == null || promptMessages == null)
            {
                return;
            }

            for (int i = 0; i < promptMessages.Count; i++)
            {
                ChatMessageContent message = promptMessages[i];
                if (message == null)
                {
                    continue;
                }

                DebugPromptMessage debugMessage = new DebugPromptMessage
                {
                    Index = i,
                    Role = message.Role.ToString(),
                    Content = message.Content ?? string.Empty
                };

                if (message.Items != null)
                {
                    for (int itemIndex = 0; itemIndex < message.Items.Count; itemIndex++)
                    {
                        object item = message.Items[itemIndex];
                        string summary = BuildPromptItemSummary(item);
                        if (!string.IsNullOrWhiteSpace(summary))
                        {
                            debugMessage.ItemSummaries.Add(summary);
                        }
                    }
                }

                info.PromptMessages.Add(debugMessage);
            }
        }

        private static void CaptureToolCallsForDebug(
            LastInvocationDebugInfo info,
            IReadOnlyList<ChatMessageContent> newMessages)
        {
            if (info == null || newMessages == null || newMessages.Count == 0)
            {
                return;
            }

            Dictionary<string, DebugToolCall> callsById = new Dictionary<string, DebugToolCall>(StringComparer.OrdinalIgnoreCase);
            Queue<DebugToolCall> anonymousCalls = new Queue<DebugToolCall>();

            void registerCall(DebugToolCall call)
            {
                info.ToolCalls.Add(call);
                string normalizedCallId = string.IsNullOrWhiteSpace(call.CallId) ? string.Empty : call.CallId.Trim();
                if (string.IsNullOrWhiteSpace(normalizedCallId))
                {
                    anonymousCalls.Enqueue(call);
                    return;
                }

                callsById[normalizedCallId] = call;
            }

            for (int i = 0; i < newMessages.Count; i++)
            {
                ChatMessageContent message = newMessages[i];
                if (message?.Items == null || message.Items.Count == 0)
                {
                    continue;
                }

                for (int itemIndex = 0; itemIndex < message.Items.Count; itemIndex++)
                {
                    if (message.Items[itemIndex] is FunctionCallContent functionCall)
                    {
                        DebugToolCall call = new DebugToolCall
                        {
                            Sequence = info.ToolCalls.Count + 1,
                            CallId = functionCall.Id?.Trim() ?? string.Empty,
                            PluginName = functionCall.PluginName ?? string.Empty,
                            FunctionName = functionCall.FunctionName ?? string.Empty,
                            Arguments = SerializeFunctionCallArguments(functionCall.Arguments)
                        };
                        registerCall(call);
                    }
                }

                for (int itemIndex = 0; itemIndex < message.Items.Count; itemIndex++)
                {
                    if (message.Items[itemIndex] is not FunctionResultContent functionResult)
                    {
                        continue;
                    }

                    string resultCallId = functionResult.CallId?.Trim() ?? string.Empty;
                    bool hasCallId = !string.IsNullOrWhiteSpace(resultCallId);

                    DebugToolCall target = null;
                    if (hasCallId)
                    {
                        callsById.TryGetValue(resultCallId, out target);
                    }
                    else if (anonymousCalls.Count > 0)
                    {
                        target = anonymousCalls.Dequeue();
                    }

                    if (target == null)
                    {
                        target = new DebugToolCall
                        {
                            Sequence = info.ToolCalls.Count + 1,
                            CallId = hasCallId ? resultCallId : string.Empty,
                            PluginName = functionResult.PluginName ?? string.Empty,
                            FunctionName = functionResult.FunctionName ?? string.Empty,
                            Arguments = string.Empty
                        };
                        registerCall(target);
                    }

                    string resultPayload = functionResult.Result as string;
                    if (string.IsNullOrWhiteSpace(resultPayload))
                    {
                        resultPayload = TrySerializeObject(functionResult.Result);
                    }

                    if (string.IsNullOrWhiteSpace(resultPayload))
                    {
                        resultPayload = message.Content ?? string.Empty;
                    }

                    target.HasReturn = true;
                    target.ReturnCallId = resultCallId;
                    target.ReturnFunctionName = functionResult.FunctionName ?? string.Empty;
                    target.ReturnValue = resultPayload ?? string.Empty;
                }
            }
        }

        private static string BuildPromptItemSummary(object item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            if (item is FunctionCallContent functionCall)
            {
                string callId = functionCall.Id?.Trim() ?? string.Empty;
                string callIdText = string.IsNullOrWhiteSpace(callId) ? "n/a" : callId;
                string arguments = BuildFunctionCallArgumentsSummary(functionCall.Arguments);
                return string.IsNullOrWhiteSpace(arguments)
                    ? $"[FunctionCall] {(functionCall.PluginName ?? string.Empty)}.{(functionCall.FunctionName ?? string.Empty)} callId={callIdText}"
                    : $"[FunctionCall] {(functionCall.PluginName ?? string.Empty)}.{(functionCall.FunctionName ?? string.Empty)} callId={callIdText} arguments={arguments}";
            }

            if (item is FunctionResultContent functionResult)
            {
                string callId = functionResult.CallId?.Trim() ?? string.Empty;
                string callIdText = string.IsNullOrWhiteSpace(callId) ? "n/a" : callId;
                return $"[FunctionResult] {(functionResult.PluginName ?? string.Empty)}.{(functionResult.FunctionName ?? string.Empty)} callId={callIdText}";
            }

            return $"[{item.GetType().Name}]";
        }

        private static string TrySerializeObject(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is string s)
            {
                return s;
            }

            try
            {
                return JsonConvert.SerializeObject(value, Formatting.Indented) ?? string.Empty;
            }
            catch
            {
                return value.ToString() ?? string.Empty;
            }
        }

        private static string SerializeFunctionCallArguments(object arguments)
        {
            if (arguments == null)
            {
                return string.Empty;
            }

            if (arguments is string text)
            {
                return text;
            }

            if (TryBuildNamedArgumentsObject(arguments, out JObject namedArguments))
            {
                return namedArguments.ToString(Formatting.Indented);
            }

            return TrySerializeObject(arguments);
        }

        private static List<ToolActivityArgumentInfo> ExtractToolActivityArgumentInfos(IReadOnlyList<ChatMessageContent> newMessages)
        {
            List<ToolActivityArgumentInfo> result = new List<ToolActivityArgumentInfo>();
            if (newMessages == null)
            {
                return result;
            }

            for (int i = 0; i < newMessages.Count; i++)
            {
                ChatMessageContent message = newMessages[i];
                if (message?.Items == null || message.Items.Count == 0)
                {
                    continue;
                }

                for (int itemIndex = 0; itemIndex < message.Items.Count; itemIndex++)
                {
                    if (message.Items[itemIndex] is not FunctionCallContent functionCall)
                    {
                        continue;
                    }

                    string arguments = SerializeFunctionCallArguments(functionCall.Arguments);
                    if (string.IsNullOrWhiteSpace(arguments))
                    {
                        continue;
                    }

                    result.Add(new ToolActivityArgumentInfo
                    {
                        CallId = functionCall.Id?.Trim() ?? string.Empty,
                        FunctionName = functionCall.FunctionName ?? string.Empty,
                        Arguments = arguments
                    });
                }
            }

            return result;
        }

        private static string BuildFunctionCallArgumentsSummary(object arguments)
        {
            string serialized = SerializeFunctionCallArguments(arguments);
            if (string.IsNullOrWhiteSpace(serialized))
            {
                return string.Empty;
            }

            string compact = serialized
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Trim();

            while (compact.Contains("  "))
            {
                compact = compact.Replace("  ", " ");
            }

            const int maxLength = 240;
            if (compact.Length > maxLength)
            {
                compact = compact.Substring(0, maxLength) + "...";
            }

            return compact;
        }

        private static bool TryBuildNamedArgumentsObject(object arguments, out JObject result)
        {
            result = null;

            if (arguments is string || arguments is not System.Collections.IEnumerable enumerable)
            {
                return false;
            }

            JObject obj = new JObject();
            foreach (object entry in enumerable)
            {
                if (entry == null)
                {
                    continue;
                }

                if (entry is System.Collections.DictionaryEntry dictionaryEntry)
                {
                    string key = dictionaryEntry.Key?.ToString();
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        continue;
                    }

                    obj[key] = BuildJTokenSafe(dictionaryEntry.Value);
                    continue;
                }

                object keyObject = TryGetPropertyValue(entry, "Key") ?? TryGetPropertyValue(entry, "Name");
                if (keyObject == null)
                {
                    continue;
                }

                string keyText = keyObject.ToString();
                if (string.IsNullOrWhiteSpace(keyText))
                {
                    continue;
                }

                object valueObject = TryGetPropertyValue(entry, "Value");
                obj[keyText] = BuildJTokenSafe(valueObject);
            }

            if (!obj.HasValues)
            {
                return false;
            }

            result = obj;
            return true;
        }

        private static HttpClient CreateInstrumentedOpenAiHttpClient()
        {
            HttpMessageHandler innerHandler = new HttpClientHandler();
            HttpMessageHandler captureHandler = new LlmHttpCaptureHandler(innerHandler);
            return new HttpClient(captureHandler, disposeHandler: true);
        }

        private static IDisposable EnterLlmHttpCapture(out LlmHttpCaptureSession session)
        {
            LlmHttpCaptureSession previous = CurrentLlmHttpCapture.Value;
            session = new LlmHttpCaptureSession();
            CurrentLlmHttpCapture.Value = session;
            return new LlmHttpCaptureScope(previous);
        }

        private static void CaptureRawLlmHttpPayloads(
            LlmHttpCaptureSession session,
            out string requestText,
            out string responseText)
        {
            if (session == null)
            {
                requestText = string.Empty;
                responseText = string.Empty;
                return;
            }

            requestText = session.BuildRequestText();
            responseText = session.BuildResponseText();
        }

        private static List<DebugHttpExchange> GetCapturedHttpExchangesSnapshot(LlmHttpCaptureSession session)
        {
            if (session == null)
            {
                return new List<DebugHttpExchange>();
            }

            List<LlmHttpExchange> exchanges = session.GetExchangeSnapshot();
            if (exchanges == null || exchanges.Count == 0)
            {
                return new List<DebugHttpExchange>();
            }

            List<DebugHttpExchange> result = new List<DebugHttpExchange>(exchanges.Count);
            for (int i = 0; i < exchanges.Count; i++)
            {
                LlmHttpExchange exchange = exchanges[i];
                if (exchange == null)
                {
                    continue;
                }

                result.Add(new DebugHttpExchange
                {
                    Sequence = exchange.Sequence,
                    Method = exchange.Method ?? string.Empty,
                    Uri = exchange.Uri ?? string.Empty,
                    RequestHeaders = exchange.RequestHeaders ?? string.Empty,
                    RequestBody = exchange.RequestBody ?? string.Empty,
                    StatusCode = exchange.StatusCode,
                    ReasonPhrase = exchange.ReasonPhrase ?? string.Empty,
                    ResponseHeaders = exchange.ResponseHeaders ?? string.Empty,
                    ResponseBody = exchange.ResponseBody ?? string.Empty,
                    Error = exchange.Error ?? string.Empty
                });
            }

            return result;
        }

        private sealed class LlmHttpCaptureScope : IDisposable
        {
            private readonly LlmHttpCaptureSession _previous;

            public LlmHttpCaptureScope(LlmHttpCaptureSession previous)
            {
                _previous = previous;
            }

            public void Dispose()
            {
                CurrentLlmHttpCapture.Value = _previous;
            }
        }

        /// <summary>
        /// Intercepts inbound and outbound HTTP requests tied to the LLM interaction chain.
        /// Maintains exhaustive observability into payloads and header exchanges to aid academic and architectural transparency.
        /// </summary>
        private sealed class LlmHttpCaptureHandler : DelegatingHandler
        {
            public LlmHttpCaptureHandler(HttpMessageHandler innerHandler)
                : base(innerHandler)
            {
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // The installed connector supports text tool results. Expand our persisted
                // screenshot envelopes into actual image inputs before the next model call.
                if (request.Content != null && request.Content.Headers.ContentType?.MediaType == "application/json")
                {
                    string original = await request.Content.ReadAsStringAsync();
                    string expanded = AssistantResponseText.PrepareRequest(WorkspaceScreenshotContent.ExpandRequest(original));
                    if (!string.Equals(original, expanded, StringComparison.Ordinal))
                    {
                        HttpContent oldContent = request.Content;
                        request.Content = new StringContent(expanded, Encoding.UTF8, "application/json");
                        foreach (var header in oldContent.Headers)
                        {
                            if (!string.Equals(header.Key, "Content-Length", StringComparison.OrdinalIgnoreCase) &&
                                !string.Equals(header.Key, "Content-Type", StringComparison.OrdinalIgnoreCase))
                                request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }
                        oldContent.Dispose();
                    }
                }
                LlmHttpCaptureSession session = CurrentLlmHttpCapture.Value;
                if (session == null)
                {
                    return await base.SendAsync(request, cancellationToken);
                }

                string requestBody = await ReadRequestContentSafeAsync(request);
                int sequence = session.RegisterRequest(
                    request?.Method?.Method ?? "UNKNOWN",
                    request?.RequestUri?.ToString() ?? string.Empty,
                    FormatHeaders(request?.Headers, request?.Content?.Headers),
                    requestBody);

                try
                {
                    HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
                    bool isStreamingExchange =
                        IsLikelyStreamingRequest(requestBody) ||
                        IsLikelyStreamingResponse(response);
                    string responseBody = isStreamingExchange
                        ? "<streaming response body capture skipped>"
                        : await ReadAndCloneResponseContentSafeAsync(response);
                    session.RegisterResponse(
                        sequence,
                        response?.StatusCode,
                        response?.ReasonPhrase ?? string.Empty,
                        FormatHeaders(response?.Headers, response?.Content?.Headers),
                        responseBody);

                    return response;
                }
                catch (Exception ex)
                {
                    session.RegisterFailure(sequence, ex);
                    throw;
                }
            }

            private static bool IsLikelyStreamingRequest(string requestBody)
            {
                if (string.IsNullOrWhiteSpace(requestBody))
                {
                    return false;
                }

                return requestBody.IndexOf("\"stream\":true", StringComparison.OrdinalIgnoreCase) >= 0 ||
                       requestBody.IndexOf("\"stream\": true", StringComparison.OrdinalIgnoreCase) >= 0;
            }

            private static bool IsLikelyStreamingResponse(HttpResponseMessage response)
            {
                if (response == null)
                {
                    return false;
                }

                string mediaType = response.Content?.Headers?.ContentType?.MediaType ?? string.Empty;
                if (mediaType.IndexOf("text/event-stream", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }

                if (response.Headers?.TransferEncodingChunked == true &&
                    response.Content?.Headers?.ContentLength == null)
                {
                    return true;
                }

                return false;
            }

            private static async Task<string> ReadRequestContentSafeAsync(HttpRequestMessage request)
            {
                try
                {
                    if (request?.Content == null)
                    {
                        return string.Empty;
                    }

                    await request.Content.LoadIntoBufferAsync();
                    byte[] payload = await request.Content.ReadAsByteArrayAsync();
                    return DecodeBytes(payload, request.Content.Headers?.ContentType?.CharSet);
                }
                catch (Exception ex)
                {
                    return $"<failed to read request body: {ex.GetType().Name}: {ex.Message}>";
                }
            }

            private static async Task<string> ReadAndCloneResponseContentSafeAsync(HttpResponseMessage response)
            {
                try
                {
                    if (response?.Content == null)
                    {
                        return string.Empty;
                    }

                    HttpContent originalContent = response.Content;
                    byte[] payload = await originalContent.ReadAsByteArrayAsync();
                    string bodyText = DecodeBytes(payload, originalContent.Headers?.ContentType?.CharSet);

                    ByteArrayContent replacement = new ByteArrayContent(payload);
                    if (originalContent.Headers != null)
                    {
                        foreach (KeyValuePair<string, IEnumerable<string>> header in originalContent.Headers)
                        {
                            replacement.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }

                    response.Content = replacement;
                    return bodyText;
                }
                catch (Exception ex)
                {
                    return $"<failed to read response body: {ex.GetType().Name}: {ex.Message}>";
                }
            }

            private static string FormatHeaders(HttpHeaders headers, HttpHeaders contentHeaders)
            {
                StringBuilder sb = new StringBuilder();
                AppendHeaders(sb, headers);
                AppendHeaders(sb, contentHeaders);
                return sb.ToString().TrimEnd();
            }

            private static void AppendHeaders(StringBuilder sb, HttpHeaders headers)
            {
                if (sb == null || headers == null)
                {
                    return;
                }

                foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
                {
                    if (header.Key == null)
                    {
                        continue;
                    }

                    sb.Append(header.Key);
                    sb.Append(": ");
                    // Diagnostics can be persisted or exported with the chat history.
                    bool containsCredentials = string.Equals(header.Key, "Authorization", StringComparison.OrdinalIgnoreCase) ||
                                               string.Equals(header.Key, "Proxy-Authorization", StringComparison.OrdinalIgnoreCase);
                    sb.AppendLine(containsCredentials ? "[redacted]" :
                        header.Value == null ? string.Empty : string.Join(", ", header.Value));
                }
            }

            private static string DecodeBytes(byte[] payload, string charSet)
            {
                if (payload == null || payload.Length == 0)
                {
                    return string.Empty;
                }

                Encoding encoding = null;
                if (!string.IsNullOrWhiteSpace(charSet))
                {
                    try
                    {
                        encoding = Encoding.GetEncoding(charSet);
                    }
                    catch
                    {
                    }
                }

                encoding ??= Encoding.UTF8;
                return encoding.GetString(payload);
            }
        }

        private sealed class LlmHttpCaptureSession
        {
            internal AIThread ContextThread { get; set; }
            internal Action ContextChanged { get; set; }
            private int? _latestPromptTokens;

            private readonly object _sync = new object();
            private readonly List<LlmHttpExchange> _exchanges = new List<LlmHttpExchange>();
            private int _nextSequence = 1;

            public int RegisterRequest(string method, string uri, string headers, string body)
            {
                _latestPromptTokens = ChatTokenEstimator.EstimateRequestTokens(body);
                if (ContextThread != null && _latestPromptTokens.HasValue)
                {
                    ContextThread.UpdateContextUsage(_latestPromptTokens.Value);
                    ContextChanged?.Invoke();
                }
                lock (_sync)
                {
                    int sequence = _nextSequence++;
                    _exchanges.Add(new LlmHttpExchange
                    {
                        Sequence = sequence,
                        Method = method ?? string.Empty,
                        Uri = uri ?? string.Empty,
                        RequestHeaders = headers ?? string.Empty,
                        RequestBody = body ?? string.Empty
                    });
                    return sequence;
                }
            }

            public void RegisterResponse(int sequence, System.Net.HttpStatusCode? statusCode, string reasonPhrase, string headers, string body)
            {
                if (sequence <= 0)
                {
                    return;
                }

                lock (_sync)
                {
                    LlmHttpExchange exchange = FindOrCreateExchange(sequence);
                    exchange.StatusCode = statusCode.HasValue ? (int)statusCode.Value : (int?)null;
                    exchange.ReasonPhrase = reasonPhrase ?? string.Empty;
                    exchange.ResponseHeaders = headers ?? string.Empty;
                    exchange.ResponseBody = body ?? string.Empty;
                }
                if (ContextThread != null && _latestPromptTokens.HasValue)
                {
                    ContextThread.UpdateContextUsage(_latestPromptTokens.Value + ChatTokenEstimator.EstimateResponseTokens(body));
                    ContextChanged?.Invoke();
                }
            }

            public void RegisterFailure(int sequence, Exception exception)
            {
                if (sequence <= 0)
                {
                    return;
                }

                lock (_sync)
                {
                    LlmHttpExchange exchange = FindOrCreateExchange(sequence);
                    string message = exception == null
                        ? "Unknown HTTP failure."
                        : $"{exception.GetType().Name}: {exception.Message}";
                    exchange.Error = message;
                }
            }

            public string BuildRequestText()
            {
                List<LlmHttpExchange> snapshot = Snapshot();
                if (snapshot.Count == 0)
                {
                    return "(keine HTTP-Requests erfasst)";
                }

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < snapshot.Count; i++)
                {
                    LlmHttpExchange exchange = snapshot[i];
                    sb.Append("[HTTP #");
                    sb.Append(exchange.Sequence);
                    sb.Append("] ");
                    sb.Append(exchange.Method);
                    sb.Append(" ");
                    sb.AppendLine(string.IsNullOrWhiteSpace(exchange.Uri) ? "(unknown-uri)" : exchange.Uri);
                    sb.AppendLine("[Headers]");
                    sb.AppendLine(string.IsNullOrWhiteSpace(exchange.RequestHeaders) ? "(none)" : exchange.RequestHeaders);
                    sb.AppendLine("[Body]");
                    sb.AppendLine(string.IsNullOrWhiteSpace(exchange.RequestBody) ? "(empty)" : exchange.RequestBody);
                    sb.AppendLine(new string('=', 90));
                }

                return sb.ToString();
            }

            public string BuildResponseText()
            {
                List<LlmHttpExchange> snapshot = Snapshot();
                if (snapshot.Count == 0)
                {
                    return "(keine HTTP-Responses erfasst)";
                }

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < snapshot.Count; i++)
                {
                    LlmHttpExchange exchange = snapshot[i];
                    sb.Append("[HTTP #");
                    sb.Append(exchange.Sequence);
                    sb.AppendLine("]");
                    sb.Append("Status: ");
                    sb.Append(exchange.StatusCode?.ToString() ?? "(none)");
                    if (!string.IsNullOrWhiteSpace(exchange.ReasonPhrase))
                    {
                        sb.Append(" ");
                        sb.Append(exchange.ReasonPhrase);
                    }

                    sb.AppendLine();
                    if (!string.IsNullOrWhiteSpace(exchange.Error))
                    {
                        sb.Append("Error: ");
                        sb.AppendLine(exchange.Error);
                    }

                    sb.AppendLine("[Headers]");
                    sb.AppendLine(string.IsNullOrWhiteSpace(exchange.ResponseHeaders) ? "(none)" : exchange.ResponseHeaders);
                    sb.AppendLine("[Body]");
                    sb.AppendLine(string.IsNullOrWhiteSpace(exchange.ResponseBody) ? "(empty)" : exchange.ResponseBody);
                    sb.AppendLine(new string('=', 90));
                }

                return sb.ToString();
            }

            public List<LlmHttpExchange> GetExchangeSnapshot()
            {
                return Snapshot();
            }

            private List<LlmHttpExchange> Snapshot()
            {
                lock (_sync)
                {
                    return _exchanges
                        .Select(exchange => exchange.Clone())
                        .OrderBy(exchange => exchange.Sequence)
                        .ToList();
                }
            }

            private LlmHttpExchange FindOrCreateExchange(int sequence)
            {
                LlmHttpExchange exchange = _exchanges.FirstOrDefault(e => e.Sequence == sequence);
                if (exchange != null)
                {
                    return exchange;
                }

                exchange = new LlmHttpExchange { Sequence = sequence };
                _exchanges.Add(exchange);
                return exchange;
            }
        }

        private sealed class LlmHttpExchange
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

            public LlmHttpExchange Clone()
            {
                return new LlmHttpExchange
                {
                    Sequence = Sequence,
                    Method = Method,
                    Uri = Uri,
                    RequestHeaders = RequestHeaders,
                    RequestBody = RequestBody,
                    StatusCode = StatusCode,
                    ReasonPhrase = ReasonPhrase,
                    ResponseHeaders = ResponseHeaders,
                    ResponseBody = ResponseBody,
                    Error = Error
                };
            }
        }

        private static string BuildRawLlmRequestDebugPayload(
            SupportedProviders provider,
            string modelId,
            string instructions,
            IReadOnlyList<ChatMessageContent> historyMessages,
            ChatMessageContent userMessage,
            KernelArguments kernelArguments)
        {
            try
            {
                JObject root = new JObject
                {
                    ["provider"] = provider.ToString(),
                    ["model"] = modelId ?? string.Empty,
                    ["instructions"] = instructions ?? string.Empty
                };

                JObject argumentsToken = new JObject();
                if (kernelArguments != null)
                {
                    foreach (KeyValuePair<string, object> argument in kernelArguments)
                    {
                        if (string.IsNullOrWhiteSpace(argument.Key))
                        {
                            continue;
                        }

                        argumentsToken[argument.Key] = BuildJTokenSafe(argument.Value);
                    }
                }

                root["kernelArguments"] = argumentsToken;
                root["historyMessages"] = BuildDebugMessageListToken(historyMessages);
                root["userMessage"] = BuildDebugMessageToken(
                    userMessage,
                    historyMessages?.Count ?? 0);

                return root.ToString(Formatting.Indented);
            }
            catch (Exception ex)
            {
                return $"Failed to build raw LLM request payload: {ex.GetType().Name}: {ex.Message}";
            }
        }

        private static JArray BuildDebugMessageListToken(IReadOnlyList<ChatMessageContent> messages)
        {
            JArray token = new JArray();
            if (messages == null || messages.Count == 0)
            {
                return token;
            }

            for (int i = 0; i < messages.Count; i++)
            {
                token.Add(BuildDebugMessageToken(messages[i], i));
            }

            return token;
        }

        private static JObject BuildDebugMessageToken(ChatMessageContent message, int index)
        {
            JObject token = new JObject
            {
                ["index"] = index,
                ["role"] = message?.Role.ToString() ?? "Unknown",
                ["content"] = message?.Content ?? string.Empty
            };

            if (message?.Items != null && message.Items.Count > 0)
            {
                JArray itemsToken = new JArray();
                for (int i = 0; i < message.Items.Count; i++)
                {
                    itemsToken.Add(BuildDebugPromptItemToken(message.Items[i], i));
                }

                token["items"] = itemsToken;
            }

            return token;
        }

        private static JObject BuildDebugPromptItemToken(object item, int index)
        {
            JObject token = new JObject
            {
                ["index"] = index,
                ["type"] = item?.GetType().Name ?? "null"
            };

            if (item is FunctionCallContent functionCall)
            {
                token["kind"] = "function-call";
                token["callId"] = functionCall.Id?.Trim() ?? string.Empty;
                token["plugin"] = functionCall.PluginName ?? string.Empty;
                token["function"] = functionCall.FunctionName ?? string.Empty;
                token["arguments"] = ParseDebugJsonTokenOrString(SerializeFunctionCallArguments(functionCall.Arguments));
                return token;
            }

            if (item is FunctionResultContent functionResult)
            {
                token["kind"] = "function-result";
                token["callId"] = functionResult.CallId?.Trim() ?? string.Empty;
                token["plugin"] = functionResult.PluginName ?? string.Empty;
                token["function"] = functionResult.FunctionName ?? string.Empty;
                token["result"] = TrySerializeObject(functionResult.Result);
                return token;
            }

            token["summary"] = BuildPromptItemSummary(item);
            token["value"] = BuildJTokenSafe(item);
            return token;
        }

        private static JToken BuildJTokenSafe(object value)
        {
            if (value == null)
            {
                return JValue.CreateNull();
            }

            if (value is string text)
            {
                return text;
            }

            try
            {
                return JToken.FromObject(value);
            }
            catch
            {
                return value.ToString() ?? string.Empty;
            }
        }

        private static JToken ParseDebugJsonTokenOrString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            try
            {
                return JToken.Parse(value);
            }
            catch
            {
                return value;
            }
        }

        /// <summary>
        /// Gets this conversation's live transport estimate, or estimates retained
        /// history plus agent instructions/tools before a request is available.
        /// </summary>
        internal int EstimateContextUsageTokens(AIThread thread, string modelId)
        {
            if (thread == null) return 0;
            int baseTokens = _promptBudgetProfile != null &&
                string.Equals(_promptBudgetProfile.ModelId, modelId, StringComparison.OrdinalIgnoreCase)
                ? _promptBudgetProfile.SystemPromptTokens + _promptBudgetProfile.ToolSchemaTokens
                : ChatTokenEstimator.EstimateTokens(Agent?.Instructions) + EstimateToolSchemaTokens(Agent?.Kernel);
            return thread.EstimateContextUsageTokens(modelId, baseTokens);
        }

        /// <summary>Estimates injected tool definitions for preflight budgeting.</summary>
        private static int EstimateToolSchemaTokens(Kernel kernel)
        {
            if (kernel == null)
            {
                return 0;
            }

            try
            {
                StringBuilder schema = new StringBuilder();
                if (kernel.Plugins is not System.Collections.IEnumerable plugins)
                {
                    return 0;
                }

                foreach (object plugin in plugins)
                {
                    if (plugin == null)
                    {
                        continue;
                    }

                    string pluginName = TryReadStringProperty(plugin, "Name");
                    if (!string.IsNullOrWhiteSpace(pluginName))
                    {
                        schema.Append("plugin ");
                        schema.Append(pluginName);
                        schema.AppendLine();
                    }

                    if (plugin is not System.Collections.IEnumerable functions)
                    {
                        continue;
                    }

                    foreach (object function in functions)
                    {
                        object metadata = TryGetPropertyValue(function, "Metadata") ?? function;
                        string functionName = TryReadStringProperty(metadata, "Name");
                        if (string.IsNullOrWhiteSpace(functionName))
                        {
                            continue;
                        }

                        schema.Append("- function ");
                        schema.Append(functionName);

                        string description = TryReadStringProperty(metadata, "Description");
                        if (!string.IsNullOrWhiteSpace(description))
                        {
                            schema.Append(": ");
                            schema.Append(description);
                        }

                        schema.AppendLine();

                        if (TryGetPropertyValue(metadata, "Parameters") is not System.Collections.IEnumerable parameters)
                        {
                            continue;
                        }

                        foreach (object parameter in parameters)
                        {
                            string parameterName = TryReadStringProperty(parameter, "Name");
                            if (string.IsNullOrWhiteSpace(parameterName))
                            {
                                continue;
                            }

                            schema.Append("  - ");
                            schema.Append(parameterName);

                            string parameterType =
                                TryReadStringProperty(parameter, "ParameterType") ??
                                TryReadStringProperty(parameter, "Type");
                            if (!string.IsNullOrWhiteSpace(parameterType))
                            {
                                schema.Append(" (");
                                schema.Append(parameterType);
                                schema.Append(")");
                            }

                            string parameterDescription = TryReadStringProperty(parameter, "Description");
                            if (!string.IsNullOrWhiteSpace(parameterDescription))
                            {
                                schema.Append(": ");
                                schema.Append(parameterDescription);
                            }

                            schema.AppendLine();
                        }
                    }
                }

                int estimatedTokens = ChatTokenEstimator.EstimateTokens(schema.ToString());
                Log.Info($"Tool schema tokens estimated: tokens={estimatedTokens}");
                return estimatedTokens;
            }
            catch (Exception ex)
            {
                Log.Warning($"Tool schema token estimation failed: {ex.Message}");
                return 0;
            }
        }

        private static object TryGetPropertyValue(object instance, string propertyName)
        {
            if (instance == null || string.IsNullOrWhiteSpace(propertyName))
            {
                return null;
            }

            try
            {
                var property = instance.GetType().GetProperty(propertyName);
                return property?.GetValue(instance, null);
            }
            catch
            {
                return null;
            }
        }

        private static string TryReadStringProperty(object instance, string propertyName)
        {
            object value = TryGetPropertyValue(instance, propertyName);
            return value?.ToString() ?? string.Empty;
        }

        private void EnsurePromptBudgetProfile(string modelId)
        {
            if (_promptBudgetProfile != null &&
                string.Equals(_promptBudgetProfile.ModelId, modelId ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            int toolSchemaTokens = EstimateToolSchemaTokens(Agent?.Kernel);
            _promptBudgetProfile = PromptBudgetPlanner.BuildProfile(modelId, Agent?.Instructions ?? string.Empty, toolSchemaTokens);
        }

        private static int CalculateRuntimeToolBudgetTokens(PromptBudget budget)
        {
            if (budget == null)
            {
                return 0;
            }

            return Math.Max(0, budget.ToolReturnBudgetTokens);
        }

        /// <summary>
        /// Injects functionality plugins into the Semantic Kernel routing pipeline.
        /// </summary>
        private static void RegisterPlugin<TPlugin>(
            Kernel kernel,
            string pluginName,
            bool hasResolvedContextWindowTokens,
            int resolvedContextWindowTokens,
            List<string> registeredPlugins)
            where TPlugin : class
        {
            if (kernel == null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            Type pluginType = typeof(TPlugin);
            MethodInfo[] kernelFunctionMethods = pluginType
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(method => method.GetCustomAttribute<KernelFunctionAttribute>() != null)
                .OrderBy(method => method.MetadataToken)
                .ToArray();

            if (kernelFunctionMethods.Length == 0)
            {
                return;
            }

            object pluginInstance = null;
            if (kernelFunctionMethods.Any(method => !method.IsStatic))
            {
                pluginInstance = kernel.Services != null
                    ? ActivatorUtilities.CreateInstance(kernel.Services, pluginType)
                    : Activator.CreateInstance(pluginType);
            }

            List<KernelFunction> functions = new List<KernelFunction>(kernelFunctionMethods.Length);
            List<string> skippedFunctions = new List<string>();
            foreach (MethodInfo method in kernelFunctionMethods)
            {
                if (!ShouldRegisterToolForContextWindow(
                        method,
                        hasResolvedContextWindowTokens,
                        resolvedContextWindowTokens,
                        out int minContextWindowTokens))
                {
                    skippedFunctions.Add($"{ResolveKernelFunctionName(method)}[min:{minContextWindowTokens}]");
                    continue;
                }

                object target = method.IsStatic ? null : pluginInstance;
                KernelFunction function = KernelFunctionFactory.CreateFromMethod(
                    method,
                    target,
                    functionName: null,
                    description: null,
                    parameters: null,
                    returnParameter: null,
                    loggerFactory: null);
                functions.Add(function);
            }

            if (functions.Count == 0)
            {
                string contextWindowText = hasResolvedContextWindowTokens
                    ? resolvedContextWindowTokens.ToString()
                    : "<unknown>";
                Log.Info($"Skipping plugin registration because no kernel functions matched minimum context-window requirements: plugin={pluginName}, modelContextWindow={contextWindowText}, pluginType={pluginType.Name}");
                return;
            }

            KernelPlugin plugin = KernelPluginFactory.CreateFromFunctions(pluginName, functions);
            kernel.Plugins.Add(plugin);
            registeredPlugins?.Add(pluginName);

            if (skippedFunctions.Count > 0)
            {
                string contextWindowText = hasResolvedContextWindowTokens
                    ? resolvedContextWindowTokens.ToString()
                    : "<unknown>";
                Log.Info(
                    $"Skipped minimum-context constrained tool registration: plugin={pluginName}, modelContextWindow={contextWindowText}, skippedFunctions=[{string.Join(", ", skippedFunctions)}]");
            }
        }

        private static bool ShouldRegisterToolForContextWindow(
            MethodInfo method,
            bool hasResolvedContextWindowTokens,
            int resolvedContextWindowTokens,
            out int minContextWindowTokens)
        {
            minContextWindowTokens = 0;

            if (method == null)
            {
                return true;
            }

            ContextWindowTokenAttribute contextWindowTokenAttribute = method.GetCustomAttribute<ContextWindowTokenAttribute>();
            if (contextWindowTokenAttribute == null)
            {
                return true;
            }

            minContextWindowTokens = contextWindowTokenAttribute.MinTokens;

            if (!hasResolvedContextWindowTokens || resolvedContextWindowTokens <= 0)
            {
                return true;
            }

            return resolvedContextWindowTokens >= minContextWindowTokens;
        }

        private static string ResolveKernelFunctionName(MethodInfo method)
        {
            if (method == null)
            {
                return "<unknown>";
            }

            KernelFunctionAttribute kernelFunction = method.GetCustomAttribute<KernelFunctionAttribute>();
            if (!string.IsNullOrWhiteSpace(kernelFunction?.Name))
            {
                return kernelFunction.Name.Trim();
            }

            return method.Name;
        }

        private static void ConfigureKernelLogging(IKernelBuilder builder)
        {
            if (builder?.Services == null)
            {
                return;
            }

            builder.Services.AddLogging(logging =>
            {
                logging.AddProvider(SemanticKernelLoggerBridge.CreateProvider());
                logging.SetMinimumLevel(SemanticKernelLoggerBridge.GetMinimumLevelFromSettings());
            });
        }

        private IChatCompletionService GetChatCompletionServiceOrThrow()
        {
            IChatCompletionService chatCompletionService =
                Agent?.Kernel?.Services?.GetService(typeof(IChatCompletionService)) as IChatCompletionService;

            if (chatCompletionService == null)
            {
                throw new InvalidOperationException("IChatCompletionService is not available for chat history summarization.");
            }

            return chatCompletionService;
        }

        private static bool IsLikelyContextWindowExceeded(Exception exception)
        {
            Exception current = exception;
            while (current != null)
            {
                string message = current.Message ?? string.Empty;
                if (message.IndexOf("exceeds the available context size", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    message.IndexOf("maximum context length", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    message.IndexOf("context window", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }

                current = current.InnerException;
            }

            return false;
        }

        internal static string GetConfiguredEndpointInfo(SupportedProviders provider)
        {
            if (provider == SupportedProviders.Local)
            {
                return NormalizeLocalOpenAiCompatibleEndpoint(Settings.Default.localModelEndpoint);
            }

            if (provider == SupportedProviders.OpenAI)
            {
                return "https://api.openai.com/v1";
            }

            return "<unknown>";
        }

        /// <summary>
        /// Resolves the configured local OpenAI-compatible endpoint and falls back to the LM Studio default if none is stored.
        /// </summary>
        internal static Uri GetConfiguredLocalEndpointUri()
        {
            string normalizedEndpoint = NormalizeLocalOpenAiCompatibleEndpoint(Settings.Default.localModelEndpoint);
            if (!Uri.TryCreate(normalizedEndpoint, UriKind.Absolute, out Uri endpoint))
            {
                throw new InvalidOperationException($"Local model endpoint '{normalizedEndpoint}' is not a valid absolute URI.");
            }

            if (!string.Equals(endpoint.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(endpoint.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Local model endpoint must use http or https.");
            }

            return endpoint;
        }

        /// <summary>
        /// Accepts either a bare local server URL or an explicit OpenAI-compatible /v1 base URL.
        /// </summary>
        internal static string NormalizeLocalOpenAiCompatibleEndpoint(string configuredEndpoint)
        {
            string fallbackOrConfigured = string.IsNullOrWhiteSpace(configuredEndpoint)
                ? DefaultLocalOpenAiCompatibleEndpoint
                : configuredEndpoint.Trim();

            if (!Uri.TryCreate(fallbackOrConfigured, UriKind.Absolute, out Uri parsedUri))
            {
                return fallbackOrConfigured;
            }

            string absolutePath = parsedUri.AbsolutePath ?? string.Empty;
            bool pathNeedsV1 =
                string.IsNullOrWhiteSpace(absolutePath) ||
                string.Equals(absolutePath, "/", StringComparison.Ordinal);

            if (pathNeedsV1 || string.Equals(absolutePath, "/v1/", StringComparison.OrdinalIgnoreCase))
            {
                UriBuilder uriBuilder = new UriBuilder(parsedUri)
                {
                    Path = "/v1"
                };

                return uriBuilder.Uri.ToString().TrimEnd('/');
            }

            return fallbackOrConfigured.TrimEnd('/');
        }

        internal void ReportToolActivityStarted(string callId, string pluginName, string functionName, string arguments)
        {
            ToolActivitySessionState session = CurrentToolActivitySession.Value;
            if (session?.Thread == null || string.IsNullOrWhiteSpace(session.SessionId))
            {
                return;
            }

            session.Thread.RecordToolActivityStarted(
                session.SessionId,
                callId,
                pluginName,
                functionName,
                arguments);
            ToolActivityChanged?.Invoke(this, EventArgs.Empty);
        }

        internal void ReportToolActivityFinished(
            string callId,
            string pluginName,
            string functionName,
            ToolActivityStatus status,
            string resultText)
        {
            ToolActivitySessionState session = CurrentToolActivitySession.Value;
            if (session?.Thread == null || string.IsNullOrWhiteSpace(session.SessionId))
            {
                return;
            }

            session.Thread.RecordToolActivityFinished(
                session.SessionId,
                callId,
                pluginName,
                functionName,
                status,
                resultText);
            ToolActivityChanged?.Invoke(this, EventArgs.Empty);
        }

        private IDisposable EnterToolActivitySession(AIThread thread)
        {
            if (thread == null)
            {
                CurrentToolActivitySession.Value = null;
                return new ToolActivitySessionScope(null);
            }

            string sessionId = Guid.NewGuid().ToString("N");
            thread.BeginToolActivitySession(sessionId);
            CurrentToolActivitySession.Value = new ToolActivitySessionState
            {
                SessionId = sessionId,
                Thread = thread
            };
            ToolActivityChanged?.Invoke(this, EventArgs.Empty);
            return new ToolActivitySessionScope(() => CurrentToolActivitySession.Value = null);
        }

        private void FinalizeToolActivitySession(ToolActivitySessionCompletion completion)
        {
            ToolActivitySessionState session = CurrentToolActivitySession.Value;
            if (session?.Thread == null || string.IsNullOrWhiteSpace(session.SessionId))
            {
                return;
            }

            session.Thread.FinalizeToolActivitySession(session.SessionId, completion);
            ToolActivityChanged?.Invoke(this, EventArgs.Empty);
        }

        internal void AttachLastCompletedToolActivityToLatestAssistantMessage()
        {
            AttachLastCompletedToolActivityToLatestAssistantMessage(ActiveThread);
        }

        internal void AttachLastCompletedToolActivityToLatestAssistantMessage(AIThread thread)
        {
            if (thread?.ChatHistory == null)
            {
                return;
            }

            int assistantMessageOrdinal = FindLatestVisibleAssistantMessageOrdinal(thread.ChatHistory);
            if (assistantMessageOrdinal <= 0)
            {
                return;
            }

            thread.AttachLastCompletedToolActivitiesToAssistantMessage(assistantMessageOrdinal);
            ToolActivityChanged?.Invoke(this, EventArgs.Empty);
        }

        private static int FindLatestVisibleAssistantMessageOrdinal(IReadOnlyList<ChatMessageContent> chatHistory)
        {
            if (chatHistory == null || chatHistory.Count == 0)
            {
                return 0;
            }

            int assistantOrdinal = 0;
            int latestAssistantOrdinal = 0;
            for (int i = 0; i < chatHistory.Count; i++)
            {
                ChatMessageContent message = chatHistory[i];
                if (!IsVisibleAssistantChatMessage(message))
                {
                    continue;
                }

                assistantOrdinal++;
                latestAssistantOrdinal = assistantOrdinal;
            }

            return latestAssistantOrdinal;
        }

        private static bool IsVisibleAssistantChatMessage(ChatMessageContent message)
        {
            if (message == null || message.Role != AuthorRole.Assistant)
            {
                return false;
            }

            string text = AssistantResponseText.GetVisibleText(message.Content);
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            string trimmed = text.TrimStart();
            if (trimmed.StartsWith("[SK][", StringComparison.Ordinal))
            {
                return false;
            }

            if (trimmed.StartsWith("LLM-get_open_editors:", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        private static string[] SplitStoredModelIds(string rawModelIds)
        {
            if (string.IsNullOrWhiteSpace(rawModelIds))
            {
                return Array.Empty<string>();
            }

            return rawModelIds
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(modelId => modelId.Trim())
                .Where(modelId => modelId.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private sealed class ToolActivitySessionState
        {
            public string SessionId { get; set; }
            public AIThread Thread { get; set; }
        }

        private sealed class ToolActivitySessionScope : IDisposable
        {
            private readonly Action _onDispose;
            private bool _disposed;

            public ToolActivitySessionScope(Action onDispose)
            {
                _onDispose = onDispose;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                _onDispose?.Invoke();
            }
        }

        private static string BuildUserFacingInvocationErrorMessage(
            SupportedProviders provider,
            string modelId,
            string endpointInfo,
            Exception exception,
            bool requestWasCancelledByClient)
        {
            string safeModelId = string.IsNullOrWhiteSpace(modelId) ? Resources.AiChatUnknownModel : modelId;
            string failureClass = DescribeInvocationFailure(provider, safeModelId, exception, requestWasCancelledByClient);

            return string.Format(CultureInfo.CurrentCulture,
                Resources.AiChatInvocationError, provider, safeModelId, endpointInfo, failureClass);
        }

        private static string DescribeInvocationFailure(
            SupportedProviders provider,
            string modelId,
            Exception exception,
            bool requestWasCancelledByClient)
        {
            if (requestWasCancelledByClient)
            {
                return Resources.AiChatRequestCancelled;
            }

            if (provider == SupportedProviders.Local && IsLikelyConnectionFailure(exception))
            {
                return Resources.AiChatLocalEndpointUnreachable;
            }

            if (provider == SupportedProviders.OpenAI && IsLikelyAuthenticationFailure(exception))
            {
                return Resources.AiChatAuthenticationFailed;
            }

            if (provider == SupportedProviders.OpenAI && IsLikelyRateLimitFailure(exception))
            {
                return Resources.AiChatRateLimitReached;
            }

            if (IsLikelyTimeoutFailure(exception))
            {
                return Resources.AiChatResponseTimeout;
            }

            if (IsLikelyTransportCancellation(exception))
            {
                if (provider == SupportedProviders.Local && IsLikelyQwenThinkingModel(modelId))
                {
                    return Resources.AiChatQwenStreamInterrupted;
                }

                return Resources.AiChatStreamInterrupted;
            }

            return GetInnermostExceptionMessage(exception);
        }

        private static bool IsLikelyQwenThinkingModel(string modelId)
        {
            if (string.IsNullOrWhiteSpace(modelId))
            {
                return false;
            }

            return modelId.IndexOf("qwen", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsLikelyConnectionFailure(Exception exception)
        {
            return ExceptionChainContainsAny(
                exception,
                "unable to connect to the remote server",
                "connection refused",
                "actively refused",
                "no connection could be made",
                "name or service not known",
                "nodename nor servname provided",
                "could not connect",
                "socketexception");
        }

        private static bool IsLikelyAuthenticationFailure(Exception exception)
        {
            return ExceptionChainContainsAny(
                exception,
                "401",
                "unauthorized",
                "invalid api key",
                "api key is missing",
                "authentication failed",
                "incorrect api key");
        }

        private static bool IsLikelyRateLimitFailure(Exception exception)
        {
            return ExceptionChainContainsAny(
                exception,
                "429",
                "rate limit",
                "too many requests",
                "quota");
        }

        private static bool IsLikelyTimeoutFailure(Exception exception)
        {
            return ExceptionChainContainsAny(
                exception,
                "timeout",
                "timed out");
        }

        private static bool IsLikelyTransportCancellation(Exception exception)
        {
            Exception current = exception;
            while (current != null)
            {
                if (current is OperationCanceledException || current is TaskCanceledException)
                {
                    return true;
                }

                current = current.InnerException;
            }

            return ExceptionChainContainsAny(
                exception,
                "operation was canceled",
                "a task was canceled",
                "client disconnected");
        }

        private static bool ExceptionChainContainsAny(Exception exception, params string[] patterns)
        {
            if (exception == null || patterns == null || patterns.Length == 0)
            {
                return false;
            }

            Exception current = exception;
            while (current != null)
            {
                string text = $"{current.GetType().FullName} {current.Message}";
                foreach (string pattern in patterns)
                {
                    if (!string.IsNullOrWhiteSpace(pattern) &&
                        text.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }

                current = current.InnerException;
            }

            return false;
        }

        private static string BuildExceptionMessageChain(Exception exception)
        {
            if (exception == null)
            {
                return "<none>";
            }

            List<string> parts = new List<string>(4);
            int depth = 0;
            Exception current = exception;
            while (current != null)
            {
                string message = string.IsNullOrWhiteSpace(current.Message) ? "<empty>" : current.Message.Trim();
                parts.Add($"[{depth}] {current.GetType().Name}: {message}");
                current = current.InnerException;
                depth++;
            }

            return string.Join(" | ", parts);
        }

        private static string GetInnermostExceptionMessage(Exception exception)
        {
            if (exception == null)
            {
                return Resources.AiChatUnknownError;
            }

            Exception current = exception;
            while (current.InnerException != null)
            {
                current = current.InnerException;
            }

            return string.IsNullOrWhiteSpace(current.Message) ? Resources.AiChatUnknownError : current.Message.Trim();
        }

        /// <summary>
        /// Interrogates the active graphical interface surface (CrypWin) to pin the current user request
        /// to a specific application tab. Essential for contextualizing operations within a multi-document UI.
        /// </summary>
        private RequestWorkspaceContext CaptureRequestWorkspaceContext(AIThread requestThread)
        {
            string openTabsSnapshotJson = "[]";
            string activeTabId = null;

            try
            {
                openTabsSnapshotJson = GetOpenTabsSnapshotFromBridge();
                activeTabId = TryExtractActiveTabId(openTabsSnapshotJson);
            }
            catch (Exception ex)
            {
                Log.Warning($"CaptureRequestWorkspaceContext failed: {ex.Message}");
            }

            _cachedOpenTabsSnapshotJson = string.IsNullOrWhiteSpace(openTabsSnapshotJson) ? "[]" : openTabsSnapshotJson;

            LastPinnedWorkspaceTabId = activeTabId;

            if (requestThread != null)
            {
                requestThread.LastPinnedWorkspaceTabId = LastPinnedWorkspaceTabId;
            }

            return new RequestWorkspaceContext
            {
                ActiveTabId = LastPinnedWorkspaceTabId
            };
        }

        internal bool TryGetCachedOpenTabsSnapshot(out string openTabsSnapshotJson)
        {
            openTabsSnapshotJson = _cachedOpenTabsSnapshotJson;
            return !string.IsNullOrWhiteSpace(openTabsSnapshotJson);
        }

        private static string GetOpenTabsSnapshotFromBridge()
        {
            return LLMPluginService.InvokeOnUi(() =>
            {
                ICrypWinAdapter bridge = CrypWinPort.Instance;
                if (bridge == null)
                {
                    return "[]";
                }

                IEnumerable<OpenTabsAbstraction> openTabs = bridge.GetOpenTabs() ?? Enumerable.Empty<OpenTabsAbstraction>();
                return JsonConvert.SerializeObject(openTabs, Formatting.Indented);
            });
        }

        private static string TryExtractActiveTabId(string openTabsSnapshotJson)
        {
            if (string.IsNullOrWhiteSpace(openTabsSnapshotJson))
            {
                return null;
            }

            try
            {
                JArray openTabs = JArray.Parse(openTabsSnapshotJson);
                JObject activeTab = openTabs
                    .OfType<JObject>()
                    .FirstOrDefault(tab => tab["IsActive"]?.Value<bool>() == true);

                return activeTab?["Id"]?.Value<string>();
            }
            catch (Exception ex)
            {
                Log.Warning($"TryExtractActiveTabId failed: {ex.Message}");
                return null;
            }
        }

        private void ClearRequestWorkspaceContext(AIThread requestThread)
        {
            _cachedOpenTabsSnapshotJson = null;
            LastPinnedWorkspaceTabId = null;

            if (requestThread != null)
            {
                requestThread.LastPinnedWorkspaceTabId = null;
            }
        }

        private sealed class RequestWorkspaceContext
        {
            public string ActiveTabId { get; set; }
        }

        private void CreateNewAgentIfNeccessary(string selectedModel)
        {
            SupportedProviders supportedProvider = ProviderToStringConverter.Convert(Settings.Default.selectedProvider);

            if (!AgentPropertysChanged(selectedModel))
            {
                return;
            }

            try
            {
                CreateNewAgent(supportedProvider, selectedModel);
                AgentsPropertysChanged = false;
                Log.Info("Created new agent instance.");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to create agent for model '{selectedModel}': {ex.Message}");
                throw;
            }
        }

        private bool AgentPropertysChanged(string newModel)
        {
            try
            {
                if (Agent == null)
                {
                    Log.Debug("AgentPropertysChanged: Agent is null");
                    return true;
                }

                if (AgentsPropertysChanged)
                {
                    Log.Debug("AgentPropertysChanged: AgentsPropertysChanged flag is set");
                    return true;
                }

                if (!string.Equals(GetAgentModelId(), newModel, StringComparison.OrdinalIgnoreCase))
                {
                    Log.Debug("AgentPropertysChanged: Model ID changed");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Warning(string.Format("AgentPropertysChanged failed, recreating agent: {0}", ex.Message));
                return true;
            }
        }

        /// <summary>
        /// Bootstraps a human-readable alias for newly instantiated threads by dissecting the initial user utterance.
        /// Limits length to maintain coherent UI list presentations.
        /// </summary>
        private static bool TryAssignThreadNameFromUserInput(AIThread thread, string userInput)
        {
            if (thread == null)
            {
                return false;
            }

            var history = thread.ChatHistory;
            if (history == null)
            {
                return false;
            }

            bool hasAnyUserMessage = history.Any(m => m.Role == AuthorRole.User);
            if (hasAnyUserMessage)
            {
                return false;
            }

            string firstLine = userInput;
            if (!string.IsNullOrEmpty(userInput))
            {
                var lines = userInput.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        firstLine = line.Trim();
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(firstLine))
            {
                return false;
            }

            const int maxLen = 60;
            if (firstLine.Length > maxLen)
            {
                firstLine = firstLine.Substring(0, maxLen).TrimEnd() + "...";
            }

            thread.Name = firstLine;
            return true;
        }

        private void OnThreadListChanged()
        {
            ThreadListChanged?.Invoke(this, EventArgs.Empty);
        }

        #region Persistence (only histories)

        /// <summary>
        /// Invalidate cached context after a user undoes an agent step so stale tool results are re-checked.
        /// </summary>
        internal void RecordAgentUndo(AIThread thread, string workspaceTitle)
        {
            if (thread == null) return;
            thread.ClearReducedHistoryCache();
            string format = Properties.Resources.ResourceManager.GetString("AiChatAgentUndoContextNotice");
            thread.ChatHistory.AddUserMessage(string.Format(format, workspaceTitle));
            SaveThreadHistoriesSafe();
            ToolActivityChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Serialize and encrypt chat histories using DPAPI.</summary>
        private void SaveThreadHistoriesSafe()
        {
            try
            {
                var persistedThreads = AIThreads
                    .Where(t => t.ChatHistory != null && t.ChatHistory.Any(m => m.Role == AuthorRole.User))
                    .ToList();
                var dto = persistedThreads.Select(t => new ThreadHistoryDto
                    {
                        Name = t.Name,
                        Messages = ChatHistoryPersistence.SerializeMessages(t.ChatHistory),
                        ReducedSourceMessageCount = Math.Max(0, t.ReducedHistoryCacheSourceMessageCount),
                        ReducedMessages = ChatHistoryPersistence.SerializeMessages(t.ReducedHistoryCacheMessages)
                    })
                    .ToList();

                var wrapper = new PersistedState
                {
                    ActiveIndex = Math.Max(0, persistedThreads.IndexOf(ActiveThread)),
                    Threads = dto
                };

                string json = JsonConvert.SerializeObject(wrapper);
                string encryptedJson = SecretProtector.EncryptToBase64(json);
                File.WriteAllText(_historyPath, encryptedJson, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Log.Warning($"Saving thread histories failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Deserializes persistent chat metadata to restore environment continuity between application launches.
        /// Gently swallows corruption exceptions to favor UI stability if historical JSON traces are illegible.
        /// </summary>
        private void LoadThreadHistoriesSafe()
        {
            try
            {
                if (!File.Exists(_historyPath))
                {
                    return;
                }

                string encryptedJson = File.ReadAllText(_historyPath, Encoding.UTF8);
                string json = SecretProtector.TryDecryptOrReturn(encryptedJson);
                var wrapper = JsonConvert.DeserializeObject<PersistedState>(json);
                if (wrapper?.Threads == null || wrapper.Threads.Count == 0)
                {
                    return;
                }

                AIThreads.Clear();
                foreach (var dto in wrapper.Threads)
                {
                    var thread = new AIThread(string.IsNullOrWhiteSpace(dto.Name) ? Resources.NewChat : dto.Name)
                    {
                        AgentThread = new ChatHistoryAgentThread()
                    };

                    foreach (ChatMessageContent message in ChatHistoryPersistence.DeserializeMessages(dto.Messages))
                    {
                        thread.AgentThread.ChatHistory.Add(message);
                    }
                    if (thread.RemoveSyntheticGreetingMessage())
                    {
                        thread.ClearReducedHistoryCache();
                    }

                    if (dto.ReducedMessages != null && dto.ReducedMessages.Count > 0 && dto.ReducedSourceMessageCount > 0)
                    {
                        List<ChatMessageContent> reduced = ChatHistoryPersistence.DeserializeMessages(dto.ReducedMessages)
                            .Where(msg => !AIThread.IsSyntheticGreetingMessage(msg))
                            .ToList();
                        thread.SetReducedHistoryCache(reduced, dto.ReducedSourceMessageCount);
                    }
                    else
                    {
                        thread.ClearReducedHistoryCache();
                    }

                    AIThreads.Add(thread);
                }

                _activeThreadIndex = Math.Min(Math.Max(wrapper.ActiveIndex, 0), AIThreads.Count - 1);
                OnThreadListChanged();
                ActiveThread = AIThreads.ElementAtOrDefault(_activeThreadIndex) ?? AIThreads.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Log.Warning($"Loading thread histories failed: {ex.Message}");
            }
        }

        private static AuthorRole ParseRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return AuthorRole.Assistant;
            if (role.Equals(nameof(AuthorRole.User), StringComparison.OrdinalIgnoreCase))
                return AuthorRole.User;
            if (role.Equals(nameof(AuthorRole.Assistant), StringComparison.OrdinalIgnoreCase))
                return AuthorRole.Assistant;
            if (role.Equals(nameof(AuthorRole.System), StringComparison.OrdinalIgnoreCase))
                return AuthorRole.System;
            if (role.Equals(nameof(AuthorRole.Tool), StringComparison.OrdinalIgnoreCase))
                return AuthorRole.Tool;

            return AuthorRole.Assistant;
        }

        private sealed class ThreadHistoryDto
        {
            public string Name { get; set; }
            public List<PersistedChatMessage> Messages { get; set; }
            public int ReducedSourceMessageCount { get; set; }
            public List<PersistedChatMessage> ReducedMessages { get; set; }
        }

        private sealed class PersistedState
        {
            public int ActiveIndex { get; set; }
            public List<ThreadHistoryDto> Threads { get; set; }
        }

        #endregion
    }
}
