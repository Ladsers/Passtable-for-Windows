using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Passtable.Containers;

namespace Passtable.Components
{
    public class StatusBar
    {
        private DockPanel[] _panels;

        public StatusBar(params DockPanel[] panels)
        {
            _panels = panels;
        }

        private void StopAll()
        {
            var emptyAnimation = new DoubleAnimation(0.0, 0.0, new Duration(TimeSpan.FromSeconds(0)));
            foreach (var panel in _panels)
            {
                panel.BeginAnimation(UIElement.OpacityProperty, emptyAnimation);
            }
        }

        public void Show(StatusKey key)
        {
            StopAll();
            
            var opacityAnimation = new DoubleAnimation(0.9, 0.0, new Duration(TimeSpan.FromSeconds(1.8)));
            _panels[(int)key].BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
        }
    }
}