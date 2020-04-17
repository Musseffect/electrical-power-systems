using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Equations.Expression
{
    class Compiler
    {
        List<StackElement> rpnStack;
        Dictionary<string, int> variableIndicies;
        public Compiler(Dictionary<string, int> variableIndicies)
        {
            this.variableIndicies = variableIndicies;
        }
        public RPNExpression Compile(Expression root)
        {
            rpnStack = new List<StackElement>();
            CompileVisitor(root);
            return new RPNExpression(rpnStack);
        }

        void CompileNegation(Negation node)
        {
            CompileVisitor(node.InnerNode);
            rpnStack.Add(new NegationOperator());
        }
        void CompilePower(Power node)
        {
            CompileVisitor(node.Left);
            CompileVisitor(node.Right);
            rpnStack.Add(new PowerOperator());
        }
        void CompileMultiplication(Multiplication node)
        {
            CompileVisitor(node.Left);
            CompileVisitor(node.Right);
            rpnStack.Add(new MultiplicationOperator());
        }
        void CompileDivision(Division node)
        {
            CompileVisitor(node.Left);
            CompileVisitor(node.Right);
            rpnStack.Add(new DivisionOperator());
        }
        void CompileFunction(Function node)
        {
            //check function signature
            foreach (var arg in node.Arguments)
                CompileVisitor(arg);
            rpnStack.Add(new StackFunction(node.Entry));
        }
        void CompileSubtraction(Subtraction node)
        {
            CompileVisitor(node.Left);
            CompileVisitor(node.Right);
            rpnStack.Add(new SubtractionOperator());
        }
        void CompileAddition(Addition node)
        {
            CompileVisitor(node.Left);
            CompileVisitor(node.Right);
            rpnStack.Add(new AdditionOperator());
        }
        void CompileVariable(Variable node)
        {
            if (variableIndicies.ContainsKey(node.Name))
            {
                rpnStack.Add(new StackVariable(variableIndicies[node.Name]));
            }
            else
            {
                throw new Exception("Не удалось найти индекс идентификатора");
            }
        }
        void CompileConstant(Float node)
        {
            rpnStack.Add(new Operand(node.Value));
        }
        void CompileVisitor(Expression node)
        {
            switch (node.Type)
            {
                case ExpressionType.Negation:
                    CompileNegation((Negation)node);
                    break;
                case ExpressionType.Power:
                    CompilePower((Power)node);
                    break;
                case ExpressionType.Multiplication:
                    CompileMultiplication((Multiplication)node);
                    break;
                case ExpressionType.Division:
                    CompileDivision((Division)node);
                    break;
                case ExpressionType.Function:
                    CompileFunction((Function)node);
                    break;
                case ExpressionType.Subtraction:
                    CompileSubtraction((Subtraction)node);
                    break;
                case ExpressionType.Addition:
                    CompileAddition((Addition)node);
                    break;
                case ExpressionType.Variable:
                    CompileVariable((Variable)node);
                    break;
                case ExpressionType.Float:
                    CompileConstant((Float)node);
                    break;
            }
            return;
        }
    }
}
