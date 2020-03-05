using ElectricalPowerSystems.Interpreter.Equations;
using ElectricalPowerSystems.Interpreter.Equations.DAE;
using ElectricalPowerSystems.Interpreter.Equations.Expression;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.MathUtils
{
    //Implicit
    //f(x,x',t)=0
    public abstract class DAEImplicitSystem
    {
        public float T0;
        public float Time;
        public int Size { get; protected set; }
        public abstract double[] F(Vector<double> x, Vector<double> dx, double t);
        public abstract Matrix<double> dFdX(Vector<double> x, Vector<double> dx, double t);
        public abstract Matrix<double> dFddX(Vector<double> x, Vector<double> dx, double t);
    }
    //SemiImplicit
    //0=f(x',x,z,t)
    //0=g(x,z,t)
    public abstract class DAESemiImplicitSystem
    {
        public int SizeX { get; protected set; }
        public int SizeZ { get; protected set; }
        public int Size { get { return SizeX + SizeZ; } }
        public abstract double[] F(Vector<double> x, Vector<double> z, double t);
        public abstract double[] G(Vector<double> x, Vector<double> z, double t);
        public abstract Matrix<double> dFdX(Vector<double> x, Vector<double> z, double t);
        public abstract Matrix<double> dFddX(Vector<double> x, Vector<double> z, double t);
        public abstract Matrix<double> dFdZ(Vector<double> x, Vector<double> z, double t);
        public abstract Matrix<double> dGdX(Vector<double> x, Vector<double> z, double t);
        public abstract Matrix<double> dGdZ(Vector<double> x, Vector<double> z, double t);
    }
    //linear DAE
    // A(t)x'+B(t)x=f(t) , A(t) - matrix NxN , B(t) - matrix NxN , f(t) - N-size vector
    class DAESemiImplicitSystemAnalytic : DAESemiImplicitSystem
    {
        RPNExpression[] fEquations;
        RPNExpression[] gEquations;
        SparseMatrix<RPNExpression> dFdXEquations;
        SparseMatrix<RPNExpression> dFddXEquations;
        SparseMatrix<RPNExpression> dFdZEquations;
        SparseMatrix<RPNExpression> dGdXEquations;
        SparseMatrix<RPNExpression> dGdZEquations;
        public DAESemiImplicitSystemAnalytic(DAESemiImplicitDefinition definition)
        {
            fEquations = definition.F.ToArray();
            gEquations = definition.G.ToArray();
            SizeX = fEquations.Length;
            SizeZ = gEquations.Length;
            /*dFdXEquations = definition.;
            dFddXEquations = ;
            dFdZEquations = ;
            dGdXEquations = ;
            dGdZEquations = ;*/
        }
        public override double[] F(Vector<double> x, Vector<double> z, double t)
        {
            throw new NotImplementedException();
        }

        public override double[] G(Vector<double> x, Vector<double> z, double t)
        {
            throw new NotImplementedException();
        }


        public override Matrix<double> dFddX(Vector<double> x, Vector<double> z, double t)
        {
            throw new NotImplementedException();
        }

        public override Matrix<double> dFdX(Vector<double> x, Vector<double> z, double t)
        {
            throw new NotImplementedException();
        }

        public override Matrix<double> dFdZ(Vector<double> x, Vector<double> z, double t)
        {
            throw new NotImplementedException();
        }

        public override Matrix<double> dGdX(Vector<double> x, Vector<double> z, double t)
        {
            throw new NotImplementedException();
        }

        public override Matrix<double> dGdZ(Vector<double> x, Vector<double> z, double t)
        {
            throw new NotImplementedException();
        }
    }
    class DAEImplicitSystemAnalytic : DAEImplicitSystem
    {
        RPNExpression[] equations;
        SparseMatrix<RPNExpression> dFddXequations;
        SparseMatrix<RPNExpression> dFdXequations;
        public DAEImplicitSystemAnalytic(DAEImplicitDefinition definition)
        {
            equations = definition.F.ToArray();
            Size = equations.Length;
            dFdXequations = definition.DfdX;
            dFddXequations = definition.DfddX;
        }
        public override Matrix<double> dFddX(Vector<double> x, Vector<double> dx, double t)
        {
            //double[,] result = new double[Size, Size];
            List<double> lx = new List<double>();
            lx.Add(t);
            lx.AddRange(x);
            lx.AddRange(dx);
            double[] _x = lx.ToArray();
            Matrix<double> result = Matrix<double>.Build.Sparse(Size,Size);
            foreach (var entry in dFddXequations.GetEntries())
            {
                result.At(entry.J,entry.I, entry.Value.execute(_x));
            }
            /*for (int j = 0; j < Size; j++)
            {
                for (int i = 0; i < Size; i++)
                {
                    result[j, i] = ;
                    result[j, i] = dFddXequations[j,i].execute(_x);
                }
            }*/
            return result;
        }

        public override Matrix<double> dFdX(Vector<double> x, Vector<double> dx, double t)
        {
            //double[,] result = new double[Size,Size];
            List<double> lx = new List<double>();
            lx.Add(t);
            lx.AddRange(x);
            lx.AddRange(dx);
            double[] _x = lx.ToArray();
            Matrix<double> result = Matrix<double>.Build.Sparse(Size, Size);
            foreach (var entry in dFdXequations.GetEntries())
            {
                result.At(entry.J, entry.I, entry.Value.execute(_x));
            }
            /*for (int j = 0; j < Size; j++)
            {
                for (int i = 0; i < Size; i++)
                {
                    result[j, i] = dFdXequations[j, i].execute(_x);
                }
            }*/
            return result;
        }

        public override double[] F(Vector<double> x, Vector<double> dx, double t)
        {
            double[] result = new double[Size];
            List<double> lx = new List<double>
            {
                t
            };
            lx.AddRange(x);
            lx.AddRange(dx);
            double[] _x = lx.ToArray();
            for (int i=0;i<Size;i++)
            {
                result[i] = equations[i].execute(_x);
            }
            return result;
        }
    }
}
