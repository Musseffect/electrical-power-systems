using MathNet.Numerics.LinearAlgebra;

namespace ElectricalPowerSystems.Equations.DAE
{
    public class XZVectorPair
    {
        public Vector<double> X;
        public Vector<double> Z;
        public double[] ToArray()
        {
            double[] result = new double[X.Count + Z.Count];
            for (int i = 0; i < X.Count; i++)
                result[i] = X[i];
            for (int j = 0; j < Z.Count; j++)
                result[j + X.Count] = Z[j];
            return result;
        }
    }
}
