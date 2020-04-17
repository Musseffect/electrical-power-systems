
#define DAE
using Antlr4.Runtime;
using ElectricalPowerSystems.Equations.DAE;
using ElectricalPowerSystems.Equations;
using ElectricalPowerSystems.Equations.Expression;
using ElectricalPowerSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricalPowerSystems.MathUtils;

namespace ElectricalPowerSystems.Equations.DAE
{
#if DAE
    public partial class Compiler
    {
        class Variable
        {
            public enum Type
            {
                Algebraic = 0,
                Differential = 1
            }
            public string Name { get; set; }
            public double InitialValue { get; set; }
            public bool Initialized { get; set; }
            public Type VarType { get; set; }
            public int DerCount { get; set; }
        }
        List<ErrorMessage> compilerErrors;
        Dictionary<string, double> parameters;
        Dictionary<string, double> constants;
        Dictionary<string, Variable> variables;
        public Implicit.DAEIDescription CompileDAEImplicit(string text)
        {
            compilerErrors = new List<ErrorMessage>();
            parameters = new Dictionary<string, double>();
            constants = new Dictionary<string, double>();
            variables = new Dictionary<string, Variable>();
            FunctionTable.Init();
            //variables.Add("t", new Variable { Name = "t", InitialValue = 0.0, Initialized = true, VarType = Variable.Type.Algebraic, Count = 0 });

            AntlrInputStream inputStream = new AntlrInputStream(text);
            DAEImplicitGrammarLexer eqLexer = new DAEImplicitGrammarLexer(inputStream);
            eqLexer.RemoveErrorListeners();
            ErrorListener<int> lexerListener = new ErrorListener<int>();
            eqLexer.AddErrorListener(lexerListener);
            CommonTokenStream commonTokenStream = new CommonTokenStream(eqLexer);
            DAEImplicitGrammarParser eqParser = new DAEImplicitGrammarParser(commonTokenStream);
            ErrorListener<IToken> parserListener = new ErrorListener<IToken>();
            eqParser.RemoveErrorListeners();
            eqParser.AddErrorListener(parserListener);
            DAEImplicitGrammarParser.CompileUnitContext eqContext = eqParser.compileUnit();
            compilerErrors = lexerListener.GetErrors();
            if (compilerErrors.Count > 0)
            {
                throw new CompilerException(compilerErrors, "Lexer Error");
            }
            compilerErrors = parserListener.GetErrors();
            if (compilerErrors.Count > 0)
            {
                throw new CompilerException(compilerErrors, "Parser error");
            }
            DAEImplicitGrammarVisitor visitor = new DAEImplicitGrammarVisitor();
            ASTNode root = visitor.VisitCompileUnit(eqContext);

            return CompileDAEImplicit((RootNode)root);
        }

        private Implicit.DAEIDescription CompileDAEImplicit(RootNode root)
        {
            List<Expression.Expression> equations = new List<Expression.Expression>();
            constants["time"] = 1.0;
            constants["t0"] = 0.0;
            foreach (var constant in root.constants)
            {
                //Convert to expression
                Expression.Expression right = ExpressionSimplifier.Simplify(ConvertToExpression(constant.Right));
                if (right is Float)
                {
                    constants[constant.Identifier] = ((Float)right).Value;
                }
                else
                {
                    constants.Add(constant.Identifier, float.NaN);
                    compilerErrors.Add(new ErrorMessage("Определение константы " + constant.Identifier + " не является константным выражением"));
                }
            }
            foreach (var parameter in root.parameters)
            {
                //Convert to expression
                Expression.Expression right = ExpressionSimplifier.Simplify(ConvertToExpression(parameter.Right));
                if (right is Float)
                {
                    //parameters.Add(parameter.Identifier, ((Float)right).Value);
                    parameters[parameter.Identifier] = ((Float)right).Value;
                }
                else
                {
                    parameters.Add(parameter.Identifier, float.NaN);
                    compilerErrors.Add(new ErrorMessage("Определение параметра " + parameter.Identifier + " не является константным выражением"));
                }
            }
            foreach (var equation in root.equations)
            {
                //Convert to expression
                SubtractionNode exp = new SubtractionNode
                {
                    Left = equation.Left,
                    Right = equation.Right,
                    Line = equation.Line,
                    Position = equation.Position
                };
                Expression.Expression expression = ExpressionSimplifier.Simplify(ConvertToExpression(exp));
                equations.Add(expression);
            }
            foreach (var initialValue in root.initialValues)
            {
                if (variables.ContainsKey(initialValue.Identifier))
                {
                    //Convert to expression
                    Expression.Expression right = ExpressionSimplifier.Simplify(ConvertToExpression(initialValue.Right));
                    if (right is Float)
                    {
                        Variable var = variables[initialValue.Identifier];
                        var.InitialValue = ((Float)right).Value;
                        var.Initialized = true;
                    }
                    else
                    {
                        //add error message
                        compilerErrors.Add(new ErrorMessage("Определение начального условия переменной " +
                            initialValue.Identifier + " не является константным выражением"));
                    }
                }
                else
                {
                    //add error message
                    compilerErrors.Add(new ErrorMessage("Определение начального условия несуществующей переменной"));
                }
            }

            //!!!!!!!!!!!!!!!!!!!!!!!!!

            //check that number of variables = number of equations
            if (variables.Count!= equations.Count)
            {
                compilerErrors.Add(new ErrorMessage("Количество переменных не совпадает с количеством уравнений"));
            }
            if (equations.Count == 0)
            {
                compilerErrors.Add(new ErrorMessage("Пустая система уравнений"));
            }
            foreach (var variable in variables)
            {
                if (variable.Value.Initialized == false)
                {
                    variable.Value.InitialValue = 0.0;
                    //compilerErrors.Add(new ErrorMessage("Не объявлены начальные условия для переменной " + variable.Key));
                }
            }
            if (compilerErrors.Count > 0)
            {
                throw new CompilerException(compilerErrors);
                //fall back;
            }

            List<RPNExpression> equationsRPN = new List<RPNExpression>();
            List<string> variableNames = new List<string>();
            List<string> symbolNames = new List<string>();
            List<double> initialValues = new List<double>();
            Dictionary<string, int> symbolIndicies = new Dictionary<string, int>();

            List<Variable> variableList = new List<Variable>();
            foreach (var variable in variables)
            {
                variableList.Add(variable.Value);
            }
            variableList.Sort(delegate(Variable x,Variable y)
            {
                return x.Name.CompareTo(y.Name);
            });
            //time variable
            symbolIndicies.Add("t", 0);

            symbolNames.Add("t");
            //variables
            foreach (var variable in variableList)
            {
                variableNames.Add(variable.Name);
                symbolNames.Add(variable.Name);
                initialValues.Add(variable.InitialValue);
                symbolIndicies.Add(variable.Name, symbolIndicies.Count);
            }
            //derivatives
            foreach (var variable in variableList)
            {
                symbolNames.Add(variable.Name+"'");
                symbolIndicies.Add(variable.Name + "'", symbolIndicies.Count);
            }

            string[] parameterNames = new string[parameters.Count];
            double[] parameterValues = new double[parameters.Count];
            {
                int i = 0;
                foreach (var parameter in parameters)
                {
                    parameterNames[i] = parameter.Key;
                    parameterValues[i] = parameter.Value;
                    i++;
                }
            }
            foreach (var parameter in parameterNames)
            {
                symbolNames.Add(parameter);
                symbolIndicies.Add(parameter, symbolIndicies.Count);
            }

            Expression.Compiler expCompiler = new Expression.Compiler(symbolIndicies);
            for (int i = 0; i < equations.Count; i++)
            {
                equationsRPN.Add(expCompiler.Compile(equations[i]));
            }
            MathUtils.SparseMatrix<RPNExpression> dfdx = MathUtils.SparseMatrix<RPNExpression>.Build(equations.Count,equations.Count);
            MathUtils.SparseMatrix<RPNExpression> dfddx = MathUtils.SparseMatrix<RPNExpression>.Build(equations.Count, equations.Count);
            Expression.DifferentiationVisitor difVisitor = new Expression.DifferentiationVisitor();
            for (int j = 0; j < equations.Count; j++)
            {
                for (int i = 0; i < equations.Count; i++)
                {
                    Expression.Expression derivative = ExpressionSimplifier.Simplify(difVisitor.Differentiate(equations[i], variableNames[j]));
                    if (derivative.Type == ExpressionType.Float)
                    {
                        if ((derivative as Expression.Float).IsZero())
                        {
                            continue;
                        }
                    }
                    RPNExpression exp = expCompiler.Compile(derivative);
                    //dfdx[i, j] = exp;
                    dfdx.Add(i,j,exp);
                }
            }
            for (int j = 0; j < equations.Count; j++)
            {
                for (int i = 0; i < equations.Count; i++)
                {
                    Expression.Expression derivative = ExpressionSimplifier.Simplify(difVisitor.Differentiate(equations[i], variableNames[j] + "'"));
                    if (derivative.Type == ExpressionType.Float)
                    {
                        if ((derivative as Expression.Float).IsZero())
                        {
                            continue;
                        }
                    }
                    RPNExpression exp = expCompiler.Compile(derivative);
                    //dfddx[i, j] = exp;
                    dfddx.Add(i, j, exp);
                }
            }
            Implicit.DAEIDescription definition = new Implicit.DAEIDescription(variableNames.ToArray(),
                parameterNames,
                parameterValues,
                initialValues.ToArray(),
                equationsRPN,
                dfdx,
                dfddx,
                constants["t0"],
                constants["time"]);
            return definition;                                                                                                          
        }
#if DAEFULL
        
        public DAEDefinition CompileDAE(string text)
        {
            compilerErrors = new List<ErrorMessage>();
            parameters = new Dictionary<string, double>();
            variables = new Dictionary<string, Variable>();
            FunctionTable.Init();
            //variables.Add("t", new Variable { Name = "t", InitialValue = 0.0, Initialized = true,VarType = Variable.Type.Algebraic,Count=0 });

            AntlrInputStream inputStream = new AntlrInputStream(text);
            DAEImplicitGrammarLexer eqLexer = new DAEImplicitGrammarLexer(inputStream);
            eqLexer.RemoveErrorListeners();
            ErrorListener<int> lexerListener = new ErrorListener<int>();
            eqLexer.AddErrorListener(lexerListener);
            CommonTokenStream commonTokenStream = new CommonTokenStream(eqLexer);
            DAEImplicitGrammarParser eqParser = new DAEImplicitGrammarParser(commonTokenStream);
            ErrorListener<IToken> parserListener = new ErrorListener<IToken>();
            eqParser.RemoveErrorListeners();
            eqParser.AddErrorListener(parserListener);
            DAEImplicitGrammarParser.CompileUnitContext eqContext = eqParser.compileUnit();
            compilerErrors = lexerListener.getErrors();
            if (compilerErrors.Count > 0)
            {
                throw new Exception("Lexer Error");
            }
            compilerErrors = parserListener.getErrors();
            if (compilerErrors.Count > 0)
            {
                throw new Exception("Parser error");
            }
            DAEImplicitGrammarVisitor visitor = new DAEImplicitGrammarVisitor();
            ASTNode root = visitor.VisitCompileUnit(eqContext);

            return CompileDAE((RootNode)root);
        }
        private DAEDefinition compileDAE(RootNode root)
        {
            List<Expression.Expression> equations = new List<Expression.Expression>();
            parameters["time"] = 1.0;
            parameters["t0"] = 0.0;
            foreach (var parameter in root.parameters)
            {
                //Convert to expression
                Expression.Expression right = ExpressionSimplifier.simplify(ConvertToExpression(parameter.Right));
                if (right is Float)
                {
                    //parameters.Add(parameter.Identifier, ((Float)right).Value);
                    parameters[parameter.Identifier] = ((Float)right).Value;
                }
                else
                {
                    parameters.Add(parameter.Identifier, float.NaN);
                    compilerErrors.Add(new ErrorMessage("Определение параметра " + parameter.Identifier + " не является константным выражением"));
                }
            }
            foreach (var equation in root.equations)
            {
                //Convert to expression
                SubtractionNode exp = new SubtractionNode
                {
                    Left = equation.Left,
                    Right = equation.Right,
                    Line = equation.Line,
                    Position = equation.Position
                };
                equations.Add(ConvertToExpression(exp));
            }
            foreach (var initialValue in root.initialValues)
            {
                if (variables.ContainsKey(initialValue.Identifier))
                {
                    Expression.Expression right = ExpressionSimplifier.simplify(ConvertToExpression(initialValue.Right));
                    if (right is Float)
                    {
                        Variable var = variables[initialValue.Identifier];
                        var.InitialValue = ((Float)right).Value;
                        var.Initialized = true;
                    }
                    else
                    {
                        //add error message
                        compilerErrors.Add(new ErrorMessage("Определение начального условия переменной " +
                            initialValue.Identifier + " не является константным выражением"));
                    }
                }
                else
                {
                    //add error message
                    compilerErrors.Add(new ErrorMessage("Определение начального условия несуществующей переменной"));
                }
            }
            //check that number of variables = number of equations
            if (variables.Count - 1 != equations.Count)
            {
                compilerErrors.Add(new ErrorMessage("Количество переменных не совпадает с количеством уравнений"));
            }
            if (equations.Count == 0)
            {
                compilerErrors.Add(new ErrorMessage("Пустая система уравнений"));
            }
            foreach (var variable in variables)
            {
                if (variable.Value.Initialized == false)
                {
                    compilerErrors.Add(new ErrorMessage("Не объявлены начальные условия для переменной "+variable.Key));
                }
            }
            if (compilerErrors.Count > 0)
            {
                throw new Exception("Equation definition errors");
                //fall back;
            }

            List<Expression.Expression> explicitDifferentialEquations = new List<Expression.Expression>();
            List<Expression.Expression> implicitDifferentialEquations = new List<Expression.Expression>();
            List<Expression.Expression> algebraicEquations = new List<Expression.Expression>();

            bool semiExplicitForm = true;
            for (int i=0;i<equations.Count;i++)
            {
                var equation = equations[i];
                var astEquation = root.equations[i];
                Subtraction exp = (Subtraction)equation;
                if (astEquation.Left.Type == ASTNodeType.Derivative)
                {
                    exp.Right = ExpressionSimplifier.simplify(exp.Right);
                    if (hasDerivative(exp.Right) || variables[(astEquation.Left as DerivativeNode).Identifier].DerCount != 1)
                    {
                        semiExplicitForm = false;
                        implicitDifferentialEquations.Add(ExpressionSimplifier.simplify(exp));
                    }
                    else
                    {
                        exp.Right = ExpressionSimplifier.simplify(exp.Right);
                        explicitDifferentialEquations.Add(exp);
                    }
                }
                else
                {
                    Expression.Expression t_exp = ExpressionSimplifier.simplify(exp);
                    if (hasDerivative(t_exp))
                    {
                        semiExplicitForm = false;
                        implicitDifferentialEquations.Add(t_exp);
                    }
                    else
                    {
                        algebraicEquations.Add(t_exp);
                    }
                }
            }
            /*
             if all differential equations are explicit then
             create semiExplicit definition
             */
            if (semiExplicitForm)
            {
                List<string> variableNamesX = new List<string>();
                List<double> initialValuesX = new List<double>();
                List<string> variableNamesZ = new List<string>();
                List<double> initialValuesZ = new List<double>();
                List<RPNExpression> differentialEquationsRPN = new List<RPNExpression>();
                List<RPNExpression> algebraicEquationRPN = new List<RPNExpression>();


                Dictionary<string, int> variableIndicies = new Dictionary<string,int>();
                List<Variable> variableListX = new List<Variable>();
                List<Variable> variableListZ = new List<Variable>();
                foreach (var variable in variables)
                {
                    if (variable.Value.VarType == Variable.Type.Algebraic)
                        variableListZ.Add(variable.Value);
                    else
                        variableListX.Add(variable.Value);
                }
                variableListX.Sort(delegate (Variable x, Variable y)
                {
                    return x.Name.CompareTo(y.Name);
                });
                variableListZ.Sort(delegate (Variable x, Variable y)
                {
                    return x.Name.CompareTo(y.Name);
                });
                variableIndicies.Add("t", 0);
                foreach(var variable in variableListX)
                {
                    variableNamesX.Add(variable.Name);
                    initialValuesX.Add(variable.InitialValue);
                    variableIndicies.Add(variable.Name, variableIndicies.Count);
                }
                foreach(var variable in variableListZ)
                {
                    variableNamesZ.Add(variable.Name);
                    initialValuesZ.Add(variable.InitialValue);
                    variableIndicies.Add(variable.Name, variableIndicies.Count);
                }
                /*
                for (int i = 0; i < explicitDifferentialEquations.Count; i++)
                {
                    Subtraction equation = (Subtraction)explicitDifferentialEquations[i];
                    Expression.Variable left = (Expression.Variable)equation.Left;
                    string variableName = left.Name.TrimEnd(new char['\'']);
                    variableIndicies.Add(variableName, variableIndicies.Count);
                    variableNamesX.Add(variableName);
                    initialValuesX.Add(variables[variableName].InitialValue);
                }
                foreach (var variable in variableList)
                {
                    if (variable.Value.VarType == Variable.Type.Algebraic)
                    {
                        variableNamesZ.Add(variable.Key);
                        initialValuesZ.Add(variable.Value.InitialValue);
                        variableIndicies.Add(variable.Key, variableIndicies.Count);
                    }
                }*/

                ExpressionCompiler expCompiler = new ExpressionCompiler(variableIndicies);
                for (int i = 0; i < explicitDifferentialEquations.Count; i++)
                {
                    Subtraction equation = (Subtraction)explicitDifferentialEquations[i];
                    differentialEquationsRPN.Add(expCompiler.compile(equation.Right));
                }
                for (int i = 0; i < algebraicEquations.Count; i++)
                {
                    algebraicEquationRPN.Add(expCompiler.compile(algebraicEquations[i]));
                }
                DAESemiExplicitDefinition definition = new DAESemiExplicitDefinition(
                    variableNamesX.ToArray(),
                    variableNamesZ.ToArray(),
                    initialValuesX.ToArray(),
                    initialValuesZ.ToArray(),
                    differentialEquationsRPN,
                    algebraicEquationRPN
                    );
                return definition;
            }
            else
            {
                List<RPNExpression> equationsRPN = new List<RPNExpression>();
                List<string> variableNames = new List<string>();
                List<double> initialValues = new List<double>();
                Dictionary<string, int> variableIndicies = new Dictionary<string, int>();

                List<Variable> variableList = new List<Variable>();
                foreach (var variable in variables)
                {
                    variableList.Add(variable.Value);
                }
                variableList.Sort(delegate (Variable x, Variable y)
                {
                    return x.Name.CompareTo(y.Name);
                });
                //time variable
                variableIndicies.Add("t", 0);
                //variables
                foreach (var variable in variableList)
                {
                    variableNames.Add(variable.Name);
                    initialValues.Add(variable.InitialValue);
                    variableIndicies.Add(variable.Name, variableIndicies.Count);
                }
                //derivatives
                foreach (var variable in variableList)
                {
                    variableIndicies.Add(variable.Name + "'", variableIndicies.Count);
                }
                
                ExpressionCompiler expCompiler = new ExpressionCompiler(variableIndicies);
                for (int i = 0; i < implicitDifferentialEquations.Count; i++)
                {
                    equationsRPN.Add(expCompiler.compile(implicitDifferentialEquations[i]));
                }
                for (int i = 0; i < explicitDifferentialEquations.Count; i++)
                {
                    Subtraction equation = (Subtraction)explicitDifferentialEquations[i];
                    equationsRPN.Add(expCompiler.compile(equation));
                }
                for (int i = 0; i < algebraicEquations.Count; i++)
                {
                    equationsRPN.Add(expCompiler.compile(algebraicEquations[i]));
                }

                DAEImplicitDefinition definition = new DAEImplicitDefinition(variableNames.ToArray(),initialValues.ToArray(),equationsRPN);
                return definition;
            }
        }
#endif
        private Expression.Expression ConvertDerivative(DerivativeNode node)
        {
            if (node.Identifier == "t")
            {
                compilerErrors.Add(new ErrorMessage("Время не может использоваться в качестве дифференциальной переменной", node.Line, node.Position));
                return new Expression.Variable { Name = node.Identifier + "'" };
            }
            if (parameters.ContainsKey(node.Identifier))
            {
                compilerErrors.Add(new ErrorMessage("Константа не может использоваться в качестве дифференциальной переменной", node.Line, node.Position));
                return new Expression.Variable { Name = node.Identifier + "'" };
            }
            if (!variables.ContainsKey(node.Identifier))
            {
                variables.Add(node.Identifier, new Variable
                {
                    InitialValue = 0.0,
                    Name = node.Identifier,
                    Initialized = false,
                    VarType = Variable.Type.Differential,
                    DerCount = 1
                });
            }
            else
            {
                variables[node.Identifier].VarType = Variable.Type.Differential;
                variables[node.Identifier].DerCount += 1;
            }
            return new Expression.Variable { Name = node.Identifier+"'" };
        }
        private Expression.Expression ConvertIdentifier(IdentifierNode node)
        {
            if (node.Value == "t")
            {
                return new Expression.Variable { Name = "t" };
            }
            if (!variables.ContainsKey(node.Value))
            {
                if (constants.ContainsKey(node.Value))
                {
                    return new Float { Value = constants[node.Value] };
                }
                if (parameters.ContainsKey(node.Value))
                {
                    return new Expression.Variable { Name = node.Value };
                }
                variables.Add(node.Value, new Variable
                {
                    InitialValue = 0.0,
                    Name = node.Value,
                    Initialized = false,
                    VarType = Variable.Type.Algebraic,
                    DerCount = 0
                });
            }
            return new Expression.Variable { Name = node.Value }; ;
        }
        private Expression.Expression ConvertMultiplication(MultiplicationNode node)
        {
            return new Multiplication
            {
                Left = ConvertToExpression(node.Left),
                Right = ConvertToExpression(node.Right)
            };
        }
        private Expression.Expression ConvertNegation(NegationNode node)
        {
            return new Negation { InnerNode = ConvertToExpression(node.InnerNode) };
        }
        private Expression.Expression ConvertAddition(AdditionNode node)
        {
            return new Addition
            {
                Left = ConvertToExpression(node.Left),
                Right = ConvertToExpression(node.Right)
            };
        }
        private Expression.Expression ConvertSubtraction(SubtractionNode node)
        {
            return new Subtraction
            {
                Left = ConvertToExpression(node.Left),
                Right = ConvertToExpression(node.Right)
            };
        }
        private Expression.Expression ConvertDivision(DivisionNode node)
        {
            return new Division
            {
                Left = ConvertToExpression(node.Left),
                Right = ConvertToExpression(node.Right)
            };
        }
        private Expression.Expression ConvertFunction(FunctionNode node)
        {
            if (FunctionTable.IsValidFunction(node.FunctionName))
            {
                //check number of arguments
                FunctionEntry entry = FunctionTable.GetFunctionEntry(node.FunctionName);
                if (entry.ArgNumber != node.Arguments.Count)
                {
                    compilerErrors.Add(new ErrorMessage(entry.ArgNumber.ToString() + "arguments expected in function " + node.FunctionName, node.Line, node.Position));
                    return new Function(entry,new List<Expression.Expression>());
                }
                List<Expression.Expression> arguments = new List<Expression.Expression>();
                foreach (var argument in node.Arguments)
                {
                    arguments.Add(ConvertToExpression(argument));
                }
                return new Function(entry, arguments);
            }
            else
            {
                compilerErrors.Add(new ErrorMessage("Unknown function " + node.FunctionName, node.Line, node.Position));
                return new Function(null, new List<Expression.Expression>());
            }
        }
        private Expression.Expression ConvertPower(PowerNode node)
        {
            return new Power
            {
                Left = ConvertToExpression(node.Left),
                Right = ConvertToExpression(node.Right)
            };
        }
        private Expression.Expression ConvertToExpression(ExpressionNode node)
        {
            switch (node.Type)
            {
                case ASTNodeType.Negation:
                    return ConvertNegation((NegationNode)node);
                case ASTNodeType.Addition:
                    return ConvertAddition((AdditionNode)node);
                case ASTNodeType.Subtraction:
                    return ConvertSubtraction((SubtractionNode)node);
                case ASTNodeType.Division:
                    return ConvertDivision((DivisionNode)node);
                case ASTNodeType.Multiplication:
                    return ConvertMultiplication((MultiplicationNode)node);
                case ASTNodeType.Float:
                    return new Float()
                    {
                        Value = ((FloatNode)node).Value
                    };
                case ASTNodeType.Derivative:
                    return ConvertDerivative((DerivativeNode)node);
                case ASTNodeType.Identifier:
                    return ConvertIdentifier((IdentifierNode)node);
                case ASTNodeType.Function:
                    return ConvertFunction((FunctionNode)node);
                case ASTNodeType.Power:
                    return ConvertPower((PowerNode)node);
            }
            throw new Exception("Unknown type in convertToExpression function");
        }
        private static bool HasDerivative(Negation node)
        {
            return HasDerivative(node.InnerNode);
        }
        private static bool HasDerivative(InfixExpression node)
        {
            return HasDerivative(node.Left) || HasDerivative(node.Right);
        }
        private static bool HasDerivative(Function node)
        {
            bool result = false;
            foreach (var argument in node.Arguments)
            {
                result |= HasDerivative(argument);
            }
            return result;
        }
        private static bool HasDerivative(Expression.Variable node)
        {
            return node.Name.Last() == '\'';
        }
        private static bool HasDerivative(Expression.Expression node)
        {
            switch (node.Type)
            {
                case ExpressionType.Negation:
                    return HasDerivative((Negation)node);
                case ExpressionType.Addition:
                    return HasDerivative((InfixExpression)node);
                case ExpressionType.Subtraction:
                    return HasDerivative((InfixExpression)node);
                case ExpressionType.Division:
                    return HasDerivative((InfixExpression)node);
                case ExpressionType.Multiplication:
                    return HasDerivative((InfixExpression)node);
                case ExpressionType.Float:
                    return false;
                case ExpressionType.Variable:
                    return HasDerivative((Expression.Variable)node);
                case ExpressionType.Function:
                    return HasDerivative((Function)node);
                case ExpressionType.Power:
                    return HasDerivative((InfixExpression)node);
            }
            throw new Exception("hasDerivative function exception");
        }
    }
#endif
}