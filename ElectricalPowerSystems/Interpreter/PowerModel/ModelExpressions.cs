//#define MODELINTERPRETER
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
        public static Dictionary<string, Constant> variableTable;
        public abstract class Value
        {
            public abstract Constant GetRValue();
        }
        public abstract class LValue : Value
        {
            public abstract void SetValue(Constant value);
        }
        public class LValueIdentifier : LValue
        {
            string key;
            public LValueIdentifier(string key)
            {
                this.key = key;
            }
            public override void SetValue(Constant value)
            {
                if (value is VoidValue)
                    throw new ModelInterpreterException("Cannot assign void.");
                variableTable[key] = value;
            }
            public override Constant GetRValue()
            {
                try
                {
                    Constant obj = variableTable[key];
                    return obj;
                }
                catch (KeyNotFoundException exc)
                {
                    throw new Exception("Undefined variable \"" + key + "\"");
                }
            }
        }
        public abstract class Constant: Value
        {
            public enum Type
            {
                Float,
                Int,
                Complex,
                Bool,
                Object,
                Array,
                String,
                Void
            };
            Type type;
            public Type ConstantType { get { return type; } }
            public Constant(Type type)
            {
                this.type = type;
            }
            public override Constant GetRValue()
            {
                return this;
            }
            public abstract StringValue CastToString();
        }
        public class VoidValue : Constant
        {
            public VoidValue() : base(Type.Void)
            {
            }
            public override StringValue CastToString()
            {
                return new StringValue("void");
            }
        }
        public class FloatValue : Constant
        {
            public double Value { get; set; }
            public FloatValue() : base(Type.Float)
            {
            }
            public FloatValue(double value):base(Type.Float)
            {
                this.Value = value;
            }
            public static FloatValue opAdd(FloatValue a, FloatValue b)
            {
                return new FloatValue(a.Value + b.Value);
            }
            public static FloatValue opSub(FloatValue a, FloatValue b)
            {
                return new FloatValue(a.Value - b.Value);
            }
            public static FloatValue opMult(FloatValue a, FloatValue b)
            {
                return new FloatValue(a.Value * b.Value);
            }
            public static FloatValue opDiv(FloatValue a, FloatValue b)
            {
                return new FloatValue(a.Value / b.Value);
            }
            public static FloatValue opPow(FloatValue a, FloatValue b)
            {
                return new FloatValue(Math.Pow(a.Value, b.Value));
            }
            public static FloatValue opNeg(FloatValue a)
            {
                return new FloatValue(-a.Value);
            }
            public override StringValue CastToString()
            {
                return new StringValue(Value.ToString(new System.Globalization.CultureInfo("en-US")));
            }
        }
        public class IntValue : Constant
        {
            public int Value { get; set; }
            public IntValue() : base(Type.Int)
            {
            }
            public IntValue(int value) : base(Type.Int)
            {
                this.Value = value;
            }
            public override StringValue CastToString()
            {
                return new StringValue(Value.ToString());
            }
            public static IntValue opAdd(IntValue a, IntValue b)
            {
                return new IntValue(a.Value + b.Value);
            }
            public static IntValue opSub(IntValue a, IntValue b)
            {
                return new IntValue(a.Value - b.Value);
            }
            public static IntValue opMult(IntValue a, IntValue b)
            {
                return new IntValue(a.Value * b.Value);
            }
            public static IntValue opDiv(IntValue a, IntValue b)
            {
                return new IntValue(a.Value / b.Value);
            }
            public static IntValue opNeg(IntValue a)
            {
                return new IntValue(-a.Value);
            }
        }
        public class BoolValue : Constant
        {
            public bool Value { get; set; }
            public BoolValue() : base(Type.Bool)
            {
            }
            public BoolValue(bool value):base(Type.Bool)
            {
                this.Value = value;
            }
            public static BoolValue opNot(BoolValue a)
            {
                return new BoolValue(!a.Value);
            }
            public static BoolValue opOr(BoolValue a, BoolValue b)
            {
                return new BoolValue(a.Value || b.Value);
            }
            public static BoolValue opAnd(BoolValue a, BoolValue b)
            {
                return new BoolValue(a.Value && b.Value);
            }
            public override StringValue CastToString()
            {
                return new StringValue(Value.ToString());
            }
        }
        public class ComplexValue : Constant
        {
            double re;
            double im;
            public double Re { get { return re; } set { re = value; } }
            public double Im { get { return im; } set { im = value; } }
            public double Magn { get { return Math.Sqrt(re * re + im * im); } }
            public double Phase { get { return Math.Atan2(im, re); } }
            public ComplexValue():base(Type.Complex)
            {
            }
            public ComplexValue(double re, double im) : base(Type.Complex)
            {
                this.re = re;
                this.im = im;
            }
            public static ComplexValue opAdd(ComplexValue a, ComplexValue b)
            {
                return new ComplexValue(a.Re + b.Re, a.Im + b.Im);
            }
            public static ComplexValue opSub(ComplexValue a, ComplexValue b)
            {
                return new ComplexValue(a.Re + b.Re, a.Im + b.Im);
            }
            public static ComplexValue opMult(ComplexValue a, ComplexValue b)
            {
                return new ComplexValue(a.Re * b.Re - a.Im * b.Im, a.Re * b.Im + a.Im * b.Re);
            }
            public static ComplexValue opDiv(ComplexValue a, ComplexValue b)
            {
                double l = b.Re * b.Re + b.Im + b.Im;
                return new ComplexValue((a.Re * b.Re - a.Im * b.Im) / l, (-a.Re * b.Im + a.Im * b.Re) / l);
            }
            public static ComplexValue opNeg(ComplexValue a)
            {
                return new ComplexValue(-a.Re, -a.Im);
            }
            public override StringValue CastToString()
            {
                return new StringValue(Magn.ToString() + "@" + Utils.Degrees(Phase).ToString());
            }
        }
        public class StringValue : Constant
        {
            public string Value;
            public StringValue() : base(Type.String)
            {
            }
            public StringValue(string value) : base(Type.String)
            {
                this.Value = value;
            }
            public static StringValue opAdd(StringValue a, StringValue b)
            {
                return new StringValue(a.Value + b.Value);
            }
            public override StringValue CastToString()
            {
                return this;
            }
        }
        public class Object : Constant
        {
            public string Name;
            public Dictionary<string, Constant> Values;
            public Object() : base(Type.Object)
            {
            }
            public override StringValue CastToString()
            {
                return new StringValue("object "+Name);
            }
        }
        public class Array : Constant
        {
            public Constant[] Values;
            public Array() : base(Type.Array)
            {
            }
            public override StringValue CastToString()
            {
                return new StringValue("array");
            }
        }
        static public Constant.Type Max(Constant.Type a, Constant.Type b)
        {
            if (a == b)
                return a;
            if (a == Constant.Type.Object || a == Constant.Type.Array || b == Constant.Type.Object || b == Constant.Type.Array)
                throw new Exception("Invalid type conversion.");
            switch (a)
            {
                case Constant.Type.Complex:
                    if (b == Constant.Type.Float || b == Constant.Type.Int || b == Constant.Type.Bool)
                    {
                        return a;
                    }
                    else if (b == Constant.Type.String)
                        return b;
                    break;
                case Constant.Type.Float:
                    if (b == Constant.Type.Complex || b == Constant.Type.String)
                    {
                        return b;
                    }
                    else if (b == Constant.Type.Int || b == Constant.Type.Bool)
                    {
                        return a;
                    }
                    break;
                case Constant.Type.Int:
                    if (b == Constant.Type.Complex || b == Constant.Type.Float || b == Constant.Type.String)
                    {
                        return b;
                    }
                    else if (b == Constant.Type.Bool)
                        return a;
                    break;
                case Constant.Type.String:
                    return a;
                case Constant.Type.Bool:
                    return b;
            }
            throw new Exception("Invalid type conversion.");
        }
        static public Constant Convert(Constant constant, Constant.Type newType)
        {
            if (newType == constant.ConstantType)
                return constant;
            switch (newType)
            {
                case Constant.Type.Complex:
                    if (constant.ConstantType == Constant.Type.Float)
                    {
                        return new ComplexValue((constant as FloatValue).Value, 0.0);
                    }
                    else if (constant.ConstantType == Constant.Type.Int)
                    {
                        return new ComplexValue((constant as IntValue).Value, 0.0);
                    }
                    break;
                case Constant.Type.Float:
                    if (constant.ConstantType == Constant.Type.Int)
                    {
                        return new FloatValue((constant as IntValue).Value);
                    }
                    if (constant.ConstantType == Constant.Type.Bool)
                    {
                        return new FloatValue((constant as BoolValue).Value ? 1.0 : 0.0);
                    }
                    break;
                case Constant.Type.String:
                    return constant.CastToString();
                case Constant.Type.Int:
                    if (constant.ConstantType == Constant.Type.Bool)
                    {
                        return new IntValue((constant as BoolValue).Value ? 1 : 0);
                    }
                    break;
                case Constant.Type.Bool:
                    if (constant.ConstantType == Constant.Type.Int)
                    {
                        return new BoolValue((constant as IntValue).Value == 0 ? true : false);
                    }
                    break;
            }
            throw new Exception("Invalid type conversion.");
        }
        private Value Assignment(AssignmentNode exp)
        {
            Value left = Eval(exp.Left);
            Value right = Eval(exp.Right);
            try
            {
                Constant rightConstant = right.GetRValue();
                if (left is LValue l)
                {
                    l.SetValue((Object)right);
                    return right;
                }
            }
            catch (Exception exc)
            {
                throw new ModelInterpreterException(exc.Message)
                {
                    Line = exp.Left.Position,
                    Position = exp.Right.Position
                };
            }
            throw new ModelInterpreterException("Left part of assignment should be LValue (Identifier).")
            {
                Line = exp.Left.Line,
                Position = exp.Left.Position
            };
        }
        private Value Negation(NegationNode exp)
        {
            Value val = Eval(exp.InnerNode);
            Constant obj = val.GetRValue();
            switch (obj.ConstantType)
            {
                case Constant.Type.Complex:
                    return ComplexValue.opNeg((ComplexValue)obj);
                case Constant.Type.Int:
                    return IntValue.opNeg((IntValue)obj);
                case Constant.Type.Float:
                    return FloatValue.opNeg((FloatValue)obj);
                default:
                    throw new ModelInterpreterException("Negation is undefined for type \"" + obj.ConstantType.ToString() + "\"")
                    {
                        Line = exp.Line,
                        Position = exp.Position
                    };
            }
            throw new ModelInterpreterException("Operator isn't available for custom types.")
            {
                Line = exp.Line,
                Position = exp.Position
            };
        }
        private Value Addition(AdditionNode exp)
        {
            try
            {
                Constant left = Eval(exp.Left).GetRValue();
                Constant right = Eval(exp.Right).GetRValue();
                Constant.Type leftType = left.ConstantType;
                Constant.Type rightType = right.ConstantType;
                Constant.Type bt = Max(leftType, rightType);
                switch (bt)
                {
                    case Constant.Type.Complex:
                        {
                            ComplexValue l = (ComplexValue)Convert(left, bt);
                            ComplexValue r = (ComplexValue)Convert(right, bt);
                            return ComplexValue.opAdd(l, r);
                        }
                    case Constant.Type.Int:
                        {
                            IntValue l = (IntValue)Convert(left, bt);
                            IntValue r = (IntValue)Convert(right, bt);
                            return IntValue.opAdd(l, r);
                        }
                    case Constant.Type.Float:
                        {
                            FloatValue l = (FloatValue)Convert(left, bt);
                            FloatValue r = (FloatValue)Convert(right, bt);
                            return FloatValue.opAdd(l, r);
                        }
                    case Constant.Type.String:
                        {
                            StringValue l = (StringValue)Convert(left, bt);
                            StringValue r = (StringValue)Convert(right, bt);
                            return StringValue.opAdd(l, r);
                        }
                }
                throw new ModelInterpreterException("Invalid type \"" + bt.ToString() + "\".")
                {
                    Line = exp.Line,
                    Position = exp.Position
                };
            }
            catch (ModelInterpreterException exc)
            {
                throw exc;
            }
            catch (Exception exc)
            {
                throw new ModelInterpreterException(exc.Message)
                {
                    Line = exp.Line,
                    Position = exp.Position
                };
            }
        }
        private Value Subtraction(SubtractionNode exp)
        {
            try
            {
                Constant left = Eval(exp.Left).GetRValue();
                Constant right = Eval(exp.Right).GetRValue();
                Constant.Type leftType = left.ConstantType;
                Constant.Type rightType = right.ConstantType;
                Constant.Type bt = Max(leftType, rightType);
                switch (bt)
                {
                    case Constant.Type.Complex:
                        {
                            ComplexValue l = (ComplexValue)Convert(left, bt);
                            ComplexValue r = (ComplexValue)Convert(right, bt);
                            return ComplexValue.opSub(l, r);
                        }
                    case Constant.Type.Int:
                        {
                            IntValue l = (IntValue)Convert(left, bt);
                            IntValue r = (IntValue)Convert(right, bt);
                            return IntValue.opSub(l, r);
                        }
                    case Constant.Type.Float:
                        {
                            FloatValue l = (FloatValue)Convert(left, bt);
                            FloatValue r = (FloatValue)Convert(right, bt);
                            return FloatValue.opSub(l, r);
                        }
                }
                throw new ModelInterpreterException("Invalid type \"" + bt.ToString() + "\".")
                {
                    Line = exp.Line,
                    Position = exp.Position
                };
            }
            catch (ModelInterpreterException exc)
            {
                throw exc;
            }
            catch (Exception exc)
            {
                throw new ModelInterpreterException(exc.Message)
                {
                    Line = exp.Line,
                    Position = exp.Position
                };
            }
        }
        private Value Multiplication(MultiplicationNode exp)
        {
            try
            {
                Constant left = Eval(exp.Left).GetRValue();
                Constant right = Eval(exp.Right).GetRValue();
                Constant.Type leftType = left.ConstantType;
                Constant.Type rightType = right.ConstantType;
                Constant.Type bt = Max(leftType, rightType);
                switch (bt)
                {
                    case Constant.Type.Complex:
                        {
                            ComplexValue l = (ComplexValue)Convert(left, bt);
                            ComplexValue r = (ComplexValue)Convert(right, bt);
                            return ComplexValue.opMult(l, r);
                        }
                    case Constant.Type.Int:
                        {
                            IntValue l = (IntValue)Convert(left, bt);
                            IntValue r = (IntValue)Convert(right, bt);
                            return IntValue.opMult(l, r);
                        }
                    case Constant.Type.Float:
                        {
                            FloatValue l = (FloatValue)Convert(left, bt);
                            FloatValue r = (FloatValue)Convert(right, bt);
                            return FloatValue.opMult(l, r);
                        }
                }
                throw new ModelInterpreterException("Invalid type \"" + bt.ToString() + "\".")
                {
                    Line = exp.Line,
                    Position = exp.Position
                };
            }
            catch (ModelInterpreterException exc)
            {
                throw exc;
            }
            catch (Exception exc)
            {
                throw new ModelInterpreterException(exc.Message)
                {
                    Line = exp.Line,
                    Position = exp.Position
                };
            }
        }
        private Value Division(DivisionNode exp)
        {
            try
            {
                Constant left = Eval(exp.Left).GetRValue();
                Constant right = Eval(exp.Right).GetRValue();
                Constant.Type leftType = left.ConstantType;
                Constant.Type rightType = right.ConstantType;
                Constant.Type bt = Max(leftType, rightType);
                switch (bt)
                {
                    case Constant.Type.Complex:
                        {
                            ComplexValue l = (ComplexValue)Convert(left, bt);
                            ComplexValue r = (ComplexValue)Convert(right, bt);
                            return ComplexValue.opDiv(l, r);
                        }
                    case Constant.Type.Int:
                        {
                            IntValue l = (IntValue)Convert(left, bt);
                            IntValue r = (IntValue)Convert(right, bt);
                            return IntValue.opDiv(l, r);
                        }
                    case Constant.Type.Float:
                        {
                            FloatValue l = (FloatValue)Convert(left, bt);
                            FloatValue r = (FloatValue)Convert(right, bt);
                            return FloatValue.opDiv(l, r);
                        }
                }
                throw new ModelInterpreterException("Invalid type \"" + bt.ToString() + "\".")
                {
                    Line = exp.Line,
                    Position = exp.Position
                };
            }
            catch (ModelInterpreterException exc)
            {
                throw exc;
            }
            catch (Exception exc)
            {
                throw new ModelInterpreterException(exc.Message)
                {
                    Line = exp.Line,
                    Position = exp.Position
                };
            }
        }
        private Value Function(FunctionNode exp)
        {
            List<FunctionDefinition> funcList;
            try
            {
                funcList = functionTable[exp.FunctionName];
            }
            catch (Exception)
            {
                throw new ModelInterpreterException("Invalid function name.")
                {
                    Line = exp.Line,
                    Position = exp.Position
                };
            }
            List<Constant> args = new List<Constant>();
            foreach (ExpressionNode expNode in exp.Arguments)
            {
                Constant obj = Eval(expNode).GetRValue();
                args.Add(obj);
            }
            Exception lastExc = new Exception("No definition for function \"" + exp.FunctionName + "\".");
            foreach(FunctionDefinition fd in funcList)
            {
                try
                {
                    return FunctionDefinition.Compute(fd, args);
                }
                catch (Exception exc)
                {
                    lastExc = exc;
                }
            }
            throw new ModelInterpreterException(lastExc.Message)
            {
                Line = exp.Line,
                Position = exp.Position
            };
        }
        private Value Or(OrNode exp)
        {
            try
            {
                Constant left = Eval(exp.Left).GetRValue();
                Constant right = Eval(exp.Right).GetRValue();
                BoolValue l = (BoolValue)Convert(left, Constant.Type.Bool);
                BoolValue r = (BoolValue)Convert(right, Constant.Type.Bool);
                return BoolValue.opOr(l, r);
            }
            catch (Exception exc)
            {
                throw new ModelInterpreterException(exc.Message)
                {
                    Line = exp.Line,
                    Position = exp.Position
                };
            }
        }
        private Value And(AndNode exp)
        {
            try
            {
                Constant left = Eval(exp.Left).GetRValue();
                Constant right = Eval(exp.Right).GetRValue();
                BoolValue l = (BoolValue)Convert(left, Constant.Type.Bool);
                BoolValue r = (BoolValue)Convert(right, Constant.Type.Bool);
                return BoolValue.opAnd(l, r);
            }
            catch (Exception exc)
            {
                throw new ModelInterpreterException(exc.Message)
                {
                    Line = exp.Line,
                    Position = exp.Position
                };
            }
        }
        private Value Not(NotNode exp)
        {
            try
            {
                Constant node = Eval(exp.InnerNode).GetRValue();
                BoolValue n = (BoolValue)Convert(node, Constant.Type.Bool);
                return BoolValue.opNot(n);
            }
            catch (Exception exc)
            {
                throw new ModelInterpreterException(exc.Message)
                {
                    Line = exp.Line,
                    Position = exp.Position
                };
            }
        }
        private Array BuildArray(ArrayNode node)
        {
            Array result = new Array();
            List<Constant> elements = new List<Constant>();
            foreach (var element in node.elements)
            {
                try
                {
                    Constant value = Eval(element).GetRValue();
                    elements.Add(value);
                }
                catch (ValueException exc)
                {
                    errors.Add(new ErrorMessage($"Не возможно преобразовать выражение в константное значение", element.Line, element.Position));
                }
            }
            result.Values = elements.ToArray();
            return result;
        }
        private Object BuildObject(ObjectNode node)
        {
            Object result = new Object();
            result.Name = node.Name;
            result.Values = new Dictionary<string, Constant>();
            foreach (var argument in node.Arguments)
            {
                try
                {
                    Constant value = Eval(argument.Value).GetRValue();
                    if (value is VoidValue)
                    {
                        throw new ValueException();
                    }
                    result.Values.Add(argument.Key, value);
                }
                catch (ArgumentException exc)
                {
                    errors.Add(new ErrorMessage($"Нельзя использовать два параметра с одинаковыми именами:{argument.Key} в {node.Name}", argument.Line, argument.Position));
                }
                catch (ValueException exc)
                {
                    errors.Add(new ErrorMessage($"Не возможно преобразовать выражение в константное значение", argument.Value.Line, argument.Value.Position));
                }
            }
            return result;
        }
        private Value Eval(ExpressionNode exp)
        {
            switch (exp.Type)
            {
                case ASTNode.NodeType.Assignment:
                    return Assignment((AssignmentNode)exp);
                case ASTNode.NodeType.Negation:
                    return Negation((NegationNode)exp);
                case ASTNode.NodeType.Addition:
                    return Addition((AdditionNode)exp);
                case ASTNode.NodeType.Subtraction:
                    return Subtraction((SubtractionNode)exp);
                case ASTNode.NodeType.Multiplication:
                    return Multiplication((MultiplicationNode)exp);
                case ASTNode.NodeType.Division:
                    return Division((DivisionNode)exp);
                case ASTNode.NodeType.Not:
                    return Not((NotNode)exp);
                case ASTNode.NodeType.And:
                    return And((AndNode)exp);
                case ASTNode.NodeType.Or:
                    return Or((OrNode)exp);
                case ASTNode.NodeType.Function:
                    return Function((FunctionNode)exp);
                /*case ASTNode.NodeType.Member:
                    return Member(exp);*/
                case ASTNode.NodeType.String:
                    return new StringValue()
                    {
                        Value = (exp as StringNode).Value
                    };
                case ASTNode.NodeType.Float:
                    return new FloatValue()
                    {
                        Value = (exp as FloatNode).Value
                    };
                case ASTNode.NodeType.Integer:
                    return new IntValue()
                    {
                        Value = ((IntNode)exp).Value
                    };
                case ASTNode.NodeType.Complex:
                    {
                        ComplexNode node = (ComplexNode)exp;
                        return new ComplexValue()
                        {
                            Re = node.Re,
                            Im = node.Im
                        };
                    }
                case ASTNode.NodeType.ComplexPhase:
                    {
                        ComplexPhaseNode node = (ComplexPhaseNode)exp;
                        return new ComplexValue()
                        {
                            Re = node.Magnitude * Math.Cos(node.Phase),
                            Im = node.Magnitude * Math.Sin(node.Phase),
                        };
                    }

                case ASTNode.NodeType.Boolean:
                    {
                        BooleanNode node = exp as BooleanNode;
                        return new BoolValue(node.Value);
                    }
                case ASTNode.NodeType.Identifier:
                    {
                        IdentifierNode node = (IdentifierNode)exp;
                        LValueIdentifier val = new LValueIdentifier(node.Value);
                        return val;
                    }
                case ASTNode.NodeType.Array:
                    {
                        return BuildArray(exp as ArrayNode);
                    }
                case ASTNode.NodeType.Object:
                    {
                        return BuildObject(exp as ObjectNode);
                    }
            }
            return new VoidValue();
        }
    }
#endif
}
