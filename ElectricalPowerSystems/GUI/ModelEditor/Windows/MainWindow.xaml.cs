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
        public bool UIEnabled {
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
            using (Stream s = typeof(MainWindow).Assembly.GetManifestResourceStream("ElectricalPowerSystems.GUI.ModelEditor.SyntaxHighlighting.xshd"))
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
            addNewTab();
            DataContext = this;
             ((FileTabItem)_tabItems[0]).Document.Text = 
@"   //rlc series scheme

v=voltageSource(""g"", ""1_a1"", 220.0, 0.0, 50.0);
resistor(""1_a1"",""1_a2"",10.0);
inductor(""1_a2"", ""1_a3"", 5.0);
capacitor(""1_a3"", ""g"", 0.01);
ground(""g"");
voltage(""1_a1"",""g"");
voltage(""1_a2"",""g"");
voltage(""1_a3"",""g"");
current(v);


//rlc parallel scheme

v=voltageSource(""g"", ""2_a1"", 220.0, 0.0, 50.0);
r=resistor(""g"",""2_a1"",10.0);
i=inductor(""g"", ""2_a1"", 5.0);
c=capacitor(""g"", ""2_a1"", 0.01);
voltage(""2_a1"",""g"");
current(v);
current(r);
current(i);
current(c);


//complex scheme

v1=voltageSource(""g"", ""3_a1"", 220.0, 0.0, 50.0);
v2=voltageSource(""3_a5"", ""3_a4"", 220.0, 0.0, 50.0);
r1=resistor(""3_a2"",""3_a5"",10.0);
r2=resistor(""3_a5"",""3_a6"",10.0);
i1=inductor(""3_a1"", ""3_a6"", 5.0);
i2=inductor(""3_a2"", ""3_a3"", 5.0);
c1=capacitor(""3_a6"", ""g"", 0.02);
c2=capacitor(""3_a3"", ""3_a4"", 0.04);
c3=capacitor(""3_a1"", ""3_a2"", 0.01);
voltage(""3_a1"",""g"");
voltage(""3_a2"",""g"");
voltage(""3_a3"",""g"");
voltage(""3_a4"",""g"");
voltage(""3_a5"",""g"");
voltage(""3_a6"",""g"");";
            FileTab.SelectedIndex = 0;

            ((FileTabItem)_tabItems[1]).Document.Text = 
@"set a = 2;
x * x + a = e()^x * sin(x);
x(0) = 0;";

            ((FileTabItem)_tabItems[2]).Document.Text =
@"set a = 2;
set t0 = 0;
set time = 1;

der(x) = a * x;
x(t0) = 1;";

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
            ClearOutput();
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
                string text = "";
                window.Dispatcher.Invoke(() => { text = tab.Document.Text; });
                Interpreter.Equations.Nonlinear.EquationCompiler compiler = new Interpreter.Equations.Nonlinear.EquationCompiler();
                Interpreter.Equations.Nonlinear.NonlinearEquationDefinition compiledEquation = compiler.CompileEquations(text);
                MathUtils.NonlinearSystemSymbolicAnalytic system = new MathUtils.NonlinearSystemSymbolicAnalytic(compiledEquation);
                //calc solution
                MathNet.Numerics.LinearAlgebra.Vector<double> solution = MathUtils.NewtonRaphsonSolver.Solve(
                system,
                MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray(compiledEquation.InitialValues),
                20,
                0.01,
                1.0
                );
                outputList.Add("Упрощённое представление");
                outputList.Add(compiledEquation.PrintEquations());
                outputList.Add("Решение");
                FormOutput(solution, compiledEquation, ref outputList);
            }
            catch (Interpreter.Equations.CompilerException exc)
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
                string text = "";
                window.Dispatcher.Invoke(() => { text = tab.Document.Text; });
                Interpreter.MainInterpreter.SolveModel(text, ref errorList, ref outputList);
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
            ClearOutput();
            await Task.Run(()=>Run(this));
            UIEnabled = true;
            Expander.IsExpanded = true;
        }
        private void ClearOutput()
        {
            OutputText = "";
            errors.Clear();
        }
        //Commands
        private void HelpCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void HelpCommandExecuted(object sender, ExecutedRoutedEventArgs e)
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
        private void SaveCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (FileTab == null)
            {
                e.CanExecute = false;
                return;
            }
            FileTabItem item = FileTab.SelectedItem as FileTabItem;
            //e.CanExecute = item.Changed;
            e.CanExecute = true;
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
            if (SaveFile(path, item.Document.Text))
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
                if (SaveFile(path, item.Document.Text))
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
            if (SaveFile(path, item.Document.Text))
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
            ClearOutput();
            await Task.Run(() => RunEquationGenerationAC(this));
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
        private void RunEquationGenerationAC(MainWindow window)
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
                string text = "";
                window.Dispatcher.Invoke(() => { text = tab.Document.Text; });
                Interpreter.MainInterpreter.EquationGeneration(text, ref errorList, ref outputList);
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
        private async void RungGenerateDAE_Click(object sender, RoutedEventArgs e)
        {
            UIEnabled = false;
            ClearOutput();
            await Task.Run(() => RunEquationGenerationDAE(this));
            UIEnabled = true;
            Expander.IsExpanded = true;
        }

        private void RunEquationGenerationDAE(MainWindow window)
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
                string text = "";
                window.Dispatcher.Invoke(() => { text = tab.Document.Text; });
                Interpreter.MainInterpreter.EquationGenerationDAE(text, ref errorList, ref outputList);
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
        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            expanderRow.Height = new GridLength(1, GridUnitType.Auto);
            tabRow.Height = new GridLength(1, GridUnitType.Star);
        }
        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            expanderRow.Height = new GridLength(1, GridUnitType.Star);
        }

        private void RLCParallelCircuit_Click(object sender, RoutedEventArgs e)
        {
            string content =
@"   //rlc series scheme

v=voltageSource(""g"", ""1_a1"", 220.0, 0.0, 50.0);
resistor(""1_a1"",""1_a2"",10.0);
inductor(""1_a2"", ""1_a3"", 5.0);
capacitor(""1_a3"", ""g"", 0.01);
ground(""g"");
voltage(""1_a1"",""g"");
voltage(""1_a2"",""g"");
voltage(""1_a3"",""g"");
current(v);";
            FileTabItem tab = new FileTabItem("RLC series circuit",null,content);
            _tabItems.Add(tab);
            FileTab.SelectedIndex = _tabItems.Count - 1;
        }

        private void RLCSeriesCircuit_Click(object sender, RoutedEventArgs e)
        {
            string content =
@"   //rlc parallel scheme
            
v=voltageSource(""g"", ""2_a1"", 220.0, 0.0, 50.0);
r=resistor(""g"",""2_a1"",10.0);
i=inductor(""g"", ""2_a1"", 5.0);
c=capacitor(""g"", ""2_a1"", 0.01);
voltage(""2_a1"",""g"");
current(v);
current(r);
current(i);
current(c);";
            FileTabItem tab = new FileTabItem("RLC parallel circuit", null, content);
            _tabItems.Add(tab);
            FileTab.SelectedIndex = _tabItems.Count - 1;
        }

        private async void RunNonlinearExpressionTest_Click(object sender, RoutedEventArgs e)
        {
            //Тестирование парсинга и упрощения алгебраических уравнений
            UIEnabled = false;
            ClearOutput();
            try
            {
                await Task.Run(() => RunNonlinearExpressionTest(this));
            }
            catch (Exception exc)
            {
                Console.Write(exc.Message);
            }
            UIEnabled = true;
            Expander.IsExpanded = true;
        }
        internal void RunNonlinearExpressionTest(MainWindow window)
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
                string text = "";
                window.Dispatcher.Invoke(() => { text = tab.Document.Text; });
                Interpreter.Equations.Nonlinear.EquationCompiler compiler = new Interpreter.Equations.Nonlinear.EquationCompiler();
                Interpreter.Equations.Nonlinear.NonlinearEquationDefinition compiledEquation = compiler.CompileEquations(text);
                outputList.Add("Variables:");
                outputList.Add(compiledEquation.PrintVariables());
                outputList.Add(compiledEquation.PrintEquations());
                outputList.Add(compiledEquation.PrintJacobiMatrix(false));
            }
            catch (Interpreter.Equations.CompilerException exc)
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

        private async void RunDAEExpressionTest_Click(object sender, RoutedEventArgs e)
        {
            //Тестирование парсинга и упрощения ДАУ
            UIEnabled = false;
            ClearOutput();
            try
            {
                await Task.Run(() => RunDAEExpressionTest(this));
            }
            catch (Exception exc)
            {
                Console.Write(exc.Message);
            }
            UIEnabled = true;
            Expander.IsExpanded = true;
        }
        internal void RunDAEExpressionTest(MainWindow window)
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
                string text = "";
                window.Dispatcher.Invoke(() => { text = tab.Document.Text; });
                Interpreter.Equations.DAE.DAECompiler compiler = new Interpreter.Equations.DAE.DAECompiler();
                Interpreter.Equations.DAE.DAEImplicitDefinition compiledEquation = compiler.CompileDAEImplicit(text);
                outputList.Add("Variables:");
                outputList.Add(compiledEquation.PrintVariables());
                outputList.Add("Equations:");
                outputList.Add(compiledEquation.PrintEquations());
            }
            catch (Interpreter.Equations.CompilerException exc)
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
        private async void RunDAETest_Click(object sender, RoutedEventArgs e)
        {
            //Тестирование решения ДАУ системы
            UIEnabled = false;
            ClearOutput();
            try
            {
                await Task.Run(() => RunDAETest(this));
            }
            catch (Exception exc)
            {
                Console.Write(exc.Message);
            }
            UIEnabled = true;
            Expander.IsExpanded = true;
        }
        internal void RunDAETest(MainWindow window)
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
                string text = "";
                window.Dispatcher.Invoke(() => { text = tab.Document.Text; });
                Interpreter.Equations.DAE.DAECompiler compiler = new Interpreter.Equations.DAE.DAECompiler();
                Interpreter.Equations.DAE.DAEImplicitDefinition compiledEquation = compiler.CompileDAEImplicit(text);
                
                MathUtils.RADAUIIA3 solver = new MathUtils.RADAUIIA3();
                float step = 0.1f;
                solver.SetStep(step);
                solver.SetNewtonIterations(20);
                solver.SetNewtonFAbsTol(0.1f);
                Interpreter.Equations.DAE.DAESolution solution = MathUtils.DAEImplicitSolver.Solve(compiledEquation,solver);
                window.Dispatcher.Invoke(() =>
                {
                    solution.ShowResults();
                });
            }
            catch (Interpreter.Equations.CompilerException exc)
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
    }
}
