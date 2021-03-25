namespace CrypTool.CertificateServer.View
{
    partial class MainForm
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

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuBar = new System.Windows.Forms.MenuStrip();
            this.fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.caCertificateMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createCACertMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importCACertMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportCACertMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tlsCertificateMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deriveTlsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importTlsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportTlsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.storeInDatabaseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.registrationAuthorityMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.serverStatusMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.serverLogMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolBar = new System.Windows.Forms.ToolStrip();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.statusBarStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusBarDatabaseLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusBarCertLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.importCertificateFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.exportCertificateFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.serverStatusView = new CrypTool.CertificateServer.View.ServerStatus();
            this.createCACertView = new CrypTool.CertificateServer.View.CreateCACertificate();
            this.serverLogView = new CrypTool.CertificateServer.View.ServerLog();
            this.registrationAuthorityView = new CrypTool.CertificateServer.View.RegistrationAuthorityView();
            this.viewPanel = new System.Windows.Forms.Panel();
            this.toolBarStartStopButton = new System.Windows.Forms.ToolStripButton();
            this.toolBarCreateButton = new System.Windows.Forms.ToolStripButton();
            this.toolBarImportButton = new System.Windows.Forms.ToolStripButton();
            this.toolBarServerLogButton = new System.Windows.Forms.ToolStripButton();
            this.toolBarServerStatusButton = new System.Windows.Forms.ToolStripButton();
            this.toolBarRegistrationAuthorityButton = new System.Windows.Forms.ToolStripButton();
            this.menuBar.SuspendLayout();
            this.toolBar.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuBar
            // 
            this.menuBar.AutoSize = false;
            this.menuBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenu,
            this.viewMenu});
            this.menuBar.Location = new System.Drawing.Point(0, 0);
            this.menuBar.Name = "menuBar";
            this.menuBar.Size = new System.Drawing.Size(624, 24);
            this.menuBar.TabIndex = 2;
            this.menuBar.Text = "menuStrip1";
            // 
            // fileMenu
            // 
            this.fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.caCertificateMenuItem,
            this.tlsCertificateMenuItem,
            this.storeInDatabaseMenuItem,
            this.exitMenuItem});
            this.fileMenu.Name = "fileMenu";
            this.fileMenu.Size = new System.Drawing.Size(37, 20);
            this.fileMenu.Text = "File";
            // 
            // caCertificateMenuItem
            // 
            this.caCertificateMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createCACertMenuItem,
            this.importCACertMenuItem,
            this.exportCACertMenuItem});
            this.caCertificateMenuItem.Name = "caCertificateMenuItem";
            this.caCertificateMenuItem.Size = new System.Drawing.Size(224, 22);
            this.caCertificateMenuItem.Text = "CA Certificate";
            // 
            // createCACertMenuItem
            // 
            this.createCACertMenuItem.Name = "createCACertMenuItem";
            this.createCACertMenuItem.Size = new System.Drawing.Size(133, 22);
            this.createCACertMenuItem.Text = "Create new";
            this.createCACertMenuItem.Click += new System.EventHandler(this.ShowCreateCACertView_Click);
            // 
            // importCACertMenuItem
            // 
            this.importCACertMenuItem.Name = "importCACertMenuItem";
            this.importCACertMenuItem.Size = new System.Drawing.Size(133, 22);
            this.importCACertMenuItem.Text = "Import";
            this.importCACertMenuItem.Click += new System.EventHandler(this.ShowImportCertificateDialog_Click);
            // 
            // exportCACertMenuItem
            // 
            this.exportCACertMenuItem.Enabled = false;
            this.exportCACertMenuItem.Name = "exportCACertMenuItem";
            this.exportCACertMenuItem.Size = new System.Drawing.Size(133, 22);
            this.exportCACertMenuItem.Text = "Export";
            this.exportCACertMenuItem.Click += new System.EventHandler(this.ShowExportCertificateDialog_Click);
            // 
            // tlsCertificateMenuItem
            // 
            this.tlsCertificateMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deriveTlsMenuItem,
            this.importTlsMenuItem,
            this.exportTlsMenuItem});
            this.tlsCertificateMenuItem.Enabled = false;
            this.tlsCertificateMenuItem.Name = "tlsCertificateMenuItem";
            this.tlsCertificateMenuItem.Size = new System.Drawing.Size(224, 22);
            this.tlsCertificateMenuItem.Text = "TLS Certificate";
            // 
            // deriveTlsMenuItem
            // 
            this.deriveTlsMenuItem.Name = "deriveTlsMenuItem";
            this.deriveTlsMenuItem.Size = new System.Drawing.Size(237, 22);
            this.deriveTlsMenuItem.Text = "Derive new from CA Certificate";
            this.deriveTlsMenuItem.Click += new System.EventHandler(this.DeriveTlsCertificate_Click);
            // 
            // importTlsMenuItem
            // 
            this.importTlsMenuItem.Name = "importTlsMenuItem";
            this.importTlsMenuItem.Size = new System.Drawing.Size(237, 22);
            this.importTlsMenuItem.Text = "Import";
            this.importTlsMenuItem.Click += new System.EventHandler(this.ShowImportCertificateDialog_Click);
            // 
            // exportTlsMenuItem
            // 
            this.exportTlsMenuItem.Enabled = false;
            this.exportTlsMenuItem.Name = "exportTlsMenuItem";
            this.exportTlsMenuItem.Size = new System.Drawing.Size(237, 22);
            this.exportTlsMenuItem.Text = "Export";
            this.exportTlsMenuItem.Click += new System.EventHandler(this.ShowExportCertificateDialog_Click);
            // 
            // storeInDatabaseMenuItem
            // 
            this.storeInDatabaseMenuItem.Enabled = false;
            this.storeInDatabaseMenuItem.Name = "storeInDatabaseMenuItem";
            this.storeInDatabaseMenuItem.Size = new System.Drawing.Size(224, 22);
            this.storeInDatabaseMenuItem.Text = "Store certificates in database";
            this.storeInDatabaseMenuItem.Click += new System.EventHandler(this.StoreInDatabaseMenuItem_Click);
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Name = "exitMenuItem";
            this.exitMenuItem.Size = new System.Drawing.Size(224, 22);
            this.exitMenuItem.Text = "Exit";
            this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
            // 
            // viewMenu
            // 
            this.viewMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.registrationAuthorityMenuItem,
            this.serverStatusMenuItem,
            this.serverLogMenuItem});
            this.viewMenu.Name = "viewMenu";
            this.viewMenu.Size = new System.Drawing.Size(44, 20);
            this.viewMenu.Text = "View";
            // 
            // registrationAuthorityMenuItem
            // 
            this.registrationAuthorityMenuItem.Name = "registrationAuthorityMenuItem";
            this.registrationAuthorityMenuItem.Size = new System.Drawing.Size(190, 22);
            this.registrationAuthorityMenuItem.Text = "Registration Authority";
            this.registrationAuthorityMenuItem.Click += new System.EventHandler(this.ShowRegistrationAuthorityView_Click);
            // 
            // serverStatusMenuItem
            // 
            this.serverStatusMenuItem.Name = "serverStatusMenuItem";
            this.serverStatusMenuItem.Size = new System.Drawing.Size(190, 22);
            this.serverStatusMenuItem.Text = "Server status";
            this.serverStatusMenuItem.Click += new System.EventHandler(this.ShowServerStatusView_Click);
            // 
            // serverLogMenuItem
            // 
            this.serverLogMenuItem.Name = "serverLogMenuItem";
            this.serverLogMenuItem.Size = new System.Drawing.Size(190, 22);
            this.serverLogMenuItem.Text = "Server Log";
            this.serverLogMenuItem.Click += new System.EventHandler(this.ShowLogView_Click);
            // 
            // toolBar
            // 
            this.toolBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolBar.ImageScalingSize = new System.Drawing.Size(48, 48);
            this.toolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolBarStartStopButton,
            this.toolBarServerStatusButton,
            this.toolBarImportButton,
            this.toolBarCreateButton,
            this.toolBarRegistrationAuthorityButton,
            this.toolBarServerLogButton});
            this.toolBar.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolBar.Location = new System.Drawing.Point(0, 24);
            this.toolBar.Name = "toolBar";
            this.toolBar.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.toolBar.Size = new System.Drawing.Size(624, 55);
            this.toolBar.TabIndex = 1;
            this.toolBar.Text = "toolStrip1";
            // 
            // statusBar
            // 
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusBarStatusLabel,
            this.statusBarDatabaseLabel,
            this.statusBarCertLabel});
            this.statusBar.Location = new System.Drawing.Point(0, 440);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(624, 22);
            this.statusBar.TabIndex = 0;
            this.statusBar.Text = "statusStrip1";
            // 
            // statusBarStatusLabel
            // 
            this.statusBarStatusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.statusBarStatusLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.statusBarStatusLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.statusBarStatusLabel.Margin = new System.Windows.Forms.Padding(2);
            this.statusBarStatusLabel.Name = "statusBarStatusLabel";
            this.statusBarStatusLabel.Size = new System.Drawing.Size(104, 18);
            this.statusBarStatusLabel.Text = "Server is stopped";
            this.statusBarStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statusBarDatabaseLabel
            // 
            this.statusBarDatabaseLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.statusBarDatabaseLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.statusBarDatabaseLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.statusBarDatabaseLabel.Margin = new System.Windows.Forms.Padding(2);
            this.statusBarDatabaseLabel.Name = "statusBarDatabaseLabel";
            this.statusBarDatabaseLabel.Size = new System.Drawing.Size(351, 18);
            this.statusBarDatabaseLabel.Spring = true;
            this.statusBarDatabaseLabel.Text = "Database not connected";
            // 
            // statusBarCertLabel
            // 
            this.statusBarCertLabel.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.statusBarCertLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.statusBarCertLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.statusBarCertLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.statusBarCertLabel.Margin = new System.Windows.Forms.Padding(2);
            this.statusBarCertLabel.Name = "statusBarCertLabel";
            this.statusBarCertLabel.Size = new System.Drawing.Size(142, 18);
            this.statusBarCertLabel.Text = "No CA certificate loaded";
            this.statusBarCertLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // importCertificateFileDialog
            // 
            this.importCertificateFileDialog.Filter = "Certificate files (*.pfx;*.p12)|*.pfx;*.p12|All files (*.*)|*.*";
            this.importCertificateFileDialog.Title = "Select Certificate";
            // 
            // exportCertificateFileDialog
            // 
            this.exportCertificateFileDialog.Filter = "Certificate File (*.crt)|*.crt|Personal Information Exchange File (*.p12)|*.p12";
            this.exportCertificateFileDialog.Title = "Select place to save the Certificate";
            // 
            // serverStatusView
            // 
            this.serverStatusView.AutoSize = true;
            this.serverStatusView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.serverStatusView.Location = new System.Drawing.Point(5, 5);
            this.serverStatusView.Name = "serverStatusView";
            this.serverStatusView.Size = new System.Drawing.Size(614, 351);
            this.serverStatusView.TabIndex = 4;
            // 
            // createCACertView
            // 
            this.createCACertView.AutoSize = true;
            this.createCACertView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.createCACertView.Location = new System.Drawing.Point(0, 0);
            this.createCACertView.Name = "createCACertView";
            this.createCACertView.Size = new System.Drawing.Size(599, 354);
            this.createCACertView.TabIndex = 5;
            // 
            // serverLogView
            // 
            this.serverLogView.AutoSize = true;
            this.serverLogView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.serverLogView.Location = new System.Drawing.Point(0, 0);
            this.serverLogView.Name = "serverLogView";
            this.serverLogView.Size = new System.Drawing.Size(599, 354);
            this.serverLogView.TabIndex = 6;
            // 
            // registrationAuthorityView
            // 
            this.registrationAuthorityView.AutoSize = true;
            this.registrationAuthorityView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.registrationAuthorityView.Location = new System.Drawing.Point(0, 0);
            this.registrationAuthorityView.Name = "registrationAuthorityView";
            this.registrationAuthorityView.Size = new System.Drawing.Size(599, 354);
            this.registrationAuthorityView.TabIndex = 6;
            // 
            // viewPanel
            // 
            this.viewPanel.AutoSize = true;
            this.viewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewPanel.Location = new System.Drawing.Point(0, 79);
            this.viewPanel.Name = "viewPanel";
            this.viewPanel.Padding = new System.Windows.Forms.Padding(5);
            this.viewPanel.Size = new System.Drawing.Size(624, 361);
            this.viewPanel.TabIndex = 3;
            // 
            // toolBarStartStopButton
            // 
            this.toolBarStartStopButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolBarStartStopButton.Enabled = false;
            this.toolBarStartStopButton.Image = global::CrypTool.CertificateServer.Properties.Resources.server_start;
            this.toolBarStartStopButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolBarStartStopButton.Name = "toolBarStartStopButton";
            this.toolBarStartStopButton.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.toolBarStartStopButton.Size = new System.Drawing.Size(62, 52);
            this.toolBarStartStopButton.Text = "Start certification server";
            this.toolBarStartStopButton.Click += new System.EventHandler(this.StartStopServer_Click);
            // 
            // toolBarCreateButton
            // 
            this.toolBarCreateButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolBarCreateButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolBarCreateButton.Image = global::CrypTool.CertificateServer.Properties.Resources.create_cert;
            this.toolBarCreateButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolBarCreateButton.Name = "toolBarCreateButton";
            this.toolBarCreateButton.Size = new System.Drawing.Size(52, 52);
            this.toolBarCreateButton.Text = "Create CA certificate";
            this.toolBarCreateButton.Click += new System.EventHandler(this.ShowCreateCACertView_Click);
            // 
            // toolBarImportButton
            // 
            this.toolBarImportButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolBarImportButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolBarImportButton.Image = global::CrypTool.CertificateServer.Properties.Resources.import_cert;
            this.toolBarImportButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolBarImportButton.Name = "toolBarImportButton";
            this.toolBarImportButton.Size = new System.Drawing.Size(52, 52);
            this.toolBarImportButton.Text = "Import CA certificate";
            this.toolBarImportButton.Click += new System.EventHandler(this.ShowImportCertificateDialog_Click);
            // 
            // toolBarServerLogButton
            // 
            this.toolBarServerLogButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolBarServerLogButton.Image = global::CrypTool.CertificateServer.Properties.Resources.server_log;
            this.toolBarServerLogButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolBarServerLogButton.Name = "toolBarServerLogButton";
            this.toolBarServerLogButton.Size = new System.Drawing.Size(52, 52);
            this.toolBarServerLogButton.Text = "Show server log";
            this.toolBarServerLogButton.Click += new System.EventHandler(this.ShowLogView_Click);
            // 
            // toolBarServerStatusButton
            // 
            this.toolBarServerStatusButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolBarServerStatusButton.Image = global::CrypTool.CertificateServer.Properties.Resources.server_status;
            this.toolBarServerStatusButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolBarServerStatusButton.Name = "toolBarServerStatusButton";
            this.toolBarServerStatusButton.Size = new System.Drawing.Size(52, 52);
            this.toolBarServerStatusButton.Text = "Show server status";
            this.toolBarServerStatusButton.Click += new System.EventHandler(this.ShowServerStatusView_Click);
            // 
            // toolBarRegistrationAuthorityButton
            // 
            this.toolBarRegistrationAuthorityButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolBarRegistrationAuthorityButton.Image = global::CrypTool.CertificateServer.Properties.Resources.RegistrationAuthority;
            this.toolBarRegistrationAuthorityButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolBarRegistrationAuthorityButton.Name = "toolBarRegistrationAuthorityButton";
            this.toolBarRegistrationAuthorityButton.Size = new System.Drawing.Size(52, 52);
            this.toolBarRegistrationAuthorityButton.Text = "Registration Authority";
            this.toolBarRegistrationAuthorityButton.Click += new System.EventHandler(this.ShowRegistrationAuthorityView_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 462);
            this.Controls.Add(this.viewPanel);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.toolBar);
            this.Controls.Add(this.menuBar);
            this.MainMenuStrip = this.menuBar;
            this.MinimumSize = new System.Drawing.Size(640, 500);
            this.Name = "MainForm";
            this.Text = "P@Porator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.menuBar.ResumeLayout(false);
            this.menuBar.PerformLayout();
            this.toolBar.ResumeLayout(false);
            this.toolBar.PerformLayout();
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private CrypTool.CertificateServer.View.ServerLog serverLogView;
        private CrypTool.CertificateServer.View.ServerStatus serverStatusView;
        private CrypTool.CertificateServer.View.CreateCACertificate createCACertView;
        private CrypTool.CertificateServer.View.RegistrationAuthorityView registrationAuthorityView;
        private System.Windows.Forms.MenuStrip menuBar;
        private System.Windows.Forms.ToolStripMenuItem fileMenu;
        private System.Windows.Forms.ToolStripMenuItem caCertificateMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createCACertMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importCACertMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportCACertMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tlsCertificateMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deriveTlsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importTlsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportTlsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewMenu;
        private System.Windows.Forms.ToolStripMenuItem serverStatusMenuItem;
        private System.Windows.Forms.ToolStripMenuItem serverLogMenuItem;
        private System.Windows.Forms.ToolStrip toolBar;
        private System.Windows.Forms.ToolStripButton toolBarStartStopButton;
        private System.Windows.Forms.ToolStripButton toolBarCreateButton;
        private System.Windows.Forms.ToolStripButton toolBarImportButton;
        private System.Windows.Forms.ToolStripButton toolBarServerStatusButton;
        private System.Windows.Forms.ToolStripButton toolBarServerLogButton;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.OpenFileDialog importCertificateFileDialog;
        private System.Windows.Forms.SaveFileDialog exportCertificateFileDialog;
        private System.Windows.Forms.Panel viewPanel;
        private System.Windows.Forms.ToolStripMenuItem storeInDatabaseMenuItem;
        private System.Windows.Forms.ToolStripMenuItem registrationAuthorityMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel statusBarStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel statusBarCertLabel;
        private System.Windows.Forms.ToolStripStatusLabel statusBarDatabaseLabel;
        private System.Windows.Forms.ToolStripButton toolBarRegistrationAuthorityButton;
    }
}

