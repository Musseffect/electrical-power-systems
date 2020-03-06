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
        public class SteadyStateSolver
        {
            public SteadyStateSolution Solve(string equations)
            {
            }
        }
        public class SteadyStateSolution
        {

        }
        public class SteadyStateModel : IModel
        {
            private List<ISteadyStateElement> elements;
            private SteadyStateSolver solver;
            private List<Node> nodeList;
            private double frequency;
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
                        nodeEquationsIn.Add($"{variable}_re", new List<CurrentFlowBlock>());
                        nodeEquationsIn.Add($"{variable}_im", new List<CurrentFlowBlock>());
                    }
                    /*nodeEquationsIn.Add($"v_{i}_re", new List<CurrentFlowBlock>());
                    nodeEquationsIn.Add($"v_{i}_im", new List<CurrentFlowBlock>());
                    nodeEquationsOut.Add($"v_{i}_re", new List<CurrentFlowBlock>());
                    nodeEquationsOut.Add($"v_{i}_im", new List<CurrentFlowBlock>());*/
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
                        var real = $"{variable}_re";
                        var im = $"{variable}_im";
                        var nodeInRe = nodeEquationsIn[real];
                        var nodeOutRe = nodeEquationsOut[real];
                        var nodeInIm = nodeEquationsIn[im];
                        var nodeOutIm = nodeEquationsOut[im];
                        string leftRe = "";
                        int k = 0;
                        //left side = right side
                        foreach (var block in nodeOutRe)
                        {
                            if (k != 0)
                                leftRe += " + ";
                            leftRe += block.Equation;
                            k++;
                        }
                        if (nodeOutRe.Count == 0)
                            leftRe = "0";
                        string rightRe = "";
                        k = 0;
                        foreach (var block in nodeInRe)
                        {
                            if (k != 0)
                                rightRe += " + ";
                            rightRe += block.Equation;
                            k++;
                        }
                        if (nodeInRe.Count == 0)
                            rightRe = "0";
                        string eqRe = $"{leftRe} = {rightRe}; //node {i}_re";
                        if (nodeOutRe.Count + nodeInRe.Count > 0)
                            equations += eqRe + Environment.NewLine;
                        string leftIm = "";
                        k = 0;
                        foreach (var block in nodeOutIm)
                        {
                            if (k != 0)
                                leftIm += " + ";
                            leftIm += block.Equation;
                            k++;
                        }
                        if (nodeOutIm.Count == 0)
                            leftIm = "0";
                        string rightIm = "";
                        k = 0;
                        foreach (var block in nodeInIm)
                        {
                            if (k != 0)
                                rightIm += " + ";
                            rightIm += block.Equation;
                            k++;
                        }
                        if (nodeInRe.Count == 0)
                            rightIm = "0";
                        string eqIm = $"{leftIm} = {rightIm}; //node {i}_im";
                        if (nodeOutIm.Count + nodeInIm.Count > 0)
                            equations += eqIm + Environment.NewLine;
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
                equations += $"set frequency = {frequency.ToString(new System.Globalization.CultureInfo("en-US"))} * 2 * pi();";
                return equations;
            }
            public string Solve()
            {


                string equations = GenerateEquations();
                SteadyStateSolution solution = solver.Solve(equations);
                return solution.ToString();
            }
        }
    }
#endif
}
