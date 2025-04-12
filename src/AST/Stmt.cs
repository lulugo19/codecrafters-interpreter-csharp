namespace AST;

public abstract class Stmt
{
    public abstract void Run(Interpreter.Context ctx);

    public class Print : Stmt
    {
        public Expr Expr {get; init;}

        public Print(Expr expr)
        {
            Expr = expr;
        }

        public override void Run(Interpreter.Context ctx)
        {
            Console.WriteLine(Expr.Eval().ToOutput());
        }
    }

}