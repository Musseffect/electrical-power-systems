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
    public enum FloatState
    {
        INTEGER,
        DOT,
        FRACTIONAL,
        EXPONENT,
        EXPONENTSIGN,
        EXPONENTPOWER

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
        INTEGER,
        DOT,
        LEFTSQUAREBRACKET,
        RIGHTSQUAREBRACKET,
        LEFTCURLYBRACKET,
        RIGHTCURLYBRACKET
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
        static int parseFloat(string text,int index,ref List<Tokens> tokens)
        {
            float value=0.0f;
            int exponent=0;
            int j = index;
            int k = index;
            if(!Char.IsDigit(text[k]))
            {
                return k;
            }
            FloatState state=FloatState.INTEGER;
            while (true)
            {
                char symbol=text[k];
                value *= 10.0f;
                switch(state)
                {
                    case FloatState.INTEGER:
                        if(Char.IsDigit(symbol))
                        {
                            state=FloatState.INTEGER;
                        }else
                        {
                            switch(symbol)
                            {
                                case '.':
                                case 'E':
                                case 'e':
                            
                            }
                        }
                        break;
                    case FloatState.DOT:
                        if(Char.IsDigit(symbol))
                        {
                            state=FloatState.FRACTIONAL;
                        }else if(Char.IsWhiteSpace())
                        {
                            
                            return ;
                        }else
                        {
                            switch(symbol)
                            {
                                case 'E':
                                case 'e':
                                    state=FloatState.EXPONENT;
                                    break;
                                default:
                                    //error
                            }
                        }
                        break;
                    case FloatState.EXPONENT:
                        if(Char.IsDigit(symbol))
                        {
                            state=FloatState.EXPONENTPOWER;
                        }else if(Char.IsWhiteSpace())
                        {
                            //error
                            return ;
                        }else
                        {
                            switch(symbol)
                            {
                                case '.':
                                case 'E':
                                case 'e':
                            
                            }
                        }
                        break;
                    case FloatState.EXPONENTSIGN:
                        if(Char.IsDigit(symbol))
                        {
                            state=FloatState.EXPONENTPOWER;
                        }else if(Char.IsWhiteSpace())
                        {
                            //error
                        }else
                        {
                            switch(symbol)
                            {
                                case 'E':
                                case 'e':
                                default:
                            }
                        }
                        break;
                    case FloatState.EXPONENTPOWER:
                        if(Char.IsDigit(symbol))
                        {
                            state=FloatState.EXPONENTPOWER;
                        }else if(Char.IsWhiteSpace())
                        {
                            //return float
                            tokens.Add(new Float());
                            return ;
                        }else
                        {
                            //error
                        }
                        break;
                }
                k++;
                if (k == text.Length)
                {
                    if(state==FloatState.INTEGER||state==FloatState.DOT||state==FloatState.EXPONENTPOWER)
                        tokens.Add(new Float(value));
                    return k;
                }
            }



        }
        static public List<Token> runLexer(string text)
        {
            List<Token> tokens=new List<Token>();
            for (int i = 0; i < text.Length; i++)
            {
                char symbol=text[i];
                switch (symbol)
                {
                    case '=':
                        tokens.Add(new Token(TokenType.EQUAL));
                        continue;
                    case '.':
                        tokens.Add(new Token(TokenType.DOT));
                        continue;
                    case ',':
                        tokens.Add(new Token(TokenType.COMMA));
                        continue;
                        break;
                    case ';':
                        tokens.Add(new Token(TokenType.SEMICOLON));
                        continue;
                        break;
                    case ':':
                        tokens.Add(new Token(TokenType.COLON));
                        continue;
                    case '(':
                        tokens.Add(new Token(TokenType.LEFTBRACKET));
                        continue;
                        break;
                    case ')':
                        tokens.Add(new Token(TokenType.RIGHTBRACKER));
                        continue;
                        break;
                    case '[':
                        tokens.Add(new Token(TokenType.LEFTSQUAREBRACKET));
                        continue;
                        break;
                    case ']':
                        tokens.Add(new Token(TokenType.RIGHTSQUAREBRACKET));
                        continue;
                        break;
                    case '{':
                        tokens.Add(new Token(TokenType.LEFTCURLYBRACKET));
                        continue;
                        break;
                    case '}':
                        tokens.Add(new Token(TokenType.RIGHTCURLYBRACKET));
                        continue;
                        break;
                    case '-':
                        tokens.Add(new Token(TokenType.MINUS));
                        continue;
                        break;
                    case '+':
                        tokens.Add(new Token(TokenType.PLUS));
                        continue;
                        break;
                    case '*':
                        tokens.Add(new Token(TokenType.MULTIPLY));
                        continue;
                        break;
                    case '/':
                        tokens.Add(new Token(TokenType.DIVIDE));
                        continue;
                        break;
                }
                if (Char.IsDigit(symbol))
                {
                    
                }
                else if (Char.IsLetter(symbol) || symbol == '_')
                {
                    StringBuilder id=new StringBuilder();
                    while (true)
                    {
                        if (Char.IsLetterOrDigit(symbol) || symbol == '_')
                        {
                            id.Append(symbol);
                        } else
                        {
                            tokens.Add(new Identifier(id));
                        }
                        i++;
                        if(Char.isWhiteSpace(symbol)||symbol == text.Length)
                        {
                            tokens.Add(new Identifier(id));
                        }
                    }
                }else
                {
                    //error
                }
            }
            return tokens;
        }
    }
    class ModelParser
    {
    }
}
