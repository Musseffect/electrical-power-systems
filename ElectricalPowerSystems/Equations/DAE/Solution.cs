using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Equations.DAE
{
    public class Solution
    {
        List<double[]> values;
        List<double> time;
        string[] variables;
        public Solution(List<double[]> values,List<double> time, string[] variables)
        {
            this.values = values;
            this.time = time;
            this.variables = variables;
        }
        public void ShowResults()
        {
            GUI.ModelEditor.Windows.ChartWindow window = new GUI.ModelEditor.Windows.ChartWindow();
            window.Plot(values,time,variables);
            window.Show();
        }
    }
}
