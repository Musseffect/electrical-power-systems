using ElectricalPowerSystems.Equations.Expression;
using ElectricalPowerSystems.MathUtils;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Equations.DAE.Explicit
{
    /// <summary>
    /// <para> x' = f(x,z,t)</para>
    /// z = g(x,t)
    /// </summary>
    public abstract class DAEESystem
    {
        public int SizeX { get; protected set; }
        public int SizeZ { get; protected set; }
        public int Size { get { return SizeX + SizeZ; } }
        public abstract Vector<double> F(Vector<double> x, Vector<double> z, double t);
        public abstract Vector<double> G(Vector<double> x, Vector<double> z, double t);
        public abstract Matrix<double> DFdX(Vector<double> x, Vector<double> z, double t);
        public abstract Matrix<double> DFdZ(Vector<double> x, Vector<double> z, double t);
        public abstract Matrix<double> DGdX(Vector<double> x, Vector<double> z, double t);
    }
    public class DAEEAnalytic:DAEESystem
    {
        RPNExpression[] fEquations;
        RPNExpression[] gEquations;
        SparseMatrix<RPNExpression> dFdXequations;
        SparseMatrix<RPNExpression> dFdZequations;
        SparseMatrix<RPNExpression> dGdXequations;
        double[] parameters;
        Dictionary<string, int> parameterIndicies;
        public DAEEAnalytic(DAEEDescription description)
        {
            fEquations = description.F.ToArray();
            gEquations = description.G.ToArray();
            SizeX = description.SizeX;
            SizeZ = description.SizeZ;
            dFdXequations = description.DfdX;
            dFdZequations = description.DfdZ;
            dGdXequations = description.DgdX;
            parameters = description.Parameters;
            parameterIndicies = description.GetParameterDictionary();
        }
        public override Vector<double> F(Vector<double> x, Vector<double> z, double t)
        {
            double[] result = new double[Size];
            List<double> lx = new List<double>
            {
                t
            };
            lx.AddRange(x);
            lx.AddRange(z);
            lx.AddRange(parameters);
            double[] _x = lx.ToArray();
            for (int i = 0; i < SizeX; i++)
            {
                result[i] = fEquations[i].Execute(_x);
            }
            return Vector<double>.Build.Dense(result);
        }
        public override Vector<double> G(Vector<double> x, Vector<double> z, double t)
        {
            double[] result = new double[Size];
            List<double> lx = new List<double>
            {
                t
            };
            lx.AddRange(x);
            lx.AddRange(parameters);
            double[] _x = lx.ToArray();
            for (int i = 0; i < SizeZ; i++)
            {
                result[i] = gEquations[i].Execute(_x);
            }
            return Vector<double>.Build.Dense(result);
        }
        public override Matrix<double> DFdX(Vector<double> x, Vector<double> z, double t)
        {
            List<double> lx = new List<double>();
            lx.Add(t);
            lx.AddRange(x);
            lx.AddRange(z);
            lx.AddRange(parameters);
            double[] _x = lx.ToArray();
            Matrix<double> result = Matrix<double>.Build.Sparse(Size, Size);
            foreach (var entry in dFdXequations.GetEntries())
            {
                result.At(entry.J, entry.I, entry.Value.Execute(_x));
            }
            return result;
        }
        public override Matrix<double> DFdZ(Vector<double> x, Vector<double> z, double t)
        {
            List<double> lx = new List<double>();
            lx.Add(t);
            lx.AddRange(x);
            lx.AddRange(z);
            lx.AddRange(parameters);
            double[] _x = lx.ToArray();
            Matrix<double> result = Matrix<double>.Build.Sparse(Size, Size);
            foreach (var entry in dFdZequations.GetEntries())
            {
                result.At(entry.J, entry.I, entry.Value.Execute(_x));
            }
            return result;
        }
        public override Matrix<double> DGdX(Vector<double> x, Vector<double> z, double t)
        {
            List<double> lx = new List<double>();
            lx.Add(t);
            lx.AddRange(x);
            lx.AddRange(parameters);
            double[] _x = lx.ToArray();
            Matrix<double> result = Matrix<double>.Build.Sparse(Size, Size);
            foreach (var entry in dGdXequations.GetEntries())
            {
                result.At(entry.J, entry.I, entry.Value.Execute(_x));
            }
            return result;
        }
        public void UpdateParameters(List<Parameter> parameters)
        {
            foreach (var parameter in parameters)
            {
                int index = parameterIndicies[parameter.Name];
                this.SetParameter(index, parameter.Value);
            }
        }
        private void SetParameter(int index, double value)
        {
            parameters[index] = value;
        }
    }
}
