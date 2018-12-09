using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems
{
    enum TokenType
    {
        IDENTIFIER,
        LEFTBRACKET,
        COMMA,
        RIGHTBRACKER,
        SEMICOLON,
        EQUAL,
        PLUS,
        MINUS,
        DIVIDE,
        MULTIPLY,
        COLON,
        NEWLINE,
        INTEGER,
        DOT
    }
    class Token
    {
        protected TokenType type;
        public TokenType Type {
            get { return type; }
        }
        public virtual string getTokenString()
        {
            return type.ToString("g");
        }
    }
    class Float : Token
    {
        float value;
        Float(float value)
        {
            this.value = value;
        }
        public override string getTokenString()
        {
            return type.ToString("g") + "(" + value.ToString() + ")";
        }
    }
    class Identifier:Token
    {
        string value;
        Identifier(string value)
        {
            type = TokenType.IDENTIFIER;
            this.value = value;
        }
        public override string getTokenString()
        {
            return type.ToString("g")+"("+value+")";
        }
    }
    class Lexer
    {
        static public List<Token> runLexer(string text)
        {
            List<Token> tokens=new List<Token>();
            for (int i = 0; i < text.Length; i++)
            {


            }

            return tokens;
        }
    }
    class ModelParser
    {
    }
}
