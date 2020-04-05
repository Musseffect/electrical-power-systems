#define MODELINTERPRETER
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Globalization;
using static ElectricalPowerSystems.PowerModel.NewModel.ModelInterpreter;

namespace ElectricalPowerSystems.PowerModel.NewModel.Elements
{
    class Resistor : Element, ITransientElement, ISteadyStateElement
    {
        float resistance;
        Pin1Phase in_pin;
        Pin1Phase out_pin;
        public Resistor(float resistance, Pin1Phase in_pin, Pin1Phase out_pin) : base()
        {
            this.resistance = resistance;
            this.in_pin = in_pin;
            this.out_pin = out_pin;
        }
        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            string G = $"G_{ID}";
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"({in_pin.Vre} - {out_pin.Vre}) * {G}",
                Node1 = in_pin.Vre,
                Node2 = out_pin.Vre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"({in_pin.Vim} - {out_pin.Vim}) * {G}",
                Node1 = in_pin.Vim,
                Node2 = out_pin.Vim
            });
            return equations;
        }
        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"set G_{ID} = {(1.0 / resistance).ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            string G = $"G_{ID}";
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"({in_pin.Vre} - {out_pin.Vre}) * {G}",
                Node1 = in_pin.Vre,
                Node2 = out_pin.Vre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = $"({in_pin.Vim} - {out_pin.Vim}) * {G}",
                Node1 = in_pin.Vim,
                Node2 = out_pin.Vim
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateParameters(double frequency)
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"constant G_{ID} = {(1.0 / resistance).ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
    }
    /*
     
            public float resistance;
            public Resistor(int node1, int node2,int index, float resistance) : base(node1, node2,index)
            {
                this.resistance = resistance;
                this.ElementType = ElementTypeEnum.Resistor;
            }
            public override List<EquationBlock> GetParametersAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new EquationBlock
                {
                    Equation = $"set G_{elementIndex} = {(1.0/resistance).ToString(new CultureInfo("en-US"))};"
                });
                return equations;
            }
            public override List<EquationBlock> GenerateEquationsAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                string G = $"G_{elementIndex}";
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"(v_{nodes[0]}_re * {G} - v_{nodes[1]}_re * {G})",
                    Node1 = $"v_{nodes[0]}_re",
                    Node2 = $"v_{nodes[1]}_re"
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"(v_{nodes[0]}_im * {G} - v_{nodes[1]}_im * {G})",
                    Node1 = $"v_{nodes[0]}_im",
                    Node2 = $"v_{nodes[1]}_im"
                });
                return equations;
            }
            public override List<EquationBlock> GetParametersTransient()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new EquationBlock
                {
                    Equation = $"set G_{elementIndex} = {(1.0 / resistance).ToString(new CultureInfo("en-US"))};"
                });
                return equations;
            }
            public override List<EquationBlock> GenerateEquationsTransient()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                string G = $"G_{elementIndex}";
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"(v_{nodes[0]} * {G} - v_{nodes[1]} * {G})",
                    Node1 = $"v_{nodes[0]}",
                    Node2 = $"v_{nodes[1]}"
                });
                return equations;
            }
            public override string ToString()
            {
                return $"Resistor{{n1 = {nodes[0]}, n2 = {nodes[1]}, resistance = {resistance} }}";
            }
            public override Complex32 GetCurrent(NonlinearSystemSolution acSolution, float frequency)
            {
                double dvRe = acSolution.getValue($"v_{nodes[0]}_re") - acSolution.getValue($"v_{nodes[1]}_re");
                double dvIm = acSolution.getValue($"v_{nodes[0]}_im") - acSolution.getValue($"v_{nodes[1]}_im");
                return new Complex32((float)dvRe, (float)dvIm) / resistance;
            }
         */
    public class SteadyStateResistorModel : ISteadyStateElementModel
    {
        ISteadyStateElement ISteadyStateElementModel.CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            double resistance = (elementObject.GetValue("R") as FloatValue).Value;
            return new Resistor((float)resistance, elementNodes["in"] as Pin1Phase, elementNodes["out"] as Pin1Phase);
        }
    }
    public class TransientResistorModel : ITransientElementModel
    {
        ITransientElement ITransientElementModel.CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            double resistance = (elementObject.GetValue("R") as FloatValue).Value;
            return new Resistor((float)resistance, elementNodes["in"] as Pin1Phase, elementNodes["out"] as Pin1Phase);
        }
    }
}