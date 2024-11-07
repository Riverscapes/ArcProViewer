using System;
using System.Windows;
using System.Diagnostics;
using ArcGIS.Desktop.Framework.Contracts;

namespace ArcProViewer.Buttons
{
    internal class FindResourcesFolderButton : Button
    {
        protected override void OnClick()
        {
            try
            {
                string folder = System.IO.Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), Properties.Resources.AppDataFolder);
                if (System.IO.Directory.Exists(folder))
                {
                    Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Finding Resources Folder");
            }
        }
    }
}
