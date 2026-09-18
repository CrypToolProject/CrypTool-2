/* Copyright 2026 CrypTool Project. Licensed under the Apache License, Version 2.0. */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using CrypTool.CrypLLM.Ports;
using CrypTool.CrypLLM.Services;
using Newtonsoft.Json.Linq;
using WorkspaceManager.Model;
using WorkspaceManager.Model.Tools;
using WorkspaceManager.View.Visuals;
using WorkspaceManagerModel.Model.Operations;

namespace CrypTool.CrypLLM.Threads
{
    /// <summary>Own only tool-dispatched native operations, separately for each touched workspace.</summary>
    internal sealed class AgentEditSession : IDisposable
    {
        private static readonly AsyncLocal<AgentEditSession> current = new AsyncLocal<AgentEditSession>();
        internal static AgentEditSession Current => current.Value;
        internal object Owner { get; } = new object();
        private readonly AgentEditSession previous;
        private readonly AIThread thread;
        private readonly Dictionary<WorkspaceModel, Entry> entries = new Dictionary<WorkspaceModel, Entry>();
        private sealed class Entry { public string TabId, Title; public int Revision; public bool Changed; }
        internal AgentEditSession(AIThread thread) { this.thread = thread; previous = current.Value; current.Value = this; }
        internal static IDisposable EnterUi(AgentEditSession session)
        {
            AgentEditSession previous = current.Value;
            current.Value = session;
            return new Scope(() => current.Value = previous);
        }
        private sealed class Scope : IDisposable
        {
            private Action action;
            internal Scope(Action action) { this.action = action; }
            public void Dispose() { Action restore = action; action = null; restore?.Invoke(); }
        }
        internal void Register(WorkspaceModel model, string tabId)
        {
            if (entries.ContainsKey(model)) return;
            OpenTabsAbstraction tab = CrypWinPort.Instance?.GetOpenTabs()?.FirstOrDefault(t => t.Id == tabId || (string.IsNullOrEmpty(tabId) && t.IsActive));
            entries[model] = new Entry { TabId = tab?.Id ?? tabId, Title = tab?.Title ?? "Workspace", Revision = model.UndoRedoManager.Revision };
        }
        internal void BeforeTool()
        {
            foreach (var entry in entries) entry.Value.Revision = entry.Key.UndoRedoManager.Revision;
        }
        internal T AfterTool<T>(T result)
        {
            var changed = entries.Where(e => e.Key.UndoRedoManager.Revision != e.Value.Revision).ToList();
            foreach (var entry in changed) entry.Value.Changed = true;
            if (result is string json && changed.Count > 0)
            {
                try
                {
                    var envelope = JObject.Parse(json);
                    envelope["automaticLayoutCheck"] = JArray.FromObject(changed.Select(e => new { tabId = e.Value.TabId, report = WorkspaceLayoutService.Check(e.Key) }));
                    return (T)(object)envelope.ToString(Newtonsoft.Json.Formatting.None);
                }
                catch (Exception ex) { Log.Debug("Automatic layout report unavailable: " + ex.Message); }
            }
            return result;
        }
        internal void Complete()
        {
            foreach (var entry in entries)
            {
                Operation group = entry.Key.UndoRedoManager.GroupOwnedOperations(Owner);
                if (!entry.Value.Changed && group == null) continue;
                LayoutReport report = null;
                try { report = WorkspaceLayoutService.Check(entry.Key); }
                catch (Exception ex) { Log.Debug("Final layout check unavailable: " + ex.Message); }
                thread.AgentWorkspaceChanges.Add(new AgentWorkspaceChange(entry.Key, group, entry.Value.TabId, entry.Value.Title, report));
            }
            // Old native stacks can hold many operations; keep the chat panel's runtime handles bounded.
            while (thread.AgentWorkspaceChanges.Count > 40) thread.AgentWorkspaceChanges.RemoveAt(0);
        }
        public void Dispose() { current.Value = previous; }
    }

    /// <summary>Runtime-only undo handle. Exact stack and tab identity protect subsequent manual edits.</summary>
    public sealed class AgentWorkspaceChange : INotifyPropertyChanged
    {
        private readonly WorkspaceModel model;
        private readonly Operation group;
        private readonly string tabId;
        private readonly LayoutReport report;
        private bool undone;
        public string Title { get; }
        internal AgentWorkspaceChange(WorkspaceModel model, Operation group, string tabId, string title, LayoutReport report)
        {
            this.model = model; this.group = group; this.tabId = tabId; this.report = report; Title = title;
            WeakEventManager<UndoRedoManager, EventArgs>.AddHandler(model.UndoRedoManager, "HistoryChanged", OnHistoryChanged);
        }
        private bool SameWorkspace => !string.IsNullOrEmpty(tabId) && CrypWinPort.Instance != null &&
            CrypWinPort.Instance.TryGetWorkspaceEditorByTabId(tabId, out var editor) && editor.Presentation is EditorVisual visual && ReferenceEquals(visual.Model, model);
        public bool CanUndo => group != null && SameWorkspace && model.UndoRedoManager.CanUndo(group);
        public string UndoLabel => Text("AiChatUndoAgentChanges");
        public string Status => undone && !CanUndo ? Text("AiChatAgentChangesUndone") : !CanUndo ? Text("AiChatAgentUndoUnavailable") :
            string.Format(Text("AiChatAgentChangesCount"), (group as GroupedOperation)?.OperationCount ?? 1);
        public string LayoutStatus => report == null ? Text("AiChatLayoutUnavailable") :
            string.Format(Text(report.complete ? "AiChatLayoutSummary" : "AiChatLayoutPartialSummary"), report.errorCount, report.warningCount);
        private static string Text(string key) => Properties.Resources.ResourceManager.GetString(key) ?? key;
        internal bool Undo()
        {
            if (!CanUndo) return false;
            // Native editing must not race a running execution engine.
            if (CrypWinPort.Instance.TryGetWorkspaceEditorByTabId(tabId, out var editor) && editor.CanStop) editor.Stop();
            if (!model.UndoRedoManager.TryUndo(group)) return false;
            undone = true;
            Notify();
            return true;
        }
        private void OnHistoryChanged(object sender, EventArgs args) => Notify();
        private void Notify()
        {
            if (Application.Current?.Dispatcher is System.Windows.Threading.Dispatcher dispatcher && !dispatcher.CheckAccess())
            { dispatcher.BeginInvoke(new Action(Notify)); return; }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanUndo)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
