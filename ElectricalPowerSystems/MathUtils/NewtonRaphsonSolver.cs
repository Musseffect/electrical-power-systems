using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.MathUtils
{

    //Broyden method
    //https://numerics.mathdotnet.com/api/MathNet.Numerics.RootFinding/RobustNewtonRaphson.htm


    class NewtonRaphsonSolver
    {
        //<summary><summary>
        public static Vector<double> Solve(NonlinearSystem system, Vector<double> x0, int maxIterations, double accuracy,double alpha)
        {
            Vector<double> x = x0;
            Vector<double> F = system.F(x);
            for (int i = 0; i < maxIterations; i++)
            {
                Matrix<double> J = system.dF(x);

                Vector<double> dx = J.Solve(F.Multiply(-alpha));
                Vector<double> x_new = x + dx;
                Vector<double> F_new = system.F(x_new);
                x = x_new;
                if (F_new.L2Norm() < accuracy)
                    return x;
                F = F_new;
            }
            throw new Exception("Решение не сошлось");
        }
    }
}
