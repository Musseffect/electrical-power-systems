using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;

namespace ElectricalPowerSystems.Scheme.Transient
{
    public class TransientState
    {
        private Vector<double> x;
        private Dictionary<string, int> variablesMap;

        public TransientState(Vector<double> x, Dictionary<string, int> variablesMap)
        {
            this.x = x;
            this.variablesMap = variablesMap;
        }
        public double GetValue(string name)
        {
            return x[variablesMap[name]];
        }
    }
}
