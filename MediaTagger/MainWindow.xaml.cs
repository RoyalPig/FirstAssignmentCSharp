using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using TagLib;
using System.Windows.Input;

namespace MediaTagger

{
    public static class MediaCommands
    {
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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void UpdateUIWithMediaInfo()
        {
            titleLabel.Content = currentMediaFile.Title;
            artistLabel.Content = currentMediaFile.Artist;
            albumLabel.Content = currentMediaFile.Album;
            artworkImage.Source = currentMediaFile.Artwork;
        }

        //All the command bindings from the ui
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
        }

        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = currentMediaFile != null;
        }

        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mediaPlayer.Pause();
        }

        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayer.CanPause; // Example logic to enable/disable
        }

        private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mediaPlayer.Stop();
        }

        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayer.Source != null; // Example logic to enable/disable
        }

        //Used clicks to start, kept a couple for the buttons that dont need command binding extras 
        private void CloseEditOverlay_Click(object sender, RoutedEventArgs e)
        {
            CloseEditOverlay();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e) {
            OpenFile(sender, e);
        }

        private void OpenEditOverlay()
        {
            titleTextBox.Text = currentMediaFile.Title;
            artistTextBox.Text = currentMediaFile.Artist;
            albumTextBox.Text = currentMediaFile.Album;

            editTagsOverlay.Visibility = Visibility.Visible;
        }

        private void CloseEditOverlay()
        {
            editTagsOverlay.Visibility = Visibility.Collapsed;
        }

        //
        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
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
                mediaPlayer.Close();

                var file = TagLib.File.Create(currentMediaFile.FilePath);
                file.Tag.Title = titleTextBox.Text;
                file.Tag.Album = albumTextBox.Text;
                file.Tag.Performers = new[] { artistTextBox.Text };
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