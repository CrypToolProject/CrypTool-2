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
using System.Threading;
using System.Windows.Threading;
using System.Collections;
using System.Windows.Media.Animation;
using System.ComponentModel;

namespace CrypTool.Plugins.FleißnerGrille
{
    /// <summary>
    /// Interaktionslogik für FleißnerGrillePresentation.xaml
    /// </summary>
    public partial class FleißnerGrillePresentation : UserControl
    {
        public event EventHandler fireEnd; //for granting Transposition to fire output after Presentation has finished
        public event EventHandler updateProgress; //updates the status of the plugin
        /// <summary>
        /// Visualisationmodul for FleißnerStencil.c
        /// </summary>
        public FleißnerGrillePresentation(FleißnerGrille myFleißner)
        {
            ars = new AutoResetEvent(false);
            //storyboard.Completed += tasteClick2;

            PresentationDisabled = new DisabledBool();

            settings = (FleißnerGrilleSettings)myFleißner.Settings;
            speed = settings.PresentationSpeed;
            InitializeComponent();
            bool encrypt = fleißnerMode();
            init(settings.myStencil, myFleißner.InputString, encrypt, myFleißner);
            timer = new DispatcherTimer();
            timer.Tick += timerTick;
            timer.Interval = TimeSpan.FromMilliseconds(1);
            SizeChanged += sizeChanged;
        }

        private bool fleißnerMode() 
        {
            bool encrypt = true;
            if (settings.myFleißnerMode.Equals(1))
            {
                encrypt = false;
            }
            return encrypt;
        }

        private bool rotateMode()
        {
            bool rotate = true;
            if (settings.ActionRotate == 0) { rotate = false; }
            else { rotate = true; }
            return rotate;
        }

        #region private variables
        private List<int> order;
        private TextBlock[] reina;
        private TextBlock[,] reoutah;
        private TextBlock[] reouta;
        private TextBlock[,] reTablea;
        private Hashtable reTablea1;
        private FleißnerGrilleSettings settings;
        private Storyboard mainStory = new Storyboard();
        private Storyboard rotateStory = new Storyboard();
        private Storyboard moveStory = new Storyboard();
        private Storyboard translationStoryboard;
        private ColorAnimation markOrangeColorAnimation;
        private ColorAnimation markOrangeTableColorAnimation;
        private SolidColorBrush brushOrange;
        private SolidColorBrush brushTransparent;
        private DoubleAnimation daRemoveLetter;
        private DoubleAnimation daVisibleTable;
        private DoubleAnimation daVisibleLetter;
        private Brush[,] brushTextblock;          // backgrounds of the matrix saved seperatly
        public AutoResetEvent ars;
        public DisabledBool PresentationDisabled;
        private DispatcherTimer timer;
        public int progress;                // progress variable to update the plugin status
        private List<Clock> aniClock = new List<Clock>();
        private double speed = 1;          // animation speed
        int moveDuration = 5000;
        int rotateDuration = 3000;
        int colorDuration = 500;
        int unvisibleDurationInput = 500;
        int visibleDurationOutput = 500;
        bool[,] stencil1;
        bool[,] stencil2;
        bool[,] stencil3;
        public bool Stop = false;      // boolean to stop animations from being executed

        #endregion

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

        #region main

        public void main(bool[,] stencil, string input, bool encrypt, bool rotate, FleißnerGrille myFleißner) 
        {
            //outputWrap.Visibility = Visibility.Hidden;

            if (stencil != null && input != null)
            {
                init(stencil, input, encrypt, myFleißner);
            }
            stencil1 = myFleißner.RotateStencil(stencil, rotate);
            stencil2 = myFleißner.RotateStencil(myFleißner.RotateStencil(stencil, rotate), rotate);
            stencil3 = myFleißner.RotateStencil(myFleißner.RotateStencil(myFleißner.RotateStencil(stencil, rotate), rotate), rotate);
            startLoadedAnimation(stencil, input, encrypt, rotate, myFleißner);
        }

        private void startLoadedAnimation(bool[,] stencil, string input, bool encrypt, bool rotate, FleißnerGrille myFleißner)
        {
            // Create a NameScope for the page so that
            // we can use Storyboards.
            //tableOrange = new ColorAnimation[vigenere.InputString.Length, 2, this.settings.AlphabetSymbols.Length + 1];
            //outputWrap.Visibility = Visibility.Visible;
            NameScope.SetNameScope(this, new NameScope());
            int stencilEmptySize = howManyEmty(stencil);
            if (myFleißner.settings.ActionMode == FleißnerGrilleSettings.FleißnerMode.Encrypt) //encryption
            {
                moveStencilCanvas(0, "AnimatedTranslateTransform", input, myFleißner);
                rotateStencil(stencil, rotateMode(), myFleißner);
                for (int i = 0; i < input.Length; i++)
                {
                    TextBlock inputTxtBlock = reina[i];
                    string inputText = inputTxtBlock.Text;
                    //1
                    colorLetterInput(inputTxtBlock, i, stencilEmptySize, myFleißner);
                    //4
                    removeLetterInput(inputTxtBlock, i, stencilEmptySize, myFleißner);
                    colorLetterInField(stencil, i, stencilEmptySize, myFleißner);                  
                }
                //6
                for (int i = 0; i < outputWrap.Children.Count; i++)
                {
                    visibleOutput(i, stencilEmptySize, myFleißner);
                } 
            }
            else //decryption
            {
                for (int i = 0; i < input.Length; i++)
                {
                    TextBlock inputTxtBlock = reina[i];
                    string inputText = inputTxtBlock.Text;
                    //1
                    colorLetterInput(inputTxtBlock, i, stencilEmptySize, myFleißner);
                    //4
                    removeLetterInput(inputTxtBlock, i, stencilEmptySize, myFleißner);
                    colorLetterInField(stencil, i, stencilEmptySize, myFleißner);
                }
                moveStencilCanvas(2, "AnimatedTranslateTransform", input, myFleißner);
                rotateStencil(stencil, rotateMode(), myFleißner);
                for (int i = 0; i < input.Length; i++)
                {
                    colorLetterInField(stencil, i, stencilEmptySize, myFleißner);
                    visibleOutput(i, stencilEmptySize, myFleißner);
                }
            }
            
            mainStory.Begin(this);
            mainStory.SetSpeedRatio(speed);
        }

        private void moveStencilCanvas(int pos, string name, string input, FleißnerGrille myFleißner)
        {
            #region auskommentiert
            ////this.Margin = new Thickness(20);

            //// Create a MatrixTransform. This transform
            //// will be used to move the stencilCanvas.
            //MatrixTransform canvasMoveTransform = new MatrixTransform();
            //stencilCanvas.RenderTransform = canvasMoveTransform;

            //// Register the transform's name with the page
            //// so that it can be targeted by a Storyboard.
            //this.RegisterName(name+pos, canvasMoveTransform);

            //// Create the animation path.
            //PathGeometry animationPath = new PathGeometry();
            //PathFigure pFigure = new PathFigure();
            //pFigure.StartPoint = new Point(0, 0);
            //PolyBezierSegment pBezierSegment = new PolyBezierSegment();
            //pBezierSegment.Points.Add(new Point(0, 0));
            //pBezierSegment.Points.Add(new Point(200, 0));
            //pBezierSegment.Points.Add(fieldWrapPanel.TranslatePoint(new Point(fieldWrapPanel.Width / 2, fieldWrapPanel.Height / 2), mainmain));
            //pFigure.Segments.Add(pBezierSegment);
            //animationPath.Figures.Add(pFigure);

            //// Freeze the PathGeometry for performance benefits.
            //animationPath.Freeze();

            //// Create a MatrixAnimationUsingPath to move the
            //// button along the path by animating
            //// its MatrixTransform.
            //MatrixAnimationUsingPath matrixAnimation =
            //    new MatrixAnimationUsingPath();
            //matrixAnimation.PathGeometry = animationPath;
            //matrixAnimation.BeginTime = TimeSpan.FromMilliseconds(0);
            //matrixAnimation.Duration = TimeSpan.FromMilliseconds(moveDuration);
            ////matrixAnimation.RepeatBehavior = RepeatBehavior.Forever;

            //// Set the animation to target the Matrix property
            //// of the MatrixTransform named "ButtonMatrixTransform".
            //Storyboard.SetTargetName(matrixAnimation, name+pos);
            //Storyboard.SetTargetProperty(matrixAnimation,
            //    new PropertyPath(MatrixTransform.MatrixProperty));

            ////apply the animation to the Storyboard
            //// Create a Storyboard to contain and apply the animation.
            //Storyboard pathAnimationStoryboard = new Storyboard();
            //pathAnimationStoryboard.Children.Add(matrixAnimation);
            //pathAnimationStoryboard.Begin(this);

            ////DoubleAnimation daRotate = new DoubleAnimation();
            ////daRotate.From = 0;
            //////da.By = 360 / 26;
            ////daRotate.To = 360 / 4;
            //////myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(5));
            ////daRotate.Duration = new Duration(TimeSpan.FromMilliseconds(5000)); //dauer animation von From zu To
            //////da.RepeatBehavior = RepeatBehavior;
            ////int angle = 0;
            ////// Create some transforms. These transforms
            ////// will be used to move and rotate the rectangle.
            ////RotateTransform imgRotateTransform = new RotateTransform(angle, stencilCanvas.Width / 2, stencilCanvas.Height / 2);
            ////// Register the transforms' names with the page
            ////// so that they can be targeted by a Storyboard.
            ////this.RegisterName(name + pos + pos, imgRotateTransform);
            //////apply the RenderTransform to the image.
            ////stencilCanvas.RenderTransform = imgRotateTransform;
            ////// Create a Storyboard to contain and apply the animations.
            ////mainStory.Children.Add(daRotate);
            ////// Set the animation to target the Angle property
            ////// of the RotateTransform named "ImgRotateTransform".
            ////Storyboard.SetTargetName(daRotate, name + pos + pos);
            ////Storyboard.SetTargetProperty(daRotate, new PropertyPath(RotateTransform.AngleProperty));
            #endregion
            // Create a TranslateTransform to  
            // move the rectangle.
            TranslateTransform animatedTranslateTransform =
                new TranslateTransform();
            moveCanvas.RenderTransform = animatedTranslateTransform;
            // Assign the TranslateTransform a name so that 
            // it can be targeted by a Storyboard. 
            this.RegisterName(
                name, animatedTranslateTransform);
            // Create a DoubleAnimationUsingKeyFrames to 
            // animate the TranslateTransform.
            DoubleAnimationUsingKeyFrames moveXAnimation
                = new DoubleAnimationUsingKeyFrames();
            moveXAnimation.Duration = TimeSpan.FromMilliseconds(getAllDuration(input.Length, myFleißner)); //dauer Animation

            double leftWrap = Canvas.GetLeft(fieldWrapPanel);
            double topWrap = Canvas.GetTop(fieldWrapPanel);
            double left = Canvas.GetLeft(moveCanvas);
            double top = Canvas.GetTop(moveCanvas);
            double pause;
            if (myFleißner.settings.ActionMode == FleißnerGrilleSettings.FleißnerMode.Encrypt) //encryption
            {
                pause = moveDuration + 3 * rotateDuration + input.Length * (colorDuration + colorDuration + unvisibleDurationInput + colorDuration + unvisibleDurationInput);
                // Animate from the starting position to 500 
                // over the first second using linear 
                // interpolation.
                moveXAnimation.KeyFrames.Add(
                    new LinearDoubleKeyFrame(
                        leftWrap - left,//900, // Target value (KeyValue)
                        KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(moveDuration))) // KeyTime  //bis dauer
                    );
                moveXAnimation.KeyFrames.Add(
                    new LinearDoubleKeyFrame(
                        leftWrap - left,//900, // Target value (KeyValue)
                        KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(pause))) // KeyTime  //bis dauer
                    );
                moveXAnimation.KeyFrames.Add(
                    new LinearDoubleKeyFrame(
                        0, // Target value (KeyValue)
                        KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(pause + moveDuration))) // KeyTime  //bis dauer
                    );
            }
            else 
            {
                pause = input.Length * 2 * colorDuration + moveDuration + 3 * rotateDuration + input.Length * colorDuration;
                // Animate from the starting position to 500 
                // over the first second using linear 
                // interpolation.
                moveXAnimation.KeyFrames.Add(
                    new LinearDoubleKeyFrame(
                        0,//900, // Target value (KeyValue)
                        KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(input.Length * 2 * colorDuration))) // KeyTime  //bis dauer
                    );
                moveXAnimation.KeyFrames.Add(
                    new LinearDoubleKeyFrame(
                        leftWrap - left,//900, // Target value (KeyValue)
                        KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(input.Length * 2 * colorDuration + moveDuration))) // KeyTime  //bis dauer
                    );
                moveXAnimation.KeyFrames.Add(
                    new LinearDoubleKeyFrame(
                        leftWrap - left,//900, // Target value (KeyValue)
                        KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(pause))) // KeyTime  //bis dauer
                    );
                moveXAnimation.KeyFrames.Add(
                    new LinearDoubleKeyFrame(
                        0, // Target value (KeyValue)
                        KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(pause + moveDuration))) // KeyTime  //bis dauer
                    );
            }
            // Set the animation to target the X property 
            // of the object named "AnimatedTranslateTransform."
            Storyboard.SetTargetName(moveXAnimation, name);
            Storyboard.SetTargetProperty(
                moveXAnimation, new PropertyPath(TranslateTransform.XProperty));


            // Create a DoubleAnimationUsingKeyFrames to 
            // animate the TranslateTransform.
            DoubleAnimationUsingKeyFrames moveYAnimation
                = new DoubleAnimationUsingKeyFrames();
            moveYAnimation.Duration = TimeSpan.FromMilliseconds(getAllDuration(input.Length, myFleißner)); //dauer Animation


            if (myFleißner.settings.ActionMode == FleißnerGrilleSettings.FleißnerMode.Encrypt) //encryption
            {
                // Animate from the starting position to 500 
                // over the first second using linear 
                // interpolation.
                moveYAnimation.KeyFrames.Add(
                    new LinearDoubleKeyFrame(
                        topWrap - top - (top / Math.Sqrt(input.Length)),//-320, // Target value (KeyValue)
                        KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(moveDuration))) // KeyTime  //bis dauer
                    );
                moveYAnimation.KeyFrames.Add(
                    new LinearDoubleKeyFrame(
                        topWrap - top - (top / Math.Sqrt(input.Length)),//-320, // Target value (KeyValue)
                        KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(pause))) // KeyTime  //bis dauer
                    );
                moveYAnimation.KeyFrames.Add(
                    new LinearDoubleKeyFrame(
                        0, // Target value (KeyValue)
                        KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(pause + moveDuration))) // KeyTime  //bis dauer
                    );
            }
            else 
            {
                // Animate from the starting position to 500 
                // over the first second using linear 
                // interpolation.
                moveYAnimation.KeyFrames.Add(
                   new LinearDoubleKeyFrame(
                       0,//900, // Target value (KeyValue)
                       KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(input.Length * 2 * colorDuration))) // KeyTime  //bis dauer
                   );
                moveYAnimation.KeyFrames.Add(
                    new LinearDoubleKeyFrame(
                        topWrap - top - (top / Math.Sqrt(input.Length)),//-320, // Target value (KeyValue)
                        KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(input.Length * 2 * colorDuration + moveDuration))) // KeyTime  //bis dauer
                    );
                moveYAnimation.KeyFrames.Add(
                    new LinearDoubleKeyFrame(
                        topWrap - top - (top / Math.Sqrt(input.Length)),//-320, // Target value (KeyValue)
                        KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(pause))) // KeyTime  //bis dauer
                    );
                moveYAnimation.KeyFrames.Add(
                    new LinearDoubleKeyFrame(
                        0, // Target value (KeyValue)
                        KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(pause + moveDuration))) // KeyTime  //bis dauer
                    );
            }
            // Set the animation to target the X property 
            // of the object named "AnimatedTranslateTransform."
            Storyboard.SetTargetName(moveYAnimation, name);
            Storyboard.SetTargetProperty(
                moveYAnimation, new PropertyPath(TranslateTransform.YProperty));


            // Create a storyboard to apply the animation.
            //Storyboard moveStoryboard = new Storyboard();
            moveStory.Children.Add(moveXAnimation);
            moveStory.Children.Add(moveYAnimation);
            mainStory.Children.Add(moveStory);
            //mainstory1.Children.Add(moveXAnimation);
            //mainstory1.Children.Add(moveYAnimation);
            //mainStory.Children.Add(mainstory1);
        }

        public void rotateHelp(DoubleAnimationUsingKeyFrames translationAnimation, double value, double starttime)
        {
            translationAnimation.KeyFrames.Add(
               new LinearDoubleKeyFrame(
                   value, // Target value (KeyValue)
                   KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(starttime))) // KeyTime //ab der Zeit wird es ausgeführt
               );
        }

        private void rotateStencil(bool[,] stencil, bool rotate, FleißnerGrille myFleißner) 
        {
            rotateStencilCanvas("RotateStencilCanvas", stencil, rotate, myFleißner);
        }

        private void rotateStencilCanvas(string name, bool[,] stencil, bool rotate, FleißnerGrille myFleißner)
        {
            // Create a TranslateTransform to  
            // rotate the Canvas.
            int angle = 0;
            RotateTransform imgRotateTransform = new RotateTransform(angle, stencilCanvas.Width / 2, stencilCanvas.Height / 2);
            stencilCanvas.RenderTransform = imgRotateTransform;
            // Assign the TranslateTransform a name so that 
            // it can be targeted by a Storyboard. 
            this.RegisterName(
                name, imgRotateTransform);
            // Create a DoubleAnimationUsingKeyFrames to 
            // animate the TranslateTransform.
            DoubleAnimationUsingKeyFrames translationRotateAnimation
                = new DoubleAnimationUsingKeyFrames();
            int count = stencil.GetLength(0) * stencil.GetLength(0);
            double allDuration = getAllDuration(count, myFleißner);
            translationRotateAnimation.Duration = TimeSpan.FromMilliseconds(allDuration);  //dauer
            if (myFleißner.settings.ActionMode == FleißnerGrilleSettings.FleißnerMode.Encrypt) //encryption
            {
                translationRotateAnimation.BeginTime = TimeSpan.FromMilliseconds(moveDuration + 1);
            }
            else 
            {
                translationRotateAnimation.BeginTime = TimeSpan.FromMilliseconds(1);
            }
            double[] startPoints = getStartPoints(stencil, myFleißner);

            if (rotateMode())  //right
            {
                rotateHelp(translationRotateAnimation, 0, startPoints[0]);   //breakTime
                rotateHelp(translationRotateAnimation, 90, startPoints[0] + rotateDuration);
                rotateHelp(translationRotateAnimation, 90, startPoints[1]);  //breakTime
                rotateHelp(translationRotateAnimation, 180, startPoints[1] + rotateDuration);
                rotateHelp(translationRotateAnimation, 180, startPoints[2]); //breakTime
                rotateHelp(translationRotateAnimation, 270, startPoints[2] + rotateDuration);
            }
            else 
            {
                rotateHelp(translationRotateAnimation, 0, startPoints[0]);   //breakTime
                rotateHelp(translationRotateAnimation, -90, startPoints[0] + rotateDuration);
                rotateHelp(translationRotateAnimation, -90, startPoints[1]);  //breakTime
                rotateHelp(translationRotateAnimation, -180, startPoints[1] + rotateDuration);
                rotateHelp(translationRotateAnimation, -180, startPoints[2]); //breakTime
                rotateHelp(translationRotateAnimation, -270, startPoints[2] + rotateDuration);
            }
            Storyboard.SetTargetName(translationRotateAnimation, name);
            Storyboard.SetTargetProperty(
                translationRotateAnimation, new PropertyPath(RotateTransform.AngleProperty));
            rotateStory.Children.Add(translationRotateAnimation);
            mainStory.Children.Add(rotateStory);

        }

        private double[] getStartPoints(bool[,] stencil, FleißnerGrille myFleißner)
        {
            int stencilEmptySize = howManyEmty(stencil);
            int round = stencilEmptySize;
            double[] startPoints = new double[3];
            for (int i = 1; i < 4; i++)
            {
                if (myFleißner.settings.ActionMode == FleißnerGrilleSettings.FleißnerMode.Encrypt) //encryption
                {
                    startPoints[i - 1] = certainDuration(0, i * stencilEmptySize, stencilEmptySize);
                }
                else 
                {
                    startPoints[i - 1] = certainDurationD(3, i * stencilEmptySize, stencilEmptySize);
                }
            }
            return startPoints;
        }

        private double getAllDuration(int count, FleißnerGrille myFleißner) 
        {
            if (myFleißner.settings.ActionMode == FleißnerGrilleSettings.FleißnerMode.Encrypt) //encryption
            {
                return (moveDuration + 3 * rotateDuration + count * (colorDuration + colorDuration + unvisibleDurationInput + colorDuration + unvisibleDurationInput) + moveDuration);
            }
            else 
            {
                return (count * (colorDuration + colorDuration) + moveDuration + 3 * rotateDuration + count * (colorDuration) + moveDuration);
            }
        }

        private int howManyEmty(bool[,] stencil)
        {
            int size=0;
            for (int i = 0; i < stencil.GetLength(0); i++)
            {
                for (int j = 0; j < stencil.GetLength(1); j++)
                {
                    if (stencil[i, j] == true) 
                    {
                        size++;
                    }
                }
            }
            return size;
        }

        #endregion

        #region private methods

        private void init(bool[,] stencil, string input, bool encrypt, FleißnerGrille myFleißner) 
        {
            //cleaning the display for new presentation
            try
            {
                mainmain.Children.Remove(fieldWrapPanel);
                mainmain.Children.Remove(moveCanvas);
                mainmain.Children.Remove(inputWrap);
                mainmain.Children.Remove(outputWrap);
                mainmain.Children.Add(fieldWrapPanel);
                mainmain.Children.Add(inputWrap);
                mainmain.Children.Add(outputWrap);
                mainmain.Children.Add(moveCanvas);
                moveCanvas.Children.Add(stencilCanvas);

            }
            catch { }
            canvasControlPanel.Visibility = Visibility.Visible;
            buttonPlay.IsEnabled = true;
            buttonBreak.IsEnabled = true;
            buttonFillPeriod.IsEnabled = true;
            buttonResume.IsEnabled = true;
            buttonSpeed.IsEnabled = true;
            buttonStop.IsEnabled = true;
            if (input != null)
            {
                reina = new TextBlock[input.Length];
                reoutah = new TextBlock[stencil.GetLength(0), stencil.GetLength(1)];
                //rekeya = new TextBlock[input.Length];
            }
            //this.stencilCanvas.Background = new SolidColorBrush(Colors.White);
            if (input != null && stencil != null)
            {
                fillWrap(input);
                fillFieldTable(input, stencil, myFleißner);
                int size = stencil.GetLength(0);
                string sizehelp = (string)this.stencilSizeLabel.Content;
                this.stencilSizeLabel.Content = sizehelp + size + " x " + size;
                drawStencil(stencil, this.stencilCanvas);
                generateOrder(myFleißner);
                string ilabel = (string)this.inputLabel.Content;
                if (myFleißner.settings.ActionMode == FleißnerGrilleSettings.FleißnerMode.Encrypt)
                {
                    inputLabel.Content = ilabel + " Plaintext";
                }
                else
                {
                    inputLabel.Content = ilabel + " Ciphertext";
                }
            }
        }

        private void initHelper()
        {
            try
            {
                mainmain.Children.Remove(fieldWrapPanel);
                mainmain.Children.Remove(moveCanvas);
                mainmain.Children.Remove(inputWrap);
                mainmain.Children.Remove(outputWrap);
                mainmain.Children.Add(fieldWrapPanel);
                mainmain.Children.Add(inputWrap);
                mainmain.Children.Add(outputWrap);
                mainmain.Children.Add(moveCanvas);
                moveCanvas.Children.Add(stencilCanvas);

            }
            catch { }
            canvasControlPanel.Visibility = Visibility.Visible;
            buttonPlay.IsEnabled = true;
            buttonBreak.IsEnabled = true;
            buttonFillPeriod.IsEnabled = true;
            buttonResume.IsEnabled = true;
            buttonSpeed.IsEnabled = true;
            buttonStop.IsEnabled = true;
        }

        //füllt wrap with text
        private void fillWrap(string input) 
        {
            for (int i = 0; i < input.Length; i++)
            {
                TextBlock txt = new TextBlock();
                String s = input[i].ToString();
                txt.Text = s;
                txt.FontSize = 30;
                txt.FontWeight = FontWeights.ExtraBold;
                txt.TextAlignment = TextAlignment.Center;
                reina[i] = txt;
                reina[i].Background = Brushes.Transparent;
                inputWrap.Children.Add(reina[i]);
            }
        }

        private void fillFieldTable(string input, bool[,] stencil, FleißnerGrille myFleißner)
        {
            reTablea1 = new Hashtable();
            char[,] encrypted = myFleißner.EncryptedMatrix(stencil, input);
            string decrypted = myFleißner.Decrypt(input);
            int inputSize = input.Length; //length of the alphabet
            int stencilLength = (int)Math.Sqrt(stencil.Length);
            reTablea = new TextBlock[stencil.GetLength(0), stencil.GetLength(1)];
            double tableWidth = fieldWrapPanel.ActualWidth;
            double tableHeight = fieldWrapPanel.ActualHeight;
            double textBoxWidth = tableWidth / (stencil.GetLength(0));
            double textBoxHeight = tableHeight / (stencil.GetLength(1));
            int count = 0;
            for (int i = 0; i < stencil.GetLength(0) ; i++) //gehe Zeilen durch
            {
                for (int j = 0; j < stencil.GetLength(1); j++) //fülle j-te Zeile
                {
                    TextBlock t;
                    TextBlock output;
                    if (myFleißner.settings.ActionMode == FleißnerGrilleSettings.FleißnerMode.Encrypt)
                    {
                        t = newTableTextBlock(encrypted[i, j].ToString(), textBoxHeight, textBoxWidth);
                        output = newTextBlock(encrypted[i, j].ToString());
                    }
                    else 
                    {
                        t = newTableTextBlock(input[count].ToString(), textBoxHeight, textBoxWidth);
                        output = newTextBlock(decrypted[count].ToString());
                    }
                    reTablea1[count] = t; //fill hashTable
                    reTablea[i, j] = t;
                    fieldWrapPanel.Children.Add(t);
                    
                    reoutah[i, j] = output;
                    outputWrap.Children.Add(output); //zeilenweise
                    count++;
                }
            }
            fillReOutARowWise();
        }

        private void generateOrder(FleißnerGrille myFleißner)
        {
            order = new List<int>();
            int count = 0;
            int stencilLength = (int)Math.Sqrt(settings.StencilString.Length);
            bool[,] stencil = settings.StringToStencil(settings.StencilString);
            for (int rotate = 0; rotate <= 3; rotate++)
            {
                for (int i = 0; i < stencilLength; i++)
                {
                    for (int j = 0; j < stencilLength; j++)
                    {
                        if (stencil[i, j] == true) 
                        {
                            order.Add(count);
                        }
                        count++;
                    }
                }
                count = 0;
                stencil = myFleißner.RotateStencil(stencil,rotateMode());
            }
        }

        private void fillReOutARowWise() 
        {
            reouta = new TextBlock[outputWrap.Children.Count];
            int k = 0;
            for(int i=0; i<reoutah.GetLength(0); i++)
            {
                for (int j = 0; j < reoutah.GetLength(1); j++)
                {
                    reouta[k] = reoutah[i, j];
                    k++;
                }
            }
        }

        private TextBlock newTableTextBlock(string text, double height, double width)
        {
            TextBlock txt = new TextBlock();
            txt.Text = text;
            txt.Height = height;
            txt.Width = width;
            //TODO: berechne Fontsize
            txt.FontSize = 30;
            txt.FontWeight = FontWeights.ExtraBold;
            txt.TextAlignment = TextAlignment.Center;
            txt.Opacity = 0.0;
            return txt;
        }

        //füllt wrap with text
        private TextBlock newTextBlock(string text)
        {
                TextBlock txt = new TextBlock();
                txt.Text = text;
                txt.FontSize = 30;
                txt.FontWeight = FontWeights.ExtraBold;
                txt.TextAlignment = TextAlignment.Center;
                txt.Opacity = 0.0;
                return txt;
        }

        //zeichnet Stencil in stencilCanvas
        private void drawStencil(bool[,] stencil, Canvas canvas)
        {
            double cWidth = canvas.ActualWidth;
            double cHeight = canvas.ActualHeight;
            int size = stencil.GetLength(0);
            //size from a rectangle
            
            double width = cWidth / size;  
            double height = cHeight / size;
            for (int i = 0; i < stencil.GetLength(0); i++)
            {
                for (int j = 0; j < stencil.GetLength(1); j++)
                {
                    if (stencil[i, j] == true) // stencil has on position i,j a hole
                    {
                        // print white rectangle
                        Rectangle r = new Rectangle();
                        r.Height = height;
                        r.Width = width;
                        //r.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                        r.Opacity = 0.0;
                        //r.Visibility = Visibility.Hidden;
                        Canvas.SetLeft(r, width * j);
                        Canvas.SetTop(r, height * i); 
                        canvas.Children.Add(r);
                    }
                    else 
                    {
                        
                        Rectangle r = new Rectangle(); //print black rectangle
                        r.Height = height;
                        r.Width = width;
                        r.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                        Canvas.SetLeft(r, width * j);
                        Canvas.SetTop(r, height * i); 
                        canvas.Children.Add(r);
                    }
                }              
            }
        }

        private System.Drawing.Bitmap drawStencilImage() 
        {
            System.Drawing.Bitmap flag = new System.Drawing.Bitmap(10, 10);
            for (int x = 0; x < flag.Height; ++x)
                for (int y = 0; y < flag.Width; ++y)
                    flag.SetPixel(x, y, System.Drawing.Color.White);
            for (int x = 0; x < flag.Height; ++x)
                flag.SetPixel(x, x, System.Drawing.Color.Red);
            return flag;
        }

        //zeichnet linien in field
        private void drawLine(Canvas canvas, int size) 
        {
            double cWidth = canvas.ActualWidth;
            double cHeight = canvas.ActualHeight;
            //size from a rectangle

            double width = cWidth / size;
            double height = cHeight / size;

            for (int i = 0; i < size; i++)
            {
                Line line = new Line();
                line.Width = cWidth;
                Canvas.SetTop(line, height * i);
                canvas.Children.Add(line);
            }
            for (int j = 0; j < size; j++)
            {
                Line line = new Line();
                line.Height = cHeight;
                line.VerticalAlignment = VerticalAlignment;
                Canvas.SetLeft(line, width * j);
                canvas.Children.Add(line);
            }
        }

        #endregion

        #region animation

        #region loadAnimations
        //1
        private void colorLetterInput(TextBlock txtBlock, int i, int stencilEmptySize, FleißnerGrille myFleißner)
        {
            double duration;
            if (myFleißner.settings.ActionMode == FleißnerGrilleSettings.FleißnerMode.Encrypt) //encryption
            {
                duration = certainDuration(1, i, stencilEmptySize);
                colorTextBlock(txtBlock, i, "myAnimatedBrushLetter", duration);
            }
            else 
            {
                duration = certainDurationD(0, i, stencilEmptySize);
                colorTextBlock(txtBlock, i, "myAnimatedBrushLetter", duration);
            }
        }

        //2
        //3

        //private bool[,] rotateStencil(bool[,] stencil, int i, FleißnerStencil myFleißner, int stencilEmptySize)
        //{
        //    double duration = certainDuration(4, i, stencilEmptySize);
        //    return myFleißner.RotateStencil(stencil);
        //}
        //4
        private void removeLetterInput(TextBlock txtBlock, int i, int stencilEmptySize, FleißnerGrille myFleißner)
        {
            double duration;
            if (myFleißner.settings.ActionMode == FleißnerGrilleSettings.FleißnerMode.Encrypt) //encryption
            {
                duration = certainDuration(4, i, stencilEmptySize);
                removeTextBlock(txtBlock, i, "RemoveInputTextblock", duration);
            }
            else 
            {
                duration = certainDurationD(1, i, stencilEmptySize);
                removeTextBlock(txtBlock, i, "RemoveInputTextblock", duration);
            }
        }

        private void colorLetterInField(bool[,] stencil, int i, int stencilEmptySize, FleißnerGrille myFleißner)
        {
            double duration;
            if (myFleißner.settings.ActionMode == FleißnerGrilleSettings.FleißnerMode.Encrypt) //encryption
            {
                duration = certainDuration(4, i, stencilEmptySize);
                findetTextBlock1(i, duration, myFleißner);
            }
            else 
            {
                duration = certainDurationD(1, i, stencilEmptySize);
                int x = (int) (i/stencil.GetLength(0));
                int y = i%stencil.GetLength(0);
                visibleTableTextBlock(reTablea[x, y], x, y, "myAnimatedTable", duration, i);
            }
        }

        private void findetTextBlock(bool[,] stencil, int stelle, double duration)
        {
            int k = -1;
            for (int x = 0; x < stencil.GetLength(0); x++)
            {
                for (int y = 0; y < stencil.GetLength(1); y++)
                {
                    if (stencil[x, y] == true)
                    {
                        k++;
                        if (k == stelle)
                        {
                            //return reTablea[x, y];
                            visibleTableTextBlock(reTablea[x, y], x, y, "myAnimatedTable", duration, stelle);
                        }
                    }
                }
            }
            //return null;
        }
        private void findetTextBlock1(int stelle, double duration, FleißnerGrille myFleißner)
        {
            visibleTableTextBlock1((TextBlock) reTablea1[order[stelle]], "myAnimatedTable", duration, stelle);
        }
        //5
        private void colorLetterInField2(bool[,] stencil, int i, int stencilEmptySize, FleißnerGrille myFleißner)
        {
            double duration;
            if (myFleißner.settings.ActionMode == FleißnerGrilleSettings.FleißnerMode.Encrypt) //encryption
            {
                duration = certainDuration(4, i, stencilEmptySize);
                findetTextBlock1(i, duration, myFleißner);
            }
            else
            {
                duration = certainDurationD(4, i, stencilEmptySize);
                colorTableTextBlock((TextBlock)reTablea1[order[i]], "myAnimatedBrushTable", duration, i);
            }
        }

        //6
        private void visibleOutput(int round, int stencilEmptySize, FleißnerGrille myFleißner)
        {
            double duration;
            if (myFleißner.settings.ActionMode == FleißnerGrilleSettings.FleißnerMode.Encrypt) //encryption
            {
                duration = certainDuration(6, round, stencilEmptySize);
                visibleOutputTextBlock(round, "myAnimatedOutput", duration);
            }
            else 
            {
                duration = certainDurationD(5, round, stencilEmptySize);
                visibleOutputTextBlock(round, "myAnimatedOutput", duration);
            }
        }  

        private void colorTextBlock(TextBlock txtBlock, int pos, string name, double duration)
        {
            markOrangeColorAnimation = new ColorAnimation(Colors.Transparent, Colors.Orange, new Duration(TimeSpan.FromMilliseconds(colorDuration)));
            markOrangeColorAnimation.BeginTime = TimeSpan.FromMilliseconds(duration);
            brushOrange = new SolidColorBrush();
            brushOrange.Color = Colors.Transparent;
            this.RegisterName(name + pos, brushOrange);
            txtBlock.Background = brushOrange;
            mainStory.Children.Add(markOrangeColorAnimation);
            Storyboard.SetTargetName(markOrangeColorAnimation, name + pos);
            Storyboard.SetTargetProperty(markOrangeColorAnimation, new PropertyPath(SolidColorBrush.ColorProperty));
        }

        private void removeTextBlock(TextBlock txtBlock, int pos, string name, double duration)
        {
            daRemoveLetter = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromMilliseconds(unvisibleDurationInput)));
            daRemoveLetter.BeginTime = TimeSpan.FromMilliseconds(duration);
            mainStory.Children.Add(daRemoveLetter);
            txtBlock.Name = name + pos;
            RegisterControl<TextBlock>(txtBlock.Name, txtBlock);
            Storyboard.SetTargetName(daRemoveLetter, txtBlock.Name);
            Storyboard.SetTargetProperty(daRemoveLetter, new PropertyPath("(Opacity)"));
        }


        private void colorTableTextBlock(TextBlock textBlock, string name, double duration, int stelle)
        {
            markOrangeTableColorAnimation = new ColorAnimation(Colors.Transparent, Colors.Orange, new Duration(TimeSpan.FromMilliseconds(colorDuration)));
            markOrangeTableColorAnimation.BeginTime = TimeSpan.FromMilliseconds(duration);
            brushOrange = new SolidColorBrush();
            brushOrange.Color = Colors.Transparent;
            this.RegisterName(name + stelle, brushOrange);
            textBlock.Background = brushOrange;
            mainStory.Children.Add(markOrangeColorAnimation);
            Storyboard.SetTargetName(markOrangeTableColorAnimation, name + stelle);
            Storyboard.SetTargetProperty(markOrangeTableColorAnimation, new PropertyPath(SolidColorBrush.ColorProperty));
        }

        private void visibleTableTextBlock(TextBlock txtBlock, int x, int y, string name, double duration, int stelle)
        {
            daVisibleTable = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(visibleDurationOutput)));
            daVisibleTable.BeginTime = TimeSpan.FromMilliseconds(duration);
            mainStory.Children.Add(daVisibleTable);
            //Storyboard.SetTarget(daVisibleLetter, reouta[x,y]);
            txtBlock.Name = name + x + y;
            RegisterControl<TextBlock>(txtBlock.Name, txtBlock);
            Storyboard.SetTargetName(daVisibleTable, txtBlock.Name);
            Storyboard.SetTargetProperty(daVisibleTable, new PropertyPath("(Opacity)"));
        }
        private void visibleTableTextBlock1(TextBlock txtBlock, string name, double duration, int stelle)
        {
            daVisibleTable = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(visibleDurationOutput)));
            daVisibleTable.BeginTime = TimeSpan.FromMilliseconds(duration);
            mainStory.Children.Add(daVisibleTable);
            //Storyboard.SetTarget(daVisibleLetter, reouta[x,y]);
            txtBlock.Name = name + stelle;
            RegisterControl<TextBlock>(txtBlock.Name, txtBlock);
            Storyboard.SetTargetName(daVisibleTable, txtBlock.Name);
            Storyboard.SetTargetProperty(daVisibleTable, new PropertyPath("(Opacity)"));
        }

        private void visibleTextBlock(TextBlock txtBlock, int x, int y, string name, double duration)
        {
            daVisibleLetter = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(visibleDurationOutput)));
            daVisibleLetter.BeginTime = TimeSpan.FromMilliseconds(duration);
            mainStory.Children.Add(daVisibleLetter);
            //Storyboard.SetTarget(daVisibleLetter, reouta[x,y]);
            txtBlock.Name = name + x + y;
            RegisterControl<TextBlock>(txtBlock.Name, txtBlock);
            Storyboard.SetTargetName(daVisibleLetter, txtBlock.Name);
            Storyboard.SetTargetProperty(daVisibleLetter, new PropertyPath("(Opacity)"));
        }

        private void visibleOutputTextBlock(int round, string name, double duration)
        {
            daVisibleLetter = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(visibleDurationOutput)));
            daVisibleLetter.BeginTime = TimeSpan.FromMilliseconds(duration);
            mainStory.Children.Add(daVisibleLetter);
            //Storyboard.SetTarget(daVisibleLetter, reouta[x,y]);
            reouta[round].Name = name + round;
            RegisterControl<TextBlock>(reouta[round].Name, reouta[round]);
            Storyboard.SetTargetName(daVisibleLetter, reouta[round].Name);
            Storyboard.SetTargetProperty(daVisibleLetter, new PropertyPath("(Opacity)"));
        }

        #region helpLoadAnimations

        void RegisterControl<T>(string textBoxName, T textBox)
        {
            if ((T)this.FindName(textBoxName) != null)
                this.UnregisterName(textBoxName);
            this.RegisterName(textBoxName, textBox);
        }

        //wählt dauer der einzelnen Animationen aus
        private double certainDuration(int i, int round, int stencilEmptySize)
        {
            double rotate = round / stencilEmptySize;
            rotate = Math.Floor(rotate);
            switch (i)
            {
                //rotate stencil
                case 0:
                    if (rotate == 0) 
                    {
                        rotate--;
                    }
                    return (/*moveDuration +*/ (rotate-1) * rotateDuration + round * colorDuration + round * colorDuration + round * unvisibleDurationInput + round * colorDuration + round * unvisibleDurationInput);
                //mark Letter input and Key
                case 1:
                    return (moveDuration + rotate*rotateDuration + round * colorDuration + round * colorDuration + round * unvisibleDurationInput + round * colorDuration + round * unvisibleDurationInput);
                //mark Letter in Table
                case 2:
                    return (moveDuration + rotate * rotateDuration + (round + 1) * colorDuration + round * colorDuration + round * unvisibleDurationInput + round * colorDuration + round * unvisibleDurationInput);
                //remove Letter input and Key, mark column and row 
                case 3:
                    return (moveDuration + rotate * rotateDuration + (round + 1) * colorDuration + (round + 1) * colorDuration + round * unvisibleDurationInput + round * colorDuration + round * unvisibleDurationInput);
                //set output visible
                case 4:
                    return (moveDuration + rotate * rotateDuration + (round + 1) * colorDuration + (round + 1) * colorDuration + (round + 1) * unvisibleDurationInput + round * colorDuration + round * unvisibleDurationInput);
                //remove marks in Table
                case 5:
                    return (moveDuration + rotate * rotateDuration + (round + 1) * colorDuration + (round + 1) * colorDuration + (round + 1) * unvisibleDurationInput + (round + 1) * colorDuration + round * unvisibleDurationInput);
                //visible output
                case 6:
                    int count = outputWrap.Children.Count;
                    return moveDuration + 3 * rotateDuration + count * (colorDuration + colorDuration + unvisibleDurationInput + colorDuration + unvisibleDurationInput) + round * unvisibleDurationInput + moveDuration;
                default:
                    return 0;
            }
        }


        private double certainDurationD(int i, int round, int stencilEmptySize)
        {
            double rotate = round / stencilEmptySize;
            rotate = Math.Floor(rotate);
            int roundOld = stencilEmptySize * 4;
            switch (i)
            {
                //mark Letter input
                case 0:
                    return ((round+1) * colorDuration + round * colorDuration);
                //set Letter in Table visible
                case 1:
                    return ((round+1) * colorDuration + (round+1) * colorDuration);
                //move stencil
                case 2:
                    return ((round + 1) * colorDuration + (round + 1) * colorDuration + moveDuration);
                //rotate stencil
                case 3:
                    if (rotate == 0) 
                    {
                        rotate--;
                    }
                    return (roundOld * colorDuration + roundOld * colorDuration + moveDuration + (rotate - 1) * rotateDuration + round * colorDuration);
                //mark Letter in Table
                    //TODO: 4 und 5
                case 4:
                    return (roundOld * colorDuration + roundOld * colorDuration + moveDuration + (rotate - 1) * rotateDuration + (round + 1) * colorDuration);
                //visible output
                case 5:
                    return (roundOld * colorDuration + roundOld * colorDuration + moveDuration + (rotate) * rotateDuration + (round + 1) * colorDuration);
                default:
                    return 0;
            }
        }

        //private void speeder() 
        //{
        //    colorDuration = colorDuration * speed;
        //    moveDuration = moveDuration * speed;
        //    rotateDuration = rotateDuration * speed;
        //}

        #endregion

        #endregion

        private Storyboard animation()
        {
            Storyboard myStory = new Storyboard();

            #region declarating coloranimations and brushes
            SolidColorBrush brush_black = new SolidColorBrush(Colors.Black);
            SolidColorBrush brush_red = new SolidColorBrush(Colors.Red);
            #endregion
            
            myStory.Duration = new Duration(TimeSpan.FromMilliseconds(1001 * 2));

            //preReadOut();
            /*if (inputWrap.is != "") //Wennn inputTextBox noch nicht leer
            {

            }
            else //inputTextBox ist leer
            { 
            
            }*/

            return myStory;
        }

        //private void preReadOut()
        //{
        //    int abort = inputWrap.Children.Count;
        //    for (int ix = 0; ix < abort; ix++)
        //    {
        //        ColorAnimation myColorAnimation_green = new ColorAnimation(Colors.LawnGreen, Colors.Yellow, new Duration(TimeSpan.FromMilliseconds(1000)));
        //        myColorAnimation_green.BeginTime = TimeSpan.FromMilliseconds(ix * 3000 + 0);

        //        ColorAnimation myColorAnimation_blue = new ColorAnimation(Colors.AliceBlue, Colors.Yellow, new Duration(TimeSpan.FromMilliseconds(1000)));
        //        myColorAnimation_blue.BeginTime = TimeSpan.FromMilliseconds(ix * 3000 + 0);

        //        SolidColorBrush brush_green = new SolidColorBrush(Colors.LawnGreen);
        //        SolidColorBrush brush_blue = new SolidColorBrush(Colors.AliceBlue);
        //        SolidColorBrush brush_trans = new SolidColorBrush(Colors.Orange);

        //        if (inputWrap.Children.Count != 0)
        //        {
        //            foreach (TextBlock letter in inputWrap.Children)
        //            {
        //                mainstory1.Children.Add(myColorAnimation_green);
        //                letter.Background = brush_green;
        //                Storyboard.SetTarget(myColorAnimation_green, letter);
        //            }
        //            DoubleAnimation fadeOut = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromMilliseconds(1000)));
        //            fadeOut.BeginTime = TimeSpan.FromMilliseconds(ix * 3000 + 1000);
        //            mainstory1.Children.Add(fadeOut);
        //            Storyboard.SetTarget(fadeOut, inputWrap);
        //            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("(Opacity)"));
        //        }
        //    }
        //    mainstory1.Begin();
        //    mainstory1.SetSpeedRatio(speed / 100);
        //}

        private Storyboard animateEncrypt(Storyboard myStoryboard)
        {
            Storyboard storyboard = myStoryboard;

            DoubleAnimation myDoubleAnimation = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromMilliseconds(1000)));
            myDoubleAnimation.BeginTime = TimeSpan.FromMilliseconds(1001);
            //Storyboard.SetTarget(myDoubleAnimation, inputWrap.Text.);
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath("(Opacity)"));
            storyboard.Children.Add(myDoubleAnimation);

            return storyboard;
        }

        private Storyboard animateDecrypt(Storyboard myStoryboard) 
        {
            Storyboard storyboard = myStoryboard;

            return storyboard;
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
            mainStory.Pause();
            mainStory.SetSpeedRatio(speed / 100);
            mainStory.Resume();
            /*
            mainStory2.Pause();
            mainStory2.SetSpeedRatio(speed / 100);
            mainStory2.Resume();
            mainStory3.Pause();
            mainStory3.SetSpeedRatio(speed / 100);
            mainStory3.Resume();*/

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

        public void timerTick(object sender, EventArgs e)
        {
            try
            {
                if (Convert.ToDouble(mainStory.GetCurrentProgress().ToString()) != 1)
                    myupdateprogress(Convert.ToInt32(Convert.ToDouble(mainStory.GetCurrentProgress().ToString()) * 1000));
                /*else if (Convert.ToDouble(mainStory2.GetCurrentProgress().ToString()) != 1)
                    myupdateprogress(Convert.ToInt32(Convert.ToDouble(mainStory2.GetCurrentProgress().ToString()) * 1000) + 1000);
                else if (Convert.ToDouble(mainStory3.GetCurrentProgress().ToString()) != 1)
                    myupdateprogress(Convert.ToInt32(Convert.ToDouble(mainStory3.GetCurrentProgress().ToString()) * 1000) + 2000);
                */
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
        /// <summary>
        /// "emergengy break" 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void my_Stop(object sender, EventArgs e)
        {   //resetting the grid

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                inputLabel.Content = "input:";
                inputWrap.Children.Clear();
                stencilSizeLabel.Content = "StencilSize:";
                stencilCanvas.Children.Clear();
                fieldWrapPanel.Children.Clear();
                //outputWrap.Visibility = Visibility.Hidden;
                outputWrap.Children.Clear();
                canvasControlPanel.Visibility = Visibility.Hidden;
                progress = 0;
                //progressTimer.Stop();
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

        public void settings_OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            FleißnerGrilleSettings settings = sender as FleißnerGrilleSettings;

            if (e.PropertyName == "PresentationSpeed")
            {

                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    //Debug.Text = "" + settings.PresentationSpeed;
                    speed = settings.PresentationSpeed;
                    mainStory.Pause();
                    //storyboard.Pause();

                    mainStory.SetSpeedRatio(speed);
                    //storyboard.SetSpeedRatio(speed);

                    mainStory.Resume();
                    //storyboard.Resume();


                }, null);

            }
        }
        #endregion

        #endregion

        #region controlPanel
        private void buttonPlay_Click(object sender, RoutedEventArgs e)
        {
            mainStory.Begin(this,true);
        }

        private void buttonBreak_Click(object sender, RoutedEventArgs e)
        {
            mainStory.Pause(this);
            buttonPlay.Visibility = Visibility.Hidden;
        }

        private void buttonSpeed_Click(object sender, RoutedEventArgs e)
        {
            // Makes the storyboard progress three times as fast as normal.
            if (speed > 10 )
            {
                speed = speed - 10;
                //speeder();
                mainStory.SetSpeedRatio(this, speed);
            }
            else if (speed == 10) 
            { 
                //maximum Speed 
            }
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            mainStory.Stop(this);
            buttonPlay.Visibility = Visibility.Visible;
        }

        private void buttonResume_Click(object sender, RoutedEventArgs e)
        {
            mainStory.Resume(this);
        }

        private void buttonFillPeriod_Click(object sender, RoutedEventArgs e)
        {
            mainStory.SkipToFill(this);
            buttonPlay.Visibility = Visibility.Visible;
        }

        #endregion
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
