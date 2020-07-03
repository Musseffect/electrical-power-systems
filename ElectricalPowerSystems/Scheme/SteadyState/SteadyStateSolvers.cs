using ElectricalPowerSystems.MathUtils;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Scheme.SteadyState
{
    public interface ISteadyStateSolver
    {
        Vector<double> Solve(NonlinearSystemSymbolicAnalytic system, Vector<double> x0);
    }
    public class SteadyStateNewtonSolver: ISteadyStateSolver
    {
        double fAbsTol;
        int iterations;
        double alpha;
        public SteadyStateNewtonSolver(double fAbsTol,int iterations,double alpha)
        {
            this.fAbsTol = fAbsTol;
            this.iterations = iterations;
            this.alpha = alpha;
        }
        //<summary><summary>
        Vector<double> ISteadyStateSolver.Solve(NonlinearSystemSymbolicAnalytic system, Vector<double> x0)
        {
            Vector<double> x = x0;
            Vector<double> F = system.F(x);
            for (int i = 0; i < iterations; i++)
            {
                Matrix<double> J = system.DF(x);

                Vector<double> dx = J.Solve(F.Multiply(-alpha));
                Vector<double> x_new = x + dx;
                Vector<double> F_new = system.F(x_new);
                x = x_new;
                if (F_new.L2Norm() < fAbsTol)
                    return x;
                F = F_new;
            }
            throw new Exception("Решение не сошлось");
        }
    }
}
