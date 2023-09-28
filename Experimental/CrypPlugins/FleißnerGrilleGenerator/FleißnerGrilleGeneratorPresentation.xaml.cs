using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Media.Animation;
using System.ComponentModel;
using CrypTool.Plugins.FleißnerGrilleGenerator;
using System.Collections;

namespace CrypTool.Plugins.FleißnerGrilleGenerator
{
    /// <summary>
    /// Interaktionslogik für FleißnerGrilleGeneratorPresentation.xaml
    /// </summary>
    public partial class FleißnerGrilleGeneratorPresentation : UserControl
    {
        #region private variables
        private FleißnerGrilleGeneratorSettings settings;
        private Storyboard mainStory = new Storyboard();
        public AutoResetEvent ars;
        public DisabledBool PresentationDisabled;
        private DispatcherTimer timer;
        public event EventHandler fireEnd; //for granting Transposition to fire output after Presentation has finished
        public event EventHandler updateProgress; //updates the status of the plugin
        public int progress;                // progress variable to update the plugin status
        private List<Clock> aniClock = new List<Clock>();
        private int speed = 100;          // animation speed
        public bool Stop = false;      // boolean to stop animations from being executed
        private int clickCounter = 0;
        private Button[,] grille;
        #endregion

        public FleißnerGrilleGeneratorPresentation(FleißnerGrilleGenerator myGenerator)
        {
            ars = new AutoResetEvent(false);
            //storyboard.Completed += tasteClick2;

            PresentationDisabled = new DisabledBool();

            settings = (FleißnerGrilleGeneratorSettings)myGenerator.Settings;
            InitializeComponent();
            init(myGenerator);
            timer = new DispatcherTimer();
            timer.Tick += timerTick;
            timer.Interval = TimeSpan.FromMilliseconds(1);
            SizeChanged += sizeChanged;
        }

        #region private methods

        private int getGrilleSize(string input) 
        {
            int inputLength = input.Length;
            if (Math.Sqrt(inputLength) - (int)Math.Sqrt(inputLength) == 0)
            {
                if ((int)Math.Sqrt(inputLength) % 2 == 0) //Die Anzahl der gesamten Felder ist durch 4 teilbar
                {
                    return (int)Math.Sqrt(inputLength);
                }
                else
                {
                    return (int)Math.Sqrt(inputLength) + 1;
                }
            }
            else
            {
                if (((int)Math.Sqrt(inputLength) + 1) % 2 == 0) // Die Anzahl der gesamten Felder ist durch 4 teilbar
                {
                    return (int)Math.Sqrt(inputLength) + 1;
                }
                else
                {
                    return (int)Math.Sqrt(inputLength) + 2;
                }
            }
        }

        /// <summary>
        /// set the DescribeLabels in the presentation
        /// </summary>
        /// <param name="fleißnerGrilleGenerator"></param>
        private void setDescribeLabels(FleißnerGrilleGenerator fleißnerGrilleGenerator) 
        {
            int inputLength = fleißnerGrilleGenerator.InputString.Length;
            int grilleSize = getGrilleSize(fleißnerGrilleGenerator.InputString);
            describeLabel.Content = "The text has a length of " + inputLength + " letters.";
            describe2Label.Content = "producing a " + grilleSize + " x " + grilleSize + " pattern random";
        }

        private void init(FleißnerGrilleGenerator myGenerator)
        {
            clickCounter = 0;
            string input = myGenerator.InputString;
            //cleaning the display for new presentation
            try
            {
                grilleWrapPanel.Children.RemoveRange(0,grilleWrapPanel.Children.Count);
                mainmain.Children.Remove(grilleWrapPanel);
                mainmain.Children.Add(grilleWrapPanel);
                setDescribeLabels(myGenerator);
            }
            catch { }
            if (input != null) //input is not empty
            {
                fillGrille(input);
                if (settings.ActionRandom == 0) //randomly
                {
                    setHolesRandomToGrille();
                    string result = getGrilleString();
                }
                else //not randomly 
                {
                    int maxClick = 0;
                    if ((input.Length / 4 - (int)(input.Length / 4) == 0))
                    {
                        maxClick = input.Length / 4;
                    }
                    else 
                    {
                        maxClick = (int)(input.Length / 4) + 1;
                    }
                    //while (clickCounter < maxClick) 
                    //{ 
                        
                    //}
                    //grille = setHolesPerHandToGrille(grille);
                    string result = getGrilleString();
                }
            }
        }

        private void setHolesRandomToGrille()
        {
            int grilleLength = grille.GetLength(0);
            for (int i = 0; i < grilleLength*grilleLength; i=i+4)
            {
                Random randomH = new Random();
                int random;
                do
                {
                    random = randomH.Next(0, grille.GetLength(0) * grille.GetLength(1) - 1); //random from 0 ... inputLength-1
                } while (getPoint(random));

                int y = (int)(random / grilleLength);
                int x = random - (y * grilleLength);
                grille[x, y].Content = "1";
                grille[x, y].Background = Brushes.Red;
                for (int rotate = 0; rotate < 3; rotate++)
                {
                    RotateGrille();
                    grille[x, y].Content = "0";
                    grille[x, y].IsEnabled = false;
                }
            }         
        }

        /// <summary>
        // rotate a 2-dimensional Array
        public void RotateGrille()
        {
            int stencilLength = (int)Math.Sqrt(grille.Length);
            rotate();
        }

        /// <summary>
        // rotate a 2-dimensional Array
        public void rotate()
        {
            int stencilLength = (int)Math.Sqrt(grille.Length);
            Button[,] ret = new Button[stencilLength, stencilLength];
            for (int i = 0; i < stencilLength; ++i)
            {
                for (int j = 0; j < stencilLength; ++j)
                {
                    ret[i, j] = grille[stencilLength - j - 1, i];
                }
            }
            grille = ret;
        }

        private bool getPoint(int random)
        {
            int grilleLength = grille.GetLength(0);
            int y = (int) (random / grilleLength);
            int x = random - (y * grilleLength);
            //for (int i = 0; i < grilleWrapPanel.Children.Count; i++) 
            //{
            //    Button element = (Button) grilleWrapPanel.Children[i];
            //    element.B
            //}
            string content = (string)grille[x, y].Content;
            if (grille[x,y].IsEnabled && content.Equals("0")) //if(grilleWrapPanel is Blue)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// fills the GrilleWrapPanel with empty Objects
        /// </summary>
        /// <param name="input"></param>
        private void fillGrille(string input) 
        {
            int grilleSize = getGrilleSize(input);
            grille = new Button[grilleSize,grilleSize];
            double wrapWidth = this.grilleWrapPanel.ActualWidth;
            double objectWidth = wrapWidth / grilleSize;
            double wrapHeight = this.grilleWrapPanel.ActualHeight;
            double objectHeight = wrapWidth / grilleSize;
            for (int x = 0; x < grilleSize; x++)
            {
                for (int y = 0; y < grilleSize; y++) 
                {
                    Button button = new Button();
                    button.Width = objectWidth;
                    button.Height = objectHeight;
                    button.Background = Brushes.White;
                    button.Content = "0";
                    button.IsEnabled = true;
                    button.Click += new RoutedEventHandler(btn_Click);
                    grilleWrapPanel.Children.Add(button);
                    grille[x, y] = button;
                }
            }
        }



        /// <summary>
        /// result of the bool[,] grille as a string
        /// </summary>
        /// <param name="grille"></param>
        /// <returns></returns>
        private string getGrilleString() 
        {
            string result = "";
            for (int y = 0; y < grille.GetLength(0); y++) //columnwise
            {
                for (int x = 0; x < grille.GetLength(0); x++) //rowwise
                {
                    if (grille[x, y].Content == "1")
                    {
                        result = result + "1";
                    }
                    else 
                    {
                        result = result + "0";
                    }
                }
                if (y != grille.GetLength(0) - 1) 
                {
                    result = result + "\n";
                }
            }
            return result;
        }

        /// <summary>
        /// making the presentation scalable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void sizeChanged(Object sender, EventArgs eventArgs)
        {
            this.mainmain.RenderTransform = new ScaleTransform(this.ActualWidth / this.mainmain.ActualWidth,
                                                            this.ActualHeight / this.mainmain.ActualHeight);
        }


        internal void main(FleißnerGrilleGenerator myGenerator)
        {
            if (myGenerator.InputString != null)
            {
                init(myGenerator);
            }
        }

        #endregion

        #region misc

        public void timerTick(object sender, EventArgs e)
        {
            try
            {
                if (Convert.ToDouble(mainStory.GetCurrentProgress().ToString()) != 1)
                    myupdateprogress(Convert.ToInt32(Convert.ToDouble(mainStory.GetCurrentProgress().ToString()) * 1000));
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

        #region events

        internal void my_Stop(FleißnerGrilleGenerator fleißnerGrilleGenerator, EventArgs eventArgs)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setDescribeLabels(fleißnerGrilleGenerator); //set the DescribeLabels in the presentation
                grilleLabel.Content = "Grille:";
                grilleWrapPanel.Children.Clear();
                grilleWrapPanel.Children.RemoveRange(0, grilleWrapPanel.Children.Count);
                grille = null;
                progress = 0;
                //TODO: MainStory
                mainStory.Stop();
                mainStory.Children.Clear();
                foreach (Clock cl in aniClock)
                {
                    cl.Controller.Stop();
                }
                aniClock.Clear();
                Stop = true;
                fireEnd(this, EventArgs.Empty);
            }, null);
        }
        #endregion

        #endregion

        //public RoutedEventHandler btn_Click { get; set; }
        void btn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender; // this is the clicked Button
            if (btn.Content == "0")
            {
                btn.Content = "1";
                btn.Background = Brushes.Red;
                int x = 0;
                int y = 0;
                //get x, y
                int count = 0;
                foreach (Button actualBtn in grilleWrapPanel.Children) 
                {
                    if (Object.ReferenceEquals(actualBtn, btn))
                    {
                        y = count % grille.GetLength(0);
                        x = (count - y) / grille.GetLength(0);
                        break;
                    }
                    else
                    {
                        count++;
                    }
                }
                for (int rotate = 0; rotate < 3; rotate++)
                {
                    RotateGrille();
                    grille[x, y].Content = "0";
                    grille[x, y].IsEnabled = false;
                }
                RotateGrille();          
                clickCounter++;
                if (clickCounter == (grille.GetLength(0) * grille.GetLength(1)) / 4)
                {
                    settings.grille = grilleToGeneratorSettings(grille);
                }
            }
            else 
            {
                btn.Content = "0";
                btn.Background = Brushes.White;
                int x = 0;
                int y = 0;
                //get x, y
                int count = 0;
                foreach (Button actualBtn in grilleWrapPanel.Children)
                {
                    if (Object.ReferenceEquals(actualBtn, btn))
                    {
                        y = count % grille.GetLength(0);
                        x = (count - y) / grille.GetLength(0);
                        break;
                    }
                    else
                    {
                        count++;
                    }
                }
                for (int rotate = 0; rotate < 3; rotate++)
                {
                    RotateGrille();
                    grille[x, y].Content = "0";
                    grille[x, y].IsEnabled = true;
                }
                RotateGrille();
                clickCounter--;
            }
        }

        private int[,] grilleToGeneratorSettings(Button[,] grille)
        {
            int[,] grilleInt = new int[grille.GetLength(0), grille.GetLength(1)];
            for (int x = 0; x < grille.GetLength(0); x++) 
            {
                for (int y = 0; y < grille.GetLength(1); y++) 
                {
                    if (grille[x, y].Content.Equals("0"))
                    {
                        grilleInt[x, y] = 0;
                    }
                    else 
                    {
                        grilleInt[x, y] = 1;
                    }
                }
            }
            return grilleInt;
        }
    }

    public class DisabledBool : INotifyPropertyChanged
    {
        public bool disabledBoolProperty;

        public DisabledBool() { }

        public bool DisabledBoolProperty
        {
            get { return disabledBoolProperty; }
            set
            {
                disabledBoolProperty = value;
                OnPropertyChanged("DisabledBoolProperty");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
