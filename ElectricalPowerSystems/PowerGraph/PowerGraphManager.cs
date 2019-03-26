using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerGraph
{
    class PowerGraphManager
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
            Dictionary<string, int> nodes;
            List<GraphElement> elements;
            public PowerGraphModel(PowerGraphManager manager)
            {
                elements = new List<GraphElement>();
                nodes = new Dictionary<string, int>();
                int nodeId = 0;
                foreach(var element in manager.elements)
                {
                    PowerModelElement el = new PowerModelElement();
                    foreach (var node in element.Nodes)
                    {
                        int id;
                        if (!nodes.ContainsKey(node))
                        {
                            id = nodeId;
                            nodes.Add(node, nodeId);
                            nodeId++;
                        }
                        else
                        {
                            id = nodes[node];
                        }
                        el.nodes.Add(id);
                    }
                    el.element = element;
                }
            }
            public bool validate(ref List<string> errors)
            {
                return true;
            }
            public PowerGraphSolveResult solve()//in phase coordinates
            {
                //for each element
                //create abcn nodes
                //for each connection in original graph connect abcn nodes of connected elements
                //solve
                //get results
                //return results
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
            PowerGraphModel model = new PowerGraphModel(this);
            if (!model.validate(ref errors))
            {
                return;
            }
            PowerGraphSolveResult result = model.solve();
            //build scheme
            //solve
            //get values
            //return values
        }
    }
}
