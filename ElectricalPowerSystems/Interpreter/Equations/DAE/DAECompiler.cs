using Antlr4.Runtime;
using ElectricalPowerSystems.Interpreter.Equations.DAE;
using ElectricalPowerSystems.Interpreter.Equations;
using ElectricalPowerSystems.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Interpreter.Equations.DAE
{
    public abstract class DAEDefinition
    {
        public abstract string PrintSystem();
    }
    public class DAESemiExplicitDefinition : DAEDefinition
    {
        private DAESemiExplicitDefinition()
        { }
        public DAESemiExplicitDefinition(string [] variableNamesX,string[] variableNamesZ,double[] initialValuesX,double[] initialValuesZ
            , List<RPNExpression> f, List<RPNExpression> g)
        {
            this.variableNamesX = variableNamesX;
            this.variableNamesZ = variableNamesZ;
            this.initialValuesX = initialValuesX;
            this.initialValuesZ = initialValuesZ;
            this.f = f;
            this.g = g;
        }
        public double[] InitialValuesX { get { return initialValuesX; } }
        public double[] InitialValuesZ { get { return initialValuesZ; } }
        string[] variableNamesX;
        string[] variableNamesZ;
        double[] initialValuesX;
        double[] initialValuesZ;
        List<RPNExpression> f;
        List<RPNExpression> g;
        public List<RPNExpression> F { get { return f; } }
        public List<RPNExpression> G { get { return g; } }
        public string[] VariableNamesX { get { return variableNamesX; } }
        public string[] VariableNamesZ { get { return variableNamesZ; } }
        public override string PrintSystem()
        {
            List<string> list = new List<string>();
            list.AddRange(variableNamesX);
            list.AddRange(variableNamesZ);
            string[] variableNames = list.ToArray();
            string result = "";
            for (int i=0;i<f.Count;i++)
            {
                result+= variableNamesX[i] +" = "+RPNExpression.print(f[i],variableNames)+ Environment.NewLine;
            }
            for (int i = 0; i < g.Count; i++)
            {
                result += "0 = " + RPNExpression.print(g[i], variableNames) + Environment.NewLine;
            }
            return result;
        }
    }
    public class DAEImplicitDefinition : DAEDefinition
    {
        private DAEImplicitDefinition()
        {
        }
        public DAEImplicitDefinition(string [] variableNames,double[] initialValues,List<RPNExpression> f)
        {
            this.variableNames = variableNames;
            this.initialValues = initialValues;
            this.f = f;
        }
        public double[] InitialValues { get { return initialValues; } }
        public string[] VariableNames { get { return variableNames; } }
        string[] variableNames;
        double[] initialValues;
        List<RPNExpression> f;
        public List<RPNExpression> F { get { return f; } }
        public override string PrintSystem()
        {
            string result = "";
            for (int i = 0; i < f.Count; i++)
            {
                result += variableNames[i] + " = " + RPNExpression.print(f[i], variableNames) + Environment.NewLine;
            }
            return result;
        }
    }
    public partial class DAECompiler
    {
        class Variable
        {
            public enum Type
            {
                Algebraic,
                Differential
            }
            public string Name { get; set; }
            public double InitialValue { get; set; }
            public bool Initialized { get; set; }
            public Type VarType { get; set; }
            public int Count { get; set; }
        }
        List<ErrorMessage> compilerErrors;
        Dictionary<string, double> parameters;
        Dictionary<string, Variable> variables;
        public DAEImplicitDefinition compileDAEImplicit(string text)
        {
            compilerErrors = new List<ErrorMessage>();
            parameters = new Dictionary<string, double>();
            variables = new Dictionary<string, Variable>();
            FunctionTable.Init();
            variables.Add("t", new Variable { Name = "t", InitialValue = 0.0, Initialized = true, VarType = Variable.Type.Algebraic, Count = 0 });

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

            return compileDAEImplicit((RootNode)root);
        }
        public DAEDefinition compileDAE(string text)
        {
            compilerErrors = new List<ErrorMessage>();
            parameters = new Dictionary<string, double>();
            variables = new Dictionary<string, Variable>();
            FunctionTable.Init();
            variables.Add("t", new Variable { Name = "t", InitialValue = 0.0, Initialized = true,VarType = Variable.Type.Algebraic,Count=0 });

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

            return compileDAE((RootNode)root);
        }
        private DAEImplicitDefinition compileDAEImplicit(RootNode root)
        {
            List<ExpressionNode> equations = new List<ExpressionNode>();
            foreach (var parameter in root.parameters)
            {
                //convert to expression
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
                //convert to expression
                SubtractionNode exp = new SubtractionNode
                {
                    Left = equation.Left,
                    Right = equation.Right,
                    Line = equation.Line,
                    Position = equation.Position
                };
                validate(exp);
                equations.Add(simplify(exp));
            }
            foreach (var initialValue in root.initialValues)
            {
                if (variables.ContainsKey(initialValue.Identifier))
                {
                    //convert to expression
                    ExpressionNode right = simplify(initialValue.Right);
                    if (right is FloatNode)
                    {
                        Variable var = variables[initialValue.Identifier];
                        var.InitialValue = ((FloatNode)right).Value;
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
                    compilerErrors.Add(new ErrorMessage("Не объявлены начальные условия для переменной " + variable.Key));
                }
            }
            if (compilerErrors.Count > 0)
            {
                throw new Exception("Equation definition errors");
                //fall back;
            }

            List<RPNExpression> equationsRPN = new List<RPNExpression>();
            List<string> variableNames = new List<string>();
            List<double> initialValues = new List<double>();
            Dictionary<string, int> variableIndicies = new Dictionary<string, int>();
            foreach (var variable in variables)
            {
                if (variable.Key != "t")
                {
                    variableNames.Add(variable.Key);
                    initialValues.Add(variable.Value.InitialValue);
                    variableIndicies.Add(variable.Key, variableIndicies.Count);
                }
            }
            foreach (var variable in variables)
            {
                if (variable.Key != "t")
                {
                    variableIndicies.Add(variable.Key + "'", variableIndicies.Count);
                }
            }
            variableIndicies.Add("t", variableIndicies.Count);
            ExpressionCompiler expCompiler = new ExpressionCompiler(variableIndicies);
            for (int i = 0; i < equations.Count; i++)
            {
                equationsRPN.Add(expCompiler.compile(equations[i]));
            }
            DAEImplicitDefinition definition = new DAEImplicitDefinition(variableNames.ToArray(), initialValues.ToArray(), equationsRPN);
            return definition;
        }
        private DAEDefinition compileDAE(RootNode root)
        {
            List<ExpressionNode> equations = new List<ExpressionNode>();
            foreach (var parameter in root.parameters)
            {
                //convert to expression
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
                //convert to expression
                SubtractionNode exp = new SubtractionNode
                {
                    Left = equation.Left,
                    Right = equation.Right,
                    Line = equation.Line,
                    Position = equation.Position
                };
                validate(exp);
                equations.Add(exp);
            }
            foreach (var initialValue in root.initialValues)
            {
                if (variables.ContainsKey(initialValue.Identifier))
                {
                    ExpressionNode right = simplify(initialValue.Right);
                    if (right is FloatNode)
                    {
                        Variable var = variables[initialValue.Identifier];
                        var.InitialValue = ((FloatNode)right).Value;
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

            List<ExpressionNode> explicitDifferentialEquations = new List<ExpressionNode>();
            List<ExpressionNode> implicitDifferentialEquations = new List<ExpressionNode>();
            List<ExpressionNode> algebraicEquations = new List<ExpressionNode>();

            bool semiExplicitForm = true;
            foreach (var equation in equations)
            {
                SubtractionNode exp = (SubtractionNode)equation;
                if (exp.Left.Type == ASTNodeType.Derivative)
                {
                    DerivativeNode left = (DerivativeNode)exp.Left;
                    exp.Right = simplify(exp.Right);
                    if (hasDerivative(exp.Right) || variables[left.Identifier].Count != 1)
                    {
                        semiExplicitForm = false;
                        implicitDifferentialEquations.Add(simplify(exp));
                    }
                    else
                    {
                        explicitDifferentialEquations.Add(exp);
                    }
                }
                else
                {
                    ExpressionNode t_exp = simplify(exp);
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
                for (int i = 0; i < explicitDifferentialEquations.Count; i++)
                {
                    SubtractionNode equation = (SubtractionNode)explicitDifferentialEquations[i];
                    DerivativeNode left = (DerivativeNode)equation.Left;
                    variableIndicies.Add(left.Identifier, variableIndicies.Count);
                    variableNamesX.Add(left.Identifier);
                    initialValuesX.Add(variables[left.Identifier].InitialValue);
                }
                foreach (var variable in variables)
                {
                    if (variable.Key != "t" && variable.Value.VarType == Variable.Type.Algebraic)
                    {
                        variableNamesZ.Add(variable.Key);
                        initialValuesZ.Add(variable.Value.InitialValue);
                        variableIndicies.Add(variable.Key, variableIndicies.Count);
                    }
                }
                variableIndicies.Add("t", variableIndicies.Count);

                ExpressionCompiler expCompiler = new ExpressionCompiler(variableIndicies);
                for (int i = 0; i < explicitDifferentialEquations.Count; i++)
                {
                    SubtractionNode equation = (SubtractionNode)explicitDifferentialEquations[i];
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
                foreach (var variable in variables)
                {
                    if (variable.Key != "t")
                    {
                        variableNames.Add(variable.Key);
                        initialValues.Add(variable.Value.InitialValue);
                        variableIndicies.Add(variable.Key, variableIndicies.Count);
                    }
                }
                foreach (var variable in variables)
                {
                    if (variable.Key != "t")
                    {
                        variableIndicies.Add(variable.Key+"'", variableIndicies.Count);
                    }
                }
                variableIndicies.Add("t", variableIndicies.Count);
                ExpressionCompiler expCompiler = new ExpressionCompiler(variableIndicies);
                for (int i = 0; i < implicitDifferentialEquations.Count; i++)
                {
                    equationsRPN.Add(expCompiler.compile(implicitDifferentialEquations[i]));
                }
                for (int i = 0; i < explicitDifferentialEquations.Count; i++)
                {
                    SubtractionNode equation = (SubtractionNode)explicitDifferentialEquations[i];
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
    }
}
