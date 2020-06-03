#define RECLOSER
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using ElectricalPowerSystems.Equations.DAE;
using ElectricalPowerSystems.PowerModel.NewModel.Recloser;
using ElectricalPowerSystems.PowerModel.NewModel.Transient;
using MathNet.Numerics.LinearAlgebra;
using static ElectricalPowerSystems.PowerModel.NewModel.ModelInterpreter;

namespace ElectricalPowerSystems.PowerModel.NewModel.Elements
{
#if RECLOSER
    public class RecloserState
    {
        //ia,ib,ic
        //ua,ub,uc
        public double IA, IB, IC;
        public double UA, UB, UC;
        public double time;
        public bool currentState;
        public RecloserState(double IA, double IB, double IC,
            double UA, double UB, double UC,
            double time,
            bool currentState)
        {
            this.IA = IA;
            this.IB = IB;
            this.IC = IC;
            this.UA = UA;
            this.UB = UB;
            this.UC = UC;
            this.time = time;
            this.currentState = currentState;
        }
    }
    public interface IRecloserProgram
    {
        bool Execute(RecloserState state);
    }
    public class RecloserProgram: IRecloserProgram
    {
        Program program;
        VirtualMachine vm;
        public RecloserProgram(string programText,int programSizeLimit, int stackSize,int callStackSize)
        {
            //some black magic here
            Recloser.Compiler compiler = new Recloser.Compiler();
            AntlrInputStream inputStream = new AntlrInputStream(programText);
            RecloserGrammarLexer programLexer = new RecloserGrammarLexer(inputStream);
            ErrorListener<int> lexerErrorListener = new ErrorListener<int>();
            programLexer.RemoveErrorListeners();
            programLexer.AddErrorListener(lexerErrorListener);
            CommonTokenStream commonTokenStream = new CommonTokenStream(programLexer);
            RecloserGrammarParser programParser = new RecloserGrammarParser(commonTokenStream);
            ErrorListener<IToken> parserErrorListener = new ErrorListener<IToken>();
            programParser.RemoveErrorListeners();
            programParser.AddErrorListener(parserErrorListener);

            RecloserGrammarParser.ProgramContext programContext = programParser.program();
            List<ErrorMessage> errorList = new List<ErrorMessage>();
            errorList.AddRange(lexerErrorListener.GetErrors());
            errorList.AddRange(parserErrorListener.GetErrors());
            if (errorList.Count > 0)
            {
                foreach (var error in errorList)
                {
                    throw new Equations.CompilerException(errorList);
                }
                return;
            }
            Visitor visitor = new Visitor();
            Node root = visitor.VisitProgram(programContext);
            Recloser.Compiler.StructType recloserStateStruct = compiler.RegisterCustomType("recloserState", 
                new List<KeyValuePair<string,Recloser.Compiler.NonRefType>>
                {
                    new KeyValuePair<string, Recloser.Compiler.NonRefType>( "ua", new Recloser.Compiler.FloatType()),
                    new KeyValuePair<string, Recloser.Compiler.NonRefType>( "ub", new Recloser.Compiler.FloatType()),
                    new KeyValuePair<string, Recloser.Compiler.NonRefType>( "uc", new Recloser.Compiler.FloatType()),
                    new KeyValuePair<string, Recloser.Compiler.NonRefType>( "ia", new Recloser.Compiler.FloatType()),
                    new KeyValuePair<string, Recloser.Compiler.NonRefType>( "ib", new Recloser.Compiler.FloatType()),
                    new KeyValuePair<string, Recloser.Compiler.NonRefType>( "ic", new Recloser.Compiler.FloatType()),
                    new KeyValuePair<string, Recloser.Compiler.NonRefType>( "currentState", new Recloser.Compiler.BoolType()),
                    new KeyValuePair<string, Recloser.Compiler.NonRefType>( "time", new Recloser.Compiler.FloatType())
                }
            );
            compiler.Init(new List<Recloser.Compiler.NativeFunctionType>());
            program = compiler.Compile(root as ProgramNode, programSizeLimit);
            if (program.HasFunction("init", new Recloser.Compiler.Type []{ }, new Recloser.Compiler.VoidType()))
            {
                throw new Exception("Функция \"void init()\" не найдена в программе");
            }
            if (program.HasFunction("updateState", new Recloser.Compiler.Type[] { recloserStateStruct }, new Recloser.Compiler.BoolType()))
            {
                throw new Exception("Функция \"bool updateState()\" не найдена в программе");
            }
            vm = new VirtualMachine();
            vm.InitVM(stackSize, callStackSize, program);
            IValue result = vm.Execute("init",new Recloser.IValue[] { });
            //throw new NotImplementedException("RecloserProgram.Constructor");
        }
        bool IRecloserProgram.Execute(RecloserState state)
        {
            IValue result = vm.Execute("main", new Recloser.IValue[]{
                    new Float(state.UA),
                    new Float(state.UB),
                    new Float(state.UC),
                    new Float(state.IA),
                    new Float(state.IB),
                    new Float(state.IC),
                    new Int(state.currentState),
                    new Float(state.time)
                }
            );
            return (result as Int).BoolValue;
            //throw new NotImplementedException("RecloserProgram.Execute");
        }
    }
    public class RecloserProgramNative: IRecloserProgram
    {
        double timer;
        double waitTime;
        double lastEventTime;
        double restoreTriesTime;
        double iPeakMax;
        double pPeakMax;
        int tries;
        int tryCounter;
        public RecloserProgramNative(double iPeakMax,double pPeakMax,int tries,double waitTime,double restoreTriesTime,double t0)
        {
            this.iPeakMax = iPeakMax;
            this.pPeakMax = pPeakMax;
            this.tries = tries;
            this.waitTime = waitTime;
            this.restoreTriesTime = restoreTriesTime;
            lastEventTime = t0;
            timer = 0;
            tryCounter = 0;
        }
        bool IRecloserProgram.Execute(RecloserState state)
        {
            if (!state.currentState)//if turned off
            {
                if (tryCounter < tries)
                {
                    timer = (state.time - lastEventTime);
                    if (timer > waitTime)
                    {
                        tryCounter++;
                        lastEventTime = state.time;
                        return true;//try to turn on
                    }
                }
                return false;
            }
            double p = state.IA*state.UA+state.IB*state.UB+state.IC*state.UC;
            if (
                Math.Abs(state.IA) > iPeakMax ||
                Math.Abs(state.IB) > iPeakMax ||
                Math.Abs(state.IC) > iPeakMax ||
                Math.Abs(p) > pPeakMax
                )
            {
                lastEventTime = state.time;
                return false;
            }
            if (state.time - lastEventTime > restoreTriesTime)
            {
                tryCounter = 0;
            }
            return state.currentState;
        }
    }
    public class RecloserEvent:TransientEvent
    {
        Recloser3P recloser;
        public RecloserEvent(double time, Recloser3P recloser):base(time)
        {
            this.recloser = recloser;
        }
        public override bool Execute(TransientState transientState)
        {
            bool prevState = recloser.State;
            bool newState = recloser.GetNewState(transientState,Time);
            recloser.State = newState;
            return prevState != newState;
        }
        public override List<Parameter> GetParameters()
        {
            return recloser.GetParameters();
        }
    }
    public class Recloser3P : Element, ITransientElement, ITransientEventGenerator
    {
        bool state;
        IRecloserProgram program;
        double frequency;
        double t0;
        Pin3Phase in_pin;
        Pin3Phase out_pin;
        public string IA { get { return $"I_{ID}a"; } }
        public string IB { get { return $"I_{ID}b"; } }
        public string IC { get { return $"I_{ID}c"; } }
        public bool State { get { return state; } set { state = value; } }
        public Recloser3P(IRecloserProgram program, bool initialState, double frequency, double t0, Pin3Phase in_pin, Pin3Phase out_pin) : base()
        {
            this.program = program;
            this.state = initialState;
            this.frequency = frequency;
            this.t0 = t0;
            this.in_pin = in_pin;
            this.out_pin = out_pin;
        }
        public bool GetNewState(TransientState transientState,double time)
        {
            RecloserState recloserState = new RecloserState(
                transientState.GetValue(IA),
                transientState.GetValue(IB),
                transientState.GetValue(IC),
                transientState.GetValue(in_pin.VA),
                transientState.GetValue(in_pin.VB),
                transientState.GetValue(in_pin.VC),
                time,
                State);
            return program.Execute(recloserState);
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
                Equation = $"({in_pin.VA} - {out_pin.VA}) * state_{ID} = {IA} * (1.0 - state_{ID});"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"({in_pin.VB} - {out_pin.VB}) * state_{ID} = {IB} * (1.0 - state_{ID});"
            });
            equations.Add(new EquationBlock
            {
                Equation = $"({in_pin.VC} - {out_pin.VC}) * state_{ID} = {IC} * (1.0 - state_{ID});"
            });
            return equations;
        }
        public List<Parameter> GetParameters()
        {
            List<Parameter> parameters = new List<Parameter>();
            double state = this.state ? 1.0 : 0.0;
            parameters.Add(new Parameter($"state_{ID}",state));
            return parameters;
        }
        List<EquationBlock> ITransientElement.GenerateParameters()
        {
            List<EquationBlock> equations = new List<EquationBlock>();
            double state = this.state ? 1.0 : 0.0;
            equations.Add(new EquationBlock
            {
                Equation = $"parameter state_{ID} = {state.ToString(new CultureInfo("en-US"))};"
            });
            return equations;
        }
        List<TransientEvent> ITransientEventGenerator.GenerateEvents(double t0, double t1)
        {
            double dt = 1.0 / frequency;
            double t = Math.Max(t0, this.t0);
            List<TransientEvent> events = new List<TransientEvent>();
            while (t <= t1)
            {
                events.Add(new RecloserEvent(t, this));
                t += dt;
            }
            return events;
        }
    }
    public class TransientRecloser3PModel : ITransientElementModel
    {
        ITransientElement ITransientElementModel.CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            double frequency = (elementObject.GetValue("Frequency") as FloatValue).Value;
            bool initialState = (elementObject.GetValue("InitialState") as BoolValue).Value;
            double t0 = (elementObject.GetValue("T0") as FloatValue).Value;
            string programText = (elementObject.GetValue("Program") as StringValue).Value;
            RecloserProgram program;
            try
            {
                program = new RecloserProgram(programText,4096,4096/4,20);
            } catch (Exception exc)
            {
                throw exc;
            }
            return new Recloser3P(program, initialState, frequency, t0, elementNodes["in"] as Pin3Phase, elementNodes["out"] as Pin3Phase);
        }
    }
    public class TransientRecloserNative3PModel : ITransientElementModel
    {
        ITransientElement ITransientElementModel.CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes)
        {
            double frequency = (elementObject.GetValue("Frequency") as FloatValue).Value;
            bool initialState = (elementObject.GetValue("InitialState") as BoolValue).Value;
            double t0 = (elementObject.GetValue("T0") as FloatValue).Value;
            double iPeakMax = (elementObject.GetValue("CurrentPeakMax") as FloatValue).Value;
            double pPeakMAx = (elementObject.GetValue("PowerPeakMax") as FloatValue).Value;
            int tries = (elementObject.GetValue("Tries") as IntValue).Value;
            double waitTime = (elementObject.GetValue("WaitTime") as FloatValue).Value;
            double restoreTriesTime = (elementObject.GetValue("RestoreTriesTime") as FloatValue).Value;
            RecloserProgramNative program = new RecloserProgramNative(iPeakMax,pPeakMAx,tries,waitTime,restoreTriesTime,t0);
            return new Recloser3P(program, initialState, frequency, t0, elementNodes["in"] as Pin3Phase, elementNodes["out"] as Pin3Phase);
        }
    }
#endif
}
