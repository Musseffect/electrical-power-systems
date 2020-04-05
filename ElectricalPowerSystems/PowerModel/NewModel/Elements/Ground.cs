using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerModel.NewModel.Elements
{
    class Ground:Element, ITransientElement,ISteadyStateElement
    {
        Pin1Phase in_pin;
        public string I { get { return $"I_{ID}"; } }
        public string Ire { get { return $"I_{ID}_re"; } }
        public string Iim { get { return $"I_{ID}_im"; } }
        public Ground(Pin1Phase in_pin):base()
        {
            this.in_pin = in_pin;
        }
        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new CurrentFlowBlock
            {
                Equation = I,
                Node1 = in_pin.V,
                Node2 = null
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.V} = 0;"
            });
            return equations;
        }
        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            return new List<EquationBlock>();
        }
        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new CurrentFlowBlock
            {
                Equation = Ire,
                Node1 = in_pin.Vre,
                Node2 = null
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = Iim,
                Node1 = in_pin.Vim,
                Node2 = null
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.Vre} = 0;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.Vim} = 0;"
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateParameters(double frequency)
        {
            return new List<EquationBlock>();
        }
    }
    public class SteadyStateGroundModel : ISteadyStateElementModel
    {
        ISteadyStateElement ISteadyStateElementModel.CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            return new Ground(elementNodes["in"] as Pin1Phase);
        }
    }
}
