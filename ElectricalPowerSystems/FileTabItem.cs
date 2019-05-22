using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElectricalPowerSystems
{
    public class FileTabItem: INotifyPropertyChanged
    {
        public string Header { get; set; }
        public string Content { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}