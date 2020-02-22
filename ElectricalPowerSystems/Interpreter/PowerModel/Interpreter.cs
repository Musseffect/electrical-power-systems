using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace ElectricalPowerSystems.Interpreter
{
    class MainInterpreter
    {
        static public void SolveModel(string inputText,ref List<ErrorMessage> errorList,ref List<string> output)
        {
            AntlrInputStream inputStream = new AntlrInputStream(inputText);
            ModelGrammarLexer modelLexer = new ModelGrammarLexer(inputStream);
            ErrorListener<int> lexerErrorListener = new ErrorListener<int>();
            modelLexer.RemoveErrorListeners();
            modelLexer.AddErrorListener(lexerErrorListener);
            CommonTokenStream commonTokenStream = new CommonTokenStream(modelLexer);
            ModelGrammarParser modelParser = new ModelGrammarParser(commonTokenStream);
            ErrorListener<IToken> parserErrorListener = new ErrorListener<IToken>();
            modelParser.RemoveErrorListeners();
            modelParser.AddErrorListener(parserErrorListener);

            ModelGrammarParser.ModelContext modelContext= modelParser.model();
            Interpreter.PowerModel.ASTVisitor visitor = new Interpreter.PowerModel.ASTVisitor();
            Interpreter.PowerModel.ASTNode root=visitor.VisitModel(modelContext);
            Interpreter.PowerModel.ASTInterpreter interpreter=new Interpreter.PowerModel.ASTInterpreter();
            var model = interpreter.generate(root, ref errorList, ref output);
            if (errorList.Count > 0)
                return;
            List<string> solverOutput = model.Solve();
            output.AddRange(solverOutput);
        }

        static public void EquationGeneration(string inputText, ref List<ErrorMessage> errorList, ref List<string> output)
        {
            AntlrInputStream inputStream = new AntlrInputStream(inputText);
            ModelGrammarLexer modelLexer = new ModelGrammarLexer(inputStream);
            ErrorListener<int> lexerErrorListener = new ErrorListener<int>();
            modelLexer.RemoveErrorListeners();
            modelLexer.AddErrorListener(lexerErrorListener);
            CommonTokenStream commonTokenStream = new CommonTokenStream(modelLexer);
            ModelGrammarParser modelParser = new ModelGrammarParser(commonTokenStream);
            ErrorListener<IToken> parserErrorListener = new ErrorListener<IToken>();
            modelParser.RemoveErrorListeners();
            modelParser.AddErrorListener(parserErrorListener);

            ModelGrammarParser.ModelContext modelContext = modelParser.model();
            Interpreter.PowerModel.ASTVisitor visitor = new Interpreter.PowerModel.ASTVisitor();
            Interpreter.PowerModel.ASTNode root = visitor.VisitModel(modelContext);
            Interpreter.PowerModel.ASTInterpreter interpreter = new Interpreter.PowerModel.ASTInterpreter();
            var model = interpreter.generate(root, ref errorList, ref output);
            if (errorList.Count > 0)
                return;
            List<string> equations = model.EquationGeneration();
            output.AddRange(equations);
        }

    }
}
