using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace KeyTextBox
{
    /// <summary>
    /// Interaction logic for KeyTextBox.xaml
    /// </summary>
    public partial class KeyTextBox : UserControl
    {
        private readonly Run _keyRun;

        public static readonly DependencyProperty KeyManagerProperty =
            DependencyProperty.Register(
            "KeyManager",
            typeof(IKeyManager),
            typeof(KeyTextBox),
            new FrameworkPropertyMetadata());

        public IKeyManager KeyManager
        {
            get => (IKeyManager)GetValue(KeyManagerProperty);
            set
            {
                if (KeyManager != null)
                {
                    KeyManager.OnKeyChanged -= KeyManagerChanged;
                }

                SetValue(KeyManagerProperty, value);

                if (value != null)
                {
                    SetKeyBox(value.GetKey(), 0);
                    value.OnKeyChanged += KeyManagerChanged;
                }
            }
        }

        public static readonly DependencyProperty CurrentKeyProperty =
            DependencyProperty.Register(
            "CurrentKey",
            typeof(string),
            typeof(KeyTextBox),
            new FrameworkPropertyMetadata());


        public string CurrentKey
        {
            get => (string)GetValue(CurrentKeyProperty);
            set
            {
                SetValue(CurrentKeyProperty, value);
                KeyManager.SetKey(value);
            }
        }

        public KeyTextBox()
        {
            InitializeComponent();
            DataObject.AddPastingHandler(KeyBox, PastingHandler);
        }

        private void PastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            string cb = e.DataObject.GetData(typeof(string)) as string;
            HandleInput(cb);
            e.CancelCommand();
            e.Handled = true;
        }

        private void KeyBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
            HandleInput(e.Text);
        }

        private void HandleInput(string input)
        {
            input = input.ToUpper();
            foreach (char inChar in input)
            {
                List<char> possibleChars;
                string key = KeyManager.GetKey();
                int caretIndex;
                TextPointer caretPosition = KeyBox.CaretPosition;
                bool next = false;

                do
                {
                    if (caretPosition == null)
                    {
                        return;
                    }

                    caretIndex = GetKeyOffset(caretPosition);
                    possibleChars = GetPossibleCharactersAtKeyEnd(key.Substring(0, caretIndex));
                    if (possibleChars == null || possibleChars.Count <= 1)
                    {
                        if (caretPosition.GetNextInsertionPosition(LogicalDirection.Forward) == null)
                        {
                            return;
                        }

                        caretPosition = caretPosition.GetNextInsertionPosition(LogicalDirection.Forward);
                        if (possibleChars != null && possibleChars.Count == 1 && possibleChars.Contains(inChar))
                        {
                            next = true;
                        }
                        possibleChars = null;
                    }
                } while (!next && (possibleChars == null || possibleChars.Count <= 1));

                if (!next)
                {
                    if (ReplaceCharInKey(key, possibleChars, inChar, caretIndex))
                    {
                        return;
                    }
                }
                else
                {
                    KeyBox.CaretPosition = caretPosition;
                }
            }
        }

        private void KeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string key = KeyManager.GetKey();
            switch (e.Key)
            {
                case Key.V:
                    e.Handled = true;
                    try
                    {
                        if (Keyboard.Modifiers == ModifierKeys.Control)
                        {
                            if (Clipboard.ContainsText())
                            {
                                string clipboardtext = Clipboard.GetText();
                                string format = KeyManager.GetFormat();
                                int start = GetKeyOffset(KeyBox.CaretPosition);
                                int end = key.Length;
                                if (KeyBox.Selection != null && !string.IsNullOrEmpty(KeyBox.Selection.Text))
                                {
                                    start = GetKeyOffset(KeyBox.Selection.Start);
                                    end = GetKeyOffset(KeyBox.Selection.End);
                                    if (end == 0)
                                    {
                                        //end == 0 means, the user pressed Ctrl + A to select everything
                                        end = key.Length;
                                    }
                                }
                                for (int i = start; i < end; i++)
                                {
                                    SetKeyOffset(i);
                                    if (i - start > clipboardtext.Length)
                                    {
                                        break;
                                    }
                                    SetKeyOffset(i);
                                    HandleInput(clipboardtext[i - start].ToString());
                                }

                            }
                        }
                    }
                    catch (Exception)
                    {
                        //wtf?
                    }
                    break;
                case Key.Space:
                    e.Handled = true;
                    HandleInput(" ");
                    break;
                case Key.Back:
                case Key.Delete:
                    e.Handled = true;
                    //remove everything that is selected when pressing delete or back
                    if (KeyBox.Selection != null && !string.IsNullOrEmpty(KeyBox.Selection.Text))
                    {
                        int start = GetKeyOffset(KeyBox.Selection.Start);
                        int end = GetKeyOffset(KeyBox.Selection.End);

                        if (end == 0)
                        {
                            //end == 0 means, the user pressed Ctrl + A to select everything
                            end = key.Length;
                        }
                        for (int i = start; i < end; i++)
                        {
                            SetKeyOffset(i);
                            HandleInput("*");
                        }
                        break;
                    }
                    int caretIndex = GetKeyOffset(KeyBox.CaretPosition);
                    if (e.Key == Key.Back)
                    {
                        if (caretIndex == 0)
                        {
                            break;
                        }
                        caretIndex--;
                    }
                    else if (e.Key == Key.Delete && caretIndex == key.Length)
                    {
                        break;
                    }
                    ReplaceCharInKey(key, GetPossibleCharactersAtKeyEnd(key.Substring(0, caretIndex)), '*', caretIndex);
                    SetKeyOffset(caretIndex);
                    break;
                case Key.Home:
                    KeyBox.ScrollToHome();
                    break;
                case Key.End:
                    double x = KeyBox.Document.ContentEnd.GetCharacterRect(LogicalDirection.Forward).X;
                    KeyBox.ScrollToHorizontalOffset(x + KeyBox.HorizontalOffset - KeyBox.ActualWidth + 5);
                    break;
            }
        }

        private bool ReplaceCharInKey(string key, List<char> possibleChars, char inChar, int caretIndex)
        {
            if (string.IsNullOrEmpty(key) || caretIndex >= key.Length)
            {
                return true;
            }

            ElementType elType = GetElementType(key, caretIndex, out int startPosition, out int endPosition);

            switch (inChar)
            {
                case '[':
                    switch (elType)
                    {
                        case ElementType.Joker:
                        case ElementType.Character:
                            if (possibleChars.Count > 1)
                            {
                                key = key.Remove(caretIndex, 1).Insert(caretIndex, "[]");
                                caretIndex++;
                            }
                            break;
                        case ElementType.Group:
                            caretIndex = startPosition + 1;
                            break;
                    }
                    break;
                case ']':
                    if (elType == ElementType.Group)
                    {
                        caretIndex = endPosition + 1;
                    }
                    break;
                case '*':
                    switch (elType)
                    {
                        case ElementType.Joker:
                        case ElementType.Character:
                            if (possibleChars.Count > 1 || possibleChars.Contains('*'))
                            {
                                key = key.Remove(caretIndex, 1).Insert(caretIndex, "*");
                                caretIndex++;
                            }
                            break;
                        case ElementType.Group:
                            key = key.Remove(startPosition, endPosition - startPosition + 1).Insert(startPosition, "*");
                            caretIndex = startPosition + 1;
                            break;
                    }
                    break;
                default:
                    if (possibleChars.Contains(inChar))
                    {
                        switch (elType)
                        {
                            case ElementType.Joker:
                            case ElementType.Character:
                                key = key.Remove(caretIndex, 1).Insert(caretIndex, inChar.ToString());
                                caretIndex++;
                                break;
                            case ElementType.Group:
                                int p = Math.Max(caretIndex, startPosition + 1);
                                key = key.Insert(p, inChar.ToString());
                                caretIndex = p + 1;
                                break;
                        }
                    }
                    else if (inChar == '-')
                    {
                        if (elType == ElementType.Group && caretIndex > startPosition && caretIndex <= endPosition)
                        {
                            key = key.Insert(caretIndex, inChar.ToString());
                            caretIndex++;
                        }
                    }
                    else
                    {
                        //if the user presses an invalid char, the * is added
                        switch (elType)
                        {
                            case ElementType.Joker:
                            case ElementType.Character:
                                key = key.Remove(caretIndex, 1).Insert(caretIndex, "*");
                                caretIndex++;
                                break;
                            case ElementType.Group:
                                //in a group, nothing happens
                                break;
                        }
                    }
                    break;
            }
            KeyManager.SetKey(key);
            SetKeyBox(key, caretIndex);
            return false;
        }

        private int GetKeyOffset(TextPointer caretPosition)
        {
            try
            {
                int count = 0;
                if (caretPosition != null && caretPosition.Paragraph != null)
                {
                    foreach (Inline inline in caretPosition.Paragraph.Inlines)
                    {
                        if (inline != caretPosition.Parent)
                        {
                            count += ((Run)inline).Text.Length;
                        }
                        else
                        {
                            count += caretPosition.GetTextRunLength(LogicalDirection.Backward);
                            return count;
                        }
                    }
                }
            }
            catch (Exception)
            {
                //wtf?
            }
            return 0;
        }


        private void SetKeyOffset(int caretPosition)
        {
            int count = 0;

            //added all the null checks due to a bug that occurs when the user copy and pastes using
            //Ctrl + a, Ctrl + c and then Ctrl + v
            TextPointer caretPos = KeyBox.CaretPosition;
            if (caretPos == null)
            {
                return;
            }
            Paragraph paragraph = caretPos.Paragraph;
            if (paragraph == null)
            {
                return;
            }
            InlineCollection inlines = paragraph.Inlines;
            if (inlines == null)
            {
                return;
            }

            foreach (Inline inline in inlines)
            {
                Run run = (Run)inline;
                if (count + run.Text.Length < caretPosition)
                {
                    count += run.Text.Length;
                }
                else
                {
                    TextPointer caret = run.ContentStart;
                    for (; count < caretPosition; count++)
                    {
                        caret = caret.GetNextInsertionPosition(LogicalDirection.Forward);
                    }
                    KeyBox.CaretPosition = caret;
                    return;
                }
            }
        }

        private void SetKeyBox(string key, int caretIndex)
        {
            Paragraph paragraph = new Paragraph
            {
                TextAlignment = TextAlignment.Left
            };
            int kcount = 0;

            while (kcount < key.Length)
            {
                if (key[kcount] == '[')
                {
                    int start = kcount;
                    bool invalidGroup = CheckGroup(key, ref kcount);
                    kcount++;
                    Run run = new Run(key.Substring(start, kcount - start))
                    {
                        Background = invalidGroup ? Brushes.DarkRed : Brushes.DarkKhaki
                    };
                    paragraph.Inlines.Add(run);
                }
                else
                {
                    paragraph.Inlines.Add(new Run(key[kcount].ToString()));
                    kcount++;
                }
            }

            KeyBoxDocument.Blocks.Clear();
            KeyBoxDocument.Blocks.Add(paragraph);
            SetKeyOffset(caretIndex);
        }

        private void KeyManagerChanged(string key)
        {
            SetValue(CurrentKeyProperty, key);
            SetKeyBox(key, 0);
        }

        private static bool CheckGroup(string key, ref int kcount)
        {
            int state = 0;
            bool invalidGroup = false;
            for (kcount++; key[kcount] != ']'; kcount++)
            {
                if (!invalidGroup)
                {
                    if (key[kcount] == '-')
                    {
                        if (state != 1)
                        {
                            invalidGroup = true;
                        }
                        else
                        {
                            state = 2;
                        }
                    }
                    else
                    {
                        state = state == 2 ? 3 : 1;
                    }
                }
            }

            if (state == 2 || state == 0)
            {
                invalidGroup = true;
            }
            return invalidGroup;
        }

        private enum ElementType { Joker, Character, Group }

        private ElementType GetElementType(string key, int position, out int startPosition, out int endPosition)
        {
            int kcount = 0;

            while (kcount < position)
            {
                if (key[kcount] == '[')
                {
                    if (GetGroupInformations(key, position, out startPosition, out endPosition, ref kcount))
                    {
                        return ElementType.Group;
                    }
                }
                else
                {
                    kcount++;
                }
            }

            startPosition = kcount;
            endPosition = kcount;
            if (key[kcount] == '*')
            {
                return ElementType.Joker;
            }
            else if (key[kcount] == '[')
            {
                GetGroupInformations(key, position, out startPosition, out endPosition, ref kcount);
                return ElementType.Group;
            }
            else
            {
                return ElementType.Character;
            }
        }

        private bool GetGroupInformations(string key, int position, out int startPosition, out int endPosition,
                                                 ref int kcount)
        {
            startPosition = kcount;
            do
            {
                kcount++;
            } while (key[kcount] != ']');
            endPosition = kcount;

            if (kcount >= position)
            {
                return true;
            }
            return false;
        }

        private List<char> GetPossibleCharactersAtKeyEnd(string key)
        {
            string format = KeyManager.GetFormat();
            int fcount = 0;
            int kcount = 0;

            while (kcount < key.Length)
            {
                if (key[kcount] == '[')
                {
                    do
                    {
                        kcount++;
                        if (kcount == key.Length)
                        {
                            return FormatHelper.GetPossibleCharactersFromFormat(format, fcount);
                        }
                    } while (key[kcount] != ']');
                    kcount++;
                    fcount = FormatHelper.GetNextFormatIndex(format, fcount);
                }
                else
                {
                    kcount++;
                    fcount = FormatHelper.GetNextFormatIndex(format, fcount);
                }
            }

            return FormatHelper.GetPossibleCharactersFromFormat(format, fcount);
        }

        private void KeyBox_KeyDown(object sender, KeyEventArgs e)
        {
            //Fix the position of the scrollviewer if necessary:
            Rect rect = KeyBox.CaretPosition.GetCharacterRect(LogicalDirection.Forward);
            if (rect.X < 0)
            {
                KeyBox.ScrollToHorizontalOffset(KeyBox.HorizontalOffset + rect.X - 5);
            }
            if (rect.X + rect.Width > KeyBox.ActualWidth)
            {
                KeyBox.ScrollToHorizontalOffset(KeyBox.HorizontalOffset + (rect.X + rect.Width - KeyBox.ActualWidth) + 5);
            }
        }
    }
}
