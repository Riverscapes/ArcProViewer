using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Diagnostics;

namespace ArcProViewer.Buttons
{
    internal class FindResourcesFolderButton : Button
    {
        protected override void OnClick()
        {
            try
            {
                string folder = System.IO.Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), Properties.Resources.AppDataFolder);
                if (System.IO.Path.Exists(folder))
                {
                    Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                //TODO handle exception
            }
        }
    }
}
