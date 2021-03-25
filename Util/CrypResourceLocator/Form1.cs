using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;


namespace CrypResourceLocator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "CrypTool Plugins (*.dll)|*.dll";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Assembly asm = Assembly.LoadFile(dlg.FileName);
                    richTextBox1.Text = "";
                    foreach (string name in asm.GetManifestResourceNames())
                    {
                        richTextBox1.Text += name + "\r\n";
                    }
                }
                catch
                {
                    MessageBox.Show("Could not open file: " + dlg.FileName);
                }
            }
        }
    }
}
