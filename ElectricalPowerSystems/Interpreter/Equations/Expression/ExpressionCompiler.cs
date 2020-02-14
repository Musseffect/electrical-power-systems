using ElectricalPowerSystems.Interpreter.Equations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Interpreter.Equations.Expression
{
    class ExpressionCompiler
    {
        List<StackElement> rpnStack;
        Dictionary<string, int> variableIndicies;
        public ExpressionCompiler(Dictionary<string, int> variableIndicies)
        {
            this.variableIndicies = variableIndicies;
        }
        public RPNExpression compile(Expression root)
        {
            rpnStack = new List<StackElement>();
            compileVisitor(root);
            return new RPNExpression(rpnStack);
        }

        void compileNegation(Negation node)
        {
            compileVisitor(node.InnerNode);
            rpnStack.Add(new NegationOperator());
        }
        void compilePower(Power node)
        {
            compileVisitor(node.Left);
            compileVisitor(node.Right);
            rpnStack.Add(new PowerOperator());
        }
        void compileMultiplication(Multiplication node)
        {
            compileVisitor(node.Left);
            compileVisitor(node.Right);
            rpnStack.Add(new MultiplicationOperator());
        }
        void compileDivision(Division node)
        {
            compileVisitor(node.Left);
            compileVisitor(node.Right);
            rpnStack.Add(new DivisionOperator());
        }
        void compileFunction(Function node)
        {
            //check function signature
            foreach (var arg in node.Arguments)
                compileVisitor(arg);
            rpnStack.Add(new StackFunction(node.Entry));
        }
        void compileSubtraction(Subtraction node)
        {
            compileVisitor(node.Left);
            compileVisitor(node.Right);
            rpnStack.Add(new SubtractionOperator());
        }
        void compileAddition(Addition node)
        {
            compileVisitor(node.Left);
            compileVisitor(node.Right);
            rpnStack.Add(new AdditionOperator());
        }
        void compileVariable(Variable node)
        {
            if (variableIndicies.ContainsKey(node.Name))
            {
                rpnStack.Add(new StackVariable(variableIndicies[node.Name]));
            }
            else
            {
                throw new Exception("Couldn't find identifier index.");
            }
        }
        void compileConstant(Float node)
        {
            rpnStack.Add(new Operand(node.Value));
        }
        void compileVisitor(Expression node)
        {
            switch (node.Type)
            {
                case ExpressionType.Negation:
                    compileNegation((Negation)node);
                    break;
                case ExpressionType.Power:
                    compilePower((Power)node);
                    break;
                case ExpressionType.Multiplication:
                    compileMultiplication((Multiplication)node);
                    break;
                case ExpressionType.Division:
                    compileDivision((Division)node);
                    break;
                case ExpressionType.Function:
                    compileFunction((Function)node);
                    break;
                case ExpressionType.Subtraction:
                    compileSubtraction((Subtraction)node);
                    break;
                case ExpressionType.Addition:
                    compileAddition((Addition)node);
                    break;
                case ExpressionType.Variable:
                    compileVariable((Variable)node);
                    break;
                case ExpressionType.Float:
                    compileConstant((Float)node);
                    break;
            }
            return;
        }
    }
}
