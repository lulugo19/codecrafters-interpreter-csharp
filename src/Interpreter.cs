using AST;

public class Interpreter
{
    public class Scope
    {
        public  Dictionary<string, Expr> Vars {get; init;} = new Dictionary<string, Expr>();
        public  Dictionary<string, Stmt.FunDecl> Funs {get; init;} = new Dictionary<string, Stmt.FunDecl>();
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

        public Expr? CallFunction(Token identifier)
        {
            string id = identifier.Lexeme;
            foreach (var scope in Scopes)
            {
                scope.Funs.TryGetValue(id, out Stmt.FunDecl? fun);
                if (fun != null)
                {
                    return fun.Fun.Body.Run(this);
                }
            }
            throw new Exception($"Undefined function '{id}'");
        }

        public void DeclareFun(Token identifier, Stmt.FunDecl fun)
        {
            CurrentScope.Funs[identifier.Lexeme] = fun;
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