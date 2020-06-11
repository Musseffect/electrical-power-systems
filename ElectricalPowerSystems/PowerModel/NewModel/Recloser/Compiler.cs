#define COMPILER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerModel.NewModel.Recloser
{
#if COMPILER
    public class Compiler
    {
        public class Variable
        {
            public int Index;
            public string Name;
            public Type Type;
            public void Save(Compiler compiler, bool isGlobal)
            {
                if (Type is RefType)
                {
                    (Type as RefType).EmitSave(compiler, Index);
                    return;
                }
                (Type as NonRefType).EmitSave(compiler, Index, isGlobal);
            }
            public void Load(Compiler compiler,bool isGlobal)
            {
                if (Type is RefType)
                {
                    (Type as RefType).EmitLoad(compiler, Index);
                    return;
                }
                (Type as NonRefType).EmitLoad(compiler, Index, isGlobal);
            }
        }
        class Scope
        {
            bool hasReturn;
            protected Dictionary<string, Variable> localVariablesDictionary;
            protected List<Variable> localVariablesList;
            protected int variableSlots;
            public Dictionary<string, Variable> VariableDictionary { get { return localVariablesDictionary; } }
            Scope parent;
            public Scope Parent { get { return parent; } }
            public bool HasReturn { get { return hasReturn; } }
            int stackPointer;
            public int StackPointer { get { return stackPointer; } }
            public Scope(Scope parent)
            {
                stackPointer = 0;
                localVariablesDictionary = new Dictionary<string, Variable>();
                localVariablesList = new List<Variable>();
                variableSlots = parent!=null?parent.variableSlots:0;
                hasReturn = false;
                this.parent = parent;
            }
            public void SetVariableSlots(int value)
            {
                this.variableSlots = value;
            }
            public int Extend(string varName, Type type)
            {
                Variable var = new Variable
                {
                    Name = varName,
                    Index = variableSlots,
                    Type = type
                };
                this.variableSlots += type.GetSlotSize();
                localVariablesDictionary.Add(varName, var);
                localVariablesList.Add(var);
                return variableSlots;
            }
            public void AddToStackPointer(int offset)
            {
                stackPointer += offset;
            }
            public void SetHasReturn()
            {
                this.hasReturn = true;
            }
        }
        class FunctionScope:Scope
        {
            FunctionType function;
            public FunctionType Function { get { return function; } }
            public FunctionScope(Scope parent, FunctionType function) : base(parent)
            {
                this.function = function;
                int c = 0;
                //variableSlots = -this.function.GetSlotSize();
                foreach (var arg in this.function.Signature.ArgumentTypes)
                {
                    Type type = arg.Type;
                    if (!arg.IsLocal)
                        type = new RefType() { BaseType = arg.Type };
                    Extend(arg.Name, type);
                }
            }
        }
        class SwitchCaseScope : Scope
        {
            List<int> breakJumpOffsets;
            public SwitchCaseScope(Scope parent):base(parent)
            {
                breakJumpOffsets = new List<int>();
            }
            public void AddBreakJumpOffset(int offset)
            {
                breakJumpOffsets.Add(offset);
            }
            public List<int> BreakJumpOffsets { get { return breakJumpOffsets; } }
        }
        class WhileLoopScope : Scope
        {
            List<int> breakJumpOffsets;
            List<int> continueJumpOffsets;
            int startOffset;
            public WhileLoopScope(Scope parent, int startOffset) : base(parent)
            {
                this.startOffset = startOffset;
                breakJumpOffsets = new List<int>();
                continueJumpOffsets = new List<int>();
            }
            public void AddBreakJumpOffset(int offset)
            {
                breakJumpOffsets.Add(offset);
            }
            public int StartOffset { get { return startOffset; } }
            public List<int> BreakJumpOffsets { get { return breakJumpOffsets; } }
            public List<int> ContinueJumpOffsets { get { return continueJumpOffsets; } }
        }
        public abstract class Type
        {
            public abstract int GetSlotSize();
            public abstract string GetName();
            public abstract void EmitPop(Compiler compiler);
            public abstract bool Equals(Type b);
        }
        public class RefType:Type
        {
            public NonRefType BaseType;
            public void EmitSave(Compiler compiler, int index)
            {
                BaseType.EmitSaveRef(compiler, index);
            }
            public override int GetSlotSize()
            {
                return 1;
            }
            public override string GetName()
            {
                return BaseType.GetName();
            }
            public void EmitLoad(Compiler compiler, int index)
            {
                BaseType.EmitLoadRef(compiler, index);
            }
            public override void EmitPop(Compiler compiler)
            {
                compiler.Emit(Instruction.POP);
            }
            public override bool Equals(Type b)
            {
                if (b is RefType)
                {
                    RefType bRef = b as RefType;
                    return this.BaseType.Equals(bRef.BaseType);
                }
                return false;
            }
        }
        public abstract class NonRefType : Type
        {
            public abstract List<IValue> DefaultValue();
            public abstract void EmitSave(Compiler compiler, int index, bool isGlobal);
            public abstract void EmitSaveRef(Compiler compiler, int refIndex, int offset);
            public abstract void EmitSaveRef(Compiler compiler, int refIndex);
            public abstract void EmitInit(Compiler compiler);
            public abstract void EmitLoad(Compiler compiler, int index, bool isGlobal);
            public abstract void EmitLoadRef(Compiler compiler, int refIndex, int offset);
            public abstract void EmitLoadRef(Compiler compiler, int refIndex);
        }
        public class ArrayType : NonRefType
        {
            NonRefType type;
            int size;
            public NonRefType Type { get { return type; } }
            public int Size { get { return size; } }
            public override void EmitInit(Compiler compiler)
            {
                for (int i = 0; i < Size; i++)
                {
                    Type.EmitInit(compiler);
                }
            }
            public override void EmitSave(Compiler compiler, int index, bool isGlobal)
            {
                throw new NotImplementedException();
                /*
                 compiler.Emit(Instruction.IPUSH);
                 compiler.Emit(new Int(index));
                 compiler.Emit(Instruction.ARRSTORE);
                 compiler.Emit(new Int(GetSlotSize()));
                 */
            }
            public override void EmitSaveRef(Compiler compiler, int refIndex)
            {
                throw new NotImplementedException();
                /*
                 compiler.Emit(Instruction.ILOAD);
                 compiler.Emit(new Short(refIndex));
                 compiler.Emit(Instruction.ARRSTORE);
                 compiler.Emit(new Int(GetSlotSize()));
                 */
            }
            public override void EmitSaveRef(Compiler compiler, int refIndex, int offset)
            {
                throw new NotImplementedException();
                /*
                 compiler.Emit(Instruction.ILOAD);
                 compiler.Emit(new Short(refIndex));
                 compiler.Emit(Instruction.IPUSH);
                 compmiler.Emit(new Int(offset))
                 compiler.Emit(Instruction.IADD);
                 compiler.Emit(Instruction.ARRSTORE);
                 compiler.Emit(new Int(GetSlotSize()));
                 */
            }
            public ArrayType(NonRefType type, int size)
            {
                this.type = type;
                this.size = size;
            }
            public override int GetSlotSize()
            {
                return type.GetSlotSize() * size;
            }
            public override string GetName()
            {
                return $"{type.GetName()}[{size}]";
            }
            public override void EmitLoadRef(Compiler compiler, int refIndex, int offset)
            {
                for (int i = 0; i < size; i++)
                {
                    type.EmitLoadRef(compiler, refIndex, offset + i * type.GetSlotSize());
                }
            }
            public override void EmitLoadRef(Compiler compiler, int refIndex)
            {
                for (int i = 0; i < size; i++)
                {
                    type.EmitLoadRef(compiler, refIndex, i * type.GetSlotSize());
                }
            }
            public override void EmitLoad(Compiler compiler, int index, bool isGlobal)
            {
                if (isGlobal)
                {
                    for (int i = 0; i < size; i++)
                    {
                        type.EmitLoad(compiler,index+i*type.GetSlotSize(), true);
                    }
                }else
                {
                    for (int i = 0; i < size; i++)
                    {
                        type.EmitLoad(compiler, index + i * type.GetSlotSize(), false);
                    }
                }
            }
            public override void EmitPop(Compiler compiler)
            {
                compiler.Emit(Instruction.ARR_POP);
                compiler.Emit(new Short((short)this.GetSlotSize()));
            }
            public override bool Equals(Type b)
            {
                if (b is ArrayType)
                {
                    ArrayType bArray = b as ArrayType;
                    return this.type.Equals(bArray.type)&&this.size == bArray.size;
                }
                return false;
            }
            public override List<IValue> DefaultValue()
            {
                List<IValue> result = new List<IValue>();
                List<IValue> elementDefaultValues = type.DefaultValue();
                for (int i=0;i<size;i++)
                {
                    result.AddRange(elementDefaultValues);
                }
                return result;
            }
        }
        public class StructType : NonRefType
        {
            class Field
            {
                NonRefType type;
                int offset;
                public Field(NonRefType type, int offset)
                {
                    this.type = type;
                    this.offset = offset;
                }
                public NonRefType GetFieldType()
                {
                    return type;
                }
                public int GetOffset()
                {
                    return offset;
                }
            }
            string name;
            Dictionary<string, Field> fields;
            List<Field> fieldList;
            int size;
            public StructType(string name)
            {
                size = 0;
                this.name = name;
                this.fields = new Dictionary<string, Field>();
                this.fieldList = new List<Field>();
            }
            public override void EmitInit(Compiler compiler)
            {
                foreach (var field in fieldList)
                {
                    field.GetFieldType().EmitInit(compiler);
                }
            }
            public void AddField(string fieldName, NonRefType type)
            {
                int offset = size;
                size +=type.GetSlotSize();
                Field field = new Field(type, offset);
                this.fields.Add(fieldName,field);
                fieldList.Add(field);
            }
            public override string GetName()
            {
                return name;
            }
            public override int GetSlotSize()
            {
                return size;
            }
            public NonRefType GetFieldType(string field)
            {
                return fields[field].GetFieldType();
            }
            public int GetOffset(string field)
            {
                return fields[field].GetOffset();
            }
            public int GetStructSize()
            {
                return size;
            }
            public override void EmitPop(Compiler compiler)
            {
                compiler.Emit(Instruction.ARR_POP);
                compiler.Emit(new Short((short)this.GetSlotSize()));
            }
            public override void EmitSave(Compiler compiler, int index, bool isGlobal)
            {
                throw new NotImplementedException();
                /*
                 compiler.Emit(Instruction.IPUSH);
                 compiler.Emit(new Int(index));
                 compiler.Emit(Instruction.ARR_STORE);
                 compiler.Emit(new Int(GetSlotSize()));
                 */
            }
            public override void EmitSaveRef(Compiler compiler, int refIndex)
            {
                throw new NotImplementedException();
                /*
                 compiler.Emit(Instruction.ILOAD);
                 compiler.Emit(new Short(refIndex));
                 compiler.Emit(Instruction.ARR_STORE);
                 compiler.Emit(new Int(GetSlotSize()));
                 */
            }
            public override void EmitSaveRef(Compiler compiler, int refIndex, int offset)
            {
                throw new NotImplementedException();
                /*
                 compiler.Emit(Instruction.ILOAD);
                 compiler.Emit(new Short(refIndex));
                 compiler.Emit(Instruction.IPUSH);
                 compmiler.Emit(new Int(offset))
                 compiler.Emit(Instruction.IADD);
                 compiler.Emit(Instruction.ARR_STORE);
                 compiler.Emit(new Int(GetSlotSize()));
                 */
            }
            public override void EmitLoad(Compiler compiler, int index, bool isGlobal)
            {
                throw new NotImplementedException();
            }
            public override void EmitLoadRef(Compiler compiler, int refIndex, int offset)
            {
                throw new NotImplementedException();
            }
            public override void EmitLoadRef(Compiler compiler, int refIndex)
            {
                throw new NotImplementedException();
            }
            public override bool Equals(Type b)
            {
                if (b is StructType)
                {
                    StructType bStruct = b as StructType;
                    return this.name == bStruct.name;
                }
                return false;
            }
            public override List<IValue> DefaultValue()
            {
                List<IValue> result = new List<IValue>();
                foreach (var field in fields)
                {
                    result.AddRange(field.Value.GetFieldType().DefaultValue());
                }
                return result;
            }
        }
        public abstract class BasicType: NonRefType
        {
            public enum BType
            {
                Int,
                Float,
                Bool,
                Void
            }
            BType type;
            public BType Type { get { return type; } }
            public BasicType(BType type)
            {
                this.type = type;
            }
            public abstract int GetByteSize();

        }
        public class FloatType : BasicType
        {
            public FloatType() : base(BType.Float)
            { }
            public override void EmitInit(Compiler compiler)
            {
                compiler.Emit(Instruction.FPUSH);
                compiler.Emit(new Float(0));
            }
            public override int GetSlotSize()
            {
                return 1;
            }
            public override int GetByteSize()
            {
                return Float.Sizeof();
            }
            public override string GetName()
            {
                return "float";
            }
            public override bool Equals(Type b)
            {
                return b is FloatType;
            }
            public override void EmitPop(Compiler compiler)
            {
                compiler.Emit(Instruction.POP);
            }
            public override void EmitLoadRef(Compiler compiler, int refIndex, int offset)
            {
                compiler.Emit(Instruction.ILOAD);
                compiler.Emit(new Short((short)refIndex));
                compiler.Emit(Instruction.IPUSH);
                compiler.Emit(new Int(offset));
                compiler.Emit(Instruction.IADD);
                compiler.Emit(Instruction.FRLOAD);
            }
            public override void EmitLoadRef(Compiler compiler, int refIndex)
            {
                compiler.Emit(Instruction.ILOAD);
                compiler.Emit(new Short((short)refIndex));
                compiler.Emit(Instruction.FRLOAD);
            }
            public override void EmitLoad(Compiler compiler, int index, bool isGlobal)
            {
                if (isGlobal)
                {
                    compiler.Emit(Instruction.IPUSH);
                    compiler.Emit(new Int(index));
                    compiler.Emit(Instruction.FRLOAD);
                }
                else
                {
                    compiler.Emit(Instruction.FLOAD);
                    compiler.Emit(new Short((short)index));
                }
            }
            public override void EmitSave(Compiler compiler, int index, bool isGlobal)
            {
                if (isGlobal)
                {
                    compiler.Emit(Instruction.IPUSH);
                    compiler.Emit(new Int(index));
                    compiler.Emit(Instruction.FRSTORE);
                }
                else
                {
                    compiler.Emit(Instruction.FSTORE);
                    compiler.Emit(new Short((short)index));
                }
            }
            public override void EmitSaveRef(Compiler compiler, int refIndex, int offset)
            {
                compiler.Emit(Instruction.ILOAD);
                compiler.Emit(new Short((short)refIndex));
                compiler.Emit(Instruction.IPUSH);
                compiler.Emit(new Int(offset));
                compiler.Emit(Instruction.IADD);
                compiler.Emit(Instruction.FRSTORE);
            }
            public override void EmitSaveRef(Compiler compiler, int refIndex)
            {
                compiler.Emit(Instruction.ILOAD);
                compiler.Emit(new Short((short)refIndex));
                compiler.Emit(Instruction.FRSTORE);
            }
            public override List<IValue> DefaultValue()
            {
                return new List<IValue> { new Float(0) };
            }

        }
        public class IntType : BasicType
        {
            public IntType() : base(BType.Int)
            { }
            public override void EmitInit(Compiler compiler)
            {
                compiler.Emit(Instruction.IPUSH);
                compiler.Emit(new Int(0));
            }
            public override int GetSlotSize()
            {
                return 1;
            }
            public override bool Equals(Type b)
            {
                return b is IntType;
            }
            public override int GetByteSize()
            {
                return Int.Sizeof();
            }
            public override string GetName()
            {
                return "int";
            }
            public override void EmitLoadRef(Compiler compiler, int refIndex, int offset)
            {
                compiler.Emit(Instruction.ILOAD);
                compiler.Emit(new Short((short)refIndex));
                compiler.Emit(Instruction.IPUSH);
                compiler.Emit(new Int(offset));
                compiler.Emit(Instruction.IADD);
                compiler.Emit(Instruction.IRLOAD);
            }
            public override void EmitLoadRef(Compiler compiler, int refIndex)
            {
                compiler.Emit(Instruction.ILOAD);
                compiler.Emit(new Short((short)refIndex));
                compiler.Emit(Instruction.IRLOAD);
            }
            public override void EmitLoad(Compiler compiler, int index, bool isGlobal)
            {
                if (isGlobal)
                {
                    compiler.Emit(Instruction.IPUSH);
                    compiler.Emit(new Int(index));
                    compiler.Emit(Instruction.IRLOAD);
                }else
                {
                    compiler.Emit(Instruction.ILOAD);
                    compiler.Emit(new Short((short)index));
                }
            }
            public override void EmitPop(Compiler compiler)
            {
                compiler.Emit(Instruction.POP);
            }
            public override void EmitSave(Compiler compiler, int index, bool isGlobal)
            {
                if (isGlobal)
                {
                    compiler.Emit(Instruction.IPUSH);
                    compiler.Emit(new Int(index));
                    compiler.Emit(Instruction.IRSTORE);
                }
                else
                {
                    compiler.Emit(Instruction.ISTORE);
                    compiler.Emit(new Short((short)index));
                }
            }
            public override void EmitSaveRef(Compiler compiler, int refIndex, int offset)
            {
                compiler.Emit(Instruction.ILOAD);
                compiler.Emit(new Short((short)refIndex));
                compiler.Emit(Instruction.IPUSH);
                compiler.Emit(new Int(offset));
                compiler.Emit(Instruction.IADD);
                compiler.Emit(Instruction.IRSTORE);
            }
            public override void EmitSaveRef(Compiler compiler, int refIndex)
            {
                compiler.Emit(Instruction.ILOAD);
                compiler.Emit(new Short((short)refIndex));
                compiler.Emit(Instruction.IRSTORE);
            }
            public override List<IValue> DefaultValue()
            {
                return new List<IValue> { new Int(0) };
            }
        }
        public class BoolType : BasicType
        {
            public BoolType() : base(BType.Bool)
            { }
            public override void EmitInit(Compiler compiler)
            {
                compiler.Emit(Instruction.IPUSH);
                compiler.Emit(new Int(0));
            }
            public override int GetSlotSize()
            {
                return 1;
            }
            public override bool Equals(Type b)
            {
                return b is BoolType;
            }
            public override int GetByteSize()
            {
                return Bool.Sizeof();
            }
            public override string GetName()
            {
                return "bool";
            }
            public override void EmitLoadRef(Compiler compiler, int refIndex, int offset)
            {
                compiler.Emit(Instruction.ILOAD);
                compiler.Emit(new Short((short)refIndex));
                compiler.Emit(Instruction.IPUSH);
                compiler.Emit(new Int(offset));
                compiler.Emit(Instruction.IADD);
                compiler.Emit(Instruction.IRLOAD);
            }
            public override void EmitLoadRef(Compiler compiler, int refIndex)
            {
                compiler.Emit(Instruction.ILOAD);
                compiler.Emit(new Short((short)refIndex));
                compiler.Emit(Instruction.IRLOAD);
            }
            public override void EmitLoad(Compiler compiler, int index, bool isGlobal)
            {
                if (isGlobal)
                {
                    compiler.Emit(Instruction.IPUSH);
                    compiler.Emit(new Int(index));
                    compiler.Emit(Instruction.IRLOAD);
                }
                else
                {
                    compiler.Emit(Instruction.ILOAD);
                    compiler.Emit(new Short((short)index));
                }
            }
            public override void EmitSave(Compiler compiler, int index, bool isGlobal)
            {
                if (isGlobal)
                {
                    compiler.Emit(Instruction.IPUSH);
                    compiler.Emit(new Int(index));
                    compiler.Emit(Instruction.IRSTORE);
                }
                else
                {
                    compiler.Emit(Instruction.ISTORE);
                    compiler.Emit(new Short((short)index));
                }
            }
            public override void EmitSaveRef(Compiler compiler, int refIndex, int offset)
            {
                compiler.Emit(Instruction.ILOAD);
                compiler.Emit(new Short((short)refIndex));
                compiler.Emit(Instruction.IPUSH);
                compiler.Emit(new Int(offset));
                compiler.Emit(Instruction.IADD);
                compiler.Emit(Instruction.IRSTORE);
            }

            public override void EmitSaveRef(Compiler compiler, int refIndex)
            {
                compiler.Emit(Instruction.ILOAD);
                compiler.Emit(new Short((short)refIndex));
                compiler.Emit(Instruction.IRSTORE);
            }
            public override void EmitPop(Compiler compiler)
            {
                compiler.Emit(Instruction.POP);
            }
            public override List<IValue> DefaultValue()
            {
                return new List<IValue> { new Int(0)};
            }
        }
        public class VoidType : BasicType
        {
            public VoidType() : base(BType.Void)
            { }
            public override int GetSlotSize()
            {
                return 0;
            }
            public override int GetByteSize()
            {
                return 0;
            }
            public override bool Equals(Type b)
            {
                return b is VoidType;
            }
            public override string GetName()
            {
                return "void";
            }
            public override void EmitPop(Compiler compiler)
            {
                throw new Exception("Нельзя использовать тип void");
            }
            public override void EmitLoad(Compiler compiler, int index, bool isGlobal)
            {
                throw new Exception("Нельзя использовать тип void");
            }
            public override void EmitInit(Compiler compiler)
            {
                throw new Exception("Нельзя использовать тип void");
            }
            public override void EmitSave(Compiler compiler, int index, bool isGlobal)
            {
                throw new Exception("Нельзя использовать тип void");
            }

            public override void EmitSaveRef(Compiler compiler, int refIndex, int offset)
            {
                throw new Exception("Нельзя использовать тип void");
            }

            public override void EmitSaveRef(Compiler compiler, int refIndex)
            {
                throw new Exception("Нельзя использовать тип void");
            }

            public override void EmitLoadRef(Compiler compiler, int refIndex, int offset)
            {
                throw new Exception("Нельзя использовать тип void");
            }

            public override void EmitLoadRef(Compiler compiler, int refIndex)
            {
                throw new Exception("Нельзя использовать тип void");
            }

            public override List<IValue> DefaultValue()
            {
                throw new Exception("Нельзя использовать тип void");
            }
        }
        public class FunctionSignature
        {
            public class ArgType
            {
                public NonRefType Type;
                public bool IsLocal;
                public string Name;
                public ArgType(NonRefType type, bool isLocal, string name)
                {
                    this.Type = type;
                    this.IsLocal = isLocal;
                    this.Name = name;
                }
            };
            public BasicType ReturnType { get; protected set; }
            public List<ArgType> ArgumentTypes { get; protected set; }
            public Dictionary<string, ArgType> ArgumentsDictionary { get; set; }//null if not initialized
            public FunctionSignature(List<ArgType> arguments, BasicType returnType)
            {
                ArgumentsDictionary = new Dictionary<string, ArgType>();
                this.ArgumentTypes = arguments;
                foreach (var argument in arguments)
                {
                    ArgumentsDictionary.Add(argument.Name,argument);
                }
                this.ReturnType = returnType;
            }
            public int GetSlotSize()
            {
                int slots = 0;
                foreach (var arg in ArgumentTypes)
                {
                    slots += arg.Type.GetSlotSize();
                }
                return slots;
            }
            public static bool Compare(FunctionSignature a, FunctionSignature b)
            {
                if (a.ArgumentTypes.Count != b.ArgumentTypes.Count)
                    return false;
                if (a.ReturnType != b.ReturnType)
                    return false;
                for (int i = 0; i < a.ArgumentTypes.Count; i++)
                {
                    if (a.ArgumentTypes[i].Type != b.ArgumentTypes[i].Type)//TODO
                        return false;
                    if (a.ArgumentTypes[i].IsLocal != b.ArgumentTypes[i].IsLocal)
                        return false;
                }
                return true;
            }

        }
        public class NativeFunctionType
        {
            public INativeFunction Function;
            public FunctionSignature Signature;
            public int Index;
            public string Name;
        }
        public class FunctionType
        {
            public FunctionSignature Signature;
            public string Name;
            public int SrcPosBegin;
            public int SrcPosEnd;
            public int Index;
            public int Offset;//address in program memory
        }
        ByteArray bytecode;
        Dictionary<string, StructType> structs;
        Dictionary<string, BasicType> basicTypes;
        Dictionary<string, FunctionType> functionsDictionary;
        Dictionary<string, NativeFunctionType> nativeFunctionsDictionary;
        List<FunctionType> functionsList;
        List<INativeFunction> nativeFunctions;
        Dictionary<string, Variable> globalsDictionary;
        List<Variable> globalsList;
        //List<IValue> globalValues;
        List<ErrorMessage> errors;
        List<Program.Line> lines;
        int currentLine;
        int programSizeLimit;
        //Dictionary<string, NativeFunction> nativeFunctions;
        public void Init(List<NativeFunctionType> nativeFunctionsList)
        {
            nativeFunctionsDictionary = new Dictionary<string, NativeFunctionType>();
            nativeFunctions = new List<INativeFunction>();
            foreach (var func in nativeFunctionsList)
            {
                nativeFunctionsDictionary.Add(func.Name, func);
                nativeFunctions.Add(func.Function);
            }
            functionsDictionary = new Dictionary<string, FunctionType>();
            functionsList = new List<FunctionType>();
            structs = new Dictionary<string, StructType>();
            basicTypes = new Dictionary<string, BasicType> {
                { "int",new IntType()},
                { "bool",new BoolType()},
                { "float",new FloatType()},
                { "void",new VoidType()}
            };
            globalsDictionary = new Dictionary<string, Variable>();
            globalsList = new List<Variable>();
        }
        public Program Compile(ProgramNode program, int programSizeLimit)
        {
            lines = new List<Program.Line>();
            currentLine = -1;
            errors = new List<ErrorMessage>();
            bytecode = new ByteArray();
            this.programSizeLimit = programSizeLimit;
            foreach (var statement in program.Statements)
            {
                CompileStatement(null,statement);
            }
            if (bytecode.Size > programSizeLimit)
                errors.Add(new ErrorMessage($"Размер программы {bytecode.Size} байт больше максимального {programSizeLimit} байт"));
            if (errors.Count > 0)
                throw new Equations.CompilerException(errors);
            return new Program(bytecode, globalsList, functionsList, functionsDictionary, nativeFunctions, lines.ToArray());
        }
        //public void RegisternativeFunctions(List<>);
        public StructType RegisterCustomType(string structType, List<KeyValuePair<string, NonRefType>> fields)
        {
            if (this.structs.ContainsKey(structType))
                throw new Exception($"Попытка переопределить существующий тип \"{structType}\"");
            if (basicTypes.ContainsKey(structType))
                throw new Exception($"Попытка переопределить базовый тип \"{structType}\"");
            StructType @struct = new StructType(structType);
            foreach (var field in fields)
            {
                if (field.Value is VoidType)
                    throw new Exception($"Некорректный тип поля");
                @struct.AddField(field.Key, field.Value);
            }
            structs.Add(structType, @struct);
            return @struct;
        }
        private int Extend(string name,Type type)//extend globals
        {
            int slot = globalsList.Count;
            Variable var = new Variable
            {
                Name = name,
                Index = slot,
                Type = type
            };
            globalsDictionary.Add(name, var);
            globalsList.Add(var);
            return slot;
        }
        private NonRefType ResolveType(TypeNode typeNode)
        {
            if (basicTypes.ContainsKey(typeNode.Typename))
            {
                if (typeNode.Size != null)
                {
                    if (typeNode.Size.Value < 0)
                        throw new Exception($"Некорректный размер массива: {typeNode.Size.Value}");
                    return new ArrayType(basicTypes[typeNode.Typename], typeNode.Size.Value);
                }
                return basicTypes[typeNode.Typename];
            }
            if (structs.ContainsKey(typeNode.Typename))
            {
                if (typeNode.Size != null)
                {
                    if (typeNode.Size.Value < 0)
                        throw new Exception($"Некорректный размер массива: {typeNode.Size.Value}");
                    return new ArrayType(structs[typeNode.Typename], typeNode.Size.Value);
                }
                return structs[typeNode.Typename];
            }
            throw new Exception($"Неизвестный тип \"{typeNode.Typename}\"");
        }
        private List<ExpressionNode> ResolveChain(ExpressionNode node)
        {
            switch (node.ExpressionType)
            {
                case ExpressionNode.ExpType.ArrayAccess:
                    {
                        ArrayAccessNode _node = node as ArrayAccessNode;
                        List<ExpressionNode> result = ResolveChain(_node.Array);
                        result.Add(_node);
                        return result;
                    }
                case ExpressionNode.ExpType.FieldExpression:
                    {
                        FieldAccessNode _node = node as FieldAccessNode;
                        List<ExpressionNode> result = ResolveChain(_node.Parent);
                        result.Add(_node);
                        return result;
                    }
                case ExpressionNode.ExpType.IdentifierExpression:
                    {
                        return new List<ExpressionNode> { node };
                    }
            }
            throw new Exception("Ожидалось: оператор [], оператор ., идентификатор");
        }
        private NonRefType ResolveType(string typename)
        {
            if (basicTypes.ContainsKey(typename))
                return basicTypes[typename];
            if (structs.ContainsKey(typename))
            {
                return structs[typename];
            }
            throw new Exception($"Неизвестный тип \"{typename}\"");
        }
        private void CompileStatement(Scope scope, StatementNode statement)
        {
            if (programSizeLimit < bytecode.Size)
                throw new Exception($"Размер программы превысил ограничение в {programSizeLimit}");
#if DEBUG || TEST
            Console.WriteLine(statement.StatementType.ToString());
#endif
            currentLine = statement.Line;
            switch (statement.StatementType)
            {
                case StatementNode.Type.VariableDeclarationStatement://TODO
                    {
                        VariableDeclarationNode declarationNode = statement as VariableDeclarationNode;
                        NonRefType type = ResolveType(declarationNode.VariableType);
                        if (type is VoidType)
                            throw new Exception($"Некорректный тип переменной");
                        if (scope == null)//global variable
                        {
                            foreach (var variable in declarationNode.Variables)
                            {
                                //check if name is already exist
                                if (globalsDictionary.ContainsKey(variable.Name))
                                {
                                    throw new Exception($"Переопределение глобальной переменной \"{variable.Name}\"");
                                }
                                //check if initializer is constexpr
                                //eval initializer
                                //create global with default value
                                this.Extend(variable.Name, type);
                            }
                        }
                        else
                        {
                            foreach (var variable in declarationNode.Variables)
                            {
                                //local variable
                                //check if name is already exist
                                if (scope.VariableDictionary.ContainsKey(variable.Name))
                                {
                                    throw new Exception($"Переопределение локальной переменной \"{variable.Name}\"");
                                }
                                //compile
                                //init variables on stack
                                /*if (type is BasicType)
                                {
                                    BasicType basicType = type as BasicType;
                                    switch (basicType.Type)
                                    {
                                        case BasicType.BType.Float:
                                            Emit(Instruction.FPUSH);
                                            Emit(new Float());
                                            break;
                                        case BasicType.BType.Int:
                                        case BasicType.BType.Bool:
                                            Emit(Instruction.IPUSH);
                                            Emit(new Int());
                                            break;
                                        case BasicType.BType.Void:
                                        default:
                                            throw new Exception("Некорректный тип данных");
                                    }
                                }
                                else if (type is ArrayType)
                                {
                                    ArrayType arrayType = type as ArrayType;
                                }
                                else if (type is StructType)
                                {
                                    StructType structType = type as StructType;

                                }
                                else if (type is RefType)
                                {
                                    throw new Exception("Критическая ошибка");
                                }*/
                                if (variable.Initializer != null)
                                {
                                    CheckType(scope, variable.Initializer, type);
                                    CompileExpression(scope, variable.Initializer);
                                }
                                else
                                {
                                    type.EmitInit(this);
                                }
                                //check initializer
                                //if initializer
                                //compile initializer
                                scope.Extend(variable.Name, type);
                                scope.AddToStackPointer(type.GetSlotSize());
                            }
                        }
                        break;
                    }
                case StatementNode.Type.StructDefinition://Done
                    {
                        StructDefinitionNode node = statement as StructDefinitionNode;
                        if (scope != null)
                            throw new Exception($"Нельзя определять структуру \"{node.Name}\" в локальной области видимости");
                        if (structs.ContainsKey(node.Name))
                            throw new Exception($"Переопределение структуры \"{node.Name}\"");
                        if(basicTypes.ContainsKey(node.Name))
                            throw new Exception($"Переопределение типа \"{node.Name}\"");
                        StructType type = new StructType(node.Name);
                        foreach (var field in node.Fields)
                        {
                            NonRefType fieldType = ResolveType(field.FieldType);
                            /*if (fieldType == ArrayType)
                            {
                                //TODO разобраться с массивами в структуре
                                throw new Exception($"Нельзя использовать массив в структуре");
                            }
                            if (!(fieldType is BasicType))
                                throw new Exception($"Нельзя использовать структуры в структурах");*/
                            if (fieldType is VoidType)
                                throw new Exception($"Некорректный тип поля");
                            type.AddField(field.Name, fieldType);
                        }
                        structs.Add(node.Name, type);
                        return;
                    }
                case StatementNode.Type.FunctionDefinition:
                    {//TODO
                        FunctionDefinitionNode functionNode = statement as FunctionDefinitionNode;
                        if (scope != null)//global variable
                        {
                            throw new Exception("Определение функции не в глобальной области видимости");
                        }
                        FunctionType function = new FunctionType();
                        function.Name = functionNode.Name;
                        //TODO: argument types and return type
                        List<FunctionSignature.ArgType> argTypes = new List<FunctionSignature.ArgType>();
                        Dictionary<string, FunctionSignature.ArgType> argDictionary = new Dictionary<string, FunctionSignature.ArgType>();
                        foreach (var arg in functionNode.Signature)
                        {
                            FunctionSignature.ArgType type = new FunctionSignature.ArgType(
                                ResolveType(arg.ArgumentType),
                                (arg.Way == SignatureArgumentNode.WayType.In),
                                arg.Name);
                            argTypes.Add(type);
                            if (argDictionary.ContainsKey(arg.Name))
                                throw new Exception($"Переопределение аргумента \"{arg.Name}\"");
                            argDictionary.Add(arg.Name, type);
                        }
                        function.SrcPosBegin = functionNode.Start;
                        function.SrcPosEnd = functionNode.Stop;
                        Type returnType = ResolveType(functionNode.ReturnType);
                        if (!(returnType is BasicType))
                            throw new Exception("Тип возвращаемого значения функции должен быть базовым");
                        function.Signature = new FunctionSignature(argTypes,returnType as BasicType);
                        if (nativeFunctionsDictionary.ContainsKey(functionNode.Name))
                        {
                            throw new Exception($"Переопределение нативной функции \"{functionNode.Name}\"");
                        }
                        else if (functionsDictionary.ContainsKey(functionNode.Name))
                        {
                            if (FunctionSignature.Compare(function.Signature, functionsDictionary[functionNode.Name].Signature))
                            {
                                if (functionsDictionary[functionNode.Name].Signature.ArgumentsDictionary != null)
                                    throw new Exception($"Переопределение функции \"{functionNode.Name}\"");
                                functionsDictionary[functionNode.Name] = function;
                                functionsList[function.Index] = function;
                            }
                            else
                            {
                                throw new Exception($"Множественные объявления функции \"{functionNode.Name}\" с различными сигнатурами");
                            }
                        }
                        else
                        {
                            function.Index = functionsDictionary.Count;
                            functionsDictionary.Add(functionNode.Name, function);
                            functionsList.Add(function);
                        }

                        //add function offset
                        function.Offset = this.bytecode.Size;

                        FunctionScope _scope = new FunctionScope(scope, function);
                        if (returnType.Equals(new VoidType()))
                        {
                            _scope.SetHasReturn();
                        }
                        foreach (var functionStatement in functionNode.Body)
                        {
                            CompileStatement(_scope,functionStatement);
                        }
                        if (_scope.HasReturn == false)
                            throw new Exception($"Отсутствует оператор return в определении функции \"{functionNode.Name}\"");
                        return;
                    }
                case StatementNode.Type.FunctionDeclaration:
                    {
                        FunctionDeclarationNode functionNode = statement as FunctionDeclarationNode;
                        if (scope != null)
                        {
                            throw new Exception("Объявление функции не в глобальной области видимости");
                        }
                        //Create decalaration
                        FunctionType function = new FunctionType();
                        function.Name = functionNode.Name;
                        //TODO: argument types and return type
                        List<FunctionSignature.ArgType> argTypes = new List<FunctionSignature.ArgType>();
                        foreach (var arg in functionNode.Signature)
                        {
                            argTypes.Add(new FunctionSignature.ArgType(ResolveType(arg.ArgumentType), (arg.Way == SignatureArgumentNode.WayType.In),arg.Name));
                        }
                        Type returnType = ResolveType(functionNode.ReturnType);
                        if (!(returnType is BasicType))
                            throw new Exception("Тип возвращаемого значения функции должен быть базовым");

                        function.Signature = new FunctionSignature(argTypes, returnType as BasicType);
                        function.Signature.ArgumentsDictionary = null;//declaration
                        if (nativeFunctionsDictionary.ContainsKey(functionNode.Name))
                        {
                            throw new Exception($"Переопределение нативной функции \"{functionNode.Name}\"");
                        }
                        else if (functionsDictionary.ContainsKey(functionNode.Name))
                        {
                            //Compare signatures

                            if (FunctionSignature.Compare(function.Signature, functionsDictionary[functionNode.Name].Signature))
                                return;
                            throw new Exception($"Множественные объявления функции \"{functionNode.Name}\" с различными сигнатурами");
                        } 
                        function.Index = functionsDictionary.Count;
                        functionsDictionary.Add(functionNode.Name, function);
                        functionsList.Add(function);
                        return;
                    }
                case StatementNode.Type.ExpressionStatement:
                    {//TODO
                        if (scope == null)
                            throw new Exception("Выражения не должны использоваться в глобальной области видимости");
                        int stackPointer = scope.StackPointer;
                        Type type = InferType(scope, statement as ExpressionNode);
                        CompileExpression(scope, statement as ExpressionNode);
                        int stackDifference = scope.StackPointer - stackPointer;
#if DEBUG || TEST
                        if (stackDifference != type.GetSlotSize())
                        {
                            throw new Exception("Критическая ошибка");
                        }
#else
#endif
                        //each expression leaves value on stack
                        type.EmitPop(this);
                        return;
                    }
                case StatementNode.Type.ReturnStatement://Done
                    {
                        if (scope == null)
                            throw new Exception("Инструкция return не должна использоваться в глобальной области видимости");
                        ReturnNode node = statement as ReturnNode;
                        FunctionScope functionScope = null;
                        Scope _scope = scope;
                        while (_scope != null)
                        {
                            if (_scope is FunctionScope)
                            {
                                functionScope = _scope as FunctionScope;
                                break;
                            }
                            _scope = _scope.Parent;
                        }
                        if(functionScope == null)
                            throw new Exception("Инструкция return используется не внутри функции");

                        if (functionScope == scope)
                            functionScope.SetHasReturn();
                        if (node.Expression == null)
                        {
                            if (functionScope.Function.Signature.ReturnType is VoidType)
                            {
                                Emit(Instruction.RET);
                                return;
                            }
                            throw new Exception("Тип возвращаемого значения не совпадает с типов в объявлении функции");
                        }
                        //get type of expression
                        Type expType = InferType(scope, node.Expression);
                        //if type is not equal to type of function throw exception
                        if (!expType.Equals(functionScope.Function.Signature.ReturnType))
                        {
                            throw new Exception("Тип возвращаемого значения не совпадает с типом в объявлении функции");
                        }
                        CompileExpression(scope, node.Expression);
                        switch (functionScope.Function.Signature.ReturnType.Type)
                        {
                            case BasicType.BType.Float:
                                Emit(Instruction.FRET);
                                return;
                            case BasicType.BType.Int:
                            case BasicType.BType.Bool:
                                Emit(Instruction.IRET);
                                return;
                        }
                        return;
                    }
                case StatementNode.Type.BreakStatement://Done
                    {
                        if (scope == null)
                            throw new Exception("Инструкция break не должна использоваться в глобальной области видимости");
                        Scope _scope = scope;
                        while (_scope != null)
                        {
                            if (_scope is WhileLoopScope)
                            {
                                WhileLoopScope whileLoopScope = _scope as WhileLoopScope;
                                bytecode.AddInstruction(Instruction.JUMP);
                                whileLoopScope.AddBreakJumpOffset(bytecode.Size);
                                bytecode.AddConstant(new Short(-1));
                                return;
                            }
                            else if (_scope is SwitchCaseScope)
                            {
                                SwitchCaseScope switchCaseScope = _scope as SwitchCaseScope;
                                bytecode.AddInstruction(Instruction.JUMP);
                                switchCaseScope.AddBreakJumpOffset(bytecode.Size);
                                bytecode.AddConstant(new Short(-1));
                                return;
                            }
                            _scope = _scope.Parent;
                        }
                        throw new Exception("Инструкция break использована неправильно");
                    }
                case StatementNode.Type.ContinueStatement://Done
                    {
                        if (scope == null)
                            throw new Exception("Инструкция continue не должна использоваться в глобальной области видимости");
                        Scope _scope = scope;
                        while (_scope != null)
                        {
                            if (_scope is WhileLoopScope)
                            {
                                WhileLoopScope whileLoopScope = _scope as WhileLoopScope;
                                bytecode.AddInstruction(Instruction.JUMP);
                                bytecode.AddConstant(new Short((short)whileLoopScope.StartOffset));
                                return;
                            }
                            _scope = _scope.Parent;
                        }
                        throw new Exception("Отсутствует цикл для инструкции continue");
                    }
                case StatementNode.Type.WhileStatement://Done
                    {
                        if (scope == null)
                            throw new Exception("Инструкция while не должна использоваться в глобальной области видимости");
                        WhileNode node = statement as WhileNode;
                        if (!(InferType(scope, node.Condition) is BoolType))
                            throw new Exception("Условие цикла while должно иметь булев тип");
                        int startOffset = bytecode.Size;
                        WhileLoopScope whileLoopScope = new WhileLoopScope(scope, startOffset);
                        CompileExpression(whileLoopScope, node.Condition);
                        Emit(Instruction.JUMP_NIF);
                        int endJumpOffset = bytecode.Size;
                        Emit(new Short(-1));//jump to the end of whileloop
                        CompileStatement(whileLoopScope, statement);
                        bytecode.Set(endJumpOffset, new Short((short)bytecode.Size));
                        foreach (var breakJumpOffset in whileLoopScope.BreakJumpOffsets)
                        {
                            bytecode.Set(breakJumpOffset, (new Short((short)bytecode.Size)));
                        }
                        return;
                    }
                case StatementNode.Type.IfStatement://Done
                    {
                        if (scope == null)
                            throw new Exception("Инструкция if не должна использоваться в глобальной области видимости");
                        IfNode node = statement as IfNode;
                        if (!(InferType(scope, node.Condition) is BoolType))
                            throw new Exception("Условие должно иметь булев тип");
                        CompileExpression(scope, node.Condition);
                        Emit(Instruction.JUMP_NIF);
                        int jumpOffset = bytecode.Size;
                        Emit(new Short(0));
                        CompileStatement(scope, node.IfBody);
                        bytecode.Set(jumpOffset,new Short((short)bytecode.Size));
                        return;
                    }
                case StatementNode.Type.IfElseStatement://Done
                    {
                        if (scope == null)
                            throw new Exception("Конструкция if else не должна использоваться в глобальной области видимости");
                        IfElseNode node = statement as IfElseNode;
                        if (!(InferType(scope, node.Condition) is BoolType))
                            throw new Exception("Условие должно иметь булев тип");
                        CompileExpression(scope, node.Condition);
                        Emit(Instruction.JUMP_NIF);
                        int jumpOffset = bytecode.Size;
                        Emit(new Short(0));
                        CompileStatement(scope, node.IfBody);
                        Emit(Instruction.JUMP);
                        int ifJumpOffset = bytecode.Size;
                        Emit(new Short(0));
                        bytecode.Set(jumpOffset, new Short((short)bytecode.Size));
                        CompileStatement(scope, node.ElseBody);
                        bytecode.Set(ifJumpOffset, new Short((short)bytecode.Size));
                        return;
                    }
                case StatementNode.Type.ScopeStatement://Done
                    {
                        if (scope == null)
                            throw new Exception("Некорректное объявление области видимости в глобальной области видимости");
                        ScopeNode node = statement as ScopeNode;
                        Scope inner = new Scope(scope);
                        foreach (var scopedStatement in node.Body)
                        {
                            CompileStatement(inner, scopedStatement);
                        }
                        return;
                    }
                case StatementNode.Type.SwitchStatement://TODO
                    {
                        if (scope == null)
                            throw new Exception("Конструкция switch case не должна использоваться в глобальной области видимости");
                        SwitchStatementNode node = statement as SwitchStatementNode;
                        Type type = InferType(scope,node.Expression);
                        bool hasDefault = false;
                        List<int> addresses = new List<int>();
                        CompileExpression(scope, node.Expression);
                        foreach (var @case in node.Cases)
                        {
                            if (@case.Value == null)//default case
                            {
                                if (hasDefault)
                                    throw new Exception("В блоке switch должен быть только один вариант по умолчанию");
                                hasDefault = true;
                                continue;
                            }
                            if (!type.Equals(InferType(scope, @case.Value)))
                                throw new Exception("Неправильный тип константы в операторе case");
                            Emit(Instruction.DUP);
                            CompileExpression(scope,@case.Value);
                            switch (@case.Value.ExpressionType)
                            {
                                case ExpressionNode.ExpType.FloatConstant:
                                    Emit(Instruction.FEQ);
                                    break;
                                case ExpressionNode.ExpType.BoolConstant:
                                case ExpressionNode.ExpType.IntConstant:
                                    Emit(Instruction.IEQ);
                                    break;
                                default:
                                    throw new Exception("Неправильный тип константы в операторе case");
                            }
                            Emit(Instruction.JUMP_IF);
                            addresses.Add(bytecode.Size);
                        }
                        Emit(Instruction.JUMP);
                        int defaultAddress = bytecode.Size;
                        SwitchCaseScope switchCaseScope = new SwitchCaseScope(scope);
                        foreach (var @case in node.Cases)
                        {
                            foreach (var caseStatement in @case.Body)
                            {
                                CompileStatement(scope, caseStatement);
                            }
                            if (@case.Value == null)//default case
                            {
                                bytecode.Set(defaultAddress, new Short((short)bytecode.Size));
                                continue;
                            }
                            bytecode.Set(addresses.First<int>(), new Short((short)bytecode.Size));
                            addresses.RemoveAt(0);
                        }
                        if (!hasDefault)
                            bytecode.Set(defaultAddress,new Short((short)bytecode.Size));
                        foreach (var address in addresses)
                            bytecode.Set(address, new Short((short)bytecode.Size));
                        return;
                    }
                default:
                    throw new Exception("Ошибка компилятора в Compiler.CompileStatement: неожиданный тип");
            }
        }
        private void Emit(Instruction i)
        {
            if (lines.Count == 0)
            {
                lines.Add(new Program.Line { LineNumber = currentLine, Count = 1 });
            }
            else
            {
                if (lines.Last().LineNumber == currentLine)
                {
                    lines.Last().Count++;
                }
                else
                {
                    lines.Add(new Program.Line() { LineNumber = currentLine, Count = 1 });
                }
            }
            bytecode.AddInstruction(i);
        }
        private void Emit(IValue v)
        {
            bytecode.AddConstant(v);
        }
        private Variable FindGlobalSymbol(string symbol)
        {
            if (globalsDictionary.ContainsKey(symbol))
                return globalsDictionary[symbol];
            return null;
        }
        private Variable FindLocalSymbol(Scope scope, string symbol)
        {
            Scope _scope = scope;
            while (_scope != null)
            {
                if (_scope.VariableDictionary.ContainsKey(symbol))
                {
                    Variable var = _scope.VariableDictionary[symbol];
                    return var;
                }
                _scope = scope.Parent;
            }
            return null;
        }
        private void CompileExpression(Scope scope,ExpressionNode expression)
        {
#if DEBUG || TEST
            Console.WriteLine(expression.ExpressionType.ToString());
#endif
            currentLine = expression.Line;
            switch (expression.ExpressionType)
            {
                case ExpressionNode.ExpType.PostDecrement:
                case ExpressionNode.ExpType.PostIncrement:
                case ExpressionNode.ExpType.PreDecrement:
                case ExpressionNode.ExpType.PreIncrement:
                    throw new NotImplementedException();
                case ExpressionNode.ExpType.Addition://Done
                    {
                        AdditionNode _node = expression as AdditionNode;
                        Type type = InferType(scope, _node.Left);
                        CheckType(scope,_node.Right, type);
                        if (type is FloatType)
                        {
                            CompileExpression(scope, _node.Left);
                            CompileExpression(scope, _node.Right);
                            Emit(Instruction.FADD);
                        }
                        else if (type is IntType)
                        {
                            CompileExpression(scope, _node.Left);
                            CompileExpression(scope, _node.Right);
                            Emit(Instruction.IADD);
                        }
                        else
                        {
                            throw new Exception("Некорректный тип выражения");
                        }
                        //scope.AddToStackPointer((-2+1) * (type as BasicType).GetByteSize());
                        scope.AddToStackPointer(-1);
                        return;
                    }
                case ExpressionNode.ExpType.Subtraction://Done
                    {
                        SubtractionNode _node = expression as SubtractionNode;
                        Type type = InferType(scope, _node.Left);
                        CheckType(scope, _node.Right, type);
                        if (type is FloatType)
                        {
                            CompileExpression(scope, _node.Left);
                            CompileExpression(scope, _node.Right);
                            Emit(Instruction.FSUB);
                        }
                        else if (type is IntType)
                        {
                            CompileExpression(scope, _node.Left);
                            CompileExpression(scope, _node.Right);
                            Emit(Instruction.ISUB);
                        }
                        else
                        {
                            throw new Exception("Некорректный тип выражения");
                        }
                        //scope.AddToStackPointer((-2+1) * (type as BasicType).GetByteSize());
                        scope.AddToStackPointer(-1);
                        return;
                    }
                case ExpressionNode.ExpType.Multiplication://Done
                    {
                        MultiplicationNode _node = expression as MultiplicationNode;
                        Type type = InferType(scope, _node.Left);
                        CheckType(scope, _node.Right, type);
                        if (type is FloatType)
                        {
                            CompileExpression(scope, _node.Left);
                            CompileExpression(scope, _node.Right);
                            Emit(Instruction.FMUL);
                        }
                        else if (type is IntType)
                        {
                            CompileExpression(scope, _node.Left);
                            CompileExpression(scope, _node.Right);
                            Emit(Instruction.IMUL);
                        }
                        else
                        {
                            throw new Exception("Некорректный тип выражения");
                        }
                        //scope.AddToStackPointer((-2+1) * (type as BasicType).GetByteSize());
                        scope.AddToStackPointer(-1);
                        return;
                    }
                case ExpressionNode.ExpType.Division://Done
                    {
                        DivisionNode _node = expression as DivisionNode;
                        Type type = InferType(scope, _node.Left);
                        CheckType(scope, _node.Right, type);
                        if (type is FloatType)
                        {
                            CompileExpression(scope, _node.Left);
                            CompileExpression(scope, _node.Right);
                            Emit(Instruction.FDIV);
                        }
                        else if (type is IntType)
                        {
                            CompileExpression(scope, _node.Left);
                            CompileExpression(scope, _node.Right);
                            Emit(Instruction.IDIV);
                        }
                        else
                        {
                            throw new Exception("Некорректный тип выражения");
                        }
                        //scope.AddToStackPointer((-2+1) * (type as BasicType).GetByteSize());
                        scope.AddToStackPointer(-1);
                        return;
                    }
                case ExpressionNode.ExpType.Mod:
                    {
                        ModNode _node = expression as ModNode;
                        Type type = InferType(scope, _node.Left);
                        CheckType(scope, _node.Right, type);
                        if (type is FloatType)
                        {
                            CompileExpression(scope, _node.Left);
                            CompileExpression(scope, _node.Right);
                            Emit(Instruction.FMOD);
                        }
                        else if (type is IntType)
                        {
                            CompileExpression(scope, _node.Left);
                            CompileExpression(scope, _node.Right);
                            Emit(Instruction.IMOD);
                        }
                        else
                        {
                            throw new Exception("Некорректный тип выражения");
                        }
                        //scope.AddToStackPointer((-2+1) * (type as BasicType).GetByteSize());
                        scope.AddToStackPointer(-1);
                        return;
                    }
                case ExpressionNode.ExpType.Negation://Done
                    {
                        NegationNode _node = expression as NegationNode;
                        Type type = InferType(scope, _node.Inner);
                        if (type is FloatType)
                        {
                            CompileExpression(scope, _node.Inner);
                            Emit(Instruction.FNEG);
                        }
                        else if (type is IntType)
                        {
                            CompileExpression(scope, _node.Inner);
                            Emit(Instruction.INEG);
                        }
                        else
                        {
                            throw new Exception("Некорректный тип выражения");
                        }
                        //scope.AddToStackPointer((-2+1) * (type as BasicType).GetByteSize());
                        return;
                    }
                case ExpressionNode.ExpType.And://Done
                    {
                        AndNode _node = expression as AndNode;
                        Type type = InferType(scope, _node.Left);
                        CheckType(scope,_node.Right,type);
                        if (type is BoolType)
                        {
                            CompileExpression(scope, _node.Left);
                            CompileExpression(scope, _node.Right);
                            Emit(Instruction.BAND);
                        }
                        else
                        {
                            throw new Exception("Некорректный тип выражения");
                        }
                        scope.AddToStackPointer(-1);
                        return;
                    }
                case ExpressionNode.ExpType.Or://Done
                    {
                        OrNode _node = expression as OrNode;
                        Type type = InferType(scope, _node.Left);
                        CheckType(scope, _node.Right, type);
                        if (type is BoolType)
                        {
                            CompileExpression(scope, _node.Left);
                            CompileExpression(scope, _node.Right);
                            Emit(Instruction.BOR);
                        }
                        else
                        {
                            throw new Exception("Некорректный тип выражения");
                        }
                        scope.AddToStackPointer(-1);
                        return;
                    }
                case ExpressionNode.ExpType.Not://Done
                    {
                        NotNode _node = expression as NotNode;
                        Type type = InferType(scope, _node.Inner);
                        if (type is BoolType)
                        {
                            CompileExpression(scope, _node.Inner);
                            Emit(Instruction.BNOT);
                        }
                        else
                        {
                            throw new Exception("Некорректный тип выражения");
                        }
                        return;
                    }
                case ExpressionNode.ExpType.Equal://Done
                    {
                        EqualNode _node = expression as EqualNode;
                        Type type = InferType(scope, _node.Left);
                        CheckType(scope, _node.Right, type);
                        CompileExpression(scope, _node.Left);
                        CompileExpression(scope, _node.Right);
                        if (type is FloatType)
                        {
                            Emit(Instruction.FEQ);
                        }
                        else if (type is IntType || type is BoolType)
                        {
                            Emit(Instruction.IEQ);
                        }
                        else
                        {
                            throw new Exception("Некорректный тип выражения");
                        }
                        scope.AddToStackPointer(-1);
                        return;
                    }
                case ExpressionNode.ExpType.NotEqual://Done
                    {
                        NotEqualNode _node = expression as NotEqualNode;
                        Type type = InferType(scope, _node.Left);
                        CheckType(scope, _node.Right, type);
                        CompileExpression(scope, _node.Left);
                        CompileExpression(scope, _node.Right);
                        if (type is FloatType)
                        {
                            Emit(Instruction.FNEQ);
                        }
                        else if (type is IntType|| type is BoolType)
                        {
                            Emit(Instruction.INEQ);
                        }
                        else
                        {
                            throw new Exception("Некорректный тип выражения");
                        }
                        scope.AddToStackPointer(-1);
                        return;
                    }
                case ExpressionNode.ExpType.Greater://Done
                    {
                        GreaterNode _node = expression as GreaterNode;
                        Type type = InferType(scope, _node.Left);
                        CheckType(scope, _node.Right, type);
                        CompileExpression(scope, _node.Left);
                        CompileExpression(scope, _node.Right);
                        if (type is FloatType)
                        {
                            Emit(Instruction.FGR);
                        }else if (type is IntType)
                        {
                            Emit(Instruction.IGR);
                        }
                        else
                        {
                            throw new Exception("Некорректный тип выражения");
                        }
                        scope.AddToStackPointer(-1);
                        return;
                    }
                case ExpressionNode.ExpType.GreaterEqual://Done
                    {
                        GreaterEqualNode _node = expression as GreaterEqualNode;
                        Type type = InferType(scope, _node.Left);
                        CheckType(scope, _node.Right, type);
                        CompileExpression(scope, _node.Left);
                        CompileExpression(scope, _node.Right);
                        if (type is FloatType)
                        {
                            Emit(Instruction.FGREQ);
                        }
                        else if (type is IntType)
                        {
                            Emit(Instruction.IGREQ);
                        }
                        else
                        {
                            throw new Exception("Некорректный тип выражения");
                        }
                        scope.AddToStackPointer(-1);
                        return;
                    }
                case ExpressionNode.ExpType.Less://Done
                    {
                        LessNode _node = expression as LessNode;
                        Type type = InferType(scope, _node.Left);
                        CheckType(scope, _node.Right, type);
                        CompileExpression(scope, _node.Left);
                        CompileExpression(scope, _node.Right);
                        if (type is FloatType)
                        {
                            Emit(Instruction.FLS);
                        }
                        else if (type is IntType)
                        {
                            Emit(Instruction.ILS);
                        }
                        else
                        {
                            throw new Exception("Некорректный тип выражения");
                        }
                        scope.AddToStackPointer(-1);
                        return;
                    }
                case ExpressionNode.ExpType.LessEqual://Done
                    {
                        LessEqualNode _node = expression as LessEqualNode;
                        Type type = InferType(scope, _node.Left);
                        CheckType(scope, _node.Right, type);
                        CompileExpression(scope, _node.Left);
                        CompileExpression(scope, _node.Right);
                        if (type is FloatType)
                        {
                            Emit(Instruction.FLSEQ);
                        }
                        else if (type is IntType)
                        {
                            Emit(Instruction.ILSEQ);
                        }
                        else
                        {
                            throw new Exception("Некорректный тип выражения");
                        }
                        scope.AddToStackPointer(-1);
                        return;
                    }
                case ExpressionNode.ExpType.FunctionCall://Done?
                    {
                        FunctionCallNode functionCallNode = expression as FunctionCallNode;
                        //TODO add native functions
                        if (functionsDictionary.ContainsKey(functionCallNode.Name))
                        {
                            FunctionType type = functionsDictionary[functionCallNode.Name];
                            if (type.Signature.ArgumentTypes.Count != functionCallNode.Arguments.Count)
                                throw new Exception($"Неверное количество аргументов в вызове функции\"{functionCallNode.Name}\"");
                            int stackPointer = scope.StackPointer;
                            for (int i = 0; i < type.Signature.ArgumentTypes.Count; i++)
                            {
                                FunctionSignature.ArgType argType = type.Signature.ArgumentTypes[i];
                                ExpressionNode argExp = functionCallNode.Arguments[i];
                                Type expType = InferType(scope, argExp);
                                if (!expType.Equals(argType.Type))
                                    throw new Exception($"Тип выражения \"{expType.GetName()}\" не совпадает с типом аргумента \"{argType.Type.GetName()}\" в объявлении функции \"{functionCallNode.Name}\" ");
                                if (argType.IsLocal)
                                {
                                    CompileExpression(scope, argExp);
                                }
                                else //load ref
                                {
                                    int slot = 0;
                                    if (argExp is IdentifierNode)
                                    {
                                        IdentifierNode identifierNode = argExp as IdentifierNode;
                                        //check ref or non-ref
                                        Variable var = null;
                                        if ((var = FindLocalSymbol(scope, identifierNode.Identifier)) != null)
                                        {
                                            if (var.Type is RefType)
                                            {
                                                Emit(Instruction.ILOAD);
                                                Emit(new Short((short)var.Index));
                                                scope.AddToStackPointer(1);
                                                continue;
                                            }
                                            else
                                            {
                                                slot = var.Index;
                                            }
                                        }
                                        if ((var = FindGlobalSymbol(identifierNode.Identifier)) != null)
                                        {
                                            Emit(Instruction.IPUSH);
                                            Emit(new Int(var.Index));
                                            scope.AddToStackPointer(1);
                                            continue;
                                        }

                                    }
                                    else if (argExp is FieldAccessNode)
                                    {
                                        /*
                                         if ref
                                         load baseRef + field slot
                                         else 
                                            rcreate slot
                                         */
                                        throw new NotImplementedException();
                                    }
                                    else if (argExp is ArrayAccessNode)
                                    {
                                        /*
                                         if ref
                                         load baseRef + field slot
                                         else 
                                            rcreate slot
                                         */
                                        throw new NotImplementedException();
                                    }
                                    else
                                        throw new Exception("Только l-value можно использовать в качестве inout-аргумента");
                                    Emit(Instruction.RCREATE);
                                    Emit(new Short((short)slot));
                                    scope.AddToStackPointer(1);
                                }
                                /*
                                 for each argument
                                 if Identifier, Field, ArrayAccess and INOUT way
                                    push global index
                                 if Identifier, Field, ArrayAccess and In way
                                    push load()
                                 if(!(Identifier, Field, ArrayAccess) and INOUT way)
                                    throw error;
                                 */
                            }
                            //call function
                            Emit(Instruction.CALL);
                            Emit(new Short((short)functionsDictionary[functionCallNode.Name].Index));
                            scope.AddToStackPointer(stackPointer - scope.StackPointer);
                            if(type.Signature.ReturnType !=null)
                                scope.AddToStackPointer(type.Signature.ReturnType.GetSlotSize());
                        }else if (nativeFunctionsDictionary.ContainsKey(functionCallNode.Name))
                        {
                           NativeFunctionType type = nativeFunctionsDictionary[functionCallNode.Name];
                            if (type.Signature.ArgumentTypes.Count != functionCallNode.Arguments.Count)
                                throw new Exception($"Неверное количество аргументов в вызове функции\"{functionCallNode.Name}\"");
                            int stackPointer = scope.StackPointer;
                            for (int i = 0; i < type.Signature.ArgumentTypes.Count; i++)
                            {
                                FunctionSignature.ArgType argType = type.Signature.ArgumentTypes[i];
                                ExpressionNode argExp = functionCallNode.Arguments[i];
                                Type expType = InferType(scope, argExp);
                                if (!expType.Equals(argType.Type))
                                    throw new Exception($"Тип выражения \"{expType.GetName()}\" не совпадает с типом аргумента \"{argType.Type.GetName()}\" в объявлении функции \"{functionCallNode.Name}\" ");
                                if (argType.IsLocal)
                                {
                                    CompileExpression(scope, argExp);
                                }
                                else //load ref
                                {
                                    int slot = 0;
                                    if (argExp is IdentifierNode)
                                    {
                                        IdentifierNode identifierNode = argExp as IdentifierNode;
                                        //check ref or non-ref
                                        Variable var = null;
                                        if ((var = FindLocalSymbol(scope, identifierNode.Identifier)) != null)
                                        {
                                            if (var.Type is RefType)
                                            {
                                                Emit(Instruction.ILOAD);
                                                Emit(new Short((short)var.Index));
                                                scope.AddToStackPointer(1);
                                                continue;
                                            }
                                            else
                                            {
                                                slot = var.Index;
                                            }
                                        }
                                        if ((var = FindGlobalSymbol(identifierNode.Identifier)) != null)
                                        {
                                            Emit(Instruction.IPUSH);
                                            Emit(new Int(var.Index));
                                            scope.AddToStackPointer(1);
                                            continue;
                                        }

                                    }
                                    else if (argExp is FieldAccessNode)
                                    {
                                        /*
                                         if ref
                                         load baseRef + field slot
                                         else 
                                            rcreate slot
                                         */
                                        throw new NotImplementedException();
                                    }
                                    else if (argExp is ArrayAccessNode)
                                    {
                                        /*
                                         if ref
                                         load baseRef + field slot
                                         else 
                                            rcreate slot
                                         */
                                        throw new NotImplementedException();
                                    }
                                    else
                                        throw new Exception("Только l-value можно использовать в качестве inout-аргумента");
                                    Emit(Instruction.RCREATE);
                                    Emit(new Short((short)slot));
                                    scope.AddToStackPointer(1);
                                }
                                /*
                                 for each argument
                                 if Identifier, Field, ArrayAccess and INOUT way
                                    push global index
                                 if Identifier, Field, ArrayAccess and In way
                                    push load()
                                 if(!(Identifier, Field, ArrayAccess) and INOUT way)
                                    throw error;
                                 */
                            }
                            //call function
                            Emit(Instruction.CALL_NATIVE);
                            Emit(new Short((short)nativeFunctionsDictionary[functionCallNode.Name].Index));
                            scope.AddToStackPointer(stackPointer - scope.StackPointer);
                            if (type.Signature.ReturnType != null)
                                scope.AddToStackPointer(type.Signature.ReturnType.GetSlotSize());
                        }
                        else
                        {
                            throw new Exception($"Вызов необъявленной функции \"{functionCallNode.Name}\"");
                        }
                        break;
                    }
                case ExpressionNode.ExpType.Assignment://Done
                    {
                        AssignmentNode assignmentNode = expression as AssignmentNode;
                        Type type = InferType(scope, assignmentNode.Left);
                        CheckType(scope, assignmentNode.Right, type);
                        CompileExpression(scope, assignmentNode.Right);
                        if (assignmentNode.Left is IdentifierNode)
                        {
                            IdentifierNode identifierNode = assignmentNode.Left as IdentifierNode;
                            Variable var = null;
                            if ((var = FindLocalSymbol(scope, identifierNode.Identifier)) != null)
                            {
                                var.Save(this, false);
                                return;
                            }
                            else if ((var = FindGlobalSymbol(identifierNode.Identifier)) != null)
                            {
                                var.Save(this, true);
                                return;
                            }
                            throw new Exception($"Неизвестный символ \"{identifierNode.Identifier}\"");
                        }
                        else if (assignmentNode.Left is ArrayAccessNode)
                        {
                            throw new NotImplementedException();
                        }
                        else if (assignmentNode.Left is FieldAccessNode)
                        {
                            throw new NotImplementedException();
                        }
                        else
                            throw new Exception("Left part of assignment should be l-value");
                    }
                case ExpressionNode.ExpType.ArrayAccess:
                    {
                        ArrayAccessNode node = expression as ArrayAccessNode;
                        CheckType(scope, node.Index, basicTypes["int"]);
                        /*
                         get node.Parent() {isLocal,offset,parentType};
                         get parentType.offset for current
                         load currentType
                         
                            Compile(scope, node.Index)
                            Emit(sizeofParent);
                            Emit(Instruction.IMUL);
                            Emit(Instruction.IADD);
                            Emit(Instruction.IRLOAD);
                         */
                        throw new NotImplementedException();
                        if (!(node.Array is IdentifierNode))
                            throw new Exception($"Использование оператора [] с некорректным типом выражения");
                        CheckType(scope, node.Index, basicTypes["int"]);
                        IdentifierNode identifier = node.Array as IdentifierNode;
                        CompileExpression(scope, node.Index);
                        Variable var = null;
                        if((var = FindLocalSymbol(scope, identifier.Identifier))!=null)
                        {
                            if (var.Type is RefType)
                            {

                            }
                            else if (var.Type is ArrayType)
                            {

                            }
                            else
                                throw new Exception("Оператор [] используется для переменной, не являющейся массивом");
                            return;
                        }
                        if ((var = FindGlobalSymbol(identifier.Identifier)) != null)
                        {

                            //Emit(Instruction.)
                            Emit(Instruction.IADD);
                            return;
                        }
                        throw new Exception($"Неизвестная переменная \"{identifier.Identifier}\"");
                    }
                case ExpressionNode.ExpType.FieldExpression:
                    {
                        FieldAccessNode node = expression as FieldAccessNode;
                        /*
                            Emit(Instruction.RCREATE) if local or emit(Instruction.IPUSH) if global
                         get node.Parent() {isLocal,offset,parentType};
                         get parentType.offset for current
                         load currentType
                         if(isLocal)
                         {
                         }else
                         {
                         
                        }
                            Emit(Instruction.IADD);
                            Emit(new Short((short)fieldOffset));
                            Emit(Instruction.IRLOAD);
                         */
                        throw new NotImplementedException();
                        ExpressionNode _node = node.Parent;
                        List<ExpressionNode> chain = ResolveChain(node.Parent);
                        //get base type and compute index of field in loop
                        if (!(chain.First() is IdentifierNode))
                            throw new Exception("Invalid expression");
                        Type baseType;
                        Type currentType = baseType is RefType?(baseType as RefType).BaseType:baseType;
                        int relativeIndex = 0;
                        for (int i = 0; i < chain.Count; i++)
                        {
                            if (currentType is StructType)
                            {

                            }
                            else if (currentType is ArrayType)
                            {

                            }
                        }//check last node
                        if (_node is IdentifierNode)
                        {

                        }
                        else throw new Exception($"Некорректное обращение к структуре");
                        /*while (true)
                        {
                            if (_node is FieldAccessNode)
                            {
                                identifierChain.Add((_node as FieldAccessNode).Field);
                                _node = (_node as FieldAccessNode).Parent;
                            }
                            else if (_node is IdentifierNode)
                            {

                            }
                        }*/
                    }
                case ExpressionNode.ExpType.TernaryOperator://Done
                    {
                        TernaryOperatorNode operatorNode = expression as TernaryOperatorNode;
                        if (InferType(scope, operatorNode.Condition) is BoolType)
                        {
                            CompileExpression(scope, operatorNode.Condition);
                            Emit(Instruction.JUMP_NIF);
                            int elseJumpOffset = bytecode.Size;
                            Emit(new Short(0));
                            CompileExpression(scope,operatorNode.First);
                            Emit(Instruction.JUMP);
                            int ifJumpOffset = bytecode.Size;
                            Emit(new Short(0));
                            bytecode.Set(elseJumpOffset, new Short((short)bytecode.Size));
                            CompileStatement(scope, operatorNode.Second);
                            bytecode.Set(ifJumpOffset, new Short((short)bytecode.Size));
                            return;
                        }
                        else
                            throw new Exception($"Неверный тип условия в тернарном операторе.");
                    }
                /*case ExpressionNode.ExpType.ArrayInitializerList:
                    {
                        ArrayInitializerListNode arrayInitializer = expression as arrayInitializer;
                    }*/
                /*case ExpressionNode.ExpType.StructInitializerList:
                    {
                        StructInitializerListNode structInitializer = expression as StructInitilizerListNode;
                    }*/
                case ExpressionNode.ExpType.IdentifierExpression://Done
                    {
                        IdentifierNode identifierNode = expression as IdentifierNode;
                        Variable var = null;
                        if ((var = FindLocalSymbol(scope, identifierNode.Identifier)) != null)
                        {
                            var.Load(this, false);
                            scope.AddToStackPointer(var.Type.GetSlotSize());
                            return;
                        }
                        else if ((var = FindGlobalSymbol(identifierNode.Identifier)) != null)
                        {
                            var.Load(this, true);
                            scope.AddToStackPointer(var.Type.GetSlotSize());
                            return;
                        }
                        throw new Exception($"Неизвестный символ \"{identifierNode.Identifier}\"");
                    }
                case ExpressionNode.ExpType.IntConstant://Done
                    {
                        IntegerNode integerNode = expression as IntegerNode;
                        Emit(Instruction.IPUSH);
                        Emit(new Int(integerNode.Value));
                        scope.AddToStackPointer(1);
                        return;
                    }
                case ExpressionNode.ExpType.FloatConstant://Done
                    {
                        FloatNode floatNode = expression as FloatNode;
                        Emit(Instruction.FPUSH);
                        Emit(new Float(floatNode.Value));
                        scope.AddToStackPointer(1);
                        return;
                    }
                case ExpressionNode.ExpType.BoolConstant://Done
                    {
                        BoolNode boolNode = expression as BoolNode;
                        Emit(Instruction.IPUSH);
                        Emit(new Int(boolNode.Value ? 1: 0));
                        scope.AddToStackPointer(1);
                        return;
                    }
                case ExpressionNode.ExpType.Cast://Done
                    {
                        CastNode castNode = expression as CastNode;
                        Type destType = ResolveType(castNode.TypeName);
                        Type srcType = InferType(scope, castNode.Exp);
                        scope.AddToStackPointer(srcType.GetSlotSize() - destType.GetSlotSize());
                        if (srcType is BasicType&&destType is BasicType)
                        {
                            BasicType _destType = destType as BasicType;
                            CompileExpression(scope,castNode.Exp);
                            switch ((srcType as BasicType).Type)
                            {
                                case BasicType.BType.Bool:
                                    break;
                                case BasicType.BType.Float:
                                    switch (_destType.Type)
                                    {
                                        case BasicType.BType.Bool:
                                            Emit(Instruction.F2I);
                                            return;
                                        case BasicType.BType.Float:
                                            return;
                                        case BasicType.BType.Int:
                                            Emit(Instruction.F2I);
                                            return;
                                    }
                                    break;
                                case BasicType.BType.Int:
                                    switch (_destType.Type)
                                    {
                                        case BasicType.BType.Bool:
                                            return;
                                        case BasicType.BType.Float:
                                            Emit(Instruction.I2F);
                                            return;
                                        case BasicType.BType.Int:
                                            return;
                                    }
                                    break;
                                case BasicType.BType.Void:
                                    break;
                            }
                        }
                        throw new Exception($"Нельзя преобразовать тип \"{srcType.GetName()}\" в \"{castNode.TypeName}\"");
                    }
                default:
                    throw new Exception("Ошибка компилятора в Compiler.CompileExpression: неожиданный тип");

            }
        }
        private void CheckType(Scope scope, ExpressionNode exp, Type type)
        {
            Type expType = InferType(scope, exp);
            if(!expType.Equals(type))
                throw new Exception($"Ожидался тип \"{type.GetName()}\", получен тип \"{expType.GetName()}\"");
        }
        private Type InferType(Scope scope, ExpressionNode exp)
        {
            switch (exp.ExpressionType)
            {
                case ExpressionNode.ExpType.Negation:
                case ExpressionNode.ExpType.PostIncrement:
                case ExpressionNode.ExpType.PostDecrement:
                case ExpressionNode.ExpType.PreIncrement:
                case ExpressionNode.ExpType.PreDecrement:
                    return InferType(scope,(exp as UnaryOperatorNode).Inner);
                //unary
                case ExpressionNode.ExpType.Addition:
                case ExpressionNode.ExpType.Subtraction:
                case ExpressionNode.ExpType.Multiplication:
                case ExpressionNode.ExpType.Division:
                case ExpressionNode.ExpType.Mod:
                    {
                        Type type = InferType(scope ,(exp as BinaryOperatorNode).Left);
                        CheckType(scope ,(exp as BinaryOperatorNode).Right,type);
                        return type;
                    }
                case ExpressionNode.ExpType.Equal:
                case ExpressionNode.ExpType.NotEqual:
                case ExpressionNode.ExpType.Or:
                case ExpressionNode.ExpType.Not:
                case ExpressionNode.ExpType.And:
                case ExpressionNode.ExpType.Greater:
                case ExpressionNode.ExpType.GreaterEqual:
                case ExpressionNode.ExpType.Less:
                case ExpressionNode.ExpType.LessEqual:
                    return basicTypes["bool"];
                case ExpressionNode.ExpType.TernaryOperator:
                    {
                        TernaryOperatorNode node = exp as TernaryOperatorNode;
                        Type type = InferType(scope, node.First);
                        CheckType(scope, node.Second, type);
                        return type;
                    }
                case ExpressionNode.ExpType.ArrayAccess:
                    {
                        throw new NotImplementedException();
                    }
                case ExpressionNode.ExpType.FieldExpression:
                    {
                        throw new NotImplementedException();
                    }
                case ExpressionNode.ExpType.Assignment:
                    {
                        AssignmentNode node = exp as AssignmentNode;
                        Type type = InferType(scope, node.Left);
                        CheckType(scope, node.Right, type);
                        return type;
                    }
                case ExpressionNode.ExpType.Cast:
                    {
                        CastNode node = exp as CastNode;
                        if (basicTypes.ContainsKey(node.TypeName))
                            return basicTypes[node.TypeName];
                        if (structs.ContainsKey(node.TypeName))
                            return structs[node.TypeName];
                        throw new Exception($"Неизвестный тип \"{node.TypeName}\"");
                    }
                case ExpressionNode.ExpType.FunctionCall:
                    if (functionsDictionary.ContainsKey((exp as FunctionCallNode).Name))
                    {
                        return functionsDictionary[(exp as FunctionCallNode).Name].Signature.ReturnType;
                    }
                    throw new Exception("Вызов необъявленной функции");
                case ExpressionNode.ExpType.IntConstant:
                    return basicTypes["int"];
                case ExpressionNode.ExpType.BoolConstant:
                    return basicTypes["bool"];
                case ExpressionNode.ExpType.FloatConstant:
                    return basicTypes["float"];
                case ExpressionNode.ExpType.ArrayInitializerList:
                    {
                        ArrayInitializerList node = exp as ArrayInitializerList;
                        return new ArrayType(ResolveType(node.TypeName),node.Size);
                    }
                case ExpressionNode.ExpType.StructInitializerList:
                    {
                        StructInitializerList node = exp as StructInitializerList;
                        if (this.structs.ContainsKey(node.StructName))
                            return structs[node.StructName];
                        throw new Exception($"Использование неизвестной структуры \"{node.StructName}\"");
                    }
                case ExpressionNode.ExpType.IdentifierExpression:
                    {
                        IdentifierNode node = exp as IdentifierNode;

                        Variable var = null;
                        if ((var = FindLocalSymbol(scope, node.Identifier)) != null)
                        {
                            Type type = var.Type;
                            if (type is RefType)
                            {
                                return (type as RefType).BaseType;
                            }
                            return type;
                        }
                        if ((var = FindGlobalSymbol(node.Identifier)) != null)
                        {
                            return var.Type;
                        }
                        throw new Exception($"Обращение к неопределённой переменной \"{node.Identifier}\"");
                    }
            }
            throw new Exception("Критическая ошибка");
        }
    }
#endif
}
