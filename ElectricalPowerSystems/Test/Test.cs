using ElectricalPowerSystems.ACGraph;
using ElectricalPowerSystems.Interpreter;
using ElectricalPowerSystems.Interpreter.Equations.DAE;
using ElectricalPowerSystems.Interpreter.Equations.Nonlinear;
using ElectricalPowerSystems.MathUtils;
using ElectricalPowerSystems.PowerGraph;
using ElectricalPowerSystems.Transients;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
voltageSource("4", "1", 1000.0, 0.0, 60.0);
voltageSource("4", "2", 1000.0, 120.0, 60.0);
voltageSource("4", "3", 1000.0, 240.0, 60.0);
resistor("8", "9", 1.0);
resistor("8", "10", 1.0);
resistor("8", "11", 1.0);
inductor("5","9",0);
inductor("6","10",0);
inductor("7","11",0);
elN=line("8","4");
elA=line("1","5");
elB=line("2","6");
elC=line("3","7");
ground("4");
current(elN);
current(elA);
current(elB);
current(elC);
     */



namespace ElectricalPowerSystems.Test
{
    class Test
    {
        static public void testCircuitModel()
        {
            CircuitModelAC model = new CircuitModelAC();
            model.addVoltageSource("a2", "a1", 10.0f, 50.0f, 10.0f);
            model.addVoltageSource("a1", "a3", 10.0f, 50.0f, 0.5f);
            model.addResistor("a1", "a3", 5.0f);
            model.addResistor("a1", "a4", 15.0f);
            model.addResistor("a4", "a2", 4.0f);
            model.addGround("a2");
            model.Solve();
        }
        static public void testCircuitEquationgeneration()
        {
            CircuitModelAC model = new CircuitModelAC();
            model.addVoltageSource("a2", "a1", 10.0f, 50.0f, 10.0f);
            model.addVoltageSource("a1", "a3", 10.0f, 50.0f, 0.5f);
            model.addResistor("a1", "a3", 5.0f);
            model.addResistor("a1", "a4", 15.0f);
            model.addResistor("a4", "a2", 4.0f);
            model.addGround("a2");
            string equations = model.testEquationGeneration();
            Stream StdoutStream = Console.OpenStandardOutput();
            StreamWriter Stdout = new StreamWriter(StdoutStream);
            Stdout.WriteLine(equations);
            Stdout.Flush();
            Stdout.Close();
        }
        static public void testPowerModel()
        {
            PowerGraph.PowerGraphManager graph = new PowerGraph.PowerGraphManager();
            PowerGraph.PowerGraphManager.powerFrequency = (float)(60.0 * 2.0 * Math.PI);
            List<int> elements=new List<int>();
            elements.Add(graph.addElement(new PowerGraph.GraphGeneratorVWye("a1", 100.0f,0.0f, new ResistanceGrounding(1000.0f))));
            elements.Add(graph.addElement(new PowerGraph.GraphGeneratorVDelta("a10", 100.0f,0.0f)));
            elements.Add(graph.addElement(new PowerGraph.GraphAirLinePiSection("a1", "a2", 10,1,0.001f,1.0f,0.001f)));
            elements.Add(graph.addElement(new PowerGraph.GraphAirLinePiSection("a3", "a4", 20, 2, 0.005f, 3.0f, 0.006f)));
            elements.Add(graph.addElement(new PowerGraph.GraphAirLinePiSection("a1", "a6", 20, 2, 0.005f, 3.0f, 0.006f)));
            elements.Add(graph.addElement(new PowerGraph.GraphAirLinePiSection("a2", "a10", 20, 2, 0.005f, 3.0f, 0.006f)));
            elements.Add(graph.addElement(new PowerGraph.GraphAirLinePiSection("a7", "a8", 20, 2, 0.005f, 3.0f, 0.006f)));
            elements.Add(graph.addElement(new PowerGraph.GraphTransformer2w("a2", "a3",10,
                new Complex32(),new Complex32(),
                new WyeWinding(WyeWinding.Mode.Y0,new SolidGrounding()),
                new DeltaWinding(DeltaWinding.Mode.D1))));
            elements.Add(graph.addElement(new PowerGraph.GraphTransformer2w("a4", "a5",0.2f,
                new Complex32(25, 0.5f),
                new Complex32(15, 0.2f),
                new WyeWinding(WyeWinding.Mode.Y0, new Ungrounded()),
                new WyeWinding(WyeWinding.Mode.Y0, new Ungrounded()))));
            elements.Add(graph.addElement(new PowerGraph.GraphTransformer2w("a6", "a7", 0.2f,
                new Complex32(10,2),
                new Complex32(20,1),
                new DeltaWinding(DeltaWinding.Mode.D1),
                new DeltaWinding(DeltaWinding.Mode.D1))));
            elements.Add(graph.addElement(new PowerGraph.GraphTransformer2w("a8", "a9", 0.2f,
                new Complex32(25, 0.5f),
                new Complex32(15, 0.2f),
                new DeltaWinding(DeltaWinding.Mode.D1),
                new WyeWinding(WyeWinding.Mode.Y0, new Ungrounded()))));
            elements.Add(graph.addElement(new PowerGraph.GraphLoadDelta("a3", 1.0f, 1.0f, 1.0f)));
            elements.Add(graph.addElement(new PowerGraph.GraphLoadDelta("a5", 1.0f, 1.0f, 1.0f)));
            elements.Add(graph.addElement(new PowerGraph.GraphLoadWye("a9", 1.0f, 1.0f, 1.0f,new Ungrounded())));


            foreach (var element in elements)
            {
                graph.addOutput(new PowerOutput(OutputMode.FULL, element));
                graph.addOutput(new CurrentOutput(OutputMode.FULL, element));
            }
            graph.addOutput(new VoltageOutput(OutputMode.FULL, "a1"));
            /*graph.addOutput(new VoltageOutput(OutputMode.FULL, "a1"));
            graph.addOutput(new VoltageOutput(OutputMode.FULL, "a2"));
            graph.addOutput(new VoltageOutput(OutputMode.FULL, "a3"));*/
            List<string> errors=new List<string>();
            List<string> output = new List<string>();
            graph.solve(ref errors,ref output);
            //send output to console stream
            Stream StdoutStream = Console.OpenStandardOutput();
            StreamWriter Stdout = new StreamWriter(StdoutStream);
            foreach(var line in output)
                Stdout.WriteLine(line);
            Stdout.Flush();
            Stdout.Close();
        }
        //Done
        static public void testNonlinearEquationParser()
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
            string equation1 = @"x*x+2=e()^x*sin(x);x[0]=0;";
            string equation2 = @"x*x+2=e()^x*sin(x);x[0]=2;";
            string equation3 = @"x*x+2=e()^x*sin(x);x[0]=5;";
            string equation4 = @"6*x^5+-3*x^4+7*x^3+2*x^2+-5*x+7.13=0.;x[0]=0;";

            Stream StdoutStream = Console.OpenStandardOutput();
            StreamWriter Stdout = new StreamWriter(StdoutStream);
            EquationCompiler compiler = new EquationCompiler();
            try
            {
                NonlinearEquationDefinition compiledEquation = compiler.compileEquations(equation1);
                Stdout.WriteLine("Equation 1: x=0");
                Stdout.WriteLine("x*x+2-e^x*sin(x)");
                Stdout.WriteLine("Derivative: 2x-e^x*sin(x)-e^x*cos(x)");
                Stdout.WriteLine(compiledEquation.PrintVariables());
                Stdout.WriteLine(compiledEquation.PrintEquations());
                Stdout.WriteLine(compiledEquation.PrintJacobiMatrix());
                compiledEquation = compiler.compileEquations(equation2);
                Stdout.WriteLine("Equation 2 x=2");
                Stdout.WriteLine(compiledEquation.PrintVariables());
                Stdout.WriteLine(compiledEquation.PrintEquations());
                Stdout.WriteLine(compiledEquation.PrintJacobiMatrix());
                compiledEquation = compiler.compileEquations(equation3);
                Stdout.WriteLine("Equation 3 x=5");
                Stdout.WriteLine(compiledEquation.PrintVariables());
                Stdout.WriteLine(compiledEquation.PrintEquations());
                Stdout.WriteLine(compiledEquation.PrintJacobiMatrix());
                compiledEquation = compiler.compileEquations(equation4);
                Stdout.WriteLine("Equation 4 x=0");
                Stdout.WriteLine(compiledEquation.PrintVariables());
                Stdout.WriteLine(compiledEquation.PrintEquations());
                Stdout.WriteLine(compiledEquation.PrintJacobiMatrix());
            }
            catch (Exception exc)
            {
                Stdout.WriteLine(exc.Message);
                var errors = compiler.getErrors();
                foreach (var error in errors)
                {
                    Stdout.WriteLine(error.Message+" Line: "+error.Line+" Position: "+error.Position);
                }
            }
            Stdout.Flush();
            Stdout.Close();
        }
        static public string printSolution(Vector<double> solution, string[] variables, NonlinearEquationDefinition system)
        {
            string result="";
            for (int i = 0; i < solution.Count; i++)
            {
                result += variables[i] + " = " + solution[i].ToString()+ Environment.NewLine;
            }
            double[] x = solution.ToArray();
            for (int i = 0; i < system.Equations.Count; i++)
            {
                result += $"F{i}(0) = {system.Equations[i].execute(x)}"+ Environment.NewLine;
            }
            return result;
        }
        static public void testNonlinearEquationSolver()
        {
            Stream StdoutStream = Console.OpenStandardOutput();
            StreamWriter Stdout = new StreamWriter(StdoutStream);
            EquationCompiler compiler = new EquationCompiler();
            string equation1 = @"x*x+2=e()^x*sin(x);x[0]=0;";
            string equation2 = @"x*x+2=e()^x*sin(x);x[0]=2;";
            string equation3 = @"x*x+2=e()^x*sin(x);x[0]=5;";
            string equation4 = @"6*x^5+-3*x^4+7*x^3+2*x^2+-5*x+7.13=0.;x[0]=0;";

            try
            {
                NonlinearEquationDefinition compiledEquation = compiler.compileEquations(equation1);
                NonlinearSystemSymbolicAnalytic system = new NonlinearSystemSymbolicAnalytic(compiledEquation);
                //calc solution
                Vector<double> solution = NewtonRaphsonSolver.Solve(
                    system,
                    Vector<double>.Build.DenseOfArray(compiledEquation.InitialValues),
                    20,
                    0.01,
                    1.0
                    );
                Stdout.WriteLine("Solution:");
                Stdout.WriteLine(printSolution(solution,compiledEquation.VariableNames,compiledEquation));
                //show solution
            }
            catch (Exception exc)
            {
                Stdout.WriteLine(exc.Message);
                var errors = compiler.getErrors();
                foreach (var error in errors)
                {
                    Stdout.WriteLine(error.Message + " Line: " + error.Line + " Position: " + error.Position);
                }
            }
            Stdout.Flush();
            Stdout.Close();
        }
        static public void testDAEInterpreter()
        {
            string differentialEquation = "x'=x;" +
                "x(t0)=1.0;" +
                "t0=0;";
            string DAEequation = "set L=1.0;" +
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
        static public void testDAEInterpreterAndSolver()
        {
            string differentialEquation = "x'=x;" +
                "x(t0)=1.0;" +
                "t0=0;" +
                "time=5";
            string DAEequation = "set L=1.0;" +
                "x^2+y^2=L;" +
                "x'=;" +
                "y'=;" +
                "time=5";
            Stream StdoutStream = Console.OpenStandardOutput();
            StreamWriter Stdout = new StreamWriter(StdoutStream);
            throw new NotImplementedException();
            try
            {
                /*ChartWindow window = new ChartWindow();
                RADAUIIA3 solver = new RADAUIIA3();
                DAEImplicitSystem system;
                List<double> timeArray = new List<double>();
                List<double> xArray = new List<double>();
                List<double> yArray = new List<double>();
                xArray.Add(system.x0);
                yArray.Add(system.z0);
                timeArray.Add(system.t0);
                for(double t=system.t0;t<system.t0+system.time;)
                {
                    points =  solver.IntegrateStep(system, x, t);
                    t = points.t;
                    timeArray.Add(t);
                }
                window.addLineSeries(xArray.ToArray(),timeArray.ToArray(),"x");
                window.addLineSeries(yArray.ToArray(),timeArray.ToArray(),"y");*/
            }
            catch (Exception exc)
            {

            }
        }
        static public void testTransientEquationGenerator()
        {
        }
        static public void testTransientSolver()
        {

        }
    }
}
