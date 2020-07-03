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

namespace ElectricalPowerSystems.GUI.ModelEditor.Windows
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
        private bool testFlag;
        public bool TestFlag
        {
            get { return testFlag; }
            private set {
                testFlag = value;
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
        /*private DelegateCommand _newCommand;
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
        }*/
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
#if DEBUG||TEST
            TestFlag = true;
#else
            TestFlag = false;
#endif
            ICSharpCode.AvalonEdit.Highlighting.IHighlightingDefinition modelHighlighting;
            ICSharpCode.AvalonEdit.Highlighting.IHighlightingDefinition recloserHighlighting;
            ICSharpCode.AvalonEdit.Highlighting.IHighlightingDefinition equationsHighlighting;
            using (Stream s = typeof(MainWindow).Assembly.GetManifestResourceStream("ElectricalPowerSystems.GUI.ModelEditor.SyntaxHighlightingModelLanguage.xshd"))
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (System.Xml.XmlReader reader = new System.Xml.XmlTextReader(s))
                {
                    modelHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                        HighlightingLoader.Load(reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
                }
            }
            using (Stream s = typeof(MainWindow).Assembly.GetManifestResourceStream("ElectricalPowerSystems.GUI.ModelEditor.SyntaxHighlightingRecloserLanguage.xshd"))
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (System.Xml.XmlReader reader = new System.Xml.XmlTextReader(s))
                {
                    recloserHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                        HighlightingLoader.Load(reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
                }
            }
            using (Stream s = typeof(MainWindow).Assembly.GetManifestResourceStream("ElectricalPowerSystems.GUI.ModelEditor.SyntaxHighlightingEquationsLanguage.xshd"))
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (System.Xml.XmlReader reader = new System.Xml.XmlTextReader(s))
                {
                    equationsHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                        HighlightingLoader.Load(reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
                }
            }
            // and register it in the HighlightingManager
            ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.RegisterHighlighting("Model language", new string[] { ".psms",".*" }, modelHighlighting);
            ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.RegisterHighlighting("Equation language", new string[] { ".eq" }, equationsHighlighting);
            ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.RegisterHighlighting("Recloser language", new string[] { ".rcl" }, recloserHighlighting);

            StatusText = "Готово";
            errors = new ObservableCollection<ErrorMessage>();
            _tabItems = new ObservableCollection<FileTabItem>();
            InitializeComponent();
            AddNewTab();
            AddNewTab();
            AddNewTab();
            AddNewTab();
            AddNewTab();
            AddNewTab();
            DataContext = this;
            /*((FileTabItem)_tabItems[0]).Document.Text = 
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
voltage(""3_a6"",""g"");";*/
            FileTab.SelectedIndex = 0;
            ((FileTabItem)_tabItems[0]).Filename = "Old_language.psms";
            ((FileTabItem)_tabItems[0]).Document.Text =
@"v1=voltageSource(""g"", ""3_a1"", 220.0, 0.0, 60.0);
v2=voltageSource(""3_a5"", ""3_a4"", 220.0, 0.0, 60.0);
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

            ((FileTabItem)_tabItems[1]).Filename = "New_language.psms";
            ((FileTabItem)_tabItems[1]).Document.Text =
@"model:
    steadystate{
	    solver = newton{
		    iterations = 20,
		    fAbsTol = 0.005,
		    alpha = 1.0
	    },
        baseFrequency = 60
	};
    /*
    transient{
	    solver = radauIIA3{
		    iterations = 20,
		    fAbsTol = 0.01,
		    alpha = 1.0,
            step = 0.004
	    },
        baseFrequency = 60,
        t0 = 0,
        t1 = 5./60//5 cycles
	};
    */
elements:
    v1 = VoltageSource{
        Peak = 220,
        Phase = 0,
        Frequency = 60 //Hz
    };
    scope1 = Scope1p{Label=""V1""};
    scope2 = Scope1p{Label=""C2""};
    g = Ground{};
    v2 = VoltageSource{
        Peak = 220,
        Phase = 0,
        Frequency = 60
    };
    r1 = Resistor{R = 10};
    r2 = Resistor{R = 10};
    i1 = Inductor{L = 5};
    i2 = Inductor{L = 5};
    c1 = Capacitor{C = 0.02};
    c2 = Capacitor{C = 0.04};
    c3 = Capacitor{C = 0.01};
connections:
    connect(g.in,v1.in);
    connect(v1.out,scope1.in);
    connect(scope1.out,c3.in);
    connect(c3.in,i1.in);
    connect(c3.out,i2.in);
    connect(c3.out,r1.in);
    connect(i2.out,scope2.in);
    connect(scope2.out,c2.in);
    connect(c2.out,v2.out);
    connect(v2.in,r1.out);
    connect(v2.in,r2.in);
    connect(r2.out,i1.out);
    connect(r2.out,c1.in);
    connect(g.in,c1.out);
";

            ((FileTabItem)_tabItems[2]).Filename = "Algebraic equations.eq";
            ((FileTabItem)_tabItems[2]).Document.Text = 
@"constant a = 2;
x * x + a = e()^x * sin(x);
x(0) = 0;";

            ((FileTabItem)_tabItems[3]).Filename = "DAE.eq";
            ((FileTabItem)_tabItems[3]).Document.Text =
@"constant a = 2;
constant t0 = 0;
constant time = 1;

der(x) = a * x + y;
der(y) = y;
z = x + y;
x(t0) = 1;
y(t0) = 1;
z(t0) = 1;";

            ((FileTabItem)_tabItems[4]).Filename = "DAE2_RLC_parallel.eq";
            ((FileTabItem)_tabItems[4]).Document.Text =
@"constant Y = 1;
constant C = 0.01;
constant L = 1;
constant E = 0.5;
constant t0 = 0;
constant f = 60;
constant w = f*2*pi();
constant time = 5/f;//5 cycles

-I_e+Y*v_1+I_l+C*der(v_1) = 0;
v_1 = -E*sin(w*t);
v_1 = L * der(I_l);
I_e(t0) = 0;
v_1(t0) = 0;
I_l(t0) = 0;
";
            ((FileTabItem)_tabItems[5]).Filename = "DAE2_RLC_serial.eq";
            ((FileTabItem)_tabItems[5]).Document.Text =
@"constant Y = 1;
constant C = 0.01;
constant L = 1;
constant E = 0.5;
constant t0 = 0;
constant f = 60;
constant w = f*2*pi();
constant time = 5/f;//5 cycles

-I_e+Y*(v_1-v_2) = 0;
I_l-Y*(v_1-v_2)=0;
der(v_3)*C-I_l=0;
v_1 = -E*sin(w*t);
L*der(I_l)=(v_2-v_3);
v_1(t0) = 0;
v_2(t0) = 0;
v_3(t0) = 0;
I_e(t0) = 0;
I_l(t0) = 0;
";

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
        private void AddNewTab()
        {
            FileTabItem tab = new FileTabItem();
            _tabItems.Add(tab);
            FileTab.SelectedIndex = _tabItems.Count - 1;
        }
        private void ClearOutput()
        {
            OutputText = "";
            errors.Clear();
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
            string content = File.ReadAllText(filepath);
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
        private static void FormOutput(MathNet.Numerics.LinearAlgebra.Vector<double> solution, Equations.Nonlinear.NonlinearEquationDescription system, ref List<string> outputList)
        {
            for (int i = 0; i < solution.Count; i++)
            {
                outputList.Add(system.VariableNames[i] + " = " + solution[i].ToString());
            }
            double[] x = solution.ToArray();
            for (int i = 0; i < system.Equations.Count; i++)
            {
                outputList.Add($"F{i}(X) = {system.Equations[i].Execute(x)}");
            }
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
#region CLICK_METHODS
        private async void RunNonlinearTest_Click(object sender, RoutedEventArgs e)
        {
            UIEnabled = false;
            ClearOutput();
            try
            {
                await Task.Run(() => RunNonlinearEquationsTest(this));
            }
            catch (Exception exc)
            {
                Console.Write(exc.Message);
            }
            UIEnabled = true;
            Expander.IsExpanded = true;
        }
        private async void RunMenuButton_Click(object sender, RoutedEventArgs e)
        {
            UIEnabled = false;
            ClearOutput();
            await Task.Run(() => Run(this));
            UIEnabled = true;
            Expander.IsExpanded = true;
        }
        private void RLCParallelCircuit_Click(object sender, RoutedEventArgs e)
        {
            string content =
@"   //rlc series scheme

model:
    steadystate{
        solver = newton{
            iterations = 20,
            fAbsTol = 0.01,
            alpha = 1.0
        }
    };
    /*
    transient{
	    solver = radauIIA3{
		    iterations = 20,
		    fAbsTol = 0.01,
		    alpha = 1.0,
            step = 0.002
	    },
        baseFrequency = 60,
        t0 = 0,
        t1 = 5./60//5 cycles
	};
    */
elements:
    v1 = voltageSource{
       Peak = 220,
       Phase = 0,
       Frequency = 50
    };
    g = ground{
    };
    scopeVR = scope1p{Label=""scopeVR""};
    scopeRL = scope1p{Label=""scopeRL""};
    scopeLC = scope1p{Label=""scopeLC""};
    r1 = resistor{R = 10};
    l1 = inductor{L = 5};
    c1 = capacitor{C = 0.01};
connections:
    connect(v1.in, g.in);
    connect(v1.out, scopeVR.in);
    connect(scopeVR.out, r1.in);
    connect(r1.out,scopeRL.in);
    connect(scopeRL.out,l1.in);
    connect(l1.out,scopeL.in);
    connect(scopeLC.out,c1.in);
    connect(c1.out,g.in);";
            FileTabItem tab = new FileTabItem("RLC_series_circuit.psms", null, content);
            _tabItems.Add(tab);
            FileTab.SelectedIndex = _tabItems.Count - 1;
        }
        private void RLCSeriesCircuit_Click(object sender, RoutedEventArgs e)
        {
            string content =
@"   //rlc parallel scheme
           
model:
    steadystate{
        solver = newton{
            iterations = 20,
            fAbsTol = 0.01,
            alpha = 1.0
        }
    };
    /*transient{
	    solver = radauIIA3{
		    iterations = 20,
		    fAbsTol = 0.01,
		    alpha = 1.0,
            step = 0.002
	    },
        baseFrequency = 60,
        t0 = 0,
        t1 = 5/60//5 cycles
	};*/
elements:
    v1 = voltageSource{
       Peak = 220,
       Phase = 0,
       Frequency = 50
    };
    g = ground{};
    scopeR = scope1p{Label=""scopeR""};
    scopeL = scope1p{Label=""scopeL""};
    scopeC = scope1p{Label=""scopeC""};
    scopeV = scope1p{Label=""scopeV""};
    r1 = resistor{R = 10};
    l1 = inductor{L = 5};
    c1 = capacitor{C = 0.01};
connections:
    connect(v1.in, g.in);
    connect(v1.out, scopeV.in);
    connect(scopeV.out, r1.in);
    connect(scopeV.out, l1.in);
    connect(scopeV.out, c1.in);
    connect(r1.out,scopeR.in);
    connect(l1.out,scopeL.in);
    connect(c1.out,scopeC.in);
    connect(scopeL.out,g.in);
    connect(scopeR.out,g.in);
    connect(scopeC.out,g.in);";
            FileTabItem tab = new FileTabItem("RLC_parallel_circuit.psms", null, content);
            _tabItems.Add(tab);
            FileTab.SelectedIndex = _tabItems.Count - 1;
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
        private async void RunDAETest_Click(object sender, RoutedEventArgs e)
        {
            DAESolverDialog dialog = new DAESolverDialog();
            if (!dialog.ShowDialog().Value)
            {
                return;
            }
            double alpha = dialog.Alpha;
            double fAbsTol = dialog.FAbsTol;
            double step = dialog.Step;
            int iterations = dialog.Iterations;
            int method = dialog.SelectedMethod;
            //Тестирование решения ДАУ системы
            UIEnabled = false;
            ClearOutput();
            try
            {
                await Task.Run(() => RunDAETest(this,alpha,fAbsTol,step,iterations,method));
            }
            catch (Exception exc)
            {
                Console.Write(exc.Message);
            }
            UIEnabled = true;
            Expander.IsExpanded = true;
        }
        private void PowerSystemExample_Click(object sender, RoutedEventArgs e)
        {
            string content =
@"   //power system example
model:
	steadystate{
		solver = newton{
			iterations = 20,
			fAbsTol = 0.005,
			alpha = 1.0
		},
        baseFrequency = 60
	};
    /*transient{
	    solver = radauIIA3{
		    iterations = 20,
		    fAbsTol = 0.01,
		    alpha = 1.0,
            step = 0.002
	    },
        baseFrequency = 60,
        t0 = 0,
        t1 = 5/60//5 cycles
	};*/
elements:
	generator1 = generatorY{
		Peak = 100.0,
		Phase = 0.0,
		Z = 0.01+ j 0.001,
        Frequency = 60 //in Herz
	};
	scope1 = scope3p{Label = ""Generator""};
    scope2 = scope3p{Label = ""Load""};
    scope3 = scope1p{Label = ""ground""};
    resistorGen1 = resistor{R = 1000};
    line1 = linePiSection{
		  R = 0.02,
          L = 0.01,
          B = 0.1,
          G = 1000,
          Bp = 0.1
	};
	transformer1 = transformerDy{
		K = 10,
		Zs = 0.1 + j 0.5,
		Zp = 0.01 + j 0.1,
        Rc = 1000,
        Xm = 10000,
        Group = Dy1
	};
	transformer2 = transformerDd{
		K = 10,
		Zs = 0.1 + j 0.5,
		Zp = 0.01 + j 0.1,
        Rc = 1000.0,
        Xm = 10000,
        Group = Dd0
	};
	load1 = loadY{
		ZA = 1,
		ZB = 1,
		ZC = 1
	};
	ground = ground{};
connections:
	connect(generator1.n, resistorGen1.in);
    connect(resistorGen1.out,scope3.in);
    connect(scope3.out, ground.in);
    connect(generator1.out, scope1.in);
    connect(scope1.out, transformer1.in);
    connect(transformer1.out, line1.in);
    connect(transformer1.out_n, ground.in);
    connect(line1.out, transformer2.out);
    connect(transformer2.in, scope2.in);
    connect(scope2.out, load1.in);
    connect(load1.n, ground.in);";
            FileTabItem tab = new FileTabItem("Power_system_example.psms", null, content);
            _tabItems.Add(tab);
            FileTab.SelectedIndex = _tabItems.Count - 1;
        }
        private async void RunGenerateEquationsButton_Click(object sender, RoutedEventArgs e)
        {
            UIEnabled = false;
            ClearOutput();
            await Task.Run(() => RunEquationGeneration(this));
            UIEnabled = true;
            Expander.IsExpanded = true;
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
        private void SaveAll_Click(object sender, RoutedEventArgs e)
        {
            SaveAllCommand();
        }
#endregion
#region TASKS
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
                Equations.Nonlinear.Compiler compiler = new Equations.Nonlinear.Compiler();
                Equations.Nonlinear.NonlinearEquationDescription compiledEquation = compiler.CompileEquations(text);
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
            catch (Equations.CompilerException exc)
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
#if TEST||DEBUG
                    window.OutputText += exc.StackTrace;
#endif
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
        internal void Run(MainWindow window)
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
                Dispatcher.Invoke(() => {
                    Scheme.Interpreter.Interpreter.RunModel(text, ref errorList, ref outputList);
                });
            }
            catch (Equations.CompilerException exc)
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
                Dispatcher.Invoke(() => {
                    window.OutputText += exc.Message;
                    window.OutputText += "\n";
#if TEST||DEBUG
                    window.OutputText += exc.StackTrace;
#endif
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
                Equations.Nonlinear.Compiler compiler = new Equations.Nonlinear.Compiler();
                Equations.Nonlinear.NonlinearEquationDescription compiledEquation = compiler.CompileEquations(text);
                outputList.Add("Variables:");
                outputList.Add(compiledEquation.PrintVariables());
                outputList.Add(compiledEquation.PrintEquations());
                outputList.Add(compiledEquation.PrintJacobiMatrix(false));
            }
            catch (Equations.CompilerException exc)
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
#if TEST||DEBUG
                    window.OutputText += exc.StackTrace;
#endif
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
                Equations.DAE.Compiler compiler = new Equations.DAE.Compiler();
                Equations.DAE.Implicit.DAEIDescription compiledEquation = compiler.CompileDAEImplicit(text);
                outputList.Add("Variables:");
                outputList.Add(compiledEquation.PrintVariables());
                outputList.Add("Equations:");
                outputList.Add(compiledEquation.PrintEquations());
            }
            catch (Equations.CompilerException exc)
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
#if TEST||DEBUG
                    window.OutputText += exc.StackTrace;
#endif
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
        internal void RunDAETest(MainWindow window,double alpha,double fAbsTol,double step,int iterations,int method)
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
                Equations.DAE.Compiler compiler = new Equations.DAE.Compiler();
                Equations.DAE.Implicit.DAEIDescription compiledEquation = compiler.CompileDAEImplicit(text);

                //Equations.DAE.Implicit.RADAUIIA3 solver = new Equations.DAE.Implicit.RADAUIIA3();
                Equations.DAE.Implicit.DAEISolver solver;
                switch (method)
                {
                    case (int)METHOD.RADAUIIA3:
                        solver = new Equations.DAE.Implicit.RADAUIIA3(fAbsTol,iterations,alpha,step);
                        break;
                    case (int)METHOD.RADAUIIA5:
                        solver = new Equations.DAE.Implicit.RADAUIIA5(fAbsTol, iterations, alpha, step);
                        break;
                    case (int)METHOD.BDF:
                        solver = new Equations.DAE.Implicit.BDF1(fAbsTol, iterations, alpha, step);
                        break;
                    case (int)METHOD.BDF2:
                        solver = new Equations.DAE.Implicit.BDF2(fAbsTol, iterations, alpha, step);
                        break;
                    case (int)METHOD.TRAPEZOID:
                        throw new NotImplementedException();
                    default:
                        throw new NotImplementedException();
                }
                Equations.DAE.Solution solution = Equations.DAE.Implicit.DAEISolver.Solve(compiledEquation, solver);
                window.Dispatcher.Invoke(() =>
                {
                    solution.ShowResults();
                });
            }
            catch (Equations.CompilerException exc)
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
#if TEST||DEBUG
                    window.OutputText += exc.StackTrace;
#endif
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
        internal void RunEquationGeneration(MainWindow window)
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
                Scheme.Interpreter.Interpreter.EquationGeneration(text, ref errorList, ref outputList);
            }
            catch (Exception exc)
            {
                Dispatcher.Invoke(() => {
                    window.OutputText += exc.Message;
                    window.OutputText += "\n";
#if TEST||DEBUG
                    window.OutputText += exc.StackTrace;
#endif
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
#endregion
#region COMMANDS
#region CANEXECUTE
        private void HelpCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
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
        private void OpenCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (FileTab == null)
            {
                e.CanExecute = false;
                return;
            }
            e.CanExecute = FileTab.Items.Count < 21;
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
        private void ExitCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private bool CanExecuteDelete(object parameter)
        {
            return _tabItems.Count > 0;
        }
#endregion
#region EXECUTE
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
        private void SaveCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            FileTabItem item = FileTab.SelectedItem as FileTabItem;
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
        private void OpenCommandExecuted(object sender, ExecutedRoutedEventArgs e)
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
                FileTab.SelectedIndex = _tabItems.Count - 1;
            }
            catch (Exception)
            {
                return;
            }
        }
        private void NewCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            AddNewTab();
        }
        private void ExitCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
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
                    AddNewTab();
                if (index == selectedIndex)
                {
                    FileTab.SelectedIndex = 0;
                }
                _tabItems.RemoveAt(index);
            }
        }
#endregion
#endregion
    }
}
