using MathNet.Numerics;
using System;
using System.Collections.Generic;

namespace ElectricalPowerSystems.Scheme.Interpreter
{
    public partial class Interpreter
    {
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
                Interpreter.GetInstanse().variableTable[key] = value;
            }
            public override Constant GetRValue()
            {
                try
                {
                    Constant obj = Interpreter.GetInstanse().variableTable[key];
                    return obj;
                }
                catch (KeyNotFoundException)
                {
                    throw new Exception($"Неопределённая переменная \"{key}\"");
                }
            }
        }
        public abstract class Constant : Value
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
            public FloatValue(double value) : base(Type.Float)
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
            public BoolValue(bool value) : base(Type.Bool)
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
            public ComplexValue() : base(Type.Complex)
            {
            }
            public ComplexValue(double re, double im) : base(Type.Complex)
            {
                this.re = re;
                this.im = im;
            }
            public static ComplexValue FromExp(double magn, double arg)
            {
                return new ComplexValue(magn * Math.Cos(arg), magn * Math.Sin(arg));
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
                if (ContainsKey(key))
                    return Values[key];
                throw new Interpreter.MissingValueException(key);
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
    }
}
