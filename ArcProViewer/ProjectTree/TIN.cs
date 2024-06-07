using System;
using System.Collections.Generic;
using System.IO;

namespace ArcProViewer.ProjectTree
{
    class TIN : GISDataset, IGISLayer
    {
        public TIN(RaveProject project, string name, string path, short transparency, string id, Dictionary<string, string> metadata)
            : base(project, name, new DirectoryInfo(path), string.Empty, transparency, "tin16", "tin16", id, metadata)
        {

        }

        public override Uri GISUri => new Uri(GISPath);
    }
}
