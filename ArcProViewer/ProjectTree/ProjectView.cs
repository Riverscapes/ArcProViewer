using System;
using System.Collections.Generic;

namespace ArcProViewer.ProjectTree
{
    public class ProjectView : ITreeItem
    {
        public readonly string Id;
        public string Name { get; }
        public readonly bool IsDefaultView;
        public readonly List<ProjectViewLayer> Layers;
        public string ImagePath => "view16.png";

        public ProjectView(string id, string name, bool is_default)
        {
            Id = id;
            Name = name;
            IsDefaultView = is_default;
            Layers = new List<ProjectViewLayer>();
        }
    }
}
