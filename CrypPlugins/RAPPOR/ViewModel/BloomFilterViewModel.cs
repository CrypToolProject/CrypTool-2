using System;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CrypTool.PluginBase;
using RAPPOR;
using RAPPOR.Helper;
using RAPPOR.Helper.ArrayDrawer;
using RAPPOR.Model;


namespace CrypTool.Plugins.RAPPOR.ViewModel
{
    /// <summary>
    /// This class represents the binding part between the Bloom filter class and the Bloom filter
    /// view. It is used for the ui logic, displaying the bloom filter in the component.
    /// </summary>
    public class BloomFilterViewModel : ObservableObject, IViewModelBase
    {
        /// <summary>
        /// The used instance of the rappor class.
        /// </summary>
        private readonly RAPPOR rappor;

        /// <summary>
        /// An used instance of the array drawer.
        /// </summary>
        private readonly ArrayDrawer arrayDrawer;

        /// <summary>
        /// An used instance of the Bloom filter canvas.
        /// </summary>
        private Canvas _bloomFilterCanvas;

        /// <summary>
        /// An used instance of the dispatcher timer instacnce.
        /// </summary>
        private readonly System.Windows.Threading.DispatcherTimer dispatcherTimer;

        /// <summary>
        /// This boolean array is used to save the bloom filter for internal calculations.
        /// </summary>
        private bool[][] tempBoolArray;

        /// <summary>
        /// Drawer class which is intended for the heatmaps view but also utilized here.
        /// </summary>
        private readonly ArrayDrawerHeatMaps arrayDrawerHM;

        /// <summary>
        /// Internal parameter to show if the bloom filter animation is paused.
        /// </summary>
        private bool pause;

        /// <summary>
        /// Initializes the Bloom filter view model. This is then used to create the Bloom filter 
        /// content part of the RAPPOR component
        /// </summary>
        /// <param name="rAPPOR"></param>
        public BloomFilterViewModel(RAPPOR rAPPOR)
        {
            pause = true;
            arrayDrawerHM = new ArrayDrawerHeatMaps();
            rappor = rAPPOR;
            rappor.RunRappor();
            arrayDrawer = new ArrayDrawer();
            _bloomFilterCanvas = new Canvas();

            //Initializes the timer used for the animation
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);

            rappor.PropertyChanged += BloomFilterCanvas_PropertyChanged;
            //slValue = "1";
            //OnPropertyChanged("slValue");
        }

        /// <summary>
        /// The binding variable of the amount of hash functions.
        /// </summary>
        private string _timerInput;

        /// <summary>
        /// Gets and sets the amount of hashfunctions used in the component.
        /// </summary>
        public string slValue
        {
            get
            {
                if (string.IsNullOrEmpty(_timerInput))
                {
                    _timerInput = "1";
                    return "1";
                }


                return _timerInput;
            }
            set
            {
                if (double.TryParse(value, out double a))
                {


                    if (value == "")
                    {
                        value = "1";
                        dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)(double.Parse(value) * 1000));
                        _timerInput = value;
                        OnPropertyChanged("slValue");
                    }
                    else if (double.Parse(value) < 0.1)
                    {
                        value = "1";
                        dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)(double.Parse(value) * 1000));
                        _timerInput = value;
                        OnPropertyChanged("slValue");
                    }
                    a = 6 - a;
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)a * 500);
                    _timerInput = value;
                    OnPropertyChanged("slValue");
                }
                else
                {
                    //This value is insertet for the russian culture conversion since it otherwise does not work.
                    //It has no relevance for the other languages.
                    value = "1";
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)(double.Parse(value) * 1000));
                    _timerInput = value;
                    OnPropertyChanged("slValue");
                    rappor.GetRAPPORSettings().OnLogMessage("There is a problem with the input for the timer. Using 0.1 Seconds as timer", NotificationLevel.Info);
                }
            }

        }

        #region Commands

        /// <summary>
        /// The binding start button variable, it is used to start the bloom filter animation.
        /// </summary>
        private ICommand _startButtonCommand;

        /// <summary>
        /// Gets the start button command.
        /// </summary>
        public ICommand StartButtonCommand => _startButtonCommand ?? (_startButtonCommand =
                    new CommandHandler(() => MyActionStartButtonCommand(), () => CanExecuteStartButtonCommand));

        /// <summary>
        /// Checks if the start button command can be started and therefore the animation can be
        /// executed.
        /// </summary>
        public bool CanExecuteStartButtonCommand =>
                // check if executing is allowed, i.e., validate, check if a process is running, etc. 
                true;


        /// <summary>
        /// This method starts the animation. It sets the Bloom filter in accordance to the given 
        /// userset parameters and the draws the canvas for the animation in a dispatch time 
        /// ticker.
        /// </summary>
        public void MyActionStartButtonCommand()
        {
            rappor.SetBloomFilter(new BloomFilter(rappor.Input.Split(','),
                rappor.GetRAPPORSettings().GetSizeOfBloomFilter(),
                rappor.GetRAPPORSettings().GetAmountOfHashFunctions()));

            UpdateTempBoolArray();

            BloomFilterCanvas.Background = Brushes.Red;

            OnPropertyChanged("BloomFilterCanvas");
            //TODO: Create a working try catch here.
            try
            {
                dispatcherTimer.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        /// <summary>
        /// variable used for internal calculations.
        /// </summary>
        private int timer = 0;

        /// <summary>
        /// This dispatcher time method is used to animate the Bloom filter. It redraws the animation continously.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text composition event arg.</param>
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                UpdateBloomFilterBig();

                OnPropertyChanged("BloomFilterCanvas");
                if (timer >= rappor.GetBloomFilter().GetHistory().Length) //-1
                {
                    dispatcherTimer.Stop();
                }
                else
                {
                    timer++;
                }

                rappor.GuiLogMessage("Time tick has passed", NotificationLevel.Debug);
            }
            catch (Exception exception)
            {
                rappor.GuiLogMessage(" Failure : " + exception.Message, NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Binding variable for the reset button command.
        /// </summary>
        private ICommand _resetButtonCommand;

        /// <summary>
        /// The method represents the reset button command.
        /// </summary>
        public ICommand ResetButtonCommand => _resetButtonCommand ?? (_resetButtonCommand =
                    new CommandHandler(() => MyActionResetButtonCommand(), () => CanExecuteResetButtonCommand));

        /// <summary>
        /// This method checks if the reset button can be executed in the current state.
        /// </summary>
        public bool CanExecuteResetButtonCommand =>
                // check if executing is allowed, i.e., validate, check if a process is running, etc. 
                true;

        /// <summary>
        /// This method is used to reset the Bloom filter animation to an initial state.
        /// </summary>
        public void MyActionResetButtonCommand()
        {
            rappor.Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
           {
               dispatcherTimer.Stop();
               timer = 0;
               UpdateTempBoolArray();
               rappor.SetBloomFilter(new BloomFilter(rappor.Input.Split(','),
                   rappor.GetRAPPORSettings().GetSizeOfBloomFilter(),
                   rappor.GetRAPPORSettings().GetAmountOfHashFunctions()));
               UpdateBloomFilterSmall();
           }, null);
            OnPropertyChanged("BloomFilterCanvas");
        }

        /// <summary>
        /// Binding variable for the reset button command.
        /// </summary>
        private ICommand _pauseButtonCommand;

        /// <summary>
        /// The method represents the reset button command.
        /// </summary>
        public ICommand PauseButtonCommand => _pauseButtonCommand ?? (_pauseButtonCommand =
                    new CommandHandler(() => MyActionPauseButtonCommand(), () => CanExecutePauseButtonCommand));

        /// <summary>
        /// This method checks if the reset button can be executed in the current state.
        /// </summary>
        public bool CanExecutePauseButtonCommand =>
                // check if executing is allowed, i.e., validate, check if a process is running, etc. 
                true;

        /// <summary>
        /// This method is used to reset the Bloom filter animation to an initial state.
        /// </summary>
        public void MyActionPauseButtonCommand()
        {
            if (pause)
            {
                rappor.Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {
                   dispatcherTimer.Stop();
                   UpdateBloomFilterBig();
               }, null);
                OnPropertyChanged("BloomFilterCanvas");
                pause = false;
            }
            else
            {
                if (timer >= rappor.GetBloomFilter().GetHistory().Length) //-1
                {
                }
                else
                {
                    dispatcherTimer.Start();
                }

                pause = true;
            }
        }

        /// <summary>
        /// Binding variable for the reset button command.
        /// </summary>
        private ICommand _previousStepCommand;

        /// <summary>
        /// The method represents the reset button command.
        /// </summary>
        public ICommand PreviousStepCommand => _previousStepCommand ?? (_previousStepCommand =
                    new CommandHandler(() => MyActionPreviousStepCommand(), () => CanExecutePreviousStepCommand));

        /// <summary>
        /// This method checks if the reset button can be executed in the current state.
        /// </summary>
        public bool CanExecutePreviousStepCommand =>
                // check if executing is allowed, i.e., validate, check if a process is running, etc. 
                true;

        /// <summary>
        /// This method is used to reset the Bloom filter animation to an initial state.
        /// </summary>
        public void MyActionPreviousStepCommand()
        {
            rappor.Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
           {
               dispatcherTimer.Stop();
               if (timer > 0)
               {
                   timer--;
                   UpdateBloomFilterBig();
               }


               if (pause)
               {
                   pause = false;
               }
           }, null);
            OnPropertyChanged("BloomFilterCanvas");
        }

        /// <summary>
        /// Binding variable for the reset button command.
        /// </summary>
        private ICommand _nextStepCommand;

        /// <summary>
        /// The method represents the reset button command.
        /// </summary>
        public ICommand NextStepCommand => _nextStepCommand ?? (_nextStepCommand =
                    new CommandHandler(() => MyActionNextStepCommand(), () => CanExecuteNextStepCommand));

        /// <summary>
        /// This method checks if the reset button can be executed in the current state.
        /// </summary>
        public bool CanExecuteNextStepCommand =>
                // check if executing is allowed, i.e., validate, check if a process is running, etc. 
                true;

        /// <summary>
        /// This method is used to reset the Bloom filter animation to an initial state.
        /// </summary>
        public void MyActionNextStepCommand()
        {
            rappor.Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
           {
               dispatcherTimer.Stop();

               if (timer < rappor.GetBloomFilter().GetHistory().Length)
               {
                   timer++;
                   UpdateBloomFilterBig();
               }

               if (pause)
               {
                   pause = false;
               }
           }, null);
            OnPropertyChanged("BloomFilterCanvas");
        }


        /// <summary>
        /// internal class used for the button commands.
        /// </summary>
        public class CommandHandler : ICommand
        {
            private readonly Action _action;
            private readonly Func<bool> _canExecute;

            /// <summary>
            /// Creates instance of the command handler
            /// </summary>
            /// <param name="action">Action to be executed by the command</param>
            /// <param name="canExecute">A bolean property to containing current permissions to execute the command</param>
            public CommandHandler(Action action, Func<bool> canExecute)
            {
                _action = action;
                _canExecute = canExecute;
            }

            /// <summary>
            /// Wires CanExecuteChanged event 
            /// </summary>
            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            /// <summary>
            /// Forcess checking if execute is allowed
            /// </summary>
            /// <param name="parameter"></param>
            /// <returns></returns>
            public bool CanExecute(object parameter)
            {
                return _canExecute.Invoke();
            }

            public void Execute(object parameter)
            {
                _action();
            }
        }

        #endregion


        /// <summary>
        /// This canvas represents the Bloom filter as it is seen in the animation.
        /// </summary>
        public Canvas BloomFilterCanvas
        {
            get
            {
                if (_bloomFilterCanvas == null)
                {
                    return null;
                }

                return _bloomFilterCanvas;
            }
            set
            {
                _bloomFilterCanvas = value;
                OnPropertyChanged("BloomFilterCanvas");
            }

        }

        /// <summary>
        /// This method checks if the binding bloom filter canvas variable has been alterered.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text composition event arg.</param>
        private void BloomFilterCanvas_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BloomFilterCanvas")
            {
                BloomFilterCanvas = rappor.GetBloomFilter().BloomFilterArray;
            }
        }

        /// <summary>
        /// This method is used to draw the canvas when the animation is stopped/resetted.
        /// </summary>
        public void DrawCanvas()
        {
            if (dispatcherTimer != null)
            {
                dispatcherTimer.Stop();
                timer = 0;
                UpdateTempBoolArray();
                rappor.SetBloomFilter(new BloomFilter(rappor.Input.Split(','),
                    rappor.GetRAPPORSettings().GetSizeOfBloomFilter(),
                    rappor.GetRAPPORSettings().GetAmountOfHashFunctions()));
                UpdateBloomFilterSmall();
            }
        }



        /// <summary>
        /// This method is used to update the temporay bloom array of the animation
        /// </summary>
        private void UpdateTempBoolArray()
        {
            rappor.RunRappor();
            tempBoolArray = new bool[rappor.Input.Split(',').Length + 1][];
            for (int i = 0; i < tempBoolArray.Length; i++)
            {
                tempBoolArray[i] = new bool[rappor.GetBloomFilter().GetBoolArray().Length];
            }

            for (int i = 0; i < rappor.GetBloomFilter().GetBoolArray().Length; i++)
            {
                tempBoolArray[0][i] = false;
            }

            for (int i = 1; i < rappor.Input.Split(',').Length + 1; i++)
            {
                for (int j = 0; j < rappor.GetBloomFilter().GetBoolArray().Length; j++)
                {
                    tempBoolArray[i][j] = tempBoolArray[i - 1][j];
                }

                for (int j = 0; j < rappor.GetRAPPORSettings().AmountOfHashFunctions; j++)
                {
                    tempBoolArray[i][rappor.GetBloomFilter().GetHistory()[i - 1][j]] = true;
                }
            }
        }

        /// <summary>
        /// This method is used to update relevant parts of the bloom filter view
        /// </summary>
        private void UpdateBloomFilterSmall()
        {
            BloomFilterCanvas.Children.Clear();
            BloomFilterCanvas.Children.Add(arrayDrawer.CreateBloomFilterStaticPathLightGray());
            BloomFilterCanvas.Children.Add(arrayDrawer.CreateBloomFilterPathBlack(tempBoolArray[timer],
                rappor.GetBloomFilter(), rappor.Input, -1));
            UpdateMovingParts(tempBoolArray[timer], false, rappor.GetBloomFilter().GetHistory()[timer]);
        }

        /// <summary>
        /// This method is used to update all parts of the bloom filter view.
        /// </summary>
        private void UpdateBloomFilterBig()
        {
            BloomFilterCanvas.Children.Clear();
            BloomFilterCanvas.Children.Add(arrayDrawer.CreateBloomFilterStaticPathLightGray());
            BloomFilterCanvas.Children.Add(arrayDrawer.CreateBloomFilterPathBlack(tempBoolArray[timer],
                rappor.GetBloomFilter(), rappor.Input, timer - 1));
            if (timer == 0)
            {
                UpdateMovingParts(tempBoolArray[timer], false, rappor.GetBloomFilter().GetHistory()[timer]);
            }
            else
            {
                UpdateMovingParts(tempBoolArray[timer], true, rappor.GetBloomFilter().GetHistory()[timer - 1]);
            }
        }

        /// <summary>
        /// This method is used to update all moving parts of the bloom filter view
        /// </summary>
        /// <param name="bArray">The boolean array used to update the moving parts of the bloom filter view</param>
        /// <param name="arrow">If the dynamic arrows of the bloom filter view are supposed to be displayed</param>
        /// <param name="iArray">The iArray used for the methdo</param>
        private void UpdateMovingParts(bool[] bArray, bool arrow, int[] iArray)
        {
            if (rappor.Input != "")
            {
                BloomFilterCanvas.Children.Add(arrayDrawer.CreateArray(tempBoolArray[timer], true));
                BloomFilterCanvas.Children.Add(arrayDrawer.CreateArray(tempBoolArray[timer], false));
            }
            else
            {
                BloomFilterCanvas.Children.Add(arrayDrawer.CreateArray(tempBoolArray[0], false));
            }

            if (arrow)
            {
                for (int i = 0; i < iArray.Length; i++)
                {
                    if (rappor.Input != "")
                    {
                        BloomFilterCanvas.Children.Add(arrayDrawer.CreateBloomFilterDynamicArrow(iArray[i], bArray));
                    }
                }
            }
        }
    }
}