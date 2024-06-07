using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Navigation;

namespace ArcProViewer.ProjectTree
{
    class Raster : GISDataset
    {
        public Raster(RaveProject project, string name, string path, string symbology, short transparency, string id, Dictionary<string, string> metadata)
            : base(project, name, new FileInfo(path), symbology, transparency, "raster16", "raster16", id, metadata)
        {

        }

        public override Uri GISUri => new Uri(this.GISPath);

    }
}
