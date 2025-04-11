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
            case '!':
                if (_Peek() == '=')
                {
                    _Advance();
                    _AddToken(TokenType.BANG_EQUAL);
                }
                else
                {
                    _AddToken(TokenType.BANG);
                }
                break;
            case '>':
                if (_Peek() == '=')
                {
                    _Advance();
                    _AddToken(TokenType.GREATER_EQUAL);
                }
                else
                {
                    _AddToken(TokenType.GREATER);
                }
                break;
            case '<':
                if (_Peek() == '=')
                {
                    _Advance();
                    _AddToken(TokenType.LESS_EQUAL);
                }
                else
                {
                    _AddToken(TokenType.LESS);
                }
                break;
            case '/':
                if (_Peek() == '/')
                {   
                    // Comments with a forward slash
                    while (!_IsAtEnd() && _Advance() != '\n');
                    _line++;
                }
                else
                {
                    _AddToken(TokenType.SLASH);
                }
                break;
            case '\"':
                _AddString();
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
                _EmitScannerError($"Unexpected character: {c}");          
                break;
        }
    }

    private void _EmitScannerError(string msg)
    {
        HasErrors = true;
        Console.Error.WriteLine($"[line {_line}] Error: {msg}");    
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

    private void _AddString()
    {
        while (!_IsAtEnd() && _Advance() != '\"');     
        string text = _source.Substring(_start, _current - _start);
        if (text[^1] != '\"')
        {
            _EmitScannerError($"Unterminated string.");
        }
        else 
        {
            _tokens.Add(new Token(TokenType.STRING, text, text[1..^1], _line));
        }
    }

    private char _Advance()
    {        
        return _source[_current++];
    }

    private char? _Peek()
    {
        return _IsAtEnd() ? null : _source[_current];
    }
}