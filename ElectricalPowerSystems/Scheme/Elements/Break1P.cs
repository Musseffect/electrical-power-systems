using ElectricalPowerSystems.Equations.DAE;
using ElectricalPowerSystems.Scheme.Transient;
using ElectricalPowerSystems.Scheme.Interpreter;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ElectricalPowerSystems.Scheme.Interpreter.Interpreter;

namespace ElectricalPowerSystems.Scheme.Elements
{
    class Break1PEvent : TransientEvent
    {
        string stateName;
        double newState;
        public Break1PEvent(string stateName, double newState, double time) : base(time)
        {
            this.stateName = stateName;
            this.newState = newState;
        }
        public override bool Execute(TransientState stateValuesx)
        {
            return true;
        }
        public override List<Parameter> GetParameters()
        {
            return new List<Parameter>() { new Parameter(stateName, newState) };
        }
    }
    class Break1P : Element, ITransientElement, ITransientEventGenerator
    {
        double switchTime;
        bool state;
        Pin1Phase in_pin;
        Pin1Phase out_pin;
        public string I { get { return $"I_{ID}"; } }
        public string Ire { get { return $"I_{ID}_re"; } }
        public string Iim { get { return $"I_{ID}_im"; } }
        public Break1P(double switchTime, bool initialState, Pin1Phase in_pin, Pin1Phase out_pin) : base()
        {
            this.switchTime = switchTime;
            this.state = initialState;
            this.in_pin = in_pin;
            this.out_pin = out_pin;
        }
        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new CurrentFlowBlock
            {
                Equation = I,
                Node1 = in_pin.V,
                Node2 = out_pin.V
            });
            equations.Add(new EquationBlock
            {
                Equation = $"({in_pin.V} - {out_pin.V}) * state_{ID} = {I} * (1.0 - state_{ID});"
            });
            return equations;
        }
        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            double stateValue = state ? 1.0 : 0.0;
            equations.Add(new EquationBlock
            {
                Equation = $"parameter state_{ID} = {stateValue.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
        List<TransientEvent> ITransientEventGenerator.GenerateEvents(double t0, double t1)
        {
            if (t1 < switchTime)
                return new List<TransientEvent>();
            double newStateValue = state ? 0.0 : 1.0;
            return new List<TransientEvent> { new Break3PEvent($"state_{ID}", newStateValue, switchTime) };
        }
    }
    public class TransientBreak1PModel : ITransientElementModel
    {
        public ITransientElement CreateElement(Interpreter.Interpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            bool initialState = (elementObject.GetValue("InitialState") as BoolValue).Value;
            double switchTime = (elementObject.GetValue("SwitchTime") as FloatValue).Value;
            return new Break1P(switchTime, initialState, elementNodes["in"] as Pin1Phase, elementNodes["out"] as Pin1Phase);
        }
    }
}
