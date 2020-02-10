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
        static public void compile(string inputText,ref List<ErrorMessage> errorList,ref List<string> output)
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
            Interpreter.ASTVisitor visitor = new Interpreter.ASTVisitor();
            Interpreter.ASTNode root=visitor.VisitModel(modelContext);
            Interpreter.ASTInterpreter interpreter=new Interpreter.ASTInterpreter();
            var model = interpreter.generate(root, ref errorList, ref output);
            if (errorList.Count > 0)
                return;
            List<string> solverOutput = model.Solve();
            output.AddRange(solverOutput);
        }


}
}
