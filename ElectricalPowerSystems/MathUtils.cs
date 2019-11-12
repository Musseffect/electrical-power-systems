using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems
{
    class Utils
    {
        static public double degrees(double value)
        {
            return 180.0 * value/ Math.PI ;
        }
        static public double radians(double value)
        {
            return Math.PI*value/180.0;
        }
        static public string complex32ToAmpPhaseString(Complex32 z)
        {
            return z.Magnitude.ToString() + "@" + z.Phase.ToString();
        }

    }
}
