using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Equations.DAE.Explicit
{
    public abstract class DAEESolver
    {
        protected double step;
        public double Step { get; }
        public abstract XZVectorPair IntegrateStep(DAEESystem system, XZVectorPair xz, double t);
        static public Solution Solve(DAEEDescription equations, DAEESolver solver)
        {
            List<double[]> values = new List<double[]>();
            List<double> time = new List<double>();
            XZVectorPair xz = new XZVectorPair
            {
                X = Vector<double>.Build.Dense(equations.InitialValuesX),
                Z = Vector<double>.Build.Dense(equations.InitialValuesZ)
            };
            DAEEAnalytic system = new DAEEAnalytic(equations);
            values.Add(xz.ToArray());
            time.Add(equations.T0);
            for (double t = 0.0; t < equations.Time;)
            {
                double ti = t + equations.T0;
                t += solver.step;
                xz = solver.IntegrateStep(system, xz, ti);
                values.Add(xz.ToArray());
                double tnext = t + equations.T0;
                time.Add(tnext);
            }
            string[] variables = equations.VariableNames;
            return new Solution(values, time, variables);
        }
    }
}
