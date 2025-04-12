using System.Security.AccessControl;
using AST;

public class Interpreter
{
    public class Context()
    {
        public  Dictionary<string, Expr> Vars {get; init;} = new Dictionary<string, Expr>();
    }

    public void Run(List<Stmt> program)
    {
        Context ctx = new Context();
        foreach (var stmt in program)
        {
            stmt.Run(ctx);
        }
    }
}