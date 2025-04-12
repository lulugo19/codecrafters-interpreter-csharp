using System.Diagnostics;
using System.Globalization;
using AST;

public class Parser
{
    private readonly string _source;
    private readonly List<Token> _tokens;
    private int _current;

    public Parser(string source)
    {
        _source = source;
        _current = 0;
        _tokens = new Scanner(source).ScanTokens();
    }

    public Expr Parse()
    {
        return _Primary();
    }

    public Expr _Primary()
    {
        if (_Match(TokenType.NIL)) return new Expr.Literal(Literal.Nil.Instance);
        if (_Match(TokenType.TRUE)) return new Expr.Literal(new Literal.Boolean(true));
        if (_Match(TokenType.FALSE)) return new Expr.Literal(new Literal.Boolean(false));
        if (_Match(TokenType.NUMBER)) return new Expr.Literal((Literal.Number)_Previous().Literal);
        if (_Match(TokenType.STRING)) return new Expr.Literal(new Literal.String(_Previous().Literal as string));

        throw new Exception("Unknown token");
    }

    private bool _IsAtEnd()
    {
        return _current >= _tokens.Count;
    }

    private Token _Advance()
    {
        return _tokens[_current++];
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