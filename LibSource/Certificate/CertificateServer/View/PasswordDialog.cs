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
    public partial class PasswordDialog : Form
    {

        #region Constructor

        private PasswordDialog(
                string windowTitle, 
                string messageDescription,
                DialogType type,
                DialogButtons buttons = DialogButtons.OKCancel,
                bool windowClosable = true)
        {
            InitializeComponent();
            this.Text = windowTitle;
            this.dialogDescription.Text = messageDescription;

            if (type == DialogType.Ask)
            {
                this.confirmPassword.Visible = false;
            }
            else
            {
                this.confirmPassword.Visible = true;
            }

            if (buttons == DialogButtons.OK)
            {
                this.cmdCancel.Visible = false;
            }

            if (windowClosable == false)
            {
                this.ControlBox = false;
            }

            EnteredPassword = "";
            EnteredConfirmPassword = "";
        }

        #endregion


        #region Static Show Dialog

        public static string ShowDialog(string title, string text, DialogType type, DialogButtons buttons)
        {
            PasswordDialog pwdDialog = new PasswordDialog(
                title,
                text,
                type,
                buttons);
            DialogResult pwdResult = pwdDialog.ShowDialog();

            if (pwdResult != System.Windows.Forms.DialogResult.OK)
            {
                return null;
            }
            return pwdDialog.EnteredPassword;
        }

        #endregion


        #region Event Handler

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            EnteredPassword = password.Text;
            EnteredConfirmPassword = confirmPassword.Text;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        #endregion


        #region Properties

        public string EnteredPassword { get; private set; }

        public string EnteredConfirmPassword { get; private set; }

        #endregion
    }

    public enum DialogButtons
    {
        OK = 0,
        OKCancel = 1
    }
    public enum DialogType
    {
        Ask = 0,
        Enter = 1
    }
}
