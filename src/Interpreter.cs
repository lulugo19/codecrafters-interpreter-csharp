using AST;

public class Interpreter
{
    public class Scope
    {
        public  Dictionary<string, Expr> Vars {get; init;} = new Dictionary<string, Expr>();
    }

    public class Context
    {
        public Stack<Scope> Scopes {get; init;} = new Stack<Scope>();

        public Scope CurrentScope => Scopes.Peek();

        public Context()
        {
            Scopes.Push(new Scope());
        }

        public void StartBlockScope()
        {
            Scopes.Push(new Scope());
        }

        public void EndBlockScope()
        {
            Scopes.Pop();
        }

        public Expr GetVarVal(Token identifier)
        {
            string id = identifier.Lexeme;
            foreach (var scope in Scopes)
            {
                scope.Vars.TryGetValue(id, out Expr? val);
                if (val != null)
                {
                    return val;
                }
            }
            throw new Exception($"Undefined variable '{id}'");
        }

        public Expr AssignVar(Token identifier, Expr val)
        {
            string id = identifier.Lexeme;
            foreach (var scope in Scopes.Reverse())
            {
                if (scope.Vars.ContainsKey(id))
                {
                    var evalued = val.Eval(this);
                    scope.Vars[id] = evalued;
                    return evalued;
                }
            }
            throw new Exception($"Undefined variable '{id}'"); 
        }

        public void DeclareVar(Token identifier, Expr val)
        {
            CurrentScope.Vars[identifier.Lexeme] = val.Eval(this);
        }
    }

    public void Run(List<Stmt> program)
    {
        Context ctx = new Context();
        foreach (var stmt in program)
        {
            stmt.Run(ctx);
        }
    }
}