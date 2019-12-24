using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElectricalPowerSystems
{
    public class FileTabItem: INotifyPropertyChanged
    {
        private string filename;
        public string Filename {
            get { return filename; }
            set { filename = value;
                OnPropertyChanged();
            }
        }
        public string Header { get { return filename + (Changed ? "*":""); }  }
        private string content;
        public string Content {
            get {
                return content;
            }
            set
            {
                content = value;
                OnPropertyChanged();
            }
        }
        private bool changed;
        public bool Changed {
            get
            {
                return changed;
            }
            set
            {
                if (changed != value)
                {
                    changed = value;
                    OnPropertyChanged();
                    OnPropertyChanged("Header");
                }
            }
        }
        private string filepath;
        public string FilePath {
            get
            {
                return filepath;
            }
            set
            {
                filepath = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        public FileTabItem()
        {
            Changed = false;
            filename = "Новый проект";
            Content = "";
            FilePath = null;
        }
    }
}