using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework.Contracts;

namespace ArcProViewer.Buttons
{
    internal class ProjectExporerButton : Button
    {
        protected override void OnClick()
        {
            ProjectExplorerDockpaneViewModel.Show();
        }
    }
}
