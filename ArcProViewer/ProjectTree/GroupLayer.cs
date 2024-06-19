

namespace ArcProViewer.ProjectTree
{
    public class GroupLayer: ITreeItem
    {
        public string Name { get; }
        public readonly bool Collapse;
        public readonly string Id;

        public string ImagePath => "folder16.png";

        public GroupLayer(string label, bool collapse, string id)
        {
            Name = label;
            Collapse = collapse;
            Id = id;
        }
    }
}
