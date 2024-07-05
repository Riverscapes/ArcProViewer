using System;
using System.Windows;
using ArcGIS.Desktop.Framework.Contracts;

namespace ArcProViewer.Buttons
{
    internal class ProjectExporerButton : Button
    {
        protected override void OnClick()
        {
            try
            {
                ProjectExplorerDockpaneViewModel.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Opening Project Explorer");
            }
        }
    }
}
