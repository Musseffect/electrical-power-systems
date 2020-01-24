using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.EquationInterpreter
{
    //TODO вынести в отдельный класс
    class ErrorListener<T> : IAntlrErrorListener<T>
    {
        List<ErrorMessage> errors;
        public ErrorListener()
        {
            this.errors = new List<ErrorMessage>();
        }
        public List<ErrorMessage> getErrors()
        {
            return this.errors;
        }
        public void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] T offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            Console.WriteLine(msg);
            errors.Add(new ErrorMessage(msg,line, charPositionInLine));
        }
    }
    public class NonlinearEquationDefinition
    {
        double[] initialValues;
        string[] variableNames;
        List<RPNExpression> equations;
        RPNExpression[,] jacobiMatrix;
        public double[] InitialValues { get { return initialValues; } }
        public string[] VariableNames { get { return variableNames; } }
        public List<RPNExpression> Equations { get { return equations; } }
        public RPNExpression[,] JacobiMatrix { get { return jacobiMatrix; } }
        private NonlinearEquationDefinition()
        {
        }
        public NonlinearEquationDefinition(double[] initialValues, string[] variableNames, List<RPNExpression> equations, RPNExpression[,] jacobiMatrix)
        {
            this.initialValues = initialValues;
            this.variableNames = variableNames;
            this.equations = equations;
            this.jacobiMatrix = jacobiMatrix;
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
        List<string> variableNames;
        List<double> initialValues;
        List<ErrorMessage> compilerErrors;


        public EquationCompiler()
        {
        }
        public List<ErrorMessage> getErrors()
        {
            return compilerErrors;
        }
        public NonlinearEquationDefinition compileEquations(string text)
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
            return compile((RootNode)root);
        }
        private NonlinearEquationDefinition compile(RootNode root)
        {

            List<RPNExpression> rpnEquations = new List<RPNExpression>();
            List<ExpressionNode> equations = new List<ExpressionNode>();

            foreach (var parameter in root.parameters)
            {
                ExpressionNode right = simplify(parameter.Right);
                if (right is FloatNode)
                {
                    parameters.Add(parameter.Identifier, ((FloatNode)right).Value);
                }
                else
                {
                    compilerErrors.Add(new ErrorMessage("Определение параметра " + parameter.Identifier + " не является константным выражением"));
                }
            }

            foreach (var equation in root.equations)
            {
                SubtractionNode exp = new SubtractionNode
                {
                    Left = equation.Left,
                    Right = equation.Right,
                    Line = equation.Line,
                    Position = equation.Position
                };
                validate(exp);
                equations.Add(exp);
                //simplify
                //index variables
            }

            foreach (var initialValue in root.initialValues)
            {
                if (variables.ContainsKey(initialValue.Identifier))
                {
                    ExpressionNode right = simplify(initialValue.Right);
                    if (right is FloatNode)
                    {
                        initialValues[variables[initialValue.Identifier]] = ((FloatNode)right).Value;
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
                compilerErrors.Add(new ErrorMessage("Количество переменных не совпадает с количеством уравнений"));
            }
            if (equations.Count == 0)
            {
                compilerErrors.Add(new ErrorMessage("Пустая система уравнений"));
            }
            if (compilerErrors.Count>0)
            {
                throw new Exception("Equation definition errors");
                //fall back;
            }
            ExpressionCompiler expCompiler = new ExpressionCompiler(variables);
            for (int i = 0; i < equations.Count; i++)
            {
                equations[i] = simplify(equations[i]);
                rpnEquations.Add(expCompiler.compile(equations[i]));
            }
            RPNExpression [,] jacobiMatrix = new RPNExpression[equations.Count, equations.Count];
            DifferentiationVisitor difVisitor = new DifferentiationVisitor(); 
            int j = 0;
            foreach (var equation in equations)
            {
                int i = 0;
                foreach (var variable in variableNames)
                {
                    //find derivative for variable 
                    ExpressionNode derivative = difVisitor.differentiate(equation, variable);
                    //simplify derivative expression
                    RPNExpression exp = expCompiler.compile(simplify(derivative));
                    jacobiMatrix[i, j] = exp;
                }
            }
            NonlinearEquationDefinition ned = new NonlinearEquationDefinition(initialValues.ToArray(),variableNames.ToArray(),
                rpnEquations,jacobiMatrix);
            return ned;
        }/*
        public static ExpressionStack compileASTExpression(ASTSimpleNode root)
        {
            varCount = 0;
            indicies = new Dictionary<string, int>();
            rpn = new List<StackElement>();
            try
            {
                compileVisitor(root);
            }
            catch (Exception exc)
            {
                throw new Exception(exc.Message);
            }
            return new ExpressionStack(rpn, indicies);
        }*/
    }
}
