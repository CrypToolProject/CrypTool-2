/*
   Copyright 2008 Martin Saternus, Arno Wacker, Thomas Schmid, Sebastian Przybylski

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
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using CrypTool.Core;
using CrypTool.CrypWin.Helper;
using CrypTool.CrypWin.Properties;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Editor;
using Microsoft.Win32;
using System.Windows.Documents;
using System.Linq;

namespace CrypTool.CrypWin
{
    public enum FileOperationResult
    {
        Abort,
        Continue
    }

    public partial class MainWindow
    {
        private RecentFileList recentFileList = RecentFileList.GetSingleton();

        private void NewProject(Type editor)
        {
            try
            {
                AddEditorDispatched(editor);
                ActiveEditor.New();

                //ActiveEditor.Presentation.ToolTip = Properties.Resources.Unsaved_workspace;
                SetCurrentEditorAsDefaultEditor();
            }
            catch (Exception ex)
            {
                //Nothing to do
            }
        }

        public void OpenProjectInGuiThread(string fileName)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    OpenProject(fileName, null);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Exception during opening of project in gui thread: {0}", ex.Message), NotificationLevel.Error);
                }
        }, null);
        }

        /// <summary>
        /// Warning: does not check for unsaved changes. Use OpenProject() if you need to.
        /// </summary>
        /// <param name="fileName"></param>
        private void OpenProject(string fileName, FileLoadedHandler OnLoaded)
        {
            try
            {
                if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
                {
                    listPluginsAlreadyInitialized.Clear();

                    var ext = new FileInfo(fileName).Extension;
                    if (ext.Length < 2)
                    {
                        return;
                    }

                    ext = ext.Remove(0, 1);
                    if (ComponentInformations.EditorExtension.ContainsKey(ext))
                    {
                        Type editorType = ComponentInformations.EditorExtension[ext];

                        var editor = AddEditorDispatched(editorType);
                        if (editor == null)
                        {
                            return;
                        }

                        if (OnLoaded != null)
                        {
                            editor.OnFileLoaded += OnLoaded;
                            editor.OnFileLoaded += delegate { editor.OnFileLoaded -= OnLoaded; };
                        }

                        editor.Open(fileName);
                        if (editor.Presentation != null)
                        {
                            editor.Presentation.ToolTip = fileName;
                        }

                        SetCurrentEditorAsDefaultEditor();
                        this.ProjectFileName = fileName;

                        recentFileList.AddRecentFile(fileName);
                    }
                }
                else
                {
                    MessageBox.Show(string.Format(Properties.Resources.File__0__doesn_t_exist_, fileName), Properties.Resources.Error_loading_file);
                    recentFileList.RemoveFile(fileName);
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Couldn't open project file {0}:{1}", fileName, ex.Message), NotificationLevel.Error);
            }
        }

        private void OpenProjectFileEvent(IEditor editor, string fileName)
        {
            OpenProject(fileName, null);
        }

        private void OpenProject()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = CreateOpenProjectFilter();
            dlg.InitialDirectory = Settings.Default.LastPath;
            
            if (dlg.ShowDialog() == true)            
            {
                this.OpenProject(dlg.FileName, null);

                if (Settings.Default.useLastPath)
                    Settings.Default.LastPath = Directory.GetParent(dlg.FileName).FullName;
            }
        }

        private string[] OpenMultipleProjectsDialog()
        {
            if (SaveChangesIfNecessary() == FileOperationResult.Abort)
                return new string[0];

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = true;
            dlg.Filter = CreateOpenProjectFilter();

#if (!DEBUG)
            if (Settings.Default.LastPath != null) dlg.InitialDirectory = Settings.Default.LastPath;
#endif

            if (dlg.ShowDialog() == true)
            {
                return dlg.FileNames;
            }

            return new string[0];
        }

        private bool CloseProject()
        {
            return CloseProject(ActiveEditor);
        }

        private bool CloseProject(IEditor editor)
        {
            FileOperationResult result = SaveChangesIfNecessary(editor);
            if (result == FileOperationResult.Continue && !restart) // in case restart was clicked there is no project to close (updater tab is open)
            {
                CloseIfOpen(editor);
            }
            return result != FileOperationResult.Abort;
        }

        private bool CloseProjects()
        {
            foreach (var editor in tabToContentMap.Values.OfType<IEditor>())
            {
                if (!CloseProject(editor))
                {
                    return false;
                }
            }
            return true;
        }

        private void CloseIfOpen(IEditor editor)
        {
            if (editor != null)
            {
                editorToFileMap[editor] = null;
                if (editor.CanStop)
                {
                    StopProjectExecution(editor);
                }
            }
        }

        private FileOperationResult SaveChangesIfNecessary(IEditor editor)
        {
            if (editor == null || !editor.HasChanges || shutdown)
                return FileOperationResult.Continue;
                        
            string file = null;
            if (editorToFileMap.ContainsKey(editor))
                file = editorToFileMap[editor];

            switch (MessageBoxHelper.SaveChanges(file))
            {
                case MessageBoxResult.Yes:
                    var before = this.ActiveEditor;
                    this.ActiveEditor = editor;
                    return SaveProject();
                    this.ActiveEditor = before;
                case MessageBoxResult.No:
                    return FileOperationResult.Continue;
                case MessageBoxResult.Cancel:
                default:
                    return FileOperationResult.Abort;
            }
        }

        private FileOperationResult SaveChangesIfNecessary()
        {
            return SaveChangesIfNecessary(this.ActiveEditor);
        }

        private FileOperationResult SaveProject()
        {
            if (ProjectFileName == null || this.ProjectFileName == String.Empty || Path.GetFullPath(this.ProjectFileName).StartsWith(defaultTemplatesDirectory,true,null))
            {
                return SaveProjectAs();
            }

            this.ActiveEditor.Save(this.ProjectFileName);
            OpenTab(ActiveEditor, new TabInfo() { Tooltip = new Span(new Run(this.ProjectFileName)) }, null);
            recentFileList.AddRecentFile(this.ProjectFileName);
            return FileOperationResult.Continue;
        }

        private FileOperationResult SaveProjectAs()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            if (this.ProjectFileName != null)
            {
                dlg.FileName = new FileInfo(this.ProjectFileName).Name; // propose current file name as new name    
            }
            dlg.InitialDirectory = Settings.Default.LastPath;
            dlg.Filter = CreateSaveProjectFilter();

            bool isInvalidPath = true;
            do
            {
                try
                {
                    if (dlg.ShowDialog() != true) // nullable bool? may be null or false
                        return FileOperationResult.Abort;
                }
                catch (Exception) // if dialog raises a Win32Exception, we silently retry again once with another InitialDirectory (addresses #362)
                {
                    dlg.InitialDirectory = personalDir;
                    if (dlg.ShowDialog() != true)
                        return FileOperationResult.Abort;
                }

                isInvalidPath = Path.GetFullPath(dlg.FileName).StartsWith(defaultTemplatesDirectory);
                if (isInvalidPath)
                {
                    MessageBox.Show(Properties.Resources.WritingInTemplatesNotAllowed,
                        Properties.Resources.TemplateUseDifferentDirectory, MessageBoxButton.OK, MessageBoxImage.Information);
                    dlg.InitialDirectory = personalDir;
                    dlg.FileName = new FileInfo(dlg.FileName).Name;
                }
            }
            while (isInvalidPath);

            this.ProjectFileName = dlg.FileName; // dialog successful

            ActiveEditor.Save(this.ProjectFileName);
            ActiveEditor.Presentation.ToolTip = this.ProjectFileName;
            recentFileList.AddRecentFile(this.ProjectFileName);

            if (Settings.Default.useLastPath)
                Settings.Default.LastPath = Directory.GetParent(ProjectFileName).FullName;

            return FileOperationResult.Continue;
        }

        private string CreateOpenProjectFilter()
        {
            string defaultPattern = "cwm (*.cwm)|*.cwm";

            try
            {
                var ext = ComponentInformations.EditorExtension.Values.ToDictionary(t => t.GetPluginInfoAttribute().Caption, t => t.GetEditorInfoAttribute().DefaultExtension);
                if (ext.Count() == 0) return defaultPattern;

                string filter = String.Join("|", ext.Select(i => string.Format("{0} (*.{1})|*.{1}", i.Key, i.Value)));
                if (ext.Count() > 1)
                {
                    string allExtensions = String.Join(";", ext.Select(i => string.Format("*.{0}", i.Value)));
                    filter = string.Format("All ({0})|{0}|{1}", allExtensions, filter);
                }
                
                return filter;
            }
            catch (Exception ex)
            {
                //creating the default extensions using the plugins failed
                //so return only cwm since this is the default file extension
                return defaultPattern;
            }
        }

        private string CreateSaveProjectFilter()
        {
            return string.Format("{0} (*.{1}) | *.{1}", this.ActiveEditor.GetPluginInfoAttribute().Caption, this.ActiveEditor.GetEditorInfoAttribute().DefaultExtension);
        }
    }

}
