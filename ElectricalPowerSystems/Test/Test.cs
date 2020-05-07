using ElectricalPowerSystems.PowerModel.OldModel.ACGraph;
using ElectricalPowerSystems;
using ElectricalPowerSystems.Equations;
using ElectricalPowerSystems.Equations.DAE;
using ElectricalPowerSystems.Equations.Nonlinear;
using ElectricalPowerSystems.MathUtils;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricalPowerSystems.PowerModel;

namespace ElectricalPowerSystems.Test
{
    class Test
    {
        static public void RunTests()
        {
            Stream StdoutStream = Console.OpenStandardOutput();
            StreamWriter Stdout = new StreamWriter(StdoutStream);
            try
            {
                //TestNewModelLanguage();
                PowerModel.NewModel.Recloser.VirtualMachine.Test();
                //TestNonlinearEquationParser();
                //TestNonlinearEquationSolver();
                //TestCircuitEquationGeneration();
                //TestCircuitModel2();
                //TestPowerModelSimple();
                //TestPowerModel();

            } catch (Exception exc)
            {
                Stdout.WriteLine("Exception in RunTests: " + exc.Message);
                Stdout.WriteLine(exc.StackTrace);
            }
            Stdout.WriteLine("");
            Stdout.Flush();
            Stdout.Close();
        }
        static public void TestNewModelLanguage()
        {
            Stream StdoutStream = Console.OpenStandardOutput();
            StreamWriter Stdout = new StreamWriter(StdoutStream);
            Stdout.WriteLine("\tTest circuit model 2");
            string modelSteadyState =
@"
model:
steadystate{
    solver = newton{
        iterations = 20,
        fAbsTol = 0.01,
        alpha = 1.0
    }
};
elements:
    v1 = voltageSource{
       Peak = 20412,
       Phase = 0,
       Frequency = 60
    };
    g = ground{
    };
    scope1 = scope1p{
        Label=""scope1""
    };
    scope2 = scope1p{
        Label=""scope2""
    };
    r1 = resistor{
        R = 600
    };
    l1 = inductor{
        L = 20
    };
    r2 = resistor{
        R = 300
    };
    l2 = inductor{
        L = 6
    };
connections:
    connect(v1.in, g.in);
    connect(v1.out, scope1.in);
    connect(scope1.out, r1.in);
    connect(r1.out, l1.in);
    connect(l1.out, scope2.in);
    connect(scope2.out, r2.in);
    connect(r2.out, l2.in);
    connect(l2.out, g.in);
";
            string modelPowerSystemSimple =
                @"
model:
	steadystate{
		solver = newton{
			iterations = 20,
			fAbsTol = 0.01,
			alpha = 1.0
		}
	};
elements:
	generator1 = generatorY{
		Peak = 100.0,
		Phase = 0.0,
		Z = 0.01+j 0.001,
        Frequency = 60 //in Herz
	};
	scope1 = scope3p
	{
		Label = ""Generator""
    };
    scope2 = scope3p
	{
		Label = ""Load""
    };
    resistorGen1 = resistor{
		R = 1000
	};
    line1 = linePiSection{
		  R = 0.02,
          L = 0.01,
          B = 0.01,
          G = 1000,
          Bp = 0.01
	};
	load1 = loadY{
		ZA = 1,
		ZB = 1,
		ZC = 1
	};
	ground = ground{
	};
connections:
	connect(generator1.n, resistorGen1.in);
    connect(resistorGen1.out, ground.in);
    connect(generator1.out, scope1.in);
    connect(scope1.out, line1.in);
    connect(line1.out, scope2.in);
    connect(scope2.out, load1.in);
    connect(load1.n, ground.in);";
            string modelPowerSystem =
                @"
model:
	steadystate{
		solver = newton{
			iterations = 20,
			fAbsTol = 0.005,
			alpha = 1.0
		},
        baseFrequency = 60
	};
elements:
	generator1 = GeneratorY{
		Peak = 100.0,
		Phase = 0.0,
		Z = 0.001+ j 0.001,
        Frequency = 60 //in Herz
	};
	scope1 = Scope3p
	{
		Label = ""Generator""
    };
    scope2 = Scope3p
	{
		Label = ""Load""
    };
    scope3 = Scope1p
	{
		Label = ""ground""
    };
    scope4 = Scope3p
	{
		Label = ""line in""
    };
    scope5 = Scope3p
	{
		Label = ""line out""
    };
    resistorGen1 = Resistor{
		R = 1000
	};
    line1 = LinePiSection{
		  R = 0.002,
          L = 0.001,
          B = 0.01,
          G = 100000,
          Bp = 0.01
	};
	transformer1 = TransformerDy{
		K = 20,
		Zs = 0.001 + j 0.005,
		Zp = 0.001 + j 0.001,
        Rc = 10000,
        Xm = 10000,
        Group = Dy1
	};
	transformer2 = TransformerDd{
		K = 20,
		Zs = 0.001 + j 0.005,
		Zp = 0.001 + j 0.001,
        Rc = 10000.0,
        Xm = 10000,
        Group = Dd0
	};
	load1 = LoadY{
		ZA = 1,
		ZB = 1,
		ZC = 1
	};
	ground = Ground{
	};
connections:
	connect(generator1.n, resistorGen1.in);
    connect(resistorGen1.out,scope3.in);
    connect(scope3.out, ground.in);
    connect(generator1.out, scope1.in);
    connect(scope1.out, transformer1.in);
    connect(transformer1.out, scope4.in);
    connect(scope4.out,line1.in);
    connect(transformer1.out_n, ground.in);
    connect(line1.out, scope5.in);
    connect(scope5.out, transformer2.out);
    connect(transformer2.in, scope2.in);
    connect(scope2.out, load1.in);
    connect(load1.n, ground.in);";
            string modelTransient =
@"
model:
transient{
    solver = radauIIA5{
        iterations = 20,
        fAbsTol = 0.01,
        alpha = 1.0,
        step = 0.01
    }
    t0 = 0,
    t1 = 1
};
elements:
    v1 = voltageSource{
       Peak = 20412,
       Phase = 0,
       Frequency = 60
    };
    g = ground{
    };
    scope = scope1p{
        Label=""scope1""
    };
    r1 = resistor{
        R = 600
    };
    l1 = inductor{
        L = 20
    };
    r2 = resistor{
        R = 300
    };
    l2 = inductor{
        L = 6
    };
connections:
    connect(v1.in, g.in);
    connect(v1.out, r1.in);
    connect(r1.out, scope.in);
    connect(scope.out, l1.in);
    connect(l1.out, r2.in);
    connect(r2.out, l2.in);
    connect(l2.out, g.in);
";
            List<string> output = new List<string>();
            List<ErrorMessage> errorList = new List<ErrorMessage>();
            try
            {
                MainInterpreter.RunModel(modelPowerSystem, ref errorList,ref output);
                foreach (var o in output)
                {
                    Stdout.WriteLine(o);
                }
            }
            catch (CompilerException exc)
            {
                Stdout.WriteLine(exc.Message);
                var errors = exc.Errors;
                foreach (var error in errors)
                {
                    Stdout.WriteLine(error.Message + " Line: " + error.Line + " Position: " + error.Position);
                }
            }
            catch (Exception exc)
            {
                Stdout.WriteLine(exc.Message);
            }
            foreach (var error in errorList)
            {
                Stdout.WriteLine(error.Message + " Line: " + error.Line + " Position: " + error.Position);
            }
            Stdout.WriteLine("");
            Stdout.Flush();
            Stdout.Close();
        }
        static public void TestCircuitModel2()
        {
            Stream StdoutStream = Console.OpenStandardOutput();
            StreamWriter Stdout = new StreamWriter(StdoutStream);
            Stdout.WriteLine("\tTest circuit model 2");
            CircuitModelAC model = new CircuitModelAC();
            int v1 = model.AddVoltageSource("g", "0", 20412.41f, 0.0f, 60.0f);
            int r1 = model.AddResistor("0","1",600.0f);
            int r2 = model.AddInductor("1", "2", 20.0f);
            int r3 = model.AddResistor("2", "3", 300.0f);
            int rg = model.AddInductor("3", "g", 6.0f);
            model.AddGround("g");
            model.AddVoltageOutput("g", "2");
            model.AddCurrentOutput(v1);
            model.AddCurrentOutput(r1);
            model.AddCurrentOutput(r2);
            model.AddCurrentOutput(r3);
            model.AddCurrentOutput(rg);
            //Stdout.WriteLine(model.TestEquationGeneration(true));
            try
            {
                List<string> solution = model.Solve();
                foreach (var s in solution)
                {
                    Stdout.WriteLine(s);
                }
            }
            catch (CompilerException exc)
            {
                Stdout.WriteLine(exc.Message);
                var errors = exc.Errors;
                foreach (var error in errors)
                {
                    Stdout.WriteLine(error.Message + " Line: " + error.Line + " Position: " + error.Position);
                }
            }
            catch (Exception exc)
            {
                Stdout.WriteLine(exc.Message);
            }
            Stdout.WriteLine("");
            Stdout.Flush();
            Stdout.Close();
        }
        static public void TestCircuitModel()
        {
            Stream StdoutStream = Console.OpenStandardOutput();
            StreamWriter Stdout = new StreamWriter(StdoutStream);
            Stdout.WriteLine("\tTest circuit model");
            CircuitModelAC model = new CircuitModelAC();
            int v = model.AddVoltageSource("a1","a2",10.0f,0.0f,50.0f);
            model.AddTransformer("a1","a2","a3","a4",2.0f);
            int r = model.AddResistor("a3","a4",2.0f);
            model.AddGround("a1");
            model.AddGround("a3");
            model.AddVoltageOutput("a1", "a2");
            model.AddVoltageOutput("a3", "a4");
            model.AddCurrentOutput(v);
            model.AddCurrentOutput(r);
            /*model.addVoltageSource("a2", "a1", 10.0f, 60.0f, 50.0f);
            model.addVoltageSource("a1", "a3", 10.0f, 0.0f, 50.0f);
            model.addCapacitor("a1", "a3", 0.1f);
            model.addResistor("a1", "a4", 15.0f);
            model.addResistor("a4", "a2", 4.0f);
            model.addInductor("a4", "a3", 5.0f);
            model.addGround("a2");
            model.addVoltageOutput("a2","a3");
            model.addVoltageOutput("a2", "a4");
            model.addVoltageOutput("a2", "a1");*/
            Stdout.WriteLine(model.TestEquationGeneration());
            try
            {
                List<string> solution = model.Solve();
                foreach (var s in solution)
                {
                    Stdout.WriteLine(s);
                }
            }
            catch (CompilerException exc)
            {
                Stdout.WriteLine(exc.Message);
                var errors = exc.Errors;
                foreach (var error in errors)
                {
                    Stdout.WriteLine(error.Message + " Line: " + error.Line + " Position: " + error.Position);
                }
            }
            catch (Exception exc)
            {
                Stdout.WriteLine(exc.Message);
            }
            Stdout.WriteLine("");
            Stdout.Flush();
            Stdout.Close();
        }
        static public void TestCircuitEquationGeneration()
        {
            Stream StdoutStream = Console.OpenStandardOutput();
            StreamWriter Stdout = new StreamWriter(StdoutStream);
            Stdout.WriteLine("\t Test circuit equation generation");
            try
            {
                CircuitModelAC model = new CircuitModelAC();
                model.AddVoltageSource("a2", "a1", 10.0f, 50.0f, 10.0f);
                model.AddVoltageSource("a1", "a3", 10.0f, 50.0f, 0.5f);
                model.AddResistor("a1", "a3", 5.0f);
                model.AddResistor("a1", "a4", 15.0f);
                model.AddResistor("a4", "a2", 4.0f);
                model.AddGround("a2");
                Stdout.WriteLine("Equations: ");
                string equations = model.TestEquationGeneration();
                Stdout.WriteLine(equations);
            }
            catch (CompilerException exc)
            {
                Stdout.WriteLine(exc.Message);
                var errors = exc.Errors;
                foreach (var error in errors)
                {
                    Stdout.WriteLine(error.Message + " Line: " + error.Line + " Position: " + error.Position);
                }
            }
            catch (Exception exc)
            {
                Stdout.WriteLine(exc.Message);
            }
            Stdout.WriteLine("");
            Stdout.Flush();
            Stdout.Close();
            //Environment.NewLine
        }
        static public void TestNonlinearEquationParser()
        {
            //create file
            /*
            x*x+2=e^x*sin(x);
            x(0)=0;
            x(0)=2;
            solution at approx 1.4 and 2.33

            6x^5+-3x^4+7x^3+2x^2+-5x+7.13=0.
            1 root at -0.963
             */
            string equation1 = @"constant a = 2;x*x+a=e()^x*sin(x);x(0)=0;";
            string equation2 = @"x*x+2=e()^x*sin(x);x(0)=2;";
            string equation3 = @"x*x+2=e()^x*sin(x);x(0)=5;";
            string equation4 = @"6*x^5+-3*x^4+7*x^3+2*x^2+-5*x+7.13=0.;x(0)=0;";

            Stream StdoutStream = Console.OpenStandardOutput();
            StreamWriter Stdout = new StreamWriter(StdoutStream);
            Stdout.WriteLine("\tTest nonlinear equation parser");
            try
            {
                Equations.Nonlinear.Compiler compiler = new Equations.Nonlinear.Compiler();
                NonlinearEquationDescription compiledEquation = compiler.CompileEquations(equation1);
                Stdout.WriteLine("Equation 1: x=0");
                Stdout.WriteLine("x*x+2-e^x*sin(x)");
                Stdout.WriteLine("Derivative: 2x-e^x*sin(x)-e^x*cos(x)");
                Stdout.WriteLine(compiledEquation.PrintVariables());
                Stdout.WriteLine(compiledEquation.PrintEquations());
                Stdout.WriteLine(compiledEquation.PrintJacobiMatrix());
                compiledEquation = compiler.CompileEquations(equation2);
                Stdout.WriteLine("Equation 2 x=2");
                Stdout.WriteLine(compiledEquation.PrintVariables());
                Stdout.WriteLine(compiledEquation.PrintEquations());
                Stdout.WriteLine(compiledEquation.PrintJacobiMatrix());
                compiledEquation = compiler.CompileEquations(equation3);
                Stdout.WriteLine("Equation 3 x=5");
                Stdout.WriteLine(compiledEquation.PrintVariables());
                Stdout.WriteLine(compiledEquation.PrintEquations());
                Stdout.WriteLine(compiledEquation.PrintJacobiMatrix());
                compiledEquation = compiler.CompileEquations(equation4);
                Stdout.WriteLine("Equation 4 x=0");
                Stdout.WriteLine(compiledEquation.PrintVariables());
                Stdout.WriteLine(compiledEquation.PrintEquations());
                Stdout.WriteLine(compiledEquation.PrintJacobiMatrix());
            }
            catch (CompilerException exc)
            {
                Stdout.WriteLine(exc.Message);
                var errors = exc.Errors;
                foreach (var error in errors)
                {
                    Stdout.WriteLine(error.Message + " Line: " + error.Line + " Position: " + error.Position);
                }
            }
            catch (Exception exc)
            {
                Stdout.WriteLine(exc.Message);
            }
            Stdout.Flush();
            Stdout.Close();
        }
        static public string PrintSolution(Vector<double> solution, string[] variables, NonlinearEquationDescription system)
        {
            string result="";
            for (int i = 0; i < solution.Count; i++)
            {
                result += variables[i] + " = " + solution[i].ToString() + Environment.NewLine;
            }
            double[] x = solution.ToArray();
            for (int i = 0; i < system.Equations.Count; i++)
            {
                result += $"F{i}(X) = {system.Equations[i].Execute(x)}" + Environment.NewLine;
            }
            return result;
        }
        static public void TestNonlinearEquationSolver()
        {
            Stream StdoutStream = Console.OpenStandardOutput();
            StreamWriter Stdout = new StreamWriter(StdoutStream);
            Stdout.WriteLine("\tTest nonlinear equation solver");
            Equations.Nonlinear.Compiler compiler = new Equations.Nonlinear.Compiler();
            string equation1 = @"constant a = 2;x*x+a=e()^x*sin(x);x(0)=0;";
            string equation2 = @"x*x+2=e()^x*sin(x);x(0)=2;";
            string equation3 = @"x*x+2=e()^x*sin(x);x(0)=5;";
            string equation4 = @"6*x^5+-3*x^4+7*x^3+2*x^2+-5*x+7.13=0.;x(0)=-0.8;";
            try
            {
                {
                    NonlinearEquationDescription compiledEquation = compiler.CompileEquations(equation1);
                    MathUtils.NonlinearSystemSymbolicAnalytic system = new MathUtils.NonlinearSystemSymbolicAnalytic(compiledEquation);
                    //calc solution
                    Vector<double> solution = MathUtils.NewtonRaphsonSolver.Solve(
                    system,
                    Vector<double>.Build.DenseOfArray(compiledEquation.InitialValues),
                    20,
                    0.01,
                    1.0
                    );
                    Stdout.WriteLine("Equation:" + equation1);
                    Stdout.WriteLine("Solution:");
                    Stdout.WriteLine(PrintSolution(solution, compiledEquation.VariableNames, compiledEquation));
                }
                {
                    NonlinearEquationDescription compiledEquation = compiler.CompileEquations(equation2);
                    MathUtils.NonlinearSystemSymbolicAnalytic system = new MathUtils.NonlinearSystemSymbolicAnalytic(compiledEquation);
                    //calc solution
                    Vector<double> solution = MathUtils.NewtonRaphsonSolver.Solve(
                    system,
                    Vector<double>.Build.DenseOfArray(compiledEquation.InitialValues),
                    20,
                    0.01,
                    1.0
                    );
                    Stdout.WriteLine("Equation:" + equation2);
                    Stdout.WriteLine("Solution:");
                    Stdout.WriteLine(PrintSolution(solution, compiledEquation.VariableNames, compiledEquation));
                }
                {
                    NonlinearEquationDescription compiledEquation = compiler.CompileEquations(equation3);
                    MathUtils.NonlinearSystemSymbolicAnalytic system = new MathUtils.NonlinearSystemSymbolicAnalytic(compiledEquation);
                    //calc solution
                    Vector<double> solution = MathUtils.NewtonRaphsonSolver.Solve(
                    system,
                    Vector<double>.Build.DenseOfArray(compiledEquation.InitialValues),
                    20,
                    0.01,
                    1.0
                    );
                    Stdout.WriteLine("Equation:" + equation3);
                    Stdout.WriteLine("Solution:");
                    Stdout.WriteLine(PrintSolution(solution, compiledEquation.VariableNames, compiledEquation));
                }
                {
                    NonlinearEquationDescription compiledEquation = compiler.CompileEquations(equation4);
                    MathUtils.NonlinearSystemSymbolicAnalytic system = new MathUtils.NonlinearSystemSymbolicAnalytic(compiledEquation);
                    //calc solution
                    Vector<double> solution = MathUtils.NewtonRaphsonSolver.Solve(
                    system,
                    Vector<double>.Build.DenseOfArray(compiledEquation.InitialValues),
                    20,
                    0.01,
                    1.0
                    );
                    Stdout.WriteLine("Equation:" + equation4);
                    Stdout.WriteLine("Solution (-0.96):");
                    Stdout.WriteLine(PrintSolution(solution, compiledEquation.VariableNames, compiledEquation));
                }
            }
            catch (CompilerException exc)
            {
                Stdout.WriteLine(exc.Message);
                var errors = exc.Errors;
                foreach (var error in errors)
                {
                    Stdout.WriteLine(error.Message + " Line: " + error.Line + " Position: " + error.Position);
                }
            }
            catch (Exception exc)
            {
                Stdout.WriteLine(exc.Message);
            }
            Stdout.Flush();
            Stdout.Close();
        }

#if DAE
        static public void TestDAEInterpreter()
        {
            string differentialEquation = "x'=x;" +
                "x(t0)=1.0;" +
                "t0=0;";
            string DAEequation = "constant L=1.0;" +
                "x^2+y^2=L;" +
                "x'=;" +
                "y'=;";
            DAECompiler compiler = new DAECompiler();
            Stream StdoutStream = Console.OpenStandardOutput();
            StreamWriter Stdout = new StreamWriter(StdoutStream);
            try {
                DAEDefinition def = compiler.compileDAE(differentialEquation);
                Stdout.WriteLine("Equation 1:");
                Stdout.WriteLine(def.PrintSystem());
                def = compiler.compileDAE(DAEequation);
                Stdout.WriteLine("Equation 2 x=2");
                Stdout.WriteLine(def.PrintSystem());

            } catch (Exception exc)
            {

            }
        }
#endif
    }
}
