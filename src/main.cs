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

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.Error.WriteLine("Logs from your program will appear here!");


switch (command)
{
    case "tokenize":
        return Tokenize();
    case "parse":
        return Parse();
    case "evaluate":
        return Evaluate();
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
    var expr = parser.ParseExpression();
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
    var expr = parser.ParseExpression();
    if (parser.HasErrors)
    {
        return 65;
    }
    try 
    {
        Console.WriteLine(expr!.Eval().ToOutput()); 
        return 0;
    }
    catch (Exception e)
    {
        Console.Error.WriteLine(e.Message);
        return 70;
    }
}