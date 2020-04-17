//#define ALTERNATIVE
using ElectricalPowerSystems.Equations;
using ElectricalPowerSystems.Equations.DAE;
using ElectricalPowerSystems.Equations.Expression;
using ElectricalPowerSystems.MathUtils;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Equations.DAE.Implicit
{
#if !ALTERNATIVE
    /// <summary>
    /// f(x,x',t) = 0
    /// </summary>
    public abstract class DAEISystem
    {
        public float T0;
        public float Time;
        public int Size { get; protected set; }
        public abstract Vector<double> F(Vector<double> x, Vector<double> dx, double t);
        public abstract Matrix<double> DFdX(Vector<double> x, Vector<double> dx, double t);
        public abstract Matrix<double> DFddX(Vector<double> x, Vector<double> dx, double t);
    }
#else
    /// <summary>
    /// f(x,z,x',t) = 0
    /// </summary>
    public abstract class DAEISystem
    {
        public float T0;
        public float Time;
        public int SizeX { get; protected set; }
        public int SizeZ { get; protected set; }
        public int Size { get { return SizeX + SizeZ; } }
        public abstract Vector<double> F(Vector<double> x, Vector<double> z, Vector<double> dx, double t);
        public abstract Matrix<double> DFdX(Vector<double> x, Vector<double> z, Vector<double> dx, double t);
        public abstract Matrix<double> DFdZ(Vector<double> x, Vector<double> z, Vector<double> dx, double t);
        public abstract Matrix<double> DFddX(Vector<double> x, Vector<double> z, Vector<double> dx, double t);
    }

#endif
#if !ALTERNATIVE
    public class DAEIAnalytic : DAEISystem
    {
        RPNExpression[] equations;
        SparseMatrix<RPNExpression> dFddXequations;
        SparseMatrix<RPNExpression> dFdXequations;
        double[] parameters;
        Dictionary<string, int> parameterIndicies;
        public DAEIAnalytic(DAEIDescription definition)
        {
            equations = definition.F.ToArray();
            Size = equations.Length;
            dFdXequations = definition.DfdX;
            dFddXequations = definition.DfddX;
            parameters = definition.Parameters;
            parameterIndicies = definition.GetParameterDictionary();
        }
        public void UpdateParameters(List<Parameter> parameters)
        {
            foreach (var parameter in parameters)
            {
                int index = parameterIndicies[parameter.Name];
                this.SetParameter(index, parameter.Value);
                //this.parameters[index] = parameter.Value;
            }
        }
        private void SetParameter(int index, double value)
        {
            parameters[index] = value;
        }
        public override Matrix<double> DFddX(Vector<double> x, Vector<double> dx, double t)
        {
            List<double> lx = new List<double>();
            lx.Add(t);
            lx.AddRange(x);
            lx.AddRange(dx);
            lx.AddRange(parameters);
            double[] _x = lx.ToArray();
            Matrix<double> result = Matrix<double>.Build.Sparse(Size,Size);
            foreach (var entry in dFddXequations.GetEntries())
            {
                //result.At(entry.J,entry.I, entry.Value.Execute(_x));
                result.At(entry.I, entry.J, entry.Value.Execute(_x));
            }
            return result;
        }

        public override Matrix<double> DFdX(Vector<double> x, Vector<double> dx, double t)
        {
            List<double> lx = new List<double>();
            lx.Add(t);
            lx.AddRange(x);
            lx.AddRange(dx);
            lx.AddRange(parameters);
            double[] _x = lx.ToArray();
            Matrix<double> result = Matrix<double>.Build.Sparse(Size, Size);
            foreach (var entry in dFdXequations.GetEntries())
            {
                //result.At(entry.J, entry.I, entry.Value.Execute(_x));
                result.At(entry.I, entry.J, entry.Value.Execute(_x));
            }
            return result;
        }

        public override Vector<double> F(Vector<double> x, Vector<double> dx, double t)
        {
            double[] result = new double[Size];
            List<double> lx = new List<double>
            {
                t
            };
            lx.AddRange(x);
            lx.AddRange(dx);
            lx.AddRange(parameters);
            double[] _x = lx.ToArray();
            for (int i=0;i<Size;i++)
            {
                result[i] = equations[i].Execute(_x);
            }
            return Vector<double>.Build.Dense(result);
        }
    }
#else
    public class DAEIAnalytic : DAEISystem
    {
        RPNExpression[] equations;
        SparseMatrix<RPNExpression> dFddXequations;
        SparseMatrix<RPNExpression> dFdXequations;
        SparseMatrix<RPNExpression> dFdZequations;
        double[] parameters;
        Dictionary<string, int> parameterIndicies;
        public DAEIAnalytic(DAEIDescription description)
        {
            equations = description.F.ToArray();
            SizeX = description.SizeX;
            SizeZ = description.SizeZ;
            dFdXequations = description.DfdX;
            dFddXequations = description.DfddX;
            dFdZequations = description.DfdZ;
            parameters = description.Parameters;
            parameterIndicies = description.GetParameterDictionary();
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
        public override Matrix<double> DFddX(Vector<double> x, Vector<double> z, Vector<double> dx, double t)
        {
            List<double> lx = new List<double>();
            lx.Add(t);
            lx.AddRange(x);
            lx.AddRange(z);
            lx.AddRange(dx);
            lx.AddRange(parameters);
            double[] _x = lx.ToArray();
            Matrix<double> result = Matrix<double>.Build.Sparse(Size,Size);
            foreach (var entry in dFddXequations.GetEntries())
            {
                result.At(entry.J,entry.I, entry.Value.Execute(_x));
            }
            return result;
        }

        public override Matrix<double> DFdX(Vector<double> x, Vector<double> z, Vector<double> dx, double t)
        {
            List<double> lx = new List<double>();
            lx.Add(t);
            lx.AddRange(x);
            lx.AddRange(z);
            lx.AddRange(dx);
            lx.AddRange(parameters);
            double[] _x = lx.ToArray();
            Matrix<double> result = Matrix<double>.Build.Sparse(Size, Size);
            foreach (var entry in dFdXequations.GetEntries())
            {
                result.At(entry.J, entry.I, entry.Value.Execute(_x));
            }
            return result;
        }

        public override Vector<double> F(Vector<double> x, Vector<double> z, Vector<double> dx, double t)
        {
            double[] result = new double[Size];
            List<double> lx = new List<double>
            {
                t
            };
            lx.AddRange(x);
            lx.AddRange(z);
            lx.AddRange(dx);
            lx.AddRange(parameters);
            double[] _x = lx.ToArray();
            for (int i=0;i<Size;i++)
            {
                result[i] = equations[i].Execute(_x);
            }
            return Vector<double>.Build.Dense(result);
        }

        public override Matrix<double> DFdZ(Vector<double> x, Vector<double> z, Vector<double> dx, double t)
        {
            List<double> lx = new List<double>();
            lx.Add(t);
            lx.AddRange(x);
            lx.AddRange(z);
            lx.AddRange(dx);
            lx.AddRange(parameters);
            double[] _x = lx.ToArray();
            Matrix<double> result = Matrix<double>.Build.Sparse(Size, Size);
            foreach (var entry in dFdZequations.GetEntries())
            {
                result.At(entry.J, entry.I, entry.Value.Execute(_x));
            }
            return result;
        }
    }
#endif
}
