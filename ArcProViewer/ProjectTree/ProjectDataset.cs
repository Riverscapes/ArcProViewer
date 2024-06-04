using System.IO;

namespace ArcProViewer.ProjectTree
{
    public class ProjectDataset
    {
        public readonly RaveProject Project;
        public readonly string Name;
        public readonly FileSystemInfo FilePath; // TINs

        public ProjectDataset(RaveProject project, FileSystemInfo filePath, string name)
        {
            Project = project;
            FilePath = filePath;
            Name = name;
        }
    }
}
