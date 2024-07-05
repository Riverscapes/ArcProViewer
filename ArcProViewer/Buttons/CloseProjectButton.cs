using System;
using System.Windows;
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
                MessageBox.Show(ex.Message, "Error Closing All Projects");
            }
        }
    }
}