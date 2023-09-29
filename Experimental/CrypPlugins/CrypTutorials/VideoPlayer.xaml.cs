using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using System.Globalization;
using System.Windows.Threading;
using Vlc.DotNet.Core;
using Vlc.DotNet.Core.Medias;
using Vlc.DotNet.Wpf;
using Microsoft.Win32;
using System.Linq;

namespace CrypTool.CrypTutorials
{

    public partial class VideoPlayer : UserControl
    {

        public static readonly DependencyProperty VolumeProperty =
            DependencyProperty.Register("Volume", typeof(int),
            typeof(VideoPlayer), new FrameworkPropertyMetadata(40, FrameworkPropertyMetadataOptions.AffectsRender, OnVolumeChanged));

        public int Volume
        {
            get { return (int)GetValue(VolumeProperty); }
            set { SetValue(VolumeProperty, value); }
        }

        public static readonly DependencyProperty IsPlayingProperty =
            DependencyProperty.Register("IsPlaying", typeof(bool),
            typeof(VideoPlayer), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, OnIsPlaying));

        public bool IsPlaying
        {
            get { return (bool)GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }

        private readonly DispatcherTimer _timer2 = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 3) };
        private readonly DispatcherTimer _timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1) };
        private static void OnIsPlaying(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            var player = (VideoPlayer)sender;
            if (player.IsPlaying)
            {
                player._timer.Start();
            }
            else
            {
                player._timer.Stop();
            }
        }

        private static void OnVolumeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            VideoPlayer player = (VideoPlayer)sender;
            if (player.MyVlcControl != null)
            {
                player.MyVlcControl.AudioProperties.Volume = player.Volume;
            }
        }

        private VlcControl myVlcControl;
        public VlcControl MyVlcControl { get { return myVlcControl; } set { myVlcControl = value; } }
       

        public VideoPlayer()
        {
            VlcLoad.VlcInitialized += new EventHandler(VlcInitializedHandler);
            DataContext = this;

            PreviewMouseMove += VideoPlayer_PreviewMouseMove;

            _timer.Tick += delegate(object o, EventArgs args)
            {
                var seSliderValue = (double)myVlcControl.Time.TotalSeconds;
                timelineSlider.Value = seSliderValue;
                timelineSlider.Maximum = MyVlcControl.Media.Duration.TotalSeconds;
            };

            _timer2.Tick += delegate(object o, EventArgs args)
            {
                Controls.Visibility = Visibility.Collapsed;
                _timer2.Stop();
            };

        }

        void VlcInitializedHandler(object sender, EventArgs e)
        {
            MyVlcControl = new VlcControl();
            InitializeComponent();
        }

        void media_StateChanged(MediaBase sender, VlcEventArgs<Vlc.DotNet.Core.Interops.Signatures.LibVlc.Media.States> e)
        {
            switch (e.Data)
            {
                case Vlc.DotNet.Core.Interops.Signatures.LibVlc.Media.States.Buffering:
                    LoadingVisual.Visibility = Visibility.Visible;
                    break;
                case Vlc.DotNet.Core.Interops.Signatures.LibVlc.Media.States.Playing:
                    LoadingVisual.Visibility = Visibility.Collapsed;
                    break;
                case Vlc.DotNet.Core.Interops.Signatures.LibVlc.Media.States.NothingSpecial:
                    break;
            }
        }


        void VideoPlayer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Controls.Visibility = Visibility.Visible;
            _timer2.Start();
        }


        public void PlayOrPause()
        {
            if (IsPlaying)
            {
                Pause();
                IsPlaying = false;
            }
            else
            {
                Play();
                IsPlaying = true;
            }
        }


        // Play the media.
        void PlayClick(object sender, RoutedEventArgs args)
        {
            // The Play method will begin the media if it is not currently active or 
            // resume media if it is paused. This has no effect if the media is
            // already running.

            PlayOrPause();

        }

        public void Stop()
        {
            MyVlcControl.Stop();
            IsPlaying = false;
        }

        public void Pause()
        {
            MyVlcControl.Pause();
            IsPlaying = false;
        }

        public void Play(LocationMedia media)
        {
            if (MyVlcControl.Media != null)
            {
                MyVlcControl.Media.StateChanged -= new VlcEventHandler<MediaBase, Vlc.DotNet.Core.Interops.Signatures.LibVlc.Media.States>(media_StateChanged);
                MyVlcControl.Media.DurationChanged -= new VlcEventHandler<MediaBase, long>(Media_DurationChanged);
                MyVlcControl.Media.Dispose();
            }

            MyVlcControl.Media = media;
            MyVlcControl.Media.StateChanged += new VlcEventHandler<MediaBase, Vlc.DotNet.Core.Interops.Signatures.LibVlc.Media.States>(media_StateChanged);
            MyVlcControl.Media.DurationChanged += new VlcEventHandler<MediaBase, long>(Media_DurationChanged);
            MyVlcControl.Play(media);
            IsPlaying = true;
        }

        void Media_DurationChanged(MediaBase sender, VlcEventArgs<long> e)
        {
            timelineSlider.Maximum = MyVlcControl.Media.Duration.TotalSeconds;
        }

        public void Play(string p)
        {
            Play(new LocationMedia(p));
        }

        public void Play()
        {
            MyVlcControl.Play();
            IsPlaying = true;
        }

        public void Close()
        {
            if (_fullScreen.IsVisible)
            {
                CloseFullscreen();
            }
            Stop();
        }


        void seek()
        {
            var sliderValue = (int)timelineSlider.Value;

            // Overloaded constructor takes the arguments days, hours, minutes, seconds, miniseconds.
            // Create a TimeSpan with miliseconds equal to the slider value.
            var ts = TimeSpan.FromSeconds(sliderValue);
            MyVlcControl.Time = ts;
        }

        void seek(double time)
        {

            // Overloaded constructor takes the arguments days, hours, minutes, seconds, miniseconds.
            // Create a TimeSpan with miliseconds equal to the slider value.

            TimeSpan ts = TimeSpan.FromSeconds(time);
            MyVlcControl.Time = ts;
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var x = sender as FrameworkElement;
            int value = int.Parse(x.Tag.ToString(), CultureInfo.InvariantCulture);

            if (Volume == value)
                Volume = 0;
            else
                Volume = value;
        }

        private Panel _preMaximizedVisualParent;
        private readonly Window _fullScreen = new Window() { WindowStyle = WindowStyle.None, ResizeMode = ResizeMode.NoResize, WindowState = WindowState.Maximized };

        private void DoFullscreen(object sender, MouseButtonEventArgs e)
        {
            if (_preMaximizedVisualParent != null)
            {
                CloseFullscreen();
            }
            else
            {
                _preMaximizedVisualParent = (Panel)this.VisualParent;
                _preMaximizedVisualParent.Children.Remove(this);
                _fullScreen.Content = this;
                _fullScreen.Show();

                _fullScreen.ContentRendered += fullScreen_ContentRendered;
            }
        }

        private void CloseFullscreen()
        {
            _fullScreen.Content = null;
            _fullScreen.Hide();
            _preMaximizedVisualParent.Children.Add(this);
            _preMaximizedVisualParent = null;
        }

        void fullScreen_ContentRendered(object sender, EventArgs e)
        {
            var window = sender as Window;
            if (window != null) window.ContentRendered -= fullScreen_ContentRendered;
        }

        private void SeekToMediaPosition(object sender, MouseButtonEventArgs e)
        {
            seek();
        }
    }

    public class VolumeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var x = (int) value;
            var y = int.Parse(parameter.ToString(), CultureInfo.InvariantCulture);

            if (x >= y)
            {
                var fromString = ColorConverter.ConvertFromString("#00a8ff");
                if (fromString != null)
                {
                    return new SolidColorBrush((Color) fromString);
                }
            }

            var convertFromString = ColorConverter.ConvertFromString("#ccc");
            return convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}