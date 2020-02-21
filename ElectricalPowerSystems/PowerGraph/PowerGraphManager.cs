using ElectricalPowerSystems.Interpreter.Equations.Nonlinear;
using MathNet.Numerics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerGraph
{
    public class PowerGraphManager
    {
        /// <summary>
        /// Частота системы в герцах
        /// </summary>
        public static float powerFrequency = (float)(50.0);
        public class ABCValue
        {
            public Complex32 A { get { return abc[0]; } set { abc[0] = value; } }
            public Complex32 B { get { return abc[1]; } set { abc[1] = value; } }
            public Complex32 C { get { return abc[2]; } set { abc[2] = value; } }
            private Complex32[] abc = new Complex32[3];
            public Complex32 get(int index)
            {
                return abc[index];
            }
            public void set(Complex32 value, int index)
            {
                abc[index] = value;
            }
        }
        public class PowerGraphSolveResult
        {
            ///<summary>Мощность для элемента</summary>
            public List<Complex32> powers;
            ///<summary>Потенциалы узлов в формате ABC относительно земли</summary>
            public List<ABCValue> nodeVoltages;
            public List<string> meterData;
            public PowerGraphSolveResult()
            {
                powers = new List<Complex32>();
                nodeVoltages = new List<ABCValue>();
                meterData = new List<string>();
            }
        }
        public class PowerModelElement
        {
            public GraphElement element;
            public List<int> nodes;
            public PowerModelElement()
            {
                nodes = new List<int>();
            }
        }
        public class PowerGraphModel
        {
            struct NodeIdPair
            {
                public int elementId;
                public int nodeLocalId;
                public NodeIdPair(int elId, int nLId)
                {
                    elementId = elId;
                    nodeLocalId = nLId;
                }
            }
            ACGraph.ACGraph acGraph;
            List<PowerElementScheme> elementsSchemes;
            List<PowerModelElement> elements;
            //List<ABCNode> nodes;
            PowerGraphManager managerRef;
            List<ABCNode> abcNodes;
            Dictionary<string, int> nodes;
            List<MeterScheme> meters;
            List<string> nodeNames;
            public PowerModelElement getElement(int elementId)
            {
                return elements[elementId];
            }
            public string GenerateEquations()
            {
                return acGraph.EquationGeneration(powerFrequency);
            }
            public PowerGraphModel(PowerGraphManager manager)
            {
                managerRef = manager;
                acGraph = new ACGraph.ACGraph();
                abcNodes = new List<ABCNode>();
                meters = new List<MeterScheme>();
                elements = new List<PowerModelElement>(manager.elements.Count);
                nodes = new Dictionary<string, int>();
                List<List<NodeIdPair>> nodeElements = new List<List<NodeIdPair>>();//Elements, connected to node
                elementsSchemes = new List<PowerElementScheme>();
                nodeNames = new List<string>();
                int nodeId = 0;
                int elementId = 0;
                //создать список всех ABC узлов
                foreach (var element in manager.elements)
                {
                    PowerModelElement el = new PowerModelElement();
                    int nodeLocalId = 0;
                    foreach (var node in element.Nodes)
                    {
                        int id;
                        if (!nodes.ContainsKey(node))
                        {
                            id = nodeId;
                            nodes.Add(node, nodeId);
                            ABCNode abcNode = new ABCNode();
                            abcNode.A = acGraph.allocateNode();
                            abcNode.B = acGraph.allocateNode();
                            abcNode.C = acGraph.allocateNode();
                            abcNodes.Add(abcNode);
                            nodeNames.Add(node);
                            nodeId++;
                        }
                        else
                        {
                            id = nodes[node];
                        }
                        el.nodes.Add(id);
                        nodeLocalId++;
                    }
                    el.element = element;
                    elementId++;
                    elements.Add(el);
                }
                //create abcn nodes
                List<ABCElement> abcElements = new List<ABCElement>();
                BitArray visitedElements = new BitArray(elements.Count, false);
                int i = 0;
                foreach (PowerModelElement element in elements)
                {
                    ABCElement abcElement = new ABCElement(
                        element.element);
                    abcElements.Add(abcElement);
                    int nodeCount = element.nodes.Count;
                    //for each node generate indexes for abcn nodes
                    for (int j = 0; j < nodeCount; j++)
                    {
                        int abcNodeIndex = element.nodes[j]; 
                        ABCNode abcNode = abcNodes[abcNodeIndex];
                        abcElement.Nodes.Add(abcNode);
                    }
                    //generate local electric scheme for element
                    PowerElementScheme scheme;
                    elementsSchemes.Add(scheme = abcElement.getElementDescription().generateACGraph(abcElement.Nodes, acGraph));
                    if (scheme is MeterScheme)
                    {
                        meters.Add(scheme as MeterScheme);
                    }
                    i++;
                }
                //если в схеме нет заземлений, то необходимо добавить одно заземление куда-нибудь для коректного расчёта
                if (acGraph.groundsCount == 0)
                {
                    acGraph.createGround(0);
                }
            }
            public int getNodeId(string label)
            {
                if (nodes.ContainsKey(label))
                {
                    return nodes[label];
                }
                throw new Exception($"Node {label} doesn't exist.");
            }
            public string getNodeName(int id)
            {
                return nodeNames[id];
            }
            public bool validate(ref List<string> errors)
            {
                throw new NotImplementedException();
                return true;
            }
            public PowerGraphSolveResult solve()//in phase coordinates
            {
                ACGraph.ACGraphSolution acSolution = acGraph.solveEquationsAC(powerFrequency);
                PowerGraphSolveResult result = new PowerGraphSolveResult();
                for (int i = 0; i < managerRef.elements.Count; i++)
                {
                    PowerElementScheme abcElement = elementsSchemes[i];
                    result.powers.Add(abcElement.ComputePower(acSolution));
                }
                foreach (var node in abcNodes)
                {
                    ABCValue phaseVoltages = new ABCValue();
                    phaseVoltages.A = acSolution.voltages[node.A];
                    phaseVoltages.B = acSolution.voltages[node.B];
                    phaseVoltages.C = acSolution.voltages[node.C];
                    result.nodeVoltages.Add(phaseVoltages);
                }
                foreach (var meter in meters)
                {
                    MeterValues values = meter.GetValues(acSolution);
                    result.meterData.Add($"Meter {meter.label}: ");
                    result.meterData.Add($"\tPower A: {values.PowerA}");
                    result.meterData.Add($"\tPower B: {values.PowerB}");
                    result.meterData.Add($"\tPower C: {values.PowerC}");
                                           
                    result.meterData.Add($"\tVoltage A: {values.VoltageA}");
                    result.meterData.Add($"\tVoltage B: {values.VoltageB}");
                    result.meterData.Add($"\tVoltage C: {values.VoltageC}");
                                           
                    result.meterData.Add($"\tVoltage AB: {values.VoltageAB}");
                    result.meterData.Add($"\tVoltage BC: {values.VoltageBC}");
                    result.meterData.Add($"\tVoltage CA: {values.VoltageCA}");
                                           
                    result.meterData.Add($"\tCurrent A: {values.CurrentA}");
                    result.meterData.Add($"\tCurrent B: {values.CurrentB}");
                    result.meterData.Add($"\tCurrent C: {values.CurrentC}");
                }
                return result;
            }
        }
        List<GraphElement> elements;
        List<GraphOutput> outputs;
        public PowerGraphManager()
        {
            elements = new List<GraphElement>();
            outputs = new List<GraphOutput>();
        }
        public void clear()
        {
            elements.Clear();
        }
        public int addElement(GraphElement element)
        {
            elements.Add(element);
            return elements.Count - 1;
        }
        public void addOutput(GraphOutput output)
        {
            outputs.Add(output);
        }
        public string TestEquationGeneration(bool useCompiledEquation = false)
        {
            PowerGraphModel model = new PowerGraphModel(this);
            string equations = model.GenerateEquations();
            string result = equations + Environment.NewLine;
            if (useCompiledEquation)
            {
                EquationCompiler compiler = new EquationCompiler();
                NonlinearEquationDefinition compiledEquation = compiler.CompileEquations(equations);
                result += compiledEquation.PrintVariables() + Environment.NewLine;
                result += compiledEquation.PrintEquations() + Environment.NewLine;
                result += compiledEquation.PrintJacobiMatrix() + Environment.NewLine;
            }
            return result;
        }
        public void solve(ref List<string> errors,ref List<string> output)
        {
            //build scheme
            PowerGraphModel model = new PowerGraphModel(this);
            //validate
            /*if (!model.validate(ref errors))
            {
                return;
            }*/
            //solve
            PowerGraphSolveResult result = model.solve();
            for (int i = 0; i < outputs.Count; i++)
            {
                output.Add(outputs[i].generate(model,result));
            }
            for (int i = 0; i < result.meterData.Count; i++)
            {
                output.Add(result.meterData[i]);
            }
            //get values
            //return values
        }
    }
}
