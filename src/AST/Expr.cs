
using System.Data.Common;
using System.Formats.Asn1;
using System.Net.Http.Headers;

namespace AST;
public abstract class Expr
{
    public abstract Expr Eval(Interpreter.Context ctx);

    public virtual string? ToOutput()
    {
        return ToString();
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
                var val = Expr.Eval(ctx);
                // false and nil is falsy and everything else is truthy
                var falsy = val is Literal lit && (lit.Value is AST.Literal.Boolean b && !b.Value || lit.Value is AST.Literal.Nil);
                return new Literal(new AST.Literal.Boolean(falsy));
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
    }
}