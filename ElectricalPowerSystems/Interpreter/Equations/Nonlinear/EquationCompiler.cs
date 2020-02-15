using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using ElectricalPowerSystems.Interpreter.Equations.Expression;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Interpreter.Equations.Nonlinear
{
    public class NonlinearSystemSolution
    {
        Vector<double> values;
        Dictionary<string, int> variableIndicies;
        public NonlinearSystemSolution(Dictionary<string,int> variableIndicies, Vector<double> values)
        {
            this.variableIndicies = variableIndicies;
            this.values = values;
        }
        public double getValue(string name)
        {
            return values[variableIndicies[name]];
        }

    }
    public class CompilerException : Exception
    {
        public List<ErrorMessage> Errors { get; private set; }
        public CompilerException(List<ErrorMessage> errors)
        {
            Errors = errors;
        }
        public CompilerException(List<ErrorMessage> errors, string message) : base(message)
        {
            Errors = errors;
        }
    }
    public class NonlinearEquationDefinition
    {
        double[] initialValues;
        string[] variableNames;
        List<RPNExpression> equations;
        RPNExpression[,] jacobiMatrix;
        Dictionary<string, int> variableMap;
        public double[] InitialValues { get { return initialValues; } }
        public string[] VariableNames { get { return variableNames; } }
        public List<RPNExpression> Equations { get { return equations; } }
        public RPNExpression[,] JacobiMatrix { get { return jacobiMatrix; } }
        private NonlinearEquationDefinition()
        {
        }
        public NonlinearSystemSolution GetSolution(Vector<double> values)
        {
            return new NonlinearSystemSolution(variableMap, values);
        }
        public NonlinearEquationDefinition(double[] initialValues, string[] variableNames, List<RPNExpression> equations, RPNExpression[,] jacobiMatrix)
        {
            this.initialValues = initialValues;
            this.variableNames = variableNames;
            this.equations = equations;
            this.jacobiMatrix = jacobiMatrix;
            variableMap = new Dictionary<string, int>();
            int i = 0;
            foreach (var variableName in variableNames)
            {
                variableMap[variableName] = i;
                i++;
            }
        }
        public string PrintVariables()
        {
            string result = "";
            for (int i = 0; i < equations.Count; i++)
            {
                result += variableNames[i] + " = " + initialValues[i].ToString() + Environment.NewLine;
            }
            return result;
        }
        public string PrintEquations()
        {
            string result = "";
            for (int i = 0; i < equations.Count; i++)
            {
                result += $"f[{i}] = " + RPNExpression.print(equations[i], variableNames) + Environment.NewLine;
            }
            return result;
        }
        public string PrintJacobiMatrix()
        {
            string result = "";
            for (int i = 0; i < equations.Count; i++)
            {
                for (int j = 0; j < equations.Count; j++)
                {
                    result += $"df[{i}]/d{variableNames[j]} = " + RPNExpression.print(jacobiMatrix[j, i], variableNames) + Environment.NewLine;
                }
                result += Environment.NewLine;
            }
            return result;
        }
    }
    public partial class EquationCompiler
    {
        Dictionary<string, double> parameters;
        Dictionary<string, int> variables;
        List<double> initialValues;
        List<string> variableNames;
        List<ErrorMessage> compilerErrors;
        public EquationCompiler()
        {
        }
        public List<ErrorMessage> GetErrors()
        {
            return compilerErrors;
        }
        public NonlinearEquationDefinition CompileEquations(string text)
        {
            compilerErrors = new List<ErrorMessage>();
            parameters = new Dictionary<string, double>();
            variables = new Dictionary<string, int>();
            variableNames = new List<string>();
            initialValues = new List<double>();
            FunctionTable.Init();

            AntlrInputStream inputStream = new AntlrInputStream(text);
            EquationGrammarLexer eqLexer = new EquationGrammarLexer(inputStream);
            eqLexer.RemoveErrorListeners();
            ErrorListener<int> lexerListener = new ErrorListener<int>();
            eqLexer.AddErrorListener(lexerListener);
            CommonTokenStream commonTokenStream = new CommonTokenStream(eqLexer);
            EquationGrammarParser eqParser = new EquationGrammarParser(commonTokenStream);
            ErrorListener<IToken> parserListener = new ErrorListener<IToken>();
            eqParser.RemoveErrorListeners();
            eqParser.AddErrorListener(parserListener);
            EquationGrammarParser.CompileUnitContext eqContext = eqParser.compileUnit();
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
            EquationGrammarVisitor visitor = new EquationGrammarVisitor();
            ASTNode root = visitor.VisitCompileUnit(eqContext);
            /*EquationGrammarVisitor visitor = new EquationGrammarVisitor();
            ASTNode root = visitor.VisitCompileUnit(expContext);
            var rootSimple = Compiler.ASTCompiler.validate(root);
            rootSimple = Compiler.ASTCompiler.simplify(rootSimple);
            return compileASTExpression(rootSimple);*/
            return compileEquations((RootNode)root);
        }
        private NonlinearEquationDefinition compileEquations(RootNode root)
        {
            List<RPNExpression> rpnEquations = new List<RPNExpression>();
            List<Expression.Expression> equations = new List<Expression.Expression>();

            foreach (var parameter in root.parameters)
            {
                Expression.Expression right = ExpressionSimplifier.simplify(convertToExpression(parameter.Right));
                //ExpressionNode right = simplify(parameter.Right);
                if (right is Float)
                {
                    parameters.Add(parameter.Identifier, ((Float)right).Value);
                }
                else
                {
                    parameters.Add(parameter.Identifier, float.NaN);
                    compilerErrors.Add(new ErrorMessage("Определение параметра " + parameter.Identifier + " не является константным выражением"));
                }
            }

            foreach (var equation in root.equations)
            {
                SubtractionNode subtraction = new SubtractionNode
                {
                    Left = equation.Left,
                    Right = equation.Right,
                    Line = equation.Line,
                    Position = equation.Position
                };
                Expression.Expression expression = ExpressionSimplifier.simplify(convertToExpression(subtraction));
                equations.Add(expression);
            }

            //initialValues aren't important
            foreach (var initialValue in root.initialValues)
            {
                if (variables.ContainsKey(initialValue.Identifier))
                {
                    Expression.Expression right = ExpressionSimplifier.simplify(convertToExpression(initialValue.Right));
                    //Expression expression = convertToExpression(right)
                    if (right is Float)
                    {
                        initialValues[variables[initialValue.Identifier]] = ((Float)right).Value;
                    }
                    else
                    {
                        //add error message
                        compilerErrors.Add(new ErrorMessage("Определение начального приближения переменной" +
                            initialValue.Identifier + " не является константным выражением"));
                    }
                }else
                {
                    //add error message
                    compilerErrors.Add(new ErrorMessage("Определение начального приближения несуществующей переменной"));
                }
            }
            //check that number of variables = number of equations
            if (variableNames.Count != equations.Count)
            {
                compilerErrors.Add(new ErrorMessage($"Количество переменных не совпадает с количеством уравнений: {variableNames.Count} переменных, {equations.Count} уравнений"));
            }
            if (equations.Count == 0)
            {
                compilerErrors.Add(new ErrorMessage("Пустая система уравнений"));
            }
            if (compilerErrors.Count>0)
            {
                throw new CompilerException(compilerErrors);
                //throw new Exception("Equation definition errors");
                //fall back;
            }
            ExpressionCompiler expCompiler = new ExpressionCompiler(variables);
            for (int i = 0; i < equations.Count; i++)
            {
                rpnEquations.Add(expCompiler.compile(equations[i]));
            }
            RPNExpression [,] jacobiMatrix = new RPNExpression[equations.Count, equations.Count];
            Expression.DifferentiationVisitor difVisitor = new Expression.DifferentiationVisitor(); 
            int j = 0;
            foreach (var equation in equations)
            {
                int i = 0;
                foreach (var variable in variableNames)
                {
                    //find derivative for variable 
                    Expression.Expression derivative = ExpressionSimplifier.simplify(difVisitor.differentiate(equation, variable));
                    //simplify derivative expression
                    RPNExpression exp = expCompiler.compile(derivative);
                    jacobiMatrix[i, j] = exp;
                    i++;
                }
                j++;
            }
            NonlinearEquationDefinition ned = new NonlinearEquationDefinition(initialValues.ToArray(),variableNames.ToArray(),
                rpnEquations,jacobiMatrix);
            return ned;
        }
        private Expression.Expression convertIdentifier(IdentifierNode node)
        {
            if (!variables.ContainsKey(node.Value))
            {
                if (parameters.ContainsKey(node.Value))
                {
                    return new Float { Value = parameters[node.Value] };
                }
                else
                {
                    variables.Add(node.Value, variableNames.Count);
                    variableNames.Add(node.Value);
                    initialValues.Add(0.0);
                }
                //compilerErrors.Add(new ErrorMessage("Параметр " + node.Value + " не определён", node.Line, node.Position));
            }
            return new Variable { Name = node.Value}; ;
        }
        private Expression.Expression convertNegation(NegationNode node)
        {
            return new Negation { InnerNode = convertToExpression(node.InnerNode) };
        }
        private Expression.Expression convertAddition(AdditionNode node)
        {
            return new Addition
            {
                Left = convertToExpression(node.Left),
                Right = convertToExpression(node.Right)
            };
        }
        private Expression.Expression convertSubtraction(SubtractionNode node)
        {
            return new Subtraction
            {
                Left = convertToExpression(node.Left),
                Right = convertToExpression(node.Right)
            };
        }
        private Expression.Expression convertDivision(DivisionNode node)
        {
            return new Division
            {
                Left = convertToExpression(node.Left),
                Right = convertToExpression(node.Right)
            };
        }
        private Expression.Expression convertMultiplication(MultiplicationNode node)
        {
            return new Multiplication
            {
                Left = convertToExpression(node.Left),
                Right = convertToExpression(node.Right)
            };
        }
        private Expression.Expression convertFunction(FunctionNode node)
        {
            if (FunctionTable.isValidFunction(node.FunctionName))
            {
                //check number of arguments
                FunctionEntry entry = FunctionTable.getFunctionEntry(node.FunctionName);
                if (entry.ArgNumber != node.Arguments.Count)
                {
                    compilerErrors.Add(new ErrorMessage(entry.ArgNumber.ToString() + "arguments expected in function " + node.FunctionName, node.Line, node.Position));
                    return null;
                }
                List<Expression.Expression> arguments = new List<Expression.Expression>();
                foreach (var argument in node.Arguments)
                {
                    arguments.Add(convertToExpression(argument));
                }
                return new Function(entry,arguments);
            }
            else
            {
                compilerErrors.Add(new ErrorMessage("Unknown function " + node.FunctionName, node.Line, node.Position));
                return null;
            }
        }
        private Expression.Expression convertPower(PowerNode node)
        {
            return new Power
            {
                Left = convertToExpression(node.Left),
                Right = convertToExpression(node.Right)
            };
        }
        private Expression.Expression convertToExpression(ExpressionNode node)
        {
            switch (node.Type)
            {
                case ASTNodeType.Negation:
                    return convertNegation((NegationNode)node);
                case ASTNodeType.Addition:
                    return convertAddition((AdditionNode)node);
                case ASTNodeType.Subtraction:
                    return convertSubtraction((SubtractionNode)node);
                case ASTNodeType.Division:
                    return convertDivision((DivisionNode)node);
                case ASTNodeType.Multiplication:
                    return convertMultiplication((MultiplicationNode)node);
                case ASTNodeType.Float:
                    return new Float() {
                        Value = ((FloatNode)node).Value
                    };
                case ASTNodeType.Identifier:
                    return convertIdentifier((IdentifierNode)node);
                case ASTNodeType.Function:
                    return convertFunction((FunctionNode)node);
                case ASTNodeType.Power:
                    return convertPower((PowerNode)node);
            }
            throw new Exception("Unknown type in convertToExpression function");
        }
    }
}
