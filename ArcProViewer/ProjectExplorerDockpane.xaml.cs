using ArcProViewer.ProjectTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


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

        private void AddChildrenToMap(TreeViewItemModel e)
        {
            e.Children.OfType<TreeViewItemModel>().ToList().ForEach(x => AddChildrenToMap(x));

            GISDataset ds = null;

            if (e.Item is GISDataset)
            {
                ds = e.Item as GISDataset;
            }
            else if (e.Item is ProjectView)
            {
                ((ProjectView)e.Item).Layers.ForEach(x => AddChildrenToMap(x.LayerNode));
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
