using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerModel.NewModel.Recloser
{
    public abstract class Value
    {
        public abstract byte[] ToBytes();
        public abstract int ByteSize();
        public abstract void FromBytes(byte[] bytes);
        public new abstract string ToString();
    }
    class Float : Value
    {
        double value;
        public double Value { get { return value; } set { this.value = value; } }
        public Float()
        { }
        public Float(double value)
        {
            this.value = value;
        }
        public Float(byte[] bytes)
        {
            FromBytes(bytes);
        }
        public static Float FromByteArray(ByteArray bytearray,int offset)
        {
            Float result = new Float();
            result.FromBytes(bytearray.At(offset,result.ByteSize()));
            return result;
        }
        public override byte[] ToBytes()
        {
            return BitConverter.GetBytes(value);
        }
        public static int Sizeof()
        {
            return sizeof(double);
        }
        public override int ByteSize()
        {
            return Sizeof();
        }
        public override void FromBytes(byte[] bytes)
        {
            value = BitConverter.ToDouble(bytes,0);
        }
        public override string ToString()
        {
            return value.ToString(new CultureInfo("en-US"));
        }
    }
    class Short : Value
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
        public override byte[] ToBytes()
        {
            return BitConverter.GetBytes(value);
        }
        public static int Sizeof()
        {
            return sizeof(short);
        }
        public override int ByteSize()
        {
            return Sizeof();
        }
        public override void FromBytes(byte[] bytes)
        {
            value = BitConverter.ToInt16(bytes, 0);
        }
        public override string ToString()
        {
            return value.ToString();
        }
    }
    class Int : Value
    {
        int value;
        public int Value { get{ return value; } set { this.value = value; } }
        public Int()
        { }
        public Int(byte[] bytes)
        {
            FromBytes(bytes);
        }
        public Int(int value)
        {
            this.value = value;
        }
        public override byte[] ToBytes()
        {
            return BitConverter.GetBytes(value);
        }
        public static int Sizeof()
        {
            return sizeof(int);
        }
        public override int ByteSize()
        {
            return Sizeof();
        }
        public override void FromBytes(byte[] bytes)
        {
            value = BitConverter.ToInt32(bytes, 0);
        }
        public override string ToString()
        {
            return value.ToString();
        }
    }
    class Bool : Value
    {
        int value;
        public int Value { get { return value; } set { this.value = value; } }
        public Bool()
        { }
        public Bool(bool value)
        {
            this.value = value?1:0;
        }
        public Bool(byte[] bytes)
        {
            FromBytes(bytes);
        }
        public override byte[] ToBytes()
        {
            return BitConverter.GetBytes(value);
        }
        static public int Sizeof()
        {
            return sizeof(int);
        }
        public override int ByteSize()
        {
            return Sizeof();
        }
        public override void FromBytes(byte[] bytes)
        {
            value = BitConverter.ToInt32(bytes, 0);
        }
        public override string ToString()
        {
            return value!=0?"true":"false";
        }
    }
    public class ByteArray
    {
        byte[] storage;
        int size;
        public int Size{get{return size;} }
        int capacity;
        public ByteArray()
        {
            size = 0;
            capacity = 16;
            storage = new byte[capacity];
        }
        public T GetValue<T>(int offset)where T: Value, new()
        {
            T result = new T();
            result.FromBytes(this.At(offset, result.ByteSize()));
            return result;
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
        public int AddConstant(Value value)
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
    public enum Instruction
    {
        ADD_SP,//add_sp offset(4B),
        SUB_SP,//sub_sp offset(4B)
        LOAD,//load offset(4B) size(4B)
        WRITE,//write offset(4B) size(4B)
        LOAD_CONSTANT,//load_constant offset(4B) size(4B),
        LOAD_GLOBAL,//load_global address
        WRITE_GLOBAL,//write_global address
        ADD_CONSTANT_I,
        ADD_I,//add_i,
        SUB_I,
        MULT_I,
        DIV_I,
        MOD_I,
        NEGATE_I,
        ADD_D,
        SUB_D,
        MULT_D,
        DIV_D,
        NEGATE_D,
        GREATER_I,//greater_i,
        GREATEREQUAL_I,
        LESSER_I,
        LESSEREQUAL_I,
        EQUAL_I,
        NEQUAL_I,
        GREATER_D,//greater_i,
        GREATEREQUAL_D,
        LESSER_D,
        LESSEREQUAL_D,
        EQUAL_D,
        NEQUAL_D,
        NOT_B,
        SET_RESULT,//write_result size(4B)
        RET,
        CALL,//call index
        CALL_NATIVE, //call_native index
        JUMP_IF,//jump_if offset
        JUMP_NIF,//jump_nif offset
        DTOI,//dtoi
        ITOD//itod

        /*OP_NEGATE = 1,
        OP_ADD_D=2,
        OP_SUB_D=3,
        OP_MULT_D =4,//two addresses
        OP_DIV_D =5,//two addresses, addresses are local
        OP_ADD_I =6,//two addresses
        OP_SUB_I =7,//two addresses
        OP_MULT_I =8,//two addresses
        OP_DIV_I =9,//two addresses
        OP_CONSTANT_I =10,//get int from constants 
        OP_CONSTANT_D = 11,//get double from constants 
        OP_LOAD_I=12,//get int from address
        OP_WRITE_I=13,//put int to address
        OP_LOAD_D=14,//get double from address
        OP_WRITE_D=15,//put double to address
        OP_POP_D=16,//pop double on stack
        OP_PUSH_D=17,//push double on stack
        OP_POP_I=18,//pop int from stack
        OP_PUSH_I=19,//put int on stack
        OP_JUMP=20,//jump to int adress
        OP_IF_NOT_JUMP=21,//if int is not 0 jump to address(int), otherwise stay
        OP_IF_JUMP=22,//if int is 0 jump to address(int)
        OP_EQUAL=23,//two adresses
        OP_GREATER =24,//two adresses
        OP_LESS =25,//two adresses
        OP_D_TO_I =26,// convert double from adress(int) to integer
        OP_I_TO_D = 27,// convert int from adress(int) to double
        OP_NEGATE_I = 28,
        OP_NEGATE_D = 29,
        OP_NOT = 30,
        OP_NEQUAL = 31,
        OP_MOVE_SP = 32 // OP_MOVE_SP Bytes(int) : sp += Bytes*/
    }
    public class Stack
    {
        byte[] storage;
        int pointer;
        public int Pointer { get { return pointer; } set { pointer = value; } }
        public Stack(int byteSize)
        {
            pointer = 0;
            storage = new byte[byteSize];
        }
        public void Empty()
        {
            pointer = 0;
        }
        public byte[] ReadBytes(int offset, int size)
        {
            if (pointer - offset < 0 || size - offset > 0)
                throw new Exception("Invalid read stack command");
            byte[] bytes = new byte[size];
            Array.Copy(storage, pointer - offset, bytes, 0, size);
            return bytes;
        }
        public void WriteBytes(byte[] bytes,int offset, int size)
        {
            if (pointer - offset < 0 || size - offset > 0)
                throw new Exception("Invalid write stack command");
            Array.Copy(bytes, 0, storage, pointer - offset, size);
        }
        public T ReadValue<T>(int offset) where T:Value,new()
        {
            T result = new T();
            int size = result.ByteSize();
            if (offset< 0)
                throw new Exception("Stack is empty");
            byte[] bytes = new byte[size];
            Array.Copy(storage, pointer - offset, bytes, 0, size);
            result.FromBytes(bytes);
            return result;
        }
        public byte[] Pop(int size)
        {
            if (pointer - size < 0)
                throw new Exception("Stack is empty");
            byte[] result = new byte[size];
            pointer -= size;
            Array.Copy(storage, pointer, result, 0, size);
            return result;
        }
        public void PushToStack(byte[] bytes)
        {
            if (pointer + bytes.Length > storage.Length)
                throw new Exception("Stack overflow");
            Array.Copy(bytes,0,storage,pointer,bytes.Length);
            pointer += bytes.Length;
        }
    }
    public class Disassembler
    {
        string result;
        public string Disassemble(Program program)
        {
            result = "";
            for (int offset = 0; offset < program.Bytecode.Size;)
            {
                offset = DisassembleInstruction(program.Bytecode, offset);
            }
            return result;
        }
        int DisassembleInstruction(ByteArray bytecode,int offset)
        {
            result += $"    {offset} ";
            byte instruction = bytecode.At(offset);
            switch (instruction)
            {
                case (byte)Instruction.ADD_SP:
                    return PrintConstantInstruction("ADD_SP", bytecode.GetValue<Int>(offset+1), offset);
                case (byte)Instruction.LOAD:
                    {
                        Int a1 = bytecode.GetValue<Int>(offset + 1);
                        Int a2 = bytecode.GetValue<Int>(offset + 1 + a1.ByteSize());
                        return PrintTwoConstantsInstruction("LOAD", a1, a2, offset);
                    }
                case (byte)Instruction.LOAD_CONSTANT:
                    {
                        Int a1 = bytecode.GetValue<Int>(offset + 1);
                        Int a2 = bytecode.GetValue<Int>(offset + 1 + a1.ByteSize());
                        return PrintTwoConstantsInstruction("LOAD_CONSTANT", a1, a2, offset);
                    }
                case (byte)Instruction.WRITE:
                    {
                        Int a1 = bytecode.GetValue<Int>(offset + 1);
                        Int a2 = bytecode.GetValue<Int>(offset + 1 + a1.ByteSize());
                        return PrintTwoConstantsInstruction("WRITE", a1, a2, offset);
                    }
                case (byte)Instruction.SET_RESULT:
                    return PrintConstantInstruction("SET_RESULT", bytecode.GetValue<Int>(offset + 1), offset);
                case (byte)Instruction.RET:
                    return PrintInstruction("RET", offset);
                case (byte)Instruction.ADD_I:
                    return PrintInstruction("ADD_I", offset);
                case (byte)Instruction.GREATER_I:
                    return PrintInstruction("GREATER_I", offset);
                case (byte)Instruction.CALL_NATIVE:
                    return PrintConstantInstruction("CALL_NATIVE", bytecode.GetValue<Int>(offset + 1), offset);
                case (byte)Instruction.CALL:
                    return PrintConstantInstruction("CALL", bytecode.GetValue<Int>(offset + 1), offset);
                default:
                    result +=$"Unknown instruction {instruction.ToString()}{Environment.NewLine}";
                    return offset + 1;
            }
        }
        int PrintConstantInstruction(string name, Value constant, int offset)
        {
            result += $"{name} {constant.ToString()} {Environment.NewLine}";
            return offset + 1 + constant.ByteSize();
        }
        int PrintTwoConstantsInstruction(string name, Value c1, Value c2, int offset)
        {
            result += $"{name} {c1.ToString()} {c2.ToString()} {Environment.NewLine}";
            return offset + 1 + c1.ByteSize() + c2.ByteSize();
        }
        int PrintTwoAddressInstruction(string name, Int add1, Int add2, int offset)
        {
            result += $"{name} {offset + add1.Value} {offset + add2.Value} {Environment.NewLine}";
            return offset + 1 + add1.ByteSize() + add2.ByteSize();
        }
        int PrintOneAddressInstruction(string name, Int add, int offset)
        {
            result += $"{name} {offset + add.Value} {Environment.NewLine}";
            return offset + 1 + add.ByteSize();
        }
        int PrintInstruction(string name,int offset)
        {
            result +=name + Environment.NewLine;
            return offset + 1;
        }
    }
    /*public class CallStack
    {
        public class CallFrame
        {
            int offset;

        }
        int stackPointer;
    }*/
    public interface INativeFunction
    {
        byte[] Execute(Stack stack);
    }
    public class NativePow: INativeFunction
    {
        byte[] INativeFunction.Execute(Stack stack)
        {
            int a1_address = 0;
            int a2_address = Float.Sizeof();
            int stackLength = a2_address + +Float.Sizeof();
            Float a1 = stack.ReadValue<Float>(stackLength - a1_address);
            Float a2 = stack.ReadValue<Float>(stackLength - a2_address);
            Float result = new Float(Math.Pow(a1.Value,a2.Value));
            return result.ToBytes();
        }
    }
    public class Program
    {
        class Global { }
        public ByteArray Bytecode { get { return code; } }
        ByteArray code;
        ByteArray constants;
        Dictionary<string, int> functionTable;
        List<int> functionAddresses;
        Dictionary<Value, int> constantsMap;
        INativeFunction[] nativeFunctions;
        Global[] globals;
        public Program()
        {
            code = new ByteArray();
            constants = new ByteArray();
            constantsMap = new Dictionary<Value, int>();
            functionTable = new Dictionary<string, int>();
            functionAddresses = new List<int>();
            globals = new Global[1];
            nativeFunctions = new INativeFunction[1] { new NativePow()};
        }
        public byte[] LoadConstant(int offset, int size)
        {
            return constants.At(offset,size);
        }
        public void RegisterFunction(string name,int offset)
        {
            functionTable.Add(name,offset);
            functionAddresses.Add(offset);
        }
        public void AddConstant(Value value)
        {
            int offset = constants.Size;
            constants.AddConstant(value);
            constantsMap.Add(value,offset);
        }
        public int GetFunctionOffset(string function)
        {
            return functionTable[function];
        }
        public int GetFunctionOffset(int index)
        {
            return functionAddresses[index];
        }
        public byte[] CallNative(Stack stack,int index)
        {
#if DEBUG||TEST
            if (index >= nativeFunctions.Length||index<0)
                throw new Exception("Incorrect native function index " + index);
#endif
            return nativeFunctions[index].Execute(stack);
        }
    }
    public class VirtualMachine
    {
        Stack stack;
        Program program;
        byte[] resultRegister;
        int basePointer;
        //CallStack callStack;
        void InitVM(int stackSize, Program program)
        {
            stack = new Stack(stackSize);
            this.program = program;
            resultRegister = new byte[8];
        }
        void Interpret(int offset)
        {
            bool flag = true;
            while (flag) {
                byte instruction = program.Bytecode.At(offset);
                switch (instruction)
                {
                    case (byte)Instruction.CALL://save stack pointer in bp and jump to address
                        {
                            offset++;
                            Int functionIndex = program.Bytecode.GetValue<Int>(offset);
                            offset += Int.Sizeof();
                            stack.PushToStack(new Int(offset).ToBytes());//save return address
                            stack.PushToStack(new Int(basePointer).ToBytes());//save frame pointer
                            basePointer = stack.Pointer;//set frame pointer to top of the stack
                            offset = program.GetFunctionOffset(functionIndex.Value);//set offset to address value
                            break;
                        }
                    case (byte)Instruction.CALL_NATIVE:
                    case (byte)Instruction.ADD_SP:
                        {
                            offset++;
                            Int bytes = program.Bytecode.GetValue<Int>(offset);
                            offset += Int.Sizeof();
                            stack.Pointer += bytes.Value;
                            break;
                        }
                    case (byte)Instruction.SUB_SP:
                        {
                            offset++;
                            Int bytes = program.Bytecode.GetValue<Int>(offset);
                            offset += Int.Sizeof();
                            stack.Pointer -= bytes.Value;
                            break;
                        }
                    case (byte)Instruction.LOAD:
                        {
                            offset++;
                            Int _offset = program.Bytecode.GetValue<Int>(offset);
                            offset += Int.Sizeof();
                            Int size = program.Bytecode.GetValue<Int>(offset);
                            offset += Int.Sizeof();
                            byte[] bytes = stack.ReadBytes(_offset.Value,size.Value);
                            stack.PushToStack(bytes);
                            break;
                        }
                    case (byte)Instruction.LOAD_CONSTANT:
                        {
                            offset++;
                            Int _offset = program.Bytecode.GetValue<Int>(offset);
                            offset += Int.Sizeof();
                            Int size = program.Bytecode.GetValue<Int>(offset);
                            offset += Int.Sizeof();
                            stack.PushToStack(program.LoadConstant(_offset.Value,size.Value));
                            break;
                        }
                    case (byte)Instruction.ADD_I:
                        {
                            Int a2 = new Int();
                            a2.FromBytes(stack.Pop(Int.Sizeof()));
                            Int a1 = new Int();
                            a1.FromBytes(stack.Pop(Int.Sizeof()));
                            stack.PushToStack(new Int(a1.Value + a2.Value).ToBytes());
                            offset += 1;
                            break;
                        }
                    case (byte)Instruction.SUB_I:
                        {
                            Int a2 = new Int();
                            a2.FromBytes(stack.Pop(Int.Sizeof()));
                            Int a1 = new Int();
                            a1.FromBytes(stack.Pop(Int.Sizeof()));
                            stack.PushToStack(new Int(a1.Value - a2.Value).ToBytes());
                            offset += 1;
                            break;
                        }
                    case (byte)Instruction.MULT_I:
                        {
                            Int a2 = new Int();
                            a2.FromBytes(stack.Pop(Int.Sizeof()));
                            Int a1 = new Int();
                            a1.FromBytes(stack.Pop(Int.Sizeof()));
                            stack.PushToStack(new Int(a1.Value * a2.Value).ToBytes());
                            offset += 1;
                            break;
                        }
                    case (byte)Instruction.DIV_I:
                        {
                            Int a2 = new Int();
                            a2.FromBytes(stack.Pop(Int.Sizeof()));
                            Int a1 = new Int();
                            a1.FromBytes(stack.Pop(Int.Sizeof()));
                            stack.PushToStack(new Int(a1.Value / a2.Value).ToBytes());
                            offset += 1;
                            break;
                        }
                    case (byte)Instruction.MOD_I:
                        {
                            Int a2 = new Int();
                            a2.FromBytes(stack.Pop(Int.Sizeof()));
                            Int a1 = new Int();
                            a1.FromBytes(stack.Pop(Int.Sizeof()));
                            stack.PushToStack(new Int(a1.Value % a2.Value).ToBytes());
                            offset += 1;
                            break;
                        }
                    case (byte)Instruction.WRITE:
                        {
                            offset++;
                            Int _offset = program.Bytecode.GetValue<Int>(offset);
                            offset += Int.Sizeof();
                            Int size = program.Bytecode.GetValue<Int>(offset);
                            offset += Int.Sizeof();
                            byte[] bytes = stack.Pop(size.Value);
                            stack.WriteBytes(bytes,_offset.Value, size.Value);
                            break;
                        }
                    case (byte)Instruction.GREATER_I:
                        {
                            Int a2 = new Int();
                            a2.FromBytes(stack.Pop(Int.Sizeof()));
                            Int a1 = new Int();
                            a1.FromBytes(stack.Pop(Int.Sizeof()));
                            stack.PushToStack(new Bool(a1.Value>a2.Value).ToBytes());
                            offset += 1;
                            break;
                        }
                    case (byte)Instruction.SET_RESULT:
                        {
                            offset++;
                            Int size = program.Bytecode.GetValue<Int>(offset);
                            offset += Int.Sizeof();
                            byte[] bytes = stack.Pop(size.Value);
                            Array.Copy(bytes,resultRegister,size.Value);
                            break;
                        }
                    case (byte)Instruction.RET:
                        {
                            stack.Pointer = basePointer;
                            basePointer = new Int(stack.Pop(Int.Sizeof())).Value;//prev frame pointer
                            //stack now points to return address
                            Int address = new Int(stack.Pop(Int.Sizeof()));
                            if (address.Value == -1)//stop program
                                return;
                            offset = address.Value;//return to place right after function call
                            break;
                        }
                    case (byte)Instruction.JUMP_IF:
                        {
                            Bool condition = new Bool(stack.Pop(Bool.Sizeof()));
                            offset++;
                            Int _offset = program.Bytecode.GetValue<Int>(offset);
                            offset += Int.Sizeof();
                            if(condition.Value!=0)
                                offset += _offset.Value;
                            break;
                        }
                    case (byte)Instruction.JUMP_NIF:
                        {
                            Bool condition = new Bool(stack.Pop(Bool.Sizeof()));
                            offset++;
                            Int _offset = program.Bytecode.GetValue<Int>(offset);
                            offset += Int.Sizeof();
                            if (condition.Value == 0)
                                offset += _offset.Value;
                            break;
                        }
                    default:
                        throw new Exception("Unknown instruction");
                }
            }
        }
        //Done
        void Execute(string functionName, Value[] arguments)
        {
            stack.Empty();
            //add arguments to stack
            foreach (Value arg in arguments)
            {
                stack.PushToStack(arg.ToBytes());
            }
            //add -1 return address
            stack.PushToStack(new Int(-1).ToBytes());//address
            stack.PushToStack(new Int(-1).ToBytes());//base pointer
            basePointer = stack.Pointer;
            //load function address/offset
            int offset = program.GetFunctionOffset(functionName);
            //start executing instructions from that offset
            Interpret(offset);
        }
        //Done
        public T GetResult<T>() where T: Value, new()
        {
            T result = new T();
            result.FromBytes(resultRegister);
            //result.FromBytes(stack.Pop(result.ByteSize()));
            return result;
        }
        /*
         Stack:
         arguments,
         return address,
         locals
             
             */
        public static void Test()
        {
            /*
             int c;
             float r[5] = new float[5]{1.0,2.0,3.0,4.0,5.0};
             float callPow(int a,int b)
             {
                return pow(a,b);//native function
             }
             int powTest(int a,int b)
             {
                float result = pow(a,b);
                if(result>4.0)
                    return b;
                return a;
             }
            callPow:
                LOAD 16
                LOAD 12
                CALL_NATIVE pow
                GET_RESULT 4
                SAVE 4
                RET
            powTest:
                LOAD 16
                LOAD 12
                CALL pow
                SUB 8
                GET_RESULT 4
                LOAD_CONSTANT 0
                GREATER
                JUMP_IF 10
                LOAD 12
                SAVE 4
                RET
                LOAD 16
                SAVE 4
                RET
             */



            /*
             bool add(in int a)
             {
                int c;
                c = a + 5;
                return c>9;
             } 

            
            stack:
            -12
            int a
            -8
            address
            -4
            base pointer
            0
            c
            4
            a+5
            8

            bytecode:
            OP_ADD_SP 4
            OP_LOAD 16
            OP_LOAD_CONSTANT 0 4
            OP_ADD_I
            OP_WRITE 4 4//pop a+5 and put in stack in position currentStackPointer -4
            OP_LOAD 4
            OP_LOAD_CONSTANT 4 4 //offset size
            OP_GREATER //pop c value in stack and constant
            OP_SAVE 4//pop and save result in special 8 bytes register
            OP_RET //jump to address in stack
             */
            Program program = new Program();
            //TODO init program
            program.Bytecode.AddInstruction(Instruction.ADD_SP);
            program.Bytecode.AddConstant(new Int(4));
            program.Bytecode.AddInstruction(Instruction.LOAD);
            program.Bytecode.AddConstant(new Int(16));
            program.Bytecode.AddConstant(new Int(4));
            program.Bytecode.AddInstruction(Instruction.LOAD_CONSTANT);
            program.Bytecode.AddConstant(new Int(0));
            program.Bytecode.AddConstant(new Int(4));
            program.Bytecode.AddInstruction(Instruction.ADD_I);
            program.Bytecode.AddInstruction(Instruction.WRITE);
            program.Bytecode.AddConstant(new Int(4));
            program.Bytecode.AddConstant(new Int(4));
            program.Bytecode.AddInstruction(Instruction.LOAD);
            program.Bytecode.AddConstant(new Int(4));
            program.Bytecode.AddConstant(new Int(4));
            program.Bytecode.AddInstruction(Instruction.LOAD_CONSTANT);
            program.Bytecode.AddConstant(new Int(4));
            program.Bytecode.AddConstant(new Int(4));
            program.Bytecode.AddInstruction(Instruction.GREATER_I);
            program.Bytecode.AddInstruction(Instruction.SET_RESULT);
            program.Bytecode.AddConstant(new Int(4));
            program.Bytecode.AddInstruction(Instruction.RET);

            program.AddConstant(new Int(5));
            program.AddConstant(new Int(9));

            program.RegisterFunction("main",0);

            VirtualMachine vm = new VirtualMachine();
            vm.InitVM(4096, program);
            Disassembler disassembler = new Disassembler();
            Stream StdoutStream = Console.OpenStandardOutput();
            StreamWriter Stdout = new StreamWriter(StdoutStream);
            Stdout.WriteLine("\tTest virtual machine");
            string result = disassembler.Disassemble(program);
            Stdout.WriteLine(result);
            //TODO write result
            vm.Execute("main",new Value[] { new Int(10)});

            Bool programResult = vm.GetResult<Bool>();
            //TODO print programResult
            Stdout.WriteLine("Result: " + programResult.ToString());
        }
    }
}
