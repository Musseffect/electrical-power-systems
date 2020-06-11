using ElectricalPowerSystems.Equations.DAE;
using ElectricalPowerSystems.PowerModel.NewModel.Transient;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerModel.NewModel.Elements
{
    class ShortCircuitEvent : TransientEvent
    {
        string stateName;
        double newState;
        public ShortCircuitEvent(string stateName, double newState, double time) : base(time)
        {
            this.stateName = stateName;
            this.newState = newState;
        }
        public override bool Execute(TransientState stateValuesx)
        {
            return true;
        }
        public override List<Parameter> GetParameters()
        {
            return new List<Parameter>() { new Parameter(stateName, newState) };
        }
    }
    class ShortCircuit : Element,ISteadyStateElement, ITransientElement
    {
        Pin1Phase a_pin;
        Pin1Phase b_pin;
        Pin1Phase c_pin;
        Pin3Phase in_pin;
        Pin3Phase out_pin;
        public string I1Are { get { return $"I_{ID}1a_re"; } }
        public string I1Bre { get { return $"I_{ID}1b_re"; } }
        public string I1Cre { get { return $"I_{ID}1c_re"; } }
        public string I1Aim { get { return $"I_{ID}1a_im"; } }
        public string I1Bim { get { return $"I_{ID}1b_im"; } }
        public string I1Cim { get { return $"I_{ID}1c_im"; } }
        public string I2Are { get { return $"I_{ID}2a_re"; } }
        public string I2Bre { get { return $"I_{ID}2b_re"; } }
        public string I2Cre { get { return $"I_{ID}2c_re"; } }
        public string I2Aim { get { return $"I_{ID}2a_im"; } }
        public string I2Bim { get { return $"I_{ID}2b_im"; } }
        public string I2Cim { get { return $"I_{ID}2c_im"; } }
        public string I1A { get { return $"I_{ID}1a"; } }
        public string I1B { get { return $"I_{ID}1b"; } }
        public string I1C { get { return $"I_{ID}1c"; } }
        public string I2A { get { return $"I_{ID}2a"; } }
        public string I2B { get { return $"I_{ID}2b"; } }
        public string I2C { get { return $"I_{ID}2c"; } }
        public ShortCircuit(Pin3Phase in_pin,Pin3Phase out_pin,Pin1Phase a_pin,Pin1Phase b_pin,Pin1Phase c_pin):base()
        {
            this.in_pin = in_pin;
            this.out_pin = out_pin;
            this.a_pin = a_pin;
            this.b_pin = b_pin;
            this.c_pin = c_pin;
        }
        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"{a_pin.Vre} = {in_pin.VAre};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{a_pin.Vim} = {in_pin.VAim};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{b_pin.Vre} = {in_pin.VBre};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{b_pin.Vim} = {in_pin.VBim};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{c_pin.Vre} = {in_pin.VCre};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{c_pin.Vim} = {in_pin.VCim};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{a_pin.Vre} = {out_pin.VAre};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{a_pin.Vim} = {out_pin.VAim};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{b_pin.Vre} = {out_pin.VBre};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{b_pin.Vim} = {out_pin.VBim};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{c_pin.Vre} = {out_pin.VCre};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{c_pin.Vim} = {out_pin.VCim};"
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = I1Are,
                Node1 = in_pin.VAre,
                Node2 = a_pin.Vre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = I1Aim,
                Node1 = in_pin.VAim,
                Node2 = a_pin.Vim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = I1Bre,
                Node1 = in_pin.VBre,
                Node2 = b_pin.Vre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = I1Bim,
                Node1 = in_pin.VBim,
                Node2 = b_pin.Vim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = I1Cre,
                Node1 = in_pin.VCre,
                Node2 = c_pin.Vre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = I1Cim,
                Node1 = in_pin.VCim,
                Node2 = c_pin.Vim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = I2Are,
                Node1 = a_pin.Vre,
                Node2 = out_pin.VAre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = I2Aim,
                Node1 = a_pin.Vim,
                Node2 = out_pin.VAim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = I2Bre,
                Node1 = b_pin.Vre,
                Node2 = out_pin.VBre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = I2Bim,
                Node1 = b_pin.Vim,
                Node2 = out_pin.VBim
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = I2Cre,
                Node1 = c_pin.Vre,
                Node2 = out_pin.VCre
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = I2Cim,
                Node1 = c_pin.Vim,
                Node2 = out_pin.VCim
            });
            return equations;
        }
        List<EquationBlock> ISteadyStateElement.GenerateParameters(double frequency)
        {
            return new List<EquationBlock>();
        }
        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new EquationBlock
            {
                Equation = $"{a_pin.V} = {in_pin.VA};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{b_pin.V} = {in_pin.VB};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{c_pin.V} = {in_pin.VC};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{a_pin.V} = {out_pin.VA};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{b_pin.V} = {out_pin.VB};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"{c_pin.V} = {out_pin.VC};"
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = I1A,
                Node1 = in_pin.VA,
                Node2 = a_pin.V
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = I1B,
                Node1 = in_pin.VB,
                Node2 = b_pin.V
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = I1C,
                Node1 = in_pin.VC,
                Node2 = c_pin.V
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = I2A,
                Node1 = a_pin.V,
                Node2 = out_pin.VA
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = I2B,
                Node1 = b_pin.V,
                Node2 = out_pin.VB
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = I2C,
                Node1 = c_pin.V,
                Node2 = out_pin.VC
            });
            return equations;
        }
        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            return new List<EquationBlock>();
        }
    }
    class DynamicShortCircuit : Element, ITransientElement,ITransientEventGenerator
    {
        Pin1Phase a_pin;
        Pin1Phase b_pin;
        Pin1Phase c_pin;
        Pin3Phase in_pin;
        Pin3Phase out_pin;
        double startTime;
        double endTime;
        public string I1A { get { return $"I_{ID}1a"; } }
        public string I1B { get { return $"I_{ID}1b"; } }
        public string I1C { get { return $"I_{ID}1c"; } }
        public string I2A { get { return $"I_{ID}2a"; } }
        public string I2B { get { return $"I_{ID}2b"; } }
        public string I2C { get { return $"I_{ID}2c"; } }
        public string I3A { get { return $"I_{ID}3a"; } }
        public string I3B { get { return $"I_{ID}3b"; } }
        public string I3C { get { return $"I_{ID}3c"; } }
        public DynamicShortCircuit(double startTime,double endTime, Pin3Phase in_pin, Pin3Phase out_pin, Pin1Phase a_pin, Pin1Phase b_pin, Pin1Phase c_pin) : base()
        {
            this.startTime = startTime;
            this.endTime = endTime;
            this.in_pin = in_pin;
            this.out_pin = out_pin;
            this.a_pin = a_pin;
            this.b_pin = b_pin;
            this.c_pin = c_pin;
        }
        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            double stateValue = 0.0;
            equations.Add(new EquationBlock
            {
                Equation = $"parameter fault_state_{ID} = {stateValue.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }

        List<TransientEvent> ITransientEventGenerator.GenerateEvents(double t0, double t1)
        {
            return new List < TransientEvent>{ new ShortCircuitEvent($"fault_state_{ID}",1.0,this.startTime),new ShortCircuitEvent($"fault_state_{ID}", 0.0, this.endTime)};
            throw new NotImplementedException();
        }

        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            equations.Add(new CurrentFlowBlock
            {
                Equation = I1A,
                Node1 = in_pin.VA,
                Node2 = null
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = I1B,
                Node1 = in_pin.VB,
                Node2 = null
            });
            equations.Add(new CurrentFlowBlock
            {
                Equation = I1C,
                Node1 = in_pin.VC,
                Node2 = null
            });
           /* equations.Add(new EquationBlock
            {
                Equation = $"fault_state_{ID} = {stateValue.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"fault_state_{ID} = {stateValue.ToString(new CultureInfo("en-US"))};"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"fault_state_{ID} = {stateValue.ToString(new CultureInfo("en-US"))};"
            });*/
            return equations;
        }
    }
    class OpenCircuit : Element,ISteadyStateElement,ITransientElement
    {
        public string IA { get { return $"I_{ID}a"; } }
        public string IB { get { return $"I_{ID}b"; } }
        public string IC { get { return $"I_{ID}c"; } }
        public string IAre { get { return $"I_{ID}a_re"; } }
        public string IBre { get { return $"I_{ID}b_re"; } }
        public string ICre { get { return $"I_{ID}c_re"; } }
        public string IAim { get { return $"I_{ID}a_im"; } }
        public string IBim { get { return $"I_{ID}b_im"; } }
        public string ICim { get { return $"I_{ID}c_im"; } }

        List<EquationBlock> ISteadyStateElement.GenerateEquations()
        {
            throw new NotImplementedException();
        }

        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            throw new NotImplementedException();
        }

        List<EquationBlock> ISteadyStateElement.GenerateParameters(double frequency)
        {
            throw new NotImplementedException();
        }

        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            throw new NotImplementedException();
        }
    }
    class DynamicOpenCircuit : Element, ITransientElement, ITransientEventGenerator
    {
        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            throw new NotImplementedException();
        }

        List<TransientEvent> ITransientEventGenerator.GenerateEvents(double t0, double t1)
        {
            throw new NotImplementedException();
        }

        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            throw new NotImplementedException();
        }
    }
}
