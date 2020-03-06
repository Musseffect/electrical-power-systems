using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Interpreter.PowerModel
{
#if MODELINTERPRETER
    public partial class ModelInterpreter
    {
        public delegate Constant FunctionExec(List<Constant> args);
        public class ArgumentDescription
        {
            public Constant defaultValue = null;
            public Constant.Type type;
        }
        public class FunctionSignature
        {
            public List<ArgumentDescription> Arguments { get; set; }
            public bool variableLength;
            public int defaultArgs;
            public FunctionSignature(List<ArgumentDescription> args, bool variableLength)
            {
                this.variableLength = variableLength;
                this.Arguments = args;
                bool checkArg = false;
                defaultArgs = 0;
                foreach (ArgumentDescription arg in args)
                {
                    if (arg.defaultValue != null)
                    {
                        defaultArgs++;
                        checkArg = true;
                    }
                    else if (checkArg == true)
                    {
                        throw new Exception("Invalid function signature. Invalid default argument values placement.");
                    }
                }
            }
        }
        public class FunctionDefinition
        {
            public FunctionExec Exec { get; set; }
            public FunctionSignature Signature { get; set; }
            static public Constant Compute(FunctionDefinition f, List<Constant> variables)
            {
                List<Constant> args = new List<Constant>();
                if (variables.Count < f.Signature.Arguments.Count)
                {
                    if (f.Signature.defaultArgs >= f.Signature.Arguments.Count - variables.Count)
                    {
                        for (int i = 0; i < variables.Count; i++)
                        {
                            Constant var = Convert(variables[i], f.Signature.Arguments[i].type);
                            args.Add(var);
                        }
                        for (int j = variables.Count; j < f.Signature.Arguments.Count; j++)
                        {
                            args.Add(f.Signature.Arguments[j].defaultValue);
                        }
                        return f.Exec(args);
                    }
                    else
                        throw new Exception("Invalid number of arguments in function");
                }
                for (int i = 0; i < f.Signature.Arguments.Count; i++)
                {
                    Constant var = Convert(variables[i], f.Signature.Arguments[i].type);
                    args.Add(var);
                }
                if (variables.Count > f.Signature.Arguments.Count)
                {
                    if (f.Signature.variableLength)
                    {
                        for (int i = f.Signature.Arguments.Count; i < variables.Count; i++)
                            args.Add(variables[i]);
                    }
                    else
                        throw new Exception("Invalid number of arguments in function");
                }
                return f.Exec(args);
            }
        }
        public Dictionary<string, List<FunctionDefinition>> functionTable;
        private void InitFunctionDictionary()
        {
            functionTable = new Dictionary<string, List<FunctionDefinition>>()
            {
            };
        }
    }
#endif
}
