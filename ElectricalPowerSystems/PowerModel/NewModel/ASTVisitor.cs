#define MODELINTERPRETER
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using static ElectricalPowerSystems.PowerModel.NewModel.ASTNode;

namespace ElectricalPowerSystems.PowerModel.NewModel
{
    public class ModelGrammarVisitor : ModelGrammarBaseVisitor<ASTNode>
    {
        public override ASTNode VisitArguments([NotNull] ModelGrammarParser.ArgumentsContext context)
        {
            return base.VisitArguments(context);
        }

        public override ASTNode VisitArray([NotNull] ModelGrammarParser.ArrayContext context)
        {
            List<ExpressionNode> elements = new List<ExpressionNode>();
            foreach (var expression in context.arrayValues().expression())
            {
                ASTNode node = VisitExpression(expression);
                elements.Add((ExpressionNode)node);
            }
            NodeType type;
            switch (context.type.type.Type)
            {
                case ModelGrammarLexer.BOOLEAN_TYPE:
                    type = NodeType.Boolean;
                    break;
                case ModelGrammarLexer.STRING_TYPE:
                    type = NodeType.String;
                    break;
                case ModelGrammarLexer.INTEGER_TYPE:
                    type = NodeType.Integer;
                    break;
                case ModelGrammarLexer.OBJECT_TYPE:
                    type = NodeType.Object;
                    break;
                case ModelGrammarLexer.FLOAT_TYPE:
                    type = NodeType.Float;
                    break;
                default:
                    throw new Exception("Invalid NodeType");
            }
            return new ArrayNode {
                ArrayType = type,
                Elements = elements,
                Line = context.start.Line,
                Position = context.start.Column
            };
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
                Value = (context.value.Type ==  ModelGrammarLexer.TRUE? true: false),
                Line = context.value.Line,
                Position = context.value.Column
            };
        }
        public override ASTNode VisitBracketExpression([NotNull] ModelGrammarParser.BracketExpressionContext context)
        {
            return Visit(context.expression());
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
                        Phase = MathUtils.MathUtils.Radians(double.Parse(context.right.Text, CultureInfo.InvariantCulture)),
                        Line = context.left.Line,
                        Position = context.left.Column
                    };
                default:
                    throw new NotSupportedException();
            }
        }
        public override ASTNode VisitConnectionStatement([NotNull] ModelGrammarParser.ConnectionStatementContext context)
        {
            return new ConnectionNode {
                Element1 = context.elementID1.Text ,
                Node1 = context.nodeID1.Text,
                Element2 = context.elementID2.Text,
                Node2 = context.nodeID2.Text,
                Line = context.start.Line,
                Position = context.start.Column
            };
        }

        public override ASTNode VisitConstant([NotNull] ModelGrammarParser.ConstantContext context)
        {
            return base.VisitConstant(context);
        }

        public override ASTNode VisitConstantExpression([NotNull] ModelGrammarParser.ConstantExpressionContext context)
        {
            return base.VisitConstantExpression(context);
        }
        public override ASTNode VisitElementStatement([NotNull] ModelGrammarParser.ElementStatementContext context)
        {
            return new ElementNode {
                Id = context.element.Text,
                Definition = VisitObject(context.definition) as ObjectNode,
                Line = context.element.Line,
                Position = context.element.Column };
        }
        public override ASTNode VisitExpression([NotNull] ModelGrammarParser.ExpressionContext context)
        {
            return Visit(context);
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
        public override ASTNode VisitString([NotNull] ModelGrammarParser.StringContext context)
        {
            return new StringNode
            {
                Value = context.value.Text.Substring(1, context.value.Text.Length - 2),
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
            return new KeyValueNode
            {
                Key = context.key.Text,
                Value = VisitExpression(context.value) as ExpressionNode,
                Line = context.key.Line,
                Position = context.key.Column
            };
        }
        public override ASTNode VisitModel([NotNull] ModelGrammarParser.ModelContext context)
        {
            List<ExpressionNode> statements = new List<ExpressionNode>();
            foreach (var statement in context.statement())
            {
                ASTNode node = VisitStatement(statement);
            }
            List<ElementNode> elements = new List<ElementNode>();
            foreach (var element in context.elementStatement())
            {
                elements.Add((ElementNode)VisitElementStatement(element));
            }
            List<ConnectionNode> connections = new List<ConnectionNode>();
            foreach (var connection in context.connectionStatement())
            {
                connections.Add((ConnectionNode)VisitConnectionStatement(connection));
            }
            ObjectNode modelParameters = VisitObject(context.modelObject) as ObjectNode;
            return new ModelNode
            {
                Statements = statements,
                Elements = elements,
                Connections = connections,
                ModelParameters = modelParameters,
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
            return new ObjectNode() {
                Name = context.name.Text,
                Arguments = arguments,
                Line = context.start.Line,
                Position = context.start.Column
            };
        }

        public override ASTNode VisitObjectExpression([NotNull] ModelGrammarParser.ObjectExpressionContext context)
        {
            return VisitObject(context.obj);
        }

        public override ASTNode VisitStatement([NotNull] ModelGrammarParser.StatementContext context)
        {
            return Visit(context.expression());
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
}
