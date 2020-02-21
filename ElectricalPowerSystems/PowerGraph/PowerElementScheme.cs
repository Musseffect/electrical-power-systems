using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricalPowerSystems.ACGraph.ElementsAC;

namespace ElectricalPowerSystems.PowerGraph
{
    public abstract class PowerElementScheme
    {
        /*public List<ABCNode> nodes;
        public List<int> schemeElements;
        public GraphElement element;*/
        private List<int> elements;
        protected PowerElementScheme()
        {
            elements = new List<int>();
        }
        protected void AddElement(int element)
        {
            elements.Add(element);
        }
        public Complex32 ComputePower(ACGraph.ACGraphSolution solution)
        {
            Complex32 power = 0.0f;
            foreach (var elementIndex in elements)
            {
                power += solution.voltageDrops[elementIndex] *solution.currents[elementIndex].Conjugate();
            }
            return power;
        }
        //abstract public void calcResults(ref PowerGraphManager.PowerGraphSolveResult result, ACGraph.ACGraphSolution solution);
    }
}
