using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Audio_player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// This is a test to see how to properly push into github using VS 
    public partial class MainWindow : Window
    {

        private MediaPlayer audioPlayer = new MediaPlayer();

        private bool audioPlayerIsPlaying = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void bttnFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Media files (*.mp3;*.mpg;*.mpeg;*.mp4)|*.mp3;*.mpg;*.mpeg;*.mp4|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                audioPlayer.Open(new Uri(openFileDialog.FileName));
        }

        private void bttnPlay_Click(object sender, RoutedEventArgs e)
        {
            audioPlayer.Play();
            audioPlayerIsPlaying = true;
        }


        private void bttnPause_Click(object sender, RoutedEventArgs e)
        {
            audioPlayer.Pause();
        }
    }
}
