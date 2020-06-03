using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerModel.OldModel
{
    public enum ASTNodeType
    {
        Model,
        Statement,
        Assignment,
        Division,
        Multiplication,
        Negation,
        Member,
        Cast,
        Addition,
        Subtraction,
        Function,
        Float,
        Int,
        String,
        Complex,
        ComplexPhase,
        Identifier,
        Power
    }
    public class ASTNode
    {
        public ASTNodeType Type { get; protected set; }
        public int Line { get; set; }
        public int Position { get; set; }
    }
    public class StatementNode : ASTNode
    {
        public StatementNode()
        {
            Type = ASTNodeType.Statement;
        }
        public ExpressionNode Expression { get; set; }
    }
    public class ModelNode : ASTNode
    {
        public ModelNode()
        {
            Type = ASTNodeType.Model;
        }
        public List<StatementNode> Statements { get; set; }
    }
    public class ExpressionNode : ASTNode
    {
    }
    public class InfixExpressionNode : ExpressionNode
    {
        public ExpressionNode Left { get; set; }
        public ExpressionNode Right { get; set; }
    }
    public class AdditionNode : InfixExpressionNode
    {
        public AdditionNode()
        {
            Type = ASTNodeType.Addition;
        }
    }
    public class SubtractionNode : InfixExpressionNode
    {
        public SubtractionNode()
        {
            Type = ASTNodeType.Subtraction;
        }
    }
    public class MultiplicationNode : InfixExpressionNode
    {
        public MultiplicationNode()
        {
            Type = ASTNodeType.Multiplication;
        }
    }
    public class DivisionNode : InfixExpressionNode
    {
        public DivisionNode()
        {
            Type = ASTNodeType.Division;
        }
    }
    public class AssignmentNode : ExpressionNode
    {
        public AssignmentNode()
        {
            Type = ASTNodeType.Assignment;
        }
        public ExpressionNode Left { get; set; }
        public ExpressionNode Right { get; set; }
    }
    public class MemberNode : ExpressionNode
    {
        public MemberNode()
        {
            Type = ASTNodeType.Member;
        }
        public ExpressionNode Left { get; set; }
        public IdentifierNode MemberId { get; set; }
    }
    public class CastNode : ExpressionNode
    {
        public CastNode()
        {
            Type = ASTNodeType.Cast;
        }
        public IdentifierNode CastType { get; set; }
        public ExpressionNode Right { get; set; }
    }
    public class PowerNode : InfixExpressionNode
    {
        public PowerNode()
        {
            Type = ASTNodeType.Power;
        }
    }
    public class NegationNode : ExpressionNode
    {
        public NegationNode()
        {
            Type = ASTNodeType.Negation;
        }
        public ExpressionNode InnerNode { get; set; }
    }
    public class StringNode : ExpressionNode
    {
        public StringNode()
        {
            Type = ASTNodeType.String;
        }
        public string Value { get; set; }
    }
    public class FunctionNode : ExpressionNode
    {
        public FunctionNode()
        {
            Type = ASTNodeType.Function;
        }
        public string FunctionName { get; set; }
        public List<ExpressionNode> Arguments { get; set; }
    }
    public class ComplexPhaseNode : ExpressionNode
    {
        public ComplexPhaseNode()
        {
            Type = ASTNodeType.ComplexPhase;
        }
        public double Magnitude { get; set; }
        public double Phase { get; set; }
    }
    public class ComplexNode : ExpressionNode
    {
        public ComplexNode()
        {
            Type = ASTNodeType.Complex;
        }
        public double Re { get; set; }
        public double Im { get; set; }
    }
    public class IdentifierNode : ExpressionNode
    {
        public IdentifierNode()
        {
            Type = ASTNodeType.Identifier;
        }
        public string Value { get; set; }
    }
    public class FloatNode : ExpressionNode
    {
        public FloatNode()
        {
            Type = ASTNodeType.Float;
        }
        public double Value { get; set; }
    }
    public class IntNode : ExpressionNode
    {
        public IntNode()
        {
            Type = ASTNodeType.Int;
        }
        public int Value { get; set; }
    }
}
