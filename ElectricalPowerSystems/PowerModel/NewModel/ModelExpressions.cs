using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricalPowerSystems.PowerModel.NewModel.Grammar;

namespace ElectricalPowerSystems.PowerModel.NewModel
{
    public partial class ModelInterpreter
    {
        public interface IType
        {
            Constant Validate(Constant constant);
        }
        public class FloatType : IType
        {
            Constant IType.Validate(Constant constant)
            {
                return Convert(constant,Constant.Type.Float);
            }
        }
        public class ComplexType : IType
        {
            Constant IType.Validate(Constant constant)
            {
                return Convert(constant, Constant.Type.Complex);
            }
        }
        public class IntType : IType
        {
            Constant IType.Validate(Constant constant)
            {
                return Convert(constant, Constant.Type.Int);
            }
        }
        public class CompelxType : IType
        {
            Constant IType.Validate(Constant constant)
            {
                return Convert(constant, Constant.Type.Complex);
            }
        }
        public class BoolType : IType
        {
            Constant IType.Validate(Constant constant)
            {
                return Convert(constant, Constant.Type.Bool);
            }
        }
        public class StringType : IType
        {
            Constant IType.Validate(Constant constant)
            {
                return Convert(constant, Constant.Type.String);
            }
        }
        public class ObjectType: IType
        {
            Dictionary<string, IType> memberTypes;
            public ObjectType(Dictionary<string, IType> memberTypes)
            {
                this.memberTypes = memberTypes;
            }
            Constant IType.Validate(Constant constant)
            {
                //return Convert(constant, Constant.Type.Object) as Object;
                Object obj = Convert(constant,Constant.Type.Object) as Object;
                Object result = new Object();
                result.Name = obj.Name;
                foreach (var value in obj.Values)
                {
                    if (memberTypes.ContainsKey(value.Key))
                    {
                        IType type = memberTypes[value.Key];
                        result.Values.Add(value.Key,type.Validate(value.Value));
                    } else
                    {
                        throw new Exception($"Неизвестное поле \"{value.Key}\" в объекте \"{obj.Name}\"");
                    }
                }
                foreach (var member in memberTypes)
                {
                    if (!result.ContainsKey(member.Key))
                    {
                        throw new Exception($"Отсутствует поле \"{member}\" в объекте \"{obj.Name}\"");
                    }
                }
                return result;
            }
        }
        public class ArrayType : IType
        {
            IType arrayType;
            public ArrayType(IType arrayType)
            {
                this.arrayType = arrayType;
            }
            Constant IType.Validate(Constant constant)
            {
                Array array = Convert(constant, Constant.Type.Array) as Array;
                for (int i = 0; i < array.Values.Length; i++)
                {
                    array.Values[i] = arrayType.Validate(array.Values[i]);
                }
                return array;
            }
        }
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
                    throw new ModelInterpreterException("Невозможно присвоить Void");
                ModelInterpreter.GetInstanse().variableTable[key] = value;
            }
            public override Constant GetRValue()
            {
                try
                {
                    Constant obj = ModelInterpreter.GetInstanse().variableTable[key];
                    return obj;
                }
                catch (KeyNotFoundException)
                {
                    throw new Exception($"Неопределённая переменная \"{key}\"");
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
            public static FloatValue OpAdd(FloatValue a, FloatValue b)
            {
                return new FloatValue(a.Value + b.Value);
            }
            public static FloatValue OpSub(FloatValue a, FloatValue b)
            {
                return new FloatValue(a.Value - b.Value);
            }
            public static FloatValue OpMult(FloatValue a, FloatValue b)
            {
                return new FloatValue(a.Value * b.Value);
            }
            public static FloatValue OpDiv(FloatValue a, FloatValue b)
            {
                return new FloatValue(a.Value / b.Value);
            }
            public static FloatValue OpPow(FloatValue a, FloatValue b)
            {
                return new FloatValue(Math.Pow(a.Value, b.Value));
            }
            public static FloatValue OpNeg(FloatValue a)
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
            public static IntValue OpAdd(IntValue a, IntValue b)
            {
                return new IntValue(a.Value + b.Value);
            }
            public static IntValue OpSub(IntValue a, IntValue b)
            {
                return new IntValue(a.Value - b.Value);
            }
            public static IntValue OpMult(IntValue a, IntValue b)
            {
                return new IntValue(a.Value * b.Value);
            }
            public static IntValue OpDiv(IntValue a, IntValue b)
            {
                return new IntValue(a.Value / b.Value);
            }
            public static IntValue OpNeg(IntValue a)
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
            public static BoolValue OpNot(BoolValue a)
            {
                return new BoolValue(!a.Value);
            }
            public static BoolValue OpOr(BoolValue a, BoolValue b)
            {
                return new BoolValue(a.Value || b.Value);
            }
            public static BoolValue OpAnd(BoolValue a, BoolValue b)
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
            public Complex32 Value { get { return new Complex32((float)re, (float)im); } }
            public ComplexValue():base(Type.Complex)
            {
            }
            public ComplexValue(double re, double im) : base(Type.Complex)
            {
                this.re = re;
                this.im = im;
            }
            public static ComplexValue FromExp(double magn, double arg)
            {
                return new ComplexValue(magn*Math.Cos(arg),magn*Math.Sin(arg));
            }
            public static ComplexValue OpAdd(ComplexValue a, ComplexValue b)
            {
                return new ComplexValue(a.Re + b.Re, a.Im + b.Im);
            }
            public static ComplexValue OpSub(ComplexValue a, ComplexValue b)
            {
                return new ComplexValue(a.Re + b.Re, a.Im + b.Im);
            }
            public static ComplexValue OpMult(ComplexValue a, ComplexValue b)
            {
                return new ComplexValue(a.Re * b.Re - a.Im * b.Im, a.Re * b.Im + a.Im * b.Re);
            }
            public static ComplexValue OpDiv(ComplexValue a, ComplexValue b)
            {
                double l = b.Re * b.Re + b.Im + b.Im;
                return new ComplexValue((a.Re * b.Re - a.Im * b.Im) / l, (-a.Re * b.Im + a.Im * b.Re) / l);
            }
            public static ComplexValue OpNeg(ComplexValue a)
            {
                return new ComplexValue(-a.Re, -a.Im);
            }
            public override StringValue CastToString()
            {
                return new StringValue(Magn.ToString() + "@" + MathUtils.MathUtils.Degrees(Phase).ToString());
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
            public static StringValue OpAdd(StringValue a, StringValue b)
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
            public Constant GetValue(string key)
            {
                if(ContainsKey(key))
                    return Values[key];
                throw new MissingValueException(key);
            }
            public void SetValue(string key, Constant value)
            {
                Values[key] = value;
            }
            public bool ContainsKey(string key)
            {
                return Values.ContainsKey(key);
            }
            public override StringValue CastToString()
            {
                string result = $"{Name}{{";
                int i = 1;
                foreach (var value in Values)
                {
                    result += $"{value.Key} = {value.Value.CastToString()}";
                    if (i != Values.Count)
                    {
                        result += ", ";
                    }
                }
                result += "}";
                return new StringValue(result);
            }
        }
        public class Array : Constant
        {
            public Type arrayType;
            public Constant[] Values;
            public Array() : base(Type.Array)
            {
            }
            public override StringValue CastToString()
            {
                string result = $"Array<{arrayType.ToString()}>[";
                int i = 1;
                foreach (var value in Values)
                {
                    result += $"{value.CastToString()}";
                    if (i != Values.Length)
                    {
                        result += ", ";
                    }
                }
                result += "]";
                return new StringValue(result);
            }
        }
        static public Constant.Type Max(Constant.Type a, Constant.Type b)
        {
            if (a == b)
                return a;
            if (a == Constant.Type.Object || a == Constant.Type.Array)
            {
                if (b == Constant.Type.String)
                    return b;
                return a;
                //throw new Exception("Invalid type conversion.");
            }
            else if (b == Constant.Type.Object || b == Constant.Type.Array)
            {
                if (a == Constant.Type.String)
                    return a;
                return b;
                //throw new Exception("Invalid type conversion.");
            }
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
            return a;
            //throw new Exception("Invalid type conversion.");
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
            throw new TypeConversionError(constant.ConstantType.ToString(), newType.ToString());
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
                    l.SetValue(rightConstant);
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
            throw new ModelInterpreterException("Левая часть присваивания должна быть идентификатором")
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
                    return ComplexValue.OpNeg((ComplexValue)obj);
                case Constant.Type.Int:
                    return IntValue.OpNeg((IntValue)obj);
                case Constant.Type.Float:
                    return FloatValue.OpNeg((FloatValue)obj);
            }
            throw new ModelInterpreterException($"Отрицание не определено для типа \"{obj.ConstantType.ToString()}\"")
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
                            return ComplexValue.OpAdd(l, r);
                        }
                    case Constant.Type.Int:
                        {
                            IntValue l = (IntValue)Convert(left, bt);
                            IntValue r = (IntValue)Convert(right, bt);
                            return IntValue.OpAdd(l, r);
                        }
                    case Constant.Type.Float:
                        {
                            FloatValue l = (FloatValue)Convert(left, bt);
                            FloatValue r = (FloatValue)Convert(right, bt);
                            return FloatValue.OpAdd(l, r);
                        }
                    case Constant.Type.String:
                        {
                            StringValue l = (StringValue)Convert(left, bt);
                            StringValue r = (StringValue)Convert(right, bt);
                            return StringValue.OpAdd(l, r);
                        }
                }
                throw new ModelInterpreterException("Сложение не определено для типа \"" + bt.ToString() + "\".")
                {
                    Line = exp.Line,
                    Position = exp.Position
                };
            }
            catch (TypeConversionError exc)
            {
                throw new ModelInterpreterException($"Не удалось преобразование из  \"{exc.Src}\" в \"{exc.Dst}\"")
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
                            return ComplexValue.OpSub(l, r);
                        }
                    case Constant.Type.Int:
                        {
                            IntValue l = (IntValue)Convert(left, bt);
                            IntValue r = (IntValue)Convert(right, bt);
                            return IntValue.OpSub(l, r);
                        }
                    case Constant.Type.Float:
                        {
                            FloatValue l = (FloatValue)Convert(left, bt);
                            FloatValue r = (FloatValue)Convert(right, bt);
                            return FloatValue.OpSub(l, r);
                        }
                }
                throw new ModelInterpreterException("Вычитание не определено для типа \"" + bt.ToString() + "\".")
                {
                    Line = exp.Line,
                    Position = exp.Position
                };
            }
            catch (TypeConversionError exc)
            {
                throw new ModelInterpreterException($"Не удалось преобразование из \"{exc.Src}\" в \"{exc.Dst}\"")
                {
                    Line = exp.Line,
                    Position = exp.Position
                };
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
                            return ComplexValue.OpMult(l, r);
                        }
                    case Constant.Type.Int:
                        {
                            IntValue l = (IntValue)Convert(left, bt);
                            IntValue r = (IntValue)Convert(right, bt);
                            return IntValue.OpMult(l, r);
                        }
                    case Constant.Type.Float:
                        {
                            FloatValue l = (FloatValue)Convert(left, bt);
                            FloatValue r = (FloatValue)Convert(right, bt);
                            return FloatValue.OpMult(l, r);
                        }
                }
                throw new ModelInterpreterException("Некорректная операция для типа \"" + bt.ToString() + "\".")
                {
                    Line = exp.Line,
                    Position = exp.Position
                };
            }
            catch (TypeConversionError exc)
            {
                throw new ModelInterpreterException($"Не удалось преобразование из  \"{exc.Src}\" в \"{exc.Dst}\"")
                {
                    Line = exp.Line,
                    Position = exp.Position
                };
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
                            return ComplexValue.OpDiv(l, r);
                        }
                    case Constant.Type.Int:
                        {
                            IntValue l = (IntValue)Convert(left, bt);
                            IntValue r = (IntValue)Convert(right, bt);
                            return IntValue.OpDiv(l, r);
                        }
                    case Constant.Type.Float:
                        {
                            FloatValue l = (FloatValue)Convert(left, bt);
                            FloatValue r = (FloatValue)Convert(right, bt);
                            return FloatValue.OpDiv(l, r);
                        }
                }
                throw new ModelInterpreterException("Деление не определено для типа \"" + bt.ToString() + "\".")
                {
                    Line = exp.Line,
                    Position = exp.Position
                };
            }
            catch (TypeConversionError exc)
            {
                throw new ModelInterpreterException($"Не удалось преобразование из \"{exc.Src}\" в \"{exc.Dst}\"")
                {
                    Line = exp.Line,
                    Position = exp.Position
                };
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
                throw new ModelInterpreterException($"Неизвестная функция \"{exp.FunctionName}\"")
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
            Exception lastExc=null;
            foreach (FunctionDefinition fd in funcList)
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
            if (lastExc != null)
            {
                throw new ModelInterpreterException(lastExc.Message)
                {
                    Line = exp.Line,
                    Position = exp.Position
                };
            }
            string exceptionMessage = $"Не существует подходящего определения для функции {exp.FunctionName} ( ";
            int i = 0;
            foreach (var arg in args)
            {
                if (i != 0)
                {
                    exceptionMessage += ", ";
                }
                exceptionMessage += arg.ConstantType.ToString();
            }
            exceptionMessage += ")";
            throw new ModelInterpreterException(exceptionMessage)
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
                return BoolValue.OpOr(l, r);
            }
            catch (TypeConversionError exc)
            {
                throw new ModelInterpreterException($"Операция \"ИЛИ\" не определена для типов \"{exc.Src}\" и \"{exc.Dst}\"")
                {
                    Line = exp.Line,
                    Position = exp.Position
                };
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
                return BoolValue.OpAnd(l, r);
            }
            catch (TypeConversionError exc)
            {
                throw new ModelInterpreterException($"Операция \"И\" не определена для типов \"{exc.Src}\" и \"{exc.Dst}\"")
                {
                    Line = exp.Line,
                    Position = exp.Position
                };
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
                return BoolValue.OpNot(n);
            }
            catch (TypeConversionError exc)
            {
                throw new ModelInterpreterException($"Операция \"НЕ\" не определена для типов \"{exc.Src}\" и \"{exc.Dst}\"")
                {
                    Line = exp.Line,
                    Position = exp.Position
                };
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
            Constant.Type type;
            switch (node.Type)
            {
                case Node.NodeType.Boolean:
                    type = Constant.Type.Bool;
                    break;
                case Node.NodeType.Integer:
                    type = Constant.Type.Int;
                    break;
                case Node.NodeType.Object:
                    type = Constant.Type.Object;
                    break;
                case Node.NodeType.Float:
                    type = Constant.Type.Float;
                    break;
                case Node.NodeType.String:
                    type = Constant.Type.String;
                    break;
                default:
                    throw new Exception($"Тип {node.Type.ToString()} не может использоваться в массиве");
            }
            List<Constant> elements = new List<Constant>();
            int index = 0;
            foreach (var element in node.Elements)
            {
                try
                {
                    Constant value = Convert(Eval(element).GetRValue(), type);
                    elements.Add(value);
                }
                catch (TypeConversionError exc)
                {
                    errors.Add(new ErrorMessage($"Невозможно преобразовать {index}-тое значение в массиве из типа {exc.Src} в тип {exc.Dst}", element.Line, element.Position));
                }
                catch (ValueException)
                {
                    errors.Add(new ErrorMessage($"Невозможно преобразовать выражение в константное значение", element.Line, element.Position));
                }
                index++;
            }
            result.arrayType = type;
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
                catch (ArgumentException)
                {
                    errors.Add(new ErrorMessage($"Нельзя использовать два параметра с одинаковыми именами:{argument.Key} в {node.Name}", argument.Line, argument.Position));
                }
                catch (ValueException)
                {
                    errors.Add(new ErrorMessage($"Невозможно преобразовать выражение в константное значение", argument.Value.Line, argument.Value.Position));
                }
            }
            return result;
        }
        private Value Eval(ExpressionNode exp)
        {
            switch (exp.Type)
            {
                case Node.NodeType.Assignment:
                    return Assignment((AssignmentNode)exp);
                case Node.NodeType.Negation:
                    return Negation((NegationNode)exp);
                case Node.NodeType.Addition:
                    return Addition((AdditionNode)exp);
                case Node.NodeType.Subtraction:
                    return Subtraction((SubtractionNode)exp);
                case Node.NodeType.Multiplication:
                    return Multiplication((MultiplicationNode)exp);
                case Node.NodeType.Division:
                    return Division((DivisionNode)exp);
                case Node.NodeType.Not:
                    return Not((NotNode)exp);
                case Node.NodeType.And:
                    return And((AndNode)exp);
                case Node.NodeType.Or:
                    return Or((OrNode)exp);
                case Node.NodeType.Function:
                    return Function((FunctionNode)exp);
                case Node.NodeType.String:
                    return new StringValue()
                    {
                        Value = (exp as StringNode).Value
                    };
                case Node.NodeType.Float:
                    return new FloatValue()
                    {
                        Value = (exp as FloatNode).Value
                    };
                case Node.NodeType.Integer:
                    return new IntValue()
                    {
                        Value = ((IntNode)exp).Value
                    };
                case Node.NodeType.Complex:
                    {
                        ComplexNode node = (ComplexNode)exp;
                        return new ComplexValue()
                        {
                            Re = node.Re,
                            Im = node.Im
                        };
                    }
                case Node.NodeType.ComplexPhase:
                    {
                        ComplexPhaseNode node = (ComplexPhaseNode)exp;
                        return new ComplexValue()
                        {
                            Re = node.Magnitude * Math.Cos(node.Phase),
                            Im = node.Magnitude * Math.Sin(node.Phase),
                        };
                    }

                case Node.NodeType.Boolean:
                    {
                        BooleanNode node = exp as BooleanNode;
                        return new BoolValue(node.Value);
                    }
                case Node.NodeType.Identifier:
                    {
                        IdentifierNode node = (IdentifierNode)exp;
                        LValueIdentifier val = new LValueIdentifier(node.Value);
                        return val;
                    }
                case Node.NodeType.Array:
                    {
                        return BuildArray(exp as ArrayNode);
                    }
                case Node.NodeType.Object:
                    {
                        return BuildObject(exp as ObjectNode);
                    }
            }
            return new VoidValue();
        }
    }
}
