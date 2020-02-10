using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.MathUtils
{
    public abstract class NonlinearSolver
    {
        public abstract void Solve(NonlinearSystem system, Vector<double> x0);
    }
}
