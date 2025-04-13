namespace AST;

public abstract class Stmt
{
    public abstract void Run(Interpreter.Context ctx);

    public bool EndsWithSemicolon {get; init;} = false;

    public class Print : Stmt
    {
        public AST.Expr Expr {get; init;}

        public Print(AST.Expr expr)
        {
            EndsWithSemicolon = true;
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
            EndsWithSemicolon = true;
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
            EndsWithSemicolon = true;
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
            EndsWithSemicolon = true;
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

    public class If : Stmt
    {
        public class ElseIf
        {
            public AST.Expr Cond {get; init;}
            public Stmt WhenTrue {get; init;}

            public ElseIf(AST.Expr cond, Stmt whenTrue)
            {
                Cond = cond;
                WhenTrue = whenTrue;
            }
        }

        public AST.Expr Cond {get; init;}
        public Stmt WhenTrue {get; init;}
        public List<ElseIf> ElseIfs {get; init;}
        public Stmt? Else {get; init;}

        public If(AST.Expr cond, Stmt whenTrue, List<ElseIf> elseIfs, Stmt? elseStmt)
        {
            Cond = cond;
            WhenTrue = whenTrue;
            ElseIfs = elseIfs;
            Else = elseStmt;
        }

        public override void Run(Interpreter.Context ctx)
        {
            if (Cond.Eval(ctx).ToBoolean())
            {
                WhenTrue.Run(ctx);
            }
            else
            {
                var trueElseIf = ElseIfs.FirstOrDefault(ei => ei.Cond.Eval(ctx).ToBoolean());
                if (trueElseIf != null)
                {
                    trueElseIf.WhenTrue.Run(ctx);
                }
                else if (Else != null)
                {
                    Else.Run(ctx);
                }
            }
        }       
    }

    public class While : Stmt
    {
        public AST.Expr Cond {get; init;}
        public Stmt LoopStmt {get; init;}

        public While(AST.Expr cond, Stmt loopStmt)
        {
            Cond = cond;
            LoopStmt = loopStmt;
        }

        public override void Run(Interpreter.Context ctx)
        {
            while (Cond.Eval(ctx).ToBoolean())
            {
                LoopStmt.Run(ctx);
            }
        }
    }
}