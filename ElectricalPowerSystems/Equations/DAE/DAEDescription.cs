using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Equations.DAE
{
    public abstract class DAEDescription
    {
        protected string[] parameterNames;
        protected double[] parameterValues;
        public double[] Parameters { get { return parameterValues; } }
        public DAEDescription(string[] parameterNames,double[]parameterValues)
        {
            this.parameterNames = parameterNames;
            this.parameterValues = parameterValues;
        }
        public Dictionary<string, int> GetParameterDictionary()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            for (int i = 0; i < parameterNames.Length; i++)
            {
                result.Add(parameterNames[i], i);
            }
            return result;
        }
        public abstract Dictionary<string, int> GetVariableDictionary();
        public abstract string PrintVariables();
        public abstract string PrintEquations();
    }
}
