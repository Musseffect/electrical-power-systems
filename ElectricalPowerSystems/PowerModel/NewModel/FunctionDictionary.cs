using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerModel.NewModel
{
    public partial class ModelInterpreter
    {
        public delegate Constant FunctionExec(List<Constant> args);
        public class ArgumentDescription
        {
            public Constant DefaultValue{ get; }
            public Constant.Type Type { get; }
            public ArgumentDescription(Constant.Type type,Constant defaultType = null)
            {
                Type = type;
                DefaultValue = defaultType;
            }
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
                    if (arg.DefaultValue != null)
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
            public FunctionExec Exec { get; private set; }
            public FunctionSignature Signature { get;private set; }
            public FunctionDefinition(FunctionExec exec, FunctionSignature signature)
            {
                Exec = exec;
                Signature = signature;
            }
            static public Constant Compute(FunctionDefinition f, List<Constant> variables)
            {
                List<Constant> args = new List<Constant>();
                if (variables.Count < f.Signature.Arguments.Count)
                {
                    if (f.Signature.defaultArgs >= f.Signature.Arguments.Count - variables.Count)
                    {
                        for (int i = 0; i < variables.Count; i++)
                        {
                            Constant var = Convert(variables[i], f.Signature.Arguments[i].Type);
                            args.Add(var);
                        }
                        for (int j = variables.Count; j < f.Signature.Arguments.Count; j++)
                        {
                            args.Add(f.Signature.Arguments[j].DefaultValue);
                        }
                        return f.Exec(args);
                    }
                    else
                        throw new Exception("Invalid number of arguments in function");
                }
                for (int i = 0; i < f.Signature.Arguments.Count; i++)
                {
                    Constant var = Convert(variables[i], f.Signature.Arguments[i].Type);
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
                { "radians", new List<FunctionDefinition>{
                    new FunctionDefinition
                    (
                        Radians,
                        new FunctionSignature(new List<ArgumentDescription>
                        {
                            new ArgumentDescription(Constant.Type.Float)
                        },false)
                    )
                    }
                },
                { "degrees", new List<FunctionDefinition>{
                    new FunctionDefinition
                    (
                        Degrees,
                        new FunctionSignature(new List<ArgumentDescription>
                        {
                            new ArgumentDescription(Constant.Type.Float)
                        },false)
                    )
                    }

                },
                { "complex", new List<FunctionDefinition>{
                    new FunctionDefinition
                    (
                        Complex,
                        new FunctionSignature(new List<ArgumentDescription>
                        {
                            new ArgumentDescription(Constant.Type.Float),
                            new ArgumentDescription(Constant.Type.Float)
                        },false)
                    )
                    }
                },
                { "complexExp", new List<FunctionDefinition>{
                    new FunctionDefinition
                    (
                        ComplexExp,
                        new FunctionSignature(new List<ArgumentDescription>
                        {
                            new ArgumentDescription(Constant.Type.Float),
                            new ArgumentDescription(Constant.Type.Float)
                        },false)
                    )
                    }
                },
                { "conj", new List<FunctionDefinition>{
                    new FunctionDefinition
                    (
                        Conj,
                        new FunctionSignature(new List<ArgumentDescription>
                        {
                            new ArgumentDescription(Constant.Type.Complex)
                        },false)
                    )
                    }
                },
                { "pow", new List<FunctionDefinition>{
                    new FunctionDefinition
                    (
                        Pow,
                        new FunctionSignature(new List<ArgumentDescription>
                        {
                            new ArgumentDescription(Constant.Type.Float),
                            new ArgumentDescription(Constant.Type.Float)
                        },false)
                    )
                    }
                },
                { "e", new List<FunctionDefinition>{
                    new FunctionDefinition
                    (
                        E,
                        new FunctionSignature(new List<ArgumentDescription>
                        {
                        },false)
                    )
                    }
                },
                { "pi", new List<FunctionDefinition>{
                    new FunctionDefinition
                    (
                        Pi,
                        new FunctionSignature(new List<ArgumentDescription>
                        {
                        },false)
                    )
                    }
                },
                { "re", new List<FunctionDefinition>{
                    new FunctionDefinition
                    (
                        Re,
                        new FunctionSignature(new List<ArgumentDescription>
                        {
                            new ArgumentDescription(Constant.Type.Float)
                        },false)
                    )
                    }
                },
                { "im", new List<FunctionDefinition>{
                    new FunctionDefinition
                    (
                        Im,
                        new FunctionSignature(new List<ArgumentDescription>
                        {
                            new ArgumentDescription(Constant.Type.Float)
                        },false)
                    )
                    }
                },
                { "arg", new List<FunctionDefinition>{
                    new FunctionDefinition
                    (
                        Arg,
                        new FunctionSignature(new List<ArgumentDescription>
                        {
                            new ArgumentDescription(Constant.Type.Float)
                        },false)
                    )
                    }
                },
                { "magn", new List<FunctionDefinition>{
                    new FunctionDefinition
                    (
                        Magn,
                        new FunctionSignature(new List<ArgumentDescription>
                        {
                            new ArgumentDescription(Constant.Type.Float)
                        },false)
                    )
                    }
                }
            };
        }
        private static Constant Complex(List<Constant> args)
        {
            FloatValue re = args[0] as FloatValue;
            FloatValue im = args[1] as FloatValue;
            return new ComplexValue(re.Value,im.Value);
        }

        private static Constant Pow(List<Constant> args)
        {
            FloatValue x = args[0] as FloatValue;
            FloatValue y = args[1] as FloatValue;
            return new FloatValue(Math.Pow(x.Value,y.Value));
        }
        private static Constant Radians(List<Constant> args)
        {
            FloatValue value = args[0] as FloatValue;
            return new FloatValue(MathUtils.MathUtils.Radians(value.Value));
        }
        private static Constant Degrees(List<Constant> args)
        {
            FloatValue value = args[0] as FloatValue;
            return new FloatValue(MathUtils.MathUtils.Degrees(value.Value));
        }
        private static Constant Magn(List<Constant> args)
        {
            ComplexValue c = args[0] as ComplexValue;
            return new FloatValue(c.Magn);
        }
        private static Constant Arg(List<Constant> args)
        {
            ComplexValue c = args[0] as ComplexValue;
            return new FloatValue(c.Phase);
        }
        private static Constant Re(List<Constant> args)
        {
            ComplexValue c = args[0] as ComplexValue;
            return new FloatValue(c.Re);
        }
        private static Constant Im(List<Constant> args)
        {
            ComplexValue c = args[0] as ComplexValue;
            return new FloatValue(c.Im);
        }
        private static Constant ComplexExp(List<Constant> args)
        {
            FloatValue magn = args[0] as FloatValue;
            FloatValue arg = args[1] as FloatValue;
            return ComplexValue.FromExp(magn.Value, arg.Value);
        }
        private static Constant Conj(List<Constant> args)
        {
            ComplexValue c = args[0] as ComplexValue;
            return new ComplexValue(c.Re, -c.Im);
        }
        private static Constant E(List<Constant> args)
        {
            return new FloatValue(Math.E);
        }
        private static Constant Pi(List<Constant> args)
        {
            return new FloatValue(Math.PI);
        }
    }
}
