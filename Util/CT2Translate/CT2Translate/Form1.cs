using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Resources;
using System.Collections;
using System.Text.RegularExpressions;

namespace WindowsFormsApplication1
{
    delegate void LogDelegate(string msg);

    public partial class CT2Translate : Form
    {
        private ListViewColumnSorter lvwColumnSorter;
        List<string> allResources = new List<string>();
        AllResources allres = new AllResources();

        public static string[] displayedLanguages = { "en", "de", "ru" };
        Dictionary<string, string> LongLanguage = new Dictionary<string, string> { {"en","Englisch"}, { "de", "Deutsch" }, { "ru", "Russisch" } };
        HashSet<string> ignorePaths = new HashSet<string> { "obj" };


        public CT2Translate()
        {
            allres.log = log;
            InitializeComponent();
            // Create an instance of a ListView column sorter and assign it to the ListView control.
            lvwColumnSorter = new ListViewColumnSorter();
            listView1.ListViewItemSorter = lvwColumnSorter;
            listView1.Columns.Clear();
            listView1.Columns.Add(new ColHeader("Resource", 70, HorizontalAlignment.Left, true));
            listView1.Columns.Add(new ColHeader("Key", 100, HorizontalAlignment.Left, true));
            foreach( string lang in displayedLanguages )
                listView1.Columns.Add(new ColHeader(LongLanguage[lang], 160, HorizontalAlignment.Left, true));
            listView1.HideSelection = false;

            fileTree.HideSelection = false;

            ToolTip toolTip1 = new ToolTip();
            toolTip1.ShowAlways = true;

            toolTip1.SetToolTip(prevmissButton, "go to previous missing item");
            toolTip1.SetToolTip(nextmissButton, "go to next missing item");
            toolTip1.SetToolTip(ClearSearchButton, "clear filter");
            toolTip1.SetToolTip(SearchButton, "apply filter");

            //pathTextBox.Text = Properties.Settings.Default.Path;
            textBox1.LostFocus += new EventHandler(LangTextBox_Leave);
        }

        public void log(string msg)
        {
            logBox.AppendText(String.Format("{0}: {1}\n", DateTime.Now, msg));
        }

        private void recursiveDirectoryScanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            FolderBrowserDialog objDialog = new FolderBrowserDialog();
            objDialog.Description = "Startpfad wählen";
            objDialog.SelectedPath = Properties.Settings.Default.Path;
            DialogResult objResult = objDialog.ShowDialog(this);

            if (objResult == DialogResult.OK)
            {
                Properties.Settings.Default.Path = objDialog.SelectedPath;
                Properties.Settings.Default.Save();

                allres.ScanDir(objDialog.SelectedPath);

                fileTree.Nodes.Clear();
                fileTree.Nodes.Add(allres.GetTree());

                UpdateList();

                basepathTextBox.Text = allres.basepath;

                toolStripStatusLabel1.Text = allres.Resources.Count + " resources found";
            } 
            
            Cursor.Current = Cursors.Default;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var sel = listView1.SelectedItems[0];
                fileTextBox.Text = sel.SubItems[0].Text;
                keyTextBox.Text = sel.SubItems[1].Text;
                lang1TextBox.Text = sel.SubItems[displayedLanguages[0]].Text;
                lang2TextBox.Text = sel.SubItems[displayedLanguages[1]].Text;
                lang3TextBox.Text = sel.SubItems[displayedLanguages[2]].Text;
                //if (displayedLanguages.Count() > 1)
                //    lang3TextBox.Text = sel.SubItems[displayedLanguages[1]].Text;
            }
        }

        private void UpdateItem(ListViewItem item)
        {
            TranslatedKey tk = (TranslatedKey)item.Tag;

            foreach (string lang in displayedLanguages)
            {
                if (tk.Translations.ContainsKey(lang))
                {
                    item.SubItems[lang].BackColor = Color.Transparent;
                    item.SubItems[lang].Text = tk.Translations[lang];
                }
                else
                {
                    item.SubItems[lang].BackColor = Color.LightSalmon;
                    item.SubItems[lang].Text = "";
                }
            }
        }

        private void listViewAdd(string fname, TranslatedResource dict, string filter = ".*")
        {
            List<ListViewItem> lvi = new List<ListViewItem>();

            try
            {
                RegexOptions options = searchcaseBox.Checked ? RegexOptions.None : RegexOptions.IgnoreCase;

                foreach (KeyValuePair<string, TranslatedKey> pair in dict.TranslatedKey)
                {
                    bool match = Regex.Match(pair.Key, filter, options).Success ||
                                   displayedLanguages.Any(l => pair.Value.Translations.ContainsKey(l) && Regex.Match(pair.Value.Translations[l], filter, options).Success);

                    if (!match) continue;
                    
                    // populate listview
                    ListViewItem item = new ListViewItem( new string[] { fname, pair.Key } );
                    item.SubItems[0].Tag = dict;
                    foreach (string lang in displayedLanguages)
                    {
                        ListViewItem.ListViewSubItem subitem = item.SubItems.Add("");
                        subitem.Name = lang;
                    }

                    item.Tag = pair.Value;  // speichere Zeiger auf TranslatedKey im Item-Tag
                    item.UseItemStyleForSubItems = false;
                    UpdateItem(item);
                    lvi.Add(item);
                }

                listView1.Items.AddRange(lvi.ToArray());    // viel schneller als einzelnes Hinzufügen!
            }
            catch (Exception e)
            {
            }
        }

        private void listViewAdd(string filter = ".*")
        {
            listViewAdd(allres.Resources.Keys.ToArray(), filter);
        }

        private void listViewAdd(string[] paths, string filter = ".*")
        {
            listView1.BeginUpdate();
            foreach (string path in paths)
                listViewAdd(path, allres.Resources[path], filter);
            listView1.EndUpdate();
            
            textBox1.Text = String.Format("{0} item{1} displayed{2}", 
                listView1.Items.Count,
                (listView1.Items.Count==1) ? "" : "s",
                (filter==".*" || filter=="") ? "" : " (filtered)"
                );
            textBox2.Text = countEmptyKeys().ToString();
        }

        private int countEmptyKeys()
        {
            int count = 0;

            foreach (ListViewItem item in listView1.Items)
            {
                var translations = ((TranslatedKey)item.Tag).Translations;
                count += displayedLanguages.Count(lang => !translations.ContainsKey(lang));
            }

            return count;
        }
        
        private void nextmissButton_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count == 0) return;
            if (listView1.SelectedItems.Count == 0)
                listView1.Items[0].Selected = true;
            var i = listView1.SelectedIndices[0];
            for (int j = 0; j < listView1.Items.Count; j++)
            {
                ListViewItem item = listView1.Items[(i + 1 + j) % listView1.Items.Count];
                foreach (string lang in displayedLanguages)
                    if (!((TranslatedKey)item.Tag).Translations.ContainsKey(lang))
                    {
                        listView1.SelectedItems.Clear();
                        item.Selected = true;
                        item.EnsureVisible();
                        listView1.Select();
                        return;
                    }
            }
        }

        private void prevmissButton_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count == 0) return;
            if (listView1.SelectedItems.Count == 0)
                listView1.Items[0].Selected = true;
            var i = listView1.SelectedIndices[0];
            for (int j = 0; j < listView1.Items.Count; j++)
            {
                ListViewItem item = listView1.Items[(i - 1 - j + listView1.Items.Count) % listView1.Items.Count];
                foreach (string lang in displayedLanguages)
                    if (!((TranslatedKey)item.Tag).Translations.ContainsKey(lang))
                    {
                        listView1.SelectedItems.Clear();
                        item.Selected = true;
                        item.EnsureVisible();
                        listView1.Select();
                        return;
                    }
            }
        }

        private void UpdateList()
        {
            if (fileTree.Nodes.Count == 0) return;

            Cursor.Current = Cursors.WaitCursor;
            
            if (fileTree.SelectedNode == null)
                fileTree.SelectedNode = fileTree.Nodes[0];

            string[] files = allres.MatchFiles(fileTree.SelectedNode.Name);

            listView1.Items.Clear();
            listViewAdd(files, filterBox.Text);

            if (listView1.Items.Count > 0)
                listView1.SelectedIndices.Add(0);

            nextmissButton.Enabled = prevmissButton.Enabled = (listView1.Items.Count > 0);

            Cursor.Current = Cursors.Default;
        }

        private void fileTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateList();
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            //listView1.AutoResizeColumn(e.Column, ColumnHeaderAutoResizeStyle.ColumnContent);
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                lvwColumnSorter.Order = (lvwColumnSorter.Order == SortOrder.Ascending)
                    ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.listView1.Sort();
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            UpdateList();
        }

        private void filterBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                SearchButton_Click(sender, e);
        }

        private void ClearSearchButton_Click(object sender, EventArgs e)
        {
            if (filterBox.Text != "")
            {
                filterBox.Text = "";
                SearchButton_Click(sender, e);
            }
        }

        //private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    if(fileTree.SelectedNode==null) return;
        //    //if( listView1.SelectedItems.Count==0 ) return;
        //    //string filename = listView1.SelectedItems[0].SubItems[0].Text;
        //    string filename = fileTree.SelectedNode.FullPath;
        //    logBox.Text += "Saving " + filename + "\n";
        //    //SaveResourceFile(listView1.SelectedItems[0].SubItems[0].Text);
        //    string basename = AllResources.getKey(filename);
        //    string culture = AllResources.getCulture(filename);
        //    if( allres.Resources.ContainsKey(basename) )
        //        allres.Resources[basename].SaveAs(culture, "test");
        //}

        private string GetOpenFileName(string title)
        {
            OpenFileDialog dlg = new OpenFileDialog();          
            dlg.Title = title;
            dlg.Filter = "Merged Resource (*.xml)|*.xml";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                return dlg.FileName;
                //toolStripStatusLabel1.Text = dlg.FileName;
                //Properties.Settings.Default.Save();
            }

            return null;
        }

        private string GetSaveFileName(string title)
        {
            SaveFileDialog dlg = new SaveFileDialog();          
            dlg.Title = title;
            dlg.Filter = "Merged Resource (*.xml)|*.xml";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                return dlg.FileName;
                //toolStripStatusLabel1.Text = dlg.FileName;
                //Properties.Settings.Default.Save();
            }

            return null;
        }

        private void saveMergedResourcesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //string fname = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\CT2resources.xml";
            string fname = GetSaveFileName("Save CrypTool Merged Resource");

            Cursor.Current = Cursors.WaitCursor;
            if (fname != null) allres.SaveXML(fname);
            Cursor.Current = Cursors.Default;
        }

        private void loadMergedResourcesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //string fname = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\CT2resources.xml";
            string fname = GetOpenFileName("Load CrypTool Merged Resource");
            if (fname == null) return;

            Cursor.Current = Cursors.WaitCursor;

            try
            {
                allres.Clear();
                allres.LoadXML(fname);
                basepathTextBox.Text = allres.basepath;

                fileTree.Nodes.Clear();
                fileTree.Nodes.Add(allres.GetTree());

                UpdateList();
            }
            catch (Exception ex)
            {
                toolStripStatusLabel1.Text = fname + " not found!";
            }

            Cursor.Current = Cursors.Default;
        }

        private void saveGermanAsTxtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            allres.SaveText(path+"\\CT2german.txt", "de");
        }

        private void saveToBasepathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(basepathTextBox.Text) || !Directory.Exists(basepathTextBox.Text))
            {
                MessageBox.Show("Please enter a valid basepath.", "No basepath", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            allres.Update(basepathTextBox.Text);
        }

        private void LangTextBox_Leave(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count <= 0) return;

            RichTextBox rtb = (RichTextBox)sender;
            string lang = (string)rtb.Tag;
            ListViewItem item = listView1.SelectedItems[0];
            TranslatedResource rsrc = (TranslatedResource)item.SubItems[0].Tag;
            TranslatedKey tk = (TranslatedKey)item.Tag;
            if (!tk.Translations.ContainsKey(lang) && rtb.Text == "") return;
            if (tk.Translations.ContainsKey(lang) && tk.Translations[lang] == rtb.Text) return;
            //if (!rsrc.files.ContainsKey(lang)) rsrc.files.Add(lang,"");
            rsrc.modified[lang] = true;
            //if (!tk.Translations.ContainsKey(lang)) tk.Add(lang, "");
            tk.Translations[lang] = rtb.Text;
            item.SubItems[lang].Text = rtb.Text;
            UpdateItem(item);
            textBox2.Text = countEmptyKeys().ToString();
        }

        private bool quitWhileModified()
        {
            return !allres.Modified || MessageBox.Show("You have not saved the modified data. Quit anyway?", "Modified Data", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning) == DialogResult.Yes;
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(quitWhileModified()) Application.Exit();
        }

        private void CT2Translate_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !quitWhileModified();
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count <= 0) return;
            ListViewItem item = listView1.SelectedItems[0];
            TranslatedResource rsrc = (TranslatedResource)item.SubItems[0].Tag;
            string nodeName = rsrc.files.Values.OrderBy(x => x).First();
            TreeNode[] tns = fileTree.Nodes.Find(nodeName,true);
            if (tns.Length > 0)
            {
                fileTree.SelectedNode = tns[0];
                fileTree.Focus();
            }
        }
    }
}