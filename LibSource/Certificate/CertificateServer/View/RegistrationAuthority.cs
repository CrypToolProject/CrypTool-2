using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CrypTool.Util.Logging;

namespace CrypTool.CertificateServer.View
{
    public partial class RegistrationAuthorityView : UserControl
    {

        #region Constructor

        public RegistrationAuthorityView()
        {
            InitializeComponent();

            this.registrationAuthorityListView.SmallImageList = CreateImageList();

            this.FillComboBoxes();
            this.refreshComboBox.SelectedIndex = 4;
            this.messageRefreshingTimer.Tick += new EventHandler(MessageRefreshingTimerTick);
        }

        #endregion


        #region GUI methods

        private ImageList CreateImageList()
        {
            ImageList list = new ImageList();
            foreach (Color c in new Color[] { Color.Green, Color.Red })
            {
                Bitmap bmp = new Bitmap(16, 16);
                Graphics.FromImage(bmp).Clear(c);
                list.Images.Add(bmp);
            }
            return list;
        }

        private void FillComboBoxes()
        {

            ComboBoxEntry[] entries = new ComboBoxEntry[] 
            { 
                new ComboBoxEntry("30 Seconds", 30), 
                new ComboBoxEntry("1 Minute", 60), 
                new ComboBoxEntry("5 Minutes", 300), 
                new ComboBoxEntry("10 Minutes", 600),
                new ComboBoxEntry("No refresh", 0) 
            };

            // Fill the comboboxes
            foreach (ComboBoxEntry entry in entries)
            {
                this.refreshComboBox.Items.Add(entry);
            }
        }

        public void Reload()
        {
            if (this.registrationAuthority == null)
            {
                return;
            }

            try
            {
                List<RegistrationEntry> list = this.registrationAuthority.GetRegistrationEntries();

                this.SuspendLayout();
                this.registrationAuthorityListView.SuspendLayout();
                this.registrationAuthorityListView.Items.Clear();
                int insertIndex = 0;
                foreach (RegistrationEntry entry in list)
                {
                    this.registrationAuthorityListView.Items.Insert(insertIndex++, RegistrationItem.Parse(entry));
                }
                this.registrationAuthorityListView.ResumeLayout();
                this.ResumeLayout();
            }
            catch (Exception ex)
            {
                Log.Warn("Could not reload Registration Authority data!", ex);
            }
        }

        #endregion


        #region EventHandler

        private void refreshComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Hint: Will also be called on startup, when selecting the default entry
            ComboBoxEntry entry = (ComboBoxEntry)this.refreshComboBox.SelectedItem;
            this.messageRefreshingTimer.Stop();
            if (entry.Seconds > 0)
            {
                this.messageRefreshingTimer.Interval = entry.Seconds * 1000;
                this.messageRefreshingTimer.Start();
            }
        }

        private void selectAllButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.registrationAuthorityListView.Items)
            {
                item.Checked = true;
            }
        }

        private void authorizeRequestsButton_Click(object sender, EventArgs e)
        {
            if (this.registrationAuthorityListView.CheckedItems.Count == 0)
            {
                return;
            }

            List<RegistrationEntry> authorizeList = new List<RegistrationEntry>();
            StringBuilder sb1 = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            foreach (ListViewItem item in this.registrationAuthorityListView.CheckedItems)
            {
                RegistrationItem regItem = item as RegistrationItem;
                authorizeList.Add(regItem.Registration);
                sb1.Append("Avatar: ").Append(regItem.Avatar);
                sb1.Append(" | Email: ").Append(regItem.Email);
                sb1.AppendLine();
                if (regItem.IsVerified)
                {
                    sb2.Append("Avatar: ").Append(regItem.Avatar);
                    sb2.Append(" | Email: ").Append(regItem.Email);
                    sb2.AppendLine();
                }
            }

            // Show dialog only if an email is going to be send.
            if (sb2.Length > 0)
            {
                sb1.AppendLine();
                sb1.AppendLine("The follow email addresses will be informed that the account has been authorized:");
                sb1.Append(sb2.ToString());
                DialogResult result = ConfirmDialog.ShowDialog("Confirm authorization", "You are on the way to authorize the following registration requests.\nPlease check your decision here and confirm when you reviewed your choices.", sb1.ToString());
                if (result != DialogResult.OK)
                {
                    return;
                }
            }

            try
            {
                this.registrationAuthority.AuthorizeRegistrationEntries(authorizeList);
                this.Reload();
            }
            catch (Exception ex)
            {
                Log.Warn("Could not authorize registration requests!", ex);
            }
        }

        private void rejectRequestButton_Click(object sender, EventArgs e)
        {
            if (this.registrationAuthorityListView.CheckedItems.Count == 0)
            {
                return;
            }

            List<RegistrationEntry> rejectList = new List<RegistrationEntry>();
            foreach (ListViewItem item in this.registrationAuthorityListView.CheckedItems)
            {
                RegistrationItem regItem = item as RegistrationItem;
                rejectList.Add(regItem.Registration);
            }
            try
            {
                this.registrationAuthority.RejectRegistrationEntries(rejectList);
                this.Reload();
            }
            catch (Exception ex)
            {
                Log.Warn("Could not reject registration requests!", ex);
            }
        }

        private void deleteRequestsButton_Click(object sender, EventArgs e)
        {
            if (this.registrationAuthorityListView.CheckedItems.Count == 0)
            {
                return;
            }

            List<RegistrationEntry> deleteList = new List<RegistrationEntry>();
            StringBuilder sb = new StringBuilder();
            foreach (ListViewItem item in this.registrationAuthorityListView.CheckedItems)
            {
                RegistrationItem regItem = item as RegistrationItem;
                deleteList.Add(regItem.Registration);
                sb.Append("Avatar: ").Append(regItem.Avatar);
                sb.Append(" | Email: ").Append(regItem.Email);
                sb.AppendLine();
            }

            DialogResult result = ConfirmDialog.ShowDialog("Confirm delete", "You are on the way to delete the following registration requests.\nPlease check your decision here and confirm when you reviewed your choices.", sb.ToString());
            if (result == DialogResult.OK)
            {
                try
                {
                    this.registrationAuthority.DeleteRegistrationEntries(deleteList);
                    this.Reload();
                }
                catch (Exception ex)
                {
                    Log.Warn("Could not delete registration requests!", ex);
                }
            }
        }

        private void RegistrationAuthority_Resize(object sender, EventArgs e)
        {
            int width = 0;
            for (int i = 0; i < this.registrationAuthorityListView.Columns.Count - 1; i++)
                width += this.registrationAuthorityListView.Columns[i].Width;

            this.registrationAuthorityListView.Columns[this.registrationAuthorityListView.Columns.Count - 1].Width = this.registrationAuthorityListView.Width - width;
        }

        private void MessageRefreshingTimerTick(object sender, EventArgs e)
        {
            this.Reload();
        }

        private void RegistrationAuthorityView_VisibleChanged(object sender, EventArgs e)
        {
            ComboBoxEntry entry = this.refreshComboBox.SelectedItem as ComboBoxEntry;
            if (this.Visible && entry.Seconds > 0)
            {
                this.messageRefreshingTimer.Start();
            }
            else
            {
                this.messageRefreshingTimer.Stop();
            }
        }

        #endregion


        #region Setter

        public void SetRegistrationAuthority(RegistrationAuthority ra)
        {
            this.registrationAuthority = ra;
        }

        #endregion


        #region Private Members

        private RegistrationAuthority registrationAuthority;

        #endregion


        #region Registration request list item

        public class RegistrationItem : ListViewItem
        {

            private RegistrationItem(RegistrationEntry entry)
            {
                this.Registration = entry;
                this.Avatar = entry.Avatar;
                this.Email = entry.Email;
                this.World = entry.World;
                this.ReceiptTime = entry.DateOfRequest.ToString();
                this.ProgramName = entry.ProgramName;
                this.IsAuthorized = entry.IsAuthorized;
                this.IsVerified = entry.IsVerified;
            }

            public static RegistrationItem Parse(RegistrationEntry entry)
            {
                RegistrationItem item = new RegistrationItem(entry);
                item.UpdateItem();
                item.AdjustColor();
                item.AdjustAuthorizationIcon();
                return item;
            }

            private void UpdateItem()
            {
                this.SubItems.Clear();
                this.SubItems.AddRange(
                    new string[] 
                { 
                    this.IsVerified.ToString(),
                    this.Avatar,
                    this.Email,
                    this.World,
                    this.ReceiptTime,
                    this.ProgramName
                });
            }

            private void AdjustColor()
            {
                this.BackColor = (this.IsVerified) ? Color.Honeydew : Color.PeachPuff;
            }

            private void AdjustAuthorizationIcon()
            {
                this.ImageIndex = (this.IsAuthorized) ? 0 : 1;
            }

            private static int GetLength(string[] baseArr, int count)
            {
                int length = count;
                for (int i = 0; i < count; i++)
                    length += baseArr[i].Length;

                return length;
            }

            public RegistrationEntry Registration { get; set; }

            public string Avatar 
            {
                get { return Registration.Avatar; }
                private set { Registration.Avatar = value; } 
            }

            public string Email
            {
                get { return Registration.Email ; }
                private set { Registration.Email = value; } 
            }

            public string World
            {
                get { return Registration.World; }
                private set { Registration.World = value; }
            }

            public string ReceiptTime
            {
                get { return Registration.DateOfRequest.ToString(); }
                private set { Registration.DateOfRequest = DateTime.Parse(value); } 
            }

            public string ProgramName
            {
                get { return Registration.ProgramName; }
                private set { Registration.ProgramName = value; }
            }

            public bool IsVerified
            {
                get { return Registration.IsVerified; }
                private set { Registration.IsVerified = value; }
            }

            public bool IsAuthorized
            {
                get { return Registration.IsAuthorized; }
                private set { Registration.IsAuthorized = value; } 
            }
        }

        #endregion


        #region Combobox entry

        class ComboBoxEntry
        {
            public ComboBoxEntry(string text, int seconds)
            {
                this.Text = text;
                this.Seconds = seconds;
            }

            public override string ToString()
            {
                return this.Text;
            }

            public string Text;
            public int Seconds;
        }

        #endregion

    }
}
