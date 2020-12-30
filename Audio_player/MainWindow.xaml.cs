using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Audio_player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// This is a test to see how to properly push into github using VS 
    public partial class MainWindow : Window
    {

        private MediaPlayer audioPlayer = new MediaPlayer();
        OpenFileDialog openFileDialog = new OpenFileDialog();
        String currentSong;
        int currentSongID;
        string path = Path.Combine(Environment.CurrentDirectory, @"Songs");
   

        private bool userIsDraggingSlider = false;
      
        private bool reverseTime = false;
        private bool isOpened = false;
        private bool isReplayOn = false;
        private bool isShuffleOn = false;
        private bool isPlayOn = true; //default is true, because when a song is picked, the song is played automaticly

        public MainWindow()
        {
            InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
            addToPlaylist();
            audioPlayer.MediaEnded += new EventHandler(mediaEndedNextSong);


        }
        /* Buttons */

        private void bttnFolder_Click(object sender, RoutedEventArgs e)
        {

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

        private void bttnPlayAndPause_Click(object sender, RoutedEventArgs e)
        {
            if (isPlayOn)
            {
                isPlayOn = false;
                audioPlayer.Pause();
                playAndPause.Content = FindResource("Pause");
            }
            else
            {
                audioPlayer.Play();
                isPlayOn = true;
                playAndPause.Content = FindResource("Play");
            }
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
                    if (isShuffleOn)
                    {
                        audioPlayer.MediaEnded -= new EventHandler(mediaEndedShuffleSong);
                        isShuffleOn = false; // if shuffle is on, weturn it off so that they both don't colide with one another
                        shuffle.Background = Brushes.LightGray;
                        audioPlayer.MediaEnded += new EventHandler(mediaEndedReplaySong);
                    }
                    else
                    {
                        audioPlayer.MediaEnded -= new EventHandler(mediaEndedNextSong);// remove the default Event
                        audioPlayer.MediaEnded += new EventHandler(mediaEndedReplaySong); // Adds the new event to MediaEnded.
                    }
                    break;

                case true:
                    isReplayOn = false;
                    repeat.Background = Brushes.LightGray;
                    audioPlayer.MediaEnded -= new EventHandler(mediaEndedReplaySong);// Removes the new event to MediaEnded so that the song isn't repeated.
                    audioPlayer.MediaEnded += new EventHandler(mediaEndedNextSong);// put back the default Event
                    break;
            }
        }

        private void bttn_shuffle(object sender, RoutedEventArgs e)
        {
            switch (isShuffleOn)
            {
                case false:
                    isShuffleOn = true;
                    shuffle.Background = Brushes.Black;
                    if (isReplayOn)
                    {
                        isReplayOn = false;
                        repeat.Background = Brushes.LightGray;
                        audioPlayer.MediaEnded -= new EventHandler(mediaEndedReplaySong); // 
                        audioPlayer.MediaEnded += new EventHandler(mediaEndedShuffleSong);
                    }
                    else
                    {
                        audioPlayer.MediaEnded -= new EventHandler(mediaEndedNextSong); // remove the default Event
                        audioPlayer.MediaEnded += new EventHandler(mediaEndedShuffleSong);
                    }
                    break;

                case true:
                    isShuffleOn = false;
                    shuffle.Background = Brushes.LightGray;
                    audioPlayer.MediaEnded -= new EventHandler(mediaEndedShuffleSong);
                    audioPlayer.MediaEnded += new EventHandler(mediaEndedNextSong); // put back the default Event
                    break;

            }



        }

        private void bttn_nextSong(object sender, RoutedEventArgs e)
        {
            string nextSongTitle = "";
            int count = XDocument.Load("Playlist.xml").XPathSelectElements("//Song").Count(); // to find how many songs are in the playlist
            Console.WriteLine(currentSong + "aa");
            currentSongID = Int32.Parse(findCurrentSongID()); // to keep track of the songs that will be after this one
            if (currentSongID > count) // since the ID count surpases the playlist count, there are no more songs to play.
            {
                audioPlayer.Stop();
            }
            else
            {
            nextSongTitle = findNextSongByID(findCurrentSongID());
            audioPlayer.Open(new Uri(path + "\\" + nextSongTitle, UriKind.Relative));
            Console.WriteLine(path + "\\" + nextSongTitle);
            audioPlayer.Play();
            currentSong = nextSongTitle;
                if (currentSong == null || currentSongID == count) // catch when the playlist has run out of songs
                {
                    audioPlayer.Stop();
                    currentSongID = count;// to prevent the songId from going out of bounds, so that when the end point has been reached
                                          // the user can listen to the next song.
                    currentSong = songName.Text + ".mp3"; // so that we can use it for later searches
                }
                else
                {
                    songName.Text = nextSongTitle.Substring(0, currentSong.Length - 4);
                    currentSongID += 1;
                }
                
            }
        }

        private void bttn_previousSong(object sender, RoutedEventArgs e)
        {
            string previousSongTitle = "";
            int count = XDocument.Load("Playlist.xml").XPathSelectElements("//Song").Count(); // to find how many songs are in the playlist
            Console.WriteLine(currentSongID);
            currentSongID = Int32.Parse(findCurrentSongID()); // to keep track of the songs that will be after this one

            if (currentSongID == 1) // if the currentSongID is equal to 1, then the player can't go back and we have to make sure that it stays as 1
            {
                audioPlayer.Stop();
                currentSongID = 1;// to prevent the songId from going out of bounds, so that when the end point has been reached
                                      // the user can listen to a previous song.
                currentSong = songName.Text + ".mp3"; // so that we can use it for later searches
                Console.WriteLine(currentSong + "bbbbbbb");

            }
            else
            {
                previousSongTitle = findPreviousSongByID(findCurrentSongID());
                audioPlayer.Open(new Uri(path + "\\" + previousSongTitle, UriKind.Relative));
                Console.WriteLine(path + "\\" + previousSongTitle);
                audioPlayer.Play();
                currentSong = previousSongTitle;
                if (currentSong == null) // catch when the playlist has run out of songs
                {
                    audioPlayer.Stop();
                    currentSongID = 1; // to prevent the songId from being null, so that when the end point has been reached
                                       // the user can listen to a next song.
                                       
                }
                else
                {
                    songName.Text = previousSongTitle.Substring(0, currentSong.Length - 4);
                    currentSongID -= 1;
                }
                
            }
        }

    


        /* Media Ended Events */

        private void mediaEndedShuffleSong(object sender, EventArgs e)
        {
            string randomSongTitle = randomSong();
            audioPlayer.Stop();
            audioPlayer.Open(new Uri(path + "\\" + randomSongTitle, UriKind.Relative));
            songName.Text = randomSongTitle.Substring(0, currentSong.Length - 4);
            currentSong = randomSongTitle;
            audioPlayer.Play();

        }


        private void mediaEndedReplaySong(object sender, EventArgs e) // New event, that replays current song.
        {
            audioPlayer.Position = TimeSpan.Zero;
            audioPlayer.Play();
        }

        private void mediaEndedNextSong(object sender, EventArgs e) // New event, that plays next song when song ends.
        {
            XDocument doc = XDocument.Load("Playlist.xml");
            string nextSongName = "";


            nextSongName = findNextSongByID(findCurrentSongID());
            Console.WriteLine(nextSongName);
            audioPlayer.Open(new Uri(path + "\\" + nextSongName, UriKind.Relative));
            audioPlayer.Play();
            currentSong = nextSongName;
                if(currentSong == null) // catch when the playlist has run out of songs
                {
                    audioPlayer.Stop();
                }
                else
                {
                    songName.Text = nextSongName.Substring(0, currentSong.Length - 4);
                }
            
 
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


        /* Playlist */

        private void addToPlaylist() // Searches for song names in a specific folder and adds them to the xml file.
        {
            clearPlaylist(); // Clears the existing Playlist.xml so that the songs, that were previously in the playlist don't get added again.
            int id = 1; // Starting ID
            XmlDocument playlist = new XmlDocument();
            playlist.Load("Playlist.xml");
            DirectoryInfo directory = new DirectoryInfo(path);//Selects the Song folder
            FileInfo[] Files = directory.GetFiles("*.mp3"); // Searches only for .mp3 files

            foreach (FileInfo file in Files)
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

        private void clearPlaylist() 
        {
            
            XDocument xdoc = XDocument.Load("Playlist.xml"); // Opens the playlist xml

            /* In this section in order to delete all songs in the playlist the file directory
           is used to go gather the song names in which these names are later used to delete them from
           XML file.*/
            DirectoryInfo directory = new DirectoryInfo(path);//Selects the Song folder
            FileInfo[] Files = directory.GetFiles("*.mp3"); // Searches only for .mp3 files


            foreach (FileInfo file in Files)
            {
                xdoc.Element("Playlist").Elements("Song").Where(x => (string)x.Attribute("Title") == file.Name)
                 .Remove();
            }         
            xdoc.Save("Playlist.xml");

        }

        private string findCurrentSongID()
        {
            XDocument xdoc = XDocument.Load("Playlist.xml");
            // Returns the current played songs ID
            string currentSongID = xdoc.Element("Playlist").Elements("Song").Where(x => (string)x.Attribute("Title") == currentSong)
                 .Select(y => (string)y.Attribute("ID").Value).FirstOrDefault();

            return currentSongID;
        }

        private string findNextSongByID(string id)
        {
            XDocument xdoc = XDocument.Load("Playlist.xml");

            // In order to check for the next song, we have to increase the id by one
            // but first we have to parse it to an int and then back to string
            int nextSongId = Int32.Parse(id) + 1;
            string nextSongIdUnparsed = nextSongId.ToString();


            // Returns the current played songs ID
            string nextSongName = xdoc.Element("Playlist").Elements("Song").Where(x => (string)x.Attribute("ID") == nextSongIdUnparsed)
                 .Select(y => (string)y.Attribute("Title").Value).FirstOrDefault();

            return nextSongName;
        }

        private string findPreviousSongByID(string id)
        {
            XDocument xdoc = XDocument.Load("Playlist.xml");

            // In order to check for the previous song, we have to deincrease the id by one
            // but first we have to parse it to an int and then back to string
            int nextSongId = Int32.Parse(id) - 1;
            string nextSongIdUnparsed = nextSongId.ToString();


            // Returns the current played songs ID
            string nextSongName = xdoc.Element("Playlist").Elements("Song").Where(x => (string)x.Attribute("ID") == nextSongIdUnparsed)
                 .Select(y => (string)y.Attribute("Title").Value).FirstOrDefault();

            return nextSongName;
        }

        private string randomSong()
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
