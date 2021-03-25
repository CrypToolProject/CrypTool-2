namespace CrypTool.CertificateServer.View
{
    partial class ConfirmDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.confirmFlowLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.questionPanel = new System.Windows.Forms.Panel();
            this.questionLabel = new System.Windows.Forms.Label();
            this.textBox = new System.Windows.Forms.RichTextBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.confirmFlowLayout.SuspendLayout();
            this.questionPanel.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // confirmFlowLayout
            // 
            this.confirmFlowLayout.Controls.Add(this.questionPanel);
            this.confirmFlowLayout.Controls.Add(this.textBox);
            this.confirmFlowLayout.Controls.Add(this.flowLayoutPanel1);
            this.confirmFlowLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.confirmFlowLayout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.confirmFlowLayout.Location = new System.Drawing.Point(0, 0);
            this.confirmFlowLayout.Name = "confirmFlowLayout";
            this.confirmFlowLayout.Size = new System.Drawing.Size(484, 292);
            this.confirmFlowLayout.TabIndex = 0;
            // 
            // questionPanel
            // 
            this.questionPanel.Controls.Add(this.questionLabel);
            this.questionPanel.Location = new System.Drawing.Point(3, 3);
            this.questionPanel.Name = "questionPanel";
            this.questionPanel.Size = new System.Drawing.Size(481, 60);
            this.questionPanel.TabIndex = 0;
            // 
            // questionLabel
            // 
            this.questionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.questionLabel.AutoSize = true;
            this.questionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.questionLabel.Location = new System.Drawing.Point(9, 11);
            this.questionLabel.Margin = new System.Windows.Forms.Padding(5);
            this.questionLabel.Name = "questionLabel";
            this.questionLabel.Size = new System.Drawing.Size(56, 15);
            this.questionLabel.TabIndex = 0;
            this.questionLabel.Text = "Question";
            // 
            // textBox
            // 
            this.textBox.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.textBox.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.textBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox.Location = new System.Drawing.Point(24, 69);
            this.textBox.Name = "textBox";
            this.textBox.ReadOnly = true;
            this.textBox.Size = new System.Drawing.Size(439, 174);
            this.textBox.TabIndex = 1;
            this.textBox.Text = "";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.flowLayoutPanel1.Controls.Add(this.button1);
            this.flowLayoutPanel1.Controls.Add(this.button2);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(21, 249);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(444, 31);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(366, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "&OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OK_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(285, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "&Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // ConfirmDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 292);
            this.Controls.Add(this.confirmFlowLayout);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(500, 330);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 330);
            this.Name = "ConfirmDialog";
            this.Text = "Title";
            this.confirmFlowLayout.ResumeLayout(false);
            this.questionPanel.ResumeLayout(false);
            this.questionPanel.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel confirmFlowLayout;
        private System.Windows.Forms.Panel questionPanel;
        private System.Windows.Forms.Label questionLabel;
        private System.Windows.Forms.RichTextBox textBox;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}