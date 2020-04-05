using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Equations.DAE
{
    class DAESolution
    {
        List<double[]> values;
        List<double> time;
        string[] variables;
        public DAESolution(List<double[]> values,List<double> time, string[] variables)
        {
            this.values = values;
            this.time = time;
            this.variables = variables;
        }
        public void ShowResults()
        {
            ChartWindow window = new ChartWindow();
            window.Plot(values,time,variables);
            window.Show();
        }
    }
}
