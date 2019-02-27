using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
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

namespace ElectricalPowerSystems
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<ModelParsing.ErrorMessage> errors;
        public ObservableCollection<ModelParsing.ErrorMessage> Errors
        {
            get { return errors; }
        }
        public MainWindow()
        {
            errors = new ObservableCollection<ModelParsing.ErrorMessage>();
            InitializeComponent();
            DataContext = this;
        }
        private void Run()
        {
            this.errors.Clear();
            List<ModelParsing.ErrorMessage> errorList = new List<ModelParsing.ErrorMessage>();
            OutputTextBox.Clear();
            List<string> outputList = new List<string>();
            try
            {
                Interpreter.MainInterpreter.compile(TextBox.Text, ref errorList, ref outputList);
            }
            catch (Exception exc)
            {
                OutputTextBox.AppendText(exc.Message);
                OutputTextBox.AppendText("\n");
                OutputTextBox.AppendText(exc.StackTrace);
                return;
            }
            foreach (ModelParsing.ErrorMessage error in errorList)
            {
                this.errors.Add(error);
            }
            foreach (var output in outputList)
            {
                OutputTextBox.AppendText(output);
                OutputTextBox.AppendText("\n");
            }
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
        private void RunMenuButton_Click(object sender, RoutedEventArgs e)
        {
            RunMenuButton.IsEnabled = false;
            Run();
            RunMenuButton.IsEnabled = true;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
