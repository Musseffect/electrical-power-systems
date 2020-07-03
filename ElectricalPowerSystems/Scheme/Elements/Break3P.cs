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
    class Break3PEvent : TransientEvent
    {
        string stateName;
        double newState;
        public Break3PEvent(string stateName,double newState,double time):base(time)
        {
            this.stateName = stateName;
            this.newState = newState;
        }
        public override bool Execute(TransientState x)
        {
            return true;
        }
        public override List<Parameter> GetParameters()
        {
            return new List<Parameter>(){new Parameter(stateName, newState) };
        }
    }
    class Break3P : Element, ITransientElement,ITransientEventGenerator
    {
        double switchTime;
        bool state;
        Pin3Phase in_pin;
        Pin3Phase out_pin;
        public string IA { get { return $"I_{ID}a"; } }
        public string IB { get { return $"I_{ID}b"; } }
        public string IC { get { return $"I_{ID}c"; } }
        public Break3P(double switchTime, bool initialState, Pin3Phase in_pin, Pin3Phase out_pin):base()
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
                Equation = IA,
                Node1 = in_pin.VA,
                Node2 = out_pin.VA
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IB,
                Node1 = in_pin.VB,
                Node2 = out_pin.VB
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IC,
                Node1 = in_pin.VC,
                Node2 = out_pin.VC
            });
            equations.Add(new EquationBlock
            {
                Equation = $"({in_pin.VA} - {out_pin.VA}) * state_{ID} = {IA} * (1.0 - state_{ID});"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"({in_pin.VB} - {out_pin.VB}) * state_{ID} = {IB} * (1.0 - state_{ID});"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"({in_pin.VC} - {out_pin.VC}) * state_{ID} = {IC} * (1.0 - state_{ID});"
            });
            return equations;
        }
        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            double stateValue = state ? 1.0:0.0;
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
    public class TransientBreak3PModel : ITransientElementModel
    {
        public ITransientElement CreateElement(Interpreter.Interpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            bool initialState = (elementObject.GetValue("InitialState") as BoolValue).Value;
            double switchTime = (elementObject.GetValue("SwitchTime") as FloatValue).Value;
            return new Break3P(switchTime, initialState, elementNodes["in"] as Pin3Phase, elementNodes["out"] as Pin3Phase);
        }
    }
}
