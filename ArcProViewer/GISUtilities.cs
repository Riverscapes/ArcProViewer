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
using System.Windows.Controls;
using System.Xml.Linq;

namespace ArcProViewer
{
    public class GISUtilities
    {
        public enum NodeInsertModes
        {
            Add,
            Insert
        };

        public async Task AddToMapAsync(TreeViewItemModel item, int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "Layer index must be greater than or equal to zero");

            await QueuedTask.Run(async () =>
            {
                // Check if there is an active map view
                if (MapView.Active == null)
                {
                    // Check if there are any maps in the project
                    var mapProjectItems = Project.Current.GetItems<MapProjectItem>();
                    Map map = null;

                    if (mapProjectItems.Count() == 0)
                    {
                        // No maps exist, so create a new map
                        map = MapFactory.Instance.CreateMap("NewMap", MapType.Map, MapViewingMode.Map);
                    }
                    else
                    {
                        // Use the first available map
                        map = mapProjectItems.FirstOrDefault()?.GetMap();
                    }

                    // Open the map in a new map view
                    if (map != null)
                    {
                        await ProApp.Panes.CreateMapPaneAsync(map);
                    }
                }


                // At this point, there should be at least one map, so you can proceed to add layers
                Map activeMap = MapView.Active?.Map ?? Project.Current.GetItems<MapProjectItem>().FirstOrDefault()?.GetMap();


                // Attempt to find existing layer and exit if its present
                if (!string.IsNullOrEmpty(item.MapLayerUri))
                {
                    Layer existingLayer = MapView.Active.Map.FindLayer(item.MapLayerUri);
                    if (existingLayer != null)
                        return;
                }

                // Build a list of the parent groups
                var parentItems = new List<TreeViewItemModel>();
                var parentItem = item.Parent;
                while (parentItem is not null)
                {
                    parentItems.Add(parentItem);
                    parentItem = parentItem.Parent;
                }

                // Now try to find these groups, starting with the root, project entry
                parentItems.Reverse();
                ILayerContainerEdit parent = MapView.Active.Map;
                foreach (TreeViewItemModel groupItem in parentItems)
                {
                    // Attempt to find the existing ToC group layer
                    if (!string.IsNullOrEmpty(groupItem.MapLayerUri))
                    {
                        var parentLayer = MapView.Active.Map.FindLayer(groupItem.MapLayerUri);
                        if (parentLayer != null)
                        {
                            parent = parentLayer as ILayerContainerEdit;
                            continue;
                        }
                    }

                    // If this is the basemap group then find the count of project nodes
                    // and ensure that we insert this base map group node below any Riverscapes projects.
                    // For now we just add it to the bottom of the map ToC (even though this
                    // means that it will be below any ESRI added basemaps
                    int groupIndex = 0;
                    if (groupItem.Item is BasemapGroup)
                    {
                        groupIndex = MapView.Active.Map.Layers.Count;
                    }
                    else
                    {
                        
                        if (!(groupItem is ProjectTree.RaveProject) && groupItem.Parent != null)
                        {
                            // This is a group node somewhere in project hierarchy. Get its positional index
                            groupIndex = groupItem.Parent.Children.IndexOf(groupItem);
                        }
                    }

                    // If got to here then the group layer doesn't exist
                    parent = LayerFactory.Instance.CreateGroupLayer(parent, groupIndex, groupItem.Name);
                    groupItem.MapLayerUri = ((ArcGIS.Desktop.Mapping.GroupLayer)parent).URI;

                    // DO NOT ATTEMPT TO EXPAND GROUP LAYERS HERE.
                    // This was causing ghosting of groups in the Map ToC.
                }

                Uri uri = null;
                if (item.Item is GISDataset dataset)
                {
                    uri = dataset.GISUri;
                    if (!dataset.Exists)
                        throw new FileNotFoundException("The dataset workspace file does not exist.", dataset.Path.FullName);
                }
                else if (item.Item is ProjectTree.WMSLayer wmsLayer)
                {
                    uri = wmsLayer.URL;
                }

                System.Diagnostics.Debug.Print("Creating layer {0} with parent {1}. Uri: {2}", item.Name, parent, uri?.ToString());
                Layer layer = LayerFactory.Instance.CreateLayer(uri, parent as ILayerContainerEdit, index, item.Name);

                // Apply symbology
                FileInfo symbologyLayerFilePath = GetSymbologyFile(item.Item as GISDataset);
                if (symbologyLayerFilePath != null)
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
                            var rendererFromLayerFile = ((CIMFeatureLayer)cimLyrDoc.LayerDefinitions[0]).Renderer;
                            ((FeatureLayer)layer).SetRenderer(rendererFromLayerFile);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.Print($"Failed to apply symbology: {ex.Message}");
                    }
                }

                // Store the ArcPro layer URI so that we can find this layer again
                item.MapLayerUri = layer.URI;

                // Apply feature filter
                if (item.Item is Vector vector && layer is FeatureLayer featureLayer)
                {
                    if (!string.IsNullOrEmpty(vector.DefinitionQuery))
                        featureLayer.SetDefinitionQuery(vector.DefinitionQuery);
                }

                // Trace back up hierarchy and expand any group layers
                parentItem = item.Parent;
                while (parentItem is not null)
                {
                    if (!string.IsNullOrEmpty(parentItem.MapLayerUri))
                    {
                        var parentLayer = MapView.Active.Map.FindLayer(parentItem.MapLayerUri);
                        if (parentLayer is ArcGIS.Desktop.Mapping.GroupLayer)
                        {
                            var groupLayer = parentLayer as ArcGIS.Desktop.Mapping.GroupLayer;
                            if (parentItem.Item is ProjectTree.GroupLayer)
                            {
                                var parentGroupItem = parentItem.Item as ProjectTree.GroupLayer;
                                if (!groupLayer.IsExpanded && parentGroupItem.Expanded)
                                    groupLayer.SetExpanded(true);
                            }
                            else
                            {
                                groupLayer.SetExpanded(true);
                            }
                        }

                    }
                    parentItem = parentItem.Parent;
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
        public FileInfo GetSymbologyFile(GISDataset layer)
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

        public async Task RemoveGroupLayer(TreeViewItemModel item, ILayerContainer parent)
        {
            await QueuedTask.Run(async () =>
           {
               if (!string.IsNullOrEmpty(item.MapLayerUri))
               {
                   Layer layer = MapView.Active.Map.FindLayer(item.MapLayerUri);
                   if (layer != null)
                       MapView.Active.Map.RemoveLayer(layer);
               }
           });
        }
    }
}