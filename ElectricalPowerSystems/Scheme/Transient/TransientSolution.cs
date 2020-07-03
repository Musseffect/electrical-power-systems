using System.Collections.Generic;

namespace ElectricalPowerSystems.Scheme.Transient
{
    public class TransientSolution
    {
        List<double[]> values;
        List<double> timeArray;
        string[] variableNames;
        int stateChangedCount;
        public void Plot()
        {
            GUI.ModelEditor.Windows.ChartWindow window = new GUI.ModelEditor.Windows.ChartWindow();
            window.Plot(values, timeArray, variableNames);
            window.Show();
        }
        public int GetPointCount()
        {
            return values.Count;
        }
        public TransientSolution(string[] variableNames)
        {
            this.values = new List<double[]>();
            this.timeArray = new List<double>();
            this.variableNames = variableNames;
        }
        public void AddPoint(double[] point, double time)
        {
            this.values.Add(point);
            this.timeArray.Add(time);
        }
        /*public void SaveTo(string filename)
        {
            
        }*/
        public void SetStateChangedCount(int stateChangedCount)
        {
            this.stateChangedCount = stateChangedCount;
        }
        public override string ToString()
        {
            return $"Transient solution: {{steps: {timeArray.Count}, changes of state: {stateChangedCount}}}";
        }
    }
}
