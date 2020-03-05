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
        static public double Degrees(double value)
        {
            return 180.0 * value/ Math.PI ;
        }
        static public double Radians(double value)
        {
            return Math.PI*value/180.0;
        }
        static public string Complex32ToAmpPhaseString(Complex32 z)
        {
            return z.Magnitude.ToString() + "@" + z.Phase.ToString();
        }
        static public double HertzToAngular(double hertz)
        {
            return hertz * Math.PI * 2.0;
        }
        static public double AngularToHertz(double angular)
        {
            return angular / (Math.PI * 2.0);
        }

    }
}
