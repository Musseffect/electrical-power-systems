using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ElectricalPowerSystems
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        private ObservableCollection<ErrorMessage> errors;
        public ObservableCollection<ErrorMessage> Errors
        {
            get { return errors; }
        }
        private ObservableCollection<FileTabItem> _tabItems;
        public ObservableCollection<FileTabItem> TabItems
        {
            get { return _tabItems; }
        }
        private string outputText;
        public string OutputText {
            get { return outputText; }
            set { outputText = value;
                OnPropertyChanged();
            }
        }
        private string statusText;
        public string StatusText
        {
            get { return statusText; }
            set {
                statusText = value;
                OnPropertyChanged();
            }
        }
        private DelegateCommand _newCommand;
        public DelegateCommand NewCommand
        {
            get
            {
                if (_newCommand == null)
                {
                    _newCommand = new DelegateCommand(CanExecuteNew, NewExecute);
                }
                return _newCommand;
            }
        }
        private DelegateCommand _deleteCommand;
        public DelegateCommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new DelegateCommand(CanExecuteDelete, DeleteExecute);
                }
                return _deleteCommand;
            }
        }
        public MainWindow()
        {
            StatusText = "Готово";
            errors = new ObservableCollection<ErrorMessage>();
            _tabItems = new ObservableCollection<FileTabItem>();
            var itemsView = (IEditableCollectionView)CollectionViewSource.GetDefaultView(_tabItems);
            itemsView.NewItemPlaceholderPosition = NewItemPlaceholderPosition.AtEnd;
            InitializeComponent();
            addNewTab();
            FileTab.SelectedIndex = 0;
            DataContext = this;
            ((FileTabItem)_tabItems[0]).Content = @"voltageSource(""a2"", ""a1"", 10.0, 10.0, 50.0);";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"voltageSource(""a1"", ""a3"", 10.0, 0.5, 50.0);";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"resistor(""a1"", ""a3"", 5.0);";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"resistor(""a4"", ""a2"", 4.0);";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"ground(""a2"");";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"voltage(""a2"",""a4"");";

            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromSeconds(2);
            dt.Tick += DispatcherTimerTick;
            dt.Start();
            //NodeCanvas canvas = new NodeCanvas();
            //canvas.Show();
            //this.Close();
        }
        private void DispatcherTimerTick(object sender, EventArgs e)
        {
            MemoryUsage.Text = Convert.ToString(Process.GetCurrentProcess().PrivateMemorySize64/1048576) + "MB";
        }
        private void addNewTab()
        {
            FileTabItem tab = new FileTabItem();
            tab.Header = "New File";
            tab.Content = "";
            _tabItems.Add(tab);
        }
        private void Run(MainWindow window)
        {
            FileTabItem tab = null;
            Dispatcher.Invoke(() => {
                tab = window.FileTab.SelectedItem as FileTabItem;
            });
            if (tab == null)
            {
                return;
            }
            List<ErrorMessage> errorList = new List<ErrorMessage>();
            Dispatcher.Invoke(() => {
                window.FileTab.Focus();
                window.StatusText = "Рассчёт";
            });
            Thread.Sleep(4000);
            List<string> outputList = new List<string>();
            try
            {
                Interpreter.MainInterpreter.compile(tab.Content, ref errorList, ref outputList);
            }
            catch (Exception exc)
            {
                Dispatcher.Invoke(() => {
                    window.OutputText += exc.Message;
                    window.OutputText += "\n";
                    window.OutputText += exc.StackTrace;
                });
                return;
            }
            Dispatcher.Invoke(() => { 
                foreach (ErrorMessage error in errorList)
                {
                    window.errors.Add(error);
                }
                foreach (var output in outputList)
                {
                    window.OutputText += output;
                    window.OutputText += "\n";
                }
                window.StatusText = "Готово";
            }
            );
            return;
            /*
            ModelGraphCreatorAC modelGraph=new ModelGraphCreatorAC();
            modelGraph.addVoltageSource("a2","a1",10.0f,50.0f,10.0f);
            modelGraph.addVoltageSource("a1", "a3", 10.0f,50.0f,0.5f);
            modelGraph.addResistor("a1","a3",5.0f);
            modelGraph.addResistor("a1", "a4", 15.0f);
            modelGraph.addResistor("a4", "a2", 4.0f);
            modelGraph.addGround("a2");
            this.errors.Clear();
            if (!modelGraph.validate(ref errorList))
            {
                foreach (string error in errorList)
                {
                    this.errors.Add(new ModelParsing.ErrorMessage(error));
                }
            }
            else
            {
                List<string> outputList=ModelSolver.SolveAC(modelGraph);
                foreach (string output in outputList)
                {
                    OutputTextBox.AppendText(output);
                    OutputTextBox.AppendText("\n");
                }
            }
            return;

            OutputTextBox.Clear();
            List<ModelParsing.Token> tokens = ModelParsing.Lexer.runLexer(TextBox.Text, ref errors);
            foreach (ModelParsing.Token token in tokens)
            {
                OutputTextBox.AppendText(token.getTokenString());
                OutputTextBox.AppendText("\n");
            }*/
        }
        private async void RunMenuButton_Click(object sender, RoutedEventArgs e)
        {
            RunMenuButton.IsEnabled = false;
            clearOutput();
            await Task.Run(()=>Run(this));
            RunMenuButton.IsEnabled = true;
        }
        private void clearOutput()
        {
            OutputText = "";
            errors.Clear();
        }
        private void NewExecute(object parameter)
        {
            addNewTab();
        }
        private void DeleteExecute(object parameter)
        {
            int index = _tabItems.IndexOf(parameter as FileTabItem);
            int selectedIndex = FileTab.SelectedIndex;
            if (index > -1 && index < _tabItems.Count)
            {
                if (_tabItems.Count == 1)
                    addNewTab();
                if (index == selectedIndex)
                {
                    FileTab.SelectedIndex = 0;
                }
                _tabItems.RemoveAt(index);
            }
        }
        private bool CanExecuteDelete(object parameter)
        {
            return _tabItems.Count > 0;
        }
        private bool CanExecuteNew(object parameter)
        {
            return true;
        }
        private void FileTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FileTab.Focus();
            TabControl control = sender as TabControl;
            if (control.SelectedIndex >= ((ObservableCollection<FileTabItem>)control.ItemsSource).Count)
            {
                control.SelectedItem = e.RemovedItems[0];
            }
        }
    }
}
