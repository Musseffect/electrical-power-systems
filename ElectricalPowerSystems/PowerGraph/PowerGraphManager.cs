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
            public List<ABCValue> powers;
            ///<summary>Потенциалы узлов в формате ABC относительно земли</summary>
            public List<ABCValue> nodeVoltages;
            ///<summary>Исходящий ток для узлов элемента</summary>
            public List<ABCValue[]> currents;
            public PowerGraphSolveResult()
            {
                powers = new List<ABCValue>();
                nodeVoltages = new List<ABCValue>();
                currents = new List<ABCValue[]>();
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
                elements = new List<PowerModelElement>(manager.elements.Count);
                nodes = new Dictionary<string, int>();
                List<List<NodeIdPair>> nodeElements = new List<List<NodeIdPair>>();//Elements, connected to node
                elementsSchemes = new List<PowerElementScheme>();
                nodeNames = new List<string>();
                int nodeId = 0;
                int elementId = 0;
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
                            abcNodes.Add(null);
                            nodeElements.Add(new List<NodeIdPair>());
                            nodeElements[nodeId].Add(new NodeIdPair(elementId, nodeLocalId));
                            nodeNames.Add(node);
                            nodeId++;
                        }
                        else
                        {
                            id = nodes[node];
                            nodeElements[id].Add(new NodeIdPair(elementId, nodeLocalId));
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
                    int counter = 0;
                    //for each node generate indexes for abcn nodes
                    for (int j = 0; j < nodeCount; j++)
                    {
                        ABCNode abcNode = new ABCNode();
                        abcNode.A = acGraph.allocateNode();
                        abcNode.B = acGraph.allocateNode();
                        abcNode.C = acGraph.allocateNode();
                        abcElement.Nodes.Add(abcNode);
                        abcNodes[element.nodes[counter]] = abcNode;
                        counter++;
                    }
                    //generate local electric scheme for element
                    elementsSchemes.Add(abcElement.getElementDescription().generateACGraph(abcElement.Nodes, acGraph));
                    i++;
                }
                foreach (var nodeList in nodeElements)
                {
                    ABCElement firstElement = abcElements[nodeList[0].elementId];
                    ABCNode firstABCNode = firstElement.Nodes[nodeList[0].nodeLocalId];
                    for (int j = 1; j < nodeList.Count; j++)
                    {
                        ABCElement currentElement = abcElements[nodeList[j].elementId];
                        ABCNode currentABCNode = currentElement.Nodes[nodeList[j].nodeLocalId];
                        //create line between firstElement and this node
                        acGraph.createLine(firstABCNode.A, currentABCNode.A);
                        acGraph.createLine(firstABCNode.B, currentABCNode.B);
                        acGraph.createLine(firstABCNode.C, currentABCNode.C);
                    }
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
                    abcElement.calcResults(ref result, acSolution);
                }
                foreach (var node in abcNodes)
                {
                    ABCValue phaseVoltages = new ABCValue();
                    phaseVoltages.A = acSolution.voltages[node.A];
                    phaseVoltages.B = acSolution.voltages[node.B];
                    phaseVoltages.C = acSolution.voltages[node.C];
                    result.nodeVoltages.Add(phaseVoltages);
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
        public string TestEquationGeneration()
        {
            PowerGraphModel model = new PowerGraphModel(this);
            return model.GenerateEquations();
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
            //get values
            //return values
        }
    }
}
