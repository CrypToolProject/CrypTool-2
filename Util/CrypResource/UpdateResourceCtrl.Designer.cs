namespace CrypTool.Resource
{
    partial class UpdateResourceCtrl
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
            this.bUpdateResource = new System.Windows.Forms.LinkLabel();
            this.bSelectResource = new System.Windows.Forms.Button();
            this.tbResourceFileName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.bSelectAssembly = new System.Windows.Forms.Button();
            this.tbAssemblyFileName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // bUpdateResource
            // 
            this.bUpdateResource.AutoSize = true;
            this.bUpdateResource.Enabled = false;
            this.bUpdateResource.Location = new System.Drawing.Point(5, 118);
            this.bUpdateResource.Name = "bUpdateResource";
            this.bUpdateResource.Size = new System.Drawing.Size(91, 13);
            this.bUpdateResource.TabIndex = 15;
            this.bUpdateResource.TabStop = true;
            this.bUpdateResource.Text = "Update Resource";
            this.bUpdateResource.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.bUpdateResource_LinkClicked);
            // 
            // bSelectResource
            // 
            this.bSelectResource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bSelectResource.Location = new System.Drawing.Point(250, 83);
            this.bSelectResource.Name = "bSelectResource";
            this.bSelectResource.Size = new System.Drawing.Size(31, 23);
            this.bSelectResource.TabIndex = 14;
            this.bSelectResource.Text = "...";
            this.bSelectResource.UseVisualStyleBackColor = true;
            this.bSelectResource.Click += new System.EventHandler(this.bSelectFolder_Click);
            // 
            // tbResourceFileName
            // 
            this.tbResourceFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbResourceFileName.Location = new System.Drawing.Point(8, 86);
            this.tbResourceFileName.Name = "tbResourceFileName";
            this.tbResourceFileName.ReadOnly = true;
            this.tbResourceFileName.Size = new System.Drawing.Size(236, 20);
            this.tbResourceFileName.TabIndex = 13;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(105, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Select Resource File";
            // 
            // bSelectAssembly
            // 
            this.bSelectAssembly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bSelectAssembly.Location = new System.Drawing.Point(250, 34);
            this.bSelectAssembly.Name = "bSelectAssembly";
            this.bSelectAssembly.Size = new System.Drawing.Size(31, 23);
            this.bSelectAssembly.TabIndex = 11;
            this.bSelectAssembly.Text = "...";
            this.bSelectAssembly.UseVisualStyleBackColor = true;
            this.bSelectAssembly.Click += new System.EventHandler(this.bSelectAssembly_Click);
            // 
            // tbAssemblyFileName
            // 
            this.tbAssemblyFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbAssemblyFileName.Location = new System.Drawing.Point(8, 36);
            this.tbAssemblyFileName.Name = "tbAssemblyFileName";
            this.tbAssemblyFileName.ReadOnly = true;
            this.tbAssemblyFileName.Size = new System.Drawing.Size(236, 20);
            this.tbAssemblyFileName.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Select Plugin";
            // 
            // UpdateResourceCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.bUpdateResource);
            this.Controls.Add(this.bSelectResource);
            this.Controls.Add(this.tbResourceFileName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.bSelectAssembly);
            this.Controls.Add(this.tbAssemblyFileName);
            this.Controls.Add(this.label1);
            this.Name = "UpdateResourceCtrl";
            this.Size = new System.Drawing.Size(293, 645);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.LinkLabel bUpdateResource;
        private System.Windows.Forms.Button bSelectResource;
        private System.Windows.Forms.TextBox tbResourceFileName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button bSelectAssembly;
        private System.Windows.Forms.TextBox tbAssemblyFileName;
        private System.Windows.Forms.Label label1;
    }
}
