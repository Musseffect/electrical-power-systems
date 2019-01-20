using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;

namespace ElectricalPowerSystems
{
    class ModelSolver
    {
        static public List<string> Solve(ModelGraphCreator graph)
        {
            //http://qucs.sourceforge.net/tech/node14.html
            //generate matrix and two vectors than solve
            int lines = graph.lines.Count;
            int inductors=graph.inductors.Count;
            int nodes=graph.nodesList.Count-graph.groundsCount;
            int voltageSources = graph.voltageSources.Count;
            int rank = nodes + voltageSources+lines+inductors + lines;
            Vector<float> y= Vector<float>.Build.Dense(rank); ;//size equal to
            for (int i = 0, j = 0; j < nodes; i++)
            {
                if (graph.nodesList[i].grounded == true)
                {
                    continue;
                }
                float value = 0.0f;
                foreach (int elId in graph.nodesList[i].connectedElements)
                {
                    Element element = graph.elements[elId];
                    if (element is CurrentSource)
                    {
                        float current=((CurrentSource)element).current;
                        if (((CurrentSource)element).nodes[1] == i)
                            value += current;
                        else
                            value -= current;
                    }

                }
                y[j] = value;
                j++;
            }
            for (int i = 0; i < voltageSources; i++)
            {
                Element el = graph.elements[graph.voltageSources[i]];
                y[i+nodes] = ((VoltageSource)(graph.elements[graph.voltageSources[i]])).voltage;
            }
            Matrix<float> A=Matrix<float>.Build.Dense(rank,rank);
            /*
             |G B|
             |C D|
             */
            //G submatrix
            /*The G matrix is an NxN matrix formed in two steps.
            Each element in the diagonal matrix is equal to the sum of the conductance (one over the resistance) of each element connected to the corresponding node. 
            So the first diagonal element is the sum of conductances connected to node 1, the second diagonal element is the sum of conductances connected to node 2, and so on.
            The off diagonal elements are the negative conductance of the element connected to the pair of corresponding node. 
            Therefore a resistor between nodes 1 and 2 goes into the G matrix at location (1,2) and locations (2,1).
            If an element is grounded, it will only have contribute to one entry in the G matrix - at the appropriate location on the diagonal. 
            If it is ungrounded it will contribute to four entries in the matrix - two diagonal entries (corresponding to the two nodes) and two off-diagonal entries.
             */
            for (int i = 0,k=0; k < nodes; i++)
            {
                if (graph.nodesList[i].grounded == true)
                {
                    continue;
                }
                for (int j = 0,l=0; l< nodes; j++)
                {
                    if (graph.nodesList[j].grounded == true)
                    {
                        continue;
                    }
                    float value=0.0f;
                    if (k == l)
                    {
                        foreach (int elementId in graph.nodesList[i].connectedElements)
                        {
                            Element element = graph.elements[elementId];
                            if (element is Resistor)
                            {
                                value += 1.0f / ((Resistor)element).resistance;
                            }
                        }
                    }
                    else
                    {
                        foreach (int elementId in graph.nodesList[i].connectedElements)
                        {
                            Element element = graph.elements[elementId];
                            if (element is Resistor)
                            {
                                Resistor res = (Resistor)element;
                                if(res.nodes[0]==j||res.nodes[1]==j)
                                    value -= 1.0f / res.resistance;
                            }
                        }
                    }
                    A.At(l,k,value);
                    l++;
                }
                k++;
            }
            //b submatrix
            /*The B matrix is an NxM matrix with only 0, 1 and -1 elements. 
             *Each location in the matrix corresponds to a particular voltage source (first dimension) or a node (second dimension).
             * If the positive terminal of the ith voltage source is connected to node k, then the element (k,i) in the B matrix is a 1.
             * If the negative terminal of the ith voltage source is connected to node k, then the element (k,i) in the B matrix is a -1.
             * Otherwise, elements of the B matrix are zero.
                If a voltage source is ungrounded, it will have two elements in the B matrix (a 1 and a -1 in the same column). 
                If it is grounded it will only have one element in the matrix.
             */
            //C submatrix
            /*The C matrix is an MxM matrix with only 0, 1 and - 1 elements.
             * Each location in the matrix corresponds to a particular node(first dimension) or voltage source(second dimension).
             * If the positive terminal of the ith voltage source is connected to node k, then the element(i, k) in the C matrix is a 1.
             * If the negative terminal of the ith voltage source is connected to node k, then the element(i, k) in the C matrix is a - 1.
             * Otherwise, elements of the C matrix are zero.
                In other words, the C matrix is the transpose of the B matrix. 
                This is not the case when dependent sources are present.*/
            for (int i = 0; i < voltageSources; i++)
            {
                for (int j = 0,k=0; k < nodes; j++)
                {
                    if (graph.nodesList[j].grounded == true)
                    {
                        continue;
                    }
                    Element vs = (graph.elements[graph.voltageSources[i]]);
                    float value = 0.0f;
                    if (vs.nodes[1] == j)
                    {
                        value = 1.0f;
                    } else if (vs.nodes[0] == j)
                    {
                        value = -1.0f;
                    }
                    A.At(k,i + nodes, value);
                    A.At(i + nodes, k, value);
                    k++;
                }
            }
            for (int i = 0; i < lines; i++)
            {
                for (int j = 0, k = 0; k < nodes; j++)
                {
                    if (graph.nodesList[j].grounded == true)
                    {
                        continue;
                    }
                    Element vs = (graph.elements[graph.lines[i]]);
                    float value = 0.0f;
                    if (vs.nodes[1] == j)
                    {
                        value = 1.0f;
                    }
                    else if (vs.nodes[0] == j)
                    {
                        value = -1.0f;
                    }
                    A.At(k, i + nodes + voltageSources, value);
                    A.At(i + nodes + voltageSources, k, value);
                    k++;
                }
            }
            for (int i = 0; i < inductors; i++)
            {
                for (int j = 0, k = 0; k < nodes; j++)
                {
                    if (graph.nodesList[j].grounded == true)
                    {
                        continue;
                    }
                    Element vs = (graph.elements[graph.inductors[i]]);
                    float value = 0.0f;
                    if (vs.nodes[1] == j)
                    {
                        value = 1.0f;
                    }
                    else if (vs.nodes[0] == j)
                    {
                        value = -1.0f;
                    }
                    A.At(k, i + nodes + voltageSources + lines, value);
                    A.At(i + nodes + voltageSources + lines, k, value);
                    k++;
                }
            }
            //D submatrix
            /*The D matrix is an MxM matrix that is composed entirely of zeros.
             * It can be non-zero if dependent sources are considered.
             */
            for (int i = 0; i < voltageSources + lines + inductors; i++)
            {
                for (int j = 0; j < voltageSources + lines + inductors; j++)
                {
                    A.At(nodes + j, nodes + i, 0.0f);
                }
            }
            Vector<float> x=A.Solve(y);
            List<string> solution=new List<string>();
            for (int i = 0,k=0; k < nodes; i++)
            {
                if (graph.nodesList[i].grounded)
                {
                    continue;
                }
                solution.Add("Voltage for node " + graph.nodesList[i].label + " is " + x[k].ToString() +" V");
                k++;
            }
            return solution;
        }
        //returns list of Currents
        static public List<string> SolveAC(ModelGraphCreatorAC graph)
        {
            int lines = graph.lines.Count;
            int nodes = graph.nodesList.Count - graph.groundsCount;
            int voltageSources = graph.voltageSources.Count;
            int inductorsCount = graph.inductors.Count;
            int rank = nodes + voltageSources+inductorsCount;
            Matrix<Complex32> A = Matrix<Complex32>.Build.Dense(rank, rank);
            HashSet<float> frequencies = new HashSet<float>();
            foreach (var source in graph.currentSources)
            {
                float freq=((ElementsAC.CurrentSource)graph.elements[source]).frequency;
                frequencies.Add(freq);
            }
            foreach (var source in graph.voltageSources)
            {
                float freq = ((ElementsAC.VoltageSource)graph.elements[source]).frequency;
                frequencies.Add(freq);
            }
            List<string> solution = new List<string>(nodes);
            for (int i = 0, k = 0; k < nodes; i++)
            {
                if (graph.nodesList[i].grounded)
                {
                    continue;
                }
                solution.Add("Voltage for node " + graph.nodesList[i].label + " is ");
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
                    if (graph.nodesList[i].grounded == true)
                    {
                        continue;
                    }
                    for (int j = 0, l = 0; l < nodes; j++)
                    {
                        if (graph.nodesList[j].grounded == true)
                        {
                            continue;
                        }
                        Complex32 value = new Complex32(0,0);
                        if (k == l)
                        {
                            foreach (int elementId in graph.nodesList[i].connectedElements)
                            {
                                ElementsAC.Element element = graph.elements[elementId];
                                if (element is ElementsAC.Resistor)
                                {
                                    value += new Complex32(1.0f / ((ElementsAC.Resistor)element).resistance,0.0f);
                                }
                                else if (element is ElementsAC.Capacitor)
                                {
                                    value += new Complex32(0.0f,frequency*((ElementsAC.Capacitor)element).capacity);
                                }
                            }
                        }
                        else
                        {
                            foreach (int elementId in graph.nodesList[i].connectedElements)
                            {
                                ElementsAC.Element element = graph.elements[elementId];
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
                for (int i = 0; i < voltageSources; i++)
                {
                    for (int j = 0, k = 0; k < nodes; j++)
                    {
                        if (graph.nodesList[j].grounded == true)
                        {
                            continue;
                        }
                        ElementsAC.VoltageSource vs = ((ElementsAC.VoltageSource)(graph.elements[graph.voltageSources[i]]));
                        Complex32 value = new Complex32(0.0f,0.0f);
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
                for (int i = 0; i < lines; i++)
                {
                    for (int j = 0, k = 0; k < nodes; j++)
                    {
                        if (graph.nodesList[j].grounded == true)
                        {
                            continue;
                        }
                        ElementsAC.Element inductor = graph.elements[graph.lines[i]];
                        Complex32 value = new Complex32(0.0f, 0.0f);
                        if (inductor.nodes[0] == k)
                            value = 1.0f;
                        else
                            value = -1.0f;
                        A.At(k, i + nodes + voltageSources, value);
                        A.At(i + nodes + voltageSources, k, value);
                        k++;
                    }
                }
                for (int i = 0; i < inductorsCount; i++)
                {
                    for (int j = 0, k = 0; k < nodes; j++)
                    {
                        if (graph.nodesList[j].grounded == true)
                        {
                            continue;
                        }
                        ElementsAC.Element inductor = graph.elements[graph.inductors[i]];
                        Complex32 value = new Complex32(0.0f, 0.0f);
                        if (inductor.nodes[0] == k)
                            value = 1.0f;
                        else
                            value = -1.0f;
                        A.At(k, i + nodes + voltageSources + lines, value);
                        A.At(i + nodes + voltageSources + lines, k, value);
                        k++;
                    }
                }
                //submatrix D
                for (int i = 0; i < voltageSources+lines + inductorsCount; i++)
                {
                    for (int j = 0; j < voltageSources + lines + inductorsCount; j++)
                    {
                        A.At(nodes + j, nodes + i, 0.0f);
                    }
                }
                for (int i = 0; i < inductorsCount; i++)
                {
                    int index = nodes + voltageSources + lines + i;
                    ElementsAC.Inductor inductor = (ElementsAC.Inductor)graph.elements[graph.inductors[i]];
                    A.At(index, index, new Complex32(0.0f, inductor.inductivity));

                }
                Vector<Complex32> y = Vector<Complex32>.Build.Dense(rank); ;//size equal to
                for (int i = 0, j = 0; j < nodes; i++)
                {
                    if (graph.nodesList[i].grounded == true)
                    {
                        continue;
                    }
                    Complex32 value = 0.0f;
                    foreach (int elId in graph.nodesList[i].connectedElements)
                    {
                        ElementsAC.Element element = graph.elements[elId];
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
                for (int i = 0; i < voltageSources; i++)
                {
                    ElementsAC.VoltageSource vs = ((ElementsAC.VoltageSource)(graph.elements[graph.voltageSources[i]]));
                    if (frequency == vs.frequency)
                    {
                        y[i + nodes] = Complex32.FromPolarCoordinates(vs.voltage,vs.phase);
                    }else
                        y[i + nodes] = new Complex32(0.0f,0.0f);
                }
                for (int i = 0; i < lines + inductorsCount; i++)
                {
                    y[i + nodes+voltageSources] = new Complex32(0.0f,0.0f);
                }
                Vector<Complex32> x = A.Solve(y);
                for (int i = 0, k = 0; k < nodes; i++)
                {
                    if (graph.nodesList[i].grounded)
                    {
                        continue;
                    }
                    solution[k]+=(x[k].Magnitude.ToString()+"@"+ MathUtils.degrees(x[k].Phase).ToString()+" w="+frequency+" ");
                    k++;
                }
            }
            for (int i = 0, k = 0; k < nodes; i++)
            {
                if (graph.nodesList[i].grounded)
                {
                    continue;
                }
                solution[k]+="V";
                k++;
            }
            return solution;
        }
        static public List<string> formOutput(ModelGraphCreatorAC graph,Vector<Complex32> x,float frequency)
        {
            List<string> output = new List<string>();
            int nodes = graph.nodesList.Count - graph.groundsCount;
            List<Complex32> elementsCurrents=new List<Complex32>();
            List<Complex32> nodesVoltages=new List<Complex32>();
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
                elementsCurrents[graph.voltageSources[i]]=current;
            }
            for (int i = 0; i < lines; i++)
            {
                Complex32 current = x[i + nodes+voltageSources];
                elementsCurrents[graph.lines[i]] = current;
            }
            for (int i = 0; i < inductorsCount; i++)
            {
                Complex32 current = x[i + nodes + voltageSources+lines];
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
