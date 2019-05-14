using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ElectricalPowerSystems.GUI
{
    public class Node : INotifyPropertyChanged
    {
        double x;
        double y;
        float width;
        float height;
        Color color;
        public double X { get {
                return x;
            }
            set
            {
                x = value;
                OnPropertyChanged("X");
            }
        }
        public double Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
                OnPropertyChanged("Y");
            }
        }
        public float Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
                OnPropertyChanged("Width");
            }
        }
        public float Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
                OnPropertyChanged("Height");
            }
        }
        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
                OnPropertyChanged("Color");
            }
        }
        //public List<Connector> Connectors { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
