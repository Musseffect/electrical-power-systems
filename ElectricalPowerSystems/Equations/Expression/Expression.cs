﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Equations.Expression
{
    public enum ExpressionType
    {
        Division,
        Multiplication,
        Negation,
        Addition,
        Subtraction,
        Inverse,
        Function,
        Float,
        Variable,
        Power,
        Parameter
    }
    public abstract class Expression
    {
        public abstract Expression Copy();
        public ExpressionType Type { get; protected set; }
    }
    public class Variable : Expression
    {
        public string Name;
        public Variable()
        {
            Type = ExpressionType.Variable;
        }

        public override Expression Copy()
        {
            return new Variable { Name = Name };
        }
    }
    public abstract class InfixExpression : Expression
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
        public override Expression Copy()
        {
            return new Addition { Left = this.Left.Copy(),Right = this.Right.Copy()};
        }
    }
    public class Subtraction : InfixExpression
    {
        public Subtraction()
        {
            Type = ExpressionType.Subtraction;
        }
        public override Expression Copy()
        {
            return new Subtraction { Left = this.Left.Copy(), Right = this.Right.Copy() };
        }
    }
    public class Multiplication : InfixExpression
    {
        public Multiplication()
        {
            Type = ExpressionType.Multiplication;
        }
        public override Expression Copy()
        {
            return new Multiplication { Left = this.Left.Copy(), Right = this.Right.Copy() };
        }
    }
    public class Division : InfixExpression
    {
        public Division()
        {
            Type = ExpressionType.Division;
        }
        public override Expression Copy()
        {
            return new Division { Left = this.Left.Copy(), Right = this.Right.Copy() };
        }
    }
    public class Power : InfixExpression
    {
        public Power()
        {
            Type = ExpressionType.Power;
        }
        public override Expression Copy()
        {
            return new Power { Left = this.Left.Copy(), Right = this.Right.Copy() };
        }
    }
    public class Negation : Expression
    {
        public Negation()
        {
            Type = ExpressionType.Negation;
        }
        public override Expression Copy()
        {
            return new Negation { InnerNode = this.InnerNode.Copy()};
        }
        public Expression InnerNode { get; set; }
    }
    public class Inverse : Expression
    {
        public Inverse()
        {
            Type = ExpressionType.Inverse;
        }
        public override Expression Copy()
        {
            return new Inverse { InnerNode = this.InnerNode.Copy() };
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
        public override Expression Copy()
        {
            return new Function(Entry, CloneArguments());
        }
        public List<Expression> CloneArguments()
        {
            List<Expression> result = new List<Expression>();
            foreach (var Arg in Arguments)
            {
                result.Add(Arg.Copy());
            }
            return result;
        }
    }
    public class Float : Expression
    {
        public Float()
        {
            Type = ExpressionType.Float;
        }
        public override Expression Copy()
        {
            return new Float { Value = Value};
        }
        public bool IsOne()
        {
            return Value == 1.0;
        }
        public bool IsZero()
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
        static public Float Pow(Float a, Float b)
        {
            return new Float { Value = Math.Pow(a.Value, b.Value) };
        }
        public double Value { get; set; }
    }

    public partial class ExpressionSimplifier
    {
        private static Expression SimplifyInverse(Inverse node)
        {
            Expression innerNode = Simplify(node.InnerNode);
            if (innerNode.Type == ExpressionType.Float)
                return new Float { Value = 1.0 / (innerNode as Float).Value };
            else if (innerNode.Type == ExpressionType.Inverse)
            {
                return ((Inverse)innerNode).InnerNode;
            }
            else if (innerNode.Type == ExpressionType.Division)
            {
                return new Division { Left=(innerNode as Division).Right,Right = (innerNode as Division).Left};
            }
            /*else if(innerNode.Type == ExpressionType.Multiplication)
            {
                return new Multiplication { Left = (innerNode as Division).Left, Right = (innerNode as Division).Right };
            }*/
            return new Inverse { InnerNode = innerNode };
        }
        private static Expression SimplifyAddition(Addition node)
        {
            /*Expression left = Simplify(node.Left);
            Expression right = Simplify(node.Right);

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
            }*/
            Stack<Expression> stack = new Stack<Expression>();
            stack.Push(node);
            List<Expression> operands = new List<Expression>();
            List<Float> floats = new List<Float>();
            while (stack.Count != 0)
            {
                Expression current = stack.Pop();
                if (current.Type == ExpressionType.Addition)
                {
                    stack.Push(Simplify((current as Addition).Right));
                    stack.Push(Simplify((current as Addition).Left));
                }
                else if (current.Type == ExpressionType.Subtraction)
                {
                    stack.Push(Simplify(new Negation { InnerNode = (current as Subtraction).Right }));
                    stack.Push(Simplify((current as Subtraction).Left));
                }
                else if (current.Type == ExpressionType.Float)
                {
                    floats.Add(current as Float);
                }
                else
                {
                    operands.Add(current);
                }
            }
            Expression root;
            if (floats.Count > 0)
            {
                Float result = new Float { Value = 0.0f };
                foreach (var fNode in floats)
                {
                    result += fNode;
                }
                root = result;
                if (result.IsZero())
                {
                    if (operands.Count > 0)
                    {
                        root = operands[0];
                        operands.RemoveAt(0);
                    }
                }
            }
            else
            {
                root = operands[0];
                operands.RemoveAt(0);
            }
            foreach (var value in operands)
            {
                if (value is Negation)
                {
                    root = new Subtraction { Left = root, Right = (value as Negation).InnerNode };
                }
                else
                {
                    root = new Addition { Left = root, Right = value };
                }
            }
            return root;
            //return new Addition { Left = left, Right = right };
        }
        private static Expression SimplifySubtraction(Subtraction node)
        {
            /*Expression left = Simplify(node.Left);
            Expression right = Simplify(node.Right);

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
            return new Subtraction { Left = left, Right = right };*/
            Stack<Expression> stack = new Stack<Expression>();
            stack.Push(node);
            List<Expression> operands = new List<Expression>();
            List<Float> floats = new List<Float>();
            while (stack.Count != 0)
            {
                Expression current = stack.Pop();
                if (current.Type == ExpressionType.Addition)
                {
                    stack.Push(Simplify((current as Addition).Right));
                    stack.Push(Simplify((current as Addition).Left));
                }
                else if (current.Type == ExpressionType.Subtraction)
                {
                    stack.Push(Simplify(new Negation { InnerNode = (current as Subtraction).Right }));
                    stack.Push(Simplify((current as Subtraction).Left));
                }
                else if (current.Type == ExpressionType.Float)
                {
                    floats.Add(current as Float);
                }
                else
                {
                    operands.Add(current);
                }
            }
            Expression root;
            if (floats.Count > 0)
            {
                Float result = new Float { Value = 0.0f };
                foreach (var fNode in floats)
                {
                    result += fNode;
                }
                root = result;
                if (result.IsZero())
                {
                    if (operands.Count > 0)
                    {
                        root = operands[0];
                        operands.RemoveAt(0);
                    }
                }
            }
            else
            {
                root = operands[0];
                operands.RemoveAt(0);
            }
            foreach (var value in operands)
            {
                if (value is Negation)
                {
                    root = new Subtraction { Left = root, Right = (value as Negation).InnerNode };
                }
                else
                {
                    root = new Addition { Left = root, Right = value };
                }
            }
            return root;
        }
        private static Expression SimplifyDivision(Division node)
        {
            Stack<Expression> stack = new Stack<Expression>();
            stack.Push(node);
            List<Expression> operands = new List<Expression>();
            List<Float> floats = new List<Float>();
            while (stack.Count != 0)
            {
                Expression current = stack.Pop();
                if (current.Type == ExpressionType.Multiplication)
                {
                    stack.Push(Simplify((current as Multiplication).Right));
                    stack.Push(Simplify((current as Multiplication).Left));
                }
                else if (current.Type == ExpressionType.Division)
                {
                    stack.Push(Simplify(new Inverse { InnerNode = (current as Division).Right }));
                    stack.Push(Simplify((current as Division).Left));
                }
                else if (current.Type == ExpressionType.Float)
                {
                    floats.Add(current as Float);
                }
                else
                {
                    operands.Add(current);
                }
            }
            Expression root;
            if (floats.Count > 0)
            {
                Float result = new Float { Value = 1.0f };
                foreach (var fNode in floats)
                {
                    result *= fNode;
                }
                root = result;
                if (result.IsOne())
                {
                    if (operands.Count > 0)
                    {
                        root = operands[0];
                        if (root is Inverse)
                        {
                            root = new Division { Left = result, Right = (root as Inverse).InnerNode };
                        }
                        operands.RemoveAt(0);
                    }
                    else
                        return result;
                }
                else if (result.IsZero())
                {
                    return result;
                }
            }
            else
            {
                root = operands[0];
                operands.RemoveAt(0);
            }
            foreach (var value in operands)
            {
                if (value is Inverse)
                {
                    root = new Division { Left = root, Right = (value as Inverse).InnerNode };
                }
                else
                {
                    root = new Multiplication { Left = root, Right = value };
                }
            }
            return root;
            /*Expression left = Simplify(node.Left);
            Expression right = Simplify(node.Right);
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
            return new Division { Left = left, Right = right };*/
        }
        private static Expression SimplifyMultiplication(Multiplication node)
        {
            /*Expression left = Simplify(node.Left);
            Expression right = Simplify(node.Right);
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
            return new Multiplication { Left = left, Right = right };*/
            Stack<Expression> stack = new Stack<Expression>();
            stack.Push(node);
            List<Expression> operands = new List<Expression>();
            List<Float> floats = new List<Float>();
            while (stack.Count != 0)
            {
                Expression current = stack.Pop();
                if (current.Type == ExpressionType.Multiplication)
                {
                    stack.Push(Simplify((current as Multiplication).Right));
                    stack.Push(Simplify((current as Multiplication).Left));
                }
                else if (current.Type == ExpressionType.Division)
                {
                    stack.Push(Simplify(new Inverse { InnerNode = (current as Division).Right }));
                    stack.Push(Simplify((current as Division).Left));
                }
                else if (current.Type == ExpressionType.Float)
                {
                    floats.Add(current as Float);
                }
                else
                {
                    operands.Add(current);
                }
            }
            Expression root;
            if (floats.Count > 0)
            {
                Float result = new Float { Value = 1.0f };
                foreach (var fNode in floats)
                {
                    result *= fNode;
                }
                root = result;
                if (result.IsOne())
                {
                    if (operands.Count > 0)
                    {
                        root = operands[0];
                        if (root is Inverse)
                        {
                            root = new Division { Left = result, Right = (root as Inverse).InnerNode };
                        }
                        operands.RemoveAt(0);
                    }
                    else
                        return result;
                }
                else if (result.IsZero())
                {
                    return result;
                }
            }
            else
            {
                root = operands[0];
                operands.RemoveAt(0);
            }
            foreach (var value in operands)
            {
                if (value is Inverse)
                {
                    root = new Division { Left = root, Right = (value as Inverse).InnerNode };
                }
                else
                {
                    root = new Multiplication { Left = root, Right = value };
                }
            }
            return root;
        }
        private static Expression SimplifyFunction(Function node)
        {
            bool @const = true;
            for (int i = 0; i < node.Arguments.Count; i++)
            {
                node.Arguments[i] = Simplify(node.Arguments[i]);
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
        private static Expression SimplifyPower(Power node)
        {
            Expression left = Simplify(node.Left);
            Expression right = Simplify(node.Right);
            if (left.Type == ExpressionType.Float)
            {
                Float nl = (Float)left;
                if (nl.IsZero() || nl.IsOne())
                {
                    return left;
                }
                if (right.Type == ExpressionType.Float)
                {
                    Float nr = (Float)right;
                    return Float.Pow(nl, nr);
                }
            }
            if (right.Type == ExpressionType.Float)
            {
                Float nr = (Float)right;
                if (nr.IsZero())
                {
                    return new Float { Value = 1.0 };
                }
                if (nr.IsOne())
                {
                    return left;
                }
            }
            return new Power { Left = left, Right = right };
        }
        private static Expression SimplifyNegation(Negation node)
        {
            Expression innerNode = Simplify(node.InnerNode);
            if (innerNode.Type == ExpressionType.Float)
                return -((Float)innerNode);
            else if (innerNode.Type == ExpressionType.Negation)
            {
                return ((Negation)innerNode).InnerNode;
            }
            else if (innerNode.Type == ExpressionType.Subtraction)
            {
                return new Subtraction { Left = (innerNode as Subtraction).Right, Right = (innerNode as Subtraction).Left };
            }
            else if (innerNode.Type == ExpressionType.Addition)
            {
                return new Addition { Left = new Negation { InnerNode = (innerNode as Addition).Right },
                    Right = new Negation { InnerNode = (innerNode as Addition).Left }
                };
            }
            return new Negation { InnerNode = innerNode};
        }
        public static Expression Simplify(Expression node)
        {
            switch (node.Type)
            {
                case ExpressionType.Negation:
                    return SimplifyNegation((Negation)node);
                case ExpressionType.Inverse:
                    return SimplifyInverse((Inverse)node);
                case ExpressionType.Addition:
                    return SimplifyAddition((Addition)node);
                case ExpressionType.Subtraction:
                    return SimplifySubtraction((Subtraction)node);
                case ExpressionType.Division:
                    return SimplifyDivision((Division)node);
                case ExpressionType.Multiplication:
                    return SimplifyMultiplication((Multiplication)node);
                case ExpressionType.Float:
                    return node;
                case ExpressionType.Variable:
                    return node;
                case ExpressionType.Function:
                    return SimplifyFunction((Function)node);
                case ExpressionType.Power:
                    return SimplifyPower((Power)node);
            }
            return node;
        }
    }

}
