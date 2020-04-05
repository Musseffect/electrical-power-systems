using ElectricalPowerSystems.PowerModel.NewModel.Elements;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ElectricalPowerSystems.PowerModel.NewModel.ModelInterpreter;

namespace ElectricalPowerSystems.PowerModel.NewModel
{
    class Break3P : Element, ITransientElement,ITransientEventGenerator
    {
        double switchTime;
        bool initialState;
        Pin3Phase in_pin;
        Pin3Phase out_pin;
        public string IA { get { return $"I_{ID}a"; } }
        public string IB { get { return $"I_{ID}b"; } }
        public string IC { get { return $"I_{ID}c"; } }
        public string IAre { get { return $"I_{ID}a_re"; } }
        public string IBre { get { return $"I_{ID}b_re"; } }
        public string ICre { get { return $"I_{ID}c_re"; } }
        public string IAim { get { return $"I_{ID}a_im"; } }
        public string IBim { get { return $"I_{ID}b_im"; } }
        public string ICim { get { return $"I_{ID}c_im"; } }
        public Break3P(double switchTime, bool initialState, Pin3Phase in_pin, Pin3Phase out_pin):base()
        {
            this.switchTime = switchTime;
            this.initialState = initialState;
            this.in_pin = in_pin;
            this.out_pin = out_pin;
        }
        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            throw new NotImplementedException();
        }
        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            double stateValue = initialState?1.0:0.0;
            equations.Add(new EquationBlock
            {
                Equation = $"parameter state_{ID} = {stateValue.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
        List<TransientEvent> ITransientEventGenerator.GenerateEvents(double t0, double t1)
        {
            throw new NotImplementedException();
        }
    }
    public class TransientBreak3PModel : ITransientElementModel
    {
        public ITransientElement CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            bool initialState = (elementObject.GetValue("InitialState") as BoolValue).Value;
            double switchTime = (elementObject.GetValue("SwitchTime") as FloatValue).Value;
            return new Break3P(switchTime, initialState, elementNodes["in"] as Pin3Phase, elementNodes["out"] as Pin3Phase);
        }
    }
}
