using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ArcProViewer
{
    /// <summary>
    /// Interaction logic for MetadataWindow.xaml
    /// </summary>
    public partial class MetadataWindow : Window
    {
        public MetadataWindow(bool isProject)
        {
            InitializeComponent();
            Title = string.Format("{0} Metadata", isProject ? "Project" : "Layer");

            //Uri iconUri = new Uri("pack://application:,,,/Images/metadata16.png", UriKind.RelativeOrAbsolute);
            //this.Icon = BitmapImage.Create(iconUri);
        }
    }
}
