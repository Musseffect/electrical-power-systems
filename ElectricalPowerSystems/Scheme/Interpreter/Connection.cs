namespace ElectricalPowerSystems.Scheme.Interpreter
{
    public partial class Interpreter
    {
        public struct Connection
        {
            public Pin pin1;
            public Pin pin2;
            public Connection(Pin pin1,Pin pin2)
            {
                this.pin1 = pin1;
                this.pin2 = pin2;
            }
            public ISteadyStateElement CreateSteadyStateElement()
            {
                if (pin1 is Pin1Phase)
                    return new Elements.Connection1P(pin1 as Pin1Phase, pin2 as Pin1Phase);
                else
                    return new Elements.Connection3P(pin1 as Pin3Phase, pin2 as Pin3Phase);
            }
            public ITransientElement CreateTransientElement()
            {
                if (pin1 is Pin1Phase)
                    return new Elements.Connection1P(pin1 as Pin1Phase, pin2 as Pin1Phase);
                else
                    return new Elements.Connection3P(pin1 as Pin3Phase, pin2 as Pin3Phase);
            }
        }
    }
}
