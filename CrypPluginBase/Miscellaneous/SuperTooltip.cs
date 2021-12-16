using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CrypTool.PluginBase.Miscellaneous
{
    public class SuperTooltip : ToolTip
    {
        public SuperTooltip(string headline, string tooltip) : this(null, headline, tooltip)
        { }

        public SuperTooltip(Image headlineImage, string headline, string tooltip) : this(headlineImage, headline, tooltip, null)
        { }

        public SuperTooltip(Image headlineImage, string headline, string tooltip, string footer) :
          this(headlineImage, headline, tooltip, null, footer)
        { }

        public SuperTooltip(Image headlineImage, string headline, string tooltip, Image footerImage, string footer)
        {
            if (headline == null || headline == string.Empty)
            {
                throw new ArgumentException("Parameter headline must not be empty.");
            }

            if (tooltip == null || tooltip == string.Empty)
            {
                throw new ArgumentException("Parameter tooltip must not be empty.");
            }

            StackPanel mainStackPanel = new StackPanel();
            StackPanel headerStackPanel = new StackPanel();
            mainStackPanel.Children.Add(headerStackPanel);
            headerStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;

            TextBlock headerTextBlock = new TextBlock
            {
                Background = Brushes.Transparent,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0),
                // headerTextBlock.FontSize = 10;
                FontWeight = FontWeights.Bold
            };
            if (headlineImage != null)
            {
                headerStackPanel.Children.Add(headlineImage);
                headerTextBlock.Text += " ";
            }
            headerTextBlock.Text += headline;
            headerStackPanel.Children.Add(headerTextBlock);
            headerStackPanel.Margin = new Thickness(0, 0, 0, 4);

            TextBlock mainTextBlock = new TextBlock();
            mainTextBlock.Inlines.Add(tooltip);
            mainStackPanel.Children.Add(mainTextBlock);

            if (footer != null && footer != string.Empty)
            {
                Rectangle rect = new Rectangle
                {
                    Margin = new Thickness(3),
                    Stroke = Brushes.LightGray,
                    Height = 1
                };
                mainStackPanel.Children.Add(rect);

                StackPanel footerStackPanel = new StackPanel
                {
                    Orientation = System.Windows.Controls.Orientation.Horizontal
                };

                TextBlock footerTextBlock = new TextBlock
                {
                    Background = Brushes.Transparent,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0)
                };
                if (footerImage != null)
                {
                    footerStackPanel.Children.Add(footerImage);
                    footerTextBlock.Text += " ";
                }
                footerTextBlock.Text += footer;
                footerStackPanel.Children.Add(footerTextBlock);
                mainStackPanel.Children.Add(footerStackPanel);
            }

            Content = mainStackPanel;
        }
    }
}
