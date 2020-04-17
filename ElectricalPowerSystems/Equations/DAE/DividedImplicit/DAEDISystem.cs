using ElectricalPowerSystems.Equations.DAE;
using ElectricalPowerSystems.Equations.Expression;
using ElectricalPowerSystems.MathUtils;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Equations.DAE.DividedImplicit
{
    /// <summary>
    /// <para> 0 = f(x',x,z,t)</para>
    /// 0 = g(x,z,t)
    /// </summary>
    public abstract class DAEDISystem
    {
        public int SizeX { get; protected set; }
        public int SizeZ { get; protected set; }
        public int Size { get { return SizeX + SizeZ; } }
        public abstract Vector<double> F(Vector<double> x, Vector<double> z, double t);
        public abstract Vector<double> G(Vector<double> x, Vector<double> z, double t);
        public abstract Matrix<double> DFdX(Vector<double> x, Vector<double> z, double t);
        public abstract Matrix<double> DFddX(Vector<double> x, Vector<double> z, double t);
        public abstract Matrix<double> DFdZ(Vector<double> x, Vector<double> z, double t);
        public abstract Matrix<double> DGdX(Vector<double> x, Vector<double> z, double t);
        public abstract Matrix<double> DGdZ(Vector<double> x, Vector<double> z, double t);
    }
    public class DAEDIAnalytic : DAEDISystem
    {
        RPNExpression[] fEquations;
        RPNExpression[] gEquations;
        SparseMatrix<RPNExpression> dFdXequations;
        SparseMatrix<RPNExpression> dFddXequations;
        SparseMatrix<RPNExpression> dFdZequations;
        SparseMatrix<RPNExpression> dGdXequations;
        SparseMatrix<RPNExpression> dGdZequations;
        double[] parameters;
        Dictionary<string, int> parameterIndicies;
        public DAEDIAnalytic(DAEDIDescription description)
        {
            fEquations = description.F.ToArray();
            gEquations = description.G.ToArray();
            SizeX = description.SizeX;
            SizeZ = description.SizeZ;
            dFdXequations = description.DfdX;
            dFddXequations = description.DfddX;
            dFdZequations = description.DfdZ;
            dGdXequations = description.DgdX;
            dGdZequations = description.DfdZ;
        }
        public override Vector<double> F(Vector<double> x, Vector<double> z, double t)
        {
            throw new NotImplementedException();
        }
        public override Vector<double> G(Vector<double> x, Vector<double> z, double t)
        {
            throw new NotImplementedException();
        }
        public override Matrix<double> DFddX(Vector<double> x, Vector<double> z, double t)
        {
            List<double> lx = new List<double>();
            lx.Add(t);
            lx.AddRange(x);
            lx.AddRange(z);
            lx.AddRange(parameters);
            double[] _x = lx.ToArray();
            Matrix<double> result = Matrix<double>.Build.Sparse(Size, Size);
            foreach (var entry in dFddXequations.GetEntries())
            {
                result.At(entry.J, entry.I, entry.Value.Execute(_x));
            }
            return result;
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
            lx.AddRange(z);
            lx.AddRange(parameters);
            double[] _x = lx.ToArray();
            Matrix<double> result = Matrix<double>.Build.Sparse(Size, Size);
            foreach (var entry in dGdXequations.GetEntries())
            {
                result.At(entry.J, entry.I, entry.Value.Execute(_x));
            }
            return result;
        }
        public override Matrix<double> DGdZ(Vector<double> x, Vector<double> z, double t)
        {
            List<double> lx = new List<double>();
            lx.Add(t);
            lx.AddRange(x);
            lx.AddRange(z);
            lx.AddRange(parameters);
            double[] _x = lx.ToArray();
            Matrix<double> result = Matrix<double>.Build.Sparse(Size, Size);
            foreach (var entry in dGdZequations.GetEntries())
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
