using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Interpreter.Equations
{
    public enum ASTNodeType
    {
        Division,
        Multiplication,
        Negation,
        Addition,
        Subtraction,
        Function,
        Float,
        Identifier,
        Power,
        Assignment,
        InitialValue,
        Parameter,
        Root,
        Derivative
    }
    public class ASTNode
    {
        public ASTNodeType Type { get; protected set; }
        public int Line { get; set; }
        public int Position { get; set; }
    }
    public class ExpressionNode : ASTNode
    {
    }
    public class InfixExpressionNode : ExpressionNode
    {
        public ExpressionNode Left { get; set; }
        public ExpressionNode Right { get; set; }
    }
    public class DerivativeNode : ExpressionNode
    {
        public string Identifier { get; set; }
        public DerivativeNode()
        {
            Type = ASTNodeType.Derivative;
        }
    }
    public class AdditionNode : InfixExpressionNode
    {
        public AdditionNode()
        {
            Type = ASTNodeType.Addition;
        }
    }
    public class AssignmentNode : InfixExpressionNode
    {
        public AssignmentNode()
        {
            Type = ASTNodeType.Assignment;
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
    public class FunctionNode : ExpressionNode
    {
        public FunctionNode()
        {
            Type = ASTNodeType.Function;
        }
        public string FunctionName { get; set; }
        public List<ExpressionNode> Arguments { get; set; }
    }
    public class IdentifierNode : ExpressionNode
    {
        public IdentifierNode()
        {
            Type = ASTNodeType.Identifier;
        }
        public string Value { get; set; }
    }
    public class InitialValueNode : ASTNode
    {
        public InitialValueNode()
        {
            Type = ASTNodeType.InitialValue;
        }
        public string Identifier { get; set; }
        public ExpressionNode Right { get; set; }
    }
    public class RootNode : ASTNode
    {
        public RootNode()
        {
            Type = ASTNodeType.Root;
            equations = new List<AssignmentNode>();
            initialValues = new List<InitialValueNode>();
            parameters = new List<ParameterNode>();
        }
        public List<AssignmentNode> equations;
        public List<InitialValueNode> initialValues;
        public List<ParameterNode> parameters;
    }
    public class ParameterNode : ASTNode
    {
        public ParameterNode()
        {
            Type = ASTNodeType.Parameter;
        }
        public string Identifier { get; set; }
        public ExpressionNode Right { get; set; }
    }
    public class FloatNode : ExpressionNode
    {
        public FloatNode()
        {
            Type = ASTNodeType.Float;
        }
        public bool isOne()
        {
            return Value == 1.0;
        }
        public bool isZero()
        {
            return Value == 0.0;
        }
        static public FloatNode operator -(FloatNode a)
        {
            a.Value = -a.Value;
            return a;
        }
        static public FloatNode operator -(FloatNode a, FloatNode b)
        {
            return new FloatNode { Value = a.Value - b.Value };
        }
        static public FloatNode operator +(FloatNode a, FloatNode b)
        {
            return new FloatNode { Value = a.Value + b.Value };
        }
        static public FloatNode operator *(FloatNode a, FloatNode b)
        {
            return new FloatNode { Value = a.Value * b.Value };
        }
        static public FloatNode operator /(FloatNode a, FloatNode b)
        {
            return new FloatNode { Value = a.Value / b.Value };
        }
        static public FloatNode pow(FloatNode a, FloatNode b)
        {
            return new FloatNode { Value = Math.Pow(a.Value, b.Value) };
        }
        public double Value { get; set; }
    }
}
