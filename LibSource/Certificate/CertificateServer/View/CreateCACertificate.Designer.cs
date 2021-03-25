namespace CrypTool.CertificateServer.View
{
    partial class CreateCACertificate
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
            this.createCertGroupBox = new System.Windows.Forms.GroupBox();
            this.createCertTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.nameLabel = new System.Windows.Forms.Label();
            this.organisationLabel = new System.Windows.Forms.Label();
            this.organisationalUnitLabel = new System.Windows.Forms.Label();
            this.countryLabel = new System.Windows.Forms.Label();
            this.emailLabel = new System.Windows.Forms.Label();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.confirmPasswordLabel = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.organisationTextBox = new System.Windows.Forms.TextBox();
            this.organisationalUnitTextBox = new System.Windows.Forms.TextBox();
            this.countryComboBox = new System.Windows.Forms.ComboBox();
            this.emailTextBox = new System.Windows.Forms.TextBox();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.confirmPasswordTextBox = new System.Windows.Forms.TextBox();
            this.createCertButton = new System.Windows.Forms.Button();
            this.processingIcon = new System.Windows.Forms.PictureBox();
            this.createCertGroupBox.SuspendLayout();
            this.createCertTableLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.processingIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // createCertGroupBox
            // 
            this.createCertGroupBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.createCertGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.createCertGroupBox.Controls.Add(this.createCertTableLayout);
            this.createCertGroupBox.Location = new System.Drawing.Point(0, 0);
            this.createCertGroupBox.Name = "createCertGroupBox";
            this.createCertGroupBox.Size = new System.Drawing.Size(600, 344);
            this.createCertGroupBox.TabIndex = 0;
            this.createCertGroupBox.TabStop = false;
            this.createCertGroupBox.Text = "Create new CA certificate";
            // 
            // createCertTableLayout
            // 
            this.createCertTableLayout.ColumnCount = 3;
            this.createCertTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.createCertTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 320F));
            this.createCertTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.createCertTableLayout.Controls.Add(this.nameLabel, 0, 1);
            this.createCertTableLayout.Controls.Add(this.organisationLabel, 0, 2);
            this.createCertTableLayout.Controls.Add(this.organisationalUnitLabel, 0, 3);
            this.createCertTableLayout.Controls.Add(this.countryLabel, 0, 4);
            this.createCertTableLayout.Controls.Add(this.emailLabel, 0, 5);
            this.createCertTableLayout.Controls.Add(this.passwordLabel, 0, 6);
            this.createCertTableLayout.Controls.Add(this.confirmPasswordLabel, 0, 7);
            this.createCertTableLayout.Controls.Add(this.statusLabel, 1, 0);
            this.createCertTableLayout.Controls.Add(this.nameTextBox, 1, 1);
            this.createCertTableLayout.Controls.Add(this.organisationTextBox, 1, 2);
            this.createCertTableLayout.Controls.Add(this.organisationalUnitTextBox, 1, 3);
            this.createCertTableLayout.Controls.Add(this.countryComboBox, 1, 4);
            this.createCertTableLayout.Controls.Add(this.emailTextBox, 1, 5);
            this.createCertTableLayout.Controls.Add(this.passwordTextBox, 1, 6);
            this.createCertTableLayout.Controls.Add(this.confirmPasswordTextBox, 1, 7);
            this.createCertTableLayout.Controls.Add(this.createCertButton, 1, 8);
            this.createCertTableLayout.Controls.Add(this.processingIcon, 2, 1);
            this.createCertTableLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.createCertTableLayout.Location = new System.Drawing.Point(3, 16);
            this.createCertTableLayout.Name = "createCertTableLayout";
            this.createCertTableLayout.RowCount = 9;
            this.createCertTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.createCertTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.createCertTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.createCertTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.createCertTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.createCertTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.createCertTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.createCertTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.createCertTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.createCertTableLayout.Size = new System.Drawing.Size(594, 322);
            this.createCertTableLayout.TabIndex = 0;
            // 
            // nameLabel
            // 
            this.nameLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(3, 33);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(82, 13);
            this.nameLabel.TabIndex = 0;
            this.nameLabel.Text = "Common Name:";
            // 
            // organisationLabel
            // 
            this.organisationLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.organisationLabel.AutoSize = true;
            this.organisationLabel.Location = new System.Drawing.Point(3, 63);
            this.organisationLabel.Name = "organisationLabel";
            this.organisationLabel.Size = new System.Drawing.Size(69, 13);
            this.organisationLabel.TabIndex = 1;
            this.organisationLabel.Text = "Organisation:";
            // 
            // organisationalUnitLabel
            // 
            this.organisationalUnitLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.organisationalUnitLabel.AutoSize = true;
            this.organisationalUnitLabel.Location = new System.Drawing.Point(3, 93);
            this.organisationalUnitLabel.Name = "organisationalUnitLabel";
            this.organisationalUnitLabel.Size = new System.Drawing.Size(99, 13);
            this.organisationalUnitLabel.TabIndex = 2;
            this.organisationalUnitLabel.Text = "Organisational Unit:";
            // 
            // countryLabel
            // 
            this.countryLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.countryLabel.AutoSize = true;
            this.countryLabel.Location = new System.Drawing.Point(3, 123);
            this.countryLabel.Name = "countryLabel";
            this.countryLabel.Size = new System.Drawing.Size(46, 13);
            this.countryLabel.TabIndex = 3;
            this.countryLabel.Text = "Country:";
            // 
            // emailLabel
            // 
            this.emailLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.emailLabel.AutoSize = true;
            this.emailLabel.Location = new System.Drawing.Point(3, 153);
            this.emailLabel.Name = "emailLabel";
            this.emailLabel.Size = new System.Drawing.Size(35, 13);
            this.emailLabel.TabIndex = 4;
            this.emailLabel.Text = "Email:";
            // 
            // passwordLabel
            // 
            this.passwordLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Location = new System.Drawing.Point(3, 183);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(56, 13);
            this.passwordLabel.TabIndex = 5;
            this.passwordLabel.Text = "Password:";
            // 
            // confirmPasswordLabel
            // 
            this.confirmPasswordLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.confirmPasswordLabel.AutoSize = true;
            this.confirmPasswordLabel.Location = new System.Drawing.Point(3, 213);
            this.confirmPasswordLabel.Name = "confirmPasswordLabel";
            this.confirmPasswordLabel.Size = new System.Drawing.Size(94, 13);
            this.confirmPasswordLabel.TabIndex = 6;
            this.confirmPasswordLabel.Text = "Confirm Password:";
            // 
            // statusLabel
            // 
            this.statusLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(153, 6);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 13);
            this.statusLabel.TabIndex = 7;
            // 
            // nameTextBox
            // 
            this.nameTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.nameTextBox.Location = new System.Drawing.Point(160, 30);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(300, 20);
            this.nameTextBox.TabIndex = 1;
            this.nameTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.nameTextBox_Validating);
            this.nameTextBox.Validated += new System.EventHandler(this.nameTextBox_Validated);
            // 
            // organisationTextBox
            // 
            this.organisationTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.organisationTextBox.Location = new System.Drawing.Point(160, 60);
            this.organisationTextBox.Name = "organisationTextBox";
            this.organisationTextBox.Size = new System.Drawing.Size(300, 20);
            this.organisationTextBox.TabIndex = 2;
            this.organisationTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.organisationTextBox_Validating);
            this.organisationTextBox.Validated += new System.EventHandler(this.organisationTextBox_Validated);
            // 
            // organisationalUnitTextBox
            // 
            this.organisationalUnitTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.organisationalUnitTextBox.Location = new System.Drawing.Point(160, 90);
            this.organisationalUnitTextBox.Name = "organisationalUnitTextBox";
            this.organisationalUnitTextBox.Size = new System.Drawing.Size(300, 20);
            this.organisationalUnitTextBox.TabIndex = 3;
            this.organisationalUnitTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.organisationalUnitTextBox_Validating);
            this.organisationalUnitTextBox.Validated += new System.EventHandler(this.organisationalUnitTextBox_Validated);
            // 
            // countryComboBox
            // 
            this.countryComboBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.countryComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.countryComboBox.Location = new System.Drawing.Point(160, 119);
            this.countryComboBox.Name = "countryComboBox";
            this.countryComboBox.Size = new System.Drawing.Size(300, 21);
            this.countryComboBox.Sorted = true;
            this.countryComboBox.TabIndex = 4;
            // 
            // emailTextBox
            // 
            this.emailTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.emailTextBox.Location = new System.Drawing.Point(160, 150);
            this.emailTextBox.Name = "emailTextBox";
            this.emailTextBox.Size = new System.Drawing.Size(300, 20);
            this.emailTextBox.TabIndex = 5;
            this.emailTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.emailTextBox_Validating);
            this.emailTextBox.Validated += new System.EventHandler(this.emailTextBox_Validated);
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.passwordTextBox.Location = new System.Drawing.Point(160, 180);
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.Size = new System.Drawing.Size(300, 20);
            this.passwordTextBox.TabIndex = 6;
            this.passwordTextBox.UseSystemPasswordChar = true;
            // 
            // confirmPasswordTextBox
            // 
            this.confirmPasswordTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.confirmPasswordTextBox.Location = new System.Drawing.Point(160, 210);
            this.confirmPasswordTextBox.Name = "confirmPasswordTextBox";
            this.confirmPasswordTextBox.Size = new System.Drawing.Size(300, 20);
            this.confirmPasswordTextBox.TabIndex = 7;
            this.confirmPasswordTextBox.UseSystemPasswordChar = true;
            this.confirmPasswordTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.passwordTextBox_Validating);
            this.confirmPasswordTextBox.Validated += new System.EventHandler(this.passwordTextBox_Validated);
            // 
            // createCertButton
            // 
            this.createCertButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.createCertButton.AutoSize = true;
            this.createCertButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.createCertButton.Location = new System.Drawing.Point(252, 240);
            this.createCertButton.Margin = new System.Windows.Forms.Padding(5);
            this.createCertButton.Name = "createCertButton";
            this.createCertButton.Size = new System.Drawing.Size(115, 23);
            this.createCertButton.TabIndex = 8;
            this.createCertButton.Text = "Create CA Certificate";
            this.createCertButton.UseVisualStyleBackColor = true;
            this.createCertButton.Click += new System.EventHandler(this.createCertButton_Click);
            // 
            // processingIcon
            // 
            this.processingIcon.Image = global::CrypTool.CertificateServer.Properties.Resources.processing;
            this.processingIcon.Location = new System.Drawing.Point(473, 28);
            this.processingIcon.Name = "processingIcon";
            this.createCertTableLayout.SetRowSpan(this.processingIcon, 8);
            this.processingIcon.Size = new System.Drawing.Size(32, 32);
            this.processingIcon.TabIndex = 9;
            this.processingIcon.TabStop = false;
            // 
            // CreateCACertificate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.createCertGroupBox);
            this.Name = "CreateCACertificate";
            this.Size = new System.Drawing.Size(600, 350);
            this.createCertGroupBox.ResumeLayout(false);
            this.createCertTableLayout.ResumeLayout(false);
            this.createCertTableLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.processingIcon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox createCertGroupBox;
        private System.Windows.Forms.TableLayoutPanel createCertTableLayout;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.Label emailLabel;
        private System.Windows.Forms.Label organisationLabel;
        private System.Windows.Forms.Label organisationalUnitLabel;
        private System.Windows.Forms.Label countryLabel;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.Label confirmPasswordLabel;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.TextBox emailTextBox;
        private System.Windows.Forms.TextBox organisationTextBox;
        private System.Windows.Forms.TextBox organisationalUnitTextBox;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.TextBox confirmPasswordTextBox;
        private System.Windows.Forms.ComboBox countryComboBox;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Button createCertButton;
        private System.Windows.Forms.PictureBox processingIcon;
    }
}
