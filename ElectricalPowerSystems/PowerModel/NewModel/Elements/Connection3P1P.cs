using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerModel.NewModel.Elements
{
    class Connection3P1P : Element, ITransientElement, ISteadyStateElement
    {
        Pin3Phase abc_pin;
        Pin1Phase a_pin;
        Pin1Phase b_pin;
        Pin1Phase c_pin;
        public string IA { get { return $"I_{ID}a"; } }
        public string IB { get { return $"I_{ID}b"; } }
        public string IC { get { return $"I_{ID}c"; } }
        public string IAre { get { return $"I_{ID}a_re"; } }
        public string IBre { get { return $"I_{ID}b_re"; } }
        public string ICre { get { return $"I_{ID}c_re"; } }
        public string IAim { get { return $"I_{ID}a_im"; } }
        public string IBim { get { return $"I_{ID}b_im"; } }
        public string ICim { get { return $"I_{ID}c_im"; } }
        public Connection3P1P(Pin3Phase abc_pin,Pin1Phase a_pin,Pin1Phase b_pin,Pin1Phase c_pin) :base()
        {
            this.abc_pin = abc_pin;
            this.a_pin = a_pin;
            this.b_pin = b_pin;
            this.c_pin = c_pin;
        }
        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"{abc_pin.VA} = {a_pin.V};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{abc_pin.VB} = {b_pin.V};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{abc_pin.VC} = {c_pin.V};"
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IA,
                Node1 = abc_pin.VA,
                Node2 = a_pin.V
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IB,
                Node1 = abc_pin.VB,
                Node2 = b_pin.V
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IC,
                Node1 = abc_pin.VC,
                Node2 = c_pin.V
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
            equations.Add(new EquationBlock
            {
                Equation = $"{abc_pin.VAre} = {a_pin.Vre};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{abc_pin.VAim} = {a_pin.Vim};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{abc_pin.VBre} = {b_pin.Vre};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{abc_pin.VBim} = {b_pin.Vim};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{abc_pin.VCre} = {c_pin.Vre};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{abc_pin.VCim} = {c_pin.Vim};"
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IAre,
                Node1 = abc_pin.VAre,
                Node2 = a_pin.Vre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IAim,
                Node1 = abc_pin.VAim,
                Node2 = a_pin.Vim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IBre,
                Node1 = abc_pin.VBre,
                Node2 = b_pin.Vre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IBim,
                Node1 = abc_pin.VBim,
                Node2 = b_pin.Vim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = ICre,
                Node1 = abc_pin.VCre,
                Node2 = c_pin.Vre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = ICim,
                Node1 = abc_pin.VCim,
                Node2 = c_pin.Vim
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateParameters(double frequency)
        {
            return new List<EquationBlock>();
        }
    }
    public class SteadyStateConnection3P1PModel : ISteadyStateElementModel
    {
        public ISteadyStateElement CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            return new Connection3P1P(elementNodes["abc"] as Pin3Phase, elementNodes["a"] as Pin1Phase, elementNodes["b"] as Pin1Phase, elementNodes["c"] as Pin1Phase);
        }
    }
}
