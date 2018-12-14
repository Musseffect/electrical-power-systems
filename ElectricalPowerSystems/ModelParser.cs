using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems
{
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
        FLOAT,
        DOT,
        LEFTSQUAREBRACKET,
        RIGHTSQUAREBRACKET,
        LEFTCURLYBRACKET,
        RIGHTCURLYBRACKET,
        STRING,
        ERRORWORD
    }
    public class ErrorMessage
    {
        string message;
        int line;
        int position;
        public string Message
        {
            get { return message; }
        }
        public string Line
        {
            get
            {
                return line == -1?"":line.ToString();
            }
        }
        public string Position
        {
            get
            {
                return position == -1 ? "" : position.ToString();
            }
        }
        public ErrorMessage(string message,int line =-1, int position= -1)
        {
            this.message = message;
            this.line = line;
            this.position = position;
        }
    }
    public class Token
    {
        protected TokenType type;
        int line;
        int position;
        public TokenType Type {
            get { return type; }
        }
        public Token(TokenType type)
        {
            this.type = type;
            line = -1;
            position = -1;
        }
        public Token(TokenType type, int line=-1, int position=-1)
        {
            this.type = type;
            this.line = line;
            this.position = position;
        }
        public virtual string getTokenString()
        {
            return type.ToString("g");
        }
    }
    class ErrorWord : Token
    {
        string value;
        public ErrorWord(string value):base(TokenType.ERRORWORD)
        {
            this.value = value;
        }
        public ErrorWord(string value, int line = -1, int position = -1) :base(TokenType.ERRORWORD, line,position)
        {
            this.value = value;
        }
        public override string getTokenString()
        {
            return type.ToString("g") + "(" + value + ")";
        }
    }
    class Float : Token
    {
        float value;
        public Float(float value) : base(TokenType.FLOAT)
        {
            type = TokenType.FLOAT;
            this.value = value;
        }
        public Float(float value, int line = -1, int position = -1) : base(TokenType.FLOAT, line, position)
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
        public Identifier(string value):base(TokenType.IDENTIFIER)
        {
            type = TokenType.IDENTIFIER;
            this.value = value;
        }
        public Identifier(string value, int line = -1, int position = -1) : base(TokenType.IDENTIFIER, line, position)
        {
            this.value = value;
        }
        public override string getTokenString()
        {
            return type.ToString("g")+"("+value+")";
        }
    }
    class StringConstant : Token
    {
        string value;
        public StringConstant(string value) : base(TokenType.STRING)
        {
            type = TokenType.IDENTIFIER;
            this.value = value;
        }
        public StringConstant(string value, int line = -1, int position = -1) : base(TokenType.STRING, line, position)
        {
            this.value = value;
        }
        public override string getTokenString()
        {
            return type.ToString("g") + "(" + value + ")";
        }
    }
    public class Lexer
    {
        enum LexerState
        {
            INTEGER,
            FLOATDOT,
            FLOATFRACTIONAL,
            FLOATEXPONENT,
            FLOATEXPONENTSIGN,
            FLOATEXPONENTPOWER,
            IDENTIFIER,
            INITIAL,
            COMMENTARY,
            INVALIDCHARACTERS,
            STRINGCONSTANT,
            STRINGCONSTANT2
        }
        static public List<Token> runLexer(string text, ref ObservableCollection<ErrorMessage> errors)
        {
            List<Token> tokens = new List<Token>();
            LexerState state = LexerState.INITIAL;
            StringBuilder strbuilder = new StringBuilder();
            int line=1;
            int position=1;
            int lastPosition = position;

            int value = 0;
            int fraction = 0;
            int fractionExponent = 0;
            int exponent = 0;
            bool sign = false;
            for (int i = 0; i < text.Length; i++, position++)
            {
                char symbol = text[i];
                switch (state)
                {
                    case LexerState.INTEGER:
                        strbuilder.Append(symbol);
                        if (Char.IsDigit(symbol))
                        {
                            value *= 10;
                            value += (symbol - '0');
                            state = LexerState.INTEGER;
                        }
                        else
                        {
                            switch (symbol)
                            {
                                case '.':
                                    state = LexerState.FLOATDOT;
                                    break;
                                case 'E':
                                case 'e':
                                    state = LexerState.FLOATEXPONENT;
                                    break;
                                default:
                                    tokens.Add(new Float(value,line,lastPosition));
                                    state = LexerState.INITIAL;
                                    value = 0;
                                    fraction = 0;
                                    fractionExponent = 0;
                                    exponent = 0;
                                    i--;
                                    position--;
                                    strbuilder.Clear();
                                    continue;
                            }
                        }
                        break;
                    case LexerState.FLOATDOT:
                        strbuilder.Append(symbol);
                        if (Char.IsDigit(symbol))
                        {
                            state = LexerState.FLOATFRACTIONAL;
                            value *= 10;
                            value += (symbol - '0');
                            fractionExponent = -1;
                        }
                        else if (Char.IsWhiteSpace(symbol))
                        {
                            tokens.Add(new Float(value, line, lastPosition));
                            state = LexerState.INITIAL;
                            value = 0;
                            fraction = 0;
                            fractionExponent = 0;
                            exponent = 0;
                            strbuilder.Clear();
                            break;
                        }
                        else
                        {
                            switch (symbol)
                            {
                                case 'E':
                                case 'e':
                                    state = LexerState.FLOATEXPONENT;
                                    break;
                                default:
                                    state = LexerState.INITIAL;
                                    tokens.Add(new Float(value,line,lastPosition));
                                    value = 0;
                                    fraction = 0;
                                    fractionExponent = 0;
                                    exponent = 0;
                                    strbuilder.Clear();
                                    break;
                            }
                        }
                        break;
                    case LexerState.FLOATFRACTIONAL:
                        strbuilder.Append(symbol);
                        if (Char.IsDigit(symbol))
                        {
                            strbuilder.Append(symbol);
                            fractionExponent -= 1;
                            value *= 10;
                            value += (symbol - '0');
                            state = LexerState.FLOATFRACTIONAL;
                        }
                        else
                        {
                            switch (symbol)
                            {
                                case 'E':
                                case 'e':
                                    state = LexerState.FLOATEXPONENT;
                                    break;
                                default:
                                    tokens.Add(new Float(value * (float)Math.Pow(10.0f, fractionExponent)));
                                    value = 0;
                                    fraction = 0;
                                    fractionExponent = 0;
                                    exponent = 0;
                                    i--;
                                    position--;
                                    state = LexerState.INITIAL;
                                    strbuilder.Clear();
                                    break;
                            }
                        }
                        break;
                    case LexerState.FLOATEXPONENT:
                        strbuilder.Append(symbol);
                        if (Char.IsDigit(symbol))
                        {
                            state = LexerState.FLOATEXPONENTPOWER;
                            exponent = symbol - '0';
                        }
                        else if (Char.IsWhiteSpace(symbol))
                        {
                            errors.Add(new ErrorMessage("Неожиданный конец",line,position));
                            tokens.Add(new Float(value * (float)Math.Pow(10.0f, fractionExponent), line,position));
                            state = LexerState.INITIAL;
                            value = 0;
                            fraction = 0;
                            fractionExponent = 0;
                            exponent = 0;
                            strbuilder.Clear();
                            break;
                        }
                        else
                        {
                            switch (symbol)
                            {
                                case '+':
                                    sign = false;
                                    state = LexerState.FLOATEXPONENTSIGN;
                                    break;
                                case '-':
                                    sign = true;
                                    state = LexerState.FLOATEXPONENTSIGN;
                                    break;
                                default:
                                    state = LexerState.INVALIDCHARACTERS;
                                    value = 0;
                                    fraction = 0;
                                    fractionExponent = 0;
                                    exponent = 0;
                                    break;
                            }
                        }
                        break;
                    case LexerState.FLOATEXPONENTSIGN:
                        strbuilder.Append(symbol);
                        if (Char.IsDigit(symbol))
                        {
                            state = LexerState.FLOATEXPONENTPOWER;
                            exponent = symbol - '0';
                        }
                        else if (Char.IsWhiteSpace(symbol))
                        {
                            state = LexerState.INITIAL;
                            errors.Add(new ErrorMessage("Неожиданный конец"));
                            tokens.Add(new Float(value * (float)Math.Pow(10.0f, fractionExponent), line, lastPosition));
                            i--;
                            position--;
                            value = 0;
                            fraction = 0;
                            fractionExponent = 0;
                            exponent = 0;
                            strbuilder.Clear();

                        }
                        else
                        {
                            state = LexerState.INVALIDCHARACTERS;
                            value = 0;
                            fraction = 0;
                            fractionExponent = 0;
                            exponent = 0;
                        }
                        break;
                    case LexerState.FLOATEXPONENTPOWER:
                        strbuilder.Append(symbol);
                        if (Char.IsDigit(symbol))
                        {
                            state = LexerState.FLOATEXPONENTPOWER;
                        }
                        else if (Char.IsWhiteSpace(symbol))
                        {
                            state = LexerState.INITIAL;
                            float val = value * (float)Math.Pow(10.0, fractionExponent + (sign ? -exponent : exponent));
                            tokens.Add(new Float(val,line,lastPosition));
                            i--;
                            position--;
                            value = 0;
                            fraction = 0;
                            fractionExponent = 0;
                            exponent = 0;
                            strbuilder.Clear();
                        }
                        else
                        {
                            state = LexerState.INVALIDCHARACTERS;
                            value = 0;
                            fraction = 0;
                            fractionExponent = 0;
                            exponent = 0;
                        }
                        break;
                    case LexerState.IDENTIFIER:
                        if (Char.IsLetterOrDigit(symbol) || symbol == '_')
                        {
                            strbuilder.Append(symbol);
                            state = LexerState.IDENTIFIER;
                        }
                        else
                        {
                            i--;
                            position--;
                            tokens.Add(new Identifier(strbuilder.ToString(), line, position));
                            state = LexerState.INITIAL;
                            strbuilder.Clear();
                        }
                        break;
                    case LexerState.INITIAL:
                        if (Char.IsWhiteSpace(symbol))
                        {
                            continue;
                        }
                        else if (Char.IsDigit(symbol))
                        {
                            lastPosition = position;
                            strbuilder.Append(symbol);
                            state = LexerState.INTEGER;
                            value = symbol - '0';
                            continue;
                        }
                        else if (Char.IsLetter(symbol) || symbol == '_')
                        {
                            lastPosition = position;
                            state = LexerState.IDENTIFIER;
                            strbuilder.Append(symbol);
                            continue;
                        }
                        switch (symbol)
                        {
                            case '=':
                                tokens.Add(new Token(TokenType.EQUAL, line, position));
                                continue;
                            case ',':
                                tokens.Add(new Token(TokenType.COMMA, line, position));
                                continue;
                            case ';':
                                tokens.Add(new Token(TokenType.SEMICOLON, line, position));
                                continue;
                            case ':':
                                tokens.Add(new Token(TokenType.COLON, line, position));
                                continue;
                            case '(':
                                tokens.Add(new Token(TokenType.LEFTBRACKET, line, position));
                                continue;
                            case ')':
                                tokens.Add(new Token(TokenType.RIGHTBRACKER, line, position));
                                continue;
                            case '[':
                                tokens.Add(new Token(TokenType.LEFTSQUAREBRACKET, line, position));
                                continue;
                            case ']':
                                tokens.Add(new Token(TokenType.RIGHTSQUAREBRACKET, line, position));
                                continue;
                            case '{':
                                tokens.Add(new Token(TokenType.LEFTCURLYBRACKET, line, position));
                                continue;
                            case '}':
                                tokens.Add(new Token(TokenType.RIGHTCURLYBRACKET, line, position));
                                continue;
                            case '-':
                                tokens.Add(new Token(TokenType.MINUS, line, position));
                                continue;
                            case '+':
                                tokens.Add(new Token(TokenType.PLUS, line, position));
                                continue;
                            case '*':
                                tokens.Add(new Token(TokenType.MULTIPLY, line, position));
                                continue;
                            case '"':
                                state = LexerState.STRINGCONSTANT;
                                continue;
                            case '\'':
                                state = LexerState.STRINGCONSTANT2;
                                continue;
                            case '/':
                                if (i + 1 != text.Length)
                                {
                                    if (text[i + 1] == '*')
                                    {
                                        i = i + 1;
                                        position++;
                                        state = LexerState.COMMENTARY;
                                        continue;
                                    }
                                }
                                tokens.Add(new Token(TokenType.DIVIDE, line, position));
                                continue;
                            case '.':
                                if (i + 1 != text.Length)
                                {
                                    if (Char.IsDigit(text[i + 1]))
                                    {
                                        strbuilder.Append(symbol);
                                        lastPosition = position;
                                        state = LexerState.FLOATDOT;
                                        continue;
                                    }
                                }
                                tokens.Add(new Token(TokenType.DOT, line, position));
                                continue;
                            default:
                                lastPosition = position;
                                state = LexerState.INVALIDCHARACTERS;
                                break;
                        }
                        break;
                    case LexerState.COMMENTARY:
                        if (symbol == '*')
                        {
                            if (i + 1 != text.Length)
                            {
                                if (text[i + 1] == '/')
                                {
                                    state = LexerState.INITIAL;
                                    i++;
                                    position++;
                                }
                            }
                        }
                        continue;
                    case LexerState.INVALIDCHARACTERS:
                        if (Char.IsWhiteSpace(symbol))
                        {
                            tokens.Add(new ErrorWord(strbuilder.ToString(), line, lastPosition));
                            i--;
                            position--;
                            strbuilder.Clear();
                            state = LexerState.INITIAL;
                        }
                        else if (symbol == ';')
                        {
                            tokens.Add(new ErrorWord(strbuilder.ToString(), line, lastPosition));
                            i--;
                            position--;
                            strbuilder.Clear();
                            state = LexerState.INITIAL;
                        }
                        continue;
                    case LexerState.STRINGCONSTANT:
                        if (symbol=='"')
                        {
                            tokens.Add(new StringConstant(strbuilder.ToString(), line, lastPosition));
                            strbuilder.Clear();
                            continue;
                        }
                        strbuilder.Append(symbol);
                        break;
                    case LexerState.STRINGCONSTANT2:
                        if (symbol == '\'')
                        {
                            tokens.Add(new StringConstant(strbuilder.ToString(), line, lastPosition));
                            strbuilder.Clear();
                            continue;
                        }
                        strbuilder.Append(symbol);
                        break;
                }
                if (symbol == '\n')
                {
                    line++;
                    position = 1;
                }
            }
            switch(state)
            {
                case LexerState.INTEGER:
                    break;
                case LexerState.FLOATDOT:
                    break;
                case LexerState.FLOATFRACTIONAL:
                    break;
                case LexerState.FLOATEXPONENT:
                    break;
                case LexerState.FLOATEXPONENTSIGN:
                    break;
                case LexerState.FLOATEXPONENTPOWER:
                    break;
                case LexerState.IDENTIFIER:
                    break;
                case LexerState.COMMENTARY:
                    break;
                case LexerState.INVALIDCHARACTERS:
                    break;
                case LexerState.STRINGCONSTANT:
                    break;
                case LexerState.STRINGCONSTANT2:
                    break;
            }
            return tokens;
        }
    }
     class ModelParser
    {
        void parse()
        {


        }
    }
}
