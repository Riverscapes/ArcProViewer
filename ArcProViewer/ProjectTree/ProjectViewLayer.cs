using System.Windows.Controls;

namespace ArcProViewer.ProjectTree
{
    public class ProjectViewLayer
    {
        public readonly TreeViewItemModel LayerNode;
        public readonly bool Visible;

        public ProjectViewLayer(TreeViewItemModel layerNode, bool visible)
        {
            LayerNode = layerNode;
            Visible = visible;
        }
    }
}
