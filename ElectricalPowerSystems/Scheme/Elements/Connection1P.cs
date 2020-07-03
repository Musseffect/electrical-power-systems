using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricalPowerSystems.Scheme.Interpreter;
using MathNet.Numerics;

namespace ElectricalPowerSystems.Scheme.Elements
{
    class Connection1P:Element,ITransientElement,ISteadyStateElement
    {
        Pin1Phase in_pin;
        Pin1Phase out_pin;
        public string I { get { return $"I_{ID}"; } }
        public string Ire { get { return $"I_{ID}_re"; } }
        public string Iim { get { return $"I_{ID}_im"; } }
        public Connection1P(Pin1Phase in_pin,Pin1Phase out_pin) : base()
        {
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
                Equation = $"{in_pin.V} - {out_pin.V} = 0;",
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new CurrentFlowBlock
            {
                Equation = Ire,
                Node1 = in_pin.Vre,
                Node2 = out_pin.Vre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = Iim,
                Node1 = in_pin.Vim,
                Node2 = out_pin.Vim
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.Vre} - {out_pin.Vre} = 0;",
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.Vim} - {out_pin.Vim} = 0;",
            });
            return equations;
        }

        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            return new List<EquationBlock>();
        }

        List<EquationBlock> ISteadyStateElement.GenerateParameters(double frequency)
        {
            return new List<EquationBlock>();
        }
    }
}
