using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using ElectricalPowerSystems.Equations.Expression;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Equations.Nonlinear
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
        public double GetValue(string name)
        {
            return values[variableIndicies[name]];
        }
    }
    public class NonlinearEquationDescription
    {
        private double[] initialValues;
        private string[] variableNames;
        private List<RPNExpression> equations;
        private RPNExpression[,] jacobiMatrix;
        Dictionary<string, int> variableMap;
        public double[] InitialValues { get { return initialValues; } }
        public string[] VariableNames { get { return variableNames; } }
        public List<RPNExpression> Equations { get { return equations; } }
        public RPNExpression[,] JacobiMatrix { get { return jacobiMatrix; } }
        private NonlinearEquationDescription()
        {
        }
        public NonlinearSystemSolution GetSolution(Vector<double> values)
        {
            return new NonlinearSystemSolution(variableMap, values);
        }
        public NonlinearEquationDescription(double[] initialValues, string[] variableNames, List<RPNExpression> equations, RPNExpression[,] jacobiMatrix)
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
                result += (i + 1) + ". " + variableNames[i] + " = " + initialValues[i].ToString() + Environment.NewLine;
            }
            return result;
        }
        public string PrintEquations()
        {
            string result = "";
            for (int i = 0; i < equations.Count; i++)
            {
                result += $"f[{i+1}] = " + RPNExpression.Print(equations[i], variableNames) + " = 0" + Environment.NewLine;
            }
            return result;
        }
        public string PrintJacobiMatrix(bool simplified = true)
        {
            string result = "";
            for (int i = 0; i < equations.Count; i++)
            {
                if (simplified)
                {
                    for (int j = 0; j < equations.Count; j++)
                    {
                        if (j != 0)
                            result += ", ";
                        result += RPNExpression.Print(jacobiMatrix[j, i], variableNames);
                    }
                    result += Environment.NewLine;
                } else
                {
                    for (int j = 0; j < equations.Count; j++)
                    {
                        result += $"df[{i+1}]/d{variableNames[j]} = " + RPNExpression.Print(jacobiMatrix[j, i], variableNames) + Environment.NewLine;
                    }
                    result += Environment.NewLine;
                }
            }
            return result;
        }
    }
    public partial class Compiler
    {
        Dictionary<string, double> constants;
        Dictionary<string, int> variables;
        List<double> initialValues;
        List<string> variableNames;
        List<ErrorMessage> compilerErrors;
        public Compiler()
        {
        }
        public List<ErrorMessage> GetErrors()
        {
            return compilerErrors;
        }
        public NonlinearEquationDescription CompileEquations(string text)
        {
            compilerErrors = new List<ErrorMessage>();
            constants = new Dictionary<string, double>();
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
            compilerErrors = lexerListener.GetErrors();
            if (compilerErrors.Count > 0)
            {
                throw new CompilerException(compilerErrors, "Lexer error");
                throw new Exception("Lexer Error");
            }
            compilerErrors = parserListener.GetErrors();
            if (compilerErrors.Count > 0)
            {
                throw new CompilerException(compilerErrors, "Parser error");
                throw new Exception("Parser error");
            }
            GrammarVisitor visitor = new GrammarVisitor();
            ASTNode root = visitor.VisitCompileUnit(eqContext);
            /*EquationGrammarVisitor visitor = new EquationGrammarVisitor();
            ASTNode root = visitor.VisitCompileUnit(expContext);
            var rootSimple = Compiler.ASTCompiler.validate(root);
            rootSimple = Compiler.ASTCompiler.simplify(rootSimple);
            return compileASTExpression(rootSimple);*/
            return CompileEquations((RootNode)root);
        }
        private NonlinearEquationDescription CompileEquations(RootNode root)
        {
            List<RPNExpression> rpnEquations = new List<RPNExpression>();
            List<Expression.Expression> equations = new List<Expression.Expression>();

            foreach (var constant in root.constants)
            {
                Expression.Expression right = ExpressionSimplifier.Simplify(ConvertToExpression(constant.Right));
                //ExpressionNode right = simplify(constant.Right);
                if (right is Float)
                {
                    constants.Add(constant.Identifier, ((Float)right).Value);
                }
                else
                {
                    constants.Add(constant.Identifier, float.NaN);
                    compilerErrors.Add(new ErrorMessage("Определение константы " + constant.Identifier + " не является константным выражением"));
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
                Expression.Expression expression = ExpressionSimplifier.Simplify(ConvertToExpression(subtraction));
                equations.Add(expression);
            }

            //initialValues aren't important
            foreach (var initialValue in root.initialValues)
            {
                if (variables.ContainsKey(initialValue.Identifier))
                {
                    Expression.Expression right = ExpressionSimplifier.Simplify(ConvertToExpression(initialValue.Right));
                    //Expression expression = ConvertToExpression(right)
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
            Expression.Compiler expCompiler = new Expression.Compiler(variables);
            for (int i = 0; i < equations.Count; i++)
            {
                rpnEquations.Add(expCompiler.Compile(equations[i]));
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
                    Expression.Expression derivative = ExpressionSimplifier.Simplify(difVisitor.Differentiate(equation, variable));
                    //simplify derivative expression
                    RPNExpression exp = expCompiler.Compile(derivative);
                    jacobiMatrix[i, j] = exp;
                    i++;
                }
                j++;
            }
            NonlinearEquationDescription ned = new NonlinearEquationDescription(initialValues.ToArray(),variableNames.ToArray(),
                rpnEquations,jacobiMatrix);
            return ned;
        }
        private Expression.Expression ConvertIdentifier(IdentifierNode node)
        {
            if (!variables.ContainsKey(node.Value))
            {
                if (constants.ContainsKey(node.Value))
                {
                    return new Float { Value = constants[node.Value] };
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
        private Expression.Expression ConvertMultiplication(MultiplicationNode node)
        {
            return new Multiplication
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
                    return null;
                }
                List<Expression.Expression> arguments = new List<Expression.Expression>();
                foreach (var argument in node.Arguments)
                {
                    arguments.Add(ConvertToExpression(argument));
                }
                return new Function(entry,arguments);
            }
            else
            {
                compilerErrors.Add(new ErrorMessage("Unknown function " + node.FunctionName, node.Line, node.Position));
                return null;
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
                    return new Float() {
                        Value = ((FloatNode)node).Value
                    };
                case ASTNodeType.Identifier:
                    return ConvertIdentifier((IdentifierNode)node);
                case ASTNodeType.Function:
                    return ConvertFunction((FunctionNode)node);
                case ASTNodeType.Power:
                    return ConvertPower((PowerNode)node);
            }
            throw new Exception("Unknown type in convertToExpression function");
        }
    }
}
