using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.MathUtils
{
    class MathUtils
    {
        static public double Clamp(double min, double max, double t)
        {
            return Math.Max(Math.Min(t,max),min);
        }
        static public Vector<double> Interpolate(Vector<double> a,Vector<double> b, double t)
        {
            t = Clamp(0.0, 1.0, t);
            if (Math.Abs(t - 1.0) < 0.001)
                return b;
            if (Math.Abs(t) < 0.0)
                return a;
            return a * (1.0 - t) + b * t;
        }
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
