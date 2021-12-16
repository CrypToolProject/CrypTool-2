using QuickZip.MiniHtml2;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace QuickZip.UserControls.HtmlTextBlock
{
    public class HtmlTextBlock : TextBlock
    {
        public static DependencyProperty HtmlProperty = DependencyProperty.Register("Html", typeof(string),
                typeof(HtmlTextBlock), new UIPropertyMetadata("Html", new PropertyChangedCallback(OnHtmlChanged)));

        public string Html { get => (string)GetValue(HtmlProperty); set => SetValue(HtmlProperty, value); }

        public static void OnHtmlChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            HtmlTextBlock sender = (HtmlTextBlock)s;

            sender.Inlines.Clear();


            HtmlTagTree tree = new HtmlTagTree();
            HtmlParser parser = new HtmlParser(tree); //output
            parser.Parse(new StringReader(e.NewValue as string));     //input

            HtmlUpdater updater = new HtmlUpdater(sender); //output
            updater.Update(tree);                       //input			
        }

        public HtmlTextBlock()
        {

        }

    }
}
