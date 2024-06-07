
namespace ArcProViewer.ProjectTree
{
    public abstract class BaseDataset
    {
        public string Name { get; private set; }
        public string Id { get; private set; }

        // No folder and no file extension
        private readonly string ImageFileNameExists;
        private readonly string ImageFileNameMissing;

        public abstract bool Exists { get; }

        public string ImageFileName { get { return Exists ? ImageFileNameExists : ImageFileNameMissing; } }

        public BaseDataset(string name, string imageFileNameExists, string imageFileNameMissing, string id)
        {
            Name = name;
            ImageFileNameExists = imageFileNameExists;
            ImageFileNameMissing = imageFileNameExists;
            Id = id;
        }
    }
}
