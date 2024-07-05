using System;
using ArcGIS.Desktop.Framework.Contracts;
using System.Windows;

namespace ArcProViewer.Buttons
{
    internal class UpdateResourcesButton : Button
    {
        protected override void OnClick()
        {
            try
            {
                if (MessageBox.Show("Are you sure you want to update the Riverscapes Viewer resources?", "Update Resources", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)
                      == MessageBoxResult.Yes)
                {
                    ResourceUpdater rru = new ResourceUpdater(Properties.Resources.ResourcesURL, Properties.Resources.BusinessLogicXMLFolder, Properties.Resources.AppDataSymbologyFolder);

                    string appDataResources = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Properties.Resources.AppDataFolder);
                    ResourceUpdater.UpdateResults results = rru.Update(appDataResources);

                    MessageBox.Show(string.Format("The Riverscapes Viewer resources were updated successfully.\n{0} resource files were updated.", results.TotalDownloads), "Resources Updated", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Updating Resources");
            }
        }
    }
}
