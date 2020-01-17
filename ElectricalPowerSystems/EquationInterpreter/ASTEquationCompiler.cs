using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.EquationInterpreter
{

    class ErrorListener : IAntlrErrorListener<int>
    {
        public void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] int offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            Console.WriteLine(msg);
        }
    }
    class ParserErrorListener : IAntlrErrorListener<int>
    {
        public void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] int offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            Console.WriteLine(msg);
        }
    }

    public partial class ASTEquationCompiler
    {
        public class NonlinearEquationDefinition
        {
            double[] initialValues;
            string[] variableNames;
            List<RPNExpression> equations;
        }

        static public NonlinearEquationDefinition compileEquations(string text)
        {
            AntlrInputStream inputStream = new AntlrInputStream(text);
            EquationGrammarLexer eqLexer = new EquationGrammarLexer(inputStream);
            eqLexer.RemoveErrorListeners();
            ErrorListener lexerListener = new ErrorListener();
            eqLexer.AddErrorListener(lexerListener);
            CommonTokenStream commonTokenStream = new CommonTokenStream(eqLexer);
            EquationGrammarParser eqParser = new EquationGrammarParser(commonTokenStream);
            ParserErrorListener parserListener = new ParserErrorListener();
            EquationGrammarParser eqParser = new EquationGrammarParser(commonTokenStream);
            eqParser.RemoveErrorListeners();
            eqParser.AddErrorListener(parserListener);
            EquationGrammarParser.CompileUnitContext expContext = eqParser.compileUnit();
            List<string> errors = lexerListener.getErrors();
            if (errors.Count > 0)
            {
                throw new Exception("Lexer Error");
            }
            errors = parserListener.getErrors();
            if (errors.Count > 0)
            {
                throw new Exception("Parser error");
            }
            /*EquationGrammarVisitor visitor = new EquationGrammarVisitor();
            ASTNode root = visitor.VisitCompileUnit(expContext);
            var rootSimple = Compiler.ASTCompiler.validate(root);
            rootSimple = Compiler.ASTCompiler.simplify(rootSimple);
            return compileASTExpression(rootSimple);*/
        }
        static private NonlinearEquationDefinition compile()
        {

        }
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
        }
        static void compileNegation(SimpleNegationNode node)
        {
            compileVisitor(node.Node);
            rpn.Add(new NegationOperator());
        }
        static void compilePower(SimplePowerNode node)
        {
            compileVisitor(node.Left);
            compileVisitor(node.Right);
            rpn.Add(new PowerOperator());
        }
        static void compileMultiplication(SimpleMultiplicationNode node)
        {
            compileVisitor(node.Left);
            compileVisitor(node.Right);
            rpn.Add(new MultiplicationOperator());
        }
        static void compileDivision(SimpleDivisionNode node)
        {
            compileVisitor(node.Left);
            compileVisitor(node.Right);
            rpn.Add(new DivisionOperator());
        }
        static void compileFunction(SimpleFunctionNode node)
        {
            //check function signature
            foreach (var arg in node.Args)
                compileVisitor(arg);
            rpn.Add(new Function(node.Func));
        }
        static void compileSubtraction(SimpleSubtractionNode node)
        {
            compileVisitor(node.Left);
            compileVisitor(node.Right);
            rpn.Add(new SubtractionOperator());
        }
        static void compileAddition(SimpleAdditionNode node)
        {
            compileVisitor(node.Left);
            compileVisitor(node.Right);
            rpn.Add(new AdditionOperator());
        }
        static void compileIdentifier(SimpleIdentifierNode node)
        {
            int index;
            if (!indicies.TryGetValue(node.VariableName, out index))
            {
                index = varCount;
                indicies[node.VariableName] = index;
                varCount++;
            }
            rpn.Add(new Variable(index));
        }
        static void compileConstant(SimpleFloatNode node)
        {
            rpn.Add(new Operand(node.Value));
        }
        static void compileVisitor(ASTSimpleNode node)
        {
            switch (node.Type)
            {
                case ASTSimpleNode.EType.Negation:
                    compileNegation((SimpleNegationNode)node);
                    break;
                case ASTSimpleNode.EType.Power:
                    compilePower((SimplePowerNode)node);
                    break;
                case ASTSimpleNode.EType.Multiplication:
                    compileMultiplication((SimpleMultiplicationNode)node);
                    break;
                case ASTSimpleNode.EType.Division:
                    compileDivision((SimpleDivisionNode)node);
                    break;
                case ASTSimpleNode.EType.Function:
                    compileFunction((SimpleFunctionNode)node);
                    break;
                case ASTSimpleNode.EType.Subtraction:
                    compileSubtraction((SimpleSubtractionNode)node);
                    break;
                case ASTSimpleNode.EType.Addition:
                    compileAddition((SimpleAdditionNode)node);
                    break;
                case ASTSimpleNode.EType.Identifier:
                    compileIdentifier((SimpleIdentifierNode)node);
                    break;
                case ASTSimpleNode.EType.Float:
                    compileConstant((SimpleFloatNode)node);
                    break;
            }
            return;
        }
        static List<StackElement> rpn;
        static Dictionary<string, int> indicies;
        static int varCount;
    }
}
