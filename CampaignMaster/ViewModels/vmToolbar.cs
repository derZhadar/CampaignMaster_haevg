using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SamCorp.WPF.Extensions;
using SamCorp.WPF.ViewModels;

namespace CampaignMaster.ViewModels {

    class vmToolbar : ViewModelBase {

        public ObservableCollection<ObjectImageSource> PaletteFloors { get; set; } = new();
        public ObservableCollection<ObjectImageSource> PaletteObjects { get; set; } = new();
        public ObservableCollection<ObjectImageSource> AdditionalPaletteObjects { get; set; } = new();
        public ObservableCollection<ObjectImageSource> CustomPaletteObjects { get; set; } = new();
        public ObservableCollection<Brush> AdditionalPaletteColors { get; set; } = new();

        public vmToolbar() {
            LoadPalettes();
        }

        private void LoadPalettes() {
            LoadAdditionalColorPalette();

            PaletteFloors.Add(null);
            LoadObjectPalette(@"Resources\Images\Floors", PaletteFloors);
            LoadObjectPalette(@"Resources\Images\Objects", PaletteObjects);
            LoadObjectPalette(@"Resources\Images\Objects\Additional", AdditionalPaletteObjects);
            LoadObjectPalette(@"Resources\Images\Objects\Custom", CustomPaletteObjects);
        }

        private static void LoadObjectPalette(string directory, ICollection<ObjectImageSource> palette) {
            if (!Directory.Exists(directory)) {
                return;
            }

            var files = new List<string>();
            files.AddRange(Directory.GetFiles(directory));
            files.ForEach(f => palette.Add(new ObjectImageSource(f)));
        }

        private void LoadAdditionalColorPalette() {
            var brushProperties = typeof(Brushes).GetProperties(BindingFlags.Static | BindingFlags.Public);
            var brushes = brushProperties.Select(prop => (SolidColorBrush)prop.GetValue(null, null)).ToList();
            AdditionalPaletteColors = new ObservableCollection<Brush>(brushes.OrderBy(b => b.Color.ColorContext).Take(119));
        }

    }

    public class ObjectImageSource {

        private const string _RegexSizePattern = "\\dx\\d";

        private static readonly Random _Random = new();
        private int _LastRandom = -1;

        private bool _HasVariations => _VariationFiles.Count != 0;
        private List<string> _VariationFiles = new();

        public BitmapImage ImageSource { get; set; }

        public string Name { get; set; }
        public string FullFileName { get; set; }
        public int ObjectWidth { get; set; } = 1;
        public int ObjectHeight { get; set; } = 1;

        public bool IsSingleSquare => ObjectWidth == 1 && ObjectHeight == 1;
        public Orientation Orientation => ObjectWidth >= ObjectHeight ? Orientation.Horizontal : Orientation.Vertical;

        public ObjectImageSource() {
        }

        public ObjectImageSource(string fileName, Rotation rotation = Rotation.Rotate0) {
            ImageSource = new BitmapImage();
            ImageSource.BeginInit();
            ImageSource.UriSource = new Uri(fileName, UriKind.Relative);
            ImageSource.Rotation = rotation;
            ImageSource.EndInit();
            ImageSource.Freeze();

            FullFileName = fileName;

            if (!Regex.IsMatch(fileName, _RegexSizePattern)) {
                return;
            }

            var match = Regex.Match(fileName, _RegexSizePattern);

            ObjectWidth = int.Parse(match.Value.Split('x')[rotation == Rotation.Rotate90 || rotation == Rotation.Rotate270 ? 1 : 0]);
            ObjectHeight = int.Parse(match.Value.Split('x')[rotation == Rotation.Rotate90 || rotation == Rotation.Rotate270 ? 0 : 1]);

            var rawFileName = Path.GetFileNameWithoutExtension(fileName);
            Name = rawFileName.Split('_')[0];
            var folder = Path.GetDirectoryName(fileName);
            var variationsFolder = Path.Combine(folder, Name);
            if (!Directory.Exists(variationsFolder)) {
                return;
            }

            _VariationFiles.Add(FullFileName);
            foreach (var variationFile in Directory.GetFiles(variationsFolder)) {
                _VariationFiles.Add(variationFile);
            }
        }

        public ObjectImageSource AsNewObjectImageSourceWithRotation(Rotation rotation) {
            return new ObjectImageSource(FullFileName, rotation);
        }

        public ObjectImageSource AsNewVariationWithRotation(Rotation rotation) {
            if (!_HasVariations) {
                return AsNewObjectImageSourceWithRotation(rotation);
            }

            var rnd = _Random.Next(0, _VariationFiles.Count);
            while (rnd == _LastRandom) {
                rnd = _Random.Next(0, _VariationFiles.Count);
            }

            _LastRandom = rnd;
            return new ObjectImageSource(_VariationFiles[rnd], rotation);
        }

        public ImageBrush AsNewImageBrush() {
            return AsNewImageBrushWithRotation(Rotation.Rotate0);
        }

        public ImageBrush AsNewImageBrushWithRotation(Rotation rotation) {
            if (FullFileName.IsNullOrEmpty()) {
                return new ImageBrush(ImageSource);
            }

            var img = new BitmapImage();
            img.BeginInit();
            img.UriSource = new Uri(FullFileName, UriKind.Relative);
            img.Rotation = rotation;
            img.EndInit();
            img.Freeze();

            return new ImageBrush(img);
        }

    }

}