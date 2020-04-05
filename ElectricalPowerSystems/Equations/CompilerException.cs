using System;
using System.Collections.Generic;

namespace ElectricalPowerSystems.Equations
{
    public class CompilerException : Exception
    {
        public List<ErrorMessage> Errors { get; private set; }
        public CompilerException(List<ErrorMessage> errors)
        {
            Errors = errors;
        }
        public CompilerException(List<ErrorMessage> errors, string message) : base(message)
        {
            Errors = errors;
        }
    }
}
