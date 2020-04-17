using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ElectricalPowerSystems.PowerModel.NewModel.ModelInterpreter;

namespace ElectricalPowerSystems.PowerModel.NewModel.Elements
{
    class Transformer : Element, ITransientElement, ISteadyStateElement
    {
        float k;
        Pin1Phase inp_pin;
        Pin1Phase ins_pin;
        Pin1Phase outp_pin;
        Pin1Phase outs_pin;
        public string Ip { get { return $"I_{ID}p"; } }
        public string Ipre { get { return $"I_{ID}p_re"; } }
        public string Ipim { get { return $"I_{ID}p_im"; } }
        public string Is { get { return $"I_{ID}s"; } }
        public string Isre { get { return $"I_{ID}s_re"; } }
        public string Isim { get { return $"I_{ID}s_im"; } }
        public Transformer(float k,Pin1Phase inp_pin, Pin1Phase ins_pin, Pin1Phase outp_pin, Pin1Phase outs_pin):base()
        {
            this.k = k;
            this.inp_pin = inp_pin;
            this.ins_pin = ins_pin;
            this.outp_pin = outp_pin;
            this.outs_pin = outs_pin;
        }
        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new CurrentFlowBlock
            {
                Equation = Ip,
                Node1 = inp_pin.V,
                Node2 = outp_pin.V
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = Is,
                Node1 = ins_pin.V,
                Node2 = outs_pin.V
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{Ip} = k_{ID} * {Is};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"k_{ID} * ({inp_pin.V} - {outp_pin.V}) = {ins_pin.V} - {outp_pin.V};"
            });
            return equations;
        }
        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"constant k_{ID} = {k.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new CurrentFlowBlock
            {
                Equation = Ipre,
                Node1 = inp_pin.Vre,
                Node2 = outp_pin.Vre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = Ipim,
                Node1 = inp_pin.Vim,
                Node2 = outp_pin.Vim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = Isre,
                Node1 = ins_pin.Vre,
                Node2 = outs_pin.Vre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = Isim,
                Node1 = inp_pin.Vim,
                Node2 = outp_pin.Vim
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{Ipre} = k_{ID} * {Isre};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{Ipim} = k_{ID} * {Isim};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"k_{ID} * ({inp_pin.Vre} - {outp_pin.Vre}) = {ins_pin.Vre} - {outp_pin.Vre};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"k_{ID} * ({inp_pin.Vim} - {outp_pin.Vim}) = {ins_pin.Vim} - {outp_pin.Vim};"
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateParameters(double frequency)
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"constant k_{ID} = {k.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
    }
    public class SteadyStateTransformerModel : ISteadyStateElementModel
    {
        public ISteadyStateElement CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            double k = (elementObject.GetValue("K") as FloatValue).Value;
            return new Transformer((float) k, elementNodes["in_p"] as Pin1Phase, elementNodes["in_s"] as Pin1Phase, elementNodes["out_p"] as Pin1Phase, elementNodes["out_s"] as Pin1Phase);
        }
    }
    public class TransientTransformerModel : ITransientElementModel
    {
        ITransientElement ITransientElementModel.CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            double k = (elementObject.GetValue("K") as FloatValue).Value;
            return new Transformer((float)k, elementNodes["in_p"] as Pin1Phase, elementNodes["in_s"] as Pin1Phase, elementNodes["out_p"] as Pin1Phase, elementNodes["out_s"] as Pin1Phase);
        }
    }
}
