#define MODELINTERPRETER
using ElectricalPowerSystems.Equations.Nonlinear;
using ElectricalPowerSystems.MathUtils;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerModel.NewModel
{
#if MODELINTERPRETER
    public interface IScopeReading
    {
    }
    public class SteadyStateSolution
    {
        List<IScopeReading[]> scopeReadings;
        public SteadyStateSolution(int frequencyCount,int scopeElements)
        {
            this.scopeReadings = new List<IScopeReading[]>();
            for (int i = 0; i < scopeElements; i++)
            {
                scopeReadings.Add(new IScopeReading[frequencyCount]);
            }
        }
        public void Set(IScopeReading reading,int scopeIndex,int frequencyIndex)
        {
            this.scopeReadings[scopeIndex][frequencyIndex] = reading;
        }
        public string ToString(List<IScopeElement> scopes,double[]frequencies)
        {
            string result = "";
            for (int i = 0; i < scopes.Count; i++)
            {
                string scopeReading = scopes[i].FormatReadings(scopeReadings[i], frequencies);
                result += scopeReading + Environment.NewLine;
            }
            foreach (var readingSequence in scopeReadings)
            {
                /*string scopeReading = $"Scope {labels[index]}: {{";
                foreach (var reading in readingSequence)
                {
                    scopeReading += reading.ToString();
                }
                scopeReading += "}";*/
            }
            return result;
        }
    }
    public class SteadyStateModel : IModel
    {
        private List<ISteadyStateElement> elements;
        private ISteadyStateSolver solver;
        private List<Pin> nodeList;
        private SortedSet<double> frequencies;
        private double baseFrequency;
        public SteadyStateModel()
        {
            elements = new List<ISteadyStateElement>();
            nodeList = new List<Pin>();
            frequencies = new SortedSet<double>();
        }
        public void SetBaseFrequency(double value)
        {
            baseFrequency = value;
        }
        public void AddPin(Pin pin)
        {
            nodeList.Add(pin);
        }
        private string GenerateParameters(double frequency)
        {
            string result = "";
            foreach (var element in elements)
            {
                var blocks = element.GenerateParameters(frequency);
                foreach (var block in blocks)
                {
                    result += block.Equation + Environment.NewLine;
                }
            }
            result += $"constant frequency = {frequency.ToString(new System.Globalization.CultureInfo("en-US"))} * 2 * pi();";
            return result;
        }
        private string GenerateEquations()
        {
            string equations = "";
            equations += $"constant baseFrequency = {baseFrequency.ToString(new CultureInfo("en-US"))} * 2 * pi();{Environment.NewLine}";
            Dictionary<string, List<CurrentFlowBlock>> nodeEquationsIn = new Dictionary<string, List<CurrentFlowBlock>>();
            Dictionary<string, List<CurrentFlowBlock>> nodeEquationsOut = new Dictionary<string, List<CurrentFlowBlock>>();
            for (int i = 0; i < nodeList.Count; i++)
            {
                string[] variables = nodeList[i].GetNodeComplexVariables();
                foreach (var variable in variables)
                {
                    nodeEquationsIn.Add(variable, new List<CurrentFlowBlock>());
                    nodeEquationsOut.Add(variable, new List<CurrentFlowBlock>());
                }
            }
            foreach (var element in elements)
            {
                List<EquationBlock> blocks = element.GenerateEquations();
                foreach (var block in blocks)
                {
                    if (block is CurrentFlowBlock currentFlowBlock)
                    {
                        if (currentFlowBlock.Node1 != null)
                            nodeEquationsIn[currentFlowBlock.Node1].Add(currentFlowBlock);
                        if (currentFlowBlock.Node2 != null)
                            nodeEquationsOut[currentFlowBlock.Node2].Add(currentFlowBlock);
                    }
                    else
                    {
                        equations += block.Equation + Environment.NewLine;
                    }
                }
            }
            equations += "//Current equations" + Environment.NewLine;
            for (int i = 0; i < nodeList.Count; i++)
            {
                string[] variables = nodeList[i].GetNodeComplexVariables();
                foreach (var variable in variables)
                {
                    var nodeIn = nodeEquationsIn[variable];
                    var nodeOut = nodeEquationsOut[variable];
                    string left = "";
                    int k = 0;
                    //left side = right side
                    foreach (var block in nodeOut)
                    {
                        if (k != 0)
                            left += " + ";
                        left += block.Equation;
                        k++;
                    }
                    if (nodeOut.Count == 0)
                        left = "0";
                    string right = "";
                    k = 0;
                    foreach (var block in nodeIn)
                    {
                        if (k != 0)
                            right += " + ";
                        right += block.Equation;
                        k++;
                    }
                    if (nodeIn.Count == 0)
                        right = "0";
                    string eq = $"{left} = {right}; //node {variable}";
                    if (nodeOut.Count + nodeIn.Count > 0)
                        equations += eq + Environment.NewLine;
                }
            }
            /*
            foreach (var element in elements)
            {
                var blocks = element.GenerateParameters();
                foreach (var block in blocks)
                {
                    equations += block.Equation + Environment.NewLine;
                }
            }
            equations += $"set frequency = {frequency.ToString(new System.Globalization.CultureInfo("en-US"))} * 2 * pi();";*/
            return equations;
        }
        public void SetSolver(ISteadyStateSolver solver)
        {
            this.solver = solver;
        }
        public void AddElement(ISteadyStateElement element)
        {
            if (element is IACSourceElement)
            {
                double[] elementFrequencies = (element as IACSourceElement).GetFrequencies();
                foreach (double frequency in elementFrequencies)
                {
                    frequencies.Add(frequency);
                }
            }
            (element as Element).SetIndex(elements.Count);
            elements.Add(element);
        }
        public string GetEquations()
        {
            string equations = GenerateEquations();
            foreach (var frequency in frequencies)
            {
                equations += $"//Parameters for frequency {frequency.ToString(new CultureInfo("en-US"))}" + Environment.NewLine;
                equations += GenerateParameters(frequency);
            }
            return equations;
        }
        public string Solve()
        {
            EquationCompiler compiler = new EquationCompiler();
            if (frequencies.Count == 0)
                frequencies.Add(0.0);
            for (int i = 0; i < elements.Count; i++)
            {
                (elements[i] as Element).SetIndex(i);
            }
            string equations = GenerateEquations();
            List<IScopeElement> scopeElements = new List<IScopeElement>();
            foreach (var element in elements)
            {
                if (element is IScopeElement)
                {
                    scopeElements.Add(element as IScopeElement);
                }
            }
            SteadyStateSolution solution = new SteadyStateSolution(frequencies.Count, scopeElements.Count);
            int frequencyIndex = 0;
            foreach (var frequency in frequencies)
            {
                string system = equations;
                system += "//Parameters" + Environment.NewLine;
                system += GenerateParameters(frequency);
                //parse equations
                NonlinearEquationDefinition compiledEquation = compiler.CompileEquations(system);
                NonlinearSystemSymbolicAnalytic nonlinearSystem = new NonlinearSystemSymbolicAnalytic(compiledEquation);
                //solve
                Vector<double> values = solver.Solve(nonlinearSystem, Vector<double>.Build.DenseOfArray(compiledEquation.InitialValues));
                NonlinearSystemSolution systemSolution = compiledEquation.GetSolution(values);
                //save results
                int index = 0;
                foreach (var scope in scopeElements)
                {
                    solution.Set(scope.GetReading(systemSolution), index, frequencyIndex);
                    index++;
                }
                frequencyIndex++;
            }
            return solution.ToString(scopeElements,frequencies.ToArray());
        }
    }
#endif
}
