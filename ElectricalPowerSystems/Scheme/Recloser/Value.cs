using System;
using System.Globalization;

namespace ElectricalPowerSystems.Scheme.Recloser
{
    public interface IValue
    {
        byte[] ToBytes();
        int ByteSize();
        void FromBytes(byte[] bytes);
        string ToString();
    }
    public class Float : IValue
    {
        double value;
        public double Value { get { return value; } set { this.value = value; } }
        public Float()
        {
            value = 0.0;
        }
        public Float(double value)
        {
            this.value = value;
        }
        public Float(byte[] bytes)
        {
            FromBytes(bytes);
        }
        public static Float FromByteArray(ByteArray bytearray, int offset)
        {
            Float result = new Float();
            result.FromBytes(bytearray.At(offset, result.ByteSize()));
            return result;
        }
        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(value);
        }
        public static int Sizeof()
        {
            return sizeof(double);
        }
        public int ByteSize()
        {
            return Sizeof();
        }
        public void FromBytes(byte[] bytes)
        {
            value = BitConverter.ToDouble(bytes, 0);
        }
        string IValue.ToString()
        {
            return value.ToString(new CultureInfo("en-US"));
        }
    }
    public class Short : IValue
    {
        short value;
        public short Value { get { return value; } set { this.value = value; } }
        public Short()
        { }
        public Short(byte[] bytes)
        {
            FromBytes(bytes);
        }
        public Short(short value)
        {
            this.value = value;
        }
        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(value);
        }
        public static int Sizeof()
        {
            return sizeof(short);
        }
        public int ByteSize()
        {
            return Sizeof();
        }
        public void FromBytes(byte[] bytes)
        {
            value = BitConverter.ToInt16(bytes, 0);
        }
        string IValue.ToString()
        {
            return value.ToString();
        }
    }
    public class Int : IValue
    {
        int value;
        public bool BoolValue { get { return value == 0 ? false : true; } set { this.value = (value ? 1 : 0); } }
        public int Value { get { return value; } set { this.value = value; } }
        public Int()
        {
            value = 0;
        }
        public Int(bool value)
        {
            this.BoolValue = value;
        }
        public Int(byte[] bytes)
        {
            FromBytes(bytes);
        }
        public Int(int value)
        {
            this.value = value;
        }
        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(value);
        }
        public static int Sizeof()
        {
            return sizeof(int);
        }
        public int ByteSize()
        {
            return Sizeof();
        }
        public void FromBytes(byte[] bytes)
        {
            value = BitConverter.ToInt32(bytes, 0);
        }
        string IValue.ToString()
        {
            return value.ToString();
        }
    }
    class Bool : IValue
    {
        int value;
        public int Value { get { return value; } set { this.value = value; } }
        public Bool()
        { }
        public Bool(bool value)
        {
            this.value = value ? 1 : 0;
        }
        public Bool(byte[] bytes)
        {
            FromBytes(bytes);
        }
        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(value);
        }
        static public int Sizeof()
        {
            return sizeof(int);
        }
        public int ByteSize()
        {
            return Sizeof();
        }
        public void FromBytes(byte[] bytes)
        {
            value = BitConverter.ToInt32(bytes, 0);
        }
        string IValue.ToString()
        {
            return value != 0 ? "true" : "false";
        }
    }
}
