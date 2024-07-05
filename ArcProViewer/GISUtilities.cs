using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Mapping;
using ArcProViewer.ProjectTree;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;

namespace ArcProViewer
{
    public struct GISUtilities
    {
        public enum NodeInsertModes
        {
            Add,
            Insert
        };

        public static async Task AddToMapAsync(TreeViewItemModel item, int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "Layer index must be greater than or equal to zero");

            await QueuedTask.Run(() =>
            {
                ILayerContainer parent = BuildArcMapGroupLayers(item, NodeInsertModes.Insert);

                Uri uri = null;
                if (item.Item is GISDataset)
                {
                    GISDataset dataset = item.Item as GISDataset;
                    uri = dataset.GISUri;
                    if (!dataset.Exists)
                        throw new FileNotFoundException("The dataset workspace file does not exist.", dataset.Path.FullName);
                }
                else if (item.Item is ProjectTree.WMSLayer)
                    uri = ((ProjectTree.WMSLayer)item.Item).URL;

                // Attempt to find the layer in the Map ToC if it has been added already and has an ArcPro URI
                Layer layer = string.IsNullOrEmpty(item.MapLayerUri) ? null : parent.FindLayer(item.MapLayerUri, false);

                if (layer == null)
                {
                    Console.WriteLine("Creating layer: {0}", uri.ToString());
                    layer = LayerFactory.Instance.CreateLayer(uri, parent as ILayerContainerEdit, index, item.Name);

                    // Apply symbology
                    FileInfo symbologyLayerFilePath = GetSymbologyFile(item.Item as GISDataset);
                    if (symbologyLayerFilePath is FileInfo)
                    {
                        try
                        {
                            // Get the Layer Document from the lyrx file
                            LayerDocument layerDoc = new LayerDocument(symbologyLayerFilePath.FullName);
                            var cimLyrDoc = layerDoc.GetCIMLayerDocument();

                            if (item.Item is ProjectTree.Raster)
                            {
                                var colorizer = ((CIMRasterLayer)cimLyrDoc.LayerDefinitions[0]).Colorizer as CIMRasterColorizer;
                                ((RasterLayer)layer).SetColorizer(colorizer);
                            }
                            else
                            {
                                //Get the renderer from the layer file
                                var rendererFromLayerFile = ((CIMFeatureLayer)cimLyrDoc.LayerDefinitions[0]).Renderer;

                                //Apply the renderer to the feature layer
                                //Note: If working with a raster layer, use the SetColorizer method.
                                ((FeatureLayer)layer).SetRenderer(rendererFromLayerFile);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to apply symbology.");

                        }
                    }

                    // Store the ArcPro layer URI so that we can find this layer again
                    item.MapLayerUri = layer.URI;

                    if (item.Item is Vector && layer is FeatureLayer)
                    {
                        Vector vector = (Vector)item.Item;
                        if (!string.IsNullOrEmpty(vector.DefinitionQuery))
                            ((FeatureLayer)layer).SetDefinitionQuery(vector.DefinitionQuery);
                    }
                }

                if (layer == null)
                    throw new InvalidOperationException("Failed to create layer from the layer file.");
            });
        }


        /// <summary>
        /// Determine the location of the layer file for this GIS item
        /// </summary>
        /// <remarks>
        /// The following locations will be searched in order for a 
        /// file with the name SYMBOLOGY_KEY.lyr
        /// 
        /// 1. ProjectFolder
        /// 2. %APPDATA%\RAVE\Symbology\esri\MODEL
        /// 3. %APPDATA%\RAVE\Symbology\esrsi\Shared
        /// 4. SOFTWARE_DEPLOYMENT\Symbology\esri\MODEL
        /// 5. SOFTWARE_DEPLOYMENT\Symbology\esri\Shared
        /// 
        /// </remarks>
        public static FileInfo GetSymbologyFile(GISDataset layer)
        {
            if (layer is null || string.IsNullOrEmpty(layer.SymbologyKey))
                return null;

            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Properties.Resources.AppDataFolder);
            string symbologyFolder = Path.Combine(appDataFolder, Properties.Resources.AppDataSymbologyFolder);

            List<string> SearchFolders = new List<string>()
            {
                layer.Project.Folder.FullName,
                Path.Combine(symbologyFolder, layer.Project.ProjectType),
                Path.Combine(symbologyFolder, Properties.Resources.AppDataSymbologySharedFolder)
            };

            foreach (string folder in SearchFolders)
            {
                if (Directory.Exists(folder))
                {
                    string path = Path.ChangeExtension(Path.Combine(folder, layer.SymbologyKey), "lyrx");
                    if (File.Exists(path))
                    {
                        return new FileInfo(path);
                    }
                }
            }

            return null;
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

                // Ensure Basemap always added at bottom and projects at the top
                if (parent == MapView.Active.Map)
                {
                    if (String.Compare(groupName, Properties.Resources.BasemapsLabel, true) == 0)
                    {
                        index = MapView.Active.Map.Layers.Count;
                    }
                    else
                    {
                        index = 0;
                    }
                }

                ArcGIS.Desktop.Mapping.GroupLayer grpLayer = LayerFactory.Instance.CreateGroupLayer(parent, index, groupName);
                grpLayer.SetExpanded(true);
                return grpLayer;
            });
        }

        public static void RemoveGroupLayer(string groupName, ILayerContainer parent)
        {
            QueuedTask.Run(() =>
           {
               // If no parent provided then look at the top level
               if (parent == null)
                   parent = MapView.Active.Map;

               ArcGIS.Desktop.Mapping.GroupLayer layer = GetGroupLayer(groupName, null).Result;
               if (layer != null)
                   ((ILayerContainerEdit)layer.Parent).RemoveLayer(layer);
           });
        }

        public static ILayerContainer BuildArcMapGroupLayers(TreeViewItemModel node, GISUtilities.NodeInsertModes topLevelMode = GISUtilities.NodeInsertModes.Insert)
        {
            //System.Diagnostics.Debug.Print("Name: {0}", node.Name);
            //System.Diagnostics.Debug.Print("Parent: {0}", node.Parent);

            ILayerContainer parent = node.Parent == null ? MapView.Active.Map : BuildArcMapGroupLayers(node.Parent, topLevelMode);

            //System.Diagnostics.Debug.Print("Parent: {0}", parent.ToString());

            if (node.Item is BaseDataset)
                return parent;
            else
                return GISUtilities.AddGroupLayer(node.Item.Name, 0, parent as ILayerContainerEdit).Result;
        }
    }
}