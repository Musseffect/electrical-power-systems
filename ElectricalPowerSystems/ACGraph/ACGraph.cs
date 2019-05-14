using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.ACGraph
{
    class ACGraphSolution
    {
    }
    class ACGraph
    {
        public List<ElementsAC.Element> elements;
        public List<int> voltageSources;
        public List<int> currentSources;
        public List<int> inductors;
        public List<Node> nodesList;
        public List<int> lines;
        public int groundsCount;
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
        public List<NodePair> outputVoltageDifference;
        public List<int> outputNodeVoltage;
        public List<int> outputCurrent;
        public int createResistor(int node1, int node2, float resistance)
        {
            nodesList[node1].connectedElements.Add(elements.Count);
            nodesList[node2].connectedElements.Add(elements.Count);
            elements.Add(new ElementsAC.Resistor(node1, node2, resistance));
            return elements.Count - 1;
        }
        public int createLine(int node1, int node2)
        {
            nodesList[node1].connectedElements.Add(elements.Count);
            nodesList[node2].connectedElements.Add(elements.Count);
            elements.Add(new ElementsAC.Line(node1, node2));
            return elements.Count - 1;
        }
        public int createCapacitor(int node1, int node2, float capacity)
        {
            nodesList[node1].connectedElements.Add(elements.Count);
            nodesList[node2].connectedElements.Add(elements.Count);
            elements.Add(new ElementsAC.Capacitor(node1, node2, capacity));
            return elements.Count - 1;
        }
        public int createVoltageSource(int node1, int node2, float voltage, float phase, float freq)
        {
            nodesList[node1].connectedElements.Add(elements.Count);
            nodesList[node2].connectedElements.Add(elements.Count);
            voltageSources.Add(elements.Count);
            elements.Add(new ElementsAC.VoltageSource(node1, node2, voltage, phase, freq));
            return elements.Count - 1;
        }
        public int createCurrentSource(int node1, int node2, float current, float phase, float freq)
        {
            nodesList[node1].connectedElements.Add(elements.Count);
            nodesList[node2].connectedElements.Add(elements.Count);
            currentSources.Add(elements.Count);
            elements.Add(new ElementsAC.CurrentSource(node1, node2, current, phase, freq));
            return elements.Count - 1;
        }
        public void createGround(int node)
        {
            nodesList[node].connectedElements.Add(elements.Count);
            nodesList[node].grounded = true;
            elements.Add(new ElementsAC.Ground(node));
            groundsCount++;
        }
        public int createInductor(int node1, int node2, float inductivity)
        {
            nodesList[node1].connectedElements.Add(elements.Count);
            nodesList[node2].connectedElements.Add(elements.Count);
            elements.Add(new ElementsAC.Inductor(node1, node2, inductivity));
            return elements.Count - 1;
        }
        //returns list of Currents
        public List<string> SolveAC()
        {
            int _lines = lines.Count;
            int nodes = nodesList.Count - groundsCount;
            int _voltageSources = voltageSources.Count;
            int inductorsCount = inductors.Count;
            int rank = nodes + _voltageSources + inductorsCount;
            Matrix<Complex32> A = Matrix<Complex32>.Build.Dense(rank, rank);
            HashSet<float> frequencies = new HashSet<float>();
            foreach (var source in currentSources)
            {
                float freq = ((ElementsAC.CurrentSource)elements[source]).frequency;
                frequencies.Add(freq);
            }
            foreach (var source in voltageSources)
            {
                float freq = ((ElementsAC.VoltageSource)elements[source]).frequency;
                frequencies.Add(freq);
            }
            List<string> solution = new List<string>(nodes);
            for (int i = 0, k = 0; k < nodes; i++)
            {
                if (nodesList[i].grounded)
                {
                    continue;
                }
                solution.Add("Voltage for node " + nodesList[i].label + " is ");
                k++;
            }
            //solve circuit for each frequency
            foreach (float frequency in frequencies)
            {
                /*
                  | G B |
                  | C D |
                */
                //submatrix G
                for (int i = 0, k = 0; k < nodes; i++)
                {
                    if (nodesList[i].grounded == true)
                    {
                        continue;
                    }
                    for (int j = 0, l = 0; l < nodes; j++)
                    {
                        if (nodesList[j].grounded == true)
                        {
                            continue;
                        }
                        Complex32 value = new Complex32(0, 0);
                        if (k == l)
                        {
                            foreach (int elementId in nodesList[i].connectedElements)
                            {
                                ElementsAC.Element element = elements[elementId];
                                if (element is ElementsAC.Resistor)
                                {
                                    value += new Complex32(1.0f / ((ElementsAC.Resistor)element).resistance, 0.0f);
                                }
                                else if (element is ElementsAC.Capacitor)
                                {
                                    value += new Complex32(0.0f, frequency * ((ElementsAC.Capacitor)element).capacity);
                                }
                            }
                        }
                        else
                        {
                            foreach (int elementId in nodesList[i].connectedElements)
                            {
                                ElementsAC.Element element = elements[elementId];
                                if (element is ElementsAC.Resistor)
                                {
                                    ElementsAC.Resistor res = (ElementsAC.Resistor)element;
                                    if (res.nodes[0] == j || res.nodes[1] == j)
                                        value -= 1.0f / res.resistance;
                                }
                            }
                        }
                        A.At(l, k, value);
                        l++;
                    }
                    k++;
                }
                //submatrix B and C
                for (int i = 0; i < _voltageSources; i++)
                {
                    for (int j = 0, k = 0; k < nodes; j++)
                    {
                        if (nodesList[j].grounded == true)
                        {
                            continue;
                        }
                        ElementsAC.VoltageSource vs = ((ElementsAC.VoltageSource)(elements[voltageSources[i]]));
                        Complex32 value = new Complex32(0.0f, 0.0f);
                        if (vs.nodes[1] == j)
                        {
                            value = 1.0f;
                        }
                        else if (vs.nodes[0] == j)
                        {
                            value = -1.0f;
                        }
                        A.At(k, i + nodes, value);
                        A.At(i + nodes, k, value);
                        k++;
                    }
                }
                for (int i = 0; i < _lines; i++)
                {
                    for (int j = 0, k = 0; k < nodes; j++)
                    {
                        if (nodesList[j].grounded == true)
                        {
                            continue;
                        }
                        ElementsAC.Element inductor = elements[lines[i]];
                        Complex32 value = new Complex32(0.0f, 0.0f);
                        if (inductor.nodes[0] == k)
                            value = 1.0f;
                        else
                            value = -1.0f;
                        A.At(k, i + nodes + _voltageSources, value);
                        A.At(i + nodes + _voltageSources, k, value);
                        k++;
                    }
                }
                for (int i = 0; i < inductorsCount; i++)
                {
                    for (int j = 0, k = 0; k < nodes; j++)
                    {
                        if (nodesList[j].grounded == true)
                        {
                            continue;
                        }
                        ElementsAC.Element inductor = elements[inductors[i]];
                        Complex32 value = new Complex32(0.0f, 0.0f);
                        if (inductor.nodes[0] == k)
                            value = 1.0f;
                        else
                            value = -1.0f;
                        A.At(k, i + nodes + _voltageSources + _lines, value);
                        A.At(i + nodes + _voltageSources + _lines, k, value);
                        k++;
                    }
                }
                //submatrix D
                for (int i = 0; i < _voltageSources + _lines + inductorsCount; i++)
                {
                    for (int j = 0; j < _voltageSources + _lines + inductorsCount; j++)
                    {
                        A.At(nodes + j, nodes + i, 0.0f);
                    }
                }
                for (int i = 0; i < inductorsCount; i++)
                {
                    int index = nodes + _voltageSources + _lines + i;
                    ElementsAC.Inductor inductor = (ElementsAC.Inductor)elements[inductors[i]];
                    A.At(index, index, new Complex32(0.0f, inductor.inductivity));

                }
                Vector<Complex32> y = Vector<Complex32>.Build.Dense(rank); ;//size equal to
                for (int i = 0, j = 0; j < nodes; i++)
                {
                    if (nodesList[i].grounded == true)
                    {
                        continue;
                    }
                    Complex32 value = 0.0f;
                    foreach (int elId in nodesList[i].connectedElements)
                    {
                        ElementsAC.Element element = elements[elId];
                        if (element is ElementsAC.CurrentSource)
                        {
                            ElementsAC.CurrentSource cs = (ElementsAC.CurrentSource)element;
                            if (cs.frequency == frequency)
                            {
                                Complex32 current = Complex32.FromPolarCoordinates(cs.current,
                                  cs.phase);
                                if (((ElementsAC.CurrentSource)element).nodes[1] == i)
                                    value += current;
                                else
                                    value -= current;
                            }
                        }
                    }
                    y[j] = value;
                    j++;
                }
                for (int i = 0; i < _voltageSources; i++)
                {
                    ElementsAC.VoltageSource vs = ((ElementsAC.VoltageSource)(elements[voltageSources[i]]));
                    if (frequency == vs.frequency)
                    {
                        y[i + nodes] = Complex32.FromPolarCoordinates(vs.voltage, vs.phase);
                    }
                    else
                        y[i + nodes] = new Complex32(0.0f, 0.0f);
                }
                for (int i = 0; i < _lines + inductorsCount; i++)
                {
                    y[i + nodes + _voltageSources] = new Complex32(0.0f, 0.0f);
                }
                Vector<Complex32> x = A.Solve(y);
                for (int i = 0, k = 0; k < nodes; i++)
                {
                    if (nodesList[i].grounded)
                    {
                        continue;
                    }
                    solution[k] += (x[k].Magnitude.ToString() + "@" + MathUtils.degrees(x[k].Phase).ToString() + " w=" + frequency + " ");
                    k++;
                }
            }
            for (int i = 0, k = 0; k < nodes; i++)
            {
                if (nodesList[i].grounded)
                {
                    continue;
                }
                solution[k] += "V";
                k++;
            }
            return solution;
        }
        static public List<string> formOutput(ModelGraphCreatorAC graph, Vector<Complex32> x, float frequency)
        {
            List<string> output = new List<string>();
            int nodes = graph.nodesList.Count - graph.groundsCount;
            List<Complex32> elementsCurrents = new List<Complex32>();
            List<Complex32> nodesVoltages = new List<Complex32>();
            for (int i = 0, k = 0; i < graph.nodesList.Count; i++)
            {
                if (graph.nodesList[i].grounded)
                {
                    nodesVoltages.Add(new Complex32(0.0f, 0.0f));
                }
                else
                {
                    nodesVoltages.Add(x[k]);
                    k++;
                }
            }
            for (int i = 0; i < graph.elements.Count; i++)
            {
                ElementsAC.Element element = graph.elements[i];
                switch (element.ElementType)
                {
                    case ElementsAC.ElementTypeEnum.Capacitor:
                        {
                            Complex32 voltageDrop = nodesVoltages[element.nodes[1]] - nodesVoltages[element.nodes[0]];
                            Complex32 current = voltageDrop * new Complex32(0.0f, frequency * ((ElementsAC.Capacitor)element).capacity);
                            elementsCurrents.Add(current);
                            break;
                        }
                    case ElementsAC.ElementTypeEnum.Resistor:
                        {
                            Complex32 voltageDrop = nodesVoltages[element.nodes[1]] - nodesVoltages[element.nodes[0]];
                            elementsCurrents.Add(voltageDrop / ((ElementsAC.Resistor)element).resistance);
                            break;
                        }
                    case ElementsAC.ElementTypeEnum.Inductor:
                        {
                            Complex32 voltageDrop = nodesVoltages[element.nodes[1]] - nodesVoltages[element.nodes[0]];
                            Complex32 current = voltageDrop / new Complex32(0.0f, -frequency * ((ElementsAC.Inductor)element).inductivity);
                            elementsCurrents.Add(current);
                            break;
                        }
                    case ElementsAC.ElementTypeEnum.CurrentSource:
                        ElementsAC.CurrentSource csel = (ElementsAC.CurrentSource)element;
                        elementsCurrents.Add(Complex32.FromPolarCoordinates(csel.current, csel.phase));
                        break;
                    case ElementsAC.ElementTypeEnum.Ground:
                    case ElementsAC.ElementTypeEnum.Line:
                    case ElementsAC.ElementTypeEnum.VoltageSource:
                        elementsCurrents.Add(new Complex32());//empty 
                        break;
                }
            }
            int voltageSources = graph.voltageSources.Count;
            int inductorsCount = graph.inductors.Count;
            int lines = graph.lines.Count;
            for (int i = 0; i < voltageSources; i++)
            {
                Complex32 current = x[i + nodes];
                elementsCurrents[graph.voltageSources[i]] = current;
            }
            for (int i = 0; i < lines; i++)
            {
                Complex32 current = x[i + nodes + voltageSources];
                elementsCurrents[graph.lines[i]] = current;
            }
            for (int i = 0; i < inductorsCount; i++)
            {
                Complex32 current = x[i + nodes + voltageSources + lines];
                elementsCurrents[graph.inductors[i]] = current;
            }
            foreach (var element in graph.outputCurrent)
            {
                Complex32 current = elementsCurrents[element];
                output.Add($"Current [id={element}] = {current.Magnitude}@{MathUtils.degrees(current.Phase)}");
            }
            foreach (var element in graph.outputNodeVoltage)
            {
                ElementsAC.Element el = graph.elements[element];
                if (el is ElementsAC.Element2N)
                {
                    Complex32 voltageDrop = nodesVoltages[el.nodes[1]] - nodesVoltages[el.nodes[1]];
                    output.Add($"Voltage difference [id={element}] = {voltageDrop.Magnitude}@{MathUtils.degrees(voltageDrop.Phase)}");
                }
            }
            foreach (var nodePair in graph.outputVoltageDifference)
            {
                Complex32 diff = nodesVoltages[nodePair.node2] - nodesVoltages[nodePair.node1];
                output.Add($"Voltage difference [n1={graph.nodesList[nodePair.node1].label}, n2={graph.nodesList[nodePair.node2].label}] = " +
                    $"{diff.Magnitude}@{MathUtils.degrees(diff.Phase)}"
                    );
            }
            return output;
        }
}
}
