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
using CrypTool.CrypLLM.Converter;
using CrypTool.CrypLLM.Helper;
using CrypTool.CrypLLM.Threads;
using CrypTool.PluginBase;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Attributes = CrypTool.PluginBase.Attributes;

// Module overview:
// WPF chat view/controller coordinating UI state, thread selection,
// request execution, and chat export for reproducible diagnostics.
namespace CrypTool.CrypLLM
{
    /// <summary>
    /// Interaction logic and binding source for the AI chat content area.
    /// This control hosts:
    /// - Observable message collection used to bind the chat UI.
    /// - Thread selection and synchronization with the AIThreadManager.
    /// - Request lifecycle (start/stop), cancellation, and UI state toggling.
    /// Concurrency: All UI updates are marshalled to the UI thread via Dispatcher.
    /// </summary>
    [Attributes.Localization("CrypTool.CrypWin.Properties.Resources")]
    public partial class AIChatContent : UserControl, INotifyPropertyChanged
    {
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Bound to the chat UI; mirrors the active thread's chat history.
        /// </summary>
        public ObservableCollection<ChatMessage> Messages { get; } = new ObservableCollection<ChatMessage>();

        /// <summary>
        /// Observable mirror of AI threads for the thread selector.
        /// </summary>
        public ObservableCollection<AIThread> ObservableThreads { get; } = new ObservableCollection<AIThread>();
        public ObservableCollection<ToolActivityViewModel> VisibleToolActivities { get; } = new ObservableCollection<ToolActivityViewModel>();

        private static readonly Type[] KnownToolPluginTypes =
        {
            typeof(WorkspaceStatusPlugin),
            typeof(ComponentCatalogPlugin),
            typeof(TemplateCatalogPlugin),
            typeof(UiTextCatalogPlugin),
            typeof(WorkspaceEditingPlugin)
        };
        private static readonly ResourceManager ToolDescriptionsResourceManager =
            new ResourceManager("CrypTool.CrypLLM.Properties.ToolDescriptions", typeof(AIChatContent).Assembly);

        private CancellationTokenSource _requestCts;
        private string _activeRequestCancelTrigger = "none";
        private bool _isRequestRunning;
        private bool _isUnavailableToolsPopupOpen;
        private bool _subscribed;
        private bool _suppressScrollTracking;
        private string _pendingUserPreviewMessage = string.Empty;
        private string _unavailableToolsTooltipText = string.Empty;
        private string _renderedThreadId = string.Empty;
        private readonly Dictionary<string, bool> _toolActivityExpansionStates = new Dictionary<string, bool>(StringComparer.Ordinal);
        private readonly Dictionary<string, bool> _toolActivityDetailExpansionStates = new Dictionary<string, bool>(StringComparer.Ordinal);
        private readonly Dictionary<string, double> _threadScrollOffsets = new Dictionary<string, double>(StringComparer.Ordinal);
        private const double AutoScrollBottomThreshold = 64d;

        /// <summary>
        /// Proxy to the manager's active thread with change notification and UI refresh.
        /// </summary>
        private AIThread ActiveThread
        {
            get => AIThreadManager.Instance.ActiveThread;
            set
            {
                if (AIThreadManager.Instance.ActiveThread != value)
                {
                    AIThreadManager.Instance.ActiveThread = value;
                }
            }
        }

        private Storyboard _spinnerStoryboard;

        public Visibility UnavailableToolsIndicatorVisibility =>
            string.IsNullOrWhiteSpace(_unavailableToolsTooltipText) ? Visibility.Collapsed : Visibility.Visible;

        public string UnavailableToolsTooltipText => _unavailableToolsTooltipText;
        public bool IsUnavailableToolsPopupOpen
        {
            get => _isUnavailableToolsPopupOpen;
            set
            {
                if (_isUnavailableToolsPopupOpen == value)
                {
                    return;
                }

                _isUnavailableToolsPopupOpen = value;
                OnPropertyChanged(nameof(IsUnavailableToolsPopupOpen));
            }
        }
        public Visibility ModelSelectionPlaceholderVisibility =>
            HasSelectedModel ? Visibility.Collapsed : Visibility.Visible;
        public string ModelSelectionPlaceholderText =>
            GetResourceTextOrFallback("AiChatSelectModelPlaceholder", "Please select a model");
        public Visibility ToolActivityPanelVisibility =>
            (_isRequestRunning || VisibleToolActivities.Count > 0) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ToolActivityEmptyStateVisibility =>
            VisibleToolActivities.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        public string ToolActivityArgumentsLabel => GetResourceTextOrFallback("AiChatToolActivityArgumentsLabel", "Arguments");
        public string ToolActivityErrorLabel => GetResourceTextOrFallback("AiChatToolActivityErrorLabel", "Error");
        public string ProcessingTooltipText => GetResourceTextOrFallback("AiChatProcessingTooltip", "Processing...");
        public Visibility ChatWelcomeVisibility => Messages.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        public string ChatWelcomeTitle => GetResourceTextOrFallback("AiChatWelcomeTitle", "Start a conversation");
        public string ChatWelcomeBody => GetResourceTextOrFallback("AiChatWelcomeBody", "Ask a question about cryptography or CrypTool 2. Depending on which tools are enabled, the AI can read the current workspace and open tabs and may also change the workspace.");
        public string ToolActivityHeaderText => BuildRunningToolActivityHeaderText(VisibleToolActivities.Count);
        public string ToolActivityEmptyStateText =>
            _isRequestRunning
                ? GetResourceTextOrFallback("AiChatToolActivityEmptyStateRunning", "No tool calls yet.")
                : GetResourceTextOrFallback("AiChatToolActivityEmptyStateCompleted", "No tool calls in the last response.");
        private bool HasSelectedModel => TryGetSelectedModelId(out _);

        public Visibility ContextUsageVisibility => HasSelectedModel ? Visibility.Visible : Visibility.Collapsed;
        public string ContextUsageTooltip => GetResourceText("AiChatContextUsageTooltip");

        /// <summary>
        /// Displays an estimate of the actual prompt against the selected
        /// model's configured window. Unknown limits never produce a percentage.
        /// </summary>
        public string ContextUsageText
        {
            get
            {
                if (!TryGetSelectedModelId(out string modelId)) return string.Empty;
                int usedTokens = AIThreadManager.Instance.EstimateContextUsageTokens(ActiveThread, modelId);
                if (ModelContextWindowResolver.TryResolveContextWindowTokens(modelId, out int capacity))
                {
                    return string.Format(CultureInfo.CurrentCulture, GetResourceText("AiChatContextUsageFormat"),
                        usedTokens, capacity, (double)usedTokens / capacity);
                }
                return string.Format(CultureInfo.CurrentCulture, GetResourceText("AiChatContextUsageUnknownFormat"), usedTokens);
            }
        }

        private void RefreshContextUsage()
        {
            OnPropertyChanged(nameof(ContextUsageText));
            OnPropertyChanged(nameof(ContextUsageVisibility));
        }

        public AIChatContent()
        {
            InitializeComponent();
            AIThreadManager.ApplyLocalProviderDefaultsMigration();
            DataContext = this;

#if DEBUG
            if (DebugLastRequestButton != null)
            {
               DebugLastRequestButton.Visibility = Visibility.Visible;
            }
#endif

            _spinnerStoryboard = TryFindResource("SpinnerStoryboard") as Storyboard;

            Loaded += AIChatContent_Loaded;
            Unloaded += AIChatContent_Unloaded;

            RefreshModelSelectorFromSettings();
            ThreadSelector.ItemsSource = ObservableThreads;
            RefreshThreads();
            RefreshToolActivities();
            RefreshMessages();
            RefreshUnavailableToolsInfo();
        }

        /// <summary>
        /// Initializes runtime subscriptions and refreshes model selection when the control enters the visual tree.
        /// </summary>
        private void AIChatContent_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshModelSelectorFromSettings();
            SafeSubscribeToThreadManager();

            Properties.Settings.Default.PropertyChanged += Settings_PropertyChanged;
        }

        /// <summary>
        /// Releases runtime subscriptions to prevent duplicate handlers and leaks when the control is unloaded.
        /// </summary>
        private void AIChatContent_Unloaded(object sender, RoutedEventArgs e)
        {
            RememberCurrentThreadScrollOffset();
            SafeUnsubscribeFromThreadManager();
            Properties.Settings.Default.PropertyChanged -= Settings_PropertyChanged;
        }

        /// <summary>
        /// Entry point invoked by the host when the user first opens the chat panel.
        /// </summary>
        public void NotifyUserOpenedChat()
        {
            ShowDisclaimerOnFirstOpen();
        }

        /// <summary>
        /// Reacts to relevant settings updates and refreshes available model choices on the UI thread.
        /// </summary>
        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            bool modelSelectionChange =
                e.PropertyName == nameof(Properties.Settings.selectedProvider) ||
                e.PropertyName == nameof(Properties.Settings.openAiModelIds) ||
                e.PropertyName == nameof(Properties.Settings.activeModelId) ||
                e.PropertyName == nameof(Properties.Settings.localModelIds);
            bool toolAvailabilityChange =
                e.PropertyName == nameof(Properties.Settings.editorStatusAllowedFunctions) ||
                e.PropertyName == nameof(Properties.Settings.allowAiToolMutations) ||
                e.PropertyName == nameof(Properties.Settings.userModelContextWindows) ||
                e.PropertyName == nameof(Properties.Settings.developerModelContextWindows);

            if (modelSelectionChange || toolAvailabilityChange)
            {
                if (Dispatcher.CheckAccess())
                {
                    if (modelSelectionChange)
                    {
                        RefreshModelSelectorFromSettings();
                    }
                    else
                    {
                        RefreshUnavailableToolsInfo();
                    }
                }
                else
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (modelSelectionChange)
                        {
                            RefreshModelSelectorFromSettings();
                        }
                        else
                        {
                            RefreshUnavailableToolsInfo();
                        }
                    }));
                }
            }
        }

        /// <summary>
        /// Displays the one-time AI usage disclaimer and persists acknowledgment in settings.
        /// </summary>
        private void ShowDisclaimerOnFirstOpen()
        {
            try
            {
                if (Properties.Settings.Default.aiDisclaimerShown)
                {
                    return;
                }

                string title = Properties.Resources.ResourceManager.GetString("AiDisclaimerTitle") ?? "AI Assistant Disclaimer";
                string disclaimerText = Properties.Resources.ResourceManager.GetString("AiDisclaimerText") ?? string.Empty;
                string settingsHint = Properties.Resources.ResourceManager.GetString("AiDisclaimerPopupSettingsHint") ?? string.Empty;

                if (string.IsNullOrWhiteSpace(disclaimerText))
                {
                    return;
                }

                string popupText = string.IsNullOrWhiteSpace(settingsHint)
                    ? disclaimerText
                    : string.Concat(disclaimerText, Environment.NewLine, Environment.NewLine, settingsHint);

                MessageBox.Show(popupText, title, MessageBoxButton.OK, MessageBoxImage.Information);

                Properties.Settings.Default.aiDisclaimerShown = true;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to show AI disclaimer popup: {ex.Message}");
            }
        }

        private void RefreshModelSelectorFromSettings()
        {
            try
            {
                string providerRaw = Properties.Settings.Default.selectedProvider;
                SupportedProviders provider = ProviderToStringConverter.Convert(providerRaw);

                string rawList;
                string activeModel;
                if (provider == SupportedProviders.Local)
                {
                    rawList = Properties.Settings.Default.localModelIds ?? string.Empty;
                }
                else
                {
                    rawList = Properties.Settings.Default.openAiModelIds ?? string.Empty;
                }
                activeModel = Properties.Settings.Default.activeModelId;

                // Model list format in settings: one model identifier per line.
                var ids = rawList
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                ModelSelector.ItemsSource = ids;

                if (!string.IsNullOrWhiteSpace(activeModel) &&
                    ids.Contains(activeModel, StringComparer.OrdinalIgnoreCase))
                {
                    ModelSelector.SelectedItem = ids.First(id =>
                        string.Equals(id, activeModel, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    ModelSelector.SelectedItem = null;
                }

                RefreshUnavailableToolsInfo();
                RefreshModelSelectionState();
            }
            catch (Exception ex)
            {
                Log.Warning(string.Format("Failed to refresh model selector from settings: {0}", ex.Message));
            }
        }

        private void ModelSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModelSelector.SelectedItem is not string selected ||
                string.IsNullOrWhiteSpace(selected))
            {
                RefreshModelSelectionState();
                return;
            }

            string currentLocalModelId = Properties.Settings.Default.activeModelId;
            if (string.Equals(currentLocalModelId, selected, StringComparison.OrdinalIgnoreCase))
            {
                RefreshModelSelectionState();
                return;
            }

            Properties.Settings.Default.activeModelId = selected;
            Properties.Settings.Default.Save();
            RefreshUnavailableToolsInfo();
            RefreshModelSelectionState();
        }

        private void UnavailableToolsButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_unavailableToolsTooltipText))
            {
                IsUnavailableToolsPopupOpen = false;
                return;
            }

            IsUnavailableToolsPopupOpen = !IsUnavailableToolsPopupOpen;
            if (IsUnavailableToolsPopupOpen)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    UnavailableToolsPopupTextBox?.Focus();
                    UnavailableToolsPopupTextBox?.Select(0, 0);
                }), DispatcherPriority.Input);
            }
        }

        private void UnavailableToolsPopup_Closed(object sender, EventArgs e)
        {
            IsUnavailableToolsPopupOpen = false;
        }

        /// <summary>
        /// Subscribes to AIThreadManager events once and initializes thread selector bindings.
        /// </summary>
        private void SafeSubscribeToThreadManager()
        {
            if (_subscribed)
            {
                return;
            }

            try
            {
                var mgr = AIThreadManager.Instance;
                if (mgr == null)
                {
                    Log.Warning("AIThreadManager instance is null; thread UI will not be initialized.");
                    return;
                }

                mgr.ThreadListChanged += OnThreadListChanged;
                mgr.ActiveThreadChanged += OnActiveThreadChanged;
                mgr.NewChatMessageReceived += OnNewChatMessageReceived;
                mgr.ToolActivityChanged += OnToolActivityChanged;

                _subscribed = true;

                RefreshThreads();
                ThreadSelector.SelectedItem = mgr.ActiveThread;
                RefreshToolActivities();
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to subscribe to AIThreadManager events: {ex.Message}");
            }
        }

        /// <summary>
        /// Unsubscribes from AIThreadManager events safely and idempotently.
        /// </summary>
        private void SafeUnsubscribeFromThreadManager()
        {
            if (!_subscribed)
            {
                return;
            }

            try
            {
                var mgr = AIThreadManager.Instance;
                if (mgr != null)
                {
                    mgr.ThreadListChanged -= OnThreadListChanged;
                    mgr.ActiveThreadChanged -= OnActiveThreadChanged;
                    mgr.NewChatMessageReceived -= OnNewChatMessageReceived;
                    mgr.ToolActivityChanged -= OnToolActivityChanged;
                }

                _subscribed = false;
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to unsubscribe from AIThreadManager: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the observable thread list when the manager signals a change.
        /// Ensures execution on the UI thread.
        /// </summary>
        private void OnThreadListChanged(object sender, EventArgs e)
        {
            if (Dispatcher.CheckAccess())
            {
                RefreshThreads();
                ThreadSelector.SelectedItem = AIThreadManager.Instance.ActiveThread;
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    RefreshThreads();
                    ThreadSelector.SelectedItem = AIThreadManager.Instance.ActiveThread;
                }));
            }
        }

        /// <summary>
        /// Refreshes selection and messages when the active thread changes.
        /// Ensures execution on the UI thread.
        /// </summary>
        private void OnActiveThreadChanged(object sender, EventArgs e)
        {
            if (Dispatcher.CheckAccess())
            {
                RememberCurrentThreadScrollOffset();
                OnPropertyChanged(nameof(ActiveThread));
                RefreshSelectionAndMessages(ChatViewportUpdateMode.RestoreThreadPosition);
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    RememberCurrentThreadScrollOffset();
                    OnPropertyChanged(nameof(ActiveThread));
                    RefreshSelectionAndMessages(ChatViewportUpdateMode.RestoreThreadPosition);
                }));
            }
        }

        private void OnToolActivityChanged(object sender, EventArgs e)
        {
            if (Dispatcher.CheckAccess())
            {
                RefreshToolActivities();
                RefreshMessages(scrollMode: ChatViewportUpdateMode.FollowIfNearBottom);
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    RefreshToolActivities();
                    RefreshMessages(scrollMode: ChatViewportUpdateMode.FollowIfNearBottom);
                }));
            }
        }

        /// <summary>
        /// Renders each completed model turn while the agent is still working.
        /// Keeping this event separate from tool activity also updates pure text
        /// responses and ensures the growing conversation remains visible.
        /// </summary>
        private void OnNewChatMessageReceived(object sender, EventArgs e)
        {
            if (Dispatcher.CheckAccess())
            {
                RefreshMessages(ChatViewportUpdateMode.ForceBottom);
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() =>
                    RefreshMessages(ChatViewportUpdateMode.ForceBottom)));
            }
        }

        /// <summary>
        /// Synchronizes the thread selector's selected item and message list with the manager's active thread.
        /// </summary>
        private void RefreshSelectionAndMessages(ChatViewportUpdateMode scrollMode = ChatViewportUpdateMode.RestoreThreadPosition)
        {
            try
            {
                ThreadSelector.SelectedItem = AIThreadManager.Instance.ActiveThread;
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to set thread selector selection: {ex.Message}");
            }

            RefreshToolActivities();
            RefreshMessages(scrollMode: scrollMode);
        }

        /// <summary>
        /// Mirrors the active thread's chat history into the `Messages` collection for UI binding.
        /// </summary>
        private void RefreshMessages(ChatViewportUpdateMode scrollMode = ChatViewportUpdateMode.FollowIfNearBottom, string newMessage = "")
        {
            OnPropertyChanged(nameof(AgentChanges));
            OnPropertyChanged(nameof(AgentChangesVisibility));
            bool shouldFollowToBottom =
                scrollMode == ChatViewportUpdateMode.ForceBottom ||
                ((scrollMode == ChatViewportUpdateMode.FollowIfNearBottom ||
                  scrollMode == ChatViewportUpdateMode.FollowLatestAssistantStartIfNearBottom) &&
                 IsViewportNearBottom());
            CaptureToolActivityExpansionStatesFromMessages();
            CaptureToolActivityDetailExpansionStatesFromMessages();
            Messages.Clear();
            List<ChatMessage> displayMessages = new List<ChatMessage>();

            var activeThread = AIThreadManager.Instance.ActiveThread;
            string targetThreadId = activeThread?.Id ?? string.Empty;
            string effectivePendingUserMessage = ResolvePendingUserPreviewMessage(activeThread, newMessage);
            Dictionary<int, List<ToolActivityViewModel>> completedToolActivitiesByAssistantOrdinal =
                BuildCompletedToolActivitiesByAssistantOrdinal(activeThread);
            int assistantMessageOrdinal = 0;
            if (activeThread?.ChatHistory != null)
            {
                foreach (var message in activeThread.ChatHistory)
                {
                    // Nur User- und Assistant-Nachrichten als normale Chat-Texte anzeigen
                    if (message.Role != AuthorRole.User && message.Role != AuthorRole.Assistant)
                    {
                        continue;
                    }

                    var text = message.Role == AuthorRole.Assistant
                        ? AssistantResponseText.GetVisibleText(message.Content)
                        : message.Content ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        // Leere Nachrichten komplett überspringen, keine "Leerzeilen" im UI
                        continue;
                    }

                    if (IsInternalLogNoiseMessage(text))
                    {
                        continue;
                    }

                    if (message.Role == AuthorRole.Assistant)
                    {
                        assistantMessageOrdinal++;
                        if (completedToolActivitiesByAssistantOrdinal.TryGetValue(assistantMessageOrdinal, out List<ToolActivityViewModel> completedToolActivities) &&
                            completedToolActivities?.Count > 0)
                        {
                            string completedStateKey = BuildCompletedToolActivityStateKey(activeThread, assistantMessageOrdinal);
                            displayMessages.Add(new ChatMessage
                            {
                                Text = BuildCompletedToolActivityHeaderText(completedToolActivities.Count),
                                Role = AuthorRole.Assistant,
                                ToolActivities = completedToolActivities,
                                IsToolActivitySummary = true,
                                ToolActivityStateKey = completedStateKey,
                                IsToolActivityExpanded = GetRememberedToolActivityExpandedState(
                                    completedStateKey,
                                    GetRememberedToolActivityExpandedState(BuildActiveToolActivityStateKey(activeThread), false))
                            });
                        }
                    }

                    displayMessages.Add(new ChatMessage
                    {
                        Text = text,
                        Role = message.Role,
                        IsError = message.Role == AuthorRole.Assistant && IsAssistantErrorMessage(text)
                    });
                }
                if (!string.IsNullOrWhiteSpace(effectivePendingUserMessage))
                {
                    displayMessages.Add(new ChatMessage
                    {
                        Text = effectivePendingUserMessage,
                        Role = AuthorRole.User,
                        IsError = false
                    });
                }
            }

            List<ToolActivityViewModel> toolActivities = BuildToolActivityViewModelsForActiveThread();
            if ((activeThread?.IsToolActivitySessionActive ?? false) && toolActivities.Count > 0)
            {
                string activeStateKey = BuildActiveToolActivityStateKey(activeThread);
                ChatMessage toolActivityMessage = new ChatMessage
                {
                    Text = BuildRunningToolActivityHeaderText(toolActivities.Count),
                    Role = AuthorRole.Assistant,
                    ToolActivities = toolActivities,
                    IsToolActivitySummary = true,
                    ToolActivityStateKey = activeStateKey,
                    IsToolActivityExpanded = GetRememberedToolActivityExpandedState(activeStateKey, false)
                };

                int lastUserIndex = displayMessages.FindLastIndex(item => item.Role == AuthorRole.User);
                if (lastUserIndex >= 0)
                {
                    int firstAssistantAfterUserIndex = displayMessages.FindIndex(
                        lastUserIndex + 1,
                        item => item.Role == AuthorRole.Assistant);

                    if (firstAssistantAfterUserIndex >= 0)
                    {
                        displayMessages.Insert(firstAssistantAfterUserIndex, toolActivityMessage);
                    }
                    else
                    {
                        displayMessages.Insert(lastUserIndex + 1, toolActivityMessage);
                    }
                }
                else
                {
                    displayMessages.Add(toolActivityMessage);
                }
            }

            foreach (ChatMessage displayMessage in displayMessages)
            {
                Messages.Add(displayMessage);
            }

            OnPropertyChanged(nameof(ChatWelcomeVisibility));
            RefreshContextUsage();

            _renderedThreadId = targetThreadId;
            int latestAssistantIndex = displayMessages.FindLastIndex(item =>
                item.Role == AuthorRole.Assistant &&
                !item.IsToolActivitySummary &&
                !string.IsNullOrWhiteSpace(item.Text));
            ChatMessage latestAssistantMessage = displayMessages.LastOrDefault(item =>
                item.Role == AuthorRole.Assistant &&
                !item.IsToolActivitySummary &&
                !string.IsNullOrWhiteSpace(item.Text));
            ChatMessage latestUserMessage = latestAssistantIndex > 0
                ? displayMessages.Take(latestAssistantIndex).LastOrDefault(item => item.Role == AuthorRole.User)
                : null;
            ApplyViewportUpdate(scrollMode, shouldFollowToBottom, targetThreadId, latestAssistantMessage, latestUserMessage);
        }

        private void CaptureToolActivityExpansionStatesFromMessages()
        {
            foreach (ChatMessage message in Messages)
            {
                if (message == null ||
                    !message.IsToolActivitySummary ||
                    string.IsNullOrWhiteSpace(message.ToolActivityStateKey))
                {
                    continue;
                }

                _toolActivityExpansionStates[message.ToolActivityStateKey] = message.IsToolActivityExpanded;
            }
        }

        private void CaptureToolActivityDetailExpansionStatesFromMessages()
        {
            foreach (ChatMessage message in Messages)
            {
                if (message?.ToolActivities == null || message.ToolActivities.Count == 0)
                {
                    continue;
                }

                CaptureToolActivityDetailExpansionStates(message.ToolActivities);
            }
        }

        private void CaptureToolActivityDetailExpansionStatesFromVisibleToolActivities()
        {
            if (VisibleToolActivities.Count == 0)
            {
                return;
            }

            CaptureToolActivityDetailExpansionStates(VisibleToolActivities);
        }

        private void CaptureToolActivityDetailExpansionStates(IEnumerable<ToolActivityViewModel> toolActivities)
        {
            if (toolActivities == null)
            {
                return;
            }

            foreach (ToolActivityViewModel toolActivity in toolActivities)
            {
                if (toolActivity == null || string.IsNullOrWhiteSpace(toolActivity.DetailStateKey))
                {
                    continue;
                }

                _toolActivityDetailExpansionStates[toolActivity.DetailStateKey] = toolActivity.IsExpanded;
            }
        }

        private bool GetRememberedToolActivityExpandedState(string stateKey, bool fallback)
        {
            if (string.IsNullOrWhiteSpace(stateKey))
            {
                return fallback;
            }

            return _toolActivityExpansionStates.TryGetValue(stateKey, out bool isExpanded)
                ? isExpanded
                : fallback;
        }

        private bool GetRememberedToolActivityDetailExpandedState(string stateKey, bool fallback)
        {
            if (string.IsNullOrWhiteSpace(stateKey))
            {
                return fallback;
            }

            return _toolActivityDetailExpansionStates.TryGetValue(stateKey, out bool isExpanded)
                ? isExpanded
                : fallback;
        }

        private static string BuildActiveToolActivityStateKey(AIThread activeThread)
        {
            return $"thread:{activeThread?.Id ?? "unknown"}:tool-activity:active";
        }

        private static string BuildCompletedToolActivityStateKey(AIThread activeThread, int assistantMessageOrdinal)
        {
            return $"thread:{activeThread?.Id ?? "unknown"}:tool-activity:assistant:{assistantMessageOrdinal}";
        }

        private static string BuildToolActivityDetailStateKey(string detailStateKeyPrefix, ToolActivityEntry entry)
        {
            if (string.IsNullOrWhiteSpace(detailStateKeyPrefix) || entry == null)
            {
                return string.Empty;
            }

            string callId = (entry.CallId ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(callId))
            {
                return $"{detailStateKeyPrefix}:detail:call:{callId}";
            }

            string functionName = (entry.FunctionName ?? string.Empty).Trim();
            string fallbackFunctionName = string.IsNullOrWhiteSpace(functionName) ? "tool" : functionName;
            return $"{detailStateKeyPrefix}:detail:seq:{entry.Sequence}:fn:{fallbackFunctionName}";
        }

        private void RefreshToolActivities()
        {
            CaptureToolActivityDetailExpansionStatesFromVisibleToolActivities();
            VisibleToolActivities.Clear();

            foreach (ToolActivityViewModel entry in BuildToolActivityViewModelsForActiveThread())
            {
                VisibleToolActivities.Add(entry);
            }

            OnPropertyChanged(nameof(ToolActivityPanelVisibility));
            OnPropertyChanged(nameof(ToolActivityEmptyStateVisibility));
            OnPropertyChanged(nameof(ToolActivityHeaderText));
            OnPropertyChanged(nameof(ToolActivityEmptyStateText));
        }

        private void ApplyViewportUpdate(
            ChatViewportUpdateMode scrollMode,
            bool shouldFollowToBottom,
            string threadId,
            ChatMessage latestAssistantMessage = null,
            ChatMessage latestUserMessage = null)
        {
            if (ChatScrollViewer == null)
            {
                return;
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (ChatScrollViewer == null)
                {
                    return;
                }

                switch (scrollMode)
                {
                    case ChatViewportUpdateMode.ForceBottom:
                        ScrollToBottom();
                        break;
                    case ChatViewportUpdateMode.FollowIfNearBottom:
                        if (shouldFollowToBottom)
                        {
                            ScrollToBottom();
                        }
                        break;
                    case ChatViewportUpdateMode.FollowLatestAssistantStartIfNearBottom:
                        if (shouldFollowToBottom)
                        {
                            ScrollLatestTurnToTop(latestUserMessage, latestAssistantMessage);
                        }
                        break;
                    case ChatViewportUpdateMode.RestoreThreadPosition:
                        RestoreThreadScrollOffset(threadId);
                        break;
                    default:
                        break;
                }
            }), DispatcherPriority.Background);
        }

        private bool IsViewportNearBottom()
        {
            if (ChatScrollViewer == null)
            {
                return true;
            }

            double remainingDistance = ChatScrollViewer.ScrollableHeight - ChatScrollViewer.VerticalOffset;
            return remainingDistance <= AutoScrollBottomThreshold;
        }

        private void RememberCurrentThreadScrollOffset()
        {
            RememberThreadScrollOffset(_renderedThreadId, ChatScrollViewer?.VerticalOffset ?? 0d);
        }

        private void RememberThreadScrollOffset(string threadId, double verticalOffset)
        {
            if (string.IsNullOrWhiteSpace(threadId))
            {
                return;
            }

            _threadScrollOffsets[threadId] = Math.Max(0d, verticalOffset);
        }

        private void RestoreThreadScrollOffset(string threadId)
        {
            if (ChatScrollViewer == null)
            {
                return;
            }

            if (!_threadScrollOffsets.TryGetValue(threadId ?? string.Empty, out double storedOffset))
            {
                ScrollToBottom();
                return;
            }

            double clampedOffset = Math.Max(0d, Math.Min(ChatScrollViewer.ScrollableHeight, storedOffset));
            ScrollToVerticalOffset(clampedOffset);
        }

        private void ScrollToBottom()
        {
            if (ChatScrollViewer == null)
            {
                return;
            }

            try
            {
                _suppressScrollTracking = true;
                ChatScrollViewer.ScrollToBottom();
                RememberCurrentThreadScrollOffset();
            }
            finally
            {
                _suppressScrollTracking = false;
            }
        }

        private void ScrollToVerticalOffset(double offset)
        {
            if (ChatScrollViewer == null)
            {
                return;
            }

            try
            {
                _suppressScrollTracking = true;
                ChatScrollViewer.ScrollToVerticalOffset(offset);
                RememberCurrentThreadScrollOffset();
            }
            finally
            {
                _suppressScrollTracking = false;
            }
        }

        private void ScrollLatestTurnToTop(ChatMessage userMessage, ChatMessage assistantMessage)
        {
            if (ChatScrollViewer == null || ChatItemsControl == null || assistantMessage == null)
            {
                ScrollToBottom();
                return;
            }

            ChatScrollViewer.UpdateLayout();
            ChatItemsControl.UpdateLayout();

            if (!TryGetMessageContainer(assistantMessage, out FrameworkElement assistantContainer))
            {
                ScrollToBottom();
                return;
            }

            double assistantTopOffset = GetMessageTopOffset(assistantContainer);
            double targetOffset = assistantTopOffset;

            if (TryGetMessageContainer(userMessage, out FrameworkElement userContainer))
            {
                double userTopOffset = GetMessageTopOffset(userContainer);
                double userHeight = userContainer.ActualHeight;
                double viewportHeight = Math.Max(1d, ChatScrollViewer.ViewportHeight);
                double desiredContextBeforeAnswer = Math.Max(140d, viewportHeight * 0.30d);
                double minVisibleQuestionHeight = Math.Min(userHeight, Math.Max(96d, viewportHeight * 0.22d));
                double latestOffsetThatStillShowsQuestion =
                    userTopOffset + Math.Max(0d, userHeight - minVisibleQuestionHeight);

                targetOffset = assistantTopOffset - desiredContextBeforeAnswer;
                targetOffset = Math.Max(0d, Math.Min(targetOffset, latestOffsetThatStillShowsQuestion));
            }

            ScrollToVerticalOffset(targetOffset);
        }

        private bool TryGetMessageContainer(ChatMessage message, out FrameworkElement container)
        {
            container = null;
            if (ChatItemsControl == null || message == null)
            {
                return false;
            }

            container = ChatItemsControl.ItemContainerGenerator.ContainerFromItem(message) as FrameworkElement;
            return container != null;
        }

        private double GetMessageTopOffset(FrameworkElement container)
        {
            GeneralTransform transform = container.TransformToAncestor(ChatScrollViewer);
            Point positionInViewport = transform.Transform(new Point(0, 0));
            return ChatScrollViewer.VerticalOffset + positionInViewport.Y;
        }

        private List<ToolActivityViewModel> BuildToolActivityViewModelsForActiveThread()
        {
            AIThread activeThread = AIThreadManager.Instance.ActiveThread;
            List<ToolActivityEntry> snapshot = activeThread?.GetVisibleToolActivitySnapshot() ?? new List<ToolActivityEntry>();
            if (!(activeThread?.IsToolActivitySessionActive ?? false))
            {
                return new List<ToolActivityViewModel>();
            }

            return BuildToolActivityViewModels(
                snapshot,
                activeThread?.LastInvocationDebugInfo?.ToolCalls,
                BuildActiveToolActivityStateKey(activeThread));
        }

        private Dictionary<int, List<ToolActivityViewModel>> BuildCompletedToolActivitiesByAssistantOrdinal(AIThread activeThread)
        {
            Dictionary<int, List<ToolActivityViewModel>> result = new Dictionary<int, List<ToolActivityViewModel>>();
            List<ToolActivityMessageAttachment> attachments = activeThread?.GetToolActivityMessageAttachmentsSnapshot() ?? new List<ToolActivityMessageAttachment>();
            foreach (ToolActivityMessageAttachment attachment in attachments)
            {
                if (attachment == null || attachment.AssistantMessageOrdinal <= 0)
                {
                    continue;
                }

                string completedStateKey = BuildCompletedToolActivityStateKey(activeThread, attachment.AssistantMessageOrdinal);
                List<ToolActivityViewModel> viewModels = BuildToolActivityViewModels(
                    attachment.Entries ?? new List<ToolActivityEntry>(),
                    activeThread?.LastInvocationDebugInfo?.ToolCalls,
                    completedStateKey);
                if (viewModels.Count == 0)
                {
                    continue;
                }

                result[attachment.AssistantMessageOrdinal] = viewModels;
            }

            return result;
        }

        private List<ToolActivityViewModel> BuildToolActivityViewModels(
            IEnumerable<ToolActivityEntry> snapshot,
            IReadOnlyList<DebugToolCall> debugToolCalls = null,
            string detailStateKeyPrefix = null)
        {
            return snapshot
                .OrderBy(item => item.Sequence)
                .Select(item =>
                {
                    string detailStateKey = BuildToolActivityDetailStateKey(detailStateKeyPrefix, item);
                    return CreateToolActivityViewModel(
                        item,
                        debugToolCalls,
                        detailStateKey,
                        GetRememberedToolActivityDetailExpandedState(detailStateKey, false));
                })
                .ToList();
        }

        private string ResolvePendingUserPreviewMessage(AIThread activeThread, string explicitMessage)
        {
            string candidate = !string.IsNullOrWhiteSpace(explicitMessage)
                ? explicitMessage
                : _pendingUserPreviewMessage;

            if (string.IsNullOrWhiteSpace(candidate))
            {
                return string.Empty;
            }

            string normalized = candidate.Trim();
            return HasLatestVisibleUserMessage(activeThread, normalized) ? string.Empty : normalized;
        }

        private static bool HasLatestVisibleUserMessage(AIThread activeThread, string expectedText)
        {
            if (activeThread?.ChatHistory == null || string.IsNullOrWhiteSpace(expectedText))
            {
                return false;
            }

            for (int i = activeThread.ChatHistory.Count - 1; i >= 0; i--)
            {
                ChatMessageContent message = activeThread.ChatHistory[i];
                if (message == null || message.Role != AuthorRole.User)
                {
                    continue;
                }

                string text = (message.Content ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                return string.Equals(text, expectedText.Trim(), StringComparison.Ordinal);
            }

            return false;
        }

        private static void EnsureUserMessagePresentInHistory(AIThread activeThread, string userInput)
        {
            string normalized = (userInput ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalized) || activeThread == null)
            {
                return;
            }

            if (HasLatestVisibleUserMessage(activeThread, normalized))
            {
                return;
            }

            activeThread.ChatHistory.Add(new ChatMessageContent(AuthorRole.User, normalized));
        }

        private static ToolActivityViewModel CreateToolActivityViewModel(
            ToolActivityEntry entry,
            IReadOnlyList<DebugToolCall> debugToolCalls,
            string detailStateKey,
            bool isExpanded)
        {
            string statusLabel;
            Brush statusBackground;
            Brush statusForeground;
            switch (entry?.Status ?? ToolActivityStatus.Incomplete)
            {
                case ToolActivityStatus.Running:
                    statusLabel = GetResourceTextOrFallback("AiChatToolActivityStatusRunning", "Running");
                    statusBackground = CreateSolidBrush("#FFFDF0C2");
                    statusForeground = CreateSolidBrush("#FF7A5A00");
                    break;
                case ToolActivityStatus.Succeeded:
                    statusLabel = GetResourceTextOrFallback("AiChatToolActivityStatusSucceeded", "Succeeded");
                    statusBackground = CreateSolidBrush("#FFDDF3E2");
                    statusForeground = CreateSolidBrush("#FF1E6A2D");
                    break;
                case ToolActivityStatus.Denied:
                    statusLabel = GetResourceTextOrFallback("AiChatToolActivityStatusDenied", "Denied");
                    statusBackground = CreateSolidBrush("#FFE7E7E7");
                    statusForeground = CreateSolidBrush("#FF4E4E4E");
                    break;
                case ToolActivityStatus.Cancelled:
                    statusLabel = GetResourceTextOrFallback("AiChatToolActivityStatusCancelled", "Cancelled");
                    statusBackground = CreateSolidBrush("#FFE9EDF2");
                    statusForeground = CreateSolidBrush("#FF506273");
                    break;
                case ToolActivityStatus.Failed:
                    statusLabel = GetResourceTextOrFallback("AiChatToolActivityStatusFailed", "Failed");
                    statusBackground = CreateSolidBrush("#FFF9D6D6");
                    statusForeground = CreateSolidBrush("#FF8B1E1E");
                    break;
                default:
                    statusLabel = GetResourceTextOrFallback("AiChatToolActivityStatusIncomplete", "Incomplete");
                    statusBackground = CreateSolidBrush("#FFF4E6D8");
                    statusForeground = CreateSolidBrush("#FF7A4E1D");
                    break;
            }

            string functionName = entry?.FunctionName ?? string.Empty;
            string arguments = entry?.Arguments ?? string.Empty;
            if (string.IsNullOrWhiteSpace(arguments))
            {
                arguments = ResolveToolArgumentsFromDebugCalls(entry, debugToolCalls);
            }

            string errorDetails = entry?.Status == ToolActivityStatus.Failed
                ? (entry?.ResultText ?? string.Empty)
                : string.Empty;
            string statusTooltip = entry?.Status == ToolActivityStatus.Denied || entry?.Status == ToolActivityStatus.Failed
                ? (entry?.ResultText ?? string.Empty)
                : string.Empty;

            return new ToolActivityViewModel
            {
                Sequence = entry?.Sequence ?? 0,
                ToolDisplayName = functionName,
                FunctionName = functionName,
                DetailStateKey = detailStateKey,
                IsExpanded = isExpanded,
                ToolDescription = GetToolDescription(functionName),
                Arguments = arguments,
                ErrorDetails = errorDetails,
                StatusTooltip = statusTooltip,
                StatusLabel = statusLabel,
                StatusBackground = statusBackground,
                StatusForeground = statusForeground
            };
        }

        private static string ResolveToolArgumentsFromDebugCalls(
            ToolActivityEntry entry,
            IReadOnlyList<DebugToolCall> debugToolCalls)
        {
            if (entry == null || debugToolCalls == null || debugToolCalls.Count == 0)
            {
                return string.Empty;
            }

            string normalizedCallId = (entry.CallId ?? string.Empty).Trim();
            DebugToolCall match = null;
            if (!string.IsNullOrWhiteSpace(normalizedCallId))
            {
                match = debugToolCalls.FirstOrDefault(call =>
                    string.Equals((call?.CallId ?? string.Empty).Trim(), normalizedCallId, StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(call?.Arguments));
            }

            if (match == null)
            {
                match = debugToolCalls.FirstOrDefault(call =>
                    string.Equals(call?.FunctionName ?? string.Empty, entry.FunctionName ?? string.Empty, StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(call?.Arguments));
            }

            return match?.Arguments ?? string.Empty;
        }

        internal static IReadOnlyList<ToolArgumentItemViewModel> BuildArgumentItems(string rawArguments)
        {
            if (string.IsNullOrWhiteSpace(rawArguments))
            {
                return Array.Empty<ToolArgumentItemViewModel>();
            }

            string trimmed = rawArguments.Trim();
            if (IsEffectivelyEmptyArguments(trimmed))
            {
                return Array.Empty<ToolArgumentItemViewModel>();
            }

            if (TryBuildJsonArgumentItems(trimmed, out IReadOnlyList<ToolArgumentItemViewModel> jsonItems))
            {
                return jsonItems;
            }

            if (TryBuildKeyValueArgumentItems(trimmed, out IReadOnlyList<ToolArgumentItemViewModel> keyValueItems))
            {
                return keyValueItems;
            }

            return new[]
            {
                new ToolArgumentItemViewModel
                {
                    Value = NormalizeArgumentValueForDisplay(trimmed)
                }
            };
        }

        internal static string BuildArgumentsDisplayText(string rawArguments)
        {
            IReadOnlyList<ToolArgumentItemViewModel> items = BuildArgumentItems(rawArguments);
            if (items.Count == 0)
            {
                return string.Empty;
            }

            List<string> lines = new List<string>(items.Count);
            foreach (ToolArgumentItemViewModel item in items)
            {
                if (!string.IsNullOrWhiteSpace(item.Name) && item.IsGroupHeader)
                {
                    lines.Add($"{item.Name}:");
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(item.Name))
                {
                    lines.Add($"{item.Name}: {item.Value}");
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(item.Value))
                {
                    lines.Add(item.Value);
                }
            }

            return string.Join(Environment.NewLine, lines);
        }

        private static bool IsEffectivelyEmptyArguments(string rawArguments)
        {
            if (string.IsNullOrWhiteSpace(rawArguments))
            {
                return true;
            }

            string trimmed = rawArguments.Trim();
            if (string.Equals(trimmed, "{}", StringComparison.Ordinal) ||
                string.Equals(trimmed, "[]", StringComparison.Ordinal) ||
                string.Equals(trimmed, "null", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            try
            {
                JToken token = JToken.Parse(trimmed);
                if (token is JObject obj)
                {
                    return !obj.Properties().Any();
                }

                if (token is JArray array)
                {
                    return !array.Any();
                }
            }
            catch
            {
                // Ignore parse failures here; non-JSON strings may still contain usable arguments.
            }

            return false;
        }

        private static bool TryBuildJsonArgumentItems(string rawArguments, out IReadOnlyList<ToolArgumentItemViewModel> items)
        {
            items = null;
            try
            {
                JToken token = JToken.Parse(rawArguments);
                if (token is not JObject obj)
                {
                    return false;
                }

                List<ToolArgumentItemViewModel> parsedItems = new List<ToolArgumentItemViewModel>();
                foreach (JProperty property in obj.Properties())
                {
                    AppendArgumentItems(parsedItems, property.Name, property.Value, 0);
                }

                if (parsedItems.Count == 0)
                {
                    return false;
                }

                items = parsedItems;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryBuildKeyValueArgumentItems(string rawArguments, out IReadOnlyList<ToolArgumentItemViewModel> items)
        {
            items = null;
            List<string> segments = SplitTopLevelArguments(rawArguments);
            if (segments.Count == 0 || !segments.Any(segment => segment.Contains("=")))
            {
                return false;
            }

            List<ToolArgumentItemViewModel> parsedItems = new List<ToolArgumentItemViewModel>();
            foreach (string segment in segments)
            {
                int separatorIndex = segment.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                string key = segment.Substring(0, separatorIndex).Trim();
                string value = segment.Substring(separatorIndex + 1).Trim();
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                parsedItems.Add(new ToolArgumentItemViewModel
                {
                    Name = key,
                    Value = NormalizeArgumentValueForDisplay(value)
                });
            }

            if (parsedItems.Count == 0)
            {
                return false;
            }

            items = parsedItems;
            return true;
        }

        private static List<string> SplitTopLevelArguments(string rawArguments)
        {
            List<string> result = new List<string>();
            if (string.IsNullOrWhiteSpace(rawArguments))
            {
                return result;
            }

            StringBuilder current = new StringBuilder();
            bool inQuotes = false;
            int nestingDepth = 0;
            for (int i = 0; i < rawArguments.Length; i++)
            {
                char c = rawArguments[i];
                if (c == '"' && (i == 0 || rawArguments[i - 1] != '\\'))
                {
                    inQuotes = !inQuotes;
                }
                else if (!inQuotes)
                {
                    if (c == '{' || c == '[' || c == '(')
                    {
                        nestingDepth++;
                    }
                    else if ((c == '}' || c == ']' || c == ')') && nestingDepth > 0)
                    {
                        nestingDepth--;
                    }
                    else if (c == ',' && nestingDepth == 0)
                    {
                        string segment = current.ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(segment))
                        {
                            result.Add(segment);
                        }

                        current.Clear();
                        continue;
                    }
                }

                current.Append(c);
            }

            string lastSegment = current.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(lastSegment))
            {
                result.Add(lastSegment);
            }

            return result;
        }

        private static void AppendArgumentItems(List<ToolArgumentItemViewModel> items, string key, JToken value, int indentLevel)
        {
            string indentPrefix = indentLevel > 0 ? new string(' ', indentLevel * 2) : string.Empty;
            string displayName = string.IsNullOrWhiteSpace(indentPrefix) ? key : indentPrefix + key;
            if (value == null || value.Type == JTokenType.Null)
            {
                items.Add(new ToolArgumentItemViewModel
                {
                    Name = displayName,
                    Value = "null"
                });
                return;
            }

            if (value is JObject obj)
            {
                items.Add(new ToolArgumentItemViewModel
                {
                    Name = displayName,
                    Value = string.Empty,
                    IsGroupHeader = true
                });

                foreach (JProperty property in obj.Properties())
                {
                    AppendArgumentItems(items, property.Name, property.Value, indentLevel + 1);
                }

                return;
            }

            if (value is JArray array)
            {
                string arrayText = string.Join(", ", array.Select(item => NormalizeArgumentValueForDisplay(item.ToString(Formatting.None))));
                items.Add(new ToolArgumentItemViewModel
                {
                    Name = displayName,
                    Value = arrayText
                });
                return;
            }

            items.Add(new ToolArgumentItemViewModel
            {
                Name = displayName,
                Value = NormalizeArgumentValueForDisplay(value.ToString(Formatting.None))
            });
        }

        private static string NormalizeArgumentValueForDisplay(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string normalized = value.Trim();
            if (normalized.Length >= 2 && normalized.StartsWith("\"", StringComparison.Ordinal) && normalized.EndsWith("\"", StringComparison.Ordinal))
            {
                normalized = normalized.Substring(1, normalized.Length - 2);
            }

            normalized = normalized.Replace("\\\"", "\"").Replace("\\\\", "\\");
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();
            return normalized;
        }

        private static string GetToolDescription(string functionName)
        {
            if (string.IsNullOrWhiteSpace(functionName))
            {
                return string.Empty;
            }

            string normalizedFunctionName = functionName.Trim();
            string resourceKey = "ToolDescription_" + normalizedFunctionName;
            string localizedDescription = ToolDescriptionsResourceManager.GetString(resourceKey);
            if (!string.IsNullOrWhiteSpace(localizedDescription))
            {
                return localizedDescription.Trim();
            }

            foreach (Type pluginType in KnownToolPluginTypes)
            {
                MethodInfo method = pluginType
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(candidate =>
                        string.Equals(candidate.Name, normalizedFunctionName, StringComparison.OrdinalIgnoreCase));
                string attributeDescription = method?.GetCustomAttribute<DescriptionAttribute>()?.Description;
                if (!string.IsNullOrWhiteSpace(attributeDescription))
                {
                    return attributeDescription.Trim();
                }
            }

            return string.Empty;
        }

        private static SolidColorBrush CreateSolidBrush(string colorCode)
        {
            var converter = new BrushConverter();
            return (SolidColorBrush)converter.ConvertFromInvariantString(colorCode);
        }

        private static bool IsInternalLogNoiseMessage(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            string trimmed = text.TrimStart();
            if (trimmed.StartsWith("[SK][", StringComparison.Ordinal))
            {
                return true;
            }

            if (trimmed.StartsWith("LLM-get_open_editors:", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static bool IsAssistantErrorMessage(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            string trimmed = text.TrimStart();
            if (trimmed.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (trimmed.StartsWith("Fehler", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (trimmed.StartsWith("Provider:", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("Endpoint:", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Copies manager threads into the observable collection.
        /// </summary>
        private void RefreshThreads()
        {
            ObservableThreads.Clear();
            var mgrThreads = AIThreadManager.Instance?.AIThreads;
            if (mgrThreads == null)
            {
                return;
            }

            foreach (var t in mgrThreads)
            {
                ObservableThreads.Add(t);
            }
        }

        /// <summary>
        /// Safe property change notification; logs instead of swallowing exceptions.
        /// </summary>
        protected void OnPropertyChanged(string propertyName)
        {
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            catch (Exception ex)
            {
                Log.Warning($"PropertyChanged invocation failed for '{propertyName}': {ex.Message}");
            }
        }

        private void ThreadSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThreadSelector.SelectedItem is not AIThread selected)
            {
                return;
            }

            ActiveThread = selected;
        }

        /// <summary>
        /// UI click handler that delegates to request start/stop lifecycle logic.
        /// </summary>
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            StartOrStopRequest("send_button");
        }

        // Press Enter to send, Shift+Enter for newline.
        private void InputBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    return;
                }

                e.Handled = true;
                StartOrStopRequest("input_enter");
            }
        }

        /// <summary>
        /// Toggles the request lifecycle:
        /// - If a request is running, cancels it.
        /// - Otherwise starts a new async request to handle user input.
        /// Updates the send/stop icon accordingly.
        /// </summary>
        private async void StartOrStopRequest(string trigger)
        {
            if (_isRequestRunning)
            {
                _activeRequestCancelTrigger = string.IsNullOrWhiteSpace(trigger) ? "unknown" : trigger;
                Log.Info($"Cancelling active AI request. trigger={_activeRequestCancelTrigger}");
                _requestCts?.Cancel();
                return;
            }

            // Ensure model list and selection are synchronized before starting a new request.
            RefreshModelSelectorFromSettings();
            if (!TryGetSelectedModelId(out _))
            {
                RefreshModelSelectionState();
                return;
            }

            _requestCts = new CancellationTokenSource();
            _activeRequestCancelTrigger = "none";
            _isRequestRunning = true;
            RefreshInputAvailability();
            AIThreadManager.Instance.ActiveThread?.ClearVisibleToolActivitySnapshot();
            RefreshToolActivities();

            try
            {
                await HandleUserInputAsync(_requestCts.Token);
            }
            catch (OperationCanceledException) when (_requestCts?.IsCancellationRequested == true)
            {
                Log.Info($"AI request cancelled by client interaction. trigger={_activeRequestCancelTrigger}");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            finally
            {
                _isRequestRunning = false;
                RefreshInputAvailability();
                RefreshToolActivities();
                _requestCts?.Dispose();
                _requestCts = null;
            }
        }

        /// <summary>
        /// Sets the send button icon to either send or stop.
        /// </summary>
        private void SetSendImage(bool isStop)
        {
            try
            {
                var uri = isStop ? "images/Stop.png" : "images/Send.png";
                SendImage.Source = new BitmapImage(new Uri(uri, UriKind.Relative));
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to set send image: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the full lifecycle of sending user input to the AI agent and showing the reply.
        /// - Validates input
        /// - Disables input and shows thinking hint
        /// - Calls AIThreadManager to get the reply (honors cancellation)
        /// - Restores input state and refreshes messages from authoritative history
        /// </summary>
        private async Task HandleUserInputAsync(CancellationToken cancellationToken)
        {
            if (!TryGetSelectedModelId(out string selectedModel))
            {
                return;
            }

            var userInput = InputBox.Text;
            if (string.IsNullOrWhiteSpace(userInput))
            {
                return;
            }
            AIThread requestThread = AIThreadManager.Instance.ActiveThread ?? AIThreadManager.Instance.CreateNewThread();
            _pendingUserPreviewMessage = userInput.Trim();
            RefreshMessages(ChatViewportUpdateMode.ForceBottom, userInput);

            SetThinkingState(true);
            RefreshToolActivities();

            try
            {
                // Section: delegate authoritative request execution to the thread manager.
                await AIThreadManager.Instance.CreateAsync(selectedModel, userInput, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (IsUiTextCatalogInitializationFailure(ex))
                {
                    ShowFatalUiTextCatalogInitializationErrorAndTerminate(ex);
                }

                EnsureUserMessagePresentInHistory(requestThread, userInput);
                requestThread.ChatHistory.Add(new ChatMessageContent
                {
                    Role = AuthorRole.Assistant,
                    Content = BuildChatErrorMessage(selectedModel, ex)
                });
                AIThreadManager.Instance.AttachLastCompletedToolActivityToLatestAssistantMessage(requestThread);

                Log.Error(BuildUiErrorLogMessage(selectedModel, ex));
                Log.Error($"AI request exception details: {ex}");
                return;
            }
            finally
            {
                SetThinkingState(false);
                _pendingUserPreviewMessage = string.Empty;
                RefreshMessages(scrollMode: ChatViewportUpdateMode.FollowLatestAssistantStartIfNearBottom);
                RefreshToolActivities();
            }
        }

        private void RefreshUnavailableToolsInfo()
        {
            RefreshContextUsage();
            try
            {
                string selectedModel = ModelSelector?.SelectedItem as string;
                if (string.IsNullOrWhiteSpace(selectedModel))
                {
                    selectedModel = Properties.Settings.Default.activeModelId;
                }

                List<UnavailableToolInfo> unavailableTools = BuildUnavailableToolInfos(selectedModel);
                if (unavailableTools.Count == 0)
                {
                    _unavailableToolsTooltipText = string.Empty;
                    IsUnavailableToolsPopupOpen = false;
                }
                else
                {
                    _unavailableToolsTooltipText = BuildUnavailableToolsTooltip(selectedModel, unavailableTools);
                }

                OnPropertyChanged(nameof(UnavailableToolsTooltipText));
                OnPropertyChanged(nameof(UnavailableToolsIndicatorVisibility));
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to refresh unavailable tools info: {ex.Message}");
                _unavailableToolsTooltipText = string.Empty;
                IsUnavailableToolsPopupOpen = false;
                OnPropertyChanged(nameof(UnavailableToolsTooltipText));
                OnPropertyChanged(nameof(UnavailableToolsIndicatorVisibility));
            }
        }

        private static List<UnavailableToolInfo> BuildUnavailableToolInfos(string modelId)
        {
            Dictionary<string, bool> allowedFunctions = ParseAllowedFunctions(Properties.Settings.Default.editorStatusAllowedFunctions);
            bool allowMutations = Properties.Settings.Default.allowAiToolMutations;
            bool hasResolvedContextWindow = ModelContextWindowResolver.TryResolveContextWindowTokens(modelId, out int contextWindowTokens);

            var unavailableTools = new List<UnavailableToolInfo>();

            foreach (Type pluginType in KnownToolPluginTypes)
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

                    string functionName = kernelFunction.Name.Trim();
                    var settingsReasons = new List<string>();
                    AiToolMutationAttribute mutationAttribute = method.GetCustomAttribute<AiToolMutationAttribute>();
                    if (!allowMutations && mutationAttribute?.CanModifyProgram == true)
                    {
                        settingsReasons.Add(GetResourceText("AiChatUnavailableToolsReasonMutationsDisabled"));
                    }

                    bool disabledBySettings =
                        allowedFunctions.TryGetValue(functionName, out bool explicitlyAllowed) &&
                        !explicitlyAllowed;
                    if (disabledBySettings)
                    {
                        settingsReasons.Add(GetResourceText("AiChatUnavailableToolsReasonManuallyDisabled"));
                    }

                    ContextWindowTokenAttribute contextWindowAttribute = method.GetCustomAttribute<ContextWindowTokenAttribute>();
                    int? missingMinTokens = null;
                    if (contextWindowAttribute != null &&
                        hasResolvedContextWindow &&
                        contextWindowTokens > 0 &&
                        contextWindowTokens < contextWindowAttribute.MinTokens)
                    {
                        missingMinTokens = contextWindowAttribute.MinTokens;
                    }

                    if (!missingMinTokens.HasValue && settingsReasons.Count == 0)
                    {
                        continue;
                    }

                    unavailableTools.Add(new UnavailableToolInfo
                    {
                        FunctionName = functionName,
                        RequiredMinContextTokens = missingMinTokens,
                        SettingsReasons = settingsReasons
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList()
                    });
                }
            }

            return unavailableTools
                .GroupBy(tool => tool.FunctionName, StringComparer.OrdinalIgnoreCase)
                .Select(group => new UnavailableToolInfo
                {
                    FunctionName = group.Key,
                    RequiredMinContextTokens = group.Max(item => item.RequiredMinContextTokens),
                    SettingsReasons = group
                        .SelectMany(item => item.SettingsReasons ?? Enumerable.Empty<string>())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList()
                })
                .OrderBy(tool => tool.FunctionName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string BuildUnavailableToolsTooltip(string modelId, List<UnavailableToolInfo> unavailableTools)
        {
            var lines = new List<string>
            {
                GetResourceText("AiChatUnavailableToolsTooltipTitle")
            };

            if (!string.IsNullOrWhiteSpace(modelId))
            {
                lines.Add(string.Format(
                    CultureInfo.CurrentCulture,
                    GetResourceText("AiChatUnavailableToolsTooltipModel"),
                    modelId.Trim()));
            }

            if (ModelContextWindowResolver.TryResolveContextWindowTokens(modelId, out int contextWindowTokens) &&
                contextWindowTokens > 0)
            {
                lines.Add(string.Format(
                    CultureInfo.CurrentCulture,
                    GetResourceText("AiChatUnavailableToolsTooltipContextWindow"),
                    FormatTokenCount(contextWindowTokens)));
            }

            List<UnavailableToolInfo> missingContextTools = (unavailableTools ?? new List<UnavailableToolInfo>())
                .Where(tool => tool.RequiredMinContextTokens.HasValue && tool.RequiredMinContextTokens.Value > 0)
                .OrderBy(tool => tool.RequiredMinContextTokens.Value)
                .ThenBy(tool => tool.FunctionName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            List<UnavailableToolInfo> settingsDisabledTools = (unavailableTools ?? new List<UnavailableToolInfo>())
                .Where(tool => tool.SettingsReasons != null && tool.SettingsReasons.Count > 0)
                .OrderBy(tool => tool.FunctionName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (missingContextTools.Count > 0)
            {
                lines.Add(string.Empty);
                lines.Add(GetResourceText("AiChatUnavailableToolsTooltipContextSection"));
                foreach (UnavailableToolInfo tool in missingContextTools)
                {
                    lines.Add(string.Format(
                        CultureInfo.CurrentCulture,
                        GetResourceText("AiChatUnavailableToolsTooltipContextEntry"),
                        tool.FunctionName,
                        FormatTokenCount(tool.RequiredMinContextTokens.Value)));
                }
            }

            if (settingsDisabledTools.Count > 0)
            {
                lines.Add(string.Empty);
                lines.Add(GetResourceText("AiChatUnavailableToolsTooltipSettingsSection"));
                foreach (UnavailableToolInfo tool in settingsDisabledTools)
                {
                    string settingsReasonText = string.Join(", ", tool.SettingsReasons ?? Enumerable.Empty<string>());
                    if (string.IsNullOrWhiteSpace(settingsReasonText))
                    {
                        lines.Add(string.Format(
                            CultureInfo.CurrentCulture,
                            GetResourceText("AiChatUnavailableToolsTooltipSettingsEntryNoReason"),
                            tool.FunctionName));
                    }
                    else
                    {
                        lines.Add(string.Format(
                            CultureInfo.CurrentCulture,
                            GetResourceText("AiChatUnavailableToolsTooltipSettingsEntry"),
                            tool.FunctionName,
                            settingsReasonText));
                    }
                }
            }

            return string.Join(Environment.NewLine, lines.Where(line => line != null));
        }

        private static string FormatTokenCount(int tokenCount)
        {
            return tokenCount.ToString("N0", CultureInfo.CurrentCulture);
        }

        private bool TryGetSelectedModelId(out string selectedModel)
        {
            selectedModel = ModelSelector?.SelectedItem as string;
            if (IsSelectableModelId(selectedModel))
            {
                return true;
            }

            string persistedModelId = Properties.Settings.Default.activeModelId;
            if (IsSelectableModelId(persistedModelId))
            {
                selectedModel = persistedModelId;
                return true;
            }

            selectedModel = string.Empty;
            return false;
        }

        private bool IsSelectableModelId(string modelId)
        {
            if (string.IsNullOrWhiteSpace(modelId))
            {
                return false;
            }

            if (ModelSelector?.ItemsSource is IEnumerable<string> modelIds)
            {
                return modelIds.Contains(modelId, StringComparer.OrdinalIgnoreCase);
            }

            return false;
        }

        private void RefreshModelSelectionState()
        {
            OnPropertyChanged(nameof(ModelSelectionPlaceholderVisibility));
            RefreshContextUsage();
            RefreshInputAvailability();
        }

        private void RefreshInputAvailability()
        {
            OnPropertyChanged(nameof(CanInteractWithAgentChanges));
            bool hasSelectedModel = HasSelectedModel;

            // Keep previews and selection stable until the current request has completed.
            ThreadSelector.IsEnabled = !_isRequestRunning;
            NewThreadButton.IsEnabled = !_isRequestRunning;
            DeleteThreadButton.IsEnabled = !_isRequestRunning;
            ModelSelector.IsEnabled = !_isRequestRunning;

            if (InputBox != null)
            {
                InputBox.IsEnabled = hasSelectedModel && !_isRequestRunning;
            }

            if (SendButton != null)
            {
                bool canInteractWithSendButton = hasSelectedModel || _isRequestRunning;
                SendButton.IsEnabled = canInteractWithSendButton;
                SendButton.Cursor = canInteractWithSendButton ? Cursors.Hand : Cursors.Arrow;
            }
        }

        public IEnumerable<AgentWorkspaceChange> AgentChanges => ActiveThread?.AgentWorkspaceChanges;
        public Visibility AgentChangesVisibility => ActiveThread?.AgentWorkspaceChanges.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        public bool CanInteractWithAgentChanges => !_isRequestRunning;

        private void UndoAgentChanges_Click(object sender, RoutedEventArgs e)
        {
            if (_isRequestRunning || !(sender is FrameworkElement element) || !(element.DataContext is AgentWorkspaceChange change)) return;
            if (change.Undo()) AIThreadManager.Instance.RecordAgentUndo(ActiveThread, change.Title);
        }

        private static string GetResourceText(string resourceKey)
        {
            string value = Properties.Resources.ResourceManager.GetString(resourceKey);
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value;
        }

        private static string GetResourceTextOrFallback(string resourceKey, string fallback)
        {
            string value = GetResourceText(resourceKey);
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private string BuildRunningToolActivityHeaderText(int itemCount)
        {
            return itemCount > 0
                ? FormatResourceText("AiChatToolActivityHeaderMessageCount", "Tool activity ({0})", itemCount)
                : GetResourceTextOrFallback("AiChatToolActivityHeaderMessage", "Tool activity");
        }

        private string BuildCompletedToolActivityHeaderText(int itemCount)
        {
            return itemCount > 0
                ? FormatResourceText("AiChatToolActivityHeaderMessageCount", "Tool activity ({0})", itemCount)
                : GetResourceTextOrFallback("AiChatToolActivityHeaderMessage", "Tool activity");
        }

        private static string FormatResourceText(string resourceKey, string fallbackFormat, params object[] args)
        {
            string format = GetResourceTextOrFallback(resourceKey, fallbackFormat);
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        private static Dictionary<string, bool> ParseAllowedFunctions(string raw)
        {
            var map = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return map;
            }

            string[] parts = raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                string[] kv = part.Split(new[] { '=' }, 2);
                string key = (kv.ElementAtOrDefault(0) ?? string.Empty).Trim();
                string val = (kv.ElementAtOrDefault(1) ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                if (bool.TryParse(val, out bool allowed))
                {
                    map[key] = allowed;
                }
            }

            return map;
        }

        /// <summary>
        /// Detects fatal static-initialization failures of the UI text catalog plugin.
        /// </summary>
        private static bool IsUiTextCatalogInitializationFailure(Exception ex)
        {
            TypeInitializationException tie = ex as TypeInitializationException;
            if (tie != null && string.Equals(tie.TypeName, "CrypTool.CrypLLM.UiTextCatalogPlugin", StringComparison.Ordinal))
            {
                return true;
            }

            return ex?.ToString().IndexOf("CrypTool.CrypLLM.UiTextCatalogPlugin", StringComparison.Ordinal) >= 0;
        }

        /// <summary>
        /// Shows a non-recoverable startup error and terminates the process to avoid undefined UI state.
        /// </summary>
        private static void ShowFatalUiTextCatalogInitializationErrorAndTerminate(Exception ex)
        {
            string details = BuildExceptionDetails(ex);
            string message = string.Format(CultureInfo.CurrentCulture,
                Properties.Resources.AiChatFatalInitializationError, details);
            MessageBox.Show(message, Properties.Resources.AiChatFatalErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(-1);
        }

        /// <summary>
        /// Expands an exception chain into a verbose diagnostic string with stack traces.
        /// </summary>
        private static string BuildExceptionDetails(Exception ex)
        {
            var sb = new StringBuilder();
            int depth = 0;
            Exception current = ex;

            while (current != null)
            {
                sb.AppendLine($"[{depth}] {current.GetType().FullName}");
                sb.AppendLine(current.Message);
                if (!string.IsNullOrWhiteSpace(current.StackTrace))
                {
                    sb.AppendLine(current.StackTrace);
                }

                current = current.InnerException;
                depth++;

                if (current != null)
                {
                    sb.AppendLine();
                    sb.AppendLine("Inner Exception:");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Builds a user-facing chat error message with provider, model, endpoint, and nested error context.
        /// </summary>
        private static string BuildChatErrorMessage(string selectedModel, Exception exception)
        {
            SupportedProviders provider = ProviderToStringConverter.Convert(Properties.Settings.Default.selectedProvider);
            string endpoint = AIThreadManager.GetConfiguredEndpointInfo(provider);
            string model = string.IsNullOrWhiteSpace(selectedModel) ? "<none>" : selectedModel;

            string primaryMessage = exception?.Message?.Trim();
            if (string.IsNullOrWhiteSpace(primaryMessage))
            {
                primaryMessage = Properties.Resources.AiChatUnknownRequestError;
            }
            if (primaryMessage.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            {
                primaryMessage = primaryMessage.Substring("Error:".Length).Trim();
            }

            // Preserve manager-generated structured technical messages unchanged.
            if (primaryMessage.IndexOf("Provider:", StringComparison.OrdinalIgnoreCase) >= 0 &&
                primaryMessage.IndexOf("Endpoint:", StringComparison.OrdinalIgnoreCase) >= 0 &&
                primaryMessage.IndexOf(Properties.Resources.AiChatErrorModelLabel, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return primaryMessage;
            }

            return string.Format(CultureInfo.CurrentCulture,
                Properties.Resources.AiChatRequestErrorDetails,
                provider, model, endpoint, BuildExceptionChain(exception), primaryMessage);
        }

        /// <summary>
        /// Builds a concise structured log line for failed request diagnostics.
        /// </summary>
        private static string BuildUiErrorLogMessage(string selectedModel, Exception exception)
        {
            SupportedProviders provider = ProviderToStringConverter.Convert(Properties.Settings.Default.selectedProvider);
            string endpoint = AIThreadManager.GetConfiguredEndpointInfo(provider);

            string model = string.IsNullOrWhiteSpace(selectedModel) ? "<none>" : selectedModel;
            return $"AI request failed: provider={provider}, model={model}, endpoint={endpoint}, errors={BuildExceptionChain(exception)}";
        }

        /// <summary>
        /// Flattens nested exceptions into a compact single-line chain for logs and UI diagnostics.
        /// </summary>
        private static string BuildExceptionChain(Exception exception)
        {
            if (exception == null)
            {
                return "<none>";
            }

            var parts = new List<string>();
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

        /// <summary>
        /// Deletes the active chat thread through the manager API.
        /// </summary>
        private void DeleteThreadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AIThreadManager.Instance.DeleteActiveThread();
            }
            catch (Exception ex)
            {
                Log.Warning($"DeleteActiveThread failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new chat thread through the manager API.
        /// </summary>
        private void NewThreadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AIThreadManager.Instance.CreateNewThread();
            }
            catch (Exception ex)
            {
                Log.Warning($"CreateNewThread failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Exports the active thread to a JSON file selected by the user.
        /// </summary>
        private void SaveChatButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var thread = AIThreadManager.Instance.ActiveThread;
                if (thread?.ChatHistory == null || !thread.ChatHistory.Any())
                {
                    return;
                }

                var dialog = new SaveFileDialog
                {
                    Filter = Properties.Resources.ChatExportFileFilter,
                    FileName = BuildDefaultExportFileName(thread, "json")
                };

                if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    return;
                }

                string exportJson = BuildChatExportJson(thread);
                File.WriteAllText(dialog.FileName, exportJson, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Log.Warning($"Saving chat failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Opens the AI chat settings dialog via the CrypWin bridge interface.
        /// </summary>
        private void OpenAiSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var bridge = Ports.CrypWinPort.Instance;
                if (bridge == null)
                {
                    Log.Warning("MainWindow bridge is not available. Cannot open AI chat settings.");
                    return;
                }

                bridge.ShowAIChatSettings();
            }
            catch (Exception ex)
            {
                Log.Warning($"Opening AI chat settings failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Opens a debug view for the last invocation history of the active thread.
        /// </summary>
        private void DebugLastRequestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AIThread thread = AIThreadManager.Instance?.ActiveThread;
                LastInvocationDebugInfo info = thread?.LastInvocationDebugInfo;
                if (info == null)
                {
                    MessageBox.Show(
                        Properties.Resources.ResourceManager.GetString("AiChatDebugNoInvocationStored") ?? "No previous AI request is stored for this thread.",
                        Properties.Resources.ResourceManager.GetString("AiChatDebugDialogTitle") ?? "Debug view",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                LastInvocationDebugWindow window = new LastInvocationDebugWindow(thread.InvocationDebugHistory)
                {
                    Owner = Window.GetWindow(this)
                };
                window.Show();
            }
            catch (Exception ex)
            {
                Log.Warning($"Opening last invocation debug window failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Builds a filesystem-safe default file name for chat export.
        /// </summary>
        private static string BuildDefaultExportFileName(AIThread thread, string extension)
        {
            string name = thread?.Name ?? "Chat";
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Chat";
            }

            extension = string.IsNullOrWhiteSpace(extension) ? "json" : extension.TrimStart('.');
            return $"{name}.{extension}";
        }

        /// <summary>
        /// Serializes a chat thread into a reproducible JSON document including message metadata.
        /// </summary>
        private static string BuildChatExportJson(AIThread thread)
        {
            var root = new JObject
            {
                ["thread"] = thread?.Name ?? "Chat",
                ["exported"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            var messages = new JArray();
            // Section: materialize message stream and optional per-item metadata for export.
            foreach (var message in thread.ChatHistory)
            {
                string messageContent = message.Role == AuthorRole.Assistant
                    ? AssistantResponseText.GetVisibleText(message.Content)
                    : message.Content ?? string.Empty;

                var messageObj = new JObject
                {
                    ["role"] = message.Role.ToString(),
                    ["content"] = messageContent
                };

                string timestamp = TryGetMessageTimestamp(message);
                if (!string.IsNullOrWhiteSpace(timestamp))
                {
                    messageObj["timestamp"] = timestamp;
                }

                JArray items = BuildChatItemsArray(message, messageContent);
                if (items != null && items.Count > 0)
                {
                    messageObj["items"] = items;
                }

                messages.Add(messageObj);
            }

            root["messages"] = messages;
            return root.ToString(Formatting.Indented);
        }

        /// <summary>
        /// Builds an export array for message items while suppressing null and duplicate payloads.
        /// </summary>
        private static JArray BuildChatItemsArray(ChatMessageContent message, string messageContent)
        {
            IEnumerable items = GetMessageItems(message);
            if (items == null)
            {
                return null;
            }

            var array = new JArray();
            foreach (var item in items)
            {
                JObject itemObj = BuildChatItemObject(item, messageContent);
                if (itemObj != null)
                {
                    array.Add(itemObj);
                }
            }

            return array;
        }

        /// <summary>
        /// Converts one message item into a normalized JSON object used by chat export.
        /// </summary>
        private static JObject BuildChatItemObject(object item, string messageContent)
        {
            if (item == null)
            {
                return null;
            }

            string typeName = item.GetType().Name;
            var obj = new JObject
            {
                ["type"] = typeName
            };

            if (typeName.Equals("FunctionCallContent", StringComparison.OrdinalIgnoreCase))
            {
                string functionName = TryGetPropertyValueString(item, "FunctionName");
                if (string.IsNullOrWhiteSpace(functionName))
                {
                    functionName = TryGetPropertyValueString(item, "Name");
                }

                string arguments = TryGetPropertyValueString(item, "Arguments");
                if (string.IsNullOrWhiteSpace(arguments))
                {
                    arguments = TryGetPropertyValueString(item, "ArgumentsJson");
                }

                if (!string.IsNullOrWhiteSpace(functionName))
                {
                    obj["functionName"] = functionName;
                }

                if (!string.IsNullOrWhiteSpace(arguments))
                {
                    obj["arguments"] = arguments;
                }

                return obj;
            }

            if (typeName.Equals("FunctionResultContent", StringComparison.OrdinalIgnoreCase))
            {
                string functionName = TryGetPropertyValueString(item, "FunctionName");
                string result = TryGetPropertyValueString(item, "Result");
                if (string.IsNullOrWhiteSpace(result))
                {
                    result = TryGetPropertyValueString(item, "Content");
                }

                if (!string.IsNullOrWhiteSpace(functionName))
                {
                    obj["functionName"] = functionName;
                }

                if (!string.IsNullOrWhiteSpace(result) && !IsDuplicateContent(result, messageContent))
                {
                    obj["result"] = result;
                }

                return obj.HasValues ? obj : null;
            }

            string content = TryGetPropertyValueString(item, "Content");
            if (!string.IsNullOrWhiteSpace(content))
            {
                if (IsDuplicateContent(content, messageContent))
                {
                    return null;
                }

                obj["content"] = content;
                return obj;
            }

            string raw = item.ToString();
            if (!string.IsNullOrWhiteSpace(raw))
            {
                if (IsDuplicateContent(raw, messageContent))
                {
                    return null;
                }

                obj["raw"] = raw;
            }

            return obj;
        }

        /// <summary>
        /// Detects whether candidate text duplicates the main message body and can be omitted.
        /// </summary>
        private static bool IsDuplicateContent(string candidate, string messageContent)
        {
            if (string.IsNullOrWhiteSpace(candidate) || string.IsNullOrWhiteSpace(messageContent))
            {
                return false;
            }

            return string.Equals(candidate.Trim(), messageContent.Trim(), StringComparison.Ordinal);
        }

        /// <summary>
        /// Attempts to read timestamp-like properties from heterogeneous message implementations.
        /// </summary>
        private static string TryGetMessageTimestamp(ChatMessageContent message)
        {
            object raw =
                TryGetPropertyValueRaw(message, "Timestamp") ??
                TryGetPropertyValueRaw(message, "TimeStamp") ??
                TryGetPropertyValueRaw(message, "CreatedAt") ??
                TryGetPropertyValueRaw(message, "CreatedOn");

            if (raw is DateTime dt)
            {
                return dt.ToString("o");
            }

            if (raw is DateTimeOffset dto)
            {
                return dto.ToString("o");
            }

            return raw?.ToString();
        }

        /// <summary>
        /// Legacy helper that exports all message items without duplicate-content filtering.
        /// </summary>
        private static JArray BuildChatItemsArray(ChatMessageContent message)
        {
            IEnumerable items = GetMessageItems(message);
            if (items == null)
            {
                return null;
            }

            var array = new JArray();
            foreach (var item in items)
            {
                JObject itemObj = BuildChatItemObject(item);
                if (itemObj != null)
                {
                    array.Add(itemObj);
                }
            }

            return array;
        }

        /// <summary>
        /// Legacy helper that converts one message item without message-content deduplication context.
        /// </summary>
        private static JObject BuildChatItemObject(object item)
        {
            if (item == null)
            {
                return null;
            }

            string typeName = item.GetType().Name;
            var obj = new JObject
            {
                ["type"] = typeName
            };

            if (typeName.Equals("FunctionCallContent", StringComparison.OrdinalIgnoreCase))
            {
                string functionName = TryGetPropertyValueString(item, "FunctionName");
                if (string.IsNullOrWhiteSpace(functionName))
                {
                    functionName = TryGetPropertyValueString(item, "Name");
                }

                string arguments = TryGetPropertyValueString(item, "Arguments");
                if (string.IsNullOrWhiteSpace(arguments))
                {
                    arguments = TryGetPropertyValueString(item, "ArgumentsJson");
                }

                if (!string.IsNullOrWhiteSpace(functionName))
                {
                    obj["functionName"] = functionName;
                }

                if (!string.IsNullOrWhiteSpace(arguments))
                {
                    obj["arguments"] = arguments;
                }

                return obj;
            }

            if (typeName.Equals("FunctionResultContent", StringComparison.OrdinalIgnoreCase))
            {
                string functionName = TryGetPropertyValueString(item, "FunctionName");
                string result = TryGetPropertyValueString(item, "Result");
                if (string.IsNullOrWhiteSpace(result))
                {
                    result = TryGetPropertyValueString(item, "Content");
                }

                if (!string.IsNullOrWhiteSpace(functionName))
                {
                    obj["functionName"] = functionName;
                }

                if (!string.IsNullOrWhiteSpace(result))
                {
                    obj["result"] = result;
                }

                return obj;
            }

            string content = TryGetPropertyValueString(item, "Content");
            if (!string.IsNullOrWhiteSpace(content))
            {
                obj["content"] = content;
                return obj;
            }

            string raw = item.ToString();
            if (!string.IsNullOrWhiteSpace(raw))
            {
                obj["raw"] = raw;
            }

            return obj;
        }

        /// <summary>
        /// Reflectively reads optional message-item collections to remain compatible across SK versions.
        /// </summary>
        private static IEnumerable GetMessageItems(ChatMessageContent message)
        {
            try
            {
                var prop = message.GetType().GetProperty("Items");
                return prop?.GetValue(message) as IEnumerable;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Reads a property value via reflection and returns null on access failures.
        /// </summary>
        private static object TryGetPropertyValueRaw(object item, string propertyName)
        {
            try
            {
                var prop = item.GetType().GetProperty(propertyName);
                return prop?.GetValue(item);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Reads a property value as string via reflection with empty-string fallback.
        /// </summary>
        private static string TryGetPropertyValueString(object item, string propertyName)
        {
            object value = TryGetPropertyValueRaw(item, propertyName);
            return value == null ? string.Empty : value.ToString();
        }

        /// <summary>
        /// Toggles request-processing UI state (input lock, spinner, and send/stop icon).
        /// </summary>
        private void SetThinkingState(bool isThinking)
        {
            try
            {
                if (isThinking) // Message accepted and currently being processed.
                {
                    InputBox.Clear();
                    SetSendImage(true);

                    ProcessingIndicator.Visibility = Visibility.Visible;
                    _spinnerStoryboard?.Begin(this, true);
                }
                else // Processing finished; restore interactive input state.
                {
                    SetSendImage(false);

                    _spinnerStoryboard?.Stop(this);
                    ProcessingIndicator.Visibility = Visibility.Collapsed;

                    RefreshInputAvailability();
                    if (InputBox.IsEnabled)
                    {
                        InputBox.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to toggle thinking state: {ex.Message}");
            }
        }

        /// <summary>
        /// Applies smooth mouse-wheel scrolling in the chat viewport independent of focus target.
        /// </summary>
        private void ChatScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ChatScrollViewer == null)
            {
                return;
            }

            const double lineHeight = 16;
            double scrollAmount = SystemParameters.WheelScrollLines * lineHeight;
            double newOffset = ChatScrollViewer.VerticalOffset;

            if (e.Delta < 0)
            {
                newOffset += scrollAmount;
            }
            else
            {
                newOffset -= scrollAmount;
            }

            newOffset = Math.Max(0, Math.Min(ChatScrollViewer.ScrollableHeight, newOffset));
            ChatScrollViewer.ScrollToVerticalOffset(newOffset);
            e.Handled = true;
        }

        private void ChatScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_suppressScrollTracking)
            {
                return;
            }

            RememberCurrentThreadScrollOffset();
        }

        private enum ChatViewportUpdateMode
        {
            FollowIfNearBottom,
            FollowLatestAssistantStartIfNearBottom,
            ForceBottom,
            RestoreThreadPosition
        }
    }

    /// <summary>
    /// Simple DTO used for UI binding of chat messages.
    /// </summary>
    public class ChatMessage
    {
        public string Text { get; set; }
        public AuthorRole Role { get; set; }
        public bool IsError { get; set; }
        public bool IsToolActivitySummary { get; set; }
        public string ToolActivityStateKey { get; set; }
        public bool IsToolActivityExpanded { get; set; }
        public List<ToolActivityViewModel> ToolActivities { get; set; } = new List<ToolActivityViewModel>();
    }

    public sealed class ToolActivityViewModel
    {
        public int Sequence { get; set; }
        public string ToolDisplayName { get; set; }
        public string FunctionName { get; set; }
        public string DetailStateKey { get; set; }
        public bool IsExpanded { get; set; }
        public string ToolDescription { get; set; }
        public string Arguments { get; set; }
        public IReadOnlyList<ToolArgumentItemViewModel> ArgumentItems => AIChatContent.BuildArgumentItems(Arguments);
        public string DisplayArgumentsText => AIChatContent.BuildArgumentsDisplayText(Arguments);
        public string ErrorDetails { get; set; }
        public string StatusTooltip { get; set; }
        public string StatusLabel { get; set; }
        public Brush StatusBackground { get; set; }
        public Brush StatusForeground { get; set; }
        public bool HasToolDescription => !string.IsNullOrWhiteSpace(ToolDescription);
        public bool HasArguments => ArgumentItems.Count > 0;
        public bool HasErrorDetails => !string.IsNullOrWhiteSpace(ErrorDetails);
        public bool HasStatusTooltip => !string.IsNullOrWhiteSpace(StatusTooltip);
        public bool HasExpandableDetails => HasArguments;
    }

    public sealed class ToolArgumentItemViewModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsGroupHeader { get; set; }
        public bool HasName => !string.IsNullOrWhiteSpace(Name);
        public bool HasValue => !string.IsNullOrWhiteSpace(Value);
        public bool ShowSeparator => HasName && !IsGroupHeader;
    }

    internal sealed class UnavailableToolInfo
    {
        public string FunctionName { get; set; }
        public int? RequiredMinContextTokens { get; set; }
        public List<string> SettingsReasons { get; set; }
    }
}
