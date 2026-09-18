// Regression tests run against the built agent assembly. Provider responses and UI
// adapters are local doubles; no credentials, network or user chat files are used.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CrypTool.CrypLLM.Ports;
using CrypTool.CrypLLM.Threads;
using CrypTool.PluginBase.Editor;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

internal static class AgentRegressionTests
{
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly Assembly AgentAssembly = typeof(AIThread).Assembly;
    private static readonly string TestDirectory = Path.Combine(Path.GetTempPath(), "CT2-AgentTests-" + Guid.NewGuid());

    [STAThread]
    public static int Main()
    {
        Directory.CreateDirectory(TestDirectory);
        try
        {
            FirstStartLoadsInstructions();
            HistoryPreservesToolMessages();
            ClosedWorkspaceDoesNotRedirect();
            RequestStaysInOriginalChat().GetAwaiter().GetResult();
            ReasoningAndToolEchoesStayOutOfAnswers().GetAwaiter().GetResult();
            FailedAgentReloadRemainsDirty();
            LocalProviderUsesItsOwnApiKey().GetAwaiter().GetResult();
            ScreenshotCaptureAndPersistence();
            ScreenshotToolReachesModelAsImage().GetAwaiter().GetResult();
            ConfigurableCompressionPreservesFullHistory().GetAwaiter().GetResult();
            ContextUsageFollowsCompressedHistory();
            AiMemoCreationPreservesSelectionAndFocus();
            ResizeToolsPreserveWorkspaceAndSupportUndo();
            MemoFitAndLayoutChecks();
            WireClearanceIncludesAttachedComponents();
            AgentGroupsPreserveManualChanges();
            TemplateSearchUsesBothLanguages();
            LocalizedResourcesResolve();
            Console.WriteLine("All agent regression tests passed.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
        finally
        {
            Directory.Delete(TestDirectory, true);
        }
    }

    private static Type AgentType(string name) { return AgentAssembly.GetType("CrypTool.CrypLLM." + name, true); }
    private static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static object Call(object target, string method, params object[] arguments)
    {
        return target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(target, arguments);
    }
    private static object CallStatic(string type, string method, params object[] arguments)
    {
        return AgentType(type).GetMethod(method, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Invoke(null, arguments);
    }

    private static void FirstStartLoadsInstructions()
    {
        Type type = AgentType("AgentInstructions.InstructionManager");
        object manager = FormatterServices.GetUninitializedObject(type);
        FieldInfo entries = type.GetField("_entries", PrivateInstance);
        entries.SetValue(manager, Activator.CreateInstance(entries.FieldType));
        string path = Path.Combine(TestDirectory, "instructions.json");
        type.GetField("_instructionsPath", PrivateInstance).SetValue(manager, path);
        Call(manager, "LoadOrInitialize");
        Assert(((ICollection)entries.GetValue(manager)).Count > 0, "Fresh profiles must load defaults.");
        Assert(!string.IsNullOrWhiteSpace((string)Call(manager, "GetInstructionText", "Default")), "Fresh profiles need a system prompt.");
        Assert(File.Exists(path), "Fresh profile defaults must be persisted.");
        Call(manager, "LoadOrInitialize");
        Assert(!string.IsNullOrWhiteSpace((string)Call(manager, "GetInstructionText", "Default")), "Persisted defaults must reload.");
        // All later code paths must use this isolated manager instead of a user profile.
        Type factoryType = typeof(Func<>).MakeGenericType(type);
        Delegate factory = System.Linq.Expressions.Expression.Lambda(factoryType,
            System.Linq.Expressions.Expression.Constant(manager, type)).Compile();
        object singleton = Activator.CreateInstance(typeof(Lazy<>).MakeGenericType(type), new object[] { factory });
        type.GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, singleton);
        Console.WriteLine("PASS: fresh instruction profile and reload");
    }

    private static void HistoryPreservesToolMessages()
    {
        var arguments = new KernelArguments { { "tabId", "workspace-a" }, { "maxLines", 5 } };
        var call = new FunctionCallContent("call-a", "CrypLLM_WorkspaceStatus", "ws_model", arguments);
        var assistant = new ChatMessageContent(AuthorRole.Assistant, (string)null);
        assistant.Items.Add(call);
        var result = new FunctionResultContent(call, "{\"components\":[\"AES\"]}").ToChatMessage();
        var history = new List<ChatMessageContent>
        {
            new ChatMessageContent(AuthorRole.User, "  keep my indentation\n"), assistant, result,
            new ChatMessageContent(AuthorRole.Assistant, "The workspace contains AES.")
        };
        object dto = CallStatic("Threads.ChatHistoryPersistence", "SerializeMessages", history);
        string json = JsonConvert.SerializeObject(dto);
        object restoredDto = JsonConvert.DeserializeObject(json, dto.GetType());
        var restored = (List<ChatMessageContent>)CallStatic("Threads.ChatHistoryPersistence", "DeserializeMessages", restoredDto);
        Assert(restored.Count == 4, "Textless tool messages must survive persistence.");
        Assert(restored[0].Content == history[0].Content, "Persistence must not trim user text.");
        FunctionCallContent restoredCall = restored[1].Items.OfType<FunctionCallContent>().Single();
        FunctionResultContent restoredResult = restored[2].Items.OfType<FunctionResultContent>().Single();
        Assert(restoredCall.Id == restoredResult.CallId && restoredCall.Id == "call-a", "Tool call/result IDs must match after reload.");
        Assert((string)restoredCall.Arguments["tabId"] == "workspace-a", "Tool arguments must reload.");
        Assert(restoredResult.Result.ToString() == "{\"components\":[\"AES\"]}", "Tool payload must reload.");
        object legacy = JsonConvert.DeserializeObject("[{\"Role\":\"User\",\"Content\":\"old chat\"}]", dto.GetType());
        restored = (List<ChatMessageContent>)CallStatic("Threads.ChatHistoryPersistence", "DeserializeMessages", legacy);
        Assert(restored.Single().Role == AuthorRole.User && restored.Single().Content == "old chat", "Legacy histories must remain readable.");
        Console.WriteLine("PASS: tool/text persistence and legacy histories");
    }

    private static void ClosedWorkspaceDoesNotRedirect()
    {
        var adapter = new ClosedWorkspaceAdapter();
        CrypWinPort.Instance = adapter;
        using ((IDisposable)CallStatic("Services.LLMPluginService", "PushPreferredWorkspaceTabId", "closed-tab"))
        {
            MethodInfo method = AgentType("Services.LLMPluginService").GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(item => item.Name == "TryGetWorkspaceEditor" && item.GetParameters().Length == 2);
            for (int attempt = 0; attempt < 2; attempt++)
            {
                Assert(!(bool)method.Invoke(null, new object[] { null, null }), "A closed pinned workspace must stay unavailable.");
            }
            Assert(adapter.ActiveEditorReads == 0, "Closed pinned targets must not fall back to the active editor.");
            object statusPlugin = Activator.CreateInstance(AgentType("WorkspaceStatusPlugin"), true);
            string screenshot = (string)Call(statusPlugin, "GetWorkspaceScreenshot", (object)null);
            Assert(screenshot.Contains("error") && adapter.ActiveEditorReads == 0, "Screenshot tools must also fail closed on stale pins.");
        }
        Console.WriteLine("PASS: closed workspace never redirects subsequent tools");
    }

    private static object NewManager()
    {
        Type type = AgentType("Threads.AIThreadManager");
        object manager = FormatterServices.GetUninitializedObject(type);
        type.GetField("<AIThreads>k__BackingField", PrivateInstance).SetValue(manager, new List<AIThread>());
        type.GetField("_historyPath", PrivateInstance).SetValue(manager, Path.Combine(TestDirectory, "history.json"));
        type.GetField("_historyReductionEngine", PrivateInstance).SetValue(manager, Activator.CreateInstance(AgentType("Threads.ChatHistoryReductionEngine"), true));
        return manager;
    }

    private static async Task RequestStaysInOriginalChat()
    {
        object manager = NewManager();
        var handler = new DelayedCompletionHandler();
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("gpt-5.4-mini", new Uri("http://localhost:1234/v1"), "test-only", null, null, new HttpClient(handler));
        var agent = new ChatCompletionAgent { Kernel = builder.Build(), Instructions = "Respond briefly." };
        manager.GetType().GetProperty("Agent").SetValue(manager, agent, null);
        var original = new AIThread("Original");
        var other = new AIThread("Other") { LastPinnedWorkspaceTabId = "other-workspace" };
        var threads = (List<AIThread>)manager.GetType().GetProperty("AIThreads").GetValue(manager, null);
        threads.Add(original);
        threads.Add(other);
        manager.GetType().GetProperty("ActiveThread").SetValue(manager, original, null);
        var task = (Task<string>)Call(manager, "CreateAsync", "gpt-5.4-mini", "test request", CancellationToken.None);
        Assert(await Task.WhenAny(handler.Started.Task, task, Task.Delay(10000)) == handler.Started.Task, "The request must reach the fake provider.");
        manager.GetType().GetProperty("ActiveThread").SetValue(manager, other, null);
        handler.Release();
        await task;
        Assert(original.ChatHistory.Any(message => message.Content == "test reply"), "The response belongs to the original chat.");
        Assert(other.ChatHistory.Count == 0 && other.LastInvocationDebugInfo == null, "Changing selection must not contaminate the other chat.");
        Assert(other.LastPinnedWorkspaceTabId == "other-workspace", "Request cleanup must not clear another chat's workspace context.");
        Assert(original.LastInvocationDebugInfo != null, "Diagnostics belong to the original chat.");
        Console.WriteLine("PASS: request/chat switch with a delayed fake provider");
    }

    private static async Task ReasoningAndToolEchoesStayOutOfAnswers()
    {
        Func<string, string> visible = text => (string)CallStatic("Threads.AssistantResponseText", "GetVisibleText", text);
        Assert(visible("<think>Private thought</think>Done.") == "Done.", "Marked reasoning must be hidden.");
        Assert(visible("<think>A<analysis>B</analysis>C</think>Done.") == "Done.", "Nested reasoning must stay hidden.");
        Assert(visible("Private prefix</think>Done.") == "Done.", "A closing reasoning marker must withhold the unmarked prefix.");
        Assert(visible("Done.<think>unfinished") == "Done.", "Incomplete reasoning must be withheld.");
        Assert(visible("<|channel|>analysisPrivate") == "<|channel|>analysisPrivate", "Unrecognized prose must not be guessed away.");
        Assert(visible("<|channel|>analysis<|message|>Private<|channel|>final<|message|>Done.") == "Done.", "Explicit final channels must hide analysis.");
        Assert(visible("unmarked thoughts<ct2_answer>Done.</ct2_answer>more thoughts") == "Done.", "Answer envelopes must select only user-facing text.");
        string code = "```xml\n<think>literal example</think>\n[tool-result-text] {}\n```\nUse `<ct2_answer>` for an envelope.";
        Assert(visible(code) == code, "Code examples and inline markup must remain literal.");
        string echo = "<ct2_answer>[tool-result-text] ws_move_memo: {\"nested\":{\"x\":100},\"text\":\"a } bracket\"} Done. [tool-result-text] ws_io: [1,2] <|tool_call_end|><|tool_calls_section_end|></ct2_answer>";
        Assert(visible(echo) == "Done.", "Echoed nested tool JSON and protocol tokens must not appear as answers.");
        Assert(visible("[tool-result-text] ws_move_memo: {} unmarked private thoughts and an answer <|tool_call_end|>") == "", "Mixed protocol and unmarked reasoning must be withheld without guessing answer boundaries.");
        Assert(visible("[tool-result-text] ws_bounds: {\"incomplete\":") == "", "Incomplete echoed tool data must be withheld.");
        Assert(visible("<ct2_answer><|tool_calls_section_begin|>raw call<|tool_calls_section_end|>Done.</ct2_answer>") == "Done.", "Raw textual tool-call sections must be hidden.");
        Assert(visible("[ct2-tool-memory] ws_io: useful archived data") == "", "Internal tool memory is not a visible answer.");

        var request = new JObject { ["messages"] = new JArray(
            new JObject { ["role"] = "user", ["content"] = "Explain <think> and [tool-result-text] {}" },
            new JObject { ["role"] = "assistant", ["content"] = "<think>secret</think><ct2_answer>Done.</ct2_answer>" },
            new JObject { ["role"] = "assistant", ["content"] = "<think>private</think>", ["tool_calls"] = new JArray(new JObject { ["id"] = "call-a" }) },
            new JObject { ["role"] = "tool", ["tool_call_id"] = "call-a", ["content"] = "<think>literal tool data</think>" },
            new JObject { ["role"] = "assistant", ["content"] = "[ct2-tool-memory] ws_io: archived result" }) };
        var prepared = JObject.Parse((string)CallStatic("Threads.AssistantResponseText", "PrepareRequest", request.ToString()));
        var messages = (JArray)prepared["messages"];
        Assert((string)messages[0]["content"] == (string)request["messages"][0]["content"], "User text must never be filtered.");
        Assert((string)messages[1]["content"] == "Done.", "Reasoning must not be repeated in future answer history.");
        Assert((string)messages[2]["tool_calls"][0]["id"] == "call-a" && (string)messages[2]["content"] == "", "Structured calls must survive filtering.");
        Assert((string)messages[3]["content"] == "<think>literal tool data</think>", "Actual tool results must remain untouched.");
        Assert((string)messages[4]["role"] == "user" && ((string)messages[4]["content"]).Contains("archived result"), "Archived tool data must be retained without modeling an assistant answer.");

        object manager = NewManager();
        var thread = new AIThread("Reasoning separation");
        ((List<AIThread>)manager.GetType().GetProperty("AIThreads").GetValue(manager)).Add(thread);
        manager.GetType().GetProperty("ActiveThread").SetValue(manager, thread);
        var handler = new ReasoningCompletionHandler();
        Type captureType = AgentType("Threads.AIThreadManager").GetNestedType("LlmHttpCaptureHandler", BindingFlags.NonPublic);
        var transport = (HttpMessageHandler)Activator.CreateInstance(captureType, new object[] { handler });
        using (var client = new HttpClient(transport))
        {
            var builder = Kernel.CreateBuilder();
            builder.AddOpenAIChatCompletion("gpt-5.4-mini", new Uri("http://localhost:1234/v1"), "test-only", null, null, client);
            var agent = new ChatCompletionAgent { Kernel = builder.Build(), Instructions = "Respond briefly." };
            manager.GetType().GetProperty("Agent").SetValue(manager, agent);
            string answer = await (Task<string>)Call(manager, "CreateAsync", "gpt-5.4-mini", "test request", CancellationToken.None);
            Assert(answer == "Done.", "The real SDK invocation must return only visible answer text.");
            Assert(thread.ChatHistory.Any(message => (message.Content ?? "").Contains("private-thought")), "Raw responses must remain available in original history and diagnostics.");
            handler.Content = "<think>private-only</think>";
            answer = await (Task<string>)Call(manager, "CreateAsync", "gpt-5.4-mini", "next request", CancellationToken.None);
            Assert(!answer.Contains("private-only") && answer.Length > 0, "Reasoning-only responses must produce a visible localized notice.");
            Assert(!handler.LastRequest.Contains("private-thought"), "The next actual SDK request must exclude private reasoning from prior answers.");
            handler.Content = "[tool-result-text] ws_io: {} unmarked mixed reasoning <|tool_call_end|>";
            answer = await (Task<string>)Call(manager, "CreateAsync", "gpt-5.4-mini", "retry request", CancellationToken.None);
            Assert(!answer.Contains("mixed reasoning") && answer.Length > 0, "A malformed mixed answer must yield a notice through the real SDK path.");
        }
        Console.WriteLine("PASS: reasoning/answer separation, tool echoes, literal code, SDK request cleanup and raw diagnostics");
    }

    private sealed class ReasoningCompletionHandler : HttpMessageHandler
    {
        internal string Content = "<think>private-thought</think><ct2_answer>Done.</ct2_answer>";
        internal string LastRequest;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = await request.Content.ReadAsStringAsync();
            var response = new JObject
            {
                ["id"] = "test-reasoning", ["object"] = "chat.completion", ["created"] = 1, ["model"] = "gpt-5.4-mini",
                ["choices"] = new JArray(new JObject { ["index"] = 0, ["finish_reason"] = "stop",
                    ["message"] = new JObject { ["role"] = "assistant", ["content"] = Content, ["reasoning_content"] = "separate private reasoning" } })
            };
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(response.ToString(), Encoding.UTF8, "application/json") };
        }
    }

    private static void FailedAgentReloadRemainsDirty()
    {
        object manager = NewManager();
        var originalAgent = new ChatCompletionAgent();
        manager.GetType().GetProperty("Agent").SetValue(manager, originalAgent, null);
        manager.GetType().GetField("AgentsPropertysChanged").SetValue(manager, true);
        // Only in-memory settings are changed in this short-lived test process.
        CrypTool.CrypLLM.Properties.Settings.Default.selectedProvider = "Local";
        CrypTool.CrypLLM.Properties.Settings.Default.localModelEndpoint = "invalid://endpoint";
        for (int attempt = 0; attempt < 2; attempt++)
        {
            bool failed = false;
            try { Call(manager, "CreateNewAgentIfNeccessary", "gpt-5.4-mini"); }
            catch (TargetInvocationException) { failed = true; }
            Assert(failed, "Each attempt must retry the invalid changed configuration.");
            Assert((bool)manager.GetType().GetField("AgentsPropertysChanged").GetValue(manager), "Failed reload must stay dirty.");
            Assert(ReferenceEquals(originalAgent, manager.GetType().GetProperty("Agent").GetValue(manager, null)), "Failed reload must preserve the working agent.");
        }
        CrypTool.CrypLLM.Properties.Settings.Default.localModelEndpoint = "http://localhost:1234/v1";
        try { Call(manager, "CreateNewAgentIfNeccessary", "model-without-context-window"); }
        catch (TargetInvocationException) { }
        Assert(ReferenceEquals(originalAgent, manager.GetType().GetProperty("Agent").GetValue(manager, null)), "Budget validation must finish before agent replacement.");
        Console.WriteLine("PASS: failed agent recreation remains retryable and atomic");
    }

    private static async Task LocalProviderUsesItsOwnApiKey()
    {
        var settings = CrypTool.CrypLLM.Properties.Settings.Default;
        string previousLocalKey = settings.localApiKey;
        string previousOpenAiKey = settings.apiKey;
        string previousEndpoint = settings.localModelEndpoint;
        FieldInfo clientField = AgentType("Threads.AIThreadManager").GetField("InstrumentedOpenAiHttpClient", BindingFlags.Static | BindingFlags.NonPublic);
        object previousClient = clientField.GetValue(null);
        var handler = new AuthenticationCompletionHandler();
        using (var client = new HttpClient(handler))
        {
            try
            {
                // Intercept the real SDK transport: no token leaves this process.
                clientField.SetValue(null, client);
                settings.localModelEndpoint = "http://localhost:1234/v1";
                settings.apiKey = CrypTool.CrypLLM.SecretProtector.EncryptToBase64("openai-test-token");
                using (var request = new HttpRequestMessage())
                {
                    request.Headers.TryAddWithoutValidation("Authorization", "Bearer local-test-token");
                    request.Headers.TryAddWithoutValidation("Proxy-Authorization", "Bearer proxy-test-token");
                    request.Headers.TryAddWithoutValidation("X-Test", "visible-header");
                    Type captureHandler = AgentType("Threads.AIThreadManager").GetNestedType("LlmHttpCaptureHandler", BindingFlags.NonPublic);
                    string diagnostics = (string)captureHandler.GetMethod("FormatHeaders", BindingFlags.Static | BindingFlags.NonPublic)
                        .Invoke(null, new object[] { request.Headers, null });
                    Assert(!diagnostics.Contains("local-test-token") && !diagnostics.Contains("proxy-test-token"), "Diagnostics must hide credentials.");
                    Assert(diagnostics.Contains("[redacted]") && diagnostics.Contains("visible-header"), "Redaction must preserve ordinary diagnostics.");
                }
                object manager = NewManager();
                MethodInfo createAgent = manager.GetType().GetMethod("CreateNewAgent", PrivateInstance);
                object localProvider = Enum.Parse(createAgent.GetParameters()[0].ParameterType, "Local");
                foreach (string token in new[] { "local-test-token", "local-changed-token", "" })
                {
                    settings.localApiKey = CrypTool.CrypLLM.SecretProtector.EncryptToBase64(token);
                    Assert(token.Length == 0 || !settings.localApiKey.Contains(token), "Local credentials must be encrypted at rest.");
                    Assert(CrypTool.CrypLLM.SecretProtector.TryDecryptOrReturn(settings.localApiKey) == token, "Encrypted credentials must round-trip.");
                    createAgent.Invoke(manager, new[] { localProvider, "gpt-5.4-mini" });
                    var agent = (ChatCompletionAgent)manager.GetType().GetProperty("Agent").GetValue(manager, null);
                    var history = new ChatHistory();
                    history.AddUserMessage("test authentication");
                    await agent.Kernel.GetRequiredService<IChatCompletionService>().GetChatMessageContentsAsync(history);
                    Assert(handler.Scheme == "Bearer", "The SDK must send Bearer authentication.");
                    Assert(handler.Token == (token.Length == 0 ? "no-api-key" : token), "The local provider must use its own current key or the unauthenticated placeholder.");
                }
                Console.WriteLine("PASS: encrypted local API key, SDK authentication, empty-key fallback and diagnostic redaction");
            }
            finally
            {
                clientField.SetValue(null, previousClient);
                settings.localApiKey = previousLocalKey;
                settings.apiKey = previousOpenAiKey;
                settings.localModelEndpoint = previousEndpoint;
            }
        }
    }

    private static void ScreenshotCaptureAndPersistence()
    {
        var canvas = new System.Windows.Controls.Canvas { Background = System.Windows.Media.Brushes.White };
        var component = new System.Windows.Controls.Border
        {
            Width = 240, Height = 120, Background = System.Windows.Media.Brushes.SteelBlue,
            Child = new System.Windows.Controls.TextBlock { Text = "AES workspace component" }
        };
        canvas.Children.Add(component);
        System.Windows.Controls.Canvas.SetLeft(component, 80);
        System.Windows.Controls.Canvas.SetTop(component, 90);
        var window = new System.Windows.Window
        {
            Content = canvas, Width = 1800, Height = 1000, Left = -10000, Top = -10000,
            ShowActivated = false, ShowInTaskbar = false, WindowStyle = System.Windows.WindowStyle.None
        };
        try
        {
            window.Show();
            window.UpdateLayout();
            string result = (string)CallStatic("Threads.WorkspaceScreenshotContent", "Capture", canvas);
            JObject screenshot = JObject.Parse(result);
            Assert((int)screenshot["width"] <= 1024 && (int)screenshot["height"] <= 1024, "Screenshot resolution must be bounded.");
            string dataUri = (string)screenshot["imageDataUri"];
            byte[] bytes = Convert.FromBase64String(dataUri.Substring("data:image/png;base64,".Length));
            using (var stream = new MemoryStream(bytes))
            {
                var png = new System.Windows.Media.Imaging.PngBitmapDecoder(stream,
                    System.Windows.Media.Imaging.BitmapCreateOptions.None, System.Windows.Media.Imaging.BitmapCacheOption.OnLoad);
                Assert(png.Frames[0].PixelWidth == (int)screenshot["width"], "Screenshot must be an actual PNG, not a textual description.");
            }
            var call = new FunctionCallContent("screenshot-call", "CrypLLM_WorkspaceStatus", "ws_screenshot");
            var messages = new[] { new FunctionResultContent(call, result).ToChatMessage() };
            object dto = CallStatic("Threads.ChatHistoryPersistence", "SerializeMessages", (object)messages);
            object restored = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(dto), dto.GetType());
            var history = (List<ChatMessageContent>)CallStatic("Threads.ChatHistoryPersistence", "DeserializeMessages", restored);
            Assert((string)history[0].Items.OfType<FunctionResultContent>().Single().Result == result, "Saved screenshots must round-trip.");
            var callMessage = new ChatMessageContent(AuthorRole.Assistant, (string)null);
            callMessage.Items.Add(call);
            var chain = new List<ChatMessageContent> { callMessage, history[0], new ChatMessageContent(AuthorRole.Assistant, "I see AES.") };
            var normalized = (List<ChatMessageContent>)CallStatic("Threads.ChatHistoryReductionEngine", "EnsureValidToolMessageChains", chain, null);
            Assert(normalized.Count == 3 && normalized[1].Items.OfType<FunctionResultContent>().Any(), "Complete image tool chains must survive normalization.");
            normalized = (List<ChatMessageContent>)CallStatic("Threads.ChatHistoryReductionEngine", "EnsureValidToolMessageChains",
                new List<ChatMessageContent> { callMessage, new ChatMessageContent(AuthorRole.Assistant, "A result was trimmed.") }, null);
            Assert(!normalized.Any(message => message.Items.OfType<FunctionCallContent>().Any()), "Trimming must never send an unanswered tool call.");
            Console.WriteLine("PASS: bounded PNG capture and screenshot history persistence");
        }
        finally { window.Close(); }
    }

    private static string TestScreenshotEnvelope()
    {
        return JsonConvert.SerializeObject(new
        {
            type = "workspace_screenshot", width = 1, height = 1,
            imageDataUri = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aWQAAAABJRU5ErkJggg=="
        });
    }

    private static async Task ScreenshotToolReachesModelAsImage()
    {
        var handler = new ScreenshotCompletionHandler();
        var contextThread = new AIThread("Live context");
        contextThread.ChatHistory.AddUserMessage(new string('a', 40000));
        Call(contextThread, "BeginContextUsage", "vision-test");
        object[] captureArguments = { null };
        var captureScope = (IDisposable)CallStatic("Threads.AIThreadManager", "EnterLlmHttpCapture", captureArguments);
        object session = captureArguments[0];
        session.GetType().GetProperty("ContextThread", PrivateInstance).SetValue(session, contextThread);
        int updates = 0;
        session.GetType().GetProperty("ContextChanged", PrivateInstance).SetValue(session, (Action)(() => updates++));
        int firstRequestTokens = 0;
        handler.BeforeResponse = call =>
        {
            int currentTokens = (int)Call(contextThread, "EstimateContextUsageTokens", "vision-test", 0);
            if (call == 1)
            {
                firstRequestTokens = currentTokens;
                Assert(currentTokens > 20 && currentTokens < 10000,
                    "Live usage must include wire tools, rather than the archived history.");
            }
            else
            {
                Assert(currentTokens > firstRequestTokens + 2048,
                    "Tool results and image input must increase usage before the response completes.");
                Assert(contextThread.ChatHistory.Count == 1,
                    "Live updates must work without the SDK adding tool results to the original thread.");
            }
        };
        Type captureType = AgentType("Threads.AIThreadManager").GetNestedType("LlmHttpCaptureHandler", BindingFlags.NonPublic);
        var transport = (HttpMessageHandler)Activator.CreateInstance(captureType, new object[] { handler });
        using (captureScope)
        using (var client = new HttpClient(transport))
        {
            var builder = Kernel.CreateBuilder();
            builder.AddOpenAIChatCompletion("vision-test", new Uri("http://localhost:1234/v1"), "test-only", null, null, client);
            var kernel = builder.Build();
            kernel.Plugins.Add(KernelPluginFactory.CreateFromFunctions("CrypLLM_WorkspaceStatus", new[]
            {
                KernelFunctionFactory.CreateFromMethod(typeof(AgentRegressionTests).GetMethod("TestScreenshotEnvelope", BindingFlags.Static | BindingFlags.NonPublic),
                    target: null, functionName: "ws_screenshot", description: null, parameters: null, returnParameter: null, loggerFactory: null)
            }));
            var history = new ChatHistory();
            history.AddUserMessage("Inspect the workspace layout.");
            var execution = new Microsoft.SemanticKernel.Connectors.OpenAI.OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = Microsoft.SemanticKernel.Connectors.OpenAI.ToolCallBehavior.AutoInvokeKernelFunctions
            };
            await kernel.GetRequiredService<IChatCompletionService>().GetChatMessageContentsAsync(history, execution, kernel);
            Assert(handler.SawImage, "The next real SDK request must contain image_url pixels.");
            Assert(handler.SawToolBeforeImage, "The image must follow the complete tool-result group.");
            Assert(history.Any(message => message.Items.OfType<FunctionResultContent>().Any(item =>
                item.Result is string text && text.Contains("imageDataUri"))), "The SDK history must retain the image envelope.");
            Assert(updates == 4, "Both requests and both responses must refresh live context usage.");
            Call(contextThread, "CompleteContextUsage");
            int completedTokens = (int)Call(contextThread, "EstimateContextUsageTokens", "vision-test", 0);
            Assert(completedTokens > firstRequestTokens + 2048, "The last prompt and answer must remain visible after completion.");
            Assert((int)Call(contextThread, "EstimateContextUsageTokens", "another-model", 10) > 10000,
                "Switching models must use the retained history and instruction estimate.");
            contextThread.ChatHistory.Clear();
            Assert((int)Call(contextThread, "EstimateContextUsageTokens", "vision-test", 10) == 10,
                "Clearing a chat must invalidate its transport snapshot.");
        }

        var imageRequest = new JObject { ["messages"] = new JArray(new JObject {
            ["role"] = "user", ["content"] = new JArray(new JObject {
                ["type"] = "image_url", ["image_url"] = new JObject { ["url"] = "data:image/png;base64," + new string('x', 100000) } }) }) };
        Assert((int)CallStatic("Threads.ChatTokenEstimator", "EstimateRequestTokens", imageRequest.ToString()) == 2056,
            "Base64 image bytes must not be counted as text tokens.");
        Console.WriteLine("PASS: live context counts actual requests, tools/images and responses, with per-chat invalidation");

        // Parallel calls must all receive results before the inserted image input.
        string envelope = TestScreenshotEnvelope();
        var request = new JObject { ["messages"] = new JArray(
            new JObject { ["role"] = "assistant", ["tool_calls"] = new JArray() },
            new JObject { ["role"] = "tool", ["tool_call_id"] = "first", ["content"] = envelope },
            new JObject { ["role"] = "tool", ["tool_call_id"] = "second", ["content"] = "other result" }) };
        JObject expanded = JObject.Parse((string)CallStatic("Threads.WorkspaceScreenshotContent", "ExpandRequest", request.ToString()));
        var messages = (JArray)expanded["messages"];
        Assert((string)messages[2]["role"] == "tool" && (string)messages[3]["role"] == "user", "Images must not interrupt parallel tool results.");
        Console.WriteLine("PASS: screenshot tool reaches the model as image input through the actual SDK");
    }

    private static async Task ConfigurableCompressionPreservesFullHistory()
    {
        var settings = CrypTool.CrypLLM.Properties.Settings.Default;
        Assert(settings.Properties["autoCompressContext"].DefaultValue.ToString() == "True" &&
            settings.Properties["contextCompressionTriggerPercent"].DefaultValue.ToString() == "90" &&
            settings.Properties["contextCompressionTargetPercent"].DefaultValue.ToString() == "50", "Compression defaults must be enabled at 90% with a 50% target.");
        settings.userModelContextWindows = "compression-test=20000";
        settings.autoCompressContext = true;
        settings.contextCompressionTriggerPercent = 90;
        settings.contextCompressionTargetPercent = 50;
        object profile = CallStatic("Threads.PromptBudgetPlanner", "BuildProfile", "compression-test", "System instructions.", 100);
        object engine = Activator.CreateInstance(AgentType("Threads.ChatHistoryReductionEngine"), true);
        var service = new SummaryCompletionService();
        var history = new ChatHistory();
        for (int index = 0; index < 60; index++)
            history.Add(new ChatMessageContent(index % 2 == 0 ? AuthorRole.User : AuthorRole.Assistant, "message " + index + " " + new string('x', 900)));
        var thread = new AIThread("Compression test");
        object originalBudget = CallStatic("Threads.PromptBudgetPlanner", "Calculate", profile, "Next request.", history.ToList(), "test");
        int availableHistory = (int)originalBudget.GetType().GetProperty("HistoryBudgetTokens").GetValue(originalBudget);
        var prepared = await (Task<object>)AwaitPreparation(engine, thread, history, profile, service);
        var messages = (List<ChatMessageContent>)prepared.GetType().GetProperty("InvocationMessages").GetValue(prepared);
        int used = (int)CallStatic("Threads.ChatTokenEstimator", "EstimateMessagesTokens", messages);
        Assert(used <= availableHistory * 50 / 100, "Compacted history must reach the configured target.");
        Assert(history.Count == 60, "Compression must preserve the complete source history.");
        Assert(service.Calls > 0, "Older conversation must be summarized through the configured model service.");
        int callsAfterCompression = service.Calls;
        await AwaitPreparation(engine, thread, history, profile, service);
        Assert(service.Calls == callsAfterCompression, "The reduced-history cache must avoid summarizing the same full history again.");

        object budget = Activator.CreateInstance(AgentType("Threads.PromptBudget"), true);
        budget.GetType().GetProperty("HistoryBudgetTokens").SetValue(budget, 1000);
        budget.GetType().GetProperty("EstimatedHistoryTokens").SetValue(budget, 899);
        MethodInfo limit = engine.GetType().GetMethod("GetReductionHistoryLimit", BindingFlags.Static | BindingFlags.NonPublic);
        Assert((int)limit.Invoke(null, new[] { budget }) == 1000, "No proactive compaction below trigger.");
        budget.GetType().GetProperty("EstimatedHistoryTokens").SetValue(budget, 900);
        Assert((int)limit.Invoke(null, new[] { budget }) == 500, "The trigger boundary must be inclusive.");
        settings.contextCompressionTriggerPercent = 70;
        settings.contextCompressionTargetPercent = 30;
        Assert((int)limit.Invoke(null, new[] { budget }) == 300, "Custom target settings must apply.");
        settings.autoCompressContext = false;
        Assert((int)limit.Invoke(null, new[] { budget }) == 1000, "Disabling proactive compression must preserve the hard budget.");
        Console.WriteLine("PASS: configurable compression threshold/target, model summary and full-history cache");
    }

    private static void ContextUsageFollowsCompressedHistory()
    {
        var thread = new AIThread("Context usage");
        Assert((int)Call(thread, "EstimateContextHistoryTokens") == 0, "Empty chats must show zero usage.");
        thread.ChatHistory.AddUserMessage(new string('x', 4000));
        thread.ChatHistory.AddAssistantMessage(new string('y', 4000));
        int archivedTokens = (int)Call(thread, "EstimateContextHistoryTokens");
        var summary = new List<ChatMessageContent> { new ChatMessageContent(AuthorRole.Assistant, "Short retained summary.") };
        Call(thread, "SetReducedHistoryCache", summary, 2);
        int compactedTokens = (int)Call(thread, "EstimateContextHistoryTokens");
        Assert(compactedTokens < archivedTokens / 2, "Usage must count the retained summary instead of the full archived chat.");
        var result = new ChatMessageContent(AuthorRole.Tool, (string)null);
        result.Items.Add(new FunctionResultContent("inspect", "Workspace", "call-usage", new string('z', 400)));
        thread.ChatHistory.Add(result);
        int resultTokens = (int)CallStatic("Threads.ChatTokenEstimator", "EstimateMessageTokens", result);
        Assert((int)Call(thread, "EstimateContextHistoryTokens") == compactedTokens + resultTokens, "New tool results must increase usage without restoring archived messages.");
        thread.ChatHistory.Clear();
        Assert((int)Call(thread, "EstimateContextHistoryTokens") == 0, "A cache beyond the current history must be ignored.");
        Console.WriteLine("PASS: context usage tracks compressed history and subsequent tool results");
    }

    private static async Task<object> AwaitPreparation(object engine, AIThread thread, ChatHistory history, object profile, IChatCompletionService service)
    {
        object task = Call(engine, "PrepareForInvocationAsync", thread, "compression-test", "Next request.", history, profile, service, CancellationToken.None);
        await (Task)task;
        return task.GetType().GetProperty("Result").GetValue(task);
    }

    private sealed class SummaryCompletionService : IChatCompletionService
    {
        internal int Calls;
        public IReadOnlyDictionary<string, object> Attributes { get; } = new Dictionary<string, object>();
        public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory history, PromptExecutionSettings settings = null, Kernel kernel = null, CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult<IReadOnlyList<ChatMessageContent>>(new[] { new ChatMessageContent(AuthorRole.Assistant, "Summary: user is inspecting an AES workspace and wants correct connector layout.") });
        }
        public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory history, PromptExecutionSettings settings = null, Kernel kernel = null, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class ScreenshotCompletionHandler : HttpMessageHandler
    {
        private int Calls;
        internal Action<int> BeforeResponse;
        internal bool SawImage;
        internal bool SawToolBeforeImage;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            JObject payload = JObject.Parse(await request.Content.ReadAsStringAsync());
            Calls++;
            BeforeResponse?.Invoke(Calls);
            string message;
            if (Calls == 1)
                message = "{\"role\":\"assistant\",\"content\":null,\"tool_calls\":[{\"id\":\"screenshot-call\",\"type\":\"function\",\"function\":{\"name\":\"CrypLLM_WorkspaceStatus-ws_screenshot\",\"arguments\":\"{}\"}}]}";
            else
            {
                var messages = (JArray)payload["messages"];
                int imageIndex = -1;
                int toolIndex = -1;
                for (int index = 0; index < messages.Count; index++)
                {
                    if ((string)messages[index]["role"] == "tool") toolIndex = index;
                    if (messages[index]["content"] is JArray parts && parts.Any(part => (string)part["type"] == "image_url")) imageIndex = index;
                }
                SawImage = imageIndex >= 0;
                SawToolBeforeImage = toolIndex >= 0 && imageIndex > toolIndex;
                message = "{\"role\":\"assistant\",\"content\":\"I see the workspace image.\"}";
            }
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\":\"vision-test\",\"object\":\"chat.completion\",\"created\":1,\"model\":\"vision-test\",\"choices\":[{\"index\":0,\"message\":" + message + ",\"finish_reason\":\"stop\"}]}", Encoding.UTF8, "application/json")
            };
        }
    }

    private static void AiMemoCreationPreservesSelectionAndFocus()
    {
        if (System.Windows.Application.Current == null)
            new System.Windows.Application { ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown };
        var model = new WorkspaceManager.Model.WorkspaceModel();
        var editor = new WorkspaceManager.WorkspaceManagerClass(model);
        editor.New();
        CrypWinPort.Instance = new ClosedWorkspaceAdapter { Workspace = editor };
        var view = (WorkspaceManager.View.Visuals.EditorVisual)editor.Presentation;
        var chatInput = new System.Windows.Controls.TextBox { Text = "Keep this input focused", Height = 28 };
        var panel = new System.Windows.Controls.DockPanel();
        System.Windows.Controls.DockPanel.SetDock(chatInput, System.Windows.Controls.Dock.Top);
        panel.Children.Add(chatInput);
        panel.Children.Add(view);
        var window = new System.Windows.Window
        {
            Content = panel, Width = 1000, Height = 700, Left = -10000, Top = -10000,
            ShowActivated = false, ShowInTaskbar = false
        };
        try
        {
            window.Show();
            window.UpdateLayout();
            System.Windows.Input.FocusManager.SetFocusedElement(window, chatInput);
            object plugin = Activator.CreateInstance(AgentType("WorkspaceEditingPlugin"), true);
            using ((IDisposable)CallStatic("Services.LLMPluginService", "PushPreferredWorkspaceTabId", "resize-tab"))
            {
                var result = JObject.Parse((string)Call(plugin, "AddMemoToWorkspace", "Created by AI", null, null, null));
                window.UpdateLayout();
                Assert((bool)result["success"] && view.SelectedText == null, "An AI memo must not become the selected text editor.");
                var memo = model.GetAllTextModels().Single();
                var memoView = (WorkspaceManager.View.Visuals.TextVisual)memo.UpdateableView;
                Assert(!memoView.IsSelected && System.Windows.Input.FocusManager.GetFocusedElement(window) == chatInput,
                    "AI creation must leave the previous focus scope unchanged and avoid scheduling memo edit focus.");
                Assert(((string)CallStatic("WorkspaceEditingPlugin", "ReadPlainTextFromMemoModel", memo)).Contains("Created by AI"), "Unselected memos must still display their content.");
                model.UndoRedoManager.Undo();
                Assert(model.GetAllTextModels().Count == 0, "AI memo creation must remain undoable.");
                model.UndoRedoManager.Redo();
                window.UpdateLayout();
                Assert(model.GetAllTextModels().Count == 1 && view.SelectedText == null && !memoView.IsSelected,
                    "Redo must not activate an AI memo's editor either.");

                var manual = (WorkspaceManager.Model.TextModel)model.ModifyModel(new WorkspaceManagerModel.Model.Operations.NewTextModelOperation(), true);
                var manualView = (WorkspaceManager.View.Visuals.TextVisual)manual.UpdateableView;
                Assert(view.SelectedText == manualView && manualView.IsSelected, "Manual memo creation must retain automatic selection.");
                result = JObject.Parse((string)Call(plugin, "AddMemoToWorkspace", "Second AI memo", null, null, null));
                window.UpdateLayout();
                Assert((bool)result["success"] && view.SelectedText == manualView && manualView.IsSelected,
                    "AI creation must also preserve an existing memo selection.");
            }
            Console.WriteLine("PASS: AI memo creation preserves selection/focus, manual behavior and Undo/Redo");
        }
        finally { window.Close(); }
    }

    private static void ResizeToolsPreserveWorkspaceAndSupportUndo()
    {
        if (System.Windows.Application.Current == null)
            new System.Windows.Application { ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown };
        var model = new WorkspaceManager.Model.WorkspaceModel();
        var editor = new WorkspaceManager.WorkspaceManagerClass(model);
        editor.New();
        var adapter = new ClosedWorkspaceAdapter { Workspace = editor };
        CrypWinPort.Instance = adapter;
        var window = new System.Windows.Window
        {
            Content = editor.Presentation, Width = 1400, Height = 900, Left = -10000, Top = -10000,
            ShowActivated = false, ShowInTaskbar = false
        };
        try
        {
            window.Show();
            var input = (WorkspaceManager.Model.PluginModel)model.ModifyModel(new WorkspaceManagerModel.Model.Operations.NewPluginModelOperation(
                new System.Windows.Point(80, 100), 0, 0, typeof(CrypTool.TextInput.TextInput)), true);
            var output = (WorkspaceManager.Model.PluginModel)model.ModifyModel(new WorkspaceManagerModel.Model.Operations.NewPluginModelOperation(
                new System.Windows.Point(800, 100), 0, 0, typeof(TextOutput.TextOutput)), true);
            input.Plugin.Settings.GetType().GetProperty("Text").SetValue(input.Plugin.Settings, "Preserve my plaintext.");
            var connection = (WorkspaceManager.Model.ConnectionModel)model.ModifyModel(new WorkspaceManagerModel.Model.Operations.NewConnectionModelOperation(
                input.GetOutputConnectors().First(), output.GetInputConnectors().First(), typeof(string)), true);
            var memo = (WorkspaceManager.Model.TextModel)model.ModifyModel(new WorkspaceManagerModel.Model.Operations.NewTextModelOperation(), true);
            CallStatic("WorkspaceEditingPlugin", "SetPlainTextInMemoModel", memo, "Preserve my memo text.");
            window.UpdateLayout();
            var inputView = (WorkspaceManager.View.Visuals.ComponentVisual)input.UpdateableView;
            inputView.State = WorkspaceManager.Model.BinComponentState.Min;
            window.UpdateLayout();
            string inputId = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(input).ToString();
            string memoId = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(memo).ToString();
            object plugin = Activator.CreateInstance(AgentType("WorkspaceEditingPlugin"), true);
            var originalPlugin = input.Plugin;
            double originalWidth = input.GetWidth();
            double originalHeight = input.GetHeight();
            double originalMemoWidth = memo.GetWidth();
            double originalMemoHeight = memo.GetHeight();
            model.UndoRedoManager.ClearStacks();
            using ((IDisposable)CallStatic("Services.LLMPluginService", "PushPreferredWorkspaceTabId", "resize-tab"))
            {
                JObject before = JObject.Parse((string)CallStatic("Services.WorkspaceInspector", "CreateWorkspaceElementBoundsJson", model));
                Assert((double)before[inputId]["width"] > 0 && (double)before[memoId]["height"] > 0, "Auto-sized elements must expose real visible bounds.");
                Assert((string)before[inputId]["sizeSource"] == "visual", "Bounds must distinguish visible dimensions from stored zero sizes.");
                JObject resized = JObject.Parse((string)Call(plugin, "ResizeComponentInWorkspace", inputId, 600.0, 260.0, null));
                Assert((bool)resized["success"] && (bool)resized["viewExpanded"], "Resizing must expand the fixed icon view to its resizable presentation.");
                Assert(input.GetWidth() == 600 && input.GetHeight() == 260, "Component model dimensions must change.");
                Assert((double)resized["size"]["width"] == 600 && (double)resized["size"]["height"] == 260, "The real visual must resize immediately.");
                resized = JObject.Parse((string)Call(plugin, "ResizeMemoInWorkspace", memoId, 560.0, 180.0, null));
                Assert((bool)resized["success"] && memo.GetWidth() == 560 && memo.GetHeight() == 180, "Memo dimensions must change.");
                Assert((double)resized["size"]["width"] == 560, "The real memo visual must resize immediately.");
                Assert(ReferenceEquals(input.Plugin, originalPlugin) && model.GetAllPluginModels().Count == 2 &&
                    model.GetAllConnectionModels().Contains(connection), "Resizing must not replace components or connections.");
                Assert((string)input.Plugin.Settings.GetType().GetProperty("Text").GetValue(input.Plugin.Settings) == "Preserve my plaintext.", "Component contents must remain unchanged.");
                Assert(((string)CallStatic("WorkspaceEditingPlugin", "ReadPlainTextFromMemoModel", memo)).Contains("Preserve my memo text."), "Memo text must remain unchanged.");
                Assert(input.GetPosition() == new System.Windows.Point(80, 100), "Resizing must preserve position.");
                model.UndoRedoManager.Undo();
                Assert(memo.GetWidth() == originalMemoWidth && memo.GetHeight() == originalMemoHeight, "Memo resize must undo.");
                model.UndoRedoManager.Undo();
                Assert(input.GetWidth() == originalWidth && input.GetHeight() == originalHeight, "Component resize must undo.");
                model.UndoRedoManager.Redo();
                model.UndoRedoManager.Redo();
                Assert(input.GetWidth() == 600 && memo.GetWidth() == 560, "Both resize operations must redo.");
                JObject workspace = JObject.FromObject(CallStatic("Services.AISchemaGenerator", "CreateWorkspaceModelAbstraction", model));
                Assert((double)workspace["Components"][0]["Bounds"]["width"] == 600 &&
                    (double)workspace["Texts"][0]["Bounds"]["width"] == 560, "The primary workspace model must expose component and memo bounds too.");
                foreach (double invalid in new[] { 0.0, -10.0, double.NaN, double.PositiveInfinity, 10001.0 })
                {
                    resized = JObject.Parse((string)Call(plugin, "ResizeMemoInWorkspace", memoId, invalid, 180.0, null));
                    Assert(!(bool)resized["success"] && memo.GetWidth() == 560, "Invalid dimensions must fail without changing the model.");
                }
                resized = JObject.Parse((string)Call(plugin, "ResizeMemoInWorkspace", memoId, 1.0, 1.0, null));
                Assert((bool)resized["success"] && (bool)resized["adjustedToConstraints"] && memo.GetWidth() >= 150 && memo.GetHeight() >= 100, "Small sizes must be clamped and reported.");
                CheckConnectorOrientations(plugin, model, input, output, connection, window);
            }
            using ((IDisposable)CallStatic("Services.LLMPluginService", "PushPreferredWorkspaceTabId", "closed-tab"))
            {
                JObject failed = JObject.Parse((string)Call(plugin, "ResizeComponentInWorkspace", inputId, 700.0, 260.0, null));
                Assert(!(bool)failed["success"] && input.GetWidth() == 600 && adapter.ActiveEditorReads == 0, "Closed pins must not redirect resize operations.");
                failed = JObject.Parse((string)Call(plugin, "SetConnectorOrientationInWorkspace", inputId, connection.From.PropertyName, "North", null));
                Assert(!(bool)failed["success"] && adapter.ActiveEditorReads == 0, "Closed pins must not redirect connector changes.");
            }
            Assert((bool)CallStatic("Threads.ToolPermissionFilter", "IsMutationFunction", "ws_resize_component") &&
                (bool)CallStatic("Threads.ToolPermissionFilter", "IsMutationFunction", "ws_resize_memo"), "Resize tools must obey mutation permission gating.");
            Assert((bool)CallStatic("Threads.ToolPermissionFilter", "IsMutationFunction", "ws_set_connector_orientation"), "Connector orientation must obey mutation permission gating.");
            Console.WriteLine("PASS: real component/memo resizing, bounds, preserved contents/connections, Undo/Redo and invalid dimensions");
        }
        finally { window.Close(); }
    }

    private static void CheckConnectorOrientations(object plugin, WorkspaceManager.Model.WorkspaceModel model,
        WorkspaceManager.Model.PluginModel source, WorkspaceManager.Model.PluginModel target,
        WorkspaceManager.Model.ConnectionModel connection, System.Windows.Window window)
    {
        foreach (var component in new[] { source, target })
        {
            var connector = component == source ? connection.From : connection.To;
            var view = (WorkspaceManager.View.Visuals.ComponentVisual)component.UpdateableView;
            string id = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(component).ToString();
            var connectorView = view.ConnectorCollection.Single(item => item.Model == connector);
            // Older files can leave the model side automatic even when the visual is already measured.
            connector.Orientation = WorkspaceManager.Model.ConnectorOrientation.Unset;
            foreach (string side in new[] { "North", "South", "West", "East" })
            {
                var previous = connector.Orientation;
                var result = JObject.Parse((string)Call(plugin, "SetConnectorOrientationInWorkspace", id, connector.PropertyName, side.ToLowerInvariant(), null));
                window.UpdateLayout();
                Assert((bool)result["success"] && connector.Orientation.ToString() == side, "Input and output connectors must accept every named side.");
                var collection = side == "North" ? view.NorthConnectorCollection : side == "South" ? view.SouthConnectorCollection
                    : side == "East" ? view.EastConnectorCollection : view.WestConnectorCollection;
                Assert(collection.Contains(connectorView) && connectorView.Orientation.ToString() == side, "The existing connector visual must move to the requested side.");
                double rotation = side == "North" ? (connector.Outgoing ? 180 : 0) : side == "South" ? (connector.Outgoing ? 0 : 180)
                    : side == "East" ? (connector.Outgoing ? -90 : 90) : (connector.Outgoing ? 90 : -90);
                Assert(connectorView.RotationAngle == rotation, "Arrow rotation must follow the native side and input/output direction.");
                Assert(model.GetAllConnectionModels().Contains(connection) && connection.From == source.GetOutputConnectors().First() &&
                    connection.To == target.GetInputConnectors().First(), "Moving connector sides must preserve wire endpoints and identity.");
                if ((bool)result["changed"])
                {
                    model.UndoRedoManager.Undo();
                    window.UpdateLayout();
                    Assert(connector.Orientation == previous, "Connector orientation must undo in model and visual.");
                    var restored = previous == WorkspaceManager.Model.ConnectorOrientation.Unset
                        ? (connector.Outgoing ? WorkspaceManager.Model.ConnectorOrientation.East : WorkspaceManager.Model.ConnectorOrientation.West) : previous;
                    Assert(connectorView.Orientation == restored, "Undo must restore the connector visual side.");
                    model.UndoRedoManager.Redo();
                    window.UpdateLayout();
                    Assert(connector.Orientation.ToString() == side && connectorView.Orientation.ToString() == side, "Connector orientation must redo.");
                }
            }
            foreach (string invalid in new[] { "Unset", "0", "4", "Diagonal", "", null })
            {
                var previous = connector.Orientation;
                var failed = JObject.Parse((string)Call(plugin, "SetConnectorOrientationInWorkspace", id, connector.PropertyName, invalid, null));
                Assert(!(bool)failed["success"] && connector.Orientation == previous, "Invalid sides must fail without changing connectors.");
            }
            var missing = JObject.Parse((string)Call(plugin, "SetConnectorOrientationInWorkspace", id, "missing-connector", "North", null));
            Assert(!(bool)missing["success"], "Unknown connector names must fail.");
            var unchanged = JObject.Parse((string)Call(plugin, "SetConnectorOrientationInWorkspace", id, connector.PropertyName, "East", null));
            Assert((bool)unchanged["success"] && !(bool)unchanged["changed"], "The current side must report a no-op.");
        }
        var schema = JObject.FromObject(CallStatic("Services.AISchemaGenerator", "CreateWorkspaceModelAbstraction", model));
        Assert(schema["Components"].Any(component => component["Outputs"].Any(item => (string)item["Orientation"] == "East")) &&
            schema["Components"].Any(component => component["Inputs"].Any(item => (string)item["Orientation"] == "East")), "The workspace schema must expose current connector sides.");
        Console.WriteLine("PASS: connector sides, visual placement/rotation, preserved wiring, Undo/Redo and invalid names/sides");
    }

    private static void MemoFitAndLayoutChecks()
    {
        var model = new WorkspaceManager.Model.WorkspaceModel();
        var editor = new WorkspaceManager.WorkspaceManagerClass(model);
        editor.New();
        CrypWinPort.Instance = new ClosedWorkspaceAdapter { Workspace = editor };
        var window = new System.Windows.Window
        {
            Content = editor.Presentation, Width = 1400, Height = 900, Left = -10000, Top = -10000,
            ShowActivated = false, ShowInTaskbar = false
        };
        try
        {
            window.Show();
            var input = (WorkspaceManager.Model.PluginModel)model.ModifyModel(new WorkspaceManagerModel.Model.Operations.NewPluginModelOperation(
                new System.Windows.Point(100, 100), 400, 220, typeof(CrypTool.TextInput.TextInput)), true);
            var output = (WorkspaceManager.Model.PluginModel)model.ModifyModel(new WorkspaceManagerModel.Model.Operations.NewPluginModelOperation(
                new System.Windows.Point(200, 100), 400, 220, typeof(TextOutput.TextOutput)), true);
            var editing = Activator.CreateInstance(AgentType("WorkspaceEditingPlugin"), true);
            var status = Activator.CreateInstance(AgentType("WorkspaceStatusPlugin"), true);
            using ((IDisposable)CallStatic("Services.LLMPluginService", "PushPreferredWorkspaceTabId", "resize-tab"))
            {
                string text = string.Join("\n", Enumerable.Range(1, 30).Select(i => "Paragraph " + i + ": A longer explanation that wraps at the selected memo width and must remain completely readable."));
                JObject added = JObject.Parse((string)Call(editing, "AddMemoToWorkspace", text, 100.0, 500.0, null));
                string memoId = (string)added["memoId"];
                var memo = model.GetAllTextModels().Single();
                Call(editing, "ResizeMemoInWorkspace", memoId, 350.0, 110.0, null);
                window.UpdateLayout();
                JObject report = JObject.Parse((string)Call(status, "CheckWorkspaceLayout", 40.0, null));
                Assert(report["issues"].Any(issue => (string)issue["code"] == "element_overlap"), "Layout checks must detect overlapping real components.");
                Assert(report["issues"].Any(issue => (string)issue["code"] == "memo_content_clipped"), "Layout checks must detect truncated multiline memos.");
                model.UndoRedoManager.ClearStacks();
                JObject fitted = JObject.Parse((string)Call(editing, "FitMemoToContent", memoId, 350.0, null));
                Assert((bool)fitted["success"] && (bool)fitted["contentFits"] && (bool)fitted["verifiedLive"], "Fitting must make every paragraph visible in the real memo.");
                Assert(memo.GetHeight() > 1000 && memo.GetPosition() == new System.Windows.Point(100, 500), "Fitting grows long memos while preserving position.");
                Assert(((string)CallStatic("WorkspaceEditingPlugin", "ReadPlainTextFromMemoModel", memo)).Replace("\r\n", "\n") == text, "Fitting must preserve all text.");
                double narrowHeight = memo.GetHeight();
                fitted = JObject.Parse((string)Call(editing, "FitMemoToContent", memoId, 700.0, null));
                Assert((bool)fitted["contentFits"] && memo.GetHeight() < narrowHeight, "Text measurement must account for wrapping at different widths.");
                model.UndoRedoManager.Undo();
                Assert(memo.GetWidth() == 350 && memo.GetHeight() == 110, "Native consecutive resize gesture undo may coalesce fitting resizes, while preserving the pre-fit box.");
                model.UndoRedoManager.Redo();
                Assert(memo.GetWidth() == 700 && ((string)CallStatic("WorkspaceEditingPlugin", "ReadPlainTextFromMemoModel", memo)).Replace("\r\n", "\n") == text, "Fit redo must preserve content.");
                model.ModifyModel(new WorkspaceManagerModel.Model.Operations.MoveModelElementOperation(output, new System.Windows.Point(700, 100)), true);
                report = JObject.Parse((string)Call(status, "CheckWorkspaceLayout", 40.0, null));
                Assert(!(report["issues"].Any(issue => (string)issue["code"] == "element_overlap" || (string)issue["code"] == "memo_content_clipped")), "Corrected rectangles and fitted content must clear layout errors.");
                foreach (double invalid in new[] { -1.0, double.NaN, double.PositiveInfinity, 10001.0 })
                {
                    fitted = JObject.Parse((string)Call(editing, "FitMemoToContent", memoId, invalid, null));
                    Assert(!(bool)fitted["success"] && memo.GetWidth() == 700, "Invalid fitting widths must fail without modifying the memo.");
                }
                var secondOutput = (WorkspaceManager.Model.PluginModel)model.ModifyModel(new WorkspaceManagerModel.Model.Operations.NewPluginModelOperation(
                    new System.Windows.Point(1100, 100), 400, 220, typeof(TextOutput.TextOutput)), true);
                var wireA = (WorkspaceManager.Model.ConnectionModel)model.ModifyModel(new WorkspaceManagerModel.Model.Operations.NewConnectionModelOperation(input.GetOutputConnectors().First(), output.GetInputConnectors().First(), typeof(string)), true);
                var wireB = (WorkspaceManager.Model.ConnectionModel)model.ModifyModel(new WorkspaceManagerModel.Model.Operations.NewConnectionModelOperation(input.GetOutputConnectors().First(), secondOutput.GetInputConnectors().First(), typeof(string)), true);
                window.UpdateLayout();
                wireA.PointList = new List<System.Windows.Point> { new System.Windows.Point(600, 350), new System.Windows.Point(1000, 350) };
                wireB.PointList = new List<System.Windows.Point> { new System.Windows.Point(800, 250), new System.Windows.Point(800, 450) };
                report = JObject.Parse((string)Call(status, "CheckWorkspaceLayout", 0.0, null));
                Assert(report["issues"].Any(issue => (string)issue["code"] == "crossing_wires"), "Layout checks must detect route crossings.");
                wireB.PointList = new List<System.Windows.Point> { new System.Windows.Point(800, 350), new System.Windows.Point(1100, 350) };
                report = JObject.Parse((string)Call(status, "CheckWorkspaceLayout", 0.0, null));
                Assert(report["issues"].Any(issue => (string)issue["code"] == "overlaid_wires"), "Layout checks must detect shared wire segments.");
                wireB.PointList = new List<System.Windows.Point> { new System.Windows.Point(200, 550), new System.Windows.Point(200, 650) };
                report = JObject.Parse((string)Call(status, "CheckWorkspaceLayout", 0.0, null));
                Assert(report["issues"].Any(issue => (string)issue["code"] == "wire_through_element" && (string)issue["otherId"] == memoId), "Wire routes through memo content must be reported.");
                wireB.PointList = null;
                report = JObject.Parse((string)Call(status, "CheckWorkspaceLayout", 0.0, null));
                Assert((int)report["uncheckedConnections"] > 0 && !(bool)report["complete"], "Missing routes must never be claimed as checked.");
            }
            Console.WriteLine("PASS: measured memo fitting, wrapped paragraphs, preserved text/position, Undo/Redo, overlaps and wire checks");
        }
        finally { window.Close(); }
    }

    private static object NewEditSession(AIThread thread) => Activator.CreateInstance(AgentType("Threads.AgentEditSession"), BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { thread }, null);
    private static AgentWorkspaceChange[] Changes(AIThread thread) => ((IEnumerable)typeof(AIThread).GetProperty("AgentWorkspaceChanges", PrivateInstance).GetValue(thread)).Cast<AgentWorkspaceChange>().ToArray();

    private static void PumpUiUntil(Task task)
    {
        var dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
        var frame = new System.Windows.Threading.DispatcherFrame();
        var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
        timer.Tick += (sender, args) => frame.Continue = false;
        task.ContinueWith(completed => dispatcher.BeginInvoke(new Action(() => frame.Continue = false)));
        timer.Start();
        try { System.Windows.Threading.Dispatcher.PushFrame(frame); }
        finally { timer.Stop(); }
        Assert(task.IsCompleted, "The dispatched tool request must finish without deadlocking the UI.");
        task.GetAwaiter().GetResult();
    }

    private static void WireClearanceIncludesAttachedComponents()
    {
        var model = new WorkspaceManager.Model.WorkspaceModel();
        var editor = new WorkspaceManager.WorkspaceManagerClass(model);
        editor.New();
        CrypWinPort.Instance = new ClosedWorkspaceAdapter { Workspace = editor };
        var window = new System.Windows.Window { Content = editor.Presentation, Width = 1400, Height = 900, Left = -10000, Top = -10000, ShowActivated = false, ShowInTaskbar = false };
        try
        {
            window.Show();
            var source = (WorkspaceManager.Model.PluginModel)model.ModifyModel(new WorkspaceManagerModel.Model.Operations.NewPluginModelOperation(new System.Windows.Point(100, 100), 400, 220, typeof(CrypTool.TextInput.TextInput)), true);
            var main = (WorkspaceManager.Model.PluginModel)model.ModifyModel(new WorkspaceManagerModel.Model.Operations.NewPluginModelOperation(new System.Windows.Point(650, 100), 400, 220, typeof(TextOutput.TextOutput)), true);
            var branch = (WorkspaceManager.Model.PluginModel)model.ModifyModel(new WorkspaceManagerModel.Model.Operations.NewPluginModelOperation(new System.Windows.Point(100, 450), 400, 220, typeof(TextOutput.TextOutput)), true);
            var mainWire = (WorkspaceManager.Model.ConnectionModel)model.ModifyModel(new WorkspaceManagerModel.Model.Operations.NewConnectionModelOperation(source.GetOutputConnectors().First(), main.GetInputConnectors().First(), typeof(string)), true);
            var branchWire = (WorkspaceManager.Model.ConnectionModel)model.ModifyModel(new WorkspaceManagerModel.Model.Operations.NewConnectionModelOperation(source.GetOutputConnectors().First(), branch.GetInputConnectors().First(), typeof(string)), true);
            window.UpdateLayout();
            var sourceRect = (System.Windows.Rect)CallStatic("Services.WorkspaceElementGeometry", "GetBodyBounds", source);
            var mainRect = (System.Windows.Rect)CallStatic("Services.WorkspaceElementGeometry", "GetBodyBounds", main);
            var branchRect = (System.Windows.Rect)CallStatic("Services.WorkspaceElementGeometry", "GetBodyBounds", branch);
            var sourceOccupied = (System.Windows.Rect)CallStatic("Services.WorkspaceElementGeometry", "GetOccupiedBounds", source);
            var originalSourceOrientation = source.GetOutputConnectors().First().Orientation;
            Assert(sourceRect.Left > source.GetPosition().X && sourceRect.Top > source.GetPosition().Y && sourceOccupied.Width > sourceRect.Width,
                "Geometry must include the real body offset and distinguish occupied connector rails from the inner box.");
            var status = Activator.CreateInstance(AgentType("WorkspaceStatusPlugin"), true);
            var editing = Activator.CreateInstance(AgentType("WorkspaceEditingPlugin"), true);
            using ((IDisposable)CallStatic("Services.LLMPluginService", "PushPreferredWorkspaceTabId", "resize-tab"))
            {
                JObject bounds = JObject.Parse((string)Call(status, "GetWorkspaceElementBounds", (object)null));
                Assert((double)bounds[source.GetHashCode().ToString()]["bodyBounds"]["x"] == sourceRect.X &&
                    (double)bounds[source.GetHashCode().ToString()]["occupiedBounds"]["width"] == sourceOccupied.Width, "Bounds tools must expose the same real geometry as the layout checker.");
                ((WorkspaceManager.View.VisualComponents.CryptoLineView.CryptoLineView)mainWire.UpdateableView).Line.SetValue(WorkspaceManager.View.VisualComponents.CryptoLineView.InternalCryptoLineView.HasComputedProperty, true);
                ((WorkspaceManager.View.VisualComponents.CryptoLineView.CryptoLineView)branchWire.UpdateableView).Line.SetValue(WorkspaceManager.View.VisualComponents.CryptoLineView.InternalCryptoLineView.HasComputedProperty, true);
                double y = sourceRect.Top + 40;
                mainWire.PointList = new List<System.Windows.Point> { new System.Windows.Point(sourceRect.Right, y), new System.Windows.Point(mainRect.Left, y) };
                branchWire.PointList = new List<System.Windows.Point>
                {
                    new System.Windows.Point(sourceRect.Right, y), new System.Windows.Point(sourceRect.Left + 80, y),
                    new System.Windows.Point(sourceRect.Left + 80, branchRect.Top + 40), new System.Windows.Point(branchRect.Left, branchRect.Top + 40)
                };
                JObject report = JObject.Parse((string)Call(status, "CheckWorkspaceLayout", 0.0, null));
                var penetrations = report["issues"].Where(issue => (string)issue["code"] == "wire_through_element").ToArray();
                Assert(penetrations.Any(issue => (string)issue["details"]["elementRole"] == "source") && penetrations.Any(issue => (string)issue["details"]["elementRole"] == "target") &&
                    penetrations.All(issue => (string)issue["severity"] == "error") && !(bool)report["passed"], "Wires through their own source and target bodies must be real layout errors.");
                JToken advice = report["issues"].Single(issue => (string)issue["code"] == "connector_sides_need_detour" && (string)issue["elementId"] == branchWire.GetHashCode().ToString())["details"];
                Assert((string)advice["suggestedSourceOrientation"] == "South" && (string)advice["suggestedTargetOrientation"] == "North" && (int)advice["sharedOutputConnections"] == 2,
                    "Vertical branch advice must include matching sides and warn about a shared output's other branches.");
                Call(editing, "SetConnectorOrientationInWorkspace", branch.GetHashCode().ToString(), branchWire.To.PropertyName, "North", null);
                JObject freshReport = JObject.Parse((string)Call(status, "CheckWorkspaceLayout", 0.0, null));
                Assert((int)freshReport["uncheckedConnections"] == 0, "A live invalidated native route must be recomputed before its geometry is reported.");
                ((WorkspaceManager.View.VisualComponents.CryptoLineView.CryptoLineView)branchWire.UpdateableView).Line.SetValue(WorkspaceManager.View.VisualComponents.CryptoLineView.InternalCryptoLineView.HasComputedProperty, true);
                branchWire.PointList = new List<System.Windows.Point>
                {
                    new System.Windows.Point(sourceRect.Right, y), new System.Windows.Point(sourceRect.Right + 60, y),
                    new System.Windows.Point(sourceRect.Right + 60, branchRect.Top - 60), new System.Windows.Point(branchRect.Left + 80, branchRect.Top - 60),
                    new System.Windows.Point(branchRect.Left + 80, branchRect.Top)
                };
                report = JObject.Parse((string)Call(status, "CheckWorkspaceLayout", 0.0, null));
                Assert((bool)report["passed"] && !report["issues"].Any(issue => (string)issue["code"] == "wire_through_element"),
                    "Boundary attachment and a clear side corridor must be allowed without forcing a shared main output to change sides.");
                Assert(source.GetOutputConnectors().First().Orientation == originalSourceOrientation && model.GetAllConnectionModels().Count == 2,
                    "Advice and inspection must preserve output sides and all fan-out connections.");
                model.ModifyModel(new WorkspaceManagerModel.Model.Operations.MoveModelElementOperation(branch, new System.Windows.Point(650, 450)), true);
                Call(editing, "SetConnectorOrientationInWorkspace", branch.GetHashCode().ToString(), branchWire.To.PropertyName, "West", null);
                window.UpdateLayout();
                branchRect = (System.Windows.Rect)CallStatic("Services.WorkspaceElementGeometry", "GetBodyBounds", branch);
                ((WorkspaceManager.View.VisualComponents.CryptoLineView.CryptoLineView)branchWire.UpdateableView).Line.SetValue(WorkspaceManager.View.VisualComponents.CryptoLineView.InternalCryptoLineView.HasComputedProperty, true);
                branchWire.PointList = new List<System.Windows.Point>
                {
                    new System.Windows.Point(sourceRect.Right, y), new System.Windows.Point(sourceRect.Right + 60, y),
                    new System.Windows.Point(sourceRect.Right + 60, branchRect.Top + 40), new System.Windows.Point(branchRect.Left, branchRect.Top + 40)
                };
                report = JObject.Parse((string)Call(status, "CheckWorkspaceLayout", 0.0, null));
                Assert((bool)report["passed"] && !report["issues"].Any(issue => (string)issue["code"] == "connector_sides_need_detour" && (string)issue["elementId"] == branchWire.GetHashCode().ToString()),
                    "Moving the receiver beyond the source's East side must clear both penetration and incompatible-side advice.");
                var memo = (WorkspaceManager.Model.TextModel)model.ModifyModel(new WorkspaceManagerModel.Model.Operations.NewTextModelOperation(false, "Do not overlap the connector rails"), true);
                model.ModifyModel(new WorkspaceManagerModel.Model.Operations.MoveModelElementOperation(memo, new System.Windows.Point(sourceRect.Right + 5, sourceRect.Top + 90)), true);
                window.UpdateLayout();
                report = JObject.Parse((string)Call(status, "CheckWorkspaceLayout", 0.0, null));
                string sourceId = source.GetHashCode().ToString(), memoId = memo.GetHashCode().ToString();
                Assert(report["issues"].Any(issue => (string)issue["code"] == "element_overlap" &&
                    (((string)issue["elementId"] == sourceId && (string)issue["otherId"] == memoId) || ((string)issue["elementId"] == memoId && (string)issue["otherId"] == sourceId))),
                    "Overlapping connector rails must be detected even when the inner body rectangles are separate.");
            }
            Console.WriteLine("PASS: attached source/target wire clearance, real body/occupied bounds, conditional fan-out advice and boundary attachment");
        }
        finally { window.Close(); }
    }

    private static void AgentGroupsPreserveManualChanges()
    {
        var model = new WorkspaceManager.Model.WorkspaceModel();
        var editor = new WorkspaceManager.WorkspaceManagerClass(model);
        editor.New();
        var adapter = new ClosedWorkspaceAdapter { Workspace = editor };
        CrypWinPort.Instance = adapter;
        var window = new System.Windows.Window { Content = editor.Presentation, Width = 1200, Height = 900, Left = -10000, Top = -10000, ShowActivated = false, ShowInTaskbar = false };
        try
        {
            window.Show();
            var memo = (WorkspaceManager.Model.TextModel)model.ModifyModel(new WorkspaceManagerModel.Model.Operations.NewTextModelOperation(false, "Original memo content"), true);
            var input = (WorkspaceManager.Model.PluginModel)model.ModifyModel(new WorkspaceManagerModel.Model.Operations.NewPluginModelOperation(new System.Windows.Point(80, 100), 400, 220, typeof(CrypTool.TextInput.TextInput)), true);
            input.Plugin.Settings.GetType().GetProperty("Text").SetValue(input.Plugin.Settings, "Original input");
            window.UpdateLayout();
            var originalPosition = memo.GetPosition();
            string memoId = memo.GetHashCode().ToString(), inputId = input.GetHashCode().ToString();
            model.UndoRedoManager.ClearStacks();
            var thread = new AIThread("Undo test");
            var plugin = Activator.CreateInstance(AgentType("WorkspaceEditingPlugin"), true);
            using ((IDisposable)CallStatic("Services.LLMPluginService", "PushPreferredWorkspaceTabId", "resize-tab"))
            {
                var session = NewEditSession(thread);
                using ((IDisposable)session)
                {
                    JObject edit = JObject.Parse((string)Call(plugin, "SetMemoTextInWorkspace", memoId, "Changed memo content", null));
                    Assert(edit["automaticLayoutCheck"] != null, "Every undoable tool mutation must automatically return a layout report.");
                    var backgroundMove = Task.Run(() => Call(plugin, "MoveMemoInWorkspace", memoId, 500.0, 450.0, null));
                    PumpUiUntil(backgroundMove);
                    Call(plugin, "SetComponentTextInWorkspace", inputId, "Changed input", null);
                    Call(plugin, "AddMemoToWorkspace", "New memo", 500.0, 650.0, null);
                    Call(session, "Complete");
                }
                AgentWorkspaceChange change = Changes(thread).Single();
                Assert(change.CanUndo && (string)CallStatic("WorkspaceEditingPlugin", "ReadPlainTextFromMemoModel", memo) == "Changed memo content", "Completed edits must expose a live undo group.");
                Assert((bool)Call(change, "Undo"), "Chat undo must undo the entire group.");
                Assert(model.GetAllTextModels().Count == 1 && memo.GetPosition() == originalPosition &&
                    (string)CallStatic("WorkspaceEditingPlugin", "ReadPlainTextFromMemoModel", memo) == "Original memo content" &&
                    (string)input.Plugin.Settings.GetType().GetProperty("Text").GetValue(input.Plugin.Settings) == "Original input", "Group undo must restore creation, movement, memo contents and component inputs together.");
                model.UndoRedoManager.Redo();
                Assert(change.CanUndo && model.GetAllTextModels().Count == 2 && (string)CallStatic("WorkspaceEditingPlugin", "ReadPlainTextFromMemoModel", memo) == "Changed memo content", "Group redo must restore all edits together.");
                model.ModifyModel(new WorkspaceManagerModel.Model.Operations.MoveModelElementOperation(memo, new System.Windows.Point(600, 450)), true);
                Assert(!change.CanUndo && !(bool)Call(change, "Undo") && memo.GetPosition().X == 600, "Chat undo must not remove later manual changes.");
                model.UndoRedoManager.Undo();
                Assert(change.CanUndo, "Undoing the later manual step must make the exact agent group available again.");
                adapter.Workspace = null;
                Assert(!change.CanUndo && !(bool)Call(change, "Undo"), "Closed or replaced tabs must invalidate undo handles.");
                adapter.Workspace = editor;
                var interrupted = NewEditSession(thread);
                using ((IDisposable)interrupted)
                {
                    Call(plugin, "MoveMemoInWorkspace", memoId, 700.0, 450.0, null);
                    model.ModifyModel(new WorkspaceManagerModel.Model.Operations.MoveModelElementOperation(memo, new System.Windows.Point(750, 450)), true);
                    Call(plugin, "MoveMemoInWorkspace", memoId, 800.0, 450.0, null);
                    Call(interrupted, "Complete");
                }
                Assert(!Changes(thread).Last().CanUndo, "Interleaved manual changes must prevent unsafe collapsing of an agent group.");
                var readOnlyThread = new AIThread("Read only");
                var readOnlySession = NewEditSession(readOnlyThread);
                using ((IDisposable)readOnlySession)
                {
                    var status = Activator.CreateInstance(AgentType("WorkspaceStatusPlugin"), true);
                    Call(status, "CheckWorkspaceLayout", 40.0, null);
                    Call(readOnlySession, "Complete");
                }
                Assert(Changes(readOnlyThread).Length == 0, "Read-only requests must not produce an undo button.");
            }
            Console.WriteLine("PASS: agent-owned grouped Undo/Redo, memo/input restoration, automatic checks and manual/closed-tab safety");
        }
        finally { window.Close(); }
    }

    private static void TemplateSearchUsesBothLanguages()
    {
        var property = typeof(CrypTool.PluginBase.IO.DirectoryHelper).GetProperty("DirectorySamples");
        string original = (string)property.GetValue(null);
        string root = Path.Combine(TestDirectory, "catalog");
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "example.cwm"), "Not deserialized by search");
        File.WriteAllText(Path.Combine(root, "example.xml"), "<sample><title lang=\"en\">A ready example</title><title lang=\"de\">Ein fertiges Beispiel</title><summary lang=\"en\">A substitution demo</summary><summary lang=\"de\">Ein Verschiebungsbeispiel</summary><description lang=\"en\">Use a numeric key</description><keywords lang=\"en\">Caesar, shift</keywords><keywords lang=\"de\">Caesar, Verschlüsselung</keywords></sample>");
        var adapter = new ClosedWorkspaceAdapter();
        CrypWinPort.Instance = adapter;
        try
        {
            property.GetSetMethod(true).Invoke(null, new object[] { root });
            object plugin = Activator.CreateInstance(AgentType("TemplateCatalogPlugin"), true);
            foreach (string query in new[] { "Caesar", "Verschlüsselung", "Verschiebungsbeispiel", "numeric" })
            {
                JObject result = JObject.Parse((string)Call(plugin, "SearchTemplates", query, 8));
                Assert((int)result["count"] == 1 && (string)result["templates"][0]["templatePath"] == "example.cwm", "Template search must match English/German keywords, summaries and descriptions without loading the workspace.");
            }
            JObject empty = JObject.Parse((string)Call(plugin, "SearchTemplates", "unrelated-algorithm", 8));
            Assert((int)empty["count"] == 0, "Unrelated tasks must not receive an invented template match.");
            Assert(adapter.TemplateOpenCalls == 0, "Template suggestions must not open a template before the user chooses.");
            Console.WriteLine("PASS: read-only template search over English/German metadata and actual paths");
        }
        finally { property.GetSetMethod(true).Invoke(null, new object[] { original }); }
    }

    private static void LocalizedResourcesResolve()
    {
        var resources = CrypTool.CrypLLM.Properties.Resources.ResourceManager;
        foreach (string key in new[] { "ExportChatTooltip", "AgentInstructionsTab", "AiChatDeleteAllConfirmation", "AiChatInvocationError", "AiChatFatalInitializationError", "LocalApiKeyLabel", "LocalApiKeyTooltip", "ContextCompressionSection", "AutoCompressContextLabel", "ContextCompressionTriggerLabel", "ContextCompressionTargetLabel", "ContextCompressionHint", "ContextCompressionInvalidPercent", "AiChatContextUsageFormat", "AiChatContextUsageUnknownFormat", "AiChatContextUsageTooltip", "AiChatNoVisibleModelAnswer" })
        {
            string english = resources.GetString(key, CultureInfo.GetCultureInfo("en"));
            string german = resources.GetString(key, CultureInfo.GetCultureInfo("de"));
            Assert(!string.IsNullOrWhiteSpace(english) && !string.IsNullOrWhiteSpace(german) && english != german, "Missing translation: " + key);
        }
        Console.WriteLine("PASS: German/English compiled resources");
        foreach (string key in new[] { "AiChatUndoAgentChanges", "AiChatAgentChangesUndone", "AiChatAgentUndoUnavailable", "AiChatAgentChangesCount", "AiChatLayoutUnavailable", "AiChatLayoutSummary", "AiChatLayoutPartialSummary", "AiChatAgentUndoContextNotice" })
        {
            string english = resources.GetString(key, CultureInfo.GetCultureInfo("en"));
            string german = resources.GetString(key, CultureInfo.GetCultureInfo("de"));
            Assert(!string.IsNullOrWhiteSpace(english) && !string.IsNullOrWhiteSpace(german) && english != german, "Missing undo/layout translation: " + key);
        }
        var toolResources = new System.Resources.ResourceManager("CrypTool.CrypLLM.Properties.ToolDescriptions", AgentAssembly);
        foreach (string key in new[] { "ToolDescription_ws_resize_component", "ToolDescription_ws_resize_memo", "ToolDescription_ws_set_connector_orientation", "ToolDescription_ws_fit_memo", "ToolDescription_ws_check_layout", "ToolDescription_tpl_search" })
        {
            string english = toolResources.GetString(key, CultureInfo.GetCultureInfo("en"));
            string german = toolResources.GetString(key, CultureInfo.GetCultureInfo("de"));
            Assert(!string.IsNullOrWhiteSpace(english) && !string.IsNullOrWhiteSpace(german) && english != german, "Missing tool translation: " + key);
        }
    }

    private sealed class AuthenticationCompletionHandler : HttpMessageHandler
    {
        internal string Scheme;
        internal string Token;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Scheme = request.Headers.Authorization?.Scheme;
            Token = request.Headers.Authorization?.Parameter;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\":\"test-auth\",\"object\":\"chat.completion\",\"created\":1,\"model\":\"gpt-5.4-mini\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":\"test reply\"},\"finish_reason\":\"stop\"}]}", Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class DelayedCompletionHandler : HttpMessageHandler
    {
        internal readonly TaskCompletionSource<bool> Started = new TaskCompletionSource<bool>();
        private readonly TaskCompletionSource<HttpResponseMessage> Response = new TaskCompletionSource<HttpResponseMessage>();
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Started.TrySetResult(true);
            return Response.Task;
        }
        internal void Release()
        {
            Response.SetResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\":\"test-completion\",\"object\":\"chat.completion\",\"created\":1,\"model\":\"gpt-5.4-mini\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":\"test reply\"},\"finish_reason\":\"stop\"}]}", Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class ClosedWorkspaceAdapter : ICrypWinAdapter
    {
        internal IEditor Workspace;
        internal int ActiveEditorReads;
        internal int TemplateOpenCalls;
        public IEditor ActiveEditor { get { ActiveEditorReads++; return null; } }
        public IEnumerable<OpenTabsAbstraction> GetOpenTabs() { return new OpenTabsAbstraction[0]; }
        public bool TryGetWorkspaceEditorByTabId(string id, out IEditor editor) { editor = id == "resize-tab" ? Workspace : null; return editor != null; }
        public bool TryGetDocumentationContextByTabId(string id, out DocumentationContextAbstraction context) { context = null; return false; }
        public IList<string> GetRecentLogMessages(int maxLines) { return new List<string>(); }
        public void ShowAIChatPane() { }
        public void ShowAIChatSettings() { }
        public OpenTabsAbstraction CreateEmptyWorkspaceTab() { return null; }
        public OpenTabsAbstraction OpenTemplateWorkspaceTab(string path) { TemplateOpenCalls++; return null; }
    }
}
