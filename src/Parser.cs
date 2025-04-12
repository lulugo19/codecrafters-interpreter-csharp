using System.Diagnostics;
using System.Globalization;
using AST;

public class Parser
{
    public class ParserException : Exception
    {
        public ParserException(string msg) : base(msg) {}
    }

    private readonly string _source;
    private readonly List<Token> _tokens;
    private int _current;

    public bool HasErrors {get; private set;} = false;

    public Parser(string source)
    {
        _source = source;
        _current = 0;
        _tokens = new Scanner(source).ScanTokens();
    }

    public Expr? ParseExpression()
    {
        HasErrors = false;
        try
        {
            return _Expression();
        }
        catch (ParserException e)
        {
            return null;
        }       
    }

    public Expr _Expression()
    {
        return _Equality();
    }

    public Expr _Equality()
    {
        var expr = _Comparison();

        while (true)
        {
            if (_Match(TokenType.EQUAL_EQUAL))
            {
                expr = new Expr.Binary.Equal(expr, _Comparison());
            }
            else if (_Match(TokenType.BANG_EQUAL))
            {
                expr = new Expr.Binary.UnEqual(expr, _Comparison());
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    public Expr _Comparison()
    {
        var comp = _Term();
        while (true)
        {
            if (_Match(TokenType.GREATER))
            {
                comp = new Expr.Binary.Greater(comp, _Term());
            }
            else if (_Match(TokenType.GREATER_EQUAL))
            {
                comp = new Expr.Binary.GreaterEq(comp, _Term());
            }
            else if (_Match(TokenType.LESS))
            {
                comp = new Expr.Binary.Less(comp, _Term());
            }
            else if (_Match(TokenType.LESS_EQUAL))
            {
                comp = new Expr.Binary.LessEq(comp, _Term());
            }
            else
            {
                break;
            }
        }
        return comp;
    }

    public Expr _Term()
    {
        var term = _Factor();
        while (true)
        {
            if (_Match(TokenType.PLUS))
            {
                term = new Expr.Binary.Add(term, _Factor());
            }
            else if (_Match(TokenType.MINUS))
            {
                term = new Expr.Binary.Sub(term, _Factor());
            }
            else
            {
                break;
            }
        }
        return term;
    }

    public Expr _Factor()
    {
        var factor = _Unary();
        while (true)
        {
            if (_Match(TokenType.STAR))
            {
                factor = new Expr.Binary.Mul(factor, _Unary());
            }
            else if (_Match(TokenType.SLASH))
            {
                factor = new Expr.Binary.Div(factor, _Unary());
            }
            else
            {
                break;
            }
        }
        return factor;
    }

    public Expr _Unary()
    {
        if (_Match(TokenType.MINUS))
        {
            return new Expr.Unary.Negation(_Unary());
        }
        if (_Match(TokenType.BANG))
        {
            return new Expr.Unary.Not(_Unary());
        }
        else
        {
            return _Primary();
        }      
    }

    public Expr _Primary()
    {
        if (_Match(TokenType.NIL)) return new Expr.Literal(Literal.Nil.Instance);
        if (_Match(TokenType.TRUE)) return new Expr.Literal(new Literal.Boolean(true));
        if (_Match(TokenType.FALSE)) return new Expr.Literal(new Literal.Boolean(false));
        if (_Match(TokenType.NUMBER)) return new Expr.Literal((Literal.Number)_Previous().Literal);
        if (_Match(TokenType.STRING)) return new Expr.Literal(new Literal.String(_Previous().Literal as string));
        if (_Match(TokenType.LEFT_PAREN)) 
        {
            var expr = _Expression();
            _Advance();
            return new Expr.Group(expr);
        }
        throw _Error();    
    }

    private ParserException _Error()
    {
        HasErrors = true;
        var msg = "No valid expression";
        if (_current > 0) 
        {          
            var token = _Previous();
            msg = $"[line {token.Line}] Error at '{token.Lexeme}': Expect expression.";
        }      
        Console.Error.WriteLine(msg);
        return new ParserException(msg);
    }


    private bool _IsAtEnd()
    {
        return _current >= _tokens.Count;
    }

    private Token _Advance()
    {
        return _tokens[_current++];
    }

    private Token _Expect(TokenType type)
    {
        if (_Check(type)) return _Advance();

        throw new Exception($"Expected token '{type}' but got '{_Peek().Type}'");
    }

    private Token _Peek()
    {
        return _tokens[_current];
    }

    private Token _PeekNext()
    {
        return _tokens[_current+1];
    }

    private Token _Previous()
    {
        return _tokens[_current-1];
    }

    private bool _Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (_Check(type))
            {
                _Advance();
                return true;
            }
        }
        return false;
    }

    private bool _Check(TokenType type)
    {
        if (_IsAtEnd()) return false;
        return _Peek().Type == type;
    }
}