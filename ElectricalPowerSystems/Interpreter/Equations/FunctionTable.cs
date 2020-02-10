using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Interpreter.Equations
{
    public delegate ExpressionNode FunctionDerivative(List<ExpressionNode> node);
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
            { "exp", new FunctionEntry{ FuncName="exp",Exec=Exp,Der=new List<FunctionDerivative>{ExpDer },ArgNumber=1}},
            { "pi", new FunctionEntry{ FuncName="pi",Exec=Pi,Der=null,ArgNumber=0}},
            { "e", new FunctionEntry{ FuncName="e",Exec=E,Der=null,ArgNumber=0}},
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
            catch (Exception exc)
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
        static public ExpressionNode SinDer(List<ExpressionNode> args)
        {
            return new FunctionNode
            {
                FunctionName = "cos",
                Arguments = args
            };
        }
        static public ExpressionNode CosDer(List<ExpressionNode> args)
        {
            return new NegationNode
            {
                InnerNode = new FunctionEntryNode(functionTable["sin"], args)
            };
        }
        static public ExpressionNode TanDer(List<ExpressionNode> args)
        {
            return new DivisionNode
            {
                Left = new FloatNode
                {
                    Value = 1.0
                },
                Right = new FunctionEntryNode(functionTable["cos"], args)
            };
        }
        static public ExpressionNode CtgDer(List<ExpressionNode> args)
        {
            return new DivisionNode
            {
                Left = new FloatNode
                {
                    Value = -1.0
                },
                Right = new FunctionEntryNode(functionTable["sin"], args)
            };
        }
        static public ExpressionNode AtanDer(List<ExpressionNode> args)
        {
            return new DivisionNode
            {
                Left = new FloatNode
                {
                    Value = 1.0
                },
                Right = new AdditionNode
                {
                    Left = new FloatNode
                    {
                        Value = 1.0
                    },
                    Right = new PowerNode
                    {
                        Left = args[0],
                        Right = new FloatNode
                        {
                            Value = 2.0
                        }
                    },
                }
            };
        }
        static public ExpressionNode ActgDer(List<ExpressionNode> args)
        {
            return new NegationNode
            {
                InnerNode = new DivisionNode
                {
                    Left = new FloatNode
                    {
                        Value = 1.0
                    },
                    Right = new AdditionNode
                    {
                        Left = new FloatNode
                        {
                            Value = 1.0
                        },
                        Right = new PowerNode
                        {
                            Left = args[0],
                            Right = new FloatNode
                            {
                                Value = 2.0
                            }
                        }
                    }
                }
            };
        }
        static public ExpressionNode AcosDer(List<ExpressionNode> args)
        {
            return new NegationNode
            {
                InnerNode = new PowerNode
                {
                    Left = new SubtractionNode
                    {
                        Left = new FloatNode
                        {
                            Value = 1.0
                        },
                        Right = new PowerNode
                        {
                            Left = args[0],
                            Right = new FloatNode
                            {
                                Value = 2.0
                            }
                        }
                    },
                    Right = new FloatNode
                    {
                        Value = -0.5
                    }
                }
            };
        }
        static public ExpressionNode AsinDer(List<ExpressionNode> args)
        {
            return new PowerNode
            {
                Left = new SubtractionNode
                {
                    Left = new FloatNode
                    {
                        Value = 1.0
                    },
                    Right = new PowerNode
                    {
                        Left = args[0],
                        Right = new FloatNode
                        {
                            Value = 2.0
                        }
                    }
                },
                Right = new FloatNode
                {
                    Value = -0.5
                }
            };
        }
        static public ExpressionNode ExpDer(List<ExpressionNode> args)
        {
            return new FunctionEntryNode(functionTable["exp"], args);
        }
        static public ExpressionNode LnDer(List<ExpressionNode> args)
        {
            return new DivisionNode
            {
                Left = new FloatNode
                {
                    Value = 1.0
                },
                Right = args[0]
            };
        }
        static public ExpressionNode LogDer1(List<ExpressionNode> args)
        {
            return new DivisionNode
            {
                Left = new FloatNode
                {
                    Value = 1.0
                },
                Right = new MultiplicationNode
                {
                    Left = args[0],
                    Right = new FunctionEntryNode(functionTable["ln"], new List<ExpressionNode> { args[1] })
                }
            };
        }
        static public ExpressionNode LogDer2(List<ExpressionNode> args)
        {
            return new NegationNode
            {
                InnerNode = new DivisionNode
                {
                    Left = new MultiplicationNode
                    {
                        Left = new FunctionEntryNode(functionTable["ln"], new List<ExpressionNode> { args[0] }),
                        Right = args[0],
                    },
                    Right = new MultiplicationNode
                    {
                        Left = args[1],
                        Right = new MultiplicationNode
                        {
                            Left = new FunctionEntryNode(functionTable["ln"], new List<ExpressionNode> { args[1] }),
                            Right = new FunctionEntryNode(functionTable["ln"], new List<ExpressionNode> { args[1] })
                        }
                    },
                }
            };
        }
        static public ExpressionNode SqrtDer(List<ExpressionNode> args)
        {
            return new MultiplicationNode
            {
                Left = new FloatNode
                {
                    Value = -0.5
                },
                Right = new PowerNode
                {
                    Left = args[0],
                    Right = new FloatNode
                    {
                        Value = -0.5
                    }
                }
            };
        }
        static public ExpressionNode SqrDer(List<ExpressionNode> args)
        {
            return new MultiplicationNode
            {
                Left = new FloatNode
                {
                    Value = 2.0
                },
                Right = args[0]
            };
        }
        static public ExpressionNode PowDer1(List<ExpressionNode> args)
        {
            return new MultiplicationNode
            {
                Left = new PowerNode
                {
                    Left = args[0],
                    Right = args[1]
                },
                Right = new FunctionEntryNode(functionTable["ln"], new List<ExpressionNode> { args[0] })
            };
        }
        static public ExpressionNode PowDer2(List<ExpressionNode> args)
        {
            return new MultiplicationNode
            {
                Left = new PowerNode
                {
                    Left = args[0],
                    Right = new SubtractionNode
                    {
                        Left = args[1],
                        Right = new FloatNode
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
    class Variable : StackElement
    {
        public int index;
        public Variable(int index) : base(StackElementType.Variable)
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
    class Function : StackElement
    {
        FunctionEntry func;
        public Function(FunctionEntry func) : base(StackElementType.Function)
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
