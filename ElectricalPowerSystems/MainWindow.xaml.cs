using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
            set { _tabItems = value;
                OnPropertyChanged();
            }
        }
        private string outputText;
        public string OutputText {
            get { return outputText; }
            set {
                outputText = value;
                OnPropertyChanged();
            }
        }
        private bool uiFlag;
        private bool UIEnabled {
            get { return uiFlag; }
            set {
                uiFlag = value;
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
            UIEnabled = true;


            ICSharpCode.AvalonEdit.Highlighting.IHighlightingDefinition customHighlighting;
            using (Stream s = typeof(MainWindow).Assembly.GetManifestResourceStream("ElectricalPowerSystems.SyntaxHighlighting.xshd"))
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (System.Xml.XmlReader reader = new System.Xml.XmlTextReader(s))
                {
                    customHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                        HighlightingLoader.Load(reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
                }
            }
            // and register it in the HighlightingManager
            ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.RegisterHighlighting("Model language", new string[] { ".*" }, customHighlighting);


            StatusText = "Готово";
            errors = new ObservableCollection<ErrorMessage>();
            _tabItems = new ObservableCollection<FileTabItem>();
            InitializeComponent();
            addNewTab();
            addNewTab();
            DataContext = this;
            ((FileTabItem)_tabItems[0]).Content += @"   //rlc series scheme";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"v=voltageSource(""g"", ""1_a1"", 220.0, 0.0, 50.0);";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"resistor(""1_a1"",""1_a2"",10.0)";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"inductor(""1_a2"", ""1_a3"", 5.0);";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"capacitor(""1_a3"", ""g"", 0.01);";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"ground(""g"");";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"voltage(""1_a1"",""g"");";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"voltage(""1_a2"",""g"");";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"voltage(""1_a3"",""g"");";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"current(v);";

            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"   //rlc parallel scheme";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"v=voltageSource(""g"", ""2_a1"", 220.0, 0.0, 50.0);";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"r=resistor(""g"",""2_a1"",10.0);";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"i=inductor(""g"", ""2_a1"", 5.0);";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"c=capacitor(""g"", ""2_a1"", 0.01);";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"ground(""g"");";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"voltage(""2_a1"",""g"");";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"current(v);";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"current(r);";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"current(i);";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"current(c);";

            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"   //complex scheme";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"v1=voltageSource(""g"", ""3_a1"", 220.0, 0.0, 50.0);";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"v2=voltageSource(""3_a5"", ""3_a4"", 220.0, 0.0, 50.0);";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"r1=resistor(""3_a2"",""3_a5"",10.0);";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"r2=resistor(""3_a5"",""3_a6"",10.0);";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"i1=inductor(""3_a1"", ""3_a6"", 5.0);";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"i2=inductor(""3_a2"", ""3_a3"", 5.0);";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"c1=capacitor(""3_a6"", ""g"", 0.02);";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"c2=capacitor(""3_a3"", ""3_a4"", 0.04);";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"c3=capacitor(""3_a1"", ""3_a2"", 0.01);";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"ground(""g"");";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"voltage(""3_a1"",""g"");";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"voltage(""3_a2"",""g"");";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"voltage(""3_a3"",""g"");";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"voltage(""3_a4"",""g"");";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"voltage(""3_a5"",""g"");";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            ((FileTabItem)_tabItems[0]).Content += @"voltage(""3_a6"",""g"");";
            ((FileTabItem)_tabItems[0]).Content += "\r\n";
            FileTab.SelectedIndex = 0;

            ((FileTabItem)_tabItems[1]).Content = @"set a = 2;";
            ((FileTabItem)_tabItems[1]).Content += "\r\n";
            ((FileTabItem)_tabItems[1]).Content += @"x*x+a=e()^x*sin(x);";
            ((FileTabItem)_tabItems[1]).Content += "\r\n";
            ((FileTabItem)_tabItems[1]).Content += @"x(0)=0;";

            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromSeconds(2);
            dt.Tick += DispatcherTimerTick;
            dt.Start();

#if TEST
            Test.Test.RunTests();
            this.Close();
            //NodeCanvas canvas = new NodeCanvas();
            //canvas.Show();
#endif
        }
        private void DispatcherTimerTick(object sender, EventArgs e)
        {
            MemoryUsage.Text = Convert.ToString(Process.GetCurrentProcess().PrivateMemorySize64 / 1048576) + "MB";
        }
        private void addNewTab()
        {
            FileTabItem tab = new FileTabItem();
            _tabItems.Add(tab);
            FileTab.SelectedIndex = _tabItems.Count - 1;
        }

        private async void RunNonlinearTest_Click(object sender, RoutedEventArgs e)
        {
            UIEnabled = false;
            clearOutput();
            try {
                await Task.Run(() => RunNonlinearEquationsTest(this));
            }catch(Exception exc)
            {
                Console.Write(exc.Message);
            }
            UIEnabled = true;
            Expander.IsExpanded = true;
        }
        internal void RunNonlinearEquationsTest(MainWindow window)
        {
            FileTabItem tab = null;
            window.Dispatcher.Invoke(() =>
            {
                tab = window.FileTab.SelectedItem as FileTabItem;
            });
            if (tab == null)
            {
                return;
            }
            List<ErrorMessage> errorList = new List<ErrorMessage>();
            window.Dispatcher.Invoke(() =>
            {
                window.FileTab.Focus();
                window.StatusText = "Расчёт";
            });
            //Thread.Sleep(4000); //Test of UI
            List<string> outputList = new List<string>();
            try
            {
                Interpreter.Equations.Nonlinear.EquationCompiler compiler = new Interpreter.Equations.Nonlinear.EquationCompiler();
                Interpreter.Equations.Nonlinear.NonlinearEquationDefinition compiledEquation = compiler.CompileEquations(tab.Content);
                MathUtils.NonlinearSystemSymbolicAnalytic system = new MathUtils.NonlinearSystemSymbolicAnalytic(compiledEquation);
                //calc solution
                MathNet.Numerics.LinearAlgebra.Vector<double> solution = MathUtils.NewtonRaphsonSolver.Solve(
                system,
                MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray(compiledEquation.InitialValues),
                20,
                0.01,
                1.0
                );
                outputList.Add("Solution");
                FormOutput(solution, compiledEquation, ref outputList);
            }
            catch (Interpreter.Equations.Nonlinear.CompilerException exc)
            {
                outputList.Add(exc.Message);
                var errors = exc.Errors;
                foreach (var error in errors)
                {
                    errorList.Add(error);
                }
            }
            catch (Exception exc)
            {
                window.Dispatcher.Invoke(() =>
                {
                    window.OutputText += exc.Message;
                    window.OutputText += "\n";
                    window.OutputText += exc.StackTrace;
                });
                return;
            }
            try
            {
                window.Dispatcher.Invoke(() =>
                {
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
            }
            catch (Exception exc)
            {
                Console.Write(exc.Message);
            }
            return;
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
                window.StatusText = "Расчёт";
            });
            //Thread.Sleep(4000); //Test of UI
            List<string> outputList = new List<string>();
            try
            {
                Interpreter.MainInterpreter.SolveModel(tab.Content, ref errorList, ref outputList);
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
        }
        private async void RunMenuButton_Click(object sender, RoutedEventArgs e)
        {
            UIEnabled = false;
            clearOutput();
            await Task.Run(()=>Run(this));
            UIEnabled = true;
            Expander.IsExpanded = true;
        }
        private void clearOutput()
        {
            OutputText = "";
            errors.Clear();
        }
        //Commands
        private void SaveCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (FileTab == null)
            {
                e.CanExecute = false;
                return;
            }
            FileTabItem item = FileTab.SelectedItem as FileTabItem;
            e.CanExecute = item.Changed;
        }
        private void SaveCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SaveCommand();
        }
        private void OpenCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (FileTab == null)
            {
                e.CanExecute = false;
                return;
            }
            e.CanExecute = FileTab.Items.Count < 21;
        }
        private void OpenCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            OpenCommand();
        }
        private void NewCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (FileTab == null)
            {
                e.CanExecute = false;
                return;
            }
            e.CanExecute = FileTab.Items.Count < 21;
        }
        private void NewCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            addNewTab();
        }
        private void ExitCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void ExitCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
        private void NewExecute(object parameter)
        {
            addNewTab();
        }
        private void DeleteExecute(object parameter)
        {
            FileTabItem item = parameter as FileTabItem;
            int index = _tabItems.IndexOf(item);
            int selectedIndex = FileTab.SelectedIndex;
            if (index > -1 && index < _tabItems.Count)
            {
                if (item.Changed == true)
                {
                    string message = (string)Application.Current.Resources["m_Unsaved_Changes"];
                    MessageBoxResult result = MessageBox.Show(message, "Unsaved changes",
                        MessageBoxButton.YesNo);
                    if (result != MessageBoxResult.Yes)
                        return;
                }
                if (_tabItems.Count == 1)
                    addNewTab();
                if (index == selectedIndex)
                {
                    FileTab.SelectedIndex = 0;
                }
                _tabItems.RemoveAt(index);
            }
        }
        private bool CanExecuteNew(object parameter)
        {
            return true;
        }
        private bool CanExecuteDelete(object parameter)
        {
            return _tabItems.Count > 0;
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
        private string LoadFile(string filepath)
        {
            string content=File.ReadAllText(filepath);
            return content;
        }
        public bool SaveFile(string filename, string content)
        {
            try
            {
                File.WriteAllText(filename, content);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        private void OpenCommand()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            if (!dialog.ShowDialog().Value)
                return;
            try
            {
                string content = LoadFile(dialog.FileName);
                //System.Diagnostics.Debug.Write(dialog.FileName+"\n"+dialog.SafeFileName);
                FileTabItem tab = new FileTabItem(dialog.SafeFileName,
                    dialog.FileName,
                    content);
                _tabItems.Add(tab);
                FileTab.SelectedIndex = _tabItems.Count-1;
            }
            catch (Exception)
            {
                return;
            }
        }
        private void SaveCommand()
        {
            FileTabItem item = FileTab.SelectedItem as FileTabItem;
            string path = item.FilePath;
            string filename = item.Filename;
            if (item.FilePath == null)
            {
                SaveFileDialog dialog=new SaveFileDialog();
                if (!dialog.ShowDialog().Value)
                    return;
                path = dialog.FileName;
                filename = dialog.SafeFileName;
            }
            if (SaveFile(path, item.Content))
            {
                item.FilePath = path;
                item.Filename = filename;
                item.Changed = false;
            }
        }
        private void SaveAllCommand()
        {
            foreach (FileTabItem item in FileTab.Items)
            {
                string path = item.FilePath;
                string filename = item.Filename;
                if (item.FilePath == null)
                {
                    SaveFileDialog dialog = new SaveFileDialog();
                    if (!dialog.ShowDialog().Value)
                        return;
                    path = dialog.FileName;
                    filename = dialog.SafeFileName;
                }
                if (SaveFile(path, item.Content))
                {
                    item.FilePath = path;
                    item.Filename = filename;
                    item.Changed = false;
                }
            }
        }
        private void SaveAsCommand()
        {
            FileTabItem item = FileTab.SelectedItem as FileTabItem;
            SaveFileDialog dialog = new SaveFileDialog();
            if (!dialog.ShowDialog().Value)
                return;
            string path = dialog.FileName;
            string filename = dialog.SafeFileName;
            if (SaveFile(path, item.Content))
            {
                item.FilePath = path;
                item.Filename = filename;
                item.Changed = false;
            }
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            /*FileTabItem item = FileTab.SelectedItem as FileTabItem;
            if (item != null)
            {
                //item.Changed = true;
            }*/
        }
        private void SaveAll_Click(object sender, RoutedEventArgs e)
        {
            SaveAllCommand();
        }
        private async void RunGenerateEquationsButton_Click(object sender, RoutedEventArgs e)
        {
            UIEnabled = false;
            clearOutput();
            await Task.Run(() => RunEquationGeneration(this));
            UIEnabled = true;
            Expander.IsExpanded = true;
        }
        private static void FormOutput(MathNet.Numerics.LinearAlgebra.Vector<double> solution, Interpreter.Equations.Nonlinear.NonlinearEquationDefinition system,ref List<string> outputList)
        {
            for (int i = 0; i < solution.Count; i++)
            {
                outputList.Add(system.VariableNames[i] + " = " + solution[i].ToString());
            }
            double[] x = solution.ToArray();
            for (int i = 0; i < system.Equations.Count; i++)
            {
                outputList.Add($"F{i}(X) = {system.Equations[i].execute(x)}");
            }
        }
        private void RunEquationGeneration(MainWindow window)
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
                window.StatusText = "Генерация уравнений";
            });
            //Thread.Sleep(4000); //Test of UI
            List<string> outputList = new List<string>();
            try
            {
                Interpreter.MainInterpreter.EquationGeneration(tab.Content, ref errorList, ref outputList);
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
        }
        private void RunDAETest_Click(object sender, RoutedEventArgs e)
        {
            /*UIEnabled = false;
            clearOutput();
            //await Task.Run(() => RunDAEEquationsTest(this));
            UIEnabled = true;
            Expander.IsExpanded = true;*/
            throw new NotImplementedException();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            bool isWindowOpen = false;

            foreach (Window w in Application.Current.Windows)
            {
                if (w is HelpWindow)
                {
                    isWindowOpen = true;
                    w.Activate();
                }
            }
            if (!isWindowOpen)
            {
                HelpWindow window = new HelpWindow();
                window.Show();
            }
        }
    }
}
