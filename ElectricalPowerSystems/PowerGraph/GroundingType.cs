using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerGraph
{

    public abstract class GroundingType
    {
        public enum Type
        {
            Ungrounded,
            ResistanceGrounding,
            SolidGrounding,
            ReactanceGrounding
        };
        public Type type;
        public abstract void createScheme(ACGraph.ACGraph graph, int neutralNode);
    }
    public class Ungrounded : GroundingType
    {
        public Ungrounded()
        {
            this.type = Type.Ungrounded;
        }
        public override void createScheme(ACGraph.ACGraph graph, int neutralNode)
        {
        }
    }
    public class ResistanceGrounding : GroundingType
    {
        public float Resistance;
        public ResistanceGrounding(float resistance)
        {
            this.type = Type.ResistanceGrounding;
            this.Resistance = resistance;
        }
        public override void createScheme(ACGraph.ACGraph graph, int neutralNode)
        {
            int groundNode = graph.allocateNode();
            graph.createResistorWithCurrent(neutralNode, groundNode, Resistance);
            graph.createGround(groundNode);
        }
    }
    public class SolidGrounding : GroundingType
    {
        public SolidGrounding()
        {
            this.type = Type.SolidGrounding;
        }
        public override void createScheme(ACGraph.ACGraph graph, int neutralNode)
        {
            graph.createGround(neutralNode);
        }
    }
    public class ReactanceGrounding : GroundingType
    {
        public float Reactance;
        public ReactanceGrounding(float reactance)
        {
            this.type = Type.ReactanceGrounding;
            this.Reactance = reactance;
        }
        public override void createScheme(ACGraph.ACGraph graph, int neutralNode)
        {
            int groundNode = graph.allocateNode();
            graph.createInductor(neutralNode, groundNode, Reactance);
            graph.createGround(groundNode);
        }
    }
}
