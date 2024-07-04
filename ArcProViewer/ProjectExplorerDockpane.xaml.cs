using ArcProViewer.ProjectTree;
using System;
using System.Collections.Generic;
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
            this.Name = "test";

            this.DataContext = new ProjectExplorerDockpaneViewModel();
        }

        private void treProject_DoubleClick(object sender, EventArgs e)
        {
            if (treProject.SelectedItem is TreeViewItemModel)
            {

                TreeViewItemModel selNode = treProject.SelectedItem as TreeViewItemModel;
                if (selNode.Item is IGISLayer)
                {
                    // TODO: GIS
                    OnAddGISToMap(sender, e);
                }
                else if (selNode.Item is FileSystemDataset)
                {
                    //TODO: GIS
                    //OnOpenFile(sender, e);
                }
                else if (selNode.Item is ProjectView)
                {
                    // TODO: GIS
                    //OnAddChildrenToMap(sender, e);
                }
            }
        }

        public async Task OnAddGISToMap(object sender, EventArgs e)
        {
            TreeViewItemModel selNode = treProject.SelectedItem as TreeViewItemModel;

            // TODO: GIS
            //IGroupLayer parentGrpLyr = BuildArcMapGroupLayers(selNode);
            //FileInfo symbology = GetSymbology(layer);

            string def_query = string.Empty;

            if (selNode.Item is ProjectTree.Vector)
                def_query = ((ProjectTree.Vector)selNode.Item).DefinitionQuery;

            try
            {
                int index = selNode.Parent.Children.IndexOf(selNode);
                await GISUtilities.AddToMapAsync(selNode, index);
                //GISUtilities.AddToMap(layer, layer.Name, parentGrpLyr, GetPrecedingLayers(selNode), symbology, transparency: layer.Transparency, definition_query: def_query);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}\n\n{1}", ex.Message, ((IGISLayer)selNode.Item).GISPath), "Error Adding Dataset To Map", MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
