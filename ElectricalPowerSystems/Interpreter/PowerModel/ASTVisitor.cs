﻿//#define MODELINTERPRETER
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace ElectricalPowerSystems.Interpreter.PowerModel
{
#if MODELINTERPRETER
    #region NEW_GRAMMAR

    public class ModelGrammarVisitor : ModelGrammarBaseVisitor<ASTNode>
    {
        public override ASTNode VisitArguments([NotNull] ModelGrammarParser.ArgumentsContext context)
        {
            return base.VisitArguments(context);
        }

        public override ASTNode VisitArray([NotNull] ModelGrammarParser.ArrayContext context)
        {
            return base.VisitArray(context);
        }

        public override ASTNode VisitArrayExpression([NotNull] ModelGrammarParser.ArrayExpressionContext context)
        {
            return base.VisitArrayExpression(context);
        }

        public override ASTNode VisitArrayValues([NotNull] ModelGrammarParser.ArrayValuesContext context)
        {
            return base.VisitArrayValues(context);
        }

        public override ASTNode VisitAssignmentExpression([NotNull] ModelGrammarParser.AssignmentExpressionContext context)
        {
            return base.VisitAssignmentExpression(context);
        }

        public override ASTNode VisitBinaryOperatorExpression([NotNull] ModelGrammarParser.BinaryOperatorExpressionContext context)
        {
            BinaryExpressionNode node;
            switch (context.op.Type)
            {
                case ModelGrammarLexer.DIVISION:
                    node = new DivisionNode();
                    break;
                case ModelGrammarLexer.ASTERISK:
                    node = new MultiplicationNode();
                    break;
                case ModelGrammarLexer.PLUS:
                    node = new AdditionNode();
                    break;
                case ModelGrammarLexer.MINUS:
                    node = new SubtractionNode();
                    break;
                case ModelGrammarLexer.OR:
                    node = new OrNode();
                    break;
                case ModelGrammarLexer.AND:
                    node = new AndNode();
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

        public override ASTNode VisitBoolean([NotNull] ModelGrammarParser.BooleanContext context)
        {
            return new BooleanNode
            {
                Value = context.value.Type ==  ModelGrammarLexer.TRUE?true: false,
                Line = context.value.Line,
                Position = context.value.Column
            };
        }
        public override ASTNode VisitBracketExpression([NotNull] ModelGrammarParser.BracketExpressionContext context)
        {
            return base.VisitBracketExpression(context);
        }
        public override ASTNode VisitComplex([NotNull] ModelGrammarParser.ComplexContext context)
        {
            return new ComplexNode
            {
                Re = 0.0,
                Im = double.Parse(context.im.Text, CultureInfo.InvariantCulture),
                Line = context.im.Line,
                Position = context.im.Column
            };
        }
        public override ASTNode VisitComplexExp([NotNull] ModelGrammarParser.ComplexExpContext context)
        {
            switch (context.type.Type)
            {
                case ModelGrammarLexer.IM:
                    return new ComplexNode
                    {
                        Re = double.Parse(context.left.Text, CultureInfo.InvariantCulture),
                        Im = double.Parse(context.right.Text, CultureInfo.InvariantCulture),
                        Line = context.left.Line,
                        Position = context.left.Column
                    };
                case ModelGrammarLexer.ANGLE:
                    return new ComplexPhaseNode
                    {
                        Magnitude = double.Parse(context.left.Text, CultureInfo.InvariantCulture),
                        Phase = Utils.radians(double.Parse(context.right.Text, CultureInfo.InvariantCulture)),
                        Line = context.left.Line,
                        Position = context.left.Column
                    };
                default:
                    throw new NotSupportedException();
            }
        }
        public override ASTNode VisitConnections([NotNull] ModelGrammarParser.ConnectionsContext context)
        {
            return base.VisitConnections(context);
        }

        public override ASTNode VisitConnectionStatement([NotNull] ModelGrammarParser.ConnectionStatementContext context)
        {
            return base.VisitConnectionStatement(context);
        }

        public override ASTNode VisitConstant([NotNull] ModelGrammarParser.ConstantContext context)
        {
            return base.VisitConstant(context);
        }

        public override ASTNode VisitConstantExpression([NotNull] ModelGrammarParser.ConstantExpressionContext context)
        {
            return base.VisitConstantExpression(context);
        }

        public override ASTNode VisitElements([NotNull] ModelGrammarParser.ElementsContext context)
        {
            return base.VisitElements(context);
        }

        public override ASTNode VisitElementStatement([NotNull] ModelGrammarParser.ElementStatementContext context)
        {
            return base.VisitElementStatement(context);
        }

        public override ASTNode VisitEmptyStatement([NotNull] ModelGrammarParser.EmptyStatementContext context)
        {
            return base.VisitEmptyStatement(context);
        }

        public override ASTNode VisitExpression([NotNull] ModelGrammarParser.ExpressionContext context)
        {
            return base.VisitExpression(context);
        }

        public override ASTNode VisitFieldExpression([NotNull] ModelGrammarParser.FieldExpressionContext context)
        {
            return base.VisitFieldExpression(context);
        }

        public override ASTNode VisitFloat([NotNull] ModelGrammarParser.FloatContext context)
        {
            return new FloatNode
            {
                Value = double.Parse(context.value.Text, CultureInfo.InvariantCulture),
                Line = context.value.Line,
                Position = context.value.Column
            };
        }
        public override ASTNode VisitFunctionArguments([NotNull] ModelGrammarParser.FunctionArgumentsContext context)
        {
            return base.VisitFunctionArguments(context);
        }

        public override ASTNode VisitFunctionExpression([NotNull] ModelGrammarParser.FunctionExpressionContext context)
        {
            return base.VisitFunctionExpression(context);
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

        public override ASTNode VisitInteger([NotNull] ModelGrammarParser.IntegerContext context)
        {
            return new IntNode
            {
                Value = int.Parse(context.value.Text),
                Line = context.value.Line,
                Position = context.value.Column
            };
        }
        public override ASTNode VisitKeyValue([NotNull] ModelGrammarParser.KeyValueContext context)
        {
            return base.VisitKeyValue(context);
        }

        public override ASTNode VisitModel([NotNull] ModelGrammarParser.ModelContext context)
        {
            List<ExpressionNode> statements = new List<ExpressionNode>();
            foreach (var statement in context.statements().statement())
            {
                ASTNode node = VisitStatement(statement);
                if (node != null)
                    statements.Add((ExpressionNode)node);
            }
            List<ElementNode> elements = new List<ElementNode>();
            foreach (var element in context.elements().elementStatement())
            {
                elements.Add((ElementNode)VisitElementStatement(element));
            }
            List<ConnectionNode> connections = new List<ConnectionNode>();
            foreach (var connection in context.connections().connectionStatement())
            {
                connections.Add((ConnectionNode)VisitConnectionStatement(connection));
            }
            List<KeyValueNode> parameters = new List<KeyValueNode>();
            foreach (var parameter in context.arguments().keyValue())
            {
                parameters.Add((KeyValueNode)VisitKeyValue(parameter));
            }
            return new ModelNode
            {
                Statements = statements,
                Elements = elements,
                Connections = connections,
                ModelType = context.type.Text,
                ModelParameters = parameters,
                Line = context.start.Line,
                Position = context.start.Column
            };
        }
        public override ASTNode VisitObject([NotNull] ModelGrammarParser.ObjectContext context)
        {
            List<KeyValueNode> arguments = new List<KeyValueNode>();
            foreach (var argument in context.arguments().keyValue())
            {
                arguments.Add((KeyValueNode)VisitKeyValue(argument));
            }
            return new ObjectNode() {Name = context.name.Text,Arguments = arguments };
        }

        public override ASTNode VisitObjectExpression([NotNull] ModelGrammarParser.ObjectExpressionContext context)
        {
            return VisitObject(context.obj);
        }

        public override ASTNode VisitStatement([NotNull] ModelGrammarParser.StatementContext context)
        {
            return base.VisitStatement(context);
        }

        public override ASTNode VisitStatementRule([NotNull] ModelGrammarParser.StatementRuleContext context)
        {
            return base.VisitStatementRule(context);
        }

        public override ASTNode VisitStatements([NotNull] ModelGrammarParser.StatementsContext context)
        {
            return base.VisitStatements(context);
        }
        public override ASTNode VisitUnaryOperatorExpression([NotNull] ModelGrammarParser.UnaryOperatorExpressionContext context)
        {
            UnaryExpressionNode node;
            switch (context.op.Type)
            {
                case ModelGrammarLexer.NOT:
                    node = new NotNode();
                    break;
                case ModelGrammarLexer.MINUS:
                    node = new NegationNode();
                    break;
                case ModelGrammarLexer.PLUS:
                    return Visit(context.expression());
                default:
                    throw new NotSupportedException();
            }
            node.InnerNode = (ExpressionNode)Visit(context.expression());
            node.Line = context.start.Line;
            node.Position = context.start.Column;
            return node;
        }
    }
    #endregion
#else
#region OLD
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
                        Phase = Utils.Radians(double.Parse(context.right.value.Text, CultureInfo.InvariantCulture)),
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
    #endregion
#endif
}
