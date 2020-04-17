using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerModel.NewModel.Recloser
{
    public abstract class Compiler
    {
        public class Local
        {
            enum Type
            {
                Int,
                Bool,
                Float
            }
            Type type;
            string name;
        }
        Local[] locals = new Local[256];


        public abstract Program Compile(ProgramNode program);
    }
}
