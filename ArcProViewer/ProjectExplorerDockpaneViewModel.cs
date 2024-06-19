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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ArcProViewer
{
    internal class ProjectExplorerDockpaneViewModel : DockPane, INotifyPropertyChanged
    {
        private const string _dockPaneID = "ArcProViewer_ProjectExplorerDockpane";

        private ObservableCollection<TreeViewItemModel> treeViewItems;
        public ObservableCollection<TreeViewItemModel> TreeViewItems
        {
            get => treeViewItems;
            set
            {
                SetProperty(ref treeViewItems, value);
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public ProjectExplorerDockpaneViewModel()
        {
            TreeViewItems = new ObservableCollection<TreeViewItemModel>();
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
        public static DirectoryInfo AppDataFolder { get { return new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Properties.Resources.AppDataFolder)); } }

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
            try
            {
                newProject.BuildProjectTree(projectItem, cmsProject);
                pevm.treeViewItems.Add(projectItem);
            }
            catch(Exception ex)
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

            //TODO
            //AssignContextMenus(tnProject);
        }

        //int test = content.treProject.Items.Count;
        //int test2 = pevm.TreeViewItems.Count;

        //content.LoadProject(filePath);


        //internal void LoadProject2(string filePath)
        //{
        //    TreeViewItems = new ObservableCollection<TreeViewItemModel>();

        //    TreeViewItemModel item = new TreeViewItemModel();
        //    item.Header = "test";
        //    item.ImagePath = "Images/viewer16.png";
        //    item.Children = new ObservableCollection<TreeViewItemModel>
        //    {
        //       new TreeViewItemModel
        //        {
        //            Header = "Item 2",
        //            ImagePath = "Images/viewer16.png",
        //            Children = new ObservableCollection<TreeViewItemModel>
        //            {
        //                new TreeViewItemModel { Header = "SubItem 2.1", ImagePath = "Images/viewer16.png" }
        //            }
        //        }
        //    };
        //    TreeViewItems.Add(item);

        //    OnPropertyChanged(nameof(TreeViewItems));
        //}

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
    }
}
