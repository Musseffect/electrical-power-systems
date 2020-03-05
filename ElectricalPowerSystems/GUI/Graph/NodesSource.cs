using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ElectricalPowerSystems.GUI
{
    public static class NodesSource
    {
        public static List<Node> createNodes()
        {
            Random random = new Random(0);
            List<Node> nodes = new List<Node>();
            for (int i = 0; i < 5; i++)
            {
                var node = new Node
                {
                    X = random.Next(0, 100),
                    Y = random.Next(0, 100),
                    Width=40,
                    Height=40,
                    Color = Color.FromRgb((byte)random.Next(255), 
                    (byte)random.Next(255), 
                    (byte)random.Next(255))
                };
                nodes.Add(node);
            }
            return nodes;
        }
    }
}
