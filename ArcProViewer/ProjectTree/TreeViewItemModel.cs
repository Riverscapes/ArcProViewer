using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;


namespace ArcProViewer.ProjectTree
{
    public class TreeViewItemModel
    {
        public readonly ITreeItem Item;
        public readonly TreeViewItemModel Parent;
        public string Name => Item.Name;
        public string ImagePath => System.IO.Path.Combine("Images", Item.ImagePath);

        public ObservableCollection<TreeViewItemModel> Children { get; set; }

        public bool IsExpanded { get; set; }

        public TreeViewItemModel(ITreeItem item, TreeViewItemModel parent)
        {
            Item = item;
            Parent = parent;
        }

        public string ContextMenu => Item.GetType().Name;

        public TreeViewItemModel() { }

        public TreeViewItemModel AddChild(ITreeItem item)
        {
            if (Children == null)
                Children = new ObservableCollection<TreeViewItemModel>();

            TreeViewItemModel newItem = new TreeViewItemModel(item, this);

            if (item is GroupLayer)
            {
                newItem.IsExpanded = !((GroupLayer)item).Collapse;
            }

            Children.Add(newItem);
            return newItem;
        }

        /// <summary>
        /// Recursively get a list of all nodes down the tree
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="node"></param>
        public static void GetAllNodes(List<TreeViewItemModel> nodes, TreeViewItemModel node)
        {
            // Add the current node to the list
            nodes.Add(node);

            if (node.Children != null)
            {
                foreach (TreeViewItemModel child in node.Children)
                    GetAllNodes(nodes, child);
            }
        }

        public static TreeViewItemModel FindTreeNodeById(TreeViewItemModel parent, string id)
        {
            string nodeId = string.Empty;
            if (parent.Item is BaseDataset)
            {
                BaseDataset ds = parent.Item as BaseDataset;
                nodeId = ds.Id;
            }
            else if (parent.Item is GroupLayer)
            {
                GroupLayer ds = parent.Item as GroupLayer;
                nodeId = ds.Id;
            }

            if (!string.IsNullOrEmpty(nodeId))
            {
                if (string.Compare(nodeId, id, true) == 0)
                    return parent;
            }

            foreach (TreeViewItemModel child in parent.Children)
            {
                TreeViewItemModel result = FindTreeNodeById(child, id);
                if (result is TreeViewItemModel)
                    return result;
            }

            return null;
        }
    }
}
