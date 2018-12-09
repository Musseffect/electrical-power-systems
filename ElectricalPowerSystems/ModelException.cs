using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems
{
    class ModelException: Exception
    {
        public virtual string getMessage()
        {
            return this.Message;
        }
    }
    enum ParserError
    {
    }
    class ModeParserSyntaxException:ModelException
    {
        int line;
        int symbol;
        ParserError errorCode;
        static public string getErrorMessage(ParserError code)
        {
            return "Not implemented";
        }
        public override string getMessage()
        {
            return "["+line.ToString()+(symbol>-1?symbol.ToString():"")+"]: "+ getErrorMessage(errorCode);
        }
    }
}
