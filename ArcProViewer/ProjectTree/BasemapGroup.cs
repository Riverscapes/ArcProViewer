using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ArcProViewer.ProjectTree
{
    public class BasemapGroup : ITreeItem
    {
        public BasemapGroup()
        { }

        public string Name => Properties.Resources.BasemapsLabel;

        public string ImagePath => "basemap16.png";
    }
}
