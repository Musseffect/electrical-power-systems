using ElectricalPowerSystems.ACGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ElectricalPowerSystems.Interpreter.PowerModel
{
    public class ASTInterpreter
    {
        static public Dictionary<string, Type> basicTypes = new Dictionary<string, Type>
            {
                { "Int",new BaseType(BasicType.Int,"Int") },
                { "Float", new BaseType(BasicType.Float,"Float") },
                { "Complex", new BaseType(BasicType.Complex,"Complex") },
                { "String", new BaseType(BasicType.String,"String") },
                { "Element", new BaseType(BasicType.Element,"Element") },
                { "Void", new BaseType(BasicType.Void,"Void") }
            };
        public Dictionary<string, Object> variableTable;
        CircuitModelAC model;

        PowerGraph.PowerGraphManager graph;
        public ASTInterpreter()
        {
        }
        public CircuitModelAC generate(ASTNode ast,ref List<ErrorMessage> errorList,ref List<string> output)
        {
            model = new CircuitModelAC();
            graph = new PowerGraph.PowerGraphManager();
            PowerGraph.PowerGraphManager.powerFrequency = (float)(60.0 * 2.0 * Math.PI);
            FunctionStorage.model = model;
            FunctionStorage.powerModel = graph;
            FunctionStorage.output = output;
            variableTable = new Dictionary<string, Object>();
            LValueIdentifier.variableTable = variableTable;
            //errorLog = new List<string>();
            ModelNode root = (ModelNode)ast;
            foreach (StatementNode node in root.Statements)
            {
                try
                {
                    evaluate(node.Expression);
                }
                catch (ModelInterpreterException exc)
                {
                    Console.WriteLine(exc.Message);
                    errorList.Add(new ErrorMessage(exc.Message, exc.Line,exc.Position));
                } catch (Exception exc)
                {
                    Console.WriteLine(exc.Message);
                    errorList.Add(new ErrorMessage(exc.Message));
                }
            }
            return model;
        }
        private Value assignment(ExpressionNode exp)
        {
            AssignmentNode node = (AssignmentNode)exp;
            Value left = evaluate(node.Left);
            Object right=null;
            try
            {
                right = evaluate(node.Right).getRValue();
            }
            catch (ModelInterpreterException exc)
            {
                throw exc;
            }
            catch (Exception exc)
            {
                throw new ModelInterpreterException(exc.Message){
                    Line = node.Left.Position,
                    Position = node.Right.Position
                };
            }
            if (left is LValue)
            {
                if (right is Void)
                    throw new ModelInterpreterException("Cannot assign void.")
                    {
                        Line = node.Right.Line,
                        Position = node.Right.Position
                    };
                LValueIdentifier l = (LValueIdentifier)left;
                l.setValue((Object)right);
                return right;
            }
            throw new ModelInterpreterException("Left part of assignment should be LValue.")
            {
                Line = node.Left.Line,
                Position = node.Left.Position
            };
        }
        private Value negation(ExpressionNode exp)
        {
            NegationNode node = (NegationNode)exp;
            Value val=evaluate(node.InnerNode);
            Object obj = val.getRValue();
            Type type = obj.getType();
            if (type is BaseType)
            {
                BaseType btype = (BaseType)type;
                switch (btype.TypeEnum)
                {
                    case BasicType.Complex:
                        return Complex.opNeg((Complex)obj);
                    case BasicType.Int:
                        return Int.opNeg((Int)obj);
                    case BasicType.Float:
                        return Float.opNeg((Float)obj);
                    default:
                        throw new ModelInterpreterException("Invalid type")
                        {
                            Line = node.Line,
                            Position = node.Position
                        };
                }
            }
            throw new ModelInterpreterException("Operator isn't available for custom types.")
            {
                Line = node.Line,
                Position = node.Position
            };
        }
        private Value addition(ExpressionNode exp)
        {
            AdditionNode node = (AdditionNode)exp;
            try
            {
                Object left = evaluate(node.Left).getRValue();
                Object right = evaluate(node.Right).getRValue();
                Type leftType = left.getType();
                Type rightType = right.getType();
                BaseType bt = (BaseType)max(leftType, rightType);
                switch (bt.TypeEnum)
                {
                    case BasicType.Complex:
                        {
                            Complex l = (Complex)convert(left, bt, leftType);
                            Complex r = (Complex)convert(right, bt, rightType);
                            return Complex.opAdd(l, r);
                        }
                    case BasicType.Int:
                        {
                            Int l = (Int)convert(left, bt, leftType);
                            Int r = (Int)convert(right, bt, rightType);
                            return Int.opAdd(l, r);
                        }
                    case BasicType.Float:
                        {
                            Float l = (Float)convert(left, bt, leftType);
                            Float r = (Float)convert(right, bt, rightType);
                            return Float.opAdd(l, r);
                        }
                    case BasicType.String:
                        {
                            String l = (String)convert(left, bt, leftType);
                            String r = (String)convert(right, bt, rightType);
                            return String.opAdd(l, r);
                        }
                    default:
                        throw new ModelInterpreterException("Invalid type \"" + bt.TypeName + "\".")
                        {
                            Line = node.Line,
                            Position = node.Position
                        };
                }
            }
            catch (ModelInterpreterException exc)
            {
                throw exc;
            }
            catch (Exception exc)
            {
                throw new ModelInterpreterException(exc.Message) {
                    Line = node.Line,
                    Position = node.Position
                };
            }
            throw new ModelInterpreterException("Operator isn't available for custom types.")
            {
                Line = node.Line,
                Position = node.Position
            };
        }
        private Value subtraction(ExpressionNode exp)
        {
            SubtractionNode node = (SubtractionNode)exp;
            try { 
                Object left = evaluate(node.Left).getRValue();
                Object right = evaluate(node.Right).getRValue();
                Type leftType = left.getType();
                Type rightType = right.getType();
                BaseType bt = (BaseType)max(leftType, rightType);
                switch (bt.TypeEnum)
                {
                    case BasicType.Complex:
                        {
                            Complex l = (Complex)convert(left, bt, leftType);
                            Complex r = (Complex)convert(right, bt, rightType);
                            return Complex.opSub(l, r);
                        }
                    case BasicType.Int:
                        {
                            Int l = (Int)convert(left, bt, leftType);
                            Int r = (Int)convert(right, bt, rightType);
                            return Int.opSub(l, r);
                        }
                    case BasicType.Float:
                        {
                            Float l = (Float)convert(left, bt, leftType);
                            Float r = (Float)convert(right, bt, rightType);
                            return Float.opSub(l, r);
                        }
                    default:
                        throw new ModelInterpreterException("Invalid type \""+bt.TypeName + "\".")
                        {
                            Line = node.Line,
                            Position = node.Position
                        };
                }
            }
            catch (ModelInterpreterException exc)
            {
                throw exc;
            }
            catch (Exception exc)
            {
                throw new ModelInterpreterException(exc.Message)
                {
                    Line = node.Line,
                    Position = node.Position
                };
            }
            throw new ModelInterpreterException("Operator isn't available for custom types.")
            {
                Line = node.Line,
                Position = node.Position
            };
        }
        private Value multiplication(ExpressionNode exp)
        {
            MultiplicationNode node = (MultiplicationNode)exp;
            try {
                Object left = evaluate(node.Left).getRValue();
                Object right = evaluate(node.Right).getRValue();
                Type leftType = left.getType();
                Type rightType = right.getType();
                BaseType bt = (BaseType)max(leftType, rightType);
                switch (bt.TypeEnum)
                {
                    case BasicType.Complex:
                        {
                            Complex l = (Complex)convert(left, bt, leftType);
                            Complex r = (Complex)convert(right, bt, rightType);
                            return Complex.opMult(l, r);
                        }
                    case BasicType.Int:
                        {
                            Int l = (Int)convert(left, bt, leftType);
                            Int r = (Int)convert(right, bt, rightType);
                            return Int.opMult(l, r);
                        }
                    case BasicType.Float:
                        {
                            Float l = (Float)convert(left, bt, leftType);
                            Float r = (Float)convert(right, bt, rightType);
                            return Float.opMult(l, r);
                        }
                    default:
                        throw new ModelInterpreterException("Invalid type \"" + bt.TypeName + "\".")
                        {
                            Line = node.Line,
                            Position = node.Position
                        };
                }
            }
            catch (ModelInterpreterException exc)
            {
                throw exc;
            }
            catch (Exception exc)
            {
                throw new ModelInterpreterException(exc.Message)
                {
                    Line = node.Line,
                    Position = node.Position
                };
            }
            throw new ModelInterpreterException("Operator isn't available for custom types.")
            {
                Line = node.Line,
                Position = node.Position
            };
        }
        private Value division(ExpressionNode exp)
        {
            DivisionNode node = (DivisionNode)exp;
            try
            {
                Object left = evaluate(node.Left).getRValue();
                Object right = evaluate(node.Right).getRValue();
                Type leftType = left.getType();
                Type rightType = right.getType();
                BaseType bt = (BaseType)max(leftType, rightType);
                switch (bt.TypeEnum)
                {
                    case BasicType.Complex:
                        {
                            Complex l = (Complex)convert(left, bt, leftType);
                            Complex r = (Complex)convert(right, bt, rightType);
                            return Complex.opDiv(l, r);
                        }
                    case BasicType.Int:
                        {
                            Int l = (Int)convert(left, bt, leftType);
                            Int r = (Int)convert(right, bt, rightType);
                            return Int.opDiv(l, r);
                        }
                    case BasicType.Float:
                        {
                            Float l = (Float)convert(left, bt, leftType);
                            Float r = (Float)convert(right, bt, rightType);
                            return Float.opDiv(l, r);
                        }
                    default:
                        throw new ModelInterpreterException("Invalid type \"" + bt.TypeName + "\".")
                        {
                            Line = node.Line,
                            Position = node.Position
                        };
                }
            }
            catch (ModelInterpreterException exc)
            {
                throw exc;
            }
            catch (Exception exc)
            {
                throw new ModelInterpreterException(exc.Message)
                {
                    Line = node.Line,
                    Position = node.Position
                };
            }
            throw new ModelInterpreterException("Operator isn't available for custom types.")
            {
                Line = node.Line,
                Position = node.Position
            };
        }
        private Value power(ExpressionNode exp)
        {
            PowerNode node = (PowerNode)exp;
            try
            {

                Object left = evaluate(node.Left).getRValue();
                Object right = evaluate(node.Right).getRValue();
                Type leftType = left.getType();
                Type rightType = right.getType();
                BaseType bt = (BaseType)max(leftType, rightType);
                switch (bt.TypeEnum)
                {
                    case BasicType.Int:
                        {
                            Float l = (Float)convert(left, bt, basicTypes["Float"]);
                            Float r = (Float)convert(right, bt, basicTypes["Float"]);
                            return Float.opPow(l, r);
                        }
                    case BasicType.Float:
                        {
                            Float l = (Float)convert(left, bt, leftType);
                            Float r = (Float)convert(right, bt, rightType);
                            return Float.opPow(l, r);
                        }
                    default:
                        throw new ModelInterpreterException("Invalid type \"" + bt.TypeName + "\".")
                        {
                            Line = node.Line,
                            Position = node.Position
                        };
                }
            }
            catch (ModelInterpreterException exc)
            {
                throw exc;
            }
            catch (Exception exc)
            {
                throw new ModelInterpreterException(exc.Message)
                {
                    Line = node.Line,
                    Position = node.Position
                };
            }
            throw new ModelInterpreterException("Operator isn't available for custom types.")
            {
                Line = node.Line,
                Position = node.Position
            };
        }
        private Value function(ExpressionNode exp)
        {
            FunctionNode node = (FunctionNode)exp;
            List<FunctionDefinition> funcList;
            try
            {
                funcList = FunctionStorage.functionTable[node.FunctionName];
            }catch(Exception)
            {
                throw new ModelInterpreterException("Invalid function name.")
                {
                    Line = node.Line,
                    Position = node.Position
                };  
            }
            List<Object> args=new List<Object>();
            foreach (ExpressionNode expNode in node.Arguments)
            {
                Object obj = evaluate(expNode).getRValue();
                args.Add(obj);
            }
            Exception lastExc=new Exception("No definition.");
            foreach (FunctionDefinition fd in funcList)
            {
                try
                {
                    return FunctionDefinition.compute(fd,args);
                }
                catch (Exception exc)
                {
                    lastExc = exc;
                }
            }
            throw new ModelInterpreterException(lastExc.Message)
            {
                Line = node.Line,
                Position = node.Position
            };
        }
        private Value member(ExpressionNode exp)
        {
            throw new Exception("Not implemented");
        }
        private Value cast(ExpressionNode exp)
        {
            CastNode node=(CastNode)exp;
            Object right = evaluate(node.Right).getRValue();
            Type main;
            try
            {
                main = basicTypes[node.CastType.Value];
            } catch (Exception)
            {
                throw new ModelInterpreterException("Invalid type name.")
                {
                    Line = node.CastType.Line,
                    Position = node.CastType.Position
                };
            }
            return convert(right,main,right.getType());
        }
        private Value evaluate(ExpressionNode exp)
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
            return new Void();
        }
        static public Type max(Type a, Type b)//only for simple types and basic operations
        {
            if (a == b)
                return a;
            if (a is BaseType && b is BaseType)
            {
                BaseType _a = (BaseType)a;
                BaseType _b = (BaseType)b;
                switch (_a.TypeEnum)
                {
                    case BasicType.Complex:
                        if (_b.TypeEnum == BasicType.Float || _b.TypeEnum == BasicType.Int)
                        {
                            return a;
                        }
                        else if (_b.TypeEnum == BasicType.String)
                            return _b;
                        break;
                    case BasicType.Float:
                        if (_b.TypeEnum == BasicType.Complex)
                        {
                            return _b;
                        }
                        else if (_b.TypeEnum == BasicType.Int)
                        {
                            return _a;
                        }
                        else if (_b.TypeEnum == BasicType.String)
                            return _b;
                        break;
                    case BasicType.Int:
                        if (_b.TypeEnum == BasicType.Complex || _b.TypeEnum == BasicType.Float)
                        {
                            return _b;
                        }
                        else if (_b.TypeEnum == BasicType.String)
                            return _b;
                        break;
                    case BasicType.String:
                        return _a;
                }
            }
            throw new Exception("Invalid type conversion.");
        }
        //create graph for basic types dependency
        static public Object convert(Object obj, Type main, Type secondary)
        {
            if (secondary is BaseType && main is BaseType)
            {
                BaseType m = (BaseType)main, s = (BaseType)secondary;
                if (m.TypeEnum == s.TypeEnum)
                    return obj;
                switch (m.TypeEnum)
                {
                    case BasicType.Complex:
                        if (s.TypeEnum == BasicType.Float)
                        {
                            return new Complex(((Float)obj).Value, 0.0);
                        }
                        else if (s.TypeEnum == BasicType.Int)
                        {
                            return new Complex(((Int)obj).Value, 0.0);
                        }
                        break;
                    case BasicType.Float:
                        if (s.TypeEnum == BasicType.Int)
                        {
                            return new Float(((Int)obj).Value);
                        }
                        break;
                    case BasicType.String:
                        return obj.castToString();
                    case BasicType.Int:
                        break;
                }
            }
            throw new Exception("Invalid type conversion.");
        }


        public delegate Object FunctionExec(List<Object> args);
        public class ArgumentDescription
        {
            public Object defaultValue = null;
            public Type type;
        }
        public class FunctionSignature
        {
            public List<ArgumentDescription> Arguments { get; set; }
            public bool variableLength;
            public int defaultArgs;
            public FunctionSignature(List<ArgumentDescription> args, bool variableLength)
            {
                this.variableLength = variableLength;
                this.Arguments = args;
                bool checkArg = false;
                defaultArgs = 0;
                foreach (ArgumentDescription arg in args)
                {
                    if (arg.defaultValue != null)
                    {
                        defaultArgs++;
                        checkArg = true;
                    }
                    else if (checkArg == true)
                    {
                        throw new Exception("Invalid function signature. Invalid default argument values placement.");
                    }
                }
            }
        }
        public class FunctionDefinition
        {
            public FunctionExec Exec { get; set; }
            public FunctionSignature Signature { get; set; }
            static public Object compute(FunctionDefinition f, List<Object> variables)
            {
                List<Object> args = new List<Object>();
                if (variables.Count < f.Signature.Arguments.Count)
                {
                    if (f.Signature.defaultArgs >= f.Signature.Arguments.Count - variables.Count)
                    {
                        for (int i = 0; i < variables.Count; i++)
                        {
                            Object var = convert(variables[i], f.Signature.Arguments[i].type, variables[i].getType());
                            args.Add(var);
                        }
                        for (int j = variables.Count; j < f.Signature.Arguments.Count; j++)
                        {
                            args.Add(f.Signature.Arguments[j].defaultValue);
                        }
                        return f.Exec(args);
                    }
                    else
                        throw new Exception("Invalid number of arguments in function");
                }
                for (int i = 0; i < f.Signature.Arguments.Count; i++)
                {
                    Object var = convert(variables[i], f.Signature.Arguments[i].type, variables[i].getType());
                    args.Add(var);
                }
                if (variables.Count > f.Signature.Arguments.Count)
                {
                    if (f.Signature.variableLength)
                    {
                        for (int i = f.Signature.Arguments.Count; i < variables.Count; i++)
                            args.Add(variables[i]);
                    }
                    else
                        throw new Exception("Invalid number of arguments in function");
                }
                return f.Exec(args);
            }
        }
        static public class FunctionStorage
        {
            static public CircuitModelAC model;
            static public PowerGraph.PowerGraphManager powerModel;
            static public List<string> output;
            static public Dictionary<string, List<FunctionDefinition>> functionTable = new Dictionary<string, List<FunctionDefinition>>
            {
                {"resistor",new List<FunctionDefinition>
                    {
                        new FunctionDefinition
                        {
                            Exec=resistor,
                            Signature=new FunctionSignature(new List<ArgumentDescription>
                                {
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["Float"]}
                                },false
                                )
                        }
                    }
                },
                {"transformer",new List<FunctionDefinition>
                    {
                        new FunctionDefinition
                        {
                            Exec=transformer,
                            Signature=new FunctionSignature(new List<ArgumentDescription>
                                {
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["Float"]}
                                },false
                                )
                        }
                    }
                },
                {"transformer3w",new List<FunctionDefinition>
                    {
                        new FunctionDefinition
                        {
                            Exec=transformer3w,
                            Signature=new FunctionSignature(new List<ArgumentDescription>
                                {
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["Float"]},
                                    new ArgumentDescription { type = basicTypes["Float"]}
                                },false
                                )
                        }
                    }
                },
                {"autotransformer",new List<FunctionDefinition>
                    {
                        new FunctionDefinition
                        {
                            Exec=autotransformer,
                            Signature=new FunctionSignature(new List<ArgumentDescription>
                                {
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["Float"]},
                                    new ArgumentDescription { type = basicTypes["Float"]}
                                },false
                                )
                        }
                    }
                },
                {"switch",new List<FunctionDefinition>
                    {
                        new FunctionDefinition
                        {
                            Exec=_switch,
                            Signature=new FunctionSignature(new List<ArgumentDescription>
                                {
                                    new ArgumentDescription { type = basicTypes["Float"]}
                                },false
                                )
                        }
                    }
                },
                {"impedance",new List<FunctionDefinition>
                    {
                        new FunctionDefinition
                        {
                            Exec=impedance1,
                            Signature=new FunctionSignature(new List<ArgumentDescription>
                                {
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["Float"]},
                                    new ArgumentDescription { type = basicTypes["Float"]}
                                },false
                                )
                        },
                        new FunctionDefinition
                        {
                            Exec=impedance2,
                            Signature=new FunctionSignature(new List<ArgumentDescription>
                                {
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["Complex"]}
                                },false
                                )
                        }
                    }
                },
                {"capacitor",new List<FunctionDefinition>
                    {
                        new FunctionDefinition
                        {
                            Exec=capacitor,
                            Signature=new FunctionSignature(new List<ArgumentDescription>
                                {
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["Float"]}
                                },false
                                )
                        }
                    }
                },
                {"inductor",new List<FunctionDefinition>
                    {
                        new FunctionDefinition
                        {
                            Exec=inductor,
                            Signature=new FunctionSignature(new List<ArgumentDescription>
                                {
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["Float"]}
                                },false
                            )
                        }
                    }

                },
                {"line",new List<FunctionDefinition>
                    {
                        new FunctionDefinition
                        {
                            Exec=line,
                            Signature=new FunctionSignature(new List<ArgumentDescription>
                                {
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["String"]}
                                },false
                            )
                        }
                    }
                },
                {"ground",new List<FunctionDefinition>
                    {
                        new FunctionDefinition
                        {
                            Exec=ground,
                            Signature=new FunctionSignature(new List<ArgumentDescription>
                                {
                                    new ArgumentDescription { type = basicTypes["String"]}
                                },false
                            )
                        }
                    }
                },
                {"voltage",new List<FunctionDefinition>
                    {
                        new FunctionDefinition
                        {
                            Exec=voltage1,
                            Signature=new FunctionSignature(new List<ArgumentDescription>
                                {
                                    new ArgumentDescription { type = basicTypes["Element"]}
                                },false
                            )
                        },
                        new FunctionDefinition
                        {
                            Exec=voltage2,
                            Signature=new FunctionSignature(new List<ArgumentDescription>
                                {
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["String"]}
                                },false
                            )
                        },

                    }

                },
                {"current",new List<FunctionDefinition>
                    {
                        new FunctionDefinition
                        {
                            Exec=current,
                            Signature=new FunctionSignature(new List<ArgumentDescription>
                                {
                                    new ArgumentDescription { type = basicTypes["Element"]}
                                },false
                            )
                        }
                    }

                },
                {"currentSource",new List<FunctionDefinition>
                    {
                        new FunctionDefinition
                        {
                            Exec=currentSource1,
                            Signature=new FunctionSignature(new List<ArgumentDescription>
                                {
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["Complex"]},
                                    new ArgumentDescription { type = basicTypes["Float"], defaultValue= new Float(0.0)}
                                },false
                            )
                        },
                        new FunctionDefinition
                        {
                            Exec=currentSource2,
                            Signature=new FunctionSignature(new List<ArgumentDescription>
                                {
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["Float"]},
                                    new ArgumentDescription { type = basicTypes["Float"]},
                                    new ArgumentDescription { type = basicTypes["Float"], defaultValue= new Float(0.0)}
                                },false
                            )
                        }
                    }

                },
                {"voltageSource",new List<FunctionDefinition>
                    {
                        new FunctionDefinition
                        {
                            Exec=voltageSource1,
                            Signature=new FunctionSignature(new List<ArgumentDescription>
                                {
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["Complex"]},
                                    new ArgumentDescription { type = basicTypes["Float"], defaultValue= new Float(0.0)}
                                },false
                            )
                        },
                        new FunctionDefinition
                        {
                            Exec=voltageSource2,
                            Signature=new FunctionSignature(new List<ArgumentDescription>
                                {
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["String"]},
                                    new ArgumentDescription { type = basicTypes["Float"]},
                                    new ArgumentDescription { type = basicTypes["Float"]},
                                    new ArgumentDescription { type = basicTypes["Float"], defaultValue= new Float(0.0)}
                                },false
                            )
                        },
                    }
                },
                {"print",new List<FunctionDefinition>
                    {
                        new FunctionDefinition
                        {
                            Exec=print,
                            Signature=new FunctionSignature(new List<ArgumentDescription>
                                {
                                    new ArgumentDescription { type = basicTypes["String"]},
                                },false
                            )
                        },
                        new FunctionDefinition
                        {
                            Exec=printElement,
                            Signature=new FunctionSignature(new List<ArgumentDescription>
                                {
                                    new ArgumentDescription { type = basicTypes["Element"]},
                                },false
                            )
                        },
                    }
                }
            };
            static public Object resistor(List<Object> args)
            {
                int index = model.AddResistor(((String)args[0]).Value, ((String)args[1]).Value, (float)((Float)args[2]).Value);
                return new Element(index);
            }
            static public Object capacitor(List<Object> args)
            {
                int index = model.AddCapacitor(((String)args[0]).Value, ((String)args[1]).Value, (float)((Float)args[2]).Value);
                return new Element(index);
            }
            static public Object inductor(List<Object> args)
            {
                int index = model.AddInductor(((String)args[0]).Value, ((String)args[1]).Value, (float)((Float)args[2]).Value);
                return new Element(index);
            }
            static public Object line(List<Object> args)
            {
                int index = model.AddLine(((String)args[0]).Value, ((String)args[1]).Value);
                return new Element(index);
            }
            static public Object voltageSource1(List<Object> args)
            {
                String arg1 = (String)args[0];
                String arg2 = (String)args[1];
                Complex arg3 = (Complex)args[2];
                Float arg4 = (Float)args[3];
                int index = model.AddVoltageSource(arg1.Value, arg2.Value, (float)arg3.Magn, (float)arg3.Phase, (float)arg4.Value);
                return new Element(index);
            }
            static public Object voltageSource2(List<Object> args)
            {
                String arg1 = (String)args[0];
                String arg2 = (String)args[1];
                Float arg3 = (Float)args[2];
                Float arg4 = (Float)args[3];
                Float arg5 = (Float)args[4];
                int index = model.AddVoltageSource(arg1.Value, arg2.Value, (float)arg3.Value, (float)Utils.radians(arg4.Value), (float)arg5.Value);
                return new Element(index);
            }
            static public Object currentSource1(List<Object> args)
            {
                String arg1 = (String)args[0];
                String arg2 = (String)args[1];
                Complex arg3 = (Complex)args[2];
                Float arg4 = (Float)args[3];
                int index = model.AddCurrentSource(arg1.Value, arg2.Value, (float)arg3.Magn, (float)arg3.Phase, (float)arg4.Value);
                return new Element(index);
            }
            static public Object currentSource2(List<Object> args)
            {
                String arg1 = (String)args[0];
                String arg2 = (String)args[1];
                Float arg3 = (Float)args[2];
                Float arg4 = (Float)args[3];
                Float arg5 = (Float)args[4];
                int index = model.AddCurrentSource(arg1.Value, arg2.Value, (float)arg3.Value, (float)Utils.radians(arg4.Value), (float)arg5.Value);
                return new Element(index);
            }
            static public Object current(List<Object> args)
            {
                Element arg1 = (Element)args[0];
                model.AddCurrentOutput(arg1.Index);
                return new Void();
            }
            static public Object voltage1(List<Object> args)
            {
                Element arg1 = (Element)args[0];
                model.AddVoltageOutput(arg1.Index);
                return new Void();
            }
            static public Object voltage2(List<Object> args)
            {
                String arg1 = (String)args[0];
                String arg2 = (String)args[1];
                model.AddVoltageOutput(arg1.Value, arg2.Value);
                return new Void();
            }
            static public Object transformer(List<Object> args)
            {
                String arg1 = (String)args[0];
                String arg2 = (String)args[1];
                String arg3 = (String)args[2];
                String arg4 = (String)args[3];
                Float arg5 = (Float)args[4];
                int index = model.AddTransformer(arg1.Value,arg2.Value,arg3.Value,arg4.Value,(float)arg5.Value);
                return new Element(index);
            }
            static public Object ground(List<Object> args)
            {
                model.AddGround(((String)args[0]).Value);
                return new Void();
            }
            static public Object print(List<Object> args)
            {
                output.Add(((String)args[0]).Value);
                return new Void();
            }
            static public Object printElement(List<Object> args)
            {
                output.Add(model.GetElementString(((Element)args[0]).Index));
                return new Void();
            }
            static public Object re(List<Object> args)
            {
                Complex arg1 = (Complex)args[0];
                return new Float(arg1.Re);
            }
            static public Object im(List<Object> args)
            {
                Complex arg1 = (Complex)args[0];
                return new Float(arg1.Im);
            }
            static public Object magn(List<Object> args)
            {
                Complex arg1 = (Complex)args[0];
                return new Float(arg1.Magn);
            }
            static public Object phase(List<Object> args)
            {
                Complex arg1 = (Complex)args[0];
                return new Float(arg1.Phase);
            }
            static public Object radians(List<Object> args)
            {
                Float arg1 = (Float)args[0];
                arg1.Value = Utils.radians(arg1.Value);
                return arg1;
            }
            static public Object degrees(List<Object> args)
            {
                Float arg1 = (Float)args[0];
                arg1.Value = Utils.degrees(arg1.Value);
                return arg1;
            }
            static public Object conj(List<Object> args)
            {
                Complex arg1 = (Complex)args[0];
                arg1.Im = -arg1.Im;
                return arg1;
            }
            static public Object transformer3w(List<Object> args)
            {
                String arg1 = (String)args[0];
                String arg2 = (String)args[1];
                String arg3 = (String)args[2];
                String arg4 = (String)args[3];
                String arg5 = (String)args[4];
                String arg6 = (String)args[5];
                Float arg7 = (Float)args[6];
                Float arg8 = (Float)args[7];
                int index = model.AddTransformer3w(arg1.Value, arg2.Value, arg3.Value, arg4.Value,arg5.Value,arg6.Value,
                    (float)arg7.Value, (float)arg8.Value);
                return new Element(index);
            }
            static public Object _switch(List<Object> args)
            {
                String arg1 = (String)args[0];
                String arg2 = (String)args[1];
                Float state = (Float)args[2];
                int index = model.AddSwitch(arg1.Value, arg2.Value, state.Value==0.0f);
                return new Element(index);
            }
            static public Object impedance1(List<Object> args)
            {
                String arg1 = (String)args[0];
                String arg2 = (String)args[1];
                Float arg3 = (Float)args[2];
                Float arg4 = (Float)args[3];
                int index = model.AddImpedance(arg1.Value,arg2.Value,new MathNet.Numerics.Complex32((float)arg3.Value, (float)arg4.Value));
                return new Element(index);
            }
            static public Object impedance2(List<Object> args)
            {
                String arg1 = (String)args[0];
                String arg2 = (String)args[1];
                Complex arg3 = (Complex)args[2];
                int index = model.AddImpedance(arg1.Value, arg2.Value, new MathNet.Numerics.Complex32((float)arg3.Re,(float)arg3.Im));
                return new Element(index);
            }
            static public Object autotransformer(List<Object> args)
            {
                String arg1 = (String)args[0];
                String arg2 = (String)args[1];
                String arg3 = (String)args[2];
                Float arg4 = (Float)args[3];
                int index = model.AddAutoTransformer(arg1.Value, arg2.Value, arg3.Value, (float)arg4.Value);
                return new Element(index);

            }
        }
        public enum BasicType
        {
            Int,
            Float,
            Complex,
            Void,
            String,
            Element
        }
        public abstract class Type
        {
            public string TypeName { get; protected set; }
            public Type(string typeName)
            {
                this.TypeName = typeName;
            }
        }
        public class BaseType : Type
        {
            public BasicType TypeEnum { get; protected set; }
            public BaseType(BasicType t, string typeName) : base(typeName)
            {
                TypeEnum = t;
            }
        }
        public abstract class Value
        {
            abstract public Object getRValue();
        };
        public abstract class Object:Value
        {
            //protected bool isConst;
            protected Type type;
            public Type getType()
            {
                return type;
            }
            override public Object getRValue()
            {
                return this;
            }
            public abstract String castToString();
        }
        public class Void : Object
        {
            public Void()
            {
                this.type = basicTypes["Void"];
            }
            public override String castToString()
            {
                throw new Exception("Can't convert to string.");
            }
        }
        public class Float : Object
        {
            double val;
            public double Value { get { return val; } set { val = value; } }
            public Float()
            {
                this.type = basicTypes["Float"];
            }
            public Float(double value) : this()
            {
                this.val = value;
            }
            public static Float opAdd(Float a, Float b)
            {
                return new Float(a.Value + b.Value);
            }
            public static Float opSub(Float a, Float b)
            {
                return new Float(a.Value - b.Value);
            }
            public static Float opMult(Float a, Float b)
            {
                return new Float(a.Value * b.Value);
            }
            public static Float opDiv(Float a, Float b)
            {
                return new Float(a.Value / b.Value);
            }
            public static Float opPow(Float a, Float b)
            {
                return new Float(Math.Pow(a.Value, b.Value));
            }
            public static Float opNeg(Float a)
            {
                return new Float(-a.Value);
            }
            public override String castToString()
            {
                return new String(Value.ToString());
            }
        }
        public class Int : Object
        {
            int val;
            public int Value { get { return val; } set { val = value; } }
            public Int()
            {
                this.type = basicTypes["Int"];
            }
            public Int(int value) : this()
            {
                this.val = value;
            }
            public static Int opAdd(Int a, Int b)
            {
                return new Int(a.Value + b.Value);
            }
            public static Int opSub(Int a, Int b)
            {
                return new Int(a.Value - b.Value);
            }
            public static Int opMult(Int a, Int b)
            {
                return new Int(a.Value * b.Value);
            }
            public static Int opDiv(Int a, Int b)
            {
                return new Int(a.Value / b.Value);
            }
            public static Int opNeg(Int a)
            {
                return new Int(-a.Value);
            }
            public override String castToString()
            {
                return new String(Value.ToString());
            }
        }
        public class Complex : Object
        {
            double re;
            double im;
            public double Re { get { return re; } set { re = value; } }
            public double Im { get { return im; } set { im = value; } }
            public double Magn { get { return Math.Sqrt(re * re + im * im); } }
            public double Phase { get { return Math.Atan2(im, re); } }
            public Complex()
            {
                this.type = basicTypes["Complex"];
            }
            public Complex(double re, double im):this()
            {
                this.re = re;
                this.im = im;
            }
            public static Complex opAdd(Complex a, Complex b)
            {
                return new Complex(a.Re + b.Re, a.Im + b.Im);
            }
            public static Complex opSub(Complex a, Complex b)
            {
                return new Complex(a.Re + b.Re, a.Im + b.Im);
            }
            public static Complex opMult(Complex a, Complex b)
            {
                return new Complex(a.Re * b.Re - a.Im * b.Im, a.Re * b.Im + a.Im * b.Re);
            }
            public static Complex opDiv(Complex a, Complex b)
            {
                double l = b.Re * b.Re + b.Im + b.Im;
                return new Complex((a.Re * b.Re - a.Im * b.Im) / l, (-a.Re * b.Im + a.Im * b.Re) / l);
            }
            public static Complex opNeg(Complex a)
            {
                return new Complex(-a.Re, -a.Im);
            }
            public override String castToString()
            {
                return new String(Magn.ToString()+"@"+ Utils.degrees(Phase).ToString());
            }
        }
        public class String : Object
        {
            string val;
            public string Value { get { return val; } set { val = value; } }
            public String()
            {
                this.type = basicTypes["String"];
            }
            public String(string value) : this()
            {
                this.val = value;
            }
            public static String opAdd(String a, String b)
            {
                return new String(a.Value + b.Value);
            }
            public override String castToString()
            {
                return this;
            }
        }
        public class Element : Object
        {
            int index;
            public int Index { get { return index; } set { index = value; } }
            public Element()
            {
                this.type = basicTypes["Element"];
            }
            public Element(int index) : this()
            {
                this.index = index;
            }
            public override String castToString()
            {
                throw new Exception("Cannot convert element to string.");
                //return new String("Element[id="+Index.ToString()+"]");
            }
        }
        abstract class LValue : Value
        {
            public abstract void setValue(Object value);
        }
        class LValueIdentifier : LValue
        {
            public static Dictionary<string, Object> variableTable;
            string key;
            public LValueIdentifier(string key)
            {
                this.key = key;
            }
            public override void setValue(Object value)
            {
                variableTable[key] = value;
            }
            public override Object getRValue()
            {
                try
                {
                    Object obj = variableTable[key];
                    return obj;
                }
                catch (Exception)
                {
                    throw new Exception("Undefined variable \""+key+"\"");
                }
            }
        }

    }
}
