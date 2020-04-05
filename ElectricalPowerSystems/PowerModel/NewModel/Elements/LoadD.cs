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
    class LoadD : Element, ITransientElement, ISteadyStateElement
    {
        Pin3Phase abc_pin;
        Complex32 zAB;
        Complex32 zBC;
        Complex32 zCA;
        public string IAB { get { return $"I_{ID}a"; } }
        public string IBC { get { return $"I_{ID}b"; } }
        public string ICA { get { return $"I_{ID}c"; } }
        public string IABre { get { return $"I_{ID}ab_re"; } }
        public string IBCre { get { return $"I_{ID}bc_re"; } }
        public string ICAre { get { return $"I_{ID}ca_re"; } }
        public string IABim { get { return $"I_{ID}ab_im"; } }
        public string IBCim { get { return $"I_{ID}bc_im"; } }
        public string ICAim { get { return $"I_{ID}ca_im"; } }
        public LoadD(Complex32 zAB, Complex32 zBC, Complex32 zCA, Pin3Phase abc_pin) : base()
        {
            this.zAB = zAB;
            this.zBC = zBC;
            this.zCA = zCA;
            this.abc_pin = abc_pin;
        }
        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            throw new NotImplementedException();
        }
        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            throw new NotImplementedException();
        }
        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            string RAB = $"R_{ID}ab";
            string XAB = $"X_{ID}ab";
            string RBC = $"R_{ID}bc";
            string XBC = $"X_{ID}bc";
            string RCA = $"R_{ID}ca";
            string XCA = $"X_{ID}ca";

            equations.Add(new CurrentFlowBlock
            {
                Equation = IABre,
                Node1 = abc_pin.VAre,
                Node2 = abc_pin.VBre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IABim,
                Node1 = abc_pin.VAim,
                Node2 = abc_pin.VBim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IBCre,
                Node1 = abc_pin.VBre,
                Node2 = abc_pin.VCre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = IBCim,
                Node1 = abc_pin.VBim,
                Node2 = abc_pin.VCim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = ICAre,
                Node1 = abc_pin.VCre,
                Node2 = abc_pin.VAre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = ICAim,
                Node1 = abc_pin.VCim,
                Node2 = abc_pin.VAim
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{abc_pin.VAre} - {abc_pin.VBre} - ({IABre} * {RAB} - {IABim} * {XAB}) = 0;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{abc_pin.VAim} - {abc_pin.VBim} - ({IABre} * {XAB} + {IABim} * {RAB}) = 0;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{abc_pin.VBre} - {abc_pin.VCre} - ({IBCre} * {RBC} - {IBCim} * {XBC}) = 0;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{abc_pin.VBim} - {abc_pin.VCim} - ({IBCre} * {XBC} + {IBCim} * {RBC}) = 0;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{abc_pin.VCre} - {abc_pin.VAre} - ({ICAre} * {RCA} - {ICAim} * {XCA}) = 0;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{abc_pin.VCim} - {abc_pin.VAim} - ({ICAre} * {XCA} + {ICAim} * {RCA}) = 0;"
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateParameters(double frequency)
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"constant R_{ID}ab = {zAB.Real.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant X_{ID}ab = {(zAB.Imaginary * frequency).ToString(new CultureInfo("en-US"))}/baseFrequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant R_{ID}bc = {zBC.Real.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant X_{ID}bc = {(zBC.Imaginary * frequency).ToString(new CultureInfo("en-US"))}/baseFrequency;"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant R_{ID}ca = {zCA.Real.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"constant X_{ID}ca = {(zCA.Imaginary * frequency).ToString(new CultureInfo("en-US"))}/baseFrequency;"
            });
            return equations;
        }
    }
    public class SteadyStateLoadDModel : ISteadyStateElementModel
    {
        public ISteadyStateElement CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            Complex32 zab = (elementObject.GetValue("ZAB") as ComplexValue).Value;
            Complex32 zbc = (elementObject.GetValue("ZBC") as ComplexValue).Value;
            Complex32 zca = (elementObject.GetValue("ZCA") as ComplexValue).Value;
            return new LoadD(zab, zbc, zca, elementNodes["in"] as Pin3Phase);
        }
    }
}
