using ElectricalPowerSystems.Equations.Nonlinear;
using ElectricalPowerSystems.Scheme.SteadyState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Scheme.Interpreter
{
    public class EquationBlock
    {
        public string Equation;
    }
    //left side of f(x)=I
    //for source voltage it is Ie=Ielement
    //for resistance it is Y(v1-v2)=Ielement
    //each current equation block creates two equation
    public class CurrentFlowBlock : EquationBlock
    {
        public string Node1;
        public string Node2;
    }
    public abstract class Element
    {
        int elementIndex;
        public int ID { get { return elementIndex; } }
        public Element(int elementIndex)
        {
            this.elementIndex = elementIndex;
        }
        public Element()
        {
            this.elementIndex = -1;
        }
        public void SetIndex(int value)
        {
            this.elementIndex = value;
        }
    }
    public interface ITransientElement
    {
        List<EquationBlock> GenerateEquations();
        List<EquationBlock> GenerateParameters();
    }
    public interface ISteadyStateElement
    {
        List<EquationBlock> GenerateEquations();
        List<EquationBlock> GenerateParameters(double frequency);
    }
    public interface IScopeElement
    {
        List<string> GetTransientVariableNames();
        IScopeReading GetReading(NonlinearSystemSolution solution);
        List<double> GetReading(Transient.TransientState solution);
        string FormatReadings(IScopeReading[] readings, double[] frequencies);
    }
    public interface IACSourceElement
    {
        double[] GetFrequencies();
    }
}