namespace CrypTool.Resource
{
    partial class CreateResourceCtrl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.tbAssemblyFileName = new System.Windows.Forms.TextBox();
            this.bSelectAssembly = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tbResourceDirectory = new System.Windows.Forms.TextBox();
            this.bSelectFolder = new System.Windows.Forms.Button();
            this.bCreateResource = new System.Windows.Forms.LinkLabel();
            this.label3 = new System.Windows.Forms.Label();
            this.tbCountryCode = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select Plugin";
            // 
            // tbAssemblyFileName
            // 
            this.tbAssemblyFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbAssemblyFileName.Location = new System.Drawing.Point(8, 36);
            this.tbAssemblyFileName.Name = "tbAssemblyFileName";
            this.tbAssemblyFileName.ReadOnly = true;
            this.tbAssemblyFileName.Size = new System.Drawing.Size(236, 20);
            this.tbAssemblyFileName.TabIndex = 1;
            // 
            // bSelectAssembly
            // 
            this.bSelectAssembly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bSelectAssembly.Location = new System.Drawing.Point(250, 34);
            this.bSelectAssembly.Name = "bSelectAssembly";
            this.bSelectAssembly.Size = new System.Drawing.Size(31, 23);
            this.bSelectAssembly.TabIndex = 2;
            this.bSelectAssembly.Text = "...";
            this.bSelectAssembly.UseVisualStyleBackColor = true;
            this.bSelectAssembly.Click += new System.EventHandler(this.bSelectAssembly_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(154, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Set Default Resource Directory";
            // 
            // tbResourceDirectory
            // 
            this.tbResourceDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbResourceDirectory.Location = new System.Drawing.Point(8, 86);
            this.tbResourceDirectory.Name = "tbResourceDirectory";
            this.tbResourceDirectory.ReadOnly = true;
            this.tbResourceDirectory.Size = new System.Drawing.Size(236, 20);
            this.tbResourceDirectory.TabIndex = 4;
            // 
            // bSelectFolder
            // 
            this.bSelectFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bSelectFolder.Location = new System.Drawing.Point(250, 84);
            this.bSelectFolder.Name = "bSelectFolder";
            this.bSelectFolder.Size = new System.Drawing.Size(31, 23);
            this.bSelectFolder.TabIndex = 5;
            this.bSelectFolder.Text = "...";
            this.bSelectFolder.UseVisualStyleBackColor = true;
            this.bSelectFolder.Click += new System.EventHandler(this.bSelectFolder_Click);
            // 
            // bCreateResource
            // 
            this.bCreateResource.AutoSize = true;
            this.bCreateResource.Enabled = false;
            this.bCreateResource.Location = new System.Drawing.Point(5, 170);
            this.bCreateResource.Name = "bCreateResource";
            this.bCreateResource.Size = new System.Drawing.Size(87, 13);
            this.bCreateResource.TabIndex = 6;
            this.bCreateResource.TabStop = true;
            this.bCreateResource.Text = "Create Resource";
            this.bCreateResource.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.bCreateResource_Clicked);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 120);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(154, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Set Default Resource Directory";
            // 
            // tbCountryCode
            // 
            this.tbCountryCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCountryCode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tbCountryCode.FormattingEnabled = true;
            this.tbCountryCode.Location = new System.Drawing.Point(8, 136);
            this.tbCountryCode.Name = "tbCountryCode";
            this.tbCountryCode.Size = new System.Drawing.Size(236, 21);
            this.tbCountryCode.TabIndex = 8;
            // 
            // UpdateResourceCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tbCountryCode);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.bCreateResource);
            this.Controls.Add(this.bSelectFolder);
            this.Controls.Add(this.tbResourceDirectory);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.bSelectAssembly);
            this.Controls.Add(this.tbAssemblyFileName);
            this.Controls.Add(this.label1);
            this.Name = "UpdateResourceCtrl";
            this.Size = new System.Drawing.Size(293, 574);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbAssemblyFileName;
        private System.Windows.Forms.Button bSelectAssembly;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbResourceDirectory;
        private System.Windows.Forms.Button bSelectFolder;
        private System.Windows.Forms.LinkLabel bCreateResource;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox tbCountryCode;
    }
}
