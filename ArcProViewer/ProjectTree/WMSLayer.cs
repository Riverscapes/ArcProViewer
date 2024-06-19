﻿
using System;

namespace ArcProViewer.ProjectTree
{
    class WMSLayer : BaseDataset, IGISLayer
    {
        public readonly string URL;

        public string GISPath { get { return URL; } }
        public string SymbologyKey { get { return string.Empty; } }

        public override bool Exists
        {
            get { return true; }
        }

        public WMSLayer(string name, string url, int imageIndex, string id)
            : base(name, "satellite16.png", "satellite16.png", id)
        {
            URL = url;
        }
    }
}
