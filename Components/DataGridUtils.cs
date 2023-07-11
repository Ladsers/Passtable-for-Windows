using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Passtable.Components
{
    public static class DataGridUtils
    {
        public static T GetObject<T>(DependencyObject parent, string objectName) where T : DependencyObject
        {
            if (parent == null) return null;

            T foundObject = null;
            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (!(child is T childType))
                {
                    foundObject = GetObject<T>(child, objectName);
                    if (foundObject != null) break;
                }
                else if (!string.IsNullOrEmpty(objectName))
                {
                    if (childType is FrameworkElement frameworkElement && frameworkElement.Name == objectName)
                    {
                        foundObject = childType;
                        break;
                    }

                    foundObject = GetObject<T>(childType, objectName);
                    if (foundObject != null) break;
                }
                else
                {
                    foundObject = childType;
                    break;
                }
            }

            return foundObject;
        }
        
        public static DataGridCell GetDataGridCell(DataGridCellInfo cellInfo)
        {
            var cellContent = cellInfo.Column.GetCellContent(cellInfo.Item);
            return (DataGridCell)cellContent?.Parent;
        }
    }
}