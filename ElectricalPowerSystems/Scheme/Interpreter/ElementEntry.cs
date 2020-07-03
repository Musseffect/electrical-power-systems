using System.Collections.Generic;

namespace ElectricalPowerSystems.Scheme.Interpreter
{
    public partial class Interpreter
    {
        class ElementEntry
        {
            ElementDescription description;
            Dictionary<string, Pin> elementPins;
            public ElementEntry(ElementDescription description, Dictionary<string, Pin> elementPins)
            {
                this.description = description;
                this.elementPins = elementPins;
            }
            public ElementDescription GetDescription()
            {
                return description;
            }
            public Pin GetPin(string name)
            {
                return elementPins[name];
            }
        }
    }
}
