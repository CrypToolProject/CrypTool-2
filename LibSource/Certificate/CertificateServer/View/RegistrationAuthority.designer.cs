namespace CrypTool.CertificateServer.View
{
    partial class RegistrationAuthorityView
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RegistrationAuthorityView));
            this.registrationAuthorityListView = new System.Windows.Forms.ListView();
            this.authorizedHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.avatarHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.emailHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.worldHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.receiptTimeHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.programNameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.selectAllToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.AuthorizeToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.RejectToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.deleteToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.refreshComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.verifiedHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // registrationAuthorityListView
            // 
            this.registrationAuthorityListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.registrationAuthorityListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.registrationAuthorityListView.CheckBoxes = true;
            this.registrationAuthorityListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.authorizedHeader,
            this.verifiedHeader,
            this.avatarHeader,
            this.emailHeader,
            this.worldHeader,
            this.receiptTimeHeader,
            this.programNameHeader});
            this.registrationAuthorityListView.FullRowSelect = true;
            this.registrationAuthorityListView.GridLines = true;
            this.registrationAuthorityListView.Location = new System.Drawing.Point(0, 28);
            this.registrationAuthorityListView.MultiSelect = false;
            this.registrationAuthorityListView.Name = "registrationAuthorityListView";
            this.registrationAuthorityListView.Size = new System.Drawing.Size(603, 328);
            this.registrationAuthorityListView.TabIndex = 2;
            this.registrationAuthorityListView.UseCompatibleStateImageBehavior = false;
            this.registrationAuthorityListView.View = System.Windows.Forms.View.Details;
            // 
            // authorizedHeader
            // 
            this.authorizedHeader.Text = "Authorized";
            this.authorizedHeader.Width = 65;
            // 
            // avatarHeader
            // 
            this.avatarHeader.Text = "Avatar";
            this.avatarHeader.Width = 100;
            // 
            // emailHeader
            // 
            this.emailHeader.Text = "Email Address";
            this.emailHeader.Width = 180;
            // 
            // worldHeader
            // 
            this.worldHeader.Text = "World RegEx";
            this.worldHeader.Width = 100;
            // 
            // receiptTimeHeader
            // 
            this.receiptTimeHeader.Text = "Receipt Time";
            this.receiptTimeHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.receiptTimeHeader.Width = 120;
            // 
            // programNameHeader
            // 
            this.programNameHeader.Text = "Program Name";
            this.programNameHeader.Width = 100;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectAllToolStripButton,
            this.AuthorizeToolStripButton,
            this.RejectToolStripButton,
            this.deleteToolStripButton,
            this.refreshComboBox});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(606, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // selectAllToolStripButton
            // 
            this.selectAllToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.selectAllToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("selectAllToolStripButton.Image")));
            this.selectAllToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.selectAllToolStripButton.Name = "selectAllToolStripButton";
            this.selectAllToolStripButton.Size = new System.Drawing.Size(57, 22);
            this.selectAllToolStripButton.Text = "Select all";
            this.selectAllToolStripButton.Click += new System.EventHandler(this.selectAllButton_Click);
            // 
            // AuthorizeToolStripButton
            // 
            this.AuthorizeToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.AuthorizeToolStripButton.ForeColor = System.Drawing.Color.DarkOliveGreen;
            this.AuthorizeToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("AuthorizeToolStripButton.Image")));
            this.AuthorizeToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.AuthorizeToolStripButton.Name = "AuthorizeToolStripButton";
            this.AuthorizeToolStripButton.Size = new System.Drawing.Size(62, 22);
            this.AuthorizeToolStripButton.Text = "Authorize";
            this.AuthorizeToolStripButton.Click += new System.EventHandler(this.authorizeRequestsButton_Click);
            // 
            // RejectToolStripButton
            // 
            this.RejectToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.RejectToolStripButton.ForeColor = System.Drawing.Color.DarkOrange;
            this.RejectToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("RejectToolStripButton.Image")));
            this.RejectToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RejectToolStripButton.Name = "RejectToolStripButton";
            this.RejectToolStripButton.Size = new System.Drawing.Size(43, 22);
            this.RejectToolStripButton.Text = "Reject";
            this.RejectToolStripButton.Click += new System.EventHandler(this.rejectRequestButton_Click);
            // 
            // deleteToolStripButton
            // 
            this.deleteToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.deleteToolStripButton.ForeColor = System.Drawing.Color.Red;
            this.deleteToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("deleteToolStripButton.Image")));
            this.deleteToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.deleteToolStripButton.Name = "deleteToolStripButton";
            this.deleteToolStripButton.Size = new System.Drawing.Size(44, 22);
            this.deleteToolStripButton.Text = "Delete";
            this.deleteToolStripButton.Click += new System.EventHandler(this.deleteRequestsButton_Click);
            // 
            // refreshComboBox
            // 
            this.refreshComboBox.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.refreshComboBox.AutoToolTip = true;
            this.refreshComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.refreshComboBox.DropDownWidth = 100;
            this.refreshComboBox.Name = "refreshComboBox";
            this.refreshComboBox.Size = new System.Drawing.Size(121, 25);
            this.refreshComboBox.ToolTipText = "Refresh interval in seconds";
            this.refreshComboBox.SelectedIndexChanged += new System.EventHandler(this.refreshComboBox_SelectedIndexChanged);
            // 
            // verifiedHeader
            // 
            this.verifiedHeader.Text = "Verified";
            this.verifiedHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.verifiedHeader.Width = 50;
            // 
            // RegistrationAuthorityView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.registrationAuthorityListView);
            this.Name = "RegistrationAuthorityView";
            this.Size = new System.Drawing.Size(606, 359);
            this.VisibleChanged += new System.EventHandler(this.RegistrationAuthorityView_VisibleChanged);
            this.Resize += new System.EventHandler(this.RegistrationAuthority_Resize);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView registrationAuthorityListView;
        private System.Windows.Forms.ColumnHeader authorizedHeader;
        private System.Windows.Forms.ColumnHeader avatarHeader;
        private System.Windows.Forms.ColumnHeader emailHeader;
        private System.Windows.Forms.ColumnHeader worldHeader;
        private System.Windows.Forms.ColumnHeader receiptTimeHeader;
        private System.Windows.Forms.ColumnHeader programNameHeader;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton selectAllToolStripButton;
        private System.Windows.Forms.ToolStripButton AuthorizeToolStripButton;
        private System.Windows.Forms.ToolStripButton RejectToolStripButton;
        private System.Windows.Forms.ToolStripButton deleteToolStripButton;
        private System.Windows.Forms.ToolStripComboBox refreshComboBox;
        private System.Windows.Forms.Timer messageRefreshingTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.ColumnHeader verifiedHeader;

    }
}
