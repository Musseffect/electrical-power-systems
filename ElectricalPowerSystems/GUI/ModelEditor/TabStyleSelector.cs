using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ElectricalPowerSystems
{
    class TabStyleSelector:StyleSelector
    {
        public Style ItemStyle { get; set; }
        public Style NewButtonStyle { get; set; }
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item == CollectionView.NewItemPlaceholder)
            {
                return NewButtonStyle;
            }
            else
            {
                return ItemStyle;
            }
        }
    }
}
