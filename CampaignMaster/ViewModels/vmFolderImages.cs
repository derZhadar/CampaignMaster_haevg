using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CampaignMaster.Data;
using SamCorp.WPF.Commands;
using SamCorp.WPF.Logging;
using SamCorp.WPF.ViewModels;

namespace CampaignMaster.ViewModels {

    public class vmFolderImages : ViewModelBase {

        private const int ImageCompileWidth = 1920;
        private string DownloadsPath = "C:\\Users\\" + Environment.UserName + "\\Downloads";

        private List<string> loadedImages = new();
        private List<string> imagesInDownload = new();

        public ICommand SelectImage {
            get => new Command<ImageSource>(ChangeImage);
        }

        public ICommand SelectScratch {
            get => new Command<ImageSource>(ChangeScratch);
        }

        public ICommand SelectTell {
            get => new Command<ImageSource>(ChangeTell);
        }

        public ObservableCollection<ImageSource> Images { get; set; }

        private ObservableCollection<claMap> _Maps;

        public ObservableCollection<claMap> Maps {
            get => _Maps;
            set => SetField(ref _Maps, value);
        }

        public vmFolderImages() {
            Images = new ObservableCollection<ImageSource> {
                null, null, null, null, null, null, null, null
            };

            Maps = new ObservableCollection<claMap> {
                null, null, null, null, null, null, null, null
            };
        }

        public void LoadMaps() {
            try {
                var newMaps = new List<claMap>();
                var files = Directory.GetFiles(App.CurrentCampaign.DirectoryMaps, "*.cmm", SearchOption.AllDirectories);
                foreach (var file in files) {
                    var AFormatter = new BinaryFormatter();
                    using (var fs = File.Open(file, FileMode.Open))
                        newMaps.Add((claMap)AFormatter.Deserialize(fs));
                }

                Maps.Clear();
                Maps = new ObservableCollection<claMap>(newMaps.OrderByDescending(m => m.EditDateTime));
            } catch (Exception ex) {
                Log.Error(ex);
            }
        }

        public void LoadImages() {
            if (Images.Contains(null)) {
                // Erstmaliges laden, also alle dummys entfernen und aktuellen Zustand des Download-Orders merken
                Images.Clear();

                imagesInDownload = Directory.GetFiles(DownloadsPath, "*.png", SearchOption.AllDirectories).ToList();
            }

            var currentImagesInDownload = Directory.GetFiles(DownloadsPath, "*.png", SearchOption.AllDirectories).ToList();
            if (currentImagesInDownload.Count != imagesInDownload.Count) {
                // Neue Bilder gefunden, übertragen
                foreach (var image in currentImagesInDownload.Where(i => !imagesInDownload.Contains(i))) {
                    File.Copy(image, Path.Combine(App.CurrentCampaign.DirectoryImages, Path.GetFileName(image)));
                }

                imagesInDownload = currentImagesInDownload;
            }

            var files = Directory.GetFiles(App.CurrentCampaign.DirectoryImages, "*.*", SearchOption.AllDirectories);
            foreach (var file in files.Where(f => !loadedImages.Contains(f)))
                LoadImage(file);
        }

        private void LoadImage(string filename) {
            if (!filename.Contains("(1)")) {
                try {
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(filename, UriKind.Relative);
                    bi.DecodePixelHeight = (1080 / 1920) * ImageCompileWidth;
                    bi.DecodePixelWidth = ImageCompileWidth;
                    bi.EndInit();
                    bi.Freeze();
                    Images.Add(bi);
                } catch (Exception ex) {
                    Log.Error(ex);
                }
            }

            loadedImages.Add(filename);
        }

        private void CheckAndChangeTell(string fullfilename) {
            var fileName = Path.GetFileNameWithoutExtension(fullfilename);
            var extension = Path.GetExtension(fullfilename);
            var secondFile = Path.Combine(Path.GetDirectoryName(fullfilename), fileName + " (1)" + extension);

            if (!File.Exists(secondFile))
                return;

            try {
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(secondFile, UriKind.Relative);
                bi.DecodePixelHeight = (1080 / 1920) * ImageCompileWidth;
                bi.DecodePixelWidth = ImageCompileWidth;
                bi.EndInit();
                bi.Freeze();
                ChangeTell(bi);
            } catch (Exception ex) {
                Log.Error(ex);
            }
        }

        private void ChangeImage(ImageSource source) {
            ((vmDrawingBoard)Application.Current.MainWindow.DataContext).BackgroundImage = source;
            ((vmDrawingBoard)Application.Current.MainWindow.DataContext).ScratchImage = null;
            CheckAndChangeTell(source.ToString());
        }

        private void ChangeScratch(ImageSource source) {
            ((vmDrawingBoard)Application.Current.MainWindow.DataContext).BackgroundImage = null;
            ((vmDrawingBoard)Application.Current.MainWindow.DataContext).ScratchImage = source;
            CheckAndChangeTell(source.ToString());
        }

        private void ChangeTell(ImageSource source) {
            ((vmDrawingBoard)Application.Current.MainWindow.DataContext).TellImage = source;
        }

    }

}