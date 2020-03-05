using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Interpreter.Equations.Expression
{
    public delegate Expression FunctionDerivative(List<Expression> node);
    public class FunctionEntry
    {
        public string FuncName { get; set; }
        public FunctionExec Exec { get; set; }
        public List<FunctionDerivative> Der { get; set; }
        public int ArgNumber { get; set; }
    }
    public class FunctionTable
    {
        static public void Init()
        {
            functionTable = new Dictionary<string, FunctionEntry>()
        {
            { "sin", new FunctionEntry{ FuncName="sin",Exec=Sin,Der=new List<FunctionDerivative>{SinDer},ArgNumber=1}},
            { "cos", new FunctionEntry{ FuncName="cos",Exec=Cos,Der=new List<FunctionDerivative>{CosDer },ArgNumber=1}},
            { "tan", new FunctionEntry{ FuncName="tan",Exec=Tan,Der=new List<FunctionDerivative>{TanDer},ArgNumber=1}},
            { "ctg", new FunctionEntry{ FuncName="ctg",Exec=Ctg,Der=new List<FunctionDerivative>{CtgDer },ArgNumber=1}},
            { "atan", new FunctionEntry{ FuncName="atan",Exec=Atan,Der=new List<FunctionDerivative>{AtanDer},ArgNumber=1}},
            { "actg", new FunctionEntry{ FuncName="actg",Exec=Actg,Der=new List<FunctionDerivative>{ActgDer},ArgNumber=1}},
            { "asin", new FunctionEntry{ FuncName="asin",Exec=Asin,Der=new List<FunctionDerivative>{AsinDer},ArgNumber=1}},
            { "acos", new FunctionEntry{ FuncName="acos",Exec=Acos,Der=new List<FunctionDerivative>{AcosDer},ArgNumber=1}},
            { "pi", new FunctionEntry{ FuncName="pi",Exec=Pi,Der=null,ArgNumber=0}},
            { "e", new FunctionEntry{ FuncName="e",Exec=E,Der=null,ArgNumber=0}},
            { "exp", new FunctionEntry{ FuncName="exp",Exec=Exp,Der=new List<FunctionDerivative>{ ExpDer},ArgNumber=1}},
            { "ln", new FunctionEntry{ FuncName="ln",Exec=Ln,Der=new List<FunctionDerivative>{LnDer },ArgNumber=1}},
            { "log", new FunctionEntry{ FuncName="log",Exec=Log,Der=new List<FunctionDerivative>{LogDer1,LogDer2 },ArgNumber=2}},
            { "sqrt", new FunctionEntry{ FuncName="sqrt",Exec=Sqrt,Der=new List<FunctionDerivative>{SqrtDer },ArgNumber=1}},
            { "sqr", new FunctionEntry{ FuncName="sqr",Exec=Sqr,Der=new List<FunctionDerivative>{SqrDer },ArgNumber=1}},
            { "pow", new FunctionEntry{ FuncName="pow",Exec=Pow,Der=new List<FunctionDerivative>{ PowDer1,PowDer2},ArgNumber=2}}
        };
        }
        static Dictionary<string, FunctionEntry> functionTable;
        static public bool isValidFunction(string functionName)
        {
            return functionTable.ContainsKey(functionName);
        }
        static public FunctionEntry getFunctionEntry(string functionName)
        {
            try
            {
                return functionTable[functionName];
            }
            catch (KeyNotFoundException)
            {
                throw new Exception("Incorrect function name [\"" + functionName + "\"].");
            }
        }
        static public Operand Sin(List<Operand> args)
        {
            return new Operand(Math.Sin(args[0].value));
        }
        static public Operand Cos(List<Operand> args)
        {
            return new Operand(Math.Cos(args[0].value));
        }
        static public Operand Tan(List<Operand> args)
        {
            return new Operand(Math.Tan(args[0].value));
        }
        static public Operand Ctg(List<Operand> args)
        {
            return new Operand(1.0/Math.Tan(args[0].value));
        }
        static public Operand Exp(List<Operand> args)
        {
            return new Operand(Math.Exp(args[0].value));
        }
        static public Operand Pow(List<Operand> args)
        {
            return new Operand(Math.Pow(args[0].value, args[1].value));
        }
        static public Operand Log(List<Operand> args)
        {
            return new Operand(Math.Log(args[0].value, args[1].value));
        }
        static public Operand Ln(List<Operand> args)
        {
            return new Operand(Math.Log(args[0].value));
        }
        static public Operand Asin(List<Operand> args)
        {
            return new Operand(Math.Asin(args[0].value));
        }
        static public Operand Acos(List<Operand> args)
        {
            return new Operand(Math.Acos(args[0].value));
        }
        static public Operand Atan(List<Operand> args)
        {
            return new Operand(Math.Atan(args[0].value));
        }
        static public Operand Actg(List<Operand> args)
        {
            return new Operand(Math.Atan2(1.0, args[0].value));
        }
        static public Operand E(List<Operand> args)
        {
            return new Operand(Math.E);
        }
        static public Operand Pi(List<Operand> args)
        {
            return new Operand(Math.PI);
        }
        static public Operand Sqrt(List<Operand> args)
        {
            return new Operand(Math.Sqrt(args[0].value));
        }
        static public Operand Sqr(List<Operand> args)
        {
            return new Operand(args[0].value * args[0].value);
        }
        static public Expression SinDer(List<Expression> args)
        {
            return new Function(functionTable["cos"], args);
        }
        static public Expression CosDer(List<Expression> args)
        {
            return new Negation
            {
                InnerNode = new Function(functionTable["sin"], args)
            };
        }
        static public Expression TanDer(List<Expression> args)
        {
            return new Division
            {
                Left = new Float
                {
                    Value = 1.0
                },
                Right = new Function(functionTable["cos"], args)
            };
        }
        static public Expression CtgDer(List<Expression> args)
        {
            return new Division
            {
                Left = new Float
                {
                    Value = -1.0
                },
                Right = new Function(functionTable["sin"], args)
            };
        }
        static public Expression AtanDer(List<Expression> args)
        {
            return new Division
            {
                Left = new Float
                {
                    Value = 1.0
                },
                Right = new Addition
                {
                    Left = new Float
                    {
                        Value = 1.0
                    },
                    Right = new Power
                    {
                        Left = args[0],
                        Right = new Float
                        {
                            Value = 2.0
                        }
                    },
                }
            };
        }
        static public Expression ActgDer(List<Expression> args)
        {
            return new Negation
            {
                InnerNode = new Division
                {
                    Left = new Float
                    {
                        Value = 1.0
                    },
                    Right = new Addition
                    {
                        Left = new Float
                        {
                            Value = 1.0
                        },
                        Right = new Power
                        {
                            Left = args[0],
                            Right = new Float
                            {
                                Value = 2.0
                            }
                        }
                    }
                }
            };
        }
        static public Expression AcosDer(List<Expression> args)
        {
            return new Negation
            {
                InnerNode = new Power
                {
                    Left = new Subtraction
                    {
                        Left = new Float
                        {
                            Value = 1.0
                        },
                        Right = new Power
                        {
                            Left = args[0],
                            Right = new Float
                            {
                                Value = 2.0
                            }
                        }
                    },
                    Right = new Float
                    {
                        Value = -0.5
                    }
                }
            };
        }
        static public Expression AsinDer(List<Expression> args)
        {
            return new Power
            {
                Left = new Subtraction
                {
                    Left = new Float
                    {
                        Value = 1.0
                    },
                    Right = new Power
                    {
                        Left = args[0],
                        Right = new Float
                        {
                            Value = 2.0
                        }
                    }
                },
                Right = new Float
                {
                    Value = -0.5
                }
            };
        }
        static public Expression ExpDer(List<Expression> args)
        {
            return new Function(functionTable["exp"], args);
        }
        static public Expression LnDer(List<Expression> args)
        {
            return new Division
            {
                Left = new Float
                {
                    Value = 1.0
                },
                Right = args[0]
            };
        }
        static public Expression LogDer1(List<Expression> args)
        {
            return new Division
            {
                Left = new Float
                {
                    Value = 1.0
                },
                Right = new Multiplication
                {
                    Left = args[0],
                    Right = new Function(functionTable["ln"], new List<Expression> { args[1] })
                }
            };
        }
        static public Expression LogDer2(List<Expression> args)
        {
            return new Negation
            {
                InnerNode = new Division
                {
                    Left = new Multiplication
                    {
                        Left = new Function(functionTable["ln"], new List<Expression> { args[0] }),
                        Right = args[0],
                    },
                    Right = new Multiplication
                    {
                        Left = args[1],
                        Right = new Multiplication
                        {
                            Left = new Function(functionTable["ln"], new List<Expression> { args[1] }),
                            Right = new Function(functionTable["ln"], new List<Expression> { args[1] })
                        }
                    },
                }
            };
        }
        static public Expression SqrtDer(List<Expression> args)
        {
            return new Multiplication
            {
                Left = new Float
                {
                    Value = -0.5
                },
                Right = new Power
                {
                    Left = args[0],
                    Right = new Float
                    {
                        Value = -0.5
                    }
                }
            };
        }
        static public Expression SqrDer(List<Expression> args)
        {
            return new Multiplication
            {
                Left = new Float
                {
                    Value = 2.0
                },
                Right = args[0]
            };
        }
        static public Expression PowDer1(List<Expression> args)
        {
            return new Multiplication
            {
                Left = new Power
                {
                    Left = args[0],
                    Right = args[1]
                },
                Right = new Function(functionTable["ln"], new List<Expression> { args[0] })
            };
        }
        static public Expression PowDer2(List<Expression> args)
        {
            return new Multiplication
            {
                Left = new Power
                {
                    Left = args[0],
                    Right = new Subtraction
                    {
                        Left = args[1],
                        Right = new Float
                        {
                            Value=1.0
                        }
                    }
                },
                Right = args[1]
            };
        }
    }
    public delegate Operand FunctionExec(List<Operand> args);
    public enum StackElementType
    {
        Operand = 0,
        Variable = 14,
        Addition = 1,
        Subtraction = 3,
        Multiplication = 5,
        Division = 7,
        Power = 9,
        Negation = 11,
        Function = 12
    }
    public abstract class StackElement
    {
        protected StackElementType type;
        public StackElementType Type { get { return type; } }
        public StackElement(StackElementType type)
        {
            this.type = type;
        }
    }
    public class Operand : StackElement
    {
        public double value;
        public Operand(double value) : base(StackElementType.Operand)
        {
            this.value = value;
        }
    }
    class StackVariable : StackElement
    {
        public int index;
        public StackVariable(int index) : base(StackElementType.Variable)
        {
            this.index = index;
        }
    }
    abstract class BinaryOperator : StackElement
    {
        public BinaryOperator(StackElementType type) : base(type) { }
        public abstract Operand exec(Operand op1, Operand op2);
    }
    abstract class UnaryOperator : StackElement
    {
        public UnaryOperator(StackElementType type) : base(type) { }
        public abstract Operand exec(Operand op);
    }
    class AdditionOperator : BinaryOperator
    {
        public AdditionOperator() : base(StackElementType.Addition) { }
        public override Operand exec(Operand op1, Operand op2)
        {
            return new Operand(op1.value + op2.value);
        }
    }
    class SubtractionOperator : BinaryOperator
    {
        public SubtractionOperator() : base(StackElementType.Subtraction) { }
        public override Operand exec(Operand op1, Operand op2)
        {
            return new Operand(op1.value - op2.value);
        }
    }
    class PowerOperator : BinaryOperator
    {
        public PowerOperator() : base(StackElementType.Power) { }
        public override Operand exec(Operand op1, Operand op2)
        {
            return new Operand(Math.Pow(op1.value, op2.value));
        }

    }
    class DivisionOperator : BinaryOperator
    {
        public DivisionOperator() : base(StackElementType.Division) { }
        public override Operand exec(Operand op1, Operand op2)
        {
            return new Operand(op1.value / op2.value);
        }

    }
    class MultiplicationOperator : BinaryOperator
    {
        public MultiplicationOperator() : base(StackElementType.Multiplication) { }
        public override Operand exec(Operand op1, Operand op2)
        {
            return new Operand(op1.value * op2.value);
        }

    }
    class NegationOperator : UnaryOperator
    {
        public NegationOperator() : base(StackElementType.Negation) { }
        public override Operand exec(Operand op)
        {
            op.value = -op.value;
            return op;
        }
    }
    class StackFunction : StackElement
    {
        FunctionEntry func;
        public StackFunction(FunctionEntry func) : base(StackElementType.Function)
        {
            this.func = func;
        }
        public void exec(Stack<Operand> operands)
        {
            //if you push 1, 2, 3, 4, 5 then ToList will give you 5, 4, 3, 2, 1
            Stack<Operand> arguments = new Stack<Operand>();
            for (int i = 0; i < func.ArgNumber; i++)
            {
                arguments.Push(operands.Pop());
            }
            List<Operand> list = arguments.ToList<Operand>();
            operands.Push(func.Exec(list));
        }
        public FunctionEntry getFunctionEntry()
        {
            return func;
        }
    }
}
