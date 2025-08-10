using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CampaignMaster.Data;
using CampaignMaster.Misc;
using CampaignMaster.ViewModels;
using SamCorp.WPF.Alerts;
using SamCorp.WPF.Extensions;
using SamCorp.WPF.Logging;

namespace CampaignMaster.Windows {

    public partial class wndCampaignMaster : Window {

        private bool _IsSession;
        private bool needMapReload = true;

        private bool stylusActive;

        /// <summary>
        /// Helfer um den DataContext einfacher als vmDrawingBoard zu verwenden
        /// </summary>
        public vmDrawingBoard DrawingContext => DataContext as vmDrawingBoard;

        private double _ObjectGridWidth => vmDrawingBoard.GridSize.Width * (DrawingContext.DrawMode is DrawMode.ROOM or DrawMode.ROOM_WILDSHAPE ? 1 : ToolbarInk.ObjectSizeMultiplier);
        private double _ObjectGridHeight => vmDrawingBoard.GridSize.Height * (DrawingContext.DrawMode is DrawMode.ROOM or DrawMode.ROOM_WILDSHAPE ? 1 : ToolbarInk.ObjectSizeMultiplier);

        private wndPlayerMap _PlayerMap;

        #region Visualisierung des entstehenden Polygons im DrawMode 'POLYGON'

        private Polyline polyline;
        private PointCollection polylinePoints;
        private Ellipse polyStart;

        private bool drawPoly = false;

        private Polygon selectedPoly;
        private Brush selectedPolyFill;
        private Brush selectedPolyStroke;

        public Polygon SelectedPoly {
            get => selectedPoly;
            set {
                selectedPoly = value;
                PolygonContextMenu.Visibility = selectedPoly != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private Point start;
        private Point origin;

        private Point startContext;
        private Point originContext;

        #endregion

        public wndCampaignMaster() {
            InitializeComponent();

            if (Environment.MachineName == "PCMJ") {
                inkDrawingBoard.PreviewMouseLeftButtonDown += InkDrawingBoard_PreviewLeftMouseDown;
            }
        }

        public void Initialize(bool live) {
            ((vmFolderImages)viewFolderImages.DataContext).LoadImages();
            ((vmFolderImages)viewFolderImages.DataContext).LoadMaps();
            //((vmMapBrowser)viewMaps.DataContext).LoadMaps();
            _IsSession = live;
            needMapReload = false;

            if (_IsSession) {
                _PlayerMap = new wndPlayerMap {
                    DataContext = DrawingContext //DataContext weitergeben damit die Visualisierung übertragen wird
                };
                _PlayerMap.Show();

                ResetMap();

                this.Activate();
            }

            ToggleToolbarVisibility();

            brdDisable.Visibility = Visibility.Hidden;

            Alert.FadeInfo("Campaign loaded", App.CurrentCampaign.Name + (_IsSession ? " (session)" : " (edit)"));
        }

        private void ToggleToolbarVisibility() {
            var newVisibility = ToolbarInk.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;

            ToolbarInk.Visibility = newVisibility;
            ToolbarGrid.Visibility = newVisibility;
            ToolbarNameGenerator.Visibility = newVisibility;
            ToolbarMapGenerator.Visibility = newVisibility;
            ToolbarCharacters.Visibility = newVisibility;
            ToolbarAudio.Visibility = newVisibility;
            btnBattle.Visibility = newVisibility;
            btnCalendar.Visibility = newVisibility;
            btnSettings.Visibility = newVisibility;
            brdGrid.Visibility = newVisibility;
            //btnCampaign.Visibility = newVisibility;
        }

        #region Misc

        /// <summary>
        /// Fügt der Karte ein Polygon hinzu inklusive aller Events zum manipulieren.
        /// </summary>
        private void AddPolygon(Polygon poly, bool colorize = true) {
            if (colorize) {
                //poly.StrokeThickness = 2;
                //poly.Stroke = new SolidColorBrush(inkDrawingBoard.DefaultDrawingAttributes.Color);
                poly.Fill = new SolidColorBrush(inkDrawingBoard.DefaultDrawingAttributes.Color);
            }

            DrawingContext.AddPolygon(poly);

            poly.MouseDown += TmpPolygon_MouseDown;
            poly.MouseMove += TmpPolygon_MouseMove;
            poly.MouseUp += TmpPolygon_MouseUp;
        }

        private void ChangePenSize(bool increase = false) {
            //DrawingContext.PenSize = increase ? DrawingContext.PenSizeIndex + 1 : DrawingContext.PenSizeIndex - 1;
        }

        /// <summary>
        /// Gibt den Grid-Index auf der X-Achse zurück, ausgehend von der übergebenen X-Koordinate.
        /// </summary>
        private double GetGridXIndex(double x) {
            return Math.Truncate(x / vmDrawingBoard.GridSize.Width);
        }

        /// <summary>
        /// Gibt den Grid-Index auf der Y-Achse zurück, ausgehend von der übergebenen Y-Koordinate.
        /// </summary>
        private double GetGridYIndex(double y) {
            return Math.Truncate(y / vmDrawingBoard.GridSize.Height);
        }

        #endregion

        #region Karten-Management

        private void SaveMap() {
            ToggleToolbarVisibility();
            var transform = DrawingContext.BoardTransform.Matrix;
            DrawingContext.BoardTransform.Matrix = DrawingContext.BoardTransformReset.Matrix;

            var rtb = new RenderTargetBitmap((int)2400, (int)1600, 96d, 96d, PixelFormats.Default);
            rtb.Render(gridContent);
            var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            byte[] preview;
            using (var ms = new MemoryStream()) {
                encoder.Save(ms);
                preview = ms.ToArray();
            }

            //using (FileStream fs = File.Open(@"test.jpg", FileMode.Create))
            //    encoder.Save(fs);

            DrawingContext.SaveToMap(preview);

            needMapReload = true;

            DrawingContext.BoardTransform.Matrix = transform;
            ToggleToolbarVisibility();

            Alert.FadeInfo("Map Saved!");
        }

        public void LoadMap(claMap map) {
            DrawingContext.LoadFromMap(map);

            foreach (Polygon poly in DrawingContext.Shapes) {
                poly.MouseDown += TmpPolygon_MouseDown;
                poly.MouseMove += TmpPolygon_MouseMove;
                poly.MouseUp += TmpPolygon_MouseUp;
            }
        }

        /// <summary>
        /// Setzt die aktuele Karte zurück um eine neue Karte anzufangen
        /// </summary>
        private void ResetMap() {
            var newContext = new vmDrawingBoard {
                DrawMode = DrawingContext.DrawMode,
                PenSizeRaw = DrawingContext.PenSize
            };
            DataContext = newContext;
            if (_PlayerMap != null)
                _PlayerMap.DataContext = DataContext;

            var p1 = new StylusPoint(0, 0);
            var p2 = new StylusPoint(0, inkDrawingBoard.ActualHeight);
            var p3 = new StylusPoint(inkDrawingBoard.ActualWidth, inkDrawingBoard.ActualHeight);
            var p4 = new StylusPoint(inkDrawingBoard.ActualWidth, 0);
            var s = new Stroke(new StylusPointCollection() { p1, p2, p3, p4, p1 }) {
                DrawingAttributes = {
                    Width = 1,
                    Height = 1
                }
            };
            DrawingContext.Scratches.Add(s);
        }

        #endregion

        #region Events

        protected override void OnPreviewMouseMove(MouseEventArgs e) {
            base.OnPreviewMouseMove(e);

            //Wenn gerade ein neues Polygon gezeichnet wird,
            //müssen dem Zeichner entsprechende Hilfslinien angezeigt werden
            if (drawPoly) {
                polyline.Points = polylinePoints.Clone();
                polyline.Points.Add(e.GetPosition(inkDrawingBoard));
            }

            //DrawingContext.MousePosition = e.GetPosition(grdDrawingBoard);

            //if (grdInk.IsMouseCaptured)
            //{
            //    Vector v = start - e.GetPosition(grdInk);
            //    ((vm)DataContext).TranslateX = origin.X - v.X;
            //    if (((vm)DataContext).ScaleY > -99) ((vm)DataContext).TranslateY = origin.Y - v.Y;
            //}
        }

        #region Polygon

        /// <summary>
        /// De-Selektiert das selektierte Polygon
        /// </summary>
        private void TmpPolygon_MouseUp(object sender, MouseButtonEventArgs e) {
            if (sender is Polygon) {
                var p = sender as Polygon;
                if (p.IsMouseCaptured) {
                    if (DrawingContext.DrawMode == DrawMode.OBJECT_SNAP) {
                        //Polygon am Grid ausrichten
                        var x = GetGridXIndex(((p.RenderTransform as TransformGroup).Children[0] as TranslateTransform).X);
                        var y = GetGridYIndex(((p.RenderTransform as TransformGroup).Children[0] as TranslateTransform).Y);

                        ((p.RenderTransform as TransformGroup).Children[0] as TranslateTransform).X = x * vmDrawingBoard.GridSize.Width;
                        ((p.RenderTransform as TransformGroup).Children[0] as TranslateTransform).Y = y * vmDrawingBoard.GridSize.Height;
                    }

                    //Farben des Polygons zurücksetzen
                    //p.Stroke = selectedPolyStroke;
                    //p.Fill = selectedPolyFill;
                    p.ReleaseMouseCapture();
                }
            }
        }

        /// <summary>
        /// Bewegt das Seektierte Polygon
        /// </summary>
        private void TmpPolygon_MouseMove(object sender, MouseEventArgs e) {
            if (sender is Polygon) {
                var p = sender as Polygon;
                if (p.IsMouseCaptured) {
                    var v = start - e.GetPosition(inkDrawingBoard);
                    ((p.RenderTransform as TransformGroup).Children[0] as TranslateTransform).X = origin.X - v.X;
                    ((p.RenderTransform as TransformGroup).Children[0] as TranslateTransform).Y = origin.Y - v.Y;

                    var vContext = startContext - e.GetPosition(gridContent);
                    (PolygonContextMenu.RenderTransform as TranslateTransform).X = originContext.X - vContext.X;
                    (PolygonContextMenu.RenderTransform as TranslateTransform).Y = originContext.Y - vContext.Y;
                }
            }
        }

        /// <summary>
        /// Selektiert das 'an-ge-klickte' Polygon
        /// </summary>
        private void TmpPolygon_MouseDown(object sender, MouseButtonEventArgs e) {
            if (sender is Polygon && (DrawingContext.DrawMode == DrawMode.OBJECT_SELECT || e.ChangedButton == MouseButton.Right)) {
                var p = sender as Polygon;

                //if(selectedPoly != null && selectedPoly != p)
                //{
                //    //Farben des Polygons zurücksetzen
                //    selectedPoly.Stroke = selectedPolyStroke;
                //    selectedPoly.Fill = selectedPolyFill;
                //    selectedPoly = null;
                //}

                //Polygonfarbe ändern um die Selektion zu kennzeichnen
                //und Ausgangspunkt für der Verschiebung merken
                SelectedPoly = p;
                selectedPolyFill = p.Fill;
                selectedPolyStroke = p.Stroke;
                p.Stroke = Brushes.DodgerBlue;
                //Sp.Fill = Brushes.LightBlue;
                start = e.GetPosition(inkDrawingBoard);
                origin = new Point(((p.RenderTransform as TransformGroup).Children[0] as TranslateTransform).X, ((p.RenderTransform as TransformGroup).Children[0] as TranslateTransform).Y);

                startContext = e.GetPosition(gridContent);
                (PolygonContextMenu.RenderTransform as TranslateTransform).X = startContext.X - 130;
                (PolygonContextMenu.RenderTransform as TranslateTransform).Y = startContext.Y - 130;
                originContext = new Point((PolygonContextMenu.RenderTransform as TranslateTransform).X, (PolygonContextMenu.RenderTransform as TranslateTransform).Y);
                p.CaptureMouse();
            }
        }

        #endregion

        #region InkCanvas

        #region Misc

        private void InkDrawingBoard_PreviewLeftMouseDown(object sender, MouseEventArgs e) {
            if (_MouseGestureActive) {
                return;
            }

            try {
                if (selectedPoly != null) {
                    //Farben des Polygons zurücksetzen
                    selectedPoly.Stroke = selectedPolyStroke;
                    selectedPoly.Fill = selectedPolyFill;
                    SelectedPoly = null;
                }

                //Mouse
                if (DrawingContext.DrawMode == DrawMode.POLYGON || DrawingContext.DrawMode == DrawMode.SCRATCH_AREA) {
                    // Polygon zeichnen
                    ((InkCanvas)sender).EditingMode = InkCanvasEditingMode.None;
                    if (e.OriginalSource == polyStart)
                        EndPolygon(); // Startpunkt wurde getroffen, also Polygon abschließen
                    else if (!(e.OriginalSource is Polygon) || drawPoly)
                        DrawPolygon(e.GetPosition(inkDrawingBoard)); // Startpunkt nicht getroffen also weiter zeichnen
                } else if (!(e.OriginalSource is Polygon)) {
                    // Es wurde kein vorhandenes Polygon angeklickt, also je nach DrawMode zeichnen erlauben
                    stylusActive = true;
                    ((InkCanvas)sender).EditingMode = InkCanvasEditingMode.Ink;

                    //switch (DrawingContext.DrawMode) {
                    //    case DrawMode.FREE:
                    //    case DrawMode.LINE:
                    //    case DrawMode.SNAP:
                    //    case DrawMode.SCRATCH:
                    //    case DrawMode.SHAPE:
                    //    case DrawMode.ROOM:
                    //    case DrawMode.OBJECT_ADD:
                    //    case DrawMode.OBJECT_SNAP:
                    //        stylusActive = true;
                    //        ((InkCanvas)sender).EditingMode = InkCanvasEditingMode.Ink;
                    //        break;
                    //}
                }
            } catch (Exception ex) {
                Alert.FadeError(ex);
            }
        }

        /// <summary>
        /// Ein Input-Down-Event. Behandelt Stylus anders als Touch
        /// </summary>
        private void InkDrawingBoard_PreviewStylusDown(object sender, StylusDownEventArgs e) {
            try {
                if (selectedPoly != null) {
                    //Farben des Polygons zurücksetzen
                    selectedPoly.Stroke = selectedPolyStroke;
                    selectedPoly.Fill = selectedPolyFill;
                    SelectedPoly = null;
                }

                //Stylus
                if (e.StylusDevice.TabletDevice.Type == TabletDeviceType.Stylus) {
                    if (DrawingContext.DrawMode == DrawMode.POLYGON || DrawingContext.DrawMode == DrawMode.SCRATCH_AREA) {
                        // Polygon zeichnen
                        ((InkCanvas)sender).EditingMode = InkCanvasEditingMode.None;
                        if (e.OriginalSource == polyStart)
                            EndPolygon(); // Startpunkt wurde getroffen, also Polygon abschließen
                        else if (!(e.OriginalSource is Polygon) || drawPoly)
                            DrawPolygon(e.GetPosition(inkDrawingBoard)); // Startpunkt nicht getroffen also weiter zeichnen
                    } else if (DrawingContext.DrawMode != DrawMode.OBJECT_SELECT && e.StylusDevice.StylusButtons[1].StylusButtonState == StylusButtonState.Up) {
                        // Es wurde kein vorhandenes Polygon angeklickt, also je nach DrawMode zeichnen erauben
                        stylusActive = true;
                        ((InkCanvas)sender).EditingMode = InkCanvasEditingMode.Ink;
                        //switch (DrawingContext.DrawMode) {
                        //    case DrawMode.FREE:
                        //    case DrawMode.LINE:
                        //    case DrawMode.SNAP:
                        //    case DrawMode.SCRATCH:
                        //    case DrawMode.SHAPE:
                        //    case DrawMode.ROOM:
                        //    case DrawMode.OBJECT_ADD:
                        //    case DrawMode.OBJECT_SNAP:
                        //        stylusActive = true;
                        //        ((InkCanvas)sender).EditingMode = InkCanvasEditingMode.Ink;
                        //        break;
                        //}
                    }
                } // Touch
                else if (e.StylusDevice.TabletDevice.Type == TabletDeviceType.Touch && !stylusActive)
                    ((InkCanvas)sender).EditingMode = InkCanvasEditingMode.GestureOnly; // Stylus außer Reichweite, also Gesten erlauben
            } catch (Exception ex) {
                Alert.FadeError(ex);
            }
        }

        private void InkDrawingBoard_PreviewStylusOutOfRange(object sender, StylusEventArgs e) {
            try {
                //Stylus nicht mehr in Reichweite also das Zeichnen deaktivieren
                stylusActive = false;
                ((InkCanvas)sender).EditingMode = InkCanvasEditingMode.None;
            } catch (Exception ex) {
                Alert.FadeError(ex);
            }
        }

        private bool _MouseGestureActive;

        private void InkDrawingBoard_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            try {
                //Um Gesten auch mit dem Stift ausführen zu können muss die Rechte Maustaste gedrückt sein (Stift hat in der Regel eine rechte Maustaste)
                ((InkCanvas)sender).EditingMode = InkCanvasEditingMode.GestureOnly;
                _MouseGestureActive = true;
            } catch (Exception ex) {
                Alert.FadeError(ex);
            }
        }

        private void InkDrawingBoard_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            try {
                ((InkCanvas)sender).EditingMode = InkCanvasEditingMode.None;
                _MouseGestureActive = false;
            } catch (Exception ex) {
                Alert.FadeError(ex);
            }
        }

        #endregion

        #region Gesture Handling

        private void InkDrawingBoard_Gesture(object sender, InkCanvasGestureEventArgs e) {
            try {
                if (DrawingContext.DrawMode == DrawMode.PROP_ADD) {
                    DrawingContext.DrawMode = DrawMode.PROP_SELECT;
                    return;
                }

                var gestureResults = e.GetGestureRecognitionResults();
                // erstes Resultat überprüfen, nur fortfahren wenn eine sehr sichere übereinstimmung gefunden wurde
                if (gestureResults[0].RecognitionConfidence <= RecognitionConfidence.Intermediate) {
                    var reset = DrawingContext.BoardTransformReset.Matrix;

                    switch (gestureResults[0].ApplicationGesture) {
                        // Show World
                        case ApplicationGesture.ChevronUp:
                            DrawingContext.BoardTransform.Matrix = reset;
                            if (DrawingContext.WorldMapVisible == Visibility.Hidden) {
                                DrawingContext.WorldMapVisible = Visibility.Visible;
                                DrawingContext.ShowTellVisible = Visibility.Hidden;
                                DrawingContext.DrawingBoardOpacity = 0;
                            } else {
                                DrawingContext.WorldMapVisible = Visibility.Hidden;
                                DrawingContext.DrawingBoardOpacity = 1;
                            }

                            break;
                        // Show & Tell
                        case ApplicationGesture.ChevronDown:
                            DrawingContext.BoardTransform.Matrix = reset;
                            if (DrawingContext.ShowTellVisible == Visibility.Hidden) {
                                DrawingContext.ShowTellVisible = Visibility.Visible;
                                DrawingContext.WorldMapVisible = Visibility.Hidden;
                                DrawingContext.DrawingBoardOpacity = 0;
                            } else {
                                DrawingContext.ShowTellVisible = Visibility.Hidden;
                                DrawingContext.DrawingBoardOpacity = 1;
                            }

                            break;
                        // New Map
                        case ApplicationGesture.Triangle:
                            ResetMap();
                            break;
                        // Reset Scaling
                        case ApplicationGesture.Circle:
                            DrawingContext.BoardTransform.Matrix = reset;
                            break;

                        case ApplicationGesture.Check:
                            SaveMap();
                            break;

                        case ApplicationGesture.DownLeft:
                            if (_IsSession) {
                                brdDisable.Visibility = Visibility.Visible;
                                var wnd = new wndQuit();
                                wnd.Closed += (sender, EventArgs) => brdDisable.Visibility = Visibility.Hidden;
                                wnd.Show();
                            } else {
                                Application.Current.Shutdown();
                            }

                            break;

                        case ApplicationGesture.UpLeft:
                            break;

                        case ApplicationGesture.UpRight:
                            break;

                        case ApplicationGesture.DownRight:
                            break;

                        default: // Geste nicht allg. verarbeitet also an die Verarbeitung für Touch/Stylus weitergeben
                            if (stylusActive)
                                HandleStylusGesture((InkCanvas)sender, gestureResults[0].ApplicationGesture);
                            else
                                HandleTouchGesture((InkCanvas)sender, gestureResults[0].ApplicationGesture);
                            break;
                    }

                    _MouseGestureActive = false;
                }
            } catch (Exception ex) {
                Alert.FadeError(ex);
            }
        }

        private void HandleTouchGesture(InkCanvas sender, ApplicationGesture gesture) {
            switch (gesture) {
                // FolderImages ein-/ausblenden
                case ApplicationGesture.Down:
                    if (needMapReload)
                        ((vmFolderImages)viewFolderImages.DataContext).LoadMaps();
                    needMapReload = false;
                    viewFolderImages.SlideIntoView();
                    break;

                case ApplicationGesture.Left:
                    viewCampaign.SlideIntoView();
                    break;

                case ApplicationGesture.Right:
                    viewCharacters.SlideIntoView();
                    break;

                case ApplicationGesture.Up:
                    viewNotes.SlideIntoView();
                    break;
            }
        }

        private void HandleStylusGesture(InkCanvas sender, ApplicationGesture gesture) {
            switch (gesture) {
                //case ApplicationGesture.Down:
                //    ToolbarGrid.SlideToolbar();
                //    break;

                //// Pensize <
                //case ApplicationGesture.Left:
                //    ChangePenSize();
                //    break;

                //// Pensize >
                //case ApplicationGesture.Right:
                //    ChangePenSize(increase: true);
                //    break;

                //// DrawMode umschalten
                //case ApplicationGesture.Up:
                //    NextDrawMode();
                //    break;

                //// Speichern
                //case ApplicationGesture.Check:
                //    SaveMap();
                //    ShowMessage("Map Saved!");
                //    break;

                //// neue Map
                //case ApplicationGesture.Circle:
                //    SaveMap();
                //    InitializeNewMap();
                //    break;

                //// Laden
                //case ApplicationGesture.ChevronUp:
                //    inkMain.EditingMode = InkCanvasEditingMode.Ink;
                //    LoadFile();
                //    break;

                //// Undo
                //case ApplicationGesture.ChevronLeft:
                //    Undo();
                //    break;

                //// Redo
                //case ApplicationGesture.ChevronRight:
                //    Redo();
                //    break;
            }
        }

        #endregion

        #region Drawing

        private void InkDrawingBoard_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e) {
            try {
                switch (DrawingContext.DrawMode) {
                    case DrawMode.FREE:
                        break;

                    case DrawMode.SNAP:
                        DrawSnap(sender, e);
                        break;

                    case DrawMode.LINE:
                        DrawLine(sender, e);
                        break;

                    case DrawMode.SHAPE:
                    case DrawMode.ROOM_WILDSHAPE:
                        DrawShape(sender, e);
                        break;

                    case DrawMode.ROOM:
                        DrawRoom(e);
                        break;

                    case DrawMode.OBJECT_ADD:
                    case DrawMode.OBJECT_SNAP:
                        DrawObjects(e);
                        break;

                    case DrawMode.PROP_SELECT:
                        SelectPropFromOcr(e);
                        break;

                    case DrawMode.PROP_ADD:
                        DrawSelectedPropFromOcr(e);
                        break;

                    default:
                        DrawingContext.Strokes.Remove(e.Stroke);
                        break;
                }
            } catch (Exception ex) {
                Alert.FadeError(ex);
            }
        }

        private void SelectPropFromOcr(InkCanvasStrokeCollectedEventArgs e) {
            //using var ms = new MemoryStream();
            //var strokes = new StrokeCollection(new[] { e.Stroke });
            //strokes.Save(ms);
            //var ink = new Ink();
            //ink.Load(ms.ToArray());

            //using var context = new RecognizerContext();
            //if (ink.Strokes.Count > 0) {
            //    context.Strokes = ink.Strokes;

            //    var result = context.Recognize(out var status);
            //    if (status != RecognitionStatus.NoError) {
            //        Alert.FadeError("OCR Failed.");
            //    }


            //    var propName = "";
            //    switch (result.TopString) {
            //        case "B":
            //            propName = "bed_1x2";
            //            break;

            //        case "C":
            //        case "c":
            //        case "(":
            //            propName = "chair";
            //            break;
            //    }

            //    if (!propName.IsNullOrEmpty()) {
            //        Alert.FadeInfo("Prop selected", propName);
            //        var image = new ObjectImageSource(@$"Objects\Props\{propName}.png");
            //        ToolbarInk.SelectedProp = image;
            //        DrawingContext.DrawMode = DrawMode.PROP_ADD;
            //    }
            //}

            DrawingContext.Strokes.Remove(e.Stroke);
        }

        private void DrawSelectedPropFromOcr(InkCanvasStrokeCollectedEventArgs e) {
            var gridPoints = new List<Point>();
            foreach (var sp in e.Stroke.StylusPoints) {
                var point = sp.ToPoint();
                var x = GetGridXIndex(point.X) * vmDrawingBoard.GridSize.Width;
                var y = GetGridYIndex(point.Y) * vmDrawingBoard.GridSize.Height;
                var gridPoint = new Point(x, y);

                if (gridPoints.Contains(gridPoint)) {
                    continue;
                }

                gridPoints.Add(gridPoint);
            }

            //DrawPropFromOcr(gridPoints);
            DrawingContext.Strokes.Remove(e.Stroke);
        }

        /// <summary>
        /// Eine gerade Linie zwischen Start-/Endpunkt zeichnen.
        /// </summary>
        private void DrawLine(object sender, InkCanvasStrokeCollectedEventArgs e) {
            // nur der erste und letzte Punkt sind von bedeutung da eine gerade Linie gezogen werden soll
            var start = e.Stroke.StylusPoints.FirstOrDefault();
            var end = e.Stroke.StylusPoints.LastOrDefault();

            e.Stroke.StylusPoints = new StylusPointCollection(new[] { start, end });
        }

        /// <summary>
        /// Eine Form zeichnen und füllen.
        /// </summary>
        private void DrawShape(object sender, InkCanvasStrokeCollectedEventArgs e) {
            var poly = new Polygon();
            if (DrawingContext.DrawMode == DrawMode.ROOM_WILDSHAPE) {
                var brush = ToolbarInk.SelectedObjectImage?.AsNewImageBrush();
                if (brush != null) {
                    brush.TileMode = TileMode.Tile;
                    brush.Viewport = new Rect(0, 0, 300, 300); //vmDrawingBoard.GridSize with { X = 0, Y = 0 };
                    brush.ViewportUnits = BrushMappingMode.Absolute;
                    brush.Stretch = Stretch.Fill;
                    brush.AlignmentX = AlignmentX.Left;
                    brush.AlignmentY = AlignmentY.Top;
                    poly.Fill = brush;
                    poly.StrokeThickness = 0;

                    e.Stroke.DrawingAttributes.Width = ToolbarInk.WallWidth;
                    e.Stroke.DrawingAttributes.Height = ToolbarInk.WallWidth;
                }
            } else {
                // alten Strich löschen, nur das Polygon soll zu sehen sein
                //DrawingContext.Strokes.Remove(e.Stroke);
                poly.Fill = new SolidColorBrush(inkDrawingBoard.DefaultDrawingAttributes.Color);
            }

            foreach (var sp in e.Stroke.StylusPoints)
                poly.Points.Add(sp.ToPoint());

            AddPolygon(poly, colorize: DrawingContext.DrawMode != DrawMode.ROOM_WILDSHAPE);
        }

        /// <summary>
        /// Eine Linie zeichnen welche sich am Grid ausrichtet.
        /// </summary>
        private void DrawSnap(object sender, InkCanvasStrokeCollectedEventArgs e) {
            // nur der erste und letzte Punkt sind von bedeutung da eine gerade Linie gezogen werden soll
            var start = e.Stroke.StylusPoints.FirstOrDefault();
            var end = e.Stroke.StylusPoints.LastOrDefault();

            // Startpunkt auf das Grid verschieben
            var gridXNumber = start.X / vmDrawingBoard.GridWidthHalf;
            var gridXIndex = Math.Truncate(start.X / vmDrawingBoard.GridWidthHalf);

            if ((gridXNumber - gridXIndex) > 0.5)
                gridXIndex++;

            start.X = gridXIndex * vmDrawingBoard.GridWidthHalf;

            var gridYNumber = start.Y / vmDrawingBoard.GridHeightHalf;
            var gridYIndex = Math.Truncate(start.Y / vmDrawingBoard.GridHeightHalf);

            if ((gridYNumber - gridYIndex) > 0.5)
                gridYIndex++;

            start.Y = gridYIndex * vmDrawingBoard.GridHeightHalf;

            // Endpunkt auf das Grid verschieben
            gridXNumber = end.X / vmDrawingBoard.GridWidthHalf;
            gridXIndex = Math.Truncate(end.X / vmDrawingBoard.GridWidthHalf);

            if ((gridXNumber - gridXIndex) > 0.5)
                gridXIndex++;

            end.X = gridXIndex * vmDrawingBoard.GridWidthHalf;

            gridYNumber = end.Y / vmDrawingBoard.GridHeightHalf;
            gridYIndex = Math.Truncate(end.Y / vmDrawingBoard.GridHeightHalf);

            if ((gridYNumber - gridYIndex) > 0.5)
                gridYIndex++;

            end.Y = gridYIndex * vmDrawingBoard.GridHeightHalf;

            e.Stroke.StylusPoints = new StylusPointCollection(new[] { start, end });
        }

        private void DrawRoom(InkCanvasStrokeCollectedEventArgs e) {
            var gridPoints = DrawObjects(e);

            if (ToolbarInk.WallWidth == 0) {
                return;
            }

            var gridOffset = 0;
            var shadowColor = (Color)ColorConverter.ConvertFromString("#4000");
            var shadowSize = ToolbarInk.WallWidth + 1;
            var shadowSizeInner = 12;
            var wallThicknessOuter = ToolbarInk.WallWidth;
            var wallThicknessInner = 5;

            foreach (var gridPoint in gridPoints) {
                var hasShapeAtTheTop = DrawingContext.Shapes.Any(shape => shape.RenderedGeometry.Bounds.X == gridPoint.X && shape.RenderedGeometry.Bounds.Y == gridPoint.Y - vmDrawingBoard.GridSize.Height);
                var hasShapeAtTheBottom = DrawingContext.Shapes.Any(shape => shape.RenderedGeometry.Bounds.X == gridPoint.X && shape.RenderedGeometry.Bounds.Y == gridPoint.Y + vmDrawingBoard.GridSize.Height);
                var hasShapeToTheRight = DrawingContext.Shapes.Any(shape => shape.RenderedGeometry.Bounds.X == gridPoint.X + vmDrawingBoard.GridSize.Width && shape.RenderedGeometry.Bounds.Y == gridPoint.Y);
                var hasShapeToTheLeft = DrawingContext.Shapes.Any(shape => shape.RenderedGeometry.Bounds.X == gridPoint.X - vmDrawingBoard.GridSize.Width && shape.RenderedGeometry.Bounds.Y == gridPoint.Y);

                // Wand rechts
                if (!gridPoints.Any(gp => gp.X == gridPoint.X + vmDrawingBoard.GridSize.Width && gp.Y == gridPoint.Y)) {
                    var clonedDrawingAttributes = inkDrawingBoard.DefaultDrawingAttributes.Clone();
                    clonedDrawingAttributes.Width = hasShapeToTheRight ? wallThicknessInner : wallThicknessOuter;
                    clonedDrawingAttributes.Height = hasShapeToTheRight ? wallThicknessInner : wallThicknessOuter;
                    var shadowAttributes = inkDrawingBoard.DefaultDrawingAttributes.Clone();
                    shadowAttributes.Color = shadowColor;
                    shadowAttributes.Width = shadowSize;
                    shadowAttributes.Height = shadowSize;

                    if (hasShapeToTheRight) {
                        shadowAttributes.Width = shadowSizeInner;
                        shadowAttributes.Height = shadowSizeInner;

                        var strokeToRemove = DrawingContext.Strokes.FirstOrDefault(s => s.StylusPoints[0].X == gridPoint.X + vmDrawingBoard.GridSize.Width && s.StylusPoints[0].Y == gridPoint.Y && s.StylusPoints[1].X == gridPoint.X + vmDrawingBoard.GridSize.Width && s.StylusPoints[1].Y == gridPoint.Y + vmDrawingBoard.GridSize.Height);
                        if (strokeToRemove != null) {
                            DrawingContext.Strokes.Remove(strokeToRemove);
                            //var shadowToRemove = DrawingContext.Strokes.FirstOrDefault(s => s.StylusPoints[0].X == strokeToRemove.StylusPoints[0].X - shadowSize && s.StylusPoints[0].Y == strokeToRemove.StylusPoints[0].Y - 11 && s.StylusPoints[1].X == strokeToRemove.StylusPoints[1].X && s.StylusPoints[1].Y == strokeToRemove.StylusPoints[1].Y - 11);
                            //if (shadowToRemove != null) {
                            //    DrawingContext.Strokes.Remove(shadowToRemove);
                            //}
                        }
                    }

                    var stroke = new Stroke(new StylusPointCollection(new[] {
                        new Point(gridPoint.X + (hasShapeToTheRight ? 0 : gridOffset) + vmDrawingBoard.GridSize.Width - (clonedDrawingAttributes.Width - (hasShapeToTheRight ? 5 : 4)), gridPoint.Y + shadowAttributes.Width / 2),
                        new Point(gridPoint.X + (hasShapeToTheRight ? 0 : gridOffset) + vmDrawingBoard.GridSize.Width - (clonedDrawingAttributes.Width - (hasShapeToTheRight ? 5 : 4)), gridPoint.Y + vmDrawingBoard.GridSize.Height - shadowAttributes.Width / 2)
                    }), shadowAttributes);
                    DrawingContext.Strokes.Insert(0, stroke);

                    //var brush = new ObjectImageSource("C:\\Users\\mj\\source\\repos\\CampaignMaster\\CampaignMaster\\bin\\Debug\\net6.0-windows\\Resources\\Images\\Floors\\wood_3.png").ImageSource;
                    //var tmpPolygon = new Polygon {
                    //    Points = new PointCollection {
                    //        new(gridPoint.X + (hasShapeToTheRight ? 0 : gridOffset) + vmDrawingBoard.GridSize.Width, gridPoint.Y - (hasShapeToTheRight ? 0 : gridOffset)),
                    //        new(gridPoint.X + (hasShapeToTheRight ? 0 : gridOffset) + vmDrawingBoard.GridSize.Width, gridPoint.Y + vmDrawingBoard.GridSize.Height)
                    //    },
                    //    Stroke = new ImageBrush(brush),
                    //    StrokeThickness = 15
                    //};
                    //AddPolygon(tmpPolygon, colorize: false);

                    stroke = new Stroke(new StylusPointCollection(new[] {
                        new Point(gridPoint.X + (hasShapeToTheRight ? 0 : gridOffset) + vmDrawingBoard.GridSize.Width, gridPoint.Y - (hasShapeToTheRight ? 0 : gridOffset)),
                        new Point(gridPoint.X + (hasShapeToTheRight ? 0 : gridOffset) + vmDrawingBoard.GridSize.Width, gridPoint.Y + vmDrawingBoard.GridSize.Height)
                    }), clonedDrawingAttributes);
                    DrawingContext.Strokes.Add(stroke);
                }

                // Wand oben
                if (!gridPoints.Any(gp => gp.X == gridPoint.X && gp.Y == gridPoint.Y - vmDrawingBoard.GridSize.Height)) {
                    var clonedDrawingAttributes = inkDrawingBoard.DefaultDrawingAttributes.Clone();
                    clonedDrawingAttributes.Width = hasShapeAtTheTop ? wallThicknessInner : wallThicknessOuter;
                    clonedDrawingAttributes.Height = hasShapeAtTheTop ? wallThicknessInner : wallThicknessOuter;
                    var shadowAttributes = inkDrawingBoard.DefaultDrawingAttributes.Clone();
                    shadowAttributes.Color = shadowColor;
                    shadowAttributes.Width = shadowSize;
                    shadowAttributes.Height = shadowSize;

                    if (hasShapeAtTheTop) {
                        shadowAttributes.Width = shadowSizeInner;
                        shadowAttributes.Height = shadowSizeInner;

                        var strokeToRemove = DrawingContext.Strokes.FirstOrDefault(s => s.StylusPoints[1].X == gridPoint.X && s.StylusPoints[1].Y == gridPoint.Y && s.StylusPoints[0].X == gridPoint.X + vmDrawingBoard.GridSize.Width && s.StylusPoints[0].Y == gridPoint.Y);
                        if (strokeToRemove != null) {
                            DrawingContext.Strokes.Remove(strokeToRemove);
                            //var shadowToRemove = DrawingContext.Strokes.FirstOrDefault(s => s.StylusPoints[0].X == strokeToRemove.StylusPoints[0].X - shadowSize && s.StylusPoints[0].Y == strokeToRemove.StylusPoints[0].Y - 11 && s.StylusPoints[1].X == strokeToRemove.StylusPoints[1].X && s.StylusPoints[1].Y == strokeToRemove.StylusPoints[1].Y - 11);
                            //if (shadowToRemove != null)
                            //{
                            //    DrawingContext.Strokes.Remove(shadowToRemove);
                            //}
                        }
                    }

                    var stroke = new Stroke(new StylusPointCollection(new[] {
                        new Point(gridPoint.X - (hasShapeAtTheTop ? 0 : gridOffset) + shadowAttributes.Width / 2, gridPoint.Y - (hasShapeAtTheTop ? 0 : gridOffset) + (clonedDrawingAttributes.Width - (hasShapeAtTheTop ? 5 : 4))),
                        new Point(gridPoint.X - (hasShapeAtTheTop ? 0 : gridOffset) + vmDrawingBoard.GridSize.Width - shadowAttributes.Width / 2, gridPoint.Y - (hasShapeAtTheTop ? 0 : gridOffset) + (clonedDrawingAttributes.Width - (hasShapeAtTheTop ? 5 : 4)))
                    }), shadowAttributes);
                    DrawingContext.Strokes.Insert(0, stroke);

                    stroke = new Stroke(new StylusPointCollection(new[] {
                        new Point(gridPoint.X - (hasShapeAtTheTop ? 0 : gridOffset), gridPoint.Y - (hasShapeAtTheTop ? 0 : gridOffset)),
                        new Point(gridPoint.X - (hasShapeAtTheTop ? 0 : gridOffset) + vmDrawingBoard.GridSize.Width, gridPoint.Y - (hasShapeAtTheTop ? 0 : gridOffset))
                    }), clonedDrawingAttributes);
                    DrawingContext.Strokes.Add(stroke);
                }

                // Wand unten
                if (!gridPoints.Any(gp => gp.X == gridPoint.X && gp.Y == gridPoint.Y + vmDrawingBoard.GridSize.Height)) {
                    var clonedDrawingAttributes = inkDrawingBoard.DefaultDrawingAttributes.Clone();
                    clonedDrawingAttributes.Width = hasShapeAtTheBottom ? wallThicknessInner : wallThicknessOuter;
                    clonedDrawingAttributes.Height = hasShapeAtTheBottom ? wallThicknessInner : wallThicknessOuter;
                    var shadowAttributes = inkDrawingBoard.DefaultDrawingAttributes.Clone();
                    shadowAttributes.Color = shadowColor;
                    shadowAttributes.Width = shadowSize;
                    shadowAttributes.Height = shadowSize;

                    if (hasShapeAtTheBottom) {
                        shadowAttributes.Width = shadowSizeInner;
                        shadowAttributes.Height = shadowSizeInner;

                        var strokeToRemove = DrawingContext.Strokes.FirstOrDefault(s => s.StylusPoints[0].X == gridPoint.X && s.StylusPoints[0].Y == gridPoint.Y + vmDrawingBoard.GridSize.Height && s.StylusPoints[1].X == gridPoint.X + vmDrawingBoard.GridSize.Width && s.StylusPoints[1].Y == gridPoint.Y + vmDrawingBoard.GridSize.Height);
                        if (strokeToRemove != null) {
                            DrawingContext.Strokes.Remove(strokeToRemove);
                            var shadowToRemove = DrawingContext.Strokes.FirstOrDefault(s => s.StylusPoints[0].X == strokeToRemove.StylusPoints[0].X + shadowSize / 2 && s.StylusPoints[0].Y == strokeToRemove.StylusPoints[0].Y + (wallThicknessOuter - 4) && s.StylusPoints[1].X == strokeToRemove.StylusPoints[1].X - shadowSize / 2 && s.StylusPoints[1].Y == strokeToRemove.StylusPoints[1].Y + (wallThicknessOuter - 4));
                            if (shadowToRemove != null) {
                                DrawingContext.Strokes.Remove(shadowToRemove);
                            }
                        }

                        var shadowStroke = new Stroke(new StylusPointCollection(new[] {
                            new Point(gridPoint.X + vmDrawingBoard.GridSize.Width - shadowAttributes.Width, gridPoint.Y + vmDrawingBoard.GridSize.Height - (clonedDrawingAttributes.Width - (hasShapeAtTheBottom ? 5 : 4))),
                            new Point(gridPoint.X, gridPoint.Y + vmDrawingBoard.GridSize.Height - (clonedDrawingAttributes.Width - (hasShapeAtTheBottom ? 5 : 4)))
                        }), shadowAttributes);
                        DrawingContext.Strokes.Insert(0, shadowStroke);
                    }

                    var stroke = new Stroke(new StylusPointCollection(new[] {
                        new Point(gridPoint.X + (hasShapeAtTheBottom ? 0 : gridOffset) + vmDrawingBoard.GridSize.Width, gridPoint.Y + vmDrawingBoard.GridSize.Height + (hasShapeAtTheBottom ? 0 : gridOffset)),
                        new Point(gridPoint.X + (hasShapeAtTheBottom ? 0 : gridOffset), gridPoint.Y + vmDrawingBoard.GridSize.Height + (hasShapeAtTheBottom ? 0 : gridOffset))
                    }), clonedDrawingAttributes);
                    DrawingContext.Strokes.Add(stroke);
                }

                // Wand links
                if (!gridPoints.Any(gp => gp.X == gridPoint.X - vmDrawingBoard.GridSize.Width && gp.Y == gridPoint.Y)) {
                    var clonedDrawingAttributes = inkDrawingBoard.DefaultDrawingAttributes.Clone();
                    clonedDrawingAttributes.Width = hasShapeToTheLeft ? wallThicknessInner : wallThicknessOuter;
                    clonedDrawingAttributes.Height = hasShapeToTheLeft ? wallThicknessInner : wallThicknessOuter;
                    var shadowAttributes = inkDrawingBoard.DefaultDrawingAttributes.Clone();
                    shadowAttributes.Color = shadowColor;
                    shadowAttributes.Width = shadowSize;
                    shadowAttributes.Height = shadowSize;

                    if (hasShapeToTheLeft) {
                        shadowAttributes.Width = shadowSizeInner;
                        shadowAttributes.Height = shadowSizeInner;

                        var strokeToRemove = DrawingContext.Strokes.FirstOrDefault(s => s.StylusPoints[0].X == gridPoint.X && s.StylusPoints[0].Y == gridPoint.Y && s.StylusPoints[1].X == gridPoint.X && s.StylusPoints[1].Y == gridPoint.Y + vmDrawingBoard.GridSize.Height);
                        if (strokeToRemove != null) {
                            DrawingContext.Strokes.Remove(strokeToRemove);
                            var shadowToRemove = DrawingContext.Strokes.FirstOrDefault(s => s.StylusPoints[0].X == strokeToRemove.StylusPoints[0].X - (wallThicknessOuter - 4) && s.StylusPoints[0].Y == strokeToRemove.StylusPoints[0].Y + shadowSize / 2 && s.StylusPoints[1].X == strokeToRemove.StylusPoints[1].X - (wallThicknessOuter - 4) && s.StylusPoints[1].Y == strokeToRemove.StylusPoints[1].Y - shadowSize / 2);
                            if (shadowToRemove != null) {
                                DrawingContext.Strokes.Remove(shadowToRemove);
                            }
                        }

                        var shadowStroke = new Stroke(new StylusPointCollection(new[] {
                            new Point(gridPoint.X + (clonedDrawingAttributes.Width - (hasShapeToTheLeft ? 5 : 4)), gridPoint.Y + (clonedDrawingAttributes.Width - (hasShapeToTheLeft ? 5 : 4))),
                            new Point(gridPoint.X + (clonedDrawingAttributes.Width - (hasShapeToTheLeft ? 5 : 4)), gridPoint.Y + vmDrawingBoard.GridSize.Height - shadowAttributes.Width)
                        }), shadowAttributes);
                        DrawingContext.Strokes.Insert(0, shadowStroke);
                    }


                    var stroke = new Stroke(new StylusPointCollection(new[] {
                        new Point(gridPoint.X - (hasShapeToTheLeft ? 0 : gridOffset), gridPoint.Y + (hasShapeToTheLeft ? 0 : gridOffset)),
                        new Point(gridPoint.X - (hasShapeToTheLeft ? 0 : gridOffset), gridPoint.Y + (hasShapeToTheLeft ? 0 : gridOffset) + vmDrawingBoard.GridSize.Height)
                    }), clonedDrawingAttributes);
                    DrawingContext.Strokes.Add(stroke);
                }
            }
        }

        private List<Point> DrawObjects(InkCanvasStrokeCollectedEventArgs e) {
            var gridPoints = new List<Point>();
            var rawPoints = new List<Point>();
            foreach (var sp in e.Stroke.StylusPoints) {
                var point = sp.ToPoint();
                var x = GetGridXIndex(point.X) * vmDrawingBoard.GridSize.Width;
                var y = GetGridYIndex(point.Y) * vmDrawingBoard.GridSize.Height;
                var gridPoint = new Point(x, y);

                if (gridPoints.Contains(gridPoint)) {
                    continue;
                }

                gridPoints.Add(gridPoint);
                rawPoints.Add(point);

                //if (DrawingContext.DrawMode != DrawMode.ROOM) {
                //    break;
                //}
            }

            if (DrawingContext.DrawMode == DrawMode.OBJECT_ADD) {
                var rotation = GetRotation(e.Stroke.StylusPoints);
                DrawGridObjectCentered(ToolbarInk.SelectedObjectImage.Orientation == Orientation.Horizontal || rotation is Rotation.Rotate0 or Rotation.Rotate270 ? rawPoints.First() : rawPoints.Last(), rotation);
            } else if (DrawingContext.DrawMode == DrawMode.ROOM) {
                gridPoints.ForEach(p => DrawGridObject(p));
            } else {
                var rotation = GetRotation(e.Stroke.StylusPoints);
                DrawGridObject(ToolbarInk.SelectedObjectImage.Orientation == Orientation.Horizontal || rotation is Rotation.Rotate0 or Rotation.Rotate270 ? gridPoints.First() : gridPoints.Last(), rotation);
            }

            // alten Strich löschen, nur die Objekte sollen zu sehen sein
            DrawingContext.Strokes.Remove(e.Stroke);

            return gridPoints;
        }

        private Rotation GetRotation(StylusPointCollection points) {
            return GetRotation(points.First().ToPoint(), points.Last().ToPoint());
        }

        private Rotation GetRotation(Point start, Point end) {
            var xStart = GetGridXIndex(start.X);
            var yStart = GetGridXIndex(start.Y);
            var xEnd = GetGridXIndex(end.X);
            var yEnd = GetGridXIndex(end.Y);

            if (xStart == xEnd) {
                // Hoch oder Runter
                return yStart < yEnd ? Rotation.Rotate0 : Rotation.Rotate180;
            } else {
                // Links oder Rechts
                return xStart < xEnd ? Rotation.Rotate270 : Rotation.Rotate90;
            }

            return Rotation.Rotate0;
        }

        /// <summary>
        /// Fügt der Karte ein Objekt in der Größe eines Grid-Feldes hinzu.
        /// Als Objekt wird das aktuel ausgewählte der InkToolbar gewählt.
        /// </summary>
        private void DrawObject(Point p) {
            var x = GetGridXIndex(p.X) * vmDrawingBoard.GridSize.Width;
            var y = GetGridYIndex(p.Y) * vmDrawingBoard.GridSize.Height;

            DrawGridObject(new Point(x, y));
        }

        /// <summary>
        /// Fügt der Karte ein Objekt in der Größe eines Grid-Feldes hinzu.
        /// Als Objekt wird das aktuel ausgewählte der InkToolbar gewählt.
        /// </summary>
        private void DrawGridObject(Point gridPoint, Rotation rotation = Rotation.Rotate0) {
            if (ToolbarInk.SelectedObjectImage == null) {
                return;
            }

            var objectImage = ToolbarInk.SelectedObjectImage.AsNewVariationWithRotation(rotation);
            var brush = new ImageBrush(objectImage.ImageSource);

            var width = _ObjectGridWidth * (DrawingContext.DrawMode == DrawMode.ROOM ? 1 : objectImage.ObjectWidth);
            var height = _ObjectGridHeight * (DrawingContext.DrawMode == DrawMode.ROOM ? 1 : objectImage.ObjectHeight);
            var tmpPolygon = new Polygon {
                Points = new PointCollection {
                    new(gridPoint.X, gridPoint.Y),
                    new(gridPoint.X + width, gridPoint.Y),
                    new(gridPoint.X + width, gridPoint.Y + height),
                    new(gridPoint.X, gridPoint.Y + height)
                },
                Fill = brush,
                Stroke = (DrawingContext.DrawMode == DrawMode.ROOM ? brush : null)
            };
            AddPolygon(tmpPolygon, colorize: false);
        }

        private void DrawGridObjectCentered(Point gridPoint, Rotation rotation = Rotation.Rotate0) {
            if (ToolbarInk.SelectedObjectImage == null) {
                return;
            }

            var objectImage = ToolbarInk.SelectedObjectImage.AsNewVariationWithRotation(rotation);
            var brush = new ImageBrush(objectImage.ImageSource);

            var width = _ObjectGridWidth * objectImage.ObjectWidth;
            var height = _ObjectGridHeight * objectImage.ObjectHeight;
            var x = gridPoint.X - width / 2;
            var y = gridPoint.Y - height / 2;
            var tmpPolygon = new Polygon {
                Points = new PointCollection {
                    new(x, y),
                    new(x + width, y),
                    new(x + width, y + height),
                    new(x, y + height)
                },
                Fill = brush,
            };
            AddPolygon(tmpPolygon, colorize: false);
        }

        //private void DrawPropFromOcr(List<Point> gridPoints) {
        //    var firstPoint = gridPoints.First();
        //    var lastPoint = gridPoints.Last();
        //    var positionPoint = lastPoint;

        //    // Rotation
        //    var rotation = Rotation.Rotate0;
        //    if (lastPoint.Y > firstPoint.Y && firstPoint.X == lastPoint.X) {
        //        rotation = Rotation.Rotate180;
        //        positionPoint = firstPoint;
        //    } else if (firstPoint.Y == lastPoint.Y) {
        //        if (firstPoint.X > lastPoint.X) {
        //            rotation = Rotation.Rotate270;
        //        } else if (lastPoint.X > firstPoint.X) {
        //            rotation = Rotation.Rotate90;
        //            positionPoint = firstPoint;
        //        }
        //    }

        //    var img = new ObjectImageSource(ToolbarInk.SelectedProp.FullFileName, rotation: rotation);
        //    var width = vmDrawingBoard.GridSize.Width * img.ObjectWidth;
        //    var height = vmDrawingBoard.GridSize.Height * img.ObjectHeight;
        //    if (img.IsSingleSquare) {
        //        positionPoint = firstPoint;
        //    }

        //    var tmpPolygon = new Polygon {
        //        Points = new PointCollection {
        //            new(positionPoint.X, positionPoint.Y),
        //            new(positionPoint.X + width, positionPoint.Y),
        //            new(positionPoint.X + width, positionPoint.Y + height),
        //            new(positionPoint.X, positionPoint.Y + height)
        //        },
        //        Fill = new ImageBrush(img.ImageSource)
        //    };

        //    AddPolygon(tmpPolygon, colorize: false);

        //    //if (tmpPolygon.RenderTransform is not TransformGroup tg) {
        //    //    return;
        //    //}

        //    //if (tg.Children[2] is not RotateTransform rt) {
        //    //    return;
        //    //}
        //}

        /// <summary>
        /// Beginnt das Zeichnen eines neuen Polygons.
        /// Wenn bereits ein Zeichenvorgang für ein neues Polygon im gange ist, wird dieses fortgeführt.
        /// </summary>
        /// <param name="start">Ausgangspunkt oder neuer Punkt für das neue Polygon</param>
        private void DrawPolygon(Point start) {
            if (!drawPoly) {
                polyline = new Polyline();
                polylinePoints = new PointCollection();
                polyline.StrokeThickness = 2;
                polyline.Stroke = Brushes.Black;
                DrawingContext.Shapes.Add(polyline);

                polylinePoints.Add(start);
                polyline.Points = polylinePoints.Clone();

                polyStart = new Ellipse();
                polyStart.Width = 20;
                polyStart.Height = 20;
                polyStart.Stroke = Brushes.Black;
                polyStart.StrokeThickness = 2;
                polyStart.Fill = new SolidColorBrush { Color = Colors.Gold };
                polyStart.Margin = new Thickness(left: polyline.Points[0].X - polyStart.Width / 2, top: polyline.Points[0].Y - polyStart.Height / 2, right: 0, bottom: 0);
                DrawingContext.Shapes.Add(polyStart);

                drawPoly = true;
            } else {
                polylinePoints.Add(start);
                polyline.Points = polylinePoints.Clone();
            }
        }

        /// <summary>
        /// Beendet und schließt das Polygon das aktuell bezeichnet wird.
        /// </summary>
        private void EndPolygon() {
            if (!drawPoly) {
                return;
            }

            DrawingContext.Shapes.Remove(polyStart);
            DrawingContext.Shapes.Remove(polyline);

            var tmpPolygon = new Polygon {
                Points = polylinePoints.Clone()
            };

            AddPolygon(tmpPolygon);
            //if (DrawingContext.DrawMode == DrawMode.POLYGON) {
            //    AddPolygon(tmpPolygon);
            //} else if (DrawingContext.DrawMode == DrawMode.SCRATCH_AREA) {
            //    FillPolygonWithInk(tmpPolygon);
            //}

            polylinePoints.Clear();
            drawPoly = false;
        }

        private void FillPolygonWithInk(Polygon poly) {
            var bounds = poly.Bounds();
            var clonedDrawingAttributes = new DrawingAttributes {
                Color = Colors.Black,
                FitToCurve = false,
                Height = 5,
                Width = 5,
                IgnorePressure = true,
                IsHighlighter = false,
                StylusTip = StylusTip.Rectangle
            };

            for (var y = bounds.Top; y < bounds.Bottom; y += 3) {
                for (var x = bounds.Left; x < bounds.Right; x += 3) {
                    var point = new Point(x, y);

                    if (!point.IsInPolygon(poly)) {
                        continue;
                    }

                    var stroke = new Stroke(new StylusPointCollection(new[] { point }), clonedDrawingAttributes);
                    DrawingContext.Scratches.Add(stroke);
                }
            }
        }

        #endregion

        #region Manipulation

        private void InkDrawingBoard_ManipulationStarting(object sender, ManipulationStartingEventArgs e) {
            try {
                //Sobald eine Manipluation erkannt wird (z.B. PinchZoom) diese auf das Grid der Zeichenoberfläsche setzen
                e.ManipulationContainer = grdDrawingBoard;
            } catch (Exception ex) {
                Alert.FadeError(ex);
            }
        }

        private void InkDrawingBoard_ManipulationDelta(object sender, ManipulationDeltaEventArgs e) {
            try {
                //Sowohl Verschieben als auch Zoomen nur mit mind. 2 Fingern um die Gestensteuerung zu erhalten 
                if (e.Manipulators.Count() >= 2) {
                    inkScratchBoard.EditingMode = InkCanvasEditingMode.None;
                    inkDrawingBoard.EditingMode = InkCanvasEditingMode.None;

                    var md = e.DeltaManipulation;
                    var trans = md.Translation;
                    var scale = md.Scale;

                    var m = DrawingContext.BoardTransform.Matrix;
                    var m2 = DrawingContext.BoardTransformReset.Matrix;

                    m.Translate(trans.X, trans.Y);
                    DrawingContext.BoardTransformReset.Matrix = m2;
                    if (md.Expansion.Length > 2.5)
                        m.ScaleAt(scale.X, scale.Y, e.ManipulationOrigin.X, e.ManipulationOrigin.Y);

                    DrawingContext.BoardTransform.Matrix = m;
                    DrawingContext.ScaleTransformCenter = e.ManipulationOrigin;

                    e.Handled = true;
                }
            } catch (Exception ex) {
                Alert.FadeError(ex);
            }
        }

        #endregion

        #endregion

        #endregion

        private void Window_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            try {
                ((vmDrawingBoard)DataContext).PropertyChanged += DrawingBoard_PropertyChanged;
            } catch (Exception ex) {
                Log.Error(ex);
            }
        }

        private void DrawingBoard_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(vmDrawingBoard.PenSize):
                    inkDrawingBoard.DefaultDrawingAttributes.Height = DrawingContext.PenSize;
                    inkDrawingBoard.DefaultDrawingAttributes.Width = DrawingContext.PenSize;

                    inkScratchBoard.DefaultDrawingAttributes.Height = DrawingContext.PenSize;
                    inkScratchBoard.DefaultDrawingAttributes.Width = DrawingContext.PenSize;
                    break;

                case nameof(vmDrawingBoard.DrawMode):
                    switch (DrawingContext.DrawMode) {
                        case DrawMode.ROOM:
                        case DrawMode.SNAP:
                            inkDrawingBoard.DefaultDrawingAttributes.StylusTip = StylusTip.Rectangle;
                            inkDrawingBoard.EditingModeInverted = InkCanvasEditingMode.EraseByStroke;
                            break;

                        default:
                            inkDrawingBoard.DefaultDrawingAttributes.StylusTip = StylusTip.Ellipse;
                            inkDrawingBoard.EditingModeInverted = InkCanvasEditingMode.EraseByPoint;
                            break;
                    }

                    break;
            }
        }

        private void DeletePolyClick(object sender, RoutedEventArgs e) {
            if (SelectedPoly == null) {
                return;
            }

            DrawingContext.DeletePolygon(SelectedPoly);
            SelectedPoly = null;
        }

        private void IncreasePolyZClick(object sender, RoutedEventArgs e) {
            if (SelectedPoly == null) {
                return;
            }

            DrawingContext.IncreasePolygonZIndex(SelectedPoly);
        }

        private void DecreasePolyZClick(object sender, RoutedEventArgs e) {
            if (SelectedPoly == null) {
                return;
            }

            DrawingContext.DecreasePolygonZIndex(SelectedPoly);
        }

        private readonly wndCombatOrder _CombatOrderWindow = new();

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
            _CombatOrderWindow.Visibility = _CombatOrderWindow.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnCampaign_OnClick(object sender, RoutedEventArgs e) {
            new wndCampaignEditor().Show();
        }

        private void BtnSettings_OnClick(object sender, RoutedEventArgs e) {
            var settings = new wndSettings {
                DataContext = DataContext
            };
            settings.ShowDialog();
        }

        private void WndCampaignMaster_OnPreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                if (viewCampaign.IsActive) {
                    viewCampaign.SlideOutOfView();
                    return;
                }

                if (viewFolderImages.IsActive) {
                    viewFolderImages.SlideOutOfView();
                    return;
                }

                if (viewCharacters.IsActive) {
                    viewCharacters.SlideOutOfView();
                    return;
                }

                if (viewNotes.IsActive) {
                    viewNotes.SlideOutOfView();
                    return;
                }

                if (Environment.MachineName == "PCMJ") {
                    Application.Current.Shutdown();
                }
            }
        }

        private void InkDrawingBoard_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            Alert.FadeInfo("blub");
        }

    }

}