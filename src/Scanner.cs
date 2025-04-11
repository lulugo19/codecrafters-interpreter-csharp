using System.Transactions;

public class Scanner
{
    private readonly string _source;
    private readonly List<Token> _tokens = new List<Token>();

    private int _start = 0;
    private int _current = 0;
    private int _line = 1;

    public Scanner(String source)
    {
        _source = source;
    }

    public List<Token> ScanTokens()
    {
        _tokens.Clear();
        while(!_IsAtEnd())
        {
            _start = _current;
            _ScanToken();
        }
        _tokens.Add(new Token(TokenType.EOF, "", null, _line));
        return _tokens;
    }

    private bool _IsAtEnd()
    {
        return _current >= _source.Length;
    }

    private void _ScanToken()
    {
        char c = _Advance();
        switch (c)
        {
            case '(': _AddToken(TokenType.LEFT_PAREN); break;
            case ')': _AddToken(TokenType.RIGHT_PAREN); break;
            case '{': _AddToken(TokenType.LEFT_BRACE); break;
            case '}': _AddToken(TokenType.RIGHT_BRACE); break;
            case ',': _AddToken(TokenType.COMMA); break;
            case '.': _AddToken(TokenType.DOT); break;
            case '-': _AddToken(TokenType.MINUS); break;
            case '+': _AddToken(TokenType.PLUS); break;
            case ';': _AddToken(TokenType.SEMICOLON); break;
            case '*': _AddToken(TokenType.STAR); break;
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;

            case '\n':
                _line++;
                break;
        }
    }

    private void _AddToken(TokenType type)
    {
        _AddToken(type, null);
    }

    private void _AddToken(TokenType type, object literal)
    {
        string text = _source.Substring(_start, _current);
        _tokens.Add(new Token(type, text, literal, _line));
    }

    private char _Advance()
    {        
        return _source[_current++];
    }
}