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
using ArcGIS.Desktop.Internal.Mapping.Controls.QueryBuilder.SqlEditor;
using ArcGIS.Core.Data.UtilityNetwork.Trace;


namespace ArcProViewer
{
    public struct GISUtilities
    {
        public enum NodeInsertModes
        {
            Add,
            Insert
        };

        public static async Task AddToMapAsync(TreeViewItemModel item)
        {
            await QueuedTask.Run(() =>
            {
                ILayerContainer parent = BuildArcMapGroupLayers(item, NodeInsertModes.Insert);

                GISDataset dataset = item.Item as GISDataset;

                // Check if the layer file exists
                if (!dataset.Exists)
                    throw new FileNotFoundException("The dataset workspace file does not exist.", dataset.Path.FullName);

                Layer layer = parent.FindLayer(dataset.GISUri.ToString(), false);
                if (layer == null)
                {
                    layer = LayerFactory.Instance.CreateLayer(dataset.GISUri, parent as ILayerContainerEdit);
                    layer.SetName(item.Name);
                }

                if (layer == null)
                    throw new InvalidOperationException("Failed to create layer from the layer file.");


                //// Get the current map
                //var map = MapView.Active.Map;

                //// Add the layer to the map
                //map.Layers.Append(layer);
            });
        }

        public static Task<ArcGIS.Desktop.Mapping.GroupLayer> GetGroupLayer(string groupName, ILayerContainer parent)
        {
            return QueuedTask.Run(() =>
            {
                if (parent == null)
                    parent = MapView.Active.Map;

                return parent.Layers.OfType<ArcGIS.Desktop.Mapping.GroupLayer>().FirstOrDefault(gl => gl.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase));

            });
        }

        public static Task<ArcGIS.Desktop.Mapping.GroupLayer> AddGroupLayer(string groupName, int index, ILayerContainerEdit parent)
        {
            return QueuedTask.Run(async () =>
            {
                ArcGIS.Desktop.Mapping.GroupLayer result = await GetGroupLayer(groupName, parent);
                if (result != null)
                    return result;

                if (parent == null)
                    parent = MapView.Active.Map;

                ArcGIS.Desktop.Mapping.GroupLayer grpLayer = LayerFactory.Instance.CreateGroupLayer(parent, index, groupName);
                grpLayer.SetExpanded(true);
                return grpLayer;
            });
        }

        public static ILayerContainer BuildArcMapGroupLayers(TreeViewItemModel node, GISUtilities.NodeInsertModes topLevelMode = GISUtilities.NodeInsertModes.Insert)
        {
            ILayerContainer parent = node.Parent == null ? MapView.Active.Map : BuildArcMapGroupLayers(node.Parent, topLevelMode);

            if (node.Item is BaseDataset)
                return parent;
            else
                return GISUtilities.AddGroupLayer(node.Item.Name, 0, parent as ILayerContainerEdit).Result;
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