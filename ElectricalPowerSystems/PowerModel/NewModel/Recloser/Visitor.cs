using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Interpreter.PowerModel.Recloser
{
    abstract class Node
    {
        public enum Type
        {
            Program,
            ProgramStatement,
            VariableDeclaration,
            FunctionDefinition,
            FunctionSignatureArgument,
            Statement,
            Expression,
            ReturnStatement,
            WhileStatement,
            SwitchStatement,
            IfStatement,
            FunctionCall,
            Identifier,
            FloatConstant,
            IntConstant,
            BoolConstant,
            Scope,
        }
        Type type;
        public Type NodeType { get { return type; } }
        public Node(Type type)
        {
            this.type = type;
        }
    }
    class ProgramNode : Node
    {
        ProgramNode(Type type) : base(type)
        {
        }
    }
    class Visitor
    {
    }
}
