using ElectricalPowerSystems.Interpreter.Equations.Nonlinear;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//https://ptolemy.berkeley.edu/projects/embedded/eecsx44/fall2011/lectures/01-modelling.pdf
namespace ElectricalPowerSystems.ACGraph
{
    public class CircuitModelAC
    {
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
        List<int> outputCurrent;
        List<NodePair> outputVoltageDifference;
        List<int> outputNodeVoltage;
        HashSet<float> frequencies;

        Dictionary<string, int> nodes;
        public List<string> nodeLabels;
        ACGraph acGraph;
        List<ErrorMessage> errors;
        public CircuitModelAC()
        {
            acGraph = new ACGraph();
            frequencies = new HashSet<float>();
            nodes = new Dictionary<string, int>();
            nodeLabels = new List<string>();
            outputVoltageDifference = new List<NodePair>();
            outputCurrent = new List<int>();
            outputNodeVoltage = new List<int>();
            errors = new List<ErrorMessage>();
        }
        private int retrieveNodeId(string key)
        {
            int node = acGraph.nodesList.Count;
            try
            {
                node = nodes[key];
            }
            catch (KeyNotFoundException)
            {
                nodes.Add(key, node);
                acGraph.allocateNode();
                nodeLabels.Add(key);
            }
            return node;
        }
        public int addTransformer(string node1, string node2, string node3, string node4, float k)
        {
            int node1Id = retrieveNodeId(node1);
            int node2Id = retrieveNodeId(node2);
            int node3Id = retrieveNodeId(node3);
            int node4Id = retrieveNodeId(node4);
            return acGraph.createTransformer(node1Id, node2Id, node3Id, node4Id,k);
        }
        public int addResistor(string node1, string node2, float resistance)
        {
            int node1Id = retrieveNodeId(node1);
            int node2Id = retrieveNodeId(node2);
            return acGraph.createResistor(node1Id,node2Id,resistance);
        }
        public int addLine(string node1, string node2)
        {
            int node1Id = retrieveNodeId(node1);
            int node2Id = retrieveNodeId(node2);
            return acGraph.createLine(node1Id,node2Id);
        }
        public int addCapacitor(string node1, string node2, float capacity)
        {
            int node1Id = retrieveNodeId(node1);
            int node2Id = retrieveNodeId(node2);
            return acGraph.createCapacitor(node1Id, node2Id, capacity);
        }
        //частота в герцах
        public int addVoltageSource(string node1, string node2, float voltage, float phase, float freq)
        {
            frequencies.Add(freq);
            int node1Id = retrieveNodeId(node1);
            int node2Id = retrieveNodeId(node2);
            return acGraph.createVoltageSource(node1Id, node2Id, voltage,phase,freq);
        }
        //частота в герцах
        public int addCurrentSource(string node1, string node2, float current, float phase, float freq)
        {
            frequencies.Add(freq);
            int node1Id = retrieveNodeId(node1);
            int node2Id = retrieveNodeId(node2);
            return acGraph.createCurrentSource(node1Id, node2Id, current, phase, freq);
        }
        public void addGround(string node)
        {
            int nodeId = retrieveNodeId(node);
            acGraph.createGround(nodeId);
        }
        public int addInductor(string node1, string node2, float inductivity)
        {
            int node1Id = retrieveNodeId(node1);
            int node2Id = retrieveNodeId(node2);
            return acGraph.createInductor(node1Id,node2Id,inductivity);
        }
        public void addCurrentOutput(int elementIndex)
        {
            if (elementIndex >= acGraph.elements.Count ||elementIndex<0)
                throw new Exception("Incorrect element index.");
            outputCurrent.Add(elementIndex);
        }
        public void addVoltageOutput(int elementIndex)
        {
            if (elementIndex >= acGraph.elements.Count || elementIndex < 0)
                throw new Exception("Incorrect element index.");
            outputNodeVoltage.Add(elementIndex);
        }
        public void addVoltageOutput(string node1, string node2)
        {
            int node1Id = retrieveNodeId(node1);
            int node2Id = retrieveNodeId(node2);
            outputVoltageDifference.Add(new NodePair(node1Id, node2Id));
        }
        public string testEquationGeneration(bool useCompiledEquation = false)
        {
            string result = "";
            foreach (float frequency in frequencies)
            {
                float hz = (float)(frequency);
                string equations = acGraph.EquationGeneration(frequency);
                result += equations + Environment.NewLine;
                if (useCompiledEquation)
                {
                    EquationCompiler compiler = new EquationCompiler();
                    NonlinearEquationDefinition compiledEquation = compiler.CompileEquations(equations);
                    result += compiledEquation.PrintVariables() + Environment.NewLine;
                    result += compiledEquation.PrintEquations() + Environment.NewLine;
                    result += compiledEquation.PrintJacobiMatrix() + Environment.NewLine;
                }
                result += Environment.NewLine;
            }
            return result;
        }
        public List<string> Solve()
        {
            List<string> output = new List<string>();
            foreach (var element in outputCurrent)
            {
                output.Add($"Current [id={element}] =");
            }
            foreach (var element in outputNodeVoltage)
            {
                ElementsAC.Element el = acGraph.elements[element];
                if (el is ElementsAC.Element2N)
                {
                    output.Add($"Voltage difference [id={element}] =");
                }
            }
            foreach (var nodePair in outputVoltageDifference)
            {
                output.Add($"Voltage difference [n1={nodeLabels[nodePair.node1]}, n2={nodeLabels[nodePair.node2]}] =");
            }
            //Решение
            //Решение схемы с источниками разных частот является композицией
            //множества решений схемы для источников отдельных частот
            foreach (float frequency in frequencies)
            {
                float hz = (float)(frequency);
                ACGraphSolution solution = acGraph.solveEquationsAC(frequency);
                int outputIndex = 0;
                foreach (var element in outputCurrent)
                {
                    Complex32 current = solution.currents[element];
                    output[outputIndex++]+=($" [{hz} Hz]({current.Magnitude}@{Utils.degrees(current.Phase)})");
                }
                foreach (var element in outputNodeVoltage)
                {
                    ElementsAC.Element el = acGraph.elements[element];
                    if (el is ElementsAC.Element2N)
                    {
                        Complex32 voltageDrop = solution.voltageDrops[element];
                        output[outputIndex++] += ($" [{hz} Hz]({voltageDrop.Magnitude}@{Utils.degrees(voltageDrop.Phase)})");
                    }
                }
                foreach (var nodePair in outputVoltageDifference)
                {
                    Complex32 diff = solution.voltages[nodePair.node2] - solution.voltages[nodePair.node1];
                    output[outputIndex++] += ($" [{hz} Hz]({diff.Magnitude}@{Utils.degrees(diff.Phase)})");
                }
            }
            return output;
        }
        public string getElementString(int index)
        {
            try
            {
                return acGraph.elements[index].ToString();
            }
            catch (Exception)
            {
                return "Invalid element index";
            }
        }
    }
}
