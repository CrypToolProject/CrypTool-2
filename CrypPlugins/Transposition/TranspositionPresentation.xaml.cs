using CrypTool.PluginBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Transposition
{
    /// <summary>
    /// Interaktionslogik für TranspositionPresentation.xaml
    /// </summary>
    public partial class TranspositionPresentation : UserControl
    {

        public event EventHandler feuerEnde; //for granting Transposition to fire output after Presentation has finished
        public event EventHandler updateProgress; //updates the status of the plugin
        /// <summary>
        /// Visualisationmodul for Transposition.c
        /// </summary>
        public TranspositionPresentation()
        {
            InitializeComponent();
            SizeChanged += sizeChanged; //fits the quickwatch size
            outPut.SizeChanged += sizeChanged;
            mainStory3.Completed += the_End;
            mainStory2.Completed += my_Help142;
            progressTimer = new DispatcherTimer();
            progressTimer.Tick += progressTimerTick;
            progressTimer.Interval = TimeSpan.FromMilliseconds(1);

            dispo = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 5) // Intervall festlegen, hier 100 ms                       
            };
        }
        /// <summary>
        /// making the presentation scalable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void sizeChanged(object sender, EventArgs eventArgs)
        {
            outPut.MaxWidth = ActualWidth / 4;
            outPut.MaxHeight = ActualHeight;

            Stack.RenderTransform = new ScaleTransform(ActualWidth / Stack.ActualWidth,
                                                            ActualHeight / Stack.ActualHeight);
            outPut.RenderTransform = new ScaleTransform(ActualWidth / outPut.ActualWidth,
                                                            ActualHeight / outPut.ActualHeight);


        }

        #region declarating variables


        private readonly Storyboard mainStory1 = new Storyboard();
        private readonly Storyboard mainStory2 = new Storyboard();
        private readonly Storyboard mainStory3 = new Storyboard();
        private readonly DispatcherTimer progressTimer;
        private TextBlock[,] teba;      // the matrix as array of textblocks
        private int outcount;           // counter
        private int outcount1;          // counter, i tried to reuse the privious but it fails
        private int outcount2;          // counter
        private int outcount3;          // counter
        private int outcount4;          // counter
        private int outcount5;          // counter
        private int countup;            // counter, counts up reverse then the outcounter
        private int countup1;           // counter, i tried to reuse the privious but it fails
        public bool Stop = false;      // boolean to stop animations from being executed
        private int per;                // permutation mode
        private int act;                // permutation action
        private int rein;               // read in mode
        private int reout;              // read out mode
        private int number;             //
        private TextBlock[] reina;      // incoming matrix as texblock array
        private TextBlock[] reouta;     // outgoing matrix as textblock array, never used
        private int speed = 100;          // animation speed
        private int rowper;             // help variable to differnce between permute by row and column
        private int colper;             // help variable to differnce between permute by row and column
        private char[,] read_in_matrix; // read in matrix as byte array
        private char[,] permuted_matrix;    // permuted matrix as byte array
        private Brush[,] mat_back;          // backgrounds of the matrix saved seperatly
        private readonly List<Clock> aniClock = new List<Clock>();
        private int[,] changes;
        private int[] key;                  // key as int array
        public int progress;                // progress variable to update the plugin status
        public int duration = 100;                // progress variable to update the plugin status
        private readonly DispatcherTimer dispo;      //buffers fast input


        #endregion

        #region main
        /// <summary>
        /// main method calling init + create, while keeping the scalability and abortability, only public method
        /// </summary>
        /// <param name="read_in_matrix"></param>
        /// <param name="permuted_matrix"></param>
        /// <param name="key"></param>
        /// <param name="keyword"></param>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="per"></param>
        /// <param name="rein"></param>
        /// <param name="reout"></param>
        /// <param name="act"></param>
        /// <param name="number"></param>
        public void main(char[,] read_in_matrix, char[,] permuted_matrix, int[] key, string keyword, char[] input, char[] output, int per, int rein, int reout, int act, int number, int speed2)
        {
            outPut.Visibility = Visibility.Hidden;
            Stack.Visibility = Visibility.Visible;

            if (keyword != null && input != null)
            {
                init(read_in_matrix, permuted_matrix, keyword, per, rein, reout, act, key, number);
                if (speed > 100)
                {
                    speed = speed2;
                }
                else
                {
                    speed = 100;
                }
                create(read_in_matrix, permuted_matrix, key, keyword, input, output);
                sizeChanged(this, EventArgs.Empty);

            }

        }

        private void t1_Tick(byte[,] read_in_matrix, byte[,] permuted_matrix, int[] key, string keyword, byte[] input, byte[] output, int per, int rein, int reout, int act, int number, int speed2)
        {

            my_Stop(this, EventArgs.Empty);

        }

        public void setinput(byte[,] read_in_matrix, byte[,] permuted_matrix, int[] key, string keyword, byte[] input, byte[] output, int per, int rein, int reout, int act, int number, int speed2)
        {
            dispo.Stop();
            dispo.Interval = new TimeSpan(0, 0, 0, 0, 20);
            dispo.Start();


        }

        #endregion

        #region init
        /// <summary>
        /// initialisation of all params the visualisation needs from the caller
        /// </summary>
        /// <param name="read_in_matrix"></param>
        /// <param name="permuted_matrix"></param>
        /// <param name="keyword"></param>
        /// <param name="per"></param>
        /// <param name="rein"></param>
        /// <param name="reout"></param>
        private string init(char[,] read_in_matrix, char[,] permuted_matrix, string keyword, int per, int rein, int reout, int act, int[] key, int number)
        {

            //background color being created
            GradientStop gs = new GradientStop();
            LinearGradientBrush myBrush = new LinearGradientBrush();
            myBrush.GradientStops.Add(new GradientStop(Colors.SlateGray, 1.0));
            myBrush.GradientStops.Add(new GradientStop(Colors.Silver, 0.5));
            myBrush.GradientStops.Add(new GradientStop(Colors.Gainsboro, 0.0));
            mycanvas.Background = myBrush;

            //cleaning the display for new presentation
            try
            {
                mainGrid.Children.Remove(mywrap2);
                mainGrid.Children.Add(mywrap1);
                mainGrid.Children.Add(myGrid);
            }
            catch { }
            mainGrid.Children.Add(mywrap2);

            //setting all vaiables to starting position, depending on the input and options
            if (act == 0)
            {
                this.rein = rein;
                this.reout = reout;
            }
            else
            {
                this.rein = reout;
                this.reout = rein;
            }



            this.read_in_matrix = read_in_matrix;
            this.permuted_matrix = permuted_matrix;
            this.per = per;
            this.act = act;
            this.key = key;
            this.number = number;

            countup = 0;
            countup1 = 0;
            outcount2 = 0;
            outcount5 = 0;

            if (keyword == null)
            {
                Stop = true;
            }

            textBox2.Clear();

            if (per == 1)
            {
                rowper = 0;
                colper = 2;
                if (this.reout == 1)
                {
                    outcount = 0;
                }
                else
                {
                    outcount = 2;
                }

                if (this.reout == 1)
                {
                    outcount4 = 0;
                }
                else
                {
                    outcount4 = 2;
                }

                if (this.rein == 1)
                {
                    outcount1 = 0;
                }
                else
                {
                    outcount1 = 2;
                }

                if (this.rein == 1)
                {
                    outcount3 = 0;
                }
                else
                {
                    outcount3 = 2;
                }
            }
            else
            {
                rowper = 2;
                colper = 0;
                if (this.reout == 1)
                {
                    outcount = 2;
                }
                else
                {
                    outcount = 0;
                }

                if (this.reout == 1)
                {
                    outcount4 = 2;
                }
                else
                {
                    outcount4 = 0;
                }

                if (this.rein == 1)
                {
                    outcount1 = 2;
                }
                else
                {
                    outcount1 = 0;
                }

                if (this.rein == 1)
                {
                    outcount3 = 2;
                }
                else
                {
                    outcount3 = 0;
                }
            }
            return "";
        }
        #endregion

        #region create

        /// <summary>
        /// method for creating the grid
        /// </summary>
        /// <param name="read_in_matrix"></param>
        /// <param name="permuted_matrix"></param>
        /// <param name="key"></param>
        /// <param name="keyword"></param>
        /// <param name="input"></param>
        /// <param name="output"></param>
        private void create(char[,] read_in_matrix, char[,] permuted_matrix, int[] key, string keyword, char[] input, char[] output)
        {

            aniClock.Clear();
            if (read_in_matrix != null && key != null)
            {//clearing display
                myGrid.Children.Clear();
                myGrid.RowDefinitions.Clear();
                myGrid.ColumnDefinitions.Clear();
                myGrid.ClearValue(WidthProperty);
                myGrid.ClearValue(HeightProperty);
                mywrap1.Width = 160;
                mywrap1.ClearValue(HeightProperty);
                textBox2.ClearValue(HeightProperty);
                label1.Width = 20;
                label2.Width = 20;
                textBox1.Clear();
                textBox2.Clear();
                mywrap1.Children.Clear();
                mywrap2.Children.Clear();
                //statusbar at the left bottom
                if (rein == 0) { textBox2.Text = typeof(Transposition).GetPluginStringResource("reading_in_by_row"); }
                else { textBox2.Text = typeof(Transposition).GetPluginStringResource("reading_in_by_column"); }

                teba = new TextBlock[read_in_matrix.GetLength(0) + rowper, read_in_matrix.GetLength(1) + colper];

                for (int i = 0; i < read_in_matrix.GetLength(0) + rowper; i++)
                {
                    myGrid.ColumnDefinitions.Add(new ColumnDefinition());
                }
                for (int i = 0; i < read_in_matrix.GetLength(1) + colper; i++)
                {
                    myGrid.RowDefinitions.Add(new RowDefinition());
                }
                for (int i = 0; i < key.Length; i++)
                {
                    TextBlock txt = new TextBlock();
                    string s = key[i].ToString();
                    txt.VerticalAlignment = VerticalAlignment.Center;
                    if (act == 0)
                    {
                        txt.Text = s;
                    }
                    else
                    {
                        txt.Text = "" + (i + 1);
                    }

                    txt.FontSize = 12;
                    txt.FontWeight = FontWeights.ExtraBold;
                    txt.TextAlignment = TextAlignment.Center;
                    txt.Width = 17;
                    if (per == 1)
                    {
                        Grid.SetRow(txt, 0);
                        Grid.SetColumn(txt, i);
                        myGrid.Children.Add(txt);
                        teba[i, 0] = txt;
                    }
                    else
                    {
                        Grid.SetRow(txt, i);
                        Grid.SetColumn(txt, 0);
                        myGrid.Children.Add(txt);
                        teba[0, i] = txt;
                    }
                }
                //getting the order of transposition from the keyword or by direct input with commas 
                if (keyword != null)
                {
                    char[] ch = keyword.ToCharArray();
                    if (act == 1)
                    {
                        Array.Sort(ch);
                    }

                    if (!keyword.Contains(','))
                    {
                        for (int i = 0; i < key.Length; i++)
                        {//writing values into the grid
                            TextBlock txt = new TextBlock
                            {
                                VerticalAlignment = VerticalAlignment.Center,
                                Text = ch[i].ToString(),
                                FontSize = 12,
                                FontWeight = FontWeights.ExtraBold,
                                TextAlignment = TextAlignment.Center,
                                Width = 20
                            };

                            if (per == 1)
                            {
                                Grid.SetRow(txt, 1);
                                Grid.SetColumn(txt, i);
                                myGrid.Children.Add(txt);
                                teba[i, 1] = txt;
                            }
                            else
                            {
                                Grid.SetRow(txt, i);
                                Grid.SetColumn(txt, 1);
                                myGrid.Children.Add(txt);
                                teba[1, i] = txt;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < key.Length; i++)
                        {
                            TextBlock txt = new TextBlock
                            {
                                Height = 0,
                                Width = 0
                            };
                            if (per == 1)
                            {
                                Grid.SetRow(txt, 1);
                                Grid.SetColumn(txt, i);
                                myGrid.Children.Add(txt);
                                teba[i, 1] = txt;
                            }
                            else
                            {
                                Grid.SetRow(txt, i);
                                Grid.SetColumn(txt, 1);
                                myGrid.Children.Add(txt);
                                teba[1, i] = txt;
                            }
                        }
                    }

                }
                //creating the chess-like backgroundpattern
                mat_back = new Brush[read_in_matrix.GetLength(0), read_in_matrix.GetLength(1)];
                for (int i = 0; i < read_in_matrix.GetLength(1); i++)
                {
                    int x = 0;
                    if (i % 2 == 0)
                    {
                        x = 1;
                    }

                    for (int ix = 0; ix < read_in_matrix.GetLength(0); ix++)
                    {
                        TextBlock txt = new TextBlock
                        {
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        if (number == 0)
                        {
                            if (Convert.ToInt64(read_in_matrix[ix, i]) != 0)
                            {
                                //filtering the whitespaces out
                                if (31 < Convert.ToInt64(read_in_matrix[ix, i]) && Convert.ToInt64(read_in_matrix[ix, i]) != 127)
                                { txt.Text = Convert.ToChar(read_in_matrix[ix, i]).ToString(); }
                                else
                                {
                                    txt.Text = "/" + Convert.ToInt64(read_in_matrix[ix, i]).ToString("X");

                                }
                            }
                            else
                            {
                                txt.Text = "";
                            }
                        }
                        else
                            if (Convert.ToInt64(read_in_matrix[ix, i]) != 0)
                        {
                            txt.Text = "/" + ((int)read_in_matrix[ix, i]).ToString("X");
                        }

                        if (ix % 2 == x)
                        {
                            mat_back[ix, i] = Brushes.AliceBlue;
                        }
                        else
                        {
                            mat_back[ix, i] = Brushes.LawnGreen;
                        }

                        txt.Background = Brushes.Yellow;
                        txt.FontSize = 12;
                        txt.Opacity = 1.0;
                        txt.FontWeight = FontWeights.ExtraBold;
                        txt.TextAlignment = TextAlignment.Center;
                        txt.Width = 20;
                        Grid.SetRow(txt, (i + colper));
                        Grid.SetColumn(txt, (ix + rowper));
                        myGrid.Children.Add(txt);
                        teba[(ix + rowper), (i + colper)] = txt;
                        teba[(ix + rowper), (i + colper)].Opacity = 0.0;
                    }
                }
                if (input.Length >= key.Length)
                {
                    reina = new TextBlock[input.Length];
                }
                else
                {
                    reina = new TextBlock[key.Length];
                }

                for (int i = 0; i < input.Length; i++)
                {
                    TextBlock txt = new TextBlock
                    {
                        FontSize = 12,
                        FontWeight = FontWeights.ExtraBold
                    };
                    if (number == 0) //special hexmode handling
                    {
                        if (31 < Convert.ToInt64(input[i]) && Convert.ToInt64(input[i]) != 127)
                        {
                            txt.Text = Convert.ToChar(input[i]).ToString();
                        }
                        else
                        {
                            txt.Text = "/" + Convert.ToInt64(input[i]).ToString("X");
                        }
                    }
                    else
                    {
                        txt.Text = "/" + ((int)input[i]).ToString("X");
                    }

                    reina[i] = txt;
                    reina[i].Background = Brushes.Transparent;
                    mywrap1.Children.Add(txt);
                }
                if (input.Length < key.Length)
                {
                    for (int i = input.Length; i < key.Length; i++)
                    {
                        TextBlock txt = new TextBlock
                        {
                            FontSize = 12,
                            FontWeight = FontWeights.ExtraBold,
                            Text = ""
                        };
                        reina[i] = txt;
                        reina[i].Background = Brushes.Transparent;
                        mywrap1.Children.Add(txt);
                    }
                }

                reouta = new TextBlock[output.Length];
                for (int i = 0; i < output.Length; i++)
                {
                    TextBlock txt = new TextBlock
                    {
                        FontSize = 12,
                        FontWeight = FontWeights.ExtraBold
                    };
                    if (number == 0)
                    {
                        //---marker
                        // if (31 < Convert.ToInt64(output[i]) && Convert.ToInt64(output[i]) != 127)
                        // { 
                        //   txt.Text = Convert.ToChar(output[i]).ToString(); 
                        // }
                        //  else
                        // {
                        //      txt.Text = "/" + Convert.ToInt64(output[i]).ToString("X");  
                        //  }
                        txt.Text = Convert.ToChar(output[i]).ToString();
                    }
                    /*if you comment the line above 
* and re-comment the lines to the "---marker"
* you will get shown the whitespaces in hexcode in the READOUT of the Presentation*/

                    else
                    {
                        txt.Text = "/" + ((int)output[i]).ToString("X");
                    }

                    reouta[i] = txt;
                    reouta[i].Background = Brushes.Orange;
                    reouta[i].Opacity = 0.0;
                    mywrap2.Children.Add(txt);
                }
            }
            mainStory1.Children.Clear();
            mainStory2.Children.Clear();
            mainStory3.Children.Clear();
            DoubleAnimation nop = new DoubleAnimation(0.0, 0.0, new Duration(TimeSpan.FromMilliseconds((1001))));

            mainStory1.Children.Add(nop);
            nop.Completed += new EventHandler(my_Helpnop);
            Storyboard.SetTarget(nop, Stack);
            Storyboard.SetTargetProperty(nop, new PropertyPath("(Opacity)"));
            mainStory1.Begin();
            mainStory1.SetSpeedRatio(speed / 100);


        }

        private void my_Helpnop(object sender, EventArgs e)
        {
            mainStory1.Children.Clear();
            Stop = false;
            sizeChanged(this, EventArgs.Empty);

            DoubleAnimation fadeIn = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds((1001))));
            DoubleAnimation fadeIn2 = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds((1001))));
            mainStory1.Children.Add(fadeIn);
            mainStory1.Children.Add(fadeIn2);

            Storyboard.SetTarget(fadeIn, Stack);
            Storyboard.SetTarget(fadeIn2, textBox2);


            Storyboard.SetTargetProperty(fadeIn, new PropertyPath("(Opacity)"));
            Storyboard.SetTargetProperty(fadeIn2, new PropertyPath("(Opacity)"));

            fadeIn.Completed += new EventHandler(my_Help3);

            mainStory1.Begin();
            mainStory1.SetSpeedRatio(speed / 100);

        }

        private void my_Help3(object sender, EventArgs e)
        {
            sizeChanged(this, EventArgs.Empty);
            if (!Stop)
            {
                preReadIn();
            }
        }

        #endregion

        #region readIn

        /// <summary>
        /// coloranimation for the text in the left wrapper to be "eaten out" and getting marked
        /// </summary>
        private void preReadIn()
        {   //declaring color animations
            mainStory1.Children.Clear();
            progressTimer.Start();
            int abort = teba.GetLength(0) - rowper;
            if (rein == 0)
            {
                abort = teba.GetLength(1) - colper;
            }

            for (int ix = 0; ix < abort; ix++)
            {
                ColorAnimation myColorAnimation = new ColorAnimation(Colors.Transparent, Colors.Orange, new Duration(TimeSpan.FromMilliseconds(1001)))
                {
                    BeginTime = TimeSpan.FromMilliseconds(0 + ix * 3000)
                };
                bool mycolset = false;

                if (reina != null)
                {
                    SolidColorBrush brush = new SolidColorBrush
                    {
                        Color = Colors.Transparent
                    };

                    int pos = countup;
                    if (rein == 0)
                    {

                        for (int i = 0; i < read_in_matrix.GetLength(0); i++)
                        {
                            if (Convert.ToInt64(read_in_matrix[i, outcount2]) != 0)
                            {
                                reina[countup].Background = brush;
                                Storyboard.SetTarget(myColorAnimation, reina[countup]);
                                mycolset = true;
                                countup++;
                            }
                        }
                    }
                    else
                    {

                        for (int i = 0; i < read_in_matrix.GetLength(1); i++)
                        {
                            if (Convert.ToInt64(read_in_matrix[outcount2, i]) != 0)
                            {

                                reina[countup].Background = brush;
                                Storyboard.SetTarget(myColorAnimation, reina[countup]);
                                mycolset = true;
                                countup++;
                            }
                        }
                    }
                    Storyboard.SetTargetProperty(myColorAnimation, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                    outcount2++;

                    for (int i = pos; i < countup; i++)
                    {
                        DoubleAnimation myFadeOut = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromMilliseconds(1001)))
                        {
                            BeginTime = TimeSpan.FromMilliseconds(1001 + ix * 3000)
                        };
                        mainStory1.Children.Add(myFadeOut);
                        Storyboard.SetTarget(myFadeOut, reina[i]);
                        Storyboard.SetTargetProperty(myFadeOut, new PropertyPath("(Opacity)"));
                    }
                    if (teba != null)
                    {
                        if (rein == 0)
                        {
                            for (int i = rowper; i < teba.GetLength(0); i++)
                            {
                                DoubleAnimation fadeIn = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(1001)))
                                {
                                    BeginTime = TimeSpan.FromMilliseconds(1001 + ix * 3000)
                                };
                                mainStory1.Children.Add(fadeIn);
                                Storyboard.SetTargetProperty(fadeIn, new PropertyPath("(Opacity)"));
                                Storyboard.SetTarget(fadeIn, teba[i, outcount1]);
                            }
                        }
                        else
                        {
                            for (int i = colper; i < teba.GetLength(1); i++)
                            {
                                DoubleAnimation fadeIn = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(1001)))
                                {
                                    BeginTime = TimeSpan.FromMilliseconds(1001 + ix * 3000)
                                };
                                mainStory1.Children.Add(fadeIn);
                                Storyboard.SetTargetProperty(fadeIn, new PropertyPath("(Opacity)"));
                                Storyboard.SetTarget(fadeIn, teba[outcount1, i]);
                            }
                        }
                        outcount1++;
                    }

                    ColorAnimation myColorAnimation_green = new ColorAnimation(Colors.Yellow, Colors.LawnGreen, new Duration(TimeSpan.FromMilliseconds(1001)))
                    {
                        BeginTime = TimeSpan.FromMilliseconds((2001 + ix * 3000))
                    };

                    ColorAnimation myColorAnimation_blue = new ColorAnimation(Colors.Yellow, Colors.AliceBlue, new Duration(TimeSpan.FromMilliseconds(1001)))
                    {
                        BeginTime = TimeSpan.FromMilliseconds((2001 + ix * 3000))
                    };

                    bool tarsetgr = false;
                    bool tarsetbl = false;

                    SolidColorBrush brush_green = new SolidColorBrush(Colors.Yellow);
                    SolidColorBrush brush_blue = new SolidColorBrush(Colors.Yellow);

                    if (teba != null)
                    {
                        if (rein == 0)
                        {
                            for (int i = rowper; i < teba.GetLength(0); i++)
                            {
                                if (mat_back[i - rowper, outcount3 - colper] == Brushes.LawnGreen)
                                {
                                    teba[i, outcount3].Background = brush_green;
                                    Storyboard.SetTarget(myColorAnimation_green, teba[i, outcount3]);
                                    tarsetgr = true;
                                }
                                else
                                {
                                    teba[i, outcount3].Background = brush_blue;
                                    Storyboard.SetTarget(myColorAnimation_blue, teba[i, outcount3]);
                                    tarsetbl = true;
                                }

                            }
                        }
                        else
                        {
                            for (int i = colper; i < teba.GetLength(1); i++)
                            {
                                if (mat_back[outcount3 - rowper, i - colper] == Brushes.LawnGreen)
                                {
                                    teba[outcount3, i].Background = brush_green;
                                    Storyboard.SetTarget(myColorAnimation_green, teba[outcount3, i]);
                                    tarsetgr = true;
                                }
                                else
                                {
                                    teba[outcount3, i].Background = brush_blue;
                                    Storyboard.SetTarget(myColorAnimation_blue, teba[outcount3, i]);
                                    tarsetbl = true;
                                }

                            }
                        }

                        if (tarsetgr)
                        {
                            mainStory1.Children.Add(myColorAnimation_green);
                        }

                        if (tarsetbl)
                        {
                            mainStory1.Children.Add(myColorAnimation_blue);
                        }

                        Storyboard.SetTargetProperty(myColorAnimation_green, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                        Storyboard.SetTargetProperty(myColorAnimation_blue, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                    }
                    if (mycolset)
                    {
                        mainStory1.Children.Add(myColorAnimation);
                    }
                }
                outcount3++;
            }
            mainStory1.Completed += new EventHandler(my_Help14);
            if (!Stop)
            {
                mainStory1.Begin();
                mainStory1.SetSpeedRatio(speed / 100);
            }


        }


        private void my_Help14(object sender, EventArgs e)
        {
            mainStory1.Completed -= my_Help14;
            GradientStop gs1 = new GradientStop(Colors.Red, 1.0);
            GradientStop gs2 = new GradientStop(Colors.Pink, 0.5);
            GradientStop gs3 = new GradientStop(Colors.PaleVioletRed, 0.0);
            ColorAnimation myColorAnimation_rg = new ColorAnimation
            {
                From = Colors.SlateGray,
                To = Colors.CornflowerBlue,
                Duration = new Duration(TimeSpan.FromMilliseconds(1001))
            };

            ColorAnimation myColorAnimation_pl = new ColorAnimation
            {
                From = Colors.Silver,
                To = Colors.SkyBlue,
                Duration = new Duration(TimeSpan.FromMilliseconds(1001))
            };

            ColorAnimation myColorAnimation_pd = new ColorAnimation
            {
                From = Colors.Gainsboro,
                To = Colors.PowderBlue,
                Duration = new Duration(TimeSpan.FromMilliseconds(1001))
            };

            LinearGradientBrush myBrush = new LinearGradientBrush();
            myBrush.GradientStops.Add(gs1);
            myBrush.GradientStops.Add(gs2);
            myBrush.GradientStops.Add(gs3);

            gs1.BeginAnimation(GradientStop.ColorProperty, myColorAnimation_rg);
            gs2.BeginAnimation(GradientStop.ColorProperty, myColorAnimation_pl);
            gs3.BeginAnimation(GradientStop.ColorProperty, myColorAnimation_pd);

            mycanvas.Background = myBrush;
            if (!Stop)
            {
                sort2();
            }
        }



        #endregion

        #region sorting

        /// <summary>
        /// (Insertion Sort) algorithm for sorting the rows OR columns by index during the permutationphase
        /// </summary>
        /// <param name="i"></param>
        /// 

        private void sort2()
        {
            if (per == 1)
            {
                changes = new int[teba.GetLength(0), 2];
            }
            else
            {
                changes = new int[teba.GetLength(1), 2];
            }

            mainStory2.Children.Clear();
            //statusbar update
            if (per == 0) { textBox2.Text = typeof(Transposition).GetPluginStringResource("permuting_by_row"); }
            else { textBox2.Text = typeof(Transposition).GetPluginStringResource("permuting_by_column"); }


            if (per == 1)
            {
                int[] tesave = new int[teba.GetLength(0)];
                for (int ix = 0; ix < teba.GetLength(0); ix++)
                {
                    tesave[ix] = Convert.ToInt32(teba[ix, 0].Text);
                }


                if (teba != null && key != null)
                {

                    int counter = 0;
                    for (int i = 0; i < tesave.Length; i++)
                    {

                        if (i < teba.GetLength(0))
                        {
                            int s = 0;
                            bool goon = false;
                            if (act == 0)
                            {
                                if (tesave[i] != i + 1)
                                {
                                    goon = true;

                                    for (int ix = i + 1; ix < tesave.Length; ix++)
                                    {
                                        if (tesave[ix] == i + 1)
                                        {
                                            s = ix;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (tesave[i] != key[i])
                                {
                                    goon = true;

                                    for (int ix = i + 1; ix < tesave.Length; ix++)
                                    {
                                        if (tesave[ix] == key[i])
                                        {
                                            s = ix;
                                        }
                                    }
                                }
                            }

                            if (goon)
                            {
                                Storyboard[] stb = preaniboard(i, s);
                                stb[0].BeginTime = TimeSpan.FromMilliseconds(4000 * counter);
                                stb[1].BeginTime = TimeSpan.FromMilliseconds(4000 * counter + 2000);
                                stb[2].BeginTime = TimeSpan.FromMilliseconds(4000 * counter + 3000);
                                mainStory2.Children.Add(stb[0]);
                                mainStory2.Children.Add(stb[1]);
                                mainStory2.Children.Add(stb[2]);



                                Clock testclock = stb[0].CreateClock();

                                testclock.Controller.SpeedRatio = speed / 100;
                                testclock.Completed += new EventHandler(my_Completed2);
                                aniClock.Add(testclock);

                                changes[counter, 0] = i;
                                changes[counter, 1] = s;
                                counter++;

                                int help = tesave[i];
                                tesave[i] = tesave[s];
                                tesave[s] = help;

                            }

                        }
                    }
                }

            }

            else
            {
                int[] tesave = new int[teba.GetLength(1)];
                for (int ix = 0; ix < teba.GetLength(1); ix++)
                {
                    tesave[ix] = Convert.ToInt32(teba[0, ix].Text);
                }


                if (teba != null && key != null)
                {

                    int counter = 0;
                    for (int i = 0; i < tesave.Length; i++)
                    {

                        if (i < teba.GetLength(1))
                        {
                            int s = 0;
                            bool goon = false;
                            if (act == 0)
                            {
                                if (tesave[i] != i + 1)
                                {
                                    goon = true;
                                    for (int ix = i + 1; ix < tesave.Length; ix++)
                                    {
                                        if (tesave[ix] == i + 1)
                                        {
                                            s = ix;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (tesave[i] != key[i])
                                {
                                    goon = true;
                                    for (int ix = i + 1; ix < tesave.Length; ix++)
                                    {
                                        if (tesave[ix] == key[i])
                                        {
                                            s = ix;
                                        }
                                    }
                                }
                            }

                            if (goon)
                            {
                                Storyboard[] stb = preaniboard(i, s);
                                stb[0].BeginTime = TimeSpan.FromMilliseconds(4000 * counter);
                                stb[1].BeginTime = TimeSpan.FromMilliseconds(4000 * counter + 2000);
                                stb[2].BeginTime = TimeSpan.FromMilliseconds(4000 * counter + 3000);
                                mainStory2.Children.Add(stb[0]);
                                mainStory2.Children.Add(stb[1]);
                                mainStory2.Children.Add(stb[2]);



                                Clock testclock = stb[0].CreateClock();

                                testclock.Controller.SpeedRatio = speed / 100;
                                testclock.Completed += new EventHandler(my_Completed2);
                                aniClock.Add(testclock);

                                changes[counter, 0] = i;
                                changes[counter, 1] = s;
                                counter++;

                                int help = tesave[i];
                                tesave[i] = tesave[s];
                                tesave[s] = help;

                            }

                        }
                    }
                }

            }

            DoubleAnimation nop = new DoubleAnimation(1.0, 1.0, new Duration(TimeSpan.FromMilliseconds((1001)))); //important because storyboard won't start empty
            mainStory2.Children.Add(nop);
            Storyboard.SetTarget(nop, Stack);
            Storyboard.SetTargetProperty(nop, new PropertyPath("(Opacity)"));
            mainStory2.Begin();
            mainStory2.SetSpeedRatio(speed / 100);
        }

        private void my_Help142(object sender, EventArgs e)
        {
            if (!Stop)
            {
                preReadOut();
            }
            //mainStory2.Completed -= my_Help142;
            GradientStop gs1 = new GradientStop(Colors.Red, 1.0);
            GradientStop gs2 = new GradientStop(Colors.Pink, 0.5);
            GradientStop gs3 = new GradientStop(Colors.PaleVioletRed, 0.0);
            ColorAnimation myColorAnimation_rg = new ColorAnimation
            {
                From = Colors.CornflowerBlue,
                To = Colors.YellowGreen,
                Duration = new Duration(TimeSpan.FromMilliseconds(1001))
            };

            ColorAnimation myColorAnimation_pl = new ColorAnimation
            {
                From = Colors.SkyBlue,
                To = Colors.GreenYellow,
                Duration = new Duration(TimeSpan.FromMilliseconds(1001))
            };

            ColorAnimation myColorAnimation_pd = new ColorAnimation
            {
                From = Colors.PowderBlue,
                To = Colors.LawnGreen,
                Duration = new Duration(TimeSpan.FromMilliseconds(1001))
            };

            LinearGradientBrush myBrush = new LinearGradientBrush();
            myBrush.GradientStops.Add(gs1);
            myBrush.GradientStops.Add(gs2);
            myBrush.GradientStops.Add(gs3);

            gs1.BeginAnimation(GradientStop.ColorProperty, myColorAnimation_rg);
            gs2.BeginAnimation(GradientStop.ColorProperty, myColorAnimation_pl);
            gs3.BeginAnimation(GradientStop.ColorProperty, myColorAnimation_pd);

            mycanvas.Background = myBrush;


        }

        #endregion

        #region readout



        private void preReadOut()
        {   //declarating coloranimations and brushes

            //statusbar update
            if (reout == 0) { textBox2.Text = typeof(Transposition).GetPluginStringResource("reading_out_by_row"); }
            else { textBox2.Text = typeof(Transposition).GetPluginStringResource("reading_out_by_column"); }
            //declarating fading animations
            mainStory3.Children.Clear();

            int abort = teba.GetLength(0) - rowper;
            if (reout == 0)
            {
                abort = teba.GetLength(1) - colper;
            }

            for (int ix = 0; ix < abort; ix++)
            {

                ColorAnimation myColorAnimation_green = new ColorAnimation(Colors.LawnGreen, Colors.Yellow, new Duration(TimeSpan.FromMilliseconds(1000)))
                {
                    BeginTime = TimeSpan.FromMilliseconds(ix * 3000 + 0)
                };

                ColorAnimation myColorAnimation_blue = new ColorAnimation(Colors.AliceBlue, Colors.Yellow, new Duration(TimeSpan.FromMilliseconds(1000)))
                {
                    BeginTime = TimeSpan.FromMilliseconds(ix * 3000 + 0)
                };

                SolidColorBrush brush_green = new SolidColorBrush(Colors.LawnGreen);
                SolidColorBrush brush_blue = new SolidColorBrush(Colors.AliceBlue);
                SolidColorBrush brush_trans = new SolidColorBrush(Colors.Orange);

                if (teba != null)
                {
                    if (reout == 0)
                    {
                        for (int i = rowper; i < teba.GetLength(0); i++)
                        {
                            if (mat_back[i - rowper, outcount4 - colper] == Brushes.LawnGreen)
                            {

                                mainStory3.Children.Add(myColorAnimation_green);

                                teba[i, outcount4].Background = brush_green;
                                Storyboard.SetTarget(myColorAnimation_green, teba[i, outcount4]);
                            }
                            else
                            {
                                mainStory3.Children.Add(myColorAnimation_blue);
                                teba[i, outcount4].Background = brush_blue;
                                Storyboard.SetTarget(myColorAnimation_blue, teba[i, outcount4]);
                            }
                            DoubleAnimation fadeOut = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromMilliseconds(1000)))
                            {
                                BeginTime = TimeSpan.FromMilliseconds(ix * 3000 + 1000)
                            };
                            mainStory3.Children.Add(fadeOut);
                            Storyboard.SetTarget(fadeOut, teba[i, outcount]);
                            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("(Opacity)"));

                        }
                    }
                    else
                    {
                        for (int i = colper; i < teba.GetLength(1); i++)
                        {
                            if (mat_back[outcount4 - rowper, i - colper] == Brushes.LawnGreen)
                            {
                                mainStory3.Children.Add(myColorAnimation_green);
                                teba[outcount4, i].Background = brush_green;
                                Storyboard.SetTarget(myColorAnimation_green, teba[outcount4, i]);
                            }
                            else
                            {
                                mainStory3.Children.Add(myColorAnimation_blue);
                                teba[outcount4, i].Background = brush_blue;
                                Storyboard.SetTarget(myColorAnimation_blue, teba[outcount4, i]);
                            }
                            DoubleAnimation fadeOut = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromMilliseconds(1000)))
                            {
                                BeginTime = TimeSpan.FromMilliseconds(ix * 3000 + 1000)
                            };
                            mainStory3.Children.Add(fadeOut);
                            Storyboard.SetTarget(fadeOut, teba[outcount, i]);
                            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("(Opacity)"));
                        }
                    }

                    outcount4++;

                    Storyboard.SetTargetProperty(myColorAnimation_green, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                    Storyboard.SetTargetProperty(myColorAnimation_blue, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));

                    ColorAnimation myColorAnimation_ot = new ColorAnimation(Colors.Orange, Colors.Transparent, new Duration(TimeSpan.FromMilliseconds(1000)))
                    {
                        BeginTime = TimeSpan.FromMilliseconds(ix * 3000 + 2000)
                    };


                    if (teba != null)
                    {
                        if (reout == 1)
                        {
                            for (int i = 0; i < permuted_matrix.GetLength(1); i++)
                            {
                                if (Convert.ToInt64(permuted_matrix[outcount5, i]) != 0)
                                {
                                    mainStory3.Children.Add(myColorAnimation_ot);
                                    DoubleAnimation fadeIn = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(1000)))
                                    {
                                        BeginTime = TimeSpan.FromMilliseconds(ix * 3000 + 1000)
                                    };
                                    mainStory3.Children.Add(fadeIn);
                                    Storyboard.SetTarget(fadeIn, reouta[countup1]);
                                    Storyboard.SetTargetProperty(fadeIn, new PropertyPath("(Opacity)"));

                                    reouta[countup1].Background = brush_trans;
                                    Storyboard.SetTarget(myColorAnimation_ot, reouta[countup1]);

                                    countup1++;

                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < permuted_matrix.GetLength(0); i++)
                            {
                                if (Convert.ToInt64(permuted_matrix[i, outcount5]) != 0)
                                {
                                    mainStory3.Children.Add(myColorAnimation_ot);
                                    DoubleAnimation fadeIn = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(1000)))
                                    {
                                        BeginTime = TimeSpan.FromMilliseconds(ix * 3000 + 1000)
                                    };
                                    mainStory3.Children.Add(fadeIn);
                                    Storyboard.SetTarget(fadeIn, reouta[countup1]);
                                    Storyboard.SetTargetProperty(fadeIn, new PropertyPath("(Opacity)"));

                                    reouta[countup1].Background = brush_trans;
                                    Storyboard.SetTarget(myColorAnimation_ot, reouta[countup1]);

                                    countup1++;
                                }
                            }

                        }
                    }
                    Storyboard.SetTargetProperty(myColorAnimation_ot, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                    outcount++;
                    outcount5++;
                }


            }
            mainStory3.Begin();
            mainStory3.SetSpeedRatio(speed / 100);
        }

        /// <summary>
        /// method for "eating out" the grid feeding the output wrapper (right one)
        /// </summary>
        /// 

        //post highlighting of the read out values

        private void the_End(object sender, EventArgs e)
        {
            textBox2.Text = typeof(Transposition).GetPluginStringResource("accomplished"); //finish
            feuerEnde(this, EventArgs.Empty);

            outPut.Visibility = Visibility.Visible;
            Stack.Visibility = Visibility.Hidden;
            outPut.Text = "";
            foreach (TextBlock t in mywrap2.Children)
            {
                outPut.Text += t.Text;
            }





            /*
            DoubleAnimation fadeOut2 = new DoubleAnimation();
            fadeOut2.From = 1.0;
            fadeOut2.To = 0.0;
            fadeOut2.Duration = new Duration(TimeSpan.FromMilliseconds((1001 - speed)));

            DoubleAnimation fadeOut = new DoubleAnimation();
            fadeOut.From = 1.0;
            fadeOut.To = 0.0;
            fadeOut.Duration = new Duration(TimeSpan.FromMilliseconds((1001 - speed)));


            fadeOut2.Completed += new EventHandler(endhelper);
            mywrap1.BeginAnimation(OpacityProperty, fadeOut2);

            myGrid.BeginAnimation(OpacityProperty, fadeOut);
            textBox2.BeginAnimation(OpacityProperty, fadeOut);*/
        }

        #endregion

        #region events

        /// <summary>
        /// "emergengy break" 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void my_Stop(object sender, EventArgs e)
        {   //resetting the grid

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    outPut.Visibility = Visibility.Hidden;
                    Stack.Visibility = Visibility.Visible;
                    outPut.Text = "";
                    progress = 0;
                    progressTimer.Stop();
                    mainStory1.Stop();
                    mainStory2.Stop();
                    mainStory3.Stop();
                    mainStory1.Children.Clear();
                    mainStory2.Children.Clear();
                    mainStory3.Children.Clear();

                    foreach (Clock cl in aniClock)
                    {
                        cl.Controller.Stop();
                    }
                    aniClock.Clear();

                    myGrid.Children.Clear();
                    myGrid.ColumnDefinitions.Clear();
                    myGrid.RowDefinitions.Clear();
                    mywrap1.Children.Clear();
                    mywrap2.Children.Clear();
                    outcount = 0;
                    textBox2.Clear();
                    Stop = true;
                    feuerEnde(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    Transposition.Transposition_LogMessage(string.Format("Exception during run of my_Stop of Transposition Presentation: {0}", ex.Message), NotificationLevel.Error);
                }

            }, null);
        }
        #region eventhandler

        private void endhelper(object sender, EventArgs e)
        {
            myGrid.Width = 0;
            mywrap1.Width = 0;
            myGrid.Height = 0;
            mywrap1.Height = 0;
            textBox2.Height = 0;
            label1.Width = 0;
            label2.Width = 0;
            sizeChanged(this, EventArgs.Empty);
            DoubleAnimation fadeOut2 = new DoubleAnimation
            {
                From = 0.0,
                To = 0.1,
                Duration = new Duration(TimeSpan.FromMilliseconds((1001 - speed)))
            };

            fadeOut2.Completed += new EventHandler(my_Help13);
            textBox2.BeginAnimation(OpacityProperty, fadeOut2);
        }

        private void my_Help13(object sender, EventArgs e)
        {
            sizeChanged(this, EventArgs.Empty);
            textBox2.Text = typeof(Transposition).GetPluginStringResource("accomplished"); //finish
            feuerEnde(this, EventArgs.Empty);
        }



        #endregion
        #endregion

        #region animations
        /// <summary>
        /// method for preanimating 
        /// </summary>
        /// <param name="von"></param>
        /// <param name="nach"></param>


        private Storyboard[] preaniboard(int von, int nach)
        {
            Storyboard[] returnboard = new Storyboard[3];
            Storyboard subreturnboard = new Storyboard();
            Storyboard subreturnboard2 = new Storyboard();
            Storyboard subreturnboard3 = new Storyboard();

            #region declarating coloranimations und brushes
            ColorAnimation myColorAnimation_gy = new ColorAnimation(Colors.LawnGreen, Colors.Yellow, new Duration(TimeSpan.FromMilliseconds(1000)));
            ColorAnimation myColorAnimation_by = new ColorAnimation(Colors.AliceBlue, Colors.Yellow, new Duration(TimeSpan.FromMilliseconds(1000)));
            ColorAnimation myColorAnimation_ty = new ColorAnimation(Colors.Transparent, Colors.Yellow, new Duration(TimeSpan.FromMilliseconds(1000)));

            ColorAnimation myColorAnimation_boy = new ColorAnimation(Colors.Orange, Colors.Yellow, new Duration(TimeSpan.FromMilliseconds(0)));
            ColorAnimation myColorAnimation_gyo = new ColorAnimation(Colors.Yellow, Colors.Orange, new Duration(TimeSpan.FromMilliseconds(0)))
            {
                BeginTime = TimeSpan.FromMilliseconds(1999)
            };
            myColorAnimation_boy.BeginTime = TimeSpan.FromMilliseconds(1999);

            ColorAnimation myColorAnimation_oy = new ColorAnimation(Colors.Orange, Colors.Yellow, new Duration(TimeSpan.FromMilliseconds(0)));
            ColorAnimation myColorAnimation_yo = new ColorAnimation(Colors.Yellow, Colors.Orange, new Duration(TimeSpan.FromMilliseconds(0)))
            {
                BeginTime = TimeSpan.FromMilliseconds(1999)
            };
            myColorAnimation_oy.BeginTime = TimeSpan.FromMilliseconds(1999);

            ColorAnimation myColorAnimation_toy = new ColorAnimation(Colors.Orange, Colors.Yellow, new Duration(TimeSpan.FromMilliseconds(0)));
            ColorAnimation myColorAnimation_tyo = new ColorAnimation(Colors.Yellow, Colors.Orange, new Duration(TimeSpan.FromMilliseconds(0)))
            {
                BeginTime = TimeSpan.FromMilliseconds(1999)
            };
            myColorAnimation_toy.BeginTime = TimeSpan.FromMilliseconds(1999);



            ColorAnimation myColorAnimation_yg = new ColorAnimation(Colors.Yellow, Colors.LawnGreen, new Duration(TimeSpan.FromMilliseconds(1001)));
            ColorAnimation myColorAnimation_yb = new ColorAnimation(Colors.Yellow, Colors.AliceBlue, new Duration(TimeSpan.FromMilliseconds(1001)));
            ColorAnimation myColorAnimation_yt = new ColorAnimation(Colors.Yellow, Colors.Transparent, new Duration(TimeSpan.FromMilliseconds(1001)));


            SolidColorBrush brush_gy = new SolidColorBrush(Colors.LawnGreen);
            SolidColorBrush brush_by = new SolidColorBrush(Colors.AliceBlue);
            SolidColorBrush brush_ty = new SolidColorBrush(Colors.Transparent);

            ColorAnimation myColorAnimation_go = new ColorAnimation(Colors.LawnGreen, Colors.Orange, new Duration(TimeSpan.FromMilliseconds(1001)));
            ColorAnimation myColorAnimation_bo = new ColorAnimation(Colors.AliceBlue, Colors.Orange, new Duration(TimeSpan.FromMilliseconds(1001)));
            ColorAnimation myColorAnimation_to = new ColorAnimation(Colors.Transparent, Colors.Orange, new Duration(TimeSpan.FromMilliseconds(1001)));

            ColorAnimation myColorAnimation_og = new ColorAnimation(Colors.Orange, Colors.LawnGreen, new Duration(TimeSpan.FromMilliseconds(1001)));
            ColorAnimation myColorAnimation_ob = new ColorAnimation(Colors.Orange, Colors.AliceBlue, new Duration(TimeSpan.FromMilliseconds(1001)));
            ColorAnimation myColorAnimation_ot = new ColorAnimation(Colors.Orange, Colors.Transparent, new Duration(TimeSpan.FromMilliseconds(1001)));


            SolidColorBrush brush_go = new SolidColorBrush(Colors.LawnGreen);
            SolidColorBrush brush_bo = new SolidColorBrush(Colors.AliceBlue);
            SolidColorBrush brush_to = new SolidColorBrush(Colors.Transparent);
            #endregion
            subreturnboard.Duration = new Duration(TimeSpan.FromMilliseconds(1001 * 2));

            returnboard[0] = subreturnboard;
            returnboard[1] = subreturnboard2;
            returnboard[2] = subreturnboard3;

            if (teba != null)
            {
                if (per == 1)
                {
                    for (int i = 0; i < teba.GetLength(1); i++)
                    {
                        if (i > 1)
                        {
                            if (mat_back[von, i - 2].Equals(Brushes.LawnGreen))
                            {
                                teba[von, i].Background = brush_gy;
                                Storyboard.SetTarget(myColorAnimation_gy, teba[von, i]);
                                Storyboard.SetTarget(myColorAnimation_og, teba[von, i]);
                                Storyboard.SetTarget(myColorAnimation_gyo, teba[von, i]);

                                Storyboard.SetTargetProperty(myColorAnimation_og, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                                Storyboard.SetTargetProperty(myColorAnimation_gy, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                                Storyboard.SetTargetProperty(myColorAnimation_gyo, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));

                                subreturnboard3.Children.Add(myColorAnimation_og);
                                subreturnboard.Children.Add(myColorAnimation_gy);
                                subreturnboard.Children.Add(myColorAnimation_gyo);

                            }

                            if (mat_back[von, i - 2].Equals(Brushes.AliceBlue))
                            {
                                teba[von, i].Background = brush_by;
                                Storyboard.SetTarget(myColorAnimation_by, teba[von, i]);
                                Storyboard.SetTarget(myColorAnimation_ob, teba[von, i]);
                                Storyboard.SetTarget(myColorAnimation_yo, teba[von, i]);
                                Storyboard.SetTargetProperty(myColorAnimation_ob, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                                Storyboard.SetTargetProperty(myColorAnimation_by, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                                Storyboard.SetTargetProperty(myColorAnimation_yo, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                                subreturnboard.Children.Add(myColorAnimation_by);
                                subreturnboard3.Children.Add(myColorAnimation_ob);
                                subreturnboard.Children.Add(myColorAnimation_yo);
                            }

                            if (mat_back[nach, i - 2].Equals(Brushes.LawnGreen))
                            {
                                teba[nach, i].Background = brush_go;
                                Storyboard.SetTarget(myColorAnimation_go, teba[nach, i]);
                                Storyboard.SetTarget(myColorAnimation_yg, teba[nach, i]);
                                Storyboard.SetTarget(myColorAnimation_boy, teba[nach, i]);
                                Storyboard.SetTargetProperty(myColorAnimation_yg, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                                Storyboard.SetTargetProperty(myColorAnimation_go, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                                Storyboard.SetTargetProperty(myColorAnimation_boy, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                                subreturnboard.Children.Add(myColorAnimation_go);
                                subreturnboard3.Children.Add(myColorAnimation_yg);
                                subreturnboard.Children.Add(myColorAnimation_boy);

                            }

                            if (mat_back[nach, i - 2].Equals(Brushes.AliceBlue))
                            {
                                teba[nach, i].Background = brush_bo;

                                Storyboard.SetTarget(myColorAnimation_bo, teba[nach, i]);
                                Storyboard.SetTarget(myColorAnimation_yb, teba[nach, i]);
                                Storyboard.SetTarget(myColorAnimation_oy, teba[nach, i]);
                                Storyboard.SetTargetProperty(myColorAnimation_yb, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                                Storyboard.SetTargetProperty(myColorAnimation_bo, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                                Storyboard.SetTargetProperty(myColorAnimation_oy, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                                subreturnboard.Children.Add(myColorAnimation_bo);
                                subreturnboard3.Children.Add(myColorAnimation_yb);
                                subreturnboard.Children.Add(myColorAnimation_oy);

                            }
                        }

                        else
                        {
                            teba[von, i].Background = brush_ty;
                            teba[nach, i].Background = brush_to;

                            Storyboard.SetTarget(myColorAnimation_ty, teba[von, i]);
                            Storyboard.SetTarget(myColorAnimation_to, teba[nach, i]);
                            Storyboard.SetTarget(myColorAnimation_yt, teba[nach, i]);
                            Storyboard.SetTarget(myColorAnimation_ot, teba[von, i]);
                            Storyboard.SetTarget(myColorAnimation_toy, teba[nach, i]);
                            Storyboard.SetTarget(myColorAnimation_tyo, teba[von, i]);

                            Storyboard.SetTargetProperty(myColorAnimation_toy, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                            Storyboard.SetTargetProperty(myColorAnimation_tyo, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                            Storyboard.SetTargetProperty(myColorAnimation_ty, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                            Storyboard.SetTargetProperty(myColorAnimation_to, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                            Storyboard.SetTargetProperty(myColorAnimation_yt, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                            Storyboard.SetTargetProperty(myColorAnimation_ot, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));

                            subreturnboard.Children.Add(myColorAnimation_ty);
                            subreturnboard.Children.Add(myColorAnimation_to);
                            subreturnboard.Children.Add(myColorAnimation_tyo);
                            subreturnboard.Children.Add(myColorAnimation_toy);

                            subreturnboard3.Children.Add(myColorAnimation_yt);
                            subreturnboard3.Children.Add(myColorAnimation_ot);

                        }

                        DoubleAnimation myDoubleAnimation = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromMilliseconds(1000)))
                        {
                            BeginTime = TimeSpan.FromMilliseconds(1001)
                        };
                        Storyboard.SetTarget(myDoubleAnimation, teba[von, i]);
                        Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath("(Opacity)"));
                        subreturnboard.Children.Add(myDoubleAnimation);

                        DoubleAnimation myDoubleAnimation2 = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromMilliseconds(1000)))
                        {
                            BeginTime = TimeSpan.FromMilliseconds(1001)
                        };
                        Storyboard.SetTarget(myDoubleAnimation2, teba[nach, i]);
                        Storyboard.SetTargetProperty(myDoubleAnimation2, new PropertyPath("(Opacity)"));
                        subreturnboard.Children.Add(myDoubleAnimation2);

                        DoubleAnimation myFadein = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(1001)));
                        Storyboard.SetTarget(myFadein, teba[nach, i]);
                        Storyboard.SetTargetProperty(myFadein, new PropertyPath("(Opacity)"));
                        subreturnboard2.Children.Add(myFadein);

                        DoubleAnimation myFadein2 = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(1001)));
                        Storyboard.SetTarget(myFadein2, teba[von, i]);
                        Storyboard.SetTargetProperty(myFadein2, new PropertyPath("(Opacity)"));
                        subreturnboard2.Children.Add(myFadein2);

                    }
                }
                else
                {
                    for (int i = 0; i < teba.GetLength(0); i++)
                    {
                        if (i > 1)
                        {
                            if (mat_back[i - 2, von].Equals(Brushes.LawnGreen))
                            {
                                teba[i, von].Background = brush_gy;
                                Storyboard.SetTarget(myColorAnimation_gy, teba[i, von]);
                                Storyboard.SetTarget(myColorAnimation_og, teba[i, von]);
                                Storyboard.SetTarget(myColorAnimation_gyo, teba[i, von]);

                                Storyboard.SetTargetProperty(myColorAnimation_og, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                                Storyboard.SetTargetProperty(myColorAnimation_gy, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                                Storyboard.SetTargetProperty(myColorAnimation_gyo, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));

                                subreturnboard3.Children.Add(myColorAnimation_og);
                                subreturnboard.Children.Add(myColorAnimation_gy);
                                subreturnboard.Children.Add(myColorAnimation_gyo);

                            }

                            if (mat_back[i - 2, von].Equals(Brushes.AliceBlue))
                            {
                                teba[i, von].Background = brush_by;
                                Storyboard.SetTarget(myColorAnimation_by, teba[i, von]);
                                Storyboard.SetTarget(myColorAnimation_ob, teba[i, von]);
                                Storyboard.SetTarget(myColorAnimation_yo, teba[i, von]);
                                Storyboard.SetTargetProperty(myColorAnimation_ob, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                                Storyboard.SetTargetProperty(myColorAnimation_by, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                                Storyboard.SetTargetProperty(myColorAnimation_yo, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                                subreturnboard.Children.Add(myColorAnimation_by);
                                subreturnboard3.Children.Add(myColorAnimation_ob);
                                subreturnboard.Children.Add(myColorAnimation_yo);
                            }

                            if (mat_back[i - 2, nach].Equals(Brushes.LawnGreen))
                            {
                                teba[i, nach].Background = brush_go;
                                Storyboard.SetTarget(myColorAnimation_go, teba[i, nach]);
                                Storyboard.SetTarget(myColorAnimation_yg, teba[i, nach]);
                                Storyboard.SetTarget(myColorAnimation_boy, teba[i, nach]);
                                Storyboard.SetTargetProperty(myColorAnimation_yg, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                                Storyboard.SetTargetProperty(myColorAnimation_go, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                                Storyboard.SetTargetProperty(myColorAnimation_boy, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                                subreturnboard.Children.Add(myColorAnimation_go);
                                subreturnboard3.Children.Add(myColorAnimation_yg);
                                subreturnboard.Children.Add(myColorAnimation_boy);

                            }

                            if (mat_back[i - 2, nach].Equals(Brushes.AliceBlue))
                            {
                                teba[i, nach].Background = brush_bo;

                                Storyboard.SetTarget(myColorAnimation_bo, teba[i, nach]);
                                Storyboard.SetTarget(myColorAnimation_yb, teba[i, nach]);
                                Storyboard.SetTarget(myColorAnimation_oy, teba[i, nach]);
                                Storyboard.SetTargetProperty(myColorAnimation_yb, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                                Storyboard.SetTargetProperty(myColorAnimation_bo, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                                Storyboard.SetTargetProperty(myColorAnimation_oy, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                                subreturnboard.Children.Add(myColorAnimation_bo);
                                subreturnboard3.Children.Add(myColorAnimation_yb);
                                subreturnboard.Children.Add(myColorAnimation_oy);

                            }
                        }

                        else
                        {
                            teba[i, von].Background = brush_ty;
                            teba[i, nach].Background = brush_to;

                            Storyboard.SetTarget(myColorAnimation_ty, teba[i, von]);
                            Storyboard.SetTarget(myColorAnimation_to, teba[i, nach]);
                            Storyboard.SetTarget(myColorAnimation_yt, teba[i, nach]);
                            Storyboard.SetTarget(myColorAnimation_ot, teba[i, von]);
                            Storyboard.SetTarget(myColorAnimation_toy, teba[i, nach]);
                            Storyboard.SetTarget(myColorAnimation_tyo, teba[i, von]);

                            Storyboard.SetTargetProperty(myColorAnimation_toy, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                            Storyboard.SetTargetProperty(myColorAnimation_tyo, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                            Storyboard.SetTargetProperty(myColorAnimation_ty, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                            Storyboard.SetTargetProperty(myColorAnimation_to, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                            Storyboard.SetTargetProperty(myColorAnimation_yt, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));
                            Storyboard.SetTargetProperty(myColorAnimation_ot, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));

                            subreturnboard.Children.Add(myColorAnimation_ty);
                            subreturnboard.Children.Add(myColorAnimation_to);
                            subreturnboard.Children.Add(myColorAnimation_tyo);
                            subreturnboard.Children.Add(myColorAnimation_toy);

                            subreturnboard3.Children.Add(myColorAnimation_yt);
                            subreturnboard3.Children.Add(myColorAnimation_ot);

                        }

                        DoubleAnimation myDoubleAnimation = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromMilliseconds(1000)))
                        {
                            BeginTime = TimeSpan.FromMilliseconds(1001)
                        };
                        Storyboard.SetTarget(myDoubleAnimation, teba[i, von]);
                        Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath("(Opacity)"));
                        subreturnboard.Children.Add(myDoubleAnimation);

                        DoubleAnimation myDoubleAnimation2 = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromMilliseconds(1000)))
                        {
                            BeginTime = TimeSpan.FromMilliseconds(1001)
                        };
                        Storyboard.SetTarget(myDoubleAnimation2, teba[i, nach]);
                        Storyboard.SetTargetProperty(myDoubleAnimation2, new PropertyPath("(Opacity)"));
                        subreturnboard.Children.Add(myDoubleAnimation2);

                        DoubleAnimation myFadein = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(1001)));
                        Storyboard.SetTarget(myFadein, teba[i, nach]);
                        Storyboard.SetTargetProperty(myFadein, new PropertyPath("(Opacity)"));
                        subreturnboard2.Children.Add(myFadein);

                        DoubleAnimation myFadein2 = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(1001)));
                        Storyboard.SetTarget(myFadein2, teba[i, von]);
                        Storyboard.SetTargetProperty(myFadein2, new PropertyPath("(Opacity)"));
                        subreturnboard2.Children.Add(myFadein2);

                    }
                }
            }

            return returnboard;
        }

        private void my_Completed2(object sender, EventArgs e)
        {
            Clock whichClock = sender as Clock;
            int which = 0;

            for (int x = 0; x < aniClock.Count; x++)
            {
                if (sender == aniClock[x])
                {
                    which = x;
                }
            }
            if (which < changes.GetLength(0))
            {
                int nach = changes[which, 1];
                int von = changes[which, 0];

                if (per == 1)
                {
                    if (teba != null)
                    {
                        for (int i = 0; i < teba.GetLength(1); i++)
                        {
                            string help = teba[nach, i].Text.ToString();
                            teba[nach, i].Text = teba[von, i].Text.ToString();
                            teba[von, i].Text = help;

                            if (i > 1)
                            {
                                Brush help2;
                                help2 = mat_back[nach, i - 2];
                                mat_back[nach, i - 2] = mat_back[von, i - 2];
                                mat_back[von, i - 2] = help2;
                            }

                        }
                    }

                }
                else
                {
                    if (teba != null)
                    {
                        for (int i = 0; i < teba.GetLength(0); i++)
                        {
                            string help = teba[i, nach].Text.ToString();
                            teba[i, nach].Text = teba[i, von].Text.ToString();
                            teba[i, von].Text = help;

                            if (i > 1)
                            {
                                Brush help2;
                                help2 = mat_back[i - 2, nach];
                                mat_back[i - 2, nach] = mat_back[i - 2, von];
                                mat_back[i - 2, von] = help2;
                            }
                        }
                    }
                }
            }
        }


        #endregion

        #region misc

        /// <summary>
        /// getter of the speed the visualisation is running
        /// </summary>
        /// <param name="speed"></param>
        public void UpdateSpeed(int speed)
        {
            this.speed = speed;
            mainStory1.Pause();
            mainStory1.SetSpeedRatio(speed / 100);
            mainStory1.Resume();
            mainStory2.Pause();
            mainStory2.SetSpeedRatio(speed / 100);
            mainStory2.Resume();
            mainStory3.Pause();
            mainStory3.SetSpeedRatio(speed / 100);
            mainStory3.Resume();

            if (aniClock != null)
            {
                foreach (Clock cl in aniClock)
                {
                    cl.Controller.Pause();
                    cl.Controller.SpeedRatio = (speed / 100);
                    cl.Controller.Resume();
                }
            }

        }

        public void progressTimerTick(object sender, EventArgs e)
        {
            try
            {
                if (Convert.ToDouble(mainStory1.GetCurrentProgress().ToString()) != 1)
                {
                    myupdateprogress(Convert.ToInt32(Convert.ToDouble(mainStory1.GetCurrentProgress().ToString()) * 1000));
                }
                else if (Convert.ToDouble(mainStory2.GetCurrentProgress().ToString()) != 1)
                {
                    myupdateprogress(Convert.ToInt32(Convert.ToDouble(mainStory2.GetCurrentProgress().ToString()) * 1000) + 1000);
                }
                else if (Convert.ToDouble(mainStory3.GetCurrentProgress().ToString()) != 1)
                {
                    myupdateprogress(Convert.ToInt32(Convert.ToDouble(mainStory3.GetCurrentProgress().ToString()) * 1000) + 2000);
                }
            }
            catch (Exception)
            {

            }

        }

        private void myupdateprogress(int value)
        {
            progress = value;
            updateProgress(this, EventArgs.Empty);
        }
        #endregion

        public Transposition Transposition { get; set; }
    }
}
