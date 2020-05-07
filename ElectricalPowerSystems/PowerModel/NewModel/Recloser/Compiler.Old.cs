#define COMPILER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if A
namespace ElectricalPowerSystems.PowerModel.NewModel.Recloser
{
#if COMPILER

    public abstract class Compiler
    {
        class Scope
        {
            bool hasReturn;
            Dictionary<string, Type> variables;
            Scope parent;
            public Scope Parent { get { return parent; } }
            public bool HasReturn { get { return hasReturn; } }
            int stackPointer;
            public int StackPointer { get { return stackPointer; } }
            public Scope(Scope parent)
            {
                stackPointer = 0;
                hasReturn = false;
                this.parent = parent;
            }
            public void AddToStackPointer(int bytes)
            {
                stackPointer += bytes;
            }
        }
        class FunctionScope:Scope
        {
            Function function;
            Dictionary<string, Type> arguments;
            BasicType returnType;
            //int startOffset;
            public BasicType ReturnType { get { return returnType; } }
            public FunctionScope(Scope parent, BasicType returnType) : base(parent)
            {
                this.returnType = returnType;
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
            public enum EType
            {
                Int,
                Bool,
                Float,
                Void
            }
        }
        public class ArrayType : Type
        {
            BasicType type;
            int size;
        }
        public class StructType : Type
        {
            class Field
            {
                BasicType type;
                int offset;
                public Field(BasicType type, int offset)
                {
                    this.type = type;
                    this.offset = offset;
                }
                public BasicType GetFieldType()
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
            int size;
            public StructType(string name)
            {
                size = 0;
                this.name = name;
                this.fields = new Dictionary<string, Field>();
            }
            public void AddField(string fieldName,BasicType type)
            {
                int offset = size;
                size +=type.GetByteSize();
                this.fields.Add(fieldName,new Field(type,offset));
            }
            public string GetName()
            {
                return name;
            }
            public BasicType GetFieldType(string field)
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
        }
        public abstract class BasicType:Type
        {
            public abstract int GetByteSize();

        }
        public class FloatType : BasicType
        {
            public override int GetByteSize()
            {
                return Float.Sizeof();
            }
        }
        public class IntType : BasicType
        {
            public override int GetByteSize()
            {
                return Int.Sizeof();
            }
        }
        public class BoolType : BasicType
        {
            public override int GetByteSize()
            {
                return Bool.Sizeof();
            }
        }
        public class Function
        {
            BasicType ReturnType;
            List<Type> ArgumentTypes;
            int Index;
            int Offset;
        }
        public class Environment
        {
            BasicType returnType;
            Type[] localVariables;
            Type[] arguments;
        }
        ByteArray bytecode;
        int stackCounter;
        ByteArray globals;
        int functions;
        Dictionary<string, StructType> structs;
        Dictionary<string, Function> functions;
        int programSizeLimit;
        //Dictionary<string, NativeFunction> nativeFunctions;
        public Program Compile(ProgramNode program, int programSizeLimit)
        {
            currentFunction = null;
            stackCounter = 0;
            functions = 0;
            bytecode = new ByteArray();
            globals = new ByteArray();
            this.programSizeLimit = programSizeLimit;
            foreach (var statement in program.Statements)
            {
                CompileStatement(null,statement);
            }
            return new Program();
        }
        private Type ResolveType(string typename)
        {
            switch (typename)
            {
                case "float":
                    return new FloatType();
                case "int":
                    return new IntType();
                case "bool":
                    return new BoolType();
                default:
                    if (structs.ContainsKey(typename))
                    {
                        return structs[typename];
                    }
                 throw new Exception($"Неизвестный тип \"{typename}\"");
            }
        }
        private Type InferType(Scope scope, ExpressionNode exp)
        {
            //if unary return type

            //if binary

            //if function
        }
        private void CompileStatement(Scope scope, StatementNode statement)
        {
            if (programSizeLimit < bytecode.Size)
                throw new Exception($"Размер программы превысил ограничение в {programSizeLimit}");
#if DEBUG || TEST
            Console.Write(statement.StatementType.ToString());
#endif
            switch (statement.StatementType)
            {
                case StatementNode.Type.VariableDeclarationStatement://TODO
                    {
                        VariableDeclarationNode
                        if (scope == null)//global variable
                        {

                        }
                        else {//local variable

                        }
                    }
                case StatementNode.Type.StructDefinition:
                    {
                        StructDefinitionNode node = statement as StructDefinitionNode;
                        if (scope != null)
                            throw new Exception($"Нельзя определять структуру \"{node.Name}\" в локальной области видимости");
                        if (structs.ContainsKey(node.Name))
                            throw new Exception($"Переопределение структуры \"{node.Name}\"");
                        StructType type = new StructType(node.Name);
                        foreach (var field in node.Fields)
                        {
                            Type fieldType = ResolveType(field.FieldType.Typename);
                            if (field.FieldType.Size != null)
                            {
                                //TODO разобраться с массивами в структуре
                                throw new Exception($"Нельзя использовать массив в структуре");
                            }
                            if (!(fieldType is BasicType))
                                throw new Exception($"Нельзя использовать структуры в структурах");
                            type.AddField(field.Name,fieldType as BasicType);
                        }
                        structs.Add(node.Name, type);
                        return;
                    }
                case StatementNode.Type.FunctionDefinition:
                    {//TODO
                        FunctionDefinitionNode node = statement as FunctionDefinitionNode;
                        if (scope != null)//global variable
                        {
                            throw new Exception("Определение функции не в глобальной области видимости");
                        }
                        //TODO args
                        FunctionScope _scope = new FunctionScope(scope);
                        foreach (var functionStatement in node.Body)
                        {
                            CompileStatement(_scope,functionStatement);
                        }
                        if (_scope.HasReturn == false)
                            throw new Exception($"Отсутствует оператор return в определении функции \"{node.Name}\"");
                        return;
                    }
                case StatementNode.Type.FunctionDeclaration:
                    {//TODO
                        FunctionDeclarationNode node = statement as FunctionDeclarationNode;
                        if (scope != null)
                        {
                            throw new Exception("Объявление функции не в глобальной области видимости");
                        }
                        return;
                    }
                case StatementNode.Type.ExpressionStatement:
                    {//TODO
                        if (scope == null)
                            throw new Exception("Выражения не должны использоваться в глобальной области видимости");
                        int stackPointer = scope.StackPointer;
                        CompileExpression(scope, statement as ExpressionNode);
                        int stackDifference = scope.StackPointer - stackPointer;
                        if (stackDifference > 0)
                        {
                            bytecode.AddInstruction(Instruction.SUB_SP);//TODO rewrite stack
                            bytecode.AddConstant(new Short((short)stackDifference));
                        }
#if DEBUG || TEST
                        if (stackDifference < 0)
                            throw new Exception("Expression corrupted the stack");
#endif
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
                            }
                            _scope = _scope.Parent;
                        }
                        if(functionScope == null)
                            throw new Exception("Инструкция return используется не внутри функции");

                        if (node.Expression == null)
                        {
                            if (functionScope.ReturnType == null)
                            {
                                Emit(Instruction.RET);
                                return;
                            }
                            throw new Exception("Тип возвращаемого значения не совпадает с типов в объявлении функции");
                        }
                        //get type of expression
                        Type expType = InferType(scope, node.Expression);
                        //if type is not equal to type of function throw exception
                        if (expType.GetType() != functionScope.ReturnType.GetType())
                        {
                            throw new Exception("Тип возвращаемого значения не совпадает с типов в объявлении функции");
                        }
                        CompileExpression(scope, node.Expression);
                        Emit(Instruction.SET_RESULT);
                        Emit(new Short((short)(expType as BasicType).GetByteSize()));
                        Emit(Instruction.RET);
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
                        bytecode.Set(jumpOffset, new Short((short)bytecode.Size));
                        CompileStatement(scope, node.ElseBody);
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
                case StatementNode.Type.SwitchStatement://Done
                    {
                        if (scope == null)
                            throw new Exception("Конструкция switch case не должна использоваться в глобальной области видимости");
                        SwitchStatementNode node = statement as SwitchStatementNode;
                        Type type = InferType(scope,node.Expression);
                        bool hasDefault = false;
                        List<int> addresses = new List<int>();
                        foreach (var @case in node.Cases)
                        {
                            if (@case.Value == null)//default case
                            {
                                if (hasDefault)
                                    throw new Exception("В блоке switch должен быть только один вариант по умолчанию");
                                hasDefault = true;
                                continue;
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
            bytecode.AddInstruction(i);
        }
        private void Emit(IValue v)
        {
            bytecode.AddConstant(v);
        }
        private void CompileExpression(Scope scope,ExpressionNode expression)
        {
#if DEBUG || TEST
            Console.Write(expression.ExpressionType.ToString());
#endif
            switch (expression.ExpressionType)
            {
                case ExpressionNode.ExpType.Addition:
                    {
                        AdditionNode _node = expression as AdditionNode;
                        Type type = InferType(scope, expression);
                        if (type is FloatType)
                        {
                            CompileExpression(scope, _node.Left);
                            if (InferType(_node.Left) is IntType)
                                Emit(Instruction.ITOD);
                            CompileExpression(scope, _node.Right);
                            if (InferType(_node.Right) is IntType)
                                Emit(Instruction.ITOD);
                            Emit(Instruction.ADD_F);
                        }
                        else if (type is IntType)
                        {
                            CompileExpression(scope, _node.Left);
                            CompileExpression(scope, _node.Right);
                            Emit(Instruction.ADD_I);
                        }
                        else
                        {
                            throw new Exception("Некорректный тип выражения ");
                        }
                        //scope.AddToStackPointer((-2+1) * (type as BasicType).GetByteSize());
                        scope.AddToStackPointer(-(type as BasicType).GetByteSize());
                        return;
                    }
                case ExpressionNode.ExpType.Subtraction:
                    {
                        SubtractionNode subtractionNode = expression as SubtractionNode;
                    }
                case ExpressionNode.ExpType.Multiplication:
                    {
                        MultiplicationNode divisionNode = expression as MultiplicationNode;
                    }
                case ExpressionNode.ExpType.Division:
                    {
                        DivisionNode divisionNode = expression as DivisionNode;
                    }
                case ExpressionNode.ExpType.Negation:
                    {
                        NegationNode negationNode = expression as NegationNode;
                    }
                case ExpressionNode.ExpType.And:
                    {
                        AndNode andNode = expression as AndNode;
                    }
                case ExpressionNode.ExpType.Or:
                    {
                        OrNode orNode = expression as OrNode;
                    }
                case ExpressionNode.ExpType.Not:
                    {
                        NotNode notNode = expression as NotNode;
                    }
                case ExpressionNode.ExpType.Equal:
                    {
                        EqualNode equalNode = expression as EqualNode;
                    }
                case ExpressionNode.ExpType.NotEqual:
                    {
                        NotEqualNode notEqualNode = expression as NotEqualNode;
                    }
                case ExpressionNode.ExpType.FunctionCall:
                    {
                        FunctionCallNode functionCallNode = expression as FunctionCallNode;
                    }
                case ExpressionNode.ExpType.Assignment:
                    {
                        AssignmentNode asssignmentNode = expression as AssignmentNode;
                    }
                case ExpressionNode.ExpType.ArrayAccess:
                    {
                        ArrayAccessNode arrayAccessNode = expression as ArrayAccessNode;
                    }
                case ExpressionNode.ExpType.FieldExpression:
                    {
                        FieldAccessNode fieldAccessNode = expression as FieldAccessNode;
                    }
                case ExpressionNode.ExpType.TernaryOperator:
                    {
                        TernaryOperatorNode operatorNode = expression as TernaryOperatorNode;
                    }
                case ExpressionNode.ExpType.ArrayInitializerList:
                    {
                        ArrayInitializerListNode arrayInitializer = expression as arrayInitializer;
                    }
                case ExpressionNode.ExpType.StructInitializerList:
                    {
                        StructInitializerListNode structInitializer = expression as StructInitilizerListNode;
                    }
                case ExpressionNode.ExpType.IdentifierExpression:
                    {
                        IdentifierNode identifierNode = expression as IdentifierNode;
                    }
                case ExpressionNode.ExpType.IntConstant:
                    {
                        IntegerNode integerNode = expression as IntegerNode;
                    }
                case ExpressionNode.ExpType.FloatConstant:
                    {
                        FloatNode floatNode = expression as FloatNode;
                    }
                case ExpressionNode.ExpType.BoolConstant:
                    {
                        BoolNode boolNode = expression as BoolNode;
                    }
                case ExpressionNode.ExpType.Cast:
                    {
                        CastNode castNode = expression as CastNode;
                    }
                default:
                    throw new Exception("Ошибка компилятора в Compiler.CompileExpression: неожиданный тип");

            }
        }
    }
#endif
}
#endif