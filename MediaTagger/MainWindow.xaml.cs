using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using TagLib;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading; // For DispatcherTimer

namespace MediaTagger

{
    public static class MediaCommands
    {
        //The routed command from the xaml
        public static readonly RoutedUICommand Play = new RoutedUICommand("Play", "Play", typeof(MediaCommands));
        public static readonly RoutedUICommand Pause = new RoutedUICommand("Pause", "Pause", typeof(MediaCommands));
        public static readonly RoutedUICommand Stop = new RoutedUICommand("Stop", "Stop", typeof(MediaCommands));
        public static readonly RoutedUICommand Open = new RoutedUICommand("Open", "Open", typeof(MediaCommands));
        public static readonly RoutedUICommand Edit = new RoutedUICommand("Edit", "Edit", typeof(MediaCommands));

    }
    public partial class MainWindow : Window
    {

        private MediaPlayer mediaPlayer = new MediaPlayer();
        private MediaFileInfo currentMediaFile;
        private DispatcherTimer mediaProgressTimer;
        private bool isMediaPlaying = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeMediaProgressTimer();
        }
        //update main page with the info 
        private void UpdateUIWithMediaInfo()
        {
            titleLabel.Content = currentMediaFile.Title;
            artistLabel.Content = currentMediaFile.Artist;
            albumLabel.Content = currentMediaFile.Album;
            artworkImage.Source = currentMediaFile.Artwork;
            yearLabel.Content = currentMediaFile.Year;
        }
        // https://learn.microsoft.com/en-us/dotnet/api/system.windows.controls.progressbar?view=windowsdesktop-8.0#examples
        private void InitializeMediaProgressTimer()
        {
            mediaProgressTimer = new DispatcherTimer();
            mediaProgressTimer.Interval = TimeSpan.FromMilliseconds(500); // Update every half second
            mediaProgressTimer.Tick += MediaProgressTimer_Tick;
        }
        
        private void MediaProgressTimer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.NaturalDuration.HasTimeSpan && mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds > 0)
            {
                mediaPlaybackProgress.Value = (mediaPlayer.Position.TotalSeconds / mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds) * 100;
            }
        }

        private void MediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            mediaProgressTimer.Start();
        }
        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mediaPlayer.Pause();
            mediaProgressTimer.Stop(); 
            isMediaPlaying = false;
        }

        private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mediaPlayer.Stop();
            mediaProgressTimer.Stop(); 
            mediaPlaybackProgress.Value = 0;
            isMediaPlaying = false;
        }

        private void Edit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenEditOverlay();
        }

        private void Edit_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = currentMediaFile != null;
        }
        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFile(sender, e);
        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mediaPlayer.Play();
            isMediaPlaying = true;
            mediaProgressTimer.Start();

        }

        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = currentMediaFile != null && !isMediaPlaying;
        }

        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isMediaPlaying;
        }

        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayer.Position > TimeSpan.Zero;
        }

        private void CloseEditOverlay_Click(object sender, RoutedEventArgs e)
        {
            CloseEditOverlay();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e) {
            OpenFile(sender, e);
        }

        // Populates the data in the overlay then makes the edttTagsOverlay visable 
        private void OpenEditOverlay()
        {
            titleTextBox.Text = currentMediaFile.Title;
            artistTextBox.Text = currentMediaFile.Artist;
            albumTextBox.Text = currentMediaFile.Album;
            yearTextBox.Text = currentMediaFile.Year.ToString();
            // A little 'woah bud you cant modify tags and then its playing'
            if (isMediaPlaying == true)
            {
                saveTagChangesButton.Content = "Save (Will restart current song)";
            } else
            {
                saveTagChangesButton.Content = "Save";
            }

            editTagsOverlay.Visibility = Visibility.Visible;
        }

        private void CloseEditOverlay()
        {
            editTagsOverlay.Visibility = Visibility.Collapsed;
        }

        //Open files, filter only mp3's then use the filename to get info 
        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                mediaPlayer.MediaOpened += MediaPlayer_MediaOpened; 

                mediaPlayer.Open(new Uri(openFileDialog.FileName));
                currentMediaFile = new MediaFileInfo(openFileDialog.FileName);
                UpdateUIWithMediaInfo();
            }
        }

        private void SaveTagChanges_Click(object sender, RoutedEventArgs e)
        {
            //used in debuging, now handled by Icommand bindings, keeping just in case
            if (currentMediaFile == null || string.IsNullOrEmpty(currentMediaFile.FilePath))
            {
                MessageBox.Show("No file is currently loaded.");
                return;
            }

            try
            {
                if (isMediaPlaying)
                {
                    mediaPlayer.Stop();
                    isMediaPlaying = false;
                }

                mediaPlayer.Close();
                Thread.Sleep(50);// my assumption is that if it happens too fast it still doesnt wanna save and because it was open 0.0000001 seconds ago
                var file = TagLib.File.Create(currentMediaFile.FilePath);
                file.Tag.Title = titleTextBox.Text;
                file.Tag.Album = albumTextBox.Text;
                file.Tag.Performers = new[] { artistTextBox.Text };
                file.Tag.Year = uint.Parse(yearTextBox.Text);
                file.Save();

                currentMediaFile = new MediaFileInfo(currentMediaFile.FilePath);

                mediaPlayer.Open(new Uri(currentMediaFile.FilePath));

                UpdateUIWithMediaInfo();

                CloseEditOverlay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving tags: {ex.Message}");
            }
        }

        private void ExitApplication_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}