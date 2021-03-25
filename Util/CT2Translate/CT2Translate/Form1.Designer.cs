namespace WindowsFormsApplication1
{
    partial class CT2Translate
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadMergedResourcesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recursiveDirectoryScanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveMergedResourcesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToBasepathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.panel3 = new System.Windows.Forms.Panel();
            this.nextmissButton = new System.Windows.Forms.Button();
            this.prevmissButton = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.basepathTextBox = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lang1TextBox = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lang2TextBox = new System.Windows.Forms.RichTextBox();
            this.lang3TextBox = new System.Windows.Forms.RichTextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.fileTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.keyTextBox = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.searchcaseBox = new System.Windows.Forms.CheckBox();
            this.ClearSearchButton = new System.Windows.Forms.Button();
            this.SearchButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.filterBox = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.fileTree = new System.Windows.Forms.TreeView();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.logBox = new System.Windows.Forms.RichTextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(956, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.quitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadMergedResourcesToolStripMenuItem,
            this.recursiveDirectoryScanToolStripMenuItem});
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.loadToolStripMenuItem.Text = "Load";
            // 
            // loadMergedResourcesToolStripMenuItem
            // 
            this.loadMergedResourcesToolStripMenuItem.Name = "loadMergedResourcesToolStripMenuItem";
            this.loadMergedResourcesToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.loadMergedResourcesToolStripMenuItem.Text = "Load merged resources";
            this.loadMergedResourcesToolStripMenuItem.Click += new System.EventHandler(this.loadMergedResourcesToolStripMenuItem_Click);
            // 
            // recursiveDirectoryScanToolStripMenuItem
            // 
            this.recursiveDirectoryScanToolStripMenuItem.Name = "recursiveDirectoryScanToolStripMenuItem";
            this.recursiveDirectoryScanToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.recursiveDirectoryScanToolStripMenuItem.Text = "Recursive directory scan";
            this.recursiveDirectoryScanToolStripMenuItem.Click += new System.EventHandler(this.recursiveDirectoryScanToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveMergedResourcesToolStripMenuItem,
            this.saveToBasepathToolStripMenuItem});
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.saveToolStripMenuItem.Text = "Save";
            // 
            // saveMergedResourcesToolStripMenuItem
            // 
            this.saveMergedResourcesToolStripMenuItem.Name = "saveMergedResourcesToolStripMenuItem";
            this.saveMergedResourcesToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.saveMergedResourcesToolStripMenuItem.Text = "Save merged resources";
            this.saveMergedResourcesToolStripMenuItem.Click += new System.EventHandler(this.saveMergedResourcesToolStripMenuItem_Click);
            // 
            // saveToBasepathToolStripMenuItem
            // 
            this.saveToBasepathToolStripMenuItem.Name = "saveToBasepathToolStripMenuItem";
            this.saveToBasepathToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.saveToBasepathToolStripMenuItem.Text = "Save to basepath";
            this.saveToBasepathToolStripMenuItem.Click += new System.EventHandler(this.saveToBasepathToolStripMenuItem_Click);
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.quitToolStripMenuItem.Text = "Quit";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 547);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(956, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.panel3);
            this.tabPage2.Controls.Add(this.panel2);
            this.tabPage2.Controls.Add(this.panel1);
            this.tabPage2.Controls.Add(this.splitContainer1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(948, 497);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Words";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.nextmissButton);
            this.panel3.Controls.Add(this.prevmissButton);
            this.panel3.Controls.Add(this.label7);
            this.panel3.Controls.Add(this.textBox2);
            this.panel3.Controls.Add(this.textBox1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(3, 357);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(942, 35);
            this.panel3.TabIndex = 14;
            // 
            // nextmissButton
            // 
            this.nextmissButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.nextmissButton.Enabled = false;
            this.nextmissButton.Location = new System.Drawing.Point(907, 3);
            this.nextmissButton.Name = "nextmissButton";
            this.nextmissButton.Size = new System.Drawing.Size(35, 23);
            this.nextmissButton.TabIndex = 5;
            this.nextmissButton.Text = ">>";
            this.nextmissButton.UseVisualStyleBackColor = true;
            this.nextmissButton.Click += new System.EventHandler(this.nextmissButton_Click);
            // 
            // prevmissButton
            // 
            this.prevmissButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.prevmissButton.Enabled = false;
            this.prevmissButton.Location = new System.Drawing.Point(870, 3);
            this.prevmissButton.Name = "prevmissButton";
            this.prevmissButton.Size = new System.Drawing.Size(35, 23);
            this.prevmissButton.TabIndex = 4;
            this.prevmissButton.Text = "<<";
            this.prevmissButton.UseVisualStyleBackColor = true;
            this.prevmissButton.Click += new System.EventHandler(this.prevmissButton_Click);
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(734, 6);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(71, 13);
            this.label7.TabIndex = 3;
            this.label7.Text = "missing items:";
            // 
            // textBox2
            // 
            this.textBox2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.textBox2.Location = new System.Drawing.Point(811, 3);
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(49, 20);
            this.textBox2.TabIndex = 2;
            this.textBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBox1
            // 
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Location = new System.Drawing.Point(3, 8);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(150, 13);
            this.textBox1.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label6);
            this.panel2.Controls.Add(this.basepathTextBox);
            this.panel2.Controls.Add(this.tableLayoutPanel1);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.fileTextBox);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.keyTextBox);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(3, 392);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(942, 102);
            this.panel2.TabIndex = 13;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(-1, 3);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(55, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "Basepath:";
            // 
            // basepathTextBox
            // 
            this.basepathTextBox.Location = new System.Drawing.Point(61, 0);
            this.basepathTextBox.Name = "basepathTextBox";
            this.basepathTextBox.Size = new System.Drawing.Size(213, 20);
            this.basepathTextBox.TabIndex = 17;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel1.Controls.Add(this.lang1TextBox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lang2TextBox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lang3TextBox, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label8, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 25);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20.77922F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 79.22078F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(942, 77);
            this.tableLayoutPanel1.TabIndex = 15;
            // 
            // lang1TextBox
            // 
            this.lang1TextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lang1TextBox.Location = new System.Drawing.Point(3, 18);
            this.lang1TextBox.Name = "lang1TextBox";
            this.lang1TextBox.Size = new System.Drawing.Size(308, 56);
            this.lang1TextBox.TabIndex = 12;
            this.lang1TextBox.Tag = "en";
            this.lang1TextBox.Text = "";
            this.lang1TextBox.TextChanged += new System.EventHandler(this.LangTextBox_Leave);
            this.lang1TextBox.Leave += new System.EventHandler(this.LangTextBox_Leave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Englisch:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(317, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Deutsch:";
            // 
            // lang2TextBox
            // 
            this.lang2TextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lang2TextBox.Location = new System.Drawing.Point(317, 18);
            this.lang2TextBox.Name = "lang2TextBox";
            this.lang2TextBox.Size = new System.Drawing.Size(308, 56);
            this.lang2TextBox.TabIndex = 7;
            this.lang2TextBox.Tag = "de";
            this.lang2TextBox.Text = "";
            this.lang2TextBox.TextChanged += new System.EventHandler(this.LangTextBox_Leave);
            this.lang2TextBox.Leave += new System.EventHandler(this.LangTextBox_Leave);
            // 
            // lang3TextBox
            // 
            this.lang3TextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lang3TextBox.Location = new System.Drawing.Point(631, 18);
            this.lang3TextBox.Name = "lang3TextBox";
            this.lang3TextBox.Size = new System.Drawing.Size(308, 56);
            this.lang3TextBox.TabIndex = 8;
            this.lang3TextBox.Tag = "ru";
            this.lang3TextBox.Text = "";
            this.lang3TextBox.TextChanged += new System.EventHandler(this.LangTextBox_Leave);
            this.lang3TextBox.Leave += new System.EventHandler(this.LangTextBox_Leave);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(631, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 13);
            this.label8.TabIndex = 11;
            this.label8.Text = "Russisch:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(280, 3);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Resource:";
            // 
            // fileTextBox
            // 
            this.fileTextBox.Location = new System.Drawing.Point(342, 0);
            this.fileTextBox.Name = "fileTextBox";
            this.fileTextBox.ReadOnly = true;
            this.fileTextBox.Size = new System.Drawing.Size(257, 20);
            this.fileTextBox.TabIndex = 14;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(605, 3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(28, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Key:";
            // 
            // keyTextBox
            // 
            this.keyTextBox.Location = new System.Drawing.Point(639, 0);
            this.keyTextBox.Name = "keyTextBox";
            this.keyTextBox.ReadOnly = true;
            this.keyTextBox.Size = new System.Drawing.Size(300, 20);
            this.keyTextBox.TabIndex = 12;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.searchcaseBox);
            this.panel1.Controls.Add(this.ClearSearchButton);
            this.panel1.Controls.Add(this.SearchButton);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.filterBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(942, 46);
            this.panel1.TabIndex = 6;
            // 
            // searchcaseBox
            // 
            this.searchcaseBox.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.searchcaseBox.AutoSize = true;
            this.searchcaseBox.Location = new System.Drawing.Point(669, 25);
            this.searchcaseBox.Name = "searchcaseBox";
            this.searchcaseBox.Size = new System.Drawing.Size(115, 17);
            this.searchcaseBox.TabIndex = 11;
            this.searchcaseBox.Text = "case sensitive filter";
            this.searchcaseBox.UseVisualStyleBackColor = true;
            // 
            // ClearSearchButton
            // 
            this.ClearSearchButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.ClearSearchButton.Location = new System.Drawing.Point(873, 0);
            this.ClearSearchButton.Name = "ClearSearchButton";
            this.ClearSearchButton.Size = new System.Drawing.Size(25, 22);
            this.ClearSearchButton.TabIndex = 10;
            this.ClearSearchButton.Text = "X";
            this.ClearSearchButton.UseVisualStyleBackColor = true;
            this.ClearSearchButton.Click += new System.EventHandler(this.ClearSearchButton_Click);
            // 
            // SearchButton
            // 
            this.SearchButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.SearchButton.Location = new System.Drawing.Point(901, 0);
            this.SearchButton.Name = "SearchButton";
            this.SearchButton.Size = new System.Drawing.Size(41, 22);
            this.SearchButton.TabIndex = 9;
            this.SearchButton.Text = "Apply";
            this.SearchButton.UseVisualStyleBackColor = true;
            this.SearchButton.Click += new System.EventHandler(this.SearchButton_Click);
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(636, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Filter:";
            // 
            // filterBox
            // 
            this.filterBox.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.filterBox.Location = new System.Drawing.Point(670, 0);
            this.filterBox.Name = "filterBox";
            this.filterBox.Size = new System.Drawing.Size(200, 20);
            this.filterBox.TabIndex = 7;
            this.filterBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.filterBox_KeyDown);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(3, 55);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.fileTree);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.listView1);
            this.splitContainer1.Size = new System.Drawing.Size(945, 296);
            this.splitContainer1.SplitterDistance = 239;
            this.splitContainer1.TabIndex = 5;
            // 
            // fileTree
            // 
            this.fileTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileTree.Location = new System.Drawing.Point(0, 0);
            this.fileTree.Name = "fileTree";
            this.fileTree.ShowNodeToolTips = true;
            this.fileTree.Size = new System.Drawing.Size(239, 296);
            this.fileTree.TabIndex = 2;
            this.fileTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.fileTree_AfterSelect);
            // 
            // listView1
            // 
            this.listView1.AllowColumnReorder = true;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(702, 296);
            this.listView1.TabIndex = 3;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView1_ColumnClick);
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            this.listView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "File";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Key";
            this.columnHeader2.Width = 77;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Englisch";
            this.columnHeader3.Width = 189;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Deutsch";
            this.columnHeader4.Width = 197;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.logBox);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(948, 497);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Log";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // logBox
            // 
            this.logBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logBox.Location = new System.Drawing.Point(3, 3);
            this.logBox.Name = "logBox";
            this.logBox.ReadOnly = true;
            this.logBox.Size = new System.Drawing.Size(942, 491);
            this.logBox.TabIndex = 0;
            this.logBox.Text = "";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 24);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(956, 523);
            this.tabControl1.TabIndex = 2;
            // 
            // CT2Translate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(956, 569);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "CT2Translate";
            this.Text = "CT2Translate";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CT2Translate_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.TreeView fileTree;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.RichTextBox logBox;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RichTextBox lang2TextBox;
        private System.Windows.Forms.TextBox keyTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox lang3TextBox;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox filterBox;
        private System.Windows.Forms.Button SearchButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox fileTextBox;
        private System.Windows.Forms.Button ClearSearchButton;
        private System.Windows.Forms.CheckBox searchcaseBox;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button nextmissButton;
        private System.Windows.Forms.Button prevmissButton;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox basepathTextBox;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadMergedResourcesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem recursiveDirectoryScanToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveMergedResourcesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToBasepathToolStripMenuItem;
        private System.Windows.Forms.RichTextBox lang1TextBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
    }
}

