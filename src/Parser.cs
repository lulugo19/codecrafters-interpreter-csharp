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

    public List<Stmt> ParseProgram()
    {
        _current = 0;
        HasErrors = false;
        var stmts = new List<Stmt>();
        try 
        {          
            stmts = _Stmts();
        }     
        catch (ParserException e)
        {
            HasErrors = true;
            Console.Error.Write(e.Message);
        }
        return stmts;
    }

    public Expr? ParseExpr()
    {
        _current = 0;
        HasErrors = false;
        try
        {
            return _Expr();
        }
        catch (ParserException e)
        {   
            HasErrors = true;
            Console.Error.Write(e.Message);
            return null;
        }       
    }

    private List<Stmt> _Stmts()
    {
        var stmts = new List<Stmt>();
        while (!_IsAtEnd())
        {                
            if (_Peek().Type == TokenType.EOF || _Peek().Type == TokenType.RIGHT_BRACE)
            {
                break;
            }
            var stmt = _StmtDeclr();
            stmts.Add(stmt);   
        }
        return stmts;
    }

    private Stmt _StmtDeclr()
    {
        if (_Match(TokenType.VAR))
        {
            return _StmtVarDecl();
        }
        else if (_Match(TokenType.FUN))
        {
            return _StmtFunDecl();
        }
        return _Stmt();
    }

    private Stmt.VarDecl _StmtVarDecl()
    {
        var id = _Expect(TokenType.IDENTIFIER);
        Stmt.VarDecl? decl = null;
        if (_Match(TokenType.EQUAL))
        {
            decl = new Stmt.VarDecl(id, _Expr());
        }
        else
        {
            decl = new Stmt.VarDecl(id, null);
        }
        _Expect(TokenType.SEMICOLON);
        return decl;         
    }

    private Stmt.FunDecl _StmtFunDecl()
    {
        var id = _Expect(TokenType.IDENTIFIER);
        _Expect(TokenType.LEFT_PAREN);
        var param = new List<Token>();
        while (!_Match(TokenType.RIGHT_PAREN))
        {
            param.Add(_Expect(TokenType.IDENTIFIER));
            if (_Peek().Type != TokenType.COMMA)
            {
                _Expect(TokenType.RIGHT_PAREN);
                break;
            }
            _Expect(TokenType.COMMA);
        }
        _Expect(TokenType.LEFT_BRACE);
        var body = _StmtBlock();

        return new Stmt.FunDecl(new Expr.Fun(id, param, body));
    }

    private Stmt _Stmt()
    {
        if (_Match(TokenType.PRINT))
        {
            return _StmtPrint();
        }
        else if (_Match(TokenType.LEFT_BRACE))
        {
            return _StmtBlock();
        }
        else if (_Match(TokenType.IF))
        {
            return _StmtIf();
        }
        else if (_Match(TokenType.WHILE))
        {
            return _StmtWhile();
        }
        else if (_Match(TokenType.FOR))
        {
            return _StmtFor();
        }
        else if (_Match(TokenType.RETURN))
        {
            return _StmtReturn(); 
        }
        else
        {
            var stmt = _StmtExpr();
            _Expect(TokenType.SEMICOLON);
            return stmt;
        }
    }

    private Stmt.Print _StmtPrint()
    {     
        var printStmt = new Stmt.Print(_Expr());
        _Expect(TokenType.SEMICOLON);
        return printStmt;
    }

    private Stmt.Expr _StmtExpr()
    { 
        return new Stmt.Expr(_Expr());
    }

    private Stmt.Block _StmtBlock()
    {
        var stmts = _Stmts();
        _Expect(TokenType.RIGHT_BRACE);
        return new Stmt.Block(stmts);
    }

    private Stmt.If _StmtIf()
    {
        _Expect(TokenType.LEFT_PAREN);
        var cond = _Expr();
        _Expect(TokenType.RIGHT_PAREN);
        var whenTrue = _Stmt();
        var elseIfs = new List<Stmt.If.ElseIf>();
        while (_Peek().Type == TokenType.ELSE && _PeekNext().Type == TokenType.IF)
        {
            _Advance();
            _Advance();
            var condElseIf = _Expr();
            var stmtElseIf = _Stmt();
            var elseIf = new Stmt.If.ElseIf(condElseIf, stmtElseIf);
            elseIfs.Add(elseIf);
        }
        Stmt? elseStmt = null;
        if (_Match(TokenType.ELSE))
        {
            elseStmt = _Stmt();
        }
        return new Stmt.If(cond, whenTrue, elseIfs, elseStmt);
    }

    private Stmt.While _StmtWhile()
    {
        _Expect(TokenType.LEFT_PAREN);
        var cond = _Expr();
        _Expect(TokenType.RIGHT_PAREN);
        var loopStmt = _Stmt();
        return new Stmt.While(cond, loopStmt);
    }

    private Stmt.For _StmtFor()
    {
        _Expect(TokenType.LEFT_PAREN);
        Stmt? init = null;
        Expr? cond = null;
        Stmt.Expr? incr = null;
        if (!_Match(TokenType.SEMICOLON))
        {
            if (_Match(TokenType.VAR))
            {
                init = _StmtVarDecl();
            }
            else
            {
                init = _StmtExpr();
                _Expect(TokenType.SEMICOLON);
            }        
        }
        if (!_Match(TokenType.SEMICOLON))
        {
            cond = _Expr();
            _Expect(TokenType.SEMICOLON);
        }
        if (!_Match(TokenType.RIGHT_PAREN))
        {
            incr = _StmtExpr();
            _Expect(TokenType.RIGHT_PAREN);
        }
        Stmt loopStmt = _Stmt();

        return new Stmt.For(init, cond, incr, loopStmt);
    }

    private Stmt.Return _StmtReturn()
    {
        Expr? retVal = null;
        if (_Peek().Type != TokenType.SEMICOLON)
        {
            retVal = _Expr();
        }
        _Expect(TokenType.SEMICOLON);
        return new Stmt.Return(retVal);
    }

    private ParserException _StmtError()
    {
        HasErrors = true;
        var msg = "No valid statement";
        if (_current > 0) 
        {          
            var token = _Peek();
            msg = $"[line {token.Line}] Error at '{token.Lexeme}': Expect statement.";
        }     
        return new ParserException(msg);
    }

    private Expr _Expr()
    {
        return _ExprVarAsgn();
    }

    private Expr _ExprVarAsgn()
    {
        if (_Peek().Type == TokenType.IDENTIFIER && _PeekNext().Type == TokenType.EQUAL)
        {
            var id = _Advance();
            _Advance();
            var val = _Expr();
            return new Expr.VarAsgn(id, val);
        }
        else
        {
            return _ExprOr();
        }      
    }

    private Expr _ExprOr()
    {
        var expr = _ExprAnd();
        while (_Match(TokenType.OR))
        {
            expr = new Expr.Binary.Or(expr, _ExprAnd());
        }
        return expr;
    }

    private Expr _ExprAnd()
    {
        var expr = _ExprEqu();
        while (_Match(TokenType.AND))
        {
            expr = new Expr.Binary.And(expr, _ExprEqu());
        }
        return expr;
    }

    private Expr _ExprEqu()
    {
        var expr = _ExprComp();

        while (true)
        {
            if (_Match(TokenType.EQUAL_EQUAL))
            {
                expr = new Expr.Binary.Equal(expr, _ExprComp());
            }
            else if (_Match(TokenType.BANG_EQUAL))
            {
                expr = new Expr.Binary.UnEqual(expr, _ExprComp());
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private Expr _ExprComp()
    {
        var comp = _ExprTerm();
        while (true)
        {
            if (_Match(TokenType.GREATER))
            {
                comp = new Expr.Binary.Greater(comp, _ExprTerm());
            }
            else if (_Match(TokenType.GREATER_EQUAL))
            {
                comp = new Expr.Binary.GreaterEq(comp, _ExprTerm());
            }
            else if (_Match(TokenType.LESS))
            {
                comp = new Expr.Binary.Less(comp, _ExprTerm());
            }
            else if (_Match(TokenType.LESS_EQUAL))
            {
                comp = new Expr.Binary.LessEq(comp, _ExprTerm());
            }
            else
            {
                break;
            }
        }
        return comp;
    }

    private Expr _ExprTerm()
    {
        var term = _ExprFactor();
        while (true)
        {
            if (_Match(TokenType.PLUS))
            {
                term = new Expr.Binary.Add(term, _ExprFactor());
            }
            else if (_Match(TokenType.MINUS))
            {
                term = new Expr.Binary.Sub(term, _ExprFactor());
            }
            else
            {
                break;
            }
        }
        return term;
    }

    private Expr _ExprFactor()
    {
        var factor = _ExprUnary();
        while (true)
        {
            if (_Match(TokenType.STAR))
            {
                factor = new Expr.Binary.Mul(factor, _ExprUnary());
            }
            else if (_Match(TokenType.SLASH))
            {
                factor = new Expr.Binary.Div(factor, _ExprUnary());
            }
            else
            {
                break;
            }
        }
        return factor;
    }

    private Expr _ExprUnary()
    {
        if (_Match(TokenType.MINUS))
        {
            return new Expr.Unary.Negation(_ExprUnary());
        }
        if (_Match(TokenType.BANG))
        {
            return new Expr.Unary.Not(_ExprUnary());
        }
        else
        {
            return _ExprCall();
        }      
    }

    private Expr _ExprCall()
    {
        var expr = _ExprPrimary();
        while (_Match(TokenType.LEFT_PAREN))
        {
            List<Expr> args = new List<Expr>();
            while (!_Match(TokenType.RIGHT_PAREN))
            {
                args.Add(_Expr());
                if (_Peek().Type != TokenType.COMMA)
                {
                    _Expect(TokenType.RIGHT_PAREN);
                    break;
                }
                _Expect(TokenType.COMMA);
            }
            expr = new Expr.FuncCall(expr, args);
        }
        return expr;
    }

    private Expr _ExprPrimary()
    {
        if (_Match(TokenType.NIL))
        {
            return new Expr.Literal(Literal.Nil.Instance);
        } 
        if (_Match(TokenType.TRUE))
        {
            return new Expr.Literal(new Literal.Boolean(true));
        }
        if (_Match(TokenType.FALSE))
        {
            return new Expr.Literal(new Literal.Boolean(false));
        }
        if (_Match(TokenType.NUMBER))
        {
            return new Expr.Literal(new Literal.Number(((Number)_Previous().Literal).Value));
        } 
        if (_Match(TokenType.STRING))
        {
            return new Expr.Literal(new Literal.String(_Previous().Literal as string));
        }
        if (_Match(TokenType.IDENTIFIER))
        {
            // parse function call
            var id = _Previous();
            return new Expr.Var(id);         
        }
        if (_Match(TokenType.LEFT_PAREN)) 
        {
            var expr = _Expr();
            _Expect(TokenType.RIGHT_PAREN);
            return new Expr.Group(expr);
        }
        throw _ExprError();    
    }

    private ParserException _ExprError()
    {
        HasErrors = true;
        var msg = "No valid expression";
        if (_current > 0) 
        {          
            var token = _Peek();
            msg = $"[line {token.Line}] Error at '{token.Lexeme}': Expect expression.";
        }
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

        var prev = _Previous();
        throw new ParserException($"[line {prev.Line}] Expected token '{type}' but got '{_Peek().Type}'");
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