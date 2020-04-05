using ElectricalPowerSystems.Equations.Nonlinear;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ElectricalPowerSystems.PowerModel.NewModel.ModelInterpreter;

namespace ElectricalPowerSystems.PowerModel.NewModel.Elements
{
    class Scope3P:Element, ITransientElement, ISteadyStateElement, IScopeElement
    {
        Pin3Phase in_pin;
        Pin3Phase out_pin;
        string label;
        public string IA { get { return $"I_{ID}a"; } }
        public string IB { get { return $"I_{ID}b"; } }
        public string IC { get { return $"I_{ID}c"; } }
        public string IAre { get { return $"I_{ID}a_re"; } }
        public string IAim { get { return $"I_{ID}a_im"; } }
        public string IBre { get { return $"I_{ID}b_re"; } }
        public string IBim { get { return $"I_{ID}b_im"; } }
        public string ICre { get { return $"I_{ID}c_re"; } }
        public string ICim { get { return $"I_{ID}c_im"; } }
        public Scope3P(Pin3Phase in_pin,Pin3Phase out_pin,string label):base()
        {
            this.in_pin = in_pin;
            this.out_pin = out_pin;
            this.label = label;
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
        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            return new List<EquationBlock>();
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
        List<EquationBlock> ISteadyStateElement.GenerateParameters(double frequency)
        {
            return new List<EquationBlock>();
        }
        string IScopeElement.FormatReadings(IScopeReading[] readings, double[] frequencies)
        {
            string vA = "A: ";
            string vB = "B: ";
            string vC = "C: ";
            string vAB = "AB: ";
            string vBC = "BC: ";
            string vCA = "CA: ";
            string iA = "A: ";
            string iB = "B: ";
            string iC = "C: ";
            string pA = "A: ";
            string pB = "B: ";
            string pC = "C: ";
            string p = "total: ";
            for (int i = 0; i < readings.Length; i++)
            {
                double frequency = frequencies[i];
                ScopeReading3P reading = readings[i] as ScopeReading3P;
                if (i != 0)
                {
                    vA += " + ";
                    vB += " + ";
                    vC += " + ";
                    vAB += " + ";
                    vBC += " + ";
                    vCA += " + ";
                    iA += " + ";
                    iB += " + ";
                    iC += " + ";
                    pA += " + ";
                    pB += " + ";
                    pC += " + ";
                    p += " + ";
                }
                vA += $"{reading.VA}={reading.VA.Magnitude}@{MathUtils.MathUtils.Degrees(reading.VA.Phase)} [{frequency} Hz]";
                vB += $"{reading.VB}={reading.VB.Magnitude}@{MathUtils.MathUtils.Degrees(reading.VB.Phase)} [{frequency} Hz]";
                vC += $"{reading.VC}={reading.VC.Magnitude}@{MathUtils.MathUtils.Degrees(reading.VC.Phase)} [{frequency} Hz]";
                vAB += $"{reading.VAB}={reading.VAB.Magnitude}@{MathUtils.MathUtils.Degrees(reading.VAB.Phase)} [{frequency} Hz]";
                vBC += $"{reading.VBC}={reading.VBC.Magnitude}@{MathUtils.MathUtils.Degrees(reading.VBC.Phase)} [{frequency} Hz]";
                vCA += $"{reading.VCA}={reading.VCA.Magnitude}@{MathUtils.MathUtils.Degrees(reading.VCA.Phase)} [{frequency} Hz]";
                iA += $"{reading.IA}={reading.IA.Magnitude}@{MathUtils.MathUtils.Degrees(reading.IA.Phase)} [{frequency} Hz]";
                iB += $"{reading.IB}={reading.IB.Magnitude}@{MathUtils.MathUtils.Degrees(reading.IB.Phase)} [{frequency} Hz]";
                iC += $"{reading.IC}={reading.IC.Magnitude}@{MathUtils.MathUtils.Degrees(reading.IC.Phase)} [{frequency} Hz]";
                pA += $"{reading.PA}={reading.PA.Magnitude}@{MathUtils.MathUtils.Degrees(reading.PA.Phase)} [{frequency} Hz]";
                pB += $"{reading.PB}={reading.PB.Magnitude}@{MathUtils.MathUtils.Degrees(reading.PB.Phase)} [{frequency} Hz]";
                pC += $"{reading.PC}={reading.PC.Magnitude}@{MathUtils.MathUtils.Degrees(reading.PC.Phase)} [{frequency} Hz]";
                p += $"{reading.P}={reading.P.Magnitude}@{MathUtils.MathUtils.Degrees(reading.P.Phase)} [{frequency} Hz]";
            }
            string result = $"scope {label}: {Environment.NewLine}{{{Environment.NewLine}";
            result += $"\tvoltage: {{{Environment.NewLine}";
            result += $"\t\t{vA},{Environment.NewLine}";
            result += $"\t\t{vB},{Environment.NewLine}";
            result += $"\t\t{vC},{Environment.NewLine}";
            result += $"\t\t{vAB},{Environment.NewLine}";
            result += $"\t\t{vBC},{Environment.NewLine}";
            result += $"\t\t{vCA}{Environment.NewLine}";
            result += $"\t}},{Environment.NewLine}";
            result += $"\tcurrent: {{{Environment.NewLine}";
            result += $"\t\t{iA},{Environment.NewLine}";
            result += $"\t\t{iB},{Environment.NewLine}";
            result += $"\t\t{iC}{Environment.NewLine}";
            result += $"\t}},{Environment.NewLine}";
            result += $"\tpower: {{{Environment.NewLine}";
            result += $"\t\t{pA},{Environment.NewLine}";
            result += $"\t\t{pB},{Environment.NewLine}";
            result += $"\t\t{pC}{Environment.NewLine}";
            result += $"\t\t{p}{Environment.NewLine}";
            result += $"\t}}{Environment.NewLine}";
            result += $"}}";
            return result;
            //return $"scope {label}: {Environment.NewLine}{{{Environment.NewLine}\tvoltage: {{ {vA}, {vB}, {vC}, {vAB}, {vBC}, {vCA} }}, {Environment.NewLine}current: {{ {iA}, {iB}, {iC} }}, {Environment.NewLine}power: {{ {pA}, {pB}, {pC}, {p}}} {Environment.NewLine}}}";

        }
        IScopeReading IScopeElement.GetReading(NonlinearSystemSolution solution)
        {
            return new ScopeReading3P(
                new Complex32((float)solution.GetValue(in_pin.VAre), (float)solution.GetValue(in_pin.VAim)),
                new Complex32((float)solution.GetValue(in_pin.VBre), (float)solution.GetValue(in_pin.VBim)),
                new Complex32((float)solution.GetValue(in_pin.VCre), (float)solution.GetValue(in_pin.VCim)),
                new Complex32((float)solution.GetValue(IAre), (float)solution.GetValue(IAim)),
                new Complex32((float)solution.GetValue(IBre), (float)solution.GetValue(IBim)),
                new Complex32((float)solution.GetValue(ICre), (float)solution.GetValue(ICim))
                );
        }
    }
    public class SteadyStateScope3PModel : ISteadyStateElementModel
    {
        ISteadyStateElement ISteadyStateElementModel.CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            string label = (elementObject.GetValue("Label") as StringValue).Value;
            return new Scope3P(elementNodes["in"] as Pin3Phase, elementNodes["out"] as Pin3Phase,label);
        }
    }
    public class ScopeReading3P:IScopeReading
    {
        Complex32 vA;
        Complex32 vB;
        Complex32 vC;
        Complex32 iA;
        Complex32 iB;
        Complex32 iC;

        public Complex32 VA { get { return vA; } }
        public Complex32 VB { get { return vB; } }
        public Complex32 VC { get { return vC; } }
        public Complex32 VAB { get { return vA-vB; } }
        public Complex32 VBC { get { return vB-vC; } }
        public Complex32 VCA { get { return vC-vA; } }
        public Complex32 IA { get { return iA; } }
        public Complex32 IB { get { return iB; } }
        public Complex32 IC { get { return iC; } }
        public Complex32 PA { get { return vA * iA.Conjugate(); } }
        public Complex32 PB { get { return vB * iB.Conjugate(); } }
        public Complex32 PC { get { return vC * iC.Conjugate(); } }
        public Complex32 P { get { return PA+PB+PC; } }
        public ScopeReading3P(Complex32 vA, Complex32 vB,Complex32 vC,Complex32 iA,Complex32 iB,Complex32 iC)
        {
            this.vA = vA;
            this.vB = vB;
            this.vC = vC;
            this.iA = iA;
            this.iB = iB;
            this.iC = iC;
        }
    }
}
