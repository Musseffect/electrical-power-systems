using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems
{
    public interface TriggerInterface<State, Symbol>
    {
        public State getState(State state, Symbol symbol);
    }
    class StateMachine<State,Trigger,Symbol> where Trigger: TriggerInterface<State,Symbol>, new()
    {
        State currentState;
        Trigger trigger;
        public StateMachine()
        {
            trigger = new Trigger();
        }
        void Init(State initialState)
        {
            currentState = initialState;
        }
        void setState(State state)
        {
            currentState = state;
        }
        State getState(Symbol symbol)
        {
            currentState = trigger.getState(currentState, symbol);
            return currentState;
        }

    }
    public enum TokenType
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
    public class Token
    {
        protected TokenType type;
        public TokenType Type {
            get { return type; }
        }
        public Token(TokenType type)
        {
            this.type = type;
        }
        public virtual string getTokenString()
        {
            return type.ToString("g");
        }
    }
    class Float : Token
    {
        float value;
        public Float(float value)
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
        public Identifier(string value)
        {
            type = TokenType.IDENTIFIER;
            this.value = value;
        }
        public override string getTokenString()
        {
            return type.ToString("g")+"("+value+")";
        }
    }
    public class Lexer
    {
        static public List<Token> runLexer(string text)
        {
            List<Token> tokens=new List<Token>();
            for (int i = 0; i < text.Length; i++)
            {
                char symbol=text[i];
                switch (symbol)
                {
                    case '.':
                        break;
                    case ',':
                        break;
                    case ';':
                        break;
                    case '(':
                        break;
                    case ')':
                        break;
                    case '[':
                        break;
                    case ']':
                        break;
                    case '{':
                        break;
                    case '}':
                        break;
                    case '-':
                        break;
                    case '+':
                        break;
                    case '*':
                        break;
                    case '/':
                        break;
                }
                if (Char.IsDigit(symbol))
                {
                    float value=0.0f;
                    int j = i;
                    int k = i;
                    while (true)
                    {
                        value *= 10.0f;
                        if (k == text.Length)
                        {
                            tokens.Add(new Float(value));       
                        }
                        if (Char.IsDigit(symbol))
                        {
                            value += (float)(symbol - 'a');
                        }
                        switch (symbol)
                        {
                            case 'e':
                            case 'E':
                            case '';
                        }
                        k++;
                    }

                }
                else if (Char.IsLetter(symbol) || symbol == '_')
                {
                    string id="";
                    int j = i;
                    int k = i;
                    while (true)
                    {
                        if (k == text.Length)
                        {
                            id.
                        }
                        if (Char.IsLetterOrDigit(symbol) || symbol == '_')
                        {

                        } else
                        {
                            tokens.Add(new Identifier(id));
                        }
                        k++;
                    }

                }

            }

            return tokens;
        }
    }
    class ModelParser
    {
    }
}
