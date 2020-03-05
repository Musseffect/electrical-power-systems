using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Interpreter.PowerModel
{
    class ModelExpressions
    {
    }
    /*
            switch (exp.Type)
            {
                case ASTNodeType.Assignment:
                    return assignment(exp);
                case ASTNodeType.Negation:
                    return negation(exp);
                case ASTNodeType.Addition:
                    return addition(exp);
                case ASTNodeType.Subtraction:
                    return subtraction(exp);
                case ASTNodeType.Multiplication:
                    return multiplication(exp);
                case ASTNodeType.Division:
                    return division(exp);
                case ASTNodeType.Power:
                    return power(exp);
                case ASTNodeType.Function:
                    return function(exp);
                case ASTNodeType.Member:
                    return member(exp);
                case ASTNodeType.Cast:
                    return cast(exp);
                case ASTNodeType.String:
                    return new String()
    {
        Value = ((StringNode)exp).Value
                    };
                case ASTNodeType.Float:
                    return new Float()
    {
        Value = ((FloatNode)exp).Value
                    };
                case ASTNodeType.Int:
                    return new Int()
    {
        Value = ((IntNode)exp).Value
                    };
                case ASTNodeType.Complex:
                    {
                        ComplexNode node = (ComplexNode)exp;
                        return new Complex()
    {
        Re = node.Re,
                            Im = node.Im
                        };
}
                case ASTNodeType.ComplexPhase:
                    {
                        ComplexPhaseNode node = (ComplexPhaseNode)exp;
                        return new Complex()
{
    Re = node.Magnitude * Math.Cos(node.Phase),
                            Im = node.Magnitude * Math.Sin(node.Phase),
                        };
                    }
                case ASTNodeType.Identifier:
                    {
                        IdentifierNode node = (IdentifierNode)exp;
LValueIdentifier val = new LValueIdentifier(node.Value);
                        return val;
                    }
            }
            return new VoidValue();
    }*/
}
