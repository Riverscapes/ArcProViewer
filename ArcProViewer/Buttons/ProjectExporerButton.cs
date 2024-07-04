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
