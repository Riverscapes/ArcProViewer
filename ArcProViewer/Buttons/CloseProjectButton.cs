using System;
using ArcGIS.Desktop.Framework.Contracts;

namespace ArcProViewer.Buttons
{
    internal class CloseProjectButton : Button
    {
        protected override void OnClick()
        {
            try
            {
                ProjectExplorerDockpaneViewModel.CloseAllProjects();
            }
            catch (Exception ex)
            {

            }
        }
    }
}