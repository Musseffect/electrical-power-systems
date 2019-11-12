using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.ACGraph
{
    public class ACGraphSolution
    {
        public List<Complex32> voltages;//Комплексные напряжения
        public List<Complex32> currents;//Комплексные токи для всех элементов
        public float frequency;//Частота активных элементов схемы расчёта
        public ACGraphSolution()
        {
            voltages = new List<Complex32>();
            currents = new List<Complex32>();
            frequency = 0.0f;
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
        public int allocateNode()
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
            lines.Add(elements.Count);
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
            inductors.Add(elements.Count);
            elements.Add(new ElementsAC.Inductor(node1, node2, inductivity));
            return elements.Count - 1;
        }
        public int createTransformer(int n1, int n2, int n3, int n4, float k)
        {
            transformers.Add(elements.Count);
            elements.Add(new ElementsAC.Transformer(n1,n2,n3,n4,k));
            return elements.Count;
        }
        //returns list of Currents
        public ACGraphSolution SolveAC(float frequency)//решить схему для выбранной частоты
        {
            ACGraphSolution solution = new ACGraphSolution();
            solution.frequency = frequency;
            int _lines = lines.Count;
            int nodes = nodesList.Count;
            int _voltageSources = voltageSources.Count;
            int inductorsCount = inductors.Count;
            int transformersCount = transformers.Count;
            int voltageRank = nodes - groundsCount;
            int auxRank = _voltageSources + _lines + inductorsCount + 2 * transformersCount;
            int linesOffset = voltageRank + _voltageSources;
            int inductorsOffset = linesOffset + _lines;
            int transformersOffset = inductorsOffset + inductorsCount;
            int rank = voltageRank + auxRank;
            Matrix<Complex32> A = Matrix<Complex32>.Build.Dense(rank, rank);
            /*
                | G B |
                | C D |
            */
            //submatrix G
            for (int i = 0, k = 0; k < voltageRank; i++)
            {
                if (nodesList[i].grounded == true)
                {
                    continue;
                }
                for (int j = 0, l = 0; l < voltageRank; j++)
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
                for (int j = 0, k = 0; k < voltageRank; j++)
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
                    A.At(k, i + voltageRank, value);
                    A.At(i + voltageRank, k, value);
                    k++;
                }
            }
            for (int i = 0; i < _lines; i++)
            {
                for (int j = 0, k = 0; k < voltageRank; j++)
                {
                    if (nodesList[j].grounded == true)
                    {
                        continue;
                    }
                    ElementsAC.Element line = elements[lines[i]];
                    Complex32 value = new Complex32(0.0f, 0.0f);
                    if (line.nodes[0] == j)
                        value = 1.0f;
                    else if(line.nodes[1] == j)
                        value = -1.0f;
                    A.At(k, i + linesOffset, value);
                    A.At(i + linesOffset, k, value);
                    k++;
                }
            }
            for (int i = 0; i < inductorsCount; i++)
            {
                for (int j = 0, k = 0; k < voltageRank; j++)
                {
                    if (nodesList[j].grounded == true)
                    {
                        continue;
                    }
                    ElementsAC.Element inductor = elements[inductors[i]];
                    Complex32 value = new Complex32(0.0f, 0.0f);
                    if (inductor.nodes[0] == j)
                        value = 1.0f;
                    else if(inductor.nodes[1]==j)
                        value = -1.0f;
                    A.At(k, i + inductorsOffset, value);
                    A.At(i + inductorsOffset, k, value);
                    k++;
                }
            }
            for (int i = 0; i < transformersCount; i++)
            {
                for (int j = 0, k = 0; k < voltageRank; j++)
                {
                    if (nodesList[j].grounded == true)
                    {
                        continue;
                    }
                    ElementsAC.Element transformer = elements[transformers[i]];
                    Complex32 value1 = new Complex32(0.0f, 0.0f);
                    Complex32 value2 = new Complex32(0.0f, 0.0f);
                    Complex32 value3 = new Complex32(0.0f, 0.0f);
                    if (transformer.nodes[0] == j)
                    {
                        value1 = 1.0f;
                        value3 = -((ElementsAC.Transformer)transformer).b;
                    }
                    else if (transformer.nodes[1] == j)
                    {
                        value1 = -1.0f;
                        value3 = ((ElementsAC.Transformer)transformer).b;
                    }
                    else if (transformer.nodes[2] == j)
                    {
                        value2 = 1.0f;
                        value3 = 1.0f;
                    }
                    else if (transformer.nodes[3] == j)
                    {
                        value2 = -1.0f;
                        value3 = -1.0f;
                    }
                    A.At(i * 2 + transformersOffset, k, value1);
                    A.At(i * 2 + 1 + transformersOffset, k, value2);
                    A.At(k, i * 2 + 1 + transformersOffset, value3);
                    k++;
                }
            }
            //submatrix D
            for (int i = 0; i < _voltageSources + _lines + inductorsCount; i++)
            {
                for (int j = 0; j < _voltageSources + _lines + inductorsCount; j++)
                {
                    A.At(voltageRank + j, voltageRank + i, 0.0f);
                }
            }
            for (int i = 0; i < inductorsCount; i++)
            {
                int index = inductorsOffset + i;
                ElementsAC.Inductor inductor = (ElementsAC.Inductor)elements[inductors[i]];
                A.At(index, index, new Complex32(0.0f, -inductor.inductivity*frequency));

            }
            for (int i = 0; i < transformersCount; i++)
            {
                ElementsAC.Element transformer = elements[transformers[i]];
                float b = ((ElementsAC.Transformer)transformer).b;
                A.At(i * 2 +transformersOffset,i*2+transformersOffset, 1.0f);
                A.At(i * 2 + 1 + transformersOffset, i * 2 + transformersOffset,-b);
            }
            Vector<Complex32> y = Vector<Complex32>.Build.Dense(rank); ;//size equal to
            for (int i = 0, j = 0; j < voltageRank; i++)
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
                    y[i + voltageRank] = Complex32.FromPolarCoordinates(vs.voltage, vs.phase);
                }
                else
                    y[i + voltageRank] = new Complex32(0.0f, 0.0f);
            }
            for (int i = 0; i < _lines + inductorsCount + transformersCount * 2; i++)
            {
                y[i + linesOffset] = new Complex32(0.0f, 0.0f);
            }
            //Решение системы
            Vector<Complex32> x = A.Solve(y);
            //Напряжения(Потенциалы) в узлах
            for (int i = 0, k = 0; i<nodes; i++)
            {
                if (nodesList[i].grounded)
                {
                    solution.voltages.Add(0.0f);
                    continue;
                }
                if (k >= voltageRank)
                    break;
                solution.voltages.Add(x[k]);
                k++;
            }
            for (int i = 0; i < elements.Count; i++)
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
                    case ElementsAC.ElementTypeEnum.Transformer:
                        solution.currents.Add(0.0f);
                        break;
                    case ElementsAC.ElementTypeEnum.Inductor:
                    case ElementsAC.ElementTypeEnum.Ground:
                    case ElementsAC.ElementTypeEnum.Line:
                    case ElementsAC.ElementTypeEnum.VoltageSource:
                        solution.currents.Add(0.0f);//Пропуск значений, поскольку данные токи присутствуют в векторе решения
                        break;
                }
            }
            for (int i = 0; i < _lines; i++)
            {
                solution.currents[lines[i]] = x[linesOffset + i];
            }
            for (int i = 0; i < _voltageSources; i++)
            {
                solution.currents[voltageSources[i]]=x[voltageRank + i];
            }
            for (int i = 0; i<inductorsCount; i++)
            {
                solution.currents[inductors[i]] = x[inductorsOffset + i];
            }
            for (int i = 0; i < transformersCount; i++)
            {
                solution.currents[transformers[i]] = x[transformersCount + i*2];
            }
            return solution;
        }
    }
}

