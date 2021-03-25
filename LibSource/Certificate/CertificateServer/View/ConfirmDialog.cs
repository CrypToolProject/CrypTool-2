using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CrypTool.CertificateServer.View
{
    public partial class ConfirmDialog : Form
    {
        private ConfirmDialog(string title, string question, string list)
        {
            InitializeComponent();
            this.Text = title;
            this.questionLabel.Text = question;
            this.textBox.Text = list;
        }

        public static DialogResult ShowDialog(string title, string question, string list)
        {
            ConfirmDialog confirmDialog = new ConfirmDialog(title, question, list);
            return confirmDialog.ShowDialog();
        }

        #region Event Handler

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        #endregion

    }
}
