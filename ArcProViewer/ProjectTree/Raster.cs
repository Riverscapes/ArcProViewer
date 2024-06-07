using System.Collections.Generic;
using System.IO;

namespace ArcProViewer.ProjectTree
{
    class Raster : GISDataset
    {
        public Raster(RaveProject project, string name, string path, string symbology, short transparency, string id, Dictionary<string, string> metadata)
            : base(project, name, new FileInfo(path), symbology, transparency, "raster16", "raster16", id, metadata)
        {

        }
    }
}
