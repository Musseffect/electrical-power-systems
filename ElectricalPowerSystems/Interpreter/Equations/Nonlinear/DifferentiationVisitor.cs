using ElectricalPowerSystems.Interpreter.Equations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Interpreter.Equations.Nonlinear
{
    partial class DifferentiationVisitor
    {
        ExpressionNode differentiateAddition(AdditionNode node)
        {
            node.Left = differentiate(node.Left);
            node.Right = differentiate(node.Right);
            return node;
        }
        ExpressionNode differentiateSubtraction(SubtractionNode node)
        {
            node.Left = differentiate(node.Left);
            node.Right = differentiate(node.Right);
            return node;
        }
        ExpressionNode differentiateNegation(NegationNode node)
        {
            node.InnerNode = differentiate(node.InnerNode);
            return node;
        }
        ExpressionNode differentiatePower(PowerNode node)
        {
            return new AdditionNode
            {
                Left = new MultiplicationNode
                {
                    Left = node,
                    Right = new MultiplicationNode
                    {
                        Left = differentiate(node.Right),
                        Right = new FunctionEntryNode(FunctionTable.getFunctionEntry("ln"), new List<ExpressionNode> { node.Left })
                    }
                },
                Right = new MultiplicationNode
                {
                    Left = new PowerNode
                    {
                        Left = node.Left,
                        Right = new SubtractionNode
                        {
                            Left = node.Right,
                            Right = new FloatNode { Value = 1.0f }
                        }
                    },
                    Right = new MultiplicationNode
                    {
                        Left = node.Right,
                        Right = differentiate(node.Left)
                    }
                }
            };
        }
        ExpressionNode differentiateFunction(FunctionEntryNode node)
        {
            if (node.Arguments.Count == 0)
                return new FloatNode { Value = 0.0 };
            if (node.Arguments.Count == 1)
            {
                return
                    new MultiplicationNode
                    {
                        Left = differentiate(node.Arguments[0]),
                        Right = node.Entry.Der[0](node.Arguments)
                    };
            }
            AdditionNode root = new AdditionNode();
            AdditionNode current = root;
            for (int i = 0; i < node.Arguments.Count; i++)
            {
                current.Left = new MultiplicationNode
                {
                    Left = differentiate(node.Arguments[i]),
                    Right = node.Entry.Der[i](node.Arguments)
                };
                current.Right = new AdditionNode();
                current = (AdditionNode)current.Right;
            }
            current.Right = node.Entry.Der[node.Arguments.Count - 1](node.Arguments);
            return root;
        }
        ExpressionNode differentiateMultiplication(MultiplicationNode node)
        {
            return new AdditionNode
            {
                Left = new MultiplicationNode { Left = differentiate(node.Left), Right = node.Right },
                Right = new MultiplicationNode { Left = node.Left, Right = differentiate(node.Right) }
            };
        }
        ExpressionNode differentiateDivision(DivisionNode node)
        {
            return new DivisionNode
            {
                Left = new SubtractionNode
                {
                    Left = new MultiplicationNode
                    {
                        Left = differentiate(node.Left),
                        Right = node.Right
                    },
                    Right = new MultiplicationNode
                    {
                        Left = node.Left,
                        Right = differentiate(node.Right)

                    }
                },
                Right = new MultiplicationNode
                {
                    Left = node.Right,
                    Right = node.Right
                },
            };
        }
        ExpressionNode differentiateIdentifier(IdentifierNode node)
        {

            if (node.Value == variable)
            {
                return new FloatNode { Value = 1.0 };
            }
            else
                return new FloatNode { Value = 0.0 };
        }

        ExpressionNode differentiateFloat(FloatNode node)
        {
            return new FloatNode { Value = 0.0 };
        }
        ExpressionNode differentiate(ExpressionNode node)
        {
            switch (node.Type)
            {
                case ASTNodeType.Addition:
                    return differentiateAddition((AdditionNode)node);
                    break;
                case ASTNodeType.Subtraction:
                    return differentiateSubtraction((SubtractionNode)node);
                    break;
                case ASTNodeType.Division:
                    return differentiateDivision((DivisionNode)node);
                    break;
                case ASTNodeType.Multiplication:
                    return differentiateMultiplication((MultiplicationNode)node);
                    break;
                case ASTNodeType.Float:
                    return differentiateFloat((FloatNode)node);
                    break;
                case ASTNodeType.Identifier:
                    return differentiateIdentifier((IdentifierNode)node);
                    break;
                case ASTNodeType.FunctionEntry:
                    return differentiateFunction((FunctionEntryNode)node);
                    break;
                case ASTNodeType.Power:
                    return differentiatePower((PowerNode)node);
                    break;
            }
            throw new Exception();
        }
        public ExpressionNode differentiate(ExpressionNode root,string variable)
        {
            this.variable = variable;
            return differentiate(root);
        }
        string variable;
    }
}
