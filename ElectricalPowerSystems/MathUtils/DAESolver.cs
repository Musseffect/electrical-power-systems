using ElectricalPowerSystems.Equations.DAE;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.MathUtils
{
    /// <summary>
    /// mathematik.hu-berlin.de/~steffen/pub/introduction_to_daes_497.pdf
    /// https://reference.wolfram.com/language/tutorial/NDSolveDAE.html
    /// </summary>
    abstract class DAEImplicitSolver
    {
        protected double step;
        public double Step { get; }
        public abstract Vector<double> IntegrateStep(DAEImplicitSystem system, Vector<double> x, double t);
        static public DAESolution Solve(Equations.DAE.DAEImplicitDefinition equations, DAEImplicitSolver solver)
        {
            List<double[]> values = new List<double[]>();
            List<double> time = new List<double>();
            Vector<double> x = Vector<double>.Build.Dense(equations.InitialValues);
            DAEImplicitSystemAnalytic system = new DAEImplicitSystemAnalytic(equations);
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
            return new DAESolution(values, time, variables);
        }
    }
    //TODO add BDF2 and Trapezoid
    class ImplicitEulerDAESolver: DAEImplicitSolver
    {
        int newtonIterations;
        double fAbsTol;
        public ImplicitEulerDAESolver()
        {
            step = 0.1;
            newtonIterations = 20;
            fAbsTol = 0.001;
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
        public override Vector<double> IntegrateStep(DAEImplicitSystem system, Vector<double> x, double t)
        {
            //solve f(xn+1,(xn+1-xn)/h,t+h)
            Vector<double> xNew = x;
            for (int i = 0; i < newtonIterations; i++)
            {
                Vector<double> dx = (xNew-x)/step;
                Vector<double> f = Vector<double>.Build.Dense(system.Size);
                double time = t + step;
                Matrix<double> dFdX = system.DFdX(xNew, dx, time); //use k as dx
                Matrix<double> dFddX = system.DFddX(xNew, dx, time);
                double[] F = system.F(xNew, dx, time);
                Matrix<double> jacobiMatrix;// = Matrix<double>.Build.Sparse(system.Size, system.Size);
                jacobiMatrix = dFdX.Add(dFddX.Divide(step));
                /* for (int m = 0; m < system.Size; m++)
                 {
                     for (int l = 0; l < system.Size; l++)
                     {
                         jacobiMatrix[m, l] = dFdX[m, l]  + dFddX[m, l]/step;
                     }
                 }*/
                Vector<double> deltax = jacobiMatrix.Solve(-f);
                xNew += deltax;
                if (f.L2Norm() < fAbsTol)
                {
                    return xNew;
                }
            }
            throw new Exception("Решение не сошлось");
        }
    }
    abstract class ImplicitRungeKuttaDAESolver : DAEImplicitSolver
    {
        protected double[,] a;
        protected double[] b;
        protected double[] c;
        public int Stages { get; protected set; }
        int newtonIterations;
        double fAbsTol;
        /*
         kxi=f(xn+sum aij*kxj,zn+sum aij*kzj,tn+ci*h)
         0=g(xn+sum aij*kxj,zn+sum aij*kzj,tn+ci*h)
         */
        public ImplicitRungeKuttaDAESolver()
        {
            step = 0.1;
            newtonIterations = 20;
            fAbsTol = 0.001;
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
        public override Vector<double> IntegrateStep(DAEImplicitSystem system,Vector<double> x,double t)
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
                    double[] F = system.F(t_x, k[j], time);
                        //subMatrix Size*Size
                    for (int m = 0; m < system.Size; m++)
                    {
                        int row = m + j * system.Size;
                        f[row] = F[m];
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
    class RADAUIIA3: ImplicitRungeKuttaDAESolver
    {
        public RADAUIIA3()
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
    }
    class RADAUIIA5 : ImplicitRungeKuttaDAESolver
    {
        public RADAUIIA5()
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
    }

    

#if SEMIEXPLICIT
    abstract class DAESemiExplicitSolver
    {
        protected double step;
        public double Step { get; }
        public abstract Vector<double> IntegrateStep(DAESemiExplicitSystem system, Vector<double> x, Vector<double> z, double t);
        static public DAESolution Solve(Interpreter.Equations.DAE.DAESemiExplicitDefinition equations, DAESemiExplicitSolver solver)
        {
            throw new NotImplementedException();
            /*List<double[]> values = new List<double[]>();
            List<double> time = new List<double>();
            Vector<double> x = Vector<double>.Build.Dense(equations.InitialValuesX);
            Vector<double> z = Vector<double>.Build.Dense(equations.InitialValuesZ);
            DAESemiExplicitSystemAnalytic system = new DAESemiExplicitSystemAnalytic(equations);
            values.Add(equations.Initi);
            time.Add(equations.T0);
            for (double t = 0.0; t < equations.Time;)
            {
                double ti = t + equations.T0;
                t += solver.step;
                x = solver.IntegrateStep(system, x,z, ti);
                values.Add(x.ToArray());
                double tnext = t + equations.T0;
                time.Add(tnext);
            }
            string[] variables = equations.VariableNames;
            return new DAESolution(values, time, variables);*/
        }
    }
    
     class SemiExplicitEulerDAESolver: DAESemiExplicitSolver
    {
        int newtonIterations;
        public override Vector<double> IntegrateStep(DAESemiExplicitSystem system, Vector<double> x, Vector<double> z, double t)
        {
            Vector<double> xNew = x;
            Vector<double> zNew = z;
            for (int i = 0; i < newtonIterations; i++)
            {
                Matrix<double> jacobiMatrix = Matrix<double>.Build.Dense(system.Size, system.Size);
                Vector<double> f = Vector<double>.Build.Dense(system.Size);
                double time = t + step;
                double[] F = system.F(xNew, zNew, time);
                double[] G = system.G(xNew, zNew, time);
                double[,] dFdX = system.dFdX(xNew, zNew, time);
                double[,] dFdZ = system.dFdZ(xNew, zNew, time);
                double[,] dGdX = system.dGdX(xNew, zNew, time);
                double[,] dGdZ = system.dGdZ(xNew, zNew, time);
                for (int j = 0; j < system.SizeX; j++)
                {
                    f[j] = xNew[j] - x[j] - step * F[j];
                    for (int k = 0; k < system.SizeX; k++)
                    {
                        jacobiMatrix[j, k] = 1.0 - step * dFdX[j, k];
                    }
                    for (int k = 0; k < system.SizeZ; k++)
                    {
                        jacobiMatrix[j, k + system.SizeX] = -step * dFdZ[j, k];
                    }
                }
                for (int j = 0; j < system.SizeZ; j++)
                {
                    int row = j + system.SizeX;
                    f[row] = G[j];
                    for (int k = 0; k < system.SizeX; k++)
                    {
                        jacobiMatrix[row, k] = dGdX[j, k];
                    }
                    for (int k = 0; k < system.SizeZ; k++)
                    {
                        jacobiMatrix[row, k + system.SizeX] = dGdZ[j, k];
                    }

                }
                Vector<double> delta = jacobiMatrix.Solve(-f);
                xNew += delta.SubVector(0, system.SizeX);
                zNew += delta.SubVector(0, system.SizeZ);
            }
            //return x and z
            throw new NotImplementedException();
        }
    }
   

#endif

}
