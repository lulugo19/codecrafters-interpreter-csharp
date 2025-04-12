
namespace AST;
public abstract class Expr
{
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
        }

        public class Not : Unary
        {
            public Not(Expr expr) : base(expr) {}

            public override string ToString()
            {
                return $"(! {Expr})";
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

            public override string ToString()
            {
                return $"(== {Left} {Right})";
            }
        }

        public class UnEqual : Binary
        {
            public UnEqual(Expr left, Expr right) : base(left, right) {}

            public override string ToString()
            {
                return $"(!= {Left} {Right})";
            }
        }

        public class Less : Binary
        {
            public Less(Expr left, Expr right) : base(left, right) {}

            public override string ToString()
            {
                return $"(< {Left} {Right})";
            }
        }

        public class Greater : Binary
        {
            public Greater(Expr left, Expr right) : base(left, right) {}

            public override string ToString()
            {
                return $"(> {Left} {Right})";
            }
        }

        public class LessEq : Binary
        {
            public LessEq(Expr left, Expr right) : base(left, right) {}

            public override string ToString()
            {
                return $"(<= {Left} {Right})";
            }
        }

        public class GreaterEq : Binary
        {
            public GreaterEq(Expr left, Expr right) : base(left, right) {}

            public override string ToString()
            {
                return $"(>= {Left} {Right})";
            }
        }

        public class Add : Binary
        {
            public Add(Expr left, Expr right) : base(left, right) {}

            public override string ToString()
            {
                return $"(+ {Left} {Right})";
            }
        }

        public class Sub : Binary
        {
            public Sub(Expr left, Expr right) : base(left, right) {}

            public override string ToString()
            {
                return $"(- {Left} {Right})";
            }
        }

        public class Mul : Binary
        {
            public Mul(Expr left, Expr right) : base(left, right) {}

            public override string ToString()
            {
                return $"(* {Left} {Right})";
            }
        }

        public class Div : Binary
        {
            public Div(Expr left, Expr right) : base(left, right) {}

            public override string ToString()
            {
                return $"(/ {Left} {Right})";
            }
        }
    }
}