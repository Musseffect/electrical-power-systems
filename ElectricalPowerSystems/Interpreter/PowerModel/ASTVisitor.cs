using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace ElectricalPowerSystems.Interpreter
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
    class ASTVisitor : ModelGrammarBaseVisitor<ASTNode>
    {
        public override ASTNode VisitModel([NotNull] ModelGrammarParser.ModelContext context)
        {
            List<StatementNode> statements = new List<StatementNode>();
            foreach(var statement in context.statement())
            {
                ASTNode node = Visit(statement);
                if (node != null)
                    statements.Add((StatementNode)node);
            }
            return new ModelNode
            {
                Statements = statements,
                Line = context.start.Line,
                Position = context.start.Column
            };
        }
        public override ASTNode VisitBracketExpression([NotNull] ModelGrammarParser.BracketExpressionContext context)
        {
            return Visit(context.expression());
        }
        public override ASTNode VisitAssignmentExpression([NotNull] ModelGrammarParser.AssignmentExpressionContext context)
        {
            return new AssignmentNode
            {
                Left = (ExpressionNode)Visit(context.lvalue),
                Right = (ExpressionNode)Visit(context.rvalue),
                Line = context.start.Line,
                Position = context.start.Column
            };
        }
        public override ASTNode VisitBinaryOperatorExpression([NotNull] ModelGrammarParser.BinaryOperatorExpressionContext context)
        {
            InfixExpressionNode node;
            int type = context.op.Type;
            switch (type)
            {
                case ModelGrammarLexer.CARET:
                    node = new PowerNode();
                    break;
                case ModelGrammarLexer.PLUS:
                    node = new AdditionNode();
                    break;
                case ModelGrammarLexer.MINUS:
                    node = new SubtractionNode();
                    break;
                case ModelGrammarLexer.ASTERISK:
                    node = new MultiplicationNode();
                    break;
                case ModelGrammarLexer.DIVISION:
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
        public override ASTNode VisitStatementRule([NotNull] ModelGrammarParser.StatementRuleContext context)
        {
            return new StatementNode
            {
                Expression = (ExpressionNode)Visit(context.expression()),
                Line = context.start.Line,
                Position = context.start.Column
            };
        }
        public override ASTNode VisitEmptyStatement([NotNull] ModelGrammarParser.EmptyStatementContext context)
        {
            return new StatementNode
            {
                Expression = null,
            };
        }
        public override ASTNode VisitStringConstant([NotNull] ModelGrammarParser.StringConstantContext context)
        {
            return new StringNode
            {
                Value = context.value.Text.Substring(1,context.value.Text.Length-2),
                Line = context.value.Line,
                Position = context.value.Column
            };
        }
        public override ASTNode VisitNumberConstant([NotNull] ModelGrammarParser.NumberConstantContext context)
        {
            if (context.value.value.Type == ModelGrammarLexer.FLOAT)
                return new FloatNode
                {
                    Value = double.Parse(context.value.value.Text, CultureInfo.InvariantCulture),
                    Line = context.value.value.Line,
                    Position = context.value.value.Column
                };
            else
                return new IntNode
                {
                    Value = int.Parse(context.value.value.Text),
                    Line = context.value.value.Line,
                    Position = context.value.value.Column
                };
        }
        public override ASTNode VisitFieldExpression([NotNull] ModelGrammarParser.FieldExpressionContext context)
        {
            return new MemberNode
            {
                Left = (ExpressionNode)Visit(context.left),
                MemberId = new IdentifierNode
                {
                    Value = context.id.Text,
                    Line = context.id.Line,
                    Position = context.id.Column
                },
                Line = context.start.Line,
                Position = context.start.Column
            };
        }
        public override ASTNode VisitCastExpression([NotNull] ModelGrammarParser.CastExpressionContext context)
        {
            return new CastNode
            {
                CastType = new IdentifierNode
                {
                    Value = context.id.Text,
                    Line = context.id.Line,
                    Position = context.id.Column
                },
                Right = (ExpressionNode)Visit(context.right),
                Line = context.start.Line,
                Position = context.start.Column
            };
        }
        public override ASTNode VisitComplex([NotNull] ModelGrammarParser.ComplexContext context)
        {
            return new ComplexNode
            {
                Re = 0.0,
                Im = double.Parse(context.im.value.Text, CultureInfo.InvariantCulture),
                Line = context.im.value.Line,
                Position = context.im.value.Column
            };
        }
        public override ASTNode VisitComplexExp([NotNull] ModelGrammarParser.ComplexExpContext context)
        {
            switch (context.type.Type)
            {
                case ModelGrammarLexer.IM:
                    return new ComplexNode
                    {
                        Re = double.Parse(context.left.value.Text, CultureInfo.InvariantCulture),
                        Im = double.Parse(context.right.value.Text, CultureInfo.InvariantCulture),
                        Line=context.left.value.Line,
                        Position=context.left.value.Column
                    };
                case ModelGrammarLexer.ANGLE:
                    return new ComplexPhaseNode
                    {
                        Magnitude = double.Parse(context.left.value.Text, CultureInfo.InvariantCulture),
                        Phase = Utils.radians(double.Parse(context.right.value.Text, CultureInfo.InvariantCulture)),
                        Line = context.left.value.Line,
                        Position = context.left.value.Column
                    };
                default:
                    throw new NotSupportedException();
            }
        }
        public override ASTNode VisitComplexConstant([NotNull] ModelGrammarParser.ComplexConstantContext context)
        {
            return VisitComplex(context.value);
        }
        public override ASTNode VisitComplexExprConstant([NotNull] ModelGrammarParser.ComplexExprConstantContext context)
        {
            return VisitComplexExp(context.value);
        }
        public override ASTNode VisitConstantExpression([NotNull] ModelGrammarParser.ConstantExpressionContext context)
        {
            return Visit(context.value);
        }
        public override ASTNode VisitFunctionExpression([NotNull] ModelGrammarParser.FunctionExpressionContext context)
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
                Line=context.func.Line,
                Position=context.func.Column
            };
        }
        public override ASTNode VisitUnaryOperatorExpression([NotNull] ModelGrammarParser.UnaryOperatorExpressionContext context)
        {
            switch (context.unaryOperator().op.Type)
            {
                case ModelGrammarLexer.PLUS:
                    return Visit(context.expression());

                case ModelGrammarLexer.MINUS:
                    return new NegationNode
                    {
                        InnerNode = (ExpressionNode)Visit(context.expression()),
                        Line=context.start.Line,
                        Position=context.start.Column
                    };

                default:
                    throw new NotSupportedException();
            }
        }
        public override ASTNode VisitIdentifierExpression([NotNull] ModelGrammarParser.IdentifierExpressionContext context)
        {
            return new IdentifierNode
            {
                Value = context.id.Text,
                Line = context.id.Line,
                Position = context.id.Column
            };
        }
    }
}
