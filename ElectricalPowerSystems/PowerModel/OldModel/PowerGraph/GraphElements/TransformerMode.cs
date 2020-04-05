using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerModel.OldModel.PowerGraph
{

    public abstract class TransformerWinding
    {
        public enum Type
        {
            Delta,
            Wye,
            ZigZag
        };
        public Type WindingType { get; protected set; }
        public abstract int[] generateWinding(int wA,int wB,int wC, ACGraph.ACGraph acGraph);
    }
    public class DeltaWinding : TransformerWinding
    {
        public enum Mode
        {
            D1 = 1,
            D3 = 3,
            D5 = 5,
            D7 = 7,
            D9 = 9,
            D11 = 11
        };
        Mode mode;
        public DeltaWinding(Mode mode)
        {
            WindingType = Type.Delta;
            this.mode = mode;
        }
        public override int[] generateWinding(int wA, int wB, int wC, ACGraph.ACGraph acGraph)
        {
            return generateWindingConnection(wA,wB,wC);
        }
        private int[] generateWindingConnection(int a, int b, int c)
        {
            switch (mode)
            {
                case Mode.D1:
                    return new int[]{a,c,
                            b,a,
                            c,b};
                case Mode.D3:
                    return new int[]{b,c,
                            c,a,
                            a,b};
                case Mode.D5:
                    return new int[]{b,a,
                            c,b,
                            a,c};
                case Mode.D7:
                    return new int[]{b,c,
                            c,a,
                            a,b};
                case Mode.D9:
                    return new int[]{c,b,
                            a,c,
                            b,a};
                case Mode.D11:
                    return new int[]{a,b,
                            b,c,
                            c,a};
            }
            throw new Exception("Non-existing transformer mode");
        }
    }
    public class WyeWinding : TransformerWinding
    {
        public enum Mode
        {
            Y0 = 0,
            Y2 = 2,
            Y4 = 4,
            Y6 = 6,
            Y8 = 8,
            Y10 = 10
        };
        Mode mode;
        GroundingType grounding;
        public WyeWinding(Mode mode,GroundingType grounding)
        {
            WindingType = Type.Wye;
            this.mode = mode;
            this.grounding = grounding;
        }

        public override int[] generateWinding(int wA, int wB, int wC, ACGraph.ACGraph acGraph)
        {
            int n = acGraph.AllocateNode();
            grounding.CreateScheme(acGraph, n);
            return generateWindingConnection(wA, wB, wC,n);
        }
        private int[] generateWindingConnection(int a, int b, int c, int n)
        {
            switch (mode)
            {
                case Mode.Y0:
                    return new int[]{a,n,
                            b,n,
                            c,n};
                case Mode.Y2:
                    return new int[]{n,c,
                            n,a,
                            n,b};
                case Mode.Y4:
                    return new int[]{b,n,
                            c,n,
                            a,n};
                case Mode.Y6:
                    return new int[]{n,a,
                            n,b,
                            n,c};
                case Mode.Y8:
                    return new int[]{c,n,
                            a,n,
                            b,n};
                case Mode.Y10:
                    return new int[]{n,b,
                            n,c,
                            n,a};
            }
            throw new Exception("Non-existing transformer mode");
        }
    }
    //TODO
    public class ZigZagWinding : TransformerWinding
    {
        public enum Mode
        {
            ZZ0
        };
        GroundingType grounding;
        Mode mode;

        public ZigZagWinding(Mode mode, GroundingType grounding)
        {
            WindingType = Type.ZigZag;
            this.mode = mode;
            this.grounding = grounding;
        }
        public override int[] generateWinding(int wA, int wB, int wC, ACGraph.ACGraph acGraph)
        {
            int n = acGraph.AllocateNode();
            grounding.CreateScheme(acGraph, n);
            return generateWindingConnection(wA, wB, wC, n);
        }
        private int[] generateWindingConnection(int a, int b, int c, int n)
        {
            throw new NotImplementedException();
        }
    }
}
