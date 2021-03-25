using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using KeySearcher.Helper;
using KeySearcher.KeyPattern;
using KeySearcher.P2P.Presentation;
using KeySearcher.P2P.Storage;
using KeySearcher.P2P.Tree;

namespace KeySearcher.Presentation.Controls
{
    /// <summary>
    /// Interaction logic for KeyPoolTreePresentation.xaml
    /// </summary>
    public partial class KeyPoolTreePresentation : UserControl
    {
        private Node _rootNode;

        public KeyPoolTreePresentation()
        {
            InitializeComponent();
        }

        private KeyPatternPool _patternPool;
        internal KeyPatternPool PatternPool
        {
            get { return _patternPool; }
            set 
            { 
                _patternPool = value;
                var nodeBaseToStringConverter = (NodeBaseToStringConverter)FindResource("NodeBaseToStringConverter");
                nodeBaseToStringConverter.PatternPool = value;
            }
        }

        internal KeyQualityHelper KeyQualityHelper
        {
            get;
            set;
        }

        internal StorageKeyGenerator KeyGenerator
        {
            get;
            set;
        }

        public StatusContainer StatusContainer
        {
            get;
            set;
        }

        private void Update()
        {
            try
            {
                _rootNode = null;
                if (PatternPool != null && KeyQualityHelper != null && KeyGenerator != null && StatusContainer != null)
                {
                    var identifier = KeyGenerator.Generate();
                    _rootNode = new Node(KeyQualityHelper, null, 0, PatternPool.Length - 1, identifier);
                    _rootNode.UpdateCache();    //Updates one path
                    _rootNode.UpdateAll();      //Updates everything that is not null in DHT
                }
            }
            catch (Exception)
            {
                _rootNode = null;
            }
        }

        private void FillTreeItem(Node node, TreeViewItem item)
        {
            item.Header = string.Format("Node: {0} to {1}", node.From, node.To);
            item.ToolTip = string.Format("Node: {0}\n{1}", node.ToString(), node.IsReserved() ? "reserved" : "not reserved");
            item.Tag = node;
            if (node.IsReserved())
                item.Background = Brushes.Yellow;

            TreeViewItem leftChildItem = CreateTreeItem(node.leftChild, node.LeftChildFinished);
            item.Items.Add(leftChildItem);
            TreeViewItem rightChildItem = CreateTreeItem(node.rightChild, node.RightChildFinished);
            item.Items.Add(rightChildItem);
        }

        private TreeViewItem CreateTreeItem(NodeBase child, bool finished)
        {
            TreeViewItem childItem = new TreeViewItem();
            if (child == null)
            {
                childItem.Header = "Not loaded!";
            }
            else
            {
                if (child is Node)
                    FillTreeItem((Node)child, childItem);
                else
                {
                    childItem.ToolTip = string.Format("Leaf: {0}\n{1}", child.ToString(), child.IsCalculated() ? "calculated" : "not calculated");
                    childItem.Header = string.Format("Leaf: {0} to {1}", child.From, child.To);
                    childItem.Tag = child;
                    childItem.Background = Brushes.Green;
                    if (child.IsReserved())
                        childItem.Background = Brushes.YellowGreen;
                }
            }
            if (finished)
            {
                childItem.Background = Brushes.DarkBlue;
                childItem.Foreground = Brushes.White;
                childItem.Header = string.Format("Finished!");
                childItem.ToolTip = string.Format("Finished!");
            }
            
            return childItem;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            waitLabel.Visibility = System.Windows.Visibility.Visible;
            errorLabel.Visibility = System.Windows.Visibility.Collapsed;
            refreshButton.IsEnabled = false;

            var thread = new Thread(delegate (Object obj)
                                        {
                                            Update();
                                            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
                                                                {
                                                                    if (_rootNode != null)
                                                                    {
                                                                        treeView.Items.Clear();
                                                                        var rootItem = new TreeViewItem();
                                                                        treeView.Items.Add(rootItem);
                                                                        FillTreeItem(_rootNode, rootItem);
                                                                    }
                                                                    else
                                                                    {
                                                                        errorLabel.Visibility = System.Windows.Visibility.Visible;
                                                                    }

                                                                    waitLabel.Visibility = System.Windows.Visibility.Collapsed;
                                                                    refreshButton.IsEnabled = true;
                                                                },
                                                              null);
                                        });
            thread.Start();
        }
    }

    class NodeBaseToStringConverter : IValueConverter
    {
        public KeyPatternPool PatternPool { set; get; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "No information available...";

            var nodebase = (NodeBase)value;
            var pattern = PatternPool.GetPatternRangeRepresentation(nodebase.From, nodebase.To);

            string res = pattern + "\n";

            BigInteger count = nodebase.Activity.SelectMany(re => re.Value).Aggregate<KeyValuePair<long, Information>, BigInteger>(0, (current, re1) => current + re1.Value.Count);
            res += string.Format("Activity Sum: {0}\n\n", count);

            res += "Results:\n";
            foreach (var re in nodebase.Result)
            {
                res += re.ToString() + "\n";
            }
            res += "\nLast Update: " + nodebase.LastUpdate;
            
            res += "\nActivity:\n";
            foreach (var re in nodebase.Activity)
            {
                res += re.Key;
                res = re.Value.Aggregate(res, (current, re1) => current + string.Format(" {0} -> {1}\n", re1.Key, re1.Value));
            }

            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
