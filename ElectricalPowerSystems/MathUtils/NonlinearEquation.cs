using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ElectricalPowerSystems.EquationInterpreter.ASTEquationCompiler;

namespace ElectricalPowerSystems.MathUtils
{
    public abstract class NonlinearSystem
    { 
        public abstract Vector<double> F(Vector<double> x);
        public abstract Matrix<double> dF(Vector<double> x);
        public int Size { get; set; }
    };
    public abstract class NonlinearEquationNumericCentralDifference : NonlinearSystem
    {
        public double Epsilon { get; set; }
        public override Matrix<double> dF(Vector<double> x)
        {
            Matrix<double> result = Matrix<double>.Build.Dense(Size, Size); ;
            //central difference
            double epsilon = Epsilon;
            for (int i = 0; i <Size; i++)
            {
                Vector<double> t_x1 = x;
                t_x1[Size] -= epsilon;
                Vector<double> t_x2 = x;
                t_x2[Size] += epsilon;
                result.SetRow(i,(F(t_x2)-F(t_x1)).Divide(2.0 * Epsilon));
            }
            return result;
        }
    };
    public class NonlinearSystemSymbolicAnalytic : NonlinearSystem
    {
        private List<RPN> equations;
        private Matrix<RPN> derivatives;
        public override Vector<double> F(Vector<double> x)
        {
            Vector<double> result=Vector<double>.Build.Dense(this.Size);
            int i = 0;
            foreach (var equation in equations)
            {
                result[i] = equation.solve(x);
                i++;
            }
            return result;
        }
        public override Matrix<double> dF(Vector<double> x)
        {
            Matrix<double> result = Matrix<double>.Build.Dense(Size, Size);

            //for each non zero derivative equation in matrix derivatives solve at x
            return result;
        }
    };
    public class NonlinearSystemSymbolicNumerical : NonlinearEquationNumericCentralDifference
    {
        private List<RPNExpression> equations;
        public override Vector<double> F(Vector<double> x)
        {
            Vector<double> result = Vector<double>.Build.Dense(this.Size);
            int i = 0;
            foreach (var equation in equations)
            {
                result[i] = equation.execute(x.ToArray());
                i++;
            }
            return result;
        }
    };
}
