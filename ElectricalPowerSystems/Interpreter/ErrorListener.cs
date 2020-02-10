using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Interpreter
{
    class ErrorListener<T> : IAntlrErrorListener<T>
    {
        List<ErrorMessage> errors;
        public ErrorListener()
        {
            this.errors = new List<ErrorMessage>();
        }
        public List<ErrorMessage> getErrors()
        {
            return this.errors;
        }
        public void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] T offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            Console.WriteLine(msg);
            errors.Add(new ErrorMessage(msg, line, charPositionInLine));
        }
    }
}
