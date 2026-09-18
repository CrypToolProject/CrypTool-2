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
using CrypTool.CrypLLM.AgentInstructions;
using CrypTool.CrypLLM.Converter;
using CrypTool.CrypLLM.Helper;
using CrypTool.CrypLLM.Properties;
using CrypTool.CrypLLM.Threads;
using CrypTool.PluginBase.Attributes;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

// Module overview:
// Settings UI/controller for provider, model, prompt-instruction, context-window,
// logging, and tool-permission configuration used by CrypLLM runtime components.
namespace CrypTool.CrypLLM
{
    /// <summary>
    /// WPF settings tab that persists LLM-related configuration and synchronizes
    /// runtime-relevant changes with <see cref="AIThreadManager"/>.
    /// </summary>
    /// <remarks>
    /// Scope: provider/model selection, instruction editing, context-window overrides,
    /// tool permission policies, and secure credential persistence.
    /// </remarks>
    [Localization("CrypTool.CrypLLM.Properties.Resources")]
    [SettingsTab("CrypLLMSettings", "/MainSettings/")]
    public partial class LLMSettingsTab : UserControl, INotifyPropertyChanged
    {
        private const int SmallContextSystemPromptMaxChars = 1000;
        private const int MinimumModelContextWindowTokens = 4096;
        private const string SmallContextInstructionKey = "SmallContextDefault";
        private string _orgId = Settings.Default.orgId;
        private string _apiKey;
        private string _localApiKey;
        private string _selectedContextWindowModelId;

        private InstructionEntry _selectedInstruction;

        private bool _toolPermissionsLoaded;
        private Dictionary<string, bool> _toolPermissionsMap = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private readonly List<DataGrid> _toolPermissionGrids = new List<DataGrid>();
        private readonly Dictionary<DataGridColumn, int> _toolPermissionColumnIndices = new Dictionary<DataGridColumn, int>();
        private readonly double[] _sharedToolPermissionColumnWidths = new double[4];
        private bool _syncingToolPermissionColumnWidths;
        private static readonly Dictionary<Type, List<string>> PluginFunctionNameCache = new Dictionary<Type, List<string>>();
        private static readonly Dictionary<Type, HashSet<string>> PluginMutationFunctionNameCache = new Dictionary<Type, HashSet<string>>();
        private static readonly ResourceManager ToolDescriptionsResourceManager =
            new ResourceManager("CrypTool.CrypLLM.Properties.ToolDescriptions", typeof(LLMSettingsTab).Assembly);

        /// <summary>
        /// Static metadata describing one tool-permission plugin group in the UI.
        /// </summary>
        private sealed class PermissionPluginInfo
        {
            public string PluginKey { get; set; }
            public string TitleResourceKey { get; set; }
            public string TitleFallback { get; set; }
            public string DescriptionResourceKey { get; set; }
            public string DescriptionFallback { get; set; }
            public Type PluginType { get; set; }
        }

        private static readonly PermissionPluginInfo[] PermissionPluginInfos =
        {
            new PermissionPluginInfo
            {
                PluginKey = "CrypLLM_WorkspaceStatus",
                TitleResourceKey = "AiToolPermissionsGroupWorkspaceTitle",
                TitleFallback = "Workspace status",
                DescriptionResourceKey = "AiToolPermissionsGroupWorkspaceDescription",
                DescriptionFallback = "Read workspace structure, run/stop state, logs and validation information.",
                PluginType = typeof(WorkspaceStatusPlugin)
            },
            new PermissionPluginInfo
            {
                PluginKey = "CrypLLM_ComponentCatalog",
                TitleResourceKey = "AiToolPermissionsGroupCatalogTitle",
                TitleFallback = "Component catalog",
                DescriptionResourceKey = "AiToolPermissionsGroupCatalogDescription",
                DescriptionFallback = "Browse available components and read global component documentation/default settings.",
                PluginType = typeof(ComponentCatalogPlugin)
            },
            new PermissionPluginInfo
            {
                PluginKey = "CrypLLM_TemplateCatalog",
                TitleResourceKey = "AiToolPermissionsGroupTemplatesTitle",
                TitleFallback = "Templates",
                DescriptionResourceKey = "AiToolPermissionsGroupTemplatesDescription",
                DescriptionFallback = "Inspect template files, metadata and template workspace content, and open an existing template in a new tab.",
                PluginType = typeof(TemplateCatalogPlugin)
            },
            new PermissionPluginInfo
            {
                PluginKey = "CrypLLM_UiTextCatalog",
                TitleResourceKey = "AiToolPermissionsGroupUiTextsTitle",
                TitleFallback = "UI help texts",
                DescriptionResourceKey = "AiToolPermissionsGroupUiTextsDescription",
                DescriptionFallback = "Read descriptive UI/help texts from documentation and instruction sources.",
                PluginType = typeof(UiTextCatalogPlugin)
            },
            new PermissionPluginInfo
            {
                PluginKey = "CrypLLM_WorkspaceEditing",
                TitleResourceKey = "AiToolPermissionsGroupWorkspaceEditTitle",
                TitleFallback = "Workspace editing",
                DescriptionResourceKey = "AiToolPermissionsGroupWorkspaceEditDescription",
                DescriptionFallback = "Modify the active workspace by adding/removing components, editing values and managing connections.",
                PluginType = typeof(WorkspaceEditingPlugin)
            }
        };

        /// <summary>
        /// Editable permission row representing one kernel function.
        /// </summary>
        public sealed class ToolPermissionEntry : INotifyPropertyChanged
        {
            private bool _isAllowed;
            private string _description;
            private bool _isMutating;
            private bool _isPermissionLocked;
            private string _minimumContextWindowDisplay;

            public string FunctionName { get; set; }
            public string Description
            {
                get => _description;
                set
                {
                    string next = value ?? string.Empty;
                    if (string.Equals(_description ?? string.Empty, next, StringComparison.Ordinal))
                    {
                        return;
                    }

                    _description = next;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
                }
            }

            public bool IsAllowed
            {
                get => _isAllowed;
                set
                {
                    if (_isAllowed == value)
                    {
                        return;
                    }

                    _isAllowed = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAllowed)));
                }
            }

            public bool IsMutating
            {
                get => _isMutating;
                set
                {
                    if (_isMutating == value)
                    {
                        return;
                    }

                    _isMutating = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsMutating)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanEditPermission)));
                }
            }

            public bool IsPermissionLocked
            {
                get => _isPermissionLocked;
                set
                {
                    if (_isPermissionLocked == value)
                    {
                        return;
                    }

                    _isPermissionLocked = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPermissionLocked)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanEditPermission)));
                }
            }

            public string MinimumContextWindowDisplay
            {
                get => _minimumContextWindowDisplay;
                set
                {
                    string next = value ?? string.Empty;
                    if (string.Equals(_minimumContextWindowDisplay ?? string.Empty, next, StringComparison.Ordinal))
                    {
                        return;
                    }

                    _minimumContextWindowDisplay = next;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MinimumContextWindowDisplay)));
                }
            }

            public bool CanEditPermission => !IsPermissionLocked;

            public event PropertyChangedEventHandler PropertyChanged;
        }

        /// <summary>
        /// Grouped permission entries for a single plugin domain.
        /// </summary>
        public sealed class ToolPermissionGroup
        {
            public string PluginKey { get; set; }
            public string PluginDisplayName { get; set; }
            public string PluginDescription { get; set; }
            public ObservableCollection<ToolPermissionEntry> Permissions { get; } = new ObservableCollection<ToolPermissionEntry>();
        }

        public ObservableCollection<ToolPermissionGroup> ToolPermissionGroups { get; } = new ObservableCollection<ToolPermissionGroup>();

        // Stable UI enum (persisted as text via Settings.Default.reasoningLevel)
        public enum ReasoningEffortSetting
        {
            Low,
            Medium,
            High
        }

        /// <summary>
        /// Display/value pair for reasoning-effort selection.
        /// </summary>
        public sealed class ReasoningOption
        {
            public string Value { get; set; }   // "low" | "medium" | "high"
            public string Display { get; set; } // localized
        }

        /// <summary>
        /// Display/value pair for LLM log-level selection.
        /// </summary>
        public sealed class LlmLogLevelOption
        {
            public string Value { get; set; }   // "debug" | "info" | "warning" | "error"
            public string Display { get; set; } // localized
        }

        public IReadOnlyList<ReasoningOption> ReasoningOptions { get; } =
            new List<ReasoningOption>
            {
                new ReasoningOption { Value = "low", Display = Properties.Resources.ReasoningEffortLow },
                new ReasoningOption { Value = "medium", Display = Properties.Resources.ReasoningEffortMedium },
                new ReasoningOption { Value = "high", Display = Properties.Resources.ReasoningEffortHigh },
            };

        public IReadOnlyList<LlmLogLevelOption> LlmLogLevelOptions { get; } =
            new List<LlmLogLevelOption>
            {
                new LlmLogLevelOption { Value = "debug", Display = GetResourceOrFallback("LlmLogLevelDebug", "Debug") },
                new LlmLogLevelOption { Value = "info", Display = GetResourceOrFallback("LlmLogLevelInfo", "Info") },
                new LlmLogLevelOption { Value = "warning", Display = GetResourceOrFallback("LlmLogLevelWarning", "Warning") },
                new LlmLogLevelOption { Value = "error", Display = GetResourceOrFallback("LlmLogLevelError", "Error") },
            };

        public string ReasoningLevel
        {
            get
            {
                var raw = (Settings.Default.reasoningLevel ?? "medium").Trim();
                return NormalizeReasoningLevel(raw);
            }
            set
            {
                var next = NormalizeReasoningLevel(value);

                var current = (Settings.Default.reasoningLevel ?? string.Empty).Trim();
                if (string.Equals(current, next, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                Settings.Default.reasoningLevel = next;
                Settings.Default.Save();
                OnPropertyChanged();

                AIThreadManager.Instance.AgentsPropertysChanged = true;
            }
        }

        private static string NormalizeReasoningLevel(string raw)
        {
            raw = (raw ?? string.Empty).Trim();

            if (raw.Equals("low", StringComparison.OrdinalIgnoreCase))
            {
                return "low";
            }
            if (raw.Equals("high", StringComparison.OrdinalIgnoreCase))
            {
                return "high";
            }

            // Default fallback keeps persisted values valid across legacy states.
            return "medium";
        }

        public string LlmLogLevel
        {
            get
            {
                var raw = (Settings.Default.llmLogLevel ?? "info").Trim();
                return NormalizeLlmLogLevel(raw);
            }
            set
            {
                var next = NormalizeLlmLogLevel(value);

                var current = (Settings.Default.llmLogLevel ?? string.Empty).Trim();
                if (string.Equals(current, next, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                Settings.Default.llmLogLevel = next;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public string AllowAiToolMutationsTooltip =>
            GetResourceOrFallback(
                "AllowAiToolMutationsHint",
                "If disabled, AI cannot modify workspaces or other CrypTool content.");

        public string AiToolPermissionsLockedTooltipText =>
            GetResourceOrFallback(
                "AiToolPermissionsLockedTooltip",
                "This tool is disabled while AI changes are not allowed.");

        private static string NormalizeLlmLogLevel(string raw)
        {
            raw = (raw ?? string.Empty).Trim();

            if (raw.Equals("debug", StringComparison.OrdinalIgnoreCase))
            {
                return "debug";
            }
            if (raw.Equals("warning", StringComparison.OrdinalIgnoreCase))
            {
                return "warning";
            }
            if (raw.Equals("error", StringComparison.OrdinalIgnoreCase))
            {
                return "error";
            }

            // Default fallback keeps persisted values valid across legacy states.
            return "info";
        }

        public InstructionEntry SelectedInstruction
        {
            get => _selectedInstruction;
            set
            {
                if (ReferenceEquals(_selectedInstruction, value))
                {
                    return;
                }

                _selectedInstruction = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedInstructionKey));
                OnPropertyChanged(nameof(SelectedInstructionText));
                OnPropertyChanged(nameof(IsSelectedInstructionEditable));
                OnPropertyChanged(nameof(IsSelectedInstructionUsingDefaultText));
                OnPropertyChanged(nameof(IsSelectedInstructionOverLengthLimit));
                OnPropertyChanged(nameof(SelectedInstructionDefaultStateText));
            }
        }

        /// <summary>
        /// Adds Ctrl+Z / Ctrl+Y / Ctrl+Shift+Z editing behavior to the instruction editor.
        /// </summary>
        private void InstructionTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(sender is TextBox textBox) || !ReferenceEquals(textBox, InstructionEditorTextBox))
            {
                return;
            }

            if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
            {
                return;
            }

            if (e.Key == Key.Z)
            {
                bool isRedoByShift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
                if (isRedoByShift)
                {
                    if (ApplicationCommands.Redo.CanExecute(null, textBox))
                    {
                        ApplicationCommands.Redo.Execute(null, textBox);
                        e.Handled = true;
                    }
                    return;
                }

                if (ApplicationCommands.Undo.CanExecute(null, textBox))
                {
                    ApplicationCommands.Undo.Execute(null, textBox);
                    e.Handled = true;
                }
                return;
            }

            if (e.Key == Key.Y)
            {
                if (ApplicationCommands.Redo.CanExecute(null, textBox))
                {
                    ApplicationCommands.Redo.Execute(null, textBox);
                    e.Handled = true;
                }
            }
        }

        public string SelectedInstructionKey => SelectedInstruction?.Key;

        public string SelectedInstructionText => SelectedInstruction?.Text;

        public bool IsSelectedInstructionUsingDefaultText => SelectedInstruction != null && SelectedInstruction.IsUsingDefaultText;

        public string SelectedInstructionDefaultStateText
        {
            get
            {
                if (SelectedInstruction == null)
                {
                    return string.Empty;
                }

                if (IsSelectedInstructionOverLengthLimit)
                {
                    int currentLength = GetSelectedInstructionLengthForUi();
                    int overflow = Math.Max(0, currentLength - SmallContextSystemPromptMaxChars);
                    return $"Bei kleinen Modellen werden nur die ersten {SmallContextSystemPromptMaxChars} Zeichen gesendet (aktuell {currentLength}, {overflow} zu viel).";
                }

                return IsSelectedInstructionUsingDefaultText
                    ? Properties.Resources.InstructionDefaultStateMatches
                    : Properties.Resources.InstructionDefaultStateDiffers;
            }
        }

        public bool IsSelectedInstructionOverLengthLimit =>
            string.Equals(SelectedInstruction?.Key, SmallContextInstructionKey, StringComparison.OrdinalIgnoreCase) &&
            GetSelectedInstructionLengthForUi() > SmallContextSystemPromptMaxChars;

        public bool IsSelectedInstructionEditable => SelectedInstruction != null && SelectedInstruction.IsUserAdjustable;

        /// <summary>
        /// Initializes settings bindings and loads persisted configuration into UI state.
        /// </summary>
        public LLMSettingsTab(Style settingsStyle)
        {
            InitializeComponent();
            AIThreadManager.ApplyLocalProviderDefaultsMigration();
            Resources.Add("settingsStyle", settingsStyle);
            DataContext = this;

            var decryptedApiKey = Settings.Default.apiKey;
            _apiKey = SecretProtector.TryDecryptOrReturn(decryptedApiKey);
            ApiKeyBox.Password = _apiKey;
            _localApiKey = SecretProtector.TryDecryptOrReturn(Settings.Default.localApiKey);
            LocalApiKeyBox.Password = _localApiKey;

            LoadInstructionEntries();
            LoadToolPermissions();
            EnsureSelectedContextWindowModelId();

            Settings.Default.PropertyChanged += Settings_PropertyChanged;
        }

        /// <summary>
        /// Persists the opt-in visibility of the AI chat ribbon group.
        /// The main window observes the setting directly, so changes apply immediately.
        /// </summary>
        public bool AiChatShowInRibbon
        {
            get => Settings.Default.aiChatShowInRibbon;
            set
            {
                if (Settings.Default.aiChatShowInRibbon == value)
                {
                    return;
                }

                Settings.Default.aiChatShowInRibbon = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        /// <summary>Controls proactive history compaction; mandatory budget checks remain active.</summary>
        public bool AutoCompressContext
        {
            get => Settings.Default.autoCompressContext;
            set
            {
                if (Settings.Default.autoCompressContext == value) return;
                Settings.Default.autoCompressContext = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        /// <summary>History budget occupancy at which proactive compaction starts (2–100%).</summary>
        public int ContextCompressionTriggerPercent
        {
            get => Settings.Default.contextCompressionTriggerPercent;
            set
            {
                if (value < 2 || value > 100 || value <= ContextCompressionTargetPercent)
                    throw new ArgumentException(Properties.Resources.ResourceManager.GetString("ContextCompressionInvalidPercent"));
                if (value == Settings.Default.contextCompressionTriggerPercent) return;
                Settings.Default.contextCompressionTriggerPercent = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        /// <summary>Desired history budget occupancy after compaction (1–99%, below trigger).</summary>
        public int ContextCompressionTargetPercent
        {
            get => Settings.Default.contextCompressionTargetPercent;
            set
            {
                if (value < 1 || value >= ContextCompressionTriggerPercent)
                    throw new ArgumentException(Properties.Resources.ResourceManager.GetString("ContextCompressionInvalidPercent"));
                if (value == Settings.Default.contextCompressionTargetPercent) return;
                Settings.Default.contextCompressionTargetPercent = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public bool AiChatOpenOnStartup
        {
            get => Settings.Default.aiChatOpenOnStartup;
            set
            {
                if (Settings.Default.aiChatOpenOnStartup == value)
                {
                    return;
                }

                Settings.Default.aiChatOpenOnStartup = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Reacts to relevant persisted-setting changes and refreshes dependent UI projections.
        /// </summary>
        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.openAiModelIds))
            {
                OnPropertyChanged(nameof(OpenAiModelIds));
                OnPropertyChanged(nameof(OpenAiModelIdsList));
                OnPropertyChanged(nameof(ContextWindowModelIds));
                EnsureSelectedContextWindowModelId();
                NotifyContextWindowStateChanged();
                if (!IsLocalSelected)
                {
                    OnPropertyChanged(nameof(CurrentModelIdsList));
                }
            }
            else if (e.PropertyName == nameof(Settings.localModelIds))
            {
                OnPropertyChanged(nameof(LocalModelIds));
                OnPropertyChanged(nameof(LocalModelIdsList));
                OnPropertyChanged(nameof(ContextWindowModelIds));
                EnsureSelectedContextWindowModelId();
                NotifyContextWindowStateChanged();
                if (IsLocalSelected)
                {
                    OnPropertyChanged(nameof(CurrentModelIdsList));
                }
            }
            else if (e.PropertyName == nameof(Settings.activeModelId))
            {
                OnPropertyChanged(nameof(ActiveModelId));
                NotifyContextWindowStateChanged();
            }
            else if (e.PropertyName == nameof(Settings.selectedProvider))
            {
                OnPropertyChanged(nameof(SelectedProvider));
                OnPropertyChanged(nameof(IsOpenAiSelected));
                OnPropertyChanged(nameof(IsLocalSelected));
                OnPropertyChanged(nameof(CurrentModelIdsList));
                OnPropertyChanged(nameof(ContextWindowModelIds));
                EnsureSelectedContextWindowModelId();
                NotifyContextWindowStateChanged();
            }
            else if (e.PropertyName == nameof(Settings.reasoningLevel))
            {
                OnPropertyChanged(nameof(ReasoningLevel));
            }
            else if (e.PropertyName == nameof(Settings.llmLogLevel))
            {
                OnPropertyChanged(nameof(LlmLogLevel));
            }
            else if (e.PropertyName == nameof(Settings.editorStatusAllowedFunctions))
            {
                LoadToolPermissions();
            }
            else if (e.PropertyName == nameof(Settings.autoCompressContext))
            {
                OnPropertyChanged(nameof(AutoCompressContext));
            }
            else if (e.PropertyName == nameof(Settings.contextCompressionTriggerPercent))
            {
                OnPropertyChanged(nameof(ContextCompressionTriggerPercent));
            }
            else if (e.PropertyName == nameof(Settings.contextCompressionTargetPercent))
            {
                OnPropertyChanged(nameof(ContextCompressionTargetPercent));
            }
            else if (e.PropertyName == nameof(Settings.aiChatShowInRibbon))
            {
                OnPropertyChanged(nameof(AiChatShowInRibbon));
            }
            else if (e.PropertyName == nameof(Settings.aiChatOpenOnStartup))
            {
                OnPropertyChanged(nameof(AiChatOpenOnStartup));
            }
            else if (e.PropertyName == nameof(Settings.allowAiToolMutations))
            {
                OnPropertyChanged(nameof(AllowAiToolMutations));
                OnPropertyChanged(nameof(AllowAiToolMutationsTooltip));
                OnPropertyChanged(nameof(AiToolPermissionsLockedTooltipText));
                UpdateMutationPermissionLocks();
            }
            else if (e.PropertyName == nameof(Settings.userModelContextWindows))
            {
                NotifyContextWindowStateChanged();
            }
            if (e.PropertyName == nameof(Settings.selectedProvider) ||
                e.PropertyName == nameof(Settings.localModelEndpoint) ||
                e.PropertyName == nameof(Settings.localApiKey) ||
                e.PropertyName == nameof(Settings.orgId) ||
                e.PropertyName == nameof(Settings.apiKey) ||
                e.PropertyName == nameof(Settings.reasoningLevel) ||
                e.PropertyName == nameof(Settings.editorStatusAllowedFunctions) ||
                e.PropertyName == nameof(Settings.allowAiToolMutations) ||
                e.PropertyName == nameof(Settings.userModelContextWindows))
            {
                AIThreadManager.Instance.AgentsPropertysChanged = true;
            }
        }

        /// <summary>
        /// Materializes tool-permission groups from plugin metadata and persisted allow/deny map.
        /// </summary>
        private void LoadToolPermissions()
        {
            if (!_toolPermissionsLoaded)
            {
                ToolPermissionGroups.CollectionChanged += (s, e) =>
                {
                    if (e.NewItems != null)
                    {
                        foreach (object item in e.NewItems)
                        {
                            if (item is ToolPermissionGroup group)
                            {
                                group.Permissions.CollectionChanged += ToolPermissionsInGroup_CollectionChanged;
                                foreach (ToolPermissionEntry entry in group.Permissions)
                                {
                                    entry.PropertyChanged += ToolPermissionEntry_PropertyChanged;
                                }
                            }
                        }
                    }

                    if (e.OldItems != null)
                    {
                        foreach (object item in e.OldItems)
                        {
                            if (item is ToolPermissionGroup group)
                            {
                                group.Permissions.CollectionChanged -= ToolPermissionsInGroup_CollectionChanged;
                                foreach (ToolPermissionEntry entry in group.Permissions)
                                {
                                    entry.PropertyChanged -= ToolPermissionEntry_PropertyChanged;
                                }
                            }
                        }
                    }
                };

                _toolPermissionsLoaded = true;
            }

            _toolPermissionsMap = ParseAllowedFunctions(Settings.Default.editorStatusAllowedFunctions);

            ToolPermissionGroups.Clear();
            foreach (var info in PermissionPluginInfos)
            {
                var group = new ToolPermissionGroup
                {
                    PluginKey = info.PluginKey,
                    PluginDisplayName = GetResourceOrFallback(info.TitleResourceKey, info.TitleFallback),
                    PluginDescription = GetResourceOrFallback(info.DescriptionResourceKey, info.DescriptionFallback)
                };

                foreach (string functionName in GetKernelFunctionNames(info.PluginType))
                {
                    bool allowed = !_toolPermissionsMap.TryGetValue(functionName, out bool v) || v;
                    bool isMutating = IsMutatingKernelFunction(info.PluginType, functionName);
                    string description = GetToolDescriptionOrFallback(functionName, string.Empty);
                    int minimumContextWindowTokens = GetMinimumContextWindowTokens(info.PluginType, functionName);

                    group.Permissions.Add(new ToolPermissionEntry
                    {
                        FunctionName = functionName,
                        MinimumContextWindowDisplay = minimumContextWindowTokens > 0
                            ? minimumContextWindowTokens.ToString("N0", CultureInfo.CurrentCulture)
                            : GetResourceOrFallback("AiToolPermissionsNoMinContextValue", string.Empty),
                        Description = description ?? string.Empty,
                        IsAllowed = allowed,
                        IsMutating = isMutating,
                        IsPermissionLocked = !Settings.Default.allowAiToolMutations && isMutating
                    });
                }

                ToolPermissionGroups.Add(group);
            }

            UpdateMutationPermissionLocks();
        }

        /// <summary>
        /// Locks mutating tool permissions when global mutation allowance is disabled.
        /// </summary>
        private void UpdateMutationPermissionLocks()
        {
            bool lockMutating = !Settings.Default.allowAiToolMutations;

            foreach (ToolPermissionGroup group in ToolPermissionGroups)
            {
                if (group == null)
                {
                    continue;
                }

                foreach (ToolPermissionEntry entry in group.Permissions)
                {
                    if (entry == null)
                    {
                        continue;
                    }

                    entry.IsPermissionLocked = lockMutating && entry.IsMutating;
                }
            }
        }

        private void ToolPermissionsDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (!(sender is DataGrid grid) || grid.Columns.Count < 4)
            {
                return;
            }

            if (!_toolPermissionGrids.Contains(grid))
            {
                _toolPermissionGrids.Add(grid);
            }

            for (int i = 0; i < 4; i++)
            {
                DataGridColumn column = grid.Columns[i];
                if (!_toolPermissionColumnIndices.ContainsKey(column))
                {
                    _toolPermissionColumnIndices[column] = i;
                    DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn));
                    descriptor?.AddValueChanged(column, ToolPermissionsColumnWidth_ValueChanged);
                }
            }

            bool needsInitialization = _sharedToolPermissionColumnWidths.Any(w => w <= 0);
            if (needsInitialization)
            {
                for (int i = 0; i < 4; i++)
                {
                    double width = grid.Columns[i].ActualWidth;
                    if (width <= 0)
                    {
                        width = grid.Columns[i].Width.DisplayValue;
                    }

                    if (width > 0)
                    {
                        _sharedToolPermissionColumnWidths[i] = width;
                    }
                }
            }

            ApplyToolPermissionColumnWidths();
        }

        private void ToolPermissionsColumnWidth_ValueChanged(object sender, EventArgs e)
        {
            if (_syncingToolPermissionColumnWidths)
            {
                return;
            }

            if (!(sender is DataGridColumn column) || !_toolPermissionColumnIndices.TryGetValue(column, out int index))
            {
                return;
            }

            double width = column.ActualWidth;
            if (width <= 0)
            {
                width = column.Width.DisplayValue;
            }

            if (width <= 0)
            {
                return;
            }

            _sharedToolPermissionColumnWidths[index] = width;
            ApplyToolPermissionColumnWidths();
        }

        private void ApplyToolPermissionColumnWidths()
        {
            _syncingToolPermissionColumnWidths = true;
            try
            {
                foreach (DataGrid grid in _toolPermissionGrids.ToList())
                {
                    if (grid == null || grid.Columns.Count < 4)
                    {
                        continue;
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        double width = _sharedToolPermissionColumnWidths[i];
                        if (width > 0)
                        {
                            grid.Columns[i].Width = new DataGridLength(width, DataGridLengthUnitType.Pixel);
                        }
                    }
                }
            }
            finally
            {
                _syncingToolPermissionColumnWidths = false;
            }
        }

        private void ToolPermissionsDataGrid_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var scrollViewer = FindScrollableAncestor(sender as DependencyObject) ?? ToolPermissionsScrollViewer;
            if (scrollViewer == null)
            {
                return;
            }

            const double lineHeight = 16;
            double scrollAmount = SystemParameters.WheelScrollLines * lineHeight;
            double newOffset = scrollViewer.VerticalOffset;

            if (e.Delta < 0)
            {
                newOffset += scrollAmount;
            }
            else
            {
                newOffset -= scrollAmount;
            }

            newOffset = Math.Max(0, Math.Min(scrollViewer.ScrollableHeight, newOffset));
            scrollViewer.ScrollToVerticalOffset(newOffset);
            e.Handled = true;
        }

        private static ScrollViewer FindScrollableAncestor(DependencyObject start)
        {
            DependencyObject current = start;
            ScrollViewer fallback = null;

            while (current != null)
            {
                if (current is ScrollViewer sv)
                {
                    if (fallback == null)
                    {
                        fallback = sv;
                    }

                    if (sv.ScrollableHeight > 0)
                    {
                        return sv;
                    }
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return fallback;
        }

        private void ToolPermissionsInGroup_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (object item in e.NewItems)
                {
                    if (item is ToolPermissionEntry entry)
                    {
                        entry.PropertyChanged += ToolPermissionEntry_PropertyChanged;
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (object item in e.OldItems)
                {
                    if (item is ToolPermissionEntry entry)
                    {
                        entry.PropertyChanged -= ToolPermissionEntry_PropertyChanged;
                    }
                }
            }
        }

        private void ToolPermissionEntry_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(ToolPermissionEntry.IsAllowed))
            {
                return;
            }

            SaveToolPermissions();
        }

        /// <summary>
        /// Persists function-level permission overrides and marks agent configuration as dirty.
        /// </summary>
        private void SaveToolPermissions()
        {
            try
            {
                var map = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

                foreach (ToolPermissionGroup group in ToolPermissionGroups)
                {
                    if (group == null)
                    {
                        continue;
                    }

                    foreach (ToolPermissionEntry entry in group.Permissions)
                    {
                        if (entry == null || string.IsNullOrWhiteSpace(entry.FunctionName))
                        {
                            continue;
                        }

                        string functionName = entry.FunctionName.Trim();
                        map[functionName] = entry.IsAllowed;
                    }
                }

                Settings.Default.editorStatusAllowedFunctions = SerializeAllowedFunctions(map);
                Settings.Default.Save();

                AIThreadManager.Instance.AgentsPropertysChanged = true;
            }
            catch
            {
                // keep UI silent
            }
        }

        /// <summary>
        /// Parses semicolon-delimited function policies in the form <c>name=true|false</c>.
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
        /// Serializes function policies into the persisted semicolon-delimited format.
        /// </summary>
        private static string SerializeAllowedFunctions(Dictionary<string, bool> map)
        {
            if (map == null || map.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(";", map
                .OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase)
                .Select(kvp => $"{kvp.Key}={kvp.Value.ToString().ToLowerInvariant()}"));
        }

        /// <summary>
        /// Reflectively extracts published kernel function names from a plugin type.
        /// </summary>
        private static List<string> GetKernelFunctionNames(Type pluginType)
        {
            if (pluginType == null)
            {
                return new List<string>();
            }

            if (PluginFunctionNameCache.TryGetValue(pluginType, out List<string> cached))
            {
                return cached;
            }

            List<string> functionNames = pluginType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Select(m => m.GetCustomAttribute<KernelFunctionAttribute>())
                .Where(attr => attr != null && !string.IsNullOrWhiteSpace(attr.Name))
                .Select(attr => attr.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            PluginFunctionNameCache[pluginType] = functionNames;
            return functionNames;
        }

        /// <summary>
        /// Determines whether a kernel function is marked as workspace-mutating.
        /// </summary>
        private static bool IsMutatingKernelFunction(Type pluginType, string functionName)
        {
            if (pluginType == null || string.IsNullOrWhiteSpace(functionName))
            {
                return false;
            }

            if (!PluginMutationFunctionNameCache.TryGetValue(pluginType, out HashSet<string> mutatingNames))
            {
                mutatingNames = pluginType
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Select(m => new
                    {
                        Kernel = m.GetCustomAttribute<KernelFunctionAttribute>(),
                        Mutation = m.GetCustomAttribute<AiToolMutationAttribute>()
                    })
                    .Where(x => x.Kernel != null && !string.IsNullOrWhiteSpace(x.Kernel.Name) && x.Mutation?.CanModifyProgram == true)
                    .Select(x => x.Kernel.Name.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                PluginMutationFunctionNameCache[pluginType] = mutatingNames;
            }

            return mutatingNames.Contains(functionName.Trim());
        }

        private static int GetMinimumContextWindowTokens(Type pluginType, string functionName)
        {
            if (pluginType == null || string.IsNullOrWhiteSpace(functionName))
            {
                return 0;
            }

            MethodInfo method = pluginType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(m =>
                {
                    KernelFunctionAttribute kernel = m.GetCustomAttribute<KernelFunctionAttribute>();
                    return kernel != null &&
                           !string.IsNullOrWhiteSpace(kernel.Name) &&
                           string.Equals(kernel.Name.Trim(), functionName.Trim(), StringComparison.OrdinalIgnoreCase);
                });

            return method?.GetCustomAttribute<ContextWindowTokenAttribute>()?.MinTokens ?? 0;
        }

        private static string GetToolDescriptionOrFallback(string functionName, string fallback)
        {
            if (string.IsNullOrWhiteSpace(functionName))
            {
                return fallback;
            }

            string key = "ToolDescription_" + functionName.Trim();
            string value = ToolDescriptionsResourceManager.GetString(key);
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static string GetResourceOrFallback(string resourceKey, string fallback)
        {
            string value = CrypTool.CrypLLM.Properties.Resources.ResourceManager.GetString(resourceKey);
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            return value;
        }

        // Descriptions are loaded from ToolDescriptions.resx / ToolDescriptions.de.resx and are not user-editable.

        /// <summary>
        /// Loads instruction entries and synchronizes selection/edit-state bindings.
        /// </summary>
        private void LoadInstructionEntries()
        {
            try
            {
                // Erzwingt Laden/Initialisieren des InstructionManagers; Fehler werden dort geloggt.
                _ = InstructionManager.Instance.Entries;
            }
            catch (Exception ex)
            {
                Log.Warning("Loading AgentInstructions.json failed: " + ex.Message);
            }

            SelectedInstruction = InstructionEntries.FirstOrDefault();
            OnPropertyChanged(nameof(InstructionEntries));
        }

        /// <summary>
        /// Restores the selected instruction text to its default template.
        /// </summary>
        private void LoadDefaultInstructionButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedInstruction == null || !SelectedInstruction.IsUserAdjustable)
            {
                return;
            }

            SyncSelectedInstructionFromEditor();
            SelectedInstruction.LoadDefaultText();

            if (InstructionEditorTextBox != null && !string.Equals(InstructionEditorTextBox.Text ?? string.Empty, SelectedInstruction.Text ?? string.Empty, StringComparison.Ordinal))
            {
                InstructionEditorTextBox.Text = SelectedInstruction.Text ?? string.Empty;
            }

            OnPropertyChanged(nameof(SelectedInstructionText));
            OnPropertyChanged(nameof(IsSelectedInstructionUsingDefaultText));
            OnPropertyChanged(nameof(SelectedInstructionDefaultStateText));
            SaveInstructions(SelectedInstruction.Text);
        }

        private void SyncSelectedInstructionFromEditor()
        {
            if (SelectedInstruction == null || InstructionEditorTextBox == null)
            {
                return;
            }

            string editorText = InstructionEditorTextBox.Text ?? string.Empty;
            if (!string.Equals(SelectedInstruction.Text ?? string.Empty, editorText, StringComparison.Ordinal))
            {
                SelectedInstruction.Text = editorText;
            }
        }

        public IEnumerable<InstructionEntry> InstructionEntries => InstructionManager.Instance.Entries;

        /// <summary>
        /// Persists edited instruction text for the currently selected instruction key.
        /// </summary>
        public void SaveInstructions(string newText)
        {
            if (SelectedInstruction == null || !SelectedInstruction.IsUserAdjustable)
            {
                return;
            }

            var key = SelectedInstruction.Key;
            newText = NormalizeInstructionTextForKey(key, newText);

            try
            {
                InstructionManager.Instance.UpdateEntryText(key, newText);
                AIThreadManager.Instance.AgentsPropertysChanged = true;
            }
            catch
            {
                // do not surface write errors in UI
            }
        }

        /// <summary>
        /// Commits instruction edits on focus loss and updates derived UI state labels.
        /// </summary>
        private void InstructionTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                var binding = textBox.GetBindingExpression(TextBox.TextProperty);
                binding?.UpdateSource();

                SaveInstructions(textBox.Text);
                OnPropertyChanged(nameof(SelectedInstructionText));
                OnPropertyChanged(nameof(IsSelectedInstructionUsingDefaultText));
                OnPropertyChanged(nameof(IsSelectedInstructionOverLengthLimit));
                OnPropertyChanged(nameof(SelectedInstructionDefaultStateText));
            }
        }

        /// <summary>
        /// Updates length-related instruction UI hints while typing.
        /// </summary>
        private void InstructionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OnPropertyChanged(nameof(IsSelectedInstructionOverLengthLimit));
            OnPropertyChanged(nameof(SelectedInstructionDefaultStateText));
        }

        public string OrgId
        {
            get => _orgId;
            set
            {
                if (_orgId == value)
                {
                    return;
                }

                _orgId = value;
                OnPropertyChanged();
                Properties.Settings.Default.orgId = _orgId;
                Properties.Settings.Default.Save();
            }
        }

        public string APIKey
        {
            get => _apiKey;
            set
            {
                if (_apiKey == value)
                {
                    return;
                }

                _apiKey = value ?? string.Empty;
                OnPropertyChanged();
                ApiKeyBox.Password = _apiKey;
                SaveEncryptedApiKey(_apiKey);
            }
        }

        private void ApiKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var pwd = ((PasswordBox)sender).Password ?? string.Empty;
            APIKey = pwd;
            SaveEncryptedApiKey(pwd);
        }

        /// <summary>
        /// Encrypts and persists API key material in application settings.
        /// </summary>
        private void SaveEncryptedApiKey(string plainText)
        {
            var encrypted = SecretProtector.EncryptToBase64(plainText ?? string.Empty);
            Properties.Settings.Default.apiKey = encrypted;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Stores the local provider's API key separately from the OpenAI key,
        /// protected with Windows DPAPI for the current user.
        /// </summary>
        public string LocalAPIKey
        {
            get => _localApiKey;
            set
            {
                string normalized = value ?? string.Empty;
                if (_localApiKey == normalized)
                {
                    return;
                }

                string encrypted = SecretProtector.EncryptToBase64(normalized);
                _localApiKey = normalized;
                LocalApiKeyBox.Password = normalized;
                Settings.Default.localApiKey = encrypted;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        private void LocalApiKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            LocalAPIKey = ((PasswordBox)sender).Password;
        }

        public SupportedProviders SelectedProviderEnum
        {
            get => ProviderToStringConverter.Convert(Properties.Settings.Default.selectedProvider);
            set
            {
                if (value == SelectedProviderEnum)
                {
                    return;
                }

                Properties.Settings.Default.selectedProvider = ProviderToStringConverter.Convert(value);
                Properties.Settings.Default.Save();

                OnPropertyChanged(nameof(SelectedProviderEnum));
                OnPropertyChanged(nameof(SelectedProvider));
                OnPropertyChanged(nameof(IsOpenAiSelected));
                OnPropertyChanged(nameof(IsLocalSelected));
                OnPropertyChanged(nameof(CurrentModelIdsList));
                OnPropertyChanged(nameof(ContextWindowModelIds));
                EnsureSelectedContextWindowModelId();
                NotifyContextWindowStateChanged();
            }
        }

        public string SelectedProvider
        {
            get => ProviderToStringConverter.Convert(SelectedProviderEnum);
            set
            {
                var parsed = ProviderToStringConverter.Convert(value);
                if (parsed == SelectedProviderEnum)
                {
                    return;
                }

                SelectedProviderEnum = parsed;
            }
        }

        public bool IsOpenAiSelected => SelectedProviderEnum == SupportedProviders.OpenAI;
        public bool IsLocalSelected => SelectedProviderEnum == SupportedProviders.Local;

        public string ActiveModelId
        {
            get => Properties.Settings.Default.activeModelId;
            set
            {
                var normalized = value?.Trim();
                if (normalized == Properties.Settings.Default.activeModelId)
                {
                    return;
                }

                Properties.Settings.Default.activeModelId = normalized;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
                if (string.IsNullOrWhiteSpace(_selectedContextWindowModelId))
                {
                    _selectedContextWindowModelId = normalized;
                }
                NotifyContextWindowStateChanged();
            }
        }

        public string[] ContextWindowModelIds => CurrentModelIdsList
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        public string SelectedContextWindowModelId
        {
            get
            {
                EnsureSelectedContextWindowModelId();
                return _selectedContextWindowModelId;
            }
            set
            {
                string normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
                if (string.Equals(_selectedContextWindowModelId, normalized, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                _selectedContextWindowModelId = normalized;
                NotifyContextWindowStateChanged();
            }
        }

        public string SelectedModelContextWindowText
        {
            get
            {
                string modelId = SelectedContextWindowModelId;
                if (string.IsNullOrWhiteSpace(modelId))
                {
                    return string.Empty;
                }

                if (ModelContextWindowResolver.TryGetUserConfiguredContextWindowTokens(modelId, out int userTokens))
                {
                    return userTokens.ToString(CultureInfo.InvariantCulture);
                }

                if (ModelContextWindowResolver.TryGetDeveloperContextWindowTokens(modelId, out int developerTokens))
                {
                    return developerTokens.ToString(CultureInfo.InvariantCulture);
                }

                return string.Empty;
            }
            set
            {
                string modelId = SelectedContextWindowModelId;
                if (string.IsNullOrWhiteSpace(modelId) || !IsSelectedModelContextWindowEditable)
                {
                    return;
                }

                string normalized = (value ?? string.Empty).Trim();
                if (!int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedTokens))
                {
                    parsedTokens = MinimumModelContextWindowTokens;
                }

                parsedTokens = Math.Max(MinimumModelContextWindowTokens, parsedTokens);

                bool hasUserOverride = ModelContextWindowResolver.TryGetUserConfiguredContextWindowTokens(modelId, out int existingUserTokens);
                if (hasUserOverride && existingUserTokens == parsedTokens)
                {
                    OnPropertyChanged(nameof(SelectedModelContextWindowText));
                    return;
                }

                if (!hasUserOverride &&
                    ModelContextWindowResolver.TryGetDeveloperContextWindowTokens(modelId, out int existingDeveloperTokens) &&
                    existingDeveloperTokens == parsedTokens)
                {
                    OnPropertyChanged(nameof(SelectedModelContextWindowText));
                    return;
                }

                ModelContextWindowResolver.SetUserConfiguredContextWindowTokens(modelId, parsedTokens);
                NotifyContextWindowStateChanged();
                AIThreadManager.Instance.AgentsPropertysChanged = true;
            }
        }

        public bool IsSelectedModelContextWindowEditable
        {
            get
            {
                string modelId = SelectedContextWindowModelId;
                return !string.IsNullOrWhiteSpace(modelId);
            }
        }

        public string SelectedModelContextWindowHint
        {
            get
            {
                string modelId = SelectedContextWindowModelId;
                if (string.IsNullOrWhiteSpace(modelId))
                {
                    return BuildModelContextWindowMinimumHint();
                }

                bool hasDeveloperDefault = ModelContextWindowResolver.TryGetDeveloperContextWindowTokens(modelId, out int developerTokens);
                bool hasUserOverride = ModelContextWindowResolver.TryGetUserConfiguredContextWindowTokens(modelId, out int userTokens);

                if (hasDeveloperDefault && hasUserOverride)
                {
                    string baseHint = string.Format(
                        CultureInfo.InvariantCulture,
                        GetResourceOrFallback("ModelContextWindowOverrideHint", "Developer default: {0}. User override active: {1}."),
                        developerTokens,
                        userTokens);
                    return AppendModelContextWindowMinimumHint(baseHint);
                }

                if (hasDeveloperDefault)
                {
                    string baseHint = string.Format(
                        CultureInfo.InvariantCulture,
                        GetResourceOrFallback("ModelContextWindowCanOverrideHint", "Developer default: {0}. You can override it."),
                        developerTokens);
                    return AppendModelContextWindowMinimumHint(baseHint);
                }

                return BuildModelContextWindowMinimumHint();
            }
        }

        /// <summary>
        /// Builds the localized minimum-token hint for context-window input.
        /// </summary>
        private string BuildModelContextWindowMinimumHint()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                GetResourceOrFallback("ModelContextWindowMinTooltip", "Minimum: {0} tokens."),
                MinimumModelContextWindowTokens);
        }

        /// <summary>
        /// Appends the minimum-token constraint hint to a base context-window hint text.
        /// </summary>
        private string AppendModelContextWindowMinimumHint(string baseHint)
        {
            string minimumHint = BuildModelContextWindowMinimumHint();
            if (string.IsNullOrWhiteSpace(baseHint))
            {
                return minimumHint;
            }

            return $"{baseHint} {minimumHint}";
        }

        /// <summary>
        /// Normalizes instruction text before persistence while preserving intentional long-form content.
        /// </summary>
        private static string NormalizeInstructionTextForKey(string instructionKey, string newText)
        {
            string normalized = string.IsNullOrWhiteSpace(newText) ? string.Empty : newText.Trim();
            if (!string.Equals(instructionKey, SmallContextInstructionKey, StringComparison.OrdinalIgnoreCase))
            {
                return normalized;
            }

            // Keep full text for editing reproducibility; truncation is applied during request composition.
            return normalized;
        }

        private int GetSelectedInstructionLengthForUi()
        {
            if (!string.Equals(SelectedInstruction?.Key, SmallContextInstructionKey, StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            if (InstructionEditorTextBox != null)
            {
                return (InstructionEditorTextBox.Text ?? string.Empty).Length;
            }

            return (SelectedInstruction?.Text ?? string.Empty).Length;
        }

        public bool AllowAiToolMutations
        {
            get => Settings.Default.allowAiToolMutations;
            set
            {
                if (Settings.Default.allowAiToolMutations == value)
                {
                    return;
                }

                Settings.Default.allowAiToolMutations = value;
                Settings.Default.Save();
                OnPropertyChanged();

                AIThreadManager.Instance.AgentsPropertysChanged = true;
            }
        }

        public string OpenAiModelIds
        {
            get => Properties.Settings.Default.openAiModelIds;
            set
            {
                var normalized = NormalizeModelIdList(value);
                if (normalized == Properties.Settings.Default.openAiModelIds)
                {
                    return;
                }

                Properties.Settings.Default.openAiModelIds = normalized;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
                OnPropertyChanged(nameof(OpenAiModelIdsList));
                OnPropertyChanged(nameof(ContextWindowModelIds));
                EnsureSelectedContextWindowModelId();
                NotifyContextWindowStateChanged();

                UpdateActiveModelAfterListChange(
                    models: OpenAiModelIdsList,
                    currentActive: ActiveModelId,
                    setActive: id => ActiveModelId = id);
            }
        }

        public string LocalModelIds
        {
            get => Properties.Settings.Default.localModelIds;
            set
            {
                var normalized = NormalizeModelIdList(value);
                if (normalized == Properties.Settings.Default.localModelIds)
                {
                    return;
                }

                Properties.Settings.Default.localModelIds = normalized;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
                OnPropertyChanged(nameof(LocalModelIdsList));
                OnPropertyChanged(nameof(ContextWindowModelIds));
                EnsureSelectedContextWindowModelId();
                NotifyContextWindowStateChanged();

                UpdateActiveModelAfterListChange(
                    models: LocalModelIdsList,
                    currentActive: ActiveModelId,
                    setActive: id => ActiveModelId = id);
            }
        }

        public string LocalModelEndpoint
        {
            get => Properties.Settings.Default.localModelEndpoint;
            set
            {
                var normalized = value?.Trim();
                if (normalized == Properties.Settings.Default.localModelEndpoint)
                {
                    return;
                }

                Properties.Settings.Default.localModelEndpoint = normalized;
                Properties.Settings.Default.Save();
                OnPropertyChanged(nameof(LocalModelEndpoint));
            }
        }

        public string[] OpenAiModelIdsList => SplitModels(OpenAiModelIds);
        public string[] LocalModelIdsList => SplitModels(LocalModelIds);

        public string[] CurrentModelIdsList => IsLocalSelected ? LocalModelIdsList : OpenAiModelIdsList;

        /// <summary>
        /// Normalizes model-id input by splitting mixed delimiters, trimming, deduplicating, and rejoining by line.
        /// </summary>
        private static string NormalizeModelIdList(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            var parts = raw
                .Replace(";", "\n")
                .Replace(",", "\n")
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => p.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return string.Join(Environment.NewLine, parts);
        }

        /// <summary>
        /// Splits normalized model-id text into an ordered non-empty list.
        /// </summary>
        private static string[] SplitModels(string models)
        {
            if (string.IsNullOrWhiteSpace(models))
            {
                return Array.Empty<string>();
            }

            return models
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(m => m.Trim())
                .Where(m => m.Length > 0)
                .ToArray();
        }

        /// <summary>
        /// Ensures the active model remains valid after model-list edits.
        /// </summary>
        private static void UpdateActiveModelAfterListChange(
            string[] models,
            string currentActive,
            Action<string> setActive)
        {
            if (models == null || models.Length == 0)
            {
                if (!string.IsNullOrEmpty(currentActive))
                {
                    setActive(null);
                }

                return;
            }

            bool stillExists = !string.IsNullOrEmpty(currentActive) &&
                               models.Any(m => string.Equals(m, currentActive, StringComparison.OrdinalIgnoreCase));

            if (!stillExists)
            {
                setActive(models[0]);
            }
        }

        /// <summary>
        /// Recreates the active agent with current instruction/settings state.
        /// </summary>
        private void ReloadAgentButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var modelId = ActiveModelId;
                if (string.IsNullOrWhiteSpace(modelId))
                {
                    MessageBox.Show(Properties.Resources.AiChatNoActiveModel, Properties.Resources.AiChatDialogTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                AIThreadManager.Instance.ReloadAgent(modelId);
                MessageBox.Show(Properties.Resources.AiChatAgentReloaded, Properties.Resources.AiChatDialogTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(CultureInfo.CurrentCulture, Properties.Resources.AiChatAgentReloadError, ex.Message), Properties.Resources.AiChatDialogTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Deletes all persisted chat threads after explicit user confirmation.
        /// </summary>
        private void DeleteAllChatsButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                Properties.Resources.AiChatDeleteAllConfirmation,
                Properties.Resources.AiChatDialogTitle,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            AIThreadManager.Instance.ClearAllThreads();
        }

        /// <summary>
        /// Raises all dependent property notifications related to context-window UI state.
        /// </summary>
        private void NotifyContextWindowStateChanged()
        {
            OnPropertyChanged(nameof(SelectedContextWindowModelId));
            OnPropertyChanged(nameof(SelectedModelContextWindowText));
            OnPropertyChanged(nameof(IsSelectedModelContextWindowEditable));
            OnPropertyChanged(nameof(SelectedModelContextWindowHint));
        }

        /// <summary>
        /// Ensures a valid model id is selected for context-window override editing.
        /// </summary>
        private void EnsureSelectedContextWindowModelId()
        {
            string[] ids = ContextWindowModelIds;
            if (ids.Length == 0)
            {
                _selectedContextWindowModelId = null;
                return;
            }

            bool exists = !string.IsNullOrWhiteSpace(_selectedContextWindowModelId) &&
                          ids.Any(id => string.Equals(id, _selectedContextWindowModelId, StringComparison.OrdinalIgnoreCase));
            if (exists)
            {
                return;
            }

            string active = (ActiveModelId ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(active) && ids.Any(id => string.Equals(id, active, StringComparison.OrdinalIgnoreCase)))
            {
                _selectedContextWindowModelId = active;
                return;
            }

            _selectedContextWindowModelId = ids[0];
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string prop = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
