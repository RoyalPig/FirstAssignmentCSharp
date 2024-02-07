using System;
using System.IO;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TagLib;

namespace MediaTagger
{

    public class MediaFileInfo
    {
        public string FilePath { get; private set; }
        public string Title { get; private set; }
        public string Artist { get; private set; }
        public string Album { get; private set; }
        public uint Year { get; private set; }
        public ImageSource Artwork { get; private set; }

        // Public function to get the tags and pass them along
        public MediaFileInfo(string filePath)
        {
            FilePath = filePath;
            LoadTags();
        }
        // Private functions logic for the public properties in the MediaFileInfo class
        private void LoadTags()
        {
            try
            {
                var file = TagLib.File.Create(FilePath);
                Title = file.Tag.Title;
                Artist = string.Join(", ", file.Tag.Performers);
                Album = file.Tag.Album;
                Year = file.Tag.Year;

                if (file.Tag.Pictures.Length > 0)
                {
                    var bin = (byte[])(file.Tag.Pictures[0].Data.Data);
                    Artwork = LoadImage(bin);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error loading media file tags: {ex.Message}");
            }
        }

        private ImageSource LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;

            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = mem;
                image.EndInit();
                image.Freeze();
            }
            return image;
        }
    }
}
