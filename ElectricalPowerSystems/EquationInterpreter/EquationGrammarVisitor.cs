using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.EquationInterpreter
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
        FunctionEntry
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
    public class FunctionEntryNode:ExpressionNode
    {
        public FunctionEntryNode(FunctionEntry entry,List<ExpressionNode> arguments)
        {
            Type = ASTNodeType.FunctionEntry;
            Entry = entry;
            Arguments = arguments;
        }
        public FunctionEntry Entry { get; set; }
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
    public class InitialValueNode:ASTNode
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
        static public FloatNode operator-(FloatNode a)
        {
            a.Value = -a.Value;
            return a;
        }
        static public FloatNode operator -(FloatNode a, FloatNode b)
        {
            return new FloatNode {Value = a.Value-b.Value };
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
    class EquationGrammarVisitor : EquationGrammarBaseVisitor<ASTNode>
    {
        public override ASTNode VisitParameterRule(EquationGrammarParser.ParameterRuleContext context)
        {
            return new ParameterNode
            {
                Identifier = context.id.Text,
                Right = (ExpressionNode)Visit(context.expression()),
                Line = context.start.Line,
                Position = context.start.Column
            };
        }
        public override ASTNode VisitBracketExpression(EquationGrammarParser.BracketExpressionContext context)
        {
            return Visit(context.expression());
        }
        public override ASTNode VisitBinaryOperatorExpression(EquationGrammarParser.BinaryOperatorExpressionContext context)
        {
            InfixExpressionNode node;
            int type = context.op.Type;
            switch (type)
            {
                case EquationGrammarLexer.CARET:
                    node = new PowerNode();
                    break;
                case EquationGrammarLexer.PLUS:
                    node = new AdditionNode();
                    break;
                case EquationGrammarLexer.MINUS:
                    node = new SubtractionNode();
                    break;
                case EquationGrammarLexer.ASTERISK:
                    node = new MultiplicationNode();
                    break;
                case EquationGrammarLexer.DIVISION:
                    node = new DivisionNode();
                    break;
                default:
                    throw new NotSupportedException();
            }
            node.Left = (ExpressionNode)Visit(context.left);
            node.Right = (ExpressionNode)Visit(context.right);
            node.Line = context.start.Line;
            node.Position = context.start.Column;
            return node;
        }
        public override ASTNode VisitFunctionExpression(EquationGrammarParser.FunctionExpressionContext context)
        {
            var functionName = context.func.Text;
            List<ExpressionNode> arguments = new List<ExpressionNode>();
            var args = context.functionArguments();
            foreach (var arg in args.expression())
            {
                arguments.Add((ExpressionNode)Visit(arg));
            }
            return new FunctionNode
            {
                FunctionName = functionName,
                Arguments = arguments,
                Line = context.func.Line,
                Position = context.func.Column
            };
        }
        public override ASTNode VisitUnaryOperatorExpression(EquationGrammarParser.UnaryOperatorExpressionContext context)
        {
            switch (context.unaryOperator().op.Type)
            {
                case EquationGrammarLexer.PLUS:
                    return Visit(context.expression());

                case EquationGrammarLexer.MINUS:
                    return new NegationNode
                    {
                        InnerNode = (ExpressionNode)Visit(context.expression()),
                        Line = context.start.Line,
                        Position = context.start.Column
                    };

                default:
                    throw new NotSupportedException();
            }
        }
        public override ASTNode VisitConstantExpression(EquationGrammarParser.ConstantExpressionContext context)
        {
            return Visit(context.value);
        }
        public override ASTNode VisitInitialValueAssignmentRule(EquationGrammarParser.InitialValueAssignmentRuleContext context)
        {
            return new InitialValueNode
            {
                Identifier=context.id.Text,
                Right= (ExpressionNode)Visit(context.expression()),
                Line = context.start.Line,
                Position = context.start.Column
            };
        }
        public override ASTNode VisitIdentifierExpression(EquationGrammarParser.IdentifierExpressionContext context)
        {
            return new IdentifierNode
            {
                Value = context.id.Text,
                Line = context.start.Line,
                Position = context.start.Column
            };

        }
        public override ASTNode VisitEquationRule(EquationGrammarParser.EquationRuleContext context)
        {
            return new AssignmentNode
            {
                Left = (ExpressionNode)Visit(context.left),
                Right = (ExpressionNode)Visit(context.right),
                Line = context.start.Line,
                Position = context.start.Column
            };
        }
        public override ASTNode VisitNumber(EquationGrammarParser.NumberContext context)
        {
            if (context.value.Type == EquationGrammarLexer.FLOAT)
                return new FloatNode
                {
                    Value = double.Parse(context.value.Text, CultureInfo.InvariantCulture),
                    Line = context.value.Line,
                    Position = context.value.Column
                };
            else
                return new FloatNode
                {
                    Value = int.Parse(context.value.Text),
                    Line = context.value.Line,
                    Position = context.value.Column
                };
        }
        public override ASTNode VisitCompileUnit(EquationGrammarParser.CompileUnitContext context)
        {
            RootNode rootNode=new RootNode();
            rootNode.equations = new List<AssignmentNode>();
            rootNode.initialValues = new List<InitialValueNode>();
            foreach (var statement in context.statement())
            {
                ASTNode node = Visit(statement);
                if (node != null)
                {
                    switch (node.Type)
                    {
                        case ASTNodeType.Assignment:
                            rootNode.equations.Add((AssignmentNode)node);
                            break;
                        case ASTNodeType.InitialValue:
                            rootNode.initialValues.Add((InitialValueNode)node);
                            break;
                        case ASTNodeType.Parameter:
                            rootNode.parameters.Add((ParameterNode)node);
                            break;
                    }
                }
            }
            //return VisitChildren(context);

            return rootNode;
        }
        public override ASTNode VisitStatementRule(EquationGrammarParser.StatementRuleContext context)
        {
            return Visit(context.eq);
        }
        public override ASTNode VisitEmptyStatement(EquationGrammarParser.EmptyStatementContext context)
        {
            return null;
        }
    }
}
