using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerModel.OldModel
{
    class ASTVisitor : OldGrammarBaseVisitor<ASTNode>
    {
        public override ASTNode VisitModel([NotNull] OldGrammarParser.ModelContext context)
        {
            List<StatementNode> statements = new List<StatementNode>();
            foreach (var statement in context.statement())
            {
                ASTNode node = Visit(statement);
                statements.Add((StatementNode)node);
            }
            return new ModelNode
            {
                Statements = statements,
                Line = context.start.Line,
                Position = context.start.Column
            };
        }
        public override ASTNode VisitBracketExpression([NotNull] OldGrammarParser.BracketExpressionContext context)
        {
            return Visit(context.expression());
        }
        public override ASTNode VisitAssignmentExpression([NotNull] OldGrammarParser.AssignmentExpressionContext context)
        {
            return new AssignmentNode
            {
                Left = (ExpressionNode)Visit(context.lvalue),
                Right = (ExpressionNode)Visit(context.rvalue),
                Line = context.start.Line,
                Position = context.start.Column
            };
        }
        public override ASTNode VisitBinaryOperatorExpression([NotNull] OldGrammarParser.BinaryOperatorExpressionContext context)
        {
            InfixExpressionNode node;
            int type = context.op.Type;
            switch (type)
            {
                case OldGrammarLexer.CARET:
                    node = new PowerNode();
                    break;
                case OldGrammarLexer.PLUS:
                    node = new AdditionNode();
                    break;
                case OldGrammarLexer.MINUS:
                    node = new SubtractionNode();
                    break;
                case OldGrammarLexer.ASTERISK:
                    node = new MultiplicationNode();
                    break;
                case OldGrammarLexer.DIVISION:
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
        public override ASTNode VisitStatement([NotNull] OldGrammarParser.StatementContext context)
        {
            return new StatementNode
            {
                Expression = (ExpressionNode)Visit(context.expression()),
                Line = context.start.Line,
                Position = context.start.Column
            };
        }
        public override ASTNode VisitStringConstant([NotNull] OldGrammarParser.StringConstantContext context)
        {
            return new StringNode
            {
                Value = context.value.Text.Substring(1, context.value.Text.Length - 2),
                Line = context.value.Line,
                Position = context.value.Column
            };
        }
        public override ASTNode VisitNumberConstant([NotNull] OldGrammarParser.NumberConstantContext context)
        {
            if (context.value.value.Type == OldGrammarLexer.FLOAT)
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
        public override ASTNode VisitFieldExpression([NotNull] OldGrammarParser.FieldExpressionContext context)
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
        public override ASTNode VisitCastExpression([NotNull] OldGrammarParser.CastExpressionContext context)
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
        public override ASTNode VisitComplex([NotNull] OldGrammarParser.ComplexContext context)
        {
            return new ComplexNode
            {
                Re = 0.0,
                Im = double.Parse(context.im.value.Text, CultureInfo.InvariantCulture),
                Line = context.im.value.Line,
                Position = context.im.value.Column
            };
        }
        public override ASTNode VisitComplexExp([NotNull] OldGrammarParser.ComplexExpContext context)
        {
            switch (context.type.Type)
            {
                case OldGrammarLexer.IM:
                    return new ComplexNode
                    {
                        Re = double.Parse(context.left.value.Text, CultureInfo.InvariantCulture),
                        Im = double.Parse(context.right.value.Text, CultureInfo.InvariantCulture),
                        Line = context.left.value.Line,
                        Position = context.left.value.Column
                    };
                case OldGrammarLexer.ANGLE:
                    return new ComplexPhaseNode
                    {
                        Magnitude = double.Parse(context.left.value.Text, CultureInfo.InvariantCulture),
                        Phase = MathUtils.MathUtils.Radians(double.Parse(context.right.value.Text, CultureInfo.InvariantCulture)),
                        Line = context.left.value.Line,
                        Position = context.left.value.Column
                    };
                default:
                    throw new NotSupportedException();
            }
        }
        public override ASTNode VisitComplexConstant([NotNull] OldGrammarParser.ComplexConstantContext context)
        {
            return VisitComplex(context.value);
        }
        public override ASTNode VisitComplexExprConstant([NotNull] OldGrammarParser.ComplexExprConstantContext context)
        {
            return VisitComplexExp(context.value);
        }
        public override ASTNode VisitConstantExpression([NotNull] OldGrammarParser.ConstantExpressionContext context)
        {
            return Visit(context.value);
        }
        public override ASTNode VisitFunctionExpression([NotNull] OldGrammarParser.FunctionExpressionContext context)
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
        public override ASTNode VisitUnaryOperatorExpression([NotNull] OldGrammarParser.UnaryOperatorExpressionContext context)
        {
            switch (context.unaryOperator().op.Type)
            {
                case OldGrammarLexer.PLUS:
                    return Visit(context.expression());

                case OldGrammarLexer.MINUS:
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
        public override ASTNode VisitIdentifierExpression([NotNull] OldGrammarParser.IdentifierExpressionContext context)
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
