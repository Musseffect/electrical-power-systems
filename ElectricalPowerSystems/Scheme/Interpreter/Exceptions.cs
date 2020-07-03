using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Scheme.Interpreter
{
    public partial class Interpreter
    {
        public class ValueException : Exception
        {
        }
        public class MissingValueException : Exception
        {
            string key;
            public MissingValueException(string key) : base()
            {
                this.key = key;
            }
            public string Key { get { return key; } }
        }
        public class TypeConversionError : Exception
        {
            string src;
            string dst;
            public TypeConversionError(string src, string dst) : base()
            {
                this.src = src;
                this.dst = dst;
            }
            public string Src { get { return src; } }
            public string Dst { get { return dst; } }
        }
    }
}
