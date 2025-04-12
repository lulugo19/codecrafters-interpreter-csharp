
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

    public class Unary : Expr
    {
        public class Negation : Unary
        {
            public Expr Expr {get; init;}

            public Negation(Expr expr)
            {
                Expr = expr;
            }

            public override string ToString()
            {
                return $"(- {Expr})";
            }
        }

        public class Not : Unary
        {
            public Expr Expr {get; init;}

            public Not(Expr expr)
            {
                Expr = expr;
            }

            public override string ToString()
            {
                return $"(! {Expr})";
            }
        }
    }
}