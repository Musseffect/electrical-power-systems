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
        private ObservableCollection<ErrorMessage> errors;
        public ObservableCollection<ErrorMessage> Errors
        {
            get { return errors; }
        }
        public MainWindow()
        {
            errors = new ObservableCollection<ErrorMessage>();
            InitializeComponent();
            DataContext = this;
        }
        private void Run()
        {
            OutputTextBox.Clear();
            List<Token> tokens = Lexer.runLexer(TextBox.Text, ref errors);
            foreach (Token token in tokens)
            {
                OutputTextBox.AppendText(token.getTokenString());
                OutputTextBox.AppendText("\n");
            }
        }
        private void RunMenuButton_Click(object sender, RoutedEventArgs e)
        {
            RunMenuButton.IsEnabled = false;
            Run();
            RunMenuButton.IsEnabled = true;
        }
    }
}
