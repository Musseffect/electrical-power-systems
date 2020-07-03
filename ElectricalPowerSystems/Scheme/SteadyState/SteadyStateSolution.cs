using ElectricalPowerSystems.Scheme.Interpreter;
using System;
using System.Collections.Generic;

namespace ElectricalPowerSystems.Scheme.SteadyState
{
    public class SteadyStateSolution
    {
        List<IScopeReading[]> scopeReadings;
        public SteadyStateSolution(int frequencyCount,int scopeElements)
        {
            this.scopeReadings = new List<IScopeReading[]>();
            for (int i = 0; i < scopeElements; i++)
            {
                scopeReadings.Add(new IScopeReading[frequencyCount]);
            }
        }
        public void Set(IScopeReading reading,int scopeIndex,int frequencyIndex)
        {
            this.scopeReadings[scopeIndex][frequencyIndex] = reading;
        }
        public string ToString(List<IScopeElement> scopes,double[]frequencies)
        {
            string result = "";
            for (int i = 0; i < scopes.Count; i++)
            {
                string scopeReading = scopes[i].FormatReadings(scopeReadings[i], frequencies);
                result += scopeReading + Environment.NewLine;
            }
            foreach (var readingSequence in scopeReadings)
            {
                /*string scopeReading = $"Scope {labels[index]}: {{";
                foreach (var reading in readingSequence)
                {
                    scopeReading += reading.ToString();
                }
                scopeReading += "}";*/
            }
            return result;
        }
    }
}
