using System.Windows.Controls;

namespace ArcProViewer.ProjectTree
{
    public class ProjectViewLayer
    {
        public readonly TreeViewItem LayerNode;
        public readonly bool Visible;

        public ProjectViewLayer(TreeViewItem layerNode, bool visible)
        {
            LayerNode = layerNode;
            Visible = visible;
        }
    }
}
