using AST;

public class Interpreter
{
    public class Scope
    {
        public  Dictionary<string, Expr> Vars {get; init;} = new Dictionary<string, Expr>();
    }

    [Flags]
    public enum Flags 
    {
        RETURN = 1,
    }

    public class Context
    {
        public Stack<Scope> Scopes {get; init;} = new Stack<Scope>();

        public Flags Flags {get; set;}

        public Expr? RetVal {get; set;}

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

        public Expr? CallFunction(Expr.FuncCall call)
        {
            string id = call.Id.Lexeme;
            foreach (var scope in Scopes)
            {
                scope.Vars.TryGetValue(id, out Expr? var);
                if (var != null)
                {
                    if (var is Expr.Fun fun)
                    {
                        return fun.Run(this, call);
                    }
                    else
                    {
                        throw new Exception($"'{id}' is not a function");
                    }
                }
            }
            throw new Exception($"Undefined function '{id}'");
        }

        public Expr AssignVar(Token identifier, Expr val)
        {
            string id = identifier.Lexeme;
            foreach (var scope in Scopes)
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

        public void OutputCurrentScope()
        {
            Console.WriteLine($"Current scope {Scopes.Count}: ");
            foreach (var key in CurrentScope.Vars.Keys)
            {
                Console.WriteLine($"{key}={CurrentScope.Vars[key]}");
            }
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