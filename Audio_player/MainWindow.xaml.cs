using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Audio_player
{
 
    public partial class MainWindow : Window
    {

        private MediaPlayer audioPlayer = new MediaPlayer();
        OpenFileDialog openFileDialog = new OpenFileDialog();
        String currentSong = ""; //Current songs title
        int currentSongID = 0; // current song ID
        string path = Path.Combine(Environment.CurrentDirectory, @"Songs"); //Path for Songs folder.
   

        private bool isUserDraggingSlider = false;
        private bool reverseTime = false;
        private bool isOpened = false;
        private bool isReplayOn = false;
        private bool isShuffleOn = false;
        private bool isPlayOn = true; // Default is true, because when a song is picked, the song is played automaticly.

        public MainWindow()
        {
            InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TimerTick;
            timer.Start();
            AddToPlaylist(); // As soon a song is opened, the playlist updates with songs in 'path' folder.
            audioPlayer.MediaEnded += new EventHandler(MediaEndedNextSong); // By default when a song ends, the next song in the playlist is played.
        }

        /* Buttons */

        private void SelectSong(object sender, RoutedEventArgs e)
        {
            openFileDialog.InitialDirectory = path; //Default folder which opens when button is clicked.
            openFileDialog.Filter = "Media files (*.mp3;*)|*.mp3;|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                audioPlayer.Open(new Uri(openFileDialog.FileName));
                isOpened = true;
                currentSong = openFileDialog.SafeFileName;
                songName.Text = currentSong.Substring(0,currentSong.Length - 4); // Removes the '.mp3' from song name.
                VolumeSettings(); // Enables volume settings
                audioPlayer.Play();
            }
        }   

        private void PlayOrPauseSong(object sender, RoutedEventArgs e)
        {
            if (isPlayOn)
            {
                isPlayOn = false;
                audioPlayer.Pause();
                playAndPause.Content = FindResource("Play");
            }
            else
            {
                audioPlayer.Play();
                isPlayOn = true;
                playAndPause.Content = FindResource("Pause");
            }
        }

        private void StopSong(object sender, RoutedEventArgs e)
        {
            audioPlayer.Stop();
        }

        private void ReplaySong(object sender, RoutedEventArgs e)
        {
            switch (isReplayOn)
            {
                case false:
                    isReplayOn = true;
                    replaySong.Background = Brushes.White;
                    if (isShuffleOn)
                    {
                        isShuffleOn = false; // If shuffle is on, we turn it off so that shuffle doesn't collide with replay.
                        shuffleSong.Background = Brushes.Red;
                        audioPlayer.MediaEnded -= new EventHandler(MediaEndedShuffleSong); // Removes shuffle.
                        audioPlayer.MediaEnded += new EventHandler(MediaEndedReplaySong); // Adds replay event to MediaEnded.
                    }
                    else
                    {
                        audioPlayer.MediaEnded -= new EventHandler(MediaEndedNextSong); // Removes the default Event.
                        audioPlayer.MediaEnded += new EventHandler(MediaEndedReplaySong); // Adds replay event to MediaEnded.
                    }
                    break;

                case true:
                    isReplayOn = false;
                    replaySong.Background = Brushes.Red;
                    audioPlayer.MediaEnded -= new EventHandler(MediaEndedReplaySong);// Removes replay event from MediaEnded.
                    audioPlayer.MediaEnded += new EventHandler(MediaEndedNextSong);// Puts back the default Event.
                    break;
            }
        }

        private void ShuffleSong(object sender, RoutedEventArgs e)
        {
            switch (isShuffleOn)
            {
                case false:
                    isShuffleOn = true;
                    shuffleSong.Background = Brushes.White;
                    if (isReplayOn)
                    {
                        isReplayOn = false;
                        replaySong.Background = Brushes.Red;
                        audioPlayer.MediaEnded -= new EventHandler(MediaEndedReplaySong); // Removes replay event from MediaEnded.
                        audioPlayer.MediaEnded += new EventHandler(MediaEndedShuffleSong); // Adds shuffle event.
                    }
                    else
                    {
                        audioPlayer.MediaEnded -= new EventHandler(MediaEndedNextSong); // Removes the default Event.
                        audioPlayer.MediaEnded += new EventHandler(MediaEndedShuffleSong); // Adds shuffle event.
                    }
                    break;

                case true:
                    isShuffleOn = false;
                    shuffleSong.Background = Brushes.Red;
                    audioPlayer.MediaEnded -= new EventHandler(MediaEndedShuffleSong); // Removes shuffle event.
                    audioPlayer.MediaEnded += new EventHandler(MediaEndedNextSong); // Puts back the default Event.
                    break;

            }



        }

        private void NextSong(object sender, RoutedEventArgs e)
        {
            string nextSongTitle = "";
            int count = XDocument.Load("Playlist.xml").XPathSelectElements("//Song").Count(); // Find how many songs are in the playlist.
            currentSongID = Int32.Parse(FindCurrentSongID()); // Identifies current songs ID.
            if (currentSongID > count) // If current songs ID is greater then the intire playlist count.
            {
                audioPlayer.Stop();
            }
            else
            {
            nextSongTitle = FindNextSongByID(FindCurrentSongID());
            audioPlayer.Open(new Uri(path + "\\" + nextSongTitle, UriKind.Relative));
            audioPlayer.Play();
            currentSong = nextSongTitle; // Changes current song title.
                if (currentSong == null || currentSongID == count) // When the playlist runs out of songs.
                {
                    audioPlayer.Stop();
                    currentSongID = count;// To prevent the song ID from going out of bounds, 
                                           // we add the last songs ID back to it, so
                                          // that the user can go and listen to the previous song prior to this last song.
                    currentSong = songName.Text + ".mp3"; // Add back '.mp3' to continue using the full file name for later actions.
                }
                else
                {
                    songName.Text = nextSongTitle.Substring(0, currentSong.Length - 4); // Remove the '.mp3' from song name and display it.
                    currentSongID += 1; // Increase current songs ID to find the next song.
                }
                
            }
        }

        private void PreviousSong(object sender, RoutedEventArgs e)
        {
            string previousSongTitle = "";
            int count = XDocument.Load("Playlist.xml").XPathSelectElements("//Song").Count(); // To find how many songs are in the playlist
            currentSongID = Int32.Parse(FindCurrentSongID()); // Identifies current songs ID.

            if (currentSongID == 1) // If the currentSongID is equal to 1, then the player can't go back 
                                    //and change the ID's number back to 1
            {
                audioPlayer.Stop();
                currentSongID = 1;// To prevent the songId from going out of bounds, so that 
                                      // the user can listen to the next song that comes after this song.
                currentSong = songName.Text + ".mp3"; // Add back '.mp3' to continue using the full file name for later actions.
            }
            else
            {
                previousSongTitle = FindPreviousSongByID(FindCurrentSongID());
                audioPlayer.Open(new Uri(path + "\\" + previousSongTitle, UriKind.Relative));
                Console.WriteLine(path + "\\" + previousSongTitle);
                audioPlayer.Play();
                currentSong = previousSongTitle;
                if (currentSong == null) // if CurrentSong = null, then it means that there are no more songs to play.
                {
                    audioPlayer.Stop();
                    currentSongID = 1; // To prevent the songId from going out of bounds, so that 
                                       // the user can listen to the next song that comes after this song.                      
                }
                else
                {
                    songName.Text = previousSongTitle.Substring(0, currentSong.Length - 4);
                    currentSongID -= 1;
                }    
            }
        }

        private void AddSong(object sender, RoutedEventArgs e)
        {
            if (openFileDialog.ShowDialog() == true)
            {
                openFileDialog.Filter = "Media files (*.mp3;*)|*.mp3;|All files (*.*)|*.*";
                string selectedSong = openFileDialog.SafeFileName;
                System.IO.Directory.Move(System.IO.Path.GetFullPath(openFileDialog.FileName), path + "\\" + selectedSong);
                // ^ Move the selected song from original path to the 'Songs' folder.
                AddToPlaylist();
            }
        }

        private void DeleteSong(object sender, RoutedEventArgs e)
        {
            if (openFileDialog.ShowDialog() == true)
            {
                openFileDialog.Filter = "Media files (*.mp3;*)|*.mp3;|All files (*.*)|*.*";
                string selectedSong = openFileDialog.SafeFileName;
                System.IO.File.Delete(System.IO.Path.GetFullPath(openFileDialog.FileName));
                AddToPlaylist();
            }

        }


        /* Media Ended Events */

        private void MediaEndedShuffleSong(object sender, EventArgs e) // Selects a random song from playlist.
        {
            string randomSongTitle = RandomSong();
            audioPlayer.Stop();
            audioPlayer.Open(new Uri(path + "\\" + randomSongTitle, UriKind.Relative)); // Open song with randomly selected title.
            songName.Text = randomSongTitle.Substring(0, randomSongTitle.Length - 4);
            currentSong = randomSongTitle;
            audioPlayer.Play();
        }

        private void MediaEndedReplaySong(object sender, EventArgs e) // Plays the next song in playlist when the previos ends.
        {
            audioPlayer.Position = TimeSpan.Zero;
            audioPlayer.Play();
        }

        private void MediaEndedNextSong(object sender, EventArgs e) // New event, that plays next song when song ends.
        {
            XDocument doc = XDocument.Load("Playlist.xml");
            string nextSongName = "";
            nextSongName = FindNextSongByID(FindCurrentSongID());
            audioPlayer.Open(new Uri(path + "\\" + nextSongName, UriKind.Relative)); // Opens selected song.
            audioPlayer.Play();
            currentSong = nextSongName;
                if(currentSong == null) // If the playlist ends.
                {
                    audioPlayer.Stop();
                }
                else
                {
                    songName.Text = nextSongName.Substring(0, currentSong.Length - 4);
                }
        }


        /* Audio Seeker */

        private void TimerTick(object sender, EventArgs e)
        {
            if ((isOpened == true) && (audioPlayer.NaturalDuration.HasTimeSpan) && (!isUserDraggingSlider))
            {
                sliderProgress.Minimum = 0;
                sliderProgress.Maximum = audioPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliderProgress.Value = audioPlayer.Position.TotalSeconds;
                SetLength();  
            }
        }

        private void SetLength() // Sets songs maximum lenght in the UI.
        {
            lenght.Text = TimeSpan.FromSeconds(sliderProgress.Maximum).ToString(@"hh\:mm\:ss");
        }

        private void TimeUpdate()
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

        private void SliderProgressValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TimeUpdate();
        }

        private void SliderProgressDragCompleted(object sender, DragCompletedEventArgs e)
        {
            isUserDraggingSlider = false;
            audioPlayer.Position = TimeSpan.FromSeconds(sliderProgress.Value);
        }

        private void SliderProgressDragStarted(object sender, DragStartedEventArgs e)
        {
            isUserDraggingSlider = true;
        }

        private void CurrentTimeMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            reverseTime = !reverseTime;
            TimeUpdate();
        }

        /* Volume Seeker */

        private void VolumeSettings() // Controls the starting volume settings.
        {
            audioPlayer.Volume = 0.5;
            volumeSlider.Minimum = 0;
            volumeSlider.Maximum = 1;
            volumeSlider.Value = 0.5;
        }

        private void VolumeSliderProgressDragCompleted(object sender, DragCompletedEventArgs e)
        {
            isUserDraggingSlider = false;
            audioPlayer.Volume = volumeSlider.Value;
        }

        private void VolumeSliderProgressDragStarted(object sender, DragStartedEventArgs e)
        {
            isUserDraggingSlider = true;
        }

        private void VolumeSliderProgressValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            audioPlayer.Volume = volumeSlider.Value;
        }

        /* Playlist */

        private void AddToPlaylist() // Searches for song names in a 'Songs' folder and adds them to the xml file.
        {
            ClearPlaylist(); // Clears the existing Playlist.xml so that the songs, that were previously in the playlist don't get added again.
            int id = 1; // Starting XML Node id.
            XmlDocument playlist = new XmlDocument();
            playlist.Load("Playlist.xml");
            DirectoryInfo directory = new DirectoryInfo(path);//Selects the 'Songs' folder.
            FileInfo[] Files = directory.GetFiles("*.mp3"); // Searches only for .mp3 files.

            foreach (FileInfo file in Files) // For every file in 'Songs' creates a Node with Title and ID
            {
                XmlNode newNode = playlist.CreateNode(XmlNodeType.Element, "Song", "");
                XmlAttribute Title = playlist.CreateAttribute("Title");
                XmlAttribute ID = playlist.CreateAttribute("ID");

                Title.InnerText = file.Name;
                ID.InnerText = id.ToString();

                newNode.Attributes.Append(Title);
                newNode.Attributes.Append(ID);

                playlist.DocumentElement.AppendChild(newNode);
                id += 1;
            }
            playlist.Save("Playlist.xml");
        }

        private void ClearPlaylist() // Deletes all songs from 'Playlist.xml'.
        {
            XDocument xdoc = XDocument.Load("Playlist.xml"); // Opens the playlist xml
            DirectoryInfo directory = new DirectoryInfo(path);// Selects the Song folder
            FileInfo[] Files = directory.GetFiles("*.mp3"); // Searches only for .mp3 files
            foreach (FileInfo file in Files)
            {
                xdoc.XPathSelectElements("//Song").Remove();
            }         
            xdoc.Save("Playlist.xml");
        }

        private string FindCurrentSongID()
        {
            XDocument xdoc = XDocument.Load("Playlist.xml");
            string currentSongID = xdoc.Element("Playlist").Elements("Song").Where(x => (string)x.Attribute("Title") == currentSong)
                 .Select(y => (string)y.Attribute("ID").Value).FirstOrDefault();
            // ^ Returns the current played songs ID.
            return currentSongID;
        }

        private string FindNextSongByID(string id)
        {
            XDocument xdoc = XDocument.Load("Playlist.xml");
            // In order to check for the next song, we have to increase the id by one
            // but first we have to parse it to an int and then back to string
            // to search it in the 'Playlist.xml'.
            int nextSongId = Int32.Parse(id) + 1; // Increase ID by one to find the next song title.
            string nextSongIdUnparsed = nextSongId.ToString();

            // Returns the next songs title.
            string nextSongName = xdoc.Element("Playlist").Elements("Song").Where(x => (string)x.Attribute("ID") == nextSongIdUnparsed)
                 .Select(y => (string)y.Attribute("Title").Value).FirstOrDefault();

            return nextSongName;
        }

        private string FindPreviousSongByID(string id)
        {
            XDocument xdoc = XDocument.Load("Playlist.xml");

            // In order to check for the next song, we have to increase the id by one
            // but first we have to parse it to an int and then back to string
            // to search it in the 'Playlist.xml'.
            int nextSongId = Int32.Parse(id) - 1; // Decrease ID by one to find the next song title.
            string nextSongIdUnparsed = nextSongId.ToString();


            // Returns the previous songs title. 
            string nextSongName = xdoc.Element("Playlist").Elements("Song").Where(x => (string)x.Attribute("ID") == nextSongIdUnparsed)
                 .Select(y => (string)y.Attribute("Title").Value).FirstOrDefault();

            return nextSongName;
        }

        private string RandomSong() // Randomly selects next song.
        {
            XDocument xdoc = XDocument.Load("Playlist.xml");
            int count = xdoc.XPathSelectElements("//Song").Count();
            Random random = new Random();

            string randomSongId = random.Next(1, count).ToString();

            string nextSongName = xdoc.Element("Playlist").Elements("Song").Where(x => (string)x.Attribute("ID") == randomSongId)
                 .Select(y => (string)y.Attribute("Title").Value).FirstOrDefault();

            return nextSongName;

        }
    }
}
