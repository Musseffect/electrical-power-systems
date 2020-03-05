using ElectricalPowerSystems.Interpreter.Equations.Expression;
using ElectricalPowerSystems.Interpreter.Equations.Nonlinear;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.MathUtils
{
    public abstract class NonlinearSystem
    { 
        public abstract Vector<double> F(Vector<double> x);
        public abstract Matrix<double> dF(Vector<double> x);
        public int Size { get; set; }
    };
    public abstract class NonlinearSystemNumericCentralDifference : NonlinearSystem
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
        private List<RPNExpression> equations;
        //private SparseMatrix<RPNExpression> equations;
        private RPNExpression [,]derivatives;
        public NonlinearSystemSymbolicAnalytic(NonlinearEquationDefinition system)
        {
            this.equations = system.Equations;
            this.derivatives = system.JacobiMatrix;
            this.Size = this.equations.Count;
        }
        public override Vector<double> F(Vector<double> x)
        {
            Vector<double> result=Vector<double>.Build.Dense(this.Size);
            int i = 0;
            double[] _x = x.ToArray();
            foreach (var equation in equations)
            {
                result[i] = equation.execute(_x);
                i++;
            }
            return result;
        }
        public override Matrix<double> dF(Vector<double> x)
        {
            Matrix<double> result = Matrix<double>.Build.Dense(Size, Size);
            double[] _x = x.ToArray();
            //for each non zero derivative equation in matrix derivatives solve at x
            for (int j = 0; j < equations.Count; j++)
            {
                for (int i = 0; i < equations.Count; i++)
                {
                    result[j,i] = derivatives[i,j].execute(_x);
                }
            }
            return result;
        }
    };
    public class NonlinearSystemSymbolicNumerical : NonlinearSystemNumericCentralDifference
    {
        private List<RPNExpression> equations;
        public NonlinearSystemSymbolicNumerical(NonlinearEquationDefinition system)
        {
            this.equations = system.Equations;
            this.Size = this.equations.Count;
        }
        public override Vector<double> F(Vector<double> x)
        {
            Vector<double> result = Vector<double>.Build.Dense(this.Size);
            int i = 0;
            double[] _x = x.ToArray();
            foreach (var equation in equations)
            {
                result[i] = equation.execute(_x);
                i++;
            }
            return result;
        }
    };
}
