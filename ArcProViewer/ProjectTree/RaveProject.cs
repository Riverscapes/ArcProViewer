﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Imaging;
using ArcGIS.Desktop.Core;

namespace ArcProViewer.ProjectTree
{
    public class RaveProject
    {
        public readonly FileInfo ProjectFile;
        public DirectoryInfo Folder { get { return ProjectFile.Directory; } }
        public readonly string ProjectType;

        // Determines whether uses V1 or V2 XSD and business logic
        public readonly int Version;

        public RaveProject(string projectFile)
        {


            ProjectFile = new FileInfo(projectFile);

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(ProjectFile.FullName);

                // Determine whether the project XSD is version 1 or version 2
                this.Version = GetVersion(xmlDoc);

                string xPath = "Project/ProjectType";
                XmlNode nodProjectType = xmlDoc.SelectSingleNode(xPath);
                if (nodProjectType == null)
                    throw new Exception("Missing XML node at " + xPath);

                if (string.IsNullOrEmpty(nodProjectType.InnerText))
                    throw new Exception(string.Format("The project type at XPath '{0}' contains no value. This XPath cannot be empty.", xPath));

                ProjectType = nodProjectType.InnerText;
            }
            catch (Exception ex)
            {
                ex.Data["Project File"] = ProjectFile.FullName;
                throw;
            }
        }

        private int GetVersion(XmlDocument xmlDoc)
        {
            Dictionary<int, String> versions = new Dictionary<int, String>()
            {
                { 1, "V1/[a-zA-Z]+.xsd"},
                { 2, "/V2/RiverscapesProject.xsd"}
            };

            XmlNode nodProject = xmlDoc.SelectSingleNode("Project");
            XmlAttribute attNamepsace = nodProject.Attributes["xsi:noNamespaceSchemaLocation"];

            foreach (KeyValuePair<int, string> kvp in versions)
            {
                Regex re = new Regex(kvp.Value);
                if (re.IsMatch(attNamepsace.Value))
                    return kvp.Key;
            }

            // If got to here then the Project XSD path didn't match any of the known versions!
            Exception ex = new Exception("Failed to determine project version");
            ex.Data["Namespace"] = attNamepsace.Value;
            throw ex;
        }

        public static bool IsSame(RaveProject proj1, string projectFile)
        {
            return string.Compare(proj1.ProjectFile.FullName, projectFile) == 0;
        }

        //private FileInfo AbsolutePath(string relativePath)
        //{
        //    return new FileInfo(Path.Combine(ProjectFile.DirectoryName, relativePath));
        //}

        /// <summary>
        /// Determine the location of the business lofic XML file for this project
        /// </summary>
        /// <remarks>
        /// The following locations will be searched in order for a 
        /// file with an XPath of /Project/ProjectType that matches (case insenstive)
        /// the ProjectType of this project object.
        /// 
        /// 1. ProjectFolder
        /// 2. %APPDATA%\RAVE\XML
        /// 3. SOFTWARE_DEPLOYMENT\XML
        /// 
        /// </remarks>
        private FileInfo LoadBusinessLogicXML()
        {
            string versionFolder = string.Format("V{0}", Version);

            List<string> SearchFolders = new List<string>()
            {
                ProjectFile.DirectoryName,
                Path.Combine(ProjectExplorerDockpaneViewModel.AppDataFolder.FullName, Properties.Resources.BusinessLogicXMLFolder, versionFolder)
            };

            foreach (string folder in SearchFolders)
            {
                string xmlPath = Path.ChangeExtension(Path.Combine(folder, ProjectType), "xml");
                if (File.Exists(xmlPath))
                {
                    try
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(xmlPath);
                        System.Diagnostics.Debug.Print(string.Format("Using business logic at {0}", xmlPath));
                        return new FileInfo(xmlPath);
                    }
                    catch (Exception ex)
                    {
                        Exception ex2 = new FileLoadException(string.Format("Error Loading business logic from the following path." +
                            " Remove or rename this file to allow RAVE to continue searching for alternative {0} business logic files.\n\n{1}",
                            ProjectType, xmlPath), ex);
                        ex2.Data["FilePath"] = xmlPath;
                        throw ex2;
                    }
                }
            }

            return null;
        }

        public XmlNode MetDataNode
        {
            get
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(ProjectFile.FullName);

                return xmlDoc.SelectSingleNode("Project/MetaData");
            }
        }

        public Uri WarehouseReference
        {
            get
            {
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(ProjectFile.FullName);

                    XmlNode nodProgram = xmlDoc.SelectSingleNode("Project/Warehouse/Meta[@name='program']");
                    XmlNode nodID = xmlDoc.SelectSingleNode("Project/Warehouse/Meta[@name='id']");

                    if (nodProgram is XmlNode && nodID is XmlNode && !string.IsNullOrEmpty(nodProgram.InnerText) && !string.IsNullOrEmpty(nodID.InnerText))
                    {
                        Uri baseUri = new Uri(Properties.Resources.DataWarehouseURL);
                        Uri projectUri = new Uri(baseUri, string.Format("#/{0}/{1}", nodProgram.InnerText, nodID.InnerText));
                        return projectUri;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error attempting to build warehouse URL for project.");
                }

                return null;
            }
        }

        /// <summary>
        /// Load a project into the tree that doesn't already exist
        /// </summary>
        /// <param name="treProject"></param>
        /// <returns></returns>
        public TreeViewItem LoadNewProject(TreeView treProject, ContextMenu cmsProjectView)
        {
            TreeViewItem tnProject = CreateTreeViewItem("TITLE_NOT_FOUND", string.Empty);
            tnProject.Tag = this;
            treProject.Items.Insert(0, tnProject);

            // Assign the project CMS here so that it is available if anything else crashes or goes wrong.
            // Allows the user to unload the partially loaded project.
            treProject.ContextMenu = cmsProjectView;

            TreeViewItem tnResult = LoadProjectIntoNode(tnProject);

            // If nothing returned then something went wrong. Remove the temporary node.
            if (tnResult == null)
            {
                ((TreeViewItem)tnProject.Parent).Items.Remove(tnProject);

                MessageBox.Show(string.Format("Failed to load project because there is no valid business logic XML file for projects of type '{0}'.", ProjectType)
                    + "\n\nEnsure that you have updated the RAVE resource files using the tool under the Help menu on the RAVE toolbar."

                    , "Missing Business Logic XML File", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            return tnResult;
        }

        public TreeViewItem LoadProjectIntoNode(TreeViewItem tnProject)
        {
            // Remove all the existing child nodes (required if refreshing existing tree node)
            tnProject.Items.Clear();

            FileInfo businessLogic = LoadBusinessLogicXML();
            if (businessLogic == null)
            {
                return null;
            }

            return LoadTree(tnProject, businessLogic);
        }

        public TreeViewItem LoadTree(TreeViewItem tnProject, FileInfo businessLogicXML)
        {
            // Determine the type of project
            XmlDocument xmlProject = new XmlDocument();
            xmlProject.Load(ProjectFile.FullName);
            XmlNode projectXMLRoot = xmlProject.SelectSingleNode("Project");

            // Load the business logic XML file and retrieve the root node
            XmlDocument xmlBusiness = new XmlDocument();
            xmlBusiness.Load(businessLogicXML.FullName);
            XmlNode nodBLRoot = xmlBusiness.SelectSingleNode("Project/Node");
            if (!(nodBLRoot is XmlNode))
            {
                MessageBox.Show("Business logic XML file does not contain 'Project/Node' XPath.", "Business Logic Error", MessageBoxButton.OK, MessageBoxImage.Information);
                return null;
            }

            // Retrieve and apply the project name to the parent node
            tnProject.Header = GetLabel(nodBLRoot, projectXMLRoot);

            // The parent node might specify the starting project node (e.g. Riverscapes Context and VBET)
            XmlAttribute attXPath = nodBLRoot.Attributes["xpath"];
            if (attXPath is XmlAttribute && !string.IsNullOrEmpty(attXPath.InnerText))
            {
                XmlNode xmlProjectChild = projectXMLRoot.SelectSingleNode(attXPath.InnerText);
                if (xmlProjectChild is XmlNode)
                    projectXMLRoot = xmlProjectChild;
            }

            // Loop over all child nodes of the business logic XML and load them to the tree
            nodBLRoot.ChildNodes.OfType<XmlNode>().ToList().ForEach(x => LoadTreeNode(tnProject, projectXMLRoot, x));

            LoadProjectViews(tnProject, xmlBusiness);

            // Expand the project tree node now that all the items have been added
            ExpandAll(tnProject);

            tnProject.IsExpanded = true;
            tnProject.UpdateLayout();

            // Loop over all tree nodes and collapse any group layers.
            // This has to be done last once all the nodes have their children
            List<TreeViewItem> allNodes = new List<TreeViewItem>();
            foreach (TreeViewItem node in tnProject.Items)
                GetAllNodes(allNodes, node);
            allNodes.Where(x => x.Tag is GroupLayer && ((GroupLayer)x.Tag).Collapse).ToList().ForEach(x => x.IsExpanded = false);

            return tnProject;
        }

        private static void ExpandAll(TreeViewItem item)
        {
            item.IsExpanded = true;
            item.UpdateLayout();

            foreach (object obj in item.Items)
            {
                if (obj is TreeViewItem)
                {
                    ExpandAll(obj as TreeViewItem);
                }
            }
        }

        private TreeViewItem LoadProjectViews(TreeViewItem tnProject, XmlNode xmlBusiness)
        {
            XmlNode nodViews = xmlBusiness.SelectSingleNode("Project/Views");
            if (nodViews == null)
                return null;

            XmlAttribute attDefault = nodViews.Attributes["default"];
            TreeViewItem defaultView = null;
            string defaultViewName = string.Empty;
            if (attDefault is XmlAttribute)
            {
                defaultViewName = attDefault.InnerText;
            }

            TreeViewItem tnViews = null;

            foreach (XmlNode nodView in nodViews.SelectNodes("View"))
            {
                XmlAttribute attName = nodView.Attributes["name"];
                if (attName == null || string.IsNullOrEmpty(attName.InnerText))
                    continue;

                string viewId = string.Empty;
                XmlAttribute viewAttId = nodView.Attributes["id"];
                if (viewAttId is XmlAttribute && !string.IsNullOrEmpty(viewAttId.InnerText))
                    viewId = viewAttId.InnerText;

                bool IsDefaultView = !string.IsNullOrEmpty(defaultViewName) && string.Compare(defaultViewName, viewId) == 0;

                string viewName = nodView.Attributes["name"].InnerText;
                ProjectView view = new ProjectView(viewId, viewName, IsDefaultView);

                foreach (XmlNode nodLayer in nodView.SelectNodes("Layers/Layer"))
                {
                    XmlAttribute attId = nodLayer.Attributes["id"];
                    if (attId == null || string.IsNullOrEmpty(attId.InnerText))
                        continue;

                    bool isVisible = true;
                    XmlAttribute attVisible = nodLayer.Attributes["visible"];
                    if (attVisible is XmlAttribute && !string.IsNullOrEmpty(attVisible.InnerText))
                    {
                        bool.TryParse(attVisible.InnerText, out isVisible);
                    }

                    TreeViewItem tnLayer = FindTreeNodeById(tnProject, attId.InnerText);
                    if (tnLayer is TreeViewItem)
                    {
                        view.Layers.Add(new ProjectTree.ProjectViewLayer(tnLayer, isVisible));
                    }
                }

                if (view.Layers.Count > 0)
                {
                    // Create the project tree branch that will contain the views
                    if (tnViews == null)
                    {
                        tnViews = CreateTreeViewItem("Project Views", "viewer16");
                        tnViews.Tag = new GroupLayer("Project Views", true, string.Empty);
                        tnProject.Items.Add(tnViews);
                    }

                    TreeViewItem tnView = CreateTreeViewItem(viewName, "view16");
                    tnView.Tag = view;
                    tnViews.Items.Add(tnView);

                    // Check if this is the default view
                    if (view.IsDefaultView)
                        defaultView = tnView;
                }
            }

            return defaultView;
        }

        private TreeViewItem FindTreeNodeById(TreeViewItem parent, string id)
        {
            string nodeId = string.Empty;
            if (parent.Tag is BaseDataset)
            {
                BaseDataset ds = parent.Tag as BaseDataset;
                nodeId = ds.Id;
            }
            else if (parent.Tag is GroupLayer)
            {
                GroupLayer ds = parent.Tag as GroupLayer;
                nodeId = ds.Id;
            }

            if (!string.IsNullOrEmpty(nodeId))
            {
                if (string.Compare(nodeId, id, true) == 0)
                    return parent;
            }

            TreeViewItem result = null;
            foreach (TreeViewItem child in parent.Items)
            {
                result = FindTreeNodeById(child, id);
                if (result is TreeViewItem)
                    return result;
            }

            return null;
        }

        private void LoadTreeNode(TreeViewItem tnParent, XmlNode xmlProject, XmlNode xmlBusiness)
        {
            try
            {
                LoadTreeNodeWorker(tnParent, xmlProject, xmlBusiness);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void LoadTreeNodeWorker(TreeViewItem tnParent, XmlNode xmlProject, XmlNode xmlBusiness)
        {
            if (xmlBusiness.NodeType == XmlNodeType.Comment)
                return;

            if (string.Compare(xmlBusiness.Name, "Repeater", true) == 0)
            {
                // Add the repeater label
                XmlAttribute attLabel = xmlBusiness.Attributes["label"];
                if (attLabel is XmlAttribute && !string.IsNullOrEmpty(attLabel.InnerText))
                {
                    TreeViewItem newNode = CreateTreeViewItem(attLabel.InnerText);
                    tnParent.Items.Add(newNode);

                    // Repeat the business logic items inside the repeater for all items in the xPath
                    XmlAttribute attXPath = xmlBusiness.Attributes["xpath"];
                    if (attXPath is XmlAttribute && !string.IsNullOrEmpty(attXPath.InnerText))
                    {
                        foreach (XmlNode xmlProjectChild in xmlProject.SelectNodes(attXPath.InnerText))
                        {
                            foreach (XmlNode xmlBusinessChild in xmlBusiness.ChildNodes)
                            {
                                LoadTreeNode(newNode, xmlProjectChild, xmlBusinessChild);
                            }
                        }
                    }
                }
            }
            else if (string.Compare(xmlBusiness.Name, "Children", true) == 0)
            {
                foreach (XmlNode xmlBusinessNode in xmlBusiness.ChildNodes)
                {
                    LoadTreeNode(tnParent, xmlProject, xmlBusinessNode);
                }
            }
            else if (string.Compare(xmlBusiness.Name, "Node", true) == 0)
            {
                // First the new project node referred to by the XPath
                XmlAttribute attXPath = xmlBusiness.Attributes["xpath"];
                if (attXPath is XmlAttribute && !string.IsNullOrEmpty(attXPath.InnerText))
                {
                    xmlProject = xmlProject.SelectSingleNode(attXPath.InnerText);
                    if (xmlProject == null)
                        return;
                }

                // Now get the label. First Try the XPath, then the label
                string label = "No Label Provided";
                XmlAttribute attXPathLabel = xmlBusiness.Attributes["xpathlabel"];
                if (attXPathLabel is XmlAttribute && !string.IsNullOrEmpty(attXPathLabel.InnerText))
                {
                    XmlNode xmlLabel = xmlProject.SelectSingleNode(attXPathLabel.InnerText);
                    if (xmlLabel is XmlNode && !string.IsNullOrEmpty(xmlLabel.InnerText))
                    {
                        label = xmlLabel.InnerText;
                    }
                }
                else
                {
                    XmlAttribute attLabel = xmlBusiness.Attributes["label"];
                    if (attLabel is XmlAttribute && !string.IsNullOrEmpty(attLabel.InnerText))
                    {
                        label = attLabel.InnerText;
                    }
                }

                // Get the ID used for associated nodes with project views
                string id = string.Empty;
                XmlAttribute attId = xmlBusiness.Attributes["id"];
                if (attId is XmlAttribute && !string.IsNullOrEmpty(attId.InnerText))
                    id = attId.InnerText;

                XmlAttribute attType = xmlBusiness.Attributes["type"];
                if (attType is XmlAttribute)
                {
                    //This is a GIS Node!

                    // Retrieve symbology key from business logic
                    string symbology = string.Empty;
                    XmlAttribute attSym = xmlBusiness.Attributes["symbology"];
                    if (attSym is XmlAttribute && !String.IsNullOrEmpty(attSym.InnerText))
                        symbology = attSym.InnerText;

                    // Retrieve the transparency from the business logic
                    short transparency = 0;
                    XmlAttribute attTransparency = xmlBusiness.Attributes["transparency"];
                    if (attTransparency is XmlAttribute && !string.IsNullOrEmpty(attTransparency.InnerText))
                    {
                        if (!short.TryParse(attTransparency.InnerText, out transparency))
                            System.Diagnostics.Debug.Print(string.Format("Invalid layer transparency for {0}: {1}", label, transparency));
                    }

                    string def_query = string.Empty;
                    XmlAttribute attDefQuery = xmlBusiness.Attributes["filter"];
                    if (attDefQuery is XmlAttribute && !string.IsNullOrEmpty(attDefQuery.InnerText))
                        def_query = attDefQuery.InnerText;

                    AddGISNode(tnParent, attType.InnerText, xmlProject, symbology, label, transparency, id, def_query);
                }
                else
                {
                    // Static label node

                    // First check if there are children to this node and if so where collapsed is specified
                    bool collapsed = true;
                    XmlNode xmlChildren = xmlBusiness.SelectSingleNode("Children");
                    if (xmlChildren is XmlNode)
                    {
                        XmlAttribute attCollapsed = xmlChildren.Attributes["collapsed"];
                        if (attCollapsed is XmlAttribute && !string.IsNullOrEmpty(attCollapsed.InnerText))
                        {
                            bool.TryParse(attCollapsed.InnerText, out collapsed);
                        }
                    }

                    TreeViewItem newNode = CreateTreeViewItem(label);
                    newNode.Tag = new GroupLayer(label, collapsed, id);
                    tnParent.Items.Add(newNode);
                    tnParent = newNode;
                }

                // Finally process all child nodes
                xmlBusiness.ChildNodes.OfType<XmlNode>().ToList().ForEach(x => LoadTreeNode(tnParent, xmlProject, x));
            }
        }

        public static void GetAllNodes(List<TreeViewItem> nodes, TreeViewItem node)
        {
            // Add the current node to the list
            nodes.Add(node);
            foreach (TreeViewItem child in node.Items)
                GetAllNodes(nodes, child);
        }

        private void AddGISNode(TreeViewItem tnParent, string type, XmlNode nodGISNode, string symbology, string label, short transparency, string id, string query_definition)
        {
            if (nodGISNode == null)
                return;

            // If the project node has a ref attribute then lookup the redirect to the inputs
            XmlAttribute attRef = nodGISNode.Attributes["ref"];
            if (attRef is XmlAttribute)
            {
                nodGISNode = nodGISNode.OwnerDocument.SelectSingleNode(string.Format("Project/Inputs/*[@id='{0}']", attRef.InnerText));
            }

            if (string.IsNullOrEmpty(label))
                label = nodGISNode.SelectSingleNode("Name").InnerText;

            string path = String.Empty;
            if (Version == 1)
            {
                path = nodGISNode.SelectSingleNode("Path").InnerText;

                if (string.Compare(nodGISNode.ParentNode.Name, "layers", true) == 0)
                {
                    XmlNode nodGeoPackage = nodGISNode.SelectSingleNode("../../Path");
                    if (nodGeoPackage is XmlNode)
                    {
                        path = nodGeoPackage.InnerText + "/" + path;
                    }
                    else
                    {
                        throw new MissingMemberException("Unable to find GeoPackage file path");
                    }
                }
            }
            else if (Version == 2)
            {
                XmlNode nodPath = nodGISNode.SelectSingleNode("Path");
                if (nodPath is XmlNode)
                {
                    path = nodPath.InnerText;
                }
                else
                {
                    if (string.Compare(nodGISNode.ParentNode.Name, "layers", true) == 0)
                    {
                        XmlNode nodGeoPackage = nodGISNode.SelectSingleNode("../../Path");
                        XmlAttribute attLayerName = nodGISNode.Attributes["lyrName"];

                        if (nodGeoPackage is XmlNode && attLayerName is XmlAttribute)
                        {
                            path = nodGeoPackage.InnerText + "/" + attLayerName.InnerText;
                        }
                        else
                        {
                            throw new MissingMemberException("Unable to find GeoPackage file path");
                        }
                    }
                }
            }

            string absPath = Path.Combine(ProjectFile.DirectoryName, path);

            // Load the layer metadata
            Dictionary<string, string> metadata = null;
            XmlNode nodMetadata = nodGISNode.SelectSingleNode("MetaData");
            if (nodMetadata is XmlNode && nodMetadata.HasChildNodes)
            {
                metadata = new Dictionary<string, string>();
                foreach (XmlNode nodMeta in nodMetadata.SelectNodes("Meta"))
                {
                    XmlAttribute attName = nodMeta.Attributes["name"];
                    if (attName is XmlAttribute && !string.IsNullOrEmpty(attName.InnerText))
                    {
                        if (!string.IsNullOrEmpty(nodMeta.InnerText))
                        {
                            metadata.Add(attName.InnerText, nodMeta.InnerText);
                        }
                    }
                }
            }

            FileSystemDataset dataset = null;
            switch (type.ToLower())
            {
                case "file":
                case "report":
                    {
                        dataset = new FileSystemDataset(this, label, new FileInfo(absPath), "viewer16", "viewer16", id);
                        break;
                    }

                case "raster":
                    {
                        dataset = new Raster(this, label, absPath, symbology, transparency, id, metadata);
                        break;
                    }

                case "vector":
                case "line":
                case "point":
                case "polygon":
                    {
                        dataset = new Vector(this, label, absPath, symbology, transparency, id, metadata, query_definition);
                        break;
                    }

                case "tin":
                    {
                        dataset = new TIN(this, label, absPath, transparency, id, metadata);
                        break;
                    }

                default:
                    throw new Exception(string.Format("Unhandled Node type attribute string '{0}'", type));
            }

            TreeViewItem newNode = CreateTreeViewItem(label, dataset.ImageFileName);
            newNode.Tag = dataset;
            tnParent.Items.Add(newNode);
        }

        private static string GetXPath(XmlNode businessLogicNode, string xPath)
        {

            XmlAttribute attXPath = businessLogicNode.Attributes["xpath"];
            try
            {
                if (attXPath is XmlAttribute && !String.IsNullOrEmpty(attXPath.InnerText))
                {
                    if (!string.IsNullOrEmpty(xPath))
                        xPath += @"/";

                    xPath += attXPath.InnerText;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error attempting get the XPath", ex);
            }

            return xPath;
        }

        private static string GetLabel(XmlNode businessLogicNode, XmlNode projectNode)
        {
            try
            {
                if (businessLogicNode.Attributes != null)
                {
                    // See if the business logic has a label attribute.
                    XmlAttribute attLabel = businessLogicNode.Attributes["label"];
                    if (attLabel is XmlAttribute && !string.IsNullOrEmpty(attLabel.InnerText))
                    {
                        return attLabel.InnerText;
                    }

                    // See if the project node has a child Name node with valid inner text.
                    if (projectNode is XmlNode)
                    {
                        XmlNode nodName = projectNode.SelectSingleNode("Name");
                        if (nodName is XmlNode && !string.IsNullOrEmpty(nodName.InnerText))
                            return nodName.InnerText;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error attempting to get node label", ex);
            }

            return string.Empty;
        }

        private TreeViewItem CreateTreeViewItem(string text, string imageFileName = "viewer16")
        {
            TreeViewItem treeViewItem = new TreeViewItem();
            treeViewItem.Header = text;

            if (!string.IsNullOrEmpty(imageFileName))
            {
                //Image image = new Image();
                //BitmapImage bitmapImage = new BitmapImage(new Uri(string.Format("pack://application:,,,/{0}.png", imageFileName)));
                //image.Source = bitmapImage;
                //image.Width = 16;
                //image.Height = 16;
            }

            return treeViewItem;
        }
    }
}