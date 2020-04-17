
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace ElectricalPowerSystems.PowerModel
{
    class MainInterpreter
    {
        static public void RunModel(string inputText, ref List<ErrorMessage> errorList, ref List<string> output)
        {
            AntlrInputStream inputStream = new AntlrInputStream(inputText);
            PowerModel.NewModel.ModelGrammarLexer modelLexer = new PowerModel.NewModel.ModelGrammarLexer(inputStream);
            ErrorListener<int> lexerErrorListener = new ErrorListener<int>();
            modelLexer.RemoveErrorListeners();
            modelLexer.AddErrorListener(lexerErrorListener);
            CommonTokenStream commonTokenStream = new CommonTokenStream(modelLexer);
            PowerModel.NewModel.ModelGrammarParser modelParser = new PowerModel.NewModel.ModelGrammarParser(commonTokenStream);
            ErrorListener<IToken> parserErrorListener = new ErrorListener<IToken>();
            modelParser.RemoveErrorListeners();
            modelParser.AddErrorListener(parserErrorListener);

            PowerModel.NewModel.ModelGrammarParser.ModelContext modelContext = modelParser.model();
            errorList.AddRange(lexerErrorListener.GetErrors());
            errorList.AddRange(parserErrorListener.GetErrors());
            if (errorList.Count > 0)
                return;
            PowerModel.NewModel.Grammar.Visitor visitor = new PowerModel.NewModel.Grammar.Visitor();
            PowerModel.NewModel.Grammar.Node root = visitor.VisitModel(modelContext);
            var model = PowerModel.NewModel.ModelInterpreter.GetInstanse().Generate((PowerModel.NewModel.Grammar.ModelNode)root, ref errorList, ref output);
            if (errorList.Count > 0)
                return;
            string solverOutput = model.Solve();
            //string solverOutput = model.GetEquations();
            output.Add(solverOutput);
        }
        static public void RunModelOldTransient(string inputText, ref List<ErrorMessage> errorList, ref List<string> output)
        {
            AntlrInputStream inputStream = new AntlrInputStream(inputText);
            PowerModel.OldModel.OldGrammarLexer modelLexer = new PowerModel.OldModel.OldGrammarLexer(inputStream);
            ErrorListener<int> lexerErrorListener = new ErrorListener<int>();
            modelLexer.RemoveErrorListeners();
            modelLexer.AddErrorListener(lexerErrorListener);
            CommonTokenStream commonTokenStream = new CommonTokenStream(modelLexer);
            PowerModel.OldModel.OldGrammarParser modelParser = new PowerModel.OldModel.OldGrammarParser(commonTokenStream);
            ErrorListener<IToken> parserErrorListener = new ErrorListener<IToken>();
            modelParser.RemoveErrorListeners();
            modelParser.AddErrorListener(parserErrorListener);

            PowerModel.OldModel.OldGrammarParser.ModelContext modelContext = modelParser.model();
            errorList.AddRange(lexerErrorListener.GetErrors());
            errorList.AddRange(parserErrorListener.GetErrors());
            PowerModel.OldModel.ASTVisitor visitor = new PowerModel.OldModel.ASTVisitor();
            PowerModel.OldModel.ASTNode root = visitor.VisitModel(modelContext);
            PowerModel.OldModel.ASTInterpreter interpreter = new PowerModel.OldModel.ASTInterpreter();
            var model = interpreter.generate(root, ref errorList, ref output);
            if (errorList.Count > 0)
                return;
            model.SolveTransient();
        }
        static public void RunModelOld(string inputText,ref List<ErrorMessage> errorList,ref List<string> output)
        {
            AntlrInputStream inputStream = new AntlrInputStream(inputText);
            PowerModel.OldModel.OldGrammarLexer modelLexer = new PowerModel.OldModel.OldGrammarLexer(inputStream);
            ErrorListener<int> lexerErrorListener = new ErrorListener<int>();
            modelLexer.RemoveErrorListeners();
            modelLexer.AddErrorListener(lexerErrorListener);
            CommonTokenStream commonTokenStream = new CommonTokenStream(modelLexer);
            PowerModel.OldModel.OldGrammarParser modelParser = new PowerModel.OldModel.OldGrammarParser(commonTokenStream);
            ErrorListener<IToken> parserErrorListener = new ErrorListener<IToken>();
            modelParser.RemoveErrorListeners();
            modelParser.AddErrorListener(parserErrorListener);

            PowerModel.OldModel.OldGrammarParser.ModelContext modelContext= modelParser.model();
            errorList.AddRange(lexerErrorListener.GetErrors());
            errorList.AddRange(parserErrorListener.GetErrors());
            PowerModel.OldModel.ASTVisitor visitor = new PowerModel.OldModel.ASTVisitor();
            PowerModel.OldModel.ASTNode root=visitor.VisitModel(modelContext);
            PowerModel.OldModel.ASTInterpreter interpreter=new PowerModel.OldModel.ASTInterpreter();
            var model = interpreter.generate(root, ref errorList, ref output);
            if (errorList.Count > 0)
                return;
            List<string> solverOutput = model.Solve();
            output.AddRange(solverOutput);
        }
        static public void EquationGeneration(string inputText, ref List<ErrorMessage> errorList, ref List<string> output)
        {
            AntlrInputStream inputStream = new AntlrInputStream(inputText);
            PowerModel.OldModel.OldGrammarLexer modelLexer = new PowerModel.OldModel.OldGrammarLexer(inputStream);
            ErrorListener<int> lexerErrorListener = new ErrorListener<int>();
            modelLexer.RemoveErrorListeners();
            modelLexer.AddErrorListener(lexerErrorListener);
            CommonTokenStream commonTokenStream = new CommonTokenStream(modelLexer);
            PowerModel.OldModel.OldGrammarParser modelParser = new PowerModel.OldModel.OldGrammarParser(commonTokenStream);
            ErrorListener<IToken> parserErrorListener = new ErrorListener<IToken>();
            modelParser.RemoveErrorListeners();
            modelParser.AddErrorListener(parserErrorListener);

            PowerModel.OldModel.OldGrammarParser.ModelContext modelContext = modelParser.model();
            errorList.AddRange(lexerErrorListener.GetErrors());
            errorList.AddRange(parserErrorListener.GetErrors());
            PowerModel.OldModel.ASTVisitor visitor = new PowerModel.OldModel.ASTVisitor();
            PowerModel.OldModel.ASTNode root = visitor.VisitModel(modelContext);
            PowerModel.OldModel.ASTInterpreter interpreter = new PowerModel.OldModel.ASTInterpreter();
            var model = interpreter.generate(root, ref errorList, ref output);
            if (errorList.Count > 0)
                return;
            List<string> equations = model.EquationGeneration();
            output.AddRange(equations);
        }
        static public void EquationGenerationDAE(string inputText, ref List<ErrorMessage> errorList, ref List<string> output)
        {
            AntlrInputStream inputStream = new AntlrInputStream(inputText);
            PowerModel.OldModel.OldGrammarLexer modelLexer = new PowerModel.OldModel.OldGrammarLexer(inputStream);
            ErrorListener<int> lexerErrorListener = new ErrorListener<int>();
            modelLexer.RemoveErrorListeners();
            modelLexer.AddErrorListener(lexerErrorListener);
            CommonTokenStream commonTokenStream = new CommonTokenStream(modelLexer);
            PowerModel.OldModel.OldGrammarParser modelParser = new PowerModel.OldModel.OldGrammarParser(commonTokenStream);
            ErrorListener<IToken> parserErrorListener = new ErrorListener<IToken>();
            modelParser.RemoveErrorListeners();
            modelParser.AddErrorListener(parserErrorListener);

            PowerModel.OldModel.OldGrammarParser.ModelContext modelContext = modelParser.model();
            errorList.AddRange(lexerErrorListener.GetErrors());
            errorList.AddRange(parserErrorListener.GetErrors());
            PowerModel.OldModel.ASTVisitor visitor = new PowerModel.OldModel.ASTVisitor();
            PowerModel.OldModel.ASTNode root = visitor.VisitModel(modelContext);
            PowerModel.OldModel.ASTInterpreter interpreter = new PowerModel.OldModel.ASTInterpreter();
            var model = interpreter.generate(root, ref errorList, ref output);
            if (errorList.Count > 0)
                return;
            List<string> equations = model.EquationGenerationTransient();
            output.AddRange(equations);
        }
    }
}
