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
                if (MessageBox.Show("Are you sure you want to update the Riverscapes Viewer AddIn resources?", "Update Resources", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)
                      == MessageBoxResult.Yes)
                {
                    // TODO: Cursor
                    //Cursor = Cursors.WaitCursor;

                    ResourceUpdater rru = new ResourceUpdater(Properties.Resources.ResourcesURL, Properties.Resources.BusinessLogicXMLFolder, Properties.Resources.AppDataSymbologyFolder);

                    string appDataResources = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Properties.Resources.AppDataFolder);
                    ResourceUpdater.UpdateResults results = rru.Update(appDataResources);

                    // TOODO: Cursor
                    //Cursor.Current = Cursors.Default;
                    MessageBox.Show(string.Format("The Riverscapes Viewer resources were updated successfully.\n{0} resource files were updated.", results.TotalDownloads), "Resources Updated", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                // TODO
                //Cursor.Current = Cursors.Default;
                //ErrorHandling.frmException.HandleException(ex, "Error Showing RAVE About Form", string.Empty);
            }
        }
    }
}
