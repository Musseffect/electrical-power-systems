//#define ALTERNATIVE
using ElectricalPowerSystems.Equations.DAE;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Equations.DAE.Implicit
{
#if !ALTERNATIVE
    /// <summary>
    /// mathematik.hu-berlin.de/~steffen/pub/introduction_to_daes_497.pdf
    /// https://reference.wolfram.com/language/tutorial/NDSolveDAE.html
    /// </summary>
    public abstract class DAEISolver
    {
        protected double step;
        public double Step { get { return step; } }
        public virtual void SetStep(double step)
        {
            this.step = step;
        }
        public abstract Vector<double> IntegrateStep(DAEISystem system, Vector<double> x, double t);
        static public Solution Solve(DAEIDescription equations, DAEISolver solver)
        {
            List<double[]> values = new List<double[]>();
            List<double> time = new List<double>();
            Vector<double> x = Vector<double>.Build.Dense(equations.InitialValues);
            DAEIAnalytic system = new DAEIAnalytic(equations);
            values.Add(equations.InitialValues);
            time.Add(equations.T0);
            for (double t = 0.0; t < equations.Time;)
            {
                double ti = t + equations.T0;
                t += solver.step;
                x = solver.IntegrateStep(system, x, ti);
                values.Add(x.ToArray());
                double tnext = t + equations.T0;
                time.Add(tnext);
            }
            string[] variables = equations.VariableNames;
            return new Solution(values, time, variables);
        }
    }
    //TODO Trapezoid (with ImplicitDividedSystem)
    public class Trapezoidal : DAEISolver
    {
        int newtonIterations;
        double fAbsTol;
        double alpha;
        public Trapezoidal(double fAbsTol, int iterations, double alpha, double step)
        {
            this.fAbsTol = fAbsTol;
            this.newtonIterations = iterations;
            this.alpha = alpha;
            this.step = step;
        }
        public Trapezoidal()
        {
            step = 0.1;
            newtonIterations = 20;
            fAbsTol = 0.001;
            alpha = 1;
        }
        public void SetStep(float step)
        {
            this.step = step;
        }
        public void SetNewtonIterations(int iterations)
        {
            this.newtonIterations = iterations;
        }
        public void SetNewtonFAbsTol(float value)
        {
            this.fAbsTol = value;
        }
        public override Vector<double> IntegrateStep(DAEISystem system, Vector<double> x, double t)
        {
            /*
             solve
             f(x_n+1,(x_n+1-x_n)*2h-x'_n,t_n);
             f(x_n,x'_n,t_n);
             only when df/dx' exists
             */
            throw new NotImplementedException();
        }
    }
    public abstract class BDFN:DAEISolver
    {
        protected DAEISolver solver;
        protected double[] a;
        public int Steps { get; protected set; }
        int counter;
        int newtonIterations;
        double fAbsTol;
        double alpha;
        List<Vector<double>> previousValues;
        public BDFN(double fAbsTol, int iterations, double alpha, double step)
        {
            this.fAbsTol = fAbsTol;
            this.newtonIterations = iterations;
            this.alpha = alpha;
            this.step = step;
            this.counter = 1;
            previousValues = new List<Vector<double>>();
        }
        public BDFN()
        {
            step = 0.1;
            newtonIterations = 20;
            fAbsTol = 0.001;
            alpha = 1;
            this.counter = 1;
            previousValues = new List<Vector<double>>();
        }
        public virtual void SetNewtonIterations(int iterations)
        {
            this.newtonIterations = iterations;
        }
        public virtual void SetNewtonFAbsTol(float value)
        {
            this.fAbsTol = value;
        }
        public override void SetStep(double value)
        {
            counter = 1;
            base.SetStep(value);
            solver.SetStep(step);
            previousValues.Clear();
        }
        public override Vector<double> IntegrateStep(DAEISystem system, Vector<double> x, double t)
        {
            if (counter != Steps)
            {
                //call some method with comparable accuracy
                previousValues.Insert(0, x);
                Vector<double> result = solver.IntegrateStep(system, x, t);
                counter++;
                return result;
            }
            //solve f(xn+1,a[0]*xn+1+SUM(a[i]*xn+1-i)/h,t+h)
            Vector<double> xNew = x;
            Vector<double> _dx = x * a[1];
            for (int j = 0; j < Steps - 1; j++)
            {
                _dx += previousValues[j] * a[j + 2];
            }
            for (int i = 0; i < newtonIterations; i++)
            {
                Vector<double> dx = (_dx + xNew * a[0]) / step;
                double time = t + step;
                Matrix<double> dFdX = system.DFdX(xNew, dx, time); //use k as dx
                Matrix<double> dFddX = system.DFddX(xNew, dx, time);
                Vector<double> F = system.F(xNew, dx, time);
                F = F * alpha;
                //Vector<double> f = Vector<double>.Build.SparseOfArray(F).Multiply(alpha);
                Matrix<double> jacobiMatrix;// = Matrix<double>.Build.Sparse(system.Size, system.Size);
                jacobiMatrix = dFdX.Add(dFddX.Multiply(a[0]/step));
                /* for (int m = 0; m < system.Size; m++)
                 {
                     for (int l = 0; l < system.Size; l++)
                     {
                         jacobiMatrix[m, l] = dFdX[m, l]  + dFddX[m, l]/step;
                     }
                 }*/
                Vector<double> deltax = jacobiMatrix.Solve(-F);
                xNew += deltax;
                if (F.L2Norm() < fAbsTol)
                {
                    previousValues.Insert(0, x);
                    previousValues.RemoveAt(previousValues.Count-1);
                    return xNew;
                }
            }
            throw new Exception("Решение не сошлось");
        }

    }
    public class BDF2 : BDFN
    {
        public BDF2(double fAbsTol, int iterations, double alpha, double step):base(fAbsTol,iterations,alpha,step)
        {
            a = new double[] { 1.5,-2, 0.5};
            Steps = 2;
            solver = new RADAUIIA3(fAbsTol, iterations, alpha, step);
        }
        public BDF2():base(0.001, 20, 1, 0.1)
        {
            Steps = 2;
            solver = new RADAUIIA3(0.001, 20, 1, step);
        }
        public override void SetNewtonIterations(int iterations)
        {
            base.SetNewtonIterations(iterations);
            (solver as RADAUIIA3).SetNewtonIterations(iterations);
        }
        public override void SetNewtonFAbsTol(float value)
        {
            base.SetNewtonFAbsTol(value);
            (solver as RADAUIIA3).SetNewtonFAbsTol(value);
        }
    }
    public class BDF1: DAEISolver
    {
        int newtonIterations;
        double fAbsTol;
        double alpha;
        public BDF1(double fAbsTol, int iterations, double alpha, double step)
        {
            this.fAbsTol = fAbsTol;
            this.newtonIterations = iterations;
            this.alpha = alpha;
            this.step = step;
        }
        public BDF1()
        {
            step = 0.1;
            newtonIterations = 20;
            fAbsTol = 0.001;
            alpha = 1;
        }
        public void SetStep(float step)
        {
            this.step = step;
        }
        public void SetNewtonIterations(int iterations)
        {
            this.newtonIterations = iterations;
        }
        public void SetNewtonFAbsTol(float value)
        {
            this.fAbsTol = value;
        }
        public override Vector<double> IntegrateStep(DAEISystem system, Vector<double> x, double t)
        {
            //solve f(xn+1,(xn+1-xn)/h,t+h)
            Vector<double> xNew = x;
            for (int i = 0; i < newtonIterations; i++)
            {
                Vector<double> dx = (xNew-x)/step;
                double time = t + step;
                Matrix<double> dFdX = system.DFdX(xNew, dx, time); //use k as dx
                Matrix<double> dFddX = system.DFddX(xNew, dx, time);
                Vector<double> F = system.F(xNew, dx, time);
                F = F * alpha * step;
                //Vector<double> f = Vector<double>.Build.SparseOfArray(F).Multiply(alpha);
                Matrix<double> jacobiMatrix;// = Matrix<double>.Build.Sparse(system.Size, system.Size);
                jacobiMatrix = dFdX.Multiply(step).Add(dFddX);
                /* for (int m = 0; m < system.Size; m++)
                 {
                     for (int l = 0; l < system.Size; l++)
                     {
                         jacobiMatrix[m, l] = dFdX[m, l]  + dFddX[m, l]/step;
                     }
                 }*/
                Vector<double> deltax = jacobiMatrix.Solve(-F);
                xNew += deltax;
                if (F.L2Norm()/step < fAbsTol)
                {
                    return xNew;
                }
            }
            throw new Exception("Решение не сошлось");
        }
    }
    ///<summary>
    ///kx_i=f(x_n+sum[a_ij*kx_j],zn+sum[a_ij*kz_j],t_n+c_i*h);<para></para> 
    ///0=g(x_n+sum[a_ij*k_xj],z_n+sum[a_ij*kz_j],tn+c_i*h);
    ///</summary>
    abstract class RungeKutta : DAEISolver
    {
        protected double[,] a;
        protected double[] b;
        protected double[] c;
        public int Stages { get; protected set; }
        int newtonIterations;
        double fAbsTol;
        double alpha;
        public RungeKutta(double fAbsTol, int iterations, double alpha, double step) {
            this.fAbsTol = fAbsTol;
            this.newtonIterations = iterations;
            this.alpha = alpha;
            this.step = step;
        }
        public RungeKutta()
        {
            step = 0.1;
            newtonIterations = 20;
            fAbsTol = 0.001;
            alpha = 1;
        }
        public void SetStep(float step)
        {
            this.step = step;
        }
        public void SetNewtonIterations(int iterations)
        {
            this.newtonIterations = iterations;
        }
        public void SetNewtonFAbsTol(float value)
        {
            this.fAbsTol = value;
        }
        /*public Vector<double> IntegrateStep(DAESemiImplicitSystem system, Vector<double> x, Vector<double> z, double t)
        {
            int sizeX = system.SizeX * Stages;
            int sizeZ = system.SizeZ * Stages;
            int size = sizeX + sizeZ;
            Vector<double>[] kx = new Vector<double>[Stages];// vector k=[k1_1,k1_2,...k1_n,...,kn_1,...,kn_n]
            Vector<double>[] kz = new Vector<double>[Stages];// vector k=[k1_1,k1_2,...k1_n,...,kn_1,...,kn_n]

            for (int i = 0; i < Stages; i++)
            {
                kx[i] = Vector<double>.Build.Dense(system.SizeX);
                kz[i] = Vector<double>.Build.Dense(system.SizeZ);
            }

            for (int i = 0; i < newtonIterations; i++)
            {
                Matrix<double> jacobiMatrix = Matrix<double>.Build.Dense(size, size);
                Vector<double> f = Vector<double>.Build.Dense(size);
                for (int j = 0; j < Stages; j++)
                {
                    Vector<double> t_x = x;
                    Vector<double> t_z = z;
                    for (int m = 0; m < Stages; m++)
                    {
                        t_x += step * a[j, m] * kx[m];
                        t_z += step * a[j, m] * kz[m];
                    }
                    double time = t + step * c[j];
                    Matrix<double> dFdX = system.dFdX(t_x, t_z, time); //use k as dx
                    Matrix<double> dFdZ = system.dFdZ(t_x, t_z, time); //use k as dx
                    Matrix<double> dGdX = system.dGdX(t_x, t_z, time);
                    Matrix<double> dGdZ = system.dGdZ(t_x, t_z, time);
                    double[] F = system.F(t_x, t_z, time);
                    double[] G = system.G(t_x, t_z, time);
                    for (int m = 0; m < system.SizeX; m++)
                    {
                        int row = j * system.SizeX + m;
                        f[row] = F[m];
                        for (int p = 0; p < Stages; p++)
                        {
                            for (int l = 0; l < system.SizeX; l++)
                            {
                                int column = l + p * system.Size;
                                //jacobiMatrix[row, column] = dFdX[m, l] * Step * a[j, p] + dFddX[m, l];
                            }
                            for (int l = 0; l < system.SizeZ; l++)
                            {
                                int column = l + p * system.Size;
                                //jacobiMatrix[row, column] = dFdX[m, l] * Step * a[j, p] + dFddX[m, l];
                            }
                        }
                    }
                    for (int m = 0; m < system.SizeZ; m++)
                    {
                        int row=j*system.SizeZ + sizeX + m;
                        f[row] = G[m];

                        for (int p = 0; p < Stages; p++)
                        {
                            for (int l = 0; l < system.SizeX; l++)
                            {
                                int column = l + p * system.Size;
                                //jacobiMatrix[row, column] = dFdX[m, l] * Step * a[j, p] + dFddX[m, l];
                            }
                            for (int l = 0; l < system.SizeZ; l++)
                            {
                                int column = l + p * system.Size;
                                //jacobiMatrix[row, column] = dFdX[m, l] * Step * a[j, p] + dFddX[m, l];
                            }
                        }
                    }
                }
                Vector<double> dk = jacobiMatrix.Solve(-f);
                for (int j = 0; j < Stages; j++)
                {
                    kx[j] += dk.SubVector(j * system.Size, system.Size);
                    kz[j] += dk.SubVector(j * system.Size, system.Size);
                }
            }

            for (int i = 0; i < Stages; i++)
            {
                x += kx[i] * step * b[i];
                x += kz[i] * step * b[i];
            }
            return x;


            throw new NotImplementedException();
        }*/
        // system is f(x,x',t)=0
        // x(n+1)=x(n)+sum k_i*b_i
        // k_i = x'(x_n + h*sum k_j * a_ij, t_n + h*c_i) where i is [1,Stages] and j is [1,Stages]
        // k_i is a vector and sizeOf(k_i) == sizeOf(x)
        // or f(x_n + h*sum k_j * a_ij, k_i, t_n + h * c_i)
        // solve this implicit nonlinear system for all k's 
        public override Vector<double> IntegrateStep(DAEISystem system,Vector<double> x,double t)
        {
            int size = system.Size * Stages;
            Vector<double> [] k = new Vector<double> [Stages];// vector k=[k1_1,k1_2,...k1_n,...,kn_1,...,kn_n]

            for (int i = 0; i < Stages; i++)
            {
                k[i] = Vector<double>.Build.Dense(system.Size);
            }
            for (int i = 0; i < newtonIterations; i++)
            {
                Matrix<double> jacobiMatrix = Matrix<double>.Build.Dense(size, size);
                Vector<double> f = Vector<double>.Build.Dense(size);
                for (int j = 0; j < Stages; j++)
                {
                    Vector<double> t_x = x;
                    //[row,column]
                    for (int m = 0; m < Stages; m++)
                    {
                        t_x += step * a[j, m] * k[m];
                    }
                    double time = t + step * c[j];
                    Matrix<double> dFdX = system.DFdX(t_x, k[j], time); //use k as dx
                    Matrix<double> dFddX = system.DFddX(t_x, k[j], time);
                    Vector<double> F = system.F(t_x, k[j], time);
                    //subMatrix Size*Size
                    for (int m = 0; m < system.Size; m++)
                    {
                        int row = m + j * system.Size;
                        f[row] = F[m]*alpha;
                        for (int p = 0; p < Stages; p++)
                        {
                            for (int l = 0; l < system.Size; l++)
                            {
                                int column = l + p * system.Size;
                                double dfddx = p == j ? dFddX[m, l]:0.0;
                                jacobiMatrix[row, column] = dFdX[m, l] * step * a[j, p] + dfddx;
                            }
                        }
                    }
                }
                Vector<double> dk = jacobiMatrix.Solve(-f);
                for(int j=0;j<Stages;j++)
                    k[j] += dk.SubVector(j*system.Size,system.Size);
                if (f.L2Norm() < fAbsTol)
                {
                    for (int j = 0; j < Stages; j++)
                    {
                        x += k[j] * step * b[j];
                    }
                    return x;
                }
            }
            throw new Exception("Решение не сошлось");
        }
    }
    class RADAUIIA3: RungeKutta
    {
        private void Init()
        {
            Stages = 2;
            a = new double[2, 2] {
                {5.0 / 12.0, -1.0 / 12.0},
                {3.0 / 4.0, 1.0 / 4.0}
            };
            b = new double[2] {
                3.0 / 4.0,
                1.0 / 4.0
            };
            c = new double[2] {
                1.0 / 3.0,
                1.0
            };
        }
        public RADAUIIA3(double fAbsTol, int iterations, double alpha, double step) : base(fAbsTol, iterations, alpha, step)
        {
            Init();
        }
        public RADAUIIA3() : base()
        {
            Init();
        }
    }
    class RADAUIIA5 : RungeKutta
    {
        private void Init()
        {
            Stages = 3;
            a = new double[3, 3] {
                {
                    11.0 / 45.0 - 7.0 * Math.Sqrt(6.0) / 360.0,
                    37.0 / 225.0 - 169.0 * Math.Sqrt(6.0) / 1800.0,
                    -2.0 / 225.0 + Math.Sqrt(6.0) / 75.0
                },
                {
                    37.0 / 225.0 + 169.0 * Math.Sqrt(6.0) / 1800.0,
                    11.0 / 45.0 + 7.0 * Math.Sqrt(6.0) / 360.0,
                    -2.0 / 225.0 - Math.Sqrt(6.0) / 75.0
                },
                {
                    4.0 / 9.0 - Math.Sqrt(6.0) / 36.0,
                    4.0 / 9.0 + Math.Sqrt(6.0) / 36.0,
                    1.0 / 9.0
                }
            };
            b = new double[3] {
                4.0 / 9.0 - Math.Sqrt(6.0) / 36.0,
                4.0 / 9.0 + Math.Sqrt(6.0) / 36.0,
                1.0 / 9.0
            };
            c = new double[3] {
                2.0 / 5.0 - Math.Sqrt(6.0) / 10.0,
                2.0 / 5.0 + Math.Sqrt(6.0) / 10.0,
                1.0
            };
        }
        public RADAUIIA5(double fAbsTol, int iterations, double alpha, double step) : base(fAbsTol, iterations, alpha, step)
        {
            Init();
        }
        public RADAUIIA5() : base()
        {
            Init();
        }
    }
#else
    public abstract class DAEISolver
    {
        protected double step;
        public double Step { get; }
        public abstract XZVectorPair IntegrateStep(DAEISystem system, XZVectorPair x, double t);
        static public DAESolution Solve(DAEIDescription equations, DAEISolver solver)
        {
            List<double[]> values = new List<double[]>();
            List<double> time = new List<double>();
            XZVectorPair xz = new XZVectorPair
            {
                X = Vector<double>.Build.Dense(equations.InitialValuesX),
                Z = Vector<double>.Build.Dense(equations.InitialValuesZ)
            };
            DAEIAnalytic system = new DAEIAnalytic(equations);
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
            return new DAESolution(values, time, variables);
        }
    }
    public class BDF1 : DAEISolver
    {
        int newtonIterations;
        double fAbsTol;
        double alpha;
        public BDF1(double fAbsTol, int iterations, double alpha, double step)
        {
            this.fAbsTol = fAbsTol;
            this.newtonIterations = iterations;
            this.alpha = alpha;
            this.step = step;
        }
        public BDF1()
        {
            step = 0.1;
            newtonIterations = 20;
            fAbsTol = 0.001;
            alpha = 1;
        }
        public void SetStep(float step)
        {
            this.step = step;
        }
        public void SetNewtonIterations(int iterations)
        {
            this.newtonIterations = iterations;
        }
        public void SetNewtonFAbsTol(float value)
        {
            this.fAbsTol = value;
        }
        public override XZVectorPair IntegrateStep(DAEISystem system, XZVectorPair x, double t)
        {
            throw new NotImplementedException();
        }
    }
    public class RungeKutta : DAEISolver
    {
        protected double[,] a;
        protected double[] b;
        protected double[] c;
        public int Stages { get; protected set; }
        int newtonIterations;
        double fAbsTol;
        double alpha;
        public RungeKutta(double fAbsTol, int iterations, double alpha, double step)
        {
            this.fAbsTol = fAbsTol;
            this.newtonIterations = iterations;
            this.alpha = alpha;
            this.step = step;
        }
        public RungeKutta()
        {
            step = 0.1;
            newtonIterations = 20;
            fAbsTol = 0.001;
            alpha = 1;
        }
        public void SetStep(float step)
        {
            this.step = step;
        }
        public void SetNewtonIterations(int iterations)
        {
            this.newtonIterations = iterations;
        }
        public void SetNewtonFAbsTol(float value)
        {
            this.fAbsTol = value;
        }
        public override XZVectorPair IntegrateStep(DAEISystem system, XZVectorPair x, double t)
        {
            throw new NotImplementedException();
        }
    }
    public class Trapezoidal : DAEISolver
    {
        public override XZVectorPair IntegrateStep(DAEISystem system, XZVectorPair x, double t)
        {
            throw new NotImplementedException();
        }
    }
    class RADAUIIA3 : RungeKutta
    {
        private void Init()
        {
            Stages = 2;
            a = new double[2, 2] {
                {5.0 / 12.0, -1.0 / 12.0},
                {3.0 / 4.0, 1.0 / 4.0}
            };
            b = new double[2] {
                3.0 / 4.0,
                1.0 / 4.0
            };
            c = new double[2] {
                1.0 / 3.0,
                1.0
            };
        }
        public RADAUIIA3(double fAbsTol, int iterations, double alpha, double step) : base(fAbsTol, iterations, alpha, step)
        {
            Init();
        }
        public RADAUIIA3() : base()
        {
            Init();
        }
    }
    class RADAUIIA5 : RungeKutta
    {
        private void Init()
        {
            Stages = 3;
            a = new double[3, 3] {
                {
                    11.0 / 45.0 - 7.0 * Math.Sqrt(6.0) / 360.0,
                    37.0 / 225.0 - 169.0 * Math.Sqrt(6.0) / 1800.0,
                    -2.0 / 225.0 + Math.Sqrt(6.0) / 75.0
                },
                {
                    37.0 / 225.0 + 169.0 * Math.Sqrt(6.0) / 1800.0,
                    11.0 / 45.0 + 7.0 * Math.Sqrt(6.0) / 360.0,
                    -2.0 / 225.0 - Math.Sqrt(6.0) / 75.0
                },
                {
                    4.0 / 9.0 - Math.Sqrt(6.0) / 36.0,
                    4.0 / 9.0 + Math.Sqrt(6.0),
                    1.0 / 9.0
                }
            };
            b = new double[3] {
                4.0 / 9.0 - Math.Sqrt(6.0) / 36.0,
                4.0 / 9.0 + Math.Sqrt(6.0),
                1.0 / 9.0
            };
            c = new double[3] {
                2.0 / 5.0 - Math.Sqrt(6.0) / 10.0,
                2.0 / 5.0 + Math.Sqrt(6.0) / 10.0,
                1.0
            };
        }
        public RADAUIIA5(double fAbsTol, int iterations, double alpha, double step) : base(fAbsTol, iterations, alpha, step)
        {
            Init();
        }
        public RADAUIIA5() : base()
        {
            Init();
        }
    }
#endif


}
