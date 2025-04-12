namespace AST;

public abstract class Stmt
{
    public abstract void Run(Interpreter.Context ctx);

    public class Print : Stmt
    {
        public AST.Expr Expr {get; init;}

        public Print(AST.Expr expr)
        {
            Expr = expr;
        }

        public override void Run(Interpreter.Context ctx)
        {
            Console.WriteLine(Expr.Eval().ToOutput());
        }
    }

    public class Expr : Stmt
    {
        public AST.Expr E {get; init;}

        public Expr(AST.Expr expr)
        {
            E = expr;
        }

        public override void Run(Interpreter.Context ctx)
        {
            E.Eval();
        }
    }

}