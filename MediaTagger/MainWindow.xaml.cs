using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using TagLib;

namespace MediaTagger
{
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

        private void PlayMedia_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Play();
        }

        private void PauseMedia_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
        }

        private void StopMedia_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
        }

        private void EditTag_Click(object sender, RoutedEventArgs e)
        {
            OpenEditOverlay();
        }

        private void CloseEditOverlay_Click(object sender, RoutedEventArgs e)
        {
            CloseEditOverlay();
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

        private void OpenFile_Click(object sender, RoutedEventArgs e)
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