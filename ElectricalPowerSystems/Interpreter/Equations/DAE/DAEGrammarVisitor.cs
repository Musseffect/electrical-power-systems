using Antlr4.Runtime.Misc;
using ElectricalPowerSystems.Interpreter.Equations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Interpreter.Equations.DAE
{
    class DAEImplicitGrammarVisitor : DAEImplicitGrammarBaseVisitor<ASTNode>
    {
        public override ASTNode VisitBracketExpression([NotNull] DAEImplicitGrammarParser.BracketExpressionContext context) {
            return Visit(context.expression());
        }
        public override ASTNode VisitDerivativeExpression([NotNull] DAEImplicitGrammarParser.DerivativeExpressionContext context) {

            return new DerivativeNode
            {
                Identifier = context.id.Text,
                Line = context.start.Line,
                Position = context.start.Column
            };
        }
        public override ASTNode VisitBinaryOperatorExpression([NotNull] DAEImplicitGrammarParser.BinaryOperatorExpressionContext context)
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
        public override ASTNode VisitFunctionExpression([NotNull] DAEImplicitGrammarParser.FunctionExpressionContext context)
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
        public override ASTNode VisitUnaryOperatorExpression([NotNull] DAEImplicitGrammarParser.UnaryOperatorExpressionContext context)
        {
            switch (context.unaryOperator().op.Type)
            {
                case DAEImplicitGrammarLexer.PLUS:
                    return Visit(context.expression());

                case DAEImplicitGrammarLexer.MINUS:
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
        public override ASTNode VisitConstantExpression([NotNull] DAEImplicitGrammarParser.ConstantExpressionContext context)
        {
            return Visit(context.value);
        }
        public override ASTNode VisitIdentifierExpression([NotNull] DAEImplicitGrammarParser.IdentifierExpressionContext context)
        {
            return new IdentifierNode
            {
                Value = context.id.Text,
                Line = context.start.Line,
                Position = context.start.Column
            };
        }
        public override ASTNode VisitEquationRule([NotNull] DAEImplicitGrammarParser.EquationRuleContext context)
        {
            return new AssignmentNode
            {
                Left = (ExpressionNode)Visit(context.left),
                Right = (ExpressionNode)Visit(context.right),
                Line = context.start.Line,
                Position = context.start.Column
            };
        }
        public override ASTNode VisitParameterRule([NotNull] DAEImplicitGrammarParser.ParameterRuleContext context)
        {
            return new ParameterNode
            {
                Identifier = context.id.Text,
                Right = (ExpressionNode)Visit(context.expression()),
                Line = context.start.Line,
                Position = context.start.Column
            };
        }
        public override ASTNode VisitInitialValueAssignmentRule([NotNull] DAEImplicitGrammarParser.InitialValueAssignmentRuleContext context)
        {
            return new InitialValueNode
            {
                Identifier = context.id.Text,
                Right = (ExpressionNode)Visit(context.expression()),
                Line = context.start.Line,
                Position = context.start.Column
            };
        }
        public override ASTNode VisitStatementRule([NotNull] DAEImplicitGrammarParser.StatementRuleContext context)
        {
            return Visit(context.eq);
        }
        public override ASTNode VisitEmptyStatement([NotNull] DAEImplicitGrammarParser.EmptyStatementContext context)
        {
            return null;
        }
        public override ASTNode VisitNumber([NotNull] DAEImplicitGrammarParser.NumberContext context)
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
        public override ASTNode VisitCompileUnit([NotNull] DAEImplicitGrammarParser.CompileUnitContext context)
        {
            RootNode rootNode = new RootNode();
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
            return rootNode;
        }
    }
}
