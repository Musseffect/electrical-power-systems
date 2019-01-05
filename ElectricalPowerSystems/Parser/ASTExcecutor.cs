using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace ElectricalPowerSystems.Parser
{
    class ASTExcecutor
    {
        public enum VariableType
        {
            String = 0,
            Float = 1,
            Element = 2,
            Complex = 3,
            Node = 4,
            Int = 5,
            Void = 6,
            Any = -2,
            MemberLValue = 8,
            IdentifierLValue = 10
        }
        static public VariableType max(VariableType a, VariableType b)
        {
            if (a == VariableType.Any || b == VariableType.Any)
                return VariableType.Any;
            if (a == b)
                return a;
            if ((a == VariableType.Float && b == VariableType.Complex )||(a == VariableType.Complex && b == VariableType.Float))
            {
                return VariableType.Complex;
            }
            throw new Exception("Incorrect type conversion");
        }
        static Variable convert(Variable a, VariableType t)
        {
            if (t == VariableType.Any)
                return a;
            if (a.Type == t)
                return a;
            if (a.Type == VariableType.Float && t == VariableType.Complex)
            {
                return new ComplexVariable {
                    Re=((FloatVariable)a).Value,
                    Im=0.0
                };
            }
            throw new Exception("Incorrect type conversion");
        }
        abstract class Class
        {
            public abstract string getName();
            public abstract Object getMember(string memberName);
            public abstract void setMember(MemberLValueVariable member, Object var);
        }
        class DictionaryClass:Class
        {
            public string className;
            public string getName()
            {
                return className
            }
            public Object getMember(string memberName)
            {
                try {
                    Object obj = Properties[memberName];
                    return obj;
                } catch (Exception)
                {
                    throw new Exception("");
                }
            }
            public Dictionary<string, Object> Properties { get; set; }
        }
        class BasicClass:Class
        {
        }
        List<Class> classes = new List<Class>{
            new DictionaryClass{
                className = "Resistor",
                Properties = new Dictionary<string, Object>{
                    new {}
                },
            },
            ,
        };
        class Object
        {
            public Class Class { get; protected set; }
        }
        class MemberLValueVariable : Variable
        {
            public Object ParentObject { get; set; }
            public string Name { get; set; }
        }
        class VoidVariable : Variable
        {
            public VoidVariable()
            {
                type = VariableType.Void;
            }
        }
        class Variable
        {
            protected VariableType type;
            public VariableType Type
            {
                get
                {
                    return type;
                }
            }
        }
        class ComplexVariable : Variable
        {
            public double Re { get; set; }
            public double Im { get; set; }
            public double Magn { get {
                    return Math.Sqrt(Re*Re+Im*Im);
                } }
            public double Phase { get
                {
                    return Math.Atan2(Im,Re);
                } }
            public ComplexVariable()
            {
                type = VariableType.Complex;
            }
            public ComplexVariable addSelf(ComplexVariable b)
            {
                Re += b.Re;
                Im += b.Im;
                return this;
            }
            public ComplexVariable subSelf(ComplexVariable b)
            {
                Re -= b.Re;
                Im -= b.Im;
                return this;
            }
            public ComplexVariable negate()
            {
                Re = -Re;
                Im = -Im;
                return this;
            }
            public ComplexVariable multSelf(ComplexVariable b)
            {
                double r = Re;
                Re = Re*b.Re - Im*b.Im;
                Im = r*b.Im+Im*b.Re;
                return this;
            }
            public ComplexVariable conj()
            {
                Im = -Im;
                return this;
            }
            public ComplexVariable divSelf(ComplexVariable b)
            {
                double r = Re;
                double l = b.Re * b.Re + b.Im + b.Im;
                Re = Re * b.Re + Im * b.Im;
                Im = - r * b.Im + Im * b.Re;
                Re /= l;
                Im /= l;
                return this;
            }
            public StringVariable toString()
            {
                return new StringVariable
                {
                    Value = Re.ToString() + " + j" + Im.ToString()
                };
            }

        }
        class IntVariable : Variable
        {
            public int Value { get; set; }
            public IntVariable()
            {
                type = VariableType.Int;
            }
            public IntVariable addSelf(IntVariable b)
            {
                Value += b.Value;
                return this;
            }
            public IntVariable subSelf(IntVariable b)
            {
                Value -= b.Value;
                return this;
            }
            public IntVariable negate()
            {
                Value = -Value;
                return this;
            }
            public IntVariable multSelf(IntVariable b)
            {
                Value *= b.Value;
                return this;
            }
            public IntVariable divSelf(IntVariable b)
            {
                Value /= b.Value;
                return this;
            }
            public ComplexVariable toComplex()
            {
                return new ComplexVariable
                {
                    Re = Value,
                    Im = 0.0
                };
            }
            public FloatVariable toFloat()
            {
                return new FloatVariable
                {
                    Value = Value
                };
            }
            public StringVariable toString()
            {
                return new StringVariable
                {
                    Value = Value.ToString()
                };
            }
        }
        class FloatVariable : Variable
        {
            public double Value { get; set; }
            public FloatVariable()
            {
                type = VariableType.Float;
            }
            public FloatVariable addSelf(FloatVariable b)
            {
                Value += b.Value;
                return this;
            }
            public FloatVariable subSelf(FloatVariable b)
            {
                Value -= b.Value;
                return this;
            }
            public FloatVariable negate()
            {
                Value = -Value;
                return this;
            }
            public FloatVariable multSelf(FloatVariable b)
            {
                Value *= b.Value;
                return this;
            }
            public FloatVariable divSelf(FloatVariable b)
            {
                Value /= b.Value;
                return this;
            }
            public FloatVariable powerSelf(FloatVariable b)
            {
                Value = Math.Pow(Value,b.Value);
                return this;
            }
            public ComplexVariable toComplex()
            {
                return new ComplexVariable
                {
                    Re = Value,
                    Im = 0.0
                };
            }
            public StringVariable toString()
            {
                return new StringVariable
                {
                    Value = Value.ToString()
                };
            }
        }
        class ElementVariable : Variable
        {
            public int Id { get; set; }
            public ElementVariable()
            {
                type = VariableType.Element;
            }
        }
        class NodeVariable : Variable
        {
            public int Id { get; set; }
            public NodeVariable()
            {
                type = VariableType.Node;
            }
        }
        class StringVariable : Variable
        {
            public string Value { get; set; }
            public StringVariable()
            {
                type = VariableType.String;
            }
        }
        Variable convertTypes(Variable a, VariableType b)
        {
            switch (a.Type)
            {
                case VariableType.Float:
                    FloatVariable var = (FloatVariable)a;
                    if (b == VariableType.Complex)
                        return new ComplexVariable {
                            Re = var.Value,
                            Im = 0.0
                        };
                    break;
                default:
                    throw new Exception("Invalid type convertion");
            }
            return a;
        }
        Dictionary<string, Variable> variableTable;
        ModelGraphCreatorAC modelGraph;
        List<string> errorLog;
        private Variable assignment(ExpressionNode exp)
        {
            AssignmentNode node = (AssignmentNode)exp;
            Variable var=evaluate(node.Right);
            variableTable[node.Id.Value]=var;
            return var;
        }
        private Variable negation(ExpressionNode exp)
        {
            NegationNode node = (NegationNode)exp;
            Variable var = evaluate(node.InnerNode);
            if (var.Type == VariableType.Float)
            {
                FloatVariable v = (FloatVariable)var;
                v.Value = -v.Value;
                return v;
            }
            else if (var.Type == VariableType.Complex)
            {
                ComplexVariable v = (ComplexVariable)var;
                v.Re = -v.Re;
                v.Im = -v.Im;
                return v;
            }
            throw new Exception("Incorrect type");
        }
        private Variable addition(ExpressionNode exp)
        {
            AdditionNode node = (AdditionNode)exp;
            Variable left = evaluate(node.Left);
            Variable right = evaluate(node.Right);
            VariableType type=max(left.Type,right.Type);
            left = convert(left,type);
            right = convert(right, type);
            if (type == VariableType.Float)
            {
                FloatVariable l = (FloatVariable)left;
                FloatVariable r = (FloatVariable)right;
                return new FloatVariable
                {
                    Value = l.Value+r.Value
                };
            }
            else if (type == VariableType.Complex)
            {
                ComplexVariable l = (ComplexVariable)left;
                ComplexVariable r = (ComplexVariable)right;
                return new ComplexVariable
                {
                    Re=l.Re+r.Re,
                    Im=l.Im+r.Im
                };
            }
            else if (type == VariableType.String)
            {
                string l = ((StringVariable)left).Value;
                string r = ((StringVariable)right).Value;
                return new StringVariable
                {
                    Value = ((StringVariable)left).Value = l + r
                };
            }
            throw new Exception("Incorrect type");
        }
        private Variable subtraction(ExpressionNode exp)
        {
            SubtractionNode node = (SubtractionNode)exp;
            Variable left = evaluate(node.Left);
            Variable right = evaluate(node.Right);
            VariableType type = max(left.Type, right.Type);
            left = convert(left, type);
            right = convert(right, type);
            if (type == VariableType.Float)
            {
                FloatVariable l = (FloatVariable)left;
                FloatVariable r = (FloatVariable)right;
                return l.subSelf(r);
            }
            else if (type == VariableType.Complex)
            {
                ComplexVariable l = (ComplexVariable)left;
                ComplexVariable r = (ComplexVariable)right;
                return l.subSelf(r);
            }
            throw new Exception("Incorrect type");
        }
        private Variable multiplication(ExpressionNode exp)
        {
            MultiplicationNode node = (MultiplicationNode)exp;
            Variable left = evaluate(node.Left);
            Variable right = evaluate(node.Right);
            VariableType type = max(left.Type, right.Type);
            left = convert(left, type);
            right = convert(right, type);
            if (type == VariableType.Float)
            {
                FloatVariable l = (FloatVariable)left;
                FloatVariable r = (FloatVariable)right;
                return l.multSelf(r);
            }
            else if (type == VariableType.Complex)
            {
                ComplexVariable l = (ComplexVariable)left;
                ComplexVariable r = (ComplexVariable)right;
                return l.multSelf(r);
            }
            throw new Exception("Incorrect type");
        }
        private Variable division(ExpressionNode exp)
        {
            DivisionNode node = (DivisionNode)exp;
            Variable left = evaluate(node.Left);
            Variable right = evaluate(node.Right);
            VariableType type = max(left.Type, right.Type);
            left = convert(left, type);
            right = convert(right, type);
            if (type == VariableType.Float)
            {
                FloatVariable l = (FloatVariable)left;
                FloatVariable r = (FloatVariable)right;
                return l.divSelf(r);
            }
            else if (type == VariableType.Complex)
            {
                ComplexVariable l = (ComplexVariable)left;
                ComplexVariable r = (ComplexVariable)right;
                return l.divSelf(r);
            }
            throw new Exception("Incorrect type");
        }
        private Variable power(ExpressionNode exp)
        {
            SubtractionNode node = (SubtractionNode)exp;
            Variable left = evaluate(node.Left);
            Variable right = evaluate(node.Right);
            VariableType type = max(left.Type, right.Type);
            left = convert(left, type);
            right = convert(right, type);
            if (type == VariableType.Float)
            {
                FloatVariable l = (FloatVariable)left;
                FloatVariable r = (FloatVariable)right;
                return l.subSelf(r);
            }
            throw new Exception("Incorrect type");
        }
        private Variable function(ExpressionNode exp)
        {
            //find function
            //check all signatures
            //if success return value
            //else error
        }
        private Variable member(ExpressionNode exp)
        {

        }
        private Variable cast(ExpressionNode exp)
        {

        }
        private Variable evaluate(ExpressionNode exp)
        {
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
                    return new StringVariable
                    {
                        Value = ((StringNode)exp).Value
                    };
                case ASTNodeType.Float:
                    return new FloatVariable {
                        Value = ((FloatNode)exp).Value
                    };
                case ASTNodeType.Int:
                    return new IntVariable
                    {

                    };
                case ASTNodeType.Complex:
                    {
                        ComplexNode node = (ComplexNode)exp;
                        return new ComplexVariable
                        {
                            Re = node.Re,
                            Im = node.Im
                        };
                    }
                case ASTNodeType.ComplexPhase:
                    {
                        ComplexPhaseNode node = (ComplexPhaseNode)exp;
                        return new ComplexVariable
                        {
                            Re = node.Magnitude*Math.Cos(node.Phase),
                            Im = node.Magnitude*Math.Sin(node.Phase),
                        };
                    }
                case ASTNodeType.Identifier:
                    {
                        Variable t;
                        IdentifierNode node = (IdentifierNode)exp;
                        try
                        {
                            t = variableTable[node.Value];
                        }
                        catch (KeyNotFoundException)
                        {
                            throw new Exception("Non-existing identifier " + node.Value);
                        }
                        return t;
                    }
            }
            return new VoidVariable();
        }
        public List<string> getErrorLog()
        {
            return errorLog;
        }
        public ModelGraphCreatorAC generate(ASTNode ast)
        {
            modelGraph = new ModelGraphCreatorAC();
            variableTable = new Dictionary<string, Variable>();
            errorLog = new List<string>();
            ModelNode root = (ModelNode)ast;
            foreach (StatementNode node in root.Statements)
            {
                try
                {
                    evaluate(node.Expression);
                }
                catch (Exception exc)
                {
                    errorLog.Add(exc.Message);
                }
            }
            return modelGraph;
        }
    }
}