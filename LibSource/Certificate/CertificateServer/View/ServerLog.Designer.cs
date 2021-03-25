namespace CrypTool.CertificateServer.View
{
    partial class ServerLog
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
            this.logPrefPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.logViewErrorButton = new System.Windows.Forms.Button();
            this.logViewWarningButton = new System.Windows.Forms.Button();
            this.logViewInfoButton = new System.Windows.Forms.Button();
            this.logViewDebugButton = new System.Windows.Forms.Button();
            this.messageDetailsView = new System.Windows.Forms.RichTextBox();
            this.splitLogContainer = new System.Windows.Forms.SplitContainer();
            this.messageView = new System.Windows.Forms.ListView();
            this.emptyHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.timeStampHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.typeHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.titleHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.messageHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.logPrefPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitLogContainer)).BeginInit();
            this.splitLogContainer.Panel1.SuspendLayout();
            this.splitLogContainer.Panel2.SuspendLayout();
            this.splitLogContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // logPrefPanel
            // 
            this.logPrefPanel.AutoSize = true;
            this.logPrefPanel.Controls.Add(this.logViewErrorButton);
            this.logPrefPanel.Controls.Add(this.logViewWarningButton);
            this.logPrefPanel.Controls.Add(this.logViewInfoButton);
            this.logPrefPanel.Controls.Add(this.logViewDebugButton);
            this.logPrefPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.logPrefPanel.Location = new System.Drawing.Point(0, 0);
            this.logPrefPanel.Name = "logPrefPanel";
            this.logPrefPanel.Size = new System.Drawing.Size(600, 29);
            this.logPrefPanel.TabIndex = 1;
            // 
            // logViewErrorButton
            // 
            this.logViewErrorButton.BackColor = System.Drawing.Color.LightCoral;
            this.logViewErrorButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.logViewErrorButton.Location = new System.Drawing.Point(3, 3);
            this.logViewErrorButton.Name = "logViewErrorButton";
            this.logViewErrorButton.Size = new System.Drawing.Size(75, 23);
            this.logViewErrorButton.TabIndex = 0;
            this.logViewErrorButton.Text = "Error";
            this.logViewErrorButton.UseVisualStyleBackColor = false;
            this.logViewErrorButton.Click += new System.EventHandler(this.logViewErrorButton_Click);
            // 
            // logViewWarningButton
            // 
            this.logViewWarningButton.BackColor = System.Drawing.Color.SandyBrown;
            this.logViewWarningButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.logViewWarningButton.Location = new System.Drawing.Point(84, 3);
            this.logViewWarningButton.Name = "logViewWarningButton";
            this.logViewWarningButton.Size = new System.Drawing.Size(75, 23);
            this.logViewWarningButton.TabIndex = 1;
            this.logViewWarningButton.Text = "Warning";
            this.logViewWarningButton.UseVisualStyleBackColor = false;
            this.logViewWarningButton.Click += new System.EventHandler(this.logViewWarningButton_Click);
            // 
            // logViewInfoButton
            // 
            this.logViewInfoButton.BackColor = System.Drawing.Color.Khaki;
            this.logViewInfoButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.logViewInfoButton.Location = new System.Drawing.Point(165, 3);
            this.logViewInfoButton.Name = "logViewInfoButton";
            this.logViewInfoButton.Size = new System.Drawing.Size(75, 23);
            this.logViewInfoButton.TabIndex = 2;
            this.logViewInfoButton.Text = "Info";
            this.logViewInfoButton.UseVisualStyleBackColor = false;
            this.logViewInfoButton.Click += new System.EventHandler(this.logViewInfoButton_Click);
            // 
            // logViewDebugButton
            // 
            this.logViewDebugButton.BackColor = System.Drawing.Color.LightSteelBlue;
            this.logViewDebugButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.logViewDebugButton.Location = new System.Drawing.Point(246, 3);
            this.logViewDebugButton.Name = "logViewDebugButton";
            this.logViewDebugButton.Size = new System.Drawing.Size(75, 23);
            this.logViewDebugButton.TabIndex = 3;
            this.logViewDebugButton.Text = "Debug";
            this.logViewDebugButton.UseVisualStyleBackColor = false;
            this.logViewDebugButton.Click += new System.EventHandler(this.logViewDebugButton_Click);
            // 
            // messageDetailsView
            // 
            this.messageDetailsView.BackColor = System.Drawing.SystemColors.Window;
            this.messageDetailsView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.messageDetailsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.messageDetailsView.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.messageDetailsView.Location = new System.Drawing.Point(0, 0);
            this.messageDetailsView.Name = "messageDetailsView";
            this.messageDetailsView.Size = new System.Drawing.Size(596, 155);
            this.messageDetailsView.TabIndex = 0;
            this.messageDetailsView.Text = "";
            this.messageDetailsView.WordWrap = false;
            // 
            // splitLogContainer
            // 
            this.splitLogContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitLogContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitLogContainer.Location = new System.Drawing.Point(0, 29);
            this.splitLogContainer.Name = "splitLogContainer";
            this.splitLogContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitLogContainer.Panel1
            // 
            this.splitLogContainer.Panel1.Controls.Add(this.messageView);
            // 
            // splitLogContainer.Panel2
            // 
            this.splitLogContainer.Panel2.Controls.Add(this.messageDetailsView);
            this.splitLogContainer.Size = new System.Drawing.Size(600, 321);
            this.splitLogContainer.SplitterDistance = 158;
            this.splitLogContainer.TabIndex = 0;
            // 
            // messageView
            // 
            this.messageView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.messageView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.emptyHeader,
            this.timeStampHeader,
            this.typeHeader,
            this.titleHeader,
            this.messageHeader});
            this.messageView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.messageView.FullRowSelect = true;
            this.messageView.GridLines = true;
            this.messageView.Location = new System.Drawing.Point(0, 0);
            this.messageView.MultiSelect = false;
            this.messageView.Name = "messageView";
            this.messageView.Size = new System.Drawing.Size(596, 154);
            this.messageView.TabIndex = 0;
            this.messageView.UseCompatibleStateImageBehavior = false;
            this.messageView.View = System.Windows.Forms.View.Details;
            this.messageView.SelectedIndexChanged += new System.EventHandler(this.SelectedMessageChanged);
            // 
            // emptyHeader
            // 
            this.emptyHeader.Text = "";
            this.emptyHeader.Width = 25;
            // 
            // timeStampHeader
            // 
            this.timeStampHeader.Text = "Time Stamp";
            this.timeStampHeader.Width = 150;
            // 
            // typeHeader
            // 
            this.typeHeader.Text = "Type";
            this.typeHeader.Width = 50;
            // 
            // titleHeader
            // 
            this.titleHeader.Text = "Header";
            this.titleHeader.Width = 150;
            // 
            // messageHeader
            // 
            this.messageHeader.Width = 220;
            // 
            // ServerLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.splitLogContainer);
            this.Controls.Add(this.logPrefPanel);
            this.Name = "ServerLog";
            this.Size = new System.Drawing.Size(600, 350);
            this.Resize += new System.EventHandler(this.ServerLog_Resize);
            this.logPrefPanel.ResumeLayout(false);
            this.splitLogContainer.Panel1.ResumeLayout(false);
            this.splitLogContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitLogContainer)).EndInit();
            this.splitLogContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel logPrefPanel;
        private System.Windows.Forms.Button logViewErrorButton;
        private System.Windows.Forms.Button logViewWarningButton;
        private System.Windows.Forms.Button logViewInfoButton;
        private System.Windows.Forms.Button logViewDebugButton;
        private System.Windows.Forms.ColumnHeader emptyHeader;
        private System.Windows.Forms.ColumnHeader timeStampHeader;
        private System.Windows.Forms.ColumnHeader typeHeader;
        private System.Windows.Forms.ColumnHeader titleHeader;
        private System.Windows.Forms.ColumnHeader messageHeader;
    }
}
