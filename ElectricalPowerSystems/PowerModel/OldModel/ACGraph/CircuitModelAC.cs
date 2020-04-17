using ElectricalPowerSystems.Equations.Nonlinear;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//https://ptolemy.berkeley.edu/projects/embedded/eecsx44/fall2011/lectures/01-modelling.pdf
namespace ElectricalPowerSystems.PowerModel.OldModel.ACGraph
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
        private int RetrieveNodeId(string key)
        {
            int node = acGraph.nodesList.Count;
            try
            {
                node = nodes[key];
            }
            catch (KeyNotFoundException)
            {
                nodes.Add(key, node);
                acGraph.AllocateNode();
                nodeLabels.Add(key);
            }
            return node;
        }
        public int AddTransformer(string node1, string node2, string node3, string node4, float k)
        {
            int node1Id = RetrieveNodeId(node1);
            int node2Id = RetrieveNodeId(node2);
            int node3Id = RetrieveNodeId(node3);
            int node4Id = RetrieveNodeId(node4);
            return acGraph.CreateTransformer(node1Id, node2Id, node3Id, node4Id,k);
        }
        public int AddTransformer3w(string node1, string node2, string node3, string node4, string node5, string node6, float k1,float k2)
        {
            int node1Id = RetrieveNodeId(node1);
            int node2Id = RetrieveNodeId(node2);
            int node3Id = RetrieveNodeId(node3);
            int node4Id = RetrieveNodeId(node4);
            int node5Id = RetrieveNodeId(node5);
            int node6Id = RetrieveNodeId(node6);
            return acGraph.CreateTransformer3w(node1Id, node2Id, node3Id, node4Id, node5Id, node6Id, k1,k2);
        }
        public int AddAutoTransformer(string node1, string node2, string node3, float k)
        {
            int node1Id = RetrieveNodeId(node1);
            int node2Id = RetrieveNodeId(node2);
            int node3Id = RetrieveNodeId(node3);
            return acGraph.CreateAutotransformer(node1Id, node2Id, node3Id, k);
        }
        public int AddSwitch(string node1, string node2, bool state)
        {
            int node1Id = RetrieveNodeId(node1);
            int node2Id = RetrieveNodeId(node2);
            return acGraph.CreateSwitch(node1Id, node2Id, state);
        }
        public int AddImpedance(string node1, string node2, Complex32 impedance)
        {
            int node1Id = RetrieveNodeId(node1);
            int node2Id = RetrieveNodeId(node2);
            return acGraph.CreateImpedance(node1Id, node2Id, impedance);
        }
        public int AddResistor(string node1, string node2, float resistance)
        {
            int node1Id = RetrieveNodeId(node1);
            int node2Id = RetrieveNodeId(node2);
            return acGraph.CreateResistor(node1Id,node2Id,resistance);
        }
        public int AddLine(string node1, string node2)
        {
            int node1Id = RetrieveNodeId(node1);
            int node2Id = RetrieveNodeId(node2);
            return acGraph.CreateLine(node1Id,node2Id);
        }
        public int AddCapacitor(string node1, string node2, float capacity)
        {
            int node1Id = RetrieveNodeId(node1);
            int node2Id = RetrieveNodeId(node2);
            return acGraph.CreateCapacitor(node1Id, node2Id, capacity);
        }
        //частота в герцах
        public int AddVoltageSource(string node1, string node2, float voltage, float phase, float freq)
        {
            frequencies.Add(freq);
            int node1Id = RetrieveNodeId(node1);
            int node2Id = RetrieveNodeId(node2);
            return acGraph.CreateVoltageSource(node1Id, node2Id, voltage,phase,freq);
        }
        //частота в герцах
        public int AddCurrentSource(string node1, string node2, float current, float phase, float freq)
        {
            frequencies.Add(freq);
            int node1Id = RetrieveNodeId(node1);
            int node2Id = RetrieveNodeId(node2);
            return acGraph.CreateCurrentSource(node1Id, node2Id, current, phase, freq);
        }
        public void AddGround(string node)
        {
            int nodeId = RetrieveNodeId(node);
            acGraph.CreateGround(nodeId);
        }
        public int AddInductor(string node1, string node2, float inductivity)
        {
            int node1Id = RetrieveNodeId(node1);
            int node2Id = RetrieveNodeId(node2);
            return acGraph.CreateInductor(node1Id,node2Id,inductivity);
        }
        public void AddCurrentOutput(int elementIndex)
        {
            if (elementIndex >= acGraph.elements.Count ||elementIndex<0)
                throw new Exception("Incorrect element index.");
            outputCurrent.Add(elementIndex);
        }
        public void AddVoltageOutput(int elementIndex)
        {
            if (elementIndex >= acGraph.elements.Count || elementIndex < 0)
                throw new Exception("Incorrect element index.");
            outputNodeVoltage.Add(elementIndex);
        }
        public void AddVoltageOutput(string node1, string node2)
        {
            int node1Id = RetrieveNodeId(node1);
            int node2Id = RetrieveNodeId(node2);
            outputVoltageDifference.Add(new NodePair(node1Id, node2Id));
        }
        public string TestEquationGeneration(bool useCompiledEquation = false)
        {
            string result = "";
            foreach (float frequency in frequencies)
            {
                float hz = (float)(frequency);
                string equations = acGraph.EquationGeneration(frequency);
                result += equations + Environment.NewLine;
                if (useCompiledEquation)
                {
                    Compiler compiler = new Compiler();
                    NonlinearEquationDescription compiledEquation = compiler.CompileEquations(equations);
                    result += compiledEquation.PrintVariables() + Environment.NewLine;
                    result += compiledEquation.PrintEquations() + Environment.NewLine;
                    result += compiledEquation.PrintJacobiMatrix() + Environment.NewLine;
                }
                result += Environment.NewLine;
            }
            return result;
        }
        public List<string> EquationGeneration()
        {
            List<string> result = new List<string>();
            foreach (float frequency in frequencies)
            {
                float hz = (float)(frequency);
                string equations = acGraph.EquationGeneration(frequency);
                result.AddRange(
                           equations.Split(new string[] { "\r\n", "\n" },
                           StringSplitOptions.None)
                           );
            }
            return result;
        }
        public List<string> EquationGenerationTransient()
        {
            List<string> result = new List<string>();
            string equations = acGraph.EquationGenerationTransient();
            result.AddRange(
                       equations.Split(new string[] { "\r\n", "\n" },
                       StringSplitOptions.None)
                       );
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
                ACGraphSolution solution = acGraph.SolveEquationsAC(frequency);
                int outputIndex = 0;
                foreach (var element in outputCurrent)
                {
                    Complex32 current = solution.currents[element];
                    output[outputIndex++]+=($" [{hz} Hz]({current.Magnitude}@{MathUtils.MathUtils.Degrees(current.Phase)})");
                }
                foreach (var element in outputNodeVoltage)
                {
                    ElementsAC.Element el = acGraph.elements[element];
                    if (el is ElementsAC.Element2N)
                    {
                        Complex32 voltageDrop = solution.voltageDrops[element];
                        output[outputIndex++] += ($" [{hz} Hz]({voltageDrop.Magnitude}@{MathUtils.MathUtils.Degrees(voltageDrop.Phase)})");
                    }
                }
                foreach (var nodePair in outputVoltageDifference)
                {
                    Complex32 diff = solution.voltages[nodePair.node2] - solution.voltages[nodePair.node1];
                    output[outputIndex++] += ($" [{hz} Hz]({diff.Magnitude}@{MathUtils.MathUtils.Degrees(diff.Phase)})");
                }
            }
            return output;
        }
        public string GetElementString(int index)
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
        public void SolveTransient()
        {
            throw new NotImplementedException();
        }
    }
}
