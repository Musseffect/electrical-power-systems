using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Interpreter.Equations.Expression
{
    public enum ExpressionType
    {
        Division,
        Multiplication,
        Negation,
        Addition,
        Subtraction,
        Function,
        Float,
        Variable,
        Power,
        Parameter
    }
    public class Expression
    {
        public ExpressionType Type { get; protected set; }
    }
    public class Variable : Expression
    {
        public string Name;
        public Variable()
        {
            Type = ExpressionType.Variable;
        }
    }
    public class InfixExpression : Expression
    {
        public Expression Left { get; set; }
        public Expression Right { get; set; }
    }
    public class Addition : InfixExpression
    {
        public Addition()
        {
            Type = ExpressionType.Addition;
        }
    }
    public class Subtraction : InfixExpression
    {
        public Subtraction()
        {
            Type = ExpressionType.Subtraction;
        }
    }
    public class Multiplication : InfixExpression
    {
        public Multiplication()
        {
            Type = ExpressionType.Multiplication;
        }
    }
    public class Division : InfixExpression
    {
        public Division()
        {
            Type = ExpressionType.Division;
        }
    }
    public class Power : InfixExpression
    {
        public Power()
        {
            Type = ExpressionType.Power;
        }
    }
    public class Negation : Expression
    {
        public Negation()
        {
            Type = ExpressionType.Negation;
        }
        public Expression InnerNode { get; set; }
    }
    public class Function : Expression
    {
        public Function(FunctionEntry entry, List<Expression> arguments)
        {
            Type = ExpressionType.Function;
            Entry = entry;
            Arguments = arguments;
        }
        public FunctionEntry Entry { get; set; }
        public List<Expression> Arguments { get; set; }
    }
    public class Float : Expression
    {
        public Float()
        {
            Type = ExpressionType.Float;
        }
        public bool isOne()
        {
            return Value == 1.0;
        }
        public bool isZero()
        {
            return Value == 0.0;
        }
        static public Float operator -(Float a)
        {
            a.Value = -a.Value;
            return a;
        }
        static public Float operator -(Float a, Float b)
        {
            return new Float { Value = a.Value - b.Value };
        }
        static public Float operator +(Float a, Float b)
        {
            return new Float { Value = a.Value + b.Value };
        }
        static public Float operator *(Float a, Float b)
        {
            return new Float { Value = a.Value * b.Value };
        }
        static public Float operator /(Float a, Float b)
        {
            return new Float { Value = a.Value / b.Value };
        }
        static public Float pow(Float a, Float b)
        {
            return new Float { Value = Math.Pow(a.Value, b.Value) };
        }
        public double Value { get; set; }
    }

    public partial class EquationCompiler
    {
        Expression simplifyAddition(Addition node)
        {
            Expression left = simplify(node.Left);
            Expression right = simplify(node.Right);

            if (left.Type == ExpressionType.Float)
            {
                Float nl = (Float)left;
                if (nl.isZero())
                {
                    return right;
                }
                if (right.Type == ExpressionType.Float)
                {
                    Float nr = (Float)right;
                    return nl + nr;
                }
            }
            if (right.Type == ExpressionType.Float)
            {
                Float nr = (Float)right;
                if (nr.isZero())
                {
                    return left;
                }
            }
            return new Addition { Left = left, Right = right };
        }
        Expression simplifySubtraction(Subtraction node)
        {
            Expression left = simplify(node.Left);
            Expression right = simplify(node.Right);

            if (left.Type == ExpressionType.Float)
            {
                Float nl = (Float)left;
                if (right.Type == ExpressionType.Float)
                {
                    Float nr = (Float)right;
                    return nl - nr;
                }
                if (nl.isZero())
                {
                    return new Negation { InnerNode = right };
                }
            }
            if (right.Type == ExpressionType.Float)
            {
                Float nr = (Float)right;

                if (nr.isZero())
                {
                    return left;
                }
            }
            return new Subtraction { Left = left, Right = right };
        }
        Expression simplifyDivision(Division node)
        {
            Expression left = simplify(node.Left);
            Expression right = simplify(node.Right);
            if (left.Type == ExpressionType.Float)
            {
                Float nl = (Float)left;
                if (nl.isZero())
                {
                    return left;
                }
                if (right.Type == ExpressionType.Float)
                {
                    Float nr = (Float)right;
                    return nl / nr;
                }
            }
            if (right.Type == ExpressionType.Float)
            {
                Float nr = (Float)right;
                if (nr.isOne())
                {
                    return left;
                }
            }
            return new Division { Left = left, Right = right };
        }
        Expression simplifyMultiplication(Multiplication node)
        {
            Expression left = simplify(node.Left);
            Expression right = simplify(node.Right);
            if (left.Type == ExpressionType.Float)
            {
                Float nl = (Float)left;
                if (nl.isZero())
                {
                    return left;
                }
                if (nl.isOne())
                {
                    return right;
                }
                if (right.Type == ExpressionType.Float)
                {
                    Float nr = (Float)right;
                    return nl * nr;
                }
            }
            if (right.Type == ExpressionType.Float)
            {
                Float nr = (Float)right;
                if (nr.isOne())
                {
                    return left;
                }
                if (nr.isZero())
                {
                    return right;
                }
            }
            return new Multiplication { Left = left, Right = right };
        }
        Expression simplifyFunction(Function node)
        {
            bool @const = true;
            for (int i = 0; i < node.Arguments.Count; i++)
            {
                node.Arguments[i] = simplify(node.Arguments[i]);
                if (node.Arguments[i].Type != ExpressionType.Float)
                    @const = false;
            }
            if (!@const)
                return node;
            double value;
            FunctionEntry entry = node.Entry;
            List<Operand> operands = new List<Operand>();
            for (int i = 0; i < node.Arguments.Count; i++)
            {
                operands.Add(new Operand(((Float)node.Arguments[i]).Value));
            }
            value = entry.Exec(operands).value;
            return new Float { Value = value };
        }
        Expression simplifyPower(Power node)
        {
            Expression left = simplify(node.Left);
            Expression right = simplify(node.Right);
            if (left.Type == ExpressionType.Float)
            {
                Float nl = (Float)left;
                if (nl.isZero() || nl.isOne())
                {
                    return left;
                }
                if (right.Type == ExpressionType.Float)
                {
                    Float nr = (Float)right;
                    return Float.pow(nl, nr);
                }
            }
            if (right.Type == ExpressionType.Float)
            {
                Float nr = (Float)right;
                if (nr.isZero())
                {
                    return new Float { Value = 1.0 };
                }
                if (nr.isOne())
                {
                    return left;
                }
            }
            return new Power { Left = left, Right = right };
        }
        Expression simplifyNegation(Negation node)
        {
            Expression innerNode = simplify(node.InnerNode);
            if (innerNode.Type == ExpressionType.Float)
                return -((Float)innerNode);
            else if (innerNode.Type == ExpressionType.Negation)
            {
                return ((Negation)innerNode).InnerNode;
            }
            return node;
        }
        Expression simplify(Expression node)
        {
            switch (node.Type)
            {
                case ExpressionType.Negation:
                    return simplifyNegation((Negation)node);
                case ExpressionType.Addition:
                    return simplifyAddition((Addition)node);
                case ExpressionType.Subtraction:
                    return simplifySubtraction((Subtraction)node);
                case ExpressionType.Division:
                    return simplifyDivision((Division)node);
                case ExpressionType.Multiplication:
                    return simplifyMultiplication((Multiplication)node);
                case ExpressionType.Float:
                    return node;
                case ExpressionType.Variable:
                    return node;
                case ExpressionType.Function:
                    return simplifyFunction((Function)node);
                case ExpressionType.Power:
                    return simplifyPower((Power)node);
            }
            return node;
        }
    }

    partial class DifferentiationVisitor
    {
        Expression differentiateAddition(Addition node)
        {
            node.Left = differentiate(node.Left);
            node.Right = differentiate(node.Right);
            return node;
        }
        Expression differentiateSubtraction(Subtraction node)
        {
            node.Left = differentiate(node.Left);
            node.Right = differentiate(node.Right);
            return node;
        }
        Expression differentiateNegation(Negation node)
        {
            node.InnerNode = differentiate(node.InnerNode);
            return node;
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
                return
                    new Multiplication
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
                case ExpressionType.Addition:
                    return differentiateAddition((Addition)node);
                    break;
                case ExpressionType.Subtraction:
                    return differentiateSubtraction((Subtraction)node);
                    break;
                case ExpressionType.Division:
                    return differentiateDivision((Division)node);
                    break;
                case ExpressionType.Multiplication:
                    return differentiateMultiplication((Multiplication)node);
                    break;
                case ExpressionType.Float:
                    return differentiateFloat((Float)node);
                    break;
                case ExpressionType.Variable:
                    return differentiateVariable((Variable)node);
                    break;
                case ExpressionType.Function:
                    return differentiateFunction((Function)node);
                    break;
                case ExpressionType.Power:
                    return differentiatePower((Power)node);
                    break;
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
