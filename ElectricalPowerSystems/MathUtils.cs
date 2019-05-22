using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems
{
    class MathUtils
    {
        static public double degrees(double value)
        {
            return 180.0 * value/ Math.PI ;
        }
        static public double radians(double value)
        {
            return Math.PI*value/180.0;
        }

    }
}
