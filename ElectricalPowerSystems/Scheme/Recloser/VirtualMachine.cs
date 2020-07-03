using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Scheme.Recloser
{
    public enum Instruction
    {
        POP,
        ARR_POP,
        DUP,//duplicate last value on stack
        RCREATE,//rcreate index 
        IRLOAD,//pop int ref | push int from (0 + ref)
        FRLOAD,//pop int ref | push float from (0 + ref)
        IRSTORE,//pop int ref, int value| push int value | save value to (0 + ref)
        FRSTORE,//pop int ref, float value| push float value | save value to (0 + ref)
        ILOAD,//ILOAD index , push int
        FLOAD,//FLOAT index, push float
        ISTORE,//ISTORE index, pop int| push int value
        FSTORE,//ISTORE index, pop float| push float value
        IPUSH,//IPUSH int, push int
        FPUSH,//FPUSH float, push float
        IMUL,//IMUL, pop int,int | push int
        IADD,
        IDIV,
        ISUB,
        IMOD,
        INEG,
        FADD,
        FSUB,
        FMUL,
        FMOD,
        FDIV,
        FNEG,

        IEQ,
        FEQ,
        INEQ,
        FNEQ,
        IGR,
        FGR,
        IGREQ,
        FGREQ,
        ILS,
        FLS,
        ILSEQ,
        FLSEQ,
        BNOT,
        BAND,
        BOR,
        
        FRET,
        IRET,
        RET,
        CALL,//call index
        CALL_NATIVE, //call_native index
        JUMP_IF,//jump_if offset
        JUMP_NIF,//jump_nif offset
        JUMP,
        F2I,//dtoi, pop float | push int
        I2F,//itod, pop int | push float
        NOP
    }
    public class Stack
    {
        IValue[] storage;
        int pointer;
        public int Pointer { get { return pointer; } set { pointer = value; } }
        public Stack(int size)
        {
            storage = new IValue[size];
            pointer = 0;
        }
        public IValue LastValue()
        {
            return storage[pointer-1];
        }
        public IValue At(int index)
        {
            if (index >= pointer)
                throw new Exception("Invalid index in Stack.At(index)");
            return storage[index];
        }
        public void Set(int index, IValue value)
        {
            if (index >= pointer)
                throw new Exception("Invalid index in Stack.At(index)");
            storage[index] = value;
        }
        public IValue Pop()
        {
            if (pointer == 0)
                throw new Exception("Invalid pop call");
            return storage[--pointer];
        }
        public int Push(IValue value)
        {
            if (pointer == storage.Length)
                throw new Exception("Stack overflow");
            storage[pointer] = value;
            return ++pointer;
        }
        public void Empty()
        {
            pointer = 0;
        }
    }
    public class Disassembler
    {
        string result;
        public string Disassemble(Program program, string programText)
        {
            result = "";
            int textLineNumber = 0;
            int functionIterator = 0;
            int lineIterator = 0;
            int localLineIterator = 0;
            var reader = new StringReader(programText);
            string lineText = "";
            result += $"OFFS  LINE  INTSRUCTION{Environment.NewLine}";
            for (int offset = 0; offset < program.Bytecode.Size;)
            {
                Program.Line line = program.Lines[lineIterator];
                if (functionIterator < program.Functions.Count)
                {
                    Compiler.FunctionType function = program.Functions[functionIterator];
                    if (offset == function.Offset)
                    {
                        result += $"{(functionIterator!=0?Environment.NewLine:"")}{programText.Substring(function.SrcPosBegin, function.SrcPosEnd - function.SrcPosBegin)}{Environment.NewLine}";
                        functionIterator++;
                    }
                }
                if (++localLineIterator == line.Count)
                {
                    localLineIterator = 0;
                    lineIterator++;
                }
                int currentLine = line.LineNumber;
                string currentLineText = " |  ";
                if (textLineNumber < currentLine)
                {
                    while (textLineNumber < currentLine)
                    {
                        lineText = reader.ReadLine();
                        textLineNumber++;
                    }
                    result +=$"{lineText}{Environment.NewLine}";
                    currentLineText = currentLine.ToString("D4");
                }
                offset = DisassembleInstruction(program, offset, currentLineText);
            }
            return result;
        }
        int DisassembleInstruction(Program program, int offset, string currentLine)
        {
            ByteArray bytecode = program.Bytecode;
            result += $"{offset.ToString("D4")}  {currentLine}  ";
            byte instruction = bytecode.At(offset);
            switch (instruction)
            {
                case (byte)Instruction.POP:
                    return PrintInstruction("pop", offset);
                case (byte)Instruction.ARR_POP:
                    return PrintConstantInstruction("arr_pop", bytecode.GetValue<Short>(offset + 1), offset);
                case (byte)Instruction.DUP:
                    return PrintInstruction("dup", offset);
                case (byte)Instruction.RCREATE:
                    return PrintConstantInstruction("rcreate", bytecode.GetValue<Short>(offset + 1), offset);
                case (byte)Instruction.IRLOAD:
                    return PrintInstruction("irload", offset);
                case (byte)Instruction.FRLOAD:
                    return PrintInstruction("frload", offset);
                case (byte)Instruction.IRSTORE:
                    return PrintInstruction("irstore", offset);
                case (byte)Instruction.FRSTORE:
                    return PrintInstruction("frstore", offset);
                case (byte)Instruction.ILOAD:
                    return PrintConstantInstruction("iload", bytecode.GetValue<Short>(offset + 1), offset);
                case (byte)Instruction.FLOAD:
                    return PrintConstantInstruction("fload", bytecode.GetValue<Short>(offset + 1), offset);
                case (byte)Instruction.ISTORE:
                    return PrintConstantInstruction("istore", bytecode.GetValue<Short>(offset + 1), offset);
                case (byte)Instruction.FSTORE:
                    return PrintConstantInstruction("fstore", bytecode.GetValue<Short>(offset + 1), offset);
                case (byte)Instruction.IPUSH:
                    return PrintConstantInstruction("ipush", bytecode.GetValue<Int>(offset + 1), offset);
                case (byte)Instruction.FPUSH:
                    return PrintConstantInstruction("fpush", bytecode.GetValue<Float>(offset + 1), offset);
                case (byte)Instruction.IMUL:
                    return PrintInstruction("imul", offset);
                case (byte)Instruction.IADD:
                    return PrintInstruction("iadd", offset);
                case (byte)Instruction.IDIV:
                    return PrintInstruction("idiv", offset);
                case (byte)Instruction.ISUB:
                    return PrintInstruction("isub", offset);
                case (byte)Instruction.IMOD:
                    return PrintInstruction("imod", offset);
                case (byte)Instruction.INEG:
                    return PrintInstruction("ineg", offset);
                case (byte)Instruction.FADD:
                    return PrintInstruction("fadd", offset);
                case (byte)Instruction.FSUB:
                    return PrintInstruction("fsub", offset);
                case (byte)Instruction.FMUL:
                    return PrintInstruction("fmul", offset);
                case (byte)Instruction.FDIV:
                    return PrintInstruction("fdiv", offset);
                case (byte)Instruction.FNEG:
                    return PrintInstruction("fneg", offset);
                case (byte)Instruction.FMOD:
                    return PrintInstruction("fmod", offset);
                case (byte)Instruction.IEQ:
                    return PrintInstruction("ieq", offset);
                case (byte)Instruction.FEQ:
                    return PrintInstruction("feq", offset);
                case (byte)Instruction.INEQ:
                    return PrintInstruction("ineq", offset);
                case (byte)Instruction.FNEQ:
                    return PrintInstruction("fneq", offset);
                case (byte)Instruction.IGR:
                    return PrintInstruction("igr", offset);
                case (byte)Instruction.FGR:
                    return PrintInstruction("fgr", offset);
                case (byte)Instruction.IGREQ:
                    return PrintInstruction("igreq", offset);
                case (byte)Instruction.FGREQ:
                    return PrintInstruction("fgreq", offset);
                case (byte)Instruction.ILS:
                    return PrintInstruction("ils", offset);
                case (byte)Instruction.FLS:
                    return PrintInstruction("fls", offset);
                case (byte)Instruction.ILSEQ:
                    return PrintInstruction("ilseq", offset);
                case (byte)Instruction.FLSEQ:
                    return PrintInstruction("flseq", offset);
                case (byte)Instruction.BNOT:
                    return PrintInstruction("bnot", offset);
                case (byte)Instruction.BAND:
                    return PrintInstruction("band", offset);
                case (byte)Instruction.BOR:
                    return PrintInstruction("bor", offset);
                case (byte)Instruction.FRET:
                    return PrintInstruction("fret", offset);
                case (byte)Instruction.IRET:
                    return PrintInstruction("iret", offset);
                case (byte)Instruction.RET:
                    return PrintInstruction("ret", offset);
                case (byte)Instruction.CALL:
                    return PrintConstantInstruction("call", bytecode.GetValue<Short>(offset + 1), offset);
                case (byte)Instruction.CALL_NATIVE:
                    return PrintConstantInstruction("call_native", bytecode.GetValue<Short>(offset + 1), offset);
                case (byte)Instruction.JUMP_IF:
                    return PrintConstantInstruction("jump_if", bytecode.GetValue<Short>(offset + 1), offset);
                case (byte)Instruction.JUMP_NIF:
                    return PrintConstantInstruction("jump_nif", bytecode.GetValue<Short>(offset + 1), offset);
                case (byte)Instruction.JUMP:
                    return PrintConstantInstruction("jump", bytecode.GetValue<Short>(offset + 1), offset);
                case (byte)Instruction.F2I:
                    return PrintInstruction("f2i", offset);
                case (byte)Instruction.I2F:
                    return PrintInstruction("i2f", offset);
                case (byte)Instruction.NOP:
                    return PrintInstruction("nop", offset);
                default:
                    result +=$"Unknown instruction {instruction.ToString()}{Environment.NewLine}";
                    return offset + 1;
            }
        }
        int PrintConstantInstruction(string name, IValue constant, int offset)
        {
            result += $"{name} {constant.ToString()} {Environment.NewLine}";
            return offset + 1 + constant.ByteSize();
        }
        int PrintTwoConstantsInstruction(string name,  IValue c1,  IValue c2, int offset)
        {
            result += $"{name} {c1.ToString()} {c2.ToString()} {Environment.NewLine}";
            return offset + 1 + c1.ByteSize() + c2.ByteSize();
        }
        int PrintInstruction(string name,int offset)
        {
            result +=name + Environment.NewLine;
            return offset + 1;
        }
    }
    public class CallStack
    {
        public class CallFrame
        {
            int stackPointer;
            int commandOffset;
            int functionIndex;
            public int CommandOffset { get { return commandOffset; } }
            public int StackPointer { get { return stackPointer; } }
            public int FunctionIndex { get { return functionIndex; } }
            public CallFrame(int stackPointer, int commandOffset, int functionIndex)
            {
                this.stackPointer = stackPointer;
                this.commandOffset = commandOffset;
                this.functionIndex = functionIndex;
            }
        }
        CallFrame []frames;
        int stackPointer;
        public int Pointer { get { return stackPointer; } }
        public CallStack(int size)
        {
            frames = new CallFrame[size];
            stackPointer = 0;
        }
        public CallFrame Last()
        {
            return frames[stackPointer - 1];
        }
        public CallFrame Pop()
        {
            return frames[--stackPointer];
        }
        public void Push(CallFrame frame)
        {
            if (stackPointer == frames.Length)
                throw new Exception("CallStack overflow");
            frames[stackPointer++] = frame;
        }
    }
    
    public class VirtualMachine
    {
        Stack stack;
        CallStack callStack;
        Program program;
        int basePointer;
        //CallStack callStack;
        public void InitVM(int stackSize,int callStackSize, Program program)
        {
            stack = new Stack(stackSize);
            callStack = new CallStack(callStackSize);
            this.program = program;
            //init stack
            foreach (var global in this.program.Globals)
            {
                List<IValue> values = (global.Type as Compiler.NonRefType).DefaultValue();
                foreach(var value in values)
                    stack.Push(value);
            }
            basePointer = stack.Pointer;
        }
        void Interpret(int offset)
        {
            bool flag = true;
            while (flag) {
                byte instruction = program.Bytecode.At(offset);
                offset += 1;
                switch (instruction)
                {
                    /*
                    RCREATE,//rcreate index 
                    IRLOAD,//pop int ref | push int from (0 + ref)
                    FRLOAD,//pop int ref | push float from (0 + ref)
                    IRSAVE,//pop int ref, int value | save value to (0 + ref)
                    FRSAVE,//pop int ref, float value | save value to (0 + ref)
                    GILOAD,//GILOAD index,  push int from stackBottom + index
                    GFLOAD,//GFLOAD index,  push float from stackBottom + index
                    GISTORE,
                    GFSTORE,//GFSTORE index,  pop float to stackBottom + index
                    ILOAD,//ILOAD index , push int
                    FLOAD,//FLOAT index, push float
                    ISTORE,//ISTORE index, pop int
                    FSTORE,//ISTORE index, pop float*/
                    case (byte)Instruction.POP://Done
                        {
                            stack.Pop();
                            break;
                        }
                    case (byte)Instruction.ARR_POP:
                        {
                            Short slots = program.Bytecode.GetValue<Short>(offset);
                            offset += Short.Sizeof();
                            for (int i = 0; i < slots.Value; i++)
                            {
                                stack.Pop();
                            }
                            break;
                        }
                    case (byte)Instruction.DUP://Done
                        {
                            stack.Push(stack.LastValue());
                            break;
                        }
                    case (byte)Instruction.RCREATE://Done?
                        {
                            Short index = program.Bytecode.GetValue<Short>(offset);
                            offset += Short.Sizeof();
                            Int address = new Int(callStack.Last().StackPointer + index.Value);
                            stack.Push(address);
                            break;
                        }
                    case (byte)Instruction.IRLOAD:
                        {
                            Int @ref = stack.Pop() as Int;
                            stack.Push(stack.At(@ref.Value) as Int);
                            break;
                        }
                    case (byte)Instruction.FRLOAD:
                        {
                            Int @ref = stack.Pop() as Int;
                            stack.Push(stack.At(@ref.Value) as Float);
                            break;
                        }
                    case (byte)Instruction.IRSTORE:
                        {
                            Int @ref = stack.Pop() as Int;
                            Int value = stack.Pop() as Int;
                            stack.Set(@ref.Value, value);
                            break;
                        }
                    case (byte)Instruction.FRSTORE:
                        {
                            Int @ref = stack.Pop() as Int;
                            Float value = stack.Pop() as Float;
                            stack.Set(@ref.Value, value);
                            break;
                        }
                    case (byte)Instruction.ILOAD:
                        {
                            Short index = program.Bytecode.GetValue<Short>(offset);
                            offset += Short.Sizeof();
                            stack.Push(stack.At(callStack.Last().StackPointer + index.Value) as Int);
                            break;
                        }
                    case (byte)Instruction.FLOAD:
                        {
                            Short index = program.Bytecode.GetValue<Short>(offset);
                            offset += Short.Sizeof();
                            stack.Push(stack.At(callStack.Last().StackPointer + index.Value) as Float);
                            break;
                        }
                    case (byte)Instruction.ISTORE:
                        {
                            Short index = program.Bytecode.GetValue<Short>(offset);
                            offset += Short.Sizeof();
                            Int value = stack.Pop() as Int;
                            stack.Set(callStack.Last().StackPointer + index.Value, value);
                            break;
                        }
                    case (byte)Instruction.FSTORE:
                        {
                            Short index = program.Bytecode.GetValue<Short>(offset);
                            offset += Short.Sizeof();
                            Float value = stack.Pop() as Float;
                            stack.Set(callStack.Last().StackPointer + index.Value, value);
                            break;
                        }
                    case (byte)Instruction.IPUSH://Done
                        {
                            Int value = program.Bytecode.GetValue<Int>(offset);
                            offset +=Int.Sizeof();
                            stack.Push(value);
                            break;//IPUSH int, push int
                        }
                    case (byte)Instruction.FPUSH://Done
                        {
                            Float value = program.Bytecode.GetValue<Float>(offset);
                            offset += Float.Sizeof();
                            stack.Push(value);
                            break;//FPUSH float, push float
                        }
                    case (byte)Instruction.IMUL://Done
                        {
                            Int b = stack.Pop() as Int;
                            Int a = stack.Pop() as Int;
                            stack.Push(new Int(a.Value * b.Value));
                            break;//IMUL, pop int,int | push int
                        }
                    case (byte)Instruction.IADD://Done
                        {
                            Int b = stack.Pop() as Int;
                            Int a = stack.Pop() as Int;
                            stack.Push(new Int(a.Value + b.Value));
                            break;
                        }
                    case (byte)Instruction.IDIV://Done
                        {
                            Int b = stack.Pop() as Int;
                            Int a = stack.Pop() as Int;
                            stack.Push(new Int(a.Value / b.Value));
                            break;
                        }
                    case (byte)Instruction.ISUB://Done
                        {
                            Int b = stack.Pop() as Int;
                            Int a = stack.Pop() as Int;
                            stack.Push(new Int(a.Value - b.Value));
                            break;
                        }
                    case (byte)Instruction.IMOD://Done
                        {
                            Int b = stack.Pop() as Int;
                            Int a = stack.Pop() as Int;
                            stack.Push(new Int(a.Value % b.Value));
                            break;
                        }
                    case (byte)Instruction.INEG://Done
                        {
                            Int a = stack.Pop() as Int;
                            stack.Push(new Int(-a.Value));
                            break;
                        }
                    case (byte)Instruction.FADD://Done
                        {
                            Float b = stack.Pop() as Float;
                            Float a = stack.Pop() as Float;
                            stack.Push(new Float(a.Value + b.Value));
                            break;
                        }
                    case (byte)Instruction.FMOD:
                        {
                            Float b = stack.Pop() as Float;
                            Float a = stack.Pop() as Float;
                            stack.Push(new Float(a.Value % b.Value));
                            break;
                        }
                    case (byte)Instruction.FSUB://Done
                        {
                            Float b = stack.Pop() as Float;
                            Float a = stack.Pop() as Float;
                            stack.Push(new Float(a.Value - b.Value));
                            break;
                        }
                    case (byte)Instruction.FMUL://Done
                        {
                            Float b = stack.Pop() as Float;
                            Float a = stack.Pop() as Float;
                            stack.Push(new Float(a.Value * b.Value));
                            break;
                        }
                    case (byte)Instruction.FDIV://Done
                        {
                            Float b = stack.Pop() as Float;
                            Float a = stack.Pop() as Float;
                            stack.Push(new Float(a.Value / b.Value));
                            break;
                        }
                    case (byte)Instruction.FNEG://Done
                        {
                            Float a = stack.Pop() as Float;
                            stack.Push(new Float(-a.Value));
                            break;
                        }
                    case (byte)Instruction.IEQ:
                        {
                            Int b = stack.Pop() as Int;
                            Int a = stack.Pop() as Int;
                            stack.Push(new Int(a.Value == b.Value?1:0));
                            break;
                        }
                    case (byte)Instruction.FEQ:
                        {
                            Float b = stack.Pop() as Float;
                            Float a = stack.Pop() as Float;
                            stack.Push(new Int(a.Value == b.Value ? 1 : 0));
                            break;
                        }
                    case (byte)Instruction.INEQ:
                        {
                            Int b = stack.Pop() as Int;
                            Int a = stack.Pop() as Int;
                            stack.Push(new Int(a.Value == b.Value ? 0 : 1));
                            break;
                        }
                    case (byte)Instruction.FNEQ:
                        {
                            Float b = stack.Pop() as Float;
                            Float a = stack.Pop() as Float;
                            stack.Push(new Int(a.Value == b.Value ? 0 : 1));
                            break;
                        }
                    case (byte)Instruction.IGR:
                        {
                            Int b = stack.Pop() as Int;
                            Int a = stack.Pop() as Int;
                            stack.Push(new Int(a.Value > b.Value ? 1 : 0));
                            break;
                        }
                    case (byte)Instruction.FGR:
                        {
                            Float b = stack.Pop() as Float;
                            Float a = stack.Pop() as Float;
                            stack.Push(new Int(a.Value > b.Value ? 1 : 0));
                            break;
                        }
                    case (byte)Instruction.IGREQ:
                        {
                            Int b = stack.Pop() as Int;
                            Int a = stack.Pop() as Int;
                            stack.Push(new Int(a.Value >= b.Value ? 1 : 0));
                            break;
                        }
                    case (byte)Instruction.FGREQ:
                        {
                            Float b = stack.Pop() as Float;
                            Float a = stack.Pop() as Float;
                            stack.Push(new Int(a.Value >= b.Value ? 1 : 0));
                            break;
                        }
                    case (byte)Instruction.ILS:
                        {
                            Int b = stack.Pop() as Int;
                            Int a = stack.Pop() as Int;
                            stack.Push(new Int(a.Value < b.Value ? 1 : 0));
                            break;
                        }
                    case (byte)Instruction.FLS:
                        {
                            Float b = stack.Pop() as Float;
                            Float a = stack.Pop() as Float;
                            stack.Push(new Int(a.Value < b.Value ? 1 : 0));
                            break;
                        }
                    case (byte)Instruction.ILSEQ:
                        {
                            Int b = stack.Pop() as Int;
                            Int a = stack.Pop() as Int;
                            stack.Push(new Int(a.Value <= b.Value ? 1 : 0));
                            break;
                        }
                    case (byte)Instruction.FLSEQ:
                        {
                            Float b = stack.Pop() as Float;
                            Float a = stack.Pop() as Float;
                            stack.Push(new Int(a.Value <= b.Value ? 1 : 0));
                            break;
                        }
                    case (byte)Instruction.BNOT:
                        {
                            Int a = stack.Pop() as Int;
                            stack.Push(new Int(a.Value == 0 ? 1 : 0));
                            break;
                        }
                    case (byte)Instruction.BAND:
                        {
                            Int b = stack.Pop() as Int;
                            Int a = stack.Pop() as Int;
                            stack.Push(new Int((a.Value != 0&&b.Value!=0)?1:0));
                            break;
                        }
                    case (byte)Instruction.BOR:
                        {
                            Int b = stack.Pop() as Int;
                            Int a = stack.Pop() as Int;
                            stack.Push(new Int((a.Value != 0 || b.Value != 0) ? 1 : 0));
                            break;
                        }
                    /*case (byte)Instruction.WRITE:
                        {
                            Short _offset = program.Bytecode.GetValue<Short>(offset);
                            offset += Short.Sizeof();
                            Short size = program.Bytecode.GetValue<Short>(offset);
                            offset += Short.Sizeof();
                            byte[] bytes = stack.Pop(size.Value);
                            stack.WriteBytes(bytes, _offset.Value, size.Value);
                            break;
                        }*/
                    case (byte)Instruction.FRET:
                        {
                            Float value = stack.Pop() as Float;
                            CallStack.CallFrame frame = callStack.Pop();
                            offset = frame.CommandOffset;//return to command right after function call
                            stack.Pointer = frame.StackPointer;//reset stack pointer
                            stack.Push(value);//push result
                            break;
                        }
                    case (byte)Instruction.IRET:
                        {
                            Int value = stack.Pop() as Int;
                            CallStack.CallFrame frame = callStack.Pop();
                            offset = frame.CommandOffset;//return to command right after function call
                            stack.Pointer = frame.StackPointer;//reset stack pointer
                            stack.Push(value);//push result
                            break;
                        }
                    case (byte)Instruction.RET:
                        {
                            CallStack.CallFrame frame = callStack.Pop();
                            offset = frame.CommandOffset;//return to command right after function call
                            stack.Pointer = frame.StackPointer;//reset stack pointer
                            break;
                        }
                    case (byte)Instruction.CALL://save stack pointer in bp and jump to address //Done
                        {
                            Short functionIndex = program.Bytecode.GetValue<Short>(offset);
                            offset += Short.Sizeof();
                            Compiler.FunctionType function = program.GetFunctionType((int)functionIndex.Value);
                            CallStack.CallFrame frame = new CallStack.CallFrame( stack.Pointer - function.Signature.GetSlotSize(), offset, functionIndex.Value);
                            callStack.Push(frame);
                            offset = function.Offset;//set offset to address value
                            break;
                        }
                    case (byte)Instruction.CALL_NATIVE:
                        {
                            Short index = program.Bytecode.GetValue<Short>(offset);
                            offset += Short.Sizeof();
                            program.CallNative(stack, index.Value);
                            break;
                        }
                    case (byte)Instruction.JUMP_IF:
                        {
                            Int condition = stack.Pop() as Int;
                            Short _offset = program.Bytecode.GetValue<Short>(offset);
                            offset += Short.Sizeof();
                            if (condition.Value != 0)
                                offset = _offset.Value;
                            break;
                        }
                    case (byte)Instruction.JUMP_NIF:
                        {
                            Int condition = stack.Pop() as Int;
                            Short _offset = program.Bytecode.GetValue<Short>(offset);
                            offset += Short.Sizeof();
                            if (condition.Value == 0)
                                offset = _offset.Value;
                            break;
                        }
                    case (byte)Instruction.JUMP:
                        {
                            Short _offset = program.Bytecode.GetValue<Short>(offset);
                            offset += Short.Sizeof();
                            offset = _offset.Value;
                            break;
                        }
                    case (byte)Instruction.F2I:
                        {
                            Float val = stack.Pop() as Float;
                            stack.Push(new Int((int)val.Value));
                            break;//dtoi, pop float | push int
                        }
                    case (byte)Instruction.I2F://Done
                        {
                            Int val = stack.Pop() as Int;
                            stack.Push(new Float((double)val.Value));
                            break;
                        }
                    case (byte)Instruction.NOP://Done
                        break;
                    default:
                        throw new Exception("Unknown instruction");
                }
                if (callStack.Pointer == 0)
                {
                    return;//stop program, float result would be on top of the stack
                }
            }
        }
        //Done
        public IValue Execute(string functionName, IValue[] arguments)
        {
            //add arguments to stack
            foreach (IValue arg in arguments)
            {
                stack.Push(arg);
            }
            //load function address/offset
            Compiler.FunctionType function = program.GetFunctionType(functionName);
            int offset = function.Offset;
            callStack.Push(new CallStack.CallFrame(basePointer,offset,program.GetFunctionIndex(functionName)));
            //start executing instructions from that offset
            Interpret(offset);
            IValue result = null;
            switch (function.Signature.ReturnType.Type)
            {
                case Compiler.BasicType.BType.Void:
                    result = null;
                    break;
                case Compiler.BasicType.BType.Bool:
                case Compiler.BasicType.BType.Int:
                    result = stack.Pop() as Int;
                    break;
                case Compiler.BasicType.BType.Float:
                    result = stack.Pop() as Float;
                    break;
            }
            stack.Pointer = basePointer;//restore stack
            return result;
        }
        public IValue[] ExtractFromStack(int size)
        {
            int i = 0;
            IValue[] result = new IValue[size];
            while (i < size)
            {
                result[size-1-i] = stack.Pop();
            }
            return result;
        }
        public static void Test()
        {
            /*
             int c;
             float r[5] = new float[5]{1.0,2.0,3.0,4.0,5.0};
             float callPow(inout int a,int b)
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
              0  RLOAD(1B) 0(2B)
              3  IRLOAD(1B)
              4  LOAD(1B) 1(2B)
              7  CALL_NATIVE(1B) pow(2B)
              10 FRET(1B)
            powTest:
              11 RCREATE(1B) ()0
              14 LOAD 1
              17 CALL pow
              20 FSTORE 2
              23 FPUSH (8B)4.0
              31 FGR
              32 JUMP_IF 10
              35 LOAD 12
              38 SAVE 4
              41 FRET
              42 LOAD 16
              45 SAVE 4
              46 FRET
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

            Stream StdoutStream = Console.OpenStandardOutput();
            StreamWriter Stdout = new StreamWriter(StdoutStream);
            #region initNativeFunctions
            //Init native functions
            List<Compiler.NativeFunctionType> nativeFunctions = new List<Compiler.NativeFunctionType>();
            nativeFunctions.Add(new Compiler.NativeFunctionType() {
                Function = new NativeFAbs(),
                Index = 0,
                Name = "fabs",
                Signature = new Compiler.FunctionSignature ( new List<Compiler.FunctionSignature.ArgType> {
                    new Compiler.FunctionSignature.ArgType(new Compiler.FloatType(),true,"x")
                }, new Compiler.FloatType()),
            });
            nativeFunctions.Add(new Compiler.NativeFunctionType()
            {
                Function = new NativeFPow(),
                Index = 0,
                Name = "fpow",
                Signature = new Compiler.FunctionSignature(new List<Compiler.FunctionSignature.ArgType> {
                    new Compiler.FunctionSignature.ArgType(new Compiler.FloatType(),true,"x"),
                    new Compiler.FunctionSignature.ArgType(new Compiler.FloatType(),true,"y")
                }, new Compiler.FloatType()),
            });
            nativeFunctions.Add(new Compiler.NativeFunctionType()
            {
                Function = new NativeFMin(),
                Index = 0,
                Name = "fmin",
                Signature = new Compiler.FunctionSignature(new List<Compiler.FunctionSignature.ArgType> {
                    new Compiler.FunctionSignature.ArgType(new Compiler.FloatType(),true,"a"),
                    new Compiler.FunctionSignature.ArgType(new Compiler.FloatType(),true,"b")
                }, new Compiler.FloatType()),
            });
            nativeFunctions.Add(new Compiler.NativeFunctionType()
            {
                Function = new NativeFMax(),
                Index = 0,
                Name = "fmax",
                Signature = new Compiler.FunctionSignature(new List<Compiler.FunctionSignature.ArgType> {
                    new Compiler.FunctionSignature.ArgType(new Compiler.FloatType(),true,"a"),
                    new Compiler.FunctionSignature.ArgType(new Compiler.FloatType(),true,"b")
                }, new Compiler.FloatType()),
            });
            nativeFunctions.Add(new Compiler.NativeFunctionType()
            {
                Function = new NativeFStep(),
                Index = 0,
                Name = "step",
                Signature = new Compiler.FunctionSignature(new List<Compiler.FunctionSignature.ArgType> {
                    new Compiler.FunctionSignature.ArgType(new Compiler.FloatType(),true,"x")
                }, new Compiler.FloatType()),
            });
            nativeFunctions.Add(new Compiler.NativeFunctionType()
            {
                Function = new NativeFAbs(),
                Index = 0,
                Name = "iabs",
                Signature = new Compiler.FunctionSignature(new List<Compiler.FunctionSignature.ArgType> {
                    new Compiler.FunctionSignature.ArgType(new Compiler.IntType(),true,"x")
                }, new Compiler.IntType()),
            });
            nativeFunctions.Add(new Compiler.NativeFunctionType()
            {
                Function = new NativeFAbs(),
                Index = 0,
                Name = "sqrt",
                Signature = new Compiler.FunctionSignature(new List<Compiler.FunctionSignature.ArgType> {
                    new Compiler.FunctionSignature.ArgType(new Compiler.FloatType(),true,"x")
                }, new Compiler.FloatType()),
            });
            nativeFunctions.Add(new Compiler.NativeFunctionType()
            {
                Function = new NativeFAbs(),
                Index = 0,
                Name = "e",
                Signature = new Compiler.FunctionSignature(new List<Compiler.FunctionSignature.ArgType> {
                }, new Compiler.FloatType()),
            });
            nativeFunctions.Add(new Compiler.NativeFunctionType()
            {
                Function = new NativeFAbs(),
                Index = 0,
                Name = "pi",
                Signature = new Compiler.FunctionSignature(new List<Compiler.FunctionSignature.ArgType> {
                }, new Compiler.FloatType()),
            });
            nativeFunctions.Add(new Compiler.NativeFunctionType()
            {
                Function = new NativeFAbs(),
                Index = 0,
                Name = "exp",
                Signature = new Compiler.FunctionSignature(new List<Compiler.FunctionSignature.ArgType> {
                    new Compiler.FunctionSignature.ArgType(new Compiler.FloatType(),true,"x")
                }, new Compiler.FloatType()),
            });
            nativeFunctions.Add(new Compiler.NativeFunctionType()
            {
                Function = new NativeFAbs(),
                Index = 0,
                Name = "sin",
                Signature = new Compiler.FunctionSignature(new List<Compiler.FunctionSignature.ArgType> {
                    new Compiler.FunctionSignature.ArgType(new Compiler.FloatType(),true,"x")
                }, new Compiler.FloatType()),
            });
            nativeFunctions.Add(new Compiler.NativeFunctionType()
            {
                Function = new NativeFAbs(),
                Index = 0,
                Name = "cos",
                Signature = new Compiler.FunctionSignature(new List<Compiler.FunctionSignature.ArgType> {
                    new Compiler.FunctionSignature.ArgType(new Compiler.FloatType(),true,"x")
                }, new Compiler.FloatType()),
            });
            nativeFunctions.Add(new Compiler.NativeFunctionType()
            {
                Function = new NativeFAbs(),
                Index = 0,
                Name = "imin",
                Signature = new Compiler.FunctionSignature(new List<Compiler.FunctionSignature.ArgType> {
                    new Compiler.FunctionSignature.ArgType(new Compiler.IntType(),true,"a"),
                    new Compiler.FunctionSignature.ArgType(new Compiler.IntType(),true,"b")
                }, new Compiler.IntType()),
            });
            nativeFunctions.Add(new Compiler.NativeFunctionType()
            {
                Function = new NativeFAbs(),
                Index = 0,
                Name = "imax",
                Signature = new Compiler.FunctionSignature(new List<Compiler.FunctionSignature.ArgType> {
                    new Compiler.FunctionSignature.ArgType(new Compiler.IntType(),true,"a"),
                    new Compiler.FunctionSignature.ArgType(new Compiler.IntType(),true,"b")
                }, new Compiler.IntType()),
            });
            for (int i = 0; i < nativeFunctions.Count; i++)
            {
                nativeFunctions[i].Index = i;
            }
            #endregion

            {
                Compiler compiler = new Compiler();
                string text = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Docs\RecloserTest1.rcl"));
                AntlrInputStream inputStream = new AntlrInputStream(text);
                RecloserGrammarLexer programLexer = new RecloserGrammarLexer(inputStream);
                ErrorListener<int> lexerErrorListener = new ErrorListener<int>();
                programLexer.RemoveErrorListeners();
                programLexer.AddErrorListener(lexerErrorListener);
                CommonTokenStream commonTokenStream = new CommonTokenStream(programLexer);
                RecloserGrammarParser programParser = new RecloserGrammarParser(commonTokenStream);
                ErrorListener<IToken> parserErrorListener = new ErrorListener<IToken>();
                programParser.RemoveErrorListeners();
                programParser.AddErrorListener(parserErrorListener);

                RecloserGrammarParser.ProgramContext programContext = programParser.program();
                List<ErrorMessage> errorList = new List<ErrorMessage>();
                errorList.AddRange(lexerErrorListener.GetErrors());
                errorList.AddRange(parserErrorListener.GetErrors());
                if (errorList.Count > 0)
                {
                    foreach (var error in errorList)
                    {
                        Console.WriteLine(error.Error);
                    }
                    return;
                }
                Visitor visitor = new Visitor();
                Node root = visitor.VisitProgram(programContext);
                compiler.Init(new List<Compiler.NativeFunctionType>());
                Program program = compiler.Compile(root as ProgramNode, 4096);
                //TODO init program
                //program.RegisterFunction("main", 0, 0);
                if (!program.HasFunction("updateState", new Compiler.Type[] { new Compiler.FloatType(), new Compiler.FloatType() }, new Compiler.BoolType()))
                    throw new Exception("Function is not found");
                VirtualMachine vm = new VirtualMachine();
                vm.InitVM(4096 / 8, 20, program);
                Disassembler disassembler = new Disassembler();
                Stdout.WriteLine("\tTest virtual machine on RecloserTest1.rcl");
                string result = disassembler.Disassemble(program, text);
                Stdout.WriteLine(result);
                Stdout.Flush();
                Int programResult = vm.Execute("updateState", new IValue[] { new Float(10), new Float(5) }) as Int;

                Stdout.WriteLine("Result: " + programResult.BoolValue.ToString());
                Stdout.Flush();
            }
            {
                Compiler compiler = new Compiler();
                string text = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Docs\RecloserTest2.rcl"));
                AntlrInputStream inputStream = new AntlrInputStream(text);
                RecloserGrammarLexer programLexer = new RecloserGrammarLexer(inputStream);
                ErrorListener<int> lexerErrorListener = new ErrorListener<int>();
                programLexer.RemoveErrorListeners();
                programLexer.AddErrorListener(lexerErrorListener);
                CommonTokenStream commonTokenStream = new CommonTokenStream(programLexer);
                RecloserGrammarParser programParser = new RecloserGrammarParser(commonTokenStream);
                ErrorListener<IToken> parserErrorListener = new ErrorListener<IToken>();
                programParser.RemoveErrorListeners();
                programParser.AddErrorListener(parserErrorListener);

                RecloserGrammarParser.ProgramContext programContext = programParser.program();
                //Console.WriteLine(programContext.ToStringTree(programParser.RuleNames));
                List<ErrorMessage> errorList = new List<ErrorMessage>();
                errorList.AddRange(lexerErrorListener.GetErrors());
                errorList.AddRange(parserErrorListener.GetErrors());
                if (errorList.Count > 0)
                {
                    foreach (var error in errorList)
                    {
                        Console.WriteLine(error.Error);
                    }
                    return;
                }
                Visitor visitor = new Visitor();
                Node root = visitor.VisitProgram(programContext);
                compiler.Init(nativeFunctions);
                compiler.RegisterCustomType("recloserState", new List<KeyValuePair<string, Recloser.Compiler.NonRefType>>
                {
                    new KeyValuePair<string, Recloser.Compiler.NonRefType>( "ua", new Recloser.Compiler.FloatType()),
                    new KeyValuePair<string, Recloser.Compiler.NonRefType>( "ub", new Recloser.Compiler.FloatType()),
                    new KeyValuePair<string, Recloser.Compiler.NonRefType>( "uc", new Recloser.Compiler.FloatType()),
                    new KeyValuePair<string, Recloser.Compiler.NonRefType>( "ia", new Recloser.Compiler.FloatType()),
                    new KeyValuePair<string, Recloser.Compiler.NonRefType>( "ib", new Recloser.Compiler.FloatType()),
                    new KeyValuePair<string, Recloser.Compiler.NonRefType>( "ic", new Recloser.Compiler.FloatType()),
                    new KeyValuePair<string, Recloser.Compiler.NonRefType>( "currentState", new Recloser.Compiler.BoolType()),
                    new KeyValuePair<string, Recloser.Compiler.NonRefType>( "time", new Recloser.Compiler.FloatType())
                });
                Program program = compiler.Compile(root as ProgramNode, 4096);
                //TODO init program
                //program.RegisterFunction("main", 0, 0);
                if (!program.HasFunction("updateState", new Compiler.Type[] { new Compiler.FloatType(), new Compiler.FloatType() }, new Compiler.BoolType()))
                    throw new Exception("Function is not found");
                VirtualMachine vm = new VirtualMachine();
                vm.InitVM(4096 / 8, 20, program);
                Disassembler disassembler = new Disassembler();
                Stdout.WriteLine("\tTest virtual machine on RecloserTest2.rcl");
                string result = disassembler.Disassemble(program, text);
                Stdout.WriteLine(result);
                Stdout.Flush();
                vm.Execute("init", new IValue[] {});//void init()
                Int programResult = vm.Execute("updateState", new IValue[] {
                    new Float(10),
                    new Float(5),
                    new Float(),
                    new Float(),
                    new Float(),
                    new Float(),
                    new Float(),
                    new Float()
                }) as Int;//bool updateState(in recloserState)

                Stdout.WriteLine("Result: " + programResult.BoolValue.ToString());
            }
        }
    }
}
