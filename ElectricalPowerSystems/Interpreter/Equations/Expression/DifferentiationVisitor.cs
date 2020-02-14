using ElectricalPowerSystems.Interpreter.Equations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Interpreter.Equations.Expression
{

    partial class DifferentiationVisitor
    {
        Expression differentiateAddition(Addition node)
        {
            return new Addition
            {
                Left = differentiate(node.Left) ,
                Right = differentiate(node.Right)
            };
        }
        Expression differentiateSubtraction(Subtraction node)
        {
            return new Subtraction
            {
                Left = differentiate(node.Left),
                Right = differentiate(node.Right)
            };
        }
        Expression differentiateNegation(Negation node)
        {
            return new Negation {
                InnerNode = differentiate(node.InnerNode)
            };
        }
        Expression differentiatePower(Power node)
        {
            return new Addition
            {
                Left = new Multiplication
                {
                    Left = node,
                    Right = new Multiplication
                    {
                        Left = differentiate(node.Right),
                        Right = new Function(FunctionTable.getFunctionEntry("ln"), new List<Expression> { node.Left })
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
                        Right = differentiate(node.Left)
                    }
                }
            };
        }
        Expression differentiateFunction(Function node)
        {
            if (node.Arguments.Count == 0)
                return new Float { Value = 0.0 };
            if (node.Arguments.Count == 1)
            {
                return new Multiplication
                    {
                        Left = differentiate(node.Arguments[0]),
                        Right = node.Entry.Der[0](node.Arguments)
                    };
            }
            Addition root = new Addition();
            Addition current = root;
            for (int i = 0; i < node.Arguments.Count; i++)
            {
                current.Left = new Multiplication
                {
                    Left = differentiate(node.Arguments[i]),
                    Right = node.Entry.Der[i](node.Arguments)
                };
                current.Right = new Addition();
                current = (Addition)current.Right;
            }
            current.Right = node.Entry.Der[node.Arguments.Count - 1](node.Arguments);
            return root;
        }
        Expression differentiateMultiplication(Multiplication node)
        {
            return new Addition
            {
                Left = new Multiplication { Left = differentiate(node.Left), Right = node.Right },
                Right = new Multiplication { Left = node.Left, Right = differentiate(node.Right) }
            };
        }
        Expression differentiateDivision(Division node)
        {
            return new Division
            {
                Left = new Subtraction
                {
                    Left = new Multiplication
                    {
                        Left = differentiate(node.Left),
                        Right = node.Right
                    },
                    Right = new Multiplication
                    {
                        Left = node.Left,
                        Right = differentiate(node.Right)

                    }
                },
                Right = new Multiplication
                {
                    Left = node.Right,
                    Right = node.Right
                },
            };
        }
        Expression differentiateVariable(Variable node)
        {
            if (node.Name == variable)
            {
                return new Float { Value = 1.0 };
            }
            else
                return new Float { Value = 0.0 };
        }

        Expression differentiateFloat(Float node)
        {
            return new Float { Value = 0.0 };
        }
        Expression differentiate(Expression node)
        {
            switch (node.Type)
            {
                case ExpressionType.Negation:
                    return differentiateNegation((Negation)node);
                case ExpressionType.Addition:
                    return differentiateAddition((Addition)node);
                case ExpressionType.Subtraction:
                    return differentiateSubtraction((Subtraction)node);
                case ExpressionType.Division:
                    return differentiateDivision((Division)node);
                case ExpressionType.Multiplication:
                    return differentiateMultiplication((Multiplication)node);
                case ExpressionType.Float:
                    return differentiateFloat((Float)node);
                case ExpressionType.Variable:
                    return differentiateVariable((Variable)node);
                case ExpressionType.Function:
                    return differentiateFunction((Function)node);
                case ExpressionType.Power:
                    return differentiatePower((Power)node);
            }
            throw new Exception();
        }
        public Expression differentiate(Expression root, string variable)
        {
            this.variable = variable;
            return differentiate(root);
        }
        string variable;
    }


}
