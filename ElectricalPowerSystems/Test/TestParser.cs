using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace ElectricalPowerSystems.Test
{
    class ErrorListener : IAntlrErrorListener<int>
    {
        public void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] int offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            Console.WriteLine(msg);
        }
    }
    class TestParser
    {
        static public void test(string inputText,ref List<ModelParsing.ErrorMessage> errorList,ref List<string> output)
        {
            string text = @"/*Possible format:*/
                    nodeLabel1=""label1"";
                    element = resistor(value, nodeLabel1, nodeLabel2);
                    element=voltageSource(value, nodeLabel1, nodeLabel2);
                    element=ground(nodeLabel);
                    element=capacitor(value, nodeLabel1, nodeLabel2);
                    element=inductor(value, nodeLabel1, nodeLabel2);
                    element=currentSource(value, nodeLabel1, nodeLabel2);
                    element=node(nodeLabel);
                    element=line(nodeLabel1, nodeLabel2);/* - zero resistance*/
                    current(element);
                    voltage(element);/* - voltage drop on element*/
                    voltage(node);/* - relative voltage on node*/";
            AntlrInputStream inputStream = new AntlrInputStream(inputText);
            ModelGrammarLexer modelLexer = new ModelGrammarLexer(inputStream);
            modelLexer.RemoveErrorListeners();
            modelLexer.AddErrorListener(new ErrorListener());
            CommonTokenStream commonTokenStream = new CommonTokenStream(modelLexer);
            /*commonTokenStream.Fill();
            foreach (var token in commonTokenStream.GetTokens())
            {
                output.Add(token.Text+" "+(token.Type>=0?modelLexer.TokenNames[token.Type]:""));
            }
            return;*/
            ModelGrammarParser modelParser = new ModelGrammarParser(commonTokenStream);
            ModelGrammarParser.ModelContext modelContext= modelParser.model();
            SimpleParser.ASTVisitor visitor = new SimpleParser.ASTVisitor();
            SimpleParser.ASTNode root=visitor.VisitModel(modelContext);
            SimpleParser.ASTInterpreter interpreter=new SimpleParser.ASTInterpreter();
            var model = interpreter.generate(root, ref errorList, ref output);
            if (errorList.Count > 0)
                return;
            List<string> solverOutput = ModelSolver.SolveAC(model);
            output.AddRange(solverOutput);
        }


}
}
