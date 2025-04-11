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
                _AddStringLiteral();
                break;
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;

            case '\n':
                _line++;
                break;
            case '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9':
                _AddNumberLiteral();
                break;
            default:
                if (char.IsDigit(c))
                {
                    _AddNumberLiteral();
                }
                if (_IsAlpha(c))
                {
                    _AddIdentifier();
                }
                else
                {
                    _EmitScannerError($"Unexpected character: {c}");
                }       
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

    private string _GetTokenText()
    {
        return _source.Substring(_start, _current - _start);
    }

    private void _AddToken(TokenType type, object literal)
    {
        string text = _GetTokenText();
        _tokens.Add(new Token(type, text, literal, _line));
    }

    private bool _IsAlpha(char c) => char.IsLetter(c) || c == '_';

    private bool _IsAlphaNumeric(char c) => _IsAlpha(c) || char.IsDigit(c);

    private void _AddStringLiteral()
    {
        while (!_IsAtEnd() && _Advance() != '\"');     
        string text = _GetTokenText();
        if (text[^1] != '\"')
        {
            _EmitScannerError($"Unterminated string.");
        }
        else 
        {
            _tokens.Add(new Token(TokenType.STRING, text, text[1..^1], _line));
        }
    }

    private void _AddNumberLiteral()
    {
        while (!_IsAtEnd() && char.IsDigit(_Advance()))
        {
            if (_Peek() == '.' && char.IsDigit(_PeekNext() ?? ' '))
            {
                _Advance();
            }
            else if (!char.IsDigit(_Peek() ?? ' '))
            {
                break;
            }
        }
        string text = _GetTokenText();
        Number number = new Number(Decimal.Parse(text));
        _tokens.Add(new Token(TokenType.NUMBER, text, number, _line));
    }

    private void _AddIdentifier()
    {
        while (_IsAlphaNumeric(_Peek() ?? ' '))
        {
            _Advance();
        }
        _AddToken(TokenType.IDENTIFIER);
    }

    private char _Advance()
    {        
        return  _source[_current++];
    }

    private char? _Peek()
    {
        return _IsAtEnd() ? null : _source[_current];
    }

    private char? _PeekNext()
    {
        return _current + 1 >= _source.Length  ? null : _source[_current + 1];
    }
}