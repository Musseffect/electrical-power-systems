﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElectricalPowerSystems
{
    public class FileTabItem: INotifyPropertyChanged
    {
        private string filename;
        public string Filename {
            get { return filename; }
            set {
                filename = value;
                OnPropertyChanged();
                OnPropertyChanged("Header");
            }
        }
        public string ToolTip
        {
            get { return filepath==null?filename:filepath; }
        }
        public string Header { get { return Filename + (Changed ? "*":""); }  }
        private string content;
        public string Content {
            get {
                return content;
            }
            set
            {
                content = value;
                Changed = true;
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
            changed = false;
            Filename = "Новый проект";
            content = "";
            FilePath = null;
        }
        public FileTabItem(string filename,string filepath,string content)
        {
            Changed = false;
            Filename = filename;
            this.content = content;
            FilePath = filepath;
        }
    }
}