using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Framework.Utilities;
using ArcGIS.Desktop.Internal.Mapping.TOC;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcProViewer.ProjectTree;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;

namespace ArcProViewer
{
    internal class ProjectExplorerDockpaneViewModel : DockPane, INotifyPropertyChanged
    {
        private const string _dockPaneID = "ArcProViewer_ProjectExplorerDockpane";

        /// <summary>
        /// User's RAVE AppData Folder
        /// </summary>
        /// <remarks>
        /// C:\Users\USERNAME\AppData\Roaming\RAVE</remarks>
        public static DirectoryInfo AppDataFolder { get { return new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Properties.Resources.AppDataFolder)); } }

        /// <summary>
        /// Software deployment folder
        /// </summary>
        public static DirectoryInfo DeployFolder { get { return new DirectoryInfo(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "RiverscapesXML")); } }

        public ICommand AddToMap { get; }
        public ICommand LayerMetaData { get; }
        public ICommand BrowseFolder { get; }
        public ICommand AddAllLayersToMap { get; }
        public ICommand DataExchange { get; }
        public ICommand OpenFile { get; }
        public ICommand Refresh { get; }
        public ICommand Close { get; }
        public ICommand AddViewToMap { get; }

        private ObservableCollection<TreeViewItemModel> treeViewItems;
        public ObservableCollection<TreeViewItemModel> TreeViewItems
        {
            get => treeViewItems;
            set
            {
                SetProperty(ref treeViewItems, value);
            }
        }

        public ProjectExplorerDockpaneViewModel()
        {
            TreeViewItems = new ObservableCollection<TreeViewItemModel>();

            AddToMap = new ContextMenuCommand(ExecuteAddToMap, CanExecuteAddToMap);
            LayerMetaData = new ContextMenuCommand(ExecuteLayerMetaData, CanExecuteLayerMetaData);
            BrowseFolder = new ContextMenuCommand(ExecuteBrowseFolder, CanExecuteBrowseFolder);
            AddAllLayersToMap = new ContextMenuCommand(ExecuteAddAllLayersToMap, CanExecuteAddAllLayersToMap);
            OpenFile = new ContextMenuCommand(ExecuteOpenFile, CanExecuteOpenFile);
            DataExchange = new ContextMenuCommand(ExecuteDataExchange, CanExecuteDataExchange);
            Refresh = new ContextMenuCommand(ExecuteRefresh, CanExecuteRefresh);
            Close = new ContextMenuCommand(ExecuteClose, CanExecuteClose);
            AddViewToMap = new ContextMenuCommand(ExecuteAddViewToMap, CanExecuteAddViewToMap);

            try
            {
                RefreshBaseMaps();
            }
            catch
            {
                Console.WriteLine("Failed to load Basemaps");
            }
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Riverscapes Viewer";
        public string Heading
        {
            get => _heading;
            set => SetProperty(ref _heading, value);
        }

        internal static void LoadProject(string filePath)
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            ProjectExplorerDockpaneViewModel pevm = (ProjectExplorerDockpaneViewModel)pane;
            //ProjectExplorerDockpaneView content = pevm.Content as ProjectExplorerDockpaneView;

            // Detect if project is already in tree and simply select the node and return;
            foreach (TreeViewItemModel rootNod in pevm.TreeViewItems)
            {
                if (rootNod.Item is RaveProject && ((RaveProject)rootNod.Item).IsSame(filePath))
                {
                    // TODO: select the existing node
                    return;
                }
            }

            // TODO: temp variable to get it compiling. Recode context menus
            ContextMenu cmsProject = null;

            RaveProject newProject = new RaveProject(filePath);
            newProject.Name = pevm.GetUniqueProjectName(newProject);

            TreeViewItemModel projectItem = new TreeViewItemModel(newProject, null);
            projectItem.IsExpanded = true;
            try
            {
                newProject.BuildProjectTree(projectItem, cmsProject);
                pevm.treeViewItems.Insert(0, projectItem);
            }
            catch (Exception ex)
            {
                //TODO: show reason that project tree failed to build
                return;
            }

            // Load default project view
            if (Properties.Settings.Default.LoadDefaultProjectView)
            {
                try
                {
                    // Find the default project view among all the tree nodes
                    List<TreeViewItemModel> allNodes = new List<TreeViewItemModel>();
                    foreach (TreeViewItemModel node in projectItem.Children)
                        TreeViewItemModel.GetAllNodes(allNodes, node);

                    TreeViewItemModel nodDefault = allNodes.FirstOrDefault(x => x.Item is ProjectView && ((ProjectView)x.Item).IsDefaultView);
                    if (nodDefault is TreeViewItemModel)
                    {
                        // TODO: GIS
                        //AddChildrenToMap(nodDefault);
                    }
                }
                catch (Exception ex)
                {
                    // Loading the default project view is optional. Do nothing in production
                    System.Diagnostics.Debug.Assert(false, ex.Message);
                }
            }
        }

        public static void CloseAllProjects()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            ProjectExplorerDockpaneViewModel pevm = (ProjectExplorerDockpaneViewModel)pane;
            var deleteList = pevm.treeViewItems.Where(x => x.Item is RaveProject).ToList();
            deleteList.ForEach(x => pevm.ExecuteClose(x));
        }

        /// <summary>
        /// Get a unique name for a project suitable for use in project tree
        /// </summary>
        /// <param name="originalName">The name of the project from the XML</param>
        /// <returns>If a project with the same name exists in the project tree
        /// already then this method will return the original name plus a unique suffix</returns>
        private string GetUniqueProjectName(RaveProject proj)
        {
            int occurences = 0;
            foreach (TreeViewItemModel nod in TreeViewItems)
            {
                if (nod.Item is RaveProject && nod.Item != proj)
                {
                    if (nod.Name.StartsWith(proj.Name))
                        occurences++;
                }
            }

            if (occurences > 0)
                return string.Format("{0} Copy {1}", proj.OriginalName, occurences);
            else
                return proj.OriginalName;
        }

        private void CollapseChildren(object parameter)
        {
            // Your action logic here
            // For example: System.Windows.MessageBox.Show("Action 1 executed");
            System.Windows.MessageBox.Show("Action 1 executed");
        }

        private void AddLayerToMap(TreeViewItemModel node, bool recursive)
        {
            if (node.Item is IGISLayer)
                GISUtilities.AddToMapAsync(node);

            if (recursive && node.Children != null && node.Children.Count > 0)
            {
                node.Children.ToList().ForEach(x => AddLayerToMap(x, recursive));
            }
        }

        #region Context Menu Commands

        private void ExecuteAddToMap(object parameter)
        {
            AddLayerToMap(parameter as TreeViewItemModel, false);
        }

        private bool CanExecuteAddToMap(object parameter)
        {
            // Your logic to determine if the command can execute
            // For example, always return true for now
            return true;
        }

        private void ExecuteLayerMetaData(object parameter)
        {
            if (parameter is TreeViewItemModel)
            {
                TreeViewItemModel node = (TreeViewItemModel)parameter;
                if (node.Item is ProjectTree.IMetadata)
                {
                    ProjectTree.IMetadata metadata = (ProjectTree.IMetadata)node.Item;
                    var metadataWindow = new MetadataWindow(node.Item is RaveProject);
                    MetadataViewModel model = metadataWindow.DataContext as MetadataViewModel;
                    foreach (KeyValuePair<string, string> item in metadata.Metadata)
                        model.Items.Add(item);

                    metadataWindow.ShowDialog();
                }
            }
        }

        private bool CanExecuteLayerMetaData(object parameter)
        {
            if (parameter is TreeViewItemModel)
            {
                var project = ((TreeViewItemModel)parameter).Item as RaveProject;
                return project is RaveProject && project.Metadata != null && project.Metadata.Count > 0;
            }
            return false;
        }

        private void ExecuteBrowseFolder(object parameter)
        {
            var node = parameter as TreeViewItemModel;
            if (node != null)
            {
                DirectoryInfo dir = null;
                if (node.Item is FileSystemDataset)
                {
                    dir = ((FileSystemDataset)node.Item).WorkspacePath;
                }
                else if (node.Item is RaveProject)
                {
                    dir = ((RaveProject)node.Item).Folder;
                }

                if (dir != null && dir.Exists)
                {
                    Process.Start(new ProcessStartInfo(dir.FullName) { UseShellExecute = true });
                }
            }
        }

        private bool CanExecuteBrowseFolder(object parameter)
        {
            var node = parameter as TreeViewItemModel;
            if (node != null)
            {
                DirectoryInfo dir = null;
                if (node.Item is FileSystemDataset)
                {
                    dir = ((FileSystemDataset)node.Item).WorkspacePath;
                }
                else if (node.Item is RaveProject)
                {
                    dir = ((RaveProject)node.Item).Folder;
                }

                return dir.Exists;
            }

            return false;
        }

        private void ExecuteAddAllLayersToMap(object parameter)
        {
            AddLayerToMap(parameter as TreeViewItemModel, true);
        }

        private bool CanExecuteAddAllLayersToMap(object parameter)
        {
            if (parameter is TreeViewItemModel)
            {
                var node = parameter as TreeViewItemModel;
                return node.Children != null && node.Children.Count > 0;
            }

            return false;
        }
        private void ExecuteOpenFile(object parameter)
        {
            var node = parameter as TreeViewItemModel;
            if (node != null && node.Item is FileSystemDataset)
            {
                var dataset = (FileSystemDataset)node.Item;
                if (dataset.Exists)
                    Process.Start(new ProcessStartInfo(dataset.Path.FullName) { UseShellExecute = true });
            }
        }

        private bool CanExecuteOpenFile(object parameter)
        {
            var node = parameter as TreeViewItemModel;
            if (node != null && node.Item is FileSystemDataset)
            {
                var dataset = (FileSystemDataset)node.Item;
                return dataset.Exists;
            }

            return false;
        }

        private void ExecuteDataExchange(object parameter)
        {
            var node = parameter as TreeViewItemModel;
            if (node != null && node.Item is RaveProject)
            {
                var project = (RaveProject)node.Item;
                if (!string.IsNullOrEmpty(project.WarehouseId))
                    Process.Start(new ProcessStartInfo(project.WarehouseUri.ToString()) { UseShellExecute = true });
            }
        }

        private bool CanExecuteDataExchange(object parameter)
        {
            var node = parameter as TreeViewItemModel;
            if (node != null && node.Item is RaveProject)
            {
                var project = (RaveProject)node.Item;
                return !string.IsNullOrEmpty(project.WarehouseId);
            }

            return false;
        }


        private void ExecuteRefresh(object parameter)
        {
            if (parameter is TreeViewItemModel)
            {
                TreeViewItemModel projectNode = (TreeViewItemModel)parameter;
                RaveProject project = projectNode.Item as RaveProject;
                if (project is RaveProject)
                {
                    string filePath = project.ProjectFile.FullName;
                    TreeViewItems.Remove(projectNode);
                    LoadProject(filePath);
                }
            }
        }

        private bool CanExecuteRefresh(object parameter)
        {
            return parameter is TreeViewItemModel && ((TreeViewItemModel)parameter).Item is RaveProject;
        }

        private void ExecuteClose(object parameter)
        {
            if (parameter is TreeViewItemModel)
            {
                TreeViewItemModel projectNode = (TreeViewItemModel)parameter;
                RaveProject project = projectNode.Item as RaveProject;
                if (project is RaveProject)
                {
                    try
                    {
                        GISUtilities.RemoveGroupLayer(project.Name, null);
                    }
                    catch (Exception ex)
                    {
                        // Proceed even though there will be lingering items in map ToC
                        Console.WriteLine("Failed to remove project group layer from map.");
                    }

                    TreeViewItems.Remove(projectNode);
                }
            }
        }

        private bool CanExecuteClose(object parameter)
        {
            return parameter is TreeViewItemModel && ((TreeViewItemModel)parameter).Item is RaveProject;
        }

        private void ExecuteAddViewToMap(object parameter)
        {
            if (parameter is TreeViewItemModel)
            {
                TreeViewItemModel projectNode = (TreeViewItemModel)parameter;
                ProjectView view = projectNode.Item as ProjectView;
                if (view is ProjectView)
                {
                    view.Layers.ForEach(x => AddLayerToMap(x.LayerNode, false));
                }
            }
        }

        private bool CanExecuteAddViewToMap(object parameter)
        {
            return parameter is TreeViewItemModel && ((TreeViewItemModel)parameter).Item is ProjectView;
        }

        #endregion

        #region Basemaps

        public void RefreshBaseMaps()
        {
            // Remove existing Basemap group
            TreeViewItemModel bGroup = TreeViewItems.FirstOrDefault(x => x.Item is BasemapGroup);
            if (bGroup != null)
                TreeViewItems.Remove(bGroup);

            // Exit if no base maps are required
            if (!Properties.Settings.Default.LoadBaseMaps || string.IsNullOrEmpty(Properties.Settings.Default.BaseMap))
                return;

            List<string> searchFolders = new List<string>() {
                AppDataFolder.FullName,
                DeployFolder.FullName,
            };

            foreach (string folder in searchFolders)
            {
                string baseMapPath = Path.Combine(folder, "BaseMaps.xml");
                if (File.Exists(baseMapPath))
                {
                    try
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(baseMapPath);
                        XmlNode nodRegion = xmlDoc.SelectSingleNode(string.Format("BaseMaps/Region[@name='{0}']", Properties.Settings.Default.BaseMap));
                        if (nodRegion is XmlNode)
                        {
                            var group = new BasemapGroup();
                            TreeViewItemModel newGroup = new TreeViewItemModel(group, null);
                            TreeViewItems.Add(newGroup);

                            LoadBaseMapsFromXML(newGroup, nodRegion);
                            return;
                        }
                    }
                    catch
                    {
                        // Do nothing. Proceed to next base map file
                    }
                }
            }
        }

        private void LoadBaseMapsFromXML(TreeViewItemModel nodParent, XmlNode nodXML)
        {
            foreach (XmlNode node in nodXML.ChildNodes)
            {
                try
                {
                    if (string.Compare(node.Name, "GroupLayer", true) == 0)
                    {
                        var group = new ProjectTree.GroupLayer(node.Attributes["name"].InnerText, true, "");
                        TreeViewItemModel groupNode = new TreeViewItemModel(group, nodParent);
                        if (nodParent.Children == null)
                            nodParent.Children = new ObservableCollection<TreeViewItemModel>();
                        nodParent.Children.Add(groupNode);

                        LoadBaseMapsFromXML(groupNode, node);
                    }
                    else if (string.Compare(node.Name, "Layer", true) == 0)
                    {
                        // Skip all but WMS layers
                        XmlAttribute attType = node.Attributes["type"];
                        if (attType is XmlAttribute && !string.IsNullOrEmpty(attType.InnerText))
                        {
                            if (string.Compare("wms", attType.InnerText, true) != 0)
                                continue;
                        }

                        if (nodParent.Children == null)
                            nodParent.Children = new ObservableCollection<TreeViewItemModel>();

                        var layer = new ProjectTree.WMSLayer(node.Attributes["name"].InnerText, node.Attributes["url"].InnerText, 0, string.Empty);
                        TreeViewItemModel newNode = new TreeViewItemModel(layer, nodParent);
                        nodParent.Children.Add(newNode);
                    }
                }
                catch (Exception ex)
                {
                    // Do nothing. Proceed to next XML node
                }
            }
        }
        #endregion
    }
}
