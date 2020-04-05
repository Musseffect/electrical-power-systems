using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Equations.Expression
{
    struct PrintElement
    {
        public enum ElementType
        {
            Identifier=5,
            Float=5,
            Addition=1,
            Negation=2,
            Multiplication=3,
            Power=4,
            Function=5
        }
        public ElementType Type { get; set; }
        public string String { get; set; }
    };
    public class RPNExpression
    {
        List<StackElement> rpn;
        public RPNExpression(List<StackElement> rpn)
        {
            this.rpn = rpn;
        }
        static public string Print(RPNExpression exp,string [] variableNames)
        {
            List<StackElement> rpn = exp.rpn;
            Stack<PrintElement> operands = new Stack<PrintElement>();
            for (int i = 0; i < rpn.Count; i++)
            {
                switch (rpn[i].Type)
                {
                    case StackElementType.Negation:
                        {
                            PrintElement operand = operands.Pop();
                            if (operand.Type < PrintElement.ElementType.Negation)
                                operands.Push(new PrintElement
                                {
                                    Type = PrintElement.ElementType.Negation,
                                    String = "-(" + operand.String + ")"
                                });
                            else
                                operands.Push(new PrintElement
                                {
                                    Type = PrintElement.ElementType.Negation,
                                    String = "-" + operand.String
                                });
                            break;
                        }
                    case StackElementType.Addition:
                        {
                            PrintElement right = operands.Pop();
                            PrintElement left = operands.Pop();
                            if (left.Type < PrintElement.ElementType.Addition)
                                left.String = "(" + left.String + ")";
                            if (right.Type < PrintElement.ElementType.Addition)
                                right.String = "(" + right.String + ")";
                            operands.Push(new PrintElement
                            {
                                Type = PrintElement.ElementType.Addition,
                                String = left.String + "+" + right.String
                            });
                            break;
                        }
                    case StackElementType.Subtraction:
                        {
                            PrintElement right = operands.Pop();
                            PrintElement left = operands.Pop();
                            if (left.Type < PrintElement.ElementType.Addition)
                                left.String = "(" + left.String + ")";
                            if (right.Type <= PrintElement.ElementType.Addition)
                                right.String = "(" + right.String + ")";
                            operands.Push(new PrintElement
                            {
                                Type = PrintElement.ElementType.Addition,
                                String = left.String + "-" + right.String
                            });
                            break;
                        }
                    case StackElementType.Multiplication:
                        {
                            PrintElement right = operands.Pop();
                            PrintElement left = operands.Pop();
                            if (left.Type < PrintElement.ElementType.Multiplication)
                                left.String = "(" + left.String + ")";
                            if (right.Type < PrintElement.ElementType.Multiplication)
                                right.String = "(" + right.String + ")";
                            operands.Push(new PrintElement
                            {
                                Type = PrintElement.ElementType.Multiplication,
                                String = left.String + "*" + right.String
                            });
                            break;
                        }
                    case StackElementType.Division:
                        {
                            PrintElement right = operands.Pop();
                            PrintElement left = operands.Pop();
                            if (left.Type < PrintElement.ElementType.Multiplication)
                                left.String = "(" + left.String + ")";
                            if (right.Type < PrintElement.ElementType.Multiplication)
                                right.String = "(" + right.String + ")";
                            operands.Push(new PrintElement
                            {
                                Type = PrintElement.ElementType.Multiplication,
                                String = left.String + "/" + right.String
                            });
                            break;
                        }
                    case StackElementType.Power:
                        {
                            PrintElement right = operands.Pop();
                            PrintElement left = operands.Pop();
                            if (left.Type <= PrintElement.ElementType.Power)
                                left.String = "(" + left.String + ")";
                            if (right.Type < PrintElement.ElementType.Power)
                                right.String = "(" + right.String + ")";
                            operands.Push(new PrintElement
                            {
                                Type = PrintElement.ElementType.Multiplication,
                                String = left.String + "^" + right.String
                            });
                            break;
                        }
                    case StackElementType.Variable:
                        {
                            operands.Push(new PrintElement
                            {
                                Type = PrintElement.ElementType.Identifier,
                                String = variableNames[((StackVariable)rpn[i]).index]
                            });
                            break;
                        }
                    case StackElementType.Function:
                        {
                            StackFunction func = (StackFunction)rpn[i];
                            FunctionEntry entry = func.GetFunctionEntry();
                            string @string = "";
                            for (int j = 0; j < entry.ArgNumber; j++)
                            {
                                PrintElement operand = operands.Pop();
                                if (j != 0)
                                    @string = ", " + @string;
                                @string = operand.String + @string;
                            }
                            operands.Push(new PrintElement
                            {
                                Type = PrintElement.ElementType.Function,
                                String = entry.FuncName + "(" + @string + ")"
                            });
                            break;
                        }
                    case StackElementType.Operand:
                        {
                            PrintElement element = new PrintElement();
                            element.Type = PrintElement.ElementType.Float;
                            element.String = Convert.ToString(((Operand)rpn[i]).value);
                            operands.Push(element);
                            break;
                        }
                }
            }
            return operands.Pop().String;
        }
        static public string Print(RPNExpression exp)
        {
            List<StackElement> rpn = exp.rpn;
            Stack<PrintElement> operands = new Stack<PrintElement>();
            for (int i = 0; i < rpn.Count; i++)
            {
                switch (rpn[i].Type)
                {
                    case StackElementType.Negation:
                        {
                            PrintElement operand = operands.Pop();
                            if (operand.Type < PrintElement.ElementType.Negation)
                                operands.Push(new PrintElement
                                {
                                    Type = PrintElement.ElementType.Negation,
                                    String = "-(" + operand.String + ")"
                                });
                            else
                                operands.Push(new PrintElement
                                {
                                    Type = PrintElement.ElementType.Negation,
                                    String = "-" + operand.String
                                });
                            break;
                        }
                    case StackElementType.Addition:
                        {
                            PrintElement right = operands.Pop();
                            PrintElement left = operands.Pop();
                            if (left.Type < PrintElement.ElementType.Addition)
                                left.String = "(" + left.String + ")";
                            if (right.Type < PrintElement.ElementType.Addition)
                                right.String = "(" + right.String + ")";
                            operands.Push(new PrintElement
                            {
                                Type = PrintElement.ElementType.Addition,
                                String = left.String + "+" + right.String
                            });
                            break;
                        }
                    case StackElementType.Subtraction:
                        {
                            PrintElement right = operands.Pop();
                            PrintElement left = operands.Pop();
                            if (left.Type < PrintElement.ElementType.Addition)
                                left.String = "(" + left.String + ")";
                            if (right.Type <= PrintElement.ElementType.Addition)
                                right.String = "(" + right.String + ")";
                            operands.Push(new PrintElement
                            {
                                Type = PrintElement.ElementType.Addition,
                                String = left.String + "-" + right.String
                            });
                            break;
                        }
                    case StackElementType.Multiplication:
                        {
                            PrintElement right = operands.Pop();
                            PrintElement left = operands.Pop();
                            if (left.Type < PrintElement.ElementType.Multiplication)
                                left.String = "(" + left.String + ")";
                            if (right.Type < PrintElement.ElementType.Multiplication)
                                right.String = "(" + right.String + ")";
                            operands.Push(new PrintElement
                            {
                                Type = PrintElement.ElementType.Multiplication,
                                String = left.String + "*" + right.String
                            });
                            break;
                        }
                    case StackElementType.Division:
                        {
                            PrintElement right = operands.Pop();
                            PrintElement left = operands.Pop();
                            if (left.Type < PrintElement.ElementType.Multiplication)
                                left.String = "(" + left.String + ")";
                            if (right.Type <= PrintElement.ElementType.Multiplication)
                                right.String = "(" + right.String + ")";
                            operands.Push(new PrintElement
                            {
                                Type = PrintElement.ElementType.Multiplication,
                                String = left.String + "/" + right.String
                            });
                            break;
                        }
                    case StackElementType.Power:
                        {
                            PrintElement right = operands.Pop();
                            PrintElement left = operands.Pop();
                            if (left.Type <= PrintElement.ElementType.Power)
                                left.String = "(" + left.String + ")";
                            if (right.Type < PrintElement.ElementType.Power)
                                right.String = "(" + right.String + ")";
                            operands.Push(new PrintElement
                            {
                                Type = PrintElement.ElementType.Multiplication,
                                String = left.String + "/" + right.String
                            });
                            break;
                        }
                    case StackElementType.Variable:
                        {
                            operands.Push(new PrintElement
                            {
                                Type = PrintElement.ElementType.Identifier,
                                String = "var[" + ((StackVariable)rpn[i]).index.ToString()+"]"
                            });
                            break;
                        }
                    case StackElementType.Function:
                        {
                            StackFunction func = (StackFunction)rpn[i];
                            FunctionEntry entry = func.GetFunctionEntry();
                            string @string = "";
                            for (int j = 0; j < entry.ArgNumber; j++)
                            {
                                PrintElement operand = operands.Pop();
                                if (j != 0)
                                    @string = ", " + @string;
                                @string = operand.String + @string;
                            }
                            operands.Push(new PrintElement
                            {
                                Type = PrintElement.ElementType.Function,
                                String = entry.FuncName + "(" + @string + ")"
                            });
                            break;
                        }
                    case StackElementType.Operand:
                        {
                            PrintElement element = new PrintElement();
                            element.Type = PrintElement.ElementType.Float;
                            element.String = Convert.ToString(((Operand)rpn[i]).value);
                            operands.Push(element);
                            break;
                        }
                }
            }
            return operands.Pop().String;
        }
        public double Execute(double[] variables)
        {
            Stack<Operand> operands = new Stack<Operand>();
            for (int i = 0; i < rpn.Count; i++)
            {
                if (rpn[i].Type == StackElementType.Negation)
                {
                    operands.Push(((NegationOperator)rpn[i]).Exec(operands.Pop()));
                }
                else if ((rpn[i].Type & StackElementType.Addition) == StackElementType.Addition)
                {
                    Operand right = operands.Pop();
                    Operand left = operands.Pop();
                    operands.Push(((BinaryOperator)rpn[i]).Exec(left,right));
                }
                else if (rpn[i].Type == StackElementType.Function)
                {
                    ((StackFunction)rpn[i]).Exec(operands);
                }
                else if (rpn[i].Type == StackElementType.Variable)
                {
                    operands.Push(new Operand(variables[((StackVariable)rpn[i]).index]));
                }
                else
                {
                    operands.Push((Operand)rpn[i]);
                }
            }
            return operands.Pop().value;
        }
    }
}
