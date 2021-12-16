using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;


namespace Solitaire
{
    /// <summary>
    /// Interaktionslogik für SolitaireQuickWatchPresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("Solitaire.Properties.Resources")]
    public partial class SolitaireQuickWatchPresentation : System.Windows.Controls.UserControl
    {
        private bool enabled = false;
        private readonly Solitaire plugin;
        private int numberOfCards, i, j;
        //private enum CipherMode { encrypt, decrypt };
        //private CipherMode mode;
        private Solitaire.CipherMode mode;
        private readonly System.Windows.Controls.RichTextBox rtb;
        private int[] oldDeck, newDeck;
        private readonly System.Drawing.Font textFont;
        private readonly System.Drawing.Font symbolFont;
        private Color black = System.Windows.Media.Color.FromRgb(0, 0, 0);
        private Color red = System.Windows.Media.Color.FromRgb(255, 0, 0);

        private const char SPADE = '\u2660';
        private const char HEART = '\u2665';
        private const char DIAMOND = '\u2666';
        private const char CLUB = '\u2663';
        private readonly char[] SUITS = new char[] { CLUB, DIAMOND, HEART, SPADE };
        private readonly string[] RANKS = new string[] { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };

        public SolitaireQuickWatchPresentation(Solitaire plugin)
        {
            this.plugin = plugin;

            InitializeComponent();

            //this.SizeChanged += new EventHandler(this.resetFontSize);
            rtb = richBox;
            textFont = new System.Drawing.Font("Arial", 9F);
            //rtb.Font = textFont;
            symbolFont = new System.Drawing.Font("Arial", 11F);
        }

        public void enable(int numberOfCards, int mode)
        {
            this.mode = (mode == 0) ? Solitaire.CipherMode.encrypt : Solitaire.CipherMode.decrypt;
            this.numberOfCards = numberOfCards;
            enabled = true;
        }

        public void stop()
        {
            enabled = false;
            button1.IsEnabled = false;
            button2.IsEnabled = false;
            button3.IsEnabled = false;
            button4.IsEnabled = false;
            button5.IsEnabled = false;
            button6.IsEnabled = false;
            button7.IsEnabled = false;
        }

        public void clear()
        {
            richBox.Document = new FlowDocument();
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";

            button1.IsEnabled = (((SolitaireSettings)(plugin.Settings)).StreamType == 1);
        }

        private string convertDeckToSymbolDeck(string deck)
        {
            int[] tempDeck = stringToDeck(deck);
            string symbolDeck = "";
            for (int i = 0; i < tempDeck.Length; i++)
            {
                symbolDeck += convertCardNumberToSymbol(tempDeck[i]);
                if (i != tempDeck.Length - 1)
                {
                    symbolDeck += ",";
                }
            }
            return symbolDeck;
        }

        private string convertCardNumberToSymbol(int card)
        {
            if (card >= 1 && card < numberOfCards - 1)
            {
                return SUITS[(card - 1) / 13] + RANKS[(card - 1) % 13];
            }

            if (card == numberOfCards - 1)
            {
                return "A";
            }

            if (card == numberOfCards)
            {
                return "B";
            }

            return "?";
        }

        private void showDeck(string deck)
        {
            TextPointer tp;
            TextRange tr;

            newDeck = stringToDeck(deck);
            rtb.Document = new FlowDocument();
            string text;
            for (int i = 0; i < numberOfCards; i++)
            {
                text = convertCardNumberToSymbol(newDeck[i]);
                tp = rtb.Document.ContentEnd;
                tr = new TextRange(tp, tp)
                {
                    Text = text.Substring(0, 1)
                };
                if (text.Contains(DIAMOND) || text.Contains(HEART))
                {
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
                }
                if (oldDeck[i] != newDeck[i])
                {
                    tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                }
                tp = rtb.Document.ContentEnd;
                tr = new TextRange(tp, tp)
                {
                    Text = text.Substring(1)
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
                if (oldDeck[i] != newDeck[i])
                {
                    tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                }
                if (i != numberOfCards - 1)
                {
                    rtb.AppendText(",");
                }
            }
        }

        private int[] stringToDeck(string seq)
        {
            string[] sequence = seq.Split(new char[] { Convert.ToChar(",") });
            HashSet<string> set = new HashSet<string>(sequence);
            if (set.Count < sequence.Length)
            {
                sequence = new string[set.Count];
                set.CopyTo(sequence);
            }
            int[] deck = new int[numberOfCards];
            for (int i = 0; i < numberOfCards; i++)
            {
                if (sequence[i].Equals("A"))
                {
                    deck[i] = numberOfCards - 1;
                }
                else if (sequence[i].Equals("B"))
                {
                    deck[i] = numberOfCards;
                }
                else
                {
                    deck[i] = int.Parse(sequence[i]);
                }
            }
            return deck;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (enabled)
            {
                //rtb.Rtf = @"{\rtf1\ansi " + plugin.GetDeck(numberOfCards) + "}";
                oldDeck = stringToDeck(plugin.GetDeck(numberOfCards));
                showDeck(plugin.GetDeck(numberOfCards));
                string tmp = plugin.InputString;
                plugin.FormatText(ref tmp);
                textBox2.Text = tmp;
                textBox3.Text = "";
                textBox4.Text = "";
                i = 0;
                j = 0;
                button1.IsEnabled = false;
                button2.IsEnabled = true;
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (enabled)
            {
                oldDeck = stringToDeck(plugin.GetDeck(numberOfCards));
                plugin.MoveCardDown(numberOfCards - 1, numberOfCards);
                showDeck(plugin.GetDeck(numberOfCards));
                button2.IsEnabled = false;
                button3.IsEnabled = true;
                button7.IsEnabled = false;
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            if (enabled)
            {
                oldDeck = stringToDeck(plugin.GetDeck(numberOfCards));
                plugin.MoveCardDown(numberOfCards, numberOfCards);
                plugin.MoveCardDown(numberOfCards, numberOfCards);
                showDeck(plugin.GetDeck(numberOfCards));
                button3.IsEnabled = false;
                button4.IsEnabled = true;
            }
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            if (enabled)
            {
                oldDeck = stringToDeck(plugin.GetDeck(numberOfCards));
                plugin.TripleCut(numberOfCards);
                showDeck(plugin.GetDeck(numberOfCards));
                button4.IsEnabled = false;
                button5.IsEnabled = true;
            }
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            if (enabled)
            {
                oldDeck = stringToDeck(plugin.GetDeck(numberOfCards));
                plugin.CountCut(numberOfCards);
                showDeck(plugin.GetDeck(numberOfCards));
                button5.IsEnabled = false;
                button6.IsEnabled = true;
            }
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            if (enabled)
            {
                if (i < textBox2.Text.Length)
                {
                    encrypt(i++);

                    plugin.OutputString = textBox4.Text;
                    plugin.OutputStream = textBox3.Text;

                    button2.IsEnabled = true;
                    button6.IsEnabled = false;
                    button7.IsEnabled = true;
                }
                else
                {
                    button6.IsEnabled = false;
                    plugin.FinalDeck = plugin.GetDeck(numberOfCards);
                }
            }
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            if (enabled)
            {
                button2.IsEnabled = false;
                button3.IsEnabled = false;
                button4.IsEnabled = false;
                button5.IsEnabled = false;
                button6.IsEnabled = false;
                button7.IsEnabled = false;

                for (; i < textBox2.Text.Length; i++)
                {
                    plugin.PushAndCut(numberOfCards);
                    encrypt(i);
                }

                showDeck(plugin.GetDeck(numberOfCards));
                plugin.FinalDeck = plugin.GetDeck(numberOfCards);
                plugin.OutputStream = textBox3.Text;
                plugin.OutputString = textBox4.Text;
            }
        }

        private void encrypt(int i)
        {
            int curKey = plugin.GetKey(numberOfCards);
            char curChar = plugin.EncryptChar(mode, textBox2.Text[i], curKey, numberOfCards);

            textBox4.Text += curChar;
            textBox3.Text += curKey;

            if (i != textBox2.Text.Length - 1)
            {
                textBox3.Text += ",";
            }

            if (i % 5 == 4)
            {
                textBox4.Text += " ";
            }
        }

    }
}
