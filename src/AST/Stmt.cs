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
            ctx.DeclareVar(Id,  Value ?? new AST.Expr.Literal(Literal.Nil.Instance));
            return null;
        }
    }

    public class FunDecl : Stmt
    {
        public Token Id {get; init;}
        public List<Token> Params { get; init; }
        public Block Body {get; init;}

        public FunDecl(Token id, List<Token> param, Block body)
        {
            Id = id;
            Params = param;
            Body = body;
        }

        public override AST.Expr? Run(Interpreter.Context ctx)
        {
            var fun = new AST.Expr.Fun(Id, Params, Body);
            ctx.DeclareVar(Id, fun);
            fun.BoundedContext = ctx.Copy();
            return null;
        }
    }

    public class ClassDecl : Stmt
    {
        public Token Id {get; init;}

        public ClassDecl(Token id)
        {
            Id = id;
        }

        public override AST.Expr? Run(Interpreter.Context ctx)
        {
            ctx.DeclareVar(Id, new AST.Expr.Class(Id));
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
                if ((ctx.Flags & Interpreter.Flags.RETURN) == Interpreter.Flags.RETURN)
                {
                    break;
                }
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
            if (Cond.IsTruthy(ctx))
            {
                WhenTrue.Run(ctx);
            }
            else
            {
                var trueElseIf = ElseIfs.FirstOrDefault(ei => ei.Cond.IsTruthy(ctx));
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
            while (Cond.IsTruthy(ctx))
            {
                LoopStmt.Run(ctx);
                if ((ctx.Flags & Interpreter.Flags.RETURN) == Interpreter.Flags.RETURN)
                {
                    break;
                }
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
            ctx.StartBlockScope();
            Init?.Run(ctx);
            while (Cond?.IsTruthy(ctx) ?? true)
            {
                LoopStmt.Run(ctx);
                if ((ctx.Flags & Interpreter.Flags.RETURN) == Interpreter.Flags.RETURN)
                {
                    break;
                }
                Incr?.Run(ctx);
            }
            ctx.EndBlockScope();
            return null;
        }
    }

    public class Return : Stmt
    {
        public AST.Expr? RetVal {get; init;}

        public Return(AST.Expr retVal)
        {
            RetVal = retVal;
        }

        public override AST.Expr? Run(Interpreter.Context ctx)
        {
            ctx.RetVal = RetVal?.Eval(ctx);
            ctx.Flags |= Interpreter.Flags.RETURN;          
            return null;
        }
    }
}