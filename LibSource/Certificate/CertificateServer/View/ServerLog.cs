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
    public partial class ServerLog : UserControl
    {

        #region Constructor

        public ServerLog()
        {
            InitializeComponent();
            PostInitialize();

            Console.SetOut(new RedirectConsoleOutput());
            RedirectConsoleOutput.OnConsoleOutput += new RedirectConsoleOutput.ConsoleOutputEventHandler(OnRedirectConsoleOutput);

            //Create Timer
            this.messageRefreshingTimer = new System.Windows.Forms.Timer();
            this.messageRefreshingTimer.Interval = 500;
            this.messageRefreshingTimer.Tick += new EventHandler(MessageRefreshingTimerTick);
            this.messageRefreshingTimer.Start();

            FilterManager.Add(FatalFilter);
            FilterManager.Add(ErrorFilter);
            FilterManager.Add(WarningFilter);
            FilterManager.Add(InfoFilter);
            FilterManager.Add(DebugFilter);
        }

        #endregion


        #region Post Initialization

        private void PostInitialize()
        {
            this.splitLogContainer.SplitterDistance = (int)(splitLogContainer.Height * 0.75);
            this.messageView.SmallImageList = CreateImageList();
        }

        private ImageList CreateImageList()
        {
            ImageList list = new ImageList();
            foreach (Color c in new Color[] { Color.LightSteelBlue, Color.Yellow, Color.SandyBrown, Color.LightCoral, Color.Red, Color.Black })
            {
                Bitmap bmp = new Bitmap(16, 16);
                Graphics.FromImage(bmp).Clear(c);
                list.Images.Add(bmp);
            }
            return list;
        }

        #endregion


        #region Filters

        private Filter FatalFilter = new Filter() { MessageType = MessageItemSeverity.FATAL.ToString(), IsEnabled = false, Text = "" };

        private Filter ErrorFilter = new Filter() { MessageType = MessageItemSeverity.ERROR.ToString(), IsEnabled = false, Text = "" };

        private Filter WarningFilter = new Filter() { MessageType = MessageItemSeverity.WARN.ToString(), IsEnabled = false, Text = "" };

        private Filter InfoFilter = new Filter() { MessageType = MessageItemSeverity.INFO.ToString(), IsEnabled = false, Text = "" };
        
        private Filter DebugFilter = new Filter() { MessageType = MessageItemSeverity.DEBUG.ToString(), IsEnabled = false, Text = "" };

        #endregion


        #region EventHandler

        // Log View - Error button clicked        
        private void logViewErrorButton_Click(object sender, EventArgs e)
        {
            if (ErrorFilter.IsEnabled)
            {
                this.logViewErrorButton.BackColor = System.Drawing.Color.LightCoral;
                logViewErrorButton.ForeColor = Color.Black;
                ErrorFilter.IsEnabled = false;
            }
            else
            {
                this.logViewErrorButton.BackColor = System.Drawing.Color.LightGray;
                logViewErrorButton.ForeColor = Color.LightCoral;
                ErrorFilter.IsEnabled = true;
            }

        }

        // Log View - Warning button clicked
        private void logViewWarningButton_Click(object sender, EventArgs e)
        {
            if (WarningFilter.IsEnabled)
            {
                this.logViewWarningButton.BackColor = System.Drawing.Color.SandyBrown;
                logViewWarningButton.ForeColor = Color.Black;
                WarningFilter.IsEnabled = false;
            }
            else
            {
                this.logViewWarningButton.BackColor = System.Drawing.Color.LightGray;
                logViewWarningButton.ForeColor = Color.DarkOrange;
                WarningFilter.IsEnabled = true;
            }
        }

        // Log View - Info button clicked
        private void logViewInfoButton_Click(object sender, EventArgs e)
        {
            if (InfoFilter.IsEnabled)
            {
                this.logViewInfoButton.BackColor = System.Drawing.Color.Khaki;
                logViewInfoButton.ForeColor = Color.Black;
                InfoFilter.IsEnabled = false;
            }
            else
            {
                this.logViewInfoButton.BackColor = System.Drawing.Color.LightGray;
                logViewInfoButton.ForeColor = Color.Yellow;
                InfoFilter.IsEnabled = true;
            }
        }

        // Log View - Debug button clicked
        private void logViewDebugButton_Click(object sender, EventArgs e)
        {
            if (DebugFilter.IsEnabled)
            {
                this.logViewDebugButton.BackColor = System.Drawing.Color.LightSteelBlue;
                logViewDebugButton.ForeColor = Color.Black;
                DebugFilter.IsEnabled = false;
            }
            else
            {
                this.logViewDebugButton.BackColor = System.Drawing.Color.LightGray;
                logViewDebugButton.ForeColor = Color.Blue;
                DebugFilter.IsEnabled = true;
            }
        }

        private void ServerLog_Resize(object sender, EventArgs e)
        {
            int width = 0;
            for (int i = 0; i < this.messageView.Columns.Count - 1; i++)
                width += this.messageView.Columns[i].Width;

            this.messageView.Columns[this.messageView.Columns.Count - 1].Width = this.messageView.Width - width;
        }

        private void BlackAllButtons()
        {
            this.logViewErrorButton.ForeColor = Color.Black;
            this.logViewWarningButton.ForeColor = Color.Black;
            this.logViewInfoButton.ForeColor = Color.Black;
            this.logViewDebugButton.ForeColor = Color.Black;
        }

        private void SelectedMessageChanged(object sender, EventArgs e)
        {
            if (messageView.SelectedItems.Count == 0)
            {
                return;
            }
            MessageItem item = (MessageItem)this.messageView.SelectedItems[0];
            this.messageDetailsView.Text = String.Empty;
            this.messageDetailsView.SelectionFont = new Font(this.messageDetailsView.Font, FontStyle.Bold);
            this.messageDetailsView.SelectedText = string.Format("{1} - {2} - {3}{0}{0}", Environment.NewLine, item.TimeStamp, item.Severity, item.Header);
            this.messageDetailsView.AppendText(item.Content);
        }

        #endregion


        #region Methods

        private void MessageRefreshingTimerTick(object sender, EventArgs e)
        {
            this.SuspendLayout();
            this.messageView.SuspendLayout();
            int insertIndex = 0;

            List<MessageItem> messages = MessageManager.GetMessages("");

            for (int i = messages.Count - 1; i >= 0; i--)
            {
                if (insertIndex > 1000)
                    break;
                MessageItem item = messages[i];

                if ((item.IsVisible) && (this.messageView.Items.Contains(item)))
                    insertIndex++;
                else if ((item.IsVisible) && (!this.messageView.Items.Contains(item)))
                {
                    item.Remove();
                    this.messageView.Items.Insert(insertIndex++, item);
                }
                else if ((!item.IsVisible) && (this.messageView.Items.Contains(item)))
                    this.messageView.Items.Remove(item);
            }
            while (this.messageView.Items.Count > 1000)
            {
                this.messageView.Items.RemoveAt(this.messageView.Items.Count - 1);
            }

            this.messageView.ResumeLayout();
            this.ResumeLayout();
        }

        void OnRedirectConsoleOutput(string text)
        {
            MessageManager.AddMessage(MessageItem.Parse(text));
            //MessageManager.GetMessages(...);
        }

        #endregion


        #region Private member

        private ListView messageView;

        private Timer messageRefreshingTimer;

        private RichTextBox messageDetailsView;

        private SplitContainer splitLogContainer;

        #endregion


        #region Logging

        public enum MessageItemSeverity
        {
            DEBUG = 0,
            INFO = 1,
            WARN = 2,
            ERROR = 3,
            FATAL = 4,
            OTHER = 5,
            ALL = 6
        }

        public class SelectedMessageEventArgs : EventArgs
        {
            public MessageItem SelectedMessageItem { get; set; }
        }

        public class MessageItem : ListViewItem
        {
            public string InstanceID { get; set; }
            public string Content { get; set; }
            public string Header { get; set; }
            public string TimeStamp { get; set; }
            public string Severity { get; set; }
            public bool IsVisible { get; set; }
            public bool Processed { get; set; }

            private MessageItem()
            {
                this.InstanceID = String.Empty;
                this.Content = String.Empty;
                this.Header = String.Empty;
                this.TimeStamp = String.Empty;
                this.Severity = String.Empty;
            }

            private void UpdateItem()
            {
                this.SubItems.Clear();
                this.SubItems.AddRange(
                    new string[] 
                { 
                    this.TimeStamp,
                    this.Severity, 
                    this.Header,
                    this.Content 
                });
                this.ImageIndex = (int)(MessageItemSeverity)Enum.Parse(typeof(MessageItemSeverity), this.Severity);
            }

            public static MessageItem Parse(string text)
            {
                MessageItem item = new MessageItem() { Content = text, TimeStamp = DateTime.UtcNow.ToString(), Severity = MessageItemSeverity.OTHER.ToString() };

                try
                {

                    item.UpdateItem();
                    if (String.IsNullOrEmpty(text))
                        return item;

                    string[] baseArr = text.Split(' ');
                    if (baseArr.Length < 4)
                        return item;

                    item.TimeStamp = string.Format("{0} {1}", baseArr[0].Trim(), baseArr[1].Trim());
                    item.Severity = ((MessageItemSeverity)Enum.Parse(typeof(MessageItemSeverity), baseArr[3].Trim())).ToString();

                    string messageBody = text.Substring(GetLength(baseArr, 4)).Trim();

                    int sIndex = messageBody.IndexOf("[");
                    int eIndex = messageBody.IndexOf("]", sIndex);

                    if ((sIndex > -1) || (eIndex > -1))
                    {
                        item.Header = messageBody.Substring(sIndex, eIndex - sIndex + 1).Trim();
                        messageBody = messageBody.Remove(sIndex, eIndex - sIndex + 1);

                        string[] instArr = item.Header.Substring(1, item.Header.Length - 2).Split(Log.Splitter);
                        for (int i = 0; i < instArr.Length; i++)
                        {
                            if (instArr[i].Contains(Log.INSTANCE_ID_PREFIX))
                            {
                                item.InstanceID = instArr[i].Trim();
                                item.InstanceID = item.InstanceID.Substring(item.InstanceID.LastIndexOf(' '));
                            }
                        }
                    }
                    item.Content = messageBody.Trim();
                    item.UpdateItem();
                    return item;
                }
                catch (Exception)
                {
                    item.Text = text;
                    return item;
                }
            }

            private static int GetLength(string[] baseArr, int count)
            {
                int length = count;
                for (int i = 0; i < count; i++)
                    length += baseArr[i].Length;

                return length;
            }
        }

        public static class MessageManager
        {
            public const string DEFAULT_INSTANCE_ID = "";
            public const string EMPTY_SEVERITY = "ALL";

            private static string currentInstanceID;
            private static List<string> headerList = new List<string>();
            private static List<MessageItem> globalMessageList = new List<MessageItem>();

            public static string[] Header
            {
                get { return headerList.ToArray(); }
            }

            public static void AddMessage(MessageItem item)
            {
                lock (globalMessageList)
                {
                    if (globalMessageList.Count >= 1000)
                        globalMessageList.RemoveAt(0);

                    globalMessageList.Add(item);
                    if ((!String.IsNullOrEmpty(item.Header)) && (!headerList.Contains(item.Header)))
                        headerList.Add(item.Header);
                }
            }

            public static List<MessageItem> GetMessages(string instanceID)
            {
                lock (globalMessageList)
                {
                    for (int i = globalMessageList.Count - 1; i >= 0; i--)
                    {
                        MessageItem item = globalMessageList[i];
                        item.IsVisible = true;

                        if ((item.InstanceID != instanceID) && (instanceID != DEFAULT_INSTANCE_ID))
                        {
                            item.IsVisible = false;
                            continue;
                        }

                        foreach (Filter filter in FilterManager.Filter)
                        {
                            if (!filter.IsEnabled)
                                continue;
                            if (((filter.MessageType == item.Severity) || (filter.MessageType == EMPTY_SEVERITY)) &&
                                ((item.Header.Contains(filter.Text)) || (item.Content.Contains(filter.Text)) || (String.IsNullOrEmpty(filter.Text))))
                            {
                                item.IsVisible = false;
                                continue;
                            }
                        }
                    }
                    currentInstanceID = instanceID;
                    return globalMessageList;
                }
            }
        }

        [Serializable]
        public class Filter
        {
            public bool IsEnabled { get; set; }
            public string Name { get; set; }
            public string MessageType { get; set; }
            public string Text { get; set; }
        }

        public static class FilterManager
        {
            private static List<Filter> filter = new List<Filter>();

            public static Filter[] Filter
            {
                get { return filter.ToArray(); }
            }

            public static void Add(Filter f)
            {
                filter.Add(f);
            }

            public static void AddRange(Filter[] range)
            {
                if (range != null)
                {
                    foreach (Filter f in range)
                    {
                        filter.Add(f);
                    }
                }
            }

            public static void Remove(Filter f)
            {
                filter.Remove(f);
            }

            public static void MoveUp(Filter f)
            {
                int index = filter.IndexOf(f);
                if (index == 0)
                    return;
                filter.Remove(f);
                filter.Insert(index - 1, f);
            }

            public static void MoveDown(Filter f)
            {
                int index = filter.IndexOf(f);
                if (index == filter.Count - 1)
                    return;
                filter.Remove(f);
                filter.Insert(index + 1, f);
            }
        }

        #endregion logging

    }
}
