using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems
{
    class ModelInterpreterException:Exception
    {
        public ModelInterpreterException(string message) : base(message)
        {
        }
        public int Line { get; set; }
        public int Position { get; set; }
    }
}
