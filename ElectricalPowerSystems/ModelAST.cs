using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace ElectricalPowerSystems
{
    class ModelAST
    {
        public class ExpressionNode
        {
        }
        public class InfixExpressionNode : ExpressionNode
        {
            public ExpressionNode Left { get; set; }
            public ExpressionNode Right { get; set; }
        }
        public class AdditionNode : InfixExpressionNode
        {
        }
        public class SubtractionNode : InfixExpressionNode
        {
        }
        public class MultiplicationNode : InfixExpressionNode
        {
        }
        public class DivisionNode : InfixExpressionNode
        {
        }
        public class AssignmentNode : InfixExpressionNode
        {
        }
        public class NegationNode : ExpressionNode
        {
            public ExpressionNode InnerNode { get; set; }
        }
        public class StringNode : ExpressionNode
        {
            public string Value { get; set; }
        }
        public class FunctionNode : ExpressionNode
        {
            public string FunctionName { get; set; }
            public List<ExpressionNode> Arguments { get; set; }
        }
        public class ComplexPhaseNode : ExpressionNode
        {
            public double Magnitude { get; set; }
            public double Phase { get; set; }
        }
        public class ComplexNode : ExpressionNode
        {
            public double Re { get; set; }
            public double Im { get; set; }
        }
        public class IdentifierNode : ExpressionNode
        {
            public string Value { get; set; }
        }
        public class FloatNode : ExpressionNode
        {
            public double Value { get; set; }
        }
        class ExpressionVisitor : ModelGrammarBaseVisitor<ExpressionNode>
        {
            public override ExpressionNode VisitBinaryOperatorExpression([NotNull] ModelGrammarParser.BinaryOperatorExpressionContext context)
            {
                InfixExpressionNode node;
                int type=context.binaryOperator().op.Type;
                switch (type)
                {
                    case ModelGrammarLexer.ASSIGN:
                        node = new AssignmentNode();
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
                node.Left = Visit(context.left);
                node.Right = Visit(context.right);
                return node;
            }
            public override ExpressionNode VisitStringConstant([NotNull] ModelGrammarParser.StringConstantContext context)
            {
                return new StringNode
                {
                    Value = context.value.Text
                };
                return base.VisitStringConstant(context);
            }
            public override ExpressionNode VisitNumberConstant([NotNull] ModelGrammarParser.NumberConstantContext context)
            {
                return new FloatNode
                {
                    Value = double.Parse(context.number().value.Text)
                };
                return base.VisitNumberConstant(context);
            }
            public override ExpressionNode VisitComplex([NotNull] ModelGrammarParser.ComplexContext context)
            {
                return new ComplexNode {
                    Re =  0.0 ,
                    Im = double.Parse(context.im.value.Text)
                };
            }
            public override ExpressionNode VisitComplexExp([NotNull] ModelGrammarParser.ComplexExpContext context)
            {
                switch (context.type.Type)
                {
                    case ModelGrammarLexer.IM:
                        return new ComplexNode
                        {
                            Re = double.Parse(context.left.value.Text),
                            Im = double.Parse(context.right.value.Text)
                        };
                    case ModelGrammarLexer.ANGLE:
                        return new ComplexPhaseNode
                        {
                            Magnitude = double.Parse(context.left.value.Text),
                            Phase = double.Parse(context.right.value.Text)
                        };
                    default:
                        throw new NotSupportedException();
                }
            }
            public override ExpressionNode VisitComplexConstant([NotNull] ModelGrammarParser.ComplexConstantContext context)
            {
                return Visit(context);
            }
            public override ExpressionNode VisitComplexExprConstant([NotNull] ModelGrammarParser.ComplexExprConstantContext context)
            {
                return Visit(context);
            }
            public override ExpressionNode VisitConstantExpression([NotNull] ModelGrammarParser.ConstantExpressionContext context)
            {
                return Visit(context.value);
            }
            public override ExpressionNode VisitFunctionExpression([NotNull] ModelGrammarParser.FunctionExpressionContext context)
            {
                var functionName = context.func.Text;
                List<ExpressionNode> arguments=new List<ExpressionNode>();
                var args = context.functionArguments();
                for (int i = 0,j=0; i < args.ChildCount; i += 2,j++)
                    arguments.Add(Visit(args.expression(j)));
                return new FunctionNode {
                    FunctionName = functionName,
                    Arguments=arguments
                };
            }
            public override ExpressionNode VisitUnaryOperatorExpression([NotNull] ModelGrammarParser.UnaryOperatorExpressionContext context)
            {
                switch (context.unaryOperator().op.Type)
                {
                    case ModelGrammarLexer.PLUS:
                        return Visit(context.expression());

                    case ModelGrammarLexer.MINUS:
                        return new NegationNode
                        {
                            InnerNode = Visit(context.expression())
                        };

                    default:
                        throw new NotSupportedException();
                }
            }
            public override ExpressionNode VisitIdentificatorExpression([NotNull] ModelGrammarParser.IdentificatorExpressionContext context)
            {
                return new IdentifierNode
                {
                    Value=context.id.Text
                };
                return base.VisitIdentificatorExpression(context);
            }
        }
        class ASTModelGenerator
        {
            //variable transformation float to Complex to ComplexPhase
            enum VariableType
            {
                String=0,
                Complex=3,
                ComplexPhase=5,
                Float=1,
                Element=2,
                Node=4,
                Void=6
            }
            class VoidVariable:Variable
            {
                public VoidVariable()
                {
                    type = VariableType.Void;
                }
            }
            class Variable
            {
                protected VariableType type;
                public VariableType Type { get {
                        return type;
                    } }
            }
            class ComplexVariable : Variable
            {
                public double Re { get; set; }
                public double Im { get; set; }
                public ComplexVariable()
                {
                    type = VariableType.Complex;
                }

            }
            class ComplexPhaseVariable : Variable
            {
                public double Magn { get; set; }
                public double Phase { get; set; }
                public ComplexPhaseVariable()
                {
                    type = VariableType.Complex;
                }
            }
            class FloatVariable : Variable
            {
                public double Value { get; set; }
                public FloatVariable()
                {
                    type = VariableType.Float;
                }
            }
            class ElementVariable : Variable
            {
                public int Id { get; set; }
                public ElementVariable()
                {
                    type = VariableType.Element;
                }
            }
            class NodeVariable : Variable
            {
                public int Id { get; set; }
                public NodeVariable()
                {
                    type = VariableType.Node;
                }
            }
            ModelGraphCreatorAC modelGraph;
            Dictionary<string, Variable> variableTable;
            bool checkNumericTypes(VariableType type)
            {
                return ((int)type & 1)!=0;
            }
            Variable add(Variable left, Variable right)
            {
                if (!(checkNumericTypes(left.Type)&&checkNumericTypes(right.Type)))
                {
                    throw new Exception("Illegal type in operation");
                }
                if (left.Type > right.Type)
                {
                    right = convert(right, left.Type);
                }
                else
                    left = convert(left, right.Type);
            }
            Variable sub(Variable left, Variable right)
            {

            }
            Variable mult(Variable left, Variable right)
            {

            }
            Variable div(Variable left, Variable right)
            {

            }
            static Variable convert(Variable a, VariableType b)
            {
                VariableType variableType=a.Type;
                switch (b)
                {
                    case VariableType.Complex:
                    case VariableType.ComplexPhase:
                    case VariableType.Element:
                    case VariableType.Float:
                    case VariableType.Node:
                    case VariableType.String:
                    default:
                        throw new Exception("Illegal convercation");
                }
            }
            Variable eval(ExpressionNode exp)
            {
                switch (exp)
                {
                    case FloatNode:
                        return new FloatVariable {
                            Value = ((FloatNode)exp).Value
                        };
                        break;
                    case AdditionNode:
                        AdditionNode node = (AdditionNode)exp;
                        Variable left = eval(node.Left);
                        Variable right = eval(node.Right);
                        break;

                }
                return new VoidVariable();
            }
            public ModelGraphCreatorAC generate(ExpressionNode exp)
            {
                modelGraph = new ModelGraphCreatorAC();
                ariableTable = new Dictionary<string, Variable>();
                evaluate(exp);
                return modelGraph;
            }

        }
    }

}
