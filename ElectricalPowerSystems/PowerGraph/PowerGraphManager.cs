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
        class PowerGraphSolveResult
        {

        }
        class PowerModelElement
        {
            public GraphElement element;
            public List<int> nodes;
            public PowerModelElement()
            {
                nodes = new List<int>();
            }
        }
        class PowerGraphModel
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
            public PowerGraphModel(PowerGraphManager manager)
            {
                acGraph = new ACGraph.ACGraph();
                Dictionary<string, int> nodes;
                List<PowerModelElement> elements = new List<PowerModelElement>(manager.elements.Count);
                nodes = new Dictionary<string, int>();
                List<List<NodeIdPair>> nodeElements=new List<List<NodeIdPair>>();//Elements, connected to node
                int nodeId = 0;
                int elementId = 0;
                foreach(var element in manager.elements)
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
                            nodeElements[nodeId].Add(new NodeIdPair ( elementId, nodeLocalId ));
                            nodeId++;
                        }
                        else
                        {
                            id = nodes[node];
                            nodeElements[id].Add(new NodeIdPair ( elementId ,nodeLocalId));
                        }
                        el.nodes.Add(id);
                        nodeLocalId++;
                    }
                    el.element = element;
                    elementId++;
                    elements.Add(el);
                }
                //create abcn nodes
                int lastAbcIndex = 0;
                List<ABCElement> abcElements = new List<ABCElement>();
                BitArray visitedElements=new BitArray(elements.Count,false);
                int i = 0;
                foreach (PowerModelElement element in elements)
                {
                    ABCElement abcElement = new ABCElement(
                        element.element);
                    abcElements.Add(abcElement);
                    List<bool> phaseNodes = element.element.getPhaseNodes();
                    //for each node generate indexes for abcn nodes
                    foreach (var node in phaseNodes)
                    {
                        ABCNode abcNode=new ABCNode();
                        abcNode.A = lastAbcIndex++;
                        abcNode.B = lastAbcIndex++;
                        abcNode.C = lastAbcIndex++;
                        abcNode.N = node ? lastAbcIndex++ :-1;
                        abcElement.Nodes.Add(abcNode);
                    }
                    //generate local electric scheme for element
                    abcElement.getElementDescription().generateACGraph(acGraph);
                    if (!visitedElements[i])
                    {

                    }
                    i++;
                }
                foreach (var nodeList in nodeElements)
                {
                    ABCElement firstElement = abcElements[nodeList[0].elementId];
                    ABCNode firstABCNode = firstElement.Nodes[nodeList[0].nodeLocalId];
                    for (int j = 1; j < nodeList.Count; j++)
                    {
                        ABCElement currentElement = abcElements[nodeList[j].elementId];
                        ABCNode currentABCNode = currentElement.Nodes[nodeList[0].nodeLocalId];
                        //create line between firstElement and this node
                        acGraph.createLine(firstABCNode.A,currentABCNode.A);
                        acGraph.createLine(firstABCNode.B, currentABCNode.B);
                        acGraph.createLine(firstABCNode.C, currentABCNode.C);
                        if(firstABCNode.N!=-1&&currentABCNode.N!=-1)
                            acGraph.createLine(firstABCNode.N, currentABCNode.N);
                    }
                }
            }
            public bool validate(ref List<string> errors)
            {
                throw new NotImplementedException();
                return true;
            }
            public PowerGraphSolveResult solve()//in phase coordinates
            {
                ACGraph.ACGraphSolution acSolution = acGraph.SolveAC();
                //get results
                //return results
                return new PowerGraphSolveResult();
            }
        }
        List<GraphElement> elements;
        PowerGraphManager()
        {
            clear();
        }
        public void clear()
        {
            elements.Clear();
        }
        public void addElement(GraphElement element)
        {
            elements.Add(element);
        }
        public void solve(ref List<string> errors)
        {
            //build scheme
            PowerGraphModel model = new PowerGraphModel(this);
            //validate
            if (!model.validate(ref errors))
            {
                return;
            }
            //solve
            PowerGraphSolveResult result = model.solve();
            //get values
            //return values
        }
    }
}
