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
    public partial class UpdateResourceCtrl : UserControl
    {
        public delegate void UpdateResourceEventHandler(string assemblyFileName, string resourceFileName);
        public event UpdateResourceEventHandler OnUpdateResource;

        public UpdateResourceCtrl()
        {
            InitializeComponent();
        }

        private void bSelectAssembly_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Open CrypTool Plugin";
            dlg.Filter = "Crytool Plugin (*.dll)|*.dll";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                tbAssemblyFileName.Text = dlg.FileName;
                if ((tbResourceFileName.Text != String.Empty) && (tbAssemblyFileName.Text != String.Empty))
                {
                    bUpdateResource.Enabled = true;
                }
            }
        }

        private void bSelectFolder_Click(object sender, EventArgs e)
        {

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Open CrypTool Plugin Resource";
            dlg.Filter = "Crytool Plugin Resource (*.resx)|*.resx";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                tbResourceFileName.Text = dlg.FileName;
                if ((tbResourceFileName.Text != String.Empty) && (tbAssemblyFileName.Text != String.Empty))
                {
                    bUpdateResource.Enabled = true;
                }
            }

        }

        private void bUpdateResource_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (OnUpdateResource != null)
                OnUpdateResource(tbAssemblyFileName.Text, tbResourceFileName.Text);
        }
    }
}
