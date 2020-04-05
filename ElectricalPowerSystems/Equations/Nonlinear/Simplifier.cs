using ElectricalPowerSystems.Interpreter.Equations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Interpreter.Equations.Nonlinear
{
#if OLD
    //TODO свёртка констант для множественного суммирования и произведения
    public partial class EquationCompiler
    {
        ExpressionNode simplifyAddition(AdditionNode node)
        {
            ExpressionNode left = simplify(node.Left);
            ExpressionNode right = simplify(node.Right);

            if (left.Type == ASTNodeType.Float)
            {
                FloatNode nl = (FloatNode)left;
                if (nl.isZero())
                {
                    return right;
                }
                if (right.Type == ASTNodeType.Float)
                {
                    FloatNode nr = (FloatNode)right;
                    return nl + nr;
                }
            }
            if (right.Type == ASTNodeType.Float)
            {
                FloatNode nr = (FloatNode)right;
                if (nr.isZero())
                {
                    return left;
                }
            }
            return new AdditionNode { Left = left, Right = right };
        }
        ExpressionNode simplifySubtraction(SubtractionNode node)
        {
            ExpressionNode left = simplify(node.Left);
            ExpressionNode right = simplify(node.Right);

            if (left.Type == ASTNodeType.Float)
            {
                FloatNode nl = (FloatNode)left;
                if (right.Type == ASTNodeType.Float)
                {
                    FloatNode nr = (FloatNode)right;
                    return nl - nr;
                }
                if (nl.isZero())
                {
                    return new NegationNode { InnerNode = right };
                }
            }
            if (right.Type == ASTNodeType.Float)
            {
                FloatNode nr = (FloatNode)right;

                if (nr.isZero())
                {
                    return left;
                }
            }
            return new SubtractionNode { Left = left, Right = right };
        }
        ExpressionNode simplifyDivision(DivisionNode node)
        {
            ExpressionNode left = simplify(node.Left);
            ExpressionNode right = simplify(node.Right);
            if (left.Type == ASTNodeType.Float)
            {
                FloatNode nl = (FloatNode)left;
                if (nl.isZero())
                {
                    return left;
                }
                if (right.Type == ASTNodeType.Float)
                {
                    FloatNode nr = (FloatNode)right;
                    return nl / nr;
                }
            }
            if (right.Type == ASTNodeType.Float)
            {
                FloatNode nr = (FloatNode)right;
                if (nr.isOne())
                {
                    return left;
                }
            }
            return new DivisionNode { Left = left, Right = right };
        }
        ExpressionNode simplifyMultiplication(MultiplicationNode node)
        {
            ExpressionNode left = simplify(node.Left);
            ExpressionNode right = simplify(node.Right);
            if (left.Type == ASTNodeType.Float)
            {
                FloatNode nl = (FloatNode)left;
                if (nl.isZero())
                {
                    return left;
                }
                if (nl.isOne())
                {
                    return right;
                }
                if (right.Type == ASTNodeType.Float)
                {
                    FloatNode nr = (FloatNode)right;
                    return nl * nr;
                }
            }
            if (right.Type == ASTNodeType.Float)
            {
                FloatNode nr = (FloatNode)right;
                if (nr.isOne())
                {
                    return left;
                }
                if (nr.isZero())
                {
                    return right;
                }
            }
            return new MultiplicationNode { Left = left, Right = right };
        }
        ExpressionNode simplifyFunctionEntry(FunctionEntryNode node)
        {
            bool @const = true;
            for (int i = 0; i < node.Arguments.Count; i++)
            {
                node.Arguments[i] = simplify(node.Arguments[i]);
                if (node.Arguments[i].Type != ASTNodeType.Float)
                    @const = false;
            }
            if (@const)
            {
                double value;
                List<Operand> operands = new List<Operand>();
                for (int i = 0; i < node.Arguments.Count; i++)
                {
                    operands.Add(new Operand(((FloatNode)node.Arguments[i]).Value));
                }
                value = node.Entry.Exec(operands).value;
                return new FloatNode { Value = value };
            }
            return node;
        }
        ExpressionNode simplifyIdentifier(IdentifierNode node)
        {
            if (!variables.ContainsKey(node.Value))
            {
                if (parameters.ContainsKey(node.Value))
                {
                    return new FloatNode { Value = parameters[node.Value] };
                }
                else
                {
                    compilerErrors.Add(new ErrorMessage("Параметр " + node.Value + " не определён", node.Line, node.Position));
                }
            }
            return node;
        }
        ExpressionNode simplifyFunction(FunctionNode node)
        {
            bool @const=true;
            for (int i = 0; i < node.Arguments.Count; i++)
            {
                node.Arguments[i] = simplify(node.Arguments[i]);
                if (node.Arguments[i].Type != ASTNodeType.Float)
                    @const = false;
            }
            if (@const)
            {
                double value;
                FunctionEntry entry = FunctionTable.getFunctionEntry(node.FunctionName);
                List<Operand> operands = new List<Operand>();
                for (int i = 0; i < node.Arguments.Count; i++)
                {
                    operands.Add(new Operand(((FloatNode)node.Arguments[i]).Value));
                }
                value = entry.Exec(operands).value;
                return new FloatNode { Value = value};
            }
            return new FunctionEntryNode(FunctionTable.getFunctionEntry(node.FunctionName),node.Arguments);
        }
        ExpressionNode simplifyPower(PowerNode node)
        {
            ExpressionNode left = simplify(node.Left);
            ExpressionNode right = simplify(node.Right);
            if (left.Type == ASTNodeType.Float)
            {
                FloatNode nl = (FloatNode)left;
                if (nl.isZero() || nl.isOne())
                {
                    return left;
                }
                if (right.Type == ASTNodeType.Float)
                {
                    FloatNode nr = (FloatNode)right;
                    return FloatNode.pow(nl, nr);
                }
            }
            if (right.Type == ASTNodeType.Float)
            {
                FloatNode nr = (FloatNode)right;
                if (nr.isZero())
                {
                    return new FloatNode { Value = 1.0 };
                }
                if (nr.isOne())
                {
                    return left;
                }
            }
            return new PowerNode { Left = left, Right = right };
        }
        ExpressionNode simplifyNegation(NegationNode node)
        {
            ExpressionNode innerNode = simplify(node.InnerNode);
            if (innerNode.Type == ASTNodeType.Float)
                return -((FloatNode)innerNode);
            else if (innerNode.Type == ASTNodeType.Negation)
            {
                return ((NegationNode)innerNode).InnerNode;
            }
            return node;
        }
        ExpressionNode simplify(ExpressionNode node)
        {
            switch (node.Type)
            {
                case ASTNodeType.Negation:
                    return simplifyNegation((NegationNode)node);
                case ASTNodeType.Addition:
                    return simplifyAddition((AdditionNode)node);
                case ASTNodeType.Subtraction:
                    return simplifySubtraction((SubtractionNode)node);
                case ASTNodeType.Division:
                    return simplifyDivision((DivisionNode)node);
                case ASTNodeType.Multiplication:
                    return simplifyMultiplication((MultiplicationNode)node);
                case ASTNodeType.Float:
                    return node;
                case ASTNodeType.Identifier:
                    return simplifyIdentifier((IdentifierNode)node);
                case ASTNodeType.Function:
                    return simplifyFunction((FunctionNode)node);
                case ASTNodeType.FunctionEntry:
                    return simplifyFunctionEntry((FunctionEntryNode)node);
                case ASTNodeType.Power:
                    return simplifyPower((PowerNode)node);
            }
            return node;
        }
    }
#endif
}
