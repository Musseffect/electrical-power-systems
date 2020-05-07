using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerModel.NewModel.Recloser
{
#if A
    public enum Instruction
    {
        ADD_SP,//add_sp offset(2B),
        SUB_SP,//sub_sp offset(2B)
        LOAD,//load offset(2B) size(2B)
        WRITE,//write offset(2B) size(2B)
        LOAD_FROM_ADDRESS,//load_from_address pop 4B address 
        SAVE_TO_ADDRESS,//save_to_address pop 4B address
        LOAD_CONSTANT,//load_constant offset(2B) size(2B),
        LOAD_GLOBAL,//load_global address
        WRITE_GLOBAL,//write_global address
        ADD_CONSTANT_I,
        ADD_I,//add_i,
        SUB_I,
        MULT_I,
        DIV_I,
        MOD_I,
        NEGATE_I,
        ADD_F,
        SUB_F,
        MULT_F,
        DIV_F,
        NEGATE_F,
        GREATER_I,//greater_i,
        GREATEREQUAL_I,
        LESSER_I,
        LESSEREQUAL_I,
        EQUAL_I,
        NEQUAL_I,
        GREATER_F,//greater_i,
        GREATEREQUAL_F,
        LESSER_R,
        LESSEREQUAL_F,
        EQUAL_F,
        NEQUAL_F,
        NOT,
        SET_RESULT,//write_result size(4B)
        RET,
        CALL,//call index
        CALL_NATIVE, //call_native index
        JUMP_IF,//jump_if offset
        JUMP_NIF,//jump_nif offset
        JUMP,
        DTOI,//dtoi
        ITOD,//itod




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
        public T ReadValue<T>(int offset) where T:IValue,new()
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
            result += $"{offset.ToString("D4")}    ";
            short instruction = BitConverter.ToInt16(bytecode.At(offset,2),0);
            switch (instruction)
            {
                case (short)Instruction.ADD_SP:
                    return PrintConstantInstruction("ADD_SP", bytecode.GetValue<Int>(offset+1), offset);
                case (short)Instruction.LOAD:
                    {
                        Short a1 = bytecode.GetValue<Short>(offset + 2);
                        Short a2 = bytecode.GetValue<Short>(offset + 2 + a1.ByteSize());
                        return PrintTwoConstantsInstruction("LOAD", a1, a2, offset);
                    }
                case (short)Instruction.LOAD_CONSTANT:
                    {
                        Short a1 = bytecode.GetValue<Short>(offset + 2);
                        Short a2 = bytecode.GetValue<Short>(offset + 2 + a1.ByteSize());
                        return PrintTwoConstantsInstruction("LOAD_CONSTANT", a1, a2, offset);
                    }
                case (short)Instruction.WRITE:
                    {
                        Short a1 = bytecode.GetValue<Short>(offset + 2);
                        Short a2 = bytecode.GetValue<Short>(offset + 2 + a1.ByteSize());
                        return PrintTwoConstantsInstruction("WRITE", a1, a2, offset);
                    }
                case (short)Instruction.SET_RESULT:
                    return PrintConstantInstruction("SET_RESULT", bytecode.GetValue<Int>(offset + 2), offset);
                case (short)Instruction.RET:
                    return PrintInstruction("RET", offset);
                case (short)Instruction.ADD_I:
                    return PrintInstruction("ADD_I", offset);
                case (short)Instruction.GREATER_I:
                    return PrintInstruction("GREATER_I", offset);
                case (short)Instruction.CALL_NATIVE:
                    return PrintConstantInstruction("CALL_NATIVE", bytecode.GetValue<Int>(offset + 2), offset);
                case (short)Instruction.CALL:
                    return PrintConstantInstruction("CALL", bytecode.GetValue<Int>(offset + 2), offset);
                default:
                    result +=$"Unknown instruction {instruction.ToString()}{Environment.NewLine}";
                    return offset + 2;
            }
        }
        int PrintConstantInstruction(string name, IValue constant, int offset)
        {
            result += $"{name} {constant.ToString()} {Environment.NewLine}";
            return offset + 2 + constant.ByteSize();
        }
        int PrintTwoConstantsInstruction(string name,  IValue c1,  IValue c2, int offset)
        {
            result += $"{name} {c1.ToString()} {c2.ToString()} {Environment.NewLine}";
            return offset + 2 + c1.ByteSize() + c2.ByteSize();
        }
        int PrintInstruction(string name,int offset)
        {
            result +=name + Environment.NewLine;
            return offset + 2;
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
                short instruction = BitConverter.ToInt16(program.Bytecode.At(offset,2),0);
                offset += 2;
                switch (instruction)
                {
                    case (short)Instruction.CALL://save stack pointer in bp and jump to address
                        {
                            Short functionIndex = program.Bytecode.GetValue<Short>(offset);
                            offset += Int.Sizeof();
                            stack.PushToStack(new Int(offset).ToBytes());//save return address
                            stack.PushToStack(new Int(basePointer).ToBytes());//save frame pointer
                            basePointer = stack.Pointer;//set frame pointer to top of the stack
                            offset = program.GetFunctionOffset(functionIndex.Value);//set offset to address value
                            break;
                        }
                    case (short)Instruction.CALL_NATIVE:
                    case (short)Instruction.ADD_SP:
                        {
                            Short bytes = program.Bytecode.GetValue<Short>(offset);
                            offset += Short.Sizeof();
                            stack.Pointer += bytes.Value;
                            break;
                        }
                    case (short)Instruction.SUB_SP:
                        {
                            Short bytes = program.Bytecode.GetValue<Short>(offset);
                            offset += Short.Sizeof();
                            stack.Pointer -= bytes.Value;
                            break;
                        }
                    case (short)Instruction.LOAD:
                        {
                            Short _offset = program.Bytecode.GetValue<Short>(offset);
                            offset += Short.Sizeof();
                            Short size = program.Bytecode.GetValue<Short>(offset);
                            offset += Short.Sizeof();
                            byte[] bytes = stack.ReadBytes(_offset.Value,size.Value);
                            stack.PushToStack(bytes);
                            break;
                        }
                    case (short)Instruction.LOAD_CONSTANT:
                        {
                            Short _offset = program.Bytecode.GetValue<Short>(offset);
                            offset += Short.Sizeof();
                            Short size = program.Bytecode.GetValue<Short>(offset);
                            offset += Short.Sizeof();
                            stack.PushToStack(program.LoadConstant(_offset.Value,size.Value));
                            break;
                        }
                    case (short)Instruction.ADD_I:
                        {
                            Int a2 = new Int();
                            a2.FromBytes(stack.Pop(Int.Sizeof()));
                            Int a1 = new Int();
                            a1.FromBytes(stack.Pop(Int.Sizeof()));
                            stack.PushToStack(new Int(a1.Value + a2.Value).ToBytes());
                            break;
                        }
                    case (short)Instruction.SUB_I:
                        {
                            Int a2 = new Int();
                            a2.FromBytes(stack.Pop(Int.Sizeof()));
                            Int a1 = new Int();
                            a1.FromBytes(stack.Pop(Int.Sizeof()));
                            stack.PushToStack(new Int(a1.Value - a2.Value).ToBytes());
                            break;
                        }
                    case (short)Instruction.MULT_I:
                        {
                            Int a2 = new Int();
                            a2.FromBytes(stack.Pop(Int.Sizeof()));
                            Int a1 = new Int();
                            a1.FromBytes(stack.Pop(Int.Sizeof()));
                            stack.PushToStack(new Int(a1.Value * a2.Value).ToBytes());
                            break;
                        }
                    case (short)Instruction.DIV_I:
                        {
                            Int a2 = new Int();
                            a2.FromBytes(stack.Pop(Int.Sizeof()));
                            Int a1 = new Int();
                            a1.FromBytes(stack.Pop(Int.Sizeof()));
                            stack.PushToStack(new Int(a1.Value / a2.Value).ToBytes());
                            break;
                        }
                    case (short)Instruction.MOD_I:
                        {
                            Int a2 = new Int();
                            a2.FromBytes(stack.Pop(Int.Sizeof()));
                            Int a1 = new Int();
                            a1.FromBytes(stack.Pop(Int.Sizeof()));
                            stack.PushToStack(new Int(a1.Value % a2.Value).ToBytes());
                            break;
                        }
                    case (short)Instruction.WRITE:
                        {
                            Short _offset = program.Bytecode.GetValue<Short>(offset);
                            offset += Short.Sizeof();
                            Short size = program.Bytecode.GetValue<Short>(offset);
                            offset += Short.Sizeof();
                            byte[] bytes = stack.Pop(size.Value);
                            stack.WriteBytes(bytes,_offset.Value, size.Value);
                            break;
                        }
                    case (short)Instruction.GREATER_I:
                        {
                            Int a2 = new Int();
                            a2.FromBytes(stack.Pop(Int.Sizeof()));
                            Int a1 = new Int();
                            a1.FromBytes(stack.Pop(Int.Sizeof()));
                            stack.PushToStack(new Bool(a1.Value>a2.Value).ToBytes());
                            offset += 2;
                            break;
                        }
                    case (short)Instruction.SET_RESULT:
                        {
                            offset += 2;
                            Short size = program.Bytecode.GetValue<Short>(offset);
                            offset += Short.Sizeof();
                            byte[] bytes = stack.Pop(size.Value);
                            Array.Copy(bytes,resultRegister,size.Value);
                            break;
                        }
                    case (short)Instruction.RET:
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
                    case (short)Instruction.JUMP_IF:
                        {
                            Bool condition = new Bool(stack.Pop(Bool.Sizeof()));
                            offset++;
                            Short _offset = program.Bytecode.GetValue<Short>(offset);
                            offset += Short.Sizeof();
                            if(condition.Value!=0)
                                offset += _offset.Value;
                            break;
                        }
                    case (short)Instruction.JUMP_NIF:
                        {
                            Bool condition = new Bool(stack.Pop(Bool.Sizeof()));
                            offset++;
                            Short _offset = program.Bytecode.GetValue<Short>(offset);
                            offset += Short.Sizeof();
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
        void Execute(string functionName, IValue[] arguments)
        {
            stack.Empty();
            //add arguments to stack
            foreach (IValue arg in arguments)
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
        public T GetResult<T>() where T: IValue, new()
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
            program.Bytecode.AddConstant(new Short(4));
            program.Bytecode.AddInstruction(Instruction.LOAD);
            program.Bytecode.AddConstant(new Short(16));
            program.Bytecode.AddConstant(new Short(4));
            program.Bytecode.AddInstruction(Instruction.LOAD_CONSTANT);
            program.Bytecode.AddConstant(new Short(0));
            program.Bytecode.AddConstant(new Short(4));
            program.Bytecode.AddInstruction(Instruction.ADD_I);
            program.Bytecode.AddInstruction(Instruction.WRITE);
            program.Bytecode.AddConstant(new Short(4));
            program.Bytecode.AddConstant(new Short(4));
            program.Bytecode.AddInstruction(Instruction.LOAD);
            program.Bytecode.AddConstant(new Short(4));
            program.Bytecode.AddConstant(new Short(4));
            program.Bytecode.AddInstruction(Instruction.LOAD_CONSTANT);
            program.Bytecode.AddConstant(new Short(4));
            program.Bytecode.AddConstant(new Short(4));
            program.Bytecode.AddInstruction(Instruction.GREATER_I);
            program.Bytecode.AddInstruction(Instruction.SET_RESULT);
            program.Bytecode.AddConstant(new Short(4));
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
            vm.Execute("main",new IValue[] { new Int(10)});

            Bool programResult = vm.GetResult<Bool>();
            //TODO print programResult
            Stdout.WriteLine("Result: " + programResult.ToString());
        }
    }
#endif
}
