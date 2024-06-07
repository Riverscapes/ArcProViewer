using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Mapping;
using ArcProViewer.ProjectTree;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Threading.Tasks;

namespace ArcProViewer
{
    public struct GISUtilities
    {
        public static async Task AddToMapAsync(FileSystemDataset dataset, string sLayerName, ArcGIS.Desktop.Mapping.GroupLayer grpLayer)
        {
            await QueuedTask.Run(() =>
            {
                // Check if the layer file exists
                if (!dataset.Exists)
                    throw new FileNotFoundException("The dataset workspace file does not exist.", dataset.Path.FullName);

                // Create a new layer from the layer file
                var layer = LayerFactory.Instance.CreateLayer(new Uri(dataset.Path.FullName), MapView.Active.Map);
                if (layer == null)
                    throw new InvalidOperationException("Failed to create layer from the layer file.");

                // Get the current map
                var map = MapView.Active.Map;

                // Add the layer to the map
                map.Layers.Append(layer);
            });
        }
    }
}