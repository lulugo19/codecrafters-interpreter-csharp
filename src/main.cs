using System;
using System.IO;


if (args.Length < 2)
{
    Console.Error.WriteLine("Usage: ./your_program.sh tokenize <filename>");
    Environment.Exit(1);
}

string command = args[0];
string filename = args[1];

string fileContents = File.ReadAllText(filename);

switch (command)
{
    case "tokenize":
        return Tokenize();
    case "parse":
        return Parse();
    case "evaluate":
        return Evaluate();
    case "run":
        return Run();
    default:
        Console.Error.WriteLine($"Unknown command: {command}");
        return 1;
}
// Uncomment this block to pass the first stage
int Tokenize()
{
    var scanner = new Scanner(fileContents);
    foreach (var token in scanner.ScanTokens())
    {
        Console.WriteLine(token);
    }
    if (scanner.HasErrors)
    {
        return 65;
    }
    return 0;
}

int Parse()
{
    var parser = new Parser(fileContents);
    var expr = parser.ParseExpr();
    if (parser.HasErrors)
    {
        return 65;
    }
    Console.WriteLine(expr); 
    return 0;
}

int Evaluate()
{
    var parser = new Parser(fileContents);
    var expr = parser.ParseExpr();
    if (parser.HasErrors)
    {
        return 65;
    }
    try 
    {
        Console.WriteLine(expr!.Eval(new Interpreter.Context()).ToOutput()); 
        return 0;
    }
    catch (Exception e)
    {
        Console.Error.WriteLine(e.Message);
        return 70;
    }
}

int Run()
{
    var parser = new Parser(fileContents);
    var program = parser.ParseProgram();
    if (parser.HasErrors)
    {
        return 65;
    }
    try 
    {
        Interpreter intrp = new Interpreter();
        intrp.Run(program);
        return 0;
    }
    catch (Exception e)
    {
        Console.Error.WriteLine(e.Message);
        Console.Error.WriteLine(e.StackTrace);
        return 70;
    }
}