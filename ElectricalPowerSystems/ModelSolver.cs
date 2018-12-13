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
            int nodes=graph.nodesList.Count-graph.groundsCount;
            int voltageSources = graph.voltageSources.Count;
            int rank = nodes + voltageSources;
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
            for (int i = 0; i < voltageSources; i++)
            {
                for (int j = 0,k=0; k < nodes; j++)
                {
                    if (graph.nodesList[j].grounded == true)
                    {
                        continue;
                    }
                    VoltageSource vs = ((VoltageSource)(graph.elements[graph.voltageSources[i]]));
                    float value = 0.0f;
                    if (vs.nodes[1] == j)
                    {
                        value = 1.0f;
                    } else if (vs.nodes[0] == j)
                    {
                        value = -1.0f;
                    }
                    A.At(k,i + nodes, value);
                    k++;
                }
            }
            //C submatrix
            /*The C matrix is an MxM matrix with only 0, 1 and - 1 elements.
             * Each location in the matrix corresponds to a particular node(first dimension) or voltage source(second dimension).
             * If the positive terminal of the ith voltage source is connected to node k, then the element(i, k) in the C matrix is a 1.
             * If the negative terminal of the ith voltage source is connected to node k, then the element(i, k) in the C matrix is a - 1.
             * Otherwise, elements of the C matrix are zero.
                In other words, the C matrix is the transpose of the B matrix. 
                This is not the case when dependent sources are present.*/
            for (int i = 0,k=0; k < nodes; i++)
            {
                if (graph.nodesList[i].grounded == true)
                {
                    continue;
                }
                for (int j = 0; j < voltageSources; j++)
                {
                    A.At(j + nodes , k,A.At(k,j + nodes));
                }
                k++;
            }
            //D submatrix
            /*The D matrix is an MxM matrix that is composed entirely of zeros.
             * It can be non-zero if dependent sources are considered.
             */
            for (int i = 0; i < voltageSources; i++)
            {
                for (int j = 0; j < voltageSources; j++)
                {
                    A.At(nodes+j,nodes+i,0.0f);
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
            }
            //solve circuit for each frequency
            foreach (float frequency in frequencies)
            {
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
                        k++;
                    }
                }
                for (int i = 0; i < inductorsCount; i++)
                {
                    for (int j = 0, k = 0; k < nodes; j++)
                    {
                        ElementsAC.Element inductor = graph.elements[graph.inductors[i]];
                        Complex32 value = new Complex32(0.0f, 0.0f);
                        if (inductor.nodes[0] == k)
                            value = 1.0f;
                        else
                            value = -1.0f;
                        A.At(k, i+nodes+voltageSources, A.At(k, i + nodes + voltageSources));
                        k++;
                    }
                }
                for (int i = 0, k = 0; k < nodes; i++)
                {
                    if (graph.nodesList[i].grounded == true)
                    {
                        continue;
                    }
                    for (int j = 0; j < voltageSources; j++)
                    {
                        A.At(j + nodes, k, A.At(k, j + nodes));
                    }
                    for (int j = 0; j < inductorsCount; j++)
                    {
                        A.At(j+voltageSources + nodes, k, A.At(k, j + nodes+ voltageSources));
                    }
                    k++;
                }
                for (int i = 0; i < voltageSources; i++)
                {
                    for (int j = 0; j < voltageSources; j++)
                    {
                        A.At(nodes + j, nodes + i, 0.0f);
                    }
                }
                for (int i = 0; i < inductorsCount; i++)
                {
                    int index = nodes + voltageSources + i;
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
                    }
                }
                for (int i = 0; i < inductorsCount; i++)
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
                    solution[k]+=(x[k].Magnitude.ToString()+"@"+x[k].Phase.ToString()+" w="+frequency+" ");
                    k++;
                }
            }
            for (int i = 0, k = 0; k < nodes; i++)
            {
                if (graph.nodesList[i].grounded)
                {
                    continue;
                }
                solution.Add("V");
                k++;
            }
            return solution;
        }

    }
}
