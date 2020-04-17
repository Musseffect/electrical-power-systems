using ElectricalPowerSystems.Equations.Expression;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ElectricalPowerSystems.Equations.DAE.SemiExplicit
{
    public class DAESEDescription : DAEDescription
    {
        public DAESEDescription(
            string[] variableNamesX,
            string[] variableNamesZ,
            string[] parameterNames,
            double[] parameterValues,
            double[] initialValuesX,
            double[] initialValuesZ,
            List<RPNExpression> f,
            List<RPNExpression> g,
            MathUtils.SparseMatrix<RPNExpression> dfdx,
            MathUtils.SparseMatrix<RPNExpression> dfdz,
            MathUtils.SparseMatrix<RPNExpression> dgdx,
            MathUtils.SparseMatrix<RPNExpression> dgdz) : base(parameterNames, parameterValues)
        {
            this.variableNamesX = variableNamesX;
            this.variableNamesZ = variableNamesZ;
            this.initialValuesX = initialValuesX;
            this.initialValuesZ = initialValuesZ;
            this.f = f;
            this.g = g;
            this.dfdx = dfdx;
            this.dfdz = dfdz;
            this.dgdx = dgdx;
            this.dgdz = dgdz;
        }
        string[] variableNamesX;
        string[] variableNamesZ;
        double[] initialValuesX;
        double[] initialValuesZ;
        double t0;
        double time;
        List<RPNExpression> f;
        List<RPNExpression> g;
        MathUtils.SparseMatrix<RPNExpression> dfdx;
        MathUtils.SparseMatrix<RPNExpression> dfdz;
        MathUtils.SparseMatrix<RPNExpression> dgdx;
        MathUtils.SparseMatrix<RPNExpression> dgdz;
        public int SizeX { get { return variableNamesX.Length; } }
        public int SizeZ { get { return variableNamesZ.Length; } }
        public string[] VariableNamesX { get { return variableNamesX; } }
        public string[] VariableNamesZ { get { return variableNamesZ; } }
        public string[] VariableNames { get { return variableNamesX.Concat(variableNamesZ).ToArray(); } }
        public double[] InitialValuesX { get { return initialValuesX; } }
        public double[] InitialValuesZ { get { return initialValuesZ; } }
        public double T0 { get { return t0; } }
        public double Time { get { return time; } }
        public List<RPNExpression> F { get { return f; } }
        public List<RPNExpression> G { get { return g; } }
        public MathUtils.SparseMatrix<RPNExpression> DfdX { get { return dfdx; } }
        public MathUtils.SparseMatrix<RPNExpression> DfdZ { get { return dfdz; } }
        public MathUtils.SparseMatrix<RPNExpression> DgdX { get { return dgdx; } }
        public MathUtils.SparseMatrix<RPNExpression> DgdZ { get { return dgdz; } }
        public override Dictionary<string, int> GetVariableDictionary()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            for (int i = 0; i < SizeX; i++)
            {
                result.Add(variableNamesX[i], i);
            }
            for (int i = SizeZ; i < SizeX + SizeZ; i++)
            {
                result.Add(variableNamesX[i], i);
            }
            return result;
        }
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
            List<string> list = new List<string>();
            list.Add("t");
            list.AddRange(variableNamesX);
            list.AddRange(variableNamesZ);
            string[] variableNames = list.ToArray();
            string result = "";
            for (int i = 0; i < f.Count; i++)
            {
                result += $"der({variableNamesX[i]}) = {RPNExpression.Print(f[i], variableNames)}{Environment.NewLine}";
            }
            for (int i = 0; i < g.Count; i++)
            {
                result += $"0 = {RPNExpression.Print(g[i], variableNames)}{Environment.NewLine}";
            }
            return result;
        }
    }
}