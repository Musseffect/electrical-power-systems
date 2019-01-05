using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace ElectricalPowerSystems
{
    class ModelAST
    {
        class ASTModelGenerator
        {
            //variable transformation float to Complex to ComplexPhase

            bool checkNumericTypes(VariableType type)
            {
                return ((int)type & 1)!=0;
            }
            Variable add(Variable left, Variable right)
            {
                if (!(checkNumericTypes(left.Type)&&checkNumericTypes(right.Type)))
                {
                    throw new Exception("Illegal type in operation");
                }
                if (left.Type > right.Type)
                {
                    right = convert(right, left.Type);
                }
                else
                    left = convert(left, right.Type);
            }
            Variable sub(Variable left, Variable right)
            {

            }
            Variable mult(Variable left, Variable right)
            {

            }
            Variable div(Variable left, Variable right)
            {

            }
            Variable eval(ExpressionNode exp)
            {
                switch (exp)
                {
                    case FloatNode:
                        return new FloatVariable {
                            Value = ((FloatNode)exp).Value
                        };
                        break;
                    case AdditionNode:
                        AdditionNode node = (AdditionNode)exp;
                        Variable left = eval(node.Left);
                        Variable right = eval(node.Right);
                        break;

                }
                return new VoidVariable();
            }
        }
    }

}
