using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerModel.NewModel.Recloser
{
    public abstract class Node
    {
        public enum Type
        {
            Program,
            Type,
            SignatureArgument,
            StructField,
            Case,


            Statement,
            FunctionDefinition,
            FunctionDeclaration,
            FunctionSignatureArgument,
            StructDefinition,
            VariableDeclarationStatement,
            ExpressionStatement,
            ScopeStatement,
            BreakStatement,
            ContinueStatement,
            WhileStatement,
            SwitchStatement,
            IfStatement,
            IfElseStatement,
            ReturnStatement,

            Expression,
            PreIncrementDecrement,
            PostIncrementDecrement,
            Equal,
            And,
            Or,
            Not,
            Negation,
            TernaryOperator,
            SelfAddition,
            SelfSubtraction,
            SelfMultilication,
            SelfDivision,
            Addition,
            Subtraction,
            Multiplication,
            Division,
            FieldExpression,
            Assignment,
            FunctionCall,
            ArrayAccess,
            IdentifierExpression,
            ArrayInitializerList,
            StructInitializerList,


            FloatConstant,
            IntConstant,
            BoolConstant,
            StringConstant,
        }
        Type type;
        public int Line { get; set; }
        public int Position { get; set; }
        public Type NodeType { get { return type; } }
        public Node(Type type)
        {
            this.type = type;
        }
    }
    public class ProgramNode : Node
    {
        List<StatementNode> statements;
        public ProgramNode(List<StatementNode> statements) : base(Type.Program)
        {
            this.statements = statements;
        }
    }
    public abstract class StatementNode : Node
    {
        public StatementNode(Type type) : base(type)
        {
        }
    }

    public abstract class ConstantNode : ExpressionNode
    {
        public ConstantNode(Type type) : base(type)
        {
        }
    }
    public class IntegerNode : ConstantNode
    {
        public int Value { get; set; }
        public IntegerNode() : base(Type.IntConstant)
        {

        }
    }
    public class FloatNode : ConstantNode
    {
        public double Value { get; set; }
        public FloatNode() : base(Type.FloatConstant)
        {

        }
    }
    public class BoolNode : ConstantNode
    {
        public bool Value { get; set; }
        public BoolNode() : base(Type.BoolConstant)
        {

        }
    }
    public class IdentifierNode : ExpressionNode
    {
        public string Identifier { get; set; }
        public IdentifierNode():base(Type.IdentifierExpression)
        {
        }
    }
    public class BreakNode : StatementNode
    {
        public BreakNode() : base(Type.BreakStatement)
        {
        }
    }
    public class ContinueNode : StatementNode
    {
        public ContinueNode() : base(Type.ContinueStatement)
        {
        }
    }
    public abstract class ExpressionNode : StatementNode
    {
        public ExpressionNode(Type type) : base(type)
        {

        }
    }
    public class IfNode : StatementNode
    {
        public ExpressionNode Condition;
        public StatementNode IfBody;
        public IfNode() : base(Type.IfStatement)
        {
        }
    }
    public class IfElseNode:StatementNode
    {
        public ExpressionNode Condition;
        public StatementNode IfBody;
        public StatementNode ElseBody;
        public IfElseNode() : base(Type.IfElseStatement)
        {
        }
    }   
    public class WhileNode: StatementNode
    {
        public ExpressionNode Condition;
        public StatementNode Body;
        public WhileNode():base(Type.WhileStatement)
        {
        }
    }
    public class SignatureArgumentNode:Node
    {
        public enum WayType {
            In,
            InOut
        };
        public string Name;
        public WayType Way;
        public TypeNode ArgumentType;

        public SignatureArgumentNode():base(Type.SignatureArgument)
        {
        }
    }
    public class FunctionCallNode : ExpressionNode
    {
        public string Name;
        public List<ExpressionNode> Arguments;
        public FunctionCallNode() : base(Type.FunctionCall)
        {
        }
    }
    public class TypeNode:Node
    {
        public string Typename;
        public IntegerNode Size;
        public TypeNode() : base(Type.Type)
        {
        }
    }
    public class StructFieldNode : Node
    {
        public string Name;
        public TypeNode FieldType;
        public StructFieldNode() : base(Type.StructField)
        {
        }
    }
    public class StructDefinitionNode : Node
    {
        public string Name;
        public List<StructFieldNode> Fields;
        public StructDefinitionNode() : base(Type.StructDefinition)
        {
        }
    }

    public class SwitchStatementNode:Node
    {
        public ExpressionNode Expression;
        public List<CaseNode> Cases;
        public SwitchStatementNode():base(Type.SwitchStatement)
        {
        }
    }
    public class CaseNode : Node
    {
        public ConstantNode Value;
        public List<StatementNode> Body;
        public CaseNode() : base(Type.Case)
        {
        }
    }
    public class FunctionDefinitionNode : StatementNode
    {
        public TypeNode ReturnType;
        public string Name;
        public List<SignatureArgumentNode> Signature;
        public List<StatementNode> Body;
        public FunctionDefinitionNode():base(Type.FunctionDefinition)
        {

        }
    }
    public class FunctionDeclarationNode : StatementNode
    {
        public TypeNode ReturnType;
        public string Name;
        public List<SignatureArgumentNode> Signature;
        public FunctionDeclarationNode() : base(Type.FunctionDeclaration)
        {
        }
    }

    public class FieldAccessNode : ExpressionNode
    {
        public ExpressionNode Parent;
        public string Field;
        public FieldAccessNode():base(Type.FieldExpression)
        {
        }
    }
    public class ReturnNode : StatementNode
    {
        public ExpressionNode Expression;
        public ReturnNode() : base(Type.ReturnStatement)
        {
        }
    }
    public class ArrayAccessNode : ExpressionNode
    {
        public ExpressionNode Array;
        public ExpressionNode Index;
        public ArrayAccessNode() : base(Type.ArrayAccess)
        {
        }
    }
    public class ScopeNode:StatementNode
    {
        public List<StatementNode> Body;
        public ScopeNode() : base(Type.ScopeStatement)
        {
        }
    }

    public class TernaryOperatorNode:ExpressionNode
    {
        public ExpressionNode Condition;
        public ExpressionNode First;
        public ExpressionNode Second;
        public TernaryOperatorNode():base(Type.TernaryOperator)
        {
        }
    }
}
