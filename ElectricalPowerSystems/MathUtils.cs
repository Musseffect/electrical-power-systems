using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems
{
    /*class Matrix
    {
        float[] data;
        int width;
        int height;
    }
    class Vector
    {
        float[] data;
    }*/
    class MathUtils
    {
        static public float degrees(float value)
        {
            return 180.0f * value/ (float)Math.PI ;
        }
        static public float radians(float value)
        {
            return (float)Math.PI*value/180.0f;
        }

    }
}
