using ElectricalPowerSystems.Equations.DAE;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Equations.DAE.DividedImplicit
{
    public abstract partial class DAEDISolver
    {
        protected double step;
        public double Step { get; }
        public abstract XZVectorPair IntegrateStep(DAEDISystem system, XZVectorPair xz, double t);
        static public Solution Solve(DAEDIDescription equations, DAEDISolver solver,double t0,double time)
        {
            List<double[]> values = new List<double[]>();
            List<double> timeArray = new List<double>();
            XZVectorPair xz = new XZVectorPair {
                X = Vector<double>.Build.Dense(equations.InitialValuesX),
                Z = Vector<double>.Build.Dense(equations.InitialValuesX)
            };
            DAEDIAnalytic system = new DAEDIAnalytic(equations);
            values.Add(xz.ToArray());
            timeArray.Add(t0);
            for (double t = 0.0; t < time;)
            {
                double ti = t + t0;
                xz = solver.IntegrateStep(system, xz, ti);
                t += solver.step;
                values.Add(xz.ToArray());
                double tnext = t + t0;
                timeArray.Add(tnext);
            }
            string[] variables = equations.VariableNames;
            return new Solution(values, timeArray, variables);
        }
    }
    public class BDF1 : DAEDISolver
    {
        public override XZVectorPair IntegrateStep(DAEDISystem system, XZVectorPair xz, double t)
        {
            throw new NotImplementedException();
        }
    }
    //x_new = x_old + h * (x'_old + x'_new)/2
    public class Trapezoidal : DAEDISolver
    {
        int iterations;
        double alpha;
        double fAbsTol;
        public Trapezoidal(int iterations, double fAbsTol, double alpha)
        {
            this.iterations = iterations;
            this.fAbsTol = fAbsTol;
            this.alpha = alpha;
        }
        public override XZVectorPair IntegrateStep(DAEDISystem system, XZVectorPair xz, double t)
        {
            throw new NotImplementedException();
        }
    }
    public class RungeKutta : DAEDISolver
    {
        public override XZVectorPair IntegrateStep(DAEDISystem system, XZVectorPair xz, double t)
        {
            throw new NotImplementedException();
        }
    }
}
