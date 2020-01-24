using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.EquationInterpreter
{
    public partial class EquationCompiler
    {
        void validateIdentifier(IdentifierNode node)
        {
            if (!variables.ContainsKey(node.Value))
            {
                variables.Add(node.Value,variableNames.Count);
                variableNames.Add(node.Value);
                initialValues.Add(0.0);
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
                compilerErrors.Add(new ErrorMessage("Unknown function "+node.FunctionName,node.Line,node.Position));
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
            }
            return;
            throw new Exception();
        }

    }
}
