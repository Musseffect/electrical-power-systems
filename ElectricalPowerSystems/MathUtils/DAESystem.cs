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
    //f(x',x,t)=0
    abstract class DAEImplicitSystem
    {
        public int Size { get; protected set; }
        public abstract double[] F(Vector<double> x, Vector<double> dx, double t);
        public abstract double[,] dFdX(Vector<double> x, Vector<double> dx, double t);
        public abstract double[,] dFddX(Vector<double> x, Vector<double> dx, double t);
    }
    //SemiExplicit
    //x'=f(x,z,t)
    //0=g(x,z,t)
    abstract class DAESemiExplicitSystem
    {
        public int SizeX { get; protected set; }
        public int SizeZ { get; protected set; }
        public int Size { get { return SizeX + SizeZ; } }
        public abstract double[] F(Vector<double> x, Vector<double> z, double t);
        public abstract double[] G(Vector<double> x, Vector<double> z, double t);
        public abstract double[,] dFdX(Vector<double> x, Vector<double> z, double t);
        public abstract double[,] dFdZ(Vector<double> x, Vector<double> z, double t);
        public abstract double[,] dGdX(Vector<double> x, Vector<double> z, double t);
        public abstract double[,] dGdZ(Vector<double> x, Vector<double> z, double t);
    }
    //linear DAE
    // A(t)x'+B(t)x=f(t) , A(t) - matrix NxN , B(t) - matrix NxN , f(t) - N-size vector
    class DAESemiExplicitSystemAnalytic : DAESemiExplicitSystem
    {
        RPNExpression[] fEquations;
        RPNExpression[] gEquations;
        RPNExpression[,] dFdXEquations;
        RPNExpression[,] dFdZEquations;
        RPNExpression[,] dGdXEquations;
        RPNExpression[,] dGdZEquations;
        public override double[] F(Vector<double> x, Vector<double> z, double t)
        {
            throw new NotImplementedException();
        }

        public override double[] G(Vector<double> x, Vector<double> z, double t)
        {
            throw new NotImplementedException();
        }

        public override double[,] dFdX(Vector<double> x, Vector<double> z, double t)
        {
            throw new NotImplementedException();
        }

        public override double[,] dFdZ(Vector<double> x, Vector<double> z, double t)
        {
            throw new NotImplementedException();
        }

        public override double[,] dGdX(Vector<double> x, Vector<double> z, double t)
        {
            throw new NotImplementedException();
        }

        public override double[,] dGdZ(Vector<double> x, Vector<double> z, double t)
        {
            throw new NotImplementedException();
        }
    }
    class DAEImplicitSystemAnalytic : DAEImplicitSystem
    {
        RPNExpression[] equations;
        RPNExpression[,] dFddXequations;
        RPNExpression[,] dFdXequations;
        public DAEImplicitSystemAnalytic(DAEImplicitDefinition definition)
        {
            Size = equations.Length;
            equations = definition.F.ToArray();
            throw new NotImplementedException();
        }
        public override double[,] dFddX(Vector<double> x, Vector<double> dx, double t)
        {
            double[,] result = new double[Size, Size];
            List<double> lx = new List<double>();
            lx.Add(t);
            lx.AddRange(x);
            lx.AddRange(dx);
            double[] _x = lx.ToArray();
            for (int j = 0; j < Size; j++)
            {
                for (int i = 0; i < Size; i++)
                {
                    result[j, i] = dFddXequations[j,i].execute(_x);
                }
            }
            return result;
        }

        public override double[,] dFdX(Vector<double> x, Vector<double> dx, double t)
        {
            double[,] result = new double[Size,Size];
            List<double> lx = new List<double>();
            lx.Add(t);
            lx.AddRange(x);
            lx.AddRange(dx);
            double[] _x = lx.ToArray();
            for (int j = 0; j < Size; j++)
            {
                for (int i = 0; i < Size; i++)
                {
                    result[j, i] = dFdXequations[j, i].execute(_x);
                }
            }
            return result;
        }

        public override double[] F(Vector<double> x, Vector<double> dx, double t)
        {
            double[] result = new double[Size];
            List<double> lx = new List<double>();
            lx.Add(t);
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
