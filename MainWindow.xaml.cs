using ACadSharp;
using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tables;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace DwgLoader
{
    public partial class MainWindow : Window
    {
        private CadDocument _doc;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadDwgFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "DWG files (*.dwg)|*.dwg",
                Title = "Vyberte DWG soubor"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                LoadDwg(filePath);
            }
        }

        private void LoadDwg(string filePath)
        {
            using (DwgReader reader = new DwgReader(filePath))
            {
                _doc = reader.Read();
            }

            // Vyplnění ListBoxu s vrstvami
            LayerListBox.ItemsSource = _doc.Layers.Select(layer => layer.Name).ToList();
        }

        private void LayersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LayerListBox.SelectedItem != null)
            {
                string selectedLayerName = LayerListBox.SelectedItem.ToString();
                Layer selectedLayer = _doc.Layers.FirstOrDefault(layer => layer.Name == selectedLayerName);

                if (selectedLayer != null)
                {
                    StringBuilder layerInfo = new StringBuilder();
                    layerInfo.AppendLine($"Název: {selectedLayer.Name}");
                    layerInfo.AppendLine($"Barva: {selectedLayer.Color}");
                    layerInfo.AppendLine($"Jméno objektu: {selectedLayer.ObjectName}");
                    layerInfo.AppendLine($"Typ objektu: {selectedLayer.ObjectType}");
                    layerInfo.AppendLine($"Majitel: {selectedLayer.Owner}");

                    LayerInfoTextBlock.Text = layerInfo.ToString();

                    var entitiesInLayer = _doc.Entities
                        .Where(entity => entity.Layer.Name == selectedLayer.Name)
                        .ToList();

                    EntityListBox.ItemsSource = entitiesInLayer.OrderBy(entity => entity.ObjectName)
                        .Select(entity => $"{entity.ObjectName} (ID: {entity.Handle})")
                        .ToList();
                }
            }
        }

        private void EntityListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EntityListBox.SelectedItem != null)
            {
                string selectedEntityInfo = EntityListBox.SelectedItem.ToString();
                string entityId = selectedEntityInfo.Split(new[] { "(ID: ", ")" }, StringSplitOptions.None)[1];

                var selectedEntity = _doc.Entities.FirstOrDefault(entity => entity.Handle.ToString() == entityId);

                if (selectedEntity != null)
                {
                    StringBuilder entityInfo = new StringBuilder();
                    entityInfo.AppendLine($"Typ: {selectedEntity.ObjectName}");
                    entityInfo.AppendLine($"Vrstva: {selectedEntity.Layer.Name}");
                    entityInfo.AppendLine($"Handle: {selectedEntity.Handle}");

                    if (selectedEntity is ACadSharp.Entities.Line line)
                    {
                        entityInfo.AppendLine($"Start: {line.StartPoint}");
                        entityInfo.AppendLine($"End: {line.EndPoint}");
                        entityInfo.AppendLine($"Thickness: {line.Thickness}");
                    }
                    else if (selectedEntity is Circle circle)
                    {
                        entityInfo.AppendLine($"Center: {circle.Center}");
                        entityInfo.AppendLine($"Radius: {circle.Radius}");
                        entityInfo.AppendLine($"Thickness: {circle.Thickness}");
                        entityInfo.AppendLine($"Normal: {circle.Normal}");
                    }
                    else if (selectedEntity is Polyline2D polyline)
                    {
                        entityInfo.AppendLine("Vrcholy:");
                        foreach (var vertex in polyline.Vertices)
                        {
                            entityInfo.AppendLine($"X - {vertex.Location.X}");
                            entityInfo.AppendLine($"Y - {vertex.Location.Y}");
                        }
                    }
                    else if (selectedEntity is Polyline3D polyline3D)
                    {
                        foreach (var vertex in polyline3D.Vertices)
                        {
                            entityInfo.AppendLine($"Location X - {vertex.Location}");
                        }
                    }
                    else if (selectedEntity is Hatch hatch)
                    {
                        entityInfo.AppendLine($"Elevation - {hatch.Elevation}");
                        entityInfo.AppendLine($"Pattern - {hatch.Pattern}");
                        entityInfo.AppendLine($"PatternType - {hatch.PatternType}");
                    }
                    else if (selectedEntity is Insert insert)
                    {
                        entityInfo.AppendLine($"Block - {insert.Block}");
                        entityInfo.AppendLine($"Object type - {insert.ObjectType}");
                        entityInfo.AppendLine($"Insert point - {insert.InsertPoint}");
                    }
                    else if (selectedEntity is MText mText)
                    {
                        entityInfo.AppendLine($"InsertPoint - {mText.InsertPoint}");
                        entityInfo.AppendLine($"Height - {mText.Height}");
                        entityInfo.AppendLine($"Value - {mText.Value}");
                    }
                    else if (selectedEntity is TextEntity textEntity)
                    {
                        entityInfo.AppendLine($"Thickness - {textEntity.Thickness}");
                        entityInfo.AppendLine($"Value - {textEntity.Value}");
                        entityInfo.AppendLine($"Rotation - {textEntity.Rotation}");
                    }
                    else if (selectedEntity is Face3D face3D)
                    {
                        entityInfo.AppendLine($"FirstCorner - {face3D.FirstCorner}");
                        entityInfo.AppendLine($"SecondCorner - {face3D.SecondCorner}");
                        entityInfo.AppendLine($"ThirdCorner - {face3D.ThirdCorner}");
                        entityInfo.AppendLine($"FourthCorner - {face3D.FourthCorner}");
                    }
                    else if (selectedEntity is Arc arc)
                    {
                        entityInfo.AppendLine($"FirstCorner - {arc.StartAngle}");
                        entityInfo.AppendLine($"SecondCorner - {arc.EndAngle}");
                    }
                    else if (selectedEntity is Solid solid)
                    {
                        entityInfo.AppendLine($"FirstCorner - {solid.FirstCorner}");
                        entityInfo.AppendLine($"SecondCorner - {solid.SecondCorner}");
                        entityInfo.AppendLine($"ThirdCorner - {solid.ThirdCorner}");
                        entityInfo.AppendLine($"FourthCorner - {solid.FourthCorner}");
                        entityInfo.AppendLine($"Thickness - {solid.Thickness}");
                    }
                    else if (selectedEntity is ACadSharp.Entities.Ellipse ellipse)
                    {
                        entityInfo.AppendLine($"Thickness - {ellipse.Thickness}");
                        entityInfo.AppendLine($"Center - {ellipse.Center}");
                        entityInfo.AppendLine($"EndPoint - {ellipse.EndPoint}");
                        entityInfo.AppendLine($"RadiusRatio - {ellipse.RadiusRatio}");
                        entityInfo.AppendLine($"StartParameter - {ellipse.StartParameter}");
                        entityInfo.AppendLine($"EndParameter - {ellipse.EndParameter}");
                    }

                    else if (selectedEntity is LwPolyline lwPolyline)
                    {
                        entityInfo.AppendLine($"Elevation - {lwPolyline.Elevation}");
                        entityInfo.AppendLine($"Thickness - {lwPolyline.Thickness}");
                        foreach (var vertex in lwPolyline.Vertices)
                        {
                            entityInfo.AppendLine($"Location X - {vertex.Location.X}");
                            entityInfo.AppendLine($"Location Y - {vertex.Location.Y}");
                        }
                    }
                    EntityInfoTextBlock.Text = entityInfo.ToString();
                }
            }
        }

        private void SaveAsDxf_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "DXF files (*.dxf)|*.dxf",
                Title = "Uložit soubor jako DXF"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (var writer = new DxfWriter(saveFileDialog.FileName, _doc, false))
                {
                    writer.Write();
                }
            }
        }


        // Přidání nové funkce pro vykreslení všech vrstev
        private void DrawAllLayers_Click(object sender, RoutedEventArgs e)
        {
            if (_doc != null)
            {
                DrawEntities(_doc.Entities);
            }
        }

        // Funkce pro vykreslení entit na Canvas
        // Funkce pro vykreslení entit na Canvas
        private void DrawEntities(IEnumerable<Entity> entities)
        {
            DrawingCanvas.Children.Clear();

            double scale = 0.1;
            double offsetX = 100;
            double offsetY = 100;

            foreach (var entity in entities)
            {
                if (entity is ACadSharp.Entities.Line line)
                {
                    System.Windows.Shapes.Line wpfLine = new System.Windows.Shapes.Line
                    {
                        X1 = line.StartPoint.X * scale + offsetX,
                        Y1 = line.StartPoint.Y * scale + offsetY,
                        X2 = line.EndPoint.X * scale + offsetX,
                        Y2 = line.EndPoint.Y * scale + offsetY,
                        Stroke = Brushes.Black,
                        StrokeThickness = 2
                    };
                    DrawingCanvas.Children.Add(wpfLine);
                }
                else if (entity is ACadSharp.Entities.Circle circle)
                {
                    System.Windows.Shapes.Ellipse wpfCircle = new System.Windows.Shapes.Ellipse
                    {
                        Width = circle.Radius * 2 * scale,
                        Height = circle.Radius * 2 * scale,
                        Stroke = Brushes.Black,
                        StrokeThickness = 2
                    };
                    Canvas.SetLeft(wpfCircle, (circle.Center.X - circle.Radius) * scale + offsetX);
                    Canvas.SetTop(wpfCircle, (circle.Center.Y - circle.Radius) * scale + offsetY);
                    DrawingCanvas.Children.Add(wpfCircle);
                }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
