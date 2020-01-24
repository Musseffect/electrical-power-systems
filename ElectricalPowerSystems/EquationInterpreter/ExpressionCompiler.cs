using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.EquationInterpreter
{
    class ExpressionCompiler
    {
        List<StackElement> rpnStack;
        Dictionary<string, int> variableIndicies;
        public ExpressionCompiler(Dictionary<string, int> variableIndicies)
        {
            this.variableIndicies = variableIndicies;
        }
        public RPNExpression compile(ExpressionNode root)
        {
            rpnStack = new List<StackElement>();
            compileVisitor(root);
            return new RPNExpression(rpnStack);
        }

        void compileNegation(NegationNode node)
        {
            compileVisitor(node.InnerNode);
            rpnStack.Add(new NegationOperator());
        }
        void compilePower(PowerNode node)
        {
            compileVisitor(node.Left);
            compileVisitor(node.Right);
            rpnStack.Add(new PowerOperator());
        }
        void compileMultiplication(MultiplicationNode node)
        {
            compileVisitor(node.Left);
            compileVisitor(node.Right);
            rpnStack.Add(new MultiplicationOperator());
        }
        void compileDivision(DivisionNode node)
        {
            compileVisitor(node.Left);
            compileVisitor(node.Right);
            rpnStack.Add(new DivisionOperator());
        }
        void compileFunction(FunctionEntryNode node)
        {
            //check function signature
            foreach (var arg in node.Arguments)
                compileVisitor(arg);
            rpnStack.Add(new Function(node.Entry));
        }
        void compileSubtraction(SubtractionNode node)
        {
            compileVisitor(node.Left);
            compileVisitor(node.Right);
            rpnStack.Add(new SubtractionOperator());
        }
        void compileAddition(AdditionNode node)
        {
            compileVisitor(node.Left);
            compileVisitor(node.Right);
            rpnStack.Add(new AdditionOperator());
        }
        void compileIdentifier(IdentifierNode node)
        {
            if (variableIndicies.ContainsKey(node.Value))
            {
                rpnStack.Add(new Variable(variableIndicies[node.Value]));
            }
            else
            {
                throw new Exception("Couldn't find identifier index.");
            }
        }
        void compileConstant(FloatNode node)
        {
            rpnStack.Add(new Operand(node.Value));
        }
        void compileVisitor(ExpressionNode node)
        {
            switch (node.Type)
            {
                case ASTNodeType.Negation:
                    compileNegation((NegationNode)node);
                    break;
                case ASTNodeType.Power:
                    compilePower((PowerNode)node);
                    break;
                case ASTNodeType.Multiplication:
                    compileMultiplication((MultiplicationNode)node);
                    break;
                case ASTNodeType.Division:
                    compileDivision((DivisionNode)node);
                    break;
                case ASTNodeType.FunctionEntry:
                    compileFunction((FunctionEntryNode)node);
                    break;
                case ASTNodeType.Subtraction:
                    compileSubtraction((SubtractionNode)node);
                    break;
                case ASTNodeType.Addition:
                    compileAddition((AdditionNode)node);
                    break;
                case ASTNodeType.Identifier:
                    compileIdentifier((IdentifierNode)node);
                    break;
                case ASTNodeType.Float:
                    compileConstant((FloatNode)node);
                    break;
                case ASTNodeType.Function:
                    throw new Exception("");
            }
            return;
        }
    }
}
