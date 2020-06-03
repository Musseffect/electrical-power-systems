//#define ALTERNATIVE
using ElectricalPowerSystems.Equations.Expression;
using ElectricalPowerSystems.MathUtils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ElectricalPowerSystems.Equations.DAE.Implicit
{
#if !ALTERNATIVE
    public class DAEIDescription : DAEDescription
    {
        public DAEIDescription(
            string[] variableNames,
            string[] parameterNames,
            double[] parameterValues,
            double[] initialValues,
            List<RPNExpression> f,
            MathUtils.SparseMatrix<RPNExpression> dfdx,
            MathUtils.SparseMatrix<RPNExpression> dfddx,
            double t0,
            double time):base(parameterNames,parameterValues)
        {
            this.variableNames = variableNames;
            this.initialValues = initialValues;
            this.parameterNames = parameterNames;
            this.parameterValues = parameterValues;
            this.f = f;
            this.t0 = t0;
            this.time = time;
            this.dfddx = dfddx;
            this.dfdx = dfdx;
        }
        string[] variableNames;
        double[] initialValues;
        double t0;
        double time;
        List<RPNExpression> f;
        MathUtils.SparseMatrix<RPNExpression> dfdx;
        MathUtils.SparseMatrix<RPNExpression> dfddx;
        public int Size { get { return variableNames.Length; } }
        public string[] VariableNames { get { return variableNames; } }
        public double[] InitialValues { get { return initialValues; } }
        public double T0 { get { return t0; } }
        public double Time { get { return time; } }
        public List<RPNExpression> F { get { return f; } }
        public MathUtils.SparseMatrix<RPNExpression> DfdX { get { return dfdx; } }
        public MathUtils.SparseMatrix<RPNExpression> DfddX { get { return dfddx; } }
        public override Dictionary<string, int> GetVariableDictionary()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            for (int i = 0; i <Size;i++)
            {
                result.Add(variableNames[i],i);
            }
            return result;
        }
        public override string PrintVariables()
        {
            string result = "";
            for (int i = 0; i < initialValues.Length; i++)
            {
                result += (i + 1) + ". " + variableNames[i] + " = " + initialValues[i].ToString() + Environment.NewLine;
            }
            return result;
        }
        public override string PrintEquations()
        {
            List<string> namesList = new List<string>();
            namesList.Add("t");
            namesList.AddRange(variableNames);
            foreach (var name in variableNames)
            {
                namesList.Add("der("+name+")");
            }
            namesList.AddRange(parameterNames);
            string[] nameArray = namesList.ToArray();
            string result = "";
            for (int i = 0; i < f.Count; i++)
            {
                result += $"f[{i + 1}] = " + RPNExpression.Print(f[i], nameArray) + " = 0" + Environment.NewLine;
            }
            return result;
        }
    }
#else
    public class DAEIDescription : DAEDescription
    {
        public DAEIDescription(
            string[] variableNamesX,
            string[] variableNamesZ,
            string[] parameterNames,
            double[] parameterValues,
            double[] initialValuesX,
            double[] initialValuesZ,
            List<RPNExpression> f,
            MathUtils.SparseMatrix<RPNExpression> dfdx,
            MathUtils.SparseMatrix<RPNExpression> dfddx,
            MathUtils.SparseMatrix<RPNExpression> dfdz,
            double t0,
            double time) : base(parameterNames, parameterValues)
        {
            this.variableNamesX = variableNamesX;
            this.variableNamesZ = variableNamesZ;
            this.initialValuesX = initialValuesX;
            this.initialValuesZ = initialValuesZ;
            this.parameterNames = parameterNames;
            this.parameterValues = parameterValues;
            this.f = f;
            this.t0 = t0;
            this.time = time;
            this.dfddx = dfddx;
            this.dfdx = dfdx;
            this.dfdz = dfdz;
        }
        string[] variableNamesX;
        string[] variableNamesZ;
        double[] initialValuesX;
        double[] initialValuesZ;
        double t0;
        double time;
        List<RPNExpression> f;
        MathUtils.SparseMatrix<RPNExpression> dfdx;
        MathUtils.SparseMatrix<RPNExpression> dfddx;
        MathUtils.SparseMatrix<RPNExpression> dfdz;
        public int SizeX { get { return variableNamesX.Length; } }
        public int SizeZ { get { return variableNamesZ.Length; } }
        public string[] VariableNamesX { get { return variableNamesX; } }
        public string[] VariableNamesZ { get { return variableNamesZ; } }
        public string[] VariableNames { get { return variableNamesX.AddRange(variableNamesZ).ToArray(); } }
        public double[] InitialValuesX { get { return initialValuesX; } }
        public double[] InitialValuesZ { get { return initialValuesZ; } }
        public double T0 { get { return t0; } }
        public double Time { get { return time; } }
        public List<RPNExpression> F { get { return f; } }
        public MathUtils.SparseMatrix<RPNExpression> DfdX { get { return dfdx; } }
        public MathUtils.SparseMatrix<RPNExpression> DfddX { get { return dfddx; } }
        public MathUtils.SparseMatrix<RPNExpression> DfdZ { get { return dfdz; } }
        public override string PrintVariables()
        {
            string result = "";
            result += "differential variables:" + Environment.NewLine;
            for (int i = 0; i < variableNamesX.Length; i++)
            {
                result += (i + 1) + ". " + variableNamesX[i] + " = " + initialValuesX[i].ToString() + Environment.NewLine;
            }
            result += "algebraic variables:" + Environment.NewLine;
            for (int i = 0; i < VariableNamesZ.Length; i++)
            {
                result += (i + 1) + ". " + VariableNamesZ[i] + " = " + initialValuesZ[i].ToString() + Environment.NewLine;
            }
            return result;
        }
        public override string PrintEquations()
        {
            List<string> namesList = new List<string>();
            namesList.Add("t");
            namesList.AddRange(variableNamesX);
            namesList.AddRange(variableNamesZ);
            namesList.AddRange(parameterNames);
            string[] nameArray = namesList.ToArray();
            string result = "";
            for (int i = 0; i < f.Count; i++)
            {
                result += $"f[{i + 1}] = " + RPNExpression.Print(f[i], nameArray) + " = 0" + Environment.NewLine;
            }
            return result;
        }
    }
#endif
}