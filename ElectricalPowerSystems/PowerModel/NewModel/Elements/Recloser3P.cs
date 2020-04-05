using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace ElectricalPowerSystems.PowerModel.NewModel.Elements
{
    public interface ITransientEventGenerator
    {
        List<TransientEvent> GenerateEvents(double t0,double t1);
    }
#if RECLOSER
    public class RecloserState
    {
        public double IA, IB, IC;
        public double UA, UB, UC;
        public double time;
        public bool currentState;
        //ia,ib,ic
        //ua,ub,uc
    }
    public class RecloserProgram
    {
        public RecloserProgram(string program)
        {
            //some black magic here
        }
        public bool Execute(RecloserState state)
        {
            throw new NotImplementedException("RecloserProgram.Execute");
        }
    }
    public class RecloserEvent:TransientEvent
    {
        Recloser3P recloser;
        double time;
        public RecloserEvent(double time, Recloser3P recloser)
        {
            this.time = time;
            this.recloser = recloser;
        }
        public override bool Execute(Vector<double> x)
        {
            bool prevState = recloser.State;
            RecloserState recloserState;
            bool newState = recloser.GetNewState(recloserState);
            recloser.State = newState;
            return prevState == newState;
            throw new NotImplementedException();
        }
        public override List<Parameter> GetParameters()
        {
            return recloser.GetParameters();
            throw new NotImplementedException();
        }
    }
    public class Recloser3P : Element, ITransientElement, ITransientEventGenerator
    {
        bool state;
        RecloserProgram program;
        double frequency;
        Pin3Phase in_pin;
        Pin3Phase out_pin;
        public bool State { get { return state; } set { state = value; } }
        public Recloser3P(RecloserProgram program, bool initialState, double frequency, Pin3Phase in_pin, Pin3Phase out_pin) : base()
        {
            this.program = program;
            this.state = initialState;
            this.frequency = frequency;
            this.in_pin = in_pin;
            this.out_pin = out_pin;
        }
        bool GetNewState(RecloserState recloserState)
        {
            return program.Execute(recloserState);
        }
        List<EquationBlock> ITransientElement.GenerateEquations()
        {
            throw new NotImplementedException();
        }
        public List<Parameter> GetParameters()
        {
            List<Parameter> parameters = new List<Parameter>();
            double state = state ? 1.0 : 0.0;
            parameters.Add(new Parameter($"state_{ID}",state..ToString(new CultureInfo("en-US"))));
            return parameters;
        }
        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            double state = state ? 1.0 : 0.0;
            equations.Add(new EquationBlock
            {
                Equation = $"parameter state_{ID} = {state.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
        List<TransientEvent> GenerateEvents(double t0,double t1)
        {
            double dt = 1.0 / frequency;
            double t = t0;
            List<TransientEvent> events = new List<TransientEvent>();
            while (t <= t1)
            {
                events.Add(new RecloserEvent(t,this));
                t += dt;
            }
            return events;
        }
    }
#endif
}
