using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems
{
    class ModelException: Exception
    {
        public virtual string getMessage()
        {
            return this.Message;
        }
    }
    class ModelInterpreterException:Exception
    {
        public ModelInterpreterException(string message) : base(message)
        {
        }
        public int Line { get; set; }
        public int Position { get; set; }
    }
}
