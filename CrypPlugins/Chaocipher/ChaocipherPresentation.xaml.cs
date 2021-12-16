using CrypTool.Chaocipher.Enums;
using CrypTool.Chaocipher.Models;
using CrypTool.Chaocipher.Presentation;
using CrypTool.Chaocipher.Properties;
using CrypTool.Chaocipher.Services;
using CrypTool.Chaocipher.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TranslationResources = CrypTool.Chaocipher.Properties.Resources;

namespace CrypTool.Chaocipher
{
    /// <summary>
    /// Interaction logic for ChaocipherPresentation.xaml
    /// </summary>
    public partial class ChaocipherPresentation : UserControl
    {
        private int _waitTime;
        private bool _isUserInteraction;
        private readonly AnimationPlayerService _animationPlayer;
        private CancellationTokenSource _source = new CancellationTokenSource();

        private readonly List<Step> _displayLeftArrow = new List<Step>
        {
            Step.BringCipherCharToZenith,
            Step.BringCharToZenith,
            Step.PermutateLeftDiskMoveChars,
            Step.PermutateRightDiskMoveChars
        };

        private readonly List<Step> _displayRightArrow = new List<Step>();

        public ChaocipherPresentation()
        {
            InitializeComponent();
            _animationPlayer = new AnimationPlayerService();
            _animationPlayer.NeedUpdate += (sender, args) =>
            {
                Play.Content = _animationPlayer.IsPlaying ? TranslationResources.Pause : TranslationResources.Play;
                ShowStep();
            };
            _animationPlayer.PlayingChanged += (sender, playing) =>
            {
                Play.Content = playing ? TranslationResources.Play : TranslationResources.Pause;
            };
            Steps.SelectionChanged += (sender, args) =>
            {
                if (Steps.SelectedIndex == -1)
                {
                    return;
                }

                if (_isUserInteraction)
                {
                    _isUserInteraction = false;
                    _animationPlayer.JumpToStep(Steps.SelectedIndex);
                }
            };
            Steps.PreviewMouseDown += (sender, args) => { _isUserInteraction = true; };
            SizeChanged += ChaocipherPresentation_SizeChanged;
            if (Steps.Columns.Count == 0)
            {
                Steps.Columns.Add(new DataGridTextColumn
                { Header = TranslationResources.StepCount, Binding = new Binding(nameof(Description.Index)) });
                Steps.Columns.Add(new DataGridTextColumn
                { Header = TranslationResources.Description, Binding = new Binding(nameof(Description.Text)) });
            }
        }

        private void ChaocipherPresentation_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ShowStep();
        }

        public void SetSpeed(int value)
        {
            _waitTime = Constants.MaxWaitTime + Constants.MinWaitTime - value;
        }

        private void ShowStep()
        {
            CanvasLeft.Children.Clear();
            CreateLegend();
            PresentationState state = _animationPlayer.GetStepToDisplay();
            if (state == null)
            {
                return;
            }

            BuildCircle(state.CipherWorkingAlphabet, false);
            BuildCircle(state.PlainWorkingAlphabet, true);
            AddFocusCharDisplay(state.InputCharInFocus);
            AddFocusCharDisplay(state.OutputCharInFocus, true);
            if (Steps.Items.Count == _animationPlayer.CurrentPosition)
            {
                return;
            }

            Steps.AutoGenerateColumns = false;
            Steps.ItemsSource = _animationPlayer.GetStepDescriptionToDisplay();
            Steps.SelectedIndex = _animationPlayer.CurrentPosition;
            Steps.ScrollIntoView(Steps.SelectedItem);
            if (_displayLeftArrow.Contains(state.Step))
            {
                DisplayArrows(true);
            }
            else if (_displayRightArrow.Contains(state.Step))
            {
                DisplayArrows(false);
            }
        }

        private void DisplayArrows(bool isLeft)
        {
            ResourceManager resourceManager = new ResourceManager("Chaocipher.g", Assembly.GetExecutingAssembly());
            ResourceSet resources = resourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true,
                true);
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = (Stream)(isLeft
                ? resources.GetObject("images/arrows_left.png")
                : resources.GetObject("images/arrows_right.png"));
            bitmap.EndInit();
            Image image = new Image
            {
                Width = CanvasLeft.ActualWidth * 0.1,
                Height = CanvasLeft.ActualHeight * 0.1,
                Source = bitmap,
            };
            Canvas.SetTop(image, CalculateCenter().Y - image.Height / 2);
            Canvas.SetLeft(image, CalculateCenter().X - image.Width / 2);
            CanvasLeft.Children.Add(image);
        }

        private void AddFocusCharDisplay(string stateInputCharInFocus, bool isRight = false)
        {
            const int textBoxRight = 10;
            const int textBoxLeft = 10;
            const int textBoxTop = 20;
            Label textBox = new Label
            {
                Content = isRight
                    ? TranslationResources.Result + ": " + stateInputCharInFocus
                    : TranslationResources.Input + ": " + stateInputCharInFocus,
                FontWeight = FontWeights.Bold,
                FontSize = 18,
                Name = "InputDisplay",
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Height = 30,
                Width = 130,
                Padding = new Thickness(5, 5, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new VisualBrush
                {
                    Visual = new Rectangle
                    {
                        Width = 90,
                        Height = 40,
                        Fill = Brushes.WhiteSmoke,
                    },
                },
            };
            if (isRight)
            {
                Canvas.SetRight(textBox, textBoxRight);
                Canvas.SetTop(textBox, textBoxTop);
            }
            else
            {
                Canvas.SetLeft(textBox, textBoxLeft);
                Canvas.SetTop(textBox, textBoxTop);
            }

            CanvasLeft.Children.Add(textBox);

            Arrow arrow = new Arrow
            {
                X1 = textBox.Width + 10 + textBoxLeft,
                X2 = CanvasLeft.ActualWidth - textBox.Width - 10 - textBoxRight,
                Y1 = textBox.Height / 2 + textBoxTop,
                Y2 = textBox.Height / 2 + textBoxTop,
                HeadHeight = 5,
                HeadWidth = 5,
                Stroke = Brushes.Black,
                StrokeThickness = 3
            };
            CanvasLeft.Children.Add(arrow);
        }


        private double CalculateRadius(bool isInner)
        {
            double shortestSide = Math.Min(CanvasLeft.ActualWidth, CanvasLeft.ActualHeight);
            double diameter = shortestSide - shortestSide * 0.05;
            diameter /= 2;
            return isInner ? diameter - 50 : diameter;
        }

        private static double CalculatePosition(int index, char[] arr)
        {
            return CalculateAngle(arr) * index;
        }

        private static double CalculateAngle(char[] arr)
        {
            return 360d / arr?.Length ?? 360;
        }

        private (double X, double Y) CalculateCenter()
        {
            return (CanvasLeft.ActualWidth / 2, CanvasLeft.ActualHeight / 2);
        }

        private void BuildCircle(char[] arr, bool isPlain)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == char.MinValue)
                {
                    continue;
                }

                CreatePiePiece(i, arr, isPlain);
            }

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == char.MinValue)
                {
                    continue;
                }

                CreatePiePieceText(i, char.ToString(arr[i]), arr, isPlain);
            }
        }

        private void CreateLegend()
        {
            Ellipse circle1 = new Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = Brushes.Green
            };
            Ellipse circle2 = new Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = Brushes.Blue
            };
            Label label1 = new Label
            {
                Content = PresentationTranslation.Zenith,
                FontSize = 12
            };
            Label label2 = new Label
            {
                Content = PresentationTranslation.Nadir,
                FontSize = 12
            };

            Canvas.SetBottom(label1, 7.5);
            Canvas.SetBottom(label2, 37.5);
            Canvas.SetLeft(label1, 30);
            Canvas.SetLeft(label2, 30);
            Canvas.SetBottom(circle1, 10);
            Canvas.SetBottom(circle2, 40);
            Canvas.SetLeft(circle1, 10);
            Canvas.SetLeft(circle2, 10);

            CanvasLeft.Children.Add(circle1);
            CanvasLeft.Children.Add(circle2);
            CanvasLeft.Children.Add(label1);
            CanvasLeft.Children.Add(label2);
        }

        private void CreatePiePieceText(int index, string character, char[] arr, bool isInner)
        {
            PieText piePieceText = new PieText
            {
                Name = "text" + index + (isInner ? "i" : ""),
                CentreX = CalculateCenter().X,
                CentreY = CalculateCenter().Y,
                Rotation = -CalculatePosition(index, arr),
                Angle = CalculateAngle(arr),
                Radius = CalculateRadius(isInner),
                Text = character,
                Stroke = Brushes.Black,
                Fill = Brushes.Black,
            };
            CanvasLeft.Children.Add(piePieceText);
        }

        protected void CreatePiePiece(int index, char[] arr, bool isInner)
        {
            SolidColorBrush brush;
            switch (index)
            {
                case 0:
                    brush = Brushes.Green;
                    break;
                case 13:
                    brush = Brushes.Blue;
                    break;
                default:
                    brush = Brushes.WhiteSmoke;
                    break;
            }

            Pie piePiece = new Pie
            {
                Name = "piece" + index + (isInner ? "i" : ""),
                CentreX = CalculateCenter().X,
                CentreY = CalculateCenter().Y,
                Radius = CalculateRadius(isInner),
                Angle = CalculateAngle(arr),
                Rotation = -CalculatePosition(index, arr) - CalculateAngle(arr) * 0.5,
                Fill = Brushes.Transparent,
                Stroke = brush
            };
            CanvasLeft.Children.Add(piePiece);
        }

        public async Task ShowEncipher(CipherResult cipherResult)
        {
            SetEnabled(true);
            _animationPlayer.LoadAnimationStates(cipherResult.PresentationStates);
            _animationPlayer.Play();
            await ShowSteps();
            SetEnabled(false);
        }

        public async Task ShowDecipher(CipherResult cipherResult)
        {
            SetEnabled(true);
            _animationPlayer.LoadAnimationStates(cipherResult.PresentationStates);
            _animationPlayer.Play();
            await ShowSteps();
            SetEnabled(false);
        }


        private async Task ShowSteps()
        {
            while (true)
            {
                while (_animationPlayer.HasNext())
                {
                    if (_animationPlayer.IsPlaying)
                    {
                        ShowStep();
                        _animationPlayer.MoveNext();
                    }

                    try
                    {
                        await Task.Delay(_waitTime, _source.Token);
                    }
                    catch
                    {
                        _source.Dispose();
                        _source = new CancellationTokenSource();
                        // ignore
                    }
                }

                _animationPlayer.RestartAnimation();
                ShowStep();
            }
        }

        private void SetEnabled(bool isEnabled)
        {
            Forward.IsEnabled = isEnabled;
            Back.IsEnabled = isEnabled;
            Steps.IsEnabled = isEnabled;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _source.Cancel();
            _animationPlayer.Backward();
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            _source.Cancel();
            _animationPlayer.Forward();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            _source.Cancel();
            if (!_animationPlayer.IsDataAvailable)
            {
                return;
            }

            if (_animationPlayer.IsPlaying)
            {
                _animationPlayer.Pause();
            }
            else
            {
                _animationPlayer.Play();
            }
        }

        public void Stop()
        {
            _animationPlayer.Pause();
        }
    }
}