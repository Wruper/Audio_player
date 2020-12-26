using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;
        private bool reverseTime = false;
        private bool isOpened = false;
        private bool isPausedByClick = false;


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
            openFileDialog.Filter = "Media files (*.mp3;*.mpg;*.mpeg;*.mp4)|*.mp3;*.mpg;*.mpeg;*.mp4|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                audioPlayer.Open(new Uri(openFileDialog.FileName));
                isOpened = true;
                currentSong = openFileDialog.SafeFileName;
                songName.Text = currentSong.Substring(0,currentSong.Length - 4); // Removes the '.mp3' from song name.
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

        /* Navigation bar */
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


        private void currentTime_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            reverseTime = !reverseTime;
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



    }
}
