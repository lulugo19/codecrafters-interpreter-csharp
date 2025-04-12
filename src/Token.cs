using AST;

public class Token
{
    public TokenType Type {get; init;}
    public string Lexeme {get; init;}
    public Literal? Literal {get; init;}
    public int Line {get; init;}

    public Token(TokenType type, string lexeme, Literal? literal, int line)
    {
        Type = type;
        Lexeme = lexeme;
        Literal = literal;
        Line = line;
    }

    public override String ToString()
    {
        return Type + " " + Lexeme + " " + (Literal == null ? "null" : Literal);
    }
}