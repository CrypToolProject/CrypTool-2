using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Xml;

namespace CrypTool.StreamComparator
{
    [Author("Thomas Schmid", "thomas.schmid@CrypTool.org", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("CrypTool.StreamComparator.Properties.Resources", "PluginCaption", "PluginTooltip", "StreamComparator/DetailedDescription/doc.xml", "StreamComparator/icon.png", "StreamComparator/Images/equal.png", "StreamComparator/Images/unequal.png", "StreamComparator/Images/contextmenu.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class StreamComparator : ICrypComponent
    {
        #region Private variables
        private bool isBinary;
        private bool stop;
        private ICrypToolStream streamOne;
        private ICrypToolStream streamTwo;
        private readonly StreamComparatorPresentation streamComparatorPresentation;
        #endregion

        private StreamComparatorSettings settings;
        public ISettings Settings
    {
      get => settings;
      set => settings = (StreamComparatorSettings)value;
    }

        #region ctor

        public StreamComparator()
        {
            streamComparatorPresentation = new StreamComparatorPresentation();
            settings = new StreamComparatorSettings();
        }

        #endregion

        #region Properties

        [PropertyInfo(Direction.InputData, "InputOneCaption", "InputOneTooltip", true)]
        public ICrypToolStream InputOne
    {
      get => streamOne;
      set => streamOne = value;
    }

        [PropertyInfo(Direction.InputData, "InputTwoCaption", "InputTwoTooltip", true)]
        public ICrypToolStream InputTwo
    {
      get => streamTwo;
      set => streamTwo = value;
    }

        private bool inputsAreEqual;
        [PropertyInfo(Direction.OutputData, "InputsAreEqualCaption", "InputsAreEqualTooltip", false)]
        public bool InputsAreEqual
        {
            get => inputsAreEqual;
            set
            {
                inputsAreEqual = value;
                OnPropertyChanged("InputsAreEqual");
            }
        }

        #endregion

        #region IPlugin Members

        public void Execute()
        {
            stop = false;
            isBinary = false;
            inputsAreEqual = false;

            int streamOneByte;
            int streamTwoByte;

            if (InputOne == null)
            {
                GuiTextChanged("Stream one is null, no comparison done.", NotificationLevel.Warning);
                stop = true;
            }
            if (InputTwo == null)
            {
                GuiTextChanged("Stream two is null, no comparison done.", NotificationLevel.Warning);
                stop = true;
            }

            if (stop)
            {
                return;
            }

            using (CStreamReader readerOne = InputOne.CreateReader(), readerTwo = InputTwo.CreateReader())
            {
                readerOne.WaitEof();
                readerTwo.WaitEof();

                if (readerOne.Length != readerTwo.Length && !settings.Diff)
                {
                    //GuiTextChanged("Inputs are not equal, because the filesize is different.", NotificationLevel.Info);
                    InputsAreEqual = false;
                    Progress(1, 1);
                    if (settings.Diff)
                    {
                        CreateDiffView();
                    }
                }
                else
                {
                    DateTime startTime = DateTime.Now;
                    int position = 0;
                    //GuiTextChanged("Starting byte comparison of files now...", NotificationLevel.Info);
                    // Read and compare a byte from each file until either a
                    // non-matching set of bytes is found or until the end of
                    // file1 is reached.
                    do
                    {
                        // Read one byte from each file.
                        streamOneByte = readerOne.ReadByte();
                        streamTwoByte = readerTwo.ReadByte();

                        if (streamOneByte == 0)
                        {
                            isBinary = true;
                        }

                        if (readerOne.Length > 0) // advance progress bar only if integer part of percentage increases
                        {
                            int newpos = (int)((readerOne.Position * 100) / readerOne.Length);
                            if (newpos > position)
                            {
                                position = newpos;
                                Progress(readerOne.Position, readerOne.Length);
                            }
                        }
                    } while ((streamOneByte == streamTwoByte) && (streamOneByte != -1));

                    // Return the success of the comparison. "file1byte" is 
                    // equal to "file2byte" at this point only if the files are 
                    // the same.
                    InputsAreEqual = ((streamOneByte - streamTwoByte) == 0);
                    DateTime stopTime = DateTime.Now;
                    TimeSpan duration = stopTime - startTime;
                    //GuiTextChanged(
                    //  "Comparison complete. Files are " + (InputsAreEqual ? "equal" : "unequal") + ".", NotificationLevel.Info);
                    //if (!InputsAreEqual)
                    //        GuiTextChanged("First position a different byte: " + readerOne.Position, NotificationLevel.Info);

                    Progress(1, 1);
                    //GuiTextChanged("Duration: " + duration, NotificationLevel.Info);

                    if (settings.Diff)
                    {
                        CreateDiffView();
                    }
                }

                if (InputsAreEqual)
                {
                    // QuickWatchInfo = "Inputs are equal";
                    StatusChanged((int)StreamComparatorImage.Equal);
                }
                else
                {
                    // QuickWatchInfo = "Inputs are unequal";
                    StatusChanged((int)StreamComparatorImage.Unequal);
                }
            }
        }

        private StringBuilder result;

        /// <summary>
        /// Creates the diff view.
        /// </summary>
        public void CreateDiffView()
        {
            if (!isBinary || true)
            {
                int maxLength = 65536;
                GuiTextChanged("Generating diff now...", NotificationLevel.Info);
                result = new StringBuilder();
                try
                {
                    using (CStreamReader readerOne = InputOne.CreateReader(), readerTwo = InputTwo.CreateReader())
                    {
                        readerOne.WaitEof();
                        readerTwo.WaitEof();

                        if (readerOne.Length > maxLength || readerTwo.Length > maxLength)
                        {
                            GuiTextChanged("Streams too big for complete diff, reading end of files only.", NotificationLevel.Warning);
                        }

                        long startIndex = Math.Max(
                  readerOne.Length - maxLength,
                  readerTwo.Length - maxLength);

                        StreamReader sr = new StreamReader(readerOne, Encoding.ASCII);
                        StringBuilder strTxt1 = new StringBuilder();

                        int size = startIndex > 0 ? (int)(readerOne.Length - startIndex) : (int)readerOne.Length;
                        char[] bArr = new char[size];

                        if (startIndex > 0)
                        {
                            sr.BaseStream.Seek(startIndex, SeekOrigin.Begin);
                        }

                        sr.Read(bArr, 0, bArr.Length);
                        bool test = sr.EndOfStream;
                        readerOne.Close();

                        for (int i = 0; i < bArr.Length; i++)
                        {
                            strTxt1.Append(bArr[i]);
                        }

                        sr = new StreamReader(readerTwo, Encoding.ASCII);
                        if (startIndex > 0)
                        {
                            sr.BaseStream.Seek(startIndex, SeekOrigin.Begin);
                        }

                        StringBuilder strTxt2 = new StringBuilder();

                        size = startIndex > 0 ? (int)(readerTwo.Length - startIndex) : (int)readerTwo.Length;
                        bArr = new char[size];
                        sr.Read(bArr, 0, bArr.Length);
                        test = sr.EndOfStream;
                        readerTwo.Close();

                        for (int i = 0; i < bArr.Length; i++)
                        {
                            strTxt2.Append(bArr[i]);
                        }

                        string[] aLines = strTxt1.ToString().Split('\n');
                        string[] bLines = strTxt2.ToString().Split('\n');

                        Diff diff = new Diff();
                        Diff.Item[] diffItem = diff.DiffText(strTxt1.ToString(), strTxt2.ToString());

                        result.AppendLine("<FlowDocument xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">");
                        result.AppendLine("<Table CellSpacing=\"0\" FontFamily=\"Tahoma\" FontSize=\"12\" Background=\"White\">");
                        result.AppendLine("<Table.Columns><TableColumn Width=\"40\" /><TableColumn/></Table.Columns>");
                        result.AppendLine("<TableRowGroup>");


                        string color = InputsAreEqual ? "#80FF80" : "#FF8080";

                        result.AppendLine("<TableRow Background=\"" + color + "\"><TableCell ColumnSpan=\"2\" TextAlignment=\"Left\">");
                        result.AppendLine("<Paragraph FontSize=\"9pt\" FontWeight=\"Bold\">");
                        if (InputsAreEqual)
                        {
                            result.AppendLine("Input streams are equal.");
                        }
                        else
                        {
                            result.AppendLine("Input streams are unequal.");
                        }

                        result.AppendLine("</Paragraph></TableCell></TableRow>");

                        int n = 0;
                        for (int fdx = 0; fdx < diffItem.Length; fdx++)
                        {
                            if (stop)
                            {
                                break;
                            }

                            Diff.Item aItem = diffItem[fdx];

                            // write unchanged lines
                            while ((n < aItem.StartB) && (n < bLines.Length))
                            {
                                Progress(n, bLines.Length);
                                WriteLine(n, DiffMode.NoChange, bLines[n]);
                                n++;
                            } // while

                            // write deleted lines
                            for (int m = 0; m < aItem.deletedA; m++)
                            {
                                Progress(n, bLines.Length);
                                WriteLine(-1, DiffMode.Remove, aLines[aItem.StartA + m]);
                            } // for

                            // write inserted lines
                            while (n < aItem.StartB + aItem.insertedB)
                            {
                                Progress(n, bLines.Length);
                                WriteLine(n, DiffMode.Add, bLines[n]);
                                n++;
                            } // while
                        } // while

                        // write rest of unchanged lines
                        while (n < bLines.Length && !stop)
                        {
                            Progress(n, bLines.Length);
                            WriteLine(n, DiffMode.NoChange, bLines[n]);
                            n++;
                        } // while
                        result.AppendLine("</TableRowGroup></Table></FlowDocument>");
                        Progress(1, 2);
                        CStreamWriter cs = new CStreamWriter(Encoding.UTF8.GetBytes(result.ToString()));
                        cs.Close();
                        streamComparatorPresentation.SetContent(cs);
                        Progress(1, 1);
                    }
                }
                catch (Exception exception)
                {
                    GuiTextChanged(exception.Message, NotificationLevel.Error);
                    GuiTextChanged("There were erros while creating the diff. Is this really text input?", NotificationLevel.Error);
                }
            }
            else
            {
                GuiTextChanged("BinaryInput, no comparison done.", NotificationLevel.Warning);
                streamComparatorPresentation.SetBinaryDocument();
            }
        }

        // public string Xaml;

        private enum DiffMode
        {
            Add, Remove, NoChange
        }


        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="lineNumber">The line number.</param>
        /// <param name="dm">The DiffMode.</param>
        /// <param name="aText">A text.</param>
        private void WriteLine(int lineNumber, DiffMode dm, string aText)
        {
            result.Append("<TableRow><TableCell Background=\"lightgray\"><Paragraph>");
            if (lineNumber != -1)
            {
                result.Append((lineNumber + 1).ToString("0000"));
            }
            else
            {
                result.Append(" ");
            }

            result.Append("</Paragraph></TableCell><TableCell>");

            if (dm == DiffMode.Add)
            {
                result.Append("<Paragraph Background=\"#80FF80\">");
            }
            else if (dm == DiffMode.Remove)
            {
                result.Append("<Paragraph Background=\"#FF8080\">");
            }
            else
            {
                result.Append("<Paragraph>");
            }

            // result.Append("<TextBlock TextWrapping=\"NoWrap\">");

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            XmlTextWriter xtw = new XmlTextWriter(sw);
            xtw.WriteString(removeUnprintablesAndUnicode(aText));
            // xtw.WriteString(aText);
            xtw.Flush();
            xtw.Close();

            result.AppendLine(sb.ToString() + "</Paragraph></TableCell></TableRow>\n");
        }

        /// <summary>
        /// Removes the unprintables and unicode.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public string removeUnprintablesAndUnicode(string text)
        {
            text = text.Replace("\r", "");
            string outputs = string.Empty;
            for (int jj = 0; jj < text.Length; jj++)
            {
                char ch = text[jj];
                if ((byte)ch >= 32 & (byte)ch <= 128)
                {
                    outputs += ch;
                }
                else
                {
                    outputs += ".";
                }
            }
            return outputs;
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public string Title { get; set; }

        public UserControl Presentation
        {
            get => streamComparatorPresentation;
            set { }
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public void Stop()
        {
            stop = true;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region methods
        private void GuiTextChanged(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private void StatusChanged(int imageIndex)
        {
            EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, imageIndex));
        }
        #endregion

        #region IPlugin Members


        public void PreExecution()
        {
            // QuickWatchInfo = null;
            streamComparatorPresentation.SetNoComparisonYetDocument();
            // InputsAreEqual = false;
        }

        public void PostExecution()
        {
            Dispose();
        }

        #endregion

        #region IPlugin Members

        #endregion
    }

    internal enum StreamComparatorImage
    {
        Default,
        Equal,
        Unequal
    }
}
