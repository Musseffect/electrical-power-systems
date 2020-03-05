#define DAE

#if DAE
using ElectricalPowerSystems.Interpreter.Equations.Expression;
using ElectricalPowerSystems.MathUtils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ElectricalPowerSystems.Interpreter.Equations.DAE
{
    public abstract class DAEDefinition
    {
        public abstract string PrintVariables();
        public abstract string PrintEquations();
    }
    public class DAESemiImplicitDefinition : DAEDefinition
    {
        private DAESemiImplicitDefinition()
        { }
        public DAESemiImplicitDefinition(string[] variableNamesX, string[] variableNamesZ, double[] initialValuesX, double[] initialValuesZ
            , List<RPNExpression> f, List<RPNExpression> g,
            SparseMatrix<RPNExpression> dfdx, SparseMatrix<RPNExpression> dfddx,
            SparseMatrix<RPNExpression> dfdz, SparseMatrix<RPNExpression> dgdx, SparseMatrix<RPNExpression> dgdz)
        {
            this.variableNamesX = variableNamesX;
            this.variableNamesZ = variableNamesZ;
            this.initialValuesX = initialValuesX;
            this.initialValuesZ = initialValuesZ;
            this.f = f;
            this.g = g;
        }
        public double[] InitialValuesX { get { return initialValuesX; } }
        public double[] InitialValuesZ { get { return initialValuesZ; } }
        string[] variableNamesX;
        string[] variableNamesZ;
        double[] initialValuesX;
        double[] initialValuesZ;
        List<RPNExpression> f;
        List<RPNExpression> g;
        public List<RPNExpression> F { get { return f; } }
        public List<RPNExpression> G { get { return g; } }
        public string[] VariableNamesX { get { return variableNamesX; } }
        public string[] VariableNamesZ { get { return variableNamesZ; } }
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
            list.AddRange(variableNamesX);
            list.AddRange(variableNamesZ);
            string[] variableNames = list.ToArray();
            string result = "";
            for (int i = 0; i < f.Count; i++)
            {
                result += "der(" + variableNamesX[i] + ")" + " = " + RPNExpression.print(f[i], variableNames) + Environment.NewLine;
            }
            for (int i = 0; i < g.Count; i++)
            {
                result += "0 = " + RPNExpression.print(g[i], variableNames) + Environment.NewLine;
            }
            return result;
        }
    }
    public class DAEImplicitDefinition : DAEDefinition
    {
        private DAEImplicitDefinition()
        {
        }
        public DAEImplicitDefinition(string[] variableNames, double[] initialValues, List<RPNExpression> f,
            SparseMatrix<RPNExpression> dfdx, SparseMatrix<RPNExpression> dfddx, double t0, double time)
        {
            this.variableNamesFull = variableNames;
            this.variableNames = variableNames.Skip(1).Take(initialValues.Length).ToArray();
            this.initialValues = initialValues;
            this.f = f;
            this.t0 = t0;
            this.time = time;
            this.dfddx = dfddx;
            this.dfdx = dfdx;
        }
        public double[] InitialValues { get { return initialValues; } }
        public string[] VariableNames { get { return variableNames; } }
        public string[] VariableNamesFull { get { return variableNamesFull; } }
        string[] variableNames;
        string[] variableNamesFull;
        double[] initialValues;
        double t0;
        double time;
        //string[] parameterNames;
        //double[] parameters;
        List<RPNExpression> f;
        SparseMatrix<RPNExpression> dfdx;
        SparseMatrix<RPNExpression> dfddx;
        public SparseMatrix<RPNExpression> DfdX { get { return dfdx; } }
        public SparseMatrix<RPNExpression> DfddX { get { return dfddx; } }
        public List<RPNExpression> F { get { return f; } }
        public double T0 { get { return t0; } }
        public double Time { get { return time; } }
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
            string result = "";
            for (int i = 0; i < f.Count; i++)
            {
                result += $"f[{i + 1}] = " + RPNExpression.print(f[i], variableNamesFull) + " = 0" + Environment.NewLine;
            }
            return result;
        }
    }
#endif
}