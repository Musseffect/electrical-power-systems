using System.Collections.Generic;

namespace ElectricalPowerSystems.Scheme.Transient
{
    public interface ITransientEventGenerator
    {
        List<TransientEvent> GenerateEvents(double t0, double t1);
    }
    public abstract class TransientEvent
    {
        double time;
        public double Time { get { return time; } }
        public TransientEvent(double time)
        {
            this.time = time;
        }
        public abstract bool Execute(TransientState stateValues);
        public abstract List<Equations.DAE.Parameter> GetParameters();
    }
}
