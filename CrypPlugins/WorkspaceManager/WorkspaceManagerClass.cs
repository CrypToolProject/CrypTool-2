/*                              
   Copyright 2010 Nils Kopal, Viktor M.

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

using System;
using System.Collections.Generic;
using System.Linq;
using CrypTool.Core;
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.Editor;
using CrypTool.PluginBase;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;
using WorkspaceManager.Model;
using WorkspaceManager.Execution;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using CrypTool.PluginBase.Miscellaneous;
using System.Windows.Media.Imaging;
using System.Printing;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Collections.ObjectModel;
using WorkspaceManager.Properties;
using WorkspaceManager.View.Visuals;
using WorkspaceManager.View.Base;
using WorkspaceManagerModel.Model.Operations;
using WorkspaceManager.View.VisualComponents.CryptoLineView;
using System.Globalization;

//Disable warnings for unused or unassigned fields and events:
#pragma warning disable 0169, 0414, 0067

namespace WorkspaceManager
{
    /// <summary>
    /// Workspace Manager - PluginEditor based on MVC Pattern
    /// </summary>
    [TabColor("LightSlateGray")]
    [EditorInfo("cwm", true, false, true, true, true, true, false, true, true )]
    [Author("Viktor Matkovic,Nils Kopal", "nils.kopal@CrypTool.org", "Universität Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("WorkspaceManager.Properties.Resources", "PluginCaption", "PluginTooltip", "WorkspaceManager/DetailedDescription/doc.xml", "WorkspaceManager/View/Images/WorkspaceManagerIcon.ico")]
    public class WorkspaceManagerClass : IEditor
    {
        public event FileLoadedHandler OnFileLoaded;
        public event EventHandler<LoadingErrorEventArgs> LoadingErrorOccurred;
        public event EventHandler PasteOccured;

        /// <summary>
        /// Create a new Instance of the Editor
        /// </summary>
        public WorkspaceManagerClass() : this(new WorkspaceModel()){}
        public WorkspaceManagerClass(WorkspaceModel workspaceModel)
        {
            this.SelectedPluginsList = new ObservableCollection<ComponentVisual>();
            Settings = new WorkspaceManagerSettings(this);
            WorkspaceModel = workspaceModel;
            WorkspaceModel.OnGuiLogNotificationOccured += this.GuiLogNotificationOccured;
            WorkspaceModel.MyEditor = this;
            WorkspaceSpaceEditorView = new EditorVisual(WorkspaceModel);
            WorkspaceSpaceEditorView.ItemsSelected += new EventHandler<SelectedItemsEventArgs>(WorkspaceSpaceEditorView_ItemsSelected);
        }

        void WorkspaceSpaceEditorView_ItemsSelected(object sender, SelectedItemsEventArgs e)
        {
            if(e.Items != null)
                this.SelectedPluginsList = new ObservableCollection<ComponentVisual>(e.Items.OfType<ComponentVisual>());
            else
                this.SelectedPluginsList = new ObservableCollection<ComponentVisual>();
        }

        private void OnSampleLoaded(string filename)
        {
            //We initialize the settings after all other loading procedures have been finsihed, thus, settings, that are set
            //to invisible, are not shown in the settings bar
            try
            {
                WorkspaceModel.InitializeSettings();
            }
            catch(Exception ex)
            {
                GuiLogMessage(String.Format("Error during initialization of settings: {0}", ex.Message), NotificationLevel.Error);
            }

            if (OnFileLoaded != null)
                OnFileLoaded.Invoke(this, filename);
        }

        #region private Members

        private WorkspaceModel WorkspaceModel = null;
        private EditorVisual WorkspaceSpaceEditorView = null;
        public ExecutionEngine ExecutionEngine = null;
        private volatile bool executing = false;
        private volatile bool stopping = false;
        private static CopyOperation copy;

        private DateTime _starttime;
        private bool _reachedTotalProgress = false;

        private CultureInfo _currentCulture;
        private CultureInfo _currentUICulture;

        #endregion

        /// <summary>
        /// Is this Editor executing?
        /// </summary>
        public bool isExecuting()
        {
            return executing;
        }

        public event EventHandler executeEvent;     //Event for BinSettingsVisual to notice when executing, to disable settings that may not be changed during execution       
        
        #region IEditor Members

        /// <summary>
        /// 
        /// </summary>
        public event SelectedPluginChangedHandler OnSelectedPluginChanged;

        /// <summary>
        /// 
        /// </summary>
        public event ProjectTitleChangedHandler OnProjectTitleChanged;

        /// <summary>
        /// 
        /// </summary>
        public event OpenProjectFileHandler OnOpenProjectFile;

        /// <summary>
        /// Current filename
        /// </summary>
        public string CurrentFile { private set; get; }

        /// <summary>
        /// Called by clicking on the new button of CrypTool
        /// Creates a new Model
        /// </summary>
        public void New()
        {
            CurrentFile = null; 
            if (this.OnProjectTitleChanged != null)
            {
                this.OnProjectTitleChanged.Invoke(this, typeof(WorkspaceManagerClass).GetPluginStringResource("unnamed_project"));
            }
            WorkspaceModel.DeleteAllModelElements();
            WorkspaceModel.UndoRedoManager.ClearStacks();
            WorkspaceModel.UpdateableView = this.WorkspaceSpaceEditorView;
            WorkspaceModel.MyEditor = this;            
            this.SelectedPluginsList.Clear();
        }

        /// <summary>
        /// Open the given model in the editor
        /// </summary>
        /// <param name="fileName"></param>
        public void Open(WorkspaceModel model)
        {
            try
            {
                WorkspaceModel = model;
                WorkspaceModel.OnGuiLogNotificationOccured += this.GuiLogNotificationOccured;
                var dispatcherOp = WorkspaceSpaceEditorView.Load(WorkspaceModel);
                HandleTemplateLoadingDispatcher(dispatcherOp, null);
                WorkspaceModel.UpdateableView = this.WorkspaceSpaceEditorView;
                WorkspaceModel.MyEditor = this;
                WorkspaceModel.UndoRedoManager.ClearStacks();
            }
            catch (Exception ex)
            {
                GuiLogMessage("Could not open Model:" + ex.ToString(), NotificationLevel.Error);
            }
        }

        private void HandleTemplateLoadingDispatcher(DispatcherOperation dispatcherOp, string filename)
        {
            dispatcherOp.Completed += delegate { OnSampleLoaded(filename); };
            if (dispatcherOp.Status == DispatcherOperationStatus.Completed)
            {
                OnSampleLoaded(filename);
            }
        }

        /// <summary>
        /// Called by clicking on the open button of CrypTool
        /// loads a serialized model
        /// </summary>
        /// <param name="fileName"></param>
        public void Open(string fileName)
        {
            try
            {
                New();
                CurrentFile = fileName;
                GuiLogMessage(String.Format(Resources.WorkspaceManagerClass_Open_Loading_Model___0_, fileName), NotificationLevel.Info);
                var persistance = new ModelPersistance();
                persistance.OnGuiLogNotificationOccured += OnGuiLogNotificationOccured;
                WorkspaceModel = persistance.loadModel(fileName, CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_TemplateReplacement);
                WorkspaceModel.OnGuiLogNotificationOccured += this.GuiLogNotificationOccured;
                var dispatcherOp = WorkspaceSpaceEditorView.Load(WorkspaceModel);
                HandleTemplateLoadingDispatcher(dispatcherOp, fileName);
                WorkspaceModel.UpdateableView = this.WorkspaceSpaceEditorView;
                this.OnProjectTitleChanged.Invoke(this, System.IO.Path.GetFileName(fileName));
                WorkspaceModel.MyEditor = this;                
                WorkspaceModel.UndoRedoManager.ClearStacks();
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
                GuiLogMessage(String.Format(Resources.WorkspaceManagerClass_Open_Could_not_load_Model___0_, s), NotificationLevel.Error);
                if (LoadingErrorOccurred != null)
                    LoadingErrorOccurred.Invoke(this, new LoadingErrorEventArgs() { Message = s });
            }
        }

        /// <summary>
        /// Called by clicking on the save button of CrypTool
        /// serializes the current model
        /// </summary>
        /// <param name="fileName"></param>
        public void Save(string fileName)
        {
            try
            {
                GuiLogMessage(String.Format(Resources.WorkspaceManagerClass_Save_Saving_Model___0_, fileName), NotificationLevel.Info);
                var persistance = new ModelPersistance();
                persistance.OnGuiLogNotificationOccured += OnGuiLogNotificationOccured;
                persistance.saveModel(this.WorkspaceModel, fileName);
                CurrentFile = fileName;
                this.OnProjectTitleChanged.Invoke(this, System.IO.Path.GetFileName(fileName));
            }
            catch (Exception ex)
            {
                GuiLogMessage(String.Format(Resources.WorkspaceManagerClass_Save_Could_not_save_Model___0_, ex.ToString()), NotificationLevel.Error);
            }

        }

        /// <summary>
        /// Called by double clicking on a plugin symbol of CrypTool
        /// Adds a new PluginModel wrapping an instance of the selected plugin
        /// </summary>
        /// <param name="type"></param>
        public void Add(Type type)
        {
            if (!executing)
            {
                var pluginModel = (PluginModel)WorkspaceSpaceEditorView.Model.ModifyModel(new NewPluginModelOperation(new Point(0, 0), 0, 0, type));
                WorkspaceSpaceEditorView.AddComponentVisual(pluginModel);
             
            }
        }

        /// <summary>
        /// Undo changes
        /// </summary>
        public void Undo()
        {
            if (WorkspaceModel.UndoRedoManager != null && WorkspaceModel.UndoRedoManager.CanUndo())
            {
                try
                {
                    WorkspaceModel.UndoRedoManager.Undo();
                }
                catch (Exception ex)
                {
                    GuiLogMessage(String.Format(Resources.WorkspaceManagerClass_Undo_Can_not_undo___0_, ex.Message), NotificationLevel.Error);
                }
            }
        }

        /// <summary>
        /// Redo changes
        /// </summary>
        public void Redo()
        {
            if (WorkspaceModel.UndoRedoManager != null && WorkspaceModel.UndoRedoManager.CanRedo())
            {
                try
                {
                    WorkspaceModel.UndoRedoManager.Redo();
                }
                catch (Exception ex)
                {
                    GuiLogMessage(String.Format(Resources.WorkspaceManagerClass_Redo_Can_not_redo___0_, ex.Message), NotificationLevel.Error);
                }
            }
        }

        private void doCopy()
        {
            List<VisualElementModel> elementsToCopy = new List<VisualElementModel>();
            if(WorkspaceSpaceEditorView.SelectedItems != null)
            {
                var filter = System.Linq.Enumerable.OfType<ComponentVisual>(WorkspaceSpaceEditorView.SelectedItems);
                var list = filter.Select(visual => visual.Model).OfType<VisualElementModel>().ToList<VisualElementModel>();
                elementsToCopy = elementsToCopy.Concat(CopyOperation.SelectConnections(list)).ToList<VisualElementModel>();
                //elementsToCopy = elementsToCopy.Concat(list).ToList<VisualElementModel>();
                    //Nils Kopal 28.08.2013: I removed this line because it lead to the following bug:
                    // Copy an component: You had 2! components in the list
                    // Paste the component - only one component appears on workspace
                    // save + load the workspace
                    // delete or move the "copied" component
                    // a "ghost" component appears at the same place, the copy was
                    // => workspace was corrupt
            }
            if (WorkspaceSpaceEditorView.SelectedText != null)
                elementsToCopy.Add(WorkspaceSpaceEditorView.SelectedText.Model);
            if (WorkspaceSpaceEditorView.SelectedImage != null)
                elementsToCopy.Add(WorkspaceSpaceEditorView.SelectedImage.Model);

            copy = new CopyOperation(new SerializationWrapper() { elements = elementsToCopy });
        }

        public void Cut()
        {
            if (WorkspaceSpaceEditorView.SelectedItems != null && !WorkspaceSpaceEditorView.IsFullscreenOpen)
            {
                doCopy();
                Remove();
            }
        }

        public void Copy()
        {
            if (!WorkspaceSpaceEditorView.IsFullscreenOpen)
            {
                doCopy();
            }
        }
        
        public IList<PluginModel> CurrentCopies = new List<PluginModel>();
        public void Paste()
        {
            if (copy == null || WorkspaceSpaceEditorView.IsFullscreenOpen || isExecuting())
            {
                return;
            }
            WorkspaceModel.ModifyModel(copy, true);
            CurrentCopies = copy.GetCopiedModelElements().OfType<PluginModel>().ToList();
            if (PasteOccured != null)
            {
                PasteOccured.Invoke(this, null);
            }
            doCopy();
        }

        public void Remove()
        {
            var editor = (EditorVisual)Presentation;
            if (editor.Model != null && !isExecuting() && editor.SelectedItems != null)
            {
                var deleteOperationsList = new List<Operation>();
                var connections = new List<ConnectionModel>();
                var images = new List<ImageModel>();
                var texts = new List<TextModel>();
                var plugins = new List<PluginModel>();

                foreach (var item in editor.SelectedItems)
                {                    
                    if (item is ComponentVisual)
                    {
                        plugins.Add(((PluginModel)((ComponentVisual)item).Model));
                        foreach (var connector in ((PluginModel)((ComponentVisual) item).Model).GetOutputConnectors())
                        {
                            foreach (var connectionModel in connector.GetOutputConnections())
                            {
                                connections.Add(connectionModel);
                            }
                        }
                        foreach (var connector in ((PluginModel)((ComponentVisual)item).Model).GetInputConnectors())
                        {
                            foreach (var connectionModel in connector.GetInputConnections())
                            {
                                connections.Add(connectionModel);
                            }
                        }
                        
                    }
                    if (item is CryptoLineView)
                    {
                        if (!connections.Contains(((CryptoLineView)item).Model))
                        {
                            connections.Add(((CryptoLineView)item).Model);
                        }
                    }

                    //if (item is TextVisual)
                    //{
                    //    if (!texts.Contains(((TextVisual)item).Model))
                    //    {
                    //        texts.Add(((TextVisual)item).Model);
                    //    }
                    //}

                    //if (item is ImageVisual)
                    //{
                    //    if (!images.Contains(((ImageVisual)item).Model))
                    //    {
                    //        images.Add(((ImageVisual)item).Model);
                    //    }
                    //}
                }

                foreach (var item in connections)
                {
                    deleteOperationsList.Add(new DeleteConnectionModelOperation(item));
                }

                //foreach (var item in images)
                //{
                //    deleteOperationsList.Add(new DeleteImageModelOperation(item));
                //}

                //foreach (var item in texts)
                //{
                //    deleteOperationsList.Add(new DeleteTextModelOperation(item));
                //}

                foreach (var item in plugins)
                {
                    deleteOperationsList.Add(new DeletePluginModelOperation(item));
                }

                WorkspaceModel.ModifyModel(new MultiOperation(deleteOperationsList));
            }
        }

        public void Print()
        {
            try
            {                
                PrintDialog dialog = new PrintDialog();
                dialog.PageRangeSelection = PageRangeSelection.AllPages;
                dialog.UserPageRangeEnabled = true;

                Nullable<Boolean> print = dialog.ShowDialog();
                if (print == true)
                {
                    this.GuiLogMessage(String.Format(Resources.WorkspaceManagerClass_Print_Printing_document___0___now, this.CurrentFile), NotificationLevel.Info);

                    ((EditorVisual)this.Presentation).FitToScreen();
                    Matrix m = PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformToDevice;
                    double dx = m.M11 * 96;
                    double dy = m.M22 * 96;
                    this.GuiLogMessage("dx=" + dx + " dy=" + dy, NotificationLevel.Debug);
                    const int factor = 4;
                    UIElement control = (UIElement)((EditorVisual)this.Presentation).ScrollViewer.Content;

                    PrintCapabilities capabilities = dialog.PrintQueue.GetPrintCapabilities(dialog.PrintTicket);
                    System.Windows.Size pageSize = new System.Windows.Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight);
                    System.Windows.Size visibleSize = new System.Windows.Size(capabilities.PageImageableArea.ExtentWidth, capabilities.PageImageableArea.ExtentHeight);

                    FixedDocument fixedDoc = new FixedDocument();
                    control.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
                    control.Arrange(new Rect(new System.Windows.Point(0, 0), control.DesiredSize));
                    System.Windows.Size size = control.DesiredSize;

                    RenderTargetBitmap bmp = new RenderTargetBitmap((int)size.Width * factor, (int)size.Height * factor, dx * factor, dy * factor, PixelFormats.Pbgra32);
                    bmp.Render(control);


                    double xOffset = 0;
                    double yOffset = 0;
                    while (xOffset < size.Width)
                    {
                        yOffset = 0;
                        while (yOffset < size.Height)
                        {
                            PageContent pageContent = new PageContent();
                            FixedPage page = new FixedPage();
                            ((IAddChild)pageContent).AddChild(page);
                            fixedDoc.Pages.Add(pageContent);
                            page.Width = pageSize.Width;
                            page.Height = pageSize.Height;
                            int width = (xOffset + visibleSize.Width) > size.Width ? (int)(size.Width - xOffset) : (int)visibleSize.Width;
                            int height = (yOffset + visibleSize.Height) > size.Height ? (int)(size.Height - yOffset) : (int)visibleSize.Height;
                            System.Windows.Controls.Image croppedImage = new System.Windows.Controls.Image();
                            CroppedBitmap cb = new CroppedBitmap(bmp, new Int32Rect((int)xOffset * factor, (int)yOffset * factor, width * factor, height * factor));
                            croppedImage.Source = cb;
                            croppedImage.Width = width;
                            croppedImage.Height = height;
                            page.Children.Add(croppedImage);
                            yOffset += visibleSize.Height;
                        }
                        xOffset += visibleSize.Width;
                    }
                    dialog.PrintDocument(fixedDoc.DocumentPaginator, "WorkspaceManager_" + this.CurrentFile);
                    this.GuiLogMessage(String.Format(Resources.WorkspaceManagerClass_Print_Printed__0__pages_of_document___1__, fixedDoc.DocumentPaginator.PageCount, this.CurrentFile), NotificationLevel.Info);
                }
            }
            catch (Exception ex)
            {
                this.GuiLogMessage(String.Format(Resources.WorkspaceManagerClass_Print_Exception_while_printing___0_, ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Show the Help site
        /// </summary>
        public void ShowHelp()
        {
            try
            {
                if (SelectedPluginsList != null && SelectedPluginsList.Count() != 0)
                {
                    ComponentVisual element = SelectedPluginsList.ElementAt(0);
                    OnlineHelp.InvokeShowDocPage(element.Model.PluginType);
                }
                else
                {
                    OnlineHelp.InvokeShowDocPage(typeof(WorkspaceManagerClass));
                }
            }
            catch (Exception e)
            {
                GuiLogMessage(e.ToString(), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Show the Description of the selected plugin
        /// </summary>
        public void ShowSelectedEntityHelp()
        {

            try
            {
                if (selectedPluginsList.Count > 0)      //This doesn't work!
                {
                    var plugin = selectedPluginsList[0];
                    OnlineHelp.InvokeShowDocPage(plugin.Model.PluginType);
                }
                else
                {
                    ShowHelp();
                }
            }
            catch (Exception e)
            {
                GuiLogMessage(e.ToString(), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Is Undo possible
        /// </summary>
        public bool CanUndo
        {
            get
            {
                if (WorkspaceModel.UndoRedoManager != null)
                {
                    return !this.isExecuting() && WorkspaceModel.UndoRedoManager.CanUndo();
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Is Redo possible?
        /// </summary>
        public bool CanRedo
        {
            get
            {
                if (WorkspaceModel.UndoRedoManager != null)
                {
                    return !this.isExecuting() && WorkspaceModel.UndoRedoManager.CanRedo();
                }
                else
                {
                    return false;
                }
            }
        }

        public bool CanCut
        {
            get { return true; }
        }

        public bool CanCopy
        {
            get { return true; }
        }

        public bool CanPaste
        {
            get { return true; }
        }

        public bool CanRemove
        {
            get { return true; }
        }

        /// <summary>
        /// Can the ExecutionEngine be started?
        /// </summary>
        public bool CanExecute
        {
            get
            {
                return ((EditorVisual)Presentation).IsLoading == true || executing ? false : true;
            }
        }

        /// <summary>
        /// Can the ExecutionEngine be stopped?
        /// </summary>
        public bool CanStop
        {
            get { return executing; }
        }

        /// <summary>
        /// Does this Editor has changes?
        /// </summary>
        public bool HasChanges
        {
            get
            {
                if (!PluginExtension.IsTestMode && this.WorkspaceModel != null)
                {
                    return this.WorkspaceModel.HasChanges;
                }
                return false;
            }
        }

        public bool CanPrint
        {
            get { return true; }
        }

        public bool CanSave
        {
            get { return true; }
        }

        public string SamplesDir
        {
            set { }
        }

        public bool ReadOnly { get; set; }
        public bool HasBeenClosed { get; set; }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// 
        /// </summary>
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        /// <summary>
        /// Settings of this editor
        /// </summary>
        public ISettings Settings
        {
            get;
            set;
        }

        /// <summary>
        /// The Presentation of this editor
        /// </summary>
        public System.Windows.Controls.UserControl Presentation
        {
            get { return WorkspaceSpaceEditorView; }
            set { WorkspaceSpaceEditorView = (EditorVisual)value; }
        }

        /// <summary>
        /// Starts the ExecutionEngine to execute the model
        /// </summary>
        public void Execute()
        {
            _currentCulture = System.Globalization.CultureInfo.CurrentCulture;
            _currentUICulture = System.Globalization.CultureInfo.CurrentUICulture;

            if (executing || stopping)
            {
                return;
            }

            try
            {
                GuiLogMessage(Resources.WorkspaceManagerClass_Execute_Execute_Model_now_, NotificationLevel.Info);
                executing = true;
                executeEvent(this, EventArgs.Empty);
                
                if (((WorkspaceManagerSettings)this.Settings).SynchronousEvents)
                {
                    EventsHelper.AsynchronousProgressChanged = false;
                    EventsHelper.AsynchronousGuiLogMessage = false;
                    EventsHelper.AsynchronousStatusChanged = false;
                }

                //Get the gui Thread
                this.WorkspaceSpaceEditorView.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    this.WorkspaceSpaceEditorView.ResetConnections();
                    this.WorkspaceSpaceEditorView.ResetPlugins(1);
                    this.WorkspaceSpaceEditorView.State = BinEditorState.BUSY;
                }
                , null);

                ExecutionEngine = new ExecutionEngine(this);
                ExecutionEngine.OnGuiLogNotificationOccured += this.GuiLogNotificationOccured;

                try
                {
                    ExecutionEngine.GuiUpdateInterval = int.Parse(((WorkspaceManagerSettings)this.Settings).GuiUpdateInterval);
                    if (ExecutionEngine.GuiUpdateInterval <= 0)
                    {
                        GuiLogMessage(Resources.WorkspaceManagerClass_Execute_, NotificationLevel.Warning);
                        ExecutionEngine.GuiUpdateInterval = 1;
                    }
                }
                catch (Exception ex)
                {
                    GuiLogMessage(String.Format(Resources.WorkspaceManagerClass_Execute_Could_not_set_GuiUpdateInterval___0_, ex.Message), NotificationLevel.Warning);
                    ExecutionEngine.GuiUpdateInterval = 100;
                }

                try
                {
                    ExecutionEngine.SleepTime = int.Parse(((WorkspaceManagerSettings)this.Settings).SleepTime);
                    if (ExecutionEngine.SleepTime < 0)
                    {
                        GuiLogMessage(Resources.WorkspaceManagerClass_Execute_SleepTime, NotificationLevel.Warning);
                        ExecutionEngine.SleepTime = 0;
                    }
                }
                catch (Exception ex)
                {
                    GuiLogMessage(String.Format(Resources.WorkspaceManagerClass_Execute_Could_not_set_SleepTime___0_, ex.Message), NotificationLevel.Warning);
                    ExecutionEngine.GuiUpdateInterval = 0;
                }

                ExecutionEngine.BenchmarkPlugins = ((WorkspaceManagerSettings)this.Settings).BenchmarkPlugins;

                //we only start gui update thread if we are visible (for example, during the execution of the wizard
                //we are not visible, so we need no update of gui elements)
                var updateGuiElements = Presentation.IsVisible;

                ExecutionEngine.OnPluginProgressChanged+=new PluginProgressChangedEventHandler(ExecutionEngine_OnPluginProgressChanged);

                _starttime = DateTime.Now;
                _reachedTotalProgress = false;
                ExecutionEngine.Execute(WorkspaceModel, updateGuiElements);
            }
            catch (Exception ex)
            {
                GuiLogMessage(String.Format(Resources.WorkspaceManagerClass_Execute_Exception_during_the_execution___0_, ex.Message), NotificationLevel.Error);
                executing = false;
                executeEvent(this, EventArgs.Empty);
                if (((WorkspaceManagerSettings)this.Settings).SynchronousEvents)
                {
                    EventsHelper.AsynchronousProgressChanged = true;
                    EventsHelper.AsynchronousGuiLogMessage = true;
                    EventsHelper.AsynchronousStatusChanged = true;
                }
            }
        }

        private void ExecutionEngine_OnPluginProgressChanged(IPlugin sender, PluginProgressEventArgs args)
        {
            Thread.CurrentThread.CurrentCulture = _currentCulture;
            Thread.CurrentThread.CurrentUICulture = _currentUICulture;

            if (CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_UseGlobalProgressbar)
            {
                WorkspaceSpaceEditorView.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    WorkspaceSpaceEditorView.Progress = args.Value;
                }, null);

                if (args.Value == args.Max && _reachedTotalProgress == false)
                {
                    var duration = DateTime.Now - _starttime;
                    string durationString = string.Empty;
                    if (duration.TotalDays > 1)
                    {
                        durationString = duration.ToString(@"dd\.hh\:mm\:ss") + " " + Resources.Days;
                    }
                    else if (duration.TotalHours > 1)
                    {
                        durationString = duration.ToString(@"hh\:mm\:ss") + " " + Resources.Hours;
                    }
                    else if (duration.TotalMinutes > 1)
                    {
                        durationString = duration.ToString(@"mm\:ss") + " " + Resources.Minutes;
                    }
                    else
                    {
                        durationString = duration.Seconds + " " + (duration.Seconds == 1 ? Resources.Second : Resources.Seconds);
                    }

                    WorkspaceSpaceEditorView.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        WorkspaceSpaceEditorView.ProgressDuration = String.Format(Resources.GlobalProgressBar_Description, durationString);
                    }, null);
                    GuiLogMessage(String.Format(Resources.GlobalProgressBar_Description, durationString), NotificationLevel.Info);
                    _reachedTotalProgress = true;
                }
                else if (args.Value < args.Max && _reachedTotalProgress == true)
                {
                    //progress fell down below MAX -> we have a new execution run
                    _reachedTotalProgress = false;
                    _starttime = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Stop the ExecutionEngine
        /// </summary>
        public void Stop()
        {
            if (!executing)
            {
                return;
            }

            var stopThread = new Thread(new ThreadStart(waitingStop));
            stopThread.CurrentCulture = _currentCulture;
            stopThread.CurrentUICulture = _currentUICulture;
            stopThread.Start();

            if (((WorkspaceManagerSettings)this.Settings).SynchronousEvents)
            {
                EventsHelper.AsynchronousProgressChanged = true;
                EventsHelper.AsynchronousGuiLogMessage = true;
                EventsHelper.AsynchronousStatusChanged = true;
            }

            this.WorkspaceSpaceEditorView.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                this.WorkspaceSpaceEditorView.ResetConnections();
                this.WorkspaceSpaceEditorView.ResetPlugins(0);
                this.WorkspaceSpaceEditorView.State = BinEditorState.READY;
                this.WorkspaceSpaceEditorView.Progress = 0;
            }
            , null);
            executing = false;
        }

        /// <summary>
        /// Stops the execution engine and blocks until this work is done
        /// </summary>
        private void waitingStop()
        {
            lock (this)
            {
                stopping = true;
                try
                {
                    GuiLogMessage(Resources.WorkspaceManagerClass_waitingStop_Stopping_execution_, NotificationLevel.Info);
                    ExecutionEngine.Stop();
                }
                catch (Exception ex)
                {
                    GuiLogMessage(String.Format(Resources.WorkspaceManagerClass_waitingStop_Exception_during_the_stopping_of_the_execution___0_,ex.Message), NotificationLevel.Error);
                }
                this.ExecutionEngine = null;
                GC.Collect();                
                executeEvent(this, EventArgs.Empty);
                stopping = false;
            }
        }

        /// <summary>
        /// Called to initialize the editor
        /// </summary>
        public void Initialize()
        {
            //nothing
        }

        /// <summary>
        /// Called when the editor is disposed
        /// </summary>
        public void Dispose()
        {
            if (ExecutionEngine != null && ExecutionEngine.IsRunning())
            {
                try
                {
                    ExecutionEngine.Stop();
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format(Resources.WorkspaceManagerClass_Dispose_Exception_during_stopping_of_the_ExecutionEngine___0_, ex), NotificationLevel.Error);
                }
            }

            try
            {
                if (WorkspaceModel != null)
                {
                    WorkspaceModel.Dispose();
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format(Resources.WorkspaceManagerClass_Dispose_Exception_during_disposing_of_the_Model___0_, ex), NotificationLevel.Error);
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// 
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IApplication Members

        /// <summary>
        /// 
        /// </summary>
        private PluginManager pluginManager;
        public PluginManager PluginManager
        {
            get { return pluginManager; }
            set
            {
                pluginManager = value;
                DragDropDataObjectToPluginConverter.PluginManager = value;
            }
        }

        #endregion

        #region GuiLogMessage

        /// <summary>
        /// Loggs a message to the logging mechanism of CrypTool
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="notificationLevel"></param>
        public void GuiLogMessage(string Message, NotificationLevel notificationLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                GuiLogEventArgs args = new GuiLogEventArgs(Message, this, notificationLevel);
                args.Title = "-";
                OnGuiLogNotificationOccured(this, args);
            }
        }

        /// <summary>
        /// GuiLogNotificationOccured
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void GuiLogNotificationOccured(IPlugin sender, GuiLogEventArgs args)
        {
            //Check if the logging event is Warning or Error and set the State of the PluginModel to
            //the corresponding PluginModelState
            if (args.NotificationLevel == NotificationLevel.Warning)
            {
                foreach (PluginModel pluginModel in this.WorkspaceModel.GetAllPluginModels())
                {
                    if (pluginModel.Plugin == sender)
                    {
                        pluginModel.State = PluginModelState.Warning;
                        pluginModel.GuiNeedsUpdate = true;
                    }
                }
            }

            if (args.NotificationLevel == NotificationLevel.Error)
            {
                foreach (PluginModel pluginModel in this.WorkspaceModel.GetAllPluginModels())
                {
                    if (pluginModel.Plugin == sender)
                    {
                        pluginModel.State = PluginModelState.Error;
                        pluginModel.GuiNeedsUpdate = true;
                    }
                }
            }

            if (OnGuiLogNotificationOccured != null)
            {
                switch (((WorkspaceManagerSettings)this.Settings).LogLevel)
                {
                    case 3://Error
                        if (args.NotificationLevel == NotificationLevel.Debug ||
                            args.NotificationLevel == NotificationLevel.Info ||
                            args.NotificationLevel == NotificationLevel.Warning)
                        {
                            return;
                        }
                        break;

                    case 2://Warning
                        if (args.NotificationLevel == NotificationLevel.Debug ||
                            args.NotificationLevel == NotificationLevel.Info)
                        {
                            return;
                        }
                        break;

                    case 1://Info
                        if (args.NotificationLevel == NotificationLevel.Debug)
                        {
                            return;
                        }
                        break;
                }
                OnGuiLogNotificationOccured(sender, args);
            }

        }

        #endregion GuiLogMessage

        /// <summary>
        /// Selected Plugin changed by View
        /// </summary>
        /// <param name="args"></param>
        public void onSelectedPluginChanged(PluginChangedEventArgs args)
        {
            if (OnSelectedPluginChanged != null)
            {
                try
                {
                    OnSelectedPluginChanged(this, args);
                }
                catch
                {
                    //wtf ?
                }
            }
        }

        #region IEditor Members

        public event OpenTabHandler OnOpenTab;
        public event OpenEditorHandler OnOpenEditor;

        #endregion

        private ObservableCollection<ComponentVisual> selectedPluginsList;

        /// <summary>
        /// Selected Collection of Plugin's
        /// </summary> 
        public ObservableCollection<ComponentVisual> SelectedPluginsList
        {
            get
            {
                return selectedPluginsList;
            }
            set
            {
                selectedPluginsList = value;
            }
        }

        public bool IsCtrlToggled = false;
        
        public BinEditorState State { get; set; }


        public void AddText()
        {
            ((EditorVisual)Presentation).AddText();
        }

        public void AddImage()
        {
            System.Windows.Forms.OpenFileDialog diag = new System.Windows.Forms.OpenFileDialog();
            if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Uri uriLocal = new Uri(diag.FileName);
                ((EditorVisual)Presentation).AddImage(uriLocal);
            }
        }
        
    }

    public class LoadingErrorEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}

//Restore warnings for unused or unassigned fields and events:
#pragma warning restore 0169, 0414, 0067