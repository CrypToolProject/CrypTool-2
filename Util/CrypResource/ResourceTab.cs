using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;

namespace CrypTool.Resource
{
    public class ResourceTab : TabPage
    {
        private static TabContentCtrl contentCtrl;
        private int selectedIndex;
        private string fileName;
        private DataTable dataSource;

        public string FileName 
        {
            get { return this.fileName; }
        }

        public DataTable DataSource
        {
            get { return this.dataSource; }
        }

        static ResourceTab()
        {
            contentCtrl = new TabContentCtrl();
            contentCtrl.Dock = DockStyle.Fill;
        }

        public ResourceTab(string fileName, DataTable dataSource)
        {
            this.SuspendLayout();
            this.fileName = fileName;
            this.dataSource = dataSource;
            this.selectedIndex = 0;
            this.Text = fileName.Substring(fileName.LastIndexOf("\\") + 1);
            this.Controls.Add(contentCtrl);
            this.Enter += new System.EventHandler(this.ResourceTab_Enter);
            this.Leave += new EventHandler(ResourceTab_Leave);
            contentCtrl.DataSource = this.dataSource;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void ResourceTab_Leave(object sender, EventArgs e)
        {
            this.selectedIndex = contentCtrl.SelectedIndex;
        }


        private void ResourceTab_Enter(object sender, EventArgs e)
        {
            this.SuspendLayout();
            contentCtrl.DataSource = this.dataSource;
            contentCtrl.SelectedIndex = this.selectedIndex;
            this.Controls.Add(contentCtrl);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

    }
}
