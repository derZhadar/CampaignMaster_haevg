using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using CampaignMaster.Data;
using CampaignMaster.Misc;
using CampaignMaster.Models;
using SamCorp.WPF.Commands;
using SamCorp.WPF.Extensions;
using SamCorp.WPF.ViewModels;

namespace CampaignMaster.ViewModels {

    public class vmDrawingBoard : ViewModelBase {

        #region Variablen

        private claMap currentMap;

        private double penSize = 5;

        private DrawMode drawMode;

        private Point mousePosition;

        private static Rect gridSize = new(0, 0, 50, 50);
        private static double gridOpacity = 0.2;
        private static Brush gridBrush = Brushes.Black;

        private MatrixTransform boardTransform;

        private ImageSource backgroundImage;
        private ImageSource scratchImage;
        private ImageSource tellImage;

        private Visibility worldMapVisible = Visibility.Hidden;
        private Visibility showTellVisible = Visibility.Hidden;
        private bool _CalendarVisible;
        private double drawingBoardOpacity = 1;

        public double PlayerMapTranslateY {
            get => App.Settings.PlayerMapTranslateY;
            set => SetField(ref App.Settings.PlayerMapTranslateY, value);
        }

        public double PlayerMapTranslateX {
            get => App.Settings.PlayerMapTranslateX;
            set => SetField(ref App.Settings.PlayerMapTranslateX, value);
        }

        public double PlayerMapScaleY {
            get => App.Settings.PlayerMapScaleY;
            set => SetField(ref App.Settings.PlayerMapScaleY, value);
        }

        public double PlayerMapScaleX {
            get => App.Settings.PlayerMapScaleX;
            set => SetField(ref App.Settings.PlayerMapScaleX, value);
        }

        public ICommand CommandChangePlayerMapScale => new Command<Button>((button) => {
            if (button.Tag is not string parameter) {
                return;
            }

            if (parameter.First().Equals('X')) {
                if (parameter.Last().Equals('I')) {
                    PlayerMapScaleX += 0.001;
                } else {
                    PlayerMapScaleX -= 0.001;
                }
            } else {
                if (parameter.Last().Equals('I')) {
                    PlayerMapScaleY += 0.001;
                } else {
                    PlayerMapScaleY -= 0.001;
                }
            }
        });

        public ICommand CommandChangePlayerMapTranslation => new Command<Button>((button) => {
            if (button.Tag is not string parameter) {
                return;
            }

            if (parameter.First().Equals('X')) {
                if (parameter.Last().Equals('I')) {
                    PlayerMapTranslateX += 1;
                } else {
                    PlayerMapTranslateX -= 1;
                }
            } else {
                if (parameter.Last().Equals('I')) {
                    PlayerMapTranslateY += 1;
                } else {
                    PlayerMapTranslateY -= 1;
                }
            }
        });

        public double DrawingBoardOpacity {
            get => drawingBoardOpacity;
            set => SetField(ref drawingBoardOpacity, value);
        }

        public ICommand CommandToggleCalendar => new Command(() => {
            CalendarVisible = !CalendarVisible;

            if (!CalendarVisible) {
                App.SaveCampaign();
            }
        });

        public bool CalendarVisible {
            get => _CalendarVisible;
            set => SetField(ref _CalendarVisible, value);
        }

        public Visibility ShowTellVisible {
            get => showTellVisible;
            set => SetField(ref showTellVisible, value);
        }

        public Visibility WorldMapVisible {
            get => worldMapVisible;
            set => SetField(ref worldMapVisible, value);
        }

        public int PenSizeIndex = 2;
        private double[] penSizeList = { 1, 2, 5, 10, 25, 50 };

        public double PenSize {
            get => penSize;
            set {
                if (PenSizeIndex + value < penSizeList.Length && PenSizeIndex + value >= 0) {
                    PenSizeIndex += (int)value;
                    SetField(ref penSize, penSizeList[PenSizeIndex]);
                }
            }
        }

        public double PenSizeRaw {
            get => penSize;
            set {
                if (!penSizeList.Contains(value)) {
                    return;
                }

                PenSizeIndex = penSizeList.ToList().IndexOf(value);

                SetField(ref penSize, value);
                RaisePropertyChanged(nameof(PenSize));
            }
        }

        public DrawMode DrawMode {
            get => drawMode;
            set => SetField(ref drawMode, value);
        }

        public StrokeCollection Strokes { get; set; }
        public StrokeCollection Scratches { get; set; }
        public ObservableCollection<Shape> Shapes { get; set; }
        public ObservableCollection<Shape> ScratchShapes { get; set; }
        public ObservableCollection<Shape> ShapesCloned { get; set; }
        public ObservableCollection<Shape> ScratchShapesCloned { get; set; }

        public Point MousePosition {
            get => mousePosition;
            set => SetField(ref mousePosition, value);
        }

        public static Rect GridSize {
            get => gridSize;
            set {
                gridSize = value;
                RaisePropertyChangedStatic();
            }
        }

        public static double GridWidthHalf => GridSize.Width / 2;
        public static double GridHeightHalf => GridSize.Height / 2;

        public static double GridOpacity {
            get => gridOpacity;
            set {
                gridOpacity = value;
                RaisePropertyChangedStatic();
                RaisePropertyChangedStatic(nameof(GridOpacityPlayer));
            }
        }

        public static double GridOpacityPlayer => GridOpacity <= 0.2 ? 0 : GridOpacity;

        public static Brush GridBrush {
            get => gridBrush;
            set {
                gridBrush = value;
                RaisePropertyChangedStatic();
            }
        }

        public ImageSource BackgroundImage {
            get => backgroundImage;
            set => SetField(ref backgroundImage, value);
        }

        public ImageSource ScratchImage {
            get => scratchImage;
            set => SetField(ref scratchImage, value);
        }

        public ImageSource TellImage {
            get => tellImage;
            set => SetField(ref tellImage, value);
        }

        public MatrixTransform BoardTransformReset { get; set; }
        public Point ScaleTransformCenter { get; set; }

        public MatrixTransform BoardTransform {
            get => boardTransform;
            set {
                if (value != boardTransform)
                    SetField(ref boardTransform, value);
            }
        }

        public ObservableCollection<mdlCharacter> CombatOrder => App.CurrentCampaign.CombatOrder;

        public mdlCharacter ActiveCombatant => App.CurrentCampaign?.ActiveCombatant;

        public static double PaperWidth => 1700;

        //public double SecondScreenWidth { get => SystemParameters.; }
        public static double PaperHeight => 960; //(1080 / 1920) * paperWidth; //1050;

        public static event PropertyChangedEventHandler StaticPropertyChanged;

        #endregion

        private static void RaisePropertyChangedStatic([CallerMemberName] string propertyName = "") {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        public vmDrawingBoard() {
            Strokes = new StrokeCollection();
            (Strokes as INotifyCollectionChanged).CollectionChanged += Strokes_CollectionChanged;
            Scratches = new StrokeCollection();
            (Scratches as INotifyCollectionChanged).CollectionChanged += Scratches_CollectionChanged;
            Shapes = new ObservableCollection<Shape>();
            ScratchShapes = new ObservableCollection<Shape>();
            ShapesCloned = new ObservableCollection<Shape>();
            ScratchShapesCloned = new ObservableCollection<Shape>();
            BoardTransform = new MatrixTransform();
            BoardTransformReset = new MatrixTransform();

            if (App.CurrentCampaign == null) {
                return;
            }

            App.CurrentCampaign.PropertyChanged += CurrentCampaignPropertyChanged;
        }

        private void CurrentCampaignPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName.Equals(nameof(mdlCampaign.ActiveCombatant))) {
                RaisePropertyChanged(nameof(ActiveCombatant));
            }
        }

        #region Misc

        public void AddPolygon(Polygon poly) {
            var tt = new TranslateTransform();
            var st = new ScaleTransform();
            var rt = new RotateTransform();

            var tg = new TransformGroup();
            tg.Children.Add(tt);
            tg.Children.Add(st);
            tg.Children.Add(rt);

            poly.RenderTransform = tg;

            if (DrawMode is DrawMode.SCRATCH or DrawMode.SCRATCH_AREA) {
                ScratchShapes.Add(poly);
            } else {
                Shapes.Add(poly);
            }

            var clone = poly.Clone();
            clone.DataContext = poly;
            clone.SetBinding(Polygon.RenderTransformProperty, "RenderTransform");
            if (DrawMode is DrawMode.SCRATCH or DrawMode.SCRATCH_AREA) {
                ScratchShapesCloned.Add(clone);
            } else {
                ShapesCloned.Add(clone);
            }
        }

        public void DeletePolygon(Polygon poly) {
            if (DrawMode is DrawMode.SCRATCH or DrawMode.SCRATCH_AREA) {
                ScratchShapes.Remove(poly);
            } else {
                Shapes.Remove(poly);
            }

            var shapeToRemove = GetClonedPolygon(poly);
            if (shapeToRemove != null)
                if (DrawMode is DrawMode.SCRATCH or DrawMode.SCRATCH_AREA) {
                    ScratchShapesCloned.Remove(shapeToRemove);
                } else {
                    ShapesCloned.Remove(shapeToRemove);
                }
        }

        public void IncreasePolygonZIndex(Polygon poly) {
            Panel.SetZIndex(poly, Panel.GetZIndex(poly) + 1);
        }

        public void DecreasePolygonZIndex(Polygon poly) {
            Panel.SetZIndex(poly, Panel.GetZIndex(poly) - 1);
        }

        private Polygon GetClonedPolygon(Polygon poly) {
            if (DrawMode is DrawMode.SCRATCH or DrawMode.SCRATCH_AREA) {
                return (from shape in ScratchShapesCloned where shape.Tag == poly select shape as Polygon).FirstOrDefault();
            }

            return (from shape in ShapesCloned where shape.Tag == poly select shape as Polygon).FirstOrDefault();
        }

        public void ResetContext() {
            Strokes.Clear();
            Scratches.Clear();
            Shapes.Clear();
            ShapesCloned.Clear();
            BackgroundImage = null;
            ScratchImage = null;
            TellImage = null;
        }

        public void SaveToMap(byte[] preview = null) {
            if (currentMap == null) {
                currentMap = new claMap();
                currentMap.Name = Regex.Replace(App.CurrentCampaign.Name, @"\s+", "_") + "_" + DateTime.Now.ToString("yyyyMMdd_hhmmss");
            }

            //Vorschaubild hinzufügen
            currentMap.PreviewImageByte = preview;

            // Hintergrund speichern
            currentMap.BackgroundImage = BackgroundImage == null ? null : XamlWriter.Save(BackgroundImage);
            currentMap.ScratchImage = ScratchImage == null ? null : XamlWriter.Save(ScratchImage);

            // Striche speichern
            using (var ms = new MemoryStream()) {
                Strokes.Save(ms);
                currentMap.Strokes = ms.ToArray();
            }

            // Scratches speichern
            using (var ms = new MemoryStream()) {
                Scratches.Save(ms);
                currentMap.Scratches = ms.ToArray();
            }

            // Formen speichern
            Shapes.ToList().ForEach((s) => currentMap.Shapes.Add(XamlWriter.Save(s)));
            ShapesCloned.ToList().ForEach((sc) => currentMap.ShapesCloned.Add(XamlWriter.Save(sc)));

            currentMap.EditDateTime = DateTime.Now;

            // Speichern
            var AFormatter = new BinaryFormatter();
            using (var fs = File.Open(System.IO.Path.Combine(App.CurrentCampaign.DirectoryMaps, $@"{currentMap.Name}.cmm"), FileMode.Create))
                AFormatter.Serialize(fs, currentMap);
        }

        public void LoadFromFile(string filename) {
            var AFormatter = new BinaryFormatter();
            using (var fs = File.Open(filename, FileMode.Open))
                LoadFromMap((claMap)AFormatter.Deserialize(fs));
        }

        public void LoadFromMap(claMap map) {
            ResetContext();

            currentMap = map;

            // Hintergrund laden
            BitmapImage source;
            if (currentMap.BackgroundImage != null) {
                using (var stringReader = new StringReader(currentMap.BackgroundImage)) {
                    using (var xmlReader = XmlReader.Create(stringReader)) {
                        source = (BitmapImage)XamlReader.Load(xmlReader);
                        var bi = new BitmapImage();
                        bi.BeginInit();
                        bi.UriSource = new Uri(source.UriSource.OriginalString, UriKind.Relative);
                        bi.DecodePixelHeight = (int)PaperHeight;
                        bi.DecodePixelWidth = (int)PaperWidth;
                        bi.EndInit();
                        bi.Freeze();
                        BackgroundImage = bi;
                    }
                }
            }


            // Scratch-Image laden
            if (currentMap.ScratchImage != null) {
                using (var stringReader = new StringReader(currentMap.ScratchImage)) {
                    using (var xmlReader = XmlReader.Create(stringReader)) {
                        source = (BitmapImage)XamlReader.Load(xmlReader);
                        var bi = new BitmapImage();
                        bi.BeginInit();
                        bi.UriSource = new Uri(source.UriSource.OriginalString, UriKind.Relative);
                        bi.DecodePixelHeight = (int)PaperHeight;
                        bi.DecodePixelWidth = (int)PaperWidth;
                        bi.EndInit();
                        bi.Freeze();
                        ScratchImage = bi;
                    }
                }
            }

            // alle Striche laden
            if (currentMap.Strokes != null) {
                using (var ms = new MemoryStream(currentMap.Strokes))
                    new StrokeCollection(ms).ToList().ForEach((s) => Strokes.Add(s));
            }

            // alle Scratches laden
            if (currentMap.Scratches != null) {
                using (var ms = new MemoryStream(currentMap.Scratches))
                    new StrokeCollection(ms).ToList().ForEach((s) => Scratches.Add(s));
            }

            // Formen laden
            if (currentMap.Shapes != null) {
                var applicationDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var pc = new ParserContext {
                    BaseUri = new Uri(applicationDirectory + "\\", UriKind.Absolute)
                };

                foreach (var poly in currentMap.Shapes.Select(str => (Polygon)XamlReader.Parse(str, pc))) {
                    AddPolygon(poly);
                }
            }
        }

        #endregion

        #region Events

        private void Strokes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            Strokes = (StrokeCollection)sender;
        }

        private void Scratches_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            Scratches = (StrokeCollection)sender;
        }

        #endregion

    }

}