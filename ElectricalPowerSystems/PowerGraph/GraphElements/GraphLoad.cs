﻿using ElectricalPowerSystems.ACGraph;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace ElectricalPowerSystems.PowerGraph
{
    public class GraphLoadPQWye : GraphElement
    {
        public Complex32 s;
        public GroundingType grounding;
        public GraphLoadPQWye(string node, Complex32 s, GroundingType grounding)
        {
            nodes.Add(node);
            this.s = s;
            this.grounding = grounding;
        }
        public override PowerElementScheme generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            return new LoadSchemePQWye(nodes, acGraph, this);
        }
    }
    public class GraphLoadPQDelta : GraphElement
    {
        public Complex32 s;
        public GraphLoadPQDelta(string node, Complex32 s)
        {
            nodes.Add(node);
            this.s = s;
        }
        public override PowerElementScheme generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            return new LoadSchemePQDelta(nodes, acGraph, this);
        }
    }
    public class GraphLoadWye :GraphElement
    {   
        public Complex32 resistanceA;
        public Complex32 resistanceB;
        public Complex32 resistanceC;
        public GroundingType grounding;
        public GraphLoadWye(string node,Complex32 resA,Complex32 resB,Complex32 resC,GroundingType grounding)
        {
            nodes.Add(node);
            this.grounding = grounding;
            resistanceA = resA;
            resistanceB = resB;
            resistanceC = resC;
        }
        public override PowerElementScheme generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            return new LoadSchemeWye(nodes,acGraph,this);
        }
    }

    public class GraphLoadDelta : GraphElement
    {
        public Complex32 resistanceA;
        public Complex32 resistanceB;
        public Complex32 resistanceC;
        public GraphLoadDelta(string node, Complex32 resA, Complex32 resB, Complex32 resC)
        {
            nodes.Add(node);
            resistanceA = resA;
            resistanceB = resB;
            resistanceC = resC;
        }
        public override PowerElementScheme generateACGraph(List<ABCNode> nodes, ACGraph.ACGraph acGraph)
        {
            return new LoadSchemeDelta(nodes, acGraph, this);
        }
    }
    public class LoadSchemePQDelta : PowerElementScheme
    {
        public LoadSchemePQDelta(List<ABCNode> nodes, ACGraph.ACGraph acGraph, GraphLoadPQDelta load)
        {
            throw new NotImplementedException();
        }
    }
    public class LoadSchemePQWye : PowerElementScheme
    {
        public LoadSchemePQWye(List<ABCNode> nodes, ACGraph.ACGraph acGraph, GraphLoadPQWye load)
        {
            throw new NotImplementedException();
        }
    }
    public class LoadSchemeDelta : PowerElementScheme
    {
        private int inA;
        private int inB;
        private int inC;
        public LoadSchemeDelta(List<ABCNode> nodes, ACGraph.ACGraph acGraph, GraphLoadDelta load)
        {
            inA = nodes[0].A;
            inB = nodes[0].B;
            inC = nodes[0].C;
            generate(acGraph, load);
        }
        private void generate(ACGraph.ACGraph acGraph, GraphLoadDelta load)
        {
            AddElement(acGraph.CreateImpedanceWithCurrent(inC, inA, load.resistanceA));
            AddElement(acGraph.CreateImpedanceWithCurrent(inA, inB, load.resistanceB));
            AddElement(acGraph.CreateImpedanceWithCurrent(inB, inC, load.resistanceC));
        }
    }
    public class LoadSchemeWye : PowerElementScheme
    {
        int inA;
        int inB;
        int inC;
        int inN;

        public LoadSchemeWye(List<ABCNode> nodes, ACGraph.ACGraph acGraph, GraphLoadWye load)
        {
            inA = nodes[0].A;
            inB = nodes[0].B;
            inC = nodes[0].C;
            inN = acGraph.AllocateNode();
            generate(acGraph, load);
        }
        private void generate(ACGraph.ACGraph acGraph, GraphLoadWye load)
        {
            AddElement(acGraph.CreateImpedanceWithCurrent(inN, inA, load.resistanceA));
            AddElement(acGraph.CreateImpedanceWithCurrent(inN, inB, load.resistanceB));
            AddElement(acGraph.CreateImpedanceWithCurrent(inN, inC, load.resistanceC));
            load.grounding.CreateScheme(acGraph, inN);
        }
    }

}
