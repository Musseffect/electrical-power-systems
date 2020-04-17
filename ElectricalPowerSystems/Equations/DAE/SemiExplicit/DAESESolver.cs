using ElectricalPowerSystems.Equations.DAE;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Equations.DAE.SemiExplicit
{
    public abstract class DAESESolver
    {
        protected double step;
        public double Step { get; }
        public abstract XZVectorPair IntegrateStep(DAESESystem system, XZVectorPair xz, double t);
        static public Solution Solve(DAESEDescription equations, DAESESolver solver)
        {
            List<double[]> values = new List<double[]>();
            List<double> timeArray = new List<double>();
            XZVectorPair xz = new XZVectorPair
            {
                X = Vector<double>.Build.Dense(equations.InitialValuesX),
                Z = Vector<double>.Build.Dense(equations.InitialValuesX)
            };
            DAESEAnalytic system = new DAESEAnalytic(equations);
            values.Add(xz.ToArray());
            timeArray.Add(equations.T0);
            for (double t = 0.0; t < equations.Time;)
            {
                double ti = t + equations.T0;
                t += solver.step;
                xz = solver.IntegrateStep(system, xz, ti);
                values.Add(xz.ToArray());
                double tnext = t + equations.T0;
                timeArray.Add(tnext);
            }
            string[] variables = equations.VariableNames;
            return new Solution(values, timeArray, variables);
        }
    }

    class BDF1 : DAESESolver
    {
        public override XZVectorPair IntegrateStep(DAESESystem system, XZVectorPair xz, double t)
        {
            throw new NotImplementedException();
        }
    }
}
