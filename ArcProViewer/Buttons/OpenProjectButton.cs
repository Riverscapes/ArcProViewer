using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcProViewer.Buttons
{
    internal class OpenProjectButton : Button
    {
        protected override void OnClick()
        {
            try
            {
                OpenFileDialog f = new OpenFileDialog();
                f.DefaultExt = "xml";
                f.Filter = "Riverscapes Project Files (*.rs.xml)|*.rs.xml";
                f.Title = "Open Existing Riverscapes Project";
                f.CheckFileExists = true;

                if (!string.IsNullOrEmpty(Properties.Settings.Default.LastUsedProjectFolder) && Directory.Exists(Properties.Settings.Default.LastUsedProjectFolder))
                {
                    f.InitialDirectory = Properties.Settings.Default.LastUsedProjectFolder;

                    // Try and find the last used project in the folder
                    string[] fis = Directory.GetFiles(Properties.Settings.Default.LastUsedProjectFolder, "*.rs.xml", System.IO.SearchOption.TopDirectoryOnly);
                    if (fis.Length > 0)
                    {
                        f.FileName = System.IO.Path.GetFileName(fis[0]);
                    }
                }

                if (f.ShowDialog() == true)
                {
                    try
                    {
                        ProjectExplorerDockpaneViewModel.LoadProject(f.FileName);

                        Properties.Settings.Default.LastUsedProjectFolder = Path.GetDirectoryName(f.FileName);
                        Properties.Settings.Default.Save();

                        // This will cause the project tree to reload all open projects
                        ProjectExplorerDockpaneViewModel.Show();
                    }
                    catch (FileLoadException exFile)
                    {
                        MessageBox.Show(exFile.Message, "Invalid Business Logic File", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Error reading the project file '{0}'. Ensure that the file is a valid project file with valid and complete XML contents.\n\n{1}", f.FileName, ex.Message), Properties.Resources.ApplicationNameLong, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO
                //ErrorHandling.frmException.HandleException(ex, "Error Opening Project", string.Empty);
            }

            //TODO: GIS
            //ArcMap.Application.CurrentTool = null;
        }
    }
}
