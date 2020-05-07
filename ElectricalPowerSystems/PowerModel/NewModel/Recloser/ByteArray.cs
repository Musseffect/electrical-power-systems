using System;

namespace ElectricalPowerSystems.PowerModel.NewModel.Recloser
{
    public class ByteArray
    {
        byte[] storage;
        int size;
        public int Size{get{return size;} }
        public int Capacity { get { return capacity; } }
        int capacity;
        public ByteArray()
        {
            size = 0;
            capacity = 16;
            storage = new byte[capacity];
        }
        public T GetValue<T>(int offset)where T: IValue, new()
        {
            T result = new T();
            result.FromBytes(this.At(offset, result.ByteSize()));
            return result;
        }
        public void Set(int offset, IValue value)
        {
            if (offset < 0)
                throw new Exception($"Invalid index in ByteArray.Set({offset},IValue)");
            byte[] bytes = value.ToBytes();
            if (bytes.Length + offset > capacity)
            {
                GrowArray(bytes.Length + offset);
            }
            Array.Copy(bytes,0,storage,offset,bytes.Length);
            size = Math.Max(bytes.Length + offset,size);
        }
        public byte[] At(int i, int count)
        {
#if DEBUG||TEST
            if (i + count > size || i < 0)
                throw new Exception($"Invalid index in ByteArray.At({i})");
#endif
            byte[] result = new byte[count];
            Array.Copy(storage,i,result,0,count);
            return result;
        }
        public byte At(int i)
        {
#if DEBUG||TEST
            if (i > size || i < 0)
                throw new Exception($"Invalid index in ByteArray.At({i})");
#endif
            return storage[i];
        }
        void GrowArray(int newCapacity)
        {
            int oldCapacity = capacity;
            while (capacity < newCapacity)
                capacity *= 2;
            Array.Resize<byte>(ref storage,capacity);
        }
        void WriteToArray(byte _byte)
        {
            if (1 + size > capacity)
            {
                GrowArray(1 + size);
            }
            storage[size] = _byte;
            size ++;
        }
        void WriteToArray(byte[] bytes)
        {
            if (bytes.Length + size > capacity)
            {
                GrowArray(bytes.Length + size);
            }
            Array.Copy(bytes,0,storage,size,bytes.Length);
            size += bytes.Length;
        }
        public int AddConstant(IValue value)
        {
            byte[] bytes = value.ToBytes();
            WriteToArray(bytes);
            return size;
        }
        public int AddInstruction(Instruction instruction)
        {
            WriteToArray((byte)instruction);
            return size;
        }
    }
}
