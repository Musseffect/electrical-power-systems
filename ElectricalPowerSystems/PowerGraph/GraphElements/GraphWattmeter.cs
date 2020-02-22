using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerGraph
{
    public class GraphWattmeter : GraphElement
    {
        public string label;
        public GraphWattmeter(string inNode, string outNode,string meterLabel)
        {
            nodes.Add(inNode);
            nodes.Add(outNode);
            label = meterLabel;
        }
        public override PowerElementScheme generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            return new WattmeterScheme(nodes, acGraph, this);
        }
    }
    public struct WattmeterValues
    {
        public Complex32 VoltageAB;
        public Complex32 VoltageBC;
        public Complex32 VoltageCA;
        public Complex32 VoltageA;
        public Complex32 VoltageB;
        public Complex32 VoltageC;
        public Complex32 PowerA;
        public Complex32 PowerB;
        public Complex32 PowerC;
        public Complex32 CurrentA;
        public Complex32 CurrentB;
        public Complex32 CurrentC;
    }
    class WattmeterScheme : PowerElementScheme
    {
        public string label;
        int inA;
        int inB;
        int inC;

        int outA;
        int outB;
        int outC;

        int l1;
        int l2;
        int l3;
        public WattmeterScheme(List<ABCNode> nodes, ACGraph.ACGraph acGraph, GraphWattmeter meter):base()
        {
            inA = nodes[0].A;
            inB = nodes[0].B;
            inC = nodes[0].C;

            outA = nodes[1].A;
            outB = nodes[1].B;
            outC = nodes[1].C;
            this.label = meter.label;
            generate(acGraph, meter);
        }
        private void generate(ACGraph.ACGraph acGraph, GraphWattmeter meter)
        {
            AddElement(l1 = acGraph.CreateLine(inA,outA));
            AddElement(l2 = acGraph.CreateLine(inB,outB));
            AddElement(l3 = acGraph.CreateLine(inC,outC));
        }
        public WattmeterValues GetValues(ACGraph.ACGraphSolution solution)
        {
            WattmeterValues result = new WattmeterValues();
            result.VoltageA = solution.voltages[inA];
            result.VoltageB = solution.voltages[inB];
            result.VoltageC = solution.voltages[inC];
            result.CurrentA = solution.currents[l1];
            result.CurrentB = solution.currents[l2];
            result.CurrentC = solution.currents[l3];
            result.VoltageAB = result.VoltageA - result.VoltageB;
            result.VoltageBC = result.VoltageB - result.VoltageC;
            result.VoltageCA = result.VoltageC - result.VoltageA;
            result.PowerA = result.VoltageA * result.CurrentA.Conjugate();
            result.PowerB = result.VoltageB * result.CurrentB.Conjugate();
            result.PowerC = result.VoltageC * result.CurrentC.Conjugate();
            return result;
        }
    }
}
