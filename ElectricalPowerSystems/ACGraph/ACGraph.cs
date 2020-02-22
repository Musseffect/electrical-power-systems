using ElectricalPowerSystems.Interpreter.Equations.Nonlinear;
using ElectricalPowerSystems.MathUtils;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.ACGraph
{
    public class ACGraphSolution
    {
        public List<Complex32> voltages;//Комплексные напряжения
        public List<Complex32> voltageDrops;//Комплексныепадения напряжений для всех элементов
        public List<Complex32> currents;//Комплексные токи для всех элементов
        public float frequency;//Частота активных элементов схемы расчёта
        public ACGraphSolution()
        {
            voltages = new List<Complex32>();
            voltageDrops = new List<Complex32>();
            currents = new List<Complex32>();
            frequency = 0.0f;
        }
    }
    public class Node
    {
        public bool grounded;
        public List<int> connectedElements;
        public Node()
        {
            grounded = false;
            connectedElements = new List<int>();
        }
    }
    public class ACGraph
    {
        public List<ElementsAC.Element> elements;
        public List<int> voltageSources;
        public List<int> currentSources;
        public List<int> transformers;
        public List<int> inductors;
        public List<Node> nodesList;
        public List<int> lines;
        public int groundsCount;
        public ACGraph()
        {
            groundsCount = 0;
            lines = new List<int>();
            nodesList = new List<Node>();
            inductors = new List<int>();
            currentSources = new List<int>();
            voltageSources = new List<int>();
            transformers = new List<int>();
            elements=new List<ElementsAC.Element>();
        }
        public int AllocateNode()
        {
            nodesList.Add(new Node());
            return nodesList.Count - 1;
        }
        public class NodePair
        {
            public int node1;
            public int node2;
            public NodePair(int node1, int node2)
            {
                this.node1 = node1;
                this.node2 = node2;
            }
        }
        public int CreateImpedance(int node1, int node2, Complex32 impedance)
        {
            nodesList[node1].connectedElements.Add(elements.Count);
            nodesList[node2].connectedElements.Add(elements.Count);
            elements.Add(new ElementsAC.Impedance(node1, node2, elements.Count, impedance));
            return elements.Count - 1;
        }
        public int CreateImpedanceWithCurrent(int node1, int node2, Complex32 impedance)
        {
            nodesList[node1].connectedElements.Add(elements.Count);
            nodesList[node2].connectedElements.Add(elements.Count);
            elements.Add(new ElementsAC.ImpedanceWithCurrent(node1, node2, elements.Count, impedance));
            return elements.Count - 1;
        }
        public int CreateResistorWithCurrent(int node1, int node2, float resistance)
        {
            nodesList[node1].connectedElements.Add(elements.Count);
            nodesList[node2].connectedElements.Add(elements.Count);
            elements.Add(new ElementsAC.ResistorWithCurrent(node1, node2, elements.Count, resistance));
            return elements.Count - 1;
        }
        public int CreateResistor(int node1, int node2, float resistance)
        {
            nodesList[node1].connectedElements.Add(elements.Count);
            nodesList[node2].connectedElements.Add(elements.Count);
            elements.Add(new ElementsAC.Resistor(node1, node2, elements.Count, resistance));
            return elements.Count - 1;
        }
        public int CreateLine(int node1, int node2)
        {
            nodesList[node1].connectedElements.Add(elements.Count);
            nodesList[node2].connectedElements.Add(elements.Count);
            lines.Add(elements.Count);
            elements.Add(new ElementsAC.Line(node1, node2,elements.Count));
            return elements.Count - 1;
        }
        public int CreateCapacitor(int node1, int node2, float capacity)
        {
            nodesList[node1].connectedElements.Add(elements.Count);
            nodesList[node2].connectedElements.Add(elements.Count);
            elements.Add(new ElementsAC.Capacitor(node1, node2, elements.Count, capacity));
            return elements.Count - 1;
        }
        public int CreateVoltageSource(int node1, int node2, float voltage, float phase, float freq)
        {
            nodesList[node1].connectedElements.Add(elements.Count);
            nodesList[node2].connectedElements.Add(elements.Count);
            voltageSources.Add(elements.Count);
            elements.Add(new ElementsAC.VoltageSource(node1, node2, elements.Count, voltage, phase, freq));
            return elements.Count - 1;
        }
        public int CreateCurrentSource(int node1, int node2, float current, float phase, float freq)
        {
            nodesList[node1].connectedElements.Add(elements.Count);
            nodesList[node2].connectedElements.Add(elements.Count);
            currentSources.Add(elements.Count);
            elements.Add(new ElementsAC.CurrentSource(node1, node2, elements.Count, current, phase, freq));
            return elements.Count - 1;
        }
        public void CreateGround(int node)
        {
            nodesList[node].connectedElements.Add(elements.Count);
            nodesList[node].grounded = true;
            elements.Add(new ElementsAC.Ground(node,elements.Count));
            groundsCount++;
        }
        public int CreateInductor(int node1, int node2, float inductivity)
        {
            nodesList[node1].connectedElements.Add(elements.Count);
            nodesList[node2].connectedElements.Add(elements.Count);
            inductors.Add(elements.Count);
            elements.Add(new ElementsAC.Inductor(node1, node2, elements.Count, inductivity));
            return elements.Count - 1;
        }
        public int CreateTransformer(int n1, int n2, int n3, int n4, float k)
        {
            nodesList[n1].connectedElements.Add(elements.Count);
            nodesList[n2].connectedElements.Add(elements.Count);
            nodesList[n3].connectedElements.Add(elements.Count);
            nodesList[n4].connectedElements.Add(elements.Count);
            transformers.Add(elements.Count);
            elements.Add(new ElementsAC.Transformer2w(n1,n2,n3,n4, elements.Count, k));
            return elements.Count-1;
        }
        public int CreateTransformer3w(int n1, int n2, int n3, int n4,int n5,int n6, float k1,float k2)
        {
            nodesList[n1].connectedElements.Add(elements.Count);
            nodesList[n2].connectedElements.Add(elements.Count);
            nodesList[n3].connectedElements.Add(elements.Count);
            nodesList[n4].connectedElements.Add(elements.Count);
            nodesList[n5].connectedElements.Add(elements.Count);
            nodesList[n6].connectedElements.Add(elements.Count);
            transformers.Add(elements.Count);
            elements.Add(new ElementsAC.Transformer3w(n1, n2, n3, n4,n5,n6, elements.Count, k1,k2));
            return elements.Count - 1;
        }
        public int CreateAutotransformer(int n1, int n2, int n3, float k)
        {
            nodesList[n1].connectedElements.Add(elements.Count);
            nodesList[n2].connectedElements.Add(elements.Count);
            nodesList[n3].connectedElements.Add(elements.Count);
            transformers.Add(elements.Count);
            elements.Add(new ElementsAC.Autotransformer(n1, n2, n3, elements.Count,k));
            return elements.Count - 1;
        }
        public int CreateSwitch(int n1, int n2,bool state)
        {
            elements.Add(new ElementsAC.Switch(n1, n2, elements.Count, state));
            return elements.Count - 1;
        }
        public string EquationGeneration(float frequency)
        {
            string equations = "";
            Dictionary<string, List<ElementsAC.CurrentFlowBlock>> nodeEquationsIn = new Dictionary<string, List<ElementsAC.CurrentFlowBlock>>();
            Dictionary<string, List<ElementsAC.CurrentFlowBlock>> nodeEquationsOut = new Dictionary<string, List<ElementsAC.CurrentFlowBlock>>();
            for (int i = 0; i < nodesList.Count; i++)
            {
                nodeEquationsIn.Add($"v_{i}_re", new List<ElementsAC.CurrentFlowBlock>());
                nodeEquationsIn.Add($"v_{i}_im", new List<ElementsAC.CurrentFlowBlock>());
                nodeEquationsOut.Add($"v_{i}_re", new List<ElementsAC.CurrentFlowBlock>());
                nodeEquationsOut.Add($"v_{i}_im", new List<ElementsAC.CurrentFlowBlock>());
            }
            foreach (var element in elements)
            {
                List<ElementsAC.EquationBlock> blocks = element.GenerateEquationsAC();
                foreach (var block in blocks)
                {
                    if (block is ElementsAC.CurrentFlowBlock)
                    {
                        ElementsAC.CurrentFlowBlock currentFlowBlock = (ElementsAC.CurrentFlowBlock)block;
                        if (currentFlowBlock.Node1 != null)
                            nodeEquationsIn[currentFlowBlock.Node1].Add(currentFlowBlock);
                        if (currentFlowBlock.Node2 != null)
                            nodeEquationsOut[currentFlowBlock.Node2].Add(currentFlowBlock);
                    }
                    else
                    {
                        equations += block.Equation+Environment.NewLine;
                    }
                }
            }
            equations += "//Current equations" + Environment.NewLine;
            for (int i = 0; i < nodesList.Count; i++)
            {
                var real = $"v_{i}_re";
                var im = $"v_{i}_im";
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
                if(nodeOutRe.Count+nodeInRe.Count>0)
                    equations += eqRe+ Environment.NewLine;
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
                    equations += eqIm+ Environment.NewLine;
            }
            foreach (var element in elements)
            {
                var blocks = element.GetParametersAC();
                foreach (var block in blocks)
                {
                    equations += block.Equation + Environment.NewLine;
                }
            }
            /*for (int i = 0; i < nodesList.Count; i++)
            {
                equations += $"v_{i}_re(0) = {1}\n";
                equations += $"v_{i}_im(0) = {0}\n";
            }*/
            equations += $"set frequency = {frequency.ToString(new CultureInfo("en-US"))} * 2 * pi();";
            return equations;
        }
        public ACGraphSolution SolveEquationsAC(float frequency)
        {
            string equations = EquationGeneration(frequency);
            //create solver
            EquationCompiler compiler = new EquationCompiler();
            NonlinearEquationDefinition compiledEquation = compiler.CompileEquations(equations);
            NonlinearSystemSymbolicAnalytic system = new NonlinearSystemSymbolicAnalytic(compiledEquation);
            //calc solution
            Vector<double> values = NewtonRaphsonSolver.Solve(
                system,
                Vector<double>.Build.DenseOfArray(compiledEquation.InitialValues),
                20,
                0.01,
                1.0
                );
            NonlinearSystemSolution solution = compiledEquation.GetSolution(values);

            ACGraphSolution acSolution = new ACGraphSolution();
            acSolution.frequency = frequency;

            for (int i = 0; i < nodesList.Count; i++)
            {
                var real = $"v_{i}_re";
                var im = $"v_{i}_im";
                acSolution.voltages.Add(new Complex32((float)solution.getValue(real),
                    (float)solution.getValue(im)));
            }
            foreach (var element in elements)
            {
                acSolution.currents.Add(element.GetCurrent(solution,frequency));
                acSolution.voltageDrops.Add(element.GetVoltageDrop(solution));
            }
            return acSolution;
            /*for (int i = 0; i < elements.Count; i++)
            {
                ElementsAC.Element element = elements[i];
                switch (element.ElementType)
                {
                    case ElementsAC.ElementTypeEnum.Capacitor:
                        {
                            Complex32 voltageDrop = solution.voltages[element.nodes[1]] - solution.voltages[element.nodes[0]];
                            Complex32 current = voltageDrop * new Complex32(0.0f, frequency * ((ElementsAC.Capacitor)element).capacity);
                            solution.currents.Add(current);
                            break;
                        }
                    case ElementsAC.ElementTypeEnum.Resistor:
                        {
                            Complex32 voltageDrop = solution.voltages[element.nodes[1]] - solution.voltages[element.nodes[0]];
                            solution.currents.Add(voltageDrop / ((ElementsAC.Resistor)element).resistance);
                            break;
                        }
                    case ElementsAC.ElementTypeEnum.CurrentSource:
                        ElementsAC.CurrentSource csel = (ElementsAC.CurrentSource)element;
                        solution.currents.Add(Complex32.FromPolarCoordinates(csel.current, csel.phase));
                        break;
                    case ElementsAC.ElementTypeEnum.Transformer2w:
                        solution.currents.Add(0.0f);
                        break;
                    case ElementsAC.ElementTypeEnum.Inductor:
                    case ElementsAC.ElementTypeEnum.Ground:
                    case ElementsAC.ElementTypeEnum.Line:
                    case ElementsAC.ElementTypeEnum.VoltageSource:
                        solution.currents.Add(0.0f);//Пропуск значений, поскольку данные токи присутствуют в векторе решения
                        break;
                }
            }*/
        }
    }
}

