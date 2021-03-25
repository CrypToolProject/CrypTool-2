using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows.Forms;
using CrypTool.PluginBase;
using System.IO;
using System.Collections;

namespace CrypTool.Resource
{
    public partial class Form1 : Form
    {
        public Dictionary<string, ResourceTab> openFiles;

        private FileInfoCtrl fileInfoCtrl;
        private CreateResourceCtrl createResourceCtrl;
        private UpdateResourceCtrl updateResourceCtrl;

        public Form1()
        {
            InitializeComponent();
            Init();
           
        }

        private void Init()
        {
            ControlStatus();

            this.openFiles = new Dictionary<string, ResourceTab>();
            this.fileInfoCtrl = new FileInfoCtrl();
            this.fileInfoCtrl.Dock = DockStyle.Fill;

            this.createResourceCtrl = new CreateResourceCtrl();
            this.createResourceCtrl.Dock = DockStyle.Fill;
            this.createResourceCtrl.OnCreateResource += new CreateResourceCtrl.CreateResourceEventHandler(createResourceCtrl_OnCreateResource);

            this.updateResourceCtrl = new UpdateResourceCtrl();
            this.updateResourceCtrl.Dock = DockStyle.Fill;
            this.updateResourceCtrl.OnUpdateResource += new UpdateResourceCtrl.UpdateResourceEventHandler(updateResourceCtrl_OnUpdateResource);
        }

        private void updateResourceCtrl_OnUpdateResource(string assemblyFileName, string resourceFileName)
        {
            ResourceManager.UpdateResourceFile(assemblyFileName, resourceFileName);
            LoadResource(resourceFileName);

        }

        private void ControlStatus()
        {
            if (resTabCtrl.TabPages.Count == 0)
            {
                this.sidePanel.Controls.Clear();
                this.createResourceMenuItem.Enabled = true;
                this.loadResourceMenuItem.Enabled = true;
                this.updateResourceMenuItem.Enabled = true;
                this.closeMenuItem.Enabled = false;
                this.closeAllMenuItem.Enabled = false;
                this.saveMenuItem.Enabled = false;
                this.exitMenuItem.Enabled = true;
                this.createResourceToolStripButton.Enabled = true;
                this.openToolStripButton.Enabled = true;
                this.saveToolStripButton.Enabled = false;
                this.closeTabToolStripButton.Enabled = false;
            }
            else
            {
                this.createResourceMenuItem.Enabled = true;
                this.loadResourceMenuItem.Enabled = true;
                this.updateResourceMenuItem.Enabled = true;
                this.closeMenuItem.Enabled = true;
                this.closeAllMenuItem.Enabled = true;
                this.saveMenuItem.Enabled = true;
                this.exitMenuItem.Enabled = true;
                this.createResourceToolStripButton.Enabled = true;
                this.openToolStripButton.Enabled = true;
                this.saveToolStripButton.Enabled = true;
                this.closeTabToolStripButton.Enabled = true;
            }
        }

        private void createResourceCtrl_OnCreateResource(string assemblyFileName, string resourceFileName)
        {
            if (File.Exists(resourceFileName))
            {
                if (MessageBox.Show("File " + resourceFileName + " already exists. Override?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    return;
            }
            ResourceManager.CreateResourceFile(assemblyFileName, resourceFileName);

            if (openFiles.ContainsKey(resourceFileName))
            {
                int index = resTabCtrl.TabPages.IndexOf(openFiles[resourceFileName]);
                resTabCtrl.TabPages.Remove(openFiles[resourceFileName]);
                ResourceTab tab = new ResourceTab(resourceFileName, ResourceManager.LoadResourceFile(resourceFileName));
                resTabCtrl.TabPages.Insert(index, tab);
                resTabCtrl.SelectedTab = tab;
                openFiles[resourceFileName] = tab;
            }
            else
            {
                ResourceTab tab = new ResourceTab(resourceFileName, ResourceManager.LoadResourceFile(resourceFileName));
                resTabCtrl.TabPages.Add(tab);
                resTabCtrl.SelectedTab = tab;
                openFiles.Add(resourceFileName, tab);
            }
            ShowFileInfo();
            ControlStatus();
        }

        private void createResourceMenuItem_Click(object sender, EventArgs e)
        {
            sidePanel.Controls.Clear();
            sidePanel.Controls.Add(createResourceCtrl);
        }

        private void loadResourceMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Open CrypTool Plugin Resource";
            dlg.Filter = "Crytool Plugin Resource (*.resx)|*.resx";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                LoadResource(dlg.FileName);
            }
        }

        private void LoadResource(string resourceFileName)
        {
            if (openFiles.ContainsKey(resourceFileName))
            {
                int index = resTabCtrl.TabPages.IndexOf(openFiles[resourceFileName]);
                resTabCtrl.TabPages.Remove(openFiles[resourceFileName]);
                ResourceTab tab = new ResourceTab(resourceFileName, ResourceManager.LoadResourceFile(resourceFileName));
                resTabCtrl.TabPages.Insert(index, tab);
                resTabCtrl.SelectedTab = tab;
                openFiles[resourceFileName] = tab;
            }
            else
            {
                ResourceTab tab = new ResourceTab(resourceFileName, ResourceManager.LoadResourceFile(resourceFileName));
                resTabCtrl.TabPages.Add(tab);
                resTabCtrl.SelectedTab = tab;
                openFiles.Add(resourceFileName, tab);
            }
            ShowFileInfo();
            ControlStatus();
        }

        private void updateResourceMenuItem_Click(object sender, EventArgs e)
        {
            sidePanel.Controls.Clear();
            sidePanel.Controls.Add(updateResourceCtrl);
        }

        private void closeMenuItem_Click(object sender, EventArgs e)
        {
            CloseTab(resTabCtrl.SelectedTab as ResourceTab);
            ControlStatus();
        }

        private void closeAllMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ResourceTab tab in resTabCtrl.TabPages)
            {
                CloseTab(tab);
            }
            ControlStatus();
        }

        private void saveMenuItem_Click(object sender, EventArgs e)
        {
            ResourceManager.SaveResourceFile(
                ((ResourceTab)this.resTabCtrl.SelectedTab).FileName,
                ((ResourceTab)this.resTabCtrl.SelectedTab).DataSource);
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ResourceTab tab in resTabCtrl.TabPages)
            {
                CloseTab(tab);
            }
            Application.Exit();
        }

        private void resTabCtrl_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowFileInfo();
        }

        private void ShowFileInfo()
        {
            this.sidePanel.Controls.Clear();
            this.sidePanel.Controls.Add(fileInfoCtrl);
            if (this.resTabCtrl.SelectedTab == null)
                this.fileInfoCtrl.FileName = String.Empty;
            else
                this.fileInfoCtrl.FileName = ((ResourceTab)this.resTabCtrl.SelectedTab).FileName;

            this.createResourceMenuItem.Enabled = true;
            this.loadResourceMenuItem.Enabled = true;
            this.updateResourceMenuItem.Enabled = true;
            this.closeMenuItem.Enabled = true;
            this.closeAllMenuItem.Enabled = true;
            this.saveMenuItem.Enabled = true;
            this.exitMenuItem.Enabled = true;

        }

        private void closeTabToolStripButton_Click(object sender, EventArgs e)
        {
            CloseTab(resTabCtrl.SelectedTab as ResourceTab);
            ControlStatus();
        }

        private void CloseTab(ResourceTab tab)
        {
            if (tab != null)
            {
                if (tab.DataSource.GetChanges() != null)
                {
                    if (MessageBox.Show("Save " + tab.Text + " ?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        ResourceManager.SaveResourceFile(
                            tab.FileName,
                            tab.DataSource);
                    }
                }
                resTabCtrl.TabPages.Remove(tab);
            }
        }

        private void createResourceToolStripButton_Click(object sender, EventArgs e)
        {
            createResourceMenuItem_Click(sender, e);
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            loadResourceMenuItem_Click(sender, e);
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            saveMenuItem_Click(sender, e);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }
    }
}
