using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace CrypTool.Resource
{
    public partial class CreateResourceCtrl : UserControl
    {
        public delegate void CreateResourceEventHandler(string assemblyFileName, string resourceFileName);
        public event CreateResourceEventHandler OnCreateResource;

        public CreateResourceCtrl()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            this.tbCountryCode.Items.Add("Default");
            this.tbCountryCode.SelectedIndex = 0;
            this.tbCountryCode.Items.AddRange(CountryCode.Codes); 
        }

        private void bSelectAssembly_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Open CrypTool Plugin"; 
            dlg.Filter = "Crytool Plugin (*.dll)|*.dll";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                tbAssemblyFileName.Text = dlg.FileName;
                tbResourceDirectory.Text = new FileInfo(dlg.FileName).DirectoryName;
                bCreateResource.Enabled = true;
            }
        }

        private void bSelectFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (tbAssemblyFileName.Text != String.Empty)
                dlg.SelectedPath = new FileInfo(tbAssemblyFileName.Text).DirectoryName;

            if (dlg.ShowDialog() == DialogResult.OK)
                tbResourceDirectory.Text = dlg.SelectedPath;
        }

        private void bCreateResource_Clicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string assemblyFileShortName = new FileInfo(tbAssemblyFileName.Text).Name;
            assemblyFileShortName = assemblyFileShortName.Substring(0, assemblyFileShortName.LastIndexOf("."));
            string directory = new DirectoryInfo(tbResourceDirectory.Text).FullName;
            
            string defaultResFileName = Path.Combine(directory, string.Format("{0}.{1}.resx",assemblyFileShortName, tbCountryCode.SelectedItem.ToString()));

            if (tbCountryCode.SelectedIndex == 0)
                defaultResFileName = Path.Combine(directory, string.Format("{0}.resx", assemblyFileShortName));

            if (OnCreateResource != null)
                OnCreateResource(tbAssemblyFileName.Text, defaultResFileName);
        }
    }
}
