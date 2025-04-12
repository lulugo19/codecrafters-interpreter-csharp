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
            Console.WriteLine(Expr.Eval(ctx).ToOutput());
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
            E.Eval(ctx);
        }
    }

    public class VarDecl : Stmt
    {
        public Token Id {get; init; }
        public AST.Expr? Value {get; init;}

        public VarDecl(Token id, AST.Expr? val)
        {
            Id = id;
            Value = val;
        }

        public override void Run(Interpreter.Context ctx)
        {
            ctx.DeclareVar(Id,  Value?.Eval(ctx) ?? new AST.Expr.Literal(Literal.Nil.Instance));
        }
    }

    public class VarAsgn : Stmt
    {
        public AST.Expr.VarAsgn Asgn {get; init;}

        public VarAsgn(AST.Expr.VarAsgn asgn)
        {
            Asgn = asgn;
        }

        public override void Run(Interpreter.Context ctx)
        {
            Asgn.Eval(ctx);
        }
    }

    public class Block : Stmt
    {
        public List<Stmt> Stmts {get; init;}

        public Block(List<Stmt> stmts)
        {
            Stmts = stmts;
        }

        public override void Run(Interpreter.Context ctx)
        {
            ctx.StartBlockScope();
            foreach (var stmt in Stmts)
            {
                stmt.Run(ctx);
            }
            ctx.EndBlockScope();
        }
    }

}