using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace ElectricalPowerSystems.Test
{
    class TestParser
    {
        static public void test()
        {
            string text = @"/*Possible format:*/
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
            AntlrInputStream inputStream = new AntlrInputStream(text);
            ModelGrammarLexer modelLexer = new ModelGrammarLexer(inputStream);
            CommonTokenStream commonTokenStream = new CommonTokenStream(modelLexer);
            ModelGrammarParser modelParser = new ModelGrammarParser(commonTokenStream);
            
            ModelGrammarParser.ModelContext modelContext= modelParser.model();
        }


}
}
