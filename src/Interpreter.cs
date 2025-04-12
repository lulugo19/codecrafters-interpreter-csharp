using System.Security.AccessControl;
using AST;

public class Interpreter
{
    public class Context()
    {

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