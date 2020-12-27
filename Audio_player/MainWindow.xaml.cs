using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Audio_player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// This is a test to see how to properly push into github using VS 
    public partial class MainWindow : Window
    {

        private MediaPlayer audioPlayer = new MediaPlayer();
        String currentSong;
        string path = Path.Combine(Environment.CurrentDirectory, @"Songs");
        private bool userIsDraggingSlider = false;
        private bool volumeSliderIsUsed = false;
        private bool reverseTime = false;
        private bool isOpened = false;
        private bool isReplayOn = false;

        /// <summary>
        ///  uztaisit array prieks playlist u dig?
        /// </summary>

        public MainWindow()
        {
            InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }
        /* Buttons */

        private void bttnFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = path;
            openFileDialog.Filter = "Media files (*.mp3;*.mpg;*.mpeg;*.mp4)|*.mp3;*.mpg;*.mpeg;*.mp4|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                audioPlayer.Open(new Uri(openFileDialog.FileName));
                isOpened = true;
                currentSong = openFileDialog.SafeFileName;
                songName.Text = currentSong.Substring(0,currentSong.Length - 4); // Removes the '.mp3' from song name.
                volumeSettings();
                audioPlayer.Play();
        }   

        private void bttnPlay_Click(object sender, RoutedEventArgs e)
        {
            audioPlayer.Play();
        }


        private void bttnPause_Click(object sender, RoutedEventArgs e)
        {
            audioPlayer.Pause();
        }


        private void bttn_Stop(object sender, RoutedEventArgs e)
        {
            audioPlayer.Stop();
        }

        private void bttn_replay(object sender, RoutedEventArgs e)
        {


            switch (isReplayOn)
            {
                case false:
                    isReplayOn = true;
                    repeat.Background = Brushes.Black;
                    audioPlayer.MediaEnded += new EventHandler(Media_Ended); // Adds the new event to MediaEnded.
                    break;

                case true:
                    isReplayOn = false;
                    repeat.Background = Brushes.LightGray;
                    audioPlayer.MediaEnded -= new EventHandler(Media_Ended); // Removes the new event to MediaEnded so that the song isn't repeated.
                    break;
            }
        }

        private void Media_Ended(object sender, EventArgs e) // Created a new event of what happens when the song ends.
        {
            audioPlayer.Position = TimeSpan.Zero;
            audioPlayer.Play();
        }



        /* Audio Seeker */
        private void timer_Tick(object sender, EventArgs e)
        {
            if ((isOpened == true) && (audioPlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                sliderProgress.Minimum = 0;
                sliderProgress.Maximum = audioPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliderProgress.Value = audioPlayer.Position.TotalSeconds;
                setLength();
                
            }
        }

        private void setLength()
        {
            lenght.Text = TimeSpan.FromSeconds(sliderProgress.Maximum).ToString(@"hh\:mm\:ss");
        }

        private void timeUpdate()
        {
            if (!reverseTime)
            {
                currentTime.Text = TimeSpan.FromSeconds(sliderProgress.Value).ToString(@"hh\:mm\:ss");
            }
            else
            {
                currentTime.Text = "-" + TimeSpan.FromSeconds(sliderProgress.Maximum - sliderProgress.Value).ToString(@"hh\:mm\:ss");
            }
        }

        private void sliderProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            timeUpdate();
        }

        private void sliderProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            audioPlayer.Position = TimeSpan.FromSeconds(sliderProgress.Value);
        }

        private void sliderProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void currentTime_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            reverseTime = !reverseTime;
            timeUpdate();

        }

        /* Volume Seeker */

        private void volumeSettings()
        {

            audioPlayer.Volume = 0.5;
            volumeSlider.Minimum = 0;
            volumeSlider.Maximum = 1;
            volumeSlider.Value = 0.5;

        }

        private void volumeSliderProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            audioPlayer.Volume = volumeSlider.Value;
        }

        private void volumeSliderProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void volumeSliderProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            audioPlayer.Volume = volumeSlider.Value;
        }

    }
}
