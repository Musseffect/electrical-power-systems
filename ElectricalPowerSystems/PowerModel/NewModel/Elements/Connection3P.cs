using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;

namespace ElectricalPowerSystems.PowerModel.NewModel.Elements
{
    class Connection3P : Element, ITransientElement, ISteadyStateElement
    {
        Pin3Phase in_pin;
        Pin3Phase out_pin;
        public string IA { get { return $"I_{ID}a"; } }
        public string IB { get { return $"I_{ID}b"; } }
        public string IC { get { return $"I_{ID}c"; } }
        public string IAre { get { return $"I_{ID}a_re"; } }
        public string IAim { get { return $"I_{ID}a_im"; } }
        public string IBre { get { return $"I_{ID}b_re"; } }
        public string IBim { get { return $"I_{ID}b_im"; } }
        public string ICre { get { return $"I_{ID}c_re"; } }
        public string ICim { get { return $"I_{ID}c_im"; } }
        public Connection3P(Pin3Phase in_pin, Pin3Phase out_pin) : base()
        {
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
                Equation = $"{in_pin.VA} - {out_pin.VA} = 0;",
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VB} - {out_pin.VB} = 0;",
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VC} - {out_pin.VC} = 0;",
            });
            return equations;
        }

        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new CurrentFlowBlock
            {
                Equation = IAre,
                Node1 = in_pin.VAre,
                Node2 = out_pin.VAre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IAim,
                Node1 = in_pin.VAim,
                Node2 = out_pin.VAim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IBre,
                Node1 = in_pin.VBre,
                Node2 = out_pin.VBre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IBim,
                Node1 = in_pin.VBim,
                Node2 = out_pin.VBim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = ICre,
                Node1 = in_pin.VCre,
                Node2 = out_pin.VCre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = ICim,
                Node1 = in_pin.VCim,
                Node2 = out_pin.VCim
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VAre} - {out_pin.VAre} = 0;",
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VAim} - {out_pin.VAim} = 0;",
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VBre} - {out_pin.VBre} = 0;",
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VBim} - {out_pin.VBim} = 0;",
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VCre} - {out_pin.VCre} = 0;",
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{in_pin.VCim} - {out_pin.VCim} = 0;",
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
