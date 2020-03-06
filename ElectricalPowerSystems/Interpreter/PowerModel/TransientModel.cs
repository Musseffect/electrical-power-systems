using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Interpreter.PowerModel
{
#if MODELINTERPRETER
    public partial class ModelInterpreter
    {
        public class TransientSolver
        {
            public TransientSolution Solve(string equations)
            {
                throw new NotImplementedException();
            }
        }
        public class TransientSolution
        {

        }
        public class TransientModel : IModel
        {
            List<ITransientElement> elements;
            TransientSolver solver;
            private string GenerateEquations()
            {
                throw new NotImplementedException();
            }
            public string Solve()
            {
                string equations = GenerateEquations();
                TransientSolution solution = solver.Solve(equations);
                return solution.ToString();
            }
        }
    }
#endif
}
