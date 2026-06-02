using System;
using System.Diagnostics;
using System.Windows;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace ArcProViewer.Buttons
{
    internal class DataExchangeButton : Button
    {
        private const string RootUrl = "https://data.riverscapes.net/s?type=Project&view=map";

        protected override void OnClick()
        {
            try
            {
                if (MapView.Active?.Extent == null)
                {
                    Process.Start(new ProcessStartInfo(RootUrl) { UseShellExecute = true });
                    return;
                }

                QueuedTask.Run(() =>
                {
                    var extent = MapView.Active.Extent;
                    var centroid = GeometryEngine.Instance.Centroid(extent);
                    var wgs84 = SpatialReferences.WGS84;
                    var projected = GeometryEngine.Instance.Project(centroid, wgs84) as MapPoint;

                    if (projected == null)
                        return;

                    double lon = projected.X;
                    double lat = projected.Y;

                    // Approximate zoom level from map scale
                    double scale = MapView.Active.Camera.Scale;
                    int zoom = ScaleToZoom(scale);

                    string url = $"{RootUrl}&geo={lon}%2C{lat}%2C{zoom}";
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Opening Data Exchange");
            }
        }

        private static int ScaleToZoom(double scale)
        {
            // Approximate conversion from map scale denominator to web map zoom level
            if (scale <= 0) return 5;
            int zoom = (int)Math.Round(Math.Log(591657550.5 / scale, 2));
            return Math.Max(0, Math.Min(zoom, 20));
        }
    }
}
