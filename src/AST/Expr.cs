
using System.ComponentModel;
using System.Data.Common;
using System.Formats.Asn1;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace AST;
public abstract class Expr
{
    public abstract Expr Eval(Interpreter.Context ctx);

    public virtual string? ToOutput()
    {
        return ToString();
    }

    public bool IsTruthy(Interpreter.Context ctx)
    {
        var val = Eval(ctx);
        var isFalsy = val is Literal lit && (lit.Value is AST.Literal.Boolean b && !b.Value || lit.Value is AST.Literal.Nil);
        return !isFalsy;
    }

    public class Literal : Expr
    {
        public AST.Literal Value {get; init;}

        public Literal(AST.Literal literal)
        {
            Value = literal;
        }

        public override string? ToString()
        {
            return Value.ToString();
        }

        public override string? ToOutput()
        {
            return Value.ToOutput();
        }

        public override Expr Eval(Interpreter.Context ctx)
        {
            return this;
        }
    }

    public class Var : Expr
    {
        public Token Id {get; init;}

        public Var(Token id)
        {
            Id = id;
        }

        public override Expr Eval(Interpreter.Context ctx)
        {         
            return ctx.GetVarVal(Id);
        }
    }

    public class VarAsgn : Expr
    {
        public Token Id {get; init;}
        public Expr Val {get; init;}

        public VarAsgn(Token id, Expr val)
        {
            Id = id;
            Val = val;
        }

        public override Expr Eval(Interpreter.Context ctx)
        {
            return ctx.AssignVar(Id, Val);
        }

        public override string ToString()
        {
            return $"(= {Id.Lexeme} {Val})";
        }
    }

    public class Group : Expr
    {
        public Expr InnerExpr {get; init;}

        public Group(Expr innerExpr)
        {
            InnerExpr = innerExpr;
        }

        public override string ToString()
        {
            return $"(group {InnerExpr})";
        }

        public override Expr Eval(Interpreter.Context ctx)
        {
            return InnerExpr.Eval(ctx);
        }
    }

    public abstract class Unary : Expr
    {
        public Expr Expr {get; init;}

        public Unary(Expr expr)
        {
            Expr = expr;
        }

        public class Negation : Unary
        {
            public Negation(Expr expr) : base(expr) {}

            public override string ToString()
            {
                return $"(- {Expr})";
            }

            public override Expr Eval(Interpreter.Context ctx)
            {
                if (Expr.Eval(ctx) is Literal lit && lit.Value is AST.Literal.Number num)
                {
                    return new Expr.Literal(new AST.Literal.Number(-num.Value));
                }
                throw new Exception("Not a number");           
            }
        }

        public class Not : Unary
        {
            public Not(Expr expr) : base(expr) {}

            public override string ToString()
            {
                return $"(! {Expr})";
            }

            public override Expr Eval(Interpreter.Context ctx)
            {
                return new Literal(new AST.Literal.Boolean(!Expr.Eval(ctx).IsTruthy(ctx)));
            }
        }
    }

    public abstract class Binary : Expr
    {
        public Expr Left {get; init;}
        public Expr Right {get; init;}

        public Binary(Expr left, Expr right)
        {
            Left = left;
            Right = right;
        }

        public class Equal : Binary
        {
            public Equal(Expr left, Expr right) : base(left, right) {}

            public override Expr Eval(Interpreter.Context ctx)
            {
                var leftVal = Left.Eval(ctx);
                var rightVal = Right.Eval(ctx);

                if (leftVal is Literal lit1 && rightVal is Literal lit2 && lit1.Value.GetType() == lit2.Value.GetType())
                {
                    return new Literal(new AST.Literal.Boolean(lit1.Value.GetValue().ToString() == lit2.Value.GetValue().ToString()));
                }

                return new Literal(new AST.Literal.Boolean(false));
            }

            public override string ToString()
            {
                return $"(== {Left} {Right})";
            }
        }

        public class UnEqual : Binary
        {
            public UnEqual(Expr left, Expr right) : base(left, right) {}

            public override Expr Eval(Interpreter.Context ctx)
            {
                var leftVal = Left.Eval(ctx);
                var rightVal = Right.Eval(ctx);

                if (leftVal is Literal lit1 && rightVal is Literal lit2 && lit1.Value.GetType() == lit2.Value.GetType())
                {
                    return new Literal(new AST.Literal.Boolean(lit1.Value.GetValue().ToString() != lit2.Value.GetValue().ToString()));
                }

                return new Literal(new AST.Literal.Boolean(false));
            }

            public override string ToString()
            {
                return $"(!= {Left} {Right})";
            }
        }

        public class Less : Binary
        {
            public Less(Expr left, Expr right) : base(left, right) {}

            public override Expr Eval(Interpreter.Context ctx)
            {
                var leftVal = Left.Eval(ctx);
                var rightVal = Right.Eval(ctx);

                if (leftVal is Literal lit1 && rightVal is Literal lit2 && lit1.Value.GetType() == lit2.Value.GetType())
                {
                    if (lit1.Value is AST.Literal.Number num1 && lit2.Value is AST.Literal.Number num2)
                    {
                        return new Literal(new AST.Literal.Boolean(num1.Value < num2.Value)); 
                    }
                }

                throw new Exception("Can't be compared");
            }

            public override string ToString()
            {
                return $"(< {Left} {Right})";
            }
        }

        public class Greater : Binary
        {
            public Greater(Expr left, Expr right) : base(left, right) {}

            public override Expr Eval(Interpreter.Context ctx)
            {
                var leftVal = Left.Eval(ctx);
                var rightVal = Right.Eval(ctx);

                if (leftVal is Literal lit1 && rightVal is Literal lit2 && lit1.Value.GetType() == lit2.Value.GetType())
                {
                    if (lit1.Value is AST.Literal.Number num1 && lit2.Value is AST.Literal.Number num2)
                    {
                        return new Literal(new AST.Literal.Boolean(num1.Value > num2.Value)); 
                    }
                }

                throw new Exception("Can't be compared");
            }

            public override string ToString()
            {
                return $"(> {Left} {Right})";
            }
        }

        public class LessEq : Binary
        {
            public LessEq(Expr left, Expr right) : base(left, right) {}

            public override Expr Eval(Interpreter.Context ctx)
            {
                var leftVal = Left.Eval(ctx);
                var rightVal = Right.Eval(ctx);

                if (leftVal is Literal lit1 && rightVal is Literal lit2 && lit1.Value.GetType() == lit2.Value.GetType())
                {
                    if (lit1.Value is AST.Literal.Number num1 && lit2.Value is AST.Literal.Number num2)
                    {
                        return new Literal(new AST.Literal.Boolean(num1.Value <= num2.Value)); 
                    }
                }

                throw new Exception("Can't be compared");
            }

            public override string ToString()
            {
                return $"(<= {Left} {Right})";
            }
        }

        public class GreaterEq : Binary
        {
            public GreaterEq(Expr left, Expr right) : base(left, right) {}

            public override Expr Eval(Interpreter.Context ctx)
            {
                var leftVal = Left.Eval(ctx);
                var rightVal = Right.Eval(ctx);

                if (leftVal is Literal lit1 && rightVal is Literal lit2 && lit1.Value.GetType() == lit2.Value.GetType())
                {
                    if (lit1.Value is AST.Literal.Number num1 && lit2.Value is AST.Literal.Number num2)
                    {
                        return new Literal(new AST.Literal.Boolean(num1.Value >= num2.Value)); 
                    }
                }

                throw new Exception("Can't be compared");
            }

            public override string ToString()
            {
                return $"(>= {Left} {Right})";
            }
        }

        public class Add : Binary
        {
            public Add(Expr left, Expr right) : base(left, right) {}

            public override Expr Eval(Interpreter.Context ctx)
            {
                var leftVal = Left.Eval(ctx);
                var rightVal = Right.Eval(ctx);

                if (leftVal is Literal lit1 && rightVal is Literal lit2 && lit1.Value.GetType() == lit2.Value.GetType())
                {
                    if (lit1.Value is AST.Literal.Number num1 && lit2.Value is AST.Literal.Number num2)
                    {
                        return new Literal(new AST.Literal.Number(num1.Value + num2.Value)); 
                    }
                    else if (lit1.Value is AST.Literal.String str1 && lit2.Value is AST.Literal.String str2)
                    {
                        return new Literal(new AST.Literal.String(str1.Value + str2.Value)); 
                    }
                }

                throw new Exception("Can't be added");
            }

            public override string ToString()
            {
                return $"(+ {Left} {Right})";
            }
        }

        public class Sub : Binary
        {
            public Sub(Expr left, Expr right) : base(left, right) {}

            public override Expr Eval(Interpreter.Context ctx)
            {
                var leftVal = Left.Eval(ctx);
                var rightVal = Right.Eval(ctx);

                if (leftVal is Literal lit1 && rightVal is Literal lit2 && lit1.Value.GetType() == lit2.Value.GetType())
                {
                    if (lit1.Value is AST.Literal.Number num1 && lit2.Value is AST.Literal.Number num2)
                    {
                        return new Literal(new AST.Literal.Number(num1.Value - num2.Value)); 
                    }
                }

                throw new Exception("Can't be subracted");
            }

            public override string ToString()
            {
                return $"(- {Left} {Right})";
            }
        }

        public class Mul : Binary
        {
            public Mul(Expr left, Expr right) : base(left, right) {}

            public override Expr Eval(Interpreter.Context ctx)
            {
                var leftVal = Left.Eval(ctx);
                var rightVal = Right.Eval(ctx);

                if (leftVal is Literal lit1 && rightVal is Literal lit2 && lit1.Value.GetType() == lit2.Value.GetType())
                {
                    if (lit1.Value is AST.Literal.Number num1 && lit2.Value is AST.Literal.Number num2)
                    {
                        return new Literal(new AST.Literal.Number(num1.Value * num2.Value)); 
                    }
                }

                throw new Exception("Can't be multiplied");
            }

            public override string ToString()
            {
                return $"(* {Left} {Right})";
            }
        }

        public class Div : Binary
        {
            public Div(Expr left, Expr right) : base(left, right) {}

            public override Expr Eval(Interpreter.Context ctx)
            {
                var leftVal = Left.Eval(ctx);
                var rightVal = Right.Eval(ctx);

                if (leftVal is Literal lit1 && rightVal is Literal lit2 && lit1.Value.GetType() == lit2.Value.GetType())
                {
                    if (lit1.Value is AST.Literal.Number num1 && lit2.Value is AST.Literal.Number num2)
                    {
                        return new Literal(new AST.Literal.Number(num1.Value / num2.Value)); 
                    }
                }

                throw new Exception("Can't be divided");
            }

            public override string ToString()
            {
                return $"(/ {Left} {Right})";
            }
        }

        public class Or : Binary
        {
            public Or(Expr left, Expr right) : base(left, right) {}

            public override Expr Eval(Interpreter.Context ctx)
            {
                var leftVal = Left.Eval(ctx);
                if (leftVal.IsTruthy(ctx))
                {
                    return leftVal;
                }
                var rightVal = Right.Eval(ctx);
                if (rightVal.IsTruthy(ctx))
                {
                    return rightVal;
                }
                return new Literal(new AST.Literal.Boolean(false));
            }

            public override string ToString()
            {
                return $"(or {Left} {Right})";
            }
        }

        public class And : Binary
        {
            public And(Expr left, Expr right) : base(left, right) {}

            public override Expr Eval(Interpreter.Context ctx)
            {
                var leftVal = Left.Eval(ctx);
                if (!leftVal.IsTruthy(ctx))
                {
                    return new Literal(new AST.Literal.Boolean(false));
                }
                var rightVal = Right.Eval(ctx);
                if (!rightVal.IsTruthy(ctx))
                {
                    return new Literal(new AST.Literal.Boolean(false));
                }
                return rightVal;               
            }

            public override string ToString()
            {
                return $"(and {Left} {Right})";
            }
        }
    }

    public class FuncCall : Expr
    {
        public Expr Fun {get; init;}
        public List<Expr> Args {get; init;}

        public FuncCall(Expr fun, List<Expr> args)
        {
            Fun = fun;
            Args = args;
        }

        public override Expr Eval(Interpreter.Context ctx)
        {
            var funVal = Fun.Eval(ctx);
            if (funVal is Expr.Fun fun)
            {
                return fun.Run(ctx, this) ?? new Literal(AST.Literal.Nil.Instance);
            }
            else
            {
                throw new Exception($"{funVal} is not a function");
            }
        }
    }

    public class Fun : Expr
    {
        public Token Id {get; init;}
        public List<Token> Params {get; init;}
        public Stmt.Block Body {get; init;}

        public Interpreter.Context BoundedContext {get; set;} = new Interpreter.Context();

        public Fun(Token id, List<Token> param, Stmt.Block body)
        {
            Id = id;
            Params = param;
            Body = body;
        }

        public override Expr Eval(Interpreter.Context ctx)
        {
            return this;
        }

        public virtual Expr? Run(Interpreter.Context ctx, FuncCall call)
        {
            if (call.Args.Count != Params.Count)
            {
                throw new Exception($"Expected {Params.Count} arguments but got ${call.Args.Count}.");
            }
            var evalArgs = call.Args.Select(arg => arg.Eval(ctx)).ToArray();
            BoundedContext.StartBlockScope();
            for (int i = 0; i < Params.Count; i++)
            {
                BoundedContext.DeclareVar(Params[i], evalArgs[i]);
            }
            foreach (var stmt in Body.Stmts)
            {
                stmt.Run(BoundedContext);
                if ((BoundedContext.Flags & Interpreter.Flags.RETURN) == Interpreter.Flags.RETURN)
                {
                    BoundedContext.Flags &= ~Interpreter.Flags.RETURN;
                    break;
                }
            }
            BoundedContext.EndBlockScope();
            return BoundedContext.RetVal;
        }

        public override string? ToOutput()
        {
            return $"<fn {Id.Lexeme}>";
        }

        public class Clock : Fun
        {
            public static Clock Instance {get; } = new Clock();

            public Clock() : base(
                    new Token(TokenType.IDENTIFIER, "clock", null, 0), 
                    new List<Token>(),
                    null
                ) {}

            public override Expr? Run(Interpreter.Context ctx, FuncCall call)
            {
                var elapsed = Convert.ToDecimal(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() / 1000);
                return new Literal(new AST.Literal.Number(elapsed));
            }
        }
    }

    public class Class : Expr
    {
        public Token Id {get; init;}

        public Class(Token id)
        {
            Id = id;
        }

        public override Expr Eval(Interpreter.Context ctx)
        {
            return this;
        }

        public override string ToOutput()
        {
            return Id.Lexeme;
        }
    }

    public class ClassInst : Expr
    {
        public class Prop : Expr
        {
            public Token Id { get; set; }
            public Expr Value { get; set; }

            public Prop(Token id, Expr val)
            {
                Id = id;
                Value = val;
            }

            public override Expr Eval(Interpreter.Context ctx)
            {
                return Value.Eval(ctx);
            }

            public override string? ToOutput()
            {
                return Value.ToString();
            }
        }

        public Token ClassId { get; init; }

        public Class Class {get; private set;}

        public Dictionary<string, Prop> Props {get; } = new Dictionary<string, Prop>();

        public ClassInst(Token classId)
        {
            ClassId = classId;
        }

        public Prop Get(Token propId, bool createNewProp = false)
        {
            string id = propId.Lexeme;
            Props.TryGetValue(id, out Prop? prop);
            if (prop == null)
            {
                if (createNewProp)
                {
                   prop = new Prop(propId, new Literal(AST.Literal.Nil.Instance)); 
                   Props[id] = prop;
                   return prop;
                }
                else
                {
                    throw new Exception($"[line {propId.Line}] Can't access undefined property '{propId.Lexeme}'.");
                }
            }
            else
            {
                return prop;
            }
        }

        public Prop Set(Token propId, Expr val, Interpreter.Context ctx)
        {
            string id = propId.Lexeme;
            var eval = val.Eval(ctx);
            if (!Props.TryAdd(id, new Prop(propId, eval)))
            {
                Props[id].Value = eval;
            }
            return Props[id];
        }

        public override Expr Eval(Interpreter.Context ctx)
        {
            try
            {
                Class = (Class)ctx.GetVarVal(ClassId);
            }
            catch (Exception)
            {
                throw new Exception($"[line {ClassId.Line}] The class '{ClassId.Lexeme}' is not defined");
            }
            return this;
        }

        public override string? ToOutput()
        {
            return $"{Class.Id.Lexeme} instance";
        }
    }

    public class Getter : Expr
    {
        public Expr ClassInstExpr { get; init; }
        public Token PropId { get; init; }

        public Getter(Expr classInstExpr, Token propId)
        {
            ClassInstExpr = classInstExpr;
            PropId = propId;
        }

        public override Expr Eval(Interpreter.Context ctx)
        {
            return Access(ctx, false).Eval(ctx);
        }
        

        public ClassInst.Prop Access(Interpreter.Context ctx, bool createNewProp)
        {
            try
            {
                return ((ClassInst)ClassInstExpr.Eval(ctx)).Get(PropId, createNewProp);
            }
            catch
            {
                throw new Exception($"[line {PropId.Line}] Expression is not a class instance.");
            }
        }       
    }

    public class Setter : Expr
    {
        public Getter PropAccessor { get; init; }
        public Expr Value { get; init; }

        public Setter(Getter accessor, Expr val)
        {
            PropAccessor = accessor;
            Value = val;
        }

        public override Expr Eval(Interpreter.Context ctx)
        {
            var eval = Value.Eval(ctx);
            var prop = PropAccessor.Access(ctx, true);
            prop.Value = eval;
            return prop.Eval(ctx);
        }
    }
}