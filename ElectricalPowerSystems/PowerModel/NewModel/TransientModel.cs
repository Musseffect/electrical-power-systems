#define MODELINTERPRETER
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerModel.NewModel
{
#if MODELINTERPRETER
    public class Parameter
    {
        string name;
        public string Name { get { return name; } }
        double value;
        public double Value { get { return value; } }
        public Parameter(string name, double value)
        {
            this.name = name;
            this.value = value;
        }
    }
    public class TransientSystem
    {
        double[] parameters;
        Dictionary<string, int> parameterIndicies;
        public void UpdateParameters(List<Parameter> parameters)
        {
            foreach (var parameter in parameters)
            {
                int index = parameterIndicies[parameter.Name];
                this.parameters[index] = parameter.Value;
            }
        }
    }

    public interface ITransientSolver
    {
        Vector<double> Step(TransientSystem system, Vector<double> x);
    }
    public class TransientSolution
    {
        List<double[]> values;
        List<double> time;
        string[] variables;
        public void Plot()
        {
            ChartWindow window = new ChartWindow();
            window.Plot(values, time, variables);
            window.Show();
        }
    }
    public abstract class TransientEvent
    {
        double time;
        public double Time { get { return time; } }
        public abstract bool Execute(Vector<double> x);
        public abstract List<Parameter> GetParameters();
    }
    public class TransientModel : IModel
    {
        List<ITransientElement> elements;
        List<TransientEvent> events;
        double t0;
        double t1;
        ITransientSolver solver;
        List<Pin> nodeList;
        private string GenerateEquations()
        {
            string equations = "";
            Dictionary<string, List<CurrentFlowBlock>> nodeEquationsIn = new Dictionary<string, List<CurrentFlowBlock>>();
            Dictionary<string, List<CurrentFlowBlock>> nodeEquationsOut = new Dictionary<string, List<CurrentFlowBlock>>();
            for (int i = 0; i < nodeList.Count; i++)
            {
                string[] variables = nodeList[i].GetNodeVariables();
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
                string[] variables = nodeList[i].GetNodeVariables();
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
            foreach (var element in elements)
            {
                var blocks = element.GenerateParameters();
                foreach (var block in blocks)
                {
                    equations += block.Equation + Environment.NewLine;
                }
            }
            return equations;
        }
        private double UpdateState(double tPrev, double t,Vector<double> xPrev,Vector<double> x, ref TransientSystem system)
        {
            throw new NotImplementedException();
            double dt = t - tPrev;
            //for each event interpolate values and update stuff
            while (events.Count>0)
            {
                TransientEvent _event = events.First<TransientEvent>();
                if (_event.Time > t)
                {
                    return t;
                }
#if TEST||DEBUG
                if (_event.Time < tPrev)
                    throw new Exception("UpdateState exception");
#endif
                events.RemoveAt(0);
                Vector<double> xCurrent = MathUtils.MathUtils.Interpolate(xPrev, x, (_event.Time - tPrev) / dt);
                if (_event.Execute(xCurrent)) ;
                {
                    system.UpdateParameters(_event.GetParameters());
                    double eventTime = _event.Time;
                    while (true)
                    {
                        _event = events.First<TransientEvent>();
                        if (_event.Time > eventTime)
                            break;
                        events.RemoveAt(0);
                        if (_event.Execute(xCurrent))
                        {
                            system.UpdateParameters(_event.GetParameters());
                        }
                    }
                    return eventTime;
                }
            }
            return t;
        }
        private void InitEvents()
        {
            //sort events by time
            events.Sort();
        }
        private TransientSolution Solve(string equations)
        {
            throw new NotImplementedException();
            double t = t0;
            InitEvents();
            TransientSystem system = null;
            Vector<double> x;
            while (t < t1)
            {
                double tPrev = t;
                //make step
                Vector<double> xNew = solver.Step( system ,x);
                double dt = t - tPrev;
                //update t
                //if state happened during [tprev,tcurr]
                t = UpdateState(tPrev, t, x, xNew, ref system);
                x = MathUtils.MathUtils.Interpolate(x,xNew,(t-tPrev)/dt);
            }
        }
        public string Solve()
        {
            string equations = GenerateEquations();
            TransientSolution solution = this.Solve(equations);
            solution.Plot();
            return solution.ToString();
        }
        public void SetT0(double t0)
        {
            this.t0 = t0;
        }
        public void SetT1(double t1)
        {
            this.t1 = t1;
        }
        public void AddPin(Pin pin)
        {
            nodeList.Add(pin);
        }
        public void AddElement(ITransientElement element)
        {
            (element as Element).SetIndex(elements.Count);
            elements.Add(element);
        }
        public string GetEquations()
        {
            return GenerateEquations();
        }
    }
#endif
}
