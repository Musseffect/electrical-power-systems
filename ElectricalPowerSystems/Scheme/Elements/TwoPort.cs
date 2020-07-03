using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricalPowerSystems.Scheme.Interpreter;
using static ElectricalPowerSystems.Scheme.Interpreter.Interpreter;

namespace ElectricalPowerSystems.Scheme.Elements
{
    class TwoPort : Element, ITransientElement, ISteadyStateElement
    {
        Pin1Phase in1_pin;
        Pin1Phase out1_pin;
        Pin1Phase in2_pin;
        Pin1Phase out2_pin;
        double b11;
        double b12;
        double b21;
        double b22;
        public TwoPort(double b11, double b12, double b21, double b22,
            Pin1Phase in1_pin, Pin1Phase in2_pin, Pin1Phase out1_pin, Pin1Phase out2_pin) : base()
        {
            this.in1_pin = in1_pin;
            this.in2_pin = in2_pin;
            this.out1_pin = out1_pin;
            this.out2_pin = out2_pin;
            this.b11 = b11;
            this.b12 = b12;
            this.b21 = b21;
            this.b22 = b22;
        }

        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock {
                Equation = $"({in1_pin.V} - {in2_pin.V}) * b11_{ID} + I_{ID}_1 * b12_{ID} = {out1_pin.V} - {out2_pin.V};"
            });
            equations.Add(new EquationBlock{
                Equation = $"({in1_pin.V} - {in2_pin.V}) * b21_{ID} + I_{ID}_1 * b22_{ID} = I_{ID}_2;"
            });
            equations.Add(new CurrentFlowBlock{
                Equation = $"I_{ID}_1",
                Node1 = in1_pin.V,
                Node2 = in2_pin.V
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"I_{ID}_2",
                Node1 = out1_pin.V,
                Node2 = out2_pin.V
            });
            return equations;
        }

        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            throw new NotImplementedException();
        }

        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"constant b11_{ID} = {b11};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant b12_{ID} = {b12};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant b21_{ID} = {b21};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant b22_{ID} = {b22};"
            });
            return equations;
        }

        List<EquationBlock> ISteadyStateElement.GenerateParameters(double frequency)
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"constant b11_{ID} = {b11};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant b12_{ID} = {b12};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant b21_{ID} = {b21};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant b22_{ID} = {b22};"
            });
            return equations;
        }
    }
    public class SteadyStateTwoPortModel : ISteadyStateElementModel
    {
        public ISteadyStateElement CreateElement(Interpreter.Interpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            double b11 = (elementObject.GetValue("Xm") as FloatValue).Value;
            double b12 = (elementObject.GetValue("Xm") as FloatValue).Value;
            double b21 = (elementObject.GetValue("Xm") as FloatValue).Value;
            double b22 = (elementObject.GetValue("Xm") as FloatValue).Value;
            return new TwoPort(b11, b12, b21, b22, elementNodes["in1"] as Pin1Phase, elementNodes["in2"] as Pin1Phase,
                elementNodes["out1"] as Pin1Phase, elementNodes["out2"] as Pin1Phase);
        }
    }
    public class TransientTwoPortModel : ITransientElementModel
    {
        public ITransientElement CreateElement(Interpreter.Interpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            double b11 = (elementObject.GetValue("Xm") as FloatValue).Value;
            double b12 = (elementObject.GetValue("Xm") as FloatValue).Value;
            double b21 = (elementObject.GetValue("Xm") as FloatValue).Value;
            double b22 = (elementObject.GetValue("Xm") as FloatValue).Value;
            return new TwoPort(b11, b12, b21, b22, elementNodes["in1"] as Pin1Phase, elementNodes["in2"] as Pin1Phase,
                elementNodes["out1"] as Pin1Phase, elementNodes["out2"] as Pin1Phase);
        }
    }
}
