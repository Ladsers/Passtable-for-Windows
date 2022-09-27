using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Passtable.Components
{
    public class DataSearcher
    {
        private readonly List<GridItem> _gridItems;
        private readonly DataGrid _dataGrid;
        private readonly List<GridItem> _allGridItems;

        public bool SearchIsRunning { get; private set; }

        public DataSearcher(List<GridItem> gridItems, DataGrid dataGrid)
        {
            _gridItems = gridItems;
            _dataGrid = dataGrid;
            _allGridItems = new List<GridItem>();
        }

        public void RememberCurrentState()
        {
            _allGridItems.Clear();
            _allGridItems.AddRange(_gridItems);
        }

        private void SearchByData(string query)
        {
            _gridItems.Clear();
            if (query.Length == 0)
            {
                SearchIsRunning = false;
                _gridItems.AddRange(_allGridItems);
                return;
            }

            SearchIsRunning = true;
            var queryLowerCase = query.ToLower();
            foreach (var gridItem in _allGridItems.Where(gridItem =>
                         gridItem.Note.ToLower().Contains(queryLowerCase) ||
                         gridItem.Login.ToLower().Contains(queryLowerCase))) _gridItems.Add(gridItem);
        }

        public async void SearchByDataAsync(string query)
        {
            await Task.Run(() => SearchByData(query));
            _dataGrid.Items.Refresh();
        }

        private void SearchByTag(bool red, bool green, bool blue, bool yellow, bool purple)
        {
            _gridItems.Clear();
            if (!red && !green && !blue && !yellow && !purple)
            {
                SearchIsRunning = false;
                _gridItems.AddRange(_allGridItems);
                return;
            }

            SearchIsRunning = true;
            foreach (var gridItem in _allGridItems.Where(gridItem =>
                         red && gridItem.Tag.Contains("1") || green && gridItem.Tag.Contains("2") ||
                         blue && gridItem.Tag.Contains("3") || yellow && gridItem.Tag.Contains("4") ||
                         purple && gridItem.Tag.Contains("5"))) _gridItems.Add(gridItem);
        }

        public async void SearchByTagAsync(ToggleButton red, ToggleButton green, ToggleButton blue, ToggleButton yellow,
            ToggleButton purple)
        {
            var redNonNull = red.IsChecked == true;
            var greenNonNull = green.IsChecked == true;
            var blueNonNull = blue.IsChecked == true;
            var yellowNonNull = yellow.IsChecked == true;
            var purpleNonNull = purple.IsChecked == true;
            
            await Task.Run(() => SearchByTag(redNonNull, greenNonNull, blueNonNull, yellowNonNull, purpleNonNull));
            _dataGrid.Items.Refresh();
        }

        public void EditAndGetAll(GridItem item)
        {
            var id = _allGridItems.FindIndex(collectionItem => collectionItem == item);
            _allGridItems[id] = item;
            GetAll();
        }
        
        public void DeleteAndGetAll(GridItem item)
        {
            var id = _allGridItems.FindIndex(collectionItem => collectionItem == item);
            _allGridItems.RemoveAt(id);
            GetAll();
        }

        public void GetAll()
        {
            _gridItems.Clear();
            _gridItems.AddRange(_allGridItems);
            _dataGrid.Items.Refresh();
            SearchIsRunning = false;
        }
    }
}