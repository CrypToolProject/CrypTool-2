using PKCS1.Library;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace PKCS1.WpfControls.Components
{
    internal static class UserControlHelper
    {

        public static ComboBoxItem GenCBoxItem(string content)
        {
            return GenCBoxItem(content, false);
        }

        public static ComboBoxItem GenCBoxItem(string content, bool isSelected)
        {
            ComboBoxItem returnItem = new ComboBoxItem
            {
                Content = content
            };
            //returnItem.IsSelected = isSelected;
            return returnItem;
        }

        private static TextRange GenTextRange(Run run, int start, int end, Color color)
        {
            TextPointer pointerStart = run.ContentStart.GetPositionAtOffset(start, LogicalDirection.Forward);
            TextPointer pointerEnd = run.ContentStart.GetPositionAtOffset(end, LogicalDirection.Backward);
            TextRange range = new TextRange(pointerStart, pointerEnd);
            range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));

            return range;
        }

        public static int GetRtbTextLength(RichTextBox richTextBox)
        {
            TextRange range = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            return range.Text.Trim().Length;
        }

        public static void loadRtbColoredSig(RichTextBox richTB, string decryptedSig)
        {
            FlowDocument flowDoc = new FlowDocument();
            Paragraph para = new Paragraph();
            Run run = new Run(decryptedSig);
            para.Inlines.Add(run);

            int paddingEnd = decryptedSig.IndexOf("ff0030"); // Ende des Padding
            int identEnd = paddingEnd + 10 + Datablock.getInstance().HashFunctionIdent.DERIdent.Length;
            int digestEnd = identEnd + 2 + (Datablock.getInstance().HashFunctionIdent.digestLength / 4);

            TextRange rangePadding = UserControlHelper.GenTextRange(run, 4, paddingEnd + 2, Colors.Green);
            TextRange rangeIdent = UserControlHelper.GenTextRange(run, paddingEnd + 8, identEnd, Colors.Blue);
            TextRange rangeDigest = UserControlHelper.GenTextRange(run, identEnd, digestEnd, Colors.Red);

            flowDoc.Blocks.Add(para);
            richTB.Document = flowDoc;
        }
    }
}
