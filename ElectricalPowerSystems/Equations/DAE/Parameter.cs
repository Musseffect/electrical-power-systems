

namespace ElectricalPowerSystems.Equations.DAE
{

    public class Parameter
    {
        string name;
        public string Name { get { return name; } }
        double value;
        public double Value { get { return value; } }
        public Parameter(string name, double value)
        {
            this.name = name;
            this.value = value;
        }
    }
}
