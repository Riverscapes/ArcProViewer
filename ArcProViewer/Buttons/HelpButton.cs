using System;
using System.Windows;
using System.Diagnostics;
using ArcGIS.Desktop.Framework.Contracts;

namespace ArcProViewer.Buttons
{
    internal class HelpButton : Button
    {
        protected override void OnClick()
        {
            try
            {
                Process.Start(new ProcessStartInfo(Properties.Resources.HelpUrl) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Launching Help");
            }
        }
    }
}
