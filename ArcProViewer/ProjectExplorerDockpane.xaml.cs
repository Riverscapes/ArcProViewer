using ArcProViewer.ProjectTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace ArcProViewer
{
    /// <summary>
    /// Interaction logic for ProjectExplorerDockpaneView.xaml
    /// </summary>
    public partial class ProjectExplorerDockpaneView : UserControl
    {
        public ProjectExplorerDockpaneView()
        {
            InitializeComponent();

            this.DataContext = new ProjectExplorerDockpaneViewModel();
        }

        private void treProject_DoubleClick(object sender, EventArgs e)
        {
            if (treProject.SelectedItem is TreeViewItem)
            {
                TreeViewItem selNode = treProject.SelectedItem as TreeViewItem;
                if (selNode.Tag is GISDataset)
                {
                    // TODO: GIS
                    OnAddGISToMap(sender, e);
                }
                else if (selNode.Tag is FileSystemDataset)
                {
                    //TODO: GIS
                    //OnOpenFile(sender, e);
                }
                else if (selNode.Tag is ProjectView)
                {
                    // TODO: GIS
                    //OnAddChildrenToMap(sender, e);
                }
                else if (selNode.Tag is WMSLayer)
                {
                    // TODO: GIS
                    //OnAddWMSToMap(sender, e);
                }
            }
        }

        public async Task OnAddGISToMap(object sender, EventArgs e)
        {
            TreeViewItem selNode = treProject.SelectedItem as TreeViewItem;
            GISDataset layer = (GISDataset)selNode.Tag;

            // TODO: GIS
            //IGroupLayer parentGrpLyr = BuildArcMapGroupLayers(selNode);
            //FileInfo symbology = GetSymbology(layer);

            string def_query = layer is ProjectTree.Vector ? ((ProjectTree.Vector)layer).DefinitionQuery : string.Empty;

            try
            {
                await GISUtilities.AddToMapAsync(layer, layer.Name, null);
                //GISUtilities.AddToMap(layer, layer.Name, parentGrpLyr, GetPrecedingLayers(selNode), symbology, transparency: layer.Transparency, definition_query: def_query);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}\n\n{1}", ex.Message, layer.Path.FullName), "Error Adding Dataset To Map", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            finally
            {
                //Cursor.Current = Cursors.Default;
            }
        }


        public void CloseAllProjects()
        {

            // Detect if project is already in tree and simply select the node and return;
            List<TreeViewItem> projects = new List<TreeViewItem>();
            foreach (TreeViewItem rootNod in treProject.Items)
            {
                if (rootNod.Tag is RaveProject)
                {
                    projects.Add(rootNod);

                    // Remove the project from the map. SearchRecursive = False
                    // will ensure it only looks at the top level of the map ToC
                    // TODO: GIS
                    //ArcMapUtilities.RemoveGroupLayer(rootNod.Text, false);
                }
            }

            foreach (TreeViewItem rootNod in projects)
            {
                treProject.Items.Remove(Parent);
            }
        }

        /// <summary>
        /// Get a unique name for a project suitable for use in project tree
        /// </summary>
        /// <param name="originalName">The name of the project from the XML</param>
        /// <returns>If a project with the same name exists in the project tree
        /// already then this method will return the original name plus a unique suffix</returns>
        private string GetUniqueProjectName(RaveProject proj, string originalName)
        {
            int occurences = 0;
            foreach (TreeViewItem nod in treProject.Items)
            {
                if (nod.Tag is RaveProject && nod.Tag != proj)
                {
                    if (nod.Header.ToString().StartsWith(originalName))
                        occurences++;
                }
            }

            if (occurences > 0)
                return string.Format("{0} Copy {1}", originalName, occurences);
            else
                return originalName;
        }

        public void LoadProject(string projectFile)
        {
            // Detect if project is already in tree and simply select the node and return;
            foreach (TreeViewItem rootNod in treProject.Items)
            {
                if (rootNod.Tag is RaveProject && RaveProject.IsSame((RaveProject)rootNod.Tag, projectFile))
                {
                    rootNod.IsSelected = true;
                    rootNod.Focus();
                    return;
                }
            }

            // TODO: temp variable to get it compiling. Recode context menus
            ContextMenu cmsProject = null;

            RaveProject newProject = new RaveProject(projectFile);
            TreeViewItem tnProject = newProject.LoadNewProject(treProject, cmsProject);
            tnProject.Header = GetUniqueProjectName(newProject, tnProject.Header.ToString());

            // Load default project view
            if (Properties.Settings.Default.LoadDefaultProjectView)
            {
                try
                {
                    // Find the default project view among all the tree nodes
                    List<TreeViewItem> allNodes = new List<TreeViewItem>();
                    foreach (TreeViewItem node in tnProject.Items)
                        RaveProject.GetAllNodes(allNodes, node);

                    TreeViewItem nodDefault = allNodes.FirstOrDefault(x => x.Tag is ProjectTree.ProjectView && ((ProjectTree.ProjectView)x.Tag).IsDefaultView);
                    if (nodDefault is TreeViewItem)
                    {
                        AddChildrenToMap(nodDefault);
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

        private void AddChildrenToMap(TreeViewItem e)
        {
            e.Items.OfType<TreeViewItem>().ToList().ForEach(x => AddChildrenToMap(x));

            GISDataset ds = null;

            if (e.Tag is GISDataset)
            {
                ds = e.Tag as GISDataset;
            }
            else if (e.Tag is ProjectView)
            {
                ((ProjectView)e.Tag).Layers.ForEach(x => AddChildrenToMap(x.LayerNode));
            }

            if (ds is GISDataset)
            {
                // TODO: GIS
                //GISDataset layer = (GISDataset)e.Tag;
                //IGroupLayer parentGrpLyr = BuildArcMapGroupLayers(e);
                //FileInfo symbology = GetSymbology(layer);

                //string def_query = ds is Vector ? ((Vector)ds).DefinitionQuery : string.Empty;

                //ArcMapUtilities.AddToMap(layer, layer.Name, parentGrpLyr, GetPrecedingLayers(e), symbology, transparency: layer.Transparency, definition_query: def_query);
                //Cursor.Current = Cursors.Default;
            }
        }

    }

}
