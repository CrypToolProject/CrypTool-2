namespace CrypTool.CertificateServer.View
{
    partial class ServerStatus
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
            this.serverStatusGroupBox = new System.Windows.Forms.GroupBox();
            this.serverStatusTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.serverStatusLabel = new System.Windows.Forms.Label();
            this.serverStatusValue = new System.Windows.Forms.Label();
            this.databaseLabel = new System.Windows.Forms.Label();
            this.databaseValue = new System.Windows.Forms.Label();
            this.tlsLabel = new System.Windows.Forms.Label();
            this.tlsValue = new System.Windows.Forms.Label();
            this.databaseInfoLabel = new System.Windows.Forms.Label();
            this.databaseInfoValue = new System.Windows.Forms.Label();
            this.certAuthGroupBox = new System.Windows.Forms.GroupBox();
            this.certAuthTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.registeredPeersLabel = new System.Windows.Forms.Label();
            this.registeredPeersValue = new System.Windows.Forms.Label();
            this.lastRegisterLabel = new System.Windows.Forms.Label();
            this.lastRegisterValue = new System.Windows.Forms.Label();
            this.caCertGroupBox = new System.Windows.Forms.GroupBox();
            this.caCertInfoTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.nameLabel = new System.Windows.Forms.Label();
            this.nameValue = new System.Windows.Forms.Label();
            this.organisationLabel = new System.Windows.Forms.Label();
            this.organisationValue = new System.Windows.Forms.Label();
            this.organisationalUnitLabel = new System.Windows.Forms.Label();
            this.organisationalUnitValue = new System.Windows.Forms.Label();
            this.countryLabel = new System.Windows.Forms.Label();
            this.countryValue = new System.Windows.Forms.Label();
            this.emailLabel = new System.Windows.Forms.Label();
            this.emailValue = new System.Windows.Forms.Label();
            this.dateOfIssueLabel = new System.Windows.Forms.Label();
            this.dateOfIssueValue = new System.Windows.Forms.Label();
            this.dateOfExpireLabel = new System.Windows.Forms.Label();
            this.dateOfExpireValue = new System.Windows.Forms.Label();
            this.serverStatusGroupBox.SuspendLayout();
            this.serverStatusTableLayout.SuspendLayout();
            this.certAuthGroupBox.SuspendLayout();
            this.certAuthTableLayout.SuspendLayout();
            this.caCertGroupBox.SuspendLayout();
            this.caCertInfoTableLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // serverStatusGroupBox
            // 
            this.serverStatusGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.serverStatusGroupBox.Controls.Add(this.serverStatusTableLayout);
            this.serverStatusGroupBox.Location = new System.Drawing.Point(0, 0);
            this.serverStatusGroupBox.Name = "serverStatusGroupBox";
            this.serverStatusGroupBox.Size = new System.Drawing.Size(600, 80);
            this.serverStatusGroupBox.TabIndex = 0;
            this.serverStatusGroupBox.TabStop = false;
            this.serverStatusGroupBox.Text = "Server Status";
            // 
            // serverStatusTableLayout
            // 
            this.serverStatusTableLayout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.serverStatusTableLayout.ColumnCount = 4;
            this.serverStatusTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.serverStatusTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.serverStatusTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.serverStatusTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.serverStatusTableLayout.Controls.Add(this.serverStatusLabel, 0, 0);
            this.serverStatusTableLayout.Controls.Add(this.serverStatusValue, 1, 0);
            this.serverStatusTableLayout.Controls.Add(this.databaseLabel, 2, 0);
            this.serverStatusTableLayout.Controls.Add(this.databaseValue, 3, 0);
            this.serverStatusTableLayout.Controls.Add(this.tlsLabel, 0, 1);
            this.serverStatusTableLayout.Controls.Add(this.tlsValue, 1, 1);
            this.serverStatusTableLayout.Controls.Add(this.databaseInfoLabel, 2, 1);
            this.serverStatusTableLayout.Controls.Add(this.databaseInfoValue, 3, 1);
            this.serverStatusTableLayout.Location = new System.Drawing.Point(3, 16);
            this.serverStatusTableLayout.Name = "serverStatusTableLayout";
            this.serverStatusTableLayout.RowCount = 2;
            this.serverStatusTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.serverStatusTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.serverStatusTableLayout.Size = new System.Drawing.Size(594, 60);
            this.serverStatusTableLayout.TabIndex = 0;
            // 
            // serverStatusLabel
            // 
            this.serverStatusLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.serverStatusLabel.AutoSize = true;
            this.serverStatusLabel.Location = new System.Drawing.Point(3, 8);
            this.serverStatusLabel.Name = "serverStatusLabel";
            this.serverStatusLabel.Size = new System.Drawing.Size(74, 13);
            this.serverStatusLabel.TabIndex = 0;
            this.serverStatusLabel.Text = "Server Status:";
            // 
            // serverStatusValue
            // 
            this.serverStatusValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.serverStatusValue.AutoSize = true;
            this.serverStatusValue.Location = new System.Drawing.Point(133, 8);
            this.serverStatusValue.Name = "serverStatusValue";
            this.serverStatusValue.Size = new System.Drawing.Size(47, 13);
            this.serverStatusValue.TabIndex = 1;
            this.serverStatusValue.Text = "Stopped";
            // 
            // databaseLabel
            // 
            this.databaseLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.databaseLabel.AutoSize = true;
            this.databaseLabel.Location = new System.Drawing.Point(300, 8);
            this.databaseLabel.Name = "databaseLabel";
            this.databaseLabel.Size = new System.Drawing.Size(56, 13);
            this.databaseLabel.TabIndex = 2;
            this.databaseLabel.Text = "Database:";
            // 
            // databaseValue
            // 
            this.databaseValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.databaseValue.AutoSize = true;
            this.databaseValue.Location = new System.Drawing.Point(430, 8);
            this.databaseValue.Name = "databaseValue";
            this.databaseValue.Size = new System.Drawing.Size(79, 13);
            this.databaseValue.TabIndex = 3;
            this.databaseValue.Text = "Not Connected";
            // 
            // tlsLabel
            // 
            this.tlsLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tlsLabel.AutoSize = true;
            this.tlsLabel.Location = new System.Drawing.Point(3, 38);
            this.tlsLabel.Name = "tlsLabel";
            this.tlsLabel.Size = new System.Drawing.Size(80, 13);
            this.tlsLabel.TabIndex = 4;
            this.tlsLabel.Text = "TLS Certificate:";
            // 
            // tlsValue
            // 
            this.tlsValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tlsValue.AutoSize = true;
            this.tlsValue.Location = new System.Drawing.Point(133, 38);
            this.tlsValue.Name = "tlsValue";
            this.tlsValue.Size = new System.Drawing.Size(59, 13);
            this.tlsValue.TabIndex = 5;
            this.tlsValue.Text = "Not loaded";
            // 
            // databaseInfoLabel
            // 
            this.databaseInfoLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.databaseInfoLabel.AutoSize = true;
            this.databaseInfoLabel.Location = new System.Drawing.Point(300, 38);
            this.databaseInfoLabel.Name = "databaseInfoLabel";
            this.databaseInfoLabel.Size = new System.Drawing.Size(114, 13);
            this.databaseInfoLabel.TabIndex = 6;
            this.databaseInfoLabel.Text = "Database Name/User:";
            // 
            // databaseInfoValue
            // 
            this.databaseInfoValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.databaseInfoValue.AutoSize = true;
            this.databaseInfoValue.Location = new System.Drawing.Point(430, 38);
            this.databaseInfoValue.Name = "databaseInfoValue";
            this.databaseInfoValue.Size = new System.Drawing.Size(24, 13);
            this.databaseInfoValue.TabIndex = 7;
            this.databaseInfoValue.Text = "n/a";
            // 
            // certAuthGroupBox
            // 
            this.certAuthGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.certAuthGroupBox.Controls.Add(this.certAuthTableLayout);
            this.certAuthGroupBox.Location = new System.Drawing.Point(0, 280);
            this.certAuthGroupBox.Name = "certAuthGroupBox";
            this.certAuthGroupBox.Size = new System.Drawing.Size(600, 50);
            this.certAuthGroupBox.TabIndex = 2;
            this.certAuthGroupBox.TabStop = false;
            this.certAuthGroupBox.Text = "Certification Authority";
            // 
            // certAuthTableLayout
            // 
            this.certAuthTableLayout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.certAuthTableLayout.ColumnCount = 4;
            this.certAuthTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.certAuthTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.certAuthTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.certAuthTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.certAuthTableLayout.Controls.Add(this.registeredPeersLabel, 0, 0);
            this.certAuthTableLayout.Controls.Add(this.registeredPeersValue, 1, 0);
            this.certAuthTableLayout.Controls.Add(this.lastRegisterLabel, 2, 0);
            this.certAuthTableLayout.Controls.Add(this.lastRegisterValue, 3, 0);
            this.certAuthTableLayout.Location = new System.Drawing.Point(3, 16);
            this.certAuthTableLayout.Name = "certAuthTableLayout";
            this.certAuthTableLayout.RowCount = 1;
            this.certAuthTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.certAuthTableLayout.Size = new System.Drawing.Size(594, 30);
            this.certAuthTableLayout.TabIndex = 0;
            // 
            // registeredPeersLabel
            // 
            this.registeredPeersLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.registeredPeersLabel.AutoSize = true;
            this.registeredPeersLabel.Location = new System.Drawing.Point(3, 8);
            this.registeredPeersLabel.Name = "registeredPeersLabel";
            this.registeredPeersLabel.Size = new System.Drawing.Size(90, 13);
            this.registeredPeersLabel.TabIndex = 0;
            this.registeredPeersLabel.Text = "Registered peers:";
            // 
            // registeredPeersValue
            // 
            this.registeredPeersValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.registeredPeersValue.AutoSize = true;
            this.registeredPeersValue.Location = new System.Drawing.Point(133, 8);
            this.registeredPeersValue.Name = "registeredPeersValue";
            this.registeredPeersValue.Size = new System.Drawing.Size(24, 13);
            this.registeredPeersValue.TabIndex = 1;
            this.registeredPeersValue.Text = "n/a";
            // 
            // lastRegisterLabel
            // 
            this.lastRegisterLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lastRegisterLabel.AutoSize = true;
            this.lastRegisterLabel.Location = new System.Drawing.Point(300, 8);
            this.lastRegisterLabel.Name = "lastRegisterLabel";
            this.lastRegisterLabel.Size = new System.Drawing.Size(101, 13);
            this.lastRegisterLabel.TabIndex = 2;
            this.lastRegisterLabel.Text = "Date of last register:";
            // 
            // lastRegisterValue
            // 
            this.lastRegisterValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lastRegisterValue.AutoSize = true;
            this.lastRegisterValue.Location = new System.Drawing.Point(430, 8);
            this.lastRegisterValue.Name = "lastRegisterValue";
            this.lastRegisterValue.Size = new System.Drawing.Size(24, 13);
            this.lastRegisterValue.TabIndex = 3;
            this.lastRegisterValue.Text = "n/a";
            // 
            // caCertGroupBox
            // 
            this.caCertGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.caCertGroupBox.Controls.Add(this.caCertInfoTableLayout);
            this.caCertGroupBox.Location = new System.Drawing.Point(0, 80);
            this.caCertGroupBox.Name = "caCertGroupBox";
            this.caCertGroupBox.Size = new System.Drawing.Size(600, 200);
            this.caCertGroupBox.TabIndex = 1;
            this.caCertGroupBox.TabStop = false;
            this.caCertGroupBox.Text = "CA Certificate";
            // 
            // caCertInfoTableLayout
            // 
            this.caCertInfoTableLayout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.caCertInfoTableLayout.ColumnCount = 4;
            this.caCertInfoTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.caCertInfoTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.caCertInfoTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.caCertInfoTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.caCertInfoTableLayout.Controls.Add(this.nameLabel, 0, 0);
            this.caCertInfoTableLayout.Controls.Add(this.nameValue, 1, 0);
            this.caCertInfoTableLayout.Controls.Add(this.organisationLabel, 0, 1);
            this.caCertInfoTableLayout.Controls.Add(this.organisationValue, 1, 1);
            this.caCertInfoTableLayout.Controls.Add(this.organisationalUnitLabel, 0, 2);
            this.caCertInfoTableLayout.Controls.Add(this.organisationalUnitValue, 1, 2);
            this.caCertInfoTableLayout.Controls.Add(this.countryLabel, 0, 3);
            this.caCertInfoTableLayout.Controls.Add(this.countryValue, 1, 3);
            this.caCertInfoTableLayout.Controls.Add(this.emailLabel, 0, 4);
            this.caCertInfoTableLayout.Controls.Add(this.emailValue, 1, 4);
            this.caCertInfoTableLayout.Controls.Add(this.dateOfIssueLabel, 0, 5);
            this.caCertInfoTableLayout.Controls.Add(this.dateOfIssueValue, 1, 5);
            this.caCertInfoTableLayout.Controls.Add(this.dateOfExpireLabel, 2, 5);
            this.caCertInfoTableLayout.Controls.Add(this.dateOfExpireValue, 3, 5);
            this.caCertInfoTableLayout.Location = new System.Drawing.Point(3, 16);
            this.caCertInfoTableLayout.Name = "caCertInfoTableLayout";
            this.caCertInfoTableLayout.RowCount = 6;
            this.caCertInfoTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.caCertInfoTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.caCertInfoTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.caCertInfoTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.caCertInfoTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.caCertInfoTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.caCertInfoTableLayout.Size = new System.Drawing.Size(594, 181);
            this.caCertInfoTableLayout.TabIndex = 0;
            // 
            // nameLabel
            // 
            this.nameLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(3, 8);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(79, 13);
            this.nameLabel.TabIndex = 0;
            this.nameLabel.Text = "Common Name";
            // 
            // nameValue
            // 
            this.nameValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nameValue.AutoSize = true;
            this.caCertInfoTableLayout.SetColumnSpan(this.nameValue, 3);
            this.nameValue.Location = new System.Drawing.Point(133, 8);
            this.nameValue.Name = "nameValue";
            this.nameValue.Size = new System.Drawing.Size(24, 13);
            this.nameValue.TabIndex = 1;
            this.nameValue.Text = "n/a";
            // 
            // organisationLabel
            // 
            this.organisationLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.organisationLabel.AutoSize = true;
            this.organisationLabel.Location = new System.Drawing.Point(3, 38);
            this.organisationLabel.Name = "organisationLabel";
            this.organisationLabel.Size = new System.Drawing.Size(66, 13);
            this.organisationLabel.TabIndex = 2;
            this.organisationLabel.Text = "Organisation";
            // 
            // organisationValue
            // 
            this.organisationValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.organisationValue.AutoSize = true;
            this.caCertInfoTableLayout.SetColumnSpan(this.organisationValue, 3);
            this.organisationValue.Location = new System.Drawing.Point(133, 38);
            this.organisationValue.Name = "organisationValue";
            this.organisationValue.Size = new System.Drawing.Size(24, 13);
            this.organisationValue.TabIndex = 3;
            this.organisationValue.Text = "n/a";
            // 
            // organisationalUnitLabel
            // 
            this.organisationalUnitLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.organisationalUnitLabel.AutoSize = true;
            this.organisationalUnitLabel.Location = new System.Drawing.Point(3, 68);
            this.organisationalUnitLabel.Name = "organisationalUnitLabel";
            this.organisationalUnitLabel.Size = new System.Drawing.Size(96, 13);
            this.organisationalUnitLabel.TabIndex = 4;
            this.organisationalUnitLabel.Text = "Organisational Unit";
            // 
            // organisationalUnitValue
            // 
            this.organisationalUnitValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.organisationalUnitValue.AutoSize = true;
            this.caCertInfoTableLayout.SetColumnSpan(this.organisationalUnitValue, 3);
            this.organisationalUnitValue.Location = new System.Drawing.Point(133, 68);
            this.organisationalUnitValue.Name = "organisationalUnitValue";
            this.organisationalUnitValue.Size = new System.Drawing.Size(24, 13);
            this.organisationalUnitValue.TabIndex = 5;
            this.organisationalUnitValue.Text = "n/a";
            // 
            // countryLabel
            // 
            this.countryLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.countryLabel.AutoSize = true;
            this.countryLabel.Location = new System.Drawing.Point(3, 98);
            this.countryLabel.Name = "countryLabel";
            this.countryLabel.Size = new System.Drawing.Size(43, 13);
            this.countryLabel.TabIndex = 6;
            this.countryLabel.Text = "Country";
            // 
            // countryValue
            // 
            this.countryValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.countryValue.AutoSize = true;
            this.caCertInfoTableLayout.SetColumnSpan(this.countryValue, 3);
            this.countryValue.Location = new System.Drawing.Point(133, 98);
            this.countryValue.Name = "countryValue";
            this.countryValue.Size = new System.Drawing.Size(24, 13);
            this.countryValue.TabIndex = 7;
            this.countryValue.Text = "n/a";
            // 
            // emailLabel
            // 
            this.emailLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.emailLabel.AutoSize = true;
            this.emailLabel.Location = new System.Drawing.Point(3, 128);
            this.emailLabel.Name = "emailLabel";
            this.emailLabel.Size = new System.Drawing.Size(32, 13);
            this.emailLabel.TabIndex = 8;
            this.emailLabel.Text = "Email";
            // 
            // emailValue
            // 
            this.emailValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.emailValue.AutoSize = true;
            this.caCertInfoTableLayout.SetColumnSpan(this.emailValue, 3);
            this.emailValue.Location = new System.Drawing.Point(133, 128);
            this.emailValue.Name = "emailValue";
            this.emailValue.Size = new System.Drawing.Size(24, 13);
            this.emailValue.TabIndex = 9;
            this.emailValue.Text = "n/a";
            // 
            // dateOfIssueLabel
            // 
            this.dateOfIssueLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.dateOfIssueLabel.AutoSize = true;
            this.dateOfIssueLabel.Location = new System.Drawing.Point(3, 159);
            this.dateOfIssueLabel.Name = "dateOfIssueLabel";
            this.dateOfIssueLabel.Size = new System.Drawing.Size(72, 13);
            this.dateOfIssueLabel.TabIndex = 10;
            this.dateOfIssueLabel.Text = "Date of issue:";
            // 
            // dateOfIssueValue
            // 
            this.dateOfIssueValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.dateOfIssueValue.AutoSize = true;
            this.dateOfIssueValue.Location = new System.Drawing.Point(133, 159);
            this.dateOfIssueValue.Name = "dateOfIssueValue";
            this.dateOfIssueValue.Size = new System.Drawing.Size(24, 13);
            this.dateOfIssueValue.TabIndex = 11;
            this.dateOfIssueValue.Text = "n/a";
            // 
            // dateOfExpireLabel
            // 
            this.dateOfExpireLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.dateOfExpireLabel.AutoSize = true;
            this.dateOfExpireLabel.Location = new System.Drawing.Point(300, 159);
            this.dateOfExpireLabel.Name = "dateOfExpireLabel";
            this.dateOfExpireLabel.Size = new System.Drawing.Size(76, 13);
            this.dateOfExpireLabel.TabIndex = 12;
            this.dateOfExpireLabel.Text = "Date of expire:";
            // 
            // dateOfExpireValue
            // 
            this.dateOfExpireValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.dateOfExpireValue.AutoSize = true;
            this.dateOfExpireValue.Location = new System.Drawing.Point(430, 159);
            this.dateOfExpireValue.Name = "dateOfExpireValue";
            this.dateOfExpireValue.Size = new System.Drawing.Size(24, 13);
            this.dateOfExpireValue.TabIndex = 13;
            this.dateOfExpireValue.Text = "n/a";
            // 
            // ServerStatus
            // 
            this.AutoSize = true;
            this.Controls.Add(this.serverStatusGroupBox);
            this.Controls.Add(this.caCertGroupBox);
            this.Controls.Add(this.certAuthGroupBox);
            this.Name = "ServerStatus";
            this.Size = new System.Drawing.Size(603, 350);
            this.serverStatusGroupBox.ResumeLayout(false);
            this.serverStatusTableLayout.ResumeLayout(false);
            this.serverStatusTableLayout.PerformLayout();
            this.certAuthGroupBox.ResumeLayout(false);
            this.certAuthTableLayout.ResumeLayout(false);
            this.certAuthTableLayout.PerformLayout();
            this.caCertGroupBox.ResumeLayout(false);
            this.caCertInfoTableLayout.ResumeLayout(false);
            this.caCertInfoTableLayout.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox serverStatusGroupBox;
        private System.Windows.Forms.TableLayoutPanel serverStatusTableLayout;
        private System.Windows.Forms.Label serverStatusValue;
        private System.Windows.Forms.Label serverStatusLabel;
        private System.Windows.Forms.Label databaseValue;
        private System.Windows.Forms.Label databaseLabel;
        private System.Windows.Forms.Label tlsValue;
        private System.Windows.Forms.Label tlsLabel;
        private System.Windows.Forms.Label databaseInfoValue;
        private System.Windows.Forms.Label databaseInfoLabel;
        private System.Windows.Forms.GroupBox caCertGroupBox;
        private System.Windows.Forms.TableLayoutPanel caCertInfoTableLayout;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.Label nameValue;
        private System.Windows.Forms.Label organisationLabel;
        private System.Windows.Forms.Label organisationValue;
        private System.Windows.Forms.Label organisationalUnitLabel;
        private System.Windows.Forms.Label organisationalUnitValue;
        private System.Windows.Forms.Label countryLabel;
        private System.Windows.Forms.Label countryValue;
        private System.Windows.Forms.Label emailLabel;
        private System.Windows.Forms.Label emailValue;
        private System.Windows.Forms.Label dateOfIssueLabel;
        private System.Windows.Forms.Label dateOfIssueValue;
        private System.Windows.Forms.Label dateOfExpireLabel;
        private System.Windows.Forms.Label dateOfExpireValue;
        private System.Windows.Forms.GroupBox certAuthGroupBox;
        private System.Windows.Forms.TableLayoutPanel certAuthTableLayout;
        private System.Windows.Forms.Label registeredPeersLabel;
        private System.Windows.Forms.Label registeredPeersValue;
        private System.Windows.Forms.Label lastRegisterLabel;
        private System.Windows.Forms.Label lastRegisterValue;
    }
}
