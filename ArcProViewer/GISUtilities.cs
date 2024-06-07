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
        public static async Task AddToMapAsync(GISDataset dataset, string sLayerName, ArcGIS.Desktop.Mapping.GroupLayer grpLayer)
        {
            await QueuedTask.Run(() =>
            {
                // Check if the layer file exists
                if (!dataset.Exists)
                    throw new FileNotFoundException("The dataset workspace file does not exist.", dataset.Path.FullName);

                // Create a new layer from the layer file
                var layer = LayerFactory.Instance.CreateLayer(dataset.GISUri, MapView.Active.Map);
                if (layer == null)
                    throw new InvalidOperationException("Failed to create layer from the layer file.");

                //// Get the current map
                //var map = MapView.Active.Map;

                //// Add the layer to the map
                //map.Layers.Append(layer);
            });
        }

        //public static async Task LoadLayerWithSymbologyAsync(string layerPath, string layerFilePath)
        //{
        //    await QueuedTask.Run(async () =>
        //    {
        //        // Check if the layer file exists
        //        if (!File.Exists(layerFilePath))
        //            throw new FileNotFoundException("The layer file does not exist.", layerFilePath);

        //        // Apply symbology from the layer file
        //        //await ApplySymbologyFromLayerFileAsync(layerPath, layerFilePath);

        //        // Create the new layer
        //        var layer = LayerFactory.Instance.CreateLayer(new Uri(layerPath));
        //        if (layer == null)
        //            throw new InvalidOperationException("Failed to create layer from the layer file.");

        //        // Get the current map
        //        var map = MapView.Active.Map;

        //        // Add the layer to the map's Layers collection
        //        map.Layers.Append(layer);
        //    });
        //}

        //private static async Task ApplySymbologyFromLayerFileAsync(string layerPath, string layerFilePath)
        //{
        //    await QueuedTask.Run(() =>
        //    {
        //        // Open the layer file
        //        var layerFile = LayerFactory.Instance.CreateLayerFile(layerFilePath);

        //        // Get the layer from the layer file
        //        var sourceLayer = layerFile.GetLayersAsFlattenedList()[0] as FeatureLayer;
        //        if (sourceLayer == null)
        //            throw new InvalidOperationException("The layer file does not contain a feature layer.");

        //        // Get the target layer
        //        var targetLayer = LayerFactory.Instance.CreateLayer(new Uri(layerPath)) as FeatureLayer;
        //        if (targetLayer == null)
        //            throw new InvalidOperationException("Failed to create layer from the specified path.");

        //        // Apply symbology from the source layer to the target layer
        //        targetLayer?.SetRenderer(sourceLayer.Renderer);
        //    });
        //}
    }
}