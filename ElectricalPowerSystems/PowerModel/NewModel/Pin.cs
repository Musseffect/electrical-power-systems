using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerModel.NewModel
{
    public abstract class Pin
    {
        private int index;
        public int Index { get { return index; } }
        public Pin(int index)
        {
            this.index = index;
        }
        public abstract string[] GetNodeVariables();
        public abstract string[] GetNodeComplexVariables();
    }
    public class Pin1Phase : Pin
    {
        public string V { get { return $"v_{Index}"; } }
        public string Vre { get { return $"v_{Index}_re"; } }
        public string Vim { get { return $"v_{Index}_im"; } }
        public Pin1Phase(int index) : base(index)
        {
        }
        public override string[] GetNodeVariables()
        {
            return new string[] { V };
        }
        public override string[] GetNodeComplexVariables()
        {
            return new string[] { Vre, Vim };
        }
    }
    public class Pin3Phase : Pin
    {
        public string VA { get { return $"v_{Index}a"; } }
        public string VB { get { return $"v_{Index}b"; } }
        public string VC { get { return $"v_{Index}c"; } }
        public string VAre { get { return $"v_{Index}a_re"; } }
        public string VAim { get { return $"v_{Index}a_im"; } }
        public string VBre { get { return $"v_{Index}b_re"; } }
        public string VBim { get { return $"v_{Index}b_im"; } }
        public string VCre { get { return $"v_{Index}c_re"; } }
        public string VCim { get { return $"v_{Index}c_im"; } }
        public Pin3Phase(int index) : base(index)
        {
        }
        public override string[] GetNodeVariables()
        {
            return new string[] { VA, VB, VC };
        }
        public override string[] GetNodeComplexVariables()
        {
            return new string[] { VAre, VAim,VBre,VBim,VCre,VCim };
        }
    }
}
