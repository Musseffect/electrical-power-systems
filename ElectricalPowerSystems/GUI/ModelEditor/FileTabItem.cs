using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElectricalPowerSystems
{
    public class FileTabItem: INotifyPropertyChanged
    {
        private string filename;
        private IHighlightingDefinition highlightdef = null;
        public IHighlightingDefinition HighlightingDef
        {
            get { return highlightdef; }
            set {
                if (this.highlightdef != value)
                {
                    this.highlightdef = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Filename {
            get { return filename; }
            set {
                filename = value;
                if (filename != null)
                    HighlightingDef = HighlightingManager.Instance.GetDefinitionByExtension(System.IO.Path.GetExtension(filename));
                OnPropertyChanged();
                OnPropertyChanged("Header");
            }
        }
        public string ToolTip
        {
            get { return filepath==null?filename:filepath; }
        }
        public string Header { get { return Filename + (Changed ? "*":""); }  }
        private TextDocument document = null;
        public TextDocument Document
        {
            get { return this.document; }
            set {
                if (this.document != value)
                {
                    this.document = value;
                    OnPropertyChanged();
                    Changed = true;
                }
            }

        }/*
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
        }*/
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
            this.document = new TextDocument();
            //content = "";
            FilePath = null;
        }
        public FileTabItem(string filename,string filepath,string content)
        {
            changed = false;
            Filename = filename;

            //this.content = content;
            this.document = new TextDocument(content);
            FilePath = filepath;
        }
    }
}