using System.Diagnostics;
using System.Transactions;

public class Scanner
{
    public bool HasErrors {get; private set;}

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
        HasErrors = false;
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
            case '=':
                if (_Peek() == '=')
                {
                    _Advance();
                    _AddToken(TokenType.EQUAL_EQUAL);
                }
                else
                {
                    _AddToken(TokenType.EQUAL);
                }
                break;
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;

            case '\n':
                _line++;
                break;
            default:
                HasErrors = true;
                Console.Error.WriteLine($"[line {_line}] Error: Unexpected character: {c}");            
                break;
        }
    }

    private void _AddToken(TokenType type)
    {
        _AddToken(type, null);
    }

    private void _AddToken(TokenType type, object literal)
    {
        string text = _source.Substring(_start, _current - _start);
        _tokens.Add(new Token(type, text, literal, _line));
    }

    private char _Advance()
    {        
        return _source[_current++];
    }

    private char _Peek()
    {
        return _source[_current];
    }
}