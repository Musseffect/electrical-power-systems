using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Equations.Expression
{

    partial class DifferentiationVisitor
    {
        Expression DifferentiateAddition(Addition node)
        {
            return new Addition
            {
                Left = Differentiate(node.Left) ,
                Right = Differentiate(node.Right)
            };
        }
        Expression DifferentiateSubtraction(Subtraction node)
        {
            return new Subtraction
            {
                Left = Differentiate(node.Left),
                Right = Differentiate(node.Right)
            };
        }
        Expression DifferentiateNegation(Negation node)
        {
            return new Negation {
                InnerNode = Differentiate(node.InnerNode)
            };
        }
        Expression DifferentiatePower(Power node)
        {
            return new Addition
            {
                Left = new Multiplication
                {
                    Left = node,
                    Right = new Multiplication
                    {
                        Left = Differentiate(node.Right),
                        Right = new Function(FunctionTable.GetFunctionEntry("ln"), new List<Expression> { node.Left })
                    }
                },
                Right = new Multiplication
                {
                    Left = new Power
                    {
                        Left = node.Left,
                        Right = new Subtraction
                        {
                            Left = node.Right,
                            Right = new Float { Value = 1.0f }
                        }
                    },
                    Right = new Multiplication
                    {
                        Left = node.Right,
                        Right = Differentiate(node.Left)
                    }
                }
            };
        }
        Expression DifferentiateFunction(Function node)
        {
            if (node.Arguments.Count == 0)
                return new Float { Value = 0.0 };
            if (node.Arguments.Count == 1)
            {
                return new Multiplication
                    {
                        Left = Differentiate(node.Arguments[0]),
                        Right = node.Entry.Der[0](node.Arguments)
                    };
            }
            Addition root = new Addition();
            Addition current = root;
            for (int i = 0; i < node.Arguments.Count - 1; i++)
            {
                current.Left = new Multiplication
                {
                    Left = Differentiate(node.Arguments[i]),
                    Right = node.Entry.Der[i](node.Arguments)
                };
                if (i < node.Arguments.Count - 2)
                {
                    current.Right = new Addition();
                    current = (Addition)current.Right;
                }
            }
            current.Right = new Multiplication
            {
                Left = Differentiate(node.Arguments[node.Arguments.Count - 1]),
                Right = node.Entry.Der[node.Arguments.Count - 1](node.Arguments)
            };
            return root;
        }
        Expression DifferentiateMultiplication(Multiplication node)
        {
            return new Addition
            {
                Left = new Multiplication { Left = Differentiate(node.Left), Right = node.Right },
                Right = new Multiplication { Left = node.Left, Right = Differentiate(node.Right) }
            };
        }
        Expression DifferentiateDivision(Division node)
        {
            return new Division
            {
                Left = new Subtraction
                {
                    Left = new Multiplication
                    {
                        Left = Differentiate(node.Left),
                        Right = node.Right
                    },
                    Right = new Multiplication
                    {
                        Left = node.Left,
                        Right = Differentiate(node.Right)

                    }
                },
                Right = new Multiplication
                {
                    Left = node.Right,
                    Right = node.Right
                },
            };
        }
        Expression DifferentiateVariable(Variable node)
        {
            if (node.Name == variable)
            {
                return new Float { Value = 1.0 };
            }
            else
                return new Float { Value = 0.0 };
        }

        Expression DifferentiateFloat(Float node)
        {
            return new Float { Value = 0.0 };
        }
        Expression Differentiate(Expression node)
        {
            switch (node.Type)
            {
                case ExpressionType.Negation:
                    return DifferentiateNegation((Negation)node);
                case ExpressionType.Addition:
                    return DifferentiateAddition((Addition)node);
                case ExpressionType.Subtraction:
                    return DifferentiateSubtraction((Subtraction)node);
                case ExpressionType.Division:
                    return DifferentiateDivision((Division)node);
                case ExpressionType.Multiplication:
                    return DifferentiateMultiplication((Multiplication)node);
                case ExpressionType.Float:
                    return DifferentiateFloat((Float)node);
                case ExpressionType.Variable:
                    return DifferentiateVariable((Variable)node);
                case ExpressionType.Function:
                    return DifferentiateFunction((Function)node);
                case ExpressionType.Power:
                    return DifferentiatePower((Power)node);
            }
            throw new Exception();
        }
        public Expression Differentiate(Expression root, string variable)
        {
            this.variable = variable;
            return Differentiate(root);
        }
        string variable;
    }


}
