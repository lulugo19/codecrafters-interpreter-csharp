namespace AST;

public class Program
{
    public List<Stmt> Stmts {get; init;}

    public Program(List<Stmt> stmts)
    {
        Stmts = stmts;
    }
}