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
using static ArcProViewer.ProjectTree.RaveProject;

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


        public ProjectExplorerDockpaneViewModel() { }

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
        private string _heading = "Riverscapes Viewer xxxx";
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
            ProjectExplorerDockpaneView content = pevm.Content as ProjectExplorerDockpaneView;
            pevm.LoadProject2(filePath);

            int test = content.treProject.Items.Count;
            int test2 = pevm.TreeViewItems.Count;

            //content.LoadProject(filePath);
        }

        internal void LoadProject2(string filePath)
        {
            TreeViewItems = new ObservableCollection<TreeViewItemModel>();

            TreeViewItemModel item = new TreeViewItemModel();
            item.Header = "test";
            item.ImagePath = "Images/viewer16.png";
            item.Children = new ObservableCollection<TreeViewItemModel>
            {
               new TreeViewItemModel
                {
                    Header = "Item 2",
                    ImagePath = "Images/viewer16.png",
                    Children = new ObservableCollection<TreeViewItemModel>
                    {
                        new TreeViewItemModel { Header = "SubItem 2.1", ImagePath = "Images/viewer16.png" }
                    }
                }
            };
            TreeViewItems.Add(item);

            OnPropertyChanged(nameof(TreeViewItems));
        }
    }

public class TreeViewItemModel
{
    public string Header { get; set; }
    public string ImagePath { get; set; }
    public ObservableCollection<TreeViewItemModel> Children { get; set; }

    public TreeViewItemModel()
    {

    }
}
}
