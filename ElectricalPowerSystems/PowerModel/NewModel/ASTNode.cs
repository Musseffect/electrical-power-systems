using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerModel.NewModel
{
    public class ASTNode
    {
        public enum NodeType
        {
            Model,
            Expression,
            Element,
            Connection,
            Addition,
            Subtraction,
            Multiplication,
            Division,
            Negation,
            Function,
            //Field,
            Object,
            Array,
            KeyValue,
            Assignment,
            Identifier,
            String,
            Float,
            Integer,
            Boolean,
            Complex,
            ComplexPhase,
            And,
            Or,
            Not,
        }
        public NodeType Type { get; protected set; }
        public int Line { get; set; }
        public int Position { get; set; }
    }
    public class ConnectionNode :ASTNode
    {
        public string Element1;
        public string Node1;
        public string Element2;
        public string Node2;
    }
    public class ElementNode:ASTNode
    {
        public string Id;
        public ObjectNode Definition;
    }
    public class KeyValueNode : ASTNode
    {
        public string Key { get; set; }
        public ExpressionNode Value { get; set; }
    }
    public class ModelNode : ASTNode
    {
        public ModelNode()
        {
            Type = ASTNode.NodeType.Model;
        }
        public ObjectNode ModelParameters { get; set; }
        //public List<KeyValueNode> ModelParameters { get; set; }
        public List<ExpressionNode> Statements { get; set; }
        public List<ElementNode> Elements { get; set; }
        public List<ConnectionNode> Connections { get; set; }
    }
    public class ExpressionNode : ASTNode
    {
    }
    public class ObjectNode : ExpressionNode
    {
        public string Name;
        public List<KeyValueNode> Arguments;
        public ObjectNode()
        {
            Type = NodeType.Object;
        }
    }
    public class AssignmentNode : ExpressionNode
    {
        public AssignmentNode()
        {
            Type = NodeType.Assignment;
        }
        public ExpressionNode Left { get; set; }
        public ExpressionNode Right { get; set; }
    }
    public class ArrayNode : ExpressionNode
    {
        public List<ExpressionNode> Elements;
        public NodeType ArrayType;
        public ArrayNode()
        {
            Type = NodeType.Array;
        }
    }
    public class ComplexNode : ExpressionNode
    {
        public ComplexNode()
        {
            Type = NodeType.Complex;
        }
        public double Re { get; set; }
        public double Im { get; set; }
    }
    public class ComplexPhaseNode : ExpressionNode
    {
        public ComplexPhaseNode()
        {
            Type = NodeType.ComplexPhase;
        }
        public double Magnitude { get; set; }
        public double Phase { get; set; }
    }
    public class IdentifierNode : ExpressionNode
    {
        public IdentifierNode()
        {
            Type = NodeType.Identifier;
        }
        public string Value { get; set; }
    }
    public class FloatNode : ExpressionNode
    {
        public FloatNode()
        {
            Type = NodeType.Float;
        }
        public double Value { get; set; }
    }
    public class IntNode : ExpressionNode
    {
        public IntNode()
        {
            Type = NodeType.Integer;
        }
        public int Value { get; set; }
    }
    public class UnaryExpressionNode : ExpressionNode
    {
        public ExpressionNode InnerNode { get; set; }
    }
    public class BinaryExpressionNode : ExpressionNode
    {
        public ExpressionNode Left { get; set; }
        public ExpressionNode Right { get; set; }
    }
    public class OrNode : BinaryExpressionNode
    {
        public OrNode()
        {
            Type = NodeType.Or;
        }
    }
    public class AndNode : BinaryExpressionNode
    {
        public AndNode()
        {
            Type = NodeType.And;
        }
    }
    public class NotNode : UnaryExpressionNode
    {
        public NotNode()
        {
            Type = NodeType.Not;
        }
    }
    public class FunctionNode : ExpressionNode
    {
        public FunctionNode()
        {
            Type = NodeType.Function;
        }
        public string FunctionName { get; set; }
        public List<ExpressionNode> Arguments { get; set; }
    }
    public class AdditionNode : BinaryExpressionNode
    {
        public AdditionNode()
        {
            Type = NodeType.Addition;
        }
    }
    public class SubtractionNode : BinaryExpressionNode
    {
        public SubtractionNode()
        {
            Type = NodeType.Subtraction;
        }
    }
    public class MultiplicationNode : BinaryExpressionNode
    {
        public MultiplicationNode()
        {
            Type = NodeType.Multiplication;
        }
    }
    public class DivisionNode : BinaryExpressionNode
    {
        public DivisionNode()
        {
            Type = NodeType.Division;
        }
    }
    public class NegationNode : UnaryExpressionNode
    {
        public NegationNode()
        {
            Type = NodeType.Negation;
        }
    }
    public class BooleanNode : ExpressionNode
    {
        public BooleanNode()
        {
            Type = NodeType.Boolean;
        }
        public bool Value { get; set; }
    }
    public class StringNode : ExpressionNode
    {
        public StringNode()
        {
            Type = NodeType.String;
        }
        public string Value { get; set; }
    }
}
