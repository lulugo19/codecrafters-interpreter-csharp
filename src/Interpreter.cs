using AST;

public class Interpreter
{
    public class Variable
    {
        public Expr Value {get; set;}

        public Variable(Expr val)
        {
            Value = val;
        }
    }

    public class Scope
    {
        public Scope() {}

        public Scope(Dictionary<string, Variable> vars)
        {
            Vars = vars;
        }

        public  Dictionary<string, Variable> Vars {get; init;} = new Dictionary<string, Variable>();
        
        public Scope Copy()
        {
            return new Scope() { Vars = new(Vars) };
        }
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

        public Scope GlobalScope {get; init;} = new Scope();

        private Token? _varDeclIdentifier;

        public Context()
        {
            Scopes.Push(GlobalScope);
            CurrentScope.Vars.Add("clock", new Variable(Expr.Fun.Clock.Instance));
        }

        public Context(Dictionary<string, Variable> vars)
        {
            Scopes.Push(new Scope(vars));
        }

        public Context Copy()
        {
            Dictionary<string, Variable> vars = new Dictionary<string, Variable>();
            foreach (var scope in Scopes)
            {
                foreach (var key in scope.Vars.Keys)
                {
                    if (!vars.ContainsKey(key))
                    {
                        vars.Add(key, scope.Vars[key]);
                    }
                }
            }
            return new Context(vars);
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
            if (id == _varDeclIdentifier?.Lexeme)
            {
                throw new Exception($"Attempting to declare local variable '{id}' initialized with itself.");
            }
            foreach (var scope in Scopes)
            {
                scope.Vars.TryGetValue(id, out Variable? val);
                if (val != null)
                {
                    return val.Value;
                }
            }
            throw new Exception($"Undefined variable '{id}'");
        }

        public Expr AssignVar(Token identifier, Expr val)
        {
            string id = identifier.Lexeme;
            foreach (var scope in Scopes)
            {
                if (scope.Vars.ContainsKey(id))
                {
                    var evalued = val.Eval(this);
                    scope.Vars[id].Value = evalued;
                    return evalued;
                }
            }
            throw new Exception($"Undefined variable '{id}'"); 
        }

        public void DeclareVar(Token identifier, Expr val)
        {
            if (CurrentScope != GlobalScope)
            {
                _varDeclIdentifier = identifier;
            }
            CurrentScope.Vars[identifier.Lexeme] = new Variable(val.Eval(this));
            _varDeclIdentifier = null;
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

    public void Run(AST.Program program)
    {
        Context ctx = new Context();
        foreach (var stmt in program.Stmts)
        {
            stmt.Run(ctx);
        }
    }
}