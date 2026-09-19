# CrypLLM: AI chat agent for CrypTool 2

CrypLLM provides an AI chat interface for inspecting, building and editing CrypTool 2 workspaces. It supports the OpenAI API and OpenAI-compatible model servers, configurable tool permissions and agent instructions, workspace screenshots, context compression and native Undo/Redo integration.

The chat interface and settings are localized in English and German. CrypLLM connects to the application through [`CrypWinAdapter`](../CrypWin/Adapter/CrypWinAdapter.cs).

## Contents

- [Build and run](#build-and-run)
- [Enable the chat and configure a model](#enable-the-chat-and-configure-a-model)
- [Example workflow](#example-workflow)
- [Tool permissions and agent instructions](#tool-permissions-and-agent-instructions)
- [Tool reference](#tool-reference)
- [Context usage and automatic compression](#context-usage-and-automatic-compression)
- [Answer display and diagnostics](#answer-display-and-diagnostics)
- [Persistence and data handling](#persistence-and-data-handling)
- [Architecture and development](#architecture-and-development)
- [Troubleshooting](#troubleshooting)
- [Regression tests](#regression-tests)

## Build and run

Requirements:

- Windows and Visual Studio with MSBuild and the .NET desktop development workload.
- The .NET Framework 4.7.2 targeting pack.
- NuGet CLI available for package restoration.

Run these commands from the repository root in a Developer PowerShell:

```powershell
nuget restore 'CrypTool 2.sln'
msbuild 'CrypTool 2.sln' /p:Configuration=Debug /p:Platform=x64 /m
& './CrypBuild/Debug/CrypWin.exe'
```

Building the solution includes the workspace components. For incremental application builds, use:

```powershell
msbuild CrypWin/CrypWin.csproj /p:Configuration=Debug /p:Platform=x64 /m
```

An application-only build does not build every component. Use the solution build when preparing a complete application or when component DLLs are missing.

Agent dependencies are stored in `Lib` or declared in `packages.config`. Build outputs and the local `packages` directory are excluded from Git.

## Enable the chat and configure a model

Open CrypTool's settings and select **AI Chat**. The ribbon button is hidden by default; enable **Show AI chat in the ribbon bar** to display it. Opening the chat at startup is a separate setting. The chat's gear button also opens its settings.

Choose a provider and configure its model IDs:

| Provider | Configuration |
| --- | --- |
| OpenAI API | Enter your OpenAI API key and the model IDs you want to use. |
| Local model / OpenAI-compatible server | Enter the server endpoint, model IDs and an optional API key. The default endpoint is `http://127.0.0.1:1234/v1`. |

For local servers, use the model ID recognized by that server. The optional local API key is separate from the OpenAI key and is sent as a Bearer token. Keys are stored encrypted for the current Windows user using Windows DPAPI. If the local key is empty, the connector uses the `no-api-key` placeholder.

Custom model IDs are supported. Set their actual context window in the model context settings, with a minimum of 4,096 tokens. Requests require a known or explicitly configured context limit. Select the model in the chat before sending a message.

### OpenAI-compatible server example

For a server exposing the OpenAI-compatible API at `http://127.0.0.1:1234/v1`, configure:

```text
Provider: Local model (LM Studio compatible)
Endpoint: http://127.0.0.1:1234/v1
Model ID: my-local-model
API key: leave empty unless the server requires authentication
Context window: the actual token limit configured on the server
```

`my-local-model` is an example; replace it with the exact ID your server accepts. The endpoint is the API base URL, not the server's web interface or the complete `/chat/completions` URL. For a remote compatible server, use its corresponding API base URL and credentials.

The model must support tool calls to edit or inspect workspaces through the agent. Image support is additionally required for screenshot inspection. A server accepting chat completion requests does not necessarily support either capability.

User-defined context windows override built-in mappings for the same model ID. Configure the window actually available to that server session, which may be smaller than the model's theoretical maximum. The configured value is used for local budgeting; it does not increase the model server's capacity.

### Settings overview

| Setting | Behavior |
| --- | --- |
| Provider and model IDs | Maintain separate model lists for OpenAI and compatible servers. |
| Model context window | Set the context limit used for prompt budgeting and the chat indicator. |
| Reasoning | Select the reasoning effort sent to the connector; support depends on the model/server. |
| CrypLLM logs | Select the agent logging level, including debug diagnostics. |
| Show AI chat in the ribbon bar | Display the chat entry point; disabled by default. |
| Open AI chat at startup | Open the chat automatically; disabled by default. |
| Allow modifying tools | Enable tools that change or execute workspaces; disabled by default. |
| Tool permissions | Enable or disable individual tools. |
| Context compression | Configure automatic reduction, its trigger and its target. |
| Delete all AI chats | Remove the locally stored conversations after confirmation. |

Keep API keys in user settings rather than repository files. When using a remote provider, workspace data, chat messages and requested screenshots may be sent to that provider.

## Example workflow

1. Build the complete application and open a workspace.
2. Configure a provider, its model ID and context window, then enable the ribbon entry point.
3. In **Tool permissions**, enable modifying tools and the specific inspection, editing and execution tools needed for the task.
4. Open the chat, choose the model and describe the desired workflow and verification criteria.
5. If the agent recommends an existing template, choose whether to open it or build a workspace from scratch.
6. Review tool activity, the resulting workspace, layout findings and actual execution outputs. Use the grouped undo control if the changes should be reverted.

For example:

> Build a Caesar encryption and decryption example for ATTACK AT DAWN with key 3. Use a NumberInput connected to the key connectors, show the intermediate ciphertext and recovered plaintext in separate TextOutput components, and add a readable memo. Avoid component overlaps and wires passing through boxes. Run the workspace, check that the recovered plaintext matches the input, and inspect the final layout.

For an existing workspace:

> Inspect this workspace without changing it. Explain its data flow, report validation errors and check for overlapping components, clipped memos and wires passing through boxes.

Permissions remain authoritative even when a prompt asks for a disabled action. For an inspection-only session, leave modifying tools disabled. To limit changes more narrowly, enable only the individual editing tools needed.

## Tool permissions and agent instructions

The **Tool permissions** settings control which tools the agent can use. The **Agent instructions** tab provides configurable instructions. Runtime workspace construction rules also apply to existing saved agent profiles.

Modifying tools have a global permission switch in addition to their individual permissions. Enabling a single modifying tool does not bypass that switch. Permission checks also run when a tool is invoked. Tool activity distinguishes completed calls, denied calls and failures; a denied call does not mean the requested operation happened.

Tool calls have no separate token quota or fixed round limit. Before each model call, retained conversation and tool results are checked against the model's context capacity and compressed when needed. User permissions and cancellation remain authoritative.

### Continuous tool execution

The agent runs explicit tool rounds until the model returns its answer or the user cancels. There is no cumulative token allowance per user request and no application-imposed maximum number of rounds. The SDK's automatic 128-round invocation ceiling is avoided by invoking tool rounds explicitly, through the same permission filters.

Each completed assistant turn is published to the chat immediately while later tool rounds continue. The chat follows the growing response to the bottom, so progress text and tool activity remain visible throughout longer workspace tasks. Publishing uses the same archived message objects and therefore does not duplicate intermediate turns when the final response completes.

Results are not truncated merely because previous tools consumed a quota. Instead, context preparation runs before every outbound model call. It summarizes older exchanges as complete groups and reuses a reduced-prefix cache when new results arrive. The full conversation archive remains intact.

The current task, system instructions and latest complete tool exchange are retained. Parallel calls and their results stay together, and the latest screenshot's image input remains after its result group. A single oversized tool result can be summarized when necessary to fit the actual input context.

Automatic compression follows the configured trigger and target. Mandatory reduction still protects against context overflow when proactive compression is disabled. If a semantic summary cannot be produced, a bounded archive preview is explicitly labeled so the agent knows to re-inspect omitted details.

The actual model context window still limits each request. If the current task, tool definitions, image inputs and necessary exchange cannot fit even after reduction, the request reports a context-capacity error. Configure the actual server capacity; removing a local tool quota does not enlarge it. Continuous execution can be stopped with the chat's cancel control.

Workspace operations stay bound to the workspace selected when the request starts. Switching chats or tabs does not redirect an ongoing request. A fabricated or stale model-supplied tab ID cannot displace a still-open request pin; a valid explicit tab can be selected only while the original pin remains valid. If the pinned workspace is closed, subsequent operations report an error rather than redirecting to another editor.

Mutation tools require exact runtime IDs from workspace inspection. Removal and connection tools return current component IDs, names and types when a component ID is unknown, allowing the agent to recover instead of guessing another identifier. The runtime instructions require successful mutation results and an exact `ws_model` topology check before reporting a structural change. Matching `ws_io` values alone do not prove that requested components or connections were changed.

### Workspace inspection and screenshots

The agent can inspect workspace structure, connector types, settings, values, bounds and validation results. With `ws_screenshot`, it can also inspect a PNG of the visible workspace viewport at its current zoom and scroll position.

Screenshots are limited to 1,024 pixels per side and are sent as image inputs to the model server. The selected model must support image inputs. The tool captures the workspace viewport only, requires the target tab to be visible and does not modify the workspace. Elements outside that viewport are not visually verified.

### Component and memo editing

The editing tools support component creation, connections, positions, sizes, memo text and connector orientation. Native workspace operations preserve Undo/Redo and saving behavior.

| Tool | Purpose |
| --- | --- |
| `ws_resize_component(componentId, width, height)` | Resize a component while preserving its position, contents and connections. |
| `ws_resize_memo(memoId, width, height)` | Resize an existing memo. |
| `ws_set_memo_text` | Update memo contents independently of its size. |
| `ws_fit_memo` | Measure formatted memo contents and adjust its size to fit. |
| `ws_set_connector_orientation(componentId, connectorName, orientation)` | Move an input or output connector to `North`, `South`, `East` or `West`. |
| `ws_bounds` | Inspect component and memo geometry. |
| `ws_check_layout` | Check element overlaps, memo clipping and available wire routes. |

Sizes are canvas units, independent of workspace zoom. Resize tools accept positive finite dimensions up to 10,000 and respect the view's minimum and maximum sizes. Results report the applied dimensions. Components in icon view switch to a resizable presentation or settings view when available.

Connector names are the technical `Name` values returned by `ws_model`. Orientation names are case-insensitive. Moving a connector preserves its identity, input/output direction and connections, and triggers wire routing updates. A shared output has one orientation for all its connections.

AI-created memos do not automatically become selected or take focus from the chat input. Manual memo creation retains its normal selection behavior.

### Geometry and layout checks

`ws_bounds` and `ws_model` distinguish several measurements:

- `x` and `y`: the anchor used to move the element.
- `width` and `height`: the resizable inner window dimensions.
- `bodyBounds`: the actual box, including its offset from the anchor.
- `occupiedBounds`: the visible footprint, including connector rails and captions.
- `sizeSource`: whether dimensions came from the visual view or a model/minimum-size fallback.

Stored dimensions of zero mean automatic sizing. Use `occupiedBounds` when planning element spacing; combining the move anchor with the inner window size does not describe the complete visible footprint.

Layout checks run after undoable AI edits and can also be requested explicitly. They report overlapping elements, clipped memo text, wire crossings and shared wire segments. A wire passing through any box interior is an error, including its own source or target. Reports include affected IDs, connector names and conditional routing advice. Missing measurements are reported as an incomplete check.

The agent's construction instructions require it to:

- Prefer connected input components for keys and parameters over internal settings. Use TextInput for text and NumberInput for compatible numeric inputs, including Caesar's integer `ShiftKey`.
- Start TextInput and TextOutput at approximately 400 × 220 canvas units and enlarge them for longer content. Fit memos to their complete text, with a typical starting width of 600 units.
- Keep components and memos from overlapping, leave at least 40 units between boxes and reserve additional space for wire corridors.
- Arrange the main data flow from left to right, with parameter inputs in separate rows. For East/West connections, place the entire source before its receiver. Use suitable North/South connectors or clear side corridors for vertical branches.
- Recheck every branch after changing a shared connector's orientation and recalculate spacing after resizing.
- Inspect the final model, bounds, layout and validation results. Where permitted, run executable workflows and check their actual outputs and errors. With a vision-capable model and screenshot access, inspect the final layout visually and repeat the check after corrections.

These are agent instructions and diagnostic checks; they do not prevent every poor layout automatically. Results depend on the model and the tools it is allowed to use.

### Grouped undo

After the agent changes a workspace, the chat displays a control to undo its changes as a group. Groups use the native workspace undo stack and preserve manual edits. Group undo becomes unavailable if the workspace is closed or its undo history changes in a way that invalidates the group. The workspace's normal Undo remains available for its existing history.

### Template suggestions

The agent can search existing templates with `tpl_search` and inspect their details with `tpl_info`. When a suitable template exists, its instructions require it to explain the recommendation and ask whether to open that template or build the workspace itself before making changes, unless the user has already chosen an approach.

`tpl_open` opens the selected template in a new tab. Searching or inspecting template metadata does not itself open or modify a workspace. Template preference is an instruction to the model, not an automatic substitution for the user's request.

## Tool reference

Tools are Semantic Kernel functions, not a separate command-line interface. The tables below list their logical names; provider requests may qualify them with their plugin name. Availability depends on the registered plugins and the user's permissions.

Workspace tools generally accept an optional `tabId`. When it is omitted, they use the workspace bound to the request. Component and memo IDs come from workspace inspection results. Component type names and connector names are technical identifiers; the agent should discover them rather than infer them from translated captions.

### Workspace status

| Tool | Purpose |
| --- | --- |
| `tabs_list` | List open editor tabs and their IDs. |
| `ws_model` | Read workspace components, memos, connectors, connections and available bounds. |
| `ws_validate` | Inspect workspace validation findings. |
| `ws_io` | Read supported input/output text values. |
| `ws_settings` | Read component settings. |
| `ws_bounds` | Read element positions, dimensions and measurement sources. |
| `ws_check_layout` | Check overlaps, memo clipping and measured wire routes; default minimum gap is 40 units. |
| `ws_screenshot` | Capture the visible target workspace viewport as an image input. |
| `app_log_recent` | Read recent application log entries. |

### Workspace editing and execution

| Tool | Purpose |
| --- | --- |
| `ws_add_component` | Create a component using its full type name, with optional name and position. |
| `ws_add_connection` | Connect named source and target connectors on existing components. |
| `ws_set_component_text` | Set text on a supported input component. |
| `ws_set_component_name` | Change a component's displayed name. |
| `ws_set_component_setting` | Set a component setting using its property name and value. |
| `ws_move_component` | Change a component's canvas position. |
| `ws_resize_component` | Change a component's inner window size. |
| `ws_set_connector_orientation` | Move a connector to a compass side of its component. |
| `ws_add_memo` | Add an explanatory memo without selecting it or taking chat focus. |
| `ws_set_memo_text` | Replace a memo's contents. |
| `ws_move_memo` | Change a memo's canvas position. |
| `ws_resize_memo` | Set a memo's dimensions. |
| `ws_fit_memo` | Fit a memo to its formatted contents, with an optional width. |
| `ws_remove_component` | Remove an existing component. |
| `ws_remove_memo` | Remove an existing memo. |
| `ws_remove_connection` | Remove a connection between named connectors. |
| `ws_run` / `ws_stop` | Start or stop workspace execution. |
| `wait_seconds` | Wait for execution results or other asynchronous workspace updates. |

Setting component text and setting a component property are different operations. The agent should use connector metadata and component documentation to choose a compatible input component and value type. A tool call reporting success does not prove that the complete workflow produces the intended result; validate and inspect actual outputs separately.

### Component catalog

| Tool | Purpose |
| --- | --- |
| `comp_list` | List available workspace components, optionally filtered by category and including tooltips. |
| `comp_list_by_category` | List components within a category. |
| `comp_fullnames` | Read full component type names for creation and inspection. |
| `comp_docs` | Read component documentation as HTML. |
| `comp_defaults` | Inspect a component's default settings. |

The catalog reflects the components available to the running application. If component DLLs are absent from the build, the agent cannot compensate by inventing their types.

### Template catalog

| Tool | Purpose |
| --- | --- |
| `tpl_search` | Search ranked template metadata; returns up to eight results by default. |
| `tpl_list` | List available templates. |
| `tpl_info` | Read details for a template path. |
| `tpl_open` | Open a template in a new editor tab. |
| `tpl_model` | Inspect a template's workspace structure. |
| `tpl_io` | Read a template's supported input/output texts. |
| `tpl_bounds` | Read a template's element bounds. |
| `tpl_settings` | Read a template's component settings. |

### Application and tutorial documentation

| Tool | Purpose |
| --- | --- |
| `text_ui` | Retrieve application UI documentation for a topic. |
| `open_documentation_text` | Obtain documentation context for an editor tab. |
| `tutorial_docs_all` | Retrieve the available documentation bundle for a tutorial. |
| `tutorial_docs_summary` | Retrieve a condensed tutorial documentation bundle. |

## Context usage and automatic compression

The context indicator beside the model selector estimates the latest model request plus its received response. It updates at every model call, including calls within a tool loop, and includes system instructions, tool definitions, tool results and images. After compression, it counts the reduced context that is actually sent. Before a request is available, it estimates retained history and agent instructions/tools.

The displayed limit comes from the model configuration. Token counts are estimates, including a fixed image cost rather than the size of base64 image data. The indicator does not sum usage across multiple requests. Unsent input and unused response reserves are excluded.

Under **AI Chat settings → Context compression**, automatic compression is enabled by default with a 90% trigger and a 50% target. Both percentages are configurable; the target must be below the trigger.

These percentages refer to the available **chat history budget**, after reserves for system instructions, tool definitions and responses. They therefore differ from the percentage shown against the full context window in the chat footer.

Compression happens before new user requests and within running tool loops before subsequent model calls, summarizing older messages. The complete conversation and its images remain saved. A reduced-history cache avoids summarizing the same archived messages repeatedly. Mandatory context overflow checks remain active when automatic compression is disabled.

For example, if the available history budget is 100,000 tokens, a 90% trigger starts reduction at approximately 90,000 history tokens and aims for at most 50,000. This does not mean that the footer must show 90% of the full model window before compression can occur.

Context preparation keeps structured tool calls and their corresponding results together. The full archive remains the source for the chat display, while a reduced invocation thread may be used for the model request. Switching models uses that model's configured limit; it does not silently replace an unknown limit with an arbitrary value.

## Answer display and diagnostics

The chat and readable text export hide explicitly marked reasoning such as `think`, `analysis` and `reasoning`. The agent is instructed to place visible answers inside `<ct2_answer>...</ct2_answer>`; when those markers are present, text outside them is hidden. Ordinary unmarked answers remain supported.

Responses containing internal tool protocol fragments without a clear answer section are withheld because their reasoning cannot be separated reliably. A localized notice appears when a new response has no displayable answer. Literal code examples are preserved, and original responses and HTTP diagnostics remain stored.

Before subsequent model calls, previous assistant text is cleaned accordingly. Structured tool calls and results remain intact. Completely unmarked reasoning without identifiable protocol fragments cannot be reliably recognized by the client; the model server must separate it, or the model must follow the answer markers.

The **Debug** view provides invocation details such as context budgets, retained prompt messages, tool calls, reductions and captured HTTP exchanges. Authorization headers are redacted in captured diagnostics. Message bodies can still contain workspace data and user-provided content.

## Persistence and data handling

CrypLLM stores its local conversation archive in `LLM/AIThreads.history.json` under CrypTool's local application data directory. Despite its filename, the persisted contents are DPAPI-encrypted and encoded as Base64. They include structured tool call/result messages and the reduced-history cache; synthetic greetings are not archived as conversation messages.

Custom instruction entries are stored separately in `LLM/AgentInstructions.json`. Model lists, permission settings, context limits and encrypted API keys use the application's user settings.

The chat's export control writes a user-selected JSON file with conversation text and structured message items. Exported assistant text uses the answer display filtering. Export files are readable JSON rather than the encrypted local history format, and can contain tool results and workspace information.

Grouped undo handles and live context snapshots are runtime state. Undo controls refer to native workspace undo stacks and are not persisted as reusable handles across application restarts.

DPAPI protection is tied to the Windows user. Copying encrypted settings or history to another account is not a supported way to transfer usable credentials or conversations. Local encryption does not change which data is sent to the configured model provider during a request.

## Architecture and development

| Area | Responsibility |
| --- | --- |
| [`AIChatContent.xaml`](AIChatContent.xaml) and its code-behind | Chat UI, model selection, tool activity, context indicator, undo controls and export. |
| [`LLMSettingsTab.xaml`](LLMSettingsTab.xaml) and its code-behind | Provider configuration, permissions, instructions and context settings. |
| [`Threads/AIThreadManager.cs`](Threads/AIThreadManager.cs) | Agent creation, request binding, invocation lifecycle, HTTP capture and history persistence. |
| [`Threads/AIThread.cs`](Threads/AIThread.cs) | Per-conversation history, reduced cache, tool activity, runtime context usage and undo handles. |
| [`Threads/ChatHistoryReductionEngine.cs`](Threads/ChatHistoryReductionEngine.cs) | Token estimation, context capacity planning and history compression. |
| [`Threads/ToolPermissionFilter.cs`](Threads/ToolPermissionFilter.cs) | Permission enforcement, tool activity reporting and execution diagnostics. |
| [`Threads/AgentEditSession.cs`](Threads/AgentEditSession.cs) | Track AI-owned changes, run layout checks and create native undo groups. |
| [`Threads/AssistantResponseText.cs`](Threads/AssistantResponseText.cs) | Filter visible answers and normalize prior assistant text for requests. |
| [`Threads/WorkspaceScreenshotContent.cs`](Threads/WorkspaceScreenshotContent.cs) | Convert screenshot results into image inputs for model requests. |
| [`Threads/LiveContextCompressor.cs`](Threads/LiveContextCompressor.cs) | Compress actual model requests within tool loops while retaining complete exchanges and caching reduced prefixes. |
| `Plugins/` | Workspace, component catalog, template catalog and documentation tools. |
| [`Services/LLMPluginService.cs`](Services/LLMPluginService.cs) | Marshal workspace operations onto the UI thread and perform native edits. |
| `Services/Workspace*` | Inspect element geometry and workspace structure, and check layout. |
| `Ports/` and `CrypWin/Adapter/` | Bridge the agent to application editors and logging. |
| `AgentInstructions/` | Built-in instruction catalog and editable instruction entries. |
| `Properties/` | Settings and English/German resource files, including tool descriptions. |

### Request lifecycle

1. Capture the selected conversation, agent and workspace target.
2. Resolve the model context window and calculate the prompt budget.
3. Prepare retained history, using compression and its cache where needed, and validate the request budget.
4. Invoke the Semantic Kernel chat agent. Each tool invocation passes through permission checks; workspace edits run on the UI thread.
5. Normalize outbound assistant text and screenshot inputs, compress the current request if needed, then update the live context estimate from each actual model request and response.
6. Preserve new messages in the full conversation archive, complete tool activity, check edited layouts and finalize eligible undo groups.
7. Display the filtered answer and save conversation state.

### Adding or changing tools

Use the existing plugins as examples. A tool needs a Semantic Kernel function name and description, appropriate permission handling, and registration through the agent's plugin setup. Classify modifying tools correctly so they cannot bypass the global modifying-tools switch.

Workspace edits should use `LLMPluginService` and native model operations rather than direct view mutations. Preserve the request's workspace target, marshal UI work appropriately and keep native Undo/Redo behavior. Supply useful result metadata so the agent can verify the applied change instead of assuming that requested values were accepted unchanged.

Keep user-visible strings and tool descriptions in the English/German resource files. Technical IDs and connector orientation values remain stable across UI languages. Add regression coverage for meaningful behavior changes, particularly request isolation, permission enforcement, image transport and native undo integration.

## Troubleshooting

| Symptom | What to check |
| --- | --- |
| No AI chat ribbon button | Enable **Show AI chat in the ribbon bar** in settings. |
| No selectable model | Select the provider and populate its model ID list, then choose a model in the chat. |
| Request blocked because the context limit is unknown | Configure the context window for the exact selected model ID. |
| Compatible server rejects the request | Verify its API base URL, model ID, credentials and support for the requested tool/image capabilities. |
| Tool call denied | Check both the individual permission and the global modifying-tools switch. Calls are not denied by a separate tool quota. |
| Agent cannot find expected components | Build the complete solution and check that the running build contains the component DLLs. |
| Screenshot unavailable | Keep the request-bound workspace open and visible, allow `ws_screenshot` and select a vision-capable model. |
| Layout report is incomplete | Some visual measurements or routes were unavailable. Do not treat `passed` with `complete=false` as full verification. |
| Wires pass through components | Inspect `bodyBounds`/`occupiedBounds`, move receivers into clear corridors or adjust connector sides, then recheck every affected branch. |
| Memo text is clipped | Use `ws_fit_memo` or resize the memo, then check its measured contents and neighboring elements. |
| Grouped undo is unavailable | The workspace may have closed or manual edits/history changes may have invalidated the group. Use native workspace Undo as appropriate. |
| Context percentage appears different from the compression trigger | The footer uses the full configured window; compression uses the available history budget after reserves. |
| Answer is hidden or the server remains in reasoning mode | Inspect the answer filtering notice and server settings. Use separated reasoning output or the answer markers; unsupported server behavior cannot be reliably repaired by display filtering. |

For provider failures, inspect the application log and the invocation Debug view. Diagnostics may contain workspace data even when credentials in headers have been redacted.

## Regression tests

Build Release/x64, then run the regression script from the repository root:

```powershell
msbuild CrypWin/CrypWin.csproj /p:Configuration=Release /p:Platform=x64 /m
& './CrypLLM/Tests/Run-AgentRegressionTests.ps1' -Configuration Release
```

To test an existing Debug build, pass `-Configuration Debug` instead.

The suite covers instruction loading, chat persistence, request isolation across chat/workspace changes, provider authentication and encrypted key storage, answer filtering, screenshot capture and image transport, live context estimates, repeated compression in a 160-round tool loop, archive preservation on cancellation, complete parallel tool groups and images, compression caching and fallback, component/memo editing, connector orientation, layout checks, grouped Undo/Redo, template search and English/German resources.

Provider responses and application adapters are local test doubles. The tests do not require a running model server or real API credentials and do not use the user's chat history files.
