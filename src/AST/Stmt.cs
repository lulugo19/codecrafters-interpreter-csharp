namespace AST;

public abstract class Stmt
{
    public abstract AST.Expr? Run(Interpreter.Context ctx);

    public class Print : Stmt
    {
        public AST.Expr Expr {get; init;}

        public Print(AST.Expr expr)
        {
            Expr = expr;
        }

        public override AST.Expr? Run(Interpreter.Context ctx)
        {
            Console.WriteLine(Expr.Eval(ctx).ToOutput());
            return null;
        }
    }

    public class Expr : Stmt
    {
        public AST.Expr E {get; init;}

        public Expr(AST.Expr expr)
        {
            E = expr;
        }

        public override AST.Expr? Run(Interpreter.Context ctx)
        {
            E.Eval(ctx);
            return null;
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

        public override AST.Expr? Run(Interpreter.Context ctx)
        {
            ctx.DeclareVar(Id,  Value?.Eval(ctx) ?? new AST.Expr.Literal(Literal.Nil.Instance));
            return null;
        }
    }

    public class FunDecl : Stmt
    {
        public AST.Expr.Fun Fun {get; init;}

        public FunDecl(AST.Expr.Fun fun)
        {
            Fun = fun;
        }

        public override AST.Expr? Run(Interpreter.Context ctx)
        {
            ctx.DeclareVar(Fun.Id, Fun);
            return null;
        }
    }

    public class Block : Stmt
    {
        public List<Stmt> Stmts {get; init;}

        public Block(List<Stmt> stmts)
        {
            Stmts = stmts;
        }

        public override AST.Expr? Run(Interpreter.Context ctx)
        {
            ctx.StartBlockScope();
            foreach (var stmt in Stmts)
            {
                stmt.Run(ctx);
            }
            ctx.EndBlockScope();
            return null;
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

        public override AST.Expr? Run(Interpreter.Context ctx)
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
            return null;
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

        public override AST.Expr? Run(Interpreter.Context ctx)
        {
            while (Cond.Eval(ctx).ToBoolean())
            {
                LoopStmt.Run(ctx);
            }
            return null;
        }
    }

    public class For : Stmt
    {
        public Stmt? Init {get; init;}
        public AST.Expr? Cond {get; init;}
        public Expr? Incr {get; init;}
        public Stmt LoopStmt {get; init;}

        public For(Stmt? init, AST.Expr? cond, Expr? incr, Stmt loopStmt)
        {
            Init = init;
            Cond = cond;
            Incr = incr;
            LoopStmt = loopStmt;
        }

        public override AST.Expr? Run(Interpreter.Context ctx)
        {
            Init?.Run(ctx);
            while (Cond?.Eval(ctx)?.ToBoolean() ?? true)
            {
                LoopStmt.Run(ctx);
                Incr?.Run(ctx);
            }
            return null;
        }
    }
}