using ArcGIS.Desktop.Framework.Contracts;
using System.Diagnostics;

namespace ArcProViewer.Buttons
{
    internal class HelpButton : Button
    {
        protected override void OnClick()
        {
            Process.Start(new ProcessStartInfo(Properties.Resources.HelpUrl) { UseShellExecute = true });
        }
    }
}
