using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Interpreter.Equations.DAE
{
#if DAE
    public partial class DAECompiler
    {
        void validateIdentifier(IdentifierNode node)
        {
            if (!variables.ContainsKey(node.Value))
            {
                variables.Add(node.Value, new Variable
                {
                    InitialValue = 0.0,
                    Name = node.Value,
                    Initialized = false,
                    VarType = Variable.Type.Algebraic,
                    Count = 0
                });
            }
        }
        void validateDerivative(DerivativeNode node)
        {
            if (node.Identifier == "t")
            {
                compilerErrors.Add(new ErrorMessage("Время не может использоваться в качестве дифференциальной переменной",node.Line,node.Position));
            }
            if (!variables.ContainsKey(node.Identifier))
            {
                variables.Add(node.Identifier, new Variable {
                    InitialValue = 0.0,
                    Name = node.Identifier,
                    Initialized = false,
                    VarType = Variable.Type.Differential,
                    Count = 0
                });
            }
            else
            {
                variables[node.Identifier].VarType = Variable.Type.Differential;
                variables[node.Identifier].Count += 1;
            }
        }
        void validateNegation(NegationNode node)
        {
            validate(node.InnerNode);
        }
        void validateAddition(AdditionNode node)
        {
            validate(node.Left);
            validate(node.Right);
        }
        void validateSubtraction(SubtractionNode node)
        {
            validate(node.Left);
            validate(node.Right);
        }
        void validateDivision(DivisionNode node)
        {
            validate(node.Left);
            validate(node.Right);
        }
        void validateMultiplication(MultiplicationNode node)
        {
            validate(node.Left);
            validate(node.Right);
        }
        void validatePower(PowerNode node)
        {
            validate(node.Left);
            validate(node.Right);
        }
        void validateFunction(FunctionNode node)
        {
            if (FunctionTable.isValidFunction(node.FunctionName))
            {
                //check number of arguments
                FunctionEntry entry = FunctionTable.getFunctionEntry(node.FunctionName);
                if (entry.ArgNumber != node.Arguments.Count)
                {
                    compilerErrors.Add(new ErrorMessage(entry.ArgNumber.ToString() + "arguments expected in function " + node.FunctionName, node.Line, node.Position));
                }
            }
            else
            {
                compilerErrors.Add(new ErrorMessage("Unknown function " + node.FunctionName, node.Line, node.Position));
            }
        }
        void validate(ExpressionNode node)
        {
            switch (node.Type)
            {
                case ASTNodeType.Negation:
                    validateNegation((NegationNode)node);
                    break;
                case ASTNodeType.Addition:
                    validateAddition((AdditionNode)node);
                    break;
                case ASTNodeType.Subtraction:
                    validateSubtraction((SubtractionNode)node);
                    break;
                case ASTNodeType.Division:
                    validateDivision((DivisionNode)node);
                    break;
                case ASTNodeType.Multiplication:
                    validateMultiplication((MultiplicationNode)node);
                    break;
                case ASTNodeType.Float:
                    break;
                case ASTNodeType.Identifier:
                    validateIdentifier((IdentifierNode)node);
                    break;
                case ASTNodeType.Function:
                    validateFunction((FunctionNode)node);
                    break;
                case ASTNodeType.Power:
                    validatePower((PowerNode)node);
                    break;
                case ASTNodeType.Derivative:
                    validateDerivative((DerivativeNode)node);
                    break;
            }
            return;
            throw new Exception();
        }
        bool hasDerivative(FunctionNode node)
        {
            bool result;
            throw new NotImplementedException();
            if (FunctionTable.isValidFunction(node.FunctionName))
            {
                //check number of arguments
                FunctionEntry entry = FunctionTable.getFunctionEntry(node.FunctionName);
                if (entry.ArgNumber != node.Arguments.Count)
                {
                    compilerErrors.Add(new ErrorMessage(entry.ArgNumber.ToString() + "arguments expected in function " + node.FunctionName, node.Line, node.Position));
                }
            }
            else
            {
                compilerErrors.Add(new ErrorMessage("Unknown function " + node.FunctionName, node.Line, node.Position));
            }
        }
        bool hasDerivative(InfixExpressionNode node)
        {
            return hasDerivative(node.Left) || hasDerivative(node.Right);
        }
        bool hasDerivative(ExpressionNode node)
        {
            switch (node.Type)
            {
                case ASTNodeType.Negation:
                    return hasDerivative(((NegationNode)node).InnerNode);
                case ASTNodeType.Addition:
                    return hasDerivative((InfixExpressionNode)node);
                case ASTNodeType.Subtraction:
                    return hasDerivative((InfixExpressionNode)node);
                case ASTNodeType.Division:
                    return hasDerivative((InfixExpressionNode)node);
                case ASTNodeType.Multiplication:
                    return hasDerivative((InfixExpressionNode)node);
                case ASTNodeType.Float:
                    return false;
                case ASTNodeType.Identifier:
                    return false;
                case ASTNodeType.Function:
                    return hasDerivative((FunctionNode)node);
                case ASTNodeType.Power:
                    return hasDerivative((InfixExpressionNode)node);
                case ASTNodeType.Derivative:
                    return true;
            }
            throw new Exception();
        }
    }
#endif
}
