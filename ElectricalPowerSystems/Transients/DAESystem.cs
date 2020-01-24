using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Transients
{
    //Implicit
    //f(x',x,t)=0
    abstract class DAEImplicitSystem
    {
        public int Size { get; }
        public abstract double[] F(Vector<double> x, Vector<double> dx, double t);
        public abstract double[,] dFdX(Vector<double> x, Vector<double> dx, double t);
        public abstract double[,] dFddX(Vector<double> x, Vector<double> dx, double t);
    }
    //SemiExplicit
    //x'=f(x,z,t)
    //0=g(x,z,t)
    abstract class DAESemiExplicitSystem
    {
        public int SizeX { get; }
        public int SizeZ { get; }
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
}
