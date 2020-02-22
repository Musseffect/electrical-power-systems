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
            int groundNode = graph.AllocateNode();
            graph.CreateResistorWithCurrent(neutralNode, groundNode, Resistance);
            graph.CreateGround(groundNode);
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
            graph.CreateGround(neutralNode);
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
            int groundNode = graph.AllocateNode();
            graph.CreateInductor(neutralNode, groundNode, Reactance);
            graph.CreateGround(groundNode);
        }
    }
}
