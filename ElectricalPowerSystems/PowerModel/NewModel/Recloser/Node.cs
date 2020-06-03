using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerModel.NewModel.Recloser
{
    public abstract class Node
    {
        public int Line { get; set; }
        public int Position { get; set; }
        public Node()
        {
        }
    }
    public class ProgramNode : Node
    {
        public List<StatementNode> Statements;
        public ProgramNode()
        {
        }
    }
    public class VariableNode : Node
    {
        public string Name;
        /// <summary>
        /// Null if not initialized
        /// </summary>
        public ExpressionNode Initializer;
        public VariableNode()
        {
        }
    }
    public class SignatureArgumentNode : Node
    {
        public enum WayType
        {
            In,
            InOut
        };
        public string Name;
        public WayType Way;
        public TypeNode ArgumentType;

        public SignatureArgumentNode() : base()
        {
        }
    }
    public class TypeNode : Node
    {
        public string Typename;
        public IntegerNode Size;
        public TypeNode()
        {
        }
    }
    public class StructFieldNode : Node
    {
        public string Name;
        public TypeNode FieldType;
        public StructFieldNode()
        {
        }
    }
    public class CaseNode : Node
    {
        public ConstantNode Value;
        public List<StatementNode> Body;
        public CaseNode()
        {
        }
    }
    #region Statements
    public abstract class StatementNode : Node
    {
        public enum Type
        {
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
            ReturnStatement
        };
        public Type StatementType;
        public StatementNode(Type type)
        {
            StatementType = type;
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
    public class IfElseNode : StatementNode
    {
        public ExpressionNode Condition;
        public StatementNode IfBody;
        public StatementNode ElseBody;
        public IfElseNode() : base(Type.IfElseStatement)
        {
        }
    }
    public class WhileNode : StatementNode
    {
        public ExpressionNode Condition;
        public StatementNode Body;
        public WhileNode() : base(Type.WhileStatement)
        {
        }
    }
    public class StructDefinitionNode : StatementNode
    {
        public string Name;
        public List<StructFieldNode> Fields;
        public StructDefinitionNode() : base(Type.StructDefinition)
        {
        }
    }
    public class SwitchStatementNode : StatementNode
    {
        public ExpressionNode Expression;
        public List<CaseNode> Cases;
        public SwitchStatementNode() : base(Type.SwitchStatement)
        {
        }
    }
    public class FunctionDefinitionNode : StatementNode
    {
        public TypeNode ReturnType;
        public string Name;
        public List<SignatureArgumentNode> Signature;
        public List<StatementNode> Body;
        public int Start { get; set; }
        public int Stop { get; set; }
        public FunctionDefinitionNode() : base(Type.FunctionDefinition)
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
    public class ReturnNode : StatementNode
    {
        public ExpressionNode Expression;
        public ReturnNode() : base(Type.ReturnStatement)
        {
        }
    }
    public class ScopeNode : StatementNode
    {
        public List<StatementNode> Body;
        public ScopeNode() : base(Type.ScopeStatement)
        {
        }
    }
    public class VariableDeclarationNode : StatementNode
    {
        public TypeNode VariableType;
        public List<VariableNode> Variables;
        public VariableDeclarationNode() : base(Type.VariableDeclarationStatement)
        {
        }
    }
    #endregion


    #region Expressions
    public abstract class BinaryOperatorNode : ExpressionNode
    {
        public ExpressionNode Left;
        public ExpressionNode Right;
        public BinaryOperatorNode(ExpType type) : base(type)
        {
        }
    }
    public class AdditionNode : BinaryOperatorNode
    {
        public AdditionNode() : base(ExpType.Addition)
        {
        }
    }
    public class SubtractionNode : BinaryOperatorNode
    {
        public SubtractionNode() : base(ExpType.Subtraction)
        {
        }
    }
    public class MultiplicationNode : BinaryOperatorNode
    {
        public MultiplicationNode() : base(ExpType.Multiplication)
        {
        }
    }
    public class DivisionNode : BinaryOperatorNode
    {
        public DivisionNode() : base(ExpType.Division)
        {
        }
    }
    public class EqualNode : BinaryOperatorNode
    {
        public EqualNode() : base(ExpType.Equal)
        {
        }
    }
    public class NotEqualNode : BinaryOperatorNode
    {
        public NotEqualNode() : base(ExpType.NotEqual)
        {
        }
    }
    public class AndNode : BinaryOperatorNode
    {
        public AndNode() : base(ExpType.And)
        {
        }
    }
    public class OrNode : BinaryOperatorNode
    {
        public OrNode() : base(ExpType.Or)
        {
        }
    }
    public abstract class UnaryOperatorNode : ExpressionNode
    {
        public ExpressionNode Inner;
        public UnaryOperatorNode(ExpType type) : base(type)
        {
        }
    }
    public class NegationNode : UnaryOperatorNode
    {
        public NegationNode() : base(ExpType.Negation)
        {
        }
    }
    public class NotNode : UnaryOperatorNode
    {
        public NotNode() : base(ExpType.Not)
        {
        }
    }
    public class PreIncrement : UnaryOperatorNode
    {
        public PreIncrement() : base(ExpType.PreIncrement)
        {
        }
    }
    public class PreDecrement : UnaryOperatorNode
    {
        public PreDecrement() : base(ExpType.PreDecrement)
        {
        }
    }
    public class GreaterNode : BinaryOperatorNode
    {
        public GreaterNode() : base(ExpType.Greater)
        {
        }
    }
    public class GreaterEqualNode : BinaryOperatorNode
    {
        public GreaterEqualNode() : base(ExpType.GreaterEqual)
        {
        }
    }
    public class LessNode : BinaryOperatorNode
    {
        public LessNode() : base(ExpType.Less)
        {
        }
    }
    public class LessEqualNode : BinaryOperatorNode
    {
        public LessEqualNode() : base(ExpType.LessEqual)
        {
        }
    }
    public class AssignmentNode : BinaryOperatorNode
    {
        public AssignmentNode() : base(ExpType.Assignment)
        {
        }
    }
    public class PostIncrement : UnaryOperatorNode
    {
        public PostIncrement() : base(ExpType.PostIncrement)
        {
        }
    }
    public class PostDecrement : UnaryOperatorNode
    {
        public PostDecrement() : base(ExpType.PostDecrement)
        {
        }
    }
    public abstract class ExpressionNode : StatementNode
    {
        public enum ExpType
        {
            PreIncrement,
            PostIncrement,
            PreDecrement,
            PostDecrement,
            Equal,
            NotEqual,
            Greater,
            GreaterEqual,
            Less,
            LessEqual,
            And,
            Or,
            Not,
            Negation,
            TernaryOperator,
            /*SelfAddition,
            SelfSubtraction,
            SelfMultilication,
            SelfDivision,*/
            Cast,
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
            BoolConstant
        };
        public ExpType ExpressionType;
        public ExpressionNode(ExpType type) : base(Type.ExpressionStatement)
        {
            ExpressionType = type;
        }
    }
    public abstract class ConstantNode : ExpressionNode
    {
        public ConstantNode(ExpType type) : base(type)
        {
        }
    }
    public class CastNode : ExpressionNode
    {
        public string TypeName;
        public ExpressionNode Exp;
        public CastNode() : base(ExpType.Cast)
        {
        }
    }
    public class ArrayInitializerList : ExpressionNode
    {
        public string TypeName;
        public int Size;
        public List<ExpressionNode> Elements;
        public ArrayInitializerList() : base(ExpType.ArrayInitializerList)
        {
        }
    }
    public class StructInitializerList:ExpressionNode
    {
        public string StructName;
        public List<ExpressionNode> Elements;
        public StructInitializerList() : base(ExpType.StructInitializerList)
        {
        }
    }
    public class IntegerNode : ConstantNode
    {
        public int Value { get; set; }
        public IntegerNode() : base(ExpType.IntConstant)
        {

        }
    }
    public class FloatNode : ConstantNode
    {
        public double Value { get; set; }
        public FloatNode() : base(ExpType.FloatConstant)
        {

        }
    }
    public class BoolNode : ConstantNode
    {
        public bool Value { get; set; }
        public BoolNode() : base(ExpType.BoolConstant)
        {

        }
    }
    public class IdentifierNode : ExpressionNode
    {
        public string Identifier { get; set; }
        public IdentifierNode():base(ExpType.IdentifierExpression)
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
    public class FunctionCallNode : ExpressionNode
    {
        public string Name;
        public List<ExpressionNode> Arguments;
        public FunctionCallNode() : base(ExpType.FunctionCall)
        {
        }
    }
    public class FieldAccessNode : ExpressionNode
    {
        public ExpressionNode Parent;
        public string Field;
        public FieldAccessNode() : base(ExpType.FieldExpression)
        {
        }
    }
    public class ArrayAccessNode : ExpressionNode
    {
        public ExpressionNode Array;
        public ExpressionNode Index;
        public ArrayAccessNode() : base(ExpType.ArrayAccess)
        {
        }
    }
    public class TernaryOperatorNode : ExpressionNode
    {
        public ExpressionNode Condition;
        public ExpressionNode First;
        public ExpressionNode Second;
        public TernaryOperatorNode() : base(ExpType.TernaryOperator)
        {
        }
    }
    #endregion
}
